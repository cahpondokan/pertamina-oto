# 06 — DOKUMENTASI API
## PERTAMINA OTO — Office Tools Organizer

> **Standar bahasa:** Bahasa Indonesia — ID-1.0

> **Status:** FINAL — LOCKED PRE-CODING API BASELINE  
> **Versi:** 1.0  
> **Tanggal:** 24 Agustus 2026  
> **Baseline fitur canonical:** 550  
> **Primary Feature Set:** 33 PDF Tool Cards  
> **Parent:** Documents 01–05 FINAL  
> **API decisions:** 9/9 LOCKED

---

# 1. Tujuan dan Ruang Lingkup

Dokumen ini adalah kontrak pre-coding resmi untuk:

- REST API;
- OpenAPI dan compatibility;
- batas autentikasi dan otorisasi;
- canonical Feature execution;
- workflow dan Assistant orchestration;
- upload, download, dan secure file reference;
- background job, idempotency, retry, resume, dan recovery;
- paginasi, filter, pencarian, dan pengurutan;
- account, history, secured files, issue/support, audit, configuration, dan admin resources;
- health, telemetry, rate limit, quota, dan backpressure;
- SignalR realtime notification serta REST reconciliation;
- mapping API untuk 33 Primary PDF Tool Cards;
- ketertelusuran dan penerimaan kontrak.

Dokumen ini tidak berarti seluruh 550 fitur canonical mempunyai endpoint pemrosesan server. Fitur local-only tetap berjalan melalui application service lokal atau Worker Lokal.

Dokumen ini tidak menentukan schema database fisik, vendor PDF/OCR/AI final, nilai kapasitas final, atau Role Matrix final. Hal tersebut mengikuti dokumen berikutnya, PoC, benchmark, tinjauan keamanan/lisensi, dan kebijakan Pertamina yang disetujui tanpa membuka kembali kontrak yang sudah dikunci.

---

# 2. Sumber Kebenaran dan Baseline

Urutan sumber:

1. `01_BUSINESS_PROCESS_FINAL_v3.4_PERTAMINA_OTO.md`
2. `02_REQUIREMENT_SPECIFICATION_FINAL_v1.5_PERTAMINA_OTO.md`
3. `03_FEATURE_CATALOG_FINAL_v1.3_PERTAMINA_OTO.md`
4. `04_SOLUTION_DESIGN_FINAL_v1.2_PERTAMINA_OTO.md`
5. `05_TECHNICAL_REFERENCE_FINAL_v1.2_PERTAMINA_OTO.md`
6. Sembilan official API decision records.

Baseline yang dikunci:

```text
Canonical features          : 550
Architecture decisions      : 18 / 18 LOCKED
Technical decisions         : 15 / 15 LOCKED
API decisions               : 9 / 9 LOCKED
REST                        : source of truth
SignalR                     : realtime notification only
Local processing            : preferred when safe/capable/allowed
Assistant                   : orchestrator only
Sign PDF                    : visual signature/paraf non-PKI
Certificate/PKI signature   : OUT OF SCOPE
Public AI endpoint          : PROHIBITED
```

---

# 3. Register Keputusan API — 9/9 LOCKED

| No | Keputusan | Hasil yang dikunci |
|---:|---|---|
| 1 | Tool / Feature API Surface | Hybrid API Surface |
| 2 | API Contract / OpenAPI | Governed Hybrid OpenAPI |
| 3 | Major API Version Routing | URI Major + stable compatibility endpoint |
| 4 | File Transfer | Hybrid streamed/resumable transfer |
| 5 | Idempotency & Retry | Persisted record + request fingerprint |
| 6 | Pagination / Filter / Sort | Hybrid List Query Standard |
| 7 | Client Authentication | Pertamina Identity + server authorization |
| 8 | SignalR Envelope & Recovery | Versioned at-least-once notification + REST recovery |
| 9 | Rate / Quota / Backpressure | Layered admission control + bounded fair queue |

---

# 4. Prinsip Desain API

1. **Local-First.** API server tidak dibuat hanya untuk menyederhanakan implementasi klien.
2. **Capability-aware.** Platform, perangkat, policy, permission, resource, engine, konektivitas, dan persetujuan menentukan eksekusi lokal/server.
3. **REST berwenang.** State REST yang tersimpan menang atas event, cache, atau proyeksi UI.
4. **SignalR tidak berwenang.** Event hanya mempercepat pembaruan state yang sudah tersimpan.
5. **Canonical Feature ID.** UI, File-First, Assistant, workflow, permission, audit, telemetry, dan resolver memakai ID canonical yang sama.
6. **Schema bertipe/berversi.** `additionalProperties=false` menjadi default untuk kontrak command.
7. **Dispatch tertutup.** Eksekusi Feature generik hanya menuju handler bertipe yang terdaftar; bukan RPC arbitrer.
8. **Endpoint resource tetap eksplisit.** Akun, job, history, secured file, issue, audit, configuration, dan admin memiliki siklus hidup sendiri.
9. **Tanpa engine duplikat.** Tool Card, workflow, Assistant, dan transport tidak membuat service pemrosesan paralel.
10. **Aman secara default.** Otorisasi deny-by-default, penggunaan resource terbatas, minimisasi data, transfer aman, dan diagnostik aman.
11. **Tanpa penimpaan diam-diam.** Sumber lokal, secured version, dan konflik sync tidak ditimpa tanpa aturan/konfirmasi resmi.
12. **Tanpa menunggu diam-diam.** Status immediate/background, progress, Activity Center, panduan retry, dan recovery harus terlihat.

---

# 5. Routing, Versioning, Headers, dan Identifier

## 5.1 Route Dasar

```text
Functional REST API : /api/v1
Compatibility       : /api/compatibility
Liveness            : /health/live
Readiness           : /health/ready
SignalR Activity Hub: /hubs/activity
```

`/api/v2` hanya dibuat untuk breaking HTTP change. v1 dan v2 dapat berjalan bersama selama support window.

## 5.2 Lapisan Versi

| Lapisan | Fungsi |
|---|---|
| AppVersion | Release client per platform/channel |
| API major | Breaking HTTP surface, misalnya v1/v2 |
| ContractVersion | Revisi contract pada major aktif |
| ParameterSchemaVersion | Parameter/result schema per Feature ID |
| WorkflowSchemaVersion | Struktur workflow plan |
| ConfigurationSchemaVersion | Payload configuration/policy |
| CapabilityVersion | Snapshot registry/exposure/capability |
| EventVersion | Schema per SignalR event type |

Versi selain API major tidak dimasukkan ke route.

## 5.3 Header Request Standar

| Header | Kegunaan |
|---|---|
| `Authorization: Bearer ...` | Approved Pertamina access token/session assertion |
| `X-Correlation-Id` | Opaque logical action/workflow correlation |
| `X-Client-Platform` | Platform canonical, misalnya `WINDOWS_DESKTOP` |
| `X-Client-Version` | App version pemanggil |
| `Idempotency-Key` | Wajib pada mutating/expensive command yang ditentukan contract |
| `If-Match` / `If-None-Match` | Optimistic concurrency/conditional read |
| `Content-Digest` | Integrity untuk chunk/content bila diwajibkan |

HTTP header bersifat case-insensitive, tetapi seluruh contoh menggunakan ejaan di atas.

## 5.4 Header Respons Standar

| Header | Kegunaan |
|---|---|
| `X-Correlation-Id` | Correlation yang diterima/dibuat server |
| `Location` | Resource/status URL pada `201` atau `202` |
| `Retry-After` | Safe retry guidance untuk `202`, `429`, atau temporary `503` |
| `ETag` | Resource/result version untuk conditional read/range |
| `Accept-Ranges: bytes` | Resume download |
| `X-Content-Type-Options: nosniff` | Safe file response |

## 5.5 Kontrak Identifier

### Feature ID

- canonical, immutable, case-stable, dan tidak didaur ulang;
- bukan database key atau label UI;
- alias hanya untuk discovery; execution memakai canonical ID.

### Job ID

- opaque dan unik;
- tidak memuat user, tenant, engine, atau database key;
- ownership/permission selalu diperiksa;
- bukan authorization proof.

### Correlation ID

- opaque dan tidak memuat data pribadi;
- satu logical action/workflow dapat membagikannya lintas component;
- bukan token, permission, atau Job ID.

### Referensi File

