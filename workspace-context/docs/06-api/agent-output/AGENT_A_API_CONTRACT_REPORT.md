# AGENT A — API CONTRACT REPORT

**Task ID:** DOC06-AGENT-A  
**Scope:** API-DEC-001, API-DEC-002, API-DEC-003  
**Status:** ANALYSIS / RECOMMENDATION ONLY — TIDAK MENGUNCI KEPUTUSAN  
**Tanggal analisis:** 24 Agustus 2026

Dokumen ini merupakan bahan keputusan untuk Document 06. Dokumen ini tidak mengubah architecture lock, tidak memilih vendor, tidak membuat production code, dan tidak mengubah master Document 06.

## 1. Executive recommendation

1. **API-DEC-001 — rekomendasi baseline: C, Hybrid API Surface.** Gunakan canonical feature execution contract hanya untuk tool processing yang benar-benar dieksekusi melalui server. Gunakan resource/domain API untuk account, compatibility, feature discovery, jobs, history, secured files, issues/support, audit, administration, dan health. Keputusan tetap **OPEN**.
2. Generic feature endpoint dapat aman pada katalog 550 feature hanya apabila endpoint tersebut merupakan **closed dispatch ke handler yang terdaftar dan bertipe**, bukan arbitrary remote procedure call. Tidak semua dari 550 record boleh dieksekusi melalui endpoint tersebut karena katalog juga berisi workflow, enterprise, platform, security, observability, dan Assistant capabilities.
3. **Local-only feature tidak mempunyai API processing.** Desktop/Mobile memanggil application contract lokal yang sama; HTTP hanya dipakai ketika canonical registry, resolver, policy, capability, dan consent menetapkan server execution.
4. Parameter setiap FeatureId harus mempunyai schema bertipe dan terversi. Generic envelope tidak boleh diteruskan sebagai dictionary dinamis sampai engine. FeatureId harus di-resolve ke request/result type serta application handler yang spesifik.
5. Core Tool Card yang merupakan bundle tidak memperoleh endpoint per card. Bundle menjadi workflow plan berisi langkah canonical FeatureId, parameter schema version, dan input/output binding.
6. Assistant menghasilkan canonical FeatureExecutionRequest atau WorkflowPlan yang sama seperti UI. Assistant tidak mempunyai jalur eksekusi istimewa dan tidak boleh memanggil database, engine, shell, file system, atau arbitrary API secara langsung.
7. **API-DEC-002 — rekomendasi: Governed Hybrid OpenAPI.** Typed C# contracts menjadi sumber implementasi; OpenAPI dibuat otomatis; snapshot/diff, contract test, contoh, compatibility policy, dan breaking-change gate menjadi kontrol governance.
8. **API-DEC-003 — rekomendasi: major version pada URI untuk functional API, yaitu /api/v1, ditambah bootstrap compatibility endpoint yang stabil.** App version, ContractVersion, ParameterSchemaVersion, WorkflowSchemaVersion, dan CapabilityVersion tidak dimasukkan ke URI.
9. REST tetap menjadi source of truth. SignalR hanya menyampaikan event; semua status job harus dapat dipulihkan dari REST dengan JobId.

BLOCKED_BY_DECISION: API-DEC-001

Rekomendasi route execution dan workflow di laporan ini adalah rancangan kandidat. Route tersebut tidak boleh dianggap locked sebelum API-DEC-001 diputuskan user.

## 2. Decision analysis

### 2.1 API-DEC-001 — Tool / Feature API Surface

| Kriteria | A — Endpoint per tool | B — Generic feature execute | C — Hybrid |
|---|---|---|---|
| Kesesuaian dengan 550 canonical features | Rendah; berisiko menyamakan record katalog dengan endpoint | Sedang; ringkas tetapi berbahaya bila semua record dianggap executable | Tinggi; hanya processing server memakai generic execute |
| Kesesuaian dengan 33 Core Tool Cards | Berisiko membuat endpoint per card/bundle | Baik untuk direct tool, kurang natural untuk resource lifecycle | Baik; direct tool dan bundle tidak menduplikasi service |
| Typed schema/OpenAPI | Sangat mudah per endpoint | Memerlukan schema registry/discriminator dan generated components | Dapat mempertahankan typed feature schema sekaligus resource schema |
| Assistant parity | Berisiko mapping khusus untuk banyak endpoint | Baik bila memakai canonical FeatureId | Paling baik; Assistant memakai contract yang sama tanpa memaksa semua domain menjadi generic |
| Local-First | Berisiko setiap tool terlihat harus mempunyai server endpoint | Aman hanya dengan explicit server-executable allowlist | Paling jelas: local application contract terpisah dari server transport |
| Authorization dan policy | Jelas per endpoint | Wajib dispatch policy per FeatureId; satu kelemahan dapat berdampak luas | Jelas pada resource API dan tetap terkontrol pada feature execution |
| Maintainability | Endpoint/DTO/operationId sprawl | Surface kecil tetapi registry/schema governance lebih berat | Seimbang |
| Rekomendasi | Tidak direkomendasikan sebagai baseline | Tidak direkomendasikan sebagai satu-satunya surface | **Direkomendasikan, belum locked** |

