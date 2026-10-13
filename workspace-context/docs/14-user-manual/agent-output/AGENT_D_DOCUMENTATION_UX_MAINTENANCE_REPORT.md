# AGENT D — DOCUMENTATION UX, SCREENSHOT, ACCESSIBILITY & MAINTENANCE REPORT

> **Task ID:** `DOC14-AGENT-D`  
> **Peran:** Senior Technical Documentation Architect; UX Content Designer; Accessibility Documentation Reviewer; Documentation Project Manager; Documentation QA Specialist; Senior Software Delivery Architect  
> **Status laporan:** `PRE-IMPLEMENTATION BASELINE` — rekomendasi specialist untuk integrasi ke Document 14; bukan master User Manual dan bukan manual release-ready  
> **Baseline yang tersedia:** Documents 01–13 FINAL; specialist reports digunakan sebagai supporting input  
> **Document 13 baseline:** FINAL v1.0 mencakup 50/50 topik, 33/33 Core Tools, dan 27 skenario Merge; pelaksanaan UAT belum diklaim  
> **Bahasa:** Indonesia `ID-1.0`  
> **Tanggal analisis:** 1 September 2026

Laporan ini merancang User Manual yang task-based, mudah dicari, bilingual-ready, accessibility-aware, terikat versi aplikasi, dan dapat dirawat oleh satu developer + Codex. Laporan tidak membuat screenshot, tidak mengarang tampilan, tidak menetapkan versi release yang belum ada, tidak mengubah UI/Feature Catalog, tidak menulis Admin Manual/Incident Guide/Deployment Guide, tidak mengedit master Document 14, dan tidak melakukan coding.

Baseline yang dipertahankan:

- 550 canonical Feature tetap menjadi baseline; Feature ID tidak diterjemahkan atau dibuat ulang;
- 33 Primary PDF Tool Cards menjadi indeks fitur utama;
- Windows Desktop adalah primary full-feature productivity client; Web dan Phone/Tablet mengikuti exposure/capability release;
- Bahasa Indonesia adalah bahasa utama dan fallback; English didukung;
- UI menggunakan key-based localization resources;
- Adaptive UX berarti langkah/platform dapat berbeda dengan business outcome yang tetap konsisten;
- Offline-First dan capability-aware Local-First harus dijelaskan tanpa menjanjikan semua fitur selalu lokal;
- Assistant adalah jalur alternatif dan tidak mempunyai kewenangan lebih tinggi;
- **Amankan ke Akun** adalah selective, explicit secured transfer/sync, bukan full cloud workspace;
- maksimal tiga versi terakhir berlaku untuk file yang diamankan ke akun;
- Sign PDF adalah visual signature/paraf non-PKI;
- AI hanya memakai provider local/enterprise-internal yang disetujui; public AI endpoint dilarang;
- UAT feedback menjadi sumber koreksi manual, tetapi technical green/UAT draft tidak membuktikan manual release-ready;
- manual tidak boleh membocorkan password, token, secret, document content, real signature, atau data Issue terbatas.

---

# 1. Manual information architecture

## 1.1 Prinsip arsitektur informasi

1. **Task-based, bukan module-internal.** Pengguna mencari tujuan seperti “Gabungkan PDF”, bukan nama service/engine.
2. **Dapat dibaca tidak berurutan.** Setiap guide berdiri sendiri dengan prasyarat, langkah, hasil, dan error guidance.
3. **Satu konsep, satu halaman utama.** Penjelasan Local Processing, Offline, atau Amankan ke Akun tidak disalin berbeda-beda ke setiap tool; guide menautkan konsep bersama.
4. **Platform-aware tanpa duplikasi penuh.** Langkah bersama ditulis sekali; variasi Desktop/Web/Phone/Tablet ditambahkan hanya bila benar-benar berbeda.
5. **Version-bound.** Setiap manual menyatakan versi aplikasi/platform yang berlaku tanpa mengarang versi yang belum dirilis.
6. **Canonical traceability.** Feature ID disimpan pada metadata guide; tidak harus tampil di body untuk user biasa.
7. **Progressive disclosure.** Langkah utama tampil lebih dahulu; limitation, detail support, dan konsep teknis aman ditempatkan setelahnya.
8. **Offline-help capable.** Bantuan dasar harus dapat dibundel untuk `FEAT-PROD-018 — Offline Help/User Guide` tanpa tergantung Assistant/server.
9. **No dead end.** Setiap error/limitation memberi tindakan aman berikutnya atau cross-reference yang tepat.
10. **Boundary-safe.** User Manual tidak mengambil alih Admin Manual, Incident Support Guide, atau Deployment Guide.

## 1.2 Struktur yang disarankan

```text
14 USER MANUAL
├── Tentang Manual Ini
│   ├── Berlaku untuk versi/platform
│   ├── Cara mencari bantuan
│   └── Arti label Catatan/Perhatian/Penting
├── Mulai Cepat
│   ├── Masuk ke PERTAMINA OTO
│   ├── Pilih cara memulai: File / Tool / Assistant
│   ├── Jalankan Merge PDF pertama
│   ├── Pantau pekerjaan di Activity Center
│   └── Simpan hasil atau Amankan ke Akun
├── Dasar Penggunaan
│   ├── Home dan pencarian fitur
│   ├── File-First
│   ├── Tool-First
│   ├── Assistant-First
│   ├── Memilih, drop, paste, share, atau scan file
│   ├── Preview, Working State, Undo/Redo
│   ├── Progress, cancel, retry, resume
│   ├── Simpan / Simpan sebagai File Baru
│   └── History, Recycle Bin, dan Activity Center
├── Fitur Utama
│   ├── Indeks 33 Primary PDF Tools
│   ├── Organize PDF
│   ├── Optimize PDF
│   ├── Convert to PDF
│   ├── Convert from PDF
│   ├── Edit PDF
│   ├── PDF Security
│   └── PDF Intelligence
├── Platform
│   ├── Windows Desktop
│   ├── Responsive Web
│   ├── Phone
│   └── Tablet
├── Assistant
│   ├── Mencari tool
│   ├── Meninjau rencana
│   ├── Workflow multi-step
│   ├── Offline/online behavior
│   └── Limitation dan safe confirmation
├── Penyimpanan & Sync
│   ├── Workspace lokal
│   ├── Autosave dan Checkpoint
│   ├── History vs file hasil
│   ├── Amankan ke Akun
│   ├── File yang Diamankan
│   ├── Maksimal tiga versi
│   └── Akses lintas perangkat
├── Masalah Umum
│   ├── Fitur tidak muncul
│   ├── File tidak dapat dibuka
│   ├── Koneksi putus
│   ├── Pekerjaan menunggu/gagal
│   ├── Hasil kedaluwarsa/tidak tersedia
│   ├── Kapasitas penyimpanan
│   └── Laporkan Masalah
├── FAQ
├── Glossary
└── Indeks
    ├── Indeks tool dan alias
    ├── Indeks tugas
    ├── Indeks platform
    └── Indeks error/recovery
```

## 1.3 Peta bab dan kebutuhan pengguna

| Bab | Pertanyaan pengguna | Bentuk konten utama | Sumber kebenaran |
|---|---|---|---|
| Mulai Cepat | “Bagaimana mulai bekerja?” | Short task flows | UI release + UAT |
| Dasar Penggunaan | “Bagaimana aplikasi bekerja?” | Concepts + common procedures | Docs 01–04 + UI/UAT |
| Fitur Utama | “Bagaimana menjalankan tool tertentu?” | 33 task guides | Feature Catalog + release registry + UAT |
| Platform | “Apakah langkah sama di perangkat saya?” | Delta guide per platform | Capability/exposure manifest + UAT |
| Assistant | “Bagaimana meminta bantuan atau workflow?” | Task + limitation | Requirements/Security/UAT |
| Penyimpanan & Sync | “Di mana file saya? Apa yang disinkronkan?” | Concept + lifecycle tasks | Requirements/Database/Security/UAT |
| Masalah Umum | “Apa yang harus saya lakukan ketika gagal?” | Symptom → cause → safe action | Error catalog + UAT + Support trends |
| FAQ | “Apa jawaban singkat untuk pertanyaan umum?” | Concise answers + deep link | Approved manual content |
| Glossary | “Apa arti istilah ini?” | Controlled definitions | Approved terminology base |

## 1.4 Template setiap guide

```text
Guide ID
Judul task berbasis kata kerja
Ringkasan tujuan
Berlaku untuk versi/platform/language
Feature ID dan Requirement ID (metadata/traceability)
Ketersediaan: offline / online / capability / permission
Prasyarat
Input yang didukung sesuai release
Langkah bernomor
Hasil yang diharapkan
Lokasi/status hasil
Warning sebelum tindakan berisiko
Catatan/Tips setelah langkah terkait
Jika terjadi masalah
Known limitation
Lihat juga
Screenshot IDs
UAT/Test references
Owner/last review/status
```

## 1.5 Boundary dengan dokumen lain

| Kebutuhan pembaca | Tindakan User Manual |
|---|---|
| Menggunakan fitur biasa | Dijelaskan penuh pada Document 14 |
| Mengelola account/permission/configuration/Support Console | Cross-reference ke Document 15 Admin Manual |
| Menangani incident, escalation, evidence preservation, atau recovery operasional mendalam | Cross-reference ke Document 16 Incident Support Guide |
| Deployment, infrastructure, rollback, secret injection | Cross-reference ke Document 17 Deployment Guide |

User Manual boleh memberi langkah user-level seperti **Coba lagi**, **Batalkan**, atau **Laporkan Masalah**, tetapi tidak menyalin runbook operasional/admin.

---

# 2. Writing style guide

## 2.1 Suara dan kalimat

- Gunakan Bahasa Indonesia yang sederhana, langsung, dan aktif.
- Gunakan kalimat pendek; pecah kalimat yang memuat lebih dari satu tindakan.
- Tulis dari sudut tujuan user, bukan implementasi.
- Gunakan “Anda” secara konsisten bila direct address diperlukan; jangan berganti antara “user”, “pengguna”, dan “Anda” dalam satu guide.
- Hindari ungkapan legal/keamanan absolut seperti “100% aman” atau “AI selalu benar”.
- Hindari jargon engine, Worker, API, schema, database, object storage, SignalR, atau provider kecuali detail tersebut membantu user/support.
- Jangan menjanjikan capability yang belum masuk release/exposure.

Contoh:

```text
Pilih **Simpan sebagai File Baru**.
```

Bukan:

```text
Pengguna kemudian dapat melakukan pemilihan terhadap opsi penyimpanan sebagai.
```

## 2.2 Langkah

1. Satu nomor berisi satu aksi utama.
2. Mulai dengan kata kerja: **Pilih**, **Buka**, **Masukkan**, **Tinjau**, **Konfirmasi**, **Simpan**.
3. Letakkan hasil langsung setelah langkah yang menyebabkannya.
4. Jangan menggabungkan Desktop dan Mobile dalam satu langkah bila control berbeda.
5. Langkah bersyarat memakai “Jika…”.
6. Jangan memakai fixed coordinate seperti “tombol ketiga di kanan”; gunakan label control.
7. Jangan menjadikan screenshot sebagai satu-satunya sumber langkah.

