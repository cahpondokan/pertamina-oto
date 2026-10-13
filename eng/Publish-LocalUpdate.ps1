param([Parameter(Mandatory=$true)][string]$BuildOutput,
      [string]$Feed = 'D:/AFT Ahmad Yani Semarang/Teknik/Dokumentasi Pertamina OTO/releases/windows-local')
$ErrorActionPreference = 'Stop'
$source = (Resolve-Path -LiteralPath $BuildOutput).Path
if (!(Test-Path -LiteralPath "$source/Pertamina.Oto.Desktop.App.exe") -or !(Test-Path -LiteralPath "$source/worker/Pertamina.Oto.LocalPdf.Worker.exe")) { throw 'Build belum lengkap.' }
$product = Get-Content -LiteralPath "$source/product-info.json" -Raw | ConvertFrom-Json
if ($product.ProductVersion -cnotmatch '^(0|[1-9][0-9]{0,5})\.(0|[1-9][0-9]{0,5})\.(0|[1-9][0-9]{0,5})$' -or $product.Channel -cnotin @('Pengembangan','Stabil')) { throw 'Identitas produk build tidak valid.' }
$binaryProduct = [Diagnostics.FileVersionInfo]::GetVersionInfo("$source/Pertamina.Oto.Desktop.App.dll").ProductVersion.Split('+')[0]
if ($binaryProduct -cne $product.ProductVersion) { throw 'Versi produk binary dan metadata berbeda.' }
$version = (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0,8)
New-Item -ItemType Directory -Path $Feed -Force | Out-Null
$package = Join-Path $Feed $version
New-Item -ItemType Directory -Path "$package/app" | Out-Null
$entries = @()
foreach ($file in Get-ChildItem -LiteralPath $source -Recurse -File) {
    $relative = $file.FullName.Substring($source.Length + 1).Replace('\','/')
    if ($relative -match '(^|/)(live-editor-audit-|.*-audit-)' -or $relative -eq 'build-info.json') { continue }
    if ($file.Extension -eq '.pdf' -or $file.Name -eq 'audit.txt') { throw 'Gunakan output build bersih, bukan folder dokumen/audit.' }
    if (($file.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { throw 'Link file tidak diizinkan.' }
    $target = Join-Path "$package/app" $relative
    New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
    Copy-Item -LiteralPath $file.FullName -Destination $target
    $entries += @{ Path=$relative; Length=$file.Length; Sha256=(Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash }
}
$buildInfo = Join-Path "$package/app" 'build-info.json'
[IO.File]::WriteAllText($buildInfo, (@{Version=$version;ManifestSha256='';ProductVersion=$product.ProductVersion;Channel=$product.Channel} | ConvertTo-Json), [Text.UTF8Encoding]::new($false))
$entries += @{Path='build-info.json';Length=(Get-Item -LiteralPath $buildInfo).Length;Sha256=(Get-FileHash -LiteralPath $buildInfo).Hash}
$manifest = Join-Path $package 'manifest.json'
[IO.File]::WriteAllText($manifest, (@{Version=$version;Files=$entries;ProductVersion=$product.ProductVersion;Channel=$product.Channel} | ConvertTo-Json -Depth 5), [Text.UTF8Encoding]::new($false))
$pending = Join-Path $Feed ('latest-' + [Guid]::NewGuid().ToString('N') + '.json')
[IO.File]::WriteAllText($pending, (@{Version=$version;ManifestSha256=(Get-FileHash -LiteralPath $manifest).Hash;ProductVersion=$product.ProductVersion;Channel=$product.Channel} | ConvertTo-Json), [Text.UTF8Encoding]::new($false))
$latest = Join-Path $Feed 'latest.json'
if (Test-Path -LiteralPath $latest) { [IO.File]::Replace($pending,$latest,(Join-Path $Feed 'previous-feed.json')) } else { [IO.File]::Move($pending,$latest) }
Write-Output "Paket lokal tersedia: $version ($($entries.Count) file). Gunakan Periksa update di aplikasi."
