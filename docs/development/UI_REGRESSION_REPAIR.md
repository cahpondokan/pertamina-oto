# UI/UX regression repair — 4 October 2026

Status: **COMPLETE_LOCAL_UI_REGRESSION_REPAIR_SCOPE**. Product **0.1.3 · Pengembangan**, immutable local build **20261004-211242-4e00f9cf**. Coding, scoped verification and local package delivery complete. No installed activation, online publication or phase approval.

## Report and diagnosis

The user's screenshots are confirmed defect reports, not speculative feedback. Organize painted source/output fields over page thumbnails; Extract, Remove, Merge and Compress concealed the source workspace beneath the settings card. The same shared layout serves fifteen active tools. The approved visual design is retained.

`MainWindow.LayoutCards` cached only width bands, sidebar state and child counts. Opening a different tool replaces Grid children without necessarily changing those values. The early return left new children at row 0 / column 0. A similar cache issue affected rebuilt home cards and quick-tool cards. The fix compares child identities as well as width bands, explicitly assigns row/column/spans, and stacks the tool panels when their content area is under 780 DIP.

The old responsiveness audit checked viewport bounds and button-row overlap, not sibling workspace occlusion. Both source and settings can be within the window while completely covering each other. That audit was insufficient evidence for visual acceptance. The new audit tests repeated forward/reverse navigation without invalidating the production cache and asserts separation of the actual sibling panels.

Additional visually observed repairs:

- An editable zoom ComboBox could show a blank native text presenter despite a nonempty `Text` property. Preserve matching preset selection and synchronize after the native presenter loads; test the actual inner TextBox value.
- Embedded read-only previews had an empty document-session picker that their `previewOnly` model intentionally never populated. Only the dedicated reader now attaches that picker.
- A result view could remain hidden behind the History container. Standard-host results now restore the standard workspace and its result breadcrumb; editor-host results retain their separate host.
- Split's range form was squeezed into a 160 DIP inner viewport on short windows, hiding fields beneath the mode controls. Stacked layouts now use the outer page scroll; wider layouts retain a larger bounded form. Selected tab text inherits the native contrasting foreground.
- The reader referenced `arrow-right.svg` and `external-link.svg`, but neither file existed in the asset directory. Both are now included using the existing icon style and attribution. The native audit requires each file and waits for successful SVG decoding.
- Empty-reader refresh keeps the document toolbar hidden; the landing page already lists the sessions. Its default-app card changes rows only when crossing the responsive breakpoint. Default-status lookup runs outside XAML layout callbacks and stale responses are ignored; no Windows association is changed.

The repeated native crash required separate investigation. A focused navigation run could pass while the full sequence still failed. Microsoft symbols for the exact WinUI DLL resolve the saved native stack to `CImageSource::FireImageFailed`, `CErrorEventArgs::UpdateErrorMessage` and `CXcpDispatcher::OnReentrancyProtectedWindowMessage`. The missing-icon requests are a concrete defect corresponding to this failure path. Moving the status lookup alone did not eliminate the failure and is not presented as the crash fix. After adding the missing assets, the complete matrix exited successfully with 1,485 passing checks and no failures. Broader native regression gates remain separate.

## Scope and ownership

This is a scoped repair, not a phase execution or a redesign. The search ExtGState handoff released source/build ownership before edits. Baseline branch `task/core-functional-first`, HEAD `84cbb0f538020003bcfb6583e94c2fcf6524f87c`; unrelated dirty work preserved.

Production paths: `MainWindow.cs`, `ZoomController.cs`, `ReaderWorkspace.cs`, `ReaderSession.cs`, `ReaderLanding.cs`, `SplitWorkspace.cs`, `UnifiedResult.cs`, and the two missing SVG assets. Debug hook: `Program.cs`; new native harness: `UiRegressionAudit.cs`; corrected stale dialog assertion: `ResponsiveAudit.cs`. Version metadata and offline release notes updated together. No PDF-engine/search-geometry/updater-security/website/OS-association changes.

Evidence root: `artifacts/ui-regression-repair/run-20261004/`.

