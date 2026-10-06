# Edit PDF functional checkpoint — 21 September 2026

Later update: [PDF_EDITOR_TEXT_AND_OBJECTS.md](PDF_EDITOR_TEXT_AND_OBJECTS.md) adds verified original-text replacement for supported fonts plus resize handles and keyboard object controls. The limitations below describe the earlier checkpoint; consult the new record for current support and evidence.

Status: functional local development slice, not full original-text editing or release acceptance. No new package or PDFium integration.

## Implemented

- Catalog opens native editor; local PDF picker, workspace selection, Ctrl+O, Ctrl+V and drop inputs.
- Real document canvas, neighboring page thumbnails, page navigation, zoom/fit and native pinch surface.
- Added Arial text, PNG/JPG images, rectangles, ellipses, lines and freehand marks.
- Object selection by pointer or accessible dropdown, position/size/color properties, duplicate/delete, undo/redo.
- Draft retained across navigation; replacement/close guards; explicit document close releases input locks.
- Existing isolated worker exports to a unique new PDF through atomic staging; cancellation, source protection, local history and internal result preview.
- Compact inspector flyout and corrected compact sidebar footer.

## Verification

Final build: `artifacts/desktop-editor-functional/Pertamina.Oto.Desktop.App.exe`.
Desktop DLL SHA256: `E5EF9D305611041272DAF980A65A4F263BED8AEC0662AAD4F8CED6E23C2F237A`.

- MSBuild Debug: PASS; diff whitespace check: PASS.
- Worker editor tests: 15 PASS, including unsupported input/geometry/glyph rejection and source preservation. Fixture evidence: `%TEMP%/oto-editor-test-7d44981e8f154e0088f95c779f53f146`.
- Existing worker regression: 41 PASS; evidence `%TEMP%/oto-functional-test-856ea060608a4613be29f438ec0ea737`.
- Native editor audit: 19 PASS; `artifacts/desktop-editor-functional/live-editor-audit-1a49ac1f94924946bced937f2bf21db1/audit.txt`. Includes real PDF/image load, property edit, undo/redo, navigation draft retention, save/re-save, cancellation, source hash, internal reopen and release checks.
- Navigation regression: 15 PASS in `artifacts/desktop-editor-functional/flow-selftest.txt`.
- Library regression: 4 PASS in `artifacts/desktop-editor-functional/library-selftest.txt`.
- Independent Poppler/pypdf/pdfplumber roundtrip: PASS in `artifacts/desktop-editor-functional/editor-independent-qa/report.json`. Added text is extractable at expected coordinates; original text retained; unchanged page and source header pixel-identical. Export engine unchanged since this check.
- Desktop/compact and before/after canvas captures inspected; independent rendered result inspected. Tests use synthetic documents, not user content.

## Limits and remaining acceptance

- Original-text replacement remains disabled; prior PDFium admission and embedded/subset glyph roundtrip gates remain unresolved. Added text is not replacement of existing text.
- Added text: single-line basic Latin ASCII, Arial, maximum 200 characters. Font families/styles and arbitrary Unicode are not implemented.
- Encrypted PDFs, forms/signatures, rotation, cropped/nonzero-origin pages and nonstandard UserUnit are rejected before editing rather than rendered/exported with uncertain geometry.
- Physical pinch/touch, manual mouse dragging, full keyboard/screen-reader acceptance and network-denied operation have not been manually certified. PNG roundtrip is verified; image EXIF orientation coverage remains pending.
- UI assertions and visual checks are development evidence, not full UI/UX approval, production readiness or phase completion.

Main implementation: `LivePdfEditor.cs`, `LiveEditorAudit.cs`, worker `EditorMarks.cs` and `EditorTests.cs`. Independent QA helper: `eng/Verify-EditorRoundtrip.py`.
