param([Parameter(Mandatory=$true)][string]$PackagePath)
$ErrorActionPreference='Stop'
$expected='5647882339B327CBF6DA9EA3F882EFB335F8D602E6FD45323ADF9E78DA53CF67'
if ((Get-FileHash -LiteralPath $PackagePath -Algorithm SHA256).Hash -ne $expected) { throw 'Artifact digest mismatch' }
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip=[IO.Compression.ZipFile]::OpenRead($PackagePath)
try {
  $inventory=@(foreach($entry in $zip.Entries) {
    if ($entry.FullName -match '(^/|(^|/)\.\.(/|$)|\\|:)') { throw 'Unsafe archive entry' }
    $s=$entry.Open(); $hash=[Security.Cryptography.SHA256]::Create()
    try { $digest=[Convert]::ToHexString($hash.ComputeHash($s)) } finally { $s.Dispose(); $hash.Dispose() }
    [ordered]@{name=$entry.FullName;length=$entry.Length;sha256=$digest}
  })
  $reader=[IO.StreamReader]::new($zip.GetEntry('PDFsharp.nuspec').Open())
  try {$nuspec=$reader.ReadToEnd()} finally {$reader.Dispose()}
  [xml]$xml=$nuspec
  if($xml.package.metadata.id -ne 'PDFsharp' -or $xml.package.metadata.version -ne '6.2.4') {throw 'Identity mismatch'}
  $nuspec | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'PDFsharp.nuspec.xml') -Encoding UTF8
  $inventory | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'archive-inventory.json') -Encoding UTF8
  $native=@($inventory | Where-Object {$_.name -match '(^runtimes/|/native/|\.(exe|so|dylib|ps1|cmd|bat|targets|props)$)'})
  if($native.Count) {throw 'Unexpected native/build/script entry requires review'}
  [ordered]@{package='PDFsharp';version='6.2.4';sha256=$expected;bytes=(Get-Item -LiteralPath $PackagePath).Length;entryCount=$inventory.Count;archivePathSafety='PASS';unexpectedNativeBuildScriptEntries=$native.Count;assemblyExecution=$false;dependenciesDownloaded=0} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'inspection.json') -Encoding UTF8
} finally {$zip.Dispose()}
if ((Get-FileHash -LiteralPath $PackagePath -Algorithm SHA256).Hash -ne $expected) {throw 'Artifact changed during inspection'}
'Inspection and independent hash recomputation PASS'
