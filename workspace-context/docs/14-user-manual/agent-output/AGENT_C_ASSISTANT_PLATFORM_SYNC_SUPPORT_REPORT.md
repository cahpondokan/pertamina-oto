# AGENT C — ASSISTANT, PLATFORM, OFFLINE, SYNC & SUPPORT REPORT

**Task ID:** DOC14-AGENT-C  
**Status:** RECOMMENDATION — keputusan final tetap milik User dan Integrator Document 14  
**Tanggal analisis:** 2026-09-01  
**Ruang lingkup:** panduan pengguna; tanpa Admin Manual, coding, atau perubahan master Document 14

## Baseline dan catatan sumber

Laporan ini diturunkan dari Document 01–12 FINAL. Pada saat analisis, **Document 13 FINAL belum tersedia di workspace**; empat laporan Agent Document 13 digunakan hanya sebagai konteks UAT non-authoritative. Isi laporan ini wajib diperiksa ulang terhadap Document 13 FINAL sebelum diintegrasikan.

Panduan ini memakai bahasa pengguna biasa. Detail teknis internal, secret, token, konfigurasi keamanan, dan cara kerja pertahanan sistem tidak dijelaskan. Nama, posisi, dan bentuk control UI yang belum dikunci ditandai **PERLU VALIDASI UI**.

# 1. Assistant user guide

PERTAMINA OTO Assistant adalah cara tambahan untuk menemukan dan menjalankan tool melalui percakapan. Anda tetap dapat memakai File-First atau Tool-First tanpa Assistant.

## 1.1 Membuka Assistant

1. Buka PERTAMINA OTO.
2. Pilih **Assistant** pada navigasi yang tersedia.
3. Jika tombol Assistant tidak terlihat, capability tersebut mungkin belum tersedia pada platform, versi aplikasi, permission, atau kondisi koneksi Anda.

> **Lokasi final menu/tombol Assistant: PERLU VALIDASI UI.** Manual final harus menggunakan label dan gambar dari release candidate yang benar.

## 1.2 Menulis permintaan

Tulis hasil yang ingin Anda capai, file yang digunakan, urutan pekerjaan, dan pilihan penting bila sudah diketahui.

Gunakan pola sederhana:

```text
Lakukan [pekerjaan] pada [file],
dengan [pilihan penting],
lalu simpan sebagai [hasil].
```

Contoh:

```text
Gabungkan tiga PDF,
tambahkan watermark,
lalu kompres hasilnya.
```

Jika file atau pilihan belum jelas, Assistant akan meminta klarifikasi. Pilih file yang benar dan jangan meminta Assistant menebak target untuk tindakan yang tidak dapat dibatalkan.

## 1.3 Memeriksa Plan Preview

Untuk pekerjaan beberapa langkah, Assistant menampilkan **Plan Preview**. Sebelum menjalankan:

1. periksa file input dan urutannya;
2. periksa langkah yang akan dilakukan;
3. periksa parameter penting, misalnya halaman, watermark, kualitas, atau bahasa;
4. periksa nama/jenis hasil;
5. perhatikan apakah ada langkah yang memerlukan koneksi atau pengiriman isi dokumen;
6. koreksi bagian yang tidak sesuai.

Plan Preview adalah rencana, bukan bukti bahwa proses sudah selesai.

## 1.4 Mengubah instruksi

Anda dapat menulis koreksi sebelum menjalankan, misalnya:

- “Ubah urutannya menjadi file B, A, lalu C.”
- “Watermark hanya pada halaman 2 sampai 10.”
- “Jangan kompres terlalu kuat.”
- “Simpan sebagai file baru, jangan timpa file asli.”

Assistant dapat memperbarui rencana. Periksa kembali rencana yang berubah.

## 1.5 Menjalankan rencana

1. Pilih tindakan **Jalankan** atau label setara pada Plan Preview.
2. Jika muncul confirmation, baca target dan dampaknya.
3. Lanjutkan hanya jika file, tujuan, dan tindakan sudah benar.

Overwrite, permanent delete, share, pengiriman isi ke server, dan tindakan berisiko lainnya tetap meminta confirmation sesuai aturan aplikasi. Percakapan biasa tidak dianggap sebagai confirmation.

> **Label final tombol Jalankan/Edit/Batal: PERLU VALIDASI UI.**

## 1.6 Memantau, membatalkan, dan melihat hasil

- Progress tampil pada proses aktif dan/atau **Activity Center**.
- Untuk proses panjang, Anda dapat melihat tahap, persentase atau indikator aktivitas, jumlah item, dan perkiraan waktu bila tersedia.
- Pilih **Batalkan** hanya bila opsi tersebut tersedia. Beberapa tahap aman untuk dibatalkan; tahap lain mungkin harus diselesaikan lebih dahulu.
- Hasil belum dianggap final sebelum proses dan pemeriksaan hasil selesai.
- Setelah selesai, buka hasil dari percakapan, notifikasi, atau Activity Center sesuai UI release.
- Selalu periksa hasil sebelum menyimpan, membagikan, atau memakai hasil untuk keputusan penting.

