$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$dir = Join-Path $root '.artifacts/clover-integracion-tests'
New-Item -ItemType Directory -Force -Path $dir | Out-Null
$project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net8.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><ProjectReference Include="../../gc.api.core/gc.api.core.csproj" /><Compile Include="../../gc.caja/Tests/Clover.Integracion.Tests.cs.txt" Link="Program.cs" /></ItemGroup></Project>'
Set-Content -LiteralPath (Join-Path $dir 'Clover.Integracion.csproj') -Value $project -Encoding utf8
dotnet build (Join-Path $dir 'Clover.Integracion.csproj') --artifacts-path (Join-Path $root '.artifacts/clover-integracion') -p:UseAppHost=false --verbosity quiet -clp:ErrorsOnly --ignore-failed-sources
if ($LASTEXITCODE -ne 0) { throw 'Falló compilación de pruebas Clover.' }
dotnet (Join-Path $root '.artifacts/clover-integracion/bin/Clover.Integracion/debug/Clover.Integracion.dll')
if ($LASTEXITCODE -ne 0) { throw 'Fallaron verificaciones Clover.' }
