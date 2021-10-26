(*********************************************************************
    PComm.pas
     -- PComm Lib unit for Delphi (32 bit version).


    History:   Date       Author         Comment
               5/29/98    Casper         Wrote it.
              12/11/98	  Casper	 Update  			

**********************************************************************)

unit PComm;

interface

const
  { baud rate setting }
  B50 = $0;
  B75 = $1;
  B110 = $2;
  B134 = $3;
  B150 = $4;
  B300 = $5;
  B600 = $6;
  B1200 = $7;
  B1800 = $8;
  B2400 = $9;
  B4800 = $A;
  B7200 = $B;
  B9600 = $C;
  B19200 = $D;
  B38400 = $E;
  B57600 = $F;
  B115200 = $10;
  B230400 = $11;
  B460800 = $12;
  B921600 = $13;

  { data bit }
  BIT_5 = $0;
  BIT_6 = $1;
  BIT_7 = $2;
  BIT_8 = $3;

  { stop bit }
  STOP_1 = $0;
  STOP_2 = $4;

  { parity }
  P_EVEN = $18;
  P_ODD  = $8;
  P_SPC  = $38;
  P_MRK  = $28;
  P_NONE = $0;

  { modem control setting }
  C_DTR = $1;
  C_RTS = $2;

  { modem line status }
  S_CTS = $1;
  S_DSR = $2;
  S_RI  = $4;
  S_CD  = $8;

  { error code }
  SIO_OK           = 0;
  SIO_BADPORT      = -1;  { No such port or port not opened }
  SIO_OUTCONTROL   = -2;  { Can't control board }
  SIO_NODATA       = -4;  { No data to read or no buffer to write }
  SIO_OPENFAIL     = -5;   { No such port or port has opened }
  SIO_RTS_BY_HW    = -6;  { Can't set because H/W flowctrl }
  SIO_BADPARM      = -7;  { Bad parameter }
  SIO_WIN32FAIL    = -8;  (* Call win32 function fail, please call }
                             GetLastError to get the error code *)
  SIO_BOARDNOTSUPPORT  = -9;  { Board does not support this function}
  SIO_FAIL         = -10; { PComm function run result fail }
  SIO_ABORT_WRITE  = -11; { Write has blocked, and user abort write }
  SIO_WRITETIMEOUT = -12; { Write timeout has happened }

  { file transfer error code }
  SIOFT_OK           = 0;
  SIOFT_BADPORT      = -1;	{ No such port or port not open }
  SIOFT_TIMEOUT	     = -2;	{ Protocol timeout }
  SIOFT_ABORT        = -3;	{ User key abort }
  SIOFT_FUNC         = -4;	{ Func return abort }
  SIOFT_FOPEN        = -5;	{ Can not open files }
  SIOFT_CANABORT     = -6;	{ Ymodem CAN signal abort }
  SIOFT_PROTOCOL     = -7;	{ Protocol checking error abort }
  SIOFT_SKIP         = -8;	{ Zmodem remote skip this send file }
  SIOFT_LACKRBUF     = -9;	{ Zmodem Recv-Buff size must >= 2K bytes }
  SIOFT_WIN32FAIL    = -10;	(* OS fail }
				  GetLastError to get the error code *)
  SIOFT_BOARDNOTSUPPORT = -11;	{ Board does not support this function}

type

  IrqProc = procedure(port: Longint);stdcall;
  CallBackProc = function(len: Longint; rlen: Longint; buf: PChar; flen: Longint): Longint;stdcall;