`uploadId` adalah identity transfer lifecycle. `fileRef` adalah typed opaque execution reference:

```json
{
  "kind": "UPLOAD",
  "id": "upl_01J...",
  "version": "1"
}
```

Jenis baseline yang diizinkan:

```text
UPLOAD       → READY upload resource
SECURED_FILE → authorized secured-file version
JOB_RESULT   → validated available result
```

Path fisik, object-storage key, path lokal, URL arbitrer, dan filename tidak pernah menjadi otoritas.

---

# 6. Governed Hybrid OpenAPI

## 6.1 Sumber dan Artefak

- Transport C# serta kontrak aplikasi yang bertipe/berversi adalah sumber implementasi.
- OpenAPI dihasilkan otomatis dari endpoint, DTO, validation, dan schema metadata.
- Normalized OpenAPI disimpan/versioned sebagai contract artifact per API major.
- Dokumentasi API manual bukan sumber kebenaran.
- Engine/provider class tidak menjadi transport contract.

## 6.2 Pipeline Tata Kelola

CI wajib menjalankan:

```text
OpenAPI generation
→ OpenAPI validation
→ normalized snapshot comparison
→ breaking-change diff
→ serialization/validation test
→ request/response contract test
→ registered-handler completeness test
→ example/redaction/security test
```

Breaking change memblokir merge kecuali task secara eksplisit menyediakan API major baru atau jalur kompatibilitas/migration yang disetujui.

## 6.3 Aturan Katalog Schema

- Common envelopes menjadi named OpenAPI components.
- Setiap server-executable Feature ID mempunyai named parameter/result components.
- Feature Contract Index memetakan Feature ID ke schema/version/input/output/risk/exposure.
- Local-only capability boleh berada pada shared Feature Contract Catalog, tetapi tidak muncul sebagai HTTP processing operation.
- `operationId` eksplisit, stabil, dan tidak bergantung nama controller/class internal.

---

# 7. Respons Standar, Error, dan Status HTTP

## 7.1 Error Standar

```json
{
  "code": "FEATURE_PARAMETER_INVALID",
  "message": "Parameter feature tidak valid.",
  "correlationId": "corr_01J...",
  "details": {
    "type": "VALIDATION",
    "issues": [
      {
        "field": "parameters.pageRange",
        "issue": "PAGE_RANGE_INVALID"
      }
    ]
  }
}
```

`details` adalah typed safe union:

```text
null
VALIDATION      → safe field/issue codes
RETRY_GUIDANCE  → retryAfterSeconds, retryable
CONFLICT        → safe currentState/expectedState
LIMIT           → safe limitScope/retryAfterSeconds
```

Detail JSON arbitrer, stack trace, exception, class, SQL, path, storage key, token, password, secret, konten dokumen/OCR, artefak tanda tangan, detail provider, kedalaman queue, dan topologi dilarang.

## 7.2 Semantik HTTP

| Status | Semantik PERTAMINA OTO |
|---:|---|
| 200 | Read atau immediate command berhasil |
| 201 | Addressable resource dibuat; `Location` wajib |
| 202 | Background operation sudah durable; Job ID/status URL/correlation wajib |
| 204 | Chunk/abort/idempotent action selesai tanpa body |
| 206 | Partial/range download |
| 304 | Conditional read tidak berubah |
| 400 | Malformed header/query/envelope/JSON/range |
| 401 | Authentication missing/invalid/expired/revoked atau reauthentication required |
| 403 | Identity valid tetapi permission/ownership/policy menolak |
| 404 | Resource tidak ada atau concealed secara konsisten |
| 408 | Request/one-shot upload timeout |
| 409 | State/idempotency/offset/checkpoint/result/storage conflict |
| 410 | Known owner resource expired/removed by retention |
| 412 | ETag/version precondition gagal |
| 413 | Hard request/file/chunk/batch limit terlampaui |
| 415 | Media/signature tidak didukung atau tidak konsisten |
| 416 | Download range tidak valid |
| 422 | Syntactically valid tetapi semantic/file/business validation gagal |
| 429 | Caller rate/quota/concurrency limit; `Retry-After` wajib |
| 500 | Unexpected internal failure; safe envelope only |
| 502 | Immediate approved upstream dependency failure |
| 503 | Shared system/worker/provider capacity/readiness tidak aman; retry guidance bila aman |
| 504 | Upstream timeout sebelum durable background acceptance |

---

# 8. Kompatibilitas dan Kesehatan Layanan

## 8.1 Kompatibilitas

```http
GET /api/compatibility
X-Client-Platform: WINDOWS_DESKTOP
X-Client-Version: 1.3.0
X-Correlation-Id: corr_01J...
```

```json
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
```

`currentVersion` adalah client version terbaru yang disetujui untuk platform/channel peminta. Status: `SUPPORTED`, `UPDATE_AVAILABLE`, `UPDATE_REQUIRED`, atau `UNSUPPORTED`. Forced update hanya untuk security/compatibility requirement nyata.

## 8.2 Kesehatan Layanan

| Method | Route | Eksposur |
|---|---|---|
| GET | `/health/live` | Minimal process-alive response; tanpa dependency detail |
| GET | `/health/ready` | `200` ready atau `503` not-ready; Production detail restricted ke approved operations boundary |

Endpoint kesehatan tidak mengungkap secret, connection string, path server, credential provider, kedalaman queue, atau topologi.

---

# 9. Batas Autentikasi dan Otorisasi

```text
Client
→ approved Pertamina Identity
→ API authentication
→ permission + ownership + enterprise policy
→ application service
```

## 9.1 Autentikasi

- HTTPS wajib.
- API memvalidasi signature, allowed algorithm, issuer, audience, lifetime, subject, dan approved claim contract.
- Native Desktop/Mobile memakai approved flow dengan PKCE dan secure OS credential storage.
- Web menjaga credential/token pada server-side session boundary bila sesuai arsitektur; browser-accessible bearer/refresh-token storage dilarang.
- API resource tidak menerima refresh token dan bukan identity provider.
- Token tidak masuk URL, logs, telemetry, issue diagnostics, atau errors.

## 9.2 Otorisasi

- deny-by-default;
- permission-based dan object-aware;
- ownership/resource scope diperiksa pada application boundary;
- role hanya permission grouping, bukan final authorization check;
- list filter/cursor/object ID bukan security proof;
- Administrator tidak otomatis dapat membaca file user;
- Superadmin/Support adalah permission-gated support surface, bukan bypass atau role baru otomatis;
- attachment access hanya untuk attachment yang user kirim eksplisit dan sesuai permission;
- every continuation/download/retry/resume/export/subscription di-authorize ulang.

## 9.3 Offline dan Layanan Internal

- Offline session hanya mengizinkan narrow local capability selama cached session/permission masih valid menurut time-bounded policy.
- Server-required dan sensitive action menunggu online authorization.
- Setelah reconnect, identity dan permission di-refresh sebelum queued command dikirim.
- API, Server Worker, storage adapter, dan approved internal provider memakai separate short-lived scoped service identity/certificate/managed identity.
- End-user access/refresh token tidak diteruskan ke Worker/provider.
- Internal network dan queue tidak otomatis dipercaya.

## 9.4 401/403/404

```text
401 → tidak ada authentication yang dapat dipakai
403 → identity valid, access ditolak permission/ownership/policy
404 → consistent concealment untuk sensitive resource tanpa discovery access
```

---

# 10. Inventaris Endpoint

Route fungsional memakai `/api/v1`; kompatibilitas dan kesehatan layanan adalah pengecualian stabil yang ditulis eksplisit. `Idem` berarti `Idempotency-Key` wajib.

## 10.1 Sistem dan Capability

| Method | Route | Tujuan | Akses | Idem |
|---|---|---|---|:---:|
| GET | `/api/compatibility` | Stable client/API compatibility bootstrap | Safe pre-auth metadata | — |
| GET | `/health/live` | Liveness | Operations/infrastructure | — |
| GET | `/health/ready` | Readiness | Operations; details restricted | — |
| GET | `/api/v1/features` | Filtered feature/capability discovery | Authenticated; exposure filtered | — |
| GET | `/api/v1/features/{featureId}` | Visible typed contract descriptor | Authenticated; exposure filtered | — |
| POST | `/api/v1/features/{featureId}/execute` | Canonical server execution | Feature permission + policy + input ownership | Yes |
| POST | `/api/v1/workflows/execute` | Server-owned canonical workflow only | Per-step permission/policy/input ownership | Yes |

