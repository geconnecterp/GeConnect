#requires -Version 5.1
# Ejecutar con Windows PowerShell 5.1. Solo GET; no cambia certificados ni IIS.
$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSEdition -ne 'Desktop') {
    throw 'Ejecutar este comprobador con Windows PowerShell 5.1 (powershell.exe).'
}
$uri = 'https://api-dev.cafeamerica.com.ar/gcservicios/api/administracion'
$peticion = [System.Net.HttpWebRequest]::Create($uri)
$peticion.Method = 'GET'
$peticion.AllowAutoRedirect = $false
$peticion.Timeout = 30000
$peticion.ReadWriteTimeout = 30000
$respuesta = $null
$lector = $null
$cert = $null
$sha = $null
try {
    $respuesta = $peticion.GetResponse()
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 -ArgumentList $peticion.ServicePoint.Certificate
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $huella256 = ([BitConverter]::ToString($sha.ComputeHash($cert.RawData))).Replace('-', '')
    if ($huella256 -ne 'CEA76F01B4590946FE818C0DAE2C73F9C739A445700705BE280138DC2B8F35A4') {
        Write-Warning "La huella recibida difiere de la emitida: $huella256"
    }
    $lector = New-Object System.IO.StreamReader -ArgumentList $respuesta.GetResponseStream()
    $contenido = $lector.ReadToEnd()
    $jsonValido = $false
    $estructura = $null
    try {
        $json = $contenido | ConvertFrom-Json -ErrorAction Stop
        $jsonValido = $true
        $estructura = @($json.PSObject.Properties.Name)
    } catch { }
    [pscustomobject]@{
        FechaUTC = [DateTime]::UtcNow.ToString('yyyy-MM-dd HH:mm:ss')
        StatusCode = [int]$respuesta.StatusCode
        ResponseUri = $respuesta.ResponseUri.AbsoluteUri
        ContentType = $respuesta.ContentType
        Subject = $cert.Subject
        Issuer = $cert.Issuer
        Thumbprint = $cert.Thumbprint
        SHA256 = $huella256
        CoincideCertificadoEmitido = ($huella256 -eq 'CEA76F01B4590946FE818C0DAE2C73F9C739A445700705BE280138DC2B8F35A4')
        NotAfterUTC = $cert.NotAfter.ToUniversalTime().ToString('yyyy-MM-dd HH:mm:ss')
        ContentLengthCharacters = $contenido.Length
        JsonValido = $jsonValido
        JsonProperties = $estructura
        CantidadRegistros = @($json.data).Count
        TotalCount = $json.meta.totalCount
    } | ConvertTo-Json -Depth 6
} finally {
    if ($lector) { $lector.Dispose() }
    if ($respuesta) { $respuesta.Dispose() }
    if ($sha) { $sha.Dispose() }
    if ($cert) { $cert.Dispose() }
}