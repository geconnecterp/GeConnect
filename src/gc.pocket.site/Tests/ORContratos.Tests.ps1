$ErrorActionPreference = 'Stop'
$src = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$script:passed = 0
function Check([bool]$Condition, [string]$Name) {
    if (-not $Condition) { throw "FALLÓ: $Name" }
    $script:passed++; Write-Output "OK: $Name"
}
$api = Get-Content -Raw (Join-Path $src 'gc.api.core/Servicios/OrdenRepartoServicio.cs')
$expected = @('@or_compte','@adm_id','@usu_id','@item','@box_id','@desarma_box','@p_id','@unidad_pres','@bulto','@us','@cantidad','@fv','@remplazar','@remplazar_box_id','@remplazar_p_id') | Sort-Object
foreach ($method in @('ValidaProductoCarritoOR','ResguardarProductoCarrito')) {
    $body = [regex]::Match($api, '(?s)public RespuestaDto '+$method+'\(.*?var result =').Value
    $names = @([regex]::Matches($body,'new SqlParameter\("([^"]+)"') | ForEach-Object { $_.Groups[1].Value })
    Check ($names.Count -eq 15) "$method envía exactamente 15 parámetros"
    Check (-not (Compare-Object $expected ($names | Sort-Object))) "$method usa nombres contractuales"
    Check ($body.Contains('request.item.Value')) "$method conserva item"
}
$controller = Get-Content -Raw (Join-Path $src 'gc.pocket.site/Areas/PocketPpal/Controllers/ORController.cs')
$pipeline = [regex]::Match($controller,'(?s)private async Task<RespuestaGenerica<RespuestaDto>> ValidarYCargarOR.*?private GridCoreSmart').Value
Check ($pipeline.IndexOf('if (!validacion.Ok) return validacion;') -lt $pipeline.IndexOf('_orServicio.ResguardarProductoCarrito')) 'Carga está después de la guarda de Valida'
Check ($pipeline.Contains('_orServicio.ValidaProductoCarritoOR')) 'Pipeline invoca validación real'
Check ($controller.Contains('await ConsultarProductosOR(session)')) 'Ordenar reconsulta SP'
Check ($controller.Contains('eliminar: true')) 'Eliminar utiliza regla de propiedad fresca'
Check ($controller.Contains('ORColeccionReglas.CoincideColeccion')) 'Guardar compara snapshot fresco'
Check ($controller.Contains('ProductoBase.P_id != p_id')) 'Guardar verifica producto consultado'
Check (-not $controller.Contains('prod.pedido < cantidad')) 'Sin bloqueo local por pedido'
Check (-not $controller.Contains('cantidad < 1')) 'Sin bloqueo local a fracciones positivas'
$grid = Get-Content -Raw (Join-Path $src 'gc.pocket.site/Areas/PocketPpal/Views/OR/_gridORListaProducto.cshtml')
foreach ($rule in @('PermiteCarga','PermiteReemplazo','PermiteEliminar','Mensaje','Diferencia')) {
    Check ($grid.Contains("ORColeccionReglas.$rule")) "Grilla comparte $rule con reglas probadas"
}
Check ($grid.Contains('!esReemplazo && diferenciaColectado != 0')) 'No presenta diferencia para reemplazo o diferencia cero'
Check ($grid.Contains('asp-route-item=')) 'Links preservan item'
$view = Get-Content -Raw (Join-Path $src 'gc.pocket.site/Areas/PocketPpal/Views/OR/ORCargaCarrito.cshtml')
Check (-not $view.Contains('id="btnCargar"')) 'No renderiza CARGAR genérico'
Check ($view.Contains('model="Model"')) 'Grilla inicial se renderiza sin segunda consulta'
Write-Output "$script:passed comprobaciones estáticas de integración OR aprobadas (no sustituyen pruebas con SQL)."
