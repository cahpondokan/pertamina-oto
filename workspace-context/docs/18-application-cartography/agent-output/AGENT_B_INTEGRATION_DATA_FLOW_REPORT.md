# AGENT B — INTEGRATION, INTERFACE, DEPENDENCY & DATA-FLOW MAPPING REPORT

**Task ID:** `DOC18-AGENT-B`  
**Scope:** REST, SignalR, local IPC, Server Worker, module contract, internal event, engine/provider, storage, SSO, observability, Audit, Issue, data flow, dan dependency  
**Status:** Rekomendasi kartografi pre-implementation; tidak mengubah API, database, security, atau master Document 18  
**Authoritative baseline:** Document 01–17 FINAL; Document 04, 06, 07, 08, 10, dan 17 menjadi fokus utama

> **Document 17 synchronization:** `17_DEPLOYMENT_GUIDE_FINAL_v1.0_PERTAMINA_OTO.md` telah dibaca dan dipakai sebagai sumber berwenang. Dokumen tersebut mengunci logical deployables, environment separation, build-once promotion, version/compatibility, internal/public network boundary, startup dependency order, Worker drain/lease behavior, storage zones, provider release gate, observability, rollback, dan Incident handoff. Exact Production topology/provider/OS/network/SSO/signing/distribution/DR tetap confirmation gap sebagaimana dinyatakan Document 17; status FINAL bukan bukti deployment atau `PRODUCTION_READY`.

Laporan ini tidak menciptakan endpoint. Seluruh route REST dan event SignalR yang dicantumkan berasal dari Document 06 FINAL. Nama internal event, wire message IPC, provider, queue technology, dan module owner yang belum dikunci ditandai sebagai gap, bukan diisi dengan asumsi tersembunyi.

---

# 1. Integration principles

1. **Local-First tetap authority.** Operasi local-only tidak memanggil API, SignalR, server Worker, SQL Server, object storage, atau provider server. Background tidak identik dengan server; pekerjaan berat lokal menggunakan Local Worker.
2. **Contract before infrastructure.** Modul berkomunikasi melalui typed Contract, Application Interface, atau versioned Internal Event. UI, Assistant, dan modul lain tidak mereferensikan DbContext, table, queue implementation, physical path, object key, vendor SDK, atau provider adapter milik modul lain.
3. **One write owner.** Setiap aggregate/table/object lifecycle mempunyai satu owner. Cross-module write melalui application contract atau post-commit event; tidak ada direct SQL lintas modul.
4. **Authority is persisted.** Job state berasal dari persisted job store; realtime hanya proyeksi. Local job berasal dari SQLite/local artifact manifest. Server metadata berasal dari SQL Server. Payload diamankan berasal dari secure storage. Feature ID berasal dari canonical Feature Registry.
5. **REST commands, SignalR notifications.** Command, upload/download, query, cancellation, retry, resume, admin, Support, dan Audit memakai REST. `/hubs/activity` hanya mengirim notifikasi at-least-once dan tidak menerima command.
6. **Typed and versioned.** API major, ContractVersion, ParameterSchemaVersion, WorkflowSchemaVersion, ConfigurationSchemaVersion, CapabilityVersion, SignalR EventVersion, `IpcContractVersion`, `LocalSchemaVersion`, engine/provider compatibility, checkpoint schema, dan storage manifest harus dapat divalidasi.
7. **Retry requires identity and idempotency.** Mutating/expensive REST command memakai `Idempotency-Key`; upload chunk memakai `uploadId + range + digest`; internal event memakai stable Event ID/stream sequence; Worker attempt tidak mengganti logical Job ID.
8. **Authorization precedes data access.** Authentication, permission, ownership/scope, policy/classification, state/version, capability, consent, connectivity, capacity, dan reauthentication yang berlaku diperiksa sebelum storage resolution, count/query, download, provider call, atau engine dispatch.
9. **Minimal data by interface.** Reference lebih diutamakan daripada bytes. Password/token/secret tidak masuk contract persistence/event/log. Document/OCR/signature/clipboard/AI payload hanya melintasi boundary yang memang membutuhkan konten dan tidak disalin ke Audit/telemetry.
10. **No silent fallback.** Fallback engine/provider hanya pre-approved dan memenuhi kontrol yang sama. Kegagalan server tidak memindahkan local-only work ke server. Kegagalan internal provider tidak beralih ke public AI.
11. **No silent wait.** Accepted long work mempunyai Job ID + Correlation ID, persisted status, visible wait/retry/cancel state, SignalR acceleration, dan REST recovery.
12. **Build/deploy compatibility.** Interface map ditautkan ke API/IPC/event/config/capability/database/component versions, exact artifact digest, dan Release Manifest. Document 17 FINAL mengunci logical deployment dependency; environment-specific executable topology/commands tetap menunggu approval dan runbook resmi.

---

# 2. Interface taxonomy

| Interface type | Producer/caller | Consumer/callee | Protocol/form | Authority/source of truth | Version/idempotency | Security boundary |
|---|---|---|---|---|---|---|
| `CLIENT_TO_API` | Desktop, Mobile/Tablet, Web; Assistant melalui official application path | ASP.NET Core API | HTTPS REST `/api/v1`; stable compatibility/health exceptions | Owning application module + persisted stores | API major + Contract/schema versions; Idempotency-Key on defined commands | Approved SSO/session, permission, owner/scope, policy, reauth, rate/admission |
| `CLIENT_TO_SIGNALR` | Post-commit publisher/outbox via Hub | Authenticated Web/Desktop/Mobile Activity Center | SignalR `/hubs/activity` | Persisted domain state/revision, never Hub memory | EventVersion + eventId + stream sequence; client dedupe | Server-derived user group; optional job subscription reauthorized |
| `DESKTOP_TO_LOCAL_WORKER` | Desktop application/Processing Coordinator | Isolated Local Worker | Named Pipe/local IPC through `ILocalWorkerTransport`; no public port | Desktop-owned `local_job`, local workspace manifest, SQLite pointer | Authenticated handshake + IPC version + request/Correlation ID; operation scoped | Same intended profile/user, restrictive ACL/instance/job binding, closed whitelist |
| `API_TO_SERVER_WORKER` | API/Job admission service and persisted dispatch marker | Server Worker | Persisted Job Store/Queue abstraction; exact queue technology unresolved | `jobs.Jobs`, attempts, state history, leases, checkpoints | Job ID stable; attempt/lease/state version; idempotency persisted | Purpose-scoped workload identity; no user token forwarding |
| `MODULE_TO_MODULE_CONTRACT` | Owning module application service | Another module application service | In-process typed interface/DTO | Callee module aggregate/store | Contract/schema version when persisted/externalized | Caller identity/context passed as verified facts, not client claims |
| `INTERNAL_EVENT` | Owner module after durable state transition | History/Audit/notification/projection consumers | Post-commit outbox + bounded dispatcher | Owner aggregate and outbox record | Stable Event ID, event type/version, stream/revision; consumer dedupe | Safe metadata allowlist; consumer cannot write owner tables |
| `APPLICATION_TO_ENGINE` | Feature application service via Engine Resolver | Approved engine adapter | Typed engine contract | Application/Job state; engine output is untrusted until validated | Engine contract + provider/engine version + checkpoint compatibility | Job-scoped refs, resource limits, no arbitrary path/URL/provider selection |
| `APPLICATION_TO_STORAGE` | Workspace/Jobs/Secured/Support adapters | Local filesystem workspace or secure object storage | Storage abstraction + streaming | SQL/SQLite metadata plus storage object integrity manifest | Object/reference version, digest, ETag/rowversion | Resolve only after authorization; opaque ref is never authority |
| `APPLICATION_TO_AUDIT` | Owner intent collector/local audit outbox | Audit ingestion/append service | Transactional outbox or authenticated REST batch | `audit.AuditEvents` append-oriented store | AuditEventId/EventVersion; batch Idempotency-Key + fingerprint | Metadata only; controlled writer; query/export separately authorized |
| `APPLICATION_TO_OBSERVABILITY` | API, clients, Workers, adapters, providers, DB/storage integration | Approved OpenTelemetry/Application Insights path | Structured logs/metrics/traces | Observability backend for health, not business truth | Telemetry schema/config version; sampling allowed | Closed field allowlist, environment/access/retention controls |
| `APPLICATION_TO_SSO` | Web/native client and API auth boundary | Approved Pertamina Identity | Protocol/profile to be confirmed; HTTPS | Pertamina Identity assertion + OTO Principal/session mapping | Issuer/audience/token/session policy; no app password store | PKCE/state/nonce/callback where applicable; signature/type/time/subject validation |
| `APPLICATION_TO_INTERNAL_PROVIDER` | Content Intelligence/ML resolver or approved engine adapter | Approved enterprise-internal provider | Typed purpose-scoped provider contract | Application Job/operation state; provider response is untrusted | Provider contract/profile version + timeout/circuit/idempotency if supported | No public AI, minimized selected content, workload identity, no user token |

`APPLICATION_TO_STORAGE` mencakup Storage Provider yang diminta. Ia bukan vendor contract langsung. `APPLICATION_TO_INTERNAL_PROVIDER` hanya aktif setelah classification, policy, permission, consent, capability, minimization, egress, timeout, dan retention gate lolos.

---

# 3. REST map

## 3.1 Common REST contract

| Concern | Baseline |
|---|---|
| Functional base | `/api/v1` |
| Stable exceptions | `/api/compatibility`, `/health/live`, `/health/ready` |
| Canonical server execution | `POST /api/v1/features/{featureId}/execute` |
| Auth | Approved Pertamina bearer/session model; health/compatibility exposure follows Document 06 |
| Trace | `X-Correlation-Id`; server may normalize/generate opaque value |
| Client facts | `X-Client-Platform`, `X-Client-Version`; never ownership/permission authority |
| Mutation dedupe | `Idempotency-Key` when Document 06 marks `Idem=Yes` |
| Optimistic concurrency | `If-Match`/ETag where contract requires |
| Content integrity | `Content-Digest` for chunk/content where required |
| Rate/backpressure | Bounded endpoint/workload limits; `429 + Retry-After` for caller quota/rate; `503` for unsafe shared capacity/readiness |
| Monitoring | Route family, method, status/error code, duration, Feature/Job/Correlation IDs where needed, bounded bytes/counts; no content/secret |
| Retry | GET/HEAD only with bounded backoff; mutation repeats exact Idempotency-Key/fingerprint; `202` reconciles existing Job/Location |

## 3.2 Resource-family map

