[CmdletBinding()]
param(
    [string]$Project
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
if ([string]::IsNullOrWhiteSpace($Project)) {
    $Project = Join-Path $repositoryRoot 'tests\Unit\Pertamina.Oto.Foundation.UnitTests\Pertamina.Oto.Foundation.UnitTests.csproj'
}
elseif (-not [System.IO.Path]::IsPathRooted($Project)) {
    $Project = Join-Path $repositoryRoot $Project
}

$safeTempRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
$canaryRoot = Join-Path $safeTempRoot ("oto-quality-canary-{0}" -f [Guid]::NewGuid().ToString('N'))
$canaryRoot = [System.IO.Path]::GetFullPath($canaryRoot)
if (-not $canaryRoot.StartsWith($safeTempRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    Write-Error 'NEGATIVE_CANARY_ERROR=unsafe_temp_path'
    exit 70
}

New-Item -ItemType Directory -Path $canaryRoot -Force | Out-Null
$previousCanary = [Environment]::GetEnvironmentVariable('OTO_CONTROLLED_NEGATIVE_CANARY', 'Process')
$powerShellExecutable = Join-Path $PSHOME 'pwsh.exe'
if (-not (Test-Path -LiteralPath $powerShellExecutable -PathType Leaf)) {
    $powerShellExecutable = Join-Path $PSHOME 'powershell.exe'
}
if (-not (Test-Path -LiteralPath $powerShellExecutable -PathType Leaf)) {
    Write-Error 'NEGATIVE_CANARY_ERROR=powershell_executable_missing'
    exit 76
}

try {
    $zeroResultDirectory = Join-Path $canaryRoot 'zero-results'
    New-Item -ItemType Directory -Path $zeroResultDirectory -Force | Out-Null
    $zeroTrx = @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary outcome="Completed">
    <Counters total="0" executed="0" passed="0" failed="0" error="0" aborted="0" />
  </ResultSummary>
</TestRun>
'@
    Set-Content -LiteralPath (Join-Path $zeroResultDirectory 'zero.trx') -Value $zeroTrx -Encoding utf8NoBOM

    & $powerShellExecutable -NoProfile -NonInteractive -File (Join-Path $PSScriptRoot 'Assert-NonZeroTestResults.ps1') -ResultsDirectory $zeroResultDirectory *> $null
    $zeroGuardExit = $LASTEXITCODE
    Write-Output "NEGATIVE_ZERO_TEST_GUARD_EXIT=$zeroGuardExit"
    if ($zeroGuardExit -eq 0) {
        Write-Error 'NEGATIVE_CANARY_ERROR=zero_test_guard_did_not_block'
        exit 71
    }

    $secretRoot = Join-Path $canaryRoot 'secret-root'
    New-Item -ItemType Directory -Path $secretRoot -Force | Out-Null
    $secretCanaryValue = 'OTO_TEST_ONLY_' + 'SECRET_CANARY_' + '0123456789AB'
    Set-Content -LiteralPath (Join-Path $secretRoot 'canary.env') -Value $secretCanaryValue -Encoding utf8NoBOM

    & $powerShellExecutable -NoProfile -NonInteractive -File (Join-Path $repositoryRoot 'eng\security\Invoke-SecretScan.ps1') -Root $secretRoot *> $null
    $secretScanExit = $LASTEXITCODE
    Write-Output "NEGATIVE_SECRET_SCAN_EXIT=$secretScanExit"
    if ($secretScanExit -eq 0) {
        Write-Error 'NEGATIVE_CANARY_ERROR=secret_scan_did_not_block'
        exit 72
    }

    [Environment]::SetEnvironmentVariable('OTO_CONTROLLED_NEGATIVE_CANARY', 'enabled-for-test-only', 'Process')
    $failingResults = Join-Path $canaryRoot 'failing-test-results'
    & $powerShellExecutable -NoProfile -NonInteractive -File (Join-Path $PSScriptRoot 'Invoke-UnitTests.ps1') -Project $Project -ResultsDirectory $failingResults *> $null
    $failingTestExit = $LASTEXITCODE
    Write-Output "NEGATIVE_FAILING_TEST_EXIT=$failingTestExit"
    if ($failingTestExit -eq 0) {
        Write-Error 'NEGATIVE_CANARY_ERROR=failing_test_did_not_block'
        exit 73
    }
}
finally {
    [Environment]::SetEnvironmentVariable('OTO_CONTROLLED_NEGATIVE_CANARY', $previousCanary, 'Process')
    if (Test-Path -LiteralPath $canaryRoot) {
        Remove-Item -LiteralPath $canaryRoot -Recurse -Force
    }
}

$greenResults = Join-Path $safeTempRoot ("oto-quality-green-{0}" -f [Guid]::NewGuid().ToString('N'))
try {
    & (Join-Path $PSScriptRoot 'Invoke-UnitTests.ps1') -Project $Project -ResultsDirectory $greenResults
    $greenExit = $LASTEXITCODE
    Write-Output "POST_CANARY_GREEN_EXIT=$greenExit"
    if ($greenExit -ne 0) {
        Write-Error 'NEGATIVE_CANARY_ERROR=working_tree_not_green_after_canary'
        exit 74
    }

    & (Join-Path $repositoryRoot 'eng\security\Invoke-SecretScan.ps1') -Root $repositoryRoot
    $postSecretExit = $LASTEXITCODE
    Write-Output "POST_CANARY_SECRET_SCAN_EXIT=$postSecretExit"
    if ($postSecretExit -ne 0) {
        Write-Error 'NEGATIVE_CANARY_ERROR=secret_scan_not_green_after_canary'
        exit 75
    }
}
finally {
    if (Test-Path -LiteralPath $greenResults) {
        Remove-Item -LiteralPath $greenResults -Recurse -Force
    }
}

Write-Output 'NEGATIVE_CANARY_RESULT=PASS'
exit 0
