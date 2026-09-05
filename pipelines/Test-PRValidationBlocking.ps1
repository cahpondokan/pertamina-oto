[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$safeTempRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
$testRoot = [System.IO.Path]::GetFullPath((Join-Path $safeTempRoot ("oto-pipeline-canary-{0}" -f [Guid]::NewGuid().ToString('N'))))
if (-not $testRoot.StartsWith($safeTempRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    Write-Error 'PIPELINE_BLOCKING_TEST_ERROR=unsafe_temp_path'
    exit 120
}

$previous = [Environment]::GetEnvironmentVariable('OTO_CONTROLLED_PIPELINE_CANARY', 'Process')
$powerShellHost = Join-Path $PSHOME 'pwsh.exe'
if (-not (Test-Path -LiteralPath $powerShellHost -PathType Leaf)) {
    Write-Error 'PIPELINE_BLOCKING_TEST_ERROR=pwsh_host_missing'
    exit 126
}
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null

try {
    [Environment]::SetEnvironmentVariable('OTO_CONTROLLED_PIPELINE_CANARY', 'phase0-agent7', 'Process')
    $failureRoot = Join-Path $testRoot 'failure'
    & $powerShellHost `
        -NoProfile `
        -File (Join-Path $PSScriptRoot 'Invoke-PRValidation.ps1') `
        -EngineSelfTest `
        -ControlledCanaryStage 'Contract' `
        -RunId 'agent7-negative-canary' `
        -EvidenceRoot $failureRoot *> $null
    $negativeExit = $LASTEXITCODE
    Write-Output "PIPELINE_NEGATIVE_CANARY_EXIT=$negativeExit"
    if ($negativeExit -eq 0) {
        Write-Error 'PIPELINE_BLOCKING_TEST_ERROR=failure_not_blocked'
        exit 121
    }

    $failureSummaryPath = Join-Path $failureRoot 'pipeline-summary.json'
    if (-not (Test-Path -LiteralPath $failureSummaryPath -PathType Leaf)) {
        Write-Error 'PIPELINE_BLOCKING_TEST_ERROR=failure_summary_missing'
        exit 122
    }
    $failureSummary = Get-Content -LiteralPath $failureSummaryPath -Raw | ConvertFrom-Json -Depth 20
    $contract = @($failureSummary.stages | Where-Object { $_.name -ceq 'Contract' })
    $later = @($failureSummary.stages | Select-Object -Skip 5)
    if ($failureSummary.status -cne 'FAIL' -or
        $contract.Count -ne 1 -or
        $contract[0].status -cne 'FAIL' -or
        @($later | Where-Object { $_.status -cne 'NOT_RUN' }).Count -ne 0) {
        Write-Error 'PIPELINE_BLOCKING_TEST_ERROR=fail_fast_contract_not_met'
        exit 123
    }

    $successRoot = Join-Path $testRoot 'success'
    & $powerShellHost `
        -NoProfile `
        -File (Join-Path $PSScriptRoot 'Invoke-PRValidation.ps1') `
        -EngineSelfTest `
        -RunId 'agent7-positive-control' `
        -EvidenceRoot $successRoot *> $null
    $positiveExit = $LASTEXITCODE
    Write-Output "PIPELINE_POSITIVE_CONTROL_EXIT=$positiveExit"
    if ($positiveExit -ne 0) {
        Write-Error 'PIPELINE_BLOCKING_TEST_ERROR=positive_control_failed'
        exit 124
    }

    $successSummary = Get-Content -LiteralPath (Join-Path $successRoot 'pipeline-summary.json') -Raw | ConvertFrom-Json -Depth 20
    if ($successSummary.status -cne 'CANARY_ONLY' -or
        @($successSummary.stages | Where-Object { $_.status -cne 'PASS' }).Count -ne 0) {
        Write-Error 'PIPELINE_BLOCKING_TEST_ERROR=positive_control_contract_not_met'
        exit 125
    }
}
finally {
    [Environment]::SetEnvironmentVariable('OTO_CONTROLLED_PIPELINE_CANARY', $previous, 'Process')
    if (Test-Path -LiteralPath $testRoot) {
        Remove-Item -LiteralPath $testRoot -Recurse -Force
    }
}

Write-Output 'PIPELINE_BLOCKING_TEST_RESULT=PASS'
exit 0
