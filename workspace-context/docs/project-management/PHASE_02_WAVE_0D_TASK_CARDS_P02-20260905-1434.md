# PERTAMINA OTO — Phase 2 Wave 0D Review Task Cards

| Field | Value |
|---|---|
| Run | `P02-20260905-1434` |
| Input checkpoint | `integration/P2-proof-capability` @ `07b5621c8d5e26793e4d8658aef9f8e23b3a86ec` |
| Cost boundary | `ZERO_ADDITIONAL_COST` |
| Restart status | Pending; no restart authorized |
| Processing authority | `NOT_AUTHORIZED` |

## Agent 3 — `P2-W0D-A3-LOCAL-POPPLER-INVENTORY`

- Continue `task/P2-A3-corpus-oracle` in its assigned worktree.
- Objective: inventory the pre-existing Codex-runtime Poppler 26.07.0 candidate as a possible internal-only render/structure oracle.
- Allowed Paths: `poc/oracles/candidate-research/local-poppler/**` only.
- Permitted: version-only invocation of existing `pdfinfo.exe` and `pdftoppm.exe`; read-only binary/dependency/signature/hash/license inventory.
- Prohibited: PDF/document processing, binary copying, download/install/update, product reference, Sandbox launch, paid service, restart.
- Acceptance: exact paths/versions/hashes/dependency inventory, origin/lifecycle/license gaps, reproducibility/removal risks, and recommendation whether it can enter a later bounded PoC-only admission.

## Agent 6 — `P2-W0D-A6-SECURITY-PACKET-REVIEW`

- Branch/worktree: `task/P2-A6-wave0d-review`; `worktrees/phase2-20260905/agent-6-wave0d-review`.
- Allowed Paths: `poc/reviews/security/wave0d/**` only.
- Objective: security review of Windows Sandbox template, proposed exact PDFsharp dependency metadata, qpdf/MuPDF oracle research, and known local Poppler candidate facts.
- Acceptance: threat/gate matrix, component allowlist restrictions, no-network/child-process/evidence plan, post-restart checks, remaining hard blockers, and recommendation for the exact next acquisition packet.
- Boundary: static/metadata only; no binaries acquired/loaded, no PDF processed, no system mutation/restart, no paid component.

## Agent 7 — `P2-W0D-A7-GOVERNANCE-PACKET-REVIEW`

- Branch/worktree: `task/P2-A7-wave0d-review`; `worktrees/phase2-20260905/agent-7-wave0d-review`.
- Allowed Paths: `poc/reviews/governance/wave0d/**` only.
- Objective: license/provenance/no-cost/evidence review of the three exact PDFsharp dependencies, qpdf 12.4.1, MuPDF 1.28.3, and pre-existing Poppler 26.07.0 candidate.
- Acceptance: component-by-component acquisition/use recommendation, MIT/Apache/GPL/AGPL obligations and gaps, no-cost disposition, exact human decision wording, and what must be deferred.
- Boundary: engineering governance, not legal advice; no acquisition/execution/product reference/restart/paid component.

## Agent 0 checkpoint

Agent 0 integrates accepted records and issues one exact no-cost packet. Sandbox launch and all document processing remain blocked until a separately approved restart, launcher/effective-control verification, exact component admission, and execution freeze.