## 1.7 Melaporkan masalah dari Assistant

Jika rencana, progress, atau hasil tidak sesuai:

1. buka **Bantuan** atau tindakan kontekstual yang tersedia;
2. pilih **Laporkan Masalah**;
3. tulis masalah secara singkat;
4. sertakan screenshot hanya jika diperlukan dan setelah preview;
5. kirim laporan;
6. lihat status laporan Anda.

# 2. Assistant examples

Contoh berikut harus disesuaikan dengan Feature yang benar-benar tersedia pada platform dan release pengguna.

| Tujuan | Contoh permintaan | Yang perlu diperiksa |
|---|---|---|
| Gabung dan rapikan PDF | “Gabungkan file A, B, dan C sesuai urutan ini, tambahkan watermark DRAFT, lalu kompres hasilnya.” | Urutan file, teks/posisi watermark, tingkat kompresi, nama hasil |
| Hapus halaman kosong | “Cari halaman kosong pada PDF ini dan tampilkan rencana penghapusannya.” | Halaman yang ditandai; jangan finalisasi sebelum review |
| Tambah nomor halaman | “Tambahkan nomor halaman di kanan bawah mulai dari halaman 3.” | Halaman awal, posisi, format nomor |
| Buat ringkasan | “Ringkas halaman 1–10 dari laporan ini.” | Halaman yang dipilih, informasi penting, keterbatasan hasil AI |
| Terjemahkan | “Terjemahkan PDF ini ke Bahasa Indonesia dan tampilkan preview sebelum ekspor.” | Bahasa sumber/target, bagian yang diproses, layout hasil |
| PDF ke Markdown | “Ubah PDF ini menjadi Markdown dan pertahankan heading serta daftar sejauh dikenali.” | Struktur heading/list/table dan bagian yang memerlukan OCR |
| Bantuan tool | “Tool apa yang sesuai untuk mengecilkan ukuran PDF tanpa mengubah file asli?” | Tool yang tersedia dan dampak pengaturan |
| Workflow saat offline | “Gabungkan dan beri nomor halaman pada file ini tanpa mengirimnya ke server.” | Status local capability; hentikan jika aplikasi menyatakan koneksi diperlukan |

Permintaan yang lebih baik menyebutkan target dengan jelas. Untuk tindakan berisiko, gunakan kalimat yang tetap memberi ruang untuk review, misalnya “tampilkan rencananya terlebih dahulu”.

# 3. Assistant limitations

Pengguna perlu memahami batas berikut:

- Assistant hanya memakai tool resmi PERTAMINA OTO.
- Hasil mengikuti capability aplikasi, platform, perangkat, permission, policy, koneksi, dan Feature yang tersedia.
- Assistant tidak memiliki permission tambahan. Jika Anda tidak boleh memakai suatu tool atau file, Assistant juga tidak boleh membukanya.
- Anda tetap harus memeriksa Plan Preview, target file, parameter, dan hasil.
- Tindakan berisiko meminta confirmation yang sama seperti penggunaan tool biasa.
- AI dapat salah, kurang lengkap, atau keliru memahami konteks. Periksa angka, nama, tanggal, istilah, terjemahan, dan kesimpulan penting.
- **PERTAMINA OTO menganggap isi dokumen sebagai data, bukan perintah kepada Assistant.**
- Isi dokumen tidak otomatis dikirim untuk pemrosesan AI.
- Public AI endpoint tidak digunakan. Fitur AI hanya memakai capability lokal atau layanan internal yang telah disetujui.
- Ketidaktersediaan layanan AI tidak menonaktifkan tool lokal yang sebenarnya masih dapat digunakan.
- Assistant tidak boleh menyatakan berhasil sebelum hasil aktual tersedia dan tervalidasi.
- Percakapan dapat dibersihkan sesuai control yang tersedia. Jangan menaruh password, token, secret, atau data yang tidak diperlukan di percakapan.

# 4. Desktop guide

Windows Desktop adalah pilihan utama untuk pekerjaan dokumen lengkap dan kompleks.

Desktop paling cocok untuk:

- menggunakan rangkaian fitur paling lengkap;
- memproses banyak file atau file besar;
- memilih file/folder dengan drag-and-drop;
- bekerja dengan keyboard, mouse, dan shortcut;
- memakai preview dan workspace yang lebih kompleks;
- menjalankan batch dan workflow beberapa langkah;
- melanjutkan pekerjaan lokal ketika capability dan session mendukung.

## Cara kerja yang disarankan

