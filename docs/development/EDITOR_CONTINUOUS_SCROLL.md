# Continuous scrolling in Edit PDF

2 October 2026. Local development change; phase gates unchanged.

Edit PDF now uses a document Canvas containing lightweight page-sized Canvas slots, with a 24-point gap and centered mixed-width pages. Wheel, touchpad and scrollbar movement uses native ScrollViewer scrolling. Its ViewChanged handler coalesces movement, selects the page near the top of the viewport, updates the page number and nearby thumbnails, and loads nearby page bitmaps. Buttons, thumbnails and NumberBox navigate to page offsets without refitting zoom. Fit applies to the active page.

Only the active page routes object editing. Pointer coordinates remain relative to that page's Canvas, with authoritative worker point dimensions. Inactive pages show added-object drafts but disable their object handlers. Switching commits supported original inline text through the existing commit path; rejected input stays on the previous page. Added text ends inline editing without dropping its model. Gestures defer scroll-driven activation until finished. Undo/redo refreshes inactive added-object visuals. Rebuilding the original-text preview invalidates all cached page backgrounds.

No full-document bitmap render: retain at most 12 page backgrounds around the viewport, release backgrounds outside that range, and prioritize the active page. Active bitmap requests cap both dimensions at 1800 and about 2 million pixels. Neighbor requests cap both dimensions at 1200 with a shared 6 million pixel request budget. Cached bitmaps may have been rendered under a previous range; the 12-image bound remains. Five nearby thumbnails remain the existing navigation model. At extreme zoom-out with very short pages, additional visible slots can remain blank until scrolled into the render window.

## Verification

- Build passed with local VS2026 MSBuild/Windows SDK; no new dependency.
- Provided 71-page font-detail fixture: 13 native scroll checks passed, including saved second-page text and unchanged source SHA-256.
- Synthetic mixed portrait/landscape with final 40 x 2250-point page: 15 checks passed. Evidence: `src/Hosts/Pertamina.Oto.Desktop.App/artifacts/editor-scroll-build/editor-scroll-audit-7aa15ba372dd4d25a394515f3fc7fc53/audit.txt`.
- Final synthetic 1000-page mixed/extreme fixture: 15 checks passed, including rapid navigation, NumberBox, zoom, bounded bitmap dimensions/count, draft ownership, undo/redo, separate PDF save and unchanged source. Evidence: `artifacts/editor-scroll-final/editor-scroll-audit-3f29b33d586c42b5853abc5027f0b60e/audit.txt`.
- Final full native editor regression: 148 PASS. Evidence: `artifacts/editor-scroll-final/live-editor-audit-77e1a5a17154400f9e0aa9149a006620/audit.txt`. Covers original inline drafts, rejected input preservation, font replacement, Save commit, object/shape/freehand/text gestures, images and source safety. Additional checks prove scrolling commits the original inline draft on its source page without jumping back and preserves zoom. Screenshot `continuous-scroll-draft.png` visually inspected from this final audit.
- Worker editor regression: 102 PASS.

Native UI audits run dedicated instances and use ChangeView, exercising the same ViewChanged pipeline as native scrolling. Physical wheel/touchpad/touch/pinch gestures and manual pointer dragging were not performed. No installed running application or user document was closed, no source PDF overwritten, no shortcut created, and no network/Production deployment performed.

Application project excludes `artifacts/**` from default content discovery, preventing nested test screenshot copies and Windows path-limit failures. Earlier delivery attempts stopped at incomplete concurrent Reader/startup edits. Those task owners confirmed stable sources and deferred their publication; no concurrent code was reverted. Local delivery evidence is recorded in `artifacts/editor-scroll-delivery-final.log`. Use Pengaturan > Periksa update > Pasang update; save drafts and reopen the stable shortcut to use the updated version.

Changed paths owned by this task: `LivePdfEditor.cs`, `EditorContinuousScroll.cs`, `EditorContinuousScrollAudit.cs`, `EditorOriginalText.cs`, `LiveEditorAudit.cs`, the `--audit-editor-scroll` branch in `Program.cs`, the artifact exclusion in the desktop `.csproj`, and this note. Concurrent sidebar/Reader/startup edits remain owned by their tasks. No commit or phase-status update was made.

## Local delivery completed

`eng/Build-LocalUpdate.ps1` completed with exit 0. Published build ID **20261002-144435-ebec47cb**, product **0.1.0 · Pengembangan**, 384 files, in workspace-root `releases/windows-local`. Installed application was not activated or overwritten.

Delivery gates: worker 41, editor 102, search 25, Word 27, Word UI 39, output-quality UI 39, product version 25, synthetic product package 8, distribution 62 and local updater 13; PDF opener self-test also passed. Log: `artifacts/editor-scroll-delivery-final.log`.

Manifest SHA-256: `4068D87FBE78693C3846F5340883C312FCF3298B6259C4B0F904EF1C076C466E`. All 384 payload lengths/hashes verified. Exact build/package desktop DLL SHA-256: `6432C0E1EB29F0428D8F2320193BD8EC854D61675FF242F89A0C4308D90D341C`.

Exact delivered build (`artifacts/update-build-55e9f245a71f433aad31a0cab7c45743`) also passed all 15 native scroll checks with the 1000-page mixed/extreme fixture; evidence `editor-scroll-audit-d32f0d768d9a42c6a1e3c128dd699218/audit.txt` under that build directory. Reader and startup task owners received the publication checkpoint under the user's explicit coordination authorization and may produce later builds containing this editor change.
