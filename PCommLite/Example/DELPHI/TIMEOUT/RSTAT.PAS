(*********************************************************************
    RStat.pas
     -- Show dialog to diaplsy read timeout status.


    History:   Date       Author         Comment
               3/11/98    Casper         Wrote it.

**********************************************************************)


unit RStat;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls;

type
  TRStatFrm = class(TForm)
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    edRCount: TEdit;
    edRCallCount: TEdit;
    edRTime: TEdit;
    RStatCancel: TButton;
    procedure RStatCancelClick(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  RStatFrm: TRStatFrm;

implementation

uses ExGlobal;
{$R *.DFM}


procedure TRStatFrm.RStatCancelClick(Sender: TObject);
begin
  GhExit := True;
  Close();
end;

end.
