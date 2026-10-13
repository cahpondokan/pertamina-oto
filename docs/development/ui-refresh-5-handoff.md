# UI 5 — Atur halaman PDF handoff

## Revision 2 — stable cards and live drag preview (current)

2026-09-15. Explicit exclusive writer granted by parent task for this revision. This section supersedes the original UI behavior and build/acceptance status below; older sections are retained as history. No dependencies, uploads, phase decisions, worker/protocol changes or final integration in this revision.

### What changed and why

The flicker came from `RenderOrganizerCards` clearing every grid item, recreating every Image and attaching another Loaded render on each operation. The board now has a stable `OrganizerPage.Id`, a dictionary of persistent card objects and a per-source/page bitmap-task cache independent of Loaded events.

- Cards show only thumbnail and result number. Source filename is a tooltip and part of the accessible name. Multiple input PDFs have stable source-colored borders; a single source uses a neutral border.
- Rotate/delete/enlarge are invisible until card hover or keyboard focus. Keyboard Tab order and accessible numbers follow the new visual order without reparenting cards. Duplicate and one-step move remain in the context menu and Ctrl+D / Ctrl+Left / Ctrl+Right.
- Blank insertion is a hover/keyboard-focus `+` in each gap, including start/end. The previous action toolbar and per-card blank buttons were removed.
- Pointer movement creates a 65%-opacity ghost sharing the existing bitmap, a destination placeholder, and 150 ms eased translations of affected cards BEFORE drop. A timer scrolls near the viewport edge.
- Preview order is a transaction separate from committed order. Drop assigns the committed model order and updates positions/numbers; it does not clear/rebuild the visual collection or reload images. Escape, pointer cancellation/loss, deactivation, unload or release outside the board cancels the transaction. State is cleared before releasing capture to avoid undoing a completed drop.
- Rotation changes only the target card's transform. Insert/delete reconcile only the added/removed identity and animate shifted cards. Existing roots, Image objects, ImageSource objects and scroll offset are retained. Undo of a deleted page may recreate that deleted card, while sharing its cached bitmap and retaining unrelated cards.
- Input, default/session output folder, progress/cancel, validated publication and result/back-edit flows remain. Session IDs are converted to the existing PagePlacement format at export and never sent in the worker protocol.

### Revision paths

- New `src/Hosts/Pertamina.Oto.Desktop.App/OrganizerPage.cs`: stable session identity and pure drag transaction.
- New `src/Hosts/Pertamina.Oto.Desktop.App/OrganizerBoard.cs`: persistent canvas/cards, bitmap cache, hover/focus actions, insertion gaps, animations and accessible numbering/tab order.
- New `src/Hosts/Pertamina.Oto.Desktop.App/OrganizerDrag.cs`: captured pointer, ghost, live placeholder/order preview, commit/cancel.
- New `src/Hosts/Pertamina.Oto.Desktop.App/OrganizerRevisionTests.cs`: Debug-only tests on the real WinUI thread.
- Revised `OrganizerWorkspace.cs`: input retained; imported/blank pages receive unique identity. Old rebuild and drag code removed.
- Shared `DocumentWorkspace.cs`: organizer model/history types, ScrollViewer board, simplified toolbar, duplicate creates a new identity, rotate updates only affected transforms.
- Shared `OrganizerExecution.cs` and `MainWindow.cs`: convert UI page identities to unchanged PagePlacement export.
- Shared `Program.cs`: Debug-only `--self-test-organizer <synthetic.pdf>` dispatch. Normal startup and prior flags preserved.
- This handoff. No edits to the four other feature implementations.

Revision snapshots/task diffs for the four modified preexisting files are under `artifacts/ui-refresh-5-revision/`. Program's revision change is only the self-test dispatch block. New files are their full new-file diffs. Older UI 5 evidence remains under `artifacts/ui-refresh-5-evidence/`.

### Verification / current build

