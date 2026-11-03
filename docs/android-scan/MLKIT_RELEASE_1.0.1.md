# OTO Scan 1.0.1 — Play candidate, 24 October 2026

## Authority and status

The owner explicitly requested release of the tested Google ML Kit version to the existing Play Store application. This authorizes preparing the main-package update and submitting it through the available Play track. It does not establish Google approval, Production eligibility, completion of closed testing, or project Phase 11 completion. Agent 0 worked alone; no phone interaction in this release task.

Status: **local signed candidate prepared and verified; not uploaded, submitted, or published**. Computer Use stopped because it could not determine the browser URL confidently enough to enforce policy. No subsequent browser input was issued. Do not substitute another browser-control mechanism to bypass that stop.

## Candidate

- Version 1.0.1, versionCode 19, package `com.niam.ayani.oto`, label OTO Scan, original icon. Check versionCode availability in Console before upload; 19 follows the last observed Play version 17 and trial version 18.
- Google ML Kit Document Scanner 16.0.0 is enabled for the main package using `OTO_GOOGLE_SCANNER`; the separate trial switch and package remain available. Explicit build script: `flutter/oto_scan/tool/Build-MLKitRelease.ps1`.
- Gallery, PDF reader, compressor, and existing private document storage remain OTO implementations. No data migration or clearing was added. Trial app storage stays separate from the main app.
- The same existing upload certificate signs both artifacts: SHA-256 `f067802bd24197cf872a5fa6ab4a073afc7f75c79af33df8c5e9ec4039c21b73`. This is the upload key, not a claim about the Google Play app-signing certificate.
- AAB and APK: `flutter/oto_scan/release-output/1.0.1/`. Immutable hashes and sizes are in `evidence/play-release-1.0.1/release-verification.json`.

## Verification and limits

- 98 Flutter tests passed with `--concurrency=1`; analyzer reported no issues. Logs: `evidence/mlkit-release-flutter-test-serial.log` and `evidence/mlkit-release-flutter-analyze.log`.
- Bundletool validation passed. 30 artifact checks passed: identity/version, target API 36, min API 29, expected permissions, QA disabled, private sharing provider, backup/cleartext disabled, original icon, licenses, no signing secrets/test classes, Flutter AOT/engine and correct ELF architecture for ARMv7/ARM64/x86_64, APK/AAB identical native and DEX contents, upload certificates/signatures, 16 KB native/ZIP alignment.
- The first parallel Flutter test attempt and first AAB build ran out of Windows memory. They are preserved as failures. Serial tests and a lower-memory Gradle build succeeded; Kotlin compilation strategy is propagated to included builds. No user applications were closed.
- Actual Google scan/crop/filter, synthetic portrait/landscape/card, Auto capture, Save/read/share chooser/back were verified on the preceding trial. See `MLKIT_TRIAL.md`. This main-package candidate has not been installed on an emulator or physical phone in this turn; do not label the previous trial's runtime tests as an exact-artifact run of this candidate. ARMv7 packaging is verified, not an ARMv7 device execution test.

## Privacy and listing work

The in-app main-version policy now describes on-device document processing and Google's SDK technical metrics. Local publication drafts, Indonesian release notes and updated store descriptions are in `release-output/1.0.1/`. They have **not** been published to the VPS or saved to Play Console. The currently published 1.0.0 policy and its no-analytics declaration must not be used unchanged for this Google SDK release.

Review the Google documentation and map the SDK metrics to Play's current form before submission: device/installation identifiers; app interactions; diagnostics and performance (configuration, processing time, formats/resolutions/sizes, versions, events and error codes). Document contents are processed on device according to Google. No app-owned upload, account or advertising was introduced. Do not mark data as ephemeral or optional merely because document processing is local. Google documents HTTPS for collected SDK data. Confirm the current form's sharing definition and applicable SDK guidance rather than assuming that no document upload means no data collection.

Sources: https://developers.google.com/ml-kit/android-data-disclosure ; https://developers.google.com/ml-kit/terms ; https://developers.google.com/ml-kit/vision/doc-scanner/android ; https://support.google.com/googleplay/android-developer/answer/10787469 . The existing dependency provenance/license/OSV audit remains in `evidence/mlkit-trial/dependencies/`; it is not a guarantee of no vulnerabilities.

## Resume in a later browser session

1. Open the actual OTO Scan app page in Google Play Console. Re-read visible Production eligibility, existing releases, version codes, and pending changes. Do not treat the last observation as current.
2. Publish the prepared privacy update at `https://oto.ayani.id/oto-scan/kebijakan-privasi` using the existing VPS deployment workflow; preserve the existing OTO site and verify the deployed page and main site. Draft Razor source is in the release directory; it has not been copied into the web worktree or deployed.
3. Update Data Safety and store descriptions for Google SDK metrics. Review store screenshots for the changed scanner UI; replace any screenshot implying the old scanner is the current default. Reuse only accurate actual-emulator captures.
4. Upload the verified AAB to the appropriate existing track. If Production is unavailable, use the existing Alpha track and report that it remains closed testing. Review errors/warnings and submit eligible changes under the owner's authorization.
5. Record the actual Console result and artifact hash. Never claim public availability until Production is visibly available/published.

Last actual Console observation was **23 October 2026, 08:22 UTC**: Alpha 1.0.0 (17) available to selected testers; Production inactive; 7 opted-in testers; Apply for production disabled; at least 12 testers for 14 continuous days required. These counts and eligibility have not been reverified on 24 October. Developer 6264037910617880183, app 4972557040399357869, Alpha track 4700188009191063810. Existing read-only monitor remains unchanged.
