# AGENT A — DOMAIN & APPLICATION UNIT TEST REPORT

> **Task ID:** DOC11-AGENT-A  
> **Role:** Senior System Analyst / Senior C#/.NET Developer / Domain & Application Architect / Unit Test Specialist  
> **Status:** Specialist recommendation for controlled integration into Document 11; no production code  
> **Date:** 27 Agustus 2026  
> **Authoritative baseline:** Documents 01–10 FINAL and `MASTER_PROJECT_SUMMARY.md` v3.3  
> **Canonical baseline:** 550 Features; 33 Primary PDF Tool Cards; first vertical slice `FEAT-PDF-005 — Merge PDF`  
> **Locked test platform:** MSTest + Microsoft.Testing.Platform (MTP)

This report designs behavior-focused unit tests. It does not edit the Document 11 master, select a PDF/IAM/AI vendor, call a real network, connect to SQL Server, run a full UI flow, or implement production code.

Traceability abbreviations used below:

- `BP` — Document 01 Business Process.
- `REQ` — Document 02 Requirement Specification.
- `FC` — Document 03 Feature Catalog.
- `SD` — Document 04 Solution Design.
- `TR` — Document 05 Technical Reference.
- `API` — Document 06 API Documentation.
- `DB` — Document 07 Database Design.
- `SEC` — Document 08 Security Design.
- `ROLE` — Document 09 Role Matrix.
- `AUD` — Document 10 Audit Trail Design.

---

# 1. Unit-test scope

## 1.1 Objective

The unit-test layer shall prove observable domain and application behavior before infrastructure is involved. The central question is not “which private method was called?” but:

```text
Given valid or invalid business context
When a domain/application operation is requested
Then the returned decision, state, result, error, and emitted intent
match the authoritative business contract
and no prohibited side effect occurs.
```

## 1.2 In scope

| Area | Behavior under test |
|---|---|
| Domain rules | Invariants, value objects, confirmation classification, source protection, result finality, retry/resume eligibility, policy limits |
| Application services | Use-case orchestration, authorization/capability outcome consumption, validation gates, execution-plan selection, safe publication |
| Commands | Execute, cancel, retry, resume, reorder/remove input, finalize/publish result |
| Queries | Feature discovery projection, Job/result projection, capability/availability explanation; no mutation |
| Handlers | Correct outcome and side-effect intent for valid/invalid commands; no dependence on handler internals |
| Validators | Feature ID/schema, parameters, input count/type/state, file validation, policy/resource budget, confirmation, result validation |
| State transitions | `QUEUED`, `RUNNING`, `WAITING_CONNECTION`, `WAITING_RESOURCE`, `RETRYING`, `COMPLETED`, `FAILED`, `CANCELLED` |
| Feature Registry | Canonical uniqueness, stable contracts, status/exposure, typed parameter version, no runtime Feature invention |
| Processing Coordinator | Immediate/background and Local/Server/Hybrid decision using capability/policy/resource/connectivity facts |
| Engine Resolver | Approved engine availability, local-first preference, fallback eligibility, no caller-selected vendor/provider |
| Result models | Immediate/background/success/failure shape, output validation, safe identifiers, final/temporary distinction |
| Error mapping | Stable safe code/category, retryability, state and API-semantic mapping, Correlation ID propagation |
| Evidence intents | Safe History/Audit/application event facts, including Document 10 L2 accepted/terminal evidence for all 33 Core Tools |

## 1.3 Outside the unit-test layer

The following need other suites and shall not be simulated as if a unit test proves them:

- real PDF parsing/rendering/fidelity or malicious-file resistance;
- real filesystem atomicity, locks, reparse/symlink/TOCTOU behavior;
- real SQLite/SQL Server constraints, EF Core mappings, transactions, migrations, or query plans;
- real HTTP, SignalR, object storage, SSO, Worker IPC, or provider communication;
- real Office/OCR/AI/scanner engines and resource consumption;
- full UI interaction, responsiveness, accessibility, or platform-native flows;
- throughput, CPU, RAM, disk, queue, concurrency, timeout, and numerical performance gates;
- penetration testing and end-to-end security assurance.

Those belong to integration, contract, component, E2E, security, PoC, and performance layers. A passing fake engine proves coordinator behavior, not PDF correctness.

## 1.4 Suite boundaries

Recommended logical test assemblies follow module ownership:

```text
Pertamina.Oto.<Module>.Domain.Tests
Pertamina.Oto.<Module>.Application.Tests
Pertamina.Oto.FeatureRegistry.Tests
Pertamina.Oto.Processing.Domain.Tests
Pertamina.Oto.Processing.Application.Tests
Pertamina.Oto.Pdf.Domain.Tests
Pertamina.Oto.Pdf.Application.Tests
Pertamina.Oto.SharedKernel.Tests        (only true shared value contracts)
```

Do not create one global test project that reaches through every module's internals. Tests may consume public/internal-to-test contracts according to the production assembly boundary, but shall not force visibility of private implementation merely to assert call order.

## 1.5 Test categories

Recommended MSTest traits:

| Trait | Meaning |
|---|---|
| `Layer=Domain` | Pure rules/value objects/state machine |
| `Layer=Application` | Use-case orchestration with fakes |
| `FeatureId=FEAT-PDF-005` | Canonical Feature trace |
| `Requirement=FR-PROC-002` | Requirement trace; repeatable for multiple refs |
| `Family=PDF_PAGE_OPERATIONS` | Reusable family suite |
| `Risk=HIGH` | Negative/sensitive behavior requires stronger review |
| `Regression=<defect-id>` | Permanent regression test for a fixed defect |

Traits support selection and traceability; they do not replace readable test names and assertions.

---

# 2. Domain-test strategy

## 2.1 Test public behavior and invariants

Each domain test should use Arrange–Act–Assert or Given–When–Then and normally assert:

1. returned decision/result;
2. aggregate state and version;
3. domain/application facts intended for downstream handling;
4. absence of prohibited mutation or disclosure;
5. stable error/category where the source contract defines one.

Avoid assertions such as “private method X called twice,” exact LINQ implementation, exact collection class, or internal helper sequence. A boundary fake may record that an engine/storage gateway was not invoked when this absence is a business/security requirement.

## 2.2 Core business invariants

| Invariant | Required unit behavior | Source trace |
|---|---|---|
| Source never silently overwritten | Default operation creates separate working/temp/result identity; a target equal to source is rejected unless a distinct authorized overwrite action and confirmation contract applies | BP §5, §14–15; `FR-STOR-001..003`; `FEAT-EDIT-012`; SD §13; DB §14 |
| Non-destructive working state | Reorder, remove page/input, crop, rotate, watermark draft, and parameter edits change working state only | BP §14; `FR-STOR-002`; `FEAT-EDIT-001..010` |
| Confirmation is risk-based | Routine reversible edits do not ask for confirmation; overwrite, permanent delete, content upload, share, final export/download, and irreversible/sensitive finalization do | BP §5/§14; `UIR-UI-004`; ROLE sensitive actions |
| Output is not final before validation | Temporary/intermediate/failed output cannot become the primary final result or `COMPLETED` | BP §11/§15/§22; `NFR-REC-002..003`; API §13.6; SEC §19 |
| Local-First is capability-aware | Eligible work selects approved local execution when capable, permitted, sufficiently resourced, and safe | `FR-PROC-001..003`; FC profiles; SD §10–11/§18 |
| Background does not imply server | Heavy local work may become a Local Worker job without using a server-processing API | SD §11.2/§18; API §19 |
| Server content transfer is explicit | Resolver cannot select/dispatch server content processing without policy, authorization, connectivity, capacity, and required notice/consent | `FR-PROC-004`; API §11/§12; SEC §18/§21 |
| Feature contract is canonical | Only registered, active, exposed, schema-compatible Feature IDs execute; aliases resolve to canonical ID but are not execution discriminators | FC §1/§6; SD §14; API §11 |
| Administrator cannot redefine semantics | Runtime configuration may enable/disable/limit but cannot change Merge from `PDF[] → PDF` or change canonical risk/destructive meaning | SD §14/§19; ROLE §14/§24 |
| Policy precedence is deterministic | Safety/security invariant wins over enterprise/environment/device/user preference | SD §19; ROLE §5 |
| Retry is bounded and classified | Only allowlisted transient failures retry automatically; permanent validation/business failures do not loop | `FR-REC-004..005`; `NFR-REC-005`; API §13.5 |
| Resume is compatibility-bound | Resume requires matching input identity, Feature/schema, engine/provider version, stage, and integrity marker | API §13.5; DB §15.4 |
| Correlation is one logical thread | Provided Correlation ID propagates unchanged; absent ID is generated once and returned; never used as authority | `NFR-REC-004`; API §5.5; SD §21; AUD §28 |
| Core processing evidence | Each of the 33 Core executions creates safe L2 accepted/terminal evidence; high-risk outcomes add L3; progress is not Audit | AUD §17/§24/§54 |

