param([Parameter(Mandatory)][string]$EvidenceRoot)
. (Join-Path $PSScriptRoot '../eng/evidence/Phase1-Evidence.ps1')
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe = Join-Path $PSHOME 'pwsh.exe'
if (Test-Path $EvidenceRoot) { throw 'new_test_root_required' }
New-Item -ItemType Directory $EvidenceRoot | Out-Null
$report = [Collections.Generic.List[string]]::new()
function Check([bool]$Condition,[string]$Label) { if (-not $Condition) { throw $Label }; $report.Add("${Label}=PASS") }
foreach ($file in @('pipelines/Invoke-Phase1Validation.ps1','eng/evidence/Phase1-Evidence.ps1','eng/baseline/Assert-Phase1Scope.ps1')) {
    $tokens=$null; $errors=$null
    $null=[Management.Automation.Language.Parser]::ParseFile((Join-Path $root $file),[ref]$tokens,[ref]$errors)
    Check ($errors.Count -eq 0) ('PARSER_' + [IO.Path]::GetFileNameWithoutExtension($file))
}
& $exe -NoProfile -File (Join-Path $root 'eng/baseline/Assert-Phase1Scope.ps1') *> $null
Check ($LASTEXITCODE -eq 0) 'SCOPE_POSITIVE'
foreach ($stage in @('Scope','Restore','Build','Unit','Architecture','Contract','Security','UI','Audit')) {
    $out = Join-Path $EvidenceRoot $stage
    & $exe -NoProfile -File (Join-Path $root 'pipelines/Invoke-Phase1Validation.ps1') -RunId canary -EvidenceRoot $out -EngineSelfTest -ControlledCanaryStage $stage *> $null
    Check ($LASTEXITCODE -ne 0) "BLOCK_$stage"
    $s=Get-Content -Raw (Join-Path $out 'pipeline-summary.json') | ConvertFrom-Json
    $failed=$false
    foreach ($r in $s.stages) { if ($failed) { Check ($r.status -eq 'NOT_RUN') "DOWNSTREAM_$($stage)_$($r.name)" }; if ($r.name -eq $stage) { Check ($r.status -eq 'FAIL') "FAILED_$stage"; $failed=$true } }
}
$positive=Join-Path $EvidenceRoot 'positive'
& $exe -NoProfile -File (Join-Path $root 'pipelines/Invoke-Phase1Validation.ps1') -RunId positive -EvidenceRoot $positive -EngineSelfTest *> $null
Check ($LASTEXITCODE -eq 0) 'ENGINE_POSITIVE'
$before=(Get-FileHash (Join-Path $positive 'pipeline-summary.json')).Hash
& $exe -NoProfile -File (Join-Path $root 'pipelines/Invoke-Phase1Validation.ps1') -RunId positive -EvidenceRoot $positive -EngineSelfTest *> $null
Check ($LASTEXITCODE -ne 0) 'EXISTING_ROOT_BLOCKED'
Check ((Get-FileHash (Join-Path $positive 'pipeline-summary.json')).Hash -ceq $before) 'EXISTING_EVIDENCE_PRESERVED'
$commit=(& git -C $root rev-parse HEAD).Trim()
Assert-Phase1Manifest $positive positive $commit
$hash=(Get-FileHash (Join-Path $positive 'manifest.json')).Hash
Write-Phase1Manifest $positive
Check ((Get-FileHash (Join-Path $positive 'manifest.json')).Hash -ceq $hash) 'MANIFEST_DETERMINISTIC'
Add-Content (Join-Path $positive 'pipeline-summary.json') ' '
$blocked=$false; try { Assert-Phase1Manifest $positive positive $commit } catch { $blocked=$true }
Check $blocked 'MANIFEST_TAMPER_BLOCKED'
foreach ($count in @(0,1)) {
    $path=Join-Path $EvidenceRoot "count-$count.trx"
    "<TestRun><ResultSummary><Counters total=`"$count`" executed=`"$count`" passed=`"$count`" failed=`"0`" error=`"0`" aborted=`"0`" /></ResultSummary><Output>private-canary-content</Output></TestRun>" | Set-Content $path
    $blocked=$false; try { Export-SafeTrx $path } catch { $blocked=$true }
    Check ($blocked -eq ($count -eq 0)) "PER_PROJECT_COUNT_$count"
    Check ((Get-Content -Raw $path) -notmatch 'private-canary-content') "SANITIZE_$count"
}
$fixture=Join-Path $EvidenceRoot 'scope-fixture'
New-Item -ItemType Directory (Join-Path $fixture 'src/Hosts/ForbiddenServer') | Out-Null
'x' | Set-Content (Join-Path $fixture 'src/Hosts/ForbiddenServer/Forbidden.cs')
& $exe -NoProfile -File (Join-Path $root 'eng/baseline/Assert-Phase1Scope.ps1') -RepositoryRoot $fixture *> $null
Check ($LASTEXITCODE -ne 0) 'SCOPE_NEGATIVE'
$report | Set-Content (Join-Path $EvidenceRoot 'self-test-results.log')
$report
'PHASE1_SELF_TEST_RESULT=PASS'
