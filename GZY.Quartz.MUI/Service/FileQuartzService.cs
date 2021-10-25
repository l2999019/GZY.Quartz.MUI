using GZY.Quartz.MUI.Model;
using GZY.Quartz.MUI.Tools;
using System;
using System.Collections.Generic;
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
                 list.Add(model);
                 _quartzFileHelper.WriteJobConfig(list);
                 return new ResultQuartzData { message = "数据保存成功!", status = true };

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

        public Task<ResultQuartzData> Pause(tab_quarz_task model)
        {
            throw new NotImplementedException();
            //return Task.Run(() =>
            //{

            //    var list = _quartzFileHelper.GetJobs(where);
            //    return list;

            //});
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
