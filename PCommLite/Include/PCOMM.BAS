Attribute VB_Name = "PComm"
'********************************************************
'    PComm.bas
'     -- PComm module for Visual Basic(5.0 above)
'
'    Description:
'       When you want to develop one VB application with PComm,
'       you should add this module to your project.
'
'    History:
'           Date    Author      Comment
'       01/10/1997  Victor      wrote it.
'       06/01/1998  Casper      supprot VB5.0 'AddressOf' operator
'                               (sio_xxx_irq & sio_Ftxxx function)
'
'       04/27/1999  Casper      Update. (add sio_ActXon, sio_ActXoff,
'                                            sio_break_ex)
'       04/14/2000  Casper      sio_xxx_irq not support.
'********************************************************

'Baud Rate Setting
Global Const B50 = &H0
Global Const B75 = &H1
Global Const B110 = &H2
Global Const B134 = &H3
Global Const B150 = &H4
Global Const B300 = &H5
Global Const B600 = &H6
Global Const B1200 = &H7
Global Const B1800 = &H8
Global Const B2400 = &H9
Global Const B4800 = &HA
Global Const B7200 = &HB
Global Const B9600 = &HC
Global Const B19200 = &HD
Global Const B38400 = &HE
Global Const B57600 = &HF
Global Const B115200 = &H10
Global Const B230400 = &H11
Global Const B460800 = &H12
Global Const B921600 = &H13

' Mode setting
Global Const BIT_5 = &H0                 ' Data bits define
Global Const BIT_6 = &H1
Global Const BIT_7 = &H2
Global Const BIT_8 = &H3

Global Const STOP_1 = &H0                ' Stop bits define
Global Const STOP_2 = &H4

Global Const P_EVEN = &H18               ' Parity define
Global Const P_ODD = &H8
Global Const P_SPC = &H38
Global Const P_MRK = &H28
Global Const P_NONE = &H0

' Modem Control setting
Global Const C_DTR = &H1
Global Const C_RTS = &H2

' Modem Line Status
Global Const S_CTS = &H1
Global Const S_DSR = &H2
Global Const S_RI = &H4
Global Const S_CD = &H8

' Error code
Global Const SIO_OK = 0
Global Const SIO_BADPORT = -1       ' No such port or port not opened
Global Const SIO_OUTCONTROL = -2    ' Can't control board
Global Const SIO_NODATA = -4        ' No data to read or no buffer to write
Global Const SIO_OPENFAIL = -5      ' No such port or port has opened
Global Const SIO_RTS_BY_HW = -6     ' RTS can't set by H/W flowctrl
Global Const SIO_BADPARM = -7       ' Bad parameter
Global Const SIO_WIN32FAIL = -8     ' Call win32 function fail, please call
                                    ' GetLastError to get the error code
Global Const SIO_BOARDNOTSUPPORT = -9   'Board does not support this function
Global Const SIO_FAIL = -10         ' PComm function run result fail
Global Const SIO_ABORT_WRITE = -11  ' Write has blocked, and user abort write
Global Const SIO_WRITETIMEOUT = -12 ' Write timeoue has happened

'File transfer error code
Global Const SIOFT_OK = 0
Global Const SIOFT_BADPORT = -1       ' No such port or port not open
Global Const SIOFT_TIMEOUT = -2       ' Protocol timeout
Global Const SIOFT_ABORT = -3         ' User key abort
Global Const SIOFT_FUNC = -4          ' Func return abort
Global Const SIOFT_FOPEN = -5         ' Can not open files
Global Const SIOFT_CANABORT = -6      ' Ymodem CAN signal abort
Global Const SIOFT_PROTOCOL = -7      ' Protocol checking error abort
Global Const SIOFT_SKIP = -8          ' Zmodem remote skip this send file
Global Const SIOFT_LACKRBUF = -9      ' Zmodem Recv-Buff size must >= 2K bytes
Global Const SIOFT_WIN32FAIL = -10    ' OS fail
                                      ' GetLastError to get the error code
