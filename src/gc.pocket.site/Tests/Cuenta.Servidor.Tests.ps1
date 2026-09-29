$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$testDir = Join-Path $root '.artifacts/cuenta-pocket-tests'
New-Item -ItemType Directory -Force -Path $testDir | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Cuenta.Servidor.Tests.csproj.template') -Destination (Join-Path $testDir 'Cuenta.Tests.csproj')
dotnet build (Join-Path $testDir 'Cuenta.Tests.csproj') -p:UseAppHost=false -p:OutputPath=bin/cuenta-review/ --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw 'Falló la compilación de las pruebas de cuenta Pocket.' }
dotnet (Join-Path $testDir 'bin/cuenta-review/Cuenta.Tests.dll')
if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas de cuenta Pocket.' }
