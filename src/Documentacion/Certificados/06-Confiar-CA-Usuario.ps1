[CmdletBinding(SupportsShouldProcess)]
param(
    [string]$ArchivoRaiz = (Join-Path $env:USERPROFILE 'GecoPKI\certs\cafeamerica-ca.cer'),
    [ValidatePattern('\A[0-9a-fA-F]{64}\z')]
    [string]$HuellaEsperada = '56DDC740DDBEC1ED58552447D2E7F85B54EB6EC06B7A9E2B27785CE93A86B87A'
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $ArchivoRaiz -PathType Leaf)) { throw "No se encuentra el certificado publico: $ArchivoRaiz" }
$huellaArchivo = (Get-FileHash -LiteralPath $ArchivoRaiz -Algorithm SHA256).Hash
if ($huellaArchivo -ne $HuellaEsperada) { throw 'La huella del archivo DER no coincide con la CA registrada. No se instalara confianza.' }

$raiz = New-Object Security.Cryptography.X509Certificates.X509Certificate2 -ArgumentList $ArchivoRaiz
try {
    $ahora = [DateTime]::UtcNow
    if ($ahora -lt $raiz.NotBefore.ToUniversalTime() -or $ahora -gt $raiz.NotAfter.ToUniversalTime()) { throw 'La raiz no esta vigente.' }
    if ($raiz.HasPrivateKey) { throw 'Se esperaba solamente el certificado publico.' }
    $rutaAlmacen = 'Cert:\CurrentUser\Root'
    $rutaCertificado = Join-Path $rutaAlmacen $raiz.Thumbprint
    Write-Host "Autoridad verificada: $($raiz.Subject)"
    Write-Host "Huella SHA-256: $huellaArchivo"
    Write-Host "Destino: $rutaAlmacen (solo el usuario actual de Windows)."

    if (-not (Test-Path -LiteralPath $rutaCertificado)) {
        if (-not $PSCmdlet.ShouldProcess($rutaCertificado, 'Instalar la CA como raiz de confianza del usuario actual')) { return }
        Import-Certificate -FilePath $ArchivoRaiz -CertStoreLocation $rutaAlmacen -ErrorAction Stop | Out-Null
    } else {
        Write-Host 'La raiz ya estaba instalada para este usuario; se comprobara su identidad.'
    }

    $instalada = Get-Item -LiteralPath $rutaCertificado -ErrorAction Stop
    try {
        $sha256 = [Security.Cryptography.SHA256]::Create()
        try { $huellaInstalada = [BitConverter]::ToString($sha256.ComputeHash($instalada.RawData)).Replace('-', '') }
        finally { $sha256.Dispose() }
        if ($huellaInstalada -ne $HuellaEsperada) { throw 'La huella en el almacen no coincide con la autoridad esperada.' }
        Write-Host "Vencimiento UTC: $($instalada.NotAfter.ToUniversalTime().ToString('yyyy-MM-dd HH:mm:ss'))"
        Write-Host 'Confianza de la CA instalada y verificada para el usuario actual.'
        Write-Host 'El servidor IIS y las cuentas de servicio se configuraran por separado.'
    } finally { $instalada.Dispose() }
} finally { $raiz.Dispose() }
