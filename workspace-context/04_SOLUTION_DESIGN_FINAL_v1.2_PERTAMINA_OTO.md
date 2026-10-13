# 04 — DESAIN SOLUSI
## PERTAMINA OTO — Office Tools Organizer

> **Standar bahasa:** Bahasa Indonesia — ID-1.0

> **Status: FINAL — LOCKED PRE-CODING SOLUTION BASELINE**

---

## 1. Informasi Dokumen

| Atribut | Nilai |
|---|---|
| Nama dokumen | `04_SOLUTION_DESIGN.md` |
| Nama proyek | **PERTAMINA OTO — Office Tools Organizer** |
| Versi | **1.2** |
| Status | **FINAL — LOCKED PRE-CODING SOLUTION BASELINE** |
| Parent Business Process | `01_BUSINESS_PROCESS_FINAL_v3.4_PERTAMINA_OTO.md` |
| Parent Requirement | `02_REQUIREMENT_SPECIFICATION_FINAL_v1.5_PERTAMINA_OTO.md` |
| Parent Feature Catalog | `03_FEATURE_CATALOG_FINAL_v1.3_PERTAMINA_OTO.md` |
| Keputusan Solution Design | **18 dari 18 dikunci** |
| Arsitektur | **Modular Monolith + Clean Architecture + Separate Deployables** |
| Primary Client | **Windows Desktop C#/.NET** |
| Mobile/Tablet | **Curated companion experience** |
| Web | **Enterprise/browser channel** |
| Backend | **ASP.NET Core Web API** |
| Database | **Microsoft SQL Server + EF Core** |
| Processing | **Offline-First, Local-First capability-aware, Hybrid Execution** |
| Monitoring | **Structured Logging + Correlation + Application Insights/APM** |
| Realtime | **REST API + SignalR events + REST refresh fallback** |

### 1.1 Riwayat Revisi

| Versi | Perubahan |
|---|---|
| 1.0 | Finalisasi 18 keputusan Solution Design, audit konsistensi 01–03, penambahan In-App Issue Reporting, Desktop-First curated mobile, dan penguncian arsitektur pre-coding |
| 1.1 | Controlled revision: menambahkan Common-Simple-First + Vertical Slice Development Strategy untuk urutan implementasi tanpa mengubah 18 architecture decision yang telah dikunci |
| 1.2 | Primary Feature Audit: 33 Core Tool Cards menjadi Primary Tool Surface; menambahkan ContentIntelligence module/provider dan mengubah sequencing menjadi Primary-Feature-Set-First tanpa mengubah 18 architecture locks |

---

## 2. Tujuan Dokumen

Dokumen ini menerjemahkan Business Process, Requirement Specification, dan Feature Catalog menjadi struktur implementasi yang:

- production-ready;
- maintainable;
- clean dan modular;
- aman;
- Offline-First dan Local-First sejauh capability memungkinkan;
- hemat resource server;
- mudah diuji;
- mudah dikembangkan oleh programmer/Codex;
- mudah dipindahkan dari satu server ke beberapa server;
- tidak mengunci produk ke satu vendor/library;
- tidak membebani user dengan detail teknis.

Dokumen ini **belum memilih library PDF/OCR/conversion final**. Pemilihan engine berisiko tinggi tetap melalui Proof-of-Capability/Feasibility Gate.

---

# 3. Hasil Audit Sebelum Penguncian

## 3.1 Ringkasan Audit

Audit dilakukan terhadap:

- 18 keputusan Solution Design;
- Business Process v3.2;
- Requirement Specification v1.2;
- Feature Catalog v1.0;
- keputusan scope Digital Signature;
- Assistant;
- Local-First Workspace;
- Selective Cross-Device Sync;
- server resource budget;
- Definition of Done 8 lapis.

Hasil:

| Pemeriksaan | Hasil |
|---|---|
| Konflik fatal antar-keputusan | **Tidak ada** |
| Bertentangan dengan Digital Signature scope | **Tidak** |
| Bertentangan dengan Offline-First/Local-First | **Tidak** |
| Bertentangan dengan server-efficiency | **Tidak** |
| Bertentangan dengan Assistant security boundary | **Tidak** |
| Bertentangan dengan Clean Architecture | **Tidak** |
| Keputusan yang membutuhkan revisi parent docs | **2 area material** |
| Status akhir | **AMAN UNTUK DIKUNCI SEBAGAI SOLUTION BASELINE** |

Dua area yang memerlukan controlled upstream revision adalah:

1. **Desktop-First + Curated Phone/Tablet** menggantikan framing lama bahwa Responsive Web adalah kanal utama.
2. **In-App Issue Reporting** adalah product capability baru yang belum tercatat sebagai fitur canonical lengkap.

Karena itu audit menghasilkan:

- `01_BUSINESS_PROCESS_FINAL_v3.3_PERTAMINA_OTO.md`
- `02_REQUIREMENT_SPECIFICATION_FINAL_v1.3_PERTAMINA_OTO.md`
- `03_FEATURE_CATALOG_FINAL_v1.1_PERTAMINA_OTO.md`

Tidak ada keputusan lama yang dihapus diam-diam.

## 3.2 Guard Ruang Lingkup yang Tetap Dikunci

- Digital Signature berbasis certificate/PKI **tetap di luar scope**.
- Visual Signature / Paraf non-PKI tetap ada.
- Assistant bukan processing engine.
- Draft, working state, undo/redo, cache, thumbnail, dan temp tidak auto-sync penuh.
- Amankan ke Akun tetap selective.
- Account file version maksimum tiga versi.
- File original tidak ditimpa diam-diam.
- Server tidak menjadi default engine untuk semua pekerjaan.

---

# 4. Register Penguncian Keputusan — 18/18

| No | Keputusan yang Dikunci | Status |
|---:|---|---|
| 1 | **Modular Monolith + Clean Architecture + Separate Deployables** | LOCKED |
| 2 | **Contract + Application Interface + Internal Event** untuk komunikasi antar-modul | LOCKED |
| 3 | **Satu SQL Server Database + Schema/Data Ownership per Module** | LOCKED |
| 4 | **Hybrid Storage: Local Workspace + SQL Metadata + Secure File/Object Storage** | LOCKED |
| 5 | **Engine Adapter/Provider + Engine Resolver** | LOCKED |
| 6 | **Hybrid Execution: Immediate untuk ringan + Background Job untuk berat** | LOCKED |
| 7 | **Adaptive Progress + Activity Center + No Silent Wait** | LOCKED |
| 8 | **Operation-Based Working State + Autosave + Checkpoint** | LOCKED |
| 9 | **Typed Canonical Feature Registry + Configurable Enterprise Metadata** | LOCKED |
| 10 | **Desktop-First + Curated Phone/Tablet + Capability-Aware Runtime** | LOCKED |
| 11 | **Shared Core + Platform-Specific UI/Adapter** | LOCKED |
| 12 | **Selective Cross-Device Sync melalui Amankan ke Akun** | LOCKED |
| 13 | **Hybrid Local Processing: In-Process ringan + Isolated Local Worker berat** | LOCKED |
| 14 | **Layered Configuration + Enterprise Policy + Feature Flags** | LOCKED |
| 15 | **Logical Separation + Flexible Physical Deployment** | LOCKED |
| 16 | **Structured Logging/Correlation + In-App Issue Reporting + Superadmin Support Console** | LOCKED |
| 17 | **Controlled Compatibility + Version Handshake + Minimum Supported Version** | LOCKED |
| 18 | **REST API + SignalR untuk event realtime + REST refresh fallback** | LOCKED |

Perubahan terhadap keputusan di atas setelah coding dimulai wajib melalui impact analysis/change control.

---

# 5. Arsitektur Tingkat Tinggi

```text
                         PERTAMINA OTO
                               │
        ┌──────────────────────┼──────────────────────┐
        │                      │                      │
        ▼                      ▼                      ▼
 WINDOWS DESKTOP        MOBILE / TABLET         RESPONSIVE WEB
 Primary Full Client    Curated Companion       Enterprise/Browser
        │                      │                      │
        └──────────────┬───────┴──────────────┬──────┘
                       │                      │
                       ▼                      ▼
                 Shared Contracts       ASP.NET Core Web API
                       │                      │
                       └──────────┬───────────┘
                                  ▼
                         Application Modules
                                  │
             ┌────────────────────┼────────────────────┐
             ▼                    ▼                    ▼
        Local Engines        Server Worker       Enterprise Services
             │                    │                    │
             ▼                    ▼                    ▼
       Local Workspace       Secure Temp        SQL + Secure Storage
```

Prinsip:

- Business rules tidak berada di UI.
- Client tidak mengakses SQL Server langsung.
- Assistant memanggil capability resmi melalui registry/application service.
- Engine vendor dibungkus adapter/provider.
- User memilih pekerjaan, bukan engine.

---

# 6. Struktur Solution / Repository

Struktur konseptual:

```text
pertamina-oto/
├── src/
│   ├── BuildingBlocks/
│   │   ├── Pertamina.Oto.Contracts
│   │   ├── Pertamina.Oto.Security
│   │   ├── Pertamina.Oto.Observability
│   │   ├── Pertamina.Oto.Storage.Abstractions
│   │   └── Pertamina.Oto.Processing.Abstractions
│   │
│   ├── Modules/
│   │   ├── Pdf
│   │   ├── Conversion
│   │   ├── Image
│   │   ├── ScannerOcr
│   │   ├── SignatureAnnotation
│   │   ├── FileProductivity
│   │   ├── Workspace
│   │   ├── AccountAdministration
│   │   ├── Audit
│   │   ├── Support
│   │   └── Assistant
│   │
│   └── Hosts/
│       ├── Pertamina.Oto.Web
│       ├── Pertamina.Oto.Api
│       ├── Pertamina.Oto.Worker
│       ├── Pertamina.Oto.Desktop
│       └── Pertamina.Oto.Mobile
│
└── tests/
    ├── Unit
    ├── Integration
    ├── Architecture
    ├── Contract
    ├── E2E
    └── Performance
```

