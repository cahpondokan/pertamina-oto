# Shared LOCAL OCR — development checkpoint, 1 October 2026

Latest checkpoint: RapidOCR candidate static intake reviewed. Seven exact dependency packages pass catalog SHA512/CRC/path/nuspec/signature checks; 20 pinned source/text files match tree blobs; 16 ZIP guard checks PASS. **HOLD**: v5 model provenance/license scope, incomplete native ORT/macOS downloads, embedded native license/security mapping and runtime evidence. No application activation or engine execution. See [RAPIDOCR_STATIC_INTAKE.md](RAPIDOCR_STATIC_INTAKE.md) and [OCR_FREE_CANDIDATES.md](OCR_FREE_CANDIDATES.md).

Earlier Tesseract checkpoint: source acquisition and static extraction/review completed; all1712 file blobs match exact official commits;16 synthetic archive-guard checks PASS. Security disposition remains required for eight upstream advisories affecting Tesseract5.5.3. See [OCR_SOURCE_INTAKE.md](OCR_SOURCE_INTAKE.md). Historical sections remain below; they do not authorize runtime/product admission.

## Alternative search — 1 October 2026 (read-only evaluation)

Parent relayed direct user request **"lanjutkan pencarian alternatif"**. New provider/archive/tool acquisition and execution remain outside the previous exact installer/model approval. No new package/source archive/tool downloaded or executed in this search. Documentation and source excerpts were inspected through web read tools only. No code/catalog/build/update changed. Status **ALTERNATIVE_IDENTIFIED_PENDING_SOURCE_INTAKE_AUTHORITY**.

### Recommended candidate: minimal build from official source

Candidate Tesseract **5.5.3**, commit `db0ec62f81b0737fbbe184d8fea40af5738f8eef`; candidate Leptonica **1.87.0**, commit `13275a278eb55b5746e33f95fbf5a2c8f604b3ab`. Official Tesseract release page displays its tag as GPG Verified, key ID `E08C21D5677450AD`. This is the GitHub observation, not local cryptographic verification or verification of the Windows installer. Leptonica release/source is identified; no local signature verdict claimed. Acquire by immutable commit, bind archive hash and per-file manifest, check reviewed tree against that identity, inspect all build hooks/licenses and resolve security findings before build authority.

Exact proposed acquisition-only URLs:

- `https://github.com/tesseract-ocr/tesseract/archive/db0ec62f81b0737fbbe184d8fea40af5738f8eef.tar.gz`
- `https://github.com/DanBloomberg/leptonica/archive/13275a278eb55b5746e33f95fbf5a2c8f604b3ab.tar.gz`

Allow only expected HTTPS GitHub/codeload redirects; log sanitized host/path, not temporary query tokens. Proposed extraction uses existing Python stdlib tarfile with traversal/link/absolute-path/size/count safeguards and a new task-owned workspace root; no NSIS extractor or package manager installation. Archive hashes must be measured after acquisition; currently unknown. Do not run CMake configure while doing static intake, because build scripts/configuration checks execute code.

License declarations: Tesseract Apache-2.0, Leptonica BSD-style permission including source/binary redistribution subject to retained notices, models existing Apache-2.0. No license fee proposed. All source-file exceptions, toolchain/runtime redistribution, generated notices, vulnerabilities and compiled imports still require review. A locally built binary is expected to be unsigned unless separately signed; its admission must explicitly use reviewed-source/toolchain/build-hash evidence. This does not override or bypass OCR-SIG-001 for the held prebuilt installer, and is not public signing/distribution approval.

Feasibility: existing Visual Studio2022 Community contains MSVC x64 compiler, CMake and Ninja (not in PATH). These were statically inventoried, not executed:

| Tool | Observed version | SHA-256 |
|---|---|---|
| CMake | 3.30.5-msvc23 | 76129878FE6DC5A2246415D442DE8BC01688FF9A48EB5D9498EA374DE8B5A086 |
| MSVC cl.exe | 19.43.34810.0 | 9BEEC04038C74406E4C055593EDC07DDDA7B166272D77CBF85507D5A6BE29FF0 |
| Ninja | version metadata absent | 5020138B3757035DF9DCA9A2243624D5810FFA6AE24444BD95F752CBD1B89123 |

