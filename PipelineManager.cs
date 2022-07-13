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
    internal class PipelineManager
    {
        public enum CommandType
        {
            Default,
            WorkingLog,
            TransferLog,
            ConnectParam,
            ConnectState,
            UploadProgress,
            DownloadProgress,
            FileTreeRequest,
            FileTreeResponse,
            FilePushRequest,
            FilePushResponse,
            FilePullRequest,
            FilePullResponse,
            FileSended,
            FileReceived,
            FolderList,
            StopSend,
            StopReceive
        }
        public const string PipelineName = "PIPE_COMTRANSFER";
        public static bool IsConnected => stream != null && stream.IsConnected;
        private static NamedPipeClientStream stream;
        private static StreamWriter writer = null;
        private static StreamReader reader = null;
        private static readonly Queue<string> MessageQueue = new Queue<string>();
        private static MainWindow mainWindow;
        private static ComPort comPort;
        public static void Initialize(MainWindow window, ComPort port)
        {
            mainWindow = window;
            comPort = port;

            StartMonitoring();
        }
        private static void ResetClient()
        {
            stream?.Dispose();

            stream = new NamedPipeClientStream(".", PipelineName, PipeDirection.InOut, PipeOptions.Asynchronous);
            stream.Connect();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };
        }
        public static void SendCommand(CommandType command, string data)
        {
            if (data != null && data.ToUpper().EndsWith(".APPCOMMAND"))
            {
                return;
            }

            string text = string.Format("{0}:{1}", command, data);
            WriteMessage(text);
        }

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

                    SendCommand(CommandType.FolderList, string.Join("|", comPort.PortOption));

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
