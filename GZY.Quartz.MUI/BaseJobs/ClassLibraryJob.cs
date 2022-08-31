using GZY.Quartz.MUI.BaseService;
using GZY.Quartz.MUI.Model;
using GZY.Quartz.MUI.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZY.Quartz.MUI.BaseJobs
{
    public class ClassLibraryJob : IJob
    {
        private IQuartzService _quartzService;
        private IQuartzLogService _quartzLogService;
        private IServiceProvider _serviceProvider ;
        private ILogger<ClassLibraryJob> _logger { get; set; }

        /// <summary>
        /// 2020.05.31增加构造方法
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="httpClientFactory"></param>
        public ClassLibraryJob(IServiceProvider serviceProvider, IQuartzService quartzService, IQuartzLogService quartzLogService, ILogger<ClassLibraryJob> logger)
        {
            this._quartzLogService = quartzLogService;
            this._quartzService = quartzService;
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            //serviceProvider.GetService()
        }
        public async Task Execute(IJobExecutionContext context)
        {
            DateTime dateTime = DateTime.Now;
            string httpMessage = "";
            AbstractTrigger trigger = (context as JobExecutionContextImpl).Trigger as AbstractTrigger;

            tab_quarz_task taskOptions =(await _quartzService.GetJobs(a => a.TaskName == trigger.Name && a.GroupName == trigger.Group)).FirstOrDefault();
            if (taskOptions == null)
            {

                taskOptions =(await _quartzService.GetJobs(a => a.TaskName == trigger.JobName && a.GroupName == trigger.JobGroup)).FirstOrDefault();

            }
            if (taskOptions == null)
            {
                _logger.LogError($"组别:{trigger.Group},名称:{trigger.Name},的作业未找到,可能已被移除");
                // FileHelper.WriteFile(FileQuartz.LogPath + trigger.Group, $"{trigger.Name}.txt", "未到找作业或可能被移除", true);
                return;
            }
            _logger.LogInformation($"组别:{trigger.Group},名称:{trigger.Name},的作业开始执行,时间:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss")}");
            Console.WriteLine($"作业[{taskOptions.TaskName}]开始:{ DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss")}");
            tab_quarz_tasklog tab_Quarz_Tasklog = new tab_quarz_tasklog() { TaskName = taskOptions.TaskName, GroupName = taskOptions.GroupName, BeginDate = DateTime.Now };
            if (string.IsNullOrEmpty(taskOptions.DllClassName))
            {
                _logger.LogError($"组别:{trigger.Group},名称:{trigger.Name},类名不能为空!,时间:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss")}");
                //FileHelper.WriteFile(FileQuartz.LogPath + trigger.Group, $"{trigger.Name}.txt", $"{ DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss")}未配置url,", true);
                return;
            }

            try
            {
               var services =  _serviceProvider.GetServices<IJobService>();
                var service = services.Where(a => a.GetType().Name == taskOptions.DllClassName).FirstOrDefault();
                if (service != null)
                {
                    httpMessage = service.ExecuteService(taskOptions.ApiParameter);
                }
                else
                {
                    httpMessage = "未找到对应类型,请检查是否注入!";
                }

               
            }
            catch (Exception ex)
            {
                httpMessage = ex.Message;
            }

            try
            {
                //string logContent = $"{(string.IsNullOrEmpty(httpMessage) ? "OK" : httpMessage)}\r\n";
                tab_Quarz_Tasklog.EndDate = DateTime.Now;
                tab_Quarz_Tasklog.Msg = httpMessage;
                await _quartzLogService.AddLog(tab_Quarz_Tasklog);
            }
            catch (Exception)
            {
            }
            Console.WriteLine(trigger.FullName + " " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss") + " " + httpMessage);
            return;
        }
    }
}
