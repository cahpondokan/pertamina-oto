# UI 3 — Hapus halaman handoff

Date: 2026-09-15. Checkout: `worktrees/core-functional-first`, branch `task/core-functional-first`.

User authorized the reference-based Remove Pages interaction and result opening. UI 1 explicitly released its writer slot before implementation. Existing dirty UI 1/UI 2 work was preserved. No subagents, new dependencies, uploads, worker changes, phase-gate changes, publishing or final integration.

## Behavior delivered

- Empty Remove screen: prominent red PILIH FILE PDF, existing drop and Explorer-file paste support.
- Loaded Remove workspace: PILIH FILE, drop one local PDF, paste button and Ctrl+V (text/password focus retains normal text paste). Reject multiple files, non-PDF, UNC inputs and files over 100 MB through this feature's importer.
- Local page thumbnails. Click/keyboard activation toggles a deletion candidate; selected pages fade with a red cross and border. Accessible names announce retained/deleted state.
- Editable deletion list accepts page numbers/ranges and synchronizes with cards. Summary shows total/deleted/remaining pages. Zero selection, invalid input and all-page deletion disable the action. Duplicate entries are normalized before the worker request.
- Prominent red Hapus halaman action; name field, explicit PILIH FOLDER HASIL and existing folder-preference control. Default-folder behavior is unchanged.
- Execution replaces the editor/preview with indeterminate local progress and BATALKAN. No fabricated percentage. Existing WorkerClient validates/publishes to a new name and protects source files.
- Only a successful worker reply produces the result screen: BUKA FILE HASIL, BUKA FOLDER HASIL, KEMBALI EDIT, selectable summary and Salin ringkasan. No preview on success. Launcher false/error outcomes show a message.
- Failure/cancel restores the editor and worker message without success controls. Back to Edit preserves source, deletion selection, output name and folder. Password is cleared after processing; a protected PDF needs re-entry for another processing attempt.

## Exact task changes

All source paths are relative to `src/Hosts/Pertamina.Oto.Desktop.App/`:

| Path | Change |
| --- | --- |
| `RemoveWorkspace.cs` | New feature-specific input, thumbnail selection, parsing, execution/progress and result screen. |
| `SourceWorkspace.cs` | Two conditional `remove` dispatch hooks in BuildSourceWorkspace/RefreshSourcePreview. Existing Split/JPG hooks retained. |
| `MainWindow.cs` | Remove-only displayed Indonesian heading, hide session favorite on this screen, include Remove in existing collapsible development limitations. No executor changes. |
| `EmptyToolScreens.cs` | Remove-only empty-screen presentation block. Existing other-tool logic retained. |
| `docs/development/ui-refresh-3-handoff.md` | This handoff. |

Task-only patches against immediate pre-edit snapshots: `artifacts/ui-refresh-3-evidence/{MainWindow,SourceWorkspace,EmptyToolScreens,RemoveWorkspace}.patch`. Baseline snapshots and final SHA256 source/build hashes are in the same evidence directory. Do not replace these dirty files from Git HEAD.

## Verification

- Final VS2026 MSBuild host build: PASS, exit 0, no restore and no analyzer suppression. Tool `D:/DevTools/VS2026Community/MSBuild/Current/Bin/MSBuild.exe`; `/t:Build /p:Configuration=Debug /p:DesktopUiOutputPath=<checkout>/artifacts/desktop-remove-ui/ /v:minimal /nologo`.
- Build output: `artifacts/desktop-remove-ui/`. Log: `artifacts/ui-refresh-3-evidence/build.txt`.
- Worker self-test: PASS, 34 checks, including remove page, reject remove-all without output, source overwrite rejection and source SHA256 unchanged. Log: `worker-selftest.txt` in evidence directory. Synthetic files only.
- Actual selection-parser source compiled and exercised by `selection-tests.ps1`: PASS, 15 checks for ranges, order/deduplication, empty selection, bounds, malformed values and overflow-sized ranges. Log: `selection-tests.txt`. The parser accepts an empty/all-page set as syntax; UI readiness and worker enforce semantic restrictions.
- `git diff --check`: PASS. Evidence `diff-check.txt`.

## Remaining acceptance and limits

No live GUI inspection was performed. Build and engine checks do not establish visual layout, actual pointer/keyboard behavior or successful opening through an installed PDF viewer.

Integrator/manual checklist: empty and loaded pick/drop/paste; choose pages 2 and 4, click again to undo; manual ranges and duplicates; empty/all/invalid selections; single-page PDF; large/locked/damaged PDF; narrow/short window and long filenames; existing output/source collision; cancellation/failure recovery; success file/folder/copy actions; Back to Edit retaining values; navigate among Merge/Split/Remove to audit shared hooks and drafts. Existing global menu Ctrl+O/save/paste routes were not extended for the new feature-specific editor; the feature's visible controls and Ctrl+V binding are provided.

Inherited preview limit is 100 MB / 1000 pages. Rendering uses the existing local renderer. Shift-range selection from the reference is not implemented; ranges can be entered directly. Existing PDF fidelity limitations remain. No Production/readiness or phase-completion claim.

Next writer: UI 4 Ambil halaman after explicit release. Latest standard to preserve: choose/drop/Explorer Ctrl+V → settings + preview + choose folder → progress/cancel → validated success with open file/open folder/back to edit and copyable summary; no success controls after failure. UI 3 must not write/build after release without renewed coordination. Final integration remains with the integrator after user instruction.
