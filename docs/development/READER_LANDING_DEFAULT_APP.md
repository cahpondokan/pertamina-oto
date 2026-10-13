# Reader landing and default PDF app — 4 October 2026

Scoped local implementation authorized by parent chat `01a07c8d-3c57-7963-8500-daed7ddb07ee`. Agent 0 only; no phase execution, central state edits, dependency acquisition, commit, installation or publication. Checkout `worktrees/core-functional-first`, branch `task/core-functional-first`, HEAD `84cbb0f538020003bcfb6583e94c2fcf6524f87c`. Existing dirty/untracked work preserved. Parent and checkout AGENTS, shared contract, state/decision register and reader/session/paste/search/default documentation reviewed. No active overlapping OTO writer appeared in the app inventory.

## Changes

- `src/Hosts/Pertamina.Oto.Desktop.App/ReaderLanding.cs`: native scrollable landing following the approved composition, existing logo/sidebar/SVG library, title, offline badge, subordinate default card, centered primary PDF action, shortcuts/privacy, real local history up to three distinct PDFs, actual thumbnails/page count/size/date, missing-file notice, existing History destination and session choices. No mockup raster or fake documents in product UI.
- `ReaderWorkspace.cs`: landing wiring and visibility; existing picker/drop/global paste remain the routes. Successful open records local reader metadata; release updates the empty state. Loaded reader/search/zoom/scroll/tool behavior remains existing implementation.
- `PdfAssociation.cs`: typed effective association query distinguishes OTO/other/unverifiable, retaining `AssocQueryStringW` and existing per-user registration.
- `PdfDefaultSettings.cs`: landing reuses existing explicit-click registration/Settings action. Safe generic Default Apps fallback when portable/registration unavailable, no success claim from merely opening Settings. Registration and launch failures are reported. Activation refresh queries actual Windows state. Verified default collapses the offer to a compact status.
- `ReaderUiAudit.cs`: additional native landing/default-state/viewport/recent/missing assertions and implementation screenshots. Default-state injection exists only in DEBUG and never writes live Windows selection.

No MainWindow/csproj, worker/search engine, updater, launcher, version policy, signing, website or central coordination changes were needed. Windows default selection remains the user's system UI choice; no UserChoice/elevation/default takeover was performed.

## Evidence and commands

Run folder: `artifacts/reader-landing-default-app/run-20261004/`. Approved reference copied to `approved-reference.png`. `baseline-status.txt` records extensive pre-existing work; early file snapshots in `baseline/` were captured after the first three landing wiring hooks (not a pristine Git baseline). Scoped diff and SHA256 inventories are supplied separately.

Final native audit directory: `build/reader-ui-audit-d7832fd354cb4dfa9f5a969d35bd6138/`. Screenshots include `landing-1366.png`, `landing-1920.png`, `landing-900.png`, `landing-default-offer.png`, `landing-recent.png`, `landing-missing.png`, and the existing reader/search/session regression captures. Screenshots render actual WinUI content; title/menu chrome is outside root capture. Default offer screenshots use the DEBUG status seam; real Windows already reported OTO default on this machine.

Build command: VS2026 `MSBuild.exe src/Hosts/Pertamina.Oto.Desktop.App/Pertamina.Oto.Desktop.App.csproj /t:Build /p:Configuration=Debug /p:DesktopUiOutputPath=<run>/build/ /v:minimal /nologo`. Final build log `build-final.txt`. Desktop DLL SHA256 `54F00CA0423EA1864473B173A0142C779F5FD6610069B00B4C4AEB3E67C2152A`.

Native command: `<run>/build/Pertamina.Oto.Desktop.App.exe --audit-reader-ui`; process awaited to completion. Search command: `<run>/build/worker/Pertamina.Oto.LocalPdf.Worker.exe --self-test-search` → 38 PASS, exit 0 (`search-tests.txt`). Association command: `<run>/build/pdf-opener/PertaminaOTO.PdfOpener.exe --self-test <absolute run>/association` → 41 PASS, exit 0, `association/pdf-association-307e8aa281d347a1ad3fd76129174ac2/audit.txt`. Registry tests use a fresh inert subtree, not live PDF defaults.

