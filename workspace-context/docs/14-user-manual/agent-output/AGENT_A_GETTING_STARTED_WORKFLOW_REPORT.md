# AGENT A — Getting Started, Navigation & Common Workflow Report

**Task ID:** DOC14-AGENT-A  
**Tujuan:** Rancangan konten User Manual PERTAMINA OTO untuk pengguna non-teknis  
**Status:** Rekomendasi Agent A; bukan perubahan terhadap master Document 14

> **Kesenjangan sumber:** Document 13 FINAL tidak ditemukan di workspace saat laporan ini disusun. `MASTER_PROJECT_SUMMARY.md` terbaru yang tersedia masih menempatkan Document 13 sebagai pekerjaan berikutnya. Karena itu, laporan ini menggunakan Document 01–12 FINAL sebagai sumber normatif dan laporan-laporan agent DOC13 hanya sebagai masukan UAT non-final. Istilah atau posisi kontrol yang belum ditetapkan dalam sumber diberi tanda **VALIDASI_UI_REQUIRED** dan tidak diperlakukan sebagai keputusan UI.

## 1. Target pembaca

Panduan ini ditujukan kepada pegawai Pertamina yang menggunakan PERTAMINA OTO untuk menyelesaikan pekerjaan dokumen, termasuk pengguna baru yang tidak memiliki pengetahuan teknis. Panduan harus dapat dibaca per bagian tanpa mengharuskan pengguna memahami cara kerja internal aplikasi.

Kelompok pembaca yang perlu dilayani:

- pengguna baru yang ingin menyelesaikan pekerjaan pertama dalam lima menit;
- pengguna rutin yang langsung memilih file atau tool;
- pengguna yang lebih mudah menjelaskan tujuan melalui Assistant;
- pengguna Desktop, Web, Phone, atau Tablet, dengan kemampuan yang dapat berbeda menurut platform;
- pengguna yang bekerja dengan koneksi terbatas atau terputus;
- pengguna yang perlu melanjutkan pekerjaan setelah aplikasi tertutup atau proses gagal.

Prasyarat minimum:

- memiliki akun Pertamina yang aktif;
- memiliki hak untuk menggunakan feature yang dipilih;
- memiliki file yang dapat diakses dari perangkat, jika pekerjaan membutuhkan file;
- memiliki ruang penyimpanan yang cukup;
- memiliki koneksi ketika menggunakan kemampuan yang memang memerlukannya.

Prinsip penulisan untuk pembaca:

1. Mulai dari tujuan pengguna, bukan struktur aplikasi.
2. Gunakan kalimat perintah pendek dan satu tindakan per langkah.
3. Selalu jelaskan hasil yang akan terlihat setelah tindakan.
4. Jelaskan perbedaan perilaku offline dengan bahasa sederhana.
5. Jangan memakai istilah API, database, Worker, schema, engine, provider, atau lapisan arsitektur.
6. Jangan menjanjikan kontrol yang belum tersedia pada UI aktual.

Konvensi label dalam rancangan ini:

- Istilah bisnis yang sudah konsisten di sumber: **Activity Center**, **Amankan ke Akun**, **File-First**, **Tool-First**, **Assistant-First**, **Riwayat**, **Simpan**, **Simpan sebagai File Baru**, **Urungkan**, **Lanjutkan**, **Coba Lagi**, **Batalkan**, dan **Laporkan Masalah**.
- Label konseptual seperti **Beranda**, **Tambahkan File**, **Cari Tool**, **Semua Tool**, **Rekomendasi**, **File yang Diamankan**, **Bantuan**, dan **Profil** harus dipastikan terhadap build UI final: **VALIDASI_UI_REQUIRED**.

Panduan ini tidak mencakup Admin Manual, Audit Trail internal, Incident Guide, atau petunjuk teknis sistem.

## 2. Quick Start

### Mulai dalam 5 menit

1. **Buka PERTAMINA OTO.**
2. **Login dengan akun Pertamina** jika aplikasi meminta Anda masuk.
3. **Pilih cara memulai:** tambahkan file, pilih tool, atau jelaskan tujuan kepada Assistant.
4. **Periksa file dan pilihan Anda, lalu pilih Proses** (**VALIDASI_UI_REQUIRED** untuk label tombol final).
5. **Periksa hasil, lalu pilih Simpan atau Simpan sebagai File Baru.**

Contoh cepat—menggabungkan dua PDF:

1. Tambahkan dua file PDF.
2. Pilih **Gabungkan PDF**.
3. Atur urutan file.
4. Jalankan proses.
5. Periksa hasil.
6. Pilih **Simpan sebagai File Baru** agar file asli tetap aman.

Tiga hal yang perlu diketahui sejak awal:

- **Activity Center** menampilkan pekerjaan yang masih berjalan, menunggu, selesai, atau gagal. Anda dapat berpindah halaman tanpa kehilangan informasi proses yang berjalan di latar belakang.
- **Amankan ke Akun** menyimpan hasil yang Anda pilih ke area file aman pada akun. Tindakan ini berbeda dari menyimpan file biasa ke perangkat dan dapat memerlukan koneksi.
- **Laporkan Masalah** digunakan jika kendala tidak selesai setelah mencoba kembali. Jelaskan apa yang sedang Anda lakukan dan periksa informasi yang akan disertakan sebelum laporan dikirim.

Checklist selesai:

- hasil dapat dibuka;
- nama dan lokasi penyimpanan benar;
- file asli masih tersedia;
- pekerjaan pada Activity Center menunjukkan **Selesai**;
- bila diperlukan, hasil sudah diamankan ke akun.

## 3. Login/session

### Login dengan akun Pertamina

1. Buka PERTAMINA OTO.
2. Pilih **Login Pertamina** (**VALIDASI_UI_REQUIRED** untuk label final).
3. Selesaikan login pada halaman Pertamina yang ditampilkan.
4. Kembali ke aplikasi setelah login berhasil.
5. Pastikan nama atau profil Anda tampil sebelum membuka file sensitif.

Selama sesi masih aktif, pengguna tidak perlu login ulang pada setiap perpindahan halaman. Aplikasi dapat meminta login kembali apabila sesi berakhir, akun dinonaktifkan, akses dicabut, atau tindakan sensitif memerlukan verifikasi ulang.

### Saat aplikasi meminta login kembali

1. Simpan pekerjaan yang masih dapat disimpan.
2. Pilih **Login Kembali** (**VALIDASI_UI_REQUIRED**).
3. Selesaikan verifikasi Pertamina.
4. Kembali ke pekerjaan sebelumnya jika aplikasi menawarkan pemulihan.

Jika login gagal berulang, jangan terus mencoba tanpa jeda. Periksa koneksi dan status akun, lalu gunakan **Laporkan Masalah** atau kanal bantuan resmi.

