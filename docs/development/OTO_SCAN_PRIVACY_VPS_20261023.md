# OTO Scan privacy page on the existing VPS

User authorization on 23 October 2026: publish the approved OTO Scan privacy policy at `https://oto.ayani.id/oto-scan/kebijakan-privasi`, then update its Google Play Console URL while keeping the existing OTO site working.

## Current status

**Published, verified, and saved in Play Console.** The operator entered sudo directly in the requested interactive terminal; activation returned exit 0 at `2026-10-23T02:08:48.9168152Z`. Independent HTTPS verification passed at `2026-10-23T02:10:34.540248+00:00`. Play Console confirmed “Change saved. Send for review in Publishing overview.” The saved URL is `https://oto.ayani.id/oto-scan/kebijakan-privasi`; unrelated release changes were not submitted for review.

The new Razor page uses the approved policy text, app icon and public support address. It adds an independent CSS file and uses the current strict CSP without scripts, trackers, forms or external assets. The app package is `com.niam.ayani.oto`. There is no APK/AAB or desktop application change.

## Package and preservation

Candidate: `oto-web-20261023-29f59f4f0beb` (688 files). Package JSON SHA-256: `ba949352b9ed34c68704945c444c633380a37dbe4dc75d82bebc934f442a0d8a`. Archive SHA-256: `226df7b6496beecf6256f7e2e9dd9e69762d1ff748d3bc6422223b9f30a72bf7`.

Previous public website: `oto-web-20261021-098f54b06177`. All its existing static assets match exactly. Changes are the compiled web assembly/PDB/static asset index/release marker, plus the new policy CSS (and build-generated compressed copies) and app icon. The existing homepage, tools, guide, updater and downloads retain their source and assets. The deploy helper checks the exact predecessor, Nginx/service hashes and desktop catalog pins before changing the web pointer. It verifies the new page and icon/CSS hashes after activation and restores the previous pointer if verification fails.

The website-only deploy does not modify Nginx, systemd configuration, TLS, installed runtime, desktop release catalog or desktop download artifacts. Protected catalog hashes are rechecked under sudo at activation; the unprivileged preflight could not read them. Their public metadata is unchanged from the previous publication.

## Verification

- .NET publish without dependency restore: PASS.
- Package upload checksum and complete server-side inventory: PASS.
- Deployment tests: 14/14 PASS on Linux, including guarded rollback and pointer/config drift rejection.
- Private VPS runtime: 15 routes PASS, including new policy/CSS/icon and existing website routes; POST refusal and empty catalog handling PASS. This test did not change the public service.
- Local HTTP: policy/assets and existing home/tool/guide/download routes PASS; browser visual review shows the correct icon, readable text and sections.
- Public preflight: home, tools, guide, downloads, health and updater metadata available; exact hashes retained for comparison after activation.
- Independent public verification after activation: new HTML, icon and CSS available over HTTPS; exact response hashes on all 10 baseline routes unchanged; desktop metadata, download HEAD/ranges and strict CSP preserved. Public browser page and saved Play Console URL verified visually.
- Flutter APK/AAB SHA-256 values unchanged. Store URL templates and current handoff updated; original release source manifest and old hosting evidence retained.

Initial Windows tests hit sandbox temporary-folder restrictions; rerunning with a workspace-owned temporary directory passed the 10 platform-independent tests (4 Linux-only tests skipped, subsequently all passed on Linux). The first local server was inaccessible from the browser under sandbox networking; the normal localhost preview passed after running with the required network access. The package comparison initially flagged expected build-generated CSS compression copies; these were explicitly included and the immutable payload verified without modification.

The first independent Windows verification decoded UTF-8 publication metadata using the Windows locale and falsely reported a release-notes mismatch. A direct byte/UTF-8 comparison proved the live metadata was identical. The verifier now explicitly decodes JSON bytes and passes; the deployed immutable package was not modified. The first Windows PowerShell launch closed because local script execution was not enabled. The requested interactive terminal was relaunched with process-only `RemoteSigned`; no permanent execution-policy setting was changed.

## Evidence and recovery

Evidence directory at workspace root: `artifacts/oto-scan-privacy-vps-20261023/`. See `operator-activation-result.json`, `public-after.json`, `play-console-update.json`, `public-privacy.jpg`, `play-console-saved.jpg`, and `STATUS.json`. The operator's password was entered only in the SSH terminal, never in chat or a saved script. Do not rerun activation: its result and the immutable installed release already exist.

The existing pinned deployment helper supports guarded rollback to `oto-web-20261021-098f54b06177`, checking the exact current pointer and catalog/config hashes before switching. The old release directory and old Sites privacy page remain available. Rollback was not performed. Current Flutter evidence is `worktrees/android-scan-a0/flutter/oto_scan/evidence/play-release/privacy-vps-deployment.json`; historical hosting evidence is unchanged. This completed privacy-publication scope is not a project phase completion or app production release.
