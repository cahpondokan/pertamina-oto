# AGENT D — IMPLEMENTATION READINESS, POC, BACKLOG & REPOSITORY PLAN

> **Task ID:** `BASELINE-AUDIT-AGENT-D`  
> **Tanggal audit:** 2 September 2026  
> **Baseline berwenang:** Document 01–19 FINAL dan `MASTER_PROJECT_SUMMARY.md` v4.2  
> **Canonical baseline:** 550 Feature ID; 33 Primary PDF Tool Cards  
> **First vertical slice:** `FEAT-PDF-005 — Merge PDF`  
> **Jenis artefak:** implementation-readiness plan; bukan source code, vendor decision, capacity baseline, release approval, atau Production plan approval  
> **Perubahan aplikasi:** tidak ada

## Keputusan ringkas

```text
Planning readiness                    : READY
Formal application coding right now   : BLOCKED
Phase 0 Repository Bootstrap           : READY_WITH_CONDITIONS
Phase 1 Shared Foundation              : NOT_READY until Phase 0 exits
Merge PDF provider-complete slice      : NOT_READY until PDF Engine PoC decision
QA/UAT/Pilot/Production                : BLOCKED / later evidence gates
```

Proyek **siap secara desain**, tetapi belum siap memulai coding formal di workspace saat ini. Tidak ada Git repository, solution/project .NET, `global.json`, central package file, pipeline, package approval, source/build owner, atau executable test. Coding formal boleh dimulai setelah repository/access/owner/toolchain minimum pada §14 dipenuhi. Production topology, Service Desk, CMDB, RPO/RTO, dan mobile distribution tidak memblokir Phase 0–1.

Urutan pertama yang direkomendasikan:

1. konfirmasi official repository, access, human integrator/owner, package source, dan exact .NET 10 SDK patch;
2. freeze Document 01–19 v4.2 sebagai canonical implementation input;
3. buat bootstrap root + minimal solution/projects + satu test discovery/smoke;
4. hidupkan PR pipeline cepat;
5. jalankan PDF Engine PoC secara paralel dengan Shared Foundation;
6. mulai Merge PDF vertical slice melalui contract/fake boundary; integrasikan provider hanya setelah PoC/license/security decision.

---

# 1. Readiness assessment

## 1.1 Overall readiness

| Decision scope | Status | Evidence/reason | Allowed next action |
|---|---|---|---|
| Documentation planning | `READY` | D01–D19 FINAL; Master v4.2; GATE-0 PASS in D19 | Freeze exact versions/hashes as implementation inputs |
| Formal repository bootstrap | `READY_WITH_CONDITIONS` | Architecture/repo model and test/pipeline rules locked; official repo/access/owner/SDK/feed absent | Execute Phase 0 only after entry checklist §14.1 |
| Product feature coding | `NOT_READY` | No repository/build/foundation; provider unresolved | Prepare scoped tasks; do not write feature code yet |
| Merge domain/contracts/UI against fake engine | `READY_WITH_CONDITIONS` | Merge behavior/test/UAT is unusually complete; requires Phase 0–1 | Start only after M2 and approved contracts task |
| Merge real PDF provider integration | `BLOCKED` | PDF engine/license/security/fidelity PoC not approved | Run `POC-PDF-001`; preserve provider abstraction |
| QA/UAT candidate | `BLOCKED` | No artifacts, executable tests, environment or actual results | Complete phases 0–8 and QA entry gates |
| Pilot/Production | `BLOCKED` | D17/D19 confirmation/evidence gates open | Do not design around assumed Production topology |

## 1.2 Area assessment

| Area | Status | Evidence/reason | Condition or boundary |
|---|---|---|---|
| Business | `READY` | D01 business flow and D02 global acceptance are FINAL | Actual Business Owner/UAT acceptance remains later confirmation |
| Requirements | `READY_WITH_CONDITIONS` | D02 FINAL and Merge groups are explicit | Do not use nonexistent `FR-FILE-006..009`; controlled source correction remains open |
| Feature Catalog | `READY` | D03 has 550 unique canonical Feature IDs and 33 Core mapping | Never turn one Tool Card into one service/endpoint automatically |
| Architecture | `READY_WITH_CONDITIONS` | Modular Monolith, Clean Architecture, separate deployables, adapter/resolver locked | Exact `ContentIntelligence` and Local Worker repository paths need bootstrap decision |
| Technology | `READY_WITH_CONDITIONS` | .NET 10 LTS, WinUI 3, .NET MAUI, MSTest/MTP, SQL/EF/SQLite and Azure DevOps locked | Exact SDK patch, package versions/sources and provider products unapproved |
| API | `READY_WITH_CONDITIONS` | D06 REST/SignalR contracts locked; Merge is local-only and needs no processing endpoint | Runtime contracts absent; unrelated admin/monitoring gaps do not block local Merge |
| Database | `READY_WITH_CONDITIONS` | D07 logical ownership/storage model locked | Merge begins with local SQLite metadata; actual schema/migration/integration tests absent |
| Security | `READY_WITH_CONDITIONS` | D08 secure SDLC, file/Worker/provider/privacy boundaries locked | Runtime scans/AuthZ/file tests and package/provider approval required before merge |
| Role/Permission | `READY_WITH_CONDITIONS` | D09 logical permission model locked | SSO/claim mapping not needed for pure domain tests, but required before integrated QA |
| Audit | `READY_WITH_CONDITIONS` | D10 typed L2 accepted/terminal policy and sensitive exclusions locked | Implement safe intent/contract; server audit persistence is not a Merge processing dependency |
| Testing | `READY_WITH_CONDITIONS` | D11 defines 50 Merge Unit cases; D12 defines 36 system cases | Test projects/runners/corpus/results do not exist; exact packages must be approved |
| UAT Design | `READY` | D13 defines 27 Merge UAT scenarios and 33/33 Core | This is design only; actual UAT remains `NOT_RUN` |
| Documentation | `READY` | D01–D19 FINAL and trace baseline available | Implementation/release docs and actual UI screenshots must be generated later |
| Support | `NOT_READY` | D16 design exists; D19 says Service Desk/owners/drills absent | Does not block Phase 0–3; blocks Pilot/operations acceptance |
| Deployment Design | `READY_WITH_CONDITIONS` | D17 defines build-once, PR/pipeline, environments and rollback principles | No actual pipeline/environment/artifact; Production is not required for bootstrap |
| Cartography | `READY_WITH_CONDITIONS` | D18 logical components/interfaces/data flows/hosts are FINAL | Actual project paths, owners and artifact links must be populated as code appears |
| Handover Baseline | `READY_WITH_CONDITIONS` | D19 GATE-0 PASS; open-item/evidence/access registers exist | Actual handover is `NOT_STARTED`; use registers as evidence sinks, not completion claims |
| Pertamina Confirmation | `CONFIRMATION_REQUIRED` | Owners, repo/access, package/provider/license, SSO and all Production gates remain open | Apply phase-specific classification in §13; do not block local work with late Production facts |

## 1.3 Readiness rule

`READY_WITH_CONDITIONS` is not permission to infer missing facts. A phase starts only when its exact entry conditions are evidenced. A missing later-phase confirmation does not block earlier local/foundation work unless it changes architecture, dependency admission, data handling, or repository authority.

---

# 2. Blocking/non-blocking gaps

One gap may block several future boundaries. Classification below states its **earliest** blocking point.

## 2.1 `BLOCKS CODING`

| Gap | Why it blocks | Minimum resolution |
|---|---|---|
| Official Azure DevOps repository/project absent | No authoritative history, branch, PR or source custody | Confirm org/project/repository ID/URL, owner, default branch and access |
| Repository/integrator/technical owners are TBD | No one can approve scope, dependency or merge | Confirm human integrator and repository/technical decision authority |
| Access not provisioned | Cannot safely create branch/worktree or validate least privilege | Approved developer/reviewer access; no shared credential |
| Exact .NET 10 SDK patch and package feeds unpinned | First restore is non-reproducible and supply-chain ambiguous | Approve `global.json` value, feeds, source mapping/credentials mechanism |
| Initial dependency set/license/security review absent | Bootstrap could silently introduce unapproved packages | Approve minimal SDK/MSTest/MTP/WinUI/SQLite/analyzer package set or stage them behind task-specific admission |
| Baseline freeze has no implementation manifest | Codex tasks could read wrong/stale copies | Record D01–D19 and Master v4.2 canonical paths/hashes; resolve archive selection rule |

The following **do not block every line of code**, but block specific projects:

- Local Worker exact path/host ownership blocks Local Worker project acceptance, not Contracts/Domain bootstrap.
- PDF engine approval blocks real adapter/provider and Merge completion, not contract/domain/UI work against a deterministic fake.
- SQLite package/migration decision blocks persistence integration, not pure Workspace/Application contracts.

## 2.2 `BLOCKS POC`

| Gap | Affected PoC | Resolution |
|---|---|---|
| Evaluation corpus/data classification not approved | All fidelity/security PoCs | Synthetic/approved corpus manifest, handling and disposal |
| License/cost reviewer and provider owner absent | All engine/provider PoCs | Named functions and decision record; no procurement assumption |
| Security/data/network conditions unknown | AI, HTML, Office, OCR server options | Approved isolated environment, egress/data boundary and evidence rules |
| Platform/device matrix absent | Scanner, Office, local AI, packaging-sensitive engines | Approved representative platforms/devices; no fake parity claim |
| Measurement method/environment absent | Resource/performance aspects | Versioned hardware/environment/tool/workload manifest; no invented limits |
| PoC exit authority absent | Every PoC | Confirm authority for `APPROVED_ENGINE`, limitation, block or defer decisions |

## 2.3 `BLOCKS QA`

- clean reproducible pipeline build and immutable artifact/digest;
- executable Unit/Architecture/Contract/Integration tests and approved corpus;
- QA environment, identity/permission fixtures, configuration, data reset and evidence store;
- real PDF provider build approved for the scope under test;
- SQLite migration/integrity/recovery test path;
- secret/privacy/static/dependency scan gates;
- owner and defect/retest workflow;
- safe evidence manifest tied to exact build/environment.

## 2.4 `BLOCKS PILOT`

