[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $ArtifactRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Assert-Equal {
    param([object] $Actual, [object] $Expected, [string] $Label)
    if ([string]$Actual -cne [string]$Expected) {
        throw "$Label mismatch: expected '$Expected', observed '$Actual'"
    }
}

function Assert-False {
    param([object] $Value, [string] $Label)
    if ($Value -ne $false) { throw "$Label must be false" }
}

function Get-Sha256 {
    param([string] $Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Missing required file: $Path"
    }
    (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash
}

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..\..'))
$manifest = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'candidate-freeze.json') |
    ConvertFrom-Json -Depth 100

Assert-Equal $manifest.packetId 'P2-W0G-A2-PDFSHARP-CANDIDATE-FREEZE' 'packetId'
Assert-Equal $manifest.baselineCommit '4a78212e01d3daee9ae47f0473fb6186df0dccde' 'baseline'
Assert-Equal $manifest.status 'FREEZE_PROPOSAL_STATIC_ONLY_NOT_EXECUTION_READY' 'status'
Assert-Equal $manifest.scope.targetFrameworkProposal 'net10.0' 'TFM'
Assert-Equal $manifest.scope.runtimeIdentifierProposal 'win-x64' 'RID'
Assert-Equal $manifest.scope.assetResolutionStatus 'NOT_RESTORED_NOT_RESOLVED' 'asset status'
Assert-False $manifest.cost.licenseFeeObserved 'license fee'
Assert-False $manifest.cost.paidSupportOrServiceIncluded 'paid support/service'
Assert-False $manifest.freezeValidity.effectiveForExecution 'execution validity'
Assert-Equal $manifest.freezeValidity.expiryStatus 'UNSET_HUMAN_DECISION_BLOCKER' 'expiry status'
if ($null -ne $manifest.freezeValidity.validUntil) { throw 'validUntil must be null' }

$flags = @($manifest.currentAuthorizationFlags.PSObject.Properties)
Assert-Equal $flags.Count 14 'authorization flag count'
foreach ($flag in $flags) { Assert-False $flag.Value "authorization.$($flag.Name)" }

$expected = [ordered]@{
    'PDFsharp' = @{version='6.2.4';bytes=4625326;sha256='5647882339B327CBF6DA9EA3F882EFB335F8D602E6FD45323ADF9E78DA53CF67';file='pdfsharp.6.2.4.nupkg';root='agent-2-pdfsharp-intake';url='https://api.nuget.org/v3-flatcontainer/pdfsharp/6.2.4/pdfsharp.6.2.4.nupkg'}
    'Microsoft.Extensions.Logging.Abstractions' = @{version='8.0.3';bytes=802188;sha256='E4C498D5A13051B4577A148F1D8C3470167215C507E2392069B75DC61322BB74';file='microsoft.extensions.logging.abstractions.8.0.3.nupkg';root='agent-2-dependency-intake';receipt='microsoft.extensions.logging.abstractions.8.0.3.acquisition.json';receiptSha='D3D6E21E7B2DE2DFFA93B47446F8244D59434E2549C76794464D0ECB0D11EA0B';url='https://api.nuget.org/v3-flatcontainer/microsoft.extensions.logging.abstractions/8.0.3/microsoft.extensions.logging.abstractions.8.0.3.nupkg'}
    'System.Security.Cryptography.Pkcs' = @{version='8.0.1';bytes=1078389;sha256='28C348909DF243FE4EE962213E3C8021AAECBC8321929DBA03A69BCB92A49DE5';file='system.security.cryptography.pkcs.8.0.1.nupkg';root='agent-2-dependency-intake';receipt='system.security.cryptography.pkcs.8.0.1.acquisition.json';receiptSha='D427151DF65BC6B64BC09747F5AE46CD0F3251589ECA0FE37B7BD9A86DAD6315';url='https://api.nuget.org/v3-flatcontainer/system.security.cryptography.pkcs/8.0.1/system.security.cryptography.pkcs.8.0.1.nupkg'}
    'Microsoft.Extensions.DependencyInjection.Abstractions' = @{version='8.0.2';bytes=278862;sha256='51F2DF1100245F10DA54F0BB7E813F277155117777D4FBBAB902214E27372606';file='microsoft.extensions.dependencyinjection.abstractions.8.0.2.nupkg';root='agent-2-dependency-intake';receipt='microsoft.extensions.dependencyinjection.abstractions.8.0.2.acquisition.json';receiptSha='B90BDEB3EA2B96ED0A202CB79D04600B5EBA47557ACD2C15A2F676A274B53E26';url='https://api.nuget.org/v3-flatcontainer/microsoft.extensions.dependencyinjection.abstractions/8.0.2/microsoft.extensions.dependencyinjection.abstractions.8.0.2.nupkg'}
}

Assert-Equal @($manifest.components).Count 4 'component count'
$packageResults = @()
foreach ($component in $manifest.components) {
    if (-not $expected.Contains($component.id)) { throw "Unexpected component $($component.id)" }
    $item = $expected[$component.id]
    Assert-Equal $component.version $item.version "$($component.id) version"
    Assert-Equal $component.artifact.bytes $item.bytes "$($component.id) bytes"
    Assert-Equal $component.artifact.sha256 $item.sha256 "$($component.id) SHA256"
    Assert-Equal $component.artifact.sourceUrl $item.url "$($component.id) URL"
    Assert-Equal $component.license.expression 'MIT' "$($component.id) license"
    if ($component.trustAndSecurity.status -notlike 'BLOCKED_SECURITY_*') {
        throw "$($component.id) security hold missing"
    }

    $archive = Join-Path ([string]$ArtifactRoot) "$($item.root)\$($item.file)"
    Assert-Equal (Get-Item -LiteralPath $archive).Length $item.bytes "$($component.id) raw length"
    Assert-Equal (Get-Sha256 $archive) $item.sha256 "$($component.id) raw hash"

    if ($item.ContainsKey('receipt')) {
        $receiptPath = Join-Path ([string]$ArtifactRoot) "$($item.root)\$($item.receipt)"
        Assert-Equal (Get-Sha256 $receiptPath) $item.receiptSha "$($component.id) receipt hash"
        $receipt = Get-Content -Raw -LiteralPath $receiptPath | ConvertFrom-Json
        Assert-Equal $receipt.id $component.id "$($component.id) receipt id"
        Assert-Equal $receipt.version $item.version "$($component.id) receipt version"
        Assert-Equal $receipt.source $item.url "$($component.id) receipt source"
        Assert-Equal $receipt.sha256 $item.sha256 "$($component.id) receipt artifact hash"
        Assert-Equal $receipt.catalogHashMatch $true "$($component.id) catalog hash"
    }
    $packageResults += [ordered]@{id=$component.id;version=$component.version;bytes=$item.bytes;sha256=$item.sha256;rawByteHashVerified=$true;candidateLoaded=$false}
}

Assert-Equal @($manifest.dependencyEdges).Count 3 'edge count'
Assert-Equal @($manifest.components | Where-Object id -eq 'PDFsharp')[0].candidateLibraryAssets.Count 9 'PDFsharp DLL candidates'
Assert-Equal $manifest.activationPolicyProposal.buildPolicy.sourceGenerators 'DENY_BY_DEFAULT' 'generator policy'
Assert-Equal $manifest.activationPolicyProposal.runtimePolicy.dynamicPluginOrReflectionLoad 'DENY' 'dynamic load policy'
Assert-Equal $manifest.activationPolicyProposal.runtimePolicy.processOrShellLaunch 'DENY' 'process policy'
Assert-Equal $manifest.activationPolicyProposal.runtimePolicy.networkOrEgress 'DENY_AND_OBSERVE_ATTEMPTS' 'network policy'
Assert-Equal $manifest.offlineLockedRestoreDesign.status 'DESIGN_ONLY_NO_RESTORE_AUTHORITY' 'restore policy'
Assert-Equal $manifest.plannedCandidateInvocation.status 'BLOCKED_NOT_IMPLEMENTED_NOT_AUTHORIZED' 'invocation status'
if ($null -ne $manifest.plannedCandidateInvocation.entrypointDigest) { throw 'entrypoint digest must be null' }
foreach ($output in $manifest.plannedCandidateInvocation.expectedOutputs) { Assert-False $output.publishAuthorized "output $($output.logicalPath)" }
Assert-Equal $manifest.sbomLicenseSecurity.sbomFormat 'UNDECIDED_BLOCKER_PER_CHARTER_SBOM_TBC' 'SBOM status'

Assert-Equal @($manifest.evidenceBindings).Count 15 'evidence binding count'
foreach ($binding in $manifest.evidenceBindings) {
    $path = [IO.Path]::GetFullPath((Join-Path $repoRoot $binding.path))
    if (-not $path.StartsWith($repoRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw "Path escape $($binding.path)" }
    Assert-Equal (Get-Sha256 $path) $binding.sha256 "evidence $($binding.path)"
}

$inspection = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'poc/intake/pdfsharp-6.2.4/inspection.json') | ConvertFrom-Json
Assert-Equal $inspection.entryCount 111 'PDFsharp entry count'
Assert-False $inspection.assemblyExecution 'PDFsharp inspection execution'
$closure = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'poc/intake/dependency-closure/closure.json') | ConvertFrom-Json -Depth 50
Assert-Equal @($closure.components).Count 4 'closure nodes'
Assert-Equal $closure.resolutionStatus 'METADATA_ONLY_PROPOSED_NOT_RESTORED' 'closure resolution'
Assert-False $closure.processingAuthorized 'closure processing authority'

[ordered]@{
    schemaVersion = 1
    packetId = $manifest.packetId
    result = 'PASS_STATIC_FREEZE_BINDING'
    baselineCommit = $manifest.baselineCommit
    componentsVerified = $packageResults.Count
    dependencyEdgesVerified = @($manifest.dependencyEdges).Count
    evidenceBindingsVerified = @($manifest.evidenceBindings).Count
    authorizationFlagsVerifiedFalse = $flags.Count
    rawArtifactsVerified = $true
    restoreOrBuild = $false
    assemblyAnalyzerGeneratorLoad = $false
    sandboxLaunch = $false
    pdfProcessing = $false
    networkAcquisition = $false
    paidCommitment = $false
    packages = $packageResults
} | ConvertTo-Json -Depth 10
