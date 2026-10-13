# AGENT B — Security, API Contract & Audit Unit-Test Report

**Task ID:** DOC11-AGENT-B  
**Scope:** authentication-support logic, authorization, permission/policy/ownership, offline authorization, reauthentication decisions, API DTO and query validation, idempotency, safe error mapping, Audit mapping, Assistant parity, and sensitive-data sanitization  
**Sources reviewed:** Documents 01–10 FINAL, with primary alignment to Documents 06, 08, 09, and 10  
**Test platform:** MSTest with Microsoft.Testing.Platform, as locked by TECH-DEC-013  
**Boundary:** unit-test design only; no production code, penetration test, full HTTP integration/E2E, real Pertamina SSO, database integration, or external provider/service

The recommended unit suite tests deterministic decisions and mappings before transport or infrastructure concerns. It does not replace contract, integration, authorization, E2E, performance/DoS, VA, or penetration testing required by Documents 06, 08, 09, and 10.

## 1. Authorization unit-test strategy

### 1.1 Testable decision components

Authorization should be decomposed into small application/security components with explicit inputs and outputs:

| Component under test | Input | Expected output |
|---|---|---|
| Authentication context builder | Validated identity abstraction, session/account state, current revisions | Canonical Principal ID and safe authentication state; never trusts client actor/role/owner |
| Authorization decision service | Subject, action/Feature requirement, resource facts, policy/context | ALLOW or stable deny/challenge decision with safe reason code |
| Permission evaluator | Current effective permission snapshot and required permission set | Exact grants satisfied or denied |
| Resource authorization handler | Server-derived owner/scope, parent-child relation, state/version | OWN/REPORTER/ADMIN_SCOPE/SUPPORT_SCOPE/AUDIT_SCOPE/workload decision |
| Offline authorization evaluator | Protected offline grant, clock evidence, local capability, policy revision | ALLOW_LOCAL, ONLINE_REQUIRED, or safe denial |
| Reauthentication evaluator | Fresh-auth context and action/resource binding | FRESH, REAUTHENTICATION_REQUIRED, or INVALID |
| Feature/DTO validator | Canonical Feature descriptor, contract/schema version, typed input | Valid typed command or closed validation failure |
| Idempotency decision service | Scope, key, fingerprint, persisted logical record | Process, replay, same-job 202, in-progress conflict, key-reuse conflict, or restart-before-commit |
| API error mapper | Domain/security/validation decision | Approved HTTP semantic plus safe error envelope |
| Audit event factory and field guard | Authoritative outcome and safe mapping context | Registered event type/version/envelope or explicit mapping rejection |
| Assistant plan authorization adapter | User context and canonical typed step(s) | Same per-step authorization result as File-First/Tool-First |

The authorization result should be a closed union rather than a Boolean. Recommended logical outcomes include ALLOW, AUTHENTICATION_REQUIRED, ACCOUNT_ACCESS_DENIED, SESSION_INVALID, PERMISSION_DENIED, RESOURCE_SCOPE_DENIED, RESOURCE_CONCEALED, POLICY_DENIED, ONLINE_REQUIRED, CONFIRMATION_REQUIRED, REAUTHENTICATION_REQUIRED, RESOURCE_STATE_DENIED, and CAPABILITY_DENIED. The transport layer maps these results later; authorization logic does not choose a message containing resource detail.

### 1.2 Test style and fixtures

- Use MSTest TestMethod for one-off invariants and DataTestMethod/DynamicData for decision matrices.
- Test public behavior, not private methods or the number of internal calls.
- Prefer immutable builders for SubjectContext, PermissionSnapshot, PolicyContext, ResourceFacts, OfflineGrant, FreshAuthenticationContext, FeatureDescriptor, and AuditMappingInput.
- Fix UTC time through an injected clock. Never use the wall clock directly in expiry/freshness tests.
- Use deterministic opaque IDs for User A, User B, Job, Result, Secured File, Issue, Audit Event, Feature, and Correlation fixtures. IDs themselves never convey authority.
- A default fixture contains the minimum valid state. Each negative test changes one dimension only, making deny precedence and regression diagnosis clear.
- Every defect in authorization, sanitization, idempotency, or Audit mapping becomes a permanent regression test.
- Do not place real employee data, production token shapes, real filenames, or real document text in fixtures.

### 1.3 Core invariant test

For every action family, a data-driven theory should prove:

    same authenticated subject + same action + same resource facts
    + same permission/policy/context
    → same authorization result

Changing the origin from FILE_FIRST or TOOL_FIRST to ASSISTANT must not change that result. UI visibility, role label, Feature exposure, confirmation, or reauthentication never creates a missing business permission.

### 1.4 Authentication-support logic

These are unit tests of OTO support logic after an identity assertion has been validated by the authentication layer; they are not cryptographic token or real SSO tests.

| Test ID | Given | Expected |
|---|---|---|
| AUTHN-UT-001 | Approved issuer-subject abstraction maps to one active Principal ID and valid session | Canonical authenticated context |
| AUTHN-UT-002 | Missing/unknown stable subject mapping | No client-side auto-provision; authentication/account decision fails safely |
| AUTHN-UT-003 | Client supplies a different Principal ID, role, unit, or owner | Client values ignored; server-derived context retained |
| AUTHN-UT-004 | Session expired or revoked | Authentication/session decision invalid; no cached permission fallback |
| AUTHN-UT-005 | Principal disabled/ineligible while identity itself is valid | ACCOUNT_ACCESS_DENIED, later mapped to safe 403 |
| AUTHN-UT-006 | Refresh result changes permission/policy revision | Effective context rebuilt atomically from the new revision |
| AUTHN-UT-007 | Refresh reports invalid/replayed/revoked grant | Silent refresh stops; no automatic offline fallback |
| AUTHN-UT-008 | Logout invoked repeatedly | Idempotent invalidation; offline authorization invalidated; no source/draft deletion |
| AUTHN-UT-009 | User token/claim attempts to select workload identity | Denied; human and workload subject types remain separate |
| AUTHN-UT-010 | Safe error created from authentication exception containing identity internals | Generic safe code/message/correlation only; no assertion, claim, token, or exception text |
| AUTHN-UT-011 | Authentication wrapper reports missing assertion | AUTHENTICATION_REQUIRED |
| AUTHN-UT-012 | Wrapper reports invalid signature or disallowed algorithm | Safe 401 decision; no algorithm/signature detail leaked |
| AUTHN-UT-013 | Issuer, audience, token type, or approved client does not match configured policy | Safe 401 decision |
| AUTHN-UT-014 | Lifetime expired, not-before is in the future, or required subject is absent | Safe 401 decision using injected clock/skew policy |
| AUTHN-UT-015 | SSO transaction state, nonce, PKCE verifier, callback binding, or redirect binding mismatches | Reject transaction; no session issued |
| AUTHN-UT-016 | Several callers request refresh concurrently | Single-flight coordinator issues one logical refresh and shares safe outcome |
| AUTHN-UT-017 | Interaction-required/invalid-grant result is returned | Stop silent refresh and require approved user interaction |
| AUTHN-UT-018 | Authentication result contains raw assertion/token/claims | Context projection retains only approved canonical fields |

