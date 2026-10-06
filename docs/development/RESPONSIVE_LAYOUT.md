# Responsive layout — PERTAMINA OTO

4 October 2026 (Asia/Jakarta). Status: COMPLETE_LOCAL_DEVELOPMENT_DELIVERY. Scope layout and local packaging gates passed. Installed-user activation and physical UAT remain separate.

## Ownership and baseline

Only Agent 0 ran this scoped task; no additional agent/chat was created. Checkout `worktrees/core-functional-first`, branch `task/core-functional-first`, baseline HEAD `84cbb0f538020003bcfb6583e94c2fcf6524f87c`. Existing dirty work is preserved. Zoom thread `01a1040b-d0e7-79d3-812f-4497367086eb` completed source/build/package handoff; its final package `20261004-070509-9021ecc9` and `ZOOM_CONSISTENCY.md` were verified before shared writes/builds.

Evidence root: `artifacts/responsive-layout/run-20261004`. Fresh pre-layout snapshots/hashes: `source-baseline/`, `baseline-hashes.json`, `baseline-status.txt`, `baseline-head.txt`, `zoom-package-latest.json`. Own paths/diff: `owned-paths.txt`, `own-diff.patch`. No unexpected change to baseline app files outside owned paths was found. Engine, updater behavior, product version, central phase records, installed app and OS settings are outside this task.

## Implemented behavior

- Shell/card columns use actual content width after sidebar and padding; breakpoints avoid rebuilding columns at every pixel. Breadcrumbs, badges, menus, intake/actions and workspace headers reflow.
- Home/catalog/favorites/storage/assistant/admin/help/settings retain approved visuals and vertical scrolling. Storage/assistant forms no longer keep a fixed 850-DIP column wider than their viewport. Unavailable capabilities remain unavailable.
- Active PDF/image/Word workspaces and results retain their processing contracts. Action rows wrap in place, preserving element instances/order. Empty intake button minima no longer force narrow overflow. Result folder paths trim with full tooltip.
- Reader retains document zoom and continuous reading. Narrow thumbnail rail is a reachable inline overlay; search and action controls reflow. Short reader header becomes one compact row with metadata available through the title tooltip. Footer can scroll. Empty-reader reactivation and successful document attachment clear suspended state, preventing landing-away-open rendering from waiting on itself.
- Editor toolbar/footer reflow; short windows expose tools through “Alat edit”, narrow thumbnails and properties remain reachable. Resize preserves draft, selection, page ownership and undo/redo. No PDF coordinate or export algorithm changed.
- History keeps sorting reachable, supports dense vertical lists and bounded preview headers. Read-preview/license dialogs are bounded to viewport and scroll. Update completion presentation is bounded; activation/updater behavior is unchanged.
- Shared zoom percentage, actual-scale, text-input guard and fit contracts are retained. Fit button uses the short caption “Sesuaikan” with the current option in its tooltip.

## Verification matrix

DIP layout, physical display scale, Windows text scale and document zoom are distinct. Fixed-root cases below are simulated WinUI content sizes, not proof of hardware DPI or a particular display. Heights conservatively cover reduced available space. Native actual window evidence records pixel size, root DIP size and RasterizationScale separately.

| Representative display / scale | Simulated root DIP | Status |
|---|---:|---|
| 1366×768 / 125% | 1093×500 | PASS layout |
| 1600×900 / 100% | 1600×820 | PASS layout |
| 1920×1080 / 150% | 1280×650 | PASS layout |
| 1920×1080 / 200% | 960×410 | PASS layout |
| 1920×1200 / 125% | 1536×880 | PASS layout |
| 2560×1440 / 175% | 1463×748 | PASS layout |
| 2560×1600 / 250% | 1024×565 | PASS layout |
| 3840×2160 / 200% | 1920×990 | PASS layout |
| Portrait / 150% | 720×1180 | PASS layout |
| Ultrawide / 100% | 3440×1360 | PASS layout |
| Narrow/restored content | 480×460, 640×340 | PASS layout |
| Loaded reader/editor, dense history/results | 480/640/960/1280 widths and short heights | PASS layout/state |
| Native restored window at scale 1 | 800×640 pixels → 784×561 root DIP | PASS layout |
| Native About/license windows | 480×340, 640×500, 960×650 root targets | PASS bounds, modal/focus |
| Physical DPI changes / monitor movement / text scaling | — | NOT_TESTED |
| Hardware pinch / screen reader / user installation | — | NOT_TESTED |

