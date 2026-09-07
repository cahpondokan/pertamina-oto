# qpdf 12.4.1 static intake - Wave 0F

Task P2-W0F-A3-QPDF-STATIC-INTAKE; run P02-20260905-1434.
Baseline c297922b87df09c4f94deb66d23b3aec36c32798; branch task/P2-A3-corpus-oracle.
Input integration checkpoint: 7fbcada232c0009a27a226f8f1d9d80cbe3c57e6 (task-card reference).
Authority P2-W0-QPDF-ACQ-001: direct human approval received 2026-09-07 for zero-cost
download/static inspection only. This supersedes the acquisition blocker recorded
in BLOCKED_REPORT.md; historical report retained. Ordinary approval review allowed
this attempt. No install, extraction, binary/DLL execution, PDF processing,
restore/build, PATH/product change, extra acquisition, restart or Sandbox launch.

## Acquisition result

Exact source: https://github.com/qpdf/qpdf/releases/download/v12.4.1/qpdf-12.4.1-msvc64.zip
Request UTC: 2026-09-07T05:10:32.208289+00:00.
Completion UTC: 2026-09-07T05:10:59.710505+00:00.
One HTTP 302 redirect to HTTPS release-assets.githubusercontent.com, then HTTP 200.
Effective path: /github-production-release-asset/4539167/93e37410-6eb6-403a-9bb2-e284012cc081.
Signed query parameters were intentionally redacted and not persisted.
No other redirect host was permitted by the request handler.
Bytes: 28165367.
Expected and locally computed SHA-256:
3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6.
Result: HASH_MATCH. No fee or paid service was used.

Raw artifacts are exclusively under the approved evidence root, run
P02-20260905-1434/agent-3-qpdf-intake:
- qpdf-12.4.1-msvc64.zip
- acquisition.json (sanitized redirect/status/time/size/hash)
- archive-static-inventory.json (all entry hashes, PE imports, certificate-table
  hashes and category-only static string indicators)
No binary was copied into the worktree or extracted anywhere.

## Archive and activation inventory

319 ZIP entries were read as bytes with Python standard-library zipfile. Path
checks rejected traversal, absolute paths, backslashes, drive/ADS colons,
case-insensitive duplicates and symlinks; none occurred. Encrypted entries absent.
Reading all members verified decompression/CRC. These are inspection safety checks,
not approval to extract. The PDF manual was hashed only, never parsed/rendered.

12 PE x64 members: 3 executables and 9 DLLs. Executables are qpdf.exe, fix-qdf.exe
and zlib-flate.exe. DLLs are qpdf30.dll, concrt140.dll, msvcp140.dll, msvcp140_1.dll,
msvcp140_2.dll, msvcp140_atomic_wait.dll, msvcp140_codecvt_ids.dll,
vcruntime140.dll and vcruntime140_1.dll.
Two linker libraries, qpdf.lib and qpdf_static.lib, are also present. Documentation
contains JavaScript. 39 paths were flagged across executable/DLL/JS/linker-library
categories. They are inventoried surfaces, not approved tools. No scripts, HTML,
linker libraries or executable contents were activated.

qpdf.exe imports qpdf30.dll, MSVCP140.dll, VCRUNTIME140.dll, VCRUNTIME140_1.dll,
KERNEL32.dll and Windows CRT API sets. qpdf30.dll imports Windows/CRT APIs including
ADVAPI32, CRYPT32, USER32, WS2_32 and runtime DLLs. No delay-import descriptors were
observed in the 12 PE files. No dependent package was fetched.

Static ASCII indicators in qpdf30.dll include OpenSSL (102 matching strings), zlib
(13), libjpeg (2), and LoadLibrary (2). These are evidence of component/API strings,
not conclusive versions, linkage/SBOM, execution or network activity. WS2_32 is a
real import, but no egress claim follows from static inspection. Full import lists
are in raw inventory; runtime resolution, dynamically loaded modules and exact
statically linked dependency versions remain unresolved.

## Trust, license and SBOM limits - HOLD

qpdf.exe, fix-qdf.exe, zlib-flate.exe and qpdf30.dll have no embedded PE certificate
table. Eight Microsoft runtime DLLs have nonempty certificate tables, whose bytes
were hashed. No Authenticode chain, timestamp, catalog signature, detached release
signature or fresh revocation verification was performed. Certificate presence is
not signature validity. Matching the frozen release hash binds bytes but does not
supply independent signer/security approval.

The exact archive includes manual-html/_sources/license.rst.txt and license.html.
The text states qpdf uses Apache License 2.0 and links to the license; it does not
supply full bundled-component license/notice coverage. No standalone LICENSE,
COPYING or NOTICE-named file was found in the member inventory. No SBOM-named
artifact was identified. Embedded documentation/JS comments can contain additional
notices and require the independent governance audit; this report does not claim
no notice exists anywhere in the archive.

SBOM candidates: qpdf 12.4.1 (asset metadata, not executed version), Microsoft C++
runtime DLL family (exact versions/rights pending), possible embedded OpenSSL,
zlib and JPEG components (exact versions/licenses pending), documentation JavaScript
and font assets (distribution scope/notice obligations pending), and external
Windows runtime/API sets (target applicability pending). No inferred component
version or product redistribution permission is accepted as fact.

Disposition: ACQUIRED_HASH_MATCH / HELD_FOR_AUDIT. The unresolved component/notice
coverage and activation surfaces prevent further admission or execution. No paid
requirement was discovered, but zero-cost complete dependency/license acceptance
is not proven by the top-level Apache statement. Do not obtain missing binaries,
install runtimes or buy support to close gaps under this task.

## Verification and handoff

inspect_zip.py is local inspection code, not code from the archive. It only hashes,
reads ZIP members and parses PE metadata in memory. Its assertion binds the exact
archive hash. Raw inventory includes every member digest; no full static-string
dump, document content, signed download token or personal path is logged.

Trace: FEAT-PDF-005, PDF001-E00/PDF001-E06; UT-MERGE-033, UT-MERGE-034;
TP-MRG-028, TP-MRG-029. These product/oracle tests are NOT_RUN. No fidelity,
performance, offline processing, security clearance or engine approval is claimed.

Required next owners: Agent 6 rehashes and reviews PE/import/trust/security findings;
Agent 7 audits exact license/notice/static-dependency/redistribution obligations.
Human Technical/License/Security authority must accept exact component closure,
resource limits, isolated environment, evidence retention and execution freeze.
Acquisition success alone does not authorize qpdf -v or any PDF command.

Reproducibility: pin ZIP plus member hashes; stop if bytes drift. Removal only means
disabling an eventual PoC profile; cleanup of raw evidence requires its retention
policy and exact owned paths. No cleanup was performed. Agent 0 integrates only this
Wave 0F tracked delta after audit; main and central coordination files unchanged.
