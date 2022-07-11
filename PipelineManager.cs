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
            FilePullResponse
        }
        public const string PipelineName = "PIPE_COMTRANSFER";
        public static bool IsConnected => stream != null && stream.IsConnected;
        private static NamedPipeClientStream stream;
        private static StreamWriter writer = null;
        private static readonly Queue<string> MessageQueue = new Queue<string>();
        private static MainWindow mainWindow;
        public static void Initialize(MainWindow window)
        {
            mainWindow = window;

            StartMonitoring();
        }
        private static void ResetClient()
        {
            stream?.Dispose();

            stream = new NamedPipeClientStream(".", PipelineName, PipeDirection.InOut, PipeOptions.Asynchronous);
            stream.Connect();
            writer = new StreamWriter(stream) { AutoFlush = true };
        }
        public static void SendCommand(CommandType command, string data)
        {
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
                    
                    if (stream.CanRead)
                    {
                        string message = ReadMessage();
                        ResolveMessage(message);
                    }

                    Thread.Sleep(100);
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
            }
        }

        public static string ReadMessage()
        {
            if (stream == null || !stream.IsConnected)
            {
                return null;
            }
            try
            {
                var reader = new StreamReader(stream);
                if (reader.EndOfStream)
                {
                    return string.Empty;
                }
                string message = reader.ReadLine();
                return message;
            }
            catch
            {
                return null;
            }
        }

        public static void WriteMessage(string message)
        {
            MessageQueue.Enqueue(message);

            while (MessageQueue.Count > 256)
            {
                MessageQueue.Dequeue();
            }
        }
    }
}
