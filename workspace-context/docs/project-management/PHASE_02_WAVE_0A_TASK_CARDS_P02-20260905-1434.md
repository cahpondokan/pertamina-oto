# PERTAMINA OTO — Phase 2 Wave 0A Task Cards

| Field | Value |
|---|---|
| Run ID | `P02-20260905-1434` |
| Checkpoint | Wave 0A — free-only intake, harness, and synthetic corpus preparation |
| Authorized at | 2026-09-05 15:17:38 +07:00 |
| Product baseline | `main` @ `5769ccce917d03905d07a1440ee527cb653c9cc4` |
| Cost boundary | `ZERO_ADDITIONAL_COST`; stop before any purchase, paid license, subscription, paid API/service, or billable infrastructure |
| Processing authority | `NOT_AUTHORIZED` in Wave 0A; no document processing with PDFsharp yet |

| Agent | Branch | Worktree |
|---|---|---|
| Agent 1 | `task/P2-A1-harness` | `D:\AFT Ahmad Yani Semarang\Teknik\Dokumentasi Pertamina OTO\worktrees\phase2-20260905\agent-1-harness` |
| Agent 2 | `task/P2-A2-pdfsharp-intake` | `D:\AFT Ahmad Yani Semarang\Teknik\Dokumentasi Pertamina OTO\worktrees\phase2-20260905\agent-2-pdfsharp-intake` |
| Agent 3 | `task/P2-A3-corpus-oracle` | `D:\AFT Ahmad Yani Semarang\Teknik\Dokumentasi Pertamina OTO\worktrees\phase2-20260905\agent-3-corpus-oracle` |

## Shared authority and boundaries

Workspace User explicitly approved the recommended Phase 2 Wave 0 packet, conditioned on no further cost. Approval covers acquisition-only of the exact PDFsharp 6.2.4 package from official NuGet, synthetic-only corpus preparation, isolated/no-network environment preparation, local sanitized evidence, measured baseline without numeric pass claim, and temporary consolidation of human roles with the independence limitation accepted.

Wave 0A does not approve a product dependency, a permanent adapter, provider execution, a paid alternative, hostile-corpus execution on the host, or an engine disposition. If any component, tool, license, service, or environment would incur cost, stop and return `BLOCKED_BY_COST` before commitment.

Agent 0 is the only writer for central coordination files and final integrator. Each agent must preserve the baseline, stay inside its worktree and Allowed Paths, return a complete handoff, and not merge into `main`.

## Agent 1 — `P2-W0A-A1-HARNESS`

- Objective: implement a throwaway, provider-neutral PoC harness skeleton and environment-manifest collector using only the already approved .NET/BCL boundary.
- Worktree: assigned by Agent 0 before dispatch.
- Allowed Paths: `poc/harness/**`, `eng/poc/**`, tests directly under those paths.
- Do Not Touch: `src/**`, permanent adapters, root dependency policy, product solution/project graph, Production configuration, central project-management files.
- Acceptance: deterministic run identity; sanitized environment manifest; timeout/cancel/process-tree supervision design; run-owned path validation; no-network mode represented explicitly; cleanup and failure records; unit/negative tests; no new package.
- Verification: locked/no-new-package restore if applicable, build, tests with non-zero discovery, secret/path scan, dependency diff.
- Stop: any new package/tool install, admin/system mutation, paid component, ambiguous owned path, or need to execute a PDF candidate.
- Handoff: files/diff, exact commands/results, known limitations, evidence paths, recommended integration order.

## Agent 2 — `P2-W0A-A2-PDFSHARP-INTAKE`

- Objective: perform acquisition-only intake for exact `PDFsharp` version `6.2.4` from the official NuGet flat-container/package source.
- Worktree: assigned by Agent 0 before dispatch.
- Allowed Paths: `poc/intake/pdfsharp-6.2.4/**`; sanitized run evidence under Agent 0-provided run-owned path.
- Do Not Touch: `src/**`, product projects/solution, `Directory.Packages.props`, root `NuGet.Config`, permanent adapter, central project-management files.
- Acceptance: exact source URL and UTC acquisition time; original `.nupkg` local SHA-256; package/signature verification result where supported; extracted nuspec/license/notice metadata; direct dependency declarations and target-framework assets; native/bundled asset inventory; no-cost evidence; provenance and maintenance references; proposed next admission record.
- Boundary: do not process a document; do not add package references; do not download transitive packages, iText, qpdf, or tools not already approved; no arbitrary latest version.
- Verification: hash recomputation, archive inventory, metadata consistency, malware/security scan if an already installed no-cost scanner is available, sanitized log.
- Stop: source/version mismatch, signature/provenance anomaly, paid term, unexpected native executable/telemetry/activation, network source outside official NuGet, or required transitive acquisition.
- Handoff: exact artifact path/hash, metadata findings, blockers, and Agent 6/7 review inputs.

## Agent 3 — `P2-W0A-A3-CORPUS-ORACLE`

- Objective: implement the deterministic synthetic corpus/oracle specification and benign generator/self-test foundation without using PDFsharp or another unapproved PDF library.
- Worktree: assigned by Agent 0 before dispatch.
- Allowed Paths: `poc/corpus/**`, `poc/oracles/**`, tests directly under those paths.
- Do Not Touch: `src/**`, permanent adapter/UI, product dependency graph, real/Production documents, central project-management files.
- Acceptance: versioned synthetic manifest schema; generated-data classification/provenance; deterministic control/ordering/duplicate/negative-oracle fixture definitions; expected-property schema; independent verifier contract; negative self-tests; retention/disposal markers; no sensitive content.
- Boundary: no personal/operational document; no hostile fixture execution; no candidate engine; no numeric pass threshold invented.
- Verification: deterministic output/digest test, schema/self-test, canary/secret scan, dependency diff.
- Stop: need for external package, paid tool/service, real document, unsafe malformed/active-content execution, or ambiguity in classification/retention.
- Handoff: files/diff, exact test results, generated-versus-deferred corpus coverage, limitations, evidence paths.

## Agent 0 checkpoint

Agent 0 will wait for all three Wave 0A handoffs, audit ownership/diffs/tests and the acquired artifact, then integrate only clean approved results. Agent 6 and Agent 7 review the frozen intake in Wave 0B. PDF processing remains prohibited until those reviews and an exact `APPROVED_FOR_POC_ONLY`/execution record are complete.