#### Apakah generic endpoint aman untuk 550 features?

**Aman secara bersyarat, bukan aman secara otomatis.** Syarat minimum:

1. Generic route hanya menerima FeatureId canonical yang aktif dan mempunyai explicit server handler registration.
2. Kelayakan server execution ditentukan dari canonical registry, execution profile, ServerDependency, feature status, permission, policy, capability, resource, engine availability, dan connectivity. Record dengan ServerDependency=None tidak otomatis memperoleh HTTP processing.
3. Enterprise capability, resource lifecycle, platform behavior, security control, observability, dan Assistant capability tidak dipaksa masuk ke generic feature execution.
4. Route dispatcher menggunakan pasangan bertipe, misalnya konsep IFeatureHandler<TParameters, TResult>. Setelah FeatureId dipilih, parameters harus di-deserialize ke TParameters; dictionary/object bebas tidak boleh diteruskan ke engine.
5. Unknown FeatureId, feature yang tidak diekspos ke API, atau feature tanpa registered handler ditolak sebelum detail schema/engine dibuka.
6. Authorization dilakukan pada application boundary untuk setiap FeatureId, bukan hanya sekali di generic controller.
7. Handler tidak menerima nama service, engine, assembly, class, SQL, path lokal, URL arbitrary, atau command dari client.
8. Limit, quota, file validation, upload consent, concurrency, backpressure, audit, dan telemetry diterapkan berdasarkan metadata FeatureId.
9. Result di-ground ke output application service yang aktual. HTTP layer dan Assistant tidak boleh mengarang keberhasilan.
10. Contract test memastikan setiap server-executable FeatureId mempunyai schema, handler, authorization policy, status mapping, dan test.

Generic endpoint **tidak aman** apabila implementasinya berubah menjadi RPC universal dengan bentuk parameters bebas, reflection berdasarkan input client, dynamic engine name, atau akses arbitrary service.

#### Typed parameter schema

Setiap FeatureId harus dipetakan ke:

- ParameterSchemaVersion;
- typed parameter DTO;
- typed result DTO atau typed result descriptor;
- input/output contract;
- required/optional fields;
- enum dan numeric/string constraints;
- additionalProperties=false secara default;
- destructive/risk metadata;
- permission/policy requirement;
- registered application handler.

Urutan validasi yang direkomendasikan:

1. Parse envelope dan header.
2. Validasi canonical FeatureId dan API exposure tanpa membocorkan detail yang tidak boleh dilihat.
3. Validasi ContractVersion dan ParameterSchemaVersion.
4. Authentication, permission, policy, feature flag, dan exposure check.
5. Deserialize ke typed parameters dan jalankan schema validation.
6. Validasi input handle, file signature, MIME, size, ownership, dan compatibility.
7. Resource/capability/engine/connectivity preflight.
8. Upload/content consent bila server memerlukan isi dokumen.
9. Pilih immediate atau background execution.
10. Validasi output sebelum status final.

ParameterSchemaVersion direkomendasikan memakai pola MAJOR.MINOR:

- perubahan dokumentasi/contoh tanpa perubahan validasi tidak perlu menaikkan versi schema;
- MINOR untuk optional field baru dengan default yang stabil atau constraint yang dilonggarkan;
- MAJOR untuk rename/remove field, perubahan type/meaning, optional menjadi required, constraint yang diperketat secara incompatible, atau perubahan default yang mengubah hasil;
- schema lama yang masih berada dalam compatibility window tetap diterima atau dimigrasikan secara eksplisit;
- preset dan workflow harus menyimpan FeatureId serta ParameterSchemaVersion yang digunakan.

#### Workflow bundle

Primary Tool Card seperti Organize PDF, Edit PDF, dan PDF Forms adalah presentation/workflow bundle, bukan FeatureId baru dan bukan alasan membuat endpoint per card.

Canonical workflow plan minimum:

- WorkflowSchemaVersion;
- ordered steps;
- stepId lokal terhadap plan;
- canonical FeatureId per langkah;
- ParameterSchemaVersion per langkah;
- typed parameters;
- input binding dari workflow input atau output langkah sebelumnya;
- output declaration;
- risk/confirmation summary yang dihitung sistem;
- execution preflight result.

Aturan eksekusi:

- jika seluruh langkah local-capable dan policy mengizinkan, workflow berjalan melalui local application services tanpa API processing;
- jika ada langkah server-required atau server dipilih resolver secara sah, hanya data yang diperlukan dikirim setelah consent/policy check;
- server-owned background workflow memperoleh satu JobId induk; status langkah berada di bawah persisted job state;
- intermediate output bersifat temporary/non-final sampai output validation selesai;
- default failure behavior adalah berhenti aman pada langkah gagal; tidak ada asumsi transaksi lintas engine;
- retry/resume mengikuti capability langkah dan checkpoint, bukan mengulang seluruh workflow secara membabi buta;
- Tool-First, File-First, dan Assistant-First menghasilkan plan dengan format yang sama.

#### Assistant memakai contract yang sama

Assistant hanya boleh menghasilkan dua artefak aplikasi:

