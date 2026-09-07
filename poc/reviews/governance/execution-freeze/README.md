# Wave 0G-C governance, license, and evidence decision packet

Agent / task: Agent 7 / P2-W0G-C-A7-GOVERNANCE-EVIDENCE-REVIEW

Run: P02-20260905-1434

Baseline: df37fd77aafd569082a568556e75e7e452d3596b

Result: DECISION_PACKET_COMPLETE_GATES_BLOCKED

This is an independent static review and a human-decision packet. It is not
legal advice, vendor approval, engine selection, risk acceptance, PRE-012
execution authority, or Phase 2 completion. Every execution/acquisition/build/
launch/processing/redistribution/product/Production/paid flag remains false.

## Decisive result

PRE-003 through PRE-011 remain blocked. PRE-012 remains human-only and
unapproved. Agent 6's BLOCKED_SECURITY disposition is controlling and was not
overridden.

The governance packet itself is complete as a blocker/decision record: every
PRE-003..012 row has an exact current status, blockers, owners, and required
human action. This does not mean any precondition passed.

A new reproducibility blocker was found. Agent 6's verifier binds an older
PROJECT_STATE.md at 10,577 bytes and SHA-256
e734889428dd9986810be44b6f3a6bd2bb63cdc756b304a2c7dbdbd10cb907e3.
The current post-dispatch file is 10,589 bytes and SHA-256
076047bfe559f53396f705e3ac46d41a27302526ca22a75e409d294fd0ad5d06.
Replay therefore exits 1. The change does not clear any finding; it requires an
independent security re-review bound to the current snapshot.

All 39 authoritative/integrated inputs, byte lengths, and SHA-256 values are in
governance-decision-packet.v1.json. The verifier recomputes them from the
repository and coordination roots.

## PRE-003..012 matrix

| Gate | Current result | Exact closing owner/action |
|---|---|---|
| PRE-003 | BLOCKED | Agent 0 + Technical Owner + Execution Approver: approve a new exact task card with branch/worktree, paths, commands/hashes, writers, independent reviewers, stop authority, effective time, and expiry only after PRE-004..011 pass. |
| PRE-004 | BLOCKED | Technical + License + Security: accept or reject the five exact candidate/oracle identities after resolved locked assets, SDK/runtime/framework/harness hashes, provenance, and complete SBOM are reviewed. |
| PRE-005 | BLOCKED | Human License/Cost + Technical + Evidence: decide internal evaluation separately from redistribution; accept exact license/notice delivery, SBOM format, support/removal owners, and zero cost. |
| PRE-006 | BLOCKED_SECURITY | Independent Security Reviewer + Human Security Approver + Technical + Agent 0: preserve all Agent 6 high findings, resolve GOV-C-001, re-review current hashes, then issue the human security decision. |
| PRE-007 | BLOCKED | Product + QA/Corpus + Security: approve/amend/reject the exact synthetic manifest and fill classification, owner/reviewer, access, retention, expiry, disposal, and missing groups. |
| PRE-008 | BLOCKED | Technical + independent PDF Quality/Oracle + Security: freeze structural, render, content/marker, source/finality identities, hashes, settings, tolerances, negative fixtures, and fail-closed aggregation. |
| PRE-009 | BLOCKED | Environment + Security + Agent 0: separately approve benign environment observation, then prove guest identity, network denial, mappings, least privilege, quota, recovery, cleanup, and expiry. |
| PRE-010 | BLOCKED | Technical + Product + QA + Security: set instrumentation, sampling, repetitions, concurrency, safety ceilings, and numeric targets; otherwise keep measured results baseline-only and blocked. |
| PRE-011 | BLOCKED | Evidence Custodian + Repository Integrator + Security + License: approve classification, access, custody, sanitization, retention, expiry, disposal, export, immutability/index schema, and SBOM format. |
| PRE-012 | HUMAN_ONLY_NOT_APPROVED | Human Execution Approver: do not authorize now. Review one exact final record only after PRE-003..011 pass and BLOCKED_SECURITY is closed. |

## Candidate/license boundary

The exact frozen identities are PDFsharp 6.2.4 plus Logging.Abstractions 8.0.3,
Cryptography.Pkcs 8.0.1, and DependencyInjection.Abstractions 8.0.2; qpdf
12.4.1 is only a proposed structural oracle. Their hashes are machine-checked in
the JSON.

MIT is declared for PDFsharp and the three NuGet dependencies. Apache-2.0 is
declared for qpdf's top level. Those declarations support a no-license-fee
proposal, not complete rights or notice clearance. Open gaps are:

- PDFsharp exact license/holder/resource and source-to-binary mapping;
- applicability and delivery of supplied Microsoft package notices;
- qpdf's eight Microsoft runtime DLLs and embedded OpenSSL/zlib/JPEG identities,
  build provenance, runtime rights, and notices;
- qpdf documentation JS/CSS/theme/font/generated-asset notice mapping;
- exact internal evaluation/copying boundary versus explicitly excluded
  redistribution/product packaging;
- an approved SBOM format/version and full component-to-notice index;
- internal support, advisory, removal, and fallback ownership.

Redistribution, desktop packaging, and shipping are out of the current scope and
not authorized. No inference from internal evaluation to redistribution is
allowed.

## Corpus, environment, evidence, and cost

The corpus remains synthetic-only, proposed, and not materialized. Its
classification authority, classification, owner, reviewer, access, retention,
expiry, disposal, and missing E03/E04/E05/E07/E08/E09/E10 groups are blockers.

Host readiness was observed, but guest boot, effective network denial,
least-privilege identity, separate input/output/temp/evidence roots, resource
ceilings, recovery, cleanup, and evidence export remain unproven or unset.

The Phase 2 evidence root decision exists, but run classification, custodian,
writer/reviewer access, sanitization, retention, expiry, disposal, immutable
index, safe export, and SBOM format remain blocked. Evidence must exclude
secrets, real documents/content, raw sensitive paths/identity, unsanitized
output, dumps, and network payloads.

No additional cost was incurred. No paid license, service, API, scanner,
support, subscription, marketplace item, or infrastructure is authorized. Stop
before any paid or unresolved cost/support commitment.

## Exact future scope, still disabled

The only future processing scope that may later be considered is
POC-PDF-001 / FEAT-PDF-005 / PDF001-E00..E12. The packet enumerates the exact
UT-MERGE-001..050 and TP-MRG-001..036 trace IDs individually.

Six ordered profiles remain BLOCKED_NOT_AUTHORIZED: benign environment
observation; PDFsharp bootstrap restore; locked restore; build; POC-PDF-001 run;
and qpdf structural oracle. All commands, arguments, processes, modules, paths,
owners, reviewers, ceilings, and final input hashes must be frozen before their
separate approval. The 24 fail-closed stop rules in the JSON apply.

Conversion/OCR and device/AI portfolio packets remain static-only and blocked.
They may not inherit POC-PDF-001 approval. Poppler, MuPDF, iText, all other PoCs,
product code, redistribution, real data, network processing, and Production are
excluded.

## Directly answerable human packet

The only safe current choices are:

1. REVISION_REQUIRED_ZERO_COST_REMEDIATION
2. DEFER_POC_PDF_001_WITH_OWNER_AND_REOPEN_CONDITION
3. REJECT_CURRENT_CANDIDATE_SET_AND_REQUIRE_NEW_INTAKE

The complete required response fields and exact safe/current and future-only
wording are stored in humanDecisionPacket. The recommended present response is
REVISION_REQUIRED_ZERO_COST_REMEDIATION unless the human authority chooses to
defer or reject. Choosing remediation authorizes no execution.

Any later execution wording is marked JANGAN GUNAKAN SEBELUM PRE-003..011 PASS
DAN BLOCKED_SECURITY DITUTUP. It binds one run-set, the exact candidate hashes,
the structural-only qpdf role, final manifest hashes, ZERO_ADDITIONAL_COST, all
stop rules, effective time, and expiry. It cannot be accepted from this draft
because eligibility is false and required fields are unresolved.

## Static verification

Run from the assigned repository worktree:

    python -B poc/reviews/governance/execution-freeze/verify_static.py --coordination-root "D:\AFT Ahmad Yani Semarang\Teknik\Dokumentasi Pertamina OTO"

The checker validates the closed top-level schema, nested required sections,
39 file lengths/hashes, five exact component identities, six license/notice
gaps, exact experiment/test sets, PRE matrix, owner/custody gaps, 24 stop rules,
portfolio isolation, false authorities, and ten unsafe in-memory mutations.

Upstream static replays during review:

- environment self-test: exit 0; 6/6 fixture checks; no host probe/process;
- PDFsharp raw-byte freeze: exit 0; 4 components/3 edges/15 bindings;
- corpus/oracle: exit 0; 2 schemas/14 bindings/10 experiments;
- conversion/OCR: exit 0; 7 PoCs; no authorization;
- device/AI: exit 0; 5 PoCs; no authorization;
- Agent 6 security: exit 1 only because of GOV-C-001 current coordination
  binding drift; BLOCKED_SECURITY retained.

No scanner, Sandbox, qpdf/PDFsharp, generator, parser, renderer, candidate,
restore, build, download, install, PDF open/processing, system mutation, or paid
commitment occurred.