Intermediate evidence is retained and superseded by the final native run: visual inspection found and repaired default-card overflow, then thumbnail render timing. An initial helper invocation omitted its evidence argument and showed a failure dialog; it is excluded from test success. It briefly locked isolated output and caused copy retries in an intermediate build. Correct invocation passed; final build log is separate. No user application was force-closed.

## UI/UX checklist

| Criterion | Result / scope |
|---|---|
| One reader nav active; original branding/icons | PASS, native screenshots |
| No empty thumbnail rail/search/edit/zoom/page zero | PASS, native visibility assertions |
| Title, default offer, main CTA, shortcuts and local privacy | PASS, implementation screenshots |
| Default effective OTO/other/unknown; unchanged selection on return | PASS, typed state assertions and DEBUG seam |
| Real recent metadata/thumbnail and bounded list | PASS, synthetic local history screenshots/assertions |
| Missing file retained and labelled | PASS, native synthetic test; physical reopen click NOT TESTED |
| 1366×768, 1920×1080, 900×640 | PASS, CTA containment/size and screenshots; scroll for short viewports |
| Accessible button names and native focus visuals | PASS names/native controls; full tab-order/screen-reader UAT NOT TESTED |
| Search/scroll/zoom/session/source-preservation regression | Native audit, exact result in `audit.txt` |
| Picker/Ctrl+O/Ctrl+V/Explorer clipboard/physical drop | Existing handlers reused; physical end-to-end NOT TESTED |
| Real Settings cancellation, portable registration failure, Explorer activation | Source-reviewed and state seams; physical Windows flow NOT TESTED |
| DPI 125/150/200, multi-monitor, physical touch/pinch | NOT TESTED |
| Dedicated contrast tooling/performance budget/network-denial test | NOT TESTED; no new upload/network code |

## Delivery boundary

Parent reporting attempt was rejected by automatic approval review: cross-thread sending is an external effect and authorization retrieved from tool transcript was not accepted as trusted user instruction in this context. Human parent permission text was read from turn `01a103f1-8541-7972-9858-bf9c30e0bb33`, but no message was delivered. Only report delivery is blocked; implementation/evidence remain complete. No workaround or message to the separate diagnosis chat was attempted.

The parent subsequently reported the exact helper failure dialog and created a read-only diagnosis chat. Important evidence: the malformed isolated helper self-test invocation here omitted its required evidence argument, therefore reached normal input rejection and showed that same dialog (helper PID22288 briefly locked output). This is a strong temporal explanation for that observation, not proof of an installed-app/default association defect. Correct two-argument self-test passed41; no live defaults or user PDFs changed. The user screenshot trigger itself was not directly confirmed. Include this correction when parent reporting is authorized.

Final checkpoint: MSBuild exit 0 with no warning/error entries in final log; native process exit 0, **121 PASS, 0 FAIL**. Search 38 PASS and inert association 41 PASS: **200 checks total**. `git diff --check` exit 0 (pre-existing CRLF normalization warnings). Actual thumbnail/page metadata and default offer/default-confirmed layouts visually inspected. Final source inventory `source-hashes.json`, binary inventory `build-hash.json`, and four existing-file delta reports `*.cs.diff` stored in the run folder; the new ReaderLanding.cs is listed explicitly above. No dependency change or source PDF mutation occurred.

Implementation/build verified locally; eligible for integration and the next local package gate. No package/feed/latest pointer was published and no running/installed version changed. Parent request explicitly excludes publication for this task, overriding the general checkout delivery preference. Final production/UAT and full phase gates are outside this scoped work. Remaining manual checks above are disclosed, not claimed complete. Native WinUI typography/icon coloring retains the existing library, so this follows mockup composition rather than claiming pixel-identical raster replication.