- QA exit and exact RC/release manifest;
- UAT-ready manual labels/screenshots and actual UAT scope;
- security review/VA-PT applicability and high-risk retest;
- Desktop/Mobile signing and approved distribution for exposed clients;
- Pilot topology, identity, monitoring/alerts, capacity observation and backpressure;
- Support/Service Desk, Incident route, owners/deputies and runbook exercises;
- tested rollback, backup/restore path where applicable;
- provider/license/support approval for every enabled optional route.

## 2.5 `BLOCKS PRODUCTION`

- all Pilot exit evidence and actual UAT/business acceptance;
- Production infrastructure/network/DNS/TLS/SSO/workload identities;
- code signing/distribution custody and revocation;
- approved data classification/retention/legal hold/deletion;
- backup/restore/DR and approved SLA/RPO/RTO;
- monitoring/alert/on-call/Service Desk/operations acceptance;
- VA/PT/security acceptance and residual-risk authority;
- license/provider contracts, CMDB/Application ID, named owners and Production authority;
- GATE-1..10 evidence in Document 19.

## 2.6 `DOES_NOT_BLOCK CURRENT PHASE`

The following may remain open during Phase 0–1 if visible and isolated:

- Production topology, hosting, network, SLA/RPO/RTO and CMDB ID;
- Hostinger KVM 2 provisioning details; no server is needed for Merge processing;
- code signing and mobile distribution for pure domain/contracts/tests;
- actual Service Desk, on-call, KT and project closure;
- UAT execution and release screenshots;
- server queue/backpressure implementation for a local-only Merge slice;
- Content Intelligence provider and secondary modules not used by Merge;
- `FR-FILE-006..009` controlled correction, provided no task cites or invents them;
- final PDF vendor choice while contracts/domain/UI use approved fakes and PoC remains parallel.

---

# 3. Implementation phases

## PHASE 0 — Repository Bootstrap

- **Objective:** establish one reproducible, governed monorepo and fast merge gate.
- **Deliverables:** official Git repo; baseline manifest; root build files; minimal solution/projects; test discovery; `eng/`; `pipelines/`; safe README/contribution/task conventions.
- **Dependencies:** repo/access/owners; exact SDK patch; feeds and initial dependency approval.
- **Parallel Tasks:** baseline manifest, root build policy, solution scaffold, architecture-test skeleton, CI definition can proceed on separate task branches after root ownership is assigned.
- **Entry Criteria:** §14.1 Phase-0 gate PASS.
- **Exit Criteria:** clean checkout restores/builds; at least one meaningful smoke/discovery test; zero missing-test-project false green; PR pipeline green; no product Feature implementation.
- **Tests:** build smoke, test discovery, architecture baseline, package/source policy validation.
- **Security:** secret scan, dependency/source verification, no credentials/config values in repo.
- **Performance:** pipeline duration measured only; no target invented; cache cannot bypass integrity.
- **Risks:** over-scaffold, package sprawl, concurrent root-file conflicts.
- **Evidence:** commit/PR, pipeline run, approved dependency manifest, build/test reports.

## PHASE 1 — Shared Foundation

- **Objective:** create only reusable boundaries required by Merge PDF.
- **Deliverables:** Contracts, Security, Observability, Storage/Processing abstractions; canonical Feature Registry for `FEAT-PDF-005`; PDF Domain/Application/Contracts skeleton with behavior tests; minimal Workspace and Desktop/Local Worker contracts.
- **Dependencies:** Phase 0; path ownership decision; no vendor-specific package needed for contracts.
- **Parallel Tasks:** Agent A foundations; Agent B PDF contracts/domain; Agent C Desktop shell/input/workspace; Agent D test/security/pipeline—after contract ownership is frozen.
- **Entry Criteria:** M2 pipeline green; project references/owners approved.
- **Exit Criteria:** dependency rules pass; Feature Registry returns Merge descriptor; request/plan/result/progress/error contracts versioned; deterministic fakes available; no vendor/UI/ORM leaks into Domain/Contracts.
- **Tests:** P0/P1 contract/domain tests, authorization decision tests, sensitive-data canaries, architecture references.
- **Security:** deny-by-default context; opaque file refs; no document/password in logs/contracts beyond required processing boundary.
- **Performance:** streaming/cancellation/resource-preflight abstractions; no whole-file API assumption.
- **Risks:** god BuildingBlocks, premature generalized workflow engine, contract churn.
- **Evidence:** contract index, reference graph, tests, pipeline reports and design decision notes.

## PHASE 2 — PoC Wave

- **Objective:** resolve engine/provider feasibility early without blocking provider-independent foundations.
- **Deliverables:** eight initial PoC reports in §6, corpus manifests, license/security/resource/fidelity results and governed disposition.
- **Dependencies:** isolated PoC environment, owner/reviewer, approved corpus and evidence rules; adapter contracts from Phase 1 where useful.
- **Parallel Tasks:** PDF Engine starts first; Office→PDF, PDF→Office, OCR, existing text edit, HTML→PDF, PDF/A and Content Intelligence run independently with bounded concurrency.
- **Entry Criteria:** PoC charter, scope, corpus, environment, reviewer and stop conditions approved.
- **Exit Criteria:** each PoC has `APPROVED_ENGINE`, `APPROVED_WITH_LIMITATION`, `BLOCKED_ALTERNATIVE_REQUIRED`, or `DEFERRED_BY_GOVERNED_DECISION`; PDF Engine decision available before Merge provider integration completes.
- **Tests:** fidelity oracles, malicious/unsupported inputs, large-file/resource observation, cancellation/failure and repeatability.
- **Security:** no public AI, least privilege, egress/data minimization, active-content/SSRF controls as applicable.
- **Performance:** measured CPU/RAM/temp/duration/concurrency on declared environment; no universal capacity number.
- **Risks:** demo-only success, unrepresentative corpus, license surprises, comparing providers without consistent oracle.
- **Evidence:** PoC ID/version, environment/corpus/digest, results/limits, decision/owner/reference.

## PHASE 3 — Merge PDF Vertical Slice

- **Objective:** deliver `FEAT-PDF-005` end-to-end locally as the first user-facing slice.
- **Deliverables:** picker/drag-drop, validation, order/remove, Merge plan, approved adapter/resolver, local execution/Worker threshold path, progress/cancel, result validation, Save As, source protection, SQLite metadata, safe logs/errors, tests and docs.
- **Dependencies:** Phase 0–1; PDF Engine PoC before real adapter acceptance; SQLite dependency approval before persistence integration.
- **Parallel Tasks:** domain/adapter; Desktop/workspace; tests/security/pipeline; shared contracts changed only through a foundation task.
- **Entry Criteria:** M2; stable Merge contract; provider integration decision path; task backlog in §5 approved.
- **Exit Criteria:** 50 Merge Unit cases and applicable 36 system scenarios pass/disposition; UAT package maps 27 scenarios; Local-First zero processing API/upload/SignalR; 8-layer DoD evidence; no Blocker/Critical.
- **Tests:** D11 `UT-MERGE-001..050`, D12 `TP-MRG-001..036`, architecture/contract/integration/security/UI/recovery and later UAT.
- **Security:** password memory-only; untrusted input; source/output safety; no sensitive evidence; correct permission/origin parity.
- **Performance:** representative/boundary workloads measured; UI responsive; streaming/temp/disk/handles bounded; no invented thresholds.
- **Risks:** PDF fidelity, large files, Worker/IPC, Save As atomicity, provider delay.
- **Evidence:** M4/M5 build, test results, corpus, artifact digest, resource report, screenshots only when actual UI is stable.

## PHASE 4 — Core PDF Local Operations

- **Objective:** reuse the Merge foundation for eight mostly local Core cards.
- **Deliverables:** Split, Remove pages, Extract pages, Organize PDF, Compress PDF, Repair PDF approved path, Rotate PDF, Compare PDF.
- **Dependencies:** Merge shared family stable; repair fallback limitations governed.
- **Parallel Tasks:** page operations family; optimize/compare family; UI surfaces; regression/security tests.
- **Entry Criteria:** M5 and family contract change review.
- **Exit Criteria:** each card has canonical mapping, source safety, result oracle, offline/local proof, Unit/system/UAT mapping and limitation disposition.
- **Tests:** family matrix D11 §34, 33-Core D12/D13 rows, Merge regression.
- **Security:** malicious/corrupt/encrypted file paths, no active-content execution, safe compare evidence.
- **Performance:** page/size/file-count boundary measurement and Local Worker routing.
- **Risks:** accidental engine duplication, shared page-order regressions, repair false success.
- **Evidence:** Core disposition registry and family test/resource reports.

## PHASE 5 — Core Conversion/OCR

- **Objective:** implement twelve conversion/scanner/OCR Core cards using approved PoC outcomes.
- **Deliverables:** Scan to PDF, OCR PDF, JPG→PDF, Word/PPT/Excel/HTML→PDF, PDF→JPG/Word/PPT/Excel, PDF/A.
- **Dependencies:** relevant PoCs approved/limited; provider/engine/license/security decision; optional server contracts only where approved.
- **Parallel Tasks:** deterministic image conversions; Office→PDF; PDF→Office; OCR/scanner; HTML/PDF-A security/conformance.
- **Entry Criteria:** each active capability has an approved PoC disposition and typed adapter contract.
- **Exit Criteria:** fidelity/conformance/device/corpus evidence, capability-aware route, no public service, safe fallback and Core test/UAT coverage.
- **Tests:** real provider integration, fidelity oracle, malicious active content, OCR quality, conformance, platform/recovery/load.
- **Security:** macro/script/SSRF/egress, OCR content privacy, device permissions, temp cleanup.
- **Performance:** provider/device-specific measured budgets and bounded server admission if enabled.
- **Risks:** font/layout fidelity, OCR accuracy, scanner variability, license/cost and heavy resource use.
- **Evidence:** PoC-to-provider decision, format support matrix, versioned corpus and measured results.

## PHASE 6 — Core Edit/Forms/Security

