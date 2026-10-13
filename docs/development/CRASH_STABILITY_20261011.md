# Crash stability audit — 11 October 2026

Status: VERIFIED LOCAL CANDIDATE. Two reproduced crash paths repaired; 20 final native groups pass (1,792 checks, zero failures). Standard delivery and exact ZIP/hash verification pass. Candidate only; installed application and active feed unchanged. This is not complete stability, human UAT or phase/Production approval.

One writer in chat `01a128b8-5669-7342-9711-485132311f39`; no subagents. Checkout `worktrees/core-functional-first`, branch `task/core-functional-first`. Existing dirty work retained; central phase records unchanged. Evidence: `artifacts/crash-stability-20261011-01a128b8/`.

## Baseline

Installed product 0.2.4 / Pengembangan, build `20261011-054806-66eec1fb`; reported DLL SHA256 `E4D11BEFF2A3AA6764166A0DB323EC300AA22702EA104060125C86D69C77BC30`. Existing centered-home design is protected. Baseline tracked patch, status, HEAD and all host top-level source hashes retained under `baseline/`.

Two original dumps read locally, never uploaded, copied into public evidence or changed. Sanitized parser output records exception metadata only:

| Crash | Dump SHA256 | Native exception | Stowed HRESULT |
| --- | --- | --- | --- |
| 08:03:36 Asia/Jakarta, PID 22036 | `c9e050c439c84abf68f99a4d026bfcb070fe994ef5623cbd0d06446f54fd8e03` | `0xc000027b`, Microsoft.UI.Xaml.dll + 0x39cde5 | `0x80004005` |
| 08:58:36 Asia/Jakarta, PID 12156 | `6e1782cf2fdce4f2055ff10112a38f09ee2426af121edfa1767867f12e149bcc` | same | same |

Both STOWED_EXCEPTION records describe 59 8-byte stack words, but none of that pointed memory is present in the minidump memory streams or thread stack descriptors. This limits historical root-cause attribution. Equal offsets/HRESULT do not establish equal causes. Historical 7 October Storage-broker repair remains in source; all statically referenced History SVGs exist.

## Diagnostic work

### Confirmed native failure path

The failing thread's captured stack memory contains addresses in WinUI's accelerator formatter and cascading menu presenter. These are a pointer scan, not a recovered/unwound original stowed stack. Offline resolution uses the already available Microsoft PDB; DbgHelp verifies the exact image, and no debugger/symbol package was downloaded. The WinUI DLL SHA256 `588E414C007CCD55E9D94C3EB8D218144D5A0B2D9F352C632644E16DF2641A78` is identical in the installed-build payload and the diagnostic application.

Native baseline reproduction is concrete: `MenuFlyoutSubItem` automation provider expands Berkas and Edit successfully, then expanding Tampilan terminates PID 24492 with `C000027B`, stowed `80004005`. Live stowed frames resolve to `GetResourceStringIdFromVirtualKey`, `KeyboardAccelerator::ConcatVirtualKey`, `GetStringRepresentationForUIElement`, `MenuFlyoutItem::InitializeKeyboardAcceleratorText`, `MenuFlyoutPresenter::OnItemsSourceChanged`, and `MenuFlyoutSubItem::EnsurePopupAndPresenter`. Evidence: `menu-baseline-run/progress.txt`, `menu-baseline-native-trace.txt`, `menu-baseline-symbols.txt`. No global exception is marked handled.

An explicit menu shortcut label alone was insufficient: candidate 1 crashed through `KeyboardAccelerator::SetToolTip` / `CKeyboardAccelerator::EnterImpl`. This failed run remains preserved in `menu-candidate-native-trace.txt` and `menu-candidate-symbols.txt`.

Candidate 2 moves only the +, − and / command accelerators to the existing root with `KeyboardAcceleratorPlacementMode.Hidden`, while retaining explicit menu labels, handlers, Ctrl/Shift modifiers, numpad and main-keyboard alternatives. All other command accelerators remain unchanged. Berkas/Edit/Tampilan/Bantuan expanded three times each and exited `00000000`: `menu-candidate-2-run/audit.txt`, `menu-candidate-2-native-trace.txt`. This is an intermediate verified repair; final candidate evidence is still required.

Ordinary production behavior changes only in `DesktopMenu.cs` and the event lifecycle guards in `LivePdfEditor.cs`. `Program.cs` changes DEBUG audit dispatch; `CrashStabilityAudit.cs` is new DEBUG-only verification code. `PortableProfile.cs` adds a DEBUG-only process environment switch to a sibling `NativeAuditData` directory, allowing installed-mode UI audits with isolated user data. ReadPreview/Favorites/Reader/LiveEditor audit sources adapt test routing and assertions to the current centered shell and add a late-event regression. Colors, geometry, cards, icons, hover, navigation layout, reader/editor/print design, minimum window size, PDF/search/export engines and dependency/version metadata are not edited.

