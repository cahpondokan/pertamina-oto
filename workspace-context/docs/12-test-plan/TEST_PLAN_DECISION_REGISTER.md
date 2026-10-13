# REGISTER KEPUTUSAN TEST PLAN
## PERTAMINA OTO — Office Tools Organizer

> **Versi:** 1.0  
> **Tanggal:** 28 Agustus 2026  
> **Status:** FINAL — 25/25 AUTO-LOCKED  
> **Dokumen:** `12_TEST_PLAN.md`  
> **Baseline:** Dokumen 01–11 FINAL; 550 fitur canonical; 33 Primary PDF Tool Cards

---

# 1. Prinsip Penguncian

Keputusan berikut merupakan turunan teknis aman dari Dokumen 01–11 dan laporan Agent A–D. Tidak ada keputusan yang mengubah requirement, Feature ID, platform support, aturan keamanan, technology lock, scope produk, atau SLA Pertamina.

---

# 2. Keputusan

| ID | Keputusan yang dikunci | Dasar | Status |
|---|---|---|---|
| `TP-DEC-001` | Test Plan menggunakan requirement-based, risk-based, dan change-based testing serta wajib membuktikan 8 lapis Definition of Done | Dokumen 01 §24.4; Dokumen 02 §17 | AUTO-LOCKED |
| `TP-DEC-002` | Test pyramid: banyak Unit, cukup Integration/Contract, E2E terpilih, serta Security/Performance/Cross-platform yang targeted | Batas Dokumen 11; efisiensi dan maintainability | AUTO-LOCKED |
| `TP-DEC-003` | Detail Unit Test tetap dimiliki Dokumen 11; Dokumen 12 hanya mengatur gate dan handoff-nya | Mencegah duplikasi dan boundary kabur | AUTO-LOCKED |
| `TP-DEC-004` | Portfolio mencakup Unit, Architecture, Integration, Contract, API, Database, Worker/IPC, Functional, E2E, Security, Performance, Load, Stress, Reliability, Recovery, Compatibility, Cross-platform, Regression, Smoke, dan Release Validation | Handoff Dokumen 11; prompt Dokumen 12 | AUTO-LOCKED |
| `TP-DEC-005` | Test non-Unit memakai namespace ID per suite dan metadata traceability minimum yang konsisten | `BR-GOV-003`; kebutuhan audit evidence | AUTO-LOCKED |
| `TP-DEC-006` | Entry/exit criterion serta status `PASS`, `FAIL`, `BLOCKED`, `NOT_RUN`, `INCONCLUSIVE`, dan `N/A` dibedakan; selain PASS tidak boleh dihitung PASS | Anti-false-green | AUTO-LOCKED |
| `TP-DEC-007` | Environment minimum tetap Development, QA/Test, Production; UAT adalah lane yang terisolasi secara logis sampai topology resmi dikonfirmasi | `NFR-ENV-001..004`; batas Dokumen 13 | AUTO-LOCKED |
| `TP-DEC-008` | Satu immutable artifact dibuat pipeline dan dipromosikan; tidak rebuild atau patch in-place setelah QA evidence | `IR-DEVOPS-001..004`; integrity release | AUTO-LOCKED |
| `TP-DEC-009` | Data test default sintetis; corpus/golden/sanitized data memakai manifest, classification, provenance, akses, dan lifecycle; Production data/secret dilarang secara default | Dokumen 08/10/11 | AUTO-LOCKED |
| `TP-DEC-010` | Automation dipilih berdasarkan nilai, risiko, pengulangan, dan determinisme; manual/hybrid tetap untuk fidelity, usability, perangkat, exploratory, dan semantic quality | Mencegah automation/manual berlebihan | AUTO-LOCKED |
| `TP-DEC-011` | Azure DevOps Pipeline memakai fast PR, broader main/QA, scheduled deep tests, dan full release-candidate gates berdasarkan risiko | `TECH-DEC-013`; `IR-DEVOPS-004` | AUTO-LOCKED |
| `TP-DEC-012` | Coverage 33 Core memakai registry completeness + family suite + processing-mode suite + exposed-platform suite + distinctive scenario + PoC/security gate | Primary Feature Set First; anti-duplikasi | AUTO-LOCKED |
| `TP-DEC-013` | `FEAT-PDF-005 — Merge PDF` menjadi full-system first vertical slice dan tetap LOCAL tanpa processing API/SignalR | Dokumen 03/05/06/11 | AUTO-LOCKED |
| `TP-DEC-014` | PoC-gated Feature tidak boleh dianggap selesai sebelum quality, format, license, security, platform, resource, maintenance, dan fallback memiliki evidence/disposition | `BR-GOV-006..009` | AUTO-LOCKED |
| `TP-DEC-015` | Background tidak berarti server; Local Worker memakai IPC/SQLite/local artifacts, Server Worker memakai durable Job/SQL/storage/REST dan SignalR notification | Dokumen 04/06/07 | AUTO-LOCKED |
| `TP-DEC-016` | Suite Integration yang mengklaim HTTP, SQL, SQLite, filesystem, IPC, engine, atau platform behavior harus memakai boundary nyata terkontrol; fake tidak boleh menjadi bukti palsu | Batas Dokumen 11 | AUTO-LOCKED |
| `TP-DEC-017` | Governed OpenAPI, typed Feature schema, API/error/fileRef/Job/SignalR/IPC/version compatibility menjadi contract gate; local-only Feature tidak menjadi server operation | Dokumen 06 | AUTO-LOCKED |
| `TP-DEC-018` | Security testing bersifat defense-in-depth dan negative-first, memakai authorized non-Production environment, synthetic canary/corpus, serta mencakup VA/PT readiness | Dokumen 08/09/10 | AUTO-LOCKED |
| `TP-DEC-019` | Performance result membedakan `TARGET_TO_BE_BASELINED`, `MEASURED_BASELINE`, `TARGET_TO_BE_APPROVED`, dan `POLICY_CONFIGURABLE`; nilai belum disetujui tidak boleh disebut SLA/PASS numerik | `NFR-PERF-006..013` | AUTO-LOCKED |
| `TP-DEC-020` | Performance/load/stress/soak mengukur CPU, RAM, disk/temp, network, SQL/DB pool, API, queue, Worker, SignalR, Audit/logging, concurrency, growth, cleanup, dan UI responsiveness | Server Resource Budget | AUTO-LOCKED |
| `TP-DEC-021` | Backpressure harus aktif sebelum exhaustion; caller limit, hard payload, state/storage conflict, dan shared saturation dibedakan sesuai governed contract | Dokumen 06; no silent wait | AUTO-LOCKED |
| `TP-DEC-022` | Recovery/fault injection mencakup disconnect, crash/restart, unavailable dependency, full test volume, retry/resume/checkpoint/idempotency, restore, dan reconciliation dengan guard keselamatan | Error & Recovery DoD | AUTO-LOCKED |
| `TP-DEC-023` | Cross-platform test mengikuti Capability Matrix/exposure aktual; platform tidak gagal karena Feature memang tidak tersedia, tetapi hide/explain/fallback/adaptive behavior wajib benar | Adaptive UX dan capability-aware execution | AUTO-LOCKED |
| `TP-DEC-024` | Defect severity (`BLOCKER/CRITICAL/MAJOR/MINOR`) dipisahkan dari repair priority (`DEF-P0..DEF-P3`); exact retest dan required regression wajib sebelum `VERIFIED` | Test management integrity | AUTO-LOCKED |
| `TP-DEC-025` | Evidence/reporting harus aman, immutable-linked, traceable, dan tidak menyamarkan failure; UAT readiness hanya handoff ke Dokumen 13, bukan UAT pass/Production approval | `BR-GOV-003`; batas Dokumen 13 | AUTO-LOCKED |

