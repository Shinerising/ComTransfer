program Simple;

uses
  Forms,
  SimpleM in 'SimpleM.pas' {SimpleForm},
  MxTool in '..\MxTool.pas',
  ExGlobal in '..\ExGlobal.pas',
  ReadThd in '..\ReadThd.pas',
  Config in '..\Config.pas' {CfgForm},
  About in '..\About.pas' {AboutFrm},
  PComm in '..\Pcomm.pas';

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TSimpleForm, SimpleForm);
  Application.CreateForm(TCfgForm, CfgForm);
  Application.CreateForm(TAboutFrm, AboutFrm);
  Application.Run;
end.
