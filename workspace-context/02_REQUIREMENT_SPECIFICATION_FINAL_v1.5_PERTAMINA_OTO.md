# 02 — SPESIFIKASI KEBUTUHAN
## PERTAMINA OTO — Office Tools Organizer

> **Standar bahasa:** Bahasa Indonesia — ID-1.0

---

## 1. Informasi Dokumen

| Atribut | Nilai |
|---|---|
| Nama dokumen | `02_REQUIREMENT_SPECIFICATION.md` |
| Nama proyek | **PERTAMINA OTO — Office Tools Organizer** |
| Versi | **1.5** |
| Status | **FINAL — PRE-CODING BASELINE** |
| Dokumen induk | `01_BUSINESS_PROCESS_FINAL_v3.4_PERTAMINA_OTO.md` |
| Jenis aplikasi | Aplikasi internal Pertamina |
| Baseline teknologi | C#, ASP.NET Core MVC, ASP.NET Core Web API, SQL Server, Entity Framework Core |
| Arsitektur | Clean Architecture |
| Kanal utama | Windows Desktop C#/.NET — primary full-feature productivity client |
| Kanal tambahan | Responsive Web; curated Mobile/Tablet C#/.NET subject to architecture/security approval |
| Model operasi | Offline-First; Local-First berbasis capability platform |
| Penyimpanan | Local-First Workspace + Selective Cross-Device Sync |
| Repository dan CI/CD | Azure DevOps Git dan Azure DevOps Pipeline |
| Environment | Development, QA/Testing, Production |
| Monitoring | Application Insights / APM |
| Bahasa awal | Bahasa Indonesia dan English |

### Riwayat Revisi

| Versi | Perubahan |
|---|---|
| 1.0 | Penyusunan final requirement berdasarkan 14 keputusan yang telah disepakati |
| 1.1 | Pre-coding hardening: adaptive capability/UX, server resource budget, selective cross-device sync, acceptance berlapis, DoD 8 lapis, mobile-first scanner/signature visual, dan penghapusan Digital Signature dari scope |
| 1.2 | Penambahan PERTAMINA OTO Assistant Offline-First Hybrid, Triple Entry Flow, assistant security boundary, workflow/schema validation, local AI routing, dan feasibility gate sebelum coding fitur berisiko tinggi |
| 1.3 | Penyelarasan Solution Design: Desktop-First + curated Phone/Tablet, In-App Issue Reporting yang sangat sederhana, safe diagnostics, Superadmin Support Console, status tracking, dan export laporan PDF/Excel |
| 1.4 | Controlled revision: menambahkan Common-Simple-First Implementation Sequencing agar pengembangan awal mendahulukan fitur sederhana, sering digunakan, low-risk, local-capable, dan low-dependency tanpa mengubah baseline canonical pada versi tersebut |
| 1.5 | Primary Feature Audit: menetapkan 33 Core PDF Tools sebagai prioritas sebelum user-tool sekunder; menambahkan requirement PDF Intelligence, Sign PDF visual non-PKI, dan Core Feature sequencing/gate |

---

## 2. Tujuan Dokumen

Dokumen ini menjadi acuan untuk:

1. Menentukan kebutuhan fungsional dan nonfungsional PERTAMINA OTO.
2. Menjaga kesesuaian implementasi dengan Business Process.
3. Mempermudah pembagian pekerjaan kepada Codex.
4. Menyediakan dasar pembuatan Feature Catalog, Solution Design, API, Database, Security Design, Test Plan, dan UAT.
5. Menjaga traceability dari kebutuhan sampai implementasi dan pengujian.
6. Mencegah perubahan arsitektur atau perilaku aplikasi tanpa keputusan resmi.

Dokumen ini tidak menggantikan proses persetujuan arsitektur, IT Security, RIC, dan tata kelola Pertamina.

---

## 3. Cara Menggunakan Dokumen Ini di Codex

Codex harus bekerja berdasarkan requirement ID.

Contoh instruksi:

```text
Implementasikan FR-FILE-001 sampai FR-FILE-006.

Ketentuan:
- Jangan mengubah requirement lain.
- Gunakan Clean Architecture.
- Tambahkan unit test.
- Jangan membuat akses langsung dari client ke SQL Server.
- Ikuti acceptance criteria pada setiap requirement.
```

Aturan kerja Codex:

1. Satu task hanya mengerjakan requirement yang disebutkan.
2. Codex tidak boleh mengubah business rule tanpa persetujuan.
3. Setiap perubahan wajib menyebut requirement ID.
4. Setiap requirement yang selesai harus memiliki test yang relevan.
5. Fitur penting wajib memenuhi **Functional Acceptance, Business Acceptance, dan UX Acceptance**.
6. Fitur tidak boleh dinyatakan selesai hanya karena build/unit test lulus; seluruh **Definition of Done 8 lapis** wajib diperiksa.
7. Semua error harus menggunakan error code.
8. Secret, token, password, isi dokumen, clipboard, dan signature asset tidak boleh masuk log.
9. Implementasi harus mempertahankan responsive **dan Adaptive UX** sesuai capability platform.
10. Pemrosesan harus mengikuti Offline-First dan Local-First sejauh capability platform memungkinkan.
11. Setiap fitur wajib mematuhi Server Resource Budget yang ditetapkan pada Feature Catalog/Solution Design.
12. Implementasi tidak boleh menambahkan Flutter.
13. Teknologi tambahan hanya boleh digunakan setelah disetujui.
14. Source code wajib mengikuti struktur Clean Architecture.
15. Tidak boleh memindahkan proses ke server hanya untuk mempermudah implementasi jika proses lokal layak, aman, dan sesuai policy.
16. Dalam priority/release yang sama, urutan implementasi awal mengikuti **Common-Simple-First**: dahulukan fitur yang sering digunakan banyak user, sederhana, low-risk, local-capable, dan dependency-nya rendah.

---

## 4. Struktur Requirement

Format ID:

```text
<TIPE>-<MODUL>-<NOMOR>
```

### 4.1 Tipe Requirement

| Kode | Arti |
|---|---|
| `FR` | Functional Requirement |
| `NFR` | Non-Functional Requirement |
| `BR` | Business Rule |
| `UIR` | UI/UX Requirement |
| `IR` | Integration Requirement |
| `DR` | Data Requirement |
| `SR` | Security Requirement |

### 4.2 Modul Requirement

| Modul | Arti |
|---|---|
| `AUTH` | Login, SSO, session |
| `ROLE` | Role dan permission |
| `FILE` | Input dan validasi file |
| `PROC` | Pemrosesan lokal/server |
| `STOR` | Penyimpanan lokal |
| `BACKUP` | Amankan ke Akun |
| `HIST` | Riwayat user |
| `AUDIT` | Audit perusahaan |
| `TRASH` | Recycle Bin |
| `REC` | Recovery, retry, resume |
| `VER` | Versioning |
| `UI` | Tampilan dan interaksi |
| `L10N` | Multi-bahasa |
| `PERF` | Performa |
| `SEC` | Keamanan |
| `API` | Web API |
| `DB` | Database |
| `MON` | Monitoring |
| `DEVOPS` | Repository dan pipeline |
| `ENV` | Environment |
| `GOV` | Governance |
| `DOC` | Dokumentasi |
| `HAND` | Handover |
| `SUP` | Support |
| `SCAN` | Scanner dan OCR lintas perangkat |
| `SIGN` | Tanda tangan visual/paraf (non-PKI) dan anotasi |
| `ASSIST` | PERTAMINA OTO Assistant, intent, plan, orchestration, dan AI fallback |
| `INTEL` | PDF Intelligence: summarization, translation, dan structured text/Markdown output |

### 4.3 Tingkat Kedalaman

#### Level Lengkap

Digunakan untuk:

- Keamanan.
- Login.
- Role.
- Audit.
- Penyimpanan.
- Backup.
- Integrasi.
- API.
- Deployment.
- Monitoring.
- Requirement lintas sistem.
- Requirement berisiko tinggi.

Format:

```text
ID
Judul
Deskripsi
Prioritas
Rilis
Aktor
Precondition
Alur utama
Business rule
Functional Acceptance
Business Acceptance
UX Acceptance
Error condition
Dependensi
Processing Mode
Server Dependency
Storage Impact
Network Impact
Concurrency Risk
```

#### Level Ringkas

Digunakan untuk fitur sederhana.

Format:

```text
ID
Deskripsi
Prioritas
Rilis
Acceptance criteria
Server Resource Budget (bila relevan)
```

---

## 5. Prioritas dan Fase Rilis

### 5.1 MoSCoW

