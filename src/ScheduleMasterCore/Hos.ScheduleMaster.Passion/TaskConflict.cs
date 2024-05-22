using Hos.ScheduleMaster.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hos.ScheduleMaster.Passion
{
    public class TaskConflict : TaskBase
    {
        public override void Run(TaskContext context)
        {
            //休息10秒钟模拟耗时的业务操作，任务设置为5秒运行一次
            System.Threading.Thread.Sleep(new Random().Next(6000, 10000));
            context.WriteLog("TaskConflict Demo：我运行完了~");
        }
    }
}
