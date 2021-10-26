(*********************************************************************
    StatDlg.pas
     -- Show dialog to display port status.

    History:   Date       Author         Comment
               3/11/98    Casper         Wrote it.

***********************************************************************)


unit StatDlg;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ExtCtrls;

type
  TStatDlgFrm = class(TForm)
    Label1: TLabel;
    edPort: TEdit;
    GroupBox1: TGroupBox;
    Label2: TLabel;
    Label3: TLabel;
    edBaudRate: TEdit;
    edParity: TEdit;
    Label4: TLabel;
    Label5: TLabel;
    edByteSize: TEdit;
    edStopBits: TEdit;
    GroupBox2: TGroupBox;
    edFlowCtrl: TEdit;
    GroupBox3: TGroupBox;
    edCts: TEdit;
    edDsr: TEdit;
    edRi: TEdit;
    edDcd: TEdit;
    GroupBox4: TGroupBox;
    Label6: TLabel;
    Label7: TLabel;
    edInqueue: TEdit;
    edOutqueue: TEdit;
    GroupBox5: TGroupBox;
    lbDStat: TListBox;
    GroupBox6: TGroupBox;
    lbTxHold: TListBox;
    StatDlgCancel: TButton;
    UpdateTimer: TTimer;
    procedure StatDlgCancelClick(Sender: TObject);
    procedure RefreshStat;
    procedure UpdateTimerTimer(Sender: TObject);
    procedure FormActivate(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormCreate(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  StatDlgFrm: TStatDlgFrm;

implementation

uses PComm,ExGlobal;

Const DATABITSMSK = BIT_5 or BIT_6 or BIT_7 or BIT_8;
Const STOPBITSMSK = STOP_1 or STOP_2;
Const PARITYMSK = P_EVEN or P_ODD or P_SPC or P_MRK or P_NONE;

{$R *.DFM}

procedure TStatDlgFrm.StatDlgCancelClick(Sender: TObject);
begin
  Close
end;

procedure TStatDlgFrm.RefreshStat();
var
  port : LongInt;
  buf  : string;
  mode,bytesize,stopbits,parity : Word;
  lstatus  : LongInt;
  txhold   : LongInt;
  d_status : LongInt;
  flow     : LongInt;
  i        : Word;

begin

  port := GCommData.port;
  {sio_data_status();This should be called first !! }
  d_status := sio_data_status(port);
  if (d_status and $1) > 0 then
      lbDStat.Items.Add('parity error');

  if (d_status and $2) > 0 then
      lbDStat.Items.Add('framing error');

  if (d_status and $4) > 0 then
      lbDStat.Items.Add('overrun error');

  if (d_status And $8) > 0 then
      lbDStat.Items.Add ('overflow error');

  edPort.Text := 'COM' + IntToStr(port);

  { sio_getbaud() }
  edBaudRate.Text:= IntTostr(sio_getbaud(port));

  { sio_getmode() }
  mode := sio_getmode(port);
  parity := mode and PARITYMSK;
  bytesize := mode and DATABITSMSK;
  stopbits := mode and STOPBITSMSK;
  for i := 0 to 4 do
    if parity = GParityTable[i] Then
    begin
      edParity.Text:= GstrParityTable[i];
      break;
    end;

  for i := 0 to 3 do
    if bytesize = GByteSizeTable[i] then
    begin
      edByteSize.Text := GstrByteSizeTable[i];
      break;
    end;

  for i := 0 to 1 do
    if stopbits = GStopBitsTable[i] then
    begin
      edStopBits.Text := GstrStopBitsTable[i];
      break;
    end;

  { sio_getflow() }
  flow := sio_getflow(port);
  if flow = 0 then
    edFlowCtrl.Text := 'None'
  else
  begin
    buf := '';
    if (flow and $1) > 0 then
      buf := buf + '<RTS>';
    if (flow and $2) > 0 then
      buf := buf + '<CTS>';
    if (flow and $4) > 0 then
      buf := buf + '<Tx XON/XOFF>';
    if (flow and $8) > 0 then
      buf := buf + '<Rx XON/XOFF>';

    edFlowCtrl.Text := buf;
  end;

  { sio_lstatus() }
  lstatus := sio_lstatus(port);
  if (lstatus and S_CTS) > 0 then
    edCts.Text := 'CTS'
  else
    edCts.Text := 'cts';

  if (lstatus and S_DSR) > 0 then
    edDsr.Text := 'DSR'
  else
    edDsr.Text := 'dsr';

  if (lstatus and S_RI) > 0 then
    edRi.Text := 'RI'
  else
    edRi.Text := 'ri';

  if (lstatus and S_CD) > 0 then
    edDcd.Text := 'DCD'
  else
    edDcd.Text := 'dcd';

  { sio_iqueue() }
  edInqueue.Text := IntToStr(sio_iqueue(port));

  { sio_iqueue() }
  edOutqueue.Text := IntToStr(sio_oqueue(port));

  { sio_Tx_hold }
  lbTxhold.Clear();
  txhold := sio_Tx_hold(port);
  if (txhold and $1)>0 then
    lbTxhold.Items.Add ('CTS is low');

  if (txhold and $2)>0 then
    lbTxhold.Items.Add('XOFF char received');

  if (txhold and $4)>0 then
    lbTxhold.Items.Add('by sio_disableTx()');

end;

procedure TStatDlgFrm.UpdateTimerTimer(Sender: TObject);
begin
  RefreshStat;
end;

procedure TStatDlgFrm.FormActivate(Sender: TObject);
begin
  UpdateTimer.Enabled := true;
end;

procedure TStatDlgFrm.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  UpdateTimer.Enabled := false;
end;

procedure TStatDlgFrm.FormCreate(Sender: TObject);
begin
  lbDStat.Items.Clear();

end;

end.
