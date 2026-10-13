# 11 — DOKUMEN UNIT TEST
## PERTAMINA OTO — Office Tools Organizer

> **Standar bahasa:** Bahasa Indonesia — ID-1.0  
> **Versi kerja:** 0.1  
> **Tanggal:** 28 Agustus 2026  
> **Status:** WORKING — PRE-CODING BASELINE  
> **Keputusan Unit Test:** 20/20 AUTO-LOCKED  
> **Baseline fitur canonical:** 550  
> **Prioritas utama:** 33 Primary PDF Tool Cards  
> **Vertical slice pertama:** `FEAT-PDF-005 — Merge PDF`

---

# 1. Tujuan

Dokumen ini menjawab:

> Bagian kode kecil apa yang harus diuji, bagaimana mengujinya, dan kapan hasilnya dianggap lulus?

Dokumen ini menjadi baseline pre-coding untuk Unit Test yang:

- dapat langsung dipakai ketika coding dimulai;
- menguji perilaku bisnis, bukan sekadar menaikkan coverage;
- dapat ditelusuri ke Requirement ID dan Feature ID;
- deterministic, cepat, isolated, repeatable, dan mudah dipelihara satu developer;
- menguji negative path, error, recovery, security, Local-First, dan privacy;
- cocok untuk task Codex yang kecil dan terkontrol;
- tidak mengambil alih Test Plan, E2E, UAT, performance, PoC, atau VA/PT.

---

# 2. Ruang Lingkup

## 2.1 Termasuk

Unit Test mencakup perilaku pada:

```text
method
class
handler/application service
validator
policy evaluator
domain rule/value object/aggregate
state machine
mapper/reducer/presenter murni
resolver
Engine Adapter boundary
typed contract logic
safe error/audit mapping
```

## 2.2 Tidak Termasuk

Unit Test tidak membuktikan:

- browser/full UI flow atau native platform interaction;
- middleware/HTTP/OpenAPI/SignalR/Named Pipe wiring nyata;
- SQL Server, EF Core, SQLite, transaction, rowversion, atau database permission nyata;
- filesystem atomicity, ACL, reparse/symlink, full-disk race, atau crash residue nyata;
- vendor PDF/OCR/Office/Image/AI fidelity dan keamanan parser nyata;
- public/internal provider network behavior;
- physical scanner/camera/share intent;
- load, performance, soak, DoS, VA/PT, atau UAT.

Area tersebut diserahkan ke Dokumen 12, Dokumen 13, PoC, dan suite Integration/Contract/E2E/Security/Performance yang sesuai.

---

# 3. Sumber Berwenang dan Baseline

| Sumber | Status |
|---|---|
| Dokumen 01 Business Process | FINAL v3.4 |
| Dokumen 02 Requirement Specification | FINAL v1.5 |
| Dokumen 03 Feature Catalog | FINAL v1.3 — 550 fitur |
| Dokumen 04 Solution Design | FINAL v1.2 — 18/18 LOCKED |
| Dokumen 05 Technical Reference | FINAL v1.2 — 15/15 LOCKED |
| Dokumen 06 API Documentation | FINAL v1.0 — 9/9 LOCKED |
| Dokumen 07 Database Design | FINAL v1.0 — 12/12 AUTO-LOCKED |
| Dokumen 08 Security Design | FINAL v1.0 — 20/20 AUTO-LOCKED |
| Dokumen 09 Role Matrix | FINAL v1.0 — 18/18 AUTO-LOCKED |
| Dokumen 10 Audit Trail Design | FINAL v1.0 — 20/20 AUTO-LOCKED |
| Master Project Summary | Versi terbaru |

Laporan Agent A–D telah diintegrasikan melalui `UNIT_TEST_INTEGRATION_AUDIT.md`. Keputusan teknis dicatat dalam `UNIT_TEST_DECISION_REGISTER.md`.

---

# 4. Prinsip Unit Test

1. **Behavior-first.** Uji hasil yang terlihat dari public behavior/invariant.
2. **Satu tujuan utama.** Satu test menjelaskan satu alasan utama kegagalan.
3. **Positive dan negative.** Happy path tanpa negative path belum memadai.
4. **Deterministik.** Input dan context yang sama menghasilkan keputusan yang sama.
5. **Isolated.** Test tidak bergantung pada urutan atau mutable global state.
6. **Cepat.** Unit Test cocok untuk local loop dan setiap Pull Request.
7. **Safe by default.** Privileged/server/public-network/destructive/overwrite default ke denied/off.
8. **Traceable.** Test penting memiliki Requirement ID, Feature ID, rule, component, dan Test ID.
9. **No false assurance.** Fake tidak boleh diklaim sebagai bukti perilaku infrastruktur nyata.
10. **No implementation lock-in.** Jangan menguji private helper, LINQ shape, atau call order yang bukan kontrak.
11. **Permanent regression.** Setiap defect yang diperbaiki memiliki regression test.
12. **Unit green bukan DONE.** Definition of Done 8 lapis tetap berlaku.

---

# 5. Batas Unit vs Integration

| Suite | Yang diuji | Batas yang diganti | Yang tidak terbukti |
|---|---|---|---|
| Domain Unit | Value object, invariant, aggregate, policy, state transition | Tidak ada atau pure data | Persistence/transport/UI |
| Application Unit | Handler/service, orchestration, validation, resolver, typed intent | Clock, storage, provider, network, permission/policy port | Real infrastructure |
| Pure Adapter Unit | DTO/mapper/error/presenter/reducer/protocol-independent validator | Framework host | Wire/middleware/platform |
| Architecture/Contract | Dependency direction, generated contract/registry compatibility | Tooling terkontrol | Business outcome penuh |
| Integration/Component | DB/filesystem/object storage/HTTP/IPC/engine sandbox | Komponen nyata terkontrol | UAT/Production scale |
| E2E/Security/Performance | Jalur lintas boundary, abuse, resource budget | Environment khusus | Tidak digantikan Unit Test |

Aturan utama:

> Fake PDF engine hanya membuktikan kontrak Coordinator/Adapter. Fake tersebut tidak membuktikan hasil PDF benar, aman, atau memiliki fidelity yang memadai.

---

# 6. Teknologi Pengujian

```text
Framework : MSTest
Runner    : Microsoft.Testing.Platform (MTP)
Runtime   : .NET 10 LTS baseline; exact SDK patch dipin saat repository scaffold
CI        : Azure DevOps Pipeline
```

Aturan dependensi:

- gunakan package yang telah disetujui dan dipin secara terpusat;
- jangan menambahkan mocking/assertion/coverage package diam-diam;
- handwritten fake/stub/spy menjadi pilihan awal;
- package baru memerlukan tujuan, lisensi, security, maintenance, compatibility review, dan approval sesuai `TECH-DEC-005`;
- exact CLI flag mengikuti versi MSTest/MTP yang dipin, bukan ditebak dalam dokumen ini.

---

# 7. Struktur Project Unit Test

Struktur target harus mengikuti assembly/modul produksi aktual:

```text
tests/
├── Unit/
│   ├── BuildingBlocks/
│   │   ├── Pertamina.Oto.Contracts.UnitTests/
│   │   ├── Pertamina.Oto.Security.UnitTests/
│   │   ├── Pertamina.Oto.Observability.UnitTests/
│   │   ├── Pertamina.Oto.Storage.Abstractions.UnitTests/
│   │   └── Pertamina.Oto.Processing.Abstractions.UnitTests/
│   ├── Modules/
│   │   ├── Pdf/
│   │   ├── Conversion/
│   │   ├── Image/
│   │   ├── ScannerOcr/
│   │   ├── SignatureAnnotation/
│   │   ├── FileProductivity/
│   │   ├── Workspace/
│   │   ├── AccountAdministration/
│   │   ├── Audit/
│   │   ├── Support/
│   │   ├── Assistant/
│   │   └── ContentIntelligence/
│   ├── Hosts/
│   │   ├── Api/
│   │   ├── Web/
│   │   ├── Worker/
│   │   ├── Desktop/
│   │   └── Mobile/
│   └── TestKit/
└── Integration/ Architecture/ Contract/ E2E/ Performance/
```

