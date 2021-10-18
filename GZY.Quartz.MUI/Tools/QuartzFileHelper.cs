using GZY.Quartz.MUI.Enum;
using GZY.Quartz.MUI.Extensions;
using GZY.Quartz.MUI.Model;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GZY.Quartz.MUI.Tools
{
    public class QuartzFileHelper
    {


        private static string _rootPath { get; set; }

        private static string _logPath { get; set; }

        public static string QuartzSettingsFolder { get; set; } = "QuartzSettings";

        public static  string Logs { get; set; }="logs";

        public static string JobConfigFileName { get; set; }  = "job_options.json";
        /// <summary>
        /// 创建作业所在根目录及日志文件夹 
        /// </summary>
        /// <returns></returns>
        public static string CreateQuartzRootPath(IWebHostEnvironment env)
        {
            if (!string.IsNullOrEmpty(_rootPath))
                return _rootPath;
            _rootPath = $"{Directory.GetParent(env.ContentRootPath).FullName}\\{QuartzSettingsFolder}\\";
            _rootPath = _rootPath.ReplacePath();
            if (!Directory.Exists(_rootPath))
            {
                Directory.CreateDirectory(_rootPath);
            }
            _logPath = _rootPath + Logs + "\\";
            _logPath = _logPath.ReplacePath();
            //生成日志文件夹
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }
            return _rootPath;
        }

        /// <summary>
        /// 初始化作业日志文件,以txt作为文件
        /// </summary>
        /// <param name="groupJobName"></param>
        public static void InitGroupJobFileLog(string groupJobName)
        {
            string jobFile = _logPath + groupJobName;
            jobFile = jobFile.ReplacePath();
            if (!File.Exists(jobFile))
            {
                File.Create(jobFile);
            }
        }

        public static List<tab_quarz_tasklog> GetJobRunLog(string taskName, string groupName, int page, int pageSize = 100)
        {
            string path = $"{_logPath}{groupName}\\{taskName}.txt";
            List<tab_quarz_tasklog> list = new List<tab_quarz_tasklog>();

            path = path.ReplacePath();
            if (!File.Exists(path))
                return list;
            var logs = ReadPageLine(path, page, pageSize, true);
            foreach (string item in logs)
            {
                string[] arr = item?.Split('_');
                if (item == "" || arr == null || arr.Length == 0)
                    continue;
                if (arr.Length != 3)
                {
                    list.Add(new tab_quarz_tasklog() { Msg = item });
                    continue;
                }
                list.Add(new tab_quarz_tasklog() { BeginDate = Convert.ToDateTime(arr[0]), EndDate = Convert.ToDateTime(arr[1]), Msg = arr[2] });
            }
            return list.OrderByDescending(x => x.BeginDate).ToList();
        }

        public static void WriteJobConfig(List<tab_quarz_task> taskList)
        {
            string jobs = JsonConvert.SerializeObject(taskList);
            //写入配置文件
            WriteFile(_rootPath, JobConfigFileName, jobs);
        }

        public static void WriteStartLog(string content)
        {
            content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "," + content;
            if (!content.EndsWith("\r\n"))
            {
                content += "\r\n";
            }
            WriteFile(LogPath, "start.txt", content, true);
        }
        public static void WriteJobAction(JobState jobAction, ITrigger trigger, string taskName, string groupName)
        {
            WriteJobAction(jobAction, taskName, groupName, trigger == null ? "未找到作业" : "OK");
        }
        public static void WriteJobAction(JobState jobAction, string taskName, string groupName, string content = null)
        {
            content = $"{jobAction.ToString()} --  {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}  --分组：{groupName},作业：{taskName},消息:{content ?? "OK"}\r\n";
            WriteFile(LogPath, "action.txt", content, true);
        }

        public static void WriteAccess(string content = null)
        {
            content = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}_{content}\r\n";
            WriteFile(LogPath, "access.txt", content, true);
        }

        public static string GetAccessLog(int pageSize = 1)
        {
            string path = LogPath + "access.txt";
            path = path.ReplacePath();
            if (!File.Exists(path))
                return "没有找到目录";
            return string.Join("<br/>", ReadPageLine(path, pageSize, 5000, true).ToList());
        }
        public static string RootPath
        {
            get { return _rootPath; }
        }

        public static string LogPath
        {
            get { return _logPath; }
        }
        /// <summary>
        /// 读取本地txt日志内容
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="seekEnd"></param>
        /// <returns></returns>
        public static IEnumerable<string> ReadPageLine(string fullPath, int page, int pageSize, bool seekEnd = false)
        {
            if (page <= 0)
            {
                page = 1;
            }
            fullPath = fullPath.ReplacePath();
            var lines = File.ReadLines(fullPath, Encoding.UTF8);
            if (seekEnd)
            {
                int lineCount = lines.Count();
                int linPageCount = (int)Math.Ceiling(lineCount / (pageSize * 1.00));
                //超过总页数，不处理
                if (page > linPageCount)
                {
                    page = 0;
                    pageSize = 0;
                }
                else if (page == linPageCount)//最后一页，取最后一页剩下所有的行
                {
                    pageSize = lineCount - (page - 1) * pageSize;
                    if (page == 1)
                    {
                        page = 0;
                    }
                    else
                    {
                        page = lines.Count() - page * pageSize;
                    }
                }
                else
                {
                    page = lines.Count() - page * pageSize;
                }
            }
            else
            {
                page = (page - 1) * pageSize;
            }
            lines = lines.Skip(page).Take(pageSize);

            var enumerator = lines.GetEnumerator();
            int count = 1;
            while (enumerator.MoveNext() || count <= pageSize)
            {
                yield return enumerator.Current;
                count++;
            }
            enumerator.Dispose();
        }

        public static string ReadFile(string path)
        {
            path = path.ReplacePath();
            if (!File.Exists(path))
                return "";
            using (StreamReader stream = new StreamReader(path))
            {
                return stream.ReadToEnd(); // 读取文件
            }
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="Path">文件路径</param>
        /// <param name="Strings">文件内容</param>
        public static void WriteFile(string path, string fileName, string content, bool appendToLast = false)
        {
            if (!path.EndsWith("\\"))
            {
                path = path + "\\";
            }
            path = path.ReplacePath();
            if (!Directory.Exists(path))//如果不存在就创建file文件夹
            {
                Directory.CreateDirectory(path);
            }
            using (FileStream stream = File.Open(path + fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                byte[] by = Encoding.Default.GetBytes(content);
                if (appendToLast)
                {
                    stream.Position = stream.Length;
                }
                else
                {
                    stream.SetLength(0);
                }
                stream.Write(by, 0, by.Length);
            }
        }
    }
}