`Pertamina.Oto.Mobile` menyatakan host mobile/companion secara konseptual. Framework client mobile final tetap tunduk pada approval arsitektur/security dan tidak boleh keluar dari baseline C#/.NET tanpa keputusan baru.

---


## 6.1 Strategi Urutan Pengembangan — Primary Feature Set First

Pengembangan user-facing menggunakan dua aturan sekaligus:

1. **Primary Feature Set First:** 33 Core PDF Tools diselesaikan sebelum user-tool sekunder.
2. **Common-Simple-First di dalam Core Set:** pilih urutan teknis yang paling reusable/low-risk tanpa mengabaikan PoC untuk fitur kompleks.

```text
Foundation minimum reusable
        ↓
Merge PDF vertical slice
        ↓
Core PDF low-risk tools
        ↓
Core conversion/OCR/edit/security tools
        ↓
Core PDF Intelligence
        ↓
Secondary user tools
```

Foundation/security/auth/audit/storage/observability/platform capability dapat dibangun kapan pun dibutuhkan sebagai dependency dan tidak dianggap secondary user-tool.

Blocked Core item tidak harus menghentikan seluruh Core program. Item lain boleh berjalan, tetapi secondary user-tool baru dimulai setelah blocker mempunyai keputusan resmi (`APPROVED_WITH_LIMITATION`, `BLOCKED_ALTERNATIVE_REQUIRED`, atau `DEFERRED` yang disetujui).

## 6.2 Permukaan Tool Utama

Tool-First menampilkan tujuh kelompok utama:
- Organize PDF;
- Optimize PDF;
- Convert to PDF;
- Convert from PDF;
- Edit PDF;
- PDF Security;
- PDF Intelligence.

Terdapat **33 Core Tool Cards**. Satu card dapat menunjuk satu canonical Feature ID atau workflow bundle. Card bukan alasan membuat engine/service duplikat.

Contoh:
- `Organize PDF` → workspace yang mengorkestrasi reorder/move/rotate/remove/extract/duplicate/add-page.
- `Edit PDF` → workspace yang mengorkestrasi add/edit text, image, shape, overlay, dan capability edit lain.
- `PDF Forms` → bundle fill/create/field/flatten capabilities.
- `Sign PDF` → `FEAT-SIGN-013`, visual signature/paraf non-PKI.
---

# 7. Batas Modul dan Komunikasi

## 7.1 Aturan Modul

Setiap modul memiliki:

```text
Domain
Application
Contracts
Infrastructure
Tests
```

Larangan:

- Tidak ada direct SQL antar-modul.
- Tidak ada akses class internal modul lain.
- Tidak ada business rule lintas modul melalui copy-paste.
- Assistant tidak boleh bypass application service.

## 7.2 Komunikasi Sinkron

Digunakan bila caller membutuhkan hasil langsung.

```text
PDF Module
   ↓
IOcrCapability
   ↓
ScannerOcr Module
   ↓
Result
```

## 7.3 Event Internal

Digunakan untuk memberitahu bahwa sesuatu telah terjadi tanpa membuat coupling kuat.

```text
DocumentProcessed
      │
      ├── History
      ├── Audit
      └── Notification
```

Event tidak digunakan untuk hal sederhana yang lebih jelas melalui application interface.

## 7.4 Message Broker Eksternal

**Bukan baseline awal.**

Message broker eksternal hanya ditambahkan apabila load/scale/reliability requirement nyata membutuhkannya.

---

# 8. Database dan Kepemilikan Data

## 8.1 Baseline

Satu Microsoft SQL Server Database dapat digunakan pada baseline awal, tetapi data dipisahkan secara logis.

Contoh schema:

```text
identity.*
admin.*
workspace.*
history.*
audit.*
backup.*
jobs.*
assistant.*
support.*
```

## 8.2 Aturan Kepemilikan

- Setiap tabel memiliki module owner.
- Modul lain tidak menulis tabel tersebut secara langsung.
- Cross-module operation melalui contract/application service.
- Cross-module foreign key diminimalkan.
- Migration ownership harus jelas.
- Shared table hanya untuk data platform yang benar-benar shared.

## 8.3 File Bukan Blob SQL Default

SQL menyimpan metadata/reference.

File hasil yang diamankan menggunakan secure file/object storage yang disetujui.

---

# 9. Arsitektur Penyimpanan

