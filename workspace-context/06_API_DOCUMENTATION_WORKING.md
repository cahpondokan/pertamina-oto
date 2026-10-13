# 06 — API DOCUMENTATION
## PERTAMINA OTO — Office Tools Organizer

> **Status:** WORKING — DECISION PHASE  
> **Versi:** 0.5  
> **Tanggal:** 24 Agustus 2026  
> **Baseline feature:** 550 canonical features  
> **Parent:** Documents 01–05 FINAL dan `MASTER_PROJECT_SUMMARY.md`

---

# 1. Tujuan

Dokumen ini mendefinisikan contract REST, file transfer, background job, authentication/authorization boundary, query, error, health, SignalR, compatibility, dan recovery PERTAMINA OTO.

Dokumen ini tidak membuat seluruh 550 canonical feature menjadi server endpoint. Feature Local-First yang berjalan lokal tidak mempunyai server-processing API.

---

# 2. Baseline Terkunci

- REST adalah source of truth.
- SignalR hanya realtime notification.
- REST menjadi recovery/fallback ketika SignalR terputus.
- Feature ID harus canonical.
- Parameter dan result memakai typed/versioned schema.
- Assistant hanya mengorkestrasi canonical application contract.
- Local-only tool tetap memakai local application service atau Local Worker.
- Background server job mempunyai `jobId` dan `correlationId`.
- Standard error memuat `code`, `message`, dan `correlationId`.
- Stack trace, secret, password, token, dan isi dokumen tidak dikirim kepada client.
- 33 Primary PDF Tool Cards tidak boleh menghasilkan endpoint atau engine duplikat.
- `Sign PDF` hanya visual signature/paraf non-PKI.
- Public AI endpoint dan certificate/PKI Digital Signature berada di luar scope.

---

# 3. Auto-Locked API Rules

## API-AUTO-001 — Standard Error Envelope

```json
{
  "code": "PDF_INVALID_INPUT",
  "message": "File PDF tidak valid.",
  "correlationId": "corr_...",
  "details": null
}
```

`details` hanya safe structured validation information.

## API-AUTO-002 — Background HTTP Semantics

- `200 OK`: read atau immediate command berhasil.
- `201 Created`: resource addressable baru dibuat dan response mempunyai `Location`.
- `202 Accepted`: background work sudah dipersist; response mempunyai `jobId`, `correlationId`, status, dan `Location`.
- `202` bukan bukti processing berhasil.

## API-AUTO-003 — Correlation

`correlationId` diteruskan melalui client, API, application service, Worker, storage, log, telemetry, SignalR, dan safe issue diagnostics.

## API-AUTO-004 — Capability-Aware Server Use

- `ServerDependency=None`: tidak mempunyai server-processing API.
- `Optional`: server hanya dipakai setelah resolver, policy, capability, connectivity, dan consent mengizinkan.
- `Required`: memakai secure server contract.
- Background local job memakai Local Worker + typed local IPC, bukan SignalR.

## API-AUTO-005 — Health

Health minimum dibagi menjadi liveness dan readiness. Detail Production dibatasi berdasarkan security policy.

## API-AUTO-006 — `fileRef` dan `uploadId`

Keduanya mempunyai fungsi berbeda dan tidak saling menggantikan:

- `uploadId` adalah identitas resource transfer/upload beserta lifecycle `CREATED`, `UPLOADING`, `VALIDATING`, `READY`, `FAILED`, `ABORTED`, atau `EXPIRED`.
- `fileRef` adalah typed opaque input reference pada execution/workflow contract.
- Untuk input upload, `fileRef.kind=UPLOAD` dan `fileRef.id` berisi `uploadId` yang sudah `READY`.
- Kind lain hanya boleh berasal dari resource resmi, misalnya secured file atau validated job result.
- Physical path, storage key, URL arbitrary, dan filename tidak pernah menjadi authority.
- Server selalu memeriksa ownership, permission, purpose, state, dan version.

Contoh:

```json
{
  "inputId": "source-1",
  "fileRef": {
    "kind": "UPLOAD",
    "id": "upl_123"
  }
}
```

## API-AUTO-007 — Waiting State, Bukan `PAUSED`