- `baseline/`: before-source snapshots, hashes and previous local feed metadata.
- `task-card.md`: acceptance and allowed ownership.
- `task-source.patch`: task-only delta against the captured dirty baseline.
- `source-final-hashes.json`: source/metadata freeze used by the delivery gate.
- `before/ui-regression-audit/`: the initial red test, 35 failed checks, including reproduced overlapping tool panels and home cards; Organize screenshot matches the reported failure.
- `after/ui-regression-audit/`: first layout-only candidate. Four false empty-home checks identified a fixture error: normal navigation had copied tool inputs back into Home. The final harness explicitly initializes an empty home and asserts that precondition; the sibling-overlap checks were not removed.
- `attempt-01/ui-regression-audit/`: intermediate candidate completed the loaded-tool matrix but ended during the later screen tour with WinUI native `0xc000027b` (Event 1000, 19:28:58 local). It is not a passing audit. This code alone does not establish a crash cause.
- `focused-navigation-audit.txt`: intermediate focused screen tour including 40 repeated Reader/History transitions and visible result checks; exit 0, but not sufficient because the later full run failed again.
- `full-attempt-02-incomplete.txt`, `navigation-after-async-2.txt`: additional retained failed runs. They are not delivery evidence.
- `native-stowed-trace.txt`, `native-symbols.txt`: crash addresses and exact-DLL Microsoft symbol resolution. Downloaded symbols are diagnostic data under ignored artifacts, not an application dependency or packaged asset.
- `ready/ui-regression-audit/`: final matrix report and rendered screenshots.
- `dialogs-stale-seven-actions.txt`, `native-results-before-dialog-test-fix.json`: the first twelve-case run stopped at three dialog assertions. `ResponsiveAudit` still expected seven About actions, whereas the existing release-history addition introduced the eighth action. The independent About audit already expected eight and passed 61 checks. The stale assertion now requires each of the eight exact accessible names once, not merely a relaxed count. This is a test correction, not a new production-dialog change. The candidate is rebuilt and the native gates rerun; the old failures remain recorded.
- `release-notes-reused-fixture.txt`: the rebuilt candidate's second test run correctly found the synthetic first-view ledger already acknowledged by the previous successful run. The first-view scenario requires a fresh ledger. `Test-Native.ps1` now archives only the candidate's resolved `restart-audit/release-notes-viewed.json` before this scenario. No user ledger is removed or changed; no application source change was needed. The last four gates are rerun on the same frozen candidate.

## Verification matrix

New native matrix: 1920, 1366, 960 and 640 DIP content widths; repeated empty/populated home; all fifteen active shared workspaces loaded with synthetic PDF/PNG/DOCX input in forward and reverse order (120 transitions); catalog return and sidebar collapse/restore; native percent-presenter checks; source/settings separation; horizontal action bounds; top/bottom scrolling; close action; 33 tool-entry pages at wide/narrow widths; main routes and result after History. Dedicated reader/editor/history/dialog regressions are separate gates, not inferred from empty screenshots.

Commands use the candidate host EXE with `--audit-ui-regression`, `--audit-ui-regression --screens-only`, and the twelve-case `Test-Native.ps1` under the evidence root. Test documents are synthetic and source SHA256 is checked. No user document is processed or modified.

The complete new matrix passed: **1,485 checks, zero failures, all 120 loaded-workspace transitions, exit 0**, including the narrow reader stage that previously crashed. `ui-matrix-final.txt` preserves this run. Both added SVG files passed actual native decoding. A source-reference scan found all 26 statically named History icons present. Native screenshot review covers the fifteen tools, empty/populated home, main pages, results and narrow-window scrolling; repeated reverse routes keep their geometry checks without saving duplicate PNGs.

All twelve final native regression groups passed on the frozen candidate: **1,648 checks, zero failures**, each process exit 0. `native-results.json` records exact commands, report paths and start/completion times.

| Native group | Passing checks |
| --- | ---: |
| Zoom / smoothness | 63 / 53 |
| Responsive layout / reader | 462 / 479 |
| Editor / embedded previews | 149 / 72 |
| History / startup window | 52 / 15 |
| Release notes / About | 50 / 61 |
| Restart and unsaved-work guards / dialogs | 165 / 27 |

