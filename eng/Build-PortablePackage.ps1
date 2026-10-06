param(
 [Parameter(Mandatory=$true)][string]$BuildOutput,
 [Parameter(Mandatory=$true)][ValidatePattern('^[0-9]{8}-[0-9]{6}-[a-f0-9]{8}$')][string]$BuildId,
 [Parameter(Mandatory=$true)][string]$SourceInventory,
 [Parameter(Mandatory=$true)][string]$Output,
 [string]$RuntimeRoot,
 [string]$RuntimeAdmission,
 [string]$RuntimeAdmissionSha256,
 [switch]$CandidateWithoutRuntime,
 [string]$Compiler = 'D:/DevTools/VS2026Community/MSBuild/Current/Bin/Roslyn/csc.exe'
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$project = Split-Path $PSScriptRoot -Parent
function NoLinks([string]$path) {
 for ($item = [IO.Path]::GetFullPath($path); $item; $item = [IO.Path]::GetDirectoryName($item)) {
  if ((Test-Path -LiteralPath $item) -and ((Get-Item -LiteralPath $item -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw 'Link/junction tidak diizinkan.' }
 }
}
function CopyTree([string]$source,[string]$target) {
 NoLinks $source
 foreach ($item in Get-ChildItem -LiteralPath $source -Recurse -Force) { NoLinks $item.FullName }
 foreach ($file in Get-ChildItem -LiteralPath $source -Recurse -File -Force) {
  if ($file.Extension -eq '.pdb') { continue }
  $relative = $file.FullName.Substring($source.TrimEnd('\','/').Length + 1)
  if ($relative -match '(^|[\\/])Data([\\/]|$)' -or $file.Extension -in @('.pdf','.docx') -or $file.Name -match 'audit|test-results|path\.txt$' -or $file.Name -in @('preferences.json','library.json','online-updates.json','update-source.json')) { throw 'Output bukan build bersih.' }
  $destination = Join-Path $target $relative
  New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
  Copy-Item -LiteralPath $file.FullName -Destination $destination
  if ((Get-FileHash -LiteralPath $file.FullName).Hash -ne (Get-FileHash -LiteralPath $destination).Hash) { throw 'Copy mismatch.' }
 }
}
$source = (Resolve-Path -LiteralPath $BuildOutput).Path
NoLinks $source; NoLinks $Output; NoLinks $SourceInventory
if (Test-Path -LiteralPath $Output) { throw 'Output immutable sudah ada; gunakan folder baru.' }
if (!(Test-Path -LiteralPath "$source/Pertamina.Oto.Desktop.App.exe") -or !(Test-Path -LiteralPath "$source/worker/Pertamina.Oto.LocalPdf.Worker.exe")) { throw 'Build tidak lengkap.' }
if (![Text.Encoding]::UTF8.GetString([IO.File]::ReadAllBytes("$source/Pertamina.Oto.Desktop.App.dll")).Contains('PortableProfile')) { throw 'Build tidak mendukung profil portable; build ulang source approved.' }
$sourceBinding = Get-Content -LiteralPath $SourceInventory -Raw | ConvertFrom-Json
if ($sourceBinding.Head -cnotmatch '^[a-f0-9]{40}$' -or !$sourceBinding.Files -or !$sourceBinding.CoreUnchangedDuringBuild) { throw 'Source binding tidak valid.' }
foreach ($entry in $sourceBinding.Files) {
 if ([IO.Path]::IsPathRooted($entry.Path) -or $entry.Path -match ':' -or @($entry.Path.Split([char[]]@('/','\')) | Where-Object { !$_ -or $_ -in @('.','..') }).Count -or $entry.Sha256 -cnotmatch '^[A-F0-9]{64}$') { throw 'Source inventory harus relative path dan hash.' }
}
$product = Get-Content -LiteralPath "$source/product-info.json" -Raw | ConvertFrom-Json
if ($product.ProductVersion -cne '0.1.0' -or $product.Channel -cne 'Pengembangan') { throw 'Product identity mismatch.' }
if (!$RuntimeRoot -and !$CandidateWithoutRuntime) { throw 'Runtime admission wajib untuk paket portable lengkap.' }
if ($RuntimeRoot) {
 if (!$RuntimeAdmission -or $RuntimeAdmissionSha256 -cnotmatch '^[A-F0-9]{64}$' -or (Get-FileHash -LiteralPath $RuntimeAdmission).Hash -cne $RuntimeAdmissionSha256) { throw 'Runtime admission hash belum dipatok integrator.' }
 $admission = Get-Content -LiteralPath $RuntimeAdmission -Raw | ConvertFrom-Json
 if ($admission.Status -cne 'APPROVED' -or $admission.Scope -cne 'PORTABLE-PUBLIC-DEV-20261002' -or !$admission.AuthorityReference -or !$admission.Files) { throw 'Runtime admission scope tidak valid.' }
 $runtimePaths = @($admission.Files | ForEach-Object { $_.Path.Replace('\','/') })
 if (@($runtimePaths | Sort-Object -Unique).Count -ne $runtimePaths.Count) { throw 'Duplicate runtime admission path.' }
 foreach ($file in $admission.Files) {
  if ($file.Path -match '(^|[\\/])\.{1,2}([\\/]|$)|:' -or [IO.Path]::IsPathRooted($file.Path) -or @($file.Path.Split([char[]]@('/','\')) | Where-Object { !$_ -or $_.EndsWith('.') -or $_.EndsWith(' ') }).Count) { throw 'Runtime admission path invalid.' }
  $path = Join-Path $RuntimeRoot $file.Path; NoLinks $path
  if ((Get-FileHash -LiteralPath $path).Hash -cne $file.Sha256 -or (Get-Item -LiteralPath $path).Length -ne $file.Length) { throw 'Runtime admission hash mismatch.' }
 }
 $actualRuntimePaths = @(Get-ChildItem -LiteralPath $RuntimeRoot -Recurse -File -Force | ForEach-Object { $_.FullName.Substring([IO.Path]::GetFullPath($RuntimeRoot).TrimEnd('\','/').Length+1).Replace('\','/') })
 if (@(Compare-Object ($runtimePaths | Sort-Object) ($actualRuntimePaths | Sort-Object)).Count) { throw 'Unadmitted runtime files.' }
}
New-Item -ItemType Directory -Path $Output | Out-Null
$package = Join-Path $Output 'PERTAMINA-OTO'
New-Item -ItemType Directory -Path "$package/app","$package/lisensi" | Out-Null
CopyTree $source "$package/app"
Copy-Item -LiteralPath "$PSScriptRoot/portable/portable-profile.json" -Destination "$package/app"
Copy-Item -LiteralPath "$PSScriptRoot/portable/MULAI-DI-SINI.txt" -Destination $package
Copy-Item -LiteralPath "$PSScriptRoot/portable/PERTAMINA-OTO.exe.config" -Destination $package
Copy-Item -LiteralPath $SourceInventory -Destination "$package/source-inventory.json"
if ($RuntimeRoot) { CopyTree ((Resolve-Path -LiteralPath $RuntimeRoot).Path) "$package/runtime"; Copy-Item -LiteralPath $RuntimeAdmission -Destination "$package/runtime-admission.json" }
# Preserve component notices separately, as exact vendor/cache text. No downloads.
Copy-Item -LiteralPath "$source/Assets/About-component-notices.txt" -Destination "$package/lisensi/INVENTARIS-KOMPONEN.txt"
Copy-Item -LiteralPath "$source/Assets/History/LICENSE.txt" -Destination "$package/lisensi/Lucide-Feather.txt"
$pdfNotice = Get-Content -LiteralPath "$project/poc/wave0h/candidate/EVALUATION-NOTICES.md" -Raw
$pdfNotice = $pdfNotice.Substring($pdfNotice.IndexOf('Copyright (c) 2001-2026 empira Software GmbH'))
[IO.File]::WriteAllText("$package/lisensi/PDFsharp-MIT.txt", $pdfNotice, [Text.UTF8Encoding]::new($false))
$legal = @()
foreach ($assetsFile in @('src/Hosts/Pertamina.Oto.Desktop.App/obj/project.assets.json','src/Workers/Pertamina.Oto.LocalPdf.Worker/obj/project.assets.json','src/Workers/Pertamina.Oto.WordPdf.Worker/obj/project.assets.json')) {
 $assets = Get-Content -LiteralPath (Join-Path $project $assetsFile) -Raw | ConvertFrom-Json
 foreach ($library in $assets.libraries.PSObject.Properties | Where-Object { $_.Value.type -eq 'package' }) {
  foreach ($cache in $assets.packageFolders.PSObject.Properties.Name) {
   $directory = Join-Path $cache $library.Value.path
   if (!(Test-Path -LiteralPath $directory)) { continue }
   $destination = Join-Path "$package/lisensi" $library.Name.Replace('/','-')
   New-Item -ItemType Directory -Path $destination -Force | Out-Null
   foreach ($file in Get-ChildItem -LiteralPath $directory -File | Where-Object { $_.Name -match '^(license|notice|third.?party.?notices)' -or $_.Extension -eq '.nuspec' }) {
    Copy-Item -LiteralPath $file.FullName -Destination $destination -Force
    $legal += @{Component=$library.Name;Path=('lisensi/' + $library.Name.Replace('/','-') + '/' + $file.Name);Sha256=(Get-FileHash -LiteralPath $file.FullName).Hash}
   }
   break
  }
 }
}
if ($RuntimeRoot) {
 foreach ($name in @('LICENSE.txt','ThirdPartyNotices.txt')) {
  if (!(Test-Path -LiteralPath "$RuntimeRoot/$name")) { throw 'Runtime vendor notices missing.' }
  Copy-Item -LiteralPath "$RuntimeRoot/$name" -Destination "$package/lisensi/dotnet-$name"
 }
}
$framework = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319'
$options = @('/nologo','/nostdlib+','/langversion:7.3','/target:winexe','/platform:x64','/optimize+','/deterministic+','/warnaserror+',('/out:"' + "$package/PERTAMINA-OTO.exe" + '"'),('/win32manifest:"' + "$PSScriptRoot/WindowsSetup/app.manifest" + '"'),('/win32icon:"' + "$project/src/Hosts/Pertamina.Oto.Desktop.App/Assets/oto.ico" + '"'))
foreach ($name in @('mscorlib','System','System.Core','System.Web.Extensions','System.Windows.Forms')) { $options += '/reference:"' + "$framework/$name.dll" + '"' }
$options += '"' + "$PSScriptRoot/portable/PortableLauncher.cs" + '"'
[IO.File]::WriteAllLines("$Output/compiler.rsp", $options, [Text.UTF8Encoding]::new($false))
& $Compiler /noconfig ('@' + "$Output/compiler.rsp")
if ($LASTEXITCODE -ne 0) { throw 'Portable launcher compilation failed.' }
[IO.File]::WriteAllText("$package/app/build-info.json", (@{Version=$BuildId;ManifestSha256='';ProductVersion=$product.ProductVersion;Channel=$product.Channel} | ConvertTo-Json), [Text.UTF8Encoding]::new($false))
$entries = @(Get-ChildItem -LiteralPath $package -Recurse -File -Force | Sort-Object FullName | ForEach-Object { @{Path=$_.FullName.Substring($package.Length+1).Replace('\','/');Length=$_.Length;Sha256=(Get-FileHash -LiteralPath $_.FullName).Hash} })
$manifest = @{Schema=1;BuildId=$BuildId;ProductVersion=$product.ProductVersion;Channel=$product.Channel;PublicPurpose='UJI PENGEMBANGAN';RuntimeBundled=[bool]$RuntimeRoot;ReleaseGate='HOLD_PENDING_INDEPENDENT_QA_AND_DISTRIBUTION_DISPOSITION';SourceInventorySha256=(Get-FileHash -LiteralPath $SourceInventory).Hash;Files=$entries;LicenseMapping=$legal}
[IO.File]::WriteAllText("$package/package-manifest.json", ($manifest | ConvertTo-Json -Depth 8), [Text.UTF8Encoding]::new($false))
$verify = Start-Process -FilePath "$package/PERTAMINA-OTO.exe" -ArgumentList '--verify' -WindowStyle Hidden -PassThru -Wait
if ($verify.ExitCode -ne 0) { throw 'Launcher package integrity verification failed.' }
$zip = Join-Path $Output "PERTAMINA-OTO-Portable-Windows-x64-$BuildId.zip"
# Stable ordering and timestamps make archive bytes reproducible for identical inputs.
$archive = [IO.Compression.ZipFile]::Open($zip,[IO.Compression.ZipArchiveMode]::Create)
try {
 foreach ($file in Get-ChildItem -LiteralPath $package -Recurse -File -Force | Sort-Object FullName) {
  $entry = $archive.CreateEntry('PERTAMINA-OTO/' + $file.FullName.Substring($package.Length+1).Replace('\','/'),[IO.Compression.CompressionLevel]::Optimal)
  $entry.LastWriteTime = [DateTimeOffset]::new(2026,1,1,0,0,0,[TimeSpan]::Zero)
  $stream = $entry.Open(); $inputFile = [IO.File]::OpenRead($file.FullName)
  try { $inputFile.CopyTo($stream) } finally { $inputFile.Dispose(); $stream.Dispose() }
 }
} finally { $archive.Dispose() }
$archive = [IO.Compression.ZipFile]::OpenRead($zip)
try {
 foreach ($entry in $archive.Entries) {
  $file = Join-Path $Output $entry.FullName; $stream=$entry.Open(); $sha=[Security.Cryptography.SHA256]::Create()
  try { $hash=[BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','') } finally { $stream.Dispose();$sha.Dispose() }
  if ($hash -cne (Get-FileHash -LiteralPath $file).Hash) { throw 'ZIP integrity mismatch.' }
 }
 if ($archive.Entries.Count -ne $entries.Count + 1) { throw 'ZIP count mismatch.' }
} finally { $archive.Dispose() }
$result = @{BuildId=$BuildId;ProductVersion=$product.ProductVersion;Channel=$product.Channel;FileName=(Split-Path $zip -Leaf);Size=(Get-Item -LiteralPath $zip).Length;Sha256=(Get-FileHash -LiteralPath $zip).Hash;ManifestSha256=(Get-FileHash -LiteralPath "$package/package-manifest.json").Hash;Files=$entries.Count;RuntimeBundled=[bool]$RuntimeRoot;ReleaseGate=$manifest.ReleaseGate}
[IO.File]::WriteAllText("$Output/artifact.json", ($result | ConvertTo-Json), [Text.UTF8Encoding]::new($false))
$result | ConvertTo-Json
