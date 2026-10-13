# Merge hover / compact header handoff — 13 October 2026

Worker scope: approved Revision 1 and Revision 4 only; FEAT-PDF-005 / P2-MERGE-DEV-001. Shared checkout authorized by integrator. Branch `task/core-functional-first`, baseline HEAD `84cbb0f538020003bcfb6583e94c2fcf6524f87c`. Existing dirty files retained. No build, runtime mutation, dependency, version, install, feed or publication work performed.

## Baseline and changed paths

Existing source baseline SHA256: `MergeWorkspace.cs` = `0CB3903A046183634053E1E7B19CE2347B4B00C926F2C2E4A340053F34F03333`.
Before-source copy was made before apply_patch at `C:/Users/cahpo/AppData/Local/Packages/sandbox.{9d9e97e1-2a7a-45b5-952d-37e05133b7cd}/AC/Temp/oto-merge-hover-header-20261013/MergeWorkspace.before.cs`. Subsequent shell calls could not access that transient sandbox path; before-source also appears in initial full tool read. Do not claim durable backup or a completed no-index diff check. Both attempted diff commands failed with exit 1 because the transient copy was unavailable.

Changed paths, all under `src/Hosts/Pertamina.Oto.Desktop.App/`:

- `MergeWorkspace.cs`: replace card-local single rotate strip and local Small factory with `CreateMergeHoverActions`; retain existing rotate mutation; wire remove to `RemoveToolInput`; wire enlarge to existing `ShowReadPreviewDialogAsync`; replace large below-card remove button with full filename tooltip on name; remove old focus/pointer visibility handlers.
- New `MergeHoverActions.cs`: three native 44 DIP buttons; names/tooltips; guarded actions; hover plus subtree focus reveal with deferred LostFocus refresh.
- New `MergeCompactHeader.cs`: compact title/subtitle and existing favorite in shared Grid; exact amber development summary and exact expanded limitations; default collapsed. Call only immediately after existing Heading and favorite creation. Replaces Heading's two visual text rows while retaining its navigation/history effects.
- New `MergeWorkspaceRevisionAudit.cs`: runtime structural/accessibility/focus checks on integrator's synthetic loaded fixture. Not executed here.
- This handoff document.

Source result SHA256:

| File | SHA256 |
| --- | --- |
| MergeWorkspace.cs | 9B6125D157F821FF3F46DC91D5F79F98AEF9B5420A68E35099F6494466A5AF1C |
| MergeHoverActions.cs | B37EDE9EF4854A7900D08000CC7F303B3644510FF71C998A15F03F23EB20F093 |
| MergeCompactHeader.cs | AB5DD1EB1DAA8C223053925C9070B4CA71234504F35A8F5C3418258F2D734899 |
| MergeWorkspaceRevisionAudit.cs | E891C6F6DC2D3F6AE4E51E54C073F70E69DF62BF3B1A5CC770D25C4AB8CECDBA |

## Integrator-owned MainWindow.cs hunks

Keep current Heading call so its nonvisual navigation state remains applied. Immediately after favorite.Click registration replace:

```csharp
if (tool.Id is not ("pdf-jpg" or "split" or "remove" or "extract")) body.Children.Add(favorite);
```

with:

```csharp
if (tool.Id == "merge") BuildMergeCompactHeader(favorite);
else if (tool.Id is not ("pdf-jpg" or "split" or "remove" or "extract")) body.Children.Add(favorite);
```

Replace `if (tool.Id is "merge" or "pdf-jpg" or "remove" or "extract")` with `if (tool.Id is "pdf-jpg" or "remove" or "extract")`. Also replace downstream `else if (tool.Id != "split")` with `else if (tool.Id is not ("split" or "merge"))` so merge does not fall into generic warning. Header then occupies body[0] and limitations body[1]; existing processing insertion at index 2 remains consistent. Empty-screen route precedes these callsites and remains unchanged.

## Audit integration

Integrator may call `await AuditMergeWorkspaceRevisionAsync()` after loading synthetic inputs in its combined native audit and append lines to its report; FAIL lines must fail the audit. No standalone Program hook is necessary if existing combined fixture is reused. If Program invokes a new integration harness, use its approved fixture initializer before window activation; call this method only after loaded idle Merge has two synthetic files. Existing runtime test entry `--audit-merge-close-flow` already creates suitable fixtures; additional checks belong after `await Open("retained.pdf")` in its width loop, but that file is outside worker ownership and was not edited.

## Verification and limits

PowerShell source assertions executed with exit 0: 10 PASS checks (three actions, 44 DIP, no large remove, existing read preview, deferred focus, filename tooltip, collapsed default, no network helpers, retained drag/reorder, retained password thumbnail). These are source checks, not runtime acceptance. Worker constants inspected: PdfOperations.cs MaxBytes = 100 * 1024 * 1024, MaxPages = 1000; Merge input admission retains 50 PDF cap. No discrepancy with requested 100 MB / 1000-page notice; its shorthand does not enumerate the separate 50-input limit. No engine limits weakened.

No new package, file content logging or network use. Preview reuses existing reader/password dialog; it reads unmodified source orientation and says card rotation applies on merge. Existing thumbnail/password render, zoom/pinch wrapper, drag/order and original-file semantics retained.

Pending integrator: compile once after all handoffs; native focus/pointer/touch, read-dialog, rotate/remove/order regression, light amber/narrow layout and screenshots at 660/1400; source hashes before/after action; empty and close-save flow regression. Performance not measured; only three native controls per existing card added. No human decision blocker. This handoff does not claim phase completion or release readiness.
