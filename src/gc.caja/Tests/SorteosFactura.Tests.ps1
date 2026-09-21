$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot
$dto = Get-Content (Join-Path $root '../gc.infraestructura/Dtos/Cajas/FactSorteoJsonDto.cs') -Raw
$model = Get-Content (Join-Path $root 'Models/SorteosFactura.cs') -Raw
$dto = $dto.Substring($dto.IndexOf('namespace '))
$model = $model.Substring($model.IndexOf('namespace ')).Replace('namespace gc.caja.Models;', 'namespace gc.caja.Models {') + "`n}"
$references = @([Newtonsoft.Json.JsonConvert].Assembly.Location) + @(Get-ChildItem (Join-Path $PSHOME 'ref') -Filter '*.dll' | ForEach-Object FullName)
Add-Type -ReferencedAssemblies $references -CompilerOptions /nowarn:1701 -TypeDefinition @"
#nullable enable
using System;
using System.Collections.Generic;
using gc.infraestructura.Dtos.Cajas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
$dto
$model
"@
$count = 0
function Check($ok, $message) { if (!$ok) { throw $message }; $script:count++; Write-Output "OK $message" }
$json = '[{"so_sorteo":"0060","so_desc":"ARCOR BAGLEY"},{"so_sorteo":"0064","so_desc":"prueba 321"}]'
$actual = [gc.caja.Models.SorteosFactura]::DesdeJson($json)
Check ($actual.Count -eq 2) 'Recibe ambos sorteos del tester'
Check ($actual[0].So_Sorteo -ceq '0060' -and $actual[1].So_Sorteo -ceq '0064') 'Conserva los ceros iniciales'
Check ($actual[0].So_Desc -ceq 'ARCOR BAGLEY' -and $actual[1].So_Desc -ceq 'prueba 321') 'Conserva ambas descripciones'
# Mismo recorrido que FacturaSorteos (sesión), FinalizarCompra y DiferirPago.
$sessionJson = [Newtonsoft.Json.JsonConvert]::SerializeObject($actual)
$restored = [gc.caja.Models.SorteosFactura]::DesdeJson($sessionJson)
$confirmationJson = [Newtonsoft.Json.JsonConvert]::SerializeObject($restored)
Check ($confirmationJson -ceq $json) 'El JSON de confirmación conserva datos y nombres exactos del contrato'
foreach ($empty in @('', ' ', 'null', '{}', '[]', '[{}]', '[null,{}]')) {
    $actual = [gc.caja.Models.SorteosFactura]::DesdeJson($json)
    $actual = [gc.caja.Models.SorteosFactura]::DesdeJson($empty)
    Check ($actual.Count -eq 0) "Nuevo cálculo sin sorteos reemplaza los anteriores: $empty"
}
foreach ($invalid in @('bad', '1', '[1]', '[{"so_desc":"sin identificador"}]')) {
    $rejected = $false
    try { [void][gc.caja.Models.SorteosFactura]::DesdeJson($invalid) } catch { $rejected = $true }
    Check $rejected "Rechaza datos inválidos para no confirmar un sorteo incorrecto: $invalid"
}
$legacy = [gc.caja.Models.SorteosFactura]::DesdeJson('[{"So_Sorteo":"0060","So_Desc":"Anterior"}]')
Check ($legacy[0].So_Sorteo -ceq '0060') 'Puede leer el formato de sesión anterior'
Write-Output "$count pruebas de contrato y sesión aprobadas. No se confirmó ninguna venta."
