# AGENT D — Test Management, Environment, Data, Automation & Release Gate Report

> **Task ID:** `DOC12-AGENT-D`  
> **Peran:** Senior QA Project Manager / Test Manager / Test Automation Architect / Senior .NET Build Engineer / DevOps-Test Pipeline Architect  
> **Tanggal:** 28 Agustus 2026  
> **Baseline berwenang:** Documents 01–11 FINAL; `MASTER_PROJECT_SUMMARY.md` v3.4  
> **Status keluaran:** Rekomendasi specialist untuk integrasi terkendali ke Document 12; bukan Test Plan master, bukan production code

## Audit baseline dan batas laporan

Baseline yang dipertahankan:

- Azure DevOps Git dan Azure DevOps Pipeline;
- minimum environment berwenang: Development, QA/Testing, dan Production;
- UAT wajib dilakukan setelah QA memenuhi kriteria, tetapi Documents 01–11 belum menetapkan UAT sebagai environment fisik permanen yang keempat;
- MSTest + Microsoft.Testing.Platform untuk Unit Test;
- .NET 10 LTS baseline dengan exact SDK/package pin saat repository scaffold;
- controlled restore, Release build, test berlapis, security scanning, package/artifact, promotion, dan evidence;
- artifact yang sama dipromosikan antar-environment; nilai environment diinjeksikan terpisah;
- Production hanya menerima artifact pipeline, debug nonaktif, least privilege, dan monitoring aktif;
- data Development sintetis; data Production tidak disalin ke non-Production tanpa masking dan approval;
- Definition of Done delapan lapis tetap menjadi syarat; test teknis hijau bukan bukti tunggal `DONE` atau Production-ready;
- 33 Primary PDF Tool Cards didahulukan, dengan `FEAT-PDF-005 — Merge PDF` sebagai vertical slice pertama;
- public AI endpoint dilarang dan Sign PDF tetap visual signature/paraf non-PKI.

Status repository saat laporan dibuat:

- Documents 01–11 FINAL tersedia;
- workspace masih documentation-only: belum ada `.sln`, `.csproj`, source code, executable pipeline, test environment, atau benchmark aktual;
- karena itu, durasi, coverage, throughput, concurrency, file-size, timeout, retry, defect SLA, evidence retention, dan kapasitas environment **tidak boleh diklaim sebagai angka final** dalam laporan ini.

Traceability utama:

```text
IR-DEVOPS-001..004  Azure DevOps Git/Pipeline, PR review, restore/build/unit/security/artifact
NFR-ENV-001..004    Dev/QA/Prod, pipeline-only Production, debug off, Production-data rule
NFR-PERF-006..013   benchmark, server budget, load/concurrency, AI/provider bounds
SR-SEC-001..023      security/privacy/file/Assistant/AI boundaries
BR-GOV-003           Requirement → Feature → Code → Unit Test → Test Case → UAT → Release
BR-GOV-006..009      PoC gate, Primary Feature Set First
NFR-HAND-001         source/build/deploy/test/UAT/handover evidence
TECH-DEC-004/013/015 monorepo, MSTest+MTP, scoped Codex task/human merge gate
Document 08 §§26–29 security pipeline, environment, test pyramid, VA/PT, restore
Document 11 §§37–45 coverage, Unit quality gate, CI, Codex, downstream handoff
```

---

## 1. Test-management model

### 1.1 Model operasi

Testing dikelola sebagai alur bukti berkelanjutan, bukan fase tunggal di akhir:

```text
Requirement/Feature dan risk review
→ test scope + priority + data + environment
→ automation/manual design
→ implementasi bersama perubahan
→ local/PR verification
→ QA deployment dan layered execution
→ defect triage + retest + regression
→ release-candidate evidence
→ UAT readiness decision
→ Document 13 UAT execution
```

Untuk konteks satu developer, model minimum cukup menggunakan:

1. satu test inventory/traceability matrix;
2. satu defect backlog;
3. satu Azure DevOps pipeline dengan stage berbasis trigger/risiko;
4. satu test summary per build atau release candidate;
5. satu UAT entry package;
6. human review/decision pada merge, exception, dan readiness.

Tidak diperlukan status, rapat, approval, atau laporan duplikat yang tidak mengubah keputusan. Tanggung jawab QA/Test Manager/Developer/Business User adalah tanggung jawab proyek; bukan role aplikasi baru pada Document 09.

### 1.2 Basis perencanaan

Test scope ditentukan dengan tiga lensa sekaligus:

| Lensa | Pertanyaan |
|---|---|
| Requirement-based | Requirement/Feature/business rule apa yang harus dibuktikan? |
| Risk-based | Apa dampak jika salah: akses, data, source file, audit, business outcome, server capacity, platform, recovery? |
| Change-based | Komponen, contract, dependency, data, environment, dan regression area apa yang berubah? |

Risk assessment minimum mempertimbangkan business criticality, security/privacy, data/source integrity, destructive action, complexity/PoC, Local/Server/Hybrid routing, shared-contract blast radius, supported platforms, historical defect, dan change frequency.

### 1.3 Test inventory minimum

Setiap test case/suite yang dikendalikan Document 12 mencatat:

```text
Test ID dan judul
Requirement ID
canonical Feature ID atau N/A
test layer/type
priority dan risk
automation/manual status
environment dan data-set ID
precondition/action/expected result
owner/responsibility
last result + build/version
defect/evidence link
applicable gate dan UAT handoff
```

Unit Test ID dan P0–P3 mengikuti Document 11. Test non-Unit dapat memakai namespace ID masing-masing setelah Integrator mengunci konvensinya; ID test bukan Requirement ID atau Feature ID baru.

### 1.4 Test-level portfolio

| Layer/type | Tujuan pengelolaan |
|---|---|
| Unit | Behavior deterministik Domain/Application/pure adapter; cepat pada PR |
| Architecture | Module ownership, forbidden reference, client-no-SQL, Assistant boundary |
| Contract | OpenAPI, API/error/event/IPC/Feature/schema version and compatibility |
| Integration/Component | SQL/SQLite/filesystem/object store/HTTP/Worker/engine sandbox nyata terkontrol |
| Authorization/Security | identity/policy/ownership/IDOR/abuse/privacy/malicious corpus |
| Functional/E2E | Alur lintas boundary dan tiga entry path pada target platform |
| Recovery/Compatibility | crash/restart/reconnect/restore/migration/client-platform/provider compatibility |
| Performance/DoS | CPU/RAM/storage/DB/network/queue/concurrency/large-malformed workloads |
| Manual/Exploratory/Usability | fidelity visual, usability, adaptive UX, dan risiko yang tidak efisien diotomasi |
| UAT | Skenario dan acceptance pengguna bisnis milik Document 13 |

---

## 2. Environment strategy

### 2.1 Prinsip environment

- Endpoint, credential, service identity, configuration, database/storage/queue/provider, telemetry, deployment authorization, dan access path dipisahkan sesuai environment.
- Artifact aplikasi dibuat sekali oleh pipeline dan dipromosikan. Environment configuration/secret tidak dibundel ke artifact.
- Tidak ada manual in-place patch, startup migration Production, atau perubahan config tanpa trace.
- Data, account, key, log, screenshot, dan test evidence mengikuti klasifikasi serta akses environment.
- Test yang berpotensi merusak, membanjiri, atau mengekspos data hanya dijalankan pada environment yang disiapkan untuk itu.

