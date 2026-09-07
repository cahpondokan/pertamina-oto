$ErrorActionPreference='Stop'
Import-Module (Join-Path $PSScriptRoot 'Environment.psm1') -Force
$count=0
$r=ConvertTo-SafeRecord AB EXITED 0 1 ('A'*64) | ConvertFrom-Json
if($r.correctness -cne 'NOT_EVALUATED') { throw 'ASSERT' }; $count++
foreach($args in @(
    @('bad','EXITED',0,1,('A'*64)),
    @('AB','PASS',0,1,('A'*64)),
    @('AB','EXITED',0,-1,('A'*64)),
    @('AB','EXITED',0,1,'secret-path')
)) {
    $reject=$false
    try { ConvertTo-SafeRecord @args | Out-Null } catch { $reject=$true }
    if(-not $reject) { throw 'REJECTION_FAILED' }; $count++
}
foreach($file in @('Environment.psm1','Invoke-GuestChild.ps1','Test-Staging.ps1')) {
    $tokens=$null; $errors=$null
    $null=[Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot $file),[ref]$tokens,[ref]$errors)
    if($errors.Count) { throw 'SYNTAX' }; $count++
}
Write-Output "DISCOVERED=$count PASSED=$count FAILED=0; CHILD_EXECUTED=false; PDF_GENERATED=false"
