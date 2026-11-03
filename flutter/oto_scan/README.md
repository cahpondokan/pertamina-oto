# Cadangan Android Google ML Kit — 1.0.1+19

Snapshot 3 November 2026. Baca [BACKUP_MLKIT.md](../../BACKUP_MLKIT.md) untuk paket, flag build Google, dan pemisahan dari versi algoritma sendiri. Catatan lama di bawah adalah riwayat.

---

# OTO Scan Flutter

## Backup source GitHub — 23 Oktober 2026

Snapshot ini menyimpan source aplikasi **1.0.0+17**, package `com.niam.ayani.oto`, beserta aset, konfigurasi build, dependency lock, test, dan tool pengembangan. Kode aplikasi tidak diubah untuk backup ini. Kandidat Alpha sudah diajukan ke Google Play; pengajuan bukan persetujuan atau rilis publik.

Folder `evidence/`, `dist/`, `release-output/`, cache/build, signing key, password, dan skrip pemasangan historis yang berisi identitas HP tidak disertakan dalam Git. Rujukan bukti dan APK pada riwayat di bawah adalah arsip lokal pemilik; tidak seluruhnya tersedia pada clone baru. Skrip `tool/` yang merujuk path SDK atau arsip lokal perlu disesuaikan dengan lingkungan pengguna. Jangan membuat ulang upload key untuk memperbarui aplikasi yang sama di Google Play; gunakan key pemilik melalui penyimpanan rahasia yang aman.

Aplikasi Android lokal sesuai desain Scan â†’ Rapikan â†’ Halaman â†’ PDF, dengan galeri dan pembaca PDF. Memakai ikon OTO yang sudah ada, Flutter untuk seluruh layar, dan adapter Camera2/Bitmap/PDF Android melalui MethodChannel. Mulai versi 0.1.8, application ID adalah `com.niam.ayani.oto` dengan nama **OTO Scan**. Android memasangnya terpisah dari **OTO Scan Flutter** (`id.ayani.oto.scanflutter`); hasil lama tetap berada di aplikasi lama dan tidak otomatis dipindahkan. Namespace Java internal tetap `id.ayani.oto.oto_scan`; ini bukan application ID.

## Kandidat Google Play 1.0.0+17 — 22 Oktober 2026

Paket `release-output/1.0.0/OTO-Scan-1.0.0.aab` disiapkan untuk Google Play dengan application ID `com.niam.ayani.oto`, minimum Android 10 dan target API 36. APK universal pendamping dipakai untuk verifikasi emulator. Kandidat ini belum diterbitkan dan tidak dipasang ke HP pengguna. Status bukti dan hambatan publikasi ada pada `release-output/1.0.0/RELEASE_HANDOFF.md`.

Build memakai `tool/Build-PlayRelease.ps1 -Kind appbundle` atau `-Kind apk`, dengan upload key terpisah dari signer uji historis. `.release-private/` berisi materi rahasia yang dilindungi ACL/Windows DPAPI dan tidak boleh disalin ke paket, Git, atau log. Pemilik perlu menyimpan backup aman sebelum distribusi. Kebijakan privasi dan lisensi tersedia dari Dokumen Saya.

## Terpasang di HP: revisi 16 — 22 Oktober 2026

**OTO Scan 0.1.15+16 sudah dipasang dan dibuka pada HP pengguna.** Pembaca PDF mempertahankan titik halaman di bawah jari saat zoom, termasuk halaman landscape pendek dan perubahan rasio yang selesai dimuat selama gerakan. Pelepasan jari setelah pinch tidak memicu gulir otomatis. Rasio potret/landscape, gulir vertikal, detail zoom dan tombol Pas lebar layar tetap didukung; preview awal memakai contain agar tidak meregang.

88 tes Flutter, analisis Dart, build release ARM64 offline dan 21 pemeriksaan APK PASS. Versi/hash terpasang cocok, launch berhasil. Semua kode native kamera, crop, orientasi, peningkatan teks dan kompresi identik dengan r15; PDF tersimpan tidak ditulis ulang. Tidak ada pengujian UI/kamera atau pembacaan dokumen HP oleh agen. Pengguna dapat membuka ulang PDF lama untuk mencoba pembaca terbaru. APK `dist/OTO-Scan-0.1.15.apk`; bukti `evidence/r16-zoom-fix.md` dan `evidence/install-handoff-r16.json`.

## Riwayat pemasangan revisi 15 — 22 Oktober 2026

**OTO Scan 0.1.14+15 sudah dipasang dan dibuka pada HP pengguna**, menggantikan 0.1.13. Memperbaiki regresi crop: guide yang benar kedaluwarsa ketika aplikasi menunggu fokus sampai 1400 ms, sehingga deteksi foto dilewati. Pilihan objek kini divalidasi saat tombol foto ditekan dan dipertahankan selama penantian fokus; keempat tepi tetap harus diverifikasi pada foto. Geometri, kualitas teks, kompresi PDF dan seluruh source Flutter tetap terjaga.

238 pemeriksaan algoritme desktop, build release ARM64 offline dan 21 pemeriksaan APK PASS. Versi/hash terpasang cocok; launch Android berhasil. Tidak ada pengujian kamera/UI, pengambilan foto atau pembacaan dokumen HP oleh agen; pengguna mencoba sendiri. APK `dist/OTO-Scan-0.1.14.apk`; bukti `evidence/r15-crop-fix.md`, `evidence/r15-installation.json`, `evidence/install-handoff-r15.json`.

