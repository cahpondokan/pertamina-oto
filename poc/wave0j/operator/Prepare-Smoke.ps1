#requires -Version 7.0
# Fixed synthetic-only preparation; no engine or Sandbox execution.
$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath("$PSScriptRoot/../../..")
$run='C:\Users\cahpo\AppData\Local\Temp\OTO-W0J-Smoke-001'
$old='C:\Users\cahpo\AppData\Local\Temp\OTO-W0I-Noargs-001'
Import-Module (Join-Path $repo 'poc/wave0i/environment/Environment.psm1') -Force
& (Join-Path $repo 'poc/wave0i/operator/Invoke-Noargs.ps1')
if(Test-Path -LiteralPath $run){throw 'RUN_ALREADY_EXISTS_DO_NOT_REUSE'}
$generator=Join-Path $repo 'poc/wave0h/corpus/generate.py'
if((Get-FileHash $generator).Hash -ne '337CDF0F7D3546572572AE0AEEE583639F43F6CDCC3F6426834DD26CA6DCA5E9'){throw 'GENERATOR_DRIFT'}
$null=New-Item -ItemType Directory -Path $run
# Restrict this new run-owned directory only, before placing any payload in it.
$acl=[Security.AccessControl.DirectorySecurity]::new()
$acl.SetAccessRuleProtection($true,$false)
$identity=[Security.Principal.WindowsIdentity]::GetCurrent().User
foreach($sid in @($identity,[Security.Principal.SecurityIdentifier]::new('S-1-5-18'),[Security.Principal.SecurityIdentifier]::new('S-1-5-32-544'))){
    $rule=[Security.AccessControl.FileSystemAccessRule]::new($sid,'FullControl','ContainerInherit,ObjectInherit','None','Allow')
    $acl.AddAccessRule($rule)
}
Set-Acl -LiteralPath $run -AclObject $acl
$stage=Join-Path $run 'input'
$null=New-Item -ItemType Directory -Path $stage
foreach($item in Get-ChildItem -LiteralPath $old -Force){Copy-Item -LiteralPath $item.FullName -Destination $stage -Recurse -ErrorAction Stop}
& 'C:\Python314\python.exe' -B $generator (Join-Path $stage 'corpus')
if($LASTEXITCODE -ne 0){throw 'GENERATOR_FAILED_RETAIN_PARTIAL'}
$expected=@{
 A=@(850,'F51921C178104BCDA76C6C792AA511DA97665A0D4E8BEB49CD833B722FC72328')
 B=@(1113,'DEB64F3196F4DDF92F18348EA81F8E9D40A3562F0023AC39C43EEBD5E4AFEF81')
 C=@(589,'B45A53A74BC04219E4FFB9574A53F0A9D8EEFAF03E20753C18CA06AB3FD8AABC')
}
foreach($name in @('A','B','C')){
 $path=Join-Path $stage "corpus/$name.pdf"
 if((Get-Item $path).Length -ne $expected[$name][0] -or (Get-FileHash $path).Hash -ne $expected[$name][1]){throw 'CORPUS_MISMATCH'}
}
$bundle=Get-Content (Join-Path $repo 'artifacts/evidence/P02-W0I/build-001/bundle.manifest.json') -Raw|ConvertFrom-Json
foreach($entry in $bundle.files){
 $path=Join-Path $stage $entry.path
 if((Get-FileHash $path).Hash -ne $entry.sha256 -or (Get-Item $path).Length -ne $entry.bytes){throw 'COPIED_BUNDLE_DRIFT'}
}
$null=Assert-PrivateDirectory $stage
$files=@(Get-ChildItem $stage -File -Recurse -Force|Sort-Object FullName|ForEach-Object {
 [ordered]@{path=[IO.Path]::GetRelativePath($stage,$_.FullName).Replace('\','/');bytes=$_.Length;sha256=(Get-FileHash $_.FullName).Hash}
})
if($files.Count -ne 219){throw 'INVENTORY_MISMATCH'}
$manifest=Join-Path $run 'manifest.json'
[ordered]@{schema=1;purpose='ENGINE_SMOKE_NOT_ORACLE_VALIDATED';files=$files}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath $manifest -Encoding utf8NoBOM
[ordered]@{status='PREPARED_NOT_EXECUTED';fileCount=$files.Count;manifestSha256=(Get-FileHash $manifest).Hash;corpusGenerated=$true;engineExecuted=$false}|ConvertTo-Json
