# AGENT A — FUNCTIONAL, INTEGRATION, CONTRACT & E2E REPORT

> **Task ID:** `DOC12-AGENT-A`  
> **Peran:** Senior System Analyst, Senior .NET Application Architect, Senior Functional Test Engineer, Integration/Contract Test Architect  
> **Status:** Analisis dan rekomendasi untuk penyusunan Document 12; bukan keputusan UAT final  
> **Baseline:** Documents 01–11 FINAL dan Master Project Summary v3.4, snapshot 28 Agustus 2026  
> **Batas:** Tidak mengubah Document 12 master, tidak membuat production code, tidak memilih vendor/tool baru

Laporan ini menggunakan 550 Feature ID canonical, 33 Primary PDF Tool Cards, Offline-First, capability-aware Local-First, REST sebagai source of truth, SignalR sebagai notifikasi, serta `FEAT-PDF-005 — Merge PDF` sebagai vertical slice pertama. Unit Test pada Document 11 tidak diulang; laporan ini menguji wiring, persistence, transport, engine nyata, platform, dan business outcome lintas boundary yang tidak dapat dibuktikan oleh Unit Test.

---

# 1. Functional-test strategy

## 1.1 Rekomendasi utama

Gunakan strategi **risk-based, boundary-first, dan canonical-outcome parity**:

```text
Unit evidence dari Document 11
→ Functional/component behavior dengan komponen nyata terkontrol
→ Integration per boundary
→ Contract compatibility
→ E2E journey representatif
→ Risk-based regression
→ UAT pada Document 13
```

Functional Test tidak cukup membandingkan input dan output file. Test harus membuktikan bahwa:

1. user mencapai business outcome yang benar;
2. Feature ID dan parameter schema canonical yang sama digunakan dari setiap entry flow;
3. permission, policy, capability, processing mode, confirmation, dan resource preflight tidak dibypass;
4. source tidak ditimpa diam-diam;
5. output hanya menjadi final setelah validasi;
6. progress, cancellation, retry, recovery, History, Audit, Job ID, dan Correlation ID benar sesuai konteks;
7. komponen nyata tidak mengubah contract yang telah lulus Unit Test;
8. local-only feature tidak membuat processing upload/API/SignalR;
9. server background operation tidak dinyatakan berhasil hanya karena menerima `202`;
10. Assistant hanya mengorkestrasi application contract resmi dan meng-ground hasil dari state/result aktual.

## 1.2 Lapisan suite

| Lapisan | Tujuan | Komponen nyata minimum | Boundary yang boleh diganti | Bukan bukti |
|---|---|---|---|---|
| Functional/Component | Membuktikan behavior satu capability dari public surface | Application host atau komponen feature, validator, storage/engine adapter yang relevan | Enterprise dependency di luar scope dapat memakai approved test double | Wiring seluruh sistem, Production scale |
| Integration | Membuktikan dua atau lebih komponen berkomunikasi benar | Transport/persistence/filesystem/worker/engine boundary aktual | Dependency eksternal yang tidak dikelola tim dapat disimulasikan pada failure mode terkontrol | UX lintas platform penuh |
| Contract | Membuktikan wire/schema/version tetap kompatibel | Generated OpenAPI, serializer, route binding, typed schema, IPC/event contract | Provider implementation | Business fidelity dokumen |
| E2E | Membuktikan user journey dari entry sampai hasil yang dapat digunakan | Client target, application/API/worker/storage yang relevan, approved engine | SSO/provider tertentu hanya melalui test environment resmi | Seluruh kombinasi 550 fitur × platform × data |
| Regression | Mencegah kembalinya defect atau pelanggaran invariant | Suite terpilih dari seluruh lapisan | Sesuai suite asal | Eksplorasi/UAT baru |

Security, performance/load/soak, malicious corpus, accessibility mendalam, VA/PT, deployment/restore drill, dan UAT tetap mempunyai pemilik suite masing-masing. Document 12 harus mengorkestrasi gate-nya, bukan menyamarkan semuanya sebagai E2E.

## 1.3 Oracle hasil canonical

Setiap test functional yang menjalankan feature menggunakan oracle berikut:

```text
Canonical request
  FeatureId + ParameterSchemaVersion + ordered inputs + parameters
  + actual principal + resource ownership/scope
  + policy/capability/configuration revision
  + processing decision + confirmation/consent facts

Canonical observable outcome
  accepted/rejected decision
  + stable error category/code
  + Job state bila background
  + validated output manifest/artifact
  + source/target integrity
  + History/Audit evidence yang diwajibkan
  + CorrelationId continuity
```

File-First, Tool-First, dan Assistant-First harus menghasilkan oracle yang ekuivalen untuk fakta yang sama. Perbedaan yang diizinkan hanya metadata origin/discovery dan presentasi yang memang spesifik platform; permission, processing decision, Feature ID, parameter meaning, final output rule, dan error semantics tidak boleh berbeda.

## 1.4 Identitas test dan traceability

| Prefix | Suite |
|---|---|
| `FT-<FAMILY>-NNN` | Functional/component |
| `BF-<ENTRY>-NNN` | Business flow/parity |
| `INT-<BOUNDARY>-NNN` | Integration boundary |
| `CT-<CONTRACT>-NNN` | Contract |
| `API-INT-NNN` | API integration |
| `DB-INT-NNN` | SQL Server/SQLite integration |
| `STO-INT-NNN` | Filesystem/object storage integration |
| `WRK-INT-NNN` | Local/Server Worker integration |
| `E2E-<FEATURE>-NNN` | End-to-end |
| `REG-<PACK>-NNN` | Regression pack entry |

Setiap case P0/P1 minimum menyimpan:

```text
Test ID
Requirement ID/range
Feature ID atau resource contract
business/security rule
entry flow dan platform
processing mode yang diharapkan
data-set/corpus version
contract/capability/configuration version
expected outcome dan forbidden side effect
Correlation ID sintetis
evidence location yang aman
defect/work item bila regression
```

## 1.5 Data dan lingkungan test

- Gunakan file sintetis, berlisensi untuk test, dan bebas data Pertamina nyata.
- Dataset memiliki manifest: media type, digest, ukuran, jumlah halaman, encryption state, expected semantic properties, dan classification test.
- Golden file dipakai hanya untuk property yang stabil. Jangan mengharuskan byte-identical output lintas engine bila contract hanya mensyaratkan semantic/fidelity property.
- Simpan corpus valid, malformed, corrupted, encrypted, very-small, multi-page, large, active-content, dan security-negative secara terpisah.
- Password, token, signature asset, OCR text, dan document content tidak boleh masuk report/log/snapshot.
- Setiap test write memakai profile, database, container/store namespace, folder, Job ID, dan Correlation ID unik.
- Clock, locale, timezone, dan connectivity state dinyatakan eksplisit; timeout bukan sinkronisasi utama.
- Test yang memerlukan resource besar atau physical device diberi label dan dijalankan pada lane khusus, tidak dipaksakan ke fast PR lane.

---

# 2. Business-flow scenarios

## 2.1 Kontrak kesetaraan tiga entry flow

Untuk file, principal, policy, capability, dan parameter semantik yang sama:

| Aspek | File-First | Tool-First | Assistant-First | Aturan parity |
|---|---|---|---|---|
| Discovery | Deteksi file dan rekomendasi 3–5 aksi relevan | Search/Tool Card | Intent/alias resolution | Boleh berbeda; Feature ID akhir harus sama |
| Input | File sudah dipilih | File dipilih setelah tool | Resource dipilih/dirujuk oleh user | Ownership, readiness, order, dan digest identity setara |
| Parameter | Preview/editor | Form tool | Typed plan/request | Setelah normalisasi harus setara |
| Authorization | Application service | Application service | Per-step application service | Keputusan identik; Assistant tidak mendapat elevasi |
| Processing | Resolver | Resolver | Resolver per step | User/Assistant tidak memilih engine/provider |
| Confirmation | Berdasarkan tindakan aktual | Sama | Plan preview + aturan risiko yang sama | Tidak boleh disimpulkan oleh Assistant |
| Execution | Tool canonical | Tool canonical | Tool canonical | Handler/application contract yang sama |
| Result | Validated result | Validated result | Grounded validated result | Success/error/state setara |
| Evidence | User Actor, UI origin | User Actor, UI origin | User Actor, `Origin=ASSISTANT` | Audit action/outcome sama; origin boleh berbeda |

## 2.2 Skenario File-First

| ID | Skenario | Expected behavior |
|---|---|---|
| `BF-FILE-001` | Drop/pick satu atau banyak file valid | Fast gate tidak memblokir UI; structural/security validation berjalan sesuai kebutuhan; status readiness jelas |
| `BF-FILE-002` | Jenis/jumlah file dikenali | Sistem hanya merekomendasikan action yang registry-exposed, compatible, permitted, policy-allowed, dan capability-eligible |
| `BF-FILE-003` | User memilih rekomendasi | Rekomendasi menghasilkan Feature ID canonical, bukan alias/endpoint baru |
| `BF-FILE-004` | Preview/edit/reorder/remove | Working state berubah; source bytes tidak berubah; tindakan reversible tidak memunculkan konfirmasi final yang tidak perlu |
| `BF-FILE-005` | User memulai proses | Typed schema, ownership, policy, resource, capacity, consent/confirmation, dan resolver dievaluasi sebelum engine |
| `BF-FILE-006` | Hasil valid | Output validator lulus sebelum publication; Save As/result baru; History dan Audit aman diperbarui |
| `BF-FILE-007` | File invalid atau satu item batch gagal pada operasi atomik | Tidak ada partial final; error aman dan dapat ditindaklanjuti; source/target tetap utuh |
| `BF-FILE-008` | Capability yang direkomendasikan berubah sebelum execute | Revalidation memutuskan Execute/Fallback/Hide/Explain; tidak memakai capability snapshot stale |

## 2.3 Skenario Tool-First

