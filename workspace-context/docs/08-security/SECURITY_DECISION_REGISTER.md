# SECURITY DECISION REGISTER
## PERTAMINA OTO — Office Tools Organizer

> **Document:** 08 Security Design  
> **Version:** 1.0  
> **Date:** 24 Agustus 2026  
> **Status:** 20/20 AUTO-LOCKED  
> **Decision mode:** Technical derivation from Documents 01–07 FINAL  
> **Canonical feature baseline:** 550

---

# 1. Lock Rule

Decisions below preserve the locked product, architecture, API, database, Local-First, Sign PDF non-PKI, and public-AI prohibition. They do not select a new IAM, storage, cryptographic, scanning, AI, or security vendor.

Unspecified Pertamina policy values and products are marked `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`; they are not invented and are not permission to weaken the secure default.

---

# 2. Decision Summary

| ID | Decision | Status |
|---|---|---|
| `SEC-DEC-001` | Zero-Trust trust boundaries and deny-by-default | **AUTO-LOCKED** |
| `SEC-DEC-002` | Pertamina Identity authenticates; OTO API authorizes | **AUTO-LOCKED** |
| `SEC-DEC-003` | Native PKCE; Web server-side session; no token in URL | **AUTO-LOCKED** |
| `SEC-DEC-004` | Narrow absolute-expiry offline authorization | **AUTO-LOCKED** |
| `SEC-DEC-005` | Permission + ownership + policy + state authorization | **AUTO-LOCKED** |
| `SEC-DEC-006` | Scoped fresh reauthentication for sensitive action | **AUTO-LOCKED** |
| `SEC-DEC-007` | Administrative authority separated from content access | **AUTO-LOCKED** |
| `SEC-DEC-008` | Protected per-profile local boundary including SQLite/WAL/artifacts | **AUTO-LOCKED** |
| `SEC-DEC-009` | Layered quarantine and validation for untrusted files | **AUTO-LOCKED** |
| `SEC-DEC-010` | Purpose-separated secure storage/transfer/version lifecycle | **AUTO-LOCKED** |
| `SEC-DEC-011` | Least-privilege isolated Worker and authenticated typed IPC | **AUTO-LOCKED** |
| `SEC-DEC-012` | Web/API/browser edge security baseline | **AUTO-LOCKED** |
| `SEC-DEC-013` | Remote fetch denied under current API; SSRF gate for future revision | **AUTO-LOCKED** |
| `SEC-DEC-014` | Assistant/AI is untrusted orchestrator with closed tools | **AUTO-LOCKED** |
| `SEC-DEC-015` | Approved-provider-only content intelligence and minimized data | **AUTO-LOCKED** |
| `SEC-DEC-016` | Approved secret/key boundary; no custom crypto | **AUTO-LOCKED** |
| `SEC-DEC-017` | Allowlisted safe logging/telemetry and data-plane separation | **AUTO-LOCKED** |
| `SEC-DEC-018` | Layered abuse controls and fail-secure degradation | **AUTO-LOCKED** |
| `SEC-DEC-019` | Secure SDLC, supply-chain, artifact, and environment gates | **AUTO-LOCKED** |
| `SEC-DEC-020` | Risk-based security testing, VA/PT, restore, and release gate | **AUTO-LOCKED** |

---

# 3. Decision Records

## SEC-DEC-001 — Trust and Secure Default

**Status:** AUTO-LOCKED

**Decision:** Every client, file, document text, OCR text, AI/provider output, IPC/queue message, identifier, internal call, and configuration value is untrusted until validated at its consuming boundary. Unknown identity, permission, resource state, Feature, contract, policy, provider, or security dependency fails closed.

**Reason:**

- aligns Zero Trust, least privilege, defense in depth, and locked object-aware authorization;
- prevents “internal network” or opaque ID from becoming authority.

**Impact:**

- negative-path tests are mandatory;
- only explicit minimal compatibility/liveness endpoints may be anonymous.

## SEC-DEC-002 — Identity and Authorization Authority

**Status:** AUTO-LOCKED

**Decision:** Approved Pertamina Identity authenticates the human. PERTAMINA OTO API validates the approved assertion/session and remains authoritative for application permissions, ownership/scope, policy/classification, resource state, compatibility, consent, and reauthentication.

**Reason:**

- preserves SSO without creating a second password system;
- a valid token proves identity, not access to an OTO resource.

