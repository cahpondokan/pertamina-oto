# PDFsharp 6.2.4 acquisition intake

Task P2-W0A-A2-PDFSHARP-INTAKE; run P02-20260905-1434.
Status: INTAKE_READY_FOR_REVIEW_WITH_LIMITATIONS. No engine admission or processing approval.
Baseline: 5769ccce917d03905d07a1440ee527cb653c9cc4; branch task/P2-A2-pdfsharp-intake.
Authority: P2-W0-ACQ-001, P2-W0-EVID-001; ZERO_ADDITIONAL_COST.

## Acquisition and integrity

Agent 0 supplied the exact artifact after this task's original escalation was rejected because original trusted user approval was not visible to the reviewer. No bypass or second download was attempted by Agent 2. Agent 0 explicitly instructed continuation against the acquired local artifact.

Source: https://api.nuget.org/v3-flatcontainer/pdfsharp/6.2.4/pdfsharp.6.2.4.nupkg
Acquisition UTC reported by Agent 0: 2026-09-05T08:22:26.2033447Z (not independently witnessed by Agent 2).
Artifact: authorized run evidence root / agent-2-pdfsharp-intake/pdfsharp.6.2.4.nupkg; ignored and untracked.
Length independently checked: 4625326 bytes.
SHA-256 independently recomputed before and after inspection: 5647882339B327CBF6DA9EA3F882EFB335F8D602E6FD45323ADF9E78DA53CF67.

## Archive findings

111 entries; every entry inventoried with uncompressed length and SHA-256. Identity PDFsharp 6.2.4 matches nuspec. No unsafe traversal entry or named native/runtime/executable/build/script asset found. This filename inventory does not certify absence of P/Invoke, telemetry or malicious behavior inside managed DLLs.

TFM directories: net8.0, net9.0, net10.0, netstandard2.0. Each contains nine DLLs: PdfSharp, PdfSharp.BarCodes, PdfSharp.Charting, PdfSharp.Cryptography, PdfSharp.Quality, PdfSharp.Shared, PdfSharp.Snippets, PdfSharp.System, plus PdfSharp.WPFonts. XML docs and PDBs are bundled. README and PNG icon are also included. No standalone LICENSE or NOTICE file; nuspec has MIT expression and https://licenses.nuget.org/MIT, copyright 2026 empira. Preserve upstream MIT notices during any later approved packaging; absence of a notice file is not permission to omit attribution.

Each of four dependency groups declares Microsoft.Extensions.Logging.Abstractions version 8.0.3 and System.Security.Cryptography.Pkcs version 8.0.1. NuGet bare versions are lower bounds, not an exact resolved lock. No transitives acquired or graph resolved. Repository metadata names https://github.com/empira/PDFsharp branch master but contains no source commit; exact source-to-binary binding remains unverified.

## Verification

1. Inspect-Package.ps1 -PackagePath <approved artifact>: exit 0; identity, archive path safety, 111 entry hashes, no named unexpected assets and before/after package hash PASS.
2. NUGET_CERT_REVOCATION_MODE=offline; dotnet nuget verify <approved artifact> --all --configfile NuGet.Config --verbosity normal --force-english-output: exit 0, successfully verified repository signature and timestamp with RevocationStatusUnknown warnings. Repository signer: NuGet.org Repository by Microsoft; owner PDFsharp-Team; SHA-256 certificate fingerprint 1F4B311D9ACC115C8DC8018B5A49E00FCE6DA8E2855F9F014CA6F34570BC482D. Timestamp signer fingerprint 4AA03FA22CD75C84C55C938F828E676B9CAECAB33FE36D269AA334F146110A33. Offline cache could not establish revocation status. This is limited verification, not full fresh trust clearance.
3. Initial verify without --configfile failed with exit 1 because user NuGet.Config was inaccessible; explicit existing repository config resolved configuration access without mutation.
4. Installed Get-MpComputerStatus returned Access denied; malware scan NOT_RUN, scanner freshness unknown. No scanner installed, updated or cloud submission requested. Agent 6 must disposition this gap.
5. Product build/tests, fidelity, performance, resource, cancellation, offline provider proof: NOT_RUN; no provider or document execution is authorized here.

## Cost, maintenance, removal and next admission

MIT permits no-license-fee evaluation subject to its terms; no paid support, subscription, API or infrastructure used. Raw package nuspec is included for Agent 7 review. Official references checked during readiness:
https://docs.pdfsharp.net/General/License/License.html
https://docs.pdfsharp.net/PDFsharp/History.html
https://www.nuget.org/packages/PDFsharp/6.2.4

6.2.4 release history includes encrypted PDF interoperability, content-array, JPEG and DecodeParms fixes. This shows maintenance activity, not absence of vulnerabilities or an SLA. No unsupported claim of guaranteed offline behavior or zero telemetry is made.

Removal path: this intake introduces no PackageReference or product dependency. Preserve evidence according to approved retention; future deletion must target only exact owned evidence paths under Agent 0 authority.

Next admission is PROPOSED, not approved: Agent 6 reviews signature revocation limitation, scanner gap and static/binary risks; Agent 7 reviews MIT notices, transitive obligations and no-cost disposition. Freeze exact dependency graph only under new acquisition authority. Then Agent 0 obtains APPROVED_FOR_POC_ONLY plus corpus/oracle/environment/execution authorization. PDF-001 trace FEAT-PDF-005; TECH-DEC-005 and TECH-DEC-006; PRE-004/PRE-005/PRE-006; P2-D01/P2-D03. No PDF-002/003 execution or product graph change.
