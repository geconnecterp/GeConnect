#requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet('Chrome','Edge','Ambos')][string]$Navegador = 'Chrome',
    [ValidateSet('Habilitar','Restaurar','Estado')][string]$Accion = 'Estado',
    [string]$Respaldo = (Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'GECO\Caja\impresion-politicas.json')
)
$ErrorActionPreference = 'Stop'
$destinos = @()
if ($Navegador -in @('Chrome','Ambos')) { $destinos += 'HKCU:\Software\Policies\Google\Chrome' }
if ($Navegador -in @('Edge','Ambos')) { $destinos += 'HKCU:\Software\Policies\Microsoft\Edge' }
$nombres = @('SilentPrintingEnabled','PrintPreviewUseSystemDefaultPrinter')
$anterior = @()
if (Test-Path -LiteralPath $Respaldo) { $anterior = @(Get-Content -LiteralPath $Respaldo -Raw | ConvertFrom-Json) }
if ($Accion -eq 'Habilitar') {
    foreach ($ruta in $destinos) {
        foreach ($nombre in $nombres) {
            if (@($anterior | Where-Object { $_.ruta -eq $ruta -and $_.nombre -eq $nombre }).Count -eq 0) {
                $clave = Get-Item -LiteralPath $ruta -ErrorAction SilentlyContinue
                $existe = $null -ne $clave -and $clave.GetValueNames() -contains $nombre
                $anterior += [pscustomobject]@{ ruta=$ruta; nombre=$nombre; existia=$existe; valor= $(if($existe){$clave.GetValue($nombre)}else{$null}); tipo= $(if($existe){$clave.GetValueKind($nombre).ToString()}else{'DWord'}) }
            }
        }
    }
    $carpeta = Split-Path -Parent ([IO.Path]::GetFullPath($Respaldo))
    New-Item -ItemType Directory -Path $carpeta -Force | Out-Null
    ConvertTo-Json -InputObject @($anterior) -Depth 4 | Set-Content -LiteralPath $Respaldo -Encoding UTF8
    foreach ($ruta in $destinos) {
        New-Item -Path $ruta -Force | Out-Null
        foreach ($nombre in $nombres) { New-ItemProperty -LiteralPath $ruta -Name $nombre -Value 1 -PropertyType DWord -Force | Out-Null }
    }
    Write-Output "Politicas habilitadas. Respaldo: $Respaldo"
    Write-Output 'Chrome/Edge 144 o posterior. La impresion silenciosa afecta todas las impresiones de ese navegador.'
    Write-Output 'No cambia la impresora predeterminada de Windows. Una politica de equipo puede tener precedencia.'
    Write-Output 'Verifique también el papel en la vista previa del navegador: los comprobantes A/B y Custodia actuales usan A4. Windows en A4 no garantiza que Chrome tenga A4.'
} elseif ($Accion -eq 'Restaurar') {
    foreach ($dato in @($anterior | Where-Object { $destinos -contains $_.ruta -and $nombres -contains $_.nombre })) {
        $actual = Get-ItemPropertyValue -LiteralPath $dato.ruta -Name $dato.nombre -ErrorAction SilentlyContinue
        if ($null -ne $actual -and $actual -ne 1) { Write-Warning "Se conserva un cambio posterior: $($dato.ruta) / $($dato.nombre)"; continue }
        if ($dato.existia) {
            New-Item -Path $dato.ruta -Force | Out-Null
            New-ItemProperty -LiteralPath $dato.ruta -Name $dato.nombre -Value $dato.valor -PropertyType $dato.tipo -Force | Out-Null
        } else { Remove-ItemProperty -LiteralPath $dato.ruta -Name $dato.nombre -ErrorAction SilentlyContinue }
    }
    Write-Output 'Valores previos restaurados; se conserva el respaldo. No se eliminan otras politicas.'
}
foreach ($ruta in $destinos) {
    foreach ($nombre in $nombres) {
        [pscustomobject]@{ Ruta=$ruta; Politica=$nombre; Valor=(Get-ItemPropertyValue -LiteralPath $ruta -Name $nombre -ErrorAction SilentlyContinue) }
    }
}
