# AGENT D — Test Architecture, Quality Gate & Codex Readiness Report

> **Task ID:** `DOC11-AGENT-D`  
> **Role:** Senior Test Architect / Senior C#/.NET Test Engineer / Technical Project Manager / Software Architecture Reviewer / Codex Workflow Designer  
> **Date:** 27 Agustus 2026  
> **Authoritative baseline:** Documents 01–10 FINAL; `MASTER_PROJECT_SUMMARY.md` v3.3  
> **Locked test tooling:** MSTest + Microsoft.Testing.Platform (MTP)  
> **Output status:** Specialist recommendation for controlled integration into Document 11; no production code and no edit to the Document 11 master

## Source and repository audit

- Documents 01–10 FINAL are present and were audited, with primary emphasis on Document 05 `TECH-DEC-004`, `TECH-DEC-005`, `TECH-DEC-013`, and `TECH-DEC-015`; Document 02 §§3, 12, 14, 16–18; Document 04 §§6, 29–31; Document 06 §22; Document 08 §§25–29 and 36; Document 09 §48; and Document 10 §54.
- The current workspace contains documentation only. No `.sln`, `.slnx`, `.csproj`, `.cs`, `global.json`, `Directory.Packages.props`, `Directory.Build.props`, MTP configuration, or `tests/` source tree exists yet.
- Consequently, the structure in this report is a **target architecture derived from the locked conceptual solution**, not a claim about code that already exists. When the repository is bootstrapped, project names and references must be reconciled against the actual source assemblies through a scoped foundation task.
- MSTest and MTP are fixed. This report does not replace them, introduce a paid test SaaS, choose a mocking/coverage vendor, create the Document 12 Test Plan, or invent numeric Pertamina thresholds.

Primary traceability:

```text
TECH-DEC-004  Structured Monorepo + module scope + task worktree
TECH-DEC-005  Central Package Management + explicit dependency approval
TECH-DEC-013  MSTest + Microsoft.Testing.Platform
TECH-DEC-015  Scoped Codex task + human merge gate
IR-DEVOPS-004 Restore → Build → Unit Test → Security Scan → Artifact
BR-GOV-003    Requirement → Feature → Code → Unit Test → Test Case → UAT → Release
DEC-REQ-021   Definition of Done 8 layers
SD §30        Architecture and contract checks beyond unit tests
SEC §27       Layered security test pyramid
ROLE §48      Authorization scenario/layer matrix
AUD §54       Audit functional/security/recovery/performance acceptance
```

The current official Microsoft guidance confirms that MTP supports `dotnet test`, MTP configuration is version-sensitive, and MSTest parallelization intent should be explicit. These are non-authoritative implementation references; the repository-pinned SDK/package version remains controlling:

- [MTP `dotnet test` command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test-mtp)
- [MTP configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-config)
- [MSTest execution and parallelization](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-writing-tests-controlling-execution)

---

## 1. Test architecture

### 1.1 Objective and governing model

The Unit Test architecture shall prove fast, deterministic, observable behavior inside Domain, Application, and pure adapter/policy code without pretending to prove infrastructure, UI, engine fidelity, or enterprise integration.

```text
Requirement ID + canonical Feature ID
                ↓
       public behavior/invariant
                ↓
MSTest test in owning module assembly
                ↓
MTP local and CI execution
                ↓
traceable result + quality evidence
                ↓
8-layer Definition of Done assessment
```

A Unit Test asks: given explicit business/security/capability facts, does the real Domain/Application behavior return the correct decision, state, error, result, and typed side-effect intent? It shall not assert private implementation steps unless the interaction itself is a locked boundary requirement.

### 1.2 Test layers and boundaries

| Suite | Purpose | Allowed dependencies | Not proven by this suite |
|---|---|---|---|
| Domain Unit | Value objects, invariants, policies, state transitions, finality, source protection | Production Domain assembly and immutable test data | Persistence, transport, engine output, UI |
| Application Unit | Use-case orchestration, validation sequence, permission/capability decision consumption, resolver behavior, typed intents | Production Application + Domain; controlled boundary fakes | Real DB/filesystem/network/provider/IPC |
| Pure adapter/host Unit | DTO mapping, safe error mapping, presenter/view-model/state reducer, protocol-independent validation | Owning adapter/host assembly; no live framework host | Middleware order, serializer/wire behavior, native UI integration |
| Architecture/Contract | Dependency direction, Feature registry, OpenAPI/event/IPC/schema compatibility | Compiled solution/contracts and approved tooling | Business acceptance or real infrastructure |
| Integration/Component | EF/SQL/SQLite, filesystem, object storage adapter, HTTP pipeline, Worker IPC, engine/provider sandbox | Controlled real component/test environment | Full user journey or Production scale |
| E2E/Security/Performance | Cross-boundary behavior, abuse, platform flows, resource budgets | Purpose-built environment | Replaced neither by Unit nor Document 11 |

Slow integration, component, E2E, malicious-file, PoC/fidelity, VA/PT, and performance tests remain separate from the Unit suite. A passing fake PDF engine proves the coordinator contract only; it does not prove PDF correctness or security.

### 1.3 Reference and ownership rules

- Each Unit Test project mirrors one production assembly or a tightly cohesive assembly pair owned by one module.
- A module test project may reference its own production assembly and approved BuildingBlocks contracts only. It must not reference another module's Infrastructure internals.
- Domain Unit projects must not acquire UI, HTTP, EF Core, SQL, file engine, or provider dependencies merely for testing.
- Application Unit projects may use small boundary fakes but execute the real application service/handler/validator/policy being tested.
- Shared TestKit code contains only stable test infrastructure. Module-specific builders and assertions stay with the owning module.
- Production code is not made public merely to ease testing. Prefer public behavior; `InternalsVisibleTo` is exceptional, assembly-scoped, reviewed, and cannot expose another module's internals.
- Test code follows the same central package approval, analyzer, secret-scanning, source-scanning, and review controls as production code.

### 1.4 Fast feedback target

Unit tests shall be small, in-memory, deterministic, independent, and suitable for frequent local execution and every pull request. Numeric duration budgets are not invented here. Initial pipeline measurements establish a baseline; a configurable budget is then approved without converting slow integration behavior into mocked Unit tests merely to meet it.

---

## 2. Test project structure

### 2.1 Target repository layout

The layout should preserve Document 04's top-level suites and mirror the eventual production assemblies below `tests/Unit`:

```text
tests/
├── Unit/
│   ├── BuildingBlocks/
│   │   ├── Pertamina.Oto.Contracts.UnitTests/
│   │   ├── Pertamina.Oto.Security.UnitTests/
│   │   ├── Pertamina.Oto.Observability.UnitTests/
│   │   ├── Pertamina.Oto.Storage.Abstractions.UnitTests/
│   │   └── Pertamina.Oto.Processing.Abstractions.UnitTests/
│   ├── Modules/
│   │   ├── Pdf/
│   │   ├── Conversion/
│   │   ├── Image/
│   │   ├── ScannerOcr/
│   │   ├── SignatureAnnotation/
│   │   ├── FileProductivity/
│   │   ├── Workspace/
│   │   ├── AccountAdministration/
│   │   ├── Audit/
│   │   ├── Support/
│   │   ├── Assistant/
│   │   └── ContentIntelligence/        (ownership name to reconcile at scaffold)
│   ├── Hosts/
│   │   ├── Api/
│   │   ├── Web/
│   │   ├── Worker/
│   │   ├── Desktop/
│   │   └── Mobile/
│   └── TestKit/
│       └── Pertamina.Oto.Testing.Unit/
├── Integration/
├── Architecture/
├── Contract/
├── E2E/
└── Performance/
```

`Authorization`, `Security`, and regression are test categories/layers that may span Unit, Integration, Contract, and E2E; this report does not create an extra top-level suite merely for naming convenience. Document 12 will define complete execution topology and numeric test-plan gates.

### 2.2 Project pattern inside each module

Use project boundaries that mirror actual production assembly boundaries, for example:

```text
tests/Unit/Modules/Pdf/
├── Pertamina.Oto.Modules.Pdf.Domain.UnitTests/
└── Pertamina.Oto.Modules.Pdf.Application.UnitTests/
```

If Domain and Application are compiled as one production project, one cohesive Unit Test project is sufficient. Do not create two test assemblies simply to satisfy a diagram. Conversely, do not create one repository-wide `UnitTests` assembly that bypasses module ownership.

Recommended internal folders are behavioral, not architectural substitutes:

```text
FeatureRegistry/
Validation/
StateTransitions/
Authorization/
Resolution/
Results/
Errors/
Evidence/
Regression/
Fixtures/
```

### 2.3 Mapping of the requested example

| Requested area | Target owner/alignment |
|---|---|
| BuildingBlocks | Mirrors the five BuildingBlocks in Document 04; registry/processing shared contracts stay with their actual source owner |
| Pdf | PDF domain/application, including reusable families and first `FEAT-PDF-005` vertical slice |
| Conversion | Office/PDF/image/text/web conversion coordination; engine fidelity remains Component/PoC |
| ScannerOcr | Scanner/OCR coordination, capability, result/error rules; real device/engine is not Unit |
| SignatureAnnotation | Visual signature/paraf non-PKI and annotation rules; certificate/PKI remains out of scope |
| Workspace | Local state, history projection, recovery, sync intent, secured-transfer orchestration |
| AccountAdministration | Authentication-support state, permission/policy administration, reauthentication decisions |
| Audit | Typed event mapping, exclusions, transaction-class decisions, retry/idempotency logic |
| Support | Issue state, consent, safe context, attachment/export decisions |
| Assistant | Canonical closed tool planning, per-step authorization parity, no prompt leakage |
| ContentIntelligence | Logical grouping for `FEAT-INTEL-001..002` and intelligence aspects of `FEAT-CONV-036`; final assembly ownership must match approved source architecture |
| Missing from example | `Image`, `FileProductivity`, and thin `Hosts` Unit suites are required when corresponding production assemblies contain testable logic |

Creating a `ContentIntelligence` test folder does not authorize a new production module. Document 04 lists `IContentIntelligenceProvider` while Document 07 names an `intelligence` data owner; the Integrator must lock the production assembly/owner name during scaffold.

### 2.4 TestKit guard

The Unit TestKit may provide deterministic clocks/IDs, common canaries, result assertions, disposable sandbox helpers, and small contract builders. It must not become:

- a second application framework;
- a global mutable fixture container;
- an imitation of EF Core/SQL Server/SignalR/HTTP;
- a backdoor reference across module internals;
- a location for business rules duplicated from production code.

---

## 3. Naming standard

### 3.1 Project, namespace, file, and class

| Item | Standard | Example |
|---|---|---|
| Project | `<ProductionAssembly>.UnitTests` | `Pertamina.Oto.Modules.Pdf.Application.UnitTests` |
| Namespace | Mirrors project + behavioral folder | `...Pdf.Application.UnitTests.Validation` |
| File/class | `<SubjectOrBehavior>Tests` | `MergePdfRequestValidatorTests` |
| Fixture/builder | `<Concept>Builder`, `<Concept>Fixture` | `CapabilitySnapshotBuilder` |
| Fake | `Fake<Boundary>` or behavior-specific name | `FakeClock`, `DeterministicPdfEngineFake` |
| Regression file | Subject file or defect-focused class under `Regression` | `SecuredFileAuthorizationRegressionTests` |

Avoid names such as `Test1`, `HappyPath`, `ShouldWork`, `Helper`, `ManagerTests`, or implementation-detail class names that do not express behavior.

### 3.2 Test-method format

Use one repository standard:

```text
<Operation>_When<Condition>_Should<ObservableOutcome>
```

Examples:

```text
Authorize_WhenResourceOwnerDiffers_ShouldDenyWithoutMetadata
ExecuteMerge_WhenOutputValidationFails_ShouldNotPublishFinalResult
ResolveEngine_WhenLocalCapabilityIsEligible_ShouldNotCallServer
CreateAuditIntent_WhenMetadataContainsPassword_ShouldRejectMapping
```

For state machines and decision tables, `Given<Context>_When<Event>_Then<Result>` is acceptable if used consistently within that suite. Requirement/Feature/defect IDs belong in test metadata and traceability artifacts, not overloaded into every method name.

### 3.3 MSTest metadata

Minimum logical metadata:

| Metadata | Values/purpose |
|---|---|
| `Layer` | `Unit` |
| `Module` | Canonical module owner |
| `Priority` | `P0`, `P1`, `P2`, or `P3` |
| `Requirement` | One or more approved Requirement IDs |
| `FeatureId` | Canonical Feature ID where feature-specific |
| `Risk` | `Security`, `Privacy`, `Recovery`, `DataIntegrity`, etc. where applicable |
| `Regression` | Defect/work-item ID for permanent regression |

Use supported MSTest category/property mechanisms from the centrally pinned version. Metadata enables filtering and traceability but never replaces a readable name or assertion.

### 3.4 Assertion naming and messages

- Assert stable codes, states, typed values, collections, and observable effects—not localized display text or private call sequence.
- Failure messages identify the scenario/field/code, not the full request, document text, password, token, signature, prompt, or security canary value.
- One test should normally prove one behavior reason. Multiple assertions are acceptable when they describe one coherent outcome/invariant.
- Data-driven row names must be deterministic and human-readable; never include sensitive rejected values.