**Impact:**

- issuer + subject maps to internal `PrincipalId`;
- no Pertamina password is collected, proxied, stored, or logged.

## SEC-DEC-003 — Client Session and SignalR Credential Transport

**Status:** AUTO-LOCKED

**Decision:** Native clients use the approved authorization flow with PKCE and OS credential protection. Web uses a same-site server-side session/BFF-style boundary with an opaque `Secure`, `HttpOnly` cookie and anti-forgery controls. Web SignalR uses that same-site session; native SignalR uses an approved authorization-header transport. Access tokens are never placed in URLs/query strings.

**Reason:**

- resolves the browser SignalR token conflict without revising Document 06;
- minimizes browser token exposure.

**Impact:**

- a platform unable to comply uses REST fallback until an approved solution exists;
- session ID rotates after login, reauthentication, and privilege-context change.

## SEC-DEC-004 — Offline Authorization

**Status:** AUTO-LOCKED

**Decision:** Offline authorization is a protected per-profile/device, capability-allowlisted projection with absolute expiry and no offline sliding renewal. It permits only eligible local capabilities and is never accepted by API/SignalR or privileged/admin/support/audit surfaces.

**Reason:**

- preserves Local-First while bounding revocation staleness;
- prevents cached claims becoming enterprise authority.

**Impact:**

- reconnect validates identity, account, permission, policy, capability, compatibility, consent, and queued commands before submission;
- exact lifetime is governed policy, not hardcoded here.

## SEC-DEC-005 — Object-Aware Authorization

**Status:** AUTO-LOCKED

**Decision:** Allow requires authenticated active principal/session + action/Feature permission + ownership or explicit scope + policy/classification + resource state/version + compatibility/capability + consent + fresh reauthentication when required.

**Reason:**

- blocks IDOR and vertical escalation;
- matches API and database ownership.

**Impact:**

- authorization occurs before count, filter, cursor, storage resolution, preview, download/range, export, retry/resume/cancel, and SignalR subscription;
- role name, public ID, Job ID, Correlation ID, `fileRef`, cursor, ETag, or storage key never grants access.

## SEC-DEC-006 — Reauthentication

**Status:** AUTO-LOCKED

**Decision:** Sensitive actions require a server-enforced, short-lived, action/resource/session-bound fresh Pertamina authentication context. It confirms identity freshness and never grants new permission.

**Reason:**

- protects destructive/high-disclosure/security changes;
- avoids permanent elevated mode.

**Impact:**

- missing freshness returns safe `401 REAUTHENTICATION_REQUIRED` before mutation;
- exact actions/age/assurance are inputs to Documents 09–10 and Pertamina policy.

## SEC-DEC-007 — Privileged Surface and Content Separation

**Status:** AUTO-LOCKED

**Decision:** Administrator, Support/Superadmin, Auditor, Operations, DBA, and backup authority do not automatically gain document-content access. Content access, administration, issue attachment access, audit metadata, and recovery operations are separate permissions/purposes.

**Reason:**

- preserves upstream privacy boundary;
- prevents title-based privilege escalation.

**Impact:**

- no implicit impersonation, storage browsing, or “view as user”;
- final Role × Permission matrix remains Document 09.

## SEC-DEC-008 — Local Data Protection Boundary

**Status:** AUTO-LOCKED

**Decision:** Local profile root, SQLite DB, WAL/SHM, migration backup, temp/cache/checkpoint, crash/diagnostic copies, Assistant state, and local artifacts share restrictive OS permissions, profile isolation, backup exclusion, and approved at-rest protection. Keys live outside data in an approved OS facility.

**Reason:**

- derivatives can reveal the same content as source;
- fixes Document 07 security handoff without selecting a vendor.

**Impact:**

- no cleartext production fallback;
- absence of verified mobile backup exclusion or approved cross-platform protection blocks that platform release.

## SEC-DEC-009 — File Validation and Quarantine

**Status:** AUTO-LOCKED

**Decision:** Every file uses bounded preflight → streamed quarantine/staging → type/signature/structure/resource/security validation → isolated processing → output revalidation → explicit promotion. Inconclusive or timed-out validation fails closed.

**Reason:**

- extension/MIME alone cannot establish safety;
- contains parser, decompression, path, and resource attacks.

**Impact:**

