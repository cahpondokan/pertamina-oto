# Shared continuous read previews — 1 October 2026

Scope: approved standardization of reading previews, Agent 0 only, existing dirty `task/core-functional-first` worktree. Baseline commit `84cbb0f538020003bcfb6583e94c2fcf6524f87c`. This task is not phase execution or a phase/Production approval. No dependency, worker protocol, source document or installed binary is changed by this implementation.

## Coordination

Read root/worktree AGENTS.md and Shared Agent Contract before edits. Read-only snapshots confirmed the predecessor reader, quality and OCR chats were idle/completed before shared-source changes or builds. Reader handoff: READER_CONTINUOUS_SCROLL.md; quality: OUTPUT_QUALITY.md; OCR: SHARED_LOCAL_OCR.md, blocked on provider admission with no runtime/source changes. No subagents were used. Existing icon, reader/search/opener, quality, OCR admission documentation and other dirty changes are preserved. Central phase state/register/log were not changed.

## Actual entry points

| Entry | Implementation and behavior |
|---|---|
| Pembaca PDF / default PDF opener | Existing PdfReaderView, continuous surface in ReaderContinuousScroll.cs retained; search/tool/default-app actions remain available in full reader. |
| Riwayat, split and expanded preview | InternalPdfReader.cs embeds the same PdfReaderView in preview-only mode inside the existing heading/split shell. Library rows, filters, density, selected file and expansion remain independent of page scrolling. Latest concurrent document-open request wins. |
| Beranda with documents / enlarged preview | BuildDesignedHome routes nonempty input to BuildDocumentHome. Its PreviewPanel now embeds the shared reader; Perluas preview opens the same read-only component in ReadPreviewDialog.cs with the current page and decrypted in-memory document when available. Empty Beranda is unchanged. |
| Active tool source previews | SourceWorkspace.RefreshSourcePreview calls PreviewPanel; PDF read previews now scroll inside the selected source. Source file choice and processing selection remain separate. Password reload is retained. |
| Saved PDF results | UnifiedResult buttons, including individual PDFs in multi-result lists, open the shared read dialog. Older AddSavedResultPreview also uses PreviewPanel. ZIP and non-PDF opening behavior is retained. |
| Editor read mode | Baca sumber opens a read-only source dialog at the current page, preserving the editing canvas and draft. Lihat hasil and saved-result Preview hasil read the saved PDF without replacing the editing document/draft. |
| Page-card dialogs / JPG source read | Existing PreviewPanel callers reuse the reader for ordinary PDF reading. Rotated operation simulations and non-PDF images retain SinglePagePreviewPanel. |
| OCR / unavailable conversions | Not activated. Future admitted PDF source/result consumers can use the same factory; OCR provider admission remains independent. |

Only one continuous page-measurement, viewport-tracking, zoom, page-jump and bounded page-render algorithm exists: PdfReaderView / ReaderContinuousScroll.cs. The previous independent single-page Riwayat algorithm was removed. Preview-only mode hides file replacement/deletion/tool routing/full-screen actions while retaining page controls, active thumbnails on wider panels, and native pinch zoom.

## Resource, lifecycle and safety

Uses the existing local Windows PDF renderer and serial RenderGate. Lightweight page frames retain actual mixed-page geometry. Viewport pages plus one neighbor on each side share the existing 8-million requested raster-pixel budget and 1800-pixel maximum dimension. Thumbnail rendering is limited to eight-page groups; these allocations and parser/frame memory are additional to the raster budget. The budget is per reader, not a total process memory guarantee; an enlarged dialog can coexist with its underlying source preview.

Render generations discard stale completions. Permanent unload releases document frames, rasters and read locks. Temporary ContentDialog reparenting invalidates page rendering immediately but defers document release by 50 ms, retaining a pending document open if content reloads; lasting unload invalidates that open as well. Read dialogs initialize after Opened and WaitLoadedAsync, avoiding early attachment to an unloaded control. Closing the dialog explicitly disposes its reader. Passwords stay in view memory and are cleared from the input; unlocked documents are not added to the global thumbnail cache by this reader.

All reading uses local PDF/file APIs, read-only file locks, and existing limits of 100 MB / 1000 pages. No document upload or server operation is introduced. Source hash preservation is tested with synthetic PDFs.

