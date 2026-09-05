[CmdletBinding()]
param(
    [string]$RepositoryRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
}
else {
    $RepositoryRoot = [System.IO.Path]::GetFullPath($RepositoryRoot)
}

$forbiddenRoots = @(
    'src\Features',
    'src\Hosts',
    'src\Modules',
    'src\Infrastructure',
    'deploy',
    'deployment',
    'infra\production'
)

$findings = [System.Collections.Generic.List[string]]::new()
foreach ($relativePath in $forbiddenRoots) {
    if (Test-Path -LiteralPath (Join-Path $RepositoryRoot $relativePath)) {
        $findings.Add($relativePath.Replace('\', '/'))
    }
}

$sourceRoot = Join-Path $RepositoryRoot 'src'
if (Test-Path -LiteralPath $sourceRoot -PathType Container) {
    $allowedPrefixes = @(
        'src/BuildingBlocks/Pertamina.Oto.Contracts/',
        'src/BuildingBlocks/Pertamina.Oto.Processing.Abstractions/'
    )
    foreach ($file in @(Get-ChildItem -LiteralPath $sourceRoot -Recurse -File)) {
        $relative = [System.IO.Path]::GetRelativePath($RepositoryRoot, $file.FullName).Replace('\', '/')
        if ($relative -match '/(?:bin|obj)/') {
            continue
        }
        $allowed = $false
        foreach ($prefix in $allowedPrefixes) {
            if ($relative.StartsWith($prefix, [System.StringComparison]::Ordinal)) {
                $allowed = $true
                break
            }
        }
        if (-not $allowed) {
            $findings.Add($relative)
        }
    }
}

Write-Output "PHASE0_SCOPE_FINDINGS=$($findings.Count)"
if ($findings.Count -gt 0) {
    foreach ($finding in @($findings | Sort-Object -Unique)) {
        Write-Output "PHASE0_SCOPE_FINDING=$finding"
    }
    Write-Error 'PHASE0_SCOPE_RESULT=FAIL'
    exit 91
}

Write-Output 'PHASE0_SCOPE_RESULT=PASS'
exit 0
