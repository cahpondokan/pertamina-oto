# Print feature — 7 October 2026

Status: COMPLETE_DEV_LOCAL_PRINT. Coding complete, development gates pass, local update ready. Product **0.2.0 / Pengembangan**, immutable build **20261007-141314-96a5fba4**. Update is published to the local feed and has not been installed or uploaded online. This is a standalone feature delivery, not a phase exit, main promotion, installer release, or Production approval.

Package: `D:/AFT Ahmad Yani Semarang/Teknik/Dokumentasi Pertamina OTO/releases/windows-local/20261007-141314-96a5fba4`.
Manifest SHA256: `A982FA6D7BA69455D9318D9ABDD769F18E187FB27BA183F7D607553080A5B474`.
All **392** payload lengths/hashes and **103** tested binary hashes match. Desktop DLL SHA256: `3D500ADF4A7086C17CD2744C70EC4A730159A4A0ACFCE0F04A46D4E32BEAD625`. latest.json, manifest, product-info, build-info, binary product version and bundled notes agree. Use the existing **Periksa update** flow to choose this local update; installed activation requires the user's separate action.

## Scope and ownership

One Agent 0; no subagents or waves. Required checkout `worktrees/core-functional-first`, branch `task/core-functional-first`, baseline HEAD `84cbb0f538020003bcfb6583e94c2fcf6524f87c`. Pre-existing dirty/untracked work is preserved. No commit, protected merge, public publishing, dependency admission/download, system default-printer change, or physical print job was performed.

Implemented reader **Cetak** beside Edit, Berkas/Ctrl+P dispatch to the active PDF, shared saved-PDF result actions and explicit per-file print for multiple PDFs. JPG results have no Print action. Editor printing uses the latest validated export; dirty/no-export state asks for export and preserves draft/undo. Result preview remains local and uses the actual saved file. Existing Back/close/details and draft-return paths are retained. Additional parent-chat UI recommendations about duplicate folder actions remain recommendations, outside this implementation.

Revisi02 is implemented with native WinUI controls: driver-owned destination/paper/orientation, page selection, copies, fit/actual size, capability-based color/duplex, independent printable-area preview, scrolling settings and pinned footer. Compact preview is expandable. Printing uses existing Windows PDF rasterization plus private DEVMODE/GDI/spooler in an isolated child, with progress/cancel/abort and honest queue acceptance. No external viewer or mockup image is used as the print pipeline or UI proof.

20 owned source/test/metadata/document paths are listed in [changed-paths](../../artifacts/print-feature-20261007/changed-paths.txt). New Print*.cs files contain the feature; existing reader/menu/result/program hooks remain narrow. Reader render/search/zoom/scroll engines, worker transforms, fonts, global tool screens, trust/update/installer logic remain outside the change. Test-only release-note category assertions and distinct wrong-version fixtures were updated for the approved 0.2.0 identity; assertions were not removed. [Scope gate](../../artifacts/print-feature-20261007/scope-gate.json) reports zero unexpected baseline desktop-source changes. Scoped baseline diffs, original copies and source hashes are retained.

## Verification and evidence

Evidence root: [print-feature-20261007](../../artifacts/print-feature-20261007/). [Delivery verification](../../artifacts/print-feature-20261007/delivery-verification.json), [native commands](../../artifacts/print-feature-20261007/final-native-commands.json), [publication log](../../artifacts/print-feature-20261007/final-delivery-certified.txt), [visual review](../../artifacts/print-feature-20261007/visual-review.json).

Final native reports are in `final-native-63f3e2d9` under the evidence root. All 11 native commands exit 0. Ten accepted assertion reports total **1,629 PASS / 0 FAIL**; the application flow self-test also exits 0.

| Native report | PASS | FAIL |
|---|---:|---:|
| Print | 75 | 0 |
| Reader UI/search/continuous pages | 479 | 0 |
| Zoom | 63 | 0 |
| Zoom smoothness | 53 | 0 |
| Read previews/results | 72 | 0 |
| Responsive layout | 462 | 0 |
| Update completion | 165 | 0 |
| About | 61 | 0 |
| Release notes | 50 | 0 |
| Live editor, correct fixture | 149 | 0 |

