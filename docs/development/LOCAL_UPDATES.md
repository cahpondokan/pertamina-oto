# Local desktop installation and updates

**4 October 2026 — 0.1.1 release notes:** new clients use the same bounded plain-text model for exact local or online package notes. Before consent, current and target product versions, summary and detail are visible. The exact version/manifest chosen at confirmation must still match under the installer lock, or installation stops. Hashes, allowlist/HTTPS, rollback, busy/unsaved work and restart handshake are retained. `Assets/release-notes.json` travels offline inside the hashed manifest; future online catalog generation adds optional notes JSON/hash while old feeds remain compatible. No website publication is implied.

After successful new-binary startup, “Yang baru di OTO” waits for an idle home view; safety/recovery dialogs, PDF loading/viewing, typing and pending work defer it. Explorer document entry schedules it without interrupting the document. The per-user version ledger is atomically saved only when the native panel opens successfully. First install establishes a baseline without claiming an update; prepared/failed/cancelled startup, rollback and same-product rebuild do not show upgrade panels. Missing notes do not consume an upgrade acknowledgment. About > Riwayat versi can reopen the local history anytime. Only known releases are listed; skipped releases do not fabricate intermediate history.

Bootstrap limitation: older applications cannot display this new pre-install UI retroactively. After 0.1.1 actually starts, its panel/history are available. Final local availability and tests are in RELEASE_NOTES.md; this note does not claim installed activation.

**2 October 2026 launcher repair:** [LAUNCHER_DELIVERY_REPAIR.md](LAUNCHER_DELIVERY_REPAIR.md) supersedes the earlier separate-only launcher delivery limitation. New packages hash-bind `restart-launcher/` in the app manifest; verified migration and rollback are integrated into new updater/installer paths. The existing user launcher was separately repaired with a retained backup and unchanged active app/shortcut. Final local development build: `20261002-213627-67c6e4b5`. Public publication remains separate.

User authorization: 28 September 2026 — one installed desktop application and updates from a local release folder. Development use only, not Production approval or internet distribution/signing approval.

## Delivery contract

Product naming, 1 October 2026: display begins at **0.1.0 · Pengembangan**. Product version/channel are maintained explicitly in `eng/ProductRelease.props` and embedded in the desktop binary. `Version` in existing manifests/pointers remains the immutable timestamp+hash build ID; updater ordering, file hashes, rollback and launcher contracts keep that ID. Standard delivery checks ProductVersionChecks as well as existing gates. Multiple builds can share one product version; UI explains a build update. Legacy product metadata uses a transparent unknown-version fallback. See [PRODUCT_VERSION.md](PRODUCT_VERSION.md) for bump rules and verification/publication status. This note does not claim an installed update.

1 October 2026: user-requested ASP.NET download portal and read-only HTTPS notifications are implemented separately from this trusted local feed; see [DOWNLOAD_PORTAL.md](DOWNLOAD_PORTAL.md). Local file updates retain their existing behavior. Standard delivery now also runs DistributionChecks (client metadata and synthetic installer/updater regressions) before rebuilding. Local release `20261001-094532-c58e4ca7` adds online notifications; existing users apply it once through the local button before online notifications can work. No automatic remote execution is added.

30 September 2026: `Build-LocalUpdate.ps1` additionally runs `--self-test-search` and the PDF opener's isolated `--self-test` before publishing. A failed regression blocks the local update.

Stable per-user installation: `%LOCALAPPDATA%/Programs/PertaminaOTO`. Stable shortcut: `PERTAMINA OTO.lnk` on the user's Desktop. Existing other shortcuts/builds are not deleted. Launcher selects `versions/<id>` through `current.json`. User settings/history remain in `%LOCALAPPDATA%/PertaminaOTO`; document output location is unchanged.

Feed: workspace-root `releases/windows-local`. After each requested code change, build and verify the change, then run `eng/Build-LocalUpdate.ps1` to publish a local update. Do not merely launch a new arbitrary artifacts build as delivery. This script runs full rebuild plus existing worker/editor regressions before publishing. Run any additional change-specific tests before publishing. Publication is explicit, not a file watcher and not automatic compilation inside the user's application.

The installed user clicks **Pengaturan → Periksa update → Pasang update**. Packages are copied and hashed before switching the active pointer. No network, elevation, document upload or forced closure is involved. Save drafts and reopen the same shortcut to use the newly installed version. Already running instances remain on their old version until closed.

## Safety and limitations

