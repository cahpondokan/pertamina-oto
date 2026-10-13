# Favorit and Riwayat — 2026-09-16

Favorites saved on each star mutation; successful shared result panel records job date, tool, output paths and summary, capped at 200 jobs. Storage: LocalApplicationData/PertaminaOTO/library.json. No document bytes/passwords. Atomic same-directory temp publication; parse/schema failure preserves existing data and blocks saving with a notice. This is local metadata, not encrypted storage or document backup.

Favorites: search, compact rows, remove/Undo. History: search, tool filter, date groups, open file/folder, multi-output selector, delete metadata only. Missing output records retained. Activity sidebar hidden until a real queue exists. Existing session-only history lost before this version cannot be reconstructed.

Verification: VS2026 MSBuild Debug passed; native isolated --self-test-library passed reload, missing file retention, persisted removals and corrupt-store preservation. Tests do not overwrite the user's library. git diff --check passed. Build artifacts/desktop-library. GUI visual/restart acceptance remains manual.

Limitations: one active application instance recommended; concurrent instance writes are not merged. Metadata has no account sync. Native full paths may expose filenames to other processes with access to the same Windows profile. No phase completion or production claim.