- **Objective:** implement nine edit/forms/security cards with stricter validation.
- **Deliverables:** page numbers, watermark, crop, Edit PDF, PDF Forms, Unlock, Protect, visual non-PKI Sign, Redact.
- **Dependencies:** existing-text edit PoC; security/permission/audit contracts; redaction forensic validator; protected local asset handling.
- **Parallel Tasks:** simple annotation; edit/forms; protect/unlock; signature; redaction hardening.
- **Entry Criteria:** high-risk task threat/test plan approved; PoC limitations visible.
- **Exit Criteria:** real removal for Redact, password memory-only, Sign explicitly non-PKI, protected results reopen, L3 evidence where required, no source destruction.
- **Tests:** negative authorization, password/secret canary, forensic result validation, forms reopen, UAT D2/D3.
- **Security:** reauth/confirmation policy, no signature asset sync/log, no overlay-only redaction success.
- **Performance:** large/complex PDF editing resource measurement; safe cancellation/rollback.
- **Risks:** false redaction, font/object edit limits, protected-file interoperability, user misunderstanding of visual signature.
- **Evidence:** security review, forensic/fidelity results, limitation/manual updates and high-risk UAT package.

## PHASE 7 — Core Intelligence

- **Objective:** implement three intelligence cards without making public AI or AI a default extraction dependency.
- **Deliverables:** AI Summarizer, Translate PDF, PDF→Markdown deterministic-first.
- **Dependencies:** Content Intelligence PoC, approved local/internal provider, classification/permission/consent/minimization, provider/license/capacity controls.
- **Parallel Tasks:** deterministic extraction/Markdown; provider resolver; summarization; translation; security/grounding evaluation.
- **Entry Criteria:** provider decision and data-handling approval; public egress denial test.
- **Exit Criteria:** grounded/traceable outputs, language/layout limitations, deterministic Markdown before OCR fallback, safe offline/unavailable behavior.
- **Tests:** prompt-injection, invented command/Feature denial, selected-scope grounding, data minimization, provider failure/timeout/quota.
- **Security:** no public AI, no content/prompt in ordinary telemetry/audit, untrusted model output.
- **Performance:** model/provider-specific measurement and concurrency/backpressure if internal server route is enabled.
- **Risks:** hallucination, translation distortion, content disclosure, local model resource pressure.
- **Evidence:** provider approval, evaluation corpus/results, privacy/egress tests and user-visible limitations.

## PHASE 8 — Web/Mobile Curated Capabilities

- **Objective:** expose only approved subsets of existing canonical Core capabilities with platform-natural UX.
- **Deliverables:** selected Web and .NET MAUI routes/surfaces; capability/exposure registry; no duplicate business rules/engines.
- **Dependencies:** Core disposition gate; SSO/API contracts for server surfaces; mobile packaging/device decisions.
- **Parallel Tasks:** Web integration; Mobile curated UI/device adapters; cross-platform contract/UX testing.
- **Entry Criteria:** exact Feature×platform scope approved; secondary user tools remain out of active queue until Core gate.
- **Exit Criteria:** parity where exposed, explain/hide where unavailable, same authorization/result contracts, platform-specific UX accepted.
- **Tests:** contract/E2E, mobile/device lifecycle, adaptive/accessibility/localization, cross-platform parity/limitations.
- **Security:** SSO/session, secure store, device permission, no direct SQL/storage/provider access.
- **Performance:** device/network profiles measured; no desktop workload assumed safe on mobile.
- **Risks:** fake parity, packaging/signing delay, duplicated client rules, MAUI platform differences.
- **Evidence:** exposure matrix, device/browser results, actual UI/manual validation.

## PHASE 9 — Hardening, UAT, Pilot

- **Objective:** convert implemented Core scope into a controlled QA/UAT/Pilot candidate.
- **Deliverables:** immutable RC; full test/security/performance/recovery evidence; manuals; UAT execution; monitoring/support/runbooks; Pilot package.
- **Dependencies:** phases 0–8 released scope complete/dispositioned; QA/Pilot confirmations.
- **Parallel Tasks:** regression; security/VA-PT; performance/load; UAT; operations/readiness; docs remediation.
- **Entry Criteria:** no missing implementation owner/artifact/trace; QA environment and data ready.
- **Exit Criteria:** GATE-2..7 evidence for Pilot scope; no Blocker/Critical; actual UAT result; rollback/restore/support drills; observation plan.
- **Tests:** complete D12 pyramid, D13 UAT, compatibility/recovery/security/performance.
- **Security:** VA/PT as applicable, findings/retest, signed artifacts, least-privilege operations.
- **Performance:** measured baselines and approved limits; overload fails safely.
- **Risks:** late cross-platform defects, insufficient capacity, documentation drift, weak operational ownership.
- **Evidence:** RC manifest/digests, results/defects, UAT/sign-off, runbook/drill and Pilot approval.

## PHASE 10 — Production Preparation

- **Objective:** prove Production readiness; not deploy merely because coding is done.
- **Deliverables:** approved topology/network/SSO/identities/config/secrets/signing/distribution; migration/backup/restore/DR; monitoring/support; Production release/observation/rollback packages.
- **Dependencies:** Pilot exit, Pertamina confirmations, Business/Security/Operations acceptance.
- **Parallel Tasks:** infrastructure/network/security; data/recovery; release; support/operations; closure/handover evidence.
- **Entry Criteria:** M10 Pilot Candidate accepted for evaluation and no prohibited gap.
- **Exit Criteria:** M11 Production Readiness decision supported by GATE-1..8; deployment remains a separate authorized action.
- **Tests:** Production-like smoke/recovery/compatibility, controlled security validation, operational exercises.
- **Security:** no Codex Production access; approved identities/custody; evidence/secret separation.
- **Performance:** Pilot observations converted to approved capacity/admission/backpressure limits.
- **Risks:** topology assumption, untested restore, owner/support gap, signing/secret failure.
- **Evidence:** approved Production Readiness package; no fabricated approval/date.

---

# 4. Repository bootstrap

## 4.1 Bootstrap principles

1. Structured monorepo; task-scoped branch/worktree; human merge gate.
2. Create a project only when its first behavior/task exists. Do not scaffold all 12 modules or all hosts as empty shells.
3. Root/shared files have one owner and one foundation task at a time.
4. Domain/contracts do not reference UI, provider SDK, ORM, filesystem path, or network infrastructure.
5. Package versions live only in `Directory.Packages.props`; package admission is explicit.
6. Merge is local-only. Do not create API/SignalR/server-processing projects merely to complete the first slice.
7. Local Worker is a separate local deployable, not `Pertamina.Oto.Worker` server and not a public port.
8. Generated artifacts/reports do not contain secret, document content, password, raw user path or sensitive diagnostic.

## 4.2 Proposed initial tree

This is a plan, not files created by this report:

```text
pertamina-oto/
├── global.json
├── Directory.Build.props
├── Directory.Packages.props
├── Pertamina.Oto.sln
├── src/
│   ├── BuildingBlocks/
│   │   ├── Pertamina.Oto.Contracts/
│   │   ├── Pertamina.Oto.Security/
│   │   ├── Pertamina.Oto.Observability/
│   │   ├── Pertamina.Oto.Storage.Abstractions/
│   │   └── Pertamina.Oto.Processing.Abstractions/
│   ├── Modules/
│   │   ├── Pdf/
│   │   │   ├── Pertamina.Oto.Modules.Pdf.Domain/
│   │   │   ├── Pertamina.Oto.Modules.Pdf.Application/
│   │   │   ├── Pertamina.Oto.Modules.Pdf.Contracts/
│   │   │   └── Pertamina.Oto.Modules.Pdf.Infrastructure/
│   │   ├── FileProductivity/
│   │   │   └── Pertamina.Oto.Modules.FileProductivity.Application/
│   │   └── Workspace/
│   │       └── Pertamina.Oto.Modules.Workspace/
│   └── Hosts/
│       ├── Pertamina.Oto.Desktop/
│       └── Pertamina.Oto.LocalWorker/
├── tests/
│   ├── Unit/
│   │   ├── BuildingBlocks/
│   │   ├── Modules/Pdf/
│   │   ├── Modules/Workspace/
│   │   └── Hosts/Desktop/
│   ├── Integration/
│   ├── Architecture/
│   │   └── Pertamina.Oto.ArchitectureTests/
│   └── Contract/
│       └── Pertamina.Oto.ContractTests/
├── docs/
│   └── implementation/
├── eng/
│   ├── baseline/
│   ├── build/
│   ├── test/
│   └── security/
└── pipelines/
    └── azure-pipelines-ci.yml
```

`Pertamina.Oto.sln` is a proposed conservative name. If the approved toolchain chooses another supported solution format, record it in the bootstrap task; do not maintain duplicate solution authorities.

## 4.3 Project creation sequence

| Project/folder | Create when | First responsibility | Do not place here |
|---|---|---|---|
| Contracts | Phase 0/1 | canonical IDs, typed/versioned cross-boundary primitives | workflow, provider, UI, ORM |
| Security | Phase 1 | authorization context/decision interfaces and safe security primitives | role branching scattered in UI |
| Observability | Phase 1 | correlation and safe structured event/log abstractions | document/password/raw path payload |
| Storage.Abstractions | Phase 1 | opaque input/temp/output refs and atomic publication ports | physical path as business authority |
| Processing.Abstractions | Phase 1 | engine/job/progress/cancel/capability ports | vendor SDK or Feature branching |
| Pdf.Domain | Phase 1 | ordered Merge rules and invariants | filesystem/UI/provider code |
| Pdf.Application | Phase 1 | use case, validation orchestration, plan/result lifecycle | vendor SDK/direct SQLite |
| Pdf.Contracts | Phase 1 | Merge request/plan/result/error schemas | engine-specific option names |
| Pdf.Infrastructure | Phase 2/3 | approved PDF adapter and result reader | selection policy/business rules |
| FileProductivity.Application | Task MRG-005 | picker/drop input normalization contract | Desktop control types |
| Workspace | Task MRG-009/018 | working state/local job/history/SQLite boundary | PDF transformation logic |
| Desktop Host | Task MRG-002 | WinUI shell and adapters | business rules/SQL/provider SDK |
| Local Worker Host | After local job/IPC decision | isolated job-scoped execution | public endpoint/user token/server queue |
| Unit Tests | With each production project | behavior at owner boundary | real network/provider fidelity claims |
| Architecture Tests | Phase 0 onward | reference/path/forbidden dependency rules | product behavior assertions |
| Contract Tests | Phase 0/1 onward | schema/version/serialization/registry completeness | invented endpoint/schema |

