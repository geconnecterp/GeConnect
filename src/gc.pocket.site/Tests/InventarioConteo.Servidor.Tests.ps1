param([Parameter(Mandatory=$true)][string]$BuildDirectory)
$ErrorActionPreference = 'Stop'
[void][Reflection.Assembly]::LoadFrom((Join-Path (Resolve-Path $BuildDirectory) 'gc.infraestructura.dll'))
$script:passed = 0
function Check([bool]$ok, [string]$name) {
    if (-not $ok) { throw "FALLO: $name" }
    $script:passed++; Write-Output "OK: $name"
}
$rule = [gc.infraestructura.Dtos.Inventario.InventarioConteoReglas]
$r = [gc.infraestructura.Dtos.Inventario.Request.InventarioRequestDto]::new()
$r.inv_nro='INV-1'; $r.usu_id='jjbenz'; $r.tipo='P'; $r.tipo_id='0'
Check ($null -eq $rule::ValidarContexto($r)) 'Planilla nueva cero'
foreach($n in @('NUEVA','-1','32768','%','1.5')) { $r.tipo_id=$n; Check ($null -ne $rule::ValidarContexto($r)) "Planilla rechazada $n" }
$r.tipo='B'; $r.tipo_id='01000000000'
Check ($null -eq $rule::ValidarContexto($r)) 'BOX once dígitos'
$r.tipo_id='010000000001'; Check ($null -ne $rule::ValidarContexto($r)) 'BOX largo rechazado'
$r.tipo_id='01000000000'; $r.usu_id='%'; Check ($null -ne $rule::ValidarContexto($r)) 'Usuario comodín rechazado'
$r.usu_id='jjbenz'; $r.tipo='R'; Check ($null -ne $rule::ValidarContexto($r)) 'Modalidad fuera de Pocket rechazada'
$p = [gc.infraestructura.Dtos.Inventario.Dto.InventarioConteoDto]::new()
$p.invd_bulto=15; $p.invd_unidad_pres=12; $p.invd_unidad_suelta=2; $p.invd_cantidad=182
Check ($null -eq $rule::ValidarCantidad($p,'U')) '15 x 12 + 2 = 182'
$p.invd_cantidad=181; Check ($null -ne $rule::ValidarCantidad($p,'U')) 'Cantidad manipulada rechazada'
$p.invd_bulto=0; $p.invd_unidad_suelta=5; $p.invd_cantidad=5
Check ($null -eq $rule::ValidarCantidad($p,'U')) 'Solo US sin bultos'
$p.invd_unidad_pres=1; $p.invd_unidad_suelta=[decimal]0.125; $p.invd_cantidad=[decimal]0.125
Check ($null -eq $rule::ValidarCantidad($p,'P')) 'Pesable con tres decimales'
Check ($null -ne $rule::ValidarCantidad($p,'U')) 'Fracción rechazada para unidades'
Check ($null -ne $rule::ValidarCantidad($p,'')) 'Tipo de unidad ausente rechazado'
$p.invd_cantidad=0; Check ($null -ne $rule::ValidarCantidad($p,'P')) 'Cantidad cero rechazada'
$p.invd_cantidad=[decimal]0.1234; $p.invd_unidad_suelta=$p.invd_cantidad
Check ($null -ne $rule::ValidarCantidad($p,'P')) 'Exceso de decimales rechazado'
Write-Output "$script:passed pruebas de reglas de servidor aprobadas, sin base de datos."