## 2.3 Domain objects and value contracts

Recommended pure-unit targets:

| Target | Positive cases | Negative cases |
|---|---|---|
| `FeatureId` | Known canonical format and case | blank, malformed, alias used as execution ID, unknown/deprecated/disabled lookup |
| `ParameterSchemaVersion` | Exact supported version; compatible version adapter result | missing where required, unsupported major/shape, extra/unknown field |
| `CorrelationId` | Opaque valid caller value or generated value | whitespace, over-limit, personal/content-bearing value if guard defines it |
| `JobId` | Opaque identity retained through state changes | used as ownership/authorization proof |
| Ordered input collection | Two or more valid PDF inputs in stable order | zero/one input; null identity; order references unknown input |
| `PolicyLimit` | boundary below/equal to limit | negative limit, overflow, over-limit count/page/size |
| `ProgressSnapshot` | bounded determinate values or explicit indeterminate | percentage outside range, completed > total, fake percentage while indeterminate |
| `CheckpointDescriptor` | compatible exact operation facts | wrong input/schema/engine/stage/integrity, expired checkpoint |
| `ExecutionResult` | validated final output or safe failure | success without validated output; failure exposing exception/path/content |
| `ConfirmationContext` | exact action/resource/policy/version/freshness | stale confirmation, wrong target/action, inferred confirmation |

## 2.4 Result semantics

Unit tests shall distinguish these concepts:

```text
Accepted       ≠ Completed
Temporary      ≠ Final
Generated      ≠ Validated
Validated      ≠ Published
Cancelled      ≠ Failed
Waiting        ≠ Failed
Retryable      ≠ Automatically retried forever
Partial batch  ≠ Successful atomic Merge
```

For Merge, `PARTIAL_SUCCESS` is invalid because API §13.6 defines Merge as atomic: if a required input/stage fails, the operation fails and no partial merged file is final.

---

# 3. Application-test strategy

## 3.1 Application-service test harness

Application tests instantiate the real handler/service under test with deterministic fakes for:

- Feature Registry snapshot;
- authorization decision already expressed as a typed result where authorization implementation is outside the use case;
- policy/classification/capability/connectivity/resource snapshots;
- input metadata/stream gateway;
- Engine Resolver or approved engine port;
- local/server Job repository abstraction;
- temporary/result publisher;
- clock, ID generator, and Correlation ID accessor;
- History/Audit intent collectors;
- safe logger/telemetry sink.

No fake may return “allow all” implicitly. Builders should default to the smallest valid User-owned, local, enabled, low-risk case, and each privileged/server capability must be explicitly enabled in the test.

## 3.2 Command/handler behavior matrix

| Use case | Success assertions | Failure/negative assertions |
|---|---|---|
| Execute Feature | Canonical descriptor loaded; typed input/schema valid; execution plan returned; accepted/terminal evidence consistent; result grounded | Unknown/disabled/unexposed Feature, bad schema/input/policy/resource stops before execution |
| Execute immediate local | Validated final result returned with Feature ID and Correlation ID | No Job ID invented; no server/upload boundary invoked |
| Start background local | Durable/local Job model returns Job ID and `QUEUED`; execution remains local | Background does not call server API merely because it has a Job |
| Start server background | Requires eligible server path, online current authorization, consent, capacity; returns durable accepted Job | No `202`/accepted result before persistence intent; no transfer on missing consent |
| Cancel Job | Valid current state moves/request-moves toward `CANCELLED` at safe point | Final/not-cancellable state returns current state or safe conflict; result is not relabeled cancelled |
| Retry Job | Transient/eligible failed attempt creates a new attempt under same logical Job ID | Permanent validation error, exhausted limit, or incompatible contract is not retried |
| Resume Job | Compatible checkpoint resumes at supported stage | Incompatible/expired checkpoint returns canonical conflict/expiry behavior; no silent restart |
| Finalize/Publish Result | Valid temp output atomically becomes final after confirmation when required | Invalid/unconfirmed/conflicting output remains non-final; source unchanged |
| Reprocess from History | Current Feature/schema/permission/policy/input/capability checks rerun | Historical success does not bypass current disablement or stale schema |
| Query Feature availability | Returns only visible compatible projection and reason state | Query does not mutate state or imply execution permission/server handler |
| Query Job/result | Current authoritative state/version projected safely | SignalR/cache event cannot overwrite newer state or expose raw checkpoint/path |

## 3.3 Observable interaction rules

It is appropriate to assert a gateway was not used when that is the requirement, for example:

- invalid parameters → merge engine receives no request;
- local-only Merge → server execution/upload client receives no request;
- output validation failure → final publisher receives no publish request;
- missing confirmation → irreversible publisher receives no request;
- disabled Feature → no engine/provider lookup;
- failed Audit-intent durability for an action whose transaction class requires it → action does not finalize.

Do not assert irrelevant call counts, exact private ordering, or concrete repository methods. Assert the business boundary: no execution, no transfer, no publication, no state regression, and no unsafe evidence.

## 3.4 Command/query parity

File-First, Tool-First, and Assistant-First must construct the same canonical command. Data-driven parity tests shall vary only `Origin` and prove identical:

- Feature ID and parameter validation;
- permission/policy/capability decision;
- confirmation requirement;
- resolver result;
- error/result state;
- Correlation ID behavior;
- Audit actor (actual User) with `Origin=ASSISTANT` only for Assistant.

Assistant-proposed Feature IDs, parameters, paths, provider names, and results remain untrusted. A handler test must reject an invented/hidden Feature or extra parameter exactly as a direct UI/API request would.

## 3.5 Evidence and internal-event behavior

Tests should inspect typed intents/facts, not persistence tables:

| Situation | Expected intent |
|---|---|
| Core execution admitted | `Processing.ExecutionAccepted` L2 with real User, Origin, Feature ID, Correlation ID, and Job ID when a Job exists |
| Core execution succeeds | `Processing.ExecutionCompleted` L2 only after output validation/final outcome |
| Core execution fails/cancels | `Processing.ExecutionFailed` or `Processing.ExecutionCancelled` L2 with safe Error/Reason Code |
| Protect/Unlock/Visual Sign/Redact | Core L2 plus applicable L3 finalization/security evidence |
| Progress update | Domain/Activity notification only; no Enterprise Audit event |
| Assistant Merge | User remains Actor; `Origin=ASSISTANT`; no independent Assistant permission/result |

