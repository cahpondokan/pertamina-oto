# AGENT D — Secure SDLC, Operations, Testing & Governance Report

**Task ID:** `DOC08-AGENT-D`  
**Scope:** Security lifecycle from Design through Update  
**Source baseline:** Document 01 FINAL v3.4, Document 02 FINAL v1.5, Document 03 FINAL v1.3, Document 04 FINAL v1.2, Document 05 FINAL v1.2, Document 06 FINAL v1.0, Document 07 FINAL v1.0, and Master Project Summary v3.0  
**Output status:** Integration recommendation for Document 08; this report does not modify Documents 01–08 and does not assert an unstated official Pertamina control.

Normative keywords in this report:

- **MUST** — required by the locked source baseline or necessary to preserve a locked security boundary.
- **SHOULD** — recommended design for Document 08, subject to normal approval.
- **CONDITIONAL** — applies only when the referenced deployment/runtime is selected.
- **`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`** — an enterprise policy value, authority, threshold, product, or implementation standard is not locked by Documents 01–07.

Primary traceability anchors are `SR-SEC-001..023`, `IR-DEVOPS-001..004`, `NFR-ENV-001..004`, `NFR-MON-001..004`, `NFR-PERF-001..013`, `NFR-REL-001..006`, `BR-GOV-001..009`, `NFR-DOC-001..004`, `NFR-HAND-001`, the Document 06 contract-test requirements, and the Document 07 governed handoffs.

---

## 1. Secure SDLC model

### 1.1 Lifecycle and mandatory evidence

```text
Design
  → Coding
  → Build
  → Test
  → Deploy
  → Operate
  → Incident
  → Update
  → back to Design through governed feedback/change control
```

| Stage | Minimum security activity | Exit evidence | Primary traceability |
|---|---|---|---|
| Design | Threat/abuse analysis proportional to risk; classify data and trust boundaries; identify authentication, authorization, storage, AI, worker, file, logging, recovery, and deployment controls; record Pertamina-policy gaps | Approved design/security notes, requirement-to-control/test links, PoC decision where required | `SR-SEC-001..023`, `BR-GOV-003..007`, Documents 04 and 07 |
| Coding | Apply the secure coding standard; use approved dependencies only; protect secrets; add unit, negative, and security regression tests with the change | Scoped PR, code review record, tests, dependency decision, no unresolved prohibited pattern | `IR-DEVOPS-003`, Document 05 |
| Build | Restore from controlled sources; pin SDK/packages; compile Release; execute layered scans; create traceable artifact, SBOM, integrity metadata, and migration deliverables | Pipeline run linked to commit, scan reports, test reports, SBOM, artifact digest | `IR-DEVOPS-002..004`, Document 05 |
| Test | Execute the applicable security test pyramid, including contract, authorization, abuse, file, AI, worker, recovery, and performance/DoS cases | QA evidence, defect disposition, VA/PT evidence when applicable; Critical/High handled | Document 02 Production Acceptance and DoD; Document 06 §22; Document 07 §20 |
| Deploy | Promote the same controlled artifact through Dev → QA → Prod; inject secrets at deployment/runtime; use least privilege; protect Production configuration; verify rollback/recovery | Approval, deployment log, verified digest, configuration delta, migration evidence, smoke/security checks | `NFR-ENV-001..004`, Document 07 §12 |
| Operate | Structured safe logging, OpenTelemetry, Application Insights/APM, monitoring, alerting, audit integration, capacity and backup monitoring | Dashboards/alerts, safe diagnostic linkage, access/change evidence, backup status | `NFR-MON-001..004`, Documents 04, 05, and 07 |
| Incident | Detect, triage, contain, preserve evidence, recover, and initiate governed remediation without exposing restricted content | Incident reference, preserved safe evidence, containment/recovery decision, remediation item | Principles only in this report; operational guide belongs to Document 16 |
| Update | Reassess risk; patch or replace safely; rerun impacted tests/scans; rebuild from controlled source; promote a new traceable artifact | Approved change, clean pipeline, regression/retest evidence, updated SBOM, post-change monitoring | `BR-GOV-002..004`, `IR-DEVOPS-004` |

Security-sensitive features cannot be accepted solely because build and unit tests are green. At minimum, the risk-appropriate functional, business, UX, authorization, abuse/negative, recovery, privacy, performance, and security evidence must pass.

### 1.2 Coverage register for the 45 requested controls

| ID | Requested control | Detailed section |
|---:|---|---:|
| 1 | Secure coding standard | 2 |
| 2 | Branch protection principle | 3 |
| 3 | Pull/merge gate | 3 |
| 4 | Dependency approval | 4 |
| 5 | Package pinning | 4 |
| 6 | Dependency vulnerability scanning | 4–5 |
| 7 | Secret scanning | 5 |
| 8 | Static analysis | 5 |
| 9 | Unsafe-code checks | 2 and 5 |
| 10 | Source scanning | 5 |
| 11 | Configuration scanning | 5 and 7 |
| 12 | Container/deployment scanning where relevant | 5 |
| 13 | SBOM recommendation | 4 and 6 |
| 14 | License review | 4 |
| 15 | Artifact integrity | 6 |
| 16 | Build reproducibility | 6 |
| 17 | Environment separation | 8 |
| 18 | Secret injection during deployment | 7 |
| 19 | Production configuration protection | 7–8 |
| 20 | Least-privilege deployment | 8 |
| 21 | Security logging | 9 |
| 22 | Security monitoring | 9 |
| 23 | Alerting | 9 |
| 24 | Suspicious behavior | 9 |
| 25 | Audit integration | 9 |
| 26 | Vulnerability handling | 10 |
| 27 | Severity classification | 10 |
| 28 | Remediation workflow | 10 |
| 29 | Emergency security update | 10 and 13 |
| 30 | Dependency compromise response | 4, 10, and 13 |
| 31 | Key/secret rotation principle | 7 and 13 |
| 32 | Backup protection | 14 |
| 33 | Restore validation | 14 |
| 34 | Penetration-test readiness | 12 |
| 35 | Security regression test | 11 |
| 36 | Abuse/load test | 11 |
| 37 | API security tests | 11 |
| 38 | Authentication tests | 11 |
| 39 | Authorization tests | 11 |
| 40 | File-security tests | 11 |
| 41 | AI security tests | 11 |
| 42 | Worker isolation tests | 11 |
| 43 | Incident evidence preservation | 13 |
| 44 | Release security checklist | 15 |
| 45 | Handover security checklist | 15 |

---

## 2. Coding controls

### 2.1 Secure coding standard

The repository SHOULD adopt one versioned secure coding standard covering Web, API, Worker, Desktop, Mobile, data access, local IPC, file handling, and approved internal AI integration. Its minimum rules are:

