(*********************************************************************
    MxTool.pas
     -- Process PComm function return value


    History:   Date       Author         Comment
               3/9/98     Casper         Wrote it.
               12/08/98   Casper         Modify message

**********************************************************************)
unit MxTool;

interface

procedure ShowSysErr(title:string);
procedure MxShowError(title:string;errcode:LongInt);

implementation

uses
  Windows,Dialogs,PComm,SysUtils,Forms;

procedure MxShowError(title:string;errcode:LongInt);
var
  buf:string;
begin
  if errcode <> SIO_WIN32FAIL then
  begin
    case errcode of
    SIO_BADPORT:
      buf := 'Port number is invalid or port is not opened in advance';
    SIO_OUTCONTROL:
      buf := 'This board does not support this function';
    SIO_NODATA:
      buf := 'No data to read';
    SIO_OPENFAIL:
      buf := 'No such port or port is occupied by other program';
    SIO_RTS_BY_HW:
      buf := 'RTS can''t be set because H/W flowctrl';
    SIO_BADPARM:
      buf := 'Bad parameter';
    SIO_BOARDNOTSUPPORT:
      buf := 'This board does not support this function';
    SIO_ABORT_WRITE:
      buf := 'Write has blocked, and user abort write';
    SIO_WRITETIMEOUT:
      buf := 'Write timeout has happened';
    else
      buf := 'Unknown Error:'+IntToStr(errcode);
    end;
    Application.MessageBox(PChar(buf),PChar(title),MB_OK or MB_ICONSTOP);
  end
  else
    ShowSysErr(title);
  end;

procedure ShowSysErr(title:string);
var
  syserr  :LongInt;
  lpMsgBuf:array[0..79] of Char;
  lang    :LongInt;
begin
  syserr := GetLastError();

  {MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT) }
  lang := (SUBLANG_DEFAULT shl 10)  + LANG_NEUTRAL;

  FormatMessage(
                FORMAT_MESSAGE_FROM_SYSTEM,
		nil,
		syserr,
		lang,
		@lpMsgBuf,
		80,
		nil
  );
  Application.MessageBox(lpMsgBuf,PChar(title),MB_OK or MB_ICONSTOP);
end;

end.

