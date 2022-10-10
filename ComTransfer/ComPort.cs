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
using System.Windows.Input;

namespace ComTransfer
{
    /// <summary>
    /// 用于管理串口通信过程的ViewModel
    /// </summary>
    public class ComPort : CustomINotifyPropertyChanged
    {
        /// <summary>
        /// 串口参数信息
        /// </summary>
        public string PortInfo => $"COM{PortID} {BaudRate}";
        /// <summary>
        /// 串口状态
        /// </summary>
        public int PortStatus { get; set; } = -1;
        /// <summary>
        /// 串口打开状态
        /// </summary>
        public bool Status_Open => PortStatus >= 0;
        /// <summary>
        /// 串口CTS状态
        /// </summary>
        public bool Status_CTS => PortStatus >= 0 && (PortStatus & PCOMM.S_CTS) > 0;
        /// <summary>
        /// 串口DSR状态
        /// </summary>
        public bool Status_DSR => PortStatus >= 0 && (PortStatus & PCOMM.S_DSR) > 0;
        /// <summary>
        /// 串口RI状态
        /// </summary>
        public bool Status_RI => PortStatus >= 0 && (PortStatus & PCOMM.S_RI) > 0;
        /// <summary>
        /// 串口CD状态
        /// </summary>
        public bool Status_CD => PortStatus >= 0 && (PortStatus & PCOMM.S_CD) > 0;

        /// <summary>
        /// 串口号
        /// </summary>
        public int PortID = 1;
        /// <summary>
        /// 波特率
        /// </summary>
        public int BaudRate = 9600;
        /// <summary>
        /// 数据位数
        /// </summary>
        public int DataBits = 8;
        /// <summary>
        /// 停止位数
        /// </summary>
        public int StopBits = 1;
        /// <summary>
        /// 纠错方式
        /// </summary>
        public string Parity = "NONE";
        /// <summary>
        /// HW标志
        /// </summary>
        public bool IsHW = false;
        /// <summary>
        /// DW标志
        /// </summary>
        public bool IsSW = false;
        /// <summary>
        /// DTR标志
        /// </summary>
        public bool IsDTR = true;
        /// <summary>
        /// RTS标志
        /// </summary>
        public bool IsRTS = true;
        /// <summary>
        /// 文件标记
        /// </summary>
        public const int FileKey = 27;
        /// <summary>
        /// 串口文件保存地址设置
        /// </summary>
        public List<string> PortOption => directoryDict.Select(item => string.Format("{0}>[{1}]", item.Key, item.Value)).ToList();
        /// <summary>
        /// 文件保存地址字典
        /// </summary>
        private readonly Dictionary<string, string> directoryDict = new Dictionary<string, string>();
        /// <summary>
        /// 默认文件存储目录
        /// </summary>
        private const string DefaultDirectory = @"C:\";
        /// <summary>
        /// 文件保存地址
        /// </summary>
        public string SaveDirectory = DefaultDirectory;
        /// <summary>
        /// 待发送文件地址
        /// </summary>
        public string SelectedFilePath { get; set; }
        /// <summary>
        /// 待拉取拉取地址
        /// </summary>
        public string PullFilePath { get; set; }
        /// <summary>
        /// 工作目录
        /// </summary>
        public const string WorkingDirectory = "files";

        /// <summary>
        /// 串口是否已打开
        /// </summary>
        public bool IsOpen { get; set; }
        /// <summary>
        /// 是否正在启动串口通信
        /// </summary>
        public bool IsOpening { get; set; }