- Tidak ditambahkan state `PAUSED`.
- Kondisi menunggu memakai persisted state `WAITING_CONNECTION` atau `WAITING_RESOURCE`.
- SignalR memakai event `JobWaiting` dengan salah satu status tersebut.
- Event tidak boleh menciptakan state baru.
- True user-pause berada di luar baseline sampai ada controlled scope revision.

---

# 4. Decision Register — 9/9

| No | Keputusan | Status |
|---:|---|---|
| 1 | Tool / Feature API Surface Strategy | **LOCKED — C: Hybrid API Surface** |
| 2 | API Contract / OpenAPI Strategy | **LOCKED — C: Governed Hybrid OpenAPI** |
| 3 | Major API Version Routing | **LOCKED — A: URI Major + Stable Compatibility** |
| 4 | File Transfer Strategy | **LOCKED — Hybrid File Transfer** |
| 5 | Idempotency & Retry Contract | **LOCKED — Persisted Record + Fingerprint** |
| 6 | Pagination / Filter / Sort Standard | **LOCKED — Hybrid List Query Standard** |
| 7 | Client Authentication Boundary | **LOCKED — Pertamina Identity + Server Authorization** |
| 8 | SignalR Event Envelope & Recovery Contract | **LOCKED — Versioned At-Least-Once Notification** |
| 9 | Rate Limit / Quota / Backpressure Contract | **LOCKED — Layered Admission Control** |

---

# 5. API-DEC-001 — Tool / Feature API Surface Strategy

**Status:** LOCKED — USER APPROVED  
**Decision:** C — Hybrid API Surface

## 5.1 Canonical server processing

```http
POST /api/{major}/features/{featureId}/execute
```

Route ini hanya menerima Feature ID canonical yang:

- aktif dan diekspos ke server API;
- mempunyai registered typed handler;
- mempunyai typed `ParameterSchemaVersion` dan result contract;
- lolos authentication, permission, ownership, policy, capability, resource, engine, capacity, dan consent check.

Generic execution adalah closed dispatch, bukan arbitrary RPC. Client tidak boleh mengirim nama engine, service, class, assembly, SQL, local path, arbitrary URL, atau command.

## 5.2 Resource/domain API

Resource yang mempunyai identity, lifecycle, ownership, query, atau authorization sendiri memakai endpoint domain:

```text
/api/{major}/account/...
/api/{major}/jobs/...
/api/{major}/history/...
/api/{major}/issues/...
/api/{major}/secured-files/...
/api/{major}/audit/...
/api/{major}/admin/...
```

## 5.3 Local-First boundary

- Local-only tool tidak mempunyai server-processing endpoint.
- Tool Card berupa bundle tidak memperoleh endpoint per card.
- Bundle memakai versioned workflow plan berisi canonical Feature ID.
- UI Tool-First, File-First, dan Assistant memakai application contract yang sama.
- Assistant tidak mempunyai execution bypass.

## 5.4 Core mapping result

```text
33 Primary PDF Tool Cards
19 local-only processing baseline
14 optional-server processing path
0 universally required-server processing path
```

Optional-server path tetap capability-aware dan PoC-gated. Resource API bersama tidak mengubah tool lokal menjadi server-processing tool.

## 5.5 Traceability

```text
Requirements:
IR-API-001..012
FR-PROC-001..005
FR-ASSIST-003..008
NFR-ARCH-002..004

Features:
FEAT-PROC-002..005
FEAT-ASSIST-009
33 Primary PDF Tool mappings dari Feature Catalog v1.3

Architecture:
Solution Design sections 14, 24, dan 25
Technical Reference execution, transport, dan Assistant boundaries
```

---

# 6. API-DEC-002 — API Contract / OpenAPI Strategy

**Status:** LOCKED — USER APPROVED  
**Decision:** C — Governed Hybrid OpenAPI

## 6.1 Contract source

- Typed/versioned C# transport dan application contract menjadi sumber implementasi.
- OpenAPI dihasilkan otomatis dari endpoint, typed DTO, validation, dan schema metadata.
- Dokumentasi API manual bukan source of truth.
- Engine/provider class tidak boleh menjadi transport contract.

## 6.2 Contract artifact

- OpenAPI dinormalisasi dan disimpan sebagai versioned contract artifact per API major.
- `operationId` harus eksplisit dan stabil.
- Named schema component dibuat untuk common envelope serta setiap server-executable Feature ID.
- Local-only feature tidak dipublikasikan sebagai HTTP processing operation.
- Shared local/server schema dapat berada pada Feature Contract Catalog terpisah.

