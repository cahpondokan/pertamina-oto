# Shared source/result previews — 2026-09-09

Agent0, existing core-functional-first worktree. Partial implementation of approved all-active-tool preview plan; not full simulation completion.

SourceWorkspace.cs adds shared source selector (basename, not raw path), add/paste, remove, page navigation/zoom via PreviewPanel and reload for authorized password input. Image inputs have ordering buttons and clearly labeled original-image preview. Existing Merge and Organize visual workspaces retained. MainWindow routes other available tools to shared preview; successful processing adds saved-result preview and open-folder actions for all tools. ZIP results are explicitly not previewed internally; extract and inspect PDFs. DocumentWorkspace renderer supports supplied passwords without adding unlocked documents to shared cache. Passwords remain in view memory, not logs/navigation snapshots. Result preview supports encrypted results via user-entered password; dialog errors caught.

No new dependencies, uploads, worker changes or release claims. Existing native renderer serializes loading, limits PDF size/pages and renders bounded-size images. Source navigation does not select processing pages: clearly labeled source-only. Existing right-side page ranges still govern operations.

Verification: Development.slnx VS MSBuild Debug with absolute DesktopUiOutputPath artifacts/desktop-previews/. Initial build exit0; final status recorded in task. Worker self-test 34 checks PASS, sources unchanged; synthetic evidence C:/Users/cahpo/AppData/Local/Temp/oto-functional-test-37e4628a0f574b048b5056875cac4189. git diff --check exit0 (existing newline warning). These do not prove visual acceptance or password-render fidelity.

Pending: GUI runtime/keyboard/resize checks; per-page visual selection for split/remove/extract; rotation/crop/watermark/number simulations; image A4-layout simulation; compression before/after comparison; ZIP internal preview. Earlier crash report not resolved. Manual smoke: open each active tool with synthetic source, switch pages/zoom, replace/remove, use authorized encrypted file, process then inspect saved preview and open folder. No phase gate promoted.
