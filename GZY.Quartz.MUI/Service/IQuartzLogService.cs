using GZY.Quartz.MUI.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GZY.Quartz.MUI.Service
{
    public interface IQuartzLogService
    {
        Task<ResultData<tab_quarz_tasklog>> GetLogs(string taskName, string groupName, int page, int pageSize = 100);
        Task<tab_quarz_tasklog> Getlastlog(string taskName, string groupName);

        Task<ResultQuartzData> AddLog(tab_quarz_tasklog tab_Quarz_Tasklog);

       

    }
}
