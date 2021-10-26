(*********************************************************************
    FSet.pas
     -- Show dialog for user to specify the file transfer
        protocol & upload or download.


     History:   Date       Author         Comment
                3/11/98    Casper         Wrote it.

**********************************************************************)

unit FSet;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ExtCtrls;

type
  TFSetFrm = class(TForm)
    FSetOk: TButton;
    FSetCancel: TButton;
    rgProtocol: TRadioGroup;
    rgDirection: TRadioGroup;
    procedure FSetOkClick(Sender: TObject);
    procedure FSetCancelClick(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  FSetFrm: TFSetFrm;

implementation

{$R *.DFM}

procedure TFSetFrm.FSetOkClick(Sender: TObject);
begin
  ModalResult := mrOk;
end;

procedure TFSetFrm.FSetCancelClick(Sender: TObject);
begin
  ModalResult := mrCancel;
end;

end.
