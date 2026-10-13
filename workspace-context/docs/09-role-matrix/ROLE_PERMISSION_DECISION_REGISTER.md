# ROLE & PERMISSION DECISION REGISTER
## PERTAMINA OTO — Office Tools Organizer

> **Document:** 09 Role Matrix  
> **Version:** 1.0  
> **Date:** 26 Agustus 2026  
> **Status:** 18/18 AUTO-LOCKED  
> **Decision mode:** Technical derivation from Documents 01–08 FINAL  
> **Canonical feature baseline:** 550

---

# 1. Lock Rule

The decisions below preserve the existing business roles, security architecture, API and database boundaries, Local-First behavior, Assistant boundary, and non-PKI scope. They do not add a fourth formal role, privileged document access, an IAM vendor, a new Feature ID, or a hidden authorization path.

Unknown Pertamina-specific values are marked `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`. Missing confirmation always means deny, unavailable, or use the narrower safe behavior; it never means fail open.

---

# 2. Decision Summary

| ID | Decision | Status |
|---|---|---|
| `ROLE-DEC-001` | Five-dimension authorization formula | **AUTO-LOCKED** |
| `ROLE-DEC-002` | Three formal roles only | **AUTO-LOCKED** |
| `ROLE-DEC-003` | No implicit role hierarchy | **AUTO-LOCKED** |
| `ROLE-DEC-004` | Superadmin is a Support permission profile | **AUTO-LOCKED** |
| `ROLE-DEC-005` | Compact action-permission catalog | **AUTO-LOCKED** |
| `ROLE-DEC-006` | Canonical permission naming | **AUTO-LOCKED** |
| `ROLE-DEC-007` | Feature-family mapping, not permission per Feature | **AUTO-LOCKED** |
| `ROLE-DEC-008` | Permission definition and assignment ownership | **AUTO-LOCKED** |
| `ROLE-DEC-009` | Resource ownership and explicit scope | **AUTO-LOCKED** |
| `ROLE-DEC-010` | Administrative/content separation | **AUTO-LOCKED** |
| `ROLE-DEC-011` | Minimum role bundles | **AUTO-LOCKED** |
| `ROLE-DEC-012` | Assignment, revocation, and segregation of duties | **AUTO-LOCKED** |
| `ROLE-DEC-013` | Sensitive action and reauthentication model | **AUTO-LOCKED** |
| `ROLE-DEC-014` | Offline authorization boundary | **AUTO-LOCKED** |
| `ROLE-DEC-015` | Assistant authorization parity | **AUTO-LOCKED** |
| `ROLE-DEC-016` | API, UI, SignalR, and Worker enforcement | **AUTO-LOCKED** |
| `ROLE-DEC-017` | Workload identity and permission cache separation | **AUTO-LOCKED** |
| `ROLE-DEC-018` | Authorization test and Audit handoff | **AUTO-LOCKED** |

---

# 3. Locked Decisions

## ROLE-DEC-001 — Five-Dimension Authorization Formula

**Decision**

```text
ALLOW = authenticated and active subject
    AND valid session or eligible protected offline grant
    AND action permission
    AND resource ownership or explicit scope
    AND enterprise policy and classification
    AND valid resource state/version
    AND compatible client/platform/capability
    AND required consent/confirmation
    AND fresh reauthentication when required
```

**Reason**

- Directly preserves Documents 06 and 08.
- Role name, UI visibility, IDs, paths, and previous decisions are never authority.

## ROLE-DEC-002 — Three Formal Roles Only

**Decision**

Formal roles remain exactly:

```text
User
Administrator
Auditor
```

**Reason**

- Required by `FR-ROLE-001`.
- No approved business gap requires a new formal role.

## ROLE-DEC-003 — No Implicit Role Hierarchy

**Decision**

`Administrator` and `Auditor` do not inherit `User`. A principal who performs more than one approved duty receives the corresponding role/profile explicitly, subject to segregation-of-duties policy.

**Reason**

- Prevents administrative or audit authority from becoming document-processing authority.
- Keeps grants visible and testable.

## ROLE-DEC-004 — Superadmin Is a Support Permission Profile

**Decision**

“Superadmin” is a user-facing Support label for an approved set of granular Support permissions. It is not a formal role, hierarchy level, wildcard, root account, impersonation mode, or content-access bypass.

**Reason**

- Reconciles `FR-SUP-005` with `FR-ROLE-001` and Document 08.
- Preserves Issue Console duties without creating God Mode.

## ROLE-DEC-005 — Compact Action-Permission Catalog

**Decision**

Permissions represent independently governable business/security actions. They are not created for every button, endpoint, platform, engine, or canonical Feature record. Read, mutation, content download, export, destructive action, privileged assignment, and Support attachment access remain separate where risk materially differs.

**Reason**

- Avoids permission sprawl.
- Retains least privilege and independent testing for meaningful risks.

## ROLE-DEC-006 — Canonical Permission Naming

**Decision**

Permission codes use exactly:

```text
domain.resource.action
```

Rules: lower-case ASCII, stable three segments, singular logical resource, controlled action vocabulary, no role name, owner, unit, environment, ID, wildcard, `all`, `any`, `bypass`, or localized text.

**Reason**

- Avoids ambiguous resource names and scattered strings.
- Maps cleanly to ASP.NET Core policies and data-driven tests.

## ROLE-DEC-007 — Feature-Family Mapping, Not Permission per Feature

**Decision**

Ordinary PDF, conversion, image, scanner, OCR, annotation, and productivity transformations share `document.content.process`. Sensitive exceptions add a second action permission for Protect, Unlock, visual Sign, Redact, approved Intelligence, server content transfer, share, overwrite, or permanent deletion.

Canonical Feature ID still controls discovery, schema, handler, exposure, platform, capability, policy, and traceability.

**Reason**

- Preserves all 550 Features without maintaining 550 nearly identical permission codes.
- Prevents one UI card from creating a parallel authorization surface.

## ROLE-DEC-008 — Permission Definition and Assignment Ownership

**Decision**

Permission definitions and meanings are release/migration-owned and versioned. Authorized Administrators may assign or revoke approved definitions within delegation scope; they cannot create arbitrary permission codes or redefine semantics in the UI.

**Reason**

- Preserves Document 07 ownership of `admin.Permissions`.
- Prevents runtime semantic drift and self-created privilege.

## ROLE-DEC-009 — Resource Ownership and Explicit Scope

**Decision**

Permission only grants the right to attempt an action. The server/application derives ownership and typed scope from authenticated identity and authoritative data. Cross-user access is denied unless a documented scope exists for Administrator metadata, Support cases, Auditor evidence, or workload job data.

Lists are scoped before count/filter/cursor; children are validated against parents; storage/content is resolved only after authorization.

**Reason**

- Prevents IDOR and metadata leakage.
- Preserves owner/reporter and module ownership rules.

## ROLE-DEC-010 — Administrative/Content Separation

**Decision**

Administrator, Support, Auditor, Operations, DBA, backup, and Worker authority does not imply access to user source files, job results, secured-file bytes, OCR text, Assistant content, visual signatures, clipboard data, passwords, tokens, secrets, or unrelated Issue attachments.

No privileged user-document access or break-glass content workflow exists in the current scope.

**Reason**

- Required by `SR-SEC-007`, `SR-SEC-020`, and Document 08.
- Avoids hidden cross-user content access.

## ROLE-DEC-011 — Minimum Role Bundles

**Decision**

- `User`: approved tools and own profile, workspace, jobs/results, history, secured files, Issues, preferences, and Assistant surface.
- `Administrator`: application account eligibility/session, approved role/permission assignment, typed configuration/policy/Feature/limit/compatibility, and safe operational metadata.
- `Auditor`: scoped safe Audit evidence read; bounded Audit export is separately grantable.
- Support permissions are separately assigned and are not a formal-role default.

All entries remain subject to scope, policy, state, online, confirmation, and reauthentication conditions.

**Reason**

- Matches Documents 01–03 without role inflation.
- Keeps bulk disclosure and privileged mutation explicit.

## ROLE-DEC-012 — Assignment, Revocation, and Segregation of Duties

**Decision**

Privileged assignment is online, backend-authoritative, scoped, versioned, reauthenticated, concurrency-protected, and denies self-assignment or grants beyond delegated authority. Revocation invalidates new online decisions, permission/session caches, and SignalR when known.

Support grants must come from one versioned backend mapping source with scope and revocation. Exact approved IAM-group mapping or future scoped principal-grant persistence is a governed implementation handoff; until configured, no Support grant exists.

**Reason**

- Prevents vertical privilege escalation and competing authorities.
- Avoids silently changing Document 07 or inventing a Superadmin role.

## ROLE-DEC-013 — Sensitive Action and Reauthentication Model

**Decision**

Fresh reauthentication proves identity freshness only; it never grants permission. It is required for role/permission assignment or revocation, another user's session revocation, high-impact security/classification policy change, permanent secured-content deletion, key/destructive recovery, and any future exceptional content access.

High-impact Support/Audit export, broad rollout/limit/retention change, and high-classification disclosure are policy-conditional. Explicit confirmation remains separate from reauthentication.

**Reason**

- Preserves offline usability for ordinary own-resource local work.
- Protects systemic, privilege, and irreversible actions.

## ROLE-DEC-014 — Offline Authorization Boundary

**Decision**

Offline uses a protected minimum absolute-expiry projection for eligible local `User` actions only. It creates no offline role or duplicate permission. Administrator, Support, Auditor, server processing, secured-account operations, Issue submission/status, Audit query/export, SignalR, and reauthentication-required actions are online-only.

Every queued intent is reauthorized after reconnect.

**Reason**

- Preserves Offline-First without turning stale claims into server authority.
- Matches Documents 02, 06, and 08.

## ROLE-DEC-015 — Assistant Authorization Parity

**Decision**

Assistant has no permission of its own and no elevation. Every workflow step uses the calling user's same Feature mapping, resource ownership, policy, capability, consent, confirmation, reauthentication, application service, and audit requirement.

Privileged Admin/Support/Audit actions are not Assistant-exposed by default.

**Reason**

- Preserves the closed orchestrator boundary.
- Prevents prompt-based authorization bypass.

## ROLE-DEC-016 — API, UI, SignalR, and Worker Enforcement

**Decision**

- UI filters availability for UX only.
- API/application service makes the final server decision.
- SignalR authenticates and authorizes subscriptions but remains notification-only.
- Local/Server Worker validates workload/job/contract/resource bounds and never maps human roles.
- Every retry, resume, cancel, result read/download, range, restore, export, and subscription is reauthorized.

**Reason**

- Prevents alternate entry paths.
- Preserves REST authority and Worker isolation.

## ROLE-DEC-017 — Workload Identity and Permission Cache Separation

**Decision**

Workload permissions are non-human, audience/purpose/environment/job scoped, and cannot be assigned to human principals. End-user tokens are not forwarded. Client permission projection is UX only; server permission caches are revision-aware, bounded, invalidated on account/session/role/permission/policy change, and never replace current sensitive-action checks.

**Reason**

- Prevents human/service privilege mixing.
- Allows efficient authorization without stale authority.

## ROLE-DEC-018 — Authorization Test and Audit Handoff

**Decision**

Every permission family requires allow, missing-permission, wrong-owner/scope, wrong-role, expired/revoked, offline/online, reauthentication, direct-API, Assistant-parity, and data-leakage tests as applicable.

Document 09 records only `AUDIT_REQUIRED = YES/NO/CONDITIONAL`. Document 10 owns event names, fields, integrity, retention, legal hold, export, alerting, and SIEM design.

**Reason**

- Makes the matrix executable as a test specification.
- Preserves the Document 09/10 boundary.

---

# 4. Governed Confirmations and Controlled Handoffs

```text
REQUIRES_PERTAMINA_STANDARD_CONFIRMATION

- approved identity claims/group mapping and provisioning authority
- exact Support, Administrator, and Auditor organizational scope vocabulary
- Support grant source: approved IAM mapping or controlled scoped-grant persistence
- segregation-of-duties incompatible combinations and approval authority
- role/permission assignment delegation ceiling and recertification cadence
- reauthentication assurance method, maximum age, retry/lock behavior, and thresholds
- offline grant lifetime and revocation-propagation objective
- permission/policy revision field, invalidation transport, and cache freshness classes
- official classification and disclosure/export thresholds
- future emergency administrative recovery; no content break-glass exists now
```

Controlled document handoffs:

- Document 06 addendum is required before implementing missing User/Role/Permission Management and other-user Session Revocation contracts. Document 09 does not invent routes.
- A controlled Document 07 addendum is required only if Pertamina does not provide an approved Support mapping and scoped principal grants must be persisted. Document 09 does not alter the FINAL database silently.
- Document 10 finalizes Audit event and evidence design.
- Document 12 finalizes numeric performance and test thresholds.

---

# 5. Final Status

```text
Role decisions              : 18 / 18 AUTO-LOCKED
Formal roles                : 3
New formal role             : 0
Superadmin wildcard         : 0
Implicit privileged content : 0
Material business change    : 0
User approval required      : 0
Canonical features          : 550
```
