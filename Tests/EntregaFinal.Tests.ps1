param([string]$Servidor='.', [string]$Salida, [switch]$ProbarActualizador)
$ErrorActionPreference='Stop'
$entregaRoot=Split-Path $PSScriptRoot -Parent
$entregaInstaller=[IO.File]::ReadAllText((Join-Path $entregaRoot 'Installer/InstalarBase.ps1'))
$entregaLista=[regex]::Match($entregaInstaller,'(?s)\$scripts\s*=\s*@\((.*?)\)').Groups[1].Value
$entregaScripts=@([regex]::Matches($entregaLista,"'([^']+\.sql)'") | ForEach-Object { $_.Groups[1].Value })
if ($entregaScripts.Count -lt 20 -or $entregaScripts[-1] -ne '16_DiagramasFinalesSSMS.sql') { throw 'No se pudo resolver la lista final del instalador.' }
$entregaBodies=@{}; $entregaHashes=@{}
foreach($entregaNombre in $entregaScripts) {
    if ($entregaNombre -notmatch '^[0-9]{2}[a-z]?_[A-Za-z0-9_.]+\.sql$') { throw 'Ruta SQL inesperada en el instalador.' }
    $entregaRuta=if($entregaNombre.StartsWith('00_')){Join-Path $entregaRoot "Database/$entregaNombre"}else{Join-Path $entregaRoot "Database/StoredProcedures/$entregaNombre"}
    $entregaSql=[IO.File]::ReadAllText($entregaRuta)
    $entregaHashes[$entregaNombre]=(Get-FileHash -Algorithm SHA256 -LiteralPath $entregaRuta).Hash
    if($entregaNombre.StartsWith('00_')){$entregaSql=$entregaSql.Substring($entregaSql.IndexOf('SET ANSI_NULLS'))}
    $entregaBodies[$entregaNombre]=[regex]::Replace($entregaSql,'(?im)^\s*USE\s+\[?HemovidaGest\]?\s*;?\s*$','')
}
$entregaManifest=Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $entregaRoot 'Database/StoredProcedures/16_DiagramasFinalesSSMS.manifest.json') | ConvertFrom-Json
if(@($entregaManifest.Diagrams).Count -ne 4){throw 'El manifiesto16 debe contener4diagramas.'}
$entregaBase='CodexEntrega_'+[Guid]::NewGuid().ToString('N').Substring(0,12)
if($entregaBase -notmatch '^CodexEntrega_[a-f0-9]{12}$'){throw 'Nombre aislado invalido.'}
$entregaBuilder=New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$entregaBuilder['Data Source']=$Servidor; $entregaBuilder['Initial Catalog']='master'; $entregaBuilder['Integrated Security']=$true; $entregaBuilder['TrustServerCertificate']=$true
$entregaMaster=New-Object System.Data.SqlClient.SqlConnection $entregaBuilder.ConnectionString
$entregaConexion=$null; $entregaTx=$null; $entregaCreada=$false
$entregaChecks=New-Object 'System.Collections.Generic.List[string]'
$entregaStages=New-Object 'System.Collections.Generic.List[object]'
function Invoke-EntregaTestSql([string]$Sql,[switch]$Table) {
    $entregaCmd=$entregaConexion.CreateCommand();$entregaCmd.CommandText=$Sql;$entregaCmd.CommandTimeout=240
    if($entregaTx){$entregaCmd.Transaction=$entregaTx}
    try {
        if($Table){$entregaTabla=New-Object System.Data.DataTable;$entregaAdapter=New-Object System.Data.SqlClient.SqlDataAdapter($entregaCmd);try{[void]$entregaAdapter.Fill($entregaTabla)}finally{$entregaAdapter.Dispose()};return ,$entregaTabla}
        return $entregaCmd.ExecuteScalar()
    } finally {$entregaCmd.Dispose()}
}
function Assert-Entrega([bool]$Condicion,[string]$Nombre){if(!$Condicion){throw "Fallo: $Nombre"};$entregaChecks.Add($Nombre)}
function Invoke-EntregaTestScript([string]$Nombre) {
    foreach($entregaBatch in [regex]::Split($entregaBodies[$Nombre],'(?im)^\s*GO\s*(?:--[^\r\n]*)?$')){
        if($entregaBatch.Trim()){[void](Invoke-EntregaTestSql $entregaBatch)}
    }
}
function Get-EntregaDiagrams {
    $entregaTabla=Invoke-EntregaTestSql "SELECT diagram_id,name,principal_id,version,DATALENGTH(definition) bytes,CONVERT(VARCHAR(64),HASHBYTES('SHA2_256',definition),2) sha256 FROM dbo.sysdiagrams ORDER BY name" -Table
    return @($entregaTabla.Rows|ForEach-Object{[pscustomobject]@{Name=[string]$_.name;ID=[int]$_.diagram_id;Owner=[int]$_.principal_id;Version=[int]$_.version;Bytes=[long]$_.bytes;SHA256=[string]$_.sha256}})
}
function Get-EntregaCounts {
    $entregaCounts=@{}
    $entregaTablas=Invoke-EntregaTestSql "SELECT s.name+'.'+t.name nombre,QUOTENAME(s.name)+'.'+QUOTENAME(t.name) objeto FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id WHERE t.is_ms_shipped=0 AND t.name NOT IN('sysdiagrams','sysdiagram_properties')" -Table
    foreach($entregaFila in $entregaTablas.Rows){$entregaCounts[[string]$entregaFila.nombre]=[long](Invoke-EntregaTestSql ('SELECT COUNT_BIG(*) FROM '+$entregaFila.objeto))}
    return $entregaCounts
}
function Assert-EntregaPreserved($Antes,$Despues,[string]$Etapa) {
    foreach($entregaOld in $Antes){
        $entregaMatches=@($Despues|Where-Object{$_.Name -eq $entregaOld.Name -and $_.Owner -eq $entregaOld.Owner})
        if($entregaMatches.Count -ne 1 -or $entregaMatches[0].ID -ne $entregaOld.ID -or $entregaMatches[0].Version -ne $entregaOld.Version -or $entregaMatches[0].SHA256 -ne $entregaOld.SHA256 -or $entregaMatches[0].Bytes -ne $entregaOld.Bytes){throw "Diagrama previo alterado en $Etapa : $($entregaOld.Name)"}
    }
    $entregaChecks.Add("$Etapa conserva IDs, propietarios, versiones ybinarios de$(@($Antes).Count)diagramas previos")
}
function Assert-EntregaCanonical($Diagramas,[string]$Etapa) {
    foreach($entregaCanon in $entregaManifest.Diagrams){
        $entregaMatch=@($Diagramas|Where-Object{$_.Name -eq $entregaCanon.Name -and $_.Owner -eq 1})
        Assert-Entrega ($entregaMatch.Count -eq 1 -and $entregaMatch[0].SHA256 -eq $entregaCanon.SHA256 -and $entregaMatch[0].Bytes -eq $entregaCanon.Bytes -and $entregaMatch[0].Version -eq $entregaCanon.Version -and $entregaCanon.AllTablesStandard) "$Etapa canonico $($entregaCanon.Name)"
    }
}
try {
    $entregaMaster.Open();$entregaCmd=$entregaMaster.CreateCommand()
    try{$entregaCmd.CommandText="CREATE DATABASE [$entregaBase]";[void]$entregaCmd.ExecuteNonQuery();$entregaCreada=$true}finally{$entregaCmd.Dispose()}
    $entregaBuilder['Initial Catalog']=$entregaBase;$entregaConexion=New-Object System.Data.SqlClient.SqlConnection $entregaBuilder.ConnectionString;$entregaConexion.Open()
    $entregaTx=$entregaConexion.BeginTransaction()
    $entregaTail=@('15_AbastecimientoRiesgo.sql','15a_AbastecimientoUI.sql','16_DiagramasFinalesSSMS.sql')
    foreach($entregaNombre in $entregaScripts|Where-Object{$_ -notin $entregaTail}){Invoke-EntregaTestScript $entregaNombre}
    $entregaAntes=Get-EntregaDiagrams;$entregaAntesCounts=Get-EntregaCounts
    Assert-Entrega ($entregaAntes.Count -eq 17) 'Baseline contiene17diagramas nativos anteriores'
    foreach($entregaNombre in $entregaTail){Invoke-EntregaTestScript $entregaNombre}
    $entregaFresh=Get-EntregaDiagrams;$entregaFreshCounts=Get-EntregaCounts
    Assert-Entrega ($entregaFresh.Count -eq 21) 'Fresh install agrega4diagramas sin duplicados'
    Assert-EntregaPreserved $entregaAntes $entregaFresh 'Fresh'
    Assert-EntregaCanonical $entregaFresh 'Fresh'
    foreach($entregaTabla in $entregaAntesCounts.Keys|Where-Object{$_ -notin @('dbo.Etiqueta','dbo.Traduccion')}){Assert-Entrega ($entregaAntesCounts[$entregaTabla] -eq $entregaFreshCounts[$entregaTabla]) "Fresh sinfilasnuevas $entregaTabla"}
    Assert-Entrega ([int](Invoke-EntregaTestSql 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores') -eq 0) 'Fresh conserva integridad'
    Assert-Entrega ([int](Invoke-EntregaTestSql 'SELECT dbo.fn_diagramobjects()') -eq 255) 'Fresh soportediagramasSSMS completo'
    $entregaSchema=Invoke-EntregaTestSql @'
SELECT COUNT(DISTINCT t.object_id) tablas,COUNT(c.column_id) columnas
FROM sys.tables t JOIN sys.columns c ON c.object_id=t.object_id
WHERE t.is_ms_shipped=0 AND t.name NOT IN('sysdiagrams','sysdiagram_properties');
'@ -Table
    Assert-Entrega ([int]$entregaSchema.Rows[0].tablas -eq [int]$entregaManifest.ApplicationTableCount -and [int]$entregaSchema.Rows[0].columnas -eq [int]$entregaManifest.ColumnCount) 'Fresh tablas ycolumnas coinciden conmanifest16'
    Assert-Entrega ([int](Invoke-EntregaTestSql "SELECT COUNT(*) FROM sys.foreign_keys WHERE is_ms_shipped=0") -eq [int]$entregaManifest.ForeignKeyCount) 'Fresh FK coinciden conmanifest16'
    $entregaTx.Commit();$entregaTx.Dispose();$entregaTx=$null
    $entregaStages.Add([pscustomobject]@{Stage='fresh';Diagrams=$entregaFresh;ApplicationRows=$entregaFreshCounts})
    [void](Invoke-EntregaTestSql "UPDATE t SET texto=N'Entrega final personalizada' FROM dbo.Traduccion t JOIN dbo.Etiqueta e ON e.id_etiqueta=t.id_etiqueta JOIN dbo.Idioma i ON i.id_idioma=t.id_idioma WHERE e.clave='SUPPLY_TITLE' AND i.codigo='es-AR'")
    for($entregaVez=1;$entregaVez -le 2;$entregaVez++){
        $entregaTx=$entregaConexion.BeginTransaction()
        foreach($entregaNombre in $entregaTail){Invoke-EntregaTestScript $entregaNombre}
        $entregaNow=Get-EntregaDiagrams;$entregaNowCounts=Get-EntregaCounts
        Assert-Entrega ($entregaNow.Count -eq 21) "Reapply$entregaVez sin diagramas duplicados"
        Assert-EntregaPreserved $entregaFresh $entregaNow "Reapply$entregaVez"
        Assert-EntregaCanonical $entregaNow "Reapply$entregaVez"
        foreach($entregaTabla in $entregaFreshCounts.Keys){Assert-Entrega ($entregaFreshCounts[$entregaTabla] -eq $entregaNowCounts[$entregaTabla]) "Reapply$entregaVez sinfilasnuevas $entregaTabla"}
        Assert-Entrega ((Invoke-EntregaTestSql "SELECT t.texto FROM dbo.Traduccion t JOIN dbo.Etiqueta e ON e.id_etiqueta=t.id_etiqueta JOIN dbo.Idioma i ON i.id_idioma=t.id_idioma WHERE e.clave='SUPPLY_TITLE' AND i.codigo='es-AR'") -eq 'Entrega final personalizada') "Reapply$entregaVez preservatraduccionpersonalizada"
        Assert-Entrega ([int](Invoke-EntregaTestSql 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores') -eq 0) "Reapply$entregaVez integridadvalida"
        $entregaTx.Commit();$entregaTx.Dispose();$entregaTx=$null
        $entregaStages.Add([pscustomobject]@{Stage="reapply$entregaVez";Diagrams=$entregaNow;ApplicationRows=$entregaNowCounts})
    }
    $entregaWorkflow=$null
    if($ProbarActualizador){
        $entregaWorkflowLines=@(& (Join-Path $entregaRoot 'Database/InstalarEntregaFinal.ps1') -Servidor $Servidor -Base $entregaBase 6>&1 | ForEach-Object{$_.ToString()})
        $entregaBackupLine=@($entregaWorkflowLines|Where-Object{$_ -match '^Respaldo verificado: '})
        Assert-Entrega ($entregaBackupLine.Count -eq 1) 'Actualizador real completa backup yVERIFYONLY'
        $entregaBackupPath=$entregaBackupLine[0].Substring('Respaldo verificado: '.Length)
        $entregaHeaderCmd=$entregaConexion.CreateCommand()
        try{
            $entregaHeaderCmd.CommandText='RESTORE HEADERONLY FROM DISK=@ruta'
            [void]$entregaHeaderCmd.Parameters.Add('@ruta',[System.Data.SqlDbType]::NVarChar,4000)
            $entregaHeaderCmd.Parameters['@ruta'].Value=$entregaBackupPath
            $entregaHeader=New-Object System.Data.DataTable
            $entregaHeaderAdapter=New-Object System.Data.SqlClient.SqlDataAdapter($entregaHeaderCmd)
            try{[void]$entregaHeaderAdapter.Fill($entregaHeader)}finally{$entregaHeaderAdapter.Dispose()}
        }finally{$entregaHeaderCmd.Dispose()}
        Assert-Entrega ($entregaHeader.Rows.Count -eq 1 -and $entregaHeader.Rows[0].DatabaseName -eq $entregaBase -and $entregaHeader.Rows[0].IsCopyOnly -and $entregaHeader.Rows[0].HasBackupChecksums) 'Backup corresponde solo aBDtemporal ycontieneCOPY_ONLY CHECKSUM'
        $entregaAfterInstaller=Get-EntregaDiagrams
        Assert-EntregaPreserved $entregaFresh $entregaAfterInstaller 'Actualizador real'
        Assert-EntregaCanonical $entregaAfterInstaller 'Actualizador real'
        $entregaWorkflow=[ordered]@{status='passed';script='Database/InstalarEntregaFinal.ps1';backupPath=$entregaBackupPath;backupCopyOnly=$true;backupHasChecksums=$true;backupVerified=$true;output=$entregaWorkflowLines;backupCleaned=$false}
        if([IO.Path]::GetFileName($entregaBackupPath).StartsWith($entregaBase+'_AntesEntregaFinal_',[StringComparison]::Ordinal) -and [IO.Path]::GetExtension($entregaBackupPath) -eq '.bak'){
            try{Remove-Item -LiteralPath $entregaBackupPath -Force -ErrorAction Stop;$entregaWorkflow.backupCleaned=$true}catch{$entregaWorkflow.backupCleanupError=$_.Exception.GetType().Name}
        }
    }
    $entregaReport=[ordered]@{status='passed';database=$entregaBase;freshInstallerOrder=$entregaScripts;sourceSHA256=$entregaHashes;previousDiagrams=$entregaAntes;canonicalDiagrams=$entregaManifest.Diagrams;stages=$entregaStages.ToArray();actualUpdaterWorkflow=$entregaWorkflow;checkCount=$entregaChecks.Count;checks=$entregaChecks.ToArray();privateConfigurationRead=$false;realDatabaseModified=$false}
    if($Salida){$entregaReport|ConvertTo-Json -Depth 12|Set-Content -Encoding UTF8 -LiteralPath $Salida}
    [pscustomobject]@{status='passed';database=$entregaBase;scripts=$entregaScripts.Count;diagramCounts=@(17,21,21,21);checks=$entregaChecks.Count}|ConvertTo-Json
} finally {
    if($entregaTx){try{$entregaTx.Rollback()}catch{};$entregaTx.Dispose()}
    if($entregaConexion){$entregaConexion.Dispose()}
    if($entregaCreada){
        if($entregaBase -notmatch '^CodexEntrega_[a-f0-9]{12}$'){throw 'Se rechazo limpiar una base ajena a la prueba.'}
        $entregaCmd=$entregaMaster.CreateCommand();try{$entregaCmd.CommandText="ALTER DATABASE [$entregaBase] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$entregaBase]";[void]$entregaCmd.ExecuteNonQuery()}finally{$entregaCmd.Dispose()}
    }
    $entregaMaster.Dispose()
}
