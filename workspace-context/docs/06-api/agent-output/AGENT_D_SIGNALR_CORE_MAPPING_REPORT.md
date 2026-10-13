# AGENT D — SIGNALR + 33 CORE FEATURE MAPPING REPORT

**Task ID:** `DOC06-AGENT-D`  
**Scope:** `API-DEC-008` + audit 33 Primary PDF Tool Cards  
**Status:** Rekomendasi untuk Integrator; bukan perubahan pada master Document 06  
**Tanggal:** 24 Agustus 2026

Dokumen yang diaudit: `01_BUSINESS_PROCESS_FINAL_v3.4`, `02_REQUIREMENT_SPECIFICATION_FINAL_v1.5`, `03_FEATURE_CATALOG_FINAL_v1.3`, `04_SOLUTION_DESIGN_FINAL_v1.2`, `05_TECHNICAL_REFERENCE_FINAL_v1.2`, dan baseline Document 06 yang tersedia di workspace. Prompt menyebut Working v0.1, tetapi file yang tersedia adalah `06_API_DOCUMENTATION_WORKING_v0.2_PERTAMINA_OTO.md`; perbedaan ini dicatat di bagian risiko dan tidak diselesaikan dengan mengubah master.

## 1. SignalR envelope

### 1.1 Rekomendasi keputusan

Gunakan pola **Persisted REST State + Versioned At-Least-Once Realtime Notification**:

- REST/persisted state tetap source of truth.
- SignalR hanya memberi notifikasi cepat bahwa state yang sudah disimpan berubah.
- State disimpan lebih dahulu, kemudian event dipublikasikan. Kegagalan publish tidak membatalkan job dan dipulihkan lewat REST.
- Delivery diperlakukan **at-least-once**; client wajib idempotent. Jangan mengasumsikan exactly-once.
- Tidak ada hub, event type, endpoint, atau processing engine per Primary Tool Card.
- Background job lokal menggunakan local IPC/Local Worker dan Activity Center yang sama, tetapi **bukan SignalR**.

### 1.2 Minimum envelope

Tiga field tambahan—`eventId`, `streamId`, dan `sequence`—diperlukan agar duplicate, ordering, gap, dan stale event dapat ditangani tanpa menjadikan SignalR source of truth.

```json
{
  "eventId": "evt_01J...",
  "eventType": "JobProgress",
  "eventVersion": 1,
  "streamId": "job:job_01J...",
  "sequence": 42,
  "jobId": "job_01J...",
  "featureId": "FEAT-PDF-005",
  "correlationId": "corr_01J...",
  "timestamp": "2026-08-24T10:15:30.125Z",
  "status": "RUNNING",
  "progress": 62,
  "currentStep": "MERGE_PAGES",
  "completedCount": 31,
  "totalCount": 50,
  "messageCode": "JOB_MERGING_PAGES"
}
```

| Field | Keharusan | Contract minimum |
|---|---|---|
| `eventId` | Wajib | Opaque unique ID untuk deduplication dan observability; tidak dipakai sebagai business key. |
| `eventType` | Wajib | Salah satu event pada catalog; case-sensitive dan stabil. |
| `eventVersion` | Wajib | Integer positif, terversi per `eventType`; bukan app version atau API major version. |
| `streamId` | Wajib | Identitas ordering stream, misalnya job/activity/issue. Aman, opaque/semi-opaque, dan tidak memuat data user. |
| `sequence` | Wajib | Bilangan monoton naik per `streamId`, dibuat dari persisted state revision. Tidak ada global ordering lintas-stream. |
| `jobId` | Kondisional | Wajib untuk `Job*`; boleh null untuk issue yang tidak terkait job. |
| `featureId` | Kondisional | Canonical Feature ID untuk job/tool; tidak boleh alias atau ID buatan Assistant. |
| `correlationId` | Wajib | Benang merah API, service, Worker, storage, log, telemetry, dan event. |
| `timestamp` | Wajib | UTC ISO-8601 dari server; hanya untuk display/diagnostic, bukan penentu ordering. |
| `status` | Wajib | Nilai persisted state domain terkait. Event tidak boleh menciptakan state baru secara implisit. |
| `progress` | Kondisional | Integer `0..100`; null jika total tidak diketahui. Jangan mengirim progress palsu. |
| `currentStep` | Kondisional | Stable code, bukan raw message, nama file, teks dokumen, atau OCR text. |
| `completedCount` | Kondisional | Integer non-negatif; dipakai bila unit total diketahui. |
| `totalCount` | Kondisional | Integer non-negatif; null jika tidak diketahui. |
| `messageCode` | Wajib | Safe localization/status/error code. UI melokalkan sendiri; jangan kirim exception/raw server message. |

### 1.3 Larangan payload

Event tidak boleh memuat:

- document content, potongan isi dokumen, atau document preview;
- password PDF;
- access/refresh token;
- secret, connection string, atau internal path;
- full OCR text;
- signature asset, signature data, atau certificate material;
- prompt/chat content;
- stack trace, exception detail, atau engine-internal message;
- nama file bila tidak benar-benar diperlukan untuk state display; Activity Center sebaiknya memakai metadata lokal/REST yang sudah diotorisasi.

`messageCode` dan `currentStep` harus berupa code terkontrol. Event tidak boleh dipakai untuk mengirim arbitrary display text.

## 2. Event catalog

| Event | Stream / status persisted | Field kondisional utama | Semantik dan batasan |
|---|---|---|---|
| `JobQueued` | job / `QUEUED` | `jobId`, `featureId`; progress null atau 0 | Job sudah diterima dan state queue sudah persisted. Bukan perintah untuk membuat job baru. |
| `JobStarted` | job / `RUNNING` | `jobId`, `featureId`, `currentStep` | Worker mulai mengeksekusi job yang sama. |
| `JobProgress` | job / `RUNNING` | progress dan/atau counts; `currentStep` | Snapshot progress yang sudah persisted. Event boleh di-coalesce/throttle; jangan publish setiap byte/page jika membebani server. |
| `JobPaused` | job / `WAITING_CONNECTION` atau `WAITING_RESOURCE` | `messageCode` alasan aman | Nama event diminta task, tetapi state `PAUSED` belum ada pada state machine locked. Jangan membuat state baru; gunakan waiting state sampai Integrator memutuskan true user-pause. |
| `JobCompleted` | job / `COMPLETED` | progress 100 bila deterministik | Output sudah tervalidasi sebagai final dan persisted. Event bukan carrier file/content. |
| `JobFailed` | job / `FAILED` | `messageCode`, `currentStep` bila aman | Hanya safe error code; tanpa exception/stack trace/raw engine output. |
| `JobCancelled` | job / `CANCELLED` | `messageCode` | Pembatalan sudah committed. Duplicate event tidak boleh memicu cancel kedua. |
| `JobRetrying` | job / `RETRYING` | `messageCode` | Retry terbatas dan sudah dicatat. Event tidak mengeksekusi retry; retry dilakukan application/job service. |
| `ActivityUpdated` | activity / persisted activity state | `jobId`/`featureId` bila terkait | Menginvalidasi/menyegarkan item Activity Center. Tidak membawa seluruh activity history. |
| `IssueStatusChanged` | issue / persisted issue status | `jobId` null kecuali benar-benar terkait | Hanya ID stream, status, waktu, correlation, dan message code aman; tidak memuat pesan issue, screenshot, attachment, atau catatan support. |

### 2.1 Hub boundary

Gunakan satu logical **Activity Hub** untuk event user-scoped: job, Activity Center, dan perubahan status issue milik user. Nama route fisik dan API-major placement mengikuti keputusan version routing; laporan ini tidak mengunci URI.

Boundary:

- Hub wajib authenticated dan menggunakan identity/claim yang disetujui; mekanisme final mengikuti `API-DEC-007`.
- Hub tidak menerima document processing command, upload, download, retry, cancel, atau perubahan state. Semua command tetap melalui application service/REST contract resmi.
- Hub tidak memiliki group per Feature ID dan tidak memiliki hub per tool.
- User-scoped `IssueStatusChanged` boleh melalui Activity Hub. Feed lintas-user untuk Superadmin/Support, bila dibutuhkan, harus menjadi boundary berizin terpisah dan bukan bagian minimum ini.
- Local Worker progress tidak masuk Activity Hub; local IPC memakai typed contract sendiri dan diproyeksikan ke Activity Center client.

### 2.2 Subscription model

1. Saat koneksi authenticated terbentuk, server memasukkan connection ke group user yang identity-nya **diturunkan dari server**, bukan dari `userId` yang dikirim client.
2. Group user menerima `ActivityUpdated`, user-owned job terminal/status event, dan `IssueStatusChanged` milik user.
3. Optional per-job subscription dapat dipakai untuk high-frequency `JobProgress` ketika detail job dibuka. `SubscribeJob(jobId)` wajib memvalidasi ownership/permission setiap kali dan memiliki subscription limit.
4. Client tidak boleh subscribe ke arbitrary user, issue, correlation, atau tenant/unit group.
5. Assistant tidak membuat subscription khusus; job yang dimulai melalui Assistant memakai `jobId`, `featureId`, group, Activity Center, dan event yang sama dengan Tool-First/File-First.

## 3. Reconnect/recovery

### 3.1 Alur wajib

```text
SignalR disconnected
→ tandai realtime channel degraded; jangan tandai job failed
→ job server tetap berjalan dan state tetap dipersist
→ automatic reconnect dengan bounded exponential backoff + jitter
→ setelah connected, lakukan compatibility check bila diperlukan
→ REST reconciliation untuk active/recent jobs, Activity Center, dan issue yang relevan
→ apply state berdasarkan persisted revision/sequence
→ resubscribe detail job yang masih authorized dan aktif
→ Activity Center pulih
```

