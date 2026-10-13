# AGENT C — KNOWLEDGE TRANSFER, SUPPORT, MAINTENANCE & OPERATIONAL READINESS REPORT

**Task ID:** `DOC19-AGENT-C`  
**Status:** specialist recommendation for Integrator review; not a change to master Document 19  
**Authoritative baseline read:** Documents 01–18 FINAL, `MASTER_PROJECT_SUMMARY.md`, Incident Runbook Register, Incident Communication Template Register, Deployment Runbook Register, Release Checklist Register, and Document 18 cartography/register references  
**Product baseline:** 550 canonical Feature IDs; 33 Primary PDF Tool Cards  
**Readiness baseline:** pre-implementation; no actual RC, implementation, operational drill, UAT sign-off, Production deployment, or accepted handover is claimed

> Decisions, appointments, access grants, dates, participants, official contacts, policy values, SLA, and acceptance remain with the user, Integrator, and authorized Pertamina functions. This report creates a handover model and evidence gates only. It does not create application roles, permissions, support contacts, or Production facts.

---

# 1. KT strategy

## 1.1 Objective and outcome

Knowledge Transfer (KT) is successful only when the receiving functions can independently explain, locate, operate, diagnose, maintain, test, and govern PERTAMINA OTO within their approved scope. Presentation attendance alone is not competence evidence.

The handover follows this outcome chain:

```text
Authoritative baseline
→ role-scoped learning path
→ demonstration
→ hands-on exercise
→ teach-back by recipient
→ evidence review
→ gap correction / repeat where needed
→ function acceptance
```

## 1.2 Governing principles

1. Use Documents 01–18 FINAL as the source hierarchy; derived slides, recordings, diagrams, and KB articles never replace them.
2. Separate product knowledge, technical knowledge, operations knowledge, privileged knowledge, and restricted security knowledge by need-to-know.
3. Teach canonical Requirement ID, Feature ID, module, interface, data owner, permission, Audit, test, runbook, and artifact relationships rather than memorizing UI labels.
4. Demonstrate with synthetic or sanitized-approved data; never use passwords, tokens, user documents, OCR text, signature assets, or Production secrets.
5. Every hands-on exercise has an expected outcome, safe stop condition, forbidden action, and evidence reference.
6. Actual environment, build, version, configuration, and material versions must be recorded for each delivered session.
7. A session may be delivered but remains unvalidated until the recipient passes the applicable teach-back.
8. Open questions become governed follow-up items; they are not answered by assumption.
9. KT status cannot be raised because a recording exists or a presenter says the material was understood.
10. Content and operational instructions are revised when release, architecture, security, incident, provider, platform, or ownership changes.

## 1.3 Audience tracks

| Track | Primary receiving functions | Required emphasis |
|---|---|---|
| Product/business | Business Owner, Product Owner, analyst, user representative | Business outcomes, 550/33 baseline, lifecycle, limitations, UAT |
| Engineering/maintenance | Application/Technical/Module owner, developer, reviewer, Integrator | Architecture, repository, contracts, data, tests, security, Codex workflow |
| Operations/deployment | Operations, Deployment, Infrastructure, DBA/storage/backup functions | Health, dependencies, capacity, rollout, rollback, backup/restore, evidence |
| Support | Service Desk/Support L1, application L2/L3 functions, Incident Owner | Issue intake, safe diagnostics, triage, escalation, runbooks, Known Error, KB |
| Security/audit/admin | Security Reviewer, Administrator, Auditor | Least privilege, content boundary, Audit, evidence handling, reauthentication |
| Training/documentation | Trainer, Documentation Owner | User/Admin manuals, versioning, screenshots, task validation, publication |

Function labels are not new application roles. Formal application roles remain User, Administrator, and Auditor; Support/Superadmin remains a permission-scoped profile.

## 1.4 KT lifecycle

| Status | Meaning | Minimum evidence |
|---|---|---|
| `PLANNED` | Scope and materials proposed; no schedule or delivery claim | Session record with objective, audience, prerequisites, material version |
| `SCHEDULED` | Official time, presenter, and invite confirmed outside this report | Approved schedule and participant invitation |
| `DELIVERED` | Session and exercise were actually conducted | Attendance, material version, recording/reference where permitted, exercise output |
| `VALIDATED` | Required teach-back passed | Assessor result, pass criteria, evidence, closed critical gaps |
| `REPEAT_REQUIRED` | Competence or evidence did not meet the gate | Failed criteria, remediation, repeat scope |
| `ACCEPTED` | Authorized receiving function accepts transfer for its scope | Acceptance record, residual gaps/backlog, ownership effective point |

Current status for the catalog in this report is `PLANNED (PROPOSAL_ONLY)`. No session is represented as scheduled, delivered, validated, or accepted.

## 1.5 Readiness boundary

- Documents 14–18 are FINAL pre-implementation baselines, not release/operations evidence.
- Document 18 lifecycle is `DESIGNED`; `HANDOVER_READY` and `PRODUCTION_READY` are explicitly not available.
- KT can begin on conceptual governance, but build/run/deploy/monitor exercises require an actual repository, environment, artifact, RC, access, and approved procedures.
- Missing prerequisites produce `BLOCKED` exercise evidence or `REPEAT_REQUIRED`; they never produce a synthetic PASS.

---

# 2. KT session catalog

## 2.1 Required session record template

```text
KT ID:
Title:
Objective:
Audience:
Presenter Function:
Prerequisite:
Materials:
Demo:
Hands-On Exercise:
Expected Competence:
Q&A:
Recording/Reference:
Attendance:
Validation Result:
Open Questions:
Follow-Up:
Status: PLANNED | SCHEDULED | DELIVERED | VALIDATED | REPEAT_REQUIRED | ACCEPTED
```

Do not populate actual date, presenter name, participant name, attendance, recording URL, or result until evidence exists.

## 2.2 Minimum 16 sessions

| KT ID / Title | Objective | Audience / Presenter function | Prerequisite and materials | Demo and hands-on exercise | Expected competence | Initial status |
|---|---|---|---|---|---|---|
| `KT-01` Product and business process | Explain product purpose, actors, Triple Entry Flow, Local-First, user outcome, and governance | Business/Product, analysts, Support, trainer / Business Owner + System Analyst | D01, D02, master summary | Trace File-First, Tool-First, and Assistant-First to the same business outcome; identify where user confirmation is required | Explain product boundaries and one end-to-end process without changing a business rule | `PLANNED` |
| `KT-02` Requirements and 550 Features | Navigate requirement taxonomy and canonical feature registry | Product, analyst, maintainer, QA, Support / Business Analyst + Product function | D02, D03, D18 | Find a Requirement ID, resolve its Feature ID, status, dependency, and traceability; identify the `FR-FILE-006..009` gap without inventing IDs | Locate authoritative requirement/feature facts and distinguish catalog completeness from implementation | `PLANNED` |
| `KT-03` 33 Core Tool Cards | Explain all Primary PDF Tool Cards, bundles, risk, mode, PoC, tests, and limitations | Product, maintainer, QA/UAT, Support, trainer / Product + Module/QA functions | D03, D05, D12–D14, D18 §22 | Trace selected low-risk, PoC-gated, and high-risk Core cards from Feature to UAT/manual/runbook | Explain 33/33 surface and why a Tool Card is not a service/endpoint/table | `PLANNED` |
| `KT-04` Architecture and module boundaries | Teach Modular Monolith, Clean Architecture, separate deployables, modules, building blocks, and dependency rules | Maintainer, architect, QA, Security, Operations / Application Architect | D04, D05, D18 §§15–21 | Draw one module path and reject client→SQL, cross-module infrastructure, SignalR authority, and Assistant bypass | Identify module owner, allowed dependency direction, host, and data boundary | `PLANNED` |
| `KT-05` Repository, branch, and Codex | Transfer monorepo, protected branch, worktree/task isolation, review, and Agent workflow | Maintainer, Integrator, reviewer / Technical Owner + Integrator | D02 §§17–18, D05 TECH-DEC-004/015, D17 §§27–31, D18 §72 | Draft a scoped Codex task with exact allowed and do-not-touch paths; review a synthetic diff/task handoff | Create a safe scoped task and explain the human merge/release gate | `PLANNED` |
| `KT-06` API and integration | Explain REST authority, OpenAPI, authentication, idempotency, pagination, transfer, Job, SignalR, and IPC boundaries | API/integration maintainer, QA, Security, Support / API Owner + Security function | D06, D08, D18 §§34–38 | Read an approved contract; map `401/403/409/412/413/429/503`; reconcile SignalR via REST | Read contract/version/security/retry behavior without inventing an endpoint | `PLANNED` |
| `KT-07` Database, SQLite, and storage | Explain schema ownership, SQL metadata/object bytes, SQLite/local workspace, migration, retention, and max-three | Data/DB, maintainer, Operations, Support / Data/Database Owner | D07, D08, D17 §§37–40/62–64, D18 §§42–47 | Trace local and server data placement; analyze a metadata/object mismatch and SQLite recovery case | Explain SOT, migration/recovery boundary, and why direct repair is prohibited | `PLANNED` |
| `KT-08` Security, role, and permission | Explain trust boundaries, deny-by-default, content separation, offline/reauth, Worker/provider/AI security | Security, Admin, Auditor, Support, maintainer / Security Owner | D08, D09, D15, D18 §§48–52 | Evaluate wrong-owner, Admin content request, Support screenshot, and Assistant prompt-injection cases | Apply least privilege and distinguish role/profile/permission/ownership/content access | `PLANNED` |
| `KT-09` Audit and observability | Separate Enterprise Audit, History, logs/telemetry, Issue, and restricted evidence | Operations, Support, Admin, Auditor, maintainer / Audit + Observability functions | D10, D15 §§40–45, D16, D18 §§59–60 | Find a Correlation ID across safe planes; perform bounded audit-search design; identify excluded fields | Read health and Audit without copying content or treating telemetry as Audit truth | `PLANNED` |
| `KT-10` Testing and UAT | Explain Unit/Test/UAT layers, risk coverage, evidence, defect/retest, and eight-layer DoD | QA/UAT, maintainer, Product, Support / QA Lead + Business/UAT function | D11–D13, D02 §17 | Map one Core Feature to Unit, integration, security, recovery, platform, UAT, and release evidence | Determine PASS/FAIL/BLOCKED correctly and reject build-only completion | `PLANNED` |
| `KT-11` User/Admin operation | Transfer User Manual, Admin Manual, task flow, high-risk communication, and content-access separation | Trainer, User representative, Admin, Support / Documentation + Admin functions | D14, D15 | Perform user Merge baseline walk-through and a safe admin policy-change rehearsal with reauth/Audit expectations | Guide users/admins at their scope without exposing restricted controls/content | `PLANNED` |
| `KT-12` Incident and support | Transfer Issue→Incident, triage, evidence, communication, escalation, Known Error, and KB | Support L1/L2/L3 functions, Operations, Security, maintainer / Support Owner + Incident Owner | D16 and its registers; D15 §§35–39 | Handle a synthetic Issue, convert/escalate when criteria apply, use one communication template, and close only with evidence | Follow lifecycle/runbook with safe evidence and explicit ownership handoff | `PLANNED` |
| `KT-13` Deployment and rollback | Explain immutable artifact, environment isolation, release gates, migration, rollout, rollback, and evidence | Release/Operations, DBA, QA, Security, maintainer / Deployment Owner | D17 and release/deployment registers; D18 §§53–58 | Conduct QA deployment rehearsal design and failed-release rollback decision using exact artifact/config/version | Explain build-once/promote, stop conditions, rollback compatibility, and approval boundary | `PLANNED` |
| `KT-14` Backup, restore, and recovery | Explain operational backup versus Amankan ke Akun; SQL/object/key/config/AuthZ/Audit reconciliation | Backup/DBA/storage, Operations, Security, Support / Backup/Recovery + Data owners | D07 §§17–20, D16, D17 §§62–64/78 | Walk through isolated restore evidence and reject reopening with incomplete object/AuthZ/Audit reconciliation | Explain full restore validation and why backup without restore test is not ready | `PLANNED` |
| `KT-15` Application Cartography | Transfer context, capability, module/component/interface/data/deployable/zone/owner/lifecycle maps | All technical/operations/support receivers / Application Owner + Cartography Owner | D18 FINAL and its registers/diagrams | Trace one Feature end-to-end; run orphan/change-impact check; identify owner/component/artifact gaps | Navigate cartography without treating diagrams as alternate truth | `PLANNED` |
| `KT-16` PoC, limitation, and known risk | Make feasibility gates, approved limitations, residual risks, Production gaps, and release restrictions visible | Product, Architecture, Security, QA, Operations, maintainer / Product + Architecture + Security | D04 §31, D05 §5, D08 §30, D14/15 limitations, D18 §§67–71 | Review a PoC case and classify `APPROVED_ENGINE`, `APPROVED_WITH_LIMITATION`, or `BLOCKED_ALTERNATIVE_REQUIRED`; draft user-safe limitation | Explain what is unproven and prevent a limitation from becoming a false feature/readiness claim | `PLANNED` |

