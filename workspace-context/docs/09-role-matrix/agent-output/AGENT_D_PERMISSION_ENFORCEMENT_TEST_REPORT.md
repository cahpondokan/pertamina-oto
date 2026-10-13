# AGENT D — Permission Enforcement & Authorization Test Report

**Task ID:** `DOC09-AGENT-D`  
**Scope:** Enforcement, offline authorization, SignalR, Worker, Assistant, testing, performance, and .NET implementation readiness  
**Source baseline:** Documents 01–08 FINAL  
**Status:** Integration recommendation for Document 09; no production code and no change to the security architecture or Document 09 master.

This report preserves the locked baseline:

- baseline roles are only **User**, **Administrator**, and **Auditor**;
- **Superadmin** is a Support permission bundle/surface, not a new all-powerful role;
- role groups permissions but is never the final authorization decision;
- the backend controls role and permission;
- UI visibility is UX, not a security boundary;
- server-side operations receive final authorization in the API/application service;
- local Desktop/Mobile actions pass through a shared application/security service;
- SignalR is authenticated notification-only; REST remains authoritative;
- Worker does not decide human business permission;
- Assistant uses the same canonical Feature/application authorization path;
- offline authority is narrow, protected, time-bounded, local-only, and revalidated on reconnect.

Names such as `P-01` in this report are **test-analysis identifiers**, not new canonical Permission IDs. Exact permission codes, role mappings, delegation, separation of duty, self-assignment rules, and reauthentication matrix remain `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` for Document 09 integration.

Primary traceability: `FR-AUTH-001..005`, `FR-ROLE-001..003`, `FR-AUDIT-001..004`, `FR-SUP-001..007`, `SR-SEC-001..023`, `UIR-UI-005..012`, `FEAT-AUTH-001..012`, `FEAT-ADMIN-001..022`, `FEAT-AUDIT-001..008`, `FEAT-ASSIST-001..030`, `FEAT-SUPPORT-001..008`, Document 06 §§9–18, Document 07 §§15–17, and Document 08 §§8–15, 21, 27, 34–36.

---

## 1. Enforcement architecture

### 1.1 Mandatory decision flow

```text
Client intent
→ Authentication
→ Account and session eligibility
→ Action / canonical Feature permission
→ Enterprise policy and classification
→ Resource ownership or explicit scope
→ Resource state and version
→ Context: platform, online/offline, compatibility, capability,
           consent, risk/confirmation, reauthentication freshness
→ Official application service
→ Local engine / API job / approved provider / bounded Worker execution
→ Validated result and safe evidence
```

Equivalent authorization formula from Document 08:

```text
Allow = Authenticated
    AND AccountActive
    AND SessionValid
    AND ActionOrFeaturePermission
    AND ResourceOwnershipOrExplicitScope
    AND EnterprisePolicyAndClassification
    AND ResourceStateAndVersion
    AND ClientCompatibilityAndCapability
    AND RequiredConsent
    AND FreshReauthenticationWhenRequired
```

An unknown identity, session, permission, Feature, resource, state, policy version, contract version, or security dependency fails closed. Role name alone never satisfies the formula.

### 1.2 Enforcement ownership by boundary

| Boundary | May do | Must not do |
|---|---|---|
| Web/Desktop/Mobile UI | Hide/disable unavailable action; explain offline/online/reauth state; request confirmation | Treat visibility, route, menu, client claim, or button state as authorization |
| Shared client application/security service | Evaluate protected local/offline grant; create typed intent; invoke official local/server application path | Scatter role checks across views/view-models; mint permission; authorize server resource |
| API authentication/session boundary | Validate credential/session/account/client and construct server-derived principal/security context | Trust client-supplied user, role, owner, permission, or scope |
| Application service | Make final server action decision using current permission, policy, ownership/scope, state/version, consent, and reauth; create durable authorized job/operation | Delegate business authorization to UI, queue, SignalR, storage, engine, or Worker |
| Repository/data query | Apply owner/scope before count/filter/cursor/projection and return only needed authorization metadata/content | Load an unrestricted collection then filter in memory; treat ID/storage key as access |
| SignalR Hub | Authenticate connection; authorize server-derived group/per-job subscription; publish safe notification | Mutate business state; accept arbitrary user/role/group; treat event as permission or truth |
| Local Worker | Validate closed contract/instance/job/path/budget; execute whitelisted operation within local resource boundary | Re-evaluate roles, obtain arbitrary local data, elevate to Administrator, or accept arbitrary commands |
| Server Worker | Authenticate workload identity; reload authoritative job; validate handler/schema/ref/state/lease; honor cancel/quarantine policy | Receive end-user token; infer human permission from queue; browse unrelated jobs/content |
| Assistant | Propose visible canonical Feature/typed plan and call the same application service | Use a parallel permission model, direct SQL/shell/filesystem/API, or infer privileged action |
| Storage/DB | Enforce least-privileged technical identity and return data only through owner module | Grant business access merely because a caller has a path, file reference, key, or DB connection |

### 1.3 Decision semantics

Implementation should return a small typed decision rather than Boolean-only ambiguity:

| Outcome | Meaning |
|---|---|
| `ALLOW` | All current checks passed for this exact action/resource/context |
| `DENY` | Authenticated principal lacks permission, scope, policy eligibility, state, or capability; server uses safe `403` or concealed `404` as applicable |
| `AUTHENTICATION_REQUIRED` | Missing/invalid/expired/revoked authentication; safe `401` |
| `REAUTHENTICATION_REQUIRED` | Valid base session, but freshness for the exact sensitive action/resource is absent; safe `401` before mutation |
| `ONLINE_REQUIRED` | A server/current-authority action cannot be executed from offline authority; no server side effect |
| `UPDATE_REQUIRED` | Client/contract compatibility prevents safe action |

These are conceptual application outcomes. Existing Document 06 error envelopes and HTTP semantics remain authoritative; this report does not add an endpoint or incompatible error contract.

### 1.4 Role and content separation

```text
Administrator authority ≠ user-document content access
Support permission       ≠ source/job/result/secured-file browsing
Auditor authority        ≠ audit mutation or document access
Operations/Worker        ≠ human impersonation or content access
```

The final Role Matrix must express permissions at action/resource scope, not broad screen access. A screen may contain actions with different permissions, ownership, reauthentication, and audit requirements.

---

## 2. Web enforcement

### 2.1 Authentication and server boundary

- Web uses the protected server-side session boundary from Document 08; browser-accessible bearer/refresh-token storage is prohibited.
- Protected MVC/API/Hub surfaces use an authenticated fallback policy. Only explicitly documented pre-auth compatibility/minimal health responses are exceptions.
- Cookie-authenticated mutations require anti-forgery validation, including logout, administration, session, issue, and export actions.
- SignalR Web authentication uses the same-site protected `Secure`, `HttpOnly`, narrowly scoped session cookie and independently validates WebSocket Origin.
- User ID, role, permission, owner, support scope, or policy supplied in form/query/body/header is ignored or rejected; the server derives these from the current principal and authoritative stores.

### 2.2 UI behavior

The Web UI may use the server-provided filtered Feature/capability projection to hide or disable actions. It should distinguish:

- unavailable by platform/capability;
- permission denied;
- connection/server required;
- update required;
- reauthentication required;
- resource no longer available.

The UI must not disclose hidden Feature descriptors, object existence, restricted counts, or support/audit/admin navigation to unauthorized callers. A direct URL or handcrafted request always reaches the same server authorization and cannot rely on route/menu secrecy.

### 2.3 Server-rendered and API-backed pages

Page authorization is a coarse entry gate. Each application operation still performs action/resource authorization. A page accessible to an Administrator does not imply every action on it, and a support page permission does not imply attachment/export permission.