### 2.2 Environment matrix

| Environment | Purpose | Data type | Access | Deployment source | Test allowed | Test prohibited |
|---|---|---|---|---|---|---|
| **Development** | Coding, local feedback, Unit, static analysis, component awal, debugging yang diizinkan | Synthetic/generated minimal; deterministic fixture; sanitized sample hanya setelah approval | Developer berwenang dan CI/build identity Development; least privilege terhadap shared service | Local build untuk development loop atau pipeline Development artifact; bukan release evidence Production | Unit, architecture, contract logic, static/security scan, isolated component, exploratory teknis, fake/stub/provider sandbox yang disetujui | Production document/account/password/token/secret; unmasked Production copy; shared destructive/load/DoS; public AI; real user notification; claim UAT/Production acceptance |
| **QA/Test** | Authoritative system verification sebelum UAT: integration, contract, E2E, security, recovery, regression, performance sesuai stage | Synthetic; sanitized and approved representative documents; generated/corrupted/boundary corpus; non-Production test identities/secrets | Developer/test responsibility and pipeline service identities according to least privilege; restricted security corpus access | Immutable pipeline artifact promoted from controlled build; environment config injected | Smoke, integration/contract, functional/E2E, authorization/negative, selected malicious corpus, recovery, compatibility, performance/load within approved bounds, migration/rollback rehearsal | Raw Production content/credential; unbounded DoS; unapproved outbound provider/public AI; manual binary/config patch; tests that can affect Production |
| **UAT logical lane/environment** | Business-user validation of a release candidate that has passed UAT entry gates | Synthetic or sanitized representative business data; pre-approved UAT accounts and scenarios; no secret reuse | Named UAT participants, limited QA/support responsibility, deployment identity; application access according to UAT scope | The same release-candidate artifact that passed QA, promoted by pipeline; no rebuild | Pre-UAT smoke, Document 13 UAT scenarios, business/UX validation, approved cross-platform checks, defect reproduction/retest needed for UAT | Penetration, malicious corpus, destructive resilience/load/DoS, exploratory infrastructure attacks, debug, direct DB/storage access, code/config fixes in place, raw Production confidential documents |
| **Production** | Live operation; post-deploy confidence and monitoring, not general test laboratory | Real operational data only through authorized product flows; dedicated approved synthetic canary only if policy allows | Operational/deployment identities and authorized users; developer has no implicit access | Same approved immutable pipeline artifact after required approvals; environment secret/config injected securely | Bounded post-deploy health/smoke, safe synthetic canary, monitoring validation, approved backup/restore/DR exercise under operational plan | Functional experimentation on user content; load/DoS/fuzz/malicious corpus/VA/PT without approved plan; test password/token; debug; direct data manipulation; untracked config/migration; public AI fallback |

### 2.3 UAT environment boundary

Documents 01–11 lock Development, QA, dan Production as minimum environments and require UAT after QA; they do not yet require a fourth permanently separate physical stack. Therefore:

- UAT **must be logically isolated** by release candidate, configuration, data, access, evidence, and test window;
- it may use a dedicated UAT deployment or an isolated/time-bounded QA lane only if no concurrent QA activity can alter its build/data/config;
- UAT must never run directly on a mutable developer environment;
- exact physical topology, network zone, identity mapping, refresh/reset, and ownership are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 2.4 Promotion and configuration checks

At every promotion verify artifact digest/version, configuration schema/version, migration compatibility, feature/policy snapshot, secret presence without disclosure, endpoint/provider environment, telemetry environment label, database/storage target, minimum client compatibility, and rollback/forward-fix readiness.

---

## 3. Test-data strategy

### 3.1 Approved data classes

| Data class | Use | Controls |
|---|---|---|
| Synthetic domain data | Default for all layers | Fictional identities/metadata; deterministic; no resemblance to employee/customer secret |
| Generated PDF/image samples | Functional/layout/size/format variants | Versioned generator/config/seed; license/provenance; no embedded secret or live URL |
| Sanitized representative documents | Fidelity/business scenarios when synthetic data is insufficient | Approval, documented sanitization, validation that re-identification/content leakage is removed, restricted environment/access |
| Known corrupted/malicious samples | Parser/validator/isolation/negative tests | Quarantined controlled corpus, hash/manifest, restricted access, no casual preview/email/sync, safe runner |
| Boundary-size samples | Below/equal/above configured limits and resource behavior | Derived from approved configuration; generated on demand where large; storage/cleanup bounded |
| Golden/reference outputs | Deterministic fidelity/contract comparison | Reviewed semantic baseline, version/digest, update requires approved diff; not auto-accepted |

### 3.2 Prohibited data

- dokumen rahasia/terbatas nyata tanpa explicit exceptional approval;
- Production password, PDF password, token, cookie, API key, certificate private key, connection string, secret, atau session artifact;
- Production document/OCR/clipboard/signature/Issue screenshot/message/provider payload copied into fixture;
- raw Production database/export/backup/log/trace/dump;
- real employee identity or path where fictional opaque IDs suffice;
- public/unapproved dataset with unclear license, embedded active content, or uncertain provenance.

Masking a document does not automatically make it safe. Production-derived data is the last resort and requires documented purpose, authority, sanitization method, verification, allowed environments, retention, and deletion.

### 3.3 Dataset manifest

Every governed reusable dataset/corpus records:

```text
Dataset ID and version
purpose + Requirement/Feature/Test IDs
source/generator/provenance/license
classification and sensitivity
format/count/size/digest
expected validation/result
allowed environment/suite
access owner
created/reviewed/expiry dates
retention/disposal rule
known active/malicious behavior
```

Large generated boundary data may be produced by a versioned generator rather than stored permanently. The manifest stores the generator/version/seed and expected digest where feasible.

### 3.4 Sanitization verification

Sanitization must check visible and hidden text, metadata, comments, embedded files, links/actions, OCR layer, annotations/forms, revision/thumbnail data, filenames/paths, document properties, images/barcodes, and recognizable identifiers as applicable. A second review or automated canary/scan is required for high-risk representative data. “Renamed file” is not sanitization.

### 3.5 Lifecycle

```text
request/generate
→ classify and register
→ scan/validate/quarantine as applicable
→ approve environment and access
→ use with read-only/reference protections where feasible
→ collect safe result only
→ expire/delete/archive under approved policy
→ verify cleanup
```

Test logs, screenshots, diffs, failure messages, dump, and reports must not reproduce the sensitive payload. Exact dataset repository, encryption/key, access group, approval authority, and retention duration require confirmation.

---

## 4. Automation strategy

### 4.1 Automation decision

Automate when behavior is repeatable and has one or more of these characteristics:

- business/security/privacy/data-integrity critical;
- regression-prone or historically defective;
- executed frequently across builds/platforms/configurations;
- deterministic expected result;
- high manual cost or broad data/state matrix;
- contract/state/permission/error boundary;
- needed as a merge/release gate;
- stable enough that maintenance cost is justified.

