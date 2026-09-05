# Independent oracle research - Wave 0C

Task: P2-W0C-A3-INDEPENDENT-ORACLE-RESEARCH. Checked: 2026-09-06 Asia/Jakarta.
Baseline: 9ad31539d1fe38c7637d35d67b302e305d1641b5; branch task/P2-A3-corpus-oracle.
Status: RESEARCH_COMPLETE; acquisition and execution NOT_AUTHORIZED.
Cost: ZERO_ADDITIONAL_COST. No restart, system mutation, Sandbox launch, binary,
package, source archive or tool acquisition. Only primary-source web pages read.
Existing corpus/comparator and product graph are unchanged.

## Candidate facts

### ORACLE-QPDF-12.4.1 - structural candidate

Fact: qpdf 12.4.1 is published in the upstream release listing, dated 2026-08-27.
The release lists Windows x64 ZIP variants. Proposed later acquisition is exactly
qpdf-12.4.1-msvc64.zip, with upstream-advertised SHA-256
3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6.
This is remote metadata, NOT a locally recomputed or authenticated artifact hash.
Source: [official release](https://github.com/qpdf/qpdf/releases/tag/v12.4.1).

Fact: upstream license is Apache License 2.0. Evaluate its notice/license terms
and every bundled dependency separately; no paid support or license is proposed.
Source: [upstream license](https://qpdf.readthedocs.io/en/stable/license.html).

Fact: qpdf is a PDF structural transformation library/tool, not a renderer.
Source: [overview](https://qpdf.readthedocs.io/en/stable/overview.html).
Its check mode tests syntax/structure but does not establish complete PDF
conformance or correct page content. It may recover damaged input; warnings and
recovery must not be silently accepted. Check output can include encryption
information, including recoverable user passwords in some older encrypted files.
Therefore raw stdout must not become ordinary evidence, and encrypted-input use
needs a separate safe-output/password design before authorization.
Source: [CLI documentation](https://qpdf.readthedocs.io/en/stable/cli.html).

Inference: use an isolated native CLI child process from the net10.0 harness,
not a managed package or in-process native bridge. Windows asset availability
supports feasibility, not verified Windows/net10 compatibility or offline proof.
Resolved DLL/runtime/crypto/compression dependencies, licenses, artifact trust,
SBOM and current advisory review remain pending acquisition-only admission.
No CVE-free claim is made. Native parser crashes/resource exhaustion remain risks.

### ORACLE-MUPDF-1.28.3 - render/text candidate

Fact: upstream identifies MuPDF 1.28.3 (2026-08-26) as source-only; 1.28.2 is also
source-only. Do not substitute a 1.28.1 Windows binary while labelling it 1.28.3.
Source: [release history](https://mupdf.com/releases/history).
Exact source tag: [ArtifexSoftware/mupdf 1.28.3](https://github.com/ArtifexSoftware/mupdf/tree/1.28.3).

Fact: GNU AGPL v3 license text is included in that tag. Upstream offers an open
source route and a separate commercial route. Only the open source route is in
this research; commercial licensing/support is excluded by ZERO_ADDITIONAL_COST.
No conclusion that subprocess use exempts the project from license obligations
is made. Human license review must accept the exact local PoC use, modifications,
source/notice obligations and any sharing; no product redistribution is proposed.
Sources: [tagged COPYING](https://github.com/ArtifexSoftware/mupdf/blob/1.28.3/COPYING),
[upstream license guide](https://mupdf.readthedocs.io/en/latest/license.html).

Fact: mutool draw renders images including PNG and extracts plain/structured text.
Its options control page selection, page box, resolution, colorspace and anti-aliasing.
An ignore-errors mode exists; it must not be used to make oracle outcomes pass.
Source: [draw documentation](https://mupdf.readthedocs.io/en/latest/tools/mutool-draw.html).
Fact: mutool show inspects PDF objects/streams, supplementing render observations;
raw output may contain document data.
Source: [show documentation](https://mupdf.readthedocs.io/en/latest/tools/mutool-show.html).

Fact: upstream documents a Windows Visual Studio solution and source submodules.
Source: [install/build guide](https://mupdf.readthedocs.io/en/latest/guide/install.html).
Inference: source-only 1.28.3 adds a build-provenance and dependency/toolchain gate.
Exact no-cost compiler eligibility, pinned submodule graph, bundled fonts/codecs,
license notices, enabled features, binary architecture/digest and support stance
are unresolved. No source build or download is authorized by this report.
Native parser/codec attack surface and broad tools (including scripting) warrant
least-privilege containment. Permit only approved inspection/render operations;
exclude script execution, viewers, repair/rewrite, OCR/model acquisition and links.
A net10.0 subprocess is a proposed boundary, not tested compatibility.

## Independence, evaluation order and oracle design

Inference: qpdf and Artifex MuPDF have distinct upstream implementations/maintainers
from PDFsharp and the local Python generator. This supports candidate independence,
not proof of independence of an uninspected binary graph. Inspect actual provenance,
shared dependencies and packaging at intake. MuPDF show and MuPDF draw are one
engine, not two independent votes. qpdf plus MuPDF offers complementary checks.

Recommendation: evaluate qpdf structural intake first because an exact Windows ZIP
is listed; evaluate MuPDF second only after its no-cost source/build/license route
is accepted. If that cannot be accepted, defer MuPDF and research a replacement;
do not buy a license, assume a binary build, or claim qpdf closes the visual gate.
These are evaluation priorities, not vendor selection or engine approval.

Both tools are intended here as local processes on local synthetic files. Web
research traffic is not document-processing egress evidence. Later execution must
prove network disabled/zero egress under the approved guest environment. No cloud
API, telemetry exception or server fallback is proposed.

Future oracle must derive observations from actual staging bytes: page count,
ordered page markers and occurrence sequence, dimensions/rotation, structural
properties, per-page render and fresh-process reopen. Compare against the frozen
expected manifest, never regenerate expectations from candidate output. Duplicate
A,A still requires four output pages. Hash binds evidence, not semantic correctness.
Run wrong-count, reordered, deduplicated, same-count-wrong-content, truncated and
corrupt output negative controls after explicit authorization. Any parser repair,
warning, timeout, disagreement or missing evidence prevents a clean validation PASS
until reviewed. Bind validated and published bytes by digest to avoid replacement.
Freeze fonts, page boxes, colorspace, render settings and manual review coverage;
no numeric tolerance is approved here. Visual comparison cannot prove preservation
of invisible metadata, bookmarks/forms, accessibility or full PDF conformance.

Exact trace: FEAT-PDF-005; PDF001-E01, PDF001-E02, PDF001-E03, PDF001-E06;
UT-MERGE-003, UT-MERGE-006, UT-MERGE-009, UT-MERGE-033, UT-MERGE-034;
TP-MRG-001, TP-MRG-006, TP-MRG-008, TP-MRG-028, TP-MRG-029.
Security design also informs PDF001-E04, PDF001-E05 and UT-MERGE-046/TP-MRG-033.
No test in those suites ran or passed during research.

## Exact next human decisions

1. Technical/License/Security authority: accept or reject each exact version for
   acquisition-only review, intended structural/render role and no-cost OSS route.
2. For qpdf, approve the named Windows ZIP and exact release URL/advertised digest;
   specify expected signer/trust verification and allowed redirects/source hosts.
   Independently recompute local SHA-256 after authorized acquisition. Do not run it.
3. For MuPDF, choose a reviewed source-build route for tag 1.28.3 or defer. Freeze
   exact source archive URL/digest, submodule graph and toolchain first; all remain
   unresolved. A version/build change requires a new decision, never silent fallback.
4. Agent 6/7 review exact dependency/resource/license/SBOM/advisory evidence. Paid
   terms, additional tools, mismatched provenance or unknown native dependencies
   stop affected acquisition. Costs cause BLOCKED_BY_COST before commitment.
5. Before execution: approved isolated guest, corpus/expected digests, output
   sanitization, retention/access/expiry, executable/argument allowlist, resource
   ceilings, timeout/cancel/cleanup and review identities. Restart requires separate
   user approval and post-restart checks; this research authorizes neither.

## Verification and handoff

Only this new directory is written. Primary-source pages above were checked on
2026-09-06; stable/latest documentation is mutable and must be frozen/rechecked at
intake. qpdf version is taken from the exact release, not stale search snippets.
MuPDF source-only status is preserved as an acquisition blocker.

Research acceptance: two exact candidates, sources/licenses, role/limitations,
Windows/net10/offline inference, native/dependency/security risks, independence,
priority and acquisition decisions documented. Build/tests/performance/provider
execution: NOT_RUN, inapplicable to this metadata-only change. No PDF bytes present.
No real data, credentials or document paths included. No added cost incurred.

Integration order: Agent 0 reviews this new directory, combines it with Agent 2
metadata and Agent 6/7 security/license findings, then issues a separate exact
acquisition packet. Do not merge the earlier branch commits again if already
integrated; cherry-pick only the Wave 0C research commit after audit.
