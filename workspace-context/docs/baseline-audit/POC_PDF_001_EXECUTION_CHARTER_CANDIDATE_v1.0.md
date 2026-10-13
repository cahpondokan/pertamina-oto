# POC-PDF-001 EXECUTION CHARTER
## PERTAMINA OTO — Kandidat Non-Authoritative

> Versi: CANDIDATE v1.0  
> Tanggal persiapan: 2 September 2026  
> Status: **NON-AUTHORITATIVE — READY FOR HUMAN REVIEW — NOT AUTHORIZED TO EXECUTE**  
> Kandidat engine/package: `TBD_APPROVED_CANDIDATE`  
> Batas: dokumen ini tidak memilih vendor, tidak menyetujui dependency, tidak mengizinkan instalasi, tidak menjalankan PoC, dan tidak membuktikan kesiapan Production.

---

# 1. Tujuan dan Kedudukan Charter

Charter ini menyiapkan cara menjalankan `POC-PDF-001` secara aman, terukur, dapat diulang, dan dapat diaudit setelah seluruh gerbang manusia dan teknis terpenuhi.

Objective utama:

1. membuktikan apakah `TBD_APPROVED_CANDIDATE` layak menjadi primary local PDF engine untuk scope Merge yang diuji;
2. membuktikan integritas urutan, jumlah halaman, occurrence duplikat, konten penanda, dan kemampuan membuka ulang hasil;
3. membuktikan source tetap utuh dan hanya hasil yang sudah tervalidasi yang dapat dipublikasikan secara atomik;
4. membuktikan Merge dapat berjalan lokal tanpa upload, API processing, SignalR, server Job, atau penyimpanan dokumen server;
5. menguji penanganan aman terhadap PDF encrypted, invalid, corrupted, malformed, polyglot, embedded object, action, dan active content;
6. mengukur CPU, RAM, disk, temp, handle/thread, durasi, cancellation, cleanup, concurrency, dan repeatability pada environment yang dideklarasikan;
7. membuktikan dependency kandidat dapat tetap berada di belakang contract/resolver/adapter yang vendor-neutral;
8. menghasilkan evidence package untuk keputusan manusia yang dapat ditelusuri ke candidate, build, commit, environment, corpus, config, dan run.

Scope langsung:

```text
PoC ID            : POC-PDF-001
Feature utama     : FEAT-PDF-005 — Merge PDF
Processing Mode   : LOCAL
Server Dependency : NONE
Input             : minimal dua PDF berurutan
Output            : satu PDF baru
Finalitas         : temp → validasi independen → atomic Save As
Source            : tidak berubah; tidak ditimpa diam-diam
Partial success   : tidak diterima
Candidate         : TBD_APPROVED_CANDIDATE
```

Capability PDF lain hanya boleh masuk jika Feature ID dan batas eksperimennya ditambahkan melalui approval charter. Kelulusan Merge tidak boleh dianggap otomatis membuktikan seluruh 33 Core PDF Tools.

---

# 2. Non-Goals

PoC ini tidak bertujuan untuk:

- memilih vendor atau package secara otomatis;
- membuat keputusan procurement, biaya, atau lisensi;
- membuktikan Production readiness, UAT, Pilot, handover, atau Definition of Done delapan lapis;
- menggantikan `UT-MERGE-001..050`, `TP-MRG-001..036`, atau `UAT-MRG-001..027`;
- membuat server endpoint, server queue, upload flow, SignalR processing event, atau server document storage untuk Merge;
- membuat dynamic third-party plugin loading;
- mengimplementasikan Local Worker sebelum exact IPC contract `BASE-OI-013` selesai;
- menetapkan batas numerik file, halaman, RAM, durasi, storage, atau concurrency tanpa measured baseline dan approval;
- menggunakan dokumen Production, identitas pegawai nyata, signature nyata, secret, token, password nyata, backup, log, atau dump sebagai corpus;
- mengubah Dokumen 01–19 FINAL, baseline manifest, roadmap, atau keputusan canonical;
- mengizinkan real PDF adapter masuk ke produk sebelum keputusan PoC yang sah.

---

# 3. Dasar Canonical dan Trace Minimum