Audit-intent tests must verify absence of document bytes/text, path/name by default, password, token, clipboard, signature asset, OCR/full extracted text, raw exception, provider secret, and arbitrary metadata.

---

# 4. Validator tests

## 4.1 Layered input validation

| Validator area | Required tests | Expected behavior |
|---|---|---|
| Envelope/command | null request, missing required field, malformed identifier, duplicate logical input ID | Reject as malformed; no domain execution |
| Feature Registry | canonical active ID, unknown ID, disabled/hidden/deprecated ID, alias input | Only canonical active visible Feature proceeds; alias must be resolved before execution |
| Schema | exact supported `ParameterSchemaVersion`, missing version, unsupported version, unknown field, wrong type, out-of-range value | Reject before engine; use typed safe issues; never pass arbitrary property bag |
| Input count | minimum, maximum, zero, one, at limit, over limit | Enforce Feature/policy count; no integer overflow |
| File type | extension/MIME/signature agree; each mismatch; unsupported format | Only compatible actual bytes/file status proceed |
| File state | `READY`, `PASSWORD_REQUIRED`, `WARNING`, `UNSUPPORTED`, `CORRUPTED`, `SECURITY_REVIEW`, `BLOCKED`, `INSUFFICIENT_STORAGE` | Only `READY` proceeds; a policy-approved warning flow remains explicit and never mutates the input list |
| Structural safety | unreadable, zero length, truncated, malformed structure, page/decompression/object limit | Return safe status/error; no engine execution |
| Ownership/reference | own authorized opaque ref, wrong owner, stale version, missing/access-revoked/changed local identity | Deny/revalidate; path/ref is not authority |
| Resource preflight | enough/just-enough/insufficient memory, temp storage, output capacity, queue slot | Insufficient resource waits/fails safely before expensive work |
| Connectivity/consent | local path, online approved server path, offline server-needed, missing upload confirmation | Local unaffected; server-needed waits/fails; no silent upload |
| Confirmation | reversible edit, final export, overwrite, permanent/high-risk action, stale/wrong confirmation | Routine edit proceeds without dialog; required final/risky action cannot finalize without exact valid confirmation |
| Output | correct type/structure/size/page count/digest/security property; corrupt/wrong/empty/over-limit output | Publish only validated output; otherwise `RESULT_VALIDATION_FAILED` or the governed code |

## 4.2 Parameter-schema tests

Each released Feature schema should produce generated/data-driven tests for:

1. required fields;
2. optional fields and defaults;
3. allowed enum values and casing;
4. numerical/string/list bounds;
5. mutually exclusive/dependent fields;
6. unknown fields (`additionalProperties=false` behavior);
7. schema-version compatibility/migration;
8. malicious path/URL/engine/provider/SQL/shell/class/assembly fields;
9. stable safe field/issue codes;
10. round-trip of the normalized command without content leakage.

Schema tests must not merely snapshot JSON. They must prove that the normalized typed command carries the intended meaning and rejects unsafe/ambiguous values.

## 4.3 Confirmation classifier

Data-driven domain tests:

| Action | Default expectation |
|---|---|
| Reorder/remove input from working list | No confirmation; reversible state update |
| Rotate/crop/edit draft/undo/redo | No confirmation |
| Cancel before finalization | No destructive confirmation; preserve source |
| Send document content to server | Confirmation/notice as policy requires before transfer |
| Final export/download | Confirmation as locked Smart Communication rule requires |
| Overwrite exact source/target | Exact target confirmation plus separate permission/context |
| Share | Exact target confirmation |
| Permanent delete | Confirmation; reauthentication per applicable policy |
| Protect/Unlock/Visual Sign/Redact finalization | Feature-specific confirmation; reauthentication remains a separate policy decision |

A generic boolean `confirmed=true` is insufficient for sensitive behavior. Unit tests should bind confirmation to principal/session, action, resource/target, policy/version, and freshness where the domain contract requires it.

## 4.4 Validator composition

Validator order is not tested as a private call sequence, but these externally visible guarantees are:

- cheap/typed invalid input cannot trigger expensive parsing/engine work;
- authorization/ownership and policy occur before content access/transfer;
- an inconclusive/unavailable required security inspection never becomes `READY`;
- only a validated input/reference may reach the engine;
- only a validated output may reach final publication;
- all failures return a safe issue/error without raw parser/provider exception or content.

---

# 5. State-machine tests

## 5.1 Canonical states

The source-of-truth Job state set is:

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

`RETRYING` is required by Documents 04 and 06 even though the prompt's abbreviated list omitted it. `PAUSED` is explicitly not a valid baseline state.

## 5.2 Valid and invalid transition specification

| From | Allowed next state(s) | Explicitly invalid direct transition examples |
|---|---|---|
| New/accepted | `QUEUED` | Direct `RUNNING`, `COMPLETED`, `FAILED` without accepted Job lifecycle |
| `QUEUED` | `RUNNING`, `WAITING_CONNECTION`, `WAITING_RESOURCE`, `CANCELLED` | `COMPLETED`, `FAILED`, `RETRYING` |
| `RUNNING` | `WAITING_CONNECTION`, `WAITING_RESOURCE`, `RETRYING`, `COMPLETED`, `FAILED`, `CANCELLED` at safe point | `QUEUED` without retry/reschedule semantics |
| `WAITING_CONNECTION` | `QUEUED`, `RUNNING`, `RETRYING`, `CANCELLED` | Direct `COMPLETED` or `FAILED` without returning to execution/attempt evaluation |
| `WAITING_RESOURCE` | `QUEUED`, `RUNNING`, `RETRYING`, `CANCELLED` | Direct `COMPLETED` without execution; resource wait cannot claim output |
| `RETRYING` | `QUEUED`, `RUNNING`, `CANCELLED` | `COMPLETED` without a running attempt; self-loop that bypasses bounded backoff/attempt count |
| `FAILED` | `RETRYING` only through a valid retry/resume command | `RUNNING`, `COMPLETED`, `CANCELLED` by arbitrary update |
| `COMPLETED` | No business transition | Any transition including `CANCELLED`, `FAILED`, `RUNNING` |
| `CANCELLED` | No business transition | Any transition including `RUNNING`, `COMPLETED`, `FAILED` |

Where the underlying implementation chooses a more restrictive transition than the minimum source set, that restriction must be explicit and contract-tested. It may not add `PAUSED`, regress a terminal state, or bypass `RETRYING` eligibility.

## 5.3 Transition test set

For every allowed transition, assert:

- state changes exactly once under expected `stateVersion`;
- a new state version/revision is produced;
- timestamps and attempt fields are coherent;
- the emitted state fact reflects the committed state;
- no output becomes final except on valid `RUNNING → COMPLETED` after result validation;
- cancellation honours a safe point and does not relabel an already completed result;
- Correlation ID, Feature ID, and logical Job ID remain stable.

For every invalid transition, assert:

- typed conflict/invalid-transition result;
- state/version/timestamps/result remain unchanged;
- no terminal/outbox/notification fact falsely claims success;
- no raw state-store exception escapes.

## 5.4 State invariants

| Invariant | Test assertion |
|---|---|
| `StartedAt` | Set when execution first becomes `RUNNING`; not rewritten on ordinary status read |
| `CompletedAt` | Non-null only for terminal outcome according to the implemented contract; never on waiting/retrying |
| `COMPLETED` | Requires validated available output for operations that produce output; Error is null/safe warnings only |
| `FAILED` | Carries safe Error Code plus retryable/resume-available decision; no final output |
| `CANCELLED` | No further progress/final publication; temp cleanup intent present where applicable |
| Waiting states | Preserve accepted Job and inputs; do not increment successful completion or invent output |
| Attempts | Retry creates/advances attempt history but retains logical Job ID |
| State version | Stale expected version fails; no silent last-write-wins |
| Terminal idempotency | Duplicate completion/cancel messages do not regress or duplicate semantic outcome |

