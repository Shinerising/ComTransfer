using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComTransfer
{
    /// <summary>
    /// 命名管道通信管理器
    /// </summary>
    internal class PipelineManager
    {
        /// <summary>
        /// 指令类型
        /// </summary>
        public enum CommandType
        {
            /// <summary>
            /// 默认指令
            /// </summary>
            Default,
            /// <summary>
            /// 工作日志
            /// </summary>
            WorkingLog,
            /// <summary>
            /// 文件传输日志
            /// </summary>
            TransferLog,
            /// <summary>
            /// 连接参数
            /// </summary>
            ConnectParam,
            /// <summary>
            /// 连接状态
            /// </summary>
            ConnectState,
            /// <summary>
            /// 上传进度
            /// </summary>
            UploadProgress,
            /// <summary>
            /// 下载进度
            /// </summary>
            DownloadProgress,
            /// <summary>
            /// 文件目录请求
            /// </summary>
            FileTreeRequest,
            /// <summary>
            /// 文件目录回应
            /// </summary>
            FileTreeResponse,
            /// <summary>
            /// 文件发送请求
            /// </summary>
            FilePushRequest,
            /// <summary>
            /// 文件发送回应
            /// </summary>
            FilePushResponse,
            /// <summary>
            /// 文件拉取请求
            /// </summary>
            FilePullRequest,
            /// <summary>
            /// 文件拉取回应
            /// </summary>
            FilePullResponse,
            /// <summary>
            /// 文件已发送
            /// </summary>
            FileSended,
            /// <summary>
            /// 文件已接收
            /// </summary>
            FileReceived,
            /// <summary>
            /// 目录列表
            /// </summary>
            FolderList,
            /// <summary>
            /// 停止发送
            /// </summary>
            StopSend,
            /// <summary>
            /// 停止接收
            /// </summary>
            StopReceive
        }
        /// <summary>
        /// 管道名称
        /// </summary>
        public const string PipelineName = "PIPE_COMTRANSFER";
        /// <summary>
        /// 命名管道是否已连接
        /// </summary>
        public static bool IsConnected => stream != null && stream.IsConnected;
        /// <summary>
        /// 命名管道通信流
        /// </summary>
        private static NamedPipeClientStream stream;
        /// <summary>
        /// 写入流
        /// </summary>
        private static StreamWriter writer = null;
        /// <summary>
        /// 读取流
        /// </summary>
        private static StreamReader reader = null;
        /// <summary>
        /// 指令发送缓冲区
        /// </summary>
        private static readonly Queue<string> MessageQueue = new Queue<string>();
        /// <summary>
        /// 主窗口
        /// </summary>
        private static MainWindow mainWindow;
        /// <summary>
        /// 串口通信管理
        /// </summary>
        private static ComPort comPort;
        /// <summary>
        /// 初始化命名管道通信
        /// </summary>
        /// <param name="window">主窗口对象</param>
        /// <param name="port">串口通信对象</param>
        public static void Initialize(MainWindow window, ComPort port)
        {
            mainWindow = window;
            comPort = port;

            StartMonitoring();
        }
        /// <summary>
        /// 重置命名管道客户端
        /// </summary>
        private static void ResetClient()
        {
            stream?.Dispose();

            stream = new NamedPipeClientStream(".", PipelineName, PipeDirection.InOut, PipeOptions.Asynchronous);
            stream.Connect();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };
        }
        /// <summary>
        /// 发送指令文本
        /// </summary>
        /// <param name="command">指令类型</param>
        /// <param name="data">文本描述</param>
        public static void SendCommand(CommandType command, string data)
        {
            if (data != null && data.ToUpper().EndsWith(".APPCOMMAND"))
            {
                return;
            }

            string text = string.Format("{0}:{1}", command, data);
            WriteMessage(text);
        }

        /// <summary>
        /// 启动监视任务
        /// </summary>
        private static void StartMonitoring()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (stream == null || !stream.IsConnected)
                    {
                        ResetClient();
                    }

                    SendCommand(CommandType.FolderList, comPort.GetDirectoryInfo());

                    if (stream.CanRead && !reader.EndOfStream)
                    {
                        string message = reader.ReadLine();
                        ResolveMessage(message);
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (stream != null && stream.IsConnected && stream.CanWrite)
                    {
                        while (MessageQueue.Count > 0)
                        {
                            try
                            {
                                string message = MessageQueue.Dequeue();
                                writer.WriteLine(message);
                                stream.WaitForPipeDrain();
                            }
                            catch
                            {

                            }
                        }
                    }
                    
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 指令文本处理过程
        /// </summary>
        /// <param name="message">指令文本内容</param>
        private static void ResolveMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            int offset = message.IndexOf(':');
            if (offset == -1)
            {
                return;
            }

            string command = message.Substring(0, offset);
            string data = message.Substring(offset + 1);

            CommandType commandType = CommandType.Default;
            try
            {
                commandType = (CommandType)Enum.Parse(typeof(CommandType), command);
            }
            catch
            {

            }

            switch (commandType)
            {
                case CommandType.Default:
                    break;
                case CommandType.FileTreeRequest:
                    mainWindow.SubmitCommand("requestfile", data);
                    break;
                case CommandType.FilePushRequest:
                    if (!string.IsNullOrEmpty(data) && data.IndexOf('>') != -1)
                    {
                        int index = data.IndexOf('>');
                        string localPath = data.Substring(0, index);
                        string remotePath = data.Substring(index + 1);
                        try
                        {
                            comPort.SubmitCommand("setlocation", Path.Combine(remotePath, new FileInfo(localPath).Name));
                            comPort.SendFile(localPath);
                            SendCommand(CommandType.FilePushResponse, "");
                        }
                        catch
                        {

                        }
                    }
                    break;
                case CommandType.FilePullRequest:
                    if (!string.IsNullOrEmpty(data) && data.IndexOf('>') != -1)
                    {
                        int index = data.IndexOf('>');
                        string remotePath = data.Substring(0, index);
                        string localPath = data.Substring(index + 1);
                        try
                        {
                            comPort.SetLocation(Path.Combine(localPath, new FileInfo(remotePath).Name));
                            comPort.SubmitCommand("fetch", remotePath);
                            SendCommand(CommandType.FilePullResponse, "");
                        }
                        catch
                        {

                        }
                    }
                    break;
                case CommandType.StopReceive:
                    if (comPort.IsReceiving)
                    {
                        comPort.ForceStopReceiving();
                    }
                    break;
                case CommandType.StopSend:
                    if (comPort.IsSending)
                    {
                        comPort.ForceStopSending();
                    }
                    break;
            }
        }
        /// <summary>
        /// 将指令文本写入到缓冲区
        /// </summary>
        /// <param name="message">指令文本</param>
        public static void WriteMessage(string message)
        {
            if (!IsConnected)
            {
                return;
            }

            MessageQueue.Enqueue(message);

            while (MessageQueue.Count > 256)
            {
                MessageQueue.Dequeue();
            }
        }
    }
}