Client tidak boleh mengirim ulang execute command hanya karena reconnect. Resubmission/retry command hanya mengikuti idempotency contract `API-DEC-005` dan pilihan/policy recovery yang sah.

Jika reconnect belum berhasil, client dapat memakai bounded/visibility-aware REST polling untuk server job aktif. Polling berhenti ketika realtime pulih atau tidak ada job aktif. Kegagalan SignalR tidak mengubah local-capable work menjadi server work.

### 3.2 Duplicate behavior

- Simpan cache terbatas `eventId` yang sudah diterapkan per session.
- Jika `eventId` pernah diterapkan, abaikan event.
- Jika `sequence <= lastAppliedSequence` untuk stream yang sama, anggap duplicate/stale dan abaikan.
- Apply event secara idempotent; terminal event ganda tidak mengulang download, notification, audit, atau side effect.
- SignalR event tidak pernah menjadi trigger business command. Ia hanya memperbarui projection/menandai bahwa REST state berubah.

### 3.3 Ordering dan gap

- Ordering dijamin secara logis **hanya per `streamId`**, bukan global dan bukan antar-job.
- `sequence` dialokasikan saat perubahan state dipersist, bukan saat koneksi mengirim event.
- `timestamp` tidak dipakai untuk menyelesaikan race/ordering karena clock skew dan publish delay.
- Jika `sequence == lastAppliedSequence + 1`, event dapat diterapkan.
- Jika `sequence > lastAppliedSequence + 1`, ada gap: tahan regresi UI dan segera lakukan REST reconciliation untuk stream tersebut.
- Reconnect, scale-out, retry publisher, atau failover boleh menghasilkan duplicate/delayed delivery; contract client tetap sama.

### 3.4 Stale event handling

- Jangan pernah menurunkan state terminal berdasarkan event lama, misalnya `COMPLETED` kembali menjadi `RUNNING`.
- Event dengan sequence di bawah persisted REST revision terakhir diabaikan.
- Setelah REST reconciliation, REST revision menjadi baseline baru; delayed event yang lebih lama tidak diterapkan.
- Jika event dan cached client state berkonflik tanpa sequence yang dapat dipercaya, REST menang.
- Event terminal dapat memicu fetch state/result metadata, tetapi tidak membuktikan file sudah berhasil diunduh ke perangkat.

## 4. REST reconciliation

### 4.1 Trigger

REST reconciliation dilakukan pada:

- startup/restore Activity Center;
- SignalR reconnect;
- sequence gap;
- unknown `eventType`;
- unsupported `eventVersion`;
- client kembali aktif setelah sleep/suspend;
- event terminal bila UI membutuhkan result metadata terbaru;
- dugaan stale/out-of-order state.

### 4.2 Data authoritative minimum

Representasi REST job/activity/issue harus menyediakan ID resource, persisted status, persisted revision yang dapat dibandingkan dengan `sequence`, `correlationId`, waktu update, dan safe status/progress metadata yang relevan. Untuk job, minimum tetap mencakup `jobId`, canonical `featureId`, status, progress, current step, counts bila diketahui, start/complete time, dan safe error/message code.

Algoritma client:

1. Catat `lastAppliedSequence` per active stream.
2. Ambil current persisted state lewat REST resource yang authorized; URI final mengikuti decision API lain.
3. Upsert Activity Center berdasarkan resource ID, bukan urutan kedatangan event.
4. Jika REST revision lebih baru, REST menggantikan projection client.
5. Jika REST revision sama, tidak ada perubahan.
6. Jika respons REST lebih tua dari projection terminal yang sudah memiliki sequence lebih tinggi, jangan regresikan UI; ulangi conditional read/backoff karena authoritative read seharusnya mencapai persisted revision tersebut.
7. Hapus/tandai item hanya berdasarkan aturan retention/tombstone REST, bukan karena event tidak diterima.
8. Setelah reconcile, resubscribe hanya ke job yang masih aktif dan authorized.

### 4.3 Version mismatch behavior

- Unknown extra fields pada versi yang didukung diabaikan untuk forward-compatible additive change.
- Known `eventType` dengan `eventVersion` lebih tinggi dari yang didukung **tidak** dideserialisasi sebagai versi lama. Catat safe telemetry, lakukan REST reconciliation, dan tampilkan state dari REST.
- Versi lama yang masih berada pada compatibility window dapat diadaptasi oleh versioned event adapter.
- `eventType` yang tidak dikenal diabaikan sebagai notification, lalu client reconcile melalui REST; jangan crash dan jangan menebak semantik.
- Jika minimum supported contract/app version tidak terpenuhi, ikuti compatibility status `UPDATE_REQUIRED/UNSUPPORTED`. Forced update hanya ketika security/compatibility memang mewajibkan.
- Event version mismatch tidak boleh menghentikan job server.