Ketentuan:

- `<ProductionAssembly>.UnitTests` adalah pola nama project;
- satu project test hanya mereferensikan assembly miliknya dan BuildingBlocks yang disetujui;
- Domain dan Application dapat digabung jika production assembly memang satu;
- jangan membuat project kecil hanya untuk mengikuti diagram;
- jangan membuat satu global Unit Test project yang melanggar module ownership;
- `ContentIntelligence` adalah grouping logis sampai owner assembly aktual ditetapkan; bukan otorisasi membuat modul baru;
- TestKit hanya memuat primitive netral seperti fake clock/ID, CanarySet, assertion aman, dan builder kontrak umum.

---

# 8. Standar Penamaan

## 8.1 Method Test

Pola wajib:

```text
MethodOrBehavior_Condition_ExpectedResult
```

Contoh:

```text
MergeAsync_WithTwoValidFiles_ReturnsMergedResult
Execute_WhenPermissionDenied_ReturnsDenied
Retry_WhenSameIdempotencyKey_DoesNotCreateDuplicateJob
Redact_WhenNotConfirmed_DoesNotFinalizeOutput
Transition_FromCompletedToRunning_ReturnsInvalidTransition
```

Nama harus menjelaskan:

1. apa yang diuji;
2. kondisi penting;
3. hasil yang diharapkan.

## 8.2 File dan Class

| Item | Pola | Contoh |
|---|---|---|
| Project | `<ProductionAssembly>.UnitTests` | `Pertamina.Oto.Modules.Pdf.Application.UnitTests` |
| Class | `<SubjectOrBehavior>Tests` | `MergePdfRequestValidatorTests` |
| Builder | `<Concept>Builder` | `CapabilitySnapshotBuilder` |
| Fake | `Fake<Boundary>` | `FakeClock` |
| Regression | Subject atau defect behavior | `SecuredFileAuthorizationRegressionTests` |

## 8.3 Metadata

Metadata minimum:

```text
Layer=Unit
Module=<owner>
Priority=P0|P1|P2|P3
Requirement=<ID/range>
Feature=<canonical Feature ID bila berlaku>
Category=<BUSINESS_RULE|SECURITY|...>
Regression=<defect/work-item bila berlaku>
```

Metadata tidak menggantikan nama test dan assertion yang jelas.

---

# 9. Arrange–Act–Assert

```text
Arrange → susun satu context deterministik dan satu perubahan utama
Act     → panggil public behavior satu kali
Assert  → periksa result/state/error/intent dan side effect yang dilarang
```

Contoh bentuk:

```csharp
[TestMethod]
public async Task Execute_WhenPermissionDenied_ReturnsDenied()
{
    // Arrange
    // Act
    // Assert
}
```

Given–When–Then boleh digunakan konsisten untuk state machine atau decision table. Jangan mencampur beberapa gaya dalam satu suite tanpa alasan.

---

# 10. Prinsip Mock, Fake, Stub, dan Spy

| Bentuk | Penggunaan |
|---|---|
| Stub | Mengembalikan satu fakta boundary tetap |
| Fake | Model behavior/state kecil dan deterministik |
| Spy | Mencatat interaksi yang bermakna, misalnya “upload tidak dipanggil” |
| Mock | Ekspektasi protokol ketat; digunakan jarang ketika jumlah/urutan call memang kontrak |

Boundary yang boleh diganti:

- clock dan ID generator;
- filesystem/artifact storage;
- SQL/SQLite repository port;
- connectivity/network state;
- Engine/Provider port;
- permission/policy/configuration snapshot;
- scheduler/cancellation;
- logger/telemetry/Audit sink;
- service eksternal.

Jangan mengganti dengan mock:

- domain rule yang sedang diuji;
- validator/policy/handler/resolver yang menjadi subject;
- seluruh rantai internal satu per satu;
- algoritme produksi yang kemudian ditulis ulang di mock setup.

Interaction assertion dibenarkan ketika ketidakadaan side effect adalah requirement, misalnya tidak ada upload, engine invocation, final publication, atau Audit leakage.

---

# 11. Determinisme

| Sumber nondeterminisme | Aturan Unit Test |
|---|---|
| Waktu | Inject fixed/advanceable UTC clock; jangan pakai wall clock langsung |
| ID | Inject deterministic opaque ID generator |
| Random | Gunakan case eksplisit atau seed yang tercatat |
| Retry/jitter | Fake scheduler + deterministic jitter |
| Network | Explicit `OFFLINE/ONLINE/DEGRADED`; tanpa socket nyata |
| Filesystem | Opaque ref dan hasil boundary; tanpa machine path |
| Culture/timezone | Set eksplisit dan pulihkan |
| Async | Await observable completion; tanpa sleep-and-hope |
| Provider | Fake response terkontrol; tanpa SDK/network nyata |
| Configuration | Immutable typed snapshot per test |

Test yang sama harus menghasilkan result, state, Error Code, dan intent yang sama pada local serta CI.

---

# 12. Isolasi, Parallelism, dan Cleanup

- setiap test memiliki object graph dan mutable fixture sendiri;
- shared fixture harus immutable;
- test tidak bergantung pada test sebelumnya;
- tidak ada fixed shared filename/current directory/global singleton;
- parallel execution diizinkan jika suite aman;
- `DoNotParallelize` hanya untuk batas yang benar-benar tidak dapat diparalelkan dan harus terdokumentasi;
- cleanup harus idempotent dan tidak menyembunyikan kegagalan;
- Unit Test tidak memulai process, listener, database, atau service nyata;
- perubahan worker count/order saat hardening digunakan untuk menemukan isolation defect.

Menonaktifkan parallelism global bukan perbaikan permanen untuk race dalam test atau production code.

---

# 13. Strategi Data Test

Data test wajib:

- sintetis dan tidak sensitif;
- minimal valid secara default;
- immutable atau dibuat ulang per test;
- mengubah hanya satu dimensi utama untuk negative test;
- tidak menggunakan employee identity, Production document, credential, signature, screenshot, atau issue nyata;
- tidak memuat secret dalam failure message/artifact.

Builder yang direkomendasikan:

```text
FeatureDescriptorBuilder
FileInputBuilder
OrderedInputsBuilder
PolicySnapshotBuilder
CapabilitySnapshotBuilder
AuthorizationDecisionBuilder
ConfirmationContextBuilder
JobBuilder
CheckpointBuilder
ValidatedArtifactBuilder
AuditEventBuilder
AssistantPlanBuilder
```

Data source bersama:

```text
CanonicalJobTransitions
InvalidJobTransitions
ValidationBoundaryCases
AuthorizationDecisionCases
OfflineGrantCases
EngineResolverCases
Core33FeatureMappings
ForbiddenEvidenceCanaries
```

Data source memuat input dan expected outcome; tidak boleh menyalin algoritme produksi.

---

# 14. Test ID dan Template Unit Test Case

## 14.1 Format Test ID

```text
UT-<AREA>-<NNN>
```

Contoh:

```text
UT-MERGE-001
UT-AUTHZ-014
UT-JOB-008
UT-AUDIT-021
UT-ASST-005
```

Test ID adalah identifier dokumentasi/test case. Test ID bukan Feature ID atau Requirement ID baru.

## 14.2 Template

