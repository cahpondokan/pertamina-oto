param([Parameter(Mandatory=$true)][string]$CandidateRoot)
$ErrorActionPreference='Stop'
$root=(Resolve-Path -LiteralPath $CandidateRoot).Path
$files=@(Get-ChildItem -LiteralPath $root -Recurse -File | Sort-Object FullName)
$records=@(foreach($file in $files) {
 $relative=$file.FullName.Substring($root.Length+1).Replace('\','/')
 $record=[ordered]@{path=$relative;bytes=$file.Length;sha256=(Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()}
 if($file.Extension -in '.exe','.dll') {
  $sig=Get-AuthenticodeSignature -LiteralPath $file.FullName
  $record.fileVersion=$file.VersionInfo.FileVersion
  $record.productVersion=$file.VersionInfo.ProductVersion
  $record.signatureStatus=[string]$sig.Status
  $record.signerThumbprint=if($sig.SignerCertificate){$sig.SignerCertificate.Thumbprint}else{$null}
 }
 [pscustomobject]$record
})
[ordered]@{schemaVersion='1.0';task='P2-W0D-A3-LOCAL-POPPLER-INVENTORY';checkedUtc=[DateTime]::UtcNow.ToString('o');rootReference='USER_NAMED_CODEX_RUNTIME_POPPLER_ROOT';rootSuffix='.cache/codex-runtimes/codex-primary-runtime/dependencies/native/poppler';packageManifest=(Get-Content -LiteralPath (Join-Path $root 'manifest.json') -Raw | ConvertFrom-Json);files=$records} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'inventory.json') -Encoding utf8
Write-Output "INVENTORIED_FILES=$($records.Count)"
