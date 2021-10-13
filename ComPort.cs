using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.ObjectModel;
using System.Windows;

namespace ComTransfer
{
    public class ComPort : CustomINotifyPropertyChanged
    {
        public string PortInfo => $"COM{PortID},{BaudRate},{DataBits},{StopBits},{Parity},RTS/CTS:{IsHW},XON/XOFF:{IsSW},RTS:{IsRTS},DTR:{IsDTR}";
        public string PortStatus { get; set; }
        public int PortID = 1;
        public int BaudRate = 9600;
        public int DataBits = 8;
        public int StopBits = 1;
        public string Parity = "NONE";
        public bool IsHW = false;
        public bool IsSW = false;
        public bool IsDTR = false;
        public bool IsRTS = false;
        public const int FileKey = 27;
        public string PortOption => "工作文件夹：" + SaveDirectory;
        public string SaveDirectory = @"D:\";
        public string SelectedFilePath { get; set; }

        public bool IsOpen { get; set; }

        public long ReceiveCount = 0;
        public long ReceiveMax = 0;
        public string ReceiveProgress => string.Format("{0}/{1}", ReceiveCount, ReceiveMax);
        public double ReceivePercent => ReceiveMax == 0 ? 0 : (double)ReceiveCount / ReceiveMax;
        public long SendCount = 0;
        public long SendMax = 0;
        public string SendProgress => string.Format("{0}/{1}", SendCount, SendMax);
        public double SendPercent => SendMax == 0 ? 0 : (double)SendCount / SendMax;

        public bool SetPort()
        {
            int result;
            int baudRate, dataBits, stopBits, parity;

            switch (BaudRate)
            {
                case 50: baudRate = PCOMM.B50; break;
                case 75: baudRate = PCOMM.B75; break;
                case 110: baudRate = PCOMM.B110; break;
                case 134: baudRate = PCOMM.B134; break;
                case 150: baudRate = PCOMM.B150; break;
                case 300: baudRate = PCOMM.B300; break;
                case 600: baudRate = PCOMM.B600; break;
                case 1200: baudRate = PCOMM.B1200; break;
                case 1800: baudRate = PCOMM.B1800; break;
                case 2400: baudRate = PCOMM.B2400; break;
                case 4800: baudRate = PCOMM.B4800; break;
                case 7200: baudRate = PCOMM.B7200; break;
                case 9600: baudRate = PCOMM.B9600; break;
                case 19200: baudRate = PCOMM.B19200; break;
                case 38400: baudRate = PCOMM.B38400; break;
                case 57600: baudRate = PCOMM.B57600; break;
                case 115200: baudRate = PCOMM.B115200; break;
                case 230400: baudRate = PCOMM.B230400; break;
                case 460800: baudRate = PCOMM.B460800; break;
                case 921600: baudRate = PCOMM.B921600; break;
                default: baudRate = PCOMM.B9600; break;
            }

            switch (DataBits)
            {
                case 5: dataBits = PCOMM.BIT_5; break;
                case 6: dataBits = PCOMM.BIT_6; break;
                case 7: dataBits = PCOMM.BIT_7; break;
                case 8: dataBits = PCOMM.BIT_8; break;
                default: dataBits = PCOMM.BIT_8; break;
            }

            switch (StopBits)
            {
                case 1: stopBits = PCOMM.STOP_1; break;
                case 2: stopBits = PCOMM.STOP_2; break;
                default: stopBits = PCOMM.STOP_1; break;
            }

            switch (Parity?.ToUpper())
            {
                case "NONE": parity = PCOMM.P_NONE; break;
                case "ODD": parity = PCOMM.P_ODD; break;
                case "EVEN": parity = PCOMM.P_EVEN; break;
                case "MARK": parity = PCOMM.P_MRK; break;
                case "SPACE": parity = PCOMM.P_SPC; break;
                default: parity = PCOMM.P_NONE; break;
            }

            int port = PortID;
            int mode = dataBits | stopBits | parity;
            int hw = IsHW ? 3 : 0;
            int sw = IsSW ? 12 : 0;

            if ((result = PCOMM.sio_ioctl(port, baudRate, mode)) != PCOMM.SIO_OK)
            {
                throw new Exception(PCOMM.GetErrorMessage(result));
            }

            if ((result = PCOMM.sio_flowctrl(port, hw | sw)) != PCOMM.SIO_OK)
            {
                throw new Exception(PCOMM.GetErrorMessage(result));
            }

            if ((result = PCOMM.sio_DTR(port, IsDTR ? 1 : 0)) != PCOMM.SIO_OK)
            {
                throw new Exception(PCOMM.GetErrorMessage(result));
            }

            if (!IsHW)
            {
                if ((result = PCOMM.sio_RTS(port, IsRTS ? 1 : 0)) != PCOMM.SIO_OK)
                {
                    throw new Exception(PCOMM.GetErrorMessage(result));
                }
            }

            if ((result = PCOMM.sio_flush(port, 2)) != PCOMM.SIO_OK)
            {
                throw new Exception(PCOMM.GetErrorMessage(result));
            }

            return true;
        }