| Resource family | Exact Document 06 routes | Consumer | Owning application/data boundary | Request → response | Auth/permission | Version/SOT | Retry/idempotency/rate/monitoring |
|---|---|---|---|---|---|---|---|
| Compatibility and Feature Registry | `GET /api/compatibility`; `GET /api/v1/features`; `GET /api/v1/features/{featureId}` | All clients; Assistant through registry | Platform Configuration + canonical Feature Registry | Client platform/version or filters → compatibility/filtered capability descriptor | Compatibility safe pre-auth metadata; Features authenticated and exposure-filtered | Stable compatibility route; `/api/v1`; Contract/Capability versions; configuration store/registry truth | Bounded GET retry; version/status/latency monitored; no caller-provided permission trusted |
| Feature/workflow execution | `POST /api/v1/features/{featureId}/execute`; `POST /api/v1/workflows/execute` | Web/Desktop/Mobile for resolver-approved server path; Assistant same path | Feature owner + Processing/Jobs | Typed fileRefs, contract/schema, parameters → `200` result or durable `202` Job | `document.content.process`; `document.server.execute`; specialized Feature action permission; per-step workflow auth; ownership/policy/consent | API v1 + Contract/Parameter/Workflow schema; Feature Registry + persisted Job truth | Same key/fingerprint replays same result/Job; per-feature rate/admission; monitor Feature/status/duration/queue safely |
| File transfer | `POST /api/v1/file-transfers/preflight`; `POST /api/v1/uploads`; `POST /api/v1/upload-sessions`; `GET /api/v1/upload-sessions/{uploadId}`; `PUT /api/v1/upload-sessions/{uploadId}/content`; `POST /api/v1/upload-sessions/{uploadId}/complete`; `DELETE /api/v1/upload-sessions/{uploadId}` | Client only after resolver selects optional/required server and policy/consent allows | Jobs/Upload application boundary + secure temporary storage | Metadata preflight or streamed bytes/chunks → policy decision/session/offset/READY validation | Applicable underlying action permission (`document.server.execute`, secured or Issue action), owner, purpose, consent, policy/capacity | API v1; SQL upload state + temp object truth | One-shot/create/complete same Idempotency-Key; chunk natural `(uploadId,range,digest)`; abort idempotent; byte/count/storage limits; monitor bytes/fail/digest without content |
| Jobs/results | `GET /api/v1/jobs`; `GET /api/v1/jobs/{jobId}`; `POST .../cancel`; `POST .../retry`; `POST .../resume`; `GET .../results`; `HEAD .../results/{resultId}/content`; `GET .../results/{resultId}/content` | Owner clients; explicitly scoped Support projection only | Processing/Jobs; `jobs` schema + result storage | Cursor/detail/action → persisted state/manifest/stream | `jobs.job.list/read/cancel/retry/resume`; `jobs.result.read/download`; owner/state/policy/checkpoint/retention | API v1; persisted Job state, not SignalR; ETag may guard status read | Action same key; logical Job ID unchanged; Range/If-Range for download; limits per caller/job/bytes; monitor state duration/retry/result expiry |
| Account | `GET /api/v1/account/profile`; `GET /api/v1/account/preferences`; `PUT /api/v1/account/preferences` | Current user clients | Identity + Workspace preference projection | Safe current profile/preferences → typed versioned preference | `account.profile.read`, `account.preference.read/update`; OWN | API v1; identity/config/workspace metadata truth; `If-Match` on update | GET bounded retry; update same key + ETag; rate low; monitor outcome/version only |
| Personal History | `GET /api/v1/history`; `GET /api/v1/history/{historyId}`; `POST .../reprocess`; `DELETE .../{historyId}` | Current owner clients | History module/schema; local history remains separate | Cursor/detail/action → projection/current execution | `history.entry.list/read/reprocess/delete`; OWN; reprocess rechecks current Feature permissions | API v1; history projection truth, never Audit truth | Reprocess/delete same key; query cursor bounded; monitor outcome/Feature without content |
| Secured Files | `POST /api/v1/secured-files`; `GET /api/v1/secured-files`; `GET /api/v1/secured-files/{securedFileId}`; `POST .../versions`; `GET .../versions`; `GET .../versions/{versionId}/content`; `POST .../versions/{versionId}/restore`; `DELETE .../{securedFileId}`; `POST .../{securedFileId}/restore`; `DELETE .../{securedFileId}/permanent` | Authenticated owner clients | Secured Files boundary; `secured` schema + secure object storage | Validated fileRef/actions → metadata/version/content/recycle state | `secured.file.create/list/read/delete/restore/purge`; `secured.version.create/list/read/download/restore`; OWN, policy, quota, confirmation/reauth | API v1; secured SQL aggregate + exact object version truth; maximum three accessible versions | Mutation same key; downloads Range/ETag; quota/version/retention limits; monitor lifecycle/outcome/digest reference safely |
| Reporter Issue | `POST /api/v1/issues`; `GET /api/v1/issues`; `GET /api/v1/issues/{issueId}`; `POST .../attachments`; `GET .../attachments/{attachmentId}/content` | Reporter | Support/Issue boundary; `support` schema + attachment storage | Minimal message/safe context or explicit streamed attachment → Issue/status/content | `issue.report.create/list/read`; `issue.attachment.create/read`; REPORTER and parent-child validation | API v1; Issue aggregate/status history truth | Submit/attach same key; attachment retry preserves message; list/bytes bounded; monitor state/outbox/error safely |
| Support Issue | `GET /api/v1/support/issues`; `GET /api/v1/support/issues/{issueId}`; `PATCH .../{issueId}`; `POST .../{issueId}/notes`; `POST .../{issueId}/exports/pdf`; `POST /api/v1/support/issues/exports/excel` | Permission-scoped Support profile | Support module/schema; export via Job/result boundary | Scoped cursor/detail/typed transition/note/export request → resource or `202` Job | `support.issue.list/read/triage`; `support.note.create` and read where exposed; `support.export.create`; attachment permission for exact content | API v1; Issue aggregate + export Job truth; PATCH `If-Match` | Mutations same key/ETag; bounded query/export; monitor access/action/Job without Issue content |
| Audit | `POST /api/v1/audit-events/batches`; `GET /api/v1/audit-events`; `POST /api/v1/audit-events/exports` | Authenticated app ingestion; Auditor query/export | Audit ingestion/query/export boundary; `audit` schema | Versioned safe metadata batch/query/filter → acknowledgement/cursor/`202` export Job | Workload `audit.event.ingest`; human `audit.event.list/read`, `audit.export.create`; `audit.export.read` applies to result retrieval | API v1; `audit.AuditEvents` truth; EventVersion independent; export Job truth | Batch/export same key; AuditEventId dedupe; bounded cursor/batch/export; monitor backlog/reject/duplicate/latency without event body |
| Effective Configuration | `GET /api/v1/configuration/effective` | Authenticated clients | Platform Configuration | Client/profile facts → filtered typed effective config/capability policy | Authenticated and filtered; no permission/owner fact from client | API v1 + ConfigurationSchemaVersion/CapabilityVersion; configuration store truth | Bounded GET retry/cache revalidation; monitor version/outcome, not values that are sensitive |
| Admin Policy/Feature/Limit | `GET /api/v1/admin/policies`; `PUT .../policies/{policyKey}`; `GET /api/v1/admin/feature-flags`; `PUT .../feature-flags/{featureId}`; `GET /api/v1/admin/limits`; `PUT .../limits/{limitKey}` | Authorized Administrator | Admin + Platform Configuration | Typed list/current version or update → updated safe metadata | `configuration.policy.read/update`; `configuration.feature.read/update`; `configuration.limit.read/update`; ADMIN_SCOPE, `If-Match`, high-impact reauth/policy | API v1 + config schema/version; SQL rowversion/config truth | PUT same key + ETag; no blind retry; admin rate bounds; monitor actor/action/target/outcome and Audit safe metadata |
| Health | `GET /health/live`; `GET /health/ready` | Infrastructure/approved operations | Host + dependency readiness aggregator | No business payload → minimal alive or ready/not-ready | Exposure per Document 06; detailed diagnostics restricted to operations boundary | Stable non-v1 route; current host/dependency probe truth only | Bounded probe cadence; no dependency detail/topology/queue depth; monitor availability externally |

## 3.3 REST gaps that must not become invented routes

- Document 09 defines admin user/session/role/permission and compatibility/maintenance permissions, but Document 06 inventory does **not** define corresponding REST routes. No route is added by this report.
- Document 09 defines `monitoring.system.read` and `monitoring.job.read`; Document 06 exposes only minimal health endpoints, not a monitoring aggregate API.
- `audit.export.read` and `support.export.read` permissions exist, while Document 06 creates export jobs but does not define a dedicated export-read route. Recommended interpretation is normal authorized Job result retrieval only if contract/parent-scope binding is explicitly confirmed.
- `ActivityUpdated` has no separate `/activity` REST resource. Reconciliation uses authoritative Jobs and relevant Issue/domain resources; the exact Activity aggregation/read model requires confirmation.
- Document 10 requests a compatible Audit batch-contract addendum for per-event accept/duplicate/reject/conflict semantics. It does not authorize a new route or API major.

---

# 4. SignalR map

## 4.1 Hub contract

```text
Persist owner state/revision
→ write owner outbox
→ bounded publisher
→ authenticated /hubs/activity
→ client applies safe event if next revision
→ REST reconcile on gap/unknown/reconnect
```

| Event | Producer | Subscriber | Authorization | Ordering/version | Duplicate/gap handling | REST reconciliation |
|---|---|---|---|---|---|---|
| `JobQueued` | Jobs owner/outbox after durable admission | Owner Activity Center; optional authorized job subscription | `realtime.activity.connect`; optional `realtime.job.subscribe`; server-derived user group and ownership recheck | EventVersion; `streamId=job:{jobId}`; sequence from persisted `stateVersion` | Ignore duplicate eventId or stale sequence; gap triggers REST | `GET /api/v1/jobs/{jobId}` and list |
| `JobStarted` | Jobs owner/outbox after worker claim/start commit | Same | Same | Same job stream; no global ordering | Same | Same |
| `JobProgress` | Jobs owner after coalesced persisted progress revision | Same | Same | Sequence from persisted state revision; progress may be indeterminate | Same; delayed progress cannot regress terminal state | Same |
| `JobWaiting` | Jobs owner after persisted `WAITING_CONNECTION`/`WAITING_RESOURCE` | Same | Same | Same; only two locked waiting statuses | Same | Same |
| `JobCompleted` | Jobs owner after output validation + terminal commit | Same | Same; result content still separately authorized | Terminal revision | Duplicate/stale ignored; terminal never regresses | Job detail + result manifest/content endpoints |
| `JobFailed` | Jobs owner after safe terminal failure commit | Same | Same | Terminal revision; safe Error/message code only | Same | Job detail |
| `JobCancelled` | Jobs owner after cooperative terminal commit | Same | Same | Terminal revision | Same | Job detail |
| `JobRetrying` | Jobs owner after persisted attempt/retry state | Same | Same | Logical Job ID remains; revision and attempt metadata safe | Same | Job detail |
| `ActivityUpdated` | Owning domain/outbox for persisted Activity projection/revision | Authenticated user Activity Center | Server-derived user group; no arbitrary principal/correlation group | Persisted Activity revision; exact aggregation contract unresolved | Duplicate/stale ignored; gap requires domain REST reconstruction | Jobs plus relevant Issue/domain list; no standalone Activity route exists |
| `IssueStatusChanged` | Support outbox after Issue transition commit | Reporter; scoped Support surface as authorized | Reporter ownership or Support permission; group membership server-derived | Persisted Issue revision; EventVersion | Duplicate/stale ignored; gap triggers REST | `GET /api/v1/issues/{issueId}` or scoped Support detail |

Common exclusions: document/preview/OCR content, password/token/secret, signature/certificate material, prompt/chat/provider payload, raw exception/engine message, physical path/storage key, attachment bytes, dan arbitrary display text. `messageCode`/`currentStep` adalah safe code yang dilokalkan client.

Reconnect memakai bounded exponential backoff + jitter, lalu REST reconciliation, apply persisted revision, dan resubscribe. Reconnect tidak mengeksekusi ulang command. SignalR failure tidak membatalkan Job dan tidak mengubah persisted result.

---

# 5. Local IPC map

## 5.1 Boundary

```text
Desktop UI
→ Processing Coordinator / local_job owner
→ ILocalWorkerTransport
→ authenticated versioned Named Pipe/local IPC
→ Local Worker instance bound to profile + job
→ approved Engine Adapter
→ staged local artifact
→ Desktop validates/finalizes and updates local_job
```

Tidak ada public network port, remote pipe access, arbitrary socket, HTTP localhost control API, atau direct SQLite/SQL access dari Worker.

