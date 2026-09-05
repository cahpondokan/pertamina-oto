# Proposed PDFsharp net10.0 dependency closure

Task P2-W0C-A2-DEPENDENCY-METADATA; run P02-20260905-1434.
Status: METADATA_PROPOSAL_COMPLETE; acquisition and processing NOT_AUTHORIZED.
Baseline 099af9d40810f54277f82baeee80573370427208; branch task/P2-A2-pdfsharp-intake.
ZERO_ADDITIONAL_COST; no restart, Sandbox launch, system mutation, provider execution, package download or restore.

## Exact proposal

| Component | Proposed pin | Selected dependency group | Outgoing dependency |
|---|---|---|---|
| PDFsharp | 6.2.4 | net10.0 | Logging.Abstractions >=8.0.3; Pkcs >=8.0.1 |
| Microsoft.Extensions.Logging.Abstractions | 8.0.3 | net8.0 | DependencyInjection.Abstractions >=8.0.2 |
| System.Security.Cryptography.Pkcs | 8.0.1 | net8.0 | None |
| Microsoft.Extensions.DependencyInjection.Abstractions | 8.0.2 | net8.0 | None |

All four official catalog records declare MIT. Three new package acquisitions are proposed; reuse the previously acquired PDFsharp 6.2.4 artifact, with Wave 0A hash/signature evidence. closure.json contains every exact proposed flat-container URL. URLs are an approval packet, not execution permission. No wildcard/latest/feed-wide acquisition is proposed.

Framework rationale (inference from metadata): choose exact net10.0 for PDFsharp and the nearest compatible .NET group net8.0 for the three dependencies. Do not union dependency groups across frameworks. System.Formats.Asn1, System.Buffers, System.Memory, System.Security.Cryptography.Cng, Microsoft.Bcl.AsyncInterfaces and System.Threading.Tasks.Extensions belong to nonselected groups and are excluded from this proposal. The three new pins are the exact lower bounds required by this isolated graph, not a claim that they are latest, best patched or approved. No other top-level dependency or central override is assumed.

This is the complete declared package dependency closure for that selection, not a restored asset lock or full deployment SBOM. SDK/runtime packs, host framework references/pruning, RID-specific asset resolution, analyzer imports and any future harness dependencies must be reconciled in a separately authorized restore. Empty dependency groups do not mean an assembly has no runtime or OS dependencies.

## Source evidence and reproducibility

Four official registration JSON leaves and their exact catalog URLs were queried on 2026-09-05 around 18:30 UTC (2026-09-06 around 01:30 Asia/Jakarta); exact per-record times are in metadata/*.json. No packageContent URL was requested. Only JSON metadata was stored. Public gallery cross-checks:
- https://www.nuget.org/packages/PDFsharp/6.2.4
- https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/8.0.3
- https://www.nuget.org/packages/System.Security.Cryptography.Pkcs/8.0.1
- https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection.Abstractions/8.0.2

The catalog includes upstream packageHash/packageHashAlgorithm and packageEntries metadata. These are publisher/service assertions, not locally verified artifact digests or a substitute for a signed-package check. Catalog entry lists may differ from the final signed archive inventory; retain Wave 0A raw artifact inventory as separate evidence. Missing vulnerabilities fields are not a security clearance.

Build-Closure.ps1 regenerates closure.json offline from frozen metadata. -Verify checks identity, exact selected groups/ranges, license expressions, 4 nodes/3 edges, deterministic regeneration and text-only scope. This is a deliberately bounded schema/invariant checker for these exact records, not a general NuGet resolver. No downloaded assembly is loaded and no source generator is run.

## Runtime/build/license observations and gaps

- Logging.Abstractions metadata lists logging generator assemblies for Roslyn 3.11, 4.0 and 4.4 and buildTransitive targets. Future build imports/analyzers need review, even though no extra package node is required.
- Pkcs metadata lists runtimes/win/lib/net8.0 assets as well as net6.0/net7.0 and buildTransitive entries. A runtimes/win managed assembly is not proof of a bundled native binary; platform interop and guest cryptography availability remain unverified.
- DependencyInjection.Abstractions has buildTransitive targets/placeholders. Exact selected imports must be checked against the future SDK.
- Native/PInvoke/bundled resource provenance, notices, package signatures, revocation freshness, local malware scan and vulnerability disposition remain open. Metadata-only work cannot close these gates.
- MIT means no license fee is proposed, subject to required copyright/permission notices and no-warranty terms; https://licenses.nuget.org/MIT . Commercial support, paid scanning, paid infrastructure or paid license are excluded. Legal/license acceptance remains a human decision. No new runtime/tool purchase is assumed.

## Acquisition decision and stop rules

Agent 0 may present only the three exact new package URLs in closure.json for human acquisition-only approval. After that separate approval, compare downloaded bytes against upstream hash plus independently recorded SHA-256, verify repository signature/trust, inspect archive licenses/notices/native/build assets, create SBOM and obtain Agent 6/7 review before any restore or processing authorization.

Stop for source/identity/version/license mismatch, unexpected graph/assets, nonofficial redirect, any paid requirement, approval expiry, new package need, security finding without disposition, or any requested restart/system mutation. Version upgrade, new top-level reference, TFM/RID change or framework policy change requires a regenerated/reviewed closure; never silently expand the allowlist. No restart without separate user approval.

## Handoff verification

Official metadata retrieval: exit 0; four identity/version checks passed. Initial sandboxed metadata request failed due to socket restriction; normal metadata-only escalation succeeded. Offline generation and verification: exit 0; 4 nodes and 3 edges, deterministic output, exact dependency pins and MIT expressions passed. Final Git scope/whitespace check recorded in handoff. No product implementation changed, so product build/performance/provider tests were not run. Only this new dependency-closure path is owned by this task; earlier intake is unchanged.

Trace: FEAT-PDF-005; TECH-DEC-005; TECH-DEC-006; P2-D01; P2-D03; PRE-004; PRE-005; PRE-006. Proposed integration order: Agent 0 reviews this packet, Agent 6/7 disposition risks and obligations, human authorizes exact acquisition; processing gate remains separate.
