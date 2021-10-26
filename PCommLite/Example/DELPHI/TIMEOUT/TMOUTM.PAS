(*********************************************************************
    TmOutM.pas
      -- Main window for read & write timeout setting example program.

      1.Select "setting..." menu item to set com port option.
      2.Select "Open" menu item to open com port.
        After selected "Open" from menu,you can select "Timeout Demo"
        to test timeout status.
        Polling Write:
		sio_write() will return immediately. The data will be 
 		copied to driver,but do not really wrtie to com port. This 
		operation will maybe let sio_write() return 0, means that
		not enough buffer to write. Be careful this polling operation 
		will 'eat' a large amount of system resource, including 
		memory, processor time.

	Block Write:
		sio_write() will block until the all data has be write to 
		com port (But maybe some data is still in com port output 
		buffer when sio_write() return). You can check the sio_write() 
		elapsed time. This operation is suitable to using another 
		thread to write data. Then you can call sio_AbortWrite() 
		to abort write operation in main thread when you want to 
		stop writing.

	Block Write (Timeout):
		This operation is the same as "Block Write". The difference 
		is that, sio_write() will block until the tiemout is arrived 
		or the all data has be write to com port. You can decrease 
		the timeout value to check the difference.

	Polling Read:
		sio_read() will return immediately. sio_read() just checks
		input buffer, gets all buffer data (maybe less than or equal
		to that sio_read() want to read), then returns. If no data in 
		buffer,	sio_read() return 0. Be careful this polling operation 
		will 'eat' a large amount of system resource, including memory, 
		processor time.
	
	Block Read:
		sio_read() will block until the input buffer data length is
		equal to that sio_read() want to read. This operation is 
		suitable to using another thread to read data. Then you can 
		call sio_AbortRead() to abort read operation in main thread 
		when you want to stop reading.

	Block Read (Total Timeout):
		This operation is the saem as "Block Read". The difference
		is that, sio_read() will block until timeout is arrived or
		the input buffer data length is equal to that sio_read()
		want to read.You can decrease the timeout value to check 
		the difference.
		In this example, you can connect to terminal. Then you can
		try 2 cases from terminal:
			send 10240 bytes, 
			or wait the timeout is arrived.
		Check the sio_read() elapsed time.

	Block Read (Interval Timeout):
		sio_read() will wait the first byte arrived, then begin
		interval timeout checking.sio_read will block until the
		interval timeout is arrived or the input buffer data length
		is equal to that sio_read() want to read.
		In this example, you can connect to terminal. Then you can
		try 2 cases from terminal:
			send 1 or 2 byte, 
			send 10240 bytes, 
		Check the sio_read() elapsed time.

	Block Read ( Total+Interval Timeout ):
		sio_read() will block until the timeout is arrived or the 
		input buffer data length is equdal to that sio_read() want 
		to read.
		In this example, you can connect to terminal. Then you can
		try 3 cases from terminal : 
			send 1 or 2 byte, 
			send 10240 bytes, 
			send > 10240 bytes
		Check the sio_read() elapsed time.
		                            
      3.Select "Close" menu item to close com port.


    This program demo:
      How to set write timeout(sio_SetWriteTimeouts());
      How to set read timeout(sio_SetReadTimeouts());
      How to abort write process(sio_AbortWrite());
      How to abort read process(sio_AbortRead());

    Use function:
        sio_open,       sio_close,   sio_ioctl,
        sio_flowctrl,   sio_DTR,     sio_RTS,
        sio_read,       sio_write,
        
        sio_SetWriteTimesout,        sio_SetReadTimeouts,
        sio_AbortWrite,              sio_AbortRead;



    History:   Date       Author         Comment
               3/11/98    Casper         Wrote it.
              12/14/98    Casper         Update.
**********************************************************************)

unit TmOutM;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Menus,ExGlobal,StdCtrls;

