(*********************************************************************
    IrqProc.pas
     -- Irq callback function for irq function demo example.


    History:   Date       Author         Comment
               3/9/98     Casper         Wrote it.

**********************************************************************)

unit IrqProc;

interface

var
  GIdx          : LongInt;
  GTermIrqCnt   : LongInt;
  GCntIrqCnt    : LongInt;
  GModemIrqCnt  : LongInt;
  GBreakIrqCnt  : LongInt;
  GTxEmptyIrqCnt: LongInt;

{ irq function must be declared stdcall }  
procedure TermIrq(Port:LongInt);stdcall;
procedure CntIrq(Port:LongInt);stdcall;
procedure ModemIrq(Port:LongInt);stdcall;
procedure BreakIrq(Port:LongInt);stdcall;
procedure TxEmptyIrq(Port:LongInt);stdcall;


implementation

uses SysUtils,ExGlobal;

{ show irq count } 
procedure ShowCnt(cnt:LongInt;title:string);
var
  lend : LongInt;
  buf : string;
begin
  Inc(GIdx);
  lend := Length(GhForm.Term.Text);

  if(lend>25000)then
  begin
    { Edit Control buffer size limit }
    GhForm.Term.Clear();
    lend := 0;
  end;

  GhForm.Term.SelStart := lend;
  GhForm.Term.SelLength := 0;
  buf := '(idx=' + IntToStr(Gidx) + ')' + title + 'count=' +
      IntToStr(cnt) + Chr(13) + Chr(10);
  GhForm.Term.SelText := buf;
end;

procedure TermIrq(Port:LongInt);stdcall;
begin
  GTermIrqCnt := GTermIrqCnt + 1;
  ShowCnt(GTermIrqCnt, 'sio_term_irq()');
  {
    When got this event,program will also
    got 'Rx event'(sio_cnt_irq) at the same time.
  }
end;

procedure CntIrq(Port:LongInt);stdcall;
begin
  GCntIrqCnt := GCntIrqCnt + 1;
  ShowCnt(GCntIrqCnt, 'sio_cnt_irq()');
end;

procedure ModemIrq(Port:LongInt);stdcall;
begin
  GModemIrqCnt := GModemIrqCnt + 1;
  ShowCnt(GModemIrqCnt, 'sio_modem_irq()');
end;

procedure BreakIrq(Port:LongInt);stdcall;
begin
  GBreakIrqCnt := GBreakIrqCnt + 1;
  ShowCnt(GBreakIrqCnt, 'sio_break_irq()');
end;

procedure TxEmptyIrq(Port:LongInt);stdcall;
begin
  GTxEmptyIrqCnt := GTxEmptyIrqCnt + 1;
  ShowCnt(GTxEmptyIrqCnt, 'sio_Tx_empty_irq()');
end;


end.
