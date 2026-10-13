# Desktop menus — 2026-09-08

User requested adapting Windows application menus from supplied screenshots to OTO. Agent0, existing development worktree and prior dirty changes preserved. Changed MainWindow.cs and added DesktopMenu.cs; no worker, dependency, OS or network changes.

Implemented native MenuBar above existing workspace: Berkas (open/add, process/save using existing explicit operation, open last result, confirmed exit), Edit (document undo/redo, file paste, settings), Tampilan (compact sidebar, home, search, favorites, session history), Bantuan (guide, shortcuts, issue report, about). Ctrl+O/F/B and F1; existing text-edit and document Ctrl+V/Z/Y behavior retained. Menu commands guard active worker/import. Save command requires loaded available-tool screen and existing validation. No fake terminal/browser/update/account features.

Build: offline VS MSBuild Development.slnx Debug with DesktopUiOutputPath=<worktree>/artifacts/desktop-menu/, exit0. git diff --check exit0. GUI menu/shortcut acceptance not yet exercised; no claim of runtime UI PASS. No engine tests repeated for this UI change.

Limitations: menu exit confirmation only; native title-bar close retains existing behavior. Session drafts not persisted. Sidebar automatically compacts on narrow windows. No back/forward route stack, full-window zoom or fullscreen added. Menu is below native Windows title bar, not a custom title-bar replacement. Latest executable artifacts/desktop-menu/Pertamina.Oto.Desktop.App.exe. Full phase/release status unchanged.
