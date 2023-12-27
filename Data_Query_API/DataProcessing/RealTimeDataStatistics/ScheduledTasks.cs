using Quartz;
using Quartz.Impl;

namespace Data_Query_API.DataProcessing.RealTimeDataStatistics
{
    /// <summary>
    /// 定时器任务建立
    /// </summary>
    public class ScheduledTasks
    {
        /// <summary>
        /// 实时数据定时器
        /// </summary>
        public static async void RealTimeDataReport()
        {
            //调度器工厂
            ISchedulerFactory DataDaily = new StdSchedulerFactory();
            //调度器
            IScheduler schedulers_work = await DataDaily.GetScheduler();
            await schedulers_work.GetJobGroupNames();
            /*-------------计划任务代码实现------------------*/
            //创建任务
            IJobDetail job_Work = JobBuilder.Create<RealTimeDataGeneration>()
                .WithIdentity("TimeTriggerddd", "TimeGroupdd")
                .Build();
            //创建触发器 指定时间被执行
            //ITrigger trigger9 = TriggerBuilder.Create().WithCronSchedule("30 03 11 * * ?").WithIdentity("TimeTriggerddd", "TimeGroupdd").Build();
            //每隔多久执行一次  这个是每隔多久执行一遍
            ITrigger trigger9 = TriggerBuilder.Create().WithIdentity("TimeTriggerddd", "TimeGroupdd").WithSimpleSchedule(t => t.WithIntervalInSeconds(360).RepeatForever()).Build();
            //添加任务及触发器至调度器中
            await schedulers_work.ScheduleJob(job_Work, trigger9);
            /*-------------计划任务代码实现------------------*/
            //启动
            await schedulers_work.Start();
        }
    }
}