1. Buka file melalui File-First, pilih tool melalui Tool-First, atau jelaskan hasil kepada Assistant.
2. Gunakan drag-and-drop untuk file/folder bila tersedia.
3. Periksa urutan file dan preview sebelum proses.
4. Untuk pekerjaan panjang, Anda boleh berpindah layar dan memantaunya melalui Activity Center.
5. Simpan hasil sebagai file baru bila tidak yakin. Overwrite selalu memerlukan perhatian khusus.
6. Gunakan **Amankan ke Akun** hanya untuk hasil yang memang ingin tersedia lintas perangkat.

PERTAMINA OTO dapat menjalankan pekerjaan berat di latar belakang pada perangkat agar aplikasi tetap dapat digunakan. Manual pengguna tidak perlu menampilkan istilah atau komponen teknis internal.

# 5. Web guide

Web cocok ketika Anda membutuhkan akses melalui browser untuk:

- account dan preferensi yang tersedia;
- History yang tersedia pada akun;
- daftar dan akses **Secured Files**;
- browser-safe tools yang dirilis;
- upload/proses server yang diizinkan dan telah diberi pemberitahuan;
- pelaporan masalah dan status laporan;
- halaman administrasi, Audit, atau Support **hanya jika permission Anda mengizinkan**.

Tidak seluruh fitur Desktop tersedia di Web. Jika suatu tool tidak terlihat atau dinyatakan tidak tersedia, gunakan alternatif yang dijelaskan aplikasi atau Desktop bila sesuai. Halaman Web yang pernah terbuka tidak memberikan kewenangan bisnis saat offline.

Saat mengunggah melalui Web:

1. periksa file yang dipilih;
2. baca tujuan pemrosesan;
3. periksa pemberitahuan dan confirmation;
4. pantau progress;
5. buka hasil hanya setelah status selesai.

# 6. Phone guide

Phone menyediakan fitur terkurasi yang cepat dan praktis. Fitur yang umum dan sesuai capability dapat mencakup:

- scan dokumen dengan kamera;
- membaca PDF;
- merge/compress sederhana;
- crop dan rotate;
- OCR ringan;
- menggambar tanda tangan/paraf visual;
- membagikan hasil;
- memilih **Amankan ke Akun**;
- membuka file yang telah diamankan;
- melihat Activity/status;
- melaporkan masalah.

## Tips penggunaan

- Gunakan portrait atau landscape sesuai dokumen.
- Periksa preview setelah scan, crop, rotate, OCR, atau signature.
- Saat menggunakan kamera/share/file picker, pilih hanya file yang diperlukan.
- Jangan mengharapkan batch, file sangat besar, multi-panel, atau semua pilihan Desktop tersedia.
- Jika aplikasi masuk background atau koneksi berubah, buka Activity Center untuk memeriksa status aktual sebelum mengulang tindakan.

# 7. Tablet guide

Tablet dapat menyediakan fitur lebih lengkap daripada Phone, tetapi tetap merupakan pengalaman terkurasi.

Tablet cocok untuk:

- membaca dan meninjau PDF pada layar lebih besar;
- scan dokumen;
- annotation ringan;
- menggambar atau menempatkan visual signature dengan touch/stylus;
- crop, rotate, reorder, dan editing terkurasi;
- beberapa workflow yang lebih nyaman daripada di Phone;
- membuka Secured Files dan Activity Center.

Gunakan stylus bila tersedia untuk annotation dan signature yang lebih presisi. Periksa tata letak setelah mengganti portrait/landscape. Fitur tambahan hanya boleh muncul jika memang didukung oleh perangkat, versi aplikasi, permission, dan policy.

# 8. Offline guide

Offline mode membantu Anda tetap bekerja pada capability lokal yang didukung.

## Apa yang dapat terjadi saat offline

- Fitur lokal tetap bekerja bila session sebelumnya, perangkat, permission offline, policy, dan capability mendukung.
- File tidak otomatis dikirim ke server.
- Draft dan pekerjaan lokal tetap berada pada perangkat sesuai kebijakan.
- Feature yang memerlukan server menunjukkan status yang jelas.
- Pending action dapat dilanjutkan setelah koneksi kembali bila Feature mendukung dan pemeriksaan akses saat itu masih lulus.

## Arti status

| Status | Arti dan tindakan pengguna |
|---|---|
| **Memerlukan koneksi** | Tindakan memerlukan layanan online. Sambungkan perangkat, lalu coba atau lanjutkan sesuai petunjuk. |
| **Menunggu koneksi** | Pekerjaan/intent sedang menunggu koneksi. Jangan membuat ulang tindakan sebelum memeriksa Activity Center. |
| **Tidak tersedia saat offline** | Capability tersebut tidak dapat digunakan tanpa koneksi. Gunakan fitur lokal lain atau coba kembali setelah online. |

## Setelah koneksi kembali