| Sumber | Penggunaan dalam charter |
|---|---|
| `docs/baseline-audit/agent-output/AGENT_C_POC_PDF_001_READINESS.md` | Audit kesiapan, eksperimen, corpus, evidence, risiko, dan gerbang |
| `docs/baseline-audit/POC_MASTER_PLAN_v1.0.md` | Model keputusan, record wajib, stop rules, dan evidence minimum |
| `docs/baseline-audit/IMPLEMENTATION_READINESS_FINAL_v1.0.md` | Status NO-GO, Phase 0 gate, Merge local-only, real adapter diblokir PoC |
| `docs/baseline-audit/IMPLEMENTATION_ROADMAP_v1.0.md` | Urutan BOOT-001, Phase 0–3, dan milestone M3 |
| `docs/baseline-audit/REPOSITORY_BOOTSTRAP_PLAN_v1.0.md` | BOOT-001..008, dependency admission, boundary project/adapter |
| `docs/baseline-audit/OPEN_ITEM_AND_CONFIRMATION_REGISTER.md` | `BASE-OI-013`, `BASE-OI-017..021`, dan `CONF-003..006` |
| `01_BUSINESS_PROCESS_FINAL_v3.4_PERTAMINA_OTO.md` | Validasi bertingkat, source safety, progress/cancel/recovery, finality |
| `02_REQUIREMENT_SPECIFICATION_FINAL_v1.5_PERTAMINA_OTO.md` | File/process/storage/performance/security/recovery/governance requirements |
| `03_FEATURE_CATALOG_FINAL_v1.3_PERTAMINA_OTO.md` | Identitas canonical `FEAT-PDF-005` dan dependency `PDF_ENGINE` |
| `04_SOLUTION_DESIGN_FINAL_v1.2_PERTAMINA_OTO.md` | Contract → Resolver → Adapter → Approved Engine; `IPdfMergeEngine` |
| `05_TECHNICAL_REFERENCE_FINAL_v1.2_PERTAMINA_OTO.md` | `TECH-DEC-005/006`, provider abstraction, Merge first vertical slice |
| `08_SECURITY_DESIGN_FINAL_v1.0_PERTAMINA_OTO.md` | Untrusted file, isolation, active content, dependency/SBOM, safe evidence |
| `11_UNIT_TEST_DOCUMENT_FINAL_v1.0_PERTAMINA_OTO.md` | Contract Merge dan `UT-MERGE-001..050` |
| `12_TEST_PLAN_FINAL_v1.0_PERTAMINA_OTO.md` | Corpus, oracle, metrik, dan `TP-MRG-001..036` |
| `13_UAT_DOCUMENT_FINAL_v1.0_PERTAMINA_OTO.md` | Batas bukti UAT; actual UAT tetap terpisah |
| `18_APPLICATION_CARTOGRAPHY_FINAL_v1.0_PERTAMINA_OTO.md` | Boundary Pdf/Local Worker dan external dependency placeholder |

Requirement yang boleh dipakai dalam PoC harus berasal dari canonical baseline. `FR-FILE-006..009` tetap gap dan tidak boleh diberi arti baru oleh charter ini.

---

# 4. Preconditions dan Entry Gate

## 4.1 Preconditions wajib

| ID | Prasyarat | Nilai/keadaan yang harus tersedia | Status saat charter dibuat | Evidence |
|---|---|---|---|---|
| `PRE-001` | BOOT-001 | Repo resmi, default branch, access, owner, integrator, reviewer, dan accepted baseline | `BLOCKED` | `TBC` |
| `PRE-002` | BOOT-002 minimum | Exact .NET SDK patch, build agent, approved feed/source mapping, dependency admission process | `BLOCKED` | `TBC` |
| `PRE-003` | PoC task card | Exact IDs, allowed paths, do-not-touch paths, branch/worktree, tests, reviewer, stop rules | `TBC` | `TBC` |
| `PRE-004` | Candidate admitted for evaluation | Exact product/package/build/version/source/digest dan dependency graph | `TBD_APPROVED_CANDIDATE` | `TBC` |
| `PRE-005` | License/cost review | Reviewer, license text/version, obligation, redistribution/runtime/support/cost disposition | `TBC` | `TBC` |
| `PRE-006` | Security review | Isolation, no-network, active-content, password, log/evidence, stop authority | `TBC` | `TBC` |
| `PRE-007` | Corpus authority | Manifest, provenance/license, classification, owner, access, retention, disposal | `TBC` | `TBC` |
| `PRE-008` | Independent oracle | Expected properties dan verifier yang tidak hanya mempercayai kandidat | `TBD_APPROVED_CANDIDATE` | `TBC` |
| `PRE-009` | Isolated environment | OS/architecture/runtime/hardware, network control, quota temp, recovery point | `TBC` | `TBC` |
| `PRE-010` | Measurement plan | Tool/instrumentation, sampling, cold/warm policy, run count, result format | `TBC` | `TBC` |
| `PRE-011` | Evidence location | Restricted, reviewable, immutable/hashed sesuai policy; tidak memuat secret | `TBC` | `TBC` |
| `PRE-012` | Execution authorization | Human approver menerima charter dan memberi izin run terbatas | `TBC` | `TBC` |

## 4.2 Entry decision

Eksekusi hanya boleh dimulai jika seluruh `PRE-001..012` berstatus `PASS` dan tidak ada stop condition aktif.

```text
Entry reviewer recommendation : TBC
Entry approver decision        : TBC (AUTHORIZED / REVISION_REQUIRED / BLOCKED)
Authorized candidate           : TBD_APPROVED_CANDIDATE
Authorized scope               : TBC
Authorized environment         : TBC
Authorized evidence root       : TBC
Effective date                 : TBC
Approval evidence              : TBC
```

Persetujuan atas prompt, rencana AI, atau charter draft tidak sama dengan execution authorization.

---

# 5. Candidate Intake Form

Satu formulir diisi per kandidat. Jangan mengisi nama package, vendor, atau version dari asumsi.

## 5.1 Identitas kandidat