Global Const SIOFT_BOARDNOTSUPPORT = -11  'Board does not support this function


Declare Function sio_open Lib "PComm.dll" (ByVal port As Long) As Long
Declare Function sio_close Lib "PComm.dll" (ByVal port As Long) As Long
Declare Function sio_ioctl Lib "PComm.dll" (ByVal port As Long, ByVal baud As Long, ByVal mode As Long) As Long
Declare Function sio_flowctrl Lib "PComm.dll" (ByVal port As Long, ByVal mode As Long) As Long
Declare Function sio_flush Lib "PComm.dll" (ByVal port As Long, ByVal func As Long) As Long
Declare Function sio_DTR Lib "PComm.dll" (ByVal port As Long, ByVal mode As Long) As Long
Declare Function sio_RTS Lib "PComm.dll" (ByVal port As Long, ByVal mode As Long) As Long
Declare Function sio_lctrl Lib "PComm.dll" (ByVal port As Long, ByVal mode As Long) As Long
Declare Function sio_baud Lib "PComm.dll" (ByVal port As Long, ByVal speed As Long) As Long
Declare Function sio_getch Lib "PComm.dll" (ByVal port As Long) As Long
Declare Function sio_read Lib "PComm.dll" (ByVal port As Long, ByRef buf As Byte, ByVal length As Long) As Long
Declare Function sio_linput Lib "PComm.dll" (ByVal port As Long, ByRef buf As Byte, ByVal length As Long, ByVal Term As Long) As Long
Declare Function sio_putch Lib "PComm.dll" (ByVal port As Long, ByVal Term As Long) As Long
Declare Function sio_putb Lib "PComm.dll" Alias "sio_write" (ByVal port As Long, ByRef buf As Byte, ByVal length As Long) As Long
Declare Function sio_write Lib "PComm.dll" (ByVal port As Long, ByRef buf As Byte, ByVal length As Long) As Long
Declare Function sio_putb_x Lib "PComm.dll" (ByVal port As Long, ByRef buf As Byte, ByVal length As Long, ByVal tick As Long) As Long
Declare Function sio_putb_x_ex Lib "PComm.dll" (ByVal port As Long, ByRef buf As Byte, ByVal length As Long, ByVal tms As Long) As Long
Declare Function sio_lstatus Lib "PComm.dll" (ByVal port As Long) As Long
Declare Function sio_iqueue Lib "PComm.dll" (ByVal port As Long) As Long
Declare Function sio_oqueue Lib "PComm.dll" (ByVal port As Long) As Long
Declare Function sio_Tx_hold Lib "PComm.dll" (ByVal port As Long) As Long
Declare Function sio_getbaud Lib "PComm.dll" (ByVal port As Long) As Long
Declare Function sio_getmode Lib "PComm.dll" (ByVal port As Long) As Long
Declare Function sio_getflow Lib "PComm.dll" (ByVal port As Long) As Long
Declare Function sio_data_status Lib "PComm.dll" (ByVal port As Long) As Long
'Declare Function sio_term_irq Lib "PComm.dll" (ByVal port As Long, ByVal func As Long, ByVal code As Byte) As Long
'Declare Function sio_cnt_irq Lib "PComm.dll" (ByVal port As Long, ByVal func As Long, ByVal count As Long) As Long
'Declare Function sio_modem_irq Lib "PComm.dll" (ByVal port As Long, ByVal func As Long) As Long
'Declare Function sio_break_irq Lib "PComm.dll" (ByVal port As Long, ByVal func As Long) As Long
'Declare Function sio_Tx_empty_irq Lib "PComm.dll" (ByVal port As Long, ByVal func As Long) As Long
Declare Function sio_break Lib "PComm.dll" (ByVal port As Long, ByVal time As Long) As Long
Declare Function sio_break_ex Lib "PComm.dll" (ByVal port As Long, ByVal time As Long) As Long
Declare Function sio_view Lib "PComm.dll" (ByVal port As Long, ByRef buf As Byte, ByVal length As Long) As Long
Declare Function sio_TxLowWater Lib "PComm.dll" (ByVal port As Long, ByVal size As Long) As Long
Declare Function sio_AbortWrite Lib "PComm.dll" (ByVal port As Long) As Long
Declare Function sio_AbortRead Lib "PComm.dll" (ByVal port As Long) As Long
Declare Function sio_SetWriteTimeouts Lib "PComm.dll" (ByVal port As Long, ByVal timeouts As Long) As Long
Declare Function sio_GetWriteTimeouts Lib "PComm.dll" (ByVal port As Long, ByRef TotalTimeouts As Long) As Long
Declare Function sio_SetReadTimeouts Lib "PComm.dll" (ByVal port As Long, ByVal TotalTimeouts As Long, ByVal IntervalTimeouts As Long) As Long
Declare Function sio_GetReadTimeouts Lib "PComm.dll" (ByVal port As Long, ByRef TotalTimeouts As Long, ByRef InterfalTimeouts As Long) As Long
Declare Function sio_ActXon Lib "PComm.dll" (ByVal port As Long) As Long
Declare Function sio_ActXoff Lib "PComm.dll" (ByVal port As Long) As Long

