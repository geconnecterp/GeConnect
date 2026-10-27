$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$dir = Join-Path $root '.artifacts/clover-dependencias-tests'
New-Item -ItemType Directory -Force -Path $dir | Out-Null
$project = '<Project Sdk="Microsoft.NET.Sdk.Web"><PropertyGroup><TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><StaticWebAssetsEnabled>false</StaticWebAssetsEnabled></PropertyGroup><ItemGroup><ProjectReference Include="../../gc.api.infra/gc.api.infra.csproj" /><Compile Include="../../gc.caja/Tests/Clover.Dependencias.Tests.cs.txt" Link="Program.cs" /></ItemGroup></Project>'
Set-Content -LiteralPath (Join-Path $dir 'Clover.Dependencias.csproj') -Value $project -Encoding utf8
$salida = Join-Path $root '.artifacts/clover-di'
dotnet build (Join-Path $dir 'Clover.Dependencias.csproj') --artifacts-path $salida -p:UseAppHost=false --verbosity quiet -clp:ErrorsOnly --ignore-failed-sources
if ($LASTEXITCODE -ne 0) { throw 'Falló la compilación de las pruebas de dependencias Clover.' }
dotnet (Join-Path $salida 'bin/Clover.Dependencias/debug/Clover.Dependencias.dll')
if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas del registro compartido de Clover.' }