### Confirmed editor late-event crash

Candidate 5 (`FEFBB3CAA1CE92FF534CA94318BF7D3EAC53213EE684111F45849228708ADB3B`) passed the full 238-check stress group, but the editor group failed a property-text assertion and then exited native `C000027B`. Its isolated safe log records `xaml-unhandled`, `ArgumentOutOfRangeException`, `80131502`, `List.get_Item` → `BuildProperties.Change` → `TextChangedEventHandler`. A new owned baseline instance, PID 8604, reproduced the native termination under the local debugger. Live stowed HRESULTs include `8000000B` and `80004005`; cached Microsoft symbols identify the WinUI TextChanged routed-event dispatcher. Evidence: `attempt-5-safe-errors.log`, `native-editor-baseline-trace.txt`, `native-editor-baseline-symbols.txt` and the baseline editor audit retained in `final-app-5/`. This is a distinct confirmed crash, not attributed merely from the common failfast offset.

The inspector's delayed TextChanged callback retained a valid generation and nonnegative chosen index after `ReleaseFiles` had cleared the object list. Its index access therefore escaped through XAML. Release now retires the inspector generation and selection before clearing the model; the callback also checks the upper index bound. An initial/same object ComboBox selection no longer needlessly recreates the inspector under queued TextChanged events. A native regression deliberately edits an old TextBox after model release and requires no restored object, no crash and unchanged source bytes. The full editor audit also checks stable initial inspector generation and real text/undo/redo updates. No global exception is marked handled.

The fifth run's reader failure at check 469 and Favorites failure after 11 checks used obsolete sidebar/catalog expectations. Reader focus now verifies the unchanged centered shell, preserved preference and document viewport; Favorites verifies the current available-tools grid. The passing 238 stress, 73 preview, 63 zoom, 53 smoothness, 52 history, 32 window, 154 virtual-print, 165 restart, 61 About and 50 notes groups remain intermediate evidence. Candidate 6 failed compilation of the new test's span expression across await (CS4007); separating the awaited hash fixes the test. No failing build/run is accepted or packaged.

### Retained failed and intermediate tests

The first complete self-contained candidate (`22B0EEDB31553025E9FD6D109F01B00966DAF2017CC36D86F04FED992A0674BA`) passed native menus 18 checks and home 30 checks. Its stress harness exited 1 after 146 passing checks at loaded Merge iteration 1. The harness had inserted `second.pdf` directly into `files`, although the retained Merge draft already contained it; the card model has one card per unique input and direct insertion caused `ArgumentOutOfRangeException`. User-facing `AcceptMergeFilesAsync` already rejects duplicates, case-insensitively. The corrected harness calls that actual input-admission method and additionally asserts draft uniqueness. No production behavior is changed to accommodate an invalid private test insertion. Evidence is retained under `attempt-1-native/`, `attempt-1-native-results.json`, `attempt-1-native.log`; it is not accepted final evidence.

The failed sandbox reader opening was separately diagnosed as `UnauthorizedAccessException` from Windows Storage acquisition. Elevated synthetic tests subsequently opened the same PDF normally. This is a test-environment distinction, not a change to document admission, an ACL modification or a security bypass.

The 1480 baseline/final home PNGs were byte-identical for the first candidate. At 800 the comparison shows the existing hover on Atur halaman and a visible native scrollbar; 640 differs in scrollbar state. Those state differences are not declared byte equality. Final visual verification is bound to the final build and untouched UI-source hashes.

Baseline home screenshots were captured from the prior admitted diagnostic build, at 1480/800/640 window sizes, with isolated library. Its audit exited 1 after 18 passing checks because PDF reader attachment was not visible and the following zoom-control lookup failed. This is retained failed evidence, not a crash reproduction or passing final gate. No new error entry appeared in the real safe log.

`CrashStabilityAudit.cs` and a DEBUG-only Program hook add a native sequence with progress persisted before each route. It targets all 33 catalog entries, 16 active tools, repeated loaded PDF navigation, support screens and reader/editor retention using synthetic documents. Initial build failed CA1305 in the new test formatter; corrected with invariant culture. Production changes are the menu repair and the subsequently confirmed editor callback lifecycle repair described above.

