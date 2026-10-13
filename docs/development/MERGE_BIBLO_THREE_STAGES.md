# Merge Biblo — three stages

User-authorized local UI integration, 11 October 2026. Product version remains 0.3.0.

Three separate implementation chats, with disjoint ownership:

- Before: `01a12a46-150a-7083-aaad-0d9a838bbc7b` — empty screen, global paste routing, intake audit.
- During: `01a12a46-2420-7311-99a7-c23f0ca2a25c` — loaded workspace and processing/result settings panel.
- After: `01a12a46-32c3-7e63-bb6e-ae8b1b2c7fb5` — bounded saved-result screen and audit.

Integrator owns shared Biblo asset/helper, feature icons in home/catalog/workspace, project content inclusion, result provenance, audit entry point and local delivery.

Canonical asset: `output/imagegen/biblo-feature-icons-v1/028-gabungkan-pdf.png` in the parent workspace. Copied unchanged to desktop `Assets/Biblo`; SHA256 `436047EA92E6DC19165C6E486D789F3C1FC001A0F0DBC3461BB9202E51EFB4AD`.

Initial view has a rounded white card, prominent Pick PDF, paste shortcut and page-wide file drop. Loaded/unloaded handlers and asynchronous intake guards prevent navigation races and duplicate subscriptions. The card becomes compact for the supported 660×810 minimum window. Existing deduplication, format/size bounds and native text paste behavior are preserved.

Loaded workspace and actual processing show Biblo. Progress remains indeterminate because the worker has no percentage contract; cancellation uses the existing token. Saved result shows Biblo with a green success badge and preserves inline details, print, folder reveal and copy summary. Other features retain their original result status icon.

Evidence root: `C:/Users/cahpo/.codex/visualizations/2026/10/11/01a129fc-1d3a-7272-98b4-f28139fc1e4e/saved-result-fit/`. Retain initial `biblo-repair` failures: intake handler teardown was fixed; saved-result audit's obsolete packaged URI expectation was corrected for the unpackaged bitmap path. Final native audits cover initial minimum fit, intake guards, real merge order/source preservation, cancellation, result fit/actions and home navigation. Physical OS drag/paste was not independently automated; clipboard replacement was skipped when its non-text contents needed preservation.

Delivery uses `eng/Build-LocalUpdate.ps1 -PreparedBuild` only after scoped verification. No installation, version bump or online publication is authorized by this task.

Final evidence: `biblo-process/before`, `biblo-process/during`, `biblo-process/after`, plus `biblo-delivery/home`; standard gate log `biblo-final-update.log`. Processing temporarily collapses unrelated settings and restores them after completion/cancellation so the entire Biblo card is immediately visible. Native screenshots were inspected, including the minimum initial/result layout and full processing card.

Published local build `20261011-164536-399f3e72`, product 0.3.0 / Pengembangan. Manifest SHA256 `259A4F032E16AD01DD3A1E85284D5E35F7D4148FDB4B39A490013F7DADC8DCD2`. Verified all 1,334 payload hashes/lengths, matching audited desktop DLL and canonical Biblo asset hash. Installed pointer remains `20261011-160223-d2434548`; installation is left to the user's existing Periksa update flow.
