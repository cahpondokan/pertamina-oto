# Favorit — approved card design

Implemented 17 September 2026, targeted local UI change (not phase or release approval).

## Design

Native WinUI cards, white rounded header and cards, pale background, local Lucide icons from the already bundled package (existing Assets/History/LICENSE applies). Three columns at desktop width, two at intermediate widths, one at narrow widths. Dynamic catalog-backed favorites; no fabricated user entries. Search includes counts and no-results state. Separate accessible star actions retain removal/undo and existing atomic persistence. Existing tool routing and capability limitations preserved.

## Verification

- VS2026 MSBuild Debug succeeded with existing warnings-as-errors settings.
- Native Favorites audit: 13 checks plus completion marker PASS. Screenshots at 1464x881 and 624x721 reviewed; no card clipping. Search, empty state, remove, undo, isolated save, tool opening and catalog navigation checked.
- Flow regression: 15 PASS.
- Library regression: 4 PASS, including restart persistence and corrupt-store protection.
- History UI regression completed with source PDFs unchanged.
- git diff --check passed (existing line-ending warning only).
- Test data is isolated; user library and source documents were not overwritten.

Build: artifacts/desktop-favorites-approved/Pertamina.Oto.Desktop.App.exe

DLL SHA256: C2AA1089D3D37B68A3FD259AB17F1A7BF9C42C5C05E088AFD9E4C38247EDC961

Evidence under that build directory:

- favorites-ui-audit/audit.txt
- favorites-ui-audit/01-desktop.png
- favorites-ui-audit/02-no-results.png
- favorites-ui-audit/03-narrow.png
- favorites-ui-audit/04-empty.png
- history-ui-audit/audit.txt
- flow-selftest.txt
- library-selftest.txt

## Remaining manual acceptance

Real user comfort, full keyboard traversal, screen reader narration, high contrast and larger system text require manual acceptance. Automated accessibility names/focus and action targets >=44 DIP passed; this is not complete accessibility certification. Existing running older builds are not replaced or forcibly closed.
