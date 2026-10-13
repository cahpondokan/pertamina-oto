param(
    [string]$Feed = 'D:/AFT Ahmad Yani Semarang/Teknik/Dokumentasi Pertamina OTO/releases/windows-local',
    [string]$Output,
    [string[]]$Notes = @('Periksa, unduh, verifikasi dan pasang update melalui aplikasi.', 'Panduan pemakaian Windows tersedia di website OTO.', 'Pencarian PDF per kemunculan dengan sorotan dan navigasi posisi.')
)
$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
if (!$Output) { $Output = Join-Path $project 'artifacts/web-distribution' }
$Output = [IO.Path]::GetFullPath($Output)
$release = Get-Content -LiteralPath (Join-Path $Feed 'latest.json') -Raw | ConvertFrom-Json
if ($release.Version -cnotmatch '^[0-9]{8}-[0-9]{6}-[a-f0-9]{8}$') { throw 'Versi feed tidak valid.' }
if ($Notes.Count -lt 1 -or $Notes.Count -gt 8 -or @($Notes | Where-Object { !$_.Trim() -or $_.Length -gt 240 -or $_ -match '[\x00-\x1f\x7f]' }).Count) { throw 'Catatan rilis tidak valid.' }
$package = Join-Path $Feed $release.Version
$manifestPath = Join-Path $package 'manifest.json'
if ((Get-FileHash -LiteralPath $manifestPath).Hash -ne $release.ManifestSha256) { throw 'Hash manifest tidak cocok.' }
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$appConfig = Get-Content -LiteralPath "$package/app/Pertamina.Oto.Desktop.App.runtimeconfig.json" -Raw | ConvertFrom-Json
$selfContained = @($appConfig.runtimeOptions.includedFrameworks | Where-Object { $_.name -eq 'Microsoft.NETCore.App' }).Count -eq 1
if ($manifest.Version -ne $release.Version) { throw 'Versi manifest tidak cocok.' }
foreach ($entry in $manifest.Files) {
    if (!$entry.Path -or $entry.Path -match '(^[/\\])|:|(^|[/\\])\.{1,2}([/\\]|$)|[. ]([/\\]|$)' -or [IO.Path]::IsPathRooted($entry.Path)) { throw 'Lokasi file tidak valid.' }
    $path = Join-Path "$package/app" $entry.Path
    for ($part = [IO.Path]::GetFullPath($path); $part; $part = [IO.Path]::GetDirectoryName($part)) {
        if ((Test-Path -LiteralPath $part) -and ((Get-Item -LiteralPath $part -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw 'Link tidak diizinkan.' }
    }
    if ((Get-Item -LiteralPath $path).Length -ne $entry.Length -or (Get-FileHash -LiteralPath $path).Hash -ne $entry.Sha256) { throw 'Isi feed rusak.' }
}
$staging = Join-Path $project ('artifacts/web-package-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $staging -Force | Out-Null
foreach ($name in @('WebDistributionInstaller', 'LocalLauncher')) {
    $csproj = Join-Path $PSScriptRoot "$name/$name.csproj"
    if ($selfContained) {
        if ($name -eq 'LocalLauncher') { continue } # Copy the already tested immutable launcher below.
        & "$PSScriptRoot/Publish-SelfContainedComponent.ps1" -ProjectFile $csproj -Output $staging
        & "$PSScriptRoot/Test-SelfContainedPayload.ps1" -Directory $staging -AssemblyName 'Setup'
        continue
    }
    & dotnet restore $csproj -p:RestoreSources=
    if ($LASTEXITCODE -ne 0) { throw 'Restore gagal.' }
    $target = if ($name -eq 'LocalLauncher') { Join-Path $staging 'launcher' } else { $staging }
    & dotnet publish $csproj --no-restore -c Release -o $target
    if ($LASTEXITCODE -ne 0) { throw 'Publish installer gagal.' }
}
# Only these runtime files are part of the stable launcher contract.
# Prefer the exact launcher already verified inside the immutable application manifest.
$bundledLauncher = Join-Path "$package/app" 'restart-launcher'
if (Test-Path -LiteralPath (Join-Path $bundledLauncher 'restart-capability.json')) {
    $capability = Get-Content -LiteralPath (Join-Path $bundledLauncher 'restart-capability.json') -Raw | ConvertFrom-Json
    New-Item -ItemType Directory -Path "$staging/launcher" -Force | Out-Null
    foreach ($name in @($capability.Files.Path) + @('restart-capability.json')) {
        if ($name -match '[/\\:]|^\.' -or $name -match '[. ]$') { throw 'Invalid launcher path.' }
        Copy-Item -LiteralPath (Join-Path $bundledLauncher $name) -Destination (Join-Path "$staging/launcher" $name) -Force
    }
}
if ($selfContained) {
    foreach ($component in @(@{Folder='';Assembly='Pertamina.Oto.Desktop.App'},@{Folder='worker';Assembly='Pertamina.Oto.LocalPdf.Worker'},@{Folder='word-worker';Assembly='Pertamina.Oto.WordPdf.Worker'},@{Folder='pdf-opener';Assembly='PertaminaOTO.PdfOpener'},@{Folder='restart-launcher';Assembly='PERTAMINA-OTO'})) {
        & "$PSScriptRoot/Test-SelfContainedPayload.ps1" -Directory (Join-Path "$package/app" $component.Folder) -AssemblyName $component.Assembly
    }
    if ($capability.Protocol -ne 2) { throw 'Self-contained launcher capability is missing.' }
}
$launchers = @()
$launcherNames = if ($capability) { @($capability.Files.Path) } else { @('PERTAMINA-OTO.exe', 'PERTAMINA-OTO.dll', 'PERTAMINA-OTO.deps.json', 'PERTAMINA-OTO.runtimeconfig.json') }
foreach ($name in $launcherNames) {
    $file = Get-Item -LiteralPath (Join-Path "$staging/launcher" $name)
    $launchers += @{Path=$name;Length=$file.Length;Sha256=(Get-FileHash -LiteralPath $file.FullName).Hash}
}
[IO.File]::WriteAllText("$staging/launcher-manifest.json", ($launchers | ConvertTo-Json), [Text.UTF8Encoding]::new($false))
New-Item -ItemType Directory -Path "$staging/feed/$($release.Version)/app" -Force | Out-Null
Copy-Item -LiteralPath "$Feed/latest.json" -Destination "$staging/feed/latest.json"
Copy-Item -LiteralPath $manifestPath -Destination "$staging/feed/$($release.Version)/manifest.json"
foreach ($entry in $manifest.Files) {
    $target = Join-Path "$staging/feed/$($release.Version)/app" $entry.Path
    New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path "$package/app" $entry.Path) -Destination $target
}
$prerequisite = if ($selfContained) { 'Runtime .NET 10 sudah disertakan di folder OTO. Tidak perlu diunduh atau dipasang terpisah ke Windows.' } else { 'Pasang .NET 10 Runtime x64 dari https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime jika belum tersedia.' }
[IO.File]::WriteAllText("$staging/BACA-DULU.txt", @"
PERTAMINA OTO — Windows x64 — Versi pengembangan
Build $($release.Version)

1. $prerequisite
2. Ekstrak SELURUH ZIP ke folder lokal, lalu jalankan Setup.exe.
3. Ketik PASANG ketika diminta. Installer tidak menutup aplikasi atau menghapus dokumen.
4. Simpan pekerjaan, tutup OTO, lalu buka kembali shortcut PERTAMINA OTO.

Ini juga jalur transisi sekali dari aplikasi lama ke updater online.
Sesudah transisi: Pengaturan > Periksa update > Unduh dan pasang jika tersedia.
Jika sudah terbaru, aplikasi memberi tahu; internet gagal tidak menghalangi pemrosesan lokal.
Simpan pekerjaan dan buka kembali shortcut bila diminta. Shortcut dan pengaturan tetap digunakan.
Panduan pemakaian: https://oto.ayani.id/panduan
Paket tidak mendaftarkan default PDF secara otomatis; gunakan Pengaturan di OTO.
Versi pengembangan belum ditandatangani. Penggunaan produksi memerlukan persetujuan rilis tersendiri.
Hash memeriksa keutuhan paket, bukan identitas penerbit.
"@, [Text.UTF8Encoding]::new($false))
& "$staging/Setup.exe" --verify $staging
if ($LASTEXITCODE -ne 0) { throw 'Verifikasi installer gagal.' }
New-Item -ItemType Directory -Path $Output -Force | Out-Null
$fileName = "PERTAMINA-OTO-Windows-x64-$($release.Version).zip"
$zip = Join-Path $Output $fileName
if (Test-Path -LiteralPath $zip) { throw 'Paket versi ini sudah ada; versi tidak boleh ditimpa.' }
# Exclude build symbols and packager metadata; include only explicit package contents.
$archive = [IO.Compression.ZipFile]::Open($zip, [IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($file in Get-ChildItem -LiteralPath $staging -Recurse -File) {
        $relative = $file.FullName.Substring($staging.Length + 1).Replace('\','/')
        if ($relative.EndsWith('.pdb') -and !$relative.StartsWith('feed/')) { continue }
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $file.FullName, $relative, [IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
} finally { $archive.Dispose() }
$catalog = @{schema=1;version=$release.Version;publishedAt=[DateTimeOffset]::UtcNow.ToString('O');channel='development';fileName=$fileName;size=(Get-Item -LiteralPath $zip).Length;sha256=(Get-FileHash -LiteralPath $zip).Hash;notes=$Notes}
$catalog.runtimeBundled = $selfContained
if ($selfContained) {
    # A raw private-runtime ZIP is an intermediate candidate, not a legacy update offer.
    # The full installer builder fills the required installer tuple; until then old
    # schema-1 readers fail closed rather than treating this as seamless migration.
    $catalog.manualInstallerOnly = $true
}
if ($release.ProductVersion) { $catalog.productVersion = $release.ProductVersion }
$notesFile = Join-Path "$package/app" 'Assets/release-notes.json'
if (Test-Path -LiteralPath $notesFile) {
    if (@($manifest.Files | Where-Object { $_.Path -ceq 'Assets/release-notes.json' }).Count -ne 1) { throw 'Catatan rilis harus terikat manifest.' }
    if ((Get-Item -LiteralPath $notesFile).Length -gt 65536) { throw 'Catatan rilis terlalu besar.' }
    $notesJson = [IO.File]::ReadAllText($notesFile)
    $notesCatalog = $notesJson | ConvertFrom-Json
    if ($notesCatalog.Schema -ne 1 -or $notesCatalog.ProductVersion -cne $release.ProductVersion) { throw 'Identitas catatan tidak cocok.' }
    $catalog.notes = @($notesCatalog.Releases[0].Summary)
    $catalog.releaseNotesJson = $notesJson
    $catalog.releaseNotesSha256 = (Get-FileHash -LiteralPath $notesFile).Hash
}
$pending = Join-Path $Output ('current-' + [Guid]::NewGuid().ToString('N') + '.json')
$catalogJson = $catalog | ConvertTo-Json -Depth 8
if ([Text.Encoding]::UTF8.GetByteCount($catalogJson) -gt 16384) { throw 'Metadata melebihi batas client online.' }
[IO.File]::WriteAllText($pending, $catalogJson, [Text.UTF8Encoding]::new($false))
$current = Join-Path $Output 'current.json'
if (Test-Path -LiteralPath $current) { [IO.File]::Replace($pending, $current, (Join-Path $Output 'previous.json')) } else { [IO.File]::Move($pending, $current) }
Write-Output "Paket unduhan lokal: $zip"
Write-Output "SHA256: $($catalog.sha256)"
