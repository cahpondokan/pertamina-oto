# Update completion and safe restart

**2 October 2026 launcher delivery repair:** see [LAUNCHER_DELIVERY_REPAIR.md](LAUNCHER_DELIVERY_REPAIR.md). The previously missing installed capability is now repaired on the originating user's laptop, with exact pointer/shortcut preservation and retained launcher backup. New app/installer packages include verified launcher migration; the historical separate-maintenance-only limitation below is superseded. Same-user handshake and work-protection requirements are unchanged.

**2 October 2026 — approved dialog redesign:** the visual layout and conservative session-presence guard described below are superseded by [UPDATE_DIALOG_DESIGN.md](UPDATE_DIALOG_DESIGN.md). The handshake/security contract remains unchanged. The new guard distinguishes actual unsaved work from open readers, saved editors and undo history. The remainder of this file is the earlier implementation/delivery evidence, not a claim of current installed launcher capability.

Task card: approved central success dialog only. Baseline commit 84cbb0f538020003bcfb6583e94c2fcf6524f87c, task/core-functional-first; preserve dirty predecessor work. About/preview handoffs completed before writes/builds. No subagents, dependency admission, phase gate or Production change.

Allowed: UpdateSettings, dedicated completion/restart UI and audit, shared restart protocol, targeted Program startup hook, launcher protocol/tests/maintenance delivery. Do not touch branding, unrelated settings, worker feature behavior, user documents or running user instances.

Acceptance: real running/staged versions, accessible native dialog, non-destructive default and close/Esc, draft/undo/pending edit/busy protection; no force kill, duplicate launch, or app close without new startup acknowledgement. Launcher failure leaves old session available. Tests use isolated synthetic installations/processes only.

Verification: build with existing analyzers, restart protocol negative/integration tests, native desktop/compact render and preservation checks, existing updater/editor/delivery regressions, local Build-LocalUpdate publication. Stop on ownership overlap, failed verification or unsafe mutation. Handoff must include paths, commands/results, evidence, package version and launcher delivery limitation.

The normal updater still verifies/copies packages and atomically switches current.json. Running version is read only from this executable's build-info.json. Neither pointer activation nor launcher process creation is treated as startup success.

Restart is conservative: documents, session drafts, pending textbox/inline input and undo/redo must be saved/explicitly closed first. Return/cancel never discards them. Session history is not autosaved. A separate launcher maintenance package is required; ordinary app updates cannot replace the stable launcher. Missing/old/mismatched launcher capability blocks restart safely.

## Implementation and boundaries

- `UpdateCompletion.cs`: native center dialog, success glyph, immutable running label vs actual installed pointer/version returned by Install, New badge, honest protection copy. Blue restart at right, continue at left, no destructive Enter default, initial focus continue, X/native dismissal, opener/inline selection restoration. Existing About `SafeRunningVersion` and editor `PauseForReadPreview` are reused.
- Restart guards retain all tool snapshots (including output/options with no selected input), home/organizer undo and redo, split/extract/merge state, editor documents/typing/history, reader documents/bookmarks, passwords and visible pending textbox content. Busy import/clipboard/worker/editor/reader/search/loading blocks restart. There is no automatic draft save or automatic discard.
- `RestartProtocol.cs`: SHA-256 capability verification of the existing stable launcher bundle, valid immutable version IDs and safe paths, argument-list process starts, same-user named pipes, exact old PID/start-time identity, startup READY from the actual new executable, old EXIT consent, normal old-process exit, new COMMIT. Update lock keeps pointer stable during handshake; maintenance lock avoids launcher replacement races. A prepared new window has input disabled until commit. Failure/cancellation disconnects cause the new instance to exit itself; no process kill is used.
- LocalAppUpdate verification/staging/current/previous atomic behavior is unchanged. A switched pointer is never reported as proof that a new app is running.
- Conservative limitation: retained session snapshots or undo/redo may still block restart after output has been saved. Some existing tools do not clear every session cache on explicit document close. In that case the user must save work, close normally through the existing session confirmation, and reopen the stable shortcut. This task does not silently erase those caches to make restart possible.
- There is no document-argument or unsaved-session transfer to the new process. Source documents, output and installed version directories are never deleted by restart.

## Launcher delivery

`eng/Build-RestartLauncher.ps1` prepares `artifacts/restart-launcher`, including a capability record hashing exe/dll/deps/runtimeconfig. `eng/Update-RestartLauncher.ps1` is a separate maintenance action: verifies the bundle, refuses an active stable launcher, locks maintenance/update, retains backup, replaces only launcher bundle files, verifies installed hashes and restores retained files on failure. It does not change app versions/current pointer/shortcut/user data. It was tested only on synthetic installations; the user's installed launcher has NOT been changed.

