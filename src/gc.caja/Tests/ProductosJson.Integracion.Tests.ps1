$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$testDir = Join-Path $root '.artifacts/caja-productos-json'
New-Item -ItemType Directory -Force -Path $testDir | Out-Null
$project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net8.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><ProjectReference Include="../../gc.caja/gc.caja.csproj" /><ProjectReference Include="../../gc.api.core/gc.api.core.csproj" /><Compile Include="../../gc.caja/Tests/ProductosJson.Integracion.Tests.cs.txt" Link="Program.cs" /></ItemGroup></Project>'
Set-Content -LiteralPath (Join-Path $testDir 'ProductosJson.Tests.csproj') -Value $project -Encoding utf8
$artifacts = Join-Path $testDir 'build'
dotnet build (Join-Path $testDir 'ProductosJson.Tests.csproj') --artifacts-path $artifacts -p:UseAppHost=false --verbosity quiet -clp:ErrorsOnly --ignore-failed-sources
if ($LASTEXITCODE -ne 0) { throw 'Falló la compilación de regresiones del JSON original.' }
dotnet (Join-Path $artifacts 'bin/ProductosJson.Tests/debug/ProductosJson.Tests.dll') $root
if ($LASTEXITCODE -ne 0) { throw 'Fallaron las regresiones del JSON original.' }