## 2.3 Pemformatan UI

| Elemen | Format | Contoh |
|---|---|---|
| Tombol/action | **Bold** | Pilih **Simpan**. |
| Menu path | Bold per label dengan `›` | Buka **Pengaturan › Bahasa**. |
| Tab/panel | **Bold** | Buka **Activity Center**. |
| Input user | Tanda kutip atau code bila literal | Ketik “gabung PDF”. |
| File contoh | Code | Pilih `contoh-kontrak.pdf`. |
| Error Code/ID | Code dan hanya bila perlu | Catat `Error Code` dan `Correlation ID`. |
| Shortcut | Code | Tekan `Ctrl+Z`. |

UI label harus sama persis dengan locale dan build yang didokumentasikan. Jika label belum final, gunakan placeholder `TBD_UI_LABEL`; jangan menebak.

## 2.4 Warning, note, dan hasil

- Letakkan **Perhatian/Penting** sebelum tindakan berisiko.
- Letakkan **Catatan/Tips** setelah langkah yang berkaitan.
- Letakkan **Hasil** setelah rangkaian langkah.
- Jangan menumpuk lebih dari satu label jika satu label yang lebih kuat sudah cukup.
- Warning menyebut risiko dan pencegahannya, bukan hanya “hati-hati”.

Contoh:

```text
PENTING — Redaksi bersifat permanen pada file hasil. Tinjau preview sebelum memilih **Selesaikan Redaksi**.
```

## 2.5 Error guidance

Gunakan pola:

```text
Apa yang terjadi
→ apakah file sumber/hasil tetap aman
→ tindakan berikutnya
→ kapan menggunakan Laporkan Masalah
→ Error Code/Correlation ID hanya bila berguna untuk support
```

Jangan menyalin stack trace, provider error, raw exception, path internal, request body, atau detail security-sensitive ke manual.

## 2.6 Istilah satu arti satu nama

- “Simpan” tidak disamakan dengan “Amankan ke Akun”.
- “History” tidak disebut backup atau Audit.
- “Working State” tidak disebut versi file.
- “Undo/Redo” tidak disebut versioning.
- “Visual Signature” tidak disebut digital signature certificate.
- “Redaction” tidak disebut sekadar menutup teks; hasil harus dipahami permanen.
- “Accepted/queued” tidak disebut selesai.

---

# 3. Terminology standard

## 3.1 Hirarki terminologi

```text
Final UI label pada exact release/locale
→ approved bilingual terminology base
→ canonical Feature Catalog name/ID
→ approved common-search aliases
```

- UI label final mengendalikan wording langkah dan screenshot.
- Feature ID mengendalikan traceability dan tidak diterjemahkan.
- Alias membantu pencarian; alias tidak mengganti nama canonical atau membuat Feature baru.
- Jika UI label bertentangan dengan requirement/business meaning, gunakan STOP protocol dan laporkan; jangan menyesuaikan manual untuk menyembunyikan defect UI.

## 3.2 Terminology table awal

Kolom UI Label di bawah adalah baseline/candidate dari source. Exact ID dan English label wajib diverifikasi terhadap resource/UI release candidate.

| Istilah Indonesia | English | UI Label baseline/candidate | Catatan |
|---|---|---|---|
| PERTAMINA OTO | PERTAMINA OTO | PERTAMINA OTO | Nama produk; tidak diterjemahkan |
| Mulai Cepat | Quick Start | Mulai Cepat / Quick Start | Label manual, bukan Feature |
| Beranda | Home | Home/label RC | Ikuti label actual locale |
| Pencarian Fitur | Feature Search | Search/label RC | Cari berdasarkan canonical name + alias |
| Fitur | Feature | Feature | Capability resmi dengan Feature ID canonical |
| File-First | File-First | File-First | Istilah arsitektur; body dapat menjelaskan “mulai dari file” |
| Tool-First | Tool-First | Tool-First | “mulai dari tool” |
| Assistant-First | Assistant-First | Assistant | Assistant tetap jalur alternatif |
| Activity Center | Activity Center | Activity Center | Pertahankan bila UI memakai istilah ini; definisikan di glossary |
| Pemrosesan Lokal | Local Processing | Label status RC | Tidak selalu tersedia di semua platform |
| Pemrosesan Server | Server Processing | Label status/notice RC | Digunakan bila perlu menjelaskan upload/koneksi |
| Offline | Offline | Offline | Bukan berarti semua fitur tersedia |
| Daring | Online | Online | Gunakan konsisten; label UI boleh “Online” sesuai RC |
| Memerlukan koneksi | Connection required | Label RC | Lebih user-friendly daripada detail server |
| Working State | Working State | Label RC/TBD | Draft edit aktif; bukan versi file |
| Autosave | Autosave | Autosave | Menyimpan working state sesuai capability |
| Checkpoint | Checkpoint | Detail recovery bila terlihat | Mungkin hanya terlihat saat recovery/support |
| Riwayat Pribadi | Personal History | History | Bukan Audit dan bukan backup |
| File yang Diamankan | Secured File | Secured Files/label RC | Hasil yang dipilih melalui Amankan ke Akun |
| Amankan ke Akun | Secure to Account | **Amankan ke Akun** / EN label TBC | Tindakan explicit selective sync; bukan full workspace sync |
| Versi | Version | Version | Maksimal tiga versi file yang diamankan |
| Simpan | Save | **Simpan** | Menyimpan ke target yang sudah dipilih bila behavior mendukung |
| Simpan sebagai File Baru | Save as New File | **Simpan sebagai File Baru** | Tidak disamakan dengan Amankan ke Akun |
| Batalkan | Cancel | **Batalkan** | Hanya bila operation aman untuk dibatalkan |
| Coba lagi | Retry | **Coba lagi** | Tidak menjamin membuat Job baru |
| Lanjutkan | Resume/Continue | **Lanjutkan** | Sesuai recovery state |
| Gabung PDF | Merge PDF | Merge PDF / localized label RC | Alias Indonesia untuk `FEAT-PDF-005` |
| Pisahkan PDF | Split PDF | Split PDF / localized label RC | Mencakup page-range sesuai card |
| Kecilkan PDF | Compress PDF | Compress PDF / localized label RC | Alias search, bukan rename canonical |
| OCR PDF | OCR PDF | OCR PDF | “Ambil tulisan” adalah alias; OCR searchable PDF harus dijelaskan |
| PDF/A | PDF/A | PDF/A | Format archival/conformance; jangan disebut PDF biasa |
| Tanda Tangan Visual | Visual Signature | Sign PDF / localized label RC | Non-PKI; tidak memakai certificate |
| Redaksi Permanen | Redaction | Redact PDF / localized label RC | Menghapus informasi secara permanen pada hasil, bukan overlay |
| Assistant | Assistant | PERTAMINA OTO Assistant | Orchestrator; bukan processing engine |
| AI Summarizer | AI Summarizer | AI Summarizer | Output perlu review; provider approved local/internal only |
| Laporkan Masalah | Report a Problem | **Laporkan Masalah** | Pesan singkat; screenshot opsional/explicit |
| Error Code | Error Code | Detail error/support | Identifier aman; tidak diterjemahkan |
| Job ID | Job ID | Detail Activity/support | Tampil hanya bila background Job ada |
| Correlation ID | Correlation ID | Detail support bila tersedia | Identifier penelusuran; bukan token/permission |

## 3.3 Termbase record

```text
Term ID
Indonesia preferred term
English preferred term
UI resource key(s)
Canonical Feature/Requirement references
Definition
Allowed aliases/search synonyms
Forbidden/ambiguous terms
Platform/locale notes
Owner/reviewer
Status and last reviewed build
```

Istilah resmi Pertamina, English translation owner, capitalization, punctuation, dan preferred UI labels memerlukan konfirmasi sebelum manual final.

---

# 4. Navigation/findability

## 4.1 Komponen findability

- daftar isi yang dapat diklik;
- heading berbasis tugas dan search-friendly;
- quick links dari landing page;
- indeks 33 Core Tools;
- indeks alias Indonesia/English;
- cross-reference “Lihat juga”;
- glossary;
- link dari error/help context ke guide terkait;
- link dari setiap tool guide ke platform delta dan masalah umum;
- stable anchor/content ID agar link tidak berubah karena judul diterjemahkan;
- offline help index untuk guide dasar.

## 4.2 Pola heading

Gunakan:

```text
Gabungkan beberapa PDF
Kecilkan ukuran PDF
Ambil tulisan dari hasil scan dengan OCR
Amankan hasil ke akun
Lanjutkan pekerjaan setelah koneksi kembali
```

Hindari:

```text
Fungsi PDF
Informasi lainnya
Prosedur lanjutan
Bab pemrosesan
```

## 4.3 Quick links landing page

```text
Mulai dengan file
Cari tool
Gunakan Assistant
Lihat Activity Center
Bekerja offline
Amankan hasil ke akun
Atasi file yang tidak dapat dibuka
Laporkan masalah
Indeks 33 tool utama
```

## 4.4 Indeks 33 Primary PDF Tools dan alias