| Field | Isian |
|---|---|
| Candidate record ID | `TBC` |
| Product/package/runtime name | `TBD_APPROVED_CANDIDATE` |
| Exact version/build | `TBC` |
| Distribution/source/feed | `TBC` |
| Source authority/owner | `TBC` |
| Package/artifact digest | `TBC` |
| Package signature/provenance | `TBC` |
| Direct dependencies | `TBC` |
| Transitive dependencies | `TBC` |
| Native components/binaries | `TBC` |
| Supported OS/architecture/runtime | `TBC` |
| Candidate support lifecycle | `TBC` |
| Patch/CVE response process | `TBC` |
| Maintainer/support channel | `TBC` |
| Removal/exit path | `TBC` |

## 5.2 Scope dan alasan evaluasi

| Field | Isian |
|---|---|
| Capability yang diuji | `TBC` |
| Capability yang tidak diuji | `TBC` |
| Exact Feature IDs | `FEAT-PDF-005`; tambahan: `TBC` |
| Requirement IDs | `TBC` |
| Test trace | `TBC` |
| Alasan masuk evaluasi | `TBC` |
| Alternatif yang sudah diberi authority untuk dibandingkan | `TBC` |
| Known limitations sebelum run | `TBC` |
| Data/network behavior yang diklaim | `TBC` |

## 5.3 License, biaya, dan dependency admission

| Field | Isian |
|---|---|
| Exact license name/version/text location | `TBC` |
| Penggunaan internal yang diizinkan | `TBC` |
| Redistribution/desktop packaging terms | `TBC` |
| Runtime/server/native terms | `TBC` |
| Notice/source disclosure obligations | `TBC` |
| Biaya/subscription/support model | `TBC` |
| License/Legal reviewer | `TBC` |
| Security reviewer | `TBC` |
| Vulnerability result/date/source | `TBC` |
| SBOM location/digest | `TBC` |
| Waiver/residual risk, bila ada | `TBC` |
| Admission decision | `TBC` (`APPROVED_FOR_POC_ONLY` / `REVISION_REQUIRED` / `REJECTED`) |
| Decision evidence/date | `TBC` |

Kandidat hanya boleh masuk environment PoC setelah keputusan `APPROVED_FOR_POC_ONLY`. Keputusan ini tidak sama dengan persetujuan sebagai engine produk.

---

# 6. Rancangan Corpus Sintetis dan Hostile

## 6.1 Kelompok corpus minimum

| Corpus group | Isi minimum | Oracle/properti yang dicatat |
|---|---|---|
| `CONTROL` | PDF sintetis A dan B, marker halaman unik, lebih dari satu halaman bila diperlukan | Digest file/halaman, page count, order, marker, reopen |
| `ORDER_DUPLICATE` | A, B, C; urutan C,A,B; occurrence A,A | Provenance occurrence, no silent dedupe, no missing/extra page |
| `STRUCTURAL_VALID` | Text, vector, raster, mixed page size/orientation, rotated page, embedded font, bookmark, annotation/form sesuai scope | Expected preservation/known limitation per properti |
| `PASSWORD` | PDF encrypted sintetis dengan password uji non-rahasia | Missing/wrong/correct flow; password tidak persist/log/evidence |
| `INVALID_CORRUPT` | Fake extension, bad signature, truncated, corrupt, malformed xref/object/stream, empty/zero-page bila valid untuk negative test | Fail closed, no final output, bounded handling |
| `ACTIVE_UNTRUSTED` | Synthetic action, JavaScript, link, embedded object, dan polyglot | Tidak ada execution/egress/shell; hasil inspeksi dan containment |
| `RESOURCE` | `CONTROL`, `TYPICAL`, `LARGE_SUPPORTED`, `POLICY_BOUNDARY`, `OVER_LIMIT`, `ADVERSARIAL_VALID` | Ukuran aktual dan semua resource metric; band value tetap `TBC` sampai disetujui |
| `FAILURE` | Cancel/crash/timeout/write/rename denial dan quota-limited temp yang terisolasi | Source/target digest, finality, cleanup/reconciliation |

## 6.2 Corpus manifest

Setiap item wajib mempunyai:

```text
Corpus ID/version              : TBC
Purpose                       : TBC
Requirement/Feature/Test IDs  : TBC
Generator/seed/provenance     : TBC
License/use authority         : TBC
Classification/sensitivity   : TBC
Format/count/size/digest      : TBC
Expected page/order/content   : TBC
Expected reopen property      : TBC
Known active/hostile behavior : TBC
Allowed environment/suite     : TBC
Owner/reviewer/access         : TBC
Created/reviewed/expiry       : TBC
Retention/disposal            : TBC
```

## 6.3 Aturan corpus

- Synthetic/generated data adalah default.
- Data Production, identitas nyata, signature nyata, secret, token, password nyata, backup, log, dan dump dilarang.
- Sanitized representative data hanya boleh dipakai jika fidelity tidak dapat dibuktikan secara sintetis dan terdapat approval, sanitization verification, restricted access, retention, serta disposal yang sah.
- Hostile corpus harus diberi label, hash, manifest, akses terbatas, dan disimpan terpisah dari corpus biasa.
- Hostile corpus hanya dijalankan di environment non-Production terisolasi dengan default-deny network dan proses berprivilege minimum.
- Disk-full tidak boleh disimulasikan pada system drive atau workspace root; gunakan target/quota khusus yang sudah diverifikasi aman.

