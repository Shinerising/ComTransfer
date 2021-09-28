using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;

namespace ComTransfer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void StartSendTask()
        {
            PCOMM.xCallBack xCallBack = new PCOMM.xCallBack((long xmitlen, int buflen, byte[] buf, long flen) =>
            {
                Console.WriteLine(string.Format("Send:{0}/{1}", xmitlen, flen));
                return 0;
            }
            );

            Task.Factory.StartNew(() =>
            {
                string fileName = @"D:\clmq.dll";
                string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempFolder);
                FileInfo fileInfo = new FileInfo(fileName);
                string compressedFileName = Path.Combine(tempFolder, fileInfo.Name + ".gz");
                using (FileStream originalFileStream = new FileStream(fileName, FileMode.Open))
                {
                    using (FileStream compressedFileStream = File.Create(compressedFileName))
                    {
                        using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                        {
                            originalFileStream.CopyTo(compressionStream);
                        }
                    }
                }
                fileName = compressedFileName;

                PCOMM.sio_open(1);
                PCOMM.sio_ioctl(1, PCOMM.B115200, PCOMM.P_NONE | PCOMM.BIT_8 | PCOMM.STOP_1);
                PCOMM.sio_flowctrl(1, 0 | 0);
                PCOMM.sio_DTR(1, 1);
                PCOMM.sio_RTS(1, 1);
                PCOMM.sio_flush(1, 2);
                int result = PCOMM.sio_FtZmodemTx(1, fileName, xCallBack, 27);
                if (result < 0)
                {
                    var a = 1;
                }
            });
        }

        public void StartReceiveTask()
        {
            PCOMM.rCallBack rCallBack = new PCOMM.rCallBack((long recvlen, int buflen, byte[] buf, long flen) =>
            {
                Console.WriteLine(string.Format("Receive:{0}/{1}", recvlen, flen));
                return 0;
            }
            );

            Task.Factory.StartNew(() =>
            {
                string path = "";
                string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                tempFolder = @"D:\test";
                Directory.CreateDirectory(tempFolder);
                Directory.SetCurrentDirectory(tempFolder);
                int fno = 1;
                int comport = 2;
                PCOMM.sio_open(comport);
                PCOMM.sio_ioctl(comport, PCOMM.B115200, PCOMM.P_NONE | PCOMM.BIT_8 | PCOMM.STOP_1);
                PCOMM.sio_flowctrl(comport, 0 | 0);
                PCOMM.sio_DTR(comport, 1);
                PCOMM.sio_RTS(comport, 1);
                PCOMM.sio_flush(comport, 2);
                int result = PCOMM.sio_FtZmodemRx(comport, ref path, fno, rCallBack, 27);
                if (result < 0)
                {
                    var a = 1;
                }

                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Extension.ToUpper() == ".GZ")
                {
                    using (FileStream originalFileStream = new FileStream(path, FileMode.Open))
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
            });
        }

        private void Button_StartSend_Click(object sender, RoutedEventArgs e)
        {
            StartSendTask();
        }

        private void Button_StartReceive_Click(object sender, RoutedEventArgs e)
        {
            StartReceiveTask();
        }
    }
    public class ComPort
    {
        public string PortInfo => $"COM{PortID},{BaudRate},{DataBits},{StopBits},{Parity},RTS/CTS:{IsHW},XON/XOFF:{IsSW},RTS:{IsDTR},DTR:{IsRTS}";
        public string PortStatus => $"";
        public int PortID = 1;
        public int BaudRate = 9600;
        public int DataBits = 8;
        public int StopBits = 1;
        public string Parity = "NONE";
        public bool IsHW = false;
        public bool IsSW = false;
        public bool IsDTR = false;
        public bool IsRTS = false;

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
                case 5:dataBits = PCOMM.BIT_5;break;
                case 6: dataBits = PCOMM.BIT_6; break;
                case 7: dataBits = PCOMM.BIT_7; break;
                case 8: dataBits = PCOMM.BIT_8; break;
                default:dataBits = PCOMM.BIT_8;break;
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

            if((result = PCOMM.sio_ioctl(port, baudRate, mode)) != PCOMM.SIO_OK)
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
                if ((result = PCOMM.sio_RTS(port, IsDTR ? 1 : 0)) != PCOMM.SIO_OK)
                {
                    throw new Exception(PCOMM.GetErrorMessage(result));
                }
            }

            return true;
        }
    }
}
