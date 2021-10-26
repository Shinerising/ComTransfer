(*********************************************************************
    TOThread.pas
     -- Read & write thread for timeout demo example program.

    History:   Date       Author         Comment
               3/11/98    Casper         Wrote it.

**********************************************************************)

unit TOThread;

interface

uses
  Classes;

const BUFLEN = 1024 * 10;  

type
  TWTOThread = class(TThread)
  private
    { Private declarations }
  protected
    procedure Update;
    procedure Execute; override;
  end;

  TRTOThread = class(TThread)
  private
    { Private declarations }
  protected
    procedure Update;
    procedure Execute; override;
  end;

implementation

uses Windows,SysUtils,PComm,ExGlobal,WStat,RStat;
var
  _GBuf : array[0..BUFLEN+1] of Char;
  GCount     :LongInt;
  GCallCount :LongInt;
  GDifTime   :LongInt;


{ TWTOThread }

procedure TWTOThread.Update;
begin
  WStatFrm.edWCount.Text:= IntToStr(GCount);
  WStatFrm.edWCallCount.Text:= IntToStr(GCallCount);
  WStatFrm.edWTime.Text:= IntToStr(GDifTime) + 'ms';
end;

procedure TWTOThread.Execute;
var
  i : Word;
  t1   : LongInt;
begin
  { Place thread code here }

  for i:=0 to BUFLEN-1 do
    _GBuf[i] := Char(Ord('0')+(i mod 10));

  GCount := 0;
  GCallCount := 0;
  GDifTime := 0;
  Synchronize(Update);

  Sleep(1000);
  while not GhExit do
  begin
    t1 := GetCurrentTime();
    if (sio_write(GCommData.Port,_GBuf,BUFLEN)>0) then
      Inc(GCount);
    Inc(GCallCount);
    GDifTime := GetCurrentTime()-t1;
    Synchronize(Update);
  end;
end;



{ TRTOThread }

procedure TRTOThread.Update;
begin
  RStatFrm.edRCount.Text:= IntToStr(GCount);
  RStatFrm.edRCallCount.Text:= IntToStr(GCallCount);
  RStatFrm.edRTime.Text:= IntToStr(GDifTime) + 'ms';
end;

procedure TRTOThread.Execute;
var
  t1 : LongInt;
begin
  GCount  := 0;
  GCallCount  := 0;
  GDifTime := 0;
  Synchronize(Update);

  Sleep(1000);
  while not GhExit do
  begin
    t1 := GetCurrentTime();
    if (sio_read(GCommData.Port,_GBuf,BUFLEN-1) > 0) then
      Inc(GCount);
    Inc(GCallCount);
    GDifTime := GetCurrentTime()-t1;
    Synchronize(Update);
  end;
end;


end.
