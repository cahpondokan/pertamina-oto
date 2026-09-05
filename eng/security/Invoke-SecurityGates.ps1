[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$DependencyTarget,
    [switch]$NoRestore
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

& (Join-Path $PSScriptRoot 'Invoke-SecretScan.ps1')
$stepExitCode = $LASTEXITCODE
Write-Output "SECURITY_GATE_STEP=SecretScan;EXIT=$stepExitCode"
if ($stepExitCode -ne 0) {
    Write-Error 'SECURITY_GATE_RESULT=FAIL;STEP=SecretScan'
    exit $stepExitCode
}

& (Join-Path $PSScriptRoot 'Invoke-StaticChecks.ps1')
$stepExitCode = $LASTEXITCODE
Write-Output "SECURITY_GATE_STEP=StaticChecks;EXIT=$stepExitCode"
if ($stepExitCode -ne 0) {
    Write-Error 'SECURITY_GATE_RESULT=FAIL;STEP=StaticChecks'
    exit $stepExitCode
}

$dependencyArguments = @{ Target = $DependencyTarget }
if ($NoRestore) {
    $dependencyArguments.NoRestore = $true
}

& (Join-Path $PSScriptRoot 'Invoke-DependencyAudit.ps1') @dependencyArguments
$stepExitCode = $LASTEXITCODE
Write-Output "SECURITY_GATE_STEP=DependencyAudit;EXIT=$stepExitCode"
if ($stepExitCode -ne 0) {
    Write-Error 'SECURITY_GATE_RESULT=FAIL;STEP=DependencyAudit'
    exit $stepExitCode
}

Write-Output 'SECURITY_GATE_RESULT=PASS'
exit 0
