# AGENT A — ROLE & PERMISSION MODEL REPORT

> **Task ID:** DOC09-AGENT-A  
> **Role:** Senior System Analyst / IAM & Authorization Architect / Senior ASP.NET Core Programmer  
> **Status:** Specialist recommendation for integration into Document 09 — **not the final Role Matrix**  
> **Date:** 26 Agustus 2026  
> **Authoritative baseline:** Documents 01–08 FINAL and `MASTER_PROJECT_SUMMARY.md` v3.1  
> **Formal role baseline:** `User`, `Administrator`, `Auditor`  
> **Constraint:** No new formal role, no IAM vendor selection, no production code, and no modification to the Document 09 master

Terminology:

- **LOCKED** — inherited from Documents 01–08 and not reopened here.
- **RECOMMENDED** — proposed model for integration and approval in Document 09.
- **CANDIDATE DEFAULT** — permission category normally needed to fulfil the role's stated business purpose; still subject to final matrix approval.
- **CANDIDATE OPTIONAL** — separately assignable capability for a narrower duty, risk, or scope; not implied merely by the role name.
- **PROHIBITED BY BASELINE** — access that conflicts with Documents 01–08.
- Permission codes in this report illustrate the proposed naming grammar. They are not an approved final catalog until the Document 09 matrix is integrated and reviewed.

---

# 1. Recommended authorization model

## 1.1 Five-dimension model

Use an authorization model with five independent dimensions:

```text
ROLE
  groups common employee/job-function permissions

PERMISSION
  answers: may this subject attempt this action type?

POLICY
  answers: is the action allowed under enterprise/security/classification rules?

RESOURCE
  answers: which concrete object and relationship are involved?

CONTEXT
  answers: under what session, connectivity, platform, state, and risk conditions?
```

The final decision is:

```text
ALLOW = Authenticated
    AND AccountActive
    AND SessionValid
    AND PermissionGranted
    AND SubjectTypeAllowed
    AND ResourceRelationAllowed
    AND ResourceStateAllowed
    AND EnterprisePolicyAllows
    AND ClassificationAllows
    AND ClientAndCapabilityCompatible
    AND ConnectivityContextAllows
    AND ConsentPresentWhenRequired
    AND ReauthenticationFreshWhenRequired
```

A valid role or permission is necessary but never sufficient by itself. Any missing, stale, unknown, or denied term produces a deny result.

## 1.2 Recommended characteristics

1. **No role-name authorization spread through the application.** Do not implement repeated checks such as `if role == "Admin"`. Endpoint and application-service authorization should refer to stable permission policies and resource requirements.
2. **No implicit role hierarchy.** `Administrator` and `Auditor` do not automatically inherit `User`. A principal who must perform both employee work and administration/audit receives the applicable roles explicitly.
3. **Roles are coarse assignment convenience; permissions are atomic capability grants.** Role membership does not bypass policy, ownership, resource state, reauthentication, or online-only requirements.
4. **Server/application services make the final decision.** UI visibility and locally cached permissions are user-experience projections only.
5. **Administrative, Support, Audit, monitoring, and content authority are separate.** A role that manages configuration or observes safe metadata never implies document-content access.
6. **Support/Superadmin is permission-based.** It is not added as a formal role and never means “all permissions.”
7. **Feature execution and resource APIs use the same authorization model.** UI, File-First, Assistant, workflow, retry, export, download, and SignalR cannot create alternate privilege paths.
8. **Offline authorization is contextual.** Do not create a second offline role or duplicate `.offline` permission set. A protected cached grant can exercise only permissions whose Feature and policy are offline-compatible.
9. **Human and workload identities are disjoint.** A Worker/service identity is not a `User`, `Administrator`, or `Auditor` and cannot receive human interactive permissions.
10. **Permission definitions are governed and stable.** Administrators may assign approved permissions when authorized; they do not invent codes or change permission meaning at runtime.

## 1.3 Effective grant calculation

Recommended conceptual calculation:

```text
Active role grants
UNION approved active principal-specific grants, if that mechanism is selected
UNION approved external-group mappings resolved by the backend, if selected
        ↓
filter by subject type, assignment scope, validity, and version
        ↓
apply policy denials/restrictions
        ↓
apply concrete resource relationship and state
        ↓
apply current context and reauthentication
        ↓
ALLOW or DENY
```

Only one authoritative mapping strategy should be selected for each grant source. Raw SSO group/role claims must not be treated as application permissions without a versioned backend mapping.

## 1.4 Enforcement locations

| Layer | Responsibility |
|---|---|
| Pertamina Identity | Authenticate the subject and issue approved claims/assertion |
| API/Web/SignalR authentication | Validate identity, account, and session |
| ASP.NET Core permission policy | Verify the action permission and allowed subject type |
| Resource authorization handler | Verify owner/reporter/case/audit/admin/workload scope and resource state |
| Policy service | Apply classification, retention, feature exposure, limit, consent, client, and safety rules |
| Application service | Recheck authorization before content access, mutation, dispatch, export, or durable job admission |
| Local application | Enforce valid cached offline projection for eligible local-only work only |
| Worker/service | Enforce separate workload identity and job-scoped operation |
| UI | Display effective capability only; never make the final decision |

## 1.5 Source alignment

| Source | Preserved outcome |
|---|---|
| Document 01 | User/Administrator/Auditor business boundaries; permission-based additional capability; Administrator no automatic document access |
| Document 02 | `FR-ROLE-001..003`; backend-controlled role/permission; offline cached permission only while valid |
| Document 03 | 550 canonical Features; authentication, administration, audit, support, and monitoring capabilities |
| Document 04 | Enterprise policy, Support Console, Assistant/application-service boundary |
| Document 05 | ASP.NET Core/.NET implementation baseline without hardcoded role logic |
| Document 06 | Endpoint-specific permission, owner/scope, policy, reauthentication, and SignalR checks |
| Document 07 | `admin.Roles`, `admin.Permissions`, `admin.RolePermissions`, `admin.PrincipalRoles`; permission definitions migration/version owned |
| Document 08 | Deny-by-default five-gate security formula, distinct content permission, narrow offline grant, no implicit Superadmin role |

---

# 2. Role definitions

## 2.1 Role semantics

A role is a stable grouping of commonly required permissions for a business persona. It is not:

- an authorization result;
- a resource scope;
- an ownership relationship;
- a data-classification clearance by itself;
- a reauthentication result;
- a Feature flag;
- an online/offline state;
- a substitute for a workload identity;
- a wildcard grant.

Formal roles remain exactly:

```text
User
Administrator
Auditor
```

Role codes should be stable and versioned. Display names may be localized, but authorization never depends on localized text.

## 2.2 User

### Definition and business purpose

`User` represents the baseline Pertamina employee capability to use approved PERTAMINA OTO tools and manage the employee's own work/resources under current policy.

### Candidate default capability groups

- read the user's safe account profile;
- read/update the user's allowed preferences;
- discover and execute approved employee Features through canonical Feature permissions;
- access and control the user's own eligible jobs and results;
- access/delete/reprocess the user's own Personal History under current policy;
- create/manage the user's own account-secured files and versions when that product capability is enabled;
- submit an Issue, explicitly attach an approved screenshot/attachment, and view the user's own Issue status;
- receive the user's own safe Activity/SignalR notifications;
- use eligible local Features while the protected offline authorization remains valid.

### Candidate optional capability groups

- high-risk or separately licensed/approved Feature execution;
- Content Intelligence or approved internal-provider Features;
- server execution where classification, consent, or organizational policy restricts availability;
- permanent secured-file purge where retention/legal policy permits;
- export/share/copy/print/download actions restricted by classification or organizational policy;
- future explicit delegated/team resource access if a governed business requirement is approved.

### Boundaries

- owner/reporter scope by default;
- cannot manage another principal, role, permission, enterprise configuration, or mandatory audit;
- cannot query another user's audit evidence, Support case, job, result, History, or secured file;
- cannot convert knowledge of an ID, path, `fileRef`, Job ID, Correlation ID, or SignalR event into access;
- cannot use an offline grant for server, admin, support, or audit authority.

## 2.3 Administrator

### Definition and business purpose

`Administrator` represents authorized administration of PERTAMINA OTO accounts, sessions, configuration, policy, feature exposure, limits, compatibility, and safe operational health metadata.

### Candidate default capability groups

- list/read approved safe user/account metadata;
- read role and permission definitions/assignments within authorized administrative scope;
- read policy, feature exposure, feature-flag, limit, compatibility, and security-profile metadata;
- read safe health, job, queue, sync, error, server, storage, version, and availability metadata;
- view administrative change status and concurrency/version information.

### Candidate optional capability groups

- enable/disable eligible accounts;
- revoke application sessions;
- assign/revoke formal roles within delegation scope;
- assign/revoke approved permissions if a governed principal-grant mechanism is selected;
- update typed policy, feature exposure/flag, limit, maintenance mode, compatibility/minimum version, retention, classification, or security profile within hard safety bounds;
- execute restricted administrative exports or operational actions explicitly defined by policy;
- receive audit read/export only through a separate audit permission or Auditor role—not by Administrator implication.

