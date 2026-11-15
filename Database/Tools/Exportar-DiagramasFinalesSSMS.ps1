param(
    [string]$Servidor='.',
    [string]$Base='HemovidaGest',
    [string]$Destino=(Join-Path $PSScriptRoot '../StoredProcedures/16_DiagramasFinalesSSMS.sql')
)
# Exportación de solo lectura de los cuatro guardados nativos finales de SSMS.
# El instalador restaura estos bytes; este script no fabrica controles ni layouts.
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'SSMS.NativeStorage.ps1')
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$taskNames=@('G07 - Modelo de datos completo','G07 - Seguridad, idiomas y auditoria','G07 - Operaciones PN1 y PN2','G07 - Catalogos y compatibilidad')
$taskFilter="t.is_ms_shipped=0 AND t.name NOT IN (N'sysdiagrams',N'sysdiagram_properties',N'dtproperties')"
$taskQueries=[ordered]@{
    tables="SELECT s.name schemaName,t.name tableName FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id WHERE $taskFilter ORDER BY s.name,t.name FOR JSON PATH"
    columns="SELECT s.name schemaName,t.name tableName,c.name columnName,ty.name dataType,c.max_length maxLengthBytes,c.precision,c.scale,c.is_nullable isNullable,c.is_identity isIdentity,c.is_computed isComputed FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id JOIN sys.columns c ON c.object_id=t.object_id JOIN sys.types ty ON ty.user_type_id=c.user_type_id WHERE $taskFilter ORDER BY s.name,t.name,c.name FOR JSON PATH"
    foreignKeys="SELECT fk.name,ps.name fromSchema,pt.name fromTable,rs.name toSchema,rt.name toTable,fk.is_disabled isDisabled,fk.is_not_trusted isNotTrusted,fk.delete_referential_action_desc onDelete,fk.update_referential_action_desc onUpdate,JSON_QUERY((SELECT pc.name fromColumn,rc.name toColumn,fc.constraint_column_id ordinal FROM sys.foreign_key_columns fc JOIN sys.columns pc ON pc.object_id=fc.parent_object_id AND pc.column_id=fc.parent_column_id JOIN sys.columns rc ON rc.object_id=fc.referenced_object_id AND rc.column_id=fc.referenced_column_id WHERE fc.constraint_object_id=fk.object_id ORDER BY fc.constraint_column_id FOR JSON PATH)) columns FROM sys.foreign_keys fk JOIN sys.tables pt ON pt.object_id=fk.parent_object_id JOIN sys.schemas ps ON ps.schema_id=pt.schema_id JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id JOIN sys.schemas rs ON rs.schema_id=rt.schema_id WHERE pt.is_ms_shipped=0 AND pt.name NOT IN (N'sysdiagrams',N'sysdiagram_properties',N'dtproperties') ORDER BY ps.name,pt.name,fk.name FOR JSON PATH"
    keys="SELECT s.name schemaName,t.name tableName,k.name,k.type,JSON_QUERY((SELECT c.name,ic.key_ordinal ordinal FROM sys.index_columns ic JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id WHERE ic.object_id=k.parent_object_id AND ic.index_id=k.unique_index_id ORDER BY ic.key_ordinal FOR JSON PATH)) columns FROM sys.key_constraints k JOIN sys.tables t ON t.object_id=k.parent_object_id JOIN sys.schemas s ON s.schema_id=t.schema_id WHERE $taskFilter ORDER BY s.name,t.name,k.type,k.name FOR JSON PATH"
    checks="SELECT s.name schemaName,t.name tableName,ch.name,ch.definition,ch.is_disabled isDisabled,ch.is_not_trusted isNotTrusted FROM sys.check_constraints ch JOIN sys.tables t ON t.object_id=ch.parent_object_id JOIN sys.schemas s ON s.schema_id=t.schema_id WHERE $taskFilter ORDER BY s.name,t.name,ch.name FOR JSON PATH"
    defaults="SELECT s.name schemaName,t.name tableName,c.name columnName,d.name,d.definition FROM sys.default_constraints d JOIN sys.tables t ON t.object_id=d.parent_object_id JOIN sys.schemas s ON s.schema_id=t.schema_id JOIN sys.columns c ON c.object_id=d.parent_object_id AND c.column_id=d.parent_column_id WHERE $taskFilter ORDER BY s.name,t.name,c.column_id FOR JSON PATH"
}
$taskBuilder=New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$taskBuilder['Data Source']=$Servidor;$taskBuilder['Initial Catalog']=$Base
$taskBuilder['Integrated Security']=$true;$taskBuilder['TrustServerCertificate']=$true
$taskBuilder['ApplicationIntent']='ReadOnly';$taskBuilder['Application Name']='G07 native export read-only'
$taskConnection=New-Object System.Data.SqlClient.SqlConnection $taskBuilder.ConnectionString
$taskSchema=[ordered]@{};$taskRawSchema=@{};$taskDefinitions=@()
try{
    $taskConnection.Open()
    foreach($taskQueryEntry in $taskQueries.GetEnumerator()){
        $taskKey=$taskQueryEntry.Key
        $taskCommand=$taskConnection.CreateCommand();$taskCommand.CommandText=$taskQueryEntry.Value
        $taskReader=$taskCommand.ExecuteReader();$taskJson=New-Object Text.StringBuilder
        while($taskReader.Read()){[void]$taskJson.Append($taskReader.GetString(0))}
        $taskReader.Dispose();$taskCommand.Dispose()
        $taskRawSchema[$taskKey]=$taskJson.ToString();$taskSchema[$taskKey]=@($taskJson.ToString()|ConvertFrom-Json)
    }
    foreach($taskName in $taskNames){
        $taskCommand=$taskConnection.CreateCommand()
        $taskCommand.CommandText="SELECT diagram_id,principal_id,version,definition FROM dbo.sysdiagrams WHERE name=@name AND principal_id=DATABASE_PRINCIPAL_ID(N'dbo')"
        [void]$taskCommand.Parameters.AddWithValue('@name',$taskName)
        $taskReader=$taskCommand.ExecuteReader()
        if(-not $taskReader.Read()){throw ('Falta el guardado nativo '+$taskName)}
        $taskDefinition=[pscustomobject]@{Name=$taskName;Id=$taskReader.GetInt32(0);Owner=$taskReader.GetInt32(1);Version=$taskReader.GetInt32(2);Bytes=[byte[]]$taskReader.GetValue(3)}
        if($taskReader.Read()){throw 'Nombre nativo ambiguo.'}
        $taskReader.Dispose();$taskCommand.Dispose();$taskDefinitions+=$taskDefinition
    }
}finally{$taskConnection.Dispose()}
if($taskSchema.tables.Count -ne 32 -or $taskSchema.columns.Count -ne 221 -or $taskSchema.foreignKeys.Count -ne 53){throw 'Cambió el alcance G07 esperado (32 tablas,221 columnas,53 FK); revisar y actualizar el modelo antes de exportar.'}
$taskRecords=@()
foreach($taskDefinition in $taskDefinitions){
    $taskBinary=Join-Path $taskRoot ('Diagramas/Diagramas Entidad Relacion/'+$taskDefinition.Name+'.ssms.bin')
    [IO.File]::WriteAllBytes($taskBinary,$taskDefinition.Bytes)
    if([BitConverter]::ToString($taskDefinition.Bytes,0,8) -ne 'D0-CF-11-E0-A1-B1-1A-E1' -or [BitConverter]::ToString($taskDefinition.Bytes,$taskDefinition.Bytes.Length-4,4) -ne '62-88-52-14'){throw 'Definición sin firma OLE y trailer DDS nativos.'}
    [xml]$taskXml=Get-SsmsNativeXml $taskBinary
    $taskTables=@($taskXml.dds.ddscontrol|Where-Object{$_.controlprogid -eq '{623AC075-2C77-4873-883D-930B4364B4BC}'})
    if($taskTables.Count -eq 0 -or @($taskTables|Where-Object{@($_.layoutobject.ddsxmlobj.property|Where-Object{$_.name -eq 'ActiveTableViewMode'})[0].value -ne '0'}).Count){throw ('Todas las tablas deben estar en Standard: '+$taskDefinition.Name)}
    $taskTableNames=@($taskTables.tooltip|Sort-Object)
    if(@($taskTableNames|Group-Object|Where-Object{$_.Count -ne 1}).Count){throw 'Tabla nativa duplicada.'}
    if(@($taskTableNames|Where-Object{$_ -notin $taskSchema.tables.tableName}).Count){throw 'Tabla nativa ajena al catálogo actual.'}
    if($taskDefinition.Name -eq $taskNames[0] -and @(Compare-Object @($taskSchema.tables.tableName|Sort-Object) $taskTableNames).Count){throw 'El general no cubre las32 tablas.'}
    $taskNativeFks=@($taskXml.dds.ddscontrol|Where-Object{$_.tooltip -like "Relationship '*' between *"}|ForEach-Object{if($_.tooltip -notmatch "^Relationship '([^']+)' between '([^']+)' and '([^']+)'$"){throw 'FK nativa no reconocida.'};$matches[1]}|Sort-Object)
    $taskExpectedFks=@($taskSchema.foreignKeys|Where-Object{$_.fromTable -in $taskTableNames -and $_.toTable -in $taskTableNames}|ForEach-Object{$_.name}|Sort-Object)
    if(@(Compare-Object $taskExpectedFks $taskNativeFks).Count){throw ('Las relaciones nativas no coinciden con las FK reales: '+$taskDefinition.Name)}
    $taskHash=(Get-FileHash -LiteralPath $taskBinary -Algorithm SHA256).Hash
    $taskRecord=[ordered]@{Name=$taskDefinition.Name;SourceDiagramId=$taskDefinition.Id;Owner=1;OwnerName='dbo';Version=$taskDefinition.Version;Bytes=$taskDefinition.Bytes.Length;SHA256=$taskHash;Tables=$taskTableNames;TableCount=$taskTableNames.Count;ColumnCount=@($taskSchema.columns|Where-Object{$_.tableName -in $taskTableNames}).Count;ForeignKeys=$taskNativeFks;ForeignKeyCount=$taskNativeFks.Count;AllTablesStandard=$true;NativeBinaryPath=$taskBinary.Substring($taskRoot.Length+1).Replace('\','/');Origin='Native SSMS open, Arrange Tables, save, exact dbo.sysdiagrams bytes'}
    $taskRecords+=[pscustomobject]$taskRecord
}
$taskSql=New-Object Text.StringBuilder
[void]$taskSql.AppendLine('-- G07: cuatro diagramas nativos guardados en SSMS, con todas sus tablas en Standard.')
[void]$taskSql.AppendLine('-- Restaura definiciones validadas; no cambia tablas, columnas, FK ni datos de aplicación.')
[void]$taskSql.AppendLine('-- Conectar explícitamente a la base destino. No usa App.local.config ni contiene credenciales.')
$taskBootstrap=[IO.File]::ReadAllText((Join-Path $taskRoot 'Database/StoredProcedures/14_DiagramasCDUSSMS.sql'))
$taskMarker=[regex]::Match($taskBootstrap,'(?m)^SET XACT_ABORT ON;\r?$')
if(-not $taskMarker.Success){throw 'No se encontró el límite del bootstrap nativo.'}
[void]$taskSql.AppendLine($taskBootstrap.Substring(0,$taskMarker.Index))
[void]$taskSql.AppendLine('SET XACT_ABORT ON;')
[void]$taskSql.AppendLine('BEGIN TRANSACTION;')
foreach($taskKey in @('tables','columns','foreignKeys')){
    $taskExpectedJson=$taskRawSchema[$taskKey].Replace("'","''")
    [void]$taskSql.AppendLine("DECLARE @G07Expected_$taskKey NVARCHAR(MAX)=N'$taskExpectedJson';")
    [void]$taskSql.AppendLine("DECLARE @G07Actual_$taskKey NVARCHAR(MAX)=($($taskQueries[$taskKey]));")
    [void]$taskSql.AppendLine("IF HASHBYTES('SHA2_256',COALESCE(@G07Actual_$taskKey,N''))<>HASHBYTES('SHA2_256',@G07Expected_$taskKey)")
    [void]$taskSql.AppendLine(" THROW 51016,'G07_SCHEMA_MISMATCH_$taskKey',1;")
}
[void]$taskSql.AppendLine('GO')
foreach($taskDefinition in $taskDefinitions){
    $taskName=$taskDefinition.Name.Replace("'","''")
    $taskHex=[BitConverter]::ToString($taskDefinition.Bytes).Replace('-','')
    [void]$taskSql.AppendLine("DECLARE @G07Owner INT=DATABASE_PRINCIPAL_ID(N'dbo'),@G07Result INT;")
    [void]$taskSql.AppendLine("DECLARE @G07Definition VARBINARY(MAX)=0x$taskHex;")
    [void]$taskSql.AppendLine("IF NOT EXISTS(SELECT 1 FROM dbo.sysdiagrams WHERE name=N'$taskName' AND principal_id=@G07Owner)")
    [void]$taskSql.AppendLine(" EXEC @G07Result=dbo.sp_creatediagram @diagramname=N'$taskName',@owner_id=@G07Owner,@version=$($taskDefinition.Version),@definition=@G07Definition;")
    [void]$taskSql.AppendLine("ELSE IF EXISTS(SELECT 1 FROM dbo.sysdiagrams WHERE name=N'$taskName' AND principal_id=@G07Owner AND (version IS NULL OR version<>$($taskDefinition.Version) OR definition IS NULL OR HASHBYTES('SHA2_256',definition)<>HASHBYTES('SHA2_256',@G07Definition)))")
    [void]$taskSql.AppendLine(" EXEC @G07Result=dbo.sp_alterdiagram @diagramname=N'$taskName',@owner_id=@G07Owner,@version=$($taskDefinition.Version),@definition=@G07Definition;")
    [void]$taskSql.AppendLine("IF @G07Result<0 THROW 51016,'G07_NATIVE_DIAGRAM_WRITE_FAILED',1;")
    [void]$taskSql.AppendLine("IF NOT EXISTS(SELECT 1 FROM dbo.sysdiagrams WHERE name=N'$taskName' AND principal_id=@G07Owner AND version=$($taskDefinition.Version) AND HASHBYTES('SHA2_256',definition)=HASHBYTES('SHA2_256',@G07Definition)) THROW 51016,'G07_NATIVE_DIAGRAM_READBACK_FAILED',1;")
    [void]$taskSql.AppendLine('GO')
}
[void]$taskSql.AppendLine('COMMIT TRANSACTION;')
[void]$taskSql.AppendLine('GO')
$taskDestination=[IO.Path]::GetFullPath($Destino)
[IO.File]::WriteAllText($taskDestination,$taskSql.ToString(),(New-Object Text.UTF8Encoding $false))
$taskManifest=[ordered]@{FormatVersion=1;Origin='Exact binary definitions saved by SQL Server Management Studio';TableView='Standard';ApplicationTableCount=$taskSchema.tables.Count;ColumnCount=$taskSchema.columns.Count;PrimaryKeyCount=@($taskSchema.keys|Where-Object{$_.type -eq 'PK'}).Count;ForeignKeyCount=$taskSchema.foreignKeys.Count;UniqueConstraintCount=@($taskSchema.keys|Where-Object{$_.type -eq 'UQ'}).Count;CheckConstraintCount=$taskSchema.checks.Count;DefaultConstraintCount=$taskSchema.defaults.Count;ExcludedSupportTables=@('sysdiagrams','sysdiagram_properties','dtproperties');Diagrams=$taskRecords;Schema=$taskSchema}
[IO.File]::WriteAllText([IO.Path]::ChangeExtension($taskDestination,'.manifest.json'),($taskManifest|ConvertTo-Json -Depth 15),(New-Object Text.UTF8Encoding $false))
[pscustomobject]@{sql=$taskDestination;diagrams=$taskRecords.Count;tables=$taskSchema.tables.Count;columns=$taskSchema.columns.Count;foreignKeys=$taskSchema.foreignKeys.Count;operation='Read-only export; local files written'}|ConvertTo-Json -Compress
