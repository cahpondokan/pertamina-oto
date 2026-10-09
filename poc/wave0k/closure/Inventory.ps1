# Static inventory only; no Python, qpdf or PDF execution.
[CmdletBinding()] param([switch]$WriteLock)
$ErrorActionPreference='Stop'
$archive='D:\OTO\pertamina-oto\artifacts\evidence\P02-20260905-1434\agent-3-qpdf-intake\qpdf-12.4.1-msvc64.zip'
$expected='3CD016CD433EF7232E42F4C13348A49CC14907A3C7278EF4F99120593126F7A6'
if((Get-FileHash -LiteralPath $archive).Hash -ne $expected){throw 'ARCHIVE_DRIFT'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip=[IO.Compression.ZipFile]::OpenRead($archive)
try {
 $qpdf=@(foreach($entry in $zip.Entries){
  if($entry.FullName -match '^qpdf-12\.4\.1-msvc64/bin/(qpdf\.exe|[^/]+\.dll)$' -or $entry.FullName -match '/(license\.rst\.txt|license\.html)$') {
   $stream=$entry.Open(); $hasher=[Security.Cryptography.SHA256]::Create()
   try {$hash=[BitConverter]::ToString($hasher.ComputeHash($stream)).Replace('-','').ToLowerInvariant()} finally {$stream.Dispose();$hasher.Dispose()}
   $target=if($entry.FullName -match '/bin/'){'qpdf/'+$entry.Name}else{'notices/qpdf/'+$entry.Name}
   [ordered]@{member=$entry.FullName;target=$target;bytes=$entry.Length;sha256=$hash}
  }
 })
} finally {$zip.Dispose()}
$root='C:\Python314'
$relative=@('python.exe','python3.dll','python314.dll','vcruntime140.dll','vcruntime140_1.dll','LICENSE.txt','DLLs\_hashlib.pyd','DLLs\libcrypto-3.dll','DLLs\select.pyd','DLLs\unicodedata.pyd')
$relative+=@(Get-ChildItem -LiteralPath "$root\Lib" -Recurse -File -Filter '*.py' | Where-Object {
 $_.FullName -notmatch '\\(site-packages|__pycache__|test|tests|idlelib|tkinter|turtledemo|ensurepip|venv)\\'
} | ForEach-Object {$_.FullName.Substring($root.Length+1)})
$python=@(foreach($rel in ($relative|Sort-Object -Unique)){
 $path=Join-Path $root $rel
 $item=Get-Item -LiteralPath $path
 for($cursor=$item;$null -ne $cursor;$cursor=if($cursor -is [IO.DirectoryInfo]){$cursor.Parent}else{$cursor.Directory}) {
  if(($cursor.Attributes -band [IO.FileAttributes]::ReparsePoint)-ne 0){throw 'REPARSE_INPUT'}
 }
 $target=if($rel -eq 'LICENSE.txt'){'notices/python/LICENSE.txt'}else{'python/'+$rel.Replace('\','/')}
 [ordered]@{source=$path;target=$target;bytes=$item.Length;sha256=(Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant()}
})
if(@($qpdf|Where-Object {$_.target -like 'qpdf/*'}).Count -ne 10){throw 'QPDF_MEMBER_COUNT'}
if(@($python.target|Sort-Object -Unique).Count -ne $python.Count){throw 'DUPLICATE_TARGET'}
if(@($python|Where-Object {$_.target -match '(site-packages|__pycache__|\.pyc$)'}).Count){throw 'EXCLUDED_MEMBER'}
$result=[ordered]@{schema=1;status='STATIC_CANDIDATE_NOT_RUNTIME_PROVEN';archive=$archive;archiveSha256=$expected.ToLowerInvariant();pythonVersionResource=(Get-Item "$root\python.exe").VersionInfo.FileVersion;pythonInvocation=@('-I','-S','-B');qpdf=$qpdf;python=$python}
$json=$result|ConvertTo-Json -Depth 8
if($WriteLock){
 $destination=Join-Path $PSScriptRoot 'inputs.lock.json'
 $stream=[IO.File]::Open($destination,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
 try {$bytes=[Text.UTF8Encoding]::new($false).GetBytes($json);$stream.Write($bytes,0,$bytes.Length)} finally {$stream.Dispose()}
}
[ordered]@{pythonFiles=$python.Count;pythonBytes=($python.bytes|Measure-Object -Sum).Sum;qpdfFiles=$qpdf.Count;qpdfBytes=($qpdf.bytes|Measure-Object -Sum).Sum;validation='3 checks PASS';noCandidateExecution=$true}|ConvertTo-Json
