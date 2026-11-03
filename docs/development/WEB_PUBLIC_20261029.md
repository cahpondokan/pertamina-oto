# Web publication — 29 October 2026

Status: **PUBLISHED_AND_VERIFIED** at [oto.ayani.id](https://oto.ayani.id/). User-authorized website-only release. This is not Phase 11 completion or corporate Production acceptance.

## Release

- ID: `oto-web-20261029-c158fce09827`.
- Previous web release: `oto-web-20261027-1baeff233927`.
- Immutable candidate and verification artifacts: `artifacts/web-public-20261029/`.
- Archive: 31,950,358 bytes; SHA-256 `ab73495ac23c2114abec1badaed2d2afcb2db8a4b0ec80a8990c552604f4d42f`.
- Package JSON SHA-256: `ad4274a06aa6d851d3de2a20e20808f0eaa80e60c433d2419e584edde1e02d29`.
- Publication manifest SHA-256: `5857579908076e1eec9826a2a0d01a8cfb7c650faa5100f5a687c31126f7eef4`.
- Release inventory: 764 files. Candidate `Downloads` host built in Release without dependency restore.

## Activation and verification

- Local package hash verification and complete inventory: PASS.
- VPS private staging: checksum, bounded extraction, helper pin and 764-file inventory all PASS; no public change during staging.
- VPS activation: operator entered sudo in their terminal. Helper reported loopback and HTTPS checks PASS, then `PASS published oto-web-20261029-c158fce09827; desktop feed and hosting config unchanged.`
- Independent public HTTPS check: full asset inventory, browser routes, privacy page/assets, CSP, restricted routes, installer metadata and download ranges PASS.
- Public marker confirms date `2026-10-29`, 10 browser tools, `documentUpload=false`, the OTO Scan privacy route, and anonymous diagnostics retained.
- Existing Split browser QA: 11/11 scenarios PASS; full local Node suite: 878/878 PASS.

The first Windows-side metadata comparison exposed PowerShell's mojibake of the multiplication sign inside `releaseNotesJson`. Both public desktop metadata objects matched after Windows UTF-8 decode normalization; the full public HTTPS verifier then passed. The pinned VPS verifier also passed during activation and in a repeat check. No desktop feed, hosting configuration, or desktop release artifact was changed.

## Hotfix — Split PDF picker mount

Published `oto-web-20261029-44379e076266`, replacing the release above. The Split workspace mount assigned a handler to the absent `pdf-new` button and threw before wiring the PDF picker. The handler is now assigned only when that optional button exists.

- Previous release: `oto-web-20261029-c158fce09827`.
- Archive SHA-256: `1e88ac4b66f2bdacb6458eeaf56e8076702fcf20744b121dc843c8fec57079cf`.
- Package JSON SHA-256: `b9f59525498a8e12360ee88f2cb422fc99e25a34c91ebd619e19a6a9c9d047b9`.
- Manifest SHA-256: `a9f7292f464f89b2ff463b57bf1edbe24bb5df9678071f1d56b6df5df102be21`.
- VPS bounded extraction and inventory: PASS. Activation's loopback and HTTPS checks: PASS. Independent repeat HTTPS route/security/assets/desktop metadata check: PASS.
- Public marker confirms 10 browser tools, the OTO Scan privacy page, anonymous diagnostics, and no document upload. Live Split page reloads without JavaScript errors; clicking **Pilih PDF** now opens the browser file chooser.

An initial attempt to insert the synthetic fixture through the ChatGPT Chrome extension was blocked by its local-file permission, after the chooser event had fired. No extension or system permission was changed. A separate isolated Playwright audit then exercised the complete flow successfully.

## Post-hotfix functional audit

Audited on 29 October 2026 against both `https://oto.ayani.id/alat/split` and the open localhost preview, using an isolated headless Chrome profile and the 7-page synthetic fixture `split-modes.pdf`.

- Home ↔ Split navigation: PASS on both targets; no page or console errors.
- Clicking **Pilih PDF** opened the file chooser; selecting the fixture loaded all 7 pages.
- Ranges 1–2 and 3–6 produced two results.
- The first downloaded result parsed as a valid 2-page PDF; the ZIP download had a valid ZIP signature and non-empty contents.
- Full live audit evidence: `artifacts/web-live-audit-20261029/audit-live-picker-flow.json` and `audit-navigation.json`.

The user's own PDF was not provided, so this audit confirms the tool with the synthetic fixture rather than diagnosing any file-specific failure. The older browser QA script also expected a per-row download button for a single-result view; the current UI intentionally uses the main save action in that case, so that stale selector was excluded from the focused audit.