---

# 7. Tiga Belas Eksperimen Minimum

Seluruh eksperimen `PDF001-E00..E12` wajib direncanakan. Eksperimen boleh ditandai `BLOCKED` hanya dengan alasan dan evidence yang jelas; tidak boleh dihitung `PASS`.

| ID | Eksperimen | Minimum tindakan dan oracle | Evidence minimum | Keputusan |
|---|---|---|---|---|
| `PDF001-E00` | Admission dan offline proof | Verifikasi exact candidate/build/digest, restore hanya dari feed approved, resolved graph, license/SBOM; jalankan dengan network disabled dan amati egress | Candidate manifest, restore/source record, graph, SBOM, license/security record, network evidence | `TBC` |
| `PDF001-E01` | Happy path dua dan banyak PDF | Merge dua lalu banyak PDF; validasi page count, order, marker/provenance, reopen, source digest | Run manifest, result digests, oracle result, sanitized log | `TBC` |
| `PDF001-E02` | Reorder dan duplicate occurrence | Jalankan C,A,B dan A,A; pastikan occurrence tidak di-dedupe dan tidak ada halaman hilang/tambah | Ordered input manifest, page provenance, independent result | `TBC` |
| `PDF001-E03` | Variasi struktur sah | Uji structural-valid corpus; catat preservation dan limitation per text/vector/raster/page size/orientation/font/bookmark/annotation/form yang disetujui | Capability-limitation matrix, render/structure result, reopen evidence | `TBC` |
| `PDF001-E04` | Encrypted/password | Tanpa password, password salah, dan benar; password hanya ephemeral dan tidak muncul di log/evidence/temp | State/error mapping, canary scan, secret-handling evidence | `TBC` |
| `PDF001-E05` | Invalid dan hostile PDF | Uji fake extension/signature, corrupt, malformed, polyglot, action/JavaScript/embedded object; fail closed dan tidak execute active content | Process/file/network observation, bounded failure result, no-final proof | `TBC` |
| `PDF001-E06` | Validasi hasil independen | Verifier terpisah memeriksa page/order/content/reopen; injeksikan output empty/wrong/truncated/corrupt dan pastikan tidak dipublikasikan | Oracle identity, validation report, rejected-publication evidence | `TBC` |
| `PDF001-E07` | Source safety dan atomic publication | Tolak target=source; lindungi existing target; injeksikan staging/write/rename failure; bandingkan digest sebelum/sesudah | Source/target digests, staging manifest, finality/reconciliation result | `TBC` |
| `PDF001-E08` | Cancel, crash, timeout, dan temp | Cancel queued/running, hentikan runner secara terkendali, restart/reconcile; pastikan no false completion/late publish | Lifecycle timeline, temp inventory, cleanup/reconcile result, digest | `TBC` |
| `PDF001-E09` | Resource bands | Jalankan semua band approved; ukur CPU/RAM/GC/thread/handle/disk/temp/duration/cleanup dan correctness | Environment/config/corpus manifests, raw and summarized measurement | `TBC` |
| `PDF001-E10` | Concurrency dan repeatability | Jalankan single dan bounded parallel sesuai nilai approved; ulangi run identik; periksa collision, residual growth, dan semantic consistency | Run series, concurrency setting, result comparison, residual inventory | `TBC` |
| `PDF001-E11` | Adapter/resolver boundary | Buktikan candidate hanya di infrastructure adapter; contract/domain/UI/public application contract bebas tipe/nama vendor; map error/limitation aman | Dependency/reference inspection, architecture test/report, boundary diagram | `TBC` |
| `PDF001-E12` | Disable, removal, dan fallback drill | Nonaktifkan/lepaskan candidate lewat profile yang disetujui; hasil safe unavailable atau fallback lokal yang memang approved; pastikan tidak ada fallback server/public endpoint | Config/profile record, dependency diff, no-egress/no-server proof, outcome | `TBC` |

Trace langsung yang dipakai untuk oracle dan test design:

- valid/order/duplicate: `UT-MERGE-003..019`, `TP-MRG-001`, `TP-MRG-004..008`;
- local/offline/engine route: `UT-MERGE-020..024`, `TP-MRG-012..018`;
- cancel/failure/recovery: `UT-MERGE-025..034`, `TP-MRG-019..025`, `TP-MRG-028..031`;
- source/finality: `UT-MERGE-035..038`, `TP-MRG-025..029`;
- privacy/evidence: `UT-MERGE-044..046`, `TP-MRG-030..034`.

Jika Local Worker belum diizinkan karena `BASE-OI-013`, eksperimen in-process yang aman dapat dieksekusi hanya sesuai scope approval. Klaim Worker/large/background tetap `BLOCKED`, bukan `PASS`.

---

# 8. Environment, Isolation, dan Cleanup

## 8.1 Environment manifest

