using System;
using System.Collections.Generic;
using System.Text;

namespace GZY.Quartz.MUI.Model
{
    public class BaseModel
    {

        /// <summary>
        /// 主键ID
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 数据插入时间
        /// </summary>
        public Nullable<DateTime> timeflag { get; set; }
        /// <summary>
        /// 数据修改时间
        /// </summary>
        public Nullable<DateTime> changetime { get; set; }
    }
}
