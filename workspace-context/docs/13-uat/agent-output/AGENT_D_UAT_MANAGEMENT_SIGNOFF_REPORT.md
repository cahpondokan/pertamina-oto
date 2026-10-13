# AGENT D — UAT MANAGEMENT, EVIDENCE, DEFECT & SIGN-OFF REPORT

> **Task ID:** `DOC13-AGENT-D`  
> **Peran:** Senior Project Manager, Senior UAT Manager, QA Governance Analyst, Release Readiness Architect  
> **Status:** Laporan specialist untuk integrasi ke Document 13; bukan master Document 13, bukan hasil UAT aktual, bukan sign-off Pertamina, dan bukan Production approval  
> **Baseline:** Documents 01–12 FINAL; `MASTER_PROJECT_SUMMARY.md` v3.5  
> **Bahasa:** Indonesia `ID-1.0`  
> **Tanggal analisis:** 1 September 2026

Laporan ini merancang tata kelola UAT yang ringan untuk model satu developer + Codex, tetapi tetap menghasilkan bukti yang dapat ditinjau stakeholder Pertamina. Laporan tidak mengubah role aplikasi pada Document 09, tidak membuat nama approver, tidak membuat Feature ID baru, tidak mengubah master Document 13, dan tidak melakukan coding.

Baseline yang dipertahankan:

- UAT hanya dimulai setelah UAT Readiness Gate Document 12 lulus;
- UAT menggunakan release candidate yang immutable dan artifact yang sama dengan yang telah melewati QA;
- UAT adalah lane yang terisolasi secara logis; topology fisik menunggu konfirmasi;
- hasil teknis hijau tidak menggantikan business acceptance;
- traceability adalah `Requirement → canonical Feature → technical test → UAT → evidence/defect → release`;
- severity defect adalah `BLOCKER`, `CRITICAL`, `MAJOR`, dan `MINOR`;
- defect priority tetap terpisah sebagai `DEF-P0..DEF-P3`;
- bukti harus aman, terkontrol akses, tidak ditimpa diam-diam, dan tidak menjadi kanal kebocoran;
- password, token, secret, document content, OCR full text, signature payload, dan data sensitif yang tidak diperlukan tidak masuk evidence;
- Codex boleh membantu persiapan, traceability, test automation pendukung, dan laporan, tetapi tidak boleh memberi business sign-off atau menerima risiko;
- UAT acceptance bukan Production approval.

---

# 1. UAT governance

## 1.1 Tujuan tata kelola

UAT membuktikan bahwa release candidate memenuhi hasil bisnis yang disetujui dari sudut pandang pengguna dan stakeholder, pada platform serta capability yang masuk release scope. UAT tidak mengulang seluruh Unit, Integration, Security, Performance, atau Recovery Test; evidence teknis tersebut menjadi prasyarat dan referensi UAT.

Model minimum:

```text
UAT Readiness PASS dari Document 12
→ scope/case/data/account freeze
→ pre-UAT smoke
→ business execution
→ evidence + defect
→ retest + affected regression
→ UAT summary
→ human sign-off decision
→ release-governance handoff
```

## 1.2 Prinsip

1. **Business-outcome first.** PASS ditentukan oleh hasil bisnis, bukan sekadar tidak ada exception.
2. **Gate before execution.** UAT tidak dipakai untuk menggantikan QA yang belum selesai.
3. **Same immutable artifact.** Build yang diuji adalah artifact pipeline yang sama; tidak ada rebuild atau patch manual setelah evidence QA.
4. **Lean single source.** Satu scope/traceability register, satu defect register, satu evidence manifest, satu summary, dan satu sign-off record per candidate.
5. **Evidence-based.** Keputusan harus menunjuk evidence yang dapat ditemukan kembali; ingatan atau pernyataan lisan saja tidak cukup.
6. **No false green.** `BLOCKED`, `NOT_RUN`, `INCONCLUSIVE`, dan `N/A` tidak dihitung sebagai PASS.
7. **Privacy-safe.** Evidence hanya memuat data minimum yang diperlukan dan mengikuti klasifikasi/akses/retensi yang disetujui.
8. **Risk-proportionate review.** Fitur bisnis biasa tidak memerlukan birokrasi berlapis; security, data-loss, destructive, atau high-impact flow memerlukan reviewer yang sesuai.
9. **Traceable change.** Build, configuration, data, expected result, atau requirement yang berubah setelah eksekusi harus memicu impact analysis dan scoped rerun.
10. **Human acceptance.** Codex, pipeline, developer, atau Support Console tidak dapat membuat keputusan business acceptance atas nama stakeholder.

## 1.3 Fungsi tanggung jawab UAT

Nama berikut adalah fungsi proyek, bukan role aplikasi baru dan tidak mengubah Role Matrix Document 09.

| Fungsi proyek | Tanggung jawab minimum | Boleh dirangkap? | Batas |
|---|---|---|---|
| Business/UAT Decision Authority | Menyetujui scope, menilai outcome bisnis, memutus hasil UAT dan known-issue acceptance | Dapat dirangkap oleh stakeholder yang memang diberi kewenangan | Nama, jabatan, delegasi, dan authority belum boleh diasumsikan |
| UAT Coordinator | Menyiapkan plan ringan, jadwal, case, data/account, execution register, defect flow, summary | Ya; dapat dilakukan developer pada model solo | Tidak boleh self-approve business acceptance tanpa authority |
| UAT Tester/Business Representative | Menjalankan atau menyaksikan business scenarios dan mencatat hasil | Dapat lebih dari satu sesuai scope | Tidak boleh diberi akses melebihi scenario/permission |
| QA/Release Evidence Owner | Menyerahkan QA summary, immutable RC, gate evidence, known issues, dan environment manifest | Ya; dapat dirangkap developer | Tidak boleh menyatakan UAT PASS hanya dari technical green |
| Developer | Menjelaskan build, memperbaiki defect, menyiapkan retest, menjaga regression | Ya | Tidak boleh menutup defect tanpa evidence retest yang diwajibkan |
| Security/Technical Reviewer | Menilai exception atau evidence pada scope security, privacy, data integrity, performance, recovery, dan platform | Hanya bila applicable | Tidak menggantikan business sign-off |
| Codex | Membantu membuat case draft, trace matrix, script pendukung, summary, consistency check | Ya, sebagai alat | Tidak menjadi approver, tester manusia, risk owner, atau sumber evidence palsu |

Untuk model satu developer, pemisahan yang wajib dipertahankan bukan organisasi besar, melainkan dua keputusan manusia yang jelas:

1. developer/QA menyatakan candidate **siap ditinjau** berdasarkan evidence;
2. stakeholder yang berwenang menyatakan outcome bisnis **diterima atau ditolak**.

## 1.4 Artefak minimum

| Artefak | Tujuan | Bentuk minimum |
|---|---|---|
| UAT Scope & Traceability Register | Menentukan apa yang diterima | Requirement, Feature, business outcome, UAT ID, platform, result |
| UAT Case | Langkah dan expected result | Template Bagian 6 |
| Execution Record | Membuktikan siapa/apa/kapan/hasil | UAT ID, build, environment, tester, timestamp, result |
| Defect Register | Mengelola ketidaksesuaian | Template Bagian 7 |
| Evidence Manifest | Menunjuk bukti aman | Template Bagian 10 |
| UAT Summary | Menyajikan status candidate | Format Bagian 11 |
| Known-Issue Decision Log | Merekam acceptance risiko terbatas | Format Bagian 12 |
| Sign-Off Record | Merekam keputusan manusia | Format Bagian 13 |

