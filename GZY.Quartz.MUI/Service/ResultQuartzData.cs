using System;
using System.Collections.Generic;
using System.Text;

namespace GZY.Quartz.MUI.Service
{
    public class ResultQuartzData
    {
        public bool status { get; set; }
        public string message { get; set; }
    }

    public class ResultDashboardData
    {
        /// <summary>
        /// 任务执行次数
        /// </summary>
        public int JobCounts { get; set; }
        /// <summary>
        /// 错误率
        /// </summary>
        public double ErrorCounts { get; set; }

        /// <summary>
        /// 平均耗时
        /// </summary>
        public double AverageTime { get; set; }

        /// <summary>
        /// 运行任务/任务总数
        /// </summary>
        public string RunJobs { get; set; }
    }

    public class ResultData<T> where T :class
    {
        public int total { get; set; }
        public List<T> data { get; set; }
    }
}
