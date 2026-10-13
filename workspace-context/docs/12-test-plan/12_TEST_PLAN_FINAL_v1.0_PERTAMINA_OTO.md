# 12 — TEST PLAN
## PERTAMINA OTO — Office Tools Organizer

> **Standar bahasa:** Bahasa Indonesia — ID-1.0  
> **Versi:** 1.0  
> **Tanggal:** 28 Agustus 2026  
> **Status:** FINAL — PRE-CODING BASELINE  
> **Keputusan Test Plan:** 25/25 AUTO-LOCKED  
> **Baseline fitur canonical:** 550  
> **Prioritas utama:** 33 Primary PDF Tool Cards  
> **Vertical slice pertama:** `FEAT-PDF-005 — Merge PDF`

---

# 1. Tujuan

Dokumen ini menjawab:

> Bagaimana sistem diuji secara menyeluruh agar benar secara teknis dan bisnis, aman, nyaman digunakan, cepat, stabil, dapat dipulihkan, sesuai platform, serta siap diserahkan ke UAT?

Test Plan ini:

- mengorkestrasi seluruh test selain detail Unit Test;
- memastikan setiap hasil teknis tetap diuji terhadap business outcome dan pengalaman user;
- menyediakan strategi untuk 550 fitur canonical dengan prioritas 33 Core;
- menjaga Offline-First, Local-First, keamanan, dan anggaran server;
- menetapkan evidence dan gate yang dapat dipakai saat coding dimulai;
- menjadi pintu masuk ke Dokumen 13, bukan pengganti UAT.

---

# 2. Ruang Lingkup

## 2.1 Termasuk

```text
Unit gate dan handoff dari Dokumen 11
Architecture
Integration dan Component
Contract dan API
Database dan Storage
Worker dan IPC
Functional dan Business Process
E2E dan UI/UX
Security dan Privacy
Performance, Load, Stress, Soak, Reliability
Recovery dan Fault Injection
Offline, Sync, Compatibility, Cross-platform
Regression, Smoke, Release Validation
Test management, data, environment, automation, manual, defect, evidence, reporting
UAT readiness
```

## 2.2 Tidak Termasuk

- detail 50 Unit case Merge yang sudah menjadi otoritas Dokumen 11;
- langkah UAT, business sign-off, dan hasil UAT—milik Dokumen 13;
- pemilihan vendor/tool/engine/provider berbayar;
- angka SLA, RPO/RTO, throughput, latency, atau concurrency yang belum disetujui;
- coding aplikasi, pipeline, test harness, atau production configuration;
- VA/PT aktif pada Production tanpa otorisasi khusus;
- certificate/PKI Digital Signature.

Sign PDF hanya berarti tanda tangan visual/paraf non-PKI.

---

# 3. Sumber Berwenang dan Baseline

```text
01 BUSINESS PROCESS FINAL v3.4
02 REQUIREMENT SPECIFICATION FINAL v1.5
03 FEATURE CATALOG FINAL v1.3 — 550 fitur canonical
04 SOLUTION DESIGN FINAL v1.2 — 18/18 LOCKED
05 TECHNICAL REFERENCE FINAL v1.2 — 15/15 LOCKED
06 API DOCUMENTATION FINAL v1.0 — 9/9 LOCKED
07 DATABASE DESIGN FINAL v1.0 — 12/12 AUTO-LOCKED
08 SECURITY DESIGN FINAL v1.0 — 20/20 AUTO-LOCKED
09 ROLE MATRIX FINAL v1.0 — 18/18 AUTO-LOCKED
10 AUDIT TRAIL DESIGN FINAL v1.0 — 20/20 AUTO-LOCKED
11 UNIT TEST DOCUMENT FINAL v1.0 — 20/20 AUTO-LOCKED
MASTER PROJECT SUMMARY v3.4
```

Laporan Agent A–D adalah masukan specialist. Jika berbeda, Dokumen 01–11 lebih berwenang.

---

# 4. Sasaran Pengujian

1. Membuktikan requirement dan business outcome yang dirilis.
2. Membuktikan canonical Feature, contract, permission, processing route, dan result tetap konsisten pada File-First, Tool-First, dan Assistant-First.
3. Membuktikan source aman dan output hanya final setelah validation.
4. Membuktikan real boundary HTTP, SQL, SQLite, filesystem, storage, Worker, IPC, engine, dan platform.
5. Menemukan error, race, abuse, resource exhaustion, recovery gap, dan incompatible change sebelum UAT.
6. Mengukur server/device budget dengan metode reproducible tanpa mengarang SLA.
7. Memastikan platform menunjukkan capability, unavailable state, fallback, dan Adaptive UX dengan benar.
8. Menyediakan evidence aman dan traceable untuk defect, release gate, UAT readiness, dan handover.

---

# 5. Prinsip Pengujian

1. **Business outcome lebih utama daripada technical green.**
2. **Risk-based dan boundary-first.** Test ditempatkan pada lapisan termurah yang memberi bukti benar.
3. **Canonical parity.** Semua entry flow memakai Feature ID, schema, authorization, resolver, dan finality rule yang sama.
4. **Offline-First dan Local-First.** Local-only tidak boleh membuat server-processing traffic.
5. **REST/persisted state berwenang.** SignalR hanya notifikasi dengan REST reconciliation.
6. **Negative test wajib.** Invalid, unauthorized, conflict, failure, cancellation, retry, recovery, dan overload bukan kasus tambahan.
7. **Real boundary, real claim.** Fake tidak boleh diklaim membuktikan persistence, wire, OS, device, atau engine fidelity nyata.
8. **Synthetic-first dan privacy-safe.** Test data serta evidence tidak boleh menjadi kanal kebocoran.
9. **Tidak ada angka rekaan.** Benchmark mendahului target; policy berasal dari approval.
10. **Anti-duplikasi.** Registry/family/contract tests memberi breadth; E2E dipilih berdasarkan risiko.
11. **No false pass.** Blocked, not run, inconclusive, rerun, quarantine, atau missing test bukan PASS.
12. **Satu artifact dipromosikan.** QA, UAT, dan Production tidak memakai rebuild berbeda.

---

# 6. Pemetaan Definition of Done 8 Lapis

| Lapis | Bukti minimum | Gate pemilik |
|---|---|---|
| 1. Technical Correctness | Build, Unit, Architecture, Contract, Integration, version compatibility | BUILD/UNIT/INTEGRATION |
| 2. Business Process Correctness | Functional flow, alternate/error flow, triple-entry parity, validated outcome | FUNCTIONAL/REGRESSION |
| 3. UI/UX Acceptance | Usability, Adaptive UX, progress, no silent wait, accessibility dasar, platform-natural interaction | UX/E2E/MANUAL |
| 4. Security & Privacy | AuthN/AuthZ, ownership, malicious input, canary exclusion, scanning, VA/PT applicability | SECURITY |
| 5. Performance / Server Budget | CPU/RAM/storage/network/DB/API/queue/Worker/Audit/concurrency baseline dan approved gate | PERFORMANCE |
| 6. Error & Recovery | Error mapping, cancel/retry/resume/checkpoint, crash/restart, restore, source/finality | RECOVERY/RELIABILITY |
| 7. Cross-Platform Behavior | Exposure/capability, adaptive layout, unavailable explanation, compatibility, real-device evidence | PLATFORM/COMPATIBILITY |
| 8. Test & Documentation | Traceability, regression, evidence, report, updated documentation, UAT package | RELEASE/UAT READINESS |

Kegagalan satu lapis berarti Feature belum `DONE`, walaupun build dan Unit Test lulus.

---

# 7. Kriteria Masuk

Test pada suatu scope boleh dimulai ketika prasyarat yang relevan tersedia:

```text
[ ] Requirement ID dan canonical Feature ID/resource contract diketahui
[ ] risk, owner, processing profile, server dependency, dan platform exposure diketahui
[ ] expected public behavior dan forbidden side effect dapat diuji
[ ] typed contract/schema/version tersedia untuk scope yang dirilis
[ ] build berhasil dan affected Unit P0/P1 lulus
[ ] environment, identity, permission/policy, configuration, dan capability sehat/terversi
[ ] Dataset ID/corpus/engine version tersedia dan diizinkan
[ ] cleanup, isolation, observability, safe evidence, dan Correlation ID strategy tersedia
[ ] failure injection mempunyai guard dan recovery runbook bila berlaku
[ ] PoC prerequisite lulus atau status BLOCKED tercatat—bukan PASS
```

---

# 8. Kriteria Keluar

```text
[ ] seluruh applicable P0/P1 dan critical business scenarios PASS
[ ] tidak ada unresolved BLOCKER/CRITICAL
[ ] no open disallowed Critical/High security finding
[ ] source/finality, authorization, privacy, Local-First, dan Audit invariants PASS
[ ] required Integration/Contract/API/DB/Worker/E2E boundaries PASS
[ ] recovery, offline/reconnect, and regression scope PASS
[ ] performance baseline valid dan approved numeric target dipenuhi bila sudah berlaku
[ ] platform/compatibility combinations yang dirilis memiliki evidence/disposition
[ ] PoC-gated Feature memiliki approved outcome/limitation/blocker
[ ] traceability dan safe evidence lengkap
[ ] immutable release candidate serta UAT entry package siap
```

Jika target numerik belum disetujui, gate terkait adalah `BLOCKED_PENDING_TARGET_APPROVAL`; bukan PASS.

---

# 9. Level Pengujian

| Level | Tujuan | Bukti nyata minimum | Batas |
|---|---|---|---|
| Unit | Rule kecil deterministik | Sesuai Dokumen 11 | Detail tidak diulang |
| Architecture | Module ownership dan forbidden dependencies | Assembly/reference graph | Bukan runtime wiring |
| Component/Functional | Behavior capability dari public component surface | Host/component + selected real adapter | Bukan seluruh journey |
| Integration | Komunikasi dua atau lebih komponen | Real boundary terkontrol | Tidak otomatis membuktikan UX |
| Contract | Wire/schema/version compatibility | OpenAPI, serializer, IPC/event schemas | Tidak membuktikan fidelity |
| E2E | Journey bernilai tinggi | Client + required stack + validated public outcome | Tidak exhaustive |
| Security | Kontrol dan abuse paths | Authorized negative/attack evidence | VA/PT terpisah sesuai scope |
| Performance/Reliability | Resource, capacity, stability, recovery | Representative environment dan instrumentation | Tidak memakai angka rekaan |
| Manual/Exploratory | Judgment fidelity, UX, device, semantic quality | Charter dan safe observations | Tidak menggantikan deterministic gate |
| UAT | Penerimaan user/business | Dimiliki Dokumen 13 | Dokumen 12 hanya readiness |

---

# 10. Jenis Pengujian

```text
Functional
Business Process
UI/UX dan Accessibility dasar
Integration dan Component
Contract dan API
Database, Storage, Migration
Worker dan IPC
E2E
Authentication, Authorization, Security, Privacy
Performance, Load, Concurrency, Stress, Soak
Reliability, Recovery, Fault Injection, Restore
Offline, Selective Sync
Compatibility, Versioning, Cross-platform
Regression, Smoke, Release Validation
Static, Dependency, Secret, Configuration, Malicious Corpus, VA/PT readiness
```