## Riwayat pemasangan revisi 14 — 22 Oktober 2026

**OTO Scan 0.1.13+14 sudah dibuat, dipasang dan dibuka pada HP pengguna.** Paket `com.niam.ayani.oto`, APK `dist/OTO-Scan-0.1.13.apk`, SHA256 `ddba095ea4100fdd8b7122bcbd3a21c71e54a66c739acd34455868fa32029fd2`. Ini menggabungkan perbaikan kualitas teks/kompresi r13 dengan perbaikan orientasi, rasio/ukuran PDF dan crop objek kecil dari audit. APK 0.1.12 sebelumnya tetap artefak historis dan tidak dipasang.

Build release ARM64 offline PASS; 21 pemeriksaan APK PASS. Versi, hash APK terpasang dan respons launch Android cocok. Source algoritme/UI tetap sama dengan hasil audit yang lulus 174 pemeriksaan JVM dan 80 tes Dart/widget; hanya versi dan dokumentasi diperbarui untuk r14. Tidak ada pengujian kamera/UI, pengambilan foto, pembacaan dokumen pribadi, pengiriman, penghapusan data, atau uninstall. Pengguna mencoba langsung; keberhasilan orientasi/crop pada kamera HP belum dinyatakan terverifikasi. Bukti: `evidence/r14-installation.json`, `evidence/install-handoff-r14.json`.

## Perbaikan source orientasi dan crop — 22 Oktober 2026

Source setelah r13 memuat perbaikan P1/P2 audit AOS-01–06: orientasi fisik capture dan pemetaan sudut, deteksi objek kecil melalui ROI terverifikasi, pencegahan perpindahan target, urutan sudut yang stabil, preset rasio crop, serta ukuran PDF yang konsisten saat rotasi. Kode kualitas teks, JPEG sumber/EXIF, autofocus, kompresi dan pembaca PDF r13 dipertahankan. Laporan dan diff: `evidence/fix-orientation-small-crop/REPORT.md`.

**Catatan historis tahap audit:** saat tahap perbaikan source selesai, APK belum dibuat dan APK 0.1.12 belum memuat perubahan ini. Build/install/launch berikutnya sudah selesai sebagai revisi 14 di atas. Tes desktop: 174 pemeriksaan JVM dan 80 tes Dart/widget PASS; analisis Dart bersih. Integrasi Camera2/EXIF/Bitmap dan pengujian perangkat ditunda sesuai instruksi pengguna. Mode Bebas memakai estimasi foto; preset A4/kartu menetapkan rasio, bukan kalibrasi ukuran fisik dokumen.

## Paket historis revisi 13

Kandidat pengembangan Android 10+ / ARM64: `dist/OTO-Scan-0.1.12.apk`, versi **0.1.12+13**. Pemeriksaan komputer selesai. **Instalasi ditunda atas permintaan pengguna sampai audit orientasi/proporsi/crop objek kecil dan perbaikannya selesai.** APK r13 belum dipasang/dibuka; status ada di `evidence/install-handoff-r13.json`. Paket tetap `com.niam.ayani.oto`, label **OTO Scan**, dengan key uji OTO yang sama. Pengujian UI/kamera/dokumen nyata di HP dilakukan pengguna sendiri.


## Cara menggunakan

1. Buka **OTO Scan**, izinkan kamera, lalu arahkan ke objek kotak dengan seluruh tepinya terlihat.
2. Garis merah mengikuti tepi yang terdeteksi. Ketuk kertas untuk memilih objek dan meminta fokus kamera di sana; kemampuan fokus bergantung hardware HP. Pergeseran besar harus konsisten pada dua pengamatan sebelum garis berpindah. Tekan tombol foto; sistem mendeteksi ulang objek yang dipilih pada foto, memotong empat sisi, dan meluruskan perspektif. Pilihan objek direset saat kamera dibuka kembali.
3. Bila deteksi tidak yakin, layar **Rapikan** membuka empat sudut yang dapat digeser. Putar/filter diterapkan ketika menekan **Gunakan halaman**.
4. Cubit gambar pada Halaman atau Buka PDF untuk memperbesar hingga 6Ã—. Filter **Jernih** otomatis meningkatkan kontras dan ketajaman ringan; pilih **Warna asli** di Rapikan jika ingin warna tanpa peningkatan tersebut. Tambah, ambil ulang, hapus, atau tahan thumbnail untuk mengubah urutan. Maksimal 20 halaman per PDF.
5. Tekan **Simpan PDF**, beri nama, dan PDF langsung terbuka. Setelah penyimpanan berhasil, sesi scan selesai: Kembali dari preview menuju **Dokumen Saya**, dengan dokumen terbaru terlihat. **Scan baru** memulai halaman kosong. Gulir ke bawah untuk membaca seluruh halaman secara berkelanjutan; cubit untuk zoom. Saat diperbesar, satu jari tetap dapat menggulir ke halaman berikutnya. Tombol **Pas lebar layar** mengembalikan ukuran tampilan. **Kirim** tersedia langsung, sedangkan **Simpan salinan**, **Ganti nama**, dan **Hapus** berada pada menu titik tiga. Hasil tetap di ruang privat aplikasi; Simpan salinan membuka pemilih lokasi Android. Menghapus aplikasi juga menghapus penyimpanan privatnya.
6. **Dokumen Saya** menampilkan galeri thumbnail, nama, jumlah halaman, tanggal, ukuran, dan pencarian nama. Ketuk kartu untuk langsung membaca PDF. Kembali mempertahankan pencarian dan posisi gulir galeri.
7. Tekan lama kartu atau tekan **Pilih**, lalu ketuk kartu lain untuk memberi tanda centang. Tekan **Kirim (jumlah)** untuk mengirim 1â€“100 PDF sekaligus sebagai file terpisah, atau **Hapus** dengan konfirmasi. Pilihan tetap tersimpan saat pencarian berubah. **Selesai** atau Kembali membatalkan mode pilih terlebih dahulu.
8. Kembali dari galeri mengikuti layar asal; dari Halaman kembali ke kamera untuk melanjutkan draf; dari kamera keluar aplikasi. Rapikan dengan perubahan memberi pilihan **Terapkan**, **Buang perubahan**, atau **Batal kembali**. Pembukaan ulang menawarkan **Lanjutkan scan** atau **Scan baru**. Scan baru meminta konfirmasi sebelum membuang draf; PDF tersimpan tidak ikut dihapus. Kembali saat proses simpan menunggu hasil; bila berhasil langsung menuju galeri, bila gagal tetap di halaman scan dengan draf utuh.
9. PDF yang dibuka dari aplikasi lain langsung masuk pembaca dan Kembali menutup OTO untuk kembali ke aplikasi pemanggil, termasuk ketika OTO sebelumnya sudah aktif. PDF luar tetap hanya dibaca, tidak otomatis dimasukkan ke Dokumen Saya.