```text
LOCAL DEVICE
├── Original/source file
├── Draft
├── Working state
├── Undo/Redo
├── Cache
├── Thumbnail
├── Checkpoint
└── Temporary/intermediate files

SERVER
├── SQL Server
│   ├── account
│   ├── metadata
│   ├── policy/config
│   ├── audit
│   ├── job metadata
│   ├── backup metadata
│   └── issue metadata
│
└── Secure File/Object Storage
    ├── file "Amankan ke Akun"
    ├── server-required processing temp
    └── issue screenshot/attachment
```

Storage business logic menggunakan abstraction, misalnya:

```text
IFileStorage
ISecuredAccountStorage
ILocalWorkspaceStore
```

Physical path tidak menjadi business contract.

---

# 10. Arsitektur Engine Adapter/Provider

Fitur tidak boleh terikat langsung pada vendor library.

```text
Feature
   ↓
Application Service
   ↓
Engine Contract
   ↓
Engine Resolver
   ↓
Provider Adapter
   ↓
Approved Library / Engine
```

Contoh contract:

```text
IPdfMergeEngine
IOcrEngine
IDocumentConversionEngine
IImageProcessingEngine
```

Engine Resolver mempertimbangkan:

- capability;
- platform;
- policy;
- file size;
- page/file count;
- memory/storage;
- offline/online;
- server availability;
- security;
- approved engine;
- license limitation.

Local provider diprioritaskan bila aman dan mampu.

Dynamic third-party plugin loading **bukan baseline**.

---

## 10.1 Provider Content Intelligence

AI Summarizer dan Translate PDF adalah **application tools**, bukan processing logic di PERTAMINA OTO Assistant.

```text
PDF Intelligence Tool
        ↓
Application Service
        ↓
IContentIntelligenceProvider
        ↓
Content Intelligence Resolver
        ↓
Approved Local / Enterprise-Internal Provider
```

Rules:
- no direct public AI endpoint;
- local-first bila provider/capability tersedia;
- enterprise-internal fallback hanya setelah policy/permission/data-minimization;
- prompt/document content tetap untrusted data;
- Assistant boleh mengorkestrasi `FEAT-INTEL-*`, tetapi tidak menjalankan model secara langsung;
- `PDF to Markdown` menggunakan deterministic text/layout extraction terlebih dahulu dan OCR fallback bila diperlukan; AI bukan dependency default.

---

# 11. Model Eksekusi

## 11.1 Langsung vs Background

```text
Ringan / cepat / aman
→ Immediate Execution

Berat / lama / batch / risk engine
→ Background Job
```

Penentuan dilakukan Processing Coordinator berdasarkan karakteristik aktual, bukan pilihan teknis user.

## 11.2 Lokal vs Server

Background job tidak identik dengan server.

```text
Background Local
→ Local Worker

Background Server
→ Server Worker
```

## 11.3 State Job

Minimum:

```text
QUEUED
RUNNING
WAITING_CONNECTION
WAITING_RESOURCE
RETRYING
COMPLETED
FAILED
CANCELLED
```

Setiap job panjang memiliki:

```text
JobId
CorrelationId
FeatureId
Status
Progress
CurrentStep
StartedAt
CompletedAt
ErrorCode
```

## 11.4 Backpressure

Queue dan worker harus memiliki batas.

Jika resource penuh:

- job menunggu;
- concurrency dikurangi;
- request dapat ditolak secara terkontrol bila kapasitas aman terlampaui;
- server tidak dibiarkan crash karena menerima job tanpa batas.

---

# 12. Progress, Activity Center, dan No Silent Wait

Prinsip UX:

```text
Sangat cepat
→ langsung selesai

Mulai terasa
→ indikator ringan

Proses panjang
→ progress detail

Background
→ Activity Center + notification
```

Progress detail bila tersedia:

- percentage;
- current step;
- file/page/item count;
- ETA bila cukup akurat;
- cancel bila aman.

Jika total belum diketahui, gunakan **indeterminate activity**, bukan progress palsu.

Activity Center menjadi infrastructure bersama untuk:

- tool biasa;
- Assistant;
- local job;
- server job;
- sync;
- recovery.

User tidak boleh menunggu tanpa feedback.

---

# 13. Working State, Autosave, Undo/Redo, dan Checkpoint

## 13.1 Editing Non-Destruktif

Original file tidak diedit langsung.

```text
Source
 + Operation-Based Working State
 + Preview
 + Temporary Output bila perlu
 + Validation
 = Final Result
```

## 13.2 Autosave

Autosave menyimpan state/delta sebanyak mungkin, bukan terus-menerus membuat full file baru.

## 13.3 Undo/Redo

Undo/Redo menggunakan operation history dengan batas/compaction yang ditentukan berdasarkan benchmark.

## 13.4 Checkpoint

Proses berat yang mendukung resume menggunakan checkpoint.

Contoh OCR 1.000 halaman tidak wajib mengulang dari halaman 1 setelah crash bila safe checkpoint tersedia.

---

# 14. Registry Feature Canonical