---

# 11. Lingkungan Pengujian

| Environment/lane | Tujuan | Data | Test yang diperbolehkan | Larangan utama |
|---|---|---|---|---|
| Development | Local feedback, Unit, static, component awal | Synthetic minimal | Unit/architecture/contract logic/isolated component | Production data/secret, shared DoS, public AI |
| QA/Test | Authoritative system verification | Synthetic, sanitized approved, corrupted/boundary corpus | Integration, Contract, E2E, Security, Recovery, Performance bounded | Raw Production data, unbounded attack/load, manual patch |
| UAT logical lane | Business validation setelah QA gate | Synthetic/sanitized approved | Pre-UAT smoke dan Dokumen 13 scenarios | Malicious corpus, penetration, load/DoS, debug, direct DB |
| Production | Live operation | Operational data | Bounded health/smoke/canary yang disetujui | General testing, fuzz, load, malicious corpus, untracked mutation |

UAT harus terisolasi secara logis melalui artifact, configuration, data, access, evidence, dan test window. Dedicated stack atau isolated QA lane memerlukan konfirmasi Pertamina; Test Plan tidak membuat topology baru.

Environment manifest minimum:

```text
environment/lane ID
build/commit/artifact digest
configuration/policy/schema/capability versions
OS/device/browser/runtime/engine/provider versions
CPU/RAM/disk/network/SQL/storage topology
identity and permission fixture versions
dataset/corpus versions
instrumentation configuration
owner, test window, stop condition, cleanup/recovery point
```

---

# 12. Strategi Data Test

| Kelas data | Penggunaan | Kontrol |
|---|---|---|
| Synthetic domain data | Default seluruh layer | Fictional, deterministic, no real identity/secret |
| Generated PDF/image | Format/layout/size combinations | Versioned generator/seed/license/digest |
| Sanitized representative | Fidelity yang tidak cukup sintetis | Approval, sanitization, verification, restricted access |
| Corrupted/malicious corpus | Parser/isolation/security negatives | Quarantined, hashed, manifested, safe runner |
| Boundary data | below/equal/above policy | Generated from configuration, bounded cleanup |
| Golden/reference output | Stable semantic/fidelity oracle | Reviewed property baseline; controlled update |

Dataset manifest:

```text
Dataset ID/version
purpose + Requirement/Feature/Test IDs
source/generator/provenance/license
classification/sensitivity
format/count/size/digest
expected property/result
allowed environment/suite
owner/access
created/reviewed/expiry
retention/disposal
known malicious/active behavior
```

Dilarang secara default: Production document/database/export/backup/log/dump; password/token/cookie/secret/key/connection string; real signature; full OCR/prompt/provider/Issue content; real employee identity/path; atau dataset publik tanpa lisensi/provenance jelas.

---

# 13. Identitas, Template, dan Status Test

## 13.1 Namespace ID

| Prefix | Suite |
|---|---|
| `FT-<FAMILY>-NNN` | Functional/Component |
| `BF-<ENTRY>-NNN` | Business flow/parity |
| `INT-<BOUNDARY>-NNN` | Integration |
| `CT-<CONTRACT>-NNN` | Contract |
| `API-INT-NNN` | API Integration |
| `DB-INT-NNN` | Database/Storage Integration |
| `WRK-INT-NNN` | Worker/IPC Integration |
| `E2E-<FEATURE>-NNN` | End-to-End |
| `SEC-<AREA>-NNN` | Security/Privacy |
| `PERF-<AREA>-NNN` | Performance/Load/Stress |
| `REC-<AREA>-NNN` | Recovery/Reliability |
| `REG-<PACK>-NNN` | Regression |

## 13.2 Template

```text
Test ID dan Title
Requirement ID
Feature ID atau resource contract
risk dan priority
test level/type
platform/environment
Dataset/Corpus ID
precondition
action/steps
expected public outcome
forbidden side effect
security/privacy assertion
performance/recovery assertion bila berlaku
automation/manual status
evidence location
last build/result
defect/UAT/release handoff
```

## 13.3 Status

| Status | Arti |
|---|---|
| `PASS` | Dieksekusi dan seluruh expected result terpenuhi |
| `FAIL` | Dieksekusi dan minimal satu expected result tidak terpenuhi |
| `BLOCKED` | Tidak dapat dijalankan karena blocker yang diketahui |
| `NOT_RUN` | Direncanakan tetapi belum dijalankan; alasan wajib |
| `INCONCLUSIVE` | Environment/data/counter/generator/evidence tidak valid |
| `N/A` | Terbukti tidak berlaku; alasan scope wajib |

Diagnostic rerun ditampilkan bersama failure awal dan tidak menghapusnya.

---

# 14. Coverage Fitur

Coverage berarti outcome evidence, bukan sekadar link Test ID atau line percentage.

```text
Requirement
→ canonical Feature/resource
→ risk + business/security rule
→ implementation family + processing mode
→ platform exposure + environment
→ required test layers
→ Test ID/result/evidence
→ defect/disposition
→ UAT scope
→ release gate
```

Registry-wide checks mencakup seluruh 550 record untuk uniqueness, canonical mapping, status/exposure, schema/version, permission/policy metadata, handler capability, result/error/Audit mapping, dan absence of invented endpoint/engine.

Hanya Feature yang dirilis/exposed yang memerlukan executable end-user scenario. Feature yang belum dirilis tetap harus mempunyai governed status dan tidak boleh dihitung PASS.

---

# 15. Pengujian Berbasis Risiko

Risk score tidak menggantikan judgment. Scope dipilih dari:

```text
business criticality
security/privacy and ownership
source/data corruption/destructive effect
implementation complexity and PoC uncertainty
number of boundaries
Local/Server/Hybrid route
shared-contract blast radius
platform/device exposure
large-resource/concurrency/recovery risk
historical defect and change frequency
```

| Risiko | Test minimum |
|---|---|
| P0 kritis | Negative security/data/source/finality/Audit/recovery; blocks release |
| P1 Core/business | Primary + alternate/error + integration/contract yang berlaku |
| P2 normal | Common variant, supported compatibility, standard recovery |
| P3 kredibel | Rare/edge/defensive berdasarkan evidence risiko |

Test Priority mengikuti Dokumen 11. Defect priority menggunakan namespace berbeda pada Bagian 47.

---

# 16. Strategi 33 Primary PDF Tool Cards

Setiap Core Card memperoleh coverage melalui:

```text
registry-wide completeness
+ implementation-family conformance
+ processing-mode/Local-First contract
+ platform adapter untuk exposure aktual
+ distinctive feature scenario
+ PoC/security/fidelity evidence bila berlaku
```

Pendekatan ini mencegah 33 × seluruh variasi tanpa menghilangkan risiko pembeda. Setiap card minimal mempunyai:

- primary business outcome;
- validation/error/cancellation yang berlaku;
- source dan finality assertion;
- permission/privacy/Audit assertion;
- correct processing route dan forbidden server side effect;
- distinctive scenario;
- platform/exposure evidence;
- PoC/limitation bila gated.

---

# 17. Matriks Coverage 33 Core

Legenda platform: `D` Desktop; `W*` Web hanya jika resmi browser-safe/server-capable; `P/T*` Phone/Tablet hanya jika registry expose; `M/T` mobile/tablet platform-natural.

| No | Primary Tool / Feature | Risk | Family | Mode | Platform minimum | PoC | Bukti pembeda |
|---:|---|:---:|---|---|---|:---:|---|
| 1 | Merge PDF `FEAT-PDF-005` | Low | Page Operations | LOCAL | D + P/T* simple | No | Full first-slice: order, atomicity, source, result |
| 2 | Split PDF `FEAT-PDF-006/007` | Low | Page Operations | LOCAL | D + P/T* | No | Range, output manifest/count |
| 3 | Remove pages `FEAT-PDF-013` | Low | Page Operations | LOCAL | D + P/T* | No | Selection dan minimum remaining page |
| 4 | Extract pages `FEAT-PDF-014` | Low | Page Operations | LOCAL | D + P/T* | No | Selected page identity/order |
| 5 | Organize PDF `FEAT-PDF-010..016` | Low | Page Workflow | LOCAL | D + P/T* | No | Workflow bundle tanpa engine/endpoint baru |
| 6 | Scan to PDF `FEAT-SCAN-014` | Low | Scan/Device | LOCAL/device | M/T + D device | Yes | Camera/scanner lifecycle, unavailable/cancel/final PDF |
| 7 | Compress PDF `FEAT-PDF-023` | Low | Optimize | LOCAL | D + P/T* | No | Size-quality/options/resource properties |
| 8 | Repair PDF `FEAT-PDF-025/026` | Low | Repair | OPTIONAL fallback | D + W* | No formal | No false repair success; local-first fallback |
| 9 | OCR PDF `FEAT-OCR-004` | Medium | OCR | OPTIONAL complex | D + P/T* + W* | Yes | Searchability, page/language, fidelity/resource |
| 10 | JPG to PDF `FEAT-CONV-007` | Low | Conversion to PDF | LOCAL | D + P/T* | No | Image order/page dimension |
| 11 | Word to PDF `FEAT-CONV-001` | Low | Office to PDF | OPTIONAL | D + W* | Yes | Layout/font/active-content corpus |
| 12 | PowerPoint to PDF `FEAT-CONV-003` | Low | Office to PDF | OPTIONAL | D + W* | Yes | Slide order/layout fidelity |
| 13 | Excel to PDF `FEAT-CONV-002` | Low | Office to PDF | OPTIONAL | D + W* | Yes | Sheet/print-area/formula safety |
| 14 | HTML to PDF `FEAT-CONV-017` | Medium | Web/Text to PDF | OPTIONAL | D + W* | Yes | No arbitrary remote URL/active content/SSRF |
| 15 | PDF to JPG `FEAT-CONV-012` | Low | Conversion from PDF | LOCAL | D + P/T* | No | Page count/resolution/color |
| 16 | PDF to Word `FEAT-CONV-004` | Low | PDF to Office | OPTIONAL | D + W* | Yes | Layout/editability fidelity |
| 17 | PDF to PowerPoint `FEAT-CONV-006` | Low | PDF to Office | OPTIONAL | D + W* | Yes | Slide segmentation/order |
| 18 | PDF to Excel `FEAT-CONV-005` | Low | PDF to Table | OPTIONAL | D + W* | Yes | Table/cell fidelity dan formula-injection safety |
| 19 | PDF to PDF/A `FEAT-PDF-075` | Medium | Conformance | OPTIONAL | D + W* | Yes | Independent conformance evidence |
| 20 | Rotate PDF `FEAT-PDF-012` | Low | Page Operations | LOCAL | D + P/T* | No | Rotation normalization |
| 21 | Add page numbers `FEAT-PDF-037` | Low | Edit/Annotation | LOCAL | D + P/T* | No | Range/start/position |
| 22 | Add watermark `FEAT-PDF-036` | Low | Edit/Annotation | LOCAL | D + P/T* | No | Opacity/position/repeat |
| 23 | Crop PDF `FEAT-PDF-019` | Low | Page Geometry | LOCAL | D + P/T* | No | Bounds/clipping/output boxes |
| 24 | Edit PDF `FEAT-PDF-031..041` | Medium | Editing Workflow | OPTIONAL subset | D + limited P/T* + W* | Yes | Existing-text fidelity/unsupported fallback |
| 25 | PDF Forms `FEAT-PDF-053..060` | Low | Forms Workflow | LOCAL baseline | D + P/T* | No formal | Field identity, save/reopen, action safety |
| 26 | Unlock PDF `FEAT-PDF-062` | High | PDF Security | LOCAL | D + P/T* | No | Valid password/authority only; no bypass; L3 |
| 27 | Protect PDF `FEAT-PDF-061` | Medium | PDF Security | LOCAL | D + P/T* | No | Reopen protection, password privacy, L3 |
| 28 | Sign PDF `FEAT-SIGN-013` | Medium | Visual Signature | LOCAL non-PKI | M/T + D | No | Placement/privacy/L3; no PKI claim |
| 29 | Redact PDF `FEAT-PDF-069` | High | Redaction | LOCAL | D + P/T* | No | Forensic removal, not overlay; confirmation/L3 |
| 30 | Compare PDF `FEAT-PDF-071/072` | Low | Compare | LOCAL/Local Worker | D | No | Provenance/alignment/resource result |
| 31 | AI Summarizer `FEAT-INTEL-001` | Medium | Intelligence | OPTIONAL approved local/internal | D + exposed W*/P/T* | Yes | Grounding/provenance/minimum selected context |
| 32 | Translate PDF `FEAT-INTEL-002` | Medium | Intelligence | OPTIONAL approved local/internal | D + exposed W*/P/T* | Yes | Language/layout/fidelity/no public endpoint |
| 33 | PDF to Markdown `FEAT-CONV-036` | Low | Extraction | OPTIONAL complex | D + W* | Yes | Deterministic-first heading/table/order/OCR fallback |