Second candidate `55CDE5B4D6D44813472FDA9EAB286F1E4F7389AE5CF9A1F4F2C83565AD80819D` passed menus/home and 224 stress checks but failed two assertions: private generic `pages.Text` was overwritten by Remove's asynchronously loaded specialized selection control; Windows PDF rejected direct opening of the encrypted Protect output with COM HRESULT `80048040`. The fixture has three pages. The final harness settles input loading and sets the actual Remove/Extract text controls, resets `resultPath` before each operation, and independently verifies Protect by unlocking through the admitted worker with the same synthetic password and reopening the decrypted copy. Production Remove/Extract/Protect code is unchanged. Evidence remains in `attempt-2-native/`; this run is not accepted final evidence.

Third candidate `051F0D89001253F883255C54071925EBDCA19E63428A022096CD4EEF4BF85477` passed 234 stress checks, including all 12 ordinary operation outputs, authorized Protect unlock/reopen, organizer output, 120 reader transitions, 40 editor transitions and unchanged source hash. Its Split output assertion failed: the private audit executed immediately while the loaded preview generation was still replacing `splitCount`. The corrected harness waits for input controls, explicitly refreshes the admitted Split source and requires three ready source pages within five seconds before export. It also reopens Split/Organizer outputs and checks page counts. No production Split code changed. Retained `attempt-3-native/` evidence is not final PASS.

Fourth candidate `4F9F58F49921F6FFD8A31935AA22A9EE29F98CBFFC8749BB6A9E3CB22C5155AB` completed all 15 independent native groups. Passing groups include menu/home, zoom 63, smoothness 53, history 52, window 32, virtual print 154, About 61 and notes 50. Six groups had invalid or stale audit setup: transient `IsLoaded` immediately after navigation (stress), portable-mode default-PDF wording (reader), wrong editor fixture without the required `PERTAMINA OTO - EDITOR TEST` run (editor), document preview queried on the current centered tools home (previews), old sidebar-dependent narrow column count (favorites), and the portable mode deliberately hiding the installed updater button (restart). All reports and exit 1 evidence are retained under `attempt-4-*` and `final-app-4/`; none is accepted final evidence. The final run uses the admitted worker's editor fixture, the real document route, bounded layout readiness, independent card bounds/overlap, and a DEBUG-only isolated data profile that preserves installed-mode behavior. This does not add a global exception suppression or change user UI.

## Final native verification

Final candidate 7 is self-contained Debug with normal analyzers enabled and private .NET 10.0.12 payloads. Build exit 0 and all five payload checks PASS: `final-build-7.log`. Tested host DLL SHA256 is `F9300F8DB943124DFA40A8152794901BDD537BD72309CC0E5AE517D447F1A4A0`; MVID `cef6b262-01de-4e03-8038-34d883783c09`. All following runs bind that exact DLL; failing historical candidates are not substituted.

| Native group | PASS | FAIL | Evidence |
| --- | ---: | ---: | --- |
| Cascading menus / OEM shortcuts | 18 | 0 | `native/menus/audit.txt` |
| Editor delayed property callback regression | 12 | 0 | `final-native-results.json` → editor-race |
| Full editor | 150 | 0 | `final-native-results.json` → editor |
| Centered home and route screenshots | 30 | 0 | `native/home/native-audit.txt` |
| Capability/navigation stress | 238 | 0 | `native/stress/audit.txt` |
| Reader / search / 1,000-page document | 479 | 0 | `final-native-results.json` → reader |
| Read previews | 73 | 0 | `final-native-results.json` → previews |
| Zoom | 63 | 0 | `final-native-results.json` → zoom |
| Render smoothness | 53 | 0 | `final-native-results.json` → smoothness |
| History | 52 | 0 | `final-native-results.json` → history |
| Favorites | 14 | 0 | `final-native-results.json` → favorites |
| Window sizes | 32 | 0 | `final-native-results.json` → window |
| Print / virtual PDF target | 154 | 0 | `final-native-results.json` → print |
| Restart and draft guards | 165 | 0 | `final-native-results.json` → restart |
| About / metadata | 61 | 0 | `final-native-results.json` → about |
| Release notes | 50 | 0 | `final-native-results.json` → notes |
| Document-close lifecycle | 53 | 0 | `final-close-results.json` |
| Result actions plus actual Explorer selection | 17 | 0 | `final-results-actions.json`, `explorer-*-inspection.json` |
| **Total (18 groups)** | **1,714** | **0** | All process exits 0 |

Standard delivery additionally ran the actual Word conversion UI and general quality UI against this same prepared binary: 39 PASS / 0 FAIL each, both exits 0. Therefore the final native total is **20 groups, 1,792 PASS, 0 FAIL**. `final-evidence-index.json` maps all 20 reports to their current preserved locations and binds report hashes/DLL identity. `final-delivery-summary.json` records the combined result. The Windows event query covers the 18 explicitly recorded PIDs, not unidentified delivery subprocess PIDs.