Projects are added incrementally by the task that introduces tested behavior; they are not all created empty on day one.

## 4.4 Root file responsibilities

| File/area | Minimum content | Owner/change guard |
|---|---|---|
| `global.json` | exact approved .NET 10 SDK patch and roll-forward policy | Foundation task only; value must come from approved environment |
| `Directory.Build.props` | language/nullable/analyzer/warnings/deterministic build/common metadata; no secret/path | Foundation task; changes run full build/tests |
| `Directory.Packages.props` | central approved package versions only | Dependency task + license/security review |
| Solution | only active projects; deterministic organization | Repository owner; no duplicate solution authority |
| `eng/baseline/` | canonical D01–D19/Master v4.2 manifest/hashes and decision references | Documentation/Integrator review; no copies of secrets |
| `eng/build/` | local/CI entry scripts and reproducibility checks | Pipeline owner; no environment-specific Production command |
| `eng/test/` | MTP filters/settings/result-sanitization and corpus manifest tooling | Test owner; zero-test is failure |
| `eng/security/` | approved scan configuration/allowlists/waiver schema | Security owner; no disabled check to go green |
| `pipelines/` | PR CI only initially | Pipeline owner; no Production deploy stage |

## 4.5 Bootstrap work packages

| Task ID | Deliverable | Exit evidence |
|---|---|---|
| `BOOT-001` | Repository authority/access and baseline manifest | approved repo reference, owner, D01–D19/Master v4.2 hashes |
| `BOOT-002` | SDK/feed/package admission | approved SDK patch, feed/source policy, initial dependency record |
| `BOOT-003` | Root build files and solution | clean restore/build from empty agent/worktree |
| `BOOT-004` | Minimal BuildingBlocks/active projects | reference graph and no empty unrelated modules |
| `BOOT-005` | MSTest/MTP discovery | meaningful smoke/testkit test discovered and reported |
| `BOOT-006` | Architecture and Contract test skeleton | forbidden reference and registry/schema test executable |
| `BOOT-007` | PR CI pipeline | Restore→Build→Unit→Architecture→Contract→security/static→report green |
| `BOOT-008` | Developer/Codex task workflow | branch/worktree/task metadata, allowed paths and stop protocol |

---

# 5. Merge PDF backlog

## 5.1 Epic boundary

```text
Epic ID     : EPIC-CORE-PDF-001
Feature ID  : FEAT-PDF-005
Title       : Merge PDF — First Local Vertical Slice
Mode        : LOCAL preferred; immediate or Local Worker
Server      : no processing API, upload or SignalR
Permission  : document.content.process
Evidence    : L2 accepted + terminal safe metadata
```

All tasks inherit these guards:

- `FR-FILE-006..009` must not be cited or invented;
- original files and existing targets are never overwritten silently;
- password is memory-only; document bytes/raw paths do not enter logs/Audit/test reports;
- Assistant/UI/File entry use the same application contract;
- package/provider/root-file changes require their own authorized foundation/dependency task;
- `docs/**/*FINAL*.md`, unrelated modules and Production configuration are do-not-touch unless a separate controlled task authorizes them;
- a task is not DONE until its required tests and downstream handoff pass.

## 5.2 Task backlog