Matrix executable harus dihasilkan dari registry dan release `CapabilityVersion`. Tabel ini tidak mengotorisasi exposure baru.

---

# 18. Test Plan Lengkap Merge PDF

## 18.1 Baseline

| Atribut | Kontrak |
|---|---|
| Feature | `FEAT-PDF-005 — Merge PDF` |
| Input/output | Dua atau lebih PDF berurutan → satu PDF |
| Mode | LOCAL; immediate atau background Local Worker |
| Server | Processing upload/API/SignalR = tidak ada |
| State | Correlation ID selalu; Job ID hanya jika background local Job |
| Finalitas | Temp → validation → atomic Save As/result baru |
| Source | Tidak ditimpa/diubah diam-diam |
| Partial success | Tidak valid; hasil Merge atomik |
| Evidence | Personal History aman + Enterprise Audit L2 accepted/terminal |

Requirement utama:

```text
FR-FILE-001..005,010..015
FR-PROC-001..005
FR-STOR-001..006
FR-HIST-001..003
FR-AUDIT-001..004
FR-REC-001..006
NFR-PERF-001..013
NFR-REC-001..005
SR-SEC-003..005,013..018
UIR-UI-005..010
BR-GOV-003,008
```

## 18.2 Alur yang Diuji

```text
Select/Drag Files
→ Fast Gate + Structural/Security Validation
→ Order/Preview
→ Reorder/Remove
→ Local Resolver
→ In-process atau Local Worker
→ Progress/Cancel
→ Validate Result
→ Atomic Save As
→ History + Audit Evidence
```

## 18.3 Fixture Sistem

- release-equivalent Windows Desktop build;
- real SQLite, protected local workspace/temp, Local Worker/IPC;
- approved real PDF engine build dan independent result parser bila tersedia;
- controllable network, quota-limited test volume, dan process termination;
- synthetic PDF corpus dengan page marker/digest/size manifest;
- safe evidence inspector untuk local Job, History, Audit outbox, log, dan server-call counter;
- source/target pre/post digest verifier.

## 18.4 Skenario

| Test ID | Level/area | Skenario | Expected result |
|---|---|---|---|
| `TP-MRG-001` | Functional | File-First A,B valid | Output valid A,B; source utuh; no server call |
| `TP-MRG-002` | Business parity | Tool-First “gabung PDF” | Request dan outcome canonical setara `001` |
| `TP-MRG-003` | Assistant parity | “Gabungkan A lalu B” | Feature/schema/order/auth sama; grounded result; Origin Assistant |
| `TP-MRG-004` | Validation | Zero/one input | Execute disabled/rejected sebelum engine |
| `TP-MRG-005` | Validation/Security | Fake extension, wrong MIME/signature/structure | Safe reject; no engine/Job/final output |
| `TP-MRG-006` | Functional | Reorder A,B,C → C,A,B | UI, plan, engine, result order konsisten |
| `TP-MRG-007` | Functional | Remove B dari A,B,C | Output A,C; B/source tidak berubah |
| `TP-MRG-008` | Functional | Many files + explicit duplicate A | Semua occurrence sesuai ordered list; no silent dedupe |
| `TP-MRG-009` | UX | Drag/drop, picker, reorder/remove, preview, Save As | Natural, no unnecessary dialog, actionable feedback |
| `TP-MRG-010` | Accessibility | Keyboard-only/focus/screen-reader labels/high contrast | Semua aksi utama dapat digunakan dan focus tidak hilang |
| `TP-MRG-011` | Adaptive UI | Resize/DPI/long localized text | No overflow/clipped action; progress/cancel tetap terlihat |
| `TP-MRG-012` | Local-First | Online local-capable | Server processing/upload/API/SignalR counter = 0 |
| `TP-MRG-013` | Offline | Valid offline grant, network putus | Merge tetap lokal; Audit metadata antre; no content sync |
| `TP-MRG-014` | Offline security | Grant expired/tampered/wrong profile | Fail closed sebelum engine; no upload/queue leakage |
| `TP-MRG-015` | Processing | Large file memilih Local Worker | UI responsif; stable local Job; typed IPC; no server |
| `TP-MRG-016` | Performance | Typical/large/boundary file | CPU/RAM/temp/duration/UI metrics tercatat; no unsafe buffer |
| `TP-MRG-017` | Performance | Typical sampai policy-boundary jumlah file | Order/output valid; handle/memory/temp bounded |
| `TP-MRG-018` | Over-limit | Sedikit di atas file/page/size policy | Early actionable reject; resource tidak exhaustion |
| `TP-MRG-019` | Cancellation | Cancel queued | Engine tidak mulai; `CANCELLED`; no final |
| `TP-MRG-020` | Cancellation | Cancel running | Stop safe point; no late completion; temp non-final |
| `TP-MRG-021` | Error | Engine transient/permanent failure | Safe mapped failure; bounded retry hanya transient |
| `TP-MRG-022` | Recovery | Local Worker crash/disconnect | Desktop hidup; reconcile; compatible resume/retry only |
| `TP-MRG-023` | Recovery | App/device restart saat background/staging | Valid state terdeteksi; no false `COMPLETED` |
| `TP-MRG-024` | Storage | Low disk saat preflight | `INSUFFICIENT_STORAGE`; engine/publisher = 0 |
| `TP-MRG-025` | Storage recovery | Disk write/rename gagal saat staging/publish | Existing target aman; no half-final; reconciliation tersedia |
| `TP-MRG-026` | Source safety | Target sama dengan source | Save As menolak; source digest tidak berubah |
| `TP-MRG-027` | Source safety | Existing target tanpa governed overwrite action | Conflict; existing target tidak berubah |
| `TP-MRG-028` | Result | Engine menghasilkan truncated/wrong-page output | Validation gagal; temp tidak dipublikasikan |
| `TP-MRG-029` | Result | Reopen valid result setelah app ditutup | PDF terbuka, page count/order benar, result addressable |
| `TP-MRG-030` | Encryption | Password required → wrong → correct ephemeral | Safe retry; password tidak persist/log/evidence |
| `TP-MRG-031` | File security | Embedded action/active content/polyglot | Tidak dieksekusi; bounded safe handling |
| `TP-MRG-032` | History | Success/failure/cancel lalu History action | Outcome benar; delete History tidak menghapus Audit/source/result |
| `TP-MRG-033` | Audit/Privacy | Accepted/terminal evidence + CanarySet | L2 tepat; no progress noise/content/path/password leakage |
| `TP-MRG-034` | Correlation | Background local lifecycle | Satu Correlation ID; Job ID hanya local background |
| `TP-MRG-035` | Compatibility | Feature/capability flag berubah sebelum execute | Final revalidation; stale UI bukan authority |
| `TP-MRG-036` | Platform | Exposed simple Merge pada P/T atau unavailable state | Jika exposed: behavior equivalent; jika tidak: explain/hide, no bypass |

## 18.5 Oracle Hasil

Result verifier tidak hanya memeriksa file ada. Minimum:

- media/signature/structure valid;
- expected total pages dan ordered markers/provenance;
- tidak ada missing/extra/duplicate selain explicit occurrence;
- source dan existing target digest tetap;
- no temp/partial publication;
- result dapat dibuka ulang;
- History/Audit/Correlation semantics benar;
- semantic property digunakan, bukan byte equality, kecuali contract memang mengharuskannya.

## 18.6 Kriteria Keluar Merge

```text
[ ] 50 Unit case Dokumen 11 applicable PASS
[ ] seluruh TP-MRG-001..036 applicable PASS atau memiliki governed blocker/disposition
[ ] functional, business, UX, security, performance, recovery, platform evidence lengkap
[ ] source/finality/order/atomicity lulus pada success/failure/cancel/crash
[ ] Local-First dibuktikan dengan zero processing API/upload/SignalR
[ ] no sensitive canary pada seluruh evidence
[ ] measured baseline tersedia untuk representative Merge workloads
```

---

# 19. Pengujian Feature Berbasis Proof-of-Capability

PoC wajib sebelum coding skala penuh untuk capability yang berisiko atau belum terbukti.

| Capability | PoC evidence minimum |
|---|---|
| Existing PDF content editing | Supported object/font/layout, fidelity, unsupported fallback, license/security/resource |
| Office → PDF | DOCX/PPTX/XLSX format/fidelity, macro/active content, font, platform, maintenance |
| PDF → Office | Editability, layout/table/slide fidelity, unsupported structure, resource |
| Complex table OCR | Indonesian/English, table structure, scanned quality, CPU/RAM, timeout/fallback |
| Background removal | Deterministic image path vs ML need, quality, local/provider security/resource |
| HTML/Web → PDF | Script/active content, SSRF/egress, layout, browser engine/license/patching |
| PDF/A | Independent conformance, conversion failure, long-term compatibility |
| Physical scanner | Driver/device matrix, cancel/reconnect, output, security, maintenance |
| Optional local AI | Model size, CPU/RAM, quality, offline capability, license, update/fallback |
| AI Summarizer | Provider approval, grounding, selected context, privacy, quality, capacity |
| Translate PDF | Language/fidelity/layout, provider retention/security, capacity/fallback |
| PDF → Markdown | Heading/list/table/order, deterministic extraction, OCR fallback, fidelity |