Satu registry menjadi sumber kebenaran capability.

Registry menyimpan contract stabil:

```text
FeatureId
RecordType
InputOutputContract
ParameterSchemaVersion
EngineCapability
Risk
Destructive
Composable
```

Enterprise configuration dapat mengatur:

```text
Enabled/Disabled
Permission
Feature Flag
Limit
Policy
Availability
```

Administrator tidak boleh mengubah arti contract fitur.

Contoh:

```text
Merge PDF
Input  = PDF[]
Output = PDF
```

tidak dapat diubah melalui database menjadi output Excel.

UI, Assistant, workflow, permission, engine resolver, dan compatibility layer menggunakan Feature ID canonical yang sama.

---

# 15. Strategi Platform

## 15.1 Desktop

**Primary full-feature client.**

Target:

- feature set terlengkap;
- batch;
- file besar;
- advanced PDF/document workflows;
- drag-and-drop;
- keyboard;
- multi-panel;
- local worker;
- local engines paling lengkap.

## 15.2 Tablet

Curated feature set dan dapat lebih kaya daripada phone.

Cocok untuk:

- membaca PDF;
- scan;
- visual signature/paraf;
- annotation;
- touch/stylus;
- beberapa editing/tool sederhana.

## 15.3 Phone

Curated feature set yang cepat/praktis:

- scan;
- read PDF;
- crop/rotate;
- OCR ringan;
- merge/compress sederhana;
- signature/paraf;
- share;
- Amankan ke Akun;
- issue reporting;
- activity/status.

## 15.4 Web

Fokus:

- account;
- history;
- selective sync;
- admin;
- audit;
- support;
- browser-safe tools;
- server-required fallback sesuai policy.

## 15.5 Eksposur + Capability Runtime

Urutan keputusan availability:

```text
Platform Exposure
      ↓
Device Capability
      ↓
Permission
      ↓
Enterprise Policy
      ↓
Resource/Storage
      ↓
Engine Availability
      ↓
Connectivity
      ↓
Execute / Fallback / Hide / Explain
```

User tidak memilih engine teknis.

---

# 16. Shared Core + UI/Adapter Spesifik Platform

Yang dibagi bersama:

- domain/business rules;
- application contracts;
- feature registry;
- validation;
- permission/policy rules;
- workflow rules;
- error model;
- processing abstractions.

Yang khusus platform:

- Desktop UI;
- Mobile/Tablet UI;
- camera;
- touch/stylus;
- drag-and-drop;
- file picker;
- filesystem integration;
- system notification;
- platform capability adapter.

Prinsip:

> **Satu aturan kerja, cara penggunaan menyesuaikan perangkat.**

Shared core tidak berarti library/engine fisik harus identik pada semua platform.

---

# 17. Sinkronisasi Selektif Antarperangkat

Workspace tetap lokal.

Cross-device hanya terjadi untuk data yang memang dipilih/diizinkan.

```text
Local Result
   ↓
[Amankan ke Akun]
   ↓
Secure Account Storage
   ↓
Bisa dibuka dari perangkat lain
```

Tidak auto-sync:

- draft;
- undo/redo;
- cache;
- thumbnail;
- clipboard;
- password;
- secret;
- temp;
- unselected source.

File yang dibuka dari akun dibuat menjadi local working copy.

Silent overwrite dilarang.

Conflict version harus dideteksi.

Real-time collaborative document editing **bukan baseline**.

Local Recycle Bin dan Account Recycle/Retention adalah lifecycle berbeda.

---

# 18. Pemrosesan Lokal Hybrid

## 18.1 Dalam Proses (`In-Process`)

Pekerjaan ringan dan aman dapat berjalan di proses Desktop utama apabila tidak mengganggu responsiveness.

## 18.2 Worker Lokal Terisolasi

Pekerjaan berat/berisiko seperti:

- OCR besar;
- batch processing;
- conversion berat;
- engine dengan risiko memory spike/leak;

dapat dijalankan di Local Worker Process terpisah.

```text
Desktop UI
   ↓
Local Job
   ↓
Local Worker
   ↓
Engine Adapter
```

Worker crash tidak boleh menjatuhkan UI utama.

Worker dapat direcycle setelah job/threshold tertentu bila diperlukan untuk menjaga memory health.

---

# 19. Configuration, Policy, dan Feature Flags

Priority layer:

```text
1. System Safety / Security Invariants
2. Enterprise Policy
3. Environment Configuration
4. Device Configuration
5. User Preference
```

Layer lebih bawah tidak boleh mengalahkan aturan di atasnya.

## 19.1 Keamanan Sistem

Contoh:

- permission check wajib;
- file validation wajib;
- password/token tidak boleh masuk log;
- Assistant tidak boleh direct SQL.

Tidak dapat dimatikan sembarangan melalui admin UI.

## 19.2 Kebijakan Enterprise

Contoh:

- file size limit;
- storage quota;
- retention;
- processing limit;
- server processing permission;
- concurrent jobs;
- feature availability.

## 19.3 Preferensi Pengguna

Contoh:

- bahasa;
- view mode;
- zoom;
- favorite tools;
- notification preference;
- default save location bila didukung.

## 19.4 Flag Fitur

Digunakan untuk:

- staged rollout;
- pilot;
- QA;
- emergency disable.

Feature flag harus mempunyai lifecycle dan tidak menjadi hutang permanen.

---

# 20. Topologi Deployment

## 20.1 Batas Deployable

```text
Pertamina.Oto.Web
Pertamina.Oto.Api
Pertamina.Oto.Worker
Pertamina.Oto.Desktop
Pertamina.Oto.Mobile
SQL Server
Secure File/Object Storage
Monitoring/APM
```

## 20.2 Dev / QA / Pilot

Beberapa deployable boleh berada pada satu physical server bila:

- security policy mengizinkan;
- resource cukup;
- failure isolation masih diterima;
- load test/pilot sesuai.

```text
SERVER AWAL
├── Web
├── API
├── Worker
├── SQL
└── Secure Storage
```

## 20.3 Produksi/Scale-Up

Komponen dapat dipisahkan tanpa perubahan business logic:

```text
Web Server
    ↓
API Server
    ↓
┌──────────────┬──────────────┐
Worker Server  SQL Server     Storage
```

Scaling didasarkan pada measurement, bukan asumsi.

---

# 21. Observability dan Model Error

## 21.1 Logging Terstruktur

Log menggunakan field terstruktur.

Contoh:

```text
FeatureId
JobId
CorrelationId
Status
Duration
EngineProvider
ErrorCode
AppVersion
```

Tidak log:

- isi dokumen;
- password;
- token;
- secret;
- full OCR text;
- signature asset;
- clipboard sensitif.

## 21.2 Identitas Diagnostik

```text
Job ID
→ identitas job processing

Correlation ID
→ benang merah aktivitas lintas komponen

Error Code
→ tipe kegagalan yang stabil
```

## 21.3 Pemantauan

Minimum:

- API latency/error;
- job duration;
- queue;
- CPU/RAM/storage;
- DB response;
- transfer failure;
- worker health;
- app version;
- correlation;
- availability.

Application Insights/APM digunakan sesuai baseline enterprise dan policy.

---

# 22. Pelaporan Masalah dalam Aplikasi — Pengalaman Pengguna Final

## 22.1 UI untuk Pengguna

Pelaporan harus sesederhana mungkin.

```text
┌───────────────────────────────────┐
│ Laporkan Masalah               ✕ │
│                                   │
│ Ceritakan apa yang tidak sesuai   │
│ [...............................]  │
│                                   │
│ [ Screenshot ]                    │
│                                   │
│                    [ Kirim ]      │
└───────────────────────────────────┘
```

**Yang dilakukan user hanya:**

1. Tulis pesan singkat.
2. Screenshot bila perlu.
3. Kirim.

Tidak ada form kategori/prioritas/Feature ID/error code/status/assignment untuk user.

## 22.2 Alur Screenshot

```text
Screenshot
   ↓
Preview
   ↓
Tandai sederhana bila perlu
   ↓
Kembali ke report
   ↓
Kirim
```

Aturan:

- screenshot opsional;
- target utamanya jendela PERTAMINA OTO, bukan seluruh desktop bila platform memungkinkan;
- user selalu melihat preview;
- tidak auto-upload;
- markup sederhana, bukan editor gambar kompleks.

## 22.3 Konteks Otomatis

Sistem menambahkan secara otomatis:

- account/user reference;
- timestamp;
- active Feature ID/page;
- app version;
- platform/OS;
- Error Code jika ada;
- Job ID jika ada;
- Correlation ID jika ada;
- safe diagnostic context sesuai policy.

## 22.4 Konsol Dukungan Superadmin

Superadmin/Support dengan permission khusus melihat:

- issue list;
- message;
- screenshot;
- diagnostic context;
- reporter/time/version;
- category;
- priority;
- status;
- internal note;
- duplicate/not-bug/need-info state.

User tidak perlu mengisi category/priority; Superadmin melakukan triage.

## 22.5 Ekspor

Superadmin dapat:

```text
Export PDF
→ detail satu issue untuk programmer

Export Excel
→ rekap issue terfilter
```

Laporan diteruskan sebagai bahan tugas perbaikan programmer.

Tidak ada WhatsApp sebagai bagian dari system workflow.

## 22.6 Penyimpanan dan Keamanan

- Issue metadata → SQL schema `support`.
- Screenshot/attachment → secure file/object storage.
- Access melalui permission.
- Screenshot user dapat mengandung informasi sensitif; karena itu preview dan explicit send wajib.
- Administrator/Superadmin tidak mendapat hak membaca file user lain hanya karena memiliki akses support.

