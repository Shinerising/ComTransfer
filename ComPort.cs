using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace ComTransfer
{
    public class ComPort : CustomINotifyPropertyChanged
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

        public bool OpenPort()
        {

            return true;
        }

        private const int BufferSize = 100;
        private readonly Task receiveTask;
        private readonly Task sendTask;
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
                    Console.WriteLine(string.Format("Receive:{0}/{1}", recvlen, flen));
                    return 0;
                }
                );

                while (!cancellation.IsCancellationRequested)
                {
                    if (IsStarted)
                    {
                        string path = "";
                        int fno = 1;
                        int comport = 2;
                        int result = PCOMM.sio_FtZmodemRx(comport, ref path, fno, rCallBack, 27);
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
                    Console.WriteLine(string.Format("Send:{0}/{1}", xmitlen, flen));
                    return 0;
                }
                );

                while (!cancellation.IsCancellationRequested)
                {
                    if (IsStarted)
                    {
                        string fileName = "";
                        int result = PCOMM.sio_FtZmodemTx(1, fileName, xCallBack, 27);
                        if (result < 0)
                        {
                            var a = 1;
                        }
                    }

                    Thread.Sleep(50);
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
        }

        private void StopTask()
        {
            IsStarted = false;
        }

        public void Dispose()
        {

        }

        public void SendFile(string fileName)
        {
            SendFileList.Enqueue(fileName);

            while (SendFileList.Count > BufferSize)
            {
                string unused;
                bool result = SendFileList.TryDequeue(out unused);
            }
        }
    }
}