```text
Environment ID                 : TBC
Purpose                        : POC-PDF-001 only
Production status              : NON-PRODUCTION
OS/build/architecture          : TBC
Runtime/SDK                    : TBC
CPU/RAM/storage                : TBC
Candidate/version/digest       : TBD_APPROVED_CANDIDATE / TBC
Oracle/verifier/version/digest : TBD_APPROVED_CANDIDATE / TBC
Branch/worktree/commit/build   : TBC
Network mode/egress control    : TBC
Execution identity/privilege   : TBC
Allowed input/output roots     : TBC
Dedicated temp/quota root      : TBC
Instrumentation/version        : TBC
Snapshot/recovery point        : TBC
Owner/reviewer                 : TBC
```

## 8.2 Isolation controls

- Environment harus non-Production dan khusus untuk PoC.
- Eksekusi memakai least-privilege identity; tidak menggunakan admin/shared credential jika tidak disetujui.
- Network default-deny untuk pembuktian offline; exception hanya untuk admission/restore yang sudah disetujui dan dicatat sebelum run engine.
- Input, output, evidence, dan temp berada di root terpisah yang exact path-nya disetujui; jangan gunakan root drive, workspace root, atau folder pengguna secara luas sebagai target eksperimen destruktif.
- Hostile corpus tidak boleh dibuka oleh aplikasi umum di luar safe runner.
- Process/shell/network/file access yang tidak diperlukan harus ditolak atau dimonitor sesuai security plan.
- Secret, password, document content, raw path, storage key/URL, dan dump sensitif tidak boleh masuk log/evidence.
- Local Worker hanya boleh dipakai setelah IPC/auth/version contract dan negative tests disetujui.

## 8.3 Cleanup dan reconciliation

Setiap run wajib mempunyai inventory sebelum dan sesudah:

```text
Run ID                        : TBC
Input digest set              : TBC
Existing target digest        : TBC
Staging/temp manifest         : TBC
Published output digest       : TBC
Orphan/residual inventory     : TBC
Cleanup action/result         : TBC
Reconciliation action/result  : TBC
Source/target post-run digest : TBC
Reviewer result               : TBC
```

Cleanup hanya boleh menyentuh exact run-scoped paths yang tercatat. Jika path tidak dapat dipastikan, hentikan cleanup dan minta review; jangan melakukan recursive delete pada path luas atau hasil komputasi yang belum diverifikasi.

---

# 9. Resource Measurement Tanpa Threshold Rekaan

## 9.1 Metrik wajib

| Area | Metrik yang dicatat |
|---|---|
| CPU | Utilization, CPU time, saturation duration, sampling interval |
| RAM/.NET | Working/private/committed, GC heap/allocation/LOH/pause, residual memory |
| Process | Thread/handle count, child process, crash/recycle |
| Disk/temp | Read/write bytes, free space, peak temp per Job/run, latency/queue bila tersedia, orphan/cleanup |
| Duration | Total dan tahap preflight/process/validation/publication/cleanup |
| Cancellation | Waktu request→safe stop, late completion, final state |
| Concurrency | Approved parallel level, queue behavior, collision, per-run dan aggregate resource |
| Network | Processing egress/upload/API/SignalR/server Job count; untuk Merge harus nol |
| Correctness under load | Page/order/marker/reopen, source digest, finality, error mapping |
| UI/harness, bila ada | Input latency, main-thread stall, progress delay, recovery behavior |

## 9.2 Context wajib untuk setiap pengukuran

```text
Candidate/version/build/digest : TBC
Commit/build/run ID            : TBC
Environment manifest/digest    : TBC
Corpus manifest/digest         : TBC
Config/policy/digest           : TBC
Cold/warm state                : TBC
Instrumentation/version        : TBC
Sampling/window/duration       : TBC
Repeat/run count               : TBC
Raw evidence location/digest   : TBC
```

## 9.3 Aturan penilaian angka

Baseline canonical tidak memberi angka universal untuk maksimum file, halaman, ukuran, RAM, durasi, storage, atau concurrency. Karena itu:

```text
Hard invariant nonnumerik       : PASS / FAIL
Resource run lengkap dan valid  : MEASURED_BASELINE
Target numerik belum disetujui  : BLOCKED_PENDING_TARGET_APPROVAL
Target/policy yang sudah sah    : TBC
Comparison antar kandidat       : hanya pada environment/corpus/config yang sama
Pemilihan pemenang              : TBC oleh human authority; bukan oleh Agent
```

Semua run dicatat, bukan hanya run terbaik. Outlier tidak boleh dihapus tanpa alasan, reviewer, dan evidence.

---

# 10. Hard PASS/FAIL dan Stop Rules

## 10.1 Hard invariants

