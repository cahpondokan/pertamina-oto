# Compact sidebar icon clipping

30 September 2026 — user supplied a compact sidebar screenshot with visibly clipped icons. Agent 0 applied a small layout fix in `MainWindow.LayoutCards`; phase gates are unchanged.

The 72 DIP compact rail has 12 DIP outer padding on both sides, leaving a 48 DIP button. Reusing expanded-mode 14 DIP button padding left only 20 DIP for the 24 DIP FontIcon, clipping its edge. Compact mode now uses 8 DIP button padding (32 DIP content space) and centered content. Expanded mode restores the original 14 DIP padding, left alignment and text labels. Icons, actions, selected-state colors, tooltips and accessible names are unchanged.

Desktop build exit 0. Existing native `--audit-history-ui unused` completed exit 0 with **35 PASS** at `artifacts/desktop-sidebar-clip/history-ui-audit/audit.txt`. Captures `07-narrow-list.png` and `01-initial-details.png` were visually reviewed: compact icons, including the previously clipped folder/history glyphs, display fully; expanded labels remain intact. No new test suite or dependency was added for this layout correction. Standard local package evidence follows below.

User delivery remains **Pengaturan → Periksa update → Pasang update**, followed by saving work and reopening the stable shortcut. Local publication does not change the installed/running version.

Published update **20260930-144755-9abaa2ed**. Standard clean rebuild and worker41/editor63/search25/helper41 passed; with the native history35 audit, **205 checks PASS**. Evidence: `artifacts/sidebar-clip-verification/package-build.txt`; helper report `artifacts/pdf-association-verification/pdf-association-9b5fdd74736a4969925dbb2e15dd1a6f/audit.txt`.

Integrity check exit 0: manifest SHA-256 `36BD66CD56E836E4FB79177401C21C1352BF0A466F71C6B00876ABB2D4E74553`; 368/368 payload lengths/hashes match, zero failures. Desktop/worker/helper DLLs are byte-identical to the audited build. Evidence: `artifacts/sidebar-clip-verification/package-integrity.txt`; previous feed `20260930-144154-279ce46c` retained. Installed app, documents and Windows defaults unchanged; no phase/Production gate promotion.
