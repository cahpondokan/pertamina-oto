# Merge PDF — native result settings panel

11 October 2026. Scoped implementation verified; local update delivery remains blocked by disk capacity. This is a UI task, not a phase execution or release approval.

## Scope and dependency

Rollback chat `01a129f9-f7ea-7043-ba41-bef23ed6c94a` completed its inverse patch and compilation before source ownership was taken. No source/checkpoint/build was written by this task before its final handoff. The approved raster design was inspected as a reference; the implementation is native WinUI controls, not an embedded image.

Only the right-hand “Hasil gabungan” settings panel was redesigned. Its existing 320 DIP layout column and small-window stacking remain unchanged. The header uses a royal-blue/cyan gradient, pale form surface, blue border, document icons, yellow folder card with complete path tooltip/accessibility name, direct session folder picker, collapsed password expander, amber/green readiness, native action states and subordinate organizer link. Original progress/results screens remain intact.

Name editing preserves the complete `.pdf` filename rather than separating the extension. The scoped 44 DIP TextBox binds two ways to the existing outputName control, with its same 120-character limit. Execution still validates the original value, chooses an available non-overwriting path, processes and saves directly. Folder selection still calls PickFolderAsync; no default-folder mutation was added. Buttons bind to the original readiness/cancellation controls and invoke existing execution/cancellation handlers. No pending-results stage, new Save As workflow or output engine was introduced.

## Source files

- `MergeWorkspace.cs`: assemble scoped controls and update panel readiness/cancel visibility.
- New `MergeResultPanel.cs`: native presentation controls and bindings only.
- `MainWindow.cs`: one exact hunk selecting the merge-specific frame.
- New `MergeResultPanelAudit.cs`: DEBUG-only synthetic native audit.
- `Program.cs`: DEBUG-only isolated audit preparation and dispatch.

All are under `src/Hosts/Pertamina.Oto.Desktop.App`. No dependencies or product-version changes. The build props, worker/parser, reader/zoom, editor, print pipeline, updater and website source were not changed. A hash comparison of 189 other source files against the task staging snapshot found zero mismatches. The prior dirty worktree is preserved. This document is the only additional repository documentation file.

## Verification and evidence

Persistent evidence root:
`C:\Users\cahpo\.codex\visualizations\2026\10\11\01a129fc-1d3a-7272-98b4-f28139fc1e4e`

- Final VS MSBuild `/t:Build /p:Configuration=Debug /p:UseSharedCompilation=false` completed with exit 0 and no warnings/errors. Build output is `final-build`; log is `stage/final-build.log`. Full project references were built in the C staging checkout with separate project intermediate directories.
- Final `--audit-merge-panel <evidence>/acceptance` exited 0: **24 PASS / 0 FAIL**. `acceptance/audit.txt` covers disabled one-PDF state, readiness count, native keyboard focus, complete long-name binding and length limit, invalid-name validation/readiness, collapsed password default, error priority, busy/cancel state, compact/normal sizes, actual direct-save merge, reopened six-page output, ordered source page dimensions, unchanged source SHA256 and actual worker cancellation with no final result.
- Native screenshots: `acceptance/one-pdf.png`, `ready.png`, `keyboard-focus.png`, `long-name-folder-password.png`, `error.png`, `busy-cancel.png`, `size-660.png`, `size-660-bottom.png`, `size-1000.png`, `size-1400.png`, `maximized.png`, `final-ready.png`. The busy screenshot uses the native test harness to set processing state; actual worker cancellation is verified separately. These are actual rendered WinUI controls, distinct from the approved mockup.
- Window sizes are outer window pixels: 660×810, 1000×900, 1400×1100, plus maximized on this display. Bottom screenshots verify scroll access. No claim of all monitors or physical DPI coverage. Existing minimum-window policy was not changed.
- Worker `--self-test`: **41 PASS**, exit 0, `worker-tests.txt`. The final worker DLL hash matches the exact tested worker DLL; see `verified-build-hashes.json`.
- Final desktop DLL SHA256: `8102C5123BF58450AFE92582B0FF5E4B459CBAEF564D7605137E5015626E32AD`.
- Before-edit source checkpoint and SHA256: `baseline/`, `baseline-hashes.json`. Exact owned diffs: `MainWindow.cs.patch`, `MergeWorkspace.cs.patch`, `Program.cs.patch`; outside-scope audit: `scope-audit.json`.

Failed attempts are retained. Initial staging lacked dependency runtime output files; copying/building project references resolved that. A fixture intake setup error and an incorrect two-page assertion were corrected (each source fixture has three pages). WinRT StorageFile access was sandbox-denied; the authorized synthetic native audit passed with the required runtime access. A compile-time high-contrast API enum mistake was fixed before the final passing build. No failed attempt is acceptance evidence.

## Remaining limits and delivery

Computer-use inspected the panel and opened the existing folder picker. Keyboard Escape cancelled it and retained the previous folder. Selecting a new folder was not completed: the helper became stuck with “Computer Use helper already has an active request” and remained blocked after the documented reset/retry. An older isolated interactive audit window may remain with a folder picker open; the installed user application was not force-closed. No complete manual mouse/keyboard UAT or screen-reader UAT is claimed. Physical high contrast/reduced-motion/DPI changes were not exercised; native button states and system highlight-text fallback are used, and no custom animation was added.

At final inspection D had 3,072,000 bytes free; C had approximately 10.4 GB. `eng/Build-LocalUpdate.ps1` has not been run because its authoritative feed on D cannot accommodate the package. No artifacts/releases/user data were deleted. A tested development build exists on C; it is not an immutable self-contained local update package. Publishing requires additional D capacity or an explicit decision to change the feed destination, then the standard delivery gates.

Installed `current.json` remains product 0.3.0 / build `20261011-130427-44487e3d`, manifest `88E9A34A4C6EAC320C3B03F26E2A919C3EA32E5F298DB3B3ECA8AD699A0C0F14`. No installation, pointer/shortcut update, online publication, GitHub publication or Production action occurred. Source ownership is released for this scoped task; no other redesign is started.
