# Agent 2 / Wave 0I handoff

Objective: concrete offline compilation and private guest runtime staging for existing MergeSmoke, without executing a compiler/engine during preparation.

Baseline: f6fa411bdc5415099d6a74ea356a5faab224df1f.
Branch: task/P2-A2-wave0i.
Worktree: worktrees/phase2-20260905/agent-2-wave0i.
Initial worktree clean. Only poc/wave0i/candidate/** changed; no product, central record, other lane or main writes.

Trace: POC-PDF-001, FEAT-PDF-005, TECH-DEC-005, TECH-DEC-006, P2-W0H-EXEC-001, PRE-006, PRE-009, PDF001-E00. AB/CAB/AA/AC support preparation for UT-MERGE-003, UT-MERGE-004, UT-MERGE-005, UT-MERGE-006, UT-MERGE-007, UT-MERGE-009 and TP-MRG-001, TP-MRG-006, TP-MRG-007, TP-MRG-008. None is claimed passed by static build-script tests.

Files: Build-Offline.ps1, CandidateTools.ps1, Test-Static.ps1, inputs.lock.json, README.md, HANDOFF.md, static-test-results.json.

Implemented: default read-only verification, pinned source/notice/archive/member/toolchain hashes, direct compiler response command with no restore/targets/analyzers, exact private runtime staging, explicit deps/runtimeconfig, fresh-output-only writing, compiler timeout/tree termination, bundle hashes after successful compilation. Runnable operator command and environment interface are in README.

Verification command actually executed: & ./poc/wave0i/candidate/Test-Static.ps1. Exit 0, 8/8 PASS. Tests include PowerShell parser checks, path traversal/UNC/ADS rejection, missing bytes, altered digest/size, ZIP missing/duplicate/case mismatch/member digest rejection, compiler response injection, runtime/deps constraints and real static input verification. Results in static-test-results.json. Real verification covers 365 installed files, four package archives, twelve library members, source and notice. Additional read-only PE-header check: dotnet.exe, hostfxr.dll and coreclr.dll are x64. Static reference-name inspection found no duplicate framework/candidate library names.

Security/privacy: text-only in-memory ZIP fixtures; no PDF fixtures generated, no package executable loaded, no build/restore/acquisition/download/network request/production mutation. Source and legal text use normalized UTF8/LF hash binding for checkout newline portability. Package and installed binary hashes use raw bytes. No raw document content or credentials appear in evidence. No new costs.

Performance/resource evidence: runtime staging inventory totals 80,370,072 bytes across 191 host/runtime files. No execution/performance or effective isolation measurement is claimed.

Status: IMPLEMENTATION_PREPARED_STATIC_TESTS_PASS / BUILD_AND_ENGINE_NOT_RUN. Build/loader success and guest containment/oracle evidence remain for root. No new human decision is requested by this handoff; current authorization remains valid. Existing bounded security/provenance limitations are not waived.

Integration: Agent 0 reviews this lane with Agent 1 environment and Agent 3 oracle, then independent Agent 6/7 reviews. Root alone invokes Build mode after technical review, checks bundle.manifest.json and stages only guest plus its manifest. Any missing/changed byte fails closed; no automatic refresh/download/restore. Full Phase 2 exit remains outstanding.
