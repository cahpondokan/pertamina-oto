# Kapasitas input web — 27 Oktober 2026

Catatan ini merekam tahap awal penyeragaman label. Batas aktif kemudian dinaikkan melalui implementasi bertahap yang disetujui pengguna; lihat [Kapasitas file-backed 4 GB](WEB_FILE_BACKED_CAPACITY_20261027.md) untuk kontrak dan hasil verifikasi terbaru. Bukti 807 tes di bawah tetap merupakan bukti tahap awal.

Implementasi lokal untuk sepuluh alat web aktif. Label, posisi, tampilan, dan sumber angka kapasitas diseragamkan; batas pemrosesan tidak dinaikkan. Template awal dan ikon Biblo tetap digunakan.

| Kelompok | Kapasitas input | Batas tambahan |
|---|---|---|
| Gabungkan PDF | 500 PDF, 4 GB total (4.000.000.000 byte) | Minimum 2 PDF; batas struktur, objek, metadata, penyimpanan dan waktu tetap berlaku |
| Atur halaman PDF | 10 PDF, 30 MiB total | 10 MiB/file; 200 halaman total |
| Pisahkan, Hapus, Ekstrak, Putar, Nomor halaman, Watermark, Potong | 1 PDF, 10 MiB | 200 halaman |
| Gambar ke PDF | 10 gambar, 30 MiB total | 10 MiB/gambar; 8 juta piksel/gambar, 16 juta piksel total, 6.000 piksel/sisi |

MiB berarti 1.048.576 byte. Label lama “MB” untuk batas biner diperjelas menjadi “MiB”; jumlah byte yang diterima tetap sama. Kapasitas adalah batas penerimaan, bukan jaminan bahwa semua dokumen di bawah batas tersebut didukung.

## Sumber tunggal

`src/Hosts/Pertamina.Oto.Web.Preview/wwwroot/js/pdf-capacity.json` menjadi sumber numerik. `Models/PdfCapacity.cs` membacanya sebagai embedded resource untuk Razor; `pdf-capacity.js` membacanya untuk UI, pesan, pekerjaan latar belakang, dan validator. Build kedua host menghasilkan `pdf-capacity-data.js` untuk classic workers. Jangan mengedit file hasil generate secara langsung.

Setiap alat dipetakan ke profil yang telah didukung. Mesin bounded dan mesin Merge tetap terpisah sesuai kebutuhan penyimpanan. Worker inspeksi kini menegakkan satu file untuk alat single-PDF, selain pembatasan UI dan eksekusi yang sudah ada. Guard tag, tautan, formulir, tanda tangan, konten aktif, dekompresi dan referensi tetap berlaku.

Jumlah 2–20 bagian pada Pisahkan PDF merupakan kapasitas hasil, ditampilkan pada pengaturan hasil dan pesannya; tidak lagi dipresentasikan sebagai kapasitas input. Informasi kapasitas tetap terlihat pada layar sempit.

## Verifikasi

- Build Web.Preview dan Downloads: berhasil, 0 warning / 0 error, tanpa restore.
- Seluruh 807 pengujian Node: lulus, 0 gagal / batal / dilewati.
- Uji tambahan: manifest browser sama dengan JSON, profil alat, byte batas tepat dan lebih satu byte, input single/multi, pesan, worker dan integrasi kedua host.
- Batas 500 file / 4 GB diperiksa pada admission metadata. Ini **bukan** bukti pengolahan berkas 4 GB sampai selesai.
- Pemeriksaan DOM 10 alat pada lebar 320, 390 dan 1.366 px: 30/30 kapasitas terlihat, muat, tanpa overflow horizontal. Screenshot desktop/ponsel ditinjau. Console browser tanpa error/warning yang tertangkap.

Bukti: `artifacts/web-input-capacity-20261027/` pada root workspace, termasuk baseline, log build/regresi, responsive.json dan screenshot. Dokumen sumber pengguna tidak dipakai atau diubah oleh pengujian ini.

Pratinjau lokal: http://127.0.0.1:53969/alat/split (selama proses preview aktif). Perubahan belum dipublikasikan ke oto.ayani.id, belum menjadi paket Windows, dan tidak mengubah feed update.
