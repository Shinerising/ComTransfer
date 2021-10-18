using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComTransfer
{
    public class TaskManager
    {
        public class TaskNode
        {
            public int TriggerTime { get; set; }
            public int FileTime_Head { get; set; }
            public int FileTime_Tail { get; set; }
            public string Folder { get; set; }
            public string Extension { get; set; }
            public string TriggerTimeText => string.Format("{0:00}:{1:00}", TriggerTime / 60, TriggerTime % 60);
            public override string ToString()
            {
                return string.Format("每天{0}发送{1}文件夹中的{2}文件，修改时间区间为{3:f1}小时至{4:f1}小时", TriggerTimeText, Folder, Extension, FileTime_Head / 60d, FileTime_Tail / 60d);
            }
        }
        public static TaskManager Instance = new TaskManager();
        public List<TaskNode> TastList { get; set; } = new List<TaskNode>
        {
            new TaskNode()
            {
                TriggerTime = 480,
                FileTime_Head = 12 * 60,
                FileTime_Tail = 24 * 60,
                Folder = @"D:\alarm",
                Extension = "TXT"
            },
            new TaskNode()
            {
                TriggerTime = 640,
                FileTime_Head = 12 * 60,
                FileTime_Tail = 24 * 60,
                Folder = @"D:\monitor",
                Extension = "MDB"
            }
        };
    }
}