1. Tunggu aplikasi memeriksa kembali session, permission, policy, capability, dan versi.
2. Buka Activity Center.
3. Periksa tindakan yang menunggu.
4. Lanjutkan atau coba lagi hanya bila tersedia.
5. Jika permission berubah, tindakan lama dapat ditolak dan harus ditinjau ulang.

Jangan menganggap semua tindakan yang tertunda akan berjalan otomatis. File tidak boleh diunggah tanpa tindakan dan izin yang berlaku.

# 9. Local-First explanation

> **Jika aman dan perangkat mampu, proses dilakukan di perangkat Anda.**

Local-First berarti aplikasi memprioritaskan pemrosesan pada perangkat bila platform, kapasitas, keamanan, policy, dan tool mendukung. Anda tidak perlu memilih metode teknis.

Jika server diperlukan:

- aplikasi memberi informasi bahwa koneksi atau pemrosesan online diperlukan;
- isi dokumen tidak dikirim tanpa pemberitahuan;
- upload tetap mengikuti permission, policy, klasifikasi, dan confirmation/consent yang berlaku;
- progress ditampilkan;
- hasil hanya dinyatakan selesai setelah benar-benar tersedia;
- file dan hasil server ditangani sesuai retention policy.

Local-First tidak berarti seluruh fitur selalu dapat berjalan offline. Sebaliknya, online juga tidak berarti file lokal otomatis diunggah.

# 10. Activity/reconnect guidance

Activity Center menyatukan status tool biasa, Assistant, pekerjaan lokal, pekerjaan server, sync, dan recovery.

> **Lokasi final tombol/menu Activity Center: PERLU VALIDASI UI.**

## Informasi yang dapat ditampilkan

- nama pekerjaan atau hasil yang aman;
- status berjalan, menunggu, mencoba lagi, selesai, gagal, atau dibatalkan;
- persentase bila dapat dihitung;
- indikator aktivitas bila total belum diketahui;
- tahap saat ini;
- jumlah item selesai dan tersisa;
- perkiraan waktu bila cukup akurat;
- tindakan Batalkan, Coba Lagi, Lanjutkan, atau Buka Hasil bila tersedia dan aman.

## Jika koneksi notifikasi terputus

Pekerjaan server dapat tetap berjalan walaupun pembaruan realtime terputus.

1. Jangan langsung menjalankan pekerjaan yang sama lagi.
2. Periksa indikator koneksi dan Activity Center.
3. Tunggu aplikasi menyambung kembali.
4. Refresh status bila control tersebut tersedia.
5. Setelah tersambung, gunakan status terbaru yang ditampilkan aplikasi.

Jika status tetap tidak jelas, gunakan **Laporkan Masalah** dan sertakan Job ID/Correlation ID hanya jika ditampilkan aplikasi. Jangan menyalin detail teknis lain yang tidak diminta.

# 11. Amankan ke Akun

**Amankan ke Akun** menyimpan hasil tertentu yang Anda pilih agar dapat diakses melalui akun dan perangkat lain yang didukung. Tindakan ini eksplisit; bukan autosync seluruh workspace.

## Langkah penggunaan

1. Selesaikan proses.
2. Pilih hasil yang benar.
3. Pilih **Amankan ke Akun**.
4. Periksa koneksi serta pemberitahuan yang tampil.
5. Tunggu progress sampai status berhasil.
6. Buka **Secured Files**.
7. Login dengan akun Pertamina yang sama untuk mengaksesnya dari perangkat lain.

## Yang perlu diketahui

- Hanya hasil yang Anda pilih yang diamankan.
- File terikat pada pemilik dan permission.
- Maksimum tiga versi terakhir disimpan.
- Saat versi keempat dibuat, versi paling lama ditangani sesuai retention policy.
- Draft, working state, undo/redo, cache, thumbnail, temporary file, checkpoint, clipboard, password, dan source yang tidak dipilih tidak ikut.
- Full Cloud Workspace dan autosync seluruh draft bukan baseline.
- Jangan menutup aplikasi atau mengulang tindakan sebelum memeriksa status jika koneksi terputus.

# 12. Secured Files/cross-device

## Membuka file dari perangkat lain

1. Login menggunakan akun Pertamina yang sama.
2. Buka **Secured Files**.
3. Pilih file dan version yang tersedia.
4. Pilih buka, download, atau restore hanya jika control tersebut tersedia dan Anda memiliki permission.
5. Periksa hasil setelah file tersedia pada perangkat.

File dari akun yang dibuka untuk diedit menjadi working copy lokal. Perubahan pada working copy tidak boleh menimpa version akun secara diam-diam.

## Version dan conflict

Jika file akun berubah, aplikasi menampilkan status/version sesuai UI release. Jangan menimpa atau membuat version baru sampai Anda memahami status tersebut.

> **Placeholder manual:** `[Status version/conflict dan pilihan tindak lanjut — PERLU VALIDASI UI]`