## 2.3 Session administration fields

For every catalog row, the following remain blank until an actual event: Q&A record, recording/reference, attendance, validation result, open questions, follow-up, schedule, presenter name, recipients, and acceptance. Recording is optional and subject to classification, participant notice, storage, access, and retention policy.

---

# 3. KT evidence

## 3.1 Evidence package

Each session must produce a traceable evidence package:

| Field | Requirement |
|---|---|
| `KtSessionId` | Exact `KT-01..KT-16` |
| Baseline | Document names/versions/digests or controlled repository references |
| Runtime context | Exact build/RC, environment, configuration/capability version when the session involves runtime behavior |
| Presenter/recipient | Actual human/function recorded by approved process, not invented in documentation |
| Attendance | Presence and completion state; attendance alone is not competence |
| Materials | Slides, guide, diagram source/render, exercise, answer key, runbook version |
| Demonstration evidence | Safe screenshots/recording/logical reference; no Production secret/content |
| Hands-on output | Worksheet, task draft, test result, runbook decision, or safe evidence package |
| Teach-back result | Assessor, criteria, result, gaps, repeat decision |
| Open questions | Owner function, decision required, due target only when officially assigned |
| Follow-up | Corrective material/exercise/repeat scope and status |
| Acceptance | Receiving function and scope accepted; residual backlog/limitations linked |

## 3.2 Evidence quality rules

- Evidence must show the recipient doing the task; a presenter demonstration cannot substitute for recipient action.
- Use stable references to authoritative content; do not paste entire sensitive payloads into KT records.
- Record failures and repeats append-oriented; do not overwrite failed attempts.
- An unavailable environment/tool is `BLOCKED`, not `PASS` or `NOT_APPLICABLE` without rationale.
- A recording, screenshot, Issue export, audit export, or runbook output inherits its classification and permission boundary.
- Evidence retention, recording location, and access are `POLICY_CONFIGURABLE` and require approved repositories.
- KT evidence does not become Enterprise Audit unless a registered Audit action requires it.

## 3.3 Acceptance evidence hierarchy

```text
Source material version
< attendance
< completed exercise
< teach-back pass
< independent rehearsal
< receiving-function acceptance
```

Higher evidence includes but does not erase lower evidence. Current report status: `NO_ACTUAL_KT_EVIDENCE_RECORDED`.

---

# 4. Teach-back validation

## 4.1 Competence matrix

| Competence ID | Recipient must be able to | Practical validation | Pass evidence | Current result |
|---|---|---|---|---|
| `TB-01` | Explain the product | Explain actors, Triple Entry, Local-First, server boundary, 33 Core, and user outcome | Structured explanation with no architecture/security contradiction | `NOT_ASSESSED` |
| `TB-02` | Find the source document | Locate the authority for business, Feature, API, DB, security, permission, test, manual, incident, deployment, and cartography facts | Correct source/version for a sampled question | `NOT_ASSESSED` |
| `TB-03` | Search Requirement ID | Find a sampled ID and explain type/module/acceptance/dependency | Correct ID and no invented range/member | `NOT_ASSESSED` |
| `TB-04` | Search Feature ID | Resolve canonical ID, alias/card, execution profile, module overlay, and status | Correct Feature, including bundle expansion where applicable | `NOT_ASSESSED` |
| `TB-05` | Explain a module | State responsibility, owned data/contracts, forbidden dependencies, hosts, and tests | Correct boundary and one-owner reasoning | `NOT_ASSESSED` |
| `TB-06` | Run build | Use exact approved SDK/source/config and record result | Reproducible build evidence tied to commit/artifact; requires actual repository | `BLOCKED_UNTIL_REPOSITORY` |
| `TB-07` | Run tests | Run applicable suites and interpret PASS/FAIL/BLOCKED/flaky correctly | Test command/output, environment, failure disposition | `BLOCKED_UNTIL_REPOSITORY` |
| `TB-08` | Read an API contract | Explain route owner, version, schema, auth, idempotency, error, rate, and SOT | Correct review of an approved OpenAPI/typed contract | `NOT_ASSESSED` |
| `TB-09` | Understand schema/migration | Trace entity/schema owner, migration compatibility, transaction, backup point, and recovery | Safe migration-review worksheet; no direct data edit | `NOT_ASSESSED` |
| `TB-10` | Read monitoring | Determine health/freshness/degradation without exposing content or confusing telemetry with Audit | Dashboard exercise on approved environment | `BLOCKED_UNTIL_MONITORING_ACCESS` |
| `TB-11` | Find Correlation ID | Use a safe ID to correlate permitted Issue/Job/log/Audit references without using it as authorization | Sanitized diagnostic path and scope checks | `NOT_ASSESSED` |
| `TB-12` | Handle a sample Issue | Intake, triage, status, safe evidence, note, communication, and escalation | Synthetic case handled within `SUPPORT_SCOPE` | `BLOCKED_UNTIL_SUPPORT_ACCESS` |
| `TB-13` | Follow an incident runbook | State facts/unknowns, choose safe checks/containment/recovery/validation/escalation | Completed tabletop worksheet with forbidden actions respected | `NOT_ASSESSED` |
| `TB-14` | Perform QA deployment rehearsal | Verify artifact/config/version/gates, execute approved rehearsal, smoke, and evidence | QA rehearsal record; no Production action | `BLOCKED_UNTIL_QA_ARTIFACT_ENV` |
| `TB-15` | Explain rollback | Classify compatibility/data state and choose rollback, forward-fix, or restore | Correct decision using `DRB-ROLLBACK-001` | `NOT_ASSESSED` |
| `TB-16` | Explain backup/restore | Distinguish product secured storage from operational backup and reconcile SQL/object/key/AuthZ/Audit | Restore tabletop or controlled drill evidence | `NOT_ASSESSED` |
| `TB-17` | Explain security boundary | Apply permission, ownership/scope, policy, reauth, content separation, and provider restrictions | Deny cases for Admin/Support/Assistant/Worker correctly handled | `NOT_ASSESSED` |
| `TB-18` | Create a scoped Codex task | Include all required task fields, exact paths, dependencies, tests, security, and performance | Integrator-reviewed task that does not expand source-of-truth scope | `BLOCKED_UNTIL_REPOSITORY_PATHS` |

The prompt lists eighteen capability statements; `TB-01..TB-18` keeps each one independently auditable. These competence IDs are handover evidence identifiers, not new product requirements.

## 4.2 Validation rule

```text
DELIVERED + attendance only                     ≠ VALIDATED
DELIVERED + presenter demo                      ≠ VALIDATED
Recipient hands-on + teach-back pass + evidence = eligible for VALIDATED
VALIDATED + receiving-function acceptance       = eligible for ACCEPTED
```