## 5. 33-Core mapping table

### 5.1 Interpretasi kolom

- `LOCAL=YES`: ada jalur eksekusi lokal canonical bila capability/policy/resource/engine mendukung.
- `OPTIONAL_SERVER=YES`: server hanya fallback/alternate path yang dibenarkan; bukan izin otomatis membuat endpoint.
- `REQUIRED_SERVER=YES`: contract feature tidak dapat dipenuhi tanpa backend. Berdasarkan source saat ini, tidak ada Core tool yang terkunci sebagai required-server secara universal.
- `PoC_REQUIRED=YES`: ada feasibility/engine gate eksplisit; scope terbatas ditulis pada alasan.
- `API needed?` menilai **server-processing execution API**, bukan API bersama untuk SSO, audit metadata, history, issue, Amankan ke Akun, atau selective sync.
- `SignalR?=CONDITIONAL` berarti hanya untuk server background job. Immediate server response tidak memerlukan progress event; local background job memakai local IPC.
- `Full*` berarti offline penuh selama engine, capability platform, policy, resource, dan storage lokal mendukung.

Matriks ini adalah overlay kebutuhan API per Primary Tool Card, bukan perubahan Feature ID atau status Feature Catalog.

| No | Primary Tool | Canonical Feature ID | LOCAL | OPTIONAL_SERVER | REQUIRED_SERVER | PoC_REQUIRED | Processing Mode | Server Dependency | Offline | BG likelihood | SignalR? | Security risk | API needed? | Alasan singkat |
|---:|---|---|:---:|:---:|:---:|:---:|---|---|---|---|---|---|---|---|
| 1 | Merge PDF | `FEAT-PDF-005` | YES | NO | NO | NO | Local (`L1`) | None—local baseline | Full* | Medium | NO | LOW | **NO** | Deterministic PDF page composition; file besar dapat memakai Local Worker. |
| 2 | Split PDF | `FEAT-PDF-006 / FEAT-PDF-007` | YES | NO | NO | NO | Local (`L1`) | None—local baseline | Full* | Medium | NO | LOW | **NO** | Split dan page-range tersedia lewat PDF engine lokal. |
| 3 | Remove pages | `FEAT-PDF-013` | YES | NO | NO | NO | Local (`L1`) | None—local baseline | Full* | Low | NO | LOW | **NO** | Operation-based working state lokal; sumber tidak ditimpa. |
| 4 | Extract pages | `FEAT-PDF-014` | YES | NO | NO | NO | Local (`L1`) | None—local baseline | Full* | Medium | NO | LOW | **NO** | Ekstraksi halaman deterministik melalui PDF engine lokal. |
| 5 | Organize PDF | `FEAT-PDF-010..016` | YES | NO | NO | NO | Local workflow (`L1`) | None—local baseline | Full* | Medium | NO | LOW | **NO** | Card workflow memakai capability page operations yang sama, bukan service baru. |
| 6 | Scan to PDF | `FEAT-SCAN-014` | YES | NO | NO | **YES** | Local/device (`L1`) | None—local baseline | Full* | Medium | NO | LOW | **NO** | Camera/import lokal; PoC hanya untuk physical scanner driver/platform integration. PoC tidak membuat server wajib. |
| 7 | Compress PDF | `FEAT-PDF-023` | YES | NO | NO | NO | Local (`L1`) | None—local baseline | Full* | Medium | NO | LOW | **NO** | Deterministic local compression; gunakan Local Worker untuk file besar. |
| 8 | Repair PDF | `FEAT-PDF-025 / FEAT-PDF-026` | YES | YES | NO | NO | Hybrid (`L1` + `H1`) | Optional—advanced fallback | Partial | Medium–High | CONDITIONAL | LOW | **CONDITIONAL** | Basic repair lokal; advanced repair dapat memakai approved specialized fallback. Tidak ada formal PoC gate pada source untuk Repair. |
| 9 | OCR PDF | `FEAT-OCR-004` | YES | YES | NO | **YES** | Hybrid, local-first OCR | Optional—complex/large fallback | Partial | High | CONDITIONAL | MEDIUM | **CONDITIONAL** | Normal ID/EN searchable OCR lokal; complex layout/table/bad scan/very large dapat ke provider internal setelah consent/policy. |
| 10 | JPG to PDF | `FEAT-CONV-007` | YES | NO | NO | NO | Local (`L1`) | None—local baseline | Full* | Medium | NO | LOW | **NO** | Konversi gambar deterministik tidak memerlukan server. |
| 11 | Word to PDF | `FEAT-CONV-001` | YES | YES | NO | **YES** | Hybrid (`H1`) | Optional | Partial | High | CONDITIONAL | LOW | **CONDITIONAL** | Fidelity, pagination, font, headless/platform, license, dan resource harus lulus PoC. |
| 12 | PowerPoint to PDF | `FEAT-CONV-003` | YES | YES | NO | **YES** | Hybrid (`H1`) | Optional | Partial | High | CONDITIONAL | LOW | **CONDITIONAL** | Slide/font/media fidelity dan provider capability memerlukan PoC. |
| 13 | Excel to PDF | `FEAT-CONV-002` | YES | YES | NO | **YES** | Hybrid (`H1`) | Optional | Partial | High | CONDITIONAL | LOW | **CONDITIONAL** | Formula/chart/print-area/pagination fidelity memerlukan PoC. |
| 14 | HTML to PDF | `FEAT-CONV-017` | YES | YES | NO | **YES** | Hybrid (`H1`) | Optional | Partial | Medium–High | CONDITIONAL | LOW | **CONDITIONAL** | Rendering/isolation/external-resource policy dan fidelity memerlukan PoC; self-contained safe HTML dapat lokal. |
| 15 | PDF to JPG | `FEAT-CONV-012` | YES | NO | NO | NO | Local (`L1`) | None—local baseline | Full* | High | NO | LOW | **NO** | Page rendering deterministik; banyak halaman tetap dapat background di Local Worker. |
| 16 | PDF to Word | `FEAT-CONV-004` | YES | YES | NO | **YES** | Hybrid (`H1`) | Optional | Partial | High | CONDITIONAL | LOW | **CONDITIONAL** | Layout/font/table fidelity dan fallback behavior memerlukan PoC. |
| 17 | PDF to PowerPoint | `FEAT-CONV-006` | YES | YES | NO | **YES** | Hybrid (`H1`) | Optional | Partial | High | CONDITIONAL | LOW | **CONDITIONAL** | Reconstruction/layout fidelity dan batch behavior memerlukan PoC. |
| 18 | PDF to Excel | `FEAT-CONV-005` | YES | YES | NO | **YES** | Hybrid (`H1`) | Optional | Partial | High | CONDITIONAL | LOW | **CONDITIONAL** | Table/data extraction accuracy dan complex OCR fallback memerlukan PoC. |
| 19 | PDF to PDF/A | `FEAT-PDF-075` | YES | YES | NO | **YES** | Hybrid (`H1`) | Optional | Partial | High | CONDITIONAL | MEDIUM | **CONDITIONAL** | Conformance validation, font embedding, metadata, dan repair behavior memerlukan PoC. |
| 20 | Rotate PDF | `FEAT-PDF-012` | YES | NO | NO | NO | Local (`L1`) | None—local baseline | Full* | Low | NO | LOW | **NO** | Page transform lokal dan deterministik. |
| 21 | Add page numbers | `FEAT-PDF-037` | YES | NO | NO | NO | Local (`L1`) | None—local baseline | Full* | Medium | NO | LOW | **NO** | PDF overlay/text placement lokal. |
| 22 | Add watermark | `FEAT-PDF-036` | YES | NO | NO | NO | Local (`L1`) | None—local baseline | Full* | Medium | NO | LOW | **NO** | Watermark teks/gambar adalah deterministic PDF operation. |
| 23 | Crop PDF | `FEAT-PDF-019` | YES | NO | NO | NO | Local (`L1`) | None—local baseline | Full* | Low | NO | LOW | **NO** | Page crop bekerja di working state lokal. |
| 24 | Edit PDF | `FEAT-PDF-031..041` | YES | YES | NO | **YES** | Hybrid workflow (`L1` + `H1`) | Optional—existing-text only | Partial | Medium | CONDITIONAL | MEDIUM | **CONDITIONAL** | Add text/image/shape/watermark/header/footer/overlay lokal; `FEAT-PDF-032` existing-text editing tetap feasibility-gated. |
| 25 | PDF Forms | `FEAT-PDF-053..060` | YES | NO | NO | NO | Local workflow (`L1`) | None—local baseline | Full* | Medium | NO | LOW | **NO** | Fill/create/field/flatten memakai PDF engine lokal; advanced Forms memiliki gap gate yang perlu Integrator klarifikasi. |
| 26 | Unlock PDF | `FEAT-PDF-062` | YES | NO | NO | NO | Local (`L1`) | None—local baseline | Full* | Low | NO | HIGH | **NO** | Hanya dengan password/otorisasi sah; password tidak disimpan/dilog dan tidak ada bypass protection. |
| 27 | Protect PDF | `FEAT-PDF-061` | YES | NO | NO | NO | Local (`L1`) | None—local baseline | Full* | Low | NO | MEDIUM | **NO** | Password/permission protection dapat dilakukan lokal; secret tidak boleh dikirim lewat event. |
| 28 | Sign PDF | `FEAT-SIGN-013` | YES | NO | NO | NO | Local (`L1`) | None—local baseline | Full* | Low | NO | MEDIUM | **NO** | Visual signature/paraf/stamp non-PKI melalui Signature Asset Service + PDF engine lokal; certificate/PKI out of scope. |
| 29 | Redact PDF | `FEAT-PDF-069` | YES | NO | NO | NO | Local (`L1`) | None—local baseline | Full* | Medium | NO | HIGH | **NO** | Final redaction harus menghapus/flatten konten, bukan overlay; preview, confirmation, dan output validation wajib. |
| 30 | Compare PDF | `FEAT-PDF-071 / FEAT-PDF-072` | YES | NO | NO | NO | Local (`L1`) | None—local baseline | Full* | High | NO | LOW | **NO** | Comparison dapat berat tetapi tetap Local Worker; background tidak identik dengan server. |
| 31 | AI Summarizer | `FEAT-INTEL-001` | YES | YES | NO | **YES** | Hybrid AI (`AI-H`) | Optional—internal provider | Partial/conditional | High | CONDITIONAL | MEDIUM | **CONDITIONAL** | Approved local provider bila tersedia; enterprise-internal fallback setelah policy/permission/data minimization. Public AI endpoint **NO**. |
| 32 | Translate PDF | `FEAT-INTEL-002` | YES | YES | NO | **YES** | Hybrid provider (`AI-H`) | Optional—internal provider | Partial/conditional | High | CONDITIONAL | MEDIUM | **CONDITIONAL** | Approved local/internal provider dan fidelity PoC; public AI/translation endpoint **NO**. |
| 33 | PDF to Markdown | `FEAT-CONV-036` | YES | YES | NO | **YES** | Hybrid, deterministic-first (`H1`) | Optional—OCR/complex fallback | Partial | Medium–High | CONDITIONAL | LOW | **CONDITIONAL** | Text layer diekstrak lokal tanpa AI; PoC untuk complex layout/table/OCR fidelity. AI bukan dependency default. |

