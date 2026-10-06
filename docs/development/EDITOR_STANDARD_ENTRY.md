# Standard Edit PDF entry — 2026-09-28

User request: match the existing tool entry screens; do not redesign the working editor.

Edit PDF now uses ShowEmptyTool when no PDF is open. It reuses the shared icon, title, primary picker, paste button, drop target, shortcuts guidance and validation notices rather than rendering an empty canvas. The editor's controls appear after accepting a single PDF. Picker/paste/drop use the common AcceptEmptyInputsAsync path and the same existing editor loader. Invalid documents return to the entry card with the editor's failure message. Existing open documents/drafts resume in the editor on navigation; they are not reset.

Closing a document and choosing Start new from the saved-result screen return to the same entry card, without automatically opening another picker. Dashboard-selected PDF input goes through the editor-opening helper. Processing and result presentation remain unchanged.

Native audit adds checks for the shared empty screen, accepted-document transition, and close-to-entry. Screenshot `editor-standard-empty.png` visually inspected at desktop size. Physical file-picker/paste/drop gestures and full accessibility remain manual UAT, not claimed as complete. Initial audit failed due to a forward-slash test path sent directly to the Windows StorageFile API; the harness now normalizes that path before calling it.

Verification: full rebuild passed; native audit exited 0 with 139 PASS at `artifacts/desktop-editor-standard-entry/live-editor-audit-76a912b4cbe14e29a738a97a426de6bd/audit.txt`. `git diff --check` passed with the pre-existing CRLF normalization notice. Local package delivery uses `eng/Build-LocalUpdate.ps1`; installed application is not overwritten or closed by this task.

Published local update `20260928-140844-77857b6b` (360 files). Delivery rebuild, 41 worker checks and 63 editor-worker checks passed. Install through Settings > Periksa update, then save work and reopen the stable Desktop shortcut.