| Task ID | Requirement IDs | Feature IDs | Objective | Allowed Paths | Do-Not-Touch Paths | Dependencies | Acceptance Criteria | Tests | Security | Performance | Output |
|---|---|---|---|---|---|---|---|---|---|---|---|
| `TASK-MRG-001` | `IR-DEVOPS-001..004`; `BR-GOV-003` | `FEAT-PDF-005` enabling | Repository foundation | root build files; solution; `eng/**`; `pipelines/**`; bootstrap tests | D01–D19 FINAL; prototype; product modules beyond scaffold | `BOOT-001..002` | clean restore/build; test discovery non-zero; PR CI stages present; no Product behavior | bootstrap smoke; architecture discovery; package/source policy | secret scan; no credential/feed token; pinned sources | measure CI only; no target invented | root scaffold, solution, CI run/evidence |
| `TASK-MRG-002` | `UIR-UI-001..003,005,008..009`; `NFR-ARCH-005` | `FEAT-PDF-005` | Minimal Desktop shell and Merge navigation surface | `src/Hosts/Pertamina.Oto.Desktop/**`; Desktop Unit tests | Pdf provider; SQLite implementation; Web/API/Mobile; root package files | MRG-001; UI contract task boundary | shell opens; Merge surface reachable; no business rule in UI; unavailable states explicit | Desktop view-model/state Unit; UI smoke; later `TP-MRG-009..011` | no file read before user action/authorization; no secret/content logs | responsive state changes measured; no processing on UI thread | Desktop shell + state adapter + tests |
| `TASK-MRG-003` | `BR-GOV-003,008`; `UIR-UI-005..006` | `FEAT-PDF-005` | Typed Feature Registry baseline containing Merge descriptor | Contracts/registry project; Contract tests; baseline registry data | UI labels as authority; provider SDK; unrelated 549 Feature handlers | MRG-001 | canonical ID resolves once; version/exposure/capability fields typed; unknown/disabled/incompatible denied | `UT-MERGE-050`; registry uniqueness/count-scope; schema compatibility | client cannot forge permission/capability; no arbitrary handler name | lookup deterministic/bounded; no network per button | registry contract/descriptor + snapshot tests |
| `TASK-MRG-004` | `FR-PROC-001..002`; `NFR-REC-002..004`; `BR-GOV-003` | `FEAT-PDF-005` | Versioned Merge request/plan/result/error contracts | Pdf Contracts; Contracts; Contract tests | vendor option names; physical paths; HTTP route; ORM/UI types | MRG-001,003 | ordered ≥2 input refs; correlation; capability/policy facts; result validation state; no server contract | `UT-MERGE-001..005,039..042,047..050`; serialization/schema tests | opaque refs; password excluded; actor/permission not client-trusted | streaming/ref semantics; no bytes copied into command | approved contract snapshot/index |
| `TASK-MRG-005` | `FR-FILE-001`; `FR-STOR-001`; `UIR-UI-005,009` | `FEAT-PDF-005` | File picker input selection and normalized ordered references | Desktop picker adapter; FileProductivity Application; owner tests | Pdf engine; raw path in Domain/telemetry; recent-file secondary Features | MRG-002,004; Storage abstraction | user selects ≥0 files; order preserved; cancellation has no side effect; refs profile-scoped | `TP-MRG-001,003..004,009`; adapter Unit | explicit user action; authorized refs; no automatic upload/content logging | picker does no full-file load; UI remains responsive | picker adapter, normalized input list, tests |
| `TASK-MRG-006` | `FR-FILE-002`; `UIR-UI-009` | `FEAT-PDF-005` | Drag/drop parity with picker | Desktop drag/drop adapter and tests | business validation duplication; shell-unrelated UI | MRG-002,005 | drop produces same ordered canonical inputs as picker; invalid drop is safe/actionable | `UAT-MRG-002`; `TP-MRG-009`; parity Unit | no implicit recursive folder/URL/network intake; no hidden processing | bounded event handling; no file bytes on UI thread | drag/drop adapter + parity evidence |
| `TASK-MRG-007` | `FR-FILE-010..015`; `FR-STOR-006`; `SR-SEC-003..005,014` | `FEAT-PDF-005` | Fast/structural/security/password/capacity validation pipeline | Workspace validation area; Pdf Application orchestration; validation Unit tests | public scanner/provider; password persistence; server API; invented limits | MRG-004..006; approved local validation dependencies | invalid/inconclusive item never reaches engine; clear status; stale input revalidated; password ephemeral | `UT-MERGE-011..019,025`; `TP-MRG-004..005,018,024,030..031` | untrusted PDF; password memory-only; safe failure; allowlisted file access | streaming fast gate; async structure check; capacity preflight; measured limits only | validation pipeline/contracts/results/tests |
| `TASK-MRG-008` | `FR-PROC-001..002`; `FR-STOR-002`; `NFR-REC-002..003` | `FEAT-PDF-005` | Deterministic ordered Merge plan | Pdf Domain/Application and Unit tests | UI controls; provider/IO/SQLite; fuzzy Feature lookup | MRG-004,007 | ≥2 READY occurrences; explicit duplicates preserved; unknown occurrence rejected; semantically deterministic | `UT-MERGE-001..010,047`; plan/property tests | only authorized validated refs; no password/content copy | plan creation bounded by approved count; no whole-file access | Merge plan builder/domain invariants |
| `TASK-MRG-009` | `FR-STOR-002`; `UIR-UI-003..004` | `FEAT-PDF-005` | Reorder/remove/Undo working state | Pdf Application; Workspace; Desktop state; owner Unit tests | physical source deletion; engine/provider; generic editor framework | MRG-002,008 | visible order equals plan; remove affects occurrence only; Undo restores; execute disabled under two inputs | `UT-MERGE-005..010`; `TP-MRG-006..008`; `UAT-MRG-006..009` | operations cannot mutate source; no audit noise for reorder | immutable/delta state; no file reload per reorder | tested working-state model + UI adapter |
| `TASK-MRG-010` | `FR-PROC-001..002`; `BR-GOV-006`; `NFR-PERF-003,007,010` | `FEAT-PDF-005` | PDF engine provider adapter behind contract | Pdf Infrastructure; adapter integration tests; approved PoC evidence reference | UI/Domain; provider name in public contract; second engine without decision; package root outside dependency task | `POC-PDF-001` decision; MRG-004,007..008; dependency approval | typed input mapping; cancellation/failure; validated output handoff; limitation/version exposed internally | adapter integration + real corpus; `UT-MERGE-020,023,029..034` orchestration | provider sandbox/least privilege; active-content safe; license/security evidence | streaming/temp/resource metrics; large file behavior measured | one approved adapter + version/limitation manifest |
| `TASK-MRG-011` | `FR-PROC-001..005`; `NFR-PERF-007..009`; `UIR-UI-010` | `FEAT-PDF-005` | Capability-aware engine resolver | Processing abstractions; Pdf Application; resolver Unit tests | provider SDK; UI-selected engine; silent server fallback; public AI/network | MRG-003,004,010 | local capable/healthy/resource route chosen; approved fallback deterministic; unavailable safely reported; server path impossible | `UT-MERGE-020..024`; resolver decision table | policy/capability facts trusted from approved snapshot; no arbitrary provider ID | decision is local/bounded; resource facts considered | resolver + decision trace/tests |
| `TASK-MRG-012` | `FR-PROC-002,005`; `NFR-PERF-003,007`; `NFR-REC-001..005` | `FEAT-PDF-005` | Execute Merge locally immediate or Local Worker | Pdf Application/Infrastructure; Processing abstractions; LocalWorker when authorized; Integration tests | API/SignalR/server queue; user token in Worker; source destructive write | MRG-007..011; Local Worker path/IPC task when background needed | local-only in online/offline; immediate/background decision accurate; retry/idempotency bounded; source safe | `UT-MERGE-020..032,041..043`; `TP-MRG-012..023` | job-scoped refs; Worker whitelist/identity/version; no upload | UI responsiveness; Worker threshold measured; bounded CPU/RAM/temp/handles | local execution coordinator + IPC integration evidence |
| `TASK-MRG-013` | `UIR-UI-003`; `NFR-REC-004`; `FR-REC-001..004` | `FEAT-PDF-005` | Accurate progress/current step/Job and Correlation projection | Processing contracts; Workspace local job/projection; Desktop state | SignalR Hub; fake ETA; Audit per progress tick | MRG-004,009,012 | immediate has no fake Job ID; background has stable local Job ID; terminal never regresses; unknown total indeterminate | `UT-MERGE-039..043`; `TP-MRG-011,034`; stale-event tests | safe IDs/counts only; no filename/content/password | progress update bounded/throttled; UI non-blocking | progress/job model and projection tests |
| `TASK-MRG-014` | `UIR-UI-003`; `FR-REC-003..006`; `NFR-REC-005` | `FEAT-PDF-005` | Cancellation and safe-point behavior | Pdf Application; Processing abstractions; Workspace; Desktop/Worker tests | force-kill as normal cancel; final-result deletion; server command | MRG-012,013 | queued cancel prevents engine; running stops safely; completed does not regress; temp non-final | `UT-MERGE-026..028`; `TP-MRG-019..020`; `UAT-MRG-012..013` | authorization/job scope revalidated; cleanup stays inside managed root | cancel latency measured, not guessed; no retry storm | cancellation contract/state tests |
| `TASK-MRG-015` | `NFR-REC-002..003`; `FR-REC-006` | `FEAT-PDF-005` | Validate output before final publication | Pdf Application; Pdf Infrastructure result reader; tests/corpus | existence-only success; byte-equality oracle; UI/provider raw details | MRG-010..012 | PDF structure/type/page order/count valid; corrupt/truncated/wrong output rejected; reopen oracle | `UT-MERGE-033..034`; `TP-MRG-028..029`; `UAT-MRG-014` | result treated untrusted; active content not executed | verifier streaming/bounded; independent resource metrics | result validator + oracle evidence |
| `TASK-MRG-016` | `FR-STOR-001,005`; `NFR-REC-001..003`; `UIR-UI-004` | `FEAT-PDF-005` | Atomic Save As to user-selected output | Storage abstractions; Desktop save adapter; Workspace publication; tests | overwrite-by-default; business path in contract; server secured storage | MRG-005,015 | validated temp→atomic publication; existing target conflict; target=source rejected; reopen succeeds | `UT-MERGE-035..038`; `TP-MRG-025..027`; `UAT-MRG-015..017` | path/reparse/scope validation; no unsafe confirmation bypass | bounded copy/move; disk capacity checked; failure leaves no half-final | Save As adapter/publication transaction tests |
| `TASK-MRG-017` | `FR-STOR-001`; `FR-REC-006`; `NFR-REC-001..003` | `FEAT-PDF-005` | Prove source/existing-target protection across success/failure/cancel/crash | Pdf/Workspace/Storage tests and test corpus | any code that writes source; Production/user real files | MRG-007,012,014..016 | pre/post digests unchanged; managed temp only; reconciliation after failure; no partial output | `UT-MERGE-014,027,034..038`; `TP-MRG-020,022..028`; `UAT-MRG-017..018,024` | synthetic fixtures; restrictive work root; no secure-delete claim | large-source verification avoids full-memory digest path where streaming available | source-safety regression suite/evidence |
| `TASK-MRG-018` | `FR-STOR-002..006`; `FR-HIST-001..003`; `FR-REC-001..004` | `FEAT-PDF-005` | Per-profile SQLite/local metadata for working state, local job, recovery and History | Workspace project; SQLite infrastructure area created by task; migrations/tests | document BLOB/password/token/signature; SQL Server; Audit table; cross-profile store | MRG-009,012..017; SQLite package approval | profile isolation; versioned migration; metadata only; History delete does not delete result/Audit; crash/reopen safe | SQLite integration/migration/integrity; `UT-MERGE-044`; `TP-MRG-023,029,032` | local protection/WAL/SHM/logout boundary; no raw sensitive path projection | bounded writes/indexes; no connection across engine work; migration timing measured | SQLite schema/migration + repository tests |
| `TASK-MRG-019` | `FR-AUDIT-001..004`; `SR-SEC-003..004`; `NFR-MON-002,004` | `FEAT-PDF-005` | Correlation and safe structured logging/Audit intents | Observability; Pdf/Workspace mapping; safe logger/evidence tests | Audit DB implementation; free-form content logging; provider/raw exception details | MRG-003,004,012..018 | one Correlation ID; L2 accepted/terminal only; History/Audit/log distinct; canaries absent | `UT-MERGE-039,044..046`; `TP-MRG-033..034` | allowlisted metadata; password/content/raw path/secret excluded | logging non-blocking and bounded; no per-progress Audit noise | log schema/mappers/canary report |
| `TASK-MRG-020` | `UIR-UI-003..004`; `FR-REC-005..006`; `IR-API-005..006` | `FEAT-PDF-005` | Stable safe error categories and actionable UI mapping | Contracts; Pdf Application; Desktop; tests | new API endpoint; stack trace/provider detail; invented error code without contract decision | MRG-004,007,011..019 | invalid/password/storage/cancel/engine/result/conflict errors mapped safely; retryability explicit; correlation shown | affected `UT-MERGE-013..019,024..030,034..038,050`; UI error tests | no enumeration/content/stack leak; deny remains safe | error mapping O(1)/bounded; no retry loop | versioned error mapping + UI resource keys |
| `TASK-MRG-021` | D11 §35 requirement set; `BR-GOV-003` | `FEAT-PDF-005` | Implement complete fast Unit suite | owner Unit-test projects; TestKit primitives | production behavior outside authorized task; ignored/disabled critical tests | MRG-003..020 behavior available | 50/50 cases implemented or governed disposition; all applicable P0/P1 pass; deterministic/isolated | `UT-MERGE-001..050` on MSTest/MTP | canary assertions; no real secrets/files/network | suite timing measured; suitable PR gate; flaky=0 accepted | Unit results/coverage-supporting report |
| `TASK-MRG-022` | D12 §18 requirement set; `BR-GOV-003` | `FEAT-PDF-005` | Real boundary/integration/system verification | Integration tests; approved corpus; Desktop/Worker/SQLite/PDF adapter test harness | fake-only fidelity claim; Production data/environment; server processing | MRG-010..021; approved engine/corpus | applicable 36 scenarios pass/disposition; real file/IPC/SQLite/provider boundaries; zero server calls | `TP-MRG-001..036`; downstream recovery/security/performance | malicious/invalid corpus isolated; evidence sanitized | representative/boundary metrics with environment manifest | Integration/system evidence package |
| `TASK-MRG-023` | `NFR-ARCH-001..005`; `BR-GOV-003` | `FEAT-PDF-005` enabling | Enforce module/reference/dependency architecture | Architecture tests and solution metadata | product implementation except minimal test fixture; external provider selection | MRG-001..004 and project graph | no UI→Domain inversion; no Contracts→provider/UI/ORM; no client SQL; no cross-module infrastructure reference | D04 §30 rules; assembly/reference/path tests | forbidden dependency and secret/source rule checks | test is fast/deterministic | architecture test report |
| `TASK-MRG-024` | `IR-DEVOPS-004`; `SR-SEC-003..005,010,013..018` | `FEAT-PDF-005` enabling | Activate dependency/secret/static/source scanning | pipeline/security configs; approved reports; scoped waiver records | app behavior; disabled rules; paid product selection; Production credentials | MRG-001; approved tools/policies; provider task when added | scans run on PR; severity gate configured by approved policy; no silent waiver; report safe | scanner self-test/canary; pipeline negative test | secret, vulnerable dependency, unsafe source/config checks | scan duration measured; heavy checks separated if necessary | scan reports + waiver schema/evidence |
| `TASK-MRG-025` | `UIR-UI-001..010`; D02 global acceptance | `FEAT-PDF-005` | UI/UX, accessibility, localization and triple-entry acceptance preparation | Desktop UI/tests; implementation docs/screenshots after stable UI | D14 FINAL direct edit without doc task; Assistant bypass; Mobile parity claim | MRG-002,005..020; actual UI build | picker/drop/order/remove/progress/cancel/Save As/error natural; keyboard/focus/scaling/ID-EN; File/Tool/Assistant contract parity | `TP-MRG-001..011,036`; `UAT-MRG-001..027` package preparation | security notices/confirmation not bypassable; no screenshot sensitive content | interactive responsiveness measured on declared device | UI acceptance evidence + known limitations |
| `TASK-MRG-026` | `NFR-DOC-001..004`; `NFR-HAND-001`; `BR-GOV-003` | `FEAT-PDF-005` | Update implementation/release traceability and usage guidance | `docs/implementation/**`; release-note/manual working copies through authorized doc task; manifests | D01–D19 FINAL/master audit files; fake screenshots; approval/sign-off | MRG-001..025 evidence | Requirement→Feature→code→tests→UAT→artifact links current; local-only/server guard documented; limitations/runbooks linked | documentation link/ID/code-sample/task test; no unverified PASS | sanitize paths/logs/screenshots; no secret/provider exploit detail | no performance claims without measured environment | implementation guide, trace manifest, release-note draft |