## 6. API-required summary

### 6.1 Required server-processing API

**Tidak ada** dari 33 Core tool yang pada source saat ini terkunci sebagai `ServerDependency=Required` secara universal. Karena itu, tidak ada alasan membuat 33 server-processing endpoint atau satu endpoint khusus per card.

### 6.2 Conditional server-processing API

Server execution contract hanya dibutuhkan bila jalur optional server benar-benar disetujui untuk:

- Repair PDF advanced fallback;
- OCR PDF complex/large fallback;
- Word/PowerPoint/Excel to PDF;
- HTML to PDF;
- PDF to Word/PowerPoint/Excel;
- PDF to PDF/A;
- existing-text subset pada Edit PDF;
- AI Summarizer;
- Translate PDF;
- PDF to Markdown complex OCR/layout fallback.

Jika jalur tersebut menjadi server background job, response mengikuti `202 Accepted` + `jobId` + `correlationId` + state location; progress/status memakai Activity Hub dan current state tetap di REST. Jalur server yang belum lulus PoC tidak boleh dianggap endpoint production-ready.

Resource API bersama—SSO/account, capability/configuration, audit metadata, history, jobs, issue/support, Amankan ke Akun, secured file, selective sync, dan health—tetap dapat diperlukan oleh aplikasi, tetapi bukan bukti bahwa operation tool harus diproses server.

