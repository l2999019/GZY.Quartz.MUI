using GZY.Quartz.MUI.EFContext;
using GZY.Quartz.MUI.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZY.Quartz.MUI.Service
{
    public class EFQuartzLogService : IQuartzLogService
    {
        private QuarzEFContext _quarzEFContext;
        public EFQuartzLogService(QuarzEFContext quarzEFContext)
        {
            _quarzEFContext = quarzEFContext;
        }

        public async Task<ResultQuartzData> AddLog(tab_quarz_tasklog tab_Quarz_Tasklog)
        {
            _quarzEFContext.Add(tab_Quarz_Tasklog);
            var date = await _quarzEFContext.SaveChangesAsync();
            var result = new ResultQuartzData { status = false, message = "" };
            if (date > 0)
            {
                result.status = true;
                result.message = "数据库添加成功!";
            }
            return result;
        }

        public async Task<tab_quarz_tasklog> Getlastlog(string taskName, string groupName)
        {
            var data = await _quarzEFContext.tab_quarz_tasklog.Where(a => a.TaskName == taskName
              && a.GroupName == groupName).OrderByDescending(a=>a.id).FirstOrDefaultAsync();
            return data;
        }

        public async Task<ResultData<tab_quarz_tasklog>> GetLogs(string taskName, string groupName, int page, int pageSize = 100)
        {
            int total = _quarzEFContext.tab_quarz_tasklog.Where(a => a.TaskName == taskName
              && a.GroupName == groupName).Count();
            var pagem = page - 1;
            var data = await _quarzEFContext.tab_quarz_tasklog.Where(a => a.TaskName == taskName
            && a.GroupName == groupName).OrderByDescending(a=>a.id).Skip(pagem * pageSize).Take(pageSize).ToListAsync();
            ResultData<tab_quarz_tasklog> resultData = new ResultData<tab_quarz_tasklog>() { total = total, data = data };
            return resultData;
        }
    }
    }