Artefak tersebut boleh dikelola dalam mekanisme repository/work item/report yang sudah disetujui. Laporan ini tidak memilih produk, SaaS, atau workflow berbayar baru.

---

# 2. Entry criteria

## 2.1 UAT Entry Gate

Status entry hanya:

- `READY` — seluruh mandatory entry terpenuhi;
- `NOT_READY` — candidate belum memenuhi satu atau lebih kriteria yang dapat diselesaikan;
- `BLOCKED` — UAT tidak dapat disiapkan karena keputusan, environment, identity, data, atau dependency eksternal belum tersedia.

Checklist mandatory:

```text
[ ] Release Candidate mempunyai version, commit, pipeline run, artifact digest, dan configuration/schema/policy version
[ ] Artifact yang sama telah melalui controlled pipeline dan QA; tidak ada rebuild/patch manual setelah QA evidence
[ ] RC telah dideploy ke controlled UAT logical lane
[ ] deployment/configuration/migration/health smoke PASS
[ ] QA summary tersedia dan menyebut planned/executed/pass/fail/blocked/not-run/N/A
[ ] applicable BUILD, UNIT, INTEGRATION, SECURITY, dan REGRESSION gates PASS
[ ] PERFORMANCE gate PASS bila target telah disetujui dan berlaku pada scope
[ ] required RECOVERY, COMPATIBILITY, dan CROSS-PLATFORM evidence PASS
[ ] seluruh P0/P1 serta critical business/security test yang applicable PASS
[ ] tidak ada open BLOCKER atau CRITICAL sebelum UAT dimulai
[ ] known MAJOR/MINOR mempunyai scope, impact, workaround, owner, target, dan disposition
[ ] UAT scope serta exclusions disetujui untuk candidate
[ ] Requirement dan canonical Feature traceability current; tidak ada ID unknown/duplicate
[ ] UAT cases menunjuk business outcome dan expected result yang dapat diamati
[ ] environment manifest, platform/device/browser/capability scope, dan support path siap
[ ] UAT accounts aktif, least-privilege, dan permission/policy sesuai scenario
[ ] test data synthetic/sanitized approved, versioned, dan dapat di-reset
[ ] evidence location, access, redaction, retention placeholder, dan naming siap
[ ] security/performance/recovery limitations dijelaskan tanpa unsupported claim
[ ] draft user guidance/release notes tersedia bila tester memerlukannya
[ ] defect, redeploy, retest, rollback/reset, dan escalation flow dipahami
```

## 2.2 UAT Entry Package

| Item | Isi minimum |
|---|---|
| Release Candidate | Build/version, commit, pipeline run, digest, config/schema/policy/capability version |
| QA Summary | Gate outcome dan execution count per layer/priority |
| Critical-test evidence | Business critical, authorization, privacy, data integrity, recovery, platform evidence yang applicable |
| Known issues | Defect ID, severity, priority, scope, impact, workaround, owner, target, disposition |
| Scope & traceability | Requirement → Feature → technical tests → UAT ID |
| Environment | Lane ID, topology/limitations, smoke, platform/device matrix |
| Accounts | Scenario identity, permission/profile, validity; tanpa password pada package |
| Test data | Dataset ID/version/classification/provenance, reset/disposal rule |
| Risk evidence | Security, performance, recovery, compatibility, PoC limitation |
| Guidance | UAT instructions, draft user guidance, issue/retest/escalation path |

## 2.3 Entry blocking rules

UAT tidak boleh dimulai atau dilanjutkan sebagai formal run apabila:

- build/digest tidak dapat dibuktikan;
- environment berubah tanpa manifest baru;
- QA evidence wajib hilang atau invalid;
- test account menggunakan bypass authorization;
- test data belum diizinkan atau memuat secret/Production content yang tidak disanitasi;
- terdapat open `BLOCKER`/`CRITICAL` dari pre-UAT gate;
- business scope atau expected outcome belum diputuskan;
- evidence tidak dapat disimpan secara aman.

Exploratory review boleh dilakukan untuk membantu klarifikasi, tetapi harus diberi label **pre-UAT/exploratory**, bukan formal PASS.

---

# 3. Exit criteria

## 3.1 Kriteria keluar minimum

```text
[ ] seluruh business-critical dan Must/Core scenarios dalam approved scope telah dieksekusi
[ ] seluruh applicable business-critical scenarios PASS
[ ] tidak ada BLOCKER terbuka
[ ] CRITICAL ditangani sesuai Bagian 3.2 dan Bagian 8
[ ] known MAJOR/MINOR terdokumentasi, dinilai, dan mempunyai decision/disposition
[ ] failed/retested cases mempertahankan hasil awal dan evidence fix
[ ] exact retest serta required affected regression PASS untuk defect yang dinyatakan VERIFIED
[ ] Requirement → Feature → UAT → Result/Evidence/Defect traceability lengkap
[ ] evidence manifest lengkap, aman, accessible kepada pihak yang berwenang, dan tidak ditimpa
[ ] planned/executed/passed/failed/blocked/N/A counts rekonsiliasi
[ ] platform/capability exclusions dan limitations dinyatakan eksplisit
[ ] UAT summary dan final recommendation selesai
[ ] decision authority, decision timestamp, scope, build, known issues, dan sign-off status tercatat
[ ] handoff findings untuk Documents 14–16 diklasifikasikan tanpa menulis dokumen tersebut
```

## 3.2 Aturan defect pada exit

| Kondisi | Exit behavior |
|---|---|
| Open `BLOCKER` | `REJECTED`; tidak dapat dikecualikan |
| Open `CRITICAL` | Default `REJECTED`; lihat exception sempit di bawah |
| Open `MAJOR` | Hanya dapat `ACCEPTED_WITH_KNOWN_ISSUES` bila risiko, workaround, owner, target, dan explicit decision lengkap |
| Open `MINOR` | Dapat diterima sebagai known issue bila tidak misleading dan tidak menutupi dampak lebih tinggi |
| Required case `FAIL` tanpa disposition | `REJECTED` |
| Required case `BLOCKED`/`NOT_RUN` | Tidak dapat dihitung PASS; hasil akhir `REJECTED` atau keputusan scope ulang yang terkendali |
| Traceability/evidence tidak lengkap | Sign-off belum dapat direkam |

Exception untuk `CRITICAL` bukan jalur normal. Exception hanya dapat dipertimbangkan apabila semuanya benar:

1. bukan security Critical;
2. bukan data loss/corruption;
3. bukan unauthorized access/disclosure, secret/content leakage, destructive/false success, atau critical Audit-integrity failure;
4. terdapat explicit business acceptance oleh authority yang sah;
5. scope terdampak, alasan, workaround/compensating control, owner, target resolution, expiry/review, dan acceptance reference tercatat;
6. hasil hanya `ACCEPTED_WITH_KNOWN_ISSUES`, bukan `ACCEPTED`;
7. keputusan UAT tidak diklaim sebagai Production risk approval.