## 7. Local-only summary

Baseline server-processing API = **NO** untuk 19 card berikut:

1. Merge PDF.
2. Split PDF.
3. Remove pages.
4. Extract pages.
5. Organize PDF.
6. Scan to PDF, termasuk camera/import; physical scanner tetap PoC lokal.
7. Compress PDF.
8. JPG to PDF.
9. PDF to JPG.
10. Rotate PDF.
11. Add page numbers.
12. Add watermark.
13. Crop PDF.
14. PDF Forms baseline.
15. Unlock PDF.
16. Protect PDF.
17. Sign PDF visual non-PKI.
18. Redact PDF.
19. Compare PDF.

Job berat pada daftar ini boleh berjalan sebagai **background local job** melalui Local Worker + typed local IPC. Activity Center tetap menampilkan progress/recovery, tetapi SignalR dan server-processing API tidak diperlukan.

## 8. Optional/required-server summary

### 8.1 Optional server

Ada 14 card dengan server fallback/alternate path kondisional:

- Repair PDF;
- OCR PDF;
- Word, PowerPoint, Excel, dan HTML to PDF;
- PDF to Word, PowerPoint, Excel, dan PDF/A;
- Edit PDF untuk existing-text editing;
- AI Summarizer;
- Translate PDF;
- PDF to Markdown untuk complex OCR/layout.

Urutan runtime tetap capability-aware:

```text
Platform exposure
→ device capability
→ permission/policy
→ resource/storage
→ approved engine/provider
→ connectivity
→ local execute / optional server fallback / hide / explain
```

