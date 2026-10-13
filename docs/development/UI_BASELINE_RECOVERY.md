# Pemulihan tata letak OTO berdasarkan 0.1.0

Status: implementasi dan verifikasi selesai; paket pembaruan lokal **0.1.4 · Pengembangan** tersedia sebagai build `20261004-223811-f0320f84`. Paket belum diaktifkan oleh pekerjaan ini dan belum dipublikasikan online. Pemulihan ini mengikuti pilihan pengguna untuk mengambil tampilan 0.1.0 sebagai pembanding, tanpa membuang perbaikan fungsi terbaru atau melaksanakan redesign yang dibahas sebelumnya.

## Pembanding dan temuan

Paket `20261004-070509-9021ecc9` adalah 0.1.0 yang tercatat sebagai instalasi sebelumnya. Seluruh 99 dokumen sumber C# aplikasi pada portable PDB paket tersebut cocok dengan snapshot `artifacts/responsive-layout/run-20261004/source-baseline`. Ini mengidentifikasi pembanding secara tepat; label 0.1.0 saja tidak cukup karena digunakan oleh beberapa build.

Catatan instalasi pada preflight menunjuk 0.1.1, sedangkan feed lokal sudah menyediakan 0.1.3. Pembacaan ulang sebelum penyerahan menunjukkan pointer instalasi sudah menjadi 0.1.3 (`20261004-211242-4e00f9cf`); pekerjaan ini tidak mengubah pointer tersebut. Pointer bukan bukti versi proses yang sedang berjalan. Tumpang-tindih sumber dan pengaturan sebelumnya disebabkan cache layout yang tidak mengenali penggantian panel dengan jumlah anak yang sama. Perbaikan 0.1.3 terhadap identitas anak serta posisi row/column dipertahankan, bukan dikembalikan ke implementasi lama.

## Perubahan terbatas

- `MainWindow.cs`: pulihkan kepadatan kartu desktop Beranda dan katalog sesuai breakpoint 0.1.0. Kunci cache mengikuti hasil layout yang dihitung. Tetap pertahankan pemeriksaan identitas panel, pemisahan sumber/pengaturan, dan penumpukan pada ruang sempit.
- `WorkspaceScreens.cs`: panel Aktivitas, File Diamankan, dan Assistant kembali rata kiri dengan lebar maksimum 850 DIP. Ukuran dihitung dari lebar induk, sehingga kontrol tetap terbatas pada jendela sempit.
- `EmptyToolScreens.cs`: kembalikan lebar desktop tombol pilih PDF pada Gabung ke 280 DIP, dibatasi ruang yang tersedia.
- `UiRegressionAudit.cs`: tambahkan pemeriksaan breakpoint, kunjungan ulang ukuran, lebar tombol, dan alignment panel. Pertahankan matriks dokumen terisi serta deteksi occlusion sebelumnya.
- Versi produk dan catatan perubahan dinaikkan bersama ke 0.1.4. Riwayat 0.1.1 sampai 0.1.3 dipertahankan.

Kode preview, zoom/cubit, pencarian, editor, lifecycle dokumen, updater, serta website tidak menjadi target perubahan. Pembanding sebelum edit berisi 1.457 file sumber/aset dan metadata; gate membatasi perubahan pada tiga file produksi UI, satu harness DEBUG, serta dua file metadata. Tidak ada reset Git, downgrade menyeluruh, dependency baru, task baru, atau perubahan phase.

## Verifikasi dan gate penerbitan

Bukti berada di `artifacts/ui-baseline-recovery/run-20261004`. `task-card.md` mencatat scope; `before/` dan `before-hashes.json` menyimpan kondisi sebelum edit; `baseline-pdb-proof.json` membuktikan pembanding. `Verified-Build.ps1` menjalankan full Rebuild melalui jalur standar `eng/Build-LocalUpdate.ps1`, lalu menjalankan native regression pada binary tersebut. Penerbitan menunggu review screenshot yang terikat hash DLL. Sumber dan payload bersih diperiksa lagi sebelum melanjutkan gate worker/distribusi standar. Artefak uji sintetis dipindahkan ke folder bukti, bukan dimasukkan ke paket atau dihapus.

Percobaan awal catatan rilis ditolak karena ringkasan hanya dua butir; kontrak meminta minimal tiga. Metadata diperbaiki tanpa melonggarkan validasi. Kandidat UI pertama menemukan overflow panel pendukung pada lebar 480/640; proses audit sintetis itu dihentikan, bukti gagal dipertahankan, dan panel dibatasi terhadap lebar induk. Tidak ada kandidat gagal diterbitkan. Folder kandidat gagal: `artifacts/update-build-4aac9d77af864ca3ae348c4c8740e735`.

## Hasil verifikasi kandidat yang diterbitkan

