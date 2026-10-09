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
    Write-Error 'SECRET_SCAN_ERROR=root_missing'
    exit 40
}

$resolvedRoot = (Resolve-Path -LiteralPath $Root).Path
$allowedExtensions = @(
    '.cs', '.csproj', '.props', '.targets', '.json', '.config', '.xml',
    '.yml', '.yaml', '.ps1', '.sh', '.cmd', '.bat', '.env', '.ini'
)
$allowedNames = @('.env', '.npmrc', 'nuget.config')
$excludedSegments = @('.git', 'bin', 'obj', 'TestResults', 'artifacts', 'packages')

$patterns = @(
    [pscustomobject]@{
        Id = 'SEC-SECRET-001'
        Regex = [regex]::new(('-----' + 'BEGIN(?: [A-Z0-9]+)? PRIVATE KEY' + '-----'), 'IgnoreCase')
    },
    [pscustomobject]@{
        Id = 'SEC-SECRET-002'
        Regex = [regex]::new(('AK' + 'IA[0-9A-Z]{16}'))
    },
    [pscustomobject]@{
        Id = 'SEC-SECRET-003'
        Regex = [regex]::new(('gh' + '[pousr]_[A-Za-z0-9]{36,}'))
    },
    [pscustomobject]@{
        Id = 'SEC-SECRET-004'
        Regex = [regex]::new(('(?i)(?:[?&])' + 'sig=[A-Za-z0-9%+/=]{16,}'))
    },
    [pscustomobject]@{
        Id = 'SEC-SECRET-005'
        Regex = [regex]::new(('(?i)(?:password|pwd|client[_-]?' + 'secret)\s*[:=]\s*["'']?[^\s"'']{12,}'))
    },
    [pscustomobject]@{
        Id = 'SEC-SECRET-CANARY'
        Regex = [regex]::new(('OTO_TEST_ONLY_' + 'SECRET_CANARY_[A-Z0-9]{12}'))
    }
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

    $notExcluded -and (
        $allowedExtensions -contains $_.Extension.ToLowerInvariant() -or
        $allowedNames -contains $_.Name.ToLowerInvariant()
    )
})

$findings = [System.Collections.Generic.List[object]]::new()
foreach ($file in $files) {
    try {
        $content = Get-Content -LiteralPath $file.FullName -Raw -ErrorAction Stop
    }
    catch {
        Write-Error 'SECRET_SCAN_ERROR=file_read_failed'
        exit 41
    }

    foreach ($pattern in $patterns) {
        $matches = $pattern.Regex.Matches($content)
        foreach ($match in $matches) {
            $line = 1 + ([regex]::Matches($content.Substring(0, $match.Index), "`n")).Count
            $findings.Add([pscustomobject]@{
                Id = $pattern.Id
                File = [System.IO.Path]::GetRelativePath($resolvedRoot, $file.FullName).Replace('\', '/')
                Line = $line
            })
        }
    }
}

Write-Output "SECRET_SCAN_FILES=$($files.Count)"
Write-Output "SECRET_SCAN_FINDINGS=$($findings.Count)"
foreach ($finding in $findings) {
    Write-Output "SECRET_FINDING=$($finding.Id);FILE=$($finding.File);LINE=$($finding.Line)"
}

if ($findings.Count -gt 0) {
    Write-Error 'SECRET_SCAN_RESULT=FAIL'
    exit 42
}

Write-Output 'SECRET_SCAN_RESULT=PASS'
exit 0
