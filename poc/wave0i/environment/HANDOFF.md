# Agent 1 Wave 0I handoff

Baseline f6fa411bdc5415099d6a74ea356a5faab224df1f; task/P2-A1-wave0i,
fresh agent-1-wave0i worktree. Only poc/wave0i/environment changed.
Scope POC-PDF-001/FEAT-PDF-005; PDF001-E00, PDF001-E08 preparation.

Files: Environment.psm1, Invoke-GuestChild.ps1, Test-Staging.ps1,
Test-Static.ps1, INTERFACE.md, HANDOFF.md.
Concrete config renderer maps dedicated verified read-only input and empty export
siblings; guest-private case output; hash-bound child with timeout and best-effort
tree stop; fixed metadata JSON export; exact staging/invocation/close interface.

Verification: `pwsh -NoProfile -File poc/wave0i/environment/Test-Static.ps1`
8/8 PASS, exit0, no child execution/PDF generation. Pure record rejection and
PowerShell AST syntax validation only. Protected-path diff empty, exit0.
No engine/build/restore/download/Sandbox/system/paid operation occurred.

Risks: writable export mapping is narrow but not write-only; compromised guest
may write arbitrary bytes. Root quarantine review mandatory. No hard disk/memory
containment or Job Object; taskkill has descendant/PID race limitations. No runtime
timeout/staging/export evidence yet. Existing authority persists; not permission
hold. Review with Agent6 before root tiny benign smoke; do not use hostile input.
No full offline/performance/PoC success claimed. Resource bounds are smoke defaults.

Integration: root reconciles Agent2 asset manifest/runtime paths and Agent3 observer,
audits with independent reviewers, stages fresh private bundle, then runs controlled
guest smoke. All outputs stay untrusted pending independent validation. Details
and runnable commands are in INTERFACE.md. No message sent to old manager.
