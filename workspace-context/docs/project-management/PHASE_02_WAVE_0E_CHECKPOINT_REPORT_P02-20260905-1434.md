# PERTAMINA OTO — Phase 2 Wave 0E Checkpoint Report

| Field | Value |
|---|---|
| Run | `P02-20260905-1434` |
| Checkpoint | 2026-09-07 10:05:22 +07:00 |
| Status | `BLOCKED_PENDING_EXACT_QPDF_AND_RESTART_AUTHORITY` |
| Product main | Clean `main` @ `5769ccce917d03905d07a1440ee527cb653c9cc4` |
| Integration | Clean `integration/P2-proof-capability` @ `7fbcada232c0009a27a226f8f1d9d80cbe3c57e6` |
| Cost | `ZERO_ADDITIONAL_COST`; no purchase or paid commitment |
| Restart / Sandbox launch | `NOT_RUN` / `NOT_RUN` |
| Restore / build / component execution / PDF processing | `NOT_RUN` |

## Outcome

Wave 0E completed every safe task allowed by the user's free-only continuation.
Agent 1 prepared a fail-closed post-restart verifier. Agent 2 acquired and statically
inspected exactly three approved NuGet artifacts. Agent 6 and Agent 7 independently
verified their byte, security, license, notice, and evidence records. Agent 0 audited
and integrated all accepted handoffs.

The qpdf lane stopped before process creation because automatic approval review did
not recognize the general free-only continuation as direct authority for the exact
binary ZIP. Agent 3 did not retry, switch downloader/host, or bypass the decision.
No qpdf byte was downloaded and no cost was incurred.

## Actual agents and integration

| Lane | Actual agent | Handoff | Integrated commit |
|---|---|---|---|
| Post-restart readiness verifier | Agent 1 | `4e86136` | `344e1db` |
| Three-package acquisition/static intake | Agent 2 | `899d803` | `e8a32de` |
| qpdf blocked acquisition record | Agent 3 | `c297922` | `dd003ea` |
| Independent security review | Agent 6 | `52012303bd27082f465931a4566566ca6e68891b` | `9289a08` |
| Independent governance/license review | Agent 7 | `bc80785f6c90e94de0ae767fece8ffc21dc86405` | `7fbcada` |
| Cross-lane audit/integration | Agent 0 | all accepted evidence | checkpoint above |

Agents 4 and 5 already completed their Phase 2 readiness lanes earlier. They were not
redispatched in Wave 0E because conversion/OCR/device/provider execution remains
blocked and this wave owned only PDF dependency/environment admission.

## Exact acquired no-cost components

| Component | License | Bytes | SHA-256 |
|---|---|---:|---|
| Microsoft.Extensions.Logging.Abstractions 8.0.3 | MIT | 802188 | `E4C498D5A13051B4577A148F1D8C3470167215C507E2392069B75DC61322BB74` |
| System.Security.Cryptography.Pkcs 8.0.1 | MIT | 1078389 | `28C348909DF243FE4EE962213E3C8021AAECBC8321929DBA03A69BCB92A49DE5` |
| Microsoft.Extensions.DependencyInjection.Abstractions 8.0.2 | MIT | 278862 | `51F2DF1100245F10DA54F0BB7E813F277155117777D4FBBAB902214E27372606` |

All three exact HTTPS requests returned HTTP 200 with no redirect. Their SHA-512
values match the frozen NuGet catalog assertions. All 124 ZIP entries match the
tracked inventory; there are no path-traversal entries, case-fold duplicate aliases,
or named native/executable entries. Each package contains `LICENSE.TXT` and
`THIRD-PARTY-NOTICES.TXT`. Preserve the supplied license, copyright, attribution,
third-party notice, and warranty-disclaimer texts in any later authorized packaging.

Package signature verification exited 0 and reported Microsoft author signatures and
NuGet.org repository countersignatures. Revocation status remains unknown because
the check used offline revocation mode. This is not fresh non-revocation, malware,
vulnerability, or source-build reproducibility proof.

Raw artifacts remain only in:
`D:\OTO\pertamina-oto\artifacts\evidence\P02-20260905-1434\agent-2-dependency-intake`.
They have not been restored, installed, referenced, loaded, or executed.

## Security and runtime limitations

- Logging includes Roslyn source-generator assemblies and `buildTransitive` targets.
  Build authorization must explicitly freeze whether and how those activation
  surfaces are admitted.
- Pkcs Windows assets contain managed interop indicators for Windows cryptography and
  network-related vocabulary. This is not proof of egress, but full process-tree
  no-network testing is mandatory before local-offline claims.
- No selected net8.0 package dependency surprise was found: Logging depends on DI
  8.0.2; Pkcs and DI declare no additional selected package dependency.
- Exact TFM/RID/SDK/runtime asset selection, full SBOM, current advisory/scanner
  treatment, and isolated execution controls remain incomplete.
- Successful static intake does not authorize restore, build, engine use, or PDF
  processing.

## Environment result

The post-restart verifier has 20/20 passing mock self-tests and zero host probes. It
cannot restart Windows, launch Sandbox, or authorize processing. The exact later
observation command is:

`pwsh -NoProfile -File poc/environment/windows-sandbox/postrestart/Verify-PostRestart.ps1 -Observe`

That command must be run only after a separately approved restart. Even a fully
passing observation ends at `REVIEW_REQUIRED` and `STOP_BEFORE_LAUNCH`.

## Cost statement

- PDFsharp and the three Microsoft packages: MIT/no license fee, with notice duties.
- qpdf proposed structural oracle: Apache-2.0/no license fee, but bytes still absent.
- Windows Sandbox: built-in Windows component; no added purchase made.
- MuPDF commercial route: potentially paid and therefore explicitly excluded.
- MuPDF AGPL route: no license fee but governance/build obligations remain unresolved,
  so it is deferred rather than silently substituted.
- No paid API, cloud service, scanner, support plan, subscription, marketplace item,
  or infrastructure was used.

## Exact blockers and next decisions

Phase 2 is not complete. The immediate trusted-channel decisions are:

1. exact acquisition-only authority for `qpdf-12.4.1-msvc64.zip`, exact official URL,
   expected SHA-256, evidence target, zero-cost condition, and no-execution boundary;
2. separate explicit authority to restart Windows, after the user saves work, followed
   only by read-only post-restart Sandbox observation;
3. after qpdf intake and post-restart observation, an exact restore/build asset policy,
   current trust/vulnerability treatment, corpus/oracle/environment/evidence freeze,
   and finally a separate bounded PDF experiment authorization.

Phase 3 remains `BLOCKED_BY_SEQUENCE`. Program progress remains 2/12 phases = 16.67%.

