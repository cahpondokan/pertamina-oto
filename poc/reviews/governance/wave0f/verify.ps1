param([Parameter(Mandatory=$true)][string]$EvidenceRoot)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$manifest=Get-Content (Join-Path $PSScriptRoot '../../../oracles/qpdf-12.4.1/intake-manifest.json') -Raw | ConvertFrom-Json
$before=@{}
foreach($f in $manifest.rawEvidence){
 $p=Join-Path $EvidenceRoot $f.name
 $h=(Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash.ToLowerInvariant()
 if($h -ne $f.sha256 -or (Get-Item -LiteralPath $p).Length -ne $f.bytes){throw "Raw evidence mismatch: $($f.name)"}
 $before[$f.name]=$h
}
$inventory=Get-Content (Join-Path $EvidenceRoot 'archive-static-inventory.json') -Raw | ConvertFrom-Json
$expected=@{}; foreach($e in $inventory.entries){$expected[$e.path]=$e}
$zip=[IO.Compression.ZipFile]::OpenRead((Join-Path $EvidenceRoot 'qpdf-12.4.1-msvc64.zip'))
$rows=@(); $seen=@{}; $noticeRows=@()
try {
 foreach($e in $zip.Entries){
  $n=$e.FullName
  if($seen.ContainsKey($n) -or $n -match '(^/|\\|:|(^|/)\.\.(/|$))'){throw 'Unsafe or duplicate member'}
  $seen[$n]=$true
  $stream=$e.Open(); $sha=[Security.Cryptography.SHA256]::Create()
  try {$h=[Convert]::ToHexString($sha.ComputeHash($stream)).ToLowerInvariant()}finally{$stream.Dispose();$sha.Dispose()}
  if(-not $expected.ContainsKey($n) -or $expected[$n].sha256 -ne $h -or $expected[$n].bytes -ne $e.Length){throw "Entry mismatch: $n"}
  $family='qpdf documentation and ancillary assets'
  if($n.EndsWith('/')){$family='directory'}
  elseif($n -match '/bin/(concrt|msvcp|vcruntime)'){$family='Microsoft C++ runtime'}
  elseif($n -match '/(bin|include|lib)/|/examples/'){$family='qpdf code/build artifacts; embedded dependencies unresolved'}
  elseif($n -match 'jquery'){$family='jQuery'}
  elseif($n -match 'stemmer'){$family='Snowball stemmer'}
  elseif($n -match '(?i)fontawesome'){$family='Font Awesome font'}
  elseif($n -match '(?i)lato'){$family='Lato font'}
  elseif($n -match '(?i)roboto'){$family='Roboto Slab font'}
  elseif($n -match '/_static/'){$family='Sphinx/theme/generated assets; exact provenance unresolved'}
  $rows += [ordered]@{path=$n;bytes=$e.Length;sha256=$h;family=$family}
  if($n -match '\.(txt|html|js|css|h|hh|c|cc|cmake|pc|svg)$'){
   $s=$e.Open();$r=[IO.StreamReader]::new($s)
   try{$t=$r.ReadToEnd()}finally{$r.Dispose()}
   $markers=@(); foreach($m in @('Apache','copyright','license','licence','OpenSSL','zlib','jpeg','Sphinx','Snowball','jQuery','SIL','MIT','BSD')){if($t -match [regex]::Escape($m)){$markers+=$m}}
   $noticeRows += [ordered]@{path=$n;sha256=$h;markers=$markers}
  }
 }
}finally{$zip.Dispose()}
if($rows.Count -ne 319 -or $rows.Count -ne $inventory.entries.Count){throw 'Entry count mismatch'}
foreach($f in $manifest.rawEvidence){if((Get-FileHash -LiteralPath (Join-Path $EvidenceRoot $f.name)).Hash.ToLowerInvariant() -ne $before[$f.name]){throw 'Raw evidence changed'}}
[ordered]@{task='P2-W0F-A7-QPDF-GOVERNANCE-REVIEW';verifiedUtc=[DateTime]::UtcNow.ToString('o');result='PASS';rawHashes=$before;entryCount=$rows.Count;allEntryHashesMatch=$true;rawEvidenceUnchanged=$true;extraction=$false;candidateExecution=$false;pdfParsing=$false;crcIndependentCheck='NOT_PERFORMED; prior intake only';entries=$rows;textMarkerInventory=$noticeRows} | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $PSScriptRoot 'verification.json') -Encoding utf8
Write-Output "PASS: 3 raw files, $($rows.Count) entry hashes/sizes, safe unique paths, raw files unchanged; $($noticeRows.Count) text entries scanned."
