# AGENT B — Audit Data Model, Integrity & Retention Report

**Task ID:** DOC10-AGENT-B  
**Scope:** SQL Server and SQLite audit evidence model, append/write boundary, integrity, deduplication, offline ingestion, indexing, retention, archive, purge, and server budget  
**Sources reviewed:** Documents 01–08 FINAL; Document 09 locked decision register, integration audit, and specialist matrix artifacts available in the workspace  
**Status:** recommendation for Document 10; no production code and no change to the Document 10 master

This design preserves the locked Document 07 boundary: Enterprise Audit is metadata-only in the existing SQL Server database under schema `audit`; offline enterprise-audit metadata uses the protected local SQLite outbox; History, observability, Support, and Audit remain separate data planes. No database, vendor, or event-store product is added.

## 1. Audit data model

### 1.1 Minimal logical model

Initial deployment needs three already-defined core tables:

| Table | Purpose | Invariant |
|---|---|---|
| `audit.AuditEvents` | Authoritative active enterprise evidence | Stable source/server Event ID unique; append-only; no ordinary update/delete |
| `audit.IngestionBatches` | Idempotent offline/client batch ingestion and acknowledgement | Unique batch in source scope; event counts, request fingerprint, receive/outcome metadata; no raw payload copy |
| `audit.ExportRequests` | Bounded asynchronous audit export | Safe versioned filter, requester/scope, job/artifact lifecycle, current authorization on retrieval |

Two small lifecycle tables are justified only when the corresponding process is enabled:

| Conditional table | Why it exists | Not used for |
|---|---|---|
| `audit.ArchiveManifests` | Proves a verified archive range/count/schema/digest and holds an opaque archive reference before active-row purge | Storing audit event bodies twice in SQL |
| `audit.RetentionRuns` | Controls/checkpoints an approved archive/purge run and records counts/outcome | Editing or deleting individual Audit Events through a user UI |

Retention definitions remain typed governed configuration owned by the configuration/policy boundary. Audit events reference retention class/policy version; Auditor cannot change retention. A legal-hold table or scope model is **conditional**, not baseline, until Pertamina confirms legal/policy-hold requirements.

### 1.2 Recommended `audit.AuditEvents`

| Field | Recommended type/shape | Null? | Purpose and rule |
|---|---|:---:|---|
| `AuditEventPk` | `bigint` identity/sequence-compatible internal key | No | Clustered primary key and stable tie-breaker; never exposed as authorization proof |
| `AuditEventId` | Source/server-generated UUID/GUID or the approved opaque public-ID type | No | Generated once when the logical event is created and reused on every retry; unique nonclustered constraint; not clustered |
| `IngestionBatchPk` | Internal FK to `audit.IngestionBatches` | Yes | Populated only for batch/client ingestion; direct server event may be null |
| `EventTypeCode` | Bounded ASCII controlled code | No | Exact versioned semantic event, for example a defined login failure or Feature completion; unknown values rejected |
| `EventVersion` | Small positive integer | No | Schema/meaning version for this event type; historical interpretation never depends on today’s DTO |
| `CategoryCode` | Bounded ASCII controlled code | No | Small governed grouping such as identity, feature, secured-file, admin, audit-access, support, or security |
| `OccurredAtUtc` | `datetime2` UTC | No | Original business/security event time, including offline time; client value is validated but not treated as trusted server time |
| `ReceivedAtUtc` | `datetime2` UTC, server assigned | No | First successful server receipt/append time; never supplied by the client |
| `ActorTypeCode` | Controlled code | No | Human principal, workload, system, or pre-auth/unknown actor category |
| `ActorId` | Canonical bounded opaque principal/workload identifier | Yes | Server derives authenticated human/workload identity; null only for approved pre-auth/system cases |
| `ActorUnitCode` | Approved bounded historical unit/scope snapshot | Yes | Supports required unit-scoped evidence without resolving current organization; never accepts arbitrary client text |
| `ActorContextCode` | Controlled context such as User/Admin/Auditor/Support/Workload/System | Yes | Captures effective action context, not a raw claim/permission dump and not a new role |
| `ResourceTypeCode` | Controlled resource code | Yes | Job, result, secured file/version, issue, policy, role assignment, audit export, etc. |
| `ResourceId` | Canonical bounded opaque ID | Yes | Logical reference only; no cross-module FK and no permission to referenced content |
| `FeatureId` | Canonical Feature ID type | Yes | Present for Feature-related events; validated against deployed Feature contract |
| `ProcessId` | Canonical bounded Process ID | Yes | Required by existing audit query requirements when a process exists |
| `JobId` | Canonical bounded Job ID | Yes | Logical link for server/background processing; not an access proof |
| `CorrelationId` | Canonical bounded Correlation ID | Yes | Cross-component diagnostics; confidential metadata and never a deduplication key |
| `SourceChannelCode` | Controlled code | No | Desktop, mobile, web, API, worker, scheduled/control-plane source |
| `PlatformCode` | Controlled code | Yes | Safe normalized platform family, not unrestricted user-agent/device fingerprint |
| `AppVersion` | Bounded normalized version | Yes | Source application/service version used for compatibility and investigation |
| `ExecutionLocationCode` | Controlled `LOCAL`/approved server/worker code | Yes | Required local/server distinction; does not name infrastructure topology |
| `OutcomeCode` | Controlled code | No | Success, denied, failed, cancelled, partial, or other approved outcome semantics |
| `ReasonCode` | Bounded allowlisted code | Yes | Safe denial/error/policy reason; never exception/message/free text |
| `InputSizeBytes` / `OutputSizeBytes` | Non-negative `bigint` or approved size-class alternative | Yes | Required process-size evidence where applicable; not indexed by default |
| `RetentionClassCode` | `SHORT`, `STANDARD`, `EXTENDED`, or `POLICY_DEFINED` | No | Classification assigned from event catalog/policy, never freely selected by client |
| `RetentionPolicyVersion` | Bounded policy version/reference | No | Records which classification rule applied; purge still uses current approved legal policy |
| `SafeMetadataJson` | Bounded `nvarchar(N)` plus JSON validity/schema check | Yes | Only event-type/version allowlisted structured metadata; not `nvarchar(max)`, not a generic property bag |
| `EventFingerprint` | Fixed/bounded binary digest plus version/algorithm code | No | Server-computed canonical fingerprint for duplicate-ID conflict detection; not a cryptographic chain |

