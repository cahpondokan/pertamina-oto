# Wave 0D governance packet review

Task P2-W0D-A7-GOVERNANCE-PACKET-REVIEW; run P02-20260905-1434.
Date 2026-09-06 Asia/Jakarta. Baseline 07b5621c8d5e26793e4d8658aef9f8e23b3a86ec.
Branch task/P2-A7-wave0d-review; assigned agent-7-wave0d-review worktree.
Result: REVIEW_COMPLETE_WITH_LIMITATIONS. Engineering governance recommendation,
not legal advice, vendor selection, human sign-off or permission to acquire/run.
ZERO_ADDITIONAL_COST. Processing/restart/Sandbox launch NOT_AUTHORIZED.

## Component decisions recommended to Agent 0

| Exact component | Acquisition recommendation (not permission) | Use recommendation |
|---|---|---|
| Microsoft.Extensions.Logging.Abstractions 8.0.3 | Present exact official NuGet package for acquisition-only review | Hold restore/build/use pending archive, MIT notices, analyzer/buildTransitive and security review |
| System.Security.Cryptography.Pkcs 8.0.1 | Present exact official NuGet package for acquisition-only review | Hold use pending selected Windows runtime assets, cryptography interop, notices and security review |
| Microsoft.Extensions.DependencyInjection.Abstractions 8.0.2 | Present exact official NuGet package for acquisition-only review | Hold use pending notices/buildTransitive and security review |
| qpdf 12.4.1 / qpdf-12.4.1-msvc64.zip | Conditional next acquisition candidate after official asset/digest revalidation and Agent 6 review | Structural oracle only; no renderer or complete-conformance claim; execution separately gated |
| MuPDF 1.28.3 | Recommend deferral of acquisition/build | AGPL use scope, exact source/build/submodules/resources/toolchain unresolved; commercial route excluded |
| Poppler 26.07.0 / poppler=26.07.0=h6618ce5_3 | No new acquisition proposed; retain existing installation unchanged | Hold PDF use, copying into guest, packaging and redistribution until origin/license/closure and exact internal-only use accepted |

These are recommendations for individual components, not governed PoC dispositions.
No new package, binary, source archive, build tool, paid service or runtime was acquired.
No executable candidate (including version switches) was invoked by Agent 7.

## Exact dependency packet

Input closure.json SHA-256:
3130E92422B3449A26D30B80B3EE3030CC1699F8810D439AFB54A767CCEA857A.
Four nodes/three edges: PDFsharp 6.2.4 net10.0 -> Logging 8.0.3 and Pkcs 8.0.1;
Logging net8.0 -> DependencyInjection 8.0.2. Pkcs and DependencyInjection net8.0
have empty declared dependency groups. Selection of net8.0 under net10.0 is a
metadata proposal; actual asset selection/SDK pruning/RID graph has not been restored.
Do not union other TFM groups or imply empty package dependencies mean no OS runtime.
Pins are lower-bound choices, not latest/security-approved versions. PDFsharp is
already acquired and must not be redownloaded. Its known hash remains in the Wave 0B
intake; closure.json's null binarySha256 does not supersede that artifact evidence.

Only these three new acquisition URLs may be put in the next package decision:
- https://api.nuget.org/v3-flatcontainer/microsoft.extensions.logging.abstractions/8.0.3/microsoft.extensions.logging.abstractions.8.0.3.nupkg
- https://api.nuget.org/v3-flatcontainer/system.security.cryptography.pkcs/8.0.1/system.security.cryptography.pkcs.8.0.1.nupkg
- https://api.nuget.org/v3-flatcontainer/microsoft.extensions.dependencyinjection.abstractions/8.0.2/microsoft.extensions.dependencyinjection.abstractions.8.0.2.nupkg