| Prioritas | Arti |
|---|---|
| Must | Wajib tersedia |
| Should | Penting tetapi dapat ditunda |
| Could | Tambahan yang bermanfaat |
| Future | Belum dibuat pada tahap sekarang |

### 5.2 Fase

| Fase | Tujuan |
|---|---|
| MVP | Produk awal aman dan dapat digunakan |
| Release 2 | Penguatan produktivitas dan fitur |
| Release 3 | Integrasi dan perluasan |
| Future | Full Cloud Workspace dan kemampuan lanjutan |

### 5.3 Ketentuan Wajib

Requirement berikut selalu berstatus **Must**:

- Keamanan.
- Login dan role.
- Audit trail.
- ASP.NET Core.
- SQL Server.
- Entity Framework Core.
- Azure DevOps.
- Development–QA–Production.
- Monitoring.
- Dokumentasi.
- Handover.

---

# 6. Keputusan Requirement Utama

## DEC-REQ-001 — Dua Tingkat Kedalaman Requirement
Requirement penting ditulis lengkap. Requirement sederhana ditulis ringkas.

## DEC-REQ-002 — Platform
Windows Desktop C#/.NET menjadi primary full-feature productivity client. Responsive Web tetap menjadi kanal enterprise/browser. Phone/Tablet menggunakan curated companion experience untuk fitur terpilih yang natural dan praktis digunakan secara mobile. Semua client menggunakan ASP.NET Core Web API dan tidak mengakses SQL Server secara langsung.

## DEC-REQ-003 — Bahasa
Bahasa Indonesia dan English.

## DEC-REQ-004 — ID Requirement
Format: `<TIPE>-<MODUL>-<NOMOR>`.

## DEC-REQ-005 — Prioritas
MoSCoW dan fase MVP, Release 2, Release 3, Future.

## DEC-REQ-006 — Login
Single Sign-On adaptif Pertamina.

## DEC-REQ-007 — Validasi File
Validasi bertingkat, cepat, dan aman.

## DEC-REQ-008 — Pemrosesan
Offline-First Enterprise Hybrid.

## DEC-REQ-009 — Penyimpanan
Local-First dengan penyimpanan bertingkat.

## DEC-REQ-010 — Backup
Selective Secure Result Backup melalui tombol **Amankan ke Akun**.

## DEC-REQ-011 — Riwayat dan Audit
Riwayat user dan audit perusahaan dipisahkan.

## DEC-REQ-012 — Penghapusan
Recycle Bin dengan notifikasi dan penghapusan aman.

## DEC-REQ-013 — Recovery
Pemulihan otomatis, retry, dan resume.

## DEC-REQ-014 — Versioning
Penyimpanan sederhana dengan maksimal tiga versi terakhir untuk file yang diamankan ke akun.

## DEC-REQ-015 — Offline-First dan Capability-Aware Local-First
Offline-First adalah perilaku produk. Local-First diterapkan sejauh capability platform, resource perangkat, security policy, dan library yang disetujui memungkinkan.

## DEC-REQ-016 — Adaptive UX
Responsive tidak berarti UI identik pada semua perangkat. Desktop, Web, dan Mobile dapat memiliki alur/interaksi berbeda selama business outcome dan konsistensi produk tetap terjaga.

## DEC-REQ-017 — Mobile-First Scanner dan Visual Signature
Scanner kamera dan drawing tanda tangan/paraf diprioritaskan pada mobile/tablet. Desktop memprioritaskan impor file scan serta upload/paste/saved visual signature; drawing mouse tetap opsional.

## DEC-REQ-018 — Server Resource Budget
Setiap fitur wajib diklasifikasikan berdasarkan Processing Mode, Server Dependency, Storage Impact, Network Impact, dan Concurrency Risk. Keterbatasan server awal tidak menghapus fitur resmi.

## DEC-REQ-019 — Local-First Workspace + Selective Cross-Device Sync
Draft, working state, undo/redo, cache, thumbnail, temporary file, dan checkpoint utama tetap lokal. File hasil yang dipilih melalui **Amankan ke Akun** beserta metadata ringan dan maksimal tiga versi dapat diakses lintas perangkat.

## DEC-REQ-020 — Acceptance Berlapis
Fitur penting wajib memiliki Functional Acceptance, Business Acceptance, dan UX Acceptance.

## DEC-REQ-021 — Definition of Done 8 Lapis
Fitur hanya dinyatakan DONE setelah Technical Correctness, Business Process Correctness, UI/UX Acceptance, Security & Privacy, Performance/Server Budget, Error & Recovery, Cross-Platform Behavior, serta Test & Documentation terpenuhi.

## DEC-REQ-022 — Digital Signature di Luar Scope
Digital Signature berbasis sertifikat/PKI tidak termasuk scope implementasi saat ini. Scope tanda tangan saat ini adalah tanda tangan visual/paraf (non-PKI), paraf, stamp, dan anotasi yang disetujui.

## DEC-REQ-023 — PERTAMINA OTO Assistant
PERTAMINA OTO Assistant menggunakan model **Offline-First Hybrid Conversational Orchestrator**. Assistant mengatur fitur resmi tetapi bukan processing engine.

## DEC-REQ-024 — Triple Entry Flow
Jalur masuk aplikasi adalah File-First, Tool-First, dan Assistant-First. Assistant merupakan alternatif, bukan pengganti navigasi visual.

## DEC-REQ-025 — Canonical Tool Orchestration
Assistant hanya dapat memanggil Feature ID canonical dan parameter yang lolos schema validation, capability check, permission/policy check, dan risk/confirmation rule.

## DEC-REQ-026 — Assistant Privacy & Security Boundary
Prompt/chat dan isi dokumen tidak dikirim atau disinkronkan otomatis. Konten dokumen adalah untrusted data; AI service internal hanya digunakan bila diperlukan, diizinkan, dan memenuhi data-minimization/consent/security policy.

## DEC-REQ-027 — In-App Issue Reporting & Support Workflow
User dapat melaporkan ketidaksesuaian langsung dari PERTAMINA OTO dengan **pesan singkat dan screenshot opsional** tanpa form panjang. Sistem melampirkan safe diagnostic context secara otomatis. Laporan masuk ke Superadmin Support Console untuk triage, status tracking, dan export PDF/Excel sebagai bahan perbaikan programmer.

## DEC-REQ-028 — Common-Simple-First Implementation Sequencing
Dalam priority dan release yang sama, implementasi awal mendahulukan fitur yang paling sering digunakan banyak user, sederhana dipahami, berisiko rendah, dapat diproses lokal bila memungkinkan, serta memiliki dependency teknis minimum. Aturan ini mengatur **urutan pengerjaan**, bukan menghapus, menurunkan, atau mengganti Feature ID canonical maupun MoSCoW/release yang telah disetujui.
## DEC-REQ-029 — Primary PDF Feature Set First
Tiga puluh tiga tool pada **Primary PDF Tool Surface** menjadi Core Feature Set. User-tool sekunder baru diprioritaskan setelah Core Feature Set selesai atau memiliki keputusan feasibility/exception resmi. Foundation/security/platform dependency tidak dianggap user-tool sekunder dan tetap dibangun sesuai kebutuhan.

## DEC-REQ-030 — PDF Intelligence sebagai Tool Resmi, bukan Kemampuan Tersembunyi Assistant
AI Summarizer dan Translate PDF adalah user-facing canonical tools yang dapat diorkestrasi Assistant tetapi tidak menjadi bagian internal Assistant. PDF to Markdown adalah conversion tool resmi. Provider AI/translation hanya local/enterprise-internal yang disetujui dan tetap tunduk pada privacy/security policy.

---

# 7. Kebutuhan Fungsional

## 7.1 Autentikasi dan Sesi

### FR-AUTH-001 — Single Sign-On
**Prioritas:** Must  
**Rilis:** MVP

Sistem harus menyediakan login menggunakan identity provider Pertamina yang disetujui.

**Acceptance criteria:**
- User dapat login menggunakan akun Pertamina.
- Sistem menerima identity dan claim yang valid.
- Login gagal tidak membuka akses aplikasi.
- Login dan gagal login dicatat sebagai audit metadata.

### FR-AUTH-002 — Login Sekali
**Prioritas:** Must  
**Rilis:** MVP

Sistem harus menjaga sesi agar user tidak perlu login berulang dalam kondisi normal.

**Acceptance criteria:**
- Sesi aktif tetap dapat digunakan.
- Token diperbarui otomatis bila didukung.
- User hanya diminta login kembali saat sesi tidak valid atau kebijakan mewajibkan.

### FR-AUTH-003 — Sesi Offline
**Prioritas:** Must  
**Rilis:** MVP