| ID | Skenario | Expected behavior |
|---|---|---|
| `BF-TOOL-001` | Search dengan istilah natural/lokal | Alias hanya membantu discovery dan selalu resolve ke Feature ID canonical |
| `BF-TOOL-002` | Tool dipilih pada platform yang tidak exposed | Tool hidden atau memberi penjelasan/fallback yang disetujui; tidak ada handler bypass |
| `BF-TOOL-003` | Input dimasukkan setelah tool | Input count/type/readiness/ownership divalidasi dengan rule yang sama seperti File-First |
| `BF-TOOL-004` | Parameter default dan explicit | Normalized typed request cocok dengan schema version yang didiscovery |
| `BF-TOOL-005` | Proses immediate | Result hanya `COMPLETED` setelah output valid; Correlation ID berlanjut |
| `BF-TOOL-006` | Proses background | Local Worker atau server Job dipilih oleh Coordinator; background tidak otomatis berarti server |
| `BF-TOOL-007` | Fitur disabled setelah halaman dibuka | Execute ditolak saat revalidation; UI state stale tidak memberi authority |

## 2.4 Skenario Assistant-First

| ID | Skenario | Expected behavior |
|---|---|---|
| `BF-ASST-001` | Intent jelas untuk satu feature | Intent resolve ke Feature ID canonical dan typed parameters; tidak membuat ID/field baru |
| `BF-ASST-002` | Intent ambigu/destruktif/berisiko | Clarification atau plan preview; Assistant tidak mengarang confirmation |
| `BF-ASST-003` | Workflow multi-step | Graph, format antar-step, schema version, permission/policy/ownership/resource/capability diperiksa per step |
| `BF-ASST-004` | Satu step unauthorized | Workflow tidak mengeksekusi step tersebut dan tidak memakai output step lain untuk bypass; error grounded |
| `BF-ASST-005` | Seluruh step local-capable | Application service lokal digunakan; tidak ada HTTP processing/upload/SignalR |
| `BF-ASST-006` | Minimal satu step sah membutuhkan server | Server-owned workflow hanya setelah preflight, consent, dan seluruh guard; satu parent Job dengan persisted step state |
| `BF-ASST-007` | Offline dengan step server-only | Status `ONLINE_REQUIRED`/waiting yang sesuai; tidak auto-upload atau mengubah feature |
| `BF-ASST-008` | Prompt injection dalam PDF/OCR/metadata | Konten dianggap data; policy, tool, parameter, URL, path, provider, dan permission tidak berubah |
| `BF-ASST-009` | Model/provider menyatakan sukses tanpa result aktual | UI menolak klaim; hanya validated result atau authoritative Job state menjadi grounding |
| `BF-ASST-010` | User A menyebut resource User B | Deny/conceal konsisten sebelum content resolution; Assistant tidak mendapat informasi keberadaan |

## 2.5 Skenario parity lintas entry

| ID | Variasi | Assertion utama |
|---|---|---|
| `BF-PARITY-001` | Satu Merge request dari tiga entry | Feature `FEAT-PDF-005`, ordered inputs, parameter meaning, permission, local decision, output property, History/Audit outcome sama |
| `BF-PARITY-002` | Permission dicabut | Ketiga entry menolak saat final authorization; tidak ada engine/Job/publication |
| `BF-PARITY-003` | Feature flag disabled | Ketiga entry tidak dapat mengeksekusi walau screen/plan stale |
| `BF-PARITY-004` | Low disk | Ketiga entry menghasilkan safe capacity outcome yang sama dan source tetap aman |
| `BF-PARITY-005` | Offline local-capable | Ketiganya tetap lokal dengan grant valid; tidak ada server call |
| `BF-PARITY-006` | Offline optional-server feature tanpa local capability | Ketiganya menjelaskan kebutuhan koneksi/wait; tidak auto-upload |
| `BF-PARITY-007` | High-risk finalization | Permission, authority, confirmation/reauth policy, source protection, output validation, dan L3 Audit sama |
| `BF-PARITY-008` | Correlation | Satu logical action memakai Correlation ID konsisten; origin berbeda tidak membuat action kedua |

---

# 3. Integration matrix

## 3.1 Boundary wajib

| ID | Boundary | Real components minimum | Positive evidence | Negative/failure evidence |
|---|---|---|---|---|
| `INT-WEB-API` | Web ↔ API | ASP.NET Core Web host, real HTTP serialization/middleware, Web client adapter | Authenticated discovery/resource command, status/header/body sesuai contract | 401/403/404, CSRF/CORS/Origin sesuai mode auth, malformed/over-limit, no direct SQL |
| `INT-DESK-APP` | Desktop ↔ Application | Desktop presentation adapter + real Application service/shared core | File/Tool command menjadi request canonical dan state UI benar | Disabled/unsupported/stale capability tidak dispatch; error tidak membocorkan path/content |
| `INT-DESK-LWK` | Desktop ↔ Local Worker | Named Pipe/typed versioned IPC, Worker process nyata, local job store | Command, heartbeat, progress, cancellation, result, Correlation ID | Version mismatch, malformed/forged job, crash, disconnect, slow/flood, stale event |
| `INT-API-SQL` | API ↔ SQL Server | API/application infrastructure, EF Core, SQL Server test database | Mapping, constraints, rowversion, transaction, idempotency, outbox | Deadlock/transient, unique conflict, stale rowversion, least-privilege denial, rollback |
| `INT-API-STO` | API ↔ Storage | API streaming adapter + approved test storage/filesystem implementation | Upload staging, digest, READY, result range/download, secured version | Partial write, wrong digest, object missing, orphan metadata/object, expiry, denied access |
| `INT-API-SWK` | API ↔ Worker (Server Worker) | Persisted Job/outbox/dispatch, Worker host, lease, result publisher | Durable `202`, claim, progress, terminal state, retry/resume | Worker crash, lease expiry, duplicate dispatch, stale completion, capacity unavailable |
| `INT-API-HUB` | API ↔ SignalR | Persisted domain state/outbox, Activity Hub, authenticated client | Persist-before-notify, sequence, reconnect + REST reconciliation | Duplicate/stale/gap/unknown event, cross-user subscription, Hub mutation attempt |
| `INT-PROC-ENG` | Processing ↔ Engine Adapter | Coordinator/resolver + approved engine adapter/engine build | Typed input mapping, cancel/deadline, validated output, stable error mapping | Unsupported/corrupt/timeout/crash/OOM/wrong output, forbidden provider fallback |
| `INT-ASST-REG` | Assistant ↔ Feature Registry | Intent/plan adapter, canonical registry, validators/application service | Canonical Feature/parameters/workflow and actual grounded result | Invented/hidden/disabled Feature, schema mismatch, permission bypass, arbitrary provider/path/API |
| `INT-AUD-BIZ` | Audit ↔ Business Action | Feature/resource owner outbox, Audit ingestion/projection, real metadata mapping | Actor/origin/executor/action/outcome/Feature/Job/Correlation correct | Duplicate/conflicting event, offline retry, transaction rollback, canary exclusion, no progress noise |

## 3.2 Prinsip isolasi integration test

1. Setiap boundary memiliki satu owner suite dan fixture lifecycle yang jelas.
2. Test tidak menggabungkan seluruh sistem jika kegagalan dapat dibuktikan pada component boundary yang lebih kecil.
3. SQL Server, SQLite, filesystem, HTTP, IPC, dan SignalR harus diuji dengan implementasi nyata pada suite yang mengklaim boundary tersebut.
4. Engine fake hanya boleh membuktikan orchestration. Suite yang mengklaim fidelity/format/security output harus memakai approved engine nyata dan corpus yang relevan.
5. Test double tidak boleh mereplikasi algoritme provider atau persistence lalu diklaim sebagai Integration Test.
6. Semua retry test mempunyai batas; tidak memakai sleep tanpa observable condition.
7. Semua test menghapus resource miliknya secara idempotent dan tetap mempertahankan failure evidence yang aman.
8. Kegagalan cleanup tidak mengubah test failure menjadi pass.

## 3.3 Chain integration prioritas

| Chain | Tujuan |
|---|---|
| Desktop → Application → Local Worker → Engine → Local Artifact/SQLite | Jalur local-heavy tanpa server |
| Web/Desktop → API → SQL/outbox → Server Worker → Storage → Job REST | Jalur server background berwenang |
| API persisted state → outbox → SignalR → client → REST reconcile | Realtime tidak menjadi authority |
| Business transaction → owner outbox → Audit ingestion | Accepted/terminal evidence tidak hilang atau ganda |
| Assistant → Registry → Application Service → Resolver → actual result | Tidak ada bypass/false grounding |
| Upload → validation → `fileRef` → execute → result manifest/download | File reference lifecycle dan ownership benar |

---

# 4. Contract-test strategy

## 4.1 Sumber contract dan gate OpenAPI

Ikuti Governed Hybrid OpenAPI Document 06:

```text
Typed/versioned C# transport + application contract
→ generate OpenAPI per API major
→ validate document
→ normalize deterministic artifact
→ compare committed snapshot
→ classify breaking/non-breaking
→ serialization + route + validation + example tests
→ handler/authz/completeness tests
```

Normalized OpenAPI artifact, Feature Contract Index, schema example, dan compatibility fixture harus menjadi versioned build outputs. Dokumentasi manual tidak boleh menutupi drift pada generated contract.

## 4.2 Contract suites minimum

