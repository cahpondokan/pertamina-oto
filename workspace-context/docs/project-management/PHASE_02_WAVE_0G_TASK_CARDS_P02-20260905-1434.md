# PERTAMINA OTO — Phase 2 Wave 0G Task Cards

| Field | Value |
|---|---|
| Run | `P02-20260905-1434` |
| Authorized at | 2026-09-07 20:31:20 +07:00 |
| User command | Continue Phase 2; free only; no Sandbox/qpdf/PDFsharp execution before a separate execution-authorization packet is ready |
| Input checkpoint | Clean `integration/P2-proof-capability` @ `4a78212e01d3daee9ae47f0473fb6186df0dccde` |
| Product main | Clean `main` @ `5769ccce917d03905d07a1440ee527cb653c9cc4` |
| Objective | Prepare and independently review an exact `PRE-003..011` execution-freeze packet; `PRE-012` remains human-only and unapproved |
| Cost boundary | `ZERO_ADDITIONAL_COST`; stop before any purchase, commercial license, paid service/API/scanner/support, subscription, marketplace item, or billable infrastructure |
| Activation boundary | No Sandbox launch; no qpdf/PDFsharp install, restore, build, load, execution, or PDF processing; no Poppler/MuPDF substitution; no new binary/model/package acquisition |

All specialists are senior/principal owners of their assigned lane. They must read
the Shared Agent Contract, Phase 2 prompt, current state/decisions, this task card,
and the directly relevant canonical/PoC sources. Each writer uses a fresh isolated
branch/worktree from the stated integration checkpoint. Agent 0 alone writes central
project-management records and performs cross-lane integration.

## Wave 0G-A — freeze inputs (Agents 1–3)

### Agent 1 — `P2-W0G-A1-ENVIRONMENT-FREEZE`

- Objective: prepare the exact non-executing Windows Sandbox environment manifest,
  control checklist, and later-launch observation plan using the completed
  post-restart evidence.
- Allowed Paths: `poc/environment/execution-freeze/**` only.
- Do Not Touch: current Sandbox template/verifier, product source, candidate binaries,
  central records, system settings, services, registry, or other lanes.
- Inputs: post-restart report/JSON, existing `offline.wsb.template`, static validator,
  Phase 2 charter/security requirements.
- Acceptance: bind OS/build/architecture/runtime and launcher identity without user
  identity/raw sensitive paths; separate observed host readiness from unproven guest
  boot/network/host-boundary controls; exact allowed/denied mappings and later
  instrumentation plan; every launch/processing authority flag false; verifier/self-
  tests must be static or fixture-only.
- Verification: no Sandbox/process launch or system mutation; deterministic manifest
  validator and negative fixtures; zero-cost statement.
- Handoff: commit, paths, commands/results, risks, blockers, evidence, and fields
  still requiring human/security authority.

### Agent 2 — `P2-W0G-A2-PDFSHARP-CANDIDATE-FREEZE`

- Objective: prepare a hash-bound PDFsharp 6.2.4 candidate/dependency/asset freeze
  proposal from already acquired static evidence, without restoring or loading it.
- Allowed Paths: `poc/intake/execution-freeze/pdfsharp/**` only.
- Do Not Touch: package archives, product/project graphs, `Directory.Packages.props`,
  existing intake, central records, other lanes.
- Inputs: PDFsharp 6.2.4 and exact Logging 8.0.3, Pkcs 8.0.1, DI 8.0.2 static
  inventories/receipts/licenses; dependency closure; target `.NET 10`/Windows x64.
- Acceptance: exact artifact identities, sizes/hashes/source records, selected TFM/RID
  proposal, direct/transitive/build/analyzer/runtime activation inventory, minimal
  allowlist/denylist, offline locked-restore design, SBOM/license/security fields and
  expiry/change invalidation. Do not claim resolved assets or supported runtime before
  a later authorized restore/build.
- Verification: raw-byte/hash/static metadata only; no restore/build/reflection/load/
  source generator/native activation/PDF processing/network acquisition.
- Handoff: commit, paths, commands/results, unresolved trust/advisory/runtime/license
  fields, and exact prerequisites for `PRE-004..006`.

### Agent 3 — `P2-W0G-A3-CORPUS-ORACLE-FREEZE`

- Objective: freeze a proposed synthetic-only POC-PDF-001 corpus and independent
  oracle plan from existing tracked corpus/oracle evidence without generating or
  processing a PDF.
- Allowed Paths: `poc/corpus/execution-freeze/**` and
  `poc/oracles/execution-freeze/**` only.
- Do Not Touch: existing corpus/oracle generators or manifests, qpdf archive, product
  source, central records, other lanes.
- Inputs: existing deterministic corpus foundation, qpdf static intake/reviews,
  `PDF001-E01..E10`, `UT-MERGE-*`, `TP-MRG-*`, charter requirements.