| Area | PASS | FAIL/STOP |
|---|---|---|
| Ketepatan | Page count, order, occurrence, marker/provenance, dan reopen sesuai oracle | Halaman hilang/tambah/salah urut, silent dedupe, hasil tidak dapat dibuka |
| Source safety | Semua source dan existing target mempunyai digest sama sebelum/sesudah success/failure/cancel/crash | Source/target berubah, rusak, atau tertimpa diam-diam |
| Finalitas | Hanya output tervalidasi dipublikasikan atomik | Temp/partial/corrupt dianggap final atau half-final terlihat sebagai hasil |
| Local-First | Processing upload/API/SignalR/server Job/egress = 0 dan offline run berhasil untuk scope yang dibuktikan | Kandidat memerlukan egress, public endpoint, hidden upload, atau server processing |
| Active content | Script/action/executable tidak dijalankan; invalid/inconclusive fail closed | Active content berjalan atau ada process/shell/network/file access tak sah |
| Password/privacy | Password ephemeral; CanarySet tidak muncul pada area yang dilarang | Password/content/raw path/secret bocor atau persist |
| Failure/recovery | Cancel/crash/timeout bounded; no false/late completion; temp/orphan dibersihkan atau direkonsiliasi | Hang tanpa batas, false success, late publish, orphan tidak terkendali |
| Architecture boundary | Candidate hanya di adapter; contract/resolver/domain/UI tetap vendor-neutral | UI/Domain/public contract mengenal vendor atau memilih provider langsung |
| License/dependency | Source, graph, license, obligation, runtime/redistribution/support dapat diverifikasi dan disetujui | License tidak jelas/tidak sesuai, source tak approved, dependency tak terlacak |
| Repeatability | Properti semantik dapat diulang dengan manifest dan evidence lengkap | Hanya demo, tidak ada oracle, atau hasil tidak dapat direproduksi |

Satu hard invariant `FAIL` menghentikan klaim kelulusan kandidat. Perbaikan dan rerun hanya boleh dilakukan melalui task/charter revision yang tercatat. Hasil gagal tidak boleh disembunyikan atau diganti dengan silent scope reduction.

## 10.2 Stop conditions

Hentikan run atau seluruh evaluasi dan laporkan kepada Human Owner/Agent 0 jika:

1. `PRE-001..012` belum seluruhnya `PASS`;
2. candidate/version/source/digest berbeda dari intake yang disetujui;
3. license, package source, dependency graph, atau corpus authority tidak dapat diverifikasi;
4. kandidat memerlukan public endpoint, hidden upload, server processing, atau architecture/security boundary baru;
5. source/existing target dapat rusak atau tertimpa;
6. active content dijalankan atau ada process/shell/network/file access yang tidak diizinkan;
7. secret, password, document content, raw path, atau data sensitif muncul di log/evidence;
8. output tidak dapat divalidasi secara independen;
9. crash, hang, resource exhaustion, cancellation, atau cleanup tidak bounded;
10. evidence tidak dapat disanitasi, ditelusuri, di-hash, atau direproduksi;
11. acceptance bertentangan dengan canonical requirements;
12. branch/worktree/environment/corpus berubah tanpa controlled re-freeze;
13. ada permintaan menjalankan PoC pada Production atau menggunakan data Production;
14. ada permintaan mengubah dokumen FINAL/manifest tanpa controlled revision;
15. reviewer/approver tidak memiliki authority yang dapat dibuktikan.

Catatan stop:

```text
Stop record ID/referensi : TBC
Run ID                   : TBC
Tanggal/waktu            : TBC
Temuan                   : TBC
Dampak                   : TBC
Immediate containment    : TBC
Owner tindakan           : TBC
Evidence                 : TBC
Keputusan lanjut         : TBC
```

---

# 11. Security, Privacy, License, dan Maintainability Review

| Area review | Bukti minimum | Reviewer/decision |
|---|---|---|
| Parser/engine vulnerability | Approved scan/advisory source, date, exact candidate/build, severity disposition | `TBC` |
| Active content | Hostile corpus, process/file/network observation, default-deny action | `TBC` |
| Resource exhaustion/file bomb | Preflight, quota, bounded concurrency, object/page/output/temp/time observation | `TBC` |
| Native/memory-unsafe component | Native inventory, platform support, isolation/crash/rollback evidence | `TBC` |
| Egress/telemetry | Config review, network-disabled run, capture/counter, no public endpoint | `TBC` |
| Password/content privacy | Ephemeral password handling, CanarySet scan, log/temp/dump policy | `TBC` |
| Output trust | Independent structural/semantic verifier dan reopen sebelum publication | `TBC` |
| Dependency provenance | Feed/source, signature/digest, direct/transitive/native graph, SBOM | `TBC` |
| License/redistribution | Exact license/version/text, obligations, desktop/runtime/native/support/cost terms | `TBC` |
| Maintainability | Release cadence, supported OS/.NET matrix, issue/CVE response, support/exit plan | `TBC` |
| Vendor lock-in | Vendor-neutral contract, adapter-only dependency, capability/limitation matrix, removal drill | `TBC` |

License atau data authority yang tidak dapat diverifikasi adalah stop condition. Agent tidak menerima residual risk dan tidak memberi waiver.

---

# 12. Evidence Naming, Struktur, dan Index

## 12.1 Naming convention kandidat

Nilai final location mengikuti policy repository/evidence yang disetujui.

```text
Evidence root : TBC
Logical root  : POC-PDF-001/<candidate-record-id>/<run-set-id>/

Run set ID    : POC-PDF-001-RS-<UTC-YYYYMMDDTHHMMSSZ>-<short-commit>-<short-env-digest>
Run ID        : POC-PDF-001-<E00..E12>-R<NNN>
Evidence ID   : POC-PDF-001-EV-<NNN>
Corpus ID     : POC-PDF-001-CORP-<GROUP>-<NNN>
Stop ID       : POC-PDF-001-STOP-<NNN>
Decision ID   : POC-PDF-001-DEC-<NNN>
```

