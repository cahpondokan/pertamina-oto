[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Join-Path $PSScriptRoot '../../coordination/phase1-20260905/agent-6/canary-results'
New-Item -ItemType Directory $root -Force | Out-Null
$guard = Join-Path $PSScriptRoot 'Assert-NonZeroTestResults.ps1'
function Write-Fixture([string]$directory, [string]$name, [int]$total, [int]$passed) {
    New-Item -ItemType Directory $directory -Force | Out-Null
    $failed = $total - $passed
    Set-Content (Join-Path $directory $name) "<TestRun><ResultSummary><Counters total='$total' executed='$total' passed='$passed' failed='$failed' error='0' aborted='0'/></ResultSummary></TestRun>"
}
foreach ($case in @('mixed-zero', 'failed', 'green')) {
    $directory = Join-Path $root $case
    Write-Fixture $directory 'passing.trx' 1 1
    if ($case -eq 'mixed-zero') { Write-Fixture $directory 'zero.trx' 0 0 }
    if ($case -eq 'failed') { Write-Fixture $directory 'failed.trx' 1 0 }
    & (Join-Path $PSHOME 'pwsh.exe') -NoProfile -File $guard -ResultsDirectory $directory *> $null
    $code = $LASTEXITCODE
    Write-Output "CANARY=$case EXIT=$code"
    if (($case -eq 'green' -and $code -ne 0) -or ($case -ne 'green' -and $code -eq 0)) { exit 1 }
}
exit 0
