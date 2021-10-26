(*********************************************************************
    PortStat.pas
     -- Main window for port status example program.

    Description:
      1.Select "setting..." menu item to set com port option.
      2.Select "Open" menu item to open com port.
        After selected "Open" from menu,program will pop up one dialog
        to display the status of the opened com port.
      3.Select "Close" menu item to close com port.

    This program demo:
        How to get the status of the com port.

    NOTE:To correctly use sio_data_status(),you must
         call sio_data_status() before calling sio_iqueue(),
         sio_oqueue(),sio_Tx_hold().

    Use function:
        sio_open,       sio_close,   sio_ioctl,
        sio_flowctrl,   sio_DTR,     sio_RTS,
        sio_read,       sio_putch,   sio_write.
        sio_SetWriteTimeouts,

        sio_lstatus(),               sio_iqueue(),
        sio_oqueue(),                sio_Tx_hold(),
        sio_getbaud(),               sio_getmode(),
        sio_getflow(),               sio_data_status();


    History:   Date       Author         Comment
               3/11/98    Casper         Wrote it.
	      12/14/98	  Casper         Update.	
*********************************************************************)

unit PStatM;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Menus,ExGlobal, StdCtrls;

type
  TPStatFrm = class(TExampleForm)
    PStatMenu: TMainMenu;
    cmPort: TMenuItem;
    cmOpen: TMenuItem;
    cmClose: TMenuItem;
    N1: TMenuItem;
    cmClear: TMenuItem;
    N2: TMenuItem;
    cmExit: TMenuItem;
    cmConfig: TMenuItem;
    cmHelp: TMenuItem;
    cmAbout: TMenuItem;
    cmSetting: TMenuItem;
    cmStatus: TMenuItem;
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
    procedure cmStatusClick(Sender: TObject);

  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  PStatFrm: TPStatFrm;

implementation

uses PComm,MxTool,Config,ReadThd, About, StatDlg;

{$R *.DFM}
procedure TPStatFrm.FormCreate(Sender: TObject);
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

  GszAppName := 'Port Status Demo';
  GbOpen := false;
  GhForm := PStatFrm;

  SwitchMenu();
end;

procedure TPStatFrm.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  if GbOpen then
    ClosePort()
end;

procedure TPStatFrm.cmOpenClick(Sender: TObject);
begin
  OpenPort();
end;

procedure TPStatFrm.cmCloseClick(Sender: TObject);
begin
  ClosePort();
end;

procedure TPStatFrm.cmClearClick(Sender: TObject);
begin
  Term.Clear();
end;

procedure TPStatFrm.cmSettingClick(Sender: TObject);
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

procedure TPStatFrm.cmAboutClick(Sender: TObject);
begin
  AboutFrm.ShowModal
end;

procedure TPStatFrm.cmExitClick(Sender: TObject);
begin
  Close;
end;

procedure TPStatFrm.TermKeyPress(Sender: TObject; var Key: Char);
begin
  sio_putch(GCommData.Port,Integer(Key));
  { or use sio_write  :
    sio_write(GCommData.Port,@Key,1);
  }
  Key:=Char(0);
end;


procedure TPStatFrm.SwitchMenu;
begin
  cmOpen.Enabled := not GbOpen;
  cmClose.Enabled := GbOpen;
  cmStatus.Enabled := GbOpen;
end;

function TPStatFrm.OpenPort:Boolean;
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
  StatDlgFrm.Show();
  SwitchMenu();
  ShowStatus();
end;

procedure  TPStatFrm.ClosePort;
begin
  GhExit := true;
  StatDlgFrm.Close;
  sio_close (GCommData.Port);
  GbOpen := False;
  SwitchMenu();
  ShowStatus();
end;

function TPStatFrm.PortSet:boolean;
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
  sio_SetWriteTimeouts(GCommData.Port,tout);

  ShowStatus();
  PortSet := True;
end;

procedure TPStatFrm.cmStatusClick(Sender: TObject);
begin
  StatDlgFrm.Show();
end;

end.
