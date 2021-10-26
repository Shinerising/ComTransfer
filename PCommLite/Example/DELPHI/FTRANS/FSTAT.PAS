(*********************************************************************
    FStat.pas
     -- Show dialog to diaplsy file transfer status.


     History:   Date       Author         Comment
                3/11/98    Casper         Wrote it.

**********************************************************************)

unit FStat;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ExtCtrls;

type
  TFStatFrm = class(TForm)
    Label1: TLabel;
    lbPort: TLabel;
    Label2: TLabel;
    lbFSize: TLabel;
    Label3: TLabel;
    lbProtocol: TLabel;
    Label4: TLabel;
    lbxLen: TLabel;
    Label5: TLabel;
    lbFname: TLabel;
    FStatCancel: TButton;
    Bevel1: TBevel;
    Bevel2: TBevel;
    Bevel3: TBevel;
    Bevel4: TBevel;
    Bevel5: TBevel;
    procedure FStatCancelClick(Sender: TObject);
    procedure RefreshDlg(xlen:LongInt;flen:LongInt;fname:string);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  FStatFrm: TFStatFrm;

implementation

uses FtProc,ExGlobal;
{$R *.DFM}

procedure TFStatFrm.FStatCancelClick(Sender: TObject);
begin
  {File transfer thread will check 'GftCancel' flag.}
  GftCancel :=true;
end;

procedure TFStatFrm.RefreshDlg(xlen:LongInt;flen:LongInt;fname:string);
begin
  lbFSize.Caption := IntToStr(flen);
  lbPort.Caption := IntToStr(GCommData.Port);
  lbProtocol.Caption := GstrProtocol[GProtocol];
  lbFName.Caption := fname;
  lbxLen.Caption := IntToStr(xlen);
end;

end.