---

## 4. Mock/fake standard

### 4.1 Boundary-only principle

The real Domain object, policy, validator, application handler/service, resolver, and state machine shall execute in Unit tests. Substitute only nondeterministic, slow, privileged, external, or infrastructure boundaries.

| Boundary | Preferred substitute | What it may model |
|---|---|---|
| File Storage / artifact publication | Small in-memory fake/spy | staged/final refs, conflicts, failure, cleanup intent; never claim real atomic filesystem proof |
| Clock/time source | Fixed and advanceable fake | expiry, backoff, freshness, retention decision |
| ID generator | Deterministic generator | stable valid IDs, collision/invalid cases |
| Network/connectivity | Explicit network-state fake | offline/online/degraded/change events; no real sockets |
| Engine/provider | Behavior fake | supported/unsupported, success, safe failure, cancel, invalid result, resource/capability facts |
| Permission/policy snapshot | Immutable stub/builder | explicit allow/deny/scope/revision/freshness; no wildcard default |
| External service/SSO/API | Port stub/fake | approved contract outcomes only; no real credentials or endpoints |
| Scheduler/delay | Controlled scheduler/fake | deterministic retry/cancellation sequence; no wall-clock sleep |
| Logger/evidence sink | Capturing fake with field guard | structured fields and forbidden-canary absence |

### 4.2 Terminology and use

- **Stub:** returns a fixed response; use for a single boundary fact.
- **Fake:** implements a small useful behavior/state model; prefer for storage, clock, job, engine, and evidence boundaries.
- **Spy:** records externally meaningful interaction such as “server path was not called” or “final artifact was not published.”
- **Mock:** expectation-driven substitute; use sparingly for a strict protocol boundary where call count/order is itself a requirement.

Interaction assertions are justified for requirements such as:

- local-only action makes no server-processing call;
- unauthorized request does not resolve storage/content;
- result is not published before output validation;
- critical mutation does not commit without durable Audit intent;
- Assistant invokes only the canonical application contract.

Do not mock every internal class in a handler chain, assert every method call, or duplicate the production algorithm inside the mock setup. Such tests lock implementation shape while failing to prove behavior.

### 4.3 Prohibited proof substitutions

- EF Core in-memory behavior does not prove SQL Server constraints, transactions, rowversion, indexes, grants, or concurrency.
- An in-memory dictionary does not prove encrypted SQLite, WAL/SHM, migrations, crash recovery, or disk-full behavior.
- A fake filesystem does not prove Windows path/reparse/TOCTOU/lock/atomic-replace behavior.
- A fake engine does not prove PDF/OCR/conversion fidelity, malicious-file resistance, CPU/RAM, or isolation.
- A fake HTTP/SignalR client does not prove serialization, middleware order, authentication configuration, proxy behavior, or reconnect transport.
- A unit sanitizer test does not prove that logs, traces, crash dumps, exports, or CI artifacts are clean end to end.

Those claims require the dedicated integration, component, contract, security, PoC, or performance suite.

### 4.4 Mocking package governance

No mocking library is selected by this report. If native handwritten fakes are insufficient, a package requires explicit purpose, license/security/maintenance review, central version pinning, and a scoped approval task under `TECH-DEC-005`. Codex may not add one silently.

---

## 5. Test-data strategy

### 5.1 Data classes

| Data class | Unit-test strategy |
|---|---|
| Domain values | Inline/minimal builders with valid defaults and single-property override |
| Canonical registries/schemas | Approved versioned snapshot/generated cases plus explicit semantic tests |
| Files/documents | Small synthetic structural tokens or minimal approved fixtures; real corpus belongs to component/security/PoC |
| Identity/permissions | Fictional opaque principals, explicit owner/scope/policy revisions; never real employee data or token strings |
| Sensitive-data canaries | Synthetic unique markers for password/token/secret/content/OCR/signature/prompt/screenshot/path; assertions must not print them |
| Localization | Resource key plus safe arguments; localized display rendering tested separately |
| Regression | Minimal synthetic fixture reproducing defect, tagged with work-item ID |

### 5.2 Builder and fixture rules

- Defaults are minimal, valid, local, owned, current-version, and deny privileged/server/AI/destructive behavior unless explicitly enabled.
- Each negative test changes one relevant dimension, so denial precedence is understandable.
- Builders are immutable or create a new object per call. They validate impossible combinations unless the test explicitly requests malformed input.
- Essential facts remain visible in the test; avoid an opaque `ValidEverything()` fixture.
- Do not reuse mutable aggregates, collections, cancellation sources, fake clocks, or event collectors across tests.
- `TestInitialize` may create fresh lightweight state. Assembly/Class initialization is limited to immutable read-only data and must not introduce order dependence.

### 5.3 Shared data sets

Recommended governed data sources include:

```text
CanonicalFeatureIds
Core33FeatureMappings
ValidAndInvalidJobTransitions
ValidationBoundaryCases
AuthorizationDecisionCases
OfflineGrantCases
EngineResolverCases
IdempotencyCases
AuditEventCatalogCases
ForbiddenFieldCanaries
ErrorMappingCases
```

The data source carries inputs and expected contract outcomes; it must not reimplement production decision logic. A catalog-wide completeness test supplements, but does not replace, hand-written semantic tests for critical cases.

### 5.4 File assets and golden results

- `tests/TestAssets` contains only synthetic, redistributable, classified, reviewed assets with provenance/license notes where relevant.
- Unit fixtures stay small. Large/malicious/fidelity corpora belong outside the Unit job and have controlled storage/access.
- Golden/snapshot output is used only when the contract is legitimately large and deterministic. Updating it requires a reviewed semantic diff and requirement justification; Codex cannot accept all changes automatically.
- Production documents, real issue screenshots, signatures, tokens, names, paths, and telemetry exports are prohibited.
- Test artifact retention must not turn a failed test into a sensitive-data store.

---

## 6. Determinism rules

### 6.1 Required controls