Document 12 mengunci no open `BLOCKER/CRITICAL` pada UAT entry dan test completion. Karena prompt Document 13 membuka exception bisnis yang sempit untuk temuan `CRITICAL` saat UAT, penggunaan exception tersebut harus dilaporkan ke Integrator dan memperoleh keputusan governance sebelum diterapkan. Severity tidak boleh diturunkan hanya agar exception terlihat memenuhi gate.

## 3.3 Exit bukan release approval

UAT exit membuktikan penerimaan bisnis atas scope/build tertentu. Production tetap memerlukan seluruh release, security, performance, recovery, deployment, monitoring, documentation, dan approval gate yang berlaku. Build baru setelah sign-off memerlukan impact analysis, retest, dan sign-off record baru atau addendum yang jelas.

---

# 4. Environment readiness

## 4.1 Boundary environment

Baseline lingkungan tetap Development, QA/Testing, dan Production. UAT diperlakukan sebagai **logical lane** yang terisolasi sampai topology fisik disetujui.

| Area | Readiness minimum | Bukti |
|---|---|---|
| Artifact | Immutable RC dari controlled pipeline; digest sesuai QA | Pipeline run + digest |
| Deployment | Automated/controlled deployment; no manual binary patch | Deployment record |
| Configuration | Environment-specific; versioned; secret diinjeksikan aman | Sanitized config manifest |
| Schema/migration | Compatible dan smoke PASS | Migration/deployment result |
| Identity | Approved non-Production test identities | Account reference; no credential |
| Permission | Least privilege sesuai role/profile/scenario Document 09 | Permission fixture/version |
| Data | Synthetic/sanitized approved dan resettable | Dataset manifest |
| Platform | In-scope OS/device/browser/app/capability tercatat | Platform matrix |
| Integration | Required API/DB/storage/Worker/SignalR/provider sehat | Health/smoke reference |
| Observability | Safe log, Correlation ID, Job ID, alert/support path tersedia | Readiness result |
| Recovery | Reset/redeploy/restore point dan stop condition dipahami | Recovery instruction reference |
| Access | Tester hanya mengakses scope UAT; restricted evidence terpisah | Access owner/reference |

## 4.2 Environment manifest

```text
UAT lane/environment ID
build/version/commit/pipeline run/artifact digest
deployment timestamp and deploy record
configuration/policy/schema/capability versions
OS/device/browser/runtime/engine/provider versions
API/DB/storage/Worker/SignalR topology and limitations
identity/permission fixture version
Dataset IDs and classification
instrumentation configuration
test window and owner
reset/redeploy/recovery procedure reference
known environment limitation
```

## 4.3 Larangan di UAT

- load/DoS/stress yang tidak dibatasi;
- penetration test atau malicious corpus tanpa environment khusus dan authorization;
- debug langsung, direct database editing, atau production bypass;
- raw Production database/document/backup/log dump;
- public AI endpoint;
- shared credential atau secret pada case/evidence;
- manual patch yang membuat digest berbeda dari QA;
- test yang memengaruhi Production atau user nyata.

Pre-UAT smoke tidak menggantikan UAT case. Smoke hanya memastikan environment layak digunakan.

---

# 5. Test-data readiness

## 5.1 Urutan preferensi data

1. synthetic/generated deterministic;
2. sanitized representative yang telah disetujui;
3. controlled real-like sample hanya ketika synthetic tidak cukup dan governance mengizinkan.

Production-derived data tidak digunakan secara default. Jika benar-benar diperlukan, metode sanitization, approval authority, verification, access, retention, dan disposal harus terdokumentasi sebelum penggunaan.

## 5.2 Dataset manifest minimum

```text
Dataset ID/version
purpose and business scenario
Requirement/Feature/UAT IDs
source/generator/provenance/license
classification/sensitivity
format/count/size/digest
expected property/result
allowed environment and users
owner/access
created/reviewed/expiry
reset/cleanup/retention/disposal
known limitation
```

## 5.3 Kesiapan per kelas data

| Kelas | Penggunaan UAT | Kontrol |
|---|---|---|
| Synthetic business documents | Default flow bisnis | Tidak memakai identitas/data Pertamina nyata |
| Generated PDF/image | Page/order/layout/size behavior | Generator/seed/version/digest |
| Sanitized representative | Fidelity bisnis yang tidak cukup sintetis | Approval, sanitization verification, restricted access |
| Boundary data | Limit, empty, maximum allowed, unsupported input | Mengikuti configuration aktif; cleanup terkontrol |
| Golden/reference output | Verifikasi hasil tertentu | Reviewed property baseline; update terkendali |
| Visual signature test asset | Sign PDF visual non-PKI | Synthetic mark; bukan signature nyata; tidak auto-sync |
| Issue screenshot | Uji support flow bila scope | Explicit preview/send, secure storage, access controlled |

## 5.4 Data yang dilarang pada case/evidence biasa

- password;
- bearer/access/refresh token, cookie, secret, key, connection string;
- raw document content tanpa kebutuhan dan approval;
- OCR/translation/Markdown full text yang sensitif;
- real signature/paraf asset;
- clipboard content;
- unrestricted Assistant prompt/provider request/response;
- raw Production log/dump/database/export/backup;
- nama/path/storage key atau identity detail yang tidak diperlukan.

## 5.5 Reset dan cleanup

Setiap formal run memiliki initial state yang dapat diulang. Cleanup harus idempotent dan tidak menghapus failure evidence. Data yang gagal dibersihkan dicatat sebagai environment issue; test berikutnya tidak boleh diam-diam menggunakan state tercemar.

---

# 6. Execution workflow

## 6.1 Identitas UAT case

Gunakan pola:

```text
UAT-<AREA>-NNN
```

Contoh: `UAT-MRG-001` untuk Merge PDF. UAT ID bukan Feature ID baru. Setiap case tetap menunjuk canonical Feature ID yang sudah ada, misalnya `FEAT-PDF-005`.

## 6.2 Template UAT case

```text
UAT ID dan title
Release/scope
Requirement ID(s)
Canonical Feature ID(s) atau governed resource
Business objective/outcome
Risk/criticality
Actor/persona dan required permission
Platform/capability
Build/version/digest
Environment/config version
Dataset ID/version/classification
Preconditions
Steps
Expected business result
Expected UX/result communication
Forbidden side effect/security/privacy assertion
Evidence required
Result: PASS / FAIL / BLOCKED / NOT_RUN / INCONCLUSIVE / N/A
Defect ID bila ada
Executor/reviewer/timestamp
Notes/limitation
```

## 6.3 Status hasil case

| Status | Arti |
|---|---|
| `PASS` | Semua expected business result dan forbidden-side-effect assertion terpenuhi |
| `FAIL` | Minimal satu expected result tidak terpenuhi |
| `BLOCKED` | Case tidak dapat dieksekusi karena blocker yang diketahui |
| `NOT_RUN` | Direncanakan tetapi belum dijalankan; alasan wajib |
| `INCONCLUSIVE` | Environment/data/evidence tidak valid untuk keputusan |
| `N/A` | Terbukti tidak berlaku pada approved scope; alasan wajib |

Diagnostic rerun tidak menghapus hasil pertama. Hanya formal retest pada fix build yang dapat mengubah disposition defect.

## 6.4 Alur eksekusi lean