1. Validate untrusted input at every trust boundary using typed allowlists, length/count/range limits, and canonicalization where applicable. Never accept arbitrary Feature ID, engine/provider, path, URL, SQL, field, filter, or sort value.
2. Enforce authentication and object-aware authorization in the owning application/API boundary. Possession of a Job ID, Correlation ID, file reference, cursor, or object-storage key is never authorization.
3. Use parameterized data access and module-owned repositories/DbContexts. Client code MUST NOT access SQL directly; cross-module table writes and unrestricted dynamic queries are prohibited.
4. Treat uploaded files, PDFs, Office files, images, OCR output, document instructions, and AI output as untrusted. Validate type, signature, structure, size, decompression behavior, schema, and result before use.
5. Stream large files and bound memory, CPU, storage, connections, timeouts, retries, concurrency, and queue occupancy. No whole-file buffering for server transfers or long-held DB connection during external work.
6. Preserve source files. Write to app-managed temporary/output locations, validate the result, then publish atomically. Never silently overwrite or delete an original on failure.
7. Use safe error codes and correlation metadata. Responses, logs, traces, metrics, audit, SignalR events, crash dumps, and diagnostics MUST NOT contain document content, full OCR/extracted text, passwords, tokens, secrets, signature data, or automatically captured issue screenshots.
8. Passwords for protected files are process-memory-only and MUST NOT be persisted, logged, audited, autosaved, backed up, included in job payloads, or sent through SignalR.
9. Use platform/framework cryptographic APIs and approved credential/key stores. Do not implement custom cryptographic algorithms or store raw key material beside SQL, SQLite, object metadata, artifacts, or configuration.
10. Assistant actions MUST use the canonical Feature registry/application service and the same authorization/validation path as UI actions. Direct SQL, shell, arbitrary code, unrestricted file access, arbitrary API calls, and public AI endpoints are prohibited.
11. Document/OCR content cannot alter system policy, tool permission, or instruction hierarchy. Internal AI transfer, when approved, requires policy/classification/permission checks, data minimization, and appropriate user notice/consent.
12. Cancellation, timeout, retry, idempotency, checkpoint, and recovery behavior must be explicit. Retry is bounded and only wraps operations proven safe/idempotent.
13. Structured logging uses stable safe fields such as Feature ID, Job ID, Correlation ID, status, duration, provider/engine identifier, error code, and application version; message templates must not interpolate restricted values.
14. Security-relevant code has unit and negative tests in the same PR. A bug fix for a security weakness adds a regression test unless technically impossible and documented.
15. Compiler/analyzer warnings that represent correctness or security risk cannot be suppressed merely to make the pipeline green. Suppression requires narrow scope, rationale, owner, expiry/review point, and approval.

The exact Pertamina secure-coding reference, required analyzer rule set, warning baseline, and exception authority are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 2.2 Unsafe-code and dangerous-capability checks

`unsafe` C#, unmanaged interop, P/Invoke, dynamic code generation, unrestricted reflection, process/shell execution, arbitrary network/file access, raw SQL, insecure/legacy serialization, and disabling TLS/certificate validation SHOULD be denied by default. If a locked feature genuinely requires one, the change must have:

- documented need and bounded call site;
- threat and platform analysis;
- explicit security and dependency approval;
- input/output and memory/resource safeguards;
- targeted negative, isolation, and failure tests;
- a maintained safe fallback or an approved limitation.

CI must detect these constructs through compiler/analyzer rules plus targeted source-pattern checks. The approved exception list and approver are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

## 3. Git/merge controls

### 3.1 Branch protection principle

Azure DevOps Git is the controlled repository. Long-lived integration/release branches MUST be protected against direct routine push, force-push, history rewrite, and deletion. Work occurs in short-lived, scoped branches/worktrees linked to approved requirement, Feature ID, defect, risk, or maintenance item. Unrelated refactoring is excluded from the change.

Minimum protected-branch policy:

- PR is mandatory;
- successful current pipeline status is mandatory;
- required discussions are resolved;
- changes remain traceable to requirement/Feature/test/release evidence;
- at least one human reviewer is present, as locked by Document 05;
- the author cannot be the sole security acceptance authority for a security-sensitive change;
- branch-policy bypass is exceptional, logged, time-bounded, and post-reviewed;
- emergency fixes use a tracked expedited path, not an unreviewed direct Production change.

Exact protected branch names, reviewer count, code-owner paths, separation-of-duty rules, bypass authority, commit-signing requirement, and retention of PR evidence are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 3.2 Pull/merge gate

A PR is mergeable only when all applicable items pass:

| Gate | Required evidence |
|---|---|
| Scope and traceability | Work item, requirement/Feature IDs, security impact, tests, docs, migration/config impact |
| Review | Human review by an appropriate owner; security specialist review for high-risk boundary changes |
| Build | Controlled restore, Release build, no unexplained warnings/errors |
| Automated tests | Unit plus impacted integration/contract/architecture/security regression suites |
| Layered scan | Secret, dependency, static/unsafe/source, configuration, and conditional deployment/container results |
| API/database compatibility | OpenAPI validation/breaking diff; migration review and compatibility checks where applicable |
| Findings | No unresolved release-blocking finding; any exception is narrow, approved, expiring, and evidenced |
| Documentation | Changed control, threat, runbook reference, API/database, deployment, and test evidence updated as applicable |

The merge gate MUST NOT be weakened or have scans disabled merely to obtain a green status.

---

## 4. Package/dependency controls

### 4.1 Approval and pinning

All first- and third-party dependencies, engines, providers, build tools, container bases if any, models, fonts, and runtime components are inventory-controlled. For .NET:

- the approved .NET 10 LTS SDK patch is pinned in `global.json` when the repository is bootstrapped;
- package versions are centralized in `Directory.Packages.props`;
- projects do not silently choose or override versions;
- floating ranges and automatic major/minor upgrades are prohibited;
- restore uses only approved package sources and produces a repeatable resolved graph;
- transitive dependencies are reviewed/scanned, not only direct dependencies;
- security patches are assessed, tested, approved, and rolled out through the pipeline.

A new or replaced dependency requires a lightweight decision record containing purpose, owner, alternatives/why existing capability is insufficient, supported platforms, license, vulnerability/security result, maintenance health, resource impact, compatibility, data/network behavior, fallback/removal plan, and PoC/benchmark result where `BR-GOV-006..007` applies. One identified human approver is the minimum baseline; the official approval authority is `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 4.2 Vulnerability scanning and update monitoring

Dependency vulnerability scanning MUST run on PR/build and on a scheduled cadence capable of detecting newly disclosed issues in unchanged code. A release also records the resolved dependency graph. Findings are deduplicated, assigned an owner, assessed for actual reachability/exposure where possible, and fed into Section 10. Scanner data must not upload proprietary source or document content to an unapproved public service.

Exact scanner product, advisory sources, offline/internal feed requirement, scheduled frequency, and severity threshold are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`. The baseline cannot depend solely on a paid add-on.

### 4.3 SBOM and license review

Each releasable artifact SHOULD include or be accompanied by a machine-readable Software Bill of Materials containing component name, version, supplier/source when known, direct/transitive relationship, license assertion, and artifact/commit/build linkage. SBOM generation is automated after the final resolved restore and retained with release evidence.

License review must classify allow, deny, conditional obligation, notice/attribution, source-disclosure risk, redistribution restriction, model/data-license restriction, and commercial-use constraints. Approval is required before the component enters the main branch or release artifact.

The required SBOM format/version, signing/attestation method, retention, disclosure audience, approved/denied license list, and legal-review authority are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 4.4 Dependency compromise response

When a dependency/source/feed is suspected compromised:

1. stop new restore/build/release from the affected source or version;
2. preserve package, hash, pipeline, SBOM, and usage evidence;
3. use SBOM and source search to identify affected artifacts/environments;
4. revoke/rotate credentials if exposure is plausible;
5. pin to a verified safe version, remove/replace the dependency, or isolate/disable only the affected capability through approved configuration;
6. rebuild from controlled clean inputs, rerun all impacted scans/tests, and publish a new artifact rather than editing an existing artifact;
7. monitor for exploitation and complete a governed post-incident dependency decision.

Operational contacts, notification and escalation belong to Document 16.

---

## 5. CI security scanning

### 5.1 Layered scan model

