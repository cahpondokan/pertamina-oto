param([Parameter(Mandatory)][string]$BundleManifest,[Parameter(Mandatory)][string]$SupplementManifest)
$ErrorActionPreference='Stop'
Import-Module (Join-Path $PSScriptRoot 'Environment.psm1') -Force
try {
    $bundle=Get-Content -LiteralPath $BundleManifest -Raw | ConvertFrom-Json
    $supplement=Get-Content -LiteralPath $SupplementManifest -Raw | ConvertFrom-Json
    ConvertTo-StagingManifest $bundle $supplement
} catch { Write-Output 'MANIFEST_ADAPTER_FAILED'; exit 3 }