### Bekerja saat offline

Sesi offline hanya dapat dipakai jika pengguna sebelumnya sudah login pada perangkat tersebut dan sesi offline masih valid. Dalam kondisi ini:

- pekerjaan yang dapat dilakukan di perangkat tetap tersedia sesuai hak yang terakhir masih valid;
- kemampuan yang memerlukan layanan online menampilkan **Memerlukan koneksi**, **Menunggu koneksi**, atau tidak tersedia;
- tindakan sensitif dapat ditunda sampai koneksi kembali dan akses dapat diperiksa ulang;
- sesi offline yang sudah berakhir tidak dapat diperpanjang tanpa login online;
- setelah tersambung kembali, aplikasi dapat memeriksa ulang akun dan hak akses sebelum melanjutkan pekerjaan.

| Kondisi | Yang dilihat pengguna | Tindakan pengguna |
|---|---|---|
| Sesi aktif | Beranda atau pekerjaan dapat digunakan | Lanjutkan pekerjaan |
| Login diperlukan | Permintaan login Pertamina | Login dengan akun Pertamina |
| Sesi berakhir | Pesan bahwa sesi perlu diperbarui | Simpan jika memungkinkan, lalu login kembali |
| Offline tetapi sesi masih valid | Penanda offline; sebagian kemampuan tetap tersedia | Gunakan kemampuan yang tersedia atau tunggu koneksi |
| Kemampuan memerlukan koneksi | Status memerlukan/menunggu koneksi | Sambungkan perangkat; pantau Activity Center |
| Akun tidak lagi diizinkan | Akses ditolak dan pekerjaan sensitif tidak dilanjutkan | Hubungi pengelola akun atau bantuan resmi |

### Logout

1. Periksa Activity Center.
2. Simpan atau batalkan perubahan yang belum selesai.
3. Buka **Profil** (**VALIDASI_UI_REQUIRED**).
4. Pilih **Logout**.
5. Pastikan halaman login tampil sebelum meninggalkan perangkat bersama.

Logout tidak boleh dianggap sebagai cara menyimpan hasil. Draft yang dapat dipulihkan tetap tunduk pada perlindungan perangkat dan akun; pengguna lain tidak boleh dapat membukanya hanya dengan login ke akun berbeda.

## 4. Home/navigation

Nama, ikon, dan posisi menu di bawah merupakan struktur konseptual. Semuanya harus dicocokkan dengan build UI final sebelum diterbitkan.

| Area konseptual | Kegunaan bagi pengguna | Cara penggunaan singkat | Status label |
|---|---|---|---|
| **Beranda** | Titik awal untuk membuka file, tool, rekomendasi, atau pekerjaan terakhir | Kembali ke sini untuk memulai pekerjaan baru | VALIDASI_UI_REQUIRED |
| **Area File / Tambahkan File** | Memilih atau menjatuhkan file untuk memulai File-First | Pilih area, lalu tentukan sumber file | VALIDASI_UI_REQUIRED |
| **Semua Tool** | Melihat kumpulan kemampuan yang tersedia | Pilih kategori atau tool yang dibutuhkan | VALIDASI_UI_REQUIRED |
| **Cari Tool** | Menemukan tool dari nama atau tujuan | Ketik kata seperti “gabungkan”, “kompres”, atau “OCR” | VALIDASI_UI_REQUIRED |
| **Rekomendasi** | Menampilkan tindakan yang sesuai dengan file terpilih | Periksa saran, lalu pilih hanya jika sesuai tujuan | VALIDASI_UI_REQUIRED |
| **Assistant** | Memulai pekerjaan dengan menjelaskan tujuan | Tulis tujuan; periksa rencana sebelum menjalankan | Istilah ditetapkan; lokasi perlu validasi |
| **Activity Center** | Memantau proses aktif dan pekerjaan latar belakang | Buka untuk melihat status, detail, retry, atau hasil | Istilah ditetapkan; kontrol perlu validasi |
| **Riwayat** | Menemukan kembali aktivitas dan hasil yang pernah dibuat | Pilih entri untuk melihat detail atau membuka hasil | Istilah bisnis ditetapkan; UI perlu validasi |
| **File yang Diamankan** | Membuka file yang sengaja diamankan ke akun | Pilih file dan versi yang tersedia | VALIDASI_UI_REQUIRED |
| **Bantuan / Laporkan Masalah** | Membaca bantuan atau melaporkan kendala | Pilih topik atau isi laporan masalah | VALIDASI_UI_REQUIRED untuk posisi/menu |
| **Profil** | Melihat akun dan logout | Periksa akun aktif sebelum menangani file sensitif | VALIDASI_UI_REQUIRED |

Aturan navigasi yang perlu dijelaskan pada manual final:

- Pengguna dapat berpindah dari Beranda ke Activity Center tanpa menghentikan proses latar belakang.
- Tombol kembali tidak boleh membuat pengguna kehilangan perubahan tanpa peringatan.
- Jika ada perubahan yang belum disimpan, aplikasi menjelaskan pilihan menyimpan, membatalkan perpindahan, atau membuang perubahan.
- Pilihan yang tidak tersedia harus disertai alasan dan langkah yang dapat dilakukan, bukan hanya dibuat redup tanpa penjelasan.
- Pada layar kecil, area yang sama dapat berada di menu tambahan. Nama fungsinya harus tetap konsisten.

## 5. Triple Entry Flow

Ketiga cara masuk berakhir pada perilaku inti yang sama: file diperiksa, pilihan dikonfirmasi bila berisiko, proses terlihat, hasil diperiksa, lalu pengguna menentukan penyimpanan.

### File-First

**Gunakan ketika:** Anda sudah mempunyai file dan ingin melihat tindakan yang sesuai.

Langkah:

1. Pilih atau masukkan file dari Beranda.
2. Tunggu pemeriksaan file.
3. Periksa status file dan rekomendasi yang muncul.
4. Pilih tool yang sesuai.
5. Atur pilihan, lalu jalankan proses.
6. Periksa dan simpan hasil.

**Contoh:** Tambahkan dua PDF. Setelah keduanya berstatus **Siap**, pilih **Gabungkan PDF**, atur urutan, lalu simpan hasil sebagai file baru.

### Tool-First

**Gunakan ketika:** Anda sudah tahu tool yang dibutuhkan.

Langkah:

1. Buka daftar tool atau gunakan pencarian.
2. Pilih tool.
3. Tambahkan file yang diminta.
4. Periksa persyaratan dan status file.
5. Atur pilihan, lalu jalankan proses.
6. Periksa dan simpan hasil.

**Contoh:** Cari **Kompres PDF**, pilih tool tersebut, tambahkan PDF, pilih tingkat kompresi, lalu simpan hasil.

