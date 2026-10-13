# One-time legacy maintenance; new app packages also deliver a manifest-bound launcher.
param([Parameter(Mandatory=$true)][string]$LauncherOutput,
      [string]$InstallRoot = (Join-Path $env:LOCALAPPDATA 'Programs/PertaminaOTO'))
$ErrorActionPreference = 'Stop'
$sourceRoot = (Resolve-Path -LiteralPath $LauncherOutput).Path
$targetRoot = (Resolve-Path -LiteralPath $InstallRoot).Path
if ($sourceRoot -eq $targetRoot) { throw 'Maintenance source must be a separate verified build.' }
if (!(Test-Path -LiteralPath (Join-Path $targetRoot 'current.json')) -or !(Test-Path -LiteralPath (Join-Path $targetRoot 'PERTAMINA-OTO.exe'))) { throw 'Existing stable installation required.' }
if ((Get-Item -LiteralPath $targetRoot).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Installation links are not supported.' }
$capability = Get-Content -LiteralPath (Join-Path $sourceRoot 'restart-capability.json') -Raw | ConvertFrom-Json
$expectedFiles = @('PERTAMINA-OTO.exe','PERTAMINA-OTO.dll','PERTAMINA-OTO.deps.json','PERTAMINA-OTO.runtimeconfig.json')
if ($capability.Protocol -ne 1 -or $capability.Files.Count -ne 4 -or ((@($capability.Files.Path | Sort-Object) -join '|') -ne (($expectedFiles | Sort-Object) -join '|'))) { throw 'Invalid launcher capability bundle.' }
if ($capability.MaintenanceProtocol -ne 1) { throw 'Rebuild the launcher maintenance bundle with the current verified helper.' }
foreach ($entry in $capability.Files) {
    $sourceFile = Join-Path $sourceRoot $entry.Path
    if ((Get-Item -LiteralPath $sourceFile).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Source links rejected.' }
    if ((Get-Item -LiteralPath $sourceFile).Length -ne $entry.Length -or (Get-FileHash -LiteralPath $sourceFile).Hash -ne $entry.Sha256) { throw 'Launcher maintenance hash mismatch.' }
}
$pointerBefore = (Get-FileHash -LiteralPath (Join-Path $targetRoot 'current.json')).Hash
$helper = Start-Process -FilePath (Join-Path $sourceRoot 'PERTAMINA-OTO.exe') -ArgumentList @('--maintain-launcher', ('"' + $targetRoot + '"')) -WindowStyle Hidden -PassThru -Wait
if ($helper.ExitCode -ne 0) { throw 'Launcher maintenance failed; retained backups are available. No app was forcibly closed.' }
foreach ($entry in $capability.Files) {
    if ((Get-FileHash -LiteralPath (Join-Path $targetRoot $entry.Path)).Hash -ne $entry.Sha256) { throw 'Post-maintenance hash mismatch.' }
}
if ((Get-FileHash -LiteralPath (Join-Path $targetRoot 'current.json')).Hash -ne $pointerBefore) { throw 'Active pointer changed concurrently; inspect installation before proceeding.' }
Write-Output 'Stable launcher restart support updated; app versions, current pointer, shortcut and user data unchanged. Prior launcher retained in launcher-backup-*.'
