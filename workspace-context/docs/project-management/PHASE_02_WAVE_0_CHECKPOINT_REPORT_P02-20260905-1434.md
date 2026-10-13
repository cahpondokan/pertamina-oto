# PERTAMINA OTO — Phase 2 Wave 0 Checkpoint Report

| Field | Value |
|---|---|
| Run | `P02-20260905-1434` |
| Checkpoint time | 2026-09-06 00:59:19 +07:00 |
| Integration branch/commit | `integration/P2-proof-capability` @ `d63155afb6d4ac72c91f1930d6ba290ec5ad689c` |
| Product main | Unchanged @ `5769ccce917d03905d07a1440ee527cb653c9cc4` |
| Cost result | `ZERO_ADDITIONAL_COST` maintained |
| Processing disposition | `BLOCKED_SECURITY` |
| Phase 2 status | `BLOCKED`, not COMPLETE |

## Completed scope

- Agent 1 implemented a provider-neutral PoC harness preparation foundation. Agent 0 verified locked empty-source restore, Release build with zero warnings/errors, and 20/20 self-tests.
- Agent 2 completed acquisition-only intake of exact PDFsharp 6.2.4 from official NuGet. Artifact size is 4,625,326 bytes; SHA-256 `5647882339B327CBF6DA9EA3F882EFB335F8D602E6FD45323ADF9E78DA53CF67`; repository signature verification returned exit 0 with offline revocation-status limitation. No package reference or document processing was performed.
- Agent 3 implemented deterministic synthetic corpus/oracle preparation. Agent 0 verified 17/17 tests and identical generator output across repeated processes. The synthetic comparator correctly never grants publication.
- Agent 6 completed an independent static security review and issued `BLOCKED_SECURITY` for processing admission. This is an admission gap, not a confirmed exploit.
- Agent 7 completed governance/license review and issued `INTAKE_GOVERNANCE_READY_WITH_LIMITATIONS`. Top-level MIT terms support no-license-fee evaluation subject to notice obligations; complete dependency/resource/security clearance is not yet proven.
- Agent 0 integrated all accepted Wave 0A/0B commits into the isolated integration branch and reran binding/static checks: 111 package entries, package hash unchanged, 12 preparation-source hashes bound, no candidate assembly executed.

## Exact blockers before PDF processing

1. The Windows Sandbox executable is absent. Feature-state inspection requires Administrator elevation; the current process could not obtain it. No feature was enabled and no reboot/system mutation occurred.
2. The package declares minimum versions `Microsoft.Extensions.Logging.Abstractions >= 8.0.3` and `System.Security.Cryptography.Pkcs >= 8.0.1`; exact resolved dependency closure, hashes, licenses, assets, and SBOM are not frozen. No transitive package was downloaded.
3. Static strings indicate networking vocabulary in Cryptography/Quality assemblies and process-launch vocabulary in Quality. Reachability is unproven; runtime must be contained and egress/process creation observed before an offline claim.
4. Fresh certificate revocation and built-in malware scanner status were not established. Defender inspection returned access denied; no scanner settings or cloud submission were changed.
5. The synthetic comparator is not an independent real PDF parser/renderer. A free, independently admitted verifier is still required for reopen/structure/render validation.
6. Exact execution corpus manifest, run retention/access/expiry, safety ceilings, environment digest, and document-processing authorization remain to be frozen.

## Web-versus-Desktop decision impact

A web shell can be produced faster for visible UI and centralized updates, but it does not remove the Phase 2 engine and safety work. Pure browser processing introduces browser/WASM capability and memory/file-access limitations. Server processing would upload documents and make the feature network-dependent, conflicting with the first-release offline-first boundary unless Product/Security explicitly changes scope.

Recommended architecture remains Windows Desktop as the complete offline-first product, followed later by a curated web companion. No web pivot or server document processing is authorized by this checkpoint.

## Exit statement

Wave 0 preparation and static reviews are complete. Real PDF processing, engine approval, Phase 2 completion, Phase 3 start, and `main` promotion are not authorized. Overall program progress remains 2/12 = 16.67%.
