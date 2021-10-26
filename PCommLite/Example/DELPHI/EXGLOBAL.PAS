(*********************************************************************
    ExGlobal.pas
     -- Global variable & Com port record defined for
        example program.


    History:   Date       Author         Comment
               3/9/98     Casper         Wrote it.

**********************************************************************)
unit ExGlobal;

interface

uses Forms,Menus,StdCtrls,PComm;

type
  TExampleForm = class(TForm)
    Term: TMemo;
    procedure ShowStatus;
  private
    { Private declarations }
  protected
  public
    { Public declarations }
  end;

  TCOMMDATA = record
    Port   : LongInt;
    BaudRate : Integer;
    Parity   : Integer;
    ByteSize : Integer;
    StopBits : Integer;
    ibaudrate : Integer;
    iparity   : Integer;
    ibytesize : Integer;
    istopbits : Integer;
    Hw  : boolean;
    Sw  : boolean;
    Dtr : boolean;
    Rts : boolean;
  end;

var
  { Global variable for example }
  GCommData  : TCOMMDATA;
  GszAppName : string;
  GhForm     : TExampleForm;     { main form }
  GbOpen     : boolean;          { opened ? }
  GhExit     : boolean;          { stop thread ? }

  GBaudTable    :array[0 .. 19] of Integer = (
    B50,B75,B110,B134,B150,B300,B600,B1200,
    B1800,B2400,B4800,B7200,B9600,B19200,B38400,
    B57600,B115200,B230400,B460800,B921600
  );

  GParityTable  :array[0..4] of Integer = (
    P_NONE,P_EVEN,P_ODD,P_MRK,P_SPC
  );
  GByteSizeTable:array[0..3] of Integer =(
    BIT_5,BIT_6,BIT_7,BIT_8
  );

  GStopBitsTable:array[0..1] of Integer = (
    STOP_1,STOP_2
  );

  GstrBaudTable    :array[0..19] of string = (
    '50','75','110','134','150','300',
    '600','1200','1800','2400','4800','7200',
    '9600','19200','38400','57600','115200',
    '230400','460800','921600'
  );

  GstrParityTable  :array[0..4] of string = (
    'None','Odd','Even','Mark','Space'
  );

  GstrByteSizeTable:array[0..3] of string = (
    '5','6','7','8'
  );

  GstrStopBitsTable:array[0..1] of string = (
    '1','2'
  );

implementation

uses SysUtils;

procedure TExampleForm.ShowStatus;
var
  szMessage : string;
begin
  szMessage := GszAppName;
  if GbOpen then
  begin
    with GCommData do
    begin
      szMessage := szMessage + ' -- COM' + IntToStr(Port) + ',';
      szMessage := szMessage +
              GstrBaudTable[ibaudrate] + ',';
      szMessage := szMessage +
              GstrParityTable[iparity] + ',';
      szMessage := szMessage +
              GstrByteSizeTable[ibytesize] + ',';
      szMessage := szMessage +
              GstrStopBitsTable[istopbits];
      if Hw then
        szMessage := szMessage + ',RTS/CTS';
      if Sw then
        szMessage := szMessage + ',XON/XOFF';
    end;
  end;
  Caption := szMessage;
end;

end.
