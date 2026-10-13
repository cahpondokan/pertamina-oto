# UI 2 — Pisahkan PDF handoff

Date: 2026-09-15. Branch: `task/core-functional-first`.
Checkout: `worktrees/core-functional-first`. User explicitly authorized the proposed four-mode split UX and local implementation for trying the app. This is feature development, not a phase gate/promotion or final integration.

## Delivered behavior

- Two tabs: Rentang Halaman and Ekstrak Halaman. Custom ranges with add/remove and optional combination; fixed page count with remainder; every page separately; selected pages via normalized text and synchronized thumbnail checkboxes, optionally combined.
- Default source range 1 through last page; numbered previews, per-page group labels and enlarge dialog retain selection.
- Source name/count, Ganti PDF, range validation, result count/summary, default session output folder with change action, dedicated process/cancel/progress, success list and Open File/Open Folder.
- Direct PDF output. One output is a new PDF in the chosen folder; multiple outputs are published together inside a unique source-derived folder. Existing names use numbered suffixes. Sources locked read-only throughout processing/publication. Existing worker `organize` layout operation handles extraction and owner-password checks; no worker/protocol/dependency change.
- All groups processed in a unique sibling staging directory, validated by existing worker and reply page count. Final publication occurs only after all groups succeed. Cancellation/failure cleans only that run's stage. Cleanup failure is reported. Combined output and all generated pages bounded at 1000; total batch bytes bounded at 250 MB.
- Panel settings scroll separately from summary/folder/CTA. Shorter split-specific layout; global logo/sidebar preserved. Split-only menu Save route uses batch executor.

## Exact ownership / changes

New files under `src/Hosts/Pertamina.Oto.Desktop.App/`:

1. `SplitPlan.cs`: pure grouping, range validation, selection normalization and labels.
2. `SplitExport.cs`: batch orchestration, staging, publication, unique names and cleanup.
3. `SplitWorkspace.cs`: split-only UI and executor; `OpenSplitFileAsync` opens a supplied local path without processing.
4. `SplitSelfTest.cs`: generated eight-page PDF; grouping/publication/cancellation/source-integrity assertions.

Shared files modified (do not replace entire files from git HEAD; baseline was already dirty):

- `SourceWorkspace.cs`: two early split routes in `BuildSourceWorkspace` and `RefreshSourcePreview`.
- `MainWindow.cs`: split display title, suppress generic favorite/warning in split, split frames in workspace, updated split help sentence. Generic execute/worker path otherwise untouched.
- `Program.cs`: `--self-test-split`; `--open-split <existing-local-path>` activates split UI without processing.
- `ToolCatalog.cs`: only split description, removes obsolete ZIP claim.
- `DesktopMenu.cs`: single split-only Save menu dispatch before generic `run.IsLoaded` check.

No edits to MergeWorkspace, EmptyToolScreens, four other feature implementations, worker, csproj, package lock, settings, logo, sidebar, project phase state/register/log. No subagents dispatched. Coordinator granted UI 2 exclusive writer before UI 1; no concurrent writer observed.

## Build / evidence

Evidence directory: `artifacts/ui-refresh-2-evidence/`.

- `build.txt`: final VS2026 MSBuild host build exit 0, no restore/no analyzer suppression. Tool: `D:/DevTools/VS2026Community/MSBuild/Current/Bin/MSBuild.exe`, project `src/Hosts/Pertamina.Oto.Desktop.App/Pertamina.Oto.Desktop.App.csproj`, `/t:Build /p:Configuration=Debug /p:DesktopUiOutputPath=<checkout>/artifacts/desktop-split-final/ /v:minimal /nologo`.
- `split-selftest.txt`: 33 PASS, zero FAIL. Real worker direct split, reopened counts and page identities, fixed remainder, normalized selection, combined output, invalid inputs, unique file/folder names, pre-cancel, mid-batch cancel, later-group failure, no stage remnants and unchanged SHA256. Run on `desktop-split-ui` build; subsequent changes are layout, help/menu/startup routing only, not SplitPlan/SplitExport behavior.
- `diff-check.txt`: `git diff --check` exit 0.
- `*.cs.patch`: task-only shared diffs against pre-edit snapshots (DesktopMenu and ToolCatalog baseline reconstructed by reversing the single documented task edit). `hashes.txt`: final source and desktop DLL hashes.
- First build invocation used VS2022 and failed SDK resolution; switched to installed VS2026. Analyzer errors fixed without suppression. First test assumed PDF points equalled Windows preview units; fixed by comparing source/output dimensions in the same renderer, then all 33 checks passed.

## Try it

Executable: `artifacts/desktop-split-final/Pertamina.Oto.Desktop.App.exe`.
Open Pisahkan PDF and choose a test file. Alternatively launch with `--open-split "<absolute PDF path>"` to go directly to the loaded workspace; this never starts processing automatically.

Synthetic sample: `artifacts/desktop-split-ui/split-test-98d88be59cf64a7c8895d59bd7ffd772/Contoh-8-halaman.pdf` (eight labeled pages, varied page widths). Final build was launched against this sample.

## UI validation and remaining acceptance

Computer Use inspected empty state and a loaded eight-page workspace with real thumbnails and all expected controls. That inspection found the CTA too low; final build reduces split heading/section overhead, uses side-by-side range inputs and independently scrollable settings. Final build succeeded and launched. User pressed physical Escape and stopped Computer Use before final visual recapture/interaction, so **full final GUI acceptance is pending**, not claimed PASS. File picker automation also could not target PickerHost; no private source was used.

Integrator/manual checks: final panel fit on narrow/short windows; mode switching; text-checkbox synchronization; add/remove ranges; enlarge preserves selection; invalid input feedback; processing/result buttons; folder picker cancellation/default retention; keyboard Save shortcut; back/forward drafts. Four-mode engine coverage is synthetic, not a fidelity guarantee for complex forms/bookmarks/signatures. Rendering and worker use existing development limits; 1000 one-page outputs may be slow but cancellation remains available.

Some prior test app windows may remain open; final executable path distinguishes this build. No old process/window was forcibly closed. Portable package and main branch were not updated. Final integration remains solely with the integrator after all five tasks and explicit user instruction.

## Writer release

All implementation, evidence and handoff writes are finished at this checkpoint. Release to UI 1 is sent after this file is saved. UI 2 must not write/build the shared checkout during UI 1's turn. Future fixes must reacquire coordinator's writer slot.
