# Kapasitas input bersama 4 GB — implementasi web lokal

Tanggal: 27 Oktober 2026. Pelaksana: Agent 0, tanpa subagent. Status: implementasi dan pemeriksaan lokal selesai; belum dipublikasikan.

Pengguna menyetujui batas atas ukuran yang sama untuk semua alat web aktif. Ini tidak menyamakan jumlah file atau menghapus pengamanan. Rilis dilakukan bertahap dalam pekerjaan ini: Split terlebih dahulu (840 tes dan uji browser 32 MiB), kemudian fondasi yang telah diverifikasi dipakai oleh alat lainnya.

## Kontrak pengguna

| Fitur aktif | Maksimum input per pekerjaan | Batas khusus tetap berlaku |
|---|---|---|
| Gabungkan PDF | 500 PDF, total 4.000.000.000 byte | Minimum 2 file; guard objek, metadata dan dekompresi |
| Atur halaman PDF | 10 PDF, total 4.000.000.000 byte | 200 halaman total, termasuk susunan/duplikasi |
| Pisahkan PDF | 1 PDF, 4.000.000.000 byte | 200 halaman; 2–20 bagian, partisi lengkap tanpa tumpang tindih |
| Hapus, Ekstrak, Putar, Nomor halaman, Tanda air, Potong halaman | 1 PDF, 4.000.000.000 byte | 200 halaman; guard rentang, geometri dan operasi |
| Gambar ke PDF | 10 gambar, total 4.000.000.000 byte | JPG/PNG statis yang didukung, 8 juta piksel/gambar, 16 juta piksel total, 6.000 piksel/sisi |

4 GB adalah batas penerimaan **setiap pekerjaan**, bukan ruang yang langsung dialokasikan dan bukan jaminan semua dokumen sebesar itu dapat diproses. Mesin memeriksa kompleksitas, struktur, dukungan browser dan ruang lokal. Semua alat selain Merge memiliki plafon hasil total 4 GB. Total gambar JPEG hasil normalisasi tetap dibatasi 40 MiB agar dekoder tidak menjadi jalan memintas batas memori; metadata header gambar dibatasi 16 MiB. Ini batas aplikasi, bukan standar resmi Pertamina.

## Implementasi

- `pdf-capacity.json`: sumber angka tunggal bagi Razor, UI dan worker. Profil `split` dan `local` menyimpan batas baru. Profil `bounded` dipertahankan bagi mesin lama/tes rendah tingkat dan plafon gambar, bukan alur input produksi baru.
- `pdf-file-engine.js`: inspeksi ulang setiap eksekusi; parser File-slice, seleksi/susunan/rotasi, copier tag dan anotasi yang sama, dekorasi halaman yang sama, penulisan lalu pemeriksaan hasil. Mixed tagged/untagged menjaga tag asli dan memberi peringatan cakupan parsial.
- `pdf-file-images.js`: header dibaca dengan potongan kecil sebelum dekoder native browser. File diberikan ke dekoder tanpa `arrayBuffer()` seluruh input. Bitmap dan canvas dilepas per gambar; ukuran aktual hasil dekoder dan total piksel diperiksa ulang. Normalisasi JPEG/transparansi putih sama dengan perilaku sebelumnya. Dekoder native tetap dapat menggunakan RAM untuk data terkompresi; angka 4 GB bukan janji dekode gambar 4 GB pada semua perangkat.
- `pdf-split-engine.js`: kompatibilitas API Split, delegasi ke mesin umum. `pdf-split-worker.js` sekarang adalah worker bersama alat non-Merge; nama rutenya dipertahankan agar API lama tetap kompatibel.
- `pdf-merge-engine.js`: parser dan serializer bersama; stream sumber tetap slice, stream keluaran ditulis bertahap (payload per 1 MiB, metadata dibatasi terpisah). Serializer menyediakan pemeriksaan rencana ukuran sebelum menulis. Tidak ada vendor baru atau perubahan vendored dependency.
- `pdf-merge-job.js`: penyimpanan OPFS dengan kepemilikan Web Lock; file `part-N.pdf` hanya ditawarkan setelah seluruh hasil lolos pemeriksaan. Gagal, batal, timeout, dispose, penggantian hasil dan mulai baru melepas direktori milik pekerjaan. Kegagalan cleanup diberitahukan; startup mencoba direktori lama yang tidak terkunci.
- `browser-pdf.js`: seluruh alat non-Merge memegang referensi File, bukan seluruh ArrayBuffer. Transfer antarfitur divalidasi kembali; organizer menjaga identitas sumber/nomor asli/rotasi, images mengikuti urutan kartu. ResultController dan Save/Download tetap dipakai.
- `pdf-session.js`: file-backed input/output tidak dibebankan ke batas buffer sesi lama 30/40 MiB. Worker tetap menegakkan batas per pekerjaan. Satu pekerjaan non-Merge per origin dapat memproses pada satu waktu melalui Web Locks; Merge mempertahankan jalur sebelumnya. Tidak ada reservasi 4 GB per tab.
- Label tampil konsisten, termasuk mobile. Pada halaman awal mobile, tinggi konten dapat tumbuh agar footer tidak bertumpuk dengan kapasitas/dukungan.

