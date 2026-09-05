$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
Push-Location $taskRoot
try {
    dotnet restore poc/harness/Harness.csproj --locked-mode --configfile eng/poc/NuGet.Offline.Config
    if ($LASTEXITCODE -ne 0) { throw 'RESTORE_FAILED' }
    dotnet build poc/harness/Harness.csproj --no-restore -c Release
    if ($LASTEXITCODE -ne 0) { throw 'BUILD_FAILED' }
    dotnet poc/harness/bin/Release/net10.0/Harness.dll self-test
    if ($LASTEXITCODE -ne 0) { throw 'TEST_FAILED' }
    dotnet poc/harness/bin/Release/net10.0/Harness.dll manifest
    if ($LASTEXITCODE -ne 0) { throw 'MANIFEST_FAILED' }
} finally { Pop-Location }
