# RapidOCR static intake — 1 October 2026

Result: **HOLD for engine execution/product admission**. Seven independent packages pass the static integrity/signature checks below. RapidOcrNet model-containing package and four v5 artifacts remain unacquired because exact converted-model provenance/license scope is not closed. Native ONNX Runtime and macOS package downloads are incomplete. This is an acquisition/static review checkpoint, not an OCR implementation or an assurance of zero vulnerabilities.

Task: shared LOCAL OCR; IDs FR-SCAN-002, FEAT-OCR-004, POC-OCR-001, UAT-CORE-009. Authority relayed from user: “ok saya izinkan. periksa dulu dan pastikan aman”, scoped to metadata, isolated acquisition, safe extraction and static evidence. Baseline HEAD 84cbb0f538020003bcfb6583e94c2fcf6524f87c, task/core-functional-first. Existing dirty files preserved. One agent, no subagents, no application/feed/PATH/central phase-record edits; no restore/configure/build, package scripts or OCR/DLL/model execution. SDK signature verification and COFF import inspection are existing-tool static checks, not engine runs.

## Exact component disposition

PASS below means hash/CRC/path/nuspec identity and NuGet signature PASS only. Every component still requires integration/runtime approval, notices retention and relevant security disposition.

| Component | Pin | Static result | License/evidence limit |
|---|---|---|---|
| RapidOcrNet source subset | 4.2.0 / 7b5c432e4262470b6d81bf1710c3bd52fc438566 | 20 source/text files match official tree git blobs | Apache-2.0 with embedded PContour MIT headers; tag commit unsigned. Package/binary/source binding not tested because model-containing package held |
| RapidOcrNet package | 4.2.0 | HOLD, not downloaded | Contains models whose exact provenance/license remains unresolved |
| Four Latin model/dictionary files | v5, source paths in pinned RapidOcrNet tree | HOLD, not downloaded | See model boundary below |
| Clipper2 | 2.0.0 | PASS | Boost-1.0 text included; repository signature verified; nuspec has repository URL without commit |
| Microsoft.ML.OnnxRuntime CPU native | 1.30.0 | HOLD, incomplete download | Official size 157230753 bytes; 4194304-byte initial partial and 7161265-byte bounded retry retained, never extracted or loaded |
| Microsoft.ML.OnnxRuntime.Managed | 1.30.0 | PASS | MIT plus full ThirdPartyNotices; nuspec commit f2c39fe2f838cf35ce7da92824f5a5e3ee6e88a7 |
| SkiaSharp | 3.119.1 | PASS | MIT; source commit cc78b5933d23e6383db5d246e70db915770d55d6 |
| SkiaSharp.NativeAssets.Win32 | 3.119.1 | PASS integrity/signature; HOLD native legal/security mapping | MIT outer package plus composite third-party notices; x64/x86/arm64 DLL Authenticode Valid, Microsoft signer |
| SkiaSharp.NativeAssets.Linux.NoDependencies | 3.119.1 | PASS integrity/signature; HOLD native legal/security mapping | Unconditional wrapper dependency; not needed as Windows runtime payload, but graph/redistribution still requires explicit handling |
| SkiaSharp.NativeAssets.macOS | 3.119.1 | HOLD, incomplete download | 625012-byte partial vs official 7099396, never extracted |
| System.Memory | 4.5.5 | PASS | MIT LICENSE.TXT plus notices retained; netcoreapp2.1 group has no extra dependencies |
| System.Numerics.Tensors | 9.0.0 | PASS | MIT plus notices; net9/net8 groups have no extra dependencies; repository commit 9d5a6a9aa463d6d10b0b0ba6d5982cc82f363dc3 |

The metadata-derived net10 CPU graph has these exact pins. RapidOcrNet minimum ORT 1.29.0 is intentionally a candidate override to 1.30.0, not an admitted version change. For a generic net10 worker SkiaSharp net8 metadata also includes macOS; Windows-specific target group uses Win32 only. Actual restore selection has not been executed. Older-framework System.Memory/Unsafe/Buffers graphs are not automatically acquired because the selected modern groups do not require them. No floating/latest package acquisition or automatic restore occurred.

## Acquisition/extraction evidence

Root: artifacts/rapidocr-intake-20261001. Metadata comes from official NuGet registration/catalog and pinned GitHub API/raw source. Seven complete packages total 59246243 bytes, 164 extracted files, 135039787 expanded bytes. All match NuGet catalog SHA512 and size, with per-file SHA256 and archive SHA256 in static-inventory.json. Hash equality is integrity/provenance evidence, not a malware proof.