| Source of nondeterminism | Rule |
|---|---|
| Time | Inject UTC clock; fix/advance time explicitly; test boundary-before/equal/after; do not use `DateTime.Now/UtcNow` directly in test behavior |
| IDs | Inject deterministic opaque ID generator; no unassertable random GUIDs |
| Randomness | Prefer explicit cases. If generative/fuzz testing is later approved, record a reproducible seed and minimized failing input |
| Filesystem | Unit tests use a deterministic abstraction/fake; real filesystem tests own a unique temporary sandbox in a non-Unit suite |
| Network | Inject explicit `Offline`, `Online`, `Degraded`, and transition facts; no DNS/HTTP/network availability call |
| Async/retry | Controlled completion sources/scheduler/clock; no arbitrary `Task.Delay`, polling sleep, or timing-window assertion |
| Culture/time zone | Set explicitly per test when relevant and restore; assert stable resource/error keys rather than translated strings |
| Environment/config | Pass typed configuration/policy snapshots; no dependency on developer machine variables, registry, current directory, installed Office/engine, or user profile |
| Ordering | Assert defined order only; sort unordered inputs before comparison; never depend on test discovery/execution order |
| Platform | Capability facts for pure logic; actual Windows/Desktop/Mobile behavior belongs to platform-specific suites |

### 6.2 Deterministic filesystem behavior

At Unit level, the filesystem abstraction models only application-visible outcomes: exists/not found, readable/not permitted, stable identity changed, free-space decision, stage/publish/cleanup result, and conflict. It must use opaque references rather than machine paths. Real path normalization, reparse/symlink, locks, atomic rename, permissions, long paths, low disk, and crash residue require controlled component tests.

### 6.3 Deterministic network behavior

Connectivity is an input state, not inferred from a live ping. Tests explicitly drive:

```text
OFFLINE → eligible local action
OFFLINE → online-required action denied/queued by approved policy
ONLINE → server path eligible after authorization
ONLINE → disconnect before acceptance
RECONNECT → state/permission refresh before continuation
```

The fake records whether a prohibited network/server/provider call occurred, enabling Local-First and public-AI negative assertions.

### 6.4 Deterministic failure output

Test diagnostics shall show case identity, safe field name, expected/actual code, and correlation fixture ID only. Custom assertion serializers must not dump the entire command/event/document/provider response when a test fails.

---

## 7. Parallelism/isolation

### 7.1 Policy

- Every Unit Test assembly explicitly declares whether it is parallel-safe. Independent Unit assemblies should enable MSTest parallel execution; deliberate sequential assemblies must document why.
- Worker count and process/module parallelism are configurable by the repository-pinned MTP/CI configuration. No numeric worker count is declared as a Pertamina standard here.
- Prefer class-level parallelism initially if method-level fixtures have not been proven isolated. Method-level parallelism may be enabled when all fixtures are per-test and thread-safe.
- `DoNotParallelize` is a narrow exception for a proven global/platform constraint, not a default remedy for shared-state design.
- Never combine contradictory assembly-level parallel/serial declarations.

### 7.2 Isolation contract

Each test owns:

- object graph and mutable collections;
- fake clock, ID generator, cancellation source, scheduler, network state, logger/evidence collector;
- temporary directory/file handles when a non-Unit filesystem test is applicable;
- culture/configuration/state changes, restored in guaranteed cleanup;
- any local server/port/database namespace used by non-Unit suites.

Tests must not mutate shared static state, `Environment.CurrentDirectory`, process-wide environment variables, default culture, global dependency container, singleton cache, common file name, or shared database record without a purpose-built isolated harness.

### 7.3 Cleanup

- Prefer `IDisposable`/`IAsyncDisposable` and `try/finally` ownership; cleanup executes on assertion failure and cancellation.
- Cleanup is idempotent, bounded, and validates leaked resources where practical.
- A failed cleanup fails the test/job; it is not swallowed because leftover files/processes can make later tests flaky.
- Do not delete a broad or unresolved path. Temporary paths are unique, resolved, verified inside the test sandbox, and used only by the appropriate non-Unit suite.
- Unit tests shall not start real processes, listeners, databases, or external services.

### 7.4 Parallel-safety verification

Run the Unit suite under normal configured parallelism and, during foundation/hardening, under alternate worker counts and changed module order. A failure that occurs only in parallel is a real isolation defect. Disabling parallelism globally is not an acceptable permanent fix unless the architecture demonstrates an unavoidable constraint and the Integrator approves it.

---

## 8. Coverage strategy

### 8.1 Behavior-first coverage

Coverage priority is determined by requirements and risk:

1. security, privacy, authorization, source/data integrity, audit durability, destructive action, and recovery invariants;
2. changed production behavior and defect regression;
3. 33 Primary Tool shared execution contracts and first `FEAT-PDF-005` vertical slice;
4. normal branch/boundary/error behavior for each module;
5. rare defensive cases based on credible risk.

Required behavior matrices matter more than an aggregate line percentage. A suite with high line coverage but no wrong-owner, output-validation, queue-full, source-protection, or sensitive-data negative tests fails the quality gate.

### 8.2 Coverage evidence

Collect at least line and branch coverage by production assembly, module, changed code, and critical namespace where the centrally approved MTP-compatible collector supports it. Exclude only reviewed non-behavioral code such as generated artifacts or platform boilerplate that is proven elsewhere. Exclusion by filename pattern to hide untested business/security logic is prohibited.

Coverage report requirements:

- maps result to commit/build/test run;
- separates Unit from non-Unit coverage rather than merging claims invisibly;
- identifies changed executable lines/branches without tests;
- preserves module ownership and trend;
- contains no source secret, document content, path, token, prompt, or Production data;
- never marks a skipped/failed test as covered evidence.

### 8.3 Threshold policy

No numeric Pertamina coverage threshold exists in Documents 01–10, so none is invented. Recommended governance:

```text
baseline measured after representative foundation/vertical slice
→ project/module threshold configured centrally
→ critical behavior matrix remains mandatory regardless of percentage
→ changed-code regression is not allowed without explicit test/evidence disposition
→ threshold may ratchet upward through reviewed change, never silently downward
```

Exact percentages and permitted exceptions require Integrator/QA approval and alignment with Document 12. Coverage is a supporting signal, not proof of correctness, security, performance, recovery, cross-platform behavior, or UX.

### 8.4 Changed-code rule

Every behavioral code change must add/update a relevant Unit test or explicitly prove that the behavior is covered at a more appropriate layer. The task output records the Requirement ID, Feature ID, changed behavior, test name/layer, and result. “No test needed” requires a reviewed reason; it is not Codex's unilateral decision.

---

## 9. Quality gate

### 9.1 Pull-request gate

