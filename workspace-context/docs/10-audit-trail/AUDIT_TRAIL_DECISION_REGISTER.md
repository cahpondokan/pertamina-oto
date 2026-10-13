# AUDIT TRAIL DECISION REGISTER
## PERTAMINA OTO — Office Tools Organizer

> **Document:** 10 Audit Trail Design  
> **Version:** 1.0  
> **Date:** 27 Agustus 2026  
> **Status:** 20/20 AUTO-LOCKED  
> **Decision mode:** Technical derivation from Documents 01–09 FINAL  
> **Canonical feature baseline:** 550  
> **Primary PDF Tool Cards:** 33

---

# 1. Lock Rule

The decisions below preserve the approved business, API, database, security, role, Local-First, and Assistant boundaries. They do not add a database, broker, vendor, formal role, privileged document access, sensitive Audit payload, Feature ID, or processing engine.

Unknown Pertamina-specific policy values remain `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`. Missing confirmation never permits an arbitrary purge, wider access, weaker evidence, or fail-open behavior.

---

# 2. Decision Summary

| ID | Decision | Status |
|---|---|---|
| `AUD-DEC-001` | Four-plane evidence separation | **AUTO-LOCKED** |
| `AUD-DEC-002` | Risk-based event eligibility and Core coverage | **AUTO-LOCKED** |
| `AUD-DEC-003` | Controlled taxonomy, naming, and outcomes | **AUTO-LOCKED** |
| `AUD-DEC-004` | Human actor, origin, and executor attribution | **AUTO-LOCKED** |
| `AUD-DEC-005` | Opaque resource and correlation model | **AUTO-LOCKED** |
| `AUD-DEC-006` | Typed/versioned minimized event envelope | **AUTO-LOCKED** |
| `AUD-DEC-007` | Governed event catalog and milestone policy | **AUTO-LOCKED** |
| `AUD-DEC-008` | Assistant and AI audit parity | **AUTO-LOCKED** |
| `AUD-DEC-009` | Closed sensitive-field exclusion | **AUTO-LOCKED** |
| `AUD-DEC-010` | Owner transaction plus durable Audit intent | **AUTO-LOCKED** |
| `AUD-DEC-011` | Protected offline Audit outbox and batch sync | **AUTO-LOCKED** |
| `AUD-DEC-012` | At-least-once delivery with effective-once storage | **AUTO-LOCKED** |
| `AUD-DEC-013` | Append-oriented integrity and linked correction | **AUTO-LOCKED** |
| `AUD-DEC-014` | Existing SQL/SQLite storage and measured indexes | **AUTO-LOCKED** |
| `AUD-DEC-015` | Explicit permission, scope, and field projection | **AUTO-LOCKED** |
| `AUD-DEC-016` | Authorization-first structured query | **AUTO-LOCKED** |
| `AUD-DEC-017` | Bounded asynchronous export and safe reports | **AUTO-LOCKED** |
| `AUD-DEC-018` | Policy-class retention, archive, hold, and purge | **AUTO-LOCKED** |
| `AUD-DEC-019` | Bounded performance, failure, and health policy | **AUTO-LOCKED** |
| `AUD-DEC-020` | Layered verification and controlled handoffs | **AUTO-LOCKED** |

---

# 3. Locked Decisions

## AUD-DEC-001 — Four-Plane Evidence Separation

**Decision**

Enterprise Audit, User History, Technical Observability, and Issue/Support records are separate planes with separate purpose, access, storage projection, and lifecycle. Correlation ID may link them but does not copy content or grant access.

**Reason**

- Prevents Audit from becoming a log, personal history, or support-content store.
- Preserves Documents 01, 06, 07, 08, and 09.

## AUD-DEC-002 — Risk-Based Event Eligibility and Core Coverage

**Decision**

Use four evidence levels:

```text
L0  No Audit event
L1  Personal/local History or technical evidence only
L2  Enterprise business Audit
L3  Privileged/security/high-risk Enterprise Audit
```

All 33 Primary PDF Tool executions create approved safe L2 process milestones because Document 09 marks them Audit-required. Protect, Unlock, visual Sign, Redact, approved AI/provider transfer, permanent deletion, and other privileged/security outcomes add L3 evidence. Secondary local actions become L2/L3 only when the event catalog identifies a business, security, administration, support, account, server, cross-device, or compliance boundary.

