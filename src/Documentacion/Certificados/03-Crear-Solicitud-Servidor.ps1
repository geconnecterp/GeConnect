[CmdletBinding()]
param(
    [ValidateLength(1, 253)]
    [ValidatePattern('\A(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\z')]
    [string]$NombreDNS = 'api-dev.cafeamerica.com.ar',
    [string]$CarpetaPKI = (Join-Path $env:USERPROFILE 'GecoPKI'),
    [string]$OpenSSLPath = 'C:\Program Files\OpenSSL-Win64\bin\openssl.exe'
)

$ErrorActionPreference = 'Stop'
$NombreDNS = $NombreDNS.ToLowerInvariant()
if (-not (Test-Path -LiteralPath $OpenSSLPath -PathType Leaf)) {
    throw "No se encuentra OpenSSL: $OpenSSLPath"
}
$carpetaServidor = Join-Path (Join-Path $CarpetaPKI 'servidores') $NombreDNS
$carpetaPrivada = Join-Path $carpetaServidor 'private'
$claveServidor = Join-Path $carpetaPrivada 'servidor.key.pem'
$solicitud = Join-Path $carpetaServidor 'servidor.csr.pem'
$configuracion = Join-Path $carpetaServidor 'servidor-request.cnf'

foreach ($salida in @($claveServidor, $solicitud, $configuracion)) {
    if (Test-Path -LiteralPath $salida) {
        throw "El archivo ya existe. No se sobrescribira: $salida"
    }
}

New-Item -ItemType Directory -Force -Path $carpetaPrivada | Out-Null
$contenidoConfiguracion = @"
[req]
prompt = no
distinguished_name = server_identity
req_extensions = server_request
default_md = sha256

[server_identity]
C = AR
O = Cafe America
OU = GECO
CN = $NombreDNS

[server_request]
basicConstraints = critical, CA:false
keyUsage = critical, digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth
subjectAltName = @server_names

[server_names]
DNS.1 = $NombreDNS
"@
[IO.File]::WriteAllText($configuracion, $contenidoConfiguracion, [Text.Encoding]::ASCII)

Write-Host "Se creara una clave propia para $NombreDNS y su solicitud CSR. No se usa la clave de la CA."
Write-Host 'Elegi una nueva contrasena para esta clave de servidor y repetila cuando OpenSSL lo solicite.'
& $OpenSSLPath req -new -newkey rsa:3072 -cipher aes-256-cbc -sha256 -config $configuracion -keyout $claveServidor -out $solicitud
if ($LASTEXITCODE -ne 0) {
    throw 'Fallo la creacion de la clave o CSR. Conserve los archivos y el error para revisar antes de reintentar.'
}

# La firma de la CSR prueba la posesion de la clave privada correspondiente.
& $OpenSSLPath req -in $solicitud -verify -noout -subject
if ($LASTEXITCODE -ne 0) {
    throw 'La firma de la CSR no es valida. No continuar con la emision.'
}
$detalleSolicitud = & $OpenSSLPath req -in $solicitud -noout -text
if ($LASTEXITCODE -ne 0) {
    throw 'No se pudo leer la CSR para comprobar las extensiones.'
}
$sanLineas = @($detalleSolicitud | Where-Object { $_ -match '^\s*DNS:' })
if ($sanLineas.Count -ne 1 -or $sanLineas[0].Trim() -cne "DNS:$NombreDNS") {
    throw 'El SAN de la CSR no coincide con el nombre solicitado.'
}
if (-not ($detalleSolicitud -match 'CA:FALSE')) {
    throw 'La CSR no contiene la restriccion CA:FALSE esperada.'
}
Write-Host "SAN verificado: DNS:$NombreDNS"
Write-Host 'Solicitud para servidor: CA:FALSE; autenticacion TLS de servidor.'
Write-Host "Clave privada cifrada: $claveServidor"
Write-Host "Solicitud publica CSR: $solicitud"
Write-Host "Configuracion: $configuracion"
Write-Host 'Paso completado: clave de servidor y CSR creadas y verificadas. El certificado y el PFX todavia no se emitieron.'
