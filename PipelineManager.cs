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
        public static void Initialize()
        {
            stream = new NamedPipeClientStream(".", PipelineName, PipeDirection.InOut, PipeOptions.Asynchronous);
            StartMonitoring();
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
                    if (!stream.IsConnected)
                    {
                        stream.Connect();
                    }
                    else if (stream.CanRead)
                    {
                        string message = ReadMessage();
                        ResolveMessage(message);
                    }

                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);
        }

        private static void ResolveMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Console.WriteLine(message);
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
            if (stream == null || !stream.IsConnected)
            {
                return;
            }

            try
            {
                var writer = new StreamWriter(stream)
                {
                    AutoFlush = true
                };
                writer.WriteLine(message);
            }
            catch
            {

            }
        }
    }
}
