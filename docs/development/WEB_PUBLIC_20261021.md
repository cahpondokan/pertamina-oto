# Web publication — 21 October 2026

Status: **PUBLISHED_AND_VERIFIED** at [oto.ayani.id](https://oto.ayani.id/). Operator activation exited 0 at **17:36:14 WIB on 21 October 2026**; independent public verification finished at 17:44 WIB. This is a scoped development website publication, not Phase 11 completion or corporate Production acceptance. No blocker remains for this requested publication.

User requested the reviewed local browser application be released at `oto.ayani.id`. Agent 0 alone prepared the application, verification and immutable deployment package. No agents or new packages were introduced. Existing dirty work and the preview source/server were preserved.

## Published release

- ID: `oto-web-20261021-098f54b06177`.
- Package: workspace-root `releases/web-browser/oto-web-20261021-098f54b06177/`.
- Payload: `payload.tar.gz`, **31,275,500 bytes**. Exact SHA256 and package pin: `artifacts/web-public-20261021/package-location.json`.
- Candidate browser: `http://127.0.0.1:5274/`.
- Public website: `https://oto.ayani.id/`; release ID verified through HTTPS.
- Archive SHA256: `88163a01391b53bd83d56a0348c123c79047576aefca07d0b55eede721e161a1`.
- Package JSON SHA256: `f30ad79e08d2f8e9448d9e6dd57bdc3f1e157c38a38f95f492221797884cdb70`.
- Existing reviewed preview remains `http://127.0.0.1:5266/`.

The download host now serves the reviewed ten-tool PDF application at `/`, `/favorit` and `/alat/{id}`. Existing desktop portal is retained at `/unduh`, the guide remains `/panduan`, and installer/download/updater routes remain unchanged. Download cards use the validated existing catalog; snapshot URLs, sizes and build labels are not the public source of truth.

The PDF engine, worker, preview renderer, shared workspace, icons, CSS and browser scripts are linked unchanged from the existing preview. Public homepage/footer wording identifies a development version. PDF.js license has an explicit text response because extensionless files are not served by the normal static file middleware.

Every request except GET/HEAD is refused. CSP retains self-hosted scripts/workers and forbids network connections from page/worker code, embedded objects, frames, form submissions and eval. No document API, upload endpoint, server processing or document storage was added. Initial site loading requires network; this is not an installed offline PWA.

## Verification

- Framework-only restore used the existing no-feed NuGet configuration; final publish passed. No runtime or library download.
- 499/499 existing engine/worker/model tests passed, including network-denied processing and negative PDF cases. Preview source fingerprints remain unchanged.
- Final immutable package and public HTTPS: 32 HTTP verification groups passed on each, including supported static asset hashes, ten active tools, inactive-route 404, POST 405, CSP, licenses, HEAD, guide, download range support, exact metadata equality and complete online-installer SHA256. Public desktop remains 0.3.0 / `20261014-002535-877d8390`.
- Actual browser: source page images for synthetic A/B displayed; Merge produced six pages; file downloaded and independently parsed with strict pypdf, with text on all six pages. Original synthetic input bytes unchanged.
- Public browser: synthetic A/B thumbnails rendered, Merge produced six pages, and the downloaded file passed independent strict pypdf parsing with text on every page. No browser error logs. Screenshots: `public-home.png`, `public-merge-result.png`; machine evidence: `public-browser.json`, `public-downloaded-merge.pdf`.
- Deployment helper: Windows 10 passed / 4 Linux-only skipped; all **14/14 passed on Linux**, including real symbolic-link fixture cases. These use isolated fixtures and mocked service operations; a deliberate root/systemd rollback has not been performed on the live service.
- Private Linux runtime smoke passed for 12 routes, CSP, POST refusal and empty-catalog fallback; all 337 runtime files matched the previously admitted official archive. No new dependency download or runtime installation.
- Exact staged activation exited 0, including guarded current-pointer replacement, service restart, loopback/HTTPS and catalog/config preservation checks. Independent read-only SSH then confirmed service active and unchanged unit/Nginx hashes. Unprivileged readlink could not inspect the private current pointer; the activation helper performed that check under operator sudo.
- Final local preservation check: all 279 preview source fingerprints and immutable archive/package pins still match.
- PowerShell package `VerifyOnly` passed. No commit or merge created.

Evidence directory: workspace-root `artifacts/web-public-20261021/`. Key records: `TASK.json`, `baseline.json`, `tests.json`, `linux-deployment-tests.log`, `linux-runtime-final.log`, `private-upload.log`, `operator-activation-result.json`, `public-http.json`, `public-browser.json`, `preservation-final.json`, `package-location.json`, `handoff.json` and the public screenshots. Earlier local/failed evidence is retained.

Two earlier package candidates are retained but rejected: `ff174ca6d9f9` and `7629ba34e7e7` had incorrectly decoded comparison metadata. Final metadata is parsed from raw UTF-8 bytes and its embedded release-notes SHA256 is checked. Public data was never altered. An initial missing C# property and duplicate static-asset include were fixed before final publish; failed/final build logs remain. Later sandbox launcher failures required approved local execution outside that unavailable launcher.

## Deployment and rollback boundary

Read-only SSH verified the pinned server identity and active `oto-downloads.service`. Installed release: `/opt/oto-downloads/oto-web-20261021-098f54b06177/web`. Previous release retained for rollback: `/opt/oto-downloads/oto-update-20261014-002535-877d8390/web`. Unit SHA256 `59e8478576c9ca537e7ddc68320575d1b44e911b28a01165883cbbe39e483551`; Nginx SHA256 `1e5024400b382868453d12e3c73b723ed436d606dc8b15a5312db1b664dfd767`. Sudo credentials were entered only by the operator in their terminal.

After explicit export approval, the pinned web package and unpack helper were uploaded to `aynadmin@187.53.134.25` and passed archive/hash/inventory verification. The payload includes compiled web binaries/assets, `publication.json` and readable `deploy_web.py`; `unpack.py` is separate. Authorized isolated test source was staged separately for the Linux tests. No documents, credentials or desktop payloads were uploaded.

`-Action Activate` requests sudo in the user's terminal. It verifies current pointer, unit/Nginx/catalog hashes, installs a new immutable directory, rechecks all copied files, switches only the website symlink and restarts the existing service. It checks loopback and HTTPS before success. Failure attempts a guarded restoration of the exact previous pointer. No Nginx/unit/runtime edits or desktop feed writes occur. `-Action Rollback` rejects later pointer/catalog/config drift. Failed/replaced release directories are retained.

The operator launcher was corrected to use .NET SHA256 because native Windows PowerShell could not resolve `Get-FileHash`. The failed launcher stopped before SSH; the corrected `Activate-VerifiedWeb.ps1` activated the exact staged package without uploading again. A matching `Rollback-VerifiedWeb.ps1` is prepared in the evidence directory, but has not been executed. Rollback validates helper/metadata pins and rejects pointer/catalog/config drift; it requires operator sudo and should only be used for an intended rollback.

## Authorization and limitations

Automatic approval review initially rejected upload/execution of source helpers in private VPS staging as an export requiring specific consent. That command did not execute. The user subsequently answered **“Izinkan unggah, pengujian, dan aktivasi”**, explicitly authorizing this package and deployment/rollback/unpack/test source for this VPS. Export, staging tests and activation then proceeded within that scope. The user confirmed **“Sudah selesai di terminal”**; exit 0 and independent HTTPS/browser checks establish success.

The site remains labeled a development/test version. Ten tools are active; eight catalog tools remain unavailable on web. Existing format/size limitations still apply, and users should inspect results. Network is needed to load the site; installed offline PWA support and broad device/accessibility UAT are not claimed. Canonical Phase 0–11 status and required human Production sign-offs are unchanged. Agent 0 alone completed the scoped work; no commit, merge or desktop release was created.
