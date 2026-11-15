param([string]$Servidor = '.', [string]$SalidaDir, [switch]$SoloUI, [switch]$SinCapturas, [switch]$CapturaFinal)
$ErrorActionPreference='Stop'
if ($SinCapturas -and !$SoloUI) { throw 'SinCapturas se usa con SoloUI; la integracion SQL genera la evidencia de la pantalla final.' }
if ($CapturaFinal -and $SoloUI) { throw 'CapturaFinal requiere la integracion SQL para obtener evidencia de datos reales de prueba.' }
$taskRoot=Split-Path $PSScriptRoot -Parent
if (!$SalidaDir) { $SalidaDir=Join-Path $taskRoot 'output/abastecimiento-2026-11-15/ui' }
$taskOutput=[IO.Path]::GetFullPath($SalidaDir)
[void][IO.Directory]::CreateDirectory($taskOutput)
$taskBuild=Join-Path $taskOutput 'build'
$taskMsBuild='C:/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe'
& $taskMsBuild (Join-Path $PSScriptRoot 'Abastecimiento.UI.Tests.csproj') /t:Build /p:Configuration=Release ('/p:OutDir='+$taskBuild+'/') /verbosity:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Fallo la compilacion del runnerUI.' }
$taskTempParent=[IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$taskTemp=Join-Path $taskTempParent ('CodexAbastUi_'+[Guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($taskTemp)
try {
    Get-ChildItem -LiteralPath $taskBuild -File | Where-Object { $_.Extension -in @('.dll','.exe') } | Copy-Item -Destination $taskTemp
    $taskRunner=Join-Path $taskTemp 'Abastecimiento.UI.Tests.exe'
    $taskArguments=@('--unit', ('"'+$taskOutput+'"'))
    if ($SinCapturas -or $CapturaFinal) { $taskArguments += '--no-captures' }
    $taskProcess=Start-Process -FilePath $taskRunner -ArgumentList $taskArguments -WindowStyle Hidden -Wait -PassThru -RedirectStandardOutput (Join-Path $taskOutput 'unit.log') -RedirectStandardError (Join-Path $taskOutput 'unit.err')
    if ($taskProcess.ExitCode -ne 0) { throw ('Fallo el runnerUI. Ver '+(Join-Path $taskOutput 'unit.err')) }
    if (!$SoloUI) {
        & (Join-Path $PSScriptRoot 'Abastecimiento.Tests.ps1') -Servidor $Servidor -Salida (Join-Path $taskOutput 'sql-integration-tests.json') -UiRunner $taskRunner -UiCapturas $taskOutput
    }
} finally {
    $taskResolved=[IO.Path]::GetFullPath($taskTemp)
    if (!$taskResolved.StartsWith($taskTempParent,[StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetFileName($taskResolved) -notmatch '^CodexAbastUi_[a-f0-9]{32}$') { throw 'Se rechazo borrar una carpeta ajena al runner.' }
    Remove-Item -LiteralPath $taskResolved -Recurse -Force
}