Use the existing shared canonical ID types/lengths instead of inventing incompatible widths. Code fields are ASCII, bounded, and constrained by an application/database catalog. `SafeMetadataJson` length is selected from the approved event catalog and measured payload, not guessed as unlimited.

### 1.3 Fields evaluated and deliberately omitted

| Candidate | Decision | Rationale |
|---|---|---|
| `Action` | Do not add as a free-text field. Derive the normalized action from versioned `EventTypeCode`; add a controlled `ActionCode` later only if an approved cross-category query proves it necessary | Avoid semantic duplication/inconsistency |
| `CreatedAtUtc` | Omit from `AuditEvents` | `ReceivedAtUtc` already represents the authoritative first server insert/receipt time; a second creation timestamp is ambiguous |
| Raw role/permission claims | Omit | Store only approved context/scope/version metadata; raw claims grow, leak, and become misleading after policy changes |
| Filename/full path/storage key | Omit by default | Confidential metadata and unnecessary for normal evidence; resource IDs enable authorized correlation |
| Message/description/exception text | Omit | Encourages secrets/content/control-character leakage; use codes |
| Document/OCR/prompt/signature/clipboard payload | Prohibited | Violates Documents 07–09 security and data-plane boundaries |
| Per-row mutable `PurgeAfterUtc` | Omit initially | Causes mass updates when policy changes; derive eligible key ranges from retention class, policy, hold, and chosen timestamp basis |
| Hash-chain predecessor/hash | Omit from baseline | No approved compliance requirement for cryptographic chaining; permissions, append path, backups, per-event fingerprint, and archive digest are proportionate initially |

### 1.4 `audit.IngestionBatches`

Minimum fields:

```text
IngestionBatchPk
IngestionBatchId                  source-generated, stable across retry
SourceInstanceId                 protected opaque app/profile/device instance
AuthenticatedActorId             server derived
RequestFingerprint + FingerprintVersion
DeclaredEventCount
AcceptedCount / DuplicateCount / RejectedCount
FirstOccurredAtUtc / LastOccurredAtUtc
ReceivedAtUtc / CompletedAtUtc
StatusCode / SafeFailureCode
RowVersion
```

Unique boundary is `SourceInstanceId + IngestionBatchId`, or globally unique `IngestionBatchId` if the approved ID contract guarantees global uniqueness. The table never stores the raw HTTP body, document content, event JSON copy, token, or exception.

### 1.5 Export, archive, and retention metadata

- `audit.ExportRequests` stores request ID, requester, authorized scope snapshot/reference, versioned bounded filter, status, Job ID, safe artifact reference, expiry, timestamps, and row version. Audit export bytes remain in approved secure object storage and require current `audit.export.read` authorization.
- `audit.ArchiveManifests` stores Archive ID, schema/export format version, covered event key/time range, event count, retention classes/categories, archive digest and algorithm version, encrypted opaque object reference, status, verification time, and created/completed time. It contains no event body.
- `audit.RetentionRuns` stores run ID, authorized policy/version, mode (`ARCHIVE`, `PURGE_ACTIVE`, `PURGE_ARCHIVE`), requested/approved actor references, cutoff basis/value, checkpoint key, candidate/archived/purged/failure counts, state, timestamps, and safe failure code. Mutable operational state uses row version; completed evidence is not rewritten.

## 2. SQL ownership

The existing SQL Server database retains one module-owned schema, `audit`. Desktop, mobile, web browser, User, Administrator, Auditor, and Support never connect directly to SQL Server.

| SQL/application identity | Minimum authority | Explicit deny/boundary |
|---|---|---|
| Audit ingestion runtime | Insert approved `AuditEvents`; create/finalize its ingestion batches through controlled application path | No update/delete of Audit Events, no DDL, no unrestricted export |
| Audit query runtime | Read only authorized safe projections through scoped query service | No insert/update/delete, no content traversal through referenced IDs |
| Audit export worker | Read bounded authorized event scope; update its own export request; write approved export artifact | No arbitrary query, policy mutation, Audit Event mutation, or durable public URL |
| Retention/archive worker | Claim approved runs, read eligible ranges, create/verify manifest, execute controlled bounded purge | No ad-hoc row deletion, no policy definition/approval, no bypass of hold |
| Migration identity | Controlled schema/catalog/index DDL during approved deployment | No routine application use; no human browsing authority |
| Backup/DBA operator | Platform backup/restore and operational duties | Does not automatically receive business permission to browse evidence or referenced content |

Recommended SQL controls:

1. Deny ordinary runtime `UPDATE` and `DELETE` on `audit.AuditEvents`; grant insertion only through the ingestion application role/procedure boundary.
2. Permit physical delete only to the retention worker through a narrowly controlled signed procedure/application path after approved run/policy/hold validation. Do not grant table-wide delete to Administrator or Auditor.
3. Separate schema owner/migration authority from runtime identities; no runtime `db_owner` or shared human credential.
4. Capture DDL, permission changes, direct DML attempts, archive/purge execution, backup/restore, and privileged database access through approved SQL/platform auditing without logging event payloads.
5. Keep resource identifiers as logical references; no cross-module FK from Audit to jobs/identity/secured/support. A referenced ID never grants content access.
6. Apply encrypted, certificate-validated connections, approved database/backup encryption, network restriction, credential rotation, and restore-access audit from Documents 07–08.

Application authorization remains primary for Auditor scope. SQL row-level security is not required for the initial design unless the approved deployment specifically chooses it and proves compatibility with exports, support operations, and query plans.

## 3. EF Core boundary

Use one module-owned `AuditDbContext` and migration history within the existing SQL Server database. It must not share entity navigation graphs or a generic repository with Identity, Jobs, Support, History, or Secured Files.

