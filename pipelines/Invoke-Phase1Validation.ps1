[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidatePattern('^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$')][string]$RunId,
    [Parameter(Mandatory)][string]$EvidenceRoot,
    [ValidateSet('Debug','Release')][string]$Configuration='Release',
    [switch]$EngineSelfTest,
    [ValidateSet('','Scope','Restore','Build','Unit','Architecture','Contract','Security','UI','Audit')][string]$ControlledCanaryStage=''
)
. (Join-Path $PSScriptRoot '../eng/evidence/Phase1-Evidence.ps1')
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$EvidenceRoot = [IO.Path]::GetFullPath($EvidenceRoot)
$hostExe = Join-Path $PSHOME 'pwsh.exe'
$records = [Collections.Generic.List[object]]::new()
$commit = (& git -C $root rev-parse HEAD).Trim()
$summary = [ordered]@{ schemaVersion=2; runId=$RunId; commit=$commit; taskIds=@('P1-A7-CI-EVIDENCE'); decisionIds=@('P0-D05','P0-D06','LOCAL-BOOT-001','CCR-015'); status='RUNNING'; stages=$records }
function Save-Summary { $summary | ConvertTo-Json -Depth 12 | Set-Content (Join-Path $EvidenceRoot 'pipeline-summary.json') -Encoding utf8NoBOM }
function Run-Command([scriptblock]$Command) {
    # Tool output is intentionally excluded: it may contain paths, secrets or test content.
    Push-Location $root
    try {
        & $Command *> $null
        if ($LASTEXITCODE -ne 0) { throw 'COMMAND_FAILED' }
    } finally { Pop-Location }
}
$createdEvidence = $false
try {
    if (Test-Path -LiteralPath $EvidenceRoot) { throw 'EVIDENCE_ROOT_MUST_BE_NEW' }
    New-Item -ItemType Directory $EvidenceRoot | Out-Null; $createdEvidence = $true
    if ($commit -notmatch '^[a-f0-9]{40}$') { throw 'COMMIT_INVALID' }
    if ($ControlledCanaryStage -and -not $EngineSelfTest) { throw 'CANARY_MODE_REQUIRED' }
    $projects = @()
    if (-not $EngineSelfTest) {
        if (@(& git -C $root status --porcelain).Count -ne 0) { throw 'WORKTREE_NOT_CLEAN' }
        Push-Location $root
        try { if ((& dotnet --version).Trim() -cne '10.0.400') { throw 'SDK_MISMATCH' } } finally { Pop-Location }
        $solution = Get-Content -Raw (Join-Path $root 'Pertamina.Oto.sln')
        $listed = @([regex]::Matches($solution,'"([^"\r\n]+\.csproj)"') | ForEach-Object { $_.Groups[1].Value.Replace('\','/') })
        $projects = @(Get-ChildItem (Join-Path $root 'tests') -Recurse -Filter *.csproj | Where-Object FullName -NotMatch '[\\/](bin|obj)[\\/]' | Sort-Object FullName)
        foreach ($p in $projects) {
            $rel = [IO.Path]::GetRelativePath($root,$p.FullName).Replace('\','/')
            if ($rel -cnotin $listed -or $rel.Split('/')[1] -notin @('Unit','Architecture','Contract','Security','UI')) { throw 'TEST_PROJECT_NOT_IN_SOLUTION_OR_LANE' }
        }
    }
    foreach ($stage in @('Scope','Restore','Build','Unit','Architecture','Contract','Security','UI','Audit')) { $records.Add([ordered]@{name=$stage;status='NOT_RUN';exitCode=$null}) }
    foreach ($record in $records) {
        $record.status='RUNNING'; Save-Summary
        try {
            if ($EngineSelfTest) { if ($record.name -ceq $ControlledCanaryStage) { throw 'CANARY_FAILURE' } }
            elseif ($record.name -eq 'Scope') {
                Run-Command { & $hostExe -NoProfile -File (Join-Path $root 'eng/baseline/Assert-Phase1Scope.ps1') }
                Run-Command { & $hostExe -NoProfile -File (Join-Path $root 'eng/baseline/Assert-ApprovedDependencies.ps1') }
            }
            elseif ($record.name -eq 'Restore') { Run-Command { & dotnet restore (Join-Path $root 'Pertamina.Oto.sln') --configfile (Join-Path $root 'NuGet.Config') --locked-mode } }
            elseif ($record.name -eq 'Build') { Run-Command { & dotnet build (Join-Path $root 'Pertamina.Oto.sln') -c $Configuration --no-restore --no-incremental -p:ContinuousIntegrationBuild=true } }
            elseif ($record.name -eq 'Audit') {
                Run-Command { & $hostExe -NoProfile -File (Join-Path $root 'eng/security/Invoke-DependencyAudit.ps1') -Target (Join-Path $root 'Pertamina.Oto.sln') -NoRestore }
                foreach ($script in @('Invoke-SecretScan.ps1','Invoke-StaticChecks.ps1')) {
                    Run-Command { & $hostExe -NoProfile -File (Join-Path $root "eng/security/$script") }
                }
                Run-Command { & dotnet format (Join-Path $root 'Pertamina.Oto.sln') --verify-no-changes --no-restore }
            }
            else {
                $lane = @($projects | Where-Object { [IO.Path]::GetRelativePath($root,$_.FullName).Split([IO.Path]::DirectorySeparatorChar)[1] -ceq $record.name })
                if ($lane.Count -eq 0) { throw 'REQUIRED_LANE_EMPTY' }
                foreach ($p in $lane) {
                    $id = [IO.Path]::GetRelativePath($root,$p.FullName).Replace('\','/').Replace('/','_').Replace('.csproj','')
                    $results = Join-Path $EvidenceRoot $id
                    New-Item -ItemType Directory $results | Out-Null
                    $apps = @(Get-ChildItem (Join-Path $p.Directory.FullName "bin/$Configuration") -Recurse -Filter "$($p.BaseName).dll" | Where-Object FullName -NotMatch '[\\/](ref|refint)[\\/]')
                    if ($apps.Count -ne 1) { throw 'TEST_APPLICATION_COUNT' }
                    try {
                        Run-Command { & dotnet $apps[0].FullName --report-trx --report-trx-filename result.trx --results-directory $results --minimum-expected-tests 1 --zero-tests-policy strict --output Minimal --no-ansi --progress off }
                    } finally {
                        $trx = @(Get-ChildItem $results -Filter *.trx)
                        if ($trx.Count -ne 1) { throw 'TRX_MISSING_OR_AMBIGUOUS' }
                        Export-SafeTrx $trx[0].FullName
                    }
                }
            }
            $record.status='PASS'; $record.exitCode=0
        } catch { $record.status='FAIL'; $record.exitCode=1; throw }
        finally { Save-Summary; "STAGE=$($record.name);STATUS=$($record.status);EXIT=$($record.exitCode)" | Set-Content (Join-Path $EvidenceRoot "$($record.name).log") -Encoding utf8NoBOM }
    }
    $summary.status = $(if ($EngineSelfTest) {'CANARY_ONLY'} else {'PASS'})
    Save-Summary
    Write-Phase1Manifest $EvidenceRoot
    Assert-Phase1Manifest $EvidenceRoot $RunId $commit
    'PHASE1_PIPELINE_RESULT=' + $summary.status
    exit 0
} catch {
    if ($createdEvidence) {
        $summary.status='FAIL'; Save-Summary
        # Remove any raw TRX that could not be projected safely; retain no arbitrary diagnostics.
        foreach ($trx in Get-ChildItem $EvidenceRoot -Recurse -Filter *.trx) {
            try { Export-SafeTrx $trx.FullName } catch { [IO.File]::WriteAllText($trx.FullName,'<TestRun status="REJECTED" />') }
        }
        Write-Phase1Manifest $EvidenceRoot
    }
    'PHASE1_PIPELINE_RESULT=FAIL'; exit 1
}
