using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace ComTransfer
{
    public class PCOMM
    {
        public const int B50 = 0x00;
        public const int B75 = 0x01;
        public const int B110 = 0x02;
        public const int B134 = 0x03;
        public const int B150 = 0x04;
        public const int B300 = 0x05;
        public const int B600 = 0x06;
        public const int B1200 = 0x07;
        public const int B1800 = 0x08;
        public const int B2400 = 0x09;
        public const int B4800 = 0x0A;
        public const int B7200 = 0x0B;
        public const int B9600 = 0x0C;
        public const int B19200 = 0x0D;
        public const int B38400 = 0x0E;
        public const int B57600 = 0x0F;
        public const int B115200 = 0x10;
        public const int B230400 = 0x11;
        public const int B460800 = 0x12;
        public const int B921600 = 0x13;

        /*	MODE setting		*/
        public const int BIT_5 = 0x00;         /* Word length define	*/
        public const int BIT_6 = 0x01;
        public const int BIT_7 = 0x02;
        public const int BIT_8 = 0x03;

        public const int STOP_1 = 0x00;            /* Stop bits define	*/ 
        public const int STOP_2 = 0x04;
        public const int P_EVEN = 0x18;            /* Parity define	*/
        public const int P_ODD = 0x08;
        public const int P_SPC = 0x38;
        public const int P_MRK = 0x28;
        public const int P_NONE = 0x00;
        /*	MODEM CONTROL settin=g	*/
        public const int C_DTR = 0x01;
        public const int C_RTS = 0x02;

        /*	MODEM LINE STATUS	=*/
        public const int S_CTS = 0x01;
        public const int S_DSR = 0x02;
        public const int S_RI = 0x04;
        public const int S_CD = 0x08;

        /* error code */
        public const int SIO_OK = 0;
        public const int SIO_BADPORT = -1;/* no such port or port not opened */
        public const int SIO_OUTCONTROL = -2;/* can't control the board */
        public const int SIO_NODATA = -4;/* no data to read or no buffer to write */
        public const int SIO_OPENFAIL = -5;    /* no such port or port has be opened */
        public const int SIO_RTS_BY_HW = -6;     /* RTS can't set because H/W flowctrl */
        public const int SIO_BADPARM = -7;/* bad parameter */
        public const int SIO_WIN32FAIL = -8;/* call win32 function fail, please call */
        /* GetLastError to get the error code */
        public const int SIO_BOARDNOTSUPPORT = -9; /* Does not support this board */
        public const int SIO_FAIL = -10;/* PComm function run result fail */
        public const int SIO_ABORTWRITE = -11;/* write has blocked, and user abort write */
        public const int SIO_WRITETIMEOUT = -12;/* write timeoue has happened */

        public static string GetErrorMessage(int error)
        {
            switch (error)
            {
                case SIO_BADPORT:
                    return "串口号错误";
                case SIO_OUTCONTROL:
                    return "设备不兼容";
                case SIO_NODATA:
                    return "无可读数据";
                case SIO_OPENFAIL:
                    return "串口号不可用或已被占用";
                case SIO_RTS_BY_HW:
                    return "无法修改受硬件流控制的串口通信";
                case SIO_BADPARM:
                    return "参数错误";
                case SIO_WIN32FAIL:
                    return "Windows接口调用错误";
                case SIO_BOARDNOTSUPPORT:
                    return "串口命令不支持";
                case SIO_FAIL:
                    return "API调用错误";
                case SIO_ABORTWRITE:
                    return "用户取消数据写入";
                case SIO_WRITETIMEOUT:
                    return "数据写入超时";
                case SIO_OK:
                default:
                    return "工作正常";
            }
        }

        /* file transfer error code */
        public const int SIOFT_OK = 0;
        public const int SIOFT_BADPORT = -1;   /* no such port or port not open */
        public const int SIOFT_TIMEOUT = -2;/* protocol timeout */
        public const int SIOFT_ABORT = -3;/* user key abort */
        public const int SIOFT_FUNC = -4;/* func return abort */
        public const int SIOFT_FOPEN = -5;/* can not open files */
        public const int SIOFT_CANABORT = -6;  /* Ymodem CAN signal abort */
        public const int SIOFT_PROTOCOL = -7;  /* Protocol checking error abort */
        public const int SIOFT_SKIP = -8;/* Zmodem remote skip this send file */
        public const int SIOFT_LACKRBUF = -9;  /* Zmodem Recv-Buff size must >= 2K bytes */
        public const int SIOFT_WIN32FAIL = -10;    /* OS fail */
        /* GetLastError to get the error code */
        public const int SIOFT_BOARDNOTSUPPORT = -11;   /* Does not support board */

        public static string GetTransferErrorMessage(int error)
        {
            switch (error)
            {
                case SIOFT_BADPORT:
                    return "串口号错误";
                case SIOFT_TIMEOUT:
                    return "数据通信超时";
                case SIOFT_ABORT:
                    return "用户取消操作";
                case SIOFT_FUNC:
                    return "强制终止";
                case SIOFT_FOPEN:
                    return "无法访问文件";
                case SIOFT_CANABORT:
                    return "获取到CAN字符，通信已终止";
                case SIOFT_PROTOCOL:
                    return "协议检查失败，通信已终止";
                case SIOFT_SKIP:
                    return "Zmodem协议跳过该文件";
                case SIOFT_LACKRBUF:
                    return "Zmodem协议接收缓冲区长度须大于2KB";
                case SIOFT_WIN32FAIL:
                    return "Windows接口调用错误";
                case SIOFT_BOARDNOTSUPPORT:
                    return "硬件设备不支持";
                case SIOFT_OK:
                default:
                    return "工作正常";
            }
        }

        private const string DLL_NAME = "PCOMM.dll";

        public delegate void pCallback(int port);
        public delegate int xCallBack(int xmitlen, int buflen, byte[] buf, int flen);
        public delegate int rCallBack(int recvlen, int buflen, byte[] buf, int flen);

        [DllImport(DLL_NAME)]
        public static extern int sio_ioctl(int port, int baud, int mode);
        [DllImport(DLL_NAME)]
        public static extern int sio_getch(int port);
        [DllImport(DLL_NAME)]
        public static extern int sio_read(int port, byte[] buf, int len);
        [DllImport(DLL_NAME)]
        public static extern int sio_putch(int port, int term);
        [DllImport(DLL_NAME)]
        public static extern int sio_write(int port, byte[] buf, int len);
        [DllImport(DLL_NAME)]
        public static extern int sio_flush(int port, int func);
        [DllImport(DLL_NAME)]
        public static extern int sio_iqueue(int port);
        [DllImport(DLL_NAME)]
        public static extern int sio_oqueue(int port);
        [DllImport(DLL_NAME)]
        public static extern int sio_lstatus(int port);
        [DllImport(DLL_NAME)]
        public static extern int sio_lctrl(int port, int mode);
        [DllImport(DLL_NAME)]
        public static extern int sio_cnt_irq(int port, pCallback cb, int count);
        [DllImport(DLL_NAME)]
        public static extern int sio_modem_irq(int port, pCallback cb);
        [DllImport(DLL_NAME)]
        public static extern int sio_break_irq(int port, pCallback cb);
        [DllImport(DLL_NAME)]
        public static extern int sio_Tx_empty_irq(int port, pCallback cb);
        [DllImport(DLL_NAME)]
        public static extern int sio_break(int port, int time);
        [DllImport(DLL_NAME)]
        public static extern int sio_break_ex(int port, int time);
        [DllImport(DLL_NAME)]
        public static extern int sio_flowctrl(int port, int mode);
        [DllImport(DLL_NAME)]
        public static extern int sio_Tx_hold(int port);
        [DllImport(DLL_NAME)]
        public static extern int sio_close(int port);
        [DllImport(DLL_NAME)]
        public static extern int sio_open(int port);
        [DllImport(DLL_NAME)]
        public static extern int sio_getbaud(int port);
        [DllImport(DLL_NAME)]
        public static extern int sio_getmode(int port);
        [DllImport(DLL_NAME)]
        public static extern int sio_getflow(int port);
        [DllImport(DLL_NAME)]
        public static extern int sio_DTR(int port, int mode);
        [DllImport(DLL_NAME)]
        public static extern int sio_RTS(int port, int mode);
        [DllImport(DLL_NAME)]
        public static extern int sio_baud(int port, long speed);
        [DllImport(DLL_NAME)]
        public static extern int sio_data_status(int port);
        [DllImport(DLL_NAME)]
        public static extern int sio_term_irq(int port, pCallback cb, char code);
        [DllImport(DLL_NAME)]
        public static extern int sio_linput(int port, byte[] buf, int lne, int term);
        [DllImport(DLL_NAME)]
        public static extern int sio_putb_x(int port, byte[] buf, int len, int tick);
        [DllImport(DLL_NAME)]
        public static extern int sio_putb_x_ex(int port, byte[] buf, int len, int tms);
        [DllImport(DLL_NAME)]
        public static extern int sio_view(int port, byte[] buf, int len);
        [DllImport(DLL_NAME)]
        public static extern int sio_TxLowWater(int port, int size);
        [DllImport(DLL_NAME)]
        public static extern int sio_AbortWrite(int port);
        [DllImport(DLL_NAME)]
        public static extern int sio_SetWriteTimeouts(int port, int TotalTimeouts);
        [DllImport(DLL_NAME)]
        public static extern int sio_GetWriteTimeouts(int port, ref int TotalTimeouts);
        [DllImport(DLL_NAME)]
        public static extern int sio_SetReadTimeouts(int port, int TotalTimeouts, int IntervalTimeouts);
        [DllImport(DLL_NAME)]
        public static extern int sio_GetReadTimeouts(int port, ref int TotalTimeouts, ref int IntervalTimeouts);
        [DllImport(DLL_NAME)]
        public static extern int sio_AbortRead(int port);

        [DllImport(DLL_NAME)]
        public static extern int sio_ActXoff(int port);
        [DllImport(DLL_NAME)]
        public static extern int sio_ActXon(int port);

        [DllImport(DLL_NAME)]
        public static extern int sio_FtASCIITx(int port, string fname, xCallBack cb, int key);
        [DllImport(DLL_NAME)]
        public static extern int sio_FtASCIIRx(int port, string fname, rCallBack cb, int key, int sec);
        [DllImport(DLL_NAME)]
        public static extern int sio_FtXmodemCheckSumTx(int port, string fname, xCallBack cb, int key);
        [DllImport(DLL_NAME)]
        public static extern int sio_FtXmodemCheckSumRx(int port, string fname, rCallBack cb, int key);
        [DllImport(DLL_NAME)]
        public static extern int sio_FtXmodemCRCTx(int port, string fname, xCallBack cb, int key);
        [DllImport(DLL_NAME)]
        public static extern int sio_FtXmodemCRCRx(int port, string fname, rCallBack cb, int key);
        [DllImport(DLL_NAME)]
        public static extern int sio_FtXmodem1KCRCTx(int port, string fname, xCallBack cb, int key);
        [DllImport(DLL_NAME)]
        public static extern int sio_FtXmodem1KCRCRx(int port, string fname, rCallBack cb, int key);
        [DllImport(DLL_NAME)]
        public static extern int sio_FtYmodemTx(int port, string fname, xCallBack cb, int key);
        [DllImport(DLL_NAME)]
        public static extern int sio_FtYmodemRx(int port, ref IntPtr ffname, int fno, rCallBack cb, int key);
        [DllImport(DLL_NAME)]
        public static extern int sio_FtZmodemTx(int port, string fname, IntPtr cb, int key);
        [DllImport(DLL_NAME)]
        public static extern int sio_FtZmodemRx(int port, ref IntPtr ffname, int fno, IntPtr cb, int key);
        [DllImport(DLL_NAME)]
        public static extern int sio_FtKermitTx(int port, string fname, xCallBack cb, int key);
        [DllImport(DLL_NAME)]
        public static extern int sio_FtKermitRx(int port, ref IntPtr ffname, int fno, rCallBack cb, int key);
    }
}
