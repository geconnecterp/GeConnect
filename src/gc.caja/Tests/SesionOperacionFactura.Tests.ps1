$ErrorActionPreference='Stop'
$source=Get-Content (Join-Path $PSScriptRoot '../Models/SesionOperacionFactura.cs') -Raw
Add-Type -TypeDefinition ("using System;`n"+$source)
$state=[Collections.Generic.Dictionary[string,string]]::new()
foreach($key in @('ProductosSeleccionados','FacturaProductos','FacturaSubtotales','FacturaSorteos','AutorizacionRemota:Vigente:FACTURACION_CAMBIO_LP','CajaActual','Token','ClienteActual','LP_Id','Backup','FacturasSeleccionadasParaCobro')) { $state[$key]='CONSERVAR' }
$remove=[Action[string]]{param($key)[void]$state.Remove($key)}
$save=[Action[string,string]]{param($key,$value)$state[$key]=$value}
$id=[gc.caja.Models.SesionOperacionFactura]::Reiniciar($remove,$save)
$count=0
function Check($ok,$message){if(-not $ok){throw $message};$script:count++;Write-Output "OK $message"}
foreach($key in @('ProductosSeleccionados','FacturaProductos','FacturaSubtotales','FacturaSorteos','AutorizacionRemota:Vigente:FACTURACION_CAMBIO_LP')) {Check (-not $state.ContainsKey($key)) "Reinicio elimina $key"}
foreach($key in @('CajaActual','Token','ClienteActual','LP_Id','Backup','FacturasSeleccionadasParaCobro')) {Check ($state[$key] -eq 'CONSERVAR') "Reinicio conserva $key"}
Check ($id -eq $state['Facturacion:OperacionId'] -and $id.Length -eq 32) 'Nueva operación tiene identidad propia'
$next=[gc.caja.Models.SesionOperacionFactura]::Reiniciar($remove,$save)
Check ($next -ne $id) 'La identidad autorizada anterior no coincide con la nueva operación'
Write-Output "$count verificaciones reales del reinicio de sesión aprobadas."
