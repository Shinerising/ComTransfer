program IrqDemo;

uses
  Forms,
  About in '..\About.pas' {AboutFrm},
  Config in '..\Config.pas' {CfgForm},
  ExGlobal in '..\ExGlobal.pas',
  MxTool in '..\MxTool.pas',
  IrqDemoM in 'IrqDemoM.pas' {IrqForm},
  IrqProc in 'IrqProc.pas',
  PComm in '..\PComm.pas';{}

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TIrqForm, IrqForm);
  Application.CreateForm(TAboutFrm, AboutFrm);
  Application.CreateForm(TCfgForm, CfgForm);
  Application.Run;
end.
