# Agent 6 static security review — P2-W0B-A6-SECURITY-REVIEW

Recommendation: BLOCKED_SECURITY for processing admission. Static review deliverable complete; no confirmed exploit or runtime failure is asserted. This is not human approval. ZERO_ADDITIONAL_COST preserved.

Run P02-20260905-1434; reviewed 2026-09-05. Input commit 42115b25c153717586f74a400a3cdc16c542f1af; branch task/P2-A6-security-review. Only poc/reviews/security changed. No product reference, package restore, installation, download, assembly loading, candidate execution, document processing, or merge performed.

## Reproduction and evidence

Use installed Python 3.14.0, standard library only:

```powershell
python -B poc/reviews/security/static_review.py <approved-pdfsharp.6.2.4.nupkg>
```

The approved artifact is identified by the task card, not an arbitrary package. Script refuses any different size/hash. Command exit 0; output saved as static-evidence.json. Archive is read in memory without extraction. SHA-256 before and after: 5647882339B327CBF6DA9EA3F882EFB335F8D602E6FD45323ADF9E78DA53CF67; size 4,625,326 bytes. 111 entries; CRC reads, case-insensitive duplicate, traversal/root/colon/backslash/trailing-dot-space and symlink checks passed. 36 DLL entries have a PE CLI header. No matched runtimes/native/build/script path among inventory. These checks are specific to this exact artifact; not a universal ZIP safety validator or proof all code is memory-safe/managed-only.

.signature.p7s exists. Cryptographic signer trust, timestamp, chain/revocation and source authenticity verification NOT_RUN. Digest agreement binds the supplied artifact; it does not independently authenticate its publisher. Exact transitive graph/SBOM and vulnerability disposition remain pending.

Get-Command Get-MpComputerStatus found ConfigDefender. Get-MpComputerStatus and Get-MpPreference returned Access denied (exit 1). Scanner NOT_RUN: signature currency, service state, no-remediation and no-upload settings could not be verified. No Defender settings changed and no sample submitted. No claim of malware-clean or current CVE clearance.

Baseline/status commands with command-scoped safe.directory returned exact input HEAD and assigned branch; initial worktree was clean. No global Git setting changed. Initial review-directory write failed on OS permissions; bounded escalated write to the already authorized path succeeded. This does not confer candidate execution authority.

## Static findings, ordered by gate impact

- S01 — High admission gap: network vocabulary HttpClient/System.Net in Cryptography net8/net9/net10 and Quality all four TFMs; Quality also contains ProcessStartInfo/ShellExecute vocabulary. These are byte/string indicators, not proven reachable calls or egress. Do not infer Merge uploads or launches processes. Before run, resolve exact selected assembly/dependency closure, inspect relevant call paths with approved tooling/source, constrain unrelated helpers, and independently observe attempted and successful egress/process creation.
- S02 — High admission gap: no implemented sandbox/job supervision/network enforcement/watchdog/atomic publisher in harness. EnvironmentManifest explicitly returns NetworkDisabledVerified=false and ProcessingAuthorized=false. Current CLI only exposes manifest/self-test. Good preparation boundary; cannot prove runtime containment.
- S03 — High admission gap: contract.py compares synthetic observations only; returns publishAuthorized=false even MATCH_ONLY. Independent parser/renderer not admitted. Actual result validation/publication stays blocked.
- S04 — Medium future executor concern: OwnedPath rejects traversal, alternate-stream syntax and existing reparse ancestors, but remains string/check-time advice, not handle-bound authorization. Reserved device names, exclusive root reservation, ACLs and TOCTOU defenses remain required. Lifecycle is unsynchronized and not a concurrent supervisor; serial model tests cannot establish cancel/publish race safety. No current filesystem publisher exists to exploit in this skeleton.
- S05 — Medium admission gap: signature/scanner/vulnerability and transitive/native graph coverage incomplete. Presence of CLI header is not a P/Invoke audit. PdfSharp.dll net8/net9/net10 has Marshal vocabulary; Unsafe vocabulary appears in core/Cryptography/Snippets. No matched DllImport/library names, dynamic-load or telemetry/activation patterns in this narrow scan; absence is NOT proof of absence. IL, ImplMap, callgraph, obfuscation, reflection-computed names and transitive binaries were not decoded/reviewed.
- S06 — Active-content semantics unresolved: core DLL includes OpenAction, Launch, EmbeddedFile, GoToR, URI vocabulary. Serialization/support for actions is not evidence that actions execute. Freeze reject/preserve/strip behavior with Security/Quality authority; test both processing and downstream output safety. No active-content safety PASS.
- S07 — Future untrusted-record limit gap: schemas lack maximum byte/string/array/depth bounds; compare is a synthetic-only helper, not hardened hostile JSON ingress. Require bounded deserialization and schema inputs before adopting it as a real verifier boundary. Do not promote generator token absence checks to malicious-PDF detection.

## Harness and corpus review

Read Program.cs, SelfTests.cs, harness README/project, Verify-Harness.ps1, corpus generator/README, schemas.py, contract.py and test_foundation.py. No candidate code is invoked by the reviewed preparation sources. Verification script would restore/build the harness; it was NOT run in this static task. Generator accepts only fixed A/B/C recipes, uses standard-library deterministic in-memory serialization and synthetic markers. It has no document input/network/subprocess operation. Existing tests cover changed observations, missing/extra/order/duplicate/reopen/inconclusive cases; their previous results are not re-certified here. Corpus binaries were not generated or processed in this review.