## 6.3 CI governance

CI wajib menjalankan:

```text
OpenAPI generation
OpenAPI validation
Normalized snapshot comparison
Breaking-change diff
Serialization/validation tests
Request/response contract tests
Registered-handler completeness tests
Safe example and redaction tests
```

Breaking change memblokir merge kecuali perubahan secara eksplisit menyediakan API major baru atau compatibility/migration path yang disetujui.

## 6.4 Compatibility guard

- Optional additive field harus mempunyai default/fallback yang kompatibel.
- Client harus memiliki unknown field/error-code handling yang aman.
- Removal, rename, type/meaning change, required-field addition, incompatible constraint, dan status-semantic change diperlakukan sebagai breaking atau potentially breaking.
- OpenAPI tidak menggantikan runtime authorization, policy, ownership, atau capability checks.

## 6.5 Traceability

`IR-API-001..012`; versioned contract requirements; `API-DEC-001`; Solution Design compatibility, REST, Assistant, dan architecture-test boundaries; Technical Reference testing dan CI security gates.

---

# 7. API-DEC-003 — Major API Version Routing

**Status:** LOCKED — USER APPROVED ALL AGENT RECOMMENDATIONS  
**Decision:** A — URI Major + Stable Compatibility Endpoint

Functional API memakai major version pada URI:

```text
/api/v1/...
/api/v2/...
```

Bootstrap compatibility tidak memakai major route:

```http
GET /api/compatibility
```

- Major route naik hanya untuk breaking HTTP change.
- Additive compatible change tetap berada pada major aktif.
- v1 dan v2 dapat berjalan berdampingan selama support window.
- `AppVersion`, `ContractVersion`, `ParameterSchemaVersion`, `WorkflowSchemaVersion`, `ConfigurationSchemaVersion`, dan `CapabilityVersion` tidak dimasukkan ke URI.
- Compatibility response minimum: `CurrentVersion`, `MinimumSupportedVersion`, `ContractVersion`, `CapabilityVersion`, status, supported majors, dan default major.
- Status compatibility: `SUPPORTED`, `UPDATE_AVAILABLE`, `UPDATE_REQUIRED`, atau `UNSUPPORTED`.

**Traceability:** compatibility requirements; Solution Design sections 23–24; Technical Reference versioned contract baseline; `API-DEC-002`.

---

# 8. API-DEC-004 — File Transfer Strategy

**Status:** LOCKED — USER APPROVED ALL AGENT RECOMMENDATIONS  
**Decision:** Hybrid File Transfer Contract

- Local-only processing tidak melakukan upload.
- Optional-server transfer hanya dimulai setelah resolver memilih server serta permission, policy, capability, capacity, connectivity, dan user consent mengizinkan.
- File kecil/menengah memakai streamed `multipart/form-data` one-shot.
- File besar, koneksi tidak stabil, atau restart cost tinggi memakai resumable upload session dengan sequential raw chunks, `Content-Range`, digest, committed offset, expiry, dan abort.
- Whole-file buffering di RAM dilarang.
- Upload harus melewati byte limit, extension/MIME/signature consistency, structural/decompression validation, dan security inspection sesuai policy.
- Hanya upload berstatus `READY` dapat menjadi execution input.
- Download hasil memakai authorized streaming, manifest, `ETag`, `Range`, dan `If-Range`.
- Temporary processing input/result berbeda lifecycle dari secured file. Hanya aksi eksplisit **Amankan ke Akun** membuat secured copy.
- Exact size, threshold, TTL, retention, dan concurrency values berasal dari typed policy + benchmark/security/capacity test.

**Traceability:** `FR-FILE-010..015`; `FR-PROC-001..005`; `FR-STOR-001..006`; `FR-BACKUP-001..010`; `IR-API-003..008`; file validation, storage, recovery, dan security Feature IDs; Solution Design sections 9, 11–13, 17, 24, 26–28.

---

# 9. API-DEC-005 — Idempotency & Retry Contract

**Status:** LOCKED — USER APPROVED ALL AGENT RECOMMENDATIONS  
**Decision:** Persisted Idempotency Record + Request Fingerprint