| Layer | Minimum scope | Gate behavior |
|---|---|---|
| Secret scanning | Changed content and repository history as supported; source, scripts, configs, test data, pipeline definitions, generated examples | Confirmed credential/secret blocks merge/release; rotate/revoke rather than only delete from latest commit |
| Dependency scan | Direct/transitive .NET and other resolved dependencies, tools, models, container bases if applicable | Severity/exploitability assessed under Section 10; release-blocking findings cannot be ignored |
| Static analysis | C#/.NET correctness and security analyzers, injection, auth/authz misuse, cryptography, deserialization, resource/error handling | High-confidence security issues block; narrow governed suppression only |
| Unsafe/source scan | `unsafe`, P/Invoke, shell/process, arbitrary path/URL/SQL, disabled TLS validation, sensitive logging, public AI endpoint/provider, forbidden direct module/data access | Prohibited pattern blocks unless explicitly approved exception |
| Configuration/IaC scan | Debug flags, secrets, HTTPS/TLS, auth defaults, CORS, logging level/content, storage exposure, connection strings, anonymous endpoints, production overrides, backup/telemetry settings | Unsafe Production-effective configuration blocks deployment |
| API/contract scan | Generated OpenAPI validity, breaking changes, typed schemas, handler/authz/test registration, redacted examples | Breaking or insecure contract blocks merge unless approved version/migration path |
| Deployment scan | Deployment scripts/templates, IIS/Azure configuration or other selected target, file permissions, identities, public exposure, insecure defaults | Mandatory for the actual deployment model |
| Container/image scan | Base image, packages, configuration, non-root/least privilege, secrets, exposed ports, provenance | **CONDITIONAL** only if containers are selected |
| Dynamic scan | Deployed QA/Pilot Web/API attack surface using synthetic data and approved safe limits | Before Production for applicable externally reachable/server surfaces; results enter vulnerability workflow |

No scanning product is selected by this report. Product choice, paid licensing, source-upload/data-residency behavior, and Azure DevOps integration are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 5.2 Pipeline placement and evidence

- Fast deterministic checks run on every PR.
- Full integration, dynamic, performance, and long-running scans run at the earliest practical controlled stage and always before the relevant Production release.
- Scheduled scans detect newly disclosed dependency/configuration issues even when code is unchanged.
- Scan versions, rule sets, timestamps, commit, artifact digest, result, suppression/waiver, and logs are retained as release evidence.
- Generated examples and diagnostic artifacts are scanned/redacted so they do not become a path for content, token, password, OCR, signature, or screenshot disclosure.
- A scan failure caused by unavailable tooling fails safe according to approved pipeline policy; it is not silently converted to pass. Exact retry/fail-open behavior is `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

## 6. Build/release controls

### 6.1 Controlled build and reproducibility

The pipeline must build on controlled, clean agents using the pinned SDK, centralized packages, approved sources, recorded tool versions, Release configuration, and explicit environment inputs. It executes restore → build → tests/scans → package → SBOM/integrity metadata → publish artifact. Generated code/contracts are regenerated and compared where required.

Reproducibility controls:

- no undeclared local developer dependency or machine-specific path;
- no floating SDK/package/container/model version;
- deterministic build settings where supported;
- resolved dependency graph and build-agent image/version captured;
- generated artifacts are produced by pipeline, not copied from an untracked workstation;
- a clean rebuild can be compared with the released artifact or with its documented reproducibility metadata;
- any known non-deterministic input is identified and bounded.

Whether Pertamina requires byte-for-byte reproducible output, the approved build-agent image baseline, controlled feed topology, and evidence-retention period are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 6.2 Artifact integrity and promotion

Every deployable artifact, database migration script/bundle, configuration package, and SBOM must be linked to source commit and pipeline run. The pipeline computes a strong digest, publishes to controlled immutable/restricted artifact storage, and verifies the digest before deployment. Dev, QA, and Prod receive the same artifact; environment-specific values are injected separately, never rebuilt into a different binary.

Artifact signing/provenance attestation is recommended, but the signing technology, certificate/key owner, trust chain, timestamping, and verification policy are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`. This build-artifact integrity control is unrelated to the product's **Sign PDF** feature, which remains visual non-PKI.

### 6.3 Release construction

Release evidence binds:

```text
Requirement / Feature / defect
→ commit and reviewed PR
→ dependency decision and resolved graph
→ build + scan + test results
→ SBOM + artifact digest
→ configuration and migration version
→ QA/UAT/security approval
→ deployment and rollback/recovery evidence
→ monitored application version
```

Production receives only pipeline artifacts. Manual in-place binary edits, ad-hoc database migration, untracked configuration change, or disabling debug/security controls are prohibited.

---

## 7. Secret/config management

### 7.1 Secret boundary and deployment injection

Passwords, access/refresh tokens, service secrets, private keys, encryption keys, and connection credentials MUST NOT be ordinary database fields, committed source, package, artifact, SBOM, test fixture, pipeline log, telemetry, audit, or issue attachment. Secrets are retrieved from an approved enterprise credential/key mechanism and injected at deployment or runtime through a least-privileged identity.

Required behaviors:

- separate secret values and access identities per environment;
- pipeline identities may retrieve only secrets needed for their target stage;
- application identities retrieve only their own runtime secrets;
- secret values are masked in pipeline/application output, but masking is not treated as permission control;
- secret retrieval and administrative change produce safe access/change evidence;
- secret absence or invalid configuration causes a safe startup/deployment failure, not a fallback to hard-coded defaults;
- developer workstations do not receive Production secrets through repository configuration.

The approved secret manager, key-management/HSM standard, service-connection pattern, break-glass process, and access-review cadence are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 7.2 Production configuration protection

Non-secret Production configuration is versioned or baselined, schema-validated, reviewed, access-controlled, environment-scoped, and change-audited. Effective configuration must keep debug off, enforce HTTPS/authentication/authorization, preserve restricted logging rules, prevent public AI/provider fallback, and apply approved file, queue, retention, backup, and telemetry boundaries.

Configuration drift detection SHOULD compare the deployed effective baseline with the approved release configuration without disclosing secrets. Emergency configuration changes follow the same traceable incident/change path and are reconciled back into the controlled baseline.

### 7.3 Rotation and local key boundary

Keys/secrets are rotated on suspected exposure, ownership/environment change, cryptographic deprecation, dependency compromise with plausible access, and the approved periodic schedule. Rotation must support overlap/rollback only for the minimum safe window, verify dependent services, and retire old material without making retained encrypted data unrecoverable.

For SQLite and local artifacts, the main DB, WAL, SHM, migration backup, temp, crash dump, diagnostics, and content artifacts form one protection boundary. Raw keys are not stored beside them. Logout locks/unmounts the profile; deletion/key destruction follows approved retention/security policy. Mobile local-only stores must be excluded from automatic cloud/device backup using an approved mechanism; absence of that mechanism is a release blocker.

The exact SQLite encryption provider, OS credential integration, WAL/temp protection, key rotation/destruction, logout behavior, secure erase semantics, and recovery escrow are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` and require dependency approval plus security tests.

---

## 8. Environment security

### 8.1 Dev / QA / Prod separation

Development, QA/Testing, and Production must have distinct effective endpoints, credentials, service identities, configuration, data/storage boundaries, telemetry context, deployment authorization, and access paths. Environment labels alone are insufficient isolation.

- Development uses synthetic data. Production data cannot be used there without approved masking and authorization.
- QA uses controlled representative data and test accounts; security/abuse tests do not target Production unless explicitly approved.
- Production has debug disabled, restricted administrative access, active monitoring/alerting, and only accepts controlled pipeline artifacts.
- Logs, backups, object storage, queues, databases, and AI/provider endpoints must not accidentally cross environments.
- Local-first capability remains usable safely when server services are unavailable; outage must never cause fallback to public AI.

The physical Azure DevOps project, tenant/subscription, network segmentation, firewall/private endpoint, host hardening, endpoint protection, administrative workstation, and data-masking standard are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 8.2 Least-privilege deployment

Deployment identities are separate from normal developer/user identities and receive only target-environment actions required for the release. Application/API/Worker/database/object-storage identities receive only their module/runtime permissions. Production database migration does not run automatically at application startup; the pipeline produces reviewed idempotent scripts/bundles, validates target compatibility, verifies a recovery point, deploys in controlled order, and records evidence.

Access to deploy, approve, modify Production configuration, execute migration, read backup, or rotate keys should be separable. The exact role names and Role Matrix are not created here; they belong to Document 09 and are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` before Production.

---

## 9. Monitoring/alerting