Setiap PoC menghasilkan salah satu status:

```text
APPROVED_ENGINE
APPROVED_WITH_LIMITATION
BLOCKED_ALTERNATIVE_REQUIRED
DEFERRED_BY_GOVERNED_DECISION
```

Generic adapter test tidak boleh dianggap fidelity PASS. Setiap PoC mencatat implementation/version, corpus, format support, license, security, platform, CPU/RAM/temp, large file, concurrency, maintainability, fallback, limitation, dan owner.

---

# 20. Pengujian Fungsional

Functional Test membuktikan public behavior satu capability dengan component/host nyata terkontrol.

Oracle canonical:

```text
FeatureId + ParameterSchemaVersion + ordered inputs + parameters
+ principal + ownership/scope + policy/capability/config revision
+ processing decision + confirmation/consent
→ accepted/rejected + safe error
+ Job state bila background
+ validated result
+ source/target integrity
+ History/Audit evidence
+ Correlation continuity
```

Setiap Feature yang dirilis minimal menguji:

- primary user outcome;
- input below/equal/above bound;
- invalid/corrupted/unsupported input;
- permission/policy/capability/compatibility denial;
- cancel dan failure bila long-running;
- correct mode dan forbidden side effect;
- result validation, reopen, and source protection;
- History/Audit/Privacy yang berlaku.

---

# 21. Pengujian Proses Bisnis

## 21.1 Triple Entry Parity

| Aspek | File-First | Tool-First | Assistant-First | Aturan |
|---|---|---|---|---|
| Discovery | File/action recommendation | Search/Tool Card | Intent/alias | Feature ID akhir sama |
| Input | Sudah dipilih | Dipilih setelah tool | Resource user | Ownership/readiness/order setara |
| Parameter | Preview/editor | Form | Typed plan | Normalized meaning setara |
| Authorization | Application service | Application service | Per-step service | Tidak ada elevasi |
| Processing | Resolver | Resolver | Resolver per step | User/model tidak memilih engine |
| Confirmation | Actual action | Sama | Plan preview + same rule | Tidak boleh diinfer Assistant |
| Result | Validated | Validated | Grounded validated | Outcome/error/state setara |
| Evidence | User + UI origin | User + UI origin | User + Assistant origin | Actual action sama |

Business scenarios minimum:

- primary flow end to end;
- alternate valid flow;
- validation/business rejection;
- cancellation/retry/recovery;
- stale permission/flag/capability/policy before final action;
- local-capable offline;
- server-required offline;
- high-risk confirmation/reauth;
- Assistant multi-step allowed/denied/mixed-mode;
- History, secured transfer, Issue/Support, Admin, dan Audit sebagai plane terpisah.

Technical output yang valid tetapi tidak memenuhi business outcome adalah FAIL.

---

# 22. Pengujian UI/UX

| Area | Test minimum |
|---|---|
| Learnability | Label/action mudah dipahami; File/Tool/Assistant entry dapat ditemukan |
| Efficiency | Tidak ada dialog/langkah berulang yang tidak perlu; defaults aman |
| Feedback | Validation/progress/wait/cancel/success/error/recovery jelas; no silent wait |
| Source/finality | Preview dan working state dibedakan dari final result; overwrite jelas |
| Adaptive layout | Window/viewport/orientation/zoom/DPI/keyboard-open; no overflow/clipping |
| Platform-natural | Mouse/keyboard/drag-drop, browser navigation, touch/stylus/camera/share sesuai platform |
| Unavailable action | Hide/explain/fallback berdasarkan capability tanpa dead control |
| Accessibility dasar | Keyboard-only, focus order/restore/indicator, labels, contrast, zoom/reflow, touch target |
| Localization | Bahasa Indonesia utama, long text, date/number/timezone/culture tanpa merusak rule |
| Security UX | Confirmation/reauth/consent/privacy notice jelas dan tidak dapat dilewati |

Automation boleh membuktikan layout state/navigation/semantic controls. Manual/real-device review tetap diperlukan untuk usability, fidelity, scanner/camera/signature, assistive technology, serta platform lifecycle yang tidak dapat dipercaya dari emulator saja.

---

# 23. Pengujian Integrasi

| Boundary | Real component minimum | Positive evidence | Negative/recovery evidence |
|---|---|---|---|
| Web ↔ API | ASP.NET Core host + real HTTP middleware | Authenticated discovery/resource/action | 401/403/404, malformed/over-limit, no direct SQL |
| Desktop ↔ Application | Presentation adapter + Application service | Canonical request dan state projection | Disabled/stale/unsupported tidak dispatch |
| Desktop ↔ Local Worker | Real process + typed/versioned IPC | Handshake/command/progress/cancel/result | Version mismatch, forged message, crash/flood/stale event |
| API ↔ SQL | EF Core + SQL Server test database | Mapping/constraint/transaction/outbox | Deadlock, unique conflict, rowversion, rollback |
| API ↔ Storage | Streaming + approved test store | Stage/digest/READY/range/download | Partial/wrong digest/orphan/expiry/ownership |
| API ↔ Server Worker | Persisted Job/dispatch/lease/worker | 202/claim/progress/result | Crash/lease expiry/duplicate/stale completion |
| API ↔ SignalR | Persisted state/outbox + Hub | Persist-before-notify, sequence, reconcile | Duplicate/gap/reorder/cross-user/slow consumer |
| Processing ↔ Engine | Resolver + approved adapter/build | Typed mapping/cancel/validated result | Unsupported/corrupt/timeout/crash/wrong output |
| Assistant ↔ Registry | Intent/plan + registry + service | Canonical plan and grounded result | Invented/disabled Feature, schema/auth/provider bypass |
| Audit ↔ Business Action | Owner outbox + ingestion | Actor/origin/action/outcome IDs tepat | Duplicate/conflict/offline retry/canary exclusion |

Aturan:

- satu boundary mempunyai owner dan isolated fixture lifecycle;
- timeout bukan synchronization utama;
- cleanup idempotent dan tidak menutupi failure;
- SQL, SQLite, filesystem, HTTP, IPC, SignalR yang diklaim harus nyata;
- engine fake hanya membuktikan orchestration, bukan fidelity/security output.

---

# 24. Pengujian Kontrak

Contract gate mengikuti Governed Hybrid OpenAPI:

```text
typed/versioned C# contract
→ generated OpenAPI per API major
→ deterministic normalization/snapshot
→ breaking-change diff
→ serialization/route/validation/examples
→ handler/authz/completeness
→ supported-client compatibility
```

| Suite | Assertion |
|---|---|
| OpenAPI structure/diff | Route/method/operationId/schema/status/media/security/nullability stable |
| Feature Contract Index | Server-executable Feature mempunyai schema/handler/authz; local-only tidak menjadi operation |
| Serialization | JSON casing/nullability/enum/discriminator/date/number/unknown-field policy |
| Validation/Error | Syntactic/semantic dipisah; safe typed error dengan Correlation ID |
| FileRef/Job | Opaque typed refs, 202, state/version/result/cancel/retry/resume |
| SignalR | Envelope/catalog/version/sequence/forbidden fields/REST recovery |
| IPC | Handshake/message/version/operation/error fail closed |
| Assistant/Workflow | Same schema per step, DAG/media/authorization/capability/finality |
| Compatibility | Current/minimum/supported/unsupported client/contract/capability versions |
| Forbidden | No secret/content/path/storage key/provider detail in examples/errors/events |

Breaking route, required field, type/meaning, enum, status, media type, atau error semantics memerlukan version/compatibility path resmi. Contract test tidak membuat API major atau error code baru.

---

# 25. Pengujian API

API Integration memakai hosted ASP.NET Core dengan HTTP dan middleware nyata; direct controller call bukan bukti API Integration.

| Status | Kasus minimum |
|---:|---|
| `200` | Read atau immediate server result valid |
| `201` | Upload/resource created + authorized `Location` |
| `202` | Durable Job/validation/export + Job ID/Location/Correlation |
| `204` | Chunk/abort/idempotent action tanpa body |
| `400` | Malformed JSON/header/query/range/envelope |
| `401` | Missing/invalid/expired/revoked credential/reauth |
| `403/404` | Permission/ownership deny atau consistent concealment |
| `409` | Idempotency/state/offset/checkpoint/storage conflict |
| `412` | Stale ETag/version precondition |
| `413/415/416` | Hard size/media/range rejection |
| `422` | Semantic/file/business validation |
| `429` | Caller rate/quota/concurrency + `Retry-After` |
| `500` | Safe unexpected error |
| `503` | Shared capacity/dependency unsafe before acceptance |

API suite mencakup:

- Correlation, Feature, Job, public resource, cursor, ETag, fileRef, and Idempotency identifiers;
- same key/same fingerprint replay; same key/different fingerprint conflict;
- authorization before count/storage/range/export/content resolution;
- one-shot/resumable upload, offset/digest/retry/expiry/abort;
- result HEAD/GET/Range/ETag/expiry/not-ready;
- Job cancel/retry/resume/concurrent state;
- SignalR reconnect + REST reconcile tanpa command replay;
- schema/injection/CSRF/CORS/Origin/Host/proxy sesuai topology;
- rate/quota/backpressure distinction;
- local-only direct attempt: safe reject + zero Job/upload/engine sampai exact contract resmi tersedia.

---

# 26. Pengujian Database dan Storage

## 26.1 SQL Server dan EF Core

- real SQL Server test database, bukan in-memory substitute;
- schema/table/column/index/check/FK/delete/rowversion mapping;
- module ownership dan least privilege;
- atomic Job admission, idempotency, attempt, reservation, outbox;
- concurrent claim/lease/completion dan stale state rejection;
- authorization-first keyset query, allowed filters/sorts, cancellation/timeout;
- deadlock/transient retry hanya pada idempotent command;
- clean/prior-version migration, expand-contract, forward-fix/rollback evidence;
- backup/restore validation dan SQL-object reconciliation.

## 26.2 SQLite dan Local Filesystem

- store per OS profile + Pertamina subject;
- account switch menutup store lama dan tidak merge queue;
- single writer dan operation cursor;
- staging → flush/close → validate/digest → atomic promote → SQLite pointer;
- crash/migration/corruption recovery;
- selective sync allowlist dan no content/password/signature/OCR in ordinary rows;
- source/target conflict, traversal/UNC/device/ADS/reparse/symlink/TOCTOU;
- cleanup tidak menghapus active/leased/referenced artifact.

## 26.3 SQL dan Object Storage

Fault injection sebelum/sesudah stage, validation, metadata intent, promotion, availability, cleanup, dan reconciliation harus membuktikan:

- no long distributed transaction;
- metadata tidak `AVAILABLE` sebelum object valid/final;
- orphan metadata/object terdeteksi dan direkonsiliasi idempotently;
- cross-user opaque ref tidak memberi akses;
- secured maximum-three active/addressable version tetap benar pada race;
- tombstone/hold/retention mencegah premature delete.

---

# 27. Pengujian Worker dan IPC

## 27.1 Local Worker

```text
compatible handshake/version
typed closed command
server-derived process/user/job binding
heartbeat/progress sequence
cancellation/deadline
crash/disconnect/restart/recycle
checkpoint compatibility
malformed/oversized/flood/rogue client
parallel workspace isolation
resource containment and cleanup
```

Local Worker tidak menerima arbitrary shell/SQL/path/URL/provider/token/network request. Real Windows security PoC membuktikan Named Pipe ACL, current-user/package/process identity, remote/rogue rejection, child-process/resource containment.

## 27.2 Server Worker

- durable dispatch dari persisted Job, bukan HTTP/SignalR connection;
- one active attempt lease di bawah concurrent claim;
- stale worker progress/completion ditolak;
- bounded transient retry dan compatible resume;
- result validation mendahului publication/`COMPLETED`;
- capacity unavailable menjadi safe reject/wait state;
- policy/auth/capability revalidation yang diwajibkan;
- least-privilege workload identity tanpa end-user token;
- actual User Actor dan Worker Executor pada Audit.

---

# 28. Pengujian End-to-End

E2E dipilih karena business value, risk, boundary count, family, mode, platform, PoC uncertainty, atau historical defect. Minimum release portfolio:

1. Merge PDF File-First lengkap.
2. Merge parity Tool-First dan Assistant-First.
3. Satu local background path melalui Local Worker.
4. Satu optional-server upload → Job → SignalR/REST → result.
5. Satu Office-conversion PoC representative.
6. OCR representative.
7. High-risk Unlock/Protect/Redact path yang dirilis.
8. Visual Sign pada platform exposed, tetap non-PKI.
9. Explicit secured-file transfer/version/restore.
10. Auditor scoped query/export dan Support issue flow tanpa content elevation.
11. Assistant multi-step dengan allowed/denied/offline combination.
12. Supported-old/update-required client dan reconnect compatibility.

E2E mengamati public UI/API/resource outcome. Internal DB/log hanya supporting evidence. Fixed sleep, byte-only existence check, dan test-only authorization bypass dilarang.

---

# 29. Pengujian Keamanan

Security Test memakai defense-in-depth dan negative oracle: denial harus terjadi sebelum content/storage/provider/side effect.

| Area | Skenario minimum |
|---|---|
| Authentication | Missing/invalid/expired/revoked, logout, disabled, refresh replay, PKCE/state/nonce, reauth, offline grant |
| Authorization | Wrong permission/owner/role/profile/scope/state; list/count/cursor/range/export/subscribe; direct API |
| File | Malformed/polyglot/bomb, MIME/signature, active content, path/TOCTOU, encrypted/password, output validation |
| API/Web | Closed schema, SQL/command/path/CRLF/XSS/CSV injection, CSRF/CORS/CSP/Origin/proxy, request ambiguity |
| Storage/Download | Cross-user ref, expiry/range/race, no size/name/existence leak |
| Worker/IPC | Handshake/identity/ACL/job binding/allowlist/flood/crash/containment |
| SignalR | Auth/group/ownership/revocation/payload/duplicate/gap/flood/reconnect |
| Assistant/AI | Direct/indirect prompt injection, invented tool, invalid plan, auth bypass, exfiltration, malicious output, false success |
| Secrets/Supply chain | Secret scan, dependency/license/vulnerability, SBOM/artifact/config integrity |
| Abuse/DoS | Bounded upload/query/queue/provider/slow consumer/malicious corpus |

Semua active security tests dilakukan pada authorized non-Production environment dengan scope, safety ceiling, stop condition, monitoring, recovery owner, cleanup, serta synthetic accounts/data.

VA/PT readiness mencakup Web/API/Identity/AuthZ/file/storage/Job/SignalR/secured/support/admin/audit, local storage/IPC/Worker, document/HTML/OCR engine, Assistant/provider, deployment/config/secret/observability. VA/PT wajib sebelum Production dan setelah material attack-surface change sesuai otoritas Pertamina.

---

# 30. Pengujian Privasi

Synthetic CanarySet mewakili password, token/cookie, secret/key, document bytes/text, OCR/translation/Markdown, signature, clipboard, prompt/provider, Issue attachment/message, raw path/storage URL/key, SQL/stack/claim dump.

Canary harus tidak muncul pada:

```text
API response/error
structured log/exception
trace/metric/health/alert
Audit/outbox/event/export
SignalR/IPC/queue
idempotency/cache/checkpoint/history
SQLite/SQL ordinary metadata
Issue diagnostics dan screenshot yang tidak dikirim eksplisit
provider capture
filesystem/temp/crash dump/backup test
test report, CI log, screenshot/video, coverage/performance/security evidence
```

Closed typed allowlist adalah kontrol utama. Pattern sanitizer hanya defense-in-depth. Sanitizer/mapper failure harus fail closed. Evidence yang memang membutuhkan restricted content mengikuti controlled handling terpisah dan tidak disalin ke sistem test biasa.

---

# 31. Pengujian Kinerja dan Anggaran Server

## 31.1 Lifecycle Nilai

| Status nilai | Arti |
|---|---|
| `TARGET_TO_BE_BASELINED` | Belum ada pengukuran valid; metode/workload harus disiapkan |
| `MEASURED_BASELINE` | Distribusi aktual dari build/environment/corpus/workload yang disebut lengkap |
| `TARGET_TO_BE_APPROVED` | Ambang usulan dari baseline, UX, risiko, kapasitas, biaya, dan headroom |
| `POLICY_CONFIGURABLE` | Batas runtime yang telah disetujui dan terversi; hard safety tetap tidak dapat dilonggarkan |

Tanpa target resmi, invarian nonnumerik tetap PASS/FAIL. Gate numerik menjadi `BLOCKED_PENDING_TARGET_APPROVAL`, bukan PASS.

## 31.2 Metrik Wajib

| Area | Metrik |
|---|---|
| CPU | Host/process utilization, saturation duration, CPU time per operation |
| RAM/.NET | Working/private/committed, GC heap/allocation/LOH/pause, thread/handle |
| Disk/temp | IOPS/bytes/latency/queue/free space, peak/job, orphan/growth/cleanup |
| Network | Bytes/job, throughput, retry/resume/reconnect efficiency |
| SQL | Reads/writes/CPU/duration/log/tempdb, waits/locks/deadlocks/plans |
| DB pool | Active/idle/checkout wait/timeout/exhaustion/recovery |
| API | Rate, concurrency, p50/p95/p99/max bila sample memadai, status/error |
| Worker | Peak/residual memory, job duration, recycle/crash, concurrency |
| Queue/Job | Depth/oldest age/arrival/service/drain/fairness, stage/end-to-end duration |
| SignalR | Connections, buffer, publish/delivery/reconnect/gap/REST reconcile |
| Audit/logging | Intent/transaction delta, rows/bytes/action, backlog/drain, APM overhead/cardinality |
| Storage growth | SQL/object/temp/result/checkpoint/export/outbox/tombstone growth |
| UI | Input latency, main-thread stall, progress delay, post-crash recovery |

## 31.3 Run Manifest dan Metode

Setiap run mencatat build, environment/topology, route, Feature/workload, corpus, load model, state cold/warm/grown, duration/window, policy, instrumentation, serta safe identifiers.

Urutan:

```text
define question/invariant
→ freeze build/config/environment/corpus
→ validate generator and instrumentation
→ control run
→ warm-up/precondition
→ repeated measurement
→ correctness/security checks during load
→ stop new load and measure drain/recovery
→ compare compatible baseline
→ propose target/policy through approval
```

Tidak boleh memilih run terbaik, menggabungkan cold/warm tanpa label, menghilangkan outlier, menonaktifkan Audit/security, atau mengubah corpus agar hasil hijau.

## 31.4 Workload Lokal

Corpus bands:

```text
CONTROL
TYPICAL
LARGE_SUPPORTED
POLICY_BOUNDARY
OVER_LIMIT
ADVERSARIAL_VALID
```

Skenario: PDF besar, banyak PDF/Merge, image besar, OCR, conversion, edit/working state, multi-step cumulative temp, concurrent local jobs, local Audit outbox, Worker recycle/crash. Ukur responsiveness, CPU/RAM/GC/handle, disk/temp, duration, queue, checkpoint, cancellation, cleanup, source digest, dan zero server-processing traffic.

## 31.5 Workload Server

Skenario: metadata/discovery/auth support, streamed/resumable upload, range/full download, server Jobs, metadata queries, Issue, Audit query/export/backlog, secured version race, SignalR, cleanup, dan approved internal provider.

Workload mix berasal dari inventory/pilot/telemetry aman dan execution profiles, bukan persentase rekaan. Local-only workflow saat online menjadi negative server-load case: server processing/upload count harus nol.

Small-server/pilot test mengukur per-process serta host total, menemukan bottleneck pertama, headroom, queue/backpressure behavior, dan scale/throttle trigger. Kapasitas terbatas tidak membenarkan penghapusan Feature.

---

# 32. Pengujian Load dan Concurrency

Tahap load:

```text
Control
→ Measured Baseline
→ Pilot/Target Candidate
→ Headroom Exploration
→ Mixed Workload
→ Burst and Drain
→ Recovery Observation
```

Load profiles: metadata-heavy, transfer-heavy, compute-heavy, connection-heavy, write-heavy, read/export-heavy, recovery-heavy, dan pilot mixed.

Concurrency races minimum:

- same/different idempotency fingerprint;
- multi-worker claim dan expired lease;
- cancel vs complete/fail/checkpoint;
- retry vs resume;
- chunk duplicate/overlap/order;
- parallel range vs expiry/revocation;
- fourth secured version;
- policy/config rowversion;
- Audit replay;
- SignalR duplicate/reorder/gap;
- DB pool contention;
- local job vs autosave/checkpoint/UI;
- reconnect backlog herd.

Barrier terkontrol harus memastikan race window benar-benar terjadi. Banyak request tanpa injection point bukan bukti concurrency correctness.

Load evidence mencatat offered/accepted/completed/rejected/cancelled/failed/retry/duplicate, latency per stage, resource distribution/trend, queue/fairness/drain, output/source/idempotency/Audit correctness, serta generator health.

---

# 33. Pengujian Stress

Stress hanya pada isolated non-Production environment. Naikkan satu dimensi per eksperimen:

```text
metadata request rate
expensive concurrent Jobs
upload streams/temp
download/range
SignalR connections/progress burst
DB connections/queries
Audit/offline backlog
storage/temp pressure
internal provider slow/busy
cleanup/export with foreground
retry/idempotency storm
mixed small-host contention
```

Stop jika host/storage mendekati safety limit, data/authorization/source safety terancam, health tidak dapat pulih, telemetry hilang, atau backpressure gagal.