All three frozen official catalog records declare MIT. Source links checked 2026-09-06:
[Logging 8.0.3](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/8.0.3),
[Pkcs 8.0.1](https://www.nuget.org/packages/System.Security.Cryptography.Pkcs/8.0.1),
[DependencyInjection 8.0.2](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection.Abstractions/8.0.2).
Metadata supports a no-license-fee proposal, with copyright/permission notices
preserved and warranty limitations retained; it does not clear each bundled asset.
No commercial support commitment or subscription is accepted. Review actual license
files, third-party notices and exact archives after separately approved acquisition.
Catalog packageHash values are remote service assertions, not local hash verification.
Freeze those values and catalog references, compare after acquisition, independently
record SHA-256/size, signature and revocation results, and inventory native/build assets.

Logging metadata includes source generators; a build can execute generators even
without PDF processing. Acquisition permission must explicitly exclude restore/build.
Pkcs Windows runtime assets and DI buildTransitive targets likewise require inspection.
Any newly discovered dependency, version change, source redirect outside the approved
hosts, paid condition, TFM/RID change or unresolved security result stops dependent work.
A resolved deployment SBOM must include framework/runtime and relevant resources;
this four-node metadata graph is not that SBOM.

## qpdf: Apache and exact artifact limitations

Upstream declares Apache-2.0: [license](https://qpdf.readthedocs.io/en/stable/license.html).
The license provides no-charge copyright/patent grants subject to conditions, including
license delivery, retention of relevant notices, notices for modified files, and NOTICE
handling when applicable. It does not grant general trademark rights or a warranty.
Review licenses of each bundled dependency separately. These obligations are not met
by recording only the Apache identifier. [Apache text](https://www.apache.org/licenses/LICENSE-2.0).

Exact release: https://github.com/qpdf/qpdf/releases/tag/v12.4.1 . Wave 0C manifest
proposes qpdf-12.4.1-msvc64.zip and advertised SHA-256
3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6.
The release page was reached in this review, but the browser's returned page did not
expose that digest; it is inherited remote metadata, not independently corroborated
by Agent 7 or computed from local bytes. Revalidate exact release asset URL/digest
through official metadata before authorizing acquisition; no alternative asset allowed.
DLL/runtime/compression/crypto licenses, signing/provenance and source-build binding
remain unverified. Do not infer security clearance from Apache licensing.

Structural success cannot satisfy visual fidelity. Warnings/recovery and raw output
must follow a reviewed oracle/sanitization policy; encryption output may be sensitive
per the earlier research. No native CLI launch, repair or rewrite is approved here.

## MuPDF: AGPL and source-build deferral

[Official license guide](https://mupdf.readthedocs.io/en/latest/license.html) identifies
AGPL and a separate commercial route. The latter is outside ZERO_ADDITIONAL_COST;
no trial, sales request or paid fallback is proposed. The OSS route can avoid license
fees only with its applicable obligations fulfilled; free does not mean unrestricted.
[Release history](https://mupdf.com/releases/history) confirms 1.28.3 is source-only.
Do not substitute another version's binary and label it 1.28.3.

Human review must define modification, linking/combination, recipients/conveyance,
corresponding source, notices and any network interaction. AGPL section 13 addresses
source availability to remote users of modified network-interactive versions; other
copying/conveyance duties still require review. An offline subprocess is not a blanket
exemption, and neither mere use nor every generated PDF is automatically a requirement
to publish the unrelated product. Do not make either categorical claim.
[AGPL text](https://spdx.org/licenses/AGPL-3.0-only.html), checked 2026-09-06; exact
license version/options must be confirmed against admitted source notices.

Defer until exact source digest/commit, submodule graph, build recipe/flags, compiler
entitlement at no additional cost, third-party codecs/fonts/notices, output digest,
SBOM and accepted use scope are frozen. No source acquisition or build is authorized.
This deferral does not eliminate the visual oracle requirement or reduce Merge scope.

## Existing Poppler: installation is not admission

Supplemental Agent 3 in-progress inventory was read from its separate worktree;
it is not part of checkpoint 07b5621c. Agent 0 must bind its final commit and reconcile
any changes before using this review. At observation: inventory.json SHA-256
15DE3F944A1B9BAB0F52BCA1AB1915007E67A8A987911CB99D3D6F5D801B4098;
version-results.json SHA-256
E45B904230BBC313B0FA8CE412C2D3DCE755EF55ADF9BD3FDEFCA85710075340.
Inventory contains 445 files. Agent 3 reports pdfinfo/pdftoppm -v exit 0, version
26.07.0. Agent 7 independently read the existing runtime manifest (win32/x64,
packageSpec poppler=26.07.0=h6618ce5_3) and hashed these existing bytes without execution:

| Relative runtime file | SHA-256 |
|---|---|
| Library/bin/pdfinfo.exe | 4065076CC881D3BFA288A1C93B5CA7EBED248D6A380B9ABCE8B0D4A80E9524CD |
| Library/bin/pdftoppm.exe | 3F2440140FEF98DDD1EF0772A10D8E58DC3564A9B15A14B4611B5BBD5BDCD37F |
| Library/bin/poppler.dll | A89BA7BB062E2463C1AB61704337D6E0612398510BE08736D2862CF71900D3DD |

Runtime root is the existing codex-primary-runtime dependencies/native/poppler.
Three hashes agree with Agent 3 inventory; no full 445-file independent audit claimed.
Agent 3 signatureStatus=Valid is an inherited observation, not reproduced here and
not proof of original upstream-to-packager-to-runtime chain or license compliance.
The inspected inventory names expose no COPYING/LICENSE/NOTICE file. That is a
license-evidence gap, not proof of infringement or a waiver. The manifest contains
no source URL, original archive digest, recipe commit or corresponding-source offer.

[Packager recipe metadata](https://github.com/conda-forge/poppler-feedstock/blob/main/recipe/meta.yaml)
labels Poppler GPL-2.0-or-later and names COPYING. It is mutable current metadata,
not proof that this exact local build came from that recipe. Upstream COPYING was
not retrievable in this review. Exact build provenance/license/third-party notices
must therefore remain unresolved. [GPL reference text](https://spdx.org/licenses/GPL-2.0-or-later.html)
distinguishes running from copying/distribution/modification; redistribution of
covered binaries requires an applicable corresponding-source route and preserved
license notices. Human review must determine exact recipients and integration scope.
Do not equate an internal CLI with automatic product-linking/redistribution clearance.

[Upstream](https://poppler.freedesktop.org/) currently lists 26.09.0; existing 26.07.0
is not asserted current or patched. No upgrade is authorized. Bind local hashes and
recheck after runtime updates; refuse silent PATH fallback or replacement. Runtime
cache removal/update can break repeatability; do not freeze availability by copying
binaries now. A later guest copy needs explicit scope, notices and exact allowlist.
Font/encoding/codec/runtime DLL origins and terms require component-level inventory.
No product bundling, upload, or external redistribution is proposed. No added fee
was incurred, but existing installation alone proves neither free entitlement for
every new use nor a maintained support SLA. Recommend HOLD_USE_PENDING_EVIDENCE.

## Gate and human decision packet

P2-D01/P2-D03 and PRE-004/PRE-005 remain partial; static metadata is not admission.
PRE-006 awaits Agent 6; PRE-007/PRE-008 require frozen corpus and real independent
oracle; PRE-009/PRE-010 require exact guest controls/budgets/instrumentation;
PRE-011 needs run access/retention/expiry and safe persistent evidence export;
PRE-012 remains NOT_AUTHORIZED. Existing temporary combined human roles and
independence limitation under P2-W0-ROLE-001 remain accepted; AI is not the approver.

Sandbox template has no persistent export path. A later writable quarantine mapping
is a changed host-write boundary requiring review, not permission implicit in this
packet. Feature Enabled or restart consent alone is not launch/processing consent.
No restart, launch, system mutation or control weakening is performed in this review.

Proposed decision A (human must supply identity/role, date/expiry, evidence reference):
'I authorize acquisition-only of Microsoft.Extensions.Logging.Abstractions 8.0.3,
System.Security.Cryptography.Pkcs 8.0.1 and Microsoft.Extensions.DependencyInjection.Abstractions
8.0.2 from the three exact official URLs in the reviewed closure manifest SHA-256
3130E92422B3449A26D30B80B3EE3030CC1699F8810D439AFB54A767CCEA857A,
into the existing approved run evidence root's agent-2-dependency-intake subdirectory.
ZERO_ADDITIONAL_COST applies. Record source/time/size, remote-hash comparison, local
SHA-256, signature/trust limitations and static license/asset inventory. Do not restore,
build, run generators/assemblies, process documents, change product references or
acquire any additional component. Stop on mismatch, unexpected dependency or cost.'
The exact child directory is a PROPOSED target for this decision, not approved now.

Proposed decision B, only after official asset metadata is frozen:
'I authorize acquisition-only of qpdf 12.4.1 qpdf-12.4.1-msvc64.zip from [verified
exact official release asset URL], expected SHA-256 [revalidated digest], into
[exact run intake directory]. Accept only the no-cost OSS route for static review.
No extraction to executable search paths, launch, PDF processing, restore or additional
runtime acquisition is included. Retain all notices and stop on provenance mismatch.'
Unfilled brackets block this decision; do not present it as ready to execute.

Proposed decisions C/D:
'I defer MuPDF 1.28.3 acquisition/build/use until its AGPL scope, exact source/build
and no-cost toolchain packet are accepted. I hold Poppler 26.07.0 PDF use/copying/
redistribution pending final hash-bound inventory, provenance, notices, source terms,
security review and a separately approved internal-only execution packet. No paid
replacement or reduced fidelity scope is authorized. Review owner/date: [human record].'

After acquisition, license/security review and complete manifests, a separate
APPROVED_FOR_POC_ONLY and execution record must bind exact artifacts, graph/SBOM,
charter, environment, executable/argument allowlist, oracle/corpus/config, safety
ceilings, cancellation/cleanup, evidence policy and human reviewers. Only a human
may issue an eventual governed engine disposition; none is issued here. Expiry,
new terms/findings, changed component/TFM/RID/toolchain or evidence digest require
controlled re-freeze, with previous records retained and no silent upgrade.

## Verification / mandatory handoff

Read AGENTS, Shared Agent Contract, Phase 2 prompt, Project State, Decision Register,
Wave 0D task card, integrated dependency/oracle/Sandbox preparation and prior governance
review. Exact baseline and task branch verified; initial status clean.
`pwsh -NoProfile -File poc/intake/dependency-closure/Build-Closure.ps1 -Verify`:
exit 0; exact identities/MIT/groups, 4 nodes/3 edges and deterministic text-only
manifest PASS. No mutation by -Verify. Input hashes independently calculated above.
Browser checks read metadata/license pages only, with no artifacts downloaded.
GNU and freedesktop license fetch failures are disclosed; license-reference text
was read at SPDX; no failed fetch is treated as affirmative license evidence.

Product builds/tests, performance/resource and PDF experiments: NOT_RUN, outside scope.
No document content, credentials or sensitive path stored. Only own wave0d review
files changed. Agent 0 should integrate this review after Agent 3 final inventory
and Agent 6 security findings, reconcile supplemental hashes, then present the
completed bounded decisions. Review complete; Phase 2 exit remains blocked.
Trace: FEAT-PDF-005, FEAT-SEC-019, FEAT-SEC-020, TECH-DEC-005, TECH-DEC-006,
P2-D01, P2-D02, P2-D03, PRE-004, PRE-005, PRE-006, PRE-007, PRE-008, PRE-009,
PRE-010, PRE-011, PRE-012; PDF001-E00 and PDF001-E06 preparation only.