Declare Function sio_FtASCIITx Lib "PComm.dll" ( _
    ByVal port As Long, ByVal fname As String, ByVal func As Long, ByVal key As Long _
) As Long

Declare Function sio_FtASCIIRx Lib "PComm.dll" ( _
    ByVal port As Long, ByVal fname As String, ByVal func As Long, ByVal key As Long, ByVal sec As Long _
) As Long

Declare Function sio_FtXmodemCheckSumTx Lib "PComm.dll" ( _
    ByVal port As Long, ByVal fname As String, ByVal func As Long, ByVal key As Long _
) As Long

Declare Function sio_FtXmodemCheckSumRx Lib "PComm.dll" ( _
    ByVal port As Long, ByVal fname As String, ByVal func As Long, ByVal key As Long _
) As Long

Declare Function sio_FtXmodemCRCTx Lib "PComm.dll" ( _
    ByVal port As Long, ByVal fname As String, ByVal func As Long, ByVal key As Long _
) As Long

Declare Function sio_FtXmodemCRCRx Lib "PComm.dll" ( _
    ByVal port As Long, ByVal fname As String, ByVal func As Long, ByVal key As Long _
) As Long

Declare Function sio_FtXmodem1KCRCTx Lib "PComm.dll" ( _
    ByVal port As Long, ByVal fname As String, ByVal func As Long, ByVal key As Long _
) As Long

Declare Function sio_FtXmodem1KCRCRx Lib "PComm.dll" ( _
    ByVal port As Long, ByVal fname As String, ByVal func As Long, ByVal key As Long _
) As Long

Declare Function sio_FtYmodemTx Lib "PComm.dll" ( _
    ByVal port As Long, ByVal fname As String, ByVal func As Long, ByVal key As Long _
) As Long

Declare Function sio_FtYmodemRx Lib "PComm.dll" ( _
    ByVal port As Long, ByRef fname As Long, ByVal fno As Long, ByVal func As Long, ByVal key As Long _
) As Long

Declare Function sio_FtZmodemTx Lib "PComm.dll" ( _
    ByVal port As Long, ByVal fname As String, ByVal func As Long, ByVal key As Long _
) As Long

Declare Function sio_FtZmodemRx Lib "PComm.dll" ( _
    ByVal port As Long, ByRef fname As Long, ByVal fno As Long, ByVal func As Long, ByVal key As Long _
) As Long

Declare Function sio_FtKermitTx Lib "PComm.dll" ( _
    ByVal port As Long, ByVal fname As String, ByVal func As Long, ByVal key As Long _
) As Long

Declare Function sio_FtKermitRx Lib "PComm.dll" ( _
    ByVal port As Long, ByRef fname As Long, ByVal fno As Long, ByVal func As Long, ByVal key As Long _
) As Long