1. `AuditEvent` is modeled as immutable after construction. Ingestion uses `Add`/bounded `AddRange`; no business method or generic repository exposes update/delete.
2. Queries use narrow DTO projections and `AsNoTracking`. Never materialize `SafeMetadataJson` or other wide fields for default lists unless requested and authorized.
3. A SaveChanges guard/interceptor rejects `Modified` or `Deleted` state for `AuditEvent`. This is defense in depth; SQL permission is the final protection.
4. `IngestionBatch`, `ExportRequest`, `ArchiveManifest`, and `RetentionRun` have explicit state machines and optimistic concurrency/row version where they mutate. `AuditEvent` does not need row version because it never mutates.
5. Event DTO validation is closed and versioned: unknown event types/versions/fields, extra JSON keys, unsafe codes, invalid timestamps, negative sizes, or oversized metadata fail before entity creation.
6. Server overwrites client-supplied actor, received time, source identity, authorization context, and policy-derived retention fields with authoritative values.
7. Migrations are additive where possible. Historical event meaning is retained by `EventTypeCode + EventVersion`; a deployment never rewrites old events merely to match a new DTO.
8. Archive/purge uses an explicit controlled bulk path and small committed batches, not entity-by-entity tracking or a generic `RemoveRange` reachable from business code.
9. Query tags/diagnostics and Query Store use safe operation names; SQL parameters containing confidential IDs/metadata are not copied into unrestricted logs.

## 4. Append/write model

### 4.1 Server-originated events

Each owning module persists a safe audit intent in its **domain-owned transactional outbox** in the same short transaction as the business mutation. An audit publisher later submits that intent to the Audit ingestion application, which validates the event catalog and appends idempotently.

```text
authorize action
→ business mutation + domain outbox in one short transaction
→ commit
→ publisher sends stable AuditEventId and safe event fields
→ Audit ingester validates/derives authoritative fields
→ append AuditEvent or acknowledge duplicate
→ acknowledge publisher
→ owning outbox cleanup after acknowledged policy
```

This avoids a distributed transaction and prevents “business change committed but no durable audit intent.” Privileged and irreversible actions must fail before mutation if their owning transaction cannot persist the mandatory audit intent.

### 4.2 Client/offline-originated events

Desktop/mobile creates `AuditEventId` once at the original event time, commits the safe event to protected SQLite `audit_outbox`, and later sends bounded batches. The server validates the originating profile binding against the authenticated synchronization context, resolves authoritative actor/context, stamps received time, and appends accepted events in a short transaction.

### 4.3 Append semantics

- Events are facts, not current-state rows. Correction, reversal, restore, retry, authorization change, or administrative intervention creates a new event referencing the same resource; it does not edit the earlier event.
- One business action may legitimately produce several event types. Therefore Correlation ID, Event Type, Actor, Resource, and timestamp combinations are not unique.
- Audit Event insertion and ingestion-batch acknowledgement occur atomically for the accepted set. The server acknowledges only after commit.
- Batch partial acceptance is allowed only with an explicit per-event result: `ACCEPTED`, `DUPLICATE`, or `REJECTED` plus safe reason. The client never interprets a transport timeout as rejection.
- High-frequency progress/log/telemetry is not copied into Enterprise Audit. The approved event catalog records durable business/security milestones, decisions, and terminal outcomes.
- Audit writes do not hold a SQL connection while file streaming, engine/provider processing, queue wait, SignalR delivery, archive object transfer, or retry delay occurs.

## 5. Integrity controls

### 5.1 Baseline integrity model

Integrity is provided by several small, complementary controls:

1. A versioned event catalog defines allowed Event Type, Category, required fields, safe metadata schema, and retention class.
2. The ingestion service derives authoritative fields such as Received At, authenticated Actor ID, effective actor context, and policy version. A client cannot assert these fields as authoritative.
3. SQL permissions allow the runtime ingestion identity to INSERT but not UPDATE or DELETE Audit Events.
4. The EF Core audit model is append-only and rejects accidental modification or removal before SQL execution.
5. Database constraints enforce required codes, valid UTC values, non-negative bounded sizes, and unique Audit Event ID.
6. Stable source IDs, a canonical event fingerprint, and unique constraints protect against retry duplication and ID reuse.
7. The business transaction or protected local outbox durably records the audit intent before asynchronous delivery.
8. Backup/restore validation, ingestion reconciliation, retention-run records, and archive manifests protect availability and lifecycle integrity.

The application runtime cannot override these controls through an ordinary user or Administrator request. A DBA can still alter the database; therefore this model is append-oriented and highly controlled, but must not be described as cryptographically tamper-proof against every infrastructure administrator.

### 5.2 Row immutability and administrative operations

- Ordinary APIs expose no update or delete operation for Audit Events.
- Audit readers, exporters, Administrators, and Auditors receive no table write permission.
- Policy purge uses a separate service identity and approved stored procedure or narrowly controlled command path. It deletes only policy-eligible key ranges and cannot accept arbitrary user predicates.
- Schema correction uses the migration identity and change process, not the ingestion identity.
- Audit read, export, retention-policy change, legal-hold change if adopted, archive access, and purge execution create their own safe audit events.
- Direct production table writes and failed privileged attempts are monitored using existing SQL Server/platform controls.

An UPDATE/DELETE-blocking trigger is not recommended for the initial deployment: DB denial plus separated identities is clearer and does not create a hidden bypass for migration or purge. Add a defense-in-depth trigger only if Pertamina database standards require it and the controlled bypass can be independently governed and audited.

### 5.3 Fingerprints and cryptographic chaining

The server calculates Event Fingerprint from a documented canonical representation of the producer-stable logical event fields after normalization. The representation excludes Audit Event PK, Ingestion Batch PK, Received At, retry counters, and any other value that can legitimately change between retries. The fingerprint supports duplicate comparison, reconciliation, export/archive verification, and corruption diagnostics. It is not a digital signature, a chain, proof of who performed the action, or protection against a privileged actor who can rewrite both row and fingerprint.

Cryptographic row chaining, external timestamping, WORM storage, and cross-system anchoring are not baseline requirements. They add ordering, key custody, restore, archive, concurrency, and purge complexity. Introduce one only after a concrete legal/compliance threat model identifies the evidence requirement.