Keep manual or hybrid when the primary evidence requires visual/semantic/user judgment, exploratory discovery, rapidly changing UX, complex usability, or rare one-off verification where automation cost exceeds repeat value.

### 4.2 Automation portfolio

| Scope | Default automation |
|---|---|
| PR | Restore/build/analyzer, Unit, architecture, fast contract/schema, secret/dependency/static scans, affected regression |
| Main/continuous | Full Unit, integration/contract by change impact, package/SBOM/digest, broader regression |
| QA deployment | Deployment verification, smoke, migration/config compatibility, API/DB/storage/Worker connectivity |
| Scheduled/nightly | Broader E2E/platform, malicious corpus, recovery, dependency re-scan, longer regression and bounded load/soak |
| Release candidate | Required full regression, security, performance, compatibility/recovery, evidence completeness, UAT readiness |

All automation should be idempotent, deterministic where possible, environment-aware, safe to repeat, able to clean its own data, and produce machine-readable plus concise human-readable results.

### 4.3 Selection by change impact

Change impact selects additional suites using explicit mapping:

```text
changed path/module
+ Requirement/Feature IDs
+ contract/schema/dependency/config change
+ security/data/platform risk
→ mandatory test projects/suites/environments/gates
```

Path-based selection alone is insufficient for shared contracts, authentication, Feature Registry, storage, Audit, Worker, and security configuration. A change to these areas triggers broader regression even if few files changed.

### 4.4 Automation quality

- Test code is reviewed/scanned/versioned like production code.
- Failures have stable Test ID, safe expected/actual, build/environment, and artifact link.
- A test must fail for the intended reason; infrastructure failure is reported separately from product failure.
- Automation must detect zero/missing tests, unexpected test-count reduction, ignored critical tests, and unsafe artifact creation.
- Retry may diagnose infrastructure instability but never converts an original product/test failure into healthy `PASS`.
- UI automation uses stable semantic identifiers/contracts, not fragile coordinates/timing where avoidable.
- Snapshot/golden updates require semantic review and cannot be bulk-approved by Codex.

### 4.5 Automation boundaries

Do not automate merely to reach an automation percentage. Do not replace meaningful manual fidelity/usability review with pixel noise, and do not keep deterministic P0/P1 regression manual when it can be reliably gated. No paid test SaaS, new framework, or external data upload is selected without dependency/security/license/data-residency approval.

---

## 5. Manual-test strategy

### 5.1 Appropriate manual testing

| Manual area | Evidence focus |
|---|---|
| PDF/Office/image visual fidelity | clipping, font/layout, page order, rendering, printable appearance, visual diff interpretation |
| Scanner/camera/signature interaction | natural capture/crop/stylus/touch flow, device capability, accessibility; signature remains visual non-PKI |
| UX/adaptive behavior | clarity, unnecessary steps/dialogs, responsive layout, platform-natural interactions, accessibility |
| Exploratory testing | unexpected combinations, workflow transitions, interruption/recovery, misunderstood errors |
| AI/translation/summarization quality | grounded usefulness and language quality within approved provider/data boundary; security remains automated + manual |
| Rare one-off verification | environment/provider/packaging compatibility where repeat automation has low value |

### 5.2 Manual charter template

```text
Charter/Test ID
Requirement and Feature IDs
objective/risk
build + environment + platform/device
dataset ID/classification
precondition and timebox
areas/actions to explore
oracles/expected invariants
observations and safe evidence
defects/known risks
coverage gaps and automation candidate
```

### 5.3 Manual execution controls

- Run smoke first so a broken build does not waste UAT/exploratory time.
- Use sanitized/synthetic data and least-privilege accounts.
- Record what was actually observed, not a blanket “looks good.”
- High-risk visual outcomes such as Redact final require semantic/security verification in addition to appearance; an overlay alone is never accepted as real redaction.
- Manual pass cannot override a failed automated security/integrity gate.
- Repeated stable manual cases are reviewed as automation candidates.

---

## 6. Pipeline test stages

### 6.1 End-to-end pipeline concept

```text
Controlled Restore
→ Release Build + analyzers
→ MSTest Unit Tests on MTP
→ Static/Security/Source/Secret/Dependency/Config checks
→ Integration/Architecture/Contract tests
→ Package + SBOM + digest + immutable artifact
→ Deploy QA
→ Deployment Smoke
→ Selected Functional/E2E/Authorization/Recovery regression
→ Stage-appropriate Performance/Security gates
→ Release-candidate Test Summary
→ UAT Readiness Gate
```

### 6.2 Stage matrix

| Stage | Normal trigger | Scope | Blocks |
|---|---|---|---|
| Restore/Build | Every PR/main | Pinned approved sources/packages, Release build, compiler/analyzers | Any restore/build failure or unapproved dependency/source |
| Unit | Every PR/main | Full fast Unit suite or locked equivalent ensuring all projects discovered; affected P0/P1 | Failure, flake, zero/missing project, ignored critical test |
| Static/Security | Every PR for fast checks; scheduled for deeper rescans | Secret, dependency, SAST/unsafe/source/config; license/SBOM inputs as applicable | Required tool/findings by approved severity policy; scanner failure not pass |
| Architecture/Contract/Integration | PR when affected; main always for core set | Module boundaries, OpenAPI/event/IPC/Feature/schema, DB/filesystem/service components | Contract/compatibility/transaction/ownership failure |
| Package | Main/release candidate | One immutable versioned artifact, SBOM/digest/config/migration references | Missing integrity/provenance/evidence or repack from different inputs |
| Deploy QA/Smoke | Main candidate/on demand | Controlled deployment, configuration/migration/health/core smoke | Deployment/health/config/data connection smoke failure |
| Functional/E2E/Regression | QA/nightly/release candidate based on impact | Affected features/platforms + shared paths + defect regression | Required scenario failure or unacceptable open defect |
| Performance/Security deep | Scheduled, milestone, attack-surface/dependency change, release candidate | Load/soak/DoS, malicious corpus, DAST, recovery, VA/PT as applicable | Threshold/security acceptance failure or missing required evidence |
| UAT Readiness | Release candidate | Aggregate gates, known issues, traceability, environment/data/user package | Any mandatory entry criterion not satisfied |

### 6.3 Lean execution for one developer

- PR runs fast deterministic gates and all affected critical tests.
- Main runs broader integration/contract and creates a candidate package only after success.
- Nightly/scheduled runs longer suites when an executable baseline exists.
- QA deployment and selected E2E occur for meaningful integrated changes, not every documentation-only commit.
- Full performance/security/recovery/compatibility runs occur at risk milestones, major dependency/boundary changes, and release candidate—not on every commit.
- A pipeline template centralizes steps; feature tasks do not copy/diverge gate logic.
- Failing gate stops dependent stages, while independent evidence collection may continue if safe.

### 6.4 Pipeline integrity

The same artifact moves toward UAT/Production; no rebuild after QA evidence. Cache may improve speed but cannot bypass restore integrity. Secrets are injected by environment-specific approved identity and never printed. Pipeline logs/artifacts are classified, access-controlled, and scanned for prohibited canaries.