Critical safety failure—such as requesting document content for ordinary support, treating SignalR as authority, using public AI, direct SQL, bypassing permission, or claiming Production readiness—requires `REPEAT_REQUIRED` for the affected competence.

---

# 5. Support handover

## 5.1 Channel and intake model

| Channel/item | Baseline | Handover requirement |
|---|---|---|
| In-App Issue Reporting | Confirmed product path through PERTAMINA OTO | Demonstrate message submission, optional screenshot preview/explicit send, safe diagnostics, and status |
| Support Console | Permission-scoped Support/Superadmin profile | Demonstrate list/detail/triage/status/note/export with deny tests |
| Pertamina Service Desk | Official organization/channel/contact not provided | `REQUIRES_PERTAMINA_SERVICE_DESK_CONFIRMATION` |
| External phone/email/chat | Not authorized or specified | Do not invent or publish; link only after official confirmation |
| Security reporting route | Restricted official process required | `REQUIRES_PERTAMINA_SECURITY_PROCESS_CONFIRMATION` |

## 5.2 Support Console capability and boundary

| Capability | Permission baseline | Allowed information | Prohibited default access |
|---|---|---|---|
| List/detail Issue | `support.issue.list`, `support.issue.read` | Issue reference, reporter/scope-safe metadata, message, status, safe diagnostics | Unrelated Issue, general user files, raw logs |
| Explicit screenshot | `support.attachment.read` | Exact attachment intentionally sent by user | Automatic screenshot, unrelated document, clipboard, signature |
| Triage/status | `support.issue.triage` | Category, priority, status transition, duplicate/relation | New role/permission, policy bypass |
| Internal note | `support.note.read/create` | Short safe observations, Feature/Job/Correlation/Error refs, next owner | Password/token/secret, document/OCR content, raw stack/SQL/path/provider payload |
| Export | `support.export.create/read` | Permission-scoped PDF detail or bounded Excel summary | Public durable link, unrestricted dump, attachment inclusion by default |

Support escalation does not broaden access. A Job ID, Correlation ID, Error Code, object reference, or screenshot reference is an identifier, not authorization.

## 5.3 Issue workflow handover

Canonical internal Issue statuses:

```text
BARU
DIPERIKSA
PERLU_PERBAIKAN
BUTUH_INFORMASI
DUPLIKAT
BUKAN_BUG
SEDANG_DIPERBAIKI
READY_FOR_TEST
TERVERIFIKASI
SELESAI
```

The receiver must demonstrate:

1. intake and security/privacy screening;
2. scoped list/detail access;
3. safe diagnostic interpretation using Feature, Job, Correlation, Error, build, platform, and time;
4. attachment preview only when needed and permitted;
5. category/priority/status selection from approved catalogs;
6. conflict-safe update using current version;
7. safe internal note;
8. user-facing communication without internal/security detail;
9. conversion/link to Incident when impact/risk requires coordinated response;
10. escalation with explicit decision request and acknowledged owner handoff.

Exact category, priority, transitions, reopen rules, queue ownership, and Service Desk integration require Pertamina confirmation.

## 5.4 Issue-to-Incident conversion

Consider Incident conversion for active service impact, security/privacy suspicion, cross-user exposure, data corruption/false success, Audit failure, broad/repeated outage, unsafe recovery, failed deployment, or a condition requiring coordinated owner/communication. Conversion records safe Issue reference; it does not copy screenshots/content into the general Incident record.

## 5.5 Communication templates

Use Document 16 templates: `COM-ACK-001`, `COM-INFO-001`, `COM-UPDATE-001`, `COM-WORKAROUND-001`, `COM-DELAY-001`, `COM-MONITOR-001`, `COM-RESOLVED-001`, `COM-CLOSED-001`, `COM-REOPEN-001`, `COM-SEC-HOLD-001`, `COM-INTERNAL-001`, and `COM-PROVIDER-001`.

The handover validates template choice, factual wording, safe scope, unknowns, next action, and approval/channel. It does not invent response time, sender authority, audience, or contact.

## 5.6 Support metrics

Candidate safe metrics include intake count, age, status distribution, re-open/duplicate rate, Issue-to-Incident conversion, attachment failure, missing safe evidence, Known Error usage, repeat issue, and sample-case success. Targets, cadence, segmentation, and interpretation are `POLICY_CONFIGURABLE`; these metrics are not an SLA or employee leaderboard.

---

# 6. Operations handover

## 6.1 Operational dependency map

| Operational area | Authority and health signals | Safe degraded/first action | Primary runbook/reference | Handover evidence |
|---|---|---|---|---|
| API | Persisted response/Job; liveness/readiness, latency/error, authz denial, DB pool, rate/admission | Stop unsafe mutation/Job admission; retain eligible Local-First route; no false `202` | `RB-API-OUTAGE-001`, `DRB-API-FAIL-001`, D17 §§34/53–58 | Read health, correlate error, explain `429/503`, identify stop condition |
| Server Worker | Persisted Job/attempt/lease/checkpoint; heartbeat, duration, failures, resource | Stop claims, drain/release lease, bounded retry; no mass retry/forced state | `RB-QUEUE-OVERLOAD-001`, `DRB-WORKER-FAIL-001` | Analyze synthetic crash/stale completion and safe recovery |
| Local Worker | Desktop-owned local Job/manifest; IPC handshake/heartbeat/crash/version | Recycle/restart and compatible resume; no silent server fallback | `RB-LOCAL-WORKER-001`, `DRB-DESKTOP-UPDATE-001` | Recover without UI crash/source loss; verify correlation |
| Queue/Job Store | Persisted depth/age/drain/retry/lease by workload | Narrow admission/concurrency, protect fairness, preserve Local-First; no row delete | `RB-QUEUE-OVERLOAD-001`, `DRB-QUEUE-HIGH-001` | Explain arrival/service/drain, overload trigger, retry storm prevention |
| SQL Server | Module-owned metadata; readiness, latency, pool, locks/log/IO/schema | Reduce admission, use approved migration/recovery path; no direct patch | D16 Database runbook, `DRB-DB-MIG-FAIL-001` | Read aggregate DB signals and migration compatibility evidence |
| Secure Storage | Domain metadata + object/digest/version; readiness, capacity, transfer, orphan mismatch | Block false finalization, protect active/source data, reconcile through owner service | `RB-STORAGE-FAIL-001`, `DRB-STORAGE-FAIL-001` | Trace staging/finalization and max-three reconciliation |
| SignalR | Notification only; connection, revision gap, reconnect | REST reconcile; do not replay command or change Job state | `RB-SIGNALR-001` | Demonstrate duplicate/stale/gap handling and terminal-state monotonicity |
| Audit pipeline | Audit intent/outbox/store; backlog, sequence, freshness, append health | Preserve intent/outbox; block action requiring durable Audit; no disable/delete/fabrication | `RB-AUDIT-FAIL-001`, D16 §51 | Show audit-of-action reference and failure decision |
| Issue service | Issue aggregate/outbox/attachment state; submission/backlog/failure | Preserve message when attachment fails; retry same logical action safely | `RB-ISSUE-001` | Handle message-only continuation and scoped status reconciliation |
| SSO/session | Pertamina Identity plus OTO eligibility/session/policy; safe auth outcome/freshness | Online/privileged fail closed; only valid eligible local offline path continues | `RB-SSO-FAIL-001` | Distinguish identity, eligibility, session, effective access, offline expiry |
| Engine/provider | Resolver decision; profile/version/health/circuit/timeout/resource | Disable/narrow affected route; use only pre-approved fallback; never public | `RB-AI-PROVIDER-001`, D16 §§48–49 | Identify affected Features, circuit behavior, data minimization, no public fallback |
| Compatibility | Release Manifest and compatibility endpoint; current/min/contract/capability versions | Preserve local work; `UPDATE_REQUIRED/UNSUPPORTED` fail safely | `DRB-CLIENT-INCOMPAT-001`, client/version runbook | Reconcile mixed versions and update guidance |
| Feature flags/config | Typed/versioned config with effective source and Audit | Narrow exposure through official control plane; no hard-safety override | `DRB-CONFIG-BAD-001`, D15 §§21–26/47 | Review diff, scope, expiry, row version, rollback, and Audit |
| Capacity/backpressure | CPU/RAM/disk/IO/network/DB pool/queue/provider; freshness/trend | Admission before exhaustion, bounded queue/concurrency/retry/cleanup | `DRB-QUEUE-HIGH-001`, D17 §§57–58 | Read headroom/trend and choose graceful degradation |
| Secured-file operation | `secured` aggregate + object; transfer/version/quota/ownership state | Stop commit on mismatch, reconcile, preserve max-three; no manual object deletion | Storage/Data Integrity runbooks, `DRB-RESTORE-001` | Execute conceptual upload/version/retrieve/delete/restore checks |

## 6.2 Operations console/access package

The receiver needs approved access to environment-specific health, telemetry, Job/queue safe projection, deployment artifact/manifest, configuration read view, runbook/KB repository, Issue path, and restricted Audit view only where separately granted. Operations access is not Administrator, Auditor, Support, or document-content permission.

## 6.3 Operational shift/handoff record

Minimum record: environment, build/config/schema/capability versions, health freshness, active Incident/change/release, queue/resource trend, provider status, backup verification state, outstanding containment/workaround, Known Error, decisions required, next checkpoint, and receiving acknowledgement. Frequency remains `POLICY_CONFIGURABLE`.

---

# 7. Incident tabletop

## 7.1 Tabletop rule

Documents 16–17 report design reviews as passed but operational exercises as not run. Therefore this catalog is `TABLETOP_DESIGN_READY`; it must not be labelled `TABLETOP_VALIDATED`, `RELEASE_READY`, or `PRODUCTION_READY` until authorized participants perform the exercise and evidence is reviewed.

## 7.2 Twelve handover scenarios

