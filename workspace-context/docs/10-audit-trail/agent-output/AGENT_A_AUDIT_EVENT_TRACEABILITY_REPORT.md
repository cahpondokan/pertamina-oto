# AGENT A — AUDIT EVENT & BUSINESS TRACEABILITY REPORT

> **Task ID:** DOC10-AGENT-A  
> **Role:** Senior System Analyst / Business Process Analyst / Audit & Security Analyst / Senior .NET Application Architect  
> **Status:** Specialist recommendation for integration into Document 10 — **not an official retention/legal-hold decision**  
> **Date:** 26 Agustus 2026  
> **Authoritative baseline:** Documents 01–09 FINAL, including `09_ROLE_MATRIX_FINAL_v1.0_PERTAMINA_OTO.md` (`18/18 AUTO-LOCKED`), and the latest available `MASTER_PROJECT_SUMMARY.md` v3.1  
> **Canonical feature baseline:** 550 Features; 33 Primary PDF Tool Cards  
> **Constraint:** No new role, no Feature ID change, no production code, no official retention duration, and no modification to the Document 10 master

> **Source reconciliation completed:** This report has been revalidated against Document 09 FINAL v1.0. The latest Master Project Summary remains v3.1 (24 Agustus 2026) and still labels Document 09 as `NEXT`; that status lag is recorded as `AUD-RISK-001`, not treated as authority over the newer FINAL document.

Terminology:

- **Enterprise Audit** — controlled append-oriented evidence in the `audit` plane.
- **Personal History** — user convenience projection; user-deletable and not compliance evidence.
- **Observability** — operational metrics/logs/traces for health and detection; not the business Audit trail.
- **Support record** — Issue message, attachment, note, and workflow in the `support` plane; not copied into Audit.
- **Core AuditRequired = YES** — every Core execution produces safe, minimal process trace metadata as required by Document 09; the applicable evidence plane is still selected by risk and route (`L1`, `L2`, or `L3`). It does not mean full Enterprise Audit detail for every local tool.
- **AuditRequired = CONDITIONAL** — Enterprise Audit is produced only when the stated route, policy, risk, or context applies.

---

# 1. Audit principles

## 1.1 Audit important business/security facts, not user exhaust

PERTAMINA OTO should record an Enterprise Audit event only when the event provides durable evidence of at least one of these concerns:

1. identity or session security;
2. privilege, role, permission, or account eligibility change;
3. policy/configuration change with enterprise impact;
4. cross-boundary processing, transfer, or persistence;
5. secured-file lifecycle or irreversible deletion;
6. privileged Support or Audit evidence access;
7. high-risk PDF security action;
8. approved AI/internal-provider use or policy denial;
9. material security denial, attack indicator, or control failure;
10. a business action for which accountability cannot be reconstructed safely from the owning domain alone.

Do not treat Audit as “log everything.” Mouse events, page opens, progress percentages, cache work, thumbnail generation, autosave, ordinary undo, and other high-volume implementation details do not belong in Enterprise Audit.

## 1.2 Four evidence levels

| Level | Name | Purpose | Typical store/handling |
|---|---|---|---|
| `L0` | Not audit evidence | UI/technical activity with no durable accountability value | None or bounded observability only |
| `L1` | Personal History | User convenience: what tool ran, when, status, result reference | Local/server `history` projection; owner-deletable; not compliance evidence |
| `L2` | Enterprise Business Audit | Important business action, centralized processing/transfer, secured lifecycle, Support action | Append-oriented `audit.AuditEvents`; controlled read/export |
| `L3` | Privileged/Security Audit | Privilege/policy change, high-risk security action, privileged evidence/content access, security incident indicator | Same controlled Audit plane with stricter query/alert/access classification |

`L3` is an access/severity classification, not an invented retention duration. Official retention, legal hold, archival, and purge authority remain governed decisions.

## 1.3 One logical action, bounded events

- Emit one request/admission event only when admission itself matters, such as server transfer, privileged mutation, or export request.
- Emit one terminal event for the logical business outcome.
- Do not emit Enterprise Audit for every progress update, job heartbeat, page processed, chunk uploaded, queue poll, Worker attempt, or SignalR notification.
- Retry/resume is recorded only when user/system action changes business traceability or security state; low-level transport retry remains observability.
- Use a stable `AuditEventId` and idempotent ingestion so retries cannot create duplicate evidence.
- For bundled cards/workflows, record the actual canonical Feature ID for each materially auditable step, not only the card name or ID range.

## 1.4 Plane separation

```text
Enterprise Audit → who did an important action, to which safe resource, and outcome
Personal History → user's own processing convenience
Observability    → performance, health, retry, queue, parser, and infrastructure behavior
Support          → explicit Issue message/attachment/notes/triage
Domain state     → authoritative current state of Job, secured file, policy, Issue, etc.
```

No plane silently copies restricted content from another. Enterprise Audit references an opaque resource ID and safe event code; it does not become a second document, Issue, policy payload, or log archive.

## 1.5 Actor truth

- Record the real human initiator when one exists.
- Assistant has no independent permission. If a user asks Assistant to execute Merge PDF, the primary actor is the `User`; `Origin=ASSISTANT`.
- A Worker executing an accepted user job is the executor, not the business initiator. Store the user as primary actor and Worker/service identity as executor when needed.
- A scheduled cleanup or policy worker with no human request uses `ActorType=SYSTEM` or `SERVICE` and records its policy/job trigger.
- Role snapshot supports historical context but never replaces the stable actor principal.

## 1.6 Content minimization

Audit may answer WHO, WHAT, WHEN, CHANNEL, RESOURCE, WHY/RESULT, OUTCOME, FEATURE, JOB, and CORRELATION without storing:

- document/file bytes or text;
- filename/path by default;
- screenshot or Issue attachment bytes;
- PDF password;
- access/refresh token, cookie, session secret, key, or claim dump;
- clipboard content;
- visual-signature payload/template;
- full OCR/extracted/translated/summary/Markdown content;
- Assistant prompt/chat or provider request/response;
- raw exception, SQL, storage key, unrestricted URL, or engine dump;
- redacted content or redaction coordinates/text;
- form values or scanned image content.

## 1.7 Failure and denial selection

- Record terminal failures of an already-audited business action.
- Record failed login according to identity/security policy.
- Do not record every ordinary validation error or every `403/404` as Enterprise Audit.
- Promote denial to `L3` when it involves privilege, cross-user resource probing, repeated abuse, token/session replay, forbidden public AI, path/IPC escape, malware, or bypass of a high-risk control.
- Rate-limit and aggregate repetitive identical security denials while preserving counts, first/last time, subject/source, and correlation evidence.

---

# 2. Event taxonomy

## 2.1 Required categories

| Category | Scope | Typical level | Examples |
|---|---|---:|---|
| `AUTH` | Authentication, session, logout, revoke, disable, reauthentication | `L2–L3` | Login success/failure, session revoke, account access denied |
| `AUTHORIZATION` | Material permission/scope decision and suspected access bypass | `L3` when important | Cross-user denial, privileged permission denial |
| `ADMIN` | Account eligibility, role/permission/Support grant administration | `L3` | Role assigned, permission revoked, account disabled |
| `POLICY` | Security, classification, retention, reauth, disclosure policy | `L3` | Classification policy changed |
| `CONFIGURATION` | Feature exposure, flag, limit, maintenance, minimum version | `L2–L3` | Feature disabled, limit changed |
| `PROCESSING` | Important execution/admission/terminal event | `L1` local low-risk; `L2` server/cross-boundary | Server job accepted/completed; local Feature History |
| `SECURED_FILE` | Amankan ke Akun, version, download, recycle, restore, purge | `L2–L3` | Version created, permanent deletion |
| `SUPPORT` | Issue submission, privileged view, triage, note, export | `L2–L3` | Attachment viewed by Support, status changed |
| `AUDIT_ACCESS` | Query/detail/export/download of Audit evidence | `L3` | Audit query, Audit export download |
| `SECURITY` | Attack/control failure/detection | `L3` | Token replay, malware, public-AI route blocked |
| `ASSISTANT` | Assistant-origin orchestration metadata | Inherits underlying action; `L2–L3` when material | High-risk step confirmed, forbidden tool request blocked |
| `AI` | Approved local/internal Content Intelligence route/provider boundary | `L2–L3` | Provider transfer authorized, public provider blocked |
| `HIGH_RISK_FEATURE` | Protect, Unlock, visual Sign, Redact and similar security finalization | `L3` | Redaction finalized, Unlock failed/denied |

## 2.2 Event name convention

Use stable PascalCase event names with category prefix:

```text
Auth.LoginSucceeded
Admin.RoleAssigned
Policy.ClassificationChanged
Processing.ExecutionCompleted
SecuredFile.VersionCreated
Support.IssueTriaged
AuditAccess.ExportDownloaded
Security.PublicAiRouteBlocked
HighRiskFeature.RedactionFinalized
```

Rules:

- name the business/security fact, not the UI control or HTTP method;
- use past tense for committed facts;
- do not encode outcome in arbitrary message text when a typed `Outcome` exists, except when distinct facts need separate controls;
- event meaning is immutable per `EventSchemaVersion`;
- a breaking semantic change creates a new version or event name with migration handling;
- localized display labels are separate from `EventName`.

## 2.3 Outcome vocabulary

Recommended controlled values:

```text
ACCEPTED
SUCCEEDED
FAILED
DENIED
CANCELLED
EXPIRED
REVOKED
PARTIALLY_COMPLETED   (only when the business contract explicitly supports it)
```

An incomplete, unvalidated, quarantined, or temporary output is never `SUCCEEDED`.

---

# 3. Actor taxonomy

## 3.1 Actor types

| Actor type | Primary actor rule | Example |
|---|---|---|
| `USER` | Human employee using own business capability | User runs Merge PDF |
| `ADMINISTRATOR` | Human principal performing an authorized admin action | Admin revokes a session |
| `AUDITOR` | Human principal reading/exporting Audit evidence | Auditor runs scoped query |
| `SUPPORT` | Human principal with granular Support permission | Support triages Issue |
| `SYSTEM` | Deterministic application/scheduler action with no service credential identity relevant to business evidence | Version rotation after fourth secured version |
| `SERVICE` | Non-human workload/service identity | Outbox publisher or export service |
| `WORKER` | Local/Server Worker executing a job/Feature step | Server Worker completes conversion |
| `ASSISTANT_ON_BEHALF` | **Assistant on behalf of User**: never primary when a human initiated it; recorded as Origin while the User remains primary actor | Assistant invokes Merge for User |

## 3.2 Actor fields

Recommended actor metadata:

```text
ActorPrincipalId            opaque internal principal when human
ActorType                   USER/ADMINISTRATOR/AUDITOR/SUPPORT/SYSTEM/SERVICE/WORKER
ActorRoleSnapshotCodes      approved active role/profile context, not authorization proof
ActorUnitSnapshotCode       only when available and approved
Origin                      UI/FILE_FIRST/ASSISTANT/API/ADMIN_CONSOLE/
                            AUDIT_CONSOLE/SUPPORT_CONSOLE/BACKGROUND/SYSTEM
ExecutorType                NONE/SERVICE/WORKER/SYSTEM
ExecutorIdentityId          safe opaque workload identity code when applicable
OnBehalfOfPrincipalId       only when executor/service performs an accepted human action
```

Do not store email/display name/employee number as the event identity key. Authorized reporting may resolve the opaque actor reference against approved identity data.

## 3.3 Actor attribution examples

| Scenario | Primary actor | Origin | Executor | Resource/action |
|---|---|---|---|---|
| User clicks Merge PDF | User | `UI` | Local Worker if used | Feature execution |
| User asks Assistant to Merge PDF | User | `ASSISTANT` | Local Worker if used | Feature execution |
| User submits optional server OCR | User | UI/File-First/Assistant | Server Worker | Job/Feature execution |
| Admin disables a Feature | Administrator | `ADMIN_CONSOLE` or API | Service | Feature configuration change |
| Auditor exports evidence | Auditor | `AUDIT_CONSOLE` | Export service | Audit export |
| Support opens submitted screenshot | Support | `SUPPORT_CONSOLE` | API/storage adapter | Issue attachment access |
| Secured fourth version rotates oldest | System | `SYSTEM` | Lifecycle service | Secured version rotation |
| Worker detects malware | Worker/Service | `BACKGROUND` | Scanner/Worker | Security finding; initiating User referenced if applicable |
| Assistant requests forbidden public AI | User | `ASSISTANT` | Policy service | Security denial |

## 3.4 Role snapshot limitations

- Store only approved role/profile codes active for the action.
- Support is a permission profile, not a formal Superadmin role.
- Role snapshot explains context but permission/resource/policy made the decision.
- Do not store the full permission set or claims dump in each event.
- For privileged events, store the relevant `PermissionCode` and authorization-policy version instead.

---

# 4. Resource taxonomy

## 4.1 Auditable resource types

| Resource type | Safe reference | Examples of auditable action | Explicit exclusion |
|---|---|---|---|
| `ACCOUNT` | Opaque `PrincipalId`/account ref | Enabled, disabled, eligibility denied | Profile content/claims dump |
| `SESSION` | Hashed/opaque session reference | Revoked, logged out | Token/cookie/session secret |
| `ROLE_ASSIGNMENT` | Assignment ID + Role Code | Assigned/revoked | Full directory membership dump |
| `PERMISSION_ASSIGNMENT` | Assignment ID + Permission Code | Assigned/revoked | Full permission list in every event |
| `POLICY` | Policy ID/key + version/scope code | Changed/activated | Whole sensitive policy payload |
| `FEATURE_CONFIGURATION` | Canonical Feature ID + configuration version | Enabled/disabled/flag changed | Handler/engine secret/internal code |
| `LIMIT_CONFIGURATION` | Limit key + scope/version | Updated | Infrastructure topology/secret ceiling detail |
| `FEATURE_EXECUTION` | Process ID + actual Feature ID | Accepted/completed/failed | Input/output content/path/name |
| `JOB` | Opaque Job ID | Accepted/cancelled/retried/terminal | Checkpoint/result bytes |
| `SECURED_FILE` | Opaque Secured File ID | Created/recycled/restored/purged | File bytes/name/path by default |
| `SECURED_FILE_VERSION` | Opaque File/Version IDs | Version created/rotated/downloaded | Content/digest when not required |
| `ISSUE` | Opaque Issue ID | Submitted/triaged/status changed | Message/note text |
| `ISSUE_ATTACHMENT` | Opaque Attachment ID + parent Issue | Submitted/viewed/downloaded | Screenshot/attachment bytes |
| `AUDIT_QUERY` | Query request ID + safe scope/filter fingerprint | Query executed/denied | Result event payloads |
| `AUDIT_EXPORT` | Opaque Export ID/Job ID | Requested/completed/downloaded | Export bytes/filter values beyond safe scope |
| `ASSISTANT_WORKFLOW` | Workflow execution ID/fingerprint | High-risk confirmation/step execution | Prompt/chat/document context |
| `AI_OPERATION` | Opaque operation/provider-profile code | Transfer/invocation/completion/denial | Prompt/content/provider response |
| `SECURITY_FINDING` | Finding/aggregation ID | Malware/replay/bypass detected | Malicious payload/content |
| `WORKLOAD_OPERATION` | Workload identity + Job/operation ID | Claim denied, scope violation | Service credential/token |

## 4.2 Resource reference rules

- Resource ID is an opaque locator, never an authorization grant.
- Store parent and child references only when needed to prove relationship, such as Issue + Attachment or Secured File + Version.
- Do not store local full path, storage key, durable delegated URL, or filename by default.
- A safe filename category or protected display reference may be added only if a confirmed Audit requirement outweighs privacy risk.
- Store a policy/configuration version and safe changed-field codes; do not duplicate whole before/after payloads.
- For content transformations, store input/output count and size category/bytes as approved metadata, never content fingerprints that can expose sensitive data unless integrity policy explicitly requires them.

---

# 5. Business event catalog

## 5.1 Catalog notation

| Code | Meaning |
|---|---|
| `F=R` | `FeatureId` required |
| `F=C` | `FeatureId` conditional/applicable |
| `J=R` | `JobId` required |
| `J=C` | `JobId` when server/background job exists |
| `C=R` | `CorrelationId` required |
| `C=C` | `CorrelationId` when request/workflow exists |
| `X1` | Exclude credential/token/password/cookie/key/claims dump |
| `X2` | Exclude document/OCR/clipboard/path/filename/content |
| `X3` | Exclude signature payload, redacted text/region, form values |
| `X4` | Exclude prompt/chat/provider request/response/translated/summary content |
| `X5` | Exclude Issue message/note/screenshot/attachment bytes |
| `OFFLINE-BATCH` | Create protected local event with stable ID; batch after reconnect |
| `ONLINE` | Event originates from authoritative server action; no offline privileged action |

Each catalog row assumes `EventName`, `EventSchemaVersion`, actor, resource, `OccurredAtUtc`, `ReceivedAtUtc`, outcome, channel/platform/app version, execution location, and safe reason/error code from Section 11.

## 5.2 Authentication and authorization events

| EventName | Category | Actor / Origin | Action | Resource | Outcomes | IDs | Level | Sensitive exclusions | Offline | Audit-required reason |
|---|---|---|---|---|---|---|---:|---|---|---|
| `Auth.LoginSucceeded` | AUTH | Human / approved SSO | Establish application session | ACCOUNT/SESSION | SUCCEEDED | `C=C` | L2 | X1 | Online; received server-side | Accountability and session start |
| `Auth.LoginFailed` | AUTH | Attempted subject when resolvable; otherwise SYSTEM | Reject authentication | ACCOUNT/SESSION | FAILED/DENIED | `C=C` | L2; L3 when abuse | X1; no account-enumeration detail | Online | Required by Documents 01/02; security detection |
| `Auth.LogoutCompleted` | AUTH | Human / UI/API | End own session | SESSION | SUCCEEDED/FAILED | `C=C` | L2 | X1 | Queue safe logout metadata only if offline behavior supports it | Required login/logout trace |
| `Auth.SessionRevoked` | AUTH/ADMIN | Admin/System; target separate | Revoke session | SESSION/ACCOUNT | REVOKED/FAILED/DENIED | `C=R` | L3 | X1 | ONLINE | Privileged security action |
| `Auth.AccountAccessDenied` | AUTH | Human / any channel | Deny disabled/ineligible account | ACCOUNT | DENIED | `C=C` | L3 when policy/security-relevant | X1; minimize reason | Online; invalidate offline when learned | Disabled-user accountability |
| `Auth.ReauthenticationSucceeded` | AUTH | Human / sensitive action | Establish scoped freshness | SESSION + target resource type | SUCCEEDED | `F=C,J=C,C=R` | L3 | X1; no challenge detail | ONLINE | Sensitive action proof |
| `Auth.ReauthenticationFailed` | AUTH | Human / sensitive action | Reject freshness | SESSION + target resource type | FAILED/DENIED | `F=C,J=C,C=R` | L3 | X1 | ONLINE | Security and abuse evidence |
| `Auth.OfflineGrantInvalidated` | AUTH | System | Invalidate cached offline authorization | ACCOUNT/profile | REVOKED/EXPIRED | `C=C` | L2 conditional | X1; no permission dump | Local event may batch | Explains loss of offline authority; do not audit every expiry if policy does not require it |
| `Authorization.PrivilegedActionDenied` | AUTHORIZATION | Human/service, actual origin | Deny admin/support/audit/sensitive action | Typed target | DENIED | `F=C,J=C,C=R` | L3 | X1–X5 as applicable | ONLINE | Important privilege denial, not every ordinary 403 |
| `Authorization.CrossResourceScopeDenied` | AUTHORIZATION/SECURITY | Human/service | Deny wrong owner/scope/parent | Typed target | DENIED | `F=C,J=C,C=R` | L3 when suspected IDOR/repeated | X2/X5 | Online; local profile violation may batch | Horizontal escalation evidence |

Successful token refresh, routine permission checks, and ordinary non-abusive resource concealment do not create individual Enterprise Audit events.

## 5.3 Processing events

| EventName | Category | Actor / Origin | Action | Resource | Outcomes | IDs | Level | Sensitive exclusions | Offline | Audit-required reason |
|---|---|---|---|---|---|---|---:|---|---|---|
| `History.FeatureCompleted` | PROCESSING | User; Origin UI/File-First/Assistant | Complete ordinary local Feature | FEATURE_EXECUTION | SUCCEEDED/FAILED/CANCELLED | `F=R,J=none,C=C` | L1 | X2–X4 | Local only; not synced as Enterprise unless required | Personal convenience for low-risk local tool |
| `Processing.ExecutionAccepted` | PROCESSING | User; executor Service/Worker | Admit server/cross-boundary execution | JOB/FEATURE_EXECUTION | ACCEPTED | `F=R,J=R,C=R` | L2 | X2–X4 | ONLINE after durable acceptance | Records server transfer/central processing decision |
| `Processing.ExecutionCompleted` | PROCESSING | User; executor Worker/Service | Complete validated execution | JOB/FEATURE_EXECUTION | SUCCEEDED | `F=R,J=C,C=R` | L2 when server/policy; L1 ordinary local | X2–X4 | Server event or OFFLINE-BATCH if required high-risk local | Business result without content |
| `Processing.ExecutionFailed` | PROCESSING | User; executor Worker/Service | Fail accepted/audited execution | JOB/FEATURE_EXECUTION | FAILED | `F=R,J=C,C=R` | Same as parent action | X2–X4; safe ErrorCode only | Same as parent | Explains failed important action |
| `Processing.ExecutionCancelled` | PROCESSING | User/System; origin recorded | Cancel accepted/audited execution | JOB/FEATURE_EXECUTION | CANCELLED | `F=R,J=C,C=R` | L2 for server/high-risk; L1 local | X2–X4 | Same as parent | User/system control of important action |
| `Processing.JobControlRequested` | PROCESSING | User/Admin where explicitly permitted | Cancel/retry/resume | JOB | ACCEPTED/DENIED/FAILED | `F=C,J=R,C=R` | L2 conditional; L3 privileged | X2 | ONLINE for server Job | Important control request; not Worker internal retry |
| `Processing.ContentTransferAuthorized` | PROCESSING/SECURITY | User; origin recorded | Authorize content upload to server/internal provider | FEATURE_EXECUTION/JOB | SUCCEEDED/DENIED | `F=R,J=C,C=R` | L2; L3 if high classification/denied | X2/X4 | ONLINE | Explicit boundary crossing and consent/policy evidence |

For low-risk local Core tools, `History.FeatureCompleted` is normally sufficient. Do not duplicate it into Enterprise Audit merely because the Feature exists in the catalog.

## 5.4 Secured-file events

| EventName | Category | Actor / Origin | Action | Resource | Outcomes | IDs | Level | Sensitive exclusions | Offline | Audit-required reason |
|---|---|---|---|---|---|---|---:|---|---|---|
| `SecuredFile.Created` | SECURED_FILE | User | Amankan ke Akun | SECURED_FILE | SUCCEEDED/FAILED/DENIED | `F=FEAT-BACKUP-001,C=R` | L2 | X2; no storage key | ONLINE | Explicit server persistence/cross-device boundary |
| `SecuredFile.VersionCreated` | SECURED_FILE | User | Add validated version | SECURED_FILE_VERSION | SUCCEEDED/FAILED/DENIED | `F=C,C=R` | L2 | X2 | ONLINE | Versioned protected content lifecycle |
| `SecuredFile.VersionRotated` | SECURED_FILE | SYSTEM/Service | Tombstone oldest active version | SECURED_FILE_VERSION | SUCCEEDED/FAILED | `F=C,C=R` | L2 | X2 | ONLINE | Maximum-three invariant evidence |
| `SecuredFile.Downloaded` | SECURED_FILE | User | Download exact authorized version | SECURED_FILE_VERSION | SUCCEEDED/FAILED/DENIED | `F=C,C=R` | L2 conditional; L3 cross-user/privileged attempt | X2; no URL/key | ONLINE | Content disclosure; policy may require every download |
| `SecuredFile.MovedToRecycle` | SECURED_FILE | User | Recycle secured resource | SECURED_FILE | SUCCEEDED/FAILED/DENIED | `F=C,C=R` | L2 | X2 | ONLINE | Recoverable lifecycle mutation |
| `SecuredFile.Restored` | SECURED_FILE | User | Restore file/version | SECURED_FILE/Version | SUCCEEDED/FAILED/DENIED | `F=C,C=R` | L2 | X2 | ONLINE | Re-enables access/version state |
| `SecuredFile.PermanentDeletionRequested` | SECURED_FILE | User/Admin only if separately authorized | Request permanent deletion | SECURED_FILE | ACCEPTED/DENIED/FAILED | `F=C,C=R` | L3 | X2; no reauth details | ONLINE | Irreversible sensitive action |
| `SecuredFile.PermanentlyDeleted` | SECURED_FILE | User as initiator; Lifecycle Service executor | Complete tombstone/purge access removal | SECURED_FILE | SUCCEEDED/FAILED | `F=C,C=R` | L3 | X2 | ONLINE | Proof of irreversible lifecycle outcome; no secure-erase claim |

Ordinary local Save As or deletion of a temporary/cache file is not a `SECURED_FILE` event.

---

# 6. 33 Core Tool audit mapping

## 6.1 Metadata profiles

| Profile | Minimum metadata |
|---|---|
| `C1-HISTORY` | ActorPrincipalId, Origin, actual FeatureId, OccurredAtUtc, Platform/AppVersion, ExecutionLocation=LOCAL, Outcome, input/output count, safe size category; no content/path/name |
| `C2-SERVER` | `C1` plus ProcessId, JobId, CorrelationId, server/provider route code, admission/terminal time, safe ErrorCode, consent/policy outcome code |
| `C3-HIGH-RISK` | `C1` plus CorrelationId, confirmation/reauth outcome code when applicable, security-operation type, validation outcome, policy version; no password/signature/redacted data |
| `C4-AI` | `C2` or local equivalent plus AI operation ID, approved provider-profile code or `LOCAL`, data-minimization/consent/classification decision codes, selected-page/count only when approved; no prompt/content/response |

## 6.2 Mapping

| # | Primary Tool | Canonical Feature ID | Audit Required? | Event Type | Risk | Minimum metadata | Outcomes | Reason |
|---:|---|---|---|---|---|---|---|---|
| 1 | Merge PDF | `FEAT-PDF-005` | **YES — L1 safe process metadata; no Enterprise by default** | `History.FeatureCompleted` | Low | `C1-HISTORY` | Succeeded/Failed/Cancelled | Satisfies Core Audit trace with minimal local History; Enterprise Audit adds little value |
| 2 | Split PDF | `FEAT-PDF-006 / FEAT-PDF-007` | **YES — L1 safe process metadata; no Enterprise by default** | `History.FeatureCompleted` | Low | `C1-HISTORY`; actual Feature ID | Succeeded/Failed/Cancelled | Ordinary local transform |
| 3 | Remove pages | `FEAT-PDF-013` | **YES — L1 safe process metadata; no Enterprise by default** | `History.FeatureCompleted` | Low | `C1-HISTORY` | Succeeded/Failed/Cancelled | Reversible working-state operation; source preserved |
| 4 | Extract pages | `FEAT-PDF-014` | **YES — L1 safe process metadata; no Enterprise by default** | `History.FeatureCompleted` | Low | `C1-HISTORY`; selected-page count only | Succeeded/Failed/Cancelled | Ordinary local derived result; no page content |
| 5 | Organize PDF | `FEAT-PDF-010..016` | **YES — L1 safe process metadata; no Enterprise by default** | `History.FeatureCompleted` per actual material Feature or workflow terminal | Low | `C1-HISTORY`; actual IDs/step count | Succeeded/Failed/Cancelled | Bundle must not emit per-click/page-move Audit noise |
| 6 | Scan to PDF | `FEAT-SCAN-014` | **YES — L1 safe process metadata; no Enterprise by default** | `History.FeatureCompleted`; `Security.FileRejected` if security-relevant | Low | `C1-HISTORY`; channel/device capability code, page count | Succeeded/Failed/Cancelled | Local capture; never audit scanned image content |
| 7 | Compress PDF | `FEAT-PDF-023` | **YES — L1 safe process metadata; no Enterprise by default** | `History.FeatureCompleted` | Low | `C1-HISTORY`; safe before/after size category | Succeeded/Failed/Cancelled | Ordinary local result |
| 8 | Repair PDF | `FEAT-PDF-025 / FEAT-PDF-026` | **YES — L1 local; elevate L2 for optional server fallback** | History or `Processing.ExecutionAccepted/Completed/Failed` | Low catalog | `C1` local; `C2` server | Accepted/Succeeded/Failed/Cancelled | Server transfer/advanced fallback is Enterprise-auditable; local repair keeps minimal History |
| 9 | OCR PDF | `FEAT-OCR-004` | **YES — L1 local; elevate L2 for server/policy/high classification** | History or Processing event | Medium | `C1` local; `C2` server; page count only | Accepted/Succeeded/Failed/Cancelled/Denied | OCR text is restricted but never an Audit payload |
| 10 | JPG to PDF | `FEAT-CONV-007` | **YES — L1 safe process metadata; no Enterprise by default** | `History.FeatureCompleted` | Low | `C1-HISTORY`; image count | Succeeded/Failed/Cancelled | Ordinary local conversion |
| 11 | Word to PDF | `FEAT-CONV-001` | **YES — L1 local; elevate L2 for server path** | History or Processing event | Low | `C1`/`C2` | Accepted/Succeeded/Failed/Cancelled | Optional server processing crosses trust boundary |
| 12 | PowerPoint to PDF | `FEAT-CONV-003` | **YES — L1 local; elevate L2 for server path** | History or Processing event | Low | `C1`/`C2` | Accepted/Succeeded/Failed/Cancelled | Same boundary rationale as Word conversion |
| 13 | Excel to PDF | `FEAT-CONV-002` | **YES — L1 local; elevate L2 for server path** | History or Processing event | Low | `C1`/`C2`; sheet count only | Accepted/Succeeded/Failed/Cancelled | No formula/cell content in Audit |
| 14 | HTML to PDF | `FEAT-CONV-017` | **YES — L1 local approved artifact; elevate L2 server/L3 security block** | History/Processing/Security | Medium security | `C1`/`C2`; security reason code only | Accepted/Succeeded/Failed/Denied | Renderer/remote-fetch risk; no HTML/URL content |
| 15 | PDF to JPG | `FEAT-CONV-012` | **YES — L1 safe process metadata; no Enterprise by default** | `History.FeatureCompleted` | Low | `C1-HISTORY`; page/output count | Succeeded/Failed/Cancelled | Ordinary local render |
| 16 | PDF to Word | `FEAT-CONV-004` | **YES — L1 local; elevate L2 for server path** | History or Processing event | Low | `C1`/`C2` | Accepted/Succeeded/Failed/Cancelled | Extracted content excluded |
| 17 | PDF to PowerPoint | `FEAT-CONV-006` | **YES — L1 local; elevate L2 for server path** | History or Processing event | Low | `C1`/`C2` | Accepted/Succeeded/Failed/Cancelled | Optional server boundary |
| 18 | PDF to Excel | `FEAT-CONV-005` | **YES — L1 local; elevate L2 for server/policy route** | History or Processing event | Low | `C1`/`C2`; output count only | Accepted/Succeeded/Failed/Cancelled | Structured data excluded |
| 19 | PDF to PDF/A | `FEAT-PDF-075` | **YES — L1 local; elevate L2 for server/regulatory policy** | History or Processing event | Medium | `C1`/`C2`; conformance profile/outcome code | Accepted/Succeeded/Failed/Cancelled | Central/regulatory conformance may require Enterprise evidence |
| 20 | Rotate PDF | `FEAT-PDF-012` | **YES — L1 safe process metadata; no Enterprise by default** | `History.FeatureCompleted` | Low | `C1-HISTORY`; page count only | Succeeded/Failed/Cancelled | Ordinary local edit |
| 21 | Add page numbers | `FEAT-PDF-037` | **YES — L1 safe process metadata; no Enterprise by default** | `History.FeatureCompleted` | Low | `C1-HISTORY` | Succeeded/Failed/Cancelled | Ordinary local edit |
| 22 | Add watermark | `FEAT-PDF-036` | **YES — L1 safe process metadata; policy may elevate** | History; High-risk/Processing only if classification requires | Low | `C1`; watermark type code only, no asset/text | Succeeded/Failed/Cancelled/Denied | Watermark payload/content excluded |
| 23 | Crop PDF | `FEAT-PDF-019` | **YES — L1 safe process metadata; no Enterprise by default** | `History.FeatureCompleted` | Low | `C1-HISTORY` | Succeeded/Failed/Cancelled | Source preserved; no crop coordinates needed |
| 24 | Edit PDF | `FEAT-PDF-031..041` | **YES — L1 normal; elevate L2 server/specific sensitive action** | History/Processing/actual sensitive event | Medium workflow | `C1`/`C2`; actual Feature ID(s) | Accepted/Succeeded/Failed/Cancelled | Do not audit every edit command or text |
| 25 | PDF Forms | `FEAT-PDF-053..060` | **YES — L1 safe process metadata; policy may elevate** | `History.FeatureCompleted` | Low | `C1-HISTORY`; actual Feature ID; field count only | Succeeded/Failed/Cancelled | Never store form values; flatten event only if policy elevates |
| 26 | Unlock PDF | `FEAT-PDF-062` | **YES — L3** | `HighRiskFeature.UnlockFinalized` / failure/denial | High | `C3-HIGH-RISK`; protection state code, no password | Succeeded/Failed/Denied/Cancelled | Removes protection; valid authorization required |
| 27 | Protect PDF | `FEAT-PDF-061` | **YES — L3** | `HighRiskFeature.ProtectFinalized` | Medium | `C3-HIGH-RISK`; security profile code only | Succeeded/Failed/Denied/Cancelled | Creates protected result; password never recorded |
| 28 | Sign PDF | `FEAT-SIGN-013` | **YES — L3** | `HighRiskFeature.VisualSignFinalized` | Medium | `C3-HIGH-RISK`; `SignatureKind=VISUAL_NON_PKI`; placement count only | Succeeded/Failed/Denied/Cancelled | Sensitive visual asset; event must not claim PKI signature |
| 29 | Redact PDF | `FEAT-PDF-069` | **YES — L3** | `HighRiskFeature.RedactionFinalized` / `RedactionValidationFailed` | High | `C3-HIGH-RISK`; target count/validation code only | Succeeded/Failed/Denied/Cancelled | Irreversible removal assurance; never redacted content/regions |
| 30 | Compare PDF | `FEAT-PDF-071 / FEAT-PDF-072` | **YES — L1 safe process metadata; no Enterprise by default** | `History.FeatureCompleted` | Low | `C1-HISTORY`; input count | Succeeded/Failed/Cancelled | Ordinary local analysis; no comparison content |
| 31 | AI Summarizer | `FEAT-INTEL-001` | **YES — L2 logical execution; L3 on policy/security denial** | `AI.IntelligenceExecutionCompleted`; provider events when applicable | Medium | `C4-AI` | Accepted/Succeeded/Failed/Denied/Cancelled | AI governance, minimization, approved provider traceability |
| 32 | Translate PDF | `FEAT-INTEL-002` | **YES — L2 logical execution; L3 on policy/security denial** | `AI.IntelligenceExecutionCompleted`; provider events when applicable | Medium | `C4-AI`; language code only | Accepted/Succeeded/Failed/Denied/Cancelled | Approved provider and content-transfer accountability |
| 33 | PDF to Markdown | `FEAT-CONV-036` | **YES — L1 deterministic local; elevate L2 for server/OCR/provider path** | History or Processing/AI-provider event | Low catalog | `C1` local; `C2/C4` non-local | Accepted/Succeeded/Failed/Denied/Cancelled | Deterministic local extraction is ordinary; content never audited |

## 6.3 Rules for all Core cards

- Document 09 `Audit=YES` is satisfied for all 33 cards by safe process metadata at the appropriate evidence plane; it is not a mandate to centralize every low-risk local completion in Enterprise Audit.
- Record the actual canonical Feature ID, not only the card label.
- A bundle/range does not justify one Audit event per UI step.
- Local low-risk execution is Personal History unless policy elevates it.
- Optional server execution produces `L2` admission and terminal evidence.
- High-risk Features produce `L3` terminal evidence even when local; protected offline batching may be used.
- Assistant origin changes `Origin`, not `ActorPrincipalId` or Audit requirement.
- Save As overwrite, share, upload, Amankan ke Akun, export, and permanent deletion are independently audited actions; processing permission does not imply them.

---

# 7. Privileged event catalog

## 7.1 Administration, policy, and configuration

| EventName | Category | Actor / Origin | Action | Resource | Outcomes | IDs | Level | Exclusions | Offline | Reason |
|---|---|---|---|---|---|---|---:|---|---|---|
| `Admin.AccountEligibilityChanged` | ADMIN | Administrator | Enable/disable account | ACCOUNT | SUCCEEDED/FAILED/DENIED | `C=R` | L3 | X1 | ONLINE | Access eligibility change |
| `Admin.RoleAssigned` | ADMIN | Administrator | Assign formal role | ROLE_ASSIGNMENT | SUCCEEDED/FAILED/DENIED | `C=R` | L3 | X1 | ONLINE | Vertical privilege change |
| `Admin.RoleRevoked` | ADMIN | Administrator | Revoke formal role | ROLE_ASSIGNMENT | SUCCEEDED/FAILED/DENIED | `C=R` | L3 | X1 | ONLINE | Privilege removal and traceability |
| `Admin.PermissionAssigned` | ADMIN | Administrator | Assign approved permission/scope | PERMISSION_ASSIGNMENT | SUCCEEDED/FAILED/DENIED | `C=R` | L3 | X1; no full permission dump | ONLINE | Fine-grained privilege change |
| `Admin.PermissionRevoked` | ADMIN | Administrator | Revoke permission/scope | PERMISSION_ASSIGNMENT | SUCCEEDED/FAILED/DENIED | `C=R` | L3 | X1 | ONLINE | Privilege removal |
| `Admin.SupportGrantChanged` | ADMIN | Administrator/authorized mapping service | Assign/revoke Support profile permission | PERMISSION_ASSIGNMENT | SUCCEEDED/FAILED/DENIED | `C=R` | L3 | X1/X5 | ONLINE | Superadmin is granular Support permission, not role |
| `Policy.SecurityPolicyChanged` | POLICY | Administrator | Change security/reauth policy | POLICY | SUCCEEDED/FAILED/DENIED | `C=R` | L3 | X1; safe changed-field codes only | ONLINE | Systemic security impact |
| `Policy.ClassificationChanged` | POLICY | Administrator | Change classification handling | POLICY | SUCCEEDED/FAILED/DENIED | `C=R` | L3 | X2; no content | ONLINE | Disclosure/processing impact |
| `Policy.RetentionChanged` | POLICY | Administrator/compliance authority | Change retention policy | POLICY | SUCCEEDED/FAILED/DENIED | `C=R` | L3 | No full rule payload | ONLINE | Evidence/data lifecycle impact; duration not set here |
| `Configuration.FeatureExposureChanged` | CONFIGURATION | Administrator | Enable/disable exposure/flag | FEATURE_CONFIGURATION | SUCCEEDED/FAILED/DENIED | `F=R,C=R` | L2; L3 broad/security impact | No handler/provider secret | ONLINE | Product availability and policy impact |
| `Configuration.LimitChanged` | CONFIGURATION | Administrator | Change typed limit/quota | LIMIT_CONFIGURATION | SUCCEEDED/FAILED/DENIED | `F=C,C=R` | L2; L3 broad/high impact | Safe old/new categories, not topology | ONLINE | Capacity/security impact |
| `Configuration.MaintenanceModeChanged` | CONFIGURATION | Administrator | Enter/exit maintenance | POLICY/configuration | SUCCEEDED/FAILED/DENIED | `C=R` | L2 | No internal topology | ONLINE | Availability/business impact |
| `Configuration.MinimumClientVersionChanged` | CONFIGURATION | Administrator | Change compatibility minimum | FEATURE_CONFIGURATION | SUCCEEDED/FAILED/DENIED | `C=R` | L2–L3 | No deployment secret | ONLINE | Access/compatibility impact |

Store safe before/after version and changed-field codes, not unrestricted JSON or secrets.

## 7.2 Audit access

| EventName | Category | Actor / Origin | Action | Resource | Outcomes | IDs | Level | Exclusions | Offline | Reason |
|---|---|---|---|---|---|---|---:|---|---|---|
| `AuditAccess.QueryExecuted` | AUDIT_ACCESS | Auditor/explicitly permitted principal | Execute scoped first-page query | AUDIT_QUERY | SUCCEEDED/FAILED/DENIED | `C=R` | L3 | No result payload; safe scope/filter fingerprint only | ONLINE | Reading controlled evidence is privileged |
| `AuditAccess.EventViewed` | AUDIT_ACCESS | Auditor | View one event detail | AUDIT_QUERY/event reference | SUCCEEDED/FAILED/DENIED | `F=C,J=C,C=R` | L3 conditional | Do not duplicate target event SafeMetadata | ONLINE | Detail access may expose confidential metadata |
| `AuditAccess.ExportRequested` | AUDIT_ACCESS | Auditor | Request bounded export | AUDIT_EXPORT | ACCEPTED/DENIED/FAILED | `J=C,C=R` | L3 | No export bytes; safe filter/scope fingerprint | ONLINE | Bulk-disclosure request |
| `AuditAccess.ExportCompleted` | AUDIT_ACCESS | Auditor as initiator; Service executor | Complete export | AUDIT_EXPORT | SUCCEEDED/FAILED/CANCELLED | `J=R,C=R` | L3 | No export content | ONLINE | Durable bulk artifact outcome |
| `AuditAccess.ExportDownloaded` | AUDIT_ACCESS | Auditor | Download authorized export | AUDIT_EXPORT | SUCCEEDED/FAILED/DENIED | `J=C,C=R` | L3 | No delegated URL/content | ONLINE | Evidence disclosure event |
| `AuditAccess.AccessDenied` | AUDIT_ACCESS/AUTHORIZATION | Human/service | Deny query/detail/export | AUDIT_QUERY/EXPORT | DENIED | `J=C,C=R` | L3 | No existence/scope leak | ONLINE | Privileged evidence-access denial |

Cursor continuation with the same query scope/correlation should update observability counters rather than generate recursive Enterprise Audit on every page. A changed query/filter/scope is a new logical query event.

## 7.3 Security events

| EventName | Category | Actor / Origin | Action | Resource | Outcomes | IDs | Level | Exclusions | Offline | Reason |
|---|---|---|---|---|---|---|---:|---|---|---|
| `Security.TokenReplayDetected` | SECURITY | SYSTEM/Identity service; attempted subject if known | Detect token/refresh replay | SESSION/ACCOUNT | DENIED/REVOKED | `C=C` | L3 | X1 | ONLINE | Account/session compromise indicator |
| `Security.ResourceProbeDetected` | SECURITY | Human/service | Detect repeated cross-owner/object enumeration | SECURITY_FINDING | DENIED | `F=C,J=C,C=C` | L3 aggregated | X2/X5 | Online/local batch as applicable | IDOR/abuse detection |
| `Security.FileRejected` | SECURITY | User as initiator; Validator/Worker executor | Reject unsafe/inconclusive file | SECURITY_FINDING/FEATURE_EXECUTION | DENIED/FAILED | `F=R,J=C,C=R` | L3 only when security-relevant | X2; safe reason code | May OFFLINE-BATCH | File security evidence; ordinary format error stays telemetry |
| `Security.MalwareDetected` | SECURITY | Validator/Worker | Detect malware/active threat | SECURITY_FINDING/JOB | DENIED/FAILED | `F=C,J=C,C=R` | L3 | X2; no sample bytes/signature details beyond approved code | Local batch/server | Material threat event |
| `Security.PathOrIpcViolationDetected` | SECURITY | Worker/System | Block path escape, rogue IPC, invalid operation | SECURITY_FINDING | DENIED/FAILED | `F=C,J=C,C=R` | L3 | X2; no raw path/message | Local batch/server | Containment boundary attack |
| `Security.PublicAiRouteBlocked` | SECURITY/AI | User as initiator; Origin UI/Assistant | Block public/unapproved provider | AI_OPERATION | DENIED | `F=R,J=C,C=R` | L3 | X4; no URL/provider secret | Local/server batch | Locked public-AI prohibition |
| `Security.WorkloadScopeDenied` | SECURITY | SERVICE/WORKER | Deny wrong audience/job/object/lease | WORKLOAD_OPERATION | DENIED | `F=C,J=R,C=R` | L3 | X1/X2 | ONLINE | Service/Worker privilege boundary |
| `Security.OutputValidationFailed` | SECURITY | Worker/Validator | Reject unsafe invalid output | FEATURE_EXECUTION/JOB | FAILED | `F=R,J=C,C=R` | L3 when security property failed | X2–X4 | Local batch/server | Prevent false success/corrupt result |
| `Security.AbuseThresholdExceeded` | SECURITY | SYSTEM | Detect material rate/flood/credential abuse | SECURITY_FINDING | DENIED | `F=C,J=C,C=C` | L3 aggregated | X1/X2; safe counts only | ONLINE | DoS/brute force/security response |

## 7.4 High-risk Feature events

| EventName | Category | Actor / Origin | Action | Resource | Outcomes | IDs | Level | Exclusions | Offline | Reason |
|---|---|---|---|---|---|---|---:|---|---|---|
| `HighRiskFeature.ProtectFinalized` | HIGH_RISK_FEATURE | User; Assistant origin allowed | Finalize protected PDF result | FEATURE_EXECUTION | SUCCEEDED/FAILED/DENIED/CANCELLED | `F=FEAT-PDF-061,C=R` | L3 | X1–X3; no password/policy secret | OFFLINE-BATCH or server | Security protection action |
| `HighRiskFeature.UnlockFinalized` | HIGH_RISK_FEATURE | User; Assistant origin allowed only through same controls | Finalize unprotected result | FEATURE_EXECUTION | SUCCEEDED/FAILED/DENIED/CANCELLED | `F=FEAT-PDF-062,C=R` | L3 | X1–X3; never password | OFFLINE-BATCH or server | Protection removal; no bypass |
| `HighRiskFeature.VisualSignFinalized` | HIGH_RISK_FEATURE | User; Origin recorded | Finalize visual non-PKI signature | FEATURE_EXECUTION | SUCCEEDED/FAILED/DENIED/CANCELLED | `F=FEAT-SIGN-013,C=R` | L3 | X2/X3; no asset/coordinates | OFFLINE-BATCH or server | Sensitive visual action; non-PKI disclaimer |
| `HighRiskFeature.RedactionFinalized` | HIGH_RISK_FEATURE | User; Origin recorded | Finalize validated irreversible redaction | FEATURE_EXECUTION | SUCCEEDED/FAILED/DENIED/CANCELLED | `F=FEAT-PDF-069,C=R` | L3 | X2/X3; no removed content/regions | OFFLINE-BATCH or server | Redaction assurance evidence |
| `HighRiskFeature.RedactionValidationFailed` | HIGH_RISK_FEATURE/SECURITY | User as initiator; Validator executor | Reject unsafe redaction output | FEATURE_EXECUTION | FAILED | `F=FEAT-PDF-069,C=R` | L3 | X2/X3 | OFFLINE-BATCH or server | Prevent false assurance |

Do not emit separate events for signature drawing/move/resize, redaction rectangle changes, password keystrokes, preview refresh, or every failed password typo. Repeated Unlock failures may be aggregated into a security-abuse event.

---

# 8. Support event catalog

| EventName | Category | Actor / Origin | Action | Resource | Outcomes | IDs | Level | Exclusions | Offline | Audit-required reason |
|---|---|---|---|---|---|---|---:|---|---|---|
| `Support.IssueSubmitted` | SUPPORT | User | Submit Issue message/safe context | ISSUE | SUCCEEDED/FAILED | `F=C,J=C,C=R` | L2 | X5; no message text | Request may queue locally only as draft; authoritative event online | Business support record creation |
| `Support.AttachmentSubmitted` | SUPPORT | User | Explicitly send previewed attachment | ISSUE_ATTACHMENT | SUCCEEDED/FAILED/DENIED | `F=C,J=C,C=R` | L2 | X5; attachment bytes excluded | ONLINE | Explicit content transfer evidence |
| `Support.IssueViewed` | SUPPORT | Support | View Issue detail/message/safe diagnostics | ISSUE | SUCCEEDED/FAILED/DENIED | `F=C,J=C,C=R` | L3 | X5; Audit records access, not message | ONLINE | Privileged Support access |
| `Support.AttachmentViewed` | SUPPORT | Support | Preview/download explicit attachment | ISSUE_ATTACHMENT | SUCCEEDED/FAILED/DENIED | `F=C,J=C,C=R` | L3 | X5; no delegated URL | ONLINE | Sensitive attachment disclosure |
| `Support.IssueTriaged` | SUPPORT | Support | Set category/priority/duplicate/not-bug/need-info | ISSUE | SUCCEEDED/FAILED/DENIED | `F=C,J=C,C=R` | L2 | X5; safe old/new state codes only | ONLINE | Business workflow accountability |
| `Support.IssueStatusChanged` | SUPPORT | Support | Change Issue status | ISSUE | SUCCEEDED/FAILED/DENIED | `F=C,J=C,C=R` | L2 | X5; state codes only | ONLINE | User-visible/business status change |
| `Support.InternalNoteAdded` | SUPPORT | Support | Add restricted internal note | ISSUE | SUCCEEDED/FAILED/DENIED | `F=C,J=C,C=R` | L2 | X5; note text excluded | ONLINE | Proves note action without duplicating note |
| `Support.ExportRequested` | SUPPORT | Support | Request bounded PDF/Excel export | AUDIT_EXPORT/Issue export | ACCEPTED/DENIED/FAILED | `F=FEAT-SUPPORT-007/008,J=C,C=R` | L3 | X5; no report content/filter excess | ONLINE | Bulk disclosure and downstream handoff |
| `Support.ExportCompleted` | SUPPORT | Support initiator; Service executor | Complete export | Issue export | SUCCEEDED/FAILED/CANCELLED | `F=R,J=R,C=R` | L3 | X5 | ONLINE | Export artifact outcome |
| `Support.ExportDownloaded` | SUPPORT | Support | Retrieve export | Issue export | SUCCEEDED/FAILED/DENIED | `F=R,J=C,C=R` | L3 | X5; no URL/content | ONLINE | Bulk disclosure event |
| `Support.AccessDenied` | SUPPORT/AUTHORIZATION | Human | Deny list/detail/attachment/triage/note/export | ISSUE/ATTACHMENT/export | DENIED | `F=C,J=C,C=R` | L3 when privileged/abusive | X5 | ONLINE | Support boundary enforcement |

Do not Audit every time a reporter views the reporter's own simplified Issue status. That is normal resource access and may be observability only. Support privileged views and exports remain auditable.

---

# 9. Assistant/AI events

## 9.1 Assistant attribution rule

Assistant does not create a parallel event for every ordinary Feature. Use the underlying Feature/business event with:

```text
ActorPrincipalId = initiating User
ActorType        = USER
Origin           = ASSISTANT
ExecutorType     = LOCAL_WORKER / SERVER_WORKER / SERVICE when applicable
FeatureId        = actual canonical Feature
WorkflowId       = safe opaque workflow execution reference when applicable
```

This preserves actor truth and prevents duplicate Audit events such as both `AssistantMerge` and `MergeCompleted` for the same logical action.

## 9.2 Assistant-specific events

| EventName | Category | Actor / Origin | Action | Resource | Outcomes | IDs | Level | Exclusions | Offline | Reason |
|---|---|---|---|---|---|---|---:|---|---|---|
| `Assistant.HighRiskStepConfirmed` | ASSISTANT | User / ASSISTANT | Confirm exact high-risk workflow step | ASSISTANT_WORKFLOW + target | SUCCEEDED/CANCELLED | `F=R,J=C,C=R` | L2–L3; normally linked to target event | X2–X4 | Protected local batch or online | Proves explicit intent without prompt text |
| `Assistant.AuditableFeatureDelegated` | ASSISTANT | User / ASSISTANT | Delegate an underlying L2/L3 Feature step | FEATURE_EXECUTION | ACCEPTED/DENIED | `F=R,J=C,C=R` | Same as underlying action; avoid duplicate if fields fit target event | X2–X4 | Same as underlying | Trace Assistant origin when necessary |
| `Assistant.PrivilegedActionBlocked` | ASSISTANT/SECURITY | User / ASSISTANT | Block Admin/Support/Audit or unauthorized tool request | ASSISTANT_WORKFLOW | DENIED | `F=C,J=C,C=R` | L3 | X2–X4 | Local batch/server | Prompt-based authorization bypass evidence |
| `Assistant.WorkflowValidationFailed` | ASSISTANT/SECURITY | User / ASSISTANT | Reject unknown Feature/schema/cycle/unsafe step | ASSISTANT_WORKFLOW | FAILED/DENIED | `F=C,J=C,C=R` | L3 only when security-relevant; otherwise telemetry | X2–X4 | Local batch/server | Closed orchestrator boundary |

Ordinary suggestion display, clarification, chat turn, plan preview, text generation, and low-risk local Feature completion do not require Assistant-specific Enterprise Audit.

## 9.3 AI/provider events

| EventName | Category | Actor / Origin | Action | Resource | Outcomes | IDs | Level | Exclusions | Offline | Reason |
|---|---|---|---|---|---|---|---:|---|---|---|
| `AI.ProviderTransferAuthorized` | AI | User; Origin UI/Assistant | Authorize minimum content transfer to approved internal provider | AI_OPERATION | SUCCEEDED/DENIED | `F=FEAT-INTEL-001/002,J=C,C=R` | L2; L3 high classification/denial | X2/X4 | ONLINE | Policy/classification/permission/consent evidence |
| `AI.ProviderInvocationCompleted` | AI | User initiator; Service executor | Complete approved provider operation | AI_OPERATION/JOB | SUCCEEDED/FAILED/CANCELLED | `F=R,J=C,C=R` | L2 | X2/X4 | ONLINE | Approved provider use and outcome |
| `AI.LocalIntelligenceCompleted` | AI | User; Local Worker executor | Complete approved local intelligence | AI_OPERATION | SUCCEEDED/FAILED/CANCELLED | `F=R,C=R` | L2 | X2/X4 | OFFLINE-BATCH allowed | Governance trace without content/provider transfer |
| `AI.PolicyDenied` | AI/AUTHORIZATION | User; Origin recorded | Deny intelligence because policy/classification/consent/provider | AI_OPERATION | DENIED | `F=R,J=C,C=R` | L3 when material | X2/X4; safe reason code | Local/server | Proves policy enforcement |
| `Security.PublicAiRouteBlocked` | SECURITY/AI | User; Origin recorded | Block public/unapproved endpoint | AI_OPERATION | DENIED | `F=R,J=C,C=R` | L3 | X4; no raw URL/prompt | Local/server | Locked public-AI prohibition |

Provider-profile code identifies an approved configuration profile, not vendor secrets or endpoint URL. Never Audit prompt, selected text, summary, translation, Markdown, provider response, or raw model safety output.

---

# 10. Events not worth auditing

## 10.1 Default exclusions

| Activity/data | Enterprise Audit disposition | Alternative |
|---|---|---|
| Mouse click, hover, focus, scroll, keyboard navigation | Do not Audit | None or anonymous UX analytics only if separately approved |
| Every page/menu opened | Do not Audit | Aggregate product analytics/observability if approved; privileged Audit/Support detail view is an exception |
| Progress percent/current page/current item/ETA | Do not Audit | Job state/SignalR/observability |
| Queue poll, Worker heartbeat, lease renewal | Do not Audit | Observability/current job state |
| Every upload chunk/range/download range | Do not Audit | Transfer state/metrics; one logical transfer/download event |
| Cache create/evict, thumbnail/preview generation | Do not Audit | Local artifact lifecycle/metrics |
| Small autosave/draft snapshot | Do not Audit | Local recovery metadata |
| Ordinary undo/redo/reorder/selection | Do not Audit | Working state |
| Clipboard read/write content | Never Audit content | No Audit; explicit high-risk resulting action may be audited |
| Document/file/OCR/form/scan content | Never Audit content | Owning protected content store only |
| Password/token/cookie/secret/key | Never Audit | Approved credential/key boundary only |
| Visual signature drawing/move/resize/payload | Never Audit payload/intermediate actions | One final visual-sign event with safe count/type |
| Redaction text/coordinates/preview changes | Never Audit payload/intermediate actions | One finalization/validation event |
| Prompt/chat/Assistant suggestion/clarification | Do not Audit content or every turn | Minimal Assistant local state; underlying Feature event when auditable |
| Successful token refresh | Do not Audit each refresh | Session telemetry; replay/failure/revoke is auditable |
| Every successful authorization check | Do not Audit | No event; only important business action |
| Every ordinary 400/403/404 | Do not Audit | API metrics; important privilege/IDOR/abuse denial only |
| SignalR connect heartbeat/reconnect/notification delivery | Do not Audit normally | Realtime metrics; auth/revocation/security denial exceptions |
| Idempotent retry returning same result | Do not duplicate Audit | Reference original event; observability count |
| Temporary cleanup/orphan scan success | Do not Audit each object | Aggregate lifecycle metrics; material purge failure/security violation exception |
| Feature discovery/search/filter keystroke | Do not Audit | None/UX metrics |
| Language/theme/window/layout preference | No Enterprise Audit | Preference state; Admin enterprise policy change is separate |
| Personal History view/delete | No Enterprise Audit by default | History owns its lifecycle; deletion never touches Audit |
| Reporter views own Issue status | No Enterprise Audit by default | Domain access log/metrics if needed |

## 10.2 Anti-patterns

Do not create events such as:

```text
ButtonClicked
PageOpened
ProgressChanged
CacheDeleted
ThumbnailCreated
TokenValueRefreshed
DocumentTextExtracted
PasswordEntered
SignatureMoved
PromptSubmitted
SqlQueryExecuted
WorkerHeartbeat
SignalRMessageReceived
```

These either lack business accountability value, create excessive volume, expose sensitive data, or belong to observability/domain state.

---

# 11. Event metadata recommendation

## 11.1 Canonical envelope

Every event definition in the governed catalog shall document this design descriptor, whether or not an optional ID is present at runtime:

| Required catalog attribute | Rule |
|---|---|
| `EventName`, `Category`, `Actor`, `Action`, `Resource`, `Outcome` | Define the stable business/security fact and controlled outcome vocabulary |
| `FeatureId?`, `JobId?`, `CorrelationId?` | Mark each identifier Required, Conditional, or Not Applicable for that event |
| `AuditLevel` | Select `L1`, `L2`, or `L3`; do not infer level from HTTP route or UI card |
| `Sensitive exclusions` | Name prohibited content/secret classes explicitly |
| `Offline handling` | State `ONLINE`, `LOCAL_ONLY`, or protected `OFFLINE-BATCH` behavior |
| `AuditRequired reason` | State the accountability, security, boundary, or policy reason; “log everything” is invalid |

Recommended safe event envelope:

```text
AuditEventId                 globally unique, stable, idempotent
EventName                    stable taxonomy code
EventSchemaVersion           typed event schema version
Category                     AUTH/.../HIGH_RISK_FEATURE
AuditLevel                   L2 or L3 for Enterprise Audit

OccurredAtUtc                time action happened at source
ReceivedAtUtc                time server accepted event
IngestionBatchId             optional for offline batch
SourceEventSequence          optional per device/profile stream

ActorPrincipalId             opaque human actor when applicable
ActorType                    USER/ADMINISTRATOR/AUDITOR/SUPPORT/SYSTEM/SERVICE/WORKER
ActorRoleSnapshotCodes       approved minimal role/profile codes
ActorUnitSnapshotCode        optional approved code
Origin                       UI/FILE_FIRST/ASSISTANT/API/... 
ExecutorType                 NONE/SYSTEM/SERVICE/WORKER
ExecutorIdentityId           safe opaque workload identity code
OnBehalfOfPrincipalId        when non-human executor runs accepted human action

ActionCode                   stable business action
ResourceType                 ACCOUNT/JOB/SECURED_FILE/... 
ResourceId                   opaque ID
ParentResourceType/Id        optional relationship proof

FeatureId                    actual canonical Feature when applicable
ProcessId                    logical processing identity when applicable
JobId                        server/background job when applicable
CorrelationId                logical action/workflow correlation

Channel                      WEB/DESKTOP/MOBILE/TABLET/API/SYSTEM
PlatformCode                 approved platform code
AppVersion                   client/application version
EnvironmentCode              DEV/QA/PROD approved code
ExecutionLocation            LOCAL/LOCAL_WORKER/SERVER_WORKER/INTERNAL_PROVIDER

Outcome                      controlled outcome code
ReasonCode                   safe business/policy/security reason
ErrorCode                    safe normalized error code when applicable
StartedAtUtc/CompletedAtUtc   only when duration is business-relevant
InputCount/OutputCount        bounded safe aggregates
SizeCategoryOrBytes          approved metadata per policy

PermissionCode               relevant action permission for privileged event
PolicyVersion                relevant policy version
ResourceVersion              relevant ETag/row version where needed
SafeMetadataSchemaVersion    version for allowlisted optional metadata
SafeMetadataJson             bounded, typed, allowlisted, nullable
```

## 11.2 Required by question

| Business question | Field(s) |
|---|---|
| WHO? | `ActorPrincipalId`, `ActorType`, approved role/unit snapshot |
| WHAT? | `EventName`, `ActionCode`, `Category` |
| WHEN? | `OccurredAtUtc`, `ReceivedAtUtc`, optional start/completion |
| WHERE/CHANNEL? | `Channel`, `PlatformCode`, `AppVersion`, `EnvironmentCode`, `ExecutionLocation`, `Origin` |
| WHICH RESOURCE? | `ResourceType`, `ResourceId`, optional parent reference |
| WHY/RESULT? | `ReasonCode`, policy/permission version, safe ErrorCode |
| SUCCESS OR FAILED? | `Outcome` |
| WHICH FEATURE? | `FeatureId` |
| WHICH JOB? | `JobId` |
| WHICH CORRELATION? | `CorrelationId` |

“Where” means approved application channel/environment/execution location, not precise physical location, raw IP, local path, storage URL, or device fingerprint. IP/network evidence, if needed for security operations, belongs in a separately protected security/observability policy.

## 11.3 Minimum per-event rules

- `EventName`, version, category, level, occurred/received time, actor type, action, resource type, outcome, channel/environment, and reason are always required for Enterprise Audit.
- `ActorPrincipalId` is required for human actions.
- `FeatureId` is required for Feature execution and Feature configuration.
- `JobId` is required for persisted server Job actions and export jobs.
- `CorrelationId` is required for privileged mutations, server processing, Support, Audit access/export, AI/provider, secured-file mutation, and high-risk Feature finalization.
- `ProcessId` preserves Requirements compatibility; it may equal a distinct logical execution ID and must not be confused with OS Process ID.
- `SafeMetadataJson` is not a dumping ground. Unknown fields fail validation; size is bounded; content/credential scanning applies.

## 11.4 Offline ingestion

```text
source creates stable AuditEventId
→ validate against local allowlisted schema
→ protect per-profile offline outbox
→ never block UI on network
→ reconnect authenticates current principal/session/client policy
→ submit bounded idempotent batch
→ server validates event type/version/actor binding/time bounds/size/content exclusions
→ server stores OccurredAtUtc and ReceivedAtUtc
→ duplicate EventId is acknowledged without duplicate evidence
→ rejected events remain diagnosable through safe error code/retry policy
```

Rules:

- Privileged Admin/Support/Auditor/server actions are online-only and originate from server authority, not an offline client assertion.
- Eligible local high-risk Feature events may queue offline because the action is local, but the event must carry the actual user/profile binding and protected integrity metadata.
- Offline client cannot claim Administrator/Support/Auditor role or fabricate server success.
- Large clock skew, rollback, future time, unknown schema, actor mismatch, or prohibited fields cause reject/quarantine, not silent acceptance.
- Legal retention clock (`OccurredAtUtc` versus `ReceivedAtUtc`) remains an approved Compliance decision; this report does not set it.

## 11.5 Integrity and access principles

- `audit.AuditEvents` is append-oriented; ordinary application/user update/delete is prohibited.
- Corrections use a new linked correction/supersession event, not in-place rewrite.
- Audit export is asynchronous, bounded, scoped, access-controlled, expiring, and itself audited.
- Query authorization occurs before count/filter/cursor/export.
- History deletion, Issue deletion/view, or resource purge cannot cascade into Audit.
- Audit readers do not gain the referenced resource's content permission.
- Exact cryptographic integrity, archival, WORM/immutability, legal hold, purge approval, and retention duration require Pertamina confirmation.

---

# 12. Traceability matrix

## 12.1 Source-to-event traceability

| Business/requirement source | Feature/API/security source | Event coverage | Evidence answer |
|---|---|---|---|
| Document 01 §19; `FR-AUTH-001..005` | `FEAT-AUTH-001..008`; Document 08 §§8–10 | AUTH catalog | Login/logout/failure/revoke/reauth/account denial |
| `FR-ROLE-001..003` | `FEAT-ADMIN-001..003`; `ROLE-DEC-008/012/013` | ADMIN catalog | Who changed role/permission, target, outcome, version |
| Document 01 Administrator duties | `FEAT-ADMIN-004..015`; `/api/v1/admin/...` | POLICY/CONFIGURATION catalog | Feature/policy/limit/compatibility change |
| `FR-PROC-001..005`; 33 Core priority | Canonical Core Feature IDs; `/features/{featureId}/execute`, `/jobs` | PROCESSING + Core mapping | Actual Feature, local/server, job, outcome |
| `FR-BACKUP-001..010`, `FR-VER-001..004` | `FEAT-BACKUP-001..013`; `/secured-files` | SECURED_FILE catalog | Create/version/recycle/restore/purge/download |
| `FR-SUP-001..007`, `SR-SEC-019..020` | `FEAT-SUPPORT-001..008`; `/issues`, `/support/issues` | SUPPORT catalog | Explicit submit/access/triage/note/export |
| `FR-AUDIT-001..004` | `FEAT-AUDIT-001..008`; `/audit-events` | AUDIT_ACCESS + envelope/offline | Safe event/query/export, no content, batch sync |
| `SR-SEC-013..018`, `FR-ASSIST-001..015` | `FEAT-ASSIST-001..030`; `ROLE-DEC-015` | ASSISTANT catalog | User actor, Assistant origin, same permission/audit |
| `FR-INTEL-001..003`, `SR-SEC-021` | `FEAT-INTEL-001/002`, `FEAT-CONV-036` | AI catalog + Core rows 31–33 | Approved route/provider, consent/policy, outcome |
| `SR-SEC-022` | `FEAT-PDF-062` | HIGH_RISK Unlock | Protection removal outcome without password |
| `SR-SEC-023` | `FEAT-PDF-069` | HIGH_RISK Redact | Finalization/validation without content |
| `FR-SIGN-003`, `SR-SEC-011..012` | `FEAT-SIGN-013` | HIGH_RISK Visual Sign | Visual non-PKI finalization without asset |
| Document 08 security monitoring | Security/Worker/API boundaries | SECURITY catalog | Important control denial/finding, safe aggregation |
| `ROLE-DEC-009/010/016/017` | Resource/Worker/SignalR enforcement | AUTHORIZATION/SECURITY | Wrong scope/workload denial; no alternate path |

## 12.2 Event-to-owner traceability

| Category | Business owner/module | Authoritative state | Audit writer source |
|---|---|---|---|
| AUTH | Identity/Account Administration | `identity.Principals`, `identity.Sessions`, approved Identity | Identity/application service after result |
| AUTHORIZATION | Authorization/Security | Current permission/policy/resource | Policy/application service; aggregate security detection |
| ADMIN | Authorization Administration | `admin` assignments | Admin owner transaction/outbox |
| POLICY/CONFIGURATION | Platform Configuration | Typed policy/config row version | Configuration owner transaction/outbox |
| PROCESSING | Jobs/local application | Local result/History or persisted Job | Local outbox for required events; Jobs outbox server-side |
| SECURED_FILE | Secured Files | Secured aggregate/version/recycle state | Secured owner transaction/outbox |
| SUPPORT | Support | Issue/attachment/note/status/export | Support owner transaction/outbox/access service |
| AUDIT_ACCESS | Audit | Query/export request/access decision | Audit access service; separate from queried event payload |
| SECURITY | Security/owning boundary | Finding/denial/aggregated signal | Security control/validator/Worker/API |
| ASSISTANT/AI | Assistant/Intelligence plus underlying owner | Workflow/operation/Job result | Underlying application service; minimal Assistant/AI metadata |
| HIGH_RISK_FEATURE | Feature/application service | Validated final local/server result | Finalization boundary after validation; offline outbox if local |

## 12.3 API route traceability

| API family | Auditable logical action | Non-audit detail |
|---|---|---|
| Authentication/session | Login/logout/revoke/reauth/disabled denial | Token refresh success, cookie operations |
| `/features/{featureId}/execute` | Server acceptance/terminal, AI/high-risk/policy denial | Request parsing/progress/each step unless material |
| `/jobs/...` | User cancel/retry/resume and terminal when L2/L3 | Poll/status GET, SignalR progress |
| `/secured-files/...` | Create/version/download conditional/recycle/restore/permanent delete | Cursor page views by owner unless policy requires access audit |
| `/issues` | Submit and explicit attachment | Reporter own status view |
| `/support/issues` | Privileged read/attachment/triage/note/export | UI navigation/search keystrokes |
| `/audit-events` | Logical query/detail/export/download/denial | Every cursor continuation/row rendered |
| `/admin/...` | Account/assignment/policy/feature/limit/compatibility mutation | Ordinary safe read unless privileged-access policy requires it |
| `/hubs/activity` | Important auth/revocation/security denial only | Connection heartbeat, progress event delivery, reconnect |

---

# 13. Risks/conflicts

## 13.1 Source/version status

| ID | Finding | Impact | Required action |
|---|---|---|---|
| `AUD-RISK-001` | `MASTER_PROJECT_SUMMARY.md` v3.1 (24 Agustus 2026) still says “Documents 01–08 FINAL / Document 09 NEXT,” while `09_ROLE_MATRIX_FINAL_v1.0_PERTAMINA_OTO.md` is FINAL dated 26 Agustus 2026 | Project status/authority discovery can be misleading even though the newer final source is present | Update Master Summary through its governed process. This report has already been revalidated against Document 09 FINAL and does not treat the stale summary status as a blocker. |

## 13.2 Design risks and apparent conflicts

| ID | Risk/conflict | Analysis/recommendation |
|---|---|---|
| `AUD-RISK-002` | `FR-AUDIT-001`, `FEAT-AUDIT-001`, and Document 09 `document.content.process: Audit required=Yes` could be read as “put every Feature execution into full Enterprise Audit” | Preserve the explicit prompt rule against Audit-all: all Core executions produce safe process trace metadata, but ordinary low-risk local execution stays `L1` Personal History; server/high-risk/policy-selected actions produce `L2/L3` Enterprise Audit. Document 10 must state evidence-plane selection explicitly. |
| `AUD-RISK-003` | Document 09 FINAL marks all 33 Core cards `Audit=YES` but intentionally leaves detailed event fields, integrity, retention, alerting, and SIEM design to Document 10 | Define `YES` as mandatory safe process metadata at the applicable `L1/L2/L3` plane. Do not reinterpret it as document-content capture, per-click events, or identical central retention for all tools. |
| `AUD-RISK-004` | Personal History and Enterprise Audit may duplicate the same Feature completion | Use one domain/source event projected independently only when required; keep different retention/access semantics and no deletion cascade. |
| `AUD-RISK-005` | Audit-access events can recursively create more Audit-access events | Audit the logical first-page query/detail/export action with recursion guard; do not emit an event per returned row/cursor continuation. |
| `AUD-RISK-006` | Offline events may be duplicated, reordered, late, clock-shifted, or forged | Stable Event ID, source sequence, occurred/received time, protected outbox, actor binding, version/size/content validation, idempotent batch ingestion. |
| `AUD-RISK-007` | Role/unit snapshots may become personal/confidential metadata or be historically stale | Store approved codes/minimum snapshot only; stable ActorPrincipalId remains primary; do not use snapshot for current authorization. |
| `AUD-RISK-008` | Event `SafeMetadataJson` becomes a content/log dumping ground | Typed per-event allowlist, `additionalProperties=false`, strict size, secret/content negative tests, schema version. |
| `AUD-RISK-009` | Job/Correlation/resource IDs may be treated as access proof | IDs provide traceability only; every query/export/detail is reauthorized. |
| `AUD-RISK-010` | Support/Audit privileged views could create excessive volume | Record one logical detail/query/download action and aggregate repetitions while preserving accountability; do not record UI navigation. |
| `AUD-RISK-011` | High-risk local actions need evidence while offline | Use protected offline outbox with user/profile binding; never block safe local completion on network; sync after current session validation. |
| `AUD-RISK-012` | Unlock failures may reveal password attempts or create denial-of-service volume | Never record password; record terminal action outcome and aggregate repeated suspicious failures into Security event. |
| `AUD-RISK-013` | Visual Sign Audit may be mistaken for cryptographic proof | Store `SignatureKind=VISUAL_NON_PKI`; never store asset or claim identity/non-repudiation. |
| `AUD-RISK-014` | Redaction event may imply safety despite failed validation | Emit success only after negative extraction/structural validation; otherwise `RedactionValidationFailed`. |
| `AUD-RISK-015` | AI/provider event may leak prompt/content/response or provider secrets | Store Feature/provider-profile/policy/consent/outcome codes only; prohibit content/raw endpoint/model response. |
| `AUD-RISK-016` | Security denials can be used for account/resource enumeration | Safe reason codes, consistent concealment, restricted L3 access, aggregation; no target details beyond authorized evidence. |
| `AUD-RISK-017` | Exact retention/legal hold/purge/archival clock is not approved | Do not set durations. Obtain Pertamina Audit/Compliance decision for OccurredAt versus ReceivedAt, hold, archive, purge authority, and export retention. |
| `AUD-RISK-018` | Integrity/WORM/cryptographic evidence technology is unspecified | Set required properties and tests; vendor/product/topology remains governed confirmation. |
| `AUD-RISK-019` | Audit failure could block work or silently lose evidence | Define risk-tiered failure behavior: privileged server mutation should fail if required durable Audit cannot be committed; eligible local work uses protected outbox; low-risk telemetry failure must not masquerade as Audit success. |
| `AUD-RISK-020` | Event catalog drift from Features/API/permissions | Generate traceability checks from canonical Feature/API/permission artifacts and fail CI on unmapped protected action/event schema change. |

## 13.3 Required confirmations

```text
REQUIRES_PERTAMINA_STANDARD_CONFIRMATION

- official Audit event ownership and approval authority
- AuditLevel access classifications
- official event retention, legal hold, archive, purge, and correction policy
- whether OccurredAtUtc or ReceivedAtUtc controls each lifecycle
- cryptographic integrity/immutability/WORM and key requirements
- allowed role/unit/network/device metadata
- secured-file download and ordinary admin-read Audit threshold
- high-classification export/AI/Support reauthentication threshold
- SIEM forwarding, alert taxonomy, aggregation, and incident handoff
- offline outbox protection, batch size, clock tolerance, and rejection workflow
- audit availability/fail-closed rules by event class
```

---

# 14. Suggested Document 10 text

The following text is ready for controlled integration into Document 10 and has been reconciled with Document 09 FINAL v1.0.

## 14.1 Purpose and plane separation

PERTAMINA OTO shall audit important business, security, privilege, cross-boundary, and irreversible actions. It shall not implement Audit as a complete recording of UI or technical activity.

Evidence planes shall remain separate:

```text
Enterprise Audit → controlled append-oriented evidence
Personal History → user convenience projection
Observability    → technical health, performance, and security detection
Support          → explicit Issue message, attachment, note, and workflow
Domain state     → authoritative current resource state
```

Personal History may record ordinary low-risk local Feature outcomes and may be deleted by its owner. Deleting History shall never delete Enterprise Audit. Observability and Support shall not silently copy their payload into Audit.

## 14.2 Audit selection levels

The application shall classify evidence as:

- `L0` — not Audit evidence; optional bounded observability;
- `L1` — Personal History only;
- `L2` — Enterprise business evidence;
- `L3` — privileged/security evidence with restricted access and alert potential.

Enterprise Audit shall be required for authentication/session security, role/permission/account eligibility changes, security/classification/retention and material configuration changes, server/cross-boundary processing selected by policy, secured-file lifecycle, privileged Support/Audit access, approved AI/provider use, high-risk PDF security finalization, and material security denial/finding.

Ordinary low-risk local Feature completion shall use Personal History unless policy elevates it. The application shall emit bounded logical action events, not every progress update, page, chunk, Worker attempt, poll, retry, or SignalR notification.

## 14.3 Event taxonomy

The canonical event categories shall be:

```text
AUTH
AUTHORIZATION
ADMIN
POLICY
CONFIGURATION
PROCESSING
SECURED_FILE
SUPPORT
AUDIT_ACCESS
SECURITY
ASSISTANT
AI
HIGH_RISK_FEATURE
```

Event names shall use stable category-prefixed past-tense business/security facts such as `Auth.LoginSucceeded`, `Admin.RoleAssigned`, `Processing.ExecutionCompleted`, `SecuredFile.VersionCreated`, `AuditAccess.ExportDownloaded`, and `HighRiskFeature.RedactionFinalized`. Event semantics shall be versioned and shall not depend on localized UI labels or HTTP methods.

## 14.4 Actor and origin

Audit shall record the actual initiator and distinguish executor/origin. Human actor types are User, Administrator, Auditor, and Support. Non-human types are System, Service, and Worker.

Assistant shall have no independent actor permission. When Assistant invokes a Feature for a User:

```text
ActorPrincipalId = User
ActorType        = USER
Origin           = ASSISTANT
Executor         = applicable Service/Worker
FeatureId        = actual canonical Feature
```

A Worker completing an accepted User job shall not replace the User as business initiator. A scheduled operation with no human initiator shall use System/Service actor and shall identify its policy/job trigger.

## 14.5 Resource and content boundary

Auditable resources shall use opaque typed references such as Account, Session, Role Assignment, Permission Assignment, Policy, Feature Configuration, Job, Feature Execution, Secured File/Version, Issue/Attachment, Audit Query/Export, Assistant Workflow, AI Operation, Security Finding, and Workload Operation.

Resource ID, Feature ID, Job ID, Process ID, and Correlation ID provide traceability only and shall never grant access.

Audit shall never contain document/file content, filename/path by default, clipboard, screenshot/attachment bytes, PDF password, token/cookie/secret/key, visual-signature payload, redacted text/regions, full OCR/extracted/form/scan/translated/summary/Markdown content, prompt/chat/provider request/response, raw exception/SQL/storage key, or unrestricted URL.

## 14.6 Core Tool policy

All 33 Primary Tool Cards have `AuditRequired=YES` for safe process trace metadata under Document 09. Document 10 shall classify the evidence plane and detail as follows:

- low-risk local transforms — `L1 Personal History`, no Enterprise Audit by default;
- optional server route — `L1` when local and `L2` admission/terminal events when server/cross-boundary route is used;
- Protect PDF, Unlock PDF, visual Sign PDF, and Redact PDF — `L3` finalization/validation events with no password/signature/redacted content;
- AI Summarizer and Translate PDF — `L2` governed execution/provider evidence and `L3` policy/security denial, with no content;
- PDF to Markdown/OCR/HTML/Office and other hybrid cards — context-dependent `L1/L2`, with actual Feature ID and server route determining the level.

Bundled cards shall record actual canonical Feature IDs for materially auditable steps and shall not emit Audit for every page move, form field, edit command, or workflow preview.

## 14.7 Privileged, secured, Support, and Audit-access events

Role/permission assignment/revocation, account enable/disable, another user's session revocation, Support grant change, security/classification/retention policy change, and material Feature/limit/maintenance/compatibility change shall produce `L3` or risk-classified `L2/L3` events after the authoritative transaction outcome.

Amankan ke Akun, secured version creation/rotation, recycle, restore, policy-selected download, permanent deletion request, and permanent deletion outcome shall be audited without file content/path/storage key. Permanent deletion Audit shall prove logical access removal/tombstone outcome and shall not claim physical secure erase.

Support shall audit Issue submission and explicit attachment send at `L2`; privileged Support Issue/attachment access and export at `L3`; and triage/status/note action at `L2`. Audit shall reference the Issue/Attachment and action/outcome, not copy message, note, screenshot, or attachment content.

Audit evidence query/detail/export/download shall itself be `L3` Audit access. The first logical query and any changed scope shall be recorded; cursor continuations and each returned row shall not recursively generate separate Audit events. Export permission and evidence access shall not imply access to referenced user documents.

## 14.8 High-risk Feature and AI events

Protect, Unlock, visual Sign, and Redact shall emit one terminal high-risk event after final validation or denial/cancellation. Unlock events shall never include password. Visual Sign events shall identify `VISUAL_NON_PKI` and shall not claim cryptographic identity/non-repudiation. Redaction success shall be emitted only after approved negative extraction/structural validation; failure shall emit a safe validation-failure code without redacted content/coordinates.

AI events shall record actual Feature ID, User actor, origin, approved provider-profile or local route, policy/classification/permission/consent decision codes, Job/Correlation IDs when applicable, and outcome. Prompt, selected text, source content, summary, translation, Markdown, provider request/response, endpoint URL, and provider secret shall be prohibited. Attempts to use a public/unapproved AI route shall produce a restricted Security event.

## 14.9 Canonical metadata

Every Enterprise Audit event shall have a stable `AuditEventId`, `EventName`, schema version, category/level, `OccurredAtUtc`, `ReceivedAtUtc`, actor type and opaque actor reference when human, origin/executor, action, resource type/opaque ID, outcome, channel/platform/app/environment/execution location, and safe reason/error code.

Feature ID is required for Feature events; Job ID for persisted Job/export events; Correlation ID for privileged mutations, server processing, Support, Audit access/export, AI/provider, secured mutation, and high-risk Feature finalization. Process ID shall be a logical execution identity, not an operating-system PID.

Optional metadata shall use typed, versioned, size-bounded allowlists. Safe aggregates may include input/output/page count and approved size category/bytes. Unknown fields or prohibited content shall be rejected, not stored.

## 14.10 Offline and integrity behavior

Eligible local high-risk Audit events may be written to a protected per-profile offline outbox with stable Event ID, source sequence, actor binding, occurred time, and schema version. Reconnect shall authenticate current identity/session/client policy before bounded idempotent batch ingestion. Server shall validate event type/version, actor binding, time bounds, size, metadata allowlist, and prohibited-content rules, then store both occurred and received time.

Privileged Administrator, Support, Auditor, server, and reauthentication actions are online-only and shall not be asserted as successful by an offline client. Duplicate Event IDs shall not create duplicate evidence. Unknown schema, actor mismatch, clock anomaly, or prohibited payload shall fail closed/quarantine with a safe error code.

`audit.AuditEvents` shall be append-oriented and unavailable to ordinary update/delete. Corrections shall use linked corrective events. Audit query/export shall be permission- and scope-authorized before count/filter/cursor/content creation. History/resource deletion shall not cascade to Audit.

## 14.11 Explicit non-audit list

By default, Enterprise Audit shall not record mouse click, ordinary page open, progress percentage, queue poll, Worker heartbeat, upload chunk, range request, cache, thumbnail, autosave, undo/redo, selection, clipboard, document content, password, token, signature payload/intermediate edit, redaction region, OCR/full text, form value, prompt/chat, successful refresh, every authorization check, every ordinary error/denial, SignalR heartbeat/reconnect, internal retry, or temporary cleanup.

These activities may belong to local state, domain state, or bounded observability, but shall not be promoted into Audit merely because instrumentation exists.

## 14.12 Governed handoffs

Document 10 shall not invent official retention duration. Pertamina Audit/Compliance shall approve retention, legal hold, correction, archive, purge authority, OccurredAt/ReceivedAt lifecycle rule, integrity/immutability technology, SIEM forwarding/alerting, allowed actor/network/device metadata, offline clock/batch policy, and event-class fail-closed behavior.

Document 09 FINAL permission codes, scopes, Support mapping, segregation-of-duties boundaries, reauthentication cases, and all 33 Core `Audit=YES` flags have been revalidated in this report. Remaining governed confirmations are the Audit ownership/classification, retention/legal hold/correction/archive/purge lifecycle, integrity technology, SIEM/alerting, allowed contextual metadata, offline ingestion thresholds, and event-class fail-closed behavior listed above.