- only validated `READY` content may become job input or secured version;
- malicious-file corpus, fuzzing, isolation, and resource tests are release gates.

## SEC-DEC-010 — Storage, Transfer, and Versions

**Status:** AUTO-LOCKED

**Decision:** Quarantine, validated input, processing/checkpoint, job result, secured version, issue attachment, and export are purpose-separated storage zones with opaque references, object authorization, encryption, integrity digest, retention, tombstone, and reconciliation. Execution upload and **Amankan ke Akun** remain distinct.

**Reason:**

- prevents confused-deputy and lifecycle leakage;
- preserves maximum three active/addressable versions.

**Impact:**

- every GET/HEAD/range reauthorizes;
- raw paths/object keys/durable unrestricted URLs are never returned or logged.

## SEC-DEC-011 — Worker and IPC

**Status:** AUTO-LOCKED

**Decision:** Local/Server Workers use least privilege, job-scoped inputs, closed operations, isolated working directories, bounded resources, cancellation, crash containment, and recycle. Named Pipe/local IPC is local-only, ACL/identity/instance bound, authenticated/versioned, framed, bounded, and allowlisted.

**Reason:**

- untrusted parsers must not inherit UI/API privileges;
- pipe name/current user alone is not full authentication.

**Impact:**

- Worker receives no arbitrary SQL/SQLite/shell/network/filesystem/provider access or user token;
- IPC/containment PoC is mandatory.

## SEC-DEC-012 — Browser and API Edge

**Status:** AUTO-LOCKED

**Decision:** HTTPS, strict token/session validation, anti-forgery, safe methods, explicit CORS origins, WebSocket Origin validation, restrictive CSP, contextual encoding, safe Markdown/export, strict DTO/query/header limits, trusted-forwarded-header boundary, and consistent safe errors are mandatory.

**Reason:**

- addresses XSS, CSRF, CORS, injection, smuggling, and information disclosure;
- implementable in locked ASP.NET Core architecture.

**Impact:**

- no raw HTML by default;
- production-equivalent proxy/API parsing tests are required.

## SEC-DEC-013 — Remote Content and Web-to-PDF

**Status:** AUTO-LOCKED

**Decision:** Under Document 06 FINAL, generic Feature execution cannot accept an arbitrary URL. `FEAT-CONV-018` remains a feasibility-gated feature and may use a validated captured/local HTML artifact under the current contract. Server-side remote URL fetching is unavailable until a controlled API addendum/revision defines a typed remote-resource contract and passes SSRF/renderer PoC.

**Reason:**

- resolves the specialist contract gap without editing Document 06;
- avoids silent SSRF/local-network access.

**Impact:**

- feature is not deleted;
- remote fetch remains disabled, has no ambient credentials, and cannot be selected by AI/provider.

## SEC-DEC-014 — Assistant and AI Control Flow

**Status:** AUTO-LOCKED

**Decision:** Assistant/AI output is untrusted. It can propose only visible canonical Feature IDs and closed typed parameters/workflows. Every step rechecks ownership, permission, policy, classification, capability, consent, confirmation, resource budget, and result state through the official application service.

**Reason:**

- prevents prompt/tool hijacking and confirmation bypass;
- preserves Assistant as orchestrator only.

**Impact:**

- no direct SQL, shell, filesystem, arbitrary API/URL, code, provider, or engine selection;
- success is grounded in validated actual result.

## SEC-DEC-015 — Content Intelligence Provider Boundary

**Status:** AUTO-LOCKED

**Decision:** AI Summarizer/Translate use only an approved local or enterprise-internal provider. Public AI is technically unreachable. Transfer requires ownership, permission, classification, purpose, notice/consent, data minimization, provider capacity, protected temporary lifecycle, and approved provider governance.

**Reason:**

- enforces `SR-SEC-015..016,021`;
- minimizes restricted-content disclosure.

**Impact:**

- pure transforms expose no tools;
- provider request/response content is excluded from ordinary DB, audit, telemetry, idempotency, SignalR, and logs.

## SEC-DEC-016 — Secrets, Encryption, and Keys

**Status:** AUTO-LOCKED

**Decision:** Secrets/keys use approved environment-separated credential/key facilities and least-privilege workload identities. Transport uses approved TLS with certificate validation; server-stored sensitive data/backups and required local state use approved at-rest protection. No custom cryptography or embedded master key.