| Field | Isi wajib |
|---|---|
| Test ID | `UT-<AREA>-<NNN>` |
| Judul | Perilaku yang diuji |
| Modul | Owner assembly/modul |
| Feature ID | Feature canonical atau `N/A` |
| Requirement ID | ID/range berwenang |
| Business/Security Rule | Aturan yang dibuktikan |
| Component/Class | Subject produksi |
| Precondition | State/context awal |
| Input | Data sintetis |
| Mock/Fake | Boundary yang diganti |
| Action | Public behavior yang dipanggil |
| Expected Result | Result/state/output yang benar |
| Expected Error | Stable Error Code/category bila gagal |
| Security Expectation | Side effect/leakage yang dilarang |
| Audit Expectation | Intent/event atau `NONE` |
| Priority | P0/P1/P2/P3 |
| Automation Status | PLANNED/IMPLEMENTED/PASS/FAIL/BLOCKED |

---

# 15. Prioritas Test

| Prioritas | Definisi | Contoh | Gate |
|---|---|---|---|
| P0 | Keamanan/bisnis kritis; risiko unauthorized access, leakage, data/source corruption, destructive error, mandatory Audit hilang, false success | Wrong owner, password leakage, source overwrite, Assistant bypass, Redact palsu | Wajib ada dan PASS; tidak boleh ignore/quarantine-as-green |
| P1 | Jalur utama Core Feature dan shared contract | Merge order, validation, Local-First, Job accepted/terminal | Wajib pada feature terkait dan PR Unit suite |
| P2 | Perilaku umum/alternate normal | Optional parameter, supported fallback | Wajib sesuai perubahan/feature |
| P3 | Edge case kredibel berisiko lebih rendah | Boundary format langka | Berdasarkan risiko dan nilai |

Prioritas test bukan MoSCoW Feature, defect severity, atau urutan eksekusi pipeline.

---

# 16. Pengujian Business Rule

Business-rule suite minimum:

| Invarian | Positive | Negative |
|---|---|---|
| Sumber tidak ditimpa diam-diam | Hasil baru/Save As | Target=sumber atau target existing tanpa kontrak |
| Working state terpisah | Edit/reorder/remove mengubah draft | Edit tidak mengubah source |
| Local-First capability-aware | Local eligible memilih local | Policy/resource/capability denial tidak dibypass |
| Background bukan server | Pekerjaan berat memilih Local Worker | Tidak ada upload/API server otomatis |
| Feature canonical | Registered/active/version-compatible | Unknown/alias execution/disabled/unregistered ditolak |
| Final hanya setelah validasi | Valid temp dipublikasikan | Invalid/partial/temp tidak final |
| Maksimum tiga secured version | Versi keempat men-tombstone versi tertua | Stale/concurrent request tidak menghasilkan >3 aktif |
| Personal History terpisah dari Audit | History dapat dihapus | Penghapusan History tidak menghapus Audit/result |
| Assistant hanya orchestrator | Memanggil application service canonical | Tidak memilih engine/provider atau mengarang sukses |

Assertion mencakup result/state dan side effect yang dilarang, bukan private method count.

---

# 17. Pengujian Validasi

Urutan validasi logis:

```text
envelope/identifier
→ Feature registry dan version
→ typed parameter schema
→ input count/type/state/ownership
→ permission/policy/classification
→ capability/resource/connectivity/consent
→ engine/provider boundary
→ output validation
→ final publication
```

Kasus wajib:

- null/missing/unknown/duplicate/ambiguous field;
- wrong type, unknown enum/discriminator, unsupported version;
- bawah/sama/di atas batas count/size/page/depth/time;
- canonical/unknown/inactive/hidden/unregistered Feature ID;
- input `READY` dan semua state non-executable;
- extension/MIME/signature/structure mismatch;
- wrong owner/stale identity/changed source;
- insufficient storage/memory/capacity;
- missing consent/confirmation/reauthentication;
- invalid output type/structure/size/page/digest/security property.

Input invalid harus berhenti sebelum engine, upload, Job, atau publication.

---

# 18. Pengujian State Machine

## 18.1 State Job Canonical

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

`PAUSED` tidak valid. State secured transfer menggunakan model berbeda dan tidak boleh digabung ke enum Job.

## 18.2 Aturan Test

- setiap transisi yang diizinkan memiliki positive test;
- setiap pasangan ilegal memiliki generated/data-driven negative test;
- stale `rowVersion/stateVersion` menghasilkan conflict, bukan last-write-wins;
- terminal state tidak regresi;
- `COMPLETED` hanya setelah output valid/final tersedia;
- `FAILED` tidak memiliki primary final output;
- `CANCELLED` menghentikan progress/final publication berikutnya;
- waiting mempertahankan Job/input tanpa mengarang failure/success;
- retry membuat attempt baru tetapi mempertahankan logical Job ID dan Correlation ID.

Contoh wajib:

```text
COMPLETED → RUNNING = INVALID
CANCELLED → COMPLETED = INVALID
FAILED → RETRYING = valid hanya melalui command/policy yang sah
WAITING_CONNECTION → QUEUED/RUNNING = valid setelah revalidation
```

---

# 19. Pengujian Error

Test Error Mapper memastikan:

- stable `code`;
- safe/localizable `message`;
- valid `correlationId`;
- hanya typed details: `VALIDATION`, `RETRY_GUIDANCE`, `CONFLICT`, atau `LIMIT`;
- tidak ada stack, SQL, path, object key, token, password, content, prompt, atau provider detail.

Semantik minimum:

| Kondisi | Hasil logis/HTTP |
|---|---|
| Malformed envelope/query/header | 400 |
| Authentication tidak valid | 401 |
| Permission/ownership/policy deny | 403 atau concealed 404 sesuai resource policy |
| State/idempotency/checkpoint conflict | 409 |
| Resource expired | 410 |
| Version/ETag mismatch | 412 |
| Hard payload limit | 413 |
| Unsupported media/signature | 415 |
| Invalid range | 416 |
| Semantic/file/business validation | 422 |
| Caller rate/quota/concurrency | 429 + safe `Retry-After` |
| Unexpected failure | safe 500 |
| Shared unsafe saturation | 503 |

Unit Test menguji decision/mapper, bukan HTTP server nyata.

---

# 20. Pengujian Recovery

Kasus wajib:

- cancel sebelum dispatch dan saat safe point;
- Worker/engine crash dipetakan menjadi recovery candidate, bukan success;
- transient failure mendapat bounded retry;
- permanent/validation/security failure tidak auto-retry;
- attempt limit mencegah hot loop;
- checkpoint hanya valid jika Feature/schema/input/engine/stage/digest/expiry cocok;
- incompatible/expired checkpoint tidak silent resume;
- partial/temp output tetap non-final;
- failure saat publish mempertahankan source/target lama;
- retry/reconcile tetap menggunakan Job ID, Correlation ID, dan Idempotency-Key yang benar;
- cleanup/reconciliation intent dipertahankan bila operasi eksternal ambigu.

Timer/retry diuji dengan fake clock/scheduler, bukan `Task.Delay`.

---

# 21. Pengujian Otorisasi

Formula decision yang diuji:

```text
AuthenticatedOrEligibleOfflineGrant
AND AccountActive
AND SessionOrGrantValid
AND ActionPermission
AND SubjectTypeAllowed
AND ResourceOwnershipOrExplicitScope
AND EnterprisePolicyAndClassification
AND ResourceStateAndVersion
AND ClientCompatibilityAndCapability
AND ConnectivityAllows
AND RequiredConsentAndConfirmation
AND FreshReauthenticationWhenRequired
```

Matrix minimum:

- allow dengan seluruh guard valid;
- permission hilang;
- role label ada tetapi permission hilang;
- wrong subject type;
- User A terhadap resource User B;
- parent-child mismatch;
- Administrator mencoba membaca dokumen pengguna tanpa permission khusus;
- Auditor mencoba update/delete Audit;
- Support mencoba resource di luar Issue/attachment yang diizinkan;
- expired/revoked/stale session/permission;
- policy/classification deny;
- confirmation/reauth ada tetapi permission tetap hilang;
- opaque ID/fileRef/Job ID/Correlation ID/cursor/ETag digunakan sebagai bukti akses;
- concealment menghasilkan outward result yang konsisten.

Authorization denial harus menghentikan content/storage resolution.

---

# 22. Pengujian Keamanan

Security logic P0 mencakup:

- deny-by-default;
- permission evaluator dan resource authorization;
- Feature/handler/schema allowlist;
- path/name/ref policy;
- password operation-memory-only;
- public/unapproved AI provider rejected;
- client/Assistant tidak dapat memilih engine/provider/class/path/URL/SQL/shell;
- required inspection yang unavailable/inconclusive tidak menghasilkan `READY`;
- output validation sebelum success;
- confirmation/reauth binding pada principal/session/action/resource/policy/freshness;
- hard safety ceiling tidak dapat dilemahkan konfigurasi admin;
- same-user/worker/IPC facts hanya diuji sebagai typed decision; OS guarantee diserahkan ke security integration.

Contoh utama:

```text
User A + resource User B → DENY
Assistant session valid + tool permission hilang → DENY
Public AI provider muncul di registry → REJECT
Password masuk Audit mapper → REJECT
```

---

# 23. Pengujian Privasi dan Sensitive-Data Exclusion

Gunakan `CanarySet` sintetis untuk:

```text
password/passphrase
access/refresh token dan cookie
secret/private/encryption key
document bytes/full text
OCR/translated/Markdown content
signature/paraf payload
clipboard
prompt/chat/provider request/response
issue message/screenshot/attachment
raw path/storage key/URL
SQL/stack/claim dump
```

Setiap canary diuji agar tidak muncul pada:

- error dan response details;
- structured log/scope/exception/trace/metric/health;
- idempotency fingerprint/snapshot/safe response;
- Audit intent/outbox/event/export/alert;
- Assistant metadata/history;
- test failure message/report/artifact.

Kontrol utama adalah closed typed allowlist dan source-aware mapping. Pattern redaction hanya defense in depth. Sanitizer failure harus fail closed.

---

# 24. Pengujian Local-First

| ID | Kondisi | Hasil wajib |
|---|---|---|
| `UT-LF-001` | Feature `LOCAL`, capability/policy/resource valid | Local executor; server preflight/upload/execute = 0 call |
| `UT-LF-002` | Feature `LOCAL`, jaringan tersedia | Tetap lokal; network tidak memicu server |
| `UT-LF-003` | Pekerjaan lokal berat | Local Worker; bukan server |
| `UT-LF-004` | `OPTIONAL_SERVER`, local provider valid | Local dipilih |
| `UT-LF-005` | Local unavailable, server diizinkan, online, consent valid | `SERVER_PREPARE`; preflight sebelum upload |
| `UT-LF-006` | Server technically available, consent hilang | Tidak ada upload; confirmation/consent required |
| `UT-LF-007` | Server dilarang policy | Deny/unavailable; tanpa public fallback |
| `UT-LF-008` | Local memory/storage kurang, server opsional | Server hanya setelah seluruh gate; kekurangan lokal bukan consent |
| `UT-LF-009` | Fakta resolver identik | Decision dan reason sama |

Client/UI/Assistant tidak boleh memaksa local/server/provider.

---

# 25. Pengujian Offline

Kasus wajib:

- valid protected grant + matching profile/device + trusted time + local eligible → `ALLOW_LOCAL`;
- server-required/secured/Admin/Support/Audit query/export/realtime → `ONLINE_REQUIRED`/DENY;
- expiry, tamper, clock rollback, wrong profile/account, stale revision → fail closed;
- offline local Merge tetap berjalan;
- queued intent saat reconnect wajib refresh identity/permission/policy/capability terlebih dahulu;
- permission revoked saat offline menghentikan dispatch;
- account switch tidak mengirim queue profil lama;
- Assistant local-only workflow dinilai per step;
- satu step online-only tidak membuat workflow upload diam-diam;
- offline Audit outbox hanya metadata dan tidak memberi authority.

Lifetime/trusted-time/revision tolerance adalah typed policy input, bukan angka hardcoded.

---

# 26. Pengujian Sinkronisasi Selektif

## 26.1 Tidak Auto-Sync

Data-driven test memastikan tidak ada generic sync mapping untuk:

```text
source
draft/working state
undo/redo
cache/thumbnail/temp
checkpoint
clipboard
password/token/secret
visual signature asset
local history/recent file
Assistant conversation
```

## 26.2 Empat Jalur Terpisah

1. execution transfer;
2. explicit **Amankan ke Akun** secured transfer;
3. metadata-only Audit outbox;
4. allowlisted account preference.

Test memastikan hanya explicit action pada satu eligible validated file/result membuat `secured_transfer`. Duplicate/retry memakai Idempotency-Key yang sama; fingerprint berubah menghasilkan conflict; remote ETag/version berubah tidak silent overwrite; content tidak auto-merge.

---

# 27. Pengujian Engine Resolver

Resolver menerima snapshot:

```text
Feature/status/risk/schema/exposure
platform capability
format/size/page/file count
memory/temp/storage
policy/classification/permission
connectivity/server capacity
approved provider/capability/version/license/health
consent/confirmation
attempt history
```

Kasus minimum:

- approved primary local dipilih;
- stable priority/tie-break untuk beberapa provider setara;
- approved specialized fallback hanya jika compatible;
- validation/security/content failure tidak difallback untuk bypass kontrol;
- incompatible/unapproved/dynamic/public provider ditolak;
- `LOCAL` tidak pernah menghasilkan server candidate;
- `OPTIONAL_SERVER` tetap Local-First;
- offline server-only menghasilkan waiting/online-required;
- stale capability revision memicu re-evaluation;
- attempt history mencegah provider cycle;
- caller/AI-supplied vendor/engine/class/URL diabaikan/ditolak.

Resolver test menggunakan provider descriptor fake, bukan vendor SDK.

---

# 28. Pengujian Provider/Engine Adapter

Shared adapter contract:

- memetakan canonical input/options ke provider DTO secara tepat;
- meneruskan cancellation/deadline/budget;
- memetakan success/unsupported/transient/permanent/cancel/timeout secara stabil;
- tidak mengekspos vendor detail;
- output provider selalu melewati output validator;
- partial/intermediate output tidak menjadi final;
- output wrong type/root/size/count ditolak;
- capability/version mapping deterministic;
- unknown polymorphic type tidak diaktifkan;
- request berulang menghasilkan provider request semantik yang sama.

Yang tidak diuji pada Unit:

- PDF fidelity/parser security/redaction forensic correctness;
- OCR accuracy dan resource behavior;
- Office conversion fidelity;
- codec/pixel/ML quality;
- provider retention/egress nyata;
- physical scanner/device behavior.

---

# 29. Pengujian Logika Kontrak API

Yang termasuk Unit:

- closed DTO/query/header validator;
- Feature Contract Index lookup;
- canonical identifier/version/fileRef logic;
- pagination/filter/search/sort normalization;
- cursor decision dengan typed verified facts;
- HTTP semantic/error response factory;
- admission/rate/quota decision calculator;
- idempotency decision service;
- safe mapping/envelope logic.

Kasus penting:

- unknown/extra/ambiguous field ditolak;
- invalid Feature/schema/fileRef/workflow graph ditolak;
- local-only Feature ke processing API ditolak;
- page + cursor bersamaan ditolak;
- invalid/expired/cross-scope cursor ditolak;
- filter/sort/search non-allowlist ditolak;
- 202 hanya dibuat setelah durable-accepted fact diberikan;
- 429 caller limit dibedakan dari 503 shared saturation;
- same idempotency key + same fingerprint me-replay resource/Job yang sama;
- same key + different fingerprint menghasilkan conflict;
- password/secret tidak masuk fingerprint.

Yang diserahkan ke Contract/Integration: generated OpenAPI diff, serializer/wire behavior, middleware order, route binding, header aktual, HTTP server, SignalR Hub.

---

# 30. Pengujian Audit Mapping

Setiap mapping memastikan:

- registered Event Type/Version, Category, dan Level;
- stable Audit Event ID/Client Event ID;
- `OccurredAtUtc` dan `ReceivedAtUtc` tidak tertukar;
- Actor aktual, Origin, Executor, dan On-Behalf-Of benar;
- Action, Resource, Outcome, reason/Error Code benar;
- Feature ID canonical;
- Job ID bila ada background Job;
- Correlation ID konsisten;
- Safe Metadata sesuai exact allowlist;
- prohibited content tidak masuk.

Aturan Core:

- seluruh 33 Core execution menghasilkan L2 accepted/terminal evidence sesuai Dokumen 10;
- Protect, Unlock, Visual Sign NON-PKI, dan Redact juga menghasilkan L3 finalization/validation evidence;
- progress/reorder/preview/ordinary validation tidak membuat Audit noise;
- duplicate Event ID + same fingerprint diakui satu kali;
- same Event ID + different fingerprint conflict;
- Correlation ID bukan deduplication key;
- Assistant-triggered action mempertahankan User sebagai Actor dan `Origin=ASSISTANT`.

Unit Test membuktikan mapper/factory. Append-only DB, protected outbox durability, ingestion race, export, archive, dan restore menjadi Integration/Operational Test.

---

# 31. Pengujian Assistant

Matrix wajib:

- intent → Feature ID canonical yang benar;
- unknown/invented/hidden/disabled Feature ditolak;
- typed parameter dan workflow dependency divalidasi;
- permission/policy/ownership/capability/confirmation/reauth dinilai per step;
- File-First, Tool-First, dan Assistant-First menghasilkan authorization decision yang sama;
- `assistant.session.use` tidak memberi tool permission;
- cross-user resource ditolak;
- engine/provider/path/URL/SQL/shell/arbitrary action ditolak;
- ambiguous destructive request menghasilkan clarification;
- high-risk action tanpa confirmation tidak berjalan;
- document/OCR/HTML/metadata instruction diperlakukan sebagai DATA;
- online-only step saat offline tidak direroute/upload diam-diam;
- model/provider output dianggap untrusted;
- success hanya berasal dari actual validated result/Job state;
- User tetap Actor; Assistant hanya Origin.

Assistant tidak memiliki engine, endpoint, atau security bypass sendiri.

---

# 32. Pengujian Configuration dan Policy

Test policy/configuration meliputi:

- system safety > enterprise policy > user preference;
- unknown/missing privileged policy fails closed;
- Feature flag dapat enable/disable exposure, tetapi tidak mengubah semantics/Feature ID/contract;
- limit boundary diuji below/equal/above;
- stale policy/config revision ditolak;
- hard safety ceiling tidak dapat dilonggarkan admin;
- classification deny tidak dapat ditimpa consent/preference;
- exact values berasal dari typed policy fixture;
- perubahan policy menghasilkan expected re-evaluation/Audit intent;
- config tidak dapat memasukkan public AI atau provider tidak disetujui.

---

# 33. Strategi 33 Primary PDF Tool Cards

Dokumen tidak membuat ratusan test copy-paste untuk setiap Tool Card. Strateginya:

```text
registry-wide completeness tests
+ shared family contract tests
+ feature-specific critical tests
+ PoC/Component tests untuk fidelity/engine/security nyata
```

Setiap Core Tool minimal memiliki:

```text
P0  authorization/privacy/source/finality/high-risk invariant yang berlaku
P1  primary success + canonical validation/failure path
P2  common option/alternate path
P3  credible edge case berdasarkan risiko
```

Registry-wide suite memastikan 33/33 mapping:

- Feature ID tersedia, unik, active/expected status;
- Primary Tool mapping tepat;
- typed schema/version tersedia ketika implementasi dirilis;
- permission/policy metadata tersedia;
- execution profile sama dengan Dokumen 06/09;
- registered handler/adapter tersedia pada platform yang diekspos;
- L2 Audit mapper accepted/terminal tersedia;
- high-risk L3 mapper tersedia untuk Protect/Unlock/Sign/Redact;
- tidak ada Tool Card yang membuat endpoint/engine paralel.

---

# 34. Implementation Family Test Matrix

| Family | Primary Tool/Feature | Shared rule tests | Critical feature-specific tests |
|---|---|---|---|
| PDF Page Operations | Merge `FEAT-PDF-005`; Split `FEAT-PDF-006/007`; Remove `FEAT-PDF-013`; Extract `FEAT-PDF-014`; Organize `FEAT-PDF-010..016`; Rotate `FEAT-PDF-012` | Ordered pages, range/count, reversible working state, source unchanged, output validation | Merge order/atomicity; Split range; Remove minimum page; Extract selected pages; Organize order; rotation normalization |
| Optimize/Compare | Compress `FEAT-PDF-023`; Repair `FEAT-PDF-025/026`; Compare `FEAT-PDF-071/072` | Local/Worker decision, resource bound, safe output, no false success | Compression option bounds; repair fallback; compare result/provenance |
| Scan/OCR | Scan to PDF `FEAT-SCAN-014`; OCR PDF `FEAT-OCR-004` | Capability/device/engine adapter, untrusted OCR, cancellation, resource limit | Scanner unavailable mapping; OCR language/page selection; no public AI |
| Conversion to PDF | JPG `FEAT-CONV-007`; Word `FEAT-CONV-001`; PowerPoint `FEAT-CONV-003`; Excel `FEAT-CONV-002`; HTML `FEAT-CONV-017` | Input/output type, Local-First, Optional Server gate, fidelity handoff | Office active-content policy; HTML arbitrary remote URL denied; JPG ordering |
| Conversion from PDF | JPG `FEAT-CONV-012`; Word `FEAT-CONV-004`; PowerPoint `FEAT-CONV-006`; Excel `FEAT-CONV-005`; PDF/A `FEAT-PDF-075` | Typed options, adapter result, output validation, PoC gate | Page/image selection; unsupported layout; PDF/A conformance handoff |
| Edit/Annotation/Form | Page number `FEAT-PDF-037`; Watermark `FEAT-PDF-036`; Crop `FEAT-PDF-019`; Edit `FEAT-PDF-031..041`; Forms `FEAT-PDF-053..060` | Working state, undo/redo, parameter bounds, preview/final separation | Page numbering range; watermark opacity/position; crop bounds; unsupported text edit; form field identity |
| PDF Security | Unlock `FEAT-PDF-062`; Protect `FEAT-PDF-061`; Sign `FEAT-SIGN-013`; Redact `FEAT-PDF-069` | Permission, confirmation/reauth policy, password memory-only, source unchanged, L3 Audit | Valid authority Unlock; Protect result; Sign visual NON-PKI/no asset leak; Redact true removal/final validation contract |
| PDF Intelligence | Summarizer `FEAT-INTEL-001`; Translate `FEAT-INTEL-002`; Markdown `FEAT-CONV-036` | Local/internal provider only, minimization, consent, untrusted output, grounding | Selected-page summary; language scope; deterministic extraction before OCR fallback |

