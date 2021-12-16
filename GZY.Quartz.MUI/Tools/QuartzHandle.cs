using GZY.Quartz.MUI.BaseJobs;
using GZY.Quartz.MUI.Enum;
using GZY.Quartz.MUI.Model;
using GZY.Quartz.MUI.Service;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZY.Quartz.MUI.Tools
{
    public class QuartzHandle : IQuartzHandle
    {

        private IQuartzService _quartzService;
        private ISchedulerFactory _schedulerFactory;
        private IQuartzLogService _quartzLogService;
        private Microsoft.Extensions.Logging.ILogger<QuartzHandle> _logger;
        private IJobFactory _jobFactory;

        public QuartzHandle(IQuartzService quartzService, ISchedulerFactory schedulerFactory, IQuartzLogService quartzLogService, IJobFactory jobFactory, ILogger<QuartzHandle> logger)
        {
            _quartzService = quartzService;
            _schedulerFactory = schedulerFactory;
            _quartzLogService = quartzLogService;
            _logger = logger;
            _jobFactory = jobFactory;
        }
        /// <summary>
        /// 获取所有的作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <returns></returns>
        public async Task<List<tab_quarz_task>> GetJobs()
        {
            List<tab_quarz_task> list = new List<tab_quarz_task>();
            try
            {
                IScheduler _scheduler = await _schedulerFactory.GetScheduler();
                var groups = await _scheduler.GetJobGroupNames();
                list = _quartzService.GetJobs(a=>1==1).Result;
                foreach (var groupName in groups)
                {
                    foreach (var jobKey in await _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)))
                    {
                        tab_quarz_task taskOption = list.Where(x => x.GroupName == jobKey.Group && x.TaskName == jobKey.Name)
                            .FirstOrDefault();
                        if (taskOption == null)
                            continue;

                        var triggers = await _scheduler.GetTriggersOfJob(jobKey);
                        foreach (ITrigger trigger in triggers)
                        {
                            DateTimeOffset? dateTimeOffset = trigger.GetPreviousFireTimeUtc();
                            if (dateTimeOffset != null)
                            {
                                taskOption.LastRunTime = Convert.ToDateTime(dateTimeOffset.ToString());
                            }
                            else
                            {
                                var runlog = await _quartzLogService.Getlastlog(taskOption.TaskName, taskOption.GroupName);
                                if (runlog != null)
                                {
                                    taskOption.LastRunTime = runlog.BeginDate;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.LogWarning("获取作业异常：" + ex.Message + ex.StackTrace);
                }
            }
            return list;
        }

        public ResultQuartzData IsValidExpression(string cronExpression)
        {
            try
            {
                CronTriggerImpl trigger = new CronTriggerImpl();
                trigger.CronExpressionString = cronExpression;
                DateTimeOffset? date = trigger.ComputeFirstFireTimeUtc(null);
                var iscron = date != null;
                return new ResultQuartzData { status = iscron, message = date == null ? $"请确认表达式{cronExpression}是否正确!" : "" };
            }
            catch (Exception e)
            {
                return new ResultQuartzData { status = false, message = $"请确认表达式{cronExpression}是否正确!" };
            }
        }
        public async void InitJobs()
        {

            var jobs = await _quartzService.GetJobs(a => 1 == 1);
            IScheduler scheduler = await _schedulerFactory.GetScheduler();
            foreach (var item in jobs)
            {
                try
                {
                    IJobDetail job = null;
                    if (item.TaskType == 1)
                    {
                        job = JobBuilder.Create<ClassLibraryJob>()
                        .WithIdentity(item.TaskName, item.GroupName)
                        .Build();

                    }
                    else
                    {
                        job = JobBuilder.Create<HttpResultfulJob>()
                        .WithIdentity(item.TaskName, item.GroupName)
                        .Build();
                    }
                    ITrigger trigger = TriggerBuilder.Create()
                       .WithIdentity(item.TaskName, item.GroupName)
                       .WithDescription(item.Describe)
                       .WithCronSchedule(item.Interval)
                       .Build();

                  

                    if (_jobFactory != null)
                    {
                        scheduler.JobFactory = _jobFactory;
                    }
                    
                    
                    if (item.Status == (int)JobState.开启)
                    {
                        await scheduler.ScheduleJob(job, trigger);
                        await _quartzLogService.AddLog(new tab_quarz_tasklog() { TaskName = item.TaskName, GroupName = item.GroupName,BeginDate=DateTime.Now, Msg = $"任务初始化启动成功:{item.Status}" });
                    }
                    else
                    {
                        await scheduler.ScheduleJob(job, trigger);
                        await Pause(item);
                        _logger.LogError($"任务初始化,未启动,状态为:{item.Status}");
                        //await _quartzLogService.AddLog(new tab_quarz_tasklog() { TaskName = item.TaskName, GroupName = item.GroupName, Msg = $"任务初始化,未启动,状态为:{item.Status}" });
                        //FileQuartz.WriteStartLog($"作业:{taskOptions.TaskName},分组:{taskOptions.GroupName},新建时未启动原因,状态为:{taskOptions.Status}");
                    }
                }
                catch (Exception ex)
                {
                    await _quartzLogService.AddLog(new tab_quarz_tasklog() { TaskName = item.TaskName, GroupName = item.GroupName, Msg = $"任务初始化未启动,出现异常,异常信息{ex.Message}" });
                    continue;
                }
                await scheduler.Start();
            }




        }

        /// <summary>
        /// 添加作业
        /// </summary>
        /// <param name="taskOptions"></param>
        /// <param name="schedulerFactory"></param>
        /// <returns></returns>
        public async Task<ResultQuartzData> AddJob(tab_quarz_task taskOptions)
        {
            ResultQuartzData isaddsql = null;
            try
            {
                var validExpression = IsValidExpression(taskOptions.Interval);
                if (!validExpression.status)
                    return validExpression;

                var model = _quartzService.GetJobs(a => a.TaskName == taskOptions.TaskName && a.GroupName == taskOptions.GroupName).Result.FirstOrDefault();
                if (model != null)
                {
                    return new ResultQuartzData { status = false, message = "任务已存在,添加失败!" };
                }
                isaddsql = await _quartzService.AddJob(taskOptions);
                IJobDetail job = null;
                if (taskOptions.TaskType == 1)
                {
                    job = JobBuilder.Create<ClassLibraryJob>()
                    .WithIdentity(taskOptions.TaskName, taskOptions.GroupName)
                    .Build();

                }
                else
                {
                    job = JobBuilder.Create<HttpResultfulJob>()
                    .WithIdentity(taskOptions.TaskName, taskOptions.GroupName)
                    .Build();
                }
                //  IJobDetail job = JobBuilder.Create<HttpResultfulJob>()
                // .WithIdentity(taskOptions.TaskName, taskOptions.GroupName)
                //.Build();
                ITrigger trigger = TriggerBuilder.Create()
                   .WithIdentity(taskOptions.TaskName, taskOptions.GroupName)
                   .WithDescription(taskOptions.Describe)
                   .WithCronSchedule(taskOptions.Interval)
                   .Build();

                IScheduler scheduler = await _schedulerFactory.GetScheduler();

                //if (jobFactory == null)
                //{
                //    try
                //    {
                //        jobFactory = HttpContext.Current.RequestServices.GetService<IJobFactory>();
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine($"创建任务[{taskOptions.TaskName}]异常,{ex.Message}");
                //    }
                //}

                if (_jobFactory != null)
                {
                    scheduler.JobFactory = _jobFactory;
                }

                //开启才加入Schedule中,如果加入在暂停而定时任务执行过快,会导致卡死
                if (taskOptions.Status == (int)JobState.开启)
                {
                    await scheduler.ScheduleJob(job, trigger);
                    await scheduler.Start();
                }
                else
                {
                    await Pause(taskOptions);
                    await _quartzLogService.AddLog(new tab_quarz_tasklog() { TaskName = taskOptions.TaskName, GroupName = taskOptions.GroupName, Msg = $"任务新建,未启动,状态为:{taskOptions.Status}" });
                    //FileQuartz.WriteStartLog($"作业:{taskOptions.TaskName},分组:{taskOptions.GroupName},新建时未启动原因,状态为:{taskOptions.Status}");
                }
                //if (!init)
                //{
                //    await _quartzService.AddJob(taskOptions);
                //}
                // FileQuartz.WriteJobAction(JobAction.新增, taskOptions.TaskName, taskOptions.GroupName);

            }
            catch (Exception ex)
            {
                return new ResultQuartzData { status = false, message = ex.Message };
            }
            if (isaddsql.status)
            {
                isaddsql.message = "任务添加成功!";
            }
            return isaddsql;// new ResultQuartzData { status = , message = "任务添加成功!" };

        }

        /// <summary>
        /// 移除作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="taskName"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task<ResultQuartzData> Remove(tab_quarz_task taskOptions)
        {
            var isjob = await IsQuartzJob(taskOptions.TaskName, taskOptions.GroupName);
            var taskmodle = (await _quartzService.GetJobs(a => a.TaskName == taskOptions.TaskName && a.GroupName == taskOptions.GroupName)).FirstOrDefault();
            string message = "";
            if (isjob.status)
            {
                try
                {
                    IScheduler scheduler = await _schedulerFactory.GetScheduler();
                    List<JobKey> jobKeys = scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(taskOptions.GroupName)).Result.ToList();
                    JobKey jobKey = jobKeys.Where(s => scheduler.GetTriggersOfJob(s).Result.Any(x => (x as CronTriggerImpl).Name == taskOptions.TaskName)).FirstOrDefault();
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);
                    ITrigger trigger = triggers?.Where(x => (x as CronTriggerImpl).Name == taskOptions.TaskName).FirstOrDefault();
                    await scheduler.PauseTrigger(trigger.Key);
                    await scheduler.UnscheduleJob(trigger.Key);// 移除触发器
                    await scheduler.DeleteJob(trigger.JobKey);
                }
                catch (Exception ex)
                {

                    message += ex.Message;
                }
            }
            if (taskmodle != null)
            {
                isjob = await _quartzService.Remove(taskmodle);
            }
            message += isjob.message;
            return new ResultQuartzData { status = isjob.status, message = message };
        }

        /// <summary>
        /// 更新作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="taskOptions"></param>
        /// <returns></returns>
        public async Task<ResultQuartzData> Update(tab_quarz_task taskOptions)
        {
            var isjob = await IsQuartzJob(taskOptions.TaskName, taskOptions.GroupName);
            var taskmodle = (await _quartzService.GetJobs(a =>a.id == taskOptions.id)).FirstOrDefault();
            var message = "";
            if (isjob.status) //如果Quartz存在就更新
            {
                try
                {


                    IScheduler scheduler = await _schedulerFactory.GetScheduler();
                    List<JobKey> jobKeys = scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(taskOptions.GroupName)).Result.ToList();
                    JobKey jobKey = jobKeys.Where(s => scheduler.GetTriggersOfJob(s).Result.Any(x => (x as CronTriggerImpl).Name == taskOptions.TaskName)).FirstOrDefault();
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);
                    ITrigger triggerold = triggers?.Where(x => (x as CronTriggerImpl).Name == taskOptions.TaskName).FirstOrDefault();
                    await scheduler.PauseTrigger(triggerold.Key);
                    await scheduler.UnscheduleJob(triggerold.Key);// 移除触发器
                    await scheduler.DeleteJob(triggerold.JobKey);
                    IJobDetail job = null;
                    if (taskOptions.TaskType == 1)
                    {
                        job = JobBuilder.Create<ClassLibraryJob>()
                        .WithIdentity(taskOptions.TaskName, taskOptions.GroupName)
                        .Build();

                    }
                    else
                    {
                        job = JobBuilder.Create<HttpResultfulJob>()
                        .WithIdentity(taskOptions.TaskName, taskOptions.GroupName)
                        .Build();
                    }
                    //  IJobDetail job = JobBuilder.Create<HttpResultfulJob>()
                    // .WithIdentity(taskOptions.TaskName, taskOptions.GroupName)
                    //.Build();
                    ITrigger triggernew = TriggerBuilder.Create()
                       .WithIdentity(taskOptions.TaskName, taskOptions.GroupName)
                       .StartNow()
                       .WithDescription(taskOptions.Describe)
                       .WithCronSchedule(taskOptions.Interval)
                       .Build();

                    if (_jobFactory != null)
                    {
                        scheduler.JobFactory = _jobFactory;
                    }
                    await scheduler.ScheduleJob(job, triggernew);
                    if (taskOptions.Status == (int)JobState.开启)
                    {
                        await scheduler.Start();
                    }
                    else
                    {
                        await scheduler.PauseTrigger(triggernew.Key);
                        await _quartzLogService.AddLog(new tab_quarz_tasklog() { TaskName = taskOptions.TaskName, GroupName = taskOptions.GroupName, Msg = $"任务新建,未启动,状态为:{taskOptions.Status}" });
                    }
                    message += "quarz已更新,";
                }
                catch (Exception ex)
                {

                    message += ex.Message;
                }
            }
            if (taskmodle != null)
            {
                isjob = await _quartzService.Update(taskOptions);
                message += isjob.message;
            }
            return new ResultQuartzData { status = isjob.status, message = message };



        }

        /// <summary>
        /// 暂停作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="taskOptions"></param>
        /// <returns></returns>
        public async Task<ResultQuartzData> Pause(tab_quarz_task taskOptions)
        {
            try
            {
                var isjob = await IsQuartzJob(taskOptions.TaskName, taskOptions.GroupName);
                var taskmodle = (await _quartzService.GetJobs(a => a.TaskName == taskOptions.TaskName && a.GroupName == taskOptions.GroupName)).FirstOrDefault();
              
                if (isjob.status)
                {
                    
                    IScheduler scheduler = await _schedulerFactory.GetScheduler();
                    List<JobKey> jobKeys = scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(taskOptions.GroupName)).Result.ToList();
                    JobKey jobKey = jobKeys.Where(s => scheduler.GetTriggersOfJob(s).Result.Any(x => (x as CronTriggerImpl).Name == taskOptions.TaskName)).FirstOrDefault();
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);
                    ITrigger trigger = triggers?.Where(x => (x as CronTriggerImpl).Name == taskOptions.TaskName).FirstOrDefault();
                    await scheduler.PauseTrigger(trigger.Key);
                    isjob.message += "Quartz已暂停";
                }
                if (taskmodle != null)
                {
                    taskmodle.Status = (int)JobState.暂停;
                    var date = await _quartzService.Update(taskmodle);
                    isjob.status = date.status;
                    isjob.message += date.message;
                }
               
                return isjob;
            }
            catch (Exception ex)
            {
                return new ResultQuartzData { status = false, message = ex.Message };
            }

        }

        /// <summary>
        /// 启动作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="taskOptions"></param>
        /// <returns></returns>
        public async Task<ResultQuartzData> Start(tab_quarz_task taskOptions)
        {
            try
            {
                var isjob = await IsQuartzJob(taskOptions.TaskName, taskOptions.GroupName);
                var taskmodle = (await _quartzService.GetJobs(a => a.TaskName == taskOptions.TaskName && a.GroupName == taskOptions.GroupName)).FirstOrDefault();
                taskmodle.Status = (int)JobState.开启;
                IScheduler scheduler = await _schedulerFactory.GetScheduler();
                if (!isjob.status) //如果不存在则加入
                {
                    IJobDetail job = null;
                    if (taskOptions.TaskType == 1)
                    {
                        job = JobBuilder.Create<ClassLibraryJob>()
                        .WithIdentity(taskOptions.TaskName, taskOptions.GroupName)
                        .Build();

                    }
                    else
                    {
                        job = JobBuilder.Create<HttpResultfulJob>()
                        .WithIdentity(taskOptions.TaskName, taskOptions.GroupName)
                        .Build();
                    }
                    ITrigger trigger = TriggerBuilder.Create()
                       .WithIdentity(taskOptions.TaskName, taskOptions.GroupName)
                       .WithDescription(taskOptions.Describe)
                       .WithCronSchedule(taskOptions.Interval)
                       .Build();
                    if (_jobFactory != null)
                    {
                        scheduler.JobFactory = _jobFactory;
                    }
                    await scheduler.ScheduleJob(job, trigger);
                    await scheduler.Start();
                }
                else //存在则直接启动
                {
                    List<JobKey> jobKeys = scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(taskOptions.GroupName)).Result.ToList();
                    JobKey jobKey = jobKeys.Where(s => scheduler.GetTriggersOfJob(s).Result.Any(x => (x as CronTriggerImpl).Name == taskOptions.TaskName)).FirstOrDefault();
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);
                    ITrigger trigger = triggers?.Where(x => (x as CronTriggerImpl).Name == taskOptions.TaskName).FirstOrDefault();
                    await scheduler.ResumeTrigger(trigger.Key);
                }
                var date = await _quartzService.Update(taskmodle);
                return date;
            }
            catch (Exception ex)
            {
                return new ResultQuartzData { status = false, message = ex.Message };
            }
        }

        /// <summary>
        /// 立即执行一次作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="taskOptions"></param>
        /// <returns></returns>
        public async Task<ResultQuartzData> Run(tab_quarz_task taskOptions)
        {
            try
            {
                var isjob = await IsQuartzJob(taskOptions.TaskName, taskOptions.GroupName);
                var taskmodle = (await _quartzService.GetJobs(a => a.TaskName == taskOptions.TaskName && a.GroupName == taskOptions.GroupName)).FirstOrDefault();
                if (isjob.status)
                {
                    //taskmodle.Status = (int)JobState.立即执行;
                    IScheduler scheduler = await _schedulerFactory.GetScheduler();
                    List<JobKey> jobKeys = scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(taskOptions.GroupName)).Result.ToList();
                    JobKey jobKey = jobKeys.Where(s => scheduler.GetTriggersOfJob(s).Result.Any(x => (x as CronTriggerImpl).Name == taskOptions.TaskName)).FirstOrDefault();
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);
                    ITrigger trigger = triggers?.Where(x => (x as CronTriggerImpl).Name == taskOptions.TaskName).FirstOrDefault();
                    await scheduler.TriggerJob(jobKey);
                    return new ResultQuartzData { status = true, message = $"{taskOptions.TaskName}立即执行任务成功" };
                }
                else
                {
                    return isjob;
                }
                
            }
            catch (Exception ex)
            {
                return new ResultQuartzData { status = false, message = ex.Message };
            }

        }

        /// <summary>
        /// 判断是否存在此任务
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="taskName"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task<ResultQuartzData> IsQuartzJob(string taskName, string groupName)
        {
            try
            {
                string errorMsg = "";
                IScheduler scheduler = await _schedulerFactory.GetScheduler();
                List<JobKey> jobKeys = scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)).Result.ToList();
                if (jobKeys == null || jobKeys.Count() == 0)
                {
                    errorMsg = $"未找到分组[{groupName}]";
                    return new ResultQuartzData { status = false, message = errorMsg };
                }
                JobKey jobKey = jobKeys.Where(s => scheduler.GetTriggersOfJob(s).Result.Any(x => (x as CronTriggerImpl).Name == taskName)).FirstOrDefault();
                if (jobKey == null)
                {
                    errorMsg = $"未找到任务{taskName}]";
                    return new ResultQuartzData { status = false, message = errorMsg };
                }
                var triggers = await scheduler.GetTriggersOfJob(jobKey);
                ITrigger trigger = triggers?.Where(x => (x as CronTriggerImpl).Name == taskName).FirstOrDefault();

                if (trigger == null)
                {
                    errorMsg = $"未找到触发器[{taskName}]";
                    return new ResultQuartzData { status = false, message = errorMsg };
                }

                return new ResultQuartzData { status = true, message = errorMsg };
            }
            catch (Exception ex)
            {
                return new ResultQuartzData { status = false, message = ex.Message };
            }
        }




    }
}
