# PDFsharp 6.2.4 candidate execution-freeze proposal

Task `P2-W0G-A2-PDFSHARP-CANDIDATE-FREEZE`; run `P02-20260905-1434`;
baseline `4a78212e01d3daee9ae47f0473fb6186df0dccde`.

Result: `FREEZE_PROPOSAL_STATIC_ONLY_NOT_EXECUTION_READY`. This packet binds four
already acquired NuGet byte sets and existing static evidence. It does not admit an
engine, resolve assets, select a permanent vendor, authorize build/load/execution,
or prove .NET 10/Windows x64 support. All 14 current authorization flags in
`candidate-freeze.json` are false. Cost remains `ZERO_ADDITIONAL_COST`.

## Exact proposed graph

| Relationship | Component | Bytes | SHA-256 |
|---|---|---:|---|
| Root | PDFsharp 6.2.4 | 4,625,326 | `5647882339B327CBF6DA9EA3F882EFB335F8D602E6FD45323ADF9E78DA53CF67` |
| Direct | Logging.Abstractions 8.0.3 | 802,188 | `E4C498D5A13051B4577A148F1D8C3470167215C507E2392069B75DC61322BB74` |
| Direct | Cryptography.Pkcs 8.0.1 | 1,078,389 | `28C348909DF243FE4EE962213E3C8021AAECBC8321929DBA03A69BCB92A49DE5` |
| Transitive via Logging | DependencyInjection.Abstractions 8.0.2 | 278,862 | `51F2DF1100245F10DA54F0BB7E813F277155117777D4FBBAB902214E27372606` |

The proposal selects PDFsharp `net10.0`, nearest compatible dependency groups
`net8.0`, and RID `win-x64`. Those are metadata proposals, not a NuGet resolver
result. The manifest binds all nine PDFsharp net10 DLL candidates, Logging runtime,
three generator and build-target surfaces, Pkcs generic/Windows runtime candidates,
and DI runtime/build placeholder. Actual compile/runtime/build/analyzer selection is
`NOT_RESTORED_NOT_RESOLVED`.

## Allowlist, denylist, and restore design

Only the four exact package identities, sources, sizes, and hashes may be considered
later. No range/latest resolution, cache/fallback, online feed, extra package,
alternate TFM/RID, qpdf, Poppler, or MuPDF is allowed.

The proposed build policy denies Logging generators by default with future setting
`DisableMicrosoftExtensionsLoggingSourceGenerator=true`. Its effectiveness and
functional impact still require an authorized build review. Any unexpected target,
`Import`, `UsingTask`, `Exec`, analyzer, generator, dependency, or asset stops work.
Runtime policy denies dynamic/reflection plugins, broad DLL search, process/shell,
viewer/browser/URI activation, inherited credentials/proxy, and network egress.

No restore was run. A later separately authorized procedure would stage only the
four hash-matched archives in a read-only guest-local feed, use a NuGet configuration
with `<clear />`, one local source and exact ID mapping, and enable
`RestorePackagesWithLockFile=true` in an isolated PoC project. A bounded bootstrap
restore would emit `packages.lock.json`, `project.assets.json`, generated props/
targets, resolved assets, and runtime/framework-pack identities for independent
review. Only after lock acceptance could a locked offline replay be considered.
Package targets/analyzers can activate before any PDF opens, so processing consent
cannot implicitly authorize restore or build.

## Planned invocation and outputs

The proposed logical entrypoint is `Oto.Poc.PdfsharpHarness.exe` with argument vector
`run --manifest RUN_INPUT/run-manifest.json`, no shell, a run-owned working directory,
and an explicit environment allowlist. It is not implemented and has no digest, so
this is a blocker rather than an executable command.

The planned adapter accepts ordered inputs behind `IPdfMergeEngine`, writes only the
run-owned staging output, preserves sources and existing targets, keeps synthetic
test passwords memory-only, and emits typed sanitized status. Candidate PDF, status,
resource, module/process/network, and quarantined console outputs are named exactly
in the manifest; every `publishAuthorized` value is false. Independent oracle review
must veto publication on mismatch or inconclusive evidence.

## License, provenance, security, and SBOM

All four nuspecs declare MIT. The three Microsoft packages contain exact license and
third-party-notice evidence. PDFsharp has no standalone license/notice member; its
applicable MIT text and holder notice remain a `PRE-005` requirement. The Microsoft
packages declare source commits; PDFsharp names a repository/branch but no commit.
No source-to-binary reproduction occurred.

Existing signature evidence reports offline NuGet verification success with
`RevocationStatusUnknown`. Current advisory and scanner disposition are absent.
Logging build activation and Pkcs Windows crypto/interop/network vocabulary remain
security holds; vocabulary is not reached-call proof. The record is an interim
four-package inventory, not a deployment SBOM. The charter's SBOM format, resolved
assets, SDK/runtime/framework packs, Windows dependencies, harness/adapter, loaded
modules, and provenance treatment remain open.

Exact prerequisites are listed in `sbomLicenseSecurity`:

- `PRE-004`: human candidate/hash acceptance, authorized lock/assets resolution,
  runtime graph, harness/adapter digests, SBOM format, and provenance treatment.
- `PRE-005`: named License/Cost reviewer, exact PDFsharp notice/text, Microsoft
  notice delivery, internal-PoC versus redistribution boundary, support owner,
  zero-cost acceptance, and expiry.
- `PRE-006`: named Security reviewer, trust/revocation/advisory/scanner disposition,
  generator policy, Pkcs interop/network reachability, PDFsharp call-path review,
  and exact DLL/process/network/temp controls.

`PRE-012` remains human-only. No candidate/Sandbox execution, restore/build/load,
PDF creation/opening/processing, network acquisition, or paid commitment occurred.

## Verification

From repository root, against the already approved read-only evidence root:

```powershell
pwsh -NoProfile -File poc/intake/execution-freeze/pdfsharp/Verify-Freeze.ps1 `
  -ArtifactRoot 'D:\OTO\pertamina-oto\artifacts\evidence\P02-20260905-1434'
```

The verifier reads JSON/text evidence and hashes raw archive/receipt bytes. It does
not extract packages, evaluate MSBuild, restore/build, load code, launch Sandbox,
open a PDF, or use the network. The packet has no usable expiry until a human sets
one; any event in `freezeValidity.invalidatedBy` invalidates it first.
