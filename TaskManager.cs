using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ComTransfer
{
    [Serializable]
    [XmlType(TypeName = "Task")]
    public class TaskNode
    {
        [XmlAttribute]
        public int Time { get; set; }

        [XmlAttribute]
        public int FileTime_Head { get; set; }

        [XmlAttribute]
        public int FileTime_Tail { get; set; }

        [XmlAttribute]
        public string Folder { get; set; } = @"C:\";

        [XmlAttribute]
        public string Extension { get; set; } = "*.*";

        [XmlIgnore]
        public int Hour
        {
            get => Time / 60;
            set => Time = value * 60 + Time % 60;
        }
        [XmlIgnore]
        public int Minute
        {
            get => Time % 60;
            set => Time = Time - (Time % 60) + value;
        }
        public DateTime TriggerTime => DateTime.Now.Date + TimeSpan.FromMinutes(Time);
        public DateTime HeadTime => DateTime.Now - TimeSpan.FromMinutes(FileTime_Head);
        public DateTime TailTime => DateTime.Now - TimeSpan.FromMinutes(FileTime_Tail);
        public string TriggerTimeText => string.Format("{0:00}:{1:00}", Time / 60, Time % 60);
        public override string ToString()
        {
            return string.Format("每天{0}发送{1}文件夹中的{2}文件，修改时间区间为{3:f1}小时至{4:f1}小时", TriggerTimeText, Folder, Extension, FileTime_Head / 60d, FileTime_Tail / 60d);
        }
        public TaskNode Clone()
        {
            return MemberwiseClone() as TaskNode;
        }
    }
    public class TaskManager
    {
        public static TaskManager Instance = new TaskManager();
        public ObservableCollection<TaskNode> TaskList { get; set; }
        public static List<TaskNode> ReadData()
        {
            try
            {
                using (StreamReader sr = new StreamReader("TaskList.xml", Encoding.UTF8))
                {
                    XmlSerializer reader = new XmlSerializer(typeof(List<TaskNode>));
                    return reader.Deserialize(sr) as List<TaskNode>;
                }
            }
            catch
            {
                return new List<TaskNode>();
            }
        }

        public static void SaveData(List<TaskNode> list)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("TaskList.xml", false, Encoding.UTF8))
                {
                    XmlSerializer writer = new XmlSerializer(typeof(List<TaskNode>));
                    writer.Serialize(sw, list);
                }
            }
            catch
            {
            }
        }
        public class FileTaskEventArgs : EventArgs
        {
            private readonly string filename;

            public FileTaskEventArgs(string filename)
            {
                this.filename = filename;
            }

            public string File => this.filename;
        }
        public EventHandler<FileTaskEventArgs> FileTaskHandler;
        private TaskManager()
        {
            TaskList = new ObservableCollection<TaskNode>();
            ReadData().ForEach(item => TaskList.Add(item));
            Start();
        }

        public void Refresh()
        {
            TaskList.Clear();
            ReadData().ForEach(item => TaskList.Add(item));
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

                    List<TaskNode> list = TaskList.ToList();
                    foreach (TaskNode task in TaskList)
                    {
                        if (task.Folder == null || task.Folder.Length == 0 || task.Extension == null || task.Extension.Length == 0)
                        {
                            continue;
                        }
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
                                        DateTime timestamp = fileInfo.LastWriteTime;
                                        if (timestamp >= task.TailTime && timestamp <= task.HeadTime)
                                        {
                                            FileTaskHandler?.Invoke(this, new FileTaskEventArgs(fileInfo.FullName));
                                        }
                                        else if (timestamp <= task.TailTime && timestamp >= task.HeadTime)
                                        {
                                            FileTaskHandler?.Invoke(this, new FileTaskEventArgs(fileInfo.FullName));
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
