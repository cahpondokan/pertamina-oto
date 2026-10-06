$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
$destination = Join-Path $project ('artifacts/distribution/OTO-Windows-x64-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
$package = Join-Path $destination 'PERTAMINA-OTO'
New-Item -ItemType Directory -Path $package | Out-Null
Copy-Item -LiteralPath (Join-Path $project 'artifacts/desktop-assistant') -Destination (Join-Path $package 'app') -Recurse
$runtime = Join-Path $package 'runtime'
New-Item -ItemType Directory -Path "$runtime/host/fxr", "$runtime/shared/Microsoft.NETCore.App" | Out-Null
Copy-Item -LiteralPath 'C:/Program Files/dotnet/dotnet.exe','C:/Program Files/dotnet/LICENSE.txt','C:/Program Files/dotnet/ThirdPartyNotices.txt' -Destination $runtime
Copy-Item -LiteralPath 'C:/Program Files/dotnet/host/fxr/10.0.11' -Destination "$runtime/host/fxr" -Recurse
Copy-Item -LiteralPath 'C:/Program Files/dotnet/shared/Microsoft.NETCore.App/10.0.11' -Destination "$runtime/shared/Microsoft.NETCore.App" -Recurse
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'portable/MULAI-OTO.cmd'),(Join-Path $PSScriptRoot 'portable/BACA-DULU.txt') -Destination $package
# Verify every copied app file against its source, including the worker.
$source = Join-Path $project 'artifacts/desktop-assistant'
foreach ($file in Get-ChildItem -LiteralPath $source -Recurse -File -Force) {
 $relative = $file.FullName.Substring($source.Length + 1)
 $copy = Join-Path "$package/app" $relative
 if ((Get-FileHash -LiteralPath $file.FullName).Hash -ne (Get-FileHash -LiteralPath $copy).Hash) { throw "App copy mismatch" }
}
$env:DOTNET_ROOT = $runtime
$env:DOTNET_ROOT_X64 = $runtime
$env:DOTNET_MULTILEVEL_LOOKUP = '0'
& "$runtime/dotnet.exe" --list-runtimes
& "$package/app/worker/Pertamina.Oto.LocalPdf.Worker.exe" --self-test
if ($LASTEXITCODE -ne 0) { throw 'Bundled worker self-test failed' }
$zip = Join-Path $destination 'PERTAMINA-OTO-Portable-Windows-x64.zip'
Compress-Archive -LiteralPath $package -DestinationPath $zip -CompressionLevel Optimal
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($zip)
try {
 $entries = @($archive.Entries | Where-Object Name)
 $files = @(Get-ChildItem -LiteralPath $package -Recurse -File -Force)
 if ($entries.Count -ne $files.Count) { throw 'Archive file count mismatch' }
 foreach ($entry in $entries) {
  $path = Join-Path $destination $entry.FullName
  $stream = $entry.Open(); $sha = [Security.Cryptography.SHA256]::Create()
  try { $hash = [Convert]::ToHexString($sha.ComputeHash($stream)) } finally { $stream.Dispose(); $sha.Dispose() }
  if ($hash -ne (Get-FileHash -LiteralPath $path).Hash) { throw 'Archive integrity mismatch' }
 }
 [pscustomobject]@{ ZIP=$zip; MB=[math]::Round((Get-Item $zip).Length/1MB,1); Files=$entries.Count; SHA256=(Get-FileHash $zip).Hash }
} finally { $archive.Dispose() }