Sebelum isi dokumen berpindah ke server: lakukan classification/permission/policy check, data minimization, pemberitahuan/consent sesuai trust boundary, secure transfer, capacity check, dan safe audit metadata. Server tidak dipilih semata-mata untuk menyederhanakan implementasi.

### 8.2 Required server

**Tidak ada** pada canonical 33 Core mapping saat ini. Required-server dapat muncul pada runtime deployment/platform tertentu, tetapi itu tidak mengubah canonical Feature ID dan tidak otomatis mengubah feature-level dependency menjadi required untuk semua platform.

## 9. PoC dependencies

| Core tool / subset | PoC minimum | Keputusan gate yang harus dihasilkan |
|---|---|---|
| Scan to PDF — physical scanner | Driver/platform support, packaging, permissions, device matrix, failure/reconnect behavior | `APPROVED_ENGINE`, `APPROVED_WITH_LIMITATION`, atau `BLOCKED_ALTERNATIVE_REQUIRED` |
| OCR PDF | ID/EN accuracy, searchable layer, bad scan, CPU/RAM, large file; complex table/layout fallback | Sama |
| Word/Excel/PowerPoint to PDF | Fidelity, pagination, fonts, formula/chart/media, headless/platform support, license, concurrency | Sama |
| HTML to PDF | Rendering fidelity, isolation, untrusted HTML, script/external resource policy, SSRF/file access guard, license | Sama |
| PDF to Word/Excel/PowerPoint | Layout/table accuracy, batch, unsupported document behavior, fallback | Sama |
| PDF to PDF/A | Target conformance, validation, font embedding, metadata, repair behavior | Sama |
| Edit PDF — existing text | Font/layout preservation, unsupported-PDF behavior, fidelity, license | Sama |
| AI Summarizer | Approved local/internal provider, quality, privacy, classification, data minimization, timeout, quota, resource, fallback | Sama; public endpoint tetap dilarang |
| Translate PDF | Approved local/internal provider, language quality, layout fidelity, privacy, resource, fallback | Sama; public endpoint tetap dilarang |
| PDF to Markdown — complex cases | Heading/list/table/layout fidelity, OCR fallback, deterministic-first behavior, performance | Sama |

PoC tidak boleh berakhir hanya dengan pilihan vendor. Hasil wajib mencatat limitation, fallback, supported platform/format/size, security/license approval, performance/concurrency budget, dan acceptance samples.

## 10. Risks/conflicts

1. **Document 06 version mismatch.** Task menyebut Working v0.1; workspace hanya menyediakan v0.2. Report memakai baseline terbaru yang tersedia dan meminta Integrator mengonfirmasi lineage sebelum merge.
2. **`JobPaused` vs locked job state.** Source minimum hanya memiliki `WAITING_CONNECTION` dan `WAITING_RESOURCE`, bukan `PAUSED`. Report tidak menambah state. Integrator harus memutuskan apakah true user-pause masuk scope dan change control.
3. **Candidate envelope belum cukup untuk ordering.** Daftar field awal tidak memiliki event identity/stream revision. `eventId`, `streamId`, dan `sequence` direkomendasikan sebagai safe metadata minimum; tanpa itu duplicate, gap, dan stale handling tidak deterministik.
4. **Generic `L1` terlalu lebar untuk API mapping.** Feature Catalog mendefinisikan `L1` sebagai local preferred dengan optional server fallback secara generik. Untuk mencegah endpoint yang tidak perlu, report menetapkan local-only API overlay pada deterministic Core tools. Integrator perlu menyelaraskan metadata per-feature tanpa mengubah Feature ID.
5. **PDF/A priority tension.** `FEAT-PDF-075` tercatat `Could / R3` dan `FEASIBILITY_GATE`, tetapi juga termasuk 33 Core Feature Set yang diprioritaskan lebih dahulu. Integrator perlu menyelaraskan sequencing/status resmi; jangan membuat ID baru.
6. **Advanced Forms gate belum eksplisit.** Technical band menyebut advanced Forms capability/engine-gated, tetapi `FEAT-PDF-053..060` berstatus `PLANNED` dan tidak ada pada formal PoC list. Mapping tidak mengarang PoC baru; Integrator perlu mengonfirmasi cakupan “advanced Forms”.
7. **Realtime privacy leakage.** Filename, issue text, document/OCR content, signature data, dan raw error dapat bocor bila event dijadikan display payload. Contract harus code-only dan authorization group harus divalidasi server-side.
8. **Scale-out duplicate/out-of-order.** Tanpa persisted revision dan idempotent client, reconnect/publisher retry dapat meregresikan Activity Center. REST reconciliation dan per-stream sequence wajib diuji.
9. **SignalR must not become execution dependency.** Job completion/correctness tidak boleh bergantung pada event delivery. Test wajib memutus koneksi saat `QUEUED/RUNNING/RETRYING` dan membuktikan job serta recovery tetap benar.
10. **AI/provider boundary.** AI Summarizer/Translate tidak boleh menggunakan public AI endpoint. Assistant hanya mengorkestrasi canonical tool/application service yang sama; tidak menjalankan model atau arbitrary API sendiri.
11. **Sign PDF scope.** `FEAT-SIGN-013` hanya visual signature/paraf/stamp non-PKI. Certificate/PKI digital signature tetap out of scope dan tidak boleh disisipkan lewat API/event/provider.
12. **Feature ID audit result.** Tidak ditemukan mismatch pada 33 canonical mapping antara Feature Catalog v1.3, Technical Reference v1.2, dan daftar task. Tidak ada Feature ID baru yang dibuat. Range/bundle tetap dipertahankan apa adanya.