### Assistant-First

**Gunakan ketika:** Anda lebih mudah menjelaskan hasil yang diinginkan daripada mencari nama tool.

Langkah:

1. Buka Assistant.
2. Jelaskan tujuan secara singkat.
3. Tambahkan file bila diminta.
4. Periksa tool dan langkah yang diusulkan Assistant.
5. Ubah pilihan jika belum sesuai.
6. Konfirmasi tindakan yang sensitif atau tidak mudah dibatalkan.
7. Jalankan, periksa, dan simpan hasil.

**Contoh:** Tulis, “Gabungkan dua PDF ini, beri nomor halaman, lalu kompres hasilnya.” Periksa urutan rencana dan file keluaran sebelum menyetujui proses.

Assistant tidak melewati pemeriksaan file, izin, batas kebijakan, konfirmasi, atau aturan penyimpanan. Tindakan tetap dilakukan atas nama pengguna dan hasil tetap harus diperiksa.

| Pertanyaan pengguna | Pilihan yang disarankan |
|---|---|
| “Saya sudah punya file; apa yang bisa dilakukan?” | File-First |
| “Saya tahu tool yang saya perlukan.” | Tool-First |
| “Saya tahu hasil yang saya mau, tetapi tidak tahu tool-nya.” | Assistant-First |

## 6. File input

Kemampuan input dapat berbeda menurut platform dan rilis. Aplikasi hanya boleh menampilkan pilihan yang benar-benar didukung pada perangkat tersebut.

| Cara input | Platform yang relevan | Langkah pengguna | Hasil | Batasan yang perlu dijelaskan |
|---|---|---|---|---|
| **File Picker** | Desktop, Web, Phone, Tablet | Pilih **Tambahkan File**; cari file; pilih **Buka/Tambahkan** | File muncul di area kerja dan diperiksa | Akses mengikuti izin perangkat; format atau ukuran tertentu dapat ditolak |
| **Drag-and-Drop** | Desktop dan Web yang mendukung | Seret file ke area yang ditandai; lepaskan | File masuk ke daftar input | Tidak semua browser/perangkat mendukung; folder hanya dapat dipakai bila dinyatakan didukung |
| **Paste** | Desktop dan platform yang mendukung clipboard | Salin file atau gambar; fokuskan area input; pilih/tempel | Salinan menjadi input sementara | Isi clipboard tidak dicatat sebagai audit payload; beberapa jenis isi tidak didukung; jangan menganggap tempel berarti sudah tersimpan |
| **Share / Open With** | Phone, Tablet, dan integrasi sistem operasi yang tersedia | Dari aplikasi sumber, pilih **Bagikan/Buka dengan**; pilih PERTAMINA OTO | File dibuka sebagai input | Bergantung pada dukungan platform dan target rilis; bukan jaminan semua aplikasi sumber didukung |
| **Scan** | Terutama Phone/Tablet; perangkat lain bila kemampuan tersedia | Pilih **Scan**; ambil halaman; periksa potongan/orientasi; selesai | Hasil scan masuk ke area kerja | Kualitas kamera, izin perangkat, jumlah halaman, dan kemampuan scanner fisik dapat membatasi hasil |
| **Recent Files** | Platform yang menyediakan akses file terbaru | Buka **Recent Files**; pilih entri | File yang masih tersedia dibuka kembali | Riwayat tidak membuat salinan otomatis; file yang dipindah, dihapus, atau izinnya berubah perlu dipilih ulang |

Setelah input:

1. Pastikan nama dan jumlah file benar.
2. Periksa status setiap file.
3. Masukkan password hanya melalui kolom yang disediakan jika diminta.
4. Hapus file yang salah dari daftar input; tindakan ini tidak menghapus file asli.
5. Untuk operasi yang memakai urutan, atur urutan sebelum memproses.

Menambahkan file ke area kerja tidak selalu berarti file dikirim ke layanan online. Jika suatu tindakan memerlukan pengiriman konten, aplikasi harus menjelaskannya sebelum proses dilanjutkan.

## 7. File validation

Gunakan status yang menjelaskan kondisi dan tindakan berikutnya dengan bahasa pengguna.

| Status pengguna | Arti | Yang dapat dilakukan pengguna |
|---|---|---|
| **Siap** | File dapat digunakan untuk langkah berikutnya | Lanjutkan memilih tool atau proses |
| **Memerlukan password** | File dilindungi dan harus dibuka terlebih dahulu | Masukkan password yang benar; pilih file lain jika tidak memilikinya |
| **Peringatan** | File dapat dipakai, tetapi ada kondisi yang perlu diperhatikan | Baca peringatan; lanjutkan hanya jika hasil dan risikonya dapat diterima |
| **Sedang diperiksa** | Aplikasi memerlukan pemeriksaan tambahan | Tunggu hasil; jangan tutup informasi status | 
| **Tidak didukung** | Jenis, versi, atau kemampuan file tidak sesuai tool/platform | Gunakan format yang didukung, pilih tool lain, atau konversi dari sumber yang tepercaya |
| **Rusak** | File tidak dapat dibaca dengan aman | Coba buka file asli di aplikasi lain; gunakan salinan/cadangan yang sehat |
| **Diblokir** | File atau tindakan tidak diizinkan oleh keamanan atau kebijakan | Jangan mencoba melewati pembatasan; gunakan file yang diizinkan atau hubungi pihak berwenang |
| **Penyimpanan tidak cukup** | Ruang kerja atau lokasi tujuan tidak mencukupi | Kosongkan ruang, pilih lokasi lain, atau kurangi ukuran/jumlah file; lalu coba lagi |

Aturan panduan:

- Status selalu ditempatkan dekat file yang bermasalah.
- Pesan menjelaskan tindakan yang dapat dilakukan, bukan hanya kode kesalahan.
- Pada beberapa file, pengguna dapat memperbaiki atau menghapus file yang bermasalah tanpa memilih ulang semua file.
- Proses yang membutuhkan seluruh input—misalnya menggabungkan PDF—hanya dapat dimulai setelah daftar final memenuhi syarat.
- Password tidak ditampilkan kembali, tidak dimasukkan ke Riwayat, dan tidak disimpan sebagai bagian dari draft.

## 8. Common processing flow

Alur umum berikut harus dipakai secara konsisten, baik pekerjaan dimulai dari file, tool, maupun Assistant.

