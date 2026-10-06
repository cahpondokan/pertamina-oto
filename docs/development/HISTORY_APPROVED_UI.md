# Riwayat — approved interactive design, 17 September 2026

Scope: user's approved `oto-riwayat-presisi.html` prototype, translated to the existing native WinUI application. No WebView, browser runtime, network requests, worker changes, library schema migration, original-document mutation, or phase promotion. Existing dirty changes were preserved.

## Design contract

- Native sidebar and original owl identity retained. Riwayat is highlighted, including programmatic navigation.
- Dedicated constrained page grid, not a StackPanel nested in the generic page scroll. Duplicate breadcrumb is hidden only on History.
- White 94-DIP page header, title/subtitle left, Offline and Buka PDF right.
- Full-width default details view with first-page thumbnails, filename/tool, Halaman, Ukuran, Waktu proses. Timestamp means processing completion, not last opened.
- Search, tool filter, sorting and icon-only card/detail controls at the top; no repeated per-row action buttons except overflow.
- Detail rows 88 DIP. Selected document has pale blue background and 3-DIP blue left edge.
- Preview split: left 36%, clamped to 300–460 DIP; fixed document header on right, gray reading canvas, bottom centered page/zoom bar. Under 650 DIP content width, reader temporarily occupies content width; close restores list.
- At normal preview widths, search/filter/view controls share a single row. Under 360 DIP list width, search is above filter/view controls. Sort stays in the full-width library view.
- Default fit uses both PDF dimensions and live viewport size. Explicit zoom survives page changes; Fit restores automatic resize fitting.
- Library native pinch changes card size/detail density, separate from PDF native pinch. Explicit buttons remain available. Card identity survives zoom and split/close.
- Thumbnail rendering is queued only for visible/nearby rows using the existing bounded rendering gate. Late file opens cannot override navigation; replacing/closing readers releases UI event closures.
- File paths/passwords are not included in new audit logs. UI evidence uses only generated A4 test reports.

## Icons / asset admission

User explicitly approved the Lucide set used in the interactive prototype. Reused **static SVG geometry only** from the locally bundled `lucide` **1.8.0** package in the Codex runtime (`dependencies/node/node_modules/lucide/dist/cjs/lucide.js`). No package installation, download, executable code, or runtime library was added to OTO.

- Source package declares ISC; bundled license also includes MIT notices for Feather-derived icons. Full notice is shipped as `Assets/History/LICENSE.txt`.
- SVGs are 24×24 viewBox, 1.7px rounded strokes, local fixed ink color; plus-white is the same plus geometry in white for the primary action.
- Icons: search, plus, layout-grid, list, file-text, wrench, folder, maximize2, minimize2, x, chevron-left/right, minus, scan, ellipsis-vertical, history, scan-eye, zoom-in. The application shell's already-established native icons are unchanged.
- All assets are local paths. Inspected as SVG primitive geometry without scripts, event attributes, links, foreignObject, external references, or embedded media. No new executable dependency or supply-chain service.
- Icon-only actions expose AutomationProperties.Name and normal hover/focus tooltips. Root Ctrl+V hint suppression remains unchanged.

## Verification and evidence

Build: Visual Studio 2026 MSBuild, Debug, existing restored dependencies, no restore or analyzer suppression. Output: `artifacts/desktop-history-approved/`.

Native audit: `Pertamina.Oto.Desktop.App.exe --audit-history-ui synthetic`.

Evidence: `artifacts/desktop-history-approved/history-ui-audit/audit.txt` and PNG captures 01–07. Captures use WinUI RenderTargetBitmap of the actual XAML root, not the HTML prototype. Native title/menu chrome is outside that capture. Read the final audit file for exact checks and outcome.

Final result (17 September 2026): **35 native History UI assertions PASS**, **15 existing flow assertions PASS**, **4 isolated library persistence assertions PASS**; all three processes exit 0. Final build exit 0, analyzers enabled; `git diff --check` exit 0 (pre-existing CRLF normalization warning only). UI DLL SHA-256: `1E25F94AA6551C65CC3C27088F78ABE131A3EC20D69E626BDA21943AE56F5323`.

Visual review of real captures corrected: collapsed-column spacing in narrow lists, overflow button placement in card mode, top alignment for short lists, single-row toolbar at normal preview width, active sidebar indication, thumbnail boundaries, and muted-text contrast. Reviewed final split capture against approved HTML: header/toolbars, 36% left panel, thumbnail/name/metadata rows, active blue edge, centered fit-page canvas, and bottom controls match the agreed structure. Native Windows font rendering/focus rings and the retained original sidebar differ intentionally from the HTML shell; this is not a pixel-identical browser renderer claim.

Static asset scan: 19 SVGs, zero script/foreignObject/event-handler/data or link references. The 54 assertions are technical checks, not full UAT or accessibility certification. The portable ZIP and previously running builds are not replaced by this development output.

Audited states: initial details, split reader, expanded reader, card grid, 1024×768 window, 640×720 window. Fixtures are generated reports with text/tables; input file hashes checked unchanged. Tests use an isolated library path, never the user's real saved history.

Remaining acceptance: physical precision-touchpad gesture feel, end-to-end Tab traversal/screen-reader use, password dialog with representative protected PDFs, OS picker/folder interaction, and user visual approval. Automated focus checks verify controls accept keyboard focus, not a full keyboard accessibility certification. High zoom remains raster-based. This is not Production/UAT sign-off or a claim all application crashes are resolved.

Earlier history implementation notes in HISTORY_READER.md are superseded for layout by this document.