Negative follow-up plan: unknown/mismatched verifier identity and digest, fabricated expected observations, oversized/deep records, duplicate occurrences, stale input/output replacement, invalid/truncated result, password-error leakage, cross-run temp collision, reparse/target alias races. All require frozen scope; no hostile fixtures executed here.

## Threat-to-experiment matrix

All runtime experiments NOT_RUN. Listed proofs are required next; none can be inferred from static vocabulary.

| Exact ID | Threat/control and required evidence |
|---|---|
| PDF001-E00 | Admission/supply chain and egress: signature/graph/SBOM disposition, selected assembly closure, OS deny plus attempt observation across process tree; restore separated from processing. |
| PDF001-E01 | Loss/corruption/leakage: independent page/order/content/reopen and before/after source digests; sanitized success path. TP-MRG-001. |
| PDF001-E02 | Silent dedupe/order error: C,A,B and A,A occurrence provenance, no missing/extra page. TP-MRG-006, TP-MRG-008. |
| PDF001-E03 | Fonts/images/forms/annotations: independent fidelity and approved active-content preservation policy; verifier isolated too. |
| PDF001-E04 | Password missing/wrong/correct: operation-memory-only, no command line/log/temp/checkpoint/dump persistence; synthetic canary exclusion. TP-MRG-030, TP-MRG-033. |
| PDF001-E05 | Malformed/polyglot/actions/path/network: safe rejection, no final, bounded process/file/network observation in approved isolated environment. TP-MRG-005, TP-MRG-031. |
| PDF001-E06 | Empty/truncated/wrong/corrupt output or verifier failure: independent veto; no publication on inconclusive. TP-MRG-028, TP-MRG-029. |
| PDF001-E07 | Traversal/alias/TOCTOU/overwrite/write/rename: exact file identity, unchanged source/existing target, atomic validated publication. TP-MRG-025, TP-MRG-026, TP-MRG-027. |
| PDF001-E08 | Timeout/cancel/crash/late completion: supervised process-tree stop, terminal consistency, run-owned reconciliation; no broad deletion. TP-MRG-019, TP-MRG-020, TP-MRG-021, TP-MRG-022, TP-MRG-023. |
| PDF001-E09 | Bomb/resource exhaustion: approved corpus bands, dedicated quota, measured CPU/RAM/GC/disk/temp/handles/time, correctness under load. TP-MRG-016, TP-MRG-017, TP-MRG-018, TP-MRG-024. |
| PDF001-E10 | Concurrency/collision/leaks: controlled barriers, distinct run roots, repeatability, aggregate/per-run residual and drain. Serial Lifecycle is insufficient. |
| PDF001-E11 | Vendor/dynamic loading/error leak: adapter-only closure, vendor-neutral contracts, safe errors; actual selected references verified. |
| PDF001-E12 | Removal/fallback egress: unavailable or explicitly approved local fallback, no server route, repeat source/finality/no-egress checks. |

## Zero-egress, canary and measurement plan

Freeze free isolated non-Production guest/recovery point; least-privilege identity; separate exact input/output/temp/evidence roots; deny all processing network before runner starts. Verify observation with approved inert synthetic probe before candidate use. Record attempted/denied as well as successful DNS/socket/HTTP activity and child processes, with observer health and blind spots. A disconnected guest alone does not show connection attempts. No public test endpoint or content upload; no exception for activation/telemetry. Scanner/oracle must obey the same boundary. Configuration is not proof.

Use approved per-run synthetic password/content/path canaries, no real secret/document. Allowlisted records carry opaque IDs, status/count/digest, never matched content. Content in authorized input/result is distinguished from leakage in logs/metadata/dumps. Scan success/failure/cancel/crash/restart paths and evidence before release; sanitizer/observer failure blocks publication of evidence. No full dump collected by default.

Freeze hardware/software/candidate/corpus/config/instrumentation digests, sampling, cold/warm protocol and repeat count. Measure idle and harness overhead, then approved bands, one stress dimension at a time. Record all runs/outliers, CPU time, memory/GC, handles/threads, I/O/peak temp, stage latency, cancellation and cleanup/drain residual. No measurements were made here. Numeric budgets (including emergency safety ceilings, timeout/grace, file/page/object/decompressed/output size, concurrency/queue/retry, sampling/repetition/soak, retention and residual bands) need human approval before their dependent run. MEASURED_BASELINE is not numeric acceptance; zero processing egress is already canonical.

## Gates and stop authority

PRE-004/PRE-005: exact closure/provenance/license/SBOM pending. PRE-006: resolve indicators, scanner disposition and runtime controls. PRE-007/PRE-008: freeze complete corpus/independent oracle. PRE-009/PRE-010: isolation, instrumentation and safety limits. PRE-011/PRE-012: run evidence policy and explicit processing authorization. P2-D01/P2-D02/P2-D03 remain scoped human decisions; acquisition approval does not close them.

Stop affected work on changed hash/source/graph, added cost, unapproved dependency/tool, hostile main-host execution, lost telemetry, egress/active execution, source risk, unbounded resource/cleanup, leakage, missing oracle or changed frozen context. Operator stops and contains; Agent 0 integrates evidence; Workspace User temporary authority decides resumption/risk, without organizational sign-off claims. Recommendation BLOCKED_SECURITY is for processing; static evidence may be integrated now after Agent 0 review. No numeric threshold, waiver or human approval invented.

Sources: AGENTS.md; Shared Agent Contract; Phase 2 prompt/task card/state/decision register; Security Design sections 19/20/23; Test Plan Merge/privacy/resource sections; POC Master Plan and POC-PDF-001 charter. Review uses exact local artifacts; no current external advisory claim.
