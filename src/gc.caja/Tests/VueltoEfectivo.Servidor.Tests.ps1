param([string]$BuildDirectory = (Join-Path $PSScriptRoot '../../.artifacts/nc-pagos-build'))
$ErrorActionPreference = 'Stop'
$buildPath = (Resolve-Path -LiteralPath $BuildDirectory).Path
foreach ($dll in @('Newtonsoft.Json.dll','gc.infraestructura.dll','gc.caja.core.dll')) { [void][Reflection.Assembly]::LoadFrom((Join-Path $buildPath $dll)) }
$count=0
function Check([bool]$value,[string]$name) { if (-not $value) { throw $name }; $script:count++; Write-Output "OK $name" }
function Pago([string]$id,[decimal]$monto) {
 $v=[gc.infraestructura.Dtos.Cajas.Json_Valor]::new(); $v.ins_id=$id; $v.rb_importe=$monto; $v.rb_nro_valor='001'; return $v
}
function Lista($items) {
 $r=[Collections.Generic.List[gc.infraestructura.Dtos.Cajas.Json_Valor]]::new()
 foreach($item in $items) { $r.Add($item) }; return ,$r
}
$ef=[gc.infraestructura.Dtos.Cajas.Response.ValoresInsResDto]::new(); $ef.ins_id='ARS'; $ef.tcf_id='EF'
$catalogo=[Collections.Generic.List[gc.infraestructura.Dtos.Cajas.Response.ValoresInsResDto]]::new(); $catalogo.Add($ef)
function Normalizar($valores,[decimal]$total,[decimal]$nc=0) {
 return [gc.caja.core.Servicios.Implementacion.Cajas.VueltoEfectivo]::Normalizar($valores,$total,$nc,$catalogo)
}
$v=Lista @((Pago 'ARS' 90000)); $r=Normalizar $v 85000
Check ($r.Ok -and $r.Vuelto -eq 5000 -and $v[0].rb_importe -eq 85000) '85000 / 90000: SP recibe efectivo neto 85000 y vuelto 5000'
$json=[Newtonsoft.Json.JsonConvert]::SerializeObject($v) | ConvertFrom-Json
Check ($json.Count -eq 1 -and $json[0].rb_importe -eq 85000 -and $json[0].ins_id -eq 'ARS') 'JSON real contiene un pago neto, sin fila negativa de vuelto'
$r=Normalizar $v 85000
Check ($r.Ok -and $r.Vuelto -eq 0 -and $v[0].rb_importe -eq 85000) 'Normalizar dos veces no descuenta dos veces'
$v=Lista @((Pago 'ARS' 200000)); $r=Normalizar $v 160554.04
Check ($r.Ok -and $r.Vuelto -eq 39445.96 -and $v[0].rb_importe -eq 160554.04) 'Caso de la captura: vuelto 39445.96, neto 160554.04'
$v=Lista @((Pago 'ARS' 130554.04)); $r=Normalizar $v 160554.04 30000
Check ($r.Ok -and $r.Vuelto -eq 0 -and $v[0].rb_importe -eq 130554.04) 'NC 30000 más efectivo exacto con centavos no se rechaza'
$v=Lista @((Pago 'ARS' 60000)); $r=Normalizar $v 85000 30000
Check ($r.Ok -and $r.Vuelto -eq 5000 -and $v[0].rb_importe -eq 55000) 'NC 30000 conserva imputación y efectivo queda neto 55000'
$v=Lista @((Pago 'BANCO' 80000),(Pago 'ARS' 10000)); $r=Normalizar $v 85000
Check ($r.Ok -and $v[0].rb_importe -eq 80000 -and $v[1].rb_importe -eq 5000) 'Pago mixto sólo reduce efectivo'
$v=Lista @((Pago 'ARS' 90000),(Pago 'ARS' 1000)); $r=Normalizar $v 85000
Check ($r.Ok -and $r.Vuelto -eq 6000 -and $v.Count -eq 1 -and $v[0].rb_importe -eq 85000 -and $v[0].rb_nro_valor -eq '001') 'Varios efectivos: distribuye descuento y elimina filas de importe cero'
$v=Lista @((Pago 'ARS' 5000)); $r=Normalizar $v 85000 85000
Check ($r.Ok -and $r.Vuelto -eq 5000 -and $v.Count -eq 0) 'NC cubre total: devuelve únicamente el efectivo recibido'
$v=Lista @(); $r=Normalizar $v 85000 85000
Check ($r.Ok -and $r.Vuelto -eq 0) 'Pago completo con NC no requiere efectivo'
$v=Lista @((Pago 'BANCO' 90000)); $r=Normalizar $v 85000
Check (-not $r.Ok -and $v[0].rb_importe -eq 90000) 'Sin efectivo no genera vuelto ni modifica otro instrumento'
$v=Lista @((Pago 'BANCO' 90000),(Pago 'ARS' 1000)); $r=Normalizar $v 85000
Check (-not $r.Ok -and $v[1].rb_importe -eq 1000) 'Excedente mayor al efectivo se rechaza sin mutar valores'
$v=Lista @((Pago 'ARS' 1000)); $r=Normalizar $v 85000 90000
Check (-not $r.Ok -and $v[0].rb_importe -eq 1000) 'NC superior a deuda no se convierte en efectivo'
$v=Lista @((Pago 'DOC' 80000),(Pago 'ARS' 10000)); $r=Normalizar $v 85000
Check (-not $r.Ok) 'DOC conserva su restricción de no generar vuelto'
$v=Lista @((Pago 'ARS' 100.01)); $r=Normalizar $v 100
Check ($r.Ok -and $r.Vuelto -eq 0.01 -and $v[0].rb_importe -eq 100) 'Un centavo de vuelto se descuenta'
$v=Lista @((Pago 'ARS' 99.99)); $r=Normalizar $v 100
Check (-not $r.Ok) 'Un centavo faltante se rechaza'
foreach ($monto in @(-1,0,100.001)) {
 $v=Lista @((Pago 'ARS' $monto)); $r=Normalizar $v 100
 Check (-not $r.Ok) "Rechaza importe inválido: $monto"
}
$v=Lista @((Pago 'ARS' 90000)); $r=[gc.caja.core.Servicios.Implementacion.Cajas.VueltoEfectivo]::Normalizar($v,85000,0,[gc.infraestructura.Dtos.Cajas.Response.ValoresInsResDto[]]@())
Check (-not $r.Ok -and $v[0].rb_importe -eq 90000) 'No asume que ARS es efectivo cuando no figura en catálogo EF'
$fake=[gc.infraestructura.Dtos.Cajas.Response.ValoresInsResDto]::new(); $fake.ins_id='BANCO'; $fake.tcf_id='BA'
$v=Lista @((Pago 'BANCO' 90000)); $r=[gc.caja.core.Servicios.Implementacion.Cajas.VueltoEfectivo]::Normalizar($v,85000,0,[gc.infraestructura.Dtos.Cajas.Response.ValoresInsResDto[]]@($fake))
Check (-not $r.Ok) 'No acepta como efectivo un instrumento de otra categoría'
Write-Output "$count verificaciones de vuelto aprobadas. Sin confirmar operaciones."
