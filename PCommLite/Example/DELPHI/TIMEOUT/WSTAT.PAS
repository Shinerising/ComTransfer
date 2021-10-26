(*********************************************************************
    WStat.pas
     -- Show dialog to diaplsy write timeout status.


    History:   Date       Author         Comment
               3/11/98    Casper         Wrote it.

**********************************************************************)

unit WStat;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls;

type
  TWStatFrm = class(TForm)
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    edWCount: TEdit;
    edWCallCount: TEdit;
    edWTime: TEdit;
    WStatCancel: TButton;
    procedure WStatCancelClick(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  WStatFrm: TWStatFrm;

implementation

uses ExGlobal;

{$R *.DFM}

procedure TWStatFrm.WStatCancelClick(Sender: TObject);
begin
  GhExit := True;
  Close();
end;

end.
