# Wave 0F independent qpdf security review

Agent 6 / P2-W0F-A6-QPDF-SECURITY-REVIEW; run P02-20260905-1434; 2026-09-07.
Baseline 2842ee03e5942d979b054341d74391eb07b59e71; branch task/P2-A6-wave0f-review, initial worktree clean. Authority: exact static review only; ZERO_ADDITIONAL_COST.

Recommendation: STATIC_INTAKE_VERIFIED_WITH_LIMITATIONS; HOLD_FOR_SECURITY_ADMISSION. Evidence may advance into preparation of an execution-freeze packet, but that packet cannot be declared complete or authorized. No confirmed exploit/malware finding, engine approval or human risk acceptance is asserted.

## Inputs and reproduction

Applied AGENTS.md, Shared Agent Contract and Phase 2 prompt read in this task history; read current Decision Register and Wave 0F task card, integrated STATIC_INTAKE.md/intake-manifest.json/inspect_zip.py and raw acquisition/inventory. Exact independent-review baseline above supersedes the earlier acquisition checkpoint for this lane. Prior qpdf approval-review blocker is historical: direct user approval and Wave 0F successful acquisition supersede it. Current blockers are evidence/admission gaps, not the old download denial.

```text
python -B poc/reviews/security/wave0f/verify_static.py <approved-agent-3-qpdf-intake-root> poc/oracles/qpdf-12.4.1
```

Installed Python standard library only. Executed exit 0; output static-evidence.json. Original archive never extracted or executed. All entries read as raw bytes; PDF manual only hashed, not parsed. Script checks exact archive size/hash before and after; verifies all raw artifact hashes against tracked intake-manifest; compares all member byte counts/digests and PE import/delay/certificate data against original inventory. It independently implements ZIP/PE parsing rather than running the producer's script, which would write the original evidence.

| Verification | Result |
|---|---|
| ZIP size | 28,165,367 bytes |
| SHA256 before/after | 3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6 |
| Raw manifest binding | ZIP, acquisition.json and archive-static-inventory.json match |
| All member hashes/lengths | 319/319 match; CRC verified by reading |
| Path checks | No absolute/traversal/backslash/colon/trailing-dot-space/casefold duplicate/symlink/encrypted entry found |
| PE files | 12 x64; imports/delay/certificate metadata match intake |
| Delay imports | None found in the inspected descriptor tables |
| Activation paths | 39 executable/DLL/JS/linker-library paths, matching intake categories |
| Extraction/load/execution/download | NONE |

Checks are for this exact hash-bound archive, not a generic extraction authorization or complete malicious-archive validator. No disassembly, callgraph, export-forwarder resolution, dynamic dependency resolution, Authenticode verification, code scanning or PDF test occurred. No new tool/package installed. ASCII plus UTF16 indicator scan counts differ from the producer's ASCII-only scan: qpdf30 OpenSSL count 106 versus 102 is a method difference, not artifact drift; entry hashes are identical.

## Findings ordered by admission severity

**F-SEC-01 — High: provenance/trust/advisory gap.** qpdf.exe, fix-qdf.exe, zlib-flate.exe and qpdf30.dll have zero embedded certificate-table bytes. Eight Microsoft runtime DLLs have nonempty tables, whose bounds/hashes match inventory. Table presence does not establish signature correctness, publisher identity, chain/timestamp validity or fresh revocation; absence does not prove maliciousness or absence of detached/catalog signatures. No detached/catalog/signature-chain verification was performed. The expected release hash and controlled-source receipt bind acquisition bytes, not independently authenticated reproducible builds. Fresh advisory/affected-version and malware-scanner evidence is NOT_RUN here; no CVE-free or malware-clean claim. Human Security/Technical authority needs artifact-bound provenance/trust treatment and current vulnerability disposition before execution. Do not invent a waiver or fetch a paid scanner.

**F-SEC-02 — High: incomplete native/runtime/SBOM closure.** qpdf.exe directly imports qpdf30.dll, MSVCP140.dll, VCRUNTIME140.dll, VCRUNTIME140_1.dll, KERNEL32 and CRT API sets. qpdf30 imports ADVAPI32, CRYPT32, USER32, WS2_32, KERNEL32, Microsoft runtime and CRT API sets. DLL names map to bundled candidates/target OS families, but exact guest resolution, runtime versions, statically linked component versions and dynamically loaded modules remain unverified. OpenSSL/zlib/libjpeg strings are component indicators; not conclusive versions or proof of linkage. Exact SBOM and bundled resource/license applicability require Agent 7 and provenance review. No extra DLL/runtime installation is authorized. API-set names are not instructions to download missing DLLs.

