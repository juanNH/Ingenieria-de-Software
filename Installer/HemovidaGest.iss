[Setup]
AppId=HemovidaGest
AppName=HemovidaGest
AppVersion=1.0
DefaultDirName={localappdata}\Programs\HemovidaGest
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\output\installer
OutputBaseFilename=HemovidaGest-Setup
WizardStyle=modern

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Files]
Source: "InstalarBase.ps1"; Flags: dontcopy
Source: "..\Database\00_tecni_salud_schema.sql"; Flags: dontcopy
Source: "..\Database\StoredProcedures\*.sql"; Excludes: "12_DiagramasSSMS.sql,99_I18n_Reset_Es_En.sql"; Flags: dontcopy
Source: "..\UI\bin\Release\UI.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\UI\bin\Release\UI.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\UI\bin\Release\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{tmp}\App.local.config"; DestDir: "{app}"; Flags: external ignoreversion; ExternalSize: 4096

[Icons]
Name: "{autoprograms}\HemovidaGest"; Filename: "{app}\UI.exe"

[Run]
Filename: "{app}\UI.exe"; Description: "Abrir HemovidaGest"; Flags: nowait postinstall skipifsilent

[Code]
var
  InstanciaSQL: TNewComboBox;
  BasePreparada: String;

function EjecutarScriptSQL(const Argumentos: String; var Codigo: Integer; var Salida: TExecOutput): Boolean;
var
  RutaScript: String;
begin
  RutaScript := ExpandConstant('{tmp}\InstalarBase.ps1');
  if not FileExists(RutaScript) then
    ExtractTemporaryFile('InstalarBase.ps1');
  Result := ExecAndCaptureOutput(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'),
    '-NoProfile -NonInteractive -ExecutionPolicy Bypass -File ' + AddQuotes(RutaScript) + ' ' + Argumentos,
    ExpandConstant('{tmp}'), SW_HIDE, ewWaitUntilTerminated, Codigo, Salida);
end;

procedure InitializeWizard;
var
  PaginaSQL: TWizardPage;
  AyudaSQL: TNewStaticText;
  Codigo, I: Integer;
  Salida: TExecOutput;
  Instancia: String;
begin
  PaginaSQL := CreateCustomPage(wpSelectDir, 'SQL Server',
    'Elegí dónde se creará la base HemovidaGest.');

  AyudaSQL := TNewStaticText.Create(PaginaSQL);
  AyudaSQL.Parent := PaginaSQL.Surface;
  AyudaSQL.AutoSize := False;
  AyudaSQL.Width := PaginaSQL.SurfaceWidth;
  AyudaSQL.WordWrap := True;
  AyudaSQL.Caption := 'Elegí una instancia local en ejecución o escribí una como SERVIDOR\INSTANCIA. El punto (.) indica la instancia local predeterminada.';
  WizardForm.AdjustLabelHeight(AyudaSQL);

  InstanciaSQL := TNewComboBox.Create(PaginaSQL);
  InstanciaSQL.Parent := PaginaSQL.Surface;
  InstanciaSQL.Top := AyudaSQL.Top + AyudaSQL.Height + ScaleY(12);
  InstanciaSQL.Width := PaginaSQL.SurfaceWidth;
  InstanciaSQL.Style := csDropDown;
  InstanciaSQL.Text := ExpandConstant('{param:SQLSERVER|}');

  if EjecutarScriptSQL('-DetectarInstancias', Codigo, Salida) and (Codigo = 0) then
  begin
    for I := 0 to GetArrayLength(Salida.StdOut) - 1 do
    begin
      Instancia := Trim(Salida.StdOut[I]);
      if Instancia = '' then Continue;
      InstanciaSQL.Items.Add(Instancia);
      Log('Instancia local detectada: ' + Instancia);
    end;
  end
  else
    Log('No se pudo detectar instancias de SQL Server.');

  if (InstanciaSQL.Text = '') and (InstanciaSQL.Items.Count = 1) and not WizardSilent then
    InstanciaSQL.ItemIndex := 0;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  Servidor, Parametros: String;
  Codigo, I: Integer;
  Salida: TExecOutput;
begin
  Result := '';
  if not IsDotNetInstalled(net48, 0) then
  begin
    Result := 'Instalá .NET Framework 4.8 y volvé a abrir este instalador.';
    Exit;
  end;

  Servidor := Trim(InstanciaSQL.Text);
  if Servidor = '' then
  begin
    Result := 'Ingresá la instancia de SQL Server.';
    Exit;
  end;
  if (Pos('"', Servidor) > 0) or (Servidor[Length(Servidor)] = '\') then
  begin
    Result := 'Revisá el nombre de la instancia de SQL Server.';
    Exit;
  end;
  if BasePreparada = Servidor then Exit;

  ExtractTemporaryFiles('*.sql');
  Parametros := '-Servidor ' + AddQuotes(Servidor) + ' -Configuracion ' +
    AddQuotes(ExpandConstant('{tmp}\App.local.config'));

  if not EjecutarScriptSQL(Parametros, Codigo, Salida) then
  begin
    Result := 'No se pudo ejecutar Windows PowerShell. ' + SysErrorMessage(Codigo);
    Exit;
  end;

  for I := 0 to GetArrayLength(Salida.StdOut) - 1 do
    Log(Salida.StdOut[I]);
  if Codigo <> 0 then
  begin
    Result := 'No se pudo preparar HemovidaGest en ' + Servidor + '.';
    for I := 0 to GetArrayLength(Salida.StdErr) - 1 do
      Result := Result + #13#10 + Salida.StdErr[I];
    Exit;
  end;
  BasePreparada := Servidor;
end;