- Acceptance: exact source hashes and proposed manifest IDs; classification/owner/
  retention/expiry placeholders must remain explicit; expected page/order/duplicate/
  marker/reopen/source/finality properties; structural qpdf oracle stays unexecuted;
  complementary render/content oracle and password/hostile/resource corpus gaps are
  explicit; no synthetic result or numeric target is invented.
- Verification: schema/fixture/static binding tests only; no PDF generation/opening,
  parser/renderer/qpdf/Poppler/MuPDF execution, or document processing.
- Handoff: commit, paths, results, gaps, exact `PRE-007/008/010/011` decisions.

## Wave 0G-B — portfolio and independent security review (Agents 4–6)

This wave begins only after Agent 0 audits and integrates Wave 0G-A.

### Agent 4 — `P2-W0G-B-A4-CONVERSION-OCR-DISPOSITION`

- Objective: prepare a no-execution scope/dependency/gate matrix for
  `POC-CONV-001..004`, `POC-OCR-001..002`, and `POC-PDF-003` so these future PoCs
  cannot be silently treated as approved by POC-PDF-001.
- Allowed Paths: `poc/portfolio/execution-freeze/conversion-ocr/**` only.
- Do Not Touch: product code, providers, packages, corpus, central records, other lanes.
- Acceptance: exact Feature IDs, canonical security/oracle/resource/failure needs,
  current disposition, prerequisite/owner/reopen condition; HTML active/network denial,
  Office macro/link denial, OCR formula-injection controls, PDF/A independent
  conformance validator; no vendor recommendation or execution claim.
- Verification: documentation/schema consistency only; zero execution/acquisition.

### Agent 5 — `P2-W0G-B-A5-DEVICE-AI-DISPOSITION`

- Objective: prepare a no-execution scope/dependency/gate matrix for
  `POC-SCAN-001`, `POC-INTEL-001/002`, `POC-CONV-004`, and optional
  `POC-AI-LOCAL-001`, consistent with Windows Desktop first and iOS deferred.
- Allowed Paths: `poc/portfolio/execution-freeze/device-ai/**` only.
- Do Not Touch: product/mobile/web source, devices, models/providers, packages,
  central records, other lanes.
- Acceptance: exact Feature IDs, approved/no-public-AI boundary, offline/data/consent/
  model/device/resource/license fields, current disposition and reopen condition;
  no model/provider/device assumption, download, endpoint call, or iOS reopening.
- Verification: documentation/schema consistency only; zero execution/acquisition.

### Agent 6 — `P2-W0G-B-A6-SECURITY-EXECUTION-FREEZE-REVIEW`

- Objective: independently review integrated Wave 0G-A inputs plus existing PDFsharp,
  qpdf, post-restart and portfolio evidence; issue a security readiness/blocker packet.
- Allowed Paths: `poc/reviews/security/execution-freeze/**` only.
- Do Not Touch: candidate archives, system/Sandbox, product graph, central records,
  other lanes.
- Acceptance: exact hash/input binding; current official advisory/provenance references
  may be read but no binary/tool acquisition; candidate/restore/build/launch/runtime
  allowlist; process/module/DLL-search/network/egress/temp/resource/cancel/cleanup/
  canary evidence plan; fail-closed stop rules; severity-ranked gaps; no self-approval.
- Verification: static/read-only checks only. Do not run malware scanner, Sandbox,
  candidate, build, restore, or PDF corpus.
- Handoff: one of `FREEZE_READY_FOR_GOVERNANCE_REVIEW` or
  `BLOCKED_SECURITY`, never execution approval.

## Wave 0G-C — independent governance/final packet review (Agent 7)

This wave begins only after Agent 0 integrates Wave 0G-B.

### Agent 7 — `P2-W0G-C-A7-GOVERNANCE-EVIDENCE-REVIEW`

- Objective: independently review all integrated freeze inputs and prepare the exact
  human decision packet for `PRE-003..012` without approving it.
- Allowed Paths: `poc/reviews/governance/execution-freeze/**` only.
- Do Not Touch: candidate archives, system/Sandbox, product source, central records,
  other lanes.
- Acceptance: candidate/component/license/notice/SBOM-format decision gaps; internal
  evaluation versus redistribution boundary; corpus/environment/evidence retention,
  access, expiry, disposal; reviewers/owners; zero-cost proof; exact execution scope,
  stop conditions, and approval wording. Any unresolved field remains blocker, not an
  invented value.
- Verification: reproducible input hashes and schema completeness; no execution,
  acquisition, build, restore, launch, PDF processing, or paid commitment.
- Handoff: commit, paths, verifier result, complete/blocked `PRE-003..012` matrix,
  residual risks, and one directly answerable human-decision packet.

## Agent 0 integration and stop rule

Agent 0 waits for every active lane, rejects incomplete/overlapping handoffs, integrates
in dependency order A → B → C, reruns only static/self-test verifiers, updates central
records, and stops before `PRE-012`. A complete packet is not execution authority.
Any hash/source/license/cost/secret/sensitive-data mismatch or need to launch/restore/
build/process stops the affected lane. Phase 2 cannot become COMPLETE in Wave 0G.
