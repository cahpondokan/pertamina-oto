# Local Poppler inventory - Wave 0D

Task P2-W0D-A3-LOCAL-POPPLER-INVENTORY; checked 2026-09-06.
Input integration checkpoint: 07b5621c8d5e26793e4d8658aef9f8e23b3a86ec (Git object verified).
Lane baseline: 7cadb0e0b6643f9773e1f396e796909d913360c8; task/P2-A3-corpus-oracle.
Only this new local-poppler directory changes. No main merge or product reference.
ZERO_ADDITIONAL_COST. No acquisition, binary copying, PDF processing, Sandbox launch,
restart or system/security mutation. Only pdfinfo.exe -v and pdftoppm.exe -v ran.

## Local evidence

The root is the exact user-named Codex runtime Poppler directory in the task prompt.
For privacy, inventory paths are relative to that root; rootSuffix plus USERPROFILE
resolves its exact location. No user document path is present. Executables:
Library/bin/pdfinfo.exe and Library/bin/pdftoppm.exe. CMD wrappers under bin were
read, not run. Version commands used the executable paths directly.

manifest.json in the runtime declares version 26.07.0, win32/x64,
packageSpec poppler=26.07.0=h6618ce5_3. Both executable version commands returned
26.07.0 with exit 0. Version resources on both executables and poppler.dll are null;
do not substitute guessed DLL package versions. PE machine 0x8664 is x64.

inventory.json records 445 files with exact relative path, size and SHA-256,
including all executables, DLLs, configuration and encoding data. 123 PE files
(2 executables and 121 DLLs) have local Authenticode status Valid at collection.
This is Windows verification at this instant, not independent upstream provenance,
a vulnerability scan, fresh revocation proof or permanent trust approval.
The two executables and poppler.dll signer thumbprint is
6EBE77CE17C9B78603A4932CA0E6068DD58DABBB, subject OpenAI OpCo, LLC;
issuer Microsoft ID Verified CS EOC CA 04. Observed certificate validity is
2026-09-04T08:04:22+07:00 through 2026-09-07T08:04:22+07:00. Timestamp-chain and
post-expiry validation were not audited; do not equate that date with binary expiry.

| File | SHA-256 |
|---|---|
| Library/bin/pdfinfo.exe | 4065076cc881d3bfa288a1c93b5ca7ebed248d6a380b9abce8b0d4a80e9524cd |
| Library/bin/pdftoppm.exe | 3f2440140fef98ddd1ef0772a10d8e58dc3564a9b15a14b4611b5bbd5bdcd37f |
| Library/bin/poppler.dll | a89ba7bb062e2463c1ab61704337d6e0612398510be08736d2862cf71900d3dd |

Inspect-PE.py reads PE bytes with Python standard library; it does not load a DLL.
pe-imports.json records normal and delay imports for every bundled PE.
static-closure.json records recursively reachable bundled names and unresolved
external names from both executable roots. This is not a runtime loader trace:
Windows API sets, DLL search order, dynamic LoadLibrary and environment/config
loads remain unproven. No system DLLs were copied or loaded by the parser.

Both executables import poppler.dll and Microsoft runtime components; pdftoppm
also directly imports lcms2.dll. poppler.dll imports freetype.dll, jpeg8.dll,
lcms2.dll, libcurl.dll, libpng16.dll, openjp2.dll, tiff.dll and zlib.dll plus
Windows/runtime components. libcurl is therefore a real static dependency edge,
not merely an unrelated file in the bundle. This does not prove network activity,
but makes effective network denial and argument restrictions mandatory later.
The full bundle also contains crypto, TLS, SSH, Kerberos, font/config and graphics
libraries; their exact imported closure and hashes are recorded, not pre-approved.

## License, provenance and lifecycle gaps

No LICENSE/COPYING/NOTICE-named file was found in the 445-file tree. The only
README-named file found describes fontconfig configuration; it is not a package
license and contains a historical build path which was not copied into evidence.
Licensing terms may appear within data-file comments; this inventory is not a full
legal audit and does not claim that no license text exists anywhere in the data.
No conda package receipts, complete package-to-file mapping or reproducible build
attestation are established by the small runtime manifest.

Primary metadata checked 2026-09-06:
- [Poppler upstream](https://poppler.freedesktop.org/) describes an xpdf-derived
  rendering library and lists 26.09.0 released 2026-09-03, including malformed-file
  crash fixes. Local 26.07.0 is older; applicability of fixes/CVEs is unresolved.
- [Conda-forge recipe](https://github.com/conda-forge/poppler-feedstock/blob/main/recipe/meta.yaml)
  declares GPL-2.0-or-later and COPYING, plus a multi-library build. This mutable
  recipe corroborates the likely packaging family, not the exact provenance of
  signed runtime files or their complete license obligations.

Required: exact source/package URL and digest, recipe/patch commit, build receipt,
signer acceptance, component versions/licenses/notices and source-offer obligations
where applicable, encoding-data/font licenses, Microsoft runtime rights, complete
SBOM, and dated security/advisory disposition. Existing installation is not a grant
of product redistribution rights. No paid license/support route is proposed.

Inference: Poppler is independently implemented from PDFsharp, but pdfinfo and
pdftoppm share poppler.dll and are not two independent parsers. A net10.0 harness
could supervise native CLI processes without a managed package, but that boundary
and offline processing remain untested. pdfinfo metadata alone does not establish
page-content/order/occurrence fidelity; rendering plus marker/structure observations
and a separately admitted structural checker may still be required.

Reproducibility risk: Codex owns this cache and may update/remove it. Freeze all
relevant binary/data/config hashes and recheck before each future run; stop on drift.
Do not delete or modify shared runtime files. Removal from the PoC means disabling
its profile/reference and cleaning only authorized run artifacts. If the cache goes
away, return unavailable; acquisition of replacement bytes requires new authority.

## Recommendation and decisions

RECOMMENDATION: retain as a possible internal-only oracle candidate for admission
review, but BLOCKED_FOR_DOCUMENT_EXECUTION now. Versions and signatures are positive
inventory evidence, not enough to issue APPROVED_FOR_POC_ONLY.
Technical/License/Security human authorities must resolve provenance/SBOM/license,
security freshness, runtime ownership and retention, argument/DLL-loading controls,
independent oracle acceptance and exact immutable file set. Then separately approve
bounded synthetic execution in the verified isolated guest. No URL inputs or server
fallback; no raw parser/text output in ordinary logs. Separate user restart approval
and post-restart environment verification remain mandatory. Cost conflicts stop as
BLOCKED_BY_COST; no such cost was incurred by this inventory.

Trace: FEAT-PDF-005; PDF001-E00, PDF001-E03, PDF001-E06;
UT-MERGE-033, UT-MERGE-034, UT-MERGE-046; TP-MRG-028, TP-MRG-029, TP-MRG-033.
No test PASS for those PDF suites is claimed. Performance/fidelity/egress tests NOT_RUN.

## Reproduction and verification

Collect-Inventory.ps1 -CandidateRoot <the user-named root> records SHA-256 and
Authenticode metadata only. Inspect-PE.py <same root> records static imports only.
Neither script invokes candidate executables. version-results.json records the
separately authorized direct executable -v calls. The scripts write only beside
themselves and must be rerun only under matching task authorization.

Agent 0 should audit this inventory and send it to Agent 6/7; cherry-pick only the
new Wave 0D commit after review. All inventory data are observations, not authority.