## 5.5 Progress tests

- determinate percentage is within `0..100`;
- completed item count is non-negative and does not exceed total;
- unknown total uses `indeterminate=true`, not a fabricated percentage/ETA;
- terminal `COMPLETED` reports the final coherent progress snapshot where applicable;
- progress cannot change `FAILED`, `COMPLETED`, or `CANCELLED` back to non-final;
- progress coalescing may reduce update count without losing current authoritative state;
- a retry may start a new attempt progress scope; tests must not impose a false global monotonic percentage across attempts;
- progress, page/item updates, Worker heartbeat, and SignalR notification produce no Enterprise Audit event.

---

# 6. Resolver tests

## 6.1 Resolver input contract

The resolver consumes typed facts, never user-selected engine/provider names:

```text
Feature descriptor/status/exposure/schema
+ platform and device capability
+ permission and resource ownership result
+ enterprise policy/classification
+ file count/size/page and resource budget
+ approved engine/provider health
+ online/offline and server availability
+ consent/confirmation state
= Execute Local / Local Worker / Server Worker / Wait / Deny / Explain
```

Unit tests pass snapshots/fakes. They do not call a real health endpoint, network, Worker, or engine.

## 6.2 Decision table

| ID | Context | Expected decision |
|---|---|---|
| `RES-001` | Local engine available, capable, healthy, approved; policy allows; resource sufficient | Select local; never consult a server path as a shortcut |
| `RES-002` | Local engine available but workload exceeds safe in-process threshold; approved Local Worker available | Select background local/Local Worker; background does not imply server |
| `RES-003` | Local unavailable; Feature server dependency `OPTIONAL`; online; server healthy; policy/permission/capacity/consent pass | Select approved server path |
| `RES-004` | Local unavailable; server optional; offline | Return `WAITING_CONNECTION`/safe online-required decision; no upload, no invented local result |
| `RES-005` | Server-required step and offline | Wait/queue typed safe intent where permitted; otherwise `ONLINE_REQUIRED`; never execute partially as success |
| `RES-006` | Local-only Feature and local engine unavailable | Use another approved local provider if declared and healthy; otherwise `WAITING_RESOURCE` or safe unavailable result; never call processing API |
| `RES-007` | Selected provider unhealthy; approved compatible fallback exists | Select only the registered approved fallback and preserve contract/schema/security properties |
| `RES-008` | Provider unhealthy; no approved fallback | Wait/fail with safe retry guidance according to transient classification; never dynamically load or use public/unapproved provider |
| `RES-009` | Server path technically available but policy/classification denies transfer | Deny server route; use local only if independently capable/allowed; no policy downgrade |
| `RES-010` | Server path needs content consent but consent absent/stale/wrong target | Return confirmation-required decision; no transfer |
| `RES-011` | Feature disabled/flag off/not exposed/incompatible | Do not resolve engine; return unavailable/denied/update-required explanation |
| `RES-012` | Local memory/storage insufficient but server optional | Server is allowed only after all normal policy/consent checks; lack of local resource alone is not implicit upload consent |
| `RES-013` | Server queue/capacity unsafe | `WAITING_RESOURCE`, `429`, or `503` semantics as the owning boundary defines; bounded retry guidance |
| `RES-014` | AI Summarizer/Translate with local approved provider | Prefer local; no public/internal call |
| `RES-015` | AI local unavailable and approved enterprise-internal provider allowed | Use internal provider only after permission/classification/minimization/consent/capacity |
| `RES-016` | PDF to Markdown with text layer | Deterministic local extraction first; AI is not a default dependency |

## 6.3 Resolver properties

Property/data-driven assertions:

- adding a lower-priority preference cannot override a safety/security denial;
- making a local engine healthy/capable must not force server execution;
- setting connectivity to offline can never create a new server execution decision;
- setting Feature status to disabled can never preserve an executable decision;
- adding an unapproved provider never changes the selected plan;
- removing consent invalidates a content-transfer plan;
- resolver output contains a registered capability/provider key, not an arbitrary class/assembly/URL;
- identical normalized input snapshots produce the same decision and safe reason;
- Feature `FEAT-PDF-005` remains Local baseline with no processing API/SignalR dependency under API §19.

---

# 7. Error/recovery tests

## 7.1 Error mapping principles

1. Domain/application errors are typed, stable, safe, and independent of exception class text.
2. A mapper preserves the Correlation ID and produces `code`, `message`, `correlationId` at minimum for an API-facing error.
3. `400` is malformed syntax/envelope; `422` is syntactically valid but semantic/file/business validation failure.
4. Waiting/offline/resource states are not automatically failures.
5. Unexpected exceptions map to safe `500` without stack/class/path/provider/SQL/content.
6. Retryability is explicit and allowlisted; it is not inferred from an arbitrary exception string.
7. The same business error maps consistently across UI, API, Assistant, and Worker boundaries.

## 7.2 Mapping test matrix

| Condition | Domain/application outcome | Existing governed code/status where available | Retry/recovery behavior |
|---|---|---|---|
| Malformed command/envelope | Rejected before use case | HTTP `400`; safe standard error | Correct request; no automatic retry |
| Valid envelope, invalid Feature parameter/count/order | Validation failure | `FEATURE_PARAMETER_INVALID`; HTTP `422`; typed issue such as governed field issue | User corrects input |
| Password-protected input without credential | Not ready | Validation status `PASSWORD_REQUIRED`; safe semantic error | Prompt again; password never persisted |
| Unsupported actual media/signature | Rejected | `UNSUPPORTED`; HTTP `415` or `422` according to exact boundary | Choose supported input |
| Structurally corrupt PDF | Rejected | `CORRUPTED`; HTTP `422` for semantic/file validation | Repair Feature may be offered separately; no silent repair inside Merge |
| Required security inspection inconclusive/unavailable | Non-addressable/blocked | `SECURITY_REVIEW` or `BLOCKED` | Wait/escalate/retry only by approved policy |
| Insufficient local storage | Preflight stops | `INSUFFICIENT_STORAGE` validation status | Free space/change safe target; no engine invocation |
| Offline while server is needed | Wait/online-required | `WAITING_CONNECTION` or `ONLINE_REQUIRED` | Resume/revalidate after reconnect |
| Shared server/provider capacity unsafe | Wait or temporary failure | HTTP `503` with safe guidance; `429` for caller quota/concurrency | Bounded backoff; no storm |
| Output validation failure | Job/immediate operation fails; temp non-final | `RESULT_VALIDATION_FAILED` | Retry only if classified safe; source unchanged |
| Output/state/path/version conflict | No overwrite/publication | HTTP `409`, or `412` for ETag/version precondition | Refresh/reselect/explicit safe action |
| Cancellation not permitted | State unchanged | `JOB_NOT_CANCELLABLE`, HTTP `409` | Explain current state; no retry loop |
| Checkpoint incompatible | Resume denied | `CHECKPOINT_NOT_RESUMABLE`, HTTP `409` | User chooses safe restart/cancel |
| Checkpoint expired | Resume denied | `CHECKPOINT_EXPIRED`, HTTP `410` | Safe restart if current input/policy allow |
| Result not ready/expired | No content access | `RESULT_NOT_READY` / `RESULT_EXPIRED` where applicable | Poll current Job or reprocess under current rules |
| Unexpected engine/application exception | `FAILED` with normalized safe error | HTTP `500` for immediate API boundary | No retry unless exception was translated into approved transient category |

Exact codes not already governed are listed as gaps in section 11 and must not be guessed differently by each module.

## 7.3 Recovery behavior tests