These tests use synthetic validation facts and exercise OTO policy/wrapper mapping; they do not reimplement cryptography or call Pertamina Identity. Actual signature verification, protocol metadata, redirect/callback transport, token handler configuration, and revocation behavior require authentication contract/integration tests with approved identity test infrastructure.

## 2. Permission and policy cases

### 2.1 Permission decision matrix

| Test ID | Scenario | Expected decision |
|---|---|---|
| PERM-UT-001 | User holds document.content.process, owns valid input, and all policy/state/capability checks pass | ALLOW |
| PERM-UT-002 | Same request without document.content.process | PERMISSION_DENIED |
| PERM-UT-003 | Role label User supplied but effective permission missing | DENY; role name is not authorization |
| PERM-UT-004 | Administrator attempts own-document processing with only Administrator defaults | DENY; Administrator does not inherit User |
| PERM-UT-005 | Auditor lists scoped Audit events with audit.event.list | ALLOW within AUDIT_SCOPE |
| PERM-UT-006 | Auditor attempts Audit update/delete | DENY; no human permission exists |
| PERM-UT-007 | Support profile reads exact scoped Issue with support.issue.read | ALLOW only for SUPPORT_SCOPE |
| PERM-UT-008 | Support profile requests password, document/OCR content, unrelated Job/result, signature, or clipboard | DENY regardless of Support assignment |
| PERM-UT-009 | Permission snapshot is expired | DENY or REFRESH_REQUIRED; never stale allow |
| PERM-UT-010 | Permission has been revoked in the current revision | DENY even when older cache granted it |
| PERM-UT-011 | Unknown permission code or subject type | Fail closed |
| PERM-UT-012 | Human permission presented by Worker or workload permission by human | DENY subject-type mismatch |
| PERM-UT-013 | Principal holds several roles and one policy denies the action | DENY; hard/policy deny wins |
| PERM-UT-014 | User holds base process permission but lacks document.pdf.unlock for Unlock | DENY |
| PERM-UT-015 | User holds base plus document.pdf.unlock and all other guards pass | ALLOW to proceed to confirmation/password/authority checks |
| PERM-UT-016 | User holds document.content.process but server route lacks document.server.execute | DENY server execution; local eligibility is evaluated separately |
| PERM-UT-017 | Assistant session permission exists but underlying Feature permission does not | DENY tool action |
| PERM-UT-018 | Audit export permission is absent although audit.event.read exists | DENY export |
| PERM-UT-019 | Secured metadata read exists but version download permission is absent | DENY bytes retrieval |
| PERM-UT-020 | Administrator has explicit scoped Audit grant and SoD/policy pass | ALLOW only the separately granted Audit action, not document content |

### 2.2 Policy and context cases

| Test ID | Changed dimension from an otherwise valid request | Expected |
|---|---|---|
| POLICY-UT-001 | Feature inactive/hidden or handler not registered | Closed denial; no dynamic dispatch |
| POLICY-UT-002 | Classification prohibits the chosen server/provider route | POLICY_DENIED; no fallback to public provider |
| POLICY-UT-003 | Resource state/version incompatible | RESOURCE_STATE_DENIED without leaking hidden state |
| POLICY-UT-004 | Client/platform/contract capability incompatible | CAPABILITY_DENIED or compatibility decision |
| POLICY-UT-005 | Required explicit upload/share/finalization consent absent | CONFIRMATION_REQUIRED; no side effect |
| POLICY-UT-006 | Quota/capacity reservation cannot be made | Admission decision fails before durable 202 |
| POLICY-UT-007 | Administrator attempts to configure beyond hard safety ceiling | DENY even with configuration permission |
| POLICY-UT-008 | Policy service/revision input unknown or unavailable for a privileged action | Fail closed |
| POLICY-UT-009 | Policy revision changes between preview and execution | Re-evaluate and reject stale decision |
| POLICY-UT-010 | User preference allows a route that enterprise classification denies | DENY; higher policy wins |
| POLICY-UT-011 | Confirmation is present but permission is absent | PERMISSION_DENIED |
| POLICY-UT-012 | Reauthentication is fresh but current policy denies | POLICY_DENIED |

Permission tests assert both the decision and a stable safe reason code. They must not assert localized UI text as the security contract.

## 3. Ownership cases

Ownership is always derived from authenticated Principal ID and authoritative resource metadata. Client-supplied owner, unit, role, scope, resource link, or Feature parameter is ignored.

| Test ID | Scenario | Expected |
|---|---|---|
| OWN-UT-001 | User A reads/cancels/retries own eligible Job | ALLOW for the exact required Job permission |
| OWN-UT-002 | User A uses User B Job ID | RESOURCE_SCOPE_DENIED or concealed result |
| OWN-UT-003 | User A uses User B Result ID with own Job ID | DENY parent-child mismatch before storage resolution |
| OWN-UT-004 | User A requests User B secured file/version | DENY; opaque ID and fileRef are not authority |
| OWN-UT-005 | User A submits ownerId=User A for a User B resource | Ignore request owner; DENY from persisted owner |
| OWN-UT-006 | Administrator requests a user's document/result/OCR/secured content | DENY by default despite ADMIN_SCOPE |
| OWN-UT-007 | Auditor follows Resource ID from Audit evidence to document/result | DENY; Audit reference grants no content access |
| OWN-UT-008 | Support reads exact explicitly sent attachment under an authorized Issue | ALLOW when parent and child membership both match |
| OWN-UT-009 | Support reads attachment from another/unassigned Issue | DENY before object/storage lookup |
| OWN-UT-010 | Reporter reads own Issue status | ALLOW reporter projection only |
| OWN-UT-011 | Reporter reads another user's Issue or attachment | DENY/conceal consistently |
| OWN-UT-012 | Audit query contains a wider client filter than server-derived AUDIT_SCOPE | Server scope remains authoritative; out-of-scope rows/counts excluded |
| OWN-UT-013 | Cursor was issued for another principal/scope/filter | INVALID_CURSOR or concealed denial; never reuse |
| OWN-UT-014 | Worker lease matches Job, purpose, environment, and artifact | ALLOW exact workload operation |
| OWN-UT-015 | Worker lease Job/purpose/environment mismatches | DENY; workload identity alone is insufficient |
| OWN-UT-016 | Range/download request is repeated after ownership revocation | DENY before range/storage resolution |
| OWN-UT-017 | Resource does not exist versus exists outside caller scope under conceal policy | Same outward not-found decision; no state/timing detail in mapper fixture |
| OWN-UT-018 | Authorized parent with child belonging to a different parent | DENY child access |

Interaction-focused unit tests may use a spy content resolver to assert it is never invoked after ownership/scope denial. Proof that database scope precedes count/projection and that object bytes are not fetched requires later integration tests.

## 4. Offline authorization tests

