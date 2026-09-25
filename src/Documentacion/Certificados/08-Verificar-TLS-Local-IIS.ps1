#requires -Version 5.1
# Ejecutar dentro del servidor IIS. Solo abre una conexion TLS local.
& {
    $ErrorActionPreference = 'Stop'
    $nombre = 'api-dev.cafeamerica.com.ar'
    $esperada = 'CEA76F01B4590946FE818C0DAE2C73F9C739A445700705BE280138DC2B8F35A4'
    $tcp = [System.Net.Sockets.TcpClient]::new()
    $tls = $null
    $cert = $null
    $sha = $null
    try {
        $conexion = $tcp.ConnectAsync('127.0.0.1', 443)
        if (-not $conexion.Wait(10000)) { throw 'No se pudo conectar al puerto 443 local en 10 segundos.' }
        [void]$conexion.GetAwaiter().GetResult()

        $tls = [System.Net.Security.SslStream]::new($tcp.GetStream(), $false)
        $autenticacion = $tls.AuthenticateAsClientAsync(
            $nombre, $null, [System.Security.Authentication.SslProtocols]::None, $false)
        if (-not $autenticacion.Wait(15000)) { throw 'La comprobacion TLS excedio 15 segundos.' }
        [void]$autenticacion.GetAwaiter().GetResult()

        $cert = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($tls.RemoteCertificate)
        $sha = [System.Security.Cryptography.SHA256]::Create()
        $huella = ([BitConverter]::ToString($sha.ComputeHash($cert.RawData))).Replace('-', '')
        $coincide = ($huella -eq $esperada)
        [pscustomobject]@{
            NombreVerificado = $nombre
            Subject = $cert.Subject
            Issuer = $cert.Issuer
            VencimientoUTC = $cert.NotAfter.ToUniversalTime().ToString('yyyy-MM-dd HH:mm:ss')
            SHA256 = $huella
            CoincideCertificadoEmitido = $coincide
            ProtocoloTLS = $tls.SslProtocol
            ConexionCifrada = $tls.IsEncrypted
            IntegridadProtegida = $tls.IsSigned
        } | Format-List

        if (-not $coincide) { throw 'El certificado recibido no coincide con el emitido. Conservar la salida para revisar.' }
        if (-not ($tls.IsEncrypted -and $tls.IsSigned)) { throw 'La conexion no tiene la proteccion TLS esperada.' }
        Write-Host 'Certificado original de IIS verificado: nombre, confianza y huella correctos.'
    }
    finally {
        if ($sha) { $sha.Dispose() }
        if ($cert) { $cert.Dispose() }
        if ($tls) { $tls.Dispose() }
        $tcp.Dispose()
    }
}