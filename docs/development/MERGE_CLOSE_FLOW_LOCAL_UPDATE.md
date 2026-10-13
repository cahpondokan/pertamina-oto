# Merge close, save and return flow

User-approved four-chat implementation for FEAT-PDF-005; scoped local development, no phase completion or Production approval.

## Four actual tasks and integration

1. `01a13081-9146-7622-b28a-a3fe9fc4b6a1`: close button at top right of Dokumen card, soft red, native busy/focus states. MainWindow Section/footer hunks and MergeCloseButton / audit helpers.
2. `01a13081-a4df-7511-9a76-9ebf5db2f0de`: MergeCloseConfirmation / audit helpers, option-4 modal with canonical Biblo028, save/discard cards and safe return action. Small viewport stacks options; three actions only.
3. `01a13081-bc35-7831-8b16-39764f951aec`: MergeWorkspace processing hunk and MergeCloseProcessingAudit; close-save message, actual destination, indeterminate progress, existing cancel.
4. `01a13081-cfe2-7aa2-be98-d5f98064a649`: MergeClosedSavedNotice / audit helpers, view-bound green banner above original empty page, immutable saved path for preview/folder actions.

All four final handoffs were received before integration/build. Parent integrated DocumentLifecycle, ExecuteAsync target forwarding, success-view suppression for close-save only, audit dispatcher, cross-tool audit action mapping and end-to-end synthetic audit. No additional chats or subagents. Namespace compilation fix and small notice/check/layout integration corrections were made by parent. Baseline branch task/core-functional-first, HEAD 84cbb0f538020003bcfb6583e94c2fcf6524f87c; dirty legitimate baseline retained, no reset/clean/dependency change.

## Behavior

Loaded Merge has one close action in its document-card header. Pending work opens the new modal. Return/Escape/default Enter keep the session; selecting save uses existing order/name/folder/password/output-collision policy. Only after validated worker success, saved checkpoint and actual new result exist does close clear this tool's session and return to empty Merge with success banner. Failure/cancel retain inputs/options and release mode/reentrancy guards; discard shows no success banner and leaves sources/prior saved results untouched. Ordinary Merge success and other tool/editor dialogs remain existing behavior. Buka hasil uses internal read-preview; Buka folder uses existing safe folder-reveal implementation with immutable saved path, guarded missing-file handling. Neither installs, uploads nor prints.

## Verification

Self-contained admitted .NET 10.0.12 candidate build passed. Final native programmatic audits on exact candidate: flow122, processing36, notice31, ordinary Merge30, empty50, cross-tool/editor document-close53 PASS. Synthetic actual close-save result reopened with six pages in document order; hashes of sources and prior saved result unchanged across save/discard/failure/cancel. Screenshot layout checked at normal and minimum 660×810 window; dialog stacks vertically, processing and success notice fit. Source audit input uses synthetic PDFs only; clipboard not changed. Physical hover/keyboard, screen reader, live high contrast toggle, alternate DPI/monitors and network-disabled UAT are not claimed. Source has high-contrast fallback but it is not a physical acceptance sign-off.

Earlier failures retained: missing AccessibilityView namespace corrected; audit PDF sizing changed from incorrect points assumption to comparing native source/result sizes; failed-fixture draft explicitly discarded before next independent scenario. Final native-r4 passed. No tests/gates weakened.

Standard Build-LocalUpdate.ps1 -PreparedBuild gates passed: release-note/product/distribution/updater, admitted five-component runtime payload, worker/editor/search, Word synthetic conversion + UI, quality, PDF opener. Detailed command/output in local-update.log. UI/test fixtures were archived outside publish payload; no artifact/release or user data deleted.

## Local delivery

Product `0.3.0 / Pengembangan`; build `20261013-002005-94b886e8`. Parent-workspace package `releases/windows-local/20261013-002005-94b886e8`, current local latest.json points to that exact build. Manifest SHA256 `81A48A9DE6C6850A56A1FABB3E41E5398C963F6CD62B9F85942281B44B8C20B0`. All 1,334 file lengths/SHA256 hashes match; desktop DLL and canonical Biblo match audited candidate. The actual installed updater's Inspect method discovers this build using the unchanged configured feed.

Installed selection remains `20261012-095354-99604f30`; stable launcher/shortcut unchanged. User updates via Pengaturan → Periksa update → Pasang update, then existing safe restart flow after saving work. No installation, product bump or online publication occurred.

Evidence root `C:/Users/cahpo/.codex/visualizations/2026/10/12/01a12df3-d5a1-7a90-aaed-790dece50553/close-integrated/`: build-r4.log, local-update.log, package-verification.txt, Verify-LocalPackage.ps1, audited-binary.txt, integration-notes.txt, source hashes and before/diffs. Native evidence: native-r4/audit.txt, processing/audit.txt, notice/audit.txt, merge-regression/audit.txt, empty-regression/audit.txt and archived document-close-audit/audit.txt. Native screenshots include native-r4/loaded-1400.png, merge-close-None-1400.png, merge-close-None-660.png, closed-saved-success.png, processing/close-save-660.png and notice/merge-closed-saved-660.png.