### Boundaries

- does not automatically inherit `User` or `Auditor`;
- does not open document content, job input/result, secured-file bytes, full OCR, PDF password, clipboard, signature asset, Issue attachment, prompt/chat, token, or secret;
- cannot change the meaning of a canonical Feature or permission code;
- cannot disable mandatory audit or weaken hard safety ceilings;
- cannot self-elevate, grant itself content access, or exceed its delegation scope;
- cannot directly write across module-owned data or impersonate a user;
- monitoring permission is safe-metadata permission, not resource-content permission.

## 2.4 Auditor

### Definition and business purpose

`Auditor` represents controlled read-only access to approved Enterprise Audit evidence for compliance, investigation, and reporting within an explicitly assigned audit scope.

### Candidate default capability groups

- list, search, filter, and read approved safe audit-event metadata;
- view recorded role, permission, configuration, authentication, processing, and security events through the Audit plane;
- access only the principal/unit/time/Feature/status/platform scope approved for the assignment.

### Candidate optional capability groups

- create/download bounded audit exports;
- broader unit/time/event-category scope where separately approved;
- access to additional safe evidence fields based on classification/compliance policy.

### Boundaries

- no configuration, role, permission, user, Issue, or job mutation;
- no Audit delete or retention-policy write;
- no automatic user-document, job-result, secured-file, Issue attachment, full OCR, password, clipboard, signature, token, or secret access;
- Audit export is separate from Audit read because it increases disclosure and bulk-extraction risk;
- `FEAT-AUDIT-005..007` offline queue/sync/retry are application/system functions, not Auditor write permissions.

## 2.5 Support/Superadmin

“Superadmin” is the user-facing name for a Support operator with an approved set of Support permissions. It is **not** a formal role, role hierarchy level, or wildcard.

Recommended Support permission categories:

- Issue list;
- Issue detail/message/safe diagnostics;
- explicitly submitted attachment read;
- triage/category/priority/status/duplicate/not-bug/need-information transitions;
- internal note creation/read according to policy;
- bounded Issue PDF/Excel export.

Support permissions are assigned only to approved principals through a governed mechanism. None is implied by `User`, `Administrator`, or `Auditor` alone. The minimum operator set should normally exclude attachment and export until the duty needs them.

Support cannot access:

- original user document or local workspace;
- unrelated job input/result or secured-file payload;
- arbitrary storage/object browsing;
- another user's non-Issue content;
- password, token, secret, clipboard, signature, or full OCR;
- arbitrary impersonation or “view as user.”

## 2.6 Multiple roles

- A principal may hold more than one formal role if business duties require it.
- Grants are additive only at the permission layer; every permission remains constrained by resource, policy, and context.
- There is no implicit inheritance chain such as `Administrator > Auditor > User`.
- Combining roles must be checked against segregation-of-duties policy.
- Holding `Administrator` and `Auditor` together must not allow the same principal to make a sensitive change and independently approve/audit it where separation is required.
- Support permissions remain separately visible even when assigned to a principal who also holds a formal role.

## 2.7 Service identity is not a role

A Worker, API, queue consumer, storage adapter, export processor, or internal-provider adapter uses a non-human workload identity with a specific audience and purpose. It does not receive `User`, `Administrator`, `Auditor`, or Support assignments and has no offline or interactive session.

---

# 3. Permission model

## 3.1 Permission definition

A permission is a stable backend-defined code representing one action category on one logical resource or canonical Feature. It grants the right to **attempt** the action; authorization still requires resource relation, policy, context, and state.

Each permission definition should carry metadata:

| Field | Purpose |
|---|---|
| Permission Code | Stable machine identifier |
| Display Name/Description | Human-readable intent, localized outside the code |
| Module | Application owner/boundary |
| Resource Type | Logical target |
| Action | Atomic operation category |
| Subject Type | Human, workload, application/system; never ambiguous |
| Canonical Feature ID | Optional mapping for user Feature execution |
| Sensitivity | Normal, elevated, sensitive, bulk-disclosure, destructive |
| Default Resource Scope | Own, Support case, Audit scope, admin metadata, job-scoped workload, or none |
| Online Requirement | Offline-compatible or online-only |
| Reauthentication Category | None or approved trigger category |
| Source Trace | Requirement, Feature, API route, security decision |
| Contract Version/Status | Active, deprecated, replacement, effective version |

Permission meaning is immutable. A materially different meaning requires a new permission code plus a controlled migration/deprecation path.

## 3.2 Permission types

| Type | Example concept | Assignment rule |
|---|---|---|
| Feature execution | Execute `FEAT-PDF-005` | Human role/principal only if Feature is user-invocable and policy allows |
| Personal resource | Read own job/result/history/secured file/Issue | Human permission plus owner/reporter relationship |
| Administration | Read/update policy, role, session, feature, limit | Administrator-duty permission; online; elevated actions often reauth |
| Support | List/read/triage/note/export Issue | Approved Support principal only; case scope; no original document implication |
| Audit | Read/export safe evidence | Auditor/approved principal; controlled audit scope; no write/delete |
| Monitoring | Read safe health/job/queue/error aggregate | Administrative/operations metadata only |
| Workload | Claim job, update job state, access job-scoped object | Workload identity only; never human token |
| System control | Audit ingestion, security enforcement, cleanup | Application/internal policy; not assignable through a human role |

## 3.3 Candidate User permissions

These are candidate categories, not the final approved matrix.

| Candidate status | Permission example | Default resource scope | Additional gate |
|---|---|---|---|
| Default | `account.profile.read` | Current principal | Active session |
| Default | `workspace.preference.read` | Current principal | Typed safe fields |
| Default | `workspace.preference.update` | Current principal | `If-Match`, policy |
| Default set | `feature.<canonical-feature-id>.execute` | User-selected owned/accessible input | Feature exposure, capability, classification, consent, connectivity |
| Default | `jobs.job.list` | Own | Authorized query scope |
| Default | `jobs.job.read` | Own | State/retention |
| Default | `jobs.job.cancel` | Own | Cancellable state |
| Default | `jobs.job.retry` | Own | Retry policy/idempotency |
| Default | `jobs.job.resume` | Own | Compatible checkpoint |
| Default | `jobs.result.read` | Own | Current authorization/retention |
| Default | `jobs.result.download` | Own | Classification/download policy |
| Default | `history.entry.list` / `history.entry.read` | Own | Personal History only |
| Default | `history.entry.delete` | Own | Does not affect Audit |
| Default | `history.entry.reprocess` | Own | Current Feature permission/policy |
| Default | `secured.file.create` / `secured.file.read` | Own | Quota/storage policy |
| Default | `secured.version.create` / `secured.version.read` / `secured.version.download` | Own | Version/classification policy |
| Default | `secured.file.delete` / `secured.file.restore` | Own | Retention/state/confirmation |
| Optional sensitive | `secured.file.purge` | Own | Online, retention/legal policy, reauthentication |
| Default | `issue.issue.create` / `issue.issue.list` / `issue.issue.read` | Reporter/own | Issue policy |
| Default | `issue.attachment.create` / `issue.attachment.read` | Reporter/own | Explicit send, validation, state |
| Default | `realtime.activity.connect` | Current principal | Valid online session |
| Default | `realtime.job.subscribe` | Own job | Reauthorize subscription |
| Default | `configuration.effective.read` | Filtered current principal | Safe effective projection only |
| Optional | Internal AI/high-risk/server Feature permission | Own/access-authorized input | Provider approval, classification, consent, capacity |

The final User Feature set should be generated from the approved user-invocable portion of the 550-Feature catalog. System controls, internal workflow steps, administrative, support, audit, and observability Features must not automatically become `User` execute permissions.

## 3.4 Candidate Administrator permissions

| Candidate status | Permission example | Scope | Additional gate |
|---|---|---|---|
| Default read | `admin.user.list` / `admin.user.read` | Approved account metadata scope | Safe metadata only |
| Default read | `admin.role.list` / `admin.role.read` | Authorization metadata | No mutation |
| Default read | `admin.permission.list` / `admin.permission.read` | Authorization metadata | Definitions immutable at runtime |
| Default read | `admin.session.read` | Approved session metadata | No raw token/cookie |
| Default read | `configuration.policy.read` | Approved policy scope | Safe typed metadata |
| Default read | `configuration.feature.read` | Feature exposure metadata | Cannot change Feature meaning |
| Default read | `configuration.limit.read` | Limit metadata | Hard ceilings concealed where necessary |
| Default read | `configuration.compatibility.read` | Compatibility metadata | — |
| Default read | `monitoring.health.read` / `monitoring.job.read` / `monitoring.queue.read` / `monitoring.error.read` / `monitoring.storage.read` | Safe operational metadata | No content/storage reference disclosure |
| Optional elevated | `admin.user.update` | Delegated account scope | Online, policy, concurrency, audit; reauth for disable/high impact |
| Optional elevated | `admin.session.revoke` | Delegated principal/session scope | Online, reauth, reason/evidence |
| Optional elevated | `admin.role.assign` / `admin.role.revoke` | Delegated role/principal scope | Online, no self-elevation, reauth, SoD |
| Optional elevated | `admin.permission.assign` / `admin.permission.revoke` | Delegated grant scope | Requires selected principal-grant model, reauth, SoD |
| Optional elevated | `configuration.policy.update` | Approved policy keys | `If-Match`, hard safety, reauth as classified |
| Optional elevated | `configuration.feature.update` | Exposure/flag only | `If-Match`, no semantic change |
| Optional elevated | `configuration.limit.update` | Approved limit keys | `If-Match`, within safety ceiling |
| Optional elevated | `configuration.compatibility.update` | Approved platform/channel scope | Reauth, rollout safety |
| Optional sensitive | `configuration.security-profile.update` / `configuration.classification.update` / `configuration.retention.update` | Approved policy scope | Reauth, SoD/approval, audit |
| Optional | Audit permissions | Explicit audit scope | Not implied by Administrator |

