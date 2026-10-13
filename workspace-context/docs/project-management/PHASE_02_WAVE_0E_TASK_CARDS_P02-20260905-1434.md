# PERTAMINA OTO — Phase 2 Wave 0E Task Cards

| Field | Value |
|---|---|
| Run | `P02-20260905-1434` |
| Authorized at | 2026-09-07 09:53:42 +07:00 |
| User command | Continue Phase 2; everything built/acquired must be free; disclose any paid requirement |
| Input checkpoint | `integration/P2-proof-capability` @ `38d5494050f62caea2427c546125791d53c73ca9` |
| Product main | Clean `main` @ `5769ccce917d03905d07a1440ee527cb653c9cc4` |
| Cost boundary | `ZERO_ADDITIONAL_COST`; stop before any paid component/service/license |
| Restart authority | `NOT_AUTHORIZED` |
| Processing authority | `NOT_AUTHORIZED` |

## Wave 0E-A — three parallel lanes

### Agent 1 — `P2-W0E-A1-POSTRESTART-VERIFIER`

- Objective: prepare only a read-only post-restart Windows Sandbox readiness verifier and runbook extension; do not restart or launch Sandbox.
- Worktree: existing Agent 1 Phase 2 worktree.
- Allowed Paths: `poc/environment/windows-sandbox/postrestart/**` only.
- Do Not Touch: system settings, existing Sandbox template, product code/graph, central records, other agent evidence.
- Acceptance: check feature state, launcher/start entry, virtualization prerequisites observable without mutation, expected template controls, and explicit stop before launch; sanitized output; zero-cost statement.
- Verification: self-test fixtures/mocks only; no Sandbox launch, feature mutation, restart, install, download, paid tool, or document processing.
- Handoff: commit, paths, self-test result, risks, and exact post-restart command that remains unexecuted.

### Agent 2 — `P2-W0E-A2-NUGET-ACQUISITION-ONLY`

- Objective: acquire and statically inspect exactly three approved NuGet artifacts into the approved evidence directory.
- Exact packages:
  - `Microsoft.Extensions.Logging.Abstractions` 8.0.3
  - `System.Security.Cryptography.Pkcs` 8.0.1
  - `Microsoft.Extensions.DependencyInjection.Abstractions` 8.0.2
- Exact URLs: only those frozen in `PHASE_02_WAVE_0D_CHECKPOINT_REPORT_P02-20260905-1434.md` and closure manifest SHA-256 `3130E92422B3449A26D30B80B3EE3030CC1699F8810D439AFB54A767CCEA857A`.
- Raw evidence target: `D:\OTO\pertamina-oto\artifacts\evidence\P02-20260905-1434\agent-2-dependency-intake`.
- Allowed tracked Paths: `poc/intake/dependency-artifacts/**` only.
- Acceptance: exact source/time/redirect/size/SHA-256/catalog SHA-512 comparison; package signature/trust limitation; path-safe archive inventory; nuspec/license/notices; analyzer/generator/build/buildTransitive/native/runtime asset inventory; dependency surprises and cost disposition.
- Prohibited: restore, build, install, assembly/analyzer/generator execution, PDF processing, product reference, wildcard/latest, extra dependency, alternate feed, paid component.
- Stop: mismatch, unexpected package/dependency/version/redirect/cost, executable script activation, or requested authority beyond acquisition/static inspection.

### Agent 3 — `P2-W0E-A3-QPDF-ACQUISITION-ONLY`

- Objective: acquire and statically inspect exactly `qpdf-12.4.1-msvc64.zip` as a structural-oracle candidate.
- Exact official URL: `https://github.com/qpdf/qpdf/releases/download/v12.4.1/qpdf-12.4.1-msvc64.zip`.
- Expected SHA-256: `3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6`.
- Raw evidence target: `D:\OTO\pertamina-oto\artifacts\evidence\P02-20260905-1434\agent-3-qpdf-intake`.
- Allowed tracked Paths: `poc/oracles/qpdf-12.4.1/**` only.
- Acceptance: exact source/time/redirect/size/hash; safe archive inventory without extraction to search paths; signature/provenance limitation; DLL/import and runtime dependency inventory without loading; license/notices/SBOM candidates; zero-cost disposition; reproducibility/removal risks.
- Prohibited: qpdf execution, PDF processing, product reference, installer, PATH change, extra component, MuPDF/Poppler use, paid support/license.
- Stop: hash/source mismatch, unexpected active content/dependency/cost, missing license evidence, or need to execute.

## Wave 0E-B — dependent independent review

After Agent 0 audits and integrates Wave 0E-A:

- Agent 6 independently reviews both exact byte sets for security, activation surfaces, dependency/import closure, trust limitations, and whether they may advance to an execution-freeze packet.
- Agent 7 independently reviews licenses/notices/provenance/redistribution/no-cost status and evidence completeness.
- Agent 0 then issues the next exact decision. No restart, Sandbox launch, restore/build, provider execution, or PDF processing occurs in Wave 0E.

## Cost rule

MIT and Apache-2.0 components may be evaluated without a license fee but still require
license/notice compliance. Any commercial license, subscription, paid API, paid support,
cloud charge, marketplace purchase, or other added fee is outside authority and must be
reported to the user before any commitment.

## Wave 0E-A checkpoint — 2026-09-07 10:00:22 +07:00

- Agent 1 completed and Agent 0 integrated the post-restart verifier @ `344e1db`;
  20/20 mock self-tests PASS and host probes remained zero.
- Agent 2 acquired exactly the three approved NuGet packages and completed static
  intake. Agent 0 independently matched all local SHA-256 and catalog SHA-512 values,
  found no unsafe/duplicate archive paths, and integrated the report @ `e8a32de`.
- All three package manifests report MIT and include license/notice files. Signature
  verification exited 0, with online revocation status still unknown. No restore,
  build, assembly/generator execution, or PDF processing occurred.
- Agent 3's qpdf acquisition command was rejected by automatic approval review before
  process creation because direct exact user confirmation was not recognized. The
  agent did not retry or bypass it; no qpdf bytes or cost. Blocker report integrated
  @ `dd003ea` and `P2-W0-QPDF-ACQ-001` remains `PROPOSED`.
- Agent 6 and Agent 7 independent reviews were dispatched against checkpoint
  `dd003ea`. Restart, Sandbox launch, and processing remain unauthorized.
