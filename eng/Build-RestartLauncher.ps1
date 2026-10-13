param([string]$Output = '')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
if (!$Output) { $Output = Join-Path $projectRoot ('artifacts/restart-launcher-' + [Guid]::NewGuid().ToString('N')) }
& dotnet build (Join-Path $PSScriptRoot 'LocalLauncher/LocalLauncher.csproj') --no-restore -c Debug -o $Output
if ($LASTEXITCODE -ne 0) { throw 'Launcher build failed.' }
$launcherRun = Start-Process -FilePath (Join-Path $Output 'PERTAMINA-OTO.exe') -ArgumentList '--write-restart-capability' -WindowStyle Hidden -PassThru -Wait
if ($launcherRun.ExitCode -ne 0) { throw 'Launcher capability generation failed.' }
Write-Output "Prepared launcher maintenance bundle: $Output"