Lists are owner/permission scoped before count, filter, search, sort, cursor, and projection. Details authorize parent and child relationship before content/attachment retrieval. Mutation uses current state/version and `If-Match` where the Document 06 contract requires it.

### 2.4 Web offline boundary

Documents 01–08 do not establish a browser/PWA offline authorization profile. A cached HTML page, service-worker asset, or stale client projection therefore grants no offline business authority. If future Web offline capability is proposed, it requires an approved architecture/contract change and must implement the same protected-grant rules as Section 9.

---

## 3. Desktop enforcement

### 3.1 Online identity and application path

- Native Desktop uses the approved Pertamina flow with PKCE and approved OS credential storage.
- The Desktop presentation layer receives a safe permission/capability projection for UX only.
- File-First, Tool-First, and Assistant-First all call the same shared application/security service with canonical Feature ID, typed parameters, resource reference, and execution context.
- Local application services evaluate local ownership/profile binding, grant validity, Feature permission, platform capability, policy/classification, file access, resource state, confirmation, and whether online/reauth is required.
- Server actions call the official API, which performs final current authorization again.

### 3.2 Avoid distributed role logic

Views, commands, menus, and view-models should bind to an `ActionAvailability`/capability projection rather than contain repeated `if (role == ...)`. The projection is calculated by one client authorization facade and includes a reason such as available, denied, online required, reauth required, unsupported, or update required.

This projection cannot authorize a server call and cannot be passed to the Worker as a capability token.

### 3.3 Local resource and profile boundary

- Local state is bound to the active OS profile plus Pertamina subject profile; account switching closes one store and opens another without implicit merge.
- Local files remain user-selected resources. The shared service validates current path/access and app-managed artifact ownership before each operation.
- Logout stops command dispatch, invalidates offline authority, clears in-memory credentials/keys/state, and locks/unmounts the local profile without silently deleting work.
- Local action audit metadata is queued through the controlled outbox; it never contains document content, password, token, signature, screenshot, clipboard, or full OCR text.

### 3.4 Local Worker call

Only an already evaluated local application operation may launch the Local Worker. The contract contains canonical Feature ID, typed parameters, job-scoped input/artifact references, deadline/budget, and Correlation ID. The Worker independently validates contract shape, allowlist, instance/job binding, path/resource boundary, and limits, but does not decide the user's role/permission.

Direct client/third-party connection to the Worker, stale Worker completion, arbitrary path/operation, or a different profile/job must be rejected.

---

## 4. Mobile enforcement

### 4.1 Curated capability boundary

Mobile/Tablet uses the official Web API and never connects directly to SQL Server or object storage. Responsive layout does not imply Desktop capability. Device features such as camera, scanner, drawing, file-provider access, or background execution are capability inputs—not business permissions.

Effective availability is:

```text
platform exposure
AND device/OS capability
AND valid permission projection
AND policy/classification
AND online/offline state
AND engine/provider availability
```

### 4.2 Identity, profile, and local data

- Use approved native SSO/PKCE and OS credential storage.
- Keep separate protected local profiles per account/subject binding; account switch never merges state.
- Apply the same shared application/security service before camera/import/scan/local-tool/Assistant actions.
- Store only the protected minimum offline grant, not raw token/cookie or unrestricted claims.
- Exclude local-only stores, DB/WAL/SHM, migration backup, and artifacts from automatic cloud/device backup using the approved platform mechanism. Lack of that mechanism remains a release blocker under Document 08.

### 4.3 Background and reconnect behavior

Mobile background callbacks do not become a separate trust path. They reopen the correct protected profile and use the same serialized persistence and authorization service. If the offline grant is expired, time integrity is uncertain, account context differs, or the action needs server/reauth/current permission, the callback records a safe waiting/denied state and performs no side effect.

On reconnect, refresh identity/account/session/permission/policy/capability/compatibility before dispatching pending commands. A queued intent is reauthorized; it is not evidence that prior authorization remains valid.

---

## 5. API enforcement

### 5.1 Layered server decision

```text
HTTPS / approved proxy
→ authentication and client/session validation
→ authenticated fallback policy
→ endpoint-level coarse permission policy
→ load minimum resource authorization metadata
→ resource-based authorization: owner or explicit scope
→ policy/classification/state/version/consent/reauth/capability
→ owning application service mutation/query
→ safe audit/outbox after commit
```

Endpoint attributes/policies prevent obvious entry, but the application service is the final server action boundary. Repository/storage/Worker checks are defense in depth and do not replace it.

### 5.2 Route-family enforcement

| Route family | Required final checks |
|---|---|
| Feature/workflow | Canonical visible Feature, registered handler/schema, permission per step, input ownership, policy/classification, feature flag, capability/engine/capacity/connectivity, consent, confirmation |
| Upload/session | Current user, purpose, upload permission/consent, owner, state/version, byte/count/storage limits, validation/quarantine, idempotency |
| Job/status/action/result | Owner or narrowly explicit safe support scope; current permission, state, retry/cancel/checkpoint, retention; fresh authorization on every read/control/result/range |
| Account/preferences/history | Current principal/owner; version for update; reprocess passes normal current Feature permission/policy; deleting personal history never deletes Enterprise Audit |
| Secured files/versions | Owner/explicit permission, version/ETag, storage/retention/recycle state, quota, confirmation; reauth for permanent/high-impact action when policy requires |
| Issue | Reporter owns message/status; attachment requires explicit preview/send and parent-child validation |
| Support | Separate list/detail/attachment/triage/note/status/export permissions; scope to issues/attachments explicitly submitted; no unrelated document/job/result access |
| Audit | Separate query/detail/export permissions; scope before count/query/export; no delete/mutation endpoint for Auditor |
| Admin/config/limits | Granular read/write permission, current policy version, `If-Match`, hard safety ceilings, reauth for high impact |
| Health/operations | Minimal public/infra health only; details require operations permission and contain safe metadata only |
| SignalR subscription | Valid connection principal/session/Hub permission; server-derived group; per-job ownership/permission; bounded subscriptions |

Opaque ID, Job ID, Correlation ID, cursor, `fileRef`, storage reference, connection ID, or event is never authorization.

### 5.3 Status and concealment

- `401`: no usable authentication or scoped reauthentication is required.
- `403`: identity valid but permission/ownership/policy denies.
- consistent concealed `404`: policy requires hiding sensitive resource existence.
- `409`/`412`: authorized request fails current state/version/ETag; these must not leak an unauthorized object's state.

Authorization precedes count, cursor, range, storage resolution, export creation, and content stream. Error/log/audit output remains content- and secret-safe.

### 5.4 Permission change and TOCTOU

Permission and policy revision must be current at authorization. Mutations validate version/state near the transaction boundary; a prior screen load or permission check cannot authorize a later change. Continuation, retry, resume, cancel, restore, permanent delete, export, range download, and result retrieval are independently reauthorized.

The API contract for User/Role/Permission Management and Session Revocation is not present in the Document 06 endpoint inventory even though related Feature IDs exist. This report does not invent endpoints; an approved Document 06 addendum/integrator decision is required before those Features can be implemented.

---

## 6. SignalR enforcement

### 6.1 Connection and subscription

The single `/hubs/activity` Hub remains notification-only.

1. Authenticate negotiate/connection using the same principal/session/account model as REST.
2. Validate session expiry/revocation, Hub permission, client compatibility, and browser Origin where applicable.
3. Derive the user group server-side from the authenticated principal; never accept client user/role/group/tenant/correlation values.
4. For optional per-job subscription, load the minimum Job metadata and verify owner or explicit authorized scope plus current permission and state.
5. Bound connections, subscriptions, handshake, keepalive, idle, reconnect, message/buffer, and slow-consumer behavior.
6. Publish only safe metadata after the authoritative transaction commits.

### 6.2 Leakage prevention

