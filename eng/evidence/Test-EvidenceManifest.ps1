[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$safeTempRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
$fixtureRoot = [System.IO.Path]::GetFullPath((Join-Path $safeTempRoot ("oto-evidence-manifest-{0}" -f [Guid]::NewGuid().ToString('N'))))
if (-not $fixtureRoot.StartsWith($safeTempRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    Write-Error 'EVIDENCE_MANIFEST_TEST_ERROR=unsafe_temp_path'
    exit 140
}

$powerShellHost = Join-Path $PSHOME 'pwsh.exe'
$manifestScript = Join-Path $PSScriptRoot 'New-EvidenceManifest.ps1'
$commit = '0123456789abcdef0123456789abcdef01234567'

New-Item -ItemType Directory -Path (Join-Path $fixtureRoot 'logs') -Force | Out-Null
try {
    'SANITIZED_TEST_RESULT=PASS' | Set-Content -LiteralPath (Join-Path $fixtureRoot 'logs\test.log') -Encoding utf8NoBOM
    & $powerShellHost `
        -NoProfile `
        -File $manifestScript `
        -RunId 'agent7-manifest-self-test' `
        -Commit $commit `
        -EvidenceRoot $fixtureRoot `
        -Status 'CANARY_ONLY' *> $null
    $manifestExit = $LASTEXITCODE
    Write-Output "EVIDENCE_MANIFEST_TEST_COMMAND_EXIT=$manifestExit"
    if ($manifestExit -ne 0) {
        Write-Error 'EVIDENCE_MANIFEST_TEST_ERROR=generator_failed'
        exit 141
    }

    $manifestPath = Join-Path $fixtureRoot 'manifest.json'
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json -Depth 20
    $expectedHash = (Get-FileHash -LiteralPath (Join-Path $fixtureRoot 'logs\test.log') -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($manifest.commit -cne $commit -or
        $manifest.status -cne 'CANARY_ONLY' -or
        $manifest.fileCount -ne 1 -or
        $manifest.files[0].path -cne 'logs/test.log' -or
        $manifest.files[0].sha256 -cne $expectedHash -or
        [System.IO.Path]::IsPathRooted([string]$manifest.files[0].path)) {
        Write-Error 'EVIDENCE_MANIFEST_TEST_ERROR=manifest_contract_mismatch'
        exit 142
    }
}
finally {
    if (Test-Path -LiteralPath $fixtureRoot) {
        Remove-Item -LiteralPath $fixtureRoot -Recurse -Force
    }
}

Write-Output 'EVIDENCE_MANIFEST_TEST_RESULT=PASS'
exit 0