| Test ID | Scenario | Expected |
|---|---|---|
| OFFLINE-UT-001 | Valid protected grant, matching profile/device, trusted time, current permitted revision, deterministic local Feature | ALLOW_LOCAL |
| OFFLINE-UT-002 | Same Feature requires server execution/upload | ONLINE_REQUIRED; no upload or route substitution |
| OFFLINE-UT-003 | Secured-file, Issue submit, Audit query/export, Admin, Support, realtime, or workload action | ONLINE_REQUIRED or DENY |
| OFFLINE-UT-004 | Offline grant absolute expiry reached | DENY; no sliding renewal |
| OFFLINE-UT-005 | Clock rollback or trusted-time uncertainty | Fail closed for protected action |
| OFFLINE-UT-006 | Grant integrity invalid/tampered | DENY and protected recovery signal |
| OFFLINE-UT-007 | Grant copied to another profile/device/account | DENY binding mismatch |
| OFFLINE-UT-008 | Last learned disable/revoke marker is set | DENY |
| OFFLINE-UT-009 | Permission/policy/capability/contract revision is stale beyond approved offline rule | DENY or ONLINE_REQUIRED; never privileged stale allow |
| OFFLINE-UT-010 | Local Protect/Unlock/Sign/Redact is eligible and required confirmation/action permission is present | ALLOW_LOCAL after all action-specific checks |
| OFFLINE-UT-011 | Reauthentication-required action is requested offline | ONLINE_REQUIRED; cached login cannot satisfy freshness |
| OFFLINE-UT-012 | Assistant workflow contains only valid offline-eligible local steps | ALLOW_LOCAL per step |
| OFFLINE-UT-013 | One Assistant step requires server/provider/secured/share action | Do not execute that step; ONLINE_REQUIRED with safe plan state |
| OFFLINE-UT-014 | Pending command reconnects after permission was revoked | Refresh first, then DENY; queued intent is not prior authorization |
| OFFLINE-UT-015 | Account switch occurs while old profile queue exists | Old queue remains isolated; no dispatch under new account |
| OFFLINE-UT-016 | Web has cached UI assets while disconnected | No offline business authority |
| OFFLINE-UT-017 | Mandatory safe Audit outbox record is created | It records evidence only and grants no Feature/server permission |
| OFFLINE-UT-018 | Offline grant fixture contains token/cookie/password/raw claims | Grant validator rejects prohibited fields |

Exact offline lifetime, trusted-time implementation, revision tolerance, and revocation propagation objective remain injected policy values. Unit tests use boundary values from policy fixtures rather than inventing Production durations.

## 5. Reauthentication decision tests

### 5.1 Required actions

The baseline test catalog marks fresh reauthentication required for:

- role and permission assign/revoke;
- another principal's eligibility update;
- another session's revocation;
- high-impact security/classification/retention policy change;
- permanent secured-file purge;
- key/destructive recovery;
- any future exceptional content access, which currently has no permission.

Policy-conditional cases include high-impact Audit/Support export create/read, broad Feature/limit/compatibility change, and high-classification upload, secured transfer, download, share, disclosure, or Support attachment access.

### 5.2 Freshness and binding matrix

| Test ID | Scenario | Expected |
|---|---|---|
| REAUTH-UT-001 | Required action with no fresh context | REAUTHENTICATION_REQUIRED before mutation |
| REAUTH-UT-002 | Fresh context bound to same principal/session/client/action/resource/policy and permission revisions | FRESH |
| REAUTH-UT-003 | Fresh context belongs to another principal/session/client | INVALID; require new reauthentication |
| REAUTH-UT-004 | Action or resource/scope differs from the binding | INVALID; no global elevation |
| REAUTH-UT-005 | Freshness age equals allowed boundary | Result follows injected inclusive/exclusive policy definition |
| REAUTH-UT-006 | Freshness exceeds configured maximum age | REAUTHENTICATION_REQUIRED |
| REAUTH-UT-007 | Logout, account disablement, or session revocation occurs | Context invalidated |
| REAUTH-UT-008 | Permission/policy/resource version changes after fresh auth | Context invalidated |
| REAUTH-UT-009 | Fresh auth succeeds but required permission is absent | PERMISSION_DENIED, not ALLOW |
| REAUTH-UT-010 | Fresh auth exists but confirmation is absent | CONFIRMATION_REQUIRED; reauth does not substitute |
| REAUTH-UT-011 | Confirmation exists but fresh auth is absent | REAUTHENTICATION_REQUIRED |
| REAUTH-UT-012 | Policy-conditional action is below configured risk/classification threshold | No reauth solely from that condition; all other guards still apply |
| REAUTH-UT-013 | Same action crosses configured risk/classification threshold | REAUTHENTICATION_REQUIRED |
| REAUTH-UT-014 | Failed/cancelled fresh-auth attempt | No partial mutation and no reusable freshness context |
| REAUTH-UT-015 | Missing freshness decision reaches error mapper | Safe 401 REAUTHENTICATION_REQUIRED |
| REAUTH-UT-016 | Valid identity and freshness but wrong owner | 403 or concealed 404 according to resource policy |

**REQUIRES_PERTAMINA_STANDARD_CONFIRMATION:** approved reauthentication mechanism, maximum age, bounded-use/reuse rule, attempt/lock behavior, risk/classification thresholds, and assurance level. Tests must consume these as typed policy inputs and must not hard-code invented values.

## 6. API validation tests

These tests target DTO validators, catalog resolvers, query validators, admission-policy calculators, and response factories directly. They do not host ASP.NET Core or send real HTTP requests.

### 6.1 Request and Feature contract validation

| Test ID | Input | Expected |
|---|---|---|
| API-VAL-001 | Minimal valid typed command for an active registered Feature | Valid normalized command |
| API-VAL-002 | Malformed JSON/parser result or invalid envelope shape | 400-class validation decision |
| API-VAL-003 | Unknown/extra property under closed schema | Reject; additional properties are not ignored |
| API-VAL-004 | Duplicate/ambiguous property representation | Reject closed parsing result |
| API-VAL-005 | Wrong scalar/array/object type | Reject with safe field and issue codes |
| API-VAL-006 | Unknown enum/discriminator | Reject; no default coercion |
| API-VAL-007 | Null, empty, whitespace, excessive length/count/depth, or out-of-range numeric/time value | Reject at configured boundary |
| API-VAL-008 | Unsupported Contract Version | Compatibility/version error; no best-effort reinterpretation |
| API-VAL-009 | Unsupported Parameter Schema Version | Schema-version error; no fallback to latest |
| API-VAL-010 | Feature ID not canonical, unknown, inactive, deprecated-without-valid-path, hidden, or not registered | Reject closed dispatch |
| API-VAL-011 | Body attempts to repeat/override Feature ID from path | Path remains authoritative; mismatch/extra discriminator rejected |
| API-VAL-012 | Feature exists in catalog but has no approved server handler | Reject server execution |
| API-VAL-013 | Local-only Feature is sent to processing API | Reject; local processing makes no HTTP processing call |
| API-VAL-014 | Input fileRef kind/ID/version is invalid or unsupported | Reject typed reference |
| API-VAL-015 | Input reference is not READY/AVAILABLE or immutable version mismatches | Semantic/state validation failure |
| API-VAL-016 | Parameter contains engine, provider, class, assembly, shell, command, SQL, filesystem path, arbitrary URL, or client-selected processing route | Reject prohibited dispatch field/value |
| API-VAL-017 | Workflow has duplicate step IDs, cycle, invalid dependency, incompatible output/input, or excessive steps | Reject plan before execution |
| API-VAL-018 | Workflow step uses a different/unsupported Feature schema version | Reject that plan; no partial execution |
| API-VAL-019 | Filename-like output parameter contains control/invalid characters or exceeds policy | Reject or normalize only according to explicit filename policy; never expose a path |
| API-VAL-020 | Password-bearing request is otherwise valid | Password remains operation-memory input and is excluded from normalized diagnostics/fingerprint/Audit |

