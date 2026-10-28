$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$testDir = Join-Path $root '.artifacts/caja-signos-tests'
New-Item -ItemType Directory -Force -Path $testDir | Out-Null
$project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net8.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><ProjectReference Include="../../gc.api.core/gc.api.core.csproj" /><Compile Include="../../gc.api.core/Tests/CajaSignos.Integracion.Tests.cs.txt" Link="Program.cs" /></ItemGroup></Project>'
Set-Content -LiteralPath (Join-Path $testDir 'CajaSignos.Tests.csproj') -Value $project -Encoding utf8
$artifacts = Join-Path $testDir 'build'
dotnet build (Join-Path $testDir 'CajaSignos.Tests.csproj') --artifacts-path $artifacts -p:UseAppHost=false --verbosity quiet -clp:ErrorsOnly --ignore-failed-sources
if ($LASTEXITCODE -ne 0) { throw 'Falló la compilación de pruebas de signos.' }
$assemblyDir = Join-Path $artifacts 'bin/CajaSignos.Tests/debug'
dotnet (Join-Path $assemblyDir 'CajaSignos.Tests.dll') (Join-Path $testDir 'evidencia')
if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas de cálculo/confirmación con signo.' }
& (Join-Path $PSScriptRoot 'CajaConfirmacionJson.Tests.ps1') -AssemblyPath (Join-Path $assemblyDir 'gc.api.core.dll')