| Scenario | Required behavior |
|---|---|
| Application closes before temp output validation | Recovery finds candidate; output remains temporary/non-final |
| Local Worker crashes | UI/application host remains alive; Job becomes safe failed/recovery candidate; source unchanged |
| Server Worker crashes | Persisted Job remains authoritative; bounded retry/checkpoint applies |
| Network drops during local work | Local-capable work continues; no state is changed to server-required |
| Network drops before server acceptance | No false accepted Job; retry uses same Idempotency Key where applicable |
| Network drops after durable acceptance | Reconcile current Job by REST using returned/known Job ID; no duplicate logical Job |
| Resume-compatible checkpoint | Continue from approved stage; same logical Job, new/continued attempt semantics |
| Resume-incompatible checkpoint | Reject with safe conflict; never silently restart from the beginning |
| Retry limit reached | Remain `FAILED`; no new attempt/hot loop |
| Result validation fails | No final result/history success; failed evidence emitted; temp cleanup/reconciliation requested |
| Cleanup fails | Business result state does not falsely claim secure erase; cleanup remains bounded/reconcilable |
| Correlation ID absent | Generate once at entry; propagate to result/error/evidence |

## 7.4 Error-safety assertions

Use canary values in fake exceptions and input metadata, then prove the returned result, log event, History/Audit intent, and notification contain none of:

- document/PDF bytes or extracted text;
- full filename/path/storage key/arbitrary URL unless a narrowly approved local projection explicitly owns it;
- password/token/cookie/key/secret;
- clipboard/screenshot/signature asset/redacted content;
- raw SQL, stack trace, exception type/message, engine command line, provider endpoint, or queue topology.

---

# 8. Feature-family strategy

## 8.1 Shared contract suite

Every user-facing Feature family reuses one parameterized contract suite:

```text
canonical active Feature
→ typed schema validation
→ compatible authorized input
→ policy/capability/resource decision
→ confirmation when applicable
→ approved execution port
→ result validation
→ safe final result/error
→ correlation/history/audit facts
```

The suite receives a `FeatureTestDescriptor`, representative valid/invalid inputs, and family-specific assertions. It prevents thousands of copy-pasted cases while retaining a dedicated suite for high-risk and algorithm-specific rules.

## 8.2 Reusable family patterns

| Family | Shared unit-test patterns | Family-specific negative cases |
|---|---|---|
| PDF Page Operations | Ordered page/input model, range normalization, non-destructive working state, undo/redo, atomic output, source preservation | Empty/out-of-range/overlapping range policy, duplicate page/input, removed-last-required item, corrupted/encrypted input |
| Conversion | Input/output media compatibility, provider abstraction, local/optional-server plan, fidelity capability statement, output type/structure validation | Wrong MIME/signature, active-content policy, unsupported format/layout, feasibility-gated provider unavailable, wrong output media |
| OCR | Text-layer detection, OCR-needed decision, page/language/resource bounds, local-first, checkpointable page stages, output treated as untrusted restricted data | Pixel/page bomb, unsupported language, partial text without valid output, full OCR text leakage, provider timeout |
| Editing | Operation-based working state, reversible edits without confirmation, preview/change summary, undo/redo branch, final publish separate from edit | Stale operation cursor, redo after new branch, source identity changed, existing-text capability gated |
| Security Tools | Separate permission, exact confirmation, source unchanged, new result validated, L2 Core plus L3 security evidence | Password persistence, unauthorized Unlock, overlay-only Redact, invalid protection result, inferred confirmation/bypass |
| Forms | Typed field operation, working-state behavior, preview, no form value in logs/evidence, final flatten/output validation where supported | Unknown field/type, stale form structure, script/action field, sensitive value leakage, unsupported flatten |
| Signature | Visual non-PKI invariant, explicit asset selection/placement/preview/finalization, protected local asset reference, source unchanged | Claiming PKI/non-repudiation, auto-save/upload, asset bytes in result metadata/evidence, stale/wrong asset/profile |
| Intelligence | Approved local/internal provider only, deterministic preprocessing, minimization, explicit content scope/consent, untrusted output, result grounding | Public endpoint, prompt injection, invented Feature/parameter/result, provider response with tool/HTML/URL control, content/prompt leak |

## 8.3 Data-driven dimensions

Use combinatorial coverage with risk-based pairwise datasets, not Cartesian explosion:

- platform exposure: Desktop/Tablet/Phone/Web;
- execution: local immediate/local background/optional server;
- connectivity: online/offline/reconnect;
- capability: capable/unavailable/unhealthy/incompatible;
- policy: allow/deny/limit/confirmation;
- file state: ready/warning/password/unsupported/corrupt/blocked/low storage;
- state: new/queued/running/waiting/retrying/final;
- origin: Tool-First/File-First/Assistant;
- schema: current/supported-old/unsupported/unknown-field;
- outcome: success/failure/cancel/wait.

Always keep explicit high-risk negative cases outside pairwise reduction.

## 8.4 Registry-wide tests

One generated/parameterized suite should assert across all 550 descriptors:

- Feature ID is unique, canonical, case-stable, and never blank;
- aliases do not collide with canonical IDs or become execution IDs;
- record type, input/output contract, execution profile, risk, dependency family, status, schema version, build priority, and platform exposure are present/valid;
- server dependency and processing modes are coherent;
- destructive/risk/special permission metadata is coherent;
- active server-executable Feature has a typed handler/authorization/result validator contract;
- 33 card mappings resolve to the exact canonical IDs from Documents 03/06/09/10;
- the 19 Local-baseline Core cards do not acquire a processing API;
- all 33 Core cards have L2 accepted/terminal Audit mapping; high-risk cards have their L3 mapping;
- changing schema, input/output contract, permission, destructive flag, or server dependency triggers a regression/impact gate.

---

# 9. Merge PDF complete unit-test specification

## 9.1 Canonical contract under test

| Attribute | Baseline |
|---|---|
| Feature ID | `FEAT-PDF-005` |
| Card | Merge PDF; direct Core card #1 |
| Input/output | Two or more ordered PDFs → one PDF |
| Record/risk | `USER_TOOL`, `PRIMARY_TOOL`, `LOW` |
| Execution | Profile `L1`; Local baseline; no processing API/SignalR dependency |
| Permission | Ordinary transformation uses `document.content.process` over authorized inputs/own result |
| Editing | Reorder/remove changes working state and is reversible/no routine confirmation |
| Finalization | Validate temp output, then Save As/new result; never silently overwrite source |
| Failure semantics | Atomic operation; no partial success when any required input/stage fails |
| Trace | Correlation ID always; Job ID only when background local Job exists |
| Evidence | Safe L2 accepted + terminal evidence; no document content/path/password/raw engine error |

The concrete Merge JSON parameter schema is not fully enumerated in Documents 01–10. Tests below use the documented `orderedInputIds` concept and mark missing exact field/code decisions in section 11 rather than inventing production contract silently.

## 9.2 Input-list and working-state cases

