# PERTAMINA OTO — Phase 2 Wave 0F qpdf Static Intake

| Field | Value |
|---|---|
| Run | `P02-20260905-1434` |
| Authorized at | 2026-09-07 12:09:00 +07:00 |
| Human approval | Workspace User explicitly approved download and static inspection of qpdf 12.4.1 at zero cost; no install or execution before audit completes |
| Input checkpoint | `integration/P2-proof-capability` @ `7fbcada232c0009a27a226f8f1d9d80cbe3c57e6` |
| Exact asset | `qpdf-12.4.1-msvc64.zip` |
| Exact URL | `https://github.com/qpdf/qpdf/releases/download/v12.4.1/qpdf-12.4.1-msvc64.zip` |
| Expected SHA-256 | `3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6` |
| Cost boundary | `ZERO_ADDITIONAL_COST` |
| Install / execution / PDF processing | `NOT_AUTHORIZED` |
| Restart / Sandbox launch | `NOT_AUTHORIZED` |

## Agent 3 — `P2-W0F-A3-QPDF-STATIC-INTAKE`

- Continue the existing Agent 3 Phase 2 branch/worktree from its qpdf blocker commit.
- Objective: acquire exactly the approved ZIP into the run-owned evidence directory and inspect it without loading or executing archive content.
- Raw evidence: `D:\OTO\pertamina-oto\artifacts\evidence\P02-20260905-1434\agent-3-qpdf-intake`.
- Allowed tracked Paths: `poc/oracles/qpdf-12.4.1/**` only.
- Required evidence: request time/status/effective URL/redirect record; exact byte size and SHA-256; safe ZIP inventory with per-entry hashes; traversal/duplicate/symlink checks; executable/DLL/import/static string inventory without loading; license/notices/provenance/signature/SBOM candidates; no-cost result and limitations.
- Prohibited: installer, extraction to PATH/search locations, `qpdf.exe` or DLL loading, PDF/document processing, product reference, restore/build, extra component, MuPDF/Poppler use, paid service/license/support, restart, or Sandbox launch.
- Stop: URL/hash/source mismatch, unexpected redirect/asset/dependency/active content/cost, or requirement to execute.

## Agent 6 and Agent 7 — independent review

After Agent 0 audits and integrates Agent 3's static intake:

- Agent 6 independently rehashes the exact ZIP and entries and reviews binary/import/activation/trust/vulnerability limitations without execution.
- Agent 7 independently reviews Apache-2.0 and every bundled license/notice/provenance/redistribution/no-cost obligation.
- Each reviewer writes only its new Wave 0F review path in a separate worktree and commits a handoff.
- Agent 0 integrates accepted reviews and issues the next decision packet.

No qpdf execution, PDF processing, restore/build, restart, or Sandbox launch occurs in
Wave 0F. Static review success is not engine approval.

