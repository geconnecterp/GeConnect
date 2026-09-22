param([string]$BuildDirectory = (Join-Path $PSScriptRoot '../../.artifacts/nc-pagos-build'))
$ErrorActionPreference = 'Stop'
$buildPath = (Resolve-Path -LiteralPath $BuildDirectory).Path
foreach ($dll in @('Newtonsoft.Json.dll','gc.infraestructura.dll','gc.caja.core.dll')) { [void][Reflection.Assembly]::LoadFrom((Join-Path $buildPath $dll)) }
$count = 0
function Check([bool]$value,[string]$name) { if (-not $value) { throw $name }; $script:count++; Write-Output "OK $name" }
foreach ($inputValue in @('00000123','RC-0000123','"00000123"','{"rb_compte":"00000123"}','[{"rb_compte":"00000123"}]','{"rb_compte":123}')) {
 $value = [gc.caja.core.Servicios.Implementacion.Cajas.ResultadoCobranzaCtaCte]::ObtenerNumeroRecibo($inputValue)
 Check (-not [string]::IsNullOrWhiteSpace($value)) "Recibo válido: $inputValue"
}
Check ([gc.caja.core.Servicios.Implementacion.Cajas.ResultadoCobranzaCtaCte]::ObtenerNumeroRecibo('00000123') -eq '00000123') 'Conserva ceros del número de recibo'
foreach ($inputValue in @('', 'null', '{}', '[]', '[{"rb_compte":"1"},{"rb_compte":"2"}]', '{"cm_compte":"00000123"}', '{', 'Error 123')) {
 Check ([string]::IsNullOrEmpty([gc.caja.core.Servicios.Implementacion.Cajas.ResultadoCobranzaCtaCte]::ObtenerNumeroRecibo($inputValue))) "No inventa recibo ante respuesta incompleta: $inputValue"
}
Check ([gc.caja.core.Servicios.Implementacion.Cajas.TotalesCobranzaCtaCte]::Coinciden(100,70,30)) 'CC acepta valores más NC validadas'
Check ([gc.caja.core.Servicios.Implementacion.Cajas.TotalesCobranzaCtaCte]::Coinciden(100,0,100)) 'CC acepta cancelación completa con NC'
Check ([gc.caja.core.Servicios.Implementacion.Cajas.TotalesCobranzaCtaCte]::Coinciden(100,100,0)) 'CC conserva pago convencional'
Check (-not [gc.caja.core.Servicios.Implementacion.Cajas.TotalesCobranzaCtaCte]::Coinciden(100,70,0)) 'CC rechaza total insuficiente'
Check (-not [gc.caja.core.Servicios.Implementacion.Cajas.TotalesCobranzaCtaCte]::Coinciden(100,100,30)) 'CC rechaza exceso de valores más NC'
$movimientos = [Collections.Generic.List[gc.infraestructura.Dtos.Cajas.Response.CtaCteResponseDto]]::new()
$mov = [gc.infraestructura.Dtos.Cajas.Response.CtaCteResponseDto]::new()
$mov.tco_id = '006'; $mov.cv_importe = 80; $mov.cv_importe_ori = 100; $mov.cv_concepto = 'Movimiento'
$movimientos.Add($mov)
$tipos = [Collections.Generic.List[gc.infraestructura.Dtos.TipoComprobanteDto]]::new()
$tipo = [gc.infraestructura.Dtos.TipoComprobanteDto]::new(); $tipo.tco_id = '006'; $tipo.tco_desc = 'Factura B'; $tipos.Add($tipo)
[gc.caja.core.Servicios.Implementacion.Cajas.DescripcionesCuentaCorriente]::Completar($movimientos,$tipos)
Check ($mov.tco_desc -eq 'Factura B' -and $mov.tco_id -eq '006') 'Completa descripción desde catálogo sin cambiar identidad'
Check ($mov.cv_importe -eq 80 -and $mov.cv_importe_ori -eq 100 -and $mov.cv_concepto -eq 'Movimiento') 'Descripción no altera importes ni concepto'
$mov.tco_desc = 'Descripción del SP'
[gc.caja.core.Servicios.Implementacion.Cajas.DescripcionesCuentaCorriente]::Completar($movimientos,$tipos)
Check ($mov.tco_desc -eq 'Descripción del SP') 'Conserva descripción provista por el SP'
Write-Output "$count verificaciones de servidor CC aprobadas. Sin confirmar cobros."