## Limits stated in UI

Editor Baca sumber displays the original source; unsaved draft appearance is deliberately excluded and explained in the UI. Saving a new PDF and using Lihat hasil previews the applied changes. The editable canvas itself remains page-at-a-time, with its established coordinates, drawing gestures, object selection and undo/redo. Continuous editing would require a separate, higher-risk refactor and is not claimed complete here.

Page-card selection/ordering stays intact for Gabung/Pisah/Atur/Hapus/Ambil. Rotated operation simulations are not converted into document read previews. Images and ZIPs are not represented as continuous PDF documents. Physical mouse/touchpad/pinch, assistive-technology and UAT acceptance remain manual; native programmatic ScrollViewer tests are not physical input evidence. No network-disabled physical UAT or scanned 1000-page stress benchmark is claimed.

## Changed paths

Desktop: ReaderWorkspace.cs, ReaderContinuousScroll.cs, InternalPdfReader.cs, HistoryGallery.cs, DocumentWorkspace.cs, UnifiedResult.cs, LivePdfEditor.cs, EditorSaveScreens.cs, EditorInlineText.cs, EditorOriginalInline.cs, Program.cs; new ReadPreviewDialog.cs, EditorReadPreview.cs and ReadPreviewAudit.cs. Tests: ReaderUiAudit.cs, HistoryUiAudit.cs. This document is the coordination/handoff artifact. All other pre-existing dirty files remain outside this task's ownership.

## Verification checkpoint

Build: VS2026 MSBuild Desktop.App.csproj `/t:Build /p:Configuration=Debug /p:DesktopUiOutputPath=<worktree>/artifacts/preview-standard/ /v:minimal /nologo`, exit 0. Native isolated processes: `--audit-read-previews`, `--audit-history-ui synthetic`, `--audit-reader-ui`. Audits use synthetic PDFs and isolated history storage; the user's application is not force-closed. Intermediate compile/test failures were corrected without weakening assertions: obsolete openingReader reference, nullable PointerCaptures, early popup attachment/reparenting, and the audit's initial selector for an unreachable empty-home path.

Shared read audit passed (exit 0): actual enlarged Beranda, actual UnifiedResult button, source/result factory reuse, forward/backward page boundaries, active page/thumbnail, retained zoom, raster request budget, cancel/resume, permanent unload, editor draft coordinates/selection/undo/redo, and source SHA256. Evidence: artifacts/preview-standard/read-preview-audit/audit.txt. Native screenshots expanded-read-preview.png and tool-source-preview.png were visually inspected. History split/narrow screenshots were also inspected. Final regression and delivery results will be recorded below.

Additional draft protections: read dialogs suspend editor LostFocus auto-apply/end-input handling and restore the active native input and its selection after dialog focus settles. Tests cover added-text input and an uncommitted original-text draft from an actual worker-produced embedded-font PDF. Native TextChanged is awaited before taking the draft baseline. The handwritten Helvetica history fixture is readable/searchable but outside the original editor's admitted font encoding; it was not treated as proof of original text editing support. No editor font/worker capability was changed to make this fixture pass.

Current results: shared previews **72 PASS** (exit 0); history **48 PASS** (exit 0), including newest concurrent selection and unchanged split/list state; full reader **69 PASS** (exit 0), including A3/A2/mixed geometry, 1000-page raster budget, navigation cancellation, search/opener regressions and source hashes. Reader evidence: artifacts/preview-standard/reader-ui-audit-3f723f3a4ccb4810a221b8581988428a/audit.txt. History: artifacts/preview-standard/history-ui-audit/audit.txt. Boundary screenshot 08-continuous-page-boundary.png was visually inspected. A navigation restoration regression was fixed by resuming retained reader documents on the actual native Loaded event, in addition to existing navigation routing; assertions were preserved.

Audit privacy correction: setting an isolated library destination did not clear history already loaded into test-process memory. Two task-generated isolated-library.json artifacts contained 14 non-fixture metadata records each. Those records were removed without printing their paths or changing the actual user library. The audit now clears its in-memory favorites/history after selecting its isolated destination, before recording any test result. The records were never package payloads: the published build was copied before the post-package read audit ran. A final package rebuild is required to include the corrected debug audit harness.

