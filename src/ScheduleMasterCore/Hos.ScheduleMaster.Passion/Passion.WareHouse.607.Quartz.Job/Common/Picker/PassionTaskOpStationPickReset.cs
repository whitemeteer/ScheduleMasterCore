using Hos.ScheduleMaster.Base;
using Hos.ScheduleMaster.Passion.Base;
using Hos.ScheduleMaster.Passion.Request;
using Hos.ScheduleMaster.Passion.Response;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hos.ScheduleMaster.Passion
{
    public class PassionTaskOpStationPickReset: PassionTaskBase
    {
        #region Fields

        #endregion

        public override async void Run(TaskContext context)
        {
            base.Run(context);
        }
        public override bool Initalizing(TaskContext context)
        {
            bool flag = true;

            try
            {
                //方法逻辑初始化

                //Func_TaskRun_Begin
                this.Func_TaskRun_Begin = (object jobContext) =>
                {
                    //TODO 记录下日志
                    context.WriteLog($"Func_TaskRun_Begin:");

                    ResponseContent response = new ResponseContent();
                    return response.OK();

                };

                //Func_TaskRun_ToBe
                this.Func_TaskRun_ToBe = (object jobContext, RequestContent myRequest) =>
                {
                    ResponseContent response = new ResponseContent();

                    return response.OK("Func_TaskRun_ToBe Success");
                };

                //Func_TaskRun_ProcessLogic
                this.Func_TaskRun_ProcessLogic = async (object jobContext, RequestContent myRequest) =>
                {
                    ResponseContent response = new ResponseContent();

                    return await Task.FromResult(response.OK("Func_TaskRun_ProcessLogic Success"));
                };

                //Func_RunTask_OnExcuting
                this.Func_RunTask_OnExcuting = (object jobContext, RequestContent myRequest) =>
                {
                    ResponseContent response = new ResponseContent();

                    return response.OK("Func_RunTask_OnExcuting Success");
                };

                //Func_RunTask_OnExcuted
                this.Func_RunTask_OnExcuted = (object jobContext, RequestContent myRequest) =>
                {
                    ResponseContent response = new ResponseContent();

                    return response.OK("Func_RunTask_OnExcuted Success");
                };

                //Func_RunTask_End
                this.Func_RunTask_End = (TaskContext jobContext, RequestContent myRequest, ResponseContent myResponse) =>
                {
                    //TODO 记录下日志
                    context.WriteLog($"Func_RunTask_End:");

                    ResponseContent response = new ResponseContent();
                    return response.OK();

                };
            }
            catch (Exception ex)
            {
                flag = false;
                throw ex;
            }

            return flag;
        }

        //验证参数是否正确
        public override ResponseContent IsValid()
        {
            if (new Random().Next(0, 1) > 0)
            {
                return ResponseContent.Instance.OK("Success");
            }
            else
            {
                return ResponseContent.Instance.Error("光电信号异常");
            }

        }
    }
}