An ordinary app update without this maintenance still displays the dialog; pressing restart fails safely with an explanation and keeps the session open. The approved local feed package must be created with Build-LocalUpdate.ps1 after direct publication authorization. No new release/version is claimed for this task yet.

## Verification, 1 October 2026

Final verification: `eng/Verify-UpdateRestart.ps1`, exit 0; `artifacts/rv-7cab8a65/verification.txt`. Desktop Rebuild Debug with enabled analyzers passed. Worker 41, editor 63, search 25; DistributionChecks 62 plus 13 updater; PDF opener and quality regressions passed. Native update completion 46, shared read preview 72, About 58 passed. Exact command logs are retained in that directory.

Successful native restart: `artifacts/rv-7cab8a65/nr-fixture-aab907e2/test-results.txt`, 3 PASS. This used two actual WinUI instances in an isolated synthetic installation with fixture storage, normal old exit and new startup commit; the user's instance was never restarted. Debug fixture child carries an audit argument so existing online notifications skip network access. This is programmatic native evidence, not physical UAT.

Protocol fixture checks: `dotnet run --project tests/RestartChecks/RestartChecks.csproj --no-restore -- artifacts/update-restart-evidence/protocol-accepted artifacts/restart-launcher`; separate initial final runs at protocol-final2 contain 15 PASS, covering missing/tampered launcher, changed pointer, wrong running build, invalid token, exactly one child, paths with spaces, cancellation, startup failure and concurrent update lock. Local updater's existing 13 tests pass at `artifacts/update-restart-evidence/updater-final/test-results.txt`. Launcher-maintenance fixture SHA-256 preservation evidence: `artifacts/update-restart-evidence/launcher-maintenance.txt`.

Screenshots: `artifacts/rv-7cab8a65/app/restart-audit/completion-1440.png`, `completion-820.png`, `completion-620.png`. These are unmodified native panel renders. Build without release metadata honestly shows Build pengembangan; the staged version in the audit is a clearly documented synthetic fixture. Production dialog receives real version values from immutable running metadata and successful installation/current pointer.

Earlier failed evidence remains: first nested long fixture did not complete and its empty audit parent stayed open; an attempted rebuild correctly refused the locked executable. The exact known audit PID/path was matched and its error dialog/window closed normally, without kill. Subsequent tests used new shorter fixture paths and passed. Initial template timing and fixture output-reuse failures were fixed before final verification. Do not interpret the preserved failed run as the final result.

Physical keyboard Esc/mouse/touchpad and screen-reader UAT remain manual. No Production readiness or phase gate change.

## Delivery checkpoint

Implementation and final verification completed; delivery remains BLOCKED by automatic approval review. Attempt to run Build-LocalUpdate.ps1 was rejected because delegated publication intent was not recognized as trusted direct user authorization. No workaround publishing was attempted. No release was published by this task, and installed versions/pointer/launcher/shortcut remain unchanged. Required direct decision: authorize local development publication and (if desired) separate installed launcher maintenance. Report to originating chat is attempted once after evidence is ready; its result is recorded in the handoff.

Final read-only audit: installed pointer and latest local feed both remain `20261001-105807-9dbe7eb6`; installed `restart-capability.json` is absent. Thus this user's launcher requires the separately prepared/fixture-verified maintenance bundle before in-app restart can be enabled. No running executable was overwritten. Final protocol-accepted 15 PASS, updater-final 13 PASS, quality 39 PASS, and git diff --check exit 0. Final source/binary SHA-256 inventory: `artifacts/update-restart-evidence/final-hashes.txt`.

Files changed by this task: `UpdateSettings.cs`, `Program.cs` (targeted startup/audit hooks), `SettingsWorkspace.cs` (DEBUG fixture preference path only), new `UpdateCompletion.cs`, `UpdateCompletionAudit.cs`, `RestartProtocol.cs`; launcher `Program.cs`/`LocalLauncher.csproj`; new `tests/RestartChecks` project/Program/zero-dependency lock; `eng/Build-RestartLauncher.ps1`, `Update-RestartLauncher.ps1`, `Test-NativeRestart.ps1`, `Verify-UpdateRestart.ps1`; this document. Existing dirty changes from predecessor tasks were preserved; no commit, merge, phase record, dependency or Production mutation.

Origin report attempt: one send_message_to_thread call to 01a07c8d-3c57-7963-8500-daed7ddb07ee was rejected by automatic approval review: no trusted user message explicitly authorized messaging that destination. The report is retained here; no alternate/indirect send was attempted. Required direct decision now includes authorizing that specific report to the originating chat, local-feed publication, and optional separate installed launcher maintenance. Implementation is verified, but end-to-end delivery is not COMPLETE.