| Scenario | Expected roles/functions | Runbook | Safe evidence | Key decision | Escalation and gap | Corrective action | Current status |
|---|---|---|---|---|---|---|---|
| API outage | Support, Incident Owner, API/Application, Operations, Security if indicated | `RB-API-OUTAGE-001`; `DRB-API-FAIL-001` | Time/environment/build, readiness, safe error/latency, Correlation ID, dependency freshness | Degrade/hold admission, restore dependency, rollback or forward-fix; keep valid local route | Owner/contact/target/topology: organization/infra confirmation | Run controlled exercise; close monitoring/route/owner gaps | `DESIGN_READY; EXERCISE_NOT_RUN` |
| Local Worker crash | User support, Desktop/Worker maintainer, Incident Owner | `RB-LOCAL-WORKER-001`; `DRB-DESKTOP-UPDATE-001` | Local Job, IPC/Worker version, heartbeat, checkpoint/result state | Restart/recycle; resume only compatible checkpoint; never silent server upload | Actual RC/UI/checkpoint evidence missing | Implement/test crash recovery and user-safe messaging | `DESIGN_READY; EXERCISE_NOT_RUN` |
| Queue overload | Operations, Worker/API owners, Support, capacity owner | `RB-QUEUE-OVERLOAD-001`; `DRB-QUEUE-HIGH-001` | Depth/age/arrival/service/drain/retry/resource/provider trend | Narrow admission/concurrency, stop retry storm, isolate workload, observe drain | Threshold/capacity/authority not approved | Baseline limits; bounded load exercise; update runbook/alerts | `DESIGN_READY; EXERCISE_NOT_RUN` |
| Storage unavailable | Storage/Data owner, API/Worker, Security, Incident Owner | `RB-STORAGE-FAIL-001`; `DRB-STORAGE-FAIL-001` | Readiness/capacity, metadata/object/digest/version/state, safe refs | Stop false finalization and unsafe writes; protect source; reconcile or recover | Storage product, recovery authority, RPO/RTO pending | Controlled failure/restore test and reconciliation evidence | `DESIGN_READY; EXERCISE_NOT_RUN` |
| SSO outage | Identity liaison, Support, API/Security, Incident Owner | `RB-SSO-FAIL-001` | Safe auth/session/policy outcome, time, account/application state | Fail closed online/privileged actions; allow only valid local offline capability | SSO route, claims, session policy, contact pending | Confirm integration/runbook; test reconnect/revalidation | `DESIGN_READY; EXERCISE_NOT_RUN` |
| Malicious PDF | Security, Incident Owner, parser/Worker maintainer, Support | `RB-MALICIOUS-DOC-001` | Opaque file ref/digest if approved, engine/build/SBOM, crash/quarantine status | Quarantine/contain affected parser/Feature; preserve restricted evidence; no Production exploit test | Evidence/quarantine/forensic authority pending | Synthetic safe corpus test, patch/disable path, restricted handling procedure | `DESIGN_READY; EXERCISE_NOT_RUN` |
| Cross-user exposure | Security/Privacy, Incident Owner, Application/AuthZ, Support, Audit | `RB-CROSS-USER-001` | Safe AuthZ/Audit/Issue/Job refs; affected scope marked confirmed/suspected/unknown | Stop affected access/path, preserve evidence, revalidate ownership/permission; no content browsing | Security/privacy/legal notification and authority pending | Security process exercise; regression canary; scope/communication confirmation | `DESIGN_READY; EXERCISE_NOT_RUN` |
| Audit pipeline failure | Audit owner, Operations, Security, Application owner | `RB-AUDIT-FAIL-001` | Intent/outbox/backlog/schema/sequence/freshness and safe event refs | Preserve outbox; block mandatory-audit actions; idempotent replay after recovery | Degraded-mode policy/owner/threshold pending | Controlled backlog/replay exercise and audit health alert validation | `DESIGN_READY; EXERCISE_NOT_RUN` |
| AI provider failure | Content Intelligence, Operations, Security, Product, Support | `RB-AI-PROVIDER-001` | Approved profile/version, circuit/timeout, Feature scope, safe outcome | Open circuit/disable affected route; approved local/internal fallback only; no public AI | Provider contract/support/egress/retention/owner pending | PoC, provider failure exercise, fallback/exit and user limitation update | `DESIGN_READY; EXERCISE_NOT_RUN` |
| Failed deployment | Deployment Approver, Operations, Application, QA, Security, Incident Owner | `RB-BAD-RELEASE-001`; `DRB-API-FAIL-001`; `DRB-ROLLBACK-001` | Release/build/commit/artifact digest, config/schema, gate/smoke/health | Hold rollout; choose compatible rollback, flag, forward-fix, or restore | Exact infrastructure/change authority/window pending | QA rehearsal, rollback evidence, release checklist correction | `DESIGN_READY; EXERCISE_NOT_RUN` |
| Failed DB migration | Database/Data owner, Application, Deployment, Security, Incident Owner | `DRB-DB-MIG-FAIL-001`; `RB-DATA-INTEGRITY-001` | Migration ID/digest, schema/version, recovery point, exact state, logs/locks safe summary | Stop incompatible admission; prefer forward-fix after data change unless proven rollback/restore safe | DBA authority/topology/recovery point/RPO/RTO pending | Test prior/large/failure states; rehearse decision and restore validation | `DESIGN_READY; EXERCISE_NOT_RUN` |
| Restore from backup | Backup/DBA/storage, Application, Security/Audit, Operations, Incident Owner | `DRB-RESTORE-001`; `RB-DATA-INTEGRITY-001` | Backup manifest/digest/key readiness, SQL/object/version/tombstone/hold/AuthZ/Job/Audit reconciliation | Restore isolated; reopen only after full integrity, authorization, Core, and monitoring validation | Backup product/site/retention/key/RPO/RTO/authority pending | Controlled restore drill with measured evidence and unresolved-gap register | `DESIGN_READY; EXERCISE_NOT_RUN` |

## 7.3 Tabletop evidence template

```text
Scenario
Expected Roles
Runbook and version
Facts / Suspected / Unknown
Evidence references
Decision and authority
Containment / Recovery / Validation
Escalation and acknowledgement
Gap
Corrective Action and owner function
Status
```

No scenario may be closed because the participants merely read the runbook.

---

# 8. Maintenance schedule model

## 8.1 Scheduling principles

- Official frequency is `POLICY_CONFIGURABLE`; this report does not create a Pertamina schedule.
- Event-driven triggers—security advisory, failed health check, release, major Incident, capacity trend, certificate/key expiry window, provider change, schema change—apply even when the periodic date is not due.
- Maintenance is measured, bounded, reversible where possible, and environment-scoped.
- Cleanup/retention/index activity yields to user workload and must not become unbounded mass work.
- Each record includes owner function, backup/deputy, scope, environment, approval, precheck, result, evidence, exception, follow-up, and next review.

## 8.2 Routine maintenance catalog

| Routine | Purpose / evidence | Primary function | Event trigger | Frequency |
|---|---|---|---|---|
| Dependency review | Approved source/version/license/SBOM/vulnerability/compatibility/fallback inventory | Technical + Security + Architecture | New dependency/version/advisory/provider change | `POLICY_CONFIGURABLE` |
| Security patch | Risk, affected artifacts, clean build, tests/scans, rollout/rollback, monitoring | Security + Technical + Deployment | Approved advisory/vulnerability/Incident | `POLICY_CONFIGURABLE` |
| SDK/runtime update | `global.json`, supported .NET, compatibility, engine/client matrix, full gate | Technical/Architecture | Runtime support/security change | `POLICY_CONFIGURABLE` |
| Database migration review | Migration/version/digest, compatibility, locks/log/IO, recovery point, forward-fix/rollback | Database/Data + Application + Deployment | Schema/model/release change | `POLICY_CONFIGURABLE` |
| Index/query review | Query owner, execution plan, latency/IO, write cost, duplicate index, retention impact | Database/Data | Regression/growth/new query/slow plan | `POLICY_CONFIGURABLE` |
| Storage cleanup | Bounded candidates, active lease/reference/hold checks, bytes/count, retry/reconcile | Storage + Domain owner | Capacity trend/orphan/expiry | `POLICY_CONFIGURABLE` |
| Retention cleanup/archive | Approved policy, tombstone/hold, archive manifest/digest/schema, restoreability | Data/Audit/Support/Storage owners by plane | Policy or lifecycle trigger | `POLICY_CONFIGURABLE` |
| Log/telemetry review | Safe-field canaries, freshness, sampling, retention, alert quality, no content/secret | Operations + Security | Signal gap, new field/component, Incident | `POLICY_CONFIGURABLE` |
| Backup verification | Backup set/manifest/integrity/encryption/key readiness/monitoring | Backup + DBA/storage | Backup failure/change/release/migration | `POLICY_CONFIGURABLE` |
| Restore rehearsal | Isolated restore, SQL/object/AuthZ/Audit/Job/Core reconciliation and measured evidence | Backup/DBA/storage + Application + Security | Backup/topology/key/schema change | `POLICY_CONFIGURABLE` |
| Certificate/signing review | Approved certificate/publisher/signature, expiry, chain, artifact validation | Security + Deployment/Distribution | Renewal/signing/channel change | `POLICY_CONFIGURABLE` |
| Feature flag review | Owner, scope, reason, expiry/review, monitoring, cleanup/exit decision | Product + Administrator/Operations | Rollout/Incident/release | `POLICY_CONFIGURABLE` |
| Provider/license review | Approval, PoC, license, security/data/egress, version, capacity, support/fallback/exit | Architecture + Security + Product | Renewal/advisory/version/provider incident | `POLICY_CONFIGURABLE` |
| Compatibility review | Product/client/contract/capability/IPC/schema/provider supported combinations | Technical + QA + Deployment | Release/minimum-version change | `POLICY_CONFIGURABLE` |
| Documentation update | Manual/runbook/diagram/task/error/limitation/traceability impact and version | Documentation + affected owner | Any material behavior/interface/release change | `POLICY_CONFIGURABLE` |
| Runbook review | Owner/contact/access, safe checks, monitoring, stop/rollback, drill evidence | Operations/Support + affected owner | Incident/dependency/topology change | `POLICY_CONFIGURABLE` |
| Knowledge Base review | Accuracy, affected version, workaround safety, owner, stale/superseded state | Support + Technical/Documentation | Known Error/fix/release/Incident | `POLICY_CONFIGURABLE` |

