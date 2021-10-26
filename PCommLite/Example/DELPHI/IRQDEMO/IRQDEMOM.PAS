(**********************************************************************
    IrqDemoM.pas
     -- Main window for Irq function demo example program.

    Description :
      1.Select "setting..." menu item to set com port option.
      2.Select "Open" menu item to open com port.
        After selected "Open" from menu,you can type any key to
        test sio_Tx_empty_irq(). You can also connect to another
        terminal to test Irq function:
          a.Sending some data to gerenate 'Rx event'(sio_cnt_irq);
          b.Changing modem line(DTR/RTS) to generate
            'modem line changed event'(sio_modem_irq);
          c.Sending break signal to generate 'break event'
            (sio_break_irq);
          d.Typing 'A' will generate 'RxFlag event'(sio_term_irq)
            and 'Rx event'(sio_cnt_irq);
        This program  will got evnet and show Irq count to screen.
      3.Select "Close" menu item to close com port.


    This program demo:
        How to use Irq function(sio_xxx_irq);
        How to disable Irq function;
        How to use Irq callback funtion.

    Use function:
        sio_open,       sio_close,      sio_ioctl,
        sio_flowctrl,   sio_DTR,        sio_RTS,
        sio_putch,      sio_write,
	sio_SetWriteTimeouts,

        sio_term_irq,   sio_cnt_irq,    sio_break_irq,
        sio_modem_irq,  sio_Tx_empty_irq.


    History:   Date       Author         Comment
               3/1/98     Casper         Wrote it.
              12/08/98    Casper         Add timer.Update.
***********************************************************************)

unit IrqDemoM;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Menus, ExGlobal,StdCtrls, ExtCtrls;

type
  TIrqForm = class(TExampleForm)
    MainMenu1: TMainMenu;
    cmPort: TMenuItem;
    cmOpen: TMenuItem;
    cmClose: TMenuItem;
    N1: TMenuItem;
    cmClear: TMenuItem;
    N2: TMenuItem;
    cmExit: TMenuItem;
    cmSetting: TMenuItem;
    cmHelp: TMenuItem;
    cmAbout: TMenuItem;
    Timer1: TTimer;
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
    procedure Timer1Timer(Sender: TObject);

  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  IrqForm: TIrqForm;

implementation
{$R *.DFM}
uses PComm,IrqProc,MxTool,Config,About;

function InitIrq(Port:LongInt;termcode:Byte):boolean;forward;
procedure ClearIrq(Port:LongInt);forward;


procedure TIrqForm.FormCreate(Sender: TObject);
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

  GszAppName := 'Irq Function Demo';
  GbOpen := false;
  GhForm := IrqForm;

  SwitchMenu();
end;

procedure TIrqForm.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  if GbOpen then
    ClosePort()
end;

procedure TIrqForm.cmOpenClick(Sender: TObject);
begin
  OpenPort();
end;

procedure TIrqForm.cmCloseClick(Sender: TObject);
begin
  ClosePort();
end;

procedure TIrqForm.cmClearClick(Sender: TObject);
begin
  Term.Clear();
end;

procedure TIrqForm.cmSettingClick(Sender: TObject);
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

procedure TIrqForm.cmAboutClick(Sender: TObject);
begin
  AboutFrm.ShowModal
end;

procedure TIrqForm.cmExitClick(Sender: TObject);
begin
  Close;
end;

procedure TIrqForm.TermKeyPress(Sender: TObject; var Key: Char);
begin
  sio_putch(GCommData.Port,Integer(Key));
  { or use sio_write  :
    sio_write(GCommData.Port,@Key,1);
  }
  Key:=Char(0);
end;

procedure TIrqForm.SwitchMenu;
begin
  cmOpen.Enabled := not GbOpen;
  cmClose.Enabled := GbOpen;
end;

function TIrqForm.OpenPort:Boolean;
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

  Term.Clear();
  if InitIrq(GCommData.Port,Byte('A'))=false then
  begin
    sio_close(GCommData.Port);
    Exit;
  end;

  OpenPort := true;
  GbOpen := true;

  SwitchMenu();
  ShowStatus();
end;

procedure  TIrqForm.ClosePort;
begin
  ClearIrq(GCommData.Port);
  sio_close (GCommData.Port);
  GbOpen := False;
  SwitchMenu();
  ShowStatus();
end;

function TIrqForm.PortSet:boolean;
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
  if tout < 100 then
      tout := 100;
  sio_SetWriteTimeouts(GCommData.Port,tout);


  ShowStatus();
  PortSet := True;
end;


function InitIrq(Port:LongInt;termcode:Byte):boolean;
var
  ret : LongInt;
begin
  InitIrq := false;
  GIdx := 0;
  GTermIrqCnt := 0;
  GCntIrqCnt := 0;
  GModemIrqCnt := 0;
  GBreakIrqCnt := 0;
  GTxEmptyIrqCnt := 0;

  ret := sio_term_irq(Port, TermIrq, termcode);
  if ret <> SIO_OK then
  begin
      MxShowError('sio_term_irq', ret);
      Exit;
  end;

  ret := sio_cnt_irq(Port, CntIrq, 1);
  if ret <> SIO_OK then
  begin
      MxShowError('sio_cnt_irq', ret);
      Exit;
  end;

  ret := sio_break_irq(Port, BreakIrq);
  if ret <> SIO_OK then
  begin
      MxShowError('sio_break_irq', ret);
      Exit;
  end;

  ret := sio_modem_irq(Port, ModemIrq);
  if ret <> SIO_OK then
  begin
      MxShowError('sio_modem_irq', ret);
      Exit;
  end;

  ret := sio_Tx_empty_irq(Port, TxEmptyIrq);
  if ret <> SIO_OK then
  begin
      MxShowError('sio_Tx_empty_irq', ret );
      Exit;
  end;

  InitIrq := true;
end;

procedure ClearIrq(Port:LongInt);
begin
    sio_term_irq(Port, nil, 0);
    sio_cnt_irq(Port, nil, 0);
    sio_modem_irq(Port, nil);
    sio_break_irq(Port, nil);
    sio_Tx_empty_irq(Port, nil);
end;

procedure TIrqForm.Timer1Timer(Sender: TObject);
var
    len : LongInt;
    cnt : LongInt;
    buf : array [0..127] of Char;
begin

    cnt := 0;
    repeat
        len := sio_read(GCommData.Port,@buf,128);
        cnt := cnt+1
    until (len=0) or (cnt>10)
end;

end.