---

# 3. Resolusi Turunan

1. Contoh SignalR yang memakai Merge pada Dokumen 06 hanya ilustrasi envelope; mapping Core yang spesifik lebih berwenang: Merge tetap LOCAL dan memakai local IPC bila background.
2. Profil umum `L1` tidak membuka server path bagi Merge karena mapping feature-specific mengunci Processing API dan SignalR sebagai `No`.
3. Status/error code exact untuk direct server attempt terhadap local-only Feature tetap `CONTRACT_PENDING`; test mengunci safe rejection dan zero Job/upload/engine tanpa mengarang status.
4. Feature-platform matrix executable digenerate dari registry, `CapabilityVersion`, dan release configuration; Test Plan tidak membuat exposure baru.
5. Numeric threshold, supported-version window, tool/vendor, topology UAT, SSO test tenant, retention, RPO/RTO, dan capacity limit tetap governed inputs sebelum executable gate terkait.
6. L2 Audit accepted/terminal untuk 33 Core tetap wajib; progress/page/chunk/preview/reorder tidak menciptakan Audit noise.

---

# 4. Status

```text
Keputusan teknis : 25 / 25 AUTO-LOCKED
Konflik material : 0
Persetujuan user : 0 diperlukan untuk finalisasi Test Plan
Perubahan 01–11  : 0
```