| No | Kelompok | Canonical Primary Tool | Feature ID | Alias pencarian Indonesia | Alias pencarian English |
|---:|---|---|---|---|---|
| 1 | Organize PDF | Merge PDF | `FEAT-PDF-005` | gabung PDF; satukan PDF | combine PDFs; join PDFs |
| 2 | Organize PDF | Split PDF | `FEAT-PDF-006 / FEAT-PDF-007` | pisahkan PDF; pecah PDF; rentang halaman | split pages; page range |
| 3 | Organize PDF | Remove pages | `FEAT-PDF-013` | hapus halaman; buang halaman | delete pages |
| 4 | Organize PDF | Extract pages | `FEAT-PDF-014` | ambil halaman; ekspor halaman tertentu | save selected pages |
| 5 | Organize PDF | Organize PDF | `FEAT-PDF-010..016` | atur halaman; urut halaman; susun PDF | reorder pages; organize pages |
| 6 | Organize PDF | Scan to PDF | `FEAT-SCAN-014` | pindai ke PDF; foto dokumen | camera scan; document scan |
| 7 | Optimize PDF | Compress PDF | `FEAT-PDF-023` | kecilkan PDF; kurangi ukuran | reduce PDF size |
| 8 | Optimize PDF | Repair PDF | `FEAT-PDF-025 / FEAT-PDF-026` | perbaiki PDF; PDF rusak | fix damaged PDF |
| 9 | Optimize PDF | OCR PDF | `FEAT-OCR-004` | ambil tulisan; kenali teks; PDF dapat dicari | recognize text; searchable PDF |
| 10 | Convert to PDF | JPG to PDF | `FEAT-CONV-007` | gambar ke PDF; foto ke PDF | image to PDF; photo to PDF |
| 11 | Convert to PDF | Word to PDF | `FEAT-CONV-001` | dokumen Word ke PDF; DOCX ke PDF | DOCX to PDF |
| 12 | Convert to PDF | PowerPoint to PDF | `FEAT-CONV-003` | PPT ke PDF; slide ke PDF | PPTX to PDF; slides to PDF |
| 13 | Convert to PDF | Excel to PDF | `FEAT-CONV-002` | XLSX ke PDF; lembar kerja ke PDF | spreadsheet to PDF |
| 14 | Convert to PDF | HTML to PDF | `FEAT-CONV-017` | halaman web ke PDF; HTML jadi PDF | web page to PDF |
| 15 | Convert from PDF | PDF to JPG | `FEAT-CONV-012` | PDF ke gambar; halaman jadi JPG | PDF pages to images |
| 16 | Convert from PDF | PDF to Word | `FEAT-CONV-004` | PDF ke DOCX; ubah PDF ke Word | PDF to DOCX |
| 17 | Convert from PDF | PDF to PowerPoint | `FEAT-CONV-006` | PDF ke PPT; PDF ke slide | PDF to PPTX |
| 18 | Convert from PDF | PDF to Excel | `FEAT-CONV-005` | PDF ke XLSX; tabel PDF ke Excel | PDF tables to spreadsheet |
| 19 | Convert from PDF | PDF to PDF/A | `FEAT-PDF-075` | PDF arsip; ubah ke PDF/A | archival PDF; PDF/A conversion |
| 20 | Edit PDF | Rotate PDF | `FEAT-PDF-012` | putar PDF; putar halaman | rotate pages |
| 21 | Edit PDF | Add page numbers | `FEAT-PDF-037` | tambah nomor halaman; penomoran PDF | number PDF pages |
| 22 | Edit PDF | Add watermark | `FEAT-PDF-036` | tambah watermark; cap air | watermark PDF |
| 23 | Edit PDF | Crop PDF | `FEAT-PDF-019` | potong PDF; crop halaman | trim page area |
| 24 | Edit PDF | Edit PDF | `FEAT-PDF-031..041` | ubah teks PDF; edit isi PDF | edit PDF content |
| 25 | Edit PDF | PDF Forms | `FEAT-PDF-053..060` | formulir PDF; isi form PDF | fill PDF form; form fields |
| 26 | PDF Security | Unlock PDF | `FEAT-PDF-062` | buka kunci PDF; buka PDF berpassword | open protected PDF | 
| 27 | PDF Security | Protect PDF | `FEAT-PDF-061` | beri password; lindungi PDF | password protect PDF |
| 28 | PDF Security | Sign PDF | `FEAT-SIGN-013` | tanda tangan visual; paraf PDF | visual signature; initial PDF |
| 29 | PDF Security | Redact PDF | `FEAT-PDF-069` | redaksi permanen; hapus informasi | permanently remove information |
| 30 | PDF Security | Compare PDF | `FEAT-PDF-071 / FEAT-PDF-072` | bandingkan PDF; cari perbedaan | PDF diff; compare versions |
| 31 | PDF Intelligence | AI Summarizer | `FEAT-INTEL-001` | ringkas PDF; rangkuman dokumen | summarize PDF |
| 32 | PDF Intelligence | Translate PDF | `FEAT-INTEL-002` | terjemahkan PDF; alih bahasa | PDF translation |
| 33 | PDF Intelligence | PDF to Markdown | `FEAT-CONV-036` | PDF ke Markdown; ekstrak Markdown | convert PDF to Markdown |

Alias harus diuji terhadap Feature Search dan Assistant intent, tetapi tidak semuanya harus ditampilkan sebagai UI label. “Unlock PDF” berarti membuka file dengan password/otorisasi yang sah, bukan melewati proteksi.

## 4.5 Cross-reference rules

- Gunakan teks bermakna: **Lihat cara melanjutkan pekerjaan setelah koneksi kembali**, bukan “klik di sini”.
- Tautkan ke heading/content ID stabil, bukan nomor halaman yang mudah berubah.
- Cross-reference platform hanya muncul jika langkah berbeda.
- Link ke Admin/Incident/Deployment guide diberi scope jelas agar user tidak masuk ke prosedur yang tidak berwenang.
- Link eksternal/vendor tidak dimasukkan tanpa approval, availability, security, dan maintenance owner.

---

# 5. Visual-label standard

## 5.1 Label semantik

Ikon/emoji berikut hanya drafting aid. Manual resmi harus tetap menampilkan kata label dan disesuaikan dengan branding/style guide Pertamina.

| Draft icon | Label Indonesia | English | Kegunaan | Posisi |
|---|---|---|---|---|
| ℹ️ | Catatan | Note | Informasi tambahan nonkritis | Setelah langkah terkait |
| ⚠️ | Perhatian | Caution | Risiko yang dapat dihindari | Sebelum tindakan |
| 🚨 | Penting | Important | Risiko tinggi, irreversible, security/data impact | Sebelum tindakan |
| ✅ | Hasil | Result | Outcome yang harus terlihat | Setelah langkah/rangkaian |
| 💡 | Tips | Tip | Cara lebih mudah/cepat, tidak wajib | Setelah langkah |
| 🌐 | Memerlukan koneksi | Connection required | Action/server/account dependency | Sebelum memulai |
| 📴 | Dapat digunakan offline | Available offline | Capability lokal pada scope/platform yang benar | Dekat prasyarat |
| 🔐 | Memerlukan izin | Permission required | Action hanya untuk permission tertentu | Dekat prasyarat |

## 5.2 Aturan visual

1. Label selalu memuat teks; icon/color tidak pernah berdiri sendiri.
2. Satu jenis label mempunyai makna yang sama di seluruh manual dan locale.
3. `Penting` tidak digunakan untuk tips agar hierarchy tidak kehilangan arti.
4. Warna, icon, border, dan capitalization mengikuti style guide yang disetujui; tidak ditetapkan laporan ini.
5. Screen reader/alt text tidak perlu membacakan emoji jika label teks sudah tersedia.
6. English set menggunakan label English yang setara, bukan campuran dua bahasa.
7. Draft emoji dihapus/diganti jika branding Pertamina tidak mengizinkan.

## 5.3 Contoh copy

```text
MEMERLUKAN KONEKSI — Anda harus online untuk memilih **Amankan ke Akun**.

PENTING — Sign PDF menambahkan tanda tangan atau paraf visual. Fitur ini bukan digital signature berbasis sertifikat/PKI.

HASIL — File baru muncul di lokasi yang Anda pilih. File sumber tidak berubah.
```

---

# 6. Screenshot register

## 6.1 Field register

```text
Screenshot ID
Chapter/Guide ID
Platform
Language
Build Version
Screen
Purpose
Data Type
Masking Required?
Status
Owner
Capture date
Artifact path/digest
Alt text
Caption
Last reviewed build
Replacement Screenshot ID
```

Field tambahan terakhir direkomendasikan untuk traceability/maintenance; field minimum prompt tetap dipertahankan.

## 6.2 Status lifecycle

```text
PLANNED
→ CAPTURED
→ REVIEWED
→ APPROVED
```

Ketika UI/build/locale/data/security context berubah:

```text
APPROVED → OUTDATED → replacement PLANNED
```

`OUTDATED` tidak dihapus sebelum link/riwayat diganti, tetapi tidak boleh tampil pada manual release aktif.

## 6.3 Register layar umum awal

Tidak ada screenshot yang dibuat pada laporan ini. Semua row memakai `PLANNED`, build `RC-TBD`, dan owner `Documentation Owner — TBC`.

| Screenshot ID | Chapter | Platform | Language | Build Version | Screen | Purpose | Data Type | Masking? | Status | Owner |
|---|---|---|---|---|---|---|---|---|---|---|
| `SS-QS-001-ID` | Mulai Cepat | Desktop | ID | `RC-TBD` | Home | Menunjukkan tiga entry flow | Dummy files/account | REVIEW | PLANNED | TBC |
| `SS-QS-002-ID` | Mulai Cepat | Desktop | ID | `RC-TBD` | File-First | Memilih file dan action | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-QS-003-ID` | Mulai Cepat | Desktop | ID | `RC-TBD` | Tool-First | Menemukan Merge PDF | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-QS-004-ID` | Mulai Cepat | Desktop | ID | `RC-TBD` | Assistant plan | Meninjau workflow sebelum run | Dummy intent/files | REVIEW | PLANNED | TBC |
| `SS-BAS-001-ID` | Dasar | Desktop | ID | `RC-TBD` | Preview/Working State | Bedakan draft dan hasil final | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-BAS-002-ID` | Dasar | Desktop | ID | `RC-TBD` | Save options | Simpan vs Simpan sebagai File Baru | Dummy path/file | YES | PLANNED | TBC |
| `SS-ACT-001-ID` | Dasar | Desktop | ID | `RC-TBD` | Activity Center | Status/progress/cancel/retry | Dummy Job | REVIEW | PLANNED | TBC |
| `SS-ACT-002-ID` | Masalah Umum | Web | ID | `RC-TBD` | Waiting connection | Menjelaskan next action | Dummy Job | REVIEW | PLANNED | TBC |
| `SS-STO-001-ID` | Penyimpanan & Sync | Desktop | ID | `RC-TBD` | Amankan ke Akun | Notice/explicit action | Dummy result/account | YES | PLANNED | TBC |
| `SS-STO-002-ID` | Penyimpanan & Sync | Web | ID | `RC-TBD` | Secured Files | Menampilkan tiga versi | Dummy account/files | YES | PLANNED | TBC |
| `SS-PLT-001-ID` | Platform | Phone | ID | `RC-TBD` | Camera Scan | Capture/crop/reorder | Dummy document | REVIEW | PLANNED | TBC |
| `SS-PLT-002-ID` | Platform | Tablet | ID | `RC-TBD` | Visual Sign | Touch/stylus preview | Synthetic mark | YES | PLANNED | TBC |
| `SS-PLT-003-ID` | Platform | Web | ID | `RC-TBD` | Upload notice | Menjelaskan transfer/koneksi | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-AST-001-ID` | Assistant | Desktop | ID | `RC-TBD` | Plan preview | File/output/server/risk summary | Dummy workflow | REVIEW | PLANNED | TBC |
| `SS-AST-002-ID` | Assistant | Phone | ID | `RC-TBD` | Offline limitation | Menjelaskan capability unavailable | Dummy intent | REVIEW | PLANNED | TBC |
| `SS-ISS-001-ID` | Masalah Umum | Desktop | ID | `RC-TBD` | Laporkan Masalah | Pesan + screenshot opsional | Dummy issue/account | YES | PLANNED | TBC |
| `SS-ISS-002-ID` | Masalah Umum | Desktop | ID | `RC-TBD` | Screenshot preview | Preview/markup/remove sebelum kirim | Dummy issue | YES | PLANNED | TBC |
| `SS-L10N-001-EN` | Localization QA | Desktop | EN | `RC-TBD` | Home EN | Memvalidasi label English | Dummy files/account | REVIEW | PLANNED | TBC |
| `SS-ACC-001-ID` | Accessibility QA | Web | ID | `RC-TBD` | Zoom/reflow | Contoh layout pada zoom release | Dummy data | REVIEW | PLANNED | TBC |
| `SS-ERR-001-ID` | Masalah Umum | Desktop | ID | `RC-TBD` | Safe error detail | Next action + Error/Correlation ID | Dummy IDs | YES | PLANNED | TBC |

