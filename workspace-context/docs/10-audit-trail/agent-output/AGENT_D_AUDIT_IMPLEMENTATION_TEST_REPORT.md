# AGENT D — Audit Implementation, Performance & Testing Report

**Task ID:** `DOC10-AGENT-D`  
**Scope:** Implementable Enterprise Audit architecture, typed contract, transactional consistency, offline recovery, performance, security, and testing  
**Requested baseline:** Documents 01–09 FINAL  
**Verified baseline:** Documents 01–08 FINAL; Document 09 FINAL was not present in the workspace. `ROLE_PERMISSION_DECISION_REGISTER.md` (18/18 AUTO-LOCKED) and `ROLE_MATRIX_INTEGRATION_AUDIT.md` (PASS) were read only as non-FINAL Document 09 integration context.  
**Output status:** Advisory input for Document 10 Integrator. No production code, external event broker, additional database, IAM/security vendor, or edit to the Document 10 master.

This report preserves these locked boundaries:

- Enterprise Audit is controlled append-oriented evidence, separate from observability, User History, and Support data.
- Audit contains safe metadata only and never document content, password, token, full OCR text, signature payload, Assistant prompt/chat, clipboard content, issue screenshot, or raw secret.
- `audit` is the single write owner of `audit.AuditEvents`, `audit.IngestionBatches`, and `audit.ExportRequests`.
- Feature modules do not write directly to Audit tables. They persist typed audit intent in their existing module-owned outbox when strong consistency is required.
- Offline Audit uses the existing protected SQLite `audit_outbox` and the existing idempotent audit-batch API.
- Role/permission enforcement from the Document 09 decision artifacts is preserved: Auditor read is scoped, Audit export is separate, ordinary update/delete does not exist, and Administrator/Support receive no implicit Audit or document-content access.

Normative terms:

- **MUST** — required to preserve Documents 01–08 or the locked Role Decision Register.
- **SHOULD** — recommended implementation design for Document 10.
- **CONDITIONAL** — applies only to the governed event/action class.
- **`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`** — exact enterprise policy, retention, severity, authority, threshold, product, or organizational scope is not locked.

Primary traceability: `FR-AUDIT-001..004`, `FR-AUTH-001..005`, `FR-ROLE-001..003`, `NFR-PERF-005`, `DR-DB-006`, `SR-SEC-001..023`, `FEAT-AUDIT-001..008`, Document 06 §§10.6, 14, 16.3, and 18; Document 07 §§6–17 and 21–25; Document 08 §§11–12, 23, 27–30, and 34–36; provisional Role Decisions `ROLE-DEC-001..018`.

---

## 1. Implementation architecture

### 1.1 Recommended server flow

```text
Application action / security decision / Worker state transition
→ typed AuditIntent created by the owning application service
→ event-specific safe-field validation
→ business mutation + module-owned OutboxMessage in one short transaction
→ commit
→ bounded Audit Dispatcher reads committed outbox rows
→ versioned Audit Ingestion Service validates, enriches, and deduplicates
→ audit.AuditEvents append
→ acknowledge source outbox row
```

The strong-consistency unit is normally **business change + durable audit intent**, not business change + direct insert into `audit.AuditEvents`. This preserves Document 07 module ownership and avoids distributed/cross-module feature code while ensuring the evidence cannot be silently lost after a critical mutation commits.

When the Audit module itself owns the action—for example permission-scoped Audit export request—the Audit application service may append the corresponding Audit event in its own short transaction because both records share one write owner.

### 1.2 Recommended offline/local flow

```text
Eligible local application action
→ typed safe local AuditIntent
→ local state/finalization + audit_outbox reservation/row atomically where possible
→ user work continues without waiting for network
→ reconnect authenticates and refreshes current client policy
→ bounded batch + Idempotency-Key to POST /api/v1/audit-events/batches
→ server validates contract/event version and source identity
→ server derives/verifies actor context, deduplicates, appends AuditEvents
→ client deletes/archives acknowledged outbox rows only under approved policy
```

The audit outbox is distinct from pending server commands, secured transfer, preferences, User History, and generic workspace sync. Audit synchronization never uploads document/file bytes or converts a local action into a server execution.

### 1.3 Read/query/export flow

```text
Auditor or explicitly granted Administrator request
→ authentication + audit.query/audit.export permission
→ approved organizational scope
→ reauthentication if policy requires
→ authorization before count/filter/cursor/export
→ bounded indexed query or asynchronous export request
→ safe access/export Audit event
→ result/export artifact under separate permission and expiry
```

Auditing Audit access must not recurse. The Audit service records one typed access/export event for the user operation; its internal append is not itself expanded into another Audit event.

### 1.4 Plane separation

| Plane | Purpose | Delivery/loss model | Prohibited use |
|---|---|---|---|
| Enterprise Audit | Required business/security evidence | Durable, append-oriented, idempotent; critical events never silently dropped | Health metrics, verbose debug, document content, User History UX |
| Observability | Technical health, performance, security detection | Sampling/aggregation may be approved; payload-safe | Compliance evidence authority or copy of full Audit payload |
| User History | Personal convenience projection | User-manageable within policy | Enterprise evidence or Audit deletion path |
| Support | Explicit issue/triage content | Case-scoped permission and retention | General Audit/document storage |

---

## 2. Audit abstraction

### 2.1 Recommended pattern

Use one small shared Audit contract/abstraction at **application use-case boundaries**:

| Conceptual component | Responsibility |
|---|---|
| `AuditPolicyCatalog` | Maps approved application action/event type to audit requirement, criticality, transaction mode, field schema, and retention class reference |
| `AuditIntentFactory` | Creates a typed intent from trusted application context and explicit safe business facts; never serializes arbitrary entity/request/exception |
| `AuditSensitiveFieldGuard` | Rejects prohibited field names/types/content sources and enforces event-specific allowlist/size/version |
| `IAuditIntentCollector` | Enlists one or more typed intents in the current owning application Unit of Work/outbox |
| `IAuditIngestionService` | Validates contract/version, enriches trusted server context, deduplicates, and appends final Audit events |
| `IOfflineAuditOutbox` | Persists protected local typed events, sequences them per source, batches, retries, and acknowledges idempotently |
| `IAuditQueryService` | Permission/scoped query, cursor/filter, and detail projection without mutation |
| `IAuditExportService` | Bounded asynchronous export request, safe rendering, artifact expiry, and access evidence |

Names are illustrative only; this is not production code.

### 2.2 Application-code usage

Application handlers should express business meaning once, for example conceptually:

```text
change approved policy
→ create PolicyChanged intent using typed old/new safe version metadata
→ enlist business update + outbox intent
→ commit one short transaction
```

They must not contain repeated direct persistence such as:

```text
db.Audit.Add(...)
```

and must not know the Audit database entity, table, retention, ingestion, export, or archive implementation.

### 2.3 Pattern evaluation

| Pattern | Assessment |
|---|---|
| Explicit typed intent from application service | **Recommended.** Contains business semantics and supports transaction selection/testability |
| Application pipeline/decorator | Useful for common actor/correlation/outcome wiring, but cannot infer sensitive business meaning or replace explicit event mapping |
| HTTP middleware only | Insufficient. Sees requests/status but not actual business commit, ownership, state transition, Worker completion, or offline action |
| EF Core interceptor/change tracker serialization | Prohibited as primary Audit design. Risks content leakage, free-form entity coupling, cross-module writes, noisy events, and unclear actor/action meaning |
| Logging/telemetry sink as Audit | Prohibited. Different delivery, access, retention, integrity, and loss semantics |
| External message broker | Not selected and not required. Existing SQL/SQLite transactional outboxes are sufficient for the current architecture |

### 2.4 Trusted context

The shared abstraction receives server-derived/current context where applicable:

- internal Actor Principal ID and actor type (`HUMAN`, `WORKLOAD`, or `SYSTEM`);
- approved role/unit snapshot codes and permission/policy revision;
- canonical Feature ID, Process ID, Job ID, Correlation ID;
- platform, application version, execution location;
- resource type and opaque resource reference only when evidence requires it;
- action outcome and stable safe error code.

Client-supplied actor, role, permission, owner, unit, outcome, or server receipt time is never trusted directly.

---

## 3. Event contract

### 3.1 Envelope

Every event MUST be typed, versioned, structured, safe, and independently testable. Recommended logical envelope:

| Field | Rule |
|---|---|
| `auditEventId` | Stable globally unique event ID assigned/accepted by the authoritative ingestion path |
| `clientEventId` | Required for offline/client-originated event; unique within source scope; nullable for server-originated event |
| `eventType` | Stable controlled code such as `Administration.PermissionAssignmentChanged`; not free-form display text |
| `eventVersion` | Version of this event type's field/schema semantics; independent from API batch `contractVersion` |
| `occurredAtUtc` | When the source action actually occurred; protected/validated for offline input |
| `receivedAtUtc` | Server ingestion time; always server-generated |
| `actorType` | Controlled `HUMAN`, `WORKLOAD`, or `SYSTEM` |
| `actorPrincipalId` | Server-derived/verified internal actor reference; never email/display name as authority |
| `roleUnitSnapshotCodes` | Approved minimized snapshot/evidence codes; trust/source must be explicit for delayed offline events |
| `actionCode` | Stable governed business/security action code; separate from role and Feature ID |
| `outcomeStatus` | Controlled outcome such as succeeded, failed, denied, cancelled, or partial where contract permits |
| `featureId` | Canonical Feature ID when applicable; nullable for identity/policy/system events |
| `processId`, `jobId`, `correlationId` | Optional safe trace identifiers when applicable; never authorization |
| `resourceTypeCode`, `resourceRef` | Opaque/minimized and only when the event requires resource evidence; no path/storage key |
| `executionLocation` | Controlled local/server/client/worker context when applicable |
| `platformCode`, `appVersion` | Controlled platform and deployed client/application version |
| `inputSizeBytes`, `outputSizeBytes` | Optional non-negative sizes or approved size category; no content |
| `errorCode` | Stable safe code only; no raw exception/engine/provider message |
| `authorizationContextVersion` | Permission/policy/offline-grant revision reference where required; not a raw claim dump |
| `safeMetadata` | Optional event-type-specific, allowlisted, versioned, size-bounded object; never an arbitrary free-form bag |

Document 07 fields `AuditEventId`, ingestion batch link, `OccurredAtUtc`, `ReceivedAtUtc`, actor/role-unit snapshot, type/outcome, Feature/Process/Job/Correlation IDs, execution location/sizes, error/platform/version, and safe metadata remain preserved.

### 3.2 Safe example

```json
{
  "auditEventId": "aevt_01K...",
  "clientEventId": null,
  "eventType": "Administration.PermissionAssignmentChanged",
  "eventVersion": 1,
  "occurredAtUtc": "2026-08-26T04:10:00Z",
  "receivedAtUtc": "2026-08-26T04:10:00Z",
  "actorType": "HUMAN",
  "actorPrincipalId": "prn_01J...",
  "roleUnitSnapshotCodes": ["ADMINISTRATOR", "UNIT_APPROVED_CODE"],
  "actionCode": "admin.permission.assign",
  "outcomeStatus": "SUCCEEDED",
  "featureId": "FEAT-ADMIN-003",
  "processId": "proc_01K...",
  "jobId": null,
  "correlationId": "corr_01K...",
  "resourceTypeCode": "PRINCIPAL_PERMISSION_ASSIGNMENT",
  "resourceRef": "asg_01K...",
  "executionLocation": "SERVER",
  "platformCode": "WEB",
  "appVersion": "1.4.0",
  "errorCode": null,
  "authorizationContextVersion": "permrev_42",
  "safeMetadata": {
    "permissionCode": "approved.permission.code",
    "assignmentOperation": "ASSIGNED",
    "targetScopeCode": "APPROVED_SCOPE_CODE",
    "previousVersion": 8,
    "newVersion": 9
  }
}
```

The example contains no target display name, email, source document, file name/path, permission claim dump, token, or free-form justification. Exact permission code/scope vocabulary comes from the final Role Matrix, not this report.

### 3.3 Event taxonomy

Recommended controlled event families:

| Family | Representative typed events | Notes |
|---|---|---|
| Identity/session | `Identity.AuthenticationSucceeded`, `Identity.AuthenticationFailed`, `Identity.SessionEnded`, `Identity.SessionRevoked`, `Identity.AccountEligibilityChanged` | Login/logout/failed login/session change; no token/claim dump |
| Authorization/security | `Security.PrivilegedAuthorizationDenied`, `Security.ReauthenticationCompleted`, `Security.ReauthenticationFailed` | Audit meaningful privileged/security decisions; ordinary high-volume denials remain safe observability unless policy elevates them |
| Processing/Feature | `Processing.ExecutionAccepted`, `Processing.ExecutionCompleted`, `Processing.ExecutionFailed`, `Processing.ExecutionCancelled` | Feature usage/process/local-server/error evidence; no progress event per percentage/page |
| Secured lifecycle | `Secured.FileCreated`, `Secured.VersionAdded`, `Secured.VersionRestored`, `Secured.FileDeletionRequested`, `Secured.FilePermanentlyDeleted` | Owner/resource opaque metadata; reauth evidence where required |
| Administration | `Administration.RoleAssignmentChanged`, `Administration.PermissionAssignmentChanged`, `Administration.PolicyChanged`, `Administration.FeatureExposureChanged`, `Administration.LimitChanged`, `Administration.MinimumVersionChanged`, `Administration.MaintenanceModeChanged` | Critical control-plane changes; before/after typed version/codes only |
| Support | `Support.IssueSubmitted`, `Support.IssueStatusChanged`, `Support.AttachmentAccessed`, `Support.ExportRequested`, `Support.ExportCompleted`, `Support.ExportFailed` | No issue message/note/screenshot bytes in Audit |
| Audit governance | `Audit.EvidenceQueried`, `Audit.ExportRequested`, `Audit.ExportCompleted`, `Audit.ExportFailed`, `Audit.RetentionPolicyChanged`, `Audit.LegalHoldChanged` | Query/export permission separate; retention/hold mutation policy-confirmed |
| Assistant | `Assistant.ExecutionResolved`, `Assistant.WorkflowCompleted`, `Assistant.WorkflowFailed` | Intent category, resolved Feature ID, outcome, Correlation ID only; no prompt/response/document content |
| Key/recovery | `Security.KeyLifecycleChanged`, `Recovery.RestoreExecuted`, `Recovery.RestoreValidationFailed` | Safe key reference/version/operation metadata only; no key material |

The final Document 10 event catalog must map each `AUDIT_REQUIRED = YES/NO/CONDITIONAL` action from the final Document 09 matrix to exactly one required event pattern. Because Document 09 FINAL is absent, this mapping cannot be declared complete in this report.

### 3.4 Explicit exclusions

The contract validator MUST reject:

```text
password or PDF password
access/refresh token, cookie, secret, private/encryption key
document/file bytes, full text, page content, extracted content
full OCR text
clipboard content
visual signature/paraf payload or template
Assistant prompt/chat or provider request/response
issue message/note/screenshot/attachment bytes
filename or full path by default
physical storage key, durable URL, connection string
raw exception, stack trace, SQL, engine/provider message
raw claim dump or arbitrary request/response body
```

### 3.5 Versioning and compatibility

- `eventType` and `eventVersion` select a registered schema/mapper/validator.
- Compatible readers ignore only explicitly optional unknown fields; unknown event type/version is never coerced into a generic string event.
- An unsupported version is retained in the existing source outbox/failed lifecycle, alerted, and retried only after compatible deployment or governed migration; it is not dropped.
- Event semantics are immutable after release. A material field/meaning change increments `eventVersion` and retains readers for supported historical versions.
- Export renders the event version that was stored and labels unknown/deprecated codes safely.

The Document 06 offline-batch example has batch `contractVersion` but no per-event `eventType` or `eventVersion`. A controlled Document 06 addendum/compatible contract revision is required before implementing this typed contract.

---

## 4. Transaction strategy

### 4.1 Transaction classes

| Class | Consistency rule | Failure behavior |
|---|---|---|
| `ATOMIC_AUDIT_INTENT` | Business mutation and typed audit intent enter the owning module's existing outbox in the same short transaction | Audit-intent insert/validation failure rolls back the business mutation: **FAIL CLOSED** |
| `DURABLE_BEFORE_DISCLOSURE` | Privileged query/export/content access records a durable access/export event or intent before releasing the protected result/artifact | Failure prevents disclosure/export acceptance: **FAIL CLOSED** |
| `DURABLE_RETRY_AFTER_COMMIT` | A valid outbox/audit_outbox intent already committed with the action; final Audit projection/sync is asynchronous | Business result remains valid; dispatcher retries idempotently: **ALLOW + DURABLE AUDIT RETRY** |
| `OBSERVABILITY_ONLY` | Technical metric/debug event is not required Enterprise Audit evidence | May aggregate/sample/drop under approved telemetry policy; must never be relabeled as Audit |

### 4.2 Action strategy matrix

| Action/event | Required strategy | Reason |
|---|---|---|
| Successful login/application session creation | Session creation + audit intent atomic; do not grant session if required intent cannot persist | Login is mandatory Audit evidence and creates authority |
| Failed login | Authentication remains denied regardless; write security Audit through an independent durable path/retry where available | Audit failure cannot turn failed login into success; inability to persist must alert |
| Logout/current-session termination | Session invalidation + intent atomic where reachable; local logout also persists safe local intent | Prevent unaudited session lifecycle |
| Other-user session revocation; account eligibility change | `ATOMIC_AUDIT_INTENT` | Privileged security mutation |
| Role/permission assign/revoke; delegation/SoD change | `ATOMIC_AUDIT_INTENT` | Privilege mutation; exact target/scope/version required |
| Security/classification/retention policy, Feature exposure, limit, minimum version, maintenance change | `ATOMIC_AUDIT_INTENT`; reauth/concurrency as applicable | Control-plane change with enterprise-wide impact |
| Permanent secured-content deletion, destructive recovery/key lifecycle | `ATOMIC_AUDIT_INTENT`; final physical cleanup may retry after tombstone | Irreversible/high-risk action |
| Server Feature/job admission and terminal completion | Job state/idempotency/artifact metadata + owning jobs outbox/audit intent atomic | Document 07 already defines job completion outbox/audit intent |
| Ordinary local Feature execution | Reserve/persist protected local audit intent at admission/finalization; after durable intent, processing and later sync use durable retry | Preserves offline UX without silent event loss |
| Issue submit/triage/status and explicitly sent attachment access | Owning Support mutation + outbox intent atomic; privileged attachment disclosure durable-before-disclosure where required | Preserves case/action evidence without copying issue content |
| Audit query | Record one bounded access event under approved policy; avoid per-row events and recursion | Evidence access is privileged; volume must remain bounded |
| Audit/Support/high-impact export | Export request + Audit event/intent atomic before job acceptance; access/download separately authorized/audited as policy requires | Bulk disclosure and asynchronous artifact |
| Job progress, queue heartbeat, UI navigation, every Merge page, OCR token/page, conversion stage | `OBSERVABILITY_ONLY` or no event unless a specific policy elevates it | Avoid Audit flood and user-performance impact |

### 4.3 Module ownership and transaction rule

No feature module obtains `AuditDbContext` or writes `audit.AuditEvents`. For a critical Jobs, Secured, Support, Identity/Admin, or Configuration action:

```text
owning aggregate mutation
+ module-owned OutboxMessage containing typed AuditIntent
= one local SQL transaction
```

The Audit Dispatcher later invokes the Audit-owned ingestion service. This is not a distributed transaction. If external object work is involved, staging/validation occurs outside the transaction; metadata/tombstone/outbox commit atomically; object promotion/purge and Audit projection reconcile asynchronously.

### 4.4 Large business trade-offs requiring Integrator decision

1. **Local queue unavailable/full:** blocking new local work preserves mandatory evidence but affects Offline-First availability. Recommended safe default is reserved Audit capacity plus early admission check; if no durable slot can be reserved, block new auditable action before mutation. Exact emergency behavior and capacity are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.
2. **Successful login when Audit store/outbox is unavailable:** recommended fail closed because granting a session without required login evidence weakens identity governance. Availability owner must explicitly accept any alternative.
3. **Privileged Audit/Support export/query when access evidence cannot persist:** recommended fail closed before disclosure. Allowing access with volatile retry is not equivalent to durable evidence.
4. **Routine server/local processing after durable intent exists:** allow completion and retry Audit projection/sync; rolling back completed heavy computation solely because projection is delayed is unnecessary and can corrupt UX/resource use.

---

## 5. Async strategy

### 5.1 Existing infrastructure only

No external event broker or new database is needed. Use:

- existing module-owned SQL outbox tables (`jobs.OutboxMessages`, `secured.OutboxMessages`, `support.OutboxMessages`, and equivalent approved owner outbox);
- existing local SQLite `audit_outbox`;
- existing `audit.IngestionBatches` and `audit.AuditEvents`;
- bounded hosted dispatcher/Worker processing under a purpose-scoped workload identity;
- existing idempotent `POST /api/v1/audit-events/batches` for offline ingestion.

If an owner currently has no outbox table/contract, Document 10 must route through an approved owner abstraction/migration rather than create an ownerless global outbox or let the feature write Audit directly. This is a controlled Document 07 implementation handoff, not permission to add a database.

### 5.2 Delivery semantics

- Delivery is at least once; storage is effectively once per logical event through unique stable Event ID/client source scope and idempotency.
- Dispatcher claims a bounded batch with a short lease/transaction, releases the connection, validates/projects events, and acknowledges only durable success.
- Retry uses bounded exponential backoff with jitter and does not hold a DB connection during delay.
- Failed/unpublished rows remain in the existing outbox lifecycle; cleanup never purges unacknowledged evidence.
- No global event order is assumed. Ordering is meaningful only within a declared source stream/action using source sequence/business row version; `OccurredAtUtc` plus `ReceivedAtUtc` represents delayed ingestion.
- Unknown event/version, invalid safe schema, or prohibited field fails the batch/event safely and remains diagnosable without logging the whole payload.
- Audit projection delay does not alter authoritative business state after a durable intent was committed.

### 5.3 Batching

Batch by bounded event count and serialized byte size, never only by time. Flush on:

- count/byte threshold;
- maximum approved age;
- application background opportunity/shutdown where safe;
- reconnect and healthy server capacity;
- critical event priority without starving normal events.

Do not create one network request per local Audit event, and do not create an unbounded in-memory buffer. The durable SQL/SQLite outbox is the buffer; memory is only a bounded delivery batch.

### 5.4 Asynchronous export

Audit export remains a bounded asynchronous job:

```text
authorized filter/version + Idempotency-Key
→ durable ExportRequest + Audit.ExportRequested
→ Worker executes indexed paged read
→ formula/macro/external-link-safe rendering
→ expiring protected artifact
→ Audit.ExportCompleted or Audit.ExportFailed
→ fresh authorization for result download
```

The export must not copy document content, issue message, prompt, password, token, signature, or unrelated Support data merely because related IDs exist.

---

## 6. Failure/recovery

### 6.1 Failure matrix

| Failure | Required behavior | Evidence/alert |
|---|---|---|
| Audit-intent validation/insert fails inside critical transaction | Roll back mutation/session grant/export acceptance; return safe failure | Stable error/Correlation ID and `audit_intent_write_failure` metric without payload |
| Audit projection insert fails after business/outbox commit | Keep outbox unpublished/failed; bounded retry; business state remains; no duplicate final event | Backlog count/oldest age/retry/failure reason code alert |
| SQL database unavailable | No server mutation or privileged disclosure without required durable intent; safe `503`; read paths follow their Audit-access policy | Availability and dependency alert |
| Local SQLite/audit outbox unavailable | Preserve source/current work; block admission of a new mandatory-audit local action unless durable reserved evidence already exists | User-safe degraded message; local audit health signal without content |
| Local queue reaches capacity ceiling | Stop accepting more audited actions after bounded sync/cleanup attempts; never delete unacknowledged rows; reserve space for terminal/critical events | Capacity/backlog/oldest age alert; Integrator-approved recovery instruction |
| Duplicate event/batch/retry | Unique Event ID/client source scope/Idempotency-Key returns or reuses prior logical result; no second Audit event | Duplicate count metric only |
| Delayed offline sync | Store both occurrence and receipt times; validate version/source; preserve original sequence; do not rewrite event as if it occurred at receipt | Delay-age distribution and excessive-delay alert |
| Application crash after business+outbox commit | Startup dispatcher resumes unpublished row; no business replay required | Recovery scan metric |
| Application crash before commit | Neither mutation nor outbox intent is visible | Normal transaction rollback evidence |
| Local app crashes during action | SQLite transaction/WAL recovers; reserved/outbox state classifies pending/completed event; retry idempotently | Recovery test and safe local status |
| Worker crashes before terminal commit | Job lease/attempt recovery; no terminal Audit event until verified terminal state; retry shares logical Job but unique attempt/event | Worker lease/retry and event dedupe metrics |
| Worker crashes after terminal transaction | Outbox row already durable; dispatcher retries projection; no duplicate terminal event | Unique event/stream sequence |
| Server restarts during dispatch | Lease expires/reclaims; at-least-once replay deduplicates | Reclaim count and oldest backlog |
| Unsupported/poison event version | Retain in failed/unpublished lifecycle, stop unsafe projection, alert owner; deploy compatible reader or governed migration | Event type/version/reason only; never raw payload in monitoring |
| Audit export generation fails | Export request becomes failed through owned state transition; append safe failure event; artifact absent/cleaned | Export failure/latency metric |
| Archive/retention job fails | Keep source evidence, retry bounded, do not delete/mark archived prematurely | Archive backlog/failure/oldest candidate alert |
| Restore | Revalidate Audit schema, event uniqueness, ingestion idempotency, owner permissions, outbox reconciliation, retention/hold, and no duplicate effects | Restore validation report |

### 6.2 Fail-closed versus durable retry decision

```text
FAIL CLOSED before commit/disclosure:
- role/permission assignment or revocation
- account eligibility or other-user session revocation
- high-impact security/classification/retention policy change
- permanent secured-content deletion and destructive/key recovery
- successful session grant when required login intent cannot persist
- privileged Audit/Support export or disclosure when access intent cannot persist

ALLOW + DURABLE AUDIT RETRY after a durable intent exists:
- accepted/terminal server Feature or Job state
- ordinary local Feature execution with protected reserved audit_outbox row
- Issue/status/Support mutation after owning transaction committed
- downstream Audit projection, batch delivery, or archive completion

ALLOW under telemetry policy, not Enterprise Audit:
- UI navigation, progress ticks, queue heartbeat, performance counters
```

No critical action may downgrade from `FAIL CLOSED` to volatile memory/best-effort logging. Any exception is a material business/security decision for the Integrator and official acceptance authority.

### 6.3 Actor context for delayed events

Document 06 currently says effective role/permission context is added server-side at ingestion, while Document 07 expects approved role-unit snapshot codes and records separate occurrence/receipt times. For a delayed offline event, received-time role may differ from occurred-time role.

Document 10 must distinguish:

- verified identity/source at ingestion;
- occurrence-time authorization/offline-grant revision or verifiable snapshot reference;
- received-time current role/permission context;
- trust status when historical context cannot be reconstructed.

It must not label current received-time role as the historical role without evidence. Exact reconstruction/source-signing mechanism requires a controlled API/security decision and `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

## 7. Offline handling

### 7.1 Local event lifecycle

Recommended state behavior using the existing `audit_outbox` lifecycle:

```text
PENDING_LOCAL
→ READY_TO_SYNC
→ IN_FLIGHT (bounded lease)
→ ACKNOWLEDGED
→ eligible for cleanup under approved policy