## 5.2 Required contract characteristics

| Concern | Recommendation |
|---|---|
| Handshake | Authenticated, instance-bound, and checked against `AppVersion`, `LocalWorkerVersion`, `IpcContractVersion`, supported handlers, engine versions, and work-root/checkpoint schema; peer identity/ACL validated; pipe name/PID/randomness alone insufficient |
| Command data | Canonical Feature ID, contract/parameter schema, typed params, job-scoped input/output artifact references, Correlation ID, deadline/resource budget |
| Operation whitelist | Closed message/operation registry. Required families are handshake, execute, progress/status, heartbeat, cooperative cancel, terminal result/error; exact codes/schema remain to be registered |
| Frame | Contract/version/type/length/request ID/Correlation ID/deadline; hard size/depth/count/rate bounds |
| Progress | Safe normalized status/progress/current-step codes; Worker claim is not authoritative until Desktop validates and persists |
| Heartbeat | Bounded liveness/instance identity only; missed threshold marks transport degraded and triggers process reconciliation, not automatic success/failure |
| Cancellation | Cooperative cancel to safe point; hard timeout may recycle Worker; partial output never finalized |
| Timeout | Separate transport deadline and engine/resource budget; timeout produces typed safe failure/recovery intent |
| Access | Local machine only; current intended user/app/package/process ACL where supported; no Administrator default |
| Files | Unique protected per-profile/job working root, random names, manifest, quota, path traversal/reparse defenses |
| Network | Denied by default; engine/provider egress only if separately approved and destination-bound |
| Crash | Desktop/UI remains alive; detect stale Worker/handle; reject stale terminal message; validate artifacts/checkpoint; bounded resume/retry |
| Source of truth | `local_job` + append attempt/checkpoint metadata in profile SQLite and protected artifact manifest, owned by Desktop application boundary |

Document 17 FINAL mengunci Desktop dan Local Worker sebagai satu signed compatibility cohort secara default. `ILocalWorkerTransport` adalah abstraction boundary dari prompt; exact method set, serialization, authentication mechanism, pipe naming, heartbeat/timeouts, compatibility ranges, dan containment tetap memerlukan PoC serta contract registration sebelum implementation.

---

# 6. Server Worker flow

```text
API validates execute request
→ persist idempotency + logical Job + first attempt + inputs/leases
  + admission reservation + dispatch marker/outbox
→ return true 202 + same Job ID/Location
→ Server Worker claims bounded eligible Job with workload identity + lease
→ reload persisted Job/handler/schema/fileRefs
→ resolve approved engine/provider + exact storage scope
→ execute under resource/time/cancellation limits
→ stage and validate output
→ commit artifact reference + terminal Job state + attempt/release/outbox/Audit intent
→ SignalR projection
→ REST Job/result retrieval
```

| Concern | Contract |
|---|---|
| Async boundary | `202` only after logical Job, attempt, input lease, idempotency, capacity reservation, and dispatch marker are durable |
| Identity | Stable logical Job ID + Correlation ID; retry/resume adds append-only attempt, not duplicate Job |
| Queue | Conceptual Job Store/Queue abstraction. External message broker is not baseline; exact queue technology and delivery contract unresolved |
| Claim | Worker uses `jobs.worker.claim`; state/version/lease and eligible workload class checked atomically |
| Execution | Closed handler registry; Feature/schema/capability revalidated; user token not forwarded |
| Retry | Automatic only for transient allowlist; bounded attempt, exponential backoff+jitter; same logical Job/idempotency context |
| Checkpoint | Resume only when input identity, Feature/schema, engine/provider version, stage, integrity, and retention are compatible |
| Result | Secure temporary result storage; validated artifact metadata in `jobs`; `COMPLETED` only after finality commit |
| Partial | `PARTIAL_SUCCESS` only for typed independent-batch semantics; Merge/atomic tools fail as a whole |
| Lifetime | Result/checkpoint/upload/temp expire by configurable policy; exact durations unresolved; active/leased/referenced content excluded from cleanup |
| Cleanup | State-aware, bounded, idempotent reconciliation; remove orphan staging after metadata checks; never delete source/secured payload by generic temp cleanup |
| Monitoring | Queue/oldest age, accepted/claimed/running/waiting/retry/terminal, lease recovery, duration, CPU/RAM/temp/storage/DB/provider, duplicate/stale completion |
| Failure | Worker crash leaves durable state; lease expiry/reclaimer and checkpoint policy decide resume/retry; no client-visible false completion |

---

# 7. Module-contract map

## 7.1 Contract rules

- `Domain`, `Application`, `Contracts`, `Infrastructure`, dan `Tests` tetap terpisah per module.
- Host/UI bergantung pada Application/Contracts; module tidak bergantung pada Host/UI.
- Provider/DB/storage implementations bergantung pada module-owned abstractions; domain/application tidak bergantung pada vendor SDK.
- Contract owner adalah callee/module yang memiliki invariant dan data write.
- Cross-module DTO berisi minimum typed facts/references; tidak membawa EF entity, physical path, storage key, raw token, atau arbitrary dictionary.

## 7.2 Contract register

| Producer/caller | Consumer/owner | Contract basis | Sync/async | Data exchanged | Version | Failure behavior |
|---|---|---|---|---|---|---|
| Web/Desktop/Mobile/Assistant | Canonical Feature Registry/Application dispatcher | Feature descriptor + typed execution request | Sync admission; async Job possible | Feature ID, schema version, fileRefs, typed params, verified context | Contract/Parameter/Workflow/Capability versions | Unknown/hidden/unsupported handler fails closed; no arbitrary dispatch |
| PDF module | ScannerOcr module | `IOcrCapability` example from Document 04 | Sync or background based coordinator | Job-scoped input ref, OCR options, result ref/metadata | Interface/schema version required before persisted use | OCR failure typed; PDF source remains unchanged; fallback only approved |
| PDF/Conversion/Image/ScannerOcr/SignatureAnnotation/FileProductivity | Processing Coordinator/Jobs boundary | Processing abstraction | Sync admission; execution sync/background | Workload profile, Feature, typed request, resource estimate, Job/Correlation refs | Feature/schema/capability | Admission wait/reject explicit; no unbounded execution |
| Feature application service | Engine Resolver | Engine capability request | Sync selection | Feature/capability/platform/policy/file/resource facts | Resolver/config/capability version | No approved provider = unavailable/typed failure, not public fallback |
| Engine Resolver | Engine adapter | PDF/OCR/Conversion/Image/Content Intelligence contract | Sync/background execution | Job-scoped refs/parameters/budget | Engine contract + provider version | Timeout/cancel/invalid output normalized; output validated |
| Processing/Feature owner | Workspace application boundary | `ILocalWorkspaceStore`/workspace contract | Sync transactional intent + file I/O outside transaction | Opaque refs, artifact manifest, state/version | Workspace/schema/artifact version | Atomic staging/finality; source never overwritten silently |
| Jobs/Secured/Support | Storage abstraction owner | `IFileStorage`/`ISecuredAccountStorage` or scoped equivalent | Stream + short metadata transaction | Opaque internal object ref, digest, media/size/lifecycle | Storage manifest/object version | Staging/reconcile on cross-store partial failure |
| Job owner | History owner | Post-commit projection contract/event | Async | Safe source event, owner, Feature, outcome/time, resource refs | Event version required | Retry/dedupe; Job completion does not roll back because History projection is delayed |
| Any owner module | Audit owner | Owner intent/outbox | Async after required intent durability | Registered safe Audit metadata | Audit EventType/EventVersion | L2/L3 degraded behavior per Document 10; no direct audit table write |
| Jobs/Support/Secured owner | Realtime publisher | Safe post-commit event | Async | Safe event envelope + stream revision | SignalR EventVersion | Retry at-least-once; client dedupe/REST recover |
| Support | Jobs/Export execution | Application contract, not jobs-table write | Async | Authorized bounded export spec + Issue scope ref | Support/export schema + Job contract | Same idempotency; failure remains linked to Issue/export request |
| Audit | Jobs/Export execution | Application contract, not jobs-table write | Async | Authorized bounded filter snapshot + Audit scope ref | Audit filter/export schema + Job contract | Same idempotency; avoid recursive Audit event loop |
| Assistant | Underlying Feature application service | `FeatureExecutionRequest` or `WorkflowPlan` | Same as normal caller | Canonical Feature IDs, typed params, origin metadata | Assistant/workflow + Feature schema versions | Same auth/confirm/error/Activity path; Assistant cannot invent success |

Exact project/module ownership untuk Secured Files, History, Jobs, Platform Configuration, dan Content Intelligence belum seluruhnya terlihat pada conceptual repository list Document 04, walaupun logical owners/schema telah didefinisikan Document 07. Ini harus diselesaikan pada Document 18 master/module ownership register.

---

# 8. Internal-event map

## 8.1 Event envelope recommendation

Internal event yang durable harus mempunyai stable Event ID, registered type, EventVersion, owner stream/resource ID, sequence/revision bila ordered, occurred time, Correlation ID, safe Actor/origin reference bila perlu, dan bounded typed payload. Internal event tidak menggunakan raw EF entity atau arbitrary JSON.

## 8.2 Known and unresolved event roles

| Producer | Consumer | Known contract/source | Purpose/data | Ordering/dedupe | Failure behavior/status |
|---|---|---|---|---|---|
| Document/processing owner | History, Audit, Notification | `DocumentProcessed` adalah contoh nama dalam Document 04 | Feature/outcome/time/owner/resource refs tanpa content | Stable event ID + source revision; each consumer dedupe | Post-commit retry; one consumer failure tidak membatalkan owner commit |
| Jobs owner | SignalR publisher/Activity projection | `jobs.OutboxMessages`; exact internal event names not locked | Safe Job state/revision/progress/terminal metadata | Unique Event ID and stream+sequence | At-least-once publish; REST remains authority |
| Jobs owner | History projection | Logical source event from Job | Safe personal activity projection | Unique owner+source event | Retry; history deletion independent |
| Jobs/Feature owner | Audit ingestion | Owner transaction/outbox | Accepted/terminal/high-risk action metadata | AuditEventId/EventVersion | Protected retry/dedupe; no progress-per-page event |
| Secured Files owner | Lifecycle/cleanup, History/Audit as registered | `secured.OutboxMessages`; exact types unresolved | Create/version/recycle/restore/purge intent/result refs | Event/stream sequence | Cleanup/reconcile bounded; no generic cascade |
| Support owner | SignalR publisher | `support.OutboxMessages` | Issue status revision and safe notification | Event/Issue revision | Retry; Issue REST truth |
| Support owner | Audit ingestion | Owner outbox | Submitted/viewed/triaged/status/export metadata only | AuditEventId | Retry/dedupe; no Issue message/attachment bytes |
| Configuration/Admin owner | Audit ingestion + capability cache invalidation | Owner outbox expected; exact contract unresolved | Policy/flag/limit/version/action/outcome metadata | Resource rowversion + Event ID | Stale cache invalidates; mutation remains authoritative in config store |
| Audit export owner | Jobs/export Worker and Audit access evidence | Application contract + audit owner outbox | Safe filter snapshot/export request ID/Job ID | Idempotency + request version | Must prevent recursive event storm and preserve access intent |
| Local application | Audit ingestion | Protected `audit_outbox` via batch API | Registered safe offline event | Source sequence + AuditEventId + batch fingerprint | Bounded retry; ack then local cleanup |

**Finding:** selain contoh `DocumentProcessed` dan public SignalR/Audit catalogs, sebagian besar internal event belum mempunyai canonical name + version + owner/consumer registry pada dokumen FINAL. Implementasi tidak boleh menebak nama/schema; Document 18 harus membuat controlled internal-contract register tanpa mengubah business/API semantics.