## 8.3 Maintenance status

Recommended record status is `PLANNED`, `APPROVED`, `IN_PROGRESS`, `COMPLETED`, `FAILED`, `BLOCKED`, or `NOT_APPLICABLE_WITH_REASON`; final terminology requires Integrator/Pertamina approval. A failed check remains visible and creates follow-up. No routine above is claimed as scheduled or completed.

---

# 9. Known Error handover

## 9.1 Definition and separation

A Known Error is a verified failure pattern with a safe recognizable symptom and a validated workaround or resolution path. It is not the same as:

- a Known Limitation, which may be an accepted product/provider/platform boundary;
- a Defect, which is verified nonconformance in a build;
- an Incident, which is active coordinated impact;
- a Problem, which tracks underlying cause/pattern;
- an unverified support hypothesis.

## 9.2 Known Error record

| Field | Requirement |
|---|---|
| `KnownErrorId` | Stable governed record ID; not Feature/Requirement/Error Code |
| Searchable title/symptoms | User/support-safe wording and trigger conditions |
| Safe Error Code | Exact approved code(s), not raw exception |
| Affected versions/components/platforms | Exact range/build/provider/schema/capability where verified |
| Feature IDs | Canonical IDs affected |
| Cause/failure mechanism | Verified or confidence-labelled; restricted detail separated |
| Validated workaround | Version/scope/prerequisite/steps/expected outcome/stop condition |
| Forbidden actions | Direct SQL, bypass, mass retry, public provider, unsafe content request, or case-specific prohibition |
| Security/data risk | Classification, potential exposure/corruption, escalation trigger |
| Resolution | Fix/release/Problem/Defect/Incident/change references |
| Owner function | Accountable maintainer/support function plus deputy; names require confirmation |
| Review/expiry | `POLICY_CONFIGURABLE`; trigger on release/fix/Incident/provider change |
| Status | Proposed lifecycle pending approval; never mark validated without evidence |
| Evidence | Synthetic reproduction, tests, runbook, safe monitoring, approval |

## 9.3 Handover gate

The receiver must be able to search by symptom/Error Code/Feature/version, verify applicability, communicate the workaround safely, stop when the result differs, and link escalation. The Known Error index is not ready until actual verified records, owners, review state, and publication location exist.

## 9.4 Content safety

Do not include passwords, tokens, secrets, document/OCR/signature/clipboard/AI payload, raw SQL/stack/path/object key, unrestricted logs, malicious binaries, exploit instructions, or reporter-identifying data. Restricted root-cause evidence stays in the approved security/evidence store and is linked by opaque reference.

---

# 10. Knowledge Base handover

## 10.1 KB index and article template

```text
KB ID
Title / Symptoms
Safe Checks
Validated Workaround
Resolution / Related Runbook
Affected Version / Platform / Component / Feature ID
Expected and Forbidden Outcome
Escalation Trigger
Owner Function / Backup Function
Review Date or Trigger
Status
Related Known Error / Incident / Defect / Problem
Source Document and Version
```

## 10.2 Governance

1. Maintain one searchable KB Index with category, symptom, Feature, component, platform, version, owner, status, and last review.
2. User-facing articles contain only safe user actions; Support articles may add scoped diagnostics; restricted security procedures remain in a separately controlled repository.
3. Every workaround is tested on the affected version and includes stop/escalation conditions.
4. A fix or release triggers applicability review; stale articles become visibly superseded, not silently overwritten.
5. Search keywords avoid document content and personal data.
6. Article feedback creates a review item; it does not directly modify the authoritative procedure.
7. Recording, attachment, screenshot, and export follow classification/access/retention rules.

## 10.3 Prohibited KB content

The KB must never contain secret/credential/key material, user document or OCR content, signature payload, clipboard, sensitive prompt/provider payload, raw Production topology/endpoint where restricted, unrestricted logs/dumps, malicious samples, or actionable exploit detail.

## 10.4 Current readiness

Document 16 defines the KB structure, but no actual approved KB repository, owner, review date, article inventory, or publication workflow is evidenced. Status: `KNOWLEDGE_BASE_OPERATIONALIZATION_REQUIRED`.

---

# 11. Backlog handover

## 11.1 Backlog record template

```text
Item ID
Type
Description
Priority
Impact
Owner
Dependency
Target Phase
Acceptance
Status
```

This report uses `PROPOSED_BLOCKING`, `PROPOSED_HIGH`, and `PROPOSED_MEDIUM` as specialist triage suggestions only. The Integrator and authorized human owners confirm official priority, owner, phase, and acceptance.

## 11.2 Proposed initial backlog register

| Item ID | Type | Description | Priority | Impact | Proposed owner function | Dependency | Target phase | Acceptance | Status |
|---|---|---|---|---|---|---|---|---|---|
| `HOB-IMP-001` | Implementation Backlog | Create actual repository/projects/namespaces/paths/test projects from approved D04/D05/D18 boundaries | `PROPOSED_BLOCKING` | Codex tasks/build/tests cannot be scoped or validated | Technical/Application Owner | Architecture + Azure DevOps approval | Foundation | Exact paths/artifacts mapped; architecture checks pass | `OPEN` |
| `HOB-IMP-002` | Implementation Backlog | Implement/version approved missing Admin/monitoring/Activity/export retrieval contracts without inventing routes | `PROPOSED_HIGH` | Admin/Support/Audit operations remain partial | API + module owners | Controlled D06 decision/addendum | Foundation/Core | Approved contract, authz, OpenAPI, tests, manual/runbook links | `OPEN_DECISION_REQUIRED` |
| `HOB-IMP-003` | Implementation Backlog | Operationalize monitoring dashboards, alert routing, freshness, and safe field allowlists | `PROPOSED_BLOCKING` | No operations/support health visibility | Operations + Observability + Security | Actual environments/components | Pre-Pilot | All critical dependencies visible; stale/unknown handled; no sensitive fields | `OPEN` |
| `HOB-POC-001` | PoC Backlog | Primary PDF engine and existing-text edit fidelity/resource/security/license | `PROPOSED_HIGH` | Core Edit PDF/related behavior cannot be claimed | Pdf/Architecture/Security | Approved candidate provider | Before full gated coding | Approved result or limitation/alternative with evidence | `POC_REQUIRED` |
| `HOB-POC-002` | PoC Backlog | Office↔PDF, PDF→Office, HTML→PDF, PDF/A fidelity and active-content isolation | `PROPOSED_HIGH` | Multiple Core conversion cards blocked/unproven | Conversion/Architecture/Security | Approved engines/corpus | Early Core | Quality, formats, security, license, resources, fallback pass | `POC_REQUIRED` |
| `HOB-POC-003` | PoC Backlog | OCR complex table, physical scanner, large-file/resource behavior | `PROPOSED_HIGH` | Scan/OCR platform and fidelity limitation unresolved | ScannerOcr/Platform/Security | Device/engine access | Early Core | Platform, quality, security, capacity, fallback evidence | `POC_REQUIRED` |
| `HOB-POC-004` | PoC Backlog | AI Summarizer, Translate, complex Markdown, optional local/internal model/provider | `PROPOSED_HIGH` | Intelligence accuracy/data/egress/provider risk unresolved | ContentIntelligence/Conversion/Security/Product | Approved local/internal provider | Early Core | Grounding/fidelity, privacy, retention, capacity, failure/exit evidence | `POC_REQUIRED` |
| `HOB-DEF-001` | Defect Backlog | Establish import/reconciliation from actual Unit/QA/UAT/Incident results; no defect is fabricated now | `PROPOSED_BLOCKING` | Defect ownership/retest/release evidence unavailable | QA + Technical + Product | Actual implementation/test execution | From first implementation | Verified defects linked to build/Feature/test; rejected hypotheses separated | `NO_VERIFIED_DEFECT_INPUT` |
| `HOB-SEC-001` | Security Backlog | Confirm Pertamina SSO protocol/claims/provisioning/revocation/reauth/offline/workload identity | `PROPOSED_BLOCKING` | Authentication/privileged operation cannot be Production-approved | Security + IAM/Application | Pertamina Identity | Foundation | Approved integration matrix and negative/reconnect tests | `CONFIRMATION_REQUIRED` |
| `HOB-SEC-002` | Security Backlog | Confirm data classification, crypto/key/secret/local profile/storage/provider handling and retention | `PROPOSED_BLOCKING` | Restricted data protection and recovery incomplete | Security/Data/Infrastructure | Pertamina standards/products | Foundation/Pre-Pilot | Approved control mapping, implementation, tests, runbooks | `CONFIRMATION_REQUIRED` |
| `HOB-SEC-003` | Security Backlog | Define scan products/policies, SBOM/provenance/signing, VA/PT and vulnerability/waiver process | `PROPOSED_HIGH` | Release/security gate cannot be evidenced | Security + DevOps/Deployment | Pertamina security governance | Before RC | Pipeline and independent test evidence; waiver governance approved | `CONFIRMATION_REQUIRED` |
| `HOB-TD-001` | Technical Debt | Resolve ContentIntelligence exact repository placement and D07 data-owner anchors for Jobs/Secured/History/Configuration | `PROPOSED_HIGH` | Ambiguous owner/path can cause cross-module changes | Architecture/Technical/Data | D04/D07/D18 controlled decision | Before module coding | Approved one-owner mapping and architecture tests | `OPEN_DECISION_REQUIRED` |
| `HOB-TD-002` | Technical Debt | Complete IPC/internal event/provider/storage contract versions, consumer registry, retry/idempotency, owner | `PROPOSED_HIGH` | Failure/circular dependency/version risk | Architecture/API/Module owners | PoC + implementation design | Foundation/Core | Typed/versioned catalogs and contract tests | `OPEN` |
| `HOB-DOC-001` | Documentation Gap | Replace `TBD_UI_LABEL`, add approved RC screenshots, platform availability, error catalog, contact route | `PROPOSED_HIGH` | User/Admin manuals remain not `RELEASE_READY` | Documentation/Product/QA/Security | Exact RC/UI/UAT | RC | Task-tested manuals, approved safe screenshots, links/contact verified | `WAITING_FOR_RC` |
| `HOB-DOC-002` | Documentation Gap | Operationalize D18 derived catalogs/registers/diagrams, owners, versioning, stale/orphan validation | `PROPOSED_MEDIUM` | Cartography can drift from implementation/release | Cartography/Application/Documentation | Actual repository/artifacts/owners | Foundation→RC | Sources/renders/catalogs current; owner and change triggers active | `PARTIAL` |
| `HOB-OPS-001` | Operations Gap | Confirm Support/Service Desk/L1-L3/Incident/Operations owners, contacts, delegation, on-call/escalation | `PROPOSED_BLOCKING` | No acknowledged support or incident path | Organization/Support/Operations | Pertamina organization decision | Pre-Pilot | Access/contact/escalation tested; ownership accepted | `CONFIRMATION_REQUIRED` |
| `HOB-OPS-002` | Operations Gap | Execute sample Issue, 12 incident table tops, deployment rehearsal, restore drill, and side-by-side evidence | `PROPOSED_BLOCKING` | Runbooks remain design-only | Support/Operations/QA/Deployment/Backup | RC, environments, access, owners | Pre-Pilot/Pre-Production | Exercises pass or corrective actions owned; no false status | `NOT_RUN` |
| `HOB-OPS-003` | Operations Gap | Establish Known Error and KB repositories, workflow, owners, review triggers, safe publication | `PROPOSED_HIGH` | Repeated cases depend on individuals | Support/Technical/Documentation | Service Desk/records approval | Pre-Pilot | Searchable index, validated articles, access/retention/review evidence | `OPEN` |
| `HOB-CONF-001` | Pertamina Confirmation Gap | Approve Production infrastructure/network/DNS/TLS/zones/capacity/monitoring/authority | `PROPOSED_BLOCKING` | Production topology/readiness cannot be claimed | Infrastructure/Network/Security/Operations | Pertamina approval | Pre-Production | Approved topology, access, monitoring, capacity and runbooks | `CONFIRMATION_REQUIRED` |
| `HOB-CONF-002` | Pertamina Confirmation Gap | Approve backup/DR product/topology/key recovery/retention/RPO/RTO/failover/drill cadence | `PROPOSED_BLOCKING` | Recovery objective and Production reopening unresolved | Backup/DR/Data/Security | Infrastructure and policy | Pre-Production | Approved plan plus successful measured restore/DR drill | `CONFIRMATION_REQUIRED` |
| `HOB-CONF-003` | Pertamina Confirmation Gap | Confirm Application/CMDB IDs, owners, criticality, classification, Support route, distribution/signing/provider contracts | `PROPOSED_BLOCKING` | Ownership, governance, publication, support, release incomplete | Organization/Application/Security/Deployment | Pertamina decisions | Before accepted handover | Registers populated and responsibilities formally accepted | `CONFIRMATION_REQUIRED` |