No Administrator permission above grants document, result, secured-file, OCR, Issue-attachment, password, clipboard, signature, token, or secret access.

## 3.5 Candidate Auditor permissions

| Candidate status | Permission example | Scope | Additional gate |
|---|---|---|---|
| Default | `audit.event.list` | Assigned audit scope | Query authorized before count/cursor |
| Default | `audit.event.read` | Assigned audit scope | Safe evidence fields only |
| Optional bulk | `audit.export.create` | Assigned audit scope | Online, bounded filter, reauth when policy requires |
| Optional bulk | `audit.export.read` | Requester/approved audit scope | Expiry and current permission |
| Prohibited | `audit.event.update` / `audit.event.delete` | None | No requirement; violates Auditor boundary |
| Not Auditor default | `audit.retention.update` | Compliance/admin policy scope only | Separate permission, approval, reauth |
| System only | `audit.batch.ingest` / `audit.batch.retry` | Authenticated app/client policy | Not a human Auditor grant |

Search and filter normally remain query capabilities under `audit.event.list/read`, not separate permissions for every UI control. Export remains separate because it creates a bulk artifact.

## 3.6 Candidate Support permissions

Support permissions are optional principal-specific duty grants; they are not default permissions of any formal role.

| Permission example | Purpose | Scope | Content boundary |
|---|---|---|---|
| `support.issue.list` | View permission-scoped Issue list | Assigned Support case queue/scope | Safe list projection only |
| `support.issue.read` | View Issue message/detail/safe diagnostics | Assigned Support case/scope | No original document/job result |
| `support.attachment.read` | View explicitly submitted attachment | Parent Issue + assigned scope | Only user-submitted attachment |
| `support.issue.triage` | Category, priority, status, duplicate/not-bug/need-info | Assigned Support case/scope | Typed state transition, `If-Match` |
| `support.note.create` | Add restricted internal note | Assigned Support case/scope | Separate note visibility/retention |
| `support.note.read` | Read internal notes | Assigned Support case/scope | Not implied by Issue list |
| `support.export.create` | Create bounded PDF/Excel export | Assigned filter/case scope | Online; current permission; expiry; reauth if policy requires |
| `support.export.read` | Retrieve generated Support export | Requester/approved Support scope | Current permission and artifact expiry |

The minimum Support operator should start with `list`, `read`, and only the triage actions needed for the duty. Attachment, internal-note, and export permissions should be separately justified.

## 3.7 Workload permissions

Illustrative workload-only permissions:

| Permission example | Subject restriction | Resource restriction |
|---|---|---|
| `jobs.worker.claim` | Approved Worker identity | Eligible queue/workload class and lease |
| `jobs.job.update` | Worker owning the active lease | Assigned Job and allowed state transition |
| `jobs.artifact.create` | Assigned Worker/export identity | Job-scoped validated artifact |
| `storage.object.read` | Approved job/storage adapter identity | One purpose/zone/object lease |
| `storage.object.write` | Approved job/storage adapter identity | One purpose/zone/object lease |
| `provider.operation.invoke` | Approved provider adapter | Approved provider profile/purpose/job only |
| `realtime.event.publish` | Approved outbox publisher | Committed safe event stream only |

These permissions are not stored in a human access token and cannot be assigned to a person through a formal role.

## 3.8 Feature-to-permission strategy

RECOMMENDED hybrid mapping:

1. User-invocable canonical tool execution receives a mechanically generated permission:

```text
feature.feat-pdf-005.execute
feature.feat-intel-001.execute
feature.feat-conv-036.execute
```

2. Resource/domain/admin/support/audit Features map to their resource permissions rather than a duplicate generic execute grant:

```text
FEAT-ADMIN-001   → admin.user.read / admin.user.update / ...
FEAT-AUDIT-002   → audit.event.list / audit.event.read
FEAT-SUPPORT-006 → support.issue.triage / support.note.create
```

3. System/internal security, audit-ingestion, monitoring-collection, and Worker Features map to system/workload policy, not human permissions.

4. Every mapping carries the canonical Feature ID for traceability, but authorization must not parse the permission string to discover the handler.

This avoids one overly broad `feature.execute` permission while also avoiding duplicate permissions for resource routes.

## 3.9 Grant types

Recommended grant model:

- **Role grant:** active `PrincipalRoles` plus `RolePermissions`; appropriate for stable formal persona defaults.
- **Scoped principal grant:** a directly assigned optional permission with scope and validity; appropriate for Support or narrowly delegated duties if approved.
- **External group mapping:** an approved Pertamina group maps through backend configuration to role/scoped permissions; raw claims are never final authorization.
- **System/workload grant:** separate subject type, identity, audience, purpose, and resource scope.

The source of a grant must be visible to administrators and tests. Multiple unsynchronized authorities must not silently compete.

## 3.10 Explicit deny strategy

Keep permission assignments allow-only at baseline. Deny by default means absence of a grant denies. Enterprise/classification/resource/context policies may further deny a granted action, and deny wins. Do not introduce arbitrary user-specific negative permissions unless a real exception requirement justifies the complexity and precedence rules.

---

# 4. Permission naming

## 4.1 Recommended convention

Choose:

```text
module.resource.action
```

Examples:

```text
account.profile.read
jobs.job.cancel
secured.version.download
support.issue.triage
audit.export.create
admin.session.revoke
configuration.policy.update
monitoring.health.read
feature.feat-pdf-005.execute
```

This convention is preferred over `resource.action` because resources such as `issue`, `file`, `export`, `session`, and `policy` can exist in multiple modules. The module prefix prevents collisions while remaining short enough for ASP.NET authorization policies and test names.

## 4.2 Grammar

Recommended normalized grammar:

```text
^[a-z][a-z0-9-]*\.[a-z][a-z0-9-]*\.[a-z][a-z0-9-]*$
```

Rules:

- lower-case ASCII machine code;
- exactly three segments for baseline permissions;
- singular logical resource name;
- verb/action is the final segment;
- use hyphen only inside a segment when unavoidable;
- no spaces, localized words, underscores, slash, wildcard, environment, tenant, unit, or resource ID;
- no role name in the permission code;
- no `own`, `all`, `any`, `superadmin`, `full-access`, or `bypass` suffix;
- ownership/scope and context belong to assignment/policy, not the code;
- canonical Feature execution preserves the lower-case canonical Feature ID as the resource segment;
- comparison and storage are case-normalized, but the catalog retains one canonical representation.

## 4.3 Module vocabulary

Recommended module vocabulary should align with the application/API ownership boundaries:

```text
account
workspace
feature
jobs
history
secured
issue
support
audit
admin
configuration
monitoring
realtime
storage       (workload only)
provider      (workload only)
```

Do not expose database schema names merely because they exist. The permission module should reflect the application security boundary.

## 4.4 Action vocabulary

Use a controlled action vocabulary and avoid ambiguous `manage` or `admin` actions.

| Action | Meaning |
|---|---|
| `list` | Query a scoped collection |
| `read` | Read one resource or safe detail |
| `create` | Create a resource/request/artifact |
| `update` | Modify allowed fields of an existing resource |
| `delete` | User-visible soft/recycle deletion where applicable |
| `purge` | Permanent destructive deletion |
| `execute` | Execute one canonical Feature |
| `cancel` | Request safe cancellation |
| `retry` | Create a new attempt for the same logical job |
| `resume` | Resume a verified compatible checkpoint |
| `download` | Obtain content bytes, distinct from metadata read |
| `restore` | Restore a version/recycled resource |
| `assign` | Add an approved role/permission assignment |
| `revoke` | Remove an assignment/session/grant |
| `triage` | Perform the typed Support triage state transition |
| `connect` | Establish an authorized realtime connection |
| `subscribe` | Subscribe to an authorized resource stream |
| `claim` | Workload-only lease/claim operation |
| `ingest` | System-only bounded evidence/event ingestion |
| `invoke` | Workload-only provider operation |

If an action cannot be described safely by the controlled vocabulary, add it through governed catalog review rather than using a broad `manage` permission.

## 4.5 Good and bad examples