---

# 23. Kompatibilitas Versi

Client dan server tidak harus selalu mempunyai versi yang sama.

Server/endpoint menyediakan compatibility information:

```text
CurrentVersion
MinimumSupportedVersion
ContractVersion
CapabilityVersion
```

Status:

```text
SUPPORTED
UPDATE_AVAILABLE
UPDATE_REQUIRED
UNSUPPORTED
```

Forced update hanya digunakan ketika keamanan/compatibility benar-benar memerlukannya.

Versioned contract mencakup:

- app version;
- API contract;
- Feature parameter schema;
- workflow schema;
- configuration schema;
- engine contract bila relevan.

Breaking change memerlukan migration/compatibility path.

---

# 24. Komunikasi REST + Realtime

## 24.1 REST API

Digunakan untuk:

- command/request;
- upload/download;
- metadata;
- history;
- settings;
- issue reporting;
- admin;
- current state refresh.

## 24.2 SignalR

Digunakan untuk event yang memang perlu cepat:

- job progress;
- job completed;
- job failed;
- relevant status update;
- activity notification.

SignalR **bukan source of truth**.

## 24.3 Fallback

Jika realtime channel putus:

```text
SignalR disconnected
      ↓
Job tetap berjalan
      ↓
Reconnect
      ↓
REST ambil state terbaru
      ↓
Activity Center pulih
```

Source of truth adalah persisted job/state data.

---

# 25. Integrasi Assistant

Assistant mengikuti jalur resmi:

```text
User Intent
→ Intent/Alias Resolution
→ Canonical Feature ID
→ Parameter Schema Validation
→ Input/Output Compatibility
→ Platform/Capability Check
→ Permission/Policy
→ Resource Preflight
→ Confirmation bila risk
→ Application Service
→ Engine Resolver
→ Actual Tool Result
```

Assistant:

- tidak membuat processing engine sendiri;
- tidak direct SQL;
- tidak arbitrary shell/code;
- tidak unrestricted filesystem;
- tidak arbitrary API;
- tidak mengarang success/result;
- menggunakan Activity Center yang sama;
- menggunakan issue/error infrastructure yang sama;
- menghormati Desktop/Mobile exposure.

---

# 26. Batas Keamanan

Wajib:

- HTTPS.
- Secure token storage.
- Least privilege.
- Permission-based access.
- No secrets in source.
- No document content in telemetry.
- No passwords in logs.
- Secure server file access.
- Encryption in transit and at rest untuk server-stored file sesuai policy.
- Screenshot issue hanya atas tindakan user.
- Safe diagnostic metadata only.
- Prompt-injection defense.
- File content treated as untrusted input.
- Dependency/license/vulnerability review.
- VA/PenTest sebelum Production.

---

# 27. Anggaran Sumber Daya Server

Setiap feature mempertahankan metadata:

```text
ProcessingMode
ServerDependency
StorageImpact
NetworkImpact
ConcurrencyRisk
```

Desain server:

- local processing first bila aman;
- server tidak menerima draft/cache/undo/temp penuh;
- batching;
- queue;
- backpressure;
- concurrency limit;
- pagination;
- retention;
- cleanup;
- quota;
- capacity check.

Issue Reporting juga mengikuti budget:

- pesan/metadata kecil di SQL;
- screenshot hanya saat user memilih;
- thumbnail/preview tidak disimpan berlebihan;
- retention issue attachment dapat diatur policy;
- export dibuat on-demand.

---

# 28. Skenario Kegagalan dan Pemulihan

| Scenario | Behavior |
|---|---|
| Desktop UI crash | Draft/checkpoint dipulihkan jika valid |
| Local Worker crash | UI tetap hidup; job gagal/retry/resume sesuai policy |
| Server Worker crash | Job state tetap tersimpan; retry/checkpoint bila aman |
| SignalR putus | Job lanjut; client REST-refresh setelah reconnect |
| Network putus | Local-capable work lanjut; server-required work menunggu/queue |
| Storage hampir penuh | Preflight memperingatkan/menghentikan secara aman |
| Engine gagal | Error Code + retry/fallback provider bila disetujui |
| Versi client lama | Compatibility check; update hanya dipaksa bila required |
| Sync conflict | Tidak silent overwrite; user diberi pilihan aman |
| Issue screenshot gagal upload | Pesan issue tidak hilang; attachment dapat retry sesuai policy |

---

# 29. Definition of Done untuk Implementasi Solution

Tidak ada komponen yang dianggap DONE hanya karena build/test teknis hijau.

Tetap wajib delapan lapis:

1. Technical Correctness.
2. Business Process Correctness.
3. UI/UX Acceptance.
4. Security & Privacy.
5. Performance / Server Budget.
6. Error & Recovery.
7. Cross-Platform Behavior.
8. Test & Documentation.

Contoh acceptance wajib:

```text
Phone:
Scan → crop → OCR ringan → PDF → Amankan ke Akun

Desktop:
Buka file aman → edit lanjut → finalisasi

Issue:
User tulis pesan → screenshot opsional → kirim
→ Superadmin melihat → triage → export PDF/Excel
```

---

# 30. Pengujian Arsitektur yang Wajib

Selain unit test, solution harus mempunyai automated architecture/contract checks untuk memastikan:

- module tidak direct-reference infrastructure internal modul lain;
- client tidak direct SQL;
- domain tidak bergantung UI;
- Assistant tidak bypass feature/application contract;
- Feature ID unique;
- parameter schema terversi;
- forbidden dependency tidak masuk;
- secret/logging rule dipatuhi;
- server API contract compatibility diuji.

---

# 31. Gerbang Kelayakan Sebelum Coding Penuh

Tetap wajib untuk:

- edit existing PDF text;
- Office → PDF;
- PDF → Word/Excel/PowerPoint;
- OCR tabel kompleks;
- background removal;
- HTML/Web → PDF;
- PDF/A;
- physical scanner integration;
- optional local AI model.
- AI Summarizer provider.
- Translate PDF provider dan fidelity.
- PDF to Markdown untuk layout/table/OCR fidelity bila format kompleks.

Gate minimum:

- output quality;
- supported formats;
- license/cost;
- security;
- platform support;
- CPU/RAM;
- large-file behavior;
- concurrency;
- maintainability;
- fallback.

Result:

```text
APPROVED_ENGINE
APPROVED_WITH_LIMITATION
BLOCKED_ALTERNATIVE_REQUIRED
```

---

# 32. Keputusan yang Sengaja Belum Mengunci Vendor/Library

Bukan gap requirement:

- PDF library final.
- OCR engine final.
- Office conversion engine final.
- image/AI model final.
- exact mobile UI framework dalam keluarga C#/.NET.
- exact Production physical topology.
- exact concurrency/file-size limits.

Hal-hal tersebut baru boleh dikunci setelah feasibility, benchmark, security/license review, dan/atau capacity test.

---

# 33. Ketertelusuran ke Dokumen Berikutnya

`04_SOLUTION_DESIGN.md` menjadi parent desain untuk:

```text
05_TECHNICAL_REFERENCE.md
→ library/runtime/version/package/technology detail

06_API_DOCUMENTATION.md
→ REST + SignalR contract/endpoints/events

07_DATABASE_DESIGN.md
→ schema ownership, jobs, support, audit, backup metadata

08_SECURITY_DESIGN.md
→ trust boundary, encryption, issue attachment security

09_ROLE_MATRIX.md
→ User/Admin/Auditor + permission Superadmin/Support

10_AUDIT_TRAIL_DESIGN.md
→ audit/status/config/issue workflow events

11_UNIT_TEST_DOCUMENT.md
→ unit + architecture + contract test scope

12_TEST_PLAN.md
→ functional/integration/performance/security scenarios

13_UAT_DOCUMENT.md
→ business + UX acceptance

14_USER_MANUAL.md
→ Desktop/mobile tools + report issue flow

15_ADMIN_MANUAL.md
→ feature flags/policy/support console

16_INCIDENT_SUPPORT_GUIDE.md
→ issue triage/escalation/programmer repair flow

17_DEPLOYMENT_GUIDE.md
→ co-hosted initial topology → separated topology

18_APPLICATION_CARTOGRAPHY.md
→ application/module/dependency map

19_HANDOVER_CHECKLIST.md
→ completeness and ownership
```

---

# 34. Penguncian Arsitektur Final

```text
Architecture:
Modular Monolith
+ Clean Architecture
+ Separate Deployables

Primary Client:
Windows Desktop — full feature

Mobile/Tablet:
Curated companion

Web:
Enterprise/browser channel

Processing:
Offline-First
Capability-Aware Local-First
Hybrid Immediate/Background
Local Worker + Server Worker

Data:
SQL metadata
+ Local Workspace
+ Secure File/Object Storage

Feature:
Canonical Registry
+ 33 Primary Tool Cards
+ Versioned Contract
+ Engine Adapter/Resolver
+ Content Intelligence Provider

Communication:
REST
+ SignalR for realtime events
+ REST fallback

Support:
Structured Logging
+ Correlation ID
+ In-App Issue Reporting
+ Superadmin Support Console
+ PDF/Excel Export

Deployment:
Logically separated
Physically flexible

Status:
LOCKED — SAFE FOR NEXT DOCUMENT
```

---

# 35. Status Dokumen

```text
Nama dokumen : 04_SOLUTION_DESIGN.md
Versi        : 1.2
Status       : FINAL — LOCKED PRE-CODING SOLUTION BASELINE
Keputusan    : 18 dari 18
Progress     : 100%
Audit        : PASS — no blocking contradiction
```

Dokumen berikutnya:

```text
05_TECHNICAL_REFERENCE.md
```
