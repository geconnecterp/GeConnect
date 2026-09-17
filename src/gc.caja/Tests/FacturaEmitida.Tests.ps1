$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot
$dto = Get-Content (Join-Path $root '../gc.infraestructura/Dtos/Cajas/Response/NCValidaResponseDto.cs') -Raw
$model = Get-Content (Join-Path $root 'Models/FacturaEmitidaRequest.cs') -Raw
$dto = $dto.Substring($dto.IndexOf('namespace '))
$model = $model.Substring($model.IndexOf('namespace ')).Replace('namespace gc.caja.Models.Facturacion;', 'namespace gc.caja.Models.Facturacion {') + "`n}"
Add-Type -TypeDefinition @"
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using gc.infraestructura.Dtos.Cajas.Response;
$dto
$model
"@
$script:checks = 0
function Check([bool]$Condition, [string]$Message) {
    if (!$Condition) { throw $Message }
    $script:checks++
}
$request = [gc.caja.Models.Facturacion.FacturaEmitidaRequest]::new()
$request.TcoId = '081'; $request.PuntoVenta = '2'; $request.Numero = '1234'
$tipo = ''; $compte = ''; $mensaje = ''
Check ($request.TryNormalizar([ref]$tipo, [ref]$compte, [ref]$mensaje)) 'Normalizacion invalida'
Check ($compte -ceq '0002-00001234') 'Relleno incorrecto'
Check ([gc.caja.Models.Facturacion.FacturaEmitidaRequest]::ConstruirClave($tipo, $compte, 0) -ceq '0810002-000012340') 'Clave incorrecta'
foreach ($nro in @('', '-1', '1.5', '123456789', '12A')) {
    $request.Numero = $nro
    Check (!$request.TryNormalizar([ref]$tipo, [ref]$compte, [ref]$mensaje)) "Acepto numero: $nro"
}
$candidatos = [System.Collections.Generic.List[gc.infraestructura.Dtos.Cajas.Response.NCValidaResponseDto]]::new()
foreach ($repetido in @(0, 2, 1)) {
    $fila = [gc.infraestructura.Dtos.Cajas.Response.NCValidaResponseDto]::new()
    $fila.tco_id = '081'; $fila.cm_compte = '0002-00001234'; $fila.cm_repetido = $repetido
    $fila.nc_ya_emitida = 1; $fila.nc_fecha_supero_dias = 1; $fila.nc_ctacte = 1
    $candidatos.Add($fila)
}
$ultimo = [gc.caja.Models.Facturacion.FacturaEmitidaRequest]::SeleccionarUltimaRepeticion($candidatos, '081', '0002-00001234')
Check ($ultimo.cm_repetido -eq 2) 'No selecciono la mayor repeticion'
Check ($ultimo.nc_ya_emitida -eq 1) 'Descarto factura por NC previa'
Check ($null -eq [gc.caja.Models.Facturacion.FacturaEmitidaRequest]::SeleccionarUltimaRepeticion($candidatos, '001', '0002-00001234')) 'Acepto otro tipo de comprobante'
foreach ($repetido in @($null, -1, 10)) {
    $rechazado = $false
    try { [void][gc.caja.Models.Facturacion.FacturaEmitidaRequest]::ConstruirClave('081', '0002-00001234', $repetido) } catch { $rechazado = $true }
    Check $rechazado 'Acepto repeticion no soportada por el SP'
}
Write-Output "OK: $script:checks verificaciones de identificacion, formato y mayor repeticion."
