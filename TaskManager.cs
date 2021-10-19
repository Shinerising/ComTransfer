using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComTransfer
{
    public class TaskManager
    {
        public class TaskNode
        {
            public int Time { get; set; }
            public int FileTime_Head { get; set; }
            public int FileTime_Tail { get; set; }
            public string Folder { get; set; }
            public string Extension { get; set; }
            public DateTime TriggerTime => DateTime.Now.Date + TimeSpan.FromMinutes(Time);
            public DateTime HeadTime => DateTime.Now.Date - TimeSpan.FromMinutes(FileTime_Head);
            public DateTime TailTime => DateTime.Now.Date - TimeSpan.FromMinutes(FileTime_Tail);
            public string TriggerTimeText => string.Format("{0:00}:{1:00}", Time / 60, Time % 60);
            public override string ToString()
            {
                return string.Format("每天{0}发送{1}文件夹中的{2}文件，修改时间区间为{3:f1}小时至{4:f1}小时", TriggerTimeText, Folder, Extension, FileTime_Head / 60d, FileTime_Tail / 60d);
            }
        }
        public static TaskManager Instance = new TaskManager();
        public List<TaskNode> TaskList { get; set; } = new List<TaskNode>
        {
            new TaskNode()
            {
                Time = 480,
                FileTime_Head = 12 * 60,
                FileTime_Tail = 24 * 60,
                Folder = @"D:\alarm",
                Extension = "TXT"
            },
            new TaskNode()
            {
                Time = 640,
                FileTime_Head = 12 * 60,
                FileTime_Tail = 24 * 60,
                Folder = @"D:\monitor",
                Extension = "MDB"
            }
        };
        public EventHandler<string> FileTaskHandler;
        private TaskManager()
        {
            Start();
        }

        private void Start()
        {
            Task.Factory.StartNew(() =>
            {
                DateTime currentTime = DateTime.Now;

                while (true)
                {
                    DateTime previousTime = currentTime;
                    currentTime = DateTime.Now;

                    foreach (TaskNode task in TaskList)
                    {
                        DateTime taskTime = task.TriggerTime;
                        if (previousTime < taskTime && currentTime >= taskTime)
                        {
                            try
                            {
                                foreach(string file in Directory.GetFiles(task.Folder, task.Extension, SearchOption.TopDirectoryOnly))
                                {
                                    FileInfo fileInfo = new FileInfo(file);
                                    if (fileInfo.Exists)
                                    {
                                        if (fileInfo.LastWriteTime >= task.TailTime && fileInfo.LastWriteTime <= task.HeadTime)
                                        {
                                            FileTaskHandler?.Invoke(this, fileInfo.FullName);
                                        }
                                    }
                                }
                            }
                            catch
                            {

                            }
                        }
                    }


                    Thread.Sleep(60000);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