{Import routine from PComm.dll}
function sio_open(port: Longint): Longint; stdcall;
function sio_close(port: Longint): Longint; stdcall;
function sio_ioctl(port, baud, mode: Longint): Longint; stdcall;
function sio_flowctrl(port, mode: Longint): Longint; stdcall;
function sio_flush(port, func: Longint): Longint; stdcall;
function sio_DTR(port, mode: Longint): Longint; stdcall;
function sio_RTS(port, mode: Longint): Longint; stdcall;
function sio_lctrl(port, mode: Longint): Longint; stdcall;
function sio_baud(port, speed: Longint): Longint; stdcall;
function sio_getch(port: Longint): Longint; stdcall;
function sio_read(port: Longint; buf: PChar; len: Longint): Longint; stdcall;
function sio_linput(port: Longint; buf:PChar; len: Longint; term:Longint): Longint; stdcall;
function sio_putch(port, term: Longint): Longint; stdcall;
function sio_putb(port: Longint; buf:PChar; len: Longint): Longint; stdcall;
function sio_write(port: Longint; buf:PChar; len: Longint): Longint; stdcall;
function sio_putb_x(port: Longint; buf:PChar; len: Longint; tick:Longint): Longint; stdcall;
function sio_putb_x_ex(port: Longint; buf:PChar; len: Longint; tms:Longint): Longint; stdcall;
function sio_lstatus(port: Longint): Longint; stdcall;
function sio_iqueue(port: Longint): Longint; stdcall;
function sio_oqueue(port: Longint): Longint; stdcall;
function sio_Tx_hold(port: Longint): Longint; stdcall;
function sio_getbaud(port: Longint): Longint; stdcall;
function sio_getmode(port: Longint): Longint; stdcall;
function sio_getflow(port: Longint): Longint; stdcall;
function sio_data_status(port: Longint): Longint; stdcall;
function sio_term_irq(port: Longint; func: IrqProc; code: Byte): Longint; stdcall;
function sio_cnt_irq(port: Longint; func: IrqProc; count: Longint): Longint; stdcall;
function sio_modem_irq(port: Longint; func: IrqProc): Longint; stdcall;
function sio_break_irq(port: Longint; func: IrqProc): Longint; stdcall;
function sio_Tx_empty_irq(port: Longint; func: IrqProc): Longint; stdcall;
function sio_break(port, time: Longint): Longint; stdcall;
function sio_view(port: Longint; buf: PChar; len: Longint): Longint; stdcall;
function sio_TxLowWater(port, size: Longint): Longint; stdcall;
function sio_AbortWrite(port: Longint): Longint; stdcall;
function sio_AbortRead(port: Longint): Longint; stdcall;
function sio_SetWriteTimeouts(port, timeouts: Longint): Longint; stdcall;
function sio_GetWriteTimeouts(port: Longint; var TotalTimeouts:Longint): Longint; stdcall;
function sio_SetReadTimeouts(port, TotalTimeouts, IntervalTimeouts: Longint): Longint; stdcall;
function sio_GetReadTimeouts(port: Longint; var TotalTimeouts, IntervalTimeouts: Longint): Longint; stdcall;
function sio_FtASCIITx(port:Longint; fname:PChar; func:CallBackProc; key:Longint): Longint; stdcall;
function sio_FtASCIIRx(port:Longint; fname:PChar; func:CallBackProc; key:Longint; sec:Longint): Longint; stdcall;
function sio_FtXmodemCheckSumTx(port:Longint; fname:PChar; func:CallBackProc; key:Longint): Longint; stdcall;
function sio_FtXmodemCheckSumRx(port:Longint; fname:PChar; func:CallBackProc; key:Longint): Longint; stdcall;
function sio_FtXmodemCRCTx(port:Longint; fname:PChar; func:CallBackProc; key:Longint): Longint; stdcall;
function sio_FtXmodemCRCRx(port:Longint; fname:PChar; func:CallBackProc; key:Longint): Longint; stdcall;
function sio_FtXmodem1KCRCTx(port:Longint; fname:PChar; func:CallBackProc; key:Longint): Longint; stdcall;
function sio_FtXmodem1KCRCRx(port:Longint; fname:PChar; func:CallBackProc; key:Longint): Longint; stdcall;
function sio_FtYmodemTx(port:Longint; fname:PChar; func:CallBackProc; key:Longint): Longint; stdcall;
function sio_FtYmodemRx(port:Longint; var fname:PChar;fno:LongInt;func:CallBackProc; key:Longint): Longint; stdcall;
function sio_FtZmodemTx(port:Longint; fname:PChar; func:CallBackProc; key:Longint): Longint; stdcall;
function sio_FtZmodemRx(port:Longint; var fname:PChar;fno:LongInt;func:CallBackProc; key:Longint): Longint; stdcall;
function sio_FtKermitTx(port:Longint; fname:PChar; func:CallBackProc; key:Longint): Longint; stdcall;
function sio_FtKermitRx(port:Longint; var fname:PChar;fno:LongInt;func:CallBackProc; key:Longint): Longint; stdcall;


implementation
function sio_open; external 'PComm.dll';
function sio_close; external 'PComm.dll';
function sio_ioctl; external 'PComm.dll';
function sio_flowctrl; external 'PComm.dll';
function sio_flush; external 'PComm.dll';
function sio_DTR; external 'PComm.dll';
function sio_RTS; external 'PComm.dll';
function sio_lctrl; external 'PComm.dll';
function sio_baud; external 'PComm.dll';
function sio_getch; external 'PComm.dll';
function sio_read; external 'PComm.dll';
function sio_linput; external 'PComm.dll';
function sio_putch; external 'PComm.dll';
function sio_putb; external 'PComm.dll';
function sio_write; external 'PComm.dll';
function sio_putb_x; external 'PComm.dll';
function sio_putb_x_ex; external 'PComm.dll';
function sio_lstatus; external 'PComm.dll';
function sio_iqueue; external 'PComm.dll';
function sio_oqueue; external 'PComm.dll';
function sio_Tx_hold; external 'PComm.dll';
function sio_getbaud; external 'PComm.dll';
function sio_getmode; external 'PComm.dll';
function sio_getflow; external 'PComm.dll';
function sio_data_status; external 'PComm.dll';
function sio_term_irq; external 'PComm.dll';
function sio_cnt_irq; external 'PComm.dll';
function sio_modem_irq; external 'PComm.dll';
function sio_break_irq; external 'PComm.dll';
function sio_Tx_empty_irq; external 'PComm.dll';
function sio_break; external 'PComm.dll';
function sio_view; external 'PComm.dll';
function sio_TxLowWater; external 'PComm.dll';
function sio_AbortWrite; external 'PComm.dll';
function sio_AbortRead; external 'PComm.dll';
function sio_SetWriteTimeouts; external 'PComm.dll';
function sio_GetWriteTimeouts; external 'PComm.dll';
function sio_SetReadTimeouts; external 'PComm.dll';
function sio_GetReadTimeouts; external 'PComm.dll';
function sio_FtASCIITx; external 'PComm.dll';
function sio_FtASCIIRx; external 'PComm.dll';
function sio_FtXmodemCheckSumTx; external 'PComm.dll';
function sio_FtXmodemCheckSumRx; external 'PComm.dll';
function sio_FtXmodemCRCTx; external 'PComm.dll';
function sio_FtXmodemCRCRx; external 'PComm.dll';
function sio_FtXmodem1KCRCTx; external 'PComm.dll';
function sio_FtXmodem1KCRCRx; external 'PComm.dll';
function sio_FtYmodemTx; external 'PComm.dll';
function sio_FtYmodemRx; external 'PComm.dll';
function sio_FtZmodemTx; external 'PComm.dll';
function sio_FtZmodemRx; external 'PComm.dll';
function sio_FtKermitTx; external 'PComm.dll';
function sio_FtKermitRx; external 'PComm.dll';

end.