Paths: `C:/Program Files/Microsoft Visual Studio/2022/Community/Common7/IDE/CommonExtensions/Microsoft/CMake/CMake/bin/cmake.exe`, sibling `CMake/Ninja/ninja.exe`; compiler under `VC/Tools/MSVC/14.43.34808/bin/Hostx64/x64/cl.exe`. Existing vcpkg executable is also present but its automatic acquisition is not authorized and is not proposed. No matching OCR package in the inspected user NuGet cache; no pacman/gpg at conventional MSYS2 paths. Bounded observations do not prove global absence.

Proposed compile-only profile after intake/admission: Leptonica SW_BUILD=OFF, BUILD_PROG=OFF, all optional image codecs OFF; Tesseract training/tests/OpenMP/debug graphics OFF and curl/archive/TIFF disabled. Windows renderer supplies a bounded uncompressed BMP/PNM raster for recognition. Leptonica has internal BMP support; this minimizes third-party codec inventory while preserving source PDF images through the separate PDF layer pipeline. BMP/PNM decode, reduced build linkage, static CRT feasibility, eng/ind OEM1 recognition, TSV output and accuracy must be verified at runtime; none is yet proven. Larger temporary raster size requires disk/pixel limits. External format decoding remains in existing Windows renderer, not this minimal engine. Initial runtime budgets and complete supervision remain as described below.

Sources supporting this design:

- [Official signed Tesseract release/tag](https://github.com/tesseract-ocr/tesseract/releases/tag/5.5.3), [immutable source commit](https://github.com/tesseract-ocr/tesseract/commit/db0ec62f81b0737fbbe184d8fea40af5738f8eef).
- [Official source build guidance](https://tesseract-ocr.github.io/tessdoc/Compiling.html), [5.5.3 CMake options](https://raw.githubusercontent.com/tesseract-ocr/tesseract/5.5.3/CMakeLists.txt). Required Leptonica minimum1.74; candidate1.87 exceeds this minimum, which does not prove ABI/build compatibility. Optional network/archive support can be disabled.
- [Leptonica immutable source commit](https://github.com/DanBloomberg/leptonica/commit/13275a278eb55b5746e33f95fbf5a2c8f604b3ab), [1.87 build options](https://raw.githubusercontent.com/DanBloomberg/leptonica/1.87.0/CMakeLists.txt), [license](https://raw.githubusercontent.com/DanBloomberg/leptonica/1.87.0/leptonica-license.txt). Optional codecs and Software Network build can be disabled.

### Other evaluated paths

| Candidate | Verifiable metadata | Assessment and permission boundary |
|---|---|---|
| MSYS2 mingw-w64-x86_64-tesseract-ocr 5.5.3-1 | Official package SHA256 `67c0a857e9f028f88d1463c0293885d806334346a81d70be5e38f08bad11e1e3`, 6.21MB, build2026-07-25, source/build CI links | Candidate fallback; direct dependency categories cc-libs/curl/icu/leptonica/libarchive/omp have a broader transitive inventory. Repository metadata/hash is not a local signature verdict. Requires exact full dependency freeze and package trust/keyring verification; no package manager installation or downloads authorized. |
| NuGet Tesseract5.2.0 by charlesw | Official gallery Apache-2.0 declaration, last update2022-11-30, package5.43MB | Older wrapper; gallery framework compatibility is not tested net10/native ABI compatibility. Native DLL identity/chain/license/hash/package signature still unknown without static acquisition. No reason to prefer its larger in-process failure boundary to a bounded subprocess solely because it is .NET. Not recommended first. |
| Corrected upstream Windows installer | Existing5.5.3 Windows asset remains signature-held | No corrected exact valid-signed artifact established from inspected release/provider pages. Do not downgrade or assume another installer is valid without review. A provider-fixed asset needs new exact hash/signature binding and authority. |
| Windows.Media.Ocr | Existing Microsoft API; desktop package identity restriction | Current unpackaged host remains unsupported for this path; Indonesian runtime support not established. Requires separate packaging/identity and actual language capability decision. |

[MSYS2 package metadata](https://packages.msys2.org/packages/mingw-w64-x86_64-tesseract-ocr), [MSYS2 package management](https://www.msys2.org/docs/package-management/), [NuGet wrapper metadata](https://www.nuget.org/packages/Tesseract/5.2.0). No vulnerability-free conclusion is made for any candidate; full review needs acquired exact tree/binary inventory. No independent OCR accuracy test or physical UAT exists at this checkpoint.

### Exact next decision

Recommend approving **acquisition-only plus safe static extraction/review of the two immutable official source archives above**, using existing tools, zero additional cost, existing eng/ind model bytes held. No new extractor/codec/package manager/toolchain/model; no configure/build/engine execution, install/system changes, product reference, update or redistribution under that approval. Stop and report if another dependency/tool/cost is required or provenance/licenses/security cannot be resolved. After review, present exact manifest, findings and bounded build/synthetic-execution proposal for separate human technical/security admission. Source build is a candidate alternative, not a completed OCR implementation.

## Acquisition follow-up — 1 October 2026, Asia/Jakarta

User explicitly replied **"ok saya izinkan. lanjutkan"** after the acquisition/report-back permission request and zero-license-fee explanation. This authorizes the proposed acquisition-only scope and report to the named parent chat; it does not waive intake findings or authorize engine execution/installation/distribution. Status now **ACQUIRED_HELD_SIGNATURE_FINDING**. Historical checkpoint below is preserved.

Downloaded exact approved assets into `artifacts/ocr-intake-20261001`, without executing installer or engine. HTTPS-only curl acquisition returned exit 0. Official GitHub release metadata was also acquired for digest verification. Host observation UTC `2026-10-01T03:50:50.5879575Z` (10:50 WIB).

| File | Bytes | SHA-256 |
|---|---:|---|
| tesseract-ocr-w64-setup-5.5.3.20260724.exe | 26573224 | BEE9E3434BD94FD65387D9BE28CD467A41F61B1275383B55B0F59A1331270AE4 |
| eng.traineddata | 4113088 | 7D4322BD2A7749724879683FC3912CB542F19906C83BCC1A52132556427170B2 |
| ind.traineddata | 1122661 | 69786901DA87AB8766C1EA7FBB10B28F2110C14DA3F6C8F2735DF131FBA95D88 |
| LICENSE (official pinned model repository) | 11358 | CFC7749B96F63BD31C3C42B5C471BF756814053E847C10F3EB003417BC523D30 |

Installer SHA-256 and byte length exactly match the asset `digest`/`size` in [official release metadata](https://api.github.com/repos/tesseract-ocr/tesseract/releases/tags/5.5.3). Embedded ProductVersion is `5.5.3.20260724`, FileVersion `2026-07-24-22-07-15`. Hash equality proves acquired bytes match the publisher's release metadata; it does not establish a valid signature or security admission.

**Blocking finding OCR-SIG-001:** `Get-AuthenticodeSignature` returned `UnknownError`: "A required certificate is not within its validity period when verifying against the current system clock or the timestamp in the signed file." Signer `CN=Universität Mannheim, O=Universität Mannheim, S=Baden-Württemberg, C=DE`, thumbprint `2F92CB990D57719BDCCA2D72134378614A040D9B`, certificate validity `2022-12-09T00:00:00Z` through `2023-12-09T23:59:59Z`. Timestamp certificate subject `CN=PN: Zeitstempel 2026, O=DFN-CERT Services GmbH, L=Hamburg, S=Hamburg, C=DE`, validity 2026-06-23 through 2029-06-22. Certificate fields are observations, not a diagnosis of the exact timestamp-chain failure. Windows validation did not PASS; do not disable signature checks or execute the installer to get its files.

**Remaining inventory blocker OCR-INV-001:** existing `tar.exe -tf` cannot read this NSIS installer (`Unrecognized archive format`, exit 1); no existing 7-Zip executable was found at conventional paths/PATH. No new extractor was acquired. Bundled DLL versions, licenses/notices and security findings remain unreviewed; Apache-2.0 declaration for engine/models does not admit all bundled components.

Quality task is now completed/idle, as is reader task; shared task activity no longer blocks ownership. No product code, UI/catalog, OCR execution, build, synthetic recognition test, screenshot or local update was produced. Existing application installation remains unchanged.

Next decision for human Technical/Security/License owner: prefer requesting a corrected validly signed exact Windows artifact from the provider, or separately authorize a new exact source-build/alternative-binary candidate and its static extraction toolchain for review. Candidate substitution, a signature exception, additional tool/model acquisition and execution must not be inferred from the current acquisition-only consent. Original download is held for evidence. Full integration remains unfinished.

Status: **BLOCKED_PROVIDER_ADMISSION**. This is an audited integration design and decision packet, not an implemented OCR feature. No engine/model was downloaded, no product source changed, no OCR tests/build/package published and no phase gate changed. Work performed by one agent; no subagents.

Baseline: branch `task/core-functional-first`, HEAD `84cbb0f538020003bcfb6583e94c2fcf6524f87c`, existing heavily dirty worktree preserved. Scope: user-requested reusable offline OCR, `FEAT-OCR-004`, `POC-OCR-001`, `UAT-CORE-009`. Scan, Word/Excel/Markdown and assistant activation remain separate. Central project state/register/log belong to the parent integrator and were not edited here.

## Observed admission and platform boundary

- `ToolCatalog.cs` keeps OCR unavailable. Desktop project targets `net10.0-windows10.0.19041.0`, explicitly `WindowsPackageType=None`. Existing worker targets net10.0 and references PDFsharp plus three supporting packages; no OCR provider/model is referenced. Central package list has no OCR dependency.
- `Get-Command tesseract` returned no command. Conventional Program Files Tesseract directories and worktree filename inventory returned no engine/model. This is a bounded inventory, not a claim that no OCR binary exists anywhere on this computer.
- Read-only `C:/Windows/OCR` directory inventory showed ar-sa, en-gb and en-us, no Indonesian directory. Directory presence alone does not prove recognizer availability. DISM capability enumeration required elevation and was not retried with elevation.
- Microsoft documents Windows.Media.Ocr as supported for desktop applications **with package identity**. It is therefore not a supported drop-in solution for the current unpackaged host. No registry/package/system-language changes were made.
- Existing conversion/OCR freeze packet explicitly has no approved provider/disposition and prohibits inheriting PDF Merge admission. P5-D01 remains OPEN. Shared contract section 7 requires stopping the affected lane when provider/license/security boundary is undecided; root AGENTS section 7 prohibits acquiring a dependency before admission/license/provenance/security/authority.
- Reader task `01a0f556-f1d4-7023-8dd1-685feaa7f5f1` was completed when inspected. Quality task `01a0f563-b888-77a3-acee-d63d41e1cba4` was active. No shared reader/worker/protocol writes or MSBuild/publishing were attempted.

Primary sources inspected (metadata/documentation only):

- [Microsoft OCR support boundary](https://learn.microsoft.com/en-us/uwp/api/windows.media.ocr?view=winrt-26100).
- [Tesseract installation, engine/model split and Windows provider](https://tesseract-ocr.github.io/tessdoc/Installation.html).
- [UB Mannheim Windows binary provider](https://github.com/UB-Mannheim/tesseract/wiki), currently lists `tesseract-ocr-w64-setup-5.5.3.20260724.exe`.
- [Official fast models](https://github.com/tesseract-ocr/tessdata_fast), Apache-2.0 declaration, English `eng` and Indonesian `ind`; immutable candidate revision `2a9d6efdcaf27824c24a87d5b6309e9c250ff868`.
- [LSTM-only model compatibility](https://tesseract-ocr.github.io/tessdoc/Data-Files.html): fast models require OEM 1. These sources support candidate selection, not runtime accuracy or binary admission.

## Concrete decision packet — OCR-ADMISSION-20261001 (PROPOSED)

Recommended route for the existing unpackaged application: Tesseract x64 child process, candidate Windows build **5.5.3.20260724** from UB Mannheim; only `eng.traineddata`, `ind.traineddata`, LICENSE/NOTICE metadata from the pinned official fast-model revision above. Candidate engine and models declare Apache-2.0; bundled DLL/runtime/codec licenses, notices, exact bytes/hashes, provenance and vulnerabilities are **not yet reviewed**. No additional license fee is proposed; stop if a cost or additional dependency is necessary.

Human decision needed: authorize acquisition-only of that exact candidate engine installer and two pinned models into a new workspace intake directory for static hash/provenance/license/security review. No installer execution, PATH/system mutation, language-pack installation, OCR execution, product reference or redistribution is included in acquisition-only authority. Hashes must be measured from acquired bytes, never invented. Stop if the pinned assets disappear/change, signature is invalid, source is redirected unexpectedly, transitive inventory cannot be established or a licensing/security finding is unresolved. Do not invoke the installer against an existing folder: the provider warns that its uninstaller can remove the installation directory.

After static review, present the exact engine/DLL/model manifest and findings for technical/license/security admission and bounded synthetic execution authority. Acquisition approval alone does not authorize execution or distribution. Local packaging must separately include reviewed redistribution notices and hash-bound assets; Windows OCR/MSIX packaging is an alternative architectural decision, not an automatic fallback. User/temporary Technical-License-Security authority owns the decision; this agent does not approve its own intake.

## Reusable contract to implement after admission

`LocalOcrCapabilities`: admitted provider/version/model hashes, actual discovered languages, availability and safe reason code. Probe only the admitted absolute engine path; no PATH search or runtime downloads. Show Indonesian/English/both only when exact models exist and runtime language discovery confirms support. Mixed recognition uses `ind+eng`, not a claim that language classification is automatic.

`LocalOcrRequest`: source handle/locked local path, selected one-based page numbers, language choice, cancellation token and explicit resource policy. Source/password must never be command-line arguments or ordinary logs. No URL/UNC input, shell interpolation, service/upload or automatic fallback. Page selection reuses existing range validation. Serialize only one page at a time; bounded raster tiles must retain their coordinate transform. Raster DPI reduction must be disclosed rather than silently claiming full accuracy on A2/A3.

`LocalOcrPageResult`: source page number, pixel dimensions, transform to original PDF coordinates, words/lines with boxes, nullable provider confidence and status (`Recognized`, `ExistingTextSkipped`, `NoTextRecognized`, `UnsupportedTextDetection`, `Failed`). Text and confidence payloads are restricted in-memory DATA, never telemetry/history/diagnostic content. Confidence is provider-specific and not calibrated probability. Results are not commands, prompts or spreadsheet formulas. Downstream consumers own formula/link/prompt safeguards and layout conversion; OCR does not promise table reconstruction.

`LocalOcrProgress`: page index/count and stage only, no text/path/password. `RecognizePageAsync` yields structured recognition for scanner/editor/future conversions. `CreateSearchablePdfAsync` uses that recognition to add an invisible searchable text layer while retaining the original page content/resources/images. Avoid replacing the source page with a newly rendered raster PDF.

### PDF preservation and duplicate prevention

Preserve page order/count, MediaBox/CropBox, rotation, physical size and source image streams. Map top-left raster word boxes through the actual render transform, including rotation/crop offsets and PDF user space; validate supported UserUnit/page coordinates. Handle unknown transforms with typed unsupported errors. Use PDF text rendering mode 3 with font mappings searchable by the existing reader; opacity zero alone is insufficient evidence. Reopen independently through the existing Windows renderer and existing search worker.

The current `ReaderSearch.ExtractPage` is partial for Form XObjects and some font encodings. Its empty result cannot establish that a page has no text. Before OCR, separately inspect text objects, including nested forms with depth/cycle/resource bounds. Skip pages with existing text; if inspection is inconclusive, skip with a limitation instead of adding a second layer. Initial mixed-PDF rule is page-level skip: a page containing both live text and a scanned region needs later region OCR and must be disclosed as skipped. This avoids falsely promising that every scanned region in a mixed page is recognized.

Reject or disclose encrypted/signed/forms/unsupported documents according to existing admission rules; never invalidate signatures silently. Save to a unique new output name in the existing default folder, stage on that volume, validate before atomic create-new publication. Hold source read locks and verify unchanged source hash in tests. Cancellation before publication produces no final artifact; cancellation after an atomic successful move reports success consistently.

### Process, memory and cleanup

Reuse current input ceilings (100 MB, 1000 pages) and bounded output checks where applicable. Existing WorkerClient uses a two-minute timeout and a sampled 1 GiB worker limit; these do not automatically bound an OCR grandchild or whole document raster memory. Extend supervision to the full owned process tree and aggregate resource budget before activating OCR. Candidate OCR per-page/whole-job budgets require measurement and explicit disposition; do not fabricate a quality/performance gate. Limit pixel count, engine threads, response bytes/word count, temporary disk usage, per-page time and whole-job time; permit cancel between render/recognize/embed/validate steps.

Temporary raster/OCR files use a new task-owned GUID directory with constrained local access. Clean only exact owned files/directories after verifying the absolute target is under that root; dispose streams/processes first, handle failures without deleting source/final paths. Do not persist OCR text for diagnostics. Filter engine stderr to fixed reason codes. Audit that the selected binary cannot auto-fetch assets or execute document instructions; offline operation requires network-disabled evidence, not merely absence of API calls in C#.

## Integration ownership and UI plan

New OCR service/provider/result files first; shared Protocol.cs, worker dispatch and WorkerClient supervisor only after the quality task completes. Host entry screen reuses existing upload/global paste/default output flow, page selection, real language capability, progress/cancel and unified result. Activate catalog only after real end-to-end OCR passes. Reader offers **Kenali teks dengan OCR** on reliably classified scanned pages and saves/opens a derivative; unknown extraction warns honestly. Editor may consume recognized text as a starting layer without claiming to erase/automatically rewrite scan images. Future scanner/conversion/assistant consumers receive the same contract without enabling their catalog entries.

## Verification plan (NOT RUN)

Synthetic fixtures: clean Indonesian, clean English, bilingual scan, rotated A3/A2, crop offsets, multi-page scan, mixed native-text/scan pages, mixed content on one page, nested form text, unsupported encoding, blank/noisy page. Keep answer text only in synthetic fixtures; evidence stores counts/metrics and fixture identifiers. Measure sample character/word accuracy and known-term search/copy; no universal accuracy claim. Independent reopened geometry/visual comparison and image-stream hashes prove preservation; word boxes/known coordinates verify invisible-layer alignment.

Negative paths: no engine/model, mismatched admission hash, wrong language, no recognition, oversized pixels/input/output/response/temp, timeout, memory/tree termination, pre-start and in-flight cancel, malformed PDF, source/output aliases, existing output, disk failure, partial text detection, protected/signed/form input and cleanup failures. Assert source unchanged and no published partial result. Run existing worker/editor/reader/search regressions, native UI audit and screenshots. Physical mouse/copy/paste/accessibility UAT remains separate. Only after passing real OCR-specific and existing checks, run `eng/Build-LocalUpdate.ps1` per LOCAL_UPDATES.md with all shared tasks complete. No update exists for this checkpoint.

## Handoff

Completed: read-only preflight, provider/platform/dependency audit, concrete acquisition/admission packet, reusable contract and preservation/resource/UI/test design. File changed: this document only. Not completed: provider acquisition/admission, implementation, OCR recognition/searchable output, UI, runtime tests/screenshots and update package. Blocker: exact provider/model admission and synthetic execution authority, owned by human Technical/License/Security authority. Shared-file work additionally waits for quality task completion. No phase COMPLETE, release readiness or physical UAT claimed.
