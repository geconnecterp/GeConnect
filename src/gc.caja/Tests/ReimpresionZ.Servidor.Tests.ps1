$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$testDir = Join-Path $root '.artifacts/reimpresion-z-tests'
New-Item -ItemType Directory -Force -Path $testDir | Out-Null
Set-Content -LiteralPath (Join-Path $testDir '.gitignore') -Value '*' -Encoding utf8
$project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net8.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App" /><ProjectReference Include="../../gc.api/gc.api.csproj" /><Compile Include="../../gc.caja/Tests/ReimpresionZ.Servidor.Tests.cs.txt" Link="Program.cs" /></ItemGroup></Project>'
Set-Content -LiteralPath (Join-Path $testDir 'ReimpresionZ.Tests.csproj') -Value $project -Encoding utf8
dotnet run --project (Join-Path $testDir 'ReimpresionZ.Tests.csproj') -p:UseAppHost=false --verbosity quiet -- (Join-Path $PSScriptRoot 'ReimpresionZ.Casos.json')
if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas de Reimpresión Z.' }