1. satu FeatureExecutionRequest yang menunjuk FeatureId canonical; atau
2. satu WorkflowPlan yang tersusun dari FeatureId canonical.

Artefak tersebut harus melewati validator, capability/permission/policy check, resource preflight, confirmation rule, dan application service yang sama dengan UI. Assistant tidak mengirim arbitrary URL atau memilih nama engine. Internal AI hanya membantu intent/parameter/plan; output AI tetap untrusted sampai tervalidasi. Hasil yang disampaikan Assistant berasal dari actual result atau persisted job state.

#### Resource API yang tidak boleh generic

Area berikut harus tetap berbentuk resource/domain API karena mempunyai identity, lifecycle, ownership, query, concurrency, atau authorization semantics sendiri:

- compatibility dan capability discovery;
- session/account/profile;
- jobs, status, cancel, retry/resume;
- upload/file transfer dan server file handles;
- secured files, versions, download, retention, dan account recycle lifecycle;
- history;
- enterprise audit;
- issues, attachments, user issue status, triage, dan support export;
- administration: feature flags, policy, limits, quota, maintenance, minimum app version;
- configuration dan preferences;
- health/liveness/readiness;
- internal AI gateway bila kelak digunakan, yang tetap bukan arbitrary client API.

### 2.2 API-DEC-002 — OpenAPI / contract strategy

| Opsi | Kelebihan | Risiko | Penilaian |
|---|---|---|---|
| A — Code-first saja | Cepat dan dekat dengan implementasi | Contract drift dapat lolos sebagai perubahan kode biasa | Tidak cukup untuk compatibility governance |
| B — Contract-first saja | Review contract eksplisit sebelum coding | Beban sinkronisasi manual tinggi bagi satu developer | Terlalu berat sebagai baseline tunggal |
| C — Governed Hybrid | Typed implementation + generated spec + diff/test | Memerlukan pipeline discipline | **Direkomendasikan, belum locked** |

Rekomendasi Governed Hybrid:

1. Typed C# request/response/application contracts menjadi sumber implementasi.
2. OpenAPI dibuat otomatis per API major dari endpoint dan schema metadata.
3. Normalized OpenAPI snapshot disimpan sebagai contract artifact dan dibandingkan dengan baseline yang disetujui.
4. Pipeline menjalankan breaking-change diff, serialization/validation test, representative request/response contract test, dan test setiap registered feature handler.
5. OperationId stabil dan tidak bergantung nama controller/class internal.
6. Common components minimum: ErrorResponse, AcceptedJobResponse, JobStatusResponse, CompatibilityResponse, paged response, feature execution envelope, WorkflowPlan, serta schema parameter/result setiap server-executable FeatureId.
7. OpenAPI hanya mendokumentasikan HTTP API. Local-only feature tidak dipublikasikan seolah mempunyai processing endpoint.
8. Feature Contract Catalog dapat mempublikasikan schema semua capability yang diperlukan Shared Core, tetapi harus dibedakan jelas dari HTTP operation catalog.
9. Contoh tidak boleh memuat isi dokumen, token, password, signature asset, OCR text lengkap, path lokal, atau data Production.
10. Dokumentasi Production harus mengikuti access policy; health detail dan internal admin schema tidak otomatis dibuka publik.

Catatan untuk generic endpoint: OpenAPI tidak secara alami memilih schema berdasarkan path parameter. Karena itu rekomendasinya adalah common transport envelope ditambah generated named components untuk parameter/result FeatureId dan machine-readable feature contract index. Runtime tetap melakukan typed dispatch. Jangan menyederhanakan OpenAPI dengan menjadikan parameters sebagai object bebas tanpa schema governance.

### 2.3 API-DEC-003 — major API version routing

| Strategi | Kelebihan | Kekurangan | Penilaian |
|---|---|---|---|
| URI major: /api/v1 | Jelas bagi client, proxy, log, OpenAPI, dan coexistence | Major tampak di URL | **Direkomendasikan** |
| Media type/header versioning | URL bersih | Operasional/debugging/code generation lebih kompleks | Tidak direkomendasikan untuk baseline |
| Tanpa major route, handshake saja | Surface sederhana saat hanya satu versi | Sulit menjalankan v1 dan v2 bersamaan | Tidak direkomendasikan |

Rekomendasi:

- functional API memakai /api/v1;
- bootstrap GET /api/compatibility dibuat stabil agar client lama dapat mengetahui versi dan route major yang didukung;
- hanya breaking API change yang menaikkan route major;
- perubahan additive compatible tetap berada di v1 dan menaikkan ContractVersion bila perlu;
- AppVersion, ParameterSchemaVersion, WorkflowSchemaVersion, ConfigurationSchemaVersion, dan CapabilityVersion tidak dimasukkan ke URI;
- OpenAPI dipublikasikan per major;
- v1 dan v2 dapat hidup berdampingan sampai support window dan MinimumSupportedVersion mengizinkan penghentian v1;
- forced update hanya untuk security/compatibility requirement yang nyata.

Definisi compatibility field yang direkomendasikan:

