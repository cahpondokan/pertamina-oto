# Online update integration — 2 October 2026

Status: PREFLIGHT COMPLETE; cross-chat outbound coordination blocked by automatic approval review pending direct authorization in this chat. No release, installation, deployment or phase promotion has occurred in this integration task.

## Scope and baseline

Scoped internal development delivery authorized in the originating chat; not a full phase. Integrator task `01a0fbb9-155b-7813-9cf2-1fd1b0b1202f`. HEAD `84cbb0f538020003bcfb6583e94c2fcf6524f87c`, branch `task/core-functional-first`. Existing dirty/untracked work is preserved. Root/worktree AGENTS, Shared Agent Contract, state/decision register and local-update/distribution/setup/restart/product/search documents reviewed.

Read-only independent verification: baseline `20261002-151729-941b31ba`, 384 manifest entries, zero length/hash failures, manifest SHA256 `77368B5609405D21FFDA42E662701F3B1B969EDCADDD4925415409C17F5FD8F4`. Existing reader audit evidence reports 105 PASS/0 FAIL; no fresh native run is claimed at preflight.

## Ownership inventory

| Lane | Task | Exclusive source |
|---|---|---|
| Integrator | `01a0fbb9-155b-7813-9cf2-1fd1b0b1202f` | This document; ProductRelease.props; final candidate gate/build scheduling |
| Updater | `01a0fbb9-f8bd-7953-80b6-5be970e019fa` | Desktop updater, src/Distribution, tests/OnlineUpdateChecks, ONLINE_MANUAL_UPDATE.md |
| Guide | `01a0fbba-10d1-7f70-be7b-b832f37e2f4c` | Pages/Panduan.cshtml{.cs}, wwwroot/guide/, WEBSITE_GUIDE.md |
| Release | `01a0fbba-29f0-7c23-bf73-24f0abfd01cb` | Other download website source, Build-DownloadPackage, Build-WindowsSetup, installer and hosting source |

Worker ownership above comes from originating task cards. Shared Desktop.App.csproj requested by updater for one Compile link; lease confirmation is pending outbound communication permission. Program.cs, RestartProtocol.cs, local build/publication scripts and launcher have no new shared lease. One delivery build and one publisher only. Release lane is the designated publisher after exact candidate acceptance; no public candidate is accepted yet.

## Proposed minimal protocol (pending worker confirmation)

- Preserve schema1 `/api/releases/windows-x64/latest`, origin `https://oto.ayani.id`, development channel, existing FileName/Size/Sha256 ZIP and optional Installer* EXE fields.
- Reuse existing ZIP layout `feed/latest.json`, `feed/<immutable build>/manifest.json`, `feed/<immutable build>/app/`; in-app updater extracts and validates feed then uses existing InstallFrom atomic version activation. Do not execute the EXE from the updater.
- No product bump is approved. A and B retain product `0.1.0`, display channel `Pengembangan`; they must have distinct immutable timestamp/hash build IDs. Do not relabel old payloads.
- Manual Update checks online, shows no-update/failure clearly, and requests install consent when a newer build exists. Existing notification toggle continues controlling notifications. Processing documents stays local.
- HTTPS origin restriction, redirects disabled, metadata/download size and SHA256 checks, bounded safe staging/extraction, cancellation/failure preservation and existing current/previous atomic pointer behavior must be tested. Hash is integrity, not publisher signature; HTTPS/server trust is the disclosed internal assumption.
- Existing installed launcher may lack restart capability. Preserve save/normal-close/reopen-shortcut fallback; no force close or installed launcher maintenance is inferred.
- Upload immutable payloads and verify first, atomically switch metadata last. Maintain old endpoint semantics and previous release for rollback. Do not advertise B before actual availability is verified.

## Remaining gates