| Suite | Assertion |
|---|---|
| `CT-OAS-STRUCTURE` | OpenAPI valid; `/api/v1` surface tepat; explicit `operationId` stabil/unik; schema reference dapat di-resolve |
| `CT-OAS-DIFF` | Route, method, required field, type/format, enum, status, media type, security requirement, dan nullability dibandingkan snapshot |
| `CT-FEATURE-INDEX` | Setiap server-executable Feature memiliki named request/result schema, schema version, handler, authz metadata, risk/exposure; local-only tidak muncul sebagai HTTP processing operation |
| `CT-SERIALIZATION` | JSON casing, nullability, unknown/extra field policy, enum/discriminator, date/time/number bounds, and round trip sesuai contract |
| `CT-VALIDATION` | Syntactic dan semantic errors dipisah; invalid payload tidak mencapai engine/Job/storage |
| `CT-ERROR` | Semua error minimum berisi `code`, `message`, `correlationId`; `details` hanya typed safe union |
| `CT-JOB` | `202` body/header, Job resource/state/version, result manifest, cancel/retry/resume contract konsisten |
| `CT-FILE-REF` | `UPLOAD`, `SECURED_FILE`, `JOB_RESULT` typed opaque refs; path/key/URL arbitrer ditolak |
| `CT-SIGNALR` | Envelope, event catalog/version, sequence, safe payload, unknown version handling, REST recovery |
| `CT-IPC` | Local Worker message/version/operation/progress/error envelope kompatibel dan fail closed |
| `CT-COMPAT` | Current/minimum/contract/capability versions dan status `SUPPORTED`/`UPDATE_AVAILABLE`/`UPDATE_REQUIRED`/`UNSUPPORTED` benar |
| `CT-ASSISTANT` | Assistant request/plan menggunakan schema yang sama; per-step contract dan result grounding diuji |
| `CT-WORKFLOW` | DAG/reference, input-output compatibility, intermediate non-final, parent Job/step state, failure/retry semantics |
| `CT-FORBIDDEN` | Tidak ada token/password/document/OCR/signature/prompt/path/storage key/provider detail pada schema contoh/error/event/evidence |

## 4.3 Typed schema untuk 550 features

Generic feature endpoint aman hanya sebagai **closed typed dispatch**, bukan arbitrary RPC. Contract test harus menjalankan registry-wide data-driven assertions atas seluruh 550 record, tetapi HTTP operation hanya berlaku untuk Feature yang `serverExecutable=true`.

Untuk setiap released/exposed Feature:

```text
FeatureId unique, immutable, canonical
→ descriptor dan ParameterSchemaVersion tersedia
→ input/output contract dapat di-resolve
→ parameter schema closed dan bounded
→ permission/policy/risk/exposure metadata tersedia
→ handler/application contract yang sah tersedia pada mode/platform yang diekspos
→ result/error/audit mapper tersedia
```

Feature local-only tetap memiliki shared typed contract untuk Desktop/Mobile/Assistant/workflow, tetapi tidak dibuatkan server-processing operation. Test harus memastikan tidak ada satu endpoint baru per UI card dan tidak ada 550 controller/route paralel.

## 4.4 Workflow bundle dan Assistant

- Tool Card bundle mengkomposisikan Feature ID canonical; card bukan engine/endpoint baru.
- Setiap step menyatakan `stepId`, Feature ID, schema version, input reference, parameters, dan output dependency.
- Test menolak missing dependency, cycle, duplicate step ID, incompatible media, unavailable Feature, mixed stale schema, dan unauthorized step.
- Preflight menghitung cumulative temp/storage/network/server dependency sebelum execution.
- Semua-local workflow tidak memanggil HTTP processing.
- Server-owned workflow hanya sah bila minimal satu step secara sah menggunakan server dan semua gate lulus.
- Assistant menghasilkan `FeatureExecutionRequest` atau `WorkflowPlan`; output model/provider selalu dianggap untrusted hingga validator/application service menerima.

## 4.5 Breaking dan non-breaking change

| Perubahan | Klasifikasi rekomendasi | Expected gate |
|---|---|---|
| Menghapus/rename route, method, media type, status yang dijanjikan | Breaking | API major baru atau compatibility/migration path approved |
| Menghapus/rename field atau membuat optional menjadi required | Breaking | Block v1 diff |
| Mengubah type/format/meaning, mempersempit numeric/string bound | Breaking bagi client lama | Block atau versioned compatibility path |
| Menghapus/rename enum value atau discriminator | Breaking | Block |
| Menambah required response behavior yang client lama tidak pahami | Breaking/conditional | Compatibility analysis wajib |
| Menambah optional request field dengan safe default | Umumnya non-breaking | Serializer/old-client tests tetap wajib |
| Menambah optional response field pada tolerant clients | Umumnya non-breaking | Supported-client deserialization tests |
| Menambah endpoint/resource baru | Non-breaking bila tidak mengubah yang lama | Authz, schema, docs, security tests wajib |
| Menambah Feature schema version | Non-breaking hanya bila old supported version tetap dilayani/ditolak secara governed | Compatibility matrix |
| Mengubah error code/status untuk kondisi lama | Breaking behavior | Block kecuali versioned migration |
| Menambah SignalR event type/version | Non-breaking hanya jika unknown handling + REST reconcile terbukti | Old-client tests |

## 4.6 Compatibility matrix

Test matrix minimum per supported platform/channel:

| Client | API major | Contract/schema | Expected |
|---|---|---|---|
| Di bawah `MinimumSupportedVersion` | advertised major | any | `UPDATE_REQUIRED`/`UNSUPPORTED`; tidak mengeksekusi secara tidak aman |
| Minimum supported | `v1` | oldest supported | Request/response tetap bekerja atau mendapat migration guidance yang governed |
| Current supported | `v1` | current | Full expected capability sesuai exposure |
| Update available | `v1` | supported older | Tidak dipaksa update kecuali security/compatibility requirement |
| Unsupported API major | unknown | any | Safe rejection; tidak fall through ke handler lain |
| Capability revision berubah | `v1` | supported | Client refresh discovery; stale capability tidak menjadi authority |

`/api/compatibility` tetap di luar route major. API `v1` dan future `v2` diuji berdampingan selama support window bila `v2` kelak resmi dibuat.

---

# 5. API integration tests

## 5.1 Surface dan middleware nyata

API Integration Test memakai in-process/hosted ASP.NET Core API nyata dengan route binding, authentication/authorization middleware, exception mapping, request limits, serializer, EF/storage adapter yang sesuai fixture. Test harus memanggil HTTP; memanggil controller secara langsung tidak membuktikan API integration.

## 5.2 Matrix status wajib

| Status | Kasus minimum | Assertion tambahan |
|---:|---|---|
| `200` | Read atau immediate server command berhasil | Contract/result version dan Correlation ID benar |
| `201` | Upload session/resource dibuat | `Location` wajib dan resource authorized |
| `202` | Job/validation/export background durable | Job ID, status URL, Correlation ID, `Location`; state, attempt, leases, idempotency, reservation, dispatch marker telah commit |
| `204` | Chunk/abort/idempotent action | Body kosong; committed offset/state benar |
| `400` | Malformed JSON/header/query/range/envelope | Safe error; handler/storage/engine tidak dipanggil |
| `401` | Token missing/invalid/expired/revoked atau reauth required | Tidak membocorkan resource; tidak ada content resolution |
| `403` | Permission/ownership/policy deny | Identity valid; authorization dilakukan sebelum count/storage/range/export |
| `404` | Resource absent atau concealed | Konsisten untuk cross-user probe; no existence leakage |
| `409` | Idempotency/state/offset/checkpoint/storage conflict | Typed `CONFLICT`; existing resource tidak berubah |
| `413` | Hard file/chunk/batch/body limit | Stream berhenti bounded; no whole-file buffer/partial final |
| `422` | Semantic/file/business validation | Typed validation issues; no Job/engine/publication |
| `429` | Caller quota/rate/concurrency | `Retry-After`; dibedakan dari shared saturation |
| `500` | Unexpected internal failure | Safe envelope saja; server detail tidak keluar; Correlation ID dapat ditelusuri |
| `503` | Shared worker/provider/system capacity tidak aman | Safe retry guidance bila aman; tidak menerima pekerjaan tanpa durable capacity |

Status lain yang relevan (`206`, `304`, `408`, `410`, `412`, `415`, `416`, `502`, `504`) juga diuji pada resource yang mendukungnya. Matrix di atas adalah minimum eksplisit, bukan daftar eksklusif.

## 5.3 Identifier dan header

| ID | Test |
|---|---|
| `API-INT-001` | `X-Correlation-Id` valid dipertahankan pada response, Job, error, outbox, log/trace aman |
| `API-INT-002` | Correlation ID hilang menghasilkan satu opaque ID; malformed/oversized ditolak/ditangani sesuai contract tanpa echo berbahaya |
| `API-INT-003` | Feature ID alias/case/unknown tidak dispatch ke Feature lain |
| `API-INT-004` | Job ID opaque tidak menjadi authorization proof; cross-user IDOR ditolak/concealed |
| `API-INT-005` | `Idempotency-Key` same key + same fingerprint replay resource/Job yang sama |
| `API-INT-006` | Same key + different fingerprint menghasilkan `409`; different key adalah intent berbeda |
| `API-INT-007` | `If-Match` stale menghasilkan `412`/governed conflict tanpa last-write-wins |
| `API-INT-008` | `X-Client-Platform`/version memengaruhi compatibility/exposure tetapi tidak dipercaya sebagai permission |

## 5.4 Feature execution dan discovery

- `GET /api/v1/features` dan detail memfilter exposure berdasarkan identity, platform, version, policy, flag, dan capability.
- Descriptor visible tidak otomatis berarti ada server handler.
- `POST /api/v1/features/{featureId}/execute` menerima hanya canonical active server-executable Feature dengan matching schema dan authorized READY/AVAILABLE `fileRef`.
- Engine/provider/class/path/URL/SQL/shell/arbitrary field selalu ditolak.
- Untuk 19 local-only Core baseline, client integration membuktikan **zero processing API call**. Direct generic endpoint attempt harus ditolak aman sebelum Job/upload/engine; exact status/error code masih memerlukan governed contract clarification dan tidak ditetapkan oleh laporan ini.
- Untuk 14 optional-server Core paths, test membuktikan local-first lebih dahulu dan server path hanya setelah resolver, policy, classification, capacity, connectivity, notification/consent, serta server revalidation.

## 5.5 File transfer

| Area | Kasus minimum |
|---|---|
| Preflight | Effective mode/limit; no resource/content transfer; tighter system/policy/capacity limit wins |
| One-shot | Streamed multipart bounded; mixed file manifest governed; no whole-file RAM buffering |
| Resumable | Create → sequential chunk → complete → validating → READY; exact offset/digest |
| Retry | Duplicate committed range + same digest di-ack; overlap/wrong offset/digest `409` |
| Validation | MIME/signature/structure/password/decompression/security status; hanya READY executable |
| Ownership | Cross-user upload/session/fileRef rejected before content read |
| Expiry/abort | Expired `410`, abort idempotent, cleanup scheduled, no reusable stale ref |
| Result | HEAD/GET/Range/If-Range/ETag/206/416; `409` not ready; `410` expired; safe filename/type/nosniff |

