# Windows self-contained candidate — 0.2.4

## Scope and authority

The user requested a package that does not install .NET 10 separately and approved product version 0.2.4 on 9 October 2026. This authorizes development and isolated package testing, not Production approval, website publication, installation over the user's running application, or removal of any system runtime.

Baseline is the byte-exact 0.2.3 backup in the private `cahpondokan/pertamina-oto` repository, tag `v0.2.3-backup-20261009`, commit `8d8780a6fe578104e19afae5ca4d2bd489f09d22`. Existing dirty source is preserved.

Do not change document rendering, zoom, editor, print, layout, or user data. Packaging is untrimmed, non-AOT, Windows x64. The application, PDF and Word workers, PDF opener, stable launcher, and package installer each carry their own runtime. Windows App SDK self-contained alone is not sufficient for .NET self-contained.

## Runtime intake (development candidate only)

Pin Microsoft.NETCore.App.Runtime.win-x64 and Microsoft.NETCore.App.Host.win-x64 to 10.0.12. Official release metadata identifies this as a security release dated 2026-09-08. Do not redistribute the unadmitted cached 10.0.11 runtime. Acquire only official NuGet package IDs, verify package signatures and SHA-512 package content hashes before build consumption, and retain LICENSE.TXT and THIRD-PARTY-NOTICES.TXT from the runtime package. Record exact downloaded hashes and verification output under `artifacts/self-contained-20261009`.

Authoritative sources reviewed:

- https://learn.microsoft.com/en-us/dotnet/core/deploying/
- https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json
- https://www.nuget.org/packages/Microsoft.NETCore.App.Runtime.win-x64/10.0.12
- https://www.nuget.org/packages/Microsoft.NETCore.App.Host.win-x64/10.0.12
- https://github.com/dotnet/runtime/blob/v10.0.12/LICENSE.TXT

This is a scoped local development intake, not a waiver of remaining security, legal, or Production release gates. No new third-party processing capability is added. Runtime security maintenance becomes part of each OTO package release.

## Transition and safety

Launcher capability protocol 2 binds the private runtime files to the existing immutable application manifest. Protocol 1 remains readable for old packages. Old 0.2.3 updaters do not support the expanded launcher contract: transition uses the complete new installer once, not a claim of seamless old in-app update compatibility. A failed migration must retain the old current pointer and restore all replaced launcher files. The same per-user application directory and stable shortcut are retained.

The native installer continues to use Windows .NET Framework 4.8, which is different from installing .NET 10. The self-contained package must never invoke the .NET download or machine runtime installer. Word conversion's optional Microsoft Word prerequisite is unchanged.

## Verification status

VERIFIED_LOCAL_CANDIDATE on 9 October 2026. Product **0.2.4 · Pengembangan**, build **20261009-113931-5d181036**. Implementation, the checks listed below, and local packaging are complete. Installation, clean-machine UAT, and online publication are not complete and are not implied by this status.

Later the same day, the user requested a website upload kit and another-laptop test. The exact desktop payload was reused; website-only migration guidance and an incompatible-client update-feed guard are recorded in [WEB_RELEASE_0_2_4.md](WEB_RELEASE_0_2_4.md). The upload kit is prepared locally, not uploaded or activated.

Delivery directory, relative to the parent workspace:

`releases/windows-local/0.2.4-self-contained-20261009-113931-5d181036`

| Artifact | Size | SHA256 |
| --- | ---: | --- |
| PERTAMINA-OTO-Setup-x64-20261009-113931-5d181036.exe | 296,400,896 bytes | `9A21DE52C0C31DB92ADDDB0217CEE31ACE0EB8BEB4F49F9FCEF14B33E6729A8F` |
| PERTAMINA-OTO-Windows-x64-20261009-113931-5d181036.zip | 296,273,916 bytes | `F8660AA420ED9F132881ACE182F1407B5B83767CBBE748CE26C4AB30C25A82A4` |

The manifest hash is `1B42752F2FDF43EA654F26F6A1E6F90D383E70B79FBBE1050EEB54B5D56379BE`. All 1,333 manifest payload files were verified against both the unpacked candidate and ZIP entry streams. The installer embeds the verified ZIP; the extracted installer verification also passed. The tested application DLL hash is `4456D1B0BD55EB551071CC41D9C204791DC69CCD0B321797D43B600CED81C550`.

The delivery folder contains its own `current.json` distribution catalog and `verification.json`. It is not the root active update feed. The installed application and root `releases/windows-local/latest.json` remain on **0.2.3**, build `20261009-035333-e218f12d`. Use the complete new installer once for migration; do not direct an old 0.2.3 updater at protocol 2. No installed files, user documents, system runtimes, GitHub releases, or website files were changed by staging this candidate.

## Test evidence

Paths below are relative to this checkout. Evidence root: `artifacts/self-contained-20261009`.

| Check | Result | Evidence |
| --- | --- | --- |
| Official runtime and apphost intake | Microsoft/NuGet signatures verified; exact versions, SHA256 and SHA512 recorded | `intake/verified-packages.json` and package signature logs |
| Standard build and regressions | Release notes 44; product version 25; packaging 8; distribution 63; legacy updater 13; PDF worker 41; editor worker 102; search 67 PASS. Word worker passed, with native Word 39 and quality 39 PASS | `final-build.log` and its referenced audit paths |
| Legacy and private-runtime launcher delivery | 38 PASS including rollback, corrupt/missing runtime, duplicate/path rejection and capability-only repair | `distribution-final/launcher-delivery/results.txt` |
| Restart with real isolated processes | 17 PASS including ready handshake, one new instance, cancellation, failed startup preserving old process, writer lock and launcher repair | `restart-delta-final/test-results.txt` |
| App-local runtime | Five probes passed for desktop, launcher, PDF worker, Word worker and PDF opener. Host traces loaded bundled CoreCLR despite invalid child DOTNET_ROOT paths. A separate copy without private runtime failed as expected | `private-runtime/summary.json` and host traces |
| Exact final native application | Reader 479, zoom 53, live editor 149 PASS; all three reports bind to the final DLL hash | `native-final.json` |
| Windows setup | 57 PASS including no .NET detection/download/install for bundled mode and no runtime-install fallback on failure | `artifacts/windows-setup-build-51a289c0e1624018b6b571c0f7dea249/tests/test-results.txt` |
| Installer layout | 68 work-area/scale pairs, 513 native state cases, 19,654 assertions, zero failures | Same setup artifact, `ui-audit/summary.json` |
| Package and unchanged activation | Full manifest, ZIP, EXE, native DLL identity, probe results, installed pointer and live feed verified | `delivery-verified.json` |

Installer screenshots `ui-audit/1366-100-ready.png` and `ui-audit/1366-150-ready.png` were visually inspected. They show version 0.2.4, runtime included rather than separately installed, and the action **Pasang OTO**. Short windows retain a scrollable details area and visible footer. These are native audit windows, not evidence of a completed installation.

## Packaging corrections and retained limitations

- Pin only the .NETCore runtime/apphost metadata. A global RuntimeFrameworkVersion also changed the Windows SDK framework reference and was rejected during early restore. Final builds use the scoped pin and existing project dependency cache; PDF processing dependency versions are unchanged.
- Publish the generated `App.xbf` and application `.pri` from the exact desktop build. Their omission in an early candidate caused XAML startup failure; the corrected final package passed native audits without changing page layouts or rendering code.
- Launcher repair copies changed files only, then verifies the full bound runtime. This keeps capability repair within the existing 25-second production restart window; rollback and immutable-pointer checks remain enforced. The test timeout was aligned with that existing window, not used to relax production behavior.
- An early editor audit without its required fixture did not establish a pass. Only `native-final.json`, referencing the proper synthetic-fixture run, is accepted as final editor evidence. Failed/preliminary artifacts are retained, not counted as success.
- The source comparison with the byte-exact 0.2.3 backup found no changes to protected renderer, zoom, editor, print or main UI implementation. Changes are limited to packaging, version/notes, launcher migration/restart, related tests, and this documentation.
- System .NET was present throughout testing. Host tracing and the negative fallback probe are useful isolation evidence, but do not replace a clean Windows PC/VM test. No system runtime was uninstalled.
- Physical DPI/monitor/printer coverage, a real user installation, Programs and Features behavior after installation, and human acceptance remain unverified. This is not Production approval or a claim that all Windows devices passed.
- The runtime is private to OTO, not eliminated: .NET 10.0.12 is redistributed inside the package. Existing separately installed .NET entries are not removed. The outer bootstrapper uses Windows .NET Framework 4.8; optional Word conversion still needs Microsoft Word.