### 9.1 Security logging

Use `Microsoft.Extensions.Logging` with structured events, OpenTelemetry correlation, and Azure Monitor/Application Insights as the locked observability baseline. Security-relevant operational events contain only safe metadata needed to investigate: timestamp, application/environment/version, event/error code, Feature ID, Job ID, Correlation ID, protected/opaque subject or object reference where authorized, outcome, duration, component, and policy decision code.

Never record document bytes/content, full OCR/extracted text, PDF password, access/refresh token, service secret/key, clipboard data, visual signature value, arbitrary Assistant prompt/chat content, or issue screenshot. Filename/path, user identity, storage reference, and correlation identifiers are confidential metadata and require minimization, access control, retention, and export restrictions.

Log forging/control characters, oversized values, and unbounded high-cardinality fields must be normalized or rejected. Crash dumps and query capture follow the same restricted-content boundary.

### 9.2 Monitoring and suspicious behavior

Monitor at least:

- request duration/error rate, availability/health, application version, and Correlation ID coverage;
- authentication failures, invalid/expired tokens, revocation failures, and abnormal session patterns;
- authorization denials and repeated object-reference access across upload, job, result, history, secured file, issue, attachment, audit, and export;
- abnormal 401/403/consistent-404, 409, 413, 429, and 503 patterns;
- invalid/malformed files, MIME/signature mismatch, decompression/structure-limit triggers, upload resume/digest failures, and repeated password failures without recording the password;
- queue depth, wait time, retry storm, worker crash/lease expiry, admission/backpressure, CPU, RAM, storage, DB, connection pool, and transfer failure;
- unusual secured-file access/version/delete/restore/export, support-console access, permission/configuration change, audit/outbox gap, backup/restore failure, or retention cleanup anomaly;
- internal AI policy denials, attempted public-provider routing, prompt-injection/tool-schema rejection, and unusual tool invocation, without content capture;
- secret/dependency/static/config scan findings and deployed-version exposure.

Detection logic must distinguish expected load/backpressure from suspected abuse and must not automatically grant broader access for investigation.

### 9.3 Alerting and audit integration

Critical errors generate alerts according to support policy. Alerts should be actionable, deduplicated, severity-tagged, environment-aware, linked to safe diagnostic context, and routed to an accountable operational owner. Alert acknowledgement/escalation thresholds, on-call schedule, SOC/SIEM integration, notification channels, and retention are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

Observability, Enterprise Audit, User History, and Support data remain separate:

- observability supports health/performance/security detection;
- Enterprise Audit is append-oriented controlled evidence for approved business/security events;
- User History is user-visible/manageable and its deletion cannot delete Enterprise Audit;
- Support attachments exist only after explicit user preview/send and remain permission-controlled.

Security-relevant business/configuration/permission actions should emit the approved safe audit event after commit. Audit event catalog, integrity/immutability mechanism, legal retention, purge/hold, and export permissions belong to Document 10 and are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

## 10. Vulnerability management

### 10.1 Intake, classification, and gate

Findings may originate from code review, SAST/DAST, dependency/secret/configuration scans, security tests, VA/PT, monitoring, incident analysis, vendor advisory, or responsible reporting. Every finding receives a stable record linked to affected component/version/artifact/environment, evidence, reporter/source, owner, classification, exploitability/exposure assessment, remediation, tests, and closure/retest.

Use working severity bands **Critical, High, Medium, Low**. Critical and High findings are release blockers until remediated/retested or handled through an explicit exceptional risk acceptance consistent with the locked Production acceptance baseline. Medium and Low findings remain owned and scheduled according to approved risk and exposure. The official scoring method, use of CVSS or another method, SLA by severity/environment, risk-acceptance authority, and expiry limit are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

A waiver/risk acceptance must never be an undocumented scanner suppression. At minimum it records scope, affected artifact/environment, rationale, exposure, compensating control, accountable owner, approver, expiry, monitoring, and remediation target.

### 10.2 Remediation workflow

```text
Detect / receive
→ validate and deduplicate
→ classify data/asset/exposure and severity
→ assign accountable owner
→ contain when necessary
→ remediate with a scoped change
→ PR + pipeline + impacted security regression
→ QA/security retest
→ controlled release and monitoring
→ close with evidence or time-bounded approved exception
```

Remediation must update tests and, where applicable, threat assumptions, secure coding guidance, dependency decisions, SBOM, monitoring, support knowledge, and handover material.

### 10.3 Emergency security update

An emergency update is expedited, not uncontrolled:

- create an incident/change reference and narrowly scoped branch;
- identify affected versions/environments through build/SBOM evidence;
- preserve evidence before destructive containment where safe;
- prefer the smallest safe patch or verified dependency update;
- execute mandatory build, secret/dependency/static/config scans and impacted tests; any reduced non-critical test scope is explicitly risk-approved and completed after stabilization;
- create a new immutable artifact with digest/SBOM, obtain authorized expedited approval, deploy through the controlled pipeline, keep rollback/forward-fix ready, and intensify monitoring;
- complete post-release review and reconcile all temporary configuration or access.

Emergency change authority, notification, permissible test reduction, target SLA, and break-glass access are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`. Detailed operational execution belongs to Document 16/17.

### 10.4 Dependency compromise

The response in §4.4 applies. Additionally, do not trust a newly published version merely because its number is higher. Verify source/feed, signature/hash/provenance where supported, advisory status, transitive graph, build inputs, and the absence of introduced network/data behavior. Rebuild and redeploy affected artifacts; never patch binaries in place.

---

## 11. Security test strategy

### 11.1 Security test pyramid

| Layer | Purpose | Minimum examples | Evidence/gate |
|---|---|---|---|
| **Unit** | Prove small validation and policy rules | allowlists, limit checks, redaction, permission predicate, path normalization, safe error mapping, crypto wrapper, prompt/tool-schema policy | Runs on PR; security bug fix adds regression |
| **Integration** | Prove component and storage boundaries | API↔DB/object, local SQLite/artifact, key/credential adapter, outbox/audit, worker IPC, cleanup/tombstone, backup metadata | Controlled dependencies; no restricted data leakage |
| **Contract** | Prove stable external/IPC/event behavior | OpenAPI validation/diff, typed Feature schema/handler/authz, old client compatibility, SignalR duplicate/stale/version/reconnect, local IPC version | Breaking/insecure contract blocks without approved major/migration |
| **Authorization** | Prove deny-by-default object-aware access | cross-user/profile/environment/module/role cases, IDOR, cursor/export/range/download, support/admin no implicit document access | All negative cases pass against final policy from Doc09 |
| **Abuse/Negative** | Prove hostile/malformed/over-limit behavior | invalid inputs/files, retry/idempotency storm, brute patterns, path/query abuse, decompression, prompt injection, error leakage | Safe rejection, bounded resources, evidence without content |
| **E2E** | Prove business flow plus security controls | sign-in→tool→job/result; upload/download; secured versions; issue explicit send; delete/tombstone; outage/reconnect/recovery | Dev/QA, synthetic/approved masked data, monitored result |
| **Dependency/Static Scan** | Detect known component/source/config weaknesses | dependency, secret, SAST, unsafe/source, config/IaC, conditional container/deployment | Layered scan gate from §5 |
| **Performance/DoS** | Prove budgets, backpressure, graceful degradation | queue/retry storm, upload/resume, concurrent worker, pool pressure, large/malformed file, worst allowlisted query, cleanup/soak | Backpressure before exhaustion; numeric budgets from Doc12 |
| **VA/PT where required** | Independent/system-level attack validation | Web/API/auth/authz/file/storage/support/admin/AI/IPC/config attack surface | Mandatory before Production; §12 |

Architecture tests also enforce deployable/module boundaries, no client direct SQL, no cross-module internal write/reference, canonical Feature ID use, Assistant non-bypass, and forbidden dependency/logging/secret rules.

### 11.2 Risk-based applicability

All changes receive unit, review, build, and applicable scans. Security-sensitive changes require expanded evidence. Examples include authentication/session, authorization/role/permission, upload/download/object reference, local encryption/key storage, secured files, support screenshot/attachment, audit, deletion/retention, AI/content transfer, worker/IPC, dependency/provider, deployment/configuration, backup/restore, and monitoring redaction.

For such a feature, a technical green build is not enough. Acceptance also requires relevant integration, contract, authorization, abuse/negative, E2E, recovery, performance, and manual/VA/PT evidence.

### 11.3 Mandatory security-focused suites

#### API security tests

- HTTPS-only behavior and security-relevant response/header configuration for the selected host;
- invalid route/schema/enum/Feature ID/provider/path/URL/SQL/filter/sort rejection;
- request/body/file/count/date/page/rate/quota limits and correct 409/413/429/503 behavior;
- injection, mass assignment/over-posting, error detail, serialization, pagination cursor tamper/expiry/scope, and replay/idempotency;
- authorization before count, storage resolution, range processing, export, and download;
- durable 202 semantics, bounded retry/cancel/checkpoint/recovery, and no side effects from SignalR events;
- no restricted values in response, error, logs, telemetry, events, or generated OpenAPI examples.

#### Authentication tests

- invalid issuer, audience, signature, lifetime, not-before, token type, and malformed token return 401;
- expiry, revocation, logout/session invalidation, replay protection where applicable, and clock-boundary behavior;
- tokens use approved secure credential storage and are absent from URL, logs, DB fields, crash data, backup, and diagnostics;
- unavailable authentication service fails safely and does not grant offline/server permissions beyond the approved local session policy.

#### Authorization tests

- deny-by-default and least privilege for every server operation;
- IDOR across upload, job, result, history, secured file/version, issue/screenshot, attachment, audit, export, configuration, and admin operations;
- account/profile switch isolation; stale cursor/link/cache/SignalR event cannot bypass permission change or delete/tombstone;
- Administrator/Support has no automatic document access;
- Assistant and background worker cannot exceed the initiating identity/policy or use a different execution path;
- exact role expectations wait for Document 09; tests become release-gating once that matrix is approved.

#### File-security tests

- extension/MIME/magic/signature/structure mismatch, malformed/truncated/polyglot content, excessive page/object count, decompression/archive bomb where applicable, embedded content/action, path traversal, symlink/reparse-point and filename edge cases;
- streaming, bounded memory/CPU/time/storage, cancellation, low disk, timeout, partial transfer, digest mismatch, resume, duplicate chunk, cleanup, and orphan reconciliation;
- password remains memory-only; Unlock requires valid password/authorization and never bypasses encryption;
- result validation, no source overwrite, protected temp/intermediate/output, expiry, range download permission, and delete/tombstone race;
- final redaction removes/flattens sensitive content according to approved engine capability rather than visual overlay only;
- visual signature/paraf remains non-PKI and is not logged, audited as value, auto-saved, or auto-synced;
- issue screenshot requires preview and explicit Send; storage and support access are permission-controlled.

#### AI security tests

- public AI/translation endpoint and silent public-provider fallback are impossible;
- document/OCR/embedded instructions are treated as data and cannot change system policy/tool permission;
- canonical Feature ID, typed parameters, capability, permission, consent, and confirmation checks cannot be bypassed;
- direct SQL, shell, arbitrary code/API/URL/path/file access is rejected;
- output is untrusted until schema/result grounding and cannot claim success without actual tool state;
- prompt/chat state excludes password, token, secret, sensitive clipboard, signature, and document content by default;
- internal-provider transfer tests prove policy/classification/permission, minimization, notice/consent, timeout, quota, backpressure, and safe degradation to deterministic/local capability only.

#### Worker isolation tests

- API/worker/local worker use typed/versioned contracts and least-privileged file/storage/data access;
- Local Worker cannot obtain arbitrary SQLite, file, network, shell, or user token access through IPC;
- crash, kill, lease expiry, duplicate delivery, retry, checkpoint incompatibility, cancellation safe point, and client/SignalR disconnect preserve source and authoritative persisted state;
- resource limits, workload separation, fair bounded queue, reduced concurrency under pressure, and provider outage prevent cross-job data leakage or host exhaustion;
- temp/result ownership, profile/environment isolation, cleanup, and logs remain correct after failover/restart.

#### Security regression and abuse/load tests

Maintain a regression corpus of prior vulnerabilities, redaction/logging leaks, malformed files, auth/authz bypass attempts, public-AI routing attempts, dependency/configuration weaknesses, and recovery failures. Test data contains no real password, token, secret, signature, or unapproved Production document.

Load/abuse tests cover history cursor, audit ingest/read/export, idempotency storm, multi-worker claims, worker crash/lease expiry, upload/resume/expiry, issue spike, fourth secured-version concurrency, cleanup/archive/restore, connection pool/deadlock faults, worst allowlisted filters/sorts, large/malformed files, retry storm, and sustained-growth soak. Backpressure must activate before resource exhaustion. Numeric thresholds belong to Document 12 after benchmark (`NFR-PERF-006`).

---

## 12. VA/PT readiness

VA and penetration testing are mandatory before Production (`SR-SEC-009`) and should be repeated after material attack-surface/security-boundary changes or major remediation. Exact recurrence and trigger policy are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 12.1 Scope candidates

The approved scope should cover all deployed/applicable surfaces:

- Web UI and server API, authentication/session, authorization/IDOR, upload/download/range/export, jobs, SignalR, secured files, support/admin/audit/configuration;
- API ↔ SQL/object storage, deployment configuration, secret handling, observability leakage, and error behavior;
- Desktop/Mobile local storage, profile isolation, secure credential/key handling, file/temp/cache, custom protocol/deep link if any, and typed local IPC/worker;
- internal AI/provider and content-transfer boundary if enabled;
- dependency/provider/container/cloud/IIS exposure according to the selected deployment.

### 12.2 Readiness package

Before the engagement, provide controlled access to:

- approved scope, architecture/data-flow/trust-boundary diagrams, endpoint/OpenAPI inventory, Feature/permission map, and environment boundary;
- QA/Pilot target isolated from Production with synthetic/approved masked data and representative test accounts;
- application/build version, artifact digest, SBOM/dependency list, configuration baseline, known limitation and risk register;
- safe testing windows/limits, rate/backpressure expectations, logging/monitoring verification, backup/recovery readiness, and evidence-handling boundary;
- defect intake/retest process and accountable technical/business/security contacts, without embedding the operational incident guide in Document 08.

### 12.3 Completion gate

Critical/High findings must be remediated and retested or receive exceptional approved risk handling before Production. Evidence includes original finding, affected version, remediation PR/build, regression test, retest result, and released artifact. The approved VA/PT provider, independence requirement, methodology, rules of engagement, evidence classification/retention, test frequency, and acceptance authority are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

## 13. Incident principles

Document 08 defines principles only. The operational `INCIDENT_SUPPORT_GUIDE`, contacts, detailed playbooks, communication templates, escalation, service targets, and exercises belong to Document 16.

### 13.1 Principles

1. **Prepare:** maintain ownership, safe observability, traceable release/SBOM, backups, restore evidence, and controlled emergency access.
2. **Detect and validate:** correlate alerts, audit, build/dependency evidence, Job ID, Correlation ID, application version, and safe diagnostic context; do not collect document content by default.
3. **Triage:** assess affected asset/data/environment/version, exploitability, scope, service and privacy impact, and whether containment is urgent.
4. **Contain:** use the least destructive reversible measure that stops exposure—permission/configuration restriction, credential revocation, component isolation, workload pause, or controlled rollback/forward-fix—without granting investigators automatic document access.
5. **Preserve evidence:** capture time-synchronized read-only copies or exports of relevant safe logs, audit, configuration, pipeline, SBOM, artifact digests, dependency packages/advisories, access records, and affected identifiers. Record collector, time, source, integrity digest, access, transfer, and disposition. Do not place passwords, tokens, secrets, document/OCR content, or signature data into the incident record.
6. **Eradicate and recover:** remediate root cause, rotate exposed material, rebuild from controlled clean source, restore/reconcile when required, rerun impacted security tests, deploy a new verified artifact, and monitor closely.
7. **Learn and update:** link findings to requirement/control/test/dependency/design changes and confirm temporary containment is removed or formalized.

The official incident severity, notification/legal/privacy triggers, evidence chain-of-custody, forensic tooling, evidence retention/legal hold, investigation access, and authority for destructive containment are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 13.2 Key/secret and dependency incidents

- Suspected secret exposure triggers revocation/rotation, usage review, dependent service validation, and repository/history/artifact/log review. Deleting the visible value alone is insufficient.
- Suspected dependency compromise follows §4.4 and §10.4; affected artifacts are identified through SBOM/build evidence and replaced by newly rebuilt artifacts.
- Rotation or dependency containment must not silently break the ability to recover approved encrypted backups; key recovery/retirement decisions follow approved policy.

---

## 14. Backup/restore security

### 14.1 Backup protection

| Store | Required security controls |
|---|---|
| SQL Server | Database and transaction-log strategy according to selected recovery model; encryption, least-privileged access, integrity verification, retention, monitoring, restore drills; verified recovery point before migration |
| Secure object/file storage | Encrypted/protected payload; permission checked before resolution; metadata and payload recovery set linked by opaque reference, version, digest, key reference, tombstone, retention/legal hold; max three active/addressable secured versions revalidated |
| SQLite/local artifacts | Profile-scoped protected backup of DB plus required app-managed artifacts and integrity manifest; WAL/SHM/temp/migration backup in same protection boundary; exclude mobile local-only data from automatic cloud/device backup; never overwrite external source |
| Pipeline/artifact/config evidence | Controlled access, integrity metadata, retention, and recovery sufficient to reproduce/verify a release without embedding secrets |

SQL backup alone is not a complete product recovery. Backup readers do not automatically obtain business permission to browse document payloads. Backup copies, keys, catalogs, restore staging, and logs must be environment-scoped and access-audited.

RPO/RTO, SQL edition/topology, full/differential/log schedule, HA/DR, backup target, immutable/offline copy, geographic placement, retention, key escrow, and operator roles are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` in Document 17/DBA review.

### 14.2 Restore validation

A restore drill is successful only when it verifies:

- backup integrity and authorized key/credential use;
- each module schema, migration history, constraints, owner permissions, and Production startup compatibility;
- object existence, version, digest, encryption/key reference, tombstone, retention/legal hold, and orphan reconciliation against SQL metadata;
- outbox/idempotency, job/attempt/lease, audit reconciliation, and no duplicate logical effect;
- maximum three active secured-file versions;
- local profile binding, SQLite plus artifact-manifest consistency, account isolation, and no source overwrite;
- deleted/tombstoned data and expired links do not become accessible again;
- safe monitoring/audit evidence with no restricted content leakage;
- application security smoke tests after recovery and measured recovery time/data point for comparison to approved objectives.

A corrupt local database must not be silently deleted/reset. Preserve the original, use read-only diagnostics, and restore only from the last validated protected recovery snapshot when available. Restore-drill frequency, acceptance owner, and RPO/RTO thresholds are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

## 15. Release security gate

### 15.1 Release security checklist

```text
[ ] 01 Scope is approved and traceable: Requirement → Feature → Code → Test → UAT → Release.
[ ] 02 Threat/abuse, data classification, trust boundary, and security impact are reviewed for the change.
[ ] 03 PR/merge policies and required human review passed; no unexplained bypass.
[ ] 04 SDK/packages are pinned and restored from approved sources; dependency approval is complete.
[ ] 05 Secret, dependency, static, unsafe/source, configuration, API contract, and applicable deployment/container scans passed.
[ ] 06 No confirmed secret remains exposed; any exposed credential was revoked/rotated and investigated.
[ ] 07 License review and notices/obligations are complete.
[ ] 08 SBOM and resolved dependency graph are linked to commit/build/artifact.
[ ] 09 Release build is reproducible to the approved level and created on a controlled pipeline agent.
[ ] 10 Artifact, migration, configuration package, and SBOM have verified integrity metadata.
[ ] 11 Unit, integration, contract, authorization, abuse/negative, E2E, security regression, and performance tests passed as applicable.
[ ] 12 API/authentication/authorization/file/AI/worker security suites passed as applicable.
[ ] 13 VA/PT and retest evidence are complete before Production.
[ ] 14 No unresolved Critical/High finding exists except an explicit approved, expiring exceptional risk acceptance.
[ ] 15 Dev/QA/Prod separation is verified; Production data use/masking is compliant; debug is off in Production.
[ ] 16 Production receives the same controlled artifact tested in QA; digest is verified before deploy.
[ ] 17 Deployment identity and application identities are least-privileged; secrets are injected from the approved mechanism.
[ ] 18 Production effective configuration is reviewed, protected, and drift/security checked; no public AI fallback.
[ ] 19 Database migration is reviewed/idempotent, compatibility-validated, backed by a verified recovery point, and has rollback/forward-fix plan.
[ ] 20 Structured logging, OpenTelemetry, Application Insights/APM, safe diagnostics, alerts, and audit integration are active without restricted content.
[ ] 21 Capacity/backpressure and load/abuse outcomes meet the approved Test Plan; no security control is bypassed under pressure.
[ ] 22 Backup protection and restore/reconciliation drill meet the approved release requirement.
[ ] 23 Deployment, security, recovery, user/admin, API/database, and known-limitation documentation are current.
[ ] 24 QA/UAT/security/Production approvals are recorded under the approved Pertamina authority matrix.
[ ] 25 Post-deployment security smoke checks, monitored version, rollback/forward-fix readiness, and observation window are active.
```

Gate outcome:

- **GO** — all applicable mandatory controls and approvals pass.
- **NO-GO** — a mandatory control/evidence is missing, a release-blocking finding remains, secrets/config/environment are unsafe, or required VA/PT/restore evidence is absent.
- **EXCEPTIONAL RISK ACCEPTANCE** — only through the approved Pertamina authority, narrowly scoped, evidenced, compensating-controlled, monitored, expiring, and tracked to remediation. It cannot legalize a prohibited baseline such as public AI fallback, password persistence, unprotected secret storage, or bypass of explicit user consent.

The final approver matrix and exception authority are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` and must align with Documents 09 and 17.

### 15.2 Handover security checklist

```text
[ ] Controlled Azure DevOps repository, branch policies, build/release pipeline, access owners, and evidence locations handed over.
[ ] Pinned SDK, central package inventory, approved feeds, dependency decision records, license obligations, vulnerability status, and SBOM handed over.
[ ] Build reproduction, artifact integrity verification, environment promotion, rollback/forward-fix, and migration procedure demonstrated.
[ ] Dev/QA/Prod configuration ownership, secret injection, key/secret rotation principles, and access review responsibilities documented without disclosing secret values.
[ ] Security architecture, trust/data-flow boundaries, data classification mapping, threat/abuse decisions, and known approved limitations documented.
[ ] Security test pyramid, automated suites, test data restrictions, scan baselines/suppressions, VA/PT findings/retests, and acceptance evidence transferred.
[ ] Application Insights/OpenTelemetry dashboards, alerts, safe logging rules, Correlation/Job ID investigation flow, and audit separation demonstrated.
[ ] Backup protection, object/SQL/local consistency, restore drill, key dependency, RPO/RTO ownership, and recovery evidence transferred.
[ ] Vulnerability intake, emergency update principle, dependency compromise principle, and Document 16 incident-guide linkage documented.
[ ] Production access, deployment, security, support, DBA, and escalation owners confirmed through approved role/operating documents.
[ ] Technical/admin/user/API/database/security documentation and knowledge-sharing sessions completed.
[ ] Side-by-side maintenance and open-risk remediation ownership are recorded; no unresolved Critical/High is silently transferred.
```

---

## 16. Acceptance criteria

The Agent D Secure SDLC design is acceptable for integration into Document 08 when all criteria below are satisfied:

| ID | Acceptance criterion |
|---|---|
| `AC-SDLC-01` | The lifecycle explicitly covers Design, Coding, Build, Test, Deploy, Operate, Incident, and Update with evidence at each exit. |
| `AC-SDLC-02` | All 45 requested control topics are addressed and traceable to a report section. |
| `AC-SDLC-03` | Azure DevOps Git/Pipeline, PR review, controlled artifact promotion, Dev/QA/Prod, central package management, OpenTelemetry, Application Insights/APM, and structured logging remain locked. |
| `AC-SDLC-04` | Secure coding includes trust-boundary validation, auth/authz, file/content/AI handling, safe logging, resource bounds, recovery, and unsafe-code review. |
| `AC-SDLC-05` | PR merge is blocked by failed required review, tests, scans, contract/migration compatibility, or unresolved release-blocking findings. |
| `AC-SDLC-06` | SDK/packages are pinned; dependencies require purpose, license, vulnerability/security, compatibility, platform/resource, maintenance, and PoC review where applicable. |
| `AC-SDLC-07` | CI has layered secret, dependency, static, unsafe/source, configuration, API contract, deployment, conditional container, and applicable dynamic scanning without selecting an unapproved paid product. |
| `AC-SDLC-08` | Each release has traceable source, pipeline, tests, scans, SBOM, artifact digest, configuration/migration version, approvals, and monitored application version. |
| `AC-SDLC-09` | Secrets are absent from source/artifacts/ordinary DB/logs and are injected through an approved least-privileged mechanism; Production configuration is protected. |
| `AC-SDLC-10` | Local SQLite/WAL/SHM/temp/backup/artifact protection and mobile backup exclusion have an approved implementation and passing security tests before release. |
| `AC-SDLC-11` | Monitoring detects security-relevant anomalies using safe metadata and preserves separation among observability, Enterprise Audit, User History, and Support data. |
| `AC-SDLC-12` | Vulnerability severity, ownership, remediation, retest, exception expiry, emergency update, and dependency-compromise handling are governed and evidenced. |
| `AC-SDLC-13` | The complete security test pyramid is applied proportionally; security-sensitive features cannot pass on technical green alone. |
| `AC-SDLC-14` | API, authentication, authorization, file, AI, worker isolation, security regression, and abuse/load test suites cover the locked boundaries. |
| `AC-SDLC-15` | VA/PT is completed before Production, with Critical/High findings handled and remediation retested. |
| `AC-SDLC-16` | Incident content is limited to principles and evidence-preservation boundaries; the operational guide remains assigned to Document 16. |
| `AC-SDLC-17` | Backup protection covers SQL, object storage, SQLite/local artifacts, keys, access, integrity, and retention; restore validates cross-store security and deletion/tombstone invariants. |
| `AC-SDLC-18` | Release and handover security checklists are completed with accountable evidence, not self-attestation only. |
| `AC-SDLC-19` | Every unstated official policy, authority, product, threshold, cadence, or topology is marked `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`. |

---

## 17. Pertamina confirmation gaps

The following items are not locked by Documents 01–07 and must not be presented as existing official Pertamina controls:

| Gap | Needed decision | Status / downstream owner |
|---|---|---|
| Official data classification | Map Document 07 working classes `I/C/R/X/A` to official labels, handling, sharing, encryption, retention, and disposal | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` — Document 08/security owner |
| Secure coding standard | Approved .NET/C#/Web/Mobile standard, analyzer rule set, warning baseline, unsafe-code exception | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Azure DevOps branch policy | Protected branches, reviewer count/code owners, self-approval, bypass, commit signing, evidence retention | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Security toolchain | Approved secret/SAST/DAST/dependency/config/IaC/container tools, licensing, data residency/source-upload restrictions | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Dependency/license governance | Approver, package feeds/source mapping, license allow/deny/obligation rules, advisory source/cadence | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| SBOM/provenance | Required format/version, artifact signing/attestation, key/certificate owner, storage/retention/disclosure | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Build baseline | Exact SDK patch at bootstrap, agent image, deterministic/reproducibility target, artifact retention | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Secret/key platform | Approved secret manager/KMS/HSM, deployment identity, rotation schedule, break-glass, recovery escrow | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| SQLite/local protection | Encryption provider, key/WAL/SHM/temp/crash/backup controls, logout, destruction/secure erase, mobile backup exclusion | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` — governed Document 07→08 handoff |
| Environment/host security | Subscription/network/host/IIS or other topology, hardening, endpoint protection, Production access and masking standard | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` — Document 17/deployment owner |
| Container applicability | Whether containers are used and, if so, base-image/registry/runtime policy | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`; scanning remains conditional |
| Role and permission matrix | Deployment, database, storage, support, audit, config, backup, restore, and approval authorities | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` — Document 09 |
| Audit governance | Approved security event catalog, integrity mechanism, legal retention, purge/hold/export access | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` — Document 10 |
| Alerting/SOC operations | Thresholds, routing, acknowledgement/escalation, on-call/SOC/SIEM, retention | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Vulnerability governance | Scoring method, SLAs, exploitability adjustment, exception/risk authority, waiver expiry, disclosure intake | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Security test thresholds | Numeric performance/DoS/capacity budgets and environment-specific acceptance | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` — Document 12 after benchmark |
| VA/PT governance | Provider/independence, methodology, rules of engagement, cadence, evidence handling, retest/acceptance authority | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Incident operations | Severity, contacts, escalation, notification/legal/privacy triggers, forensics, custody/retention, exercises | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` — Document 16 |
| Backup/DR operations | SQL topology/recovery model, RPO/RTO, schedules, HA/DR, immutable/offline copy, key recovery, drill frequency | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` — Document 17/DBA |
| Emergency update | Change authority, permitted expedited path/test reduction, break-glass control, notification and target SLA | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` — Documents 16/17 |

---

## 18. Risks/conflicts

### 18.1 Cross-document audit result

No material contradiction was found among Documents 01–07 for Secure SDLC, build, test, monitoring, or operational security. The following boundaries must remain explicit to avoid future conflict:

| Risk/conflict | Assessment | Required treatment |
|---|---|---|
| Working data classes could be mistaken for official Pertamina classes | Open handoff, not a contradiction | Keep `I/C/R/X/A` as technical working labels until official mapping is confirmed |
| Mandatory VA/PT exists without provider/cadence/method | Baseline is clear; implementation policy open | Do not select a product/provider; confirm governance before engagement |
| Container scanning requested while deployment may be IIS/Azure or another target | Deployment topology not locked | Scan actual deployment artifacts; make container scan conditional |
| Artifact signing could be confused with Sign PDF | Different boundaries | State that build signing/provenance does not expand visual non-PKI Sign PDF scope |
| Observability could be used as audit/history storage | Violates Documents 06–07 separation | Keep security logs/telemetry, Enterprise Audit, User History, and Support separately governed |
| Detailed incident runbook could duplicate Document 16 | Scope conflict | Document 08 contains principles/evidence boundary only; operational guide stays in Document 16 |
| Role or approver names could pre-empt Document 09 | Scope conflict | Define least-privilege/separation principles only; confirm named roles in Document 09 |
| Numeric load/DoS or alert values could be invented | Benchmark/policy values are open | Route performance thresholds to Document 12 and alert thresholds to approved operations policy |
| RPO/RTO, HA/DR, immutable copy could be prematurely locked | Governed Document 17/DBA handoff | Require decision and restore evidence without inventing values/topology |
| SQLite encryption/key controls are required but provider is unlocked | Release risk | Complete dependency approval, implementation design, mobile backup exclusion, and security tests before release |
| Scanner/SBOM/security platform choice could imply a paid product commitment | Commercial decision not authorized | Specify capabilities and gates, not vendors; baseline cannot rely only on paid add-ons |
| Emergency pressure could bypass controls | Operational risk | Use expedited tracked pipeline/approval; never in-place patch or undocumented secret/config change |
| Automated scanning could upload source or metadata to public services | Data/security risk | Approve tool data flow/residency and prefer controlled/internal mode where policy requires |
| AI security test could accidentally transmit content to public AI | Locked prohibition | Use local/approved internal provider only; test public routing as a blocked negative case |
| Backup restore could resurrect deleted or unauthorized files | Cross-store consistency risk | Revalidate tombstones, permissions, key references, object digests, retention, and max-three versions |
| Excessive monitoring could leak restricted content or create DoS through cardinality | Privacy/capacity risk | Use allowlisted safe fields, minimization, bounds/sampling/retention after approved policy |

### 18.2 Residual risk statement

The design is lifecycle-complete at principle/control level, but Production readiness remains conditional on the confirmations in Section 17, completion of the security test pyramid, VA/PT, approved role/audit policies, SQLite/local protection, numeric capacity gates, and demonstrated backup/restore. These are governed handoffs, not permission to weaken the locked baseline.

---

## 19. Suggested Document 08 text

The following text is ready for Integrator adaptation into Document 08.

### 19.1 Secure SDLC policy

PERTAMINA OTO applies security throughout Design → Coding → Build → Test → Deploy → Operate → Incident → Update. Each stage produces traceable evidence linked from Requirement/Feature to code, tests, artifact, approval, deployment, and monitored version. A security-sensitive feature is not accepted solely because build and unit tests are green; all applicable business, authorization, abuse/negative, privacy, recovery, performance, and security evidence must pass.

### 19.2 Repository and coding

Source code is maintained in controlled Azure DevOps Git. Protected branches require reviewed PRs and successful mandatory pipeline gates. Changes are short-lived, scoped, and traceable. Secure coding enforces typed allowlists and bounds, object-aware authorization, parameterized/module-owned data access, safe file and untrusted-content handling, resource limits, bounded retry/idempotency, protected temporary/output handling, and safe structured logging. `unsafe`, unmanaged interop, shell/process execution, arbitrary path/URL/SQL/API access, insecure serialization, custom cryptography, and disabled TLS validation are denied by default and require narrowly approved exceptions. Exact branch and analyzer policy is `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 19.3 Dependencies and CI scanning

The approved .NET SDK patch is pinned in `global.json`; package versions are centralized in `Directory.Packages.props`; floating and silent upgrades are prohibited. Every dependency requires purpose, license, security/vulnerability, compatibility, platform/resource, maintenance, and fallback review, plus PoC where required. CI performs automated secret, dependency, static, unsafe/source, configuration, API-contract, and deployment scans; container/image scanning is conditional on container use, and dynamic scanning is applied to the deployed QA attack surface. A scan is not disabled merely to obtain a green build. The approved toolchain and severity thresholds are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 19.4 Build, SBOM, and artifact integrity

Controlled clean agents execute restore, Release build, tests/scans, packaging, SBOM generation, integrity metadata, and artifact publication. Every release artifact, migration, configuration package, and SBOM is linked to source commit and pipeline run and protected by a verified digest in controlled artifact storage. The same artifact is promoted through Dev, QA, and Production; environment values are injected separately. Production rejects ad-hoc binary, database, and configuration changes. Required SBOM format, signing/provenance mechanism, reproducibility target, and retention are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 19.5 Secrets, configuration, and environments

Passwords, tokens, service secrets, and encryption keys are never ordinary database fields, source, artifacts, logs, telemetry, audit, or support attachments. They are injected at deployment/runtime from an approved mechanism through least-privileged environment-specific identities. Production configuration is schema-validated, protected, change-audited, drift-checked, and keeps debug disabled. Dev, QA, and Production separate identity, endpoint, data/storage, secret, telemetry, and deployment boundaries; Production data is not used in Development without approved masking. The secret/key platform, deployment role matrix, host/network hardening, and SQLite encryption/key/WAL/backup implementation are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 19.6 Monitoring and vulnerability management

PERTAMINA OTO uses structured logging, OpenTelemetry, and Application Insights/APM with safe identifiers such as Feature ID, Job ID, Correlation ID, error code, status, duration, component, environment, and application version. Document content, full OCR text, password, token, secret, clipboard data, signature value, and automatic issue screenshot are prohibited. Monitoring covers authentication/authorization anomalies, suspicious object/file/support access, invalid files, rate/backpressure, jobs/queues/workers, resources, transfer/storage/database, configuration changes, audit gaps, backup/restore, dependency exposure, and AI policy rejection. Enterprise Audit, User History, Support, and observability remain separate.

Vulnerabilities are validated, classified, owned, contained where necessary, remediated through a scoped PR, regression-tested, retested, released through the pipeline, and monitored. Critical/High findings block release unless handled by an exceptional approved, expiring risk acceptance. Official scoring, SLA, alert routing, exception authority, audit catalog, and retention are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 19.7 Security testing and VA/PT

The security test strategy covers Unit → Integration → Contract → Authorization → Abuse/Negative → E2E → Dependency/Static Scan → Performance/DoS → VA/PT where required, with architecture tests as an additional boundary gate. Dedicated suites cover API, authentication, authorization/IDOR, file security, internal/local AI, worker/IPC isolation, safe logging, secrets, deletion/tombstone, backup/restore, and prior security regressions. Numeric performance/DoS criteria are approved in Document 12 after benchmark. VA/PT is completed before Production, and Critical/High remediation is retested. Provider, methodology, cadence, evidence handling, and acceptance authority are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 19.8 Incident and emergency update boundary

Document 08 defines prepare, detect, triage, contain, preserve evidence, eradicate/recover, and learn/update principles. Evidence uses time-synchronized, integrity-verifiable safe logs, audit, configuration, pipeline, SBOM, artifact digest, dependency, access, Job ID, and Correlation ID context without collecting restricted content by default. Emergency security updates remain tracked, reviewed, scanned, tested, built as new artifacts, approved, deployed through the controlled pipeline, rollback/forward-fix ready, and monitored. Dependency compromise freezes affected inputs, identifies impact through SBOM/build evidence, rotates exposed credentials, rebuilds from verified inputs, and redeploys. The operational `INCIDENT_SUPPORT_GUIDE` remains in Document 16; severity, contacts, notification, custody, emergency authority, and playbooks are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 19.9 Backup, restore, release, and handover

Backup security covers SQL database/log according to the selected recovery model, secure object/file payload plus metadata, SQLite plus required local artifacts, encryption, least privilege, integrity, retention, monitoring, and restore drills. Restore reconciles schema/migration/permissions, SQL metadata with object version/digest/key/tombstone/retention, audit/outbox/idempotency, profile isolation, deletion state, and the maximum-three secured-file versions. It never silently resets corrupt local data or overwrites external source files. RPO/RTO, topology, schedule, HA/DR, immutable/offline copy, key recovery, and drill frequency are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` in Document 17/DBA review.

Production release requires complete PR/build/scan/test/SBOM/artifact/configuration/migration/VA-PT/backup-restore/monitoring/documentation evidence and approved QA/UAT/security/deployment decisions. Handover transfers repository/pipeline, dependency/SBOM, build/integrity, environment/secret, architecture, test/VA-PT, monitoring/audit, backup/restore, vulnerability/emergency, incident-guide linkage, documentation, knowledge sharing, and open-risk ownership without exposing secrets.

---

**Integrator note:** No official Pertamina standard has been inferred where Documents 01–07 are silent. Resolve every `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` item through the appropriate governance owner before marking the associated Production gate complete.
