# Agent 2 / Wave0K observer closure

Baseline branch task/P2-A2-wave0i, clean before work, HEAD 672252324b7c2047b210d57b3738f6f3e0a156b4. Input observer from integration f94e93f79aace66749b85540b211086208b8594d. Scope POC-PDF-001 / FEAT-PDF-005 / P2-W0H-EXEC-001; private bounded evaluation, not engine admission or production redistribution.

Only new `poc/wave0k/closure/Inventory.ps1`, `inputs.lock.json`, `Test-Lock.ps1`, and this handoff. No commit, extraction, copy, PDF generation/parsing, Python execution, qpdf execution, installation or acquisition. Agent0 owns integration/staging/runtime and independent reviewer must accept exact closure first.

## Interface

Lock schema1: archive/archiveSha256; qpdf array of member,target,bytes,sha256; python array of source,target,bytes,sha256. Targets relative to proposed staging root. qpdf target directory contains exactly10 binaries, not notices; notices stored separately. Python entries contain 563 runtime/stdlib files and one PSF notice. Total Python 23,893,893 bytes; qpdf binaries/notices 9,111,646 bytes. Lock SHA256 `0A2F34BFA7BB805AE16F87C6686F1E9B842EB8C131E4CA68DF7C0E2718CDE7BB`.

`Inventory.ps1` default is read-only hashing; -WriteLock creates a NEW lock only and fails if it exists. No overwrite. Inputs must remain exact and be checked at root staging, including ancestor reparse checks and exact inventory. Runtime closure is a static candidate, not a proven portable Python distribution. Use full guest path python.exe with -I -S -B and clean environment; do not use host registry/user site, third-party packages or fallback host Python. Retain Lib layout so standard landmark/path discovery can work. Guest import/load failure means stop and diagnose; never automatically acquire a missing library.

Python3.14.0 FileVersion resource observed; python.exe Authenticode Valid on host. Selected runtime includes python.exe, python3.dll, python314.dll, two local VC runtime DLLs, _hashlib.pyd/libcrypto-3.dll, select.pyd and unicodedata.pyd. Stdlib .py includes encodings/json/argparse/re/subprocess/pathlib/hashlib and transitive Python source conservatively, excluding site-packages, caches, tests, test, idlelib, tkinter, turtledemo, ensurepip and venv. This is NOT a general all-stdlib native extension closure. Other .pyd libraries intentionally absent. No dynamic import tracing or guest execution has proved this subset yet.

qpdf archive rehashed against prior acquisition; selected members hashed by streaming ZIP entries without extraction. All bin DLLs plus qpdf.exe conservatively retained; fix-qdf.exe and zlib-flate.exe omitted. Prior static PE inventory shows normal imports resolved by qpdf30/MSVCP140/VCRUNTIME140/VCRUNTIME140_1 plus Windows OS API libraries. Extra VC DLLs retained as conservative supplied closure. Delay imports empty in prior inventory; dynamic LoadLibrary and embedded dependency identity remain limitations. Do not mix Python/qpdf VC DLL directories.

## Provenance, notices and limits

Existing qpdf acquisition.json records official GitHub release URL/hash and archive-static-inventory.json records319entries/12PE. qpdf license.rst.txt and license.html retained, but these are documentation notices, NOT a complete bundled dependency/SBOM/license proof. Full Apache license, embedded OpenSSL/zlib/JPEG identity and Microsoft redistribution rights remain governed gaps; no public distribution approval. PSF C:\Python314\LICENSE.txt (35,407bytes) is locked separately. Installed Python's original installer/acquisition chain has not been reconstructed; host Valid signature on launcher is not verification of every stdlib/native file. No online lookups or new cost.

Verification: Inventory.ps1 checks archive hash, selected10binary count, duplicate/excluded targets and source reparse paths; source count564. Test-Lock.ps1 checks schema/status, target/hash/size schema,6negative paths, uniqueness, everyPython hash/length, archive hash and12selectedmember hashes/lengths against independent prior intake. Tests read metadata/bytes only. Root still must prove exact guest loader and observer behavior. No fidelity/engine-disposition/pass claim.

Integration order: review lock/notice gaps -> stage exact entries with private ACL and read-only guest mapping -> verify staged inventory -> isolated import/observer smoke -> independent result audit. Avoid writable host document mapping. Root outer timeout and cleanup mandatory; no hostile or real document inputs.