## 10.2 File transfer

| Method | Route | Tujuan | Akses | Idem |
|---|---|---|---|:---:|
| POST | `/api/v1/file-transfers/preflight` | Effective policy/mode/capacity before content transfer; no resource created | Authenticated + feature policy | — |
| POST | `/api/v1/uploads` | Streamed multipart one-shot upload | Owner + upload permission/consent | Yes |
| POST | `/api/v1/upload-sessions` | Create resumable upload session | Owner + upload permission/consent | Yes |
| GET | `/api/v1/upload-sessions/{uploadId}` | Session status/offset/expiry | Owner/authorized principal | — |
| PUT | `/api/v1/upload-sessions/{uploadId}/content` | Sequential raw chunk | Owner; range/digest/state checked | Natural |
| POST | `/api/v1/upload-sessions/{uploadId}/complete` | Integrity close and validation | Owner; session state checked | Yes |
| DELETE | `/api/v1/upload-sessions/{uploadId}` | Abort and cleanup scheduling | Owner; idempotent | — |

## 10.3 Job dan Hasil

| Method | Route | Tujuan | Akses | Idem |
|---|---|---|---|:---:|
| GET | `/api/v1/jobs` | Personal Activity/job list | Authenticated owner scope | — |
| GET | `/api/v1/jobs/{jobId}` | Authoritative persisted state | Owner/authorized support scope | — |
| POST | `/api/v1/jobs/{jobId}/cancel` | Cooperative safe cancellation | Owner/permission + state check | Yes |
| POST | `/api/v1/jobs/{jobId}/retry` | New attempt on same logical job | Owner/permission + retry policy | Yes |
| POST | `/api/v1/jobs/{jobId}/resume` | Resume verified compatible checkpoint | Owner/permission + checkpoint policy | Yes |
| GET | `/api/v1/jobs/{jobId}/results` | Result manifest and retention | Owner/authorized principal | — |
| HEAD | `/api/v1/jobs/{jobId}/results/{resultId}/content` | Result metadata/range support | Owner/authorized principal | — |
| GET | `/api/v1/jobs/{jobId}/results/{resultId}/content` | Authorized streaming download | Owner/authorized principal | — |

## 10.4 Akun, Riwayat, dan File yang Diamankan

| Method | Route | Tujuan | Akses | Idem |
|---|---|---|---|:---:|
| GET | `/api/v1/account/profile` | Current safe profile metadata | Current user | — |
| GET | `/api/v1/account/preferences` | Selective synced preferences | Current user | — |
| PUT | `/api/v1/account/preferences` | Versioned preference update | Current user + `If-Match` | Yes |
| GET | `/api/v1/history` | Personal history cursor list | Current user only | — |
| GET | `/api/v1/history/{historyId}` | Personal history detail | Owner only | — |
| POST | `/api/v1/history/{historyId}/reprocess` | Reinvoke stored canonical request through normal execution boundary | Owner + current feature permission/policy | Yes |
| DELETE | `/api/v1/history/{historyId}` | Remove personal history item, not enterprise audit | Owner only | Yes |
| POST | `/api/v1/secured-files` | Explicit **Amankan ke Akun** from validated `fileRef` | Owner + storage policy/quota | Yes |
| GET | `/api/v1/secured-files` | Authorized secured-file cursor list | Owner/permission scope | — |
| GET | `/api/v1/secured-files/{securedFileId}` | Metadata/current version | Owner/authorized principal | — |
| POST | `/api/v1/secured-files/{securedFileId}/versions` | Save validated `fileRef` as new version of existing secured file | Owner + storage/version policy | Yes |
| GET | `/api/v1/secured-files/{securedFileId}/versions` | Maximum-three-version metadata | Owner/authorized principal | — |
| GET | `/api/v1/secured-files/{securedFileId}/versions/{versionId}/content` | Authorized secured content download | Owner/authorized principal | — |
| POST | `/api/v1/secured-files/{securedFileId}/versions/{versionId}/restore` | Restore selected previous version through version service | Owner + version/state policy | Yes |
| DELETE | `/api/v1/secured-files/{securedFileId}` | Move to Account Recycle/retention lifecycle | Owner + confirmation rule | Yes |
| POST | `/api/v1/secured-files/{securedFileId}/restore` | Restore from Account Recycle | Owner + retention/state check | Yes |
| DELETE | `/api/v1/secured-files/{securedFileId}/permanent` | Confirmed permanent server deletion | Owner + policy + reauth when required | Yes |

## 10.5 Masalah dan Dukungan

| Method | Route | Tujuan | Akses | Idem |
|---|---|---|---|:---:|
| POST | `/api/v1/issues` | Submit minimal message + safe context | Current user | Yes |
| GET | `/api/v1/issues` | Current user's issue cursor list | Reporter only | — |
| GET | `/api/v1/issues/{issueId}` | Current user's simple issue status/detail | Reporter or support permission | — |
| POST | `/api/v1/issues/{issueId}/attachments` | Explicit streamed screenshot/attachment upload | Reporter + explicit-send state | Yes |
| GET | `/api/v1/issues/{issueId}/attachments/{attachmentId}/content` | Authorized attachment preview/download | Reporter or support permission | — |
| GET | `/api/v1/support/issues` | Permission-scoped support cursor list | Support list permission | — |
| GET | `/api/v1/support/issues/{issueId}` | Triage detail + explicitly sent attachment metadata | Support detail permission | — |
| PATCH | `/api/v1/support/issues/{issueId}` | Category/priority/status/duplicate/not-bug/need-info update | Support triage permission + `If-Match` | Yes |
| POST | `/api/v1/support/issues/{issueId}/notes` | Internal note | Support note permission | Yes |
| POST | `/api/v1/support/issues/{issueId}/exports/pdf` | One-issue PDF export job | Support export permission | Yes |
| POST | `/api/v1/support/issues/exports/excel` | Filtered Excel export job | Support export permission | Yes |

## 10.6 Audit, Konfigurasi, dan Administrasi

| Method | Route | Tujuan | Akses | Idem |
|---|---|---|---|:---:|
| POST | `/api/v1/audit-events/batches` | Idempotent sync of safe offline audit metadata | Authenticated app/client policy | Yes |
| GET | `/api/v1/audit-events` | Permission-scoped audit cursor query | Auditor permission | — |
| POST | `/api/v1/audit-events/exports` | Bounded asynchronous audit export | Auditor export permission | Yes |
| GET | `/api/v1/configuration/effective` | Effective safe configuration/capability policy | Authenticated; filtered | — |
| GET | `/api/v1/admin/policies` | Policy metadata | Admin policy-read permission | — |
| PUT | `/api/v1/admin/policies/{policyKey}` | Typed/versioned policy update | Admin policy-write + `If-Match` | Yes |
| GET | `/api/v1/admin/feature-flags` | Feature flag list | Admin feature-read permission | — |
| PUT | `/api/v1/admin/feature-flags/{featureId}` | Exposure flag; cannot change feature meaning | Admin feature-write + `If-Match` | Yes |
| GET | `/api/v1/admin/limits` | Effective configurable limits within safety ceilings | Admin limit-read permission | — |
| PUT | `/api/v1/admin/limits/{limitKey}` | Typed limit update within hard safety bounds | Admin limit-write + `If-Match` | Yes |

Local Recycle Bin, local path opening, draft, undo/redo, cache, thumbnail, temp, clipboard, signature asset, and local checkpoint are not server API resources by default.

# 11. Kontrak Feature Canonical, Workflow, dan Assistant

## 11.1 Penemuan Feature

`GET /api/v1/features` dan descriptor detail hanya mengembalikan capability yang boleh dilihat caller berdasarkan platform, app compatibility, permission, policy, feature flag, dan exposure.

Discovery metadata tidak berarti Feature ID mempunyai server handler. Field minimum:

```json
{
  "featureId": "FEAT-CONV-001",
  "recordType": "FEATURE",
  "displayKey": "feature.word_to_pdf",
  "contractVersion": "1.0",
  "parameterSchemaVersion": "1.0",
  "capabilityVersion": "2026.08.24.1",
  "serverDependency": "OPTIONAL",
  "serverExecutable": true,
  "processingModes": ["LOCAL", "SERVER"],
  "risk": "LOW",
  "availability": "AVAILABLE"
}
```

## 11.2 Dispatch Bertipe dan Tertutup

