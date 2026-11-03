# Shared browser PDF components

The browser workspace uses composition: small objects own shared UI behavior and call operation-specific ports. The PDF worker remains the authority for parsing, capacity, security and output validation. A UI profile never grants execution capability.

## Components and ownership

| Change needed | Single owner |
|---|---|
| Tool title/icon/category and browser presentation capabilities | `src/Hosts/Pertamina.Oto.Web.Preview/Models/ToolCatalog.cs`: `ToolDefinition` and immutable `BrowserToolProfile` |
| Input counts, byte/page/image budgets, output budgets and timeouts | `wwwroot/js/pdf-capacity.json`, consumed by `PdfCapacity.cs` and `pdf-capacity.js`; `BuildPdfCapacity.targets` generates the classic-worker data envelope for both hosts |
| Header/navigation buttons | `Views/Shared/_OtoHeader.cshtml`, linked into both hosts |
| CSS/JS inclusion and load order | `Views/Shared/_PdfWorkspaceAssets.cshtml`, linked into both hosts |
| Palette and shared visual controls | `wwwroot/css/oto-theme.css`, `oto-components.css` |
| Lifecycle, active listeners and disposal | `wwwroot/js/pdf-workspace-ui.js`: `WorkspaceController` |
| Picker/keyboard/drop/paste admission to the UI | same file: `FileInputController` |
| Native text-vs-file clipboard rules | `wwwroot/js/pdf-paste.js`, composed by `FileInputController` |
| PDF reason codes and compatibility notices | `wwwroot/js/pdf-document-messages.js`: `DocumentMessages`, operation-specific profiles |
| Shared heading, empty state, process and result markup | `_PdfWorkspaceHeading`, `_PdfDropZone`, `_PdfProcessPanel`, `_PdfResultPanel` with the records in `Models/PdfWorkspacePresentation.cs`; linked into both hosts |
| Progress, filename validation, save/download, result notices, narrow-screen settings | `wwwroot/js/pdf-workspace-flow.js`: `ProgressController`, `ResultController`, `SettingsController` |
| Rendering, zoom, virtual pages and raster cache | `wwwroot/js/pdf-preview.js`: shared preview service |
| Stateless page order/readiness/input mapping | `wwwroot/js/pdf-workspace-model.js` |
| Actual execution | `pdf-job.js`/`pdf-merge-job.js` and their isolated workers/engines |

All JavaScript paths above are relative to Web.Preview. Downloads links these sources instead of copying an independently maintained implementation.

## Controller contract

Both `OtoMergeMount` and `OtoBrowserMount` return a `WorkspaceController`. The object owns `activate`, `deactivate`, `dispose`, `listen`, `use`, `busy`, `hasWork`, `resources` and `beforeLeave`. Each workspace provides its operations through callbacks. Merge retains file-backed staging; smaller tools retain their bounded memory model. These different lifetimes are intentional.

Register shared/global listeners with `controller.listen(target, event, handler)`. Register disposable services with `controller.use(cleanup)`. Inactive/disconnected workspaces ignore events. Disposal removes listeners once and attempts every cleanup even if one throws. Keep async cancellation/generation checks in the operation adapter, where the corresponding job lives.

Route file selection through `FileInputController`. Its `pick` and `receive` methods protect busy work, open dialogs and existing results for picker, drop and paste alike. Do not add another per-tool global paste/keydown listener. File extensions in `ToolProfile` are only an early UI filter; worker checks remain required.

`BrowserToolProfile` is serialized by Razor into the workspace's escaped `data-tool-profile` attribute. `ToolProfile` validates this presentation schema in the browser. Navigation reads the same profile for source transfer and multi-source selection. There is no second hard-coded browser-enabled tool list in JavaScript.

## Extending a tool

1. Add the catalog entry with the appropriate browser profile only once its actual execution capability is admitted and tested. A catalog entry alone does not implement a processor.
2. Supply tool-specific settings and execution options in the operation adapter and Razor view. Reuse the existing workspace controller, file intake, preview and theme.
3. Add messages for new reason codes in the shared message catalog. Different operation budgets may need different wording; never reuse Merge's 4 GB limit for a smaller worker.
4. For genuinely different behavior, inject a dedicated service/strategy instead of adding a chain of subclasses or copying the whole controller.
5. Verify file/text paste, detached workspace behavior, unsaved results, cancellation/disposal, offline execution, output correctness and responsive keyboard UI. Both hosts must build and serve the shared assets once in dependency order.

## Scope

This is the reusable foundation for ten existing browser tools. All ten admitted browser tools share heading/dropzone/process/result partials, flow objects and the continuous PDF reader. The nine bounded tools use _PageWorkspace with per-tool options; Merge keeps its staging-backed adapter. Pure PDF transformations remain functions where no object lifecycle is needed. The C# application already has service/interface boundaries (`MergeService`, `IMergePreflight`, `ILocalMergeProcessor`, `IMergePublication`); Desktop `MainWindow` partials have not been rewritten by this browser task.

