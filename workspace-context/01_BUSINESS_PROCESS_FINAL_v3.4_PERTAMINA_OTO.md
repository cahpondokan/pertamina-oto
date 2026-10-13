# 01 — PROSES BISNIS
## PERTAMINA OTO

> **Standar bahasa:** Bahasa Indonesia — ID-1.0
### Office Tools Organizer

---

## 1. Informasi Dokumen

| Atribut | Nilai |
|---|---|
| Nama dokumen | `01_BUSINESS_PROCESS.md` |
| Nama proyek | **PERTAMINA OTO — Office Tools Organizer** |
| Versi | **3.4** |
| Status | **FINAL — Selaras Standar Teknologi IT Pertamina** |
| Jenis aplikasi | Aplikasi internal Pertamina |
| Fungsi owner | Ditentukan melalui proses Requirement Initiation Checklist (RIC) |
| Target pengguna | Pekerja dan fungsi resmi Pertamina |
| Role awal | User, Administrator, Auditor |
| Web UI | ASP.NET Core MVC responsif |
| Backend | ASP.NET Core Web API — Clean Architecture |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core |
| Desktop | C#/.NET Windows Desktop, subject to architecture approval |
| Mobile/Tablet | Curated companion app C#/.NET untuk fitur terpilih, subject to architecture/security approval |
| Worker pemrosesan | C#/.NET Worker Service atau service lain yang disetujui |
| Repository dan CI/CD | Azure DevOps Git dan Azure DevOps Pipeline |
| Environment | Development, QA/Testing, Production |
| Monitoring | Application Insights / Application Performance Monitoring |
| Acuan | Standar Teknologi IT Pertamina dan persyaratan alih kelola aplikasi ke IT C&T |

### Riwayat Revisi

| Versi | Perubahan |
|---|---|
| 1.0 | Baseline awal proses bisnis |
| 1.1 | Penambahan ruang lingkup konversi dokumen |
| 2.0 | Penyelarasan awal dengan Standar Teknologi IT Pertamina |
| 2.1 | Perubahan nama proyek menjadi PERTAMINA OTO |
| 2.2 | Penghapusan Flutter dari baseline teknologi |
| 3.0 | Penyusunan ulang final, perbaikan struktur, konsistensi C#/.NET, tata kelola, keamanan, monitoring, dan handover |
| 3.1 | Pre-coding architecture hardening: Offline-First sebagai perilaku produk, Local-First berbasis capability platform, Adaptive UX, server-efficiency, Selective Cross-Device Sync, Business/UX Acceptance, dan Definition of Done berlapis |
| 3.2 | Penambahan PERTAMINA OTO Assistant sebagai Offline-First Hybrid Conversational Orchestrator, perubahan Dual Entry menjadi Triple Entry Flow, penguatan assistant security boundary, workflow validation, dan pre-coding feasibility gate |
| 3.3 | Solution-design alignment: Desktop-First sebagai client produktivitas terlengkap, curated Phone/Tablet companion, serta In-App Issue Reporting sederhana dengan screenshot, safe diagnostics, halaman Superadmin, dan export laporan |
| 3.4 | Controlled product-priority revision: menetapkan 33 Primary PDF Tools sebagai Core Feature Set yang dikerjakan sebelum user-tool sekunder; menambahkan PDF Intelligence dan menegaskan Sign PDF sebagai visual signature/paraf non-PKI |

---

## 2. Identitas Produk

```text
Nama utama:
PERTAMINA OTO

Kepanjangan:
Office Tools Organizer

Penulisan resmi:
PERTAMINA OTO — Office Tools Organizer
```

Aturan penamaan:

- Nama **PERTAMINA OTO** digunakan pada splash screen, halaman login, dashboard, dokumentasi, dan komunikasi resmi.
- Tulisan **Office Tools Organizer** ditampilkan sebagai subjudul agar arti “OTO” selalu jelas.
- Nama repository utama menggunakan `pertamina-oto`.
- Namespace teknis menggunakan pola `Pertamina.Oto.*`.
- Identitas visual wajib melalui persetujuan branding internal Pertamina.

---

## 3. Latar Belakang

Pekerja membutuhkan berbagai alat untuk membuka, mengubah, menggabungkan, memisahkan, mengompres, memindai, menandatangani, mengamankan, dan mengonversi dokumen maupun gambar.

Pada kondisi saat ini, pekerjaan tersebut sering dilakukan dengan banyak aplikasi atau situs berbeda. Kondisi ini menimbulkan masalah:

- User harus berpindah-pindah aplikasi.
- Alur kerja berbeda untuk setiap alat.
- File perusahaan berpotensi dikirim ke layanan publik.
- Pekerjaan berulang membutuhkan terlalu banyak langkah.
- Tidak semua alat dapat digunakan tanpa internet.
- Proses panjang sering tidak memberikan informasi progres.
- Riwayat, audit, logging, dan monitoring tidak terkelola secara konsisten.
- Dukungan dan handover aplikasi menjadi sulit karena teknologi serta dokumentasi tidak seragam.

PERTAMINA OTO dibuat sebagai aplikasi internal yang menyatukan berbagai kebutuhan office tools dalam satu sistem yang mudah digunakan, cepat, responsif, aman, dapat diaudit, dan dapat dialihkelolakan sesuai Standar Teknologi IT Pertamina.

---

## 4. Tujuan Bisnis

PERTAMINA OTO bertujuan untuk:

1. Menyatukan alat pengolahan dokumen dan gambar dalam satu aplikasi internal.
2. Mengurangi penggunaan layanan publik untuk dokumen perusahaan.
3. Mempercepat pekerjaan melalui drag-and-drop, copy-paste, paste screenshot, pencarian fitur, smart recommendation, dan batch processing.
4. Mengutamakan pemrosesan lokal yang disetujui untuk pekerjaan sederhana dan sensitif.
5. Menggunakan server internal untuk proses berat, kompleks, terpusat, atau yang memerlukan kontrol enterprise.
6. Memberikan pengalaman penggunaan minimal semudah aplikasi converter populer.
7. Menyediakan progress, tahapan, status, retry, resume, dan pemulihan proses.
8. Melindungi file asli dengan non-destructive editing.
9. Menyediakan autosave draft, undo, dan redo.
10. Menyediakan Single Sign-On, role management, audit trail, logging, dan monitoring.
11. Mengikuti proses RIC, persetujuan, pengembangan, QA, UAT, Production, support, dan handover Pertamina.
12. Menyediakan dokumentasi lengkap agar mudah dikembangkan bersama developer, Codex, dan tim IT.

---

## 5. Prinsip Proses Bisnis

```text
Mudah digunakan
Cepat dan responsif
Aman untuk data perusahaan
Non-destructive
Offline-First sebagai perilaku produk
Local-First sejauh capability platform memungkinkan
Server-efficient: server hanya untuk fungsi yang memang perlu terpusat
Selective Cross-Device Sync, bukan full workspace sync otomatis
User tetap memegang kendali
Otomatisasi membantu, bukan memaksa
Tidak ada proses panjang tanpa informasi
Semua aktivitas penting dapat diaudit
```

Prinsip tambahan:

- User tidak memilih teknologi atau engine pemrosesan.
- **Offline-First** adalah perilaku produk; aplikasi tetap memberikan kemampuan lokal yang aman saat koneksi tidak tersedia sesuai masa berlaku sesi dan capability perangkat.
- **Local-First** diterapkan sejauh platform, resource perangkat, security policy, dan library yang disetujui memungkinkan.
- Responsive tidak berarti UI dan kemampuan harus identik pada semua perangkat; sistem menggunakan **Adaptive UX** dan **Capability-Aware Execution**.
- Sistem menentukan metode terbaik berdasarkan file, perangkat, permission, koneksi, kapasitas, dan kebijakan.
- Fitur yang tidak didukung tidak boleh ditampilkan sebagai tombol yang berakhir gagal.
- Nama fitur, ikon, dan alur harus konsisten di seluruh kanal.
- Edit rutin tidak menampilkan konfirmasi berulang; Undo/Redo wajib tersedia.
- Konfirmasi hanya digunakan untuk tindakan final, permanen, berisiko, atau pengiriman dokumen ke server.
- Seluruh library, plugin, converter, dan komponen pihak ketiga wajib ditinjau lisensi dan keamanannya.

---

## 6. Ruang Lingkup Proses Bisnis

Rincian seluruh fitur individual dicatat pada `03_FEATURE_CATALOG.md`. Dokumen ini menetapkan kelompok proses bisnis utama.

### 6.1 Tool PDF

- Membuka dan membaca PDF.
- Menggabungkan dan memisahkan PDF.
- Menghapus, memutar, menduplikasi, memindahkan, dan mengurutkan halaman.
- Mengekstrak halaman dan gambar.
- Menambahkan halaman kosong, nomor halaman, header, footer, dan watermark.
- Mengompres, mengoptimalkan, meratakan, memperbaiki, dan mengamankan PDF.
- Mengisi atau membuat formulir PDF sesuai fitur yang disetujui.
- Menambahkan **tanda tangan visual/paraf (non-PKI)** dan anotasi; tanda tangan digital berbasis sertifikat/PKI tidak termasuk scope saat ini.
- Mengelola bookmark, metadata, attachment, dan preferensi viewer.
- Membuat indeks dan daftar isi dokumen.

### 6.2 Konversi Dokumen

- Word/DOCX ke PDF dan format lain yang disetujui.
- Excel/XLSX ke PDF atau CSV.
- PowerPoint/PPTX ke PDF.
- PDF ke Word, Excel, PowerPoint, TXT, JPG, PNG, atau WebP.
- Gambar ke PDF.
- TXT dan HTML ke PDF.
- PDF hasil scan menjadi searchable PDF melalui OCR.
- Ekstraksi tabel, teks, dan gambar dari dokumen.
- Konversi batch banyak file.
- Pemilihan halaman, sheet, slide, orientasi, kualitas, dan ukuran kertas.

### 6.3 Tool Gambar

- Membuka dan melihat gambar.
- Konversi JPG, PNG, WebP, BMP, dan format yang disetujui.
- Crop, rotate, flip, resize, compress, grayscale, dan pengaturan kualitas.
- Menghapus atau mengganti background.
- Menggabungkan gambar vertikal/horizontal.
- Membuat kolase dan foto formal.
- Menambahkan teks, panah, bentuk, watermark, sensor, dan tanda tangan visual/paraf (non-PKI).
- Membandingkan hasil sebelum dan sesudah.
- Melihat dan menghapus metadata gambar.

### 6.4 Scanner dan OCR

- Scan dokumen melalui kamera atau scanner yang didukung.
- **Scanner kamera diprioritaskan pada mobile/tablet** karena alur touch-camera paling sesuai untuk pekerjaan lapangan.
- Deteksi tepi, crop otomatis, koreksi perspektif, dan filter dokumen.
- Scan multi-halaman.
- OCR Bahasa Indonesia dan English.
- Ekstraksi teks dan tabel.
- Penyusunan searchable PDF.
- Koreksi hasil OCR sebelum ekspor.
- Pada desktop, fokus utama adalah impor file/gambar hasil scan dan OCR; integrasi scanner fisik dapat ditambahkan bila disetujui.

### 6.5 Utilitas Produktivitas

- QR Code generator.
- File checksum.
- Batch rename.
- ZIP hasil batch.
- Informasi dokumen dan jumlah halaman.
- Text cleaner.
- Clipboard screenshot.
- Favorite tool, preset, dan repeat last action.
- Recent file dan recent tool.
- Print dan export sesuai platform serta permission.

### 6.6 Manajemen File

- Pilih file.
- Drag-and-drop file atau folder.
- Copy-paste file.
- Paste gambar atau screenshot dari clipboard.
- Share/Open With dari aplikasi lain.
- Recent files.
- Folder watcher opsional dan terbatas.
- Deteksi duplikat.
- Pemulihan sesi.
- Pilihan mode tampilan: ikon, daftar, detail, ubin, isi, panel rincian, dan panel pratinjau.

### 6.7 PERTAMINA OTO Assistant

PERTAMINA OTO menyediakan **Offline-First Hybrid Conversational Orchestrator** sebagai jalur penggunaan tambahan. Assistant tidak menjadi processing engine dan tidak memiliki akses bebas ke file system, database, shell, atau service.

Kemampuan utama:

- Perintah natural Bahasa Indonesia dan English.
- Local Intent Router untuk perintah umum saat offline.
- Pencarian tool melalui percakapan.
- Penyusunan workflow multi-step dari Feature ID resmi.
- Ekstraksi dan validasi parameter berdasarkan schema fitur.
- Rekomendasi fitur sesuai file, platform, capability, permission, policy, dan koneksi.
- Plan preview untuk workflow multi-step atau aksi yang perlu dipastikan.
- Jika intent/parameter ambigu atau confidence rendah, Assistant meminta klarifikasi singkat dan tidak menebak aksi berisiko.
- User dapat meninjau/mengoreksi parameter atau langkah workflow sebelum eksekusi jika diperlukan.
- Eksekusi tool melalui application layer resmi.
- Jawaban hasil proses harus bersumber dari status/output tool yang benar-benar diterima, bukan hasil yang diasumsikan Assistant.
- Contextual help, error explanation, recovery guidance, preset, dan repeat workflow.
- Optional local small language model hanya setelah architecture/security/license/performance review.
- Internal AI fallback hanya untuk instruksi yang benar-benar membutuhkan reasoning tambahan dan ketika policy/koneksi mengizinkan.

Prinsip wajib:

- Assistant hanya boleh memilih **Feature ID canonical** yang terdaftar pada `03_FEATURE_CATALOG.md`.
- Parameter harus lolos schema validation sebelum eksekusi.
- Role, permission, policy, capability, dan confirmation rule tetap berlaku seperti UI biasa.
- Ketidaktersediaan AI service tidak boleh menonaktifkan tool lokal yang sebenarnya dapat digunakan.
- Isi dokumen tidak otomatis dikirim ke AI; hanya data minimum yang diperlukan dan diizinkan yang dapat dipakai.
- Teks/instruksi yang ditemukan di dalam dokumen diperlakukan sebagai **data**, bukan perintah kepada Assistant.

---

### 6.8 Permukaan Primary PDF Tool — Core Feature Set

