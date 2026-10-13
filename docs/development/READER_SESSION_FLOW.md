# Pembaca PDF — session flow, 2 October 2026

Authorized local application improvement; no phase gate or Production approval change.

Ownership: ReaderWorkspace.cs, ReaderSession.cs, ReaderUiAudit.cs, UnifiedResult.cs; MainWindow.cs only compact layout condition; LivePdfEditor.cs only removal of forced sidebar collapse. Existing editor scroll code and other dirty files preserved. No additional agents spawned.

Reader follows the user's sidebar preference, with explicit focus mode independent of that preference and automatic compact layout for small windows. Session picker identifies the active document and disambiguates matching names with their directories. One selected PDF opens when the reader is empty; ambiguous sessions require a selection; an already open manual document wins. Failed opens retain the existing document. Explicit result/Explorer activation bypasses automatic session selection.

Navigation retains document, page, native scroll offsets and zoom. PDF result rows have Baca hasil; session result identity uses Hasil pengolahan, source identity uses Sumber asli. Tools receive the current reader path. Returning to the same editor source reuses the editor and its draft instead of reopening it. Reader explains that the original is shown until edits are saved.

Verification: native --audit-reader-ui passed 78 checks, including search, 1000-page raster bounds, source hashes, session ambiguity/selection, focus/sidebar preference and scroll/zoom restoration. Evidence: artifacts/reader-flow/reader-ui-audit-61b3a30b1bd3421cb76a12d1a1f74757/audit.txt. Screenshots 12-session-flow.png and 03-reader-compact.png reviewed; a stale original-source notice on a processed result was corrected and audit repeated. SDK build requires host access outside the restricted sandbox. Shared workspace builds must be serialized with other delivery tasks.

Manual UAT remains for physical touchpad/mouse/pinch, file picker, drop/paste, password dialog, Save As, recycle confirmation, full-screen and installed-app update clicks. No user application forced closed, PDF source overwritten, user data modified or shortcut replaced. Local delivery is through eng/Build-LocalUpdate.ps1 and the existing releases/windows-local feed.

## Verified local delivery

Coordinated serial delivery with the editor task, after directly verifying the user's coordination permission. Its official Build-LocalUpdate.ps1 run completed successfully; no duplicate package was built. Feed version: 20261002-144435-ebec47cb, product 0.1.0, channel Pengembangan, 384 manifest entries. Exact output: artifacts/update-build-55e9f245a71f433aad31a0cab7c45743.

Reader audit on that exact output: 78 PASS, 0 FAIL, process exit 0; evidence reader-ui-audit-a03b20d15d004b0084c488703ca121c6/audit.txt beneath the output. Its 12-session-flow.png visually checked, with correct Hasil pengolahan identity and no stale source notice. Published payloads remain immutable; the audit wrote only to the build output.

Independent read-only verification matched every payload length/hash and the feed manifest hash. Manifest SHA256: 4068D87FBE78693C3846F5340883C312FCF3298B6259C4B0F904EF1C076C466E. Audited and packaged desktop DLL SHA256: 6432C0E1EB29F0428D8F2320193BD8EC854D61675FF242F89A0C4308D90D341C.

Official delivery checkpoint: worker 41, editor worker 102, search 25, Word 27, Word UI 39, quality 39, product version 25 plus package 8, distribution 62 plus updater 13, and PDF opener self-test passed. Editor task additionally reports 148 native editor checks and 15 native continuous-scroll checks. This document does not claim installed-user activation or completion of physical UAT.

Update through Pengaturan → Periksa update → Pasang update; save drafts and close normally, then reopen the same stable PERTAMINA OTO shortcut. Existing running version and user data were left intact.