Stress PASS kualitatif berarti overload menghasilkan wait/reject eksplisit, accepted work tetap reconcilable, no duplicate/corrupt/cross-user effect, no terminal regression/partial final, backlog/resource pulih, dan local capability tetap tersedia. Breaking point test bukan Production policy limit.

---

# 34. Pengujian Reliability dan Soak

Soak mencakup beberapa siklus yang relevan: Job/checkpoint/lease, refresh/reconnect, upload/result expiry, outbox, retry/backoff, cleanup, log lifecycle, Worker recycle, dan archive/purge jika aktif. Durasi tepat tetap `TARGET_TO_BE_APPROVED`.

Drift yang dicari:

| Area | Failure signature |
|---|---|
| Memory/handle/thread | Residual naik per siklus setelah load setara selesai |
| DB connection | Checkout wait/active count tidak kembali |
| Queue/backlog | Depth/oldest age terus naik ketika arrival ≤ service rate |
| Temp/object | Orphan/growth tanpa cleanup atau active lease |
| SQL/log/index | Growth/plan/write amplification tidak sesuai volume |
| Worker | Peak/residual/crash/retry memburuk antarsiklus |
| SignalR | Buffer/subscription/reconnect/polling bocor |
| Audit/logging | Backlog/duplicate/cardinality/volume tidak bounded |
| UI | Responsiveness memburuk setelah repeated job/recovery |

Setelah load berhenti, queue drain, active resources kembali ke approved baseline band, eligible cleanup selesai, dan tidak ada stranded nonterminal Job. Tren residual positif persisten tidak boleh ditutupi rata-rata.

---

# 35. Pengujian Recovery dan Fault Injection

## 35.1 Recovery Matrix

| Test ID | Failure | Expected behavior/evidence |
|---|---|---|
| `REC-SYS-001` | SignalR disconnect | Job lanjut; reconnect+jitter; REST reconcile; no replay |
| `REC-SYS-002` | Upload network disconnect | Resume committed offset/digest; no corruption |
| `REC-SYS-003` | Commit terjadi, response hilang | Same idempotency key returns same resource/effect |
| `REC-SYS-004` | API restart before/after acceptance | No false 202; accepted Job remains discoverable |
| `REC-SYS-005` | Server Worker crash/lease expiry | Safe reclaim; one active attempt; stale completion rejected |
| `REC-SYS-006` | Local Worker crash | UI alive; source safe; reconcile/resume only if compatible |
| `REC-SYS-007` | App/device restart/sleep | SQLite/artifact/checkpoint consistent; no stale grant extension |
| `REC-SYS-008` | SQL unavailable | No required mutation without durable state/Audit intent |
| `REC-SYS-009` | Object storage unavailable | No false final result; orphan/reconcile safe |
| `REC-SYS-010` | Dedicated test volume full | Safe preflight/failure; source/target safe; cleanup |
| `REC-SYS-011` | Publish rename/finalization failure | Existing target safe; staged non-final; retry/reconcile |
| `REC-SYS-012` | Provider timeout/busy | Bounded retry/circuit/backpressure; no public fallback |
| `REC-SYS-013` | Engine transient/permanent/security failure | Retry only allowlisted transient; safe permanent failure |
| `REC-SYS-014` | Crash after checkpoint | Resume only with matching feature/schema/input/engine/stage/digest/expiry |
| `REC-SYS-015` | Corrupt/incompatible/expired checkpoint | Safe conflict/expired behavior; no silent resume |
| `REC-SYS-016` | Cancel at safe/non-cancellable point | Cooperative cancel atau governed conflict; terminal stable |
| `REC-SYS-017` | Offline Audit backlog reconnect | Reauth then bounded idempotent sync; occurred/received preserved |
| `REC-SYS-018` | Audit commit, acknowledgement lost | Replay stores once |
| `REC-SYS-019` | Cleanup/archive restart | Resume checkpoint; verify hold/lease/reference before delete |
| `REC-SYS-020` | Backup/restore | Schema/grants/outbox/idempotency/object/digest/append validity |

## 35.2 Injection Points

```text
before durable write
during transaction before commit
after commit before acknowledgement
after external staging before metadata finalization
after metadata finalization before promotion/response
after checkpoint before notification
when lease expires and old Worker returns
after external delete before metadata finalize
```

## 35.3 Safety Guard

- non-Production dan synthetic data saja;
- resolved explicit target, blast radius, owner, timeout, stop condition;
- disk-full memakai quota-limited test volume, bukan system drive/workspace root;
- backup/recovery point dan cleanup/reconciliation runbook tersedia;
- monitoring tetap aktif;
- credentials/content tidak disimpan dalam script/output/dump;
- injected exception bukan PASS sampai state/data invariants diverifikasi.

RPO/RTO dilaporkan sebagai measured baseline dan target proposal sampai disetujui; bukan SLA.

---

# 36. Pengujian Offline

| Skenario | Expected result |
|---|---|
| Valid protected offline grant + local eligible Feature | Local execution berjalan; server processing = 0 |
| Grant expired/tampered/clock rollback/wrong profile | Fail closed atau `ONLINE_REQUIRED` |
| Server-required Feature offline | Actionable online-required/wait; no auto-upload |
| Cached privileged/Admin/Support/Audit action | Ditolak; cached state bukan enterprise authority |
| Draft/cache/temp/checkpoint offline | Tetap lokal; tidak auto-sync |
| App/account switch | Queue/profile lama tidak digunakan identity baru |
| Reconnect | Refresh auth/account/permission/policy/capability/compatibility sebelum dispatch |
| Network flapping | No duplicate command/upload/job; bounded retry/backoff |

Offline test harus membedakan local business capability, local History, protected Audit outbox, dan server resource. Tidak ada full workspace sync terselubung.

---

# 37. Pengujian Sinkronisasi Selektif

Yang boleh disinkronkan hanya berdasarkan contract:

```text
explicit execution transfer bila server path sah
explicit “Amankan ke Akun” secured transfer/version
allowlisted preference/configuration
safe Audit metadata outbox
approved issue submission/attachment action
```

Test wajib:

- explicit action/consent dan ownership;
- no generic draft/cache/temp/thumbnail/undo/checkpoint/content sync;
- account/profile isolation;
- same item replay idempotent;
- conflict/version/maximum-three secured versions;
- reconnect reauthorization dan gradual backlog release;
- partial transfer/ack loss/resume;
- no password/token/signature/OCR/prompt in sync payload;
- delete History tidak menghapus Enterprise Audit/source/result;
- server state/REST menjadi authority setelah reconciliation.

---

# 38. Pengujian Lintas Platform

| Area | Windows Desktop | Responsive Web | Phone | Tablet |
|---|---|---|---|---|
| Postur | Full primary productivity client | Account/history/admin/audit/support/browser-safe tools | Curated quick/practical | Curated richer companion |
| Layout | Window/DPI/multi-panel/keyboard | Breakpoint/zoom/reflow/browser nav | Portrait/landscape/safe-area/keyboard | Orientation/split/touch/stylus |
| Input | Picker/drag-drop/mouse/shortcut | Picker/drop bila allowed | Picker/camera/share | Picker/camera/share/stylus/keyboard |
| Execution | Local/Local Worker; server only resolver | Server/browser-safe official exposure | Curated capability route | Curated capability route |
| Offline | Eligible local only | Hanya resmi didukung | Eligible curated local | Eligible curated local |
| Lifecycle | Close/restart/sleep | Refresh/multi-tab | Background/resume/share | Resume/orientation/multi-window |
| Accessibility | Keyboard/focus/screen reader/high contrast/DPI | Semantic nav/zoom/focus | Screen reader/dynamic text/touch | Screen reader/dynamic text/touch/stylus |

Assertions:

1. exposed Feature mempunyai validation/permission/business outcome equivalent;
2. hidden Feature tidak dapat bypass lewat deep link/route/Assistant/direct API;
3. unavailable action dijelaskan atau disembunyikan sesuai resolver;
4. resize/orientation/zoom/keyboard/long text tidak menutupi confirmation/cancel/privacy;
5. input adapter tetap melewati file/security validation yang sama;
6. sleep/resume/network switch tidak membuat duplicate upload/execution;
7. real device diperlukan untuk camera/share/picker/credential store/lifecycle/accessibility claims.

---

# 39. Pengujian Compatibility dan Versioning

| Status | Expected behavior |
|---|---|
| `SUPPORTED` | Operasi normal sesuai exposure/capability/policy |
| `UPDATE_AVAILABLE` | Compatible operation tetap berjalan; notice non-blocking |
| `UPDATE_REQUIRED` | Incompatible/security-sensitive server action diblok aman; local data tidak rusak |
| `UNSUPPORTED` | Unknown/removed combination fail closed tanpa crash/data loss |

Matriks minimum:

- server current vs current/oldest-supported/below-minimum/newer client bila berlaku;
- API major, ContractVersion, Feature parameter schema, workflow/config schema;
- CapabilityVersion stale/changed/provider disabled;
- SignalR eventVersion dan reconnect after deploy;
- IPC/Worker/engine contract;
- Audit EventVersion;
- checkpoint/result/cursor/idempotency version;
- existing secured/history/job/result dari supported prior version;
- long-running Job dan mixed-version rollout/rollback.

Forced update hanya sah bila fixture menyatakan kebutuhan security/compatibility nyata. Exact supported-version window adalah configuration input terkontrol.

---

# 40. Pengujian Regression

## 40.1 Pack

| Pack | Isi minimum |
|---|---|
| Core Smoke | Registry 33/33, representative per family, Merge primary, parity, local Worker, optional server representative, auth denial, source/finality/Audit, migration/readiness |
| Core Regression | Seluruh Smoke + family conformance + distinctive 33 scenarios + triple entry + Local-First/offline/cancel/recovery + boundary suites |
| High-Risk | Unlock/Protect/Sign/Redact, ownership/role, Assistant/AI, secured delete/version/restore, Audit/export, privacy canary, malicious corpus |
| API Contract | OpenAPI diff, operation/schema/auth/status, server Feature completeness, local-only absence, clients/version, errors/file/Job/SignalR/IPC/workflow |
| Historical Defect | Semua repeatable fixed defects pada layer terendah yang benar |

## 40.2 Trigger

| Perubahan | Regression minimum |
|---|---|
| Feature/domain/application | Affected Unit + family functional + Core Smoke + feature regression |
| DTO/route/schema/serializer | Contract + API Integration + supported clients |
| EF/migration/query | DB Integration + migration + affected API/E2E |
| Worker/IPC/engine | Worker Integration + fidelity + crash/recovery |
| Auth/permission/policy | High-Risk + direct API + entry/Assistant parity + Hub/Worker negatives |
| Storage/file lifecycle | Storage + source/finality + recovery/restore |
| Registry/capability/exposure | Registry 550/33 + platform discovery/selection |
| Defect fix | Exact permanent regression + affected/shared-risk pack |

