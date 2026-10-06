# PDF editor: direct interaction checkpoint

## Follow-up: shared save/result presentation

Editor production saves now show a dedicated cancellable, indeterminate progress screen, then use the existing UnifiedResult component in an editor-local host. The shared component retains its existing behavior for other tools. Editor result actions include internal preview, open folder, return to retained draft, new document, and copy summary. The editor already records history, so the local result host does not record it a second time. Failure/cancellation returns to the editor notice and draft. Keyboard object editing is blocked while the result screen hides the draft.

Build `artifacts/desktop-editor-save-screens` passed. Native audit: 132 PASS, exit 0, `live-editor-audit-a26ceac035c64ff8bf856c1f04ef3198/audit.txt`. Native screenshots `editor-saving.png` and `editor-saved.png` inspected. Added tests check screen replacement, cancellation token request, editor restoration, retained draft, no duplicate history from presentation, and hidden-draft shortcut protection. Shell folder opening, clipboard and physical input remain manual UAT, not a full UI/UX sign-off.

## Follow-up: default output folder

Editor Save now resolves the current shared session folder, then the configured default, instead of always opening a folder picker. Missing/unavailable folders prompt for a replacement; cancellation retains the draft. A chosen replacement updates the session only, not the persisted default. The shared output-location control is shown in the editor footer, including session/default folder actions. Unique output naming remains unchanged.

Debug build: `artifacts/desktop-editor-default-save`. Native audit: 126 PASS, exit 0, at `live-editor-audit-d11ec3a9ada541b1a9c28df91e57563f/audit.txt`. Added real export checks cover default destination without picker, session override and repeated unique saves. Tests temporarily replace in-memory folder settings and restore them, suppressing history persistence; no user preferences changed. Desktop screenshot inspected. Picker cancellation/unavailable-folder interaction still requires manual UAT.

Date: 2026-09-21. Local development increment, not a release/UAT approval.

## Fixes

- Freehand release now snapshots and commits the stroke before releasing pointer capture. Previously capture-lost cancellation cleared the working points before commit. Pencil mode remains active for consecutive strokes; abandoned gestures still cancel safely.
- Original text supports on-page native text input: double-click selects a word, triple-click selects the current PDF text run, Enter applies, Escape cancels the pending draft. Native text selection, clipboard, Delete and typing undo remain handled by the text box.
- Pending original input survives navigation and is committed before saving. Validation failures retain typed text without changing the PDF. Save reads the current native input synchronously, including when TextChanged is still queued.
- Original text is changed through the existing original-operand replacement pipeline, not a saved white rectangle overlay.

## Verification

- MSBuild Debug passed into `artifacts/desktop-editor-inline-original`.
- Native audit: 119 PASS; process exit 0. Evidence: `artifacts/desktop-editor-inline-original/live-editor-audit-a8fbd549033e43cc9940370c2d59aec5/audit.txt`.
- Audit covers release/capture-lost order, stroke export, undo/cancel, long-stroke sampling, original double/triple-click selection helpers, native typing, rejected input preservation, navigation and immediate save.
- Independent pypdf extraction confirmed final `INLINE SAVED 789`, removal of the original heading, retention of neighboring source content and the control page.
- Independent PDF path inspection confirmed the freehand polyline. Poppler renders of saved freehand and original-text outputs and the native `original-inline-typing.png` screenshot were visually inspected.
- Synthetic fixtures only; no source user PDF overwritten. No new dependencies.

## Limits / pending manual checks

## Follow-up: pen bounds and pre-draw styling

Freehand rendering now places an unsized Polyline inside a Canvas instead of forcing the shape into its geometry bounds. New stroke bounds include half the stroke width plus a 1 pt allowance; page-edge pointer clamping uses the chosen thickness. This prevents the preview from clipping the outside of strokes without changing saved point positions.

The active pencil panel exposes six named colors and thickness 1–24 pt before drawing, with the same values used in the live preview and saved mark. Existing objects can also use all six colors. Styling remains available after each stroke.

MSBuild passed into `artifacts/desktop-editor-pen-colors`. Native audit passed 123 checks, exit 0: `live-editor-audit-a2e9a34d024048488ff8e8a7b053195f/audit.txt` under that output. New checks cover the actual color/thickness controls, persisted mark styling, padded bounds, and unsized pen geometry. The worker successfully exported the generated stroke. The native `freehand-color-bounds.png` screenshot was visually inspected: the 12 pt red stroke and settings are visible without cropped outer edges. Physical mouse comfort and touch UAT remain pending.

The audit invokes production gesture helpers and native text controls; it does not inject physical mouse double/triple-clicks or touch gestures. Manual input-device UAT remains required. Original editing is per admitted PDF text run, not paragraph reflow; unsupported PDFs/fonts retain existing restrictions. Original-text font-size shortcuts are not implemented by this increment. Accessibility and full UI/UX approval are not claimed.