The test fixture should be generated from or checked against the approved Feature Contract Index so that released server-executable Feature IDs have exactly one typed parameter/result contract, handler registration, authorization requirement, and validation suite. It must not duplicate a second hand-maintained Feature registry.

### 6.2 Header and identifier logic

| Test ID | Scenario | Expected |
|---|---|---|
| API-HDR-001 | Valid opaque Correlation ID | Preserved across decision/result mapping |
| API-HDR-002 | Missing Correlation ID where server generation is allowed | One safe opaque ID generated; no identity/content encoded |
| API-HDR-003 | Invalid/oversized/control-character Correlation ID | Reject or replace only according to the approved header policy; never echo unsafe input |
| API-HDR-004 | Job ID, Correlation ID, fileRef, cursor, ETag, or storage reference offered as authorization proof | Ignored as authority |
| API-HDR-005 | Required Idempotency Key missing/empty/whitespace | 400 validation decision before command execution |
| API-HDR-006 | Idempotency Key exceeds configured size or invalid syntax | 400 safe validation decision |
| API-HDR-007 | If-Match is missing for a version-required configuration mutation | Precondition-required contract decision if adopted by endpoint; no mutation |
| API-HDR-008 | If-Match does not equal current version | 412 PRECONDITION_FAILED without unauthorized state leakage |

Exact identifier syntax/length ceilings must come from the approved contract/schema. Unit tests cover below, equal, and above each configured bound rather than inventing Production constants.

### 6.3 Pagination, filter, search, and cursor logic

| Test ID | Scenario | Expected |
|---|---|---|
| API-QRY-001 | Ordinary list uses default page/pageSize | Default 25 and valid normalized query |
| API-QRY-002 | Audit list uses default cursor page size | Default 50 |
| API-QRY-003 | pageSize equals allowed maximum | Accepted |
| API-QRY-004 | page/pageSize is zero, negative, or above maximum | 400 INVALID_PAGINATION; never silently clamp |
| API-QRY-005 | page and cursor supplied together | 400 INVALID_PAGINATION |
| API-QRY-006 | Cursor integrity invalid | 400 INVALID_CURSOR |
| API-QRY-007 | Cursor expired | 410 CURSOR_EXPIRED |
| API-QRY-008 | Cursor belongs to another subject/endpoint/filter/sort/date range/scope/revision | Reject; no continuation |
| API-QRY-009 | Permission or scope changes after cursor issue | Reauthorization fails or cursor invalidates safely |
| API-QRY-010 | Date range uses valid UTC RFC 3339 half-open interval | Accepted normalized range |
| API-QRY-011 | from is equal to/after to, range too wide, or timezone/format invalid | Reject |
| API-QRY-012 | Filter/sort/search field and operator are allowlisted | Accepted with stable sort plus unique tie-breaker |
| API-QRY-013 | Arbitrary field path, regex, raw expression, dynamic JSON path/SQL, or content search | Reject |
| API-QRY-014 | Same filter field repeated | Normalizes to OR |
| API-QRY-015 | Different filter fields combined | Normalizes to AND |
| API-QRY-016 | Client attempts to submit an authorization filter | Ignore/reject; server scope is separate and applied first |

Authorization-before-count/projection and keyset SQL behavior need repository integration tests. Unit tests verify the query specification always carries a server-derived scope predicate and cannot be built from a client scope.

### 6.4 HTTP semantic and admission-policy logic

| Test ID | Condition | Expected semantic |
|---|---|---|
| API-SEM-001 | Immediate command completes with validated result | 200 |
| API-SEM-002 | Addressable resource created | 201 plus required Location |
| API-SEM-003 | Background operation is only proposed/reserved in memory | 202 factory refuses response |
| API-SEM-004 | Background Job and idempotency record are durably accepted | 202 with same Job ID, status URL, result URL, Correlation ID, Location, and safe Retry-After |
| API-SEM-005 | Caller quota/rate/concurrency exceeded | 429 with safe Retry-After |
| API-SEM-006 | Hard body/file/chunk/batch limit exceeded | 413 |
| API-SEM-007 | Storage/state/version conflict | 409 |
| API-SEM-008 | Shared queue/worker/provider saturation is unsafe to accept | 503, not a fake 202 |
| API-SEM-009 | Immediate upstream fails before durable background acceptance | 502 or 504 according to failure/timeout decision |
| API-SEM-010 | Known authorized resource expired by retention | 410 |

Admission-policy tests calculate the strictest effective bound across system safety, enterprise policy, environment, subject, endpoint/workload, worker/storage/database, and provider layers. Cases cover below/equal/above limit, weighted job cost, atomic reservation success/failure/release, caller versus shared saturation, and Retry-After presence. Exact numeric values remain injected policy fixtures.

## 7. Idempotency logic tests

### 7.1 Command idempotency

The logical scope is authenticated principal/tenant plus HTTP method, canonical route, and Contract Version. Fingerprints use the canonical request, Feature/schema versions, immutable resource versions, and content digest where relevant. Passwords and secrets never enter the fingerprint snapshot.

| Test ID | Stored/request condition | Expected |
|---|---|---|
| IDEM-UT-001 | New valid key and no scoped record | PROCESS_ONCE |
| IDEM-UT-002 | Same key, same scope/fingerprint, committed immediate resource | Replay original status and safe response/resource |
| IDEM-UT-003 | Same key, same scope/fingerprint, Job running | 202 with exactly the original Job ID and Location |
| IDEM-UT-004 | Same key/fingerprint is IN_PROGRESS with no resource yet | 409 IDEMPOTENCY_REQUEST_IN_PROGRESS plus Retry-After |
| IDEM-UT-005 | Same key/scope with a different fingerprint | 409 IDEMPOTENCY_KEY_REUSED; no second effect |
| IDEM-UT-006 | Failure occurred before durable commit | Same logical operation may restart/clean safely; no duplicate active resource |
| IDEM-UT-007 | Same key under another principal/tenant/method/route/Contract Version | Different scope; never replay another caller's response |
| IDEM-UT-008 | Different keys with identical content digest | Treat as different user intents |
| IDEM-UT-009 | Canonical property order/format differences with identical semantics | Same fingerprint |
| IDEM-UT-010 | Feature/schema or immutable input version changes | Different fingerprint and key-reuse conflict for same key |
| IDEM-UT-011 | Password/secret value changes while all permitted semantic inputs are equal | Forbidden values absent from snapshot; operation-specific design must not expose or persist them |
| IDEM-UT-012 | Two concurrent creates race on the same key/fingerprint | Exactly one creator wins; other observes in-progress or replays committed result |
| IDEM-UT-013 | Original safe response contains expiring resource | Replay follows stored logical result and current retrieval authorization; does not mint unauthorized bytes |
| IDEM-UT-014 | Retry/cancel/resume command reuses key after transport timeout | One logical control action; no duplicate attempt/effect |

