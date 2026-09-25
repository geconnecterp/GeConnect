#requires -Version 5.1
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $true)]
    [ValidateLength(1, 253)]
    [ValidatePattern('\A(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\z')]
    [string]$NombreDNS,
    [ValidateRange(1, 200)]
    [int]$Anios = 200,
    [string]$CarpetaPKI = (Join-Path $env:USERPROFILE 'GecoPKI'),
    [string]$OpenSSLPath = 'C:\Program Files\OpenSSL-Win64\bin\openssl.exe',
    [ValidatePattern('\A[0-9A-Fa-f]{64}\z')]
    [string]$HuellaCA = '56DDC740DDBEC1ED58552447D2E7F85B54EB6EC06B7A9E2B27785CE93A86B87A'
)
$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSEdition -ne 'Desktop') { throw 'Ejecutar con Windows PowerShell 5.1 (powershell.exe).' }
Import-Module (Join-Path $PSHOME 'Modules\Microsoft.PowerShell.Utility\Microsoft.PowerShell.Utility.psd1') -ErrorAction Stop
function HashArchivo([string]$Ruta) {
    $flujo = [IO.File]::OpenRead($Ruta)
    $sha = [Security.Cryptography.SHA256]::Create()
    try { ([BitConverter]::ToString($sha.ComputeHash($flujo))).Replace('-', '') }
    finally { $sha.Dispose(); $flujo.Dispose() }
}
$NombreDNS = $NombreDNS.ToLowerInvariant()
$CarpetaPKI = [IO.Path]::GetFullPath($CarpetaPKI)
$rutaComponente = $PSCommandPath
if ([string]::IsNullOrWhiteSpace($rutaComponente)) { $rutaComponente = $MyInvocation.MyCommand.Path }
if ([string]::IsNullOrWhiteSpace($rutaComponente)) { throw 'No se pudo determinar la ubicacion del componente.' }
$componentes = [IO.Path]::GetDirectoryName($rutaComponente)
$solicitud = Join-Path $componentes '03-Crear-Solicitud-Servidor.ps1'
$emision = Join-Path $componentes '04-Emitir-Certificado-Servidor.ps1'
$exportacion = Join-Path $componentes '05-Exportar-PFX-IIS.ps1'
$raizDer = Join-Path $CarpetaPKI 'certs\cafeamerica-ca.cer'
$raizPem = Join-Path $CarpetaPKI 'certs\cafeamerica-ca.cert.pem'
$claveCA = Join-Path $CarpetaPKI 'private\cafeamerica-ca.key.pem'
$carpetaServidor = Join-Path (Join-Path $CarpetaPKI 'servidores') $NombreDNS
foreach ($entrada in @($OpenSSLPath, $solicitud, $emision, $exportacion, $raizDer, $raizPem, $claveCA)) {
    if (-not (Test-Path -LiteralPath $entrada -PathType Leaf)) { throw "Falta el archivo requerido: $entrada" }
}
foreach ($nombre in @('index.txt', 'index.txt.attr', 'serial', 'openssl-ca.cnf')) {
    if (-not (Test-Path -LiteralPath (Join-Path $CarpetaPKI ('ca-db\' + $nombre)) -PathType Leaf)) {
        throw 'La base de la CA existente esta incompleta. Recuperarla; no crear otra CA ni reinicializarla.'
    }
}
if (-not (Test-Path -LiteralPath (Join-Path $CarpetaPKI 'ca-db\newcerts') -PathType Container)) { throw 'Falta ca-db\newcerts.' }
if (Test-Path -LiteralPath $carpetaServidor) {
    throw "Ya existe material para $NombreDNS en $carpetaServidor. No se sobrescribe ni se reemite; revisar el paso pendiente."
}
if ((HashArchivo $raizDer) -ne $HuellaCA) { throw 'El certificado DER de la CA no coincide con la autoridad esperada.' }
$huellaPem = & $OpenSSLPath x509 -in $raizPem -noout -fingerprint -sha256
if ($LASTEXITCODE -ne 0) { throw 'No se pudo leer el certificado PEM de la CA.' }
$huellaPemNormalizada = (($huellaPem -join '').Split('=')[-1] -replace ':', '').Trim()
if ($huellaPemNormalizada -ne $HuellaCA) { throw 'El certificado PEM y la CA esperada no coinciden.' }
& $OpenSSLPath verify -x509_strict -check_ss_sig -CAfile $raizPem $raizPem
if ($LASTEXITCODE -ne 0) { throw 'La CA no supera la verificacion de firma o vigencia.' }
if (-not $PSCmdlet.ShouldProcess($NombreDNS, 'Crear clave y CSR, emitir con la CA existente y exportar PFX protegido')) { return }
$argumentos = @{ NombreDNS=$NombreDNS; CarpetaPKI=$CarpetaPKI; OpenSSLPath=$OpenSSLPath }
Write-Host "1/3 - Clave privada propia y CSR para $NombreDNS."
& $solicitud @argumentos
Write-Host "2/3 - Emision con la CA existente. Vigencia solicitada: $Anios anios, limitada por el vencimiento de la CA."
& $emision @argumentos -Anios $Anios
Write-Host "3/3 - Exportacion PFX para $NombreDNS. La clave solicitada es la de ESTE servidor."
& $exportacion @argumentos
$pfx = Join-Path $carpetaServidor 'servidor.iis.pfx'
if (-not (Test-Path -LiteralPath $pfx -PathType Leaf)) { throw 'La exportacion no produjo el archivo PFX esperado.' }
$certificado = Join-Path $carpetaServidor 'servidor.cert.pem'
[pscustomobject]@{
    NombreDNS = $NombreDNS
    PFX = $pfx
    SHA256PFX = (HashArchivo $pfx)
    CertificadoServidor = $certificado
    CertificadoPublicoCA = $raizDer
} | Format-List
Write-Host 'PFX nuevo creado y verificado. Copiar al IIS destino el PFX y, si hace falta, la CA publica.'
Write-Host 'La clave privada de la CA permanece en la computadora de administracion. No se modifico IIS.'
Write-Host 'Actualizar el respaldo de GecoPKI: esta emision modifico la base de la CA.'