Manual final tidak boleh menentukan nama tombol atau hasil conflict sebelum UI release dikunci. Acceptance minimum: conflict terlihat, silent overwrite tidak terjadi, dan pengguna dapat mengetahui version yang sedang dipakai.

## Delete dan restore

- Menghapus file lokal berbeda dari menghapus file pada akun.
- File akun mengikuti Account Recycle, retention, dan legal-hold policy yang berlaku.
- Permanent delete memerlukan confirmation dan dapat meminta login ulang.
- File yang tidak lagi diizinkan tidak boleh dibuka hanya karena pernah tersedia pada perangkat lain.

# 13. Visual signature guidance

Visual Signature memungkinkan Anda menempatkan tanda tangan/paraf/stamp visual pada dokumen.

## Membuat atau memilih signature

- Phone/Tablet: gambar dengan jari atau stylus bila didukung.
- Desktop: upload/paste gambar transparan, pilih saved signature yang tersedia, gambar dengan mouse, atau gunakan stylus/pen tablet bila didukung.
- Opsi draw/upload/paste/saved signature berbeda menurut platform dan release.

## Menempatkan signature

1. Pilih sumber visual signature.
2. Periksa preview.
3. Pilih halaman.
4. Atur posisi.
5. Atur ukuran.
6. Periksa kembali dokumen.
7. Finalisasi setelah confirmation.

Gunakan Clear/Undo saat menggambar bila tersedia. Menyimpan signature untuk dipakai ulang harus menjadi pilihan pengguna dan hanya berlaku sesuai perlindungan perangkat/profile.

> **Peringatan:** Visual Signature bukan certificate-based Digital Signature. Visual Signature bukan sertifikat digital, bukan tanda tangan kriptografis, dan tidak memberikan jaminan identitas/non-repudiation seperti PKI.

Jangan membagikan atau menyinkronkan asset signature kecuali ada alur resmi yang jelas. Asset visual signature tidak ikut autosync secara default.

# 14. PDF Intelligence guidance

## 14.1 AI Summarizer

AI Summarizer membantu membuat ringkasan PDF.

1. Pilih PDF dan halaman/bagian yang diperlukan.
2. Mulai summarization secara eksplisit.
3. Jika dokumen hasil scan, aplikasi mungkin perlu OCR.
4. Jika koneksi atau layanan internal diperlukan, baca pemberitahuan yang tampil.
5. Periksa ringkasan terhadap dokumen sumber.

Ringkasan berbantuan AI bukan pengganti review manusia. Periksa nama, angka, tanggal, keputusan, syarat, dan informasi keselamatan/kepatuhan.

## 14.2 Translate PDF

1. Pilih PDF.
2. Pilih atau periksa bahasa sumber.
3. Pilih bahasa target yang didukung.
4. Jalankan proses.
5. Preview hasil sebelum final export.
6. Periksa istilah teknis, nama, angka, tabel, dan tata letak.

Layout dipertahankan sejauh capability memungkinkan. Jangan menganggap hasil selalu sama persis dengan dokumen asli.

## 14.3 PDF to Markdown

1. Pilih PDF.
2. Jalankan **PDF to Markdown**.
3. Jika PDF memiliki text layer, aplikasi memprosesnya secara langsung bila memungkinkan.
4. Dokumen scan dapat memerlukan OCR.
5. Preview heading, list, table, dan urutan teks.
6. Simpan sebagai file `.md` setelah hasil diperiksa.

PDF to Markdown tidak selalu memerlukan AI. Dokumen dengan layout kompleks, tabel rumit, atau hasil scan dapat memerlukan koreksi manual.

## 14.4 Batas umum Intelligence

- Hasil AI/translation dapat salah atau tidak lengkap.
- Beberapa fitur memerlukan koneksi atau provider internal yang disetujui.
- Public AI endpoint tidak digunakan.
- Isi dokumen tidak otomatis dikirim; ikuti pemberitahuan dan policy yang tampil.
- Dokumen sumber tetap menjadi acuan utama.
- Jangan memproses dokumen sensitif bila Anda tidak memiliki izin.

# 15. Issue Reporting guide

Gunakan **Laporkan Masalah** untuk memberi tahu ketidaksesuaian langsung dari PERTAMINA OTO.

## Langkah penggunaan

1. Pilih **Bantuan**.
2. Pilih **Laporkan Masalah**.
3. Tulis pesan singkat tentang yang terjadi dan hasil yang Anda harapkan.
4. Ambil screenshot jika perlu dan jika platform mendukung.
5. Tandai area dengan panah, lingkaran, atau highlight bila tersedia.
6. Periksa preview.
7. Pilih **Kirim**.
8. Buka laporan Anda untuk melihat status.

Anda tidak perlu mengisi kategori, prioritas, Feature ID, versi aplikasi, platform, error code, assignment, atau status internal. Sistem menambahkan safe diagnostics yang diperlukan secara otomatis.

Status sederhana untuk pengguna dapat berupa:

- **Dikirim**;
- **Sedang diperiksa**;
- **Sedang ditindaklanjuti**;
- **Selesai**.

> **Lokasi final Bantuan, status, dan halaman laporan: PERLU VALIDASI UI.**

Jangan mengirim laporan melalui channel tidak resmi. Gunakan alur dalam aplikasi atau jalur resmi yang benar-benar ditetapkan organisasi; manual ini tidak membuat alamat/kontak baru.

# 16. Privacy/security tips

- Jangan memasukkan password, token, kode rahasia, atau credential ke percakapan Assistant atau laporan masalah.
- Periksa screenshot sebelum mengirim. Tutupi atau ambil ulang bila ada informasi yang tidak diperlukan.
- Jangan membagikan dokumen sensitif tanpa permission dan tujuan yang jelas.
- Sebelum memilih **Amankan ke Akun**, pastikan hasil dan akun tujuan benar.
- Logout dari perangkat bersama dan jangan meninggalkan file terbuka.
- Periksa hasil AI, ringkasan, OCR, Markdown, dan terjemahan sebelum digunakan.
- Gunakan **Simpan sebagai File Baru** bila Anda belum yakin ingin mengganti file.
- Periksa target sebelum overwrite, permanent delete, share, upload, download, atau final export.
- Jangan menganggap file yang pernah dibuka masih boleh diakses setelah permission berubah.
- Laporkan perilaku tidak wajar, hasil yang menyatakan sukses padahal belum selesai, file yang muncul pada akun yang salah, atau upload yang tidak Anda pilih.
- Jangan menonaktifkan perlindungan perangkat atau memindahkan file kerja aplikasi secara manual untuk mengatasi error.

# 17. Screenshot requirements

Screenshot laporan masalah bersifat opsional dan hanya dikirim setelah tindakan pengguna.

## Sebelum mengirim

1. Pilih **Screenshot** hanya jika gambar membantu menjelaskan masalah.
2. Pastikan target utamanya adalah jendela PERTAMINA OTO bila platform mendukung.
3. Periksa preview setiap kali.
4. Tandai area yang bermasalah bila markup sederhana tersedia.
5. Periksa apakah terlihat nama, isi dokumen, notifikasi, email, identitas, signature, atau informasi sensitif yang tidak diperlukan.
6. Batalkan atau ambil ulang screenshot jika ragu.
7. Pilih **Kirim** hanya setelah preview sudah aman.

## Yang tidak dilakukan aplikasi

- Screenshot tidak diunggah otomatis.
- Membuka form laporan tidak otomatis mengambil screenshot.
- Safe diagnostics tidak mencakup otomatis document content, password, token, clipboard, visual-signature asset, full OCR text, atau secret.
- Screenshot yang dibatalkan tidak boleh dikirim.

> **Kemampuan screenshot dan markup per platform: PERLU VALIDASI UI/PLATFORM.**

# 18. Risks/gaps

| Risiko/gap | Dampak pada User Manual | Tindakan yang diperlukan |
|---|---|---|
| Document 13 FINAL belum tersedia | Hasil UAT final, limitation, dan wording yang diterima belum dapat dirujuk | Revalidasi laporan ini setelah Document 13 FINAL diterbitkan |
| Lokasi/nama/icon Assistant, Activity Center, Help, Secured Files, serta action controls belum dikunci di sumber | Langkah navigasi detail berisiko salah | Ganti seluruh placeholder **PERLU VALIDASI UI** memakai release candidate |
| Exact Feature × Desktop/Web/Phone/Tablet exposure belum dibekukan untuk release | Manual dapat menjanjikan capability yang tidak tersedia | Turunkan manual per platform dari release capability matrix; unavailable state harus dijelaskan |
| Feature Catalog masih memuat status perencanaan untuk beberapa capability | User Manual dapat mendahului implementasi | Masukkan hanya Feature yang benar-benar dirilis dan lolos UAT |
| UX conflict/version belum dikunci | Nama tombol/decision path dapat salah | Pertahankan placeholder; validasi dengan UI dan policy final; jangan menjanjikan merge/overwrite otomatis |
| Delete/Recycle/retention/legal-hold wording belum final per policy | Pengguna dapat salah memahami kapan data benar-benar dihapus | Review bersama product/security/legal/policy owner sebelum publikasi |
| Exact offline session lifetime dan platform support bersifat policy/configuration | Angka atau janji offline dapat salah | Jangan hardcode durasi; gunakan wording capability/session/policy-aware |
| AI provider, bahasa, ukuran, OCR, dan quality limitation dapat berubah | Intelligence guide berisiko overpromise | Tambahkan supported matrix dan known limitations dari release/UAT |
| Visual signature source/save-for-reuse berbeda per platform | Instruksi generik mungkin tidak cocok | Buat varian Desktop, Phone, Tablet berdasarkan build final; pertahankan warning NON-PKI |
| Screenshot capture/markup capability berbeda per platform | Langkah dapat tidak tersedia | Gunakan “bila tersedia”; tambahkan screenshot per platform setelah UAT |
| Issue status dan navigation placement belum divalidasi pada UI final | User tidak menemukan laporan/status | Cocokkan label final dengan Requirement dan release candidate |
| Jalur support resmi di luar aplikasi belum ditentukan | Risiko membuat kontak palsu | Jangan menambahkan nomor/email/URL sebelum ada keputusan resmi |
| UI localization Bahasa Indonesia/English belum final | Istilah manual dan layar dapat berbeda | Lakukan terminology pass menggunakan resource release final |
| Manual ini bukan Admin Manual | Pembaca dengan permission administratif mungkin mencari instruksi lain | Sediakan manual terpisah bila resmi diminta; jangan memperluas scope laporan ini |
| Privacy evidence dapat berubah setelah UAT/known issue | Guidance mungkin tidak mencerminkan limitation rilis | Sinkronkan manual dengan UAT Summary, known issues, dan security sign-off |

