# Original text reader repair - 2 October 2026

User requested strict implementation after a read-only audit reproduced zero editable runs on the synthetic four-page font PDF. This is a local development bug fix, not phase promotion, new provider admission, or public distribution approval.

Baseline: dirty `task/core-functional-first` at `84cbb0f538020003bcfb6583e94c2fcf6524f87c`; existing changes preserved. Input SHA256 `a4e475d5b5cd1601766b0702c73523e1c3a377139ced424a364e295a44201c3b`. Previous worker SHA256 `4d863e3b013d5fd8483e057d126bb9ecc40c9888f5bfa873fc4dce6db4d70ab8` returned zero runs on all four pages.

## Scope and task card

- Owner: Agent 0, no new agents/chats. Feature: existing original-text editing; no canonical phase/gate changes.
- Allowed: worker original text/font reader and tests, desktop empty-result explanation, bounded regression verifier and this development note.
- Do not touch: installed app/profile, original PDF, immutable portable candidate, web server, dependency admissions, unrelated features.
- Implement: embedded simple TrueType with explicit ToUnicode but absent Encoding; conservative standard Western Type1 fonts; harmless T-star immediately before ET; specific rejection explanations.
- Retain guards: ambiguous mappings, missing metrics, custom encodings, forms/signatures/encryption, tagged/inline-image content, rotated/skewed text, dependent positioning, overflow, stale edits, and source overwrite.
- Verify: build/analyzers, existing regressions, positive/negative font fixtures, original four-page PDF run discovery, real replacement/save/reopen, independent extraction/operator/pixel checks and source hashes.
- Delivery: only after verification, standard local update build. No install, forced restart, or public upload.
- Stop: unsafe serialization, changed source, unadmitted dependency, conflicting writer, failed checks; do not waive checks.

Synthetic diagnostic PDFs are test artifacts, not replacements for the user's original test file.

## Implementation

- Simple TrueType: an absent Encoding is admitted only with an embedded FontFile2 stream, explicit ToUnicode mapping, and valid per-code widths. The original codes/font resource remain unchanged; no WinAnsi entry is synthesized. Conflicting source-code mappings and ambiguous inverse glyph mappings are rejected. Encoding dictionaries remain outside the admitted subset and are rejected without an invalid name cast.
- Base fonts: the 12 Western standard Type1 faces are supported for printable ASCII with explicit WinAnsiEncoding. Descriptor/width/ToUnicode overrides, custom/subset names, Symbol and Dingbats are not assigned guessed standard metrics. No new runtime/font dependency is bundled. Advance metrics are numeric standard-font data cross-checked using the existing PDF test runtime. Reference: ISO 32000-1 sections 9.6.2.2, 9.6.6 and 9.10 (Adobe public PDF reference).
- Position: only a terminal T-star immediately followed by ET gains admission. Dependent text advances and unsupported rotations/skews remain excluded.
- Empty and partial results now explain font/mapping versus positioning exclusions, or whole-page unsupported content; they no longer identify valid text PDFs as scans by default.
- Inline typing now honors the visible preserve-PDF-font selection. Matching installed fonts remain explicit user choices rather than a silent default.

## Verification completed before local packaging

- Build/analyzers: worker and desktop build exit 0; no suppressed checks or warnings-as-errors changes.
- Editor worker: 102 PASS (existing 63 retained plus 39 boundary checks). Negative cases include custom/overridden Type1, missing encoding, encoding dictionary, dependent positioning, missing ToUnicode/program/widths, duplicate source mappings and duplicate Unicode mappings.
- Native editor regression: 139 PASS in `artifacts/desktop-font-repair/live-editor-audit-1bcb391b0dff48d1918b448982ea0141/audit.txt`.
- Exact original four-page fixture: 23/21/21/23 admitted runs, including all six base-font samples and all eight Windows-font samples. Rotated sample still explicitly excluded.
- Independent regression: 28 real edit/save/reopen cases, 344 assertions PASS in `artifacts/font-repair-roundtrip-final/results.json`. pypdf independently extracts replacement text; exactly one Tj operand changes, no overlay operators, untouched page streams/pixels unchanged, changed pixels confined to the target line, source SHA256 unchanged. Page-height serialization changes calculated Y by 0.0002 pt; tolerance is bounded to 0.001 pt with exact underlying position-operator comparison and raster verification retained.
- Actual native compatibility audit: 16 PASS in `artifacts/desktop-font-repair/font-ui-audit-86821f95d0b34734ab64c3bb76939e06/audit.txt`. Four-page overlays, double/triple-click event handlers and native focused TextBox, preserve-font choice, real preview, rejected overflow draft retention, Save As, internal reopen, source hash. Screenshots and representative Poppler renders visually reviewed.
- Tested worker SHA256: `BF9ADEAD712D278BA09533E45545D2F3A251F69F4EC44453C1919F968341FAA8`.
- Tested desktop DLL SHA256: `571E4C83EF6A6F9542D72EF692024E5B346735E4B2C7B72482F5849920DFD10D`.

Earlier diagnostic attempts are not PASS evidence: initial build required sandbox approval for compiler cache writes; a test fixture incorrectly used direct streams and was corrected to indirect streams; a new audit had an async Span compilation error and was corrected; the initial verifier expected byte-exact calculated page Y and was corrected to measured PDF serialization precision without relaxing operand/pixel checks. Final reruns above passed.

## Residual limits

Not all fonts, encodings, PDFs, or scripts. Embedded subset replacement can only reuse available glyphs unless the user explicitly chooses an allowed installed local font. Standard-font support here is ASCII/explicit WinAnsi only. Original text remains per run, not paragraph reflow; long replacement is rejected unless the existing explicit local-font fit option applies. Tagged/inline-image content, Form XObject text, unsupported transformations, OCR, forms/signatures/encryption retain existing guards. The friend's original TTE PDF has not been provided, so its exact cause/fix is unproven. No physical mouse/touch UAT, screen-reader acceptance, clean-PC or network-denial test is claimed.

No installed profile, website, frozen portable candidate, canonical phase state, or security setting changed. Product version remains 0.1.0 / Pengembangan per current explicit version policy; immutable build ID identifies this fix.

## Local delivery

Standard `eng/Build-LocalUpdate.ps1` completed exit 0. Local build ID **20261002-133640-8546a738**, 380/380 payload length/SHA256 checks PASS, manifest SHA256 `6F69B9499BF8D649CC0581AF525B6338A4E8E23B78907A0DAFF2093D51E27AE1`. Packaged desktop and worker hashes exactly match the UI/roundtrip-tested binaries above.

Delivery pipeline: product25 + legacy updater13, synthetic product package8, distribution62 + updater13, worker41, editor102, search25, Word27 + native39, quality39, PDF association helper self-test exit0. Full log `artifacts/font-repair-delivery.log`; final build `artifacts/update-build-2b0bbd3603ad49079d6feb07e044d43e`. Product-package test emits a separate five-file inert fixture message; that is not the application release.

Installed activation is NOT performed. User applies through Pengaturan > Periksa update > Pasang update, saves work and reopens the existing shortcut. Local publication is not website deployment and does not update the friend's laptop automatically. Prior installed data and immutable portable/public packages remain untouched.
