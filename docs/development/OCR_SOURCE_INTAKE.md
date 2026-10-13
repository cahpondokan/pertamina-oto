# OCR source intake — 1 October 2026

Status: **STATIC_INTAKE_COMPLETE_SECURITY_DISPOSITION_REQUIRED**. Direct user consent relayed by parent: "saya izinkan. resikonya apa?", for exact source acquisition, safe extraction and static review only. No configure, build, engine execution, installation, product reference, update or redistribution occurred. The prior Windows installer signature finding is unchanged. One agent; no subagents; no central phase records edited.

Baseline branch `task/core-functional-first`, HEAD `84cbb0f538020003bcfb6583e94c2fcf6524f87c`; dirty work preserved. Scope `FR-SCAN-002`, `FEAT-OCR-004`, `POC-OCR-001`, `UAT-CORE-009`. Files authored: this review, `eng/OcrSourceIntake.py`, `eng/TestOcrSourceIntake.py`, checkpoint link in SHARED_LOCAL_OCR.md. Product source was not changed.

## Acquisition and provenance

`python -B eng/OcrSourceIntake.py` exit0. Archives obtained from exact immutable official GitHub commit URLs, HTTPS redirects limited to GitHub/codeload/API. Source-tree metadata came from the official GitHub API. Archive SHA-256 binds acquired bytes; Git blob SHA-1 matches verify every archived file against the requested source tree, not a local GPG signature. GitHub's tag verification is separate. No extra model/tool/dependency was acquired.

| Candidate | Commit | Archive bytes / SHA-256 | Files / expanded bytes |
|---|---|---|---|
| Tesseract5.5.3 | db0ec62f81b0737fbbe184d8fea40af5738f8eef | 1918077 / 21dbaff9867d937c156d0b898e90b726aa339129cfebd7e1365cd5fee5046d67 | 724 / 8185994 |
| Leptonica1.87.0 | 13275a278eb55b5746e33f95fbf5a2c8f604b3ab | 13725547 / e72242133adf678e8584081a506ca3a665c8f6f6de8c3fe7b47d1d5fcf9a4ce4 | 988 / 24762088 |

All **1712 files matched**, zero missing blobs, zero mismatches. Tesseract772 and Leptonica1001 archive entries (including directories) were validated before any payload extraction. Limits: 64MiB download, 32MiB/file, 512MiB expanded/archive, 10000 entries. Reject traversal/absolute/Windows reserved/ADS/control/case-collision paths, links/special/sparse entries and unexpected prefix. Only validated regular bytes are written using create-new; archive execute bits/ownership are not applied.

Evidence: `artifacts/ocr-source-intake-20261001/manifest.json` SHA256 **D17A05506582203845E08644C75C207A6E2CD418EA95B156F63D4700620C338A**, per-file SHA256/gitBlob bindings and official tree metadata. Extracted trees are kept in the same intake root; do not import/run them. `python -B eng/TestOcrSourceIntake.py` exit0, **16 guard checks PASS**, including traversal, links, collisions, file size and zero payload write before full validation. Evidence `artifacts/ocr-intake-guard-tests-34f0818d70144748ad6d300465016a1e/results.json`. These are archive safety tests, not OCR tests.

## License and component review

Acquired Tesseract LICENSE is Apache-2.0. Leptonica root/src/prog notices are BSD-style two-condition source/binary redistribution permissions. Preserve copyright, license conditions and disclaimers; Tesseract/model Apache notices and change descriptions must accompany any later approved redistribution. No license fee identified. Target source/header search for GPL/LGPL/GNU General/Lesser declarations returned no hits in the inspected source/text file scope; this is a basic inventory, not legal sign-off or proof of every artifact's rights. Test images/data remain test assets and must not be packaged into the product by default.

Tesseract `.gitmodules` declares googletest and test-corpus submodules; these are gitlinks, not archived file blobs. They were **not acquired** and are outside the proposed runtime build with BUILD_TESTS=OFF. No exe/DLL payloads found in the inspected source archives. Source build optional codecs and training tools must stay disabled to avoid unreviewed transitive libraries. Compiler/static CRT or dynamic runtime rights/import inventory and generated binary/license manifest still need review before product admission. Existing held `eng`/`ind` models remain unchanged and runtime quality/support is not yet tested.

## Build-hook findings

- Tesseract top-level CMake conditionally runs `git describe` if `.git` exists; acquired archive has no `.git`. Use exact archive source and independent build directory.
- **Configure itself executes code:** `cmake/CheckFunctions.cmake:34` contains a `try_run` TIFF probe linked to Leptonica. A future configure/build authorization must explicitly cover this tiny generated probe as well as compiler/linker execution; do not call configure under static-only authority.
- Training CMake contains a network ICU download and extraction under a training branch. BUILD_TRAINING_TOOLS=OFF is mandatory; no ICU/pkg-config or training corpus acquisition is approved.
- Leptonica defaults SW_BUILD=ON on Windows; explicitly OFF is mandatory to avoid Software Network dependency acquisition. Optional zlib/PNG/JPEG/TIFF/GIF/WebP/OpenJPEG flags must all be OFF in the proposed minimal BMP/PNM build.
- Both projects provide uninstall targets. Do not run uninstall/install-to-system targets; any later CMake staging must use an exact task-owned workspace prefix and no system mutation.
- Leptonica source has `callSystemDebug` and related external program/debug operations. `LeptDebugOK` defaults0, but future engine wrapper must keep it disabled; CLI/debug settings need tracing before runtime admission. Tesseract network graphics disabled and curl/archive OFF are mandatory. Source flags are not OS network isolation or proof of zero runtime egress.

