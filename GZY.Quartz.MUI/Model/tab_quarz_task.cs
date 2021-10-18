using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GZY.Quartz.MUI.Model
{

        /// <summary>
        /// 任务
        /// </summary>
        [Description("任务表")]
        public class tab_quarz_task : BaseModel
        {
            /// <summary>
            /// 任务名
            /// </summary>
            [Description("任务名")]
            public string TaskName { get; set; }
            /// <summary>
            /// 分组名
            /// </summary>
            [Description("分组名")]
            public string GroupName { get; set; }
            /// <summary>
            /// 间隔时间
            /// </summary>
            [Description("间隔时间")]
            public string Interval { get; set; }
            /// <summary>
            /// 调用的API地址
            /// </summary>
            [Description("调用的API地址")]
            public string ApiUrl { get; set; }

            /// <summary>
            /// 任务描述
            /// </summary>
            [Description("任务描述")]
            public string Describe { get; set; }
            /// <summary>
            /// 最近一次运行时间
            /// </summary>
            [Description("最近一次运行时间")]
            public DateTime? LastRunTime { get; set; }
            /// <summary>
            /// 运行状态
            /// </summary>
            [Description("运行状态")]
            public int Status { get; set; }
            /// <summary>
            /// 任务类型(1.DLL类型,2.API类型)
            /// </summary>
            [Description("任务类型(1.DLL类型,2.API类型)")]
            public int TaskType { get; set; }
            #region Api类型专用参数
            /// <summary>
            /// API访问类型(API类型)
            /// </summary>
            [Description("API访问类型(API类型)")]
            public string ApiRequestType { get; set; }
            /// <summary>
            /// 授权名(API类型)
            /// </summary>
            /// 
            [Description("授权名(API类型)")]
            public string ApiAuthKey { get; set; }
            /// <summary>
            /// 授权值(API类型)
            /// </summary>
            /// 
            [Description("授权值(API类型)")]
            public string ApiAuthValue { get; set; }
            /// <summary>
            /// API参数
            /// </summary>
            /// 
            [Description("API参数")]
            public string ApiParameter { get; set; }
            #endregion

            #region DLL类型专用参数
            /// <summary>
            /// DLL类型名
            /// </summary>
            /// 
            [Description("DLL类型名")]
            public string DllClassName { get; set; }

            /// <summary>
            /// Dll方法名
            /// </summary>
            /// 
            [Description("Dll方法名")]
            public string DllActionName { get; set; }
            #endregion
        }
    
}
