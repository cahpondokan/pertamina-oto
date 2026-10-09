# Download portal and online notifications

User request: 1 October 2026, C# ASP.NET website at `oto.ayani.id`; release notification when installed OTO is open/running. VPS target: Ubuntu/Debian. This authorizes local implementation of distribution support and supersedes the earlier local-feed-only development restriction for this work. It does not promote project phases or approve a public/Production release.

## Implementation

- ASP.NET Core 10 Razor Pages, shared framework only, no new NuGet dependencies. Responsive Indonesian landing page, actual synthetic OTO screenshot, version, notes, ZIP size/hash, installation instructions. No public upload/admin endpoint, document processing API, accounts, database, analytics, or CDN.
- `GET /api/releases/windows-x64/latest`: schema 1 JSON, or 204 when no verified release is available. Host catalog validates strict development version/name, size, SHA-256, notes and local paths. Downloads only expose the current verified ZIP; GET/HEAD and byte ranges work. Files and metadata stay outside wwwroot. Invalid/missing data does not fabricate a download.
- Desktop: delayed startup check, every 30 minutes and connection restoration while open; HTTPS metadata only, 10-second total deadline and 16 KiB maximum. Redirects and cookies disabled. No filenames, document data, device identifier, or installed version in the request. HTTPS server naturally sees connection IP. Network/metadata failures are quiet during automatic checks and never disable PDF processing.
- Newer timestamp produces a closable banner with **Lihat unduhan**, opening a fixed `https://oto.ayani.id/#unduh`. Dismissal remembers that version. Settings can disable polling or manually check. No service, push subscription, background process after OTO closes, auto-download or auto-execution.
- Existing installations need this local update once before they can check the online feed.
- `Setup.exe` is a .NET 10 x64 console installer for the downloaded/extracted package. It asks the user to type PASANG. Reuses the stable per-user launcher and shortcut; verifies all payload hashes, preserves existing local update-source preferences, immutable old versions and user data, prevents downloaded downgrade, and atomically updates current.json. No elevation or forced close. The extracted feed is never persisted as the update source. .NET 10 Runtime x64 is a prerequisite; runtime installation is not bundled.
- Package hash detects corruption; it is not publisher authentication. These development packages are unsigned. Existing dependency/distribution admission and signing/public-release gates remain required before public distribution.

## Local build and preview (PowerShell, repository directory)

```powershell
dotnet restore tests/DistributionChecks/DistributionChecks.csproj -p:RestoreSources=
dotnet run --project tests/DistributionChecks/DistributionChecks.csproj --no-restore -- artifacts/distribution-checks-NEW-ID
./eng/Build-LocalUpdate.ps1
./eng/Build-DownloadPackage.ps1
dotnet restore src/Hosts/Pertamina.Oto.Downloads/Pertamina.Oto.Downloads.csproj -p:RestoreSources=
dotnet publish src/Hosts/Pertamina.Oto.Downloads/Pertamina.Oto.Downloads.csproj --no-restore -c Release -o artifacts/download-portal/publish
dotnet run --project src/Hosts/Pertamina.Oto.Downloads/Pertamina.Oto.Downloads.csproj --no-build --no-launch-profile -c Release --urls http://127.0.0.1:5088 --Distribution:Root="ABSOLUTE-PATH-TO/artifacts/web-distribution"
```

Build-DownloadPackage verifies the latest local release, builds the installer/launcher from installed framework references, writes an immutable ZIP and atomically replaces current.json after hashing it. Do not rerun with the same version/output; generate a new local release or choose a fresh output directory. ZIPs, binaries and preview evidence are artifacts, not source. No external publication occurs in these scripts.

## Ubuntu/Debian deployment preparation

Prepared files: `eng/hosting/oto-downloads.service`, HTTPS Nginx configuration and HTTP ACME bootstrap. Exact distro/version, architecture, existing services/ports and installed runtimes still require server inspection. Do not overwrite an existing Nginx site or certificate. Microsoft hosting reference: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-10.0

Maintainer sequence after server/publication authorization:

1. Confirm `oto.ayani.id` A/AAAA records point to the intended VPS; check IPv6 consistency and existing web server. Keep Kestrel bound to loopback; expose HTTPS through Nginx. Inspect before changing firewall rules.
2. Install supported .NET 10 ASP.NET Core Runtime and Nginx from admitted official distro/Microsoft sources for the actual OS. Confirm `dotnet --list-runtimes`. Create a dedicated non-login `oto-web` service account, separate from the authenticated deployment account.
3. Upload the web publish directory into a new versioned `/opt/oto-downloads/<web-build>` folder. Set `/opt/oto-downloads/current` to that version under controlled deployment. The service user only reads application files.
4. Place release ZIPs plus catalog in `/var/lib/oto-downloads/releases` as ordinary files/directories (no symlinks). Give the service user read access and only the deployment account write access. Keep older packages and catalog copies for rollback. Do not include local update-source.json or development workspace paths.
5. Obtain a valid certificate using the existing ACME setup. The bootstrap template serves only the challenge path and returns 503 for the app. Validate the HTTPS template with `nginx -t` before activation. The templates assume the conventional `/etc/letsencrypt/live/oto.ayani.id` paths; adapt to verified certificate locations.
6. Install and start the reviewed systemd unit. Check loopback `/health` (204), public HTTPS page, metadata, actual download hash, HEAD/range and application notification from an older installed build. Public download requires signing/distribution gate completion; do not call the development package Production-ready.

## Publishing a later release

Run desktop verification and local packaging, then Build-DownloadPackage with accurate release notes. Upload the new immutable ZIP first, verify server-side SHA-256/size, and upload catalog under a temporary name. Atomically rename catalog to `current.json` on the same filesystem only after the ZIP is complete. The app notices the newer version on its next eligible check. Web service restart is unnecessary for a desktop release. Rollback restores the previous catalog and package; existing desktop installations are not silently downgraded.

