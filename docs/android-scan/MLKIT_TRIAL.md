# OTO Scan — Google ML Kit trial, 23 October 2026

## Scope and authority

Owner request: build a trial using Google ML Kit for scan, automatic crop and enhancement; preserve the OTO gallery, PDF reader and compression; test synthetic documents before delivering an APK; automatically capture when the document is clear. This explicitly supersedes the older no-new-scanner-dependency restriction for this bounded trial. Agent 0 only; no phase execution, Production approval, Play Console update, physical-phone testing, or publication.

Baseline: commit `697655358a6d2d3736ca0677be63c350a3d6183b`, backed up to GitHub branch `backup/oto-scan-v1.0.0-20261023`. Work remains in the existing `task/android-scan-a0` worktree. Allowed: `flutter/oto_scan/` and this trial documentation. Existing unrelated MAUI files remain untouched.

## Dependency intake

- Provider: Google ML Kit Document Scanner, `com.google.android.gms:play-services-mlkit-document-scanner:16.0.0`, explicitly selected by the owner. Official latest-version and Android integration documentation both specify 16.0.0.
- Source: Google's HTTPS Maven repository, `https://dl.google.com/dl/android/maven2/`. No third-party Flutter scanner wrapper or alternative binary mirror.
- POM inspected before implementation/acquisition. License: ML Kit Terms of Service, incorporating Google APIs Terms, not Apache licensing of the Google documentation samples. Preserve third-party notices; do not extract or modify the delivered model.
- Data boundary: document processing on device; SDK modules/models may download through Play services. Google documents SDK performance/usage metrics. Trial privacy text must disclose this; do not reuse the old no-analytics claim for the trial. The existing public privacy page/Play declarations belong to the previously submitted version and are not modified by this development task.
- Security controls: pin version; record resolved transitive dependencies, license metadata and artifact hashes; inspect merged manifest and final APK; no app-owned document upload, ad SDK, account login, or arbitrary remote file access. Perform vulnerability lookup on resolved Maven coordinates before delivery; a lookup failure is not a clean result.
- Admission is for isolated development and owner testing under the current request, not an enterprise security or public-release sign-off. Missing provenance/license, serious vulnerability or unexpected sensitive-data behavior blocks dependent packaging until resolved.

References: https://developers.google.com/ml-kit/vision/doc-scanner/android ; https://developers.google.com/ml-kit/release-notes ; https://developers.google.com/ml-kit/terms ; https://developers.google.com/ml-kit/android-data-disclosure . Metadata evidence: `../../flutter/oto_scan/evidence/mlkit-trial/dependencies/`.

## Design and verification plan

- Explicit trial build switch, app id `com.niam.ayani.oto.uji`, label **OTO Scan Uji**. Separate application storage preserves the installed app and its documents. The default production build keeps the local scanner and does not acquire the ML Kit dependency.
- Google FULL scanner mode, Auto capture/Manual selected inside Google's camera UI, gallery import for synthetic test documents, JPEG results only. SDK 16.0.0 exposes no public capture-mode setter; the app cannot force the selection. Auto capture was selected in the clean emulator and successfully captured without a shutter tap. Keep the user's own gallery, reader, PDF size policy and compressor.
- Import returned JPEGs transactionally into private draft storage, honor orientation, preserve source bytes, avoid a second crop/enhancement pass. Keep limits, cancellation, invalid-result/error handling and lifecycle safety. No repeated capture on return from cancellation.
- Verify new Flutter flows (multi-page, retake, cancel, failure, limit, save/back), native import/EXIF/rollback and existing regression suites. Exercise actual ML Kit on a Google Play emulator with synthetic portrait/landscape/card documents, then save/open/zoom/share chooser without sending. Distinguish imported-fixture checks from camera auto-capture evidence.
- Deliver a signed non-debug APK with QA hooks off, artifact hash and honest test/limitation report. Do not use or install on the physical phone in this task.

## Delivered trial and verification

Status: trial implementation complete; signed APK ready for owner testing. This is not a public-release or production approval.