Nama harus stabil dan tidak memuat nama pengguna, raw path, document content, password, secret, atau data sensitif.

## 12.2 Struktur logical evidence package

```text
POC-PDF-001/
└── <candidate-record-id>/
    └── <run-set-id>/
        ├── 00-charter/
        │   ├── approved-charter.md
        │   └── approval-evidence-index.md
        ├── 01-candidate/
        │   ├── candidate-manifest.json
        │   ├── dependency-graph.json
        │   ├── sbom.TBC
        │   └── dependency-license-security-record.md
        ├── 02-environment/
        │   └── environment-manifest.json
        ├── 03-corpus/
        │   ├── corpus-manifest.json
        │   └── expected-properties.json
        ├── 04-runs/
        │   ├── run-manifest.jsonl
        │   ├── correctness-results.json
        │   ├── resource-results.json
        │   └── failure-recovery-results.json
        ├── 05-sanitized-logs/
        ├── 06-digests/
        │   └── result-digests.json
        ├── 07-review/
        │   ├── capability-limitation-matrix.md
        │   ├── security-license-review.md
        │   └── evidence-validation-report.md
        └── 08-decision/
            └── decision-record.md
```

Ekstensi `sbom.TBC` menandai bahwa format SBOM belum disetujui; jangan menggantinya dengan format rekaan pada evidence resmi.

## 12.3 Evidence index

| Evidence ID | Eksperimen/area | Deskripsi | Candidate/build | Env/corpus/config | Commit/run | Digest | Lokasi | Klasifikasi/sanitasi | Reviewer | Status |
|---|---|---|---|---|---|---|---|---|---|---|
| `POC-PDF-001-EV-001` | Charter/approval | Approved charter dan authorization | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `POC-PDF-001-EV-002` | Candidate | Candidate/dependency/license/security record | `TBD_APPROVED_CANDIDATE` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `POC-PDF-001-EV-003` | Environment | Frozen environment manifest | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `POC-PDF-001-EV-004` | Corpus | Approved corpus/oracle manifest | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `POC-PDF-001-EV-005` | E00 | Admission/offline result | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `POC-PDF-001-EV-006` | E01–E03 | Correctness/structure result | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `POC-PDF-001-EV-007` | E04–E06 | Password/hostile/oracle result | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `POC-PDF-001-EV-008` | E07–E08 | Source/finality/failure/cleanup result | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `POC-PDF-001-EV-009` | E09–E10 | Resource/concurrency/repeatability result | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `POC-PDF-001-EV-010` | E11–E12 | Architecture/removal/fallback result | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `POC-PDF-001-EV-011` | Review | Limitation, security/license, evidence validation | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `POC-PDF-001-EV-012` | Decision | Human decision record | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |

Setiap evidence wajib terikat ke exact candidate/version/build/digest, commit/build, environment, corpus, config, timestamp, run ID, owner/reviewer, dan hasil sanitasi. Evidence yang berubah setelah hash harus di-hash ulang dan direview ulang.

---

# 13. Result Classification dan Decision Rule

## 13.1 Status per eksperimen

```text
NOT_RUN       : belum dijalankan
BLOCKED       : prasyarat/evidence/authority belum tersedia
PASS          : seluruh acceptance eksperimen terpenuhi dan evidence valid
FAIL          : satu atau lebih acceptance/hard invariant gagal
INCONCLUSIVE  : evidence tidak cukup atau hasil tidak dapat direproduksi
```

`INCONCLUSIVE` tidak boleh dihitung sebagai `PASS`.

## 13.2 Minimum sebelum decision review

```text
[ ] PRE-001..012 PASS
[ ] E00..E12 mempunyai hasil dan evidence tervalidasi
[ ] Seluruh hard invariant PASS
[ ] Tidak ada stop condition aktif
[ ] Candidate/dependency/license/security record lengkap
[ ] Corpus/oracle/environment/config frozen dan terikat digest
[ ] Resource result berstatus MEASURED_BASELINE
[ ] Target numerik/policy boundary berstatus TBC atau sudah approved secara eksplisit
[ ] Capability dan limitation ditulis tanpa silent scope reduction
[ ] Evidence lolos sanitasi, CanarySet, digest, dan independent review
[ ] Removal/fallback path terbukti aman
```

Jika target numerik masih `TBC`, hasil resource tetap `BLOCKED_PENDING_TARGET_APPROVAL`; Agent tidak boleh mengubahnya menjadi PASS dengan ambang buatan.

## 13.3 Pilihan keputusan manusia

| Keputusan | Arti |
|---|---|
| `APPROVED_ENGINE` | Kandidat disetujui hanya untuk capability/scope/platform/config yang dibuktikan dan ditulis eksplisit |
| `APPROVED_WITH_LIMITATION` | Kandidat disetujui dengan limitation, capability guard, fallback, dan test yang diwajibkan |
| `BLOCKED_ALTERNATIVE_REQUIRED` | Kandidat tidak memenuhi hard gate atau risikonya tidak dapat diterima; evaluasi alternatif memerlukan intake/approval baru |
| `DEFERRED_BY_GOVERNED_DECISION` | Keputusan ditunda karena target, authority, evidence, atau kebijakan yang dibutuhkan belum lengkap |