- `Idempotency-Key` wajib untuk mutating command yang membuat upload/session/job/attempt/cancel/retry/resume atau secured copy.
- Scope key: authenticated principal/tenant + method + canonical route + contract version.
- Server menyimpan key hash, request fingerprint, logical correlation ID, resource ID, status, safe response snapshot, dan expiry.
- Idempotency record serta resource dibuat secara konsisten sebelum success atau `202` dikirim.
- Key dan fingerprint sama mengembalikan resource/response yang sama.
- Key sama dengan fingerprint berbeda menghasilkan `409 IDEMPOTENCY_KEY_REUSED`.
- Chunk upload idempotent berdasarkan `uploadId + range + digest`.
- Transport retry tidak membuat Job ID baru.
- Automatic retry, explicit retry, dan resume mempertahankan satu logical `jobId`; setiap attempt dicatat append-only.
- Retry hanya untuk transient error allowlist, bounded, memakai exponential backoff + jitter.
- Resume hanya memakai compatible verified checkpoint. Tidak ada silent restart dari awal.

**Traceability:** `FR-REC-001..006`; `NFR-REC-001..005`; `NFR-PERF-009..010`; `IR-API-004..008`; recovery/progress/monitoring Feature IDs; Solution Design sections 11–13, 21, 24, 27–28.

---

# 10. API-DEC-006 — Pagination / Filter / Sort Standard

**Status:** LOCKED — USER APPROVED ALL AGENT RECOMMENDATIONS  
**Decision:** Hybrid List Query Standard

- Bounded ordinary metadata list memakai `page` + `pageSize`.
- Audit, issue, secured-file, history/job, dan large/changing list memakai opaque keyset cursor + `pageSize`.
- Satu endpoint tidak menerima page dan cursor bersamaan.
- Default umum `page=1`, `pageSize=25`, maximum `100`.
- History/job memakai default `25`, maximum `50`.
- Invalid atau excessive value menghasilkan `400`; tidak di-clamp diam-diam.
- Cursor harus opaque, integrity-protected, expiring, scope-bound, dan di-authorize ulang setiap continuation.
- Exact total tidak wajib untuk cursor list.
- Sort harus stabil dan memakai unique tie-breaker.
- `sort`, `filter[field]`, `search`, `from`, dan `to` hanya menerima allowlisted field/operator.
- Raw expression, regex, arbitrary field path, dynamic SQL, document-content search, dan client-defined authorization filter dilarang.
- Authorization scope diterapkan sebelum pagination dan count.
- Wide report atau exact snapshot memakai permission-gated asynchronous export job.

**Traceability:** `FR-HIST-001..003`; `FR-AUDIT-001..004`; `FR-SUP-001..007`; `NFR-PERF-007..013`; `SR-SEC-001..023`; history, audit, issue, secured-file, dan recovery Feature IDs.

---

# 11. API-DEC-007 — Client Authentication Boundary

**Status:** LOCKED — USER APPROVED ALL AGENT RECOMMENDATIONS  
**Decision:** Pertamina Identity + Server-Enforced Permission/Ownership/Policy

```text
Client
→ Pertamina Identity
→ API authentication
→ permission + ownership + enterprise policy
→ application service
```

- HTTPS wajib.
- API memvalidasi signature, allowed algorithm, issuer, audience, lifetime, subject, serta approved claim contract.
- Authorization deny-by-default, permission-based, object-aware, dan server-enforced.
- Role hanya mengelompokkan permission; role tidak menjadi authorization final.
- Administrator tidak otomatis dapat membaca file user.
- Superadmin/Support adalah permission-gated support surface, bukan bypass atau role baru otomatis.
- Native Desktop/Mobile memakai approved authorization flow dengan PKCE dan secure OS storage.
- Web menjaga credential/token pada server-side session boundary bila sesuai arsitektur; browser-accessible bearer/refresh-token storage dilarang.
- Offline session hanya untuk narrow local capability selama time-bounded policy masih valid. Server/sensitive action menunggu online authorization.
- Worker/internal provider memakai service identity terpisah dan job-scoped access; end-user token tidak diteruskan.
- `401` untuk authentication invalid/missing; `403` untuk valid identity yang ditolak permission/policy; consistent `404` boleh dipakai untuk concealment resource sensitif.
- Exact protocol, issuer, claim mapping, credential technology, offline TTL, dan sensitive-action catalog mengikuti approved Pertamina Identity, Security Design, dan Role Matrix tanpa membuka kembali boundary ini.

