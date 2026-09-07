# Agent 7 Wave 0I independent governance handoff

Status: REVIEW_COMPLETE; BUILD_ONLY_GO_WITH_BOUNDARIES / ENGINE_GUEST_ADMISSION_PENDING.
Agent / task: Agent 7 / Wave 0I governance.
Objective: review whether root can build a nonexecuted runner using existing bytes, separately from guest engine admission.
Baseline: c9dec05a204e79a4420d556f691e4ea744df2ca2, initially clean.
Branch/worktree: task/P2-A7-wave0i / worktrees/phase2-20260905/agent-7-wave0i.
Files changed: poc/wave0i/governance/HANDOFF.md only. No product/main/central record writes.

## Build-only disposition

Agent 0 may proceed with the reviewed offline Build mode under continuing P2-W0H-EXEC-001 after integrating independent security findings. No renewed execution permission is needed. This covers private compilation/staging only: it executes the existing compiler/runtime, reads candidate assemblies as metadata, and produces a runner that remains unexecuted. It does not authorize automatic engine launch or imply engine success.

Reviewed concrete operator command from the integrated worktree:

```powershell
& ./poc/wave0i/candidate/Build-Offline.ps1 -Mode Build -OutputDirectory (Join-Path $env:TEMP 'OTO-W0I-PDFsharp-build-001')
```

The target must be new and private; choose a fresh run name if occupied. No overwrite, cleanup, package resolution, restore, new acquisition, paid service, product reference or Production action. Do not silently change the lock to accommodate drift. Build failure leaves diagnostic partial output, not a valid bundle. Promote only a successful BUILT_NOT_EXECUTED bundle with recorded input lock, output manifest, compiler receipt and hashes. Compiler logs stay private and must be reviewed for paths/content before promotion.

Observed implementation: pinned lock/source/notice, 365 installed file hashes, four full archive hashes and twelve library member hashes; exact .NET 10.0.11 / SDK10.0.400 compiler; explicit references; direct csc invocation, no MSBuild/targets/analyzers/source generators. New-file writes, rejected reparse paths, cleared child environment, no shell interpretation, telemetry opt-out and a 60-second compiler timeout/tree-kill attempt reduce accidental execution channels. No network-request command exists in this build path. This is an offline build design, not measured host egress containment. Hash equality is not publisher/security clearance; fresh private staging is not race-proof against a hostile same-account process. These limits do not create an unrelated guest-oracle prerequisite for compiling the runner.

Notice retention is concrete: original package legal records plus bound PDFsharp notice and installed .NET notices accompany the private evaluation bundle. No blanket redistribution clearance is required for this build-only evaluation, and none is granted. qpdf remains a distinct observer dependency with its previously recorded component/provenance limitations; it is not part of the four-package compiler input closure.

## Runtime evidence and scope

Read docs/project-management/PHASE_02_WAVE_0I_TASK_CARDS.md and PHASE_02_WAVE_0I_SANDBOX_OBSERVATION.md with continuing Wave0H authorization/contract/Phase2 scope. The operator's recorded second run query returned ExitCode0: its source counts non-loopback interfaces and exits0 only for zero. Session stop followed by empty list is closure evidence for that recorded run. About150 seconds met the300-second target; first run about336 seconds did not. Preserve both outcomes. Logon summary remained NOT_OBSERVED; no screenshot-digest/visual PASS is claimed. These are reviewed operator records, not independently replayed telemetry.

Historical648MiB resource hold is no longer the latest readiness evidence. The later6088MiB and subsequent5110MiB readings and4608MiB benign guard describe past observations only. Recheck resources immediately before a new engine session; do not assume a benign guard is a product performance threshold. No user application closure, restart or host security-setting change is authorized here.

## Engine guest admission remains separate

