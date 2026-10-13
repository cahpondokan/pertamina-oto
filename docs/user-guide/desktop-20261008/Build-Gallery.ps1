$ErrorActionPreference = 'Stop'
$checkout = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$out = Join-Path $checkout 'artifacts/user-guide-20261008'
New-Item -ItemType Directory -Force -Path $out | Out-Null
$catalog = Get-Content (Join-Path $checkout 'src/Hosts/Pertamina.Oto.Desktop.App/ToolCatalog.cs') -Raw
$tools = @([regex]::Matches($catalog, 'new\("([^"]+)", "([^"]+)", "([^"]+)", "[^"]+", "([^"]+)"(, true)?\)') | ForEach-Object {
    [ordered]@{ id=$_.Groups[1].Value; title=$_.Groups[2].Value; group=$_.Groups[3].Value; description=$_.Groups[4].Value; available=$_.Groups[5].Success; inventoryEvidence='ToolCatalog.cs'; captureStatus= $(if ($_.Groups[5].Success) {'PENDING_NATIVE_CAPTURE'} else {'NOT_APPLICABLE_UNAVAILABLE'}) }
})
$release = Join-Path (Split-Path (Split-Path $checkout)) 'releases/windows-local/20261008-084354-201e4764'
$mf = Get-Content (Join-Path $release 'manifest.json') -Raw | ConvertFrom-Json
$checks = @($mf.Files | ForEach-Object { $p=Join-Path (Join-Path $release 'app') $_.Path; [ordered]@{path=$_.Path; pass=((Test-Path -LiteralPath $p) -and (Get-Item -LiteralPath $p).Length -eq $_.Length -and (Get-FileHash -LiteralPath $p).Hash -eq $_.Sha256)} })
$baseline=[ordered]@{build='20261008-084354-201e4764'; version='0.2.2'; channel='Pengembangan'; manifestSha256=(Get-FileHash (Join-Path $release 'manifest.json')).Hash; dllSha256=(Get-FileHash (Join-Path $release 'app/Pertamina.Oto.Desktop.App.dll')).Hash; payloadCount=$checks.Count; payloadFailed=@($checks | Where-Object {-not $_.pass}).Count; installedIdentity='NOT_INSPECTED'; capturedFromBaseline=$false}
$baseline | ConvertTo-Json -Depth 5 | Set-Content -Encoding utf8 (Join-Path $out 'baseline-verification.json')
$pages=@(
    @('home','Beranda','Mulai dari navigasi dan pilih fitur.'),
    @('catalog','Semua Alat','Pilih alat aktif; kartu belum aktif tidak didemokan.'),
    @('reader','Pembaca PDF','Pilih PDF contoh; baca, gulir, zoom, dan cari teks per kemunculan.'),
    @('editor','Edit PDF — dokumen terbuka','Edit konten yang didukung lalu simpan salinan; batas font dan dokumen scan dijelaskan.'),
    @('result','Hasil dan penyimpanan','Periksa hasil, simpan salinan, buka hasil atau tampilkan file terpilih di folder.'),
    @('print','Cetak — dialog','Periksa preview dan opsi; dokumentasi tidak mengirim print job.'),
    @('history','Riwayat','Temukan keluaran lokal dan buka dokumen yang tersedia.'),
    @('favorites','Favorit','Akses alat favorit dari profil contoh terisolasi.'),
    @('secured','File Diamankan','Periksa daftar lokal; gunakan hanya fixture contoh.'),
    @('settings','Pengaturan','Tinjau preferensi; jangan mengubah pengaturan pengguna.'),
    @('update','Periksa update / catatan rilis','Dokumentasikan label dan batas koneksi tanpa menjalankan unduh/pasang.'),
    @('help','Bantuan','Petunjuk penggunaan dan batas kemampuan yang tersedia.'),
    @('assistant','Assistant','Tinjau halaman; jangan mengklaim AI lokal telah tersedia.'),
    @('admin','Administrasi','Tinjau halaman; jangan melakukan transaksi administrasi.'),
    @('about','Tentang aplikasi','Periksa identitas versi dan informasi aplikasi.')
) | ForEach-Object { [ordered]@{id=$_[0]; title=$_[1]; instructions=$_[2]; captureStatus='PENDING_NATIVE_CAPTURE'; evidence='MainWindow.cs / docs/development'; states=@('empty','loaded','success','error','modal'); stateSelection='Pilih hanya state yang relevan; bukan seluruh kombinasi.'} }
$manifest=[ordered]@{schemaVersion=1; date='2026-10-08'; stageA='INVENTORY_READY_FINAL_CAPTURE_WAITING_ON_TASK04'; finalTarget=[ordered]@{policy='Latest final desktop build after task04 minimum implementation; website release excluded';status='WAITING_ON_TASK04';build=$null;dllSha256=$null;allFinalCapturesSameBuild=$true}; stageB='WAITING_ON_TASK04'; minimumDip=$null; baseline=$baseline; activeToolCount=@($tools|Where-Object available).Count; unavailableToolCount=@($tools|Where-Object {-not $_.available}).Count; tools=$tools; pages=$pages; captures=@(); targetModes=@('maximized','snap-left','snap-right','minimum-exact'); nativeCaptureRequirements=@('physicalMonitorPx','workAreaPx','windowsScale','rasterizationScale','outerPx','clientDip','targetPid','binaryPath','dllSha256','build','route','state','fixture','imageWidth','imageHeight','imageSha256','nativeFrameIncluded','visualReview','layoutStatus'); limitations=@('Source inventory is not end-to-end acceptance.','No new native screenshots until NativeUiOwner task05.','Minimum requires DocsMinimumReady and justified final frozen DLL.','Fixed-root client rendering is not native Snap evidence.','PPT requires gallery review.')}
$manifest | ConvertTo-Json -Depth 10 | Set-Content -Encoding utf8 (Join-Path $out 'manifest.json')
$rows=@($pages | ForEach-Object {[pscustomobject]@{id=$_.id;title=$_.title;maximized='PENDING';snapLeft='PENDING';snapRight='PENDING';minimum='WAITING_ON_TASK04';reason='Native UI lease task04; no screenshot assumed'}})+@($tools|ForEach-Object {[pscustomobject]@{id=$_.id;title=$_.title;maximized=$_.captureStatus;snapLeft=$_.captureStatus;snapRight=$_.captureStatus;minimum=$(if($_.available){'WAITING_ON_TASK04'}else{'NOT_APPLICABLE_UNAVAILABLE'});reason=$_.description}})
$rows | Export-Csv -NoTypeInformation -Encoding utf8 (Join-Path $out 'coverage.csv')
$cards=($pages | ForEach-Object {'<article><h2>'+[Net.WebUtility]::HtmlEncode($_.title)+'</h2><p>'+[Net.WebUtility]::HtmlEncode($_.instructions)+'</p><p class="pending">PENDING — gambar native belum diambil.</p></article>'}) -join "`n"
$table=($tools | ForEach-Object {'<tr><td>'+[Net.WebUtility]::HtmlEncode($_.title)+'</td><td>'+$(if($_.available){'Aktif dalam katalog'}else{'Belum tersedia'})+'</td><td>'+[Net.WebUtility]::HtmlEncode($_.description)+'</td></tr>'}) -join "`n"
$html=@"
<!doctype html><html lang="id"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>PERTAMINA OTO — Galeri review desktop</title><style>body{margin:0;background:#edf3fa;color:#102b50;font:16px/1.6 Segoe UI,Arial}main{max-width:1400px;margin:auto;padding:24px}header,article,section{background:white;border:1px solid #d9e4ef;border-radius:14px;padding:24px;margin:20px 0}h1{font-size:30px}h2{font-size:21px}.pending{border-left:4px solid #c58a20;padding:8px 16px}a{color:#126aca}.table{overflow:auto}table{border-collapse:collapse;width:100%;min-width:600px}td,th{padding:12px;text-align:left;border-bottom:1px solid #d9e4ef}img{max-width:100%;height:auto}</style><main><header><h1>PERTAMINA OTO · Dokumentasi Windows</h1><p>8 Oktober 2026 · Galeri persiapan review. Belum ada screenshot baru.</p><p>Baseline historis saja: 0.2.2 / Pengembangan · build 20261008-084354-201e4764 · $($baseline.payloadCount) payload diperiksa; $($baseline.payloadFailed) gagal.</p><p>Target: versi aplikasi terakhir setelah ukuran minimum selesai, bukan versi website. Semua screenshot final maksimal/Snap/minimum menunggu DLL final dan slot UI task04. Minimum: WAITING_ON_TASK04; angka tidak ditentukan oleh dokumentasi.</p><p><a href="manifest.json">Manifest</a> · <a href="coverage.csv">Matriks cakupan</a> · <a href="baseline-verification.json">Bukti baseline</a></p></header>$cards<section><h2>Katalog aktual: $($manifest.activeToolCount) aktif / $($manifest.unavailableToolCount) belum tersedia</h2><div class="table"><table><thead><tr><th>Alat</th><th>Status</th><th>Fungsi dan batasan</th></tr></thead><tbody>$table</tbody></table></div></section></main></html>
"@
$html | Set-Content -Encoding utf8 (Join-Path $out 'gallery.html')
if($baseline.payloadFailed -ne 0){throw 'Baseline payload mismatch'}
Write-Output "Inventory: $($tools.Count) tools, $($pages.Count) pages/states; baseline $($checks.Count) payload PASS"


