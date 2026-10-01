$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$testDir = Join-Path $root '.artifacts/iniciador-protocolo-tests'
New-Item -ItemType Directory -Force -Path $testDir | Out-Null
Set-Content -LiteralPath (Join-Path $testDir '.gitignore') -Value '*'
$project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net8.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><Compile Include="../../gc.caja.iniciador/RespuestaInicio.cs" Link="RespuestaInicio.cs" /><Compile Include="../../gc.caja.iniciador/Tests/RespuestaInicio.Tests.cs.txt" Link="Program.cs" /></ItemGroup></Project>'
Set-Content -LiteralPath (Join-Path $testDir 'Protocolo.Tests.csproj') -Value $project -Encoding utf8
dotnet run --project (Join-Path $testDir 'Protocolo.Tests.csproj') --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas del protocolo de inicio.' }
