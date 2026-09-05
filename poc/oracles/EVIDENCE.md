# Agent 3 Wave 0A verification evidence

Task: P2-W0A-A3-CORPUS-ORACLE; Run: P02-20260905-1434.
Baseline: 5769ccce917d03905d07a1440ee527cb653c9cc4.
Branch: task/P2-A3-corpus-oracle. Worktree: assigned agent-3-corpus-oracle.
No central records or product dependency graph modified. No merge to main.

Commands run from assigned worktree:

| Command/check | Result |
|---|---|
| python -B poc/oracles/test_foundation.py | exit 0; 17 tests, 0 failures, 0 errors |
| python -B poc/corpus/generate.py | exit 0; generated in-memory fixture digests |
| Two separate generator processes, exact stdout comparison | PASS; identical |
| pwsh -NoProfile -File eng/security/Invoke-SecretScan.ps1 -Root poc | exit 0; 2 JSON files scanned at initial check, 0 findings |
| Python privacy scan of generator/schema/contract/test sources, manifest, expectations, benign bytes and result record | PASS; synthetic leak negative control detected |
| AST import allowlist | PASS; stdlib and local modules only |
| Generator xref offsets and stream lengths | PASS; serialization self-check only |

Privacy scan is heuristic, not certification that arbitrary future content is safe.
The repository scanner excludes Python; the Python self-test supplies complementary
source coverage. No real document was used. Negative records, not hostile PDFs,
were exercised. No candidate, independent parser, renderer, network capture or
resource benchmark ran. No performance numeric threshold was introduced.
Observed short self-test time is not a product performance measurement.

Artifact binding: generator-evidence.json holds deterministic manifest, expectation
and fixture SHA-256 values. source-digests.json binds implementation/specification
files; this evidence note and the digest index itself are excluded to avoid cycles.
Git commit binds the complete handoff. No PDF binary is retained.

Preparation acceptance: PASS, subject to Agent 0 audit. Engine disposition and
Phase 2 exit: NOT_CLAIMED. Later generation to disk/processing requires the frozen
manifest, exact retention/access policy, environment, admitted independent verifier
and execution record. No added cost incurred. No implementation blocker remains.

Integration: Agent 0 reviews only poc/corpus and poc/oracles, integrates alongside
Agent 1's harness without adding the corpus to product dependency graph, then
sends the exact frozen input packet to Agent 6/7. This comparator must not be used
as the real PDF validation or publication gate.
