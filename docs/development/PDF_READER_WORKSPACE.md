# Pembaca PDF development implementation

**30 September 2026 follow-ups:** local content search is documented in `READER_SEARCH.md`, the Windows default-app setting in `PDF_DEFAULT_APP.md`, and continuous vertical page scrolling in `READER_CONTINUOUS_SCROLL.md`. The original no-search/no-association/single-page statements below describe the first reader build and are superseded by those scoped implementations. Remaining printing and manual-UAT limits still apply.

The user approved the reader mockup on 30 September 2026 and authorized implementation. This change implements the main native reading workspace, not the complete reader roadmap or a phase gate. Existing editor, organizer, history and local update delivery remain in place. No package or engine was added.

## Implemented behavior

- Dedicated Pembaca PDF navigation and a link from the existing history preview. Collapsed application sidebar, thumbnail rail, central gray canvas, file actions and fixed bottom navigation follow the approved composition using existing SVG icons.
- One local PDF through picker, Ctrl+O, global Ctrl+V or whole-reader drag and drop. No upload. Limits are 100 MB and 1000 pages. Password prompt is available; unsuccessful loads preserve the previous document.
- Native pinch zoom, zoom buttons, fit page, page input, previous and next, PageUp and PageDown, fullscreen and Escape. Rendering is single-page; thumbnails are loaded in groups of eight to bound work. Session bookmarks are limited to 100 and are not persisted or written into the PDF.
- Edit PDF and Atur halaman reuse existing tools. Reading does not change the source. The source remains read-locked until replacement, explicit removal or app close.
- Simpan salinan chooses a destination folder and generates a unique name. It copies exact bytes without overwriting. Buka lokasi opens the containing folder.
- Hapus file asks confirmation with Batal as default, then uses the Windows recycle operation. No explicit permanent-delete fallback is implemented. Actual Recycle Bin behavior and cancellation still require manual UAT with disposable files.
- Stale rendering is invalidated across navigation. Returning restores the current document and page. Image dimensions are bounded to 1800 pixels per axis for the main render and 220 for thumbnails; extreme aspect ratios are rejected.

## Remaining differences from the mockup

Search is visibly disabled because the current Windows PDF rendering API does not supply the general text extraction and selection implementation needed here. The original-text editor parser is intentionally restricted and is not misrepresented as a general document search engine. Printing is not implemented or shown as an active action. Windows PDF association, continuous multi-page scrolling, persistent reading position/bookmarks and text selection remain outside this implementation. The existing Windows title/menu bar is retained; the redundant mockup brand row is not duplicated.

## Verification

Full MSBuild Rebuild of the desktop csproj, Debug with DesktopUiOutputPath pointing to artifacts/desktop-reader-final, passed with exit 0. Native commands use that executable with --audit-reader-ui, --audit-history-ui unused, and --audit-live-editor with the existing synthetic editor fixture.

- Reader audit: 27 PASS, exit 0, artifacts/desktop-reader-final/reader-ui-audit-6ac6e215a72c4c92bc83e32485c9ea63/audit.txt. Covers empty state, native rendering, accessible button names, fit, zoom, navigation, retained document after bad input, session bookmarks, source preservation, editor handoff, exact unique copies, A3, A2 and A2 landscape.
- History regression: 35 PASS, exit 0, artifacts/desktop-reader-final/history-ui-audit/audit.txt.
- Editor regression: 139 PASS, exit 0, artifacts/desktop-reader-final/live-editor-audit-e05cf0b81b884d1f85bfdbb250cb984c/audit.txt.
- Reader desktop and compact screenshots inspected. The audit exposed and fixed cancellation of rendering during the WinUI unload/reload cycle. PDF page size assertions use aspect ratio because Windows sizes are DIPs, not PDF points.
- git diff --check passed. No new network calls, dependencies or external processing added. Network-denial execution was not performed.

Physical pinch, complete keyboard/screen-reader traversal, picker, clipboard/drop, password prompt, fullscreen, recycle confirmation and printer behavior are not certified by these programmatic checks. This is not human UAT or Production readiness. Development notes follow the repository documentation destination; nothing was saved to an external Page service.

## Delivery

Publish through eng/Build-LocalUpdate.ps1 only after verification. The running installed version is not overwritten or force-closed. User installation is through Pengaturan, Periksa update, Pasang update, followed by saving work and reopening the same desktop shortcut.

Published local development release: 20260930-130204-c6ca5caf, 360 manifest files. Build-LocalUpdate.ps1 exited 0, with worker 41 PASS and editor worker 63 PASS. Clean build output is artifacts/update-build-53453822834744aba0140fd0abb69b33. Publication does not mean the installed application has already applied this update.
