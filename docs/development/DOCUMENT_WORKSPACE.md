# Document workspace — 2026-09-08

Authority: `DOCUMENT-WORKSPACE-001`, user requested implementation of all approved screen designs. Agent0 only. Baseline `84cbb0f538020003bcfb6583e94c2fcf6524f87c`, branch `task/core-functional-first`; preserved existing UI edits. Scope: Desktop.App UI, existing worker Organize/protocol, synthetic tests, development and coordination notes. No new dependency, cost, service, Sandbox, main promotion or phase completion.

## Implemented

- Shared native B3-inspired owl/OTO vector brand and existing navy/pastel theme across screens. Native interpretation, not pixel-identical reproduction.
- Empty three-path home; single-file large preview and recommendations; multi-file adds a checkbox list, reorder and removal from session. Preview focus is independent of checkboxes.
- Built-in Windows PDF preview and JPG/PNG rendering, previous/next page and size controls. Clear failure message instead of fake preview.
- Picker/File Explorer clipboard, Ctrl+V, session undo/redo, Ctrl+Z/Y outside text fields. Alias search and recommendations carry checked inputs into tools.
- Visual Organize thumbnail grid, source/page IDs, selection, drag reorder, accessible arrow buttons, rotate, duplicate, exclude, undo/redo, large preview, source list, output folder/name, create new PDF, cancel/open result.
- Optional PagePlacement(Source, Page, Rotation) worker request extends actual Organize to multiple sources. Validates indices, rotation and limits; retains permission checking and existing atomic new-name publication. Sources never saved over.

## Verification

VS MSBuild `Pertamina.Oto.Development.slnx /t:Build /p:Configuration=Debug /p:DesktopUiOutputPath=<worktree>/artifacts/desktop-workspace/ /v:minimal /nologo`: exit 0, offline/no restore. No analyzer suppression.

Worker `--self-test`: **30 checks PASS**, exit 0. Includes multi-source layout, independent duplicate rotations, invalid source/page/rotation/empty layout, and original SHA256 unchanged. Synthetic evidence directory: `C:/Users/cahpo/AppData/Local/Temp/oto-functional-test-e22b2e0d24424da29e74b4be84cf541e`.

WorkerClient smoke executable: **21 checks PASS**, exit 0. Atomic save, existing target/source rejection, wrong password, pre-start cancellation, malformed PDF and staging cleanup. Evidence: `C:/Users/cahpo/AppData/Local/Temp/oto-client-test-58a7615e775645ecb9919da488398cf0/results.json`. Existing integration tests use legacy Organize requests; new visual layout tested in worker self-test, not dedicated GUI E2E.

`git diff --check`: exit 0. Build launched via Computer Use; empty home, logo and catalog observed. Synthetic picker automation stopped on detected user activity; refreshed state showed user on catalog. No further clicks. Single/multiple previews, Organizer drag/save, narrow-window visual acceptance remain **manual verification pending**.

## Limits

Development build, not complete product/phase or pixel-perfect acceptance. Assistant/other unavailable engines remain inactive. PDF/JPG/PNG only; no Office preview or bitmap clipboard. Undo/session history not persistent; maximum 30 undo operations (history clears at limit). Organizer keeps source-list entries after excluding/undoing their pages.

Windows PDF preview parses in the host, not a hard sandbox. One render at a time, at most two cached PDF objects, 220px thumbnail/850px preview width, 100MB per-file and 1000-page checks; these are not hard memory/time guarantees. Use benign test copies only. Complex fidelity, encrypted preview, externally changed files, preview cancellation, measured performance, accessibility and responsive acceptance remain to be hardened. No fresh network-disabled proof; code contains no upload/network request.

Manual acceptance: add one benign PDF and navigate preview; add second, check files, search `satukan`; Organize reorder/rotate/duplicate/exclude/undo; save under a new name and inspect output/originals. Compare UI against approved images.

Build: `artifacts/desktop-workspace/Pertamina.Oto.Desktop.App.exe`. Existing user app instances not closed. Main unchanged.
