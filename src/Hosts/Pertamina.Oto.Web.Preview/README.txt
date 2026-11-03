PERTAMINA OTO WEB — LOCAL FRONTEND PREVIEW
Task: WEB-MVC-UI-20261016

PURPOSE
Interactive frontend for design review, built with C# ASP.NET Core MVC/Razor,
local CSS, and vanilla JavaScript. This is a separate host, not a desktop update
or a replacement for the public download portal. Not a Production build and
not evidence of full Pertamina compliance or Phase 8 completion.

START
Prerequisite: existing .NET SDK 10.0.400 (repository global.json).
In PowerShell, run this folder's Start-Preview.ps1.
Alternatively, double-click Buka-Pratinjau.cmd to open the preview terminal.
The script restores only installed framework references, builds this project,
then serves http://127.0.0.1:5266/. Open that address after the server starts.
Use -Port 5267 if 5266 is occupied. Press Ctrl+C in that terminal to stop.
NuGet.Config clears package feeds. No third-party package is installed.

WEBSITE HOMEPAGE REVIEW MODE (2026-10-16)
Run Start-Preview.ps1 -Website for the proposed oto.ayani.id catalog homepage.
All 18 PDF tools are marked unavailable in this mode. Search, categories,
favorites, theme controls and explanatory dialogs remain interactive.
Workspace routes return 404: no PDF selection or processing is offered.
Download buttons use the public desktop release snapshot verified on 2026-10-16
(build 20261014-002535-877d8390). These are real downloads, not simulations;
they do not automatically install the application. Publication is on hold
pending the user's local review. No public website/feed has been changed.
The checks listed below originally apply to the default interactive mode;
the catalog-mode evidence is recorded separately in the evidence directory.

IMPLEMENTED UI
1. Shared navy header with horizontal navigation, no sidebar or mobile drawer,
   light/dark themes, responsive full-width content layout.
2. Catalog of 18 PDF tools, category/search filtering and browser-local favorites.
3. Merge and Organize workspaces, sample files, stable source/page identifiers,
   source list on the right, keyboard-accessible actions and undo/redo.
4. Confirmation on tool navigation and close; clear simulated result dialog.
5. Shortcut tooltips only: Ctrl+O, Ctrl+V, Ctrl+Z, Ctrl+Shift+Z.
6. Other tools clearly marked unavailable; their cards explain this on click.
7. Existing Biblo icons copied unchanged from output/imagegen/biblo-feature-icons-v1.

BOUNDARIES / PRIVACY
All HTML routes are GET-only; POST is refused and there is no processing API.
The host accepts loopback clients only. All assets are local. The CSP disables
fetch/XHR/WebSocket and form submission, and forbids external asset sources.
This is NOT an authentication/security architecture for a public service.
Data Protection uses an in-memory repository for this preview only.

Picking/pasting/dropping a file reads browser-supplied NAME and SIZE only.
It does not read bytes, parse PDF, count actual pages, render actual PDF contents,
upload files or create an output PDF. Actual files show a generic placeholder.
Only built-in synthetic documents demonstrate page actions and result layout.
Filename/type/size checks are UI convenience, not validation of PDF content.
Metadata/synthetic order is kept in this browser tab's sessionStorage.
Theme and favorite IDs use localStorage. Closing documents clears the workspace.
Undo history is intentionally not carried between pages and the dialog says so.
If browser storage is unavailable, saved preference/session restoration may fail.

FOLLOW-UP BEFORE FUNCTIONAL WEB RELEASE
Confirm official corporate web/brand guidance and approved runtime baseline.
Integrate capability-aware PDF processing, secure file lifecycle and validation.
Add approved identity/SSO/roles, Web API boundary, audited persistence,
observability/APM, enterprise controls and required human approvals.
Optimize large source icon rasters through an approved asset pipeline.
Test real file picker/paste/Explorer drag, supported browsers, accessibility,
network failure, input security and processing correctness. Do not infer those
gates from the frontend checks below.

CHECKS RUN ON 2026-10-16
- dotnet restore with local NuGet.Config: PASS, no external package sources.
- dotnet build --no-restore: PASS, 0 warnings, 0 errors.
- node --test tests/workspace-model.test.cjs: 9/9 PASS.
- PowerShell tests/Verify-Http.ps1: 31/31 PASS (routes, headers, negative routes,
  POST rejection and all 22 static assets).
- Browser: search, categories, unavailable tool dialog, light/dark, favorites,
  sample reorder/undo/duplicate, stable source IDs, focus preservation,
  result simulation, invalid output name, navigation cancel/continue,
  close cancel/confirm and mobile menu: observed working.
- Visual: desktop 1280x720 and narrow 390x844; no horizontal page overflow.
- No browser console errors observed during those checks.
- Native file transfer, real PDF processing, SSO, all-device coverage and formal
  accessibility/security compliance were NOT tested or claimed.

EVIDENCE
Workspace-root artifacts/web-mvc-preview-20261016 contains scope, verification
summary and numbered browser screenshots. Preview is independently runnable;
no solution-wide integration, commit, desktop installation or web publication
was performed as part of this frontend task.
