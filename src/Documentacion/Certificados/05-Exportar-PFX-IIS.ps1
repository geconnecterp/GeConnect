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
$CarpetaPKI = [IO.Path]::GetFullPath($CarpetaPKI)
$carpetaServidor = Join-Path (Join-Path $CarpetaPKI 'servidores') $NombreDNS
$claveServidor = Join-Path $carpetaServidor 'private\servidor.key.pem'
$certificado = Join-Path $carpetaServidor 'servidor.cert.pem'
$certificadoCA = Join-Path $CarpetaPKI 'certs\cafeamerica-ca.cert.pem'
$pfx = Join-Path $carpetaServidor 'servidor.iis.pfx'
$pfxPendiente = "$pfx.pending"
foreach ($entrada in @($OpenSSLPath, $claveServidor, $certificado, $certificadoCA)) {
    if (-not (Test-Path -LiteralPath $entrada -PathType Leaf)) { throw "Falta el archivo: $entrada" }
}
foreach ($salida in @($pfx, $pfxPendiente)) {
    if (Test-Path -LiteralPath $salida) { throw "El archivo ya existe. No se sobrescribira: $salida" }
}

& $OpenSSLPath verify -x509_strict -purpose sslserver -verify_hostname $NombreDNS -CAfile $certificadoCA $certificado
if ($LASTEXITCODE -ne 0) { throw 'El certificado no supera la verificacion TLS.' }
Write-Host 'OpenSSL pedira: 1) contrasena de la CLAVE DE ESTE SERVIDOR; 2) nueva contrasena de EXPORTACION PFX y su confirmacion.'
Write-Host 'Guarde la contrasena del PFX: se necesitara para importarlo en IIS.'
& $OpenSSLPath pkcs12 -export -out $pfxPendiente -inkey $claveServidor -in $certificado -certfile $certificadoCA -name $NombreDNS -keypbe AES-256-CBC -certpbe AES-256-CBC -macalg SHA256 -iter 100000
if ($LASTEXITCODE -ne 0) { throw 'Fallo la exportacion. No reemitir el certificado; conservar el error y revisar el archivo pending.' }

Write-Host 'Ingrese otra vez la contrasena de EXPORTACION PFX para comprobar su integridad.'
& $OpenSSLPath pkcs12 -in $pfxPendiente -info -noout
if ($LASTEXITCODE -ne 0) { throw 'Fallo la verificacion del PFX. Se conserva como pending; no importarlo todavia.' }
Move-Item -LiteralPath $pfxPendiente -Destination $pfx -ErrorAction Stop
Write-Host "PFX creado y verificado: $pfx"
Write-Host 'Contiene la clave privada de ESTE SERVIDOR, su certificado y el certificado PUBLICO de la CA.'
Write-Host 'No contiene la clave privada de la CA. No se instalo confianza ni se modifico IIS.'
