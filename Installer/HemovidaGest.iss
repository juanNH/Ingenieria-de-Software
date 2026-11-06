#ifndef LocalDbMsi
  #define LocalDbMsi "..\output\installer\SqlLocalDB.msi"
  #if !FileExists(AddBackslash(SourcePath) + LocalDbMsi)
    #error Copia SqlLocalDB.msi a output\installer antes de compilar.
  #endif
#endif

#ifndef VcRedistX64
  #define VcRedistX64 "..\output\installer\vc_redist.x64.exe"
  #if !FileExists(AddBackslash(SourcePath) + VcRedistX64)
    #error Copia vc_redist.x64.exe a output\installer antes de compilar.
  #endif
#endif

#ifndef VcRedistX86
  #define VcRedistX86 "..\output\installer\vc_redist.x86.exe"
  #if !FileExists(AddBackslash(SourcePath) + VcRedistX86)
    #error Copia vc_redist.x86.exe a output\installer antes de compilar.
  #endif
#endif

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
Source: "DatosPrueba.sql"; Flags: dontcopy
Source: "{#LocalDbMsi}"; DestName: "SqlLocalDB.msi"; Flags: dontcopy
Source: "{#VcRedistX64}"; DestName: "vc_redist.x64.exe"; Flags: dontcopy
Source: "{#VcRedistX86}"; DestName: "vc_redist.x86.exe"; Flags: dontcopy
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
const
  InstanciaLocalDB = '(localdb)\HemovidaGest';

var
  InstanciaSQL: TNewComboBox;
  IncluirDatosPrueba: TNewCheckBox;
  BasePreparada: String;

function EjecutarScriptSQL(const Argumentos: String; var Codigo: Integer; var Salida: TExecOutput): Boolean;
var
  RutaScript: String;