## 11.3 Backlog governance

- Every active item has one accountable human function, not “team” or Codex.
- Owner acceptance and priority are explicit; blank owner is a visible blocker.
- `DONE` requires stated acceptance evidence; “documented” is not enough for implementation/operations items.
- Security/Production/confirmation gaps cannot be downgraded merely to close handover.
- Deferred items retain impact, compensating control, authority, expiry/review, and target phase.

---

# 12. Technical debt/limitations

## 12.1 Classification rule

| Class | Meaning |
|---|---|
| Known limitation | An explicit capability/platform/provider/fidelity boundary users/operators must understand |
| Technical debt | A consciously deferred implementation/design quality obligation with owner and remediation |
| Confirmation gap | A Pertamina fact/authority/policy not yet decided and not inferable |
| PoC gap | Capability/provider quality/security/resource evidence required before full implementation |
| Defect | Verified deviation on an actual build; none is invented from pre-implementation documents |

## 12.2 Limitation and risk handover

| Area | Current limitation/risk | Operational/user consequence | Required treatment |
|---|---|---|---|
| Platform | Desktop is primary; Web/Phone/Tablet are capability-aware curated surfaces, not parity | Feature may be hidden/limited/unavailable by platform/build/device | Release-specific capability matrix, clear unavailable reason, platform tests |
| Provider | PDF/OCR/conversion/AI/provider portfolio is not selected/approved | Gated Features cannot be promised or routed arbitrarily | PoC, license/security/data/resource/support/fallback/exit approval |
| Conversion fidelity | Office/PDF/HTML/PDF-A/edit transformations may change layout/font/table/structure | Human review and limitations are required; no false conformance | Representative corpus, oracle, visible limitation, block unsafe output |
| OCR | Scan quality, language, layout, table, handwriting, page volume affect accuracy/resources | Searchable text/data may be wrong; review required | Quality thresholds, page/resource limits, provenance, approved fallback |
| AI accuracy | Summary/translation can omit or hallucinate facts | Output is untrusted assistance, not authoritative replacement | Grounding/provenance, selected context, user review, no direct action |
| PDF edit | Existing-text editing and advanced forms are PoC-gated | Only released subset may be available; layout preservation not guaranteed | Exact subset/version documentation and approved limitation |
| Offline | Only eligible local Features operate under valid bounded offline grant | Server/account/privileged/provider actions wait/deny; revocation is not immediate offline | Absolute expiry, protected projection, reconnect authorization revalidation |
| Server dependency | Optional/required server workloads depend on API/queue/Worker/SQL/storage/provider capacity | Waiting/degraded behavior without removing local capability | Admission/backpressure, bounded retries, no silent upload/server fallback |
| Version compatibility | Actual supported client/contract/schema/IPC/provider window is not available | Old/new clients may require update or fail safely | Release Manifest, compatibility tests, mixed-version recovery, user guidance |
| Feature availability | All 33 manuals are pre-implementation; no RC exposure evidence | No feature may be advertised as released merely because catalog/design is FINAL | Actual implementation, PoC, test/UAT, exposure, artifact, owner, runbook evidence |
| Security restrictions | No public AI, direct SQL, arbitrary shell/filesystem/HTTP, implicit content access, or PKI claim | Some convenient shortcuts are intentionally unavailable | Preserve restriction; explain safe approved path rather than bypass |
| Local Worker/IPC | Exact project, contract implementation, identity/ACL/resource behavior require PoC | Heavy/risky local features cannot be declared ready | Signed cohort, authenticated/versioned Named Pipe, crash/resource tests |
| Data/retention | Official classification, retention/hold/disposal and numeric limits are not approved | Cleanup/export/backup behavior cannot use guessed values | Typed policy, data-owner approval, bounded lifecycle tests |
| Production | Topology, network, SSO, secret/storage products, signing/distribution, capacity, DR, owner are pending | `PRODUCTION_READY` and `HANDOVER_READY` cannot be used | Complete confirmation gates, actual exercises, acceptance evidence |
| Cartography | Actual components/paths/artifacts/owners/per-Feature overlays are partial | Codex scope and operational ownership may be ambiguous | Update D18 registers from actual implementation; orphan/stale checks |

## 12.3 Debt acceptance

Accepted technical debt must record description, affected Feature/component/version/environment, impact, compensating control, owner, authority, remediation, target/expiry, review trigger, tests/monitoring, and closure evidence. A hidden limitation or ownerless debt is not accepted debt.

---

# 13. Codex workflow handover

## 13.1 Operating model

```text
Human Owner
  └─ owns intent, priority, business/security/Production decisions, and final approval
       ↓
Agent 0 — Integrator
  └─ reconciles sources, assigns scopes, resolves conflicts, integrates specialist outputs
       ↓
Four Specialist Agents
  └─ analyze bounded domains and write only allowed outputs; no master edit unless authorized
       ↓
Human review / merge / release gate
```

Agents are not application actors, owners, approvers, security authorities, Incident Owners, release executors, or evidence signatories.

## 13.2 Required task contract

```text
Task ID
Requirement ID(s)
Feature ID(s)
Objective
Allowed Paths
Do-Not-Touch Paths
Dependencies and consumed/provided contract versions
Acceptance Criteria
Tests
Security and privacy boundaries
Performance / server resource budget
Processing Mode and Server Dependency
Data stores/classification/retention impact
Runbook/manual/deployment references
Expected Output and evidence location
```

If actual repository paths, module/component owner, contract, or test project is unknown, the task remains blocked for scope resolution. Codex must not substitute conceptual paths as authorization.

## 13.3 Task workflow

```text
Human-approved objective
→ Integrator resolves authoritative IDs and dependency scope
→ dedicated short-lived branch/worktree
→ specialist/implementation task within exact paths
→ smallest change; no unrelated refactor
→ build/tests/scans/evidence
→ specialist output and gaps
→ Integrator conflict/traceability review
→ human PR/merge/acceptance
→ approved pipeline/release process
```

Shared contracts, architecture, dependencies, schemas, security controls, and business rules require an explicit foundation/change task. A failing task is corrected within its scope or escalated; another module is not edited silently to make it green.

## 13.4 Eight-layer Definition of Done

No task/Feature is `DONE` unless evidence covers:

1. Technical Correctness;
2. Business Process Correctness;
3. UI/UX Acceptance;
4. Security & Privacy;
5. Performance / Server Resource Budget;
6. Error & Recovery;
7. Cross-Platform Behavior;
8. Test & Documentation.