Fitur lokal harus tetap dapat digunakan selama sesi offline masih valid.

**Acceptance criteria:**
- Sistem mengenali status offline.
- Fitur lokal tetap tersedia.
- Fitur server diberi status memerlukan koneksi.
- Sesi offline memiliki masa berlaku.

### FR-AUTH-004 — Logout dan Pencabutan Sesi
**Prioritas:** Must  
**Rilis:** MVP

Sistem harus mendukung logout serta pencabutan sesi oleh sistem atau administrator yang berwenang.

### FR-AUTH-005 — Batas Kewenangan Sesi Offline
**Prioritas:** Must  
**Rilis:** MVP

Saat offline, sistem hanya menggunakan claim/permission yang dicache secara aman dan masih valid sesuai policy. Aksi sensitif yang membutuhkan status permission terkini atau server authorization harus ditunda sampai online. Masa berlaku offline session harus configurable melalui policy.

---

## 7.2 Role dan Permission

### FR-ROLE-001 — Role Awal
**Prioritas:** Must  
**Rilis:** MVP

Sistem harus menyediakan role awal: User, Administrator, dan Auditor.

### FR-ROLE-002 — Permission-Based Access
**Prioritas:** Must  
**Rilis:** MVP

Setiap fitur harus dikendalikan oleh permission, bukan hanya nama role.

### FR-ROLE-003 — Role dari Server
**Prioritas:** Must  
**Rilis:** MVP

Role dan permission harus dikendalikan oleh backend.

---

## 7.3 Input File

### FR-FILE-001 — File Picker
**Prioritas:** Must  
**Rilis:** MVP

Sistem harus menerima satu atau banyak file melalui file picker.

### FR-FILE-002 — Drag-and-Drop
**Prioritas:** Must  
**Rilis:** MVP

Sistem harus mendukung drag-and-drop pada platform yang mendukung.

### FR-FILE-003 — Paste File dan Screenshot
**Prioritas:** Must  
**Rilis:** MVP

Sistem harus menerima file atau gambar dari clipboard.

### FR-FILE-004 — Share/Open With
**Prioritas:** Should  
**Rilis:** Release 2

Sistem harus menerima file dari aplikasi lain melalui mekanisme platform yang disetujui.

### FR-FILE-005 — Recent Files
**Prioritas:** Should  
**Rilis:** MVP

Sistem harus menampilkan file yang baru digunakan tanpa menyalin isi file secara otomatis.

---

## 7.4 Validasi File Bertingkat

### FR-FILE-010 — Fast Gate
**Prioritas:** Must  
**Rilis:** MVP

Saat file dimasukkan, sistem harus memeriksa nama, ekstensi, MIME type, file signature, ukuran, hak baca, dan allowlist format.

### FR-FILE-011 — Structural Validation
**Prioritas:** Must  
**Rilis:** MVP

Sistem harus memeriksa file rusak, password, jumlah halaman, macro, embedded object, dan ukuran dekompresi.

### FR-FILE-012 — Security Inspection
**Prioritas:** Must  
**Rilis:** MVP

Sistem harus menjalankan pemeriksaan keamanan jika diwajibkan oleh jenis file, proses, atau kebijakan.

### FR-FILE-013 — Status Validasi
**Prioritas:** Must  
**Rilis:** MVP

Status minimum:
`READY`, `PASSWORD_REQUIRED`, `WARNING`, `UNSUPPORTED`, `CORRUPTED`, `SECURITY_REVIEW`, `BLOCKED`, `INSUFFICIENT_STORAGE`.

### FR-FILE-014 — Batch Validation
**Prioritas:** Must  
**Rilis:** MVP

File valid dalam batch tetap dapat diproses meskipun ada file lain yang bermasalah.

### FR-FILE-015 — Password File
**Prioritas:** Must  
**Rilis:** MVP

Password file hanya digunakan selama proses dan tidak boleh masuk log, audit, autosave, atau backup.

---

## 7.5 Pemrosesan Lokal dan Server

### FR-PROC-001 — Pemilihan Engine Otomatis
**Prioritas:** Must  
**Rilis:** MVP

Sistem harus memilih pemrosesan lokal atau server secara otomatis berdasarkan fitur, format, ukuran, jumlah halaman, kemampuan perangkat, kapasitas, jaringan, role, klasifikasi, kebijakan keamanan, dan ketersediaan server. Pemilihan tidak boleh menganggap semua platform memiliki capability lokal yang sama.

### FR-PROC-002 — Local-First
**Prioritas:** Must  
**Rilis:** MVP

Pemrosesan dilakukan lokal jika platform mendukung, kapasitas perangkat cukup, kebijakan mengizinkan, dan fitur tidak memerlukan server. Windows Desktop dapat memiliki local processing paling lengkap; Web dan Mobile menggunakan capability lokal yang tersedia dan fallback server hanya bila dibutuhkan.

### FR-PROC-003 — Server Processing
**Prioritas:** Must  
**Rilis:** MVP

Server digunakan untuk proses berat, kompleks, membutuhkan library terpusat, security inspection, atau tidak didukung lokal. Server tidak boleh dipakai untuk memindahkan beban yang sebenarnya layak diproses lokal hanya demi menyederhanakan implementasi.

### FR-PROC-004 — Pemberitahuan Upload
**Prioritas:** Must  
**Rilis:** MVP

Sistem harus meminta persetujuan sebelum isi dokumen dikirim ke server.

### FR-PROC-005 — Offline Behavior
**Prioritas:** Must  
**Rilis:** MVP

Saat offline, fitur lokal tetap berjalan, fitur server diberi status tertunda/tidak tersedia, audit metadata disimpan lokal, dan job dapat dilanjutkan saat koneksi tersedia.

---

## 7.6 Penyimpanan Lokal

### FR-STOR-001 — File Sumber Tetap Lokal
**Prioritas:** Must  
**Rilis:** MVP

File sumber tetap berada di lokasi asal dan tidak boleh ditimpa otomatis.

### FR-STOR-002 — Working State
**Prioritas:** Must  
**Rilis:** MVP

Perubahan disimpan sebagai working state terpisah dari file sumber.

### FR-STOR-003 — Autosave Draft
**Prioritas:** Must  
**Rilis:** MVP

Sistem menyimpan draft otomatis dengan debounce. Draft bukan file final, password tidak disimpan, dan file asli tidak ditimpa.

### FR-STOR-004 — Cache dan Thumbnail
**Prioritas:** Must  
**Rilis:** MVP

Cache dan thumbnail dibuat hanya saat diperlukan.

### FR-STOR-005 — Temporary File
**Prioritas:** Must  
**Rilis:** MVP

File sementara dibersihkan setelah proses selesai atau gagal sesuai kebijakan.

### FR-STOR-006 — Pemeriksaan Kapasitas
**Prioritas:** Must  
**Rilis:** MVP

Sistem memeriksa kapasitas sebelum memulai proses besar.

---

## 7.7 Local-First Workspace + Selective Cross-Device Sync

### FR-BACKUP-001 — Tombol Amankan ke Akun
**Prioritas:** Must  
**Rilis:** MVP

Setelah file hasil dibuat, user dapat menekan tombol **Amankan ke Akun**.

### FR-BACKUP-002 — Data yang Dicadangkan
**Prioritas:** Must  
**Rilis:** MVP

Secara default, hanya file hasil yang dipilih user yang dicadangkan.

### FR-BACKUP-003 — Data yang Tidak Otomatis Dicadangkan
**Prioritas:** Must  
**Rilis:** MVP

Sistem tidak boleh otomatis mencadangkan file sumber, draft, cache, temporary file, password, token, atau clipboard.

### FR-BACKUP-004 — Akses Antarperangkat
**Prioritas:** Must  
**Rilis:** MVP

File yang diamankan dapat dibuka atau diunduh setelah user login dari perangkat lain menggunakan akun yang sama.

### FR-BACKUP-005 — Enkripsi
**Prioritas:** Must  
**Rilis:** MVP

File dienkripsi saat dikirim dan saat disimpan.

### FR-BACKUP-006 — Ownership
**Prioritas:** Must  
**Rilis:** MVP

File backup terikat pada pemilik dan tidak dapat dibuka user lain tanpa permission resmi.

### FR-BACKUP-007 — Full Cloud Workspace
**Prioritas:** Future  
**Rilis:** Future

Sistem dapat dikembangkan menjadi penyimpanan otomatis seluruh draft, aset, dan hasil setelah infrastruktur dinyatakan siap.

### FR-BACKUP-008 — Selective Metadata Sync
**Prioritas:** Must  
**Rilis:** MVP

