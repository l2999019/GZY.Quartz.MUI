using GZY.Quartz.MUI.Enum;
using GZY.Quartz.MUI.Extensions;
using GZY.Quartz.MUI.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace GZY.Quartz.MUI.Tools
{
    public class QuartzFileHelper
    {

        private  string _rootPath { get; set; }

        private  string _logPath { get; set; }
        private static readonly ConcurrentDictionary<string, object> _fileLocks = new();

        public  string QuartzSettingsFolder { get; set; } = "QuartzSettings";

        public   string Logs { get; set; }="logs";

        public  string TaskJobFileName { get; set; }  = "task_job.json";
        private IWebHostEnvironment _env;
        private IConfiguration _configuration;

        public QuartzFileHelper(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
            CreateQuartzRootPath();
        }
        /// <summary>
        /// 初始化
        /// 创建作业所在根目录及日志文件夹 
        /// </summary>
        /// <returns></returns>
        public  string CreateQuartzRootPath()
        {
            if (!string.IsNullOrEmpty(_rootPath))
                return _rootPath;
            _rootPath = Path.Combine(_env.ContentRootPath, QuartzSettingsFolder) + "/";
            if (!Directory.Exists(_rootPath))
            {
                Directory.CreateDirectory(_rootPath);
            }
            _logPath = Path.Combine(_rootPath, Logs)+ "/";
            //生成日志文件夹
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }
            return _rootPath;
        }

        /// <summary>
        /// 获取jobs
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public List<tab_quarz_task> GetJobs(Expression<Func<tab_quarz_task, bool>> where)
        {
            string path = $"{_rootPath}/{TaskJobFileName}";
            List<tab_quarz_task> list = new List<tab_quarz_task>();

            if (!File.Exists(path))
                return list;
            var tasks = ReadFile(path);
            if (string.IsNullOrEmpty(tasks))
            {
                return null;
            }
            var _taskList = JsonConvert.DeserializeObject<List<tab_quarz_task>>(tasks);
            return _taskList.Where(where.Compile()).ToList();
        }

        /// <summary>
        /// 读取任务日志
        /// </summary>
        /// <param name="taskName"></param>
        /// <param name="groupName"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public  List<tab_quarz_tasklog> GetJobRunLog( string taskName, string groupName, int page, int pageSize = 100)
        {
            string path = $"{_logPath}{groupName}/{taskName}";
            List<tab_quarz_tasklog> list = new List<tab_quarz_tasklog>();
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

        /// <summary>
        /// 写入任务(全量)
        /// </summary>
        /// <param name="taskList"></param>
        public  void WriteJobConfig(List<tab_quarz_task> taskList)
        {
            
            string jobs = JsonConvert.SerializeObject(taskList);
            //写入配置文件
            WriteFile(_rootPath, TaskJobFileName, jobs);
        }

        public  void WriteStartLog(string content)
        {
            content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "," + content;
            if (!content.EndsWith("\r\n"))
            {
                content += "\r\n";
            }
            WriteFile(LogPath, "start.txt", content, true);
        }


        public  void WriteJobLogs(tab_quarz_tasklog tab_Quarz_Tasklog)
        {
            var content = JsonConvert.SerializeObject(tab_Quarz_Tasklog)+"\r\n";
            WriteFile(LogPath, "logs.txt", content, true);
        }


        public  List<tab_quarz_tasklog> GetJobsLog(int pageSize = 1)
        {
            string path = LogPath + "logs.txt";
            path = path.ReplacePath();
            if (!File.Exists(path))
                return null;
            var listlogs = new List<string>();

            listlogs = ReadPageLine(path, pageSize, 5000, true).ToList();

            List<tab_quarz_tasklog> listtasklogs = new List<tab_quarz_tasklog>();
            foreach (var item in listlogs)
            {
                listtasklogs.Add(JsonConvert.DeserializeObject<tab_quarz_tasklog>(item));
            }
            return listtasklogs;
        }
        public  string RootPath
        {
            get { return _rootPath; }
        }

        public  string LogPath
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
        public  IEnumerable<string> ReadPageLine(string fullPath, int page, int pageSize, bool seekEnd = false)
        {
            if (page <= 0)
            {
                page = 1;
            }
            fullPath = fullPath.ReplacePath();
            fullPath= Path.GetFullPath(fullPath);
            var fileLock = _fileLocks.GetOrAdd(fullPath, _ => new object());
            lock (fileLock)
            {
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
        }

        public  string ReadFile(string path)
        {
            path = path.ReplacePath();
            if (!File.Exists(path))
                return "";

            path = Path.GetFullPath(path);
            var fileLock = _fileLocks.GetOrAdd(path, _ => new object());
            lock (fileLock) //加锁顺序执行.
            {
                using (StreamReader stream = new StreamReader(path))
                {
                    return stream.ReadToEnd(); // 读取文件
                }
            }
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="Path">文件路径</param>
        /// <param name="Strings">文件内容</param>
        public  void WriteFile(string path, string fileName, string content, bool appendToLast = false)
        {
            if (!path.EndsWith("/"))
            {
                path = path + "/";
            }
            if (!Directory.Exists(path))//如果不存在就创建file文件夹
            {
                Directory.CreateDirectory(path);
            }
            var pathfile = Path.GetFullPath(path + fileName);
            var fileLock = _fileLocks.GetOrAdd(pathfile, _ => new object());
            lock (fileLock) //按文件加锁顺序执行.
            {
                using (FileStream stream = File.Open(pathfile, FileMode.OpenOrCreate, FileAccess.Write))
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
}