Latest evidence audit supersedes interim compile failure: desktop-build-final.txt shows desktop output at `artifacts/online-update-native`; worker reports exit0. Independently counted security-final33 PASS/0FAIL, legacy13/0, distribution-final62/0, native online audit22/0. Real HTTPS evidence has2 PASS including370 verified hashes for the old public build (download/extraction only; no binary execution). These are source/fixture/native-check evidence, not final immutable delivery or live A→B. Native corrected text has worker visual verification; guide final QA and publication remain pending candidate. Final handoff/source freeze required before serialized release packaging.

Updater native interim evidence independently read: `artifacts/online-update-native/online-update-audit/audit.txt` contains13 PASS/0 FAIL. This covers presence of primary manual online action, local bootstrap action and existing notification behavior/layout; it does not exercise download/install through native UI. Worker fixed the compile error and reports isolated build success. On worker request integrator changed only SettingsWorkspace.cs update row from obsolete local-source/offline description to online check/local-document/fallback text. That new row still needs fresh native render. Guide checkpoint reports desktop/mobile320/390/1280 and links QA, pending final candidate synchronization. Communication and publication blockers remain.

Integrator compile integration performed after updater explicitly requested integrator to add the link: Desktop.App.csproj now includes `../../Distribution/OnlineUpdatePackage.cs`. No worker source was changed. No MSBuild/dotnet process was observed at start. Desktop `/t:Build` Debug output `artifacts/online-integration-desktop/` initially failed sandbox Windows SDK read; authorized escalation passed that boundary but compile failed `CS1061` at `ManualOnlineUpdate.cs:59`: concrete `Progress<string>` does not expose `Report` (use an `IProgress<string>` reference). This repair belongs to updater; no lease transfer or source takeover is made. No deliverable candidate exists from this failed build. Build command: `MSBuild.exe src/Hosts/Pertamina.Oto.Desktop.App/Pertamina.Oto.Desktop.App.csproj /t:Build /p:Configuration=Debug /p:DesktopUiOutputPath=<worktree>/artifacts/online-integration-desktop/ /v:minimal /nologo`; exit1. Latest source reports 30 updater +13 legacy checks and corrected204/size UI, not independently rerun.

Read-only interim review (not candidate acceptance): updater source streams download to CreateNew file, checks exact byte count/SHA256, caps runtime, checks cancellation and ZIP path/case/parent/device/size boundaries, then validates extracted feed before existing atomic installer. Its reported 25 checks have not been independently rerun. Integrated verification must exercise the real full packaging ZIP (including 220-character path admission), actual native progress/cancel/confirmation behavior, source/data preservation, and isolated activation/launch rather than only fixture extraction. Candidate UI currently reports no release (204) under a 'Sudah terbaru' title; review whether unavailable feed should be distinguished from known current metadata. Download consent should show package size alongside notes/version.

Guide draft read-only review: route `/panduan` and release-derived download link present; update section explicitly remains pending final contract. Product0.1.0 and development limits disclosed. No Razor build/responsive validation or final content acceptance is claimed yet. Release worker reports 34 initial portal checks and public old build; not independently audited.

Preflight observations: installed `restart-capability.json` is absent (existence check only); read of installed current.json is denied by this sandbox, so current installed build is not inferred. Release worker reports live read-only SSH confirms active/enabled OTO service and root-owned deployment/release paths; `sudo -n -l` requires a password. This is a worker observation pending audit of its evidence, not independent integrator server verification. Privileged publication requires the user's terminal; no secret is requested or stored.

1. Direct outbound communication authorization; initial parent message and guide reply were rejected by automatic approval review despite original user transcript inspection. No bypass attempted.
2. Confirm exact shared leases/protocol with workers, audit all three handoffs and source scope.
3. Serialized integrated build/regressions/native updater UI evidence; retain reader/editor/startup behavior.
4. Prepare and verify A then B identities, installer/ZIP/catalog graph and exact audited binaries.
5. Release publisher activates accepted website/guide/payload only with verified access; administrator credential absence is a specific blocker, not permission to bypass.
6. Independent public HTTPS bytes/hash verification and native A→B isolated installation/launch evidence; active user installation/data untouched. Isolated automated tests are distinct from user laptop UAT.
7. Evidence-based final report to originating chat after authorized communication.