Semua fidelity, malicious corpus, real parser/provider, PDF/A compliance, redaction forensic correctness, scanner device, dan resource benchmark berada di luar Unit Test.

---

# 35. Spesifikasi Lengkap Vertical Slice Merge PDF

## 35.1 Kontrak

| Atribut | Baseline |
|---|---|
| Feature | `FEAT-PDF-005 — Merge PDF` |
| Input/output | Dua atau lebih PDF berurutan → satu PDF |
| Profile | `L1`, LOCAL baseline |
| Risiko | LOW; rule source/finality/privacy tetap kritis |
| Permission | `document.content.process` pada input yang diotorisasi dan hasil milik sendiri |
| State | Immediate local atau background Local Worker |
| Server | Tidak ada processing API/SignalR untuk baseline Merge |
| Finalisasi | Temp tervalidasi → Save As/hasil baru; source tidak ditimpa diam-diam |
| Partial success | Tidak valid; Merge bersifat atomik |
| Trace | Correlation ID selalu; Job ID hanya jika background local Job |
| Evidence | L2 accepted + terminal; metadata aman |

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

Exact parameter field/error code yang belum ada pada governed contract tidak boleh dikarang. Unit Test menggunakan semantic category dan kemudian diikat ke versioned contract artifact saat scaffold.

## 35.2 Test Case

| Test ID | Judul/perilaku | Expected Result | Priority |
|---|---|---|:---:|
| `UT-MERGE-001` | Execute tanpa input | Ditolak minimum count; engine/Job/publisher = 0 call | P1 |
| `UT-MERGE-002` | Execute dengan satu PDF | Ditolak; tidak membuat copy palsu sebagai hasil Merge | P1 |
| `UT-MERGE-003` | Execute dengan dua PDF valid | Satu plan/hasil PDF valid; order dipertahankan | P1 |
| `UT-MERGE-004` | Execute dengan banyak PDF valid | Semua occurrence ikut tepat sesuai order | P1 |
| `UT-MERGE-005` | Initial order A,B,C | Engine request tetap A,B,C; bukan filename/time/hash order | P1 |
| `UT-MERGE-006` | Reorder A,B,C menjadi C,A,B | Working state dan execution plan menjadi C,A,B | P1 |
| `UT-MERGE-007` | Remove B dari A,B,C | Plan menjadi A,C; B tidak dihapus/diubah | P1 |
| `UT-MERGE-008` | Remove hingga tersisa satu input | Working edit boleh tersimpan, tetapi execute invalid | P2 |
| `UT-MERGE-009` | Explicit duplicate occurrence A,A | Tidak silent dedupe; dua occurrence dipertahankan deterministik | P2 |
| `UT-MERGE-010` | Ordered ID tidak ada di input | Command ditolak; tidak best-effort/drop diam-diam | P0 |
| `UT-MERGE-011` | Input count/page/size pada batas | Sama dengan batas diterima; di atas batas ditolak | P1 |
| `UT-MERGE-012` | Extension/MIME/signature/structure valid | Input menjadi `READY` | P1 |
| `UT-MERGE-013` | Extension PDF tetapi signature bukan PDF | Ditolak sebelum engine | P0 |
| `UT-MERGE-014` | Satu PDF corrupted | Merge atomik gagal; tidak ada partial/final result | P0 |
| `UT-MERGE-015` | Security inspection blocked/inconclusive | Non-addressable; engine tidak dipanggil | P0 |
| `UT-MERGE-016` | Encrypted input tanpa password | `PASSWORD_REQUIRED`; tidak execute | P1 |
| `UT-MERGE-017` | Password salah | Safe failure; raw password/provider detail tidak muncul | P0 |
| `UT-MERGE-018` | Password valid ephemeral | Proses boleh lanjut; password tidak persist/log/audit/checkpoint | P0 |
| `UT-MERGE-019` | Input berubah setelah preflight | Conflict/revalidation; stale bytes tidak digunakan | P0 |
| `UT-MERGE-020` | Local engine capable/healthy/resources cukup | Resolver memilih local; server/upload/SignalR = 0 call | P1 |
| `UT-MERGE-021` | Offline tetapi local capability valid | Merge tetap berjalan lokal | P1 |
| `UT-MERGE-022` | Pekerjaan besar melebihi in-process tetapi cocok Worker | Background Local Worker, bukan server | P1 |
| `UT-MERGE-023` | Approved compatible local fallback tersedia | Fallback lokal dipilih deterministik | P2 |
| `UT-MERGE-024` | Semua approved local engine unavailable | `WAITING_RESOURCE`/safe unavailable; tidak membuat server path | P1 |
| `UT-MERGE-025` | Disk/temp/output capacity kurang | `INSUFFICIENT_STORAGE`; engine/publisher = 0 call | P0 |
| `UT-MERGE-026` | Cancel saat queued | `CANCELLED`; engine tidak berjalan; tidak ada final output | P1 |
| `UT-MERGE-027` | Cancel saat running | Stop pada safe point; temp non-final; source aman | P0 |
| `UT-MERGE-028` | Cancel setelah completed | State tidak regresi; result tidak dihapus/dilabel ulang | P0 |
| `UT-MERGE-029` | Engine transient failure | Safe retryable error; bounded retry; raw exception tidak keluar | P1 |
| `UT-MERGE-030` | Engine permanent/validation failure | `FAILED`; tidak auto-retry; tidak ada final result | P1 |
| `UT-MERGE-031` | Retry dengan Job/idempotency context yang sama | Logical Job tetap; maksimal satu attempt baru yang sah | P0 |
| `UT-MERGE-032` | Same request + same Idempotency-Key | Tidak membuat duplicate job/effect | P0 |
| `UT-MERGE-033` | Output type/structure/page/size valid | Publish diizinkan dan result menjadi final | P1 |
| `UT-MERGE-034` | Output empty/wrong/truncated/corrupt | `RESULT_VALIDATION_FAILED`; temp tidak final | P0 |
| `UT-MERGE-035` | Output target invalid/outside allowed policy | Ditolak sebelum publication | P0 |
| `UT-MERGE-036` | Output target sama dengan salah satu source | Ditolak untuk baseline Save As; source tidak dibuka destructive-write | P0 |
| `UT-MERGE-037` | Existing target tanpa overwrite contract/confirmation | Conflict; target lama tetap aman | P0 |
| `UT-MERGE-038` | Publication gagal setelah staging | Tidak ada half-final result; reconciliation/cleanup intent tersedia | P0 |
| `UT-MERGE-039` | Correlation ID valid diberikan | ID sama pada plan/Job/result/error/log/History/Audit intent | P1 |
| `UT-MERGE-040` | Correlation ID hilang | Satu ID aman dibuat dan dipropagasikan konsisten | P1 |
| `UT-MERGE-041` | Immediate local success | `COMPLETED`, output tervalidasi, tanpa Job ID palsu | P1 |
| `UT-MERGE-042` | Background local accepted | Stable Job ID + `QUEUED`; accepted bukan completed | P1 |
| `UT-MERGE-043` | Completed menerima stale progress/running event | Terminal state tidak regresi | P0 |
| `UT-MERGE-044` | History metadata dibuat | Metadata aman; tidak ada PDF bytes/password/raw path | P1 |
| `UT-MERGE-045` | Accepted/terminal Audit intents | L2 event tepat; progress/reorder tidak membuat noise | P0 |
| `UT-MERGE-046` | Logger menerima exception/input canaries | Tidak ada document content/password/path/provider detail | P0 |
| `UT-MERGE-047` | Plan dibuat dua kali dari fakta identik | Plan semantik identik dan deterministic | P1 |
| `UT-MERGE-048` | Tool/File/Assistant origin dengan context sama | Command/authorization/outcome sama; hanya Origin berbeda | P0 |
| `UT-MERGE-049` | Assistant user tidak memiliki permission | Deny; Assistant tidak menjalankan engine | P0 |
| `UT-MERGE-050` | Unknown/disabled/schema-incompatible Merge descriptor | Ditolak sebelum resolver/engine | P0 |

