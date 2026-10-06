# Evaluasi mesin editor PDF lokal

Tanggal: 2026-09-18. Otorisasi: pengguna memilih jalur 1, evaluasi mesin tambahan tanpa instalasi/pembelian sebelum pemeriksaan selesai.
Status: evaluasi dokumentasi selesai; rekomendasi kandidat PoC bersyarat. BUKAN dependency admission, kelulusan keamanan, bukti runtime, atau kelulusan UI/UX.

## Baseline lokal

Directory.Packages.props memakai PDFsharp 6.2.4. Worker PdfOperations tidak mempunyai operasi edit; protocol tidak memuat objek edit. InternalPdfReader menggunakan Windows.Data.Pdf untuk rendering. WorkerClient sudah mempunyai proses terpisah, timeout, pembatasan memori, staging dan publikasi nama baru. Proses terpisah bukan sandbox keamanan OS.

## Perbandingan

| Kandidat | Bukti kemampuan | Integrasi dan batas | Kesimpulan |
|---|---|---|---|
| PDFium | Public API mengakses objek halaman, mengganti isi objek teks, membaca informasi font, menghasilkan ulang konten dan menyimpan PDF | Native C API, perlu binding C# dan lifetime management; bukan widget editor WinUI. Font subset, encoding, objek bertingkat dan teks terfragmentasi tetap harus diuji | Kandidat pertama untuk PoC offline tanpa biaya lisensi engine |
| Apryse Core | Panduan Server/Desktop menyediakan penggantian teks dan gambar, termasuk contoh C# | Kandidat komersial; ketentuan redistribusi, offline activation/telemetry, fitur yang tercakup dan harga harus dikonfirmasi. WebViewer content editor bukan kontrol WinUI native dan merupakan produk/add-on berbeda | Alternatif bila anggaran lisensi disetujui; tidak dibeli/diunduh |
| MuPDF | Dokumentasi menyediakan engine lokal dan API; fitur viewer mencakup anotasi dan redaksi | AGPL atau lisensi komersial menurut penerbit. Jangan menganggap proses terpisah otomatis menyelesaikan kewajiban lisensi. Kesetaraan editing teks interaktif belum dibuktikan dalam evaluasi ini | Bukan pilihan awal untuk distribusi tertutup sebelum keputusan lisensi |
| PDFsharp existing | Penulisan grafis/teks dan manipulasi konten tingkat rendah | FAQ menyebut tidak tersedia helper pengeditan konten/koordinat tingkat tinggi | Tetap untuk alat existing; jangan dipaksakan menjadi editor teks penuh tanpa pembuktian |

Tidak ada klaim bahwa kandidat otomatis mempertahankan semua font, paragraf, tag aksesibilitas, form atau tanda tangan. OCR tidak termasuk edit teks asli pada scan. API mengganti objek teks bukan jaminan penataan ulang paragraf seperti Word.

## Mengapa PDFium layak diuji

Dokumentasi resmi fpdf_edit.h menyatakan FPDFText_SetText mengganti teks pada objek, dengan masukan UTF-16LE. Itu bukti kemampuan API, bukan bukti keberhasilan pada corpus kita. Pengujian akan memakai object identity dan font/glyph yang didukung; tidak menutup teks lama dengan kotak. Penulisan wajib menghasilkan ulang page content sebelum save. Penghapusan teks kosong memerlukan jalur tersendiri, bukan mengasumsikan SetText menerima string kosong.

Lisensi inti BSD-style mengizinkan redistribusi dengan syarat notice dan disclaimer. Seluruh third-party notices, font, wrapper dan binary build harus diperiksa secara terpisah. Ini penilaian awal teknis/lisensi, bukan persetujuan hukum/distribusi.

## Admission yang masih wajib sebelum akuisisi/integrasi

- Pilih exact upstream revision, asal binary/build pipeline, Windows x64 ABI, build flags dan checksum; jangan menggunakan floating latest dalam produk.
- Cocokkan header dengan binary, ekspor fungsi, calling convention, pointer ownership dan API experimental. Perhatikan dokumentasi main dapat berubah.
- Inventaris dependency, SBOM/notices dan lisensi wrapper bila ada. Evaluasi binding langsung minimal untuk menghindari wrapper yang hanya mendukung viewer.
- Periksa advisory resmi terhadap revision terpilih dan transitif; tanpa exact build tidak boleh mengklaim bebas kerentanan.
- Rencanakan update keamanan, nonaktifkan JavaScript/XFA bila tidak diperlukan, larang aksi eksternal, batasi resource dan audit trust boundary. Worker terpisah perlu hardening tambahan, bukan otomatis sandbox.
- Review/otorisasi sesuai aturan proyek. Evaluasi ini tidak memasang, mengunduh binary, atau menambah package reference.

## PoC berikutnya setelah admission

1. Fixture sintetis dengan teks sederhana, karakter Indonesia, angka, tanda baca, font subset, rotasi dan crop box. Sertakan scan, encoding tidak didukung dan signed/encrypted input untuk penolakan eksplisit.
2. Baca teks dan koordinat, pilih satu objek, ganti teks, simpan PDF baru tanpa overlay.
3. Buka ulang, bandingkan ekstraksi teks dan render menggunakan renderer berbeda; teks lama pada objek target harus tidak lagi menjadi konten aktif, teks baru benar, objek lain tidak berubah. Ini bukan bukti redaksi/secure erasure.
4. Uji unsupported glyph, teks lebih panjang, shared form objects dan transform; tolak aman jika fidelity tidak dapat dijamin, jangan substitusi diam-diam.
5. Uji timeout/crash/cancel, hash sumber, hasil existing, cleanup, batas memori dan offline runtime dengan jaringan diblokir.
6. Hanya setelah PoC lulus integrasikan desain WinUI dan jalankan seluruh gate PDF_EDITOR_DESIGN_GATE.md. Seluruh kontrol mockup tetap tunduk pada kemampuan yang terbukti.

## Sumber primer yang diperiksa

- PDFium edit API: https://pdfium.googlesource.com/pdfium/+/refs/heads/main/public/fpdf_edit.h
- PDFium text API: https://pdfium.googlesource.com/pdfium/+/refs/heads/main/public/fpdf_text.h
- PDFium save API: https://pdfium.googlesource.com/pdfium/+/refs/heads/main/public/fpdf_save.h
- PDFium license: https://pdfium.googlesource.com/pdfium/+/refs/heads/main/LICENSE
- PDFium build/Windows: https://pdfium.googlesource.com/pdfium/+/refs/heads/main/README.md
- Apryse Core C# replacement: https://docs.apryse.com/core/guides/features/search/replace/csharp
- Apryse Core editor scope: https://docs.apryse.com/core/guides/features/edit
- Apryse Web content-edit add-on: https://docs.apryse.com/web/guides/edit/text-edit
- MuPDF license: https://mupdf.readthedocs.io/en/latest/license.html
- PDFsharp limitations: https://docs.pdfsharp.net/PDFsharp/Overview/FAQ.html

Perubahan pada turn ini hanya laporan evaluasi. Aplikasi, paket, dokumen pengguna dan status fase tidak berubah. Tidak ada audit runtime/UI yang dijalankan atau diklaim lulus untuk editor baru.