Build success or Unit test alone is insufficient. Traceability must resolve `Requirement → Feature → Code → Test → UAT → Release`.

## 13.5 Codex teach-back exercise

The receiver drafts a task for a selected canonical Feature and demonstrates:

- exact Feature/Requirement resolution;
- one owning module and actual allowed paths;
- explicit do-not-touch paths/contracts/data;
- local/server mode and resource impact;
- authorization, sensitive-data exclusion, Audit, and recovery expectations;
- applicable Unit/Integration/Security/Platform/UAT evidence;
- no master/source-of-truth edit outside authorization;
- no invented endpoint, permission, role, EventType, table, provider, limit, or Production detail.

Pass requires Integrator review. Until an actual repository exists, this exercise can validate the task specification only, not build/test execution.

---

# 14. Side-by-side maintenance

## 14.1 Duration and coverage

Calendar duration is `TARGET_TO_BE_APPROVED`. Document 01 requires side-by-side maintenance to cover at least one business-process cycle; this is a coverage requirement, not a fabricated number of days or weeks.

## 14.2 Transition stages

| Stage | Provider-side activity | Receiver-side activity | Evidence / exit condition |
|---|---|---|---|
| Readiness | Confirm materials, access, owners, environments, runbooks, Known Error/KB, backlog | Confirm prerequisites and role boundaries | No blocking access/source ambiguity for planned scope |
| Observe | Demonstrate normal operations, Issue, monitoring, maintenance, release/incident decision | Observe using checklist and ask bounded questions | Observation record and open questions owned |
| Assist | Support receiver while receiver executes sample operation/Issue/runbook/release observation | Execute with guidance and preserve evidence | Safe completion; mistakes/remediation recorded |
| Receiver leads | Provide backup/clarification only; do not take silent ownership back | Lead operational handoff, troubleshooting, documentation correction | Teach-back passes and owner handoff acknowledged |
| Acceptance/exit | Close documentation corrections and transfer known risks/backlog | Accept ownership or explicitly reject/block by scope | Exit criteria met and residual items accepted by authority |

## 14.3 Required activities

- issue clarification and sample Issue handling;
- Incident assistance and at least the approved tabletop/drill scope;
- release observation or QA deployment rehearsal;
- KT reinforcement/repeat sessions;
- documentation/KB/Known Error correction;
- backlog/technical debt/limitation review;
- ownership transition with primary/deputy coverage;
- review of monitoring, backup verification, restore/rollback decision, and security boundaries.

## 14.4 Exit criteria

Side-by-side is complete only when:

- the receiving functions accept ownership and access works;
- required KT sessions are `VALIDATED` and acceptance scope is explicit;
- a sample Issue and applicable Incident/recovery exercise succeed;
- monitoring and escalation can be used independently;
- one approved release/QA rehearsal and rollback decision are understood;
- backup/restore evidence is reviewed by the correct function;
- Known Error/KB/backlog/limitations have owners and publication path;
- critical documentation corrections are completed;
- no unresolved critical security/content-access misunderstanding remains;
- residual gaps, duration end, and exit approval are recorded.

Elapsed time, presenter availability, or the end of a support window does not automatically satisfy exit.

---

# 15. Support readiness gate

## 15.1 Gate assessment

| Gate | Baseline available | Operational evidence required | Current assessment |
|---|---|---|---|
| Support access available | Permission model exists | Personal account, `SUPPORT_SCOPE`, deny tests, environment access | `NOT_EVIDENCED` |
| Runbook available | D16 and registers exist | Exact RC/monitoring/access/owner plus controlled exercise | `BASELINE_AVAILABLE; NOT_OPERATIONALLY_VALIDATED` |
| Issue flow understood | User/Admin/Incident design exists | Recipient handles synthetic Issue end-to-end | `NOT_ASSESSED` |
| Safe evidence understood | Allowlist/exclusions defined | Teach-back with Job/Correlation/Error refs and privacy denial cases | `NOT_ASSESSED` |
| Monitoring readable | Health model exists | Approved dashboard/access/freshness/alert exercise | `NOT_EVIDENCED` |
| Escalation known | Function types/templates exist | Official route/contact/authority and acknowledged handoff test | `REQUIRES_PERTAMINA_SERVICE_DESK_CONFIRMATION` |
| Sample Issue handled | Test/UAT design exists | Successful case with triage/status/note/communication/closure evidence | `NOT_RUN` |
| Privacy boundary understood | D08/D09/D15/D16 controls exist | Deny password/token/content/OCR/signature/clipboard/public-AI requests | `NOT_ASSESSED` |
| Known Error available | Record structure defined | Actual validated searchable Known Error index | `NOT_EVIDENCED` |
| Contact/owner confirmed | Functions are listed | Named approved primary/deputy/service routes | `REQUIRES_PERTAMINA_OWNER_CONFIRMATION` |

## 15.2 Overall decision

```text
Support readiness: NOT_READY_TO_DECLARE
Reason: access, owner/contact, monitoring, Known Error/KB, sample handling,
teach-back, and operational drill evidence are not yet available.
```

This decision does not invalidate the FINAL design baselines; it prevents a false operational-readiness claim.

---

# 16. Pertamina confirmation gaps

