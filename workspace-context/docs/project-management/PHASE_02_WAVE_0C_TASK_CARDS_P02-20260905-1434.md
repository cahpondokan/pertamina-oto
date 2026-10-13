# PERTAMINA OTO — Phase 2 Wave 0C Task Cards

| Field | Value |
|---|---|
| Run | `P02-20260905-1434` |
| Authorized at | 2026-09-06 01:27:27 +07:00 |
| Authority | Workspace User approved Windows Sandbox inspection/activation with zero cost and explicitly prohibited restart without further approval |
| Environment observation | Optional feature `Containers-DisposableClientVM` reports `Enabled`; launcher absent; CBS and pending-file-rename indicators show restart pending |
| Cost boundary | `ZERO_ADDITIONAL_COST` |
| Processing authority | `NOT_AUTHORIZED` |

## Agent 1 — `P2-W0C-A1-SANDBOX-CONFIG`

- Continue branch/worktree: `task/P2-A1-harness`; `worktrees/phase2-20260905/agent-1-harness`.
- Objective: prepare a no-cost, no-launch Windows Sandbox configuration and verification/runbook for later execution.
- Allowed Paths: `poc/environment/windows-sandbox/**` only for this task.
- Do Not Touch: existing harness files, product code/graph, system settings, central records, evidence outside run-owned paths.
- Acceptance: `.wsb` template/config with networking disabled, read-only input mapping, minimal host exposure, sanitized evidence export design, clipboard/printer/audio/video controls where supported, prelaunch checks, post-restart verification, no-restart stop marker, and official Microsoft references.
- Verification: XML/config validation, path/secret scan, no Sandbox launch, no system mutation, no paid component.
- Stop: restart, feature/system/security mutation, credential prompt, unsupported setting, or cost.

## Agent 2 — `P2-W0C-A2-DEPENDENCY-METADATA`

- Continue branch/worktree: `task/P2-A2-pdfsharp-intake`; `worktrees/phase2-20260905/agent-2-pdfsharp-intake`.
- Objective: produce an exact proposed `net10.0` dependency closure using official NuGet metadata only.
- Allowed Paths: `poc/intake/dependency-closure/**` only for this task.
- Do Not Touch: earlier intake, raw package, product code/graph, root NuGet policy, central records.
- Acceptance: direct and transitive component/version/source/license proposal, dependency edges, framework selection rationale, no-cost status, acquisition allowlist proposal, unresolved runtime/native/assets, and change-control/stop conditions.
- Boundary: metadata queries only; no `.nupkg`, binary, installer, source archive, or tool acquisition; no restore/package reference/provider execution.
- Verification: official endpoints/links and check time, deterministic manifest/schema validation, no binary files, dependency/license gaps explicit.

## Agent 3 — `P2-W0C-A3-INDEPENDENT-ORACLE-RESEARCH`

- Continue branch/worktree: `task/P2-A3-corpus-oracle`; `worktrees/phase2-20260905/agent-3-corpus-oracle`.
- Objective: research two or three exact no-cost, independently maintained PDF structural/render oracle candidates from primary sources.
- Allowed Paths: `poc/oracles/candidate-research/**` only for this task.
- Do Not Touch: earlier corpus/oracle files, product code/graph, central records.
- Acceptance: exact version/source/license, structural/render capability, Windows/net10/offline boundary, native/subprocess implications, dependency/license/security risks, independence from PDFsharp, recommended evaluation order, and exact acquisition decision inputs.
- Boundary: research/metadata only; no download/install/execute, public endpoint, real document, or paid component.
- Verification: primary-source links and check date; clearly separate facts, inference, recommendation, and human decision.

## Agent 0 checkpoint

Agent 0 will integrate acceptable preparation records, then issue one exact zero-cost acquisition packet. No Sandbox launch or PDF processing occurs before a user-approved restart and post-restart environment verification.
