# Paket website 0.2.4 untuk uji laptop lain

Pada 9 Oktober 2026 paket **0.2.4 · Pengembangan telah diunggah dan dipublikasikan di https://oto.ayani.id** setelah pengguna menyetujui penggantian unduhan publik. Publikasi selesai pada 13:00 WIB; verifikasi HTTPS dari Windows lulus. Pengujian pada laptop lain belum dilakukan dan ini bukan persetujuan Production. Biner desktop tidak dibangun ulang: EXE/ZIP identik dengan kandidat yang diverifikasi dalam [SELF_CONTAINED_PACKAGE.md](SELF_CONTAINED_PACKAGE.md).

## Identitas dan lokasi

- Produk: **0.2.4 · Pengembangan**; build `20261009-113931-5d181036`.
- Folder paket, relatif ke parent workspace: `releases/web-upload/0.2.4-20261009-113931-5d181036`.
- ZIP administrator: `releases/web-upload/PERTAMINA-OTO-WebRelease-0.2.4-20261009-113931-5d181036.zip`, **590,570,883 byte**, SHA256 `0F921FE2F5A8D03D82161B328D8513C32E92A4815B711666669D401377A4BCC6`. Sembilan file top-level diverifikasi ulang setelah kompresi; folder `contents` tidak digandakan di ZIP ini.
- `payload.tar.gz`: **590,554,604 byte**, SHA256 `4188165ea4d55dd2c6bff71d93d6e35ed93b4789219ed01f05f7d227c50075e1`.
- Publication metadata SHA256: `5232c226272c1388a6349f0753ebb12695c6e27c59200edbad3cb4e05f998be0`.
- EXE SHA256 tetap `9A21DE52C0C31DB92ADDDB0217CEE31ACE0EB8BEB4F49F9FCEF14B33E6729A8F`; ZIP SHA256 tetap `F8660AA420ED9F132881ACE182F1407B5B83767CBBE748CE26C4AB30C25A82A4`.
- Arsip memuat 32 payload yang terdaftar, ditambah metadata dan helper: 34 file reguler, tanpa link atau file tambahan.

## Perubahan terbatas pada distribusi web

Halaman unduhan dan petunjuk pemasangan menjelaskan runtime privat, instalasi offline setelah EXE diunduh, serta transisi memakai installer lengkap sekali. Panduan menyertakan checklist `/panduan#uji-laptop`. CSS, gambar panduan, dan desain dasar situs dipertahankan; gambar lama diberi penjelasan sebagai contoh build sebelumnya. Tidak ada perubahan implementasi preview, zoom, editor, print atau proses dokumen desktop.

Katalog web menambahkan `manualInstallerOnly=true` dan mempertahankan `runtimeBundled=true`. Karena updater 0.2.3 tidak mendukung launcher protocol 2, endpoint `/api/releases/windows-x64/latest` memberikan **204 untuk seluruh klien selama rilis manual ini aktif**. Website dan `/api/releases/windows-x64/installer` tetap menyediakan metadata/tautan EXE dan ZIP yang terverifikasi. Ini mencegah penawaran paket yang tidak kompatibel kepada updater lama, bukan kegagalan jaringan. Jangan mempromosikan paket ini ke feed lokal 0.2.3. Rilis berikutnya membutuhkan strategi kompatibilitas yang diuji sebelum menonaktifkan kebijakan manual.

`ReleaseInfo` menambahkan dua field opsional untuk kontrak web. Paket desktop yang sudah dibekukan tidak diubah. ReleaseCatalog tetap memeriksa ukuran/hash, path aman, dan metadata berbatas; endpoint baru bersifat GET read-only dengan no-store. Katalog lama tanpa flag mempertahankan perilaku sebelumnya. Helper publikasi memverifikasi endpoint installer dan memastikan feed updater 204 untuk rilis manual. Pengaman baseline, immutable paths, rollback, dan konfigurasi server tetap dipertahankan.

## Baseline server dan batas otorisasi

Pemeriksaan SSH read-only dengan host key ED25519 tersemat dan GET HTTPS tanpa redirect mengonfirmasi layanan aktif/enabled serta produk publik 0.2.3/build `20261009-035333-e218f12d`. Symlink web: `/opt/oto-downloads/oto-update-20261009-035333-e218f12d/web`. Hash unit/Nginx sama dengan publikasi terdahulu. Bukti baru berada pada `artifacts/web-release-0.2.4-20261009/server-baseline.json` dan `previous-catalog.json`.

Pada pelaksanaan upload, `Update-OTO.ps1 -Action VerifyOnly` dan `-Action Check` lulus. Arsip di staging privat `/home/aynadmin/oto-updates/oto-update-20261009-113931-5d181036-4188165ea4d5` cocok dengan SHA256 lokal; bounded extraction dan seluruh inventory/catalog/guide lulus pada Linux. Direktori staging berizin `700`.