```text
1. Freeze UAT scope, RC, environment manifest, account, dan dataset
2. Review entry package dan known issues secara singkat
3. Jalankan pre-UAT smoke
4. Jalankan UAT case berdasarkan business priority/risk
5. Catat result segera dan tautkan evidence minimum
6. Jika FAIL, buat/tautkan satu defect; jangan membuat duplicate
7. Triage severity + priority + owner + target
8. Deploy fix sebagai RC baru melalui pipeline
9. Exact retest + required affected regression
10. Rekonsiliasi counts, coverage, defect, evidence, dan handoff findings
11. Buat UAT summary dan final recommendation
12. Decision authority mencatat sign-off outcome
```

Briefing dan daily/status update dapat berupa satu ringkasan singkat. Meeting tambahan hanya dibuat jika ada blocker, conflicting requirement, high-risk defect, atau keputusan acceptance yang benar-benar diperlukan.

## 6.5 Stop protocol

Eksekusi scope terkait dihentikan dan dilaporkan jika:

- requirement/expected result saling bertentangan;
- Feature ID tidak ditemukan/duplicate;
- build/digest/environment tidak cocok;
- tester tidak memiliki permission yang benar;
- data/evidence berisiko bocor;
- output berpotensi merusak source/data;
- hasil tidak dapat dibedakan antara product defect dan environment failure.

Catat source, observed behavior, impact, affected UAT IDs, smallest decision options, dan decision owner. Jangan memilih business rule sendiri.

---

# 7. Defect workflow

## 7.1 Record minimum

```text
Defect ID
UAT ID
Title/Description
Requirement ID and canonical Feature ID
Build/version/commit/digest
Environment/config/platform
Dataset ID/classification
Expected
Actual
Safe reproduction
Evidence reference
Severity
Defect Priority: DEF-P0..DEF-P3
Status
Owner
Fix Version
Retest Result
Regression Test ID/result bila berlaku
Issue ID bila ditautkan ke Support workflow
Disposition/known-issue decision reference
```

## 7.2 Workflow UAT yang diminta

```text
FOUND
↓
CONFIRMED
↓
IN PROGRESS
↓
READY FOR RETEST
↓
VERIFIED
↓
CLOSED
```

Terminal/non-bug disposition:

```text
NOT A BUG
DUPLICATE
```

Semantik:

| Status | Exit condition |
|---|---|
| `FOUND` | Observasi awal direkam dengan UAT ID, expected/actual, build, dan evidence minimum |
| `CONFIRMED` | Dapat direproduksi atau divalidasi sebagai ketidaksesuaian; severity/priority/owner ditetapkan |
| `IN PROGRESS` | Analisis/perbaikan aktif; target build diketahui |
| `READY FOR RETEST` | Fix tersedia pada immutable pipeline build dan reproduction steps siap |
| `VERIFIED` | Exact retest serta required affected regression PASS |
| `CLOSED` | Verification lengkap, record/evidence/traceability/known-issue handling selesai |
| `NOT A BUG` | Perilaku sesuai requirement/design yang berwenang; alasan dan source reference wajib |
| `DUPLICATE` | Menunjuk primary Defect ID; evidence unik dipertahankan pada relasi |

Jika retest gagal, defect kembali ke `CONFIRMED` atau `IN PROGRESS`; jangan membuat Defect ID baru untuk kegagalan yang sama. Jika defect muncul kembali setelah `CLOSED`, reopen record atau buat regression-linked defect sesuai policy tool yang disetujui; riwayat lama tidak dihapus.

## 7.3 Rekonsiliasi dengan Document 12

Document 12 FINAL mengunci alur `NEW → CONFIRMED → IN_PROGRESS → READY_FOR_RETEST → VERIFIED → CLOSED` serta disposition `AS_DESIGNED` dan `DUPLICATE`. Prompt Document 13 meminta `FOUND` dan `NOT A BUG`.

Rekomendasi tanpa mengubah master sebelumnya:

| Label UAT | Istilah Document 12 | Perlakuan sementara |
|---|---|---|
| `FOUND` | `NEW` | Label/projection UAT; semantik intake yang sama |
| `IN PROGRESS` | `IN_PROGRESS` | Perbedaan tampilan/spasi saja |
| `READY FOR RETEST` | `READY_FOR_RETEST` | Perbedaan tampilan/spasi saja |
| `NOT A BUG` | `AS_DESIGNED` | Label UAT dengan alasan/source; stored disposition menunggu keputusan Integrator |
| `DUPLICATE` | `DUPLICATE` | Konsisten |

Integrator harus memilih salah satu sebelum enum/workflow diimplementasikan:

1. mempertahankan status canonical Document 12 dan menggunakan label UI UAT;
2. membuat controlled terminology addendum yang memetakan kedua vocabulary.

Laporan ini tidak membuat enum, endpoint, database field, atau Feature ID baru.

## 7.4 UAT defect vs In-App Issue

- UAT defect adalah release-validation record dan menjadi sumber sign-off/coverage.
- In-App Issue adalah product support workflow `FEAT-SUPPORT-002..008`.
- Jika tester memakai **Laporkan Masalah**, tautkan `Issue ID` ke satu Defect ID setelah triage; jangan menggandakan defect untuk laporan yang sama.
- Screenshot/pesan Issue tetap di plane Support; tidak disalin penuh ke defect, Audit, log, atau UAT summary.
- Support status dan UAT defect status tidak dianggap enum yang sama.

---

# 8. Severity handling

## 8.1 Severity canonical

| Severity | Definisi konsisten dengan Document 12 | UAT consequence default |
|---|---|---|
| `BLOCKER` | Required UAT/primary product tidak dapat dilanjutkan; environment/build unusable; tidak ada safe workaround | Stop affected run; `REJECTED`; fix/redeploy/retest wajib |
| `CRITICAL` | Unauthorized access, secret/content leak, source/data corruption, destructive/false success, atau critical Audit/business/security failure | Stop affected scope; default `REJECTED`; exception bisnis sangat sempit mengikuti Bagian 3.2 |
| `MAJOR` | Important requirement/Core path materially wrong/unavailable dengan limited workaround | Fix sebelum sign-off atau explicit known-issue decision |
| `MINOR` | Limited noncritical/cosmetic/text/report issue dengan safe workaround dan tanpa misleading outcome | Dapat menjadi known issue dengan owner/target |

Severity mengukur dampak. Urutan perbaikan tetap memakai:

| Priority | Arti |
|---|---|
| `DEF-P0` | Investigate/fix segera; memblokir validation/release saat ini |
| `DEF-P1` | Harus selesai untuk target release kecuali governed disposition |
| `DEF-P2` | Direncanakan setelah risiko lebih tinggi |
| `DEF-P3` | Low urgency backlog/value review |

Severity dan priority tidak boleh disamakan. Security scanner/VA-PT severity tetap authoritative dan tidak boleh diturunkan untuk memenuhi jadwal.

## 8.2 Triage rule

Triage minimum menjawab:

1. outcome bisnis apa yang gagal;
2. scope pengguna/platform/data yang terdampak;
3. apakah ada security/privacy/data/source/destructive impact;
4. apakah workaround aman, sederhana, dan tidak misleading;
5. apakah defect dapat direproduksi pada exact RC/environment/data;
6. siapa owner dan target fix;
7. test/retest/regression apa yang wajib;
8. apakah UAT harus pause, lanjut pada scope lain, atau dihentikan.