## 5.3 Backlog sequencing

```text
MRG-001
→ MRG-002..004
→ MRG-005..009
→ MRG-010..012  (POC-PDF-001 gates real adapter)
→ MRG-013..020
→ MRG-021..025
→ MRG-026
```

Tasks 002–004 may run in parallel after root scaffold. Tasks 005–009 may split by path after contract freeze. Tasks 010–012 must not merge a real provider before PoC/dependency approval. Verification tasks begin early and grow with behavior; they are not postponed until the end.

---

# 6. PoC Master Plan

## 6.1 Decision model

PoC membuktikan kelayakan teknis, lisensi, keamanan, kualitas hasil, dan biaya sumber daya; PoC bukan pemilihan vendor permanen atau klaim Production-ready. Semua kandidat berjalan di belakang contract sehingga keputusan dapat diganti tanpa mengubah Feature ID atau business rule.

Status keluaran yang diperbolehkan:

- `APPROVED_ENGINE` — cukup untuk implementasi dalam scope dan limitation yang tercatat;
- `APPROVED_WITH_LIMITATION` — boleh dipakai hanya pada capability/format/platform yang dibuktikan;
- `BLOCKED_ALTERNATIVE_REQUIRED` — gagal pada gate wajib; evaluasi alternatif melalui PoC baru;
- `DEFERRED_BY_GOVERNED_DECISION` — ditunda dengan owner, alasan, dampak, dan entry condition yang jelas.

Setiap PoC wajib menyimpan manifest implementasi/versi, keputusan dependency dan lisensi, corpus sintetis/berizin, oracle hasil, platform/runtime, konfigurasi, waktu proses, peak CPU/RAM/temp/disk/handle bila relevan, perilaku cancel/crash/retry, keamanan data, limitation, dan rekomendasi. Nilai target tidak boleh direkayasa; baseline diberi status `MEASURED`, `TARGET_TO_BE_BASELINED`, atau `POLICY_CONFIGURABLE`.

## 6.2 Initial parallel PoCs

| PoC ID | Priority | Objective and Core dependency | Main risk to prove | Minimum evidence | Blocks |
|---|---|---|---|---|---|
| `POC-PDF-001` | P0 | engine lokal untuk Merge dan operasi PDF dasar; `FEAT-PDF-001..005,007..008,020..030` sesuai capability | fidelity, encrypted/malformed PDF, cancel, large input, license, unsafe content | Merge corpus; page order/count/content oracle; source immutability; invalid/password/cancel/crash/resource runs; package/license/security record | real adapter `TASK-MRG-010`; M3; broad Phase 4/6 |
| `POC-CONV-001` | P0 | Office→PDF untuk Word/PowerPoint/Excel | fidelity, headless support, Office runtime/licensing, isolation, fonts/macros | representative DOCX/PPTX/XLSX; visual/structural oracle; timeout/crash; dependency/license/platform manifest | Phase 5 Office→PDF |
| `POC-CONV-002` | P0 | PDF→Word/PowerPoint/Excel | editable-output fidelity, tables/layout, scanned PDF behavior | native/scanned/mixed corpus; semantic/layout oracle; limitation matrix; resources/license | Phase 5 PDF→Office |
| `POC-OCR-001` | P0 | OCR lokal untuk Bahasa Indonesia/English and mixed scans | accuracy, rotation/noise, large page sets, language model size/license | approved corpus; measurable character/word/field oracle; page/resource/error evidence; no full OCR text in logs | `FEAT-PDF-009`; downstream intelligence on scans |
| `POC-PDF-EDIT-001` | P0 | existing-text edit feasibility | PDF text object complexity, font embedding/subsetting, reflow and safe fallback | simple/complex/font/rotated corpus; before/after rendering and structural oracle; limitation/fallback decision | `FEAT-PDF-024`; Phase 6 claim boundary |
| `POC-HTML-001` | P1 | HTML→PDF under controlled local/internal rendering | active content, network retrieval, sandbox, font/layout fidelity | offline/allowlisted cases; script/network denial; rendering oracle; timeout/resource/license record | `FEAT-PDF-014`; Phase 5 |
| `POC-PDFA-001` | P1 | PDF→PDF/A conformance path | conformance level, fonts/color profiles, attachments/signatures, verifier trust | declared target level; independent conformance result; invalid cases; limitation/license record | `FEAT-PDF-019`; Phase 5 |
| `POC-INTEL-001` | P0 | approved local/internal Summarizer, Translate, and complex PDF→Markdown | data boundary, hallucination/omission, prompt injection, OCR dependency, model/runtime license/resources | provider-boundary proof; offline/internal-only route; quality/safety corpus; refusal/injection tests; no public AI calls; traceable limitations | `FEAT-PDF-031..033`; Phase 7 |

The eight PoCs can run in parallel after Phase 0 isolation, dependency admission and test-corpus controls. `POC-PDF-001` is the only one on the critical path to completing the first real Merge provider; Merge contracts, domain rules, UI state and fake-adapter tests may proceed after Phase 1.

## 6.3 Later conditional PoCs

| PoC ID | Trigger | Evidence focus | Phase impact |
|---|---|---|---|
| `POC-SCAN-001` | approved physical scanner/device matrix is available | TWAIN/WIA or approved interface, cancel, feeder/duplex, permissions, device loss, packaging | Scan to PDF in Phase 5 |
| `POC-IMG-001` | background-removal feature is scheduled and model/runtime policy is approved | local model quality, license, CPU/RAM/package size, malformed image, no public inference | later secondary image capability; does not block 33 Core gate |

## 6.4 PoC stop rules

A PoC stops and reports to the Integrator when licensing cannot be verified, sample data authority is absent, processing requires an unapproved public endpoint, a secret/document is exposed, source files can be damaged, a candidate needs a new architectural boundary, or acceptance criteria conflict with canonical requirements. A failed PoC does not authorize silent scope reduction or a vendor choice.

---

# 7. Multi-agent implementation model

The model supports one developer plus Codex. “Agent” is an execution lane, not a new approval role. Human Integrator remains the authority for baseline interpretation, merge, dependency/provider decision and stop/go.

| Lane | Scope/ownership | Typical allowed paths | Do-not-touch | Required handoff |
|---|---|---|---|---|
| Agent 0 — Human Integrator | task slicing, dependency graph, contract freeze, review, merge order, decision log | task/decision metadata; approved integration changes | unilateral requirement/Feature ID change; fabricated approval | approved task card, conflict decisions, merged evidence |
| Agent A — Repository/Foundation | root SDK/build/package/test/pipeline primitives and BuildingBlocks | root files; `eng/**`; `pipelines/**`; BuildingBlocks; architecture/contract bootstrap tests | Feature implementation/UI/provider; master FINAL documents | reproducible build, dependency record, pipeline evidence |
| Agent B — PDF Domain/Application/Engine | Pdf contracts/domain/application, provider adapter after PoC | `src/Modules/Pdf/**`; matching Unit/Integration tests | Desktop UI; root package versions; server endpoint; unrelated module | contract diff, test/corpus evidence, limitation manifest |
| Agent C — Desktop/UI/Workspace | WinUI shell, picker/drop/state/Save As, Workspace metadata | Desktop host; Workspace/FileProductivity slices; matching tests | PDF business/provider rules; pipeline/root packages; Web/Mobile | UX state map, screenshots free of sensitive data, UI/integration evidence |
| Agent D — Tests/Security/Pipeline | cross-cutting test completeness, negative/security scans, CI quality gates | test projects; approved `eng/test`, `eng/security`, pipeline task paths | changing expected behavior to pass; app scope outside defect task | test trace, scan reports, waiver/defect list |

Rules:

1. one task card, branch and worktree per active lane; allowed paths are exclusive unless Agent 0 records a shared-file window;
2. root files, canonical contracts and registries have a single writer at a time;
3. agents consume contract snapshots; proposed contract changes stop downstream work, identify impacted tests and require Integrator approval;
4. every handoff includes Task/Feature/Requirement IDs, changed paths, tests run, known gaps, security/performance evidence and next dependency;
5. no agent may delete/skip a critical test, loosen an assertion, add a server route, approve a package/provider, or change expected behavior solely to obtain green status;
6. merge order is foundation → contracts → domain/application → adapters/UI → cross-cutting tests/docs; failed downstream evidence reopens the owning task;
7. parallel work is capped by independent paths and reviewer capacity, not by an invented agent count.

---

# 8. Git/worktree strategy

## 8.1 Branch model

- protected integration branch: `main`;
- short-lived implementation branch: `task/<task-id>-<short-name>`;
- PoC branch: `poc/<poc-id>-<short-name>`;
- documentation-only implementation evidence: `docs/<task-id>-<short-name>`;
- no long-lived Git Flow branches unless later justified by the approved release model.

Each branch gets its own worktree. The worktree registry records task, owner/lane, base revision, allowed paths and dependency PRs. Generated artifacts, local databases, secrets, corpus payloads and tool caches are ignored or stored in approved evidence storage—not committed accidentally.

## 8.2 Merge gates

| Gate | Required condition |
|---|---|
| Scope | task card complete; only allowed paths; Feature/Requirement trace present |
| Build | clean restore/build on approved SDK/package sources |
| Unit | all relevant MSTest/MTP tests pass; non-zero discovery; no ignored critical test |
| Architecture/Contract | reference rules, registry uniqueness and versioned contract checks pass |
| Security | secret/dependency/static/source checks pass or approved, time-bounded waiver exists |
| Behavior | acceptance criteria and negative/recovery cases have evidence |
| Review | human review completed; unresolved requirement/test conflict blocks merge |

Do not rebase/force-push another lane's branch, merge incomplete shared contracts, or resolve conflicts by taking an entire side. If a FINAL source or task scope conflicts, stop and report the exact IDs/lines to the Integrator. Dependency PRs merge before dependent PRs; dependent worktrees then update and rerun their full relevant gates.

---

# 9. Pipeline bootstrap

The first Azure DevOps pipeline is PR validation only. It produces no Production deployment and consumes no Production secret.

```text
Restore
  ↓
Build
  ↓
Unit
  ↓
Architecture
  ↓
Contract
  ↓
Dependency Audit
  ↓
Secret Scan
  ↓
Static Analysis
  ↓
Artifact / Report
```