`POST /api/v1/features/{featureId}/execute` hanya tersedia secara logis jika:

1. Feature ID canonical aktif dan visible;
2. registered server handler tersedia;
3. typed parameter/result schema cocok;
4. input reference valid, owned, dan READY/AVAILABLE;
5. permission, policy, classification, feature flag, capability, engine, capacity, connectivity, dan consent lolos;
6. output validation tersedia.

Klien tidak boleh mengirim nama engine, service, class, assembly, SQL, shell, path filesystem, URL arbitrer, provider, atau command.

## 11.3 Request Eksekusi Feature

```http
POST /api/v1/features/FEAT-CONV-001/execute
Authorization: Bearer <approved-token>
Idempotency-Key: 7d2e...
X-Correlation-Id: corr_01J...
Content-Type: application/json
```

```json
{
  "contractVersion": "1.0",
  "parameterSchemaVersion": "1.0",
  "inputs": [
    {
      "inputId": "source-1",
      "fileRef": {
        "kind": "UPLOAD",
        "id": "upl_01J...",
        "version": "1"
      }
    }
  ],
  "parameters": {
    "outputFileName": "dokumen.pdf",
    "pageRange": null
  }
}
```

Feature ID pada path authoritative. Body tidak mengulang discriminator Feature ID.

## 11.4 Hasil Langsung

```http
HTTP/1.1 200 OK
X-Correlation-Id: corr_01J...
```

```json
{
  "featureId": "FEAT-CONV-001",
  "contractVersion": "1.0",
  "parameterSchemaVersion": "1.0",
  "status": "COMPLETED",
  "result": {
    "primaryOutput": {
      "fileRef": {
        "kind": "JOB_RESULT",
        "id": "res_01K...",
        "version": "1"
      },
      "mediaType": "application/pdf",
      "sizeBytes": 245901,
      "temporary": true,
      "expiresAt": "2026-08-25T10:00:00Z"
    }
  },
  "correlationId": "corr_01J..."
}
```

## 11.5 Background Diterima

```http
HTTP/1.1 202 Accepted
Location: /api/v1/jobs/job_01J...
Retry-After: 3
X-Correlation-Id: corr_01J...
```

```json
{
  "jobId": "job_01J...",
  "featureId": "FEAT-CONV-001",
  "status": "QUEUED",
  "statusUrl": "/api/v1/jobs/job_01J...",
  "resultUrl": "/api/v1/jobs/job_01J.../results",
  "correlationId": "corr_01J..."
}
```

`202` berarti durable acceptance, bukan final success.

## 11.6 Rencana Workflow

Tool Card bundle dan Assistant multi-step intent memakai contract yang sama:

```json
{
  "workflowSchemaVersion": "1.0",
  "steps": [
    {
      "stepId": "merge",
      "featureId": "FEAT-PDF-005",
      "parameterSchemaVersion": "1.0",
      "inputs": [{ "from": "workflow.inputs.pdfFiles" }],
      "parameters": {
        "orderedInputIds": ["pdf-1", "pdf-2"]
      }
    },
    {
      "stepId": "number",
      "featureId": "FEAT-PDF-037",
      "parameterSchemaVersion": "1.0",
      "inputs": [{ "from": "steps.merge.outputs.primary" }],
      "parameters": {
        "startNumber": 1,
        "position": "BOTTOM_CENTER"
      }
    }
  ],
  "output": { "from": "steps.number.outputs.primary" }
}
```

Rules:

- seluruh langkah local-capable → local application services, tanpa HTTP processing;
- server-owned workflow hanya bila minimal satu langkah secara sah memakai server;
- server workflow memiliki satu parent Job ID dan persisted step state;
- permission/policy/schema/input validation dilakukan per step;
- intermediate output bukan final;
- default failure berhenti aman; tidak mengasumsikan transaction lintas engine;
- retry/resume mengikuti safe checkpoint per step.

## 11.7 Batas Assistant

Assistant hanya menghasilkan:

1. `FeatureExecutionRequest`; atau
2. `WorkflowPlan` dari canonical Feature IDs.

Assistant melewati validator, permission, policy, capability, preflight resource, konfirmasi risiko, application service, resolver, infrastruktur Job/Activity, dan grounding hasil yang sama dengan UI. Assistant tidak mengakses SQL/shell/filesystem/API arbitrer secara langsung, tidak memilih engine/provider, dan tidak mengarang keberhasilan.

PDF Intelligence memakai approved local atau enterprise-internal provider setelah policy/classification/permission/data-minimization/consent check. Document/OCR content diperlakukan untrusted. Public AI/translation endpoint dilarang.

---

# 12. Kontrak Transfer File dan Download

## 12.1 Keputusan Routing

```text
Resolver memilih LOCAL
→ tidak ada upload/API processing

Resolver memilih OPTIONAL/REQUIRED SERVER
→ authorization/policy/capability/capacity preflight
→ user notification/consent
→ one-shot atau resumable transfer
→ validation READY
→ execute
```

Klien tidak mengirim parameter untuk memaksa lokal/server. Server memverifikasi ulang keputusan eksekusi.

## 12.2 Pemeriksaan Awal (`preflight`)

```http
POST /api/v1/file-transfers/preflight
```

```json
{
  "featureId": "FEAT-OCR-004",
  "intendedUse": "PROCESSING_INPUT",
  "files": [
    {
      "clientFileId": "local-1",
      "fileName": "scan.pdf",
      "sizeBytes": 734003200,
      "declaredMediaType": "application/pdf"
    }
  ]
}
```

```json
{
  "decision": "SERVER_TRANSFER_ALLOWED",
  "recommendedMode": "RESUMABLE_SESSION",
  "consentRequired": true,
  "limits": {
    "maxFileBytes": 1073741824,
    "maxBatchBytes": 2147483648,
    "maxFileCount": 10,
    "maxChunkBytes": 8388608,
    "policyVersion": "policy_..."
  },
  "correlationId": "corr_..."
}
```

Angka di atas hanya contoh bentuk response, bukan nilai Production yang dikunci. Runtime wajib mengembalikan effective positive policy value. Effective limit adalah nilai paling ketat dari system safety, proxy/API, enterprise policy, feature profile, classification, quota, storage, dan capacity.

## 12.3 Multipart Sekali Kirim

Gunakan streamed multipart untuk file/batch kecil-menengah ketika restart murah dan request budget cukup. Controls:

- bounded parts, header length, field names, file count, file size, dan total bytes;
- stream langsung ke secure temporary storage;
- no whole-file RAM buffering;
- per-file manifest untuk mixed batch outcome;
- no `207 Multi-Status` baseline.

## 12.4 Sesi Upload yang Dapat Dilanjutkan

State:

```text
CREATED → UPLOADING → VALIDATING → READY
                    ↘ FAILED
CREATED/UPLOADING/FAILED → ABORTED
inactive non-final → EXPIRED
```

Create response:

```http
HTTP/1.1 201 Created
Location: /api/v1/upload-sessions/upl_123
```

```json
{
  "uploadId": "upl_123",
  "state": "CREATED",
  "nextOffset": 0,
  "maxChunkBytes": 8388608,
  "expiresAt": "2026-08-24T12:00:00Z",
  "correlationId": "corr_..."
}
```

Chunk:

```http
PUT /api/v1/upload-sessions/upl_123/content
Content-Type: application/octet-stream
Content-Length: 8388608
Content-Range: bytes 16777216-25165823/734003200
Content-Digest: sha-256=:base64digest:
```

```http
HTTP/1.1 204 No Content
Upload-Offset: 25165824
```

Baseline chunk sequential dan bounded. Range yang sudah committed dengan digest sama di-ack ulang. Offset/overlap/digest conflict menghasilkan `409` dengan safe expected offset.

`complete` mengembalikan `200` jika validation selesai cepat. Deep validation mengembalikan `202` + validation Job ID; session tetap `VALIDATING` sampai `READY` atau `FAILED`.

## 12.5 Pipeline Validasi

1. preflight extension, declared MIME/size/count, permission/policy/capacity;
2. streaming hard byte counter, bounded signature sniffing, digest, size stop;
3. MIME/signature consistency, parser/structural/corruption/password check;
4. page/item count, macro, embedded executable/object, decompression/bomb limits;
5. malware/security inspection jika policy mewajibkan;
6. quarantine bila diperlukan;
7. hanya `READY` dapat dipakai execute.

