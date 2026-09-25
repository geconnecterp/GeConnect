[CmdletBinding()]
param(
    [ValidateLength(1, 253)]
    [ValidatePattern('\A(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\z')]
    [string]$NombreDNS = 'api-dev.cafeamerica.com.ar',
    [string]$CarpetaPKI = (Join-Path $env:USERPROFILE 'GecoPKI'),
    [string]$OpenSSLPath = 'C:\Program Files\OpenSSL-Win64\bin\openssl.exe',
    [ValidateRange(1, 200)]
    [int]$Anios = 200
)

$ErrorActionPreference = 'Stop'
$NombreDNS = $NombreDNS.ToLowerInvariant()
$CarpetaPKI = [IO.Path]::GetFullPath($CarpetaPKI)
$carpetaServidor = Join-Path (Join-Path $CarpetaPKI 'servidores') $NombreDNS
$solicitud = Join-Path $carpetaServidor 'servidor.csr.pem'
$certificado = Join-Path $carpetaServidor 'servidor.cert.pem'
$extensiones = Join-Path $carpetaServidor 'servidor-cert.cnf'
$certificadoCA = Join-Path $CarpetaPKI 'certs\cafeamerica-ca.cert.pem'
$claveCA = Join-Path $CarpetaPKI 'private\cafeamerica-ca.key.pem'
$baseCA = Join-Path $CarpetaPKI 'ca-db'
$configuracionCA = Join-Path $baseCA 'openssl-ca.cnf'

foreach ($entrada in @($OpenSSLPath, $solicitud, $certificadoCA, $claveCA)) {
    if (-not (Test-Path -LiteralPath $entrada -PathType Leaf)) { throw "Falta el archivo: $entrada" }
}
if (Test-Path -LiteralPath $certificado) { throw "El certificado ya existe. No se sobrescribira: $certificado" }

& $OpenSSLPath req -in $solicitud -verify -noout -subject
if ($LASTEXITCODE -ne 0) { throw 'La firma de la CSR no es valida.' }
$detalle = & $OpenSSLPath req -in $solicitud -noout -text
if ($LASTEXITCODE -ne 0) { throw 'No se puede leer la CSR.' }
$san = @($detalle | Where-Object { $_ -match '^\s*DNS:' })
if ($san.Count -ne 1 -or $san[0].Trim() -cne "DNS:$NombreDNS") { throw 'El SAN de la CSR no coincide exactamente con el nombre esperado.' }
$subject = & $OpenSSLPath req -in $solicitud -noout -subject -nameopt RFC2253
if ($LASTEXITCODE -ne 0 -or $subject -notmatch ('(?:^subject=|,)CN=' + [regex]::Escape($NombreDNS) + '(?:,|$)')) { throw 'El CN de la CSR no coincide con el nombre esperado.' }
& $OpenSSLPath verify -x509_strict -check_ss_sig -CAfile $certificadoCA $certificadoCA
if ($LASTEXITCODE -ne 0) { throw 'No se pudo verificar la raiz.' }
$finCA = & $OpenSSLPath x509 -in $certificadoCA -enddate -dateopt iso_8601 -noout
if ($LASTEXITCODE -ne 0 -or $finCA -notmatch '^notAfter=') { throw 'No se pudo leer el vencimiento de la CA.' }
$cultura = [Globalization.CultureInfo]::InvariantCulture
$estiloUTC = [Globalization.DateTimeStyles]::AssumeUniversal -bor [Globalization.DateTimeStyles]::AdjustToUniversal
$finCAUtc = [DateTime]::ParseExact($finCA.Substring(9), "yyyy-MM-dd HH:mm:ss'Z'", $cultura, $estiloUTC)
$instanteEmision = [DateTime]::UtcNow
$finSolicitado = $instanteEmision.AddYears($Anios)
$finEfectivo = $finSolicitado
if ($finSolicitado -gt $finCAUtc) {
    $finEfectivo = $finCAUtc
    Write-Host 'La vigencia solicitada se limita al vencimiento de la CA existente.'
}
if ($finEfectivo -le $instanteEmision) { throw 'La CA no permite emitir un certificado vigente.' }
$inicio = $instanteEmision.ToString("yyyyMMddHHmmss'Z'", $cultura)
$vencimiento = $finEfectivo.ToString("yyyyMMddHHmmss'Z'", $cultura)

$contenidoExtensiones = @"
[server_certificate]
basicConstraints = critical, CA:false
keyUsage = critical, digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth
subjectKeyIdentifier = hash
authorityKeyIdentifier = keyid:always
subjectAltName = DNS:$NombreDNS
"@
if (Test-Path -LiteralPath $extensiones) {
    if ([IO.File]::ReadAllText($extensiones).Trim() -cne $contenidoExtensiones.Trim()) { throw 'La configuracion de extensiones existente difiere; revisar antes de continuar.' }
} else {
    [IO.File]::WriteAllText($extensiones, $contenidoExtensiones, [Text.Encoding]::ASCII)
}