        public bool InitialPort()
        {
            PortID = int.Parse(ConfigurationManager.AppSettings["com"]);
            BaudRate = int.Parse(ConfigurationManager.AppSettings["baudrate"]);
            DataBits = int.Parse(ConfigurationManager.AppSettings["databits"]);
            StopBits = int.Parse(ConfigurationManager.AppSettings["stopbits"]);
            Parity = ConfigurationManager.AppSettings["parity"].ToUpper();
            IsHW = ConfigurationManager.AppSettings["ishw"].ToUpper() == "TRUE";
            IsSW = ConfigurationManager.AppSettings["issw"].ToUpper() == "TRUE";
            IsDTR = ConfigurationManager.AppSettings["isdtr"].ToUpper() == "TRUE";
            IsRTS = ConfigurationManager.AppSettings["isrts"].ToUpper() == "TRUE";

            SaveDirectory = ConfigurationManager.AppSettings["directory"];

            return SetPort();
        }

        public bool CheckOption()
        {
            return true;
        }

        public bool OpenPort()
        {
            int result;
            if ((result = PCOMM.sio_open(PortID)) != PCOMM.SIO_OK)
            {
                return false;
            }

            if (!InitialPort())
            {
                return false;
            }

            IsOpen = true;
            Notify(new { IsOpen });
            StartTask();
            return true;
        }

        public bool ClosePort()
        {
            int result;
            if ((result = PCOMM.sio_close(PortID)) != PCOMM.SIO_OK)
            {
                return false;
            }
            IsOpen = false;
            Notify(new { IsOpen });
            StopTask();
            return true;
        }

        private const int BufferSize = 100;
        private readonly Task receiveTask;
        private readonly Task sendTask;
        private readonly Task fileTask;
        private readonly Task statusTask;
        private readonly CancellationTokenSource cancellation;
        private readonly ConcurrentQueue<string> SendFileList;
        private readonly ConcurrentQueue<string> ReceiveFileList;
        public EventHandler<FileSystemEventArgs> ReceiveHandler;
        private bool IsStarted;

