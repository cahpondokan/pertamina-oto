# PERTAMINA OTO — Phase 2 Readiness Task Cards

| Field | Value |
|---|---|
| Run ID | `P02-20260905-1434` |
| Phase | 2 — Proof of Capability |
| Mode | `READINESS_ONLY` — execution entry gate belum lulus |
| Product repository | `D:\OTO\pertamina-oto` |
| Frozen baseline | `main` @ `5769ccce917d03905d07a1440ee527cb653c9cc4` |
| Human authority | Workspace User — temporary Project/Product Owner and approval authority |
| Agent 0 | Project Manager, analyst, master integrator |
| Evidence target | `D:\OTO\pertamina-oto\artifacts\evidence\P02-20260905-1434` setelah execution authorization |

## Shared boundary

Entry blockers: `P2-D01`, `P2-D02`, `P2-D03` serta charter `PRE-003..012`. Kandidat/version, corpus, independent oracle, reviewer, isolated environment, measurement plan, evidence root, dan execution authorization belum frozen.

Semua task pada checkpoint ini read-only. Dilarang mengunduh, menginstal, restore package kandidat, menjalankan binary/provider/model/device, membuat product adapter, mengubah `D:\OTO\pertamina-oto`, memakai data Production, atau memberi status engine approval. Gunakan dokumen canonical dan sumber primer/official untuk fakta yang dapat berubah. Bedakan fakta, inference, rekomendasi, dan keputusan manusia yang masih dibutuhkan.

Output setiap agen: readiness status, exact missing inputs, 2–3 opsi bila relevan, trade-off, recommended safe default, official-source links, security/license caveat, proposed task/evidence plan, dan stop conditions. Agent 0 adalah satu-satunya writer catatan pusat.

## Agent 1 — Harness and Environment Readiness

Task ID: `P2-R-A1-HARNESS-ENV`.

Objective: audit read-only kesiapan harness, isolated environment, resource instrumentation, network-disabled method, timeout/cancel/cleanup, dan repeatable evidence untuk `PDF001-E00..E12`.

Inputs: Phase 2 prompt, PoC Master Plan, POC-PDF-001 charter, Phase 1 final report, current repository/toolchain state.

Do not touch: seluruh product/document files; no installation or candidate execution.

Acceptance: mapping `PRE-001..012`, environment options, tool gaps, safe isolation proposal, and exact approval items.

## Agent 2 — PDF Candidate Readiness

Task ID: `P2-R-A2-PDF-CANDIDATES`.

Objective: research primary-source landscape for `POC-PDF-001` critical path and relevant `POC-PDF-002/003`; propose 2–3 exact candidate/version records without approving a vendor.

Acceptance: capability fit, current exact version/source, license summary with official link, .NET/Windows/offline compatibility, native/runtime implications, security/maintenance signals, removal path, independent-oracle need, and recommended evaluation order.

Stop: no download/install/run; do not infer redistribution rights or choose a winner.

## Agent 3 — Corpus, Oracle, and Fidelity Readiness

Task ID: `P2-R-A3-CORPUS-ORACLE`.

Objective: convert charter corpus groups and `TP-MRG`/`UT-MERGE` trace into a synthetic, non-sensitive corpus/oracle proposal for human approval.

Acceptance: corpus groups/items, generator/provenance/classification/retention proposal, expected properties, independent verifier strategy, visual/structural/manual-review boundaries, and approval checklist. No corpus bytes generated yet.

## Agent 4 — Conversion and OCR Scope Readiness

Task ID: `P2-R-A4-CONV-OCR`.

Objective: review `POC-CONV-001..004` and `POC-OCR-001..002`, identify which belong in approved Release-1 Phase 2 scope after the PDF critical path, and prepare primary-source candidate categories plus deferral trade-offs.

Acceptance: priority/wave recommendation, candidate/version information where safely researchable, license/offline/security constraints, corpus/oracle needs, and exact decisions. No provider execution.

## Agent 5 — Device, Platform, and Local-AI Readiness

Task ID: `P2-R-A5-DEVICE-PLATFORM`.

Objective: assess readiness of `POC-SCAN-001`, `POC-AI-LOCAL-001`, and cross-platform feasibility against the release profile: Windows Desktop first, Android/Web later, iOS deferred.

Acceptance: what should run, defer, or require device/account/model authority; device/environment matrix; consent/offline/resource/thermal/package/license risks; no public AI endpoint.

## Agent 6 — Security and Performance Readiness

Task ID: `P2-R-A6-SEC-PERF`.

Objective: threat-led review of planned PoC, hard invariants, hostile corpus, sandbox/network observation, resource exhaustion, concurrency, cancellation/crash, cleanup, and evidence sanitization.

Acceptance: control-to-experiment matrix, safe canary plan, nonnumeric measurement baseline, proposed policy thresholds that require human approval, stop authority, and gaps. Do not invent pass thresholds.

## Agent 7 — License, Governance, and Disposition Readiness

Task ID: `P2-R-A7-GOVERNANCE`.

Objective: prepare candidate intake/license/provenance/SBOM/evidence/disposition decision packet and identify exact human roles required.

Acceptance: completeness matrix for `P2-D01..03` and `PRE-003..012`, license/cost/redistribution/runtime/support checklist, evidence schema, reviewer independence, decision options, expiry/change-control, and exact authorization wording. No vendor approval.

## Agent 0 integration

Agent 0 waits for all seven handoffs, audits source quality and consistency, resolves overlap, and produces one Phase 2 readiness/decision packet. If mandatory human inputs remain unavailable, Phase 2 remains `BLOCKED`; progress stays 2/12. No PoC execution task/worktree is created until exact approval freezes Wave 0.