| Test ID | Behavior specification |
|---|---|
| `UT-MERGE-001` | **No files:** command is invalid because Merge requires at least two PDFs; returns `FEATURE_PARAMETER_INVALID`/typed minimum-count issue; engine, Job creation, and final publisher are not invoked |
| `UT-MERGE-002` | **One file:** same minimum-count failure; input is not copied or transformed as a fake “merge” |
| `UT-MERGE-003` | **Two valid PDFs:** normalized command preserves both IDs and produces one validated PDF result |
| `UT-MERGE-004` | **Multiple valid PDFs:** all inputs participate exactly once per ordered occurrence and result page count equals the sum reported by trusted fake outputs |
| `UT-MERGE-005` | **Initial input ordering:** engine request follows `orderedInputIds`, not filename, selection timestamp, hash, or repository order |
| `UT-MERGE-006` | **Reorder:** changing `[A,B,C]` to `[C,A,B]` updates working state without confirmation and changes the observable merged page order accordingly |
| `UT-MERGE-007` | **Remove input with two remaining:** removing B from `[A,B,C]` leaves `[A,C]`, remains valid, requires no confirmation, and does not delete/modify B |
| `UT-MERGE-008` | **Remove input leaving one:** working state may reflect the reversible edit, but execution becomes invalid and no engine call occurs until at least two inputs exist |
| `UT-MERGE-009` | **Duplicate file:** two explicit occurrences of the same authorized PDF are never silently deduplicated; duplicate detection may return a safe warning, but order/occurrence is preserved unless the governed schema later says otherwise |
| `UT-MERGE-010` | **Unknown ordered ID:** an order list referencing an absent input is rejected; no best-effort reorder or dropped item |
| `UT-MERGE-011` | **Duplicate logical input ID with different refs:** reject ambiguous/malformed command; an occurrence ID must identify one input deterministically |
| `UT-MERGE-012` | **Policy count/page/size limit:** values at the configured limit pass; above limit fail before engine; limits come from policy fixture, not hardcoded test folklore |

## 9.3 File-validation and encryption cases

| Test ID | Behavior specification |
|---|---|
| `UT-MERGE-013` | Extension says PDF and MIME/signature/structure agree → input can become `READY` |
| `UT-MERGE-014` | Extension/MIME says PDF but signature is not PDF → `UNSUPPORTED`/typed validation failure; engine not invoked |
| `UT-MERGE-015` | **Invalid/corrupted PDF:** any required input with invalid structure becomes `CORRUPTED`; Merge fails atomically with no final/partial result |
| `UT-MERGE-016` | Required security inspection reports blocked/inconclusive → input remains non-addressable; no merge execution |
| `UT-MERGE-017` | **Encrypted input without password:** status `PASSWORD_REQUIRED`; command does not execute |
| `UT-MERGE-018` | **Encrypted input with valid ephemeral password:** validation/engine may proceed; password is absent from normalized persisted command, checkpoint, evidence, result, log, and error |
| `UT-MERGE-019` | **Wrong password:** safe validation failure/password-required state; no raw password or provider detail; retry requires user input and is bounded against abuse |
| `UT-MERGE-020` | Mixed batch with one invalid/encrypted-unresolved input and other valid inputs → atomic Merge does not process only the valid subset as success |
| `UT-MERGE-021` | Input identity changes after preflight and before execution → conflict/revalidation; never merge stale bytes under the prior decision |

## 9.4 Resolver and resource cases

| Test ID | Behavior specification |
|---|---|
| `UT-MERGE-022` | **Local processing mode:** capable/healthy approved local engine + sufficient resources always selects local; no server-processing API, upload, or SignalR call |
| `UT-MERGE-023` | Large Merge exceeds safe in-process threshold but fits approved Local Worker budget → background local Job, not server execution |
| `UT-MERGE-024` | **Engine unavailable with another approved compatible local engine:** resolver selects approved local fallback and preserves the same Merge contract |
| `UT-MERGE-025` | **All approved local engines unavailable:** return `WAITING_RESOURCE` or governed safe unavailable failure; because Merge is Local baseline, never invent an optional server path |
| `UT-MERGE-026` | Offline with local capability available → execution remains local; offline does not disable Merge |
| `UT-MERGE-027` | **Low disk:** preflight returns `INSUFFICIENT_STORAGE`; no engine/temp/final publication starts |
| `UT-MERGE-028` | Exactly sufficient configured temp/output capacity passes; arithmetic overflow/negative/free-space anomaly fails closed |
| `UT-MERGE-029` | Feature disabled, not exposed, schema-incompatible, or permission denied → no engine resolution/execution and a safe reason is returned |

## 9.5 Cancellation, failure, and recovery cases

| Test ID | Behavior specification |
|---|---|
| `UT-MERGE-030` | **Cancelled while queued:** transition to `CANCELLED`, no engine run, no final output, safe cancelled evidence |
| `UT-MERGE-031` | **Cancelled while running:** cooperative cancellation stops at safe point, temp output is non-final/cleaned or reconciled, source files remain unchanged |
| `UT-MERGE-032` | Cancellation requested after `COMPLETED` does not regress state or delete/relabel the validated result |
| `UT-MERGE-033` | Engine throws a governed transient failure → normalized retryable result; bounded retry policy applies, raw exception does not escape |
| `UT-MERGE-034` | Engine throws permanent/validation failure → `FAILED`, not automatically retried, no final result |
| `UT-MERGE-035` | Retry limit reached → remains `FAILED`; no new attempt/hot loop |
| `UT-MERGE-036` | Compatible checkpoint exists for a background local Merge only if the chosen engine/contract supports it; otherwise resume is not falsely advertised |
| `UT-MERGE-037` | Incompatible/expired checkpoint → `CHECKPOINT_NOT_RESUMABLE`/`CHECKPOINT_EXPIRED` behavior; no silent restart from input zero |

## 9.6 Output and source-protection cases

| Test ID | Behavior specification |
|---|---|
| `UT-MERGE-038` | **Output validation succeeds:** output has approved PDF signature/structure, expected non-zero pages/size bounds, and safe integrity metadata before publication |
| `UT-MERGE-039` | **Output validation fails:** return `RESULT_VALIDATION_FAILED`, state `FAILED`, temp not published, no success History/Audit result, originals unchanged |
| `UT-MERGE-040` | Engine returns wrong media type/empty output/truncated PDF → same validation-failure behavior; engine “success” alone is not application success |
| `UT-MERGE-041` | **Output-path conflict:** existing target cannot be replaced by default; return conflict or require the separate exact overwrite contract/permission/confirmation |
| `UT-MERGE-042` | **Source overwrite prevention:** target matching any source is rejected for the baseline Save As flow; no source stream is opened for destructive write |
| `UT-MERGE-043` | Required final confirmation absent/stale/wrong target → validated temp may remain recoverable by policy, but no final publication occurs |
| `UT-MERGE-044` | Valid exact finalization uses temp → close/flush → validate/checksum → atomic publication abstraction → commit result pointer; test asserts final visibility only after success, not private call order |
| `UT-MERGE-045` | Publication failure leaves no half-final result; original and pre-existing target remain safe; reconciliation/cleanup intent is returned |

## 9.7 Identity, state, progress, and result-model cases

| Test ID | Behavior specification |
|---|---|
| `UT-MERGE-046` | **Provided Correlation ID propagation:** exact opaque ID appears in execution plan, Job when any, result/error, safe log, History/Audit intents; it never grants access |
| `UT-MERGE-047` | **Missing Correlation ID:** application creates one once and propagates/returns it consistently |
| `UT-MERGE-048` | Immediate local success returns `featureId=FEAT-PDF-005`, `status=COMPLETED`, validated primary output, schema/contract version where modeled, and Correlation ID; no fake Job ID |
| `UT-MERGE-049` | Background local acceptance returns stable Job ID, `QUEUED`, Feature ID, Correlation ID, and status lookup reference/model; acceptance is not completion |
| `UT-MERGE-050` | **Completion state:** Job reaches `COMPLETED` only from `RUNNING` after output validation/publication contract; `CompletedAt` set and Error absent |
| `UT-MERGE-051` | **Failure state:** Job reaches `FAILED` with safe code/retryability and no primary final output; `CompletedAt`/terminal timestamp follows the chosen locked model |
| `UT-MERGE-052` | Progress uses page/item totals when known, otherwise indeterminate; cancellation capability is truthful; terminal state cannot regress from stale progress |
| `UT-MERGE-053` | Result/evidence contains no PDF bytes, password, raw path/storage key, raw engine/provider exception, or unnecessary filename/content |

