# Wave 0F independent governance handoff

Agent / Task ID: Agent 7 / P2-W0F-A7-QPDF-GOVERNANCE-REVIEW
Run: P02-20260905-1434
Baseline: 2842ee03e5942d979b054341d74391eb07b59e71 (clean)
Branch: task/P2-A7-wave0f-review
Worktree: worktrees/phase2-20260905/agent-7-wave0f-review
Scope: poc/reviews/governance/wave0f/** only.

## Result

Static governance review COMPLETE; candidate disposition HOLD_FOR_COMPONENT_NOTICE_CLOSURE. This is not APPROVED_ENGINE, permission to execute, or a Phase 2 exit. Independent byte binding PASS; complete bundled license/provenance/redistribution acceptance remains BLOCKED. No new download, extraction, installation, candidate load/execution, document processing, build, restart, Sandbox launch or paid service occurred.

Objective: independently verify acquired qpdf and identify every inventory family's license/notice/provenance and cost obligations without activation. Trace: POC-PDF-001, FEAT-PDF-005, PDF001-E00, PDF001-E06, UT-MERGE-033, UT-MERGE-034, TP-MRG-028, TP-MRG-029; decisions P2-W0-QPDF-ACQ-001, P2-D01, P2-D03, P2-W0-RESTART-001. Product tests are NOT_RUN, not passed by this review.

## Independent verification

Exact asset qpdf-12.4.1-msvc64.zip, 28165367 bytes, SHA-256 `3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6`.

`verify.ps1` uses .NET ZIP streams and SHA-256 independently of Agent 3's Python inspector. Command from this worktree:

```powershell
& './poc/reviews/governance/wave0f/verify.ps1' -EvidenceRoot 'D:\OTO\pertamina-oto\artifacts\evidence\P02-20260905-1434\agent-3-qpdf-intake'
```

Exit 0: all three raw file hashes/sizes match intake-manifest.json; all 319 member hashes/sizes match archive-static-inventory.json; paths unique case-insensitively and free of checked traversal/absolute/ADS/backslash forms; all raw hashes unchanged after review. 186 text entries scanned for license/component markers. `verification.json` records each entry's digest, classification, and marker inventory. Classifications are triage, not an SBOM or license grant. Script does not independently validate CRC/encryption/symlink flags; those checks remain Agent 3 evidence. PDF manual only hashed. No PDF parser, browser rendering, JS activation or PE loader used.

## License and provenance matrix

Paths below are relative to qpdf-12.4.1-msvc64/. Both manual-html and manual-single-page-html trees are covered by the per-entry inventory.

| Family / evidence | Finding and required closure |
|---|---|
| qpdf bin/qpdf.exe, fix-qdf.exe, zlib-flate.exe, qpdf30.dll; include/qpdf; lib; examples; manuals | license.rst.txt declares Apache-2.0. Buffer.hh retains Jay Berkenbilt / Manfred Holger notices and an optional historic Artistic licensing statement; this review evaluates Apache only. libqpdf.pc confirms metadata version 12.4.1, not executed version. Preserve notices; bind exact release sources and full license to distributed files. Apache does not license unrelated bundled components. |
| Microsoft runtime: concrt140.dll, msvcp140.dll, msvcp140_1.dll, msvcp140_2.dll, msvcp140_atomic_wait.dll, msvcp140_codecvt_ids.dll, vcruntime140.dll, vcruntime140_1.dll | Eight separate binaries. Exact build versions, applicable Microsoft license/REDIST list, original distributor entitlement and our intended redistribution rights are unresolved. Certificate tables reported by intake are not proof of valid signatures or license entitlement. Distinguish internal use from shipping the DLLs with a product. Do not infer a required purchase or blanket free redistribution. |
| Embedded OpenSSL candidate | Intake reports strings in qpdf30.dll, not exact version/linkage. Need release-bound build provenance and applicable license/NOTICE. OpenSSL licensing differs across versions; do not assign modern Apache terms to unidentified embedded bytes. |
| zlib candidate | libqpdf.pc explicitly lists Requires.private: zlib; intake also has strings. Need exact version/source linkage and license evidence for binaries/static library. Preserve origin and notices and mark modifications as required by applicable zlib terms. |
| JPEG candidate | libqpdf.pc lists libjpeg; intake has libjpeg strings. Exact implementation (IJG, libjpeg-turbo or other), version and compiled features unresolved. libjpeg-turbo terms are only a conditional reference, not identification of these bytes. Its multiple applicable license families cannot be collapsed to qpdf Apache. |
| jQuery | jquery.js identifies v3.6.0, OpenJS contributors and license URL. MIT upstream declaration corroborates family; retain copyright and applicable permission text, including embedded subcomponents. Header alone does not establish full bundle closure. |
| Sphinx JS/CSS | _sphinx_javascript_frameworks_compat.js declares two-clause BSD and Sphinx contributors. doctools/searchtools/sphinx_highlight/basic.css and generated search/language/config assets require exact generator/source and applicable notices. A mention of underscores.js in a comment is not proof of a separate bundled package. |
| Snowball | english-stemmer.js identifies generation by Snowball 3.0.1; base-stemmer.js also present. Upstream BSD notice reference requires exact generator/runtime attribution coverage. Generated version comment alone does not identify all supporting source revisions. |
| Theme/RTD assets | theme.js, badge_only.js/css, versions.js, theme.css plus generated Pygments/wraptable assets: map exact theme/generator versions and bundled subcomponents to licenses and preserve notices. Do not label the complete theme MIT solely from the embedded Font Awesome CSS notice. |
| Font Awesome | theme.css identifies 4.7.0 with font SIL OFL 1.1 / CSS MIT. Font files present. Preserve applicable OFL/license/copyright and reserved-name constraints if modified; font binary provenance still needs mapping to exact source. |
| Lato / Roboto Slab | Multiple ttf/woff/woff2 variants across css/fonts and fonts trees, including RobotoSlab v7 filenames. Exact upstream revisions, full font licenses, copyright/reserved names and conversion provenance absent from accepted evidence. Do not infer license from family name. |
| Remaining documentation/images/metadata/directories | Every path hash is listed. Directories have no independent content; remaining files are not automatically exempt. HTML/source examples inherit only established component rights; PDF manual and binary image/font internal notices were not parsed. Need asset/source/notice mapping or a human-approved future minimal distribution manifest; no files removed now. |

No standalone LICENSE/COPYING/NOTICE-named artifact or SBOM was identified in intake. This does not mean no notices exist: source headers, JS and CSS carry real notices. Marker scanning cannot prove every obligation was found. Complete coverage remains expressly unresolved for embedded binaries, fonts, minified bundles and PDF contents. No claim that the supplier is noncompliant follows from this evidence gap.

## Obligations and authoritative references

References consulted 2026-09-07 are contextual guidance, not release-bound evidence or a replacement for exact supplied terms.

- [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0): no-charge copyright/patent grants subject to terms; redistribution needs a license copy, change notices for modified files, retained applicable attribution and NOTICE content when applicable. No blanket trademark or warranty grant. An external link alone is not the planned complete redistribution package.
- [Microsoft redistribution guidance](https://learn.microsoft.com/en-us/cpp/windows/redistributing-visual-cpp-files?view=msvc-170): redistribution depends on applicable Visual Studio license terms and REDIST eligibility. Obtain matching rights evidence and servicing ownership; do not treat runtime presence as entitlement or force a paid license as the assumed remedy.
- [OpenSSL license](https://openssl-library.org/source/license/index.html): determine actual embedded version before choosing its terms.
- [zlib license](https://zlib.net/zlib_license.html): origin/alteration/notice requirements need mapping to exact bundled code.
- [libjpeg-turbo license](https://github.com/libjpeg-turbo/libjpeg-turbo/blob/main/LICENSE.md): conditional implementation reference only; pin applicable revision if identity is established.
- [jQuery license](https://jquery.com/license/), [Snowball license](https://snowballstem.org/license.html), [Font Awesome 4 license](https://fontawesome.com/v4/license/): corroborate observed family declarations, not unverified full-asset coverage.

## Blockers, owner and concrete next decision

| Blocker | Owner | Evidence to close |
|---|---|---|
| GOV-F01 exact component provenance | Technical Owner / supplier provenance reviewer | Release-bound component/version/build mapping for all binaries/static libraries and docs assets; source and checksum references; explain embedded linkage. |
| GOV-F02 complete notice/redistribution coverage | Human License authority | Exact applicable license texts and attribution matrix, Microsoft rights/REDIST applicability, font and docs notices; define internal PoC versus redistribution scope. |
| GOV-F03 trust/security admission | Security authority with Agent 6 | Accepted signature/provenance/advisory disposition, environment and runtime limits. Hash equality alone does not close this. |
| GOV-F04 scope/cost/support acceptance | Human Technical/License/Cost authority | Confirm zero additional cost, no paid support/license/service, accepted support/maintenance responsibility and evidence retention. No budget or purchase approved. |

Recommended human wording for the next packet (PROPOSED, not approved): "Saya menerima hasil verifikasi statis ZIP qpdf 12.4.1 dengan SHA-256 3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6 sebagai evidence saja. Tutup GOV-F01 sampai GOV-F04 dengan bukti versi, lisensi/notice, hak runtime, keamanan, dan biaya nol sebelum meminta izin eksekusi terpisah. Tidak ada izin install, load/execute, pemrosesan PDF, restart, Sandbox launch, distribusi, atau pembelian dari penerimaan laporan ini."

No repeated download approval is needed: P2-W0-QPDF-ACQ-001 already approved and acquisition completed. Historical rejection is superseded. Agent 0 should reconcile stale P2-D01/P2-D03 wording (qpdf not acquired) with current acquisition evidence while retaining license/execution holds. Agent 7 has not edited central records. Restart remains a separate proposed decision and does not close license gaps.

## Handoff details

Files changed: README.md (this disposition/blocker report), verify.ps1 (reviewer-authored inspection), verification.json (sanitized per-entry evidence), all under the allowed path.
Behavior implemented: no product behavior; reproducible static evidence audit only.
Tests and exact results: verifier command above exit 0; 3/3 raw hashes and 319/319 entry hashes/sizes PASS, 186 text entries inventoried. Build/unit/integration/performance/offline processing tests NOT_RUN because activation prohibited.
Security/privacy: no raw binary copied, no extraction, no signed URL query, secrets or document content saved; generated evidence contains public archive paths/hashes and marker names. Raw source evidence unchanged. Host certificate/trust validation not performed.
Performance/resource evidence: not measured; no PoC resource or fidelity claim.
Known limitations: missing complete component provenance/notice matrix and runtime rights; static scan is not legal or security sign-off. No engine selection.
Evidence paths: this directory plus immutable input poc/oracles/qpdf-12.4.1 and run-owned agent-3-qpdf-intake (read-only).
Recommended integration order: Agent 0 audits this scoped commit after integrated Agent 3 intake, combines with independent Agent 6 review, reconciles decisions and issues one human packet. No specialist merge or phase transition performed.