Sistem dapat menyinkronkan metadata ringan yang disetujui seperti daftar file yang diamankan, status versi, favorite tools, preset tertentu, serta preferensi bahasa/tampilan tanpa menyinkronkan isi workspace aktif secara penuh.

### FR-BACKUP-009 — Data Workspace Tetap Lokal
**Prioritas:** Must  
**Rilis:** MVP

Draft, working state, undo/redo, cache, thumbnail, temporary file, checkpoint utama, clipboard, password PDF, dan file sumber yang tidak dipilih tidak boleh disinkronkan otomatis ke server.

### FR-BACKUP-010 — Restore Lintas Perangkat
**Prioritas:** Must  
**Rilis:** MVP

User yang login dengan akun Pertamina yang sama dapat melihat metadata file yang diamankan dan membuka/mengunduh file tersebut dari perangkat lain sesuai permission dan policy.

---

## 7.8 Riwayat User

### FR-HIST-001 — Riwayat Pribadi
**Prioritas:** Must  
**Rilis:** MVP

Riwayat menampilkan nama file, fitur, waktu, status, lokasi hasil, ukuran, dan durasi.

### FR-HIST-002 — Aksi Riwayat
**Prioritas:** Should  
**Rilis:** MVP

User dapat membuka hasil, membuka lokasi, memproses ulang, atau menghapus item riwayat pribadi.

### FR-HIST-003 — Lazy Loading
**Prioritas:** Must  
**Rilis:** MVP

Riwayat dimuat bertahap, maksimal 50 item pada pemuatan awal.

---

## 7.9 Audit Perusahaan

### FR-AUDIT-001 — Audit Metadata
**Prioritas:** Must  
**Rilis:** MVP

Audit minimum mencatat Process ID, User ID, role, waktu, fitur, status, ukuran, lokal/server, error code, dan versi aplikasi.

### FR-AUDIT-002 — Data yang Dilarang
**Prioritas:** Must  
**Rilis:** MVP

Audit tidak boleh mencatat isi dokumen, password, token, clipboard, screenshot, teks OCR lengkap, atau tanda tangan.

### FR-AUDIT-003 — Sinkronisasi Audit
**Prioritas:** Must  
**Rilis:** MVP

Saat offline, audit metadata disimpan lokal secara aman dan disinkronkan otomatis ketika koneksi tersedia.

### FR-AUDIT-004 — Audit Tidak Dapat Dihapus Sembarangan
**Prioritas:** Must  
**Rilis:** MVP

Audit perusahaan hanya dikelola sesuai kebijakan retensi dan kewenangan resmi.

---

## 7.10 Recycle Bin

### FR-TRASH-001 — Pindah ke Recycle Bin
**Prioritas:** Must  
**Rilis:** MVP

File hasil, backup, dan draft yang dihapus user masuk Recycle Bin.

### FR-TRASH-002 — Undo Penghapusan
**Prioritas:** Must  
**Rilis:** MVP

Setelah file dipindahkan, sistem menampilkan opsi **Urungkan**.

### FR-TRASH-003 — Notifikasi 30 Hari
**Prioritas:** Must  
**Rilis:** MVP

Sistem memberi pengingat jika ada file lebih dari 30 hari di Recycle Bin.

### FR-TRASH-004 — Notifikasi Kapasitas
**Prioritas:** Must  
**Rilis:** MVP

Sistem memberi pengingat jika ukuran Recycle Bin lebih dari 2 GB atau sisa penyimpanan kurang dari 15%.

### FR-TRASH-005 — Batas Frekuensi
**Prioritas:** Must  
**Rilis:** MVP

Pengingat biasa maksimal satu kali dalam tujuh hari. Peringatan kritis dapat muncul lebih cepat.

### FR-TRASH-006 — Hapus Permanen
**Prioritas:** Must  
**Rilis:** MVP

Penghapusan permanen memerlukan konfirmasi.

### FR-TRASH-007 — Cache Tidak Masuk Recycle Bin
**Prioritas:** Must  
**Rilis:** MVP

Cache dan temporary file dibersihkan otomatis.

### FR-TRASH-008 — Konfigurasi Administrator
**Prioritas:** Should  
**Rilis:** Release 2

Nilai 30 hari, 2 GB, dan 15% dapat diubah melalui kebijakan administrator.

### FR-TRASH-009 — Pemisahan Local dan Account Recycle Bin
**Prioritas:** Must  
**Rilis:** MVP

Recycle Bin lokal mengelola draft/hasil lokal, sedangkan penghapusan file yang telah **Amankan ke Akun** mengikuti Account Recycle/retention policy server. Implementasi tidak boleh menganggap keduanya sebagai storage yang sama. Path lokal lengkap tidak disinkronkan ke audit/server.

---

## 7.11 Recovery, Retry, dan Resume

### FR-REC-001 — Simpan Checkpoint
**Prioritas:** Must  
**Rilis:** MVP

Proses panjang menyimpan checkpoint jika format dan engine mendukung.

### FR-REC-002 — Recovery Startup
**Prioritas:** Must  
**Rilis:** MVP

Saat aplikasi dibuka setelah berhenti tidak normal, sistem mendeteksi draft atau job belum selesai.

### FR-REC-003 — Pilihan Pemulihan
**Prioritas:** Must  
**Rilis:** MVP

User mendapat pilihan **Lanjutkan**, **Coba Lagi**, atau **Batalkan**.

### FR-REC-004 — Resume
**Prioritas:** Must  
**Rilis:** MVP

Proses dilanjutkan dari checkpoint terakhir jika didukung.

### FR-REC-005 — Fallback Retry
**Prioritas:** Must  
**Rilis:** MVP

Jika resume tidak didukung, sistem hanya mengulang tahap yang diperlukan tanpa menghilangkan edit user.

### FR-REC-006 — File Sumber Aman
**Prioritas:** Must  
**Rilis:** MVP

Kegagalan recovery tidak boleh merusak file sumber.

---

## 7.12 Undo dan Redo

### FR-UI-020 — Undo
**Prioritas:** Must  
**Rilis:** MVP

User dapat membatalkan perubahan terakhir selama sesi edit.

### FR-UI-021 — Redo
**Prioritas:** Must  
**Rilis:** MVP

User dapat mengembalikan perubahan yang baru dibatalkan.

### BR-UI-001 — Undo/Redo Bukan Versioning
**Prioritas:** Must  
**Rilis:** MVP

Undo/Redo hanya berlaku selama proses edit atau draft aktif dan berbeda dari versi file yang telah disimpan.

---

## 7.13 Versioning Terbatas

### FR-VER-001 — Maksimal Tiga Versi
**Prioritas:** Must  
**Rilis:** MVP

Untuk file yang diamankan ke akun, sistem menyimpan maksimal tiga versi terakhir.

### FR-VER-002 — Versi Keempat
**Prioritas:** Must  
**Rilis:** MVP

Saat versi keempat dibuat, versi paling lama dihapus atau dipindahkan sesuai kebijakan retensi.

### FR-VER-003 — Catatan Tidak Wajib
**Prioritas:** Must  
**Rilis:** MVP

User tidak diwajibkan menulis catatan perubahan.

### FR-VER-004 — Simpan atau Simpan Sebagai
**Prioritas:** Must  
**Rilis:** MVP

User dapat memilih **Simpan** atau **Simpan sebagai File Baru**.

### BR-VER-001 — Tidak Menggunakan Model Commit Lengkap
**Prioritas:** Must  
**Rilis:** MVP

Model commit lengkap seperti GitHub tidak digunakan pada tahap awal.


---

## 7.14 Scanner dan Visual Signature Lintas Perangkat

### FR-SCAN-001 — Scanner Kamera Mobile-First
**Prioritas:** Must  
**Rilis:** MVP

Scanner kamera diprioritaskan pada mobile/tablet. Alur minimum: capture, edge detection, crop/perspective correction, reorder multi-page, preview, dan simpan/proses lebih lanjut sesuai capability perangkat.

**Business Acceptance:**
- User dapat mengubah dokumen fisik menjadi input digital tanpa memerlukan desktop.

**UX Acceptance:**
- Aksi capture, retake, crop, reorder, dan selesai mudah dijangkau melalui touch.
- UI scanner tidak dipaksakan sebagai pola desktop yang dikecilkan.

### FR-SCAN-002 — Desktop Scan/OCR
**Prioritas:** Should  
**Rilis:** MVP

Desktop memprioritaskan impor gambar/PDF hasil scan dan OCR. Integrasi scanner fisik dapat ditambahkan apabila driver/library/platform disetujui.

### FR-SIGN-001 — Visual Signature Mobile-First
**Prioritas:** Must  
**Rilis:** MVP

Mobile/tablet mengutamakan drawing tanda tangan atau paraf menggunakan touch/stylus.

