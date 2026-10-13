# AGENT A — IDENTITY, AUTHENTICATION & AUTHORIZATION SECURITY REPORT

> **Task ID:** DOC08-AGENT-A  
> **Role:** Senior Identity & Access Security Architect / Senior ASP.NET Core Security Programmer  
> **Status:** Specialist recommendation for integration into Document 08 — **not a final Role Matrix**  
> **Date:** 24 Agustus 2026  
> **Authoritative baseline:** Documents 01–07 FINAL and `MASTER_PROJECT_SUMMARY.md` v3.0  
> **Canonical feature baseline:** 550 features; 33 Primary PDF Tool Cards  
> **Constraint:** No IAM vendor selection, no new identity provider, no production code, and no modification to Documents 01–07

Terminology in this report:

- **LOCKED** means inherited from Documents 01–07 and not reopened here.
- **RECOMMENDED** means the proposed security design for Document 08.
- **OPEN** means an approval, policy value, provider capability, or detailed mapping is still required.
- Permission names written as plain-language capabilities are **categories**, not the final permission codes for Document 09.

---

# 1. Executive recommendation

## 1.1 Recommended model

Use the following security model:

```text
Approved Pertamina Identity
        │ authenticates the human
        ▼
PERTAMINA OTO API authentication boundary
        │ validates token/session + account/session state
        ▼
Policy-based authorization
        │ permission + ownership/scope + enterprise policy
        │ + resource state + reauthentication when required
        ▼
Application service / registered Feature handler
        │ persists an authorized command or returns a resource
        ▼
Module-owned data, Worker, storage, and safe SignalR notification
```

The core recommendation is:

1. **Pertamina Identity is the identity authority; PERTAMINA OTO API is the enterprise authorization authority.** A valid identity token proves who the caller is, but does not by itself grant access to a Feature, file, job, issue, configuration, or audit record.
2. **Authentication and application authorization must remain separate.** The API validates the approved Pertamina token/session assertion, then evaluates current account status, session status, permissions, ownership/scope, policy, resource state, and reauthentication freshness.
3. **Role is only a permission grouping.** `User`, `Administrator`, and `Auditor` remain the baseline personas. “Superadmin” is the user-facing name for a permission-gated support surface, not an automatic fourth all-powerful role.
4. **Administrative authority and content-access authority must be disjoint.** Administrator, Support/Superadmin, and Auditor do not automatically gain the right to open user documents, OCR text, clipboard content, PDF passwords, signatures, job artifacts, or secured-file payloads.
5. **Native clients use the approved Pertamina flow with PKCE and protected operating-system credential storage.** Web should keep bearer/refresh credentials behind a server-side session boundary. The application must not collect or store the user's Pertamina password.
6. **Offline authorization is a narrow, time-bounded local capability.** It may authorize eligible local-only operations while its cached authorization state is still valid. It must never be accepted by the server as a substitute for current online authorization.
7. **Sensitive actions require server-enforced reauthentication.** Reauthentication is scoped, short-lived, and bound to the current principal/session and requested high-impact action; it is not a long-lived elevation mode.
8. **Worker and internal services use separate workload identities.** End-user access or refresh tokens must not be forwarded to a Worker, queue consumer, storage adapter, or internal provider.
9. **SignalR uses the same authenticated principal and authorization model as REST, but remains notification-only.** Groups are derived server-side; every connection and job subscription is authorized; REST remains authoritative.
10. **Deny by default and test negative paths first.** The security release gate must include invalid-token, stale-session, IDOR, vertical escalation, offline expiry, CSRF, CORS, SignalR, service-identity, and log-secret tests.

## 1.2 Source alignment

This recommendation preserves the following upstream decisions:

| Source | Preserved security baseline |
|---|---|
| Document 01 | Pertamina SSO, adaptive offline session, session revocation, sensitive reauthentication, distinct User/Administrator/Auditor boundaries, no automatic document access |
| Document 02 | `FR-AUTH-001..005`, `FR-ROLE-001..003`, permission-based authorization, backend-controlled role/permission, secure token storage |
| Document 03 | `FEAT-AUTH-001..012`, administrative and support/audit capabilities, canonical Feature IDs |
| Document 04 | Modular boundary, enterprise policy, Support Console, API + SignalR, Assistant as orchestrator |
| Document 05 | ASP.NET Core/.NET baseline and security approval gates without choosing an IAM vendor |
| Document 06 | Pertamina Identity + server authorization, bearer/session assertion, PKCE for native clients, server-side web session boundary, endpoint-specific authorization, REST authority, SignalR notification only |
| Document 07 | `identity.Principals`, hashed `identity.Sessions`, role/permission tables, no raw token/password in SQL/SQLite, module ownership, content outside SQL, logout locks/unmounts local profile |

Implementation guidance is additionally checked against the official [ASP.NET Core 10 policy-based authorization guidance](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-10.0), [ASP.NET Core 10 SignalR authentication guidance](https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-10.0), and [RFC 9700 OAuth 2.0 Security Best Current Practice](https://www.rfc-editor.org/info/rfc9700/). These references support implementation technique only; they do not replace the project source documents or select the Pertamina IAM protocol/vendor.

---

# 2. Trust boundary

## 2.1 Boundary map

```text
UNTRUSTED / PARTIALLY TRUSTED DEVICE ZONE
┌─────────────────────────────────────────────────────────────┐
│ Web browser / Desktop / Mobile / Tablet                     │
│ UI state, cached claims, FeatureId, IDs, fileRef, cursor,   │
│ CorrelationId, Assistant plan, and local clock are inputs   │
│ and are never server authorization proof.                   │
└───────────────────────┬─────────────────────────────────────┘
                        │ HTTPS; approved SSO flow
                        ▼
ENTERPRISE IDENTITY BOUNDARY
┌─────────────────────────────────────────────────────────────┐
│ Approved Pertamina Identity                                │
│ Authenticates user and issues approved assertion/token.    │
│ PERTAMINA OTO does not collect Pertamina credentials.      │
└───────────────────────┬─────────────────────────────────────┘
                        │ issuer/audience/signature/lifetime/subject
                        ▼
APPLICATION SECURITY BOUNDARY
┌─────────────────────────────────────────────────────────────┐
│ PERTAMINA OTO API / Web server / SignalR Hub                │
│ Authentication → account/session validation → permission   │
│ → ownership/scope → policy/classification → resource state │
│ → reauthentication → application contract                  │
└──────────────┬──────────────────────┬───────────────────────┘
               │ trusted contract     │ persisted authorized work
               ▼                      ▼
MODULE DATA BOUNDARY             INTERNAL WORKLOAD BOUNDARY
┌──────────────────────┐         ┌────────────────────────────┐
│ Module-owned SQL     │         │ Queue / Worker / adapters  │
│ + secure object data │         │ Separate service identity  │
│ + approved key store │         │ No end-user token forward  │
└──────────────────────┘         └────────────┬───────────────┘
                                             │ scoped provider contract
                                             ▼
                                    APPROVED INTERNAL PROVIDER
```

## 2.2 Trust rules

| Boundary | May be trusted for | Must not be trusted for |
|---|---|---|
| Approved Pertamina Identity | Authenticated subject and approved identity claims after validation | PERTAMINA OTO resource ownership, content access, Feature permission, application policy, or resource state unless explicitly mapped and validated |
| Client/UI | User intent and presentation | Role, permission, owner ID, support scope, resource existence, policy result, reauthentication result, or server Feature handler selection |
| Offline local profile | Local access only while protected authorization state remains valid | Server authorization, current revocation state, current permission state, cross-device truth, or privileged administration |
| API | Enterprise security enforcement and application command admission | Blind trust in tokens, IDs, queue messages, SignalR groups, or downstream responses |
| SQL/object storage | Persisted data after authorization | Authorization merely because a caller knows a key, path, public ID, storage reference, Job ID, or Correlation ID |
| Queue/Worker/internal network | Delivery of authenticated internal work under a separate workload identity | Implicit trust due to network location; direct acceptance of client payload or forwarded user token |
| SignalR | Safe notification that a committed revision may be available | Authoritative state, authorization grant, business command, or proof that a user still owns a resource |
| Administrator/Support/Auditor UI | A user interface for already-authorized actions | A privileged trust zone or automatic content-access grant |

## 2.3 Identity and trace identifiers

- The external identity key is the validated combination of approved issuer and external subject. It maps to the internal opaque `PrincipalId` in `identity.Principals`.
- A `SessionId` or equivalent session reference is stored only in hashed/non-reversible form in `identity.Sessions`; the database row is session/revocation metadata, not a bearer credential.
- `JobId` identifies a persisted job and is traceable to its owner, Feature ID, and authorization context. It never grants access.
- `CorrelationId` links a logical action across API, Worker, safe audit, and diagnostics. It is neither identity nor authorization evidence.
- Opaque IDs, `fileRef`, cursors, ETags, storage references, and SignalR connection IDs are locators only.

---

# 3. Authentication flow

## 3.1 SSO boundary

The application must redirect or broker authentication to the approved Pertamina Identity boundary. PERTAMINA OTO must not:

- create a separate username/password database;
- collect, proxy, log, or retain the Pertamina password;
- imitate the enterprise login page;
- select a new identity provider or MFA method;
- implement password recovery, account unlock, or credential lifecycle outside the approved Pertamina process;
- treat an unvalidated identity claim, email address, display name, employee number, or client-supplied principal ID as a stable subject.

The exact Pertamina endpoint, protocol profile, issuer, audience, client registration, claim contract, and logout capability are **OPEN** integration inputs. The security properties required by Document 06—validated signature/algorithm, issuer, audience, lifetime, subject, and approved claims—remain mandatory.

## 3.2 Common online authentication sequence

```text
1. Client checks compatibility and local secure-session state.
2. If no valid online session exists, client starts the approved Pertamina SSO flow.
3. Pertamina Identity authenticates the user and performs enterprise controls.
4. Client/server validates transaction binding such as state/nonce and PKCE where applicable.
5. The API validates the presented access token/session assertion.
6. The API maps validated issuer + subject to the internal PrincipalId.
7. The API verifies the PERTAMINA OTO principal is known/eligible, not disabled,
   and the application session is not expired or revoked.
8. The API resolves current effective permissions and applicable policy.
9. Every resource request receives object-aware authorization.
10. The client receives only a safe profile and effective capability projection.
```

The provisioning rule for an unknown but valid Pertamina subject—pre-provision only, approved just-in-time mapping, or another governed process—is **OPEN** and must not be guessed by the client.

## 3.3 Desktop and Mobile/Tablet

- Use the approved authorization flow with PKCE as locked by Document 06.
- Use the system/browser/broker behavior approved by Pertamina; do not embed or scrape an enterprise login form.
- Bind the authentication response to the initiating transaction and client instance. Reject unsolicited, replayed, mismatched, or expired callbacks.
- Keep the authorization code and PKCE verifier in memory for the transaction only.
- Store refresh credentials, if Pertamina Identity issues them to that client class, only in approved OS credential storage. Access tokens should be memory-resident where practical and protected by the same approved credential boundary if persistence is necessary.
- Do not store access or refresh tokens in SQLite, ordinary configuration, app logs, crash data, clipboard, URL, analytics, issue diagnostics, or user-exportable backup.
- Device registration, device compliance, sender-constrained tokens, or hardware binding may be used only if supported and approved by Pertamina. They are not assumed in this report.

## 3.4 Web

- Prefer a server-side session/BFF-style boundary consistent with Document 06: the browser holds only an opaque protected session cookie, while access/refresh credentials stay server-side in an approved credential boundary.
- The session cookie must be `Secure`, `HttpOnly`, narrowly scoped by host/path, and use the strictest `SameSite` behavior compatible with the approved SSO flow.
- Rotate the web session identifier after login, reauthentication, and any material privilege-context change.
- Do not expose bearer or refresh tokens to browser JavaScript, Web Storage, URL fragments, query strings, page source, or service-worker caches.
- If an approved Web architecture later requires browser-held OAuth tokens, it requires explicit security review and must not be inferred from this recommendation.

## 3.5 Login once

“Login sekali” is a usability requirement, not a perpetual security grant. It means the application should silently renew an eligible session through the approved Pertamina mechanism while all of these remain true:

- the enterprise session/grant remains valid;
- the local application session has not expired or been revoked;
- the account is active;
- refresh is permitted for that client type;
- current policy and minimum supported client version allow access;
- a sensitive action does not require fresh authentication.

Interactive login is required after unrecoverable refresh failure, revocation, account disablement, offline-session expiry, local credential loss, provider-required interaction, or a policy-mandated reauthentication.

## 3.6 Revoked and disabled accounts

| Condition | Recommended server behavior | Client behavior |
|---|---|---|
| Missing, malformed, invalid, expired, wrong issuer/audience, or revoked token/session | `401` with safe error | Stop retry loop; refresh once when eligible, otherwise require login |
| PERTAMINA OTO session explicitly revoked | Mark `identity.Sessions` revoked, reject subsequent calls with `401`, close/deny SignalR connections when known | Clear credentials, invalidate offline authorization, lock local profile |
| Valid identity but PERTAMINA OTO principal disabled/ineligible | Deny before resource disclosure; recommended `403 ACCOUNT_DISABLED` pending final API error policy | Lock server features and require the approved support/identity recovery path; do not continuously refresh |
| Identity provider disables/revokes account | Consume the approved propagation mechanism; deny at next validation at the latest | Same as revoked/disabled |
| Revocation occurs while device is offline | Immediate remote enforcement is impossible; residual access is bounded by offline expiry and local-only restrictions | At reconnect, refresh identity/permission before any queued server action |

No response should reveal unnecessary account existence, employment status, role membership, or administrative reason.

## 3.7 Logout

Logout must be idempotent and perform, in a safe order:

1. stop SignalR and prevent further server command dispatch;
2. invalidate/revoke the PERTAMINA OTO application session when online;
3. clear access/refresh credentials from memory and approved client credential storage as applicable;
4. invalidate the cached offline authorization state;
5. lock/unmount the local profile so another account cannot see its metadata or protected artifacts;
6. clear session cookies and rotate/delete server-side web session data;
7. request enterprise IdP logout only when the approved Pertamina flow and desired single-sign-out behavior require it;
8. write safe audit metadata without token, cookie, IP-derived identity assumptions, or document content.

Logout must not delete the user's original files. Deletion or cryptographic key destruction for app-managed local artifacts remains governed by an approved retention/logout policy and must be recoverable or explicitly communicated where required.

---

# 4. Session/token model

## 4.1 Artifact model

| Artifact | Purpose | Location | Lifetime/rotation | Prohibited use |
|---|---|---|---|---|
| Authorization code / PKCE verifier | Complete one login transaction | Memory only | One-time, transaction-bound | Storage, logging, reuse |
| Pertamina access token/session assertion | Authenticate API/SignalR caller | Native protected memory/credential boundary; server-side for Web | Short-lived according to approved IdP policy; validate every request/connection | Offline authorization, Worker forwarding, storage access key |
| Refresh token/grant, if issued | Obtain a new access token | Approved OS credential store for eligible native client; approved server credential boundary for Web | Provider-controlled expiry; rotation/replay protection according to approved provider capability | API resource call, SQL/SQLite persistence, browser-accessible storage |
| Web application session cookie | Reference server-side Web session | Browser cookie only | Rotated after login/reauth/context change; expires/revokes by policy | Bearer credential outside its origin/path |
| `identity.Sessions` row | Application session, expiry, revocation, validation metadata | SQL Server `identity` schema | Explicit timestamps/status; lifecycle policy | Raw token/cookie/refresh credential storage |
| Offline authorization state | Permit a narrow set of local capabilities | Protected per-profile/device local security boundary | Configurable, bounded, non-renewable while offline | Server/API/SignalR authorization or admin/support/audit access |
| Reauthentication context | Prove recent interactive authentication for one risk context | Server session/authorization context | Short, policy-defined, action/resource-bound | Global role elevation or reusable bearer token |
| Workload credential | Authenticate API, Worker, queue consumer, adapter/provider | Approved server workload credential boundary | Short-lived and rotatable | End-user impersonation or client distribution |

## 4.2 Access-token validation

For each API request and initial SignalR connection, authentication must validate at least:

- HTTPS and expected host/endpoint;
- signature or approved reference-token validation;
- allowed cryptographic algorithm/profile;
- exact approved issuer;
- API-specific audience/resource;
- token type/use, lifetime, `not-before`, and bounded clock skew;
- stable subject;
- approved client/application context where required;
- session reference/revocation state when available;
- token/claim schema only from the approved claim contract.

Fail closed if validation metadata, signing keys, revocation status required by policy, or the account/session store is unavailable. A `503` may be used when the security dependency is unavailable and the token is not safely verifiable; the system must never degrade to anonymous or cached server authorization.

## 4.3 Refresh behavior

- Refresh is performed against the approved Pertamina Identity boundary, never against ordinary PERTAMINA OTO resource APIs.
- Refresh proactively within a policy-defined window and use a single-flight guard so concurrent API failures do not create a refresh storm.
- Retry only safe transient failures with bounded backoff and jitter. An invalid grant, reuse/replay detection, revoked session, account disablement, or required user interaction ends silent refresh.
- Apply a newly issued token atomically. Never allow old and new refresh credentials to race across threads/processes.
- If the provider supports or requires rotation/sender constraint, preserve its replay-detection semantics. Do not implement an incompatible parallel rotation scheme.
- A refreshed identity does not preserve stale application permissions. Re-resolve current PERTAMINA OTO account/session/permission/policy state.
- A failed refresh must not silently fall back to offline mode if the offline authorization state is absent, expired, tampered, or ineligible for the requested operation.

Exact access-token lifetime, refresh-token lifetime, inactivity timeout, absolute session lifetime, clock skew, refresh window, and retry count are **OPEN** policy values.

## 4.4 Token expiry during operations

- Synchronous API authorization is evaluated at request admission. Long file transfers should be bounded/resumable so every new request/range/continuation is reauthorized.
- A background job accepted durably under a valid user authorization runs under a Worker identity, not the user's expiring access token.
- Access to job state, cancellation, retry, checkpoint, artifact, result, or download is reauthorized on every request.
- Revocation after job acceptance must at minimum prevent the revoked principal from retrieving results. Whether an already accepted job is cancelled, allowed to finish without exposure, or revalidated at a safe execution boundary is an **OPEN** policy decision by workload/risk class.

## 4.5 Error semantics

Preserve Document 06:

| Status | Identity/authorization meaning |
|---:|---|
| `401` | Authentication missing, invalid, expired, revoked, or reauthentication required |
| `403` | Identity valid but permission, ownership, account eligibility, or policy denies access |
| `404` | Resource absent or consistently concealed to avoid unauthorized enumeration |
| `409` | Security-relevant state/version conflict, not a substitute for authorization denial |
| `429` | Authentication/session/reauth/connection abuse or rate limit; include safe `Retry-After` when applicable |
| `500` | Unexpected server failure with no sensitive detail |
| `503` | Required security dependency unavailable or server unable to make a safe decision |

Every error includes at minimum `code`, `message`, and `correlationId`; it must exclude token, claims dump, session ID, stack trace, policy internals, storage reference, document content, or account-enumeration detail.

---

# 5. Offline-session security

## 5.1 Security objective

Offline session provides continuity for eligible local work; it is not an offline copy of enterprise authority. The application may use last-known authorization only while it is valid, locally protected, bound to the active profile/device where supported, and sufficient for the requested local capability.

## 5.2 Minimum offline authorization state

Store only the minimum protected authorization state needed for offline evaluation:

- internal profile/principal reference and approved external-subject binding;
- issue time, last successful online-validation time, and absolute expiry;
- offline-eligible capability/permission projection—not a raw unrestricted claim dump;
- permission/policy/capability/contract version identifiers;
- applicable safety ceilings and classification restrictions;
- device/profile binding information where the platform provides an approved mechanism;
- integrity/version metadata and last-known trusted time evidence;
- revocation/disable marker learned during the last online validation.

Do not store access token, refresh token, password, cookie, service credential, full enterprise directory claims, document content, or unrestricted support/admin/audit permissions in this state. It must reside in the per-profile protected local boundary and be excluded from ordinary cloud/device backup and cross-account access.

## 5.3 Offline eligibility

| Capability class | Offline recommendation |
|---|---|
| Eligible deterministic local Feature operating on user-selected local files | Allowed only while offline authorization and applicable policy remain valid |
| Local draft, preview, undo/redo, local history, or local recovery | Allowed within protected profile and data-policy limits |
| Server processing, upload, account-secured storage, cross-device data, issue submit, audit export | Denied/pended until online authorization |
| Administrator, Support/Superadmin, Auditor server surfaces | Denied offline |
| Sensitive action requiring reauthentication | Denied until approved online reauthentication completes |
| Action dependent on current classification, ownership, quota, legal hold, revocation, or server policy | Denied or held pending online validation |
| Assistant local orchestration | May propose only offline-eligible local Features; cannot bypass the same local permission/policy checks |

The local client must not create an API processing dependency for a feature that is local-only.

## 5.4 Lifetime and clock integrity

- Offline lifetime must be configurable by approved enterprise policy and bounded by an absolute expiry. No sliding renewal is allowed while offline.
- Permission/claim validity cannot exceed the offline-session validity or the governing policy/version validity.
- The client should combine trusted last-online time, monotonic elapsed time when available, and rollback detection. A wall-clock rollback must not extend the offline grant.
- Reboot, sleep, hibernate, time-zone change, daylight-saving change, and clock rollback must be tested.
- If time integrity cannot be established, fail closed for protected actions and require online validation.
- The exact lifetime by device and risk/classification is **OPEN**. This report intentionally does not invent a number.

## 5.5 Expiry, logout, account switching, and device loss

- On offline-session expiry, deny permission-bearing operations and require online authentication. Do not silently delete user source files.
- Preserve app-managed work only according to approved recovery/retention policy; a read-only recovery UX, if desired, requires explicit product/security approval.
- Logout invalidates offline authorization and locks/unmounts the local profile.
- Account switching must close the previous profile, clear its in-memory keys/state, and mount a physically/logically separated profile. Favorites, history, Assistant state, drafts, secured cache metadata, and claims must not bleed across accounts.
- Device loss cannot be immediately remediated while the device remains offline. The residual risk is bounded by local encryption/key protection, platform login/device controls, offline expiry, and the prohibition on offline privileged/server actions.

## 5.6 Reconnect validation

```text
Connectivity returns
→ do not send queued server commands yet
→ refresh/obtain approved online identity
→ validate issuer/audience/lifetime/subject
→ validate principal active + session not revoked
→ load current permission, policy, capability, and compatibility versions
→ compare with cached offline state
→ invalidate removed capabilities and stale policy
→ reauthorize every queued operation using current resource state/ownership
→ request consent/reauthentication where now required
→ submit only still-valid commands with idempotency protection
→ reconcile REST state; SignalR only accelerates notification
```

Queued intent is not a prior authorization grant. A command created offline may be rejected after reconnect because permission, ownership, policy, classification, quota, client compatibility, resource state, or reauthentication requirements have changed.

---

# 6. Reauthentication

## 6.1 Model

Sensitive reauthentication must be enforced by the server authorization policy, not by UI visibility or a local confirmation dialog. It proves that the current authenticated principal recently completed an approved interactive authentication step; it does not grant new permissions.

Recommended evaluation:

```text
valid current identity
AND active/non-revoked application session
AND required permission
AND resource ownership/scope
AND current policy/state
AND authentication freshness/method satisfies risk policy
= sensitive action may proceed
```

If the approved Pertamina Identity interface supports a step-up/fresh-authentication request, use it. Otherwise require a new approved interactive login. A device PIN, app PIN, biometric prompt, or password typed into PERTAMINA OTO is not automatically enterprise reauthentication unless Pertamina explicitly approves that assurance mapping.

## 6.2 Candidate trigger categories

Document 09/10 and enterprise security policy must finalize the action matrix. Recommended candidate categories are:

- permanent deletion of secured server content;
- changes to roles, permissions, security profiles, identity/session policy, or another user's access;
- self-affecting privileged changes and any potential self-elevation;
- high-impact export or disclosure of audit/support information;
- exceptional content access if such a capability is ever approved;
- destructive recovery/restore or key-lifecycle actions;
- revoking other users' sessions or disabling accounts;
- policy-defined high-classification transfers or externalization actions.

Routine local editing, ordinary Feature discovery, and safe read-only metadata should not request reauthentication merely because they are frequent. The exact trigger list, maximum authentication age, assurance method, and retry/lock policy are **OPEN**.

## 6.3 Reauthentication context

The accepted context should be:

- bound to `PrincipalId`, current application session, client/device context where approved, action category, resource/scope, and policy version;
- short-lived and preferably single-action or bounded-use;
- invalidated by logout, session revocation, account disablement, material permission change, or risk-policy change;
- audited with safe metadata such as action category, success/failure, actor, time, Correlation ID, and target type/opaque ID—never credentials or challenge details;
- unusable by another browser tab/client/account for an unrelated action.

When freshness is missing, return Document 06's `401` semantics with a safe code such as `REAUTHENTICATION_REQUIRED`; do not perform a partial mutation before the reauthentication completes.

---

# 7. Authorization model

## 7.1 Decision formula

Every protected action is evaluated as:

```text
Allow = Authenticated
    AND AccountActive
    AND SessionValid
    AND EndpointOrFeaturePermission
    AND ResourceOwnershipOrExplicitScope
    AND EnterprisePolicyAndClassification
    AND ResourceStateAndVersion
    AND ClientCompatibilityAndCapability
    AND RequiredConsent
    AND ReauthenticationFreshnessWhenRequired
```

Absence, uncertainty, stale state, or handler failure results in deny. Role name, Feature ID, route, UI card, opaque ID, Job ID, Correlation ID, `fileRef`, cursor, ETag, storage key, SignalR group name, or Assistant instruction can never replace any term in this formula.

## 7.2 ASP.NET Core enforcement pattern

RECOMMENDED implementation structure, without production code:

- Configure a global authenticated fallback policy for protected API, MVC, and SignalR surfaces. Only explicitly documented endpoints such as minimal compatibility/liveness may be anonymous.
- Express permission, account/session state, reauthentication freshness, classification, and other reusable checks as named authorization requirements/policies.
- Use resource-based authorization handlers for owner/scoped actions on jobs, results, uploads, secured files, issues, attachments, audit exports, and configuration aggregates.
- Invoke authorization inside the application boundary before loading content, resolving a storage reference, opening a stream, counting rows, exporting, subscribing, or dispatching a Feature handler.
- Keep the final decision server-side. Client-side checks are usability hints only.
- Fail closed on missing policy handlers, unknown permission codes, unknown Feature IDs, unsupported schema versions, or unavailable authoritative policy data.

## 7.3 Resource ownership

- Persist an owner or governing scope for each user resource using internal `PrincipalId` or a typed application scope.
- Query lists through the authorized scope before count, pagination, filter, search, sort, or cursor generation.
- For object reads, load only the minimal ownership/classification metadata needed to authorize; resolve content/storage only after success.
- Reauthorize every continuation, range request, download, retry, resume, checkpoint, cancel, restore, export, and SignalR subscription.
- Do not accept an owner ID from the client when the owner is the current caller; derive it from the validated principal.
- Transfer, delegation, shared ownership, team scope, or legal/compliance hold must use an explicit governed model if later required. None is invented here.

## 7.4 Horizontal privilege-escalation prevention

- Treat every public identifier as guessable.
- Enforce `resource.OwnerPrincipalId == current PrincipalId` or an explicit separately authorized scope.
- Scope child resources through the parent and independently verify child membership; do not trust `{parentId}/{childId}` consistency from the route.
- Conceal unauthorized resources with consistent `404` where Document 06 policy chooses concealment.
- Prevent enumeration through total counts, timing, export status, error wording, range metadata, thumbnail existence, cursor behavior, and SignalR subscription responses.
- Test cross-user access to uploads, jobs, artifacts, results, history, secured files/versions, issue messages/attachments, exports, and local profile caches.

## 7.5 Vertical privilege-escalation prevention

- Never authorize solely with `IsInRole("Administrator")`, a UI category, or a client-supplied permission list.
- Map validated identity claims and internal assignments to explicit effective permissions; deny unknown/unmapped claims.
- Protect role/permission assignment, session revocation, configuration, feature flags, security profiles, and support/audit exports with distinct permissions and reauthentication where required.
- Prevent self-assignment/self-elevation unless an explicitly approved policy allows it. A privileged administrator must not grant themselves content access by default.
- Apply optimistic concurrency/version checks to permission/policy changes so a stale administrator cannot overwrite a newer restriction.
- The generic Feature execution endpoint may dispatch only to the registered typed handler for the canonical `FeatureId`, with that Feature's required permission and policy. Parameter content cannot select an arbitrary handler, SQL, path, engine, or elevated action.
- Assistant-resolved Feature execution uses the same application contract and current principal. Prompt text or a workflow plan cannot add permission.

## 7.6 Authorization-change propagation

- The server authorization store is authoritative. Any client entitlement projection is display/cache data only.
- Permission, role, account, session, policy, and feature-exposure changes must carry a version or invalidation signal.
- Online API requests use current authoritative state or a tightly bounded cache with explicit invalidation appropriate to risk.
- SignalR may notify a client that its authorization context changed, but the notification is not the change authority.
- Offline clients learn changes at reconnect; residual offline access is bounded by Section 5.

---

# 8. Permission model

## 8.1 Model dimensions

The final permission matrix belongs to Document 09. Document 08 should define permission semantics across these independent dimensions:

| Dimension | Examples of capability category | Rule |
|---|---|---|
| Feature execution | Execute a canonical local/server Feature; execute a workflow | Bound to canonical Feature ID, exposure, schema, capability, classification, and current policy |
| Personal resource | Read/update/delete own job, result, history, secured file, issue | Owner check remains mandatory; permission alone is insufficient |
| Content access | Open/download/preview/export a document or restricted attachment | Separate from administrative, support, monitoring, and audit-metadata authority |
| Administration | Read/write policy, role, permission, feature flag, limit, compatibility, session state | Granular read/write; hard safety ceiling and reauthentication for high-impact changes |
| Support | List/detail issue, read explicitly submitted attachment, triage, note, export | Separate capability for each surface; no access to original user documents or unrelated jobs/files |
| Audit | Query safe metadata, view detail, export bounded evidence | Read/export separated; no delete; no automatic document content |
| Monitoring/operations | View safe job/queue/error/health metadata; perform approved operation | Metadata-only by default; no content or service secret disclosure |
| Service/workload | Claim job, update owned job state, access job-scoped object, publish outbox | Workload identity only; narrow audience/scope; not grantable to human client tokens |

## 8.2 Role and permission relationship

```text
Baseline persona/role
→ groups a maintained set of permissions
→ permissions are evaluated with resource and context
→ final decision is allow/deny for one action on one resource
```

- `User`, `Administrator`, and `Auditor` are baseline role/persona groupings, not hardcoded universal allowlists.
- A person may hold multiple groupings, but permissions do not imply one another. An administrative permission does not imply content read; an audit permission does not imply issue attachment read; support triage does not imply secured-file access.
- “Superadmin” is a label for users holding the necessary Support permissions. Document 07 correctly does not create a `Superadmin` database role.
- Role/permission definitions and assignments are backend-controlled and versioned. The UI never decides or mutates them without an authorized server command.
- The exact permission codes, role-to-permission mappings, segregation-of-duties rules, delegated scopes, and approval workflow are **OPEN for Document 09**.

## 8.3 Effective permission evaluation

Recommended order:

1. accept only validated identity claims from the approved Pertamina assertion;
2. map issuer + subject to an active internal principal;
3. load explicit active role/permission assignments and policy restrictions;
4. compute effective permission with deny restrictions taking precedence;
5. bind the result to the current contract/policy/capability versions;
6. apply resource ownership/scope and classification/state checks;
7. require reauthentication if the action policy demands it;
8. produce only a safe, minimal entitlement projection for UI discovery.

If Pertamina Identity supplies roles/groups, they are inputs to an approved mapping—not automatic PERTAMINA OTO permissions. Whether effective permissions live primarily in token claims, the PERTAMINA OTO authorization store, or a controlled combination is **OPEN**; the selected design must support prompt revocation, versioning, deny-by-default, and Document 07 ownership.

---

# 9. Admin/support boundary

## 9.1 Separation of authority

```text
Administrative authority  ≠  Content-access authority
Support authority         ≠  User-document authority
Audit authority           ≠  Document-content authority
Monitoring authority      ≠  Storage-object authority
```

Even if the same person legitimately holds more than one permission set, the API must evaluate the permission relevant to the requested action. It must not infer content access from a powerful-sounding title.

## 9.2 Boundary by persona/surface

| Surface | May be authorized for | Must not be implied |
|---|---|---|
| User | Own local work, own server jobs/results/history/secured files/issues subject to Feature/policy | Other user's resource, admin/support/audit surface |
| Administrator | User/session administration, roles/permissions, feature exposure, flags, limits, policy, compatibility, safe job/queue/health monitoring according to granular permission | Opening user document bytes, full OCR, PDF password, clipboard, signature asset, issue attachment, or result content |
| Support/Superadmin | Issue list/detail, message, explicitly submitted screenshot/attachment, triage, status, note, bounded support export according to separate permissions | Original document, unrelated local/server file, secured-file payload, job result, arbitrary user storage, token/secret, or permission bypass |
| Auditor | Safe audit metadata query/detail/export according to separate permissions and classification | Audit deletion, unrestricted support message/attachment, user document content, password/token/clipboard/signature/full OCR |
| Operations/Worker | Safe health/queue/job metadata or job execution under workload identity | Human impersonation, role management, unrestricted content browsing |

## 9.3 Administrator limits

- The session-revocation capability required by Document 01 must be permission-gated; the title Administrator alone is not the final check.
- Role and permission administration must not let an administrator silently self-elevate to content access. Self-assignment, peer approval, segregation of duties, and emergency access rules require Document 09/enterprise approval.
- Administrator cannot alter the semantic meaning of a canonical Feature, bypass the application contract, disable mandatory audit, weaken hard safety ceilings, or directly update another module's tables.
- Monitoring returns safe metadata only. Filename/path, storage references, document preview, OCR, raw exception, secret, and provider payload are excluded unless a separately approved content-access workflow exists.
- High-impact security-policy changes require reauthentication, optimistic concurrency, safe audit evidence, and no partial application.

## 9.4 Support/Superadmin limits

- Support sees only the issue content and attachment that the reporter explicitly submitted through the Issue contract.
- Support access is split into list, detail, attachment, triage, note, status, and export permissions as locked by Document 06.
- An Issue `JobId` or `CorrelationId` permits safe diagnostic correlation only. It does not grant access to the job, its input, result, or the user's secured file.
- Export is bounded, filtered, watermarked/labeled if required by policy, access-controlled, expiring, and audited. Export permission is distinct from view permission.
- There is no implicit “view as user”, arbitrary impersonation, storage browsing, or break-glass content access.
- If emergency content access is later required, it must be designed as a separate, explicit, purpose-bound, time-bounded, reauthenticated, approved, and audited capability. This report does not create it.

## 9.5 Auditor limits

- Authorization occurs before count, query, detail, cursor, and export.
- Auditor reads approved safe evidence metadata and cannot delete or mutate the enterprise audit trail.
- Personal-history deletion does not affect enterprise audit.
- Audit evidence may reference opaque resource IDs for traceability but does not expose the referenced content.
- Legal-hold, retention, audit-integrity, and event-catalog specifics remain Document 10 responsibilities.

---

# 10. API/SignalR auth

## 10.1 API authentication and authorization

- Protect `/api/v1` resources with the approved authentication scheme and global deny-by-default authorization.
- Keep `/api/compatibility` and minimal health endpoints explicitly scoped and free of sensitive information; do not make other routes anonymous by convention.
- Validate authentication before expensive parsing/processing where practical, then validate the typed request and perform resource authorization before content/storage access.
- For canonical Feature/workflow execution, authorize the Feature ID, registered handler, typed parameter schema version, input ownership, capability, policy/classification, consent, and current capacity/state.
- Identity/session/account functions are resource/security flows, not arbitrary calls through `/api/v1/features/{featureId}/execute`, even though `FEAT-AUTH-001..012` remain canonical catalog capabilities.
- The Assistant calls the same Feature/application contract under the initiating user principal. It cannot call SQL, storage, Worker, or privileged endpoints directly and cannot convert prompt instructions into permission.
- Reauthorize every download, range request, continuation, retry, resume, cancel, export, and result retrieval.
- Keep `401`, `403`, and consistent concealed `404` behavior aligned across MVC, API, download, export, and SignalR subscription surfaces.

## 10.2 Web/API and CSRF

Cookie-authenticated Web requests are CSRF-sensitive even when the underlying API is secure. Apply:

- anti-forgery token validation for every state-changing cookie-authenticated request, including session/logout/admin mutations and any SignalR negotiation flow that relies on cookies;
- safe HTTP semantics: no mutation on `GET`/`HEAD`;
- strict cookie scope and `SameSite` as defense in depth, not as the sole anti-CSRF control;
- transaction-bound `state`/`nonce`/PKCE protections for the authentication callback as supported by the approved flow;
- Origin/Referer validation where appropriate and compatible with approved deployment;
- no CORS assumption as a substitute for CSRF protection.

Bearer-only native API calls with tokens in the `Authorization` header are not normally ambient-cookie CSRF targets, but they remain subject to token theft, replay, malicious deep links/callbacks, and confused-deputy attacks.

## 10.3 CORS principles

- Deny cross-origin access by default.
- Allow only explicit HTTPS origins per environment; never reflect arbitrary `Origin` values.
- Do not combine wildcard origins with credentials.
- Allow only required methods and headers; keep exposed response headers minimal.
- Treat CORS as a browser read boundary, not authentication or authorization.
- Separate Web origins from native clients; native clients do not gain authorization by claiming an allowed Origin.
- Protect preflight behavior from leaking security/configuration detail.
- Deployment must supply the final host/origin inventory before Production.

## 10.4 SignalR authentication

Use the locked hub `/hubs/activity` and the same approved principal as REST:

1. authenticate the negotiate/connection request;
2. validate account/session state and Hub-access permission;
3. derive the user/group key server-side from the validated internal principal;
4. authorize any job subscription against current ownership or explicit support scope;
5. publish only safe metadata after state is committed;
6. on reconnect, reauthenticate, reauthorize, and reconcile via REST;
7. close or deny connections when expiry/revocation/disablement becomes known.

Browser transports can place an access token in the `access_token` query parameter when headers cannot be set, as documented by [Microsoft SignalR configuration guidance](https://learn.microsoft.com/en-us/aspnet/core/signalr/configuration?view=aspnetcore-10.0). If this approved mechanism is used:

- accept it only on the exact Hub path and only over HTTPS;
- redact query strings and token values from proxy, server, WAF, tracing, and diagnostic logs;
- never persist or echo the value;
- prevent referrer leakage and third-party analytics on that endpoint;
- prefer server-side cookie/session authentication for Web where the chosen architecture supports it.

## 10.5 SignalR authorization and expiry behavior

- SignalR executes no business command. Subscription control cannot mutate a job or resource.
- A client cannot request an arbitrary user, role, or group name.
- Connection ID and group membership are not reusable authorization grants.
- Events contain no document/preview/OCR/prompt/chat/password/token/secret/storage key/signature/exception content.
- Each event is at-least-once, versioned, sequence-aware, and points the client to an authorized REST refresh.
- Token refresh does not retroactively change an established connection principal. Reconnect with a fresh token/session when expiry or policy requires it.
- Because a long-lived transport may not automatically revalidate every token property after connection establishment, use bounded connection/reconnect policy and actively terminate known revoked/disabled sessions. Exact maximum connection age and revocation propagation SLA are **OPEN**.

## 10.6 Session fixation, replay, and abuse

- Rotate web session identifiers after login, reauthentication, and privilege-context changes; reject pre-authentication session reuse.
- Use transaction-bound PKCE/state/nonce protections and exact redirect URI validation for approved authorization flows.
- Treat bearer tokens as replayable unless the approved provider supplies sender constraint; TLS and protected storage remain mandatory.
- Refresh-token replay protection follows the approved provider's rotation/sender-constraint capability.
- API idempotency prevents duplicate business effects but does not make a stolen token safe.
- Rate-limit authentication callback, refresh coordination, session validation/revocation, reauthentication, token-failure patterns, SignalR negotiate/reconnect, and protected API categories.
- Return `429` with safe retry guidance; use identity/account/client/device signals when available, with IP only as a supplemental signal.
- Do not implement a separate application password lockout. Authentication brute-force controls remain at Pertamina Identity; PERTAMINA OTO detects and limits abuse of its own endpoints without revealing account existence or enabling easy lockout denial-of-service.

---

# 11. Service/worker identity

## 11.1 Identity separation

Use distinct, approved workload identities for independently trusted server components, such as API, Worker, queue publisher/consumer, storage adapter, export processor, and approved internal provider integration where deployment topology requires them.

Each workload identity must have:

- a specific audience and service purpose;
- minimum operations and module/data scope;
- short lifetime and approved rotation/revocation;
- no interactive human login and no reuse by a client application;
- no embedded secret in source, configuration file, database, log, container image, package, or issue diagnostic;
- independent audit/telemetry identity so human and workload actions are distinguishable.

The exact certificate, managed identity, token service, or secretless platform mechanism is **OPEN** and must use an approved Pertamina dependency. This report does not select a vendor.

## 11.2 Job authorization handoff

```text
User token authenticates API request
→ API authorizes Feature + input + permission + policy + consent
→ API persists authorized Job and immutable/safe authorization context
→ queue/outbox publishes only after commit
→ Worker authenticates with its workload identity
→ Worker claims only an eligible Job through the jobs contract
→ Worker validates Job state, handler/schema, lease, and scoped artifacts
→ Worker writes only through the owning module contract
→ user later reauthorizes to view/control/download result
```

The persisted job authorization context supports traceability but is not an unrestricted capability token. It should include only required references/versions such as owner `PrincipalId`, canonical `FeatureId`, contract/parameter/workflow versions, policy/capability version, decision timestamp/category, `JobId`, and `CorrelationId`; it must exclude the end-user token, raw claim dump, password, prompt/document content, and service credential.

## 11.3 Internal trust controls

- Never forward an end-user access/refresh token to Worker, queue, storage, or provider.
- Authenticate both producer and consumer; authorize queue/topic and message type; validate message schema, issuer/source, replay/idempotency, Job state, lease, and sequence.
- A Worker must not execute a client-specified arbitrary executable, Feature handler, storage path, URL, SQL, or provider.
- Scope storage access to the approved job/artifact and minimum read/write duration. A storage reference alone grants nothing.
- Restrict database access by module ownership. Worker transactions are short and must not bypass the jobs/application contract or write across schemas.
- Provider adapters receive only the minimum approved content and operation context after policy/consent; they do not receive user identity tokens.
- Internal service failure must fail safely, preserve persisted REST state, and never fall back to public AI or a weaker identity mechanism.
- Rotation and revocation must not require redeploying secrets embedded in binaries.

---

# 12. Attack scenarios

| No. | Scenario | Security failure sought | Required prevention/detection |
|---:|---|---|---|
| 1 | Forged token or wrong issuer/audience | Authentication bypass | Strict token validation and allowlisted metadata; `401` |
| 2 | Algorithm/key confusion or stale key | Accept attacker-controlled assertion | Allowed algorithm/profile, trusted key discovery/cache rules, fail closed |
| 3 | Authorization-code interception/injection | Session takeover | Approved redirect, PKCE, state/nonce, transaction binding, one-time callback |
| 4 | Login CSRF/account swapping | Bind victim browser to attacker account | Callback CSRF protections, rotate session, show safe active identity |
| 5 | Access-token theft/replay | API/SignalR impersonation | TLS, protected storage, short lifetime, audience restriction, provider replay controls if available |
| 6 | Refresh-token theft/race | Persistent account takeover | Approved secure storage, single-flight refresh, rotation/sender constraint when supported, reuse response |
| 7 | Session fixation | Retain pre-login session ID | Regenerate server session after login/reauth/context change |
| 8 | Logout residue | Continue using cached credential/profile | Server revoke, clear credentials, invalidate offline state, stop Hub, lock profile |
| 9 | Revoked/disabled account with still-valid token | Continue online access | Application session/account-state check and approved revocation propagation |
| 10 | Offline account revocation gap | Continue local access after remote revoke | Absolute offline expiry, local-only scope, protected profile, reconnect validation |
| 11 | Device clock rollback | Extend offline permission indefinitely | Trusted last-online time, monotonic elapsed time, rollback detection, fail closed |
| 12 | Cross-account local cache bleed | User B sees User A data/claims | Per-profile/device store, unmount/zero in-memory state, account-switch tests |
| 13 | Horizontal IDOR | Read another user's job/file/issue/result | Owner-scoped query and resource authorization on every operation |
| 14 | Child-resource ID substitution | Access attachment/version under owned parent | Verify child belongs to authorized parent and independently enforce scope |
| 15 | List/count/cursor enumeration | Infer other user resources | Apply scope before count/filter/cursor and use consistent concealment |
| 16 | Client changes role/permission/owner | Vertical or horizontal escalation | Ignore client authority fields; derive principal and effective permissions server-side |
| 17 | Admin self-elevation | Gain content/security control | Separate permissions, reauth, concurrency, audit, segregation-of-duties policy |
| 18 | Administrator opens user documents | Abuse powerful role | Separate content-access permission; authorize before storage resolution |
| 19 | Support follows JobId/fileRef to source | Access original user document | Correlation IDs are diagnostic only; Issue attachment is explicit isolated resource |
| 20 | Auditor export expands beyond authorized scope | Bulk disclosure | Separate export permission, bounded filters, authorization before count/query/export |
| 21 | Generic Feature endpoint handler injection | Execute privileged/arbitrary code | Closed registry, canonical Feature ID, typed schema, per-Feature permission/policy |
| 22 | Assistant prompt asks to bypass policy | Privileged tool execution/data access | Assistant only orchestrates registered contract under user authorization |
| 23 | Mass assignment in admin/resource request | Mutate protected owner/role/state fields | Strict DTO allowlist, `additionalProperties=false`, server-derived fields |
| 24 | CSRF on cookie-authenticated mutation | Unauthorized action using browser cookie | Anti-forgery validation, safe methods, SameSite, Origin policy |
| 25 | Permissive CORS | Malicious origin reads/calls Web surface | Explicit HTTPS origins, no wildcard credentials, server authorization still required |
| 26 | WebSocket cross-site hijack/arbitrary group | Receive another user's notifications | Authenticate negotiate, validate origin/session, server-derived groups, subscription authorization |
| 27 | SignalR query-token logging | Token disclosure via infrastructure logs | Hub-path restriction and end-to-end query/token redaction |
| 28 | Stale Hub connection after revocation | Continue receiving metadata | Revocation-aware disconnect, bounded reconnect/connection policy, safe events only |
| 29 | Queue message spoof/replay | Worker runs unauthorized job twice | Workload auth, trusted producer, schema/state/lease/idempotency/sequence checks |
| 30 | Worker confused deputy | Use Worker privilege for arbitrary object/provider | Job-scoped contract, closed handler, scoped object access, no client URL/path |
| 31 | Service credential theft | Lateral movement | Short-lived scoped workload identity, approved credential boundary, rotation and alerting |
| 32 | Storage reference disclosure | Direct content download | Authorize before resolution; time/scope-bound storage access; no raw key response/log |
| 33 | Brute force/token spraying | Resource exhaustion or account probing | IdP controls plus API rate limits, generic errors, safe telemetry, IP supplemental only |
| 34 | Refresh/reconnect storm after outage | Availability/security dependency exhaustion | Single-flight refresh, bounded jittered retry, connection/admission limits |
| 35 | Stale permission cache | Retain removed privilege | Version/invalidation, bounded cache, current check for sensitive actions, offline expiry |
| 36 | Error/log/trace leakage | Expose token, claim, document, path, or policy | Central redaction, safe error envelope, security log tests, least-data diagnostics |

---

# 13. Mitigations

## 13.1 Preventive controls

| Control family | Required controls |
|---|---|
| Identity | Approved Pertamina SSO only; exact issuer/audience/claim allowlist; no local passwords; PKCE for native; callback transaction binding |
| Token/session | HTTPS; protected credential storage; no raw token in SQL/SQLite/browser storage/log; bounded lifetime; refresh coordination; hashed session metadata; explicit revoke/logout |
| Offline | Per-profile protection; minimum authorization projection; absolute expiry; no offline privileged/server actions; clock rollback defense; reconnect revalidation |
| Authorization | Global deny; permission + ownership/scope + policy/classification + state + reauth; server-side resource handlers; role only grouping |
| Content boundary | Separate content-access capability; authorize before storage resolution; no administrator/support/auditor implication; explicit issue attachment only |
| API contract | Typed/versioned DTO; closed Feature dispatch; server-derived owner/principal; no arbitrary URL/path/SQL/handler/provider; safe HTTP/error semantics |
| Web | Server-side credential/session boundary; secure cookie; anti-forgery; strict origin/CORS; session rotation |
| SignalR | Authenticated Hub; server-derived groups; subscription owner check; safe events; REST reconciliation; token-log redaction |
| Worker/service | Separate short-lived workload identity; no user-token forwarding; scoped queue/object/module access; closed job contract |
| Availability/abuse | Layered rate limit; bounded queue/retry; jitter; security dependency fail closed; no account enumeration |

## 13.2 Detective controls

- Record safe authentication/session events: successful login/session start, logout, failed authentication category, refresh terminal failure category, revocation, disablement learned, and reauthentication outcome.
- Record safe authorization events for high-risk denies and privileged actions without flooding logs or leaking target existence.
- Detect repeated invalid issuer/audience/signature, token replay indicators supplied by the provider, refresh reuse, cross-owner requests, denied Feature IDs, arbitrary SignalR groups, session-revocation misses, and service-identity audience failures.
- Correlate via `CorrelationId`, internal `PrincipalId`, hashed/session reference, `JobId`, canonical `FeatureId`, action category, outcome, and timestamp. Do not log credentials or content.
- Alert on suspicious permission/role changes, self-assignment attempts, mass export, unusual support attachment access, repeated concealed-object probes, and Worker access outside assigned job scope.
- Keep enterprise audit separate from personal History, support messages, and observability as locked by Document 07.

## 13.3 Responsive controls

- Revoke the affected application session/grant through approved mechanisms and terminate known SignalR connections.
- Invalidate effective-permission/offline projections and require fresh online validation.
- Rotate compromised workload credentials and stop the affected consumer/provider path without degrading to a weaker identity.
- Quarantine, cancel, or withhold output from suspicious jobs according to approved job-security policy.
- Preserve safe evidence under Document 10; never copy tokens or restricted document content into an incident ticket/log.
- Use generic user-facing errors and route recovery through approved Pertamina Identity or Support processes.

## 13.4 Secure defaults

- Unknown identity, claim, role, permission, Feature, resource state, client version, policy version, or service caller: deny.
- Missing online validation for a server/sensitive action: wait/deny; never reuse offline authorization.
- Missing content-access permission: metadata-only or deny; never infer from admin/support/audit.
- Security dependency unavailable: fail closed with safe `503`; do not use stale server authorization beyond approved cache policy.
- Unknown SignalR event/version: ignore safely and reconcile REST.
- Unknown queued command/job handler: do not execute.

---

# 14. Security acceptance criteria

The following are report-local candidate acceptance IDs; they are not new Requirement or Feature IDs. Numeric timing/load thresholds remain Test Plan inputs.

## 14.1 Authentication and session

- **SEC08-AC-001:** A token with invalid signature, disallowed algorithm, wrong issuer, wrong audience, missing subject, premature validity, or expiry is rejected with `401` and a minimal error envelope.
- **SEC08-AC-002:** Desktop/Mobile authentication uses the approved PKCE flow; a missing/mismatched verifier, state/nonce, redirect, or transaction is rejected.
- **SEC08-AC-003:** The Web browser cannot access an access token or refresh token through JavaScript, Web Storage, URL, page source, browser log, or service-worker cache.
- **SEC08-AC-004:** Access/refresh tokens are absent from SQL Server, SQLite, configuration files, logs, telemetry, issue diagnostics, crash reports, SignalR events, and API errors.
- **SEC08-AC-005:** Concurrent refresh attempts collapse into one controlled operation; terminal refresh failure does not loop or create a request storm.
- **SEC08-AC-006:** Refresh reuse/replay signals from the approved IdP cause the affected session/grant to be rejected and require interactive recovery.
- **SEC08-AC-007:** A revoked application session is rejected on API and on new SignalR connections; known live Hub connections are terminated within the approved propagation objective.
- **SEC08-AC-008:** A disabled/ineligible application principal cannot query, count, mutate, download, subscribe, export, or start a job even if the presented identity assertion is otherwise valid.
- **SEC08-AC-009:** Logout revokes/clears the application session, stops SignalR, invalidates offline authorization, and locks/unmounts the local profile without deleting user source files.
- **SEC08-AC-010:** Web session identifiers change after login, reauthentication, and material privilege-context change; a fixed pre-login identifier is unusable.

## 14.2 Offline session and reconnect

- **SEC08-AC-011:** A valid offline authorization state permits only allowlisted local capabilities and is never accepted by API/SignalR.
- **SEC08-AC-012:** Expired, tampered, wrong-profile, wrong-device-bound, missing-version, or rollback-extended offline state fails closed.
- **SEC08-AC-013:** Reboot, sleep, hibernate, time-zone change, forward clock change, and rollback cannot extend the absolute offline lifetime.
- **SEC08-AC-014:** Administrator, Support/Superadmin, Auditor, server upload/storage, and sensitive reauthentication-required actions are unavailable offline.
- **SEC08-AC-015:** Account switching cannot expose another account's claims, permissions, favorites, history, Assistant state, drafts, secured cache metadata, or protected artifacts.
- **SEC08-AC-016:** On reconnect, identity/account/session/permission/policy/capability/compatibility are validated before any queued server command is sent.
- **SEC08-AC-017:** A queued offline command is rejected safely if permission, ownership, policy, classification, resource state, consent, or client compatibility changed.

## 14.3 Reauthentication and authorization

- **SEC08-AC-018:** A sensitive endpoint rejects a valid but insufficiently fresh session with `401 REAUTHENTICATION_REQUIRED` before mutation.
- **SEC08-AC-019:** A reauthentication context cannot be replayed by another account/session/client or for a different action/resource beyond its approved scope/lifetime.
- **SEC08-AC-020:** Reauthentication never grants a permission the principal did not already have.
- **SEC08-AC-021:** All protected routes use deny-by-default server authorization; removing a UI button does not constitute the control.
- **SEC08-AC-022:** Role name alone cannot authorize a resource action; effective permission and resource scope are independently required.
- **SEC08-AC-023:** Cross-user IDOR tests fail for upload, job, result, artifact, history, secured file/version, issue, attachment, audit/export, and SignalR subscription.
- **SEC08-AC-024:** Authorization is applied before list count, filter/search, cursor generation, storage lookup, preview, range read, download, and export.
- **SEC08-AC-025:** An Administrator without a separate content-access capability cannot open or download any user's document/result/secured payload.
- **SEC08-AC-026:** Support can access only issue message and explicitly submitted attachments permitted by its granular support permissions; Job ID/Correlation ID does not unlock source/result content.
- **SEC08-AC-027:** Auditor can query/export only approved audit evidence scope and cannot delete audit or open document content by role implication.
- **SEC08-AC-028:** Self-assignment/self-elevation and stale concurrent permission-policy updates are denied according to the approved administrative policy.
- **SEC08-AC-029:** A client-supplied role, permission, owner, subject, policy result, or reauthentication flag is ignored/rejected.
- **SEC08-AC-030:** Unknown permission/Feature/policy/schema/handler state fails closed.

## 14.4 API, Assistant, SignalR, and service identity

- **SEC08-AC-031:** Canonical Feature execution reaches only its registered typed handler and always enforces Feature permission, input ownership, policy/classification, capability, consent, and state.
- **SEC08-AC-032:** Assistant execution produces the same authorization outcome as direct UI/File-First execution for the same principal, Feature, parameters, resource, and policy.
- **SEC08-AC-033:** API errors always contain `code`, `message`, and `correlationId` and never expose credentials, claims dump, policy internals, resource existence detail beyond the concealment policy, content, path, SQL, or storage key.
- **SEC08-AC-034:** State-changing cookie-authenticated Web requests fail without a valid anti-forgery proof; no `GET`/`HEAD` mutates state.
- **SEC08-AC-035:** Disallowed CORS origins receive no credentialed access; allowed Origin never bypasses authentication/authorization.
- **SEC08-AC-036:** SignalR rejects unauthenticated/expired/revoked callers, derives groups server-side, and rejects arbitrary group or unauthorized job subscriptions.
- **SEC08-AC-037:** SignalR events are safe, versioned, idempotently consumable notifications only; duplicate/stale/gap/unknown events cannot cause a business mutation and REST reconciliation wins.
- **SEC08-AC-038:** Any SignalR query-carried token is absent from application, proxy, gateway, WAF, telemetry, and issue logs.
- **SEC08-AC-039:** Worker/provider calls use a distinct scoped workload identity; end-user access/refresh tokens are absent from queue messages and downstream requests.
- **SEC08-AC-040:** A spoofed/replayed/malformed queue message, wrong service audience, expired workload credential, invalid lease, wrong Job state, unknown handler, or out-of-scope storage reference cannot execute work.
- **SEC08-AC-041:** Revoking a user after job admission prevents that user from retrieving or controlling the result; workload-specific finish/cancel behavior matches the approved policy.
- **SEC08-AC-042:** Authentication/session/reauth/SignalR abuse is rate-limited with bounded retry and safe `429` semantics without revealing account existence or relying on IP as sole identity.
- **SEC08-AC-043:** Security dependency loss produces a safe closed failure, not anonymous access, stale privileged authorization, public-AI fallback, or direct client SQL/storage access.

---

# 15. Open risks

| ID | Open item / risk | Impact if unresolved | Required owner/decision |
|---|---|---|---|
| `IAM-RISK-001` | Exact Pertamina Identity protocol/profile, issuer, audiences, client registrations, redirect URIs, token type, metadata/key refresh | Authentication cannot be configured or tested safely | Pertamina IAM + Security + Application |
| `IAM-RISK-002` | Approved identity claim contract and stable subject mapping | Duplicate/wrong principal or claim injection | Pertamina IAM + Identity module |
| `IAM-RISK-003` | Unknown-subject provisioning lifecycle | Unauthorized auto-provisioning or valid employees locked out | Business owner + IAM + Account Administration |
| `IAM-RISK-004` | Access/refresh/session lifetimes, inactivity/absolute limits, clock skew, refresh window | Excessive exposure or poor usability/refresh storm | Pertamina IAM/Security policy |
| `IAM-RISK-005` | Refresh-token issue/rotation/sender-constraint/reuse behavior by client type | Persistent token replay risk | Pertamina IAM |
| `IAM-RISK-006` | Revocation and disabled-account propagation mechanism/SLA | Valid token may outlive desired access | Pertamina IAM + API/Identity |
| `IAM-RISK-007` | Final `401` versus `403` code for locally disabled but cryptographically valid identity | Inconsistent client recovery and test contract | Document 08/06 controlled clarification |
| `IAM-RISK-008` | Offline lifetime by device/risk/classification and trusted-time strategy | Excessive offline access or unusable Local-First behavior | Security + Product + Platform |
| `IAM-RISK-009` | Approved local encryption/credential/key provider, WAL/SHM/crash/backup protection, device-loss behavior | Tokens/claims/local artifacts may leak | Security/dependency approval; Document 08 |
| `IAM-RISK-010` | Logout handling for app-managed data keys/artifacts and optional read-only recovery | Data exposure or destructive loss | Security + Product + Records |
| `IAM-RISK-011` | Reauthentication trigger catalog, max age, accepted assurance/method, failure/lock behavior | Sensitive actions under- or over-protected | Security + Business; Document 09/10 input |
| `IAM-RISK-012` | Final permission catalog, role mappings, scoped assignments, segregation of duties, self-elevation rule | Vertical escalation or unimplementable administration | Document 09 |
| `IAM-RISK-013` | Any legitimate non-owner content access, delegated/team scope, or emergency access | Accidental blanket admin/support access if left implicit | Business + Security + Legal; explicit future decision only |
| `IAM-RISK-014` | Accepted-job behavior after user/session revoke or policy change | Waste, processing of now-forbidden content, or result exposure | Security + Processing/Jobs by workload class |
| `IAM-RISK-015` | Workload identity mechanism, audience/scopes, rotation, queue/storage authorization | Lateral movement or confused deputy | Platform Security + Deployment |
| `IAM-RISK-016` | Final Web host/domain/session topology and CORS origin inventory | CSRF/CORS/session-domain weakness | Deployment + Web Security |
| `IAM-RISK-017` | SignalR token transport, connection maximum age, revocation disconnect channel/SLA | Token logging or stale connection after revoke | API/Realtime + Platform Security |
| `IAM-RISK-018` | Official Pertamina data classification and content-access handling matrix | Inconsistent owner/admin/support/audit policy | Security/Compliance; Document 08 master |
| `IAM-RISK-019` | Audit event catalog, privileged-access evidence, retention/legal hold | Insufficient evidence or excess sensitive logging | Document 10 |
| `IAM-RISK-020` | Numeric abuse thresholds, lockout interactions, false-positive and DoS resistance | Brute force/resource exhaustion or user lockout | Security Operations + Test Plan |

None of these open items authorizes a weaker interim default. Until approved, the applicable action fails closed or remains unavailable.

---

# 16. Conflicts with source documents

## 16.1 Material contradiction result

```text
Material contradiction against Documents 01–07 : NONE FOUND
Architecture lock changed                       : NO
New IAM vendor/provider selected                : NO
Final Role Matrix created                       : NO
Production code introduced                      : NO
```

## 16.2 Clarifications and apparent conflicts

| Topic | Source observation | Finding / required interpretation |
|---|---|---|
| “Superadmin” terminology | Document 01 starts with User/Administrator/Auditor; Documents 01/03/04 use Superadmin for Support UI; Document 07 explicitly creates no Superadmin role | Not a material conflict if “Superadmin” remains a user-facing Support permission bundle. Document 09 must not silently create an all-powerful role from the label. |
| Login once versus expiry/reauthentication | Documents 01/02 require one login/persistent session and also revocation/reauthentication | Not a conflict. Login once means silent approved renewal while eligible, not infinite authentication or bypass of sensitive reauthentication. |
| Offline role change versus revocation | Document 01 says role changes apply at synchronization; prompt requires revoked/disabled handling | Immediate enforcement cannot be guaranteed on a disconnected device. Risk must be bounded by local-only permissions and absolute offline expiry; current authorization is restored before queued server work. |
| Token storage | Document 06 allows native secure OS credential storage; Document 07 prohibits tokens in SQL/SQLite | No conflict. Credential storage is an approved security boundary outside ordinary SQL/SQLite. `identity.Sessions` stores only hashed session/revocation metadata. |
| Web session versus bearer API | Document 06 accepts approved bearer/session assertion and recommends a Web server-side session boundary | No conflict. Browser-to-Web may use a protected cookie while server-to-API uses the approved assertion; browser-accessible bearer/refresh storage remains prohibited. |
| Authentication Feature IDs | Document 03 catalogs `FEAT-AUTH-001..012`; Document 06 uses resource/security authentication flow and closed Feature execution | Clarify that catalog IDs provide traceability/capability identity. Login/logout/token/session are not arbitrary generic Feature executions and do not require a processing endpoint for local-only work. |
| Administrator session revoke | Document 01 says Administrator can revoke sessions; Documents 02/06/07 require permission-based authorization | Interpret as an administrative capability that Document 09 grants through an explicit permission, not unconditional role hardcoding. |
| Disabled account HTTP status | Documents 01–07 define revocation and general `401/403`, but do not explicitly define locally disabled principal semantics | Gap, not contradiction. This report recommends `403 ACCOUNT_DISABLED` for a valid identity denied by local account policy, while invalid/revoked token/session remains `401`; final API/security integration must confirm. |
| Offline lifetime and reauth age | Sources require configurability and security but provide no values | Intentional open policy, not a contradiction. Do not invent numeric values in Document 08 without Pertamina approval. |
| Local encryption/logout details | Document 07 delegates provider/key/WAL/backup/logout behavior to Document 08; this task prohibits vendor selection | Document 08 can set security properties and release gates; dependency/vendor selection remains a controlled approval outside this specialist report. |
| Auditor/support content | Documents 01, 04, 06, and 07 consistently deny automatic document access | No conflict. Audit/support metadata and explicitly submitted issue attachments remain separate from user content and secured-file/job-result access. |

---

# 17. Suggested text for Document 08

The following text is ready for controlled integration into the Document 08 master. It defines the model and rules but intentionally leaves the final Role Matrix to Document 09.

## 17.1 Identity and trust statement

PERTAMINA OTO shall use only the approved Pertamina Identity service for human authentication. The application shall not create a parallel password identity, collect Pertamina credentials, or select another identity provider without governed approval. Pertamina Identity is authoritative for authenticated subject identity; the PERTAMINA OTO API is authoritative for application authorization.

A valid identity assertion proves identity only after signature/reference validation, allowed algorithm/profile, issuer, audience, lifetime, subject, and approved claim-contract validation. It does not by itself grant a Feature, role, permission, resource, document, job, support, audit, configuration, or SignalR subscription.

The API shall map the validated issuer and subject to an internal opaque `PrincipalId`, then verify account eligibility, application session expiry/revocation, effective permission, resource ownership/scope, enterprise policy/classification, resource state/version, client compatibility/capability, consent, and reauthentication freshness where required. Absence or uncertainty shall deny access.

Public IDs, `FeatureId`, `JobId`, `CorrelationId`, `fileRef`, cursor, ETag, storage reference, SignalR connection ID, and group name are locators or trace identifiers only and shall never be accepted as authorization proof.

## 17.2 Authentication and session rules

Native Desktop and Mobile/Tablet clients shall use the approved Pertamina authorization flow with PKCE and approved operating-system credential storage. Authorization codes and PKCE verifiers shall be transaction-bound and memory-only. Access and refresh credentials shall not be stored in SQL Server, SQLite, browser-accessible storage, logs, telemetry, URLs, issue diagnostics, crash data, or exports.

Web shall use a server-side credential/session boundary where compatible with the approved architecture. The browser shall receive only an opaque `Secure`, `HttpOnly`, narrowly scoped session cookie with the strictest compatible `SameSite` policy. The session identifier shall rotate after login, reauthentication, and material privilege-context change. Browser JavaScript shall not receive an access or refresh token.

Access tokens/session assertions shall be validated on every API request and initial SignalR connection. Refresh shall occur only through the approved Pertamina Identity boundary, use single-flight coordination and bounded retry, and stop on invalid grant, replay/reuse indication, revocation, disablement, or required interaction. A refreshed identity shall cause current PERTAMINA OTO permissions and policy to be re-evaluated.

“Login sekali” means silent approved renewal while the enterprise grant, application session, account, policy, and client remain eligible. It does not mean an unlimited session and does not bypass reauthentication.

Logout shall stop SignalR, revoke/invalidate the application session where reachable, clear credentials, invalidate offline authorization, clear the Web session cookie/server session, and lock/unmount the local profile. Logout shall not delete user source files. Identity-provider-wide logout and app-managed key/artifact deletion shall follow approved enterprise and retention policy.

Missing, invalid, expired, or revoked authentication and required reauthentication shall return safe `401` semantics. A valid identity denied by permission, ownership, account policy, or enterprise policy shall return safe `403` or a consistently concealed `404` for protected resource existence. Errors shall contain at minimum `code`, `message`, and `correlationId` and no sensitive detail.

## 17.3 Offline-session rules

Offline authorization shall be a protected, time-bounded, per-profile/device projection of the minimum local capabilities required for eligible local work. It shall contain issue/validation/expiry time, relevant permission/policy/capability versions, safety restrictions, integrity metadata, and profile/device binding where supported. It shall not contain an access token, refresh token, password, cookie, service secret, unrestricted claim dump, document content, or privileged server permission.

Offline authorization shall permit only approved local capabilities while both the session and relevant claims/policy remain valid. It shall never authenticate or authorize an API, SignalR, upload, secured-file operation, support/admin/audit surface, or action requiring current online authorization or reauthentication. Its lifetime shall be configurable and absolute; it shall not slide or renew while offline.

The client shall detect clock rollback using trusted last-online time and monotonic elapsed time where available. If expiry or time integrity cannot be established, protected local actions shall fail closed and require online validation. Account switching shall unmount the prior profile and prevent claims, history, drafts, Assistant state, preferences, or protected artifacts from crossing profiles.

After reconnect, the client shall authenticate online and refresh account/session/permission/policy/capability/compatibility state before sending queued server commands. Every queued command shall be reauthorized against current ownership, classification, resource state, consent, and reauthentication policy. Queued intent is not a previous authorization grant.

## 17.4 Reauthentication rules

Sensitive reauthentication shall be enforced by the API authorization policy before mutation. It shall confirm recent approved interactive authentication for the current principal/session and shall not grant any new permission. If an approved Pertamina step-up/fresh-authentication method is unavailable, the user shall complete a new approved interactive login; PERTAMINA OTO shall not collect the enterprise password.

The reauthentication context shall be short-lived, bound to the current principal, application session, action category, resource/scope, and policy version, and invalidated by logout, revocation, disablement, or material authorization change. It shall not be a reusable global elevation mode. Exact trigger actions, authentication age, assurance method, and retry behavior require approved policy and Document 09/10 alignment.

## 17.5 Authorization and permission rules

Authorization shall be deny-by-default, permission-based, policy-based, and object-aware. Role shall only group permissions; role name shall not be the final access decision. ASP.NET Core policies/requirements shall express reusable permission and security-state checks, while resource authorization handlers shall enforce ownership/scope, classification, resource state, and action-specific rules before content access or mutation.

Authorization shall occur before list count, pagination, filter/search, cursor generation, storage resolution, preview, download/range processing, export, retry/resume/cancel, checkpoint access, and SignalR subscription. Every continuation shall be reauthorized. Client-supplied role, permission, owner, subject, policy result, reauthentication flag, handler, path, URL, SQL, engine, or provider shall be rejected or ignored.

The generic Feature execution contract shall dispatch only a canonical `FeatureId` to its registered typed/versioned handler. It shall verify the Feature permission, parameter schema, input ownership, capability/exposure, classification/policy, consent, and state. Assistant execution shall use the identical application contract and current user authorization; prompt text and workflow plans cannot grant permission or bypass the API.

The final permission catalog and role mapping shall be defined in Document 09. `User`, `Administrator`, and `Auditor` remain baseline groupings. “Superadmin” shall remain a user-facing Support permission bundle and shall not become an automatic all-powerful role.

## 17.6 Administrative, Support, Auditor, and content boundary

Administrative permission and content-access permission shall be independent. Administrator, Support/Superadmin, Auditor, and Operations shall not automatically open user documents, job inputs/results, secured-file payloads, full OCR, PDF passwords, clipboard data, signature assets, tokens, or secrets.

Administrator actions shall use granular permissions for session, user, role, permission, feature, policy, configuration, limit, compatibility, and monitoring capabilities. High-impact security changes shall require current permission, reauthentication, optimistic concurrency, and safe audit evidence. Administrator shall not alter canonical Feature semantics, disable mandatory audit, bypass hard safety ceilings, write across module-owned data, or silently self-grant content access.

Support/Superadmin shall use separate permissions for issue list, detail, explicitly submitted attachment, triage, status, internal note, and export. An Issue `JobId` or `CorrelationId` shall support safe diagnostics only and shall not unlock original documents, jobs, results, or secured files. No arbitrary impersonation, “view as user”, storage browsing, or break-glass content access exists unless a separate governed capability is explicitly approved.

Auditor shall receive only approved audit metadata query/detail/export capabilities and shall not delete audit evidence or gain document-content access by role implication. Authorization shall occur before audit count/query/export. Personal History deletion shall not affect Enterprise Audit.

## 17.7 API, Web, SignalR, and abuse controls

Protected API, MVC, and SignalR surfaces shall require authenticated fallback authorization except explicitly documented minimal compatibility and health endpoints. Cookie-authenticated state-changing Web requests shall validate anti-forgery proof; `GET` and `HEAD` shall not mutate state. CORS shall deny by default, allow only explicit HTTPS origins, never combine wildcard origins with credentials, and shall not be treated as authentication, authorization, or the sole CSRF defense.

SignalR `/hubs/activity` shall authenticate the same principal as REST, derive user groups server-side, authorize every job subscription, and carry only safe versioned metadata after state persistence. SignalR shall not execute business commands and shall never be the source of truth. Reconnect shall authenticate and authorize again, then reconcile through REST. Query-carried Hub tokens, when required by an approved browser transport, shall be restricted to the Hub path and redacted from every infrastructure/application log.

Authentication/session/reauthentication/SignalR endpoints shall use layered abuse controls, bounded retry, and safe `429` responses. Pertamina Identity remains responsible for enterprise credential brute-force controls; PERTAMINA OTO shall rate-limit abuse of its own boundaries without exposing account existence, relying on IP only as a supplemental signal.

## 17.8 Service and Worker rules

API, Worker, queue consumers, storage adapters, export processors, and approved internal providers shall use separate short-lived, audience-restricted, least-privilege workload identities where their deployment boundaries require identity. End-user access or refresh tokens shall never be forwarded to Worker, queue, storage, or provider.

The API shall authorize the user and persist the Job before dispatch. The Worker shall authenticate independently, claim only an eligible persisted Job, validate handler/schema/state/lease and job-scoped artifacts, and write only through the owning application/module contract. Internal networks and queue messages shall not be implicitly trusted. Storage references, Job IDs, and persisted authorization context shall not be unrestricted capability tokens.

Service credentials shall reside only in an approved credential/key boundary, support rotation/revocation, and never appear in source, SQL/SQLite, package/image, logs, telemetry, issue diagnostics, or client configuration. Exact workload-identity technology remains an approved deployment/security decision and no vendor is selected by this document.

## 17.9 Mandatory security gates

Before Production, automated and adversarial tests shall demonstrate:

- invalid issuer/audience/signature/algorithm/lifetime/subject and revoked sessions fail closed;
- PKCE/callback transaction binding, refresh concurrency/replay handling, logout, session fixation, and disabled-account behavior;
- offline expiry, tamper, rollback, account switch, and reconnect reauthorization;
- server-enforced sensitive reauthentication with no new permission;
- horizontal IDOR and vertical role/permission/self-elevation prevention;
- strict Administrator/Support/Auditor versus content-access separation;
- equivalent authorization for UI, File-First, Assistant, workflow, retry, export, and download paths;
- anti-forgery and explicit CORS behavior;
- SignalR authenticated groups, subscription ownership, token-log redaction, revocation, and REST reconciliation;
- separate scoped Worker/service identity with no user-token forwarding;
- credentials, claims dumps, restricted content, storage references, and policy internals absent from logs/errors/events;
- rate limits and dependency failures fail safely without anonymous access, stale privilege, direct SQL/storage access, or public-AI fallback.

Numeric token/session/offline/reauthentication/connection lifetimes, propagation objectives, origin inventory, claims mapping, final permission codes, role mappings, segregation of duties, content-access exception policy, and workload-identity technology shall remain governed approvals and shall not be inferred by implementation teams.