Exact Azure DevOps YAML/template layout, hosted/self-hosted agent, branch policy, products/extensions, timeout, parallel jobs, retry policy, retention, and approval identities require repository implementation and Pertamina confirmation.

---

## 7. Defect model

### 7.1 Minimum defect record

```text
Defect ID + concise title
build/version/commit and environment
Requirement ID + Feature ID or N/A
test/step and data-set ID
expected vs actual
safe reproduction steps
severity and defect priority
affected platform/module/contract
security/privacy/data/recovery impact
safe evidence links
owner and current status
target release or disposition
fix build/PR and regression Test ID
```

Defect evidence must not contain document payload, password, token, signature, full OCR/prompt, Production secret, raw path/storage key, or unrestricted diagnostics.

### 7.2 Lean status flow

```text
NEW
→ CONFIRMED
→ IN_PROGRESS
→ READY_FOR_RETEST
→ VERIFIED
→ CLOSED
```

Rules:

- `NEW`: observation recorded with minimum reproducible safe evidence.
- `CONFIRMED`: reproducible/credible, scope and severity established.
- `IN_PROGRESS`: scoped fix/investigation active.
- `READY_FOR_RETEST`: fix artifact/build exists and affected automated test is added/updated where appropriate.
- `VERIFIED`: exact defect retest passed and required regression passed on the target environment/build.
- `CLOSED`: evidence and traceability complete; known-risk/deployment/document impact resolved.
- Retest failure returns to `CONFIRMED` or `IN_PROGRESS`; it does not create a duplicate defect.

`DUPLICATE`, `AS_DESIGNED`, `NOT_REPRODUCIBLE`, `DEFERRED`, atau `ACCEPTED_RISK` should be resolution/disposition values, not a large status workflow. Each needs evidence and human decision appropriate to impact. Security exceptions follow Document 08 formal expiring acceptance; Codex cannot choose them.

### 7.3 Triage cadence

Triage occurs when a new Blocker/Critical appears, a pipeline/release candidate fails, or before a release/UAT readiness decision. For a solo developer, there is no need for a standing committee: one concise review of validity, severity, priority, scope, regression, target, and decision owner is sufficient.

---

## 8. Severity/Priority

### 8.1 Defect severity — impact

| Severity | Impact definition | Examples |
|---|---|---|
| **BLOCKER** | Testing, deployment, UAT, or primary product use cannot continue meaningfully; no viable safe workaround; or environment/build is unusable for required validation | Release candidate cannot start, data migration prevents all tests, core app fails startup, test environment integrity unknown |
| **CRITICAL** | Severe security/privacy/data/source-integrity/business failure, widespread outage, unauthorized access, destructive behavior, or false success; some unrelated testing may still continue | Cross-user content access, secret/document leakage, source corruption, Redact only overlays, Audit critical loss, Assistant permission bypass |
| **MAJOR** | Important requirement/core path materially wrong or unavailable; significant impact with limited safe workaround | Core conversion incorrect on supported format, retry/resume broken, major platform path unavailable, incorrect result without destructive/security impact |
| **MINOR** | Limited noncritical defect with small impact and safe workaround; cosmetic/text/report issue that does not mislead security/business outcome | Spacing/alignment issue, noncritical wording, low-impact diagnostic/report formatting |

Severity is based on technical/business impact and does not change merely because a deadline is near.

### 8.2 Defect priority — repair order

Use a distinct namespace to avoid confusion with Document 11 Test Priority:

| Defect priority | Meaning |
|---|---|
| `DEF-P0` | Fix/investigate immediately; blocks current validation/release decision |
| `DEF-P1` | Must be resolved for the current target release/UAT entry unless explicitly dispositioned |
| `DEF-P2` | Planned after higher-risk work or next suitable iteration |
| `DEF-P3` | Low urgency backlog/cleanup after value review |

Priority considers severity, exposure, reproducibility, affected user count, milestone, dependency/blocking, workaround, fix risk, and regulatory/security obligation. It is a scheduling decision, not impact classification.

Examples of separation:

- A MAJOR defect that blocks all UAT scenarios can be `DEF-P0`.
- A MINOR misleading security label may be `DEF-P1` if it affects imminent release clarity.
- A CRITICAL vulnerability normally requires `DEF-P0`; any alternate handling requires the governed security exception process, not ordinary backlog reprioritization.

Security-scanner severity and VA/PT rating remain their authoritative security classification. They must not be down-scored merely to fit the QA defect labels. Mapping and remediation SLA require Pertamina confirmation.

---

## 9. Retest/regression

### 9.1 Retest

A retest verifies the exact defect against the fix build using the same relevant environment, dataset/config/precondition, and safe reproduction steps. It also checks the promised negative side effect—for example no upload, no unauthorized disclosure, no source overwrite, and no invalid final result.

Retest evidence records:

```text
Defect ID
fix build/commit/PR
environment/config/data-set version
test steps or automated Test ID
expected/actual result
PASS/FAIL/BLOCKED
safe evidence link
tester/reviewer and timestamp
```

### 9.2 Regression selection

Regression scope includes:

1. permanent automated test for the defect when repeatable;
2. affected component/module suite;
3. upstream/downstream contract and shared-family tests;
4. authorization/security/privacy/source/Audit tests when the boundary is touched;
5. platform/entry-path parity where relevant;
6. historical defect set for the changed risk area;
7. full regression at P5/release candidate.

`Retest PASS` without required regression is insufficient for `VERIFIED`. Conversely, full regression does not prove the exact defect if its reproduction was omitted.

### 9.3 Regression tiers

| Tier | Trigger | Scope |
|---|---|---|
| Affected | Every behavioral change/defect fix | Direct project/test + exact regression |
| Shared-risk | Shared contract, auth, storage, Audit, Worker, registry, config, dependency | All consumers and critical negative suites |
| Core | Integrated Core tool milestone/QA candidate | 33-tool shared families and released feature-specific P0/P1 |
| Full | P5 hardening and release candidate | All required automated/manual suites across target platforms and layers |

Tests cannot be removed from regression solely because they are slow; they must be optimized, scheduled appropriately, or formally dispositioned with risk.

---

## 10. Evidence management

### 10.1 Evidence types

Store when required:

- machine-readable test result and concise summary;
- safe structured log/trace window linked by synthetic or approved Correlation ID;
- sanitized screenshot/video for UI/visual defect only;
- coverage/architecture/contract diff report;
- performance/resource report and configuration;
- security/static/dependency/secret/VA/PT report under appropriate access;
- dataset manifest and generator/version/digest;
- environment/build/config/artifact/SBOM/digest record;
- defect retest/regression evidence;
- Requirement/Feature/test/UAT/release traceability report.

### 10.2 Evidence manifest

```text
Evidence ID/type
build/version/commit/artifact digest
pipeline run and environment
test suite/tool/config version
started/completed time
result + counts
Requirement/Feature/Test/Defect links
dataset ID/classification, not payload
artifact location/digest
access classification/owner
retention/expiry/disposal status
known limitation
```

### 10.3 Storage and access principle

