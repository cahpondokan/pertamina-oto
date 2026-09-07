param([Parameter(Mandatory)][string]$InputRoot,[Parameter(Mandatory)][string]$ManifestPath)
$ErrorActionPreference='Stop'
Import-Module (Join-Path $PSScriptRoot 'Environment.psm1') -Force
try {
    $root=Assert-PrivateDirectory $InputRoot
    $manifest=Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
    if($manifest.schema -ne 1 -or @($manifest.files).Count -eq 0) { throw 'EMPTY_MANIFEST' }
    $seen=@{}
    foreach($entry in $manifest.files) {
        $name=[string]$entry.path
        if($name -notmatch '^[A-Za-z0-9_-]+(?:/[A-Za-z0-9_.-]+)*$' -or $name.Contains('..') -or $seen.ContainsKey($name) -or $entry.sha256 -cnotmatch '^[A-F0-9]{64}$') { throw 'MANIFEST_ENTRY' }
        $seen[$name]=$true
        $file=Join-Path $root $name
        if(-not (Test-Path -LiteralPath $file -PathType Leaf) -or (Get-FileHash -LiteralPath $file).Hash -cne $entry.sha256) { throw 'STAGING_DIGEST' }
    }
    $actual=@(Get-ChildItem -LiteralPath $root -File -Recurse -Force)
    if($actual.Count -ne $seen.Count) { throw 'EXTRA_ASSETS' }
    Write-Output 'STAGING=PASS; NO_ASSET_EXECUTED'
} catch { Write-Output 'STAGING=FAIL; NO_RAW_ERROR_EXPORTED'; exit 3 }
