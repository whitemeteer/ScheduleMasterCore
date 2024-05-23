using Hos.ScheduleMaster.Base;
using Hos.ScheduleMaster.Passion.Request;
using Hos.ScheduleMaster.Passion.Response;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hos.ScheduleMaster.Passion.Base
{
    public class PassionTaskBase : TaskBase
    {
        #region 增加任务流程ABP切片片段
        ///1. 委托Func<>
        ///2. 虚方法virtual

        /// <summary>
        /// 初始化Request,Response
        /// </summary>
        public virtual void Intial()
        {
            //数值Field初始化
            if (TaskRequest == null)
            {
                TaskRequest = new RequestContent();
            }

            if (TaskResponse == null)
            {
                TaskResponse = new ResponseContent();
            }
        }

        /// <summary>
        /// 设置Func_TaskRun_Begin,Func_TaskRun_ToBe,Func_TaskRun_ProcessLogic,Func_RunTask_OnExcuting,Func_RunTask_OnExcuted,Func_RunTask_End
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool Initalizing(TaskContext context)
        {
            bool settingFlag = true;
            try
            {
                //方法逻辑初始化
                this.Func_TaskRun_Begin = (object jobContext) => {
                    //TODO 记录下日志

                    ResponseContent response = new ResponseContent();
                    return response.OK();

                };
            }
            catch (Exception ex)
            {
                settingFlag = false;
                throw ex;
            }
            return settingFlag;
        }

        /// <summary>
        /// 任务开启前
        /// 记录调佣日志,记录参数
        /// </summary>
        protected Func<object, object> Func_TaskRun_Begin;

        /// <summary>
        /// 检查参数数据，运行条件，前后文
        /// </summary>
        /// <returns></returns>
        public virtual ResponseContent IsValid()
        {
            ResponseContent validResponseContent = new ResponseContent();

            //do something here to check the request

            validResponseContent.OK();
            return validResponseContent;
        }

        /// <summary>
        /// 任务构造传入参数
        /// </summary>
        protected Func<object, RequestContent, ResponseContent> Func_TaskRun_ToBe;

        /// <summary>
        /// 任务逻辑操作
        /// </summary>
        protected Func<object, RequestContent, Task<ResponseContent>> Func_TaskRun_ProcessLogic;

        /// <summary>
        /// 任务运行中
        /// 1.边车任务
        /// </summary>
        protected Func<object, RequestContent, ResponseContent> Func_RunTask_OnExcuting;

        /// <summary>
        /// 任务运行完
        /// 1.Func_TaskRun_ProcessLogic的流程正常结束
        /// </summary>
        protected Func<object, RequestContent, ResponseContent> Func_RunTask_OnExcuted;

        /// <summary>
        /// 任务结束后
        /// 1.正常运行结束
        /// 2.非正常结束
        /// 记录处理
        /// </summary>
        protected Func<TaskContext, RequestContent, ResponseContent, object> Func_RunTask_End;

        #endregion



        public RequestContent TaskRequest { get; set; }
        public ResponseContent TaskResponse { get; set; }



        public virtual RequestContent SetRequest(TaskContext context)
        {
            Type currentClassType = this.GetType();
            List<PropertyInfo> propertiesList = currentClassType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

            propertiesList.ForEach(f => {

                #region 通过Property属性的类型，反射从context上下文中通过方法GetArgument<T>获取具体value

                // 1.属性的类型
                Type currentPublicFieldProperType = f.PropertyType;

                // 2.获取泛型方法
                Type contextTaskContextType = typeof(TaskContext);
                MethodInfo directionMethod = contextTaskContextType.GetMethods()
                    .First(m => m.Name == "GetArgument" && m.IsGenericMethodDefinition);
                // 3. 得到泛型方法实例
                MethodInfo genericGetArgumentMethod = directionMethod.MakeGenericMethod(currentPublicFieldProperType);

                // 4. 获取context的类型的实例
                //x 已经明确知晓实例类型，不需要重新获取instance
                //object contextInstance = Activator.CreateInstance(context.GetType());

                // 5: 调用方法并处理返回的泛型对象
                object resultObject = genericGetArgumentMethod.Invoke(context, new object[] { f.Name });
                object convertedObject = Convert.ChangeType(resultObject, currentPublicFieldProperType);

                // 6. 给属性设值
                f.SetValue(f.Name, convertedObject);
                // 7. string对象化
                TaskRequest.Parameters.Add(f.Name, convertedObject.ToString());

                #endregion

            });

            return TaskRequest;
        }

        /// <summary>
        /// 设置Response
        /// TODO
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual ResponseContent SetResponse(TaskContext context)
        {
            return TaskResponse.OK("Success");
        }

        public override async void Run(TaskContext context)
        {
            //停滞5毫秒
            await Task.Delay(5);

            //实例Requst, Response
            this.Intial();

            //设置Func_TaskRun_Begin,Func_TaskRun_ToBe,Func_TaskRun_ProcessLogic,Func_RunTask_OnExcuting,Func_RunTask_OnExcuted,Func_RunTask_End等等
            this.Initalizing(context);

            //构造传入的Request
            this.SetRequest(context);

            // Task任务开启前逻辑片段...
            if (this.Func_TaskRun_Begin != null)
            {
                this.Func_TaskRun_Begin(context);
            }

            if (this.IsValid().Status)
            {
                // Task任务实际运行逻辑片段...
                if (this.Func_TaskRun_ToBe != null)
                {
                    TaskResponse = (ResponseContent)this.Func_TaskRun_ToBe(context, TaskRequest);
                }

                // Task任务实际运行逻辑片段...
                if (this.Func_TaskRun_ProcessLogic != null)
                {
                    TaskResponse = (ResponseContent) await this.Func_TaskRun_ProcessLogic(context, TaskRequest);
                }

                // Task任务运行中逻辑片段...
                if (this.Func_RunTask_OnExcuting != null)
                {
                    TaskResponse = (ResponseContent)this.Func_RunTask_OnExcuting(context, TaskRequest);
                }

                // Task任务运行完逻辑片段...
                if (this.Func_RunTask_OnExcuted != null)
                {
                    TaskResponse = (ResponseContent)this.Func_RunTask_OnExcuted(context, TaskRequest);
                }

                // Task任务设置返回Content
                this.SetResponse(context);
            }
            else
            {
                TaskResponse.Error("传入参数验证不通过!");
                // Task任务设置返回Content
                this.SetResponse(context);
            }


            // Task任务结束前逻辑片段...
            if (this.Func_RunTask_End != null)
            {
                this.Func_RunTask_End(context, TaskRequest, TaskResponse);
            }

            //停滞5毫秒
            await Task.Delay(5);
        }
    }
}