Explorer inspection uses the actual displayed Sky screenshots: first `Hasil Ω & [01].pdf`, then `Hasil Ω & [02].pdf`, in the exact owned fixture folder. Screenshots and SHA256 are retained in `explorer-single-evidence.jpg` and `explorer-multiple-second-evidence.jpg`. The optional API `selected_elements` field was absent; its second-stage accessibility details snapshot remained stale for the first file. It is not used as second-selection proof. The actual second screenshot visibly highlights the second row and identifies it in the details pane. Handoff files are written only after observing the requested selection; API return or markers alone are not counted as proof. No computer input was needed.

Stress exercised all 33 catalog routes in four empty cycles, thirteen loaded PDF routes in four cycles, source admission including Merge duplicate rejection, 120 reader transitions and 40 editor transitions. Synthetic ordinary tool, Split and Organizer outputs reopen with expected formats/page counts. Protect is verified by an authorized worker unlock and reopening its decrypted copy. Source hashes remain unchanged. Working set is sampled once per second: stress reached 559.0 MiB over 209.0 seconds; this is not an OS peak measurement or a general leak guarantee.

`final-source-scope.json` records 125 original top-level host source files: 117 unchanged, eight changed, one added. The two production writers are `DesktopMenu.cs` and `LivePdfEditor.cs`; six original DEBUG verification/isolation files changed and `CrashStabilityAudit.cs` was added. `source-freeze-7.json` and `final-build-binding.json` verify all 126 current files remained unchanged after build. Protected shell/design/engine/metadata sources were not edited by this task. `git diff --check -- src/Hosts/Pertamina.Oto.Desktop.App` exits 0.

Visual inspection covers the actual final 1480/800/640 home images plus reader/editor/result routes. The final 1480 home PNG is byte-identical to baseline, SHA256 `2F8F24B42ED80299C3AD41D290AFA9910359ED23D4CEE285F104B7624C2B9DF6`. Narrow screenshots retain the design and differ in existing hover/native scrollbar state; no narrow byte-equality claim. Evidence: `final-visual-comparison.json`, `final-visual-inspection.json`, `native/home/`.

`final-owned-crash-events.json` successfully queries Windows Application event 1000 for the 18 exact owned final audit PIDs from 10:32:45 Asia/Jakarta and finds zero crash events. `final-safe-errors-summary.json` filters the actual final MVID: zero `xaml-unhandled` or `runtime-unhandled`. Handled synthetic negative-test errors are retained rather than hidden. Original dump attribution remains limited as documented above.

## Coverage ledger

PASS means the explicitly described synthetic/native cases passed; it does not imply every possible file or physical input has been tested.

| Capability | Final verified scope |
| --- | --- |
| merge | PASS: empty/loaded routes, duplicate input admission, repeated history/return, valid reopened merged PDF, source hash |
| split | PASS: source-ready lifecycle, actual split export, reopened one-page output, repeated routes |
| remove | PASS: actual specialized page selection, export, reopened PDF, source hash, navigation |
| extract | PASS: actual specialized selection, export, reopened PDF, source hash, navigation |
| organize | PASS: import/export, reopened three-page output, retained draft/navigation |
| compress | PASS: processing/reopened PDF, source hash, navigation |
| images | PASS: actual synthetic image-to-PDF conversion/reopen plus home, close, print and preview lifecycle checks |
| word-pdf | PASS: native navigation, admitted local Word worker 27 checks, real A4 conversion and reopened native output 39 UI checks; unverified A3/landscape/letter fail closed with explicit limitations |
| pdf-jpg | PASS: processing, decoded image output, source hash, repeated routes |
| rotate | PASS: output/reopen, route and preview lifecycle |
| numbers | PASS: output/reopen, route and preview lifecycle |
| watermark | PASS: output/reopen, route and preview lifecycle |
| crop | PASS: output/reopen, route and preview lifecycle |
| edit | PASS: full editor, delayed-event regression, real text/undo/redo/export, source hash, dirty/close/print guards |
| unlock | PASS: synthetic authorized unlock/reopen, credential/error and draft guards, source hash |
| protect | PASS: processing, authorized unlock validation/reopen, credential lifecycle, source hash |
| reader | PASS: search/highlights, virtualization/cache, mixed paper sizes, zoom, session retention and stale render cases |
| result/history/favorites | PASS: exact result open/reveal/print target, missing result errors, persistence and repeated return |
| settings/help/assistant/admin | PASS: navigation and bounded unavailable states; no administration/online service claim |
| print | PASS: Microsoft Print to PDF only for real jobs; other negative cases use empty/virtual audit providers |
| native window | PASS: minimum/restored/maximized and final screenshots |