## 6.4 Register 33 Core Tool screenshot plan

Satu screenshot per tool bukan kewajiban dekoratif. Row berikut merencanakan satu visual utama hanya jika screenshot benar-benar membantu membedakan input/parameter/preview/result. Setelah RC, reviewer boleh menandai `N/A — text sufficient` melalui decision log; jangan membuat screenshot tanpa nilai.

| Screenshot ID | Tool/Feature | Platform baseline | Language | Build | Purpose | Data | Masking? | Status | Owner |
|---|---|---|---|---|---|---|---|---|---|
| `SS-CORE-001-ID` | Merge PDF — `FEAT-PDF-005` | Desktop | ID | `RC-TBD` | Input order + process action | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-CORE-002-ID` | Split PDF — `FEAT-PDF-006/007` | Desktop | ID | `RC-TBD` | Range selection | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-CORE-003-ID` | Remove pages — `FEAT-PDF-013` | Desktop | ID | `RC-TBD` | Selected pages + warning | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-CORE-004-ID` | Extract pages — `FEAT-PDF-014` | Desktop | ID | `RC-TBD` | Page selection + output | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-CORE-005-ID` | Organize PDF — `FEAT-PDF-010..016` | Desktop | ID | `RC-TBD` | Reorder/preview | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-CORE-006-ID` | Scan to PDF — `FEAT-SCAN-014` | Phone | ID | `RC-TBD` | Capture/crop/reorder | Dummy document | REVIEW | PLANNED | TBC |
| `SS-CORE-007-ID` | Compress PDF — `FEAT-PDF-023` | Desktop | ID | `RC-TBD` | Quality/size choice | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-CORE-008-ID` | Repair PDF — `FEAT-PDF-025/026` | Desktop | ID | `RC-TBD` | Validation/repair outcome | Corrupt dummy PDF | REVIEW | PLANNED | TBC |
| `SS-CORE-009-ID` | OCR PDF — `FEAT-OCR-004` | Desktop | ID | `RC-TBD` | Language/scope/preview | Dummy scan | REVIEW | PLANNED | TBC |
| `SS-CORE-010-ID` | JPG to PDF — `FEAT-CONV-007` | Desktop | ID | `RC-TBD` | Image order/page options | Dummy images | REVIEW | PLANNED | TBC |
| `SS-CORE-011-ID` | Word to PDF — `FEAT-CONV-001` | Desktop | ID | `RC-TBD` | Input/options/limitation | Dummy DOCX | REVIEW | PLANNED | TBC |
| `SS-CORE-012-ID` | PowerPoint to PDF — `FEAT-CONV-003` | Desktop | ID | `RC-TBD` | Slide conversion scope | Dummy PPTX | REVIEW | PLANNED | TBC |
| `SS-CORE-013-ID` | Excel to PDF — `FEAT-CONV-002` | Desktop | ID | `RC-TBD` | Sheet/range selection | Dummy XLSX | REVIEW | PLANNED | TBC |
| `SS-CORE-014-ID` | HTML to PDF — `FEAT-CONV-017` | Release scope TBC | ID | `RC-TBD` | Approved input/notice | Dummy HTML | REVIEW | PLANNED | TBC |
| `SS-CORE-015-ID` | PDF to JPG — `FEAT-CONV-012` | Desktop | ID | `RC-TBD` | Page/image options | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-CORE-016-ID` | PDF to Word — `FEAT-CONV-004` | Desktop | ID | `RC-TBD` | Conversion/limitation | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-CORE-017-ID` | PDF to PowerPoint — `FEAT-CONV-006` | Desktop | ID | `RC-TBD` | Conversion/limitation | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-CORE-018-ID` | PDF to Excel — `FEAT-CONV-005` | Desktop | ID | `RC-TBD` | Table/sheet result | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-CORE-019-ID` | PDF to PDF/A — `FEAT-PDF-075` | Desktop | ID | `RC-TBD` | Conformance result/limitation | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-CORE-020-ID` | Rotate PDF — `FEAT-PDF-012` | Desktop | ID | `RC-TBD` | Page rotation preview | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-CORE-021-ID` | Add page numbers — `FEAT-PDF-037` | Desktop | ID | `RC-TBD` | Range/start/position | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-CORE-022-ID` | Add watermark — `FEAT-PDF-036` | Desktop | ID | `RC-TBD` | Text/image/opacity preview | Dummy PDF/mark | REVIEW | PLANNED | TBC |
| `SS-CORE-023-ID` | Crop PDF — `FEAT-PDF-019` | Desktop | ID | `RC-TBD` | Crop bounds preview | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-CORE-024-ID` | Edit PDF — `FEAT-PDF-031..041` | Desktop | ID | `RC-TBD` | Supported edit + limitation | Dummy PDF | REVIEW | PLANNED | TBC |
| `SS-CORE-025-ID` | PDF Forms — `FEAT-PDF-053..060` | Desktop | ID | `RC-TBD` | Field fill/validation | Dummy form | REVIEW | PLANNED | TBC |
| `SS-CORE-026-ID` | Unlock PDF — `FEAT-PDF-062` | Desktop | ID | `RC-TBD` | Password prompt/safe retry | Dummy protected PDF | YES | PLANNED | TBC |
| `SS-CORE-027-ID` | Protect PDF — `FEAT-PDF-061` | Desktop | ID | `RC-TBD` | Protection options/warning | Dummy PDF | YES | PLANNED | TBC |
| `SS-CORE-028-ID` | Sign PDF — `FEAT-SIGN-013` | Phone/Tablet | ID | `RC-TBD` | Visual non-PKI preview | Synthetic signature mark | YES | PLANNED | TBC |
| `SS-CORE-029-ID` | Redact PDF — `FEAT-PDF-069` | Desktop | ID | `RC-TBD` | Mark + permanent finalization | Dummy sensitive-like text | YES | PLANNED | TBC |
| `SS-CORE-030-ID` | Compare PDF — `FEAT-PDF-071/072` | Desktop | ID | `RC-TBD` | Differences view | Dummy versions | REVIEW | PLANNED | TBC |
| `SS-CORE-031-ID` | AI Summarizer — `FEAT-INTEL-001` | Release scope TBC | ID | `RC-TBD` | Scope/result/AI limitation | Dummy corpus | YES | PLANNED | TBC |
| `SS-CORE-032-ID` | Translate PDF — `FEAT-INTEL-002` | Release scope TBC | ID | `RC-TBD` | Language/result review | Dummy bilingual corpus | YES | PLANNED | TBC |
| `SS-CORE-033-ID` | PDF to Markdown — `FEAT-CONV-036` | Desktop | ID | `RC-TBD` | Extraction/preview | Dummy PDF | REVIEW | PLANNED | TBC |

English screenshots use separate IDs such as `SS-CORE-001-EN`; localized text may not be composited over an Indonesian capture.

---

# 7. Screenshot security rules

## 7.1 Data yang dilarang

Screenshot tidak boleh menampilkan:

- password atau password hint nyata;
- token, cookie, secret, key, connection string, QR login, atau session detail;
- dokumen/file internal sensitif;
- nama user/employee/account nyata tanpa kebutuhan dan izin;
- data internal rahasia atau classification marking yang tidak diizinkan;
- visual signature/paraf nyata;
- Issue message/screenshot/lampiran terbatas;
- raw log, stack trace, SQL, storage path/key, provider endpoint, atau claim dump;
- browser tabs, notification, taskbar, recent list, clipboard, atau background window yang tidak relevan.

## 7.2 Data capture yang digunakan

```text
dummy PDF
dummy account
dummy Issue
dummy filename/path
synthetic visual-signature mark
synthetic Job ID/Correlation ID/Error Code
sanitized output sample
```

Dummy data tetap diperiksa; nama dummy tidak boleh menyerupai identitas nyata atau memuat informasi operasional.

## 7.3 Capture workflow

```text
1. Verify exact RC build/platform/locale/theme/viewport
2. Load approved dummy/sanitized Dataset ID
3. Close unrelated apps/tabs/notifications/recent items
4. Navigate through actual application UI
5. Capture minimum relevant area
6. Inspect visible content and image metadata
7. Crop/redact only what is not part of expected UI truth
8. Record Screenshot ID/build/platform/locale/data/status
9. Documentation + security/privacy review
10. Approve, store, and link by controlled path/digest
```

## 7.4 Integrity rules

- Jangan membuat mockup atau gambar hasil editing lalu menyebutnya screenshot aplikasi.
- Cropping/redaction tidak boleh mengubah state, label, error, result, atau business meaning.
- Jika diperlukan ilustrasi konseptual, beri label **Ilustrasi**, bukan screenshot, dan minta approval style; laporan ini tidak membuatnya.
- Screenshot failure/old build tidak digunakan untuk guide normal tanpa caption yang menjelaskan konteks.
- Original restricted capture, bila memang harus dipertahankan, disimpan terpisah dengan access/retention policy; derivative manual tidak mewarisi akses publik.
- File screenshot harus mempunyai owner, digest/reference, dan last-reviewed build.
- Screenshot `OUTDATED` tidak boleh ikut package release aktif.

## 7.5 Issue screenshot boundary

Screenshot untuk menjelaskan **Laporkan Masalah** harus memakai dummy Issue. Product flow tetap mewajibkan preview dan explicit user send. Manual tidak menyalin screenshot Issue aktual ke documentation evidence tanpa purpose, authorization, minimization, dan secure storage.

---

# 8. Localization strategy

## 8.1 Model bahasa

- Manual utama: Bahasa Indonesia.
- English: dokumen terpisah atau translation set dari sumber konten yang sama.
- Fallback produk/manual offline: Bahasa Indonesia.
- Feature ID, Requirement ID, Job ID, Correlation ID, Error Code, filename, dan user content tidak diterjemahkan.
- UI label, error, tooltip, progress, notification, dan accessibility text harus cocok dengan locale aplikasi.

Rekomendasi: satu struktur/content source dengan stable Content ID, dua locale output, dan screenshot terpisah per locale. Hindari dua manual yang di-copy lalu berkembang sendiri.

## 8.2 Localization workflow

```text
Approve Indonesian source + termbase
→ extract translatable content
→ translate to English
→ terminology/UI-resource check
→ linguistic review
→ functional review against English UI
→ screenshot capture per locale
→ link/layout/accessibility QA
→ UAT cross-check
→ release together or record locale limitation
```