| Exact remaining item | Owner / required evidence |
|---|---|
| Build/runtime result | Agent0: successful compiler receipt and fresh bundle hashes; actual private runtime/loader behavior remains untested. Compilation success alone is not dependency-load success. |
| Staging contract | Agent0/environment: reconcile candidate guest/dotnet and guest/app with environment example runtime/app paths, hash every staged input including qpdf/Python/scripts/corpus; verify no extras/reparse paths, ownership, narrow read-only input and initially empty private export. Do not map repo/SDK/TEMP roots. |
| Observer export | Agent0/oracle/environment: environment wrapper accepts only four small CASE.json receipts while observer produces result.json and raw version/observation stdout/stderr. Define an explicit bounded sanitized export/ledger mapping for observer raw evidence; current receipt-only path does not prove independent markers. Never auto-open arbitrary quarantine output. |
| Supervision | Agent0/Agent6: bind exact session ID to finally/timeout stop and verify absence; decide adequacy of best-effort child termination for tiny benign input. No Job Object or hard memory/disk quota is implemented. Observer requires two15-second children plus cleanup; reconcile its budget with wrapper deadline and total session target rather than silently accepting timeout. |
| Actual independent result | Agent0/oracle reviewer: source ledger before merge, actual merger/tool hashes, independent qpdf observations, output and capture digests, failure/timeout evidence. JSON fixtures/mocks do not prove real PDFsharp output compatibility. Reject unsupported structure rather than broaden parser to pass. |
| Limited security/provenance | Agent0/Agent6/technical authority: preserve known parser exposure and qpdf provenance limits in bounded evaluation admission. This is technical disposition under existing permission, not another permission request or shipping approval. |

Read the environment and oracle interfaces as concrete proposals with honest limits. Writable quarantine is not write-only containment. No-interface query does not prove attempted-egress blocking, every redirection, arbitrary-input safety, visual fidelity, cleanup semantics, or full PoC. Observer is independent of PDFsharp only when actual run receipts establish merger identity; qpdf self-observation is excluded. Four-case structural evidence cannot close UT-MERGE-005 initial A/B/C ordering or UI reorder/removal behavior in full.

## Evidence binding and checks

Input Git blobs at baseline:

| File under poc/wave0i | Blob |
|---|---|
| candidate/Build-Offline.ps1 | bc0c89f6949658f15f87cf2687784a02b49cba2b |
| candidate/CandidateTools.ps1 | 43b2108a8da4166dd3c62bedf50cb31ff0461932 |
| candidate/inputs.lock.json | 8a5377707f70ebcf506b3c5dee80fb54ed428bb6 |
| environment/Environment.psm1 | 339ea173b0302f534bbebbd54f470cc9016f87e6 |
| environment/Invoke-GuestChild.ps1 | 7c0f28a1d7a890029b455e20cdbf18974cfe286b |
| oracle/observe.py | d3a1a627f3ad240f0dac12148d0b9ffa3195fbcd |

Executed verification: `& ./poc/wave0i/candidate/Build-Offline.ps1 -Mode Verify` exit0, JSON status PASS, mode STATIC_INPUT_VERIFICATION, dotnetFiles365, packages4, members12, sourceBound true, compilerExecuted false, engineExecuted false. This reviewer independently rechecked actual bytes; no Build mode or engine process ran. Git status remained clean after Verify. No need to rerun writer tests that write into another lane; their8/8 static tests and oracle mocks remain attributed preparation evidence, not independently rerun results.

Requirement/decision trace: P2-W0H-EXEC-001, POC-PDF-001, FEAT-PDF-005, TECH-DEC-005, TECH-DEC-006, PRE-006, PRE-008, PRE-009, PDF001-E00. Structural support only: UT-MERGE-003, UT-MERGE-004, UT-MERGE-005, UT-MERGE-006, UT-MERGE-007, UT-MERGE-009, TP-MRG-001, TP-MRG-006, TP-MRG-007, TP-MRG-008, TP-MRG-029. No canonical test or Phase2 exit passed here.
Behavior implemented: review artifact only. Security/privacy: no acquisition, launch, build, engine, PDF generation/processing, paid service or real data; source evidence untouched. Performance: no new measurement, only attributed operator observations and installed-byte verification. Known limitations: runtime/compiler success and engine evidence outstanding; no distribution/visual/full portfolio sign-off.
Recommended integration order: Agent0 audits this scoped commit with Agent6, performs permitted Build-only step if no concrete security blocker, records exact outcome, then resolves guest-specific admission items before engine run. No central/main mutation or new approval request from this lane.
