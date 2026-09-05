# Agent 7 Wave 0B governance review

Task: P2-W0B-A7-LICENSE-GOVERNANCE-REVIEW. Run: P02-20260905-1434.
Review date: 2026-09-05 Asia/Jakarta. Recommendation: INTAKE_GOVERNANCE_READY_WITH_LIMITATIONS.
Input checkpoint: 42115b25c153717586f74a400a3cdc16c542f1af.
Branch: task/P2-A7-license-review; assigned agent-7-license-review worktree.
Authority: Wave 0B task card and P2-W0-ACQ-001, P2-W0-CORPUS-001,
P2-W0-ENV-001, P2-W0-EVID-001, P2-W0-ROLE-001. ZERO_ADDITIONAL_COST.
This is an engineering recommendation, not legal advice, vendor approval,
APPROVED_FOR_POC_ONLY, engine disposition, or document-processing authorization.

## Identity and provenance

Independently checked existing PDFsharp 6.2.4 nupkg: 4,625,326 bytes;
SHA-256 5647882339B327CBF6DA9EA3F882EFB335F8D602E6FD45323ADF9E78DA53CF67
before and after static inspection. All 111 archive entry lengths/digests match
Agent 2 inventory. Nuspec ID/version/license/dependencies match the reviewed copy.
Reported acquisition source is official NuGet flatcontainer endpoint for that
exact package/version; acquisition itself was performed by Agent 0, not witnessed
by this reviewer. Hash equality proves binding to supplied bytes, not authorship.

Signature entry .signature.p7s is present. Agent 2 reports dotnet nuget verify
exit 0 with offline RevocationStatusUnknown warnings and NuGet.org repository
signature. This review does not independently rerun cryptographic verification;
only the summary is available in the committed intake README. The raw verifier
log is not present beside the package. Preserve exact command/output/certificate
and timestamp evidence and resolve fresh-revocation policy with Agent 6 before
trust clearance. Repository signing is not an independently reproduced source
build. Nuspec gives github.com/empira/PDFsharp, branch master, no source commit;
exact source-to-binary provenance remains unverified. No FAIL_PROVENANCE is
inferred merely from these disclosed limitations, and no full provenance PASS
is claimed.

## License, notice, and cost