## 35.3 Fixture Minimum

```text
FeatureRegistryFixture
OrderedInputsBuilder
DeterministicMergeEngineFake
EngineCatalogFake
ArtifactStoreFake
JobStoreFake
FakeClock
DeterministicIdGenerator
PolicySnapshotBuilder
CapabilitySnapshotBuilder
AuthorizationDecisionBuilder
EvidenceCollectorFake
SafeLoggerFake
CancellationHarness
```

## 35.4 Kriteria Lulus Merge

```text
[ ] 50/50 case terdokumentasi telah diimplementasikan atau memiliki disposition terkontrol
[ ] seluruh P0/P1 yang berlaku PASS
[ ] input invalid tidak mencapai engine
[ ] order dan reversible working state benar
[ ] Merge tetap LOCAL baseline
[ ] background local tidak dianggap server
[ ] source/target tidak ditimpa diam-diam
[ ] invalid/temp/partial output tidak final
[ ] Job/Correlation/Retry/Idempotency konsisten
[ ] History dan Audit metadata aman
[ ] tidak ada canary sensitif pada output test/report
[ ] handoff Component/PoC tercatat
```

---

# 36. Ketertelusuran

Trace chain:

```text
Business Process
→ Requirement ID
→ canonical Feature ID
→ Business/Security Rule
→ Production Module/Component
→ Unit Test ID/Class/Method/Priority
→ Pipeline Result
→ required downstream test
→ UAT/Release evidence
```

Matrix minimum:

| Area | Requirement | Feature | Unit suite |
|---|---|---|---|
| File validation/source safety | `FR-FILE-001..015`, `FR-STOR-001..006`, `NFR-REC-001..003` | Core file-consuming Features | Validation, Storage, Merge |
| Local/server routing | `FR-PROC-001..005`, `NFR-PERF-007..013` | 33 Core mappings | Coordinator, Resolver, Local-First |
| Retry/recovery | `FR-REC-001..006`, `NFR-REC-001..005` | `FEAT-REC-001..015` | Job, Retry, Checkpoint |
| Authorization | `FR-AUTH-001..005`, `FR-ROLE-001..003`, `SR-SEC-001..023` | `FEAT-AUTH-001..012` + all protected actions | AuthN support, AuthZ, Ownership, Reauth |
| Selective sync | `FR-BACKUP-001..010`, `FR-VER-001..004` | `FEAT-BACKUP-001..013`, `FEAT-VER-001..006` | Sync, SecuredVersion |
| Audit | `FR-AUDIT-001..004` | `FEAT-AUDIT-001..008` + Core evidence | AuditMapper, Canary, Dedup |
| Assistant | `FR-ASSIST-001..015`, `SR-SEC-013..018` | `FEAT-ASSIST-001..030` | Intent, Plan, Parity, Grounding |
| Intelligence | `FR-INTEL-001..003`, `SR-SEC-021` | `FEAT-INTEL-001..002`, `FEAT-CONV-036` | ProviderPolicy, Minimization, OutputGuard |
| Sign/Unlock/Redact | `FR-SIGN-001..003`, `SR-SEC-011..012,022..023` | `FEAT-SIGN-013`, `FEAT-PDF-061/062/069` | HighRiskFeature, Privacy, Audit L3 |
| API logic | `IR-API-001..012` | Server-executable Features/resources | DTO, Query, Error, Idempotency |
| DevOps | `IR-DEVOPS-001..004` | N/A | CI discovery, gate, report |

Unknown/duplicate Requirement ID atau Feature ID harus memblokir trace generation. Tool tidak boleh menciptakan mapping sendiri.

---

# 37. Strategi Coverage

Coverage digunakan sebagai indikator risiko, bukan pengganti kualitas assertion.

| Area ukur | Aturan |
|---|---|
| Perilaku | Setiap rule, branch berisiko, state transition, dan failure path penting memiliki test |
| Line dan branch | Diukur per module, perubahan, serta namespace kritis |
| Perubahan | Perilaku yang diubah wajib memiliki test baru/terbarui atau disposition terkontrol |
| Kritis | Authorization, privacy, Local-First routing, state machine, output publication, dan Audit diprioritaskan |
| Exclusion | Hanya generated code/boilerplate yang dibenarkan dan tercatat |
| Pelaporan | Unit coverage dipisahkan dari Integration, Contract, E2E, dan jenis test lain |

Ambang angka tidak dibuat pada dokumen ini karena baseline executable belum tersedia. Saat implementasi dimulai:

1. ukur baseline nyata per module;
2. tetapkan threshold terpusat melalui controlled change;
3. threshold boleh dinaikkan bertahap, tidak diturunkan untuk meloloskan perubahan;
4. pencapaian angka tidak membenarkan test tanpa assertion bermakna.

---

# 38. Quality Gate

| Gate | Syarat minimum |
|---|---|
| G0 — Scope | Requirement, Feature ID, module, dan prioritas test dikenali |
| G1 — Restore | Dependency restore terkunci dan berhasil |
| G2 — Build | Build `Release` dan analyzer berhasil tanpa pelonggaran aturan |
| G3 — Unit discovery | Seluruh project Unit ditemukan melalui MTP; zero-test discovery = gagal |
| G4 — Unit result | Semua P0/P1 yang berlaku dan seluruh affected Unit test lulus |
| G5 — Evidence | Hasil, traceability, dan coverage aman dipublikasikan |
| G6 — Integrity | Tidak ada test dihapus, diabaikan, dilemahkan, atau di-rerun untuk menyamarkan failure |
| G7 — Review | Reviewer memeriksa perubahan rule, negative path, dan handoff test |

Lulus Unit Test hanya membuat perubahan **layak digabungkan** pada lapisan Unit. Status ini bukan bukti bahwa fitur telah `DONE`, aman untuk Production, atau telah lulus seluruh test pyramid.

---

# 39. Pencegahan dan Penanganan Flaky Test

## 39.1 Sumber Nondeterminisme

| Risiko | Kontrol |
|---|---|
| Waktu nyata | `FakeClock`/abstraksi waktu |
| Random tanpa kontrol | Seed tetap dan dicatat |
| Network/provider | Fake atau stub; tidak ada network nyata |
| File bersama | Direktori per-test atau in-memory abstraction |
| Global mutable state | Instance/context per-test |
| Urutan test | Setiap test dapat berjalan mandiri dan dalam urutan apa pun |
| Parallel execution | Resource unik dan tidak ada shared write tersembunyi |
| Culture/timezone | Nilai eksplisit pada fixture |
| Async | Selalu di-`await`; tidak menggunakan delay sebagai sinkronisasi |
| Cleanup | Idempotent dan tetap berjalan ketika assertion gagal |
| Platform behavior | Dibungkus typed port; Unit test memakai fake |

## 39.2 Alur Flaky Failure

```text
Preserve failure evidence
→ reproduce deterministically
→ classify root cause
→ fix production code/test infrastructure/test expectation
→ add regression test
→ run affected suite
→ run full Unit suite
```

- Rerun hanya alat diagnosis; failure awal tetap tercatat sebagai failure.
- Test P0/P1 yang flaky memblokir perubahan.
- Isolasi sementara test P2/P3 hanya boleh dengan owner, work item, alasan, dampak, tanggal kedaluwarsa, dan persetujuan.
- Test yang diisolasi tidak dihitung `PASS` dan tidak boleh mengurangi scope gate secara diam-diam.