- Volatile run outputs belong to restricted Azure DevOps/pipeline evidence storage or another approved repository; do not commit large logs/results/screenshots to source by default.
- Stable test definitions, manifests, traceability baseline, and reviewed small synthetic fixtures may live in the repository.
- Security findings, dumps, malicious corpus, sanitized representative files, and UAT evidence use restricted access separate from ordinary build output where needed.
- Artifact links must not embed bearer token/SAS/secret. Access is reauthorized at retrieval.
- Integrity uses pipeline/run linkage and digest where appropriate. Evidence cannot be silently overwritten by a later run.

### 10.4 Content minimization

Evidence must never include raw document bytes/content, full OCR/extracted/translated/summarized text, password/token/secret/key, visual signature payload, Assistant prompt/provider request/response, issue attachment/message beyond explicit restricted need, raw Production path/storage key, authorization header/cookie, or unrestricted database/log dump.

Screenshots are captured only when necessary, reviewed/cropped/redacted before upload, and use synthetic/sanitized data. If safe evidence cannot be produced, record the finding through controlled restricted handling rather than copying the payload into the ordinary defect/test system.

Exact evidence retention, legal hold, deletion, access groups, export authority, and security-report handling are `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

## 11. Test reporting

### 11.1 Minimum report

Every QA/release-candidate summary contains:

| Field | Required content |
|---|---|
| Build/version | Application version, commit, pipeline run, artifact digest/reference |
| Environment | Name/lane, config/policy/schema version, platform matrix |
| Executed | Planned/executed/not-run/blocked count by layer and priority |
| Passed/Failed/Blocked | Counts plus links; no rerun hiding initial failure |
| Critical defects | Open Blocker/Critical, status, owner, impact, target/disposition |
| Known risks/issues | Major/Minor, limitations, workaround, scope, decision |
| Requirement coverage | Covered/pass/fail/blocked/not-tested; Must/Core gaps explicit |
| Feature coverage | Canonical Feature ID and platform/entry-path state; 33 Core visibility |
| Performance observations | Workload/config/environment/resource metrics and threshold status; no unsupported extrapolation |
| Security status | Scan/test status, open findings/exceptions, VA/PT applicability, privacy canary result |
| Recovery/compatibility | Applicable crash/retry/restore/migration/client/platform result |
| UAT readiness | READY / NOT READY / BLOCKED with unmet criteria |

### 11.2 Status semantics

```text
PASS       executed and expected result satisfied
FAIL       executed and expected result not satisfied
BLOCKED    could not execute due to known precondition/environment/decision blocker
NOT RUN    planned but not executed; reason required
N/A        demonstrably not applicable; scope reason required
```

`BLOCKED`, `NOT RUN`, and `N/A` are not `PASS`. A diagnostic rerun result is shown alongside the original failure.

### 11.3 Coverage reporting

Requirement/Feature coverage is outcome coverage, not merely link count. A requirement is not “covered” when its only test is failed, blocked, obsolete, or at the wrong layer. Reports separate automated/manual and Unit/Integration/Contract/E2E/Security/Performance/UAT evidence.

### 11.4 Report cadence

- PR: compact build/Unit/static/affected-test result.
- QA deployment: deployment smoke and selected suite result.
- Milestone/nightly: trend and open risk, only when useful.
- Release candidate: complete UAT readiness summary.
- UAT execution result belongs to Document 13, not this report design.

---

## 12. Release gates

### 12.1 Gate outcomes

Each gate is `PASS`, `FAIL`, `BLOCKED`, or `NOT_APPLICABLE` with evidence and reason. A conditional/exception path requires explicit human authority, scope, compensating control, expiry, and follow-up; it is not silently relabeled `PASS`. Critical/High security findings follow Document 08 and normally block release unless the formal exceptional process applies.

### 12.2 Gate matrix

| Gate | Minimum evidence | Fails/blocks when | Normal cadence |
|---|---|---|---|
| **BUILD GATE** | Controlled restore, Release build/analyzers, approved dependency graph, no untracked generated change | Restore/build/analyzer failure; unapproved package/source; artifact cannot be reproduced/identified | Every PR/main |
| **UNIT GATE** | MTP discovery, all applicable P0/P1 and affected/full fast Unit pass, no critical ignore/flake, safe result | Failure/hang/crash/zero discovery; missing critical test; retry/weakening/deletion used to obtain green | Every PR/main |
| **INTEGRATION GATE** | Required architecture/contract/component/integration tests for change, DB/storage/API/IPC/engine boundary as applicable | Contract break, ownership/transaction/migration/wiring failure, unsupported compatibility | PR if affected; main/QA candidate |
| **SECURITY GATE** | Secret/dependency/static/config scans; authorization/privacy/abuse tests; malicious corpus/DAST/VA/PT according to stage | Required scan unavailable without approved policy; prohibited data leak; open disallowed finding; public AI/permission/content boundary failure | Fast every PR; deep scheduled/milestone/RC |
| **REGRESSION GATE** | Exact defect regression, affected/shared/core/full tier according to change/release | Required case fails/not run; historical critical defect recurs; platform/entry-path parity gap | Every fix; QA/milestone/RC |
| **PERFORMANCE GATE** | Representative workload, environment/config, CPU/RAM/storage/DB/network/queue/concurrency metrics and approved thresholds | Threshold/resource budget/backpressure/recovery objective fails or evidence not representative | Risk milestone, scale change, RC; not every commit |
| **UAT READINESS GATE** | Immutable RC, all entry gates, test summary, acceptable known issues, traceability, UAT environment/data/accounts, user-guide draft where needed | Any Blocker/Critical open; mandatory test failed/missing; environment/build/data not controlled; acceptance scope not traceable | Release candidate only |

### 12.3 Risk-based depth

| Change | Additional mandatory depth |
|---|---|
| Documentation-only with no executable/config contract change | Document validation/review; no artificial E2E deployment |
| Local low-risk implementation | Build/Unit/static + affected component/functional; Local-First/no-server regression |
| API/schema/event/permission/storage/Audit/Worker change | Broad contract/integration/authorization/security/recovery regression |
| Engine/provider/dependency/native/model change | PoC/compatibility/license/security/malicious corpus/resource/fallback evidence |
| High-risk Sign/Unlock/Protect/Redact/AI/intelligence | P0 security/privacy/confirmation/Audit/provider/data-boundary + manual semantic review |
| Release candidate/scale expansion | Full required regression, performance, security/VA/PT applicability, recovery, UAT package |

### 12.4 Release gate versus DoD

Gates provide evidence for the eight DoD layers:

```text
Technical → Build/Unit/Integration
Business → Functional regression + UAT handoff
UX → manual/E2E/accessibility/adaptive checks
Security → Security Gate
Performance → Performance Gate
Recovery → recovery/restore/retry evidence
Cross-platform → target platform matrix
Test/Documentation → reports, traceability, guide/handover
```

No single gate replaces the other layers. UAT Ready is not UAT Passed and not Production Approved.

---

## 13. Codex testing workflow

### 13.1 Mandatory task contract

Every Codex coding/test task states:

```text
TASK ID
Requirement IDs
Feature IDs
objective and risk
Allowed Paths
Do-Not-Touch/Non-Scope Paths
dependencies/locked decisions
Acceptance Criteria
Tests Required: layer, Test ID/priority, environment, command/gate
expected evidence/output
```

### 13.2 Workflow

```text
read authoritative requirement/feature/test and existing failures
→ verify scope/ownership/dependency/environment/data
→ run feasible baseline/affected tests
→ implement smallest authorized change + tests
→ run affected → module → required gate scope
→ report commands, discovered/pass/fail/blocked/skipped, evidence, risks
→ human review and merge/readiness decision
```

Codex may recommend a non-Unit handoff but cannot claim a fake/unit result proves the real DB/filesystem/HTTP/IPC/engine/UI/security/performance boundary.

### 13.3 Prohibitions

Codex must not:

- delete, ignore, skip, quarantine-as-green, or repeatedly retry a failing important test;
- lower assertion/expected result, broaden authorization, or auto-accept snapshots to make a gate pass;
- alter a Requirement/Feature/business/security rule to match current code/test;
- add/replace/upgrade test framework/package/SaaS/tool or upload source/data externally without approval;
- change pipeline/gate/threshold/priority/severity to hide a failure;
- use Production secret/content/data in tests or evidence;
- edit outside Allowed Paths or mix unrelated refactor;
- mark `DONE`, UAT Ready, or Production Ready from technical green alone.

### 13.4 Conflict STOP protocol

If requirement, test, implementation, or expected result conflict, Codex stops the behavior-changing work and reports:

```text
Task ID
Requirement/Feature and authoritative source
conflicting Test ID/path/expectation
actual observed behavior and build/environment, if safely verified
business/security/compatibility impact
smallest resolution options
files left unchanged
human decision owner required
```

Codex does not decide which source to rewrite. After human controlled decision, implementation, test, traceability, and defect/disposition are updated together.

---

## 14. Test phasing

| Phase | Scope | Key outputs | Exit criteria |
|---|---|---|---|
| **P0 — Test Foundation** | Repo/test scaffold, MSTest+MTP pin, Unit/Integration/Contract structure, pipeline templates, synthetic data generator/manifest, safe evidence, traceability, defect workflow, environment access/config | Executable fast pipeline, discovery gate, baseline reports, approved package/tool inventory | Restore/build/Unit/static pipeline works; no secret/data leak; target environments and ownership identified |
| **P1 — Merge PDF Vertical Slice** | `FEAT-PDF-005`, Document 11's 50 Unit cases plus real filesystem/PDF/component/E2E/manual fidelity and Local-First evidence | End-to-end first slice, regression seed, benchmark baseline, deployment smoke | Applicable P0/P1 pass; source/finality/order/local/Audit/privacy proven at correct layers; limitations recorded |
| **P2 — Core Low-Risk Tools** | Shared families and low-risk local Core: organize/page operations, JPG/PDF, rotate/page number/watermark/crop/compress/repair/compare and approved simple variants | Reusable family automation, Core regression pack, platform/capability mapping | Each released tool has traceability, shared + feature-specific critical evidence, no unauthorized server path |
| **P3 — PoC-Gated Core** | Office↔PDF, PDF→Office, OCR/scan, HTML→PDF, PDF/A, existing-text edit, advanced forms and other engine/device-gated capabilities | Approved PoC/limitation, provider/adapter contract, fidelity/security/resource corpus, fallback decision | `APPROVED_ENGINE`, `APPROVED_WITH_LIMITATION`, or governed blocker/alternative/defer decision; no full-scale coding claim without gate |
| **P4 — Core Security/Intelligence** | Unlock/Protect/Redact, visual Sign, AI Summarizer, Translate PDF, complex PDF to Markdown, Assistant/provider/privacy/Audit | P0 security/privacy/authorization/consent/Audit tests, malicious/prompt corpus, semantic/manual quality, public-AI denial | Required security gate pass; no password/signature/prompt/content leak; Redact real; provider approved; Sign remains non-PKI |
| **P5 — Full Regression / Hardening** | All implemented Core tools, platforms, three entry paths, recovery/restore/migration/compatibility, load/soak/DoS, security/VA/PT applicability, flaky/automation hardening | Full regression result, resolved defects/known risks, numeric benchmark evidence, release candidate | No open disallowed finding/defect; required gates pass; evidence complete and repeatable |
| **P6 — UAT Readiness** | Freeze/promote RC, prepare controlled UAT lane/data/accounts, summary/traceability/known issues/user-guide draft | Signed/recorded UAT entry package for Document 13 | UAT Readiness Gate PASS; environment and business participants ready; no claim UAT itself passed |

Phasing controls test implementation and evidence maturity. It does not alter canonical Feature IDs, 33-Core priority, or permit secondary tools before the approved Core gate.

---

## 15. UAT readiness criteria

### 15.1 UAT entry criteria

An application/build is ready to enter UAT only when:

```text
[ ] one immutable release candidate/version/digest is identified
[ ] RC is deployed by pipeline to controlled UAT lane/environment
[ ] deployment/config/migration/health smoke passes
[ ] Build, Unit, Integration, Security, Regression gates pass for scope
[ ] Performance gate passes where required for the target scope/scale
[ ] required recovery/compatibility/cross-platform evidence passes
[ ] no open BLOCKER or CRITICAL defect
[ ] MAJOR/MINOR known issues have scope, workaround, risk, owner, and human disposition
[ ] Must/Core requirement and canonical Feature traceability is current
[ ] planned UAT scope is testable and maps to Document 13 ownership
[ ] UAT accounts/permissions/data/platforms are prepared and least-privilege
[ ] test data is synthetic/sanitized and approved
[ ] user guide/release notes draft is available where needed
[ ] evidence package and support/escalation contact are ready
[ ] reset/redeploy/defect/retest process is understood
```

### 15.2 UAT entry package

| Item | Minimum content |
|---|---|
| Release candidate | Build/version, commit, artifact digest, environment/config/schema |
| Test summary | Executed/pass/fail/blocked/not-run by layer/priority; gate outcomes |
| Known issues | Defect ID, severity/priority, affected scope, workaround, decision, retest plan |
| Requirement traceability | Requirement → Feature → technical tests → UAT scope |
| Feature/platform scope | Included/excluded/limited Feature IDs and target Web/Desktop/Mobile capability |
| Security/performance/recovery status | Applicable findings, thresholds/results, exceptions/limitations |
| UAT data/accounts/environment | Dataset IDs/classification, accounts/permissions, readiness/smoke evidence |
| User guidance | Draft user guide/release notes for the tested scope when needed |
| Support path | How UAT participant records issue and how build reset/retest occurs |

### 15.3 Boundary with Document 13

Document 12 defines readiness, package, environment, evidence, and gate. Document 13 owns UAT actors, business scenarios, detailed steps, acceptance/sign-off, UAT defect handling detail, and final UAT outcome. This report intentionally does not write those scenarios or claim user acceptance.

---

## 16. Risks/conflicts

| ID | Risk/conflict | Impact | Required disposition |
|---|---|---|---|
| `D12-D-001` | Documents 01–11 lock Dev/QA/Prod minimum but not a permanent fourth UAT stack | Treating UAT as physical requirement may invent topology/cost; sharing mutable QA can invalidate evidence | Lock UAT logical isolation now; confirm dedicated vs isolated QA lane with Pertamina/infrastructure owner |
| `D12-D-002` | Workspace has no code solution, pipeline, test environment, or executable benchmark | Commands, duration, coverage, capacity, discovery count, and gates cannot be empirically validated | P0 foundation task must scaffold/pin/measure before numeric approval |
| `D12-D-003` | Exact Azure DevOps branch/reviewer/agent/approval/artifact/retention policy is unconfirmed | Pipeline gate may not be enforceable or evidence may be mishandled | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`; do not select paid extension/SaaS silently |
| `D12-D-004` | Exact Production-data masking/sanitization standard and approval authority are unconfirmed | Re-identification or confidential content can enter QA/UAT evidence | Default synthetic; production-derived data only after documented approved method and verification |
| `D12-D-005` | Test dataset/corpus repository, encryption, access, retention, and disposal are undecided | Malicious/sanitized/large files can leak or accumulate | Approve classified manifest-based store and lifecycle before corpus ingestion |
| `D12-D-006` | Numeric performance/server-budget/load/DoS thresholds remain benchmark-owned | Gate could pass meaningless or invented numbers | Measure representative QA environment; approve thresholds and variance; do not extrapolate unsupported scale |
| `D12-D-007` | QA/UAT infrastructure may not match Production topology/capacity/provider | False performance/security/compatibility confidence | Document differences and correction factors/limitations; require representative environment for critical claims |
| `D12-D-008` | PoC-gated engines/providers and exact provider test endpoints are not selected | Automation/data-egress/security/fidelity cases cannot finalize | Complete approved PoC/license/security/platform/resource/fallback decision before full test gate |
| `D12-D-009` | Public AI is prohibited, but test tooling/provider defaults may attempt external calls | Source/document/test evidence exfiltration | Network-deny/default fake; approve internal/local endpoints explicitly and test technical unreachability |
| `D12-D-010` | Defect fix SLA, priority authority, and security-severity mapping are not official | Inconsistent scheduling or security down-scoring | Confirm lightweight policy; keep severity impact and `DEF-P*` scheduling separate; Document 08 security rating remains authoritative |
| `D12-D-011` | Evidence retention/access/legal hold and sensitive security-report handling are unconfirmed | Premature deletion or overexposure | Confirm policy; minimize content; separate restricted evidence; preserve integrity/run linkage |
| `D12-D-012` | Full pipeline on every commit would be too heavy for one developer | Slow feedback and gate bypass pressure | Use fast PR, broader main/QA, scheduled deep, and full RC stages based on risk |
| `D12-D-013` | Excessive change-impact filtering can miss shared-contract consumers | False green after auth/storage/registry/Audit/Worker changes | Maintain explicit dependency/risk mapping and mandatory broad triggers for shared foundations |
| `D12-D-014` | Automation framework/coverage/UI/security tools beyond MSTest/MTP are not approved | Dependency/license/data-residency drift | Evaluate only when needed; central pin and approval; no paid SaaS baseline assumption |
| `D12-D-015` | Flaky test retry/quarantine can hide product concurrency defects | Unreliable release evidence | Preserve original failure; P0/P1 block; temporary P2/P3 isolation visible, owned, expiring, not counted PASS |
| `D12-D-016` | Manual visual/AI quality can become subjective | Inconsistent pass/fail or acceptance leakage into QA | Use charter/reference/oracle and explicit limitation; Document 13 owns business acceptance |
| `D12-D-017` | Security/load/malicious tests can destabilize shared QA/UAT | Availability and evidence contamination | Isolate/timebox/bound workloads; prohibit in UAT/Production without approved plan |
| `D12-D-018` | Production smoke/canary can touch real data or create records | Operational/privacy impact | Use safe read-only/approved synthetic canary, bounded cleanup and monitoring; exact policy confirmation required |
| `D12-D-019` | UAT known-issue acceptance could be treated as Production risk approval | Major defect or limitation bypasses later release governance | UAT entry disposition is scope-specific and temporary; Production acceptance remains separate |
| `D12-D-020` | Same labels P0–P3 exist for Unit Test priority and may be reused for defect priority | Reporting/triage confusion | Prefix defect priority as `DEF-P0..3`; always report both `TestPriority` and `DefectPriority` fields |
| `D12-D-021` | Document 13 scenarios do not exist yet | UAT coverage cannot be finalized in Document 12 | Handoff requirement/feature/business outcomes and entry package only; do not author full UAT here |
| `D12-D-022` | One developer owns much of implementation, tests, and triage | Self-confirming expectations and single-person bottleneck | Human review at behavior/gate/readiness points, scoped Codex tasks, immutable evidence, and explicit conflict STOP |