PERTAMINA OTO memiliki **33 tool utama** yang menjadi prioritas produk dan prioritas pengembangan user-facing. Tool sekunder dari Feature Catalog tetap berada dalam scope, tetapi implementasinya dilakukan setelah Core Feature Set ini selesai atau memiliki keputusan feasibility/exception resmi.

#### Mengatur PDF
- Merge PDF.
- Split PDF.
- Remove pages.
- Extract pages.
- Organize PDF.
- Scan to PDF.

#### Mengoptimalkan PDF
- Compress PDF.
- Repair PDF.
- OCR PDF.

#### Konversi ke PDF
- JPG to PDF.
- Word to PDF.
- PowerPoint to PDF.
- Excel to PDF.
- HTML to PDF.

#### Konversi dari PDF
- PDF to JPG.
- PDF to Word.
- PDF to PowerPoint.
- PDF to Excel.
- PDF to PDF/A.

#### Mengedit PDF
- Rotate PDF.
- Add page numbers.
- Add watermark.
- Crop PDF.
- Edit PDF.
- PDF Forms.

#### Keamanan PDF
- Unlock PDF — hanya dengan password/otorisasi yang sah; bukan mekanisme bypass proteksi.
- Protect PDF.
- **Sign PDF — tanda tangan visual/paraf non-PKI**, bukan certificate/PKI digital signature.
- Redact PDF.
- Compare PDF.

#### PDF Intelligence
- AI Summarizer.
- Translate PDF.
- PDF to Markdown.

Aturan prioritas:
- **Primary Feature Set First:** user-tool sekunder tidak didahulukan dari Core Feature Set.
- Foundation, authentication, security, audit, observability, storage, dan platform capability tetap boleh/dan harus dibangun sebagai dependency agar Core Feature Set dapat berjalan dengan benar.
- **Common-Simple-First tetap berlaku di dalam Core Feature Set** untuk menentukan urutan teknis yang paling aman.
- Fitur Core yang memerlukan Proof-of-Capability tetap diprioritaskan PoC-nya; status gate tidak boleh disamarkan sebagai fitur selesai.
- Jika satu fitur Core terblokir secara teknis, development dapat melanjutkan fitur Core lain. User-tool sekunder baru dimulai setelah item Core berstatus `DONE`, `APPROVED_WITH_LIMITATION`, atau memiliki keputusan `BLOCKED_ALTERNATIVE_REQUIRED/DEFERRED` yang disetujui.

---

## 7. Aktor dan Role

### 7.1 User

User menggunakan office tools sesuai permission.

Kewenangan utama:

- Memasukkan, melihat, mengedit, mengonversi, dan mengekspor file.
- Menggunakan fitur lokal dan server yang diizinkan.
- Melihat riwayat proses pribadi.
- Mengelola draft, hasil, preset, dan preferensi miliknya.
- Menghapus data lokal miliknya sesuai kebijakan.

Batasan:

- Tidak dapat mengubah konfigurasi enterprise.
- Tidak dapat mengelola user lain.
- Tidak dapat menonaktifkan audit wajib.
- Tidak otomatis dapat mengakses audit pengguna lain.

### 7.2 Administrator

Administrator mengelola konfigurasi dan kesehatan aplikasi.

Kewenangan utama:

- Mengelola konfigurasi fitur, limit, maintenance mode, dan versi minimum.
- Mengelola user dan role sesuai kebijakan Pertamina.
- Melihat kesehatan layanan, sinkronisasi, job, error teknis, dan monitoring.
- Mengaktifkan atau menonaktifkan fitur berdasarkan kebijakan.
- Mengatur retensi, klasifikasi, dan profil keamanan yang disetujui.

Batasan:

- Tidak otomatis dapat membaca isi dokumen user.
- Tidak boleh melihat password PDF, token, isi clipboard, tanda tangan, atau teks OCR lengkap tanpa kewenangan khusus.

### 7.3 Auditor

Auditor memeriksa aktivitas dan kepatuhan.

Kewenangan utama:

- Melihat audit trail dan laporan.
- Menyaring berdasarkan user, unit, waktu, fitur, status, dan platform.
- Melihat perubahan konfigurasi dan role.
- Mengekspor laporan audit sesuai permission.

Batasan:

- Tidak dapat mengubah konfigurasi.
- Tidak dapat menghapus audit trail.
- Tidak otomatis dapat membuka isi dokumen.

Role tambahan dapat dibuat melalui permission-based access control tanpa mengubah business rule utama.

---

## 8. Kanal Aplikasi

### 8.1 Responsive Web — Kanal Enterprise dan Browser

Responsive Web tetap menjadi kanal resmi untuk akses berbasis browser, account, history, selective sync, admin/audit/support sesuai permission, dan tool yang aman serta masuk akal dijalankan melalui browser.

Teknologi baseline:

```text
ASP.NET Core MVC
ASP.NET Core Web API
C#
Microsoft SQL Server
Entity Framework Core
```

Web API dan Web UI tetap merupakan deployable terpisah. Keduanya dapat ditempatkan pada infrastruktur fisik yang sama pada tahap Development/QA/Pilot bila kapasitas dan kebijakan mengizinkan, lalu dipisahkan tanpa mengubah business logic ketika Production membutuhkannya.

### 8.2 Windows Desktop — Klien Full-Feature Utama

Windows Desktop menjadi **client produktivitas utama dan target fitur paling lengkap**.

Fokus utama:

- Pemrosesan lokal paling lengkap.
- File besar dan batch processing.
- Drag-and-drop file/folder.
- Clipboard dan screenshot.
- Shortcut keyboard.
- Integrasi File Explorer.
- Preview multi-panel.
- Workflow kompleks.
- Local Worker untuk proses berat.
- Fitur PDF, conversion, image, OCR, file productivity, workspace, dan Assistant yang memerlukan pengalaman desktop penuh.

Teknologi desktop menggunakan C#/.NET dan wajib melalui review arsitektur, security, packaging, deployment, dan support.

### 8.3 Aplikasi Mobile/Tablet — Pengalaman Companion Terkurasi

Phone dan tablet **bukan salinan Desktop**. Mobile/Tablet menyediakan fitur terpilih yang lebih mudah, cepat, atau natural dikerjakan melalui perangkat bergerak.

Contoh fokus:

- Scan dokumen dengan kamera.
- Baca PDF.
- Crop/rotate/reorder sederhana.
- OCR ringan.
- Merge/compress sederhana bila capability mencukupi.
- Visual signature/paraf dengan touch/stylus.
- Anotasi ringan.
- Share hasil.
- Amankan ke Akun dan membuka file yang telah diamankan.
- Activity/status pekerjaan.
- Assistant untuk capability yang tersedia pada perangkat.

Tablet dapat memiliki UX yang lebih kaya daripada phone karena ukuran layar lebih besar.

Client mobile/tablet C#/.NET harus menggunakan Web API resmi, tidak mengakses SQL Server langsung, dan tetap mengikuti permission, audit, monitoring, serta security policy perusahaan.

### 8.4 Prinsip Desktop-First, Curated Mobile, dan Capability-Aware

| Kanal | Fokus capability |
|---|---|
| Windows Desktop C#/.NET | **FULL / primary productivity client**; local processing paling lengkap, file besar, batch, folder, clipboard, shortcut, multi-panel, workflow kompleks |
| Mobile/Tablet | **Curated feature set**; scanner kamera, baca PDF, visual signature/paraf, OCR/editing ringan, tool sederhana dan sering digunakan |
| Responsive Web | Account, history, selective sync, admin/audit/support, tool browser-safe, serta fallback server bila sesuai policy |

