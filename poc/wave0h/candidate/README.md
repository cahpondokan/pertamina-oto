# Wave 0H candidate: bounded static recommendation

Baseline: d472b85e179d715b9795773bcfda01dc628e9d42. Scope: POC-PDF-001 / FEAT-PDF-005. Agent 2 prepared this packet without acquisition, installation, build, restore, or engine execution. Current user authorization is recorded in the Wave 0H task card; historical Wave 0G restrictions remain historical evidence.

Recommendation: conditional GO for reviewed, locally generated benign smoke only after Agent 0 verifies environment, corpus, supervisor, and runtime staging. HOLD arbitrary PDFs and full capability admission. This is neither a claim that upstream defects are fixed nor a Phase 2 exit decision.

## Primary reports and exact-version relevance

| Source | Observed report | Consequence for this candidate |
|---|---|---|
| [PDFsharp #361](https://github.com/empira/PDFsharp/issues/361) | Reporter tested 6.2.4: cyclic page tree can cause fatal stack overflow during open/page traversal. | Directly relevant to acquired 6.2.4. Use only reviewed generator output with flat, acyclic page tree. Catching exceptions is not a process containment mechanism. No shipped fix verified. |
| [qpdf #1739](https://github.com/qpdf/qpdf/issues/1739) | Reporter tested Linux version labelled 12.4.0, commit 503d401615a842f7c5220a3ee425f5db2f25f537; linearization hint parsing may allocate excessively during checks. | Not exact Windows 12.4.1 reproduction. Exclude linearized files and --check, --check-linearization, --show-linearization. Do not infer a fix. |
| [qpdf #1688](https://github.com/qpdf/qpdf/issues/1688) | Reporter tested 12.3.0; predictor arithmetic and decompression resource limits are implicated. | Exclude image/predictor/compressed streams. Applicability to 12.4.1 remains unresolved. |

[qpdf release notes](https://qpdf.readthedocs.io/en/12.4/release-notes.html) describe 12.4.1 JSON and another linearization correction, and 12.4.0 depth bounds. These do not establish resolution of #1739/#1688. Sources reviewed during Wave 0H; issue states may change.

## Exact available assets

| Asset | SHA256 |
|---|---|
| PDFsharp 6.2.4 nupkg | 5647882339B327CBF6DA9EA3F882EFB335F8D602E6FD45323ADF9E78DA53CF67 |
| Microsoft.Extensions.Logging.Abstractions 8.0.3 | E4C498D5A13051B4577A148F1D8C3470167215C507E2392069B75DC61322BB74 |
| System.Security.Cryptography.Pkcs 8.0.1 | 28C348909DF243FE4EE962213E3C8021AAECBC8321929DBA03A69BCB92A49DE5 |
| Microsoft.Extensions.DependencyInjection.Abstractions 8.0.2 | 51F2DF1100245F10DA54F0BB7E813F277155117777D4FBBAB902214E27372606 |
| qpdf 12.4.1 msvc64 zip | 3CD016CD433EF7232E42F4C13348A49CC14907A3C7278EF4F99120593126F7A6 |

These are existing intake identities, not new downloads. Member hashes and inventories: poc/intake/execution-freeze/pdfsharp/candidate-freeze.json, poc/intake/dependency-artifacts, poc/oracles/qpdf-12.4.1/intake-manifest.json. Raw archives remain under D:\OTO\pertamina-oto\artifacts\evidence\P02-20260905-1434, in the agent-2-pdfsharp-intake, agent-2-dependency-intake and agent-3-qpdf-intake directories. Recheck hashes before staging.

## Minimal operator sequence (not executed)

1. Agent 0 audits Agent 1 containment and Agent 3 generator/oracle. Default Sandbox has no mapped host drive; network, clipboard, printer, audio and video redirection are disabled. Resolve a reviewed staging/export route before launch; do not silently re-enable controls.
2. Accept only digest-bound output of the reviewed generator: A/B/C, flat acyclic page trees, uncompressed text. Exclude filters, object streams, images, actions, attachments, forms, encryption, external fonts and linearization. Filename or byte scanning is not a sanitizer for arbitrary documents.
3. Proposed operational ceilings: three input arguments, three pages and 64 KiB per input, 256 KiB output, one processing child and 15 seconds per invocation. The three-page limit matches the reviewed corpus spec.json: A has two pages, B three, and C one; CAB expects six pages and AA four. Agent 1 must bind guest memory and termination supervision. These are smoke safety limits, not canonical performance acceptance. Terminate the whole guest on resource runaway.
4. Stage existing qpdf bin and required bundled DLLs in private guest C:\OTO-Smoke\qpdf. Run `qpdf.exe --version`, then `qpdf.exe --show-npages C:\OTO-Smoke\input\A.pdf` and `qpdf.exe --show-pages C:\OTO-Smoke\input\A.pdf` under supervision. Loader success and page structure observations do not prove PDFsharp merging or visual fidelity. Do not activate auxiliary executables or manual HTML scripts.
5. MergeSmoke.cs is unbuilt source. Agent 0 must prepare and hash a runner using the already installed SDK and explicit existing assembly references, without package restore or importing package build/analyzer assets. Select PDFsharp lib/net10.0 libraries, Logging/DI lib/net8.0 and Windows Pkcs runtime library using the freeze inventory; avoid duplicate assemblies. Bind actual compilation command, runtime configuration, output hashes and guest .NET runtime before execution. This lane has not proved compilation or runtime availability.
6. Proposed invocation: `dotnet MergeSmoke.dll C:\OTO-Smoke\output\CAB.partial.pdf C:\OTO-Smoke\input\C.pdf C:\OTO-Smoke\input\A.pdf C:\OTO-Smoke\input\B.pdf`. Repeat A/A with a new output path. Use private fixed guest paths; never pass user documents or host paths.
7. Record source hashes before/after, process exit/time/resource evidence and independent order/duplicate marker oracle. Reopen validation must precede any success claim. Relates to TP-MRG-006, TP-MRG-008, TP-MRG-026, TP-MRG-028, TP-MRG-029; static preparation does not pass these tests. This runner does not implement product Save As.

Technical blockers remaining: reviewed guest transfer/export route; compiled runner and guest runtime binding; enforceable supervision; generated corpus and independent marker oracle; actual smoke evidence. These are readiness work, not a request to repeat human authorization.

qpdf inventory notes unsigned qpdf.exe/qpdf30.dll and bundled runtime signature presence; this does not prove publisher trust. Embedded dependency provenance gaps remain in static intake. Preserve them in admission review. No additional cost, paid support or public redistribution is authorized by this packet.