# Un unico escritor por CA. No modificar index/serial manualmente mientras se emite.
$bloqueo = [IO.File]::Open((Join-Path $CarpetaPKI 'ca-emision.lock'), [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
try {
    if (-not (Test-Path -LiteralPath $baseCA)) {
        New-Item -ItemType Directory -Path (Join-Path $baseCA 'newcerts') | Out-Null
        $serialInicial = & $OpenSSLPath rand -hex 16
        if ($LASTEXITCODE -ne 0) { throw 'No se pudo inicializar la serie de la CA.' }
        [IO.File]::WriteAllText((Join-Path $baseCA 'index.txt'), '', [Text.Encoding]::ASCII)
        [IO.File]::WriteAllText((Join-Path $baseCA 'index.txt.attr'), "unique_subject = no`n", [Text.Encoding]::ASCII)
        [IO.File]::WriteAllText((Join-Path $baseCA 'serial'), "$serialInicial`n", [Text.Encoding]::ASCII)
        $contenidoCA = @'
[ca]
default_ca = CA_default

[CA_default]
database = ./ca-db/index.txt
new_certs_dir = ./ca-db/newcerts
serial = ./ca-db/serial
certificate = ./certs/cafeamerica-ca.cert.pem
private_key = ./private/cafeamerica-ca.key.pem
default_md = sha256
# La vigencia se pasa explicitamente mediante -startdate y -enddate.
default_crl_days = 30
policy = policy_server
unique_subject = no
copy_extensions = none

[policy_server]
countryName = match
organizationName = match
organizationalUnitName = optional
commonName = supplied
'@
        [IO.File]::WriteAllText($configuracionCA, $contenidoCA, [Text.Encoding]::ASCII)
    }
    foreach ($archivoBase in @('index.txt', 'index.txt.attr', 'serial', 'openssl-ca.cnf')) {
        if (-not (Test-Path -LiteralPath (Join-Path $baseCA $archivoBase) -PathType Leaf)) { throw 'La base de la CA esta incompleta. No reinicializarla; revisar el error.' }
    }
    if (-not (Test-Path -LiteralPath (Join-Path $baseCA 'newcerts') -PathType Container)) { throw 'Falta la carpeta newcerts de la CA.' }

    Write-Host "Certificado para $NombreDNS. Vigencia solicitada: $Anios anios. Inicio UTC: $inicio. Vencimiento UTC: $vencimiento."
    Write-Host 'Ingrese la contrasena de la AUTORIDAD CERTIFICANTE cuando OpenSSL la solicite.'
    Push-Location $CarpetaPKI
    try {
        & $OpenSSLPath ca -batch -notext -config $configuracionCA -extfile $extensiones -extensions server_certificate -startdate $inicio -enddate $vencimiento -md sha256 -in $solicitud -out $certificado
        if ($LASTEXITCODE -ne 0) { throw 'Fallo la emision. Conservar archivos e indice; revisar antes de reintentar.' }
    } finally { Pop-Location }
} finally { $bloqueo.Dispose() }

& $OpenSSLPath verify -x509_strict -purpose sslserver -verify_hostname $NombreDNS -CAfile $certificadoCA $certificado
if ($LASTEXITCODE -ne 0) { throw 'El certificado no supera la validacion de cadena, nombre o uso TLS.' }
$publicaCSR = & $OpenSSLPath req -in $solicitud -pubkey -noout
if ($LASTEXITCODE -ne 0) { throw 'No se pudo leer la clave publica de la CSR.' }
$publicaCertificado = & $OpenSSLPath x509 -in $certificado -pubkey -noout
if ($LASTEXITCODE -ne 0 -or ($publicaCSR -join "`n") -cne ($publicaCertificado -join "`n")) { throw 'La clave publica emitida no coincide con la CSR.' }
& $OpenSSLPath x509 -in $certificado -noout -subject -issuer -serial -dates -fingerprint -sha256 -ext 'subjectAltName,basicConstraints,keyUsage,extendedKeyUsage'
if ($LASTEXITCODE -ne 0) { throw 'No se pudo leer el resumen del certificado.' }
$fechasEmitidas = & $OpenSSLPath x509 -in $certificado -startdate -enddate -dateopt iso_8601 -noout
$inicioEsperado = 'notBefore=' + $instanteEmision.ToString("yyyy-MM-dd HH:mm:ss'Z'", $cultura)
$finEsperado = 'notAfter=' + $finEfectivo.ToString("yyyy-MM-dd HH:mm:ss'Z'", $cultura)
if ($LASTEXITCODE -ne 0 -or $fechasEmitidas -cnotcontains $inicioEsperado -or $fechasEmitidas -cnotcontains $finEsperado) {
    throw 'Las fechas del certificado no coinciden con las solicitadas. No continuar con el PFX.'
}
Write-Host 'Vigencia verificada: el certificado no supera el vencimiento de la CA.'
Write-Host 'Cadena, nombre DNS, uso TLS y clave publica verificados.'
Write-Host "Certificado del servidor: $certificado"
Write-Host "Registro de emisiones de la CA: $baseCA"
Write-Host 'Certificado emitido. Falta preparar el PFX e instalarlo en IIS.'