- User A never joins User B's group or receives User B's job/activity/issue event.
- A support permission does not grant a broad job group. Optional support visibility must be explicit and safe-metadata-only.
- Arbitrary Job ID, Correlation ID, issue ID, connection ID, group name, or guessed subscription is rejected.
- Event payload has no document/preview/OCR, password/token/secret, signature, prompt/chat, physical path/storage key, stack/raw engine message, or arbitrary display text.
- Hub has no execute, upload, cancel, retry, resume, admin, role, permission, issue, or other business mutation.

### 6.3 Reconnect and permission change

Known expiry, logout, disablement, revocation, or relevant permission change terminates or forces reauthentication/reconnect. Reconnect:

```text
revalidate identity + account + session + Hub permission
→ derive current user group
→ REST reconcile authoritative jobs/activity/issues
→ reauthorize each active per-job subscription
→ resume notification only
```

If authorization cannot be revalidated, realtime remains unavailable and REST itself must also deny unauthorized access. A stale connection or event never preserves authority. Duplicate/stale/gap/unknown events follow Document 06 reconciliation and cannot regress terminal state.

---

## 7. Worker boundary

### 7.1 Business authorization ownership

The API/shared application service decides the human business permission before durable execution is accepted. The queue message is a locator, not an authorization grant. Job admission context is safe trace metadata, not a capability token.

The Worker does not evaluate `User`, `Administrator`, `Auditor`, or Support bundle mappings and does not accept a forwarded user token.

### 7.2 Mandatory Worker validation

Even after application authorization, every Worker must:

- authenticate its workload/IPC identity and expected audience/purpose;
- reload the persisted authoritative Job/local operation;
- validate contract version, canonical Feature ID, registered whitelisted handler, typed schema, input/artifact references, Job/profile/environment binding, state/version/lease, deadline, and resource budgets;
- reject unknown/extra operation, arbitrary path/URL/provider/engine/class/assembly/SQL/shell/process/network command;
- enforce its allowed DB/object/queue/provider/path scope and never browse unrelated jobs/resources;
- validate engine/provider output before commit and never promote partial output to final;
- honor cancellation, quarantine, expiry, policy-stop marker, and bounded retry/checkpoint rules;
- emit safe normalized status only.

### 7.3 Revocation during execution

When revocation/disablement/permission or policy change is learned after admission:

- API immediately denies new control/read/result access;
- application-owned job policy determines whether work may continue to a safe point, cancel, or quarantine output;
- Worker reads that authoritative job/policy state at defined safe boundaries, rather than independently interpreting role/permission;
- no result is released until a fresh authorized retrieval.

This reconciles the requirement that Worker not decide business permission with Document 08's requirement to stop/quarantine when continuation is forbidden.

### 7.4 Local Worker specifics

Local Worker runs as the intended current user or stricter approved local identity, never Administrator by default. It uses authenticated, versioned, local-machine-only, instance/job-bound IPC; closed operation allowlist; restrictive ACL; protected per-profile/job working root; path/reparse defense; and CPU/RAM/time/temp/output/thread/handle/child-process limits. Direct or stale clients, cross-user requests, malformed/flooding peers, and job mismatch fail closed.

---

## 8. Assistant enforcement

### 8.1 Same authorization path

```text
User intent
→ minimum permitted context
→ visible canonical Feature ID / typed workflow
→ schema and input ownership
→ permission
→ policy/classification/consent
→ platform/offline/capability/provider/budget
→ ambiguity clarification
→ confirmation and reauthentication where required
→ official application service
→ actual result grounding
```

No model output can skip, reorder, or change these controls. Assistant has no separate role, elevation, service credential, or broad content permission.

### 8.2 Required authorization cases

| Attempt | Required outcome |
|---|---|
| Action user is allowed to perform | Allowed only when the Feature is Assistant-visible and the same permission/ownership/policy/context checks pass |
| Action user is not allowed to perform | Denied identically to UI/direct API; do not reveal hidden Feature/resource details |
| Sensitive action | Require the same explicit confirmation and scoped fresh reauthentication; AI inference alone is insufficient |
| Cross-user resource | Deny with safe `403`/concealed `404`; no other-user data enters model context |
| Admin/Support/Audit action | Deny unless explicitly exposed to Assistant and the principal holds the exact granular permission; current baseline must not infer exposure from role |
| Offline action | Only an offline-eligible local Feature under a valid protected grant; no server/admin/support/audit-export/reauth-required action |
| Multi-step workflow | Check every step, intermediate input/output ownership, confirmation, and current capability; one denied step stops the plan safely |

### 8.3 Tool and data boundary

- Assistant chooses only canonical active visible Feature IDs and typed parameters.
- It cannot call direct SQL, shell, process, filesystem, arbitrary HTTP/API/URL, plugin, engine, provider, Hub, or Worker.
- Document/OCR/HTML/metadata instructions are data, never authority.
- Public AI/translation routing remains technically blocked.
- Assistant output cannot claim success before validated persisted tool/job result.
- Password, token, secret, signature, sensitive clipboard, unrelated user data, and unnecessary document pages never enter plan/history/provider context.

---

## 9. Offline behavior

### 9.1 Ten required offline decisions

| No. | Topic | Required behavior |
|---:|---|---|
| 1 | Cached claims | Do not cache unrestricted claims. Store a protected minimum projection: internal principal/profile binding, offline-eligible permissions/capabilities, policy/classification ceilings, versions, issued/last-online/absolute-expiry, integrity/time evidence, device/profile binding where supported, and last learned revoke/disable marker |
| 2 | Expiry | Absolute configurable expiry only; no sliding renewal offline. Trusted last-online plus monotonic elapsed time where available; reboot/sleep/timezone/DST/wall-clock rollback cannot extend authority; uncertain time fails closed |
| 3 | Permission revision | Grant carries permission/policy/capability/contract revisions. A cached revision authorizes only listed local capability within its lifetime; it never authorizes a current-state server action |
| 4 | Account revoked while offline | Immediate knowledge is impossible without connectivity. Residual risk is bounded by local-only scope and absolute expiry. If a last-learned marker exists, deny. On reconnect, refresh before any queued command and invalidate the grant when revocation is learned |
| 5 | Device reconnect | Refresh identity, account/session, permission/policy, capability/compatibility; invalidate stale cache; reconcile REST; reauthorize each pending intent and subscription before dispatch |
| 6 | Sensitive offline action | Deny any action requiring current server status, fresh authentication, high-impact disclosure/export, permanent secured deletion, role/permission/security change, session revocation, key/destructive recovery, or policy-defined classified transfer |
| 7 | Online-required actions | Upload/server processing, secured storage/version server action, cross-device access, issue send/status, audit query/export, Admin/Support/Auditor server surfaces, SignalR, and reauthentication-required actions wait/deny until online authority |
| 8 | Server-dependent action | UI may record a minimized typed pending intent if the existing product flow permits; it cannot upload/execute, claim success, or reuse an earlier authorization grant while offline |
| 9 | Offline audit metadata | Queue safe append-oriented action metadata in the protected local outbox; no content/password/token/clipboard/screenshot/full OCR/signature; include enough profile/action/Feature/time/result/version context for idempotent server ingestion |
| 10 | Sync after reconnect | Audit outbox sync is separate from command/content sync. Pending commands reauthorize current ownership/state/consent/reauth. Current denial prevents the side effect but does not erase truthful offline audit evidence |

### 9.2 Offline action classification