Test lambat tidak boleh dihapus; optimalkan, pindahkan ke appropriate lane, atau lakukan governed disposition dengan risiko terlihat.

---

# 41. Pengujian Smoke

## 41.1 Deployment Smoke QA/UAT

```text
artifact digest/version correct
configuration/schema/migration compatibility
app/API/Worker startup and readiness
Pertamina test identity login
Feature Registry and 33 Core mapping load
Merge simple local happy path
one protected authorization denial
server resource metadata query
Job/SignalR/REST representative if server scope exists
safe History/Audit evidence
no secret/content canary
```

## 41.2 Production Smoke

Production hanya menjalankan bounded, read-only atau approved synthetic canary sesuai operation policy. Tidak menggunakan user content, malicious corpus, load, fuzz, penetration, debug, atau direct data mutation.

Smoke PASS tidak menggantikan Regression, Security, Performance, Recovery, UAT, atau Production approval.

---

# 42. Release Validation

| Gate | Evidence minimum | Blocker |
|---|---|---|
| BUILD | Controlled restore, Release build/analyzers, approved dependencies | Failure/unapproved source/package |
| UNIT | MTP discovery dan applicable P0/P1/affected/full fast suite | Fail/flake/zero/ignored/weakened |
| INTEGRATION | Architecture/Contract/API/DB/Storage/IPC/Engine as applicable | Wiring/transaction/ownership/breaking compatibility |
| SECURITY | Scans, AuthZ/Privacy/Abuse, malicious corpus, DAST/VA-PT applicability | Leakage/bypass/public AI/disallowed finding/missing required evidence |
| REGRESSION | Exact defect + affected/shared/Core/full tier | Required case fail/not run |
| PERFORMANCE | Representative manifest, metrics, approved target/invariants | Invalid baseline, unsafe behavior, failed target |
| RECOVERY/PLATFORM | Required crash/restore/migration/compatibility/device evidence | Data/source/state loss atau unsupported claim |
| UAT READINESS | Immutable RC, all applicable gates, summary, traceability, lane/data/accounts/package | Any mandatory entry unmet |

Setiap gate berstatus `PASS`, `FAIL`, `BLOCKED`, atau `NOT_APPLICABLE` dengan evidence. Exception membutuhkan authority, scope, compensating control, expiry, dan follow-up; tidak disamarkan sebagai PASS.

---

# 43. Strategi Automation

Automate ketika repeatable dan business/security/data critical, regression-prone, frequent, deterministic, broad matrix, atau menjadi merge/release gate.

| Lane | Automation default |
|---|---|
| PR | Restore/build/analyzer, Unit, Architecture, fast Contract, secret/dependency/static, affected regression |
| Main | Full Unit, affected/broader Integration/Contract, SBOM/digest, Core Smoke |
| QA deploy | Deployment/config/migration smoke, API/DB/Storage/Worker connectivity |
| Scheduled | Broader E2E/platform, malicious corpus, recovery, longer regression, bounded load/soak |
| Release candidate | Required full regression/security/performance/compatibility/recovery/UAT readiness |

Automation harus idempotent, isolated, environment-aware, self-cleaning, machine-readable + human-readable, mendeteksi missing/zero/reduced/ignored tests, dan membedakan product vs infrastructure failure.

UI automation menggunakan semantic identifier, observable readiness, dan authoritative state; bukan coordinate/fixed sleep. Snapshot/golden update memerlukan semantic human review.

Tidak ada target persentase automation. Tool/framework/SaaS tambahan memerlukan dependency, security, license, maintenance, dan data-residency approval.

---

# 44. Ruang Lingkup Manual Test

| Area manual/hybrid | Fokus evidence |
|---|---|
| PDF/Office/image fidelity | Clipping, font, layout, order, printability, semantic diff |
| Scanner/camera/signature | Capture/crop/touch/stylus/device lifecycle/accessibility |
| Adaptive UX/usability | Clarity, unnecessary step/dialog, platform-natural interaction |
| Exploratory | Unexpected combinations, interruption, recovery, confusing errors |
| AI/translation/summarization | Grounded usefulness, language/fidelity within approved boundary |
| Rare platform/provider | One-off compatibility where automation value rendah |

Manual charter:

```text
Charter/Test ID
Requirement/Feature IDs
objective/risk/timebox
build/environment/platform/device
Dataset ID/classification
areas/actions/oracles
safe observations/evidence
defect/gap/automation candidate
```

Manual PASS tidak dapat menimpa failed automated security/integrity gate. Stable repeated critical cases ditinjau untuk automation.

---

# 45. Pipeline Test

```text
Controlled Restore
→ Release Build + Analyzer
→ MSTest Unit on MTP
→ Static/Security/Secret/Dependency/Configuration Checks
→ Architecture/Contract/Integration
→ Package + SBOM + Digest + Immutable Artifact
→ Deploy QA
→ Deployment Smoke
→ Functional/E2E/Authorization/Recovery Regression
→ Stage-appropriate Performance/Security
→ Release Candidate Test Summary
→ UAT Readiness Gate
```

Fast PR memberi feedback cepat. Main/QA memperluas boundary evidence. Test berat dijalankan scheduled/milestone/release candidate. Shared auth/registry/storage/Audit/Worker/contract changes memicu suite lebih luas daripada path filter biasa.

Cache tidak boleh melewati restore integrity. Secret diinjeksikan oleh environment identity dan tidak dicetak. Artifact yang sama dipromosikan; tidak rebuild setelah QA evidence.

---

# 46. Pengelolaan Defect

Defect record minimum:

```text
Defect ID/title
build/commit/environment
Requirement/Feature/Test IDs
Dataset/config/platform
expected vs actual
safe reproduction
severity + DEF priority
security/privacy/data/recovery impact
safe evidence
owner/status/target/disposition
fix build + regression Test ID
```

Status lean:

```text
NEW
→ CONFIRMED
→ IN_PROGRESS
→ READY_FOR_RETEST
→ VERIFIED
→ CLOSED
```

`DUPLICATE`, `AS_DESIGNED`, `NOT_REPRODUCIBLE`, `DEFERRED`, dan `ACCEPTED_RISK` adalah disposition, bukan PASS. Security exception mengikuti authority/expiry/compensating-control Dokumen 08.

---

# 47. Severity dan Priority

## 47.1 Severity — Dampak

| Severity | Definisi |
|---|---|
| `BLOCKER` | Required testing/UAT/primary product tidak dapat dilanjutkan; environment/build unusable; no safe workaround |
| `CRITICAL` | Unauthorized access, secret/content leak, source/data corruption, destructive/false success, critical Audit/business/security failure |
| `MAJOR` | Important requirement/Core path materially wrong/unavailable dengan limited workaround |
| `MINOR` | Limited noncritical/cosmetic/text/report issue dengan safe workaround dan no misleading outcome |

## 47.2 Defect Priority — Urutan Perbaikan

| Priority | Arti |
|---|---|
| `DEF-P0` | Investigate/fix segera; blocks current validation/release |
| `DEF-P1` | Harus selesai untuk target release/UAT entry kecuali governed disposition |
| `DEF-P2` | Planned setelah risiko lebih tinggi |
| `DEF-P3` | Low urgency backlog/value review |

Security scanner/VA-PT severity tetap authoritative dan tidak diturunkan agar sesuai jadwal.

---

# 48. Retest

Retest memverifikasi exact defect pada fix build dengan relevant environment, dataset/config, precondition, dan reproduction. Selain expected result, retest memeriksa forbidden side effect seperti no upload, no disclosure, no overwrite, no invalid final result.

```text
Defect ID
fix build/commit
environment/config/Dataset versions
reproduction/Test ID
expected/actual
PASS/FAIL/BLOCKED
safe evidence
reviewer/time
```

Retest gagal mengembalikan defect ke `CONFIRMED` atau `IN_PROGRESS`; tidak membuat duplicate defect.

---

# 49. Regression Setelah Perbaikan

Sebuah defect hanya `VERIFIED` jika:

1. exact retest PASS;
2. permanent automated regression tersedia bila repeatable;
3. affected component/module suite PASS;
4. upstream/downstream contract dan family/shared-risk suite PASS;
5. security/privacy/source/Audit/platform/entry parity dijalankan bila boundary tersentuh;
6. historical defect set yang relevan PASS.

Full regression tanpa exact reproduction tidak membuktikan defect. Exact retest tanpa required regression juga tidak cukup.

---

# 50. Test Evidence

Evidence yang disimpan bila relevan:

- machine-readable result dan concise summary;
- safe log/trace window dengan synthetic/approved Correlation ID;
- sanitized screenshot/video hanya untuk UI/visual need;
- coverage/architecture/contract diff;
- performance/resource/environment report;
- restricted security/scan/VA-PT report;
- dataset/generator/version/digest manifest;
- artifact/SBOM/config/migration/digest;
- defect retest/regression evidence;
- Requirement/Feature/Test/UAT/Release traceability.

Evidence manifest:

```text
Evidence ID/type
build/commit/artifact digest/pipeline run
environment/config/tool version
time/result/count
Requirement/Feature/Test/Defect links
Dataset ID/classification only
artifact location/digest
access owner/classification
retention/expiry/disposal status
known limitation
```

Evidence tidak boleh silently overwritten. Link tidak memuat bearer token/SAS/secret. Large/restricted evidence tidak dikomit ke source repository secara default.

---

# 51. Test Reporting

Setiap QA/release-candidate summary minimum memuat:

| Area | Isi |
|---|---|
| Build | Version, commit, pipeline run, artifact digest |
| Environment | Lane, config/policy/schema, platform matrix |
| Execution | Planned/executed/pass/fail/blocked/not-run/N/A per layer/priority |
| Defect | Open Blocker/Critical, owner, impact, target/disposition |
| Coverage | Requirement and Feature outcome; 33 Core visibility |
| Performance | Workload/environment/metrics/value-status; no unsupported extrapolation |
| Security | Scan/test/VA-PT applicability, findings/exceptions, privacy canary |
| Recovery/Compatibility | Crash/retry/restore/migration/platform result |
| Known risk | Limitation, workaround, affected scope, decision owner |
| UAT readiness | READY / NOT_READY / BLOCKED dan unmet criteria |

PR report ringkas. QA report memuat smoke + selected suites. Release-candidate report lengkap untuk UAT readiness. UAT result tetap milik Dokumen 13.

---

# 52. Metrics

## 52.1 Test Management

```text
planned/executed/pass/fail/blocked/not-run/N/A by layer and priority
requirement outcome coverage
released Feature outcome coverage
33 Core mapped/evidenced/dispositioned
automation/manual distribution by value—not target percentage
first-pass failure and diagnostic-rerun count
flaky/isolated/expired-isolation count
defect open/age/reopen/escape by severity
retest and regression result
gate result and evidence completeness
```

## 52.2 Technical/Operational

Performance metrics mengikuti Bagian 31. Security metrics melaporkan executed scenarios, finding/severity/status, canary leakage count, waiver expiry, dan retest. Recovery metrics melaporkan detection/restart/reconcile/resume/drain duration serta lost/duplicate/corrupt/orphan effect count.