## 11. Suggested Document 06 text

Teks berikut siap dipertimbangkan Integrator untuk bagian `API-DEC-008`; status final tetap mengikuti approval/integration Document 06.

---

### API-DEC-008 — SignalR Event Envelope & Recovery Contract

**Status:** RECOMMENDED FOR LOCK — Integrator approval required

**Decision:** **Persisted REST State + Versioned At-Least-Once Realtime Notification**.

REST/persisted job, activity, dan issue state adalah source of truth. SignalR hanya mengirim notification setelah perubahan state dipersist. SignalR disconnect tidak menghentikan job, tidak menandai job failed, dan tidak menyebabkan execute command dikirim ulang. Setelah reconnect, client mengambil state authoritative melalui REST dan memulihkan Activity Center.

Minimum envelope:

```json
{
  "eventId": "evt_...",
  "eventType": "JobProgress",
  "eventVersion": 1,
  "streamId": "job:job_...",
  "sequence": 42,
  "jobId": "job_...",
  "featureId": "FEAT-PDF-005",
  "correlationId": "corr_...",
  "timestamp": "2026-08-24T10:15:30.125Z",
  "status": "RUNNING",
  "progress": 62,
  "currentStep": "MERGE_PAGES",
  "completedCount": 31,
  "totalCount": 50,
  "messageCode": "JOB_MERGING_PAGES"
}
```

Rules:

- `eventId` unique digunakan untuk deduplication.
- `streamId` + monotonically increasing `sequence` menentukan order per stream; tidak ada global order lintas-stream.
- `eventVersion` terversi per event type. Unknown/higher version tidak ditebak; client melakukan REST reconciliation.
- Delivery at-least-once. Duplicate/stale event diabaikan secara idempotent.
- Sequence gap memicu REST refresh untuk stream terkait.
- Timestamp hanya untuk display/diagnostic, bukan ordering.
- Unknown total menggunakan null/indeterminate progress, bukan progress palsu.
- Event tidak membawa document content, password, token, full OCR text, signature data, secret, prompt/chat, stack trace, atau raw engine message.
- `currentStep` dan `messageCode` adalah stable safe code yang dilokalkan di client.

Event catalog minimum:

```text
JobQueued
JobStarted
JobProgress
JobPaused
JobCompleted
JobFailed
JobCancelled
JobRetrying
ActivityUpdated
IssueStatusChanged
```

`JobPaused` pada baseline dipakai hanya untuk persisted `WAITING_CONNECTION` atau `WAITING_RESOURCE`; penambahan state `PAUSED` memerlukan keputusan state-machine terpisah.

Satu logical authenticated Activity Hub melayani event user-scoped untuk job, Activity Center, dan issue status. Server menentukan user group dari authenticated identity. Optional per-job subscription wajib memvalidasi ownership/permission. Tidak ada hub/group per Feature ID dan tidak ada document processing command pada hub.

Reconnect flow:

```text
SignalR disconnected
→ job tetap berjalan
→ automatic reconnect dengan bounded backoff + jitter
→ REST reconcile persisted job/activity/issue state
→ apply state berdasarkan persisted revision
→ resubscribe authorized active job
→ Activity Center pulih
```

Local background job memakai typed local IPC + Activity Center dan tidak memerlukan SignalR. SignalR hanya kondisional untuk server background execution. Local-only Primary Tool tidak memperoleh server-processing endpoint hanya karena memiliki card UI. Assistant memakai canonical Feature ID, application service, Job ID, Correlation ID, Activity Center, dan event contract yang sama dengan Tool-First/File-First.

Untuk 33 Primary PDF Tool Cards, server-processing API hanya dibuat untuk approved optional/required server execution path. Berdasarkan mapping saat ini, tidak ada Core tool yang universally required-server; 19 card memiliki local-only baseline dan 14 card memiliki optional server path yang capability-aware/PoC-gated. Public AI endpoint tidak diizinkan. `Sign PDF` hanya visual signature/paraf/stamp non-PKI; certificate/PKI tetap out of scope.

---