1. **Tambahkan input.** Pilih file atau sumber lain yang didukung.
2. **Periksa validasi.** Tangani password, peringatan, format, kerusakan, atau ruang penyimpanan.
3. **Pilih tujuan.** Pilih tool, rekomendasi, atau rencana Assistant.
4. **Atur pilihan.** Tentukan urutan, halaman, kualitas, nama hasil, atau pilihan lain yang relevan.
5. **Periksa preview.** Pastikan perubahan sesuai tujuan jika preview tersedia.
6. **Jalankan proses.** Konfirmasi hanya jika tindakan berisiko, mengirim konten, membagikan, menimpa, menghapus permanen, atau sulit dibatalkan.
7. **Pantau progres.** Lihat indikator pada halaman atau Activity Center.
8. **Periksa hasil.** Buka hasil dan baca ringkasan atau peringatan.
9. **Simpan hasil.** Pilih Simpan atau Simpan sebagai File Baru; gunakan Amankan ke Akun bila memang dibutuhkan.

| Tahap | Tanggung jawab pengguna | Respons yang harus terlihat dari aplikasi |
|---|---|---|
| Input | Memilih file yang benar | Nama, jumlah, dan status file terlihat |
| Pilihan | Menentukan hasil yang diinginkan | Dampak pilihan dan preview terlihat |
| Konfirmasi | Menyetujui tindakan berisiko | Ringkasan objek, tindakan, dan konsekuensi jelas |
| Proses | Menunggu atau melanjutkan pekerjaan lain | Status dan perubahan status terlihat; tidak ada waktu tunggu diam-diam |
| Hasil | Memeriksa apakah tujuan tercapai | Ringkasan hasil, peringatan, dan lokasi sementara terlihat |
| Simpan | Menentukan tujuan akhir | Nama/lokasi final serta keberhasilan penyimpanan terlihat |

Perubahan yang mudah dibatalkan tidak memerlukan konfirmasi berulang. Gunakan **Urungkan** untuk tindakan edit biasa. Konfirmasi digunakan ketika dampaknya penting, bukan pada setiap klik.

## 9. Progress/Activity Center

### Tiga tingkat informasi progres

1. **Indikator cepat** memberi tahu bahwa tindakan singkat sedang berlangsung atau sudah selesai.
2. **Progres detail** menampilkan tahap, status, dan tindakan yang tersedia untuk pekerjaan aktif.
3. **Activity Center** mengumpulkan pekerjaan yang berjalan di latar belakang, menunggu, selesai, gagal, atau memerlukan tindakan.

### Status yang harus dipahami pengguna

| Status | Artinya | Tindakan pengguna |
|---|---|---|
| **Menunggu** | Pekerjaan sudah diterima dan menunggu giliran | Tunggu atau batalkan jika tombol tersedia |
| **Berjalan** | Pekerjaan sedang diproses | Pantau progres; Anda dapat melanjutkan pekerjaan lain bila diizinkan |
| **Menunggu koneksi** | Tahap berikutnya memerlukan koneksi | Sambungkan perangkat; pekerjaan dapat dilanjutkan setelah pemeriksaan ulang |
| **Menunggu sumber daya** | Aplikasi belum memiliki ruang atau kemampuan yang diperlukan | Baca detail; kosongkan ruang atau tunggu; gunakan Coba Lagi jika tersedia |
| **Mencoba lagi** | Aplikasi mengulang tahap yang aman | Tunggu dan periksa apakah status berubah |
| **Selesai** | Hasil telah dibuat dan diperiksa | Buka hasil, lalu simpan atau amankan |
| **Gagal** | Pekerjaan tidak menghasilkan keluaran final yang valid | Baca penyebab; perbaiki input/kondisi; pilih Coba Lagi atau Laporkan Masalah |
| **Dibatalkan** | Pekerjaan dihentikan | Pastikan file asli tetap ada; mulai kembali jika dibutuhkan |

### Menggunakan Activity Center

1. Buka **Activity Center**.
2. Pilih pekerjaan untuk melihat detail.
3. Baca status terbaru dan tindakan yang tersedia.
4. Pilih **Batalkan** hanya bila Anda tidak lagi membutuhkan proses.
5. Pilih **Coba Lagi** setelah penyebab kegagalan diperbaiki.
6. Pilih **Lanjutkan** jika pekerjaan menunggu tindakan atau koneksi sudah kembali.
7. Untuk pekerjaan selesai, pilih **Buka Hasil** (**VALIDASI_UI_REQUIRED**) dan simpan hasil bila belum disimpan.

Prinsip **No Silent Wait** dalam panduan pengguna:

- setiap proses yang tidak segera selesai menunjukkan bahwa aplikasi sedang bekerja;
- status berubah saat aplikasi menunggu koneksi, ruang, izin, atau tindakan pengguna;
- proses latar belakang dapat ditemukan kembali di Activity Center;
- kegagalan memberi alasan yang dapat dipahami dan langkah berikutnya;
- persentase hanya ditampilkan bila benar-benar bermakna; jika tidak, tahap proses lebih berguna daripada angka palsu.

Menutup halaman proses tidak selalu membatalkan pekerjaan. Periksa Activity Center sebelum memulai ulang agar tidak membuat pekerjaan ganda.

## 10. Save/autosave/recovery

> **File asli tetap aman.** Perubahan dilakukan di area kerja aplikasi sampai pengguna menyimpan hasil. Aplikasi tidak boleh menimpa file asli secara diam-diam.

| Tindakan | Arti sederhana | Hal yang perlu diketahui |
|---|---|---|
| **Urungkan (Undo)** | Membatalkan perubahan terakhir di area kerja | Berlaku pada perubahan yang masih dapat dibalik; bukan pengganti backup |
| **Ulangi (Redo)** | Mengembalikan perubahan yang baru diurungkan | Hanya tersedia bila urutan perubahan masih mendukung |
| **Autosave** | Menyimpan keadaan kerja sementara | Bukan file hasil final dan bukan jaminan penyimpanan permanen |
| **Restore Draft / Pulihkan Draft** | Membuka kembali keadaan kerja yang tersimpan sementara | Periksa kembali file dan perubahan sebelum melanjutkan; label perlu validasi UI |
| **Simpan** | Menyimpan hasil ke tujuan yang sedang dipilih | Jika berpotensi menimpa file, aplikasi harus meminta konfirmasi yang jelas |
| **Simpan sebagai File Baru** | Menyimpan hasil dengan nama atau lokasi baru | Pilihan yang disarankan bila ingin memastikan file asli tidak berubah |
| **Batalkan Perubahan** | Membuang perubahan yang belum disimpan | Harus ada peringatan bila perubahan akan hilang; label perlu validasi UI |
| **Amankan ke Akun** | Menyimpan hasil terpilih ke area aman akun | Berbeda dari Simpan/Autosave dan dapat memerlukan koneksi |

### Menyimpan hasil

1. Buka dan periksa hasil.
2. Pilih **Simpan** atau **Simpan sebagai File Baru**.
3. Tentukan nama dan lokasi bila diminta.
4. Jika aplikasi memperingatkan bahwa file akan ditimpa, periksa nama file dengan teliti.
5. Tunggu pesan bahwa penyimpanan selesai.
6. Buka file dari lokasi tujuan untuk memastikan hasil dapat digunakan.