| Field | Makna |
|---|---|
| CurrentVersion | Versi client terbaru yang disetujui untuk platform/channel peminta |
| MinimumSupportedVersion | Versi client terendah yang masih dapat memakai server |
| ContractVersion | Revisi penuh contract dalam API major aktif |
| CapabilityVersion | Revisi snapshot canonical feature registry, exposure, dan capability metadata |

Compatibility status minimum: SUPPORTED, UPDATE_AVAILABLE, UPDATE_REQUIRED, UNSUPPORTED.

## 3. Proposed routes

Seluruh route di bawah adalah **proposal**, bukan keputusan locked. Placeholder {major} digunakan sampai API-DEC-003 diputuskan.

### 3.1 Bootstrap dan system

| Method | Route | Tujuan |
|---|---|---|
| GET | /api/compatibility | Handshake versi stabil lintas major |
| GET | /health/live | Liveness minimal |
| GET | /health/ready | Readiness; detail Production dibatasi |

### 3.2 Feature discovery dan server processing

BLOCKED_BY_DECISION: API-DEC-001

| Method | Route | Tujuan |
|---|---|---|
| GET | /api/{major}/features | Capability/feature metadata yang telah difilter sesuai platform, permission, policy, dan exposure |
| GET | /api/{major}/features/{featureId} | Contract descriptor dan schema version untuk FeatureId yang boleh dilihat |
| POST | /api/{major}/features/{featureId}/execute | Eksekusi satu canonical feature yang memang melewati server |
| POST | /api/{major}/workflows/execute | Eksekusi server-owned canonical multi-step workflow; bukan endpoint per Tool Card |

GET feature discovery tidak menjadikan local-only feature sebagai server-processing operation. POST execute hanya tersedia secara logis untuk FeatureId yang mempunyai registered server handler dan lolos policy.

### 3.3 Jobs

| Method | Route | Tujuan |
|---|---|---|
| GET | /api/{major}/jobs/{jobId} | Persisted current state; REST source of truth |
| POST | /api/{major}/jobs/{jobId}/cancel | Safe cancel; bukan delete resource |
| POST | /api/{major}/jobs/{jobId}/retry | Retry bila status dan policy mengizinkan |
| GET | /api/{major}/jobs | Activity Center/personal job query dengan pagination |

Retry/resume detail harus diselaraskan dengan API-DEC-005 Idempotency & Retry Contract; route di atas belum locked.

### 3.4 Resource/domain route families

| Route family | Bentuk yang direkomendasikan |
|---|---|
| Account/session | /api/{major}/account/... |
| History | /api/{major}/history/... |
| Secured files dan versions | /api/{major}/secured-files/... |
| Issues milik user | /api/{major}/issues/... |
| Issue attachment | /api/{major}/issues/{issueId}/attachments/... |
| Support/Superadmin | /api/{major}/support/issues/... |
| Audit | /api/{major}/audit/... |
| Admin/policy/configuration | /api/{major}/admin/... |

File upload/download route final ditunda ke API-DEC-004. Pagination/filter/sort final ditunda ke API-DEC-006. Authentication header/claim detail ditunda ke API-DEC-007.

## 4. Request/response examples

### 4.1 Standard server feature execution request

BLOCKED_BY_DECISION: API-DEC-001

Contoh ini hanya berlaku jika resolver/policy telah memilih server execution. Contoh ini **bukan** kewajiban API untuk feature yang dapat dan harus berjalan local-only.

~~~http
POST /api/v1/features/FEAT-CONV-001/execute
Authorization: Bearer <approved-token>
X-Correlation-Id: corr_01J...
Content-Type: application/json
~~~

~~~json
{
  "contractVersion": "1.0",
  "parameterSchemaVersion": "1.0",
  "inputs": [
    {
      "inputId": "source-1",
      "fileRef": "file_01J..."
    }
  ],
  "parameters": {
    "outputFileName": "dokumen.pdf",
    "pageRange": null
  }
}
~~~

FeatureId pada path adalah authoritative. fileRef merupakan opaque server-side handle; bentuk upload/temporary handle final menunggu API-DEC-004. Client tidak boleh mengirim local filesystem path atau nama engine.

### 4.2 Immediate success

~~~http
HTTP/1.1 200 OK
X-Correlation-Id: corr_01J...
~~~

~~~json
{
  "featureId": "FEAT-CONV-001",
  "contractVersion": "1.0",
  "parameterSchemaVersion": "1.0",
  "status": "COMPLETED",
  "result": {
    "primaryOutput": {
      "fileRef": "file_01K...",
      "mediaType": "application/pdf",
      "sizeBytes": 245901
    }
  },
  "correlationId": "corr_01J..."
}
~~~

Gunakan 200 untuk command yang selesai segera dan mengembalikan hasil. Gunakan 201 hanya apabila sebuah resource addressable baru benar-benar dibuat dan response mengandung Location.

### 4.3 Background accepted

~~~http
HTTP/1.1 202 Accepted
Location: /api/v1/jobs/job_01J...
Retry-After: 3
X-Correlation-Id: corr_01J...
~~~

~~~json
{
  "jobId": "job_01J...",
  "featureId": "FEAT-CONV-001",
  "status": "QUEUED",
  "statusUrl": "/api/v1/jobs/job_01J...",
  "correlationId": "corr_01J..."
}
~~~