| Package | Archive SHA256 |
|---|---|
| Clipper2 | f07dfcf6adccd035fde042f0ff6fd6d746b989e9bb361cda38216f185304172b |
| ORT.Managed | 46c0b692c4d0d4e63c37686b31d3a4d447b8ba28ac5f9ccb0f741d94de9d69ac |
| SkiaSharp | 4c856be7636943d71a6f978909c1ff3981e334e4ca8700aaa4294ad9cf12e133 |
| Skia Linux | 4a9ad38720294e16c3a1e4e7fc969a4bb4ca766f5290ca153bc57c1ee0b2a692 |
| Skia Win32 | c923e49fc4ab4a13aa862842df7109e0726df5d7ac0c8482e20008e054018b65 |
| System.Memory | 10f43da352a29fb2b3188e4edd4dcf5100194c8b526e4f61fe2e2b5623775a22 |
| System.Numerics.Tensors | b750243c36002a62b28b1ac5d3fbc284ad340ba1494cc36aca110611a0b1f959 |

Extractor rejects traversal, absolute/drive/ADS/backslash/control/reserved paths, noncanonical segments, case/prefix collisions, encryption and link/special entries. Original ZIP name is checked before Windows normalization can hide a backslash/NUL. All metadata validates before payload writes; bounded reads check CRC and exact size, create-new destinations, 10000 entries, 256MiB download/file, 1GiB expanded/file-archive ceiling, compression ratio ceiling. Downloads use HTTPS official host with no redirects, 15s connect/90s total deadline and expected size cap. Original partials are preserved and excluded from accepted inventory.

Initial ZIP guard tests exposed Windows filename normalization; fixed using orig_filename checks. Initial long-path extraction errors left partial extracted copies, which are not accepted evidence. Final re-extraction to short x/index paths with Windows extended-path handling succeeds for all seven complete packages. Authoritative inventory is static-inventory.json, not the historical package-manifest.json error list. Final synthetic guard tests: 16 PASS, python -B eng/TestRapidOcrPackageIntake.py exit0; evidence guard-tests-64cfebed3e194f788a3a822014c5c413/results.json. This tests intake safety, not OCR.

Existing SDK 10.0.400 `dotnet nuget verify <seven complete packages> --all --verbosity normal` exit0, seven successfully verified; signature-verification.log and signature-verification-result.json. Clipper2 has NuGet repository signature; Microsoft packages show author/repository signatures. Strong-name signing in RapidOcrNet project is not Authenticode or an independent trust proof.

## Model boundary and licenses