Validation statuses:

```text
READY
PASSWORD_REQUIRED
WARNING
UNSUPPORTED
CORRUPTED
SECURITY_REVIEW
BLOCKED
INSUFFICIENT_STORAGE
```

Password bersifat ephemeral dan tidak masuk persistence, logs, audit, telemetry, backup, idempotency snapshot, atau event.

## 12.6 Konten Download/Hasil

Manifest:

```json
{
  "jobId": "job_123",
  "outcome": "SUCCESS",
  "results": [
    {
      "resultId": "res_1",
      "fileName": "hasil.pdf",
      "mediaType": "application/pdf",
      "sizeBytes": 12582912,
      "etag": "\"result-version\"",
      "state": "AVAILABLE",
      "expiresAt": "2026-08-25T10:00:00Z",
      "contentUrl": "/api/v1/jobs/job_123/results/res_1/content"
    }
  ],
  "correlationId": "corr_..."
}
```

Content endpoint:

- authorize every request;
- stream storage-to-response;
- validated `Content-Type`, safe attachment filename, length, ETag, `nosniff`;
- support `HEAD`, `Range`, `If-Range`, `206`, dan `416`;
- private/no-store caching for temporary sensitive result;
- `409 RESULT_NOT_READY` jika belum siap;
- `410 RESULT_EXPIRED` setelah retention;
- source lokal tidak ditimpa otomatis.

Temporary result hanya menjadi secured file melalui explicit `POST /api/v1/secured-files`.

---

# 13. Kontrak Background Job

## 13.1 State Machine

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

Tidak ada state `PAUSED`. True user pause bukan baseline.

Transitions minimum:

```text
QUEUED → RUNNING
QUEUED/RUNNING ↔ WAITING_RESOURCE
QUEUED/RUNNING ↔ WAITING_CONNECTION
RUNNING/WAITING_* → RETRYING → QUEUED/RUNNING
non-final → CANCELLED after safe point
RUNNING → COMPLETED | FAILED
FAILED → RETRYING only through valid retry/resume command
```

## 13.2 Resource Job Berwenang

```json
{
  "jobId": "job_123",
  "correlationId": "corr_...",
  "featureId": "FEAT-OCR-004",
  "status": "RUNNING",
  "outcome": null,
  "attemptNumber": 2,
  "stateVersion": 17,
  "progress": {
    "percentage": 62,
    "indeterminate": false,
    "currentStep": "OCR_PAGES",
    "completedItems": 619,
    "totalItems": 1000,
    "etaSeconds": 180
  },
  "checkpoint": {
    "supported": true,
    "available": true,
    "stage": "OCR_PAGES",
    "completedItems": 600,
    "resumeAllowed": true,
    "lastSavedAt": "2026-08-24T10:00:00Z"
  },
  "cancellation": {
    "requested": false,
    "canCancel": true
  },
  "startedAt": "2026-08-24T09:58:00Z",
  "completedAt": null,
  "error": null
}
```

Raw checkpoint bytes/token/storage key/provider secret tidak diekspos. Status read mendukung ETag/conditional GET.

## 13.3 Penerimaan Tahan Lama

Sebelum `202`, server harus secara konsisten mempersist:

- logical job;
- initial attempt;
- input leases;
- idempotency record;
- capacity/quota reservation;
- dispatch marker/state.

Koneksi klien yang terputus atau kegagalan SignalR tidak membatalkan job.

## 13.4 Pembatalan

- `QUEUED` dapat menjadi `CANCELLED` secara immediate.
- `RUNNING` memakai cooperative cancellation; response `202`, worker berhenti di safe point, lalu state dipersist `CANCELLED`.
- cancel ulang pada cancelled job mengembalikan current state.
- unsafe cancellation menghasilkan `409 JOB_NOT_CANCELLABLE`.
- completed/finalized output tidak diubah menjadi cancelled.

## 13.5 Percobaan Ulang dan Resume

- automatic retry hanya transient allowlist;
- bounded attempts, exponential backoff + jitter;
- logical Job ID tetap;
- attempt history append-only;
- resume memakai checkpoint yang cocok dengan input identity, Feature/parameter schema, engine/provider version, stage, dan integrity marker;
- incompatible checkpoint → `409 CHECKPOINT_NOT_RESUMABLE`;
- expired checkpoint → `410 CHECKPOINT_EXPIRED`;
- server tidak silent restart dari awal; response memberi safe allowed action.

## 13.6 Penyelesaian dan Hasil Parsial

- `COMPLETED + SUCCESS`: seluruh required output tervalidasi.
- `COMPLETED + PARTIAL_SUCCESS`: hanya untuk independent batch semantics yang diizinkan typed Feature contract.
- atomic operation seperti Merge gagal bila required stage/input gagal.
- intermediate output tidak diekspos sebagai final.
- `FAILED` membawa safe error, `retryable`, dan `resumeAvailable` tanpa raw engine detail.

---

# 14. Kontrak Idempotency

## 14.1 Perintah yang Wajib

`Idempotency-Key` wajib pada:

- one-shot upload;
- create/complete upload session;
- feature/workflow execution;
- job cancel/retry/resume;
- issue submission/attachment;
- support/admin mutating actions;
- secured copy/restore/delete;
- audit batch/export.

Chunk PUT mempunyai natural idempotency dari `uploadId + range + digest`.

## 14.2 Ruang Lingkup dan Record

Scope:

```text
authenticated principal/tenant
+ HTTP method
+ canonical route
+ ContractVersion
```

Persisted record minimum:

```text
IdempotencyKeyHash
Scope
RequestFingerprint
State: IN_PROGRESS | COMPLETED | FAILED_BEFORE_COMMIT
ResourceType + ResourceId
OriginalStatusCode + SafeResponseSnapshot
OperationCorrelationId
CreatedAt + ExpiresAt
```

Fingerprint menggunakan canonical request, Feature/schema version, referenced resource immutable version, dan content digest bila relevan. Password/secret tidak masuk fingerprint snapshot.

## 14.3 Perilaku Duplikat

| Condition | Behavior |
|---|---|
| New key | Process once |
| Same key + same fingerprint + committed | Replay original resource/response |
| Same key + same fingerprint + resource running | `202` + same `Location`/Job ID |
| Same key + same fingerprint, no resource yet | `409 IDEMPOTENCY_REQUEST_IN_PROGRESS` + `Retry-After` |
| Same key + different fingerprint | `409 IDEMPOTENCY_KEY_REUSED` |
| Failure before durable commit | Safe cleanup/restart under same logical operation; no duplicate active resource |

Different keys are different user intents even if content hash is equal.

---

# 15. Paginasi, Filter, Pencarian, dan Pengurutan

## 15.1 Dua Mode

| List type | Mode | Default / maximum |
|---|---|---:|
| Bounded ordinary metadata/configuration | `page` + `pageSize` | 25 / 100 |
| Audit/event | opaque cursor | 50 / 100 |
| Issue | opaque cursor | 25 / 100 |
| Secured file | opaque cursor | 25 / 100 |
| History/job | opaque cursor | 25 / 50 |

Satu endpoint tidak menerima page dan cursor bersamaan. Invalid/zero/negative/excessive value menghasilkan `400 INVALID_PAGINATION`; tidak di-clamp diam-diam.

## 15.2 Respons Cursor

```json
{
  "items": [],
  "pageInfo": {
    "mode": "cursor",
    "pageSize": 25,
    "hasNext": true,
    "nextCursor": "opaque-integrity-protected-value"
  }
}
```

Cursor:

- keyset-based, opaque, integrity-protected, expiring;
- scope-bound ke subject, endpoint, filters, search, date range, sort, dan direction;
- di-authorize ulang setiap continuation;
- tidak memuat SQL, token, secret, storage key, atau sensitive data;
- tampered → `400 INVALID_CURSOR`;
- expired → `410 CURSOR_EXPIRED`;
- exact total tidak wajib.

## 15.3 Standar Query

```http
GET /api/v1/issues?pageSize=25&cursor=...&filter[status]=DIPERIKSA&sort=-createdAt,issueId&search=ERR-102&from=2026-08-01T00:00:00Z&to=2026-09-01T00:00:00Z
```

Standard parameters:

```text
page
pageSize
cursor
sort
filter[field]
search
from
to
```

Rules:

