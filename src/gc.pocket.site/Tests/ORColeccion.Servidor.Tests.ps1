param([Parameter(Mandatory=$true)][string]$BuildDirectory)
$ErrorActionPreference = 'Stop'
[void][Reflection.Assembly]::LoadFrom((Join-Path (Resolve-Path $BuildDirectory) 'gc.infraestructura.dll'))
$script:passed = 0
function Check([bool]$Condition, [string]$Name) {
    if (-not $Condition) { throw "FALLÓ: $Name" }
    $script:passed++; Write-Output "OK: $Name"
}
function Product([decimal]$Own = 0, [decimal]$Total = 0) {
    $p = [gc.infraestructura.Dtos.OrdenReparto.ORProductoDto]::new()
    $p.item=2; $p.pedido=500; $p.colectado=$Own; $p.colectado_x_p=$Total
    $p.up_id='07'; $p.unidad_pres=10; $p.bulto=$Own / 10
    return $p
}
function Reject([scriptblock]$Action, [string]$Name) {
    $rejected=$false
    try { & $Action | Out-Null } catch { $rejected=$true }
    Check $rejected $Name
}
$rule = [gc.infraestructura.Dtos.OrdenReparto.ORColeccionReglas]
foreach ($own in @(0,200,500,600)) { $p=Product $own $own; Check ($rule::PermiteCarga($p,'jjbenz')) "Propia/inicial $own" }
$p=Product 0 500
Check (-not $rule::PermiteCarga($p,'jjbenz')) 'Completo solo por otros no carga'
$p.colectado_x_p=600
Check (-not $rule::PermiteReemplazo($p,'jjbenz')) 'Excedido solo por otros no reemplaza'
$p=Product 1 600; $p.resultado='21'
Check ($rule::PermiteCarga($p,'jjbenz')) 'Original con colección propia conserva flexibilidad en estado 21'
$p=Product 12 30; $p.pedido=30; $p.colectado_remplazo=18
Check ($rule::Diferencia($p) -eq 0) '30 = 12 + 18 + 0'
$p.colectado_remplazo=10; $p.colectado_otros=2
Check ($rule::Diferencia($p) -eq 6) 'Faltan 6'
$p.colectado_remplazo=20
Check ($rule::Diferencia($p) -eq -4) 'Sobran 4'
$p=Product 200 200; $p.remplazo='s'; $p.remplazo_usu_id=' JJBENZ '
Check ($rule::PermiteCarga($p,'jjbenz')) 'Reemplazo propio carga'
Check ($rule::PermiteEliminar($p,'jjbenz')) 'Reemplazo propio elimina'
Check (-not $rule::PermiteReemplazo($p,'jjbenz')) 'No reemplaza otro reemplazo'
Check (-not $rule::PermiteCarga($p,'otro')) 'No carga reemplazo ajeno'
Check (-not $rule::PermiteEliminar($p,'otro')) 'No elimina reemplazo ajeno'
Check (-not $rule::PermiteCarga($p,'')) 'Usuario ausente no adquiere propiedad'
$p.remplazo='N'; $p.resultado='40'
Check ($rule::EsReemplazo($p)) 'Código 40 identifica reemplazo aunque falte marca'
foreach ($state in @('E1','21')) { $p.remplazo='S'; $p.resultado=$state; Check (-not $rule::PermiteCarga($p,'jjbenz')) "Estado $state bloquea reemplazo" }
$p=Product; $p.item=$null
Check (-not $rule::PermiteCarga($p,'jjbenz')) 'Sin item no carga'
$p=Product
Check (-not $rule::PermiteEliminar($p,'jjbenz')) 'Sin colección propia no elimina'
foreach ($code in @('00','01','02','03','04','40','10','11','20','21','PE','E0','E1')) {
    $p.resultado=$code; $p.resultado_msj=' '
    Check ($rule::Mensaje($p) -ne 'Estado informado por el servidor') "Respaldo $code"
}
$p.resultado='99'; $p.resultado_msj=' Nuevo mensaje DBA '
Check ($rule::Mensaje($p) -eq 'Nuevo mensaje DBA') 'Desconocido conserva mensaje SP'
$p.resultado_msj=$null
Check ($rule::Mensaje($p) -eq 'Estado informado por el servidor') 'Desconocido sin texto genérico'
$p=Product 200 200
Check ($rule::CoincideColeccion($p,200,20,0,10)) 'Snapshot coincide'
Check (-not $rule::CoincideColeccion($p,199,20,0,10)) 'Cambio concurrente detectado'
Check (-not $rule::CoincideColeccion($p,$null,20,0,10)) 'Snapshot ausente no válido'
$r=$rule::ResolverCarga($p,'acumular','07',10,30,0,300)
Check ($r.Item1 -eq 50 -and $r.Item2 -eq 0 -and $r.Item3 -eq 500) 'Acumula propia 200 + nueva 300'
$p.colectado_otros=100
$r=$rule::ResolverCarga($p,'acumular','07',10,30,0,300)
Check ($r.Item3 -eq 500) 'No suma colección de otros'
$r=$rule::ResolverCarga($p,'sobrescribir','07',10,30,0,300)
Check ($r.Item3 -eq 300) 'Sobrescribe en 300'
Reject { $rule::ResolverCarga($p,'nueva','07',10,30,0,300) } 'No omite consulta de propia'
Reject { $rule::ResolverCarga($p,'acumular','07',12,25,0,300) } 'No acumula con presentación distinta'
$p.us=1
Reject { $rule::ResolverCarga($p,'acumular','07',10,30,0,300) } 'No acumula desglose inconsistente'
$p=Product
$r=$rule::ResolverCarga($p,'nueva','KG',1,0,0.5,0.5)
Check ($r.Item3 -eq 0.5) 'Permite fracción menor de 1'
$r=$rule::ResolverCarga($p,'nueva','07',10,60,0,600)
Check ($r.Item3 -eq 600) 'No bloquea exceso localmente'
Reject { $rule::ResolverCarga($p,'nueva','07',10,0,0.5,0.5) } 'Rechaza fracción por unidades'
Reject { $rule::ResolverCarga($p,'nueva','07',10,0,0,0) } 'Rechaza cero en carga'
Reject { $rule::ResolverCarga($p,'nueva','07',10,-1,20,10) } 'Rechaza bultos negativos'
Reject { $rule::ResolverCarga($p,'nueva','07',10,1,0,11) } 'Rechaza desglose incoherente'
Reject { $rule::ResolverCarga($p,'nueva','KG',1,0,0.1234,0.1234) } 'Rechaza precisión excesiva'
Reject { $rule::ResolverCarga($p,'otro','07',10,1,0,10) } 'Rechaza modo desconocido'
Write-Output "$script:passed pruebas de reglas OR aprobadas (sin acceso a base de datos)."