Developer dapat mengusulkan severity, tetapi acceptance authority/reviewer terkait harus dapat meninjau dampaknya. Codex tidak boleh menurunkan severity atau memilih acceptance.

## 8.3 Absolute no-hide rule

`ACCEPTED_WITH_KNOWN_ISSUES` tidak boleh menyembunyikan:

- `BLOCKER`;
- critical security issue;
- critical data-loss/corruption issue;
- unauthorized access/disclosure;
- password/token/secret/content leakage;
- destructive/false-success behavior;
- evidence atau Audit integrity failure yang membuat keputusan tidak dapat dipercaya.

---

# 9. Retest

## 9.1 Retest package

```text
Defect ID dan linked UAT ID
original failure build/environment/data/evidence
fix build/version/commit/digest
environment/config/Dataset versions
exact reproduction steps
expected result and forbidden side effects
retest executor/reviewer/time
PASS / FAIL / BLOCKED
safe evidence
required affected regression result
```

## 9.2 Rules

1. Fix harus masuk ke pipeline artifact baru; tidak ada patch langsung pada lane.
2. Exact original reproduction dijalankan terlebih dahulu.
3. Forbidden side effects diperiksa: no unauthorized upload/disclosure, no source overwrite, no corrupt/partial final result, no audit/privacy regression.
4. Affected component/shared-contract/security/platform/entry-path regression dijalankan sesuai blast radius.
5. Repeatable defect mendapat permanent automated regression pada layer terendah yang benar.
6. Retest PASS tanpa required regression tidak cukup untuk `VERIFIED`.
7. Full regression PASS tanpa exact reproduction tidak cukup untuk `VERIFIED`.
8. Hasil gagal awal tetap dipertahankan; evidence fix ditambahkan, bukan menimpa.
9. Perubahan requirement/expected behavior memerlukan controlled decision; bukan sekadar mengubah expected result agar test hijau.

Pada model satu developer, tester/stakeholder awal sebaiknya menyaksikan retest untuk `BLOCKER`, `CRITICAL`, dan business-critical `MAJOR` bila feasible. Untuk defect deterministik berisiko rendah, automated exact regression + evidence yang dapat ditinjau dapat mengurangi administrasi.

---

# 10. Evidence rules

## 10.1 Evidence yang dapat digunakan

| Evidence | Penggunaan | Aturan minimum |
|---|---|---|
| Screenshot | UI state, visual output, error/flow | Capture minimum, preview, sanitize, access controlled |
| Test Result | Automated/manual result | UAT ID, build, environment, result, timestamp |
| Safe Log Reference | Menunjuk window diagnostik aman | Reference/window, bukan raw dump |
| Correlation ID | Menghubungkan request/process/evidence | Bukan credential atau authorization proof |
| Job ID | Menghubungkan background job | Hanya bila job ada; bukan access grant |
| Exported result sample | Menilai business output/fidelity | Synthetic/sanitized, restricted bila perlu, digest |
| Video | Temporal/gesture/reconnect defect yang sulit dibuktikan dengan cara lain | Hanya jika benar-benar diperlukan; trim/sanitize/restrict |
| QA/security/performance report | Prasyarat dan risk evidence | Tautkan report yang disetujui; jangan salin payload sensitif |

## 10.2 Evidence manifest

```text
Evidence ID/type
UAT/Defect/Requirement/Feature links
build/version/commit/artifact digest/pipeline run
environment/config/platform/tool version
Dataset ID/classification only
executor/reviewer/timestamp
result/count
artifact location/digest
access owner/classification
retention/expiry/disposal status
redaction/sanitization status
known limitation
```

## 10.3 Sensitive-data exclusion

Evidence tidak boleh menyimpan atau menampilkan:

- password;
- access/refresh token, cookie, secret, key, connection string;
- unrestricted document bytes/content;
- OCR, translation, Markdown, atau AI prompt/response lengkap;
- real signature/paraf payload;
- clipboard content;
- raw SQL, stack, request/response body, claim dump, storage key/SAS URL;
- screenshot/lampiran Issue yang tidak dikirim eksplisit atau tidak dibutuhkan;
- nama/path/identity detail yang tidak diperlukan untuk keputusan.

Jika evidence yang diperlukan mengandung restricted content:

1. minimalkan/crop/redact;
2. gunakan secure restricted storage yang disetujui;
3. batasi akses pada exact review purpose;
4. catat classification, owner, expiry, disposal;
5. jangan commit large/restricted evidence ke source repository secara default.

## 10.4 Screenshot dan video

- Capture hanya area aplikasi yang relevan.
- Preview dilakukan sebelum disimpan/dikirim.
- Notification, taskbar, browser tab, username, path, dan document preview di luar scope disembunyikan bila perlu.
- Markup tidak boleh menutupi fakta penting atau memalsukan hasil.
- Original dan redacted derivative, bila keduanya perlu, memiliki access level berbeda dan digest/reference yang jelas.
- Video hanya dipakai untuk timing, gesture, animation, reconnect, crash/recovery, atau defect intermittent yang tidak cukup dibuktikan screenshot/result.

## 10.5 Integrity, storage, retention, dan access

- Evidence tidak ditimpa diam-diam; versi/addendum harus dapat ditelusuri.
- Link tidak membawa bearer token, SAS/secret, atau unrestricted direct object access.
- Access mengikuti least privilege dan purpose; Support, Administrator, atau Auditor tidak otomatis memperoleh document content.
- UAT evidence bukan Enterprise Audit, User History, Observability, atau Issue/Support payload. Plane tersebut tetap terpisah.
- Correlation ID dapat menghubungkan plane, tetapi tidak memberikan akses atau izin menyalin content.
- Exact retention duration, legal hold, official classification, approved storage, disposal, dan sign-off record retention adalah `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

## 10.6 Evidence completeness rule

Evidence harus cukup untuk menjawab:

```text
Apa yang diuji?
Build/environment/data yang mana?
Siapa yang menjalankan/meninjau?
Apa expected result-nya?
Apa hasil aktualnya?
Adakah defect/known limitation?
Dapatkah keputusan ditelusuri ulang tanpa membuka data yang tidak perlu?
```

---

# 11. UAT summary format

## 11.1 Header

```text
UAT Summary ID
Product: PERTAMINA OTO
Release/Scope
Build Version
Commit/Pipeline Run/Artifact Digest
Environment/Lane
Configuration/Policy/Schema/Capability Version
Period
Prepared By
Reviewed By
Decision Authority: TBC until assigned
```

## 11.2 Execution summary

| Metric | Nilai |
|---|---:|
| Cases Planned | |
| Cases Executed | |
| Passed | |
| Failed | |
| Blocked | |
| Not Run | |
| Inconclusive | |
| Not Applicable | |

Count harus rekonsiliasi:

```text
Planned = Passed + Failed + Blocked + Not Run + Inconclusive + Not Applicable
Executed = Passed + Failed
```

Jika organisasi memilih definisi `Executed` yang memasukkan `BLOCKED`/`INCONCLUSIVE`, definisi harus dicatat dan digunakan konsisten; jangan membandingkan persentase dari denominator berbeda.

## 11.3 Defect summary

| Severity | Open | Ready for Retest | Verified/Closed | Accepted Known Issue |
|---|---:|---:|---:|---:|
| BLOCKER | | | | |
| CRITICAL | | | | |
| MAJOR | | | | |
| MINOR | | | | |

Report juga mencantumkan `DEF-P0..DEF-P3` secara terpisah bila perlu.

## 11.4 Coverage dan risk summary

```text
Core Coverage:
- in-scope Primary PDF Tool Cards
- planned/executed/passed/failed/blocked/dispositioned
- Feature/platform/capability limitations

