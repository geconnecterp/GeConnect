$ErrorActionPreference = 'Stop'
$raiz = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$directorio = Join-Path $raiz '.artifacts/clover-log-tests'
New-Item -ItemType Directory -Force -Path $directorio | Out-Null
$proyecto = '<Project Sdk="Microsoft.NET.Sdk.Web"><PropertyGroup><TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><StaticWebAssetsEnabled>false</StaticWebAssetsEnabled></PropertyGroup><ItemGroup><ProjectReference Include="../../gc.caja.core/gc.caja.core.csproj" /><Compile Include="../../gc.caja/Tests/Clover.Log.Tests.cs.txt" Link="Program.cs" /></ItemGroup></Project>'
Set-Content -LiteralPath (Join-Path $directorio 'Clover.Log.csproj') -Value $proyecto -Encoding utf8
$salida = Join-Path $raiz '.artifacts/clover-log-build'
dotnet build (Join-Path $directorio 'Clover.Log.csproj') --artifacts-path $salida -p:UseAppHost=false --verbosity quiet -clp:ErrorsOnly --ignore-failed-sources
if ($LASTEXITCODE -ne 0) { throw 'Falló la compilación de diagnóstico Clover.' }
Push-Location $raiz
try { dotnet (Join-Path $salida 'bin/Clover.Log/debug/Clover.Log.dll'); if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas de diagnóstico Clover.' } }
finally { Pop-Location }
