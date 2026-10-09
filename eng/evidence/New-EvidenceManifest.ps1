[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$')]
    [string]$RunId,

    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9a-fA-F]{40}$')]
    [string]$Commit,

    [Parameter(Mandatory)]
    [string]$EvidenceRoot,

    [ValidateSet('PASS', 'FAIL', 'BLOCKED', 'CANARY_ONLY')]
    [string]$Status = 'PASS'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$evidencePath = [System.IO.Path]::GetFullPath($EvidenceRoot)
if (-not (Test-Path -LiteralPath $evidencePath -PathType Container)) {
    New-Item -ItemType Directory -Path $evidencePath -Force | Out-Null
}

$manifestPath = Join-Path $evidencePath 'manifest.json'
$files = @(Get-ChildItem -LiteralPath $evidencePath -File -Recurse | Where-Object {
    $_.FullName -cne $manifestPath
} | Sort-Object FullName)

$entries = foreach ($file in $files) {
    $relative = [System.IO.Path]::GetRelativePath($evidencePath, $file.FullName).Replace('\', '/')
    if ($relative.StartsWith('../', [System.StringComparison]::Ordinal) -or
        [System.IO.Path]::IsPathRooted($relative)) {
        Write-Error 'EVIDENCE_MANIFEST_ERROR=unsafe_relative_path'
        exit 100
    }

    [ordered]@{
        path = $relative
        bytes = [long]$file.Length
        sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    }
}

$manifest = [ordered]@{
    schemaVersion = 1
    runId = $RunId
    commit = $Commit.ToLowerInvariant()
    taskIds = @('BOOT-007', 'BOOT-008')
    decisionIds = @('LOCAL-BOOT-001', 'CCR-015')
    status = $Status
    generatedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
    fileCount = @($entries).Count
    files = @($entries)
}

$manifest | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $manifestPath -Encoding utf8NoBOM
$manifestHash = (Get-FileHash -LiteralPath $manifestPath -Algorithm SHA256).Hash.ToLowerInvariant()

Write-Output "EVIDENCE_MANIFEST_FILES=$(@($entries).Count)"
Write-Output "EVIDENCE_MANIFEST_SHA256=$manifestHash"
Write-Output 'EVIDENCE_MANIFEST_RESULT=PASS'
exit 0