Requirement Coverage:
- in-scope Must/Core Requirement count
- mapped to UAT
- executed with PASS evidence
- failed/blocked/not applicable with reason

Known Risks:
- Issue/Defect
- impact/scope
- workaround/compensating control
- owner/target
- decision reference

Security/Performance/Recovery/Compatibility Evidence:
- applicable gate result
- report reference
- approved limitation/target status
```

## 11.5 Final recommendation

Summary minimum memuat:

```text
Final Recommendation:
- ACCEPTED
- ACCEPTED_WITH_KNOWN_ISSUES
- REJECTED

Recommendation rationale
Unmet criteria
Known-issue decision references
Sign-Off Status: PENDING / RECORDED
```

Tidak ada target pass-rate numerik yang dibuat oleh laporan ini. Business-critical coverage dan mandatory outcomes lebih penting daripada aggregate pass percentage.

---

# 12. Known-issue handling

## 12.1 Decision log minimum

```text
Decision Record ID
Issue/Defect ID
UAT ID(s)
Severity and DEF priority
Affected Requirement/Feature/platform/scope
Risk/impact
Reason for acceptance
Safe workaround or compensating control
Owner
Target resolution/fix version
Expiry/review date if applicable
Acceptance authority/function
Acceptance timestamp
Acceptance reference
Release/UAT scope limitation
Status/follow-up
```

## 12.2 Acceptance rules

1. Known issue harus dijelaskan kepada decision authority sebelum sign-off.
2. `ACCEPTED_RISK`/known-issue disposition bukan `PASS`, bukan `VERIFIED`, dan bukan `CLOSED` karena fix.
3. Defect tetap dapat dicari dan mempunyai owner/target; sign-off hanya mencatat risiko yang diterima untuk scope/build tertentu.
4. Build/scope/platform berbeda memerlukan review ulang; acceptance tidak diwariskan otomatis.
5. Workaround harus aman, dapat dipahami user, dan tidak membuat output salah tampak berhasil.
6. Security exception mengikuti Security Design, authority, expiry, compensating control, dan retest; business stakeholder tidak sendirian menerima security risk.
7. `BLOCKER` tidak dapat diterima sebagai known issue.
8. Critical security/data-loss/corruption/disclosure/destructive/false-success tidak dapat disembunyikan dengan status ini.
9. Target resolution yang terlewat memicu review; issue tidak hilang dari report.

## 12.3 Major dan Minor

- `MAJOR` memerlukan explicit decision, documented workaround, affected scope, owner, dan target.
- `MINOR` dapat diterima secara batch hanya jika setiap Defect ID tetap tercatat dan tidak ada dampak tersembunyi lebih tinggi.
- Banyak Minor pada satu workflow dapat secara agregat menjadi usability/business `MAJOR`; triage harus menilai cumulative impact.

## 12.4 Critical business exception

Jika exception sempit Bagian 3.2 dipertimbangkan, decision log harus menyatakan bahwa:

- severity tetap `CRITICAL` dan tidak diturunkan karena jadwal;
- issue bukan security/data integrity/disclosure/destructive class yang dilarang;
- business acceptance eksplisit tersedia;
- Production release governance masih dapat menolak candidate;
- target fix dan expiry/review wajib;
- Integrator/governance owner menyetujui penggunaan exception karena Document 12 memakai no-open-Critical gate.

---

# 13. Sign-off model

## 13.1 Hasil UAT

| Result | Arti | Kondisi minimum |
|---|---|---|
| `ACCEPTED` | Scope/build diterima tanpa known issue material | Exit criteria lengkap; no Blocker/Critical; required outcome PASS; evidence/traceability lengkap |
| `ACCEPTED_WITH_KNOWN_ISSUES` | Scope/build diterima dengan issue yang dinyatakan jelas | No Blocker; no prohibited Critical; decision log lengkap; owner/target/workaround/authority tercatat |
| `REJECTED` | Scope/build tidak dapat diterima | Blocker, prohibited/unaccepted Critical, failed critical outcome, insufficient evidence, invalid environment/build, atau sign-off criteria tidak terpenuhi |

`ACCEPTED_WITH_KNOWN_ISSUES` tidak boleh menjadi mekanisme untuk mengejar jadwal dengan menyamarkan failure.

## 13.2 Sign-off record

```text
Sign-Off Record ID
Product/release/scope
Build/version/commit/pipeline run/artifact digest
Environment/config/schema/policy/capability version
UAT period
Summary and traceability reference
Cases planned/executed/passed/failed/blocked/N/A
Defects by severity
Known-issue decision references
Known risks and exclusions
Final UAT Result
Decision rationale
Decision authority name/function: populated only when officially assigned
Decision timestamp
Approval/evidence reference
Conditions/expiry/follow-up
Production approval status: SEPARATE / NOT IMPLIED
```

## 13.3 Authority and authenticity

- Laporan ini tidak menentukan nama approver atau mengklaim sign-off Pertamina.
- Business/UAT Decision Authority, delegation, quorum/reviewer need, acceptable electronic record, evidence repository, retention, dan formal approval mechanism adalah `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.
- Conceptual record dapat menggunakan controlled repository/work item/approval mechanism yang disetujui, dengan identity, timestamp, immutable reference, dan build digest.
- Approval oleh Codex, automation identity, shared account, atau developer tanpa delegated business authority tidak sah sebagai business sign-off.
- Product capability **Sign PDF** tetap visual non-PKI dan tidak dipakai sebagai asumsi mekanisme legal/PKI untuk UAT sign-off.

## 13.4 Perubahan setelah sign-off

Perubahan pada code, dependency, artifact, configuration, schema, policy, engine/provider, platform scope, data contract, atau critical documentation setelah sign-off memerlukan:

```text
impact analysis
→ affected technical regression
→ affected UAT retest/rerun
→ updated summary
→ new sign-off or controlled sign-off addendum
```

Build berbeda tidak mewarisi sign-off hanya karena nomor release serupa.

## 13.5 Hubungan dengan Production

UAT result adalah input release decision. Ia tidak mengesahkan:

- Production deployment;
- security exception;
- performance SLA;
- data-classification exception;
- rollout lintas Region;
- change request di luar approved scope.

Approval tersebut tetap mengikuti authority dan gate yang berwenang.

---

# 14. Handoff to Docs 14–16

## 14.1 Klasifikasi temuan

| Temuan UAT | Handoff target | Contoh input | Yang tidak dilakukan di Document 13 |
|---|---|---|---|
| Cara menggunakan aplikasi | Document 14 — User Manual | Langkah yang membingungkan, label/menu, file input/output, error guidance, accessibility, platform-specific usage | Tidak menulis manual final |
| Cara administrasi | Document 15 — Admin Manual | Account/permission/config/feature flag/limit/monitoring/Support Console procedure | Tidak mengubah Role Matrix atau membuat admin authority baru |
| Support, incident, recovery | Document 16 — Incident Support Guide | Error Code, Job/Correlation lookup, retry/recovery, escalation, evidence preservation, known operational failure | Tidak menulis incident runbook lengkap |

