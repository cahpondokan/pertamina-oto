# Launcher delivery repair — 2 October 2026

Agent 0, scoped development repair explicitly authorized in the originating chat after diagnosis. Baseline `task/core-functional-first`, HEAD `84cbb0f538020003bcfb6583e94c2fcf6524f87c`; 244 existing dirty/untracked entries preserved. Not a phase execution or public publishing authorization. No extra agents.

## Task card

- Objective: repair installed legacy launcher's missing restart capability and deliver the same capability through subsequent app/installer packages.
- Inputs: installed September 28 launcher lacks restart-capability.json; sanitized errors.log shows FileNotFoundException in ValidateLauncher. Previously tested same-user restart handshake remains authoritative.
- Allowed: local updater/launcher maintenance, launcher build and distribution scripts, desktop packaging reference, installer integration, targeted regression tests, scoped failure messaging and this evidence document.
- Do not touch: PDF processing, website/public release, current user documents/session/shortcut, existing immutable releases, unrelated edits, product version policy or phase gates.
- Acceptance: verify entire bundle before mutation; maintenance/update lock ordering; backup and rollback on failure; no replacing an active launcher; exact installed pointer and user data preserved in one-time maintenance; capability included and hash-bound in new app manifests; fresh/existing install and update paths tested; real isolated WinUI restart completes.
- Verification: legacy updater and distribution tests, new migration/negative/rollback/concurrency tests, protocol tests, native two-instance restart, standard Build-LocalUpdate gates, package inventory verification. Installed maintenance only after tests, no forced restart of user's session.
- Stop on failing verification, unexpected active writer, new dependency/credential, or need to force-close user work. Public publication is separate.

## Results

Implemented and verified for local development. No public website mutation or user-session restart performed.

### Implementation

- `LauncherUpdate.cs`: exact four-file/protocol/size/hash validation; app-manifest binding for the five bundled files; maintenance/update lock ownership in callers; no replacement of active stable launcher; staged verified copies, retained before-images, rollback if any replacement or pointer commit fails. Identical bundles are no-ops. Reparse links and partial manifests are rejected.
- `LocalAppUpdate.cs`: ordered maintenance/update locks, verify installed payload before launcher migration, commit current pointer last and roll launcher back if commit fails. Same-version explicit install can repair missing capability. Legacy packages without a bundle remain compatible and never downgrade an existing launcher via their outer installer bundle.
- `RestartProtocol.cs`: unchanged same-user READY/EXIT/COMMIT handshake, plus exact-current-target bundled launcher repair under locks for legacy updater transitions; capability advertises maintenance protocol 1. User work guards are unchanged.
- Desktop project includes a private=false build reference and copies only four launcher runtime files into `restart-launcher`, then writes capability metadata. All five are hashed into the immutable app manifest. No stray top-level launcher apphost is shipped.
- `InstallPackage.cs` and `Build-DownloadPackage.ps1`: fresh/existing installer paths use the same manifest-bound bundle as the in-app updater; ZIP outer launcher bytes match the inner bundle. Full input is validated before mutation.
- Launcher `--maintain-launcher` and `Update-RestartLauncher.ps1`: explicit one-time legacy maintenance uses the same verified transaction implementation, with pointer-preservation check. Shared compile links were added to launcher, distribution installer, PDF opener, portable/profile and product/distribution/online/restart tests; no dependency added.

### Verification

- `dotnet run --project tests/DistributionChecks --no-restore -- artifacts/lr-checks-04`: **63 PASS + 13 legacy updater checks**; nested `launcher-delivery/results.txt` **29 PASS**. Includes partial replacement rollback, pointer-commit failure, missing prior capability, locked files, update/maintenance concurrency, cancellation, same-version repair, older release refusal, fresh/existing installer and source-link rejection.
- `dotnet run --project tests/OnlineUpdateChecks --no-restore -- artifacts/lr-online-01`: **33 PASS**, plus 13 legacy checks.
- `tests/RestartChecks` against exact final launcher: **17 PASS**, `artifacts/lr-protocol-final/test-results.txt`. Includes successful legacy bundle repair, exactly one child, cancellation, failed startup, matching target and concurrency rejection.
- `eng/Test-NativeRestart.ps1`: **3 PASS** at `artifacts/nr-fixture-4edf435e/test-results.txt`; real WinUI parent exits normally only after new startup readiness, child receives COMMIT. Final launcher DLL SHA256 `4D589290A8D0F58BCFF2B3897F9A935E9A2FC3DA034B6AF8D6F7029201F37A8B`.
- Compatibility with copied original installed build A (`20261002-193948-6169e95a`), not just new app code: **3 PASS**, `artifacts/nr-fixture-23a164ba/test-results.txt`. Original release/installed files untouched.
- Exact final desktop native `--audit-update-completion`: **163 PASS**; `--audit-online-update`: **62 PASS**, under `artifacts/update-build-98cc4aa0afb94906acbe0fb597989db1/{restart-audit,online-update-audit}/audit.txt`.
- Standard `eng/Build-LocalUpdate.ps1` exits 0: build/analyzers, product25, product package8, distribution, worker/editor/search38, Word/provider and native Word/quality/PDF opener gates. Exact output `artifacts/launcher-repair-delivery-final.log`.
- Windows setup build/embedded verification exits 0: **54 bootstrapper checks**, **7 native wizard checks**, at `artifacts/windows-setup-build-73c795f2f6734aebbc088fbaa74ca229/{tests,ui-audit}`. Log `artifacts/launcher-repair-package-final.log`.
- Actual final ZIP install + native startup: **7 wrapper PASS**, including **15 native startup checks**, plus installed launcher four-file hash validation. `artifacts/online-update-finalization/live-d526c5f8f0944e36b1c0e68a38c73760/results.txt`. All **389** payload sizes/hashes verified before and after actual isolated startup.
- Earlier failures retained: analyzer token-order/forwarding corrections, an incorrect fixture path, and PowerShell native array-argument packaging invocation. None were treated as successful. First 392-file build `20261002-213335-e8c1f551` retained but superseded after removing three redundant root launcher files; final389-file build below is authoritative.

### Installed maintenance and delivery

User-authorized `eng/Update-RestartLauncher.ps1` completed with exit 0 on the existing per-user installation. Capability and all four installed launcher hashes match the tested bundle. Backup retained at `%LOCALAPPDATA%/Programs/PertaminaOTO/launcher-backup-db208c6efebc4f2bb5522a71da6b541e/before`.

Before/after `current.json`, `previous.json`, `update-source.json`, and Desktop shortcut SHA256 values are unchanged. Existing app PID **23224** remained the same. Installed/current app remained **20261002-193948-6169e95a**; no user documents, preferences/history, version directory or session were modified by maintenance. An actual click-to-restart in the user's session remains user UAT; it was not forced by this task.

New local feed: **20261002-213627-67c6e4b5**, product **0.1.0 · Pengembangan**, manifest SHA256 `008DDB40AE7261804B3CE67197D940386F7635B9CFBD4A083E8539846162DE31`, **389 files**. Root workspace `releases/windows-local/latest.json` selects it. Product version policy was not changed.

Local distribution output: `artifacts/launcher-repair-release`:

- ZIP: `PERTAMINA-OTO-Windows-x64-20261002-213627-67c6e4b5.zip`, 38,003,777 bytes, SHA256 `01BD844E43793663F60B9D36C23D12A5FAD13BA48739B589694B43066DD32C1B`.
- EXE: `PERTAMINA-OTO-Setup-x64-20261002-213627-67c6e4b5.exe`, 38,116,352 bytes, SHA256 `0280924BD75FD357EF5B9C245F8B15F5B34445EDC63F69AE57D514903AF533D5`.

### Residual boundaries

No website upload/activation, signing, dependency/runtime installation, phase acceptance or Production readiness claim. Website still has its separately activated B release; the new package must be explicitly published separately. On other legacy installations, an old updater cannot execute new migration code before the new app is opened: first transition may still need a normal save/close/reopen, or the repaired installer/authorized maintenance. Subsequent new-updater installs migrate the launcher transactionally.

Rollback covers tested file/pointer errors, not a claimed power-loss-atomic transaction across multiple Windows files; retained backups support recovery. Session drafts are not automatically transferred. No process kill, document upload, security-policy change, commit, merge or deletion of existing releases was performed.