Kirim hanya membuka pilihan berbagi. Aplikasi tidak memilih penerima atau menyatakan dokumen sudah terkirim.

## Detail teks, kompresi dan zoom — revisi 13

Kamera mengarahkan area fokus ke objek pilihan, menunggu fokus/exposure dan kestabilan dua sampel preview. Status fokus gagal tidak lagi dianggap berhasil. Batas tunggu 1,4 detik menjaga kamera lambat/fixed-focus tetap dapat mengambil foto; hasil yang fokus/gerakannya belum pasti mendapat saran periksa/ambil ulang. Pemeriksaan penyebaran tepi pada hasil foto memberi saran tambahan untuk blur, bukan jaminan ketajaman atau penolakan halaman kosong. JPEG kamera asli disimpan sebagai sumber draf; edit membaca orientasi EXIF yang sama dengan capture sehingga tidak melewati penyimpanan JPEG ulang. Toleransi 3% pada pemilihan/decode menghindari foto 4032×3024 turun menjadi seperempat hanya karena sedikit melewati 12 juta piksel. Output tetap dibatasi memori, tanpa memperbesar piksel sumber.

Filter Jernih meratakan pencahayaan menggunakan estimasi kertas lokal dan penajaman ringan. Hitam-putih memakai Sauvola lokal dengan penyimpanan baris terbatas, menggantikan ambang tetap 155. Mode Warna asli tetap tersedia. Binarisasi mengubah gambar menjadi dua warna sehingga tidak menjanjikan seluruh tinta sangat pucat tetap ada; mode Jernih berwarna tidak dibinarisasi otomatis.

Ekspor PDF memilih berdasarkan ukuran hasil encoding pada raster yang sama: hitam-putih murni memakai Flate 1 bit; halaman seperti kertas mencoba RGB Flate dengan prediksi baris yang mempertahankan setiap piksel, kemudian lapisan foreground RGB+mask lossless di atas latar JPEG bila lebih kecil; sisanya memakai JPEG90. Foreground berwarna, antialiasing, dan stempel disalin sebagai piksel, tanpa OCR/generasi huruf. White matte pada mask mencegah halo saat render. Kandidat berlapis dipilih hanya jika seluruh stream plus allowance struktur lebih kecil dari JPEG90; foto/halaman padat memiliki fallback. Tidak ada dependency baru. CCITT Group 4 dibandingkan pada fixture BW: Flate stream 17.157 byte vs TIFF Group4 23.440 byte termasuk container; Flate yang sudah ada dipertahankan, bukan klaim bahwa selalu unggul pada semua dokumen.

Preview PDF memakai raster dasar dan tile PNG maksimal 513×513 piksel dengan satu piksel overlap. Detail mengikuti area yang benar-benar terlihat, memprioritaskan halaman tengah viewport dan mencakup halaman sebelah yang terlihat. Batas bitmap seluruh halaman 8 MP tidak lagi membatasi detail zoom; lebar virtual maksimal 12.000 piksel, maksimal 24 tile aktif, antrean serial, buang hasil basi, cache disk 64 MiB/7 hari, dan retry. PNG detail tidak menambah kehilangan JPEG. Ini hanya tampilan; PDF lama tidak ditulis ulang. Fokus yang tidak terekam dan detail yang sudah hilang tetap tidak bisa dipulihkan.