## 5.6 Job dan SignalR

- `202` diuji dengan fault injection pada setiap persist step; response tidak boleh keluar sebelum durable acceptance lengkap.
- Job state/ETag/stateVersion diuji terhadap concurrent cancel, retry, resume, completion, dan stale progress.
- Logical Job ID tetap ketika attempt bertambah; Correlation ID tetap pada satu logical action.
- SignalR event selalu terbit setelah state persist; delivery duplicate/gap/reorder tidak mengubah authority.
- Reconnect melakukan REST reconcile lalu authorized resubscribe; tidak mengulang execute.
- Hub tidak menerima upload/download/cancel/retry/process command.
- Cross-user group/subscription, forged Job ID, slow consumer, dan revocation setelah connect diuji negatif.

## 5.7 Authorization langsung

Direct API cases wajib mencakup User A terhadap upload/job/result/history/secured/version/issue User B; Administrator terhadap content user; Auditor terhadap write dan export di luar scope; Support terhadap issue/attachment/job yang tidak terkait; stale/revoked permission; disabled account; forged cursor/ETag/fileRef; serta authorization-before-count/range/storage/export. UI hidden bukan bukti security.

---

# 6. Database/Storage integration

## 6.1 SQL Server dan EF Core

Gunakan SQL Server nyata pada database/schema terisolasi per run atau per suite. Mekanisme provisioning tidak ditentukan di laporan ini. Integration suite harus membuktikan:

| ID | Area | Assertion |
|---|---|---|
| `DB-INT-001` | EF mapping | Schema/table, column type/length/Unicode/precision, index, check, delete behavior, rowversion explicit dan sesuai owner module |
| `DB-INT-002` | Module ownership | DbContext tidak dapat menulis tabel modul lain; tidak ada navigation/DbSet lintas modul |
| `DB-INT-003` | PK/unique/FK | Public ID, canonical code, event ID, idempotency scope, stream sequence, dan invariant unik benar di bawah concurrency |
| `DB-INT-004` | Optimistic concurrency | Stale `rowversion` pada Job, Upload, Secured File, Issue, Policy, Feature Flag, Preference, reservation menghasilkan conflict |
| `DB-INT-005` | Job admission | Idempotency + Job + initial attempt + reservation + outbox/dispatch marker commit atomik sebelum `202` |
| `DB-INT-006` | Worker claim | Multi-worker claim/lease tidak mengeksekusi satu attempt dua kali; lease expiry dapat dipulihkan aman |
| `DB-INT-007` | Job completion | Valid artifact ref + terminal state + release reservation + outbox/audit intent konsisten; stale completion ditolak |
| `DB-INT-008` | Outbox | Persist-before-publish, duplicate event idempotent, sequence unik, retry tidak menggandakan business effect |
| `DB-INT-009` | Query | Authorization predicate sebelum count/projection, stable keyset order, cursor scope, bounded filters/sorts, cancellation/timeout |
| `DB-INT-010` | Least privilege | Deployable/owner identity hanya mempunyai grant yang diperlukan; Worker tidak memperoleh arbitrary cross-schema/content access |
| `DB-INT-011` | Failure | Deadlock/transient retry bounded hanya pada command idempotent; rollback tidak meninggalkan half-state |

Jangan menguji SQL Server hanya dengan in-memory repository atau database yang berbeda semantics lalu mengklaim constraint, transaction, locking, query, migration, dan rowversion telah terbukti.

## 6.2 Migration

SQL Server migration tests:

- clean database → current schema;
- setiap supported prior version → current;
- expand-and-contract compatibility selama mixed app window;
- migration history per module/schema;
- module migration tidak mengubah tabel owner lain;
- reference/status historical codes tetap readable;
- large backfill bounded/resumable dan tidak memblokir tak terbatas;
- failure/forward-fix/approved rollback evidence;
- reviewed script/bundle, bukan automatic Production startup migration.

SQLite migration tests memakai database nyata per profile dan mencakup empty/normal/large dataset, draft/undo/checkpoint/pending intent, secured cache, low disk, corrupt row/artifact, crash di tiap fase, account switch, newer unsupported schema, serta unsupported downgrade. Kegagalan tidak boleh membuat database kosong atau menghapus draft/source.

## 6.3 SQLite dan local filesystem

| ID | Area | Assertion |
|---|---|---|
| `DB-INT-L001` | Profile isolation | Satu store per OS profile + Pertamina subject; account switch menutup store lama; tidak implicit merge |
| `DB-INT-L002` | Single writer | Mutation terserialisasi; expected row/operation cursor mencegah last-write-wins |
| `DB-INT-L003` | Artifact ordering | Staging → flush/close → validate/digest → atomic promote → SQLite pointer commit |
| `DB-INT-L004` | Crash recovery | WAL/journal dan recovery scan memulihkan valid state; partial/temp tidak menjadi final |
| `DB-INT-L005` | Selective sync | Hanya execution transfer, explicit secured transfer, Audit metadata outbox, dan allowlisted preference; tidak ada generic sync |
| `DB-INT-L006` | Content exclusion | PDF bytes, password, token, signature asset, OCR text tidak masuk ordinary SQLite row/audit outbox |
| `STO-INT-L001` | Source protection | Success/failure/cancel/crash tidak mengubah source; publish conflict tidak merusak target existing |
| `STO-INT-L002` | Path safety | Traversal/UNC/device/ADS/reparse/symlink/TOCTOU dan cross-job path ditolak oleh real filesystem boundary |

## 6.4 SQL Server ↔ object storage consistency

Test sequence nyata harus mencakup:

```text
stage object
→ validate digest/type/size/security
→ commit metadata/intention
→ promote/finalize object
→ publish availability
→ cleanup/reconcile orphan/tombstone
```

Fault injection dilakukan sebelum/sesudah setiap fase. Assertions:

- tidak ada distributed long transaction SQL + storage;
- metadata tidak menunjukkan `AVAILABLE` jika object belum valid/final;
- object tanpa metadata dan metadata tanpa object terdeteksi dan direkonsiliasi secara idempotent;
- cross-user opaque reference tetap tidak dapat diakses;
- maximum-three active secured versions tetap benar pada concurrent fourth version dan setelah restore;
- tombstone/retention/hold/state mencegah premature deletion;
- restore memvalidasi schema, migration history, grants, outbox/idempotency, object existence/version/digest/key reference/tombstone;
- durasi retensi resmi tidak ditentukan oleh test code tanpa standard Pertamina.

---

# 7. Worker integration

## 7.1 Dua boundary berbeda

| Worker | Authority/transport | Persistence | Progress |
|---|---|---|---|
| Local Worker | Desktop Application melalui typed/versioned Named Pipe/local IPC | Local Job/SQLite + protected artifacts | Typed local IPC ke Activity Center; bukan SignalR |
| Server Worker | Persisted server Job/dispatch/lease | SQL Server + approved secure storage | Persisted state/outbox; SignalR hanya notifikasi |

Test tidak boleh menyamakan background dengan server atau menjalankan Local Worker melalui public/network endpoint.

## 7.2 Local Worker suite

| ID | Skenario | Expected evidence |
|---|---|---|
| `WRK-INT-L001` | Compatible handshake/version | Only supported contract/operation accepted; capability reported correctly |
| `WRK-INT-L002` | Typed command | Feature, schema, ordered refs, Job ID, Correlation ID, cancellation token/deadline diterima tanpa arbitrary command/path/provider |
| `WRK-INT-L003` | Heartbeat/progress | Bounded/coalesced progress; duplicate/stale sequence tidak meregresi terminal state |
| `WRK-INT-L004` | Cancel queued/running | Queued tidak mulai; running berhenti di safe point; temp non-final; source aman |
| `WRK-INT-L005` | Worker crash/disconnect | Desktop tetap hidup; Job menjadi recovery candidate/safe failure; checkpoint/artifact direkonsiliasi |
| `WRK-INT-L006` | Restart/recycle | Compatible Job dapat retry/resume sesuai policy; incompatible checkpoint ditolak |
| `WRK-INT-L007` | Malformed/stale/unknown version | Fail closed, safe error, no engine invocation |
| `WRK-INT-L008` | Forged identity/Job/ref/path | Rejected; Worker tidak membuka arbitrary file/DB/network/token |
| `WRK-INT-L009` | Flood/slow consumer | Bounded queue/backpressure; UI responsive; no memory-unbounded buffering |
| `WRK-INT-L010` | Low disk/OOM/timeout/engine crash | Resource containment dan stable safe mapping; no partial final |
| `WRK-INT-L011` | Parallel jobs | Workspace/temp/output isolation; no cross-job content/result/progress |
| `WRK-INT-L012` | Cleanup | Lease/reference-aware cleanup; active artifact tidak terhapus; orphan dapat retry |

IPC security lane juga mencakup cross-user, remote, same-user rogue process, stale lease, job mismatch, malformed/fuzzed message, dan child-process/resource containment sesuai Security Design.

## 7.3 Server Worker suite

| ID | Skenario | Expected evidence |
|---|---|---|
| `WRK-INT-S001` | Durable dispatch | Worker tidak bergantung koneksi HTTP/SignalR; claim memakai persisted marker/lease |
| `WRK-INT-S002` | Concurrent claim | Satu active owner per attempt; duplicate message/claim tidak menggandakan effect |
| `WRK-INT-S003` | Lease expires | Stale worker completion/progress ditolak; recovery attempt sah dapat mengambil alih |
| `WRK-INT-S004` | Retry policy | Hanya transient allowlist, bounded attempts/backoff; validation/security/permanent failure tidak auto-retry |
| `WRK-INT-S005` | Resume | Feature/schema/input/engine/stage/digest/expiry checkpoint compatibility diverifikasi |
| `WRK-INT-S006` | Result publish | Output validation mendahului artifact available dan Job `COMPLETED` |
| `WRK-INT-S007` | Capacity unavailable | Job menunggu/ditolak sesuai durable state; `503` tidak disalahartikan sebagai caller `429` |
| `WRK-INT-S008` | Revocation/policy change before dispatch | Authorization/policy/capability direvalidasi sesuai contract; stale queue bukan authority |
| `WRK-INT-S009` | Workload identity | Worker permission least-privilege; tidak membawa end-user token atau memperoleh content-access liar |
| `WRK-INT-S010` | Audit/notification | Actual user Actor, Worker Executor; outbox/event aman dan idempotent |

