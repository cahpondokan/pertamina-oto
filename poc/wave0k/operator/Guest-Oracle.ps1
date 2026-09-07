param(
 [Parameter(Mandatory)][ValidateSet('AB','CAB','AA','AC')][string]$CaseId,
 [Parameter(Mandatory)][ValidatePattern('^[a-fA-F0-9]{64}$')][string]$LedgerSha256,
 [Parameter(Mandatory)][ValidatePattern('^[a-fA-F0-9]{64}$')][string]$QpdfManifestSha256
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
function Assert-Plain([string]$Path,[bool]$Directory) {
 $item=Get-Item -LiteralPath $Path -Force
 if($item.PSIsContainer -ne $Directory -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw 'PATH_SHAPE' }
 $parent=$null
 if($Directory) { $parent=$item.Parent } else { $parent=$item.Directory }
 while($null -ne $parent) { if($parent.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'LINK_PARENT' }; $parent=$parent.Parent }
}
function Assert-Bound([long]$Length,[bool]$AllowEmpty) {
 if($Length -gt 1048576 -or $Length -lt 0 -or (-not $AllowEmpty -and $Length -eq 0)) { throw 'EXPORT_BOUND' }
}
function Assert-Sources($Ledger) {
 if((@($Ledger.PSObject.Properties.Name | Sort-Object) -join ',') -cne 'A,B,C') { throw 'SOURCE_SCHEMA' }
 foreach($name in @('A','B','C')) {
  $entry=$Ledger.$name; $path='C:\PocInput\corpus\'+$name+'.pdf'; Assert-Plain $path $false
  if($entry.sha256 -cnotmatch '^[a-f0-9]{64}$' -or $entry.bytes -isnot [int] -or $entry.bytes -le 0 -or $entry.bytes -gt 65536) { throw 'SOURCE_SCHEMA' }
  if((Get-Item -LiteralPath $path).Length -ne $entry.bytes -or (Get-FileHash -LiteralPath $path).Hash -ine $entry.sha256) { throw 'SOURCE_DRIFT' }
 }
}
function Write-NewJson([string]$Path,$Value) {
 $bytes=[Text.Encoding]::UTF8.GetBytes(($Value | ConvertTo-Json -Depth 10 -Compress))
 $stream=[IO.File]::Open($Path,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
 try { $stream.Write($bytes,0,$bytes.Length) } finally { $stream.Dispose() }
}
function Invoke-Child([string]$Exe,[string]$Arguments,[string]$Directory,[int]$Seconds) {
 $info=New-Object Diagnostics.ProcessStartInfo
 $info.FileName=$Exe; $info.Arguments=$Arguments; $info.WorkingDirectory=$Directory
 $info.UseShellExecute=$false; $info.CreateNoWindow=$true; $info.EnvironmentVariables.Clear()
 $values=@{SystemRoot='C:\Windows';WINDIR='C:\Windows';TEMP=$Directory;TMP=$Directory;PATH='C:\Windows\System32';DOTNET_ROOT='C:\PocInput\dotnet';DOTNET_MULTILEVEL_LOOKUP='0';DOTNET_ROLL_FORWARD='Disable';DOTNET_CLI_TELEMETRY_OPTOUT='1'}
 foreach($key in $values.Keys) { $info.EnvironmentVariables[$key]=$values[$key] }
 $child=[Diagnostics.Process]::Start($info)
 try {
  if(-not $child.WaitForExit($Seconds*1000)) {
   # Best-effort process-tree cleanup; outer root watchdog disposes the guest.
   $kill=New-Object Diagnostics.ProcessStartInfo
   $kill.FileName='C:\Windows\System32\taskkill.exe'; $kill.Arguments='/PID '+$child.Id+' /T /F'; $kill.UseShellExecute=$false; $kill.CreateNoWindow=$true
   $killer=[Diagnostics.Process]::Start($kill)
   try { if(-not $killer.WaitForExit(5000)) { $killer.Kill(); $null=$killer.WaitForExit(1000) } } finally { $killer.Dispose() }
   if(-not $child.WaitForExit(5000)) { $child.Kill(); $null=$child.WaitForExit(1000) }
   throw 'CHILD_TIMEOUT'
  }
  return $child.ExitCode
 } finally { $child.Dispose() }
}
$caseDir='C:\PocPrivate\'+$CaseId
$exitCode=27
try {
 if($env:SystemRoot -ine 'C:\Windows' -or $env:USERNAME -ine 'WDAGUtilityAccount') { exit 20 }
 if(@([Net.NetworkInformation.NetworkInterface]::GetAllNetworkInterfaces() | Where-Object {$_.NetworkInterfaceType -ne 'Loopback'}).Count) { exit 21 }
 foreach($dir in @('C:\PocInput','C:\PocInput\corpus','C:\PocInput\app','C:\PocInput\dotnet','C:\PocInput\python','C:\PocInput\qpdf','C:\PocInput\oracle','C:\PocExport')) { Assert-Plain $dir $true }
 if(@(Get-ChildItem -LiteralPath 'C:\PocExport' -Force).Count -ne 0 -or (Test-Path -LiteralPath 'C:\PocPrivate')) { exit 24 }
 $ledgerPath='C:\PocInput\corpus\sources.json'; $qpdfPath='C:\PocInput\qpdf-files.json'
 foreach($path in @($ledgerPath,$qpdfPath)) { Assert-Plain $path $false; if((Get-Item -LiteralPath $path).Length -gt 65536) { throw 'MANIFEST_BOUND' } }
 if((Get-FileHash -LiteralPath $ledgerPath).Hash -ine $LedgerSha256 -or (Get-FileHash -LiteralPath $qpdfPath).Hash -ine $QpdfManifestSha256) { throw 'MANIFEST_DRIFT' }
 $ledger=Get-Content -LiteralPath $ledgerPath -Raw | ConvertFrom-Json
 $qpdfFiles=Get-Content -LiteralPath $qpdfPath -Raw | ConvertFrom-Json
 Assert-Sources $ledger
 $null=New-Item -ItemType Directory -Path $caseDir
 $cases=@{AB=@('A','B');CAB=@('C','A','B');AA=@('A','A');AC=@('A','C')}
 $output=$caseDir+'\result.pdf'
 $arguments='C:\PocInput\app\MergeSmoke.dll '+$output
 foreach($name in $cases[$CaseId]) { $arguments+=' C:\PocInput\corpus\'+$name+'.pdf' }
 $exitCode=31
 if((Invoke-Child 'C:\PocInput\dotnet\dotnet.exe' $arguments $caseDir 15) -ne 0) { throw 'MERGE_NONZERO' }
 Assert-Plain $output $false
 if((Get-Item -LiteralPath $output).Length -le 0 -or (Get-Item -LiteralPath $output).Length -gt 262144) { throw 'OUTPUT_BOUND' }
 Assert-Sources $ledger
 $sourceHashes=[ordered]@{}; foreach($name in @('A','B','C')) { $sourceHashes[$name]=$ledger.$name.sha256 }
 $binding=[ordered]@{schema=1;case=$CaseId;merger='PDFsharp/6.2.4';sourceSha256=$sourceHashes;outputSha256=(Get-FileHash -LiteralPath $output).Hash.ToLowerInvariant();qpdfFiles=$qpdfFiles}
 $bindingPath=$caseDir+'\binding.json'; Write-NewJson $bindingPath $binding
 $bindingHash=(Get-FileHash -LiteralPath $bindingPath).Hash.ToLowerInvariant()
 $arguments='-I -S -B C:\PocInput\oracle\observe.py --binding '+$bindingPath+' --binding-sha256 '+$bindingHash+' --bundle C:\PocInput\qpdf --sources C:\PocInput\corpus --output '+$output+' --evidence '+$caseDir+'\evidence'
 $exitCode=32
 if((Invoke-Child 'C:\PocInput\python\python.exe' $arguments $caseDir 45) -ne 0) { throw 'OBSERVER_NONZERO' }
 Assert-Sources $ledger
 if((Get-FileHash -LiteralPath $ledgerPath).Hash -ine $LedgerSha256 -or (Get-FileHash -LiteralPath $qpdfPath).Hash -ine $QpdfManifestSha256) { throw 'MANIFEST_DRIFT' }
 $exitCode=0
} catch { } finally {
 # Quarantine only: no PDF is exported. Partial failure captures are not PASS.
 try {
  $names=@('binding.json','result.json','version.stdout','version.stderr','observation.stdout','observation.stderr')
  $copied=0
  foreach($name in $names) {
   $source=$caseDir+'\evidence\'+$name
   if($name -eq 'binding.json') { $source=$caseDir+'\binding.json' }
   if(Test-Path -LiteralPath $source) {
    Assert-Plain $source $false
    Assert-Bound (Get-Item -LiteralPath $source).Length ($name.EndsWith('.stderr'))
    Assert-Plain 'C:\PocExport' $true
    [IO.File]::Copy($source,('C:\PocExport\'+$CaseId+'.'+$name),$false)
    $copied++
   }
  }
  if($exitCode -eq 0 -and $copied -ne 6) { $exitCode=33 }
 } catch { $exitCode=34 }
}
exit $exitCode
