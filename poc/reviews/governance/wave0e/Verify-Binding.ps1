param([Parameter(Mandatory)][string]$IntegrationRoot,[Parameter(Mandatory)][string]$ArtifactRoot)
$ErrorActionPreference='Stop'
$head=& git -c safe.directory='*' -C $IntegrationRoot rev-parse HEAD
if($head -ne 'dd003ea20ec8e00b96c265907560f66957783a5d'){throw 'Checkpoint changed'}
$intake=Join-Path $IntegrationRoot 'poc/intake/dependency-artifacts'
$results=Get-Content (Join-Path $intake 'results.json') -Raw | ConvertFrom-Json
if($results.Count -ne 3){throw 'Expected three packages'}
$verified=@(foreach($r in $results){
 $stem=$r.id.ToLowerInvariant()+'.'+$r.version
 $file=Join-Path $ArtifactRoot ($stem+'.nupkg')
 $before=(Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash
 if($before -ne $r.receipt.sha256 -or (Get-Item -LiteralPath $file).Length -ne $r.receipt.bytes){throw 'Package mismatch'}
 $sha512=[Convert]::ToBase64String([Convert]::FromHexString((Get-FileHash -LiteralPath $file -Algorithm SHA512).Hash))
 $metadata=Get-Content (Join-Path $IntegrationRoot ('poc/intake/dependency-closure/metadata/'+$stem+'.json')) -Raw | ConvertFrom-Json
 if($sha512 -ne $r.receipt.catalogSha512 -or $sha512 -ne $metadata.catalog.packageHash){throw 'Catalog mismatch'}
 $entries=Get-Content (Join-Path $intake ($stem+'.inventory.json')) -Raw | ConvertFrom-Json
 $legal=Get-Content (Join-Path $intake ($stem+'.legal.json')) -Raw | ConvertFrom-Json
 $z=[IO.Compression.ZipFile]::OpenRead($file)
 try {
  if($z.Entries.Count -ne $entries.Count){throw 'Entry count mismatch'}
  foreach($e in $entries){
   $entry=$z.GetEntry($e.name)
   if($null -eq $entry -or $entry.Length -ne $e.bytes){throw 'Entry mismatch'}
   $s=$entry.Open(); $h=[Security.Cryptography.SHA256]::Create()
   try{$hash=[Convert]::ToHexString($h.ComputeHash($s))}finally{$h.Dispose();$s.Dispose()}
   if($hash -ne $e.sha256){throw 'Entry hash mismatch'}
  }
  foreach($l in $legal){
   $reader=[IO.StreamReader]::new($z.GetEntry($l.entry).Open())
   try{$txt=$reader.ReadToEnd()}finally{$reader.Dispose()}
   if($txt -cne $l.text){throw 'Legal text differs'}
  }
  $reader=[IO.StreamReader]::new($z.GetEntry($r.id+'.nuspec').Open())
  try{$xmlText=$reader.ReadToEnd()}finally{$reader.Dispose()}
  if($xmlText.Replace("`r`n","`n").TrimEnd("`n") -cne (Get-Content (Join-Path $intake ($stem+'.nuspec.xml')) -Raw).Replace("`r`n","`n").TrimEnd("`n")){throw 'Nuspec differs after newline normalization'}
  [xml]$xml=$xmlText
  if($xml.package.metadata.license.'#text' -ne 'MIT'){throw 'Unexpected license'}
 }finally{$z.Dispose()}
 if((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $before){throw 'Artifact changed'}
 [ordered]@{id=$r.id;version=$r.version;bytes=$r.receipt.bytes;sha256Before=$before;sha256After=$before;catalogSha512Match=$true;entriesVerified=$entries.Count;legalTextsVerified=$legal.Count;sourceCommit=$xml.package.metadata.repository.commit;result='PASS_STATIC_BINDING';candidateExecuted=$false}
})
[ordered]@{task='P2-W0E-B-A7-GOVERNANCE-REVIEW';checkpoint=$head;date='2026-09-07';packages=$verified;acquisition=0;restoreBuild=0;documentProcessing=0;cost='ZERO_ADDITIONAL_COST'} | ConvertTo-Json -Depth 6
