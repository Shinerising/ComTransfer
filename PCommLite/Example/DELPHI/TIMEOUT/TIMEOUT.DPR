program Timeout;

uses
  Forms,
  About in '..\About.pas' {AboutFrm},
  Config in '..\Config.pas' {CfgForm},
  ExGlobal in '..\ExGlobal.pas',
  MxTool in '..\MxTool.pas',
  TmOutM in 'TmOutM.pas' {TmOutFrm},
  TOThread in 'TOThread.pas',
  WStat in 'WStat.pas' {WStatFrm},
  RStat in 'RStat.pas' {RStatFrm},
  PComm in '..\PComm.pas';

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TTmOutFrm, TmOutFrm);
  Application.CreateForm(TAboutFrm, AboutFrm);
  Application.CreateForm(TCfgForm, CfgForm);
  Application.CreateForm(TWStatFrm, WStatFrm);
  Application.CreateForm(TRStatFrm, RStatFrm);
  Application.Run;
end.