| Action class | Offline result |
|---|---|
| Eligible deterministic local Feature on user-selected local file | `ALLOW` only while protected grant, policy, profile, capability, file access, and time integrity remain valid |
| Local draft/preview/undo/history/recovery | `ALLOW` within protected profile and data/retention policy |
| Local destructive action | Only if explicitly offline-eligible, owned/app-managed, correctly confirmed, and not reauth/current-server dependent; external source remains protected |
| Upload, server processing, secured-account transfer/version/restore/delete | `ONLINE_REQUIRED`; pending intent may wait but is not authorized |
| Issue submission/status and support attachment | `ONLINE_REQUIRED`; screenshot remains local preview until explicit send and fresh online authorization |
| Admin, Support console, Auditor query/export, operations server detail | `ONLINE_REQUIRED` and denied offline |
| Assistant local orchestration | Same local eligibility checks per step; no permission widening |

### 9.3 Reconnect order

```text
network available
→ compatibility check
→ refresh/validate identity and application session
→ load current account status and permission/policy revision
→ replace/invalidate client projection and offline grant as policy permits
→ reconcile REST-owned state
→ reauthorize each pending intent with current resource/state/consent/reauth
→ dispatch allowed intents using existing idempotency key
→ sync safe offline audit metadata idempotently
→ reauthorize SignalR connection/subscriptions
```

No queue is flushed before the current authority is known. Account/profile mismatch closes the queue for that profile and never merges it into another account.

---

## 10. Permission cache strategy

### 10.1 Cache types and authority

| Cache/projection | Purpose | Allowed contents | Not authority for |
|---|---|---|---|
| Client online UI projection | Avoid permission/network lookup per button; drive availability and explanations | Safe visible Feature/action availability, platform/capability, revision, expiry/refresh hint | Server action, object ownership, sensitive freshness |
| Protected offline grant | Authorize explicitly eligible local operations during bounded offline session | Minimum fields in §9.1; integrity/profile/time evidence | API/Hub, privileged server permission, current revocation truth, reauth |
| Server request security context | Resolve principal/session and effective permissions once per request/operation | Server-derived IDs, permission set, revisions, policy context; no raw token in business layer | A later request after revision/session change |
| Server permission cache | Reduce repetitive backend lookup | Revision-aware effective permission mapping keyed by principal/session/scope/environment | Permanent authorization; sensitive operation without current revision/revalidation |
| Resource authorization projection | Avoid loading content/entity graph | Owner ID, parent ID, scope/state/version/classification/tombstone only | Content disclosure before authorization |

### 10.2 Safe caching rules

1. Cache permission sets, not repeated role conditionals. Role is an input to backend permission resolution only.
2. Key server cache by stable internal principal, application session/security scope, environment, and permission/policy revision; never by display name/email alone.
3. Use a bounded TTL plus authoritative revision/invalidation. Exact duration is `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` and must be shorter for higher-risk administration/reauth contexts.
4. Permission/role/account/policy/session changes publish invalidation to affected server cache and clients where possible. SignalR notification is best effort, not the only mechanism.
5. Every server action still checks current session/account and revision. Sensitive mutations, role/permission/session changes, permanent deletion, and high-impact export bypass stale positive cache or require explicit freshness.
6. Client projection is replaced atomically. Never merge an older revision into a newer one.
7. Logout, account switch, disablement, revocation, permission change, reauthentication expiry, app update incompatibility, or profile corruption invalidates the relevant cache/grant.
8. Negative/denied results may be cached only narrowly and briefly; they must not reveal object existence or survive an approved grant longer than policy permits.
9. Cache logging uses safe revision/decision/reason metadata, never full claim/token or restricted content.

### 10.3 Avoid per-button and N+1 authorization

- Return one filtered Feature/action availability projection for the current page/context rather than one network call per button.
- Resolve the current server permission set once per request or application operation, then evaluate multiple requirements in memory.
- Scope SQL queries by owner/explicit authorized scope before count/filter/cursor; project only ownership/state/version fields needed by the handler.
- Batch resource-authorization metadata only for bounded visible rows and do not preload content.
- Do not issue a separate permission DB query from every authorization handler; handlers share the request security context.
- For workflows, resolve the permission snapshot once, but evaluate each step's Feature/resource/policy/state individually.

Document 08 requires version/invalidation, but Document 06 does not lock the exact permission-revision response field or refresh contract. The Integrator must align this with existing profile/configuration/feature contracts or approve an API addendum; this report does not invent a new endpoint/header.

---

## 11. Authorization test matrix

### 11.1 Test fixtures and outcome legend

Use stable synthetic fixtures, not Production identities or files:

- **User A** and **User B**: baseline User principals with separate profiles, files, jobs, history, secured files, and issues;
- **Administrator without content permission**: administrative permissions only, proving administration is not document access;
- **Auditor query-only** and **Auditor export-enabled**: separate audit permissions, no mutation/content access;
- **Support List**, **Support Detail/Attachment**, **Support Triage/Note**, and **Support Export**: permission bundles/fixtures, not new roles;
- **Operations metadata-only**: health/job/queue safe metadata, no user-content access;
- expired, disabled, revoked, permission-revision-old, offline-grant-valid, offline-grant-expired, wrong-profile, and reauthentication-stale variants;
- authenticated Local Worker/Server Worker identities with job-scoped technical permissions only.

Legend:

| Code | Expected result |
|---|---|
| `A` | Allow when all other permission/policy/ownership/state/context checks pass |
| `D403` | Deny; authenticated but permission/policy/scope fails |
| `D404` | Consistent concealed not-found for a sensitive resource where policy requires |
| `U401` | Authentication/session invalid or expired |
| `R401` | Scoped fresh reauthentication required before any mutation/disclosure |
| `O` | Online/current server authorization required; no side effect while offline |
| `L*` | Allowed locally only under a valid protected offline grant and all local checks |
| `SAME` | Assistant must produce the same Allow/Deny/Reauth/Online result as ordinary UI/application service |
| `X` | Surface/action is not exposed; deny without adding a new capability |
| `N/A` | Scenario does not apply to that permission family |

Scenario columns mean:

- **ALLOW**: correct principal, permission, owner/scope, policy, state, and context;
- **DENY**: identical request with the action permission absent;
- **WRONG OWNER**: valid permission but resource belongs to User B or another profile/scope;
- **EXPIRED**: online/application session or applicable offline grant expired;
- **REVOKED**: permission/account/session revocation is current/known;
- **OFFLINE / ONLINE**: connectivity mode with otherwise valid context;
- **REAUTH**: sensitive action attempted without required freshness;
- **WRONG ROLE**: a baseline role not mapped to the permission and no explicit approved grant;
- **DIRECT API**: unauthorized caller bypasses UI and invokes the route directly;
- **ASSISTANT**: the same action is attempted through Assistant.

### 11.2 Permission-family scenario matrix

