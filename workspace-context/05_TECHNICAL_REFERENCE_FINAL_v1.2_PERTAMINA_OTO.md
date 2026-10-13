# 05 — REFERENSI TEKNIS
## PERTAMINA OTO — Office Tools Organizer

> **Standar bahasa:** Bahasa Indonesia — ID-1.0

> **Status:** FINAL — LOCKED PRE-CODING TECHNICAL BASELINE  
> **Versi:** 1.2  
> **Tanggal:** 18 Agustus 2026  
> **Parent:** `01_BUSINESS_PROCESS_FINAL_v3.4_PERTAMINA_OTO.md`; `02_REQUIREMENT_SPECIFICATION_FINAL_v1.5_PERTAMINA_OTO.md`; `03_FEATURE_CATALOG_FINAL_v1.3_PERTAMINA_OTO.md`; `04_SOLUTION_DESIGN_FINAL_v1.2_PERTAMINA_OTO.md`

---

# 1. Tujuan

Dokumen ini mengunci technical baseline untuk implementasi PERTAMINA OTO setelah Business Process, Requirement Specification, Feature Catalog, dan Solution Design selesai.

Fokus:
- runtime dan version policy;
- framework Desktop dan Mobile/Tablet;
- repository dan dependency management;
- strategi PDF/OCR/Office Conversion/Image/Content Intelligence engine/provider;
- Local Worker IPC;
- local metadata store;
- observability;
- test platform;
- security/dependency scanning;
- standar kerja Codex untuk satu developer dengan beberapa sesi paralel.

Dokumen ini tidak mengganti approval arsitektur/security/infrastruktur Pertamina.

---

# 2. Audit Konsistensi terhadap Dokumen 01–04

## 2.1 Hasil Audit

| Area | Hasil |
|---|---|
| Standar C#/.NET, ASP.NET Core, SQL Server, EF Core | PASS |
| Desktop-First + Curated Mobile/Tablet | PASS |
| Offline-First / capability-aware Local-First | PASS |
| Server-efficiency | PASS |
| Modular Monolith + Clean Architecture | PASS |
| Separate Deployables | PASS |
| Engine Adapter/Provider + Resolver | PASS |
| Local Worker isolation | PASS |
| Local Workspace + SQL metadata + Secure Storage | PASS |
| Structured Logging + Correlation + Application Insights | PASS |
| Assistant security boundary | PASS |
| Digital Signature PKI out-of-scope | PASS |
| Proof-of-Capability gate | PASS |
| Codex scoped parallel workflow | PASS |
| Fatal contradiction | **NONE** |

**Audit conclusion: PASS — no blocking contradiction. Primary Feature Audit kemudian menghasilkan controlled alignment revision 01–05: 33 Core Tool Cards ditetapkan sebagai Primary Feature Set, 4 capability canonical baru ditambahkan, dan architecture lock 18/18 tetap tidak berubah.**

## 2.2 Koreksi Minor yang Diterapkan Saat Audit

1. **Guard persetujuan Desktop/Mobile dipulihkan secara eksplisit.**
   - WinUI 3 merupakan framework Desktop terpilih dan tetap memerlukan persetujuan arsitektur/keamanan/packaging.
   - .NET MAUI merupakan framework Mobile/Tablet terpilih dan tetap memerlukan persetujuan arsitektur/keamanan/distribusi.
   - Responsive Web tetap menjadi kanal enterprise/browser dan fallback ketika klien native tidak disetujui.

2. **Baseline major runtime dibuat deterministik untuk Codex.**
   - Baseline major runtime: **.NET 10 LTS**.
   - Patch SDK tepat yang disetujui dikunci dalam `global.json` saat repository dibuat.
   - Upgrade patch tetap terkendali; downgrade/perubahan major memerlukan pengecualian eksplisit.

3. **Versi/header/penomoran bagian dokumen dinormalisasi.**