---

# 8. E2E strategy

## 8.1 Tujuan dan batas E2E

E2E membuktikan sedikit journey bernilai tinggi melalui stack aktual, bukan mengulang semua branch Unit/Integration. Pilih kombinasi berdasarkan:

```text
business criticality
+ security/data-loss risk
+ boundary count
+ implementation family
+ processing mode
+ platform exposure
+ PoC uncertainty
+ historical defect
```

E2E sukses harus mengamati result dari public UI/API/resource, bukan internal method call atau database row saja. Database/log dapat dipakai sebagai supporting evidence yang aman, bukan user outcome utama.

## 8.2 Environment lanes

| Lane | Tujuan | Baseline |
|---|---|---|
| E2E-Local-Desktop | Core local dan Local Worker | Windows Desktop, real SQLite/filesystem/IPC, approved local engine, network state controllable |
| E2E-Server | Optional-server/background | Client/Web + API + SQL Server + storage + Server Worker + SignalR/REST recovery |
| E2E-Mobile/Tablet | Curated natural interaction | Camera/share/touch/stylus/scan/sign/light tools sesuai registry exposure |
| E2E-Web | Enterprise/browser-safe | Account/history/secured/support/admin/audit dan server/browser-safe tools yang resmi exposed |
| E2E-Recovery | Crash/reconnect/fault | Controllable API/Worker/SignalR/storage/network disruption |
| E2E-PoC | Engine/device/fidelity uncertainty | Approved target engine/device/platform dan version yang dicatat |

Pertamina SSO memakai identity test environment/subject resmi. Jangan membuat authentication bypass khusus test yang tidak melewati application authorization boundary.

## 8.3 Platform coverage rule

- Desktop adalah minimum utama untuk seluruh Feature yang tidak naturally mobile-only atau masih gated.
- Phone/Tablet diuji untuk feature yang registry expose; scan dan visual sign mendapat prioritas platform-natural.
- Web hanya menguji user tool yang browser-safe/server-capable dan resmi exposed; Web tidak dipakai untuk memaksa local-only processing API.
- Satu shared business-rule suite dipakai lintas platform; test UI platform hanya membuktikan input adapter, projection, capability, lifecycle, dan interaction natural yang berbeda.
- Tidak wajib menjalankan 33 × semua platform bila exposure tidak ada. Registry-wide test harus memastikan setiap exposed combination memiliki owner dan minimum journey.

## 8.4 E2E journey portfolio

Minimum release portfolio:

1. Merge PDF local File-First lengkap.
2. Merge parity Tool-First dan Assistant-First.
3. Satu local background tool melalui Local Worker.
4. Satu optional-server file transfer + Job + SignalR/REST recovery.
5. Satu Office conversion PoC family representative.
6. OCR local/optional-server representative.
7. Satu high-risk Protect/Unlock/Redact finalization path per applicable release.
8. Visual Sign mobile/tablet dan Desktop sesuai exposure, tetap non-PKI.
9. Secured file explicit transfer/version/restore, terpisah dari processing.
10. Auditor scoped query/export dan Support issue flow tanpa content elevation.
11. Assistant multi-step workflow dengan mixed allowed/denied/offline step.
12. Compatibility old-supported/update-required clients dan SignalR reconnect.

## 8.5 Stabilitas dan evidence

- Gunakan observable readiness, stateVersion, event/Job state, atau file lock release; jangan fixed sleep.
- Capture screenshot/video hanya jika diperlukan dan disanitasi; jangan mempublikasikan document content.
- Simpan app/API/worker version, Feature/schema/capability/config revision, platform/OS, engine descriptor, seed/dataset digest, Correlation ID, Job ID, dan safe expected-vs-actual.
- First failure tetap failure; diagnostic rerun tidak mengubah hasil awal menjadi pass.
- Test yang diisolasi tidak dihitung lulus dan wajib memiliki owner, alasan, work item, expiry, serta risk disposition.

---

# 9. 33 Core coverage

## 9.1 Legenda

Platform coverage:

- `D` — Windows Desktop minimum utama.
- `T/P*` — Tablet/Phone hanya bila registry exposure/capability merilisnya; `*` berarti curated.
- `M/T` — Mobile/Tablet mempunyai journey platform-natural yang diprioritaskan.
- `W*` — Web hanya bila browser-safe/server-capable exposure resmi tersedia.

PoC dan server profile mengikuti Documents 05–06. Platform exposure tidak boleh dikarang per card; test matrix executable harus digenerate dari Feature Registry/CapabilityVersion release aktual.

## 9.2 Coverage map

| No | Primary Tool / Feature | Risk | Implementation Family | Processing Mode | Platform minimum | PoC | Reusable coverage + distinctive evidence |
|---:|---|:---:|---|---|---|---|---|
| 1 | Merge PDF — `FEAT-PDF-005` | Low | PDF Page Operations | LOCAL | D + T/P* simple | No | Page-order/source/finality suite + full first-slice E2E |
| 2 | Split PDF — `FEAT-PDF-006/007` | Low | PDF Page Operations | LOCAL | D; T/P* if exposed | No | Page-range suite + split manifest/count |
| 3 | Remove pages — `FEAT-PDF-013` | Low | PDF Page Operations | LOCAL | D; T/P* if exposed | No | Selection/reversible-state suite + minimum remaining page |
| 4 | Extract pages — `FEAT-PDF-014` | Low | PDF Page Operations | LOCAL | D; T/P* if exposed | No | Selection/order suite + extracted page identity |
| 5 | Organize PDF — `FEAT-PDF-010..016` | Low | PDF Page Workflow | LOCAL workflow | D; T/P* if exposed | No | Workflow schema/reorder suite + bundle has no parallel engine |
| 6 | Scan to PDF — `FEAT-SCAN-014` | Low | Scan/Device | LOCAL/device | M/T + D device path | Yes — physical scanner | Device/capability suite + camera/scanner unavailable/cancel/final PDF |
| 7 | Compress PDF — `FEAT-PDF-023` | Low | Optimize | LOCAL | D + T/P* simple | No | Resource/output validation suite + option/size-quality property |
| 8 | Repair PDF — `FEAT-PDF-025/026` | Low | Optimize/Repair | OPTIONAL advanced fallback | D; W* only if exposed | No formal gate | Local-first/fallback suite + repaired structure/no false success |
| 9 | OCR PDF — `FEAT-OCR-004` | Medium | OCR | OPTIONAL complex/large | D; T/P* light; W* if exposed | Yes | OCR adapter/provider suite + searchable result/language/page/fidelity gate |
| 10 | JPG to PDF — `FEAT-CONV-007` | Low | Conversion to PDF | LOCAL | D + T/P* | No | Conversion/common order suite + image order/page dimension |
| 11 | Word to PDF — `FEAT-CONV-001` | Low | Office to PDF | OPTIONAL | D; W* if exposed | Yes | Office-conversion suite + layout/font/active-content corpus |
| 12 | PowerPoint to PDF — `FEAT-CONV-003` | Low | Office to PDF | OPTIONAL | D; W* if exposed | Yes | Office-conversion suite + slide/order/layout fidelity |
| 13 | Excel to PDF — `FEAT-CONV-002` | Low | Office to PDF | OPTIONAL | D; W* if exposed | Yes | Office-conversion suite + sheet/print-area/formula safety |
| 14 | HTML to PDF — `FEAT-CONV-017` | Medium | Web/Text to PDF | OPTIONAL | D; W* if exposed | Yes | Conversion/security suite + no arbitrary remote URL/active content |
| 15 | PDF to JPG — `FEAT-CONV-012` | Low | Conversion from PDF | LOCAL | D; T/P* if exposed | No | Page-selection/image-output suite + count/resolution/color property |
| 16 | PDF to Word — `FEAT-CONV-004` | Low | PDF to Office | OPTIONAL | D; W* if exposed | Yes | Reverse-conversion suite + layout/editability fidelity corpus |
| 17 | PDF to PowerPoint — `FEAT-CONV-006` | Low | PDF to Office | OPTIONAL | D; W* if exposed | Yes | Reverse-conversion suite + slide segmentation/order |
| 18 | PDF to Excel — `FEAT-CONV-005` | Low | PDF to Office/Table | OPTIONAL | D; W* if exposed | Yes | Reverse/table suite + table/cell/formula-injection safety |
| 19 | PDF to PDF/A — `FEAT-PDF-075` | Medium | Conformance Conversion | OPTIONAL | D; W* if exposed | Yes | Conversion/output validator suite + independent conformance evidence |
| 20 | Rotate PDF — `FEAT-PDF-012` | Low | PDF Page Operations | LOCAL | D + T/P* | No | Page-transform/reversible suite + rotation normalization |
| 21 | Add page numbers — `FEAT-PDF-037` | Low | Edit/Annotation | LOCAL | D; T/P* if exposed | No | Overlay/range/preview suite + numbering start/position |
| 22 | Add watermark — `FEAT-PDF-036` | Low | Edit/Annotation | LOCAL | D; T/P* if exposed | No | Overlay/source/finality suite + opacity/position/repeat |
| 23 | Crop PDF — `FEAT-PDF-019` | Low | Edit/Page Geometry | LOCAL | D + T/P* | No | Geometry/preview suite + page-bound clipping and output boxes |
| 24 | Edit PDF — `FEAT-PDF-031..041` | Medium | PDF Editing Workflow | OPTIONAL existing-text subset | D; T/P* limited; W* if exposed | Yes | Working-state/workflow suite + existing-text fidelity/unsupported fallback |
| 25 | PDF Forms — `FEAT-PDF-053..060` | Low | Forms Workflow | LOCAL baseline | D; T/P* if exposed | No formal gate | Field identity/state suite + save/reopen values/action safety |
| 26 | Unlock PDF — `FEAT-PDF-062` | High | PDF Security | LOCAL | D; T/P* if exposed | No | High-risk/auth/password/L3 suite + valid authority only/no bypass |
| 27 | Protect PDF — `FEAT-PDF-061` | Medium | PDF Security | LOCAL | D; T/P* if exposed | No | High-risk/password/finality/L3 suite + reopen access control |
| 28 | Sign PDF — `FEAT-SIGN-013` | Medium | Visual Signature | LOCAL visual non-PKI | M/T + D | No | Asset/privacy/placement/L3 suite + saved visual mark; no PKI claim |
| 29 | Redact PDF — `FEAT-PDF-069` | High | PDF Security/Redaction | LOCAL | D; T/P* only if exposed | No | High-risk confirmation/L3 suite + forensic content removal, not overlay |
| 30 | Compare PDF — `FEAT-PDF-071/072` | Low | Compare/Optimize | LOCAL/Local Worker | D | No | Worker/resource/result suite + comparison provenance/alignment |
| 31 | AI Summarizer — `FEAT-INTEL-001` | Medium | PDF Intelligence | OPTIONAL approved local/internal | D; W*/T/P* if exposed | Yes | AI policy/minimization/grounding suite + selected-page factual provenance |
| 32 | Translate PDF — `FEAT-INTEL-002` | Medium | PDF Intelligence | OPTIONAL approved local/internal | D; W*/T/P* if exposed | Yes | AI policy/fidelity suite + language scope/layout/no public endpoint |
| 33 | PDF to Markdown — `FEAT-CONV-036` | Low | Intelligence/Extraction | OPTIONAL complex OCR/layout | D; W* if exposed | Yes | Deterministic-first extraction suite + heading/table/order/OCR fallback |