---

# 9. Engine/provider map

`Primary provider` pada tabel berarti primary **slot/profile**, bukan vendor yang telah dipilih. Tidak ada vendor final yang dipilih laporan ini.

| Capability | Owning application area | Contract/resolver | Primary/fallback concept | Local/server | PoC/license/security status | Health/capability/timeout | Resource class |
|---|---|---|---|---|---|---|---|
| PDF Engine | PDF + Signature/Annotation where applicable | `IPdfMergeEngine` example; broader PDF contracts need registry; Engine Resolver | Approved primary engine; specialized fallback only if PoC proves gap | In-process light or Local Worker heavy; server only resolver-approved optional/required | Vendor unresolved; fidelity, malformed corpus, license, SBOM, isolation PoC required | Capability matrix by operation/version/platform; bounded operation deadline; output validation | Light page op through memory/temp-intensive large PDF |
| OCR Engine | ScannerOcr | `IOcrEngine`; Engine Resolver | Approved local primary where capable; approved server/internal fallback only by policy | Local Worker default for heavy; server optional/required when approved | Vendor/model/language/license unresolved; accuracy/security/resource/device PoC | Language/page/type capability, engine health, cancel/checkpoint if supported | CPU/RAM/temp intensive, long-running |
| Conversion Engine | Conversion | `IDocumentConversionEngine`; Engine Resolver | Approved primary; fallback per format only after fidelity/security/license PoC | Local or Local Worker first; server only resolver-approved | Office/PDF format fidelity, macro/active-content, platform, license unresolved | Format/capability profile; hard timeout; process recycle/child-process controls | Heavy, native/process risk, memory/temp intensive |
| Image Engine | Image | `IImageProcessingEngine`; Engine Resolver | Approved local provider; specialized ML fallback only if required | Mostly local/in-process or Local Worker; server capability conditional | Codec/malformed image/metadata/resource/license PoC | Codec/operation/size capability; decompression/pixel bounds; timeout | Light edit to decompression/memory intensive |
| ML Provider | Image/feature-specific ML capability; exact owner unresolved | Exact interface not locked; must sit behind approved resolver/adapter | Local approved model preferred; internal provider only if explicitly allowed; no public fallback | Local or enterprise-internal | Model/source/license/privacy/accuracy/bias/security/resource status unresolved | Model/version/capability/health/circuit/time limit; output untrusted | Accelerator/CPU/RAM intensive or internal-network dependent |
| Content Intelligence Provider | PDF Intelligence application tools | `IContentIntelligenceProvider` + Content Intelligence Resolver | Approved local provider; enterprise-internal fallback after all gates; never public AI | Local or approved internal | Provider/data residency/retention/training/subprocessor/license/PoC unresolved | Feature/language/context limit, health/circuit/timeout, policy profile | Network/internal-provider or local model intensive |
| Storage Provider | Workspace/Jobs/Secured/Support storage owners | `IFileStorage`, `ISecuredAccountStorage`, `ILocalWorkspaceStore`; provider adapter | Approved local filesystem boundary or private secure object storage; no arbitrary user URL/provider | Local and server by store purpose | Vendor/encryption/key/storage topology unresolved; integrity/range/cleanup/restore PoC | Read/write/range/digest/capacity/readiness; bounded stream timeout | I/O/network/storage intensive |

Resolver inputs: canonical Feature, platform, policy/classification, file/page/count/size, memory/storage/network, offline/online, server/provider availability, security, license, capability, resource budget, and approved provider profile. User/Assistant/API request tidak boleh memilih vendor, engine class, executable, path, URL, atau provider endpoint.

---

# 10. Storage integration

## 10.1 Storage map

| Store | Data | Owning interface | Metadata authority | Payload authority | Access/retry/cleanup |
|---|---|---|---|---|---|
| Local Workspace | Source locator, draft, working state, undo/redo, cache, thumbnail, checkpoint, temp/intermediate, local result | `ILocalWorkspaceStore`/workspace application service | Per-profile SQLite + protected manifest | Local filesystem app-managed artifact or user-selected source | Current profile/selection; atomic stage/finalize; source unchanged; state-aware local cleanup |
| Local SQLite | Local workspace/job/history/preferences/pending intents/audit outbox/secured cache metadata | Local persistence abstraction; serialized writer | SQLite itself per profile/device | No document/signature bytes by default | Worker has no arbitrary access; WAL/schema migration/recovery protected |
| SQL Server | Identity/config/jobs/secured/history/audit/support/assistant/intelligence metadata | Module-owned EF Core/application repositories | One schema/write owner per aggregate | No file payload default | Short transaction/rowversion/outbox; no client/direct cross-module SQL |
| Secure Processing Temp/Result | Server upload, staging, intermediate, checkpoint/result bytes | Jobs/storage adapter | `jobs` metadata/state | Private secure object storage | Job-scoped workload permission; digest; expiry; active lease/ref protection; reconciliation |
| Secured Account Storage | Explicit selected file/result, max three accessible versions | Secured application service/`ISecuredAccountStorage` | `secured` aggregate/version/recycle state | Immutable object versions | Owner permission; exact version; no auto-sync; retention/legal-hold/version cleanup |
| Issue Attachment/Export Storage | Explicit screenshot/attachment and bounded Support export | Support storage adapter | `support` metadata/parent relation/export request | Case-bound private object | Reporter/Support exact parent authorization; explicit send; expiry/retention separate |
| Audit Export Storage | Bounded authorized export artifact | Audit export service/storage adapter | `audit.ExportRequests` + Job result link | Restricted private export artifact | Audit scope/reauth/expiry/current permission checked on retrieval |

## 10.2 Cross-store transaction pattern

```text
Authorize + validate intent
→ create staging reference/intent
→ stream/write object outside long SQL transaction
→ validate digest/structure
→ short owner transaction commits metadata/current state/outbox
→ finalize/promote object
→ reconcile ambiguous partial outcome idempotently
```

SQL + object storage tidak berada dalam satu distributed long transaction. Physical path, object key, SAS URL, provider credential, dan client-controlled URL tidak menjadi business contract. Every download authorizes before resolving storage and again binds child object to parent Job/Secured File/Issue/Export.

---

# 11. SSO integration

```text
Client compatibility check
→ approved Pertamina SSO initiation
→ Pertamina authenticates
→ state/nonce/PKCE/callback validation as applicable
→ API validates signature/reference, algorithm, issuer, audience,
  token type, lifetime/not-before, subject, approved client/claims
→ issuer + subject maps to internal PrincipalId
→ account/session eligibility and revocation
→ current permissions/policy
→ resource/action authorization
```

| Integration concern | Contract |
|---|---|
| Identity source | Approved Pertamina Identity; OTO does not store parallel username/password |
| Stable mapping | Validated issuer + subject → internal PrincipalId; email/display name/employee number from client is not stable authority |
| Web session | Protected opaque Secure/HttpOnly narrowly scoped session model; no browser bearer exposure where baseline applies |
| Native token | Approved header transport; protected memory/OS credential boundary if persistence required; never URL/SQLite/log/Issue |
| SignalR | Web same-site protected session cookie; native approved authorization header; otherwise REST fallback, never token in URL |
| Workload | Separate purpose/audience-scoped workload identity; no end-user token forwarded to Worker/storage/provider |
| Offline | Protected narrow offline grant only for eligible local actions; never API/Admin/Support/Audit/server permission |
| Refresh | Approved identity only; single-flight, bounded transient retry; revocation/replay/interaction-required stops silent refresh |
| Logout | Idempotent stop Hub/dispatch, revoke reachable session, clear credential/grant/memory state, lock profile; do not delete source/draft silently |
| Failure | `401` for missing/invalid/expired auth; `403` for valid identity but OTO access denied; sensitive ownership may use consistent `404` concealment |

`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`: exact protocol/profile, issuer, audience, redirect URI, token type, claims, stable subject, provisioning, lifetimes, logout, revocation, MFA/reauth integration, workload identity, dan client secure-store integration.

---

# 12. Observability/audit flow

## 12.1 Plane separation

```text
Business owner state ──safe intent/outbox──► Enterprise Audit
Runtime component ──safe log/metric/trace──► Observability
User action outcome ──safe projection──────► Personal History
Explicit report ──message/attachment───────► Issue/Support
```

| Plane | Purpose | Permitted data | Prohibited copy/authority |
|---|---|---|---|
| Observability | Health, error, latency, resource, trace, alert | Environment/app/component/version, Feature/Job/Correlation refs, safe codes/outcome/duration/bounded counters/provider profile | No document/OCR/signature/password/token/prompt/Issue bytes; not business/audit truth |
| Enterprise Audit | Controlled evidence: Actor did action to resource with outcome | Registered typed metadata, times, Actor/origin/executor, Feature/Process/Job/Correlation, safe resource refs | No document/Issue/AI payload; not editable/deletable by ordinary user |
| Personal History | User convenience/reprocess projection | Owned safe Feature/outcome/time/result metadata | Delete History never deletes Audit/Job/result/secured/source |
| Issue/Support | Explicit problem report and triage | User message, safe context, explicit attachment, restricted notes | Not generic telemetry/Audit store; no auto screenshot/document/log dump |

## 12.2 Audit path

Online owner mutation writes owner intent/outbox in its transaction and a bounded dispatcher calls Audit ingestion. Offline approved enterprise event writes protected metadata-only `audit_outbox`, then `POST /api/v1/audit-events/batches` with stable AuditEventId, EventVersion, source sequence, Idempotency-Key/fingerprint. Server authenticates source, validates/enriches/deduplicates, appends to `audit.AuditEvents`, acknowledges, then local queue cleans only acknowledged rows.

## 12.3 Observability path

Components emit allowlisted structured fields through approved OpenTelemetry/Application Insights integration. Sampling/agaggregation may apply. Provider/queue/storage/DB/Worker health must be monitored without copying request body, Audit row, raw exception/SQL, path/object key, or restricted content. Alert links use safe opaque evidence references with controlled access.

---

# 13. Data-flow register proposal

## 13.1 Flow diagrams

### Local flows

```text
User → Desktop Application/Feature Service
           ├─ light → Local Engine → staged local result
           └─ heavy → ILocalWorkerTransport → Local Worker → Engine
                                      ↓
Local Workspace artifacts ↔ per-profile SQLite state/checkpoint/history/audit_outbox

No API / SignalR / SQL Server / server storage for local-only processing
```

### Server and selective-storage flows

```text
Client → Resolver/preflight → Upload (only if server route allowed)
→ POST canonical execute → persisted Job/Queue → Server Worker
→ Engine/Internal Provider → Result Store → Job terminal state
→ SignalR hint → REST reconcile/download
                          └─ explicit Amankan ke Akun → Secured Files/Storage
```

### Control, evidence, and support flows

```text
Pertamina Identity → API authentication/authorization
Admin → typed configuration mutation → config store → Audit intent
Runtime → Observability
Owner modules → Audit outbox → Audit Store → bounded Audit export Job
User → explicit Issue → Support Store/Attachment → status event
Support/Operations → safe refs across Issue/Observability/Audit → Incident diagnosis
```

## 13.2 Flow register

All permissions are attempts only; ownership, state, policy, capability, consent, connectivity, capacity, and reauthentication remain mandatory context.