4. **Nama vendor engine sengaja belum dipilih.**
   - Strategi telah dikunci.
   - Provider PDF/OCR/Konversi Office/Image ML aktual masih memerlukan Proof-of-Capability, tinjauan lisensi/keamanan, dan benchmark bila diperlukan.

5. **Primary Feature Set First + Common-Simple-First di dalam Core Set.**
   - Controlled alignment diterapkan ke Business Process v3.4, Requirement v1.5, Feature Catalog v1.3, dan Solution Design v1.2.
   - Primary surface berisi 33 Core Tool Cards.
   - 4 capability canonical baru: Sign PDF visual, AI Summarizer, Translate PDF, PDF to Markdown.
   - Canonical count menjadi 550.

---

# 3. Register Keputusan Teknis — 15/15 LOCKED

## TECH-DEC-001 — Kebijakan Runtime dan Versi

**Keputusan:** **Baseline .NET 10 LTS + Upgrade Terkendali**.

Aturan:
- baseline major = `.NET 10 LTS`;
- patch SDK yang disetujui dikunci dengan `global.json`;
- versi package harus reproducible;
- patch keamanan diuji sebelum rollout;
- upgrade major/minor tidak otomatis;
- Codex tidak boleh melakukan upgrade package/framework massal tanpa task khusus;
- kompatibilitas dengan engine terpilih yang disetujui harus divalidasi sebelum implementasi engine tersebut.

---

## TECH-DEC-002 — Framework UI Windows Desktop

**Keputusan:** **WinUI 3**.

Aturan:
- Windows Desktop adalah klien produktivitas full-feature utama;
- aturan bisnis tetap berada di luar UI;
- Desktop dapat menggunakan interaksi Windows yang spesifik platform;
- Shared Core digunakan ulang bila sesuai;
- implementasi tetap memerlukan persetujuan arsitektur/keamanan/packaging Pertamina.

---

## TECH-DEC-003 — Framework Mobile/Tablet

**Keputusan:** **.NET MAUI** untuk companion Mobile/Tablet terkurasi.

Aturan:
- Phone/Tablet bukan salinan Desktop;
- gunakan ulang logika bisnis/kontrak/validasi bila sesuai;
- kamera, sentuhan, stylus, share, navigasi, dan capability perangkat tetap platform-aware;
- distribusi framework/klien tetap memerlukan persetujuan arsitektur/keamanan;
- Responsive Web tetap tersedia untuk skenario enterprise/browser.

---

## TECH-DEC-004 — Model Repository dan Codex Paralel

**Keputusan:** **Structured Monorepo + Scope Modul + Git Branch/Worktree per task Codex**.

Konteks:
- satu developer manusia;
- beberapa sesi/task Codex dapat berjalan paralel;
- area bersama memerlukan task khusus;
- setiap workstream diisolasi berdasarkan scope dan Git branch/worktree.

Struktur konseptual:

```text
pertamina-oto/
├── src/
│   ├── BuildingBlocks/
│   ├── Modules/
│   └── Hosts/
└── tests/
```

---

## TECH-DEC-005 — Manajemen Package/Dependensi

**Keputusan:** **Central Package Management + Persetujuan Dependensi Eksplisit**.

Baseline:
- gunakan `Directory.Packages.props` sebagai sumber versi package pusat;
- file project tidak memilih versi package secara independen;
- Codex dapat menggunakan dependensi yang telah disetujui;
- Codex tidak boleh menambah, mengganti, atau meng-upgrade package diam-diam;
- dependensi baru memerlukan alasan serta tinjauan lisensi/keamanan/kompatibilitas;
- satu developer bertindak sebagai approver sehingga proses terkendali tetapi ringan.

---

## TECH-DEC-006 — Strategi Engine PDF

**Keputusan:** **Engine PDF Utama + Fallback Khusus jika Terbukti Diperlukan**.