| Good | Avoid | Reason |
|---|---|---|
| `support.attachment.read` | `superadmin.all` | No wildcard/power-title permission |
| `admin.session.revoke` | `admin.manage` | Atomic action is testable |
| `secured.version.download` | `file.read-any` | Content download and cross-user scope must be explicit |
| `audit.export.create` | `auditor.write` | Export is not general write |
| `feature.feat-pdf-005.execute` | `tools.execute` | Exact Feature authorization and traceability |
| `configuration.feature.update` | `feature.change-meaning` | Administrator changes exposure only, never Feature semantics |
| `jobs.result.download` | `jobs.full-access` | Metadata, control, and content are separate |

## 4.6 ASP.NET Core policy usage

Recommended conceptual policy naming:

```text
Permission policy : perm:jobs.job.read
Resource policy   : resource:jobs.job.read
Sensitive policy  : sensitive:admin.session.revoke
```

The endpoint may require the action permission, while the application service invokes a resource-aware authorization handler with the actual object. The exact framework registration belongs to implementation, but the model should support:

- one reusable permission requirement;
- resource-specific ownership/scope handlers;
- separate context/reauthentication requirements;
- generated negative tests for every permission/route/Feature mapping;
- no controller/application-service role-name branches.

---

# 5. Resource ownership model

## 5.1 Ownership is independent from permission

Permission answers whether a subject may attempt an action. Resource relationship answers whether the subject may attempt it on this concrete object.

```text
jobs.job.read + Owner(job)                  → candidate allow
jobs.job.read + NotOwner(job)               → deny
support.issue.read + AssignedSupportScope   → candidate allow
audit.event.read + AssignedAuditScope       → candidate allow
admin.user.read + AdminMetadataScope        → candidate allow
```

An object ID, `fileRef`, Job ID, Correlation ID, route nesting, cursor, ETag, storage reference, or SignalR group never establishes a relationship.

## 5.2 Resource relationship types

Recommended conceptual relationship types:

| Relationship | Applies to | Meaning |
|---|---|---|
| `OWN` | Job/result/history/secured file/preference | `OwnerPrincipalId` equals current principal |
| `REPORTER` | User Issue/attachment | `ReporterPrincipalId` equals current principal |
| `SUPPORT_SCOPE` | Issue/detail/attachment/note/export | Issue is within the operator's approved case/queue scope and permission |
| `AUDIT_SCOPE` | Audit event/export | Event/filter is within approved user/unit/time/category/platform scope |
| `ADMIN_METADATA_SCOPE` | User/session/config/monitoring metadata | Administrative delegation covers target; content remains excluded |
| `WORKLOAD_JOB_SCOPE` | Job/artifact/object/provider operation | Workload identity owns active lease/purpose for the assigned Job |
| `EXPLICIT_RESOURCE_GRANT` | Future delegated/team content only | Exists only after approved business/security design; no baseline wildcard |

Do not use an unbounded `ALL` relationship. Enterprise-wide audit or admin scope is still a typed, approved scope with data-field restrictions.

## 5.3 Resource access sequence

```text
authenticate subject
→ verify account/session
→ verify action permission
→ load minimum resource ownership/classification/state metadata
→ verify relationship/scope
→ apply policy/context/reauthentication
→ only then resolve storage/content/detail
→ execute or return safe response
```

For collections, apply the authorized scope before count, search, filter, sort, pagination, or cursor creation. For child resources, verify both the parent authorization and the child's membership in that parent.

## 5.4 Cross-user access

Cross-user access is denied by default. Current approved exceptions are limited to:

- Administrator reading safe account/configuration/operational metadata within delegation scope;
- Support reading Issue message/safe diagnostics and explicitly submitted attachment within Support scope;
- Auditor reading safe Audit evidence within Audit scope;
- approved Worker accessing a job-scoped object under workload identity.

None of these exceptions grants original user-document content. A future team share, delegated access, legal review, or emergency content-access workflow requires a separate approved Resource relationship, permission, policy, reauthentication, and Audit design.

## 5.5 Content-access distinction

Keep metadata and content permissions distinct:

```text
jobs.job.read            ≠ jobs.result.download
secured.file.read        ≠ secured.version.download
support.issue.read       ≠ support.attachment.read
audit.event.read         ≠ referenced document read
monitoring.job.read      ≠ job input/result read
admin.user.read          ≠ user resource read
```

Storage object resolution occurs only after the content-specific permission and resource relationship succeed.

## 5.6 Local resource ownership

Local workspace, draft, cache, history, checkpoint, signature, and app-managed artifacts are isolated by OS profile, PERTAMINA OTO profile, and approved subject binding. The Local Worker receives only job-scoped references through authenticated typed IPC. A cached permission cannot change the profile owner or cross to another local account.

---

# 6. Policy model

## 6.1 Policy purpose

Policy restricts when an otherwise granted permission may be exercised. It does not create a missing permission and cannot change a canonical Feature or resource meaning.

## 6.2 Policy layers

| Layer | Examples | Authority/behavior |
|---|---|---|
| Hard security invariant | No public AI, no direct SQL, no token in log, no Administrator implicit content access | Non-overridable by ordinary admin configuration |
| Enterprise security policy | Session/offline lifetime, classification handling, reauth, transfer/export rules | Typed/versioned, approved, deny wins |
| Feature/capability policy | Feature active/exposed, platform/capability/engine/provider availability | Cannot alter Feature semantics |
| Resource policy | Owner, state, retention, legal hold, version, quarantine, expiry | Evaluated on current resource |
| Organizational scope policy | Unit/case/audit/admin delegation | Explicit typed scope; no client-provided wildcard |
| Operational safety policy | File/job/query limits, quota, capacity, backpressure | Strictest safety value wins |
| User/device preference | Allowed preference only | Lowest priority; cannot weaken higher policy |

Recommended precedence:

```text
hard security/safety invariant
→ enterprise and classification policy
→ resource/retention/legal-hold state
→ organizational assignment scope
→ environment/platform/device capability
→ user preference
```

Any deny or unsatisfied mandatory requirement stops the action.

## 6.3 Policy inputs

Policy may use only approved, typed, server/application-derived inputs:

- validated principal and subject type;
- effective permission and assignment source/version;
- formal role for grouping/trace only;
- resource type, owner/reporter, classification, state, version, retention, legal hold;
- canonical Feature ID and contract/schema/capability versions;
- client platform/version, environment, connectivity, approved device context;
- online/offline state and cached-grant expiry;
- authentication/reauthentication time and approved assurance;
- consent/notice state;
- quota/capacity/risk category;
- workload identity, job lease, purpose, and provider profile.

Client-supplied role, permission, owner, policy result, classification override, reauthentication flag, or scope is never trusted.

## 6.4 Context model

Recommended context dimensions:

| Context | Example evaluation |
|---|---|
| Connectivity | Server/admin/support/audit/reauth actions require online; eligible local Feature may run offline |
| Session | Active, non-revoked, non-disabled, compatible permission version |
| Authentication freshness | Sensitive action requires approved recent authentication |
| Client | Platform and minimum supported version allow action |
| Capability | Local/server engine/provider/resource capability available |
| Classification | Transfer, content access, export, copy/print/download allowed |
| Resource state | Job cancellable, upload ready, Issue transition valid, file not purged/held |
| Time | Assignment/grant and offline authorization within absolute validity |
| Environment | Dev/QA/Production identity and data boundary |
| Consent | Explicit upload/provider/Issue attachment action present where required |

Do not encode these variables into separate role names such as `OfflineUser`, `AdminProd`, or `AuditorExport`. Keep role stable and let policy/context decide.

## 6.5 Deny behavior

- Unknown role, permission, scope, subject type, Feature, policy version, resource state, or context: deny.
- Permission granted but ownership absent: deny or consistently concealed `404`.
- Policy service unavailable for a server/sensitive action: safe `503`, never stale privileged allow.
- Feature unavailable locally but eligible for server: require online authorization/consent; do not assume permission from visibility.
- Hard safety policy cannot be overridden by Administrator.
- A client cache may hide or show UI, but only server/application policy returns the final decision.

---

# 7. Sensitive authorization

## 7.1 Sensitive-action rule

A sensitive action requires all normal authorization plus fresh approved reauthentication. Reauthentication proves freshness; it does not add permission or scope.

```text
current permission
+ current resource/scope
+ current policy/state
+ approved fresh authentication bound to action/resource/session
= sensitive action may proceed
```

## 7.2 Candidate trigger matrix

This is a recommendation for final matrix review, not a locked trigger list.

| Action category | Online-only | Reauth recommendation | Additional control |
|---|:---:|:---:|---|
| Assign/revoke Administrator or Auditor role | Yes | Required | Delegation scope, SoD, reason, concurrency, Audit |
| Assign/revoke Support or other elevated permission | Yes | Required | Selected principal-grant mechanism, scope/expiry, Audit |
| Change role-permission mapping | Yes | Required | Approved catalog only, SoD, versioned change |
| Disable/enable another principal | Yes | Required | Delegated target scope, safe reason, Audit |
| Revoke another user's session | Yes | Required | Delegated target/session scope, Audit |
| Change security profile/classification/retention policy | Yes | Required | Typed policy, `If-Match`, approval/hard bounds |
| Change feature exposure/limit/maintenance/minimum version | Yes | Risk-based; required when impact is broad/security-relevant | `If-Match`, rollout/safety validation, Audit |
| Permanent secured-file purge | Yes | Required when policy permits | Owner, retention/legal hold, confirmation, idempotency |
| High-impact Audit export | Yes | Policy-required | Bounded scope, expiry, current permission, Audit |
| High-impact Support export/attachment disclosure | Yes | Policy-required | Case scope, purpose, expiry, Audit |
| Exceptional non-owner content access if ever approved | Yes | Always required | Separate permission, explicit approval, purpose/time limit, evidence |
| Routine own-job status/read or ordinary local edit | Context-dependent | Not normally | Current permission/ownership/policy still required |

