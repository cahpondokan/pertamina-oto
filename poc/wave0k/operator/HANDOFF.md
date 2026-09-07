# Agent 1 / Wave0K guest source handoff

Baseline 16a53801020ab4571bfa09b3bdd39961eabf9a99; task/P2-A1-wave0i worktree. Existing untracked poc/wave0j preserved. No commit, download, engine, Sandbox, or PDF execution performed.

Scope POC-PDF-001 / FEAT-PDF-005 partial structural count/order evidence, P2-W0H-EXEC-001 standing authority; no canonical test PASS or disposition claimed.

New files: Guest-Oracle.ps1, Test-Static.ps1, this handoff under poc/wave0k/guest only.

Guest source SHA256 FC256F051E489A877F799785909D078905EB4C9F75B61A06E2DB2F1C23CCF14A. Test SHA256 883D45B9D092C10EFD96978CEE674148C8FCC658463F82901702D8FCC31EAB63.

Interface: CaseId AB/CAB/AA/AC, LedgerSha256 binds C:\PocInput\corpus\sources.json, QpdfManifestSha256 binds C:\PocInput\qpdf-files.json plain filename/lowercase-hash object. Root separately verifies complete immutable runtime/stage manifest before/after each guest; read-only mapping required. Fixed python/python.exe, qpdf flat directory, oracle/observe.py. Clean child environment, Python -I -S -B. NIC absence gate; fixed private paths; source hash before/after. No arbitrary input paths.

One merge child 15 seconds, observer child 45 seconds. Timeout tree cleanup best-effort up to 12 seconds; outer root watchdog and exact guest disposal mandatory. No hard Job-object/memory/disk quota claim.

Fresh empty C:\PocExport quarantine receives only CASE.binding.json, CASE.result.json, CASE.version.stdout, CASE.version.stderr, CASE.observation.stdout, CASE.observation.stderr. Each max 1 MiB, empty only stderr. Partial failure captures allowed but never success: exit 0 requires all six copied, observer exit 0 and source checks. Exit31 merge,32 observer/postcheck,33 incomplete export,34 export fault,27 setup; 20/21/24 environment/network/notfresh. No PDF exported. Independent host validation remains mandatory; source/tool hashes and output hash guest-bound, not signed attestation. Fidelity NOT_ASSESSED.

Verification: Windows PowerShell 5 inline evaluation of Test-Static.ps1 yielded PASS 7 parse/pure-bound checks. -File first blocked by execution policy; did not change policy. Tests cover zero/nonzero/max/oversize/negative export size and syntax. No real I/O/engine behavior proved by those tests. git diff --check clean for tracked files (new files untracked). Root integration + independent source review must precede execution. No new human authority requested.
