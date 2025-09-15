using GZY.Quartz.MUI.EFContext;
using GZY.Quartz.MUI.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GZY.Quartz.MUI.Service
{
    public class EFQuartzService : IQuartzService
    {
        private QuarzEFContext _quarzEFContext;
        public EFQuartzService(QuarzEFContext quarzEFContext)
        {
            _quarzEFContext = quarzEFContext;
        }
        public async Task<ResultQuartzData> AddJob(tab_quarz_task model)
        {
            _quarzEFContext.Add(model);
            var date= await _quarzEFContext.SaveChangesAsync();
            var result =new ResultQuartzData { status = false, message ="" };
            if (date > 0)
            {
                result.status = true;
                result.message = "数据库添加成功!";
            }
            return result;
        }

        public async Task<ResultDashboardData> GetDashboardInfo()
        {
            var result = new ResultDashboardData();
            //查询当天任务执行次数
            result.JobCounts = await _quarzEFContext.tab_quarz_tasklog.Where(a => a.BeginDate >= DateTime.Today).CountAsync();
            //查询运行中的任务数量/任务总数
            result.RunJobs =string.Join("/", _quarzEFContext.tab_quarz_task.Where(a => a.Status == 6).Count(), _quarzEFContext.tab_quarz_task.Count());
            //查询当天任务执行平均耗时
            var logs = await _quarzEFContext.tab_quarz_tasklog.Where(a => a.BeginDate >= DateTime.Today).ToListAsync();
            result.AverageTime =logs.Select(a => a.DurationMs).DefaultIfEmpty(0).Average();   // 如果 logs 为空，默认值 0
            //查询当天任务错误率
            var daylogs = await _quarzEFContext.tab_quarz_tasklog.Where(a => a.BeginDate >= DateTime.Today).Select(a => a.JobStatus).ToListAsync();
            int logcount = daylogs.Count;
            int errorcount = daylogs.Where(a=>a==1).Count();
            result.ErrorCounts = logcount == 0? 0: ((double)errorcount / logcount) *100;
            return result;

        }

        public async Task<List<dynamic>> GetFailureRate()
        {
            var query = await _quarzEFContext.tab_quarz_tasklog
                         .Where(x => x.BeginDate.HasValue)
                         .GroupBy(x => x.TaskName)
                         .Select(g => new
                         {
                             TaskName = g.Key,
                             LastExec = g.Max(x => x.BeginDate), // 最近一次执行时间
                             Total = g.Count(),
                             Fail = g.Count(x => x.JobStatus != 0)
                         })
                         .OrderByDescending(x => x.LastExec) // 按最近执行排序
                         .Take(10)                           // 只取前 10 个
                         .ToListAsync();                    // 转内存计算失败率
            var result = query
                         .Select(g => new
                         {
                             g.TaskName,
                             FailRate = g.Total == 0 ? 0 : Math.Round((double)g.Fail / g.Total * 100, 2)
                         })
                         .ToList<dynamic>();
            return result;
        }

        public async Task<List<tab_quarz_task>> GetJobs(Expression<Func<tab_quarz_task, bool>> where)
        {
            return await _quarzEFContext.tab_quarz_task.Where(where).AsNoTracking().ToListAsync();
        }

        public async Task<List<dynamic>> GetTrendInfo()
        {
            var query = await _quarzEFContext.tab_quarz_tasklog
                        .Where(x => x.BeginDate.HasValue && x.BeginDate.Value.Date == DateTime.Today)
                        .GroupBy(x => x.BeginDate.Value.Hour)
                        .Select(g => new {
                            Hour = g.Key, // 小时 (0-23)
                            SuccessCount = g.Count(x => x.JobStatus == 0),
                            FailCount = g.Count(x => x.JobStatus != 0),
                            AvgDuration = g.Average(x => x.DurationMs)
                        })
                        .OrderBy(x => x.Hour)
                        .ToListAsync<dynamic>();

            return query;

        }

        public async Task<ResultQuartzData> Remove(tab_quarz_task model)
        {
            _quarzEFContext.Remove(model);
            var date = await _quarzEFContext.SaveChangesAsync();
            var result = new ResultQuartzData { status = false, message = "" };
            if (date > 0)
            {
                result.status = true;
                result.message = "数据库删除成功!";
            }
            return result;
        }

      

        public async Task<ResultQuartzData> Update(tab_quarz_task model)
        {
            _quarzEFContext.Attach(model);
            _quarzEFContext.Entry(model).State = EntityState.Modified;
            var date = await _quarzEFContext.SaveChangesAsync();
            var result = new ResultQuartzData { status = false, message = "" };
            if (date > 0)
            {
                result.status = true;
                result.message = "数据库修改成功!";
            }
            return result;
        }

        public async Task<dynamic> GetDurationDistribution()
        {
            var today = DateTime.Today;
            var todayLogs =await  _quarzEFContext.tab_quarz_tasklog
                .Where(x => x.BeginDate >= today && x.DurationMs > 0).ToListAsync();

            var result = new
            {
                Fast =  todayLogs.Count(x => x.DurationMs < 100),
                Normal =  todayLogs.Count(x => x.DurationMs >= 100 && x.DurationMs < 500),
                Slow =  todayLogs.Count(x => x.DurationMs >= 500 && x.DurationMs < 1000),
                VerySlow =  todayLogs.Count(x => x.DurationMs >= 1000)
            };
            return result;

        }

        public async Task<List<dynamic>> GetErrorTop10()
        {
            var today = DateTime.Today;
            var topErrorTasks = await _quarzEFContext.tab_quarz_tasklog
                .Where(x => x.BeginDate >= today && x.JobStatus == 1) // 失败任务
                .GroupBy(x => x.TaskName)
                .Select(g => new {
                    TaskName = g.Key,
                    ErrorCount = g.Count()
                })
                .OrderByDescending(x => x.ErrorCount)
                .Take(10)
                .ToListAsync<dynamic>();

            return topErrorTasks;

        }
    }
}
