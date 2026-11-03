# Original text: audit-document corpus repair, 26 October 2026

User authorized repairing Edit Teks Asli for the two named audit PDFs. This is a scoped Windows desktop correction by Agent 0, not Phase 6 promotion, website work or a new provider admission. Existing dirty checkout `task/core-functional-first` at `84cbb0f538020003bcfb6583e94c2fcf6524f87c` is preserved.

## Cause and implementation

Both inputs have seven pages and ordinary horizontal text, without catalog tags, forms, encryption or signatures. The previous worker admitted only 196/449 and 221/506 text operations: it invalidated text position on a nonterminal `T*`, rejected `Tw`, and could not edit consecutive text fragments. The issue was not damaged PDF content or a lack of actual text.

The reader now keeps text and text-line origins separately, handles `TL`, `TD`, `Td` and `T*`, and counts word spacing only for a single-byte source code 32. Text advances do not move the line origin. For adjacent fragments, a `TJ` numerical adjustment retains the original endpoint after a shorter edit or deletion. Explicit local-font replacements restore the exact line matrix and endpoint rather than use rounded native font measurement for the following fragment. Numeric-only advances remain readable after the old font no longer paints any glyphs.

Original-font editing changes encoded text operands, retaining font resources, colors and source geometry. It does not paint a rectangle over text or rasterize the page. No dependency was added. Unsupported font/encoding, tags/inline content, rotations/skews, forms/signatures/encryption, stale edits, source overwrite, unavailable glyphs and overflow retain existing rejection paths. Source PDFs are never written.

Reference: Adobe PDF Reference, text matrices/positioning/showing operators, section 5.3 (equivalent ISO 32000-1 section 9.4), https://stuff.mit.edu/afs/sipb/contrib/doc/specs/software/adobe/pdf/PDFReference.pdf .

## Scope and source files

- Worker: `OriginalTextEditing.cs`, `OriginalLocalFontReplacement.cs`, `OriginalFontBoundaryTests.cs`.
- Debug-only native corpus audit: `OriginalCorpusAudit.cs` and dispatch in `Program.cs`; opt-in isolated test environment required.
- Existing product 0.3.0 release notes updated. No reader-header/button redesign, installation or online publication.

## Verification

Evidence is in the main workspace's `artifacts/original-text-repair-20261026/`.

- `baseline/inspection.json`: original 14-page admission counts and source hashes; no document text in diagnostic logs.
- `corpus-verified/verification.json`: **13,394 assertions PASS** on the final worker, including all **955/955** actual text operations, real replacements on every page, reopen, exact encoded-glyph prefixes, original font/resource programs, unchanged other page streams, and Poppler raster comparison. Pixels outside edited text regions remain identical. pypdf independently extracts changed text. This assertion count includes individual unchanged-operator checks; it is not a count of independent scenarios or devices.
- `replacement-cases/verification.json`: **163 assertions PASS**, distinct phrase replacement, second edit after save/reopen, neighbor geometry, independent extraction/raster checks, overflow/stale/unavailable-character refusal and original overwrite refusal on both real inputs.
- `local-font-pixels/verification.json`: **4 assertions PASS**, independent raster/extraction checks on a mixed-font synthetic fixture; the following fragment and line remain in place after local-font replacement.
- `native-c5b75276e09b43ffb042c3543baa0a32/audit.txt`: **37 native checks PASS**, all 14 page overlay counts, production double-click callback/focused input, real preview, undo/redo, overflow draft recovery, Save As to separate files, reopen and original source hashes. Screenshots visually reviewed. These are production callback/native-render tests, not physical mouse or accessibility acceptance.
- Source hashes remain `62cee15c80771e9e5736f30c7e5b34314d6f270ee065daa38a6c49ca0403a3bb` and `6c3d2b51cdddaa0fe3f007e67ccb9be6c9108cabdffc12b43fc069731510a658`.
- Exact worker DLL: `BC21E3D491C7EA4C6EC98755E0EE9C75B7E9584E21694F3118CF5EFACCF5B862`; native desktop DLL: `457379A82DAACA7E0AA25BB5B6DF8128955D3BC3662447B529AB8027A101A3D8`.

Earlier failed attempts remain in evidence: a diagnostic script name shadowed Python `inspect` and was renamed; independent verifier switched to available pypdf extraction because bundled pdftotext is absent; native-font measurement initially shifted a neighboring fragment 0.026pt, then exact PDF endpoint restoration fixed it. An initial build/test overlap caused copy retries; final immutable candidate checks used stable DLL hashes. A later mixed-font regression found the numeric-only font-state case and was fixed before final verification.

The first standard delivery attempt stopped at the native Word-result preview callback (`local-delivery.log`, `candidate/word-audit-01fad33787294bccace5282742496e2d/audit.txt`). No update was published by that attempt. A focused rerun and the subsequent full standard run each passed all 39 Word UI checks on the same desktop/worker hashes. The initial intermittent preview failure was not conclusively reproduced or diagnosed; it remains recorded, not relabeled as a passing attempt. The subsequent quality audit passed 39 checks; see `delivery-native-gates.json`.

## Limits

Editing remains per text fragment/line, not Word-style paragraph or table reflow. Existing 200-character/run and 100-change limits remain. Longer replacements must fit the existing region, or use the explicitly selected local-font fit control; glyph availability is enforced. Detection and tested editing of these two exact PDFs does not promise perfect editing of every PDF or arbitrary new layout. No new document upload, website release, UAT, offline network-denial experiment, multimonitor, screen-reader or Production claim.

## Delivery

The complete rerun of `eng/Build-LocalUpdate.ps1 -PreparedBuild` passed (`local-delivery-retry.log`). Verified local build **20261026-112441-ede2bdee**, product **0.3.0 · Pengembangan**, contains **1,366 files / 554,288,486 bytes**. Manifest SHA-256: `C2B49817ABE0550129E8FBDD316BB20DD49DFA78E52C594993227B9E67F85A13`. `Verify-Package.ps1` checked every payload length/hash, the exact tested desktop/worker assemblies, original PDF hashes, and unchanged installed pointer; see `package-verification.json`.

The installed selection remains **20261025-213630-60869e33**. Apply the new package through **Menu → Pengaturan → Update lokal → Pasang update**. No automatic installation or online publication occurred. Retain the intermittent Word-preview observation above as a development limitation.