202 berarti request diterima, bukan berarti proses berhasil. Keberhasilan hanya dinyatakan setelah GET job menunjukkan COMPLETED dan output telah tervalidasi.

### 4.4 Workflow plan shared by UI and Assistant

~~~json
{
  "workflowSchemaVersion": "1.0",
  "steps": [
    {
      "stepId": "merge",
      "featureId": "FEAT-PDF-005",
      "parameterSchemaVersion": "1.0",
      "inputs": [
        { "from": "workflow.inputs.pdfFiles" }
      ],
      "parameters": {
        "orderedInputIds": ["pdf-1", "pdf-2"]
      }
    },
    {
      "stepId": "number",
      "featureId": "FEAT-PDF-037",
      "parameterSchemaVersion": "1.0",
      "inputs": [
        { "from": "steps.merge.outputs.primary" }
      ],
      "parameters": {
        "startNumber": 1,
        "position": "BOTTOM_CENTER"
      }
    },
    {
      "stepId": "compress",
      "featureId": "FEAT-PDF-023",
      "parameterSchemaVersion": "1.0",
      "inputs": [
        { "from": "steps.number.outputs.primary" }
      ],
      "parameters": {
        "qualityPreset": "BALANCED"
      }
    }
  ]
}
~~~

Plan yang seluruh langkahnya local-capable dijalankan lokal. Tidak ada HTTP processing hanya karena plan dibuat Assistant.

### 4.5 Standard error

~~~http
HTTP/1.1 422 Unprocessable Content
X-Correlation-Id: corr_01J...
~~~

~~~json
{
  "code": "FEATURE_PARAMETER_INVALID",
  "message": "Parameter feature tidak valid.",
  "correlationId": "corr_01J...",
  "details": [
    {
      "field": "parameters.pageRange",
      "issue": "PAGE_RANGE_INVALID"
    }
  ]
}
~~~

code, message, dan correlationId selalu ada. details opsional dan hanya berisi safe structured validation information. Stack trace, class name, query SQL, path internal, token, dan isi dokumen dilarang.

### 4.6 Compatibility handshake

~~~http
GET /api/compatibility
X-Client-Platform: WINDOWS_DESKTOP
X-Client-Version: 1.3.0
X-Correlation-Id: corr_01J...
~~~

~~~json
{
  "currentVersion": "1.4.0",
  "minimumSupportedVersion": "1.2.0",
  "contractVersion": "1.3",
  "capabilityVersion": "2026.08.24.1",
  "status": "UPDATE_AVAILABLE",
  "supportedApiMajors": ["v1"],
  "defaultApiMajor": "v1",
  "correlationId": "corr_01J..."
}
~~~

### 4.7 HTTP status semantics

| Status | Semantics PERTAMINA OTO | Contoh code |
|---:|---|---|
| 200 | Read/command selesai sukses; tidak menyatakan resource baru bila sebenarnya hanya hasil command | — |
| 201 | Resource addressable baru dibuat; wajib Location | ISSUE_CREATED, SECURED_FILE_CREATED |
| 202 | Background work diterima; wajib JobId, status URL, dan CorrelationId | JOB_ACCEPTED |
| 400 | JSON/header/query/envelope salah secara sintaksis atau FeatureId path/body mismatch bila kelak body discriminator dipakai | REQUEST_INVALID |
| 401 | Token tidak ada, invalid, expired, atau session tidak valid | AUTHENTICATION_REQUIRED |
| 403 | User terautentikasi tetapi permission/policy melarang aksi | FEATURE_FORBIDDEN |
| 404 | Resource/FeatureId tidak ditemukan atau sengaja tidak diekspos untuk mencegah information disclosure | RESOURCE_NOT_FOUND |
| 409 | Konflik dengan current resource/job/version state; termasuk optimistic concurrency atau aksi tidak valid terhadap status sekarang | JOB_STATE_CONFLICT |
| 413 | Request/file/attachment melewati size limit sebelum atau selama streaming | PAYLOAD_TOO_LARGE |
| 422 | Request sintaksis valid tetapi parameter, input/output compatibility, file semantics, atau business validation gagal | FEATURE_PARAMETER_INVALID |
| 429 | Rate/quota/concurrency/backpressure limit terlampaui; sertakan Retry-After bila aman/diketahui | RATE_LIMITED, QUEUE_CAPACITY_REACHED |
| 500 | Kegagalan internal tak terduga; tanpa stack trace ke client | INTERNAL_ERROR |
| 503 | Maintenance, dependency/worker/storage/engine sementara tidak siap, atau kapasitas layanan tidak tersedia | SERVICE_UNAVAILABLE |

Perbedaan penting:

- 400 untuk bentuk request yang tidak dapat dipahami; 422 untuk bentuk valid tetapi makna feature tidak valid.
- 409 untuk konflik state saat ini, bukan untuk semua validation error.
- 429 untuk pembatasan terhadap caller/tenant/quota/queue admission; 503 untuk service/dependency yang secara umum tidak siap.
- Retry-After direkomendasikan pada 429 dan 503 bila estimasi aman tersedia.

### 4.8 Identifier contract

