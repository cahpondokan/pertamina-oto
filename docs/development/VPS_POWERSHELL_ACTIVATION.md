# User-run VPS activation package — 1 October 2026

## HTTPS404 retry correction — current package

User's real sudo activation passed systemd/runtime and loopback health/metadata/full EXE verification, then returned HTTP404 immediately after Nginx reload. Read-only SSH confirmed original site hash restored, service inactive/disabled and root-owned marker `failed-retained-for-retry` at the prior manifest `fdce8cbf1a78c44c3f7f6e9e7b6e623313bbb2a2ce64b849ee74b2d04aa70e31`. Privileged journal access is unavailable to the SSH account. The exact original HTTP failure path/timing cannot therefore be proved retrospectively, but code issued public verification immediately after reload with no readiness wait. Nginx's asynchronous reload behavior is documented at https://nginx.org/en/docs/control.html.

Corrected helper waits at most45 seconds for public health204 before full verification. Transient404/502/503/504 or connection failures may retry; wrong200,403 and TLS validation failures do not pass. HTTP errors now identify the endpoint. Metadata/CTA/HEAD/range/full EXE SHA256 checks and guarded rollback remain mandatory. A narrowly pinned migration permits only the exact prior failed marker, same deployment, original Nginx and matching unit. Active/unknown predecessor states or altered files still stop. No asset/runtime/application change.

The existing user command/path below now runs the correction. Updated ZIP123,394,524 bytes, SHA256 `2E87BC4400D1917B308B1E10450FED2CCB43C242407AE92ACE2FE07F5519DF27`; new full contents manifest `a44411b9f6ff422c58b5fa8192f96c1d117d6ac6e4e24cab8a40f355bc3407bf`. Immutable payload archive and private staging path remain the same. Original delivered ZIP retained in `artifacts/vps-activation-reload-verification/before-reload-fix.zip`.

Verification:32 final Python/Linux tests PASS; actual independent unprivileged Nginx on loopback50839 confirmed persistent404 rejection and delayed reload404-to204 success, then stopped. Nginx emitted permission warnings for its compiled-in default log before reading the private config; configured private process and logs worked, all commands exit0, public config unchanged by the test. Full PowerShell5.1 staging/manifest/preflight accepted the actual failed predecessor without sudo. Final delivered package verifies in PowerShell5.1 and7. Evidence `artifacts/vps-activation-reload-verification/linux-tests.txt`, `test_real_nginx.py`, `delivery.txt`, `server-proof.txt`; candidate `artifacts/vps-activation-reload-fix-01`. Initial package facts below are historical and superseded where indicated.

Final observation during follow-up: the corrected package was activated outside the agent's read-only checks. Exact new root-owned manifest marker is now `active`, service `active` and `enabled`, site hash equals candidate `1e5024400b382868453d12e3c73b723ed436d606dc8b15a5312db1b664dfd767`, and public health204. Independent external verification from Windows PASS: HTTPS health204, exact version/installer metadata, main EXE CTA, HEAD size,206 byte range and full37,575,168-byte EXE SHA256 `E6704300B8E3D40FE8E60F228CB942CF2C06E72622D4594859BCCB7A46C139B1`. Evidence `external-public-proof.txt`, `public-metadata.json`, `public-home.html`, `public-exe-head.txt` and downloaded EXE in the same evidence folder. No installer executed and no agent sudo command issued. Scoped development portal activation is now verified; clean-Windows UAT, signing and phase/Production gates remain unchanged.

The user explicitly requested VPS activation and a package directly runnable from PowerShell, after disclosure that the EXE is a development candidate without signing. This authorizes this scoped development portal/manual-download activation workflow. It does not claim Production/phase11 readiness or a signing approval. Final sudo action belongs to the user's terminal; no credential is requested or stored in chat.

## Delivered package

Workspace-root `releases/vps-activation/oto-20261001-102920-701ec3e1-v1/Aktifkan-OTO.ps1`. Portable ZIP: `releases/vps-activation/OTO-Aktivasi-VPS-20261001.zip`,123,383,734 bytes, SHA256 `815BC04C4C3382A756CECAC94DCE25FD27DC8C669F2EC194ED34FB2914563DDD`.

Run from ordinary Windows PowerShell:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "D:\AFT Ahmad Yani Semarang\Teknik\Dokumentasi Pertamina OTO\releases\vps-activation\oto-20261001-102920-701ec3e1-v1\Aktifkan-OTO.ps1"
```

ExecutionPolicy applies only to the child process; no permanent policy modification. OpenSSH Client is required and was found. Normal SSH/sudo prompts appear directly in the terminal; no password parameter/file/transcript is used. Keep the terminal open until the final success/error message. `-Action VerifyOnly` checks local files; `Check` stages and checks without sudo; `SmokeTest` temporarily serves loopback; `Rollback` restores the guarded prior Nginx config and disables only the OTO service while keeping files. Default is `Activate`. Exact package can be retried; unrelated/existing deployments or changed Nginx/unit/host keys cause a stop.

## Scope and implementation

PowerShell5.1/7 validates its pinned package manifest, files and trusted SSH ED25519 key (fingerprint `SHA256:J7+ll/4vwXOWWQuJcUE2s8Ozv+DOojWBWC5QV+i9Ujg`). Uploads use a private folder under `/home/aynadmin/oto-activation/`; complete matching archives are reused. The compressed asset archive is immutable; small versioned control files have separate SHA256 pins and a complete post-overlay inventory/hash check before sudo. This lets fixes to deployment control code reuse the large verified data upload. The Python3.10 standard-library server helper rejects archive traversal, links, special files, duplicates, oversize and unlisted content.

Final private server staging: `/home/aynadmin/oto-activation/oto-20261001-102920-701ec3e1-v1-7fef7923cac2`. Asset archive SHA256 `7fef7923cac268a142a6bbe7b9b97570e3c8ad7d0bc5d0f941e24ae853d3e488`; final contents manifest SHA256 `fdce8cbf1a78c44c3f7f6e9e7b6e623313bbb2a2ce64b849ee74b2d04aa70e31`. Final control files and full contents verified on the actual VPS. Previous staging/candidates are retained.

Fresh VPS preflight: Ubuntu22.04 x86_64, DNS correct, Nginx baseline `7ca6b06b4911ae5b094c025ce13f7be87365ab6e95b7d19d42ddd965da706fba` unchanged, port5088 free, deployment targets/service absent. libc6, libgcc-s1, libstdc++6, libicu70, libssl3, zlib1g, libgssapi-krb5-2 and ca-certificates installed. sudo still requires the user's password.

The activation creates root-owned application/runtime/release files and a non-login `oto-web` account. ASP.NET10.0.12 is isolated in `/opt/oto-downloads/runtime/10.0.12`; no apt repository/global runtime/PATH changes. systemd binds Kestrel to127.0.0.1:5088 under a restricted account. Nginx/Certbot settings for this exact site are preserved, backed up and replaced only after loopback health, metadata, HEAD/range and full EXE hash checks pass. Public HTTPS checks repeat before enabling success. Root filesystem paths/markers and existing service identity are guarded; permissions do not depend on the operator's umask. Interrupt/failure attempts guarded rollback. Runtime/service files remain for retry; unrelated services/containers, firewall and documents are untouched.

## Runtime intake

Official Microsoft ASP.NET Core10.0.12 linux-x64 archive downloaded after scoped intake in root task `VPS_ACTIVATION_TASK_20261001.md`. URL: https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/10.0.12/aspnetcore-runtime-10.0.12-linux-x64.tar.gz. SHA512 from official10.0 release metadata: `c489fc3a9dcc85f24f2d52e9a81b46fdf51affc8a360d008dbd5e3b7ce524969adc4a54f555ad13e29b822f59ed1cae9785608cbac2282f2c9c4fda45c21c7d1`. Verified before use. Original LICENSE.txt and ThirdPartyNotices.txt remain in the archive and installed runtime. Official guidance https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual supports the extracted ASP.NET bundle including both runtimes. Version-pinned license https://github.com/dotnet/aspnetcore/blob/v10.0.12/LICENSE.txt reviewed. No downloaded executable script or TLS bypass. Private runtime updates require a later verified package, not automatic apt updates.

## Verification and limits

- Final PowerShell5.1 and7 local verification PASS. Final corrupt fixture fails before SSH. Packaged script executed through Windows PowerShell5.1 against the real VPS; trusted host pin, cached archive reuse, control overlay, bounded extraction and full manifest/preflight PASS.
-22 Python checks PASS on final staged Linux code: traversal/duplicate/link/corrupt archive failures, safe retry, unrelated service/deployment and Nginx drift refusal, rollback preservation and failed Nginx validation restoring the candidate. Root command calls in rollback tests are stubbed; no privileged rollback was executed.
- Actual .NET and ASP.NET10.0.12 binaries ran as `aynadmin` in private staging, started the real portable portal on an ephemeral loopback port, and passed health204, metadata, EXE CTA, HEAD,206 range and full EXE SHA256/size. Temporary process stopped. Identical web/release/runtime assets remain in final package; subsequent changes were guarded control-file/permission handling only.
- Public Nginx checksum unchanged after tests; /opt/oto-downloads, /var/lib/oto-downloads and systemd unit still absent. Public `/health` remains404 on the original placeholder. No sudo activation, reload, package-manager change or Windows installation performed by these tests.
- Evidence: `artifacts/vps-activation-verification/local-tests.txt`, `linux-final-tests.txt`, `server-proof.txt`, `delivery.txt`, `final-corrupt-test.txt`. Source: `eng/hosting/powershell/` and `eng/hosting/Build-ActivationPackage.ps1`. Existing dirty work preserved; desktop code/feed not changed.

Package preparation/testing is complete. User's default PowerShell run will perform the actual privileged activation and final public checks. Actual systemd/UAC/operator acceptance, signed release and phase gates are not represented as passed by the preparation tests.