| Gate | Required evidence | Failure behavior |
|---|---|---|
| G0 Task scope | Task ID, Requirement ID, Feature ID, Allowed/Do-Not-Touch paths, acceptance, tests | Stop before change if missing/contradictory |
| G1 Controlled restore | Pinned SDK/package sources and approved dependency graph restore | Fail; no silent source/package/framework substitution |
| G2 Build/analyzers | Release build and required compiler/MSTest/security/static analyzers pass | Fail; warning waiver must be explicit and governed |
| G3 Unit execution | All affected and full fast Unit suites discovered and pass under MTP | Fail on test failure, crash, hang, or unexpected zero/missing tests |
| G4 Critical behavior | Applicable P0 security/business/recovery negative and positive tests exist and pass | Fail even if coverage percentage is high |
| G5 Change/traceability | Changed behavior maps Requirement → Feature → Code → Unit Test; coverage evidence/report created | Fail or require approved documented disposition |
| G6 Test integrity | No flaky/ignored critical test; no deleted/weakened assertion without authorized reason; test artifacts safe | Fail; retry cannot convert instability into health |
| G7 Review/handoff | Human review, resolved conflicts, downstream layer tests identified, 8-layer status honest | No merge/DONE claim until satisfied |

### 9.2 Merge and release meaning

Passing the Unit PR gate makes a change eligible for human integration; it does **not** prove feature DONE or Production readiness. Integration, Contract, Architecture, Authorization, E2E, Security, Performance/DoS, PoC/VA/PT, UAT, deployment, monitoring, and documentation gates remain applicable.

### 9.3 Ignored/skipped tests

- P0/P1 required tests may not be ignored/skipped/conditionally disabled to obtain a green result.
- Platform conditions are allowed only for genuinely inapplicable platform tests and must run in the targeted platform pipeline; they are not a Unit failure escape hatch.
- Any temporary noncritical exclusion requires a work item, owner, reason, impact, alternate evidence, expiration, and approval. The report must show the exclusion and cannot count it as passed.
- A zero-test or unexpectedly reduced-test run fails discovery validation.

### 9.4 Assertion/test-change review

Changes that delete a test, broaden an allow result, change expected status/error/permission, reduce a security assertion, update a large snapshot, or add an ignore/retry are treated as behavior-impacting. They require requirement evidence and explicit reviewer attention; they cannot be hidden inside an implementation refactor.

---

## 10. CI execution

### 10.1 Simple pipeline

```text
Controlled Restore
        ↓
Release Build + analyzers
        ↓
Fast MSTest Unit suite on MTP
        ↓
Required security/static/source/config checks
        ↓
Publish test result + coverage + safe artifact/report
```

This preserves `IR-DEVOPS-004`. Slow Integration/Component/Contract/E2E/Performance suites run as separate named jobs/stages with their own environment and gate; they are never relabeled as Unit and do not delay the normal local Unit loop unnecessarily.

### 10.2 Command model

The exact CLI options are locked only after the repository pins `.NET 10` SDK patch, MSTest/MTP packages/SDK, and one MTP configuration model. The intended command sequence is conceptually:

```text
dotnet restore <solution>
dotnet build <solution> --configuration Release --no-restore
dotnet test --solution <solution> --configuration Release --no-build <Unit filter/config/report options>
```

MTP supports solution/project/module selection, filtering, result reporting, coverage extensions, and bounded module parallelism, but flags differ by pinned version. A repository-owned documented wrapper may provide one local/CI entry point; it must transparently invoke the locked toolchain and not swallow failures.

### 10.3 Required CI controls

- Azure DevOps uses a clean controlled agent and the same central SDK/package/configuration as local execution.
- Restore, build, and test are distinct; later stages do not rebuild with different settings.
- Unit selection is metadata/path/project based and cannot accidentally include slow suites or omit a new Unit project.
- Discovery verifies expected modules/projects and detects an empty result. A centrally maintained manifest or solution filter may be used.
- Unit test timeout/hang diagnostics are bounded and sanitized. Dumps/logs are access-controlled and must not contain prohibited values.
- TRX/equivalent result, coverage, analyzer/security results, command/config version, commit, and run identifiers are published as safe evidence.
- Failed tests remain failed even if diagnostic rerun passes.
- Required scan/test tool failure is not converted to success; outage policy requires controlled disposition.
- Artifacts are retained per approved pipeline policy. This report sets no retention duration.

### 10.4 Execution frequency

| Trigger | Minimum Unit behavior |
|---|---|
| Developer local loop | Affected project/priority/filter, then relevant full module suite before handoff |
| Pull request | Full fast Unit suite plus required analyzers/security/static gates |
| Merge/main | Full Unit suite and downstream integration/contract jobs according to change impact |
| Release candidate | Full required test pyramid and 8-layer evidence; Unit is one input only |
| Dependency/security change | Full Unit suite plus dependency/static/security regression and affected compatibility tests |

Numeric maximum duration, worker count, timeout, retry, and report-retention values are configurable project policy and `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` where applicable.

---

## 11. Flaky-test prevention

### 11.1 Prevention matrix

| Root cause | Prevention | Prohibited shortcut |
|---|---|---|
| Real time/timer | Fake clock/scheduler; explicit boundary advancement | Longer `Task.Delay` or generous timing race |
| Randomness | Explicit cases or recorded reproducible seed | Unseeded random data and “run again” |
| Network/provider | Deterministic port fake; real network only in controlled non-Unit suite | Public/live endpoint in Unit tests |
| Shared files/path | Per-test owned sandbox outside Unit or in-memory abstraction | Fixed shared filename/current directory |
| Global static/singleton | Per-test object graph; reset-free design | Test ordering or global lock as permanent fix |
| Test order | Fully independent setup/cleanup | Ordered tests or dependency on prior test output |
| Parallel race | Immutable/per-test fixtures and thread-safe fakes | Disable all parallelism without root cause |
| Culture/time zone | Explicit value plus guaranteed restore | Depend on agent/user locale |
| Async fire-and-forget | Await observable completion; controlled cancellation | Sleep and hope background work completes |
| Resource leak | Deterministic disposal and leak assertion | Swallow cleanup failure |
| Platform/environment | Typed capability input or correct platform job | Developer-machine detection inside Unit logic |

### 11.2 Detection and handling

A test is flaky if unchanged code/input/config can alternate pass/fail. On detection:

```text
preserve original failing evidence
→ reproduce with fixed seed/time/order/parallelism/config
→ classify test defect vs product race vs environment defect
→ assign owner and priority
→ repair root cause
→ add/strengthen regression evidence
→ rerun affected + full Unit suite
```

Bounded reruns may be used diagnostically, but the initial failure keeps the CI run unstable/failed. Automatic retry must not transform a flaky test into a healthy green gate.

### 11.3 Quarantine policy

- P0/P1 tests block merge/release and cannot be quarantined as green.
- A temporary P2/P3 isolation, if approved, requires a defect, owner, reason, impact, expiry, and visible failing status/alternate evidence; it is not counted as pass or coverage.
- Repeated quarantine or global serial execution is an architecture smell requiring Integrator review.
- `Ignore`, conditional skip, retry, timeout increase, assertion weakening, and snapshot rewrite are reviewed as quality-impact changes.

---

## 12. Codex testing rules

