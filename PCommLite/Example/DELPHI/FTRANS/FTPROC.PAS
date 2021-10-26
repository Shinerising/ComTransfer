(*********************************************************************
    FtProc.pas
     -- File transfer thread for file transfer example program.


    History:   Date       Author         Comment
               3/11/98    Casper         Wrote it.
               12/14/98   Casper         Modify message.Update.
**********************************************************************)

unit FtProc;

interface
uses
  Classes;

Const
  FT_XMIT = 0;
  FT_RECV = 1;

  FTXMDM1KCRC = 0;
  FTXMDMCHK = 1;
  FTXMDMCRC = 2;
  FTZMDM = 3;
  FTYMDM = 4;
  FTKERMIT = 5;
  FTASCII = 6;

  MAX_PATH = 260; {Win32 defined}

type
  TFtProc = class(TThread)
  private
    { Private declarations }
  protected
    procedure Execute; override;
  end;

var
  GxFname : array[0..MAX_PATH] of Char;
  GrFname : array[0..MAX_PATH] of Char;
  GrPath : array[0..MAX_PATH] of Char;

  GstrProtocol:array [0..6] of string = (
    'XModem-1KCRC','XModem-CheckSum','XModem-CRC',
    'ZModem','YModem','Kermit','ASCII'
  );

  GProtocol : Word;
  GDirection : Word;
  GftCancel : boolean;

implementation

uses Windows,Forms,PComm,ExGlobal,FStat,MxTool;

function xCallBack(xmitlen:LongInt;buflen:LongInt;buf:PChar;flen:LongInt):
         LongInt;stdcall;forward;
function rCallBack(recvlen:LongInt;buflen:LongInt;buf:PChar;flen:LongInt):
         LongInt;stdcall;forward;
procedure ProcessRet(port:LongInt;ret:LongInt;protocol:Word;direction:Word);
          forward;

{ TFtProc }

(*
  After create thread object in main process,'Execute()' function
  will be called automatically.
  If user press 'Cancel' button which on status dialog,
  'GftCancel' flag will be set to true.This will let callback
  function to return -1 to terminate file transfer.
*)
procedure TFtProc.Execute;
var
  ret : LongInt;
  port: LongInt;
  fname : PChar;

begin
  { Place thread code here }
  port := GCommData.Port;

  ret := 0;
  if (GDirection = FT_XMIT) then
  begin
    case GProtocol of
    FTXMDM1KCRC:
      ret := sio_FtXmodem1KCRCTx(port,GxFname,xCallBack, 27);
    FTXMDMCHK:
      ret := sio_FtXmodemCheckSumTx(port,GxFname,xCallBack, 27);
    FTXMDMCRC:
      ret := sio_FtXmodemCRCTx(port,GxFname,xCallBack, 27);
    FTZMDM:
      ret := sio_FtZmodemTx(port,GxFname,xCallBack, 27);
    FTYMDM:
      ret := sio_FtYmodemTx(port,GxFname,xCallBack, 27);
    FTKERMIT:
      ret := sio_FtKermitTx(port,GxFname,xCallBack, 27);
    FTASCII:
      ret := sio_FtASCIITx(port,GxFname,xCallBack, 27);
    end;
  end
  else   {FT_RECV}
  begin
    case GProtocol of
    FTXMDM1KCRC:
      ret := sio_FtXmodem1KCRCRx(Port, GrFname,rCallBack, 27);
    FTXMDMCHK:
      ret := sio_FtXmodemCheckSumRx(Port, GrFname,rCallBack, 27);
    FTXMDMCRC:
      ret := sio_FtXmodemCRCRx(Port, GrFname,rCallBack, 27);
    FTZMDM:
      begin
        fname := GrFname;
        ret := sio_FtZmodemRx(Port, fname,1,rCallBack, 27);
      end;
    FTYMDM:
      begin
        fname := GrFname;
        ret := sio_FtYmodemRx(Port, fname, 1, rCallBack, 27);
      end;
    FTKERMIT:
      begin
        fname := GrFname;
        ret := sio_FtKermitRx(Port, fname, 1, rCallBack, 27);
      end;
    FTASCII:
      ret := sio_FtASCIIRx(Port, GrFname,rCallBack, 27,3);
    end;
  end;

  FStatFrm.Close();

  if ret < 0 then  { maybe something error }
    ProcessRet(port, ret, GProtocol, GDirection)
  else
    if (GDirection = FT_XMIT) then
      Application.MessageBox(PChar('File Transmit OK'),PChar(GszAppName),MB_OK)
    else
      Application.MessageBox(PChar('File Receive OK'),PChar(GszAppName),MB_OK);
end;


function xCallBack(xmitlen:LongInt;buflen:LongInt;buf:PChar;flen:LongInt):
         LongInt;stdcall;
begin
  if GftCancel then
  begin
    xCallBack := -1; { this will terminate file transfer }
    Exit;
  end;

  FStatFrm.RefreshDlg(xmitlen, flen, GxFname);
  xCallBack := 0;
end;

function rCallBack(recvlen:LongInt;buflen:LongInt;buf:PChar;flen:LongInt):
         LongInt;stdcall;
begin
  if GftCancel then
  begin
    rCallBack := -1; { this will terminate file transfer }
    Exit;
  end;

  FStatFrm.RefreshDlg(recvlen, flen, GrFname);
  rCallBack := 0;
end;

procedure ProcessRet(port:LongInt;ret:LongInt;protocol:Word;direction:Word);
var
  buf : string;
begin
  if (ret <> SIOFT_WIN32FAIL) then
  begin
    case ret of
      SIOFT_BADPORT:
        buf := 'Port is not opened in advance';
      SIOFT_TIMEOUT:
        if (direction = FT_RECV) then
          buf := 'Receive timeout'
        else
          buf := 'Transmit Timeout';
      SIOFT_FUNC:
        if ((protocol = FTASCII) And (direction = FT_RECV)) then
          { When downloading ASCII file,user must press "Cancel"
            button to stop ASCII receive }
          buf := 'Receive File Ok'
        else
          buf := 'User abort';
      SIOFT_FOPEN:
        buf := 'Can''t open file';
      SIOFT_CANABORT:
        buf := 'Remote side abort';
      SIOFT_BOARDNOTSUPPORT:
        buf := 'Board does not support this function';
      SIOFT_PROTOCOL, SIOFT_SKIP:
        buf := 'File transfer error';
      else
        buf := 'File transfer error';
    end;
    Application.MessageBox(PChar(buf),PChar(GszAppName),MB_OK);
  end
  else
    ShowSysErr(GszAppName);
end;


end.
