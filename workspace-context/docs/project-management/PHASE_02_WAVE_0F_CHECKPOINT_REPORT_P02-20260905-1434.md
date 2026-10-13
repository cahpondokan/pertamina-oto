# PERTAMINA OTO — Phase 2 Wave 0F Checkpoint Report

| Field | Value |
|---|---|
| Run | `P02-20260905-1434` |
| Checkpoint | 2026-09-07 12:22:17 +07:00 |
| Status | `BLOCKED_PENDING_RESTART_AND_EXECUTION_FREEZE` |
| Product main | Clean `main` @ `5769ccce917d03905d07a1440ee527cb653c9cc4` |
| Integration | Clean `integration/P2-proof-capability` @ `4a78212e01d3daee9ae47f0473fb6186df0dccde` |
| Cost | `ZERO_ADDITIONAL_COST`; no purchase or paid commitment |
| qpdf install / load / execution / PDF processing | `NOT_RUN` |
| Restart / Sandbox launch / restore / build | `NOT_RUN` |

## Outcome

The user's exact approval was fulfilled within its boundary. Agent 3 downloaded only
the frozen official qpdf 12.4.1 Windows ZIP and performed static byte inspection.
Agent 6 and Agent 7 independently reviewed its security, activation, provenance,
license, notice, redistribution, and cost evidence. Agent 0 audited and integrated
all accepted handoffs. qpdf was not installed, extracted to an execution location,
loaded, run, or used to process a PDF.

The static audit is complete, but qpdf is not an approved engine. Security trust,
complete component/SBOM and notice closure, effective isolated no-network controls,
oracle semantics, corpus/limits/evidence freeze, and separate execution permission
remain mandatory.

## Actual agents and integration

| Lane | Actual agent | Handoff | Integrated commit |
|---|---|---|---|
| Exact acquisition and static ZIP/PE intake | Agent 3 | `2fa6b0a` | `2842ee0` |
| Independent security review | Agent 6 | `18a456f96091432711213f997a7502e7926fe73a` | `01c16f9` |
| Independent governance/license review | Agent 7 | `17f0f635975cbbcb991a993327e5ea0eb3db0503` | `4a78212` |
| Cross-lane audit/integration | Agent 0 | all accepted evidence | checkpoint above |

Agents 1, 2, 4, and 5 were not redispatched in Wave 0F because their Phase 2 lanes
were already complete or outside this exact qpdf static-audit scope. Agent 1's
post-restart mock verifier and Agent 2's dependency closure were rerun by Agent 0.

## Exact asset and static evidence

| Item | Result |
|---|---|
| Official URL | `https://github.com/qpdf/qpdf/releases/download/v12.4.1/qpdf-12.4.1-msvc64.zip` |
| Receipt | one GitHub redirect, then HTTP 200 |
| Size | 28,165,367 bytes |
| SHA-256 | `3CD016CD433EF7232E42F4C13348A49CC14907A3C7278EF4F99120593126F7A6` |
| ZIP entries | 319/319 hashes and lengths matched |
| PE files | 12 x64; three EXE and nine DLL records matched |
| Activation-surface paths | 39 |
| Unsafe / duplicate / symlink / encrypted entries | 0 / 0 / 0 / 0 |
| Text entries triaged for governance | 186 |

Raw evidence is retained at
`D:\OTO\pertamina-oto\artifacts\evidence\P02-20260905-1434\agent-3-qpdf-intake`.
It is not referenced by the product and was not placed on PATH.

## Security disposition

Agent 6's recommendation is
`STATIC_INTAKE_VERIFIED_WITH_LIMITATIONS; HOLD_FOR_SECURITY_ADMISSION`.

- qpdf.exe, fix-qdf.exe, zlib-flate.exe, and qpdf30.dll have no embedded certificate
  table. This does not prove maliciousness, but signature-chain/catalog, current
  advisory, malware-scan, and reproducible-build evidence remain missing.
- qpdf30.dll imports `WS2_32` and contains dynamic-loading indicators. No actual
  network activity was observed because nothing was executed. A later experiment
  must enforce and observe default-deny networking, module resolution, process tree,
  exact paths/digests, least privilege, and controlled DLL search.
- Only qpdf.exe plus a reviewed minimal dependency set may later be proposed for an
  allowlist. fix-qdf.exe, zlib-flate.exe, linker libraries, documentation scripts,
  and viewers remain outside any execution authorization.
- qpdf can be only a structural oracle candidate. Warnings, repair, inconclusive
  output, timeout, or parser error must not silently become a validation PASS; a
  complementary rendering/content oracle remains required.

## Governance, license, and cost disposition

Agent 7's recommendation is `HOLD_FOR_COMPONENT_NOTICE_CLOSURE`.

- qpdf declares Apache License 2.0. This supports evaluation without a license fee,
  while retaining license, attribution, change-notice, patent, trademark, and
  warranty limitations as applicable.
- The ZIP bundles eight Microsoft C++ runtime DLLs. Their exact build identity,
  applicable REDIST/license terms, servicing ownership, and intended redistribution
  rights are not yet accepted.
- Embedded OpenSSL, zlib, and JPEG candidates require exact version/build/source and
  applicable license/notice mapping. Documentation also includes jQuery, Sphinx,
  Snowball, Font Awesome, and font/theme assets requiring notice coverage.
- No standalone complete SBOM or release-wide LICENSE/COPYING/NOTICE artifact was
  identified in the intake. Existing notices are real but not yet a complete
  component-to-file redistribution matrix.
- No paid component, scanner, API, cloud service, support plan, subscription,
  infrastructure, or license was used. MuPDF's commercial route remains excluded;
  its no-fee AGPL route remains deferred rather than silently substituted.

## Verification

- Agent 0 qpdf security verifier: archive hash before/after matched; 319/319 entries,
  12/12 PE records, and raw evidence binding PASS.
- Agent 7 evidence: `PASS`; 319/319 entry hashes/sizes and 186 text entries recorded;
  extraction, execution, and PDF parsing are false.
- Post-restart verifier self-test: 20 discovered, 20 passed, 0 failed, 0 host probes.
- Dependency-closure verifier: PASS; exact MIT identities/groups, 4 nodes/3 edges.
- Integration worktree: clean; product `main`: clean and unchanged.

These are static/readiness checks. Build, runtime, fidelity, performance, offline
processing, and product tests remain `NOT_RUN`, not passed.

## Exit gate and blockers

Phase 2 exit gate did not pass. Current blocker groups are:

1. separate user approval to restart Windows, followed only by the already prepared
   read-only post-restart Windows Sandbox observation;
2. qpdf/PDFsharp trust, vulnerability, exact runtime/assets, SBOM, license/notice,
   support, and cost ownership closure;
3. frozen non-sensitive corpus, independent structural plus rendering/content
   oracles, numeric resource/performance limits, evidence retention/redaction, and
   effective isolated no-network controls;
4. a later separate, exact restore/build and bounded execution authorization.

Phase 3 remains `BLOCKED_BY_SEQUENCE`. Program progress remains 2/12 phases = 16.67%.
