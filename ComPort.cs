using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ComTransfer
{
    public class ComPort : CustomINotifyPropertyChanged
    {
        public string PortInfo => $"COM{PortID} {BaudRate}";
        public int PortStatus { get; set; } = -1;
        public bool Status_Open => PortStatus >= 0;
        public bool Status_CTS => PortStatus >= 0 && (PortStatus & PCOMM.S_CTS) > 0;
        public bool Status_DSR => PortStatus >= 0 && (PortStatus & PCOMM.S_DSR) > 0;
        public bool Status_RI => PortStatus >= 0 && (PortStatus & PCOMM.S_RI) > 0;
        public bool Status_CD => PortStatus >= 0 && (PortStatus & PCOMM.S_CD) > 0;
        public int PortID = 1;
        public int BaudRate = 9600;
        public int DataBits = 8;
        public int StopBits = 1;
        public string Parity = "NONE";
        public bool IsHW = false;
        public bool IsSW = false;
        public bool IsDTR = true;
        public bool IsRTS = true;
        public const int FileKey = 27;
        public List<string> PortOption => directoryDict.Select(item => string.Format("{0}>[{1}]", item.Key, item.Value)).ToList();
        private readonly Dictionary<string, string> directoryDict = new Dictionary<string, string>();
        private const string DefaultDirectory = @"C:\";
        public string SaveDirectory = DefaultDirectory;
        public string SelectedFilePath { get; set; }
        public string PullFilePath { get; set; }
        public const string WorkingDirectory = "files";

        public bool IsOpen { get; set; }
        public bool IsOpening { get; set; }

        public long ReceiveCount { get; set; } = 0;
        public long ReceiveMax { get; set; } = 0;
        public string ReceiveProgress => string.Format("{0}/{1}", ReceiveCount, ReceiveMax);
        public double ReceivePercent => ReceiveMax == 0 ? 0 : (double)ReceiveCount / ReceiveMax;
        public double ReceiveTime => ReceiveMax == 0 || ReceiveMax <= ReceiveCount ? 0 : (double)(ReceiveMax - ReceiveCount) / BaudRate * 8 * 1.7;
        public string ReceiveTimeText => ReceiveTime > 60 ? string.Format("{0:0}分钟{1:0}秒", ReceiveTime / 60, (int)ReceiveTime % 60) : string.Format("{0:0}秒", ReceiveTime);
        public long SendCount { get; set; } = 0;
        public long SendMax { get; set; } = 0;
        public string SendProgress => string.Format("{0}/{1}", SendCount, SendMax);
        public double SendPercent => SendMax == 0 ? 0 : (double)SendCount / SendMax;
        public double SendTime => SendMax == 0 || SendMax <= SendCount ? 0 : (double)(SendMax - SendCount) / BaudRate * 8 * 1.7;
        public string SendTimeText => SendTime > 60 ? string.Format("{0:0}分钟{1:0}秒", SendTime / 60, (int)SendTime % 60) : string.Format("{0:0}秒", SendTime);
        public int SizeLimit = 50;

        public void SetPort()
        {
            int result;
            int baudRate, dataBits, stopBits, parity;
            bool customBaudRate = false;

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
                default: baudRate = PCOMM.B9600; customBaudRate = true; break;
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

            if (customBaudRate)
            {
                if ((result = PCOMM.sio_baud(port, BaudRate)) != PCOMM.SIO_OK)
                {
                    throw new Exception(PCOMM.GetErrorMessage(result));
                }
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

            //PCOMM.sio_SetReadTimeouts(port, 60000, 1000);
            //PCOMM.sio_SetWriteTimeouts(port, 60000);

            if ((result = PCOMM.sio_flush(port, 2)) != PCOMM.SIO_OK)
            {
                throw new Exception(PCOMM.GetErrorMessage(result));
            }
        }

        public bool InitialPort()
        {
            SizeLimit = int.Parse(ConfigurationManager.AppSettings["maxsize"] ?? "50");

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

            InitialDirectory();

            InitialWorkingDirectory();

            PipelineManager.SendCommand(PipelineManager.CommandType.FolderList, string.Join("|", PortOption));

            Notify(new { PortInfo, PortOption });

            return true;
        }
        private void InitialWorkingDirectory()
        {
            string workingfolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, WorkingDirectory);
            if (!Directory.Exists(workingfolder))
            {
                Directory.CreateDirectory(workingfolder);
            }
            File.SetAttributes(workingfolder, FileAttributes.Hidden);
            Directory.SetCurrentDirectory(workingfolder);
        }
        private void InitialDirectory()
        {
            directoryDict.Clear();
            if (SaveDirectory != null)
            {
                foreach (string option in SaveDirectory.Split('|'))
                {
                    string[] options = option.Split('>');
                    if (options.Length >= 2)
                    {
                        directoryDict.Add(options[0].ToUpper(), options[1]);
                    }
                }
            }
            if (!directoryDict.ContainsKey("*"))
            {
                directoryDict.Add("*", DefaultDirectory);
            }
        }
        public string GetDirectory(string extension, string filename)
        {
            string path = GetLocation(filename);
            if (path != null)
            {
                return path;
            }
            if (extension == null)
            {
                return DefaultDirectory;
            }
            if (directoryDict == null)
            {
                return DefaultDirectory;
            }
            else if (!directoryDict.ContainsKey(extension.ToUpper()))
            {
                return directoryDict["*"];
            }
            else
            {
                return directoryDict[extension.ToUpper()];
            }
        }

        public void DelayOpen(int delay)
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(delay);

                OpenPort();
            });
        }


        public bool OpenPort()
        {
            if (IsOpening)
            {
                return false;
            }
            IsOpening = true;
            try
            {
                InitialPort();
            }
            catch (Exception e)
            {
                AddLog("程序故障", "程序配置读取失败：" + e.Message);
                IsOpening = false;
                return false;
            }

            try
            {
                int result;
                if ((result = PCOMM.sio_open(PortID)) != PCOMM.SIO_OK)
                {
                    AddLog("程序故障", "串口通信打开失败：" + PCOMM.GetErrorMessage(result));
                    IsOpening = false;
                    return false;
                }

                SetPort();
            }
            catch (Exception e)
            {
                AddLog("程序故障", "串口通信打开失败：" + e.Message);
                IsOpening = false;
                return false;
            }

            AddLog("操作记录", "串行端口打开：" + PortInfo);

            IsOpen = true;
            IsOpening = false;
            Notify(new { IsOpen, IsOpening });
            StartTask();
            return true;
        }

        public bool ClosePort()
        {
            StopTask();
            int result;
            if ((result = PCOMM.sio_close(PortID)) != PCOMM.SIO_OK)
            {
                AddLog("程序故障", "串口通信关闭失败：" + PCOMM.GetErrorMessage(result));
                return false;
            }

            AddLog("操作记录", "串行端口关闭：" + PortInfo);

            IsOpen = false;
            Notify(new { IsOpen });
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
        public bool IsStarted { get; private set; }
        public bool IsSending { get; private set; }
        public bool IsReceiveWaiting { get; private set; }
        public bool IsReceiving { get; private set; }
        public string CurrentSendingFile { get; private set; }

        public ComPort()
        {
            SendFileList = new ConcurrentQueue<string>();
            ReceiveFileList = new ConcurrentQueue<string>();

            try
            {
                InitialPort();
            }
            catch
            {

            }

            cancellation = new CancellationTokenSource();
            receiveTask = new Task(() =>
            {
                byte[] buffer = new byte[260];
                GCHandle gc_fn = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                IntPtr fnp = gc_fn.AddrOfPinnedObject();

                rCallBack cb = new rCallBack(ReceiveCallback);
                GCHandle gc = GCHandle.Alloc(cb);
                IntPtr cbp = Marshal.GetFunctionPointerForDelegate(cb);

                while (!cancellation.IsCancellationRequested)
                {
                    if (IsStarted && !IsSending)
                    {
                        string filename = string.Empty;
                        int fno = 1;
                        int result = 0;
                        try
                        {
                            result = PCOMM.sio_iqueue(PortID);
                            if (result > 0)
                            {
                                Array.Clear(buffer, 0, buffer.Length);
                                IsReceiveWaiting = true;
                                result = PCOMM.sio_FtZmodemRx(PortID, ref fnp, fno, cbp, FileKey);
                                filename = Encoding.Default.GetString(buffer).TrimEnd('\0');
                                if (!IsStarted)
                                {
                                    continue;
                                }
                                if (IsSending)
                                {
                                    continue;
                                }
                                if (result < 0)
                                {
                                    string message = PCOMM.GetTransferErrorMessage(result);
                                    AddLog("文件接收", "文件接收失败：" + message, filename);
                                }
                                else
                                {
                                    ReceiveFileList.Enqueue(filename);
                                    AddLog("文件接收", "正在处理文件", filename);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            AddLog("文件接收", "文件接收失败：" + e.Message, filename);

                            SendErrorReport(string.Format("文件{1}接收失败：{0}", e.Message, filename));
                        }
                        finally
                        {
                            IsReceiveWaiting = false;

                            ReceiveCount = 0;
                            ReceiveMax = 0;

                            Notify(new { ReceiveCount, ReceiveMax, ReceiveProgress, ReceivePercent, ReceiveTimeText });
                        }
                    }

                    Thread.Sleep(50);
                }

                gc_fn.Free();
                gc.Free();
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
                            string shortname = filename;
                            string targetname = filename;

                            try
                            {
                                FileInfo fileInfo = new FileInfo(filename);
                                shortname = fileInfo.Name;
                                if (fileInfo.Extension.ToUpper() == ".GZ")
                                {
                                    AddLog("文件接收", "正在解压文件", shortname);
                                    using (FileStream originalFileStream = new FileStream(filename, FileMode.Open))
                                    {
                                        string currentFileName = fileInfo.FullName;
                                        string outputFileName = fileInfo.Name.Remove(fileInfo.Name.Length - fileInfo.Extension.Length);
                                        targetname = Path.Combine(GetDirectory(new FileInfo(outputFileName).Extension, outputFileName), fileInfo.Name.Remove(fileInfo.Name.Length - fileInfo.Extension.Length));

                                        using (FileStream decompressedFileStream = File.Create(targetname))
                                        {
                                            using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                                            {
                                                decompressionStream.CopyTo(decompressedFileStream);
                                            }
                                        }
                                    }
                                }
                                else if (fileInfo.Extension.ToUpper() == ".APPCOMMAND")
                                {
                                    AddLog("文件接收", "正在解析指令", shortname);
                                    try
                                    {
                                        string text = DecompressText(File.ReadAllBytes(filename));
                                        ResolveCommand(text);
                                    }
                                    catch
                                    {

                                    }
                                }
                                else
                                {
                                    AddLog("文件接收", "正在拷贝文件", shortname);
                                    targetname = Path.Combine(GetDirectory(fileInfo.Extension, shortname), filename);
                                    File.Copy(filename, targetname, true);
                                }

                                File.Delete(filename);

                                AddLog("文件接收", "文件接收成功", shortname);
                                PipelineManager.SendCommand(PipelineManager.CommandType.FileReceived, targetname);
                                AddTransferRecord(false, targetname);

                                ReceiveHandler?.BeginInvoke(this, new FileSystemEventArgs(WatcherChangeTypes.Created, "", ""), null, null);
                            }
                            catch (Exception e)
                            {
                                AddLog("文件接收", "文件处理失败：" + e.Message, shortname);

                                SendErrorReport(string.Format("文件{1}处理失败：{0}", e.Message, shortname));
                            }
                        }
                    }

                    Thread.Sleep(50);
                }
            }, cancellation.Token, TaskCreationOptions.LongRunning);
            sendTask = new Task(() =>
            {
                xCallBack cb = new xCallBack(SendCallback);
                GCHandle gc = GCHandle.Alloc(cb);
                IntPtr cbp = Marshal.GetFunctionPointerForDelegate(cb);

                while (!cancellation.IsCancellationRequested)
                {
                    if (IsStarted)
                    {
                        if (SendFileList.Count > 0)
                        {
                            string filename = string.Empty;
                            SendFileList.TryDequeue(out filename);
                            string shortname = filename;
                            string sourcename = filename;
                            string tempFolder = null;
                            try
                            {
                                FileInfo fileInfo = new FileInfo(filename);
                                shortname = fileInfo.Name;
                                CurrentSendingFile = shortname;
                                AddLog("文件发送", "检查文件属性", shortname);
                                if (!fileInfo.Exists)
                                {
                                    TaskManager.Instance.FileSendedTaskHandler?.Invoke(this, new TaskManager.FileTaskEventArgs(filename, false));
                                    AddLog("文件发送", "文件不存在", shortname);
                                    SendErrorReport(string.Format("文件不存在：{0}", shortname), filename);
                                    filename = null;
                                }
                                if (fileInfo.Length >= 1024 * 1024 * SizeLimit)
                                {
                                    TaskManager.Instance.FileSendedTaskHandler?.Invoke(this, new TaskManager.FileTaskEventArgs(filename, false));
                                    AddLog("文件发送", "文件体积超过限制", shortname);
                                    SendErrorReport(string.Format("文件体积超过限制：{0}", shortname), filename);
                                    filename = null;
                                }
                                else if(fileInfo.Extension.ToUpper() != ".APPCOMMAND")
                                {
                                    AddLog("文件发送", "正在压缩文件", shortname);
                                    tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
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
                            }
                            catch (Exception e)
                            {
                                TaskManager.Instance.FileSendedTaskHandler?.Invoke(this, new TaskManager.FileTaskEventArgs(filename, false));
                                AddLog("文件发送", "文件处理失败:" + e.Message, shortname);
                                SendErrorReport(string.Format("文件{1}处理失败：{0}", e.Message, shortname), filename);
                                filename = null;
                                CurrentSendingFile = null;
                            }

                            if (filename != null)
                            {
                                AddLog("文件发送", "正在发送文件", shortname);
                                int result;
                                IsSending = true;
                                while (IsReceiveWaiting)
                                {
                                    Thread.Sleep(10);
                                }

                                try
                                {
                                    PCOMM.sio_flush(PortID, 2);
                                    result = PCOMM.sio_FtZmodemTx(PortID, filename, cbp, FileKey);

                                    if (result < 0)
                                    {
                                        string message = PCOMM.GetTransferErrorMessage(result);
                                        TaskManager.Instance.FileSendedTaskHandler?.Invoke(this, new TaskManager.FileTaskEventArgs(filename, false));
                                        AddLog("文件发送", "文件发送失败：" + message, shortname);
                                        SendErrorReport(string.Format("文件{1}发送失败：{0}", message, shortname), filename);
                                    }
                                    else
                                    {
                                        TaskManager.Instance.FileSendedTaskHandler?.Invoke(this, new TaskManager.FileTaskEventArgs(filename, true));
                                        PipelineManager.SendCommand(PipelineManager.CommandType.FileSended, sourcename);
                                        AddLog("文件发送", "文件发送成功", shortname);
                                        AddTransferRecord(true, sourcename);
                                    }
                                }
                                catch (Exception e)
                                {
                                    TaskManager.Instance.FileSendedTaskHandler?.Invoke(this, new TaskManager.FileTaskEventArgs(filename, false));
                                    AddLog("文件发送", "文件发送失败：" + e.Message, shortname);
                                    SendErrorReport(string.Format("文件{1}发送失败：{0}", e.Message, shortname), filename);
                                }
                                finally
                                {
                                    IsSending = false;
                                    CurrentSendingFile = null;

                                    try
                                    {
                                        File.Delete(filename);
                                        if (tempFolder != null)
                                        {
                                            Directory.Delete(tempFolder, true);
                                        }
                                    }
                                    catch
                                    {

                                    }

                                    SendCount = 0;
                                    SendMax = 0;

                                    Notify(new { SendCount, SendMax, SendProgress, SendPercent, SendTimeText });
                                }

                            }
                        }
                    }

                    Thread.Sleep(50);
                }

                gc.Free();
            }, cancellation.Token, TaskCreationOptions.LongRunning);
            statusTask = new Task(() =>
            {
                while (!cancellation.IsCancellationRequested)
                {
                    int result = PCOMM.sio_lstatus(PortID);
                    PortStatus = result;

                    Notify(new { PortStatus, Status_Open, Status_DSR, Status_CTS, Status_RI, Status_CD });

                    PipelineManager.SendCommand(PipelineManager.CommandType.ConnectParam, string.Format("COM{0},{1}", PortID, BaudRate));
                    PipelineManager.SendCommand(PipelineManager.CommandType.ConnectState, string.Join(",", Status_Open, Status_DSR, Status_CTS, Status_RI, Status_CD));

                    Thread.Sleep(500);
                }
            }, cancellation.Token, TaskCreationOptions.LongRunning);
        }

        private bool IsForceStopSending;
        private bool IsForceStopReceiving;

        public void ForceStopSending()
        {
            IsForceStopSending = true;
        }

        public void ForceStopReceiving()
        {
            IsForceStopReceiving = true;
        }

        public delegate int xCallBack(int xmitlen, int buflen, byte[] buf, int flen);
        public delegate int rCallBack(int recvlen, int buflen, byte[] buf, int flen);

        private int ReceiveCallback(int recvlen, int buflen, byte[] buf, int flen)
        {
            if (IsSending)
            {
                return -1;
            }

            if (IsStarted && !IsForceStopReceiving)
            {
                if (ReceiveCount == 0 && recvlen != 0)
                {
                    //AddLog("文件接收", "开始接收文件");
                }
                if (recvlen != 0)
                {
                    IsReceiving = true;
                }
                else
                {
                    IsReceiving = false;
                }

                ReceiveCount = recvlen;
                ReceiveMax = flen;

                PipelineManager.SendCommand(PipelineManager.CommandType.DownloadProgress, string.Format("{0}/{1}", recvlen, flen));

                Notify(new { ReceiveCount, ReceiveMax, ReceiveProgress, ReceivePercent, ReceiveTimeText });

                return 0;
            }
            else
            {
                IsForceStopReceiving = false;

                ReceiveCount = 0;
                ReceiveMax = 0;

                Notify(new { ReceiveCount, ReceiveMax, ReceiveProgress, ReceivePercent, ReceiveTimeText });

                return -1;
            }
        }

        private int SendCallback(int xmitlen, int buflen, byte[] buf, int flen)
        {
            if (IsStarted && !IsForceStopSending)
            {
                SendCount = xmitlen;
                SendMax = flen;

                if (!CurrentSendingFile.ToUpper().EndsWith(".APPCOMMAND"))
                {
                    PipelineManager.SendCommand(PipelineManager.CommandType.UploadProgress, string.Format("{0}/{1}", xmitlen, flen));
                }

                Notify(new { SendCount, SendMax, SendProgress, SendPercent, SendTimeText });

                return 0;
            }
            else
            {
                IsForceStopSending = false;

                SendCount = 0;
                SendMax = 0;

                Notify(new { SendCount, SendMax, SendProgress, SendPercent, SendTimeText });

                return -1;
            }
        }

        private void FileTaskHandler(object sender, TaskManager.FileTaskEventArgs e)
        {
            AddLog("计划任务", "开始准备发送", e.File);
            if (e.Message != null)
            {
                AddLog("计划任务", e.Message, e.File);
            }
            SendFile(e.File);
        }

        private void StartTask()
        {
            IsStarted = true;

            TaskManager.Instance.FileTaskHandler += FileTaskHandler;

            try
            {
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
            catch (Exception e)
            {
                AddLog("程序故障", "线程启动失败：" + e.Message);
            }
        }

        private void StopTask()
        {
            TaskManager.Instance.FileTaskHandler -= FileTaskHandler;

            IsStarted = false;
            IsSending = false;
            IsReceiveWaiting = false;
            IsReceiving = false;
        }

        public void Dispose()
        {
            cancellation.Cancel();
        }

        public void SelectRemotePath(string filename)
        {
            PullFilePath = filename;
            Notify(new { PullFilePath });
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

        private bool IsCommandBusy;

        public void SendErrorReport(string message, string filename = null)
        {
            if (message==null || message.ToUpper().EndsWith(".APPCOMMAND"))
            {
                return;
            }
            if (filename != null && filename != LastFetchFile)
            {
                return;
            }
            SubmitCommand("errorreport", message);
        }

        public void SubmitCommand(string command, string param)
        {
            if (command == null || command.Trim().Length == 0)
            {
                return;
            }
            if (param == null)
            {
                return;
            }

            if (IsCommandBusy)
            {
                return;
            }

            IsCommandBusy = true;

            string commandType;

            switch (command)
            {
                case "requestfile":
                    commandType = "查询文件目录";
                    break;
                case "responsefile":
                    commandType = "发送文件目录";
                    break;
                case "fetch":
                    commandType = "拉取文件";
                    break;
                case "setlocation":
                    commandType = "推送文件";
                    break;
                case "errorreport":
                    commandType = "错误报告";
                    break;
                default:
                    commandType = "未知";
                    break;
            }

            AddLog("命令发送", string.Format("向远程发送 {0} 操作命令", commandType) );

            try
            {
                string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempFolder);
                string filename = Path.Combine(tempFolder, DateTime.Now.ToString("yyyyMMddHHmmss") + ".APPCOMMAND");
                byte[] data = CompressTextToBytes(string.Format("{0} {1}", command, param));

                File.WriteAllBytes(filename, data);

                SendFile(filename);
            }
            catch (Exception e)
            {
                AddLog("命令发送", "命令发送失败：" + e.Message);
            }

            IsCommandBusy = false;
        }

        public string LastCommand;
        private string LastFetchFile;
        private string setLocation;
        private DateTime setLocationTime;

        public string GetLocation(string filename)
        {
            if (setLocation == null || filename == null || DateTime.Now - setLocationTime > TimeSpan.FromMinutes(20) || !setLocation.EndsWith(filename))
            {
                return null;
            }
            string location = setLocation;
            ClearLocation();
            return location.Substring(0, location.Length - filename.Length);
        }

        public void SetLocation(string path)
        {
            if (path.StartsWith("[") && path.Contains(@"]"))
            {
                return;
            }
            setLocation = path;
            setLocationTime = DateTime.Now;
        }

        public void ClearLocation()
        {
            setLocation = null;
        }

        private void ResolveCommand(string command)
        {
            LastCommand = command;
            if (command == null)
            {
                return;
            }
            else if (command.StartsWith("fetch"))
            {
                string filename = command.Substring(5).Trim();
                LastFetchFile = filename;
                SendFile(filename);
            }
            else if (command.StartsWith("requestfile"))
            {
                string root = command.Substring(11).Trim();
                Task.Factory.StartNew(() => SubmitCommand("responsefile", GetFileTreeText(root)));
            }
            else if (command.StartsWith("responsefile"))
            {
                string result = command.Substring(12).Trim();
                PipelineManager.SendCommand(PipelineManager.CommandType.FileTreeResponse, result);
            }
            else if (command.StartsWith("setlocation"))
            {
                SetLocation(command.Substring(11).Trim());
            }
            else if (command.StartsWith("errorreport"))
            {
                string message = command.Substring(11).Trim();
                AddLog("远程错误", message);
            }
        }

        private string CompressText(string text)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Compress))
                    {
                        byte[] data = Encoding.Default.GetBytes(text);
                        deflateStream.Write(data, 0, data.Length);
                    }
                    return Encoding.Default.GetString(stream.ToArray());
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private byte[] CompressTextToBytes(string text)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Compress))
                    {
                        byte[] data = Encoding.Default.GetBytes(text);
                        deflateStream.Write(data, 0, data.Length);
                    }
                    return stream.ToArray();
                }
            }
            catch
            {
                return null;
            }
        }

        private string DecompressText(byte[] data)
        {
            try
            {
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (MemoryStream inputStream = new MemoryStream(data))
                    {
                        using (DeflateStream deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                        {
                            deflateStream.CopyTo(outputStream);
                            return Encoding.Default.GetString(outputStream.ToArray());
                        }
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private string DecompressText(string text)
        {
            try
            {
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (MemoryStream inputStream = new MemoryStream(Encoding.Default.GetBytes(text)))
                    {
                        using (DeflateStream deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                        {
                            deflateStream.CopyTo(outputStream);
                            return Encoding.Default.GetString(outputStream.ToArray());
                        }
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private string GetFileTreeText(string root)
        {
            try
            {
                if (root == null || root.Trim() == "")
                {
                    return "|" + string.Join("|", DriveInfo.GetDrives().Select(item => string.Format("D>[{0}]{1}", item.IsReady ? item.TotalSize : 0, item.Name)));
                }
                int count = 0;
                int rootLength = root.Length;
                StringBuilder stringBuilder = new StringBuilder(root + "|");
                foreach (string entry in Directory.EnumerateDirectories(root))
                {
                    FileInfo info = new FileInfo(entry);

                    if (info.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        continue;
                    }

                    stringBuilder.Append("F>");
                    stringBuilder.Append("[");
                    stringBuilder.Append(info.LastWriteTimeUtc);
                    stringBuilder.Append("]");
                    stringBuilder.Append(entry.Substring(rootLength).TrimStart('\\'));
                    stringBuilder.Append("|");

                    count += 1;
                    if (count >= 1000)
                    {
                        break;
                    }
                }
                foreach (string entry in Directory.EnumerateFiles(root))
                {
                    FileInfo info = new FileInfo(entry);

                    if (info.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        continue;
                    }

                    stringBuilder.Append("[");
                    stringBuilder.Append(info.Length);
                    stringBuilder.Append("]");
                    stringBuilder.Append("[");
                    stringBuilder.Append(info.LastWriteTimeUtc);
                    stringBuilder.Append("]");
                    stringBuilder.Append(entry.Substring(rootLength).TrimStart('\\'));
                    stringBuilder.Append("|");

                    count += 1;
                    if (count >= 1000)
                    {
                        break;
                    }
                }
                return stringBuilder.ToString();
            }
            catch
            {
            }
            return string.Empty;
        }

        private const int LogLimit = 200;
        public ObservableCollection<string> LogList { get; set; } = new ObservableCollection<string>();
        private const int RecordLimit = 100;
        public ObservableCollection<string> RecordList { get; set; } = new ObservableCollection<string>();
        
        public void AddSimpleLog(string message)
        {
            try
            {
                Application.Current.Dispatcher?.Invoke((Action)(() =>
                {
                    LogList.Add(message);

                    while (LogList.Count > LogLimit)
                    {
                        LogList.RemoveAt(0);
                    }
                }));
            }
            catch
            {

            }
        }

        public void AddLog(string brief, string message, string filename = null)
        {
            if (filename != null && filename.ToUpper().EndsWith(".GZ"))
            {
                filename = filename.Remove(filename.Length - 3);
            }
            if (filename != null && filename.ToUpper().EndsWith(".APPCOMMAND"))
            {
                return;
            }
            string log = filename == null || filename.Length == 0 ? string.Format("[{0}] {1} {2}", DateTime.Now.ToString("MM-dd HH:mm:ss"), brief, message) : string.Format("[{0}] {1} {2} 文件：{3}", DateTime.Now.ToString("MM-dd HH:mm:ss"), brief, message, filename);

            try
            {
                Application.Current.Dispatcher?.Invoke((Action)(() =>
                {
                    LogList.Add(log);

                    while (LogList.Count > LogLimit)
                    {
                        LogList.RemoveAt(0);
                    }
                }));

                LogHelper.PushLog("SYSTEM", log);

                PipelineManager.SendCommand(PipelineManager.CommandType.WorkingLog, message);
            }
            catch
            {

            }
        }

        public void AddTransferRecord(bool isSend, string filename)
        {
            if (filename != null && filename.ToUpper().EndsWith(".APPCOMMAND"))
            {
                return;
            }
            string message = string.Format("{0}文件：{1}", isSend ? "已发送" : "已接收", filename);
            string record = string.Format("[{0}] {1}", DateTime.Now.ToString("MM-dd HH:mm:ss"), message);

            try
            {
                Application.Current.Dispatcher?.Invoke((Action)(() =>
                {
                    RecordList.Add(record);

                    while (RecordList.Count > RecordLimit)
                    {
                        RecordList.RemoveAt(0);
                    }
                }));

                LogHelper.PushLog("RECORD", record);

                PipelineManager.SendCommand(PipelineManager.CommandType.TransferLog, message);
            }
            catch
            {

            }
        }

        public void ClearLog()
        {
            LogList.Clear();
        }

        public void ClearRecord()
        {
            RecordList.Clear();
        }
    }
}