| ID | Important permission/action family | ALLOW | DENY | WRONG OWNER | EXPIRED | REVOKED | OFFLINE | ONLINE | REAUTH | WRONG ROLE | DIRECT API | ASSISTANT |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `P-01` | Discover/execute an eligible **local** canonical Feature on own selected file/profile | `A` | `D403` | `D403` profile/resource | `U401` or expired-grant deny | deny when known; bounded residual until grant expiry if unknowable offline | `L*` | `A` | `A` unless policy-sensitive | `D403` | `N/A` local; server spoof denied | `SAME` |
| `P-02` | Execute canonical **server** Feature/workflow and upload own input | `A` | `D403` | `D404` | `U401` | `D403/U401` | `O` | `A` | per action policy | `D403` | `D403/D404` | `SAME` per step |
| `P-03` | Read own Activity/Job/result/download/range | `A` | `D403` | `D404` | `U401` | `D403/U401` | `O` for server state | `A` | normally `A`; high disclosure policy may require freshness | `D403` | `D404` | `SAME` if Assistant-visible |
| `P-04` | Cancel/retry/resume own Job | `A` if state permits | `D403` | `D404` | `U401` | `D403/U401` | `O` | `A` | per risk policy | `D403` | `D403/D404` | `SAME` plus confirmation where required |
| `P-05` | Read/manage local draft, preview, undo, local history, recovery in active profile | `A` | `D403` | `D403` wrong profile | expired-grant deny | deny when known | `L*` | `A` | normally `A` | `D403` | `N/A` | `SAME` for exposed local Feature |
| `P-06` | Read/delete/reprocess own **server personal history** | `A` | `D403` | `D404` | `U401` | `D403/U401` | `O` | `A` | reprocess uses current Feature policy; delete per policy | `D403` | `D404` | `SAME` if exposed; no Audit deletion |
| `P-07` | List/read/download own secured file/version | `A` | `D403` | `D404` | `U401` | `D403/U401` | `O` | `A` | high-disclosure policy may require `R401` | `D403` | `D404` | `SAME` only if explicitly exposed |
| `P-08` | Create secured file; add/restore version; restore from Account Recycle | `A` | `D403` | `D404` | `U401` | `D403/U401` | `O` | `A` | confirmation/reauth per policy | `D403` | `D403/D404` | `SAME` plus consent/confirmation |
| `P-09` | Permanent secured-file deletion | `A` after policy/confirmation/freshness | `D403` | `D404` | `U401` | `D403/U401` | `O` | `A` only after checks | `R401` when freshness absent | `D403` | `D403/D404` | `SAME`; AI inference alone never sufficient |
| `P-10` | Submit own issue, explicitly send attachment, view own simple issue status | `A` | `D403` | `D404` | `U401` | `D403/U401` | `O`; screenshot may remain local preview | `A` | normally `A` | `D403` | `D403/D404` | `SAME` only if Feature exposure permits; explicit Send remains required |
| `P-11` | Support issue list/detail/explicit attachment access | `A` within exact support scope | `D403` | `D403/D404` outside support scope | `U401` | `D403/U401` | `O` | `A` | high-classification disclosure may require `R401` by final matrix | `D403` | `D403/D404` | `X` unless explicitly Assistant-exposed |
| `P-12` | Support triage/status/category/priority and internal note | `A` with separate action permission | `D403` | `D403/D404` | `U401` | `D403/U401` | `O` | `A` + state/ETag | reauth per high-impact matrix | `D403` | `D403/D404` | `X` by safe default |
| `P-13` | Support PDF/Excel export | `A` with export permission and bounded scope | `D403` | `D403/D404` | `U401` | `D403/U401` | `O` | `A` | `R401` when classified/high-impact export policy requires | `D403` | `D403/D404` | `X` unless explicitly exposed and freshly authorized |
| `P-14` | Audit query/search/filter/detail | `A` within approved audit scope | `D403` | `D403` scope; no ownership bypass | `U401` | `D403/U401` | `O` | `A` | normally `A`, policy may require freshness for sensitive scope | `D403` | `D403` before count/query | `X` unless explicitly exposed; no document content |
| `P-15` | Audit export | `A` with separate export permission | `D403` | `D403` scope | `U401` | `D403/U401` | `O` | `A` | `R401` when high-impact export policy requires | `D403` | `D403` | `X` by default |
| `P-16` | Read policy/feature flag/limit/health configuration metadata | `A` with granular read permission | `D403` | `D403` environment/scope | `U401` | `D403/U401` | `O` | `A` | normally `A` | `D403` | `D403` | `X` unless explicitly Assistant-exposed |
| `P-17` | Write policy/feature flag/limit/security/classification/maintenance/min-version | `A` within hard safety ceiling and version | `D403` | `D403` scope | `U401` | `D403/U401` | `O` | `A` + `If-Match` | `R401` for high-impact category per final matrix | `D403` | `D403` | `X` by safe default |
| `P-18` | User/Role/Permission management and session revocation | `A` only after exact permission/SoD/freshness | `D403` | `D403` target/scope | `U401` | `D403/U401` | `O` | `A` only when API contract exists | `R401` | `D403` | deny until approved API contract; then `D403` for unauthorized | `X` by safe default |
| `P-19` | Operations job/queue/sync/error/server/storage health **safe metadata** | `A` | `D403` | `D403` scope | `U401` | `D403/U401` | `O` | `A` | normally `A` | `D403` | `D403` | `X`; Assistant receives no operations elevation |
| `P-20` | SignalR personal activity/user group and owned per-job subscription | `A` | `D403`/disconnect | `D404`/reject subscription | `U401`/disconnect | disconnect/revalidate | `O` | `A` | `N/A`; base session freshness still valid | `D403` | direct arbitrary group/job subscribe denied | `X`; Assistant does not use Hub as command channel |
| `P-21` | Generate and batch-sync mandatory safe offline audit metadata | system/application path under authenticated client policy | cannot be disabled by ordinary user | profile/actor mismatch rejected | sync waits; evidence retained under policy | current auth controls sync; do not erase truthful local evidence | queue locally | idempotent authenticated sync | no human reauth merely to preserve mandatory metadata unless policy says otherwise | role does not suppress mandatory audit | forged batch/actor/revision rejected | `X`; Assistant cannot alter/suppress audit |

Notes:

1. Exact support, audit, admin, reauthentication, and delegation mappings are Document 09 decisions. A role without a permission is denied; a permission must not be inferred only from the role name.
2. A revocation that occurs while a device is truly offline cannot be learned immediately. This is an acknowledged bounded residual risk, not a reason to grant server/sensitive authority offline.
3. `FEAT-ADMIN-001..003` and `FEAT-AUTH-007` exist, but the corresponding management/revocation API contract is not defined in Document 06. `P-18` cannot be Production-ready until that gap is resolved through governance.

### 11.3 Minimum layer coverage for every permission family

| Layer | Required test behavior |
|---|---|
| Unit | Requirement/handler decision for permission present/absent, role-only denial, policy, owner/scope, state/version, reauth freshness, offline eligibility, cache revision |
| Integration | Current permission snapshot plus owner-scoped repository/DB/object lookup; parent-child relation; invalidation; audit/outbox; no content loaded before authorization |
| Authorization | Execute all applicable scenario columns in §11.2, including cross-user, cross-profile, wrong role, revoked, expired, and direct-path cases |
| API | Call actual route without UI; validate 401/403/concealed 404/409/412 semantics, no existence/content leak, and authorization before count/storage/range/export |
| E2E | Perform allowed and denied flows through Web, Desktop, Mobile, Assistant, reconnect, and SignalR where applicable |
| Negative | Tamper principal/role/permission/owner/Feature/resource/revision/ETag/cursor/Job/connection/IPC fields; verify fail closed |
| Regression | Every corrected authorization defect becomes a permanent test at the lowest useful layer plus an API/E2E proof where boundary bypass was possible |

Technical green is insufficient for a permission-sensitive Feature until applicable rows and layers pass.

---

## 12. Negative security tests

