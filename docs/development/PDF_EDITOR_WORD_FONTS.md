# Word-exported PDF text and local fonts — 21 September 2026

User-authorized local editor increment; not a phase completion or release approval. Baseline `84cbb0f538020003bcfb6583e94c2fcf6524f87c`, existing dirty worktree preserved. No dependency downloads, PDFium integration, font installation/redistribution, source overwrites, or canonical phase/status changes.

## Implemented

- Original text reader handles `TJ` string/kerning arrays as editable runs, in addition to `Tj`. Tracks advance and small character spacing; admits Latin/punctuation mappings needed by the provided Word-exported sample. Independent following text positioning is still mandatory.
- Default subset-font replacement rewrites the original operand. Untouched TJ arrays and kerning are retained exactly.
- Explicit local-font choice uses a bounded allowlist of installed TrueType font files. Reads OS/2 embedding flags and rejects restrictive, preview-only, no-subsetting and bitmap-only cases. No silent font substitution. Regular/bold/italic faces are separate choices.
- PDFsharp generates the embedded Unicode font resource and encoded operand. Only font/text operators are transplanted at the original text position; source font and character spacing are restored afterward. Scratch drawing streams are discarded. Original color, clipping, transforms and stacking are retained, rather than painting a rectangle over text.
- Original-editor inspector has a font picker and matching-font suggestion. Optional fit-to-original-area shrinks font when needed, with a 6-point physical-size floor. Default oversize replacement remains rejected.
- Saved output reopens internally; local font names remain matchable after PDFsharp changes the font naming convention. Existing added-text gestures are unchanged.

## Evidence

- Build: MSBuild Debug PASS, `artifacts/desktop-editor-word-fonts/Pertamina.Oto.Desktop.App.exe`.
- Worker editor: 63 PASS on final build; synthetic fixtures `%TEMP%/oto-editor-test-dce54926a45045b99d102b627af2422c`. Includes existing 35 checks, TJ preservation, new glyphs across 19 installed faces, invalid/missing font and overflow rejection, fit and unreadable-size rejection.
- Worker general PDF regression: 41 PASS; `%TEMP%/oto-functional-test-3aca64a9ec76447abad70e95708b46f9`.
- Final native UI audit: 95 PASS, `artifacts/desktop-editor-word-fonts/live-editor-audit-5b5af11dd0d343c59d12de4375773603/audit.txt`. Includes actual inspector font selection, apply, preview, save and reopen. Screenshots inspected at 1600x1000; existing compact checks retained.
- An earlier UI run failed its existing initial textbox-focus assertion (`live-editor-audit-d80a2c215c7a49988807483eca0510fa`). Unchanged rerun passed, and final run passed. No assertion was removed. Physical focus/UAT remains a residual risk, not a proven fix.
- Provided sample: reader increased from 0 to all 36 TJ runs. `eng/Verify-TjLocalFont.py` tested one selected run with new characters using matching Times New Roman Bold on a private temporary copy. pypdf/pdfplumber/Poppler independently verified old text removed, replacement font/size/position, pixel-identical area outside target, and unchanged source hash. No source text or sample-derived PDF is committed as evidence.
- Independently checked fit result: 30 repeated characters remain within original width at a smaller font of at least 6 pt; rendered fixture inspected.

## Remaining scope / limitations

- Not “all fonts/all PDFs”. Current machine exposes 19 allowlisted faces: Arial, Times New Roman, Calibri, Verdana (four styles each), and Cambria bold/italic/bold-italic. Aptos files are not found in supported local directories; Cambria regular is TTC and intentionally omitted. CFF/TTC/variable font support remains unimplemented.
- Local replacement input remains one line of basic Latin characters, up to 200 characters. Source decoding admits a broader Latin/punctuation range, but complex scripts/shaping are not supported.
- Text is edited per PDF run, not grouped paragraphs. Automatic line wrapping, enlarging original text regions, original-text direct inline editing/font-size shortcuts and paragraph reflow are not implemented in this increment. Added-text double/triple-click remains a separate capability.
- Scan/outline text, tagged content, form XObjects, unsupported encodings/transforms, forms/signatures/encryption retain prior guards.
- No physical mouse/keyboard/touch injection, screen-reader acceptance, or network-denial test performed. Worker uses local installed fonts and existing local processing only; no full UI/UX or production acceptance is asserted.

Manual path: Edit PDF → open source → Edit teks asli → select outlined run → choose suggested local font when new glyphs are needed → enter replacement → optionally enable fit → Terapkan teks → Simpan PDF baru.
