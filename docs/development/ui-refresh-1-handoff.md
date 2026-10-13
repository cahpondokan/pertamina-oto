# UI 1 — Gabungkan PDF handoff

Date: 2026-09-15. Checkout: worktrees/core-functional-first.
User authorized implementation based on four reference screenshots. UI 2 explicitly released its writer slot before this task wrote. No subagents used. Existing dirty changes preserved; no phase gates, worker, dependencies, branding, sidebar or folder preferences changed. Final integration remains with the integrator.

## Behavior

- Merge empty screen: large Choose PDF action, concise explanation, drop target and paste retained.
- Merge workspace: numbered thumbnail cards with truncated names/full-name tooltips, file count, drag ordering, keyboard-accessible arrow/rotate/remove actions, enlarged multi-page preview dialog, add/paste and A–Z sort.
- Loaded workspace accepts external PDF drops. Picker/paste/drop additions share local PDF filtering, case-insensitive deduplication, 50-file/100-MB bounds. An over-limit incoming batch is not added. Existing worker validation remains authoritative.
- All-page rotation remains per card; the previous rotate-all toolbar button is replaced by A–Z sorting.
- Primary Merge action is prominent. Output name/folder and password controls retained, with ordering guidance and a directly visible Pick Output Folder button. Loaded workspace supports Ctrl+V file paste; empty screen retains its Ctrl+V binding. Shared run styling resets when another tool opens.
- During merge, a local processing panel hides the source cards and offers cancellation. Progress is indeterminate because the existing worker does not stream measured percentage. No upload, simulated percentage or estimated completion time.
- Success screen appears only on successful worker reply after publication: large Open Result File button, Open Result Folder, Back to Edit preserving the actual input/order/rotation/options controls, and selectable/copyable filename/count/size/path summary. No preview on success, per latest user standard relayed by integrator while this task still held writer. Failure/cancel restores workspace and existing worker message; it does not show success.

## Exact changed paths

All source paths below are under src/Hosts/Pertamina.Oto.Desktop.App/:

- MergeWorkspace.cs: feature-only UI, import validation, sorting, process and success views.
- EmptyToolScreens.cs: conditional merge-only presentation after constructing the existing input controls. Other tools retain their original presentation and import logic.
- MainWindow.cs: reset shared run styling; merge picker import route; merge process/reply/success hooks; omit duplicate generic result preview for merge. UI 2 split changes retained.
- docs/development/ui-refresh-1-handoff.md: this handoff.

Task-only diffs against immediate pre-edit snapshots: artifacts/ui-refresh-1-evidence/{MainWindow,MergeWorkspace,EmptyToolScreens}.patch. Do not replace dirty files with versions from git HEAD.

## Verification

- VS2026 MSBuild host project /t:Build /p:Configuration=Debug, no restore, exit 0. Output: artifacts/desktop-merge-ui/. Log: artifacts/ui-refresh-1-evidence/build.txt. First attempt failed on an unavailable helper, replaced with native InfoBar; final build passed.
- artifacts/desktop-merge-ui/worker/Pertamina.Oto.LocalPdf.Worker.exe --self-test: exit 0, 34 checks passed, including per-file rotation and unchanged source SHA256. Log: artifacts/ui-refresh-1-evidence/worker-selftest.txt. Worker engine unchanged by UI 1.
- git diff --check: exit 0; artifacts/ui-refresh-1-evidence/diff-check.txt.
- Final source/build hashes: artifacts/ui-refresh-1-evidence/hashes.txt.

## Remaining GUI acceptance / risks

No live GUI inspection was performed for this build. Build and worker checks do not establish visual fit or drag/keyboard acceptance. Integrator/manual verification: empty pick/drop/paste; loaded external drop/Ctrl+V; mixed formats/duplicates/over-limit batch; 0/1/2 files; reorder via drag/arrows/A–Z; rotate and enlarge; long filenames; narrow/short window; default and explicit folder selection; protected PDF; failure/existing output/cancel recovery; success Open File/Open Folder/Copy Summary/Back to Edit retaining settings; then navigate to Split and other tools to check shared run style reset.

Preview retains existing renderer and development limitations. A–Z is ascending only. Worker does not supply percentage progress. Existing output collision behavior is unchanged: choose a new name if destination exists. No feature-chain/cloud buttons from the reference were introduced. No final integration, production approval, publishing or phase completion claimed.

Next writer: UI 3 after explicit release from UI 1. After release UI 1 must not edit/build without coordinating again.