**Traceability:** `FR-AUTH-001..005`; `FR-ROLE-001..003`; `SR-SEC-001..023`; `IR-API-002`; auth, account, support, audit, dan security Feature IDs; Solution Design sections 19, 22.6, dan 26.

---

# 12. API-DEC-008 — SignalR Event Envelope & Recovery Contract

**Status:** LOCKED — USER APPROVED ALL AGENT RECOMMENDATIONS  
**Decision:** Persisted REST State + Versioned At-Least-Once Realtime Notification

- State dipersist sebelum SignalR event dipublikasikan.
- SignalR delivery diperlakukan at-least-once; client wajib idempotent.
- Minimum envelope: `eventId`, `eventType`, `eventVersion`, `streamId`, `sequence`, applicable resource ID, canonical `featureId` bila relevan, `correlationId`, UTC timestamp, persisted status, safe progress, `currentStep`, counts, dan `messageCode`.
- `sequence` monoton naik per stream; tidak ada global ordering.
- Duplicate/stale event diabaikan. Sequence gap, unknown type, atau unsupported version memicu REST reconciliation.
- Timestamp tidak menentukan ordering.
- Satu authenticated Activity Hub melayani user-scoped job, Activity Center, dan issue-status notification.
- Hub tidak menerima upload, processing, retry, cancel, atau business command.
- Server menentukan user group dari authenticated identity. Optional per-job subscription selalu memeriksa ownership/permission.
- Disconnect tidak menghentikan job. Reconnect memakai bounded backoff + jitter, lalu REST reconciliation dan authorized resubscription.
- Event tidak membawa document/OCR content, password, token, secret, signature asset, prompt/chat, stack trace, raw engine message, atau arbitrary display text.
- Tidak ada state `PAUSED`; `JobWaiting` membawa `WAITING_CONNECTION` atau `WAITING_RESOURCE`.
- Local background job memakai typed local IPC, bukan SignalR.

**Traceability:** `IR-API-004..008`; progress, activity, recovery, monitoring, issue, dan support Feature IDs; Solution Design sections 11–12, 21–24, dan 28.

---

# 13. API-DEC-009 — Rate Limit / Quota / Backpressure Contract

**Status:** LOCKED — USER APPROVED ALL AGENT RECOMMENDATIONS  
**Decision:** Layered Admission Control + Bounded Fair Queue

- Effective limit adalah nilai paling ketat dari system safety, enterprise policy, environment capacity, subject scope, endpoint category, worker capacity, dan provider capacity.
- Primary partition untuk authenticated request adalah stable subject ID + endpoint category. IP hanya signal tambahan.
- Kategori minimum: auth/session, metadata read, command/write, issue, upload/download, job admission, expensive AI/OCR/conversion, dan admin/audit export.
- Per-user request quota, weighted expensive-job quota, concurrent-job limit, upload/session/storage budget, export quota, dan provider capacity diperiksa sebelum admission.
- Reservation dibuat sebelum `202` dan dilepas pada reject, cancel, terminal state, expiry, atau cleanup.
- `429` untuk caller rate/quota/concurrency dan wajib `Retry-After`.
- `413` untuk hard payload limit.
- `409` untuk storage quota/resource conflict.
- `503` untuk shared system/worker/provider saturation yang tidak aman diantrikan; temporary response sebaiknya membawa `Retry-After`.
- Queue wajib bounded, fair, dipisahkan menurut workload class, dan tidak hanya in-memory.
- Retry/backlog release bounded dengan exponential backoff + jitter.
- Saat tekanan naik, safe local capability tetap berjalan; server admission/concurrency diturunkan; public AI fallback tetap dilarang.
- Exact numeric rates, weights, concurrency, queue size, timeout, TTL, dan retention ditetapkan lewat typed policy setelah PoC, benchmark, load test, license/security review, dan safety headroom.

**Traceability:** `NFR-PERF-007..013`; `SR-SEC-001..023`; `IR-API-003..009`; performance, monitoring, recovery, upload, issue, audit, dan provider Feature IDs; Solution Design sections 11.4, 26, dan 27.

---

# 14. Progress

```text
06_API_DOCUMENTATION
Decision progress : 9 / 9
Document status   : ACTIVE
Overall complete  : 5 / 19 FINAL documents
Overall progress  : 26.3%
Next step         : Final cross-document audit and Document 06 finalization
```
