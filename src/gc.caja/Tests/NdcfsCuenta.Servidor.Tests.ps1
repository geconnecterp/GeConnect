$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$testDir = Join-Path $root '.artifacts/ndcfs-cuentas-tests'
New-Item -ItemType Directory -Force -Path $testDir | Out-Null
Set-Content -LiteralPath (Join-Path $testDir '.gitignore') -Value '*' -Encoding utf8
$project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net8.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><ProjectReference Include="../../gc.caja/gc.caja.csproj" /><Compile Include="../../gc.caja/Tests/NdcfsCuenta.Servidor.Tests.cs.txt" Link="Program.cs" /></ItemGroup></Project>'
Set-Content -LiteralPath (Join-Path $testDir 'NdcfsCuentas.Tests.csproj') -Value $project -Encoding utf8
# Salidas aisladas: no se reemplazan los binarios que utiliza Visual Studio.
$artifacts = Join-Path $testDir 'build'
dotnet build (Join-Path $testDir 'NdcfsCuentas.Tests.csproj') --artifacts-path $artifacts -p:UseAppHost=false --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw 'Falló la compilación de Caja/pruebas de cuentas.' }
dotnet (Join-Path $artifacts 'bin/NdcfsCuentas.Tests/debug/NdcfsCuentas.Tests.dll')
if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas de validación de cuentas.' }
