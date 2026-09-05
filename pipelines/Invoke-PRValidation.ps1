[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [ValidatePattern('^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$')]
    [string]$RunId,

    [string]$EvidenceRoot,

    [switch]$EngineSelfTest,

    [ValidateSet('', 'Restore', 'Build', 'Unit', 'Architecture', 'Contract', 'Dependency Audit', 'Secret Scan', 'Static Analysis', 'Safe Reports')]
    [string]$ControlledCanaryStage = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ([string]::IsNullOrWhiteSpace($RunId)) {
    $RunId = 'phase0-{0}' -f [DateTimeOffset]::UtcNow.ToString('yyyyMMddTHHmmssZ')
}
if ([string]::IsNullOrWhiteSpace($EvidenceRoot)) {
    $EvidenceRoot = Join-Path $repositoryRoot ("artifacts\evidence\{0}" -f $RunId)
}
$EvidenceRoot = [System.IO.Path]::GetFullPath($EvidenceRoot)

if ($EngineSelfTest) {
    if ([Environment]::GetEnvironmentVariable('OTO_CONTROLLED_PIPELINE_CANARY', 'Process') -cne 'phase0-agent7') {
        Write-Error 'PIPELINE_SELF_TEST_ERROR=explicit_canary_environment_required'
        exit 110
    }
}
elseif (-not [string]::IsNullOrWhiteSpace($ControlledCanaryStage)) {
    Write-Error 'PIPELINE_CANARY_ERROR=canary_only_allowed_in_engine_self_test'
    exit 111
}

function ConvertTo-SafeLogText {
    param([object[]]$Items)

    $text = ($Items | ForEach-Object { [string]$_ }) -join [Environment]::NewLine
    $replacements = @(
        [pscustomobject]@{ Value = $repositoryRoot; Replacement = '<REPO_ROOT>' },
        [pscustomobject]@{ Value = [Environment]::GetFolderPath('UserProfile'); Replacement = '<USER_PROFILE>' },
        [pscustomobject]@{ Value = [System.IO.Path]::GetTempPath().TrimEnd('\', '/'); Replacement = '<TEMP_ROOT>' }
    )
    foreach ($replacement in $replacements) {
        if (-not [string]::IsNullOrWhiteSpace($replacement.Value)) {
            $text = $text.Replace($replacement.Value, $replacement.Replacement, [System.StringComparison]::OrdinalIgnoreCase)
        }
    }

    $text = [regex]::Replace(
        $text,
        '(?i)(password|pwd|token|client[_-]?secret|authorization)\s*[:=]\s*[^\s;]+',
        '$1=<REDACTED>'
    )
    return $text
}

function Invoke-CheckedCommand {
    param(
        [Parameter(Mandatory)]
        [scriptblock]$Command,
        [Parameter(Mandatory)]
        [string]$FailureCode
    )

    $output = @(& $Command 2>&1)
    $exitCode = $LASTEXITCODE
    $output
    if ($exitCode -ne 0) {
        throw "$FailureCode;EXIT=$exitCode"
    }
}

function Invoke-PowerShellGate {
    param(
        [Parameter(Mandatory)]
        [string]$Script,
        [object[]]$Arguments = @(),
        [Parameter(Mandatory)]
        [string]$FailureCode
    )

    Invoke-CheckedCommand -FailureCode $FailureCode -Command {
        & $Script @Arguments
    }
}

function Invoke-MtpLane {
    param(
        [Parameter(Mandatory)]
        [ValidateSet('Unit', 'Architecture', 'Contract')]
        [string]$Lane
    )

    $laneRoot = Join-Path $repositoryRoot ("tests\{0}" -f $Lane)
    $projects = @()
    if (Test-Path -LiteralPath $laneRoot -PathType Container) {
        $projects = @(Get-ChildItem -LiteralPath $laneRoot -Filter '*.csproj' -File -Recurse | Sort-Object FullName)
    }
    if ($projects.Count -eq 0) {
        throw "${Lane}_TEST_ERROR=no_test_projects"
    }

    $laneResultsRoot = Join-Path $EvidenceRoot ("test-results\{0}" -f $Lane.ToLowerInvariant())
    New-Item -ItemType Directory -Path $laneResultsRoot -Force | Out-Null

    foreach ($project in $projects) {
        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project.Name)
        $candidateRoot = Join-Path $project.Directory.FullName ("bin\{0}" -f $Configuration)
        $applications = @()
        if (Test-Path -LiteralPath $candidateRoot -PathType Container) {
            $applications = @(Get-ChildItem -LiteralPath $candidateRoot -Filter "$projectName.dll" -File -Recurse | Where-Object {
                $_.FullName -notmatch '[\\/](?:ref|refint)[\\/]'
            })
        }
        if ($applications.Count -ne 1) {
            throw "${Lane}_TEST_ERROR=test_application_count_$($applications.Count)"
        }

        $projectResults = Join-Path $laneResultsRoot $projectName
        New-Item -ItemType Directory -Path $projectResults -Force | Out-Null
        Invoke-CheckedCommand -FailureCode "${Lane}_TEST_ERROR=runner_failed" -Command {
            & dotnet $applications[0].FullName `
                --report-trx `
                --report-trx-filename "$projectName.trx" `
                --results-directory $projectResults `
                --minimum-expected-tests 1 `
                --zero-tests-policy strict `
                --output Minimal `
                --show-stdout None `
                --show-stderr Failed `
                --no-ansi `
                --progress off
        }

        Invoke-PowerShellGate `
            -Script (Join-Path $repositoryRoot 'eng\test\Assert-NonZeroTestResults.ps1') `
            -Arguments @('-ResultsDirectory', $projectResults) `
            -FailureCode "${Lane}_TEST_ERROR=zero_or_failed_test_guard"
    }
}

$stageNames = @(
    'Restore',
    'Build',
    'Unit',
    'Architecture',
    'Contract',
    'Dependency Audit',
    'Secret Scan',
    'Static Analysis',
    'Safe Reports'
)

$stageRecords = [System.Collections.Generic.List[object]]::new()
foreach ($name in $stageNames) {
    $stageRecords.Add([ordered]@{
        name = $name
        status = 'NOT_RUN'
        exitCode = $null
        startedAtUtc = $null
        completedAtUtc = $null
    })
}

function Write-PipelineSummary {
    param(
        [Parameter(Mandatory)]
        [string]$Commit,
        [Parameter(Mandatory)]
        [ValidateSet('RUNNING', 'PASS', 'FAIL', 'CANARY_ONLY')]
        [string]$Status
    )

    $summary = [ordered]@{
        schemaVersion = 1
        runId = $RunId
        commit = $Commit
        taskIds = @('BOOT-007', 'BOOT-008')
        decisionIds = @('LOCAL-BOOT-001', 'CCR-015')
        mode = $(if ($EngineSelfTest) { 'ENGINE_SELF_TEST' } else { 'PR_VALIDATION' })
        status = $Status
        stages = @($stageRecords)
    }
    $summary | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $EvidenceRoot 'pipeline-summary.json') -Encoding utf8NoBOM
}

try {
    New-Item -ItemType Directory -Path $EvidenceRoot -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $EvidenceRoot 'logs') -Force | Out-Null

    $commit = (& git -C $repositoryRoot rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or $commit -notmatch '^[0-9a-fA-F]{40}$') {
        throw 'PIPELINE_PREFLIGHT_ERROR=commit_unavailable'
    }

    if (-not $EngineSelfTest) {
        $requiredPaths = @(
            'global.json',
            'NuGet.Config',
            'Pertamina.Oto.sln',
            'eng\test\Assert-NonZeroTestResults.ps1',
            'eng\security\Invoke-DependencyAudit.ps1',
            'eng\security\Invoke-SecretScan.ps1',
            'eng\security\Invoke-StaticChecks.ps1',
            'eng\baseline\Assert-ApprovedDependencies.ps1',
            'eng\baseline\Assert-Phase0Scope.ps1',
            'eng\evidence\New-EvidenceManifest.ps1'
        )
        foreach ($requiredPath in $requiredPaths) {
            if (-not (Test-Path -LiteralPath (Join-Path $repositoryRoot $requiredPath))) {
                throw 'PIPELINE_PREFLIGHT_ERROR=integrated_layout_incomplete'
            }
        }

        $actualSdk = (& dotnet --version).Trim()
        if ($LASTEXITCODE -ne 0 -or $actualSdk -cne '10.0.400') {
            throw 'PIPELINE_PREFLIGHT_ERROR=sdk_mismatch'
        }

        & git -C $repositoryRoot diff --quiet
        if ($LASTEXITCODE -ne 0) {
            throw 'PIPELINE_PREFLIGHT_ERROR=tracked_worktree_changes'
        }
        & git -C $repositoryRoot diff --cached --quiet
        if ($LASTEXITCODE -ne 0) {
            throw 'PIPELINE_PREFLIGHT_ERROR=staged_worktree_changes'
        }
    }

    Write-PipelineSummary -Commit $commit -Status 'RUNNING'

    for ($index = 0; $index -lt $stageNames.Count; $index++) {
        $stage = $stageRecords[$index]
        $stage.status = 'RUNNING'
        $stage.startedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
        Write-PipelineSummary -Commit $commit -Status 'RUNNING'
        $stageOutput = @()

        try {
            if ($EngineSelfTest) {
                $stageOutput += "PIPELINE_ENGINE_SELF_TEST_STAGE=$($stage.name)"
                if ($stage.name -ceq $ControlledCanaryStage) {
                    throw 'PIPELINE_CONTROLLED_NEGATIVE_CANARY=BLOCKED'
                }
            }
            else {
                switch ($stage.name) {
                    'Restore' {
                        $stageOutput += Invoke-CheckedCommand -FailureCode 'RESTORE_RESULT=FAIL' -Command {
                            & dotnet restore (Join-Path $repositoryRoot 'Pertamina.Oto.sln') `
                                --configfile (Join-Path $repositoryRoot 'NuGet.Config') `
                                --locked-mode `
                                --nologo
                        }
                    }
                    'Build' {
                        $stageOutput += Invoke-CheckedCommand -FailureCode 'BUILD_RESULT=FAIL' -Command {
                            & dotnet build (Join-Path $repositoryRoot 'Pertamina.Oto.sln') `
                                --configuration $Configuration `
                                --no-restore `
                                --nologo `
                                --verbosity minimal
                        }
                    }
                    'Unit' { $stageOutput += Invoke-MtpLane -Lane Unit }
                    'Architecture' { $stageOutput += Invoke-MtpLane -Lane Architecture }
                    'Contract' { $stageOutput += Invoke-MtpLane -Lane Contract }
                    'Dependency Audit' {
                        $stageOutput += Invoke-PowerShellGate `
                            -Script (Join-Path $repositoryRoot 'eng\baseline\Assert-ApprovedDependencies.ps1') `
                            -FailureCode 'DEPENDENCY_POLICY_STAGE=FAIL'
                        $stageOutput += Invoke-PowerShellGate `
                            -Script (Join-Path $repositoryRoot 'eng\baseline\Assert-Phase0Scope.ps1') `
                            -FailureCode 'PHASE0_SCOPE_STAGE=FAIL'
                        $stageOutput += Invoke-PowerShellGate `
                            -Script (Join-Path $repositoryRoot 'eng\security\Invoke-DependencyAudit.ps1') `
                            -Arguments @('-Target', (Join-Path $repositoryRoot 'Pertamina.Oto.sln'), '-NoRestore') `
                            -FailureCode 'DEPENDENCY_AUDIT_STAGE=FAIL'
                    }
                    'Secret Scan' {
                        $stageOutput += Invoke-PowerShellGate `
                            -Script (Join-Path $repositoryRoot 'eng\security\Invoke-SecretScan.ps1') `
                            -Arguments @('-Root', $repositoryRoot) `
                            -FailureCode 'SECRET_SCAN_STAGE=FAIL'
                    }
                    'Static Analysis' {
                        $stageOutput += Invoke-PowerShellGate `
                            -Script (Join-Path $repositoryRoot 'eng\security\Invoke-StaticChecks.ps1') `
                            -Arguments @('-Root', $repositoryRoot) `
                            -FailureCode 'STATIC_CHECK_STAGE=FAIL'
                        $stageOutput += Invoke-CheckedCommand -FailureCode 'FORMAT_CHECK_STAGE=FAIL' -Command {
                            & dotnet format (Join-Path $repositoryRoot 'Pertamina.Oto.sln') `
                                --verify-no-changes `
                                --no-restore `
                                --verbosity minimal
                        }
                    }
                    'Safe Reports' {
                        $stageOutput += Invoke-PowerShellGate `
                            -Script (Join-Path $repositoryRoot 'eng\evidence\New-EvidenceManifest.ps1') `
                            -Arguments @('-RunId', $RunId, '-Commit', $commit, '-EvidenceRoot', $EvidenceRoot, '-Status', 'PASS') `
                            -FailureCode 'SAFE_REPORT_STAGE=FAIL'
                    }
                }
            }

            $stage.status = 'PASS'
            $stage.exitCode = 0
        }
        catch {
            $stageOutput += $_.Exception.Message
            $stage.status = 'FAIL'
            $stage.exitCode = 1
            throw
        }
        finally {
            $stage.completedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
            $safeText = ConvertTo-SafeLogText -Items $stageOutput
            $logName = '{0:D2}-{1}.log' -f ($index + 1), ($stage.name.ToLowerInvariant() -replace '[^a-z0-9]+', '-')
            Set-Content -LiteralPath (Join-Path $EvidenceRoot "logs\$logName") -Value $safeText -Encoding utf8NoBOM
            Write-PipelineSummary -Commit $commit -Status $(if ($stage.status -eq 'FAIL') { 'FAIL' } else { 'RUNNING' })
        }
    }

    $finalStatus = $(if ($EngineSelfTest) { 'CANARY_ONLY' } else { 'PASS' })
    Write-PipelineSummary -Commit $commit -Status $finalStatus

    if (-not $EngineSelfTest) {
        Invoke-PowerShellGate `
            -Script (Join-Path $repositoryRoot 'eng\evidence\New-EvidenceManifest.ps1') `
            -Arguments @('-RunId', $RunId, '-Commit', $commit, '-EvidenceRoot', $EvidenceRoot, '-Status', 'PASS') `
            -FailureCode 'FINAL_EVIDENCE_MANIFEST=FAIL'
    }

    Write-Output "PR_VALIDATION_RUN_ID=$RunId"
    Write-Output "PR_VALIDATION_COMMIT=$commit"
    Write-Output "PR_VALIDATION_STATUS=$finalStatus"
    exit 0
}
catch {
    $message = ConvertTo-SafeLogText -Items @($_.Exception.Message)
    Write-Error "PR_VALIDATION_STATUS=FAIL;$message"
    exit 1
}