#### FeatureId

- canonical, immutable, case-stable, dan tidak pernah didaur ulang;
- format saat ini mengikuti FEAT-{DOMAIN}-{NNN}, misalnya FEAT-PDF-005;
- bukan database key dan bukan label UI;
- dipakai konsisten oleh UI, Assistant, workflow, permission, audit, telemetry, engine resolver, dan contract registry;
- alias hanya untuk discovery; request execution harus memakai canonical FeatureId.

#### JobId

- diterbitkan server untuk setiap background server job dan diterbitkan local coordinator untuk local background job dalam boundary lokal;
- opaque, unik, tidak memuat user, tenant, database identity, atau engine name;
- digunakan pada REST status, SignalR event, Activity Center, recovery, issue diagnostics, dan support;
- kepemilikan JobId selalu diverifikasi; mengetahui JobId tidak memberikan authorization.

#### CorrelationId

- satu identifier opaque untuk menelusuri satu user action/workflow lintas client, API, application service, Worker, storage, log, telemetry, SignalR, dan issue diagnostics;
- client boleh meneruskan nilai yang valid; server membuat nilai baru jika tidak ada/invalid;
- selalu dikembalikan pada response header dan wajib berada dalam error envelope;
- bukan authentication token, bukan authorization proof, bukan JobId, dan tidak boleh memuat data pribadi;
- seluruh langkah workflow dapat berbagi CorrelationId, sedangkan setiap background job tetap mempunyai JobId sendiri.

## 5. OpenAPI recommendation

1. Gunakan satu generated OpenAPI document per supported API major.
2. Generate dari typed transport DTO dan endpoint metadata; jangan generate dari engine/provider classes.
3. Generate named schema components untuk setiap server-executable FeatureId parameters/result.
4. Generate feature contract index yang memetakan FeatureId ke ParameterSchemaVersion, input/output contract, schema component, risk, dan compatibility metadata.
5. Stabilkan operationId secara eksplisit.
6. Simpan normalized snapshot sebagai reviewed contract artifact.
7. Pipeline minimum:
   - schema generation;
   - OpenAPI validation;
   - breaking-change diff;
   - request/response serialization tests;
   - contract tests terhadap running API;
   - registered-handler completeness test;
   - representative 200/201/202/error examples;
   - security/log redaction tests.
8. Perubahan breaking memblokir merge kecuali task secara eksplisit menaikkan API major atau menyediakan compatibility adapter/migration path.
9. Local-only feature schema boleh berada pada shared Feature Contract Catalog, tetapi tidak boleh muncul sebagai HTTP processing operation.
10. SignalR event schema didokumentasikan terpisah dan harus membawa JobId/CorrelationId; REST job schema tetap authoritative.

## 6. Versioning recommendation

### 6.1 Layer version yang harus dipisahkan

| Layer | Contoh | Kapan berubah |
|---|---|---|
| AppVersion | 1.4.0 | Release client |
| API route major | v1 | Breaking HTTP surface |
| ContractVersion | 1.3 | Revisi compatible atau breaking yang dikelola dalam/antar-major |
| ParameterSchemaVersion | 2.0 per FeatureId | Perubahan parameter feature |
| WorkflowSchemaVersion | 1.1 | Perubahan workflow plan structure |
| ConfigurationSchemaVersion | 1.0 | Perubahan configuration payload |
| CapabilityVersion | 2026.08.24.1 | Perubahan registry/exposure/capability snapshot |

### 6.2 Breaking change

Perubahan berikut dianggap breaking atau potentially breaking:

- menghapus/rename route, method, request/response field, FeatureId, atau operationId yang dipakai client;
- mengubah type, meaning, format, default behavior, atau HTTP status semantics;
- optional field menjadi required;
- memperketat constraint sehingga request lama ditolak;
- menghapus accepted enum value;
- menambah enum value apabila client contract memakai closed/exhaustive enum dan tidak mempunyai unknown fallback;
- mengubah input/output contract FeatureId;
- menaikkan ParameterSchemaVersion major tanpa compatibility handler;
- mengubah destructive flag, permission boundary, atau ServerDependency dengan dampak client/workflow;
- mengubah background operation menjadi immediate-only atau sebaliknya tanpa compatible envelope;
- mengubah identity/lifecycle semantics resource;
- menghentikan v1 sebelum support window dan MinimumSupportedVersion mengizinkan.

Penanganan: API major baru, compatibility adapter, dual-read/dual-accept sementara, explicit migration untuk preset/workflow, deprecation notice, contract test, dan regression test.

### 6.3 Non-breaking change

Perubahan berikut dapat non-breaking apabila memenuhi aturan:

- menambah optional response field dan client wajib mengabaikan field yang tidak dikenal;
- menambah optional request field dengan default yang tidak mengubah request lama;
- menambah endpoint baru;
- menambah FeatureId/capability baru tanpa mengubah contract lama;
- melonggarkan constraint;
- menambah error code apabila client memiliki unknown-code fallback;
- memperbaiki deskripsi/contoh tanpa mengubah runtime semantics;
- mengubah feature flag/exposure tanpa mengubah contract, dengan CapabilityVersion baru.

