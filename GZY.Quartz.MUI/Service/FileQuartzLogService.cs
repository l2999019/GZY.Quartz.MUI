using GZY.Quartz.MUI.Model;
using GZY.Quartz.MUI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZY.Quartz.MUI.Service
{
    public class FileQuartzLogService : IQuartzLogService
    {

        private QuartzFileHelper _quartzFileHelper;
        public FileQuartzLogService(QuartzFileHelper quartzFileHelper)
        {
            _quartzFileHelper = quartzFileHelper;
        }
        public Task<ResultQuartzData> AddLog(tab_quarz_tasklog tab_Quarz_Tasklog)
        {
            return Task.Run(() =>
            {
                try
                {
                    _quartzFileHelper.WriteJobLogs(tab_Quarz_Tasklog);
                    return new ResultQuartzData { message = "日志数据保存成功!", status = true };
                }
                catch (Exception)
                {

                    return new ResultQuartzData { message = "日志数据保存失败!", status = false };
                }
            });
        }

        public Task<tab_quarz_tasklog> Getlastlog(string taskName, string groupName)
        {
            return Task.Run(() =>
            {

                var list = _quartzFileHelper.GetJobsLog();
                var date = list.Where(a => a.TaskName == taskName && a.GroupName == groupName).OrderByDescending(a=>a.BeginDate).FirstOrDefault();
                return date;

            });
        }

        public Task<ResultData<tab_quarz_tasklog>> GetLogs(string taskName, string groupName, int page, int pageSize = 100)
        {
            return Task.Run(() =>
            {

                var list = _quartzFileHelper.GetJobsLog();
                int total = list.Where(a => a.TaskName == taskName
            && a.GroupName == groupName).Count();
                var date = list.Where(a => a.TaskName == taskName && a.GroupName == groupName).Skip((page - 1) * pageSize).Take(pageSize).ToList();
                ResultData<tab_quarz_tasklog> resultData = new ResultData<tab_quarz_tasklog>() { total = total, data = date };
                return resultData;

            });
        }
    }
}
