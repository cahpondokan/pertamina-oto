# Versi pengembangan: fungsi utama berdasarkan prototipe

Instruksi pengguna 8 September 2026: semua fungsi fitur utama harus bisa digunakan
lebih dahulu berdasarkan prototipe. Baseline UI adalah `uiux/integrator`, bukan
branch repository-anchor yang hanya berisi layar awal. Daftar scope: 33 kartu pada
`uiux/prototype/shared/core-tools.ts` dan dokumen 03 bagian 4.1.

Agent 0 mengerjakan branch `task/core-functional-first`; main tidak dipromosikan.
Ruang tulis: host Desktop.App, LocalPdf.Worker, tests terkait, dependency composition,
dan catatan implementasi ini. Preferensi hemat resource: tidak ada dispatch agen
otomatis atau pengulangan Sandbox. Tidak ada API processing/server/cloud.

Urutan implementasi mengikuti reuse: operasi halaman dan gambar; edit/keamanan;
konversi/OCR; scanner dan intelligence. Kriteria setiap tool adalah input asli dari
picker, parameter berfungsi, proses nyata, hasil dapat disimpan dan dibuka, error
yang jelas, serta sumber tidak tertimpa. Simulasi tidak dihitung sebagai fungsi.

Komponen awal hanya paket existing: PDFsharp 6.2.4 + tiga dependency exact yang
sudah diinventarisasi, serta Windows App SDK 1.7.250909003 yang sudah ada pada cache
pengembangan. Lisensi SDK bagian 1a mengizinkan development/test di Windows; tidak
ada acquisition atau komitmen berbayar. Resolusi dependency harus offline dan gagal
jika paket belum tersedia. Review rilis/distribusi tetap terpisah.

Pembatasan development: PDFsharp masih memiliki finding malformed page-tree.
Worker terpisah diperlukan agar kegagalan engine tidak mematikan UI. Pengujian
otomatis hanya memakai fixture sintetis. Jangan mengklaim validasi redaction,
PDF/A, OCR, AI, atau fidelity hanya karena file berhasil disimpan.

## Build yang dapat dicoba — 8 September 2026

Executable: `artifacts/desktop/Pertamina.Oto.Desktop.App.exe` (Windows x64,
.NET 10 existing). WinUI self-contained dari cache existing; bukan installer,
tidak mendaftarkan service, tidak membuka server, tidak membutuhkan Hostinger.
Tampilan mengikuti katalog/kelompok prototipe, bukan salinan pixel-perfect.

Build: `./build-desktop.ps1 -RestoreOffline`; uji sintetis: tambahkan `-Test`.
Script hanya memakai sumber lokal di `dev-offline.config`; konfigurasi ini khusus
mesin pengembangan saat ini. Tidak ada fallback download/install. Gunakan MSBuild
Visual Studio untuk resource WinUI, bukan `dotnet build` untuk host.

Alur: pilih alat → tambah file → ubah urutan/opsi → pilih folder hasil dan nama
baru → proses → buka hasil. Nama existing selalu ditolak. Split menghasilkan ZIP
satu PDF per halaman terpilih. Password dikirim lewat anonymous stdin pipe,
tidak melalui argumen command line dan tidak ditulis ke history/log.

| Operasi dasar aktif | Batas cakupan saat ini |
|---|---|
| Merge | Urutan file; hasil baru; preservasi form/bookmark/signature belum teruji |
| Split | Satu halaman per PDF dalam ZIP; belum split range-group/batch size |
| Remove / Extract | Pemilihan nomor/rentang; minimal satu halaman tersisa |
| Organize | Urutan, duplikasi, sisihkan halaman; belum editor thumbnail/insert blank |
| JPG to PDF | JPG/PNG ke A4, fit; satu gambar per halaman |
| Rotate | 90/180/270 derajat pada halaman terpilih |
| Crop | Margin seragam terhadap CropBox; tidak menghapus data |
| Page numbers / Watermark | Footer nomor / teks tengah; belum editor posisi bebas |
| Compress | Stream lossless, bukan downsample; bisa lebih besar |
| Protect / Unlock | AES-256; unlock hanya kata sandi pemilik sah |

**13 kartu memiliki operasi dasar, bukan 13 fitur kanonik COMPLETE.** Dua puluh
kartu lain menampilkan alasan belum aktif dan tidak memproduksi hasil simulasi:
scan, repair, OCR, Word/PPT/Excel/HTML→PDF, PDF→JPG/Word/PPT/Excel/PDF-A,
edit, forms, sign, redact, compare, summary, translate, Markdown.

## Bukti dan residual

- MSBuild VS2026 host build PASS; startup awal gagal karena metadata XAML belum
  tergenerasi, diperbaiki dengan App.xaml standar; startup aktual kini PASS.
- Computer Use mengamati katalog berikon, halaman Merge, serta dialog pemilih file.
  Automated GUI save end-to-end **belum dibuktikan**. Saat pengguna berinteraksi
  dengan picker, Agent0 tidak memproses pilihan pengguna dan melanjutkan uji sintetis.
- Worker self-test: 23 pemeriksaan PASS; artefak sementara
  `C:/Users/cahpo/AppData/Local/Temp/oto-functional-test-299b545f77e649ee89a5a3e34d026e46`.
- Harness memakai WorkerClient yang sama dengan UI: 21 pemeriksaan PASS;
  `C:/Users/cahpo/AppData/Local/Temp/oto-client-test-0a0b274cc6214c39a4936bef715a1243/results.json`.
  Salinan evidence: `artifacts/verification/client-results.json` dalam worktree.
  Percobaan fixture PNG base64 awal gagal; diganti generator PNG RGB valid dengan
  checksum chunk, lalu pengujian lengkap lulus. Tidak melemahkan validator.
- Source SHA256 tetap sama; wrong-password/malformed/empty-result/source-target/
  existing-target ditolak; pre-start cancellation tidak menerbitkan output;
  tidak ada stage tersisa pada seluruh kasus uji. In-flight cancellation, crash
  recovery, visual fidelity kompleks, disk-full, dan malicious PDF belum lengkap.
- Worker memakai timeout 2 menit, polling memori 1 GB, input 100 MB/1000 halaman,
  read locks, output stage unik dan atomic move create-new. Pemisahan proses bukan
  sandbox: belum ada OS-level network/filesystem/memory sandbox atau crash job-object.
- Input tidak diunggah oleh kode processing. Shell picker dapat menampilkan lokasi
  cloud Windows; pilih disk lokal bila ingin benar-benar tanpa akses jaringan.
- History hanya memori sesi, belum metadata/recovery persisten. Development host
  belum dihubungkan melalui semua port MergeService foundation; integrasi arsitektur,
  canonical feature acceptance, review independen, dan gate rilis masih pekerjaan terbuka.
- Tidak ada phase dipromosikan COMPLETE, tidak ada main merge, deployment, biaya,
  dependency download, restart, atau Sandbox tambahan pada checkpoint ini.