**REQUIRES_PERTAMINA_STANDARD_CONFIRMATION:** whether audit records require independently verifiable tamper evidence beyond SQL access control, backup controls, reconciliation, and archive digest. This is a material Integrator decision.

## 6. Deduplication

### 6.1 Event identity

- The producer generates one stable Audit Event ID when the event is first durably recorded. Every retry reuses it.
- A server-originated domain outbox also persists the stable Audit Event ID; a publisher never generates a new ID per attempt.
- Audit Event ID has a unique SQL constraint and is the authoritative event-level deduplication key.
- Correlation ID, Job ID, Process ID, Event Type, Actor ID, Resource ID, and timestamps are searchable relationships, not deduplication keys. One correlation can legitimately contain multiple events.

### 6.2 Duplicate decision

| Condition | Server decision | Client-visible result |
|---|---|---|
| New Audit Event ID and valid event | Insert once | ACCEPTED with Audit Event ID |
| Existing ID and same canonical fingerprint | Do not insert | DUPLICATE with existing Audit Event ID |
| Existing ID and different canonical fingerprint | Reject; do not overwrite either representation | HTTP 409 or per-item EVENT_ID_REUSE_CONFLICT |
| Invalid event/metadata/catalog version | Do not insert | REJECTED with safe stable reason code |
| Race between identical retries | Unique constraint selects the winner; loser is re-read and compared | ACCEPTED for winner, DUPLICATE for replay |

The ID-reuse conflict is a security/integrity signal and must not be “fixed” by assigning a replacement ID on the server.

### 6.3 Batch idempotency

Each source instance creates a stable Ingestion Batch ID and request fingerprint. A unique constraint on the approved source scope plus Batch ID ensures:

- a byte/logically equivalent replay returns the original batch disposition;
- the same Batch ID with a different request fingerprint returns conflict;
- per-event results remain authoritative if a batch contains a mixture of new, duplicate, and rejected events.

A source instance is an opaque installation/profile identifier bound by the server to the authenticated account and approved application registration. It is not a hardware fingerprint.

Deduplication is guaranteed for the supported late-delivery/replay window. Before an Audit Event is finally purged, that window must already have elapsed. If policy requires accepting retries after the event retention boundary, retain a compact dedup key for the longer window; do not create that extra table until this need is confirmed.

## 7. Offline queue and synchronization

### 7.1 Minimal SQLite outbox

The local protected audit outbox stores only what is necessary for eventual append:

| Field | Purpose |
|---|---|
| AuditEventPk | Local sequential key |
| AuditEventId | Stable globally unique source event ID; local unique constraint |
| EventTypeCode, EventVersion, CategoryCode | Versioned server validation |
| OccurredAtUtc | Original event time |
| FeatureId, ProcessId, JobId, CorrelationId | Optional safe linkage |
| ResourceType, ResourceId | Optional opaque resource reference; never a local path |
| OutcomeCode, ReasonCode | Controlled result codes |
| SafePayloadJson | Bounded, versioned, allowlisted metadata only |
| SourceProfileBindingId | Protected opaque originating account/profile binding; not a token or claims |
| State | PENDING, IN_FLIGHT, ACKNOWLEDGED, or REJECTED |
| AttemptCount, NextAttemptAtUtc, LastSafeErrorCode | Retry control without raw exceptions |
| IngestionBatchId | Stable current batch identity when assigned |
| CreatedAtUtc, AcknowledgedAtUtc | Local lifecycle control |

The queue is isolated to the account/profile that originated the event and carries a protected opaque binding. The server verifies that binding against the authenticated synchronization context and resolves Actor ID/context from authoritative identity data; a queue is never sent under a different account merely because that account is currently signed in. Received At, retention policy version, and other server-authoritative facts are also added by the server. The local queue stores no raw claims or tokens.

SQLite database, WAL, and SHM files must inherit profile-private permissions and required local encryption. They are excluded from automatic consumer-cloud backup where the platform permits. One writer owns queue transitions; account/profile switching cannot expose or transmit another profile's queue.

### 7.2 Synchronization flow

    auditable action
    → commit safe event to local audit_outbox
    → bounded batch selected by count and encoded bytes
    → mark/lease batch IN_FLIGHT
    → authenticated idempotent submit
    → server validates source, catalog, policy, and each item
    → server commits accepted events and batch disposition
    → explicit per-item acknowledgement
    → client commits ACKNOWLEDGED/REJECTED state
    → acknowledged rows removed by local cleanup policy

- Transport timeout, process crash, and lost acknowledgement cause replay with the same event and batch IDs.
- Transient failures use exponential backoff with jitter and a server Retry-After hint; no busy loop occurs.
- A rejected “poison” event remains protected with a safe reason and does not hot-loop. Recovery can occur through a compatible application update or an authorized diagnostic workflow; the event is not silently discarded or edited.
- Queue limits are configurable by row count, encoded bytes, and oldest unacknowledged age. Diagnostic events may be sampled before Enterprise Audit events, but mandatory audit events are never evicted oldest-first.
- If mandatory audit intent cannot be durably queued, a new auditable destructive/privileged mutation must not proceed. Existing local source files, drafts, and recovery state remain available, and the UI shows an actionable audit-recovery state.
- Background synchronization must not freeze the UI. Backlog, last successful acknowledgement, and a safe error code remain visible to operational support.

**REQUIRES_PERTAMINA_STANDARD_CONFIRMATION:** exact queue limits, acceptable late-arrival window, clock-skew tolerance, and which ordinary non-privileged actions may continue when the mandatory audit queue is full or unavailable.

## 8. Index strategy

### 8.1 Initial indexes

Start with narrow indexes that satisfy the documented primary paths:

| Table | Key | Purpose |
|---|---|---|
| AuditEvents | Clustered AuditEventPk | Sequential append and stable tie-breaker |
| AuditEvents | Unique AuditEventId | Idempotency and direct reconciliation |
| AuditEvents | ActorId, OccurredAtUtc DESC, AuditEventPk DESC | Actor audit timeline |
| AuditEvents | FeatureId, OccurredAtUtc DESC, AuditEventPk DESC; filtered when null | Feature audit timeline |
| AuditEvents | OutcomeCode, OccurredAtUtc DESC, AuditEventPk DESC | Outcome/status investigation |
| AuditEvents | ProcessId, AuditEventPk; filtered when null | Process trace and requirement DR-DB-006 |
| IngestionBatches | SourceScopeId, IngestionBatchId unique | Batch idempotency |
| IngestionBatches | StatusCode, ReceivedAtUtc, IngestionBatchPk | Recovery and bounded cleanup |
| ExportRequests | RequesterActorId, StatusCode, CreatedAtUtc, ExportRequestPk | User/admin export list and cleanup |

If authorization scope is primarily organizational unit, add or prefer an index beginning with Actor Unit Code followed by Occurred At and Audit Event PK. The final choice must match the actual scope predicate, not a guessed organization hierarchy.

### 8.2 Conditional indexes

Add only after Query Store/approved metrics show a frequent selective route:

- Category Code or Event Type Code plus Occurred At;
- Resource Type, Resource ID, Occurred At;
- Job ID or Correlation ID;
- Received At for ingestion-latency operations;
- an index optimized for non-success outcomes if a filtered predicate is stable and supported.

Do not index Safe Metadata JSON, every foreign identifier, every filter combination, or large display columns. Do not add broad INCLUDE lists before observing lookup cost. If a metadata key becomes an approved first-class search field, promote it to a typed column through schema governance.

Every added index must state its query owner, expected selectivity, write/storage cost, and removal criterion. Index health is reviewed using execution plans and Query Store with safe parameter capture.

## 9. Query patterns

All audit queries apply authorization scope before counting, filtering, sorting, and pagination. The application never loads a broad result set and filters it in memory.

| Query | Recommended pattern |
|---|---|
| Default timeline | Required bounded time range, approved scope predicate, Occurred At plus Audit Event PK keyset cursor |
| Actor timeline | Exact Actor ID plus bounded time range |
| Event/category investigation | Controlled code plus bounded time range |
| Resource lookup | Exact Resource Type and opaque Resource ID plus bounded range |
| Feature/outcome analysis | Exact controlled codes plus bounded range |
| Process, Job, or Correlation trace | Exact ID; then chronological keyset paging |
| Ingestion operations | Received At range, source scope, batch status, or safe reason |
| Detail view | One authorized Audit Event ID; Safe Metadata loaded only when needed |
| Broad/old export | Asynchronous Export Request with versioned filters and current authorization |
| Archive search | Separate asynchronous archive search/restore path; never an unbounded union with the hot table |

The default list projection excludes Safe Metadata JSON. Sort fields, operators, page size, maximum interactive time range, and export limits are allowlisted. Deep OFFSET paging, leading-wildcard search, arbitrary dynamic SQL, exact count on broad ranges, and user-defined JSON predicates are not supported.

Occurred At represents the original business/security event and is the default investigation timeline. Received At supports ingestion-latency, offline-arrival, and recovery analysis. A cursor always includes the unique Audit Event PK tie-breaker so equal timestamps do not skip or duplicate rows.

Queries use parameterization, bounded command timeout, cancellation, and a safe stable error response. Revoked permission or changed scope is re-evaluated for every page and every export execution; a previously issued cursor never preserves old authorization.

## 10. Retention model

### 10.1 Policy classes

Use a small versioned classification rather than a duration column chosen by each caller:

| Retention class | Intended category, subject to policy mapping |
|---|---|
| SHORT | Ingestion-batch operational records, expired export request/result metadata, and other low-evidence lifecycle records |
| STANDARD | Routine successful/failed feature actions and normal resource lifecycle events |
| EXTENDED | Authentication/authorization changes, Administrator or Support-profile actions, permission/policy changes, audit access/export, destructive actions, security outcomes, and exceptional recovery |
| POLICY_DEFINED | Records whose duration or disposition depends on document classification, investigation, regulation, contract, incident, or an approved hold |

The authoritative event catalog maps Event Type plus Event Version to Category and Retention Class. The server stamps Retention Class and Retention Policy Version at ingestion; the client cannot lower either. A future policy may extend retention, but it must not retroactively shorten eligible records without an approved migration/change decision.

Retention is independent from user history, job/result expiry, application logs, support diagnostics, secured-file versions, and source-document deletion. Deleting any of those does not implicitly delete Enterprise Audit.

### 10.2 Time basis and policy authority

Occurred At preserves the original event time; Received At proves when the server accepted it. Offline delivery means the two can differ materially. The legal retention clock must be one explicitly governed rule—for example Received At, Occurred At with a late-arrival guard, or the later of two calculated deadlines. The purge worker must not infer this from UI sorting.

**REQUIRES_PERTAMINA_STANDARD_CONFIRMATION:** durations for every class, authoritative retention timestamp, acceptable late-arrival/replay window, retroactive policy-change behavior, minimum evidence categories, and whether legal/policy hold is required. Production purge remains disabled until these are approved and represented in a versioned policy.

Policy changes require a dedicated permission, confirmation or reauthentication according to Document 09 integration, separation of duties where mandated, and their own audit event. Auditor read/export access does not confer retention-policy mutation.

### 10.3 Hold and backup interaction

If Pertamina policy requires a legal/investigation hold, the hold blocks archive purge and active-table policy purge for the covered immutable IDs or approved scope; it does not edit the event. Hold create/release is highly controlled and audited.

Expiry from the active database does not imply immediate removal from backups. Backup retention and secure media lifecycle are governed independently, and restore procedures must reapply current hold and purge controls before restored audit data becomes generally queryable.

## 11. Archive and purge

### 11.1 Lifecycle

    active Audit Events
    → select closed policy-eligible key range
    → archive if policy/volume requires
    → write immutable archive object through existing approved storage
    → verify count, range, schema version, and digest
    → commit Archive Manifest
    → bounded active-table purge
    → retain archive until its policy deadline or hold release
    → controlled archive purge

Do not deploy an archive tier merely because one is possible. Initial deployment may retain policy-eligible events in the same SQL Server until measured hot-table growth, backup windows, query latency, or compliance requires archive. Archive uses already approved object/file storage; it does not introduce a new database, event store, or vendor.

### 11.2 Archive manifest

