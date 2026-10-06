# OTO Windows EXE installer

1 October 2026. Agent0, scoped development request: one branded EXE opening a graphical wizard, detecting requirements, downloading/installing missing .NET10 x64, then installing OTO. No phase promotion, public activation, signing or automatic installation on this development computer.

## User flow and design

- Open the EXE. The existing OTO icon is embedded in the executable and wizard. It checks Windows10 build19041+/Windows11 x64, space across required drives and stable Microsoft.NETCore.App10.0.x x64 in registered/default machine locations. Native PE architecture and hostfxr/coreclr presence are checked; newer compatible hostfxr and SDK/Desktop Runtime installations containing the Core runtime are recognized.
- **Pasang OTO** uses the embedded payload without internet when the runtime exists. Otherwise **Pasang .NET dan OTO** downloads the pinned Microsoft runtime, checks SHA512 and full Microsoft Authenticode trust, requests UAC for that vendor installer, rechecks the prerequisite and continues.
- Preparation/download can be canceled. Installer processes are not force-killed. UAC cancellation, bad download/signature, unsuccessful setup or missing post-install runtime stop app activation. Reboot-required outcome is disclosed; no forced restart or app closure. Existing versions, documents and preferences remain.
- Existing validated package core installs for the current Windows account. The stable launcher/shortcut are preserved or created. Completion offers an unchecked **Buka OTO setelah selesai** option; restart-required state does not launch OTO. Shortcut failure is disclosed separately from app installation.

The C# GUI uses .NET Framework4.8 and existing Windows libraries/compiler, so it opens before modern .NET10 is installed. Microsoft lists Framework4.8 as included with Windows10 version2004 and Windows11: https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies. The application runtimeconfig requires Microsoft.NETCore.App10.0.0 with patch roll-forward, not Microsoft.WindowsDesktop.App. Windows App SDK native files are already bundled through the existing self-contained SDK configuration. No new NuGet/installer-tool dependency was added.

`eng/WindowsSetup` contains detection, download/trust verification, testable flow coordinator, bounded embedded ZIP extraction and WinForms UI. Private GUID staging uses restricted ACLs; archive paths reject traversal/ADS/reparse/duplicates. Long Windows paths work without system-policy changes. Cleanup only deletes the owned staging folder after path/link checks. Only verified Microsoft runtime installation gets elevation; OTO installation remains per-user. Intake/source pin: [DOTNET_RUNTIME_SETUP_INTAKE.md](DOTNET_RUNTIME_SETUP_INTAKE.md).

Build: `./eng/Build-WindowsSetup.ps1`. It verifies the latest local payload, compiles the EXE using the existing Microsoft compiler, and runs regression, embedded/core integrity and native UI gates before publishing immutable files/catalog locally. Optional `-PayloadZip` reuses a verified package; use a fresh output directory for rebuilding the same version. `-Notes` supplies release notes. Desktop/shared changes still use `Build-LocalUpdate.ps1` first.

Portal uses optional `installerFileName/installerSize/installerSha256`. Legacy schema1 ZIP fields continue describing a real verified ZIP, preserving old notification clients. Both files are hash-checked and support HEAD/ranges; unsafe/partial/damaged EXE metadata fails closed. Main CTA/help describe the EXE wizard. Website/notifications never remotely execute an installer.

## Artifacts and verification

- Local release `20261001-102920-701ec3e1`, manifest SHA256 `57BFC8878F86566C0B70145674BE6D04B6366C469DBFF4177B51299FB134F9B2`. All370 sizes/hashes verified; desktop/worker/PDF-helper DLLs match the tested build. `artifacts/windows-exe-download-proof/local-feed-integrity.txt`.
- EXE: `artifacts/windows-exe-distribution/PERTAMINA-OTO-Setup-x64-20261001-102920-701ec3e1.exe`, 37,575,168 bytes (35.8MiB), SHA256 `E6704300B8E3D40FE8E60F228CB942CF2C06E72622D4594859BCCB7A46C139B1`.
- Legacy ZIP in the same folder: `PERTAMINA-OTO-Windows-x64-20261001-102920-701ec3e1.zip`, 37,462,679 bytes, SHA256 `D39F9803B95FBA5F612912BEF3209071C7C34FFFE8E7D708DB2C808C72C5B9A7`.
- Bootstrapper54 checks PASS: `artifacts/windows-setup-build-0f86459deb4c4cee8f42a3b340d373c8/tests/test-results.txt`. Includes architecture/version, long paths, archive escape/duplicate/size, cancellation, HTTP redirect/oversize/truncation/offline, hash/signature rejection, present/missing runtime, UAC/error/reboot outcomes, re-detection and app errors. Confirms Framework CLR4 execution. Install outcomes are simulated; no real UAC/install claim.
- Native wizard7 checks PASS; six states and compact screenshots reviewed in that build's `ui-audit`. Corrected wrapped branding, low-resolution logo and Windows long-path extraction before final delivery. Failed/intermediate build folders retained as superseded evidence.
- Real Microsoft10.0.12 binary downloaded only to `artifacts/runtime-intake-10.0.12`; official metadata SHA512 and Windows signature Valid, signer `CN=.NET, O=Microsoft Corporation`. Final bootstrapper's own `--verify-runtime` passed: `bootstrapper-verification.txt`. Vendor executable was not run.
- Distribution62 + updater13 PASS: `artifacts/distribution-checks-4e23f01e16ef4c0882307292f684ae21`. Portable portal31 PASS: `artifacts/portal-checks-23cbd7c3621d48cca0212d5c8b86ba9f/test-results.txt`. Covers old-client ZIP compatibility, actual bytes, headers, ranges, invalid metadata and tampering.
- Actual local HTTP EXE download matches catalog size/hash; downloaded EXE embedded extraction and core full-payload checks pass. `artifacts/windows-exe-download-proof/test-results.txt`. Top-level desktop DLL inspection found no external msvcp/vcruntime import; dynamic dependency/clean-Windows acceptance remains pending.
- Standard desktop rebuild: worker41/editor63/search25, quality39, PDF-helper41 PASS. Quality: `artifacts/update-build-38e20f564cc94aed83072df4daa92ec9/quality-audit-f91ba0ab691b46b69afa60e6c2871777/audit.txt`; helper: `artifacts/pdf-association-verification/pdf-association-462cb08636c14090bdeed0eb3914f318/audit.txt`. Packaging script syntax and `git diff --check` pass.

## Hosting and remaining acceptance

Preview: `http://127.0.0.1:5089/#unduh`. Portable publish: `artifacts/download-portal/linux-exe-publish`. New deployment bundle prepared **locally only**: `artifacts/oto-vps-exe-stage-20261001-102920-701ec3e1.tar.gz`, SHA256 `7A7F402A8955A07959C8A8797EB16F43E0A1FE0E25215FEA065B2E03F0C67693`, 20 staged files. Its copied activation script/notes target this exact build. Earlier VPS staging is untouched; this new bundle is not uploaded/activated.

Public oto.ayani.id activation still requires previously identified administrator ASP.NET10 setup/sudo and public development-distribution/signing disposition. OTO EXE is unsigned; no certificate acquisition or SmartScreen bypass. Phase/Production gates unchanged.

Remaining: clean Windows without modern .NET; actual UAC allow/deny, standard-user elevation, reboot and interrupted install; physical high-DPI/keyboard UAT; initial install/update/app launch on a non-development PC; public HTTPS/download/notification after authorized activation. Minimum Windows build check does not claim all Windows10 editions remain vendor-supported. The user's active OTO installation was not changed by verification; applying the update/EXE remains an explicit user action.