Aturan:

- Feature Catalog tetap satu dan canonical.
- Desktop menjadi target fitur terlengkap.
- Phone/Tablet hanya mengekspos fitur yang memang berguna dan nyaman pada perangkat tersebut.
- Tablet dapat lebih kaya daripada phone.
- Availability ditentukan oleh **platform exposure + device capability + permission + policy + engine availability**.
- Sistem menyembunyikan fitur yang tidak relevan/tidak didukung atau memberikan alternatif yang jelas.
- User tidak dibebani pilihan engine teknis.
- Satu fitur dapat memiliki provider/UX berbeda antarplatform tanpa membuat business rule paralel.
- Responsive tidak berarti UI identik.


## 9. Model Navigasi: Tiga Jalur Masuk (`Triple Entry Flow`)

### 9.1 File-First

```text
Upload / Drop / Paste / Share / Scan
                ↓
Sistem mengenali jenis dan jumlah file
                ↓
Sistem menampilkan 3–5 tindakan relevan
                ↓
User memilih tindakan
                ↓
Preview / Edit / Proses
                ↓
Finalisasi dan simpan hasil
```

Contoh:

```text
Paste screenshot
→ Crop
→ OCR
→ Kompres
→ Hapus background
→ Simpan sebagai gambar
→ Masukkan ke PDF
```

### 9.2 Tool-First

```text
Cari fitur
    ↓
Pilih tool
    ↓
Masukkan file
    ↓
Atur parameter jika diperlukan
    ↓
Preview / Proses
    ↓
Simpan hasil
```

Pencarian mendukung istilah natural, misalnya:

| Kata yang dicari | Fitur |
|---|---|
| kecilkan PDF | Kompres PDF |
| hapus latar | Hapus Background |
| ambil tulisan | OCR / Gambar ke Teks |
| gabung gambar jadi PDF | Gambar ke PDF |
| ubah JPG ke PNG | Konversi Gambar |

### 9.3 Assistant-First

```text
User menjelaskan hasil yang diinginkan
        ↓
Assistant memahami intent secara lokal bila memungkinkan
        ↓
Assistant memetakan ke Feature ID canonical
        ↓
Validasi parameter + input/output compatibility
        ↓
Capability + Permission + Policy Check
        ↓
Plan Preview / Confirmation bila diperlukan
        ↓
Tool resmi PERTAMINA OTO mengeksekusi
        ↓
Progress / Result / Recovery
```

Contoh:

```text
"Gabungkan PDF ini, hapus halaman kosong,
beri nomor halaman, lalu kompres."

→ Merge PDF
→ Detect/Remove Blank Pages
→ Add Page Number
→ Compress PDF
```

Assistant tidak boleh menciptakan Feature ID, parameter, atau kemampuan yang tidak terdaftar.

---

## 10. Sumber File

Sumber file yang didukung:

1. File picker.
2. Drag-and-drop satu atau banyak file.
3. Drag-and-drop folder yang diizinkan.
4. Copy-paste file.
5. Paste gambar dan screenshot.
6. Kamera atau scanner.
7. Share/Open With.
8. Recent files.
9. Folder watcher opsional.
10. Pemulihan draft atau sesi.

### 10.1 Folder Watcher

- Default tidak aktif.
- Hanya memantau folder yang dipilih user.
- Tidak boleh memindai seluruh penyimpanan.
- Tidak boleh memproses file tanpa tindakan user.
- Hanya menampilkan rekomendasi.

### 10.2 Paste Screenshot

Sistem harus:

- Mendeteksi data gambar pada clipboard.
- Membuat preview.
- Memberikan nama sementara.
- Menawarkan crop, OCR, compress, background removal, convert to PDF, atau save.
- Tidak menyimpan atau mengirim isi clipboard ke server tanpa tindakan user.

---

## 11. Alur Proses Utama

```text
1. User membuka PERTAMINA OTO.
2. Sistem menyesuaikan layout dengan perangkat dan ukuran layar.
3. Sistem memulihkan sesi aman bila tersedia.
4. User melakukan login/SSO.
5. Sistem memvalidasi role dan permission.
6. User memilih jalur File-First, Tool-First, atau Assistant-First.
7. User memasukkan file.
8. Sistem melakukan validasi bertingkat.
9. Sistem memberikan smart recommendation bila relevan.
10. User mengatur parameter dan melakukan edit non-destructive.
11. Sistem menjalankan autosave draft.
12. User memulai proses.
13. Sistem menentukan engine yang disetujui.
14. Sistem menampilkan progres, tahap, dan status.
15. Sistem memvalidasi hasil.
16. Sistem menampilkan ringkasan perubahan.
17. User menyimpan, mengekspor, mengunduh, membagikan, atau mencetak.
18. Sistem memperbarui riwayat.
19. Audit metadata disimpan dan disinkronkan otomatis.
20. File sementara dibersihkan.
```

---

## 12. Validasi File

Validasi menggunakan pendekatan bertingkat.

### 12.1 Pemeriksaan Instan

- Nama file.
- Ekstensi.
- Ukuran.
- MIME type.
- File signature/header.
- Hak akses.

### 12.2 Pemeriksaan Saat File Dipilih

- File dapat dibuka.
- Password atau encryption.
- Jumlah halaman.
- Resolusi.
- Potensi duplikat.
- Kapasitas penyimpanan.

### 12.3 Pemeriksaan Khusus

Hanya jika diperlukan:

- Hash penuh.
- Antivirus atau malware scanning.
- Validasi struktur mendalam.
- Pemeriksaan embedded object.
- Pemeriksaan server-side.

Pemeriksaan berat tidak boleh menghambat UI.

---

## 13. Pemrosesan Lokal dan Server

### 13.1 Prinsip

- **Offline-First adalah perilaku produk.**
- **Local-First diterapkan berdasarkan capability platform**, resource perangkat, keamanan, dan library yang disetujui.
- User tidak memilih engine.
- Sistem memilih metode terbaik secara otomatis.
- Proses sederhana diprioritaskan lokal bila platform dan kebijakan memungkinkan.
- Windows Desktop menjadi kandidat local processing paling lengkap; browser/mobile menggunakan capability yang tersedia.
- Proses berat, kompleks, terpusat, atau diwajibkan kebijakan menggunakan server internal.
- Isi dokumen tidak dikirim ke server tanpa pemberitahuan.
- Audit metadata dan metadata ringan yang disetujui dapat disinkronkan otomatis tanpa konfirmasi berulang.
- Keterbatasan server awal tidak menjadi alasan menghapus fitur resmi; strategi eksekusi dan fase rilis yang disesuaikan.

### 13.2 Pemrosesan Lokal

Pemrosesan lokal dapat digunakan pada client desktop C#/.NET yang telah disetujui untuk:

- Manipulasi halaman PDF.
- Konversi gambar sederhana.
- Resize dan compress.
- Watermark.
- Preview.
- Batch lokal yang sesuai kapasitas.
- Fitur lain yang disetujui pada desain solusi.

### 13.3 Pemrosesan Server

Server internal digunakan untuk:

- Konversi Office/PDF kompleks.
- OCR atau AI berat.
- Ekstraksi tabel kompleks.
- Proses yang memerlukan library terpusat.
- Proses yang membutuhkan monitoring dan capacity control.
- Proses yang diwajibkan kebijakan keamanan.

Server processing menggunakan ASP.NET Core Web API dan C#/.NET Worker Service atau komponen lain yang telah disetujui arsitektur.

Pemberitahuan kepada user:

```text
Koneksi diperlukan

Fitur ini memerlukan server internal
untuk memproses dokumen.

[Batal] [Lanjutkan]
```


### 13.4 Anggaran Sumber Daya Server

Setiap fitur pada `03_FEATURE_CATALOG.md` wajib memiliki metadata:

```text
Processing Mode   : Local / Server / Hybrid
Server Dependency : None / Optional / Required
Storage Impact    : None / Metadata / File
Network Impact    : None / Low / Medium / High
Concurrency Risk  : Low / Medium / High
```

Prinsip efisiensi server:

- Draft, working state, undo/redo, cache, thumbnail, dan temporary file tidak disinkronkan penuh secara otomatis.
- Gunakan batching, asynchronous/background processing, pagination, lazy loading, retention, cleanup, quota, capacity check, queue, dan backpressure bila relevan.
- Server diprioritaskan untuk SSO/account, role/permission, policy/configuration, audit metadata, monitoring, **Amankan ke Akun**, selective cross-device sync, dan proses khusus yang memang membutuhkan server.
- Capacity dan load/performance test menjadi gate sebelum perluasan skala pengguna.

### 13.5 Pemrosesan Assistant dan Routing AI

Urutan pemrosesan Assistant:

1. Local deterministic intent/alias/command parser diprioritaskan.
2. Local workflow planner menggunakan canonical feature registry dan parameter schema.
3. Optional local model hanya digunakan bila tersedia, diizinkan, dan lolos benchmark.
4. Internal AI fallback dapat dipakai untuk instruksi kompleks dengan timeout, quota/rate control, capacity check, dan fallback yang jelas.
5. Processing file tetap mengikuti Local/Server/Hybrid mode milik fitur yang dipanggil; penggunaan AI tidak otomatis memindahkan file processing ke server.

Untuk workflow multi-step, sistem melakukan preflight terhadap format input/output antar-step, permission, resource perangkat, kebutuhan temporary storage, network, dan server dependency sebelum proses dimulai. Output intermediate bersifat temporary/non-final sampai workflow selesai atau user memilih menyimpannya.

---

## 14. Editing, Undo, Redo, dan Finalisasi

Editing dilakukan secara non-destructive.

Tindakan rutin yang tidak memerlukan dialog:

- Hapus halaman dari working state.
- Rotate.
- Crop.
- Ubah urutan.
- Tambah watermark sementara.
- Ubah kualitas.
- Hapus item dari antrean.

Setelah tindakan:

```text
1 halaman dihapus    [Urungkan]
```

Konfirmasi hanya digunakan untuk:

- Menimpa file asli.
- Menghapus file permanen.
- Mengirim isi dokumen ke server.
- Membagikan dokumen.
- Menutup sesi dengan perubahan belum disimpan.
- Final export atau download.
- Tindakan yang tidak dapat dipulihkan.

Sebelum finalisasi, tampilkan ringkasan perubahan.

---

## 15. Autosave, Penyimpanan, dan Recovery

### 15.1 Autosave

Autosave menyimpan draft/state, bukan file final.

Yang disimpan:

- Referensi file.
- Urutan halaman.
- Halaman yang dihapus.
- Rotasi dan crop.
- Parameter proses.
- Posisi terakhir.
- Status job.
- Checkpoint proses panjang.

File asli tidak pernah ditimpa otomatis.

### 15.2 Penyimpanan Mode Kinerja

- Metadata-only history.
- Lazy loading.
- Virtual scrolling.
- Thumbnail on-demand.
- Pagination.
- Maksimal 50 item pada pemuatan awal.
- Hash penuh hanya bila diperlukan.
- Tidak memindai seluruh penyimpanan.
- Background cleanup.
- File asli tidak disalin tanpa kebutuhan.

### 15.3 File Sementara

```text
Buat file sementara
→ proses
→ validasi hasil
→ rename menjadi file final
→ hapus file sementara
```

Jika aplikasi berhenti:

- Sesi dapat dipulihkan.
- File setengah jadi tidak dianggap final.
- User dapat melanjutkan, mencoba ulang, atau membuang draft.


### 15.4 Workspace Local-First + Sinkronisasi Selektif Antarperangkat

Workspace aktif tetap lokal secara default:

- Draft.
- Working state.
- Undo/Redo.
- Cache dan thumbnail.
- Temporary file.
- Checkpoint utama.
- File sumber yang tidak dipilih untuk backup.

Cross-device hanya untuk data yang memang dipilih atau ringan:

- File hasil yang dipilih user melalui tombol **Amankan ke Akun**.
- Metadata file yang diamankan.
- Maksimal tiga versi terakhir file yang diamankan.
- Daftar file yang diamankan.
- Favorite tools, preset tertentu, preferensi bahasa/tampilan, dan status versi bila disetujui.

File yang telah diamankan dapat dibuka atau diunduh dari perangkat lain setelah user login dengan akun Pertamina yang sama. Full Cloud Workspace dan autosync seluruh draft/working state bukan baseline saat ini dan hanya dapat dipertimbangkan setelah kapasitas infrastruktur, keamanan, biaya, serta disaster recovery disetujui.

---

## 16. Progress dan Komunikasi

Tidak boleh ada proses panjang tanpa pesan.

User harus mengetahui:

- Persentase.
- Tahap yang sedang dilakukan.
- File/halaman/sheet/slide yang sedang diproses.
- Jumlah selesai dan sisa.
- Estimasi waktu bila memungkinkan.
- Status berhasil, gagal, tertunda, atau menunggu koneksi.
- Opsi batalkan jika aman.

Contoh:

```text
Memproses PDF...

62%

Menggabungkan halaman 31 dari 50
Estimasi sekitar 12 detik lagi

[Batalkan]
```

### 16.1 Sistem Komunikasi Cerdas

- Snackbar untuk informasi ringan.
- Undo untuk tindakan rutin.
- Dialog hanya untuk tindakan final/berisiko.
- Progress panel untuk proses panjang.
- Notifikasi sistem untuk proses background.
- Activity Center untuk job berjalan, selesai, gagal, atau menunggu tindakan.
- Detail teknis hanya ditampilkan saat diminta atau kepada role yang berwenang.

---

## 17. UI/UX, Responsivitas, dan Aksesibilitas

Target UX:

```text
Minimal semudah aplikasi converter populer,
lebih unggul pada keamanan internal,
clipboard, autosave, progress, audit, dan integrasi enterprise.
```

Ketentuan:

- Responsif pada phone, tablet, laptop, dan desktop.
- Menggunakan **Adaptive UX**: responsive tidak berarti UI harus identik pada semua perangkat.
- Scanner kamera dan drawing signature/paraf diprioritaskan pada mobile; desktop memprioritaskan drag-drop, file besar, batch, keyboard, dan multi-panel.
- Tidak boleh overflow.
- Mendukung portrait dan landscape.
- Mendukung text scaling.
- Mendukung keyboard navigation.
- Ikon selalu disertai teks pada tindakan penting.
- Informasi tidak hanya mengandalkan warna.
- Mendukung screen reader sesuai kemampuan platform.
- User dapat memilih mode tampilan data.
- Preferensi tampilan disimpan.
- Nama fitur dan alur konsisten.
- User tidak melihat istilah engine, worker, API, atau server kecuali diperlukan.
- Assistant adalah **jalur alternatif**, bukan kewajiban; seluruh fungsi tetap dapat ditemukan melalui File-First/Tool-First sesuai capability.
- Assistant wajib memperlihatkan rencana yang dapat dipahami untuk workflow multi-step dan tidak boleh mengeksekusi aksi irreversible tanpa confirmation yang berlaku.
- Ketika offline, Assistant menjelaskan dengan jelas bagian yang dapat berjalan lokal dan bagian yang menunggu koneksi.

---

## 18. Multi-Bahasa

Bahasa awal:

```text
Bahasa Indonesia
English
```

Perilaku:

- Mengikuti bahasa perangkat jika didukung.
- Fallback ke Bahasa Indonesia.
- User dapat mengganti bahasa manual.
- Seluruh UI, error, tooltip, progress, notifikasi, dan accessibility text dilokalisasi.
- Localization menggunakan key-based resources.
- Layout wajib aman terhadap perbedaan panjang teks.

---

## 19. Autentikasi dan Akses

Model autentikasi:

```text
Single Sign-On adaptif
```

Ketentuan:

- User login sekali.
- Token disimpan pada secure storage.
- Token diperbarui otomatis.
- Password tidak disimpan pada file lokal.
- Proses server memerlukan sesi online valid.
- Fitur lokal dapat digunakan selama sesi offline masih valid.
- Administrator dapat mencabut sesi.
- Perubahan role diterapkan saat sinkronisasi.
- Verifikasi ulang hanya untuk tindakan sensitif.
- Login, logout, gagal login, dan pencabutan sesi dicatat pada audit trail.

Implementasi identitas mengikuti user management dan role management standar Pertamina.

---

## 20. Keamanan Dokumen

Model:

```text
Enterprise Security Simplified
```

Ketentuan:

- Default aman.
- File asli tidak diubah langsung.
- Cache lokal terenkripsi bila menyimpan data sensitif.
- Password PDF tidak dicatat.
- Token dan secret tidak masuk log.
- File sementara dihapus otomatis.
- Upload dokumen memerlukan pemberitahuan.
- Audit hanya mencatat metadata yang diperlukan.
- Hak share, copy, print, download, dan export mengikuti role serta klasifikasi.
- Administrator tidak otomatis dapat membaca isi dokumen.
- Library pihak ketiga wajib melalui license review, vulnerability review, dan security approval.

Kontrol keamanan enterprise:

- HTTPS.
- Reverse proxy.
- Security headers.
- Request size limit.
- Rate limiting bila diperlukan.
- Vulnerability assessment.
- Penetration test.
- SAST, DAST, dependency scanning, secret scanning, dan SBOM sesuai kebijakan.
- Remediasi dan retest untuk temuan Critical/High.

### 20.1 Batas Keamanan untuk Assistant

- Assistant tidak memiliki direct database access, unrestricted file-system access, shell access, atau arbitrary API execution.
- Assistant hanya melakukan tool invocation melalui registry dan application service yang disetujui.
- System instruction, policy, permission, dan parameter schema memiliki prioritas di atas isi percakapan maupun isi dokumen.
- Konten dokumen yang dibaca untuk OCR/extraction/help diperlakukan sebagai untrusted data dan tidak boleh mengubah tool policy.
- Prompt/chat content tidak disinkronkan ke server secara default; audit mencatat metadata intent/Feature ID/status yang diperlukan, bukan seluruh percakapan.
- Jika layanan AI internal memerlukan bagian isi dokumen, sistem harus melakukan policy check, data minimization, klasifikasi/permission check, dan pemberitahuan/consent sesuai risiko sebelum transfer.
- Assistant tidak boleh menyimpan password PDF, token, clipboard content, visual signature asset, atau secret di conversation history.
- Conversation state disimpan lokal/minimal secara default dan user dapat membersihkan percakapan tanpa menghapus enterprise audit metadata yang diwajibkan.

---

## 21. Riwayat dan Audit Trail

### 21.1 Riwayat User

Riwayat user menampilkan:

- Nama file.
- Fitur.
- Waktu.
- Status.
- Lokasi hasil.
- Ukuran.
- Durasi.
- Buka hasil.
- Buka folder.
- Proses ulang.
- Hapus dari riwayat pribadi.

### 21.2 Audit Trail

Audit mencatat metadata:

- Process ID.
- User ID.
- Role.
- Unit/fungsi bila tersedia.
- Platform dan versi aplikasi.
- Fitur.
- Waktu mulai dan selesai.
- Status.
- Ukuran file.
- Lokal atau server.
- Error code.
- Login/logout.
- Perubahan konfigurasi.
- Perubahan role.
- Export audit.

Audit tidak mencatat:

- Isi dokumen.
- Isi clipboard.
- Screenshot.
- Password.
- Token.
- Tanda tangan.
- Teks OCR lengkap.

### 21.3 Sinkronisasi Audit

- Disimpan lokal terenkripsi saat offline bila diperlukan.
- Disinkronkan otomatis ketika koneksi tersedia.
- Dikirim batch di background.
- Mendukung retry.
- Tidak menghambat UI.
- User tidak diminta konfirmasi setiap sinkronisasi.
- User diinformasikan melalui kebijakan privasi dan halaman informasi aplikasi.

---

## 22. Pemrosesan Tangguh

Sistem harus menangani:

- Aplikasi tertutup.
- Laptop mati.
- Penyimpanan penuh.
- File rusak.
- Koneksi terputus.
- Server tidak tersedia.
- User membatalkan proses.
- Job berhenti di tengah.
- Antrean penuh.

Mekanisme:

- Job ID.
- Checkpoint.
- Retry terbatas.
- Resume upload/download bila didukung.
- Validasi hasil.
- Recovery saat startup.
- Fallback yang disetujui.
- Correlation ID.
- Error code untuk support.
- File asli tetap aman.

---

## 23. Tata Kelola Requirement

Alur formal:

```text
1. User/Fungsi Owner menyusun RIC.
2. RIC ditinjau dan ditandatangani selevel VP sesuai ketentuan.
3. User membuat Nota Dinas.
4. Nota Dinas dilengkapi RIC yang telah disetujui.
5. Dokumen dikirim kepada Manager Business Relationship.
6. Business Relationship dan IT melakukan review.
7. Hasil review menjadi dasar persetujuan pengembangan.
```

Aturan:

- Perubahan scope material menggunakan change request.
- Setiap requirement memiliki owner.
- Setiap requirement memiliki traceability ke feature, test case, UAT, dan release.
- Rollout lintas Region memerlukan persetujuan Fungsi Owner di Pusat.
- Bukti persetujuan disimpan pada repository dokumentasi resmi.

---

## 24. Siklus Pengembangan dan Rilis