- Build PASS: `artifacts/desktop-organizer-revision-final/Pertamina.Oto.Desktop.App.exe`. VS2026 MSBuild Debug, no restore or analyzer suppression. Log `artifacts/ui-refresh-5-revision/build.txt`.
- Desktop DLL SHA256: `924BC9120466AF8B9AAB21F99BAADC8F6B0329335A6506CE9ADF385256925BA5`.
- **2,871 pure model checks PASS**, all source/target permutations for 1–20 pages, committed-order immutability and stable object identity, blank duplication identity. Script/log `artifacts/ui-refresh-5-revision/model-tests.ps1` / `model-tests.txt`.
- **84 real WinUI checks PASS** on the final build: forward/backward/across-row preview/drop, cancel, ghost opacity/placeholder, animation targets, no committed mutation before drop, unchanged card/Image/bitmap references, no extra thumbnail requests, retained nonzero scroll, rotate, blank at start/middle/end, delete/undo/duplicate, accessible numbers and Tab order. Log `artifacts/ui-refresh-5-revision/winui-tests.txt`; runnable Debug source above. These directly exercise UI state and objects, not a screenshot-based smoothness measurement.
- Bundled worker regression **41 checks PASS**, original SHA256 unchanged; log `artifacts/ui-refresh-5-revision/worker-tests.txt`. Worker code unchanged by the revision.
- GUI observed with three synthetic PDFs: simple numbered cards, source-color legend/borders, hidden actions/gaps at rest, source tooltip and actions on hover. A real `sky.drag` moved the first page to visual position six across rows/sources; source color and numbering followed correctly. The reverse drag action's refresh timed out; do not claim its outcome from that action alone. The subsequent lightweight recovery also timed out.
- The last build change after that GUI check only corrects TabIndex ordering; final 84 WinUI checks were rerun. `git diff --check` passes (CRLF normalization warning only).

### Remaining acceptance / risks

GUI frame-by-frame smoothness, Escape from an actual held mouse gesture, touch/pen behavior, and complete final-result interaction remain for manual acceptance. Do not claim realtime visual smoothness has been measured: live preview state, animations and object retention are tested; one actual cross-row pointer drop is observed. All fixtures are synthetic blank PDFs, so bitmap identity is verified programmatically but rich-content visual acceptance is still useful.

The canvas retains the session's cards/bitmap cache (no virtualization); inherited 50-file/100 MB/1000-page caps remain, and high page counts can consume more memory. At the end of a shortened document scroll naturally clamps if the old offset no longer exists. UIA logical child traversal remains creation order, while accessible labels and keyboard Tab order follow visual order. Importing PDFs with passwords remains limited as described below.

Revision writer can be released after this handoff; final integration remains the integrator's responsibility after user instruction.

---

## Original UI 5 delivery (historical)

Date: 2026-09-15. Checkout: `worktrees/core-functional-first`.
UI 4 explicitly released the writer slot before this implementation. Existing uncommitted work was preserved. No agents, dependency additions, uploads, phase-gate changes, commits, production actions or final integration.

## Delivered behavior

- Existing empty input picker/drop/Explorer paste preserved. Loaded organizer adds PDF-only picker, paste and Ctrl+V, and external file drop onto the grid. Rejects UNC, unsupported/locked/corrupt files, excess files/bytes/pages. Local processing only.
- Each page has source/position labels and a colored border. The source legend uses matching colors, names and result counts. Three-source appearance checked in the running app. Colors use spaced hues; source labels remain the distinguishing cue for many files or color-vision limitations.
- Per-card rotate/delete controls become fully opaque on hover/focus, remain discoverable at reduced opacity otherwise, and have tooltips/accessibility names. Perbesar preserves the existing local preview.
- Drag from the page body to the left/right half of a destination card inserts before/after it, including across source PDFs. Pointer capture drives the final implementation; thick blue edge and text mark the destination. Near-edge pointer movement scrolls the grid. Toolbar arrows provide an alternate one-page move. File-drop handling remains separate.
- Add blank A4 after a particular card, after the selected pages, or at the end. Blank pages are real pages in the output, support rotation/duplication/removal/reorder, and participate in Undo/Redo.
- Save uses the existing default/session folder controls, source locks, worker process, validation and atomic no-overwrite publication. Input PDF is still required to begin; deleting every original while retaining a blank is supported.
- Processing replaces the editor with indeterminate progress and BATALKAN. Success only after WorkerClient validation/publication shows BUKA FILE HASIL, BUKA FOLDER HASIL, selectable/copyable name/page-count/size/location summary and KEMBALI EDIT. Failure/cancel restores the editor without success controls. Return preserves layout, output name and folder; password cleared after processing.

## Exact paths and shared audit

New:
- `src/Hosts/Pertamina.Oto.Desktop.App/OrganizerWorkspace.cs`: cards, local input, pointer drag, colors, blank insertion, insertion-index helper; Debug-only fixture opener.
- `src/Hosts/Pertamina.Oto.Desktop.App/OrganizerExecution.cs`: isolated organizer progress/result flow using WorkerClient.
- `docs/development/ui-refresh-5-handoff.md`: this handoff.