| ID | Attack/negative case | Expected evidence |
|---|---|---|
| `NEG-01` | User A guesses User B secured-file/version/result/download ID | `403` or consistent concealed `404`; no metadata, count, range support, filename, size, ETag, storage ref, or timing-based disclosure |
| `NEG-02` | Administrator calls User A document/result/OCR/secured-content route | Denied without a separate explicitly approved content permission; current design has no implicit content access |
| `NEG-03` | Auditor sends PUT/PATCH/DELETE against audit records or bypasses UI | No supported mutation; route/method denied and DB identity cannot update/delete audit outside governed lifecycle |
| `NEG-04` | Support lists storage/jobs/files or accesses unrelated User A content | Denied; only permission-scoped issues, explicitly sent attachments, and safe diagnostics are accessible |
| `NEG-05` | Support tries to retrieve password, token, secret, clipboard, signature, document, or full OCR through issue/export/log | Fields do not exist in safe contract/evidence; deny content and verify absence across response/export/log/audit |
| `NEG-06` | User posts forged owner ID, role, permission, PrincipalId, support flag, or audit actor | Server ignores/rejects client value and derives current principal/context |
| `NEG-07` | User with screen access invokes a hidden action directly | Endpoint/application service denies exact missing action permission |
| `NEG-08` | Role claim says Administrator but backend permission snapshot omits write permission | Deny; role grouping cannot bypass explicit permission |
| `NEG-09` | Attempt self-assignment, privilege escalation, or removal of safety ceiling | Deny using separate permission, target/scope, SoD, reauth, state/version, and hard-bound checks; safe audit emitted |
| `NEG-10` | Stale permission cache after role/permission/session revocation | Current server action denies; client projection invalidates/reloads; stale positive cache is not authority |
| `NEG-11` | Offline grant is expired, rolled-back clock, copied to another account/device/profile, or tampered | Local action fails closed; no automatic renewal or profile merge |
| `NEG-12` | Account revoked while offline, then reconnects with pending server intents | Refresh learns revocation before dispatch; intents remain denied/waiting/cancelled per policy; audit evidence is not erased |
| `NEG-13` | Pending intent contains arbitrary URL, SQL, engine, class, shell, raw HTTP, password, signature, or file bytes | Local persistence/validator rejects; nothing is dispatched |
| `NEG-14` | Queued intent authorized yesterday is flushed before permission refresh | Test fails unless refresh and per-intent reauthorization occur first |
| `NEG-15` | SignalR client requests User B group/job, arbitrary role/group/correlation, or reuses stale connection after permission change | Subscription rejected/disconnected; no cross-user event; REST also denies |
| `NEG-16` | SignalR event triggers cancel/retry/upload/admin mutation | Impossible: Hub exposes no business mutation and client event handler cannot create authoritative side effect |
| `NEG-17` | Worker receives forged queue/IPC command with valid-looking role/authorization context | Reject unknown job/instance/handler/schema/ref/state/lease; queue context is not a capability token |
| `NEG-18` | Local Worker is contacted by another OS user, rogue same-user process, stale instance, remote pipe, or wrong job | ACL/identity/handshake/instance/job/version checks reject; source and profile remain isolated |
| `NEG-19` | Server Worker is sent end-user token or tries unrelated DB/object/provider/path | Token is absent; least-privileged workload identity and allowlisted references deny access |
| `NEG-20` | Assistant requests an unauthorized Feature or User B resource | Same deny/concealment as UI/API; other-user data never enters model context |
| `NEG-21` | Assistant attempts an admin/support/audit/sensitive action using prompt injection or inferred confirmation | Hidden/not-exposed Feature denied; otherwise exact permission, scope, confirmation, and reauth required; AI inference never satisfies them |
| `NEG-22` | Assistant builds a multi-step plan where one later step lacks permission | Per-step validation stops before execution/transfer; no partial unauthorized step or claimed success |
| `NEG-23` | Client changes canonical Feature ID/body/schema or asks arbitrary provider/engine | Closed registry/schema/handler rejects; no dynamic dispatch |
| `NEG-24` | Cursor/filter/count/range/export is issued for unauthorized scope | Authorization/ownership is applied before count/query/range/export; no differential leak |
| `NEG-25` | Child attachment/version/result ID belongs to a different parent | Parent and child relationship check fails before content stream |
| `NEG-26` | Permission changes between read and write (TOCTOU), or stale `If-Match` is used | Mutation rechecks current permission, state, and version near transaction; deny or `409/412` without partial effect |
| `NEG-27` | Delete/tombstone followed by stale link, cache, retry, SignalR event, range, or restore | Current permission/deletion state denies; deleted object is not resurrected or redisclosed |
| `NEG-28` | User tries to disable, edit, or delete mandatory Enterprise Audit through history/conversation/local cleanup | APIs and storage ownership keep Audit separate; personal deletion has no cascade |
| `NEG-29` | Forged/off-sequence offline audit batch with another actor/profile or duplicate idempotency key | Authenticated client policy, actor/profile binding, schema/version, sequence/integrity/idempotency reject or safely deduplicate |
| `NEG-30` | API error/log/telemetry/SignalR/Assistant reveals denied resource, claims, role map, document, password, token, or secret | Safe error code/Correlation ID only; prohibited values absent in automated leakage assertions |

Mandatory example outcomes from the prompt are therefore explicit:

- User A cannot read User B's secured file.
- Administrator does not automatically read User A's document content.
- Auditor cannot modify/delete audit records.
- Support cannot see password, token, unrelated document content, signature, clipboard, or full OCR.
- Assistant cannot bypass permission, ownership, role mapping, confirmation, reauthentication, offline, or platform exposure.

---

## 13. Performance considerations

### 13.1 Performance principles

Authorization must be correct before it is fast. Optimization may cache and batch safe inputs, but cannot remove final server checks or expand authority.

| Risk | Required design |
|---|---|
| DB query per UI button | One filtered Feature/action availability projection per page/context; UI evaluates locally for UX |
| Permission lookup in every handler | Resolve revision-aware server permission snapshot once per request/application operation and share it among requirements |
| N+1 ownership lookup in lists | Owner/explicit-scope predicate in the main query before count/filter/cursor; narrow DTO projection |
| Load full object/content before authorization | Load only authorization metadata first or query by ID + owner/scope; resolve/stream content afterward |
| Workflow repeats role lookup per step | Reuse current permission snapshot, but evaluate each Feature/resource/policy/state separately |
| Long-lived stale positive cache | Bounded TTL plus revision/invalidation; current session/revision and sensitive freshness still checked |
| Permission-change storm | Coalesce invalidation by principal/revision, use bounded fan-out, and make clients refresh once; correctness cannot depend solely on SignalR |
| Reconnect storm | Single-flight identity/permission refresh and bounded exponential retry/jitter; do not flush queued commands concurrently before reauthorization |
| SignalR group/subscription growth | Server-derived user group, bounded per-job subscriptions/connections, cleanup on disconnect/revocation, slow-consumer limits |
| Authorization log cardinality | Stable action/permission/reason/revision codes and Correlation ID; no raw claims, resource content, or unbounded path/name |

### 13.2 Query and cache rules

- Apply authorization scope before count; an exact total for unauthorized data is a disclosure and a performance cost.
- Prefer indexed owner + state/time + internal-key query shapes defined in Document 07.
- Use a single short-lived DbContext/connection per application operation; no connection during queue wait, file stream, engine/provider work, SignalR connection, or retry delay.
- Do not load all role memberships, all permissions, or a large entity graph when only a compact effective set/revision and owner metadata are needed.
- Permission snapshot size, TTL, invalidation latency, authorization-query latency, cache hit ratio, DB calls per request, and denied-request cost are measured without recording restricted content.
- Denied paths must have bounded cost and rate protection; an attacker cannot force expensive content/object lookup before authorization.

### 13.3 Performance/security tests

Test at minimum:

- feature-discovery/page availability with realistic permission count and no per-button calls;
- lists with owner scope, filters, cursor, count omission, and cross-user denial under load;
- permission revision/invalidation during concurrent reads/mutations;
- logout/revocation while Web/API/Hub/Worker activity is in flight;
- reconnect of many Desktop/Mobile clients with queued intents and audit outboxes;
- SignalR group/subscription churn, unauthorized subscription flood, slow consumers, and revocation cleanup;
- Assistant multi-step authorization without duplicate database lookups or skipped per-step checks;
- cache cold start, outage, stale entry, corrupted entry, and invalidation-service failure—failure must remain secure.

Numeric budgets and acceptance thresholds belong to Document 12 after benchmark. They must not be invented in Document 09.

---

## 14. Implementation-readiness notes

### 14.1 Recommended .NET shape

The following are conceptual component responsibilities, not production code or final type names:

| Concept | Responsibility |
|---|---|
| Shared Permission Catalog | One canonical set of approved permission constants generated/validated from Document 09; no string literals scattered through UI/services |
| Authorization Policy Registry | Maps action/Feature category to reusable ASP.NET Core policy requirements; rejects unknown permission/policy |
| Permission Snapshot Provider | Resolves current backend effective permissions and revision once per operation; provides protected offline projection separately |
| Resource Authorization Loader | Loads minimum owner/parent/scope/state/version/classification/tombstone metadata without content |
| Requirement/Handler set | Authentication/account/session, permission, owner/explicit scope, policy/classification, state/version, capability, consent, confirmation, reauth |
| Application Authorization Service | Orchestrates the formula and returns typed `ALLOW`/deny/reauth/online/update decision with safe reason code |
| Client Action Availability Service | Converts server/local projection into UX availability for Web/Desktop/Mobile without becoming server authority |
| Offline Grant Evaluator | Validates integrity/profile/version/absolute expiry/trusted time and allows only listed local capability |
| Reauthentication Context | Short-lived, action/resource/session/client/policy-scoped freshness; invalidated by logout/revocation/permission change |
| Worker Contract Validator | Authenticates instance/workload, validates closed contract/job/ref/budget/allowlist; never resolves human roles |

### 14.2 ASP.NET Core authorization pattern

- Set an authenticated fallback policy for protected API/MVC/Hub surfaces.
- Use endpoint/page policies for coarse permission entry.
- Use resource-based authorization requirements/handlers for ownership, explicit support/audit scope, parent-child, state/version, classification, and reauthentication.
- Keep final call in the owning application service so scheduled jobs, Assistant, workflows, and non-HTTP entry points cannot bypass the rule.
- Construct the principal and permission snapshot server-side; reject client mass assignment.
- Return existing safe Document 06 errors, not handler internals or role maps.
- Register policies from a validated catalog at startup; unknown policy/permission fails startup or fails closed according to the approved deployment design.

### 14.3 Permission naming guidance

Document 09 should use action-oriented, resource-specific constants rather than role names, for example conceptual families:

```text
Feature.Execute
Job.ReadOwn / Job.ControlOwn / Result.ReadOwn
SecuredFile.ReadOwn / Version.WriteOwn / DeletePermanentOwn
Issue.SubmitOwn / Issue.ReadOwn
Support.IssueList / IssueDetail / AttachmentRead / Triage / Note / Export
Audit.Query / Audit.Export
Admin.PolicyRead / PolicyWrite / FeatureRead / FeatureWrite / LimitRead / LimitWrite
Operations.HealthRead / JobMetadataRead
```

These are naming examples only. Integrator must map or rename them to the approved canonical Permission Catalog and must not treat them as permission decisions already locked by this report.

### 14.4 Simple application flow

1. UI asks the Action Availability Service once for a page/context and renders hide/disable/explanation.
2. On invocation, the shared client application service creates a typed request with canonical Feature/action and resource reference.
3. Local operation uses the Offline/Online Authorization Service; a heavy local action receives an authorized internal operation object, then launches the bounded Worker contract.
4. Server operation calls the API; authentication constructs current principal/session and permission snapshot.
5. Application service loads minimum resource metadata, evaluates policy/owner/state/reauth, and executes only on `ALLOW`.
6. Worker receives only persisted job locator/closed contract; output is validated and persisted.
7. Every later read/control/continuation/result/subscription is reauthorized.

### 14.5 Test-project readiness

Recommended logical test suites:

```text
Authorization.UnitTests
Authorization.IntegrationTests
Api.AuthorizationContractTests
OfflineGrant.SecurityTests
SignalR.AuthorizationTests
WorkerContract.SecurityTests
Assistant.AuthorizationParityTests
Authorization.E2ETests
Authorization.RegressionTests
```

Use data-driven cases from §11.2 so every permission family receives the same base scenarios without hand-copying role checks. Each test records permission family, principal fixture, resource owner/scope, session/grant/revision, context, expected decision/status, and absence of forbidden data.

### 14.6 Coding readiness gates

Before production coding at scale:

- Document 09 approves canonical permission codes and baseline role mappings;
- policy/handler catalog and deny-by-default fallback are reviewed;
- reauthentication triggers and separation of duty are approved;
- offline grant fields, expiry, integrity, revision, and invalidation contract are approved;
- missing administration/session-revocation API contracts are resolved without ad-hoc endpoints;
- shared test fixtures and matrix harness exist;
- cache/invalidation design has failure-mode and performance tests;
- no IAM vendor or new security architecture is selected by implementation convenience.

---

## 15. Risks/conflicts

### 15.1 Cross-document findings

