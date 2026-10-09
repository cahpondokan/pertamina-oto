# Static-only inspection. No downloaded assembly, generator, analyzer or build target is executed.
$ErrorActionPreference='Stop'
$raw='D:\OTO\pertamina-oto\artifacts\evidence\P02-20260905-1434\agent-2-dependency-intake'
$closure=Get-Content "$PSScriptRoot/../dependency-closure/closure.json" -Raw | ConvertFrom-Json
Add-Type -AssemblyName System.IO.Compression.FileSystem
$results=@(foreach($c in $closure.components | Where-Object id -NE 'PDFsharp') {
 $name=$c.id.ToLowerInvariant()+'.'+$c.version
 $receipt=Get-Content "$raw/$name.acquisition.json" -Raw | ConvertFrom-Json
 $file="$raw/$name.nupkg"
 if((Get-FileHash -LiteralPath $file).Hash -ne $receipt.sha256){throw 'Hash changed'}
 $z=[IO.Compression.ZipFile]::OpenRead($file)
 try {
 $items=@(foreach($e in $z.Entries){
  if($e.FullName -match '(^/|(^|/)\.\.(/|$)|\\|:)'){throw 'Unsafe entry'}
  $s=$e.Open(); try {$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))} finally {$s.Dispose()}
  [ordered]@{name=$e.FullName;bytes=$e.Length;sha256=$hash;activationSurface=($e.FullName -match '^(analyzers|build|buildTransitive|runtimes)/')}
 })
 $nuspec=@($z.Entries | Where-Object FullName -Like '*.nuspec')
 if($nuspec.Count -ne 1){throw 'Nuspec count'}
 $reader=[IO.StreamReader]::new($nuspec[0].Open()); try{$text=$reader.ReadToEnd()}finally{$reader.Dispose()}
 [xml]$n=$text
 if($n.package.metadata.id -ne $c.id -or $n.package.metadata.version -ne $c.version -or $n.package.metadata.license.InnerText -ne 'MIT'){throw 'Identity/license mismatch'}
 $text | Set-Content "$PSScriptRoot/$name.nuspec.xml"
 $group=@($n.package.metadata.dependencies.group | Where-Object targetFramework -EQ 'net8.0')
 if($group.Count -ne 1){throw 'Missing net8 group'}
 $deps=@(foreach($d in $group[0].dependency){if($null -ne $d){[ordered]@{id=$d.id;version=$d.version}}})
 if($deps.Count -ne $c.dependencies.Count){throw 'Dependency count mismatch'}
 foreach($d in $deps){$expected=@($c.dependencies | Where-Object id -EQ $d.id); if($expected.Count -ne 1 -or $expected[0].proposedVersion -ne $d.version){throw 'Dependency mismatch'}}
 $legal=@(foreach($e in $z.Entries | Where-Object FullName -Match '(?i)(license|notice)'){
  $r=[IO.StreamReader]::new($e.Open());try{$body=$r.ReadToEnd()}finally{$r.Dispose()}
  [ordered]@{entry=$e.FullName;text=$body}
 })
 $legal | ConvertTo-Json -Depth 5 | Set-Content "$PSScriptRoot/$name.legal.json"
 $items | ConvertTo-Json -Depth 5 | Set-Content "$PSScriptRoot/$name.inventory.json"
 $env:NUGET_CERT_REVOCATION_MODE='offline'
 $log=(& dotnet nuget verify $file --all --configfile "$PSScriptRoot/../../../NuGet.Config" --verbosity normal --force-english-output 2>&1 | Out-String)
 $verifyExit=$LASTEXITCODE
 $log.Replace($raw,'<RUN_EVIDENCE>') | Set-Content "$PSScriptRoot/$name.signature.txt"
 [ordered]@{id=$c.id;version=$c.version;receipt=$receipt;entries=$items.Count;signatureExit=$verifyExit;revocationUnknown=($log -match 'RevocationStatusUnknown');license='MIT';legalEntries=@($legal.entry);selectedDependencies=$deps;activationEntries=@($items | Where-Object activationSurface | ForEach-Object name);nativeNamedEntries=@($items | Where-Object name -Match '(^|/)native/|\.(exe|so|dylib)$' | ForEach-Object name);providerExecuted=$false}
 }finally{$z.Dispose()}
 if((Get-FileHash -LiteralPath $file).Hash -ne $receipt.sha256){throw 'Hash changed after inspection'}
})
$results | ConvertTo-Json -Depth 12 | Set-Content "$PSScriptRoot/results.json"
$results | Select-Object id,entries,signatureExit,revocationUnknown