| ID / Flow | Actor + trigger | Source → destination / protocol | Data + sensitivity | Persistence + source of truth | Permission + Audit | Error, retry, cleanup |
|---|---|---|---|---|---|---|
| `DF-01` Local PDF Processing | User selects local PDF tool and confirms required action | Local source/workspace → Desktop Feature service → local PDF engine → staged/final local result; in-process contract | Typed parameters, selected local bytes, output; **Restricted content** | Working/source/result local filesystem; state/history/checkpoint SQLite; local workspace/job is truth | `document.content.process` plus sensitive action permission if applicable; safe process metadata via protected audit outbox when required | Typed validation/engine/storage error; retry only safe operation; cleanup staging/temp, never source/result/history by cache cleanup |
| `DF-02` Local Worker Processing | User starts heavy/batch/risk operation | Desktop → `ILocalWorkerTransport` → Named Pipe → Local Worker → Engine → Desktop validation | Job-scoped refs/parameters/progress; Restricted content stays local | `local_job`, attempts/checkpoint + artifact manifest are truth; IPC status display only | Same Feature permission/offline grant; safe Actor/Feature/outcome Audit metadata | Heartbeat/timeout/crash detection; cooperative cancel; compatible checkpoint resume or new attempt; recycle Worker and orphan staging safely |
| `DF-03` Optional Server Processing | Resolver selects server option and policy/user consent permit | Client → preflight/upload → canonical execute API | File bytes/metadata and typed request; Restricted content crosses server boundary explicitly | Upload/Job SQL + temp/result object store; persisted Job truth | `document.content.process` + `document.server.execute` + Feature-specific permission; accepted/terminal Audit | Deny/wait/local fallback only if same capability approved; exact idempotency key; upload/session expiry cleanup; no automatic server choice by failure |
| `DF-04` Server Background Job | API durably accepts long work | API → Job store/queue → Server Worker → Engine/provider → result store → Job state → SignalR/REST | fileRefs, typed params, workload metadata, result bytes; Confidential/Restricted | `jobs` state/attempt/checkpoint/outbox + object storage; persisted Job source | Feature/job/result permissions; Worker workload permissions; safe accepted/terminal Audit | `202` after durable commit; transient bounded retry/same Job; resume compatible checkpoint; lease recovery; expiry and staging cleanup |
| `DF-05` Amankan ke Akun | User explicitly selects validated file/result and confirms | Local/result fileRef → upload if needed → `POST /secured-files` or version route → secure object storage | Selected bytes + safe metadata; Restricted | `secured` aggregate/version/recycle + exact secure object; max three accessible versions | `secured.file.create` or `secured.version.create`; OWN/policy/quota/reauth; lifecycle Audit | Same Idempotency-Key; fingerprint conflict no overwrite; reconcile partial storage/metadata; version/retention cleanup only by secured owner |
| `DF-06` Cross-Device Retrieval | Same authorized user chooses secured version on another device | Device → secured list/detail/content API → local working copy | Secured metadata and selected bytes; Restricted | Server secured aggregate/object is source until downloaded; new local copy/workspace becomes local working truth | `secured.file/version.list/read/download`; every request reauthorized; download Audit as required | Range/ETag resume if valid; `410` expired/removed; local target conflict requires safe Save As/choice; temp download cleanup |
| `DF-07` Issue Report | User explicitly submits message; optionally previews/sends screenshot | Client → `POST /issues`; optional attachment stream → Support store; status via SignalR/REST | Minimal message/safe context; explicit screenshot is Restricted | `support.Issues`/status history + private attachment object; Issue aggregate truth | `issue.report.create`, `issue.attachment.create/read`; reporter ownership; Support events audited safely | Message persists if attachment fails; same-key retry; attach or send-without; protected local staging expires/cleans after ack/cancel |
| `DF-08` Audit Event | Registered business/security action occurs online/offline | Owner outbox or local `audit_outbox` → dispatcher/batch API → Audit ingestion/store | Safe typed metadata only; Controlled Audit evidence | `audit.AuditEvents` append-oriented truth; outbox truth until ack | System `audit.event.ingest`; no Auditor write permission | Stable Event ID/batch key dedupe; poison/version protected; ack then local cleanup; no loss on crash after append/before ack |
| `DF-09` User History | Completed/failed/cancelled user action produces personal projection | Owner source event → local/server History projection → History API/UI | Feature/outcome/time/safe result refs; Personal metadata | Local `local_history` or server `history.Entries` projection; not Audit truth | `history.entry.list/read/delete/reprocess`; OWN; reprocess reauthorizes Feature and audits new action | Projection retry/dedupe; missing history does not roll back job; delete projection only; retention separate |
| `DF-10` Technical Telemetry | Runtime emits health/performance/error signal | API/client/Worker/adapters → OTel/Application Insights approved path | Safe IDs/codes/duration/counters; Confidential metadata minimized | Observability backend, not business truth | Workload emission; human read controlled e.g. `monitoring.*`; security-relevant actions may separately create Audit event | Buffer/sample/drop policy must not corrupt business state; bounded retry; retention policy cleanup; never queue restricted payload |
| `DF-11` Assistant Invocation | User asks Assistant to use a capability | User intent → alias resolution → Feature Registry → schema/auth/policy/preflight/confirm → same application service | Minimized intent, canonical Feature IDs, typed params, refs; document content only if chosen Feature needs it | Assistant minimal local/server metadata; actual Job/result/History stores remain truth | `assistant.session.use` plus every underlying Feature/action permission; Origin=Assistant, Actor=user in Audit | Invalid/ambiguous/destructive intent requires clarification/confirmation; no arbitrary API; retry follows underlying operation idempotency; conversation cleanup does not delete Audit/result |
| `DF-12` AI Summarizer | User selects content/pages and invokes `FEAT-INTEL-*` | Application service → Content Intelligence Resolver → approved local or enterprise-internal provider → validated result | Minimized selected content; Restricted; prompt/provider response untrusted | Operation/Job metadata only; result local by default unless explicitly secured; no payload in ordinary SQL | `document.intelligence.execute` plus local/internal route policy/consent; safe provider-profile/outcome Audit | No public AI fallback; bounded timeout/circuit/retry only if safe/idempotent; provider temp/context cleanup per approved retention |
| `DF-13` Translate PDF | User selects PDF/language and approved route | Feature service → extraction/OCR as needed → Content Intelligence Resolver/provider → translated result | Selected text/layout and output; Restricted | Local workspace or server Job/result according to resolver; provider not truth | `document.intelligence.execute` + processing/server permission where applicable; safe Audit | Unsupported language/provider timeout/partial output typed; no silent public fallback; checkpoint only if compatible; temp/provider cleanup |
| `DF-14` PDF to Markdown | User invokes canonical PDF-to-Markdown feature | PDF/text-layout extraction → OCR fallback if necessary → deterministic Markdown validation | Document text/layout; Restricted | Local result/workspace or approved server Job; extraction/output artifact truth | `document.content.process`; server permission if server route; safe process Audit | AI is not default dependency; OCR fallback only approved; malformed/unsupported typed failure; cleanup intermediates, preserve final/source |
| `DF-15` Visual Sign PDF | User selects saved/drawn visual non-PKI signature and placement | Local sensitive signature asset → SignatureAnnotation service → PDF engine → new PDF result | Signature bytes/template + document; Restricted/Sensitive asset | Signature asset protected local store; output local workspace; no auto-sync | `document.signature.apply`; confirmation/policy; action metadata Audit only, never asset | Invalid placement/output fails safely; retry uses same local asset ref; clear transient rendering copies; source unchanged |
| `DF-16` Redact PDF | User marks redactions and finalizes irreversible output | Local PDF/workspace → redaction application/engine → forensic/output validation → new result | Document content/redaction regions; Restricted | Working annotations/checkpoint local; validated final result local unless explicit server/secured action | `document.redaction.finalize` + processing permission; confirmation/reauth policy; high-risk Audit metadata | Validation failure blocks final result; no visual-only false success; retry from safe working state; purge transient pre-final copies by policy, preserve source |
| `DF-17` Admin Configuration Change | Authorized admin changes an existing typed policy/flag/limit | Admin UI → existing PUT API + `If-Match` → config owner SQL/outbox → cache/capability refresh | Typed key/scope/old-new safe summary; Confidential control metadata | `configuration/admin` rowversion/versioned policy store | `configuration.policy/feature/limit.update`; ADMIN_SCOPE, high-impact reauth/SoD; mandatory safe Audit | Stale version `412`; same key retry; invalid/hard-ceiling reject; staged rollback via previous verified version; no arbitrary key/value |
| `DF-18` Audit Export | Auditor requests bounded export | Audit UI → `POST /audit-events/exports` → Audit filter snapshot + Job/Worker → restricted export object → authorized result retrieval | Scoped safe Audit evidence; Controlled/possibly Restricted metadata | Audit store is source; export Job/status and expiring artifact are derived | `audit.export.create/read`, AUDIT_SCOPE, high-impact policy/reauth; access/export Audit event | Same-key dedupe; bounded async retry; authorization rechecked before retrieval; expiry cleanup; no document/resource pivot |
| `DF-19` Incident Diagnosis | Support/operations/security investigates an incident using safe evidence | Incident record → Issue/Observability/Audit/Job/Correlation/config/build refs through their authorized interfaces | Safe metadata, aggregate, sanitized evidence; restricted content only through separately approved exact scope | Each plane remains its own truth; Incident record links references, not copied databases | `support.*`, `monitoring.*`, `audit.*`, operations authority separately; diagnostic access audited as required | Missing dependency/evidence remains UNKNOWN; no direct DB/storage browsing; bounded safe checks; Production change hands off to approved Document 17 procedure; evidence retention/cleanup policy |

---

# 14. Dependency graph

## 14.1 Directional graph

```text
Web / Desktop / Mobile
        │
        ├── Shared Contracts + Feature Registry
        ├── Local Application ──► Local Workspace/SQLite
        │                         └──► Local Worker ──► Local Engine Adapters
        └── HTTPS API ──► Application Modules
                          ├──► Pertamina Identity / Authorization
                          ├──► Module-owned SQL schemas
                          ├──► Job Store/Queue ──► Server Worker ──► Engine/Internal Provider
                          ├──► Storage Abstractions ──► Secure Object Storage
                          ├──► Owner Outboxes ──► Audit Store
                          └──► Owner Outboxes ──► SignalR Hub

All runtime components ──► Observability (one-way technical signal)

Forbidden reverse edges:
Provider/Storage/Observability/SignalR → business command authority
Module Infrastructure → another module table/infrastructure
Assistant → SQL/shell/filesystem/arbitrary API/provider
Local-only Feature → API/server dependency
```

Document 17 FINAL mengunci deployment overlay berikut:

```text
Public/approved client surface:
Clients → approved SSO + HTTPS → Web / API / SignalR
                               ├─ /api/compatibility
                               └─ minimal health exposure

Restricted/internal dependency surface:
network/time/identity/config/secret
→ SQL / private storage / Job Store-Queue / Audit path
→ API + Server Worker readiness
→ Web + SignalR
→ compatible Desktop/Mobile clients
```

Web, API, Server Worker, Desktop, Local Worker, Mobile/Tablet, SQL Server, Secure File/Object Storage, Monitoring/Telemetry, dan SQLite local state tetap logical deployables/dependencies yang distinct. Local Development, Shared Development, QA/Testing, Pilot, dan Production memakai exact promoted artifact dengan environment-specific configuration/secret. Co-hosting temporary tidak menggabungkan identity, config, data/folder, health, atau resource budget.

## 14.2 Dependency register