**UX Acceptance:**
- Canvas signature memiliki clear/undo, preview, resize/position setelah ditempatkan, dan tidak menyimpan asset tanpa pilihan user.

### FR-SIGN-002 — Visual Signature Desktop
**Prioritas:** Must  
**Rilis:** MVP

Desktop mendukung upload/paste gambar tanda tangan transparan dan saved signature yang disimpan aman. Drawing mouse tetap tersedia sebagai opsi tambahan; stylus/pen tablet digunakan bila perangkat mendukung.

### BR-SIGN-001 — Tidak Ada Digital Signature PKI pada Scope Saat Ini
**Prioritas:** Must  
**Rilis:** MVP

Sistem tidak mengimplementasikan certificate-based/PKI Digital Signature pada scope saat ini. Penambahan di masa depan memerlukan keputusan scope baru, integrasi resmi, security review, dan legal/compliance review.
---

### FR-SIGN-003 — Sign PDF Visual
**Prioritas:** Must  
**Rilis:** MVP

Sistem harus menyediakan tool **Sign PDF** untuk menempatkan tanda tangan visual/paraf/stamp yang diizinkan ke working state PDF dan menghasilkan PDF final setelah preview.

**Acceptance criteria:**
- Sign PDF tidak menggunakan certificate/PKI digital signature.
- User dapat memilih asset signature yang sah, menempatkan, memindahkan, dan mengubah ukuran sebelum finalisasi.
- Signature asset tidak masuk log/telemetry.
- File sumber tidak ditimpa otomatis.
- Final export mengikuti confirmation dan audit metadata yang berlaku.

---

## 7.15 PERTAMINA OTO Assistant

### FR-ASSIST-001 — Assistant-First Entry
**Prioritas:** Must  
**Rilis:** MVP

Sistem menyediakan Assistant-First sebagai jalur ketiga selain File-First dan Tool-First. Assistant tidak boleh menjadi satu-satunya cara mengakses tool.

### FR-ASSIST-002 — Offline Local Intent Router
**Prioritas:** Must  
**Rilis:** MVP

Perintah umum, alias fitur, dan workflow sederhana harus dapat dipetakan secara lokal tanpa AI server bila capability platform mendukung.

### FR-ASSIST-003 — Canonical Feature Registry
**Prioritas:** Must  
**Rilis:** MVP

Assistant hanya boleh memilih Feature ID yang terdaftar aktif di Feature Catalog dan tersedia bagi platform/permission user. Assistant dilarang membuat Feature ID atau tool baru saat runtime.

### FR-ASSIST-004 — Parameter Schema Validation
**Prioritas:** Must  
**Rilis:** MVP

Parameter hasil interpretasi Assistant harus divalidasi terhadap schema fitur sebelum tool dipanggil. Parameter tidak dikenal, format salah, atau nilai di luar batas harus ditolak/diperbaiki sebelum eksekusi.

### FR-ASSIST-005 — Multi-Step Workflow Plan
**Prioritas:** Must  
**Rilis:** MVP

Assistant dapat menyusun workflow multi-step dengan urutan yang valid. Sebelum eksekusi, sistem memvalidasi compatibility input/output, capability, permission, policy, temporary storage, network, dan server dependency.

### FR-ASSIST-006 — Plan Preview dan Safe Confirmation
**Prioritas:** Must  
**Rilis:** MVP

Workflow multi-step harus dapat ditampilkan sebagai rencana yang dapat dipahami user. User dapat mengoreksi parameter/langkah yang memang editable sebelum eksekusi. Aksi irreversible, overwrite, permanent delete, share, upload content, atau aksi sensitif tetap memerlukan confirmation sesuai Smart Communication rule.

### FR-ASSIST-007 — Local/Server AI Routing
**Prioritas:** Should  
**Rilis:** Release 2

Sistem memprioritaskan deterministic local routing. Optional local model dapat digunakan setelah approval/benchmark. Internal AI fallback hanya digunakan ketika reasoning tambahan benar-benar dibutuhkan dan policy/koneksi mengizinkan.

### FR-ASSIST-008 — Tool Execution Boundary
**Prioritas:** Must  
**Rilis:** MVP

Assistant tidak mengeksekusi arbitrary code, shell, direct SQL, unrestricted file-system operation, atau arbitrary API. Semua aksi dilakukan melalui application service/tool adapter resmi.

### FR-ASSIST-009 — Queue Until Online
**Prioritas:** Should  
**Rilis:** Release 2

Jika workflow membutuhkan service online yang tidak tersedia, Assistant dapat menyimpan job metadata yang aman untuk dilanjutkan saat koneksi kembali tanpa otomatis mengunggah isi dokumen sebelum consent/policy check.

### FR-ASSIST-010 — Contextual Help dan Error Guidance
**Prioritas:** Should  
**Rilis:** MVP

Assistant dapat menjelaskan fungsi tool, parameter, error, dan opsi recovery berdasarkan Feature Catalog/error code resmi tanpa mengarang kemampuan.

### FR-ASSIST-011 — Preset/Repeat Workflow
**Prioritas:** Should  
**Rilis:** Release 2

Workflow valid dapat disimpan sebagai preset atau diulang menggunakan service preset/history yang sama, bukan implementasi terpisah khusus Assistant.

### FR-ASSIST-012 — Assistant Progress & Recovery
**Prioritas:** Must  
**Rilis:** MVP

Workflow Assistant menggunakan Progress/Activity Center, Job ID/checkpoint/retry/resume yang sama dengan tool biasa. Intermediate output bukan file final sampai validasi selesai.

### FR-ASSIST-013 — Ambiguity & Confidence Handling
**Prioritas:** Must  
**Rilis:** MVP

Jika intent, file target, urutan langkah, atau parameter penting ambigu/tidak cukup yakin, Assistant harus meminta klarifikasi terarah atau menawarkan pilihan yang aman. Assistant dilarang menebak aksi destructive/sensitif.

### FR-ASSIST-014 — Tool Result Grounding
**Prioritas:** Must  
**Rilis:** MVP

Assistant hanya boleh menyatakan proses berhasil, gagal, atau menghasilkan nilai tertentu berdasarkan status/output aktual dari tool/application service. Assistant tidak boleh mengarang hasil eksekusi.

### FR-ASSIST-015 — Local Conversation State & Clear
**Prioritas:** Must  
**Rilis:** MVP

Conversation state disimpan lokal dan seminimal mungkin secara default. User dapat membersihkan percakapan. Penghapusan conversation state tidak menghapus enterprise audit metadata yang memang wajib dipertahankan. Secret, password PDF, token, clipboard sensitif, signature asset, dan isi dokumen penuh tidak boleh disimpan sebagai chat history otomatis.

---

## 7.16 Support & In-App Issue Reporting

### FR-SUP-001 — Lapor Masalah dari Aplikasi
**Prioritas:** Must  
**Rilis:** MVP

User dapat membuka aksi **Laporkan Masalah** langsung dari PERTAMINA OTO, termasuk secara kontekstual ketika suatu proses gagal.

### FR-SUP-002 — Form Pelaporan Minimal
**Prioritas:** Must  
**Rilis:** MVP

UI pelaporan hanya mewajibkan **pesan singkat**. Screenshot bersifat opsional. User tidak diwajibkan mengisi judul, kategori, prioritas, Feature ID, versi aplikasi, platform, error code, assignment, atau status.

### FR-SUP-003 — Screenshot, Preview, dan Markup Sederhana
**Prioritas:** Must  
**Rilis:** MVP

User dapat menekan tombol Screenshot untuk mengambil tampilan aplikasi yang relevan bila platform mendukung. Screenshot harus dipreview sebelum dikirim dan dapat diberi tanda sederhana seperti panah/lingkaran/highlight. Screenshot tidak boleh diunggah otomatis tanpa tindakan user.

### FR-SUP-004 — Safe Diagnostic Context Otomatis
**Prioritas:** Must  
**Rilis:** MVP

Sistem otomatis mengaitkan context yang aman seperti user reference, waktu, halaman/Feature ID, app version, platform/OS, Error Code, Job ID, Correlation ID, dan metadata diagnostik yang diizinkan. Isi dokumen, password, token, clipboard, signature asset, OCR text lengkap, atau secret tidak boleh dilampirkan otomatis.

### FR-SUP-005 — Superadmin Support Console
**Prioritas:** Must  
**Rilis:** MVP

Laporan masuk ke halaman khusus Superadmin/Support yang dilindungi permission. Superadmin dapat melihat laporan, screenshot, safe diagnostics, mengatur kategori/prioritas/status, menambah catatan internal, serta menandai duplicate, bukan bug, atau butuh informasi tambahan.