Setiap perubahan tetap melewati OpenAPI diff, impact analysis, dan regression test. Label non-breaking tidak berarti bebas review.

### 6.4 Deprecation

- Tandai endpoint/field deprecated pada OpenAPI dan release notes.
- Ukur pemakaian melalui telemetry tanpa isi dokumen.
- Pertahankan selama support window.
- Sediakan replacement dan migration guidance.
- Hapus hanya pada major berikutnya atau setelah seluruh MinimumSupportedVersion yang bergantung padanya tidak lagi didukung.

## 7. Risks

| Risiko | Dampak | Mitigasi |
|---|---|---|
| Generic endpoint berubah menjadi arbitrary RPC | Critical security/architecture bypass | Closed handler registry, typed dispatch, allowlist, architecture test |
| Semua 550 feature dianggap server executable | Melanggar Local-First dan memperbesar surface | Pisahkan Feature Catalog dari HTTP operation catalog |
| parameters tetap object/dictionary bebas | Validation drift dan injection | Typed DTO, additionalProperties=false, schema/handler completeness test |
| Authorization hanya di controller generic | Satu kelemahan membuka banyak feature | Per-FeatureId application authorization/policy evaluation |
| Endpoint dibuat per UI card/bundle | Contract dan engine duplikat | Card hanya memetakan canonical FeatureId/workflow plan |
| Assistant mempunyai execution endpoint khusus | Business rule paralel/bypass | Shared FeatureExecutionRequest dan WorkflowPlan |
| OpenAPI generated tetapi tidak di-review | Breaking change lolos | Snapshot, diff, contract test, merge gate |
| OpenAPI oneOf/components membesar | Build/doc/codegen lambat | Hanya server-executable schemas pada HTTP spec; split docs per major/tag bila perlu |
| Route version, ContractVersion, dan schema version tercampur | Forced upgrade dan migration kacau | Pisahkan version layers dan handshake |
| Enum tambahan mematahkan generated client | Runtime failure pada perubahan yang dianggap additive | Unknown enum fallback atau treat as breaking |
| 202 diperlakukan sebagai success final | Assistant/UI mengarang hasil | Persisted job state + result grounding |
| 201 dipakai untuk semua proses cepat | Semantics dan Location tidak konsisten | Batasi 201 ke resource creation |
| JobId/CorrelationId dianggap authorization | IDOR/data exposure | Ownership/permission check; opaque identifiers |
| File path lokal masuk HTTP contract | Privacy/platform coupling | Opaque server file handle; upload strategy terpisah |
| Retry menggandakan processing | Duplicate output/cost | API-DEC-005 idempotency contract dan job state guard |
| 429/503 tanpa recovery hint | Retry storm | Retry-After, backoff guidance, queue/backpressure policy |

## 8. Conflicts found

1. **API-DEC-001 status conflict.** Prompt task menyatakan API-DEC-001 masih OPEN, baseline C hanya rekomendasi, dan melarang Agent A mengunci keputusan. File yang tersedia, 06_API_DOCUMENTATION_WORKING_v0.2_PERTAMINA_OTO.md, menyatakan API-DEC-001 LOCKED — C. Laporan ini mengikuti instruksi task: keputusan diperlakukan OPEN dan semua route yang bergantung padanya ditandai BLOCKED_BY_DECISION.
2. **Source filename mismatch.** Prompt menunjuk 06_API_DOCUMENTATION_WORKING_v0.1_PERTAMINA_OTO.md, tetapi file tersebut tidak tersedia. Workspace hanya mempunyai v0.2. Analisis memakai v0.2 sebagai working document yang tersedia dan tidak mengeditnya.
3. **Canonical count editorial inconsistency.** Requirement Specification v1.5 pada revision note v1.4 masih menyebut scope 546 canonical features, sedangkan header/final v1.5, Feature Catalog v1.3, Solution Design v1.2, Technical Reference v1.2, dan task lock menyatakan 550. Baseline yang digunakan laporan ini adalah 550; angka 546 perlu koreksi editorial saat master direvisi oleh owner.
4. **201 semantics belum tegas.** Working v0.2 menyatakan proses cepat dapat mengembalikan 200/201, tetapi belum membatasi 201 pada resource creation. Rekomendasi laporan: 200 untuk immediate command result; 201 hanya jika resource addressable dibuat dan Location diberikan.
5. Tidak ditemukan konflik fatal antara rekomendasi Hybrid API, Local-First, REST source of truth, SignalR non-authoritative, canonical FeatureId, typed versioned schema, no direct SQL, Assistant boundary, dan background JobId.

## 9. Questions requiring user decision

1. **API-DEC-001:** Apakah user memilih A endpoint per tool, B generic execution, atau C Hybrid API Surface? Rekomendasi Agent A: **C**, tetapi belum locked.
2. **API-DEC-002:** Apakah Governed Hybrid OpenAPI disetujui sebagai baseline: typed C# implementation contract + generated OpenAPI + snapshot/diff + contract test? Rekomendasi: **ya**.
3. **API-DEC-003:** Apakah functional API memakai URI major /api/v1 dengan bootstrap /api/compatibility yang stabil? Rekomendasi: **ya**.
4. Apakah CurrentVersion pada compatibility response didefinisikan sebagai versi client terbaru per platform/channel, bukan versi server deployment? Rekomendasi: **versi client terbaru per platform/channel**.

