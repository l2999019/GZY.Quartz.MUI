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

        public async Task<List<tab_quarz_task>> GetJobs(Expression<Func<tab_quarz_task, bool>> where)
        {
            return await _quarzEFContext.tab_quarz_task.Where(where).AsNoTracking().ToListAsync();
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
    }
}
