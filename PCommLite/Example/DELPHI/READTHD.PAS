(*********************************************************************
    ReadThd.pas
     -- Read Thread for example program.

    History:   Date       Author         Comment
               3/9/98     Casper         Wrote it.

**********************************************************************)

unit ReadThd;

interface

uses  Classes;

type
  TReadThread = class(TThread)
  private
    m_buf : array [0..511] of Char;

    { Private declarations }
  protected
    procedure Execute; override;
    procedure ShowData;
  end;

implementation

uses Windows,PComm,ExGlobal;

{ TReadThread }
procedure TReadThread.ShowData;
var
  lend : LongInt;
begin
  (*
    When got any data,dump buffer to Edit window.

    NOTE:
    If any Null character in buffer,
    characters after null can't be dumped
    to Edit window.
  *)

  lend := Length(GhForm.Term.Text);

  if(lend>25000)then
  begin
    { Edit Control buffer size limit }
    GhForm.Term.Text := string(m_buf);
    Exit;
  end;

  GhForm.Term.SelStart := lend;
  GhForm.Term.SelLength := 0;
  GhForm.Term.SelText := string(m_buf);
end;

procedure TReadThread.Execute;
var
  len : LongInt;
begin
  (* before close port,set GhExit to true to terminate
     the read thread *)
  while not GhExit do
  begin
    Sleep(10);
    len := sio_read(GCommData.Port,@m_buf,511);
    if (len>0) then
    begin
      m_buf[len] := Char(0);{null terminated string}
      Synchronize(ShowData);
    end
  end;
end;

end.