## 9.8 Cross-entry and evidence cases

| Test ID | Behavior specification |
|---|---|
| `UT-MERGE-054` | Tool-First, File-First, and Assistant-First normalize to the same Merge command and outcome for the same inputs/context |
| `UT-MERGE-055` | Assistant-origin execution retains User as Actor and records `Origin=ASSISTANT`; Assistant cannot supply engine/provider or declare success |
| `UT-MERGE-056` | Accepted Merge produces one safe L2 accepted intent and terminal success/failure/cancel produces the correct L2 terminal intent with the same Correlation ID |
| `UT-MERGE-057` | Progress, reorder, remove, preview, and ordinary validation updates do not create Enterprise Audit event noise |
| `UT-MERGE-058` | Retried delivery of the same Audit intent/event identity is duplicate-safe; it does not represent a second Merge business outcome |
| `UT-MERGE-059` | Personal History records safe user-facing completion/failure metadata separately; deleting History cannot delete Enterprise Audit evidence |

## 9.9 Merge acceptance summary

The Merge unit suite passes only when:

```text
invalid input never reaches an engine
valid ordered inputs remain ordered
reversible list edits require no confirmation
no input/source is silently changed
Merge remains local baseline
background local is not treated as server
no server transfer occurs
temporary/invalid output is never final
cancel/failure/retry state is coherent
Correlation ID is stable
result/error is safe
all Core L2 accepted/terminal evidence is produced
```

---

# 10. Reusable fixtures/builders

## 10.1 Recommended test objects

| Fixture/builder/fake | Purpose and safe default |
|---|---|
| `FeatureDescriptorBuilder` | Active `FEAT-PDF-005`, current schema, Desktop exposure, `L1`, Local, LOW, non-destructive; overrides explicit |
| `FeatureRegistryFixture` | Immutable canonical snapshot; supports unknown/disabled/alias/collision cases |
| `FileInputBuilder` | Opaque input ID/ref, PDF MIME/signature, page/size metadata, `READY`, owner scope; no real path required |
| `OrderedInputsBuilder` | Creates stable occurrence IDs and explicit order; supports duplicate occurrence without ambiguous ID |
| `PolicySnapshotBuilder` | Smallest valid limits and allow decisions; server transfer/AI/overwrite disabled unless explicitly enabled |
| `CapabilitySnapshotBuilder` | Platform, local engine, Local Worker, connectivity, server/provider health and resource facts |
| `AuthorizationDecisionBuilder` | Explicit permission/ownership/policy result; no implicit wildcard grant |
| `ConfirmationContextBuilder` | Principal/action/resource/target/policy version/freshness-bound confirmation |
| `JobBuilder` | Valid state, attempt, state version, timestamps, Correlation ID; can create each canonical state |
| `CheckpointBuilder` | Input/Feature/schema/engine/stage/integrity/expiry fields with one-field mismatch helpers |
| `ValidatedArtifactBuilder` | Temporary PDF result with validation facts; invalid variants for empty/wrong/corrupt/over-limit output |
| `DeterministicMergeEngineFake` | Produces an output page-token sequence equal to ordered input page tokens; configurable cancel/failure/invalid output |
| `EngineCatalogFake` | Approved compatible local providers and health; never dynamic arbitrary provider |
| `ArtifactStoreFake` | In-memory staging/final publication semantics; records prohibited publication, conflict, and cleanup intent |
| `JobStoreFake` | In-memory aggregate persistence with expected-version conflict behavior; not an EF/SQL imitation |
| `EvidenceCollectorFake` | Captures typed History/Audit/domain facts and runs sensitive-field canary checks |
| `SafeLoggerFake` | Captures structured fields only and rejects forbidden values |
| `FakeClock` | Fixed/advanceable UTC time for retry, expiry, timestamps, confirmation/checkpoint freshness |
| `DeterministicIdGenerator` | Stable Job/Correlation/Audit IDs; supports collision/invalid tests |
| `CancellationHarness` | Cancels before execution or at an explicit fake-engine safe point |

## 10.2 Builder rules

- Default objects are valid, minimal, owned, local, and deterministic.
- A test changes only facts relevant to the behavior under examination.
- Privileged, server, provider, upload, overwrite, destructive, and reauthentication capability defaults to denied/off.
- Builders validate themselves so an invalid fixture is intentional and named.
- Do not hide essential scenario facts behind an opaque “ValidEverything” helper.
- Do not reuse mutable aggregate instances across tests.
- Use synthetic page tokens/metadata, never Production files or confidential content.

## 10.3 Data sources

Recommended shared data sets:

```text
CanonicalJobTransitions
InvalidJobTransitions
ValidationStatusCases
ResolverDecisionCases
ConfirmationClassificationCases
ErrorMappingCases
Core33CanonicalMappings
FeatureSchemaBoundaryCases
ForbiddenEvidenceCanaries
```

Data sources should return domain values, not dependency mocks. Family-specific assertions remain near the family suite.

## 10.4 Determinism and isolation

- no system clock, random GUID, environment variable, machine path, installed Office/PDF engine, current locale, real network, or real database dependency;
- cancellation and retry timing are driven by fake clock/scheduler, never arbitrary `Task.Delay`;
- parallel-safe tests own all mutable fixtures;
- culture-sensitive messages are not asserted as authorization/error identity; assert code/resource key and safe arguments;
- tests run under MTP locally and in Azure DevOps with the same result.

---

# 11. Risks/gaps

| ID | Risk/gap | Impact on tests | Recommendation |
|---|---|---|---|
| `UT-RISK-001` | Prompt state list omits `RETRYING`, while Documents 04/06/07 require it | An incomplete test machine would reject valid retry flows | Keep `RETRYING`; explicitly test that `PAUSED` is invalid |
| `UT-RISK-002` | Exact `FEAT-PDF-005` parameter JSON schema/version is not fully enumerated in Documents 01–10 | Field names/defaults/error paths could be guessed inconsistently | Lock the Merge schema before implementation; generate validator tests from the governed artifact plus hand-written semantic tests |
| `UT-RISK-003` | Exact canonical error-code catalog is incomplete for input-count, engine-unavailable, output-path conflict, source-changed, and low-disk application errors | Modules may invent different codes for the same behavior | Document 11/implementation foundation should define one safe error catalog; until then assert category/state and only existing governed codes |
| `UT-RISK-004` | Duplicate-file semantics are not explicitly locked | Silent dedupe, reject, or duplicate pages could differ across clients | Baseline recommendation: never silently dedupe; preserve explicit occurrences and optionally warn. Obtain product confirmation if duplicate occurrence should require acknowledgement |
| `UT-RISK-005` | Immediate versus Local Worker thresholds for Merge are benchmark/policy dependent | Brittle tests could hardcode file/page sizes | Test threshold behavior using injected policy/capability values; numeric thresholds belong to benchmark/Test Plan/Document 12 |
| `UT-RISK-006` | Final export/download confirmation is locked broadly, but the exact boundary between “validated temp result” and “Save As publication” needs a concrete application contract | Tests could request confirmation too early or publish without it | Separate Execute from Finalize/Publish and bind confirmation to the final target/action |
| `UT-RISK-007` | Approved local provider fallback compatibility/health taxonomy is not enumerated | Resolver tests may encode vendor-specific assumptions | Use capability keys and compatibility facts only; provider catalog is injected and approval-controlled |
| `UT-RISK-008` | Progress monotonicity across stages/retry attempts is not fully specified | A global monotonic assertion may reject valid retry resets or permit stale regressions | Require coherent bounded progress and terminal non-regression; define attempt/stage progress semantics before stronger monotonic tests |
| `UT-RISK-009` | Multiple encrypted Merge inputs may need one or several ephemeral password prompts | Password association/UX may differ | Define password-to-input occurrence binding; never persist any password regardless of decision |
| `UT-RISK-010` | Output-validation depth for Merge is not fully locked | A parse-only fake may overstate correctness | Unit tests assert validation decision contract; real PDF structure/render/page-order/fidelity belongs to component/PoC corpus tests |
| `UT-RISK-011` | Document 10 FINAL requires L2 accepted/terminal evidence for every Core execution, including local Merge | Older assumptions that ordinary local Merge is History-only would miss required evidence | Treat Document 10 FINAL as authority; test protected offline Audit intent/outbox at unit/application level and ingestion in integration tests |
| `UT-RISK-012` | Audit durability failure behavior varies by transaction class and exact local outbox protection technology | A test might either lose mandatory evidence or unnecessarily block safe work | Define transaction class per event/action; unit-test the chosen policy, integration-test actual durability/recovery |
| `UT-RISK-013` | `CompletedAt` semantics for failed/cancelled terminal states need exact model naming | Timestamp assertions may diverge (`CompletedAt` versus generic terminal time) | Lock terminal timestamp fields in the Job domain contract; keep state finality behavior invariant |
| `UT-RISK-014` | Real filesystem overwrite/atomic rename and low-disk race cannot be proven in pure unit tests | Passing fakes may mask TOCTOU/partial-write behavior | Add filesystem component tests with controlled temp sandbox; keep unit tests on publisher contract |
| `UT-RISK-015` | Real SQL rowversion/idempotency/outbox guarantees cannot be proven with an in-memory fake | Concurrency/durability defects remain possible | Add EF Core/SQL integration tests separately; do not substitute EF InMemory behavior as proof |
| `UT-RISK-016` | Feature-family PoC items may not yet have an approved engine | Tests can prove contracts while real feasibility is unknown | Keep ports/fakes and contract suites; release remains gated by approved PoC, malicious corpus, fidelity, resource, and fallback evidence |

