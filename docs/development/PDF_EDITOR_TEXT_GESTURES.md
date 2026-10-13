# Added-text gestures — 21 September 2026

Scope: conventional editing of added text. Original PDF text remains a separate restricted engine; the user's failing PDF has been requested and has not yet been supplied. No general original-font resizing or Acrobat-equivalent editing is claimed.

## Interaction

- Double-click an added text object in Select mode to focus its inline TextBox and select the word under the pointer. Triple-click selects its entire single line.
- Multi-click recognition survives canvas element redraws, observes the Windows double-click interval, rejects distant/other-object clicks, and resets on page/document changes and object movement. Minor pointer jitter does not move the object.
- F2 or Enter starts typing in selected added text. Enter/Escape finishes typing (retains the current edit).
- Ctrl+[ / Ctrl+] decrease/increase the entire added text object's font by one point. Ctrl+Shift+< / Ctrl+Shift+> use preset sizes. This is object-level formatting, not mixed fonts within a word selection.
- Font size remains 1–72 pt; the text box expands when necessary and enlargement is refused if it would cross the page boundary. Inline control, focus and selection persist. Inspector values stay synchronized.
- PreviewKeyDown routes editor shortcuts before ScrollViewer handles keys. Native text typing/selection and Delete remain owned by TextBox while typing; document shortcuts are guarded against property inputs.

## Verification

- MSBuild Debug PASS: `artifacts/desktop-editor-text-gestures/Pertamina.Oto.Desktop.App.exe`.
- Native audit: 91 PASS (18 new text gesture checks plus 73 prior checks), `artifacts/desktop-editor-text-gestures/live-editor-audit-117985d5f45949698fb9d6d377b0cc39/audit.txt`.
- Audit invokes the same interaction helpers as pointer/key handlers and operates a real focused WinUI TextBox. It does **not** inject physical mouse/keyboard input. Physical double/triple-click timing, keyboard-layout variants, pinch and assistive technology require manual UAT.
- Independent pypdf/pdfplumber/Poppler check PASS: exact added text exports at 18 pt; original source text retained; untouched second page and source header pixel-identical. Evidence: `artifacts/desktop-editor-text-gestures/roundtrip/report.json`.
- Inspected rendered result and native direct-typing screenshot: no clipping in this synthetic fixture; sidebar shows synchronized 18 pt.
- `git diff --check` PASS; existing CRLF notice only. No dependencies added or source PDFs overwritten. No production/UI-UX acceptance asserted.

Manual check: open the new build, add a short line, finish with Enter, double-click a word, type a replacement, triple-click, try both font shortcut pairs, save and reopen. Supply a non-sensitive failing source PDF for the separate original-text diagnosis.
