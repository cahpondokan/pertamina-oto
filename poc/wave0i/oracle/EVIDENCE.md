# Agent 3 Wave 0I handoff evidence

Date: 2026-09-08 (Asia/Jakarta).
Task: concrete independent marker observation route and safe negative self-tests.
Baseline: f6fa411bdc5415099d6a74ea356a5faab224df1f.
Branch: task/P2-A3-wave0i.
Fresh worktree: worktrees/phase2-20260905/agent-3-wave0i.
Allowed/changed paths: poc/wave0i/oracle/observe.py, test_observe.py, README.md, EVIDENCE.md only.

Read current Wave0I task card, Shared Contract, Phase2, status/register, prior Wave0H Agent6/7 reviews, MergeSmoke/generator/oracle/spec, canonical UT/TP exact IDs and charter oracle requirements. New execution permission remains valid; old snapshots are unchanged. No central records, product source, other lane or main modified. No agents delegated from this lane.

## Actual safe verification

Command from this fresh worktree:

```text
C:\Python314\python.exe -B -m unittest discover -s poc/wave0i/oracle -p test_observe.py -v
```

Final code run: exit 0; 28 tests discovered, 28 passed; 0 failures/errors; unittest elapsed 0.055 seconds. Earlier 25-test checkpoint also passed before adding three provenance/timeout tests. Timing is self-test timing, not PDF processing/performance evidence.

Tests actually cover four marker cases aligned with Wave0H; unsupported page/font/content schema; missing/cyclic/duplicate references; incorrect ordering/count and marker programs; empty/duplicate-key/nonfinite/oversized JSON; unknown schema/type/merger; source/tool/output hash mismatch and output/source alias using mocked reads of in-memory text; qpdf warning/empty/stdout/stderr rejection; bounded text file reads; mocked timeout/oversize child kill/reap. Every Popen is guarded or replaced by a mock. No PDF container bytes were generated/read; no qpdf/PDFsharp executable, guest, generator, compiler, build or restore was run. No new dependency, download, paid service or product reference.

## Implemented controls and practical limits

Runtime observer directly captures qpdf version/JSON rather than trusting supplied marker JSON. Root supplies a separately frozen exact binding digest; observer checks strict schema and supported merger identity, bundle inventory/member hashes, all source and output hashes before and after parsing, observer script hash, and actual capture hashes. CLI errors print fixed codes. Current directory/environment are private and restricted; raw stderr remains quarantined evidence.

Root ledger is the trust anchor: it must bind pre-merge source hashes, admitted executable/DLL bytes, actual PDFsharp invocation/output, script and binding digest. This code does not authenticate operator assertions, sign receipts, bind loaded system modules, prove runtime isolation or prevent races against a concurrent guest adversary. Direct-child polling/kill is implemented, not hard memory/disk/process-tree containment. Outer Agent1/root supervisor still required.

Ready for integrated Agent6/7 source review. Agent0 is the sole next operator. Concrete runtime prerequisites: existing compatible Python in guest, staged admitted qpdf binary closure, generated/frozen benign sources and PDFsharp output, independently retained binding digest, reviewed outer supervision/export route. Runtime PDFsharp-output compatibility remains untested and may fail closed on an unsupported structure; do not describe this as a measured independent oracle PASS. No new human authorization requested by this lane.

Integration order: Agent0 integrates candidate/environment preparation and this oracle; Agent6/7 review the combined interface; Agent0 stages actual bytes and runs under existing bounded permission only when technical readiness holds. Phase2 exit, engine admission, fidelity, UI flows and Production are not asserted. Exact IDs and partial-coverage caveats are in README.md.