| Dependency/type | Affected component | Impact when unavailable | Fallback/degraded behavior | Monitoring/runbook | Owner | Confirmation gap |
|---|---|---|---|---|---|---|
| Canonical Feature Registry — hard/runtime | UI, Assistant, API dispatcher, resolver | Cannot safely identify/validate feature | Fail closed; preserve eligible known local state but no invented Feature | Version/registry validation; `RB-DATA-INTEGRITY-001` | Platform Configuration/Feature Registry | Exact runtime ownership/cache invalidation contract |
| Shared typed Contracts — hard/build+runtime | All hosts/modules | Build or compatibility failure | No dynamic dictionary/reflection bypass | Contract/architecture tests | Contract owner per module | Internal contract registry/version policy |
| Approved Pertamina Identity — hard for online/external, optional for valid local offline | API/Hub/server actions; client login | Online action denied/waits | Valid protected offline grant may allow eligible local-only work | Auth outcomes; `RB-SSO-FAIL-001` | Identity integration + Pertamina IAM | Protocol/issuer/audience/claims/revocation |
| Permission/policy/configuration store — hard/runtime | API authorization, resolver, Admin | Privileged/server actions fail closed | Narrow valid offline projection only for eligible local actions | policy revision/failure; cross-user tests | Identity/Admin/Configuration | Cache invalidation/freshness/SoD |
| SQL Server — hard for server metadata | API/Worker/Jobs/Secured/History/Audit/Support | Server operations unavailable; no true `202` | Local-capable work stays local; no direct storage bypass | readiness/DB I/O; `RB-STORAGE-FAIL-001`, `RB-API-OUTAGE-001`, `DRB-DB-MIG-FAIL-001` | Module DB owners/DB operations | Physical topology/HA/RPO/RTO/connection budgets |
| Secure object storage — hard for server payload; optional for local-only | Upload/result/secured/Issue/export | Content transfer/result unavailable | Local operation/result where contract allows; no SQL BLOB fallback | capacity/digest/failure; `RB-STORAGE-FAIL-001`, `DRB-STORAGE-FAIL-001` | Storage platform + domain owner | Product/key/retention/delegated download/topology |
| Job Store/Queue — hard for server background | API/Server Worker/exports | Cannot durably accept/dispatch Job | Immediate result only if actual safe/fast contract; otherwise reject/wait | depth/age/lease/retry; `RB-QUEUE-OVERLOAD-001`, `DRB-QUEUE-HIGH-001` | Jobs/Processing | Queue implementation, broker decision, delivery targets |
| Server Worker — runtime | Server background features/exports | Queued/waiting work not executed | Explicit `WAITING_RESOURCE`; local route only if resolver had approved it independently | worker/lease/duration; `DRB-WORKER-FAIL-001` plus Incident Queue/API runbooks | Processing operations | Scale/concurrency/drain/topology targets; logical drain/lease behavior locked by Document 17 |
| SignalR — optional/runtime | Activity Center realtime | Realtime degraded only | REST reconciliation/polling; Job continues | gaps/reconnect; `RB-SIGNALR-001` | Realtime publisher/host | Connection scale/transport/proxy details |
| Local Worker — optional/fallback for heavy local, hard for feature profile requiring isolation | Desktop heavy local features | Affected local capability unavailable/stuck | In-process only for separately approved safe profile; never auto-server | IPC/heartbeat/crash; `RB-LOCAL-WORKER-001`, `DRB-DESKTOP-UPDATE-001` | Desktop/Processing | Exact IPC wire/auth/containment; Desktop+Worker signed cohort is locked default |
| Local Workspace/SQLite — hard for local state | Desktop/Mobile local work | Cannot safely persist/recover work | Do not silently reset or upload; safe block/recovery | integrity/storage/recovery; `RB-DATA-INTEGRITY-001`, `DRB-SQLITE-001` | Workspace/client | Encryption/key/WAL/backup/platform details |
| PDF/OCR/Conversion/Image engine — runtime | Core processing families | Feature/profile unavailable/fails | Approved compatible provider only; no vendor guess | engine error/resource; `RB-MALICIOUS-DOC-001` | Owning feature module | Vendor/PoC/license/security/resource |
| Content Intelligence provider — optional/fallback | AI Summarizer/Translate | Intelligence route unavailable | Approved local provider if capable; otherwise explicit unavailable, never public AI | circuit/timeout/policy; `RB-AI-PROVIDER-001` | Content Intelligence owner | Module owner/provider/data governance |
| Observability backend — optional for business execution except security policy may block critical actions | Operations/incident detection | Reduced visibility | Local bounded safe buffering or explicit degraded state; no business data loss | self-health/ingest; Incident process | Observability/platform | Retention/alerts/access/topology |
| Audit ingestion/store — hard for required L3/privileged intent; asynchronous for governed lower levels | Owner modules/Admin/Support/Auditor | Evidence path degraded/blocked by policy | Protected outbox and retry; block required privileged action if intent cannot be durable | backlog/schema/sequence; `RB-AUDIT-FAIL-001` | Audit | Exact degraded-mode policy/retention/hold |
| Support/Issue service — runtime | User issue/report and Support console | Issue submit/status unavailable | Preserve local draft/message and retry; do not auto-send screenshot | outbox/attachment; `RB-ISSUE-001` | Support | Routing/owner/retention/contact |
| Azure DevOps artifacts/config — hard build/deployment governance | All deployables/contracts | Cannot verify/promote compatible release | Use retained verified artifact only; no workstation rebuild/patch | artifact/config/version; `RB-BAD-RELEASE-001`, `DRB-ROLLBACK-001`, `DRB-HOTFIX-001` | Release/DevOps | Exact pipeline resources, repository controls, approver, artifact retention, environment topology |
| External package/native/model sources — build/external | Build and engine packages | Supply-chain/build blocked | Approved retained feed/artifact only; no arbitrary public download | scan/SBOM/digest | DevOps + dependency owner | Approved feeds/licenses/signing/provenance |

---

# 15. Source-of-truth matrix

| Subject | Source of truth | Non-authoritative projections/caches | Forbidden authority shortcut |
|---|---|---|---|
| Feature ID/definition | Canonical Feature Registry/deployed configuration snapshot | UI catalog/cache, Assistant alias index | UI label, Assistant guess, route body discriminator |
| API contract | Typed C# contract + generated governed OpenAPI linked to build | Client generated models/cache | Handwritten client payload, database schema, wiki example alone |
| Compatibility/capability | Versioned compatibility/configuration/Feature Registry policy | Client last-known projection | Cached availability, visible button, SignalR event |
| Authentication identity | Validated Pertamina assertion/session mapped by issuer+subject to PrincipalId | Safe profile projection/offline grant | Email/display name/client claim |
| Permission/policy | Current server permission/policy/config revisions; protected eligible offline grant for local-only | UI visibility/cache | Role name from client, menu, stale token/permission snapshot |
| Server Job state | Persisted `jobs.Jobs` + attempts/state history/checkpoints | SignalR event, client Activity cache | Hub connection/event order, Worker memory |
| Local Job/workspace state | Per-profile SQLite + protected local artifact manifest | UI progress | Local Worker message/process existence alone |
| Realtime status | Display/event only; latest persisted domain state wins | Hub buffer/client sequence cache | SignalR as terminal authority |
| Server metadata | SQL Server schema owned by module | Search/list projections | Client state, object metadata, storage key |
| Local metadata/state | Per-profile SQLite | In-memory view model | Server generic sync or Worker direct DB access |
| Working/source/draft/cache/temp | Local Workspace/user-selected local source | Recent/history metadata | Server workspace, SQL BLOB, silent sync |
| Server temporary/result payload | Private processing object store tied to `jobs` metadata | Download stream/cache headers | Raw object URL/key |
| Secured payload/version | Secure object storage object version + `secured` aggregate metadata | Device secured-file cache | Local cache, filename, user-supplied ID |
| Issue status/content | `support` Issue aggregate/status history + exact attachment object | Reporter/Support list projection, SignalR status | Incident/Audit/log copy |
| Personal History | Local/server History projection | UI cache | Enterprise Audit or Job table as user-deletable history |
| Enterprise Audit | `audit.AuditEvents` append-oriented store | Export artifact/report | Observability log, History, Issue, client batch |
| Technical health | Approved Observability/readiness instrumentation | Dashboards/alerts | Audit/Issue as monitoring database |
| Engine/provider selection | Approved Engine/Content Intelligence Resolver + versioned capability/policy | Safe provider profile code in telemetry | User/Assistant/provider request, file content instruction |
| Release/deployed interface set | Original verified immutable pipeline artifact + Document 17 Release/compatibility manifest and promotion evidence | Runtime version display/health observation | Rebuild from source/workstation copy, environment branch, or untracked binary/config |

---

# 16. Failure/retry map

| Boundary/failure | Authoritative detection | Retry/idempotency | Recovery/user behavior | Cleanup |
|---|---|---|---|---|
| REST timeout before durable acceptance | No Job/resource/commit can be confirmed | Retry same Idempotency-Key/fingerprint; inspect safe response/Location | Do not claim success; reconcile if outcome ambiguous | Owner-domain reconciliation for staging/idempotency record |
| REST returns `202` | Persisted Job exists | Do not execute again; GET same Job; duplicate same key returns same Job | Activity shows queued/waiting; SignalR optional | Job/result retention policy |
| `429` caller limit | API policy/admission | Retry only after `Retry-After` with same logical request/key | Explicit wait/reject, no silent spinner | None beyond expired reservation/request state |
| `503` shared readiness/capacity | Readiness/admission | Bounded retry only if contract says safe | Local-capable action may stay local; server-required waits/rejects | Release unused reservation/staging |
| Upload disconnect | Upload session committed offset/digest | Resume from authoritative offset; repeated same range+digest acknowledged | Conflict returns expected offset; no guessed append | Expire inactive non-final session/temp |
| SignalR disconnect/gap/unknown version | Client sequence vs persisted revision | Reconnect with backoff; never retry command | REST reconcile jobs/issues; realtime marked degraded | Drop stale/duplicate client events |
| Local IPC disconnect/heartbeat miss | Desktop-owned local_job + Worker instance binding | Restart/recycle; compatible checkpoint resume or new bounded attempt | UI survives; no false terminal state | Kill stale Worker; validate/quarantine/delete orphan staging by policy |
| Server Worker crash/lease expiry | Persisted state/lease/attempt | Reclaim with new attempt; same logical Job; checkpoint only compatible | Visible retry/wait/fail; stale completion rejected | Release old lease/reservation; reconcile partial object |
| Engine/provider transient failure | Normalized adapter outcome | Bounded allowlist retry, backoff/jitter; provider idempotency where supported | No automatic unapproved fallback; explicit state | Recycle process/context; clear transient artifact |
| Engine/provider permanent/security failure | Validation/security classification | No automatic retry/fallback to bypass | Typed safe failure/quarantine; security incident as needed | Protected quarantine/evidence lifecycle |
| SQL unavailable/deadlock/transient | Module repository/transaction | EF/provider execution strategy only for safe idempotent transaction unit; unique constraints/outbox prevent duplicates | Server action fails/waits; local-first continues where eligible | Rollback transaction; reconcile external stage |
| Object storage partial write/finalize | Digest/object metadata vs owner metadata | Same operation/key; stage/finalize/reconcile | Do not expose partial final; safe failure | Bounded orphan scanner with owner/reference/lease checks |
| Audit append timeout/ack loss | AuditEventId uniqueness/store | Replay same event/batch key; duplicate acknowledged | Preserve protected outbox; privileged action block if required intent not durable | Remove local/outbox only after durable ack/policy |
| History projection failure | Source event/outbox | Consumer dedupe/retry | Business action remains complete; History temporarily stale | Retain outbox until projected/expired by policy |
| Issue attachment failure | Issue aggregate vs attachment state | Same key/parent; retry attachment only | Message remains; send without attachment option | Protected local/server staging expiry |
| Secured storage metadata/object mismatch | Secured aggregate/version + object digest | Idempotent reconciliation; no duplicate version | Do not advance current pointer until valid; conflict visible | Remove orphan stage or repair pointer via owner service only |
| SSO/refresh/revocation | Identity/session validation | Only allowlisted transient single-flight retry | Stop online/Hub; eligible valid offline-only route may continue | Clear invalid credential/grant/memory state |
| Observability unavailable | Telemetry SDK/exporter health | Bounded buffer/backoff/drop per safe policy | Business state unaffected unless required security visibility policy blocks action | Drop/expire sanitized telemetry, never retain content |
| Version/contract mismatch | Compatibility/handshake/contract validator | No blind retry | `UPDATE_REQUIRED`/unsupported fail closed; preserve local work | Cleanup only transient attempt; do not reset data |