Print evidence includes actual Microsoft Print to PDF roundtrips, two software copies, mixed A4/A3/A2 and rotated input, portrait/landscape × fit/actual, actual rendered content, cancellation before/during submission, worker cleanup, invalid ranges, missing/protected PDF, no-printer state, source locking/hash, reader identity/page/zoom and editor draft/undo/latest export. Only virtual spooler jobs were sent. Producer routing is mapped in [result-route-audit](../../artifacts/print-feature-20261007/result-route-audit.md); not every producer or physical driver was separately printed.

Actual native captures: [dialog desktop](../../artifacts/print-feature-20261007/final-native-63f3e2d9/print-audit/dialog-1400.png), [compact/enlarged text](../../artifacts/print-feature-20261007/final-native-63f3e2d9/print-audit/dialog-480.png), [result](../../artifacts/print-feature-20261007/final-native-63f3e2d9/print-audit/result-single.png). Captures at 1400/760/620/480 widths and advanced/no-printer states are retained. Final inspected images and hashes are listed in visual-review.json; references are in `design/`.

Standard gates: ReleaseNotesChecks 44; ProductVersionChecks 25 plus legacy updater 13; inert package fixtures 8; DistributionChecks 63 plus local updater 13; functional PDF worker 41; editor worker 102; search worker 67; Word worker 27; native Word 39 and quality 39; PDF-opener fixture exits 0. Restart fixture **17 PASS**, with identical launcher-bundle hashes verified against the final build.

Full Debug Rebuild with normal analyzers is recorded in `final-build-publication-native-3.txt`, original output `artifacts/update-build-6de0ed17799d4d3780c3289856f5ebad`. Native tests ran on an isolated copy to keep build output clean. A wrong editor fixture was corrected without code changes; the same binary then passed. The official `eng/Build-LocalUpdate.ps1` resumed using that exact certified source/binary set, performed its mandatory worker/Word/quality/opener gates and published from `artifacts/update-build-c091101cf02b4b388221da97d22f9a9a`. Source and binary guards, reuse certificate and original-build identity are retained; successful native tests were not unnecessarily repeated after the fixture correction.

Earlier compile/analyzer/layout/identity-timing/driver/fixture failures remain documented in `failures-retained.txt` and their original logs. The wrong-fixture editor report remains immutable with its hash and reason in `superseded-audits.json`; it is explicitly superseded by the successful correct-fixture run, not counted as an accepted pass. Initial sandbox printing/restart failures passed with approved native Windows access. No failed candidate was published.

## Authorization, preserved baseline and limits

Human approved product 0.2.0 in the parent chat. Initial automatic review rejected local publication; no side effects occurred. Human subsequently explicitly approved publishing 0.2.0 to releases/windows-local/latest.json after tests: parent turn `01a1151b-67f3-7c40-84a9-34b9b70ea7c2`, “saya izinkan sekarang”. Accepted execution and [authorization evidence](../../artifacts/print-feature-20261007/local-publication-authorization.json) are retained. No publication blocker remains for this local delivery.

Baseline package `20261007-112106-cd7fafab` manifest hash `EB93D1827A72AC688C36D35160860361D92C68F135B7FA1C8417FFE95ED4DCD8` is unchanged and previous-feed.json still identifies it. Installed current.json remains SHA256 `B8B2EE6ACDC6920981B442E8A717CA24B0DBB9BE6E22A628D13B6BF7E220D975`; installed version is not inferred from the feed. Shortcut/launcher installation and installer 1.0.1 were not activated or changed by this task.

PDF input <=512 MB; 1–10,000 logical pages, 1–99 copies, <=10,000 selected pages including copies. Print raster requests are capped at 16M pixels/max edge4096 and preview requests at 2M/max edge1600. This is raster printing, not a vector or total-process-memory/DPI fidelity guarantee. The encryption guard conservatively rejects protected/encrypted markers and can also reject an ordinary PDF containing that marker; it does not bypass permissions.

Physical printers, real duplex/color/preferences/virtual Save-As interaction, monitor DPI/multiple monitors, hardware keyboard/gestures and screen-reader UAT are not universally verified. Enlarged-text capture is not physical DPI evidence. Forced OS termination recovery is not claimed. Submission has a ten-minute bound and cancellation attempts abort/own-job deletion; already received pages can still print. Queue acceptance does not prove physical output. Network printers depend on their connection; local document processing uses no server upload.

Ownership is released for the 20 listed paths and build/local feed. No active child build or agent handoff remains. Further UI recommendations and user installation/UAT can proceed separately; this task does not start another phase or broaden feature scope.