Final complete tour: 462 PASS / 0 FAIL / exit 0 after short-reader and shared-host row corrections. It covers nine standard screens at twelve sizes, every catalog empty entry, every available workspace except editor (audited separately), loaded reader/editor, long names, 60 synthetic history records, preview return actions, source SHA256 preservation. Bounds checks inspect visible actions and wrapped-row overlap; they do not prove offscreen controls or hardware input. Final native counts and delivery integrity are recorded below.

Before visuals from original source snapshots: `baseline-before-build/baseline-before/before-480-*.png` and `before-960-*.png`. After visuals: `build-final-row-reset/responsive-audit/*.png`; native screenshot `native-reader-final-100.jpg`. These are native WinUI RenderTargetBitmap captures of simulated content, not physical DPI screenshots. Representative reader/editor/result/settings images were visually reviewed.

## Final verification and package

Final DLL SHA256: `339CEE63930AC24B4A30585BD74D47F93677476A2B4FCD9985266362270AA7F4`. The DLL in the clean standard rebuild and package is byte-identical to the final tested candidate. Evidence: `build-final-row-reset/full-layout-gate.json`, `row-reset-full-layout-audit.txt`, `row-reset-regressions/regression-results.json`.

| Gate | Result |
|---|---|
| Complete layout matrix/workspaces/loaded-state bounds | 462 PASS, 0 FAIL, exit 0 |
| Zoom / reader / editor | 52 / 131 / 148 PASS, exit 0 each |
| Read preview / history | 72 / 50 PASS, exit 0 each |
| Startup / update completion and restart / About/licenses | 15 / 163 / 27 PASS, exit 0 each |
| History-return native stability: 60 synthetic jobs, three previews, four reader returns, bounded canvases, idle 60 seconds | 9 PASS, exit 0 |
| Formerly failing focused interactive stress | 156 PASS, 0 FAIL; remained alive beyond 60 seconds, successful native inputs, graceful synthetic-session closure; no native crash event |
| Standard delivery | PASS version 25 + legacy updater 13; fixture package 8; distribution 63 + updater 13; worker 41; editor worker 102; search 38; Word worker 27; native Word 39; quality 39; PDF opener self-test |
| Package integrity | All 389 manifest lengths and hashes, manifest/latest hash, tested DLL equality PASS |

Native inputs on the final tested DLL: Enter applied 125%; next page retained 125%; Ctrl+0 returned to 100% on page 2. Earlier thumbnail/Escape/focus-return and navigation restoration checks remain recorded. New evidence: `native-reader-final-100.jpg`, `native-reader-final-state.json`, `final-interactive-stability.json`, `final-history-return-stability-gate.json`. Screenshot capture is of our synthetic audit window; installed user window was untouched. Bounds/source/state tests do not prove physical monitor DPI, physical touch, OS file-picker UAT or screen-reader UAT.

Published trusted LOCAL feed package: **`20261004-093759-64c84421`**, product **0.1.0 · Pengembangan**, 389 files, under workspace-root `releases/windows-local`. Manifest SHA256: `143B0DC4164236FA3BEF3C5174CA04419495588F697C5C82689D63F5FCF57D3D`. Clean build: `artifacts/update-build-22b551e64a664f8ca17bb1a07d34a0a1`. Logs and integrity: `local-update-gates-row-reset.txt`, `standard-native-gates-final.json`, `package-verification-final.json`.