**Reason**

- Preserves 33/33 Core Audit coverage without sending clicks, previews, page progress, or every secondary local action to the server.
- Keeps Enterprise Audit meaningful and storage-bounded.

## AUD-DEC-003 — Controlled Taxonomy, Naming, and Outcomes

**Decision**

Event codes use stable PascalCase facts in the form `Category.PastTenseFact`, for example `Auth.LoginSucceeded` and `Administration.PermissionAssigned`. Each event has an independent integer `EventVersion`.

Controlled outcomes are:

```text
ACCEPTED
SUCCEEDED
FAILED
DENIED
CANCELLED
PARTIAL
EXPIRED
REVOKED
```

`SUCCESS` is normalized to `SUCCEEDED`; `PARTIALLY_COMPLETED` is normalized to `PARTIAL`.

**Reason**

- Removes string/schema drift across Agent reports.
- Keeps code, storage, export, and contract tests deterministic.

## AUD-DEC-004 — Human Actor, Origin, and Executor Attribution

**Decision**

Primary actor types are `USER`, `ADMINISTRATOR`, `AUDITOR`, `SUPPORT`, `SYSTEM`, `SERVICE`, and `WORKER`. Assistant is never the primary authority. For Assistant-triggered work:

```text
Actor       = actual authenticated user
Origin      = ASSISTANT
Executor    = application/service/worker that performed the step
OnBehalfOf  = user reference when a workload executes asynchronously
```

**Reason**

- Preserves accountability and Document 09 authorization parity.
- Prevents Assistant from becoming a super actor.

## AUD-DEC-005 — Opaque Resource and Correlation Model

**Decision**

Audit references logical resource type plus opaque resource ID. Feature ID, Process ID, Job ID, Correlation ID, Issue ID, SecuredFile ID, and Export Request ID are trace identifiers only. They are never authorization or storage locators.

Filename, path, URL, object key, and document content are excluded by default.

**Reason**

- Enables safe investigation without disclosing content or physical storage.
- Prevents IDOR and identifier-based privilege escalation.

## AUD-DEC-006 — Typed/Versioned Minimized Event Envelope

**Decision**

Every event is validated against a registered type/version schema. The envelope uses only controlled fields and event-specific allowlisted metadata. `SafeMetadata` is closed, versioned, depth/size bounded, and rejects unknown fields.

**Reason**

- Satisfies FR-AUDIT-001 while enforcing FR-AUDIT-002.
- Prevents arbitrary request/entity/exception serialization.

## AUD-DEC-007 — Governed Event Catalog and Milestone Policy

**Decision**

One authoritative Event Catalog maps business action to event type/version, evidence level, required fields, allowed metadata, transaction class, permission/scope, and retention class. Applications cannot invent local event strings.

Processing Audit records accepted and terminal milestones only, plus a materially meaningful retry/security outcome. It does not record progress percentages, pages, chunks, heartbeats, navigation, or internal pipeline steps.

**Reason**

- Avoids duplicate contracts and over-auditing.
- Makes Feature, role, API, database, and test traceability explicit.

## AUD-DEC-008 — Assistant and AI Audit Parity

**Decision**

Assistant invocations reuse the underlying canonical Feature event with `Origin=ASSISTANT`. Separate Assistant events exist only for meaningful orchestration/security boundaries. AI evidence stores route class (`LOCAL` or `INTERNAL_APPROVED`), policy decision, Feature ID, outcome, and safe provider-profile code only when authorized.

Prompt, conversation, document context, provider request/response, and generated content are never Audit payload.

**Reason**

- Preserves the Assistant-as-orchestrator rule and public-AI prohibition.
- Avoids duplicate Feature events for the same action.

## AUD-DEC-009 — Closed Sensitive-Field Exclusion

**Decision**

The Audit contract, outboxes, storage, export, logs, traces, and alerts reject passwords/passphrases; access/refresh tokens; cookies; secrets/keys; connection strings; document bytes/text/previews; filenames/paths by default; OCR/extracted/generated text; clipboard; signature/paraf assets; issue message/note/screenshot bytes; Assistant/AI content; raw claims; raw exceptions/stack/SQL; provider payload; storage URL/key; device fingerprint; and exact IP in the baseline.

**Reason**

- Security and privacy are enforced end to end, not only in the final SQL row.
- Exact IP or other exceptional identity metadata requires a separately approved restricted profile.