The exact nuspec declares MIT and https://licenses.nuget.org/MIT, copyright
2026 empira. It contains requireLicenseAcceptance=false; that flag does not waive
license obligations. There is no standalone LICENSE or NOTICE entry in this
archive. Official upstream license page states MIT and identifies empira Software
GmbH (2001-2026); its text grants use/copy/modification/distribution rights without
a license charge, subject to preservation of copyright and permission notices.
Retain complete applicable upstream notices with any later approved software
packaging. The reference license page contains generic placeholders; do not use
those placeholders in place of the actual holder notice. No full license text
has been vendored by this review. [Official license](https://docs.pdfsharp.net/General/License/License.html),
[NuGet license reference](https://licenses.nuget.org/MIT), checked 2026-09-05.

Recommendation: the reviewed top-level package's stated MIT terms are compatible
with zero-license-fee evaluation. This does not establish every dependency,
bundled resource, runtime, font, distribution channel, or support service is
cleared. Paid support/subscription, purchases, trial-to-paid conversion, paid
cloud/API, paid tools, and new infrastructure remain excluded. Stop with
BLOCKED_LICENSE_COST if an unavoidable additional charge or unresolved cost term
appears. No purchase, registration, paid service or package acquisition occurred.

The NuGet page distinguishes the Core package and advertises professional support
separately. No purchased support, response SLA, warranty or maintenance commitment
is included in this review. MIT's warranty disclaimer must not be rewritten as a
support guarantee. [Exact package page](https://www.nuget.org/packages/PDFsharp/6.2.4),
checked 2026-09-05. Official history lists 6.2.4 encryption interoperability,
content-array, JPEG and DecodeParms fixes. This demonstrates recorded maintenance,
not security clearance or future support. Do not silently upgrade the frozen
candidate. [Release history](https://docs.pdfsharp.net/PDFsharp/History.html),
checked 2026-09-05.

## Dependency and bundled-asset boundary

| Declared TFM | Logging.Abstractions | Cryptography.Pkcs |
|---|---|---|
| net8.0 | Microsoft.Extensions.Logging.Abstractions >= 8.0.3 | System.Security.Cryptography.Pkcs >= 8.0.1 |
| net9.0 | Microsoft.Extensions.Logging.Abstractions >= 8.0.3 | System.Security.Cryptography.Pkcs >= 8.0.1 |
| net10.0 | Microsoft.Extensions.Logging.Abstractions >= 8.0.3 | System.Security.Cryptography.Pkcs >= 8.0.1 |
| .NETStandard2.0 | Microsoft.Extensions.Logging.Abstractions >= 8.0.3 | System.Security.Cryptography.Pkcs >= 8.0.1 |

These are declared minimum ranges, not exact locks. Local nuspec and official
NuGet dependency listing agree. No dependency artifact was acquired or resolved.
The existing BCL-only harness lock is empty and cannot serve as PDFsharp's SBOM.
Before execution choose exact target TFM/RID/runtime, resolve a reviewed exact
closure under separate authority, record component source/version/hash/license,
direct/transitive/native/runtime edges and notices, and approve SBOM format.
Do not mistake an installed runtime or cached dependency for admitted evidence.

Four lib TFMs each contain PdfSharp, BarCodes, Charting, Cryptography, Quality,
Shared, Snippets, System and WPFonts DLLs plus symbols/XML documentation. README,
PNG icon and NuGet metadata/signature are bundled. Archive names expose no native,
runtimes, build/buildTransitive or script assets. This does not exclude embedded
resources, P/Invoke or dynamically selected runtime dependencies. In particular,
WPFonts resources need origin/notice review before claiming all bundled assets
cleared; filename and top-level MIT expression alone do not prove each resource's
provenance. No assembly was loaded, executed or reflected over.

Runtime compatibility in metadata is not verified offline operation. Activation,
egress, active-content handling and document correctness remain security/PoC
questions. Future packaging should include only reviewed required components,
with a tested removal path; no trimming or asset removal is authorized here.
Current intake has no product dependency to remove. Preserve raw evidence under
retention policy rather than deleting it during a removal drill.

## Evidence and next-gate recommendations

Twelve corpus/oracle source hashes independently match source-digests.json at
this checkpoint. Corpus self-tests and harness tests are upstream preparation
results, not re-executed here. Actual independent parser/renderer admission,
hostile corpus, complete experiment results and OS-isolated offline evidence
are absent. MATCH_ONLY/publishAuthorized=false and NetworkDisabledVerified=false
are appropriate limitations; do not convert them into PDF001-E06/E00 PASS.
Preparation expiry=null means unassigned, not perpetual retention.

| Gate | Recommendation at this review |
|---|---|
| P2-D01 | Acquisition/preparation accepted for exact PDFsharp artifact only; full scope/admission still pending |
| P2-D02 | Synthetic preparation authority accepted; exact execution corpus/oracle and numeric policies pending |
| P2-D03 | Top-level MIT/no-fee recommendation positive with notice constraints; graph/assets/security/support acceptance incomplete |
| PRE-001 | Prior local bootstrap authority available; Agent 0 revalidates scope |
| PRE-002 | Prior SDK/feed approval available; not blanket admission of PDFsharp closure |
| PRE-003 | Wave 0B review task satisfied; execution task/paths/trace still to freeze |
| PRE-004 | Exact artifact bound; resolved closure/SBOM and PoC-only admission missing |
| PRE-005 | This review is evidence, not human acceptance of complete closure |
| PRE-006 | Await Agent 6 disposition, scanner/signature gaps and execution isolation |
| PRE-007 | Preparation manifest available; run policy, access/retention/expiry/disposal pending |
| PRE-008 | Synthetic comparator is not real independent PDF oracle; admission pending |
| PRE-009 | Exact isolated environment and OS network proof pending |
| PRE-010 | Full instrumentation/sampling/repeats/bands/threshold decisions pending |
| PRE-011 | Local root and SHA-256 authorized; run manifest/access/retention and complete indexed binding pending |
| PRE-012 | NOT_AUTHORIZED for provider/document execution |

Older prose in P2-D01/P2-D02 says human approval absent while bounded Wave 0
records explicitly authorize preparation/acquisition. Treat newer bounded records
as authority for exactly those acts, not full P2-D approval. Agent 0 should reconcile
central wording without broadening scope. Same distinction applies to stale
charter bootstrap status. BASE-OI-020 remains unresolved by intake; BASE-OI-021
is not closed by preparation tests; BASE-OI-013 still blocks Worker claims.

Agent 7 did not author the intake/harness/corpus reviewed here. P2-W0-ROLE-001
records the user's temporary combined human roles and accepted independence
limitation. Preserve that limitation; do not require a new role approval already
given, claim organizational independent sign-off, or enter an AI as human approver.

## Controlled next decision

Recommended next step is a free metadata-only closure proposal before any new
acquisition: enumerate exact target and proposed dependency versions, official
sources, notices, asset provenance and security findings; then seek bounded
acquisition authority if required. Neither this report nor a version range
allows downloading transitives. Alternative: defer the candidate pending missing
evidence; do not pick another engine automatically.

Proposed wording for Agent 0 to complete and present (NOT a granted decision):
'I authorize acquisition-only of the exact components listed in [manifest digest],
for [TFM/RID/runtime], from [official sources], with ZERO_ADDITIONAL_COST. The
allowlist includes exact versions/digests and approved paths. Stop on mismatches,
new dependencies, paid terms, or new security boundaries. No candidate loading,
assembly execution, document processing, product reference, or merge is authorized.
Authority/effective date/expiry/evidence reference: [filled human record].'

Subsequent APPROVED_FOR_POC_ONLY and execution authorization must separately bind
candidate plus complete graph/SBOM, charter, environment, corpus, independent
oracle, config, measurement policy, security findings, evidence retention/access,
reviewers and exact experiments. Each later engine decision must be exactly one
of APPROVED_ENGINE, APPROVED_WITH_LIMITATION, BLOCKED_ALTERNATIVE_REQUIRED or
DEFERRED_BY_GOVERNED_DECISION, issued by human authority. This intake issues none.
Changes to any bound input, newly discovered license/security restrictions, or
expiry require controlled re-freeze; retain superseded records and failed evidence.

## Verification and handoff

Run from assigned worktree:
`pwsh -NoProfile -File poc/reviews/governance/Verify-Review.ps1 -PackagePath <approved-artifact>`
Exit 0: 111 archive hashes and 12 preparation-source hashes match; before/after
package SHA-256 identical. Output: verification.json. Script uses BCL ZIP/hash
APIs and reads bytes only. It does not extract files or load candidate assemblies.
No provider/build/document/performance tests run; performance evidence NOT_RUN.
No fresh signature, malware, or full vulnerability clearance claimed.

Files owned: this README.md, Verify-Review.ps1, verification.json. Git commit binds
the handoff; raw package remains unchanged and untracked. Changes are confined
to poc/reviews/governance. No product/package graph, central registry, system
security setting or main branch change. Review text contains public metadata,
synthetic IDs and digests only; no document content or credential included.

Trace: FEAT-PDF-005, FEAT-SEC-019, FEAT-SEC-020, TECH-DEC-005, TECH-DEC-006,
P2-D01, P2-D02, P2-D03, PRE-004, PRE-005, PRE-006, PRE-007, PRE-008, PRE-009,
PRE-010, PRE-011, PRE-012; PDF001-E00 admission preparation only.
Integration order: Agent 0 audits this review with Agent 6's separate review,
records unresolved evidence/authority gaps, then freezes the next bounded packet.
This review task is complete; Phase 2 exit and processing remain blocked.