```text
Requirement & Approval
        ↓
Solution Design
        ↓
Development
        ↓
Unit Test
        ↓
QA / Integration Test
        ↓
Security Test
        ↓
UAT
        ↓
Production Deployment
        ↓
Monitoring & Support
        ↓
Handover
```

### 24.1 Pengembangan

- Source code di Azure DevOps Git.
- Data uji/sintetis.
- Build otomatis.
- Debug hanya pada environment yang diizinkan.
- Fitur dengan risiko dependency tinggi wajib melewati **Proof-of-Capability/Technical Feasibility Gate** sebelum implementasi penuh. Kandidat awal meliputi PDF content editing, Office↔PDF, PDF→Office, OCR tabel kompleks, background removal, HTML/Web→PDF, PDF/A, integrasi scanner fisik, dan local AI model.
- Gate memverifikasi kualitas hasil, lisensi, dukungan format, resource consumption, platform support, security, maintainability, dan fallback sebelum library/engine dikunci.

### 24.2 QA/Pengujian

- Functional test.
- **Business Process Acceptance**: hasil dan urutan kerja harus sesuai business outcome yang ditetapkan.
- **UX Acceptance**: alur harus mudah dipahami, tidak menambah langkah/dialog yang tidak perlu, responsif/adaptive, dan memberikan feedback yang jelas.
- Integration test.
- Regression test.
- Performance test termasuk server resource budget dan concurrency sesuai fase.
- Security validation.
- Cross-platform/capability behavior test untuk fitur yang berbeda antara Web, Desktop, dan Mobile.
- Error, recovery, retry, dan resume test.
- UAT setelah QA memenuhi kriteria.
- Tidak menggunakan data sensitif Production tanpa masking dan approval.

### 24.3 Produksi

- Artefak berasal dari pipeline.
- Deployment memiliki approval.
- Debug nonaktif.
- Secret dikelola aman.
- Monitoring aktif.
- Rollback plan tersedia.
- Deployment manual tanpa jejak audit tidak diperbolehkan.


### 24.4 Definition of Done Berlapis

Fitur tidak dinyatakan selesai hanya karena build dan test teknis berhasil. Fitur dinyatakan **DONE** apabila seluruh lapisan berikut memenuhi acceptance yang ditetapkan:

1. Technical Correctness.
2. Business Process Correctness.
3. UI/UX Acceptance.
4. Security & Privacy.
5. Performance / Server Budget.
6. Error & Recovery.
7. Cross-Platform Behavior.
8. Test & Documentation.

Untuk fitur penting, minimal tersedia tiga sudut acceptance:

```text
Functional Acceptance
Business Acceptance
UX Acceptance
```

Kegagalan salah satu lapisan berarti fitur belum selesai meskipun unit test lulus.

---

## 25. Arsitektur Teknologi Wajib

```text
Bahasa:
C#

Web UI:
ASP.NET Core MVC

Backend:
ASP.NET Core Web API

Arsitektur:
Clean Architecture

Database:
Microsoft SQL Server

ORM:
Entity Framework Core

Hosting:
IIS dan/atau Microsoft Azure

Repository:
Azure DevOps Git

CI/CD:
Azure DevOps Pipeline

Environment:
Development
QA/Testing
Production

Monitoring:
Application Insights
Application Performance Monitoring
```

Ketentuan:

- Web UI dan Web API dipisahkan pada deployment.
- Client tidak mengakses SQL Server langsung.
- Semua integrasi melalui API/service yang disetujui.
- Komponen interaktif .NET, UI framework, icon, font, dan library harus melalui architecture, security, dan license review.
- Static asset sebaiknya dibundel/self-hosted untuk aplikasi internal.
- Analytics publik, session recording, font CDN, atau runtime CDN tidak boleh dipakai tanpa approval.
- Dedicated C#/.NET Worker Service dapat digunakan untuk job dokumen setelah review kapasitas dan keamanan.

---

## 26. Pemantauan dan Dukungan

Monitoring minimum:

- API response time.
- Error rate.
- Job processing duration.
- Queue length.
- Upload/download failure.
- Database response time.
- CPU, memory, storage.
- Versi aplikasi.
- Correlation ID.
- Crash/error client yang disetujui.
- Availability dan health check.

Monitoring tidak boleh merekam isi dokumen atau data sensitif.

Support mencakup:

- General/Common Incident Guide.
- Post Go-Live Support.
- Jalur eskalasi L1/L2/L3.
- Service Desk knowledge.
- Kode error yang aman.
- Informasi aplikasi ke Service Desk.
- Ownership bisnis dan teknis.
- **In-App Issue Reporting** langsung dari PERTAMINA OTO.

### 26.1 Pelaporan Masalah dalam Aplikasi — Alur Pengguna

Pelaporan ketidaksesuaian harus **sangat ringan** dan tidak membebani user.

User hanya melihat:

```text
Laporkan Masalah

Pesan singkat:
[................................]

[ Screenshot ]   (opsional)

[ Kirim Laporan ]
```

Aturan UX:

- Tidak ada form panjang.
- User tidak diminta memilih kategori, prioritas, Feature ID, versi aplikasi, device, error code, assignment, atau status.
- Screenshot bersifat opsional.
- Tombol Screenshot mengambil tampilan aplikasi yang relevan bila platform mendukung.
- Screenshot wajib dipreview sebelum dikirim.
- User dapat memberi tanda sederhana pada screenshot, misalnya panah/lingkaran/highlight.
- Screenshot tidak boleh diunggah otomatis tanpa tindakan user.

Sistem otomatis menambahkan konteks teknis yang aman:

- User/account reference.
- Tanggal dan waktu.
- Page/menu/Feature ID aktif.
- Versi aplikasi.
- Platform/OS.
- Error Code bila ada.
- Job ID bila ada.
- Correlation ID bila ada.
- Metadata diagnostik aman sesuai policy.

Sistem tidak otomatis melampirkan isi dokumen, password, token, clipboard, signature asset, OCR text lengkap, atau secret.

### 26.2 Konsol Dukungan Superadmin

Laporan dikirim **melalui software PERTAMINA OTO**, bukan melalui WhatsApp.

Superadmin yang memiliki permission support mendapatkan halaman khusus untuk:

- Melihat daftar laporan.
- Membuka pesan dan screenshot.
- Melihat safe diagnostic context.
- Memberikan kategori/prioritas setelah laporan diterima.
- Mengubah status.
- Menambahkan catatan internal.
- Menandai duplicate/bukan bug/butuh informasi tambahan.
- Menyiapkan laporan perbaikan untuk programmer.
- Export detail satu issue ke PDF.
- Export rekap issue ke Excel.

Status internal minimum:

```text
BARU
DIPERIKSA
PERLU_PERBAIKAN
BUTUH_INFORMASI
DUPLIKAT
BUKAN_BUG
SEDANG_DIPERBAIKI
READY_FOR_TEST
TERVERIFIKASI
SELESAI
```

Status yang ditampilkan ke user boleh disederhanakan menjadi:

```text
Dikirim
Sedang diperiksa
Sedang ditindaklanjuti
Selesai
```

User tidak perlu melakukan administrasi ticket. Beban triage berada pada Superadmin/Support.


## 27. Dokumentasi dan Serah Terima

Dokumen proyek:

1. `01_BUSINESS_PROCESS.md`
2. `02_REQUIREMENT_SPECIFICATION.md`
3. `03_FEATURE_CATALOG.md`
4. `04_SOLUTION_DESIGN.md`
5. `05_TECHNICAL_REFERENCE.md`
6. `06_API_DOCUMENTATION.md`
7. `07_DATABASE_DESIGN.md`
8. `08_SECURITY_DESIGN.md`
9. `09_ROLE_MATRIX.md`
10. `10_AUDIT_TRAIL_DESIGN.md`
11. `11_UNIT_TEST_DOCUMENT.md`
12. `12_TEST_PLAN.md`
13. `13_UAT_DOCUMENT.md`
14. `14_USER_MANUAL.md`
15. `15_ADMIN_MANUAL.md`
16. `16_INCIDENT_SUPPORT_GUIDE.md`
17. `17_DEPLOYMENT_GUIDE.md`
18. `18_APPLICATION_CARTOGRAPHY.md`
19. `19_HANDOVER_CHECKLIST.md`

Proses handover:

- Source code tersedia di Azure DevOps.
- Monitoring terpasang.
- Knowledge sharing bisnis dan teknis.
- Side-by-side maintenance minimal satu siklus proses bisnis.
- Handover authorization.
- Service Desk update.
- Verifikasi backup, restore, rollback, dan recovery.
- Penetapan owner bisnis, owner teknis, L2, dan L3.
- Pemeriksaan dokumentasi dan akses.

---

## 28. Indikator Keberhasilan

### 28.1 Kemudahan

- User baru dapat menjalankan tool dasar tanpa panduan panjang.
- Tool utama ditemukan maksimal dalam dua interaksi.
- File dapat dimasukkan melalui pilih, drop, paste, share, atau scan.
- Tidak ada konfirmasi yang tidak perlu.
- User selalu mengetahui langkah berikutnya.
- User dapat memilih File-First, Tool-First, atau Assistant-First tanpa kehilangan akses ke fitur resmi.
- Assistant tidak mengarang fitur/parameter dan memberikan rencana yang dapat dipahami pada workflow multi-step.
- Alur mobile dan desktop dinilai berdasarkan UX yang sesuai capability masing-masing, bukan kesamaan tampilan semata.

### 28.2 Performa

- Halaman utama tampil cepat pada spesifikasi yang disetujui.
- Interaksi UI terasa langsung.
- Thumbnail menggunakan lazy loading.
- Sinkronisasi audit tidak menghambat UI.
- Proses panjang selalu menampilkan progress.
- Target rinci ditentukan dalam Requirement Specification dan Test Plan.
- Beban server, storage, bandwidth, database, queue, dan concurrency berada dalam budget yang disetujui.

### 28.3 Keandalan

- File asli tidak rusak.
- Draft dapat dipulihkan.
- Proses dapat di-retry atau dilanjutkan bila didukung.
- Hasil divalidasi.
- Error memberi solusi yang jelas.
- Crash-free dan success-rate ditentukan pada NFR.

### 28.4 Keamanan dan Kepatuhan

- File tidak dikirim tanpa pemberitahuan.
- Audit hanya metadata.
- Cache dan token dikelola aman.
- Role dan permission diterapkan.
- VA/PT dan security review selesai sebelum Production.
- Dokumentasi dan handover lengkap.

---

## 29. Batasan dan Asumsi

- Aplikasi digunakan untuk kebutuhan internal Pertamina.
- Semua teknologi mengikuti standar dan approval resmi.
- Windows Desktop C#/.NET menjadi primary full-feature productivity client setelah approval arsitektur/security.
- Responsive Web tetap menjadi enterprise/browser channel, bukan target feature-completeness utama.
- Mobile/Tablet companion app direncanakan untuk curated feature set dan tetap tunduk pada approval arsitektur/security.
- Fitur individual lengkap dikelola dalam Feature Catalog.
- Akurasi converter, OCR, dan AI bergantung pada library yang disetujui.
- Library komersial dapat memerlukan biaya lisensi.
- Ukuran file, jumlah user, concurrency, dan SLA ditetapkan dalam dokumen requirement dan solution design.
- Persentase lokal/server adalah target desain dan dapat disesuaikan setelah benchmark serta security review.
- Digital Signature berbasis sertifikat/PKI berada di luar scope saat ini; yang termasuk scope adalah tanda tangan visual/paraf (non-PKI).
- PERTAMINA OTO Assistant bersifat Offline-First Hybrid; full local LLM bukan baseline wajib.
- Internal AI fallback hanya menggunakan service/model yang disetujui dan tidak boleh menjadi single point of failure untuk fitur lokal.
- Seluruh fitur dengan dependency library/engine berisiko tinggi harus lolos feasibility gate sebelum coding skala penuh.

---

## 30. Ringkasan Keputusan Final

```text
Nama:
PERTAMINA OTO — Office Tools Organizer

Jenis:
Aplikasi internal Pertamina

Web:
ASP.NET Core MVC Responsive
Enterprise/browser channel

Backend:
ASP.NET Core Web API
Clean Architecture

Database:
Microsoft SQL Server
Entity Framework Core

Desktop:
C#/.NET Windows Desktop
Primary full-feature productivity client
Subject to architecture approval

Mobile/Tablet:
Curated companion app
Selected practical/mobile-first features
C#/.NET subject to approval

Worker:
C#/.NET Worker Service

Navigasi:
Triple Entry Flow
File-First + Tool-First + Assistant-First

Primary Feature Surface:
33 Core PDF Tools
Primary Feature Set First
Common-Simple-First di dalam Core Set

Assistant:
Offline-First Hybrid Conversational Orchestrator
Local Intent Router + Canonical Tool Orchestration
Internal AI fallback hanya bila diperlukan

Pengolahan:
Offline-First
Local-First sesuai capability platform
Server internal hanya bila dibutuhkan

Keamanan:
Enterprise Security Simplified

Penyimpanan:
Performance Mode
Local-First Workspace

Sinkronisasi:
Selective Cross-Device Sync
melalui Amankan ke Akun
maksimal 3 versi

Komunikasi:
Smart Communication System

Riwayat dan audit:
Riwayat User + Audit Enterprise

Pengembangan:
Azure DevOps
Development–QA–Production

Monitoring:
Application Insights

Pelaporan masalah:
In-App Issue Reporting
pesan + screenshot opsional
safe diagnostics otomatis
Superadmin Support Console
export PDF / Excel

Definition of Done:
8 lapis — technical, business, UX,
security, server budget, recovery,
cross-platform, test/documentation

Handover:
Dokumentasi, knowledge sharing,
side-by-side maintenance, dan Service Desk
```

---

## 31. Ketertelusuran ke Dokumen Berikutnya

Business Process ini diterjemahkan menjadi:

```text
02_REQUIREMENT_SPECIFICATION.md
→ Functional, Non-Functional, Business Rule,
  Integration, Data, UI/UX, Security, Acceptance Criteria

03_FEATURE_CATALOG.md
→ Master seluruh fitur individual

04_SOLUTION_DESIGN.md
→ Arsitektur implementasi dan deployment
```

---

## 32. Status Dokumen

```text
Nama dokumen : 01_BUSINESS_PROCESS.md
Versi        : 3.4
Status       : FINAL
Progress     : 100%
```

Dokumen berikutnya:

```text
02_REQUIREMENT_SPECIFICATION.md
```