        /// <summary>
        /// 数据接收计数
        /// </summary>
        public long ReceiveCount { get; set; } = 0;
        /// <summary>
        /// 数据接收总数
        /// </summary>
        public long ReceiveMax { get; set; } = 0;
        /// <summary>
        /// 数据接收进度文本
        /// </summary>
        public string ReceiveProgress => string.Format("{0}/{1}", ReceiveCount, ReceiveMax);
        /// <summary>
        /// 数据接收百分比
        /// </summary>
        public double ReceivePercent => ReceiveMax == 0 ? 0 : (double)ReceiveCount / ReceiveMax;
        /// <summary>
        /// 数据接收剩余时间
        /// </summary>
        public double ReceiveTime => ReceiveMax == 0 || ReceiveMax <= ReceiveCount ? 0 : (double)(ReceiveMax - ReceiveCount) / BaudRate * 8 * 1.7;
        /// <summary>
        /// 数据接收剩余事件文本
        /// </summary>
        public string ReceiveTimeText => ReceiveTime > 60 ? string.Format("{0:0}分钟{1:0}秒", ReceiveTime / 60, (int)ReceiveTime % 60) : string.Format("{0:0}秒", ReceiveTime);
        public long SendCount { get; set; } = 0;
        public long SendMax { get; set; } = 0;
        public string SendProgress => string.Format("{0}/{1}", SendCount, SendMax);
        public double SendPercent => SendMax == 0 ? 0 : (double)SendCount / SendMax;
        public double SendTime => SendMax == 0 || SendMax <= SendCount ? 0 : (double)(SendMax - SendCount) / BaudRate * 8 * 1.7;
        public string SendTimeText => SendTime > 60 ? string.Format("{0:0}分钟{1:0}秒", SendTime / 60, (int)SendTime % 60) : string.Format("{0:0}秒", SendTime);
        public int SizeLimit = 50;

        /// <summary>
        /// 设置串口基本参数
        /// </summary>
        /// <exception cref="Exception">串口参数设置异常</exception>
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

        /// <summary>
        /// 初始化串口设置
        /// </summary>
        /// <returns>是否初始化成功</returns>
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

            PipelineManager.SendCommand(PipelineManager.CommandType.FolderList, GetDirectoryInfo());

            Notify(new { PortInfo, PortOption });

            return true;
        }
        public void RestartPort(int id, int baudrate)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings["com"].Value = id.ToString();
            configuration.AppSettings.Settings["baudrate"].Value = baudrate.ToString();
            configuration.Save(ConfigurationSaveMode.Minimal, true);
            ConfigurationManager.RefreshSection("appSettings");