## AUD-DEC-010 — Owner Transaction Plus Durable Audit Intent

**Decision**

Feature modules never write `audit.AuditEvents` directly. A critical business mutation and its typed Audit intent commit in the owning module's short transaction/outbox. Final projection is asynchronous.

Transaction classes are:

```text
ATOMIC_AUDIT_INTENT
DURABLE_BEFORE_DISCLOSURE
DURABLE_RETRY_AFTER_COMMIT
OBSERVABILITY_ONLY
```

Role/permission/account/session/policy changes, permanent deletion, key/recovery changes, successful session grant, and privileged Audit/Support disclosure fail closed when required intent cannot be made durable. After a durable intent exists, routine processing may complete while projection retries.

**Reason**

- Protects important evidence without distributed transactions or synchronous heavy Audit calls.
- Preserves module ownership and server efficiency.

## AUD-DEC-011 — Protected Offline Audit Outbox and Batch Sync

**Decision**

Offline-required Audit metadata is validated and persisted in protected metadata-only SQLite `audit_outbox`, then synchronized through the existing idempotent batch endpoint when connectivity and authenticated policy permit.

Rows remain durable until acknowledgement and required events are never silently evicted. If no durable slot exists, a new L3/privileged mandatory-audit action is blocked before mutation; existing source, draft, and recovery data remain accessible. The exact degraded-mode boundary for ordinary L2 processing is `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` and may continue only through an approved path that does not discard required evidence.

**Reason**

- Satisfies Offline-First and FR-AUDIT-003.
- Prevents a disconnected device from losing mandatory evidence.

## AUD-DEC-012 — At-Least-Once Delivery With Effective-Once Storage

**Decision**

Delivery is at least once. A stable Audit Event ID/client Event ID, source scope, batch identity, request fingerprint, and unique constraints make storage effectively once per semantic event.

Same ID plus same fingerprint returns duplicate acknowledgement. Same ID plus different immutable data is a conflict/security signal and never overwrites the stored event. Source sequence preserves local order where required; no global arrival order is inferred.

**Reason**

- Handles retries, crashes, reconnect, duplicate batches, and reordered delivery safely.
- Correlation ID, Job ID, timestamps, or event type are not deduplication keys.

## AUD-DEC-013 — Append-Oriented Integrity and Linked Correction

**Decision**

Ordinary application/user paths have insert/query only. Source events are not updated or deleted. Corrections, late context, reversals, or supersession create new linked events. Governed retention purge is a lifecycle operation, not an ordinary event mutation.

Integrity uses server validation/derivation, restricted identities, database constraints, fingerprints, durable outboxes, backups/restores, and optional verified archive manifests. Document 10 does not claim cryptographic tamper-proof or WORM capability.

**Reason**

- Provides trustworthy evidence without unsupported guarantees.
- Reconciles append-oriented behavior with policy-governed lifecycle deletion.

## AUD-DEC-014 — Existing SQL/SQLite Storage and Measured Indexes

**Decision**

Use existing SQL Server `audit` schema and existing local SQLite metadata store. Baseline server tables remain `audit.AuditEvents`, `audit.IngestionBatches`, and `audit.ExportRequests`; conditional lifecycle tables are introduced only through controlled Document 07 revision when policy/scale requires them.

Indexes start narrow for unique event ID, actor/time, Feature/time, outcome/time, Process ID, batch identity/recovery, and export ownership/state. Resource, category/type, received-time, Job ID, Correlation ID, or organizational-scope indexes require measured selective queries.

**Reason**

- No new database, event store, or broker is needed.
- Limits SQL write amplification and storage growth.

## AUD-DEC-015 — Explicit Permission, Scope, and Field Projection

**Decision**

Every Audit list, detail, report, export, lifecycle, or linked-support view requires online authentication, explicit permission, server-derived scope, category/type allowance, field projection, current policy/classification, and reauthentication when required.

Auditor has scoped `audit.event.list/read`; export is separate. Administrator and Support receive no implicit Audit access. Support sees issue-linked safe evidence only. User has Personal History, not Enterprise Audit Console.

**Reason**

- Preserves Document 09 rights exactly.
- Masking supplements authorization and never replaces it.

## AUD-DEC-016 — Authorization-First Structured Query

**Decision**

