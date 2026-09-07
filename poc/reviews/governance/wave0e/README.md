# Agent 7 Wave 0E-B governance review

Run P02-20260905-1434; task P2-W0E-B-A7-GOVERNANCE-REVIEW; 2026-09-07.
Input and review baseline dd003ea20ec8e00b96c265907560f66957783a5d.
Branch task/P2-A7-wave0e-review; assigned agent-7-wave0e-review worktree.
Recommendation: STATIC_GOVERNANCE_READY_WITH_LIMITATIONS for three NuGet intakes;
HOLD_EXECUTION. qpdf remains BLOCKED_BY_APPROVAL_REVIEW.
Engineering governance only, not legal advice, vendor approval or human sign-off.
ZERO_ADDITIONAL_COST; no paid commitment, acquisition, restore/build, candidate
load/execution, PDF processing, Sandbox launch, restart or product change.

## Independent verification and exact component disposition

| Component | Bytes / archive entries | SHA-256 | Disposition |
|---|---|---|---|
| Microsoft.Extensions.Logging.Abstractions 8.0.3 | 802188 / 67 | E4C498D5A13051B4577A148F1D8C3470167215C507E2392069B75DC61322BB74 | Static license/byte intake ready with limitations; hold build/generator and runtime use |
| System.Security.Cryptography.Pkcs 8.0.1 | 1078389 / 31 | 28C348909DF243FE4EE962213E3C8021AAECBC8321929DBA03A69BCB92A49DE5 | Static license/byte intake ready with limitations; hold runtime/interop use |
| Microsoft.Extensions.DependencyInjection.Abstractions 8.0.2 | 278862 / 26 | 51F2DF1100245F10DA54F0BB7E813F277155117777D4FBBAB902214E27372606 | Static license/byte intake ready with limitations; hold build/runtime use |

Verify-Binding.ps1 independently read all original nupkg bytes from the approved
agent-2-dependency-intake evidence root. All three local SHA-256 values, sizes,
SHA-512 values against frozen catalog records, and all 124 entry hashes/lengths
match. Six legal text entries match tracked JSON exactly. Nuspec copies match
with CRLF/LF and terminal newline normalization; original nuspec entry hashes
remain verified byte-for-byte. No archives were extracted, no assembly reflected
or loaded. Before/after package SHA-256 unchanged. verification.json records results.

Each exact nuspec declares MIT, Microsoft copyright and dotnet/runtime repository.
Logging source commit: eba546b0f0d448e0176a2222548fd7a2fbf464c0.
Pkcs and DI source commit: 81cabf2857a01351e5ab578947c7403a5b128ad1.
These are declared source identities, not independently reproduced binary builds.
Receipts record exact official NuGet URLs, acquisition UTC, HTTP 200 and zero
redirects; Agent 7 did not witness acquisition or re-request any URL. Hash agreement
binds this review to acquired bytes and frozen service metadata, not full supply-chain
trust. The tracked signature logs show Microsoft author primary signatures plus
NuGet.org repository countersignatures, successful verification, and unknown
revocation status. Agent 7 reviewed those logs, not reran signature verification.
Fresh revocation, malware/vulnerability and source-build reproducibility remain gaps.

## License and notice review

All three original archives contain LICENSE.TXT and THIRD-PARTY-NOTICES.TXT.
The bundled MIT text names .NET Foundation and Contributors; preserve that holder
notice as well as applicable Microsoft/upstream notices. Its no-charge permission
supports no-license-fee evaluation subject to preservation of copyright/permission
notices and warranty disclaimers. It is not permission to strip attribution, a
support agreement, a product distribution decision or a waiver of third-party terms.

Full third-party notice collections are preserved in the tracked legal JSON.
They include MIT/BSD-style notices, Apache references (including LLVM exceptions),
Unicode, zlib, W3C, public-domain/CC0 notices, and other origin-specific notices.
The broad .NET collection is not proof every described component is in these
selected deployment assets. Conversely the top-level MIT label does not replace
those notices. Recommended safe default for a later authorized package: retain the
complete supplied LICENSE and THIRD-PARTY-NOTICES per component/version, plus a
component-to-notice index; do not prune notices without asset/source applicability
review. Preserve attribution/disclaimers; applicable BSD-style binary notices belong
in documentation/materials, non-endorsement terms remain, and relevant modification
or origin notices must not be removed. Referenced license texts or exceptions needing
full compliance assessment must be bound before distribution. No new license text
or source archive was acquired in this review.

This is not a claim all third-party clauses apply to every binary, nor an exhaustive
legal compatibility opinion. Remaining mapping is exact selected runtime/build/
generator/resource asset -> upstream component -> applicable terms -> notice delivery.
The notice mention of historical ImageSharp code is not evidence of a dependency
on a current ImageSharp commercial package or authorization to acquire one.

No per-seat/runtime royalty or mandatory paid support is established by the inspected
MIT permission. No added cost incurred. Any paid service, subscription, scan tool,
commercial support, cloud charge or license is outside authority and must stop before
commitment. Existing SDK/Windows availability does not imply universal deployment
rights or a free SLA. No support lifecycle/current-version claim is made from old pins.
This review relies on exact local supplied legal texts and existing metadata, not
unverified new vendor/pricing claims.

## Runtime, redistribution, graph and evidence gates