Installed `current.json` remains byte-identical to the pre-task pointer (SHA256 `079B2ED6EC5592F15978D9C29A7F9D1A7C3B61E4C8C0B136C3FFB56D97AA0107`), active installed version `20261004-070509-9021ecc9`. No installation/activation, shortcut change, user-data overwrite or forced closure was performed. User may apply the local update through the existing authorized local update flow after saving drafts; see `LOCAL_UPDATES.md`. This is local development delivery, not Production approval, public publication or phase completion.

Owned source/harness paths: 28 (`owned-paths.txt`), plus this report. Baseline-to-scope diff `own-diff.patch`; final source snapshots/hashes `source-final/` and `source-final-hashes.json`; dirty-worktree listing `final-status.txt`. Package verification found zero unexpected changes outside owned baseline app paths. No new dependency, specialist agent/chat, commit, merge, engine-processing change, product-version change, OS/default setting change or central phase-record edit.

## Retained failures, correction and limits

- Interactive history-to-reader stress initially crashed in Microsoft.UI.Xaml.dll, `0xc000027b`, offset `0x39cde5`; isolated pre-layout baseline reproduced the same failure. Gallery-only and empty-gallery variants also failed. A helper read native stowed stacks only from its own synthetic process (`SyntheticNativeTrace.cs`, `native-stowed-trace.txt`); no OS debugger configuration or dependency installation occurred. Original native events and hashes remain in `native-history-reopen-failures.json`, `reopen-reproduction-hashes.json`, baseline `responsive-preview/audit.txt`, and `build-history-isolation/*-result.json`.
- The gallery left Auto/Star/Auto rows on shared `historyPage`. Reader/editor entry previously reused row 0 without resetting those definitions. Both entry points now clear gallery-specific rows so their canvas receives a bounded host viewport. This presentation-container correction removed the observed crash in the full reproduction (60-second idle, exit 0), complete tour, functional gates and focused interactive stress. It does not establish a universal cause/fix for every WinUI native exception. PDF raster/export/zoom algorithms remain unchanged.
- A retained empty reader could remain suspended after reattachment; successful attach/open and loaded/resume now clear suspension. The complete tour and reader regression cover it.
- Early fast synthetic tours changed the Merge collection before initial layout settled; awaiting initial layout removed the observed audit crash. Failed reports remain. One short-reader canvas failure at 640×340 was corrected by the compact header.
- The first standard delivery attempt stopped on `OutputQualityAudit` casting wrapped presets to `StackPanel`. It now uses `Panel` without removing functional assertions. The corrected quality gate passed 39 and the full standard delivery completed. Failed attempt log is retained as `local-update-gates.txt`; successful log is `local-update-gates-row-reset.txt`.
- An initial editor harness used the wrong fixture and overly long evidence paths (266–286 characters). It now uses the existing worker's exact admitted fixture and `editor-source.pdf`; final editor 148 PASS. Long-path processing behavior was not changed.
- One earlier startup run wrote 15 PASS but exited 2170; identical-binary retry and final candidate startup exited 0. The failure/event remains recorded; its cause is not established.
- Legacy `--audit-about` fails an obsolete expectation that audit mode opens an updater dialog; the original baseline also fails it. Scoped dialog audit verifies the current mutation guard, bounds and focus (27 PASS). No updater guard was removed.
- Whole-file shared ZoomController restore was rejected by automatic approval review because it could lose authorized work. Only inspected targeted presentation edits were used. A scratch restore initially used the wrong package cache; assets were repaired using the already admitted offline cache, without dependency downloads.
- Physical DPI/monitor movement, Windows text scaling, hardware pinch/multitouch, screen-reader UAT, manual OS picker behavior and installed-user activation remain NOT_TESTED. Simulated DIP sizes and programmatic native ChangeView are not hardware proof. No general app/Production readiness claim is made.

## Coordination

Completion report successfully delivered to authorized parent chat `01a07c8d-3c57-7963-8500-daed7ddb07ee`. Heartbeat `lanjutkan-responsivitas-setelah-zoom-selesai` is stopped (`PAUSED`, confirmed by automation_update). No remaining source/build/package writes are required for this scoped task.