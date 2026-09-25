#requires -Version 5.1
[CmdletBinding()]
param(
    [string]$CarpetaDestino = (Join-Path $env:USERPROFILE 'GecoPKI-Respaldos'),
    [string]$CarpetaPKI = (Join-Path $env:USERPROFILE 'GecoPKI'),
    [string]$CarpetaDocumentacion,
    [string]$RarPath = 'C:\Program Files\WinRAR\Rar.exe'
)
$ErrorActionPreference = 'Stop'
# Resolver la ubicacion despues de enlazar parametros: PSScriptRoot puede estar vacio en valores predeterminados.
if ([string]::IsNullOrWhiteSpace($CarpetaDocumentacion)) {
    $rutaComponente = $PSCommandPath
    if ([string]::IsNullOrWhiteSpace($rutaComponente)) { $rutaComponente = $MyInvocation.MyCommand.Path }
    if ([string]::IsNullOrWhiteSpace($rutaComponente)) { throw 'No se pudo determinar la ruta del script. Indicar -CarpetaDocumentacion.' }
    $CarpetaDocumentacion = [IO.Path]::GetDirectoryName([IO.Path]::GetDirectoryName($rutaComponente))
}
if ($PSVersionTable.PSEdition -ne 'Desktop') { throw 'Ejecutar con Windows PowerShell 5.1 (powershell.exe).' }
Import-Module (Join-Path $PSHOME 'Modules\Microsoft.PowerShell.Utility\Microsoft.PowerShell.Utility.psd1') -ErrorAction Stop
function RutaCompleta([string]$Ruta) { [IO.Path]::GetFullPath($Ruta).TrimEnd('\') }
function Dentro([string]$Ruta, [string]$Base) {
    $Ruta.Equals($Base, [StringComparison]::OrdinalIgnoreCase) -or $Ruta.StartsWith($Base + '\', [StringComparison]::OrdinalIgnoreCase)
}
function SinEnlaces([string]$Ruta) {
    $actual = $Ruta
    while ($actual) {
        if (Test-Path -LiteralPath $actual) {
            if ((Get-Item -LiteralPath $actual -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) {
                throw "La ruta contiene un enlace o punto de reanalisis: $actual"
            }
        }
        $padre = Split-Path $actual -Parent
        if ($padre -eq $actual) { break }
        $actual = $padre
    }
}
function CarpetaPrivada([string]$Ruta) {
    $acl = [Security.AccessControl.DirectorySecurity]::new()
    $acl.SetAccessRuleProtection($true, $false)
    $identidades = @([Security.Principal.WindowsIdentity]::GetCurrent().User.Value, 'S-1-5-18', 'S-1-5-32-544') | Select-Object -Unique
    foreach ($sid in $identidades) {
        $regla = [Security.AccessControl.FileSystemAccessRule]::new(
            [Security.Principal.SecurityIdentifier]::new($sid),
            [Security.AccessControl.FileSystemRights]::FullControl,
            [Security.AccessControl.InheritanceFlags]'ContainerInherit, ObjectInherit',
            [Security.AccessControl.PropagationFlags]::None,
            [Security.AccessControl.AccessControlType]::Allow)
        [void]$acl.AddAccessRule($regla)
    }
    if (Test-Path -LiteralPath $Ruta) { throw "La carpeta temporal ya existe: $Ruta" }
    [void][IO.Directory]::CreateDirectory($Ruta, $acl)
}
function EnteroRAR($Lector) {
    [long]$valor = 0
    for ($desplazamiento = 0; $desplazamiento -lt 63; $desplazamiento += 7) {
        $b = $Lector.ReadByte()
        $valor = $valor -bor ([long]($b -band 127) -shl $desplazamiento)
        if (($b -band 128) -eq 0) { return $valor }
    }
    throw 'Cabecera RAR invalida.'
}
$CarpetaPKI = RutaCompleta $CarpetaPKI
$CarpetaDocumentacion = RutaCompleta $CarpetaDocumentacion
$CarpetaDestino = RutaCompleta $CarpetaDestino
foreach ($ruta in @($CarpetaPKI, $CarpetaDocumentacion, $CarpetaDestino)) { SinEnlaces $ruta }
if ((Dentro $CarpetaDestino $CarpetaPKI) -or (Dentro $CarpetaDestino (Split-Path $CarpetaDocumentacion -Parent))) {
    throw 'El destino debe estar fuera de la PKI y de la carpeta del proyecto.'
}
foreach ($ruta in @($CarpetaPKI, $CarpetaDocumentacion)) {
    if (-not (Test-Path -LiteralPath $ruta -PathType Container)) { throw "Falta la carpeta: $ruta" }
}
if (-not (Test-Path -LiteralPath $RarPath -PathType Leaf)) { throw 'No se encontro Rar.exe.' }
$obligatorios = @('private\cafeamerica-ca.key.pem', 'certs\cafeamerica-ca.cer', 'certs\cafeamerica-ca.cert.pem',
    'ca-db\index.txt', 'ca-db\index.txt.attr', 'ca-db\serial', 'ca-db\openssl-ca.cnf',
    'servidores\api-dev.cafeamerica.com.ar\private\servidor.key.pem',
    'servidores\api-dev.cafeamerica.com.ar\servidor.cert.pem',
    'servidores\api-dev.cafeamerica.com.ar\servidor.iis.pfx')
foreach ($relativa in $obligatorios) {
    if (-not (Test-Path -LiteralPath (Join-Path $CarpetaPKI $relativa) -PathType Leaf)) { throw "Falta material requerido: $relativa" }
}
if (-not @(Get-ChildItem -LiteralPath (Join-Path $CarpetaPKI 'ca-db\newcerts') -File -Force).Count) { throw 'La base no tiene certificados emitidos.' }
if (-not (Test-Path -LiteralPath (Join-Path $CarpetaDocumentacion 'SEG-006_Bitacora_CA_y_HTTPS_IIS.md'))) { throw 'Falta la bitacora.' }
$identificador = (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0,8)
$temporalBase = RutaCompleta ([IO.Path]::GetTempPath())
SinEnlaces $temporalBase
$trabajo = Join-Path $temporalBase ('GECO-Backup-' + $identificador)
$carga = Join-Path $trabajo 'contenido'
$extraido = Join-Path $trabajo 'verificacion'
$archivoFinal = Join-Path $CarpetaDestino ('GecoPKI-' + $identificador + '.rar')
$pendiente = Join-Path $CarpetaDestino ('GecoPKI-' + $identificador + '.pendiente.rar')
$bloqueo = $null
try {
    CarpetaPrivada $trabajo
    [void][IO.Directory]::CreateDirectory($carga)
    [void][IO.Directory]::CreateDirectory($extraido)
    [void][IO.Directory]::CreateDirectory($CarpetaDestino)
    # Comparte el bloqueo del emisor 04. No ejecutar otros cambios PKI durante esta copia.
    $bloqueo = [IO.File]::Open((Join-Path $CarpetaPKI 'ca-emision.lock'), [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    $entradasPKI = @(Get-ChildItem -LiteralPath $CarpetaPKI -Recurse -Force)
    if (@($entradasPKI | Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint }).Count) { throw 'La PKI contiene enlaces. Revisar antes de copiar.' }
    $archivosPKI = @($entradasPKI | Where-Object { -not $_.PSIsContainer -and $_.FullName -ne (Join-Path $CarpetaPKI 'ca-emision.lock') })
    foreach ($clave in @($archivosPKI | Where-Object { $_.Name -like '*.key.pem' })) {
        $lector = [IO.File]::OpenText($clave.FullName)
        try { $cabecera = $lector.ReadLine() } finally { $lector.Dispose() }
        if ($cabecera -ne '-----BEGIN ENCRYPTED PRIVATE KEY-----') { throw "Clave PEM sin el cifrado esperado: $($clave.Name)" }
    }
    $componentes = Join-Path $CarpetaDocumentacion 'Certificados'
    SinEnlaces $componentes
    $archivosDoc = @((Get-Item -LiteralPath (Join-Path $CarpetaDocumentacion 'SEG-006_Bitacora_CA_y_HTTPS_IIS.md')))
    $archivosDoc += @(Get-ChildItem -LiteralPath $componentes -File | Where-Object { $_.Extension -in @('.ps1', '.cnf') })
    $plan = @()
    foreach ($f in $archivosPKI) { $plan += [pscustomobject]@{ Origen=$f.FullName; Ruta=('GecoPKI\' + $f.FullName.Substring($CarpetaPKI.Length + 1)) } }
    foreach ($f in $archivosDoc) {
        SinEnlaces $f.FullName
        $plan += [pscustomobject]@{ Origen=$f.FullName; Ruta=('Documentacion\' + $f.FullName.Substring($CarpetaDocumentacion.Length + 1)) }
    }
    $manifest = @()
    foreach ($f in $plan) {
        $hash = (Get-FileHash -LiteralPath $f.Origen -Algorithm SHA256).Hash
        $copia = Join-Path $carga $f.Ruta
        [void][IO.Directory]::CreateDirectory((Split-Path $copia -Parent))
        [IO.File]::Copy($f.Origen, $copia, $false)
        if ((Get-FileHash -LiteralPath $copia -Algorithm SHA256).Hash -ne $hash) { throw "Copia distinta: $($f.Ruta)" }
        $manifest += [pscustomobject]@{ Ruta=$f.Ruta; SHA256=$hash; Bytes=(Get-Item -LiteralPath $copia).Length }
    }
    foreach ($f in $plan) {
        $entrada = $manifest | Where-Object { $_.Ruta -eq $f.Ruta }
        if ((Get-FileHash -LiteralPath $f.Origen -Algorithm SHA256).Hash -ne $entrada.SHA256) { throw "El origen cambio durante la copia: $($f.Ruta)" }
    }
    $bloqueo.Dispose(); $bloqueo = $null
    $manifiestoRuta = Join-Path $carga 'MANIFIESTO-SHA256.json'
    $manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $manifiestoRuta -Encoding UTF8
    $hashManifiesto = (Get-FileHash -LiteralPath $manifiestoRuta -Algorithm SHA256).Hash
    Write-Host "Preparados $($manifest.Count) archivos. WinRAR pedira una NUEVA contrasena del respaldo y su confirmacion."
    Write-Host 'Guardar esta contrasena por separado. No escribirla en el chat ni en la linea de comandos.'
    Push-Location $carga
    try {
        & $RarPath a -cfg- -hp -r -idq -m3 $pendiente '*'
        if ($LASTEXITCODE -ne 0) { throw "WinRAR no completo el archivo. Codigo: $LASTEXITCODE" }
    } finally { Pop-Location }
    $lector = [IO.BinaryReader]::new([IO.File]::OpenRead($pendiente))
    try {
        if ([BitConverter]::ToString($lector.ReadBytes(8)) -ne '52-61-72-21-1A-07-01-00') { throw 'El archivo no tiene formato RAR5.' }
        [void]$lector.ReadUInt32()
        [void](EnteroRAR $lector)
        if ((EnteroRAR $lector) -ne 4) { throw 'El archivo no tiene cabeceras cifradas. No usarlo como respaldo protegido.' }
    } finally { $lector.Dispose() }
    Write-Host 'Ingrese otra vez la contrasena del RESPALDO para extraer una copia de prueba.'
    & $RarPath x -cfg- -p -idq -o- $pendiente ($extraido + '\')
    if ($LASTEXITCODE -ne 0) { throw "No se pudo extraer y verificar. Codigo: $LASTEXITCODE" }
    if ((Get-FileHash -LiteralPath (Join-Path $extraido 'MANIFIESTO-SHA256.json') -Algorithm SHA256).Hash -ne $hashManifiesto) { throw 'El manifiesto extraido difiere del original.' }
    foreach ($f in $manifest) {
        if ((Get-FileHash -LiteralPath (Join-Path $extraido $f.Ruta) -Algorithm SHA256).Hash -ne $f.SHA256) { throw "Restauracion distinta: $($f.Ruta)" }
    }
    if (@(Get-ChildItem -LiteralPath $extraido -Recurse -File -Force).Count -ne ($manifest.Count + 1)) { throw 'El numero de archivos extraidos no coincide.' }
    [IO.File]::Move($pendiente, $archivoFinal)
    $hashFinal = (Get-FileHash -LiteralPath $archivoFinal -Algorithm SHA256).Hash
    $verificacion = [pscustomobject]@{
        Archivo=$archivoFinal; SHA256=$hashFinal; ArchivosVerificados=$manifest.Count
        Formato='RAR5 AES-256, datos y nombres cifrados'; ExtraccionVerificada=$true
        FechaUTC=[DateTime]::UtcNow.ToString('yyyy-MM-dd HH:mm:ss')
    }
    $verificacion | ConvertTo-Json | Set-Content -LiteralPath ($archivoFinal + '.verificacion.json') -Encoding UTF8
    $verificacion | Format-List
    Write-Host 'Respaldo cifrado creado y extraido para verificacion. Todos los archivos coinciden con el origen.'
} finally {
    if ($bloqueo) { $bloqueo.Dispose() }
    # Eliminar solo el directorio temporal unico de esta ejecucion, nunca originales ni archivos RAR.
    if (Test-Path -LiteralPath $trabajo) {
        $resuelto = RutaCompleta ((Resolve-Path -LiteralPath $trabajo).ProviderPath)
        if ($resuelto -ne (RutaCompleta $trabajo) -or -not (Dentro $resuelto $temporalBase) -or (Split-Path $resuelto -Leaf) -ne ('GECO-Backup-' + $identificador)) { throw 'La ruta temporal no coincide. No se elimina.' }
        SinEnlaces $resuelto
        if (@(Get-ChildItem -LiteralPath $resuelto -Recurse -Force | Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint }).Count) { throw 'Hay enlaces dentro del temporal; no se elimina automaticamente.' }
        Remove-Item -LiteralPath $resuelto -Recurse -Force
    }
}