### FR-SUP-006 — Status untuk User
**Prioritas:** Should  
**Rilis:** MVP

User dapat melihat status laporannya sendiri dalam bentuk sederhana: Dikirim, Sedang diperiksa, Sedang ditindaklanjuti, atau Selesai. User tidak dibebani administrasi ticket internal.

### FR-SUP-007 — Export Laporan Perbaikan
**Prioritas:** Must  
**Rilis:** MVP

Superadmin dapat mengekspor detail satu issue ke PDF dan rekap issue terfilter ke Excel untuk diteruskan sebagai bahan tugas perbaikan programmer.

---

## 7.17 Intelligence PDF

### FR-INTEL-001 — AI Summarizer
**Prioritas:** Should  
**Rilis:** MVP

Sistem harus menyediakan ringkasan isi PDF menggunakan provider local atau enterprise-internal yang disetujui.

**Acceptance criteria:**
- User memilih dokumen dan memulai summarization secara eksplisit.
- Text layer digunakan langsung bila tersedia; OCR dapat menjadi dependency bila dokumen scan.
- Isi dokumen tidak dikirim ke public AI endpoint.
- Jika server/internal AI diperlukan, user/policy menerima pemberitahuan sesuai trust boundary.
- Hasil ditandai sebagai ringkasan berbantuan AI dan tidak mengganti dokumen sumber.

### FR-INTEL-002 — Translate PDF
**Prioritas:** Should  
**Rilis:** MVP

Sistem harus menyediakan penerjemahan konten PDF ke bahasa target yang didukung menggunakan provider yang disetujui.

**Acceptance criteria:**
- Bahasa sumber/target dapat dipilih atau dideteksi sesuai provider.
- Konten sensitif mengikuti policy dan data-minimization.
- Hasil terjemahan dipreview sebelum final export.
- Layout preservation dinyatakan sesuai capability; sistem tidak boleh menjanjikan fidelity yang tidak dapat dipenuhi.

### FR-INTEL-003 — PDF to Markdown
**Prioritas:** Should  
**Rilis:** MVP

Sistem harus mengekstrak struktur teks PDF menjadi Markdown.

**Acceptance criteria:**
- PDF dengan text layer diproses deterministik secara lokal bila memungkinkan.
- Dokumen scan dapat menggunakan OCR sebagai dependency.
- Heading/list/table dipertahankan sejauh dapat dikenali.
- Output dapat dipreview dan disimpan sebagai `.md`.
- Tidak memerlukan AI jika deterministic extraction sudah cukup.

---
# 8. Kebutuhan UI/UX

### UIR-UI-001 — Responsive
Sistem responsif pada mobile, tablet, laptop, dan desktop.

### UIR-UI-002 — Tidak Overflow
Tidak boleh ada konten terpotong atau overflow pada ukuran layar yang didukung.

### UIR-UI-003 — Progress
Proses panjang menampilkan persentase, tahap, jumlah item, estimasi waktu jika tersedia, dan opsi batal jika aman.

### UIR-UI-004 — Smart Communication
Tindakan rutin menggunakan snackbar dan Undo. Konfirmasi hanya untuk upload, overwrite, hapus permanen, share, final export, dan tindakan berisiko.

### UIR-UI-005 — Triple Entry Flow
Sistem menyediakan File-First, Tool-First, dan Assistant-First. Ketiga jalur harus menuju capability/tool yang sama dan tidak membuat business rule paralel.

### UIR-UI-006 — Capability Adaptive
Sistem hanya menampilkan fitur yang didukung platform dan permission.

### UIR-UI-007 — Mode Tampilan
User dapat memilih ikon sangat besar, besar, sedang, kecil, list, detail, tiles, content, preview pane, dan details pane.

### UIR-UI-008 — Adaptive UX
Sistem menyesuaikan pola interaksi berdasarkan perangkat dan capability. Responsive layout tidak boleh ditafsirkan sebagai UI identik pada mobile, tablet, desktop, dan web.

### UIR-UI-009 — Platform-Natural Interaction
Mobile memprioritaskan touch, camera, scan, dan drawing; desktop memprioritaskan drag-and-drop, keyboard shortcut, batch, file besar, dan multi-panel.

### UIR-UI-010 — Capability Fallback
Jika capability lokal tidak tersedia, sistem harus menyembunyikan fitur yang benar-benar tidak dapat digunakan atau menawarkan alternatif/fallback server yang disetujui dengan pemberitahuan yang jelas.

### UIR-UI-011 — Assistant Plan Clarity
Untuk workflow multi-step, Assistant menampilkan ringkasan langkah, file yang terlibat, output yang diharapkan, kebutuhan server jika ada, dan aksi berisiko sebelum eksekusi.

### UIR-UI-012 — Assistant Offline State
Saat offline, UI Assistant membedakan dengan jelas aksi yang dapat dijalankan lokal, aksi yang dapat diantrikan, dan aksi yang benar-benar memerlukan koneksi.

### UIR-UI-013 — Minimal Issue Reporting
Pelaporan masalah harus dapat diselesaikan dengan alur sesingkat mungkin: **tulis pesan → screenshot bila perlu → kirim**. Tidak boleh ada form teknis panjang untuk user.

### UIR-UI-014 — Screenshot Consent & Preview
Screenshot laporan selalu dipreview sebelum dikirim. Sistem tidak boleh mengambil dan mengunggah screenshot secara diam-diam.

### UIR-L10N-001 — Bahasa
Sistem mendukung Bahasa Indonesia dan English.

### UIR-L10N-002 — Fallback
Fallback bahasa adalah Bahasa Indonesia.

### UIR-L10N-003 — Key-Based Resource
Semua teks UI menggunakan localization resource key.

---

# 9. Kebutuhan Nonfungsional

## 9.1 Kinerja

- `NFR-PERF-001`: Fast Gate tidak boleh memblokir UI.
- `NFR-PERF-002`: Structural validation berjalan asynchronous.
- `NFR-PERF-003`: File besar tidak dibaca seluruhnya ke memory jika streaming dapat digunakan.
- `NFR-PERF-004`: Thumbnail menggunakan lazy loading.
- `NFR-PERF-005`: Audit sync tidak menghambat pekerjaan user.
- `NFR-PERF-006`: Target waktu respons ditetapkan pada Test Plan setelah benchmark.
- `NFR-PERF-007`: Setiap fitur memiliki Server Resource Budget: Processing Mode (`Local/Server/Hybrid`), Server Dependency (`None/Optional/Required`), Storage Impact (`None/Metadata/File`), Network Impact (`None/Low/Medium/High`), dan Concurrency Risk (`Low/Medium/High`).
- `NFR-PERF-008`: Server memprioritaskan metadata dan fungsi enterprise; draft/working state/cache/thumbnail/undo-redo tidak disinkronkan penuh.
- `NFR-PERF-009`: API/job berat menggunakan batching, asynchronous/background processing, queue/backpressure, pagination/lazy loading, retention/cleanup, quota, dan capacity check sesuai kebutuhan.
- `NFR-PERF-010`: Load/performance test wajib memverifikasi CPU, RAM, storage, database, bandwidth, queue, dan concurrency sebelum peningkatan skala pengguna.
- `NFR-PERF-011`: Assistant command umum harus diprioritaskan ke local deterministic routing agar tidak menambah AI/API request yang tidak perlu.
- `NFR-PERF-012`: Internal AI fallback wajib memiliki timeout, concurrency control, quota/rate policy, queue/backpressure bila diperlukan, dan graceful fallback.
- `NFR-PERF-013`: Multi-step workflow melakukan preflight terhadap temporary storage dan cumulative resource requirement sebelum dijalankan.

## 9.2 Keamanan