### Memulihkan pekerjaan

Jika aplikasi tertutup tidak sengaja atau pekerjaan terhenti, aplikasi dapat menawarkan draft atau pekerjaan yang dapat dipulihkan.

1. Buka kembali PERTAMINA OTO.
2. Periksa ringkasan pekerjaan yang ditemukan.
3. Pilih salah satu tindakan:
   - **Lanjutkan** untuk kembali ke keadaan kerja yang aman;
   - **Coba Lagi** untuk mengulang proses yang aman setelah penyebabnya diperbaiki;
   - **Batalkan** untuk menghentikan dan membuang pekerjaan sementara sesuai penjelasan yang tampil.
4. Periksa input, urutan, pilihan, dan lokasi hasil.
5. Jalankan atau simpan kembali bila diperlukan.

Jika tidak ada draft yang dapat dipulihkan, pilih kembali file asli dan mulai proses baru. File hasil setengah jadi tidak boleh dianggap sebagai hasil final.

## 11. History

### Riwayat

Riwayat membantu pengguna menemukan aktivitas dan hasil pekerjaannya sendiri. Informasi yang berguna antara lain nama file, feature/tool, waktu, status, lokasi hasil, ukuran, dan durasi. Ketersediaan setiap detail harus disesuaikan dengan UI final.

Tindakan umum yang dapat tersedia:

- membuka hasil yang masih tersedia;
- membuka lokasi file;
- menjalankan kembali pekerjaan dengan input yang masih dapat diakses;
- melihat ringkasan hasil atau kegagalan;
- menghapus entri Riwayat pribadi.

Riwayat bukan backup. Jika file dipindah, dihapus, atau aksesnya berubah, entri Riwayat dapat tetap terlihat tetapi file mungkin tidak dapat dibuka. Menghapus entri Riwayat tidak boleh dipahami sebagai menghapus file asli, file yang diamankan, atau catatan enterprise.

### Riwayat dan Audit Trail

| Riwayat | Audit Trail |
|---|---|
| Membantu pengguna melanjutkan atau menemukan pekerjaan pribadi | Catatan enterprise internal untuk keamanan, kepatuhan, dan penelusuran |
| Menampilkan informasi yang relevan untuk aktivitas pengguna | Aksesnya dibatasi dan tidak menjadi menu kerja harian pengguna biasa |
| Dapat menawarkan buka hasil, buka lokasi, atau proses ulang | Tidak dijelaskan rinci dalam User Manual |
| Penghapusan entri mengikuti fungsi Riwayat | Tidak otomatis terhapus hanya karena entri Riwayat dihapus |

File yang dipilih melalui **Amankan ke Akun** berada pada area file aman dan bukan sekadar entri Riwayat.

## 12. Common errors

| Masalah | Kemungkinan penyebab | Yang dapat dilakukan pengguna |
|---|---|---|
| **File tidak didukung** | Format, versi, ukuran, atau kemampuan platform tidak sesuai | Periksa format; pilih file lain; gunakan tool yang sesuai; lakukan konversi hanya dari sumber tepercaya |
| **File rusak** | File tidak lengkap, penyimpanan gagal, atau isi tidak dapat dibaca | Coba buka file asli di aplikasi lain; gunakan salinan/cadangan; minta pengirim mengirim ulang |
| **Password salah** | Password salah atau file memakai perlindungan yang tidak didukung | Ketik ulang dengan memperhatikan huruf/angka; pastikan password berasal dari pemilik file; jangan terus menebak |
| **Penyimpanan penuh** | Ruang perangkat, area kerja, atau lokasi tujuan tidak cukup | Hapus atau pindahkan file yang tidak diperlukan; pilih lokasi lain; mulai ulang setelah ruang tersedia |
| **Koneksi putus** | Jaringan berubah atau perangkat offline | Jangan memulai ulang pekerjaan ganda; periksa Activity Center; sambungkan kembali; pilih Lanjutkan/Coba Lagi bila tersedia |
| **Layanan tidak tersedia** | Kemampuan yang memerlukan koneksi sedang tidak dapat digunakan | Simpan draft; gunakan kemampuan yang tersedia di perangkat; coba lagi nanti; laporkan jika berulang |
| **Proses gagal** | Input bermasalah, ruang tidak cukup, kemampuan tidak tersedia, atau proses terhenti | Baca detail; perbaiki kondisi; pastikan file asli aman; pilih Coba Lagi; gunakan Laporkan Masalah jika tetap gagal |
| **Aplikasi tertutup tidak sengaja** | Aplikasi berhenti, perangkat restart, atau jendela tertutup | Buka kembali aplikasi; pilih Lanjutkan/Coba Lagi bila pemulihan ditawarkan; periksa hasil sebelum menyimpan |
| **Hasil tidak ditemukan** | Hasil belum disimpan, dipindah, dihapus, atau lokasi tidak lagi dapat diakses | Periksa Riwayat dan Activity Center; buka lokasi yang tercatat; proses ulang dari file asli bila perlu |
| **Tindakan tidak diizinkan** | Hak akses, kepemilikan file, status akun, atau kebijakan tidak mengizinkan tindakan | Jangan mencoba melewati pembatasan; pastikan akun yang benar; hubungi pengelola akses atau bantuan resmi |

Sebelum memilih **Laporkan Masalah**:

1. Catat pekerjaan dan langkah terakhir yang dilakukan.
2. Pastikan file sensitif atau isinya tidak ditulis ke kolom deskripsi.
3. Periksa informasi diagnostik yang akan dikirim jika aplikasi menampilkannya.
4. Sertakan screenshot hanya bila aman dan diizinkan.
5. Kirim laporan, lalu simpan nomor laporan bila tersedia.

## 13. Screenshot requirements

Jangan membuat screenshot tiruan. Seluruh gambar harus diambil dari build aplikasi yang telah disetujui, menggunakan data contoh yang tidak sensitif.

Format placeholder yang digunakan selama dokumen belum memiliki screenshot:

`[PLACEHOLDER SCREENSHOT — <ID> — tangkap dari build UI final setelah label dan navigasi disetujui]`