        public ComPort()
        {
            SendFileList = new ConcurrentQueue<string>();
            ReceiveFileList = new ConcurrentQueue<string>();

            cancellation = new CancellationTokenSource();
            receiveTask = new Task(() =>
            {
                PCOMM.rCallBack rCallBack = new PCOMM.rCallBack((long recvlen, int buflen, byte[] buf, long flen) =>
                {
                    ReceiveCount = recvlen;
                    ReceiveMax = flen;

                    Notify(new { ReceiveProgress, ReceivePercent });

                    return 0;
                }
                );

                while (!cancellation.IsCancellationRequested)
                {
                    if (IsStarted)
                    {
                        string path = "";
                        int fno = 1;
                        int result = PCOMM.sio_FtZmodemRx(PortID, ref path, fno, rCallBack, FileKey);
                        if (!IsStarted)
                        {
                            continue;
                        }
                        if (result < 0)
                        {
                            var a = 1;
                        }
                        else
                        {
                            ReceiveFileList.Enqueue(path);
                        }
                    }

                    Thread.Sleep(50);
                }
            }, cancellation.Token, TaskCreationOptions.LongRunning);
            fileTask = new Task(() =>
            {
                while (!cancellation.IsCancellationRequested)
                {
                    if (IsStarted)
                    {
                        if (ReceiveFileList.Count > 0)
                        {
                            string filename = string.Empty;
                            ReceiveFileList.TryDequeue(out filename);

                            FileInfo fileInfo = new FileInfo(filename);
                            if (fileInfo.Extension.ToUpper() == ".GZ")
                            {
                                using (FileStream originalFileStream = new FileStream(filename, FileMode.Open))
                                {
                                    string currentFileName = fileInfo.FullName;
                                    string newFileName = currentFileName.Remove(currentFileName.Length - fileInfo.Extension.Length);

                                    using (FileStream decompressedFileStream = File.Create(newFileName))
                                    {
                                        using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                                        {
                                            decompressionStream.CopyTo(decompressedFileStream);
                                        }
                                    }
                                }
                            }

                            ReceiveHandler?.BeginInvoke(this, new FileSystemEventArgs(WatcherChangeTypes.Created, "", ""), null, null);
                        }
                    }

                    Thread.Sleep(50);
                }
            }, cancellation.Token, TaskCreationOptions.LongRunning);
            sendTask = new Task(() =>
            {
                PCOMM.xCallBack xCallBack = new PCOMM.xCallBack((long xmitlen, int buflen, byte[] buf, long flen) =>
                {
                    SendCount = xmitlen;
                    SendMax = flen;

                    Notify(new { SendProgress, SendPercent });

                    return 0;
                }
                );

                while (!cancellation.IsCancellationRequested)
                {
                    if (IsStarted)
                    {
                        if (SendFileList.Count > 0)
                        {
                            string filename = string.Empty;
                            SendFileList.TryDequeue(out filename);

                            try
                            {
                                AddLog("文件发送", "检查文件属性", filename);
                                FileInfo fileInfo = new FileInfo(filename);
                                if (fileInfo.Exists)
                                {
                                    AddLog("文件发送", "正在压缩文件", filename);
                                    string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                                    Directory.CreateDirectory(tempFolder);
                                    string compressedFileName = Path.Combine(tempFolder, fileInfo.Name + ".gz");
                                    using (FileStream originalFileStream = new FileStream(filename, FileMode.Open))
                                    {
                                        using (FileStream compressedFileStream = File.Create(compressedFileName))
                                        {
                                            using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                                            {
                                                originalFileStream.CopyTo(compressionStream);
                                            }
                                        }
                                    }
                                    filename = compressedFileName;
                                }
                                else
                                {
                                    AddLog("文件发送", "文件不存在", filename);
                                    filename = null;
                                }
                            }
                            catch (Exception e)
                            {
                                AddLog("文件发送", "文件处理失败", filename);
                                filename = null;
                            }

                            if (filename != null)
                            {
                                AddLog("文件发送", "正在发送文件", filename);
                                int result = PCOMM.sio_FtZmodemTx(PortID, filename, xCallBack, FileKey);
                                if (result < 0)
                                {
                                    string message = PCOMM.GetTransferErrorMessage(result);
                                    AddLog("文件发送", "文件发送失败", filename);
                                }
                                else
                                {
                                    AddLog("文件发送", "文件发送成功", filename);
                                }
                            }
                        }
                    }

                    Thread.Sleep(50);
                }
            }, cancellation.Token, TaskCreationOptions.LongRunning);
            statusTask = new Task(() =>
            {
                while (!cancellation.IsCancellationRequested)
                {
                    int result = PCOMM.sio_lstatus(PortID);
                    PortStatus = result.ToString();
                    Notify(new { PortStatus });

                    Thread.Sleep(100);
                }
            }, cancellation.Token, TaskCreationOptions.LongRunning);
        }

        private void StartTask()
        {
            IsStarted = true;

            if (receiveTask.Status != TaskStatus.Running)
            {
                receiveTask.Start();
            }

            if (sendTask.Status != TaskStatus.Running)
            {
                sendTask.Start();
            }

            if (fileTask.Status != TaskStatus.Running)
            {
                fileTask.Start();
            }

            if (statusTask.Status != TaskStatus.Running)
            {
                statusTask.Start();
            }
        }

        private void StopTask()
        {
            IsStarted = false;
        }

        public void Dispose()
        {

        }

        public void SelectFile(string filename)
        {
            SelectedFilePath = filename;
            Notify(new { SelectedFilePath });
        }

        public void SendFile(string filename)
        {
            SendFileList.Enqueue(filename);

            AddLog("文件发送", "已添加文件发送任务", filename);

            while (SendFileList.Count > BufferSize)
            {
                bool result = SendFileList.TryDequeue(out filename);

                if (result)
                {
                    AddLog("文件发送", "从缓冲区删除过期文件发送任务", filename);
                }
                else
                {
                    AddLog("文件发送", "删除过期发送任务失败", filename);
                }
            }
        }

        private const int LogLimit = 200;
        public ObservableCollection<string> LogList { get; set; } = new ObservableCollection<string>();

        public void AddLog(string brief, string message, string filename = null)
        {
            string log = filename == null ? string.Format("[{0}] {1} {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), brief, message) : string.Format("[{0}] {1} {2} 文件名:{3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), brief, message, filename);

            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                LogList.Add(log);

                while (LogList.Count > LogLimit)
                {
                    LogList.RemoveAt(0);
                }
            }));
        }

        public void AddTransferRecord(bool isSend, string filename)
        {

        }
    }
}
