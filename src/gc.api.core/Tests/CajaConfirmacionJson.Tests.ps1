param(
    [string]$AssemblyPath = (Join-Path $PSScriptRoot '..\bin\Debug\net8.0\gc.api.core.dll')
)

$ErrorActionPreference = 'Stop'
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $AssemblyPath))
$type = $assembly.GetType('gc.api.core.Servicios.LineaCaja.CajaConfirmacionJson', $true)
$method = $type.GetMethod('Normalizar')
$fields = $type.GetField('CamposNumericos', [Reflection.BindingFlags]'NonPublic,Static').GetValue($null)
$integerFields = @('po_limite','bultos','item','orden','cm_compte_cuota','rb_nro_valor','rb_opcion_cuota')
$script:checks = 0

function Convert-ConfirmationJson([string]$Json) {
    $method.Invoke($null, @($Json))
}

function Assert-Equal($Actual, $Expected, [string]$Message) {
    if ($Actual -cne $Expected) { throw "$Message. Actual: $Actual; esperado: $Expected" }
    $script:checks++
}

$previousCulture = [Threading.Thread]::CurrentThread.CurrentCulture
try {
    foreach ($culture in @('es-AR', 'en-US')) {
        [Threading.Thread]::CurrentThread.CurrentCulture = [cultureinfo]::GetCultureInfo($culture)
        foreach ($field in $fields) {
            $integer = $field -in $integerFields
            $values = if ($integer) { @('-42', '"-42"', '"42"', '0') } else { @('-42.18', '"-42,18"', '"42.18"', '0') }
            foreach ($value in $values) {
                $json = '[{"' + $field + '":' + $value + '}]'
                $normalized = Convert-ConfirmationJson $json
                $result = $normalized | ConvertFrom-Json
                $expected = if ($value -eq '0') { [decimal]0 } elseif ($integer) { [decimal]42 } else { [decimal]42.18 }
                Assert-Equal $result[0].$field $expected "$culture / $field / $value"
                if ($integer) {
                    Assert-Equal $normalized ('[{"' + $field + '":' + $expected.ToString('0', [cultureinfo]::InvariantCulture) + '}]') "Tipo entero de $field"
                }
            }
        }
    }

    $record = @{
        cv_importe = '-42166.18'; cv_importe_ori = '-42166,18'
        cm_compte = '0002-00000015'; cta_id = 'C0195587'
        cv_fecha_carga = '2026-09-14 17:03:05.123'; cv_fecha_vto = '2026-09-14T17:03:05.000'
        cv_concepto = 'NC "relacionada" C:\Caja - saldo'; ve_id = '01'; po = $false
        so_sorteo = '-001'; rb_dato1_valor = '-12345'
    }
    $json = Convert-ConfirmationJson (ConvertTo-Json -InputObject @($record) -Compress)
    $reader = [Newtonsoft.Json.JsonTextReader]::new([IO.StringReader]::new($json))
    $reader.DateParseHandling = [Newtonsoft.Json.DateParseHandling]::None
    $result = [Newtonsoft.Json.Linq.JToken]::Load($reader)
    Assert-Equal ([decimal]$result[0]['cv_importe'].ToString()) ([decimal]42166.18) 'NC aplicada positiva'
    Assert-Equal ([decimal]$result[0]['cv_importe_ori'].ToString()) ([decimal]42166.18) 'NC original positiva'
    foreach ($field in @('cm_compte','cta_id','cv_fecha_carga','cv_fecha_vto','cv_concepto','ve_id','so_sorteo','rb_dato1_valor')) {
        Assert-Equal $result[0][$field].ToString() $record[$field] "Preservar $field"
    }
    $reader.Dispose()
    Assert-Equal (Convert-ConfirmationJson '[]') '[]' 'Lista vacia'
    Assert-Equal (Convert-ConfirmationJson '{}') '{}' 'Objeto vacio'
    Assert-Equal (Convert-ConfirmationJson '[{"importe":null,"po":false}]') '[{"importe":null,"po":false}]' 'Nulo y booleano preservados'
    Assert-Equal (Convert-ConfirmationJson '[{"p_pneto":-123456789012345.1234567890}]') '[{"p_pneto":123456789012345.1234567890}]' 'Precision decimal'
    Assert-Equal (Convert-ConfirmationJson '[{"orden":1.0,"importe":-42166.18}]') '[{"orden":1,"importe":42166.18}]' 'Regresion OPENJSON subtotales'
    Assert-Equal (Convert-ConfirmationJson '[{"cm_compte_cuota":"1","cv_importe":"-42166.18"}]') '[{"cm_compte_cuota":1,"cv_importe":42166.18}]' 'Regresion OPENJSON union'

    foreach ($invalid in @('[{"importe":"abc"}]', '[{"importe":true}]', '[{"importe":"1.234,56"}]', '[{"orden":1.5}]', '[{"cm_compte_cuota":"1,5"}]', '[1]', '"texto"')) {
        $rejected = $false
        try { $null = Convert-ConfirmationJson $invalid } catch { $rejected = $true }
        Assert-Equal $rejected $true "Rechazar datos invalidos: $invalid"
    }
    "PASS: $script:checks verificaciones de importes positivos y preservacion del contrato."
}
finally {
    [Threading.Thread]::CurrentThread.CurrentCulture = $previousCulture
}