| Stage | Initial responsibility | Failure rule/evidence |
|---|---|---|
| Restore | approved sources, locked central versions, deterministic dependency graph | unexpected/unapproved source or unresolved version fails |
| Build | solution on exact SDK patch; analyzers/warnings policy | compile/analyzer policy failure blocks |
| Unit | fast deterministic MSTest/MTP suites | zero tests, failed tests or ignored critical test blocks |
| Architecture | module/reference/path/forbidden dependency checks | violation blocks |
| Contract | registry/schema/version/backward-compatibility checks | duplicate/missing canonical mapping or incompatible change blocks |
| Dependency Audit | vulnerability/license/package admission evidence | threshold follows approved policy; unknown/unwaived finding blocks according to that policy |
| Secret Scan | repository/history/change scan with canary self-test | detected credential/secret blocks; rotate if real exposure |
| Static Analysis | unsafe code/source/config rules | critical unwaived finding blocks |
| Artifact/Report | signed/checksummed when policy is known; test/scan/trace manifests | artifact incomplete, mutable, or containing sensitive payload is rejected |

Initial CI favors fast PR feedback. Real-provider, large-file, packaging and slow integration suites run in an explicitly separate governed lane before release gates; they may not be relabeled Unit. Cache keys include SDK and dependency locks, and cache use cannot bypass restore verification. Exact commands, numerical thresholds, artifact retention and agent image remain `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` until the bootstrap environment is approved.

---

# 10. Server-budget guard

## 10.1 Local-first decision

`FEAT-PDF-005` is local-only. It must not create an API endpoint, SignalR event, upload, server queue, server Worker load, or server document storage merely because the UI contains Merge PDF. Desktop and Assistant invoke the same local application contract; a Local Worker, if measured as necessary, stays on the device.

| Capability | Initial placement | Server dependency | Guard |
|---|---|---|---|
| Merge processing/input/output/temp | client/local Worker | none | no upload; opaque local refs; source-safe atomic publication |
| Feature registry and policy snapshot | packaged local + approved synchronized metadata later | optional for revision/update | cached/versioned; server final check only for server action |
| Authentication/account/permission administration | server when those modules are implemented | required for authoritative online state | no client authority; not needed to transform an already-authorized local file during allowed offline session |
| User History/Activity | local metadata first; optional governed sync | optional/required by later policy | no document content; Audit remains separate |
| Audit | safe local intent/queue and later durable server policy | depends on event level/action | critical event not silently dropped; no document/password/OCR/signature payload |
| Issue/support attachment | explicit user send only | required for central issue workflow | secure object storage/access; screenshot opt-in |
| Secured Files | payload in secure object/file storage; metadata SQL | required only when user invokes that service | max three latest versions; no accidental Merge coupling |
| SignalR | server job/activity notification only | required only for server jobs | never source of truth; reconnect uses REST |

Every task declares `ProcessingMode`, `ServerDependency`, `StorageImpact`, `NetworkImpact` and `ConcurrencyRisk`. A task proposing `OPTIONAL_SERVER` or `REQUIRED_SERVER` must show an existing requirement, data-boundary review, API/authorization contract and measured budget; “tool exists in UI” is not evidence.

Server-heavy work introduced later requires queue/backpressure, per-user/global concurrency policy, timeout/cancel, payload/temp limits, retry/idempotency, resource telemetry and capacity evidence. Values remain `TARGET_TO_BE_BASELINED`/`POLICY_CONFIGURABLE`; no artificial CPU/RAM/file/page/concurrency limits are asserted here. Hostinger KVM2 remains a temporary non-Production Dev/QA/Pilot possibility only and is not accepted as Production topology.

---

# 11. Milestones

Milestones are evidence gates, not dates or percentage-complete claims.

| Milestone | Outcome | Dependencies | Exit evidence | Does not imply |
|---|---|---|---|---|
| M0 — Documentation audited | canonical D01–D19/Master v4.2 input identified | current baseline | manifest, version/hash list, conflict/open-item links | coding authority or acceptance |
| M1 — Repository builds | official repo restores/builds minimal active solution | BOOT-001..006 | clean worktree build and non-zero test discovery | feature functionality |
| M2 — Pipeline green | PR gates execute and report | M1; BOOT-007..008 | green reference PR, negative/canary proof, reports | QA/release approval |
| M3 — PDF Engine PoC approved | scoped engine decision for Merge/basic PDF | M2; `POC-PDF-001`; corpus/license/security | approved status + limitation/resource manifest | permanent vendor or all-PDF support |
| M4 — Merge alpha | usable local happy path behind approved contracts | M2; Merge foundation; M3 for real provider | picker/order/merge/validated Save As demo + tests | recovery/security/UAT completion |
| M5 — Merge vertical slice done | first slice satisfies canonical behavior, negative/recovery/security gates | M4; `TASK-MRG-001..026` applicable work | Unit/integration/system/trace/doc evidence | Production readiness |
| M6 — Core local | local PDF organize/optimize subset complete | M5; approved PDF capability | relevant Core tests and limitation matrix | conversion/intelligence/mobile parity |
| M7 — Conversion/OCR | conversion, OCR and scan scope implemented to approved PoCs | M2; relevant PoCs/devices | fidelity/accuracy/security/performance evidence | every document format fidelity |
| M8 — Security/Intelligence | edit/forms/security and approved intelligence Core scope complete | M6–M7; security/AI PoCs | negative/data-boundary/quality evidence | PKI signature or public AI |
| M9 — QA/UAT candidate | deployable Release Candidate enters governed QA/UAT | M5–M8; environments/test data | build/QA summary, trace, known issues, security/performance evidence | stakeholder acceptance |
| M10 — Pilot candidate | approved Pilot package, runbook and rollback readiness | M9; Pilot confirmations | Pilot entry approval/evidence; support/monitoring/rollback prepared | Production approval |
| M11 — Production readiness | all Production confirmation, control and acceptance gates satisfied | M10; D17–D19 open items resolved | approval records, topology/security/restore/DR/KT/handover evidence | project closure until actual acceptance completes |

---

# 12. Risk register

`Owner` below is a required accountable function; named individuals remain to be confirmed. Likelihood uses qualitative planning labels until PoC/implementation evidence exists.

| Risk | Impact | Likelihood | Mitigation | Contingency | Owner | Phase |
|---|---|---|---|---|---|---|
| Vendor/license incompatibility | engine cannot be distributed or used in target environment | `POSSIBLE` | explicit package/license admission; PoC records exact version/use | evaluate another contract-compatible candidate; defer governed scope | Dependency/Legal owner `TBC` | 0–7 |
| PDF fidelity limitations | corrupt or materially changed output | `LIKELY` for complex corpus | independent result oracle, rendering/structure corpus, limitation matrix | safe failure; alternative adapter/limited supported subset with approval | PDF technical owner `TBC` | 2–6 |
| OCR accuracy/resource failure | wrong searchable/text result or unusable runtime | `POSSIBLE` | language/quality corpus, confidence rules, rotation/noise tests | mark low confidence; allow review/retry; alternative approved engine | OCR owner `TBC` | 2,5 |
| Office conversion runtime/fidelity | feature unusable headlessly or loses layout | `POSSIBLE` | split direction PoCs; runtime/license/font evidence | approved limitation, alternative engine or deferred capability | Conversion owner `TBC` | 2,5 |
| WinUI packaging/signing | Desktop cannot install/update reliably | `POSSIBLE` | early packaging spike after scaffold; supported OS matrix | governed installer alternative within locked architecture; block Pilot | Desktop/release owner `TBC` | 1,8–10 |
| MAUI/platform divergence | shared contracts behave differently or unsupported platform grows scope | `POSSIBLE` | curated capability matrix; contract tests; defer parity claims | limit platform exposure; no forced Core parity | Mobile owner `TBC` | 8–10 |
| Local Worker IPC/isolation failure | UI hang, unauthorized job, unrecoverable crash | `POSSIBLE` | versioned/whitelisted job contracts, process isolation, crash/reconnect tests | immediate mode for measured safe cases; disable incompatible background capability | Processing owner `TBC` | 1,3–9 |
| File-security boundary failure | source overwrite, path escape or sensitive leak | `POSSIBLE` | managed roots, opaque refs, atomic publication, canary/negative tests | fail closed, quarantine artifact, incident evidence and remediation | Security owner `TBC` | 1–10 |
| Large files/resource exhaustion | crash, disk exhaustion or degraded device/server | `LIKELY` without baseline | streaming, preflight, bounded concurrency, temp cleanup, measured corpus | reject safely with actionable capacity error; resume/retry only when safe | Performance owner `TBC` | 2–9 |
| SQLite schema/migration corruption | local state/history/recovery lost | `POSSIBLE` | versioned migrations, backups where applicable, crash/upgrade tests | restore/rebuild non-authoritative projections; preserve user output | Workspace/data owner `TBC` | 1,3–10 |
| API/contract incompatibility | clients/workers cannot interoperate after change | `POSSIBLE` | versioned contracts, compatibility tests, staged deprecation | reject incompatible request safely; rollback contract/artifact | API owner `TBC` | 1,8–10 |
| Solo-developer capacity bottleneck | review, PoC and implementation queues stall | `LIKELY` | critical-path sequencing, small tasks, evidence automation, WIP limit | reduce parallel scope; defer non-core/secondary features through governance | Project owner `TBC` | all |
| Codex merge/scope conflicts | wrong baseline, overwritten work or misleading green | `POSSIBLE` | path ownership, worktrees, task metadata, human review/stop protocol | close/re-slice conflicting task; rebuild from approved base | Integrator `TBC` | all |
| Overengineering foundation | schedule consumed by unused abstractions/projects | `POSSIBLE` | create only active projects; first vertical slice tests abstractions | remove unused proposal before public contract; return to minimum slice | Architect/Integrator `TBC` | 0–3 |
| Premature server processing | cost, exposure and operations burden without requirement | `POSSIBLE` | server-budget declaration/gate; local-only Merge; API evidence required | reject endpoint/upload; re-plan as local or approved later phase | Architecture/security owner `TBC` | 1–8 |
| Incomplete UI/UX/accessibility | Core behavior technically green but unusable/unaccepted | `POSSIBLE` | early task flows, keyboard/focus/localization tests, evidence review | block UAT candidate; fix owning slice before broad feature expansion | UX/UAT owner `TBC` | 3–9 |