Pengguna kemudian menyetujui "Ya, tampilkan 0.2.4 di website" dan memasukkan password sudo langsung di terminal operator, tanpa password dibaca atau dicatat oleh agent. `Publish-VerifiedUpload.ps1` menjalankan helper yang sudah diunggah setelah memeriksa ulang pin helper/metadata dan bukti upload, tanpa mengunggah ulang paket. Helper melakukan pemeriksaan baseline, backup, pergantian web/katalog, restart hanya `oto-downloads.service`, serta validasi HTTP penuh. Operasi selesai dengan exit code 0 pada `2026-10-09T06:00:23.4291217Z`.

Symlink publik sekarang `/opt/oto-downloads/oto-update-20261009-113931-5d181036/web`; layanan aktif/enabled dan hash konfigurasi unit/Nginx tidak berubah. Direktori web versi sebelumnya masih ada. Pemeriksaan isi direktori lama dari akun non-root dibatasi izin `750 root:oto-web`; percobaan tersebut tercatat, bukan bukti file hilang atau klaim audit isi backup independen. Tidak ada instalasi runtime VPS, perubahan Nginx, atau perubahan aplikasi terpasang. Feed lokal dan pointer aplikasi pengguna tetap produk 0.2.3/build `20261009-035333-e218f12d`.

## Verifikasi

Bukti relatif ke `artifacts/web-release-0.2.4-20261009`:

| Pemeriksaan | Hasil | Bukti |
| --- | --- | --- |
| Build website saja | Lulus, tanpa restore/dependency baru atau build desktop | `web/` |
| Portal sintetis dan kebijakan migrasi | 48 PASS; katalog lama tetap berfungsi, rilis manual diblokir dari updater, tipe flag tidak valid ditolak | `portal-tests-localhost/test-results.txt` |
| Paket asli melalui HTTP lokal | 19 PASS; metadata, EXE/ZIP penuh, SHA256, HEAD/range, panduan dan CSP | `real-portal-results.txt` |
| Helper publikasi | 24 unit tests PASS; aktivasi/ownership dimock pada Windows | `helper-tests.txt` |
| Kontrak distribusi/updater | 63 dan 13 PASS | `distribution-contract-checks/` |
| Browser Chrome headless | Beranda dan panduan pada 1366/960/390 CSS px; tidak overflow horizontal, tidak ada error halaman atau gambar termuat rusak | `browser-check.json` dan PNG |
| Arsip nyata | Seluruh hash/ukuran/path terikat inventory; helper HTTP nyata ke localhost juga lulus | `package-verification.json` |
| Upload ke staging Linux | SHA256 arsip/helper/metadata cocok; ekstraksi berbatas dan inventory lulus | `private-upload-check.txt`, `private-upload-server-proof.txt` |
| Publikasi server | Exit 0 setelah pemeriksaan hash penuh EXE/ZIP melalui localhost dan HTTPS, HEAD/range, metadata dan panduan | `operator-publication-result.json`, helper terikat hash pada paket |
| HTTPS publik dari Windows | 28 PASS; produk/build, kebijakan manual, metadata hash, halaman/panduan, HEAD/attachment/range EXE dan ZIP | `public-verification.json`, `public-verification.txt` |
| Status server dan laptop lokal | Layanan aktif/enabled; unit/Nginx tidak berubah; pointer lokal tetap 0.2.3 | `server-publication-final-access.txt`, `local-installation-check.json` |

Screenshot `home-1366.png`, `download-390.png`, dan `checklist-960.png` diperiksa langsung. Browser bawaan gagal karena kernel assets tidak tersedia; verifikasi menggunakan Chrome headless dengan profil uji terpisah. Ini bukan simulasi screenshot generatif atau UAT DPI fisik.

Percobaan helper pertama gagal pada izin direktori temporary sandbox; ulang dengan temp khusus workspace lulus. Percobaan portal pertama gagal pada izin DataProtection/loopback sandbox; ulang localhost di luar sandbox lulus. Kegagalan awal tidak dihitung sebagai kelulusan dan bukti tetap tersedia. Tidak ada penonaktifan kontrol keamanan.

## Langkah berikutnya

Unduh installer lengkap dari https://oto.ayani.id/#unduh pada laptop uji dan ikuti https://oto.ayani.id/panduan#uji-laptop. Transisi 0.2.3 ke 0.2.4 memakai installer lengkap, bukan updater lama. PC/VM bersih tanpa .NET 10, instalasi nyata pada laptop lain, daftar aplikasi setelah instalasi, DPI fisik dan printer fisik masih belum diuji. Jangan menghapus .NET milik aplikasi lain atau melewati kebijakan keamanan Windows untuk menguji kandidat.

Paket administrator beserta `BACA-DULU-UPLOAD.txt` dan `VERIFICATION.json` tetap dibekukan sesuai kondisi saat paket dibuat; status prapublikasi di dalam arsip bukan status situs terkini. Bukti pelaksanaan terbaru berada pada file-file di atas. Jangan menjalankan aktivasi ulang menggunakan ID yang sama; helper menolak ID publikasi yang sudah terpakai. Rollback hanya melalui otorisasi operator untuk publikasi exact ini dan setelah memeriksa tidak ada rilis/config yang lebih baru.