## 8.3 Yang tidak diterjemahkan

```text
canonical Feature ID
Requirement/Test/UAT ID
Job ID / Correlation ID / Error Code
permission/policy/resource/event code bila tampil pada detail berwenang
API/contract/schema version
filename, folder name, preset name, atau content buatan user
PDF, PDF/A, OCR, JPG, HTML, Markdown
PERTAMINA OTO
```

Nama format dapat memiliki penjelasan lokal, tetapi literal format tidak berubah.

## 8.4 UI label synchronization

Setiap step mempunyai metadata:

```text
UI resource key
ID label at build
EN label at build
platform
manual content ID
screenshot ID
last verified build
```

Missing resource key, raw key/enum, mixed language yang merusak pemahaman, atau label manual yang tidak cocok UI adalah defect documentation/product localization sesuai source masalahnya.

## 8.5 Locale-specific QA

- Teks English/Indonesia panjang tidak terpotong pada manual layout.
- Tanggal, waktu, timezone, angka, ukuran, durasi, page count, dan persen mengikuti locale tanpa mengubah nilai.
- Search aliases diuji pada kedua bahasa.
- Alt text/caption ikut diterjemahkan dan tetap cocok dengan screenshot locale.
- Link target locale tidak berpindah diam-diam ke bahasa lain kecuali fallback dinyatakan.
- Assistant instruction/manual tidak menjanjikan intent/Feature yang tidak didukung locale/build.

Machine translation boleh membantu draft, tetapi tidak menjadi approval. Istilah security, legal, data lifecycle, visual signature, redaction, dan AI limitation membutuhkan human review.

---

# 9. Accessibility guidance

Manual harus dapat dipakai oleh pembaca dengan cara baca, ukuran layar, zoom, atau assistive technology yang berbeda. Laporan ini tidak mengklaim certification/conformance standard tertentu sebelum target resmi dan format publikasi ditetapkan.

## 9.1 Struktur

- Satu heading level 1 untuk judul dokumen.
- Heading turun berurutan; jangan melompat hanya demi ukuran visual.
- Judul heading menjelaskan task/topic tanpa perlu context sebelumnya.
- Daftar langkah menggunakan numbered list.
- Daftar non-urutan menggunakan bullet list.
- Paragraf pendek; satu gagasan utama per paragraf.

## 9.2 Screenshot

- Setiap screenshot mempunyai alt text dan caption.
- Alt text menjelaskan tujuan/state yang relevan, bukan seluruh pixel atau “gambar layar”.
- Informasi penting pada screenshot juga tersedia sebagai teks langkah.
- Jangan mengandalkan warna, garis, lingkaran, atau arrow markup saja.
- Caption menyebut platform/locale bila relevan.

Template alt text:

```text
[Screen] pada [platform] yang menunjukkan [state/action utama] dan [informasi penting].
```

Contoh draft:

```text
Activity Center pada Windows Desktop yang menunjukkan satu pekerjaan menunggu koneksi dan tombol Coba lagi.
```

## 9.3 Link dan navigation

- Teks link menyebut destination/purpose.
- Hindari “klik di sini”, “selengkapnya”, atau URL mentah sebagai satu-satunya teks.
- Daftar isi dan “Lihat juga” dapat dinavigasi keyboard pada format digital yang dipilih.
- Anchor stabil dan unik per locale/content ID.
- External link diberi konteks dan owner.

## 9.4 Tabel

- Hindari tabel sangat lebar untuk langkah pengguna.
- Pecah tabel per platform/topic jika horizontal reading menjadi berat.
- Sediakan heading row yang jelas.
- Jangan memakai merged cell kompleks bila format target menyulitkan screen reader.
- Informasi kritis tidak disampaikan hanya dengan simbol/status color.

## 9.5 Bahasa dan arah

- Gunakan bahasa sederhana dan konsisten.
- Hindari referensi arah yang mudah berubah seperti “panel di kanan” kecuali platform/build-spesifik dan masih benar.
- Jelaskan icon dengan nama action, bukan bentuk semata.
- Acronym dijelaskan pada pemakaian pertama dan glossary.
- Error guidance menyatakan dampak dan next action.

## 9.6 Format publication

HTML/help bundle, PDF, atau format lain belum dipilih oleh laporan ini. Setelah format ditetapkan, QA harus memeriksa heading semantics, reading order, text alternatives, keyboard access, zoom/reflow, contrast/style, link behavior, language metadata, table semantics, dan export quality. Target standard resmi adalah `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

# 10. FAQ structure

## 10.1 Template FAQ

```text
Pertanyaan berbasis bahasa user
Jawaban singkat satu–tiga kalimat
Mengapa/penjelasan bila perlu
Langkah aman berikutnya
Lihat juga
Applies-to platform/version
Feature/Requirement/UAT trace metadata
Known limitation/last review
```

FAQ berasal dari UAT failure/questions, Support Issue trends, search query tanpa hasil, dan product changes. FAQ tidak digunakan untuk menambal defect kritis atau requirement yang belum diputuskan.

## 10.2 Draft FAQ minimum

### Mengapa fitur tidak muncul?

Fitur yang terlihat bergantung pada platform, versi aplikasi, capability perangkat, permission, policy, koneksi, dan release exposure. Periksa koneksi dan versi aplikasi, lalu buka kembali pencarian fitur. Jika fitur seharusnya tersedia tetapi tetap tidak muncul, pilih **Laporkan Masalah** tanpa mencoba direct route yang tidak berwenang.

### Mengapa file tidak dapat dibuka?

File mungkin tidak didukung, rusak, masih terkunci, berubah sejak dipilih, atau tidak lagi dapat diakses. Pilih file kembali dan ikuti pesan validasi. Jika file meminta password, masukkan password yang sah; PERTAMINA OTO tidak menyimpan password di manual, log, atau evidence.

### Apakah file dikirim ke server?

Tidak selalu. PERTAMINA OTO memprioritaskan pemrosesan lokal ketika capability dan policy mengizinkan. Jika suatu proses memerlukan server, aplikasi harus memberi pemberitahuan dan meminta persetujuan sebelum document content diunggah. Tool yang benar-benar local-only tidak menggunakan processing API server.

### Bagaimana bekerja offline?

Fitur yang eligible untuk pemrosesan lokal tetap dapat digunakan selama offline session policy mengizinkan. Fitur server/account, Amankan ke Akun, dan tindakan daring lain akan menunggu atau dinyatakan tidak tersedia. Setelah koneksi kembali, aplikasi memvalidasi ulang account, permission, policy, capability, dan compatibility sebelum melanjutkan.

### Bagaimana membatalkan proses?

Buka panel progress atau **Activity Center**, lalu pilih **Batalkan** jika action tersedia. Pembatalan tidak selalu tersedia setelah hasil final diterbitkan. Aplikasi harus menjaga file sumber dan tidak menganggap output parsial sebagai hasil final.

### Apa itu Amankan ke Akun?

**Amankan ke Akun** mengirim hasil yang Anda pilih secara eksplisit ke penyimpanan akun agar dapat diakses pada perangkat lain yang berwenang. Tindakan ini bukan autosync seluruh workspace. Draft, Working State, cache, temporary file, checkpoint utama, password, clipboard, dan aset tanda tangan visual tidak otomatis disinkronkan.

### Mengapa hanya tiga versi?

Baseline produk menyimpan maksimal tiga versi terakhir untuk file yang diamankan ke akun. Saat versi keempat dibuat, versi paling lama dihapus atau dipindahkan sesuai kebijakan retensi. Exact wording tentang pemindahan/penghapusan harus mengikuti policy yang berlaku pada release.

### Apakah Sign PDF merupakan digital signature certificate?

Tidak. Sign PDF pada scope saat ini menambahkan tanda tangan atau paraf visual non-PKI. Fitur ini tidak membuat digital signature berbasis certificate dan tidak boleh diklaim memberi validasi cryptographic/legal yang tidak tersedia.

### Apakah AI Summarizer selalu benar?

Tidak. Ringkasan dapat melewatkan konteks atau menghasilkan interpretasi yang perlu diperiksa. Tinjau kembali dokumen sumber sebelum mengambil keputusan. PERTAMINA OTO hanya boleh memakai provider local atau enterprise-internal yang disetujui; public AI endpoint tidak digunakan.

### Bagaimana melaporkan masalah?

Buka **Laporkan Masalah**, tulis pesan singkat, lalu tambahkan screenshot jika benar-benar membantu. Tinjau screenshot sebelum memilih **Kirim**. Screenshot bersifat opsional dan tidak boleh diunggah otomatis. Sistem dapat menambahkan metadata diagnostik aman seperti versi aplikasi, Error Code, Job ID, atau Correlation ID tanpa melampirkan document content atau secret.

---

# 11. Glossary

| Istilah | English | Definisi untuk user | Kapan terlihat |
|---|---|---|---|
| Activity Center | Activity Center | Tempat melihat pekerjaan yang berjalan, menunggu, selesai, gagal, dibatalkan, atau dapat dicoba lagi | UI utama/help |
| Assistant | Assistant | Jalur percakapan untuk mencari dan mengatur tool resmi; bukan engine pemrosesan dan tidak dapat melewati permission | UI/manual |
| Feature | Feature | Fungsi resmi PERTAMINA OTO yang mempunyai identitas dan aturan capability | Manual; Feature ID biasanya metadata |
| Pemrosesan Lokal | Local Processing | Pemrosesan pada perangkat ketika capability, resource, dan policy mengizinkan | Notice/status bila relevan |
| Pemrosesan Server | Server Processing | Pemrosesan melalui layanan internal yang disetujui ketika fungsi tidak layak/tersedia lokal | Notice/consent bila content ditransfer |
| Offline | Offline | Kondisi tanpa koneksi ke layanan server; hanya fungsi lokal yang eligible dapat berjalan | Status/help |
| Working State | Working State | Keadaan edit/draft aktif sebelum hasil final disimpan | Editor/recovery |
| Autosave | Autosave | Penyimpanan otomatis Working State sesuai capability agar pekerjaan dapat dipulihkan | Status/recovery |
| Checkpoint | Checkpoint | Titik pemulihan proses yang dapat digunakan untuk melanjutkan pekerjaan tertentu | Recovery/detail bila perlu |
| File yang Diamankan | Secured File | Hasil yang dipilih user melalui Amankan ke Akun dan tersimpan pada account scope yang berwenang | Secured Files |
| Amankan ke Akun | Secure to Account | Tindakan explicit untuk mengamankan hasil tertentu agar tersedia lintas perangkat; bukan full workspace sync | Action/help |
| OCR | Optical Character Recognition | Proses mengenali teks dari gambar/scan; hasil perlu ditinjau terutama pada scan buruk atau layout kompleks | Tool/help |
| PDF/A | PDF/A | Varian PDF untuk kebutuhan archival/conformance; hasil harus divalidasi oleh fungsi yang disetujui | Tool/help |
| Tanda Tangan Visual | Visual Signature | Gambar/drawing tanda tangan atau paraf yang ditempatkan pada PDF; non-PKI | Sign PDF/help |
| Redaksi Permanen | Redaction | Penghapusan informasi dari file hasil secara permanen melalui proses finalisasi; bukan sekadar kotak penutup | Redact PDF/warning |
| Correlation ID | Correlation ID | Identifier aman untuk menghubungkan satu alur/request dengan informasi support | Detail error/support bila tersedia |
| Job ID | Job ID | Identifier pekerjaan background tertentu | Activity/detail support |
| History | Personal History | Riwayat aktivitas pribadi; bukan backup file dan bukan Enterprise Audit | History/help |
| Preview | Preview | Tampilan pemeriksaan sebelum perubahan disimpan/final | Tool/editor |
| Hasil Final | Final Result | Output yang telah selesai dan tervalidasi untuk disimpan/diunduh/dibagikan | Completion state |
| Retry | Retry | Mencoba kembali tindakan yang gagal sesuai state yang aman | Error/Activity |
| Resume | Resume | Melanjutkan pekerjaan dari state/checkpoint yang didukung | Recovery/Activity |

`Correlation ID`, `Job ID`, Error Code, atau detail teknis hanya ditampilkan pada manual/user UI bila benar-benar membantu support dan tidak membuka data/otoritas tambahan.

---

# 12. Documentation testing

## 12.1 Test strategy

| Review/test | Tujuan | Evidence minimum | Gate |
|---|---|---|---|
| Task-Based Review | User menyelesaikan task dari manual | Case/result/observation | Critical task selesai tanpa penjelasan tambahan |
| New User Review | Menilai findability dan plain language | Search path, questions, task result | Tool utama ditemukan dan langkah dipahami |
| UAT Cross-Check | Manual cocok dengan accepted behavior | UAT ID, build, guide ID, disposition | No unresolved manual/product mismatch |
| UI Label Check | Tombol/menu/path cocok dengan locale UI | Resource key/build/platform diff | Tidak ada invented/outdated label |
| Screenshot Check | Visual cocok build/locale dan aman | Screenshot register/review | Semua used screenshot APPROVED/current |
| Link Check | Internal/external link valid | Machine + manual report | No broken/misdirected/restricted link |
| Feature Availability Check | Guide tidak menjanjikan capability yang tidak diekspos | Release capability matrix | Platform/version badges benar |
| Security Warning Check | Risk notice benar dan ditempatkan sebelum action | Security review reference | No missing/misleading warning |
| Localization Review | ID/EN meaning dan label setara | Termbase/resource/screenshot diff | No raw key/mixed-language meaning defect |
| Accessibility Review | Struktur, alt text, links, table, readability | Checklist + format inspection | No critical documentation access barrier |
| Version Compatibility Check | Manual berlaku untuk exact release range | App/manual compatibility manifest | No unsupported version claim |
| Offline Help Check | Guide dasar tersedia tanpa server | Offline package test | Core help/search/link works offline as scoped |

## 12.2 Documentation test case template

```text
Documentation Test ID
Guide/Content ID
Requirement/Feature/UAT references
build/platform/locale/manual version
review type and risk
participant profile or reviewer
precondition
task/question
allowed manual entry point
expected findability/outcome
expected UI labels/screenshots/warnings
actual result
PASS / FAIL / BLOCKED / NOT_RUN / INCONCLUSIVE / N/A
evidence
product defect or documentation defect link
owner/retest build
```

## 12.3 Task-based acceptance untuk fitur utama

Untuk setiap Primary Tool yang masuk release:

```text
1. Tester membuka manual dari Home/Search/Help context yang disetujui
2. Tester menemukan guide melalui canonical name atau common alias
3. Tester mengidentifikasi platform/koneksi/permission prerequisite
4. Tester menyelesaikan task memakai dummy/sanitized data
5. Tester mengenali hasil final dan lokasi/action berikutnya
6. Tester dapat menangani minimal satu safe error/recovery path
7. Tester tidak memerlukan penjelasan lisan tambahan
```

Jika tester hanya berhasil setelah developer menjelaskan UI, guide belum PASS meskipun aplikasi berfungsi.

## 12.4 Merge PDF documentation vertical slice

`FEAT-PDF-005 — Merge PDF` menjadi guide pertama yang divalidasi end-to-end:

- ditemukan melalui “Merge PDF”, “Gabung PDF”, dan alias yang disetujui;
- menjelaskan minimal dua input PDF, urutan file, validation, process, progress/background bila berlaku, cancel/recovery, hasil final, dan save action;
- mempertahankan Local-First/local-only baseline tanpa processing API server;
- menyebut Job ID hanya jika background local Job benar-benar ada;
- tidak mengklaim SignalR/server untuk baseline lokal;
- memakai dummy PDF dan screenshot RC yang disetujui;
- diuji pada platform release yang benar dan dalam ID/EN;
- ditautkan ke `UAT-MRG-001..027` dan acceptance Merge pada Document 13 FINAL.

## 12.5 Defect classification

| Temuan | Klasifikasi |
|---|---|
| Manual berbeda dari approved UI yang benar | Documentation defect |
| UI berbeda dari requirement/business rule | Product defect; jangan ubah manual untuk membenarkan UI |
| UI dan requirement ambigu/bertentangan | STOP + Integrator/product decision |
| Screenshot lama | Documentation maintenance defect; status OUTDATED |
| Feature tidak ada pada platform sesuai release | Documentation defect bila guide menjanjikan; product defect bila exposure menjanjikan |
| English meaning mengubah action/security | Localization defect; dapat menjadi product/manual defect |

## 12.6 UAT handoff

Documentation findings dari UAT harus memuat UAT ID, observed confusion/gap, actual UI label/flow, affected platform/locale, suggested content need, evidence, severity/priority, owner, dan target build. Mapping menggunakan UAT ID Document 13 FINAL dan wajib direkonsiliasi apabila controlled revision berubah.

---

# 13. Quality checklist

## 13.1 Checklist per guide

```text
[ ] tujuan user jelas pada judul/ringkasan
[ ] Requirement ID dan canonical Feature ID traceable pada metadata
[ ] applies-to version/platform/locale jelas
[ ] capability/offline/online/permission prerequisite jelas
[ ] input dan data prerequisite jelas
[ ] langkah lengkap, bernomor, satu langkah satu aksi
[ ] UI label dan menu path sama dengan exact build/locale
[ ] hasil final dan lokasi/action berikutnya jelas
[ ] warning muncul sebelum risiko
[ ] notes/tips muncul setelah langkah terkait
[ ] error/recovery guidance actionable
[ ] source/result/preview/working-state distinction benar
[ ] Local-First/server transfer wording benar
[ ] known limitation current dan tidak menyembunyikan blocked Feature
[ ] screenshot relevan, current, approved, dan aman atau dinyatakan tidak perlu
[ ] alt text dan caption tersedia
[ ] terminology konsisten dengan termbase
[ ] link mempunyai teks bermakna dan valid
[ ] tabel/layout dapat dibaca pada format target
[ ] tidak bergantung pada warna/icon/screenshot saja
[ ] Bahasa Indonesia dan English telah direview sesuai release scope
[ ] documentation test PASS
[ ] UAT cross-check PASS/dispositioned
[ ] owner/status/last-reviewed build tercatat
```

## 13.2 Checklist manual global

```text
[ ] daftar isi dan quick links berfungsi
[ ] search alias index mencakup 33 Core Tools
[ ] glossary mencakup istilah minimum
[ ] FAQ berasal dari actual risk/UAT/Support input
[ ] cross-platform deltas sesuai release capability matrix
[ ] tidak ada Admin/Incident/Deployment procedure yang diduplikasi
[ ] no password/token/secret/document/real-signature/private-Issue leakage
[ ] Sign PDF selalu visual non-PKI
[ ] AI limitation dan approved-provider boundary jelas
[ ] Amankan ke Akun tidak disebut full autosync/cloud workspace
[ ] maksimal tiga versi dan version-fourth behavior sesuai current policy wording
[ ] screenshot register tidak memuat OUTDATED asset pada active release
[ ] broken-link, missing-alt, untranslated-key, dan stale-version checks lulus
[ ] manual version/app compatibility mapping lengkap
[ ] UAT feedback resolved atau governed known limitation
[ ] release owner dan maintenance owner terdefinisi
```

## 13.3 Definition of Done integration

Manual merupakan bagian lapisan kedelapan Definition of Done. Guide belum selesai jika behavior, business outcome, UX, security, performance limitation, recovery, platform exposure, test evidence, atau documentation traceability belum konsisten. Dokumentasi tidak boleh dipakai untuk menurunkan acceptance fitur.

---

# 14. UI change process

## 14.1 Flow

```text
Feature/UI change proposed or detected
→ identify Requirement/Feature/resource and release scope
→ documentation impact analysis
→ update terminology/UI-resource mapping
→ update text and cross-references
→ capture/replace affected screenshots from actual RC
→ localization review
→ accessibility/security/documentation QA
→ UAT cross-check or affected task retest
→ approve manual change
→ release with compatible application build
```

## 14.2 Documentation impact record

```text
Change/Work Item ID
Requirement/Feature IDs
change type and reason
affected build/platform/locale
old/new UI resource key/label/flow
affected Guide/Content/Screenshot/FAQ/Glossary IDs
security/accessibility/localization impact
UAT/test impact
owner/reviewer
target manual/app version
status/evidence
```

## 14.3 Change classification

| UI/change | Manual action |
|---|---|
| Label/copy only | Update steps, termbase, search aliases, alt text, screenshot if visible |
| Control/menu path | Update steps/navigation/cross-links and recapture screenshot |
| Flow/order | Re-run task-based review and UAT cross-check |
| Permission/role visibility | Update prerequisites/platform guidance; verify no hidden-resource leak |
| Feature exposure/platform | Update applies-to matrix, index, FAQ, screenshots |
| Error/recovery | Update symptom/action/warning and test failure path |
| Security behavior/warning | Security review required; warning placed before action |
| Processing route/connectivity | Update Local/Server/Offline explanation and upload consent text |
| Compatibility/minimum version | Update applicability and old-version guidance |

## 14.4 Automation/Codex boundary

Codex dapat:

- mencari impacted guides berdasarkan Feature/resource/UI key;
- membuat draft text diff;
- memeriksa heading/link/term consistency;
- membandingkan screenshot register/build metadata;
- membuat test checklist dan change report.

Codex tidak boleh:

- membuat screenshot palsu;
- menebak label UI/route/version;
- mengubah expected behavior agar sesuai screenshot;
- menghapus warning/limitation agar manual terlihat sederhana;
- menyatakan screenshot/UI/UAT reviewed tanpa evidence manusia/actual build;
- mengubah Feature ID, Requirement ID, atau platform exposure.

---

# 15. Versioning

## 15.1 Manual version metadata

```text
Manual ID
Manual Version
Language/locale
Status
Applies From App Version
Applies Through App Version or current
Supported platforms/channels
Build/commit/artifact digest used for validation
UI resource/capability/compatibility version
Release date
Owner/reviewer
Known limitations
Supersedes/superseded-by
```

Jangan mengisi angka `x.y` atau tanggal release sampai release candidate dan version policy tersedia.

## 15.2 Status manual

| Status | Arti |
|---|---|
| `PRE-IMPLEMENTATION BASELINE` | Struktur, rules, templates, dan traceability disiapkan sebelum UI/RC |
| `DRAFT — WAITING FOR RELEASE CANDIDATE` | Content task tersedia tetapi label/screenshot/build belum final |
| `RELEASE CANDIDATE REVIEW` | Content dan screenshot terikat RC serta sedang diuji/review |
| `RELEASED` | Seluruh release-readiness criteria PASS untuk exact version/scope |
| `OUTDATED` | Tidak berlaku untuk current UI; tidak boleh menjadi default manual |
| `ARCHIVED` | Dipertahankan untuk supported old version/record sesuai policy |

## 15.3 Compatibility rules

- Manual aktif menyatakan “Berlaku untuk PERTAMINA OTO versi …” hanya setelah exact version disetujui.
- `SUPPORTED`, `UPDATE_AVAILABLE`, `UPDATE_REQUIRED`, dan `UNSUPPORTED` mengikuti compatibility contract aplikasi; manual tidak menciptakan status lain.
- Forced update guidance hanya mengikuti security/compatibility decision nyata.
- Manual versi lama tetap tersedia bila client lama masih supported dan behavior berbeda material.
- Jika satu manual mencakup rentang versi, differences harus kecil, eksplisit, dan diuji; jangan memakai rentang luas untuk menyembunyikan UI drift.
- Screenshot selalu menyimpan exact build version, bukan hanya manual version.
- Feature/platform availability berasal dari release registry/capability manifest, bukan inferensi manual.

## 15.4 Content identity

Stable `Guide ID`, `Content ID`, Term ID, dan Screenshot ID dipertahankan lintas perubahan judul/locale. ID dokumentasi tidak menjadi Feature ID dan tidak boleh dipakai untuk dispatch aplikasi.

---

# 16. Release readiness

## 16.1 Gate

Manual release-ready hanya jika:

```text
[ ] exact application version/build/digest and supported platform matrix recorded
[ ] UI labels and menu paths final for ID and EN
[ ] all in-scope guides have tested complete steps
[ ] 33 Core index matches release exposure and canonical Feature IDs
[ ] screenshots are actual RC captures, APPROVED, current, and privacy-safe
[ ] screenshot alt text/captions are complete
[ ] known limitations and error guidance are current
[ ] Local/Server/Offline/Sync/Assistant/Security wording approved
[ ] links, anchors, search aliases, TOC, and offline-help package validated
[ ] accessibility/document-structure review passed for selected format
[ ] terminology and localization reviews passed
[ ] UAT feedback resolved or formally dispositioned
[ ] Feature ID/UAT/manual traceability complete
[ ] no unresolved documentation Blocker/Critical
[ ] owner, reviewer, maintenance path, and release mechanism defined
[ ] Admin/Incident/Deployment boundaries cross-referenced without duplication
```

## 16.2 Current assessment

| Area | Current status | Reason |
|---|---|---|
| Information architecture | BASELINED | Struktur dan templates tersedia |
| Style/terminology | BASELINED WITH GAPS | UI label/English term owner belum final |
| 33-Core index | BASELINED | Canonical IDs cocok dengan Document 03 |
| UI steps | BLOCKED | Production code/RC/UI final belum tersedia |
| Screenshots | `PLANNED` | Tidak ada actual RC capture; screenshot palsu dilarang |
| Localization validation | BLOCKED | Final ID/EN resources/UI belum tersedia |
| Accessibility validation | BLOCKED | Publication format, UI/RC, dan standard target belum final |
| Documentation testing | BLOCKED | Exact manual/RC/UAT cycle belum tersedia |
| Document 13 traceability | BASELINED | FINAL v1.0 mencakup 50/50 topik, 33 Core Tools, dan 27 skenario Merge; pelaksanaan UAT belum dicatat |
| Release version compatibility | BLOCKED | Exact app/manual release version belum ditetapkan |
| Overall | `PRE-IMPLEMENTATION BASELINE` | Belum layak disebut User Manual release-ready |

Jika task guide sudah ditulis sebelum UI stabil, gunakan `DRAFT — WAITING FOR RELEASE CANDIDATE` dan jangan memberi status screenshot `CAPTURED/APPROVED` tanpa actual build.

---

# 17. Maintenance triggers

## 17.1 Trigger matrix

| Trigger | Impact minimum yang diperiksa |
|---|---|
| Feature added | IA, index, guide, aliases, platform, screenshot, FAQ, UAT/test |
| Feature removed/deferred | Availability, index, links, screenshots, archived-version manual; governance reference |
| UI label/control/path changed | Steps, termbase, search, screenshot, alt text, localization |
| Permission changed | Prerequisite, visibility, denial guidance, Admin cross-reference |
| Platform exposure changed | Applies-to, platform delta, N/A/limitation, screenshots, UAT |
| Error/message flow changed | Troubleshooting, warning, Error Code guidance, FAQ |
| Offline/processing route changed | Local/Server/connection notice, consent, recovery, platform guidance |
| Sync/secured-file behavior changed | Amankan ke Akun, max-three, lifecycle, FAQ, warning |
| Assistant changed | Plan, capability, permission, offline, limitation, security copy |
| Security warning changed | Warning hierarchy, placement, screenshot/data rule, security review |
| Known limitation changed | Guide, FAQ, release note, UAT disposition |
| Compatibility/minimum version changed | Applies-to metadata, update guidance, archive/default manual |
| Localization resource changed | UI labels, termbase, layout, screenshots, alt text, search |
| Accessibility/design system changed | Structure, label semantics, visual label, output QA |
| UAT finding | Guide/FAQ/UX explanation, documentation defect, affected retest |
| Support trend/search failure | FAQ, alias, troubleshooting, contextual help; no automatic rule change |

## 17.2 Maintenance backlog record

```text
Documentation Change ID
Trigger/source work item/UAT/Issue ID
Requirement/Feature IDs
affected guide/term/screenshot/link IDs
affected build/platform/locale
risk/priority
owner/reviewer
target app/manual version
status
test/evidence
```

## 17.3 Lean maintenance for one developer

- Satu documentation backlog dihubungkan ke code/UI work item.
- Change template selalu mempunyai “Documentation impact: YES/NO + reason”.
- Update text dilakukan dalam change yang sama atau tracked follow-up sebelum release gate.
- Screenshot capture ditunda sampai RC stabil, lalu dilakukan secara batch per platform/locale.
- Codex menjalankan consistency/link/coverage checks, tetapi human memvalidasi UI truth, tone, screenshots, accessibility, and UAT outcome.
- Tidak ada kalender/cadence angka rekaan; review dipicu perubahan/rilis/known issue dan policy yang disetujui.

## 17.4 Staleness detection

Manual/screenshot ditandai untuk review bila:

- last-reviewed build berbeda dari release candidate;
- UI resource key/label berubah;
- Feature/platform exposure berubah;
- screenshot build tidak termasuk supported range;
- link/anchor gagal;
- UAT/Support menemukan langkah tidak dapat diselesaikan;
- security warning/policy berubah;
- known limitation/PoC disposition berubah.

Stale detection membuat `REVIEW_REQUIRED` work item/status internal; ia tidak otomatis memperbarui content atau menyatakan asset `OUTDATED` tanpa review.

---

# 18. Risks/gaps

| ID | Risiko/gap | Dampak | Rekomendasi/handoff |
|---|---|---|---|
| `D14-D-001` | Spesifikasi UAT sudah FINAL, tetapi pelaksanaan UAT dan evidence Release Candidate belum dicatat | Manual dapat tampak tervalidasi padahal belum diuji terhadap UI/build aktual | Pertahankan status `PRE-IMPLEMENTATION BASELINE`; finalisasi hanya setelah actual RC, execution result, defect disposition, dan evidence tersedia |
| `D14-D-002` | Actual repository/application UI dan RC belum tersedia | Langkah/label/screenshot tidak dapat diverifikasi | Status tetap `PRE-IMPLEMENTATION BASELINE`; capture hanya dari actual RC |
| `D14-D-003` | Exact app/manual version belum ditetapkan | Applicability dapat dikarang atau cepat usang | Gunakan placeholder `RC-TBD`; isi hanya dari controlled release metadata |
| `D14-D-004` | UI labels/resource keys ID/EN belum final | Manual dan screenshot berpotensi berbeda dari aplikasi | Freeze locale resource manifest; UI Label Check sebelum release |
| `D14-D-005` | English terminology/copy owner belum ditetapkan | Translation security/business meaning tidak konsisten | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`: owner/reviewer termbase |
| `D14-D-006` | Branding/style guide Pertamina untuk icon/emoji/callout belum tersedia | Visual label dapat tidak sesuai manual resmi | Emoji hanya drafting aid; confirm approved icon/color/callout system |
| `D14-D-007` | Publication format/hosting/search/offline bundle belum dipilih | Accessibility, links, search, and update mechanism tidak executable | Confirm approved formats/repository/distribution; no new SaaS assumption |
| `D14-D-008` | Accessibility standard/conformance target belum ditetapkan | Klaim aksesibilitas dapat berlebihan | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`; sementara gunakan guidance dasar tanpa certification claim |
| `D14-D-009` | Supported device/OS/browser/platform matrix belum final | Platform guidance dapat salah | Bind manual to release capability matrix dan real-device UAT |
| `D14-D-010` | Banyak Core tools masih PoC-gated | Manual dapat menjanjikan fidelity/flow yang belum disetujui | Guide berstatus blocked/draft sampai approved engine/limitation tersedia |
| `D14-D-011` | Screenshot capture dapat membocorkan identity/document/Issue/signature | Documentation menjadi exfiltration channel | Dummy/sanitized data, capture preflight, privacy/security review, secure storage |
| `D14-D-012` | Screenshot per 33 tool dapat menjadi dekoratif dan mahal dirawat | Staleness/beban solo developer | Capture hanya visual yang menambah pemahaman; text-first; shared screenshot bila state sama |
| `D14-D-013` | “Amankan ke Akun”, “Secured File”, dan “Sync” mudah dianggap full cloud workspace | Ekspektasi data salah dan unintended upload | Terminology/FAQ/warning menegaskan explicit selective sync dan data yang tidak auto-sync |
| `D14-D-014` | Sign PDF dapat dianggap certificate digital signature | Klaim legal/cryptographic palsu | Selalu gunakan visual/non-PKI wording dan UAT clarity check |
| `D14-D-015` | Redact PDF dapat dianggap overlay visual | Informasi sensitif dapat tetap ada | Gunakan permanent-removal/finalization warning dan task-based verification |
| `D14-D-016` | AI output dapat dianggap authoritative | Keputusan bisnis dari summary/translation yang salah | Review-source warning, limitation, approved-provider boundary, UAT rubric |
| `D14-D-017` | Alias natural dapat bertabrakan dengan Feature lain | Search/Assistant mengarah ke tool salah | Alias registry one-to-one/ambiguity handling; test against canonical Feature ID |
| `D14-D-018` | Satu developer mengelola code, UI, screenshots, dan manual | Documentation drift/self-review bias | Automated impact/link checks + stakeholder/UAT documentation review |
| `D14-D-019` | Admin/Incident/Deployment content dapat masuk User Manual | Scope duplikat dan permission confusion | Cross-reference only; enforce content-owner boundary |
| `D14-D-020` | FAQ dapat menjadi workaround permanen untuk product defect | Defect tidak diperbaiki | FAQ source linked to defect/UAT; Blocker/Critical tidak diselesaikan dengan copy |
| `D14-D-021` | Exact documentation/screenshot/evidence retention belum ditetapkan | Asset lama terhapus atau data terlalu lama disimpan | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`; minimize and version-reference meanwhile |
| `D14-D-022` | Correlation/Job IDs dapat terlihat seperti credential | User membagikan detail berlebihan atau menganggap ID memberi akses | Glossary menyatakan identifier support only; no token/link semantics |
| `D14-D-023` | UI adaptive berbeda per platform/locale | Satu langkah universal menjadi salah | Shared outcome + explicit platform deltas; separate locale screenshots |
| `D14-D-024` | Tool availability can be hidden by permission/security projection | Manual troubleshooting dapat membocorkan hidden capability | Gunakan safe generic reasons dan authorized support path; jangan mengungkap resource/policy detail |

## 18.1 Inconsistency audit

- Tidak ada mismatch canonical Feature ID pada indeks 33 Core yang digunakan; mapping mengikuti Document 03 FINAL.
- `FEAT-PROD-018` mendukung offline help/user guide; exact packaging/distribution belum dikunci.
- Source mengunci Bahasa Indonesia dan English serta fallback Bahasa Indonesia; manual utama Indonesia konsisten.
- Source mengunci Sign PDF visual non-PKI, max-three secured versions, and Local-First/explicit upload boundary; terminology/FAQ mempertahankannya.
- Document 13 FINAL v1.0 telah dibaca dan dipakai untuk UAT ID, status, 33-Core, serta Merge handoff; dokumen mencatat bahwa pelaksanaan UAT belum diklaim.

Tidak ada Feature ID, role, UI screen, screenshot, release version, vendor, atau product capability baru yang dibuat laporan ini.

---

# 19. Suggested Document 14 text

Teks berikut disarankan untuk integrasi setelah actual Release Candidate, execution evidence UAT, release UI, terminology, platform matrix, dan publication format tersedia.

## 19.1 Status dan tujuan manual

> User Manual PERTAMINA OTO menggunakan struktur task-based dan tidak harus dibaca dari awal sampai akhir. Manual utama berbahasa Indonesia dengan English translation set yang setara. Bahasa Indonesia menjadi fallback. Setiap guide terikat pada version, platform, capability, locale, canonical Feature ID, requirement, documentation test, dan UAT evidence yang relevan.
>
> Sebelum actual release candidate, manual berstatus `PRE-IMPLEMENTATION BASELINE` atau `DRAFT — WAITING FOR RELEASE CANDIDATE`. Status `RELEASED` hanya diberikan setelah UI labels, langkah, screenshots, links, terminology, localization, accessibility, security warning, version compatibility, UAT feedback, dan owner lulus gate.

