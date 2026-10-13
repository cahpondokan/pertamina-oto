# PERTAMINA OTO — Phase 2 Readiness and Decision Packet

| Field | Value |
|---|---|
| Run ID | `P02-20260905-1434` |
| Phase | 2 — Proof of Capability |
| Command | `kerjakan phase 2` |
| Integrated at | 2026-09-05 14:40:41 +07:00 |
| Agent 0 disposition | `BLOCKED_PRE_EXECUTION` |
| Execution performed | No — readiness analysis only |
| Product baseline | `D:\OTO\pertamina-oto`, clean `main` @ `5769ccce917d03905d07a1440ee527cb653c9cc4` |
| Program progress | 2/12 phases = 16.67% |

## Executive result

Phase 1 entry evidence is valid, but Phase 2 cannot yet execute a real provider. All seven specialist identities ran as separate sidebar tasks and completed read-only handoffs. Agent 0 audited and integrated their findings into this packet.

No candidate package, native binary, model, or device was downloaded, restored, installed, or executed. No corpus bytes were generated. No product adapter or product-repository change was made. This is required because `P2-D01`, `P2-D02`, `P2-D03`, and charter conditions `PRE-003..012` do not yet have exact human approval.

`BLOCKED_PRE_EXECUTION` is a controlled gate, not a technical failure. Phase 2 remains incomplete and Phase 3 may not start.

## Actual agent execution

| Identity | Task | Sidebar task ID | Result |
|---|---|---|---|
| Agent 1 | Harness and environment readiness | `01a0707e-e610-7f33-ae84-99cea43eb724` | Completed; execution environment blocked |
| Agent 2 | PDF candidate readiness | `01a0707e-fc6c-7433-89e6-155fb25c8f71` | Completed; three evaluation candidates proposed, no winner |
| Agent 3 | Corpus, oracle, and fidelity readiness | `01a0707f-0fd9-7bc1-a26b-6c0f8296cce5` | Completed; synthetic corpus/oracle proposal |
| Agent 4 | Conversion and OCR scope readiness | `01a0707f-2688-7862-8c24-ec98c56c92fc` | Completed; release/wave recommendation |
| Agent 5 | Device, platform, and local-AI readiness | `01a0707f-405e-7cc3-82a7-c6ff2b978c71` | Completed; defer/conditional matrix |
| Agent 6 | Security and performance readiness | `01a0707f-5645-7322-a910-ed049c7d94a0` | Completed; threat and measurement matrix |
| Agent 7 | License, governance, and disposition readiness | `01a0707f-6f56-70a2-ab67-47454ad4781f` | Completed; approval/admission packet |
| Agent 0 | Gate audit and master integration | Current task | Completed readiness integration; did not bypass human gates |

All specialist handoffs reported no workspace changes and no provider execution.

## Entry and charter status

| ID | Result | Evidence or blocker |
|---|---|---|
| Phase 1 COMPLETE | `PASS` | Final report and evidence available; baseline commit exact and clean |
| PRE-001 | `PASS_LOCAL` | Official local repository and authority established by Phase 0/1 |
| PRE-002 | `PASS_FOUNDATION_ONLY` | .NET SDK `10.0.400` and existing package policy are frozen; new PoC candidate is not admitted |
| PRE-003 | `BLOCKED` | Execution task card, worktree, exact tests, writers, and independent reviewer not frozen |
| PRE-004 | `BLOCKED` | Exact candidate artifact, local digest, resolved graph, and SBOM not admitted |
| PRE-005 | `BLOCKED` | Named human license/cost review and disposition absent |
| PRE-006 | `BLOCKED` | Named human security acceptance of isolation, hostile input, egress observation, and stop authority absent |
| PRE-007 | `BLOCKED` | Synthetic corpus manifest, classification, retention, and corpus authority not approved |
| PRE-008 | `BLOCKED` | Independent oracle/version/digest and expected properties not frozen |
| PRE-009 | `BLOCKED` | Disposable environment/image/network/quota/recovery point not approved and provisioned |
| PRE-010 | `BLOCKED` | Instrumentation, sampling, repetitions, concurrency, and numeric acceptance targets not approved |
| PRE-011 | `BLOCKED` | Run-specific evidence access, retention, sanitation, integrity, and SBOM format not frozen |
| PRE-012 | `BLOCKED` | Exact human execution authorization absent |

## Wave 0 critical path

The only recommended initial execution scope is `POC-PDF-001` for `FEAT-PDF-005` (Merge PDF), experiments `PDF001-E00..PDF001-E12`. The experiment family must prove candidate admission and offline behavior, correctness/order/duplicates/fidelity, password and hostile-input handling, independent output validation, source safety and atomic publication, cancel/crash/timeout/recovery, resource and concurrency behavior, vendor-neutral boundaries, and disable/removal behavior.

A real Merge adapter must not be developed before this PoC receives a governed disposition.