The 17 unavailable IDs are `scan`, `repair`, `ocr`, `ppt-pdf`, `excel-pdf`, `html-pdf`, `pdf-word`, `pdf-ppt`, `pdf-excel`, `pdfa`, `forms`, `sign`, `redact`, `compare`, `summary`, `translate`, `markdown`. All have navigation PASS / function NOT_IMPLEMENTED. They are not counted as implemented operation PASS.

Physical mouse wheel/touch/pinch, hardware DPI/multiple monitors, OS picker/clipboard/drop/Save As gestures, physical/network printers, screen-reader/human UAT, fresh Windows without global runtime, installed activation and online/Production distribution are NOT_TESTED. Synthetic automation invokes real controls and processing but does not replace physical UAT. No all-future-crash, complete stability, phase completion or Production claim.

## Verified isolated delivery

Candidate build **`20261011-104936-27c39aeb`**, product **0.2.4 / Pengembangan**, exists only under parent-workspace `releases/windows-local-candidates/crash-stability-01a128b8/`. The package and ZIP carry the exact tested DLL above. No rebuild occurred between native verification and packaging.

`Build-IsolatedCandidate.ps1` retains the standard `eng/Build-LocalUpdate.ps1` gate sequence with the prepared exact binary, redirects only process audit data and association evidence, and archives generated audit files before the unchanged publisher writes the isolated candidate feed. `delivery-isolation.patch` records the adaptation. The command exited 0 (`final-delivery.log`). All restores report cached/up-to-date dependencies; no download, new dependency or provider-admission waiver.

| Standard gate | Actual result |
| --- | --- |
| ReleaseNotesChecks | 44 PASS |
| ProductVersionChecks | 25 PASS plus 13 legacy updater checks |
| Inert product-version package fixture | 8 PASS; synthetic fixture only |
| DistributionChecks | 63 PASS plus 13 local updater checks |
| Private runtime payloads | Five PASS; .NET 10.0.12 |
| Local PDF worker | 41 checks passed |
| Editor worker | 102 checks passed |
| Search worker | 67 PASS |
| Word worker | 27 PASS; actual admitted local provider |
| Word native UI | 39 PASS / 0 FAIL |
| Quality native UI | 39 PASS / 0 FAIL |
| Isolated PDF association/opener self-test | 46 PASS; no real association change |

`Restore-CleanCandidate.ps1` verified every original clean payload hash, retained all 1,331 original payload files, and moved 458 generated audit files into `preserved-native-generated/`; no evidence was deleted. The publisher adds one immutable `build-info.json`, yielding 1,332 manifest entries. Native documents, credentials, isolated library, portable marker and audit reports are absent from the deliverable. The original external dump files were never packaged.

`Verify-CandidatePackage.ps1` exits 0 and verifies candidate latest → manifest hash, every payload length/SHA256, all 18 primary/supplemental passing native records, all 20 indexed report hashes and DLL bindings (including Word/quality), both actual Explorer inspection records and screenshot hashes, ZIP entry count and every decompressed ZIP hash. Evidence: `final-evidence-index.json`, `candidate-package-verification.json`, `final-package.log`.

| Identity | Value |
| --- | --- |
| Build | `20261011-104936-27c39aeb` |
| Manifest SHA256 | `D08CC98AE59E1749980DF91923BD15F0B930AF9A91B489E849912D8A4A3BE03E` |
| ZIP | `OTO-local-candidate-0.2.4-20261011-104936-27c39aeb.zip` |
| ZIP bytes | `222152773` |
| ZIP SHA256 | `E126937C7C99C0B1A1D5F52229B585C7BED751E4C758FE25003A8C62167E0F8A` |

Final integrity recheck after delivery remains PASS: all 126 source hashes unchanged, installed `current.json` and root active `latest.json` equal their baseline hashes, zero unhandled safe-log records for the final MVID. See `final-integrity.json`, `installed-feed-after.json`, `final-safe-errors-summary.json`. No installation, promotion, shortcut/launcher mutation, force-closing user work, network publishing, product bump or central phase-record change occurred. One owned synthetic fixture Explorer window was left open after read-only inspection; no user window was closed or manipulated.

No local candidate gate remains blocked. Physical UAT and any decision to install/promote the candidate are outside this task and retain their human authority. The delivered evidence supports the repaired reproduced paths and tested cases, without claiming all future crashes impossible.