**F-SEC-03 — High: networking/dynamic-loading containment gap.** WS2_32 is an actual import; LoadLibrary/GetProcAddress vocabulary occurs in qpdf30 and some runtimes. No conclusion that the planned structural command performs network I/O follows. No WinHttp/WinInet/CreateProcess/ShellExecute pattern matched the scan; this cannot prove their absence via indirection/OS code. Future isolated execution must use exact executable and dependency digests, controlled DLL search/environment/current-directory, least privilege, network default-deny and attempted/successful egress plus process-tree/module observation. No ambient host profile/credential or fallback download. DLL search hijacking and runtime module drift require handle/path/digest controls, not reliance on PATH.

**F-SEC-04 — Medium: broader archive activation than required oracle.** Three executable members are qpdf.exe, fix-qdf.exe and zlib-flate.exe; nine DLLs include qpdf30 plus eight runtimes. Link libraries qpdf.lib/qpdf_static.lib, documentation JS/HTML/fonts and manual are additional package surfaces. None is approved merely because archived. Propose only qpdf.exe plus reviewed required dependency subset for a later structural oracle profile; helpers, linker libraries, browser documentation, scripts and viewers remain outside execution allowlist. Do not auto-open HTML/docs or shell-execute imported results. Selecting a minimal runtime subset is a future reviewed action, not extraction performed here; retain all required notices.

**F-SEC-05 — High: safe oracle semantics/output and environment still unproven.** qpdf is a structural candidate, not a visual oracle. Prior frozen research documents warnings/recovery and possible sensitive encryption output. qpdf30 has password vocabulary; counts alone establish neither leakage nor safety. Future invocation must use typed arguments and exact operation allowlist, bounded stdout/stderr handling and approved ephemeral password design; raw output must not enter logs/evidence. Any parser repair/warning/inconclusive/timeout must not silently yield validation PASS. Independent rendering/content/occurrence/reopen evidence is still required. No synthetic PDF was processed here.

## Exact execution-freeze packet requirements

| Gate / experiment | Evidence needed before dependent execution |
|---|---|
| PRE-004/PRE-005/PRE-006; PDF001-E00/E11 | Exact ZIP/member/runtime/OS/SBOM and license/trust/advisory dispositions; reviewed minimal binary/argument activation list |
| PRE-007/PRE-008; PDF001-E01/E02/E03/E06 | Frozen corpus/expected order/count/duplicate/semantic properties; complementary independently admitted render oracle; actual staging-byte validation plan |
| PRE-006/PRE-009; PDF001-E00/E05/E12 | Effective guest isolation, least privilege, denied network, attempted-egress/module/process observation and safe unavailable/fallback behavior |
| PRE-010; PDF001-E08/E09/E10 | Approved memory/temp/output/file/decompression/time/concurrency ceilings, supervised process-tree kill/cancel, drain and reconciliation; no numeric threshold invented |
| PRE-011; PDF001-E04/E06/E08 | Synthetic canaries, allowlisted sanitized records, bounded data-only export quarantine, source/target digests, retention/access/expiry/disposal; no raw password/path/content/dump |
| PRE-012 | Exact human execution authorization after preceding evidence review; acquisition/static approval insufficient |

Source safety/atomic publication PDF001-E07 remains required by the Merge workflow; static qpdf intake does not prove it. Freeze qpdf operation outputs as data, not publication authority. Supervisor must stop the full process tree and prevent late publish. Guest disposal is not application cleanup evidence and does not reverse writable host mappings. Missing launcher/restart authority/effective controls remain separate environment gates; this review does not restart or launch anything. No main-host hostile test or substituted Poppler/MuPDF allowed.

For current safe progress, Agent 0 may integrate this review and Agent 7's exact license/notice review, then prepare a bounded follow-up addressing provenance, component-version/SBOM/trust/advisory and final runtime/oracle policy. Any metadata/source acquisition needed for those gaps requires its own exact scope; this task authorizes none. Human authority decides residual risk, not Agent 6. Unknowns are explicit blockers, not automatic rejection of qpdf or permission to buy alternatives.

## Verification and handoff

Static script exit 0; git baseline/branch/status confirmed. Only REVIEW.md, verify_static.py and static-evidence.json under poc/reviews/security/wave0f written. Full package-member hashes, PE import/certificate metadata and indicator counts are in static-evidence.json; no raw static-string/document/token dump. Raw input evidence unchanged. Git staged diff check required before commit. Runtime/offline/performance/scanner/product tests NOT_RUN by scope; no restore/build or binary version invocation.

Cost: no added fee, purchase, paid support/API/scanner/infrastructure; ZERO_ADDITIONAL_COST maintained. Stop on changed hash/path/source/asset, unexpected dependency/activation, paid requirement, sensitive evidence, absent containment/oracle, unapproved execution/restart or unresolved human authority. Integrate through Agent 0 only; no main merge. Scope IDs: FEAT-PDF-005, P2-W0-QPDF-ACQ-001, P2-D01/P2-D02/P2-D03, TECH-DEC-005/006, UT-MERGE-033/034, TP-MRG-028/029/033. No canonical test or phase exit PASS is claimed.