### 12.1 Mandatory task contract

Every coding task assisted by Codex must state:

```text
TASK ID
REQUIREMENT ID(s)
FEATURE ID(s)
OBJECTIVE
ALLOWED PATHS
DO-NOT-TOUCH PATHS / NON-SCOPE
DEPENDENCIES AND LOCKED DECISIONS
ACCEPTANCE CRITERIA
TESTS REQUIRED (layer + priority + command)
VALIDATION / OUTPUT EVIDENCE
```

This extends the user's minimum fields without changing `TECH-DEC-015`. A dedicated short-lived branch/worktree and human merge gate remain mandatory.

### 12.2 Codex execution contract

Before editing, Codex shall:

1. read the named Requirement/Feature and affected architecture/security/API/database/audit contracts;
2. verify allowed paths, ownership, dependency/package authority, and current test state;
3. identify affected Unit tests and required non-Unit handoffs;
4. stop if a test encodes behavior that conflicts with an authoritative requirement and no approved resolution exists.

During implementation, Codex shall:

- modify only allowed paths and avoid unrelated refactoring;
- create/update relevant tests in the same task as behavioral code;
- test observable behavior and required negative cases;
- use approved dependencies and the locked MSTest/MTP platform;
- preserve canonical Feature/Requirement IDs, business rules, error/security boundaries, and test integrity;
- run the stated restore/build/test/security validation available in scope;
- report exact commands, pass/fail/skipped/discovered summary, changed files, coverage evidence where configured, and remaining risks.

### 12.3 Explicit prohibitions

Codex must not:

- delete or disable a failing test to make the build green;
- reduce assertions, broaden an allow result, or update expected behavior without source authority;
- mark a critical test ignored/skipped or rely on retry as success;
- accept/update snapshots or golden files without semantic review;
- add/replace/upgrade test packages or frameworks silently;
- expose private production internals solely for a brittle test;
- mock away the behavior under test or claim a fake proves infrastructure/engine fidelity;
- invent a coverage threshold, Pertamina standard, Feature ID, Requirement ID, error code, permission, or acceptance result;
- edit outside Allowed Paths, perform a mass refactor, or change shared foundation without explicit scope;
- claim DONE from build/Unit success alone.

### 12.4 Requirement–test conflict protocol

If an existing test and authoritative requirement disagree, Codex shall **STOP** that behavior-changing part and report:

```text
Task ID
Requirement ID / Feature ID and source location
Conflicting test name/path and current expectation
Observed production behavior, if safely verified
Security/business/compatibility impact
Smallest resolution options
Files left unchanged
Decision owner required
```

Codex does not choose whether requirement or test changes. After an explicit decision, the originating scoped task updates implementation/test/traceability together and records the reason.

### 12.5 Codex task quality checklist

```text
[ ] Required IDs and paths are valid
[ ] Baseline tests were inspected/run where possible
[ ] No unapproved dependency/framework/config change
[ ] New/changed behavior has prioritized tests
[ ] P0 security/business negative cases preserved
[ ] Unit suite passes without ignored critical tests
[ ] No flaky retry converted failure to success
[ ] Required downstream tests/handoffs identified
[ ] 8-layer DoD status reported honestly
[ ] Human reviewer receives concise evidence and blockers
```

---

## 13. Definition of Done integration

### 13.1 Unit Test contribution to the eight layers

| DoD layer | Unit Test contribution | Evidence Unit tests cannot replace |
|---|---|---|
| 1. Technical Correctness | Domain/application behavior, validation, error code, state/result, build and changed-code regression | Real infrastructure wiring, architecture/contract integration |
| 2. Business Process Correctness | Business invariants and alternate/error decision paths | End-to-end business outcome and owner acceptance |
| 3. UI/UX Acceptance | Pure presenter/view-model/state logic where applicable | Responsive/adaptive/native interaction, accessibility and user validation |
| 4. Security & Privacy | Permission/policy/ownership decisions, safe mapping, forbidden-field negative tests | Real auth, storage, HTTP/IPC, exporter/log pipeline, VA/PT |
| 5. Performance / Server Budget | Algorithmic bounds and no-server/no-whole-buffer decision intent where observable | CPU/RAM/storage/network/queue/concurrency benchmark |
| 6. Error & Recovery | Retry/resume/state/source-safety decision logic | Crash, disk, process, database, network, restore and real checkpoint recovery |
| 7. Cross-Platform Behavior | Capability matrix and fallback decision logic | Actual Web/Desktop/Mobile device/platform behavior |
| 8. Test & Documentation | Required Unit result, traceability, regression, updated technical notes | Integration/Contract/E2E/Test Plan/UAT/release/handover evidence |

### 13.2 Feature completion rule

A feature coding task is not DONE when any of these is true:

- a required Unit test fails, crashes, hangs, is flaky, or is not discovered;
- a critical business/security/recovery rule lacks positive and negative test evidence;
- changed behavior has no Requirement/Feature/test traceability;
- a failing critical test is ignored, retried into green, deleted, or weakened;
- required integration/contract/security/performance/cross-platform/UX/UAT evidence is missing;
- an unresolved requirement–test conflict exists;
- a required PoC or Pertamina confirmation remains open for the claimed release status.

The correct status is `NOT_DONE`, `BLOCKED`, or the governed limitation/feasibility status—not a technical “green” DONE claim.

### 13.3 Traceability evidence

For each changed/released Feature:

```text
Business Process
→ Requirement ID
→ canonical Feature ID
→ source module/assembly and code path
→ Unit test project/class/method + priority
→ required Integration/Contract/E2E/Security/Performance test case
→ UAT
→ pipeline/release artifact
```

Traceability may be generated from approved metadata, but automatic generation must fail on unknown/duplicate IDs and cannot invent a relationship.

---

## 14. Test priority

### 14.1 Priority model

| Priority | Definition | Examples | Gate treatment |
|---|---|---|---|
| **P0 — security/business critical** | Failure can cause unauthorized access, sensitive leakage, source/data corruption, incorrect destructive/privileged action, lost mandatory Audit, or false final success | wrong owner/permission; password/token/content exclusion; source unchanged; final redaction rule; max-three versions; critical Audit intent; Assistant no-bypass; public AI impossible | Must exist and pass when applicable; no ignore/quarantine-as-green; blocks merge/release |
| **P1 — Core Feature path** | Primary user/business path and shared contracts required for the 33 Core Tools | Merge order/output publication; canonical Feature mapping; local-first resolver; validation; Job accepted/terminal; retry/resume eligibility | Runs in every PR full Unit suite; blocks merge for affected feature |
| **P2 — normal behavior** | Common variants and normal alternate flows that do not meet P0/P1 | optional parameters, standard validation variants, normal empty display projection, safe supported fallback | Expected in normal Unit suite; defect must be resolved or explicitly governed |
| **P3 — edge/rare behavior** | Credible low-frequency defensive behavior with lower impact | unusual boundary combinations, legacy-compatible noncritical mapping, rare formatting case | Included according to risk/value; may run in full suite and remains real evidence |

