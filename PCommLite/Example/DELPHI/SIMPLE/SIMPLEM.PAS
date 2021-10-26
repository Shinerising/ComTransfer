(*********************************************************************
    SimpleM.pas
     -- Main window for simple dumb terminal example program.

    Description:
      1.Select "setting..." menu item to set com port option.
      2.Select "Open" menu item to open com port.
        After selected "Open" from menu,you can type any character
        to send to com port.When any data received from com port,
        program will dump data to window.
      3.Select "Close" menu item to close com port.

    This program demo:
        How to open com port(sio_open);
        How to set commnunication parameter(sio_ioctl,sio_flowctrl);
        How to control line status(sio_DTR,sio_RTS);
        How to send data to com port(sio_write,sio_putch);
        How to read data(sio_read);
        How to close com port(sio_close);
        How to use background thread to read data.

    Use function:
        sio_open,       sio_close,   sio_ioctl,
        sio_flowctrl,   sio_DTR,     sio_RTS,
        sio_read,       sio_putch,   sio_write.
        sio_SetWriteTimeouts;

    History:   Date       Author         Comment
               3/9/98     Casper         Wrote it.
              12/14/98    Casper         Update.
**********************************************************************)

unit SimpleM;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Menus,ExGlobal, StdCtrls;

type
  TSimpleForm = class(TExampleForm) {TExampleForm defined in ExGlobal}
    SimpleMenu: TMainMenu;
    cmPort: TMenuItem;
    cmOpen: TMenuItem;
    cmClose: TMenuItem;
    N1: TMenuItem;
    cmClear: TMenuItem;
    N2: TMenuItem;
    cmExit: TMenuItem;
    cmSetting: TMenuItem;
    cmHelp: TMenuItem;
    cmAbout:TMenuItem;

    procedure FormCreate(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure cmOpenClick(Sender: TObject);
    procedure cmCloseClick(Sender: TObject);
    procedure cmClearClick(Sender: TObject);
    procedure cmExitClick(Sender: TObject);
    procedure cmSettingClick(Sender: TObject);
    procedure cmAboutClick(Sender: TObject);
    procedure TermKeyPress(Sender: TObject; var Key: Char);
    procedure SwitchMenu;
    function  OpenPort:Boolean;
    procedure ClosePort;
    function  PortSet:boolean;
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  SimpleForm : TSimpleForm;

implementation

uses PComm,MxTool,Config,ReadThd, About;

{$R *.DFM}

procedure TSimpleForm.FormCreate(Sender: TObject);
begin
  with GCommData do
  begin
    { com port default setting }
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

  GszAppName := 'Simple Demo';
  GbOpen := false;
  GhForm := SimpleForm;

  SwitchMenu();
end;

procedure TSimpleForm.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  if GbOpen then
    ClosePort()
end;

procedure TSimpleForm.cmOpenClick(Sender: TObject);
begin
  OpenPort();
end;

procedure TSimpleForm.cmCloseClick(Sender: TObject);
begin
  ClosePort();
end;

procedure TSimpleForm.cmClearClick(Sender: TObject);
begin
  Term.Clear();
end;

procedure TSimpleForm.cmSettingClick(Sender: TObject);
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

procedure TSimpleForm.cmAboutClick(Sender: TObject);
begin
  AboutFrm.ShowModal
end;

procedure TSimpleForm.cmExitClick(Sender: TObject);
begin
  Close;
end;

procedure TSimpleForm.TermKeyPress(Sender: TObject; var Key: Char);
begin
  sio_putch(GCommData.Port,Integer(Key));
  { or use sio_write  :
    sio_write(GCommData.Port,@Key,1);
  }

  Key:=Char(0);
end;


procedure TSimpleForm.SwitchMenu;
begin
  cmOpen.Enabled := not GbOpen;
  cmClose.Enabled := GbOpen;
end;

function TSimpleForm.OpenPort:Boolean;
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
  GhExit := false;
  TReadThread.Create(false);
  GbOpen := true;
  SwitchMenu();
  ShowStatus();
end;

procedure  TSimpleForm.ClosePort;
begin
  GhExit := true;
  sio_close (GCommData.Port);
  GbOpen := False;
  SwitchMenu();
  ShowStatus();
end;

function TsimpleForm.PortSet:boolean;
var
  port  : LongInt;
  mode  : LongInt;
  hw,sw : LongInt;
  ret   : LongInt;
  tout  : LongInt;
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

  tout := 1000 div sio_getbaud(GCommData.Port) ; {ms /byte}
  if tout <1  then
    tout := 1;
  tout :=  tout * 1* 3; { 1 byte; '*3' is for delay }
  if tout <100 then
    tout := 100;
  sio_SetWriteTimeouts(GCommData.Port,tout);

  ShowStatus();
  PortSet := True;
end;

end.