## 9.3 Anti-duplication model

Coverage setiap card adalah:

```text
registry-wide completeness
+ one implementation-family conformance suite
+ one processing-mode suite
+ platform adapter suite hanya untuk exposed combination
+ feature-specific distinctive scenario
+ PoC/security suite bila ditandai
```

Contoh: Split, Remove, Extract, Organize, dan Rotate tidak menyalin ulang semua source/finality/authorization test Merge. Shared PDF Page Operations suite dijalankan terhadap adapter descriptor masing-masing, lalu setiap Feature menambah hanya behavior pembeda. Sebaliknya, Redact tidak boleh hanya “mewarisi” generic overlay suite karena true content removal dan final confirmation adalah risiko spesifik.

---

# 10. Merge PDF full scenarios

## 10.1 Baseline dan prasyarat

`FEAT-PDF-005` adalah LOCAL baseline, risiko Low, input dua atau lebih PDF berurutan, output satu PDF atomik. Immediate local atau background Local Worker dipilih Coordinator berdasarkan karakteristik aktual. Tidak ada processing API atau SignalR untuk baseline Merge. Local Job ID hanya ada bila background; Correlation ID selalu ada. Accepted dan terminal L2 Audit evidence tetap diwajibkan Document 10, termasuk melalui protected offline outbox.

Fixture E2E minimum:

- Desktop build dan exposed Merge Feature descriptor yang sama dengan release;
- approved real PDF engine version yang dicatat;
- real SQLite, protected local workspace/temp filesystem, dan Local Worker;
- controllable network, disk quota/fault boundary, dan process termination;
- synthetic PDF corpus dengan manifest page markers/digest;
- safe History/Audit evidence inspector;
- source/target pre/post digest verifier.

## 10.2 Skenario E2E

| ID | Skenario | Langkah lintas boundary | Expected outcome/evidence |
|---|---|---|---|
| `E2E-MRG-001` | Happy path File-First | Drop A,B → detect PDF[] → rekomendasi Merge → preview order → execute → validate → Save As | Satu valid PDF berurutan A,B; source digest sama; History + L2 accepted/terminal; Correlation continuous; no API/upload/SignalR |
| `E2E-MRG-002` | Happy path Tool-First | Search “gabung PDF” → `FEAT-PDF-005` → pilih A,B → process → save | Canonical request dan semantic output setara `001`; alias hanya discovery |
| `E2E-MRG-003` | Happy path Assistant-First | “Gabungkan A lalu B” → plan preview → typed request → application service → result grounding | Feature/schema/order/auth/resolver sama; Actor User, Origin Assistant; no engine/provider selection by Assistant |
| `E2E-MRG-004` | Invalid file | Batch A valid + file ekstensi PDF tetapi signature/structure invalid | Validation berhenti sebelum engine; operasi atomik gagal; no Job/final/history success; error aman; source utuh |
| `E2E-MRG-005` | Reorder | Pilih A,B,C → reorder C,A,B → preview → execute | Page markers/provenance menunjukkan C,A,B; UI order, engine request, output order konsisten |
| `E2E-MRG-006` | Remove input | Pilih A,B,C → remove B → execute | Output hanya A,C; B tidak diubah/dihapus; bila tersisa satu file execute disabled/rejected |
| `E2E-MRG-007` | Multiple files dan explicit duplicate | A,B,C,... plus A occurrence kedua → background/local sesuai resource | Semua occurrence tepat satu per daftar, duplicate tidak silent-dedupe, stable order, atomic result |
| `E2E-MRG-008` | Cancel queued | Paksa Local Worker queue → cancel sebelum claim | State local `CANCELLED`; engine tidak mulai; no final; temp/lease dibersihkan aman |
| `E2E-MRG-009` | Cancel running | Mulai Merge besar → cancel saat progress → wait safe point | UI tetap hidup; terminal `CANCELLED`; no late completed/progress regression; source aman; temp non-final |
| `E2E-MRG-010` | Engine failure | Fault approved test engine/adapter pada middle stage | Stable safe failure; no raw engine detail; no partial final; retry hanya bila classified transient dan bounded |
| `E2E-MRG-011` | Engine unavailable | Remove/disable approved compatible local provider | `WAITING_RESOURCE`/safe unavailable yang governed; tidak otomatis pindah ke server |
| `E2E-MRG-012` | Low disk preflight | Quota temp/output di bawah kebutuhan sebelum execute | `INSUFFICIENT_STORAGE`; engine/publisher tidak berjalan; source/target utuh |
| `E2E-MRG-013` | Disk habis saat staging/publish | Sediakan preflight cukup lalu inject disk failure pada controlled phase | Tidak ada half-final; old target/source utuh; cleanup/reconciliation recoverable |
| `E2E-MRG-014` | Large files | Corpus besar memicu Local Worker, monitor responsive UI/streaming/resource | Tidak whole-file buffer bila streaming tepat; stable local Job ID; progress/cancel; tetap zero server call |
| `E2E-MRG-015` | Save conflict | Existing target dipilih tanpa overwrite contract → pilih nama baru | Conflict aman; existing target digest tidak berubah; output hanya dipublish ke target baru setelah user action |
| `E2E-MRG-016` | Source protection | Target sengaja sama dengan A; ulang pada success/failure/cancel/crash | Baseline Save As menolak; A/B digest sama pada seluruh outcome; tidak ada destructive open/write |
| `E2E-MRG-017` | Output validation failure | Engine test menghasilkan truncated/wrong page-count output | `RESULT_VALIDATION_FAILED`/governed equivalent; temp tidak final; tidak dapat dibuka dari History sebagai success |
| `E2E-MRG-018` | Reopen result | Selesaikan dan Save As → tutup screen/app → buka hasil dari filesystem/recent/history action yang sah | PDF dapat dibuka dan manifest/page order benar; History metadata menunjuk hasil tanpa menyimpan bytes/raw sensitive path |
| `E2E-MRG-019` | History lifecycle | Success, failure, cancel; buka/delete personal History item | History outcome benar dan delete hanya History; enterprise Audit/result/source tidak ikut terhapus |
| `E2E-MRG-020` | Offline behavior | Valid protected offline grant + local capability → disconnect sebelum discovery/execute | Merge tetap lokal; no network processing; Audit metadata antre aman; reconnect revalidates before sync, no content upload |
| `E2E-MRG-021` | Offline grant invalid | Expired/tampered/wrong profile grant | Fail closed sebelum content/engine; local file tidak diunggah; account switch tidak mengirim queue profil lama |
| `E2E-MRG-022` | Worker crash dan recovery | Kill Local Worker saat Merge besar → restart app/worker | Desktop tidak crash; Job/checkpoint/temp direkonsiliasi; resume/retry hanya jika compatible; no false completed |
| `E2E-MRG-023` | Encrypted input | Password required → wrong password → correct ephemeral password | Wrong password safe; correct may process; password absent from SQLite/log/History/Audit/report/checkpoint |
| `E2E-MRG-024` | Correlation/evidence | Jalankan background Merge lalu telusuri UI/local job/log/History/Audit outbox | One Correlation ID; Job ID hanya local job; Actor/origin/outcome correct; no content/path/password canary |
| `E2E-MRG-025` | Capability/flag stale | Buka Merge lalu disable feature/change capability revision sebelum execute | Final revalidation rejects/explains; no engine call; stale UI descriptor bukan authority |

## 10.3 Property verification hasil

Jangan hanya memeriksa bahwa file “ada”. Minimum verifier membuka hasil menggunakan parser/validator independen yang disetujui bila memungkinkan dan memeriksa:

- media/signature/structure valid;
- expected total pages;
- ordered page marker/provenance;
- no missing/extra/duplicated occurrence selain duplicate explicit;
- source digest unchanged;
- target atomicity dan absence of temp/partial publication;
- password/security property sesuai skenario;
- result dapat dibuka ulang;
- no provider-specific byte-equality requirement kecuali engine contract memang menguncinya.

---

# 11. Regression packs

## 11.1 Core Smoke Pack

Tujuan: feedback cepat bahwa release build dapat menjalankan jalur utama tanpa pelanggaran besar.

Isi minimum:

1. compatibility/discovery/registry mapping 33/33 dan handler/exposure completeness untuk Feature yang dirilis;
2. satu happy-path representative per implementation family;
3. Merge File-First primary happy path;
4. Tool/File/Assistant parity untuk satu local feature;
5. satu Local Worker background path;
6. satu optional-server `202 → Job → validated result` path bila server scope dirilis;
7. SSO/current-user authorization dan one cross-user denial;
8. source/finality/correlation/History/Audit canary assertions;
9. SignalR reconnect + REST state untuk server background representative;
10. startup/migration/readiness check pada supported upgrade path.

