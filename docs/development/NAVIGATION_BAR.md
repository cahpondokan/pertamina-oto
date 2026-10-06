# Navigation bar implementation — 2026-09-08

Agent0, user-approved extension of DESKTOP_MENU.md. Existing task/core-functional-first changes preserved. UI-only paths: MainWindow.cs, DesktopMenu.cs, DocumentWorkspace.cs, DesignedHome.cs, new NavigationHistory.cs. No worker/dependency changes or production authority.

Implemented visible sidebar/back/forward buttons with accessible names and shortcut tooltips. Alt+Left/Right traverses up to 50 screen entries; new navigation truncates forward history. It is separate from document undo. Tool input/order/options are snapshotted for the session, excluding passwords. Existing staged home and organizer page lists persist; organizer output name preserved. Explicit home input transfer replaces rather than duplicates saved tool inputs.

Added Ctrl+Shift+S for explicit process/save, Ctrl+A/Esc for selection, Ctrl+numeric-keypad +/- and Ctrl+0 for available preview slider, Ctrl+/ shortcut guide. Existing text editing retains its normal control behavior. Menus guard active processing/import; navigation travel also guards running operations. Title-bar close/Alt+F4 now cancels close during processing/import and requests confirmation when session data may remain. This conservative warning is not a precise saved/dirty document tracker.

Verification: offline VS MSBuild Pertamina.Oto.Development.slnx /t:Build /p:Configuration=Debug /p:DesktopUiOutputPath=<worktree>/artifacts/desktop-navigation/ /v:minimal /nologo exit0, no restore. git diff --check exit0. Runtime UI navigation, focus/shortcut handling, dialog close, selection and draft-restoration acceptance NOT yet executed; do not report E2E PASS. No PDF engine tests rerun for UI-only change.

Remaining polish: no persistence across restart; catalog query/scroll position not snapshotted; preview menu zoom acts on first visible workspace slider, not an open modal; keypad +/- shortcuts rather than every keyboard-layout-specific plus/minus mapping. Contextual organizer toolbar still uses existing controls instead of fully hiding all irrelevant actions. Precise dirty-state/status and complete tooltip coverage remain to refine. No phase/Production completion.

Build to open: artifacts/desktop-navigation/Pertamina.Oto.Desktop.App.exe. Existing running windows not closed or overwritten.