Initial delivery checkpoint: local update 20261001-104511-4101e46f, 370 manifest files; SHA256 CB7C818E42C97021AA960149C3A6DEA8A06BE92CEA589F0B8C5E12D06EE903CE. Integrity 370/370 lengths/hashes verified, zero PDF/read-audit/quality-audit evidence payloads; tested build/package desktop DLL hashes match. Build-LocalUpdate.ps1 exit 0: distribution 62 plus local updater 13, worker 41, editor 63, search 25, quality audit and opener checks passed. The actual delivery build additionally passed the shared-read 72 checks. Build log: artifacts/preview-standard-package.txt; actual build: artifacts/update-build-a59cecd8024b465385696632b13873f5. This version is retained and will be superseded by the final synthetic-only audit-harness package below.

## Handoff / final delivery blocker

Preview implementation is verified; final corrected-audit-harness delivery is **BLOCKED_BY_SHARED_BUILD_OWNERSHIP**. During the final rebuild, a newly active separate About task added AboutDrawer.cs / AboutDrawerAudit.cs into the same project's implicit Compile glob. AboutDrawerAudit.cs compilation failed (FocusManager unresolved, lines 51/52/53/75). These files are outside this task's ownership and were not edited here. Log: artifacts/preview-standard-package-final.txt, exit 1. No new update was published by this failed attempt; latest.json still selects 20261001-104511-4101e46f. Its 370/370 package hashes passed and its actual build passed 72 preview checks. The final source-only change after those checks clears audit-process favorites/history; it does not change ordinary application read behavior.

Read-only About status: chat 01a0f58d-e94b-76a3-9da8-3770072b4713, title “Terapkan panel samping Tentang aplikasi”, active. Its commentary explicitly waits for this preview handoff before shared integration/build, although standalone .cs files were already picked up by our rebuild. To avoid a circular wait and contested obj/build output, this preview lane stops shared source/build writes and hands off now. The About owner can fix/verify its own files and then integrate/package the current worktree, including this completed preview implementation and the synthetic-only audit correction. Run --audit-read-previews on that exact build (72 PASS expected), confirm isolated-library.json Jobs contain only paths under its read-preview-audit fixture directory, then Build-LocalUpdate.ps1 / package integrity as appropriate. Do not exclude/delete or overwrite the About lane to force a preview-only package.

No phase gate, protected branch merge, Production deployment, shortcut, installed version, user document or actual user library was changed. Remaining manual scope is physical input/a11y/UAT, continuous editable canvas and unsaved draft appearance in read mode, inactive OCR/conversion admission. This handoff is not a claim that the final corrected package has been delivered.

Report-back blocker: send_message_to_thread to Agent 0 was rejected by automatic approval review. A read-only check retrieved the human's report-back preferences from the destination's history, but review also rejected a retry supported by those messages: tool-output authorization cannot establish permission in the current trusted transcript. No message was delivered and no indirect workaround was used. Direct user approval in this chat is required to send this handoff to Agent 0. The complete local handoff remains available here for the About owner/integrator to inspect read-only.

## Blocker resolution — 1 October 2026

The About integration completed and superseded the initial package with local development update **20261001-105807-9dbe7eb6**. ABOUT_DRAWER.md records a successful delivery build, 371/371 payload integrity, shared preview 72 PASS and zero unexpected audit metadata records. Manifest SHA256: D3F5CE5CA5597E8BD96A645E4C27CD491B29B1241AF91CFB2913F0B8446E4C4E. Final evidence: artifacts/about-package-integrity.txt and artifacts/about-drawer/read-preview-audit/audit.txt. The corrected synthetic-only harness is included. The shared-build blocker above is historical and resolved; installed-version activation remains a user action through Pengaturan → Periksa update → Pasang update, followed by reopening the stable shortcut after saving drafts.

The user explicitly authorized report-back in this chat. The final preview/integrated-package report was successfully sent to Agent 0 chat 01a07c8d-3c57-7963-8500-daed7ddb07ee. The report-back blocker is resolved. Manual UAT limitations and phase/Production boundaries above remain unchanged.