---

# 17. Security/privacy validation

## 17.1 Validation results

| Audit question | Result | Required control |
|---|---|---|
| Endpoint without owner | **Partial gap** | Existing route families map to logical/schema owners; several admin/monitoring permissions lack Document 06 routes and must not be invented |
| Interface without version | **Gap** | Public API/SignalR are versioned; exact local IPC, several internal events, engine/storage contracts still need controlled versions |
| Unknown consumer | **Gap for internal events** | Consumer registry required; public route/hub consumers are identifiable |
| Data flow without auth | **No authorized exception identified** | Compatibility/health use only documented safe exposure; all content/resource flows require auth/ownership/scope |
| Retry without idempotency | **Controlled for REST/Job/upload/Audit; gap for provider/internal event details** | Provider call and internal consumer contracts must state idempotency/dedupe before coding |
| SignalR as source of truth | **Rejected** | Persist-before-publish; REST persisted state always wins |
| Direct DB from client | **Rejected** | API/application/local persistence abstractions only |
| Direct module infrastructure access | **Rejected by architecture** | Architecture tests must enforce project/reference and repository ownership |
| Arbitrary Assistant API access | **Rejected** | Only canonical FeatureExecutionRequest/WorkflowPlan and same application services |
| Public AI flow | **Rejected** | Local or approved enterprise-internal provider only |
| Sensitive telemetry/Audit | **Rejected** | Closed allowlist and negative canary tests across logs/events/outbox/export |
| Local tool silently uses server | **Rejected** | Resolver + policy + explicit transfer/consent; local-only has zero server processing calls |
| Unmonitored external dependency | **Gap until provider/topology selected** | Every external dependency needs owner, health, circuit, alert, runbook, and safe evidence |
| Missing failure path | **Several operational details open** | Queue/IPC/provider/storage/deployment exact recovery contracts require confirmation/PoC |

## 17.2 Prohibited data by boundary

| Data | REST business payload | SignalR | IPC | Audit | Observability | Issue |
|---|---|---|---|---|---|---|
| PDF password | Operation memory only where required | Never | Only if unavoidable operation-scoped protected mechanism is explicitly designed; current safest recommendation is Desktop-mediated ephemeral use, never persist | Never | Never | Never |
| Access/refresh token, secret/key | Authorization/approved credential boundary only | Transport only by approved non-URL mechanism | Never as job parameter | Never | Never | Never |
| Document/OCR full content | Only exact upload/provider/processing/download contract | Never | Job-scoped local processing only | Never | Never | Never auto; attachment only explicit user send |
| Visual signature | Local signature operation only | Never | Job-scoped local operation only if necessary | Never payload | Never | Never auto |
| Clipboard | Local explicit input only | Never | Only job-scoped if actual Feature requires, not general access | Never | Never | Never auto |
| AI request/response/document payload | Approved local/internal provider contract only | Never | Only if approved local provider job contract | Never | Never | Never auto |

Every opaque ID, Correlation ID, Job ID, cursor, fileRef, ETag, connection ID, event ID, and storage reference is an identifier—not authorization.

## 17.3 Global telemetry and Audit exclusion

Tidak pernah dikirim ke telemetry atau ordinary Audit payload: **password, token, secret, document content, OCR full text, visual signature payload, clipboard content, dan AI document/prompt/provider payload**. Audit hanya menerima metadata tindakan yang typed dan aman. Bila sebuah konten diperlukan oleh engine atau approved internal provider untuk menyelesaikan operasi pengguna, konten tersebut tetap berada pada processing boundary yang purpose-scoped dan tidak disalin ke SignalR, History, telemetry, Audit, atau diagnostic Issue otomatis.

---

# 18. Orphan/circular dependency audit

## 18.1 Orphan/interface gaps

| Finding | Severity for design | Evidence | Recommendation without contract invention |
|---|---|---|---|
| Production physical topology/resources remain intentionally unresolved | High operational confirmation gap, not a missing Document | Document 17 FINAL locks logical boundaries but explicitly leaves hosting/OS/container/proxy/DNS/TLS/ports/firewall, products, capacity, authority, and executable commands open | Use Document 17 logical deployables/startup order; finalize environment-specific cartography only after official infrastructure/network/security approval |
| Admin user/session/role/permission REST routes absent | High functional gap | Permissions exist in Document 09; no Document 06 route | Controlled API decision/addendum or explicitly mark surfaces not exposed; do not infer CRUD routes |
| Admin compatibility/maintenance update route absent | Medium/High | Permission exists; only compatibility GET/effective config and policy/flag/limit PUT routes exist | Decide whether typed policy route owns it or Document 06 addendum is needed |
| Monitoring aggregate API absent | Medium | `monitoring.*` permissions exist; only minimal health routes | Keep operations dashboard integration outside public API until explicit contract |
| Activity REST authority not separately addressable | Medium | `ActivityUpdated` exists; no Activity route | Define whether Activity is reconstructed from Jobs/Issues/History or needs future controlled resource |
| Export retrieval binding unclear | High security gap | Audit/Support export creation routes + read permissions; no dedicated read route | Bind to authorized Job result only after explicit parent-scope/permission contract confirmation |
| Local IPC wire contract/version/auth/timeout unresolved | High security/recovery gap | Architecture/security principles only | Create typed IPC contract register + PoC before implementation; no localhost HTTP workaround |
| Internal event catalog incomplete | High reliability gap | `DocumentProcessed` example and domain outboxes, but most names/versions/consumers unstated | Register owner/type/version/payload/consumer/dedupe/retention before coding |
| Server queue technology/delivery contract unresolved | Medium | External broker not baseline; Job Store/Queue conceptual | Start with persisted Job dispatch abstraction; decide technology from measured requirements |
| Content Intelligence module ownership unclear | Medium | Provider and `intelligence` schema exist; conceptual module list omits named module | Assign application/schema/contract owner without selecting vendor |
| ML Provider interface/owner not locked | Medium | Requested category; no exact interface in FINAL | Treat as capability behind resolver; no direct SDK reference until PoC/decision |
| Engine/storage interface coverage only examples | Medium | Examples exist, full operation schemas/versions do not | Build controlled contract inventory per capability, not one unversioned mega-interface |
| Exact SSO protocol/claims/workload identity unresolved | High security gap | Explicit `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` | Integration cannot be Production-ready until IAM contract is approved |
| External provider/storage monitoring owner unresolved | Medium | Vendor/topology not selected | Require owner/runbook/health/circuit during provider approval |

## 18.2 Circular dependency risks

No required architecture cycle is confirmed if boundaries are implemented as designed, but these cycles must be prevented:

| Potential cycle | Risk | Break mechanism |
|---|---|---|
| Jobs → Audit → Audit export Job → Jobs → Audit | Recursive event storm/commit coupling | Audit access intent owned by Audit; export execution via application contract; suppress progress noise; registered terminal event only; no direct table writes |
| Support → Job export → Job History/Audit → Support diagnostics | Cross-plane content pivot | Opaque Issue/Export refs, permission per plane, metadata-only events, no automatic content copy |
| Configuration → Feature Registry consumers → Configuration invalidation | Startup/cache loop | Configuration is one-way authority; versioned snapshot/event invalidates consumers; consumers never write config as fallback |
| Assistant → Feature modules → Assistant history/grounding | Assistant becomes dependency of business modules | Assistant is caller/orchestrator only; modules return typed results through shared contract and never call Assistant |
| Processing module → Engine adapter → application module | Provider adapter calls back into business layer | Adapter depends on engine abstraction and returns result only; no application service callback/tool execution |
| Domain → Observability/Audit service → domain decision | Logging/evidence outage changes arbitrary business behavior or deadlocks | One-way bounded emission/outbox; only explicitly policy-critical Audit intent can gate before mutation |
| Storage adapter → owner module for authorization → storage adapter | Reentrant authorization/data access | Authorize and construct job-scoped capability/context before adapter; adapter validates scoped request but does not call owner recursively |
| History projection → reprocess → Feature execution → History | Duplicate/infinite projection | Reprocess is explicit user command with new action/event/idempotency; projection consumer never auto-reprocesses |

Architecture tests should enforce: Hosts → modules, modules → own contracts/building blocks, Infrastructure → owned abstractions; never module infrastructure → another module infrastructure, providers → application modules, Assistant → arbitrary infrastructure, or clients → SQL/storage.

---

# 19. Confirmation gaps

1. `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` / `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION` — Document 17 FINAL now provides the authoritative logical deployment map, but exact Production hosting/OS/container/proxy/DNS/TLS/ports/firewall, service identities, pipeline resources, capacity, and executable runbooks remain unresolved.
2. `BLOCKED_BY_DECISION: REST_ADMIN_MONITORING_SURFACE` — decide disposition of admin user/session/role/permission, compatibility/maintenance, and monitoring permissions without routes.
3. `BLOCKED_BY_DECISION: EXPORT_RETRIEVAL_CONTRACT` — confirm Audit/Support export result authorization, parent binding, route, expiry, and reauth.
4. `BLOCKED_BY_DECISION: ACTIVITY_READ_MODEL` — define persisted Activity projection/REST reconciliation owner or formally declare reconstruction from existing resources.
5. `BLOCKED_BY_DECISION: INTERNAL_EVENT_CATALOG` — owner, canonical event names, EventVersion, payload, consumer, ordering, dedupe, retry, and retention.
6. `BLOCKED_BY_DECISION: LOCAL_IPC_CONTRACT` — serialization/framing, handshake identity, ACL, instance binding, compatibility, heartbeat, cancellation, timeout, resource limits, and PoC.
7. `BLOCKED_BY_DECISION: SERVER_QUEUE_DISPATCH` — persisted dispatch mechanism, claim/lease, external broker applicability, delivery semantics, drain, capacity, and owner.
8. `BLOCKED_BY_DECISION: MODULE_OWNERSHIP_COMPLETION` — Jobs, History, Secured Files, Platform Configuration, Content Intelligence/ML logical owners and project boundaries.
9. `REQUIRES_PERTAMINA_SSO_CONFIRMATION` — SSO protocol/profile, issuer/audience/claims/stable subject/provisioning/session/revocation/reauth/workload identity.
10. `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION`, `REQUIRES_PERTAMINA_SECURITY_CONFIRMATION`, dan `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` — SQL/object storage/queue/observability/provider product/topology, encryption/key/credential, network, capacity, health, HA/DR/RPO/RTO.
11. `BLOCKED_BY_DECISION: ENGINE_PROVIDER_PORTFOLIO` — vendor-free PoC outcomes, license/security/SBOM/data/network/resource behavior, primary/fallback profiles.
12. `BLOCKED_BY_DECISION: STORAGE_CONTRACT_AND_PROVIDER` — range/digest/finalize/retention/cleanup/restore semantics and vendor adapter.
13. `BLOCKED_BY_DECISION: CONTRACT_VERSION_POLICY` — compatibility/support window for API minor contract, IPC, internal event, checkpoint, configuration, capability, engine/provider.
14. `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` — exact rate/quota/concurrency/size/timeout/retry/retention numbers. No number is invented here.
15. `REQUIRES_PERTAMINA_SECURITY_PROCESS_CONFIRMATION` — provider content governance, Audit degraded behavior, VA/PT, incident authority, restricted evidence access/retention.
16. `BLOCKED_BY_DECISION: TECHNICAL_OBSERVABILITY_SCHEMA` — metric/log/trace schema, sampling, exporter buffering, alerts, owner, dashboard/access, retention.
17. `BLOCKED_BY_DECISION: DOCUMENT_06_AUDIT_BATCH_ADDENDUM` — compatible per-event acknowledgement/conflict semantics requested by Document 10.