- per-endpoint allowlist for field/operator;
- same filter field repeated = OR; different fields = AND;
- UTC RFC 3339 half-open date range `[from,to)`;
- stable default sort + unique tie-breaker;
- mutable status/priority not used as default ordering;
- raw expression, regex, arbitrary field path, dynamic SQL, client-defined authorization filter, document/OCR/screenshot content search prohibited;
- authorization applied before pagination and count;
- wide snapshot/exact report uses permission-gated asynchronous export.

---

# 16. Contoh Kontrak Resource

## 16.1 Amankan ke Akun

```http
POST /api/v1/secured-files
Idempotency-Key: ...
```

```json
{
  "contractVersion": "1.0",
  "source": {
    "kind": "JOB_RESULT",
    "id": "res_01K...",
    "version": "1"
  },
  "displayName": "hasil.pdf"
}
```

```http
HTTP/1.1 201 Created
Location: /api/v1/secured-files/sfile_01J...
```

```json
{
  "securedFileId": "sfile_01J...",
  "versionId": "sfver_01J...",
  "versionNumber": 1,
  "maximumRetainedVersions": 3,
  "state": "AVAILABLE",
  "correlationId": "corr_..."
}
```

Metadata berada di server persistence; payload berada di approved secure file/object storage. Source, draft, undo, cache, thumbnail, password, clipboard, signature asset, dan temp tidak auto-sync.

`POST /secured-files` berarti **Simpan sebagai File Baru**. `POST /secured-files/{securedFileId}/versions` berarti **Simpan** sebagai versi baru pada file yang sama. Maximum tiga versi terbaru dipertahankan; saat versi keempat dibuat, versi tertua dihapus atau dipindahkan sesuai version/retention policy.

## 16.2 Pengiriman Masalah

```http
POST /api/v1/issues
Idempotency-Key: ...
```

```json
{
  "message": "Hasil konversi tidak dapat dibuka.",
  "safeContext": {
    "featureId": "FEAT-CONV-001",
    "appVersion": "1.4.0",
    "platform": "WINDOWS_DESKTOP",
    "errorCode": "RESULT_VALIDATION_FAILED",
    "jobId": "job_123",
    "correlationId": "corr_..."
  }
}
```

```http
HTTP/1.1 201 Created
Location: /api/v1/issues/issue_01J...
```

```json
{
  "issueId": "issue_01J...",
  "userStatus": "DIKIRIM",
  "attachmentAllowed": true,
  "correlationId": "corr_..."
}
```

User hanya wajib mengirim pesan singkat. Screenshot opsional, previewed, explicitly sent, validated, limited, access-controlled, dan disimpan pada secure file/object storage. Jika attachment gagal, message issue tetap ada; pengiriman tanpa attachment memerlukan pilihan user, bukan silent drop.

User status sederhana:

```text
DIKIRIM
SEDANG_DIPERIKSA
SEDANG_DITINDAKLANJUTI
SELESAI
```

Support internal status/category/priority/note tidak dipaksakan ke form user.

## 16.3 Batch Audit

Offline audit sync hanya membawa safe metadata:

```json
{
  "contractVersion": "1.0",
  "events": [
    {
      "clientEventId": "audit_local_01J...",
      "processId": "proc_...",
      "featureId": "FEAT-PDF-005",
      "status": "COMPLETED",
      "processingMode": "LOCAL",
      "inputSizeBytes": 245901,
      "outputSizeBytes": 233804,
      "errorCode": null,
      "appVersion": "1.4.0",
      "correlationId": "corr_...",
      "occurredAt": "2026-08-24T10:00:00Z"
    }
  ]
}
```

Authenticated user reference dan effective role/permission context ditambahkan server-side agar tidak dapat dipalsukan client. Document content, password, token, clipboard, screenshot, full OCR text, dan signature data dilarang. Enterprise audit tidak dapat dihapus melalui personal history API.

---

# 17. Event SignalR dan Pemulihan REST

## 17.1 Otoritas dan Pengiriman

- state dipersist sebelum event publish;
- delivery at-least-once;
- client idempotent;
- SignalR tidak mengeksekusi command;
- job correctness tidak bergantung pada event delivery;
- REST persisted state selalu menang.

## 17.2 Envelope

