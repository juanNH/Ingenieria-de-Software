$ErrorActionPreference = 'Stop'
$output = Join-Path $PSScriptRoot '..\UI\bin\Debug'
Add-Type -Path (Join-Path $output 'Abstractions.dll')
Add-Type -Path (Join-Path $output 'Domain.dll')
Add-Type -Path (Join-Path $output 'PdfSharp.dll')
Add-Type -Path (Join-Path $output 'Services.dll')

$folder = Join-Path ([IO.Path]::GetTempPath()) ('hemovida-pdf-' + [guid]::NewGuid())
New-Item -ItemType Directory -Path $folder | Out-Null
try {
    $service = New-Object Services.ComprobantePdfService_380_jh
    $units = New-Object 'System.Collections.Generic.List[Domain.Unidad_380_jh]'
    for ($i = 1; $i -le 25; $i++) {
        $unit = New-Object Domain.Unidad_380_jh
        $unit.Id_380_jh = $i; $unit.IdDonacion_380_jh = 7
        $unit.CodigoIdentificacion_380_jh = 'U-' + $i
        $unit.Donante_380_jh = 'María Núñez'; $unit.DocumentoDonante_380_jh = '12345678'
        $unit.TipoComponente_380_jh = 'Glóbulos rojos'; $unit.GrupoSanguineo_380_jh = 'O'; $unit.FactorRh_380_jh = '+'
        $unit.FechaVencimiento_380_jh = [datetime]'2027-01-01'; $unit.EstadoOperativo_380_jh = 'LIBERADA'
        $unit.FechaAlta_380_jh = [datetime]'2026-10-01'; $unit.FechaUltimaModificacion_380_jh = [datetime]'2026-10-02'
        $unit.IdUsuarioUltimaModificacion_380_jh = 3; $units.Add($unit)
    }
    $pn1 = Join-Path $folder 'pn1.pdf'
    $service.GuardarPN1_380_jh($pn1, $units, 'operador')
    $pdf = [PdfSharp.Pdf.IO.PdfReader]::Open($pn1, [PdfSharp.Pdf.IO.PdfDocumentOpenMode]::Import)
    if ($pdf.PageCount -lt 2) { throw 'PN1 debe paginar una donación extensa.' }; $pdf.Close()
    $units[0].IdDonacion_380_jh = 8
    $rejected = $false
    try { $service.GuardarPN1_380_jh((Join-Path $folder 'invalido.pdf'), $units, 'operador') } catch { $rejected = $true }
    if (-not $rejected) { throw 'Se aceptaron unidades de distintas donaciones.' }

    $request = New-Object Domain.SolicitudTransfusional_380_jh
    $request.Id_380_jh = 9; $request.PacienteReceptor_380_jh = 'José Pérez'; $request.TipoComponente_380_jh = 'Glóbulos rojos'
    $request.FechaSolicitud_380_jh = [datetime]'2026-10-10'; $request.CantidadRequerida_380_jh = 3
    $request.CantidadEntregada_380_jh = 1; $request.SaldoPendiente_380_jh = 2; $request.EstadoSolicitud_380_jh = 'PARCIAL'
    $deliveries = New-Object 'System.Collections.Generic.List[Domain.Entrega_380_jh]'
    $delivery = New-Object Domain.Entrega_380_jh
    $delivery.Id_380_jh = 21; $delivery.IdSolicitudTransfusional_380_jh = 9; $delivery.CodigoIdentificacion_380_jh = 'U-1'
    $delivery.FechaEntrega_380_jh = [datetime]'2026-10-11'; $delivery.Destino_380_jh = 'DEVUELTA'
    $deliveries.Add($delivery)
    $effective = New-Object Domain.Entrega_380_jh
    $effective.Id_380_jh = 22; $effective.IdSolicitudTransfusional_380_jh = 9; $effective.CodigoIdentificacion_380_jh = 'U-2'
    $effective.FechaEntrega_380_jh = [datetime]'2026-10-12'; $effective.Destino_380_jh = 'TRANSFUNDIDA'
    $deliveries.Add($effective)
    $pn2 = Join-Path $folder 'pn2.pdf'; $individual = Join-Path $folder 'entrega.pdf'
    $service.GuardarPN2_380_jh($pn2, $request, $deliveries, 'operador', $null)
    $service.GuardarPN2_380_jh($individual, $request, $deliveries, 'operador', 21)
    foreach ($path in @($pn2, $individual)) {
        $pdf = [PdfSharp.Pdf.IO.PdfReader]::Open($path, [PdfSharp.Pdf.IO.PdfDocumentOpenMode]::Import)
        if ($pdf.PageCount -lt 1) { throw 'PDF sin páginas.' }; $pdf.Close()
    }
    $rejected = $false
    try { $service.GuardarPN2_380_jh((Join-Path $folder 'inexistente.pdf'), $request, $deliveries, 'operador', 99) } catch { $rejected = $true }
    if (-not $rejected) { throw 'Se aceptó una entrega inexistente.' }
    Write-Output "PDF de prueba: $folder"
} catch { throw }
