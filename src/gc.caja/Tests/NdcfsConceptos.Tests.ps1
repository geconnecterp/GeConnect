$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot
$source = Get-Content (Join-Path $root 'Areas/Facturacion/Controllers/NotaDebitoCreditoController.cs') -Raw
$jsonMethod = [regex]::Match($source, '(?s)        private static object CrearJsonConcepto\(.*?(?=        private bool DebeImprimirComprobanteElectronico)').Value.Replace('private static object', 'public static object')
$ivaMethod = [regex]::Match($source, '(?s)        private static decimal CalcularIvaManual\(.*?(?=        private static void AgregarJson)').Value
$conceptClass = [regex]::Match($source, '(?s)        public sealed class NotaDebitoCreditoConceptoRequest.*?\r?\n        }').Value
$responseDto = Get-Content (Join-Path $root '../gc.infraestructura/Dtos/Cajas/Response/CalculaFilasResDto.cs') -Raw
if (!$jsonMethod -or !$ivaMethod -or !$conceptClass) { throw 'No se encontraron metodos de produccion' }
Add-Type -TypeDefinition @"
using System;
$responseDto
public class NdcfsConceptosTest {
    public class CuentaTest { public string cta_id { get; set; } = "C0017189"; }
    public class NotaDebitoCreditoContextoSesion { public CuentaTest Cuenta { get; set; } = new CuentaTest(); }
$conceptClass
$jsonMethod
$ivaMethod
}
"@
$script:checks = 0
function Check([bool]$condition, [string]$message) {
    if (!$condition) { throw $message }
    $script:checks++
}
$casos = @(
    @{Texto='publicidad'; Neto=5000d; Iva=21d; Cantidad=1d; Unitario=6050d; Total=6050d; ImpIva=1050d},
    @{Texto='Otra Publicidad'; Neto=60000d; Iva=21d; Cantidad=5d; Unitario=72600d; Total=363000d; ImpIva=63000d},
    @{Texto='publicidad gondola'; Neto=1223.33d; Iva=21d; Cantidad=4d; Unitario=1480.23d; Total=5920.92d; ImpIva=1027.6d},
    @{Texto='sin iva'; Neto=150d; Iva=0d; Cantidad=3d; Unitario=150d; Total=450d; ImpIva=0d},
    @{Texto='media alicuota'; Neto=100d; Iva=10.5d; Cantidad=2d; Unitario=110.5d; Total=221d; ImpIva=21d}
)
foreach ($modo in @('ND','NC','FS')) {
    $filas = @()
    foreach ($caso in $casos) {
        $concepto = [NdcfsConceptosTest+NotaDebitoCreditoConceptoRequest]::new()
        $concepto.Concepto = $caso.Texto; $concepto.NetoGravado = $caso.Neto
        $concepto.AlicuotaIva = $caso.Iva; $concepto.Cantidad = $caso.Cantidad
        $fila = [NdcfsConceptosTest]::CrearJsonConcepto([NdcfsConceptosTest+NotaDebitoCreditoContextoSesion]::new(), $modo, $concepto, $filas.Count + 1)
        Check ($fila.p_pvta -eq $caso.Unitario) "$modo unitario incorrecto: $($caso.Texto)"
        Check ($fila.p_pvta_tot -eq $caso.Total) "$modo total incorrecto: $($caso.Texto)"
        Check ($fila.cm_iva -eq $caso.ImpIva) "$modo IVA incorrecto: $($caso.Texto)"
        Check ($fila.p_pvta * $fila.cantidad_tot -eq $fila.p_pvta_tot) 'Falla validacion cantidades por precio del SP'
        Check ($fila.p_desc -ceq $caso.Texto.ToUpperInvariant()) 'Concepto no normalizado'
        Check ($fila.co_tipo -eq $modo) 'Cambio el tipo de operacion'
        $filas += $fila
    }
    Check (($filas[0..2] | Measure-Object p_pvta_tot -Sum).Sum -eq 374970.92d) 'Total de venta incorrecto'
    Check (($filas[0..2] | Measure-Object cantidad_tot -Sum).Sum -eq 10) 'Cantidad de control incorrecta'
}
$dto = [gc.infraestructura.Dtos.Cajas.Response.CalculaFilasResDto]::new()
$dto.tipo = 'ER'; $dto.concepto = 'Existen registros de productos de Cantidades por precio de venta distinto del total'
Check ($dto.tipo -eq 'ER' -and $dto.concepto.Length -gt 0) 'DTO no conserva rechazo SP'
Check ($source.Contains('resultado.tipo?.Trim(), "ER"')) 'Controlador no valida ER'
Check ($source.Contains('p_pvta = precioUnitario')) 'Regresion JSON unitario'
Write-Output "OK: $script:checks verificaciones de metodos C# reales, importes y contrato de error."