```json
{
  "eventId": "evt_01J...",
  "eventType": "JobProgress",
  "eventVersion": 1,
  "streamId": "job:job_01J...",
  "sequence": 17,
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

For job stream, `sequence` berasal dari persisted job `stateVersion`. Activity dan issue stream memakai persisted revision masing-masing. Tidak ada global ordering lintas-stream. Timestamp hanya display/diagnostic.

## 17.3 Katalog Event

```text
JobQueued
JobStarted
JobProgress
JobWaiting
JobCompleted
JobFailed
JobCancelled
JobRetrying
ActivityUpdated
IssueStatusChanged
```

`JobWaiting` hanya memakai status `WAITING_CONNECTION` atau `WAITING_RESOURCE`.

## 17.4 Larangan Payload

Event tidak membawa:

- document/preview/OCR content;
- password/token/secret/connection string;
- signature asset/certificate material;
- prompt/chat content;
- stack trace/exception/raw engine message;
- physical path/storage key;
- arbitrary display text.

`currentStep` dan `messageCode` adalah stable safe codes yang dilokalkan client.

## 17.5 Hub dan Subscription

- one authenticated Activity Hub at `/hubs/activity`;
- server-derived user group, bukan client-supplied user ID;
- user group menerima owned activity, job status, dan issue-status notification;
- optional per-job detail subscription memeriksa ownership/permission dan mempunyai subscription limit;
- no arbitrary user/tenant/issue/correlation group;
- no per-tool/per-Feature-ID hub;
- no upload/download/cancel/retry/process command through hub;
- local job uses typed local IPC and projects to local Activity Center.

## 17.6 Perilaku Duplikat, Urutan, Gap, dan Stale

- duplicate `eventId` ignored;
- `sequence <= lastAppliedSequence` ignored as stale/duplicate;
- `sequence == last + 1` can be applied;
- `sequence > last + 1` triggers REST reconciliation;
- terminal state never regresses from delayed event;
- unknown event type or unsupported event version triggers safe telemetry + REST reconciliation, not crash;
- unknown total uses null/indeterminate progress.

## 17.7 Alur Reconnect

```text
SignalR disconnected
→ realtime marked degraded; job not marked failed
→ server job continues and persists state
→ bounded exponential reconnect + jitter
→ REST reconcile active/recent jobs, activity, and relevant issues
→ apply persisted state/revision
→ resubscribe authorized active job
→ Activity Center recovers
```

Jika realtime tetap tidak tersedia, polling REST terbatas yang mempertimbangkan visibility dapat digunakan untuk job server aktif. Reconnect tidak pernah mengirim ulang perintah execute.

# 18. Keamanan, Rate Limit, Anggaran Sumber Daya, dan Retensi

## 18.1 Penegakan Spesifik Endpoint

| Keluarga route | Penegakan wajib |
|---|---|
| Feature/workflow execution | Authentication; Feature permission; input ownership; policy/classification; handler/schema exposure; capability/engine/capacity; consent |
| Upload/session | Owner; purpose; feature policy; state/version; byte/count/storage limit; validation/quarantine |
| Job/status/action/result | Owner or explicit support permission; current state; retry/cancel/checkpoint policy; result retention |
| History | Personal owner scope; enterprise audit remains separate |
| Secured files | Owner/permission; version; encryption; quota; retention/recycle state; reauth for high-impact deletion when policy requires |
| Issue | Reporter owns message/status; attachment requires explicit send and validation |
| Support | Separate list/detail/attachment/triage/note/export permissions; no implicit document access |
| Audit | Auditor permission; authorization before count/query/export; no user delete |
| Configuration/admin | Granular read/write permission; typed version; `If-Match`; hard safety ceiling cannot be bypassed |
| Health | Minimal safe response; detailed diagnostics restricted to operations boundary |
| SignalR subscription | Authenticated server-derived user group; per-job ownership/permission recheck |

Opaque IDs, cursor, Job ID, Correlation ID, `fileRef`, and storage reference never grant authorization.

## 18.2 Invarian Keamanan dan Privasi

- HTTPS and encryption at rest for approved server-stored payload according to policy.
- Secure token/credential storage.
- No secret in source.
- No direct SQL from client.
- File/document content remains untrusted after format validation.
- Prompt/document injection cannot change system policy, tool permission, or handler selection.
- No document content, full OCR text, password, token, clipboard, signature asset, prompt/chat, screenshot, or secret in logs/telemetry/audit by default.
- Issue screenshot only after capture, preview, optional markup, and explicit send.
- Unlock PDF requires valid password/authority; no protection bypass.
- Redaction final must remove/flatten sensitive content per approved engine capability, not overlay only.
- Dependency/license/vulnerability review, automated security scanning, VA, and PenTest remain mandatory.

## 18.3 Admission Control Berlapis

Batas efektif adalah nilai paling ketat dari:

```text
system safety ceiling
enterprise policy
environment capacity
subject scope
endpoint/workload category
worker/storage/database capacity
approved provider capacity
```

Partisi utama untuk request terautentikasi: subject ID stabil + kategori endpoint. Sinyal IP/jaringan hanya pelengkap, bukan identitas.

Categories:

```text
authentication/session
metadata read
command/write
issue submission/attachment
upload/download
background job admission
expensive AI/OCR/conversion
admin/audit export
```

Controls:

- per-subject request quota;
- weighted expensive-job quota;
- per-user/category/global concurrency;
- upload stream/session/temp-storage limits;
- secured storage quota;
- export quota;
- provider timeout/circuit/capacity;
- atomic reservation before `202`;
- reservation expiry/release on reject/cancel/terminal/cleanup.

## 18.4 Semantik Backpressure

```text
429 → caller rate/quota/concurrency; Retry-After required
413 → hard payload/file/request limit
409 → storage quota or resource/state conflict
503 → shared queue/worker/provider saturation unsafe to queue
```

Queue must be bounded, fair, persistent, and workload-separated. Worker concurrency may reduce under CPU/RAM/storage/DB/provider pressure. Retry and offline backlog release use bounded exponential backoff + jitter.

Graceful degradation preserves safe local capability. Internal AI outage does not disable deterministic/local tools and never falls back to public AI.

Exact numeric rates, weights, thresholds, timeouts, TTL, concurrency, and queue size are typed policy values approved after PoC, benchmark, load/security/license review, and safety headroom.

## 18.5 Siklus Hidup, Retensi, dan Pembersihan

| Data/resource | Aturan |
|---|---|
| Incomplete upload | Expires after configurable inactivity; partial/orphan data cleaned idempotently |
| READY upload | Leased through job/recovery window; cannot be deleted while referenced |
| Intermediate/checkpoint | Temporary; retained only for safe recovery window/lease |
| Job result | Available until visible expiry; then bytes removed and manifest becomes `EXPIRED` |
| Secured file | Created only by explicit Amankan ke Akun; maximum three latest versions |
| Account Recycle | Separate from local recycle; restore/permanent delete follow server policy and confirmation |
| Personal history | User-manageable; deletion does not delete enterprise audit |
| Enterprise audit | Retention/archival by official policy; no arbitrary user deletion |
| Issue/attachment | Metadata and attachment have policy-based retention; attachment remains permission-controlled |
| Idempotency record | Retained long enough to cover safe retry window, then cleaned without creating duplicate effect |

Cleanup uses persisted lifecycle state, lease/ref-count, tombstone, retry, and orphan reconciliation. Age alone must not delete an actively referenced resource.

---

# 19. Pemetaan API Primary PDF Tool — 33 Core Cards

Interpretation:

```text
LOCAL      = baseline server-processing API not needed
OPTIONAL   = approved capability-aware server path may use canonical execute API
REQUIRED   = universal server dependency; none in current Core baseline
COND       = SignalR only for server background execution
NO         = local background progress uses typed local IPC, not SignalR
```

| No | Primary Tool | Canonical Feature ID | Server profile | PoC | Processing API | SignalR | Security |
|---:|---|---|---|:---:|---|---|---|
| 1 | Merge PDF | `FEAT-PDF-005` | LOCAL | No | No | No | Low |
| 2 | Split PDF | `FEAT-PDF-006 / FEAT-PDF-007` | LOCAL | No | No | No | Low |
| 3 | Remove pages | `FEAT-PDF-013` | LOCAL | No | No | No | Low |
| 4 | Extract pages | `FEAT-PDF-014` | LOCAL | No | No | No | Low |
| 5 | Organize PDF | `FEAT-PDF-010..016` | LOCAL workflow | No | No | No | Low |
| 6 | Scan to PDF | `FEAT-SCAN-014` | LOCAL/device | Yes: physical scanner | No | No | Low |
| 7 | Compress PDF | `FEAT-PDF-023` | LOCAL | No | No | No | Low |
| 8 | Repair PDF | `FEAT-PDF-025 / FEAT-PDF-026` | OPTIONAL advanced fallback | No formal gate | Conditional | Cond | Low |
| 9 | OCR PDF | `FEAT-OCR-004` | OPTIONAL complex/large fallback | Yes | Conditional | Cond | Medium |
| 10 | JPG to PDF | `FEAT-CONV-007` | LOCAL | No | No | No | Low |
| 11 | Word to PDF | `FEAT-CONV-001` | OPTIONAL | Yes | Conditional | Cond | Low |
| 12 | PowerPoint to PDF | `FEAT-CONV-003` | OPTIONAL | Yes | Conditional | Cond | Low |
| 13 | Excel to PDF | `FEAT-CONV-002` | OPTIONAL | Yes | Conditional | Cond | Low |
| 14 | HTML to PDF | `FEAT-CONV-017` | OPTIONAL | Yes | Conditional | Cond | Medium |
| 15 | PDF to JPG | `FEAT-CONV-012` | LOCAL | No | No | No | Low |
| 16 | PDF to Word | `FEAT-CONV-004` | OPTIONAL | Yes | Conditional | Cond | Low |
| 17 | PDF to PowerPoint | `FEAT-CONV-006` | OPTIONAL | Yes | Conditional | Cond | Low |
| 18 | PDF to Excel | `FEAT-CONV-005` | OPTIONAL | Yes | Conditional | Cond | Low |
| 19 | PDF to PDF/A | `FEAT-PDF-075` | OPTIONAL | Yes | Conditional | Cond | Medium |
| 20 | Rotate PDF | `FEAT-PDF-012` | LOCAL | No | No | No | Low |
| 21 | Add page numbers | `FEAT-PDF-037` | LOCAL | No | No | No | Low |
| 22 | Add watermark | `FEAT-PDF-036` | LOCAL | No | No | No | Low |
| 23 | Crop PDF | `FEAT-PDF-019` | LOCAL | No | No | No | Low |
| 24 | Edit PDF | `FEAT-PDF-031..041` | OPTIONAL existing-text subset | Yes | Conditional | Cond | Medium |
| 25 | PDF Forms | `FEAT-PDF-053..060` | LOCAL workflow baseline | No formal gate | No | No | Low |
| 26 | Unlock PDF | `FEAT-PDF-062` | LOCAL | No | No | No | High |
| 27 | Protect PDF | `FEAT-PDF-061` | LOCAL | No | No | No | Medium |
| 28 | Sign PDF | `FEAT-SIGN-013` | LOCAL visual non-PKI | No | No | No | Medium |
| 29 | Redact PDF | `FEAT-PDF-069` | LOCAL | No | No | No | High |
| 30 | Compare PDF | `FEAT-PDF-071 / FEAT-PDF-072` | LOCAL/Local Worker | No | No | No | Low |
| 31 | AI Summarizer | `FEAT-INTEL-001` | OPTIONAL approved internal | Yes | Conditional | Cond | Medium |
| 32 | Translate PDF | `FEAT-INTEL-002` | OPTIONAL approved internal | Yes | Conditional | Cond | Medium |
| 33 | PDF to Markdown | `FEAT-CONV-036` | OPTIONAL complex OCR/layout | Yes | Conditional | Cond | Low |

Summary:

```text
Local-only processing baseline : 19 cards
Optional-server path           : 14 cards
Universal required-server      : 0 cards
```

Core priority does not bypass feasibility, security, license, capacity, or release gates. Background work does not imply server work.

---

# 20. Serah Terima Tanggung Jawab API ↔ Persistence

Document 07 owns physical database design. Document 06 only defines persistence responsibility:

| Resource API | Tanggung jawab persistence | Payload biner |
|---|---|---|
| Compatibility/config/feature exposure | Server metadata/configuration | None |
| Upload session | Server transfer metadata + lifecycle/lease | Secure temporary/object storage |
| Job/attempt/checkpoint summary | Persisted server job state | Intermediate/checkpoint storage outside SQL blob default |
| Result manifest | Persisted result metadata/retention | Secure temporary/object storage |
| History | Personal metadata | No document blob by default |
| Secured file/version | Ownership/version/retention metadata | Approved secure file/object storage |
| Audit event | Safe enterprise metadata, separate from history | No document/screenshot/signature content |
| Issue | Message/status/safe context metadata | Attachment in secure file/object storage |
| Policy/feature flag/limit | Typed versioned metadata + concurrency token | None |
| Assistant | Intent category/Feature ID/status/correlation metadata only when required | No chat/document content by default |
| SignalR | No separate authority; derived from persisted revision | None |

Klien tidak pernah mengakses SQL Server secara langsung. Penulisan lintas modul dan kepemilikan database ditetapkan dalam Dokumen 07.

---

# 21. Ketertelusuran Requirement dan Feature

## 21.1 Requirement API

| Requirement | Cakupan kontrak |
|---|---|
| `IR-API-001` | `/api/v1`, endpoint inventory, ASP.NET Core Web API boundary |
| `IR-API-002` | Section 9 and endpoint-specific enforcement |
| `IR-API-003` | Section 12 streamed/resumable upload and hard limits |
| `IR-API-004` | Sections 11.5 and 13 Job ID/durable 202 |
| `IR-API-005` | Section 7 typed standard error |
| `IR-API-006` | Section 7 safe error prohibition |
| `IR-API-007` | Section 8 health routes |
| `IR-API-008` | Sections 17–18 safe telemetry/event rules |
| `IR-API-009` | Sections 11.7 and 18 approved internal provider only |
| `IR-API-010` | Section 11 versioned Feature/workflow/Assistant contract |
| `IR-API-011` | Sections 10.5 and 16.2 issue/status/support/export |
| `IR-API-012` | Sections 10.5, 16.2, and 18 attachment limit/validation/access/storage |

## 21.2 Grup Fungsional dan Nonfungsional

| Grup requirement | Bagian API |
|---|---|
| `FR-PROC-001..005` | 4, 11, 12, 19 |
| `FR-BACKUP-001..010` | 10.4, 16.1, 18.5 |
| `FR-HIST-001..003` | 10.4, 15 |
| `FR-AUDIT-001..004` | 10.6, 16.3, 18.5 |
| `FR-REC-001..006` | 13, 14, 17 |
| `FR-ASSIST-001..015` | 11.6–11.7, 17 |
| `FR-SUP-001..007` | 10.5, 16.2, 18.1 |
| `NFR-PERF-003,007..013` | 12, 15, 18, 19 |
| `NFR-REC-001..005` | 12–14, 17–18 |
| `NFR-ARCH-002..004` | 4, 9, 20 |
| `SR-SEC-001..023` | 7, 9, 12, 16–18 |

## 21.3 Grup Feature Canonical

| Grup Feature | Cakupan kontrak |
|---|---|
| `FEAT-PROC-002..005` | execution resolver and Local/Server boundary |
| `FEAT-ASSIST-009` and Assistant catalog | canonical invocation/workflow boundary |
| `FEAT-VALID-001..018` | transfer and execution validation |
| `FEAT-STOR-011..016` | temporary/local storage boundary and secured copy |
| `FEAT-REC-001..015` | job retry/resume/checkpoint/recovery |
| `FEAT-PROG-001..008` | job progress, Activity Center, SignalR projection |
| `FEAT-AUTH-001..012` | identity/account/permission boundary |
| `FEAT-HIST-013..015` | history query/actions |
| `FEAT-AUDIT-002..008` | safe audit batch/query/export |
| `FEAT-SUPPORT-001..008` | issue, attachment, status, support, export |
| `FEAT-MON-004..007,013` | correlation, state, telemetry, health |
| 33 Core mappings | Section 19 |

Seluruh Feature ID eksplisit yang dirujuk dalam dokumen ini tersedia di Feature Catalog v1.3. Rentang merujuk pada sequence canonical yang ada dan tidak membuat ID baru.

---

# 22. Requirement Pengujian Kontrak

Minimum automated tests:

## 22.1 OpenAPI dan Kompatibilitas

- generated OpenAPI validates;
- normalized diff detects breaking route/schema/status/enum changes;
- operation IDs stable and unique;
- every server-executable Feature ID has typed schema, handler, authorization, and test;
- old supported client/contract versions remain compatible or receive correct compatibility status.

## 22.2 Autentikasi dan Keamanan

- invalid issuer/audience/signature/lifetime → 401;
- permission/ownership/policy negative cases → 403 or consistent 404;
- IDOR tests for upload, job, result, history, secured file, issue, attachment, audit, export;
- token/secret/password/document/OCR/signature/screenshot content absent from logs/telemetry/errors/events;
- arbitrary Feature ID/engine/path/URL/SQL/field/filter rejected;
- public AI/provider fallback impossible.

## 22.3 File dan Job

- streaming does not whole-file buffer;
- size/count/MIME/signature/structure/decompression/security controls;
- chunk duplicate/range/digest/resume/expiry;
- same idempotency key never creates duplicate upload/job/effect;
- 202 only after durable state;
- cancel safe point, bounded retry, checkpoint compatibility, result validation, expiry, range download;
- worker/client/SignalR disconnect does not corrupt source or authoritative state.

## 22.4 Query dan Kapasitas

- page maximum and cursor tamper/expiry/scope/permission change;
- stable ordering and authorization before count;
- filter/search/sort allowlist and query timeout/date-span guard;
- 429/413/409/503 semantics and `Retry-After`;
- bounded queue, reservation cleanup, retry storm/backlog release, graceful degradation.

## 22.5 SignalR

- duplicate/stale event idempotency;
- per-stream order, sequence gap, unknown type/version;
- REST reconciliation after reconnect/sleep/failover;
- terminal state never regresses;
- no event-driven business side effect;
- local job uses local IPC, not SignalR.

## 22.6 Core dan Assistant

- all 33 cards map to correct canonical Feature IDs;
- local-only cards make no processing API call;
- optional-server path requires resolver/policy/consent;
- Tool-First, File-First, and Assistant-First share the same contract;
- Assistant output remains untrusted until validation and result grounding.

---

# 23. Checklist Penerimaan Final

```text
[x] 9/9 API decisions locked
[x] Base route and compatibility route defined
[x] Version layers separated
[x] Standard headers, identifiers, response, error, and HTTP semantics defined
[x] Authentication/authorization boundary defined
[x] Endpoint inventory completed
[x] Canonical Feature execution and WorkflowPlan defined
[x] Assistant and Content Intelligence boundary defined
[x] File upload/download/preflight/resume/validation defined
[x] Job create/status/cancel/retry/resume/result/recovery defined
[x] Idempotency contract defined
[x] Pagination/filter/search/sort defined
[x] Account/history/secured-file/issue/support/audit/config/admin routes defined
[x] SignalR envelope/catalog/reconnect/REST reconciliation defined
[x] Rate/quota/backpressure/retention defined
[x] Endpoint-specific security boundary defined
[x] 33 Primary PDF Tool mapping included
[x] Requirement and Feature traceability included
[x] API-to-persistence handoff included
[x] Contract test requirements included
[x] No Feature ID mismatch found
[x] No Local-First violation found
[x] No duplicate processing engine/service introduced
[x] No public AI endpoint introduced
[x] Sign PDF remains visual non-PKI
[x] Documents 01–05 unchanged
```

---

# 24. Audit Akhir dan Status

Audit against Documents 01–05 and Agent A–D reports:

```text
Decision completeness             : PASS — 9/9 LOCKED
Requirement coverage              : PASS
Feature ID validation             : PASS
Endpoint inventory                : PASS
Schema/example coverage           : PASS
Local-First                       : PASS
REST/SignalR authority            : PASS
Authentication/authorization      : PASS
Security/privacy                  : PASS
Server resource budget            : PASS
Retention/recovery                : PASS
Core 33 mapping                   : PASS
Duplicate endpoint/engine risk    : PASS
Fatal contradiction               : NONE
```

Status final:

```text
06_API_DOCUMENTATION_FINAL_v1.0_PERTAMINA_OTO.md
Status          : FINAL — LOCKED PRE-CODING API BASELINE
Final documents : 6 / 19
Overall progress: 31.6%
Next document   : 07_DATABASE_DESIGN.md
```
