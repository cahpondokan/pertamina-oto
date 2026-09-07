# Wave 0E three-package acquisition and static intake

Task P2-W0E-A2-NUGET-ACQUISITION-ONLY; run P02-20260905-1434.
Status: STATIC_INTAKE_COMPLETE_WITH_TRUST_LIMITATIONS; not execution admission.
Authority P2-W0-DEP-002; ZERO_ADDITIONAL_COST.
Baseline ffac0d59f0e40849d5950d6b2029c82f72fec32a; branch task/P2-A2-pdfsharp-intake.
Frozen closure SHA-256 3130E92422B3449A26D30B80B3EE3030CC1699F8810D439AFB54A767CCEA857A matched before acquisition.

## Exact artifact evidence

Raw root: D:\OTO\pertamina-oto\artifacts\evidence\P02-20260905-1434\agent-2-dependency-intake . Only original nupkg and acquisition receipt JSON are there. No archive extracted to an execution/search path.

| Package | Version | Bytes | SHA-256 |
|---|---|---:|---|
| Microsoft.Extensions.Logging.Abstractions | 8.0.3 | 802188 | E4C498D5A13051B4577A148F1D8C3470167215C507E2392069B75DC61322BB74 |
| System.Security.Cryptography.Pkcs | 8.0.1 | 1078389 | 28C348909DF243FE4EE962213E3C8021AAECBC8321929DBA03A69BCB92A49DE5 |
| Microsoft.Extensions.DependencyInjection.Abstractions | 8.0.2 | 278862 | 51F2DF1100245F10DA54F0BB7E813F277155117777D4FBBAB902214E27372606 |

results.json includes exact HTTPS URLs, UTC receipt times, HTTP 200/effective URL/zero-redirect receipts and complete local/catalog SHA-512 values. All three catalog SHA-512 comparisons passed. curl used --proto =https --max-redirs 0 (without following redirects), --fail and bounded timeouts. Only three frozen URLs requested; no latest/version lookup or extra artifact.

## Static findings

- Logging: 67 entries; Pkcs: 31; DI: 26. Every entry has SHA-256/length and path-safety check. Before/after package hashes remained unchanged.
- Nuspec identity and MIT expression match exactly. All three include LICENSE.TXT and THIRD-PARTY-NOTICES.TXT; full legal texts and nuspec metadata are retained in tracked text reports. Notices describe upstream .NET third-party resources; their presence does not automatically mean every listed resource is deployed by these packages. Agent 7 must assess attribution/redistribution applicability.
- Selected net8.0 dependency group: Logging -> DI >=8.0.2; Pkcs and DI have none. No selected-group surprise. Other framework groups remain visible in nuspec and do not authorize acquisition of those additional packages.
- Logging contains Roslyn 3.11/4.0/4.4 source-generator/analyzer DLLs and localized resources. Its buildTransitive targets gather/remove analyzers according to Roslyn support and DisableMicrosoftExtensionsLoggingSourceGenerator, plus unsupported-framework warning targets. No build target was executed; any later build requires review of these activation surfaces. No policy switch was changed or warning suppressed.
- Pkcs contains runtimes/win/lib/net6.0, net7.0, net8.0 DLL/XML assets and buildTransitive compatibility warnings/placeholders. DI contains buildTransitive compatibility warnings/placeholders. Targets were read as text, not evaluated by MSBuild.
- No native/ directory or .exe/.so/.dylib named entry was found. This filename finding does not establish absence of P/Invoke, native OS cryptography, malicious code, telemetry or runtime dependencies inside managed DLLs. PE/import/IL analysis and actual RID selection remain review gaps.

## Signature and verification

Installed SDK command per package: NUGET_CERT_REVOCATION_MODE=offline; dotnet nuget verify <artifact> --all --configfile <existing repository NuGet.Config> --verbosity normal --force-english-output.
All three returned exit 0 and Successfully verified. All three also contain RevocationStatusUnknown: cached revocation evidence was insufficient. Logs retain signer/timestamp fingerprints and trust detail with raw root replaced by RUN_EVIDENCE. No fresh online revocation assurance, malware clearance, vulnerability clearance or source-build reproducibility is claimed. No trust policy mutation occurred.

Inspect-Artifacts.ps1 performed archive-only reads, original hash rechecks, identity/license/selected dependency checks, inventories and SDK signature verification; exit 0. The compact console table was blank due to OrderedDictionary formatting; persisted results.json was reread and confirmed all three entries with exit 0 and revocationUnknown true. This display issue does not alter inspection results.

No restore/build/install/reference, assembly/analyzer/generator load, PDF processing, extra dependency, Sandbox launch, restart, system mutation or paid commitment. No production/user document or secret used. Product tests and resource/performance measurements NOT_RUN because component execution is prohibited. Raw packages are ignored by repository policy; only allowed text reports/scripts are tracked.

## Review handoff and remaining gates

Agent 6: independently rehash all three exact byte sets; assess revocation/scanner/vulnerability gaps, build/analyzer activation and runtime/native imports. Agent 7: review embedded MIT and third-party notices, source/provenance, runtime/redistribution obligations and cost acceptance. Zero license fee is supported by bundled MIT terms, not a waiver of notices or an offer of paid support. All paid services/licenses/support are excluded.

No mismatch or cost blocker occurred. Processing remains blocked until dependency/security/license disposition, complete runtime/asset freeze, isolated environment, corpus/oracle and human execution approval. Do not use successful signature verification as an engine approval. Agent 0 integrates this allowed path after independent review; no merge to main by Agent 2.

Trace: FEAT-PDF-005; TECH-DEC-005; TECH-DEC-006; PRE-004; PRE-005; PRE-006; P2-D01; P2-D03; P2-W0-DEP-002.