---

# 13. Pertamina confirmation impact

The labels below classify impact; they do not claim a Pertamina standard or approval that is not in the baseline.

| Impact class | Confirmation/evidence required | Current effect |
|---|---|---|
| `BLOCKS CODING` | official repository/location; access; human Integrator/repository/build/security owners; exact .NET 10 SDK patch; approved NuGet feeds/package admission; branch/pipeline authority | Phase 0 cannot produce an authoritative implementation repository until confirmed |
| `BLOCKS POC` | PoC owner/reviewer; corpus/test-data authority and secure location; package/runtime/license allowance; external-process/container/device permission; quality oracle; evidence storage | each affected PoC remains planning-only; PDF real adapter remains blocked |
| `BLOCKS QA` | Dev/QA environment and identity; approved test accounts/data; security scan policy/tooling; artifact/retention/access; device/OS/browser matrix; defect/reviewer ownership | no QA Release Candidate or official result claim |
| `BLOCKS PILOT` | Pilot scope/users/environment; package signing/distribution; operational monitoring/support/escalation; rollback/backup/restore; known-risk acceptance | M10 cannot pass |
| `BLOCKS PRODUCTION` | Production topology/network/domain/TLS/SSO; secret/key ownership/rotation; backup/RPO/RTO/DR; monitoring/SLA/on-call; VA/PT and release approvals; CMDB/Service Desk; retention/deletion; Production acceptance/KT/handover owners | M11 and actual handover remain blocked |
| `DOES_NOT_BLOCK CURRENT PHASE` | final Production topology, CMDB/Service Desk IDs, Production RPO/RTO/SLA, named Production support roster, mobile-store distribution, Production acceptance signatures | these must remain visible open items but do not prevent planning, repo bootstrap, shared foundation or locally isolated PoCs |

Record unresolved items using the matching `REQUIRES_PERTAMINA_*` marker from D17–D19. A confirmation changes only the affected gate; it does not retroactively mark technical evidence PASS.

---

# 14. Next-phase entry criteria

## 14.1 Current Phase 0 authorization checklist

| Criterion | Current status | Required evidence |
|---|---|---|
| D01–D19 FINAL and Master v4.2 are frozen inputs | `READY_WITH_CONDITIONS` | version/hash manifest and Integrator acknowledgment |
| official Git repository and access exist | `BLOCKED` | repository reference/access test |
| human Integrator plus repo/build/security owners are assigned | `BLOCKED` | ownership record; no invented names |
| exact .NET 10 SDK patch/agent support is approved | `BLOCKED` | approved value and clean-agent availability |
| NuGet source/package admission process is approved | `BLOCKED` | source policy and first dependency record |
| branch protection/PR/pipeline authority is available | `BLOCKED` | repository settings/authorized service connection evidence |
| initial task cards and allowed paths are approved | `READY_WITH_CONDITIONS` | `BOOT-001..008`, then `TASK-MRG-001` review |
| no Production secret/data is needed | `READY` | local/synthetic bootstrap boundary |

Current decision: **GO for planning and confirmation collection; NO-GO for formal application coding.** The first authorized action is `BOOT-001`. `TASK-MRG-001` begins only after `BOOT-001..002` establish repository/toolchain authority.

## 14.2 Phase 1 entry

M1 must show clean restore/build, meaningful MSTest/MTP discovery, reference graph, baseline manifest, package locks and owners. M2 PR gates may be built during Phase 1 but Phase 1 exits only when architecture/contract/security primitives have executable tests and no unrelated empty module fleet was created.

## 14.3 PoC and Merge entry

- PoC: isolated branch/worktree, approved candidate/package and license review, safe corpus authority, oracle, resource capture and stop rules.
- Merge contract/domain/UI work: M2 pipeline green, canonical IDs/requirements indexed, storage/processing/security abstractions approved, task paths exclusive.
- Merge real adapter: `POC-PDF-001` is `APPROVED_ENGINE` or specifically `APPROVED_WITH_LIMITATION`, dependency admitted, limitations mapped to tests/UI.
- Merge vertical-slice exit: applicable `TASK-MRG-001..026` evidence, `UT-MERGE-001..050`, `TP-MRG-001..036`, UAT package, source safety, local-only server guard and traceability pass.

## 14.4 Later QA/Pilot/Production entry

QA requires an immutable Release Candidate, environment/accounts/data, pass summary, known issues, traceability and security/performance evidence. Pilot and Production then use the confirmation/evidence gates in §13 and D17–D19; no earlier technical green may bypass stakeholder acceptance.

---

# 15. Risks/conflicts

1. D02 canonical Merge references use `FR-FILE-001..005` and `FR-FILE-010..015`; `FR-FILE-006..009` are absent. Backlog must never normalize the gap into new IDs.
2. D04 conceptual tree does not name a `ContentIntelligence` module or exact Local Worker project, while D18 locks both as logical boundaries. The tree in §4 is a proposal; Integrator must confirm exact names/paths before they become contract.
3. D19 states the official repository and .NET solution/source are absent; this workspace is not a Git repository. Existing planning files cannot be treated as implementation history.
4. Exact .NET SDK patch, package feeds, dependency versions and security tool thresholds are not approved. `global.json` and pipeline commands must not invent them.
5. No PDF engine/provider/license decision exists. Fake/contract work can proceed after foundation; provider-complete Merge cannot.
6. D06/D18 contain server/API gaps for administrative and integration surfaces. They do not justify a Merge processing endpoint and do not block its local slice.
7. D11 mentions conceptual local/CLI test surfaces. A CLI host must not be added unless an authorized test/runtime requirement proves it necessary.
8. Production confirmations in D17–D19 remain material but are not Phase 0 design blockers. Conversely, their non-blocking status now is not acceptance to ignore them later.
9. The 33 Core Tool Cards guide implementation phases; the other canonical Features remain mapped/traceable. This report intentionally does not expand all 550 Features into coding tasks.
10. Phase 2 PoCs run parallel to foundation/contract work, but each real adapter and claimed capability remains gated by its PoC. This balances progress without pretending provider feasibility.
11. Mobile/Web exposure is curated after Core contracts; forcing platform parity early conflicts with Local-First and solo-developer capacity.
12. No master/FINAL document was changed. Suggested text below is an Integrator input, not an automatic baseline amendment.

---

# 16. Suggested Roadmap content

> **Implementation readiness decision.** Document 01–19 FINAL and Master Project Summary v4.2 are sufficient for implementation planning, but formal coding is not authorized until the official Git repository, access, human Integrator/owners, exact .NET 10 SDK patch, approved package sources and PR pipeline authority are confirmed. Phase 0 creates only the root build/test/pipeline baseline and active minimum projects; it does not scaffold every module or claim feature readiness.
>
> **Implementation sequence.** Execute Phase 0 Repository Bootstrap, Phase 1 Shared Foundation, Phase 2 parallel PoCs, Phase 3 Merge PDF vertical slice, Phase 4 local Core PDF operations, Phase 5 conversion/OCR, Phase 6 edit/forms/security, Phase 7 approved local/internal intelligence, Phase 8 curated Web/Mobile capability exposure, Phase 9 hardening/UAT/Pilot and Phase 10 Production preparation. Each phase uses explicit entry/exit, evidence and security/performance/recovery gates; no date or capacity is assumed before measurement and owner confirmation.
>
> **First vertical slice.** `FEAT-PDF-005 — Merge PDF` is local-only and uses the same application contract for File, Tool and Assistant entry. It must support validated ordered inputs, reorder/remove/Undo, capability-aware local execution, safe progress/cancel/recovery, independent result validation, atomic Save As, source protection, SQLite metadata/history separation, safe correlation/logging and full traceability. It must not create an API, SignalR channel, upload or server job. The real PDF adapter is gated by `POC-PDF-001`; contracts/domain/UI can use a fake boundary after foundation gates.
>
> **PoC wave.** Run isolated evidence-producing PoCs for PDF Engine, Office→PDF, PDF→Office, OCR, existing PDF text edit, HTML→PDF, PDF/A and approved local/internal Content Intelligence. Physical scanner and local background-removal PoCs follow when their device/model policy and schedule exist. Outcomes are `APPROVED_ENGINE`, `APPROVED_WITH_LIMITATION`, `BLOCKED_ALTERNATIVE_REQUIRED` or `DEFERRED_BY_GOVERNED_DECISION`; PoCs do not select a permanent vendor by implication.
>
> **Delivery model.** Use a protected `main`, short-lived task/PoC branches, one worktree per lane, exclusive path ownership and a human Integrator. Foundation, PDF, Desktop/Workspace and Test/Security/Pipeline lanes may run in parallel only where contracts and paths are independent. PR gates run Restore, Build, Unit, Architecture, Contract, Dependency Audit, Secret Scan, Static Analysis and safe Artifact/Report generation. Test or requirement conflicts stop the task; tests may not be removed or weakened to obtain green status.
>
> **Server and release guard.** Local Features remain local unless an explicit requirement/data-boundary decision proves server dependency. Server-heavy work must carry measured queue, concurrency, timeout, storage, telemetry and cost evidence. Hostinger KVM2 is temporary non-Production only. M0–M11 are evidence milestones; QA, Pilot, Production and actual handover remain blocked until their Pertamina confirmations and implementation evidence are complete.

## Integrator handoff

1. resolve §14.1 and create/authorize `BOOT-001`;
2. approve the proposed root/project names, especially `ContentIntelligence` and `Pertamina.Oto.LocalWorker` placement;
3. authorize the eight PoC cards and their corpus/license/security reviewers;
4. import `TASK-MRG-001..026` into the chosen work-tracking system without changing canonical IDs;
5. keep D19 open items/confirmation gaps linked to M9–M11 rather than marking them complete from planning evidence.

---

**Final status:** `IMPLEMENTATION_PLANNING_READY`; `FORMAL_CODING_BLOCKED_PENDING_PHASE_0_CONFIRMATIONS`; `PRODUCTION_READINESS_NOT_CLAIMED`.
