# Home card click repair — 11 October 2026

Authorized by the user's request to fix the diagnosed menu click problem and make it available through the installed application's local update button.

`CenteredHome.cs` wraps the launch ContentPresenter in a Border binding the transparent button Background and CornerRadius. Empty padding and icon gaps now hit the launch button; the independent favorite control remains above it. No navigation handler, engine, hover animation or layout redesign was introduced.

Actual WinUI A/B reproduces the old missing padding target. Final centered-home audit: 83 PASS / 0 FAIL, including 48 point hit tests at 1480/640 widths and four actual control Invoke routes. Standard delivery additionally passed Word/quality UI (39 each), worker/editor/search/Word (41/102/67/27), notes/product/distribution/updater and PDF opener fixtures. Physical mouse/Enter/Space UAT was not completed because the helper encountered another window and then detected user activity; no physical-input PASS is claimed.

Local product 0.3.0 / Pengembangan, immutable build `20261011-154323-b1783f4d`, is now the active existing `releases/windows-local` feed. Manifest SHA256 `63DF43000EA64A4A44DA690EC60425B38B8AA2766B63207D10A274FF8B5D8D9F`, 1,332 entries. Exact tested app DLL SHA256 `A7ED829F38594A8CDDA3D31642EF59A5EF59E3C979CCA47B012EB4AE87B5296E`.

The actual installed 0.3.0 / `20261011-130427-44487e3d` DLL accepted and installed this package in an isolated fixture, preserving its previous-version pointer and synthetic data. The same binary then read the real install root's unchanged update-source.json and verified all 1,332 payload files from the active D feed. Installed current.json, stable launcher and source preference remained unchanged at final check; the user applies the package via Settings > Update lokal > Pasang update > Mulai ulang sekarang. No online publication, phase promotion, main merge or Production claim.

Build/output initially used C because D had only 3 MB; after D space became available, the exact package was copied and hash-verified before atomic feed activation. Existing dirty changes were preserved. Full report and evidence: `C:/Users/cahpo/.codex/visualizations/2026/10/11/01a12a06-7af1-7ea3-afe0-9c062014e759/menu-click-repair-report.md`.