Metric tanpa build/environment/corpus/config/window tidak menjadi baseline. Pass rate tidak boleh menyembunyikan untested critical requirement.

---

# 53. Traceability

```text
Business Process
→ Requirement ID
→ canonical Feature ID/resource
→ business/security rule + 8-layer DoD
→ component/boundary/platform
→ Unit Test ID (Dokumen 11)
→ non-Unit Test ID/type/environment
→ result/evidence/defect
→ UAT scope (Dokumen 13)
→ release gate
```

| Area | Requirement | Feature/resource | Evidence owner |
|---|---|---|---|
| Core business flow | `FR-PROC-001..005`, `UIR-UI-005..010`, `BR-GOV-008..009` | 33 Core | Functional/E2E/Regression |
| File/source/finality | `FR-FILE-001..015`, `FR-STOR-001..006`, `NFR-REC-001..003` | File-consuming Features | File/Storage/Recovery |
| API/Job | `IR-API-001..012`, `FR-REC-001..006` | Server resources/features | Contract/API/Worker |
| Performance/server budget | `NFR-PERF-001..013` | Released workloads | Performance/Load/Stress |
| Security/privacy | `SR-SEC-001..023` | All protected actions | Security/Privacy/VA-PT |
| Data | `DR-DB-001..007` | SQL/SQLite/Storage resources | DB/Storage/Restore |
| DevOps/environment | `IR-DEVOPS-001..004`, `NFR-ENV-001..004` | N/A | Pipeline/Release |
| Audit | `FR-AUDIT-001..004` | 33 Core + privileged actions | Audit Integration/Security |
| Assistant/AI | `FR-ASSIST-001..015`, `SR-SEC-013..021` | Assistant + underlying Features | Functional/Security/Provider PoC |
| Sign/Unlock/Redact | `FR-SIGN-001..003`, `SR-SEC-011..012,022..023` | `FEAT-SIGN-013`, `FEAT-PDF-062/069` | High-Risk/Manual/Security |

Unknown/duplicate Requirement ID, Feature ID, atau Test ID memblokir trace generation. Test tooling tidak boleh membuat mapping sendiri.

---

# 54. Kriteria Penyelesaian Pengujian

Satu Feature/scope dianggap selesai pada Test Plan hanya jika:

- seluruh 8 lapis applicable mempunyai PASS evidence;
- all critical scenarios and negative paths PASS;
- no unresolved Blocker/Critical atau disallowed security finding;
- Unit/Integration/Contract/E2E/Security/Performance/Recovery/Platform evidence sesuai risk;
- exact target numeric yang sudah disetujui terpenuhi;
- target yang belum disetujui tetap BLOCKED, bukan dianggap lulus;
- PoC/limitation/disposition resmi tersedia;
- Regression dan exact defect retest selesai;
- traceability/evidence/report/documentation lengkap;
- UAT entry package siap jika scope menuju UAT.

Selesai pada Dokumen 12 bukan berarti UAT Passed atau Production Approved.

---

# 55. Kesiapan UAT

## 55.1 Entry Criteria

```text
[ ] immutable release candidate/version/digest identified
[ ] same RC deployed by pipeline to controlled UAT lane
[ ] deployment/config/migration/health smoke PASS
[ ] applicable Build/Unit/Integration/Security/Regression gates PASS
[ ] Performance gate PASS where approved and required
[ ] required Recovery/Compatibility/Cross-platform evidence PASS
[ ] no open BLOCKER/CRITICAL
[ ] known MAJOR/MINOR have scope/workaround/risk/owner/disposition
[ ] Must/Core Requirement and Feature traceability current
[ ] planned UAT scope maps to Document 13
[ ] UAT accounts/permissions/data/platforms prepared
[ ] data synthetic/sanitized and approved
[ ] user guide/release-note draft available where needed
[ ] evidence package and support/escalation path ready
[ ] reset/redeploy/defect/retest process understood
```

## 55.2 UAT Entry Package

| Item | Minimum |
|---|---|
| Release candidate | Build/commit/digest/environment/config/schema |
| Test summary | Results by layer/priority + gate outcomes |
| Known issues | Defect/severity/priority/scope/workaround/disposition |
| Traceability | Requirement → Feature → technical tests → UAT scope |
| Platform scope | Included/excluded/limited Feature/platform combinations |
| Risk evidence | Security/performance/recovery/compatibility results/limitations |
| UAT assets | Approved lane/data/accounts/platform/smoke |
| Guidance/support | Draft guide/release notes + issue/retest path |

Dokumen 13 memiliki user actors, detailed business scenarios, steps, expected acceptance, defect handling during UAT, sign-off, dan final outcome.

---

# 56. Risiko dan Input yang Belum Terkunci

| Risiko/input | Status | Dampak/tindakan |
|---|---|---|
| Numeric latency/throughput/concurrency/resource/recovery target | TARGET_TO_BE_BASELINED | Benchmark lalu Pertamina approval |
| Pilot topology/hardware/workload/corpus | CONFIGURATION REQUIRED | Freeze environment manifest sebelum baseline |
| Exact platform/device/browser/version inventory | CONFIGURATION REQUIRED | Release capability matrix dan real-device plan |
| Engine/provider/device builds | PoC REQUIRED | Baseline per approved implementation/version |
| Pertamina Identity test tenant/lifecycle | ENVIRONMENT REQUIRED | No test-only authorization bypass |
| UAT physical topology | CONFIRMATION REQUIRED | Logical isolation tetap wajib |
| Scanner/security/coverage tool and rule pack | APPROVAL REQUIRED | Vendor-neutral baseline; no paid SaaS assumption |
| Exact local-only API rejection/error code | CONTRACT PENDING | Safe reject + zero side effect until addendum |
| Audit batch/field/API/database addenda | CONTROLLED ADDENDUM | Freeze contract tests setelah addendum |
| Evidence/data retention/access/legal hold | POLICY REQUIRED | Minimize/restrict until confirmed |
| RPO/RTO and restore objective | TARGET_TO_BE_BASELINED | Record measured recovery; do not call SLA |
| Defect SLA/security severity mapping | POLICY REQUIRED | Keep impact and priority separate |

Tidak satu pun gap mengizinkan penghapusan Feature, penurunan security/acceptance, server-processing untuk local-only, public AI, atau PASS palsu.

---

# 57. Panduan Implementasi untuk Codex

Setiap task coding/test menyebutkan:

```text
Task ID
Requirement IDs
Feature IDs
objective/risk
Allowed Paths
Do-Not-Touch/Non-Scope
locked dependencies/decisions
Acceptance Criteria
Tests Required: level, Test ID/priority, environment, command/gate
expected evidence/output
```

Workflow:

```text
read authoritative sources and current failures
→ verify scope/owner/environment/data/dependency
→ run feasible baseline
→ implement smallest authorized change + tests
→ run affected → module → required gates
→ report discovered/pass/fail/blocked/skipped, commands, evidence, risks
→ human review/merge/readiness decision
```

Codex dilarang menghapus/skip/ignore/quarantine-as-green/retry test penting; menurunkan assertion; mengubah requirement/security rule/threshold/gate agar hijau; menambah package/SaaS/provider tanpa approval; menggunakan Production data/secret; mengedit di luar scope; atau menyatakan `DONE`/UAT Ready/Production Ready hanya dari technical green.

Konflik requirement–test–implementation wajib memakai STOP protocol: sebutkan source, Test ID, observed behavior, impact, smallest options, untouched files, dan decision owner.

---

# 58. Checklist Penerimaan Dokumen 12

```text
[x] 1. Purpose tersedia
[x] 2. Scope dan boundary Dokumen 11/13 jelas
[x] 3. Test objectives tersedia
[x] 4. Test principles dikunci
[x] 5. Entry criteria tersedia
[x] 6. Exit criteria tersedia
[x] 7. Test levels tersedia
[x] 8. Test types tersedia
[x] 9. Test environments tersedia
[x] 10. Test data strategy tersedia
[x] 11. Feature coverage tersedia
[x] 12. Risk-based testing tersedia
[x] 13. 33 Core strategy tersedia
[x] 14. Merge PDF complete Test Plan tersedia
[x] 15. PoC feature testing tersedia
[x] 16. Functional testing tersedia
[x] 17. Business-process testing tersedia
[x] 18. UI/UX testing tersedia
[x] 19. Integration testing tersedia
[x] 20. Contract testing tersedia
[x] 21. API testing tersedia
[x] 22. Database/Storage testing tersedia
[x] 23. Worker/IPC testing tersedia
[x] 24. E2E testing tersedia
[x] 25. Security testing tersedia
[x] 26. Privacy testing tersedia
[x] 27. Performance testing tersedia
[x] 28. Load/Concurrency testing tersedia
[x] 29. Stress testing tersedia
[x] 30. Reliability/Soak testing tersedia
[x] 31. Recovery/Fault Injection testing tersedia
[x] 32. Offline testing tersedia
[x] 33. Selective Sync testing tersedia
[x] 34. Cross-platform testing tersedia
[x] 35. Compatibility/Versioning testing tersedia
[x] 36. Regression testing tersedia
[x] 37. Smoke testing tersedia
[x] 38. Release validation tersedia
[x] 39. Automation strategy tersedia
[x] 40. Manual-test scope tersedia
[x] 41. Defect management tersedia
[x] 42. Severity/Priority tersedia dan tidak ambigu
[x] 43. Retest tersedia
[x] 44. Regression after fix tersedia
[x] 45. Test evidence tersedia
[x] 46. Test reporting tersedia
[x] 47. Metrics tersedia tanpa angka rekaan
[x] 48. Traceability tersedia
[x] 49. Test completion criteria tersedia
[x] 50. UAT readiness/package tersedia
[x] 51. Risks/deferred inputs tersedia
[x] 52. Acceptance checklist lengkap
[x] 8-layer Definition of Done tercakup
[x] 33/33 Core mapped
[x] FEAT-PDF-005 tetap LOCAL dan complete
[x] Security, server budget, recovery, platform, Local-First, dan PoC tercakup
[x] baseline 550 canonical features digunakan
[x] Sign PDF tetap visual non-PKI; public AI tetap dilarang
[x] Documents 01–11 dan specialist reports tidak diubah
[x] Bahasa Indonesia ID-1.0 digunakan
```

---

# 59. Status Finalisasi

```text
Document             : 12_TEST_PLAN
Status               : FINAL v1.0
Decision progress    : 25 / 25 AUTO-LOCKED
Specialist reports   : 4 / 4 INTEGRATED
Required areas       : 52 / 52 COVERED
Definition of Done   : 8 / 8 LAYERS COVERED
Canonical features   : 550
Primary PDF tools    : 33 / 33 MAPPED
First vertical slice : FEAT-PDF-005 — Merge PDF
Merge scenarios      : 36 DOCUMENTED
Material conflict    : NONE
User decision needed : NONE FOR FINALIZATION
Next document        : 13_UAT_DOCUMENT.md
```