| ID | Finding | Risk | Required Integrator action |
|---|---|---|---|
| `RC-01` | Baseline roles are User, Administrator, Auditor; “Superadmin” appears as a UI/support label | Accidental fourth all-powerful role or bypass | Model Superadmin as a granular Support permission bundle only; do not add a role |
| `RC-02` | Document 03 has `FEAT-ADMIN-001..003` User/Role/Permission Management and `FEAT-AUTH-007` Session Revocation, but Document 06 inventory has no corresponding management/revocation routes | Teams may invent ad-hoc endpoints or put authorization only in UI | Report to API Integrator; approve a Document 06 addendum/contract before implementation; no endpoint invented here |
| `RC-03` | Document 03 lists `FEAT-AUDIT-008 Audit Retention` under `AUDITOR/ADMIN`, while Documents 01/08 say Auditor cannot change configuration/delete audit and audit mutation is separately governed | Auditor could be mapped to retention mutation by UI label | Split view/verification from retention administration in the Permission Matrix; Auditor gets no mutation by default; preserve Feature ID and report ambiguity |
| `RC-04` | `FEAT-AUDIT-005..007` are catalogued on Auditor/Admin surfaces, but mandatory offline audit emission/sync is a system/app behavior for all user activity | Ordinary User audit might be skipped, or Auditor role incorrectly required to emit audit | Treat production/outbox/sync as system capability under authenticated client policy; keep audit query/export permissions separate |
| `RC-05` | API allows Job owner or explicit support scope, while Document 08 denies Support unrelated job/result/secured-file browsing | Broad support permission could leak content/results | Limit support job access to explicit safe metadata and issue-linked scope only; no result/content permission implied |
| `RC-06` | Document 08 requires permission/policy version and invalidation, but Document 06 does not lock an exact revision field/refresh endpoint | Cache can become stale or each client invents a contract | Integrate with existing profile/effective configuration/Feature contracts or approve one compatible addendum; do not create parallel APIs |
| `RC-07` | Exact SSO claims, stable subject, provisioning, token/session lifetime, and revocation delivery remain unconfirmed | Wrong claim becomes identity/role authority | Keep backend mapping deny-by-default and mark `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `RC-08` | Offline expiry and revoked-account behavior necessarily include a disconnected detection gap | Revoked user may retain bounded local ability until grant expiry | Keep local-only minimum permission and absolute expiry; revalidate before any server command; approve residual risk/expiry |
| `RC-09` | Documents do not define browser/PWA offline authorization | Cached Web UI could be mistaken for offline grant | Deny/infer no Web offline authority until approved architecture and protected grant exist |
| `RC-10` | Exact permission codes, delegation, separation of duty, self-assignment, role mapping, and reauth trigger/age are intentionally deferred to Document 09 | Implementation may hard-code roles or inconsistent checks | Lock these in the master Role Matrix before coding; generate/validate shared constants from it |
| `RC-11` | Assistant can invoke visible canonical Features, but admin/support/audit Features are not automatically Assistant-exposed | Prompt could become a hidden privileged UI | Deny by default; any future exposure must be explicit per Feature/action and use identical permission/reauth/confirmation |
| `RC-12` | Client-side caching is needed for performance but server remains authority | Stale client projection can look enabled after revocation | Treat cache as UX only; revision-aware server final check and reconnect refresh are mandatory |
| `RC-13` | Worker must not decide business permission, but Document 08 requires policy recheck at safe boundaries | Duplicate/contradictory authorization logic might enter Worker | Application owns allow/cancel/quarantine state; Worker validates persisted execution eligibility and never maps role/permission itself |
| `RC-14` | Support attachment is explicitly sent issue content, while User document remains unrelated restricted content | “Attachment access” could become general file access | Authorize issue parent + exact attachment relation and dedicated storage zone; no arbitrary `fileRef`/storage browsing |
| `RC-15` | Role Matrix could optimize for screens rather than actions | One broad screen permission grants triage, note, attachment, export, and content together | Preserve Document 08 dimensions: separate list/detail/attachment/triage/note/export and read/write permissions |

### 15.2 Required confirmations

The following remain `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`:

- canonical permission identifiers and baseline Role → Permission mappings;
- delegation, scope hierarchy, self-assignment prevention, separation of duty, emergency/break-glass rule;
- reauthentication action matrix, assurance method, maximum age, retry/lock behavior;
- approved identity claims, provisioning, session/offline lifetime, revocation propagation and disabled-account process;
- permission/policy revision and invalidation contract, cache TTL and freshness class;
- content-access exception policy—current baseline has no implicit or break-glass document access;
- exact support, audit, operations, administration, and export scopes;
- authorization test data ownership and final acceptance authority.

No unresolved confirmation permits a fail-open default, role-only authorization, public AI, direct storage/SQL, or UI-only control.

---

## 16. Suggested Document 09 text

The following text is ready for Integrator adaptation into Document 09.

### 16.1 Authorization model

PERTAMINA OTO uses permission-based, deny-by-default, object-aware authorization. Baseline roles are User, Administrator, and Auditor. A role groups permissions but does not make the final decision. Superadmin is a granular Support permission bundle/surface and is not a new all-powerful role. Final authorization evaluates authenticated identity, active account, valid session, action or canonical Feature permission, resource ownership or explicit scope, enterprise policy/classification, resource state/version, client compatibility/capability, required consent/confirmation, and fresh reauthentication where required.

UI visibility or enablement is UX only. Server API/application services perform final authorization for server actions. Local Desktop/Mobile actions use the shared application/security service; role checks must not be distributed as `if(role)` through UI code.

### 16.2 Channel enforcement

Web uses the protected server session, authenticated fallback policies, anti-forgery for cookie mutations, and application-service resource authorization. Desktop and Mobile use approved native authentication/secure credential storage, protected per-account local profiles, and the same canonical local/server application path. Device capability influences availability but never grants business permission. A direct URL, handcrafted API call, guessed ID, cache, route, menu, file reference, Job ID, Correlation ID, or storage key never proves access.

### 16.3 API enforcement

The API derives principal, permissions, owner, and scope server-side. Endpoint policies provide coarse entry control; resource-based handlers and the owning application service authorize ownership/scope, parent-child relation, policy/classification, state/version, consent, capability, and reauthentication. Authorization is applied before count, filter, cursor, range, storage resolution, export, and content stream. Every continuation, retry, resume, cancel, restore, permanent deletion, export, result retrieval, download range, and Hub subscription is reauthorized. Unauthorized access returns safe 401/403 or consistent concealed 404 without content/existence leakage.

### 16.4 SignalR

`/hubs/activity` is authenticated notification-only. The server derives user groups, authorizes optional per-job subscriptions by current ownership/permission, bounds connections/subscriptions, and sends safe metadata only after commit. The Hub has no business mutation. Logout, expiry, revocation, disablement, or relevant permission change terminates/revalidates the connection. Reconnect refreshes identity/session/permission, reconciles REST authority, and reauthorizes every subscription. Cross-user activity/job/issue leakage is prohibited.

### 16.5 Worker

Human business authorization is completed by the shared/API application service before execution is accepted. Queue/IPC context is not a capability token, and no end-user token is forwarded. Worker authenticates its workload/instance, reloads the authoritative job, validates canonical whitelisted handler/schema/reference/state/version/lease/deadline/budget, enforces resource boundaries, rejects arbitrary operation/path/URL/provider/SQL/shell, and validates output. When policy revocation is learned, the application-owned job state directs safe cancel/quarantine; Worker does not map roles or independently grant permission.

### 16.6 Assistant

Assistant has no separate elevation. It may propose only a visible canonical Feature ID and typed parameters/workflow. Every step passes the same input ownership, permission, policy/classification, consent, capability, confirmation, reauthentication, application service, and result-grounding checks as ordinary UI/API. Unauthorized, cross-user, sensitive, admin, support, audit, and offline attempts receive the same deny/online/reauth outcome. Admin/Support/Audit Feature exposure to Assistant is denied unless explicitly approved in the Feature and Permission Matrix.

### 16.7 Offline authorization

Offline authorization stores no token, cookie, password, service credential, unrestricted claim dump, document content, or privileged server permission. A protected minimum grant contains the internal profile binding, offline-eligible permission/capability projection, policy/classification/contract revisions and ceilings, issued/last-online/absolute-expiry, integrity/trusted-time evidence, device/profile binding where supported, and last learned revoke marker. Expiry is absolute and never slides offline; uncertain time fails closed.

Only eligible deterministic local Features and protected local draft/history/recovery are allowed. Upload, server processing, secured-account operations, cross-device access, issue/server status, admin/support/audit server surfaces, SignalR, and reauthentication-required actions wait or deny until online. On reconnect, the client refreshes identity/account/session/permission/policy/capability/compatibility before reauthorizing each pending intent. A queued intent is not a prior authorization grant. Safe offline audit metadata sync is separate and cannot contain document content or secrets.

### 16.8 Permission caching and performance

Clients receive one safe versioned availability projection for UX instead of making a permission call per button. Servers resolve an effective revision-aware permission snapshot once per request/application operation, reuse it across requirements, and apply owner/scope in indexed queries before count/projection. Content is not loaded before authorization. Cache entries are scoped by principal/session/environment/revision, bounded by approved TTL, invalidated by account/session/role/permission/policy changes, and never replace the final server check. Sensitive mutations require current revision and scoped reauthentication.

### 16.9 Test requirement

Every important permission family has ALLOW, DENY, WRONG OWNER, EXPIRED SESSION, REVOKED PERMISSION, OFFLINE, ONLINE, REAUTH REQUIRED, WRONG ROLE, DIRECT API ATTEMPT, and ASSISTANT ATTEMPT tests as applicable. Minimum layers are Unit, Integration, Authorization, API, E2E, Negative, and Regression. Mandatory proofs include User A denied User B secured file; Administrator denied automatic User A document access; Auditor denied audit mutation/deletion; Support denied password/token/unrelated document content; SignalR denied cross-user subscription; Worker denied forged/unscoped contract; and Assistant authorization parity.

### 16.10 Integration gaps

Exact permission codes/mappings, delegation, separation of duty, self-assignment, reauthentication triggers, identity claims/lifetimes/revocation, offline expiry, permission revision/invalidation, cache TTL, content exception, and privileged scopes are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`. Document 03 contains User/Role/Permission Management and Session Revocation Features, but Document 06 does not define their API routes; implementation requires an approved API addendum and must not invent ad-hoc endpoints. Audit retention and offline-audit catalog labels must be mapped so Auditor cannot mutate audit/configuration and mandatory audit generation is not restricted to Auditor/Admin roles.

---

**Integrator note:** Preserve canonical Feature IDs and the Documents 01–08 security architecture. Resolve the reported Role/Permission and API contract gaps before coding; do not add roles, IAM vendors, parallel authorization services, or UI-only enforcement.