Scope is applied before count, filter, sort, cursor, detail, or export. Query uses controlled exact filters, bounded UTC half-open ranges, stable keyset pagination, narrow list projection, and authorization-bound expiring cursors.

No arbitrary SQL, JSON path, regex, expression, leading-wildcard large-table scan, raw payload search, or filename/document/OCR/issue/prompt full-text Audit search exists.

**Reason**

- Prevents cross-scope inference and unbounded database cost.
- Matches Document 06 list-query and Document 07 index rules.

## AUD-DEC-017 — Bounded Asynchronous Export and Safe Reports

**Decision**

Enterprise Audit export uses one bounded asynchronous flow with separate permission, current scope, optional policy-driven reauthentication/approval, request fingerprint, safe projection, private expiring artifact, manifest/digest, repeated authorization, and audited request/generation/download/expiry lifecycle.

UTF-8 CSV is the baseline detailed-row format. Fixed PDF is optional for approved summary templates. XLSX is not baseline and requires an explicit business requirement. Cells are neutralized against spreadsheet formulas.

**Reason**

- Avoids a second synchronous disclosure path and protects a small server.
- Does not add a new dashboard or reporting product.

## AUD-DEC-018 — Policy-Class Retention, Archive, Hold, and Purge

**Decision**

Events map to versioned classes `SHORT`, `STANDARD`, `EXTENDED`, or `POLICY_DEFINED`; clients cannot lower the class. Durations, authoritative retention timestamp, legal-hold authority, retroactivity, and archive obligations are not invented and remain `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

Production purge stays disabled until policy is approved. Lifecycle work is authorized, hold-aware, bounded, checkpointed, resumable, auditable, and verifies archive integrity before removal. Backup and archive are separate.

**Reason**

- Controls growth without arbitrary deletion or indefinite-retention assumptions.
- Preserves Auditor read-only and official-policy ownership.

## AUD-DEC-019 — Bounded Performance, Failure, and Health Policy

**Decision**

Audit uses narrow intents, short transactions, bounded batches/concurrency, durable queues, retry with jitter, no connection held during delay, keyset queries, asynchronous exports, throttled cleanup, and backpressure. Required Audit is never disabled for performance.

Health exposes safe counts, ages, rates, latency, outcomes, and reason codes for intents, outboxes, ingestion, offline sync, queries/exports, archive/purge, and unusual event rates. Monitoring never copies Audit payload and includes a recursion guard.

**Reason**

- Protects user processing, SQL Server, and network while retaining required evidence.
- Numeric limits belong to benchmark/Document 12 and official operations policy.

## AUD-DEC-020 — Layered Verification and Controlled Handoffs

**Decision**

Acceptance requires unit, contract, integration, authorization, negative/abuse, offline/reconnect, failure/recovery, E2E, performance/DoS, backup/restore, and security-readiness evidence. Forbidden-field canaries must be absent from contract, outbox, SQL, export, logs, traces, and alerts.

Document 06 requires a controlled compatible Audit batch-contract addendum for per-event type/version, source sequence, trust provenance, and per-event acknowledgement/conflict results. Document 07 requires a controlled addendum only if typed searchable fields, owner outboxes, or lifecycle tables are absent. Neither parent FINAL is edited silently.

**Reason**

- Converts the design into testable implementation gates.
- Records gaps without creating parallel endpoints, storage, roles, or Feature IDs.

---

# 4. Confirmation Register

The following values are intentionally not locked:

```text
REQUIRES_PERTAMINA_STANDARD_CONFIRMATION

- official retention durations per class
- authoritative retention clock for delayed/offline events
- legal/investigation hold authority and approval chain
- organizational Audit scope vocabulary and assignment authority
- official data-classification mapping and masking rules
- whether exact IP/security-device metadata is mandated
- ordinary-L2 degraded behavior when protected Audit queue is unavailable/full
- queue, batch, export, query-range, and archive thresholds
- RPO/RTO, alert routing, on-call owner, and SIEM route
- archive technology/product if measured need later exists
```

These confirmations do not reopen the 20 logical decisions unless the official standard directly conflicts with a locked parent requirement.

---

# 5. Status

```text
Audit decisions       : 20 / 20 AUTO-LOCKED
Open technical choice : 0
User approval required: 0
Feature baseline      : 550
Core PDF coverage     : 33 / 33
```