## Candidate readiness proposal — not approval

| Priority | Candidate | Proposed role | Current official-source findings | Blocking review |
|---:|---|---|---|---|
| 1 | PDFsharp Core `6.2.4` | First managed .NET Merge baseline | Official NuGet lists .NET 8/9/10 and .NET Standard targets/compatibility; project documents MIT licensing | Exact acquired artifact digest, resolved dependencies, SBOM, vulnerability/maintenance review, redistribution/runtime acceptance |
| 2 | iText Core/Community `9.7.0` | Broader capability comparator after license approval | Official package targets .NET Standard 2.0; official licensing is AGPLv3 or commercial | Legal/license path, commercial need/cost, exact modules, graph, offline/runtime proof |
| 3 | qpdf `12.4.1` Windows MSVC x64 | Independent structural-transform comparator | Official release publishes Windows assets; official documentation states Apache License 2.0 | Native/subprocess admission, exact asset/digest verification, packaging/runtime/security review; not a visual renderer |

Official references checked 2026-09-05:

- PDFsharp package: <https://www.nuget.org/packages/PDFsharp/6.2.4>
- PDFsharp license: <https://docs.pdfsharp.net/General/License/License.html>
- iText package: <https://www.nuget.org/packages/itext/9.7.0>
- iText licensing: <https://itextpdf.com/how-buy/AGPLv3-license>
- qpdf release: <https://github.com/qpdf/qpdf/releases/tag/v12.4.1>
- qpdf license: <https://qpdf.readthedocs.io/en/stable/license.html>

These are evaluation candidates, not selected engines. Compatibility metadata and upstream digest claims are not substitutes for local artifact, graph, license, security, and offline verification.

## Controlled two-step admission recommendation

The charter currently requires digest and resolved dependency evidence before admission while readiness rules prohibit acquisition. To avoid silently bypassing this circular dependency, Agent 0 proposes two separately bounded decisions:

1. `ACQUISITION_ONLY`: authorize only PDFsharp `6.2.4` from the official NuGet source into an isolated PoC intake/cache. Permitted output is identity, local SHA-256, signatures/provenance where available, resolved direct/transitive/native graph, license/notice capture, SBOM candidate, and vulnerability/maintenance snapshot. No document processing, product reference, or engine disposition.
2. `APPROVED_FOR_POC_ONLY` plus `AUTHORIZED`: only after human license/security review accepts the acquired record, freeze the charter, task, environment, corpus/oracle/config, evidence policy, reviewer, and expiry; then execute `PDF001-E00..E12`.

If PDFsharp reaches a hard blocker or unacceptable limitation, iText or qpdf requires its own fresh acquisition/admission decision. This minimizes dependency and licensing exposure.

## Synthetic corpus and independent oracle proposal

The approved corpus should contain only generated, non-sensitive files and explicit provenance. Proposed groups are:

- control files with unique page markers;
- reordered and duplicated inputs, including `C,A,B` and `A,A`;
- valid structural variants: text, vector, raster, mixed size/orientation, fonts, bookmarks, annotations, and forms;
- password cases: missing, wrong, and correct synthetic password;
- invalid/corrupt inputs: fake extension, bad signature, truncation, malformed structure/xref;
- inert active/untrusted constructs: actions, JavaScript markers, links, attachments, and polyglot-like test fixtures;
- resource bands, with exact sizes/counts approved by humans rather than invented;
- controlled cancellation, crash, timeout, write/rename/quota failure cases;
- negative-oracle outputs: empty, wrong, reordered, truncated, or corrupt output.

The candidate engine may not be its own only verifier. Acceptance requires independent structural checks plus reopen/render/property checks appropriate to each file and negative self-tests proving the oracle rejects deliberately wrong output. Manual visual review is supplementary; it cannot replace page count, order, duplicate, source-integrity, and atomic-finality assertions.

No personal, operational, or Production document is permitted in the PoC corpus.

## Environment, security, and measurement proposal

Recommended default is a dedicated disposable Windows VM with a frozen image, separate experiment disk/quota, least privilege, recovery point, networking disabled, and egress observation across the entire process tree. Windows Sandbox is an alternative only if its feature and configuration are first verified; command discovery did not confirm it on the current host. The main workstation is not approved for the hostile suite.

Continuation revalidation at 2026-09-05 15:15:18 +07:00 found Windows 10 Pro, 64-bit, OS build `10.0.26200.0`; `C:\Windows\System32\WindowsSandbox.exe` was not present. Reading the online optional-feature state requires elevation and was not authorized by the continuation command. Therefore Windows Sandbox must not be assumed available, and no feature installation was attempted.

The harness must supervise child processes, provide bounded timeout/cancel/termination, track run-owned files, and preserve failure/outlier evidence. Network proof must detect attempted egress, not merely the absence of successful connections. Passwords and synthetic canary values must not appear in commands, logs, dumps, paths, or evidence.