begin
  RutaScript := ExpandConstant('{tmp}\InstalarBase.ps1');
  if not FileExists(RutaScript) then
    ExtractTemporaryFile('InstalarBase.ps1');
  Result := ExecAndCaptureOutputWithNativeSysDir(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'),
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

  InstanciaSQL := TNewComboBox.Create(PaginaSQL);
  InstanciaSQL.Parent := PaginaSQL.Surface;
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

  if InstanciaSQL.Items.Count = 0 then
  begin
    InstanciaSQL.Items.Add(InstanciaLocalDB);
    if Trim(InstanciaSQL.Text) = '' then
      InstanciaSQL.ItemIndex := 0;
    AyudaSQL.Caption := 'No se detectó una instancia local en ejecución. Se preparará ' +
      InstanciaLocalDB + ' si dejás esa opción. También podés escribir otra instancia.';
  end
  else if (Trim(InstanciaSQL.Text) = '') and (InstanciaSQL.Items.Count = 1) and not WizardSilent then
    InstanciaSQL.ItemIndex := 0;

  WizardForm.AdjustLabelHeight(AyudaSQL);
  InstanciaSQL.Top := AyudaSQL.Top + AyudaSQL.Height + ScaleY(12);

  IncluirDatosPrueba := TNewCheckBox.Create(PaginaSQL);
  IncluirDatosPrueba.Parent := PaginaSQL.Surface;
  IncluirDatosPrueba.Top := InstanciaSQL.Top + InstanciaSQL.Height + ScaleY(12);
  IncluirDatosPrueba.Width := PaginaSQL.SurfaceWidth;
  IncluirDatosPrueba.Height := ScaleY(20);
  IncluirDatosPrueba.Caption := 'Cargar datos ficticios de demostración';
  IncluirDatosPrueba.Checked := False;
end;

function RuntimeVisualCppDisponible(const Directorio, Arquitectura: String): Boolean;
begin
  Result := FileExists(Directorio + '\vcruntime140.dll') and
    FileExists(Directorio + '\msvcp140.dll');
  if Arquitectura = 'x64' then
    Result := Result and FileExists(Directorio + '\vcruntime140_1.dll');
end;

function AsegurarRuntimeVisualCpp(const Arquitectura, Directorio: String;
  var NeedsRestart: Boolean): String;
var
  Archivo, Ruta: String;
  Codigo: Integer;
begin
  Result := '';
  if RuntimeVisualCppDisponible(Directorio, Arquitectura) then Exit;
  if WizardSilent then
  begin
    Result := 'Abrí el asistente en modo interactivo para instalar Microsoft Visual C++ ' + Arquitectura + '.';
    Exit;
  end;

  Archivo := 'vc_redist.' + Arquitectura + '.exe';
  Ruta := ExpandConstant('{tmp}\' + Archivo);
  if not FileExists(Ruta) then
    ExtractTemporaryFile(Archivo);
  if not ShellExec('runas', Ruta, '/install /quiet /norestart',
    ExpandConstant('{tmp}'), SW_HIDE, ewWaitUntilTerminated, Codigo) then
  begin
    Result := 'No se pudo abrir el instalador de Microsoft Visual C++ ' +
      Arquitectura + '. ' + SysErrorMessage(Codigo);
    Exit;
  end;

  Log('Resultado del instalador de Microsoft Visual C++ ' + Arquitectura + ': ' + IntToStr(Codigo));
  if (Codigo = 3010) or (Codigo = 1641) then
  begin
    NeedsRestart := True;
    Result := 'Reiniciá Windows y volvé a abrir este instalador.';
  end
  else if not RuntimeVisualCppDisponible(Directorio, Arquitectura) then
    Result := 'No se pudo preparar Microsoft Visual C++ ' + Arquitectura +
      '. Código ' + IntToStr(Codigo) + '.';
end;

function AsegurarLocalDB(var NeedsRestart: Boolean): String;
var
  Codigo: Integer;
  Salida: TExecOutput;
begin
  Result := '';
  if IsX64OS then
  begin
    Result := AsegurarRuntimeVisualCpp('x64', ExpandConstant('{sysnative}'), NeedsRestart);
    if Result <> '' then Exit;
    Result := AsegurarRuntimeVisualCpp('x86', ExpandConstant('{syswow64}'), NeedsRestart);
    if Result <> '' then Exit;
  end;

  if not EjecutarScriptSQL('-VerificarLocalDB', Codigo, Salida) then
  begin
    Result := 'No se pudo ejecutar Windows PowerShell. ' + SysErrorMessage(Codigo);
    Exit;
  end;
  if Codigo = 0 then Exit;
  if Codigo <> 2 then
  begin
    Result := 'No se pudo verificar la instalación de SQL Server Express LocalDB.';
    if GetArrayLength(Salida.StdErr) > 0 then
      Result := Result + #13#10 + Salida.StdErr[0];
    Exit;
  end;
  if not IsX64OS then
  begin
    Result := 'El motor LocalDB incluido requiere Windows x64. Ingresá una instancia SQL Server existente.';
    Exit;
  end;
  if WizardSilent then
  begin
    Result := 'Abrí el asistente en modo interactivo para instalar SQL Server Express LocalDB y aceptar su licencia.';
    Exit;
  end;

  ExtractTemporaryFile('SqlLocalDB.msi');
  if not ShellExec('runas', ExpandConstant('{sys}\msiexec.exe'),
    '/i ' + AddQuotes(ExpandConstant('{tmp}\SqlLocalDB.msi')) + ' /norestart',
    ExpandConstant('{tmp}'), SW_SHOWNORMAL, ewWaitUntilTerminated, Codigo) then
  begin
    Result := 'No se pudo abrir el instalador de LocalDB. ' + SysErrorMessage(Codigo);
    Exit;
  end;
  Log('Resultado del instalador de LocalDB: ' + IntToStr(Codigo));
  if (Codigo = 3010) or (Codigo = 1641) then
  begin
    NeedsRestart := True;
    Result := 'Reiniciá Windows y volvé a abrir este instalador.';
  end
  else if Codigo = 1602 then
    Result := 'Se canceló la instalación de SQL Server Express LocalDB.'
  else if Codigo <> 0 then
    Result := 'No se pudo instalar SQL Server Express LocalDB. Código ' + IntToStr(Codigo) + '.';
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

  if CompareText(Servidor, InstanciaLocalDB) = 0 then
  begin
    Result := AsegurarLocalDB(NeedsRestart);
    if Result <> '' then Exit;
  end;

  ExtractTemporaryFiles('*.sql');
  Parametros := '-Servidor ' + AddQuotes(Servidor) + ' -Configuracion ' +
    AddQuotes(ExpandConstant('{tmp}\App.local.config'));
  if CompareText(Servidor, InstanciaLocalDB) = 0 then
    Parametros := Parametros + ' -PrepararLocalDB';
  if IncluirDatosPrueba.Checked then
    Parametros := Parametros + ' -DatosPrueba';

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
