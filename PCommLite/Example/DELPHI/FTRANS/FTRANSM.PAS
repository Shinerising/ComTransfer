(*********************************************************************
    FTransM.pas
     -- Main window for file transfer example program.

    Description:
      1.Select "setting..." menu item to set com port option.
      2.Select "Open" menu item to open com port.
        After selected "Open" from menu,you can select
        "File Transfer" form menu to test file transfer function.
        Program will pop up a dialog to display status.
      3.When transferring file,you can push "Cancel" button to
        abort.
      4.Select "Close" menu item to close com port.

    This program demo:
        How to use file transfer function(sio_FtxxxTx,sio_FtxxxRx);
        How to use callback function to display file transfer status;
        How to process file transfer function return code;
        How to abort file transfer process in callback function;

    Use function:
        sio_open,       sio_close,         sio_ioctl,
        sio_flowctrl,   sio_DTR,           sio_RTS,
        sio_FtASCIITx,                     sio_FtASCIIRx,
        sio_FtXmodemCheckSumTx,            sio_FtXmodemCheckSumRx,
        sio_FtXmodemCRCTx,                 sio_FtXmodemCRCRx,
        sio_FtXmodem1KCRCTx,               sio_FtXmodem1KCRCRx,
        sio_FtYmodemTx,                    sio_FtYmodemRx,
        sio_FtZmodemTx,                    sio_FtZmodemRx,
        sio_FtKermitTx,                    sio_FtKermitRx.

    History:   Date       Author         Comment
               3/11/98    Casper         Wrote it.
               12/14/98   Casper         Update.
**********************************************************************)
unit FTransM;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Menus,ExGlobal, StdCtrls;

type
  TFTransForm = class(TExampleForm) { TExampleForm defined in ExGlobal }
    MainMenu1: TMainMenu;
    Port1: TMenuItem;
    cmOpen: TMenuItem;
    cmClose: TMenuItem;
    N1: TMenuItem;
    cmExit: TMenuItem;
    cmSetting: TMenuItem;
    cmHelp: TMenuItem;
    cmAbout: TMenuItem;
    cmFTrans: TMenuItem;
    OpenDlg: TOpenDialog;
    Term: TMemo;
    SaveDlg: TSaveDialog;

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
    procedure XmitFile;
    procedure RecvFile;
    procedure cmFTransClick(Sender: TObject);

  private
    b_busy : boolean;
    { Private declarations }
  public
    { Public declarations }
  end;

var
  FTransForm: TFTransForm;

implementation

{$R *.DFM}
uses PComm,MxTool,Config,FSet,FtProc,About,FStat,DirDlg;

procedure TFTransForm.FormCreate(Sender: TObject);
begin
  with GCommData do
  begin
    Port      := 1;
    ibaudrate := 14;
    iparity   := 0;
    ibytesize := 3;
    istopbits := 0;
    BaudRate  := B38400;
    Parity    := P_NONE;
    ByteSize  := BIT_8;
    StopBits  := STOP_1;
    Hw := false;
    Sw := false;
    Dtr := true;
    Rts := true;
  end;

  GszAppName := 'File Transfer Demo';
  GbOpen := false;
  GhForm := FTransForm;
  b_busy := false;

  SwitchMenu();
end;

procedure TFTransForm.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  if GbOpen then
    ClosePort()
end;

procedure TFTransForm.cmOpenClick(Sender: TObject);
begin
  OpenPort();
end;

procedure TFTransForm.cmCloseClick(Sender: TObject);
begin
  ClosePort();
end;

procedure TFTransForm.cmSettingClick(Sender: TObject);
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

procedure TFTransForm.cmAboutClick(Sender: TObject);
begin
  AboutFrm.ShowModal
end;

procedure TFTransForm.cmExitClick(Sender: TObject);
begin
  Close;
end;

procedure TFTransForm.SwitchMenu;
begin
  cmOpen.Enabled := not GbOpen;
  cmClose.Enabled := GbOpen;
  cmFTrans.Enabled := GbOpen and (not b_busy);
end;

function TFTransForm.OpenPort:Boolean;
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

procedure  TFTransForm.ClosePort;
begin
  sio_close (GCommData.Port);
  GbOpen := False;
  SwitchMenu();
  ShowStatus();
end;

function TFTransForm.PortSet:boolean;
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

procedure TFTransForm.XmitFile;
begin
  OpenDlg.Options := OpenDlg.Options + [ofFileMustExist,ofHideReadOnly];
  if not OpenDlg.Execute() then
    Exit;

  lstrcpy(GxFname,PChar(OpenDlg.FileName));
  GftCancel := false;
  TFtProc.Create(false);
  FStatFrm.ShowModal;
end;

procedure TFTransForm.RecvFile;
begin

  if (GProtocol=FTZMDM) or (GProtocol=FTYMDM) or (GProtocol=FTKERMIT)then
  begin
    {ZModem,YModem,Kermit download protocol will pass file
     name in package,so we only need to specify the directory.}
    if DirFrm.ShowModal() = mrCancel then
      Exit;
    lstrcpy(GrPath,PChar(DirFrm.DirBox.Directory));
    SetCurrentDir(GrPath);
  end
  else
  begin
    SaveDlg.Options := SaveDlg.Options + [ofOverwritePrompt,ofHideReadOnly];
    if not SaveDlg.Execute() then
      Exit;
    lstrcpy(GrFname,PChar(SaveDlg.FileName));
  end;

  GftCancel := false;
  TFtProc.Create(false);
  FStatFrm.ShowModal;
end;

procedure TFTransForm.cmFTransClick(Sender: TObject);
begin
  if FSetFrm.ShowModal = mrCancel then
    Exit;

  GProtocol := FSetFrm.rgProtocol.ItemIndex;
  GDirection := FSetFrm.rgDirection.ItemIndex;
  
  if FSetFrm.rgDirection.ItemIndex = FT_XMIT then
    XmitFile
  else
    RecvFile;
end;

end.
