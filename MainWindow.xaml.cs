using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace ComTransfer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ComPort port;
        public MainWindow()
        {
            port = new ComPort();

            DataContext = port;

            InitializeComponent();
        }

        public void StartSendTask()
        {
            PCOMM.xCallBack xCallBack = new PCOMM.xCallBack((long xmitlen, int buflen, byte[] buf, long flen) =>
            {
                Console.WriteLine(string.Format("Send:{0}/{1}", xmitlen, flen));
                Thread.Sleep(1);
                return 0;
            }
            );

            PCOMM.sio_open(1);
            PCOMM.sio_ioctl(1, PCOMM.B115200, PCOMM.P_NONE | PCOMM.BIT_8 | PCOMM.STOP_1);
            PCOMM.sio_flowctrl(1, 0 | 0);
            PCOMM.sio_DTR(1, 1);
            PCOMM.sio_RTS(1, 1);
            PCOMM.sio_SetWriteTimeouts(1, 1000000);
            PCOMM.sio_flush(1, 2);

            Task.Factory.StartNew(() =>
            {
                string fileName = @"D:\clmq.dll";
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

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            //StartSendTask();
            //return;
            if (port.IsOpen)
            {
                port.ClosePort();
            }
            else
            {
                port.OpenPort();
            }
        }

        private void Button_Option_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Pull_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Push_Click(object sender, RoutedEventArgs e)
        {
            port.SendFile(port.SelectedFilePath);
        }

        private void Button_Select_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "选择要发送的文件",
                DefaultExt = ".*",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "全部文件|*.*"
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                port.SelectFile(dialog.FileName);
            }
        }
    }
}
