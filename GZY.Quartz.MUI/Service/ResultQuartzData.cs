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

    public class ResultData<T> where T :class
    {
        public int total { get; set; }
        public List<T> data { get; set; }
    }
}
