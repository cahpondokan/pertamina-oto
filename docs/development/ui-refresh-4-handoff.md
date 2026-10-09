# UI 4 — Ambil halaman handoff

Date: 2026-09-15. Checkout `worktrees/core-functional-first`, branch `task/core-functional-first`.
UI 3 explicitly released the writer slot before edits. Existing dirty changes preserved. No dependencies, uploads, worker mutations, phase-gate edits or final integration.

## Delivered behavior
- Existing empty-screen file picker, drag/drop and Explorer paste preserved.
- Dedicated loaded Extract workspace: PILIH FILE, paste button, Explorer Ctrl+V and single-local-PDF drop. Text/password inputs retain normal text paste. Reject multiple inputs, UNC and files over 100 MB.
- Local thumbnails with blue checkmarks and accessible selection labels. Click pages in desired output order; click again to deselect. Manual ordered ranges such as 3,1,4-6 are supported. Duplicate/out-of-bounds/malformed selections rejected. Empty selection disables processing; selecting every page is allowed.
- Source/result page counts and output-order summary; name field, PILIH FOLDER HASIL and existing default-folder controls preserved.
- Execution replaces preview/editor with indeterminate local progress and BATALKAN. Existing WorkerClient handles output validation, source protection and publication.
- Only successful worker replies show BUKA FILE HASIL, BUKA FOLDER HASIL, KEMBALI EDIT and selectable/copyable name/page-count/size/location summary. No result preview. Failure/cancel restores editor and error without success buttons.
- Back to Edit preserves source, selection order, output name and folder. Password cleared after processing for security; protected inputs need re-entry on another run.

## Exact files / diff
- `src/Hosts/Pertamina.Oto.Desktop.App/ExtractWorkspace.cs`: new feature-specific workspace, parser, executor and result controls, adapted from established Remove workspace without changing Remove.
- `src/Hosts/Pertamina.Oto.Desktop.App/SourceWorkspace.cs`: two extract-only Build/Refresh dispatch hooks.
- `src/Hosts/Pertamina.Oto.Desktop.App/MainWindow.cs`: extract-only Indonesian title, hidden session favorite, collapsed development limitations.
- `docs/development/ui-refresh-4-handoff.md`: this report.
Task-only shared diffs and immediate baseline snapshots: `artifacts/ui-refresh-4-evidence/{MainWindow,SourceWorkspace}.patch` and `*.before.cs`. Do not restore shared files from HEAD. New ExtractWorkspace.cs is the complete new-file diff. No EmptyToolScreens changes in this task.

## Verification
- VS2026 MSBuild `/t:Build /p:Configuration=Debug /p:DesktopUiOutputPath=<checkout>/artifacts/desktop-extract-ui/ /v:minimal /nologo`: PASS exit 0, no restore or analyzer suppression. Log `artifacts/ui-refresh-4-evidence/build.txt`.
- Actual final parser compiled with PowerShell Add-Type: 18 checks PASS (order/ranges/all/empty/bounds/duplicates/malformed/+ prefix). Repro script `artifacts/ui-refresh-4-evidence/selection-tests.ps1`. Console output observed; direct Console.WriteLine may bypass the redirected log.
- Worker `--self-test`: 34 checks PASS, synthetic inputs, source SHA256 unchanged. Log `artifacts/ui-refresh-4-evidence/worker-selftest.txt`.
- `git diff --check`: PASS, only CRLF normalization warning. `git diff --no-index` used for patches returns 1 for expected differences, not test failure.

## Acceptance / risks / shared audit
No GUI launch or visual acceptance performed. Integrator should verify empty/loaded pick/drop/paste, ordered click 3 then 1, typed ranges, all/empty/invalid/duplicate selection, narrow window/long names, locked/corrupt PDFs, source/output collision, cancellation/failure recovery, result open/copy and Back to Edit.
Inherited limits 100 MB / 1000 pages and renderer/fidelity limits remain. Thumbnail rendering uses existing local renderer. No Shift-range interaction; use typed ranges. Global menu Ctrl+O/save routes not extended; feature visible picker and Ctrl+V are available. Empty screen remains existing generic presentation. Audit navigation/drafts and shared source-preview lifecycle among all five features in final integration.

Next writer UI 5 after explicit release. Final integration only by integrator after user instruction. This is not production/phase completion or GUI acceptance.
