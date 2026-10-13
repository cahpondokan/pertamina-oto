# Distribution refresh — 11 October 2026

Scoped APP lane, not a phase execution or release approval. Product remains 0.2.4 / Pengembangan. No subagents, user-profile installation, shortcut/registry changes, active feed promotion, remote upload or publication. Coordination/task cards under workspace `output/task-plans/distribution-refresh-20261011/` own the authorization and path boundaries.

Status: APP code/tests/candidates complete for local integration with limitations. Final immutable identities, artifacts and terminal result are recorded in workspace APP/HANDOFF.md and APP/READY.json. No public distribution or user installation is approved by this result.

## Behavior

The thin online EXE runs on Windows Framework 4.8; it does not embed the application or need global .NET 10. An explicit download action streams the exact pinned full offline installer from fixed HTTPS oto.ayani.id, rejects redirects/foreign hosts, checks byte length/SHA256 and keeps those verified bytes locked while the offline wizard runs. That wizard separately asks the user to install. Download cancellation never activates an application. The online UI shows byte progress and status outside its scrolling detail panel, checks a conservative temporary-space budget, uses a 20-minute deadline and at most one retry for HTTP connection failure. It does not force-close the wizard or running application.

The full offline installer contains the same application ZIP and uses the existing per-user installation/runtime/launcher transaction. Private .NET 10.0.12 is included; bundled mode never detects/downloads/installs global .NET. The added extraction preflight rejects ZIP links, Windows device names, ambiguous file-parent paths and excessive expansion before writes. Old application versions and data remain. No unrelated PDF/UI capability is changed.

The new updater uses schema2 at `/api/releases/windows-x64/baseline-1/latest`, with protocol2/baseline1, x64/private-runtime requirements, one immutable build/manifest and online/offline/update descriptors. Metadata is bounded to 96 KiB, ten seconds; payloads retain existing bounded streaming, safe extraction, manifest validation and atomic pointer/launcher rollback. Five private-runtime configurations and launcher protocol2 are checked before activation. Notifications remain preference-controlled while the app is open; download/install consent and draft-aware restart are explicit. A failed connection does not prevent local PDF work.

Schema1 is not repurposed. `current.json` keeps the private-runtime installer transition guard; the WEB lane suppresses the legacy updater endpoint when a baseline catalog is present. The old laptops can reinstall once. There is no promise of legacy updater migration.

## Contract and source binding

APP/contract.md and CONTRACT_READY.json checkpoint1 freeze DistributionRelease/OnlineReleaseClient plus existing shared types. WEB handoff was observed read-only; shared source hashes stayed frozen. `distribution-v2.json` is the new verified catalog, with actual sizes/hashes, not the synthetic fixture. Download total for online setup is offlineInstaller.size; its initial EXE size is onlineInstaller.size.

Crash handoff: build 20261011-104936-27c39aeb, DLL F9300F8DB943124DFA40A8152794901BDD537BD72309CC0E5AE517D447F1A4A0. Initial 126 source hashes match; all 20 preserved report hashes plus manifest/DLL/ZIP bindings were rechecked without repeating the old full native audit. The new tested desktop DLL is BF5784607A99227EC2617259FCFBCB86031FB8BADDBF936307C7EB50D914D724. Only ManualOnlineUpdate.cs, OnlineUpdateNotifications.cs and the csproj link changed among those original host inputs; BaselineUpdateAdmission.cs was added. PDF/Word/opener/launcher DLLs are byte-identical to the crash baseline. DesktopMenu/LivePdfEditor fixes remain unchanged. The final 1480 home image is byte-identical to its baseline.

## Verified checkpoints

Evidence root: `artifacts/distribution-refresh-app-20261011/`.

- Contract: 15 baseline +63 distribution +13 legacy checks. Updater security: 33 +13 legacy checks.
- Self-contained application build: analyzers enabled, five runtime payload checks, exit0. Exact native DLL above.
- Scoped final native: menus18, delayed-editor-callback12, updater67, restart/draft165, window32, home30, zoom63 =387 PASS/0 FAIL. Native updater uses inert payloads/in-memory HTTP; it is not positive public HTTPS evidence.
- Final online bootstrap: 22 transport/platform/Framework/thin-payload checks and 30 native render/focus/action cases across 680/480/360 widths and 520/360 heights. Actual full offline EXE bytes were streamed and verified through the local HTTP handler. Physical monitor/DPI transitions are not inferred from these windows.
- Final offline bootstrap: 64 self-tests, exact embedded ZIP/core installer validation, 68 work-area/scale pairs, 513 native states and 19,654 assertions with zero failures. These are simulated geometry cases, not physical-monitor UAT.
- Sanitized log has zero unhandled records for final MVID 6ab43c21-c285-4177-bbc6-51c51c45ef51; expected handled negative cases remain.

Failed attempts remain: initial sandbox Storage screenshot denial; home test input containing mixed path separators (canonical Windows path rerun passes); initial bootstrap focus harness before explicit activation; command-verifier CS0126 corrected to return0. None is counted as a passing gate. No protected feature source was changed to satisfy the harness.

Packaging also caught a CLR4 activation path of 255 characters (262 including .config), an unsafe ZIP-name check running after path resolution, and a PowerShell double serialized into the Int64 expandedBytes field. Shorter evidence paths plus a precompile path guard, name checks before path APIs, an explicit Int64 cast and a schema2 precommit gate resolve those issues. Failed inputs/logs and rejected metadata remain indexed. Drive D filled during the first actual-A extraction; only exact owned reproducible staging/failed build copies were cleaned after containment/no-link checks and hash capture. Original immutable packages, source, raw reports and user data were retained. The real-package harness checks its peak disk budget before extraction and releases only its own extracted A staging after verified isolated activation.

Measured B artifacts: thin online EXE 60,416 bytes; full offline EXE 296,432,640 bytes; update ZIP 296,302,774 bytes. The thin launcher downloads the full offline EXE, not the update ZIP. Build B is 20261011-115630-ebe2f901; product stays 0.2.4. Actual public HTTPS delivery is still unproven.

Actual A (20261011-114932-79bdfcb4) to B transition passed 18 checks, including corrupt/hash/cancel/wrong-manifest/concurrent preservation, baseline protocol/private-runtime admission, B activation, exact previous-A pointer and retained A files. Installed B passed 15 native startup/render checks and a host trace binds CoreCLR to its app-private folder with invalid DOTNET_ROOT variables. The synthetic user-data sentinel is outside the installation root; this is not a real settings/history/document migration UAT. Transport streamed actual package bytes through an in-memory HTTP handler. Both builds were genuinely generated from the same tested functional binaries and product 0.2.4; their build/manifest identities differ. Original B payload hashes remained unchanged after startup.

## Delivery and limits

The isolated standard-delivery wrapper preserves all mandatory checks, uses the exact prepared binary, confines native data/evidence and archives generated audit files before publishing to the APP candidate feed. It refuses a default active-feed target. Its diff and raw logs are retained. New A/B IDs must be generated by the unchanged publisher, never by relabeling old packages; same-product build updates are legitimate fixtures for baseline testing.

Final packaging/transition results follow in the terminal APP handoff. Actual package-byte HTTP-handler tests, inert native tests, real installed-B startup and public HTTPS are distinct evidence categories. Positive public HTTPS/native baseline UI, clean Windows without a system runtime, physical DPI/keyboard/accessibility and laptop/uninstall UAT remain separate gates. No system runtime was removed. Packages are unsigned: hashes prove integrity, not publisher identity. A new product version, signing/security disposition and any installation/publication decision belong to the human owners.
