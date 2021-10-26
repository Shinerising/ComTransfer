(*********************************************************************
    DirDlg.pas
     -- show dialog for user to select directory.
        (File transfer example)


     History:   Date       Author         Comment
                3/11/98    Casper         Wrote it.

**********************************************************************)

unit DirDlg;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, FileCtrl;

type
  TDirFrm = class(TForm)
    DriveBox: TDriveComboBox;
    DirBox: TDirectoryListBox;
    DirDlgOK: TButton;
    DirDlgCancel: TButton;
    Label1: TLabel;
    DirResult: TLabel;
    procedure DirDlgOKClick(Sender: TObject);
    procedure DirDlgCancelClick(Sender: TObject);
    procedure DirBoxChange(Sender: TObject);
    procedure DriveBoxChange(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  DirFrm: TDirFrm;

implementation

{$R *.DFM}

procedure TDirFrm.DirDlgOKClick(Sender: TObject);
begin
  ModalResult := mrOk;
end;

procedure TDirFrm.DirDlgCancelClick(Sender: TObject);
begin
  ModalResult := mrCancel;
end;

procedure TDirFrm.DirBoxChange(Sender: TObject);
begin
  DirResult.Caption := DirBox.Directory;
end;

procedure TDirFrm.DriveBoxChange(Sender: TObject);
begin
  DirBox.Drive := DriveBox.Drive;
end;

end.