A concurrency unit test can use an atomic fake idempotency store to prove decision behavior. SQL unique constraints, transaction races, process restarts, and persisted replay require later integration tests.

### 7.2 Audit/offline deduplication

| Test ID | Scenario | Expected |
|---|---|---|
| AUD-IDEM-001 | Same Audit Event ID and canonical Event Fingerprint | DUPLICATE acknowledgement; one logical event |
| AUD-IDEM-002 | Same Audit Event ID with different fingerprint | Conflict/security signal; original remains unchanged |
| AUD-IDEM-003 | New ID with same Correlation/Job/Feature/Event Type/timestamp | New event; those fields are not dedup keys |
| AUD-IDEM-004 | Same client event and trusted source scope is retried | Same logical event |
| AUD-IDEM-005 | Same Audit batch key and request fingerprint | Replay original per-event dispositions |
| AUD-IDEM-006 | Same Audit batch key with different request fingerprint | Batch conflict; no overwrite |
| AUD-IDEM-007 | A materially new business outcome occurs during retry/recovery | New Audit Event ID linked by correlation/resource |

## 8. Error and sanitization tests

### 8.1 Error envelope

The error mapper emits only code, localized-safe message key/result, Correlation ID, and one approved typed details union: VALIDATION, RETRY_GUIDANCE, CONFLICT, or LIMIT. It never serializes an exception or arbitrary dictionary.

| Test ID | Domain result | Expected HTTP/error behavior |
|---|---|---|
| ERR-UT-001 | Missing/invalid/expired/revoked authentication | Safe 401 |
| ERR-UT-002 | Fresh authentication required | 401 REAUTHENTICATION_REQUIRED |
| ERR-UT-003 | Valid identity, permission/ownership/policy denied | 403 or consistent concealed 404 |
| ERR-UT-004 | Hidden/out-of-scope resource under conceal policy | 404 without state/version hint |
| ERR-UT-005 | Malformed header/query/envelope/JSON | 400 with bounded VALIDATION details |
| ERR-UT-006 | Syntactically valid semantic/file/business validation failure | 422 |
| ERR-UT-007 | Idempotency/state/storage/checkpoint conflict | 409 with allowlisted conflict state only |
| ERR-UT-008 | ETag/version precondition failure | 412 |
| ERR-UT-009 | Unsupported/inconsistent media/signature | 415 |
| ERR-UT-010 | Invalid byte range | 416 |
| ERR-UT-011 | Caller limit | 429 plus Retry-After |
| ERR-UT-012 | Shared unsafe capacity/readiness | 503 with retry guidance only when safe |
| ERR-UT-013 | Unexpected exception | 500 generic envelope and valid Correlation ID |
| ERR-UT-014 | Exception contains a known resource state but caller is unauthorized | Concealed response wins; no 409/412 leak |
| ERR-UT-015 | Localized message catalog lacks a key | Safe generic fallback; code remains stable |

### 8.2 Sanitizer and leakage checks

Build one reusable synthetic CanarySet with unique non-production markers for password, token, secret, path, storage key, document content, OCR text, signature, clipboard, AI prompt/provider payload, screenshot, issue message, SQL, stack trace, and claim dump.

For each test, place canaries in top-level fields, nested objects, arrays, exception Data, ModelState-like errors, provider errors, filenames, headers, and mixed-case prohibited property names. Assert that no canary appears in:

- error code, message, details, headers, or serialized safe response;
- captured structured log state, scopes, exception rendering, or trace tags;
- metric names/tags and health output;
- idempotency key hash input, request fingerprint snapshot, or stored safe response;
- Audit event, Safe Metadata, outbox item, batch outcome, export projection, or alert;
- Assistant metadata/conversation summary or workflow validation failure.

Additional negative cases:

| Test ID | Scenario | Expected |
|---|---|---|
| SAN-UT-001 | Safe metadata contains prohibited key such as password, accessToken, documentText, ocrText, signatureImage, clipboard, or prompt | Mapping rejected; prohibited value never reaches event |
| SAN-UT-002 | Unknown metadata property | Rejected by closed event-version schema |
| SAN-UT-003 | Metadata exceeds size/depth/count bound | Rejected with safe issue code |
| SAN-UT-004 | Error/filename/reason contains CR/LF/control characters | Normalized or rejected; no log/header forging |
| SAN-UT-005 | Raw exception includes SQL/path/object key/provider response | Generic safe error and safe log code only |
| SAN-UT-006 | Authentication failure includes employee/email/claim details | Generic non-enumerating response |
| SAN-UT-007 | LIMIT/RETRY details attempt to include queue depth/topology | Allowlist drops/rejects unsafe fields |
| SAN-UT-008 | Safe metadata uses an allowed field with content-like value from a prohibited source | Source-aware field guard rejects it; key-name filtering alone is insufficient |
| SAN-UT-009 | A canary is split across nested structures/encoded text | Canonical serialization remains bounded and no prohibited source is mapped |
| SAN-UT-010 | Sanitizer fails internally | Fail closed; do not emit original object as fallback |

The primary control is positive, typed allowlisting. Pattern-based redaction is defense in depth and must not be the only protection.

## 9. Audit mapping tests

### 9.1 Envelope invariants

Every mapping test asserts:

- registered Event Type plus exact Event Version, server-validated Category and Audit Level;
- stable Audit Event ID and Client Event ID for client/offline origin;
- original Occurred At and server-assigned Received At remain distinct;
- actual accountable Actor Principal ID/Type, minimized role/unit snapshot, Origin, Executor, and On-Behalf-Of semantics;
- controlled Action, Resource Type/opaque ID, Outcome, Reason/Error Code;
- canonical Feature ID where a Feature action exists;
- Job ID when a background Job exists and Correlation ID for server/privileged/Support/secured/AI/high-risk/audit-access/cross-component actions;
- execution location and processing mode are controlled values;
- Safe Metadata matches the exact event-type/version schema and contains no prohibited data.

### 9.2 Event mapping matrix

