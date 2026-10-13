# Saved PDF result viewport

## Inline details refinement — 2026-10-11

User approved the inline-details mockup. `SavedResultViewport.cs` now places the original copy-summary command and selectable full file path in a bordered detail card beneath the main actions. It removes the Detail button/flyout. `PrintResultPanel.cs` aligns file information to the top. Below 650 DIP content width, the folder-only tile and path caption collapse because the full path remains accessible in the inline detail card. Long paths use ellipsis with full tooltip and selectable text; copy summary retains original metadata plus full path. Existing PDF/print/reveal/back/close handlers are unchanged.

Updated native audit: **143 PASS**, exit 0, evidence `saved-result-fit/inline-audit` under the C evidence root below. Includes 660×810, 1000×850, 1400×1000 outer windows, maximized, long file path/name, copy-summary success notice, missing result, reader, safe print routing, navigation, unchanged synthetic source/result hashes, and zero page scrolling. Normal/minimum screenshots were visually inspected. Standard packaging log: `saved-result-fit/inline-update.log`. This is the current layout; the flyout descriptions below are historical.

Standard delivery completed successfully: product **0.3.0 · Pengembangan**, immutable local build **20261011-160223-d2434548**, manifest **899BF8DBEE5AA0F92E4623EF3A58A8D6C79F9AB1FB64D2BB6157618D77B79027**. All 1,333 payload files and manifest verified, zero failures, exact audited desktop assembly matches published assembly. Installed pointer remains **20261011-155226-731a8109**, manifest **93C55B4CCFDC43D702A0FC5A0CDA2B617DEFE153740D100B450668A9BBF9A784**; no installed activation occurred in this task. Local update is available through the existing app flow. No online publishing, dependency additions, product version bump, or phase status changes.

## Approved modern redesign — 2026-10-11

The user approved the image concept and requested implementation. The standalone saved-PDF screen now has a circular green success mark, a navy heading, navigation actions at the upper right, PDF badge, separate page-count/size chips, a bordered storage-location card, and action icons. The preview/info proportions are 46:54 on wide windows; narrow windows retain 40:60 and place upper-right navigation on a second header row. The former bottom action row and duplicate safety footer are removed. Existing result handlers remain unchanged. Initial native redesign audit passed 141 checks; evidence is `saved-result-fit/redesign-audit` under the C evidence root below. Final package verification and delivery are recorded at the end of this document.

Implemented 2026-10-11 for the requested standalone single-PDF saved-result page. This is a scoped development UI change, not a phase completion or production approval.

The result card follows the available viewport height. The preview scales within the remaining space, while the title and six actions remain visible. Duplicate metadata and the permanent print explanation are removed; print guidance remains in its tooltip. Long names use two-line ellipsis with full text available through the tooltip and details. Details open in a bounded native flyout with internal scrolling when necessary. Empty notices take no space; errors remain visible. Embedded result panels and multiple-output results retain their existing layout.

## Changed source

- `UnifiedResult.cs`: activate fitting only for standalone single-PDF results.
- `PrintResultPanel.cs`: constrained preview and compact result information, preserving existing PDF/print behavior.
- `SavedResultViewport.cs`: viewport sizing, action row, bounded details flyout, and event cleanup.
- `SavedResultViewportAudit.cs`, `Program.cs`: DEBUG-only native acceptance entry point.

## Verification

MSBuild Debug compilation succeeded with the existing admitted dependencies. Native `--audit-saved-result-fit` completed with exit 0 and 141 PASS checks. Tested outer windows: 660×810, 1000×850, 1400×1000 pixels and maximized on the current display. Page scrollable height was zero in each case, including long names, error notices, details opening, and leaving/returning to the result. All six actions remained in bounds with minimum 44 DIP height; keyboard focus was checked. Existing reader opened the exact output, print routing received the exact output without submitting a print job, and back/close callbacks remained functional. Synthetic source/result SHA256 hashes were unchanged by UI actions.

Evidence directory: `C:\Users\cahpo\.codex\visualizations\2026\10\11\01a129fc-1d3a-7272-98b4-f28139fc1e4e\saved-result-fit`. Contains `build.log`, baseline source, and `audit-1\audit.txt` plus screenshots. Normal, minimum, maximized, long-name, and error screenshots were visually inspected. The details screenshot capture returned a 1-pixel image and is not visual evidence; flyout bounds and accessibility were verified through native assertions.

Claims are limited to supported application window sizes on the current display. Below-minimum windows and other monitor/DPI/accessibility configurations are not verified. Folder reveal uses its existing handler; external Explorer interaction was not exercised.

## Initial delivery attempt (superseded below)

Build is available in the evidence directory's `build` folder. Standard local-update packaging and installation remain blocked by the D drive having only 3,018,752 free bytes at verification. No version bump, release-feed mutation, installed-pointer change, shortcut change, or artifact deletion was performed. Existing unrelated checkout changes and prior Merge panel changes are preserved. Free sufficient D space or explicitly authorize a different local feed destination before packaging via `eng/Build-LocalUpdate.ps1`.

## Final redesign delivery

Drive D had 12.3 GB available during the approved redesign implementation. Standard `eng/Build-LocalUpdate.ps1` completed successfully with admitted private .NET 10.0.12 runtime; release-notes, product/version, distribution/updater, PDF worker/editor/search, Word, native Word/quality, and PDF association gates passed. Full log: C evidence root `saved-result-fit/local-update.log`.

Published local product **0.3.0 · Pengembangan**, immutable build **20261011-155226-731a8109**, manifest SHA256 **93C55B4CCFDC43D702A0FC5A0CDA2B617DEFE153740D100B450668A9BBF9A784**. All 1,333 payload lengths/hashes and manifest hash verified with zero failures. Published desktop assembly hash equals the final audited assembly. The complete current source checkout was built, preserving the separately completed Home repair; two stale Home source files in the earlier C staging checkout were detected, so final delivery used the actual D checkout rather than that staging copy.

The exact final package native saved-result audit passed **141 checks**, exit 0, at C evidence root `saved-result-fit/final-redesign-audit`. Wide, minimum-size and long-name screenshots were visually inspected. No page scrolling at the tested supported sizes; reader/print routing/back/close and source/result hashes preserved. The former baseline appearance descriptions above are historical; final navigation is upper-right, with a second header row on narrow windows.

Installed pointer remains **20261011-154323-b1783f4d** with manifest **63DF43000EA64A4A44DA690EC60425B38B8AA2766B63207D10A274FF8B5D8D9F**. The launcher, shortcut, user data and running installed application were not changed. User can activate the local package through the existing in-app update flow. No online distribution or Production approval is implied.