`native-results.json` merekam **3.126 assertion PASS, 0 FAIL** pada binary yang sama dengan paket. Angka ini jumlah assertion berulang pada skenario uji, bukan jumlah fitur atau perangkat yang disertifikasi.

| Kelompok native | PASS | FAIL |
|---|---:|---:|
| Matriks UI dan pemulihan baseline | 1.581 | 0 |
| Zoom | 63 | 0 |
| Kehalusan render | 53 | 0 |
| Layout responsif | 462 | 0 |
| Pembaca PDF | 479 | 0 |
| Editor | 149 | 0 |
| Preview | 72 | 0 |
| Riwayat | 52 | 0 |
| Restart dan lifecycle pembaruan | 165 | 0 |
| Catatan perubahan | 50 | 0 |

Matriks UI mencakup 120 transisi workspace terisi pada 15 alat bersama dan empat lebar, serta pemeriksaan halaman awal, breakpoint, dan panel pendukung. Review visual mencakup 24 screenshot native; daftar dan observasi ada di `visual-gate.json`. Contoh: `native-evidence/ui-regression-audit/1920-organize.png`, `1366-remove.png`, dan `640-organize.png` di folder bukti.

Jalur standar `eng/Build-LocalUpdate.ps1` selesai dengan exit 0. `delivery.log` memuat pemeriksaan catatan rilis (44), versi produk (25 ditambah 13 legacy updater), paket fixture inert (8), distribusi (63 ditambah 13 updater), operasi PDF (41), editor worker (102), pencarian (67), dan Word worker (27). Audit Word UI dan quality masing-masing 39 PASS, 0 FAIL pada folder `word-audit-008372f1a2694821b02e63ae31d7f51a` dan `quality-audit-32ed8b2f24954832acc4bef723093e0c` di kandidat `artifacts/update-build-aff34fe4c3904b0aac7fbd999e27575a`. Self-test PDF opener juga exit 0; buktinya `artifacts/pdf-association-verification/pdf-association-c210473ab19348e99155907a4e1e50a7/audit.txt`. Semua ini pengujian lokal, bukan sign-off UAT manusia.

`Verify-Package.ps1` selesai dengan exit 0 dan menghasilkan `package-integrity.json`:

- Seluruh 392 berkas manifest cocok ukuran dan SHA256-nya; 390 berkas payload awal identik dengan kandidat yang diuji. Dua berkas tambahan adalah metadata build dan ringkasan self-test JPG tanpa dokumen pengguna.
- SHA256 manifest: `8688D7513140A04B79E345516ED3E0BBA5D7D5025700E8B82D8E8044C1F66E72`.
- SHA256 DLL aplikasi: `D9C176245F11181A5B6E1C02253F8D02386BA0CEAF05BEE81F4560C23C4C6BE1`.
- Sumber/aset yang dibekukan tidak berubah setelah pengujian. Riwayat catatan versi 0.1.1–0.1.4 tetap ada.
- 596 berkas bukti sintetis dari sepuluh audit dipertahankan di `native-evidence/`, tidak disertakan sebagai dokumen dalam paket.
- Feed sebelumnya `20261004-211242-4e00f9cf` dipertahankan; tidak ada paket lama dihapus.

## Memakai pembaruan lokal

Feed terpasang menunjuk `D:/AFT Ahmad Yani Semarang/Teknik/Dokumentasi Pertamina OTO/releases/windows-local`; paket baru tersedia pada subfolder build di atas. Dari aplikasi, buka **Pengaturan → Update lokal → Pasang update**. Simpan pekerjaan sebelum restart. Tombol utama **Periksa update** pada implementasi saat ini memeriksa website, bukan feed lokal. Pesan generik pada skrip penerbitan masih menyebut tombol tersebut; instruksi lokal yang benar adalah **Update lokal**.

Tidak ada instalasi otomatis, penutupan aplikasi pengguna, unggahan website, perubahan asosiasi PDF, atau penerbitan Production dalam pekerjaan ini.

## Batas penerimaan

Ini pemulihan layout terarah, bukan janji semua piksel identik dengan 0.1.0 atau semua perangkat lulus. Perbedaan gaya halaman awal Gabung dan Hapus sudah ada di pembanding 0.1.0; penyeragaman desain baru tidak disisipkan pada pekerjaan ini. Pemeriksaan native memakai dokumen sintetis dan ukuran konten DIP. Matriks memanggil pembangun halaman langsung; pilihan sidebar pada screenshot bukan bukti uji navigasi klik pengguna. Satu screenshot Gabung menangkap thumbnail sedang memudar selama transisi; screenshot audit zoom berikutnya normal. Persepsi gerakan fisik tetap memerlukan pemeriksaan pengguna. DPI fisik, perpindahan monitor, gesture cubit perangkat, pembaca layar, seluruh PDF nyata, dan persetujuan pengguna tetap terpisah. Aplikasi pengguna tidak dipaksa tutup atau diaktifkan; website tidak diterbitkan.
