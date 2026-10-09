param([Parameter(Mandatory=$true)][string]$PackagePath)
$ErrorActionPreference = 'Stop'
$expected = '5647882339B327CBF6DA9EA3F882EFB335F8D602E6FD45323ADF9E78DA53CF67'
$before = (Get-FileHash -LiteralPath $PackagePath -Algorithm SHA256).Hash
if ($before -ne $expected -or (Get-Item -LiteralPath $PackagePath).Length -ne 4625326) { throw 'Artifact mismatch' }
$inventory = Get-Content poc/intake/pdfsharp-6.2.4/archive-inventory.json -Raw | ConvertFrom-Json
$zip = [System.IO.Compression.ZipFile]::OpenRead($PackagePath)
try {
 if ($zip.Entries.Count -ne 111 -or $inventory.Count -ne 111) { throw 'Count mismatch' }
 foreach ($record in $inventory) {
  $entry = $zip.GetEntry($record.name)
  if ($null -eq $entry -or $entry.Length -ne $record.length) { throw 'Entry mismatch' }
  $stream = $entry.Open(); $sha = [System.Security.Cryptography.SHA256]::Create()
  try { $digest = [Convert]::ToHexString($sha.ComputeHash($stream)) }
  finally { $sha.Dispose(); $stream.Dispose() }
  if ($digest -ne $record.sha256) { throw 'Entry digest mismatch' }
 }
} finally { $zip.Dispose() }
$sourceRecords = Get-Content poc/oracles/source-digests.json -Raw | ConvertFrom-Json
foreach ($record in $sourceRecords) {
 if ((Get-FileHash -LiteralPath $record.path -Algorithm SHA256).Hash -ne $record.sha256) { throw 'Preparation source digest mismatch' }
}
$after = (Get-FileHash -LiteralPath $PackagePath -Algorithm SHA256).Hash
if ($after -ne $before) { throw 'Package changed' }
[ordered]@{package='PDFsharp';version='6.2.4';bytes=4625326;sha256Before=$before;sha256After=$after;archiveEntriesVerified=111;preparationSourcesVerified=$sourceRecords.Count;candidateExecuted=$false;result='PASS_STATIC_BINDING'} | ConvertTo-Json