`manual-ui-review.md` records subsequent Windows input tests in a uniquely named synthetic instance: plus 100→115%, custom 125% + Enter, catalog and Merge reopen at unchanged size, correct navigation selection, reader landing and opening a three-page session PDF. These are agent-driven input checks, not human or physical-touch UAT. The synthetic instance was stopped after review; the installed app was not terminated.

Standard delivery gates also passed: ReleaseNotesChecks 44; ProductVersionChecks 25 plus legacy updater 13; ProductVersionPackage 8 (inert fixture only); DistributionChecks 63 plus local updater 13; PDF worker 41; editor worker 102; search worker 67; Word worker 27; native Word 39; native quality 39. The PDF opener isolated self-test exited 0. Repeated legacy checks are not counted as unique additional coverage. Exact output is retained in `delivery.log`; Word and quality report directories are referenced by the staged output's audit-path files.

## Delivery and limitations

`Build-VerifiedLocalUpdate.ps1` requires the preserved `ui-matrix-final.txt` with all 120 loaded-workspace transitions and the twelve native gates. The final matrix file is not replaced by shorter manual or diagnostic runs.

The first standard delivery attempt performed a full Rebuild but was stopped by `Verified-MSBuild.ps1`: only `Pertamina.Oto.Desktop.App.dll` differed from the tested candidate. Portable PDB document hashes localize the differing compilation inputs to five files under `WinRT.SourceGenerator/Generator.WinRTAotSourceGenerator`; all nongenerated input documents match. The regenerated output remains intact at `artifacts/update-build-c0bdf7cd7f004ee5acf85b08b503075c/`. The two builds are **not** claimed to be bit-identical or accepted as interchangeable.

The delivery uses the exact tested artifact instead. `Stage-VerifiedCandidate.ps1` verifies unchanged source hashes and nongenerated PDB inputs, requires the regenerated DLL to be the only differing application binary, creates a new clean staging directory from the rebuild, and promotes the tested DLL/PDB pair into that copy. Every staged EXE/DLL/PRI must then match the native-tested candidate exactly. This staging hook is passed to the standard `eng/Build-LocalUpdate.ps1`; its remaining worker, editor, search, Word, quality and opener gates still run before local publication. The original strict-gate failure, regenerated binary and PDB differences are retained in `delivery-rebuild-gate-stopped.log` and `rebuild-generated-differences.json`.

`Verify-Package.ps1` checks all published manifest hashes/lengths, tested binary hashes, product version, notes and installed pointer afterward. `prepublication-binary-gate.json`, `delivery.log` and `package-integrity.json` identify the final artifact. Failed gates block delivery.

Final delivery verified:

- Local feed: workspace-root `releases/windows-local/latest.json`.
- Product/channel: **0.1.3 · Pengembangan**.
- Build: **20261004-211242-4e00f9cf**.
- Manifest: **392 files**, every length and SHA256 matched; **103 EXE/DLL/PRI files** equal the native-tested candidate.
- Manifest SHA256: `10127887B71471973F8CE5970E53CB0649F1778999DE2F4C6634E17C47CF9AAE`.
- Desktop DLL SHA256: `5E31376C121BABD36BC98D2CC0315C49FFCCD4D810EC34AE2FDEFA964A84D35B`.
- Notes SHA256: `DE4DF857591842888971CFC408F9781777591409C734EB33D8F0C7C5A3565A88`.
- Previous local feed retained: **0.1.2**, build `20261004-181107-307e2601`.
- Installed `current.json` hash unchanged; no activation or forced user-app closure. The synthetic audit instance alone was terminated after its review.
- Online website, online catalog, installer and Windows settings unchanged.

Use the installed application's **Pengaturan → Periksa update** in the local application-update card, then review **0.1.3** and choose **Pasang update**. This is not the separate online-check button. Save work before restarting. This handoff releases this repair's source/build/package ownership; it does not authorize another agent to activate or publish it.

Fixed-DIP native rendering and simulated native zoom do not prove physical Windows DPI/text-scale changes, moving between monitors, hardware pinch, keyboard/screen-reader UAT or all real PDFs. A prior failed native run is retained, not relabeled as success. The 17 inactive tool entries are presentation checks only; they are not newly implemented capabilities. No Production/universal-device acceptance is claimed. The installed app and online site must not be described as updated by this work.