Priority is not the same as MoSCoW Feature priority, test execution order, severity of a discovered defect, or layer. A P0 Unit test does not make the entire Feature P0, and a P3 test is not permission to ignore a valid failure.

### 14.2 Classification rules

- Assign the highest applicable consequence, not the easiest execution path.
- Security/privacy/authorization/source-integrity/destructive-action rules default to P0 unless documented otherwise.
- Core 33 happy paths and common failure paths are at least P1; their security invariants remain P0.
- Every fixed P0/P1 defect becomes a permanent regression test at the same or higher priority.
- Priority metadata is reviewed with Requirement/Feature ownership and cannot be downgraded by Codex solely to bypass a gate.
- All Unit tests should remain fast enough for frequent execution; priority filters optimize diagnosis, not substitute for the full PR Unit suite.

### 14.3 Minimum feature matrix

Each feature/application use case should identify:

```text
P0  unauthorized/sensitive/destructive/data-integrity/recovery invariant, if applicable
P1  primary success + canonical failure/validation path
P2  common option/alternate behavior
P3  credible rare boundary where risk/value justifies it
```

Do not manufacture a P0 case for a harmless formatter merely to fill a row. Conversely, do not classify every test P1; priority must remain decision-useful.

---

## 15. Risks/conflicts

| ID | Risk/conflict | Impact | Required disposition |
|---|---|---|---|
| `D11-D-001` | No source solution, production assembly, or test project exists in the current workspace | Exact project count, names, references, MTP config, and commands cannot be verified | Create a scoped repository-foundation task; reconcile this target tree to actual assemblies before coding features |
| `D11-D-002` | Document 04 module list does not explicitly name `ContentIntelligence`, while it defines `IContentIntelligenceProvider` and Document 07 has `intelligence` ownership | Creating a test project could silently imply a new production module | Integrator locks the owning assembly/name; until then treat it as a logical suite under the approved owner |
| `D11-D-003` | Exact .NET 10 SDK patch and MSTest/MTP package/configuration versions are not yet present | CLI flags/config/analyzer behavior and local/CI parity can drift | Pin in `global.json` and central package/config files through approved foundation task; validate current official compatibility |
| `D11-D-004` | No numeric Unit duration, coverage, changed-code, timeout, or worker threshold is approved | A hardcoded gate would invent a Pertamina/project standard | Measure the first representative baseline and approve configurable thresholds; Document 12 owns full numeric Test Plan |
| `D11-D-005` | Mocking, assertion, coverage, architecture-test, and report extensions are not selected | Silent package addition violates dependency governance | Prefer native MSTest/handwritten fakes first; each additional package requires explicit approval and central pinning |
| `D11-D-006` | Exact Azure DevOps branch, reviewer, bypass, test-artifact retention, and failed-tool policy require confirmation | Quality gate may be unenforceable or inconsistent across pipelines | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`; keep logical fail conditions locked |
| `D11-D-007` | Unit tests cannot prove PDF/OCR/Office/AI fidelity, malicious-file handling, real filesystem/DB/IPC/API behavior, or resource budgets | Over-mocking can create false assurance | Keep explicit component/integration/contract/security/PoC/performance handoffs; do not relabel fakes as proof |
| `D11-D-008` | Shared TestKit or one global test assembly can become a cross-module coupling path | Architectural erosion and brittle tests | Enforce reference/ownership architecture tests; shared TestKit limited to neutral deterministic primitives |
| `D11-D-009` | Too many tiny test projects can increase restore/build/discovery overhead; too few can violate ownership | Slow feedback or hidden coupling | Mirror actual production assemblies; combine/split only by measured ownership and build evidence, not the illustrative tree |
| `D11-D-010` | Canonical Feature/event/permission/schema catalogs span modules | Completeness tests may have unclear owner or duplicate behavior | Assign one governed registry/contract test owner plus module semantic tests; fail on unknown/duplicate/missing mappings |
| `D11-D-011` | Exact parameter schemas and some canonical error-code details remain implementation prerequisites for feature tasks | Tests may invent fields/codes or conflict across clients | Lock versioned contract/catalog before the affected implementation; STOP on conflict rather than guess |
| `D11-D-012` | Ordinary-L2 behavior when protected offline Audit queue is unavailable/full remains a confirmed Document 10 handoff | Unit expected outcome cannot be finalized for that branch | Test generic policy-selected outcomes; obtain required policy confirmation before release acceptance |
| `D11-D-013` | Flaky-test ownership, temporary P2/P3 isolation authority, and remediation time are not defined | Quarantine can become permanent or hide product races | Approve lightweight owner/expiry policy; P0/P1 always block and retries never create green status |
| `D11-D-014` | Coverage collection and test diagnostics may expose source paths or canary values | Test evidence becomes a leakage channel | Configure minimization/access/retention and run forbidden-canary checks over reports/logs/artifacts |
| `D11-D-015` | Host/platform logic may depend on WinUI/MAUI/web threading/runtime | Pure Unit configuration may fail or falsely skip targeted behavior | Keep pure presenters/state reducers in Unit; run real framework behavior on its approved platform job with visible conditions |
| `D11-D-016` | Test priority may be confused with Feature MoSCoW, defect severity, or pipeline order | All tests become one priority or critical tests are downgraded | Use the explicit P0–P3 consequence model and reviewer-controlled metadata |
| `D11-D-017` | Codex can change tests and implementation in the same task | Self-confirming expectations may mask requirement drift | Require authoritative source trace, behavior-impact diff review, human merge gate, and STOP protocol for conflicts |
| `D11-D-018` | Current Agent A/B/C reports contain detailed domain, security/API/Audit, and Local-First/Worker/engine recommendations that may be integrated concurrently | Duplicate or inconsistent fixture/category terminology is possible | Integrator normalizes one naming/metadata/TestKit standard while preserving their behavior matrices and gaps |

No issue above authorizes replacing MSTest/MTP, introducing paid test SaaS, lowering security/quality gates, editing Documents 01–10, creating a Feature/Requirement ID, or moving Document 12 content into Document 11.

---

## 16. Suggested Document 11 text

> ### Unit Test architecture and platform
>
> PERTAMINA OTO Unit Tests SHALL use MSTest with Microsoft.Testing.Platform. Test projects SHALL mirror production assembly and module ownership under the structured monorepo. Domain tests exercise real invariants and state behavior; Application tests exercise real use-case orchestration with small substitutes only at nondeterministic or infrastructure boundaries. A Unit test SHALL NOT claim to prove SQL/SQLite transactions, filesystem atomicity, HTTP/SignalR/IPC wiring, engine fidelity, malicious-file containment, UI/platform behavior, performance, VA/PT, or Production readiness.
>
> The repository SHALL maintain separate `Unit`, `Integration`, `Architecture`, `Contract`, `E2E`, and `Performance` suites. Slow tests SHALL not be placed in the Unit suite. Unit projects reference their owning production assembly and approved BuildingBlocks only; a global test project or shared TestKit SHALL not bypass module boundaries.
>
> ### Structure and naming
>
> Unit projects SHALL use `<ProductionAssembly>.UnitTests`; namespaces mirror the module and behavioral folder; classes use `<SubjectOrBehavior>Tests`; and methods use `<Operation>_When<Condition>_Should<ObservableOutcome>` or a consistent Given/When/Then equivalent. Test metadata SHALL identify Layer, Module, P0–P3 Priority, Requirement ID, canonical Feature ID where applicable, risk category, and regression/work-item ID where relevant.
>
> Test failures SHALL report safe codes and scenario identifiers without serializing requests, documents, passwords, tokens, OCR text, signatures, Assistant prompts, provider payloads, or sensitive canary values.
>
> ### Fakes, fixtures, and data
>
> Substitutes SHALL be limited to boundaries such as file/artifact storage, clock, ID generation, network state, engine/provider, permission/policy snapshot, scheduler, logger/evidence sink, and external service ports. Handwritten deterministic fakes/stubs/spies are preferred. Internal application/domain classes SHALL not be mocked one by one, and call order SHALL be asserted only when the protocol order is itself a requirement.
>
> Test builders SHALL create minimal valid immutable objects by default and expose scenario-relevant differences. Privileged, server, AI, public-network, destructive, and overwrite behavior defaults to denied. Fixtures SHALL use synthetic data only. Production documents, employee identity data, credentials, real signatures, issue screenshots, and confidential content are prohibited. Large/malicious/fidelity corpora belong to controlled non-Unit suites.
>
> ### Determinism, isolation, and parallelism
>
> Unit tests SHALL inject a fixed/advanceable UTC clock, deterministic opaque IDs, explicit connectivity and capability states, controlled scheduler/completion, explicit culture/time zone where relevant, and typed configuration/policy snapshots. They SHALL not depend on wall-clock sleeps, live network, machine paths, installed engines/Office, environment variables, test execution order, or mutable shared global state.
>
> Every test owns its mutable object graph and resources. Cleanup is deterministic, idempotent, and cannot swallow failures. Each Unit assembly explicitly declares parallel or sequential intent. Parallel execution is preferred for isolated tests; `DoNotParallelize` is a narrow documented exception. Worker count and process/module parallelism are centrally configurable after benchmark and are not hardcoded as a Pertamina standard.
>
> ### Coverage and quality gate
>
> Quality SHALL be based on critical behavior, security/privacy negative cases, changed-code regression, test pass/discovery, traceability, and absence of unresolved flaky or ignored critical tests. Line/branch coverage is a supporting metric. Numeric coverage and duration thresholds SHALL be measured and configured through approved project policy; they SHALL not be invented in Document 11 and SHALL never override a missing P0/P1 behavior test.
>
> Pull-request gates SHALL verify task scope, controlled restore, Release build/analyzers, full fast Unit execution on MTP, applicable P0 critical behavior, changed-code traceability, test integrity, safe reports, and human review. A zero-test/unexpectedly reduced run, failed/ignored critical test, flaky retry, deleted/weakened assertion without authority, or unapproved package/framework change blocks the gate.
>
> ### CI execution
>
> The simple CI order is Controlled Restore → Release Build → MSTest Unit Tests on MTP → required Security/Static Checks → safe Test/Coverage/Artifact Reports. Slow Integration/Component/Contract/E2E/Performance jobs remain separate. The same pinned SDK, central packages, MTP configuration, categories, and command entry point SHALL run locally and in Azure DevOps. Exact CLI flags are fixed by the repository-pinned version and validated during the foundation task.
>
> ### Flaky-test policy
>
> Flakiness SHALL be treated as a test, product-race, or environment defect. Root causes such as wall time, randomness, network, shared files, global state, ordering, parallel races, culture, asynchronous leakage, and cleanup failure SHALL be removed. Reruns may diagnose but cannot turn the original failure into green health. P0/P1 flaky tests block merge/release. Any temporary noncritical isolation requires an approved work item, owner, impact, expiry, visible status, and alternate evidence and is not counted as passed.
>
> ### Codex task and conflict rules
>
> Every Codex coding task SHALL state Task ID, Requirement IDs, Feature IDs, objective, Allowed Paths, Do-Not-Touch/Non-Scope paths, dependencies/locked decisions, Acceptance Criteria, Tests Required with layer/priority/command, and validation/output evidence. Codex SHALL add/update relevant tests with the behavioral change and report exact commands/results/risks.
>
> Codex SHALL NOT delete/disable a failing test, weaken assertions, broaden expected authorization, skip a critical test, accept snapshots blindly, rely on retry for green status, silently add/change a package/framework, invent expected behavior/IDs/thresholds, or claim DONE from build/Unit success alone. If a requirement and test conflict, Codex SHALL stop the behavior-changing work and report the authoritative source, conflicting test, impact, smallest resolution options, untouched files, and required decision owner.
>
> ### Priority and Definition of Done
>
> Tests SHALL use P0 security/business critical, P1 Core Feature path, P2 normal behavior, and P3 credible edge/rare behavior. Priority reflects consequence, not Feature MoSCoW, defect severity, or execution order. P0 includes authorization, sensitive-data exclusion, source/data integrity, destructive/privileged action, mandatory Audit, Assistant no-bypass, and similar critical rules. P1 includes the primary path and shared contracts for the 33 Core Tools.
>
> Unit tests are one mandatory part of the eight-layer Definition of Done. A Feature is not DONE when a required Unit test fails/is absent/flaky/ignored, a critical rule or security negative case is untested, traceability is missing, or any applicable Business, UX, Security, Performance, Recovery, Cross-Platform, Integration/E2E, Documentation, PoC, UAT, or release evidence remains unsatisfied.

---

**Integrator handoff:** reconcile the proposed project tree with the actual repository scaffold, decide the `ContentIntelligence` production owner, pin the .NET/MSTest/MTP configuration centrally, normalize Agent A/B/C/D fixture and metadata vocabulary, and record the numeric-threshold handoff to Document 12. This report changes no master document and contains no production implementation.