No risk above authorizes weakening Documents 01–11, creating a Feature/Requirement/role, using Production secrets/content, replacing MSTest/MTP, adding a test SaaS/vendor, editing the Document 12 master, or coding production.

---

## 17. Suggested Document 12 text

> ### Model pengelolaan test
>
> Testing PERTAMINA OTO dikelola sejak Requirement/Feature review sampai release candidate melalui model requirement-based, risk-based, dan change-based. Satu test inventory/traceability matrix, satu defect backlog, Azure DevOps Pipeline bertingkat, satu test summary per candidate, dan satu UAT entry package merupakan baseline lean untuk satu developer. Test teknis hijau tidak menggantikan Definition of Done delapan lapis.
>
> Setiap test mencatat Test ID, Requirement ID, canonical Feature ID atau N/A, layer/type, priority/risk, automation status, environment, Dataset ID, expected result, last build/result, defect/evidence, dan gate/UAT handoff. Unknown/duplicate Requirement atau Feature mapping harus dilaporkan, bukan diciptakan otomatis.
>
> ### Environment dan promotion
>
> Development digunakan untuk coding, Unit/static/component awal dengan synthetic/generated data. QA/Test digunakan untuk integration, contract, E2E, security, recovery, regression, dan performance dengan synthetic/sanitized/corrupted/boundary data yang disetujui. UAT menggunakan release candidate, access, configuration, dan data yang logically isolated; UAT dapat berupa dedicated deployment atau isolated QA lane setelah topology approval. Production hanya menerima immutable pipeline artifact, debug nonaktif, least privilege dan monitoring aktif, serta hanya menjalankan bounded approved smoke/canary—not general testing.
>
> Artifact dibuat sekali dan dipromosikan; environment configuration dan secret diinjeksikan terpisah. Endpoint, credential, identity, database/storage/queue/provider, telemetry, deployment authorization, dan access path dipisahkan. Manual in-place binary/config/migration dan rebuild setelah QA evidence dilarang.
>
> ### Test data
>
> Test memakai synthetic data, generated PDF/image, sanitized approved documents, known corrupted/malicious corpus, dan below/equal/above-boundary samples. Setiap reusable dataset memiliki versioned manifest berisi purpose/traceability, provenance/license/generator, classification, format/count/size/digest, expected result, allowed environment, access, retention, dan malicious behavior.
>
> Production document/content, password, token, cookie, secret/key, connection string, signature, OCR/prompt/provider content, issue screenshot/message, backup/export/log/dump, dan real identity/path dilarang sebagai ordinary test data. Production-derived data adalah pengecualian terakhir yang memerlukan purpose, authority, sanitization, verification, environment, retention, dan deletion yang disetujui. Test result/log/screenshot/diff juga tidak boleh menyalin payload sensitif.
>
> ### Automation dan manual testing
>
> Automation diprioritaskan untuk behavior repeatable, high-value, regression-prone, business/security critical, deterministic, frequent, dan required-gate. Manual/hybrid testing digunakan untuk visual fidelity, exploratory discovery, complex usability/adaptive UX, semantic AI/translation quality, device interaction, dan rare one-off verification. Tidak ada target “semua otomatis” atau “semua manual”; repeated stable manual cases ditinjau sebagai automation candidate.
>
> Automation harus deterministic/idempotent, environment-aware, membersihkan data sendiri, mendeteksi zero/missing/ignored tests, memisahkan product dan infrastructure failure, dan menghasilkan safe machine/human-readable evidence. Retry hanya diagnosis dan tidak mengubah failure awal menjadi PASS. Test code dan package/tool mengikuti review, central pinning, security/license/data-residency approval.
>
> ### Pipeline
>
> Pipeline mengikuti Controlled Restore → Release Build/Analyzer → MSTest Unit pada MTP → Static/Security/Secret/Dependency/Config Checks → Integration/Architecture/Contract → Package/SBOM/Digest → Deploy QA → Smoke → Selected Functional/E2E/Authorization/Recovery Regression → stage-appropriate Performance/Security → Test Summary → UAT Readiness.
>
> PR menjalankan fast deterministic gates dan seluruh affected critical tests. Main/QA menjalankan integration/contract dan broader regression. Scheduled/milestone/release-candidate stage menjalankan test berat, malicious corpus, load/soak/DoS, recovery, compatibility, serta VA/PT sesuai applicability. Tidak semua commit menjalankan suite terberat, tetapi shared security/contract changes memicu regression lebih luas daripada path filter biasa.
>
> ### Defect, retest, dan regression
>
> Defect flow minimum adalah NEW → CONFIRMED → IN_PROGRESS → READY FOR RETEST → VERIFIED → CLOSED. Retest failure kembali ke CONFIRMED/IN_PROGRESS. DUPLICATE, AS_DESIGNED, NOT_REPRODUCIBLE, DEFERRED, dan ACCEPTED_RISK adalah disposition dengan evidence dan human decision, bukan workflow besar.
>
> Severity mengukur impact: BLOCKER, CRITICAL, MAJOR, MINOR. Defect Priority mengukur urutan perbaikan dan memakai `DEF-P0..DEF-P3` agar tidak tertukar dengan Test Priority Document 11. Security finding mempertahankan authoritative security severity dan tidak boleh diturunkan untuk memenuhi jadwal.
>
> Setiap fix menjalani exact retest dan regression berdasarkan direct impact, shared contracts, security/data/platform boundary, serta historical defect. Repeatable defect mendapat permanent automated regression test. Retest PASS tanpa required regression, atau regression PASS tanpa exact reproduction, tidak cukup untuk VERIFIED.
>
> ### Evidence dan reporting
>
> Evidence yang diperlukan mencakup test result, safe log, sanitized screenshot, coverage/contract/traceability report, performance report, security report, dataset manifest, artifact/SBOM/digest/config, dan defect retest. Evidence manifest mengaitkan build/commit/run/environment/tool/config/result/count/traceability/dataset classification/artifact digest/access/retention tanpa menyimpan document payload atau secret.
>
> Test summary minimum memuat build/version, environment, executed/passed/failed/blocked/not-run, critical defects, known risks, Requirement/Feature coverage, performance observations, security/recovery/compatibility status, dan UAT readiness. BLOCKED, NOT RUN, dan N/A bukan PASS. Coverage berarti valid outcome evidence, bukan sekadar link atau line percentage.
>
> ### Release gates
>
> Release gates terdiri dari BUILD, UNIT, INTEGRATION, SECURITY, REGRESSION, PERFORMANCE, dan UAT READINESS. Setiap gate berstatus PASS/FAIL/BLOCKED/NOT_APPLICABLE dengan evidence. Exception memerlukan explicit authority, scope, compensating control, expiry, dan follow-up serta tidak boleh disamarkan sebagai PASS. Critical/High security handling mengikuti Security Design.
>
> BUILD dan UNIT berjalan setiap PR/main. INTEGRATION/CONTRACT berjalan saat affected dan pada integrated candidate. SECURITY memiliki fast PR dan deep milestone/release layers. REGRESSION mengikuti affected/shared/core/full tiers. PERFORMANCE berjalan setelah benchmark pada risk/scale/release milestone. UAT READINESS hanya untuk immutable release candidate setelah seluruh applicable entry gate dan package siap.
>
> ### Codex workflow
>
> Setiap Codex task menyebut Task ID, Requirement IDs, Feature IDs, objective/risk, Allowed Paths, Do-Not-Touch/Non-Scope, dependencies/locked decisions, Acceptance Criteria, Tests Required beserta layer/priority/environment/command/gate, dan expected evidence. Codex menjalankan baseline yang feasible, membuat perubahan dan test terkecil dalam scope, menjalankan affected/module/required gates, lalu melaporkan discovered/pass/fail/blocked/skipped, command, evidence, dan risk.
>
> Codex dilarang menghapus/skip/ignore/quarantine-as-green/retry test penting, menurunkan assertion/expected result, mengubah requirement agar hijau, menambah framework/package/SaaS tanpa approval, mengubah threshold/gate/severity/priority untuk menyembunyikan failure, menggunakan Production data/secret, atau menyatakan DONE/UAT Ready dari technical green. Konflik requirement-test-implementation wajib STOP dan human review.
>
> ### Phasing dan UAT handoff
>
> Test delivery mengikuti P0 Test Foundation, P1 Merge PDF Vertical Slice, P2 Core Low-Risk Tools, P3 PoC-Gated Core, P4 Core Security/Intelligence, P5 Full Regression/Hardening, dan P6 UAT Readiness. Phasing tidak mengubah canonical Feature ID atau 33-Core-first rule.
>
> UAT entry memerlukan immutable release candidate, controlled UAT lane, smoke pass, applicable Build/Unit/Integration/Security/Regression/Performance/Recovery/Compatibility evidence, no open Blocker/Critical, known-issue disposition, Requirement/Feature traceability, approved accounts/data/platforms, user-guide/release-note draft bila perlu, dan support/retest path. Document 13 tetap memiliki skenario, langkah, acceptance/sign-off, dan hasil UAT; UAT Ready bukan UAT Passed atau Production Approved.

---

**Serah terima Integrator:** konfirmasikan topology UAT, kebijakan Azure DevOps dan evidence, sanitization/data authority, defect/security severity mapping, tool/package inventory, serta numeric performance/coverage/duration/capacity thresholds setelah P0/P1 benchmark. Laporan ini tidak mengubah Documents 01–12, tidak memilih test SaaS/vendor berbayar, tidak menulis UAT Document 13, dan tidak mengimplementasikan production code.