## 14.2 Handoff record

```text
Handoff ID
Source UAT/Defect/Decision ID
Target Document 14 / 15 / 16
Observed gap
Affected Requirement/Feature/platform
User/admin/support impact
Required content/change
Evidence reference
Owner: TBC/assigned
Priority/target release
Status
```

## 14.3 Rules

- Temuan usage bukan otomatis product defect; dapat menjadi documentation gap, UX defect, training need, atau requirement ambiguity.
- Jika aplikasi perlu diubah, tetap buat/tautkan Defect ID; documentation handoff tidak menggantikan fix.
- Support/recovery finding yang menyentuh incident operations menjadi input Document 16, tetapi Document 13 hanya menyimpan ringkasan dan evidence reference.
- Screenshot, Issue message, safe diagnostics, Audit, dan logs tetap pada storage/permission plane masing-masing.
- Handoff tidak membuat Feature ID atau Requirement ID baru. Gap ID baru hanya dibuat melalui governance dokumen yang berwenang.

---

# 15. Risks/gaps

| ID | Risiko/gap | Dampak | Rekomendasi/owner handoff |
|---|---|---|---|
| `D13-D-001` | Document 12 memakai `NEW`; prompt UAT memakai `FOUND` | Enum/reporting dapat terpecah | Integrator tetapkan canonical storage + UAT display mapping sebelum implementation |
| `D13-D-002` | Document 12 memakai `AS_DESIGNED`; prompt memakai `NOT A BUG` | Disposition duplicate/metrics tidak konsisten | Map secara eksplisit atau buat controlled terminology addendum |
| `D13-D-003` | Document 12 no-open-`CRITICAL`; prompt membuka explicit business exception | UAT acceptance dapat bertentangan dengan release gate | Exception sempit Bagian 3.2 wajib governance decision; Production approval tetap terpisah |
| `D13-D-004` | Business/UAT Decision Authority dan approver belum bernama | Sign-off tidak mempunyai authority | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`; jangan isi nama palsu |
| `D13-D-005` | UAT physical topology belum dikunci | Evidence dapat tercemar oleh shared mutable QA | Pertahankan logical isolation; konfirmasikan dedicated vs isolated QA lane |
| `D13-D-006` | Satu developer merangkap implementation, QA coordination, dan triage | Self-confirming bias dan bottleneck | Stakeholder human menilai outcome/sign-off; immutable evidence; witnessed high-risk retest |
| `D13-D-007` | Codex dapat menghasilkan ringkasan meyakinkan tanpa execution nyata | False evidence/sign-off | Semua claim menunjuk run/evidence; Codex bukan executor/approver kecuali automation output yang diverifikasi |
| `D13-D-008` | Exact stakeholder/tester availability belum diketahui | Critical business flow tidak terwakili | Freeze participant/scope sebelum entry; record unavailable representation sebagai gap |
| `D13-D-009` | Evidence classification, storage, retention, disposal, dan legal hold belum final | Kebocoran atau penghapusan dini | Minimize/restrict; `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `D13-D-010` | Production-derived data mungkin diminta untuk fidelity | Data sensitif masuk UAT/evidence | Synthetic-first; approved sanitization + verification + restricted access |
| `D13-D-011` | UAT defect dan In-App Issue dapat diduplikasi | Count/owner/status divergen | Satu Defect ID sebagai release record; link optional Issue ID |
| `D13-D-012` | Screenshot/video/output sample dapat memuat isi dokumen/identity/path | Evidence menjadi exfiltration channel | Preview, crop/redact, secure restricted storage, canary/privacy check |
| `D13-D-013` | Shared credentials atau test-only authorization bypass | Hasil authorization UAT tidak valid | Approved identities, least privilege, current permission/policy; no bypass |
| `D13-D-014` | RC diubah/rebuild setelah QA/UAT evidence | Evidence tidak lagi mewakili artifact | Digest gate; new build memicu impact analysis/retest/sign-off |
| `D13-D-015` | Performance/security targets atau evidence belum disetujui | Business sign-off memberi false technical assurance | Gate tetap `BLOCKED`/limitation; tidak mengarang threshold atau SLA |
| `D13-D-016` | Platform/device/browser inventory belum final | Supported claim tidak terbukti | Release capability matrix dan representative-device UAT plan |
| `D13-D-017` | Accepted known issue dianggap defect closed | Risiko hilang dari backlog | Acceptance reference tidak mengubah defect menjadi fixed; owner/target/expiry tetap aktif |
| `D13-D-018` | UAT accepted dianggap Production approved | Gate operasi/security/deployment terlewati | Tulis `Production approval: separate/not implied` pada summary/sign-off |
| `D13-D-019` | Major usability issues terpecah menjadi banyak Minor | Aggregate business impact diremehkan | Triage cumulative impact per workflow/outcome |
| `D13-D-020` | Manual UAT evidence berlebihan untuk satu developer | Proses menjadi birokratis dan lambat | Evidence minimum per risk; satu register/summary; automate repeatable setup/reconciliation |
| `D13-D-021` | Exact sign-off mechanism/electronic record belum ditentukan | Authenticity/retention dispute | Konfirmasi approved repository/identity/timestamp/approval record; no fake signature |
| `D13-D-022` | Draft guidance belum tersedia untuk business flow baru | Tester menilai berdasarkan penjelasan ad hoc | Entry package memuat concise instruction/release notes; gap diteruskan ke Doc 14/15 |

## 15.1 Inconsistency audit result

Temuan material untuk Integrator:

1. mapping terminology `FOUND ↔ NEW`;
2. mapping disposition `NOT A BUG ↔ AS_DESIGNED`;
3. reconciliation Critical business exception dengan no-open-Critical gate Document 12;
4. authority, topology, evidence policy, dan sign-off mechanism masih memerlukan konfirmasi Pertamina.

Tidak ditemukan ketidakcocokan canonical Feature ID pada rujukan yang digunakan. Laporan memakai `FEAT-PDF-005` dan `FEAT-SUPPORT-001..008` sesuai Feature Catalog; tidak ada Feature ID baru yang dibuat.

---

# 16. Suggested Document 13 text

Teks berikut disarankan untuk diintegrasikan oleh Integrator ke master Document 13 setelah terminology/status dan authority gaps pada Bagian 15 diputuskan.

## 16.1 Tujuan dan governance

> UAT PERTAMINA OTO memvalidasi bahwa release candidate memenuhi business outcome pada scope, platform, dan capability yang disetujui. UAT dimulai hanya setelah UAT Readiness Gate Document 12 lulus. UAT tidak menggantikan Unit, Integration, Security, Performance, Recovery, Compatibility, atau Release Validation. Technical green saja tidak cukup untuk business acceptance.
>
> Proses UAT menggunakan model lean: satu scope/traceability register, satu defect register, satu evidence manifest, satu UAT summary, dan satu sign-off record per release candidate. Model ini dapat dijalankan satu developer, tetapi keputusan business acceptance tetap diberikan manusia yang mempunyai authority. Codex dapat membantu persiapan, traceability, automation pendukung, dan summary; Codex tidak boleh memberikan sign-off atau menerima risiko.

