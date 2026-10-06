# Reader verification and local text search — 30 September 2026

User authorized both reader verification/fixes and PDF content search in the continuation chat. Work remains local development on `task/core-functional-first` at `84cbb0f` plus preserved prior uncommitted work. Agent 0 only. Task card: workspace-root `docs/project-management/READER_SEARCH_TASK_20260930.md`.

## Behavior

- Reader search field, Enter/Cari, contextual Ctrl+F, count/coverage, clickable page/snippet list, emphasized query text within snippets, previous/next result with wrap, cancel and close results.
- PDFsharp's existing content parser runs in the existing worker child process. Search does not use editor-only editable-run extraction. No new package, PDFium product admission, OCR, service, or upload.
- Literal case-insensitive matching; whitespace normalized; reads Tj, TJ, single/double quote text operators, supported simple Standard/WinAnsi fonts and fixed one/two-byte ToUnicode maps. Handles bfchar, bfrange scalar/array, Unicode surrogate pairs and multi-character mappings. Drawing order is used, not guaranteed visual reading order.
- Up to 100 result snippets, with explicit truncation; 100 MB / 1000-page input limit. Existing worker cancellation, two-minute timeout and sampled 1 GiB working-set guard apply. These are not an OS sandbox or a hard preallocation guarantee.
- Query/snippets exist in memory and the anonymous worker pipe only; no search index or plaintext document cache is written. Search never saves the PDF. The input stays read-locked. Logs contain exception type/stack only, not query/document content/passwords.
- Changing query, opening a different document, navigation or closing cancels/invalidates pending work. Failed document open preserves the prior document and completed results. Search can be repeated after cancellation.

## Fixes and limits

Ctrl+F previously navigated to the tool catalog even in the reader; it now focuses PDF search there. Native testing also exposed a delayed WinUI TextChanged event that could clear a newly started search. The handler now compares the actual query before clearing. In-flight cancellation tests cover query edits, page navigation away and replacement.

No OCR, protected-PDF search, Form XObject/annotation text extraction, custom encoding Differences, variable-width CMaps, or visual reading-order reconstruction. These produce partial/no-extracted-text notices rather than an assertion that the PDF has no match. A match opens its page; glyph boxes are not highlighted on the PDF canvas. In-snippet emphasis is available. Printing remains unimplemented. The subsequent user-requested Windows association feature is documented separately in [PDF_DEFAULT_APP.md](PDF_DEFAULT_APP.md).

Reference: [PDFsharp extraction FAQ](https://docs.pdfsharp.net/PDFsharp/Overview/FAQ.html#can-i-use-pdfsharp-to-extract-text-from-pdf) explains the low-level drawing-order extraction boundary; no higher-level text API is assumed.

## Verification

All fixture documents are synthetic. Final targeted Debug Rebuild exit 0:

`D:/DevTools/VS2026Community/MSBuild/Current/Bin/MSBuild.exe src/Hosts/Pertamina.Oto.Desktop.App/Pertamina.Oto.Desktop.App.csproj /t:Rebuild /p:Configuration=Debug /p:DesktopUiOutputPath=<worktree>/artifacts/desktop-reader-search/ /v:minimal /nologo`

- Worker `--self-test-search`: 25 PASS, exit 0. Includes literal/case/whitespace matching, WinAnsi, Unicode/ligatures, unknown encodings and gaps, malformed text maps, scans/forms disclosure, exact/exceeded result cap, invalid queries, encrypted/corrupt inputs and source/output non-mutation.
- Worker `--self-test`: 41 PASS, exit 0; `--self-test-editor`: 63 PASS, exit 0.
- Native `--audit-reader-ui`: 41 PASS, exit 0; `artifacts/desktop-reader-search/reader-ui-audit-c293d56c2afb41cea82fa80f64b4b21b/audit.txt`.
- Native `--audit-history-ui unused`: 35 PASS, exit 0; `artifacts/desktop-reader-search/history-ui-audit/audit.txt`.
- Native `--audit-live-editor <synthetic-source.pdf>`: 139 PASS, exit 0; `artifacts/desktop-reader-search/live-editor-audit-532ddf38456947a7ba05dfca23cc02e2/audit.txt`.
- Worker reports/native exit codes: `artifacts/reader-search-verification/`.
- Desktop and compact search screenshots inspected; final captures at the reader audit folder above. Reader regression also exercises A3/A2 portrait/landscape, source hash, unique copies, failed opens, bookmarks, zoom and editor handoff.
- Initial shell launches inside the restricted desktop context did not produce new audit reports. Actual native verification ran successfully in the normal local desktop context. Historical baseline reports are not represented as newly executed baseline results.
- Physical gestures, complete screen-reader traversal, printer/picker/recycle UAT and network-denial execution were not performed. Code uses local files and pipes only; no claim of network-blocked runtime proof.

## Delivery

`eng/Build-LocalUpdate.ps1` now gates publication on the search suite in addition to existing worker/editor regressions. It creates a clean build and a new immutable local development package. Installed/running application is not overwritten or force-closed. User applies via Pengaturan > Periksa update > Pasang update and reopens the stable shortcut after saving work.

Published after the user's explicit current-chat **Setujui update lokal** approval: `20260930-135728-08b297b4`, 360 manifest files. `Build-LocalUpdate.ps1` exit 0; its clean rebuild plus worker41/editor63/search25 regressions passed. Package build evidence: `artifacts/reader-search-verification/package-build.txt`.

Read-only package verification exit 0: manifest SHA-256 `D478C2026D4DC26B179407AD2EDB9091B5F345BB2E7D2410258D53103DA863E1` matches `latest.json`; all 360 payload files match length and SHA-256, zero failures. Published desktop/worker DLLs are byte-identical to the native-audited build. `previous-feed.json` retains `20260930-130204-c6ca5caf`. The installed/running application was not changed; the user still applies the update in Settings. The initial automatic-review rejection was resolved by explicit current-chat consent. Phase statuses/gates and Production approval remain unchanged.