Pinned RapidOCR metadata revision 549847df506a32421086d2a08d19646ee958a6e0 contains [python/MODEL_LICENSES.md](https://github.com/RapidAI/RapidOCR/blob/549847df506a32421086d2a08d19646ee958a6e0/python/MODEL_LICENSES.md). This corrects the previous research's unresolved path (the file is under python, not root or python/rapidocr). Its explicit covered-artifact section lists only v6 det_small, v6 rec_small and legacy v2 classifier, and says other models need their own source records. Therefore that notice alone does not close the selected v5 set.

Four held files: ch_PP-OCRv5_mobile_det.onnx, ch_PP-LCNet_x0_25_textline_ori_cls_mobile.onnx, latin_PP-OCRv5_rec_mobile_infer.onnx, ppocrv5_latin_dict.txt. RapidOcrNet NOTICE declares PaddleOCR/Apache-2.0 origin; official PaddlePaddle [Latin model card](https://huggingface.co/PaddlePaddle/latin_PP-OCRv5_mobile_rec) and [orientation model card](https://huggingface.co/PaddlePaddle/PP-LCNet_x0_25_textline_ori) also label Apache-2.0. These are positive upstream-license evidence, but not proof that the particular renamed converted bytes/dictionary in RapidOcrNet match that source and conversion. RapidOCR registry uses latin_PP-OCRv5_rec_mobile.onnx (without infer suffix), with SHA256 b20bd37c168a570f583afbc8cd7925603890efbcdc000a59e22c269d160b5f5a; classifier SHA256 54379ae5174d026780215fc748a7f31910dee36818e63d49e17dc598ecc82df7. Name matching alone is insufficient. No substituted v6/new model was acquired.

Native notices require retention in full. Skia aggregate notices cover ANGLE, HarfBuzz, Skia, etc1, GIF, libpng, DNG SDK, expat, FreeType, ICU, imgui, jsoncpp, libjpeg-turbo, libwebp, libmicrohttpd, piex, SDL, sfntly, SPIR-V headers/tools and zlib. Terms include BSD/MIT/Apache, libpng/zlib/IJG/FreeType/ICU, Adobe DNG terms, GIF MPL1.1/GPL2/LGPL2.1 choices and libmicrohttpd LGPL2.1/eCos choices depending configuration. This aggregate is not proof all are linked, nor permission to claim the native library is MIT-only/GPL-free. Exact build/link/revision applicability and obligations remain HOLD for legal/security owner. ORT.Managed includes Intel MKL/Eigen/ONNX and other aggregate notices; native ORT package missing, so runtime composition remains unverified. System.Memory/Tensors MIT texts and their notices are retained separately.

## Security/static source findings

Ten exact NuGet package/version GitHub advisory queries returned empty arrays. Official NuGet base/update snapshot has one historical SkiaSharp entry [GHSA-j7hp-h8jx-5ppr / CVE-2023-4863](https://github.com/advisories/GHSA-j7hp-h8jx-5ppr), range >=2.0.0,<2.88.6; candidate3.119.1 lies outside that range. No matching package-version advisory found in these sources. Missing fields/empty query results do not prove absence of unpublished, embedded/native or model-loader vulnerabilities. security-metadata-manifest.json binds snapshot receipts; index/feed dates recorded in files.

SkiaSharp source tree binds externals/skia gitlink 40f75dc0051d141913c07c20d4c19590c7da0cb7. That is a starting point for native SBOM, not completed compiler/link evidence. Static `dumpbin /dependents` x64 exit0 lists ole32, FONTSUB, USER32, d3d12, D3DCOMPILER_47, KERNEL32; imports alone cannot identify statically linked codecs or prove no network activity.

20-file RapidOcrNet source review: model paths default to relative models/v5, and InitModel accepts paths with File.Exists followed by InferenceSession; no hash/dictionary size lock. Path image overload decodes SKBitmap before later resampling, so image dimensions/resource validation must precede decode. Some failures are swallowed into null/default results and Debug.WriteLine(exception/stack), which requires wrapper failure validation and sanitized logging; release/debug behavior not tested. Cancellation uses per-run RunOptions.Terminate and disposes registration before options; recognition default parallelism1, but default ORT thread count0 is automatic. Explicit CPU/thread/pixel/page/model bounds and process-tree supervision still required. No HTTP/download/process-start call found in this source subset; ORT/OS/native behavior is not covered by that observation. Do not expose model paths/custom operators to user input, and do not treat empty OCR as successful processing after engine failure.

## SDK first-run observation

The first signature-verification invocation with a fresh task-local CLI home emitted “Installed an ASP.NET Core HTTPS development certificate.” That unexpected initialization message is recorded as a scope deviation/uncertainty, not silently ignored. Subsequent read-only CurrentUser/My observations show only localhost ASP.NET certificates dated 25 February and 2 September 2026, no certificate dated this intake. There was no pre-invocation store baseline, so no exhaustive before/after mutation claim is made. No certificates were trusted, removed or manually changed. Subsequent verifier explicitly disabled certificate generation/workload notification per process and completed exit0. certificate-observation.json/after.json retain public metadata only. No private keys exported.

## Evidence and next work

Authoritative static-inventory.json SHA256 569FBB88A1CB32C9282CECA4715BCEE0B8C8C8F059B3B791FD0E68C318C7D31C; metadata-manifest.json 3B7BEB8FE0D1B8467C613316D80E51BC387C76C401030CFB32E237DF29215D9E; signature log D04B10BFF1A188864D819E5DA39E636F319DC55B06E0EBCB14C5B4CBA4F522B3. Custom inventory/SBOM draft includes file hashes, licenses/notices, repository binding, native/build files; it is not a fully resolved CycloneDX/native build SBOM.

Exact blockers/owners: Technical+License owner must bind the four v5 converted artifacts/dictionary to source/license/attribution before their acquisition and RapidOcrNet package intake; Technical owner must obtain complete ORT CPU1.30.0 and relevant macOS graph artifact using verified official metadata (the current authorization is acquisition-only, not a waiver); License+Security owner must disposition embedded native composition/licenses/advisories and package-to-source provenance. No need to request identical acquisition authority again within that exact scope; changed versions/models/sources require separate authority.

After static admission is complete, separate execution authority is still required for isolated CPU synthetic ID/EN/mixed PDF tests, word coordinates/searchable text/font/rotation, A3/A2 tile fidelity, cancellation/timeout/memory/source hashes/temp recovery, and OS-enforced network-disabled/telemetry evidence. Product integration/update follows only after the required evidence. Runtime safety, accuracy, physical UAT and Production readiness are unproved.

Authored paths: eng/RapidOcrMetadataIntake.py, RapidOcrMetadataFollowup.py, RapidOcrPackageIntake.py, TestRapidOcrPackageIntake.py, RapidOcrStaticInventory.py, RapidOcrSecurityMetadata.py; this report and links/corrections in OCR_FREE_CANDIDATES.md and SHARED_LOCAL_OCR.md. No final integration/phase promotion is recommended at this checkpoint.
