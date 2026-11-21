param([string]$Servidor='.', [string]$SalidaDir, [string]$RutaBinarios)
$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
if (!$SalidaDir) { $SalidaDir=Join-Path ([IO.Path]::GetTempPath()) 'HemovidaGest/Pruebas/CicloSesion' }
$taskOutput=[IO.Path]::GetFullPath($SalidaDir)
[void][IO.Directory]::CreateDirectory($taskOutput)
Write-Host "Resultados de las pruebas en $taskOutput"
$taskMsBuild='C:/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe'
if (!$RutaBinarios) {
    $RutaBinarios=Join-Path $taskOutput 'build'
    & $taskMsBuild (Join-Path $taskRoot 'UI/UI.csproj') /restore /t:Build /p:Configuration=Release ('/p:OutDir='+$RutaBinarios+'/') /verbosity:minimal /nologo
    if ($LASTEXITCODE -ne 0) { throw 'Fallo la compilacion UI para la regresion de cierre.' }
}
$taskTempParent=[IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$taskTemp=Join-Path $taskTempParent ('CodexCicloSesion_'+[Guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($taskTemp)
try {
    Get-ChildItem -LiteralPath $RutaBinarios -File | Where-Object {$_.Extension -in @('.dll','.exe','.pdb')} | Copy-Item -Destination $taskTemp
    $taskRunner=Join-Path $taskTemp 'CicloSesion.UI.Tests.exe'
    $taskCsc=Join-Path (Split-Path $taskMsBuild -Parent) 'Roslyn/csc.exe'
    $taskReferences=@('UI.exe','Application.dll','DAL.dll','Domain.dll','Services.dll') | ForEach-Object {'/r:'+(Join-Path $taskTemp $_)}
    & $taskCsc /nologo /debug /target:exe ('/out:'+$taskRunner) /r:System.dll /r:System.Core.dll /r:System.Data.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll @taskReferences (Join-Path $PSScriptRoot 'CicloSesion.UI.Tests.cs')
    if ($LASTEXITCODE -ne 0) { throw 'Fallo la compilacion del runner de cierre.' }
    & (Join-Path $PSScriptRoot 'Abastecimiento.Tests.ps1') -Servidor $Servidor -Salida (Join-Path $taskOutput 'sql-fixture-tests.json') -UiRunner $taskRunner -UiCapturas $taskOutput
} finally {
    $taskResolved=[IO.Path]::GetFullPath($taskTemp)
    if (!$taskResolved.StartsWith($taskTempParent,[StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetFileName($taskResolved) -notmatch '^CodexCicloSesion_[a-f0-9]{32}$') { throw 'Se rechazo limpiar una carpeta ajena al runner.' }
    Remove-Item -LiteralPath $taskResolved -Recurse -Force
}
