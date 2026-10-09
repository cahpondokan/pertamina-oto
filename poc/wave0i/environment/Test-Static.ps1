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
$info=[Diagnostics.ProcessStartInfo]::new()
$info.EnvironmentVariables['DOTNET_STARTUP_HOOKS']='canary'
$info.EnvironmentVariables['COR_ENABLE_PROFILING']='1'
Set-CleanChildEnvironment $info 'C:\PocPrivate\AB'
if($info.EnvironmentVariables.Count -ne 8 -or $info.EnvironmentVariables.ContainsKey('DOTNET_STARTUP_HOOKS') -or $info.EnvironmentVariables['DOTNET_ROOT'] -cne 'C:\PocInput\dotnet' -or $info.EnvironmentVariables['TEMP'] -cne 'C:\PocPrivate\AB' -or $info.EnvironmentVariables['DOTNET_ROLL_FORWARD'] -cne 'Disable') { throw 'ENVIRONMENT_FAILED' }; $count++
$bundle=[pscustomobject]@{schemaVersion=1;status='BUILT_NOT_EXECUTED';engineExecuted=$false;entryPoint='dotnet/dotnet.exe';runner='app/MergeSmoke.dll';inputLockSha256=('A'*64);files=@([pscustomobject]@{path='dotnet/dotnet.exe';sha256=('B'*64)},[pscustomobject]@{path='app/MergeSmoke.dll';sha256=('C'*64)})}
$supplement=[pscustomobject]@{schema=1;files=@([pscustomobject]@{path='environment/Environment.psm1';sha256=('D'*64)})}
$combined=ConvertTo-StagingManifest $bundle $supplement | ConvertFrom-Json
if($combined.schema -ne 1 -or $combined.files.Count -ne 3) { throw 'ADAPTER_FAILED' }; $count++
$reject=$false
try { ConvertTo-StagingManifest $bundle ([pscustomobject]@{schema=1;files=$bundle.files}) | Out-Null } catch { $reject=$true }
if(-not $reject) { throw 'DUPLICATE_ACCEPTED' }; $count++
$bundle.schemaVersion=2; $reject=$false
try { ConvertTo-StagingManifest $bundle $supplement | Out-Null } catch { $reject=$true }
if(-not $reject) { throw 'SCHEMA_ACCEPTED' }; $count++
Write-Output "DISCOVERED=$count PASSED=$count FAILED=0; CHILD_EXECUTED=false; PDF_GENERATED=false"