| Test ID | Authoritative input/outcome | Expected event |
|---|---|---|
| AUD-MAP-001 | Application session granted | Auth.LoginSucceeded with USER Actor and safe session reference only if policy requires |
| AUD-MAP-002 | Meaningful authentication failure | Auth.LoginFailed with safe reason; no token/claims/password |
| AUD-MAP-003 | Sensitive action blocked for missing freshness | Authorization.ReauthenticationRequired |
| AUD-MAP-004 | Privileged action denied | Authorization.PrivilegedActionDenied with actual actor/resource/action and DENIED |
| AUD-MAP-005 | Cross-user resource probe | Authorization.CrossResourceScopeDenied; no target content |
| AUD-MAP-006 | Ordinary Feature accepted for processing | Processing.ExecutionAccepted with canonical Feature/Process/Correlation and Job when present |
| AUD-MAP-007 | Same Feature completes | Processing.ExecutionCompleted with SUCCEEDED and safe size/location fields |
| AUD-MAP-008 | Feature fails/cancels/partially completes | ExecutionFailed/ExecutionCancelled or approved terminal mapping with matching controlled Outcome |
| AUD-MAP-009 | Protect PDF final result | HighRiskFeature.ProtectFinalized for FEAT-PDF-061; no password |
| AUD-MAP-010 | Unlock PDF final result | HighRiskFeature.UnlockFinalized for FEAT-PDF-062; no password/content |
| AUD-MAP-011 | Visual Sign final result | HighRiskFeature.VisualSignFinalized for FEAT-SIGN-013; no signature asset |
| AUD-MAP-012 | Redaction final validation succeeds/fails | RedactionFinalized or RedactionValidationFailed for FEAT-PDF-069; no text/coordinates |
| AUD-MAP-013 | Secured file/version created or permanently deleted | Correct SecuredFile event with opaque resource/version and reauth/policy outcome |
| AUD-MAP-014 | Role/permission/configuration/policy change | Exact Administration/Policy/Configuration fact with initiator, target, versions, outcome, correlation |
| AUD-MAP-015 | Audit query/export access | Approved AuditAccess event; no raw SQL/filter/content/result rows |
| AUD-MAP-016 | Support attachment viewed | Support.AttachmentViewed with scoped Issue/Attachment refs; no bytes/name/key |
| AUD-MAP-017 | AI/provider route denied by policy | AI.PolicyDenied or Security.PublicAiRouteBlocked according to catalog |
| AUD-MAP-018 | Assistant invokes ordinary Feature | Underlying Feature event with actual User Actor and Origin=ASSISTANT; no duplicate default Assistant event |
| AUD-MAP-019 | Delayed Worker executes accepted user Job | User remains accountable/on-behalf-of context; Worker is Executor, not replacement human Actor |
| AUD-MAP-020 | Local offline event later syncs | Original Occurred At/client sequence retained; server stamps Received At and validates source binding |
| AUD-MAP-021 | Routine low-value 403 not selected by catalog | No Enterprise Audit event; safe Observability signal may be produced |
| AUD-MAP-022 | Sensitive/repeated denial crosses catalog threshold | One approved security/authorization event, not an event per noise signal |

### 9.3 Catalog-wide tests

- Enumerate the authoritative 33 Core Tool registry and assert each execution maps to approved L2 accepted/terminal evidence.
- Assert Protect, Unlock, Visual Sign, and Redact additionally map to their L3 high-risk finalization events.
- Assert every registered event has a unique Event Type/Version pair, Category, Audit Level, retention class, required-field rule, Safe Metadata schema, and mapper.
- Assert Feature actions without a canonical Feature ID fail mapping rather than inventing an ID.
- Assert a background processing event without Job ID fails when a Job exists.
- Assert correlation is preserved from application decision through Audit mapping and never used as a deduplication key.
- Assert correction/reversal/recovery generates a new linked event; no mapper exposes update/delete of historical events.
- Assert unknown Event Type/Version and unknown metadata fields remain rejected/blocked, never coerced into a generic event.

Audit unit tests verify mapping output. Append-only database permissions, SQL uniqueness, offline persistence, archive, restore, and end-to-end no-duplicate behavior remain integration/operational tests.

## 10. Assistant authorization tests

Assistant requires assistant.session.use to use the interface, but that permission gives no tool authority. Each proposed Feature step is processed through the same canonical validator, permission/policy, resource ownership, connectivity, confirmation, reauthentication, capability, and application service used by File-First and Tool-First.

| Test ID | Scenario | Expected |
|---|---|---|
| ASST-UT-001 | User may use Assistant and is allowed to execute the canonical Feature on own input | Assistant step receives the same ALLOW as direct UI |
| ASST-UT-002 | User may use Assistant but lacks underlying Feature/action permission | DENY; Assistant session permission does not elevate |
| ASST-UT-003 | User is denied by ownership/policy/classification/state | Same denial as direct UI |
| ASST-UT-004 | Same semantic request evaluated through FILE_FIRST, TOOL_FIRST, and ASSISTANT origins | Equal authorization result; only origin metadata differs |
| ASST-UT-005 | Assistant proposes unknown, malformed, inactive, hidden, or invented Feature ID | Reject before execution |
| ASST-UT-006 | Assistant proposes a canonical Feature with invalid/extra/out-of-bound typed parameters | Reject schema validation |
| ASST-UT-007 | Assistant proposes engine, provider, shell, SQL, path, arbitrary URL/API, or Worker command | Reject as non-canonical operation |
| ASST-UT-008 | Destructive/sensitive intent or target is ambiguous | CLARIFICATION_REQUIRED; no confirmation inferred |
| ASST-UT-009 | Exact destructive/high-risk action is clear but confirmation absent | CONFIRMATION_REQUIRED; no side effect |
| ASST-UT-010 | Confirmation exists but current permission/ownership/policy/reauth fails | DENY; confirmation is not authority |
| ASST-UT-011 | Multi-step workflow has all valid authorized steps | Each step independently allowed at execution time |
| ASST-UT-012 | A later share/overwrite/Redact/server transfer/Amankan step lacks its extra permission or confirmation | Stop before that step; prior successful step does not authorize it |
| ASST-UT-013 | Workflow input points to another user's resource | DENY same as direct API |
| ASST-UT-014 | Offline workflow contains only eligible deterministic local steps | ALLOW_LOCAL per step |
| ASST-UT-015 | Offline workflow contains online-only server/provider/secured action | ONLINE_REQUIRED; do not silently reroute or upload |
| ASST-UT-016 | Assistant proposes Admin/Support/Audit action under normal User Assistant surface | DENY/not exposed by default |
| ASST-UT-017 | Document/OCR/HTML/metadata instructs Assistant to change policy or call a tool | Treat as DATA; no plan authority |
| ASST-UT-018 | Assistant claims success before authoritative tool/Job result | Result-grounding validator rejects the claim/state |
| ASST-UT-019 | Actual Feature execution completes/fails | Assistant response uses actual terminal status/result and the same Correlation ID |
| ASST-UT-020 | Assistant-triggered Feature is audited | Actual User is Actor, ASSISTANT is Origin, Feature/Job/Correlation mapping remains canonical |

For parity tests, run one shared authorization case set twice: direct application invocation and Assistant-adapted invocation. The Assistant adapter may transform validated intent into the canonical request, but it cannot modify subject, grants, resource ownership, policy, risk, consent, or freshness.

## 11. Sensitive-data negative tests

### 11.1 Canary catalog

Use unique synthetic values, never real sensitive data:

| Category | Example fixture field/source | Must never appear in |
|---|---|---|
| Password/passphrase | password, pdfPassword, unlockPassword | DB/outbox, fingerprint snapshot, Audit, log, error, telemetry, Assistant/chat, issue diagnostic |
| Token/cookie/credential | Authorization, accessToken, refreshToken, sessionCookie | Any persistence or output except the isolated authentication boundary |
| Secret/key/connection | clientSecret, privateKey, encryptionKey, connectionString | Error, log, configuration projection, Audit, test artifact |
| Document content | fileBytes, pageText, extractedText | Audit, logs, errors, telemetry, idempotency snapshot |
| OCR/translated/Markdown content | ocrText, translatedText, markdownBody | Same prohibited planes |
| Signature/paraf | signatureImage, signatureTemplate, stampBytes | Audit, logs, telemetry, automatic sync |
| Clipboard | clipboardText, clipboardImage | Audit, logs, errors, telemetry, Assistant persistence |
| AI/Assistant payload | prompt, chat, systemInstruction, providerRequest, providerResponse, modelOutput | Audit, ordinary SQL, logs, telemetry, idempotency |
| Support payload | issueMessage, screenshotBytes, attachmentBytes, noteText | Audit/log/telemetry; only permission-gated Support storage when explicitly sent |
| Infrastructure detail | localPath, UNC path, objectKey, storageUrl, SQL, stackTrace, claims | Public errors, Audit, unrestricted logs/metrics |

### 11.2 Negative-test requirements

1. Feed every canary through each mapper independently and in one combined deeply nested object.
2. Repeat prohibited property names with case, separator, alias, and nesting variations; closed typed schemas must reject unknown variants.
3. Assert the complete serialized result does not contain the canary string or byte marker, not merely that one expected property is null.
4. Capture log message template, structured values, scopes, exception, trace tags, metric tags, and health projection.
5. Capture idempotency fingerprint source/snapshot and persisted safe response abstraction.
6. Capture Audit intent, local outbox representation, server event, safe metadata, batch rejection, and export projection.
7. Verify password values are available only to the operation boundary for the required lifetime and are not copied into a record. Managed-memory zeroization must not be claimed as absolute.
8. Verify visual-signature metadata can contain only approved action/asset-safe references; image/template bytes never enter Audit or automatic sync.
9. Verify Correlation ID and opaque IDs are treated as confidential metadata: valid where required, masked/minimized for broad projection, and never authorization proof.
10. Verify sanitizer failure returns a closed safe failure, never the unsanitized source object.
11. Verify safe validation issues contain field identifiers and stable issue codes only, never rejected values.
12. Verify test failure output itself does not print the full canary-bearing request; assertions should report category/key, not secret value.

The desired behavior for forbidden Audit metadata is reject-and-record-safe-reason, not silently preserve a partly sanitized arbitrary property bag. A defensive redactor may remove known sensitive values before safe diagnostics, but successful Audit mapping still requires a positive event-version allowlist.

## 12. Risks and gaps

| Risk/gap | Unit-test implication | Required follow-up |
|---|---|---|
| Pertamina SSO protocol, issuer/audience/claims, provisioning, MFA/reauth method are unconfirmed | Unit tests can cover OTO state/mapping only, not real identity assurance | **REQUIRES_PERTAMINA_STANDARD_CONFIRMATION** and approved authentication integration tests |
| Reauthentication age/use/attempt/threshold policy is unconfirmed | No hard-coded duration or assurance assumption | Inject typed policy and add boundary tests after approval |
| Offline grant lifetime, trusted-time method, revision tolerance, and revocation propagation are unconfirmed | Offline suite tests generic expiry/integrity rules only | **REQUIRES_PERTAMINA_STANDARD_CONFIRMATION** plus platform integration/rollback tests |
| Exact organization/unit Admin, Support, and Audit scope vocabulary is unconfirmed | Scope fixtures may otherwise encode a false hierarchy | Approve scope model and expand data-driven cases |
| Concealed 404 policy may differ by resource family | One global 403/404 expectation could leak or hide incorrectly | Define per-resource disclosure policy and test ErrorMapper entries |
| Numeric DTO/header/rate/quota/date/batch limits move to approved policy/Document 12 | Fixed values would make tests brittle or invent Production rules | Test injected below/equal/above boundaries and add approved policy snapshots later |
| Event Type/Version and Safe Metadata catalog will evolve | Hand-written mapper tests may miss new events | Generate catalog completeness cases and require a test with every catalog change |
| Unit tests cannot prove middleware order, serializer behavior, generated OpenAPI, or actual HTTP headers | Pure tests can pass while endpoint wiring is wrong | Add Document 11 contract tests without turning this suite into full HTTP E2E |
| Unit tests cannot prove SQL unique/append permissions, transaction race, SQLite durability, or object authorization before bytes | Mock/fake behavior can overstate assurance | Add focused integration tests for API-DB-object/outbox/idempotency boundaries |
| Unit tests cannot prove log/trace exporters, crash dumps, proxy logs, or test runner artifacts are clean | Capture-logger tests cover application mapping only | Add pipeline/config/integration scanning and forbidden-canary checks |
| Sanitization cannot reliably identify arbitrary secrets by value | Heuristic redaction can miss data | Use closed typed DTO/event schemas and source-aware allowlists as primary control |
| Ordinary-L2 behavior when protected Audit queue is full/unavailable is unconfirmed | Expected allow/block state cannot be finalized | **REQUIRES_PERTAMINA_STANDARD_CONFIRMATION**; L3 remains fail closed |
| Rate-limit/admission unit logic does not prove distributed atomicity or retry-storm behavior | Fake reservation may hide races | Add concurrency/load tests in later layers |
| Assistant ambiguity/confidence thresholds are not numerical contracts | Hard-coded model score tests would be unstable | Test deterministic ambiguity categories/rules; benchmark/approve model threshold separately |

No unit test in this report substitutes for VA/PenTest, malicious-file corpus/fuzzing, real proxy/API parsing tests, cross-process Worker/IPC tests, database security verification, or real identity/provider exercises.

## 13. Suggested Document 11 text

The following text is ready for integration into Document 11.

### 13.1 Unit-test platform and boundary

Security, API-contract logic, and Audit mapping unit tests SHALL use MSTest with Microsoft.Testing.Platform. The unit layer SHALL test deterministic authentication-support state, authorization, permission, policy, resource ownership, offline eligibility, reauthentication decisions, DTO/query/header validation, admission semantics, idempotency, error mapping, sanitization, Audit mapping, and Assistant parity.

The unit layer SHALL NOT simulate full HTTP E2E, real Pertamina SSO, real external provider, penetration testing, database permissions, object storage, or cross-process behavior. Those concerns SHALL be covered by the applicable contract, integration, authorization, E2E, performance/abuse, VA, and penetration-test layers.

### 13.2 Authorization decision tests

Authorization tests SHALL evaluate:

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

Every permission family SHALL include ALLOW, missing permission, wrong role/profile, wrong owner/scope, disabled account, expired/revoked session, expired/revoked/stale permission snapshot, policy denial, wrong state/version, offline, online-required, reauthentication-required, and safe no-leakage cases. Unknown or stale security input SHALL fail closed.