Numeric limits for files/pages/objects/decompression, CPU/RAM/disk/temp, concurrency/queue/retry, timeout/cancel/cleanup, repetitions/soak, and fidelity remain human decisions. An initial run may collect `MEASURED_BASELINE`, but it cannot claim numeric acceptance until thresholds are approved. The canonical zero-processing-egress invariant remains mandatory.

## Recommended portfolio after the PDF critical path

| PoC family | Recommendation | Condition |
|---|---|---|
| `POC-CONV-001` Office to PDF | Wave 1A | Passive generated DOCX/XLSX/PPTX corpus and provider admission |
| `POC-OCR-001` local ID/EN OCR | Wave 1A | Local-only provider/model admission, accuracy corpus and thresholds |
| `POC-CONV-003` HTML to PDF | Wave 1B | Local/captured HTML only; arbitrary remote URL prohibited |
| `POC-CONV-004` PDF to Markdown | Wave 2 | Deterministic text-layer path first; approved local OCR fallback only |
| `POC-CONV-002` PDF to Office | Governed defer recommendation | High-fidelity/editability complexity; Product decision required |
| `POC-OCR-002` complex tables/layout | Governed defer recommendation | Clarify basic-table MVP versus complex-layout scope |
| `POC-SCAN-001` | Conditional Wave 2 or governed defer | Requires approved physical scanner/device matrix; must not silently remove Core scan scope |
| `POC-AI-LOCAL-001` | Governed defer recommendation | Optional, model/package/license/resource/security authority absent |
| Android/Web | Prepare only after Core offline stable | Curated capability matrix and platform approvals required |
| iOS | Deferred/out of first release | Unchanged; no Mac/Xcode/account/signing authority |

These are Agent 0 recommendations, not human-approved scope dispositions.

## Human decision packet required to continue

The following values must be accepted explicitly and bound to an auditable reference:

1. Scope: acquisition-only for PDFsharp `6.2.4`, then—only after intake review—`POC-PDF-001` / `FEAT-PDF-005` / `PDF001-E00..E12`.
2. Source and boundary: official NuGet only; isolated PoC intake; no product dependency, no document processing during acquisition.
3. Corpus: synthetic-only proposal above; owner, classification, access, retention, and disposal.
4. Environment: dedicated disposable Windows VM, or explicitly approved verified alternative; no-network enforcement and attempted-egress observation.
5. Human roles: named Technical Owner, Product/QA corpus authority, License/Cost Reviewer, Security Reviewer/stop authority, Environment Owner, Evidence Owner, and Execution Approver. If one person temporarily holds several roles, the conflict/independence limitation must be accepted explicitly.
6. Evidence policy: `D:\OTO\pertamina-oto\artifacts\evidence\P02-20260905-1434`, sanitized records, SHA-256 binding, access/retention/expiry.
7. Numeric threshold policy: either supply exact acceptance values or authorize baseline measurement while keeping final threshold disposition blocked.
8. Effective time, expiry, and change-control rule for candidate/environment/corpus/oracle/config changes.

Only a human can approve these items. Agent 0, Agent 6, and Agent 7 may prepare and audit them but cannot act as organizational risk, license, security, or execution approvers.

## Verification and artifacts

- Seven sidebar task states: completed/idle with final handoffs, no agent error.
- Product repository: unchanged by this run; no package restore, build, product test, or PoC test was warranted at the blocked gate.
- Documentation artifacts created/updated by Agent 0: readiness task cards, this readiness report, Project State, Decision Register, and Execution Log.
- Existing Phase 1 evidence remains at `D:\OTO\pertamina-oto\artifacts\evidence\P01-20260905-1319-integrated-r2`; manifest SHA-256 `261AFDC211BF54EBEC8F4E3738F13B9EDA401E4C0EF9E533A2BA45A238030E8B`.
- Target Phase 2 evidence root is not created or populated until authorization.

## Exit-gate result

| Exit requirement | Result |
|---|---|
| Approved Release-1 PoC scope | `BLOCKED` |
| All scoped PoCs executed with complete evidence | `NOT_RUN` |
| Governed disposition per scoped PoC | `NOT_AVAILABLE` |
| License/security/corpus/environment decisions | `BLOCKED` |
| Product implementation remains provider-neutral | `PASS_NO_PRODUCT_CHANGE` |
| Phase 2 exit gate | `FAIL_TO_OPEN / BLOCKED_PRE_EXECUTION` |

Phase 2 status is `BLOCKED`, not `COMPLETE`. Program progress remains 2/12 = 16.67%. The application is not ready for use, UAT, Production, or launch.

## Next permitted command

After the Workspace User approves or amends the exact packet and identifies the temporary human roles/environment, use:

`lanjutkan phase 2 sesuai rencana`

Agent 0 must then revalidate any time-sensitive candidate/license/security evidence, perform acquisition-only admission first, and may create execution work only after all mandatory conditions are actually PASS.