---

# 40. Strategi Eksekusi Test

| Tahap | Eksekusi |
|---|---|
| Saat mengembangkan | Affected test → affected project → full Unit module |
| Pull request | Restore, Release build, analyzer, seluruh fast Unit suite |
| Main branch | Seluruh Unit suite dan downstream test yang dipicu perubahan |
| Release candidate | Seluruh test pyramid sesuai Document 12 dan dokumen test lanjutan |
| Perubahan dependency/policy | Seluruh Unit suite + security/compatibility test relevan |

Contoh konseptual:

```powershell
dotnet restore <solution>
dotnet build <solution> --configuration Release --no-restore
dotnet test --solution <solution> --configuration Release --no-build <unit-filter-config-report-options>
```

Nama solution, filter, format report, dan opsi MTP final harus mengikuti repository nyata saat scaffolding. Contoh ini bukan kontrak perintah yang boleh ditebak sebelum struktur kode tersedia.

---

# 41. Penggunaan pada CI

```text
Controlled Restore
→ Release Build + Analyzer
→ Fast MSTest Unit Suite pada MTP
→ Security/Static Checks
→ Publish Safe Test Result + Coverage
→ Merge Gate
```

Aturan CI:

- berjalan pada clean agent dengan SDK dan dependency version yang sama dengan baseline;
- gagal jika Unit project tidak ditemukan atau jumlah test nol;
- hasil test, coverage, dan diagnostic artifact harus bebas secret, token, password, document content, serta raw sensitive path;
- kegagalan awal tetap berstatus gagal walaupun diagnostic rerun lulus;
- kegagalan test runner, collector, atau report publisher tidak boleh diperlakukan sebagai test lulus;
- caching tidak boleh mengubah hasil, melewati restore integrity, atau memakai artefak Production sensitif;
- Unit stage harus cukup cepat untuk PR gate, sedangkan test lebih berat masuk stage sesuai jenisnya.

---

# 42. Pelaporan Kegagalan

Failure report minimum memuat:

```text
Unit Test ID
method/test name
Requirement ID
Feature ID bila berlaku
priority
module/component
expected vs actual dalam bentuk aman
environment/configuration version
seed/fake time bila relevan
synthetic Correlation ID bila relevan
downstream test impact
```

Failure report dilarang memuat secret, token, password, document content, full request/response sensitif, raw user path, atau provider detail internal yang tidak diperlukan.

---

# 43. Panduan Implementasi untuk Codex

Setiap task implementasi Unit Test harus menyebutkan:

```text
objective
in-scope module/file
Requirement ID
Feature ID
Unit Test ID/priority
expected behavior dan negative path
allowed production-code change
required commands
expected evidence
downstream handoff
```

Urutan kerja:

1. baca source of truth dan test yang sudah ada;
2. konfirmasi boundary Unit serta owner project;
3. buat/ubah test terkecil yang membuktikan perilaku;
4. ubah production code hanya bila task memberi wewenang;
5. jalankan affected test, affected project, lalu scope gate yang diwajibkan;
6. laporkan command, hasil, failure, coverage, dan test yang belum dapat dijalankan;
7. catat handoff ke test non-Unit.

Codex dilarang:

- menghapus, mengabaikan, atau melemahkan test untuk membuat pipeline hijau;
- menggunakan snapshot besar tanpa assertion semantik;
- menambah package/framework tanpa keputusan terkontrol;
- membuat Requirement ID, Feature ID, error code, schema, policy, atau threshold baru;
- menganggap mock interaction sebagai bukti integrasi nyata;
- menyatakan fitur `DONE` hanya karena Unit Test lulus.

Jika requirement dan expected test bertentangan, hentikan perubahan terkait, simpan bukti, tunjukkan sumber konflik, dan minta controlled decision. Jangan memilih perilaku bisnis sendiri.

---

# 44. Tahap Kesiapan Implementasi

| Tahap | Hasil |
|---|---|
| P0 — Foundation | Struktur Unit project, MTP/MSTest baseline, deterministic fakes, metadata, discovery gate, canary check |
| P1 — First slice | Seluruh contract Unit untuk `FEAT-PDF-005 — Merge PDF` |
| P2 — Shared families | Validator, state machine, resolver, adapter, storage, authorization, audit, dan family-shared rule |
| P3 — Feature-specific | Test khusus seluruh 33 Core Tools berdasarkan family matrix |
| P4 — Hardening | Flaky prevention, parallel safety, coverage baseline/gate, regression set, dan seluruh handoff evidence |

Urutan P0–P4 tidak mengubah prioritas produk: 33 Primary PDF Tool Cards tetap selesai sebelum secondary user tools.

---

# 45. Serah Terima ke Dokumen Test Lanjutan

Unit Test harus menyerahkan test berikut ketika bukti nyata diperlukan:

| Area | Handoff |
|---|---|
| EF Core/SQL Server/SQLite mapping, migration, transaction | Integration Test |
| HTTP serialization, OpenAPI, ProblemDetails, SignalR wire | API/Contract Test |
| Module boundary dan dependency rule | Architecture Test |
| Real authentication middleware/policy enforcement | Authorization/Security Test |
| Desktop/Web/Mobile user journey | E2E/UI Test |
| Large file, concurrency, memory, throughput | Performance Test |
| Malware scanner, encryption, secret vault, penetration | Security Test |
| Windows/platform/engine binary/provider | Compatibility Test |
| Cross-feature historical defect | Regression Test |
| Crash, restart, failover, reconciliation nyata | Recovery/Resilience Test |

Handoff wajib mencatat Requirement ID, Feature ID bila berlaku, risiko, prasyarat, expected evidence, dan dokumen test pemilik.

---

# 46. Kriteria Penerimaan Dokumen 11

```text
[x] seluruh source 01–10 dan MASTER PROJECT SUMMARY diperiksa
[x] seluruh 4/4 specialist report diperiksa dan diaudit silang
[x] 20/20 keputusan Unit Test AUTO-LOCKED tanpa keputusan bisnis baru
[x] baseline 550 canonical features digunakan; angka lama tidak digunakan
[x] 33 Primary PDF Tool Cards dipetakan per keluarga
[x] FEAT-PDF-005 — Merge PDF memiliki spesifikasi lengkap dan 50 case
[x] Unit boundary dan handoff non-Unit jelas
[x] MSTest + Microsoft.Testing.Platform menjadi baseline
[x] naming, AAA, fake/stub/spy, determinism, isolation, dan data strategy ditetapkan
[x] business, validation, state, error, retry, recovery, security, privacy, Local-First,
    Offline, Sync, Resolver, Adapter, API logic, Audit, Assistant, config/policy tercakup
[x] coverage, quality gate, flaky handling, CI, failure report, dan Codex guidance ditetapkan
[x] sign PDF tetap visual signature/paraf NON-PKI; PKI tetap OUT OF SCOPE
[x] tidak ada public AI endpoint atau server-processing untuk local-only baseline
[x] tidak ada endpoint, database schema, error code, atau Feature ID baru yang diciptakan
[x] Documents 01–10 dan output specialist tidak diubah
[x] Bahasa Indonesia ID-1.0 digunakan
```

---

# 47. Status Finalisasi

```text
Document             : 11_UNIT_TEST_DOCUMENT
Status               : READY FOR FINAL v1.0
Decision progress    : 20 / 20 AUTO-LOCKED
Specialist reports   : 4 / 4 INTEGRATED
Canonical features   : 550
Primary PDF tools    : 33 / 33 MAPPED
First coding slice   : FEAT-PDF-005 — Merge PDF
Merge unit cases     : 50 DOCUMENTED
Material conflict    : NONE
User decision needed : NONE
Next document        : 12_TEST_PLAN.md
```