The final assurance method, maximum authentication age, one-time/bounded-use behavior, retry handling, and exact triggers require Pertamina confirmation and Document 09/10 integration.

## 7.3 Reauthentication context

The accepted context should be bound to:

- current `PrincipalId` and active session;
- permission/action category;
- target resource or assignment scope;
- client/device context where approved;
- policy/permission version;
- issue time and short expiry;
- single or bounded use.

Logout, session revocation, account disablement, material permission/policy change, or target mismatch invalidates it. Missing freshness produces `401 REAUTHENTICATION_REQUIRED` before mutation.

## 7.4 Confirmation is not reauthentication

A confirmation dialog protects user intent but does not prove identity freshness. Sensitive destructive actions may require both:

```text
explicit confirmation of intent
AND approved reauthentication freshness
```

Neither control substitutes for permission, ownership, or policy.

---

# 8. Offline authorization

## 8.1 Model

Offline authorization uses a protected, minimum, absolute-expiry projection of offline-eligible permissions. It is not a separate role, enterprise token, or server grant.

Recommended evaluation:

```text
valid protected offline grant
AND cached permission still within absolute validity
AND Feature marked offline-compatible
AND resource belongs to mounted local profile
AND classification/local policy allows
AND no online-only or reauthentication requirement
= local action allowed
```

## 8.2 Offline-compatible candidates

- eligible deterministic local user Features on user-selected local files;
- local draft, preview, undo/redo, local result, local Personal History, and recovery within profile policy;
- local Assistant orchestration only for the same eligible canonical Features;
- safe local preference/display behavior;
- creation of safe offline Audit metadata outbox by the application, not an Auditor write grant.

## 8.3 Online-only actions

- role/permission assignment, revocation, and synchronization authority;
- account enable/disable and session revocation;
- all Administrator, Support/Superadmin, and Auditor server surfaces;
- server Feature execution, upload, account-secured storage, cross-device resource, result download;
- Issue submission/status/Support operations;
- Audit query/export and configuration/monitoring detail;
- sensitive action requiring current server authorization or reauthentication;
- workload/service operation.

## 8.4 Permission-code rule

Do not create duplicate permission codes such as:

```text
feature.feat-pdf-005.execute-offline
feature.feat-pdf-005.execute-online
```

Use one stable permission, then evaluate Feature capability and connectivity context. This prevents drift between online and offline catalogs.

## 8.5 Stale permission/session behavior

- The cached grant has an absolute configurable expiry and no offline sliding renewal.
- It carries permission/policy/capability/contract versions and only offline-eligible capabilities.
- Clock rollback or uncertain time fails closed for protected actions.
- Role/permission removal while offline cannot be learned instantly; residual access is bounded by expiry and local-only scope.
- A learned account disable/session revoke invalidates the offline grant.
- On reconnect, refresh identity/account/session/permission/policy/capability/compatibility before sending queued commands.
- Every queued command is reauthorized; queued intent is not a previous grant.
- Removed permissions cause UI/local projection removal and deny; they do not silently dispatch before synchronization.

## 8.6 Disabled user

Online, an identity whose OTO principal is disabled/ineligible receives safe `403 ACCOUNT_ACCESS_DENIED` under Document 08 and cannot query, mutate, export, subscribe, or retrieve result content. When disablement is learned, invalidate the application session, permission cache, offline grant, and SignalR connection. Existing local source files are not silently deleted; app-managed profile recovery/retention follows approved policy.

---

# 9. Role assignment/revocation

## 9.1 Assignment principles

- Backend is authoritative for roles and permissions.
- The target subject must map to an approved active principal.
- The assigner needs an explicit assignment permission and delegation scope.
- The assigner cannot assign a role/permission broader than the assigner's delegated authority.
- Self-assignment/self-elevation is denied by default.
- Sensitive assignment requires online fresh reauthentication.
- Segregation-of-duties policy runs before the change.
- Assignment records actor, target, role/permission, scope, reason/reference, effective/expiry time, prior/new version, outcome, Correlation ID, and safe evidence.
- Optimistic concurrency prevents stale overwrite.
- Permission definitions come from the governed catalog; an administrator cannot create an arbitrary code/meaning through the assignment UI.

## 9.2 Recommended role-assignment flow

```text
authenticate assigner online
→ require admin.role.assign or admin.role.revoke
→ require fresh reauthentication
→ validate target active and role canonical
→ validate assigner delegation scope
→ validate segregation of duties and self-assignment rule
→ validate effective/expiry and current row version
→ persist assignment/revocation in owner module transaction
→ publish safe invalidation after commit
→ audit safe before/after identifiers and outcome
→ target session refresh/revalidation
```

## 9.3 Permission assignment

Recommended order of preference:

1. Put stable persona defaults in `RolePermissions`.
2. Use principal-specific scoped grants only for real duties that do not justify a formal role, especially Support.
3. Use external-group mapping only if Pertamina supplies an approved authoritative group and the backend maps/version-controls it.
4. Never create ad-hoc roles per person, unit, temporary task, environment, or single permission merely to work around a missing scoped-grant model.

Principal-specific grants, if approved, should contain:

```text
principal
permission
scope type/reference
valid from / expires at
assigned by / reason/reference
assignment version
revoked at / revoked by
concurrency token
```

This is a logical requirement, not production DDL.

## 9.4 Revocation behavior

- Revocation takes effect immediately for new online authorization decisions after commit/invalidation.
- Active API/session/permission caches must be invalidated or forced to re-resolve within the approved propagation objective.
- SignalR is closed/reconnected when role/permission/session changes become known.
- The target cannot retrieve/control results after permission/account revocation.
- Long-running jobs recheck policy at safe boundaries and cancel/quarantine/withhold output according to Document 08 workload policy.
- Offline residual access is limited to the pre-existing narrow grant until absolute expiry or reconnect.
- Revocation does not delete audit evidence or user source files.
- Regrant is a new authorized assignment, not silent removal of the revocation record.

## 9.5 Stale assignment handling

- Every assignment and permission catalog carries a version/effective state.
- Online sensitive actions use current authoritative state, not only token claims.
- A stale client projection triggers refresh/deny; it never receives a grace period for privileged action.
- An assignment that has expired, been revoked, references an unknown permission, or violates current SoD policy is inactive.
- If the authorization store is unavailable and no safe decision can be made, privileged/server action fails closed.

## 9.6 Preventing administrative lockout

The final matrix should define a governed rule for last-authorized-administrator/assignment-owner removal and emergency recovery. This report does not create a break-glass content role. Any emergency administrative recovery must be separate from content access, time-bounded, reauthenticated, independently approved, and audited.

## 9.7 Persistence gap requiring decision

Document 07 defines `admin.Roles`, `admin.Permissions`, `admin.RolePermissions`, and `admin.PrincipalRoles`, but no principal-specific permission/scope assignment. Because Support is not a formal role, Document 09 must select one controlled approach before implementation:

- add a module-owned scoped principal-permission assignment model through a controlled Document 07 revision; or
- map an approved Pertamina Support group to scoped permissions through a versioned backend configuration; or
- provide another governed backend-owned equivalent with the same traceability, expiry, scope, revocation, and concurrency properties.

Until selected, Support assignment is a design gap. Do not solve it by making all Administrators Support or by creating a hidden Superadmin wildcard role.

---

# 10. Admin boundary

## 10.1 Administrative authority

Administrator may be authorized to manage:

- safe user/account and session metadata;
- role and approved permission assignments;
- feature exposure and flags;
- limits, maintenance, minimum client version, compatibility;
- approved policy, retention, classification, and security-profile settings;
- safe job/queue/sync/error/server/storage/application health metadata.

Each surface requires distinct read/update/assign/revoke permissions. “Administrator” is not itself the endpoint policy.

## 10.2 Content exclusion

The following remain excluded unless a completely separate approved content-access capability exists:

- user source/document bytes;
- job input/result content and preview;
- secured-file/version content;
- full OCR/extracted text;
- PDF password;
- clipboard and visual signature asset;
- Issue screenshot/attachment;
- prompt/chat/provider request/response;
- access/refresh token, cookie, key, or service secret;
- arbitrary storage path/reference.

Filename, path, identity, Job/Correlation ID, and storage reference are confidential metadata and are minimized even for administrators.

## 10.3 Self-elevation prevention

