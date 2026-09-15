$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot
$source = Get-Content (Join-Path $root 'Areas/Facturacion/Controllers/NotaCreditoController.cs') -Raw

# Ejecuta el bloque real del controlador en memoria, sin servicios ni base de datos.
function Get-MethodSource([string]$Name) {
    $match = [regex]::Match($source, "(?ms)^        private static [^\r\n]+ $Name\(.*?^        }")
    if (!$match.Success) { throw "No se encontro el metodo $Name" }
    $match.Value
}
$start = $source.IndexOf('var respuestaSp =', $source.IndexOf('public async Task<IActionResult> AgregarProductoManual'))
$end = $source.IndexOf('var codigoRespuesta =', $start)
if ($start -lt 0 -or $end -lt $start) { throw 'No se encontro el bloque de carga manual' }
$block = $source.Substring($start, $end - $start)
$dtoRoot = Join-Path $root '../gc.infraestructura/Dtos/Cajas/Response'
$dto = Get-Content (Join-Path $dtoRoot 'NCProductoBuscarResponseDto.cs') -Raw
$valida = Get-Content (Join-Path $dtoRoot 'NCValidaResponseDto.cs') -Raw
$valida = $valida.Substring($valida.IndexOf('namespace '))
$helpers = @('CrearResultadoProductoMensaje','RenumerarProductosDevolucion','IntegrarProductoManual','ObtenerMensajeBloqueo') | ForEach-Object { Get-MethodSource $_ }
$code = @"
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using gc.infraestructura.Dtos.Cajas.Response;
$dto
$valida
public class EstadoCarga {
    public List<NCProductoBuscarResponseDto> ProductosDevolucion = new();
    public string CoTipo = "DV", JsonProductosCalculado = "calculado", JsonSubtotal = "subtotal", JsonSorteo = "sorteo";
    public DateTime? FechaUltimoCalculoUtc = new DateTime(2026, 9, 14);
    public DateTime FechaUltimaCargaProductosUtc = new DateTime(2026, 9, 14);
}
public class LoggerPrueba { public void LogWarning(string texto, params object[] args) {} }
public class PruebaCarga {
    private LoggerPrueba? _logger = null;
    public int Guardados;
    private void GuardarContextoDevolucion(EstadoCarga estado) { Guardados++; }
    public (int Aceptados, int Advertencias, int Rechazos) Ejecutar(EstadoCarga contexto, List<NCProductoBuscarResponseDto> filas) {
        var resultado = new { ListaEntidad = filas };
        var correlationId = "prueba";
        $block
        return (productosAceptados.Count, advertencias.Count, rechazos.Count);
    }
    public string Bloqueo(int previa, int sinDetalle, int plazo) => ObtenerMensajeBloqueo(new NCValidaResponseDto {
        nc_ya_emitida = previa, nc_sin_detalle = sinDetalle, nc_fecha_supero_dias = plazo
    });
    $($helpers -join "`n")
}
"@
Add-Type -TypeDefinition $code
$script:checks = 0
function Assert-True([bool]$Condition, [string]$Message) {
    if (!$Condition) { throw $Message }
    $script:checks++
}
function New-Product([string]$Id, $Response = 0, $Quantity = 1) {
    $p = [gc.infraestructura.Dtos.Cajas.Response.NCProductoBuscarResponseDto]::new()
    $p.p_id = $Id; $p.respuesta = $Response; $p.cantidad_tot = $Quantity
    $p.p_pvta = 100; $p.item = 1
    return $p
}
$cases = @(
    @{ Name='rechazo luego OK'; Rows=@((New-Product '001' -1), (New-Product '001')) },
    @{ Name='OK luego rechazo'; Rows=@((New-Product '001'), (New-Product '001' -1)) },
    @{ Name='combo parcialmente rechazado'; Rows=@((New-Product '002' 2), (New-Product '003' -1)) },
    @{ Name='fila nula'; Rows=@((New-Product '002'), $null) },
    @{ Name='estado ausente'; Rows=@((New-Product '002'), (New-Product '003' $null)) },
    @{ Name='identificador vacio'; Rows=@((New-Product '002'), (New-Product '' 0)) },
    @{ Name='cantidad cero'; Rows=@((New-Product '002'), (New-Product '003' 0 0)) },
    @{ Name='cantidad negativa'; Rows=@((New-Product '002'), (New-Product '003' 0 -1)) }
)
foreach ($case in $cases) {
    $estado = [EstadoCarga]::new()
    $estado.ProductosDevolucion.Add((New-Product '001' 0 2))
    $before = $estado | ConvertTo-Json -Depth 10 -Compress
    $prueba = [PruebaCarga]::new()
    $result = $prueba.Ejecutar($estado, $case.Rows)
    Assert-True ($result.Item1 -eq 0) "$($case.Name): incorporo filas"
    Assert-True ($result.Item2 -eq 0 -and $result.Item3 -gt 0) "$($case.Name): respuesta incorrecta"
    Assert-True ($prueba.Guardados -eq 0) "$($case.Name): guardo sesion"
    Assert-True (($estado | ConvertTo-Json -Depth 10 -Compress) -ceq $before) "$($case.Name): altero estado"
}
foreach ($response in @(0, 2)) {
    $estado = [EstadoCarga]::new()
    $estado.ProductosDevolucion.Add((New-Product '001' 0 2))
    $prueba = [PruebaCarga]::new()
    $result = $prueba.Ejecutar($estado, @((New-Product '001' $response), (New-Product '002' $response)))
    Assert-True ($result.Item1 -eq 2 -and $result.Item3 -eq 0) 'Lote valido no incorporado'
    Assert-True ($result.Item2 -eq $(if ($response -gt 0) { 2 } else { 0 })) 'Advertencias incorrectas'
    Assert-True ($estado.ProductosDevolucion.Count -eq 2 -and $estado.ProductosDevolucion[0].cantidad_tot -eq 3) 'Acumulacion incorrecta'
    Assert-True ($estado.ProductosDevolucion[1].item -eq 2 -and $prueba.Guardados -eq 1) 'Numeracion o persistencia incorrecta'
    Assert-True ($estado.JsonProductosCalculado -eq '' -and $estado.JsonSubtotal -eq '' -and $null -eq $estado.FechaUltimoCalculoUtc) 'No invalido calculo previo'
}
$prueba = [PruebaCarga]::new()
$result = $prueba.Ejecutar([EstadoCarga]::new(), @())
Assert-True ($result.Item1 -eq 0 -and $prueba.Guardados -eq 0) 'Respuesta vacia modifico sesion'
Assert-True ($prueba.Bloqueo(1,0,0) -eq '') 'NC previa sigue bloqueada'
Assert-True ($prueba.Bloqueo(0,0,0) -eq '') 'Comprobante valido bloqueado'
Assert-True ($prueba.Bloqueo(1,1,0) -ne '') 'Se perdio bloqueo sin detalle'
Assert-True ($prueba.Bloqueo(1,0,1) -ne '') 'Se perdio bloqueo por plazo'
Write-Output "OK: $checks verificaciones de carga manual y validacion NC. Sin acceso a base de datos."
