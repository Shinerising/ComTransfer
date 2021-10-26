program FTrans;

uses
  Forms,
  FtransM in 'FtransM.pas' {FTransForm},
  About in '..\About.pas' {AboutFrm},
  Config in '..\Config.pas' {CfgForm},
  ExGlobal in '..\ExGlobal.pas',
  MxTool in '..\MxTool.pas',
  FSet in 'FSet.pas' {FSetFrm},
  FStat in 'FStat.pas' {FStatFrm},
  FtProc in 'FtProc.pas',
  DirDlg in 'DirDlg.pas' {DirFrm},
  PComm in '..\PComm.pas';

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TFTransForm, FTransForm);
  Application.CreateForm(TAboutFrm, AboutFrm);
  Application.CreateForm(TCfgForm, CfgForm);
  Application.CreateForm(TFSetFrm, FSetFrm);
  Application.CreateForm(TFStatFrm, FStatFrm);
  Application.CreateForm(TDirFrm, DirFrm);
  Application.Run;
end.