## 16.2 Entry gate

> UAT berstatus `READY` hanya jika immutable release candidate telah dideploy melalui pipeline ke controlled UAT lane; deployment smoke dan seluruh applicable Build/Unit/Integration/Security/Regression gate lulus; required Performance/Recovery/Compatibility/Cross-platform evidence tersedia; tidak ada open Blocker/Critical; known Major/Minor terdokumentasi; scope dan Requirement/Feature traceability current; serta environment, account, permission, test data, evidence, guidance, defect, retest, dan support path siap. Kriteria yang belum terpenuhi menghasilkan `NOT_READY` atau `BLOCKED`, bukan PASS.

## 16.3 Environment dan data

> UAT menggunakan logical isolation atas artifact, configuration, data, identity, access, evidence, dan test window. Dedicated UAT deployment atau isolated QA lane ditentukan melalui konfirmasi topology. Artifact tidak dibangun ulang atau dipatch manual setelah QA evidence. Data synthetic/generated menjadi default; data representative yang disanitasi hanya digunakan setelah approval dan verifikasi. Password, token, secret, raw Production data, real signature, dan unrestricted document/OCR/Assistant content dilarang pada test package dan evidence biasa.

## 16.4 Execution dan result

> Setiap case menggunakan `UAT-<AREA>-NNN` dan mencatat Requirement ID, canonical Feature ID/resource, business outcome, actor/permission, platform/capability, build/digest, environment/configuration, Dataset ID, steps, expected result, forbidden side effect, evidence, executor, timestamp, dan result. Result adalah `PASS`, `FAIL`, `BLOCKED`, `NOT_RUN`, `INCONCLUSIVE`, atau `N/A`. Hanya PASS yang dihitung sebagai accepted execution outcome; diagnostic rerun tidak menghapus failure awal.

## 16.5 Defect

> Defect UAT minimal mencatat Defect ID, UAT ID, description, expected, actual, safe evidence, severity, status, Fix Version, dan Retest Result, ditambah Requirement/Feature, build/environment/data, owner, priority, serta regression reference bila berlaku. Workflow lean adalah `FOUND → CONFIRMED → IN PROGRESS → READY FOR RETEST → VERIFIED → CLOSED`, dengan disposition `NOT A BUG` dan `DUPLICATE`. Terminology ini harus dipetakan ke canonical workflow Document 12 sebelum implementation agar `FOUND/NEW` dan `NOT A BUG/AS_DESIGNED` tidak menjadi dua enum berbeda.

## 16.6 Severity dan retest

> Severity adalah `BLOCKER`, `CRITICAL`, `MAJOR`, dan `MINOR`; defect priority tetap terpisah sebagai `DEF-P0..DEF-P3`. Blocker menghentikan acceptance. Critical default-nya menolak candidate. Critical security, data-loss/corruption, unauthorized disclosure, secret/content leakage, destructive/false-success, dan evidence/Audit-integrity failure tidak dapat diterima sebagai known issue. Setiap fix menggunakan pipeline build baru, exact retest, dan affected regression; hasil failure awal tetap dipertahankan.

## 16.7 Evidence

> Evidence dapat berupa screenshot yang disanitasi, test result, safe log reference, Correlation ID, Job ID, exported synthetic/sanitized result sample, serta video hanya bila temporal behavior tidak dapat dibuktikan secara lebih ringan. Evidence tidak boleh memuat password, token, secret, unrestricted document/OCR/AI content, real signature, raw diagnostics, atau sensitive document tanpa kebutuhan. Evidence diberi manifest, digest/reference, access owner, classification, retention/disposal status, dan tidak ditimpa diam-diam.

## 16.8 Exit dan known issues

> Exit memerlukan seluruh business-critical scenarios PASS, no Blocker, Critical ditangani sesuai governance, Major/Minor mempunyai disposition, exact retest/regression selesai, traceability dan evidence lengkap, counts rekonsiliasi, UAT summary selesai, dan sign-off tercatat. Known-issue acceptance mencatat Issue, Risk, Reason, Owner, Target Resolution, Acceptance Reference, scope, workaround/compensating control, expiry/review, dan decision authority. Accepted known issue bukan PASS, VERIFIED, CLOSED, security exception, atau Production approval.

## 16.9 Summary dan sign-off

> UAT Summary minimum memuat Build Version, Environment, Period, Cases Planned, Executed, Passed, Failed, Blocked, Not Run, Inconclusive, Not Applicable, Defects by Severity, Core Coverage, Requirement Coverage, Known Risks, Final Recommendation, dan Sign-Off Status. Hasil UAT adalah `ACCEPTED`, `ACCEPTED_WITH_KNOWN_ISSUES`, atau `REJECTED`.
>
> Sign-off record mengikat result pada release/scope, build/version/commit/pipeline run/artifact digest, environment/configuration, period, summary/evidence, defect/known-issue references, authority, timestamp, conditions, dan follow-up. Nama approver, delegation, approval mechanism, dan evidence retention hanya diisi setelah dikonfirmasi resmi. UAT acceptance tidak menyiratkan Production approval.

## 16.10 Handoff

> UAT findings tentang cara menggunakan aplikasi diteruskan sebagai input Document 14 User Manual; temuan administrasi diteruskan ke Document 15 Admin Manual; temuan support, incident, recovery, evidence preservation, Job ID, Correlation ID, retry, dan escalation diteruskan ke Document 16 Incident Support Guide. Document 13 hanya mencatat handoff ID, source UAT/Defect, observed gap, target document, owner, evidence, dan status; dokumen 14–16 tidak ditulis di dalam UAT Document.

## 16.11 Acceptance checklist untuk integrasi

```text
[ ] governance lean dan fungsi tanggung jawab jelas tanpa membuat role aplikasi baru
[ ] UAT entry mempertahankan semua gate Document 12
[ ] exit membedakan PASS, known issue, dan rejected outcome
[ ] severity BLOCKER/CRITICAL/MAJOR/MINOR konsisten
[ ] defect priority DEF-P0..DEF-P3 tetap terpisah
[ ] FOUND/NEW dan NOT A BUG/AS_DESIGNED telah direkonsiliasi
[ ] Critical exception tidak menutupi security/data-loss/disclosure/destructive risk
[ ] evidence aman, traceable, access controlled, dan retention policy ditandai
[ ] summary memuat semua field minimum
[ ] sign-off human, build-bound, scope-bound, dan tidak dipalsukan
[ ] ACCEPTED_WITH_KNOWN_ISSUES bukan jalur menyembunyikan Blocker/Critical
[ ] UAT acceptance tidak diklaim sebagai Production approval
[ ] handoff Docs 14–16 tercatat tanpa menulis dokumen tersebut
[ ] tidak ada nama approver, produk, vendor, role, endpoint, atau Feature ID yang dibuat tanpa sumber
```

---

**Serah terima kepada Integrator:** pertahankan baseline Document 12; putuskan terminology mapping `FOUND/NEW` dan `NOT A BUG/AS_DESIGNED`; selaraskan exception bisnis `CRITICAL` dengan release gate; lalu konfirmasikan UAT authority, participant model, physical topology, evidence classification/storage/retention, approved sign-off mechanism, dan data sanitization standard. Jangan mencatat sign-off Pertamina sampai keputusan aktual diberikan oleh pihak yang berwenang.
