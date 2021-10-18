using GZY.Quartz.MUI.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GZY.Quartz.MUI.Service
{
    public class FileQuartzService : IQuartzService
    {
        public Task<ResultQuartzData> AddJob(tab_quarz_task model)
        {
            throw new NotImplementedException();
        }

        public Task<List<tab_quarz_task>> GetJobs(Expression<Func<tab_quarz_task, bool>> where)
        {
            throw new NotImplementedException();
        }

        public Task<ResultQuartzData> Pause(tab_quarz_task model)
        {
            throw new NotImplementedException();
        }

        public Task<ResultQuartzData> Remove(tab_quarz_task model)
        {
            throw new NotImplementedException();
        }

        public Task<ResultQuartzData> Run(tab_quarz_task model)
        {
            throw new NotImplementedException();
        }

        public Task<ResultQuartzData> Start(tab_quarz_task model)
        {
            throw new NotImplementedException();
        }

        public Task<ResultQuartzData> Update(tab_quarz_task model)
        {
            throw new NotImplementedException();
        }
    }
}