- APK: `../../flutter/oto_scan/dist/OTO-Scan-Uji-MLKit-1.0.1.apk`, version `1.0.1-mlkit.1` (18), package `com.niam.ayani.oto.uji`, 35,445,921 bytes, ARM64 and x86_64. SHA-256: `ec0c037807f7b269e0dd7c8b7072c4eb48c45a8e9b5b98326fe8dbd9c3a85a91`.
- Existing OTO icon retained. Trial has separate application storage. Google FULL camera/crop/filters return JPEGs; OTO imports them without applying its old detector or enhancement again. Gallery, local PDF compressor, vertical PDF reader, sharing, and save/back behavior remain OTO implementations. No changes to `ScanStore.java`, `CompactPdf.java`, or `PdfReaderEngine.java` in this trial.
- `flutter test --no-pub`: **96 passed**, including eight new Google flow cases. Test console result was reviewed in this work session. `flutter analyze --no-pub`: no issues (`evidence/mlkit-trial/flutter-analyze.log`). `git diff --check`: passed.
- Native adapter: **29 checks passed** for JPEG byte preservation, decoded pixels, portrait/landscape/card/EXIF orientation, rollback, input limits, checkpoints, gallery, PDF geometry and rendering. Evidence: `emulator-mlkit-import.log`, `device-qa/mlkit-import-checks.json`, and `device-qa/mlkit-adapter.pdf`. Final APK Java DEX matches the tested adapter build.
- Final APK: **20 checks passed** including identity, QA disabled, original icon, signature, permissions, no test classes/signing secrets, AOT and engine libraries for both ABIs, matching ELF architectures, and 16 KB ZIP alignment. Evidence: `apk-verification.json` and associated manifest/signature/alignment files.
- Dependency intake: 65 resolved Maven coordinates checked against official POMs; no missing license metadata and no returned OSV vulnerabilities on the audit date. Binary hashes retained in `dependencies/dependency-audit.json`. This is a database lookup, not a guarantee of absence of vulnerabilities.

### Actual Google SDK runtime checks

Only the explicitly guarded `emulator-5582` was used, Android 36 Google Play x86_64. All images are generated fixtures labelled DATA BUATAN; no physical camera or personal document was used.

1. Imported portrait, landscape and small-card fixtures through Google's actual gallery picker. Google automatically cropped all three without manually moving corners. Selected Auto filter and Apply to all. Screenshots: `google-portrait-result.png`, `google-landscape-result.png`, `google-card-result.png` (before the explicit Auto-filter selection).
2. Google's result returned all three pages to OTO; OTO saved, opened and shared through the Android chooser. No recipient selected and no message/file sent. Back returned to Dokumen Saya. Evidence: `google-returned-pages.xml`, `google-pdf-reader.xml`, `google-pdf-share.xml`, `google-gallery-saved.xml`.
3. Actual output `google-scanned-three-pages.pdf`: **575,758 bytes (562.3 KiB; app displays 563 KB)**. Page sizes: 562.592 x 842 pt (portrait), 842 x 581.421 pt (landscape), 842 x 529.568 pt (card). All page rotations 0; no forced portrait page for landscape input. All three Poppler renders and a 300-dpi text detail were visually reviewed: `pdf-renders/google-1.png` through `google-3.png`, `google-text-zoom.png`.
4. Camera auto-capture: loaded the synthetic portrait into the emulator virtual-scene wall poster, used the Android Emulator's bundled `Walk_to_image_room` macro, and observed `Scanning...hold steady` followed by a captured-page review **without pressing Capture**. Repeated with the paper-only fixture, again captured automatically, then returned one page to OTO. Evidence: `google-auto-scanning.xml`, `google-auto-captured.xml`, `google-auto-paper-scanning.xml`, `google-auto-paper-captured.xml` and corresponding screenshots.

### Fixes and practical limits

- An early APK lacked Flutter `libapp.so` despite a successful Gradle result, causing startup failure. A generated build-directory junction had mixed cache paths. Restored a physical build directory, invalidated the affected generated Flutter intermediates/stamps, rebuilt, and verified AOT/engine presence and ELF architectures. The build script now rejects an incomplete APK before copying it to `dist`. Only the hash above is deliverable; the failed artifact is preserved as diagnostic evidence only.
- Google Play services initially failed a UI-module download with `ERR_SSL_PROTOCOL_ERROR`. Normal retry and official module updates resolved it; the actual Google camera and gallery scanner then worked. No TLS bypass or third-party Google binary used. First use still requires internet and compatible Google Play services.
- The scripted `mlkit-ui` run was interrupted by an emulator System UI ANR during startup. It is **not** recorded as a passing suite. The final APK's actual Save/read/share/back flow was subsequently verified by direct emulator UI operations, including the three-page Google output above.
- These synthetic checks do not establish real-camera crop accuracy or unlimited zoom fidelity. Google left a narrow background edge on some gallery fixtures; the virtual room also partly obscures the wall poster with furniture, so that camera exercise proves automatic triggering/return, not perfect crop accuracy. Text remains raster content and cannot gain details missing from the source image. Portrait, landscape and card proportions were retained in the saved PDF.
- No installation on the user's phone, no original-app data migration, no Play Console mutation, and no GitHub publication in this trial. The public privacy policy and Play declarations still belong to the original release; a later public ML Kit release requires reviewing them against the trial's Google SDK disclosure.

All runtime/artifact evidence above is relative to `../../flutter/oto_scan/evidence/mlkit-trial/`.