## Pengamanan dan pembatasan

PDF terenkripsi, tanda tangan digital, formulir berisi, lampiran, script/aksi otomatis dan skema tautan berbahaya tetap ditolak. Tag/tautan yang didukung disalin dan diperiksa; target tautan yang dibuang serta operasi tag/duplikasi yang ambigu tetap ditolak. Sumber tidak diubah atau diunggah. Bookmark/metadata arsip mengikuti keterbatasan sebelumnya, bukan janji preservasi seluruh struktur PDF atau sertifikasi PDF/UA/PDF/A.

Parser tetap memakai plafon 20.000 objek/PDF, 40.000 objek total input, 16 MiB metadata/parser, dan batas dekompresi/struktur vendored. Batas waktu pekerjaan file-backed 30 menit. OPFS dan Web Locks diperlukan; penyimpanan dapat habis atau dibatasi browser. Hasil sementara bukan pemulihan sesi setelah reload. SaveFilePicker tergantung dukungan/izin browser, dengan unduh langsung sebagai fallback.

## Bukti

Root evidence: `artifacts/web-split-file-backed-20261027/` pada workspace utama.

- `final-regression.log`: **869 lulus**, 0 gagal, 0 dilewati/batal. Mencakup perbandingan hasil mesin lama dan baru, tag/tautan, mixed sources, keamanan, pembatalan, quota, cleanup, transfer antaralat, native decoder port, penulisan/pembukaan ulang hasil.
- `file-tools-targeted.log`: 48 tes khusus lulus. Tes mesin mengeksekusi PDF virtual 32 MiB untuk tujuh operasi PDF lanjutan.
- Split **4.000.000.000 byte virtual** diproses, diserialisasi dan diperiksa ulang melewati offset 2 GiB tanpa mengisi disk. Ini bukan pengukuran kinerja OPFS fisik 4 GB. Boundary admission 4 GB/+1 byte diuji untuk seluruh alat.
- Browser asli: Split dan Putar memakai fixture fisik 32 MiB. Hasil Split diunduh dan dibuka ulang, konten cocok serta SHA sumber tidak berubah (`browser-download-verification.json`). Putar menghasilkan 2 halaman, 33,56 MB dengan orientasi benar (`browser-rotate-result.png`).
- Browser asli: JPG sintetis 12 MiB (padding, 400×600 piksel) dan PNG transparan diterima; urutan dipindah melalui UI, PDF 2 halaman dengan PNG di depan berhasil dibuat dan dirender (`browser-images-result.png`). Ini menguji batas ukuran berkas, bukan gambar beresolusi raksasa.
- `final-capacity-mobile.json`, `final-split-mobile.png`, `final-split-desktop.png`: kapasitas 10 fitur pada viewport ponsel; tidak ada overflow horizontal. Footer Split sudah dipisahkan dari keterangan dukungan.
- `final-build-preview.log`, `final-build-downloads.log`: kedua host berhasil, 0 warning/error. Percobaan build pertama terhalang DLL preview yang sedang dipakai; hanya proses uji task ini dihentikan lalu dibangun ulang. Kegagalan percobaan tetap dicatat.
- `final-browser-console.json`: tidak ada error/warning yang tertangkap pada uji browser.

Seluruh fixture sintetis. Tidak ada klaim semua perangkat/semua PDF lulus, bebas crash, atau Production.

## Distribusi

Preview terbaru: **http://127.0.0.1:53970/alat/split**; perlu proses lokal tetap hidup. Identitas proses tersimpan dalam `final-preview-process.json`.

Situs publik oto.ayani.id, aplikasi Windows terpasang, feed/current/latest, dan paket rilis Windows tidak diubah. Branch/dirty work lain dipertahankan. Tidak ada push, deploy, publikasi, atau dependency tambahan.
