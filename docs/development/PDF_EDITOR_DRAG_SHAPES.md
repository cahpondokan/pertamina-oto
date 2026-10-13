# Shape drawing interaction — 21 September 2026

User requested conventional click-drag creation instead of fixed-size shape placement. Updated rectangle, ellipse and line modes: press captures the pointer; movement draws a temporary preview; release commits one object and one undo snapshot. A click/jitter creates nothing. Cancellation/Esc/capture loss discard the preview. Shift constrains rectangles/ellipses to equal dimensions. Existing text click-to-type behavior is retained.

Lines now retain two local endpoints so horizontal, vertical and rising/reversed drags are represented correctly in both XAML and PDF export. Older lines without endpoints retain their prior behavior. Numeric resizing scales endpoints; invalid endpoint counts/coordinates fail before publication. No package or PDFium changes, source overwrites, or user-document tests.

Build: `artifacts/desktop-editor-shapes/Pertamina.Oto.Desktop.App.exe` — MSBuild PASS; diff check PASS.

- Native audit: 73 PASS in `artifacts/desktop-editor-shapes/live-editor-audit-99c6a390f886453e86b921599ef6e99c/audit.txt`. Nineteen new shape assertions plus all prior text/editor checks. Includes no-drag/no-history, reverse preview, release/undo, cancellation, horizontal/vertical lines and Shift circle.
- Worker: 35 PASS, synthetic evidence `%TEMP%/oto-editor-test-6f3cb127b35844acb9fb30458c5cc7d8`. Includes saved shapes and invalid endpoint rejection.
- Independent pdfplumber inspection confirms horizontal `(40,300)-(220,300)`, vertical `(280,300)-(280,400)`, rising diagonal `(40,550)-(220,450)` in top-down point coordinates. Poppler result `artifacts/desktop-editor-shapes/drawn-shapes.png` visually inspected.

Automated audit exercises the same gesture state functions as pointer handlers; physical mouse/touch injection, pinch and screen reader tests remain unperformed. No full UI/UX acceptance or production approval claimed. Existing original-text/font limitations are unchanged.