- Separate `admin.role.assign`, `admin.permission.assign`, and configuration-update permissions.
- Default deny self-assignment and assignments beyond delegation scope.
- Run SoD policy before commit.
- Require reauthentication and optimistic concurrency.
- Audit actor, target, approved code/scope, reason/reference, before/after version, and outcome.
- Do not allow an admin to alter permission definition semantics or hard safety policy.
- Do not infer Support, Auditor, content, database-owner, backup-reader, deployment, or Worker permission from Administrator.

## 10.4 Monitoring boundary

Monitoring returns aggregate/safe metadata by default. Job monitoring does not allow job result download; storage health does not allow object read; error monitoring does not expose raw stack/content; Correlation ID tracking does not authorize the referenced resource.

## 10.5 Administrative action categories

| Category | Default posture |
|---|---|
| Read safe admin/config/health metadata | Candidate Administrator default within assigned scope |
| Enable/disable user or revoke session | Optional elevated; online; reauth; Audit |
| Assign/revoke role/permission | Optional elevated; online; reauth; SoD; Audit |
| Update feature/limit/maintenance/compatibility | Optional; typed, `If-Match`, hard bounds; risk-based reauth |
| Update security/classification/retention policy | Optional sensitive; reauth; approval/SoD; Audit |
| Read Audit evidence | Separate Auditor/audit permission |
| Read user content | Prohibited by baseline implication |

---

# 11. Auditor boundary

## 11.1 Read-only evidence model

Auditor receives controlled read of the Enterprise Audit plane, not read access to all resources referenced by audit events.

```text
audit event metadata may reference JobId/FeatureId/opaque object ID
but the reference does not authorize Job/result/document access
```

## 11.2 Scope

Audit scope should be typed and explicit, for example approved combinations of:

- unit/organizational scope;
- principal scope;
- date/time range;
- Feature/event category;
- status/outcome;
- platform/environment;
- classification-safe field projection.

The final allowed scope types and assignment authority require business/compliance approval. Arbitrary client-supplied filters cannot expand scope.

## 11.3 Default and optional

- Candidate default: `audit.event.list` and `audit.event.read` within assigned scope.
- Candidate optional: `audit.export.create` and `audit.export.read` because export increases volume, persistence, and disclosure risk.
- Search/filter are allowed query behavior under authorized list/read and are not separate write powers.
- Retention-policy administration is not an Auditor default.
- Audit ingestion/sync/retry is a system/client capability, not an Auditor capability.

## 11.4 Prohibited actions

- mutate or delete audit evidence;
- change application configuration/role/permission merely to aid an audit;
- read user document, Issue attachment, Support note, password, token, clipboard, signature, or full OCR without a separate approved capability;
- expand scope through count, cursor, export, or referenced opaque ID;
- use Personal History deletion to affect Enterprise Audit.

## 11.5 Segregation of duties

Where enterprise policy requires independence, the same principal should not both perform a sensitive administrative change and be the sole auditor/approver of that change. The final prohibited combinations, dual-approval rules, and exception authority belong to the approved matrix/policy.

---

# 12. Support boundary

## 12.1 Support as a permission profile

Support/Superadmin is a visible collection of individual Support permissions, not a formal role. UI access is computed from effective permissions; the label does not grant authority.

```text
Support Console visible
only if at least one required Support permission is effective
and current online policy permits the surface
```

## 12.2 Granularity

Preserve Document 06's separation:

| Surface | Permission category |
|---|---|
| Issue collection | `support.issue.list` |
| Issue message/detail/safe diagnostics | `support.issue.read` |
| Explicitly submitted screenshot/attachment | `support.attachment.read` |
| Category/priority/status/duplicate/not-bug/need-info | `support.issue.triage` |
| Internal notes | `support.note.create` / `support.note.read` |
| PDF/Excel export | `support.export.create` / `support.export.read` |

Do not collapse these into `support.issue.manage` or `superadmin.all`.

## 12.3 Resource scope

- Support permission is evaluated against an approved Issue/case/queue scope.
- Parent Issue authorization and attachment membership are both verified.
- A Job ID or Correlation ID in the Issue provides safe correlation only.
- Export filters cannot exceed the operator's Issue list/detail scope.
- Generated export remains access-controlled, expiring, and reauthorized on download.
- Internal notes may have narrower visibility/retention than Issue message.

## 12.4 Content limits

Support may access only the message, safe diagnostics, and attachment explicitly sent through the Issue contract and permitted by its grants. Support does not receive original document, unrelated result, secured file, local workspace, arbitrary object storage, full logs, password, token, clipboard, signature, or full OCR.

## 12.5 Reauthentication and online requirement

All Support Console operations are online-only. Reauthentication is recommended for bulk export, broad/high-classification attachment disclosure, permission assignment, and any future exceptional content access. Ordinary Issue triage may rely on the current valid session unless enterprise risk policy requires freshness.

## 12.6 No implicit break-glass

No “view as user,” unrestricted impersonation, storage browsing, or emergency content access exists. If later approved, emergency content access must be a separate permission and workflow with purpose, resource, scope, time limit, reauthentication, independent approval, notification/evidence, and Document 10 Audit coverage.

---

# 13. Risks

| ID | Risk/open decision | Impact | Recommendation/owner |
|---|---|---|---|
| `RPM-RISK-001` | No selected persistence/mapping for principal-specific Support permissions | Temptation to make all Administrators Support or create wildcard role | Decide controlled scoped principal grants or approved external mapping; Document 09 + controlled Document 07 follow-up |
| `RPM-RISK-002` | Final role-to-permission mapping not yet approved | Excessive or missing capability | Build matrix from candidate defaults/optional rows and obtain business/security approval |
| `RPM-RISK-003` | Exact Pertamina claims/group mapping unknown | Raw group may be over-trusted or role sync may fail | Backend-owned versioned allowlist mapping; Pertamina IAM confirmation |
| `RPM-RISK-004` | No approved delegation/scope vocabulary | Cross-user admin/support/audit access may become unbounded | Define typed scope catalog; prohibit generic `ALL` |
| `RPM-RISK-005` | Segregation-of-duties combinations not defined | Self-approval or concealed privileged change | Define prohibited combinations/dual control/exception authority |
| `RPM-RISK-006` | Self-assignment and assignment-beyond-authority rules not finalized | Vertical privilege escalation | Default deny; reauth; delegation ceiling; independent approval where required |
| `RPM-RISK-007` | Permission catalog could become 550 manually maintained rows | Drift and mapping errors | Generate user-Feature permissions/mapping mechanically from canonical catalog; review exposed subset |
| `RPM-RISK-008` | UI Feature classification may be mistaken for role permission | `AUDITOR/ADMIN` or `SUPERADMIN` label becomes implicit allow | Treat UI field as exposure hint only; matrix and server policy are authoritative |
| `RPM-RISK-009` | Audit export and retention authority unclear | Bulk disclosure or Auditor write access | Export separate optional permission; retention update admin/compliance-only |
| `RPM-RISK-010` | Administrator monitoring fields not classified | Content/path/storage leakage through metadata UI | Safe projection/field allowlist and separate content permissions |
| `RPM-RISK-011` | Reauthentication trigger/age not finalized | Sensitive actions under- or over-protected | Approve action categories and freshness policy with Document 08/10 |
| `RPM-RISK-012` | Offline grant lifetime and invalidation values not finalized | Stale permission persists too long | Absolute expiry, local-only subset, rollback protection, reconnect revalidation |
| `RPM-RISK-013` | Disable/revoke propagation objective not approved | Online/Hub caches may briefly retain authority | Versioned invalidation, current check for sensitive action, bounded cache/SLA |
| `RPM-RISK-014` | Accepted job behavior after privilege/account revocation varies by workload | Forbidden content may continue processing or be exposed | Define cancel/quarantine/finish-withhold per workload class; result always reauthorized |
| `RPM-RISK-015` | Future team/delegated content sharing has no resource model | Ad-hoc cross-user bypass | Deny until separate business/security/API/database design is approved |
| `RPM-RISK-016` | Service identities may be treated as human roles | Excessive lateral access or user impersonation | Separate subject type, audience, purpose, grant catalog, and token/session path |
| `RPM-RISK-017` | Last-administrator removal/emergency recovery undefined | Administrative lockout or unsafe break-glass | Governed administrative recovery separate from content access |
| `RPM-RISK-018` | Permission definition administration conflicts with migration ownership | Runtime semantic drift | Admin assigns approved codes only; definitions/version changes through controlled release |
| `RPM-RISK-019` | Permission cache/version strategy not finalized | Stale authorization or excessive database load | Define authoritative source, version/invalidation, sensitive-action current check, test gates |
| `RPM-RISK-020` | No final owner for role/permission approval and periodic recertification | Privilege accumulation | Assign business owner, security approver, operational custodian, review/expiry cadence |

Until resolved, the safe default is deny or no assignment; an unresolved risk never authorizes a broader role.

---

# 14. Conflicts

## 14.1 Material contradiction result

```text
Material contradiction against Documents 01–08 : NONE FOUND
New formal role created                           : NO
Superadmin wildcard created                       : NO
Final Role Matrix declared                        : NO
Document 09 master edited                         : NO
Production code introduced                        : NO
```