failure remains durable:
RETRY_PENDING / VERSION_BLOCKED / POLICY_BLOCKED
```

These are conceptual lifecycle labels, not a request to add a new database/table. The implementation may map them to existing approved outbox fields/statuses.

### 7.2 Local contract rules

- Event is created from the same shared typed catalog as server Audit.
- `clientEventId` is stable across retries; source profile/device/app installation scope is protected and opaque.
- Per-source sequence supports local ordering/reconciliation; global chronological order is never inferred.
- `occurredAtUtc`, monotonic/trusted-time evidence, offline grant/policy revision, platform/app version, Process/Feature/Correlation IDs, mode, safe sizes, outcome, and safe error code are stored when applicable.
- Token, cookie, password, secret, raw claims, document/file/OCR/signature/prompt/screenshot/clipboard/path are prohibited.
- Local outbox is encrypted/protected with SQLite/WAL/SHM/backup/artifact boundary and excluded from automatic consumer cloud/device backup under approved mechanisms.

### 7.3 Reconnect and sync

```text
connectivity available
→ validate compatible client and authenticated app/client policy
→ refresh account/session/permission/policy for command execution
→ select bounded eligible audit_outbox rows
→ POST idempotent batch with same clientEventIds and Idempotency-Key
→ server validates contract/event versions and source
→ server adds/verifies actor/context without trusting client role
→ append/deduplicate AuditEvents + IngestionBatch atomically
→ return per-batch/per-event durable acknowledgement
→ mark acknowledged locally
→ cleanup only under approved retention and recovery policy
```

Audit sync is not a queued business command and must not be discarded merely because a later permission was revoked. However, no anonymous/public ingestion is introduced. If a revoked/disabled user cannot authenticate, evidence remains protected locally until the approved authenticated client/device recovery path can ingest it. The exact recovery identity and maximum delay are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 7.4 Duplicate and ordering rules

- Reusing the same batch Idempotency-Key/request fingerprint returns the same logical ingestion result.
- Duplicate `clientEventId` within the same trusted source scope maps to one Audit event.
- Same ID with different fingerprint is a conflict/security signal, not an overwrite.
- Batch order is not legal order; use source sequence and business version for event-series consistency.
- A later-arriving older event keeps its original `OccurredAtUtc`, current `ReceivedAtUtc`, and delay/trust metadata.
- Missing sequence creates a reconciliation/gap signal; it does not invent the missing event.

### 7.5 Queue capacity

Use configured ceilings for event count, serialized bytes, oldest age, per-event size, batch size, and retry rate. Reserve protected space for terminal/critical events, compact only data proven redundant by event design, and never merge distinct required events into a free-form summary. Do not purge unacknowledged evidence to keep the UI green.

---

## 8. Performance

### 8.1 Hot-path design

Audit overhead on Merge PDF, OCR, conversion, and UI must be bounded:

- create a small typed intent from IDs/codes already in application context; do not serialize domain entities, files, OCR, requests, or exceptions;
- write one narrow outbox row in the existing short transaction at admission/terminal state, not one event per page, chunk, progress update, OCR token, or conversion stage;
- keep engine/file/provider work outside DB transactions and connections;
- project Audit asynchronously after commit;
- reuse Process/Job/Correlation/Feature IDs instead of duplicating metadata/content;
- batch offline ingestion and server outbox dispatch;
- do not synchronously call a remote Audit service from local processing or wait for SignalR/telemetry;
- avoid auditing ordinary UI navigation; use telemetry aggregates if needed.

### 8.2 Write path

| Concern | Control |
|---|---|
| Transaction latency | Narrow append-only outbox/event insert; minimal indexes on hot write path; one short transaction |
| Connection pressure | Claim small batches; release connection before retry/delay/rendering; no connection during engine work |
| Duplicate writes | Stable unique event/client ID and idempotency key; no read-before-insert race when unique constraint can decide |
| High concurrency | Sequential internal key, unique public Event ID, bounded batch, owner/source sequence, short leases |
| Progress volume | Persist coalesced business progress separately; Audit only meaningful accepted/terminal/security action |
| Large safe metadata | Event-specific size cap/typed columns; do not put arbitrary large JSON or exception in `SafeMetadataJson` |

### 8.3 Query/index path

Preserve Document 07 index intent:

- actor/time/unique-row ordering;
- Feature/time/unique-row ordering;
- status/time/unique-row ordering;
- Process ID lookup;
- Job ID/Correlation ID only where justified by real query/support evidence;
- keyset cursor with stable timestamp plus unique tie-breaker;
- permission/organizational scope before filter/count/sort/projection;
- narrow list projection; detail only after authorization;
- no `nvarchar(max)`, message, content, JSON payload, storage key, or raw diagnostics in index keys/includes;
- no index for every filter combination and no automatic missing-index adoption.

Exact index definitions require measured query plans and DBA review; this report does not alter Document 07.

### 8.4 Retention, archive, and cleanup

- Retention is a typed governed policy reference/version, never hardcoded.
- Unpublished/failed outbox rows are retained until acknowledged/reconciled policy allows cleanup.
- Audit archive/cleanup is indexed, bounded, checkpointed, resumable, lease/hold-aware, observable, and throttles before competing with user workloads.
- Archive is encrypted, access-controlled, integrity-verified, restorable, and has a manifest; exact storage technology is not selected.
- Ordinary user/Auditor delete/update does not exist. Policy/legal-hold purge is an audited governed lifecycle, not a hidden SQL delete.
- Whether `OccurredAtUtc` or `ReceivedAtUtc` starts legal retention is `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 8.5 Performance acceptance

Measure Audit intent write latency, added transaction duration, rows/bytes per business action, outbox backlog/oldest age, dispatch throughput, ingestion batch latency, duplicate rate, query p50/p95/p99, export duration/size, index/write amplification, database/log growth, archive/cleanup throughput, restore window, CPU/RAM/storage/connection impact, and user job throughput with Audit enabled.

Numeric targets belong to Document 12 after benchmark. Audit may add bounded overhead but cannot silently drop required evidence to meet a performance target.

---

## 9. Observability

Audit-pipeline observability is operational telemetry about delivery health; it is **not another copy of the Audit payload**. Signals should use the approved Document 08 Application Insights/OpenTelemetry and structured-logging path without making either system the Audit authority. Metrics and diagnostic logs may contain only allowlisted identifiers such as Event ID, Event Type Code, Event Version, source module, environment, result/reason code, Correlation ID, count, duration, and queue age.

### 9.1 Minimum health signals

| Area | Signals to measure | Alert condition principle |
|---|---|---|
| Intent/outbox | intent-write failures, pending/failed rows, oldest unpublished age, retry count, lease recovery | required intent cannot be made durable; backlog age/count grows outside confirmed operating threshold |
| Dispatcher | active/stale lease, dispatch attempts, batch size, throughput, transient/permanent failure | no forward progress, repeated poison event, or lease repeatedly expires |
| Ingestion | batches/events accepted, duplicate count, rejected count/reason, unsupported version, latency | rejection/version block increases or accepted volume unexpectedly becomes zero |
| Offline | local queue count/bytes/capacity, oldest age, sync delay, retry/duplicate rate | queue approaches capacity, cannot decrypt/read, or remains unsynchronized beyond policy |
| Query/export | authorization denial, query latency, export queue age/failure/size | unusual denial spike, broad/slow query, failed or stalled privileged export |
| Retention/archive | eligible/held/archived/purged counts, checkpoint age, failures, verification result | hold violation, manifest/integrity failure, or cleanup/archive stalls |
| Behavior | event-rate baseline by type/outcome/environment, authentication and privileged-action failure spikes | statistically unusual spike/drop; threshold and routing require operational confirmation |

Health should expose at least `Healthy`, `Degraded`, and `Unhealthy`, with a safe reason code. Alerts must be deduplicated, severity-classified, environment-aware, and routed to an accountable owner. Exact numeric thresholds, notification routing, SIEM integration, and on-call ownership are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 9.2 Logging and recursion guard

- Diagnostic logs must not serialize an `AuditIntent`, ingestion request, event payload, export row, raw exception, or database command parameter set.
- Monitoring records identifiers and aggregate counts only; it never records password, token/secret, document content, filename/path by default, OCR text, signature data, Assistant prompt, screenshot, clipboard data, key material, or raw claims.
- Audit failures generate operational alerts. They must not recursively create unlimited Audit events about Audit-event creation failures.
- A bounded administrative action such as starting an Audit export may itself be audited once; internal reads/writes performed to persist that event are not recursively audited.
- Health telemetry has separate retention, access, and purpose from Audit Trail. Copying Audit rows into ordinary logs or tracing systems is prohibited.

---

## 10. Security testing

### 10.1 Authorization and data-boundary matrix

| Principal/context | Allowed test | Mandatory denial test |
|---|---|---|
| User | Own permitted business action produces safe Audit evidence | Query/export/update/delete Audit; read another user's evidence |
| Auditor | Read only the explicitly scoped, safe Audit projection | Update/delete; unscoped read; export without the separate permission; document-content access |
| Administrator | Audit access only when explicitly granted; governed administrative changes produce evidence | Implicit read merely because role is Administrator; document-content access without a separate content permission |
| Support permission profile | Only explicitly scoped Support/Audit projection necessary for the assigned issue | Password/token/document content, unrelated issue/user, unrestricted Audit query/export |
| Assistant | Same application authorization and same event outcome as direct UI/API invocation | Permission bypass, cross-user resource, hidden Admin action, prompt persistence |
| Worker/service identity | Append an authorized execution result for a validated contract | Human-role impersonation, arbitrary event type, different tenant/owner/resource, forged initiating actor |
| Anonymous/expired/revoked | None beyond the explicitly approved authentication-failure record | Query, count, export, ingestion, or disclosure of existence |

Every query/count/filter/sort/detail/export test must prove that authorization and resource/organizational scope are applied before result count or data projection. Direct HTTP attempts, SignalR/reconnect context where relevant, and bypass of the UI must be included.

### 10.2 Append-only, integrity, and replay tests

- Try `PUT`, `PATCH`, and `DELETE` against Audit routes; no ordinary mutation route may exist.
- Try direct SQL update/delete using each application/runtime identity; database grants must reject it. A separately governed retention principal is not exposed to the application.
- Submit the same `clientEventId`/Event ID and idempotency key repeatedly and concurrently; one semantic event is stored and duplicates return the defined idempotent outcome.
- Reuse an Event ID with different immutable data; reject and alert as an idempotency conflict, never overwrite the original.
- Forge actor, role, unit, permission, receipt time, or server-derived fields in an offline/server batch; trusted server context wins and the client value is ignored or rejected by contract.
- Tamper with batch hash/count/version, truncate a batch, reorder retries, replay after acknowledgment, and send an unsupported event version; behavior must be deterministic and observable.
- Attempt to modify or delete the source business record after the event; the Audit record remains append-only and obeys its independent retention/hold policy.
- Test archive manifest/integrity verification and restoration before any source cleanup is accepted.

### 10.3 Mandatory leakage tests

Seed every forbidden value with a unique canary and assert that it is absent from SQL Audit tables, module/server outboxes, local `audit_outbox`, API request/response, export, logs, traces, metrics dimensions, crash reports, and monitoring alerts.

| Forbidden canary | Required negative scenario |
|---|---|
| Password/passphrase | login failure, protected-file unlock/protect failure, validation exception |
| Access/refresh token, API key, secret, authorization header, cookie | authentication, deployment/runtime failure, provider call |
| Full/partial document bytes or extracted page content | Merge, OCR, conversion, secured-file action, worker crash |
| Full OCR/extracted/translated/summarized text | OCR, AI Summarizer, Translate PDF, PDF to Markdown |
| Signature/paraf image, vector, biometric, certificate data | Sign PDF success/failure; visual non-PKI remains metadata-only |
| Assistant prompt/conversation and provider raw request/response | allowed, denied, malformed, provider-failure Assistant action |
| Clipboard, screenshot, issue attachment, issue free-text message | UI/support action and exception paths |
| Filename, local path, storage key, object URL | open/process/export/retry and diagnostic paths unless an explicit approved exception exists |
| Key material, recovery secret, raw claim set, stack trace/SQL/provider response | key rotation/recovery, authorization denial, ingestion failure |

Also test CSV/spreadsheet formula injection, markup/script injection, unsafe links, excessive cells/rows, and metadata disclosure in Audit export. Export fields remain an allowlisted safe projection.

### 10.4 Abuse and resilience security tests

- Exhaust query filters, pagination bounds, date ranges, sort fields, batch count/bytes, metadata size/depth, export concurrency, and retry rate; reject early with safe codes and no resource amplification.
- Exercise cross-user, wrong-owner, expired-session, revoked-permission, offline, reauthentication-required, wrong-role, direct-API, and Assistant attempts for every important permission.
- Verify encryption and least-privilege access for local queue, backup, archive, export artifact, and transport.
- Simulate database unavailability and queue pressure during critical actions; the selected fail-closed boundary must hold without partial privileged change or disclosure.
- Attempt legal-hold bypass, premature purge, retention-policy rollback, and use of an unapproved event version.

Security-sensitive acceptance requires both technical success and the expected security/audit evidence. A green business test without the required safe Audit event is a failure.

---

## 11. Unit/integration recommendations

### 11.1 Unit tests

| Test group | Minimum assertions |
|---|---|
| Event mapping | Each application action maps to the approved typed Event Type Code, outcome, Feature ID, Process/Job/Correlation identifiers, and transaction class |
| Contract/version | Required/optional fields, canonical enum/code values, supported versions, deterministic upgrade path, unknown version rejection |
| Sensitive-field guard | Forbidden names, values, nested structures, object serializers, exception messages, and oversize metadata fail validation |
| Safe metadata | Per-event allowlist and size/depth/count constraints; no arbitrary property bag or raw object serialization |
| Authorization | Read/export/retention permissions and resource/organizational scope; role name alone never authorizes |
| Idempotency | Stable ID/fingerprint, equal replay acceptance, conflicting replay rejection, batch identity behavior |
| Time/order | Occurred/received time validation, clock-skew marking, source sequence, no claim of global order |
| Export | Fixed safe columns, escaping/encoding, formula neutralization, permission-specific projection |
| Assistant | Intent category/Feature/outcome/Correlation only; prompt and provider payload cannot map into Audit |
| Failure policy | Critical actions resolve to fail closed; routine actions require a durable intent before acknowledgment |

Property/fuzz tests should generate nested sensitive keys, Unicode/case variants, huge values, malformed identifiers, timestamps, versions, metadata, and export fields.

### 11.2 Integration and contract tests

| Scenario | Expected result |
|---|---|
| Business change plus module outbox | Both commit or both roll back; no external call or dispatcher work inside the transaction |
| Business commit, projector unavailable | Durable intent remains; retry eventually appends once |
| Audit append | Insert succeeds; ordinary update/delete fails at service and database boundary |
| Batch ingestion/replay | Per-event accepted/duplicate/rejected outcome; concurrent duplicates store once; conflicts do not overwrite |
| Crash windows | Crash before commit leaves neither change nor intent; after commit retains retryable intent; after append-before-ack replays idempotently |
| Query/filter/cursor | Stable keyset pages with no unauthorized count/data leakage; permitted filters use bounded plans |
| Retention/hold/archive | Held rows are excluded from purge; archive verifies before cleanup; interrupted batches resume safely |
| Offline reconnect | Encrypted queued events sync in bounded batches, retry with backoff, deduplicate, and retain until acknowledged |
| Export | Separate permission, reauthentication where policy requires, bounded asynchronous result, safe fields, access expiry and evidence |
| Backup/restore | Restored Audit/outbox state preserves uniqueness, append behavior, retention/hold state, and resumable dispatch |

Contract tests must cover the Document 06 batch API addendum: per-event Event Type/Version, batch identity, limits, partial result, idempotency conflict, unsupported version, authentication/revocation, and safe error codes. Until that controlled addendum exists, the test fixture documents the gap and cannot redefine the master API.

Fault injection must cover database connection loss, command timeout/deadlock, dispatcher crash, expired lease, application/worker restart, disk full/local queue full, corrupt local row, network loss, partial batch response, and archive failure.

---

## 12. E2E scenarios

| Actor and scenario | Expected Audit evidence | Required negative assertion |
|---|---|---|
| User login success/failure/logout | Typed Identity event with outcome, server actor context where known, time, app/platform, Correlation ID | No password, token, raw claim, authorization header, or stack trace |
| User runs Merge PDF locally while offline, then reconnects | Accepted/terminal Processing events use canonical Feature ID and stable client Event IDs; delayed sync deduplicates | No file name/path/content; UI job is not blocked by network; queue row is retained until acknowledged |
| User submits server OCR job | Accepted event transactionally tied to Job/outbox and one terminal result linked by Process/Job/Correlation IDs | No document/OCR text; progress notifications are not persisted as high-volume Audit noise |
| User deletes visible History | History entry follows History policy | Audit evidence is unchanged and cannot be deleted through History |
| User permanently deletes a Secured File | Reauthentication/policy check and critical deletion intent are consistent; only approved metadata/version count is recorded | No payload, storage key, signature, password, or silent deletion when Audit intent fails |
| Assistant invokes an allowed/denied tool | Same event type/outcome and authorization as direct action; safe intent category/Feature/Correlation only | No prompt, conversation, provider payload, or authorization bypass |
| Administrator changes role/permission/config/session policy | Atomic critical Administration/Security event with target identifier/class, before/after safe codes or policy version, and outcome | No implicit document/Audit content access; no success without durable evidence |
| Auditor queries Audit | Scopes and filters are evidenced only where the approved catalog requires; safe projection returned | Cannot modify/delete or see outside scope; query must not recursively audit internal reads |
| Auditor exports Audit | Separate permission, bounded async request, reauthentication when policy requires, export-created/downloaded evidence | No export through read permission alone; no unsafe column/formula/content |
| Support handles an issue | Approved issue-status/support action records safe identifiers/status only | No issue screenshot/message copied into Audit; no unrelated user or document access |
| Worker completes/fails/retries | Terminal/retry outcome links initiating actor snapshot and service identity/attempt as approved | Worker cannot forge human permission, event type, owner, or resource boundary |
| Permission revoked or session expires during a delayed action | Server final check denies where required and records the approved security outcome | Cached/offline claim cannot authorize online-only critical action |
| Duplicate/reordered offline and server delivery | One immutable semantic event; source order retained where supplied, receipt order visible | No overwrite, global-order assumption, or duplicate side effect |

Final actor-to-event and permission-to-evidence mapping remains provisional until Document 09 FINAL is supplied. The locked role decision artifacts may guide tests but are not a substitute source of truth.

---

## 13. Performance tests

| Workload | Measure | Pass principle |
|---|---|---|
| High sustained event volume plus bursts | intent latency, transaction delta, inserts/sec, log/storage growth, backlog age, CPU/RAM/IO/connections | bounded overhead, no required loss, backlog drains after burst |
| Concurrent Merge/OCR/conversion jobs | job throughput/latency with Audit path, dispatcher fairness, DB contention | heavy engine/file work remains outside DB transaction; no starvation/deadlock |
| Large Audit table | scoped actor/Feature/status/Process queries, keyset pages, count strategy, p50/p95/p99, plans/reads | stable bounded plans using justified indexes; no unauthorized pre-count |
| Outage and recovery | backlog growth, retry/backoff, connection recovery, drain rate, oldest age | no retry storm; every durable intent reaches one append or a visible poison state |
| Duplicate/replay storm | unique-constraint contention, conflict handling, response time | semantic duplicates store once without read-before-write race |
| Local offline queue near capacity | enqueue latency, encryption overhead, disk use, reconnect drain, UI response | warning/reservation policy works; no silent eviction of required events |
| Concurrent Audit exports | worker/database impact, memory, artifact size, cancellation/expiry | bounded asynchronous work; normal query/job traffic remains serviceable |
| Retention/archive/hold | batch time, locks/log growth, checkpoints, throttle, restart/resume | bounded/resumable; no held row or unverified archive removed |
| Backup restore and replay | restore duration, uniqueness, dispatcher recovery, query correctness | no duplicate/lost required evidence; post-restore append continues safely |
| Long-duration soak | leaks, connection/lease accumulation, index fragmentation/growth, backlog drift | steady state without increasing age/resources or silent failure |

Benchmark the same representative business workload with the production Audit path enabled and a controlled test-harness baseline; there must be no product setting that disables required Audit to achieve a target. Collect only identifiers and aggregate timings—never document or Audit payload. Numeric SLOs, batch sizes, queue limits, export limits, retention windows, and recovery objectives belong to Document 12 and `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

## 14. Implementation sequencing

### 14.1 Delivery order

| Phase | Scope and exit gate |
|---|---|
| **P0 FOUNDATION** | Obtain Document 09 FINAL; approve event taxonomy/catalog, Event Type/Version contract, transaction-class catalog, forbidden-field guard, abstraction boundaries, API batch addendum, module-owned outbox ownership, migration/version ownership, permission vocabulary, and automated contract/unit test harness. Exit: no feature invents an event/string/field locally. |
| **P1 CORE AUDIT** | Implement existing-database Audit append store, module/server outbox dispatch, authenticated/idempotent ingestion, encrypted local `audit_outbox`, batch acknowledgment/retry, basic authorized query, health metrics and safe alerts. Exit: crash/replay/failure tests prove durable once-only semantic append. |
| **P2 FEATURE AUDIT** | Map identity, Processing/33 Primary Tools, Jobs, Secured Files, Support, Worker, and Assistant actions to approved events. Exit: representative E2E allow/deny/offline/server scenarios pass and forbidden canaries are absent everywhere. |
| **P3 ADMIN/REPORTING** | Add role/permission/config/session/key lifecycle evidence, governed retention/hold/archive, scoped query, separate bounded export, export lifecycle evidence. Exit: authorization, append-only, hold, export security, and restore tests pass. |
| **P4 HARDENING** | Fault injection, negative/fuzz tests, performance/load/soak, archive/cleanup and backup/restore validation, threat-model review, VA/PT readiness, runbook/handover evidence. Exit: confirmed thresholds and no unresolved high-risk finding. |

### 14.2 Dependency and ownership gates

| Dependency | Required input/output |
|---|---|
| API / Document 06 | Controlled addendum for typed/versioned per-event batch contract, limits, result/conflict/version behavior; no new endpoint merely for a UI feature |
| Database / Document 07 | Existing SQL/SQLite stores, append grants, unique keys/indexes, module-owned outboxes, migration order, retention/hold/archive mechanics; no additional database |
| Security / Document 08 | secret/config/key boundary, encryption, scanning, monitoring/alert ownership, backup/restore controls and test evidence |
| Role Matrix / Document 09 | FINAL permissions for Audit read/export/retention/hold and administrative actions; actor/resource scopes and reauthentication rules |
| Testing / Documents 08/12 | automated layers, representative datasets, fault/security/performance environments, numeric acceptance thresholds and evidence retention |

Each phase must produce: approved contract/catalog revision, migrations/configuration as applicable, automated tests, threat/risk update, monitoring evidence, and handover notes. This sequence is implementation readiness only; it does not authorize production coding in this task.

---

## 15. Risks/conflicts

| ID | Risk/conflict | Impact | Required disposition |
|---|---|---|---|
| `D10-D-001` | Document 09 FINAL is absent; only a decision register/integration audit and agent outputs are present | Final permission/event mapping cannot be asserted from the requested source set | **Integrator:** provide/approve Document 09 FINAL before acceptance; do not silently elevate non-FINAL artifacts |
| `D10-D-002` | Document 06 offline batch example has a batch `contractVersion`, but no explicit per-event `eventType` and `eventVersion` | Cannot satisfy a typed/versioned Audit event or deterministic evolution test | **Integrator/API owner:** controlled Document 06 addendum/revision; preserve endpoint and canonical IDs |
| `D10-D-003` | Batch identity, per-event partial result/conflict semantics, source sequence, and trust rules need precise contract | Ambiguous replay/ordering/error recovery | Confirm request/header/body and response schema without adding an endpoint/database/vendor |
| `D10-D-004` | Actor role/permission may change between occurrence and delayed ingestion | Incorrect historical actor attribution if current role is substituted | Persist a safe client occurrence snapshot only where approved; server records trusted receipt context and provenance separately |
| `D10-D-005` | Identity/Admin/Config critical changes need an owner transaction plus outbox, but all owner outboxes are not explicitly enumerated | Cross-schema writes or non-atomic evidence | Document 07-controlled ownership addendum/migration if absent; never use a global ownerless outbox |
| `D10-D-006` | Fail closed for login, privileged changes/deletion, and export can reduce availability | Material business/operational trade-off | **Integrator/Security owner:** approve action-level matrix and degraded-mode procedure; never downgrade silently |
| `D10-D-007` | Local queue full or corrupt while offline | Required evidence versus continued local work conflict | Confirm reserved capacity, warning/block boundary, recovery/export support, and maximum offline policy |
| `D10-D-008` | Append-only evidence conflicts with retention purge, legal hold, privacy deletion, and backup copies if authority is unclear | Unauthorized deletion or indefinite retention | Define governed retention executor, hold precedence, purge evidence, archive/backup scope, and authority |
| `D10-D-009` | `OccurredAtUtc` versus `ReceivedAtUtc` as legal retention start is unresolved | Different deletion dates for offline/delayed events | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `D10-D-010` | Auditing Audit queries/exports can recurse or generate excessive events | Loop, noise, storage and latency | Catalog only human boundary actions; do not audit internal persistence/query operations recursively |
| `D10-D-011` | Treating progress/navigation/telemetry as Audit | Volume explosion and reduced evidentiary value | Audit accepted/terminal/security/business outcomes only; keep observability separate |
| `D10-D-012` | Auditor/Admin/Support scopes and `FEAT-AUDIT-*` visibility are not yet backed by Document 09 FINAL | Cross-user/overprivileged access risk | Finalize explicit permissions/scopes; Administrator and Support never gain implicit access |
| `D10-D-013` | SQL polling/outbox backlog sizing has no confirmed thresholds | Delayed evidence or database pressure | Measure under Document 12 workloads; no external broker selection in this document |
| `D10-D-014` | `SafeMetadataJson` can become an ungoverned free-form escape hatch | Sensitive-data leakage/schema drift | Event-specific allowlist, size/depth limit, validator, version and negative canary tests |
| `D10-D-015` | Audit export can expose broad data or execute spreadsheet formulas | Confidentiality/client-side execution risk | Separate permission, reauthentication policy, bounded scope, fixed safe projection, formula neutralization, secure artifact lifecycle |
| `D10-D-016` | Monitoring/SIEM may copy full Audit payload | Creates a second uncontrolled sensitive store | Identifiers/aggregates only; payload routing prohibited; exact SIEM route requires confirmation |
| `D10-D-017` | Worker retry/attempt/service identity versus initiating human identity can be conflated | Misleading accountability | Preserve both safe initiating-actor snapshot/provenance and execution service/attempt fields where approved |
| `D10-D-018` | Archive/restore or backup restore can reintroduce duplicates or lose hold/outbox state | Integrity and recovery failure | Manifest/hash verification, uniqueness preservation, hold restoration, replay and post-restore append tests |
| `D10-D-019` | Retention windows, queue/batch/export limits, alert thresholds, RPO/RTO, archive medium, SIEM route are undecided | Cannot set production-safe numeric gates | Document 12/Pertamina confirmation; keep values configurable and governed, not guessed |

No conflict found that justifies an external broker, a second database, a new vendor, a new role, direct cross-schema writes, or a change to a canonical Feature ID.

---

## 16. Suggested Document 10 text

> ### Audit implementation boundary
>
> Audit Trail shall be implemented through a shared, typed abstraction: Application Action → versioned Audit Event Contract → Audit Service → durable module-owned outbox/local audit outbox → append-oriented Audit storage. Feature code shall not write directly to the Audit table. Audit Trail is separate from User History, Activity Center, Support content, logs, metrics, traces, and business progress.
>
> The event catalog shall define Event Type Code, Event Version, required fields, allowed metadata, transaction class, permission scope, and retention class. Events shall use stable Event IDs and approved canonical Feature IDs. The minimum safe envelope includes occurrence/receipt time, trusted actor context and provenance, action/outcome, Feature/Process/Job/Correlation identifiers where applicable, execution location, platform/app version, safe size/error codes, and versioned allowlisted metadata. Free-form payload serialization is prohibited.
>
> Password/passphrase, token/secret/key material, document content, filename/path by default, clipboard, screenshot/issue attachment, OCR/extracted/generated text, signature data, Assistant prompt/conversation, raw claim set, raw exception/SQL/provider request or response, and storage URL/key shall not be stored in Audit, outbox, export, or audit-pipeline telemetry.
>
> ### Consistency and failure policy
>
> A critical identity, permission, role, security-policy, permanent-deletion, key-lifecycle, or privileged-export action shall not commit or disclose its result unless its Audit intent is durable in the same owner transaction or at an approved durable-before-disclosure boundary. Failure is fail closed. Routine processing, local tool execution, Support workflow, and worker results may complete only after a durable owner/local intent exists; projection to central Audit may retry asynchronously. Telemetry failure shall not be treated as Audit success or failure.
>
> Delivery shall be at least once and idempotent. Stable event/client IDs, unique constraints, batch identity, conflict detection, bounded retries with backoff, leases, poison-event visibility, and acknowledgment-based cleanup shall prevent duplicate semantic evidence. An equal replay is accepted idempotently; the same ID with different immutable data is rejected and alerted. Critical Audit events shall never be silently dropped.
>
> ### Offline, ordering, and recovery
>
> Offline events shall be validated and written to an encrypted, metadata-only local `audit_outbox` before success is acknowledged when Audit is required. The queue retains rows until server acknowledgment, synchronizes in bounded batches after reconnect, and records safe failure state. A full/corrupt queue shall follow an approved action-level block/degraded policy; silent eviction is prohibited. Source sequence, `OccurredAtUtc`, and server `ReceivedAtUtc` are retained where approved. No global event order is inferred from arrival time.
>
> Crash before the owner transaction commits leaves neither business change nor intent. Crash after commit leaves a retryable intent. Crash after append but before acknowledgment is recovered by idempotent replay. The same principles apply to application, worker, dispatcher, database, network, and server restarts.
>
> ### Performance and storage
>
> Audit transactions shall remain short and shall not include PDF engine, file/object storage, AI/provider, network, or export work. Dispatch and ingestion use bounded batches, bounded concurrency, connection reuse, and retry backoff. Query uses authorization-first keyset pagination and only measured indexes for actor/time, Feature/time, status/time, Process ID, and justified Job/Correlation lookups. Archive and cleanup are bounded, resumable, hold-aware, observable, throttled, and verify archive integrity before governed cleanup.
>
> Numeric latency, throughput, batch, queue, export, retention, archive, alert, and recovery thresholds shall be confirmed through Document 12 and applicable Pertamina standards. Required Audit shall not be disabled to meet a performance target.
>
> ### Observability and authorization
>
> The Audit pipeline shall expose safe health metrics for write failures, pending/failed backlog and oldest age, retries, ingestion acceptance/duplicate/rejection, unsupported versions, offline capacity/sync delay, query/export latency, archive failures, and unusual event-rate changes. Logs and alerts contain identifiers, counts, durations, and safe reason codes only; they shall not copy the Audit payload. Audit-pipeline monitoring shall include a recursion guard.
>
> Authorization is enforced by the application/API and database boundaries before Audit count, query, detail, export, retention, or hold behavior. Auditor read, Audit export, and retention administration are separate permissions. User, Auditor, Administrator, and Support identities cannot update/delete Audit through ordinary paths. Administrator or Support status grants no implicit Audit or document-content access. The Assistant receives the same authorization and Audit behavior as the equivalent UI/API action.
>
> ### Verification and delivery
>
> Acceptance shall include unit, contract, integration, authorization, negative/abuse, offline/reconnect, E2E, performance/DoS, backup/restore, and VA/PT-readiness tests as applicable. Tests shall prove typed/versioned mapping, transaction consistency, append behavior, replay/deduplication, retention/hold, query/export scope, failure recovery, and absence of every forbidden field across storage, transport, export, logs, traces, and alerts. A security-sensitive business test is not accepted unless its required safe Audit evidence is also correct.
>
> Delivery is sequenced as P0 Foundation, P1 Core Audit, P2 Feature Audit, P3 Admin/Reporting, and P4 Hardening. Document 09 FINAL and a controlled Document 06 Audit batch-contract addendum are prerequisites for final permission/event acceptance. Exact retention, legal-hold, export, alert/SIEM, queue-capacity, and fail-closed operational policies require Integrator and applicable Pertamina confirmation.

---

**Integrator handoff:** do not merge the suggested text as final until Document 09 FINAL is available and findings `D10-D-001` through `D10-D-019` are dispositioned by the responsible document owners. This report does not edit Documents 01–10, create production code, select a vendor/broker, add a database, create a role, or alter a canonical Feature ID.
