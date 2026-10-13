# Direct text input fix — 21 September 2026

User reported that added text could not be typed. Inspection found the page used a non-interactive TextBlock and did not transfer focus to its separate inspector TextBox. The inspector is hidden in compact mode. Existing tests changed inspector text programmatically and did not cover the actual placement-to-typing interaction.

Fix: placement now opens a native TextBox on the page, focuses it and selects the placeholder. Redraw preserves that exact control rather than clearing it on each character. Enter/Escape finishes editing; model changes remain undoable. Existing text also has a visible `Ketik langsung di halaman` action. Save/navigation cleanly end the inline editor. Delete remains normal text editing while the field has focus.

Changed: `EditorInlineText.cs`, `LivePdfEditor.cs`, `LiveEditorAudit.cs`. No new package, worker engine change, source overwrite or user-document testing.

Build: `artifacts/desktop-editor-typing/Pertamina.Oto.Desktop.App.exe`; MSBuild Debug and diff checks PASS.

Audit: `artifacts/desktop-editor-typing/live-editor-audit-87f14d047ddd40539cb8c0e195198ebb/audit.txt`, 54 PASS. Seven added assertions cover immediate input focus, native SelectedText updates, focus/control identity after redraw, Delete protection, compact input, real save and exact saved text. Existing editor/object/original-text assertions retained. UIA Value pattern was unavailable; the harness now uses the native text selection/replacement API, not a claim of physical keystroke injection.

Independent pypdf extraction confirms exact `EDITOR NEW TEXT UPDATED` and absence of `Teks baru` in the first saved PDF. Poppler rendered `typed-result.png`; output and compact input screenshot visually inspected. Physical keyboard/mouse, pinch and screen reader acceptance still require device-level testing. Existing supported-font/text-length limitations remain unchanged.