| ID | Placeholder | Keadaan yang harus ditangkap | Callout minimum | Teks alternatif awal |
|---|---|---|---|---|
| LOGIN | `[PLACEHOLDER SCREENSHOT — LOGIN]` | Halaman masuk sebelum autentikasi | Login Pertamina; bantuan login | “Halaman login PERTAMINA OTO.” |
| HOME | `[PLACEHOLDER SCREENSHOT — HOME]` | Beranda tanpa file aktif | area file; tool; pencarian; Assistant; Activity Center | “Beranda PERTAMINA OTO dan pilihan untuk memulai.” |
| FILE PICKER | `[PLACEHOLDER SCREENSHOT — FILE PICKER]` | Pemilih file aktual pada platform utama | lokasi; pilihan file; tombol buka | “Pemilih file untuk menambahkan dokumen.” |
| FILE-FIRST | `[PLACEHOLDER SCREENSHOT — FILE-FIRST]` | File sudah ditambahkan dan rekomendasi terlihat | status file; rekomendasi; lanjut | “Alur File-First setelah file siap.” |
| TOOL-FIRST | `[PLACEHOLDER SCREENSHOT — TOOL-FIRST]` | Tool dipilih sebelum input | nama tool; area input; persyaratan | “Alur Tool-First pada tool yang dipilih.” |
| ASSISTANT-FIRST | `[PLACEHOLDER SCREENSHOT — ASSISTANT-FIRST]` | Rencana Assistant sebelum eksekusi | tujuan; feature/tool; parameter; konfirmasi | “Rencana Assistant yang perlu diperiksa pengguna.” |
| ACTIVITY CENTER | `[PLACEHOLDER SCREENSHOT — ACTIVITY CENTER]` | Terdapat proses berjalan, menunggu, selesai, dan gagal | status; detail; cancel/retry; buka hasil | “Activity Center dengan beberapa status pekerjaan.” |
| SAVE AS | `[PLACEHOLDER SCREENSHOT — SAVE AS]` | Dialog penyimpanan file baru | nama; lokasi; jenis file; simpan | “Dialog Simpan sebagai File Baru.” |
| RECOVERY | `[PLACEHOLDER SCREENSHOT — RECOVERY]` | Pekerjaan yang dapat dipulihkan setelah aplikasi dibuka kembali | ringkasan; Lanjutkan; Coba Lagi; Batalkan | “Pilihan pemulihan pekerjaan yang terhenti.” |

Standar pengambilan screenshot:

- gunakan build, platform, bahasa, dan tema yang dicantumkan pada metadata dokumen;
- gunakan akun uji dan file sintetis;
- sembunyikan nama pegawai, lokasi lokal, nomor identitas, isi dokumen, dan informasi sensitif;
- tampilkan satu tujuan utama per gambar;
- gunakan nomor callout yang konsisten dengan langkah;
- sertakan teks alternatif yang menjelaskan tujuan, bukan hanya tampilan;
- ambil ulang gambar bila label atau tata letak berubah;
- buat varian platform hanya jika perbedaan langkah benar-benar memengaruhi pengguna;
- lakukan review aksesibilitas, keamanan informasi, dan akurasi UI sebelum publikasi.

## 14. Terminology

| Istilah yang digunakan | Arti untuk pengguna |
|---|---|
| **PERTAMINA OTO** | Aplikasi untuk menyelesaikan pekerjaan dokumen melalui file, tool, atau Assistant |
| **Beranda** | Titik awal aplikasi; label final perlu validasi UI |
| **File asli** | File yang dipilih pengguna sebelum perubahan |
| **Area kerja** | Tempat sementara untuk mengatur input dan perubahan sebelum hasil disimpan |
| **Draft** | Keadaan kerja sementara yang dapat dipulihkan; bukan hasil final |
| **Hasil** | File atau keluaran yang selesai dibuat dan telah diperiksa aplikasi |
| **Tool** | Kemampuan untuk melakukan pekerjaan tertentu pada file |
| **File-First** | Memulai dari file yang sudah dimiliki |
| **Tool-First** | Memulai dari tool yang sudah diketahui |
| **Assistant-First** | Memulai dengan menjelaskan tujuan kepada Assistant |
| **Rekomendasi** | Saran tindakan berdasarkan file atau konteks; pengguna tetap memilih |
| **Preview** | Pratinjau perubahan sebelum hasil final dibuat |
| **Proses** | Pekerjaan yang sedang dilakukan aplikasi |
| **Progress/Progres** | Informasi tahap dan keadaan proses |
| **Activity Center** | Tempat memantau pekerjaan aktif, menunggu, selesai, atau gagal |
| **Simpan** | Menyimpan hasil ke tujuan yang dipilih |
| **Simpan sebagai File Baru** | Menyimpan hasil sebagai file lain agar file asli tidak berubah |
| **Autosave** | Penyimpanan keadaan kerja sementara secara otomatis |
| **Urungkan / Ulangi** | Membatalkan atau mengembalikan perubahan yang masih dapat dibalik |
| **Riwayat** | Daftar aktivitas dan hasil yang membantu pengguna menemukan pekerjaan sebelumnya |
| **Audit Trail** | Catatan enterprise internal; tidak dijelaskan rinci dalam User Manual |
| **Amankan ke Akun** | Menyimpan hasil terpilih ke area file aman pada akun |
| **Offline** | Perangkat tidak terhubung; hanya kemampuan yang tersedia tanpa koneksi yang dapat digunakan |
| **Menunggu koneksi** | Pekerjaan dapat dilanjutkan setelah koneksi tersedia dan akses diperiksa |
| **Menunggu sumber daya** | Pekerjaan menunggu ruang atau kemampuan yang diperlukan |
| **Lanjutkan** | Meneruskan pekerjaan dari keadaan yang aman |
| **Coba Lagi** | Mengulang tahap yang aman setelah penyebab masalah diperbaiki |
| **Batalkan** | Menghentikan pekerjaan; dampaknya harus dijelaskan sebelum perubahan dibuang |
| **Laporkan Masalah** | Mengirim laporan kendala melalui saluran dukungan aplikasi |

Istilah internal yang tidak digunakan untuk menjelaskan workflow pengguna: API, endpoint, database, SQL, Worker, service identity, schema, contract version, engine adapter, provider, Job ID, dan Correlation ID. Bila informasi teknis diperlukan oleh dukungan, aplikasi sebaiknya menyiapkannya melalui detail masalah tanpa meminta pengguna memahaminya.

Gunakan satu istilah secara konsisten. Jangan mencampur **Riwayat** dengan **Audit Trail**, **Autosave** dengan **Simpan**, atau **Amankan ke Akun** dengan penyimpanan file biasa.

## 15. Risks/gaps