## 14.2 Apparent conflicts and gaps

| Topic | Source observation | Required interpretation |
|---|---|---|
| Formal roles versus Support | Documents 01/02 define User/Administrator/Auditor; Documents 01/03/04 call Support “Superadmin”; Documents 07/08 explicitly avoid a Superadmin role | Support is a permission profile assigned through a governed mechanism, not a fourth formal wildcard role. |
| Additional roles | Document 01 permits additional roles through permission-based access; current task says do not add without a gap | Permission to add future roles is not a requirement to do so now. Keep three formal roles until an approved business gap exists. |
| Audit UI `AUDITOR/ADMIN` | Feature Catalog marks Audit Features with `AUDITOR/ADMIN` UI | UI exposure category is not authorization. Administrator gets Audit only by explicit audit permission/role. |
| Audit retention | `FEAT-AUDIT-008` is in Audit group, while Auditor cannot write configuration/delete Audit | Treat retention administration as separate admin/compliance policy permission, never Auditor default. |
| Offline Audit queue/sync | `FEAT-AUDIT-005..007` sit in Audit group but are automatic client/system behavior | They are application/system ingestion permissions, not Auditor human write permissions. |
| Permission Management Feature | `FEAT-ADMIN-003` says Permission Management; Document 07 says canonical permission definitions are migration/version owned | Administrator may assign/revoke approved permissions, not invent or redefine permission codes/semantics at runtime. |
| Administrator session revocation | Document 01 says Administrator can revoke; Documents 02/06/08 require permission-based checks | Session revocation is an optional elevated Administrator permission, not a role-name shortcut. |
| Support grant persistence | Document 07 has role mapping tables but no direct principal-permission/scope assignment; Support must not become a formal role | Open design gap requiring a controlled storage/mapping decision before implementation. |
| Role claims from SSO | Pertamina authenticates; backend controls role/permission; exact claims mapping is unconfirmed | Raw SSO groups do not directly authorize. Use an approved versioned backend mapping. |
| Role change during offline | Document 01 says changes apply at sync; Document 08 requires narrow absolute-expiry offline grant | No conflict: online authority changes immediately; offline residual local access is bounded until expiry/reconnect. |
| User Feature permissions | `FR-ROLE-002` says every Feature controlled by permission; Feature Catalog includes user, admin, system, security, workflow, and observability Features | Generate human execute permissions only for user-invocable Features; map domain/admin/system Features to resource/workload policies. |
| Cross-user content | Business documents describe owner, support, audit, and admin boundaries but no general team share/delegation | Deny generic cross-user content. Do not infer share scope from role or organizational unit. |
| Role inheritance | Sources do not require Administrator/Auditor to inherit User | Recommend no implicit hierarchy; explicitly assign multiple roles when duties require them. |

---

# 15. Suggested matrix structure

Document 09 should contain several linked matrices rather than one oversized table that mixes identity, action, scope, and context.

## 15.1 Matrix A — Role catalog

Recommended columns:

```text
RoleCode
DisplayName
BusinessPurpose
DefaultAssignmentPopulation
RoleOwner
AssignmentAuthority
CanBeCombinedWith
SoDRestrictions
OnlineOnlyRoleManagement
ReauthenticationForAssignment
ReviewCadence
SourceRequirements
Status/Version
```

Rows: only `User`, `Administrator`, and `Auditor` unless a later approved gap justifies another formal role. Support is referenced as a permission profile, not a role row.

## 15.2 Matrix B — Permission catalog

Recommended columns:

```text
PermissionCode
Module
ResourceType
Action
Description
SubjectType
CanonicalFeatureId (optional)
Sensitivity
DefaultScopeType
OnlineRequirement
OfflineEligible
ReauthenticationCategory
ContentAccessFlag
BulkDisclosureFlag
DestructiveFlag
SourceRequirement
SourceFeature
API/ApplicationOperation
Owner
Version
Status/Replacement
```

## 15.3 Matrix C — Role × Permission mapping

Use explicit values, not checkmarks with ambiguous meaning:

```text
D = candidate/default permission for the role
O = optional/separately assigned permission
P = prohibited by role baseline
N = not applicable/not a human role permission
```

Recommended columns:

```text
RoleCode
PermissionCode
MappingStatus (D/O/P/N)
DefaultResourceScope
AssignmentScopeLimit
SoDRule
RequiresSeparateRoleOrGrant
Rationale
ApprovalOwner
SourceTrace
```

The final matrix must not use wildcard rows such as `admin.*`, `support.*`, or `feature.*` without an explicitly expanded generated artifact and review.

## 15.4 Matrix D — Resource relationship and cross-user scope

Recommended columns:

```text
ResourceType
OwnerFieldOrRelationship
AllowedScopeTypes
DefaultScope
CrossUserAllowed
CrossUserPurpose
ContentOrMetadata
ParentChildValidation
ConcealmentRule
ListScopeBeforeCount
StorageResolutionGate
Source/API/DatabaseOwner
```

This matrix makes the difference between `jobs.job.read` and actual permission to read a particular job explicit.

## 15.5 Matrix E — Sensitive/context action

Recommended columns:

```text
PermissionCode/ActionCategory
OnlineOnly
OfflineEligible
ReauthenticationRequired
MaxAuthenticationAgePolicyRef
ExplicitConfirmation
DualApproval/SoD
ResourceStateRequirement
ClassificationGate
ConsentGate
AuditEventRef
FailureStatus/ErrorCode
```

Document 10 supplies the final Audit Event reference and field/retention design.

## 15.6 Matrix F — Assignment and revocation governance

Recommended columns:

```text
GrantType (Role/Principal/ExternalMapping/Workload)
AssignableByPermission
TargetSubjectType
AllowedScope
SelfAssignment
GrantBeyondOwnAuthority
ReasonRequired
Effective/ExpirySupported
Reauthentication
Approval/SoD
Concurrency
RevocationPropagation
Recertification
Evidence/AuditRef
```

## 15.7 Matrix G — Workload identities

Recommended columns:

```text
WorkloadIdentityCode
Component/Purpose
Audience
PermissionCode
Resource/Queue/JobScope
CredentialLifetime/RotationPolicyRef
HumanImpersonationAllowed (always NO)
EndUserTokenForwarding (always NO)
Environment
Owner
Audit/TelemetryIdentity
```

## 15.8 Illustrative structure rows

The rows below demonstrate structure only; they are not the final matrix:

| Role/Profile | Permission | Mapping | Scope | Context | Note |
|---|---|:---:|---|---|---|
| User | `jobs.job.read` | D | `OWN` | Online for server job | Permission plus owner check |
| Administrator | `admin.session.revoke` | O | Delegated principal/session | Online + reauth | Not implied by title alone |
| Auditor | `audit.export.create` | O | Assigned Audit scope | Online + bounded export; reauth by policy | Separate from read |
| Support profile | `support.attachment.read` | O | Assigned Support case | Online; explicit attachment only | Not a formal role |
| Worker | `jobs.worker.claim` | N for human roles | Queue/workload class | Workload identity + lease | Separate matrix G |

## 15.9 Matrix validation rules

Before approval, validate:

- every protected API/application operation maps to at least one permission and resource/context rule;
- every user-invocable canonical Feature maps to one approved Feature permission;
- no system/internal Feature is accidentally granted to a human role;
- no permission code uses role name, wildcard, `all`, `any`, or `bypass`;
- Administrator/Support/Auditor content-access flags remain `NO` unless a separately approved row exists;
- Auditor has no write/delete row;
- Support has no generic user-document/resource row;
- all cross-user rows identify typed scope and purpose;
- all sensitive rows specify reauthentication/confirmation/SoD/Audit behavior;
- all offline rows are local-only and absolute-expiry compatible;
- role/permission revocation has a propagation and stale-session rule;
- workload permissions cannot be assigned to human principals;
- each permission has Requirement/Feature/API/application traceability and a test owner.

---

# 16. Suggested Document 09 text

The following text is ready for controlled integration into the Document 09 master. It defines the model and matrix rules but does not pre-approve every Role × Permission row.

## 16.1 Authorization model

PERTAMINA OTO shall use a five-dimension authorization model consisting of Role, Permission, Policy, Resource, and Context. Role groups common permissions; Permission represents one action category; Policy applies enterprise/security/classification restrictions; Resource establishes the concrete object and owner/scope relationship; Context applies current session, connectivity, client, capability, state, consent, and reauthentication conditions.

Authorization shall allow an action only when the subject is authenticated, active, and using a valid session; the action permission is effective; the subject type and resource relationship are allowed; resource state, policy, classification, client/capability, connectivity, consent, and reauthentication requirements all pass. Unknown, stale, missing, or denied input shall fail closed.

Role names shall not be used as final authorization decisions throughout controllers, endpoints, application services, Workers, or clients. ASP.NET Core permission policies shall perform reusable action checks, and resource authorization handlers/application services shall evaluate the concrete object and context before content access, mutation, dispatch, export, or subscription. UI visibility and cached entitlements are presentation only; the server/application boundary remains authoritative.

## 16.2 Formal roles

The formal role baseline is:

```text
User
Administrator
Auditor
```

No role hierarchy is implied. Administrator and Auditor do not automatically inherit User. A principal may receive multiple roles when approved duties require them, subject to segregation-of-duties policy.

`User` represents baseline employee capability to execute approved Features and manage the user's own work/resources under current policy. It does not grant administration, Support, another user's resource, or Enterprise Audit access.

`Administrator` represents granular account, session, role/permission assignment, feature exposure, configuration, policy, limit, compatibility, and safe operational-health administration. It does not automatically grant User, Auditor, Support, document-content, job-result, secured-file, OCR, password, clipboard, signature, token, or secret access.

`Auditor` represents controlled read-only Enterprise Audit evidence within an assigned audit scope. Audit export is a separate optional permission. Auditor cannot mutate configuration, role, permission, Issue, job, or Audit evidence and does not receive referenced document content.

“Superadmin” is the user-facing label for a principal holding approved Support permissions. It shall not be implemented as a formal wildcard role. Support permissions for Issue list, detail, explicitly submitted attachment, triage, note, and export shall remain separate and shall not grant original user-document or unrelated resource access.

## 16.3 Permission catalog and naming

Permission codes shall use:

```text
module.resource.action
```

Codes shall be lower-case ASCII, stable, exactly three baseline segments, and use a controlled action vocabulary. Role, owner/scope, environment, unit, resource ID, `own`, `all`, `any`, `superadmin`, `full-access`, and `bypass` shall not appear in the code. Ownership and context shall be evaluated separately.

Examples:

```text
account.profile.read
jobs.job.cancel
secured.version.download
support.issue.triage
audit.export.create
admin.session.revoke
configuration.policy.update
feature.feat-pdf-005.execute
```

Permission definitions shall record module, resource, action, subject type, optional canonical Feature ID, sensitivity, default scope, online/offline behavior, reauthentication category, source trace, owner, and version/status. Permission meaning shall not change in place. Administrators may assign approved catalog permissions when authorized; they shall not create arbitrary codes or redefine semantics at runtime.

User-invocable canonical tool Features shall receive mechanically generated `feature.<canonical-feature-id>.execute` permissions. Resource, administrative, Support, Audit, monitoring, security, and system Features shall map to resource or workload permissions rather than duplicate generic execute grants. Every mapping shall preserve the canonical Feature ID for traceability.

## 16.4 Default and optional grants

Role defaults shall contain only the minimum common permissions needed for the role's business purpose. Narrower or higher-risk duties shall use optional scoped grants rather than expanding the whole role.

Candidate `User` defaults include safe profile/preferences; approved employee Feature execution; own jobs/results/history/secured files; own Issue reporting/status/explicit attachment; own Activity notification; and eligible local work while the protected offline grant remains valid. High-risk/internal-provider Features, permanent purge, classification-restricted export/share/copy/print/download, and future delegated resource access remain optional or policy-gated.

Candidate `Administrator` defaults include read-only safe account/authorization/configuration/feature/limit/compatibility/health metadata. User disable/enable, session revoke, role/permission assignment, and configuration/policy/security/classification/retention changes are optional elevated permissions with online authorization, reauthentication where required, delegation/segregation checks, concurrency, and safe Audit evidence.

Candidate `Auditor` defaults include audit-event list/read within assigned scope. Audit export is optional and bounded. Audit event update/delete is prohibited. Audit retention administration belongs to a separate admin/compliance policy permission, and offline Audit ingestion/sync is a system capability rather than an Auditor write permission.

Support permissions are optional duty grants to approved principals and are not default permissions of a formal role. Attachment, note, and export shall be separately justified from list/detail/triage.

## 16.5 Resource ownership and cross-user access

Permission shall never replace resource ownership/scope. Owner or reporter identity shall be derived from the authenticated principal and persisted resource, not from the request body. Lists shall be scoped before count, filter, search, sort, pagination, or cursor generation. Child resources shall be verified against the authorized parent. Content/storage shall be resolved only after content-specific permission and object authorization succeed.

Cross-user access is denied by default. Current approved cross-user purposes are limited to Administrator safe metadata within administrative scope, Support Issue content/explicit attachment within Support scope, Auditor safe evidence within Audit scope, and Worker job-scoped data under workload identity. None grants original user-document content. Future team/delegated/emergency content access requires a separate approved permission, typed resource relationship, policy, reauthentication, and Audit design.

Metadata and content permissions shall remain distinct. Job read does not imply result download; secured-file metadata read does not imply version-content download; Support Issue read does not imply attachment read; Audit/monitoring/admin references do not imply referenced-resource access.

## 16.6 Policy and context

Permission grants shall be restricted by hard security invariants, enterprise/classification policy, Feature/capability policy, resource ownership/state/retention/legal hold, organizational assignment scope, operational safety bounds, environment/platform/device capability, and user preference in that order. Any deny or unsatisfied mandatory condition shall stop the action. Administrator cannot override hard security/safety invariants or change canonical Feature meaning.

Context shall include online/offline state, session/revocation status, permission/policy versions, authentication freshness, client platform/version, capability/provider, resource state, classification, consent, assignment validity, environment, and workload lease/purpose. These states shall not be encoded as additional role names.

## 16.7 Sensitive authorization

Sensitive actions shall require normal permission/resource/policy authorization plus fresh approved reauthentication bound to the current principal/session, action, resource/scope, and policy version. Reauthentication shall not add permission. Confirmation of intent and reauthentication are separate controls and may both be required.

Candidate sensitive categories include role/permission assignment or revocation, account disable/enable, another user's session revocation, security/classification/retention policy change, permanent secured-file purge, high-impact audit/support export, and any future exceptional non-owner content access. The final trigger, assurance, maximum age, retry, dual approval, and Audit-event mapping require Pertamina and Document 10 approval.

## 16.8 Offline authorization

Offline authorization shall use a protected, minimum, absolute-expiry projection of offline-eligible permissions. It shall not create offline roles or duplicate permission codes. The same Feature permission may execute locally only when the Feature is offline-compatible, the mounted local profile owns the resource, the cached grant/policy remains valid, classification permits local work, and no online-only or reauthentication requirement applies.

Administrator, Support, Auditor, server resource, account-secured storage, Issue submission, Audit query/export, role/permission assignment, and sensitive actions are online-only. On reconnect, identity/account/session/permission/policy/capability/compatibility shall refresh before queued commands. Every queued command shall be reauthorized; queued intent is not a previous authorization grant.

## 16.9 Assignment, revocation, and disabled users

Role/permission assignment shall be backend-owned and require an authorized assigner, active target principal, canonical role/permission, delegation scope, segregation-of-duties validation, no self-elevation by default, current row version, reason/reference, effective/expiry state where applicable, reauthentication for elevated grants, and safe Audit evidence.

Stable persona defaults shall use role-permission mapping. Principal-specific scoped grants may be used for Support or narrow duties only after a governed persistence/mapping mechanism is approved. Raw SSO groups shall not directly authorize PERTAMINA OTO; any external group shall map through versioned backend configuration. Ad-hoc per-person roles and hidden Superadmin roles are prohibited.

Revocation shall invalidate new online authorization after commit and propagate to session/permission caches and SignalR within the approved objective. A revoked or disabled principal cannot retrieve/control job results. Long-running jobs follow the approved cancel/quarantine/finish-withhold policy. Offline residual local access is bounded by the valid absolute-expiry grant and ends at expiry, reconnect, or learned revocation.

A disabled OTO principal shall receive safe `403 ACCOUNT_ACCESS_DENIED`, and the application shall invalidate known sessions, cached entitlements, offline grants, and SignalR access. Disablement or logout shall not silently delete user source files.

## 16.10 Service identities

Service and Worker identities shall be non-human, audience- and purpose-scoped, environment-specific, short-lived/rotatable, and restricted to the required queue, Job, object, module, or provider operation. They shall not receive User, Administrator, Auditor, or Support roles; shall not have offline or interactive sessions; shall not impersonate a human; and shall never receive a forwarded end-user access/refresh token.

Workload permissions such as job claim/state update, job-scoped artifact/object access, provider invocation, and safe event publication shall be maintained separately from the human role matrix and tested so they cannot be assigned to human principals.

## 16.11 Required matrix artifacts and approval gates

Document 09 shall maintain linked Role Catalog, Permission Catalog, Role × Permission Mapping, Resource Relationship/Scope, Sensitive/Context Action, Assignment/Revocation Governance, and Workload Identity matrices. Each protected route/application action and each user-invocable canonical Feature shall map to a permission, resource/scope rule, context, source requirement/Feature, and test owner.

The matrix shall use explicit Default/Optional/Prohibited/Not-Applicable values; shall not use wildcard grants; and shall demonstrate that Administrator, Support, and Auditor lack implicit content access, Auditor lacks write/delete, cross-user scopes are typed, sensitive actions define reauthentication/segregation/evidence, offline actions are local-only, revocation handles stale sessions, and workload permissions are not human-assignable.

Before implementation, the project shall resolve the current persistence/mapping gap for principal-specific Support permissions without adding a wildcard Superadmin role or giving Support permissions to every Administrator.
