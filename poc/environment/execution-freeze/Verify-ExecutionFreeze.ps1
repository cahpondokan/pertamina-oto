param(
    [switch]$SelfTest,
    [string]$CoordinationRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Fail([string]$Code) { throw $Code }
function Assert-Equal($Actual, $Expected, [string]$Code) {
    if ($Actual -cne $Expected) { Fail $Code }
}
function Assert-FalseAuthorities([System.Collections.IDictionary]$Authorities, [string]$Code) {
    foreach ($entry in $Authorities.GetEnumerator()) {
        if ($entry.Value -ne $false) { Fail $Code }
    }
}
function Assert-Array([object[]]$Actual, [string[]]$Expected, [string]$Code) {
    if ($Actual.Count -ne $Expected.Count) { Fail $Code }
    for ($i = 0; $i -lt $Expected.Count; $i++) {
        if ([string]$Actual[$i] -cne $Expected[$i]) { Fail $Code }
    }
}
function Read-JsonText([string]$Text, [string]$Code) {
    try { return ($Text | ConvertFrom-Json -AsHashtable -Depth 30) }
    catch { Fail $Code }
}

function Assert-Freeze([string]$ManifestText, [string]$ChecklistText, [string]$PlanText) {
    foreach ($text in @($ManifestText, $ChecklistText, $PlanText)) {
        if ($text -match '(?i)C:\\\\Users\\\\|[D-Z]:\\\\|\\\\\\\\|cahpo|CodexSandboxOffline') { Fail 'RAW_SENSITIVE_PATH' }
    }

    $manifest = Read-JsonText $ManifestText 'MANIFEST_JSON_INVALID'
    Assert-Equal $manifest.schemaVersion 1 'MANIFEST_SCHEMA_INVALID'
    Assert-Equal $manifest.manifestId 'P02-20260905-1434-ENV-FREEZE-WINDOWS-SANDBOX-V1' 'MANIFEST_ID_INVALID'
    Assert-Equal $manifest.baselineCommit '4a78212e01d3daee9ae47f0473fb6186df0dccde' 'BASELINE_INVALID'
    Assert-Equal $manifest.environmentDisposition 'HOST_OBSERVED_GUEST_AND_NETWORK_UNPROVEN' 'DISPOSITION_INVALID'
    Assert-Equal $manifest.costBoundary 'ZERO_ADDITIONAL_COST' 'COST_BOUNDARY_INVALID'
    Assert-Equal $manifest.stopMarker 'STOP_BEFORE_LAUNCH' 'STOP_MARKER_INVALID'
    Assert-FalseAuthorities $manifest.authorities 'AUTHORITY_FLAG_TRUE'
    Assert-Equal $manifest.guestEvidence.guestBooted $false 'GUEST_BOOT_CLAIM_INVALID'
    Assert-Equal $manifest.guestEvidence.status 'UNPROVEN_NOT_LAUNCHED' 'GUEST_STATUS_INVALID'
    Assert-Equal $manifest.hostEvidence.operatingSystemBaseline.version '10.0.26200' 'HOST_OS_BINDING_INVALID'
    Assert-Equal $manifest.hostEvidence.operatingSystemBaseline.architecture '64-bit' 'HOST_ARCH_BINDING_INVALID'
    Assert-Equal $manifest.hostEvidence.operatingSystemBaseline.freshness 'PRE_RESTART_BASELINE_NOT_REOBSERVED_AFTER_RESTART' 'HOST_FRESHNESS_INVALID'
    Assert-Equal $manifest.hostEvidence.runtimeBaseline.dotnetSdk '10.0.400' 'HOST_RUNTIME_BINDING_INVALID'
    Assert-Equal $manifest.hostEvidence.runtimeBaseline.dotnetHost '10.0.11' 'HOST_RUNTIME_BINDING_INVALID'
    Assert-Equal $manifest.hostEvidence.launcher.locationClass 'WINDOWS_SYSTEM32' 'LAUNCHER_LOCATION_NOT_SANITIZED'
    Assert-Equal $manifest.hostEvidence.launcher.basename 'WindowsSandbox.exe' 'LAUNCHER_IDENTITY_INVALID'
    Assert-Equal $manifest.hostEvidence.launcher.sha256 '7C0095F52A1FDED07D90171A005BB495440C92DA099C064E0ED9AD08656AC3FB' 'LAUNCHER_IDENTITY_INVALID'
    Assert-Equal $manifest.hostEvidence.launcher.signatureStatus 'Valid' 'LAUNCHER_IDENTITY_INVALID'
    Assert-Equal $manifest.hostEvidence.launcher.signer 'Microsoft Windows' 'LAUNCHER_IDENTITY_INVALID'
    Assert-Equal $manifest.hostEvidence.launcher.binaryArchitecture 'UNOBSERVED' 'LAUNCHER_ARCH_CLAIM_INVALID'
    foreach ($value in $manifest.hostEvidence.postRestartObservation.checks.Values) {
        Assert-Equal $value 'PASS' 'HOST_OBSERVATION_INVALID'
    }

    $expectedBindings = [ordered]@{
        'SRC-POSTRESTART-REPORT' = 'D9863580DE97AF7D940FE3CC762169DB9B35A9D9239AA6649574D875DAFF29D6'
        'SRC-POSTRESTART-JSON' = '5BA5CF8C75D8A945DD5F88F14918E744C17473F16D97147942F673468BAE924D'
        'SRC-HOST-INVENTORY' = '2CC668283AC1190B3E12E035FA2BFF0793238A5DB673434EC9FAB675E770E306'
        'SRC-W0G-TASK-CARD' = '030C687FA281B0704023EB6B692C1629184119210984EBE85A828A4C68E50E88'
        'SRC-PDF001-CHARTER' = 'E47460DFB3768F6EE76AFB0EF30AAF6F0672905BEFA2A93B5137218BF1176F5A'
        'SRC-SECURITY-DESIGN' = 'DF9C8BA8B70B224206918096FC2ACBE990C452AA932F27871FB36B282F6F404F'
        'SRC-OFFLINE-TEMPLATE' = 'A011670A197F054D2AA787499F401BD48A72E59B1683D036FA8E58576A6BA97F'
        'SRC-TEMPLATE-VALIDATOR' = 'ED63C2F60F55A705CDF8F0BF09436811E92E0524FE5A2C893AF2B92FBBBD6C05'
        'SRC-POSTRESTART-VERIFIER' = 'BA73EED4376233298BFE7FEE1F26492CB5CCE9AA97CC4D8EFC67402978ADCE03'
    }
    if (@($manifest.sourceBindings).Count -ne $expectedBindings.Count) { Fail 'SOURCE_BINDING_COUNT_INVALID' }
    $bindingById = @{}
    foreach ($binding in @($manifest.sourceBindings)) {
        if ($bindingById.ContainsKey($binding.id)) { Fail 'SOURCE_BINDING_DUPLICATE' }
        if ([string]$binding.sha256 -cnotmatch '^[A-F0-9]{64}$') { Fail 'SOURCE_BINDING_HASH_INVALID' }
        $bindingById[$binding.id] = $binding
    }
    foreach ($entry in $expectedBindings.GetEnumerator()) {
        if (-not $bindingById.ContainsKey($entry.Key)) { Fail 'SOURCE_BINDING_MISSING' }
        Assert-Equal $bindingById[$entry.Key].sha256 $entry.Value 'SOURCE_BINDING_HASH_INVALID'
    }

    $checklist = Read-JsonText $ChecklistText 'CHECKLIST_JSON_INVALID'
    Assert-Equal $checklist.checklistId 'P02-20260905-1434-ENV-CONTROLS-V1' 'CHECKLIST_ID_INVALID'
    Assert-Equal $checklist.policy 'DENY_ANY_UNLISTED_SETTING_OR_MAPPING' 'CHECKLIST_POLICY_INVALID'
    Assert-Equal $checklist.templateValidation.status 'PASS_STATIC_ONLY' 'TEMPLATE_STATUS_INVALID'
    Assert-Equal $checklist.templateValidation.runtimeEnforcementProven $false 'RUNTIME_PROOF_PREMATURE'
    $expectedSettings = @('Networking=Disable','vGPU=Disable','AudioInput=Disable','VideoInput=Disable','PrinterRedirection=Disable','ClipboardRedirection=Disable','ProtectedClient=Enable','MappedFolders=EXACT_SINGLE_MAPPING_BELOW')
    $actualSettings = @($checklist.allowlistedConfigurationElements | ForEach-Object { '{0}={1}' -f $_.name,$_.value })
    Assert-Array $actualSettings $expectedSettings 'SETTINGS_INVALID'
    Assert-Equal $checklist.allowedMappings.maximumCount 1 'MAPPING_COUNT_INVALID'
    Assert-Equal $checklist.allowedMappings.input.hostFolderToken '__APPROVED_READONLY_INPUT_ROOT__' 'INPUT_MAPPING_INVALID'
    Assert-Equal $checklist.allowedMappings.input.sandboxFolder 'C:\PocInput' 'INPUT_MAPPING_INVALID'
    Assert-Equal $checklist.allowedMappings.input.readOnly $true 'INPUT_MAPPING_INVALID'
    Assert-Equal $checklist.allowedMappings.input.resolvedHostLocationRecorded $false 'INPUT_MAPPING_INVALID'
    Assert-Equal $checklist.allowedMappings.output.present $false 'OUTPUT_MAPPING_INVALID'
    if (@($checklist.deniedMappings).Count -ne 10) { Fail 'DENIED_MAPPING_SET_INVALID' }
    foreach ($control in @($checklist.controlEvidence)) {
        Assert-Equal $control.runtimeStatus 'NOT_RUN' 'RUNTIME_CLAIM_INVALID'
    }
    Assert-FalseAuthorities $checklist.authorities 'AUTHORITY_FLAG_TRUE'
    Assert-Equal $checklist.stopMarker 'STOP_BEFORE_LAUNCH' 'STOP_MARKER_INVALID'

    $plan = Read-JsonText $PlanText 'PLAN_JSON_INVALID'
    Assert-Equal $plan.status 'PLAN_ONLY_NOT_AUTHORIZED' 'PLAN_STATUS_INVALID'
    Assert-Equal $plan.scope 'BENIGN_ENVIRONMENT_OBSERVATION_ONLY' 'PLAN_SCOPE_INVALID'
    Assert-Equal $plan.stopMarker 'STOP_BEFORE_LAUNCH' 'STOP_MARKER_INVALID'
    if (@($plan.preconditionMatrix).Count -ne 10) { Fail 'PRECONDITION_SET_INVALID' }
    $pre12 = @($plan.preconditionMatrix | Where-Object id -eq 'PRE-012')
    if ($pre12.Count -ne 1 -or $pre12[0].status -cne 'HUMAN_ONLY_NOT_APPROVED') { Fail 'PRE012_INVALID' }
    foreach ($value in $plan.requiredHumanAndSecurityFields.Values) {
        Assert-Equal $value 'REQUIRED_NOT_SET' 'HUMAN_FIELD_PREMATURE'
    }
    if (@($plan.orderedStages).Count -ne 8) { Fail 'PLAN_STAGE_SET_INVALID' }
    if (@($plan.instrumentation.collectors).Count -ne 8) { Fail 'COLLECTOR_SET_INVALID' }
    foreach ($collector in @($plan.instrumentation.collectors)) {
        if ($collector.executionStatus -notmatch '^(NOT_RUN|BLOCKED_)') { Fail 'COLLECTOR_EXECUTION_CLAIM_INVALID' }
    }
    Assert-Equal $plan.evidencePolicy.persistentGuestExportInCurrentProfile $false 'EXPORT_CLAIM_INVALID'
    Assert-Equal $plan.evidencePolicy.writableHostMappingInCurrentProfile $false 'OUTPUT_MAPPING_INVALID'
    Assert-FalseAuthorities $plan.authorities 'AUTHORITY_FLAG_TRUE'

    return [pscustomobject]@{ Manifest=$manifest; Checklist=$checklist; Plan=$plan }
}

$manifestPath = Join-Path $PSScriptRoot 'environment-manifest.v1.json'
$checklistPath = Join-Path $PSScriptRoot 'control-checklist.v1.json'
$planPath = Join-Path $PSScriptRoot 'later-launch-observation-plan.v1.json'
$manifestText = Get-Content -Raw -LiteralPath $manifestPath
$checklistText = Get-Content -Raw -LiteralPath $checklistPath
$planText = Get-Content -Raw -LiteralPath $planPath
$documents = Assert-Freeze $manifestText $checklistText $planText

if (-not [string]::IsNullOrWhiteSpace($CoordinationRoot)) {
    $repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..'))
    foreach ($binding in @($documents.Manifest.sourceBindings)) {
        $root = if ($binding.sourceDomain -ceq 'REPOSITORY_BASELINE') { $repoRoot } elseif ($binding.sourceDomain -ceq 'COORDINATION_WORKSPACE') { $CoordinationRoot } else { Fail 'SOURCE_DOMAIN_INVALID' }
        $candidate = Join-Path $root ([string]$binding.logicalPath).Replace('/', [System.IO.Path]::DirectorySeparatorChar)
        if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) { Fail 'SOURCE_FILE_MISSING' }
        Assert-Equal (Get-FileHash -Algorithm SHA256 -LiteralPath $candidate).Hash $binding.sha256 'SOURCE_FILE_HASH_MISMATCH'
    }
}

