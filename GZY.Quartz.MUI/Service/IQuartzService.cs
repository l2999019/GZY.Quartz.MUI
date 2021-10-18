using GZY.Quartz.MUI.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GZY.Quartz.MUI.Service
{
    public interface IQuartzService
    {
        /// <summary>
        /// 获取所有作业
        /// </summary>
        /// <returns></returns>
        Task<List<tab_quarz_task>> GetJobs(Expression<Func<tab_quarz_task, bool>> where);

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ResultQuartzData> AddJob(tab_quarz_task model);

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ResultQuartzData> Remove(tab_quarz_task model);

        /// <summary>
        /// 更新任务
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        Task<ResultQuartzData> Update(tab_quarz_task model);

      
    }
}
