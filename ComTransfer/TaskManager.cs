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
    /// <summary>
    /// 用于记录计划任务信息的基本类
    /// </summary>
    [Serializable]
    [XmlType(TypeName = "Task")]
    public class TaskNode
    {
        /// <summary>
        /// 触发时间
        /// </summary>
        [XmlAttribute]
        public int Time { get; set; }

        /// <summary>
        /// 文件修改时间起点
        /// </summary>
        [XmlAttribute]
        public int FileTime_Head { get; set; }

        /// <summary>
        /// 文件修改时间终点
        /// </summary>
        [XmlAttribute]
        public int FileTime_Tail { get; set; }

        /// <summary>
        /// 目标文件夹
        /// </summary>
        [XmlAttribute]
        public string Folder { get; set; } = @"C:\";

        /// <summary>
        /// 文件后缀名
        /// </summary>
        [XmlAttribute]
        public string Extension { get; set; } = "*.*";

        /// <summary>
        /// 触发时间的小时数
        /// </summary>
        [XmlIgnore]
        public int Hour
        {
            get => Time / 60;
            set => Time = value * 60 + Time % 60;
        }
        /// <summary>
        /// 触发时间的分钟数
        /// </summary>
        [XmlIgnore]
        public int Minute
        {
            get => Time % 60;
            set => Time = Time - (Time % 60) + value;
        }
        /// <summary>
        /// 实际触发事件
        /// </summary>
        public DateTime TriggerTime => DateTime.Now.Date + TimeSpan.FromMinutes(Time);
        /// <summary>
        /// 实际文件修改时间起点
        /// </summary>
        public DateTime HeadTime => DateTime.Now - TimeSpan.FromMinutes(FileTime_Head);
        /// <summary>
        /// 实际文件修改时间终点
        /// </summary>
        public DateTime TailTime => DateTime.Now - TimeSpan.FromMinutes(FileTime_Tail);
        /// <summary>
        /// 触发时间的文字描述信息
        /// </summary>
        public string TriggerTimeText => string.Format("{0:00}:{1:00}", Time / 60, Time % 60);
        /// <summary>
        /// 获取文本信息
        /// </summary>
        /// <returns>文本字符串</returns>
        public override string ToString()
        {
            return string.Format("每天{0}发送{1}文件夹中的{2}文件，修改时间区间为{3:f1}小时至{4:f1}小时", TriggerTimeText, Folder, Extension, FileTime_Head / 60d, FileTime_Tail / 60d);
        }
        /// <summary>
        /// 获取浅表复制
        /// </summary>
        /// <returns>当前实例的复制</returns>
        public TaskNode Clone()
        {
            return MemberwiseClone() as TaskNode;
        }
    }
    /// <summary>
    /// 用于描述发送失败文件的基本类
    /// </summary>
    [Serializable]
    [XmlType(TypeName = "File")]
    public class FailFileNode
    {
        /// <summary>
        /// 文件地址
        /// </summary>
        [XmlAttribute]
        public string FilePath { get; set; }
        /// <summary>
        /// 失败时间
        /// </summary>
        [XmlAttribute]
        public DateTime FailTime { get; set; }
        /// <summary>
        /// 下一次重试时间
        /// </summary>
        [XmlAttribute]
        public DateTime RetryTime { get; set; }
        /// <summary>
        /// 失败计数
        /// </summary>
        [XmlAttribute]
        public int FailCount { get; set; }
    }
    /// <summary>
    /// 计划任务管理器
    /// </summary>
    public class TaskManager
    {
        /// <summary>
        /// 唯一实例
        /// </summary>
        public static TaskManager Instance = new TaskManager();
        /// <summary>
        /// 计划任务列表
        /// </summary>
        public ObservableCollection<TaskNode> TaskList { get; set; }
        /// <summary>
        /// 发送错误文件列表
        /// </summary>
        public List<FailFileNode> FailFileList { get; set; }
        /// <summary>
        /// 读取计划任务列表数据
        /// </summary>
        /// <returns>计划任务列表</returns>
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

        /// <summary>
        /// 保存计划任务列表数据
        /// </summary>
        /// <param name="list">计划任务列表</param>
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
        /// <summary>
        /// 读取发送错误文件列表数据
        /// </summary>
        /// <returns>发送错误文件列表信息</returns>
        public static List<FailFileNode> ReadFailFileData()
        {
            try
            {
                using (StreamReader sr = new StreamReader("FailFileList.xml", Encoding.UTF8))
                {
                    XmlSerializer reader = new XmlSerializer(typeof(List<FailFileNode>));
                    return reader.Deserialize(sr) as List<FailFileNode>;
                }
            }
            catch
            {
                return new List<FailFileNode>();
            }
        }
        /// <summary>
        /// 保存发送错误文件列表数据
        /// </summary>
        /// <param name="list">发送错误文件列表信息</param>
        public static void SaveFailFileData(List<FailFileNode> list)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("FailFileList.xml", false, Encoding.UTF8))
                {
                    XmlSerializer writer = new XmlSerializer(typeof(List<FailFileNode>));
                    writer.Serialize(sw, list);
                }
            }
            catch
            {
            }
        }
        /// <summary>
        /// 计划任务文件发送事件参数
        /// </summary>
        public class FileTaskEventArgs : EventArgs
        {
            /// <summary>
            /// 文件地址
            /// </summary>
            private readonly string file;
            /// <summary>
            /// 是否发送成功
            /// </summary>
            private readonly bool succeed;
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="filename">文件名称</param>
            public FileTaskEventArgs(string filename)
            {
                file = filename;
            }
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="filename">文件名称</param>
            /// <param name="isSucceed">是否发送成功</param>
            public FileTaskEventArgs(string filename, bool isSucceed)
            {
                file = filename;
                succeed = isSucceed;
            }
            /// <summary>
            /// 错误消息
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// 文件地址
            /// </summary>
            public string File => file;
            /// <summary>
            /// 是否发送成功
            /// </summary>
            public bool IsSucceed => succeed;
        }
        public EventHandler<FileTaskEventArgs> FileTaskHandler;
        /// <summary>
        /// 文件发送过程结束处理器
        /// </summary>
        public EventHandler<FileTaskEventArgs> FileSendedTaskHandler;
        /// <summary>
        /// 构造函数
        /// </summary>
        private TaskManager()
        {
            TaskList = new ObservableCollection<TaskNode>();
            ReadData().ForEach(item => TaskList.Add(item));

            FailFileList = ReadFailFileData();

            Start();

            FileSendedTaskHandler += FileSendedHandler;
        }
        /// <summary>
        /// 文件发送过程结束事件响应
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void FileSendedHandler(object sender, FileTaskEventArgs e)
        {
            string filename = e.File;
            FailFileNode node;

            lock (FailFileList)
            {
                node = FailFileList.FirstOrDefault(item => item.FilePath == filename && item.FailCount >= 0 && item.FailCount < 3);
            }

            if (node != null)
            {
                if (e.IsSucceed)
                {
                    node.FailCount = -1;
                }
                else
                {
                    node.FailCount++;
                    node.FailTime = DateTime.Now;
                    node.RetryTime = DateTime.Now + TimeSpan.FromHours(node.FailCount);
                }
            }
        }

        /// <summary>
        /// 刷新所有计划任务
        /// </summary>
        public void Refresh()
        {
            TaskList.Clear();
            ReadData().ForEach(item => TaskList.Add(item));
        }

        /// <summary>
        /// 启动监视任务
        /// </summary>
        private void Start()
        {
            Task.Factory.StartNew(() =>
            {
                DateTime currentTime = DateTime.Now;

                while (true)
                {
                    DateTime previousTime = currentTime;
                    currentTime = DateTime.Now;

                    while (FailFileList.Count > 100)
                    {
                        FailFileList.RemoveAt(0);
                    }

                    // 删除失效错误文件项目
                    FailFileList.RemoveAll(item => item.FailCount == -1 || item.FailCount >= 3);

                    // 尝试重新发送失败的文件项目
                    foreach (FailFileNode node in FailFileList)
                    {
                        if (node.FailCount == 0)
                        {
                            continue;
                        }
                        DateTime taskTime = node.RetryTime;
                        if (previousTime < taskTime && currentTime >= taskTime)
                        {
                            try
                            {
                                FileInfo fileInfo = new FileInfo(node.FilePath);
                                if (fileInfo.Exists)
                                {
                                    FileTaskHandler?.Invoke(this, new FileTaskEventArgs(fileInfo.FullName) { Message = string.Format("错误重传计数：{0}", node.FailCount) });
                                }
                            }
                            catch
                            {

                            }
                        }
                    }

                    List<TaskNode> list = TaskList.ToList();

                    // 检查计划任务信息
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
                                            FailFileList.Add(new FailFileNode() { FilePath = fileInfo.FullName });
                                            FileTaskHandler?.Invoke(this, new FileTaskEventArgs(fileInfo.FullName));
                                        }
                                        else if (timestamp <= task.TailTime && timestamp >= task.HeadTime)
                                        {
                                            FailFileList.Add(new FailFileNode() { FilePath = fileInfo.FullName });
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