Modified shared paths:
- `src/Hosts/Pertamina.Oto.Desktop.App/DocumentWorkspace.cs`: organizer-specific toolbar/grid/import changes; organizer Ctrl+V routing. Home/preview implementations preserved. Original card builder replaced by feature-specific render dispatch.
- `src/Hosts/Pertamina.Oto.Desktop.App/MainWindow.cs`: one organizer-only early dispatch in ExecuteAsync. Existing unreachable legacy organizer branch retained to minimize unrelated diff.
- `src/Hosts/Pertamina.Oto.Desktop.App/Program.cs`: Debug-only `--open-organizer <PDF paths...>` fixture route; normal startup and other flags preserved. This imports locally, does not execute/export automatically and does not change default folders.
- `src/Workers/Pertamina.Oto.LocalPdf.Worker/PdfOperations.cs`: Arrange recognizes only Source=-1, Page=-1 with valid quarter-turn rotation as an A4 blank. Invalid negative indices still rejected.
- `src/Workers/Pertamina.Oto.LocalPdf.Worker/WorkerTests.cs`: blank geometry/rotation, three-source order, only-blank output and invalid sentinel checks.

Protocol.cs was NOT changed by UI 5. Blank sentinel semantics must be audited with the Arrange worker change. Four other feature implementations were not edited.

Immediate pre-edit snapshots and exact task-only patches: `artifacts/ui-refresh-5-evidence/*.before.cs` and `*.patch`. New organizer files are their complete new-file diffs. Do not restore shared files from HEAD. `git diff --no-index` exit 1 denotes expected differences.

## Verification

- Final VS2026 MSBuild `/t:Build /p:Configuration=Debug /p:DesktopUiOutputPath=<checkout>/artifacts/desktop-organizer-final/ /v:minimal /nologo`: PASS, no package restore or analyzer suppression. Log: `artifacts/ui-refresh-5-evidence/build.txt`.
- Actual insertion helper compiled and tested: **740 checks PASS**, covering every source/gap for 1–12 pages, explicit first/last/adjacent cases, and invalid input. Script/log: `artifacts/ui-refresh-5-evidence/order-tests.ps1`, `order-tests.txt`. The independent oracle inserts a marker in the original list before removing the original element.
- Worker synthetic smoke suite: **41 checks PASS**, including blank A4 dimensions/rotation, multi-source order, no overwrite and original SHA256 unchanged. Log: `artifacts/ui-refresh-5-evidence/worker-tests.txt`. Worker code unchanged after that successful run.
- `git diff --check`: PASS; CRLF normalization warnings only.
- GUI observed: initial input screen, loaded 9-page/3-source screen with distinct borders and matching legend, source/page labels, per-card actions, folder defaults. Clicking the first card's blank button inserted a blank at result position 2 and increased count to 10.
- GUI drag attempts exposed ineffective native grid drag. The final build replaces this with explicit captured-pointer handling. **Final pointer drag has NOT been accepted in GUI:** subsequent Computer Use activation timed out, and recovery reported `Computer Use helper already has an active request`. Do not describe drag as GUI-verified solely from the 740 insertion-index tests.

Final build: `artifacts/desktop-organizer-final/Pertamina.Oto.Desktop.App.exe`.
SHA256 Desktop DLL: `94B5ABBF0155E585CC2EF36103C28FB3897D7227BDE6B6A11ECA2001D82E2672`.
SHA256 Worker DLL: `AE6DBA8130A4F609588DA0BE217E28D03C65B00F0D7EF8BD3B9A78BD04D5951E`.
Synthetic fixtures retained as `artifacts/ui-refresh-5-evidence/Contoh-{A,B,C}.pdf`.

## Remaining acceptance and limitations

Implementation and automated build/engine/order checks complete; interactive acceptance remains partial. Verify final mouse drag both directions/across rows/sources, edge scroll and release outside target; rotate/delete/Undo/Redo; external drop/Explorer paste; narrow window and many pages; result save/open/copy/Back to Edit; cancel/failure recovery. Touch-drag acceptance is not claimed. Native OS file picker could be displayed but not automated because PickerHost is a separate target process.

Blank size is A4 only. Import requires readable unencrypted PDFs; locked input reports a limitation. Inherited caps remain 50 sources, 100 MB and 1000 pages; worker also checks total source pages. Undoing import retains source provenance in the legend with zero result pages. Preview work is serialized by the existing renderer and can be slow near the page cap. Save controls may require vertical scroll on short windows. Global menu/navigation lifecycle across all five features needs integrator audit.

No runtime success is claimed for untested result interactions. This is a local development build, not phase completion, production approval or final integration. Integrator task `01a0a411-3d77-7871-8e60-7f656fb93d2a` may review this handoff; final integration still requires the user's explicit instruction.
