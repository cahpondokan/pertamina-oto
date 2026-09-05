[CmdletBinding()]
param(
    [string]$Root
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
if ([string]::IsNullOrWhiteSpace($Root)) {
    $Root = $repositoryRoot
}

if (-not (Test-Path -LiteralPath $Root -PathType Container)) {
    Write-Error 'STATIC_SCAN_ERROR=root_missing'
    exit 50
}

$resolvedRoot = (Resolve-Path -LiteralPath $Root).Path
$extensions = @('.cs', '.csproj', '.props', '.targets')
$excludedSegments = @('.git', 'bin', 'obj', 'TestResults', 'artifacts', 'packages')
$rules = @(
    [pscustomobject]@{ Id = 'SEC-STATIC-001'; Pattern = 'DangerousAcceptAnyServerCertificateValidator' },
    [pscustomobject]@{ Id = 'SEC-STATIC-002'; Pattern = 'ServerCertificateCustomValidationCallback\s*=' },
    [pscustomobject]@{ Id = 'SEC-STATIC-003'; Pattern = '#pragma\s+warning\s+disable' },
    [pscustomobject]@{ Id = 'SEC-STATIC-004'; Pattern = '<TreatWarningsAsErrors>\s*false\s*</TreatWarningsAsErrors>' },
    [pscustomobject]@{ Id = 'SEC-STATIC-005'; Pattern = '<AllowUnsafeBlocks>\s*true\s*</AllowUnsafeBlocks>' },
    [pscustomobject]@{ Id = 'SEC-STATIC-006'; Pattern = '\[(?:DllImport|LibraryImport)\s*\(' },
    [pscustomobject]@{ Id = 'SEC-STATIC-007'; Pattern = '(?:System\.Diagnostics\.)?Process\.Start\s*\(' }
)

$files = @(Get-ChildItem -LiteralPath $resolvedRoot -Recurse -File | Where-Object {
    $relative = [System.IO.Path]::GetRelativePath($resolvedRoot, $_.FullName)
    $segments = $relative -split '[\\/]'
    $notExcluded = $true
    foreach ($excluded in $excludedSegments) {
        if ($segments -contains $excluded) {
            $notExcluded = $false
            break
        }
    }

    $notExcluded -and ($extensions -contains $_.Extension.ToLowerInvariant())
})

$findings = [System.Collections.Generic.List[object]]::new()
foreach ($file in $files) {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    foreach ($rule in $rules) {
        $matches = [regex]::Matches($content, $rule.Pattern, 'IgnoreCase')
        foreach ($match in $matches) {
            $line = 1 + ([regex]::Matches($content.Substring(0, $match.Index), "`n")).Count
            $findings.Add([pscustomobject]@{
                Id = $rule.Id
                File = [System.IO.Path]::GetRelativePath($resolvedRoot, $file.FullName).Replace('\', '/')
                Line = $line
            })
        }
    }
}

Write-Output "STATIC_SCAN_FILES=$($files.Count)"
Write-Output "STATIC_SCAN_FINDINGS=$($findings.Count)"
foreach ($finding in $findings) {
    Write-Output "STATIC_FINDING=$($finding.Id);FILE=$($finding.File);LINE=$($finding.Line)"
}

if ($findings.Count -gt 0) {
    Write-Error 'STATIC_SCAN_RESULT=FAIL'
    exit 51
}

Write-Output 'STATIC_SCAN_RESULT=PASS'
exit 0