## New security finding OCR-SRC-SEC-001

Official Tesseract advisories inspected on1October2026 identify **eight advisories whose affected range includes5.5.3**. These chiefly involve malicious `.traineddata` parsing/loading or model-driven dimensions; they do not establish that ordinary PDF input alone triggers each issue. Six are High, two Moderate according to the advisory headers. Known-model hash pinning and disabling legacy code reduce exposure but are not upstream fixes or immunity. No hostile payload was generated or executed.

| Advisory | Affected / patched metadata | Boundary |
|---|---|---|
| [GHSA-5j2p-r5vc-q7f3](https://github.com/tesseract-ocr/tesseract/security/advisories/GHSA-5j2p-r5vc-q7f3) | <5.5.4 /5.5.4 | Legacy normproto stack overflow |
| [GHSA-f6h7-cqr4-6fx4](https://github.com/tesseract-ocr/tesseract/security/advisories/GHSA-f6h7-cqr4-6fx4) | <=5.5.3 /None | Empty model stack, initialization DoS |
| [GHSA-q44c-23p6-5mw6](https://github.com/tesseract-ocr/tesseract/security/advisories/GHSA-q44c-23p6-5mw6) | <=5.5.3 /None | FullyConnected dimension mismatch |
| [GHSA-jgq8-pprg-vc68](https://github.com/tesseract-ocr/tesseract/security/advisories/GHSA-jgq8-pprg-vc68) | <=5.5.3 /None | LSTM gate/dimension mismatch |
| [GHSA-7v9h-3q3m-w68g](https://github.com/tesseract-ocr/tesseract/security/advisories/GHSA-7v9h-3q3m-w68g) | <=5.5.3 /None | Recoder values, out-of-bounds write |
| [GHSA-2hm8-q5c7-c373](https://github.com/tesseract-ocr/tesseract/security/advisories/GHSA-2hm8-q5c7-c373) | <=5.5.3 /None | Unicharset desynchronization |
| [GHSA-88qp-4g94-3rf3](https://github.com/tesseract-ocr/tesseract/security/advisories/GHSA-88qp-4g94-3rf3) | <=5.5.3 /5.5.4 | GenericVector model counts |
| [GHSA-rphx-x795-5qjv](https://github.com/tesseract-ocr/tesseract/security/advisories/GHSA-rphx-x795-5qjv) | <=5.5.3 /None | Legacy inttemplate counts |

Two additional inspected advisories list affected <5.5.3: [Convolve](https://github.com/tesseract-ocr/tesseract/security/advisories/GHSA-7j76-5rq5-5jg8) lists patched5.5.3; [DAWG](https://github.com/tesseract-ocr/tesseract/security/advisories/GHSA-x3vq-7rr7-5x3h) lists patchedNone despite affected<5.5.3, so it needs disposition rather than assuming fully fixed. The official release page still identified5.5.3 as Latest when researched; references to patched5.5.4 do not establish an available reviewed release. [Leptonica advisory page](https://github.com/DanBloomberg/leptonica/security/advisories) had no published advisories; this does not establish absence of vulnerabilities. Further image-parser review/isolated failure testing remains.

## Exact next authority, residual risk and recommendation

Source intake is complete, but **product admission is not ready**. Human Technical/Security owner must disposition OCR-SRC-SEC-001 before execution. Do not silently switch versions or patch vendor source. Preferred product path: reviewed upstream remediation for all relevant issues, or an exact reviewed patchset with a separate decision, tests and immutable binding. Acquisition scope must name any new source/patch assets first.

If the human owner wants a limited development feasibility test before product disposition, exact proposed scope is:

1. Explicitly accept the documented residual model-loader risks **only for bounded source build/synthetic evaluation**, not product activation/distribution. Use the exact acquired source hashes above with existing CMake3.30.5/MSVC19.43.34810 tools; no dependency acquisition. Build independent workspace output, static CRT if feasible, code-generation/TIFF configure probe included, cap build concurrency2 and stop on missing components rather than auto-fetch.
2. Minimal profile: training/tests/OpenMP/legacy/graphics disabled; curl/archive/TIFF disabled; Leptonica SW and optional codecs disabled; keep debug/system-command paths off. Inspect generated config and PE imports before engine run. No automatic system install or PATH changes.
3. Runtime only synthesized BMP/PNM fixtures (no personal/Production PDF), OEM1, exact held eng/ind models, hashes rechecked with read locks for every run. No user-supplied model path/config/URL, no training, hostile model or network fallback. Use task-owned temp/output, fixed safe errors, cancel and process-tree supervision; cap60seconds/run and1GiB aggregate sampled memory, serial recognition. These are proposed safety ceilings, not canonical performance/accuracy acceptance gates and not an OS sandbox claim.
4. Before any engine run, demonstrate a suitable approved isolated/network-disabled test boundary. Current worker separation alone is not a sandbox. If that boundary cannot be established within existing authority/tools, stop runtime and return compile-only evidence. Do not enable/alter system security settings or launch extra environments without required authority.

No build/test authority is inferred from acquisition approval. Accuracy, mixed-PDF preservation, invisible-layer alignment, A3/A2, cancellation/failure safety, offline proof, native UI and physical UAT remain unproved. No update package exists for OCR. Report to parent and await the material security/execution decision; do not claim implementation complete.