Tidak ada gap yang mengizinkan janji full workspace sync, AI selalu akurat, seluruh fitur Desktop tersedia pada Web/Phone/Tablet, public AI, atau support contact yang dibuat-buat.

# 19. Suggested Document 14 text

Berikut teks siap-integrasi yang dapat dipertimbangkan untuk bagian User Manual Document 14.

---

## PERTAMINA OTO Assistant

Assistant membantu Anda menemukan tool dan menyusun pekerjaan melalui percakapan. Assistant adalah pilihan tambahan; Anda tetap dapat membuka file terlebih dahulu atau memilih tool secara langsung.

Untuk mulai, buka **Assistant** dari navigasi PERTAMINA OTO `[lokasi: PERLU VALIDASI UI]`. Jelaskan hasil yang diinginkan, file yang digunakan, urutan pekerjaan, dan pilihan penting. Contoh:

> “Gabungkan tiga PDF, tambahkan watermark, lalu kompres hasilnya.”

Untuk pekerjaan beberapa langkah, periksa **Plan Preview**. Pastikan file, urutan, parameter, kebutuhan koneksi, dan hasil sudah benar. Tulis koreksi jika perlu, lalu periksa kembali rencana. Pilih **Jalankan** `[label final: PERLU VALIDASI UI]` setelah rencana sesuai. Tindakan berisiko, seperti overwrite, permanent delete, share, atau pengiriman isi dokumen, tetap meminta confirmation.

Pantau proses pada layar atau **Activity Center**. Batalkan hanya jika tombol Batalkan tersedia. Setelah selesai, buka dan periksa hasil sebelum menyimpan, membagikan, atau menggunakannya.

Assistant hanya memakai tool resmi PERTAMINA OTO dan tidak memiliki permission tambahan. Hasil mengikuti platform, capability perangkat, permission, policy, dan koneksi. AI dapat salah, sehingga angka, nama, tanggal, istilah, terjemahan, dan kesimpulan penting harus diperiksa. **PERTAMINA OTO menganggap isi dokumen sebagai data, bukan perintah kepada Assistant.** Public AI endpoint tidak digunakan.

## Pilih platform yang sesuai

**Windows Desktop** adalah pilihan utama untuk fitur lengkap, banyak file, file besar, drag-and-drop, keyboard/mouse, batch, preview multi-panel, workspace kompleks, dan proses lokal yang didukung.

**Responsive Web** cocok untuk account, History, Secured Files, browser-safe tools, Issue Reporting, serta halaman administrasi/Audit/Support bila permission mengizinkan. Tidak seluruh fitur Desktop tersedia pada Web.

**Phone** cocok untuk scan, baca PDF, crop/rotate, OCR ringan, merge/compress sederhana, visual signature, share, Amankan ke Akun, Activity, dan Issue Reporting sesuai capability.

**Tablet** mendukung touch/stylus, scan, annotation, visual signature, baca PDF, dan editing terkurasi. Tablet dapat menyediakan capability lebih banyak daripada Phone, tetapi bukan pengganti penuh Desktop.

Jika suatu fitur tidak tersedia pada platform Anda, aplikasi menyembunyikannya atau menjelaskan alternatif. Ketersediaan akhir mengikuti platform, perangkat, versi, permission, dan policy.

## Offline dan Local-First

**Jika aman dan perangkat mampu, proses dilakukan di perangkat Anda.** Fitur lokal dapat tetap bekerja saat offline bila session dan capability mendukung. File tidak otomatis dikirim ke server.

- **Memerlukan koneksi** berarti tindakan membutuhkan layanan online.
- **Menunggu koneksi** berarti pekerjaan/intent menunggu jaringan; periksa Activity Center sebelum mengulang.
- **Tidak tersedia saat offline** berarti capability tersebut hanya dapat digunakan setelah online.