            if (IsOpen)
            {
                ClosePort();
            }
            OpenPort();
        }
        /// <summary>
        /// 初始化工作目录
        /// </summary>
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
        /// <summary>
        /// 初始化文件保存地址
        /// </summary>
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

        /// <summary>
        /// 获取文件保存位置信息
        /// </summary>
        /// <returns>文件位置信息文本</returns>
        public string GetDirectoryInfo()
        {
            return string.Join("|", directoryDict.Select(item => string.Format("{0}>{1}", item.Key, item.Value)));
        }
        /// <summary>
        /// 根据文件扩展名获取文件保存地址
        /// </summary>
        /// <param name="extension">文件扩展名</param>
        /// <param name="filename">文件名</param>
        /// <returns>文件保存地址</returns>
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

        /// <summary>
        /// 延迟一段时间后打开串口通信
        /// </summary>
        /// <param name="delay">延迟时间</param>
        /// <param name="wait">重试等待时间</param>
        /// <param name="retry">重试次数</param>
        public void DelayOpen(int delay, int wait = 1000, int retry = 0)
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(delay);

                if (IsOpen)
                {
                    return;
                }

                AddLog("操作记录", "正在自动打开串行端口：" + PortInfo);
                OpenPort();

                while (!IsOpen && retry > 0)
                {
                    AddLog("操作记录", string.Format("串口打开失败，{0}毫秒后自动重试", wait));

                    Thread.Sleep(wait);

                    AddLog("操作记录", "正在重试打开串行端口：" + PortInfo);
                    OpenPort();

                    retry -= 1;
                }
            });
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns>是否成功打开</returns>
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

        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <returns>是否成功关闭</returns>
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

        /// <summary>
        /// 发送任务缓冲数量
        /// </summary>
        private const int BufferSize = 100;
        /// <summary>
        /// 数据接收任务
        /// </summary>
        private readonly Task receiveTask;
        /// <summary>
        /// 数据发送任务
        /// </summary>
        private readonly Task sendTask;
        /// <summary>
        /// 文件处理任务
        /// </summary>
        private readonly Task fileTask;
        /// <summary>
        /// 状态监视任务
        /// </summary>
        private readonly Task statusTask;
        /// <summary>
        /// 任务退出标记
        /// </summary>
        private readonly CancellationTokenSource cancellation;
        /// <summary>
        /// 文件发送列表
        /// </summary>
        private readonly ConcurrentQueue<string> SendFileList;
        /// <summary>
        /// 文件接收列表
        /// </summary>
        private readonly ConcurrentQueue<string> ReceiveFileList;
        /// <summary>
        /// 文件接收处理器
        /// </summary>
        public EventHandler<FileSystemEventArgs> ReceiveHandler;
        /// <summary>
        /// 任务是否已启动
        /// </summary>
        public bool IsStarted { get; private set; }
        /// <summary>
        /// 是否正在发送文件
        /// </summary>
        public bool IsSending { get; private set; }
        /// <summary>
        /// 是否正在等待接收文件
        /// </summary>
        public bool IsReceiveWaiting { get; private set; }
        /// <summary>
        /// 是否正在接收文件
        /// </summary>
        public bool IsReceiving { get; private set; }
        /// <summary>
        /// 正在发送的文件名称
        /// </summary>
        public string CurrentSendingFile { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
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
                            IsReceiving = false;
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

        /// <summary>
        /// 是否执行强制终止发送指令
        /// </summary>
        private bool IsForceStopSending;
        /// <summary>
        /// 是否执行强制终止接收指令
        /// </summary>
        private bool IsForceStopReceiving;

        /// <summary>
        /// 强制终止文件发送
        /// </summary>
        public void ForceStopSending()
        {
            IsForceStopSending = true;
        }

        /// <summary>
        /// 强制终止文件接收
        /// </summary>
        public void ForceStopReceiving()
        {
            IsForceStopReceiving = true;
        }

        /// <summary>
        /// 数据发送回调委托
        /// </summary>
        /// <param name="xmitlen">已发送数据长度</param>
        /// <param name="buflen">当前时间片段数据长度</param>
        /// <param name="buf">当前时间片段数据内容</param>
        /// <param name="flen">发送数据总长度</param>
        /// <returns></returns>
        public delegate int xCallBack(int xmitlen, int buflen, byte[] buf, int flen);
        /// <summary>
        /// 数据接收回调委托
        /// </summary>
        /// <param name="recvlen">已接收数据长度</param>
        /// <param name="buflen">当前时间片段数据长度</param>
        /// <param name="buf">当前时间片段数据内容</param>
        /// <param name="flen">接收数据总长度</param>
        public delegate int rCallBack(int recvlen, int buflen, byte[] buf, int flen);

        /// <summary>
        /// 数据接收回调
        /// </summary>
        /// <param name="recvlen">已接收数据长度</param>
        /// <param name="buflen">当前时间片段数据长度</param>
        /// <param name="buf">当前时间片段数据内容</param>
        /// <param name="flen">接收数据总长度</param>
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

        /// <summary>
        /// 数据发送回调
        /// </summary>
        /// <param name="xmitlen">已发送数据长度</param>
        /// <param name="buflen">当前时间片段数据长度</param>
        /// <param name="buf">当前时间片段数据内容</param>
        /// <param name="flen">发送数据总长度</param>
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

        /// <summary>
        /// 计划任务相关文件发送事件处理
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="e">事件参数</param>
        private void FileTaskHandler(object sender, TaskManager.FileTaskEventArgs e)
        {
            AddLog("计划任务", "开始准备发送", e.File);
            if (e.Message != null)
            {
                AddLog("计划任务", e.Message, e.File);
            }
            SendFile(e.File);
        }

        /// <summary>
        /// 启动各个任务
        /// </summary>
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

        /// <summary>
        /// 终止所有任务
        /// </summary>
        private void StopTask()
        {
            TaskManager.Instance.FileTaskHandler -= FileTaskHandler;

            IsStarted = false;
            IsSending = false;
            IsReceiveWaiting = false;
            IsReceiving = false;
        }

        /// <summary>
        /// 释放所有托管资源
        /// </summary>
        public void Dispose()
        {
            cancellation.Cancel();
        }

        /// <summary>
        /// 选择远程文件地址
        /// </summary>
        /// <param name="filename">文件地址</param>
        public void SelectRemotePath(string filename)
        {
            PullFilePath = filename;
            Notify(new { PullFilePath });
        }

        /// <summary>
        /// 选择本地文件地址
        /// </summary>
        /// <param name="filename">文件地址</param>
        public void SelectFile(string filename)
        {
            SelectedFilePath = filename;
            Notify(new { SelectedFilePath });
        }

        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="filename">文件地址</param>
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

        /// <summary>
        /// 是否正在处理命令
        /// </summary>
        private bool IsCommandBusy;

        /// <summary>
        /// 发送错误报告
        /// </summary>
        /// <param name="message">错误信息文本</param>
        /// <param name="filename">文件名</param>
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

        /// <summary>
        /// 发出指令内容
        /// </summary>
        /// <param name="command">指令文本</param>
        /// <param name="param">指令参数</param>
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

        /// <summary>
        /// 最近一次指令文本
        /// </summary>
        public string LastCommand;
        /// <summary>
        /// 最近一次拉取的文件名
        /// </summary>
        private string LastFetchFile;
        /// <summary>
        /// 当前文件存放位置
        /// </summary>
        private string setLocation;
        /// <summary>
        /// 设置当前文件存放位置的时间戳
        /// </summary>
        private DateTime setLocationTime;

        /// <summary>
        /// 获取当前文件保存位置
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns>目标文件地址</returns>
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

        /// <summary>
        /// 设置当前文件保存位置
        /// </summary>
        /// <param name="path">文件目录地址</param>
        public void SetLocation(string path)
        {
            if (path.StartsWith("[") && path.Contains(@"]"))
            {
                return;
            }
            setLocation = path;
            setLocationTime = DateTime.Now;
        }

        /// <summary>
        /// 清除当前文件保存位置
        /// </summary>
        public void ClearLocation()
        {
            setLocation = null;
        }

        /// <summary>
        /// 处理指令文本
        /// </summary>
        /// <param name="command">指令内容</param>
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

        /// <summary>
        /// 压缩字符串
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns>压缩后数据</returns>
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

        /// <summary>
        /// 解压为字符串
        /// </summary>
        /// <param name="data">原始压缩数据</param>
        /// <returns>字符串</returns>
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

        /// <summary>
        /// 获取包含文件目录列表信息的字符串
        /// </summary>
        /// <param name="root">文件目录位置</param>
        /// <returns>数据字符串</returns>
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

        /// <summary>
        /// 工作日志显示上限
        /// </summary>
        private const int LogLimit = 200;
        /// <summary>
        /// 工作日志列表
        /// </summary>
        public ObservableCollection<string> LogList { get; set; } = new ObservableCollection<string>();
        /// <summary>
        /// 传输记录显示上限
        /// </summary>
        private const int RecordLimit = 100;
        /// <summary>
        /// 传输记录列表
        /// </summary>
        public ObservableCollection<string> RecordList { get; set; } = new ObservableCollection<string>();
        
        /// <summary>
        /// 添加一条简单日志内容
        /// </summary>
        /// <param name="message">日志文本</param>
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

        /// <summary>
        /// 添加一条工作日志信息
        /// </summary>
        /// <param name="brief">信息摘要</param>
        /// <param name="message">信息文本</param>
        /// <param name="filename">文件名</param>
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

        /// <summary>
        /// 添加一条文件传输记录
        /// </summary>
        /// <param name="isSend">是否为发送操作（若否表示接收操作）</param>
        /// <param name="filename">文件名称</param>
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

        /// <summary>
        /// 清除工作日志
        /// </summary>
        public void ClearLog()
        {
            LogList.Clear();
        }

        /// <summary>
        /// 清除传输记录
        /// </summary>
        public void ClearRecord()
        {
            RecordList.Clear();
        }
    }
}