Failed PoC tidak memilih vendor berikutnya secara otomatis dan tidak mengizinkan pengurangan scope diam-diam.

---

# 14. Approval dan Sign-off

## 14.1 Charter approval sebelum eksekusi

```text
Charter version/digest        : TBC
Candidate record/digest       : TBD_APPROVED_CANDIDATE / TBC
Authorized Feature/capability : TBC
Authorized experiments        : PDF001-E00..E12 / pengecualian: TBC
Authorized environment        : TBC
Authorized corpus/oracle      : TBC
Evidence root/access          : TBC
Security reviewer             : TBC
Dependency/License reviewer   : TBC
PDF/Quality reviewer          : TBC
Technical Owner               : TBC
Execution approver            : TBC
Effective date/expiry         : TBC
Decision                     : TBC (AUTHORIZED / REVISION_REQUIRED / BLOCKED)
Approval evidence             : TBC
```

## 14.2 Result review dan engine decision

```text
Run set ID                    : TBC
Candidate/version/build       : TBD_APPROVED_CANDIDATE / TBC
Evidence package digest       : TBC
E00..E12 status               : TBC
Hard invariant status         : TBC
Resource status               : TBC
Security recommendation       : TBC
Dependency/License result     : TBC
PDF/Quality recommendation    : TBC
Known limitations             : TBC
Residual risk                 : TBC
Technical Owner review        : TBC
Product/Application scope     : TBC
Decision authority            : TBC
Decision/date                 : TBC
Decision status               : TBC
Decision evidence             : TBC
```

AI/Agent tidak boleh diisi sebagai owner, reviewer, risk acceptor, atau approver organisasi. Keputusan hanya berlaku pada exact scope, candidate, version/build, environment, corpus, config, dan evidence yang direview.

---

# 15. Urutan Pasca-BOOT-001

```text
1. BOOT-001 dinyatakan PASS oleh human authority
   Repo/access/owner/integrator/reviewer + accepted baseline terikat hash
        ↓
2. Selesaikan BOOT-002 dan Phase 0 minimum
   Exact SDK/build agent/feed/source mapping/dependency policy
   + isolated branch/worktree + build/test/scan authority
        ↓
3. Siapkan vendor-neutral boundary Phase 1
   Contract/resolver/result/progress/cancel/temp/security test ports
   Fake tests boleh; real adapter tetap belum boleh
        ↓
4. Lengkapi dan review charter POC-PDF-001
   Exact Feature IDs, candidate/version, corpus, oracle, environment,
   reviewer, stop rules, evidence path, measurement plan
        ↓
5. Admit candidate hanya untuk PoC
   Source/digest/graph/SBOM/license/security/vulnerability/platform/fallback
        ↓
6. Freeze seluruh input eksekusi
   Charter + candidate + branch/commit/build + environment + corpus
   + oracle + config + instrumentation + evidence root
        ↓
7. Jalankan PDF001-E00..E12
   Admission/offline → correctness/structure/password/hostile/oracle
   → source/finality/failure/cleanup → resource/concurrency → boundary/removal
        ↓
8. Validasi evidence secara independen
   Digests + complete run manifest + sanitized logs + CanarySet
   + no egress/server + limitation matrix + repeatability
        ↓
9. Human decision pada gate M3
   APPROVED_ENGINE / APPROVED_WITH_LIMITATION /
   BLOCKED_ALTERNATIVE_REQUIRED / DEFERRED_BY_GOVERNED_DECISION
        ↓
10. Jika approved atau approved-with-limitation
    Implementasikan real PDF adapter hanya pada scope yang dibuktikan;
    mapping limitation ke capability guard, UI state, dan tests
        ↓
11. Lanjutkan Phase 3 Merge vertical slice
    50 Unit + 36 system + 27 UAT package + evidence delapan lapis
```

Persiapan dokumentasi charter, candidate intake kosong, corpus design, dan evidence schema dapat dilakukan paralel setelah aman. Restore candidate, instalasi dependency, build kandidat, dan eksekusi PoC tetap menunggu seluruh approval yang disebut di charter ini.

---

# 16. Status Saat Charter Dibuat

```text
Charter                         : CANDIDATE — NON-AUTHORITATIVE
Execution authorization        : NOT AUTHORIZED
POC-PDF-001                     : NOT_STARTED
Candidate/vendor/package       : TBD_APPROVED_CANDIDATE
Exact version/source/digest     : TBC
Dependency/license/security     : TBC
Corpus/oracle/environment       : TBC
Numeric target/policy boundary  : TBC
BOOT-001                        : BLOCKED; wajib PASS sebelum urutan lanjut
Real Merge adapter              : BLOCKED_BY_POC-PDF-001
Production action               : NOT AUTHORIZED
```

Dokumen ini hanya menjadi input review. Semua status harus diperbarui melalui evidence dan keputusan manusia yang sah; tidak ada nilai `TBC` atau `TBD_APPROVED_CANDIDATE` yang boleh diisi dari asumsi.