if ($SelfTest) {
    $discovered = 1
    foreach ($fixturePath in @(Get-ChildItem -File -LiteralPath (Join-Path $PSScriptRoot 'fixtures') -Filter '*.json' | Sort-Object Name)) {
        $fixture = Get-Content -Raw -LiteralPath $fixturePath | ConvertFrom-Json -AsHashtable
        $target = switch ($fixture.target) {
            'manifest' { $manifestText }
            'checklist' { $checklistText }
            'plan' { $planText }
            default { Fail 'FIXTURE_TARGET_INVALID' }
        }
        $matches = [regex]::Matches($target, [regex]::Escape([string]$fixture.search))
        if ($matches.Count -ne 1) { Fail 'FIXTURE_SEARCH_INVALID' }
        $mutated = $target.Replace([string]$fixture.search, [string]$fixture.replace)
        $m = if ($fixture.target -ceq 'manifest') { $mutated } else { $manifestText }
        $c = if ($fixture.target -ceq 'checklist') { $mutated } else { $checklistText }
        $p = if ($fixture.target -ceq 'plan') { $mutated } else { $planText }
        $failure = $null
        try { $null = Assert-Freeze $m $c $p } catch { $failure = $_.Exception.Message }
        if ($failure -cne $fixture.expectedFailure) { Fail 'NEGATIVE_FIXTURE_NOT_REJECTED_AS_EXPECTED' }
        $discovered++
    }
    Write-Output "DISCOVERED=$discovered PASSED=$discovered FAILED=0; HOST_PROBES=0; PROCESS_LAUNCHES=0"
}

Write-Output 'FREEZE_STATIC_VALIDATION=PASS; LAUNCH_AUTHORIZED=false; PROCESSING_AUTHORIZED=false; NETWORK_PROVEN=false; COST=ZERO_ADDITIONAL_COST'
