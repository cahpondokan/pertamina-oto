[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Target,
    [switch]$NoRestore
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
if (-not [System.IO.Path]::IsPathRooted($Target)) {
    $Target = Join-Path $repositoryRoot $Target
}

if (-not (Test-Path -LiteralPath $Target -PathType Leaf)) {
    Write-Error 'DEPENDENCY_AUDIT_ERROR=target_missing'
    exit 60
}

$arguments = @(
    'list'
    $Target
    'package'
    '--vulnerable'
    '--include-transitive'
    '--format'
    'json'
)
if ($NoRestore) {
    $arguments += '--no-restore'
}

$rawOutput = @(& dotnet @arguments 2>&1)
$auditExitCode = $LASTEXITCODE
Write-Output "DEPENDENCY_AUDIT_COMMAND_EXIT=$auditExitCode"

if ($auditExitCode -ne 0) {
    Write-Error 'DEPENDENCY_AUDIT_RESULT=BLOCKED_OR_FAILED;DETAIL=restore_cache_or_network_may_be_required'
    exit 61
}

$jsonText = $rawOutput -join [Environment]::NewLine
$jsonStart = $jsonText.IndexOf('{')
if ($jsonStart -lt 0) {
    Write-Error 'DEPENDENCY_AUDIT_ERROR=json_report_missing'
    exit 62
}

try {
    $report = $jsonText.Substring($jsonStart) | ConvertFrom-Json -Depth 100
}
catch {
    Write-Error 'DEPENDENCY_AUDIT_ERROR=json_report_invalid'
    exit 63
}

function Get-VulnerabilityCount {
    param([object]$Node)

    if ($null -eq $Node) {
        return 0
    }

    $count = 0
    if ($Node -is [System.Collections.IEnumerable] -and $Node -isnot [string]) {
        foreach ($item in $Node) {
            $count += Get-VulnerabilityCount -Node $item
        }
        return $count
    }

    foreach ($property in $Node.PSObject.Properties) {
        if ($property.Name -eq 'vulnerabilities' -and $null -ne $property.Value) {
            $count += @($property.Value).Count
        }
        elseif ($property.Value -isnot [string] -and $property.Value -isnot [ValueType]) {
            $count += Get-VulnerabilityCount -Node $property.Value
        }
    }

    return $count
}

$vulnerabilityCount = Get-VulnerabilityCount -Node $report
Write-Output "DEPENDENCY_AUDIT_VULNERABILITIES=$vulnerabilityCount"

if ($vulnerabilityCount -gt 0) {
    Write-Error 'DEPENDENCY_AUDIT_RESULT=FAIL'
    exit 64
}

Write-Output 'DEPENDENCY_AUDIT_RESULT=PASS'
exit 0