BLOCKED_BY_DECISION: API-DEC-001

5. Apakah server-owned multi-step workflow memperoleh route khusus /api/v1/workflows/execute, sementara local-only workflow tetap tanpa HTTP processing? Rekomendasi: **ya**, hanya untuk persisted server orchestration dan tidak per Tool Card.
6. Apakah HTTP OpenAPI perlu memuat hanya server-executable feature schemas, sedangkan schema local/shared ditempatkan pada Feature Contract Catalog terpisah? Rekomendasi: **ya**.

## 10. Suggested text for Document 06

Teks berikut siap dipertimbangkan untuk master setelah user mengambil keputusan. Status sengaja tetap OPEN.

### API-DEC-001 — Tool / Feature API Surface Strategy

**Status: OPEN — USER DECISION REQUIRED**

**Recommendation: Option C — Hybrid API Surface.**

BLOCKED_BY_DECISION: API-DEC-001

Canonical feature execution route digunakan hanya untuk tool processing yang benar-benar melewati server:

~~~http
POST /api/{major}/features/{featureId}/execute
~~~

Route ini bukan endpoint universal untuk seluruh 550 FeatureId. Hanya FeatureId canonical yang aktif, mempunyai typed ParameterSchemaVersion, registered server handler, dan lolos permission/policy/capability/resource/engine/consent check yang dapat dieksekusi. Feature local-only tetap menggunakan local application service dan tidak mempunyai API processing.

Resource dengan identity/lifecycle/query sendiri tetap menggunakan domain route, termasuk compatibility, account, jobs, history, secured files, issues/support, audit, admin/configuration, dan health.

Primary Tool Card yang berupa workflow bundle tidak memperoleh endpoint per card. Bundle memetakan langkah canonical FeatureId melalui versioned WorkflowPlan. UI, File-First, Tool-First, dan Assistant memakai FeatureExecutionRequest/WorkflowPlan serta application service yang sama. Assistant tidak boleh bypass application contract.

### API-DEC-002 — API Contract / OpenAPI Strategy

**Status: OPEN — USER DECISION REQUIRED**

**Recommendation: Option C — Governed Hybrid OpenAPI.**

Typed C# transport/application contracts menjadi sumber implementasi. OpenAPI dihasilkan otomatis per API major. Pipeline menyimpan normalized snapshot, menjalankan breaking-change diff, schema validation, serialization/contract test, dan completeness test untuk setiap server-executable FeatureId.

Generic execution memakai common envelope tetapi setiap FeatureId tetap mempunyai typed parameters/result dan ParameterSchemaVersion. OpenAPI menyediakan named schema components serta machine-readable feature contract index. Local-only capability tidak didokumentasikan sebagai HTTP processing operation.

Standard error minimum:

~~~json
{
  "code": "FEATURE_PARAMETER_INVALID",
  "message": "Parameter feature tidak valid.",
  "correlationId": "corr_...",
  "details": null
}
~~~

Stack trace, internal path, SQL, token, password, secret, dan isi dokumen tidak pernah dikirim kepada client.

### API-DEC-003 — Major API Version Routing

**Status: OPEN — USER DECISION REQUIRED**

**Recommendation: URI major untuk functional API.**

Functional routes menggunakan /api/v1. Bootstrap GET /api/compatibility tetap stabil untuk mengembalikan:

~~~text
CurrentVersion
MinimumSupportedVersion
ContractVersion
CapabilityVersion
Status
SupportedApiMajors
~~~

Route major hanya berubah untuk breaking HTTP change. AppVersion, ContractVersion, ParameterSchemaVersion, WorkflowSchemaVersion, ConfigurationSchemaVersion, dan CapabilityVersion dikelola sebagai version layer terpisah dan tidak dimasukkan seluruhnya ke URI.

Breaking change mencakup penghapusan/rename route atau field, perubahan type/meaning/status semantics, optional menjadi required, constraint incompatible, perubahan input/output contract, atau ParameterSchemaVersion major tanpa compatibility path. Non-breaking change dapat mencakup optional field dengan default stabil, endpoint/FeatureId baru, constraint yang dilonggarkan, dan optional response field apabila client mengabaikan unknown fields.

### Standard HTTP semantics

~~~text
200  immediate success/read
201  addressable resource created + Location
202  background accepted + JobId + status URL + CorrelationId
400  malformed request/envelope
401  authentication invalid/missing
403  authenticated but forbidden by permission/policy
404  resource/FeatureId not found or not exposed
409  resource/job/version state conflict
413  payload/file/attachment too large
422  semantic parameter/input/business validation failed
429  rate/quota/concurrency/backpressure limit
500  unexpected internal error, no stack trace
503  service/dependency temporarily unavailable
~~~

JobId adalah opaque identity background job; CorrelationId adalah opaque trace identifier lintas komponen; FeatureId adalah canonical capability identity yang stabil dan human-readable. Tidak satu pun dari ketiganya memberikan authorization.
