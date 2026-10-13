# Download portal refresh — 11 Oktober 2026

Status: VERIFIED_LOCAL_WITH_LIMITATIONS. Scoped local WEB work, source frozen for Agent 0 integration. No phase execution, deployment, installation, version change, new dependency, subagent or cross-chat message.

Baseline: task/core-functional-first, HEAD 84cbb0f538020003bcfb6583e94c2fcf6524f87c, dirty existing files preserved. Host hashes recorded before edits in workspace-root output/task-plans/distribution-refresh-20261011/WEB/baseline-hashes.json. baseline-status.txt was captured after first layout edits; it is not a pristine pre-edit status.

## Result and contract

Compact Indonesian Razor home retains existing brand, synthetic reader image and Panduan. Online EXE is primary and full offline installer secondary only after every baseline artifact validates. Schema1-only 0.2.4 still shows its actual full installer and explains online unavailability. Missing/invalid baseline fails closed without a stale legacy offer. Metadata supplies product/build/channel/date/size/hash/notes/runtime. No claim that all 111 features work.

Terang/Gelap/Ikuti sistem selection persists locally and tolerates blocked storage. Real links work without JS. Closable aria-live notice says only “Permintaan unduh dikirim. Periksa unduhan browser.” and expires after 10 seconds. One same-origin external script requires CSP script-src self; no unsafe-inline/eval, CDN or analytics. Focus and reduced-motion CSS retained.

APP/CONTRACT_READY.json checkpoint1 freezes DistributionRelease.cs, schema2 baseline1. Host links that source and validates bounded distribution-v2.json, exact names, actual lengths/SHA256 and reparse-point-free paths. /api/releases/windows-x64/baseline-1/latest returns schema2/no-store or204. Legacy latest returns204 whenever a baseline catalog exists and never serves schema2. Installer compatibility endpoint projects the verified baseline to guarded schema1/manualInstallerOnly=true. Panduan uses the same baseline offline installer. GET/HEAD/range remain supported.

## Verification

Evidence root: artifacts/distribution-refresh-web-20261011.

- WEB-only dotnet publish --no-restore -c Release to publish-accepted: exit0, build-accepted.txt. No solution build/dependency acquisition.
- eng/Test-DownloadPortal.ps1, publish-accepted, port5192: 48 PASS, legacy-accepted/test-results.txt.
- tests/DownloadPortalRefreshChecks/Test-Refresh.ps1, frozen APP fixture, port5194: 36 PASS, refresh-accepted/test-results.txt. All three actual HTTP fixture bytes/hash, HEAD/range, metadata/no-store, schema/path/baseline/hash/length/missing/malformed/POST rejection and recovery. Fixture illustration sizes/hashes are replaced with actual inert test bytes; no fixture executable is run.
- Test-ExistingPackage.ps1, guarded real root releases/web-upload/0.2.4-20261009-113931-5d181036/contents/releases, port5195: 10 PASS, real-public-catalog/test-results.txt. Full actual 296400896-byte EXE and296273916-byte ZIP HTTP SHA256/HEAD/range, full-installer fallback and legacy204 passed.
- node tests/DownloadPortalRefreshChecks/interactions.cjs: 9 PASS, interactions-final.txt. Storage failure, system changes, ordinary navigation, honest notice, close and expiry.
- Final browser DOM: 390/768/1024/1440 viewport × light/dark/system, zero horizontal overflow/broken images in all12 cases, browser-final.json. System resolved to dark. Saved dark preference survives reload; Enter activates download and checksum details; close hides status, browser-interactions.json. Tab focuses offline link with3px solid outline, browser-focus-palettes.json. Measured body/muted/CTA/offline-link contrasts exceed4.5, contrast.json; not a comprehensive accessibility audit.

Twelve actual earlier-iteration screenshots: portal-{390,768,1024,1440}-{light,dark,system}.jpg. Desktop/light and mobile/dark visually inspected. Final screenshot API failed with “Unable to capture screenshot”, including fresh-tab retry. Earlier screenshots show MB-rounded tiny fixtures; final DOM verifies accurate byte/KB/MB text. Do not label earlier screenshots final. No generative screenshot used. Browser zoom shortcuts did not change innerWidth/devicePixelRatio:200% NOT_VERIFIED. Reduced-motion OS emulation, screen reader, physical DPI/human UAT NOT_TESTED.

Initial sandbox server attempts failed on denied ASP.NET key-ring writes; authorized localhost execution passed without security-policy changes. real-existing initial run used the unguarded local package catalog, not the guarded web catalog, and failed its expected204 assertion. Corrected real-public-catalog run is authoritative. refresh-final exposed stale compatibility endpoint metadata; source fixed and publish-accepted rerun passed. Failed/intermediate evidence is retained.

## Freeze and integration

Candidate: publish-accepted. build-inventory.json pins every output. source-freeze.json and source-changes.json bind host sources to initial hashes; contract-binding.json matches all frozen APP hashes. Host was already untracked, so hash before/after records are supplied rather than inventing a pristine Git text diff. No commit/stage/push.

Final preview address was http://127.0.0.1:5194/ with Distribution:FixturePreview=true. Background preview is stopped at teardown. Restart from publish-accepted working directory: dotnet Pertamina.Oto.Downloads.dll --urls http://127.0.0.1:5194 --Distribution:Root="ABSOLUTE_PATH_TO/refresh-accepted/fixture-releases" --Distribution:FixturePreview=true.

Agent0 must test this frozen WEB build with APP's real admitted online/offline/update artifacts and distribution-v2.json. Fixture readiness is not publication approval. APP version/signing/security/distribution/native baseline→next release gates remain outside WEB ownership.

Keep ASP.NET/systemd/Nginx. Future authorized operator must capture fresh config/service/current-web/catalog drift evidence, retain previous versioned web and immutable packages plus both catalogs, upload immutable files first and verify all hashes. Promote guarded schema1 current.json before atomically promoting verified distribution-v2.json last on the same filesystem, after candidate web health/guide checks. Rollback restores captured prior web and both catalog states, including prior presence/absence of distribution-v2.json; retain all binaries. Existing Build-ExistingUpdatePackage/update_existing only understands old schema1 inventory: do not use unchanged for schema2/online EXE. Extending and testing that helper is a future integration/publication gate. No SSH, remote preflight, upload or activation ran in WEB.