| ID | Risiko atau kesenjangan | Dampak pada User Manual | Tindakan yang disarankan sebelum publikasi |
|---|---|---|---|
| R-01 | Document 13 FINAL tidak tersedia; ringkasan proyek masih menyebut Document 13 sebagai berikutnya | UAT final belum dapat dipakai sebagai bukti penerimaan workflow | Sinkronkan laporan ini setelah Document 13 FINAL diterbitkan |
| R-02 | Build UI final dan inventaris label tidak tersedia | Nama menu, tombol, ikon, dan lokasi kontrol dapat keliru | Lakukan UI string inventory dan walkthrough build kandidat rilis |
| R-03 | Screenshot aktual belum tersedia | Pengguna belum mendapat petunjuk visual | Isi placeholder hanya dari build yang disetujui dan data sintetis |
| R-04 | Struktur Beranda/navigasi final belum dikonfirmasi | Urutan dan lokasi area dapat berbeda per platform | Validasi seluruh item bertanda VALIDASI_UI_REQUIRED |
| R-05 | Detail tampilan login, verifikasi ulang, dan pesan akun dicabut belum final | Langkah login dapat tidak cocok dengan layar aktual | Validasi dengan pemilik SSO/security dan build integrasi |
| R-06 | Arti UI **Simpan** terhadap target aktif belum dibuktikan | Pengguna dapat salah mengira file asli akan/tidak akan berubah | Tetapkan copywriting dan konfirmasi overwrite yang eksplisit |
| R-07 | Tindakan, filter, dan detail Activity Center belum diinventarisasi dari UI | Manual dapat menjanjikan cancel/retry/resume yang tidak tersedia pada semua keadaan | Buat state/action matrix dari build final |
| R-08 | Aksi Riwayat dan ketersediaan file berbeda menurut platform/lokasi | Pengguna dapat menganggap Riwayat sebagai backup | Uji open result, open location, reprocess, dan delete pada tiap platform |
| R-09 | Share/Open With, Paste, Scan, folder input, dan scanner fisik tergantung platform/rilis | Panduan dapat menyatakan dukungan terlalu luas | Terbitkan capability matrix per platform dan rilis |
| R-10 | Masa berlaku sesi offline dan detail tindakan online-only bukan keputusan User Manual | Janji durasi offline dapat salah | Gunakan pesan “selama sesi masih valid”; referensikan kebijakan final tanpa menebak angka |
| R-11 | Copywriting status/error dan bahasa UI belum dibekukan | Istilah pada gambar dan teks dapat tidak konsisten | Lakukan review lokalisasi dan content-design sebelum sign-off |
| R-12 | Perilaku recovery per jenis gangguan belum dibuktikan pada UAT final | Lanjutkan/Coba Lagi/Batalkan dapat tidak tersedia untuk semua keadaan | Cocokkan manual dengan hasil recovery UAT final |
| R-13 | Penempatan Help/Laporkan Masalah dan informasi diagnostik yang terlihat belum final | Pengguna kesulitan meminta bantuan atau dapat membagikan data berlebih | Uji alur laporan dengan privacy preview dan data sintetis |
| R-14 | Perbedaan Desktop, Web, Phone, dan Tablet belum memiliki bukti langkah final | Satu panduan dapat membingungkan platform lain | Gunakan panduan inti bersama dan tambahkan varian hanya saat langkah berbeda |
| R-15 | Kesesuaian aksesibilitas dan navigasi keyboard belum divalidasi dari build | Panduan tidak cukup membantu pengguna kebutuhan aksesibilitas | Tambahkan hasil UAT aksesibilitas dan deskripsi fokus/label yang benar |

Potensi konflik yang harus dijaga saat menyusun master:

1. **“Login sekali” bukan berarti sesi tidak pernah berakhir.** Manual harus tetap menjelaskan login kembali, pemeriksaan saat reconnect, dan verifikasi ulang untuk tindakan sensitif.
2. **Menambahkan file bukan selalu mengunggah file.** Gunakan istilah “tambahkan/pilih file”; jelaskan secara khusus hanya saat konten perlu dikirim.
3. **Simpan tidak boleh melemahkan source protection.** Tidak ada overwrite diam-diam; Simpan sebagai File Baru harus mudah dipilih.
4. **Autosave bukan hasil final.** Draft pemulihan tidak boleh dipromosikan sebagai file yang sudah selesai.
5. **Riwayat bukan Audit Trail dan bukan backup.** Ketiganya memiliki tujuan berbeda.
6. **Assistant bukan jalur khusus yang melewati kontrol.** Rencana, izin, validasi, konfirmasi, progress, dan penyimpanan tetap berlaku.
7. **Proses background bukan proses tanpa informasi.** Activity Center dan No Silent Wait harus konsisten pada seluruh platform.

## 16. Suggested Document 14 text

Teks berikut dapat dipindahkan ke bagian Getting Started pada Document 14 setelah semua label bertanda **VALIDASI_UI_REQUIRED** dicocokkan dengan UI final dan hasil UAT Document 13 FINAL tersedia.

---

### Memulai PERTAMINA OTO

PERTAMINA OTO membantu Anda menyelesaikan pekerjaan dokumen dengan tiga cara: mulai dari file, mulai dari tool, atau jelaskan tujuan kepada Assistant. Ketiga cara tersebut akan memeriksa file, menampilkan proses, dan meminta Anda memeriksa hasil sebelum menyimpan.

#### Mulai dalam 5 menit

1. Buka PERTAMINA OTO.
2. Login dengan akun Pertamina jika diminta.
3. Tambahkan file, pilih tool, atau jelaskan tujuan kepada Assistant.
4. Periksa pilihan, lalu jalankan proses.
5. Periksa hasil dan pilih **Simpan** atau **Simpan sebagai File Baru**.

File asli Anda tidak ditimpa secara diam-diam. Untuk menjaga file asli, gunakan **Simpan sebagai File Baru**.

#### Login dan bekerja offline

Gunakan akun Pertamina untuk masuk. Selama sesi masih aktif, Anda dapat melanjutkan pekerjaan tanpa login pada setiap halaman. Aplikasi akan meminta login kembali bila sesi berakhir atau akses perlu diperiksa ulang.

Saat tidak ada koneksi, kemampuan yang tersedia di perangkat tetap dapat digunakan selama sesi offline masih valid. Kemampuan yang memerlukan koneksi akan menunjukkan **Memerlukan koneksi** atau **Menunggu koneksi**. Setelah tersambung kembali, aplikasi dapat memeriksa akun dan akses sebelum melanjutkan.

Sebelum logout, simpan pekerjaan dan periksa Activity Center. Untuk logout, buka **[Profil — VALIDASI_UI_REQUIRED]**, lalu pilih **Logout**.

#### Mengenal halaman utama

Pada **[Beranda — VALIDASI_UI_REQUIRED]**, Anda dapat:

- menambahkan file;
- melihat dan mencari tool;
- memilih rekomendasi untuk file;
- membuka Assistant;
- memantau pekerjaan di Activity Center;
- membuka Riwayat;
- membuka file yang diamankan ke akun;
- membuka Bantuan atau Laporkan Masalah.

Pada Phone atau Tablet, sebagian menu dapat berada di menu tambahan. Cari nama fungsi yang sama.

#### Pilih cara memulai

**File-First — jika Anda sudah mempunyai file**

1. Tambahkan file.
2. Periksa status file.
3. Pilih rekomendasi atau tool.
4. Atur pilihan dan jalankan.
5. Periksa dan simpan hasil.

Contoh: tambahkan dua PDF, pilih **Gabungkan PDF**, atur urutannya, lalu simpan sebagai file baru.

**Tool-First — jika Anda sudah tahu tool yang dibutuhkan**

1. Buka daftar tool atau pencarian.
2. Pilih tool.
3. Tambahkan file yang diminta.
4. Periksa persyaratan dan atur pilihan.
5. Jalankan, periksa, dan simpan hasil.

Contoh: cari **Kompres PDF**, tambahkan file, pilih tingkat kompresi, lalu simpan hasil.

**Assistant-First — jika Anda lebih mudah menjelaskan tujuan**

1. Buka Assistant.
2. Tulis tujuan Anda.
3. Tambahkan file bila diminta.
4. Periksa tool, langkah, dan pilihan yang diusulkan.
5. Konfirmasi tindakan yang sensitif atau sulit dibatalkan.
6. Jalankan, periksa, dan simpan hasil.

Contoh: “Gabungkan dua PDF ini, beri nomor halaman, lalu kompres hasilnya.” Assistant tetap mengikuti pemeriksaan file, izin, konfirmasi, dan aturan penyimpanan yang sama.

#### Menambahkan file

Gunakan cara yang tersedia pada perangkat Anda:

- **File Picker:** pilih **[Tambahkan File — VALIDASI_UI_REQUIRED]**, cari file, lalu pilih Buka.
- **Drag-and-Drop:** pada Desktop atau Web yang mendukung, seret file ke area input.
- **Paste:** salin file atau gambar, fokuskan area input, lalu tempel.
- **Share/Open With:** pada perangkat yang mendukung, bagikan file dari aplikasi lain ke PERTAMINA OTO.
- **Scan:** ambil halaman, periksa potongan dan orientasi, lalu tambahkan hasil scan.
- **Recent Files:** pilih file terbaru yang masih tersedia pada lokasi asalnya.

Menambahkan file ke area kerja tidak selalu berarti file dikirim secara online. Jika suatu tindakan memerlukan pengiriman konten, aplikasi akan menjelaskannya sebelum Anda melanjutkan.

#### Memahami status file

- **Siap:** lanjutkan ke tool atau proses.
- **Memerlukan password:** masukkan password yang benar.
- **Peringatan:** baca dampaknya sebelum melanjutkan.
- **Tidak didukung:** pilih format atau tool lain.
- **Rusak:** gunakan salinan yang sehat atau minta file dikirim ulang.
- **Diblokir:** jangan melewati pembatasan; gunakan file yang diizinkan atau minta bantuan.
- **Penyimpanan tidak cukup:** kosongkan ruang atau pilih lokasi lain, lalu coba lagi.

Menghapus file dari daftar input tidak menghapus file asli.

#### Menjalankan pekerjaan

1. Periksa file dan pilih tool.
2. Atur urutan, halaman, kualitas, atau pilihan lain.
3. Periksa preview jika tersedia.
4. Jalankan proses.
5. Jika tindakan dapat menimpa, menghapus permanen, membagikan, atau mengirim konten, baca konfirmasi dengan teliti.
6. Pantau status pada halaman atau Activity Center.
7. Buka dan periksa hasil.
8. Simpan hasil ke nama dan lokasi yang benar.

Untuk perubahan biasa yang masih dapat dibalik, gunakan **Urungkan** atau **Ulangi**. Aplikasi tidak perlu meminta konfirmasi untuk setiap perubahan kecil.

#### Memantau proses di Activity Center

Activity Center menampilkan pekerjaan yang **Menunggu**, **Berjalan**, **Menunggu koneksi**, **Menunggu sumber daya**, **Selesai**, **Gagal**, atau **Dibatalkan**.

1. Buka Activity Center.
2. Pilih pekerjaan untuk melihat detail.
3. Gunakan **Batalkan**, **Coba Lagi**, **Lanjutkan**, atau **[Buka Hasil — VALIDASI_UI_REQUIRED]** jika tindakan tersebut tersedia.

Jika Anda meninggalkan halaman proses, pekerjaan latar belakang dapat tetap berjalan. Periksa Activity Center sebelum memulai proses yang sama lagi.

#### Menyimpan dan memulihkan pekerjaan

Perubahan berada di area kerja sampai Anda menyimpan hasil.

- **Autosave** menyimpan keadaan kerja sementara; bukan hasil final.
- **Simpan** menyimpan ke tujuan yang dipilih dan meminta konfirmasi bila ada risiko menimpa.
- **Simpan sebagai File Baru** membuat hasil dengan nama atau lokasi baru.
- **Amankan ke Akun** menyimpan hasil terpilih ke area aman akun dan dapat memerlukan koneksi.

Jika aplikasi tertutup tidak sengaja, buka kembali aplikasi dan periksa pilihan pemulihan:

- pilih **Lanjutkan** untuk kembali ke pekerjaan yang aman;
- pilih **Coba Lagi** setelah penyebab kegagalan diperbaiki;
- pilih **Batalkan** jika pekerjaan sementara tidak lagi diperlukan.

Periksa kembali input, urutan, pilihan, dan lokasi hasil setelah pemulihan.

#### Menggunakan Riwayat

Riwayat membantu Anda menemukan aktivitas dan hasil sebelumnya. Anda dapat melihat ringkasan dan, jika file masih tersedia, membuka hasil atau lokasi file. Riwayat tidak membuat backup otomatis. Jika file dipindah atau dihapus, file tersebut mungkin tidak dapat dibuka dari Riwayat.

Audit Trail adalah catatan enterprise internal dan berbeda dari Riwayat pengguna. Menghapus entri Riwayat tidak otomatis menghapus file asli, file yang diamankan, atau catatan enterprise.

#### Jika terjadi masalah

1. Baca penyebab dan tindakan yang ditampilkan.
2. Pastikan file asli masih tersedia.
3. Perbaiki kondisi—misalnya sambungkan koneksi, kosongkan ruang, atau pilih file yang sehat.
4. Periksa Activity Center agar tidak menjalankan pekerjaan ganda.
5. Pilih **Coba Lagi** bila tersedia.
6. Jika masalah tetap terjadi, pilih **Laporkan Masalah**. Jangan menulis isi dokumen, password, atau informasi sensitif dalam laporan.

---

Sebelum teks ini masuk ke master Document 14, pemilik dokumen perlu menghapus seluruh tanda **VALIDASI_UI_REQUIRED** hanya setelah bukti UI final, capability per platform, dan hasil UAT final tersedia.