## Shared flow ports and Organize

`ResultController` receives ports for the current result (`file`, `name`, `count`, `warnings`), rename, cleanName, messages, busy state, preview, notification, render and successful native save. It owns the download URL and abort signal. It never assumes OPFS: Merge supplies its staged File; Organize supplies its bounded Blob. Only a completed native write/close calls `saved`; cancellation/failure retains the result and exposes direct download. A download notification is not a successful-save receipt. The owning controller disposes/reset this component when releasing its result.

`ProgressController` renders actual worker messages; unknown-duration writing is indeterminate. `pdf-job.js` distinguishes progress messages from terminal success/failure and ignores messages after cancellation. The engine emits progress after the same validation/copy/verification work, without relaxing its guards.

Organize and Split use `data-workspace-layout="shared"` in `_PageWorkspace`; `_OrganizeWorkspace` is a compatibility partial forwarding to that shared page shell. `browser-pdf.js` remains the bounded operation adapter; its page/source identities and `pdf-workspace-model.js` supply the editing order. No separate organizer file-input handler or PDF engine exists. Organize's maximum remains 10 files, 10 MiB/file, 30 MiB total and 200 pages; tagged/annotated duplicate restrictions remain authoritative in the engine.

Pass `{continuous:true}` to the shared preview service (Merge's existing `{merge:true}` option remains compatible). A card can supply a fourth reader port `{source, index}`. A virtual source has ordered `pages: [{source, index, rotation}]` and `count`; the reader resolves every page independently. It adds reading-only rotation to the individual edit rotation. Per-page CropBox geometry and inherited PDF rotation determine spacing, including landscape pages and differently rotated duplicates. The renderer keeps bounded raster/cache, cancellation and cleanup behavior. This metadata is presentation data, never an execution authorization receipt.

The controller keeps Organize source/result state across local tool navigation; `deactivate` closes its reader. New input is blocked while the output exists. Starting over requires confirmation. The remaining seven browser tools also use this shared visual design; their workers and budgets remain unchanged. See `pdf-organize-shared.test.cjs` for content/resource/order/save/lifecycle integration checks and `worker-offline.test.cjs` for real worker regression.


## Multiple result files (Split)

Split uses the same page shell, theme, file intake, progress, continuous reader, settings and result panel. `PdfFlowViewModel.Multiple` enables the result selector; single-result tools omit it. `ResultController` accepts optional `results` and `select` ports. It validates unique names within the result set, recreates only the selected download URL and keeps each result name/save status independent. The bounded adapter retains all result Blobs and counts their total bytes against the session budget. Changing the selected part renders that PDF in the shared reader.

A successful native write/close marks only that part saved. Other parts and sources stay available; once every part is confirmed saved, the workspace releases them. Download-start is not a saved receipt. Cancel/failure, navigation and blocked new input preserve the entire result set; confirmed New releases it. Renaming a previously saved part makes that new name unsaved. No ZIP, bulk-download protocol, second PDF engine or provider was introduced. Split retains one PDF / 10 MiB / 200 pages / 2–20 exhaustive, non-overlapping parts, enforced by the existing engine.

`pdf-split-shared.test.cjs` exercises actual controller/engine output, invalid partitions and retry, maximum output count, selected-part save, naming collisions, partial save/failure, lifecycle and progress. Real browser QA additionally verifies picker, reader, responsive layout and both downloaded PDFs.


## Full template coverage and readable scale (26 October 2026)

`_BrowserWorkspace` dispatches Merge to its adapter and all other admitted tools to `_PageWorkspace`. `PdfToolPresentation.For` supplies specific headings and input copy; `PdfCapacity.For` supplies capacity from the shared manifest. `_PdfToolSettings` preserves each operation's existing option IDs and `_PdfToolSupport` retains its supported-input and privacy notices. `_PdfDropZone` receives the catalog icon and input label; it displays a large existing Biblo asset for PDF and image intake.

`oto-comfort.css` is loaded after the shared component styles in both hosts. Main explanatory text and form controls use 16px, primary buttons are at least 48px, and the empty-state icon is 160px (128px on short laptops, 120px on narrow screens). Empty states grow with their content instead of overflowing a fixed-height box. At narrow widths, settings use the same dialog and loaded-state headings omit the benefit row to keep output names and actions visible. Browsers with enlarged zoom may need normal vertical scrolling; no browser zoom or OS display setting is changed.

Images keep their normalized input thumbnails; generated PDF results now use the same PDF preview and ResultController. Native-save completion, cancellation, fallback and cleanup follow the existing shared contract. `pdf-tools-shared.test.cjs` checks the six newly migrated PDF tools through actual engine output and simulated native writable ports; browser QA additionally covers all ten processors and real image conversion. No processor, admission limit or cloud provider is enabled by this presentation change.
