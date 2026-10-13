# PDF editor design and implementation gate

Date: 2026-09-18. Scope: user-approved native editor design, not a phase completion or production approval.

## Baseline findings

- Worker uses existing PDFsharp 6.2.4 (Directory.Packages.props and worker csproj).
- PdfOperations.Execute does not dispatch an edit operation; unknown operations explicitly fail.
- PdfRequest has no editable-object model, text-run identities, font mapping, or edit transaction protocol.
- InternalPdfReader uses Windows.Data.Pdf rendering into images, not editable text objects.
- ToolCatalog edit entry remains unavailable. Existing dirty files and user library were left unchanged.
- Official PDFsharp FAQ states that text extraction and content manipulation require low-level implementation, without coordinate helpers: https://docs.pdfsharp.net/PDFsharp/Overview/FAQ.html

This does not mean original text editing is impossible. It means the current engine integration cannot supply the promised editing interactions without substantial low-level engine development or an admitted additional engine. No white rectangles, raster replacement, or added text overlays may be presented as original-text editing.

## Approved visual contract

Reference: generated concept exec-a2dee2cb-aef3-4576-917a-90417f5d1c3b.png, shown and approved in the task conversation.

- Compact navy navigation rail; existing branding, no replacement logo.
- Header: back, Edit PDF, filename, honest dirty state, offline state, Save new PDF primary action.
- Toolbar: Select, Edit original text, Add text, Image, Freehand, Shapes, Undo, Redo. Existing Lucide asset family, no emoji replacements. Use an image icon for Image (the concept's pencil is semantically incorrect).
- Left page thumbnails, large center document canvas, contextual properties on right.
- Footer: page navigation, zoom, fit, pinch hint. Preserve document focal point during zoom.
- At smaller sizes collapse properties into an explicitly accessible drawer; do not squeeze the document to unusable width. Toolbar may overflow into a labelled menu.
- Properties show only supported controls. Do not imply a font, style, or text run is editable before inspection.

## Interaction contract

| Mode | Properties and behavior |
|---|---|
| Select | Select added object, move, size, rotate, duplicate, delete; maintain keyboard equivalents |
| Original text | Hit-test supported source text, edit contents, expose only supported font/size/color/styles; reject unsupported encoding or font substitutions with explanation |
| Add text | Click page to create text, font, size, weight/style, color, alignment, line spacing |
| Image | Local image picker; cancellation does not mutate document; position, aspect-locked size, rotation, opacity |
| Freehand | Color, width, opacity; individual stroke selection/removal |
| Shapes | Line, arrow, rectangle, ellipse; stroke color/width, optional fill |
| Undo/Redo | Transactional edits across pages; new edit clears redo; no source mutations |
| Save new PDF | Unique name and clear destination; atomic validated output; failure retains editor draft, no false success |

Unsigned sample documents only in automated tests. Signed input requires an explicit warning that editing can invalidate its signatures. Scanned pages must not be labelled editable source text. OCR, redaction, and existing-image manipulation are not silently added to scope.

## Required audit gates — NOT RUN / NOT PASSED

1. Native screenshots for each toolbar mode, no selection, selected object, original-text unsupported, empty input, busy, save success and failure.
2. Compare layout against reference at desktop and narrow widths; no overlapping/clipped controls, no duplicate outer scroll, visible primary action.
3. Verify names/tooltips, keyboard focus and traversal, focus return, meaningful disabled states, 44 DIP action targets, contrast and non-color selection cues.
4. Real PDF round trip: added objects match preview coordinates at multiple zooms, page rotations and crop boxes; font/glyph behavior verified.
5. Original-text edit proof: old text actually replaced in content, new text extracted correctly, unaffected page content unchanged. Overlay-only output fails this gate.
6. Undo/redo sequences, page changes, document replacement, cancel, unsaved navigation and close protection.
7. Existing outputs never overwritten; source hashes unchanged; crash/cancel cannot publish partial output; user documents absent from logs.
8. Resource tests with repeated open/close, bounded rendering and large documents; PDF processing remains in isolated worker.
9. Regression: page tools, Favorites, History, paste, output naming and persistence.

## Decision required

Select an implementation route before committing to the full original-text editor:

- Evaluate an additional local engine, starting with read-only technical/license/security comparison; no package acquisition, purchase or integration without the required admission.
- Or explicitly scope a first milestone to annotation/additional objects with original-text editing visibly unavailable, using the current engine.

Current outcome: technical preflight and design/audit contract only. No editor code activated, no dependencies installed, no UI/UX pass claimed. User approval of a picture is not evidence of functional editor readiness.