**Reason:**

- prevents secret disclosure and unrecoverable ad-hoc crypto;
- no new vendor is selected.

**Impact:**

- raw password/token/key/secret is absent from source, config, SQL/SQLite, artifacts, logs, telemetry, audit, issue, and backups;
- exact algorithms/provider/rotation/escrow require Pertamina confirmation.

## SEC-DEC-017 — Logging and Privacy

**Status:** AUTO-LOCKED

**Decision:** Logs/telemetry use an allowlist of safe operational identifiers/codes/counters. Document/OCR/prompt/chat/password/token/secret/clipboard/signature/screenshot content is prohibited. Observability, Enterprise Audit, User History, and Support remain separate data planes.

**Reason:**

- supports troubleshooting without content leakage;
- aligns Documents 06–07.

**Impact:**

- query strings/headers/delegated links are redacted;
- crash dumps and diagnostic bundles follow the same boundary.

## SEC-DEC-018 — Abuse and Fail-Secure Degradation

**Status:** AUTO-LOCKED

**Decision:** Apply layered request, session, upload, query, range, SignalR, job, retry, Worker, engine, provider, temp/storage, connection, and export limits. Queues are persistent, bounded, fair, and workload-separated. Security dependency failure never grants anonymous/stale privileged access or public-AI fallback.

**Reason:**

- protects small initial server capacity;
- preserves feature availability through safe queue/degradation.

**Impact:**

- `413/429/409/503` semantics follow Document 06;
- local-safe capability remains available under server pressure.

## SEC-DEC-019 — Secure SDLC and Supply Chain

**Status:** AUTO-LOCKED

**Decision:** Azure DevOps PR/pipeline gates include review, pinned SDK/packages, approved sources/dependencies, secret/dependency/static/unsafe/config/API/deployment scans, license review, SBOM, artifact digest/provenance evidence, Dev/QA/Prod separation, and same-artifact promotion.

**Reason:**

- preserves locked DevOps and dependency governance;
- addresses parser/native/model supply-chain risk.

**Impact:**

- no manual Production binary/config/database patch;
- exact tools/signing/products/branch authority require Pertamina confirmation.

## SEC-DEC-020 — Test, VA/PT, Restore, and Release Gate

**Status:** AUTO-LOCKED

**Decision:** Security-sensitive releases require applicable unit, integration, contract, authorization, abuse/negative, malicious-file, Worker/IPC, AI, performance/DoS, recovery, backup/restore, and regression evidence. VA/PT is mandatory before Production. Critical/High findings block release unless exceptional approved expiring risk acceptance exists.

**Reason:**

- converts controls into testable release evidence;
- supports recovery and incident readiness.

**Impact:**

- release outcome is GO/NO-GO/evidenced exceptional acceptance;
- final approver matrix belongs to Pertamina governance/Documents 09, 12, 16, and 17.

---

# 4. Governed Confirmations

The following are not source-document decisions and must be confirmed before their Production gate is complete:

```text
REQUIRES_PERTAMINA_STANDARD_CONFIRMATION
- IAM protocol/profile, claims, provisioning, lifetimes, revocation propagation
- official data-classification mapping and handling matrix
- reauthentication action/assurance/age policy
- final Role × Permission and separation-of-duty matrix
- SQLite/local encryption, backup exclusion, key/logout/recovery implementation
- approved secret/key platform and cryptographic standard
- object storage/KMS/delegated-download policy
- malware/content inspection and engine/provider approval
- internal AI data location, retention, training/reuse, operator/subprocessor rules
- secure coding/analyzer/branch/scan/SBOM/signing standards and toolchain
- environment/network/host hardening and container applicability
- audit event/integrity/legal retention policy
- vulnerability scoring/SLA/exception authority
- alert/SOC/SIEM and incident/evidence policy
- VA/PT provider/method/cadence/acceptance authority
- RPO/RTO, backup/DR topology, key recovery, restore cadence
- numeric capacity, rate, timeout, retention, and performance gates
```

Until confirmed, implementation uses the deny-by-default/no-public-fallback/no-new-sensitive-storage rule or remains unavailable at that boundary.

---

# 5. Final Status

```text
Security decisions      : 20 / 20 AUTO-LOCKED
Material architecture change: 0
New vendor/cost decision: 0
User approval required  : 0
Feature baseline        : 550
```