Aturan:
- satu engine utama menangani sebagian besar capability PDF;
- provider khusus hanya ditambahkan jika engine utama tidak memenuhi kebutuhan kualitas/fidelity/format/kinerja/keamanan/lisensi/maintenance;
- semua provider disembunyikan di balik kontrak adapter/provider PDF dan Engine Resolver;
- UI/Assistant/logika bisnis tidak pernah memilih vendor secara langsung;
- vendor/library final tetap memerlukan gerbang Proof-of-Capability.

---

## TECH-DEC-007 — Strategi Engine OCR

**Keputusan:** **OCR Utama Local-First + Fallback Khusus/Server untuk Kasus Kompleks**.

Aturan:
- OCR Bahasa Indonesia/Inggris normal dan searchable PDF mengutamakan eksekusi lokal jika capability perangkat memungkinkan;
- tabel/layout kompleks, scan buruk, atau dokumen sangat besar dapat diarahkan ke provider server khusus yang disetujui;
- teks OCR tetap merupakan data sensitif/tidak dipercaya;
- upload ke layanan eksternal/publik dilarang tanpa kebijakan yang disetujui;
- engine final harus lulus Proof-of-Capability OCR.

---

## TECH-DEC-008 — Strategi Engine Konversi Office

**Keputusan:** **Provider Konversi Hybrid**.

Aturan:
- satu provider utama menangani sebagian besar konversi Word/Excel/PowerPoint ↔ yang disetujui;
- automasi Microsoft Office Desktop **bukan** fondasi arsitektur;
- provider alternatif hanya digunakan jika fidelity/capability memerlukannya;
- provider disembunyikan di balik Conversion Module + Engine Resolver;
- konversi berisiko tinggi tetap memerlukan Proof-of-Capability.

---

## TECH-DEC-009 — Strategi Engine Pemrosesan Gambar

**Keputusan:** **Engine Gambar Deterministik Utama + Provider ML Khusus**.

Aturan:
- crop/resize/rotate/convert/compress/watermark/annotation menggunakan engine gambar deterministik biasa;
- background removal/segmentation/task khusus ML menggunakan provider khusus;
- jangan menggunakan ML jika pemrosesan gambar biasa sudah memadai;
- Local-First tetap menjadi default;
- ML/provider final memerlukan tinjauan keamanan/lisensi/kinerja dan Proof-of-Capability bila berlaku.

---

## TECH-DEC-010 — IPC Desktop ↔ Worker Lokal

**Keputusan:** **Named Pipe/IPC Lokal + Kontrak Bertipe dan Berversi**.

Aturan:
- berlaku untuk Windows Desktop ↔ Worker Lokal terisolasi;
- endpoint jaringan publik tidak diperlukan untuk kanal lokal ini;
- transport diabstraksikan, misalnya `ILocalWorkerTransport`;
- mendukung command, progress, cancellation, status, heartbeat, dan error;
- crash Worker tidak boleh menghentikan UI Desktop;
- implementation spike hanya dapat mengganti transport jika blocker platform/keamanan nyata terbukti.

---

## TECH-DEC-011 — Penyimpanan Metadata Workspace Lokal

**Keputusan:** **SQLite untuk metadata/state lokal**.

Pemisahan:

```text
User files / results
→ Local Workspace filesystem

Local app metadata/state
→ SQLite

Enterprise/server data
→ SQL Server
```

SQLite dapat memuat:
- workspace state;
- draft references;
- job state;
- checkpoint/recovery metadata;
- local history metadata;
- presets;
- favorites.

Aturan:
- logika bisnis tidak boleh terikat langsung ke SQLite;
- gunakan repository/abstraction;
- migration schema harus berversi dan diuji;
- data lokal sensitif mengikuti aturan perlindungan Security Design;
- SQLite tidak menggantikan SQL Server enterprise.

---

## TECH-DEC-012 — Stack Observability dan Logging

**Keputusan:** **Microsoft.Extensions.Logging + OpenTelemetry + Azure Monitor/Application Insights**.