Pack ini berjalan pada PR/main/release sesuai biaya lane. Feature PoC yang belum dirilis tidak dipaksa menjadi smoke-pass palsu; ia mempunyai explicit gated disposition.

## 11.2 Core Regression Pack

Mencakup:

- seluruh Core Smoke;
- family conformance suite terhadap 33 card mappings;
- feature-specific distinctive scenario dari tabel Section 9;
- three-entry parity, Local-First, Offline, validation, source/finality, cancellation, recovery;
- SQL/SQLite mapping, constraints, migrations, transactions, outbox, object consistency;
- API file/Job/idempotency/query/status contract;
- Local/Server Worker crash/lease/retry/resume;
- cross-platform exposed combinations;
- every fixed Core defect as permanent regression.

Jalankan affected pack pada PR dan full pack pada main/nightly/release candidate berdasarkan durasi. Pembagian lane tidak boleh menghilangkan release evidence.

## 11.3 High-Risk Regression Pack

Prioritas:

- Unlock `FEAT-PDF-062` — authority/password only, no bypass, no secret persistence;
- Protect `FEAT-PDF-061` — confirmation/reauth policy, access property, recovery;
- Visual Sign `FEAT-SIGN-013` — asset isolation, placement, privacy, explicit non-PKI meaning;
- Redact `FEAT-PDF-069` — final confirmation dan forensic content removal;
- Administrator/Support/Auditor content boundary dan IDOR;
- Assistant prompt/tool/permission/provider boundary;
- HTML/PDF Intelligence/public-provider blocking;
- secured permanent delete/version/restore and Audit access/export;
- canary scan across error/log/trace/event/DB/export/test artifacts;
- malicious/edge corpus handoff ke Security suite.

Semua P0 high-risk cases wajib lulus tanpa quarantine-as-green sebelum release terkait.

## 11.4 API Contract Regression Pack

- generated OpenAPI validation + normalized diff;
- stable unique `operationId`;
- route/method/security/status/header/media/schema/enum/nullability diff;
- server Feature schema/handler/authz completeness;
- local-only absence from processing operations;
- supported old client and compatibility status matrix;
- error envelope/status mapping;
- fileRef/upload/result/Job/idempotency contracts;
- SignalR event envelope/version/gap/reconcile;
- typed workflow/Assistant contract;
- forbidden-field/example/canary checks;
- consumer deserialization for each supported client platform/version.

Contract pack memblokir merge atas breaking drift yang tidak mempunyai API major/compatibility change resmi.

## 11.5 Trigger selection

| Change | Pack minimum |
|---|---|
| Feature/domain/application | Affected Unit + family functional + Core Smoke + feature regression |
| DTO/route/schema/serializer | API Contract + API Integration + supported-client compatibility |
| EF/migration/query | DB Integration + migration paths + affected API/E2E |
| Worker/IPC/engine | Worker Integration + family fidelity + crash/recovery |
| Auth/permission/policy | High-Risk + direct API + three-entry/Assistant parity + SignalR/Worker negatives |
| Storage/file lifecycle | Storage Integration + source/finality + recovery/restore |
| Registry/capability/exposure | Registry-wide 550/33 checks + platform discovery/E2E selection |
| Defect fix | Reproducing permanent test pada lapisan terendah yang tepat + relevant pack |

---

# 12. Entry/exit criteria

## 12.1 Entry criteria umum

```text
[ ] Requirement ID, Feature ID/resource contract, risk, owner, dan platform exposure diketahui
[ ] expected public behavior dan forbidden side effect dapat diuji
[ ] typed contract/schema/version tersedia untuk scope yang dirilis
[ ] build deployable dan Unit P0/P1 affected lulus
[ ] test environment, identity, permission/policy, data corpus, dan engine version terkendali
[ ] SQL/storage/worker/IPC/API dependency sehat atau failure mode sengaja diaktifkan
[ ] cleanup, isolation, evidence redaction, dan Correlation ID strategy tersedia
[ ] PoC prerequisite lulus atau test diberi status BLOCKED dengan keputusan resmi—bukan PASS
```

## 12.2 Exit criteria per lapisan

| Lapisan | Exit minimum |
|---|---|
| Functional | Semua in-scope P0/P1 dan primary/negative business path lulus; source/finality/parity terbukti |
| Integration | Real boundary success/failure/concurrency/recovery lulus; tidak ada forbidden bypass atau orphan tidak terdeteksi |
| Contract | OpenAPI/schema/IPC/event valid; no unapproved breaking diff; supported-client matrix lulus |
| E2E | Journey target pada platform/exposure lulus dengan validated result dan evidence aman; no false success |
| Regression | Semua affected packs lulus; no deleted/weakened/ignored test; known failure mempunyai approved disposition |
| Release handoff | Security/performance/recovery/cross-platform/documentation evidence terkumpul; UAT tetap menunggu Document 13 |

## 12.3 Gate kegagalan

- P0/P1 failed atau flaky memblokir scope terkait.
- Zero test discovery, runner/collector failure, missing environment evidence, atau corrupted report adalah failure, bukan pass.
- Breaking contract diff tanpa approved version/compatibility path memblokir merge.
- Source corruption, unauthorized content access, public AI route, secret/content leakage, false `COMPLETED`, missing mandatory Audit, atau duplicate durable effect selalu release blocker.
- PoC-gated feature tidak boleh diberi `DONE` bila fidelity/security/resource/platform gate belum lulus.
- Numeric response/performance/scale threshold tidak dikarang. Baseline diukur, target diajukan dari benchmark dan approved capacity/SLA; sampai itu tersedia, statusnya `REQUIRES_BENCHMARK_AND_APPROVAL`.

## 12.4 Batas dengan UAT

Exit Document 12 berarti bukti technical-functional-integration-contract-E2E memadai untuk diserahkan ke UAT. Ia tidak menetapkan user acceptance final, business sign-off, Production approval, atau menggantikan Document 13.

---

# 13. Risks/gaps

## 13.1 Risiko dan gap terbuka

| ID | Risiko/gap | Dampak pada test | Rekomendasi |
|---|---|---|---|
| `R-01` | Exact platform exposure per Feature belum berupa matrix executable | Coverage lintas Desktop/Tablet/Phone/Web dapat over/under-test | Generate release test matrix dari registry + `CapabilityVersion`; review terhadap Catalog baseline |
| `R-02` | Banyak Feature/engine masih PoC-gated | Golden/fidelity oracle belum stabil | Catat engine/version/corpus; PoC gate terpisah; jangan menganggap generic adapter pass sebagai fidelity pass |
| `R-03` | Numeric performance/SLA/resource threshold belum di-approved | Exit performance tidak dapat diberi angka final | Benchmark representative workloads lalu controlled approval sesuai `NFR-PERF-006` |
| `R-04` | Exact status/error code untuk direct generic execute pada local-only Feature tidak dinyatakan | Contract test tidak boleh memilih 404/409/422 sendiri | Addendum compatible Document 06/error catalog sebelum implementation; sementara assert safe reject + zero Job/upload/engine |
| `R-05` | Document 10 memerlukan addendum batch Audit dan field tertentu pada Documents 06–07 | Contract/DB tests tidak dapat mengunci exact wire/column sebelum addendum | Selesaikan controlled addendum tanpa route/API-major baru; lalu snapshot/compatibility tests |
| `R-06` | Exact complete stable Error Code catalog belum terlihat untuk semua failure | Test rentan membandingkan pesan atau mengarang code | Governed typed error catalog; message localization diuji terpisah dari stable code |
| `R-07` | Test identity/SSO environment, revocation hooks, dan account fixtures belum ditetapkan | Auth/E2E dapat memakai bypass tidak representatif | Provision Pertamina-approved QA identities dan lifecycle controls; no test-only authorization bypass |
| `R-08` | Approved storage/engine/provider products belum dipilih untuk semua lane | Integration/fidelity semantics berbeda antar implementation | Contract suite vendor-neutral + capability-specific PoC per approved implementation |
| `R-09` | Office/PDF/A/OCR/AI fidelity tidak dapat dinilai hanya dengan file existence | False green | Property-based/golden semantic oracle + independent conformance/parser where approved + human review sample pada PoC |
| `R-10` | E2E kombinasi 33 × data × platform × mode sangat besar | Runtime dan maintenance tidak terkendali | Registry/family/processing/platform combinatorial selection; exhaustive pada contract/unit, representative pada E2E |
| `R-11` | Real file/process/network fault injection dapat flaky | False failure atau hidden race | Deterministic controllable fault points, observable state, unique resources, preserve first failure |
| `R-12` | Large/malicious corpus dapat membebani PR lane | Feedback lambat | Pisahkan fast PR, scheduled heavy, PoC/security/load lanes tanpa mengurangi release gate |
| `R-13` | Audit L2 untuk semua 33 Core menambah offline/outbox failure surface | Missing/duplicate evidence atau user blocking | Integration test atomic intent, idempotent ingestion, non-blocking sync, canary exclusion |
| `R-14` | Local Job dan server Job dapat tercampur pada telemetry/test assertions | Merge dapat salah dianggap perlu API/SignalR | Distinct fixture/contract; Local Worker → IPC/SQLite, Server Worker → REST/SQL/outbox/SignalR |
| `R-15` | Byte-identical result lintas approved engines mungkin tidak deterministik | Brittle golden tests | Assert semantic properties, conformance, page order/content provenance; byte equality hanya bila contract mengharuskan |
| `R-16` | Restore/deployment/VA/PT/UAT berada di dokumen/suite lain | Salah klaim coverage Document 12 | Trace explicit handoff dan release gate; jangan menamai smoke E2E sebagai restore/VA/PT/UAT |

## 13.2 Konflik/ambiguitas sumber yang ditemukan

