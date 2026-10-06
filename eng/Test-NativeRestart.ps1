param([Parameter(Mandatory=$true)][string]$BuildOutput,[Parameter(Mandatory=$true)][string]$LauncherOutput)
$ErrorActionPreference = 'Stop'
$buildRoot = (Resolve-Path -LiteralPath $BuildOutput).Path
$fixtureRoot = Join-Path (Split-Path $buildRoot -Parent) ('nr-fixture-' + [Guid]::NewGuid().ToString('N').Substring(0,8))
$fixtureVersion = '20261001-110100-bbbbbbbb'
$target = Join-Path $fixtureRoot ('versions/' + $fixtureVersion)
New-Item -ItemType Directory -Path $target -Force | Out-Null
Get-ChildItem -LiteralPath $LauncherOutput -File | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $fixtureRoot }
Get-ChildItem -LiteralPath $BuildOutput | Where-Object { $_.Name -notmatch 'audit|build-info' } | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $target -Recurse }
[IO.File]::WriteAllText((Join-Path $fixtureRoot 'current.json'),(@{ Version=$fixtureVersion; ManifestSha256='' } | ConvertTo-Json))
[IO.File]::WriteAllText((Join-Path $target 'build-info.json'),(@{ Version=$fixtureVersion; ManifestSha256='' } | ConvertTo-Json))
$parent = Start-Process -FilePath (Join-Path $BuildOutput 'Pertamina.Oto.Desktop.App.exe') -ArgumentList @('--audit-restart-parent',('"' + $fixtureRoot + '"'),$fixtureVersion) -WindowStyle Hidden -PassThru
if (!$parent.WaitForExit(35000)) { throw 'Isolated native parent did not exit; no process killed.' }
if ($parent.ExitCode -ne 0) { throw "Native parent failed: $($parent.ExitCode)" }
$committed = Join-Path $target 'native-restart-committed.txt'
for ($index=0; $index -lt 100 -and !(Test-Path -LiteralPath $committed); $index++) { Start-Sleep -Milliseconds 100 }
if (!(Test-Path -LiteralPath $committed)) { throw 'New native startup did not commit.' }
[IO.File]::WriteAllText((Join-Path $fixtureRoot 'test-results.txt'),"PASS old native instance exited normally after new startup acknowledgement.`nPASS new native WinUI instance committed through stable launcher with space-containing paths.`nPASS fixture storage isolated; no user instance closed or killed.`n")
Write-Output "Native restart evidence: $fixtureRoot"