## 19.2 Information architecture

> Struktur manual adalah Mulai Cepat, Dasar Penggunaan, Fitur Utama, Platform, Assistant, Penyimpanan & Sync, Masalah Umum, FAQ, Glossary, dan Indeks. Landing page menyediakan daftar isi, quick links, search-friendly heading, alias umum, cross-reference, dan indeks 33 Primary PDF Tools. User dapat masuk langsung dari task, tool, error, Activity Center, atau Help context tanpa membaca bab sebelumnya.

## 19.3 Guide standard

> Setiap guide memuat tujuan, applies-to version/platform/language, availability/offline/online/permission, prasyarat, langkah bernomor, expected result, warning, error/recovery guidance, known limitation, related links, Screenshot IDs, Feature/Requirement/UAT traceability, status, owner, dan last-reviewed build. Feature ID digunakan untuk traceability dan tidak harus ditampilkan kepada user biasa.

## 19.4 Writing style

> Manual menggunakan kalimat pendek dan aktif. Satu langkah berisi satu aksi. Tombol dan control dicetak tebal; menu path menggunakan pola **Menu › Submenu › Action**. Warning diletakkan sebelum tindakan berisiko, sedangkan Catatan/Tips diletakkan setelah langkah terkait. Istilah satu arti memakai satu nama: Simpan berbeda dari Amankan ke Akun; History berbeda dari backup/Audit; Working State berbeda dari version; Undo/Redo berbeda dari versioning; Visual Signature berbeda dari digital signature certificate.

## 19.5 Findability dan terminology

> Nama tool pada manual harus sama dengan final UI label untuk locale/build terkait. Alias seperti “Gabung PDF” untuk Merge PDF, “Kecilkan PDF” untuk Compress PDF, dan “Ambil tulisan” untuk OCR PDF dipakai untuk search/index/Assistant intent tanpa mengubah canonical Feature ID. Terminology base menyimpan istilah Indonesia, English, UI resource key, definition, aliases, forbidden terms, platform notes, owner, status, dan last-reviewed build.

## 19.6 Visual labels

> Label semantik adalah Catatan, Perhatian, Penting, Hasil, Tips, Memerlukan koneksi, Dapat digunakan offline, dan Memerlukan izin. Icon/emoji hanya drafting aid sampai style guide Pertamina mengizinkan. Teks label selalu tersedia; makna tidak disampaikan hanya melalui color atau icon.

## 19.7 Screenshot governance

> Screenshot register mencatat Screenshot ID, chapter/guide, platform, language, build version, screen, purpose, data type, masking requirement, status, owner, capture date, artifact reference/digest, alt text, caption, last-reviewed build, dan replacement ID. Status adalah `PLANNED`, `CAPTURED`, `REVIEWED`, `APPROVED`, atau `OUTDATED`. Screenshot `OUTDATED` tidak boleh masuk active manual release.
>
> Screenshot hanya dibuat dari actual application build menggunakan dummy/sanitized PDF, account, Issue, filename, identifier, dan synthetic visual-signature mark. Password, token, secret, sensitive document, real user identity, confidential internal data, real signature, private Issue, raw log/stack/path/storage key, dan unrelated desktop/browser content dilarang. Mockup/edited image tidak boleh disebut screenshot.

## 19.8 Localization

> Bahasa Indonesia menjadi source utama dan fallback. English menggunakan content structure yang sama, stable Content ID, approved termbase, exact English UI labels, dan locale-specific screenshots. Feature/Requirement/Test/UAT/Job/Correlation/Error IDs, filename, user content, product name, dan format names tidak diterjemahkan. Translation melewati terminology, linguistic, functional UI, screenshot, link/layout, accessibility, dan UAT review.

## 19.9 Accessibility

> Manual menggunakan heading terstruktur, numbered steps, plain language, meaningful links, readable tables, alt text, captions, dan text equivalents untuk screenshot. Informasi tidak mengandalkan color/icon/image saja. Target certification/conformance hanya dinyatakan setelah format publikasi dan standard resmi divalidasi; manual tidak membuat klaim accessibility resmi tanpa evidence.

## 19.10 FAQ dan glossary

> FAQ dipelihara dari UAT questions, Support trends, search failures, dan known limitations. FAQ minimum menjawab feature visibility, file-open failure, server transfer, offline work, cancel, Amankan ke Akun, maximum-three versions, visual non-PKI Sign PDF, AI Summarizer limitation, dan Laporkan Masalah. Glossary minimum mendefinisikan Activity Center, Assistant, Feature, Local/Server Processing, Offline, Working State, Autosave, Checkpoint, Secured File, Amankan ke Akun, OCR, PDF/A, Visual Signature, Redaction, dan Correlation ID bila terlihat user/support.

## 19.11 Documentation testing

> Documentation QA mencakup Task-Based Review, New User Review, UAT Cross-Check, UI Label Check, Screenshot Check, Link Check, Feature Availability Check, Security Warning Check, Localization Review, Accessibility Review, Version Compatibility Check, dan Offline Help Check. Untuk fitur utama, tester harus dapat menemukan dan menyelesaikan task menggunakan manual tanpa penjelasan tambahan. Manual tidak diubah untuk membenarkan UI yang bertentangan dengan requirement; konflik memakai STOP protocol.

## 19.12 Change, version, dan maintenance

> Perubahan Feature/UI memicu documentation impact analysis, terminology/UI-resource update, text/cross-reference update, actual RC screenshot replacement, localization/accessibility/security review, affected documentation test/UAT cross-check, approval, dan release bersama compatible app build. Manual metadata mencatat manual version, locale, status, applies-from/through app version, platforms, validated build/digest, UI/capability version, owner, limitations, dan supersession.
>
> Maintenance dipicu Feature add/remove, UI label/flow, permission, platform exposure, error/recovery, Local/Server route, sync, Assistant, security warning, compatibility, localization, accessibility, known limitation, UAT finding, dan Support/search trend. Satu developer menggunakan satu documentation backlog dan automated impact/link checks; human tetap memvalidasi UI truth, screenshots, translation, accessibility, security, dan UAT outcome.

## 19.13 Release checklist

```text
[ ] Document 13 FINAL traceability direkonsiliasi dengan actual UAT execution dan evidence
[ ] exact RC/app version/digest and platform matrix recorded
[ ] ID/EN UI labels final and matching manual
[ ] all in-scope steps task-tested
[ ] 33-Core index/capability mapping current
[ ] all used screenshots actual, APPROVED, safe, captioned, and have alt text
[ ] known limitations/errors/security warnings current
[ ] links/search/TOC/offline help pass
[ ] localization and accessibility reviews pass
[ ] UAT feedback resolved/dispositioned
[ ] traceability and owner complete
[ ] no Admin/Incident/Deployment duplication
[ ] no sensitive data or false product/version/accessibility claim
```

---

**Serah terima kepada Integrator:** konfirmasikan actual UAT execution/evidence, actual UI/resource labels, application/manual version policy, supported platform matrix, publication/search/offline-help format, bilingual terminology owner, Pertamina visual documentation style, accessibility target, screenshot storage/retention, dan documentation ownership. Setelah RC tersedia, ubah status dari `PRE-IMPLEMENTATION BASELINE` ke `DRAFT — WAITING FOR RELEASE CANDIDATE`/review yang tepat hanya dengan actual UI, screenshot, documentation test, dan UAT evidence; jangan membuat screenshot atau approval palsu.
