# Tab Menyatu — desktop header

Scope approved by the user on 25 October 2026: implement design 04, use Beranda as the single tool hub, and prepare a local update. Product identity remains 0.3.0 / Pengembangan; a new immutable build identifies this presentation update. No automatic installation or web publication.

## Implementation

- One 48-DIP dark window header owns branding, Beranda, persistent document tabs, new-document action, application menu and native caption controls. A separate non-interactive drag target preserves button/tab input. The normal system title bar remains the fallback when customization is unsupported.
- Beranda holds the tool catalog. Existing PDF sessions remain mounted, retain reading/search state, and can be activated directly from the global tab strip. Beranda clears only the tab selection, not the document session.
- The reader removes the duplicated filename/action header. Its contextual toolbar contains page thumbnails, Baca, Atur halaman, search, page navigation, zoom, Cetak, Edit PDF, focus, bookmark and document overflow. Search/Cetak shortcuts are tooltips. No duplicate Semua alat action.
- On compact windows the toolbar reflows into two rows and secondary text labels become named icon buttons. The thumbnail panel is optional. Embedded tool previews retain their original layout.
- History, settings, help, session documents and existing desktop commands remain in Menu aplikasi. Busy/modal guards, file identity, processing validation, source locks and update integrity are retained.

## Verification and evidence

Evidence root: `artifacts/reader-header-20261025` in the main workspace (outside the checkout).

- `audit-packaged-reader/audit.txt`: 66 PASS, including tab deduplication, page/zoom/scroll/search retention, Beranda round trips, detach/rejoin, close and unsaved-draft protection, source SHA256 invariance, and bounds checks at requested widths 1480/1024/800/600. The native minimum size still applies; a 600-pixel request does not imply a supported 600-DIP client.
- `activation-checks-native/checks.txt`: 15 PASS, including independent process forwarding, concurrency, literal paths and rejected malformed/remote inputs.
- `native-input.md`: real native clicks found and then verified the fix for the selected-tab/Beranda issue. Synthetic page 2 was retained after the round trip.
- `build-verified.log`: final self-contained desktop publish. `.NET 10.0.12` remains private and pinned; no new dependency was introduced.
- `local-delivery-verified.log` (exit 0) and `package-verification.json` are the authority for final delivery status. Earlier candidate logs are historical attempts, not final package evidence.
- Before snapshots and final source hashes preserve the exact scoped edit against the pre-existing dirty checkout.

No physical multimonitor/DPI, touchscreen, screen-reader or clean-machine certification is claimed. Native audit inputs are synthetic. The original installed application remains the user's running version until the user applies Update lokal.

API reference consulted: https://learn.microsoft.com/en-us/windows/apps/develop/title-bar?tabs=winui3

## Final local delivery

Build `20261025-213630-60869e33`, product 0.3.0 / Pengembangan, **1,366 files / 554,263,457 bytes**. Manifest SHA256: `CFFAFDFC4180C9053C0955F5825CA6FBDE80A7174233572073C621F7245B64DA`. Desktop SHA256: `CDAC65911496855957B1784878EFD839163D200C1A2E011EC7CA5746DD8B856D`.

Standard delivery completed with exit 0; Word UI 39 and quality 39 checks passed. The exact tested desktop, release notes and validated protocol-2 restart capability are in the hashed package. All 193 launcher capability entries also match. Installed pointer remains `20261019-235909-abd98cbe`. Online publication: none.

User action: **Menu → Pengaturan → Update lokal → Pasang update** after saving work. Online Periksa update refers to the website, which does not host this build.