- `SR-SEC-001`: Semua komunikasi menggunakan HTTPS.
- `SR-SEC-002`: Token disimpan di secure storage.
- `SR-SEC-003`: Password PDF tidak boleh disimpan.
- `SR-SEC-004`: Isi dokumen tidak boleh masuk log atau telemetry.
- `SR-SEC-005`: File server harus melalui access control.
- `SR-SEC-006`: File backup dienkripsi saat transit dan tersimpan.
- `SR-SEC-007`: Administrator tidak otomatis berhak membaca isi file.
- `SR-SEC-008`: Library pihak ketiga melalui license dan vulnerability review.
- `SR-SEC-009`: VA dan penetration test wajib sebelum Production.
- `SR-SEC-010`: Secret tidak boleh disimpan pada source code.
- `SR-SEC-011`: Asset tanda tangan visual tidak boleh masuk log, audit, telemetry, atau sinkronisasi tanpa pilihan user dan kontrol keamanan yang disetujui.
- `SR-SEC-012`: Digital Signature berbasis sertifikat/PKI tidak boleh diimplementasikan tanpa perubahan scope resmi dan review security/legal/compliance.
- `SR-SEC-013`: Assistant hanya dapat melakukan tool invocation melalui canonical registry/application service; direct SQL, shell, arbitrary code execution, unrestricted file access, dan arbitrary API tidak diizinkan.
- `SR-SEC-014`: Isi dokumen, OCR text, atau embedded instruction diperlakukan sebagai untrusted data dan tidak dapat mengubah system policy/tool permission.
- `SR-SEC-015`: Prompt/chat content tidak disinkronkan atau dikirim ke AI server secara default; audit hanya mencatat metadata yang diperlukan seperti intent category, Feature ID, status, dan correlation ID.
- `SR-SEC-016`: Jika AI internal memerlukan content, sistem wajib melakukan policy/classification/permission check, data minimization, dan consent/pemberitahuan yang sesuai sebelum transfer.
- `SR-SEC-017`: Assistant conversation state tidak boleh menyimpan password PDF, token, secret, clipboard sensitif, atau signature asset.
- `SR-SEC-018`: Tool plan dan parameter harus divalidasi server/client-side sesuai trust boundary sebelum eksekusi; output AI tidak pernah dianggap trusted command.
- `SR-SEC-019`: Screenshot issue tidak boleh dikirim otomatis; user harus melihat preview dan melakukan tindakan Kirim.
- `SR-SEC-020`: Superadmin/Support hanya dapat mengakses issue sesuai permission dan tidak memperoleh hak otomatis membaca dokumen user di luar lampiran yang memang dikirim user.
- `SR-SEC-021`: PDF Intelligence tidak boleh mengirim isi dokumen ke public AI/translation endpoint. Local atau enterprise-internal provider harus melalui policy, permission, data-minimization, dan security review.
- `SR-SEC-022`: Unlock PDF hanya boleh dilakukan dengan password/otorisasi yang sah; produk tidak menyediakan mekanisme untuk membypass enkripsi/proteksi tanpa otorisasi.
- `SR-SEC-023`: Redaction final harus menghapus/meratakan konten sensitif sesuai kemampuan engine, bukan sekadar menutup tampilan dengan overlay; preview dan final confirmation wajib.


## 9.3 Keandalan

- `NFR-REC-001`: File sumber tidak boleh rusak karena kegagalan.
- `NFR-REC-002`: Output dianggap final hanya setelah validasi.
- `NFR-REC-003`: Temporary file tidak dianggap hasil final.
- `NFR-REC-004`: Job memiliki correlation ID.
- `NFR-REC-005`: Retry dibatasi agar tidak loop tanpa akhir.

## 9.4 Kemudahan Pemeliharaan

- `NFR-ARCH-001`: Backend menggunakan Clean Architecture.
- `NFR-ARCH-002`: Web UI tidak mengakses SQL Server langsung.
- `NFR-ARCH-003`: Integrasi client melalui Web API.
- `NFR-ARCH-004`: Web UI dan Web API dipisahkan dalam deployment.
- `NFR-ARCH-005`: Domain dan business rule tidak bergantung pada UI framework.

---

# 10. Kebutuhan Data

- `DR-DB-001`: Database utama menggunakan Microsoft SQL Server.
- `DR-DB-002`: ORM menggunakan Entity Framework Core.
- `DR-DB-003`: Database menyimpan metadata dan konfigurasi; isi dokumen hanya jika disetujui.
- `DR-DB-004`: File backup menggunakan file/object storage internal yang disetujui.
- `DR-DB-005`: Metadata backup memuat File ID, Owner, nama, extension, MIME, ukuran, storage key, waktu, versi, dan retensi.
- `DR-DB-006`: Audit memiliki indeks untuk user, waktu, fitur, status, dan Process ID.
- `DR-DB-007`: Issue report menyimpan metadata/status/catatan di SQL Server; screenshot/lampiran menggunakan secure file/object storage yang disetujui, bukan blob besar di tabel SQL secara default.

---

# 11. Kebutuhan API

- `IR-API-001`: Web UI menggunakan ASP.NET Core Web API.
- `IR-API-002`: API menggunakan autentikasi dan authorization Pertamina.
- `IR-API-003`: API upload mendukung batas ukuran dan streaming.
- `IR-API-004`: API job mengembalikan Job ID.
- `IR-API-005`: API error menggunakan format standar berisi code, message, dan correlationId.
- `IR-API-006`: API tidak mengembalikan stack trace kepada user.
- `IR-API-007`: API mendukung health check.
- `IR-API-008`: API mencatat telemetry tanpa isi dokumen.
- `IR-API-009`: Jika internal AI fallback digunakan, akses dilakukan melalui service/API internal yang disetujui, bukan direct external/public AI endpoint dari client.
- `IR-API-010`: Assistant tool invocation menggunakan contract/schema terversi dan correlation ID; perubahan schema tidak boleh memutus preset/workflow tanpa compatibility handling.
- `IR-API-011`: API menyediakan submission issue report, status issue milik user, dan operasi Superadmin Support Console sesuai permission.
- `IR-API-012`: Upload screenshot/lampiran issue menggunakan limit, validation, access control, dan storage key yang aman.

---

# 12. DevOps dan Lingkungan

- `IR-DEVOPS-001`: Source code disimpan pada Azure DevOps Git.
- `IR-DEVOPS-002`: Build dan deployment menggunakan Azure DevOps Pipeline.
- `IR-DEVOPS-003`: Pull Request wajib melalui review.
- `IR-DEVOPS-004`: Pipeline menjalankan restore, build, unit test, security scan, dan artifact creation.
- `NFR-ENV-001`: Environment minimum Development, QA/Testing, dan Production.
- `NFR-ENV-002`: Production hanya menerima artifact dari pipeline.
- `NFR-ENV-003`: Debug nonaktif di Production.
- `NFR-ENV-004`: Data Production tidak digunakan di Development tanpa masking dan approval.

---

# 13. Pemantauan

- `NFR-MON-001`: Application Insights/APM memonitor request duration, error rate, job duration, queue, transfer failure, database, CPU, memory, storage, version, dan correlation ID.
- `NFR-MON-002`: Monitoring tidak merekam isi dokumen, password, token, clipboard, atau teks OCR lengkap.
- `NFR-MON-003`: Critical error menghasilkan alert sesuai kebijakan support.
- `NFR-MON-004`: Issue report dapat mengaitkan safe diagnostic metadata/log window yang relevan melalui Error Code/Job ID/Correlation ID tanpa merekam isi dokumen atau secret.

---

# 14. Tata Kelola

- `BR-GOV-001`: Pengembangan mengikuti proses RIC dan persetujuan Pertamina.
- `BR-GOV-002`: Perubahan scope material menggunakan change request.
- `BR-GOV-003`: Traceability: Requirement → Feature → Code → Unit Test → Test Case → UAT → Release.
- `BR-GOV-004`: Teknologi tambahan memerlukan review arsitektur, security, dan lisensi.
- `BR-GOV-005`: Rollout lintas Region mengikuti persetujuan Fungsi Owner dan tata kelola resmi.
- `BR-GOV-006`: Fitur dengan dependency/engine berisiko tinggi wajib melewati Proof-of-Capability/Technical Feasibility Gate sebelum coding skala penuh. Gate minimum memverifikasi kualitas output, format support, license, security, platform support, resource usage, maintainability, dan fallback.
- `BR-GOV-007`: Kandidat awal feasibility gate meliputi PDF content editing, Office↔PDF, PDF→Office, OCR tabel kompleks, background removal, HTML/Web→PDF, PDF/A, physical scanner integration, dan optional local AI model.
- `BR-GOV-008`: **Primary Feature Set First.** Core Feature Set berisi 33 Primary PDF Tools dan didahulukan dari user-tool sekunder. Di dalam Core Set, sequencing menggunakan Common-Simple-First/vertical slice; fitur kompleks tetap melewati feasibility gate.
- `BR-GOV-009`: Secondary user-tool implementation baru dimulai setelah seluruh Core Feature Set berstatus `DONE`, `APPROVED_WITH_LIMITATION`, atau memiliki keputusan `BLOCKED_ALTERNATIVE_REQUIRED/DEFERRED` yang disetujui. Foundation, security, auth, audit, storage, observability, dan platform dependency tidak dianggap secondary user-tool.

---

# 15. Dokumentasi dan Serah Terima

