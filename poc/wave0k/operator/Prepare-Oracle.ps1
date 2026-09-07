#requires -Version 7.0
# Existing bytes only. No engine, Python, Sandbox or network execution.
$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath("$PSScriptRoot/../../..")
$run='C:\Users\cahpo\AppData\Local\Temp\OTO-W0K-Oracle-001'
$old='C:\Users\cahpo\AppData\Local\Temp\OTO-W0J-Smoke-001'
$lockPath=Join-Path $repo 'poc/wave0k/closure/inputs.lock.json'
# Integrated LF copy; original Agent2 CRLF lock0A2F34BF... remains historical.
# Normalized text equality independently checked before pinning this raw hash.
if((Get-FileHash $lockPath).Hash -ne 'CF3EA447F666D3ADA7D3B42E95414327B476271769803D8D492BFAB672767FA4'){throw 'LOCK_DRIFT'}
$lock=Get-Content $lockPath -Raw|ConvertFrom-Json
if((Get-FileHash $lock.archive).Hash -ine $lock.archiveSha256){throw 'ARCHIVE_DRIFT'}
$oldManifest=Join-Path $old 'manifest.json'
if((Get-FileHash $oldManifest).Hash -ne '4D620CD59E18A6AE6E51B403E0CC84F3024417C5E7B884B80274DDC11C4B4F3C'){throw 'BASELINE_MANIFEST_DRIFT'}
$baseline=Get-Content $oldManifest -Raw|ConvertFrom-Json
Import-Module (Join-Path $repo 'poc/wave0i/environment/Environment.psm1') -Force
$null=Assert-PrivateDirectory (Join-Path $old 'input')
foreach($entry in $baseline.files){
 $p=Join-Path $old ('input/'+$entry.path)
 if((Get-FileHash $p).Hash -ne $entry.sha256 -or (Get-Item $p).Length -ne $entry.bytes){throw 'BASELINE_DRIFT'}
}
if(Test-Path -LiteralPath $run){throw 'RUN_EXISTS_DO_NOT_REUSE'}
$null=New-Item -ItemType Directory -Path $run
$acl=[Security.AccessControl.DirectorySecurity]::new();$acl.SetAccessRuleProtection($true,$false)
$user=[Security.Principal.WindowsIdentity]::GetCurrent().User
$sids=@($user,[Security.Principal.SecurityIdentifier]::new('S-1-5-18'),[Security.Principal.SecurityIdentifier]::new('S-1-5-32-544'))
foreach($sid in $sids){$acl.AddAccessRule([Security.AccessControl.FileSystemAccessRule]::new($sid,'FullControl','ContainerInherit,ObjectInherit','None','Allow'))}
Set-Acl -LiteralPath $run -AclObject $acl
$actual=Get-Acl -LiteralPath $run
if(-not $actual.AreAccessRulesProtected -or @($actual.Access).Count -ne 3){throw 'PRIVATE_ACL_FAILED'}
foreach($rule in $actual.Access){if($rule.IdentityReference.Translate([Security.Principal.SecurityIdentifier]).Value -notin @($sids.Value) -or $rule.AccessControlType -ne 'Allow'){throw 'PRIVATE_ACL_FAILED'}}
$stage=Join-Path $run 'input';$null=New-Item -ItemType Directory -Path $stage
function New-Target([string]$Name){
 if($Name -cnotmatch '^[A-Za-z0-9_.-]+(/[A-Za-z0-9_.-]+)*$' -or $Name.Contains('..')){throw 'UNSAFE_TARGET'}
 $target=Join-Path $stage $Name
 if(Test-Path -LiteralPath $target){throw 'DUPLICATE_TARGET'}
 $null=[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($target))
 return $target
}
foreach($entry in $baseline.files){[IO.File]::Copy((Join-Path $old ('input/'+$entry.path)),(New-Target $entry.path),$false)}
foreach($entry in $lock.python){
 if((Get-FileHash $entry.source).Hash -ine $entry.sha256 -or (Get-Item $entry.source).Length -ne $entry.bytes){throw 'PYTHON_INPUT_DRIFT'}
 [IO.File]::Copy($entry.source,(New-Target $entry.target),$false)
}
$zip=[IO.Compression.ZipFile]::OpenRead($lock.archive)
try{foreach($entry in $lock.qpdf){
 $member=$zip.GetEntry($entry.member)
 if($null -eq $member -or $member.Length -ne $entry.bytes){throw 'ARCHIVE_MEMBER_DRIFT'}
 $target=New-Target $entry.target
 $inputStream=$member.Open();$outputStream=[IO.File]::Open($target,[IO.FileMode]::CreateNew)
 try{$inputStream.CopyTo($outputStream)}finally{$inputStream.Dispose();$outputStream.Dispose()}
 if((Get-FileHash $target).Hash -ine $entry.sha256){throw 'EXTRACT_HASH_DRIFT'}
}}finally{$zip.Dispose()}
foreach($entry in @($lock.python)+@($lock.qpdf)){
 $p=Join-Path $stage $entry.target
 if((Get-FileHash $p).Hash -ine $entry.sha256 -or (Get-Item $p).Length -ne $entry.bytes){throw 'STAGE_DRIFT'}
}
$observer=Join-Path $repo 'poc/wave0i/oracle/observe.py'
[IO.File]::Copy($observer,(New-Target 'oracle/observe.py'),$false)
$qpdfFiles=[ordered]@{}
foreach($entry in $lock.qpdf){if($entry.target.StartsWith('qpdf/')){$qpdfFiles[[IO.Path]::GetFileName($entry.target)]=$entry.sha256}}
$qpdfFiles|ConvertTo-Json|Set-Content -LiteralPath (New-Target 'qpdf-files.json') -Encoding utf8NoBOM
$sources=Get-Content (Join-Path $stage 'corpus/sources.json') -Raw|ConvertFrom-Json
$sourceHashes=[ordered]@{};foreach($name in @('A','B','C')){$sourceHashes[$name]=$sources.$name.sha256}
[ordered]@{schema=1;observerScriptSha256=(Get-FileHash $observer).Hash.ToLowerInvariant();sourceSha256=$sourceHashes;qpdfFiles=$qpdfFiles}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $run 'expected.json') -Encoding utf8NoBOM
$null=Assert-PrivateDirectory $stage
$files=@(Get-ChildItem $stage -File -Recurse -Force|Sort-Object FullName|ForEach-Object{[ordered]@{path=[IO.Path]::GetRelativePath($stage,$_.FullName).Replace('\','/');bytes=$_.Length;sha256=(Get-FileHash $_.FullName).Hash}})
if($files.Count -ne (219+@($lock.python).Count+@($lock.qpdf).Count+2)){throw 'STAGE_COUNT'}
$manifest=Join-Path $run 'manifest.json'
[ordered]@{schema=1;files=$files}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath $manifest -Encoding utf8NoBOM
[ordered]@{status='PREPARED_NOT_EXECUTED';files=$files.Count;manifestSha256=(Get-FileHash $manifest).Hash;expectedSha256=(Get-FileHash (Join-Path $run 'expected.json')).Hash;privateAclVerified=$true;engineExecuted=$false}|ConvertTo-Json
