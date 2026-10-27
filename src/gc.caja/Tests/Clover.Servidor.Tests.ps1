param([switch]$Serve, [string]$OpcionesJson)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$testDir = Join-Path $root '.artifacts/clover-tests'
New-Item -ItemType Directory -Force -Path $testDir | Out-Null
Set-Content -LiteralPath (Join-Path $testDir '.gitignore') -Value '*' -Encoding utf8
$project = '<Project Sdk="Microsoft.NET.Sdk.Web"><PropertyGroup><TargetFramework>net8.0</TargetFramework><StaticWebAssetsEnabled>false</StaticWebAssetsEnabled><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><ProjectReference Include="../../gc.caja/gc.caja.csproj" /><ProjectReference Include="../../gc.api/gc.api.csproj" /><Compile Include="../../gc.caja/Tests/Clover.Servidor.Tests.cs.txt" Link="Program.cs" /></ItemGroup></Project>'
Set-Content -LiteralPath (Join-Path $testDir 'Clover.Tests.csproj') -Value $project -Encoding utf8
dotnet build (Join-Path $testDir 'Clover.Tests.csproj') --artifacts-path (Join-Path $root '.artifacts/clover-verificacion') -p:UseAppHost=false --verbosity quiet -clp:ErrorsOnly --ignore-failed-sources
if ($LASTEXITCODE -ne 0) { throw 'No se pudo compilar la prueba Clover.' }
$argumentos = if ($Serve) { @('--serve') } else { @() }
if ($OpcionesJson) {
    if (-not $Serve) { throw 'OpcionesJson requiere Serve; no reemplaza el catálogo de las regresiones.' }
    $argumentos += @('--opciones-json', (Resolve-Path -LiteralPath $OpcionesJson).Path)
}
Push-Location $root
try { dotnet (Join-Path $root '.artifacts/clover-verificacion/bin/Clover.Tests/debug/Clover.Tests.dll') @argumentos; if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas Clover.' } }
finally { Pop-Location }
