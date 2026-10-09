[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$ResultsDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $ResultsDirectory -PathType Container)) {
    Write-Error 'TEST_GATE_ERROR=results_directory_missing'
    exit 20
}

$resultFiles = @(Get-ChildItem -LiteralPath $ResultsDirectory -Filter '*.trx' -File -Recurse)
if ($resultFiles.Count -eq 0) {
    Write-Error 'TEST_GATE_ERROR=trx_missing'
    exit 21
}

$totals = [ordered]@{
    Total = 0
    Executed = 0
    Passed = 0
    Failed = 0
    Error = 0
    Aborted = 0
}

foreach ($resultFile in $resultFiles) {
    try {
        [xml]$document = Get-Content -LiteralPath $resultFile.FullName -Raw
    }
    catch {
        Write-Error 'TEST_GATE_ERROR=trx_invalid_xml'
        exit 22
    }

    $counters = $document.SelectSingleNode("//*[local-name()='ResultSummary']/*[local-name()='Counters']")
    if ($null -eq $counters) {
        Write-Error 'TEST_GATE_ERROR=trx_counters_missing'
        exit 23
    }

    $perFile = @{}
    foreach ($name in @('total', 'executed', 'passed', 'failed', 'error', 'aborted')) {
        $value = 0
        $attribute = $counters.Attributes[$name]
        if ($null -eq $attribute -or -not [int]::TryParse($attribute.Value, [ref]$value) -or $value -lt 0) {
            Write-Error 'TEST_GATE_ERROR=invalid_per_file_counter'
            exit 28
        }
        $perFile[$name] = $value
    }
    if ($perFile.total -le 0 -or $perFile.executed -le 0 -or
        $perFile.total -ne $perFile.executed -or $perFile.passed -ne $perFile.executed -or
        ($perFile.failed + $perFile.error + $perFile.aborted) -gt 0) {
        Write-Error 'TEST_GATE_ERROR=non_passing_individual_suite'
        exit 29
    }
    foreach ($field in @('Total', 'Executed', 'Passed', 'Failed', 'Error', 'Aborted')) {
        $attributeName = $field.ToLowerInvariant()
        $attribute = $counters.Attributes[$attributeName]
        if ($null -ne $attribute) {
            $totals[$field] += [int]$attribute.Value
        }
    }
}

Write-Output "TEST_RESULT_FILES=$($resultFiles.Count)"
Write-Output "TEST_DISCOVERED=$($totals.Total)"
Write-Output "TEST_EXECUTED=$($totals.Executed)"
Write-Output "TEST_PASSED=$($totals.Passed)"
Write-Output "TEST_FAILED=$($totals.Failed)"
Write-Output "TEST_ERROR=$($totals.Error)"
Write-Output "TEST_ABORTED=$($totals.Aborted)"

if ($totals.Total -le 0) {
    Write-Error 'TEST_GATE_ERROR=zero_test_discovery'
    exit 24
}

if ($totals.Executed -le 0) {
    Write-Error 'TEST_GATE_ERROR=zero_test_execution'
    exit 25
}

if (($totals.Failed + $totals.Error + $totals.Aborted) -gt 0) {
    Write-Error 'TEST_GATE_ERROR=non_passing_test_result'
    exit 26
}

if ($totals.Passed -ne $totals.Executed) {
    Write-Error 'TEST_GATE_ERROR=incomplete_passing_result'
    exit 27
}

Write-Output 'TEST_GATE_RESULT=PASS'
exit 0
