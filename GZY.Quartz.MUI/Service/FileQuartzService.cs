using GZY.Quartz.MUI.Enum;
using GZY.Quartz.MUI.Model;
using GZY.Quartz.MUI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GZY.Quartz.MUI.Service
{
    public class FileQuartzService : IQuartzService
    {
        private QuartzFileHelper _quartzFileHelper;
        public FileQuartzService(QuartzFileHelper quartzFileHelper)
        {
            _quartzFileHelper = quartzFileHelper;
        }
        public Task<ResultQuartzData> AddJob(tab_quarz_task model)
        {
            return Task.Run(() =>
             {
                var list=  _quartzFileHelper.GetJobs(a=>1==1);
                 if (list == null)
                 {
                     list=  new List<tab_quarz_task>();

                 }
                 if (list.Count == 0)
                 {
                     model.id =1;
                 }
                 else
                 {
                     model.id = list.Max(a => a.id) + 1;
                 }
                 list.Add(model);
                 _quartzFileHelper.WriteJobConfig(list);
                 return new ResultQuartzData { message = "数据保存成功!", status = true };

             });


        }

        public Task<ResultDashboardData> GetDashboardInfo()
        {
            return Task.Run(() =>
            {
                var result = new ResultDashboardData();
                var jobs = _quartzFileHelper.GetJobs(a => 1 == 1);
                var joblogs = _quartzFileHelper.GetJobsLog();
                //查询当天任务执行次数
                result.JobCounts = jobs.Count;
                //查询运行中的任务数量/任务总数
                result.RunJobs = string.Join("/", jobs.Where(a => a.Status == 6).Count(), jobs.Count());
                //查询当天任务执行平均耗时
                result.AverageTime = joblogs.Where(a => a.BeginDate >= DateTime.Today).Select(a=>a.DurationMs).DefaultIfEmpty(0).Average();
                //查询当天任务错误率
                int errorcount = joblogs.Where(a => a.BeginDate >= DateTime.Today && a.JobStatus == 1).Count();
                int logscount = joblogs.Where(a => a.BeginDate >= DateTime.Today).Count();
                result.ErrorCounts = logscount == 0 ? 0 : ((double)errorcount / logscount) * 100;
                return result;
            });
        }

        public Task<List<dynamic>> GetFailureRate()
        {
            return Task.Run(() =>
            {
                var query = _quartzFileHelper.GetJobsLog()
                          .Where(x => x.BeginDate.HasValue)
                          .GroupBy(x => x.TaskName)
                          .Select(g => new {
                              TaskName = g.Key,
                              LastExec = g.Max(x => x.BeginDate), // 最近一次执行时间
                              Total = g.Count(),
                              Fail = g.Count(x => x.JobStatus != 0)
                          })
                          .OrderByDescending(x => x.LastExec) // 按最近执行排序
                          .Take(10)                           // 只取前 10 个
                          .Select(g => new {
                              g.TaskName,
                              FailRate = g.Total == 0 ? 0 : Math.Round((double)g.Fail / g.Total * 100, 2)
                          })
                          .ToList<dynamic>();
                return query;
            });
            
        }

        public Task<List<tab_quarz_task>> GetJobs(Expression<Func<tab_quarz_task, bool>> where)
        {
            return Task.Run(() =>
            {

                var list = _quartzFileHelper.GetJobs(where);
                return list;

            });
        }

        public Task<List<dynamic>> GetTrendInfo()
        {

            return Task.Run(() =>
            {
                var query = _quartzFileHelper.GetJobsLog()
                            .Where(x => x.BeginDate.HasValue && x.BeginDate.Value.Date ==DateTime.Today)
                            .GroupBy(x => x.BeginDate.Value.Hour)
                            .Select(g => new {
                                Hour = g.Key, // 小时 (0-23)
                                SuccessCount = g.Count(x => x.JobStatus == 0),
                                FailCount = g.Count(x => x.JobStatus != 0),
                                AvgDuration = g.Average(x => x.DurationMs)
                            })
                            .OrderBy(x => x.Hour)
                            .ToList<dynamic>();
                return query;
            });
        }

        public Task<ResultQuartzData> Pause(tab_quarz_task model)
        {
            //throw new NotImplementedException();
            return Task.Run(() =>
            {
                var list = _quartzFileHelper.GetJobs(a => 1 == 1);
                list.ForEach(f=> { 
                 if(f.TaskName == model.TaskName && f.GroupName == model.GroupName)
                    {
                        f.Status = Convert.ToInt32(JobState.暂停);
                    }
                }) ;
                _quartzFileHelper.WriteJobConfig(list);
                return new ResultQuartzData { message = "数据暂停成功!", status = true }; ;

            });
        }

        public Task<ResultQuartzData> Remove(tab_quarz_task model)
        {
            return Task.Run(() =>
            {
                var list = _quartzFileHelper.GetJobs(a => 1 == 1);
                list.Remove(list.Find(a=>a.TaskName==model.TaskName&&a.GroupName==model.GroupName));
                _quartzFileHelper.WriteJobConfig(list);
                return new ResultQuartzData { message = "数据暂停成功!", status = true }; ;

            });
        }


        public Task<ResultQuartzData> Start(tab_quarz_task model)
        {
            return Task.Run(() =>
            {
                var list = _quartzFileHelper.GetJobs(a => 1 == 1);
                list.ForEach(f => {
                    if (f.TaskName == model.TaskName && f.GroupName == model.GroupName)
                    {
                        f.Status = Convert.ToInt32(JobState.开启);
                    }
                });
                _quartzFileHelper.WriteJobConfig(list);
                return new ResultQuartzData { message = "数据开启成功!", status = true }; ;

            });
        }

        public Task<ResultQuartzData> Update(tab_quarz_task model)
        {
            return Task.Run(() =>
            {
                var list = _quartzFileHelper.GetJobs(a => 1 == 1);
                list.Remove(list.Find(a =>a.id==model.id));
                list.Add(model);
                _quartzFileHelper.WriteJobConfig(list);
                return new ResultQuartzData { message = "数据修改成功!", status = true }; ;

            });
        }


        public async Task<dynamic> GetDurationDistribution()
        {
           return await Task.Run(() =>
            {

                var today = DateTime.Today;
                var todayLogs =  _quartzFileHelper.GetJobsLog()
                    .Where(x => x.BeginDate >= today && x.DurationMs > 0).ToList();

                var result = new
                {
                    Fast = todayLogs.Count(x => x.DurationMs < 100),
                    Normal = todayLogs.Count(x => x.DurationMs >= 100 && x.DurationMs < 500),
                    Slow = todayLogs.Count(x => x.DurationMs >= 500 && x.DurationMs < 1000),
                    VerySlow = todayLogs.Count(x => x.DurationMs >= 1000)
                };
                return result;
            });


        }

        public async Task<List<dynamic>> GetErrorTop10()
        {
            return await Task.Run(() =>
            {
                var today = DateTime.Today;
                var topErrorTasks = _quartzFileHelper.GetJobsLog()
                    .Where(x => x.BeginDate >= today && x.JobStatus == 1) // 失败任务
                    .GroupBy(x => x.TaskName)
                    .Select(g => new {
                        TaskName = g.Key,
                        ErrorCount = g.Count()
                    })
                    .OrderByDescending(x => x.ErrorCount)
                    .Take(10)
                    .ToList<dynamic>();

                return topErrorTasks;
            });
           

        }
    }
}