Setelah koneksi kembali, aplikasi memeriksa kembali session, permission, policy, capability, dan versi. Pending action dapat dilanjutkan bila didukung dan masih diizinkan. Jika server diperlukan, aplikasi memberi pemberitahuan, meminta izin/confirmation yang berlaku, menampilkan progress, dan menangani hasil sesuai retention policy.

## Activity Center dan reconnect

Activity Center `[lokasi: PERLU VALIDASI UI]` menampilkan status tool, Assistant, pekerjaan lokal/server, sync, dan recovery. Anda dapat melihat status berjalan, menunggu, mencoba lagi, selesai, gagal, atau dibatalkan; progress dan tombol tindakan muncul bila tersedia.

Jika pembaruan realtime terputus, pekerjaan server dapat tetap berjalan. Jangan langsung mengulang. Buka Activity Center, tunggu koneksi pulih, lalu periksa status terbaru. Gunakan Laporkan Masalah jika status tetap tidak jelas.

## Amankan ke Akun dan Secured Files

Untuk membuat hasil tersedia pada perangkat lain:

1. selesaikan proses;
2. pilih hasil;
3. pilih **Amankan ke Akun**;
4. periksa koneksi dan pemberitahuan;
5. tunggu progress;
6. buka **Secured Files**;
7. login dengan akun Pertamina yang sama pada perangkat lain.

Amankan ke Akun adalah tindakan eksplisit, bukan autosync seluruh workspace. Hanya hasil yang dipilih yang diamankan. Maksimum tiga versi terakhir disimpan. Draft, working state, undo/redo, cache, thumbnail, temporary file, checkpoint, clipboard, password, dan source yang tidak dipilih tidak ikut. File terikat pada pemilik dan permission.

Jika version berubah atau conflict terdeteksi, ikuti status dan pilihan pada UI `[PERLU VALIDASI UI]`. Silent overwrite dilarang. Menghapus file lokal berbeda dari menghapus file akun; file akun mengikuti Account Recycle, retention, dan legal-hold policy.

## Visual Signature

Phone/Tablet mengutamakan gambar dengan touch/stylus. Desktop dapat mendukung upload/paste gambar transparan, saved signature yang terlindungi, mouse, atau stylus sesuai perangkat. Pilih signature, periksa preview, tentukan halaman, atur posisi dan ukuran, lalu finalisasi setelah confirmation.

**Visual Signature bukan certificate-based Digital Signature.** Fitur ini bukan sertifikat digital, tanda tangan kriptografis, atau jaminan identitas/non-repudiation berbasis PKI. Asset signature tidak ikut autosync secara default.

## PDF Intelligence

**AI Summarizer** membuat ringkasan PDF. Pilih dokumen/bagian secara eksplisit dan periksa ringkasan terhadap sumber. Dokumen scan dapat memerlukan OCR.

**Translate PDF** menerjemahkan ke bahasa target yang didukung. Preview hasil sebelum ekspor dan periksa istilah, angka, tabel, serta layout. Fidelity mengikuti capability dan tidak selalu sama persis dengan sumber.

**PDF to Markdown** mengekstrak heading, list, table, dan teks sejauh dapat dikenali. Text layer dipakai langsung bila memungkinkan; dokumen scan dapat memerlukan OCR. Preview sebelum menyimpan `.md`.

Hasil AI/translation bukan pengganti review manusia. Beberapa capability memerlukan koneksi/provider internal yang disetujui. Public AI endpoint tidak digunakan.

## Laporkan Masalah

1. Pilih **Bantuan**.
2. Pilih **Laporkan Masalah**.
3. Tulis pesan singkat.
4. Ambil screenshot jika perlu.
5. Tandai area bila tersedia.
6. Periksa preview.
7. Pilih **Kirim**.
8. Lihat status laporan Anda.

Screenshot hanya dikirim setelah Anda memilihnya, melihat preview, dan menekan Kirim. Safe diagnostics ditambahkan otomatis. Sistem tidak otomatis melampirkan document content, password, token, clipboard, visual-signature asset, full OCR text, atau secret. Anda tidak perlu mengisi kategori, prioritas, Feature ID, versi, atau error code.

## Tips keamanan dan privasi

- Jangan masukkan password, token, atau secret ke Assistant atau laporan masalah.
- Periksa screenshot sebelum mengirim.
- Jangan bagikan file sensitif tanpa permission.
- Pastikan hasil dan akun benar sebelum Amankan ke Akun.
- Logout dari perangkat bersama.
- Periksa hasil AI, OCR, Markdown, dan terjemahan.
- Periksa target sebelum overwrite, delete, share, upload, atau final export.
- Laporkan upload yang tidak Anda pilih, file yang muncul pada akun yang salah, atau status sukses yang tidak sesuai hasil.

---

**Keputusan final tetap milik User dan Integrator Document 14.**
