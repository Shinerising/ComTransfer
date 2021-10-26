program PortStat;

uses
  Forms,
  About in '..\About.pas' {AboutFrm},
  Config in '..\Config.pas' {CfgForm},
  ExGlobal in '..\ExGlobal.pas',
  MxTool in '..\MxTool.pas',
  ReadThd in '..\ReadThd.pas',
  PStatM in 'PStatM.pas' {PStatFrm},
  StatDlg in 'StatDlg.pas' {StatDlgFrm},
  PComm in '..\PComm.pas';

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TPStatFrm, PStatFrm);
  Application.CreateForm(TAboutFrm, AboutFrm);
  Application.CreateForm(TCfgForm, CfgForm);
  Application.CreateForm(TStatDlgFrm, StatDlgFrm);
  Application.Run;
end.
