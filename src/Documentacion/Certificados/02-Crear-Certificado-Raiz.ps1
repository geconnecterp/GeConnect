[CmdletBinding()]
param(
    [string]$CarpetaCA = (Join-Path $env:USERPROFILE 'GecoPKI'),
    [string]$OpenSSLPath = 'C:\Program Files\OpenSSL-Win64\bin\openssl.exe',
    [ValidateRange(1, 200)]
    [int]$Anios = 200
)

$ErrorActionPreference = 'Stop'
$clave = Join-Path $CarpetaCA 'private\cafeamerica-ca.key.pem'
$certificados = Join-Path $CarpetaCA 'certs'
$certificado = Join-Path $certificados 'cafeamerica-ca.cert.pem'
$certificadoWindows = Join-Path $certificados 'cafeamerica-ca.cer'
$configuracion = Join-Path $PSScriptRoot 'cafeamerica-root-ca.cnf'

foreach ($entrada in @($OpenSSLPath, $clave, $configuracion)) {
    if (-not (Test-Path -LiteralPath $entrada -PathType Leaf)) {
        throw "No se encuentra el archivo requerido: $entrada"
    }
}
foreach ($salida in @($certificado, $certificadoWindows)) {
    if (Test-Path -LiteralPath $salida) {
        throw "El archivo ya existe. No se sobrescribira: $salida"
    }
}

# UTC y anios calendario: incluye los bisiestos, sin aproximar a 200 * 365 dias.
$instanteEmision = [DateTime]::UtcNow
$formatoFecha = "yyyyMMddHHmmss'Z'"
$inicio = $instanteEmision.ToString($formatoFecha, [Globalization.CultureInfo]::InvariantCulture)
$vencimiento = $instanteEmision.AddYears($Anios).ToString($formatoFecha, [Globalization.CultureInfo]::InvariantCulture)

New-Item -ItemType Directory -Force -Path $certificados | Out-Null
Write-Host "Vigencia de la raiz: $Anios anios calendario. Inicio UTC: $inicio. Vencimiento UTC: $vencimiento."
Write-Host 'OpenSSL solicitara la contrasena de la clave existente. El script no la guarda.'

& $OpenSSLPath req -new -x509 -sha256 -not_before $inicio -not_after $vencimiento -config $configuracion -key $clave -out $certificado
if ($LASTEXITCODE -ne 0) {
    throw 'No se pudo crear el certificado. Conserve la clave y revise el error antes de reintentar.'
}

& $OpenSSLPath verify -x509_strict -check_ss_sig -CAfile $certificado $certificado
if ($LASTEXITCODE -ne 0) {
    throw 'Fallo la verificacion de la raiz. No instalar ni distribuir el certificado.'
}

& $OpenSSLPath x509 -in $certificado -outform DER -out $certificadoWindows
if ($LASTEXITCODE -ne 0) {
    throw 'Fallo la exportacion del certificado publico DER.'
}

# Comprobar que Windows interpreta correctamente la fecha superior al anio 2049.
$certificadoLeido = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 -ArgumentList $certificadoWindows
try {
    $inicioLeido = $certificadoLeido.NotBefore.ToUniversalTime().ToString($formatoFecha, [Globalization.CultureInfo]::InvariantCulture)
    $finLeido = $certificadoLeido.NotAfter.ToUniversalTime().ToString($formatoFecha, [Globalization.CultureInfo]::InvariantCulture)
    if ($inicioLeido -ne $inicio -or $finLeido -ne $vencimiento) {
        throw 'Las fechas interpretadas por Windows no coinciden con las solicitadas. No distribuir el certificado.'
    }
} finally {
    $certificadoLeido.Dispose()
}

& $OpenSSLPath x509 -in $certificado -noout -subject -issuer -serial -dates -fingerprint -sha256 -ext 'basicConstraints,keyUsage,subjectKeyIdentifier,authorityKeyIdentifier'
if ($LASTEXITCODE -ne 0) {
    throw 'Fallo la lectura de los datos publicos del certificado.'
}

Write-Host 'Fechas verificadas con Windows: coinciden con las solicitadas.'
Write-Host "Certificado publico PEM: $certificado"
Write-Host "Certificado publico DER: $certificadoWindows"
Write-Host 'Raiz creada y verificada. No se instalo confianza ni se modifico IIS.'