No item above authorizes a production-code shortcut, vendor selection, new Feature ID, changed business rule, or use of real Production data.

---

# 12. Suggested Document 11 text

The following text is suitable for controlled integration into the Domain & Application Unit Test section of Document 11.

## 12.1 Testing principle

PERTAMINA OTO unit tests shall verify observable domain/application behavior rather than private implementation details. A test shall express business context, requested action, returned decision/state/result/error, required evidence intent, and prohibited side effects. Private method count, helper order, and concrete collection/repository implementation shall not be assertions unless they are themselves a released contract.

MSTest is the main .NET framework and Microsoft.Testing.Platform is the main runner. Tests shall run deterministically both locally and in Azure DevOps. Unit tests shall use synthetic data and deterministic fakes; no real network, SQL Server, external provider, full UI flow, or Production document is permitted.

## 12.2 Required domain suites

Minimum domain suites shall cover:

1. canonical Feature ID and versioned parameter schema;
2. ordered input and non-destructive working state;
3. confirmation classification for reversible versus final/risky actions;
4. policy/resource limits and precedence;
5. Job state machine and state-version conflicts;
6. retry limit and checkpoint compatibility;
7. local-first execution decision;
8. validated temporary versus final result;
9. safe typed errors and Correlation ID;
10. History/Audit intent boundaries and forbidden-field protection.

## 12.3 Application-service suites

Execute/cancel/retry/resume/finalize handlers shall be tested through public commands and results. Invalid Feature/schema/input/authorization/policy/resource/confirmation shall stop before engine, transfer, or publication. A successful operation is reported only from the actual validated result. File-First, Tool-First, and Assistant-First shall normalize to the same command and receive the same authorization, validation, resolver, state, and result behavior; Assistant changes Origin only and never becomes an independent authority or engine.

## 12.4 Job state machine

Canonical states are:

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

`PAUSED` is not valid. Unit tests shall cover every allowed transition and representative/all generated invalid pairs. `COMPLETED` and `CANCELLED` never regress. `FAILED → RETRYING` requires a valid retry/resume command. A Job completes only after required output validation. Stale state version fails without silent last-write-wins.

## 12.5 Resolver

Resolver tests shall use Feature/exposure/schema, platform capability, permission/ownership, policy/classification, input/resource budget, approved provider health, connectivity/server availability, and consent facts. An approved capable local engine is preferred. Heavy local work may use a Local Worker. Local unavailable plus Optional server may use server only when online and all policy/permission/capacity/consent gates pass. Offline server-needed work waits/fails safely. An unhealthy provider may fall back only to a registered approved compatible provider. No caller/AI may select an arbitrary engine/provider/URL/class/assembly.

## 12.6 Validation, result, and error

Only `READY` inputs execute. Password, unsupported, corrupted, security-review, blocked, and insufficient-storage states remain non-executable until their governed resolution. Output is staged, closed/flushed, validated, and atomically published through the application contract. Temporary/invalid/partial output is not final.

Errors shall be typed and safe. API-facing errors contain at least `code`, `message`, and `correlationId`. Malformed input maps to `400`; semantic/file/business validation maps to `422`; state conflict to `409`; version precondition to `412`; hard limit to `413`; caller quota/concurrency to `429`; unexpected failure to safe `500`; temporary shared readiness/capacity to `503`. Existing canonical codes such as `FEATURE_PARAMETER_INVALID`, `RESULT_VALIDATION_FAILED`, `JOB_NOT_CANCELLABLE`, `CHECKPOINT_NOT_RESUMABLE`, `CHECKPOINT_EXPIRED`, and `ONLINE_REQUIRED` shall be preserved.

## 12.7 Merge PDF first vertical slice

`FEAT-PDF-005` shall have a complete unit specification covering at minimum:

```text
zero and one input rejected
two/many valid inputs accepted
input ordering and reorder preserved
remove input is reversible and updates validity
duplicate occurrence never silently deduplicated
invalid/corrupt/encrypted input handled safely
cancel before/during processing
approved local engine and fallback decisions
no server-processing API for Merge baseline
insufficient storage preflight
output validation and atomic publication
output/source path conflict and no silent overwrite
Correlation ID propagation
QUEUED/RUNNING/COMPLETED/FAILED/CANCELLED behavior where applicable
safe result/error model
L2 accepted and terminal Audit evidence
Tool/File/Assistant parity
```

Merge is atomic: failure of a required input/stage cannot produce `PARTIAL_SUCCESS`. A background Merge may be a local Job; background never implies server.

## 12.8 Family reuse

Reusable contract suites shall cover PDF Page Operations, Conversion, OCR, Editing, Security Tools, Forms, Signature, and Intelligence. Each family supplies representative valid/invalid inputs and specialized safety assertions. The project shall not generate thousands of copy-pasted tests; it shall combine registry-wide generated checks, shared contract tests, equivalence/boundary datasets, pairwise context coverage, and explicit high-risk negative tests.

## 12.9 Fixtures and acceptance

Fixtures shall provide deterministic Feature, file, policy, capability, Job, checkpoint, result, engine, storage, clock, ID, cancellation, logger, History, and Audit objects. Privileged/server/destructive capabilities default to off. No system clock, random identity, machine path, installed engine, environment, real provider, SQL, or network may determine a unit-test result.

A Domain/Application unit-test change is acceptable only when:

```text
all affected requirement/Feature IDs are traced
positive, boundary, negative, cancellation, and recovery behavior is covered
source and temporary/final invariants pass
state transitions and Correlation ID are coherent
resolver remains capability-aware Local-First
safe error/evidence fields contain no prohibited data
tests pass under MSTest + MTP locally and in pipeline
regressions become permanent tests
no unit test falsely substitutes for integration/PoC/security/performance evidence
```