Selected metadata path remains PDFsharp net10.0 -> Logging 8.0.3 and Pkcs 8.0.1;
Logging net8.0 -> DI 8.0.2. Pkcs and DI selected net8.0 groups declare no further
packages. Nonselected TFM groups do not authorize additional acquisition.
Logging includes Roslyn generators/resources and buildTransitive targets; a future
build may activate code before any PDF run. Pkcs includes Windows runtime assets;
managed filenames do not exclude native OS cryptography/interoperation. DI includes
buildTransitive targets. No restore, asset selection, framework pruning or generator
activation was tested. Exact TFM/RID/SDK/runtime, build assets, OS imports, sources,
security disposition and complete SBOM remain required. Do not treat archive inventory
or metadata closure as a resolved deployment SBOM or offline proof.

P2-W0-DEP-002 acquisition authorization is already granted and fulfilled for these
three bytes; no repeat acquisition approval is requested. P2-D01/P2-D03 and PRE-004/
PRE-005 improve from metadata-only to hash-bound static evidence, but full admission
and deployment acceptance remain pending. PRE-006 awaits Agent 6 security disposition;
PRE-008 independent oracle remains missing; PRE-009/PRE-010/PRE-011 require verified
isolation, budgets and evidence policy; PRE-012 stays NOT_AUTHORIZED. Agent 0 should
update stale central prose saying packages are unacquired, without broadening approval.

Evidence root/SHA-256 policy is accepted; run retention/access/expiry, final SBOM
format, indexed signature-review decision and complete execution freeze remain open.
Use exact component/artifact, input commit, source/corpus/oracle/environment/config,
run, reviewer/time, sanitized result and digest binding. Hash changed artifacts and
review again. Freeze expiry explicitly; no invented perpetual authorization.
Preserve errors and superseded evidence. Agent 7 is independent of acquisition authors;
prior accepted temporary human role combination/independence limitation persists.

## qpdf blocker and exact next human decision

Reviewed poc/oracles/qpdf-12.4.1/BLOCKED_REPORT.md at this checkpoint. It records
that automatic approval review rejected the acquisition before process creation:
new binary dependency before required admission, and no trusted exact user authority
visible to that review. Local P2-W0-QPDF-ACQ-001 records approval, but that does not
permit bypassing the rejection. No retry/download was attempted by Agent 7.
Expected remote SHA-256 is not local verification; qpdf archive license/notices,
DLL/runtime closure, signature and SBOM are NOT_INSPECTED. This is an authority/
acquisition blocker, not evidence that qpdf is paid, incompatible or malicious.
Prior Apache metadata cannot clear the unacquired ZIP and bundled components.
MuPDF/Poppler are not substitutes authorized by this review.

Agent 0 should present this exact statement through the trusted user approval channel:
'I authorize acquisition-only and static inspection of qpdf-12.4.1-msvc64.zip from
https://github.com/qpdf/qpdf/releases/download/v12.4.1/qpdf-12.4.1-msvc64.zip,
expected SHA-256 3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6,
into the approved run evidence root / agent-3-qpdf-intake. ZERO_ADDITIONAL_COST applies.
Use the ordinary approval mechanism; do not bypass review. No qpdf execution, PDF
processing, install/PATH change, additional component, paid service, product reference,
restore/build, restart or Sandbox launch is authorized. Stop on source/hash/redirect/
dependency/cost mismatch. Record my identity/temporary role, date, expiry and approval
reference with this decision.'

For the acquired NuGet artifacts, proposed human record:
'I accept the static MIT/no-license-fee findings for the three exact identities and
SHA-256 values in this review, preserving supplied license/third-party notices and
all stated trust/runtime limitations. This permits preparation of the missing
security/asset/SBOM and execution-freeze packet only. It grants no restore/build,
assembly/generator execution, PDF processing, product redistribution, paid commitment,
restart or Sandbox launch. Record scope, reviewer/authority, date/expiry and reference.'

A separate concrete execution decision is premature until security, oracle, guest,
corpus, ceilings and evidence-export/retention fields are complete. No governed engine
status or Phase 2 completion is issued. Alternative human disposition: defer qpdf
acquisition and record owner/reopen condition while preserving the independent-oracle
gate; no silent scope reduction or fallback acquisition.

## Verification and handoff

Command from this worktree:
`pwsh -NoProfile -File poc/reviews/governance/wave0e/Verify-Binding.ps1 -IntegrationRoot ../agent-0-integration -ArtifactRoot <approved-agent-2-dependency-intake>`
Final exit 0, 3 package hashes/SHA-512 checks, 124 entry hashes and 6 legal-text
comparisons PASS. Initial checker exit 1 was due to CRLF/LF nuspec serialization;
normalizing only line endings resolved it, with original archive hash checks retained.
No input was edited. An exploratory regex command also failed; it mutated no file.

The first checker draft was prepared in the previously assigned Agent 7 worktree
before the new Wave 0E assignment arrived; only those two uncommitted task-owned
files were copied into this assigned worktree and removed from the old location.
No old branch commit or integration source was altered. Commit only this wave0e
path. Product tests/performance/resource/PoC suites NOT_RUN, intentionally outside
static review. Only public package metadata/legal text references and hashes stored;
no document data, credentials or raw sensitive paths in output evidence.

Trace: FEAT-PDF-005, FEAT-SEC-019, FEAT-SEC-020, TECH-DEC-005, TECH-DEC-006,
P2-W0-DEP-002, P2-W0-QPDF-ACQ-001, P2-D01, P2-D03, PRE-004, PRE-005, PRE-006,
PRE-008, PRE-009, PRE-010, PRE-011, PRE-012. No canonical test PASS claimed.
Integration: Agent 0 audit this commit with Agent 6 review, retain qpdf blocker,
reconcile central evidence status, then obtain the bounded decision through the
ordinary approval route. No main merge by Agent 7.
