using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace ComTransfer
{
    /// <summary>
    /// 日志记录辅助器
    /// </summary>
    internal static class LogHelper
    {
        /// <summary>
        /// 上次日志更新时间
        /// </summary>
        private static DateTime lastSaveTime = DateTime.MinValue;
        /// <summary>
        /// 日志文件夹列表
        /// </summary>
        private static readonly List<string> FolderList = new List<string>() { "SYSTEM", "RECORD" };
        /// <summary>
        /// 过期时间
        /// </summary>
        private static int expiredDays = 30;
        private static string lastMessage = "";
        private static int sameMessageCount = 0;

        /// <summary>
        /// 获取文件夹地址
        /// </summary>
        /// <param name="folder">文件夹名称</param>
        /// <returns>文件夹地址</returns>
        public static string GetFolder(string folder)
        {
            return folder == null ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs") : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", folder);
        }
        /// <summary>
        /// 检查所有日志文件夹，若不存在则自动创建
        /// </summary>
        /// <returns>是否创建成功</returns>
        public static bool CheckFolder()
        {
            try
            {
                foreach (string folder in FolderList)
                {
                    Directory.CreateDirectory(GetFolder(folder));
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void WriteLog(string name, string text)
        {
            Task.Run(async () =>
            {
                await WriteLogAsync(name, text);
            });
        }

        public static async Task<bool> WriteLogAsync(string name, string text)
        {
            if (name + text == lastMessage)
            {
                sameMessageCount++;

                if (sameMessageCount == 10)
                {
                    text = "相同日志已出现多次，停止重复记录...";
                }
                else if (sameMessageCount > 10)
                {
                    return true;
                }
            }
            else
            {
                lastMessage = name + text;
                sameMessageCount = 0;
            }

            DateTime currentTime = DateTime.Now;
            name = name.ToUpper();
            if (!FolderList.Contains(name))
            {
                FolderList.Add(name);
                bool result = CheckFolder();
                if (!result)
                {
                    return false;
                }
            }
            if (lastSaveTime == DateTime.MinValue)
            {
                //第一次写入时检查文件夹
                bool result = CheckFolder();
                if (!result)
                {
                    return false;
                }
            }
            if (currentTime.Date != lastSaveTime.Date)
            {
                //当天第一次写入时删除过期日志
                await DeleteExpiredLogs();
            }
            lastSaveTime = currentTime;

            string timeString = currentTime.ToString("yyyy-MM-dd HH:mm:ss,fff");
            string nameString = name;
            string dataString = text ?? string.Empty;
            string log = string.Format("{0} {1} {2}", timeString, nameString, dataString);
            string path = Path.Combine(GetFolder(name), currentTime.ToString("yyyyMMdd") + ".log");
            try
            {
                FileInfo fileInfo = new FileInfo(path);
                //日志文件大小超过800MB停止写入
                if (fileInfo.Exists && fileInfo.Length > 800 * 1024 * 1024)
                {
                    return false;
                }
                using (StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8))
                {
                    await sw.WriteLineAsync(log);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 设置过期天数
        /// </summary>
        /// <param name="days">过期天数</param>
        public static void SetExpiredDays(int days)
        {
            expiredDays = days;
        }

        /// <summary>
        /// 删除过期日志
        /// </summary>
        /// <returns>是否删除成功</returns>
        public static async Task<bool> DeleteExpiredLogs()
        {
            return await Task.Run(() =>
            {
                DateTime expireTime = DateTime.Now - TimeSpan.FromDays(expiredDays);
                try
                {
                    foreach (string folder in FolderList)
                    {
                        foreach (string file in Directory.GetFiles(GetFolder(folder), "*.log"))
                        {
                            if (File.GetCreationTime(file) < expireTime)
                            {
                                File.Delete(file);
                            }
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }
    }
}