| Area | Required confirmation | Marker / impact |
|---|---|---|
| Service Desk | Official intake channel, queue, contact, handoff, hours/cadence, integration, authority | `REQUIRES_PERTAMINA_SERVICE_DESK_CONFIRMATION`; Support gate blocked |
| Organization/ownership | Business/Product/Application/Technical/Module/Data/Security/Operations/Support L1-L3/DB/Deployment/Audit/Documentation owners and deputies | `REQUIRES_PERTAMINA_OWNER_CONFIRMATION`; acceptance blocked |
| Responsibility/SoD | RACI-like authority, delegation, approval, escalation, no self-approval | `REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION` |
| KT administration | Schedule, presenters, recipients, attendance, recording, repository, retention, validation authority | Pertamina organization/training/privacy confirmation |
| Side-by-side | Calendar duration, start/end, coverage, availability, exit authority | `TARGET_TO_BE_APPROVED` |
| Support/Incident standards | Severity, priority, status transition/reopen, response targets, communication cadence | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`; no SLA invented |
| SSO/identity | Protocol, issuer/audience/claims, provisioning, revocation, reauth, offline grant, workload identity, help route | `REQUIRES_PERTAMINA_SSO_CONFIRMATION` |
| Security/privacy/legal | Classification, evidence, forensics, notification, legal hold, exploit/sample handling, security channel | `REQUIRES_PERTAMINA_SECURITY_PROCESS_CONFIRMATION` |
| Maintenance | Cadence, maintenance windows, approvals, status terminology, review/expiry | `POLICY_CONFIGURABLE` |
| Retention/records | KT/KB/Known Error/Issue/Incident/Audit/log/export/backup retention and disposal | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Production infrastructure/network | Hosting/topology/OS/container/proxy/DNS/TLS/firewall/ports/zones/capacity/monitoring authority | D17 gates; `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` and `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION` |
| Backup/DR | Product/site/replication/key recovery/retention/RPO/RTO/failover/failback/drill | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| Distribution/signing | Desktop/Mobile channels, publisher/team IDs, signing, update rings, managed wipe | D17 distribution confirmations |
| Provider/license | Approved engine/internal AI/provider, support path, data/egress/retention, fallback/exit | Architecture/Security/Product confirmation |
| Repository/pipeline | Azure DevOps project/repo/pipeline/service connections/permissions/artifact publication | Implementation and DevOps authority confirmation |
| Cartography/app identity | Application ID, CMDB ID, official owners, classification, criticality, template/publish location | D18 confirmation markers |
| RC/manual/UAT | Exact release/build/platform exposure/UI labels/screenshots/error catalog/UAT sign-off | Release/QA/Product/Documentation confirmation |

An open confirmation gap is never permission to assume a default.

---

# 17. Risks/conflicts

| ID | Risk/conflict | Impact | Required disposition |
|---|---|---|---|
| `D19-C-001` | KT is treated as a presentation without hands-on/teach-back | False competence and fragile operations | Enforce evidence hierarchy and `REPEAT_REQUIRED` |
| `D19-C-002` | Support responsibility is assigned before accounts/permissions/environment access work | Incidents cannot be handled or unsafe workaround emerges | Access matrix and deny tests before acceptance |
| `D19-C-003` | Runbook existence/design review is presented as operational validation | Recovery fails under real conditions | Execute controlled tabletop/drill on exact environment/RC |
| `D19-C-004` | Escalation owner/contact/acknowledgement is unclear | Unowned impact and delayed decisions | `REQUIRES_PERTAMINA_SERVICE_DESK_CONFIRMATION` and owner acceptance |
| `D19-C-005` | No governed Known Error index | Repeated diagnosis and inconsistent workaround | Create verified safe registry with owner/review/version |
| `D19-C-006` | Backlog has no owner, priority, target phase, or acceptance | Gaps disappear after handover | Refuse `DONE/ACCEPTED`; keep confirmation required visible |
| `D19-C-007` | Platform/provider/fidelity/security/Production limitations are hidden | False user/release promise and unsafe output | Publish version-scoped limitations and user-safe guidance |
| `D19-C-008` | Maintenance and knowledge depend on one person | Single-person operational failure | Primary/deputy, teach-back, side-by-side, KB/runbook ownership |
| `D19-C-009` | Codex workflow is misunderstood as authority to expand scope/edit master/merge/release | Source-of-truth drift and uncontrolled change | Exact task contract, Agent 0 integration, human review/approval |
| `D19-C-010` | Security boundary is taught incorrectly | Permission/content/provider/AI/Worker bypass | Mandatory deny-case teach-back and Security validation |
| `D19-C-011` | Support asks for full user document/OCR/signature/password/token | Privacy/security breach | Safe diagnostics/explicit attachment only; separate controlled content workflow if ever approved |
| `D19-C-012` | Missing Production facts are filled with Hostinger or guessed topology/contact/SLA | Governance and security misrepresentation | Keep approved placeholders/markers; no Production claim |
| `D19-C-013` | Side-by-side is considered complete because target duration elapsed | Receiver cannot operate independently | Evidence-based exit criteria and explicit acceptance |
| `D19-C-014` | D01 says at least one business-process cycle while calendar duration is unapproved | Apparent schedule conflict | Treat one cycle as coverage minimum; duration stays `TARGET_TO_BE_APPROVED` |
| `D19-C-015` | 550/33 catalog completeness is confused with implementation/release readiness | False `DONE/PRODUCTION/HANDOVER_READY` | Preserve D18 lifecycle/evidence boundaries |
| `D19-C-016` | D14–D18 FINAL baselines are confused with actual RC/UAT/drill/Production evidence | Manuals/runbooks/cartography become misleading | Label pre-implementation; replace placeholders only from exact RC evidence |
| `D19-C-017` | L1/L2/L3 or Incident/Operations functions are treated as new application roles | Authorization expansion | Formal roles remain User/Admin/Auditor; Support remains scoped profile |
| `D19-C-018` | KB/Known Error/KT artifacts include restricted content or exploit detail | Secondary data leakage | Field allowlists, approved repositories, classification/access/retention review |
| `D19-C-019` | A workaround disables Audit, validation, rate limit, quarantine, Local-First, or public-AI block | Safety degradation during Incident | Narrow authorized containment; security controls remain non-bypassable |
| `D19-C-020` | Operations treats Job/Correlation/object ID or dashboard visibility as authorization | Cross-user/resource disclosure | Reauthorize permission/ownership/scope/state on every resource action |

No material contradiction was found between the requested handover model and Documents 01–18. The main tensions are readiness semantics, unconfirmed organization/Production facts, and the distinction between design evidence and operational evidence.

---

# 18. Suggested Document 19 text

The following text is proposed for Integrator review and controlled inclusion in master Document 19.

## Handover status and purpose

> **Status:** `FINAL — PRE-IMPLEMENTATION HANDOVER BASELINE` only after Integrator approval.  
> **Operational handover status:** `NOT_READY_TO_DECLARE` until access, named owners, KT validation, actual artifact/environment, support/monitoring, runbook drills, Known Error/KB, and side-by-side evidence exist.  
> **Product baseline:** 550 canonical Features and 33 Primary PDF Tool Cards.  

Document 19 governs transfer of product, technical, operational, support, security, testing, deployment, recovery, cartography, backlog, limitation, and Codex knowledge. A FINAL document is not proof that the receiving functions can operate the application.

## Knowledge Transfer

KT uses `PLANNED`, `SCHEDULED`, `DELIVERED`, `VALIDATED`, `REPEAT_REQUIRED`, and `ACCEPTED`. Sixteen minimum sessions cover product/business; requirements/550 Features; 33 Core Tool Cards; architecture/module boundaries; repository/branch/Codex; API/integration; database/SQLite/storage; security/role/permission; Audit/observability; Test/UAT; User/Admin operation; Incident/Support; Deployment/rollback; Backup/restore/recovery; Application Cartography; and PoC/limitations/known risks.

Each session records objective, audience, presenter function, prerequisites, material versions, demonstration, hands-on exercise, expected competence, Q&A, recording/reference where permitted, attendance, validation result, open questions, follow-up, and status. No actual date, participant, presenter, or result is populated without evidence.

KT is validated only after the recipient independently demonstrates product explanation, source/Requirement/Feature lookup, module explanation, build/test execution when the repository exists, API/schema/monitoring reading, safe Correlation ID use, sample Issue handling, Incident runbook execution, QA deployment rehearsal, rollback and backup/restore reasoning, security-boundary decisions, and creation of a scoped Codex task.

## Support and operations handover

The confirmed product intake path is In-App Issue Reporting. Official Service Desk route, contacts, queues, ownership, and integration require `REQUIRES_PERTAMINA_SERVICE_DESK_CONFIRMATION`. Support/Superadmin is a permission-scoped profile, not a role with unlimited access. It can use authorized Issue list/detail, explicit attachment, triage/status, internal note, and export actions. It cannot automatically access user documents, OCR content, signatures, clipboard, passwords, tokens, secrets, unrelated Audit, or raw diagnostics.

Operations handover covers API, Local/Server Worker, Queue/Job Store, SQL Server, Secure Storage, SignalR, Audit pipeline, Issue service, SSO, approved providers, compatibility, typed configuration/feature flags, capacity/backpressure, and secured-file operations. For each dependency, the receiver must know its source of truth, liveness/readiness/degraded signals, safe first action, forbidden action, correlation method, runbook, owner, and evidence.

## Incident readiness

Controlled handover exercises cover API outage, Local Worker crash, queue overload, storage unavailable, SSO outage, malicious PDF, cross-user exposure, Audit pipeline failure, AI provider failure, failed deployment, failed DB migration, and restore from backup. Each exercise records Scenario, Expected Roles, Runbook, Evidence, Decision, Escalation, Gap, Corrective Action, and Status.

Design review is not operational validation. Until actual exercises run on approved environments with authorized functions and safe evidence, status remains `TABLETOP_DESIGN_READY; EXERCISE_NOT_RUN`.

## Maintenance, Known Error, and Knowledge Base

Routine maintenance covers dependency review, security patch, SDK update, database migration, index/query review, storage/retention cleanup, log/telemetry, backup verification, restore rehearsal, certificate/signing, feature flags, provider/license, compatibility, documentation, runbooks, and KB. Official frequency is `POLICY_CONFIGURABLE`; event triggers remain mandatory.

Known Error records contain safe symptoms/Error Code, affected versions/components/Features, verified cause/confidence, validated workaround, prerequisites, forbidden actions, security/data risk, fix references, owner, review/expiry, status, and evidence. Known Error is separate from limitation, Defect, Problem, and Incident.

KB articles contain ID, symptom, safe checks, approved workaround, resolution/runbook, affected version, owner, review trigger, status, escalation, and related records. They never contain credentials, user documents/OCR/signatures/clipboard/AI payload, raw Production diagnostics, malicious samples, or exploit detail.

## Backlog, debt, and limitations

The handover has separate Implementation, PoC, Defect, Security, Technical Debt, Documentation Gap, Operations Gap, and Pertamina Confirmation Gap registers. Every record has Item ID, Type, Description, Priority, Impact, Owner, Dependency, Target Phase, Acceptance, and Status. A record without accepted owner or evidence cannot be closed.

Receivers are explicitly informed of platform/provider/conversion/OCR/AI/PDF-edit/offline/server/version/availability/security/Production limitations. Catalog or document completeness never implies implementation, release, or Production readiness. Accepted technical debt records compensating control, authority, owner, expiry/review, remediation, tests/monitoring, and closure evidence.

## Codex workflow

The operating model is Human Owner + Agent 0 Integrator + four Specialist Agents. The Human Owner retains intent, priority, business/security/Production decisions, and final approval. Agent 0 reconciles authoritative sources and integrates bounded specialist outputs. Specialist Agents work only within allowed paths and do not edit master/source-of-truth files, merge, deploy, or approve unless explicitly authorized.

Every Codex task specifies Task ID, Requirement IDs, Feature IDs, Objective, Allowed Paths, Do-Not-Touch Paths, Dependencies/contracts, Acceptance Criteria, Tests, Security, Performance/server budget, processing/data impact, runbook/manual references, and Output. Missing actual repository path, owner, contract, or data boundary blocks scope expansion. Definition of Done remains eight-layer: Technical, Business, UI/UX, Security/Privacy, Performance/Server Budget, Error/Recovery, Cross-Platform, and Test/Documentation.

## Side-by-side and readiness gate

Side-by-side calendar duration is `TARGET_TO_BE_APPROVED` and covers at least one business-process cycle. Stages are readiness, observe, assist, receiver-led operation, and acceptance/exit. It includes Issue clarification, Incident assistance, release observation, KT reinforcement, documentation correction, backlog/limitation review, ownership transfer, and evidence-based exit.

Support readiness requires approved access, operationally validated runbooks, understood Issue and safe-evidence flows, readable monitoring, confirmed escalation, successful sample Issue, demonstrated privacy boundary, available Known Error/KB, and confirmed owner/contact. Missing evidence yields `NOT_READY_TO_DECLARE`, not an assumed PASS.

## Confirmation and acceptance boundary

Pertamina must confirm Service Desk, organization/owners/deputies/SoD, KT administration/recording, side-by-side duration, Support/Incident standards, SSO, Security/Privacy/Legal, maintenance/retention, Production infrastructure/network/capacity, backup/DR, distribution/signing, provider/license/support, repository/pipeline, Application/CMDB/classification/criticality, and exact RC/manual/UAT facts.

Handover acceptance requires:

```text
[ ] required KT sessions DELIVERED and teach-back VALIDATED
[ ] receiving functions and deputies formally accept scope
[ ] required accounts/permissions/environments/repositories work
[ ] monitoring, Issue, escalation, and safe evidence are usable
[ ] 12 tabletop/recovery scenarios have governed evidence and actions
[ ] QA deployment/rollback and backup/restore rehearsals meet approved gates
[ ] Known Error and KB indexes are approved and searchable
[ ] backlog, technical debt, limitations, and confirmation gaps have owners
[ ] side-by-side exit criteria are met
[ ] no password/token/secret/document/OCR/signature/clipboard/AI payload leakage
[ ] no false IMPLEMENTED, TESTED, PILOT, PRODUCTION, or HANDOVER_READY claim
```

Until every mandatory item is evidenced or formally accepted with governed residual risk, ownership remains in transition and handover is not complete.

---

**Integrator handoff:** use this report as the Agent C source for Document 19. Preserve the distinction between FINAL documentation and operational readiness; do not populate dates, people, contacts, SLA, policy values, Product/Production facts, or acceptance without authoritative evidence.
