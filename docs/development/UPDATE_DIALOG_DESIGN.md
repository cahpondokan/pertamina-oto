# Approved update dialog — 2 October 2026

Agent 0 only. User approved the two-state mockup in the current chat and requested implementation. This is a scoped local development change, not phase execution, signing, launcher maintenance, website publication, or Production approval.

Baseline: `task/core-functional-first`, HEAD `84cbb0f538020003bcfb6583e94c2fcf6524f87c`, existing dirty work preserved. Previous local delivery: `20261002-133640-8546a738`. No new dependency or model, no source PDF overwrite, no automatic restart of a user instance.

## Task card / acceptance

- Scope: native update completion + unsaved-work dialog, named work review, real saved-state guards, targeted tests and local update package.
- Allowed paths: UpdateCompletion/RestartWork and their audits, scoped draft/quality snapshots and successful-save checkpoints in NavigationHistory, MainWindow, OrganizerWorkspace/Execution, SplitWorkspace, ExtractWorkspace, RemoveWorkspace, DocumentLifecycle; four matching outline SVGs; relevant verification/delivery scripts and this documentation.
- Do not touch: restart handshake/launcher security, worker PDF processing, installed application/pointer, unrelated dirty changes, public portal, version policy and phase gates.
- Acceptance: white/navy/blue approved hierarchy; green completion and amber unsaved icons; left restart/right blue continue; separate warning with filenames, tool names, status and actionable review; safe X/dismiss/default focus. Consistent local outline icon system, not emoji or Adobe branding.
- Busy operations and real changes must still block restart. Opening a PDF, searching, or retaining undo/redo alone must not. Save failure/cancel must never mark a draft saved. Undo after save must be evaluated against saved content, not stack count.
- Verification: build/analyzers, native widths 1440/820/620 and visual render inspection, long filenames/multiple work items, actual editor/tool/organizer save and failure paths, protocol/isolated native restart, existing PDF/editor/search/distribution/quality regressions. Stop delivery on test failures.

## Implementation

`Versi baru siap digunakan` uses `Lanjut bekerja` as the blue safe action and `Mulai ulang sekarang` as the secondary outlined action. Product/build identity remains accessible in collapsed **Detail versi** rather than dominating the dialog. The app does not claim the running process already changed versions.

`Simpan perubahan dahulu` identifies each pending work item without displaying a full path. **Nanti saja** / X leave work untouched; **Tinjau & simpan** navigates to the chosen work item, not an automatic export or discard. Existing save buttons and validation remain authoritative. More than one item is selectable; long names are ellipsized with full filename tooltip and automation label; a narrow row stacks its status badge.

Restart work uses editor `Dirty` plus active gestures, explicit tool output/options and composition snapshots, and organizer content checkpoints. Home selection, viewer/search state, undo stack counts and merely opened documents no longer masquerade as unsaved PDF content. Successful exports checkpoint the exact tool state; failed exports do not. Tool/organizer changes after save remain guarded. Checkpoints and passwords remain memory-only; closing the actual tool clears its checkpoint. No autosave or session restoration is introduced.

The existing verified launcher handshake and last-moment busy/change checks remain. Missing/unmaintained launchers still fail safely and keep the app open; updating the installed stable launcher is a separate maintenance action, not performed by this request. Closing normally and opening the stable desktop shortcut remains supported.

## Verification

- `eng/Verify-UpdateRestart.ps1` exit 0: `artifacts/rv-4fb89f6c/verification.txt`. Full Debug rebuild with existing analyzers passed.
- Native update dialog/work-state audit: **163 PASS**, including widths 1440/820/620, safe default/focus restoration, continue/X/native dismissal, preservation of inline text and selection, actual review routing, failed overflow save, actual editor save, asynchronous undo/redo, reader open without edits, actual watermark export/failure/navigation, actual organizer export and edits after save, 14 standard tool state cases, multi-document selection and long filename rendering. `app/restart-audit/audit.txt` under the verification directory. Final screenshots inspected visually, including `completion-1440.png`, `unsaved-1440.png`, `unsaved-620.png`, `unsaved-multiple-long-name.png`.
- Actual editor regression: **139 PASS**, exit 0, `artifacts/rv-4fb89f6c/app/live-editor-audit-53c0142982fd46bb99ab4eb219967cb0/audit.txt`. The old suite's final limitations line predates the separate standard/embedded-font repair; it is not a new font restriction introduced here.
- Shared read previews **72 PASS**, About **59 PASS**, output quality **39 PASS**; isolated PDF association helper exit 0; worker operations **41**, worker editor **102**, search **25** passed.
- Distribution **62** + updater **13** passed. Restart protocol **15 PASS**, `artifacts/update-dialog-protocol-20261002/test-results.txt` (missing/tampered launcher, cancellation, startup failure, exact old process identity and update lock).
- Native two-instance restart **3 PASS**, `artifacts/rv-4fb89f6c/nr-fixture-7de95882/test-results.txt`: prepared new WinUI instance, orderly old exit, new commit. Synthetic installation only, no user process closed.
- Tested desktop DLL SHA-256: `8FAB73046DF5A2EC8AAEB8B88CD74109053F8C7B0339D5E1181EA78E5F124354`. `git diff --check` exit 0, existing CRLF notices only.
- No dependency, credential, network endpoint, security policy or PDF processing engine changed. Native audit arguments skip online startup notifications; network-denial testing was not performed. Fixture state and output are isolated; memory-only checkpoints are not diagnostic logs.

Early failed runs caught a popup-root omission in test traversal, replacement characters unavailable in the synthetic embedded subset, and a fixed-delay test issuing redo before undo had completed. Final tests use admitted existing characters for the positive case, explicit overflowing existing glyphs for the rejection case, and bounded transaction completion checks. No production font/overflow/history guard was weakened.

Read-only installed-launcher check on 2 October found no `restart-capability.json`; automatic restart on that older launcher remains unavailable. It is intentionally not replaced as part of this UI change. Failed restart keeps the old instance open and explains reopening the stable shortcut.

## Local delivery

`eng/Build-LocalUpdate.ps1` completed with exit 0. Log: `artifacts/update-dialog-delivery.log`. Published immutable local build **20261002-140611-e6ffd3b0**, product **0.1.0 · Pengembangan**, to the root workspace's `releases/windows-local`; `latest.json` now references it. There are **384 payload files**. Package checks passed: manifest SHA-256 `45CC8548AC556B4B2D4FB9CA079DCE38750DD4DA7FC55DD849AB26E88124577C`, every payload length/hash, containment, no PDF/restart-audit/live-editor-audit fixtures, and exact packaged desktop DLL match to the tested SHA-256 above. The additional local delivery pipeline passed its version/distribution/worker/Word/quality/opener checks.

Use the existing application's **Periksa update → Pasang update**, finish saving work, then close normally and reopen the same stable desktop shortcut. No installed-user activation, stable launcher replacement, shortcut change, user-process termination or website publication was performed. The older installed launcher limitation above still applies.

Physical mouse/keyboard/touch and screen-reader UAT remain manual; native automation/render evidence is not human acceptance or clean-machine certification.