- `NFR-DOC-001`: Setiap modul memiliki dokumentasi teknis.
- `NFR-DOC-002`: API memiliki dokumentasi endpoint.
- `NFR-DOC-003`: Database memiliki ERD dan data dictionary.
- `NFR-DOC-004`: Security control didokumentasikan.
- `NFR-HAND-001`: Handover mencakup source code, build, deployment, manual, test evidence, UAT, incident guide, cartography, knowledge sharing, dan side-by-side maintenance.

---

# 16. Kriteria Penerimaan Global

Aplikasi siap Production apabila:

1. Seluruh requirement Must MVP selesai.
2. Unit test lulus.
3. Functional dan integration test lulus.
4. **Business Process Acceptance** lulus untuk alur utama.
5. **UX Acceptance** lulus pada platform/capability yang ditargetkan.
6. UAT disetujui.
7. Security & privacy review selesai.
8. Temuan Critical/High ditangani.
9. Performance test memenuhi target termasuk **Server Resource Budget** dan concurrency.
10. Error, recovery, retry, resume, backup, dan restore diuji.
11. Cross-platform/capability behavior diuji untuk fitur yang relevan.
12. Monitoring aktif dan tidak merekam data sensitif.
13. Dokumentasi selesai.
14. Deployment dan rollback diuji.
15. Persetujuan Production diperoleh.

Build berhasil atau unit test hijau **tidak cukup** untuk menyatakan fitur maupun aplikasi siap Production.

---

# 17. Definition of Done untuk Codex

Sebuah fitur hanya boleh diberi status **DONE** setelah delapan lapisan berikut lulus:

### 17.1 Ketepatan Teknis
```text
[ ] Requirement ID disebutkan
[ ] Code mengikuti Clean Architecture
[ ] Validasi input tersedia
[ ] Error code tersedia
[ ] Build berhasil
[ ] Unit/integration test teknis yang relevan lulus
[ ] Tidak ada perubahan di luar scope
```

### 17.2 Ketepatan Proses Bisnis
```text
[ ] Business outcome sesuai Business Process
[ ] Alur utama dan alternate/error flow sesuai requirement
[ ] Tidak ada shortcut teknis yang mengubah business rule
```

### 17.3 Penerimaan UI/UX
```text
[ ] UX Acceptance terpenuhi
[ ] Responsive dan Adaptive UX tervalidasi
[ ] Tidak ada overflow
[ ] Tidak ada dialog/langkah yang tidak perlu
[ ] Progress/feedback jelas untuk proses panjang
[ ] Alur natural sesuai platform
```

### 17.4 Keamanan dan Privasi
```text
[ ] Tidak ada secret pada source code
[ ] Logging/telemetry aman
[ ] Isi dokumen, clipboard, password, token, OCR penuh, dan signature asset tidak bocor
[ ] Permission dan upload consent sesuai requirement
```

### 17.5 Kinerja/Anggaran Server
```text
[ ] Processing Mode sesuai desain
[ ] Server Dependency tidak dinaikkan tanpa alasan
[ ] Storage/Network/Concurrency Risk sesuai budget
[ ] Tidak ada whole-file memory load jika streaming lebih tepat
[ ] Background/batching/pagination/cleanup diterapkan bila relevan
```

### 17.6 Error dan Pemulihan
```text
[ ] Error condition diuji
[ ] File sumber tetap aman
[ ] Retry/resume/recovery diuji bila relevan
[ ] Partial/temporary output tidak dianggap final
```

### 17.7 Perilaku Lintas Platform
```text
[ ] Capability platform diverifikasi
[ ] Fallback/hidden state benar saat capability tidak tersedia
[ ] Mobile/Desktop/Web mengikuti Adaptive UX yang ditetapkan
[ ] Assistant capability/offline state tidak menjanjikan fungsi yang tidak tersedia
```

### 17.8 Pengujian dan Dokumentasi
```text
[ ] Functional Acceptance lulus
[ ] Business Acceptance lulus
[ ] UX Acceptance lulus
[ ] Regression test relevan lulus
[ ] Dokumentasi diperbarui
[ ] Traceability Requirement → Feature → Code → Test → UAT → Release tersedia
[ ] Untuk Assistant: canonical Feature ID, parameter schema, tool-plan validation, dan security boundary diuji
```

Kegagalan satu lapisan berarti fitur **belum DONE**, meskipun build dan unit test berhasil.

---

# 18. Template Task Codex

```text
TASK ID:
OTO-XXX

REQUIREMENT:
FR-XXX-001 sampai FR-XXX-005

TUJUAN:
Jelaskan hasil yang harus dibuat.

SCOPE:
- File/folder yang boleh diubah.
- Endpoint yang dibuat.
- UI yang dibuat.
- Test yang dibuat.

NON-SCOPE:
- Modul yang tidak boleh diubah.
- Business rule yang tidak boleh diubah.

TECHNOLOGY:
- C#
- ASP.NET Core
- Entity Framework Core
- SQL Server
- Clean Architecture

ACCEPTANCE:
- Functional Acceptance
- Business Acceptance
- UX Acceptance

SERVER RESOURCE BUDGET:
- Processing Mode
- Server Dependency
- Storage Impact
- Network Impact
- Concurrency Risk

VALIDATION:
- dotnet restore
- dotnet build
- dotnet test
- business flow validation
- UX/capability validation
- error/recovery validation bila relevan
- server resource validation bila relevan
- assistant/tool-plan/schema validation bila task melibatkan Assistant

OUTPUT:
- Ringkasan perubahan
- Daftar file
- Hasil test
- Risiko atau blocker
```

---

# 19. Matriks Ketertelusuran Awal

| Keputusan | Requirement utama |
|---|---|
| Dua tingkat requirement | Bagian 4 |
| Platform C#/.NET | NFR-ARCH-001 s.d. 005 |
| Multi-bahasa | UIR-L10N-001 s.d. 003 |
| ID requirement | Bagian 4 |
| MoSCoW | Bagian 5 |
| SSO adaptif | FR-AUTH-001 s.d. 004 |
| Validasi bertingkat | FR-FILE-010 s.d. 015 |
| Hybrid otomatis | FR-PROC-001 s.d. 005 |
| Local-first storage | FR-STOR-001 s.d. 006 |
| Local-First Workspace + Selective Cross-Device Sync | FR-BACKUP-001 s.d. 010 |
| Riwayat dan audit terpisah | FR-HIST-001 s.d. 003; FR-AUDIT-001 s.d. 004 |
| Recycle Bin | FR-TRASH-001 s.d. 008 |
| Retry dan resume | FR-REC-001 s.d. 006 |
| Maksimal 3 versi | FR-VER-001 s.d. 004 |
| Adaptive UX | UIR-UI-008 s.d. 010 |
| Scanner mobile-first | FR-SCAN-001 s.d. 002 |
| Visual signature mobile-first | FR-SIGN-001 s.d. 002; BR-SIGN-001 |
| Server Resource Budget | NFR-PERF-007 s.d. 010 |
| Acceptance berlapis | Bagian 16 dan 17 |
| Definition of Done 8 lapis | Bagian 17 |
| Digital Signature di luar scope | BR-SIGN-001; SR-SEC-012 |
| PERTAMINA OTO Assistant Offline-First Hybrid | FR-ASSIST-001 s.d. 015 |
| Triple Entry Flow | UIR-UI-005; FR-ASSIST-001 |
| Canonical Tool Orchestration | FR-ASSIST-003 s.d. 008; IR-API-010 |
| Assistant Privacy & Prompt-Injection Boundary | SR-SEC-013 s.d. 018 |
| Assistant server-efficiency | NFR-PERF-011 s.d. 013 |
| Technical Feasibility Gate | BR-GOV-006 s.d. 007 |
| Desktop-First + Curated Phone/Tablet | DEC-REQ-002; UIR-UI-006, 008, 009, 010 |
| In-App Issue Reporting | FR-SUP-001 s.d. 007; UIR-UI-013 s.d. 014; SR-SEC-019 s.d. 020 |
| Common-Simple-First Implementation Sequencing | DEC-REQ-028; BR-GOV-008 |
| Primary PDF Tool Surface / Core Feature Set | DEC-REQ-029; UIR-UI-015..016; BR-GOV-008..009 |
| PDF Intelligence | DEC-REQ-030; FR-INTEL-001..003; SR-SEC-021 |
| Sign PDF visual non-PKI | FR-SIGN-003; SR-SEC-011..012 |

---

# 20. Status Dokumen

```text
Nama dokumen : 02_REQUIREMENT_SPECIFICATION.md
Versi        : 1.5
Status       : FINAL — PRE-CODING BASELINE
Keputusan    : 30 keputusan baseline
Progress     : 100%
```

Dokumen berikutnya:

```text
03_FEATURE_CATALOG.md
```
