(*********************************************************************
    Config.pas
     -- Config dialog for com port commnucation parameters


    History:   Date       Author         Comment
               3/9/98     Casper         Wrote it.

**********************************************************************)
unit Config;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms,
  Dialogs,StdCtrls;

type
  TCfgForm = class(TForm)
    GroupBox1: TGroupBox;
    Label1: TLabel;
    cbPort: TComboBox;
    Label2: TLabel;
    cbBaudRate: TComboBox;
    Label3: TLabel;
    cbParity: TComboBox;
    Label4: TLabel;
    cbByteSize: TComboBox;
    Label5: TLabel;
    cbStopBits: TComboBox;
    GroupBox2: TGroupBox;
    chHw: TCheckBox;
    chSw: TCheckBox;
    CfgOK: TButton;
    CfgCancel: TButton;
    GroupBox3: TGroupBox;
    chDtr: TCheckBox;
    chRts: TCheckBox;
    procedure CfgOKClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure chHwClick(Sender: TObject);
    procedure CfgCancelClick(Sender: TObject);
    procedure FormActivate(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  CfgForm: TCfgForm;

implementation

uses ExGlobal;

var
  Gfhw : boolean;

{$R *.DFM}

procedure TCfgForm.FormCreate(Sender: TObject);
var
  i:Word;
begin
  for i:=1 to 256 do
    cbPort.Items.Add('COM'+IntToStr(i));
end;

procedure TCfgForm.FormActivate(Sender: TObject);
begin
  with GCommData do
  begin
    cbPort.ItemIndex     := Port-1;

    cbBaudRate.ItemIndex := ibaudrate;
    cbParity.ItemIndex   := iparity;
    cbByteSize.ItemIndex := ibytesize;
    cbStopBits.ItemIndex := istopbits;

    chHw.Checked  := Hw;
    chSw.Checked  := Sw;
    chRts.Checked := Rts;
    chDtr.Checked := Dtr;

    Gfhw := Hw;
    chRts.Enabled := not Gfhw;

    { disable com port setting when opend }
    cbPort.Enabled := not GbOpen;
  end;
end;


procedure TCfgForm.CfgOKClick(Sender: TObject);
begin
  with GCommData do
  begin
    Port      := cbPort.ItemIndex + 1;

    ibaudrate := cbBaudRate.ItemIndex;
    iparity   := cbParity.ItemIndex;
    ibytesize := cbByteSize.ItemIndex;
    istopbits := cbStopBits.ItemIndex;

    BaudRate  := GBaudTable[ibaudrate];
    ByteSize  := GByteSizeTable[ibytesize];
    Parity    := GParityTable[iparity];
    StopBits  := GStopBitsTable[istopbits];

    Hw  := chHw.Checked;
    Sw  := chSw.Checked;
    Rts := chRts.Checked;
    Dtr := chDtr.Checked;
  end;
  ModalResult := mrOk;
end;

procedure TCfgForm.chHwClick(Sender: TObject);
begin
  chRts.Enabled := Gfhw;
  Gfhw := Not Gfhw;
end;

procedure TCfgForm.CfgCancelClick(Sender: TObject);
begin
  ModalResult := mrCancel;
end;

end.
