# Portable package — PORTABLE-PUBLIC-DEV-20261002

2 October 2026, lane1 packaging. **HOLD / internal framework-dependent candidate.** No website publication, local feed update, installation, system settings change, phase promotion or Production approval occurred. User wants a complete portable ZIP for public development evaluation; this candidate does **not** yet meet the no-runtime-install prerequisite requirement.

## Frozen candidate and binding

- Worktree `task/core-functional-first`; historical HEAD `84cbb0f538020003bcfb6583e94c2fcf6524f87c`, existing dirty source preserved.
- Product `0.1.0 / Pengembangan`; immutable build ID `20261002-085443-ef3f12a3`.
- ZIP `artifacts/portable-01a0fa43-c0ed/candidate-03/PERTAMINA-OTO-Portable-Windows-x64-20261002-085443-ef3f12a3.zip`, **37,362,220 bytes**, SHA256 `9BC67C5F3DFA1022FD26AF329497A6653150AF39A72B01D45AE577D7BE86CE16`.
- Package manifest SHA256 `5397E5688772B29EC5371B5E02794B5B45E77299D9E661E447031061B44A5D09`: 400 payload files plus the manifest; 20 separate legal/inventory files.
- Root entry point `PERTAMINA-OTO.exe`, guide `MULAI-DI-SINI.txt`, `app/`, `lisensi/`. No prepopulated user Data. Runtime **not bundled**; requires existing .NET10 x64. Windows x64, API minimum build19041; Windows support/clean-machine matrix remains unverified.
- `source-inventory.json` contains relative filenames, sizes and hashes of selected desktop/worker/shared/packaging source, not private documents or absolute user paths. Core source unchanged during the successful build; generated compiler/dependency build reproducibility is not independently proven. Manifest hashes bind the actual copied binaries/configuration/notices. Public inventory is not an SBOM or full compiler provenance attestation.
- ZIP orders frozen files with fixed timestamps. Re-archiving those exact frozen files can be compared bytewise; independent reproduction from the full toolchain/source has not been claimed.

Candidate01 failed integrity verification because .NET Framework path handling rejected deep workspace/license paths. It has no released ZIP. Candidate02 was superseded after final guide qualification/source validation; it is not approved. Candidate03 is frozen; subsequent QA must refer to its exact hash. Do not overwrite candidates or reuse historical PASS as proof.

## Implemented behavior

`PortableProfile.cs` detects `app/portable-profile.json`, including a malformed directory marker. It validates bounded schema1/Data, rejects links/junctions and checks Data write access before WinRT starts. Expected marker/IO failures show a sanitized native message and exit1, without fallback to installed AppData. An intact marker also applies when directly opening the desktop host. Removing the marker from a copied package is tampering outside that direct-host guarantee; use the root launcher, which verifies the marker and every manifest entry.

Portable preferences, favorite/history metadata, diagnostics and online preference paths use package sibling `Data`; default output is `Data/Hasil` while an explicit saved portable output preference is retained. Existing nonportable profile and test overrides remain. Portable UI omits installer update/default association actions; lower-level install/association methods also guard the marker. Automatic online checks are disabled; explicit website opening remains available. Word conversion keeps the existing hash-pinned optional local provider and bounded DOCX A4 behavior; no Office/provider is bundled or installed. OCR stays inactive.

The .NET Framework4.8 root launcher is compiled with the existing admitted Roslyn compiler and Windows Framework references, without dependency acquisition. It shows development/unsigned disclosure before launch, checks OS/x64 and package hashes, rejects traversal and normalized duplicate paths, and notices child exit within5seconds. A successful Process.Start or five seconds alive does **not** prove window readiness or clean-machine acceptance. The process-local config and extended Windows file paths support verifier long paths without registry/policy edits. No shortcut, PDF association, runtime installation or installed-app update is automatically performed. Hashes detect integrity, not publisher authenticity; Smart App Control can still block unsigned code. No bypass/disabling instructions are supplied. This is not a no-trace promise: Windows/Office caches, chosen external outputs and system artifacts may remain outside Data.

## Verification

Fresh successful commands, run in the worktree with isolated output:

```powershell
MSBuild.exe src/Hosts/Pertamina.Oto.Desktop.App/Pertamina.Oto.Desktop.App.csproj /t:Rebuild /p:Configuration=Debug /p:DesktopUiOutputPath=<lane1>/build/ /v:minimal /nologo
eng/Portable-CollectEvidence.ps1 -EvidenceRoot <lane1>
dotnet restore eng/portable/ProfileChecks.csproj -p:RestoreSources=
dotnet run --project eng/portable/ProfileChecks.csproj --no-restore -- <lane1>/profile-tests
<lane1>/build/worker/Pertamina.Oto.LocalPdf.Worker.exe --self-test
<lane1>/build/worker/Pertamina.Oto.LocalPdf.Worker.exe --self-test-editor
<lane1>/build/worker/Pertamina.Oto.LocalPdf.Worker.exe --self-test-search
<lane1>/build/word-worker/Pertamina.Oto.WordPdf.Worker.exe --self-test
dotnet restore tests/DistributionChecks/DistributionChecks.csproj -p:RestoreSources=
dotnet run --project tests/DistributionChecks/DistributionChecks.csproj --no-restore -- <lane1>/distribution-tests
eng/Build-PortablePackage.ps1 -BuildOutput <lane1>/build -BuildId 20261002-085443-ef3f12a3 -SourceInventory <lane1>/source-inventory.json -Output <lane1>/candidate-03 -CandidateWithoutRuntime
```

`<lane1>` resolves to worktree `artifacts/portable-01a0fa43-c0ed`. Rebuild exit0 after correcting CA1806 return-value handling; first build exit1 retained in `build.log`, successful retry in `build-retry.log`. Profile13, PDF worker41, editor63, search25, Word27, Distribution62 plus updater13 PASS, exit0. Evidence: `profile-tests/profile-results.txt`, `profile.log`, `worker-self-test*.log`, `word.log`, `distribution.log`; package `package-03-retry.log` and `candidate-03/artifact.json`. ZIP creation verifies each decompressed entry hash and count against the frozen package; root launcher `--verify` exits0.

Negative integrity checks use a separate copied fixture, never the immutable candidate. Intact/restored package exits0; changed guide, missing marker, traversal and normalized duplicate entries exit1. Missing runtime admission refuses a complete package before creating output. Results are saved in `integrity-results.txt`. No document processing on user data or global network/security setting change occurred. Synthetic worker tests are local operations; they are not an independently observed whole-desktop network-disabled test. UI/native startup, fresh Windows/runtime absence, ACL read-only Data, physical UAT, complex fidelity and hard containment remain independent QA/manual acceptance gaps.

## Runtime and distribution gate packet

Existing local runtime candidate: .NET10.0.11, commit `e2f47b0110ed922f21a1522da67279133ce28f32`,193 files /80,458,478 bytes. `runtime-candidate.json` records relative path/size/SHA256 for host, fxr and Microsoft.NETCore.App, exact root license/third-party notices, plus Valid Microsoft signatures for host/fxr/coreclr. Host SHA256 `AB1B71FD3DD71062E074C9FAB8312081A81B7F2B3E0327C48C4D249C8D1A3135`; root LICENSE SHA256 `7F6839A61CE892B79C6549E2DC5A81FDBD240A0B260F8881216B45B7FDA8B45D`; ThirdPartyNotices SHA256 `DEB4427A295E1ED474B0D81C5A0D972C1B550B9A715CDA939CDFA9236B1B418F`.

**NOT_ADMITTED:** valid local signatures do not establish original archive/acquisition binding, contemporary security assessment, or human redistribution admission. Existing10.0.12 Windows Setup intake authorizes an unmodified vendor installer, explicitly excludes repackaging; it is not portable-bundle admission. No new runtime was acquired or executed. The existing root license is Microsoft .NET Library terms, not a generic MIT substitution. Section3a permits conditional distributable object code and requires protective end-user/distributor terms, copyright and distributor indemnity. Windows App SDK exact vendor terms also require a separate distribution disposition. Lane1 has neither accepted indemnity nor authored substitute legal terms.

The recipe requires a scope-specific APPROVED record with authority reference, a separately pinned admission SHA256 and exhaustive unique normalized runtime path/hash set. These structural checks are not proof of genuine human approval; integrator must verify authority and provenance before invoking runtime bundling. Future public publication requires exact bundled artifact QA plus scoped legal/security/unsigned evaluation disposition. A public-evaluation request is not Production approval or a waived admission gate.

Notices preserve cached nuspec/license/notice text for desktop/PDF/Word asset inventories, the exact locally retained PDFsharp MIT text, Lucide/Feather text, and existing component inventory. No qpdf, PDFium, OCR or Office binary is added. Complete component-to-shipped-native-binary license coverage still needs independent audit; merely including20 files is not a legal-clearance claim.

## Handoff / remaining work

Lane1 changed only integrator-approved paths: new `PortableProfile.cs`, `Program.cs` startup guard, `PersistentLibrary.cs`, `LocalDiagnostics.cs`, `SettingsWorkspace.cs`, `OnlineUpdateNotifications.cs`, `LocalAppUpdate.cs`, `PdfAssociation.cs`, `PdfDefaultSettings.cs`, `UpdateSettings.cs`; new `eng/Build-PortablePackage.ps1`, `eng/Portable-CollectEvidence.ps1`, `eng/portable/PortableLauncher.cs`, `ProfileChecks.cs/.csproj`, `PERTAMINA-OTO.exe.config`, `portable-profile.json`, `MULAI-DI-SINI.txt`, this report and lane1 artifacts. Existing obsolete `Package-Portable.ps1`, old portable templates, product props, feed, installed app and central phase records were not changed.

QA chat `01a0fa43-d973-7911-b663-1a86cdd2c1f5` and web chat `01a0fa43-ef69-74b2-bb16-cbfea4107f2c` received candidate03 exact bytes/hash. Integrator `01a0fa43-2418-7732-9e30-2350bd032eb5` owns final cross-lane integration and user report. Recommended order: independent current-candidate audit; resolve precise runtime/legal/security blockers with named human authority; create new immutable admitted-runtime candidate and QA that exact artifact; only then activate the public development catalog. No full-portable or public-ready claim is made for candidate03.