Each manifest records only lifecycle evidence: manifest ID, approved event-key/time range, count, event/schema and retention-policy versions, created/verified timestamps, digest algorithm and value, opaque storage reference, lifecycle status, and hold state if applicable. The storage reference is never exposed directly to ordinary clients.

Archive objects are encrypted, access-controlled, integrity-verified, and restore-tested. The object format is versioned and supports bounded verification without loading the entire archive into application memory.

### 11.3 Purge execution

- A dedicated retention worker processes a small ordered key range under a short lease.
- It checkpoints the last completed key/range so restart is safe.
- Archive-required rows are not deleted until the object and manifest have been committed and verified.
- The worker rechecks current policy and hold immediately before deletion.
- Deletion is batched and throttled against transaction-log growth, lock waits, replication/backup requirements, and foreground query latency.
- No ordinary user-facing API accepts an arbitrary audit-delete filter.
- Retention runs record safe counts, policy version, range, status, checkpoint, and reason code. The run and administrative lifecycle produce separate audit events.

Archive search is a separately authorized asynchronous operation. Broad restore/search does not silently reinsert rows into the active table. If temporary rehydration is required, it is read-only, isolated, expiring, deduplicated against active IDs, and itself audited.

Archive is not a backup, and backup is not an archive. A verified backup supports disaster recovery; the archive enforces long-term policy access and lifecycle. Table partitioning, compression, and online archival products are deferred until measured scale, SQL edition, and operational standards justify them.

## 12. Server budget

### 12.1 Lightweight initial deployment

Use the existing SQL Server, the audit schema, one ingestion application, a bounded background publisher for domain outboxes, the existing job mechanism for exports, and one throttled retention worker. No new database, message/event-store product, or archive vendor is required.

Capacity planning uses measured variables:

- daily events = actions by category × events per action, including failure/retry milestones;
- active data growth = accepted events × measured p50/p95 encoded row width;
- index growth = actual index-to-data ratio after representative load;
- transaction-log generation = inserts plus index maintenance plus purge batches;
- archive growth = events leaving hot retention × encoded archive size and redundancy;
- queue recovery time = backlog size ÷ sustained acknowledged ingestion rate;
- cleanup safety = sustained eligible cleanup throughput must remain above long-term eligible growth.

### 12.2 Resource controls

- Batch by both event count and encoded bytes; both limits are configurable and returned in safe validation errors.
- Keep transactions short; perform validation/canonicalization before opening the SQL transaction where safe.
- Cap concurrent ingestion batches, audit exports, archive verification, and retention runs independently.
- Prefer sequential internal keys and narrow indexes to reduce page splits, memory grants, log traffic, and storage amplification.
- Bound Safe Metadata JSON and reject unknown/oversized schemas before SQL insertion.
- Interactive queries require scope, time bounds, page-size limits, cancellation, and keyset pagination.
- Broad/old reports run as lower-priority asynchronous exports with concurrency, row, byte, and execution-time budgets.
- Retention/archive work pauses or backs off on lock, log, storage, CPU, or latency pressure.
- Transient capacity pressure returns HTTP 429 with Retry-After or HTTP 503 without declaring the event rejected. Clients retain the same IDs and retry.

Operational metrics include accepted/duplicate/rejected rate, batch size/latency, oldest unpublished age, late-arrival distribution, rows/day, p95 row width, log bytes/event, index:data ratio, active/archive growth, top authorized queries, export concurrency, cleanup throughput, and archive verification failures. Metrics contain no event payload, Actor ID, Resource ID, or Safe Metadata values.

Do not set numerical limits in this document. Load testing and Pertamina infrastructure budgets must approve the initial values and alert thresholds.

## 13. Failure and recovery

| Failure | Required behavior |
|---|---|
| Client crashes before local commit | Auditable mutation must not be reported committed if mandatory intent was not durably stored |
| Client crashes after outbox commit | Row remains pending and is replayed after recovery |
| Server commits but acknowledgement is lost | Client replays same IDs; server responds DUPLICATE without a second event |
| Batch contains invalid items | Accepted/duplicate items commit once; rejected items receive stable per-item reasons and remain locally diagnosable |
| Same Event ID carries different content | Reject conflict, retain original, emit safe integrity signal; never overwrite |
| SQLite is full, locked, or corrupt | Show audit-recovery state; do not silently reset/evict mandatory rows; block new privileged/destructive auditable mutation |
| SQL Server is unavailable | Server-side business mutation whose mandatory audit intent cannot be transacted to its owning outbox fails before commit; existing durable outboxes retry later |
| Publisher/worker crashes | Lease expires; stable IDs and checkpoints allow another worker to resume |
| Deadlock or transient SQL timeout | Roll back the short batch and retry with jitter using identical IDs |
| Client clock is implausible | Preserve supplied Occurred At as untrusted/original evidence or reject according to catalog policy; always stamp Received At and safe clock reason |
| Archive transfer or verification fails | Keep active rows; mark run failed/retryable; never purge |
| Purge worker stops mid-range | Committed sub-batches remain deleted; checkpoint and manifest permit idempotent continuation |
| Archive object is missing or corrupt | Quarantine manifest/object, deny ordinary delivery, alert operations, and recover from verified copy/backup |
| Restore encounters existing IDs | Verify fingerprint; treat same as duplicate and different as integrity conflict |
| Retention policy is unavailable/invalid | Fail closed for purge; retain data and alert rather than guessing |
| Permission is revoked during export | Reauthorize at execution and result retrieval; cancel/deny and expire any unauthorized result |

Recovery never edits a historical Audit Event. A compensating/correction event records the newly established fact. Automated retries are limited to transient failures; permanent schema, authorization, validation, or ID-conflict failures require an explicit safe recovery path.

Backup/restore tests verify row counts/ranges, unique Audit Event IDs, event/catalog versions, batch reconciliation, archive manifests, hold/policy state, and absence of duplicate insertion after publishers reconnect.

## 14. Security and privacy

### 14.1 Data minimization

Enterprise Audit stores evidence that an action occurred, not document or user-content payload. The schema and event catalog must prohibit:

- passwords, PINs, PDF passwords, tokens, cookies, authorization headers, secrets, private keys, and connection strings;
- document bytes, document text, OCR full text, clipboard contents, AI document prompts/responses, chat content, visual-signature images, thumbnails, screenshots, and scanner images;
- raw local paths, filenames by default, storage keys/URLs, temporary-file names, provider request/response bodies, SQL text/parameters, claims, stack traces, and raw exceptions;
- support issue text or attachment contents.

Allowed Safe Metadata is versioned per Event Type, allowlisted, bounded in encoded bytes and nesting, and restricted to controlled codes, booleans, safe counters/sizes, durations, versions, execution location, and approved opaque references. Unknown properties are rejected rather than stored “for later.”

Actor ID, Resource ID, Process/Job/Correlation ID, organization context, timestamps, and action outcomes remain confidential metadata even though they are safe for the Audit plane. They are excluded from telemetry and diagnostic logs, encrypted in transit, protected at rest according to Pertamina standards, and disclosed only within authorization scope.

### 14.2 Authorization boundary

The Document 09 permission grammar should map the Audit plane as follows:

- audit.event.list — scoped list/filter capability;
- audit.event.read — scoped detail capability;
- audit.export.create — create an authorized asynchronous export;
- audit.export.read — retrieve only an authorized, unexpired export;
- a separate highly controlled configuration permission for retention/hold policy if approved.

There is no audit.event.update or audit.event.delete permission. An Auditor is read-only; an Administrator receives audit access only through explicit grant, not implied role hierarchy. Audit offline batching/retry is a trusted system/client capability, not a user permission that bypasses the initiating user's authorization.

Every request applies current actor, scope, and resource authorization. Export creation, execution, and result retrieval each reauthorize. Result files are encrypted, time-limited, access-controlled, and deleted on expiry; URLs/storage references are never treated as authorization.

### 14.3 Ingestion and operational privacy

- TLS is mandatory between client/API, application/SQL Server, and application/object storage.
- Authentication context, effective permission/scope, Received At, and policy mapping are established server-side.
- Client/source identity is bound to an approved account/application profile without hardware fingerprinting.
- Metadata JSON is parsed into a typed event-version schema; content sniffing or free-form pass-through is not used.
- Logs and traces record safe codes, aggregate counts, latency, and Correlation ID only where the logging policy permits; they do not copy the audit row or SQL parameters.
- Audit access/export, failed privileged access, policy changes, archive access, and purge lifecycle are themselves audited without recursively generating unbounded events.
- Encryption and key custody use existing Pertamina platform services and separation of duties; keys are never stored alongside export/archive objects or in application configuration plaintext.

## 15. Risks and conflicts

| Risk or unresolved decision | Impact | Required action |
|---|---|---|
| No integrated Document 09 FINAL file was present; only its locked decision register, PASS integration audit, and specialist outputs were available | Permission names or role wording could drift during final integration | Integrator must reconcile this report against the issued Document 09 FINAL without weakening no-update/no-delete rules |
| Legal retention durations and authoritative clock are unknown | Purge could be premature or indefinite | **REQUIRES_PERTAMINA_STANDARD_CONFIRMATION**; keep production purge disabled until approved |
| Exact Event Type catalog and safe metadata schemas are not final | Overcollection, inconsistent evidence, or incompatible offline events | Approve versioned catalog, ownership, compatibility, and deprecation process |
| Audit Event ID data type/canonical form is not finalized | Cross-platform and SQL index inconsistency | Select one source-safe opaque identifier representation and test desktop/mobile/server generation |
| Actor role/unit snapshot semantics and source are not final | Historical authorization context may be inaccurate or overexposed | Define authoritative, minimal server-derived context and scope handling |
| Queue, batch, replay, lateness, and clock-skew limits are unset | Data loss risk or uncontrolled server load | **REQUIRES_PERTAMINA_STANDARD_CONFIRMATION** plus load/recovery tests |
| Behavior for ordinary actions when local mandatory-audit queue is unavailable is unset | Availability versus evidence conflict | Integrator/Security classify fail-closed actions; never silently drop mandatory events |
| Legal/investigation hold requirement is unknown | Required evidence could be purged | **REQUIRES_PERTAMINA_STANDARD_CONFIRMATION** before retention implementation |
| Archive need, format, hot-window, and search SLA are unknown | Premature architecture or poor recoverability | Measure volume/query/backup window first; use existing approved storage only if required |
| Tamper-evidence requirement beyond SQL controls is unknown | Hash chaining/WORM may be overbuilt or required evidence may be missed | **REQUIRES_PERTAMINA_STANDARD_CONFIRMATION**; material Integrator/Compliance decision |
| Fingerprint canonicalization and algorithm are not approved | False ID conflicts or unverifiable archives | Version the canonical form/algorithm and test across producers; do not claim signature/nonrepudiation |
| SQL edition and infrastructure constraints are not documented here | Partition/compression/encryption assumptions may fail | Confirm capabilities before adopting optional optimizations |
| Numeric infrastructure and export budgets are unknown | Write amplification, queue growth, and resource contention | Establish through representative load tests; tune indexes/limits from Query Store and metrics |
| Backup retention may exceed active audit retention | Purged records remain recoverable in backup | Document backup media lifecycle and guarded restore/re-purge process |

There is no identified need for a new database, event-store product, vendor, full cloud workspace, or document payload in Audit. Adding any of them would conflict with Documents 01–08 unless separately approved.

## 16. Suggested Document 10 text

The following text is ready for integration, subject to the marked Pertamina policy decisions.

### 16.1 Audit storage and ownership

PERTAMINA OTO SHALL store Enterprise Audit in the existing SQL Server under the audit schema. Audit Events SHALL be append-oriented facts. Ordinary application, User, Administrator, Auditor, Support-profile, export, and reporting identities SHALL NOT update or delete Audit Event rows. Schema migration and policy purge SHALL use separate, narrowly controlled identities and SHALL be independently logged and audited.

The initial model SHALL use audit.AuditEvents, audit.IngestionBatches, and audit.ExportRequests. Audit Events SHALL use a sequential internal bigint key for write locality and a source-generated stable Audit Event ID with a unique constraint for idempotency. Optional archive manifests, retention runs, and legal-hold structures SHALL be introduced only when the corresponding policy/lifecycle is approved.