---

# 20. Risks/conflicts

| Risk/conflict | Consequence | Mitigation |
|---|---|---|
| Treating Document 17 logical deployment map as implemented topology | False operational readiness | Keep pre-implementation status; require environment-specific approved topology, runbook status, exact artifact/config/version, drill, and release evidence |
| Filling missing admin/monitoring APIs by convention | Unauthorized API expansion | Route freeze against Document 06; controlled decision/addendum only |
| SignalR event used as state | Terminal regression, missed update | Persist-before-publish, sequence, dedupe, REST reconciliation |
| Local Worker granted SQLite/filesystem/network broadly | Cross-profile data access/RCE/exfiltration | Job-scoped refs, ACL/instance binding, closed IPC, protected root, no arbitrary access |
| Provider adapter selects public fallback | Data leakage/policy breach | Approved resolver profiles only; public AI technically unreachable |
| Assistant becomes integration bus | Permission bypass/arbitrary tool access | Canonical Feature Registry/application service only; same auth/confirm/gates |
| Module reads another schema for convenience | Ownership coupling/migration deadlock | Application contract or internal event; architecture tests |
| Generic global outbox/event table | Ownerless data and coupling | Domain-owned outbox; Audit ingestion separate |
| Audit write blocks all low-risk work indiscriminately | Availability failure | Policy-defined L2 degraded behavior; durable owner/local outbox; strict L3 intent |
| Audit/export Job recursive events | Event storm/evidence noise | Explicit event catalog and recursion guard; accepted/terminal only |
| Retry with new idempotency key | Duplicate Job/secured version/Issue/export | Persist/reuse same logical key/fingerprint until terminal/reconciled |
| Storage ref treated as authorization | IDOR/content disclosure | Parent ownership/scope/state authorization before object resolution/stream |
| `202` before durable acceptance | Lost work/false queued state | Persist Job/attempt/inputs/idempotency/reservation/dispatch before response |
| Engine output trusted because input validated | Malicious/corrupt final output | Validate output structure/type/size/security before finality commit |
| Over-telemetry of Job/Correlation/path | Re-identification/content correlation | Minimize/cardinality/access/retention; never content/path/object key by default |
| IPC/API/event version skew | Crash/data loss/unsafe resume | Explicit handshake/compatibility matrix; unknown version fail closed |
| Queue/provider/storage unmonitored | Silent wait and capacity collapse | Owner, health/circuit/backpressure/alert/runbook before enabling dependency |
| Local-only Feature takes server dependency through shared service | Violates Local-First/offline | Architecture test zero server call; split local abstractions from server adapters |
| Content Intelligence owner absent | Orphan schema/provider/security responsibilities | Assign one module/application owner in master cartography |
| Export read permission lacks explicit retrieval contract | Cross-scope evidence disclosure | Parent-bound Job result retrieval decision and authorization tests before release |

---

# 21. Suggested Document 18 text

## 21.1 Integration baseline

PERTAMINA OTO menggunakan REST untuk command/query/transfer, SignalR hanya untuk notifikasi realtime, Named Pipe/local IPC untuk Desktop–Local Worker, typed Application Interface untuk komunikasi sinkron antar-module, dan owner-domain outbox/Internal Event untuk proyeksi pasca-commit. Client tidak mengakses SQL Server/object storage langsung. Modul tidak mengakses infrastructure atau table milik modul lain. Assistant menggunakan canonical Feature Registry dan application service yang sama dengan UI.

Operasi local-only tidak memiliki dependency server. Background lokal menggunakan Local Worker; background server menggunakan persisted Job Store/Queue dan Server Worker. SignalR, Worker memory, cache client, identifier, dan storage reference bukan source of truth atau bukti otorisasi.

## 21.2 Authority map

```text
Canonical Feature ID     → Feature Registry
Typed HTTP contract      → C# contract + generated OpenAPI
Server Job state         → persisted jobs store
Realtime                 → display/event only; REST persisted state wins
Server metadata          → module-owned SQL Server schema
Local state              → per-profile SQLite
Working/source files     → Local Workspace/user-selected filesystem
Secured payload          → private Secure File/Object Storage + secured metadata
Enterprise Audit         → append-oriented Audit Store
Technical health         → approved Observability backend
Engine/provider choice   → approved Resolver + capability/policy
```

## 21.3 REST interface

Functional API uses `/api/v1`; compatibility uses `/api/compatibility`; liveness/readiness use `/health/live` and `/health/ready`. Canonical server execution is `POST /api/v1/features/{featureId}/execute`. Only routes listed in Document 06 are valid. A breaking HTTP surface requires controlled API-major/version decision.

Every route has one owning application module, typed request/response, authentication/permission/ownership/policy/state checks, API/contract/schema version, source of truth, idempotency/retry behavior, rate/admission control, and safe monitoring. Mutating/expensive commands marked by Document 06 require `Idempotency-Key`. GET/HEAD retries are bounded. `429` uses `Retry-After`; `503` expresses unsafe shared readiness/capacity, not hidden waiting.

Document 09 permissions without Document 06 routes—admin user/session/role/permission, compatibility maintenance, monitoring aggregates, and export retrieval—remain controlled gaps. No endpoint is inferred from permission names.

## 21.4 SignalR interface

The single authenticated Activity Hub is `/hubs/activity`. Its catalog is `JobQueued`, `JobStarted`, `JobProgress`, `JobWaiting`, `JobCompleted`, `JobFailed`, `JobCancelled`, `JobRetrying`, `ActivityUpdated`, and `IssueStatusChanged`.

State is persisted before publish. Delivery is at-least-once. Each event has Event ID, EventType, EventVersion, stream ID, persisted sequence/revision, timestamp, relevant safe identifiers, and safe code fields. Duplicate/stale events are ignored; gaps, unknown types/versions, and reconnect trigger REST reconciliation. Terminal state never regresses. Hub has no execute/upload/download/cancel/retry/resume/admin/Issue command.

## 21.5 Local IPC

Desktop communicates with the Local Worker only through `ILocalWorkerTransport` over authenticated, versioned local IPC/Named Pipe. No public or localhost network control port is exposed. The parent creates an instance-bound endpoint with restrictive local identity/ACL. Messages are framed and bounded and carry only canonical Feature ID, typed schema/parameters, job-scoped artifact references, request/Correlation IDs, deadline/budget, safe progress, cancellation, and terminal status.

The Local Worker has no arbitrary SQLite/SQL, filesystem, shell, process, registry, network, provider, secret, or application-service access. Desktop-owned local Job state and artifact manifest remain authoritative. A Worker crash does not terminate the UI; stale completion is rejected; compatible checkpoints may resume; partial output never becomes final.

## 21.6 Server Worker

Before returning `202`, the API durably persists the logical Job, initial attempt, input leases, idempotency record, admission reservation, and dispatch marker. The Server Worker uses a separate purpose-scoped workload identity, claims bounded Jobs by state/version/lease, reloads the authoritative handler/schema/references, invokes an approved adapter, validates output, and commits artifact reference + terminal state + outbox/Audit intent before notification.

Logical Job ID remains unchanged across retry/resume. Automatic retry is limited to transient allowlist with bounded attempts/backoff/jitter. Resume requires compatible input, Feature/schema, engine/provider version, stage, integrity, and checkpoint lifetime. Result/checkpoint/upload/temp cleanup is state-aware, bounded, and never removes active/leased/referenced or secured/source content.

## 21.7 Module, event, engine, and storage contracts

Cross-module communication uses callee-owned typed Application Interfaces or registered post-commit Internal Events. An event has stable Event ID, EventType/EventVersion, owner stream/revision, time, Correlation ID, and bounded safe payload. External message broker is not an initial baseline.

PDF, OCR, Conversion, Image, ML, Content Intelligence, and Storage capabilities sit behind owned abstractions and Resolver/adapter boundaries. No vendor is selected by this document. Provider approval requires PoC, license, security, data/network behavior, resource budget, health/circuit/timeout, output validation, SBOM/provenance, update, rollback, fallback, and runbook. Public AI and arbitrary provider/URL selection are prohibited.

Local Workspace, SQLite, SQL Server, processing temp/result storage, Secured Account Storage, Issue storage, and Audit export storage have separate owners and lifecycles. SQL stores metadata, not file payload by default. Cross-store writes use staging, validation, short owner commit, finalize, outbox, and idempotent reconciliation; physical path/object key is never a business contract.

## 21.8 SSO, Audit, and Observability

Approved Pertamina Identity is the authentication authority. OTO validates the approved assertion/session and maps issuer + subject to internal PrincipalId before resolving account/session eligibility, current permissions/policy, ownership/scope, state, consent, capability, and reauthentication. Exact SSO protocol/claims/lifetimes/revocation/workload identity require Pertamina confirmation. Token is never placed in URL, ordinary database, log, telemetry, Audit, Issue, or package.

Observability, Enterprise Audit, Personal History, and Issue/Support remain separate data planes. Observability receives safe technical fields and aggregates. Audit receives registered append-oriented metadata through owner/local outboxes and idempotent ingestion. History is an owned convenience projection. Issue receives explicit message/attachment only. No plane silently copies document, OCR, signature, clipboard, password, token, secret, or AI payload from another.

## 21.9 Data-flow and dependency governance

Every flow record identifies Actor, trigger, source, destination, data/classification, protocol, persistence, source of truth, permission, Audit behavior, error, retry/idempotency, and cleanup. Every dependency is classified as hard, optional, fallback, runtime, build, or external and records impact, affected component, degraded behavior, monitoring, runbook, owner, and confirmation gap.

Architecture tests prohibit direct client database/storage access, cross-module infrastructure references, provider callbacks into application commands, Assistant arbitrary API/tool access, local-only server dependency, SignalR authority, and circular dependencies. Potential Audit-export/Job, Support-export/Job, Configuration/cache, Assistant/Feature, provider/application, and storage/authorization cycles are broken through one-way owner contracts, outboxes, opaque references, and explicit recursion/dedupe rules.

## 21.10 Open decisions

Document 17 FINAL is the deployment authority and must be consumed through its Release Manifest, environment/configuration matrix, deployment runbooks, and release checklist. Before implementation is release-ready, Pertamina must still confirm the missing REST surface disposition, export retrieval, Activity read model, internal event catalog, local IPC contract/PoC, Server Queue dispatch, module ownership, SSO/workload identity, exact Production topology, provider/storage/observability products, engine/provider portfolio, version compatibility window, numeric limits/retention, Audit batch addendum, security/incident governance, and environment-specific executable runbooks.

---

**Agent B conclusion:** kartografi yang konsisten adalah arsitektur satu arah dan owner-based: clients/Assistant memanggil typed application boundary; modules own contracts and stores; engines/providers/storage sit behind adapters; Workers operate only on scoped Jobs; owner state is persisted before event projection; SignalR and Observability never become authority; Audit/History/Issue remain separate; and Local-First routes contain no accidental server dependency. Gaps yang ditemukan harus menjadi controlled decisions, bukan endpoint/interface/provider yang diciptakan saat coding.
