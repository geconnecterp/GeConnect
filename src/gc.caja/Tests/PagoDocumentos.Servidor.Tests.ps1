param([string]$BuildDirectory = (Join-Path $PSScriptRoot '../../.artifacts/nc-pagos-build'))
$ErrorActionPreference = 'Stop'
$script:passed = 0
$buildPath = (Resolve-Path -LiteralPath $BuildDirectory).Path
[void][Reflection.Assembly]::LoadFrom((Join-Path $buildPath 'Newtonsoft.Json.dll'))
[void][Reflection.Assembly]::LoadFrom((Join-Path $buildPath 'gc.infraestructura.dll'))
[void][Reflection.Assembly]::LoadFrom((Join-Path $buildPath 'gc.caja.core.dll'))
function New-Doc([decimal]$Amount = 70, [string]$Date = ([DateTime]::Today.AddDays(30).ToString('yyyy-MM-dd'))) {
    $value = [gc.infraestructura.Dtos.Cajas.Json_Valor]::new()
    $value.ins_id = 'DOC'
    $value.rb_importe = $Amount
    $value.rb_fecha_valor = [DateTime]::ParseExact($Date, 'yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture)
    return $value
}
function Test-Values($Values, [decimal]$Total = 100, [decimal]$Nc = 30) {
    $list = [Collections.Generic.List[gc.infraestructura.Dtos.Cajas.Json_Valor]]::new()
    foreach ($item in $Values) { $list.Add($item) }
    return [gc.caja.core.Servicios.Implementacion.Cajas.DocumentoCuentaCorriente]::ValidarYNormalizar($list, $Total, $Nc)
}
function Assert-That([bool]$Condition, [string]$Description) {
    if (-not $Condition) { throw "FALLO: $Description" }
    $script:passed++
    Write-Output "OK $Description"
}
$document = New-Doc
$expectedDate = $document.rb_fecha_valor.ToString('yyyy-MM-dd')
$document.rb_rec = 25; $document.rb_aux = 9; $document.rb_estado = 'A'
$document.rb_opcion_cuota = '0'; $document.rb_dato1_valor = 'Dato ajeno'; $document.id_externo = 'No aplica'
Assert-That ([string]::IsNullOrEmpty((Test-Values @($document)))) 'Servidor acepta documento que cubre el pendiente después de NC'
Assert-That ($document.rb_fecha_valor.ToString('yyyy-MM-dd') -eq $expectedDate -and $document.rb_fecha_valor.Kind -eq 'Unspecified') 'Conserva vencimiento como fecha civil'
Assert-That ($document.rb_estado -eq 'N' -and $document.rb_opcion_cuota -eq '1' -and $document.rb_rec -eq 0 -and $document.rb_aux -eq 0 -and $document.rb_cupon_manual -eq 'N' -and $document.rb_ch_dif -eq 'N') 'Normaliza estado, cuota, indicadores y recargos DOC'
Assert-That ($document.rb_dato1_valor -eq '' -and $document.rb_dato2_valor -eq '' -and $document.rb_dato3_valor -eq '' -and $document.id_externo -eq '') 'Elimina datos que no corresponden al contrato DOC'
foreach ($amount in @(0, -1, 0.001, 70.01)) {
    Assert-That (-not [string]::IsNullOrEmpty((Test-Values @((New-Doc $amount))))) "Rechaza monto $amount"
}
$missingDate = New-Doc
$missingDate.rb_fecha_valor = [DateTime]::MinValue
Assert-That (-not [string]::IsNullOrEmpty((Test-Values @($missingDate)))) 'Rechaza vencimiento omitido o por defecto'
$yesterday = [DateTime]::Today.AddDays(-1).ToString('yyyy-MM-dd')
$today = [DateTime]::Today.ToString('yyyy-MM-dd')
$tomorrow = [DateTime]::Today.AddDays(1).ToString('yyyy-MM-dd')
Assert-That ((Test-Values @((New-Doc 70 $yesterday))) -match 'anterior') 'Rechaza vencimiento de ayer'
Assert-That ([string]::IsNullOrEmpty((Test-Values @((New-Doc 70 $today))))) 'Acepta vencimiento de hoy sin comparar la hora'
Assert-That ([string]::IsNullOrEmpty((Test-Values @((New-Doc 70 $tomorrow))))) 'Acepta vencimiento de mañana'
$cash = New-Doc 20
$cash.ins_id = 'PES'; $cash.rb_estado = 'A'; $cash.rb_opcion_cuota = '0'
Assert-That ([string]::IsNullOrEmpty((Test-Values @((New-Doc 50), $cash)))) 'Permite DOC parcial con efectivo y NC'
Assert-That ($cash.rb_estado -eq 'A' -and $cash.rb_opcion_cuota -eq '0') 'No altera el contrato de otros instrumentos'
Assert-That (-not [string]::IsNullOrEmpty((Test-Values @((New-Doc 50.01), $cash)))) 'Impide excedente de un centavo con pagos combinados'
Assert-That (-not [string]::IsNullOrEmpty((Test-Values @((New-Doc 40), (New-Doc 40))))) 'Controla el total de varios documentos'
$settings = [Newtonsoft.Json.JsonSerializerSettings]::new()
$settings.DateFormatString = 'yyyy-MM-dd HH:mm:ss.fff'
$settings.DateTimeZoneHandling = [Newtonsoft.Json.DateTimeZoneHandling]::Unspecified
$json = [Newtonsoft.Json.JsonConvert]::SerializeObject($document, $settings)
Assert-That ($json.Contains('"rb_fecha_valor":"' + $expectedDate + ' 00:00:00.000"')) 'Serialización real conserva vencimiento para el backend'
$catalog = [Collections.Generic.List[gc.infraestructura.Dtos.Cajas.Response.ValoresMPResDto]]::new()
Assert-That (-not [gc.caja.core.Servicios.Implementacion.Cajas.DocumentoCuentaCorriente]::EstaHabilitado($catalog)) 'Catálogo vacío no habilita DOC'
$mp = [gc.infraestructura.Dtos.Cajas.Response.ValoresMPResDto]::new()
$mp.tcf_id = 'EF'; $mp.tcf_desc = 'Documento en CtaCte'
$catalog.Add($mp)
Assert-That (-not [gc.caja.core.Servicios.Implementacion.Cajas.DocumentoCuentaCorriente]::EstaHabilitado($catalog)) 'La descripción visible no habilita un medio incorrecto'
$mp.tcf_id = 'DO'; $mp.tcf_desc = 'Descripción modificada'
Assert-That ([gc.caja.core.Servicios.Implementacion.Cajas.DocumentoCuentaCorriente]::EstaHabilitado($catalog)) 'DO habilita DOC aunque cambie su descripción'
$instrument = [gc.caja.core.Servicios.Implementacion.Cajas.DocumentoCuentaCorriente]::CrearInstrumentoSimulado()
Assert-That ($instrument.ins_id -eq 'DOC' -and $instrument.tcf_id -eq 'DO' -and $instrument.ins_tiene_vto -eq 'S') 'Instrumento simulado DOC pertenece a DO y requiere vencimiento'
Assert-That ($instrument.ins_comision -eq 0 -and $instrument.ins_comision_fija -eq 0 -and $instrument.ins_vuelto -eq 'N') 'Documento simulado no genera recargos ni vuelto'
Assert-That (-not [gc.caja.core.Servicios.Implementacion.Cajas.DocumentoCuentaCorriente]::EsMedioDocumento('DEF')) 'DEF de otros medios no se convierte en documento'
Write-Output "$script:passed verificaciones del servidor aprobadas. Sin invocar SP de confirmación."