type
  TTmOutFrm = class(TExampleForm)
    TOMenu: TMainMenu;
    cmPort: TMenuItem;
    cmOpen: TMenuItem;
    cmClose: TMenuItem;
    N1: TMenuItem;
    cmExit: TMenuItem;
    cmSetting: TMenuItem;
    cmTODemo: TMenuItem;
    cmPollingWrite: TMenuItem;
    cmBlockWrite: TMenuItem;
    cmBlockWriteTO: TMenuItem;
    N2: TMenuItem;
    cmPollingRead: TMenuItem;
    cmBlockRead: TMenuItem;
    cmBlockReadTTO: TMenuItem;
    cmBlockReadITO: TMenuItem;
    cmBlockReadTITO: TMenuItem;
    cmHelp: TMenuItem;
    cmAbout: TMenuItem;

    procedure FormCreate(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure cmOpenClick(Sender: TObject);
    procedure cmCloseClick(Sender: TObject);
    procedure cmExitClick(Sender: TObject);
    procedure cmSettingClick(Sender: TObject);
    procedure cmAboutClick(Sender: TObject);
    procedure SwitchMenu;
    function  OpenPort:Boolean;
    procedure ClosePort;
    function  PortSet:boolean;
    procedure cmPollingWriteClick(Sender: TObject);
    procedure cmBlockWriteClick(Sender: TObject);
    procedure cmBlockWriteTOClick(Sender: TObject);
    procedure cmPollingReadClick(Sender: TObject);
    procedure cmBlockReadClick(Sender: TObject);
    procedure cmBlockReadTTOClick(Sender: TObject);
    procedure cmBlockReadITOClick(Sender: TObject);
    procedure cmBlockReadTITOClick(Sender: TObject);

  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  TmOutFrm: TTmOutFrm;

implementation

uses PComm,MxTool,Config,About, WStat, RStat, TOThread;

const IDM_WRITE_POLL = 0;
const IDM_WRITE_BLOCK = 1;
const IDM_WRITE_TIMEOUT = 2;
const IDM_READ_POLL = 3;
const IDM_READ_BLOCK = 4;
const IDM_READ_BLOCK_T = 5;
const IDM_READ_BLOCK_I = 6;
const IDM_READ_BLOCK_TI = 7;

{$R *.DFM}

procedure TTmOutFrm.FormCreate(Sender: TObject);
begin
  with GCommData do
  begin
    Port := 1;
    ibaudrate  := 14;
    iparity := 0;
    ibytesize := 3;
    istopbits := 0;
    BaudRate  := B38400;
    Parity := P_NONE;
    ByteSize := BIT_8;
    StopBits := STOP_1;
    Hw := false;
    Sw := false;
    Dtr := true;
    Rts := true;
  end;


  GszAppName := 'Timeout Demo';
  GbOpen := false;
  GhForm := TmOutFrm;

  SwitchMenu();
end;

procedure TTmOutFrm.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  if GbOpen then
    ClosePort()
end;

procedure TTmOutFrm.cmOpenClick(Sender: TObject);
begin
  OpenPort();
end;

procedure TTmOutFrm.cmCloseClick(Sender: TObject);
begin
  ClosePort();
end;

procedure TTmOutFrm.cmSettingClick(Sender: TObject);
var
  bakdata : TCOMMDATA;
begin

  bakdata := GCommData;
  if CfgForm.ShowModal = mrCancel then
    Exit;

  if GbOpen then
    if PortSet()=false then
    begin
      GCommData := bakdata;
      Exit;
    end;

  ShowStatus();
end;

procedure TTmOutFrm.cmAboutClick(Sender: TObject);
begin
  AboutFrm.ShowModal
end;

procedure TTmOutFrm.cmExitClick(Sender: TObject);
begin
  Close;
end;

procedure TTmOutFrm.SwitchMenu;
begin
  cmOpen.Enabled := not GbOpen;
  cmClose.Enabled := GbOpen;
  cmTODemo.Enabled := GbOpen;
end;

function TTmOutFrm.OpenPort:Boolean;
var
  ret:Integer;
begin
  OpenPort := false;
  ret := sio_open(GCommData.Port);
  if ret <> SIO_OK then
  begin
    MxShowError('sio_open',ret);
    Exit;
  end;
  if PortSet() = false then
  begin
    sio_close(GCommData.Port);
    Exit;
  end;
  OpenPort := true;
  GbOpen := true;
  SwitchMenu();
  ShowStatus();
end;

procedure  TTmOutFrm.ClosePort;
begin
  sio_close (GCommData.Port);
  GbOpen := False;
  SwitchMenu();
  ShowStatus();
end;

function TTmOutFrm.PortSet:boolean;
var
  port  : LongInt;
  mode  : LongInt;
  hw,sw : LongInt;
  ret : LongInt;
begin
  port := GCommData.Port;
  mode := GCommData.Parity or GCommData.ByteSize or GCommData.StopBits;
  PortSet := false;

  if GCommData.Hw  then
    hw := 3        { bit0 and bit1 }
  else
    hw := 0;

  if GCommData.Sw  then
    sw := 12        { bit2 and bit3 }
  else
    sw := 0;


  ret := sio_ioctl(port,GCommData.BaudRate,mode);
  if ret<>SIO_OK then
  begin
    MxShowError('sio_ioctl',ret);
    Exit;
  end;

  ret := sio_flowctrl(port,hw or sw);
  if ret<>SIO_OK then
  begin
    MxShowError('sio_flowctrl',ret);
    Exit;
  end;

  ret := sio_DTR(port,Integer(GCommData.Dtr));
  if ret<>SIO_OK then
  begin
    MxShowError('sio_DTR',ret);
    Exit;
  end;

  if not GCommData.Hw  then
  begin
    ret := sio_RTS(port,Integer(GCommData.Rts));
    if ret<>SIO_OK then
    begin
      MxShowError('sio_RTS',ret);
      Exit;
    end;
  end;

  ShowStatus();
  PortSet := True;
end;


procedure DemoWriteTimeout(port:LongInt;testitem:Integer);
var
  baud : LongInt;
  tout : LongInt;
begin
  case testitem of
    IDM_WRITE_POLL:
      sio_SetWriteTimeouts(port, MAXDWORD);
    IDM_WRITE_BLOCK:
      sio_SetWriteTimeouts(port, 0);
    IDM_WRITE_TIMEOUT:
      begin
        baud := sio_getbaud(port);
        { timeout(ms)  = Buflen *(1000/ (baud/10));
                       = Buflen *  (ms /byte)
        }
        tout := (BUFLEN *1000 div (baud div 10)) * 3;{*3 is for dealy}
        sio_SetWriteTimeouts(port, tout);
      end;
  end;

  GhExit := False;
  TWTOThread.Create(false);

  WStatFrm.ShowModal;

  sio_AbortWrite(port);
  sio_flush(port, 2);

end;


procedure DemoReadTimeout(port:LongInt;testitem:Integer);
var
  baud : LongInt;
  tout : LongInt;
begin
  baud := sio_getbaud(port);
  { timeout(ms)  = Buflen *(1000/ (baud/10));
                 = Buflen *  (ms /byte)
  }

  tout := (BUFLEN * 1000 div (baud div 10)) * 3; { *3  is for delay }

  case testitem of
    IDM_READ_POLL:
        sio_SetReadTimeouts(port,MAXDWORD,0);

    IDM_READ_BLOCK:
        sio_SetReadTimeouts(port,0,0);

    IDM_READ_BLOCK_T:
        sio_SetReadTimeouts(port,tout,0);

    IDM_READ_BLOCK_I:
        sio_SetReadTimeouts(port,0,1000);

    IDM_READ_BLOCK_TI:
        sio_SetReadTimeouts(port,tout,1000);
  end;

  GhExit := false;
  TRTOThread.Create(false);

  RStatFrm.ShowModal;

  sio_AbortRead(port);
  sio_flush(GCommData.Port,2);
end;


procedure TTmOutFrm.cmPollingWriteClick(Sender: TObject);
begin
  DemoWriteTimeout(GCommData.Port,IDM_WRITE_POLL);
end;

procedure TTmOutFrm.cmBlockWriteClick(Sender: TObject);
begin
  DemoWriteTimeout(GCommData.Port,IDM_WRITE_BLOCK);
end;

procedure TTmOutFrm.cmBlockWriteTOClick(Sender: TObject);
begin
  DemoWriteTimeout(GCommData.Port,IDM_WRITE_TIMEOUT);
end;

procedure TTmOutFrm.cmPollingReadClick(Sender: TObject);
begin
  DemoReadTimeout(GCommData.Port,IDM_READ_POLL);
end;

procedure TTmOutFrm.cmBlockReadClick(Sender: TObject);
begin
  DemoReadTimeout(GCommData.Port,IDM_READ_BLOCK);
end;

procedure TTmOutFrm.cmBlockReadTTOClick(Sender: TObject);
begin
  DemoReadTimeout(GCommData.Port,IDM_READ_BLOCK_T);
end;

procedure TTmOutFrm.cmBlockReadITOClick(Sender: TObject);
begin
  DemoReadTimeout(GCommData.Port,IDM_READ_BLOCK_I);
end;

procedure TTmOutFrm.cmBlockReadTITOClick(Sender: TObject);
begin
  DemoReadTimeout(GCommData.Port,IDM_READ_BLOCK_TI);
end;

end.