## Remaining acceptance

Linux service execution, public portal/download/client connectivity after activation, package signing/distribution admission and a full clean-Windows human install/update UAT remain outstanding. SSH/DNS and the existing placeholder's HTTPS were subsequently verified below. Neither portal activation nor installed-user activation of the final build is claimed.

## Verified delivery — 1 October 2026

- Desktop local release `20261001-094532-c58e4ca7`, manifest SHA-256 `21B8214B1DD88FA71047BF22B992DCD661E7496A5BD965F35399B27861CD645C`. 372/372 file lengths/hashes verified; desktop/worker/PDF-helper DLLs match the native-audited build byte-for-byte. No installed-pointer change or forced app closure.
- Download ZIP: 37,448,855 bytes, SHA-256 `2826328E3AA2A747FC1A861CF1EA2C85B88166C8A786F9F1BD41A911F68817ED`, `artifacts/web-distribution/PERTAMINA-OTO-Windows-x64-20261001-094532-c58e4ca7.zip`. Actual HTTP download and extracted Setup verification passed.
- Distribution/client/installer checks49 plus existing updater13 PASS: `artifacts/distribution-checks-final/test-results.txt` and `local-updater/test-results.txt`. Build-LocalUpdate now runs this gate before future builds. No external test framework/package added.
- Native notification/settings audit11 PASS: `artifacts/desktop-online-update/online-update-audit/audit.txt`. Wide and compact images reviewed. Visual review found inherited fixed-width Settings clipping; replaced the fixed column with a constrained star column and added a real toggle-bounds check. This supersedes the initial10-check layout result. The earlier delivery attempt was interrupted and superseded by the final verified build.
- Portal integration21 PASS on the final portable publish: `artifacts/portal-checks-d0b2cb017071428981e7442b1bfcb2fa/test-results.txt`. Includes actual attachment bytes, hash, HEAD/range, encoded notes, no write endpoint, malformed metadata and bad package rejection. Desktop worker41/editor63/search25/helper41 PASS in the standard delivery build. Evidence summary: `artifacts/online-release-verification/`.
- Responsive browser inspected at 1280px and390px; no horizontal document overflow. Actual synthetic reader preview only; no customer documents included.
- Found and fixed InvalidDataException handling separately from IOException; malformed metadata now fails quietly in desktop and does not break the portal. Automatic check also discards a pending result after the user disables notifications.

## VPS inspection and administrator handoff

User supplied `aynadmin@187.53.134.25`. Read-only verification confirmed DNS A `oto.ayani.id` → that IP, trusted SSH authentication, Ubuntu22.04.5 x86_64, existing Nginx HTTPS200 and Certbot site. Existing site is a197-byte placeholder. Host dotnet is absent; cached ASP.NET image is8.0.30. Port5088 was free. Other apps are left untouched. `sudo -n` reports a password is required; no credentials requested/stored, no sudo bypass through Docker attempted.

Existing Nginx config hash: `7ca6b06b4911ae5b094c025ce13f7be87365ab6e95b7d19d42ddd965da706fba`. Exact candidate retains existing TLS/Certbot settings: `eng/hosting/oto.ayani.id.vps-reviewed.conf`. Administrator activation script checks the baseline, service/path/port conflicts and runtime; backs up the site, health-checks the service, and restores the site on activation failure. Script is prepared for review, not claimed Linux deployment-tested. Runtime installation is separate; no new runtime/image was acquired by this task.

Runtime guidance was checked against Microsoft's Ubuntu22.04 instructions: .NET10 uses Ubuntu .NET backports; the existing ASP.NET8 container does not satisfy that dependency. See [official install guidance](https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install#ubuntu-2204). Do not add the old Microsoft feed assuming it supplies .NET10 for Ubuntu22.04.

Staging bundle: `artifacts/oto-vps-stage-20261001-094532-c58e4ca7.tar.gz`, SHA-256 `D71D4C5498C4D9CDFF7E614BDC038365DC3C26015F7BCAE1F83851694BDA3020`. Intended private server folder `/home/aynadmin/oto-downloads-stage/20261001-094532-c58e4ca7`. Contents are reviewed web publish, ZIP/catalog, checksums, configs and `SERVER-NEXT-STEPS.txt`; no local feed preference, server secrets or user documents. Activation requires administrator runtime setup and the documented public development-distribution/signing disposition; public site is not claimed replaced. Phase gates remain unchanged.

Upload completed to that private folder: archive checksum and all18 staged-file checksums PASS, `bash -n activate-vps-development.sh` PASS. Original Nginx hash remains unchanged after staging. No runtime download, service start, Nginx reload or public release activation occurred. Local installed current pointer was observed as `20261001-092910-960a246e`; the final `094532` package still needs the user's in-app installation action. Earlier/intermediate versions were not removed or reverted.


## EXE installer follow-up — 1 October 2026

The new primary download is the branded graphical EXE, with automatic .NET10 prerequisite detection and user-approved Microsoft runtime installation. Legacy ZIP metadata remains valid for existing notification clients. Current verified candidate is 20261001-102920-701ec3e1; preview port5089. Use eng/Build-WindowsSetup.ps1 for new EXE releases and upload both actual EXE/ZIP files before atomic catalog replacement. Updated portable publish and deployment bundle are prepared locally only. Prior server stage is retained but superseded as the candidate. Full tests, hashes, runtime intake and outstanding UAT/activation gates: [WINDOWS_SETUP.md](WINDOWS_SETUP.md). Earlier historical evidence above remains preserved.