### 16.2 Minimal event contract

An Audit Event SHALL contain only approved structured evidence: Audit Event ID; Event Type and Version; Category; Occurred At UTC; server-stamped Received At UTC; server-established Actor Type/ID and approved minimal actor context; Action only when not derivable from Event Type; optional opaque Resource, Feature, Process, Job, and Correlation identifiers; Source Channel, Platform, App Version, and Execution Location; Outcome and safe Reason Code; approved size counters; Retention Class/Policy Version; and bounded versioned Safe Metadata.

Created At SHALL NOT duplicate Received At unless a distinct semantic need is approved. The schema SHALL NOT store passwords, tokens, secrets, document bytes/text, OCR text, signature images, clipboard, AI prompts/responses, screenshots, raw paths, storage keys, raw exceptions, SQL, claims, or provider payloads.

### 16.3 Append, integrity, and corrections

Server-originated business mutations SHALL persist an audit intent in a domain-owned transactional outbox in the same short transaction as the mutation. Offline clients SHALL persist safe events in a protected SQLite audit outbox before reporting a mandatory auditable mutation complete. Publishers SHALL append idempotently and SHALL NOT hold database transactions across external work.

Correction, reversal, recovery, and administrative intervention SHALL append a new event rather than modify an earlier event. Integrity SHALL rely initially on controlled event schemas, server-derived authority fields, SQL permissions, EF Core append guards, constraints, unique IDs, reconciliation, backup controls, and verified archive digests. A server-computed per-event fingerprint SHALL support duplicate comparison and corruption diagnostics, but SHALL NOT include retry-varying fields or be represented as a signature or nonrepudiation control.

Cryptographic chaining, WORM storage, or external anchoring SHALL NOT be introduced without a confirmed legal/compliance requirement. **REQUIRES_PERTAMINA_STANDARD_CONFIRMATION:** whether independently verifiable tamper evidence is mandatory.

### 16.4 Idempotency and offline synchronization

The producer SHALL create one stable Audit Event ID at first durable recording and reuse it for every retry. The server SHALL insert a new valid ID once, acknowledge an existing ID with the same canonical fingerprint as DUPLICATE, and reject an existing ID with different content as EVENT_ID_REUSE_CONFLICT. Correlation ID, Job ID, Event Type, Actor, Resource, and timestamp SHALL NOT be used as event deduplication keys.

Offline sync SHALL follow durable queue, bounded batch, authenticated submit, server commit, explicit per-item acknowledgement, local acknowledgement commit, and acknowledged-row cleanup. Batch IDs SHALL also be stable and idempotent. Transport timeout SHALL cause replay with identical IDs. Mandatory audit rows SHALL NOT be silently dropped, overwritten, or evicted due to a queue limit. Transient failures SHALL use bounded backoff with jitter; permanent rejected events SHALL stop hot retry and remain safely diagnosable.

### 16.5 Query, index, and export

Audit queries SHALL apply authorization scope before filtering, counting, sorting, and pagination. Interactive queries SHALL require bounded time ranges, allowlisted filters/sorts, page-size limits, and keyset pagination using timestamp plus the unique internal tie-breaker. Default lists SHALL exclude Safe Metadata JSON. Broad or old requests SHALL execute as bounded asynchronous exports and SHALL reauthorize at creation, execution, and retrieval.

The initial index set SHALL remain narrow: clustered internal key; unique Audit Event ID; Actor/time; Feature/time; Outcome/time; and Process lookup, plus operational indexes for ingestion batches and export requests. Category/Event Type, Resource, Job, Correlation, Received At, JSON promotion, INCLUDE columns, partitioning, and compression SHALL be added only for measured query paths and supported infrastructure.

### 16.6 Retention, archive, and purge

Audit retention SHALL use the versioned classes SHORT, STANDARD, EXTENDED, and POLICY_DEFINED. Event Type plus Version SHALL determine the class; clients SHALL NOT lower retention. Audit retention SHALL remain independent from user history, job/result expiry, logs, secured-file versions, or document deletion.

**REQUIRES_PERTAMINA_STANDARD_CONFIRMATION:** class durations, authoritative retention timestamp, late-arrival/replay window, retroactive policy behavior, minimum evidence categories, and legal/policy hold. Production purge SHALL remain disabled until the policy is approved and versioned.

If archive is required, the system SHALL use existing approved encrypted storage and a verified manifest containing range, count, schema/policy versions, digest, timestamps, status, and opaque reference. Active rows SHALL NOT be purged before required archive verification. Purge SHALL be policy/hold aware, checkpointed, throttled, resumable, limited to small key ranges, and unavailable as an arbitrary user API. Archive search/restore and final archive purge SHALL be separately authorized and audited.

### 16.7 Security and permissions

Audit list, read, export create, and export retrieval SHALL use separate lower-case business permissions consistent with Document 09. There SHALL be no ordinary audit update or delete permission. Auditor SHALL remain read-only; Administrator access SHALL require explicit grant. Retention/hold configuration, if adopted, SHALL be a separate highly controlled permission and SHALL NOT be implied by audit read/export.

Audit data, exports, local outboxes, SQL connections, and archive objects SHALL use approved encryption and key-management controls. Actor/resource identifiers and correlation metadata SHALL be treated as confidential. Export results SHALL be authorized, expiring, and cleaned up; an object reference or download URL SHALL never constitute authorization.

### 16.8 Operations and recovery

Ingestion, query, export, archive, and retention workloads SHALL have independent concurrency, batch, byte, time, and queue budgets. Capacity pressure SHALL use explicit retryable responses without changing event IDs. Retention/archive work SHALL back off under foreground resource pressure.

The service SHALL measure safe aggregates for event rate, row width, duplicate/reject rate, queue age, ingestion latency, SQL log/index growth, export concurrency, active/archive growth, and cleanup throughput. Metrics SHALL NOT contain event payload or confidential identifiers. Recovery tests SHALL verify unique IDs, row/range counts, batch reconciliation, policy/hold state, manifests/digests, and duplicate-free reconnect after restore.