1. **Contoh SignalR menggunakan Merge PDF.** Envelope pada Document 06 Section 17.2 memakai `FEAT-PDF-005`, sementara tabel API Core Section 19 secara spesifik menetapkan Merge sebagai `LOCAL`, Processing API `No`, SignalR `No`. Interpretasi aman: contoh envelope bersifat ilustratif terhadap bentuk event, bukan otorisasi SignalR untuk Merge. Test Merge wajib memakai local IPC. Disarankan contoh future revision memakai Feature optional-server agar tidak ambigu.
2. **Profil `L1` generik vs mapping Merge spesifik.** Feature Catalog menjelaskan `L1` sebagai local preferred dengan optional server fallback, sedangkan locked Core API mapping menetapkan Merge LOCAL. Mapping feature-specific yang lebih baru/spesifik digunakan: tidak ada server processing untuk Merge baseline. Perubahan harus melalui controlled contract/design decision.
3. **Audit lama yang mungkin dianggap hanya History.** Final Document 10 mengunci seluruh 33 Core menghasilkan L2 accepted/terminal evidence; karena itu Merge E2E menguji History dan Enterprise Audit sebagai plane terpisah. Progress/reorder/preview tetap tidak menjadi Audit noise.
4. **Audit addendum belum diterapkan ke parent contract/schema.** Document 10 secara eksplisit menyerahkan compatible Document 06–07 addenda. Ini bukan alasan mengarang payload/column pada Document 12.

Tidak ditemukan konflik material lain yang memaksa perubahan architecture lock. Rekomendasi test mempertahankan REST authority, SignalR notification-only, Local-First, client no-direct-SQL, module ownership, dan Assistant no-bypass.

## 13.3 Pertanyaan yang perlu keputusan/konfirmasi terkontrol

1. Status HTTP dan stable Error Code apa yang resmi untuk direct attempt menjalankan Feature local-only pada `/api/v1/features/{featureId}/execute`?
2. Feature-by-platform exposure matrix release pertama mana yang menjadi input executable Test Plan?
3. Supported client/version window apa yang harus ada dalam compatibility regression matrix?
4. Benchmark workload dan approved numeric thresholds apa yang akan mengunci response, queue, memory, file-size, concurrency, cleanup, dan recovery gates?
5. Engine/provider/device build mana yang disetujui untuk PoC/fidelity lanes dan corpus lisensi apa yang boleh disimpan di repository/test storage?
6. Controlled addendum Audit batch/field mana yang selesai lebih dahulu agar Contract/DB tests dapat di-freeze?
7. Environment dan lifecycle Pertamina SSO QA apa yang tersedia untuk revoke/disable/reauth/offline/reconnect E2E?

---

# 14. Suggested Document 12 text

## 14.1 Teks rekomendasi inti

> **Strategi Pengujian Functional, Integration, Contract, dan E2E**  
> PERTAMINA OTO menggunakan pendekatan risk-based, boundary-first, dan canonical-outcome parity. Unit Test membuktikan rule/handler/validator secara isolated. Functional Test membuktikan business behavior dari public component surface. Integration Test membuktikan HTTP, SQL Server, SQLite, filesystem/object storage, Local/Server Worker, engine adapter, SignalR, Assistant Registry, dan Audit boundary dengan implementation nyata terkontrol. Contract Test membuktikan OpenAPI, typed Feature schema, fileRef, Job, IPC, SignalR event, compatibility, serialization, error, dan status semantics. E2E hanya menguji user journey bernilai tinggi melalui stack aktual dan tidak menggantikan exhaustive lower-level tests, Security/Performance/PoC/UAT.

> **Kesetaraan Triple Entry Flow**  
> File-First, Tool-First, dan Assistant-First wajib resolve ke Feature ID canonical, ParameterSchemaVersion, application service, permission/policy/ownership, capability/resolver, confirmation/consent, output validation, History/Audit, dan error/result semantics yang sama. Perbedaan hanya boleh pada discovery/presentation dan Origin metadata. Assistant tetap memakai User sebagai Actor, tidak memilih engine/provider, tidak direct SQL/filesystem/shell/arbitrary API, dan hanya menampilkan keberhasilan yang grounded pada validated result atau authoritative Job state.

> **Batas Local dan Server**  
> Background work tidak identik dengan server work. Local Worker memakai typed/versioned local IPC, SQLite/local artifact state, dan local Activity Center; tidak memakai server processing API atau SignalR. Server Worker memakai durable Job, SQL Server/outbox, approved storage, REST state, dan SignalR notification. Untuk seluruh local-only Feature, client test membuktikan zero processing upload/API/SignalR. Optional-server path hanya berjalan setelah resolver, permission, policy, classification, capability, capacity, connectivity, notification/consent, dan server revalidation.

> **OpenAPI dan Compatibility Gate**  
> Typed/versioned C# transport dan application contract adalah implementation source. OpenAPI digenerate, divalidasi, dinormalisasi, disimpan per API major, dibandingkan dengan snapshot, dan dianalisis untuk breaking change. Setiap server-executable Feature mempunyai named request/result schema, handler, authorization metadata, dan test. Local-only Feature tetap berada pada shared Feature Contract Catalog tetapi tidak menjadi HTTP processing operation. Breaking route/schema/status/enum/meaning change memerlukan API major baru atau approved compatibility/migration path.

> **API Integration Minimum**  
> API Integration Test menggunakan ASP.NET Core host dan HTTP middleware nyata. Test mencakup `200`, `201`, `202`, `400`, `401`, `403`, `404`, `409`, `413`, `422`, `429`, `500`, dan `503`, serta status resource-specific lain. Setiap error minimum memiliki `code`, `message`, dan `correlationId`; details hanya typed safe union. `202` hanya boleh dikembalikan setelah Job, initial attempt, input lease, idempotency, reservation, dan dispatch marker/state durable. Job ID, Feature ID, Correlation ID, fileRef, ETag, cursor, dan opaque resource ID tidak pernah menjadi authorization proof.

> **Persistence dan Storage Integration**  
> SQL Server Integration Test memakai SQL Server nyata untuk membuktikan EF mapping, module-owned schema, PK/FK/unique/check, rowversion, transaction, idempotency, lease, outbox, query authorization, migration, dan least privilege. SQLite/local filesystem test membuktikan per-profile isolation, single writer, artifact-first atomic publication, migration/crash recovery, dan selective sync. SQL Server dan object storage tidak memakai long distributed transaction; staging, validation, metadata commit, promotion, outbox, cleanup, dan reconciliation diuji dengan fault injection.

> **33 Core Coverage**  
> Coverage 33 Primary PDF Tool Cards menggunakan kombinasi registry-wide completeness, implementation-family conformance, processing-mode tests, platform adapter tests untuk exposure aktual, feature-specific distinctive scenario, dan PoC/security tests. Pendekatan ini mencegah duplikasi test tanpa mengurangi evidence feature-specific. Feature-platform matrix dihasilkan dari registry/capability release; platform exposure tidak mengubah Feature ID atau business rule.

> **Merge PDF Vertical Slice**  
> `FEAT-PDF-005` diuji end-to-end pada File-First, Tool-First, dan Assistant-First untuk happy path, invalid file, reorder, remove, multiple/duplicate occurrence, cancellation, engine unavailable/failure, low disk, large files, save conflict, source protection, output validation, reopen result, History, Audit, offline grant, Worker crash/recovery, encrypted input, Correlation ID, dan stale capability. Merge tetap LOCAL baseline; no server processing API/SignalR. Source digest tidak berubah dan output hanya final setelah independent property validation.

> **Regression Packs**  
> Empat pack minimum adalah Core Smoke, Core Regression, High-Risk Regression, dan API Contract Regression. P0/P1 failure atau flaky test, unauthorized content access, source corruption, false success, mandatory Audit loss, public AI route, sensitive-data leakage, duplicate durable effect, atau unapproved breaking contract change memblokir release scope terkait. Rerun hanya diagnosis dan tidak menghapus first failure.

> **Entry, Exit, dan Handoff**  
> Test dimulai setelah requirement/Feature/risk/owner/exposure, versioned contract, environment, identity, corpus, dependency, cleanup, dan evidence strategy tersedia. Exit memerlukan seluruh applicable P0/P1, business/negative paths, real boundary evidence, compatibility, E2E journey, dan regression pack lulus. Numeric performance thresholds berasal dari benchmark dan controlled approval. Lulus Document 12 adalah handoff ke UAT; bukan user acceptance atau Production approval final.

## 14.2 Suggested acceptance checklist

```text
[ ] File-First, Tool-First, Assistant-First canonical parity terbukti
[ ] 33/33 Core card mapping dan family coverage mempunyai evidence/disposition
[ ] local-only processing menghasilkan zero upload/API/SignalR
[ ] optional-server path melewati seluruh resolver/policy/consent gate
[ ] OpenAPI normalized diff dan supported-client compatibility lulus
[ ] API status/error/identifier/idempotency/file/job contracts lulus
[ ] SQL Server/SQLite/storage mapping, transaction, migration, concurrency, recovery lulus
[ ] Local Worker/Server Worker crash, cancel, retry, resume, lease, isolation lulus
[ ] SignalR duplicate/gap/reconnect memulihkan state dari REST
[ ] source tidak ditimpa diam-diam dan output hanya final setelah validation
[ ] History, Audit, Observability, dan Support evidence tetap terpisah dan aman
[ ] Merge PDF full E2E scenarios lulus pada target build
[ ] Core Smoke, Core Regression, High-Risk, dan API Contract packs lulus
[ ] tidak ada password/token/document/OCR/signature/prompt/path/provider secret pada evidence
[ ] PoC, performance, security, compatibility, recovery, dan UAT handoff tercatat
[ ] test failure/flake/quarantine tidak disamarkan sebagai PASS
```

---

**Kesimpulan Agent A:** baseline terbaik untuk Document 12 adalah test portfolio berlapis yang mengutamakan kesetaraan business outcome dan bukti boundary nyata. Coverage luas diperoleh melalui registry/family/contract tests; E2E tetap sedikit namun bernilai tinggi. Strategi ini menjaga 550 Feature tetap terkelola, memberi coverage eksplisit untuk 33 Core, dan menjadikan Merge PDF bukti vertical slice lengkap tanpa menciptakan server API untuk feature lokal atau mengulang 50 Unit cases Document 11.