The server-derived Principal ID, effective permissions, owner/scope, policy revision, and resource facts SHALL be authoritative. Client-supplied actor, role, permission, owner, unit, scope, classification, confirmation, reauthentication, Job ID, Correlation ID, fileRef, cursor, ETag, or storage reference SHALL NOT create authority.

### 13.3 Ownership and role boundaries

Unit tests SHALL prove User A cannot access User B upload, Job, result, history, secured file/version, Issue, attachment, export, or content. Administrator SHALL be denied user document/result/OCR/secured content by default. Auditor SHALL be limited to explicitly granted scoped safe Audit list/read/export and SHALL have no Audit update/delete. Support SHALL be limited to the exact scoped Issue and explicitly sent attachment; it SHALL have no password, token, source document, unrelated Job/result/file, signature, clipboard, or full OCR authority.

Parent and child membership SHALL be validated before content/storage resolution. Concealed resources SHALL map consistently to the approved 404 policy without leaking existence, state, version, retention, or timing.

### 13.4 Offline and reauthentication

Offline unit tests SHALL validate protected account/profile/device binding, absolute expiry, trusted-time/rollback evidence, current allowed revision, offline-eligible local capability, classification ceiling, and last learned revoke/disable state. Offline authority SHALL NOT authorize upload, server processing, secured storage, server Issue, Admin, Support, Audit query/export, realtime, workload operations, or any reauthentication-required action.

Reconnect SHALL refresh identity, account, session, permission, policy, capability, and compatibility before dispatching a queued command. Queued intent SHALL NOT be treated as earlier authorization.

Fresh reauthentication SHALL be scoped to the principal, session, client, action, resource/scope, policy and permission revisions, assurance, and approved lifetime. Logout, disablement, revocation, revision/resource mismatch, or expiry SHALL invalidate it. Fresh authentication SHALL NOT replace permission, ownership, confirmation, policy, or resource-state checks. Missing required freshness SHALL map to safe 401 REAUTHENTICATION_REQUIRED before mutation.

**REQUIRES_PERTAMINA_STANDARD_CONFIRMATION:** reauthentication method/age/use/attempt policy, offline grant lifetime/trusted-time/revision rules, and revocation propagation objective.

### 13.5 API validation and HTTP decision logic

Command contracts SHALL be closed, typed, versioned, and bounded. Unit tests SHALL reject malformed or ambiguous payloads, unknown fields/enums/types/versions, invalid canonical Feature IDs, missing handlers, invalid file references, invalid typed parameters, invalid workflow graphs, and client-selected engine/provider/class/assembly/shell/path/URL/SQL/command.

Pagination tests SHALL reject page plus cursor, zero/negative/excessive page values, tampered/expired/cross-scope cursors, invalid date ranges, and non-allowlisted filter/search/sort expressions without silent clamping. Authorization scope SHALL remain server-derived and separate from client filters.

HTTP decision tests SHALL verify 202 can be created only after durable acceptance and includes the original Job ID/status location/Correlation ID. Admission logic SHALL distinguish 429 caller limit with Retry-After, 413 hard payload limit, 409 state/storage conflict, and 503 shared unsafe saturation. Numeric limits SHALL be injected from approved policy and tested below, at, and above the boundary.

### 13.6 Idempotency

Idempotency tests SHALL scope the key by authenticated principal/tenant, method, canonical route, and Contract Version. The fingerprint SHALL include canonical request semantics, Feature/schema version, immutable resource version, and content digest where relevant, while excluding passwords and secrets.

The suite SHALL verify new-key process-once, committed replay, running-job replay to the same Job/Location, in-progress conflict, same-key/different-fingerprint conflict, restart after failure-before-commit, scope separation, and concurrency winner behavior. Different keys SHALL represent different user intents even when content is identical.

Audit deduplication tests SHALL use stable Audit/Client Event ID plus trusted source scope and Event Fingerprint. Same ID/same fingerprint SHALL acknowledge one event; same ID/different fingerprint SHALL conflict without overwrite. Correlation, Job, Feature, Event Type, Outcome, and timestamp SHALL NOT be deduplication keys.

### 13.7 Safe errors and sensitive-data tests

The error mapper SHALL emit only a stable code, safe/localizable message, Correlation ID, and an approved bounded details union. Unit tests SHALL cover the Document 06 semantics for 400, 401, 403, concealed 404, 409, 410, 412, 413, 415, 416, 422, 429, 500, 502, 503, and 504 where the component owns that decision.

A shared synthetic canary suite SHALL prove that password, token, cookie, secret/key, document bytes/text, OCR/extracted/translated content, signature/paraf, clipboard, prompt/chat/provider payload, screenshot/Issue content, raw path/storage key, SQL, exception/stack, claims, and authorization headers do not enter errors, logs/traces/metrics, idempotency snapshots, Audit/outboxes/exports, Assistant metadata, alerts, or test failure artifacts. Closed typed allowlisting SHALL be primary; sanitizer failure SHALL fail closed.

### 13.8 Audit mapping

Audit unit tests SHALL verify registered Event Type/Version, Category/Level, stable event identity, Occurred/Received time semantics, actual Actor, minimized snapshot, Origin, Executor/On-Behalf-Of, Action, Resource, Outcome, safe reason/error, canonical Feature ID, Job ID where applicable, Correlation ID, execution location/mode, and exact Safe Metadata schema.

All 33 Core Tool executions SHALL map to approved L2 accepted/terminal evidence. Protect, Unlock, visual non-PKI Sign, and Redact SHALL also map to their approved L3 finalization events. No password, document/OCR content, signature image, redaction text/coordinates, filename/path, recipient secret, prompt/provider payload, Issue content, raw SQL, or exception SHALL be emitted.

Assistant-triggered execution SHALL retain the real User as Actor and use Origin=ASSISTANT. The underlying canonical Feature event is the default evidence; the mapper SHALL NOT create an unnecessary duplicate Assistant event. Unknown Feature/Event Type/version/metadata SHALL be rejected rather than invented or coerced.

### 13.9 Assistant parity and acceptance

The Assistant SHALL have no tool permission of its own. A shared data-driven suite SHALL prove that File-First, Tool-First, and Assistant-First receive the same authorization result for the same subject, action, resource, and context. Every workflow step SHALL be validated and authorized independently at execution time.

Assistant tests SHALL reject an invalid/invented/hidden Feature ID, invalid typed parameters, arbitrary operations/providers, cross-user resources, confirmation bypass, stale permission, and online-only action while offline. Ambiguous destructive/sensitive requests SHALL produce clarification; clear high-risk requests without approval SHALL produce confirmation or reauthentication as required. Assistant success text SHALL be grounded in the actual validated tool/Job result.

The security unit suite is accepted only when all decision matrices pass, every registered permission/action/event mapper has coverage, every forbidden-data canary is absent from every safe output, and no unit test claims assurance that belongs to an integration, E2E, performance, VA, or penetration-test layer.