Aturan:
- log/trace/metric terstruktur melalui abstraction standar;
- Correlation ID konsisten di Desktop/Web/API/Worker/job;
- Application Insights tetap menjadi backend observability enterprise;
- jangan mencatat konten dokumen, password, token, secret, clipboard sensitif, artefak tanda tangan, atau teks OCR lengkap;
- filtering/sampling/retention/minimisasi data dirinci kemudian.

---

## TECH-DEC-013 — Framework dan Platform Pengujian

**Keputusan:** **MSTest + Microsoft.Testing.Platform (MTP)**.

Aturan:
- MSTest adalah framework utama pengujian unit/integrasi .NET;
- MTP adalah runner/platform utama;
- satu repository menggunakan platform pengujian yang konsisten;
- pengujian harus dapat berjalan lokal dan dalam Azure DevOps Pipeline;
- architecture/contract/E2E/performance dapat menggunakan tool tambahan yang disetujui bila diperlukan;
- Codex wajib membuat pengujian yang relevan untuk setiap task.

---

## TECH-DEC-014 — Pemindaian Keamanan dan Dependensi

**Keputusan:** **Pemindaian Keamanan Berlapis Otomatis**.

Minimum:
- audit kerentanan dependensi;
- pemindaian secret;
- analisis statis kode/keamanan;
- gerbang build/deployment berbasis severity;
- waiver terdokumentasi eksplisit jika temuan diterima sebagai pengecualian;
- add-on komersial dapat ditambahkan bila berlisensi, tetapi baseline tidak boleh hanya bergantung pada add-on berbayar;
- Codex tidak boleh menonaktifkan pemeriksaan keamanan hanya agar pipeline berstatus hijau.

---

## TECH-DEC-015 — Standar Pengembangan Codex Paralel

**Keputusan:** **Task Berumur Singkat dan Terbatas + Git Branch/Worktree Khusus + Human Merge Gate**.

Aturan:
1. Satu task Codex = satu tujuan/scope yang jelas.
2. Setiap task menggunakan branch/worktree khusus.
3. Setiap task menyebut Task ID, Requirement ID, Feature ID, Allowed Paths, Do-Not-Touch Paths, dependensi, kriteria penerimaan, dan pengujian.
4. Kontrak/dependensi/arsitektur/aturan bisnis bersama tidak boleh berubah kecuali task mengizinkannya secara eksplisit.
5. Refactor yang tidak terkait dilarang.
6. Branch harus berumur singkat.
7. Build + pengujian + pemeriksaan keamanan wajib sebelum merge.
8. Developer manusia adalah reviewer/integrator/approver final.
9. Perubahan fondasi bersama menggunakan task fondasi terpisah.
10. Penerimaan yang gagal diperbaiki dalam task asal, bukan dengan mengubah modul tidak terkait secara diam-diam.

---

# 4. Guard Teknis yang Dipertahankan dari Desain Solusi

Ketentuan berikut tetap tidak berubah:

- Modular Monolith + Clean Architecture.
- Separate deployables: Web, API, Worker, Desktop, Mobile.
- Satu SQL Server dengan kepemilikan schema/data per modul.
- Klien tidak pernah mengakses SQL Server secara langsung.
- Local Workspace + SQL metadata + Secure File/Object Storage.
- Engine Adapter/Provider + Engine Resolver.
- Pemrosesan langsung untuk pekerjaan ringan; background job untuk pekerjaan berat.
- Worker Lokal terisolasi untuk pekerjaan lokal berat/berisiko.
- Adaptive Progress + Activity Center + No Silent Wait.
- Operation-based working state + autosave + checkpoint.
- Typed Canonical Feature Registry.
- Selective Cross-Device Sync via **Amankan ke Akun**.
- Maksimum tiga versi file akun.
- REST + SignalR + REST refresh fallback.
- Structured logging + Correlation ID.
- In-App Issue Reporting + Superadmin Support Console.
- Digital Signature Certificate/PKI tetap di luar ruang lingkup.
- Assistant tetap sebagai orchestrator, bukan engine pemrosesan.

---

# 5. Proof-of-Capability yang Masih Diperlukan

Sebelum implementasi penuh, hal berikut tetap memerlukan gerbang:

- edit existing PDF text;
- Office → PDF;
- PDF → Word/Excel/PowerPoint;
- complex table OCR;
- background removal;
- HTML/Web → PDF;
- PDF/A;
- physical scanner integration;
- optional local AI model.
- AI Summarizer provider.
- Translate PDF provider/fidelity.
- complex PDF to Markdown layout/table fidelity.

Gerbang minimum:
- output quality;
- format support;
- license/cost;
- security;
- platform support;
- CPU/RAM;
- large-file behavior;
- concurrency;
- maintainability;
- fallback.

---

# 6. Prinsip Implementasi Awal — PRIMARY FEATURE SET FIRST

Arahan pengguna:

> 33 tool pada Primary PDF Tool Surface adalah fitur utama yang harus dibuat terlebih dahulu. Setelah Core Feature Set selesai/ditetapkan status feasibility resminya, barulah user-tool sekunder dikerjakan.

Interpretasi teknis:
- Foundation/security/auth/audit/storage/observability/platform dependency tetap boleh dikerjakan lebih awal karena dibutuhkan Core tools.
- **Common-Simple-First tetap dipakai di dalam Core Set**, bukan untuk mendahulukan secondary tool.
- Gated feature mendapat PoC lebih awal agar blocker diketahui sebelum integrasi terlambat.
- Secondary user-tool tidak masuk active implementation queue sebelum Core Gate terpenuhi.

Kategori Core:
1. Organize PDF
2. Optimize PDF
3. Convert to PDF
4. Convert from PDF
5. Edit PDF
6. PDF Security
7. PDF Intelligence

Total: **33 Primary Tool Cards**.

# 7. Vertical Slice Pertama yang Direkomendasikan untuk Pengguna

## FITUR PERTAMA — `FEAT-PDF-005` Merge PDF

**Rekomendasi:** **Merge PDF adalah fitur pertama untuk pengguna yang diimplementasikan.**

Alasan:
- task kantor yang sangat umum;
- model mental sederhana: beberapa PDF → satu PDF;
- katalog canonical menandainya sebagai `PRIMARY_TOOL`;
- profil eksekusi `L1`/mengutamakan lokal;
- risiko `LOW`;
- ditargetkan untuk MVP;
- menguji fondasi penting tanpa kompleksitas OCR/AI/server.

Implementasi pertama adalah **vertical slice**, bukan hanya fungsi PDF.

Scope end-to-end minimum:

```text
Pick / Drag PDF files
→ Validate files
→ Show selected order
→ Allow reorder/remove
→ Execute Merge locally
→ Show progress/status
→ Validate result
→ Save As new file
→ Never overwrite original silently
→ Record safe local history metadata
→ Structured log + Correlation ID
→ Unit/integration tests
```

Capability pendukung hanya diimplementasikan sejauh diperlukan vertical slice ini:
- Feature Registry;
- input adapter;
- file validation;
- PDF provider abstraction + resolver;
- Local Workspace/temp management;
- result/save service;
- error model;
- basic progress;
- SQLite metadata;
- logging/telemetry abstraction;
- automated tests/security checks.

Pendekatan ini membuktikan arsitektur melalui satu fitur nyata sebelum ekspansi horizontal.

---

# 8. Program Fitur Core — 33 Primary Tools

Backlog Core adalah:

1. **Merge PDF** — `FEAT-PDF-005`
2. **Split PDF** — `FEAT-PDF-006 / FEAT-PDF-007`
3. **Remove pages** — `FEAT-PDF-013`
4. **Extract pages** — `FEAT-PDF-014`
5. **Organize PDF** — `FEAT-PDF-010..016`
6. **Scan to PDF** — `FEAT-SCAN-014`
7. **Compress PDF** — `FEAT-PDF-023`
8. **Repair PDF** — `FEAT-PDF-025 / FEAT-PDF-026`
9. **OCR PDF** — `FEAT-OCR-004`
10. **JPG to PDF** — `FEAT-CONV-007`
11. **Word to PDF** — `FEAT-CONV-001`
12. **PowerPoint to PDF** — `FEAT-CONV-003`
13. **Excel to PDF** — `FEAT-CONV-002`
14. **HTML to PDF** — `FEAT-CONV-017`
15. **PDF to JPG** — `FEAT-CONV-012`
16. **PDF to Word** — `FEAT-CONV-004`
17. **PDF to PowerPoint** — `FEAT-CONV-006`
18. **PDF to Excel** — `FEAT-CONV-005`
19. **PDF to PDF/A** — `FEAT-PDF-075`
20. **Rotate PDF** — `FEAT-PDF-012`
21. **Add page numbers** — `FEAT-PDF-037`
22. **Add watermark** — `FEAT-PDF-036`
23. **Crop PDF** — `FEAT-PDF-019`
24. **Edit PDF** — `FEAT-PDF-031..041`
25. **PDF Forms** — `FEAT-PDF-053..060`
26. **Unlock PDF** — `FEAT-PDF-062`
27. **Protect PDF** — `FEAT-PDF-061`
28. **Sign PDF** — `FEAT-SIGN-013`
29. **Redact PDF** — `FEAT-PDF-069`
30. **Compare PDF** — `FEAT-PDF-071 / FEAT-PDF-072`
31. **AI Summarizer** — `FEAT-INTEL-001`
32. **Translate PDF** — `FEAT-INTEL-002`
33. **PDF to Markdown** — `FEAT-CONV-036`

Urutan engineering yang direkomendasikan **tidak harus mengikuti urutan visual kiri-ke-kanan**. Mulai dengan Merge PDF sebagai vertical slice pertama, lalu maksimalkan reuse sambil menjalankan PoC lebih awal untuk fitur yang memiliki gerbang.

## 8.1 Kelompok Teknis

**Kelompok A — fondasi + Core lokal berisiko rendah**
Merge/Split/Remove/Extract/Organize, Compress/Repair, JPG↔PDF, Rotate, Page Number, Watermark, Crop, Protect, Compare, basic Sign PDF and Forms where approved.

**Kelompok B — Core dengan gerbang capability/engine**
Scan to PDF where physical scanner is involved, OCR PDF, Office→PDF, PDF→Office, HTML→PDF, PDF/A, existing-text Edit PDF, advanced Forms.

**Kelompok C — Core berisiko tinggi/intelligence**
Unlock PDF, Redact PDF, AI Summarizer, Translate PDF, and complex PDF→Markdown cases.

Kelompok memungkinkan task Codex paralel, tetapi **semuanya tetap Core** dan harus selesai/diputuskan sebelum tool pengguna sekunder.

## 8.2 Adendum Teknis — PDF Intelligence

- `AI Summarizer` and `Translate PDF` use `IContentIntelligenceProvider` + resolver.
- Local/approved enterprise-internal provider only.
- No public AI endpoint.
- Document content is untrusted and minimized.
- `PDF to Markdown` uses deterministic extraction first; OCR fallback as needed.
- Provider/vendor remains Proof-of-Capability gated.

## 8.3 Adendum Teknis — Sign PDF

`Sign PDF` means **visual signature/paraf non-PKI**. It composes Signature Asset Service + PDF Engine through canonical `FEAT-SIGN-013`. Certificate/PKI digital signing remains out of scope.

# 9. Status

```text
Document  : 05_TECHNICAL_REFERENCE.md
Version   : 1.2
Status    : FINAL — LOCKED PRE-CODING TECHNICAL BASELINE
Decisions : 15 / 15 LOCKED
Audit     : PASS — no blocking contradiction
Upstream alignment : COMPLETED — 01 v3.4 / 02 v1.5 / 03 v1.3 / 04 v1.2
Next documentation : 06_API_DOCUMENTATION.md
First implementation recommendation : FEAT-PDF-005 Merge PDF vertical slice
Core user-facing backlog : 33 Primary Tool Cards before secondary user tools
```