- Version directories are immutable to the updater once complete; verified existing versions may be reused. Installation uses a single-writer file lock and atomic current-pointer replacement.
- Failed copies/hash mismatches leave current.json unchanged. Incomplete staging folders remain for diagnosis; updater does not recursively delete user folders.
- Previous pointer and binaries remain. Recovery: with the app closed, a maintainer may restore previous.json as current.json after checking the referenced version. Automatic startup-health rollback is not implemented.
- Manifest SHA-256 and per-file SHA-256 detect corruption, not publisher authenticity. This is a trusted, same-user LOCAL development feed, not an internet updater. Anyone able to rewrite that feed could replace application code. Signed manifests and distribution admission are required before remote updates.
- Rejects absolute/traversing/alternate-stream file paths and reparse links, invalid versions, oversized metadata/payloads, missing host/worker entries, and concurrent installs.
- Uses .NET 10 already installed on this laptop; this is not a portable runtime installer for other computers.
- Updates do not register PDF associations automatically. The explicit **Pengaturan → Pembuka PDF default → Jadikan default** action registers OTO for the current user and opens Windows default-app settings. Final selection belongs to Windows UI; see [PDF_DEFAULT_APP.md](PDF_DEFAULT_APP.md). No system-wide install, task scheduler or background watcher is installed.
- The launcher itself is stable; updating the launcher/runtime is a separate maintenance action.

## Verification

Initial installed release: `20260928-102132-2a48640d` (360 manifest entries). Published under the configured local feed and installed successfully through the same LocalAppUpdate.Install implementation used by the application button. Stable launcher installed at `C:/Users/cahpo/AppData/Local/Programs/PertaminaOTO/PERTAMINA-OTO.exe`; shortcut at the actual Windows Desktop location `C:/Users/cahpo/OneDrive/Desktop/PERTAMINA OTO.lnk`. No existing shortcut or installation was overwritten.

Application build passed; worker regression 41 PASS; editor worker regression 63 PASS. Native audit 136 PASS at `artifacts/desktop-updatable/live-editor-audit-1f581fc09dd64707aecbc8571656ac93/audit.txt`, including Settings update-button presence. `settings-local-update.png` visually inspected. The shortcut target/icon and installed build-info were verified and the desktop shortcut launched. Full physical click-through of a second application-version update remains manual UAT; core package activation/failure behavior is covered by the updater tests below.

Launcher build passed without external package sources; 13 updater tests passed at `artifacts/update-tests-0dcd7e6ee4d54cbe9e5feb5920966ca1/test-results.txt`: initial install, same-version no-op, corrupted payload/manifest rejection, unchanged current after failure, activation, retained previous version, traversal/absolute/ADS/version rejection and concurrent installer refusal. These tests use inert fixture binaries and do not claim full desktop UAT.


1 October 2026 EXE follow-up: local feed20261001-102920-701ec3e1 is verified. A separate graphical EXE now detects/downloads missing modern runtime for new computers; existing local-feed behavior stays unchanged. See [WINDOWS_SETUP.md](WINDOWS_SETUP.md). No automatic installed-user activation occurred.

## 0.1.2 scoped search correction - 4 October 2026

COMPLETE_LOCAL_READER_SEARCH_EXTGSTATE_SCOPE. Product 0.1.2 / Pengembangan, immutable local build 20261004-181107-307e2601. Manifest SHA256 7B967462A20000FE8BF91D3C7AB57AEDD34C57899767275569F2710C70B1AE33. All 390 payload hashes/lengths and 103 tested application binaries verified; installed pointer unchanged. Search on the authorized original now has two exact positioned hits; worker 67 PASS and eight final native gates 1,455 PASS / 0 FAIL. Notes retain 0.1.1 history. Local update only; no installed activation, online publication, phase or Production approval. Exact diagnosis, scope, commands, failures, evidence and ownership release: [READER_SEARCH_EXTGSTATE.md](READER_SEARCH_EXTGSTATE.md).

## 0.1.3 shared UI regression repair — 4 October 2026

COMPLETE_LOCAL_UI_REGRESSION_REPAIR_SCOPE. Product 0.1.3 / Pengembangan, immutable local build `20261004-211242-4e00f9cf`. Shared layout now recognizes replacement panels during same-size navigation, restoring source/settings separation and rebuilt home cards. Additional fixes cover the visible zoom value, embedded preview selector, result visibility, split form and missing reader icons. The UI matrix passed 1,485 checks including 120 loaded-workspace transitions; twelve native groups passed 1,648 checks, followed by limited native input review and standard delivery regressions. All 392 payload files and 103 tested binaries verified. Manifest SHA256 `10127887B71471973F8CE5970E53CB0649F1778999DE2F4C6634E17C47CF9AAE`. The exact tested assembly was staged after a separate WinRT-generated rebuild difference was diagnosed and retained; no interchangeable-rebuild claim is made. Installed pointer unchanged, previous 0.1.2 retained, online publication not performed. Physical DPI, multimonitor, pinch and human acceptance remain separate. Evidence, failed attempts and delivery details: [UI_REGRESSION_REPAIR.md](UI_REGRESSION_REPAIR.md).