Verifikasi komputer: 74 tes Flutter, analisis Dart, 16 pemeriksaan kualitas baru, 14 regresi kualitas, 23 pemeriksaan cache/raster, 19 pemeriksaan penulis PDF, serta 60 pemeriksaan independen pypdf/PDFium/Poppler. Visual reader sintetis termasuk zoom/scroll lintas halaman dan gambar 6–12 pt diperiksa. Contoh teks: 618.401 → 352.830 byte; teks dengan stempel: 642.758 → 364.569 byte; bayangan: 651.139 → 563.680 byte. Ketiga keluaran memakai RGB lossless, seluruh piksel hasil proses cocok persis setelah decode. Angka encoder/waktu adalah Java ImageIO di komputer, bukan Android/kamera/HP. Ukuran dan hasil nyata tetap bergantung sumber. Bukti: `evidence/r13-quality/verification.json`, `r13-quality-tests.log`, `r13-flutter-tests.log`, `r13-reader-visual.log`, `r13-reader-zoom-desktop.png`.

Audit orientasi/proposi potret-landscape dan crop objek kecil berada di task terpisah, menggunakan baseline beku `evidence/r13-baseline/`. Revisi ini tidak mengubah detektor, transformasi koordinat, orientasi activity, atau kebijakan ukuran fisik halaman; perbaikan temuan tersebut belum diklaim di APK ini. Tidak ada uji kamera, screenshot, instrumentation, baca dokumen privat, atau pengiriman nyata dari HP oleh agent.

## Sesi scan selesai setelah simpan — revisi 12

Alur: Scan → Rapikan/Halaman → Simpan → Preview PDF (Kirim tersedia) → Kembali → Dokumen Saya. Setelah native save mengonfirmasi PDF valid, thumbnail galeri yang terpisah dari foto draf disiapkan dan daftar dokumen diperbarui. Checkpoint kosong ditulis secara atomik oleh penyimpanan native yang sudah ada; foto draf kemudian dibersihkan. Baru setelah itu daftar halaman, pilihan, dan indeks ambil ulang direset. Galeri menjadi halaman di bawah preview; pencarian/pilihan/posisi galeri direset khusus setelah menyimpan agar dokumen terbaru terlihat. Membuka PDF biasa dari galeri tetap mempertahankan pencarian dan posisi sebelumnya.

Tombol galeri sekarang **Scan baru**. Draf yang belum disimpan tetap meminta konfirmasi sebelum dibuang; setelah penyimpanan berhasil tidak ada halaman lama yang terbawa. Batal penamaan, save gagal, atau respons save yang tidak terkonfirmasi tidak membersihkan draf. Bila PDF sudah tersimpan tetapi checkpoint kosong gagal, tersedia **Selesaikan penyimpanan** untuk mencoba lagi tanpa membuat PDF kedua selama sesi aplikasi yang sama. Back ketika save masih berlangsung menunggu hasil, lalu menuju galeri jika berhasil. Thumbnail/list gagal tidak menghilangkan PDF yang sudah valid. Draft lama sebelum pembaruan tidak dihapus otomatis. Gangguan proses sebelum checkpoint kosong selesai dapat meninggalkan draf untuk pemulihan; ini bukan transaksi gabungan PDF+draf yang atomik terhadap process kill.

Verifikasi komputer: 72 tes Flutter PASS, analisis Dart tanpa masalah, build release ARM64 offline PASS, serta 21 pemeriksaan APK PASS. Kasus uji meliputi Back toolbar/sistem, satu/dua halaman, Kirim dari preview, scan baru kosong dan buka ulang, Back saat save, cancel/gagal, retry penyelesaian tanpa duplikat, dan fallback thumbnail/list. Native storage/PDF/camera, deteksi, enhancement, pembaca vertikal/zoom/cache, ikon, izin, dan dependency tidak berubah. Tidak ada uji UI/kamera/dokumen di HP. Bukti: `evidence/r12-flutter-tests.log`, `r12-preservation.json`, dan `install-handoff-r12.json`.

## Pembaca vertikal dan pemuatan bertahap — revisi 11

PDF tampil sebagai daftar halaman vertikal berkelanjutan, dengan jarak antarlembar dan nomor halaman yang mengikuti bagian tengah layar. Gulir tetap berjalan saat zoom hingga 6×. Halaman pertama menggunakan thumbnail galeri atau gambar hasil scan yang sudah tersedia sambil menunggu render sesuai layar. Resolusi tinggi baru diminta setelah zoom/gerakan berhenti; gambar yang sudah terlihat dipertahankan sampai penggantinya siap. Metadata ukuran halaman memperbarui tata letak dengan menjaga posisi baca.

Pembaca mempunyai worker dan sesi PdfRenderer tersendiri; thumbnail tidak lagi mengantrekan pekerjaan pembaca. Satu render berjalan pada satu waktu. Halaman terlihat diprioritaskan, kemudian satu halaman berikutnya dimuat lebih awal. Perpindahan cepat membuang hasil yang sudah tidak relevan dan mengganti sisa antrean; pekerjaan native yang sudah berjalan selesai terlebih dahulu. Menutup pembaca menutup sesi dan menghentikan pekerjaan lanjutan. Hasil render dikirim sebagai byte JPEG (94 untuk layar, 98 untuk detail), tanpa jalur PNG besar lalu dibaca ulang. Ini hanya pratinjau; byte PDF simpan/kirim tidak berubah.

Raster layar maksimum 2 MP/2200 piksel sisi panjang; detail maksimum 8 MP/3800 piksel. Memori menyimpan halaman sekitar viewport dengan satu halaman detail aktif dan sasaran cache 48 MiB, bukan jaminan total memori proses. Cache disk terpisah dibatasi 64 MiB/7 hari dengan identitas file, halaman, ukuran raster, dan tingkat detail. Cache valid digunakan kembali; retry membuang seluruh variasi halaman terkait; hapus dokumen membuang cache dokumen. Cache PDF luar dibuang ketika sesi ditutup. Kamera, detektor, peningkatan gambar, kompresi PDF, ikon, permission, dan dependency lock dipertahankan.

Verifikasi komputer: 67 tes Flutter, 23 pemeriksaan cache/batas raster Java, 16 pemeriksaan pengelolaan dokumen Java, analisis Dart, build ARM64 offline, dan 21 pemeriksaan APK PASS. Tes mencakup gulir vertikal saat zoom, kembali saat render tertunda, penggantian dokumen luar, prioritas viewport, buang hasil basi, retry, cache, galeri, multi-kirim, draf, dan alur Kembali. Gambar `evidence/r11-reader-desktop.png` dan `evidence/r11-reader-page2-desktop.png` memakai dokumen sintetis.

Pengukuran referensi komputer atas PDF scan sintetis 1/2/10/20 halaman memakai PDFium/Pillow: render awal layar sekitar 91–95 ms, pembacaan cache + decode sekitar 13–15 ms, dibanding jalur referensi 8 MP PNG sekitar 936–1083 ms. **Angka ini bukan pengukuran Android PdfRenderer atau kecepatan HP.** Pratinjau detail mempertahankan 100% piksel tinta gelap pada ambang uji, dengan galat kanal rata-rata 0,304/255; visual 6–12 pt juga diperiksa. Hash sumber PDF tetap sama. Bukti lengkap: `evidence/r11-reader/measurements.json`. PDF rumit, sumber luar, atau cache pertama masih dapat memerlukan waktu; raster/zoom tidak menambah informasi yang tidak ada pada scan asli. Pengujian nyata di HP diserahkan kepada pengguna.

## Galeri, navigasi Kembali, dan pilihan beberapa dokumen â€” revisi 10

Menghapus layar perantara Hasil scan/Buka. Draf dicatat sebelum Rapikan otomatis dibuka, sehingga Kembali tidak membuang foto baru. Indikasi tersimpan muncul singkat setelah penyimpanan berhasil. Halaman PDF dirender saat dibutuhkan dan dapat digeser/di-zoom tanpa konflik gestur. Pembaca memiliki Kirim serta menu simpan salinan, ganti nama, dan hapus. Galeri menyimpan posisi gulir/pencarian dan menyediakan mode pilihan dengan centang serta jumlah pilihan.

Pengiriman menggunakan `ACTION_SEND` untuk satu PDF dan `ACTION_SEND_MULTIPLE` untuk beberapa PDF, dengan seluruh URI di EXTRA_STREAM/ClipData dan izin baca sementara. Setiap salinan privat menyimpan nama tampilan aslinya. Tidak ada penerima yang dipilih aplikasi. Keberhasilan membuka pilihan berbagi tidak dianggap pengiriman berhasil. Dukungan beberapa lampiran bergantung aplikasi tujuan. Rujukan: [Android Sharesheet](https://developer.android.com/develop/ui/compose/sharing/send).

Ganti nama memperbarui metadata atomik tanpa mengubah byte PDF. Hapus beberapa dokumen memvalidasi seluruh ID lebih dahulu, memindahkan file ke area penampungan privat, lalu menandai penyelesaian. Kegagalan sebelum selesai dipulihkan; proses aplikasi yang terhenti sebelum penanda selesai dipulihkan ketika pustaka/draf dibuka lagi. Penghapusan yang sudah selesai tidak dibatalkan otomatis. Cache pratinjau terkait ikut dibersihkan. Kamera, detektor, peningkatan gambar, kompresi PDF, ikon, permission, dan dependency lock tetap identik.

Verifikasi komputer: 59 test Flutter dan 16 pemeriksaan Java pengelolaan dokumen PASS; analisis Dart tanpa masalah; build release ARM64 offline dan 21 pemeriksaan APK PASS. Tes mencakup pilihan multi-PDF/ID tepat, batal hapus, pemulihan kegagalan dan proses terhenti, rename, posisi gulir, pencarian, layar sempit dengan teks 2Ã—, swipe/pinch, asal Kembali, draf, perubahan Rapikan, serta Kembali saat simpan berhasil/gagal. Gambar `evidence/r10-gallery-desktop.png` dan `evidence/r10-selection-desktop.png` memakai data sintetis. Pengujian UI/perilaku kamera dan pengiriman nyata di HP diserahkan kepada pengguna.

## Package baru dan pembuka PDF â€” revisi 09

Buka PDF dari aplikasi File/Unduhan, pilih **OTO Scan**, lalu **Selalu** jika ditawarkan oleh Android. Jika pembaca lain sudah menjadi default, hapus preferensi default pembaca tersebut di Pengaturan aplikasi lalu buka PDF lagi. Pilihan default merupakan keputusan pengguna pada dialog sistem, bukan sesuatu yang ditetapkan aplikasi sendiri. Antarmuka pengaturan berbeda antar-HP.

Handler `ACTION_VIEW` hanya menerima MIME `application/pdf` dengan URI `content`/`file`. Pengirim harus menyediakan izin baca; tidak ada izin penyimpanan luas atau jaringan baru. Deep-link bawaan Flutter dinonaktifkan agar tidak mencoba menjadikan URI PDF sebagai route. Pembukaan awal langsung ke pembaca tanpa izin kamera/pemulihan draft; intent berikutnya ditangani melalui `onNewIntent`, termasuk pergantian dokumen saat pembacaan masih berlangsung. Pengguna dapat berpindah halaman dan memperbesar hingga 6Ã—. Pembaca membuat salinan sementara di cache privat, tidak mengubah sumber atau otomatis memasukkannya ke galeri. URI file lama hanya diizinkan di shared storage dan tetap tunduk pada izin OS; content URI yang diberi izin adalah jalur yang dianjurkan.

Batas awal pembaca: 100 MiB, 10.000 halaman, raster maksimum 8 MP/3800 piksel dan dua cache halaman per sesi. PDF berkata sandi/terenkripsi tidak dibuka dan menampilkan pesan. Sesi Activity dipisahkan dan dibersihkan saat ditutup; cache sesi yang tertinggal dibersihkan setelah 24 jam pada pembukaan aplikasi berikutnya. Tidak ada eksekusi JavaScript, URL unduhan, atau modifikasi sumber PDF. Parser menggunakan PdfRenderer platform di worker aplikasi; isolasi parser dalam proses terpisah dan pengujian PDF berbahaya/kompatibilitas perangkat masih termasuk pekerjaan hardening sebelum rilis publik.

Verifikasi komputer: 45 test Flutter, 12 pemeriksaan stream PDF Java, analisis Dart, build offline ARM64, dan 21 pemeriksaan APK semuanya PASS. Manifest hasil kompilasi diverifikasi untuk MIME/scheme, authority baru, label, dan pengaturan deep-link. Tidak ada pengujian runtime pembukaan PDF atau perubahan default di HP. Kode scanner, kualitas, PDF hasil scan, galeri, ikon, dan dependency lock tetap identik dengan r08; perubahan reader hanya pada alur PDF masuk. Skrip handoff historis r01/r03 mempertahankan ID paket lamanya; tool QA aktif menargetkan ID baru tetapi tidak dijalankan di HP.

Referensi: [Android intents](https://developer.android.com/guide/components/intents-filters), [PdfRenderer](https://developer.android.com/reference/android/graphics/pdf/PdfRenderer), [Flutter deep links](https://docs.flutter.dev/cookbook/navigation/set-up-app-links), [pilihan default Android](https://support.google.com/pixelphone/answer/6271667).

## Galeri dokumen dan pencarian â€” revisi 08

Grid responsif menampilkan dua kolom pada ukuran HP umum, satu kolom pada layar sempit, dan lebih banyak kolom pada layar lebar. Thumbnail dibuat saat dibutuhkan menggunakan halaman pertama PDF, maksimum sisi panjang 600 piksel, dengan cache privat terpisah dari preview detail. Metadata dibaca tanpa membuka setiap PDF ketika informasi tersimpan tersedia; membuka dokumen tetap memvalidasi PDF. Thumbnail yang gagal memiliki tombol muat ulang. Pencarian lokal berdasarkan nama, bukan OCR isi halaman.

Verifikasi komputer: 40 test Flutter PASS, analisis Dart tanpa masalah, build release ARM64 offline PASS, serta 16 pemeriksaan APK/izin/ikon/tanda tangan PASS. Pemeriksaan visual menggunakan dokumen sintetis di `evidence/r08-gallery-desktop.png`, bukan dokumen atau screenshot HP. Tes mencakup filter kata/huruf besar/spasi, dokumen di luar 100 pertama, thumbnail sesuai kebutuhan dan retry, alur Buka/Kirim dengan mock, kembali ke pencarian, layar sempit dan teks 2Ã—. Tata letak tombol halaman hasil diperbaiki setelah tes menemukan overflow. Kode kamera, detektor, kualitas gambar, dan kompresi PDF tetap identik dengan revisi 07. Uji perilaku Android dan performa koleksi nyata diserahkan kepada pengguna.

## Scanner mandiri dan pilihan objek â€” revisi 07

Pengguna meminta scanner sendiri dengan target kualitas lebih baik dari Google ML Kit. Revisi ini memperbaiki OTO tanpa ML Kit, dependency, model atau dataset baru. **Keunggulan terhadap ML Kit belum dibuktikan**; pembanding yang benar-benar dijalankan adalah versi OTO sebelumnya pada input sintetis identik.

Pemilihan kandidat memakai titik ketukan dan kedekatan geometri dengan dokumen sebelumnya. Hasil foto menggunakan pilihan yang sama setelah koordinat dipetakan dari preview yang dipotong ke gambar penuh, lalu tepi dideteksi ulang. Fallback Hough memeriksa konsistensi arah perubahan warna di empat sisi agar tidak menggabungkan batas dua kertas. Garis memakai filter adaptif berbasis kecepatan, pencocokan urutan sudut dan konfirmasi dua pengamatan untuk perpindahan besar. Frame kamera yang sama tidak dihitung berulang sebagai bukti baru. Fokus sentuh memakai Camera2 AF/AE metering dengan pemeriksaan dukungan hardware.

Rujukan: [1â‚¬ Filter, Casiez/Roussel/Vogel](https://gery.casiez.net/1euro/), [Hough line transform](https://docs.opencv.org/4.x/d9/db0/tutorial_hough_lines.html), [Camera2 AF regions](https://developer.android.com/reference/android/hardware/camera2/CaptureRequest#CONTROL_AF_REGIONS). Implementasi ditulis sendiri; tidak menyalin SDK atau memasukkan OpenCV. `evidence/r07-research.json` mencatat rujukan, batasan dan protokol pembanding nyata yang masih pending.

Verifikasi komputer: 59 pemeriksaan Java (20 detector, 7 sensitivitas, 13 aliran YUV, 19 pemilihan/geometri), 34 test Flutter, analisis Dart, build release offline dan 16 pemeriksaan APK PASS. Pada 24 scene dua kertas dengan variasi kontras, pilihan kertas kecil benar 24/24; r06 yang tidak memiliki pilihan objek mengambil kertas besar pada 24/24 scene. Ini mengukur kemampuan memilih objek, bukan akurasi umum semua dokumen. Galat sudut maksimum pada scene tersebut 0,0131 dalam koordinat ternormalisasi. Pada 160 frame koordinat dengan seed tetap, RMS jitter turun 22,06%; sepuluh gangguan satu frame tidak menyebabkan perpindahan objek (r06: 10). Gerak lambat masih mengikuti dengan lag maksimum 0,0070 dalam koordinat ternormalisasi. Seluruh angka tersebut berasal dari komputer; bukan kamera nyata atau ML Kit.

APK 16.557.560 byte, SHA256 `c1a46c58058a1f4860e858fcbb547846f2024f83e9a9b306514f949a5dcedb0e`. PDF ringkas, peningkatan ketajaman, ikon, izin dan dependency lock dipertahankan. Kertas melengkung, oklusi berat, blur dan tepi yang menyatu dengan latar belum diselesaikan. Sensor fokus, performa HP, hasil crop aktual dan pembanding ML Kit tetap belum diuji. Gambar usulan galeri belum diimplementasikan pada revisi scanner ini.

## Garis sudut dan crop otomatis â€” revisi 06

Menangani laporan pengguna bahwa garis sudut serta pemotongan foto sama-sama tidak berfungsi. Deteksi langsung sekarang menerima frame YUV Camera2 dengan konversi stride/rotasi/proporsi sesuai tampilan, tanpa bergantung pada salinan bitmap TextureView. Kamera dibuat ulang saat kembali dari latar atau saat mencoba ulang. Status membedakan pencarian tepi yang berjalan dengan aliran gambar yang terhenti; status terhenti dapat diketuk untuk memulai ulang kamera.

Detektor foto dan preview memakai pencarian garis tepi sebagai fallback jika segmentasi warna gagal, termasuk saat bayangan membuat kecerahan kertas dan latar tumpang tindih. Kandidat tetap harus memiliki empat sisi yang didukung kontras nyata; garis tabel, noise dan tepi yang terpotong bingkai ditolak. Tidak ada bingkai palsu saat deteksi gagal.

Verifikasi komputer: 20 regresi detector, 7 sensitivitas, 13 konversi/aliran gambar dan 26 test Flutter PASS; analisis Dart tanpa masalah; build ARM64 offline dan 16 pemeriksaan APK PASS. Fixture pencahayaan campuran gagal pada detektor lama untuk preview maupun foto, dan terdeteksi pada keduanya setelah perubahan. Median fallback desktop 12,759 ms/P95 17,406 ms adalah pengukuran sintetis komputer, bukan performa HP. Jalur kualitas gambar, PDF ringkas, ikon, izin, dan dependency lock tetap identik dengan revisi 05.

Instalasi 0.1.5+6 berhasil; 16.557.560 byte, SHA256 `1edb774e4576b50fd731f4933193ee929b049b45eedefec70fb09299832f01ce`. Tidak ada pembukaan aplikasi, penggunaan kamera, atau pembacaan dokumen HP. Penyebab runtime pada HP dan keberhasilan crop fisik belum diverifikasi. Bukti: `evidence/r06-reproduction.log`, `evidence/r06-preservation.json`, `evidence/r06-*.log`, `evidence/install-handoff-r06.json`.

## PDF lebih kecil â€” revisi 05

PDF baru memakai kompresi JPEG kualitas 90 dengan sisi panjang maksimum 3508 piksel (sekitar 300 dpi untuk A4), mempertahankan proporsi dan warna. Halaman hitam-putih murni memakai kompresi lossless 1-bit pada resolusi penuh; teks abu-abu atau stempel berwarna tidak dibinarisasi otomatis. Crop PNG dan foto sumber tetap untuk pengeditan. PDF lama tidak ditulis ulang.

Penulis PDF first-party menanam byte gambar terkompresi secara langsung, satu halaman per giliran, dengan overhead kecil; semua halaman dibuka/render untuk validasi sebelum file diterbitkan ke Dokumen Saya. Kegagalan penulisan/validasi membersihkan staging dan tidak mengganti dokumen lama.

Verifikasi komputer: 19 test JVM PDF PASS, 94 pemeriksaan independen pypdf/PDFium PASS, serta inspeksi visual render Poppler, detail teks 6â€“12 pt dan stempel warna. Pada fixture A4 sintetis 2480Ã—3508: PDF referensi RGB lossless 6,456,321 byte; PDF berwarna 1,301,070 byte (**79,85% lebih kecil**); PDF hitam-putih 22,768 byte. Retensi piksel tinta gelap pada area teks kecil 100%; galat RGB rata-rata 1,51/255. Ini memakai encoder Java ImageIO di komputer, bukan pengukuran kamera/encoder Android atau jaminan ukuran setiap dokumen.

Build offline dan 16 pemeriksaan APK PASS. Instalasi 0.1.4+5 berhasil dan hash cocok. Kamera, detector, kualitas crop, UI Flutter, ikon, izin, dan dependency lock identik dengan revisi 04. Tidak ada tes HP, pembukaan aplikasi, atau pembacaan dokumen pengguna. Ukuran akhir bergantung pada isi, warna, tekstur, serta noise gambar. Detail/fokus yang tidak tertangkap kamera tetap tidak bisa dikembalikan oleh kompresi.

Bukti: `evidence/r05-pdf/verification.json`, `evidence/r05-native-tests.log`, `evidence/r05-preservation.json`, `evidence/r05-installation.json`.

## Verifikasi dan batas â€” revisi 04

- Flutter analyze tanpa masalah; 18 test Dart/widget PASS, termasuk toleransi deteksi terputus, pergerakan garis, draft, permintaan render PDF detail, serta alur Simpan/Kirim dengan native mock di komputer.
- JVM: 20 regresi detector + 7 sensitivitas + 14 kualitas piksel PASS. Kertas lebih kecil dan satu tepi lemah diterima; gambar kosong, noise, sudut silang dan kertas terpotong bingkai tetap ditolak.
- Build release ARM64 offline berhasil; 16/16 pemeriksaan APK, izin, ikon OTO, alignment dan tanda tangan PASS. Mode QA dimatikan.
- Deteksi preview diperbarui tiap 250 ms, dengan satu proses aktif dan toleransi hilang 600 ms. Segmentasi tetap memeriksa keempat tepi; kandidat lemah tidak dipaksa menjadi crop. Foto penuh memakai refinement tepi pada resolusi sumber.
- Foto sampai 12 MP sesuai kemampuan kamera dan batas memori (8/6 MP pada heap lebih kecil), JPEG sumber kualitas 98, hasil crop PNG, peningkatan kontras/ketajaman per baris, thumbnail kecil terpisah. Pembaca PDF membuat raster detail sampai 8 MP; foto sumber tetap menjadi dasar setiap pengeditan sehingga filter tidak menumpuk.
- Benchmark JVM desktop, 40 sampel setelah 12 warmup: median preview kuat 22.121 â†’ 4.705 ms; kontras lemah 14.737 â†’ 5.114 ms. P95 masing-masing 31.226 â†’ 8.029 ms dan 18.523 â†’ 18.964 ms. Perbandingan menggunakan raster preview lama 300Ã—400 vs baru 240Ã—320. Ini bukan pengukuran performa HP.
- Tidak ada pengoperasian kamera, UI, instrumentation, pengambilan screenshot, pembacaan dokumen, atau uji Simpan/Kirim di HP untuk revisi 04. Pengguna melakukan pengujian sendiri. Verifikasi pemasangan hanya membaca identitas paket dan hash APK.

Resolusi dan PNG dapat menambah ukuran hasil serta waktu pemrosesan setelah memotret. Ketajaman masih bergantung pada fokus, cahaya, gerakan, dan piksel yang benar-benar tertangkap; peningkatan ini tidak merekonstruksi tulisan yang hilang. Tepi yang menyatu dengan latar atau keluar bingkai masih bisa memerlukan Rapikan. Tidak ada OCR, model baru, atau upload dokumen. Draft/history memakai JSON atomik dan filesystem privat.

Riwayat: revisi 03 diuji kamera â†’ crop â†’ Simpan â†’ Buka â†’ chooser, dengan 24/24 native checks pada APK lama. Lihat `evidence/camera-handoff-r03.json` dan `../../docs/android-scan/FLUTTER_TASK.md`. Hasil historis tersebut tidak dianggap sebagai kelulusan runtime revisi 04. Gate phase/UAT/Production tetap tidak berubah.

## Build lokal

Toolchain kandidat Play: Flutter 3.29.3 / Dart 3.7.2, Android SDK 36, AGP 8.10.1, Gradle 8.11.1, NDK 27.0.12077973. Komponen build berasal dari instalasi/cache resmi yang sudah tersedia. Tidak ada plugin kamera/PDF eksternal atau model baru. Verifikasi lokal tidak menggantikan review Google Play, pengujian pengguna, atau sign-off Production.

```powershell
flutter pub get --offline
flutter analyze --no-pub
flutter test --no-pub
$env:OTO_SCAN_QA='false'
flutter build apk --release --no-pub --target-platform android-arm64
```

Build unsigned tidak mengakses signing key. `tool/Sign-TestApk.ps1` menggunakan key uji existing dengan authority yang sudah diverifikasi; secret tidak disalin ke proyek/paket/log. `tool/verify_apk.py` memeriksa artifact final. Mode fixture khusus QA memerlukan **keduanya** `OTO_SCAN_QA=true` dan `--dart-define=OTO_TEST_MODE=true`; keduanya dimatikan pada kandidat pengguna.

Implementasi: `lib/`; adapter dan detector: `android/app/src/main/java/id/ayani/oto/oto_scan/`; test native: `android/app/src/androidTest/`. Dokumen scope/admission: `../../docs/android-scan/FLUTTER_TASK.md`. Tidak ada commit/merge, publikasi, perubahan server, penutupan gate phase, atau sign-off Production.
