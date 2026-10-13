# Result actions fix — 7 October 2026

Status as of 8 October 2026: COMPLETE_LOCAL_RESULT_ACTIONS / VERIFIED / LOCAL_UPDATE_READY. Local 0.2.2 published and verified; installed activation and online publication excluded. The 7 October checkpoint below is historical and superseded by the final 8 October handoff.

Task03, one implementer, no subagents; independently authorized result actions, not phase execution. Root/worktree AGENTS and Shared Agent Contract read. Checkout task/core-functional-first, HEAD 84cbb0f538020003bcfb6583e94c2fcf6524f87c; large existing dirty work preserved. Read-only work waited for task02 terminal handoff. Task02 ownership.json releases source/build/native UI; its reviewed 0.2.1 staging package is immutable and excluded from this change.

Task card: exact saved outputs open in OTO reader; existing Print dialog takes exact PDF result without pipeline changes; supported Windows reveal/select API identifies exact file; remove duplicate folder button; preserve single/multiple/nonPDF actions and source selection. Allowed paths: UnifiedResult.cs, new ResultFileReveal.cs and ResultActionsAudit.cs, explicitly approved ReadPreviewAudit.cs and debug Program.cs hook, this document and artifacts/result-actions-20261007. PrintResultPanel.cs inspected but unchanged. All Print pipeline/engine/editor/theme/update/security/phase/installed/private document paths excluded. Stop on competing writer, failed checks, unsafe file mutation or missing publication authorization.

Implementation: single and multi PDF Buka hasil call existing OpenPdfReaderAsync(processed:true). File presence checked before open/print/reveal; missing result leaves visible message. Each multi-output row reveals its own file; single output has one reveal action. SHParseDisplayName plus SHOpenFolderAndSelectItems(cidl=0) uses an absolute item identifier with no shell interpolation/association change; allocated identifiers are freed. Location remains text. Preview hasil remains shared dialog. PrintResultPanel preview/card unchanged.

Audit: new synthetic spaced Unicode similar-name fixtures; exact second output in reader and Print dialog; no submitted print job, duplicate-folder absence, missing-output actions, nonPDF capability limits, original/output hashes. ReadPreviewAudit updates full-reader contract and still exercises explicit result preview with all prior preview/editor checks. Explorer native selected item evidence required separately; success HRESULT is not selection proof. No physical printer/UAT/network-disabled OS claim.

Baseline copies/hashes: artifacts/result-actions-20261007/baseline. Build and audit evidence pending. Version 0.2.2 decision remains with parent/human; metadata and publication untouched.

API: https://learn.microsoft.com/en-us/windows/win32/api/shlobj_core/nf-shlobj_core-shopenfolderandselectitems
## Final verification handoff

Desktop analyzer build command: D:/DevTools/VS2026Community/MSBuild/Current/Bin/MSBuild.exe src/Hosts/Pertamina.Oto.Desktop.App/Pertamina.Oto.Desktop.App.csproj /t:Build /p:Configuration=Debug /p:DesktopUiOutputPath=<checkout>/artifacts/result-actions-20261007/candidate-final/ /v:minimal /nologo; exit 0. Normal analyzers/warnings-as-errors retained. First audit compile CA1865 correction and sandbox capture access denial retained in build.txt/result-command.json; excluded from final results. Two final source adjustments landed during the first failed build; final manifest and candidate-final were then frozen and revalidated after tests.

Same native binary: --audit-result-actions exit 0 (15 PASS); --audit-read-previews exit 0 (73 PASS); --audit-responsive exit 0 (462 PASS); --self-test-flow exit 0. Total 550 assertions, 0 FAIL; no zero-discovery claim. Native result audit used synthetic files and empty printer list, never submitted a print job. Native Windows capture required auto-reviewed elevated access after sandbox denial. See result-command-native.json, regression-commands.json and verification-summary.json.

Acceptance mapping:
- Single PDF exact output opens in full OTO reader: PASS.
- Second output of multi-PDF opens in full OTO reader: PASS.
- Original input selection and source/output hashes preserved: PASS.
- Existing Print dialog receives exact second output; no submitted job: PASS.
- Missing output Buka/Cetak/reveal produces visible message: PASS.
- Single reveal and per-file multi reveal offered; duplicate folder button absent: PASS.
- Non-PDF open/reveal offered, no unsupported Print: PASS.
- Shared explicit preview/editor draft regression and responsive suite: PASS.
- Explorer exact item selected and visible: NOT VERIFIED. Native reveal call completed and requested synthetic Hasil Ω 02.pdf; this is API evidence only, not selection proof.

Computer-use kernel failed three initializations (including reset) with `failed to write kernel assets: The system cannot find the path specified. (os error 3)`. Supported recovery exhausted; no custom UI automation or user-window manipulation used. native-inspection-blocker.json records exact blocker. No native Explorer screenshot exists. Restore the computer-use runtime and inspect the selected item in the isolated fixture; rerun source-aware audit as necessary. Do not call this feature fully verified before selection proof.

Single-result, multiple-results and missing-output RenderTargetBitmap captures in candidate-final/result-actions-audit were visually inspected. Saved file/detail/actions visible, folder duplicate absent, missing message readable. Multi-output uses existing scroll list; each row has explicit file-specific actions. These are application captures, not physical UI input or Explorer evidence. Existing short/responsive layouts passed 462 assertions, without redesign.

Final app DLL SHA256: 947D048E69A20F219F4901A3A4D85CF17CAD0A65AAA492DF052645728A1C67D9. Baseline and final source hashes, three limited diffs and unchanged Print-lane/metadata hashes retained in evidence. PrintResultPanel.cs remains unchanged. No dependency/download/upload/default-association/OS setting/source-document/installed mutation. LocalDiagnostics records sanitized type/HResult/method only; private screenshot stayed in its original temporary location. Reveal helper has no network or document processing. OS network-disabled testing/physical UAT not claimed.

Source/build/native UI ownership RELEASED in ownership.json. Parent may read this handoff directly. Remaining owners: computer-use runtime/Integrator for native selection evidence; human/parent for version decision (0.2.2 proposed, no answer received). Delivery build/package gates remain necessary after both acceptance and metadata authorization; no staging package was relabelled and task02 immutable package is unaffected. No public publication, installed activation or Production approval. No task02 callback retry after its reported rejection.
## Resumed approval and native proof — 8 October 2026

Human consent relayed by Agent 0 authorizes 0.2.2 / Pengembangan and local publication after gates. Task02 independently published immutable 0.2.1 and released feed ownership. Existing result source/candidate matched all 7 October hashes before changes. No feature implementation was restarted. Metadata and notes now use 0.2.2, actual date 2026-10-08; prior release history preserved.

Current computer-use skill 26.1002.52244 initializes successfully. Baseline rerun exposed audit-only timing race in Print Opened assertion; retained failure at artifacts/result-actions-20261008/preflight-native. ResultActionsAudit now waits for the actual filename visual and captures assertion failures without an unhandled UI exception; assertion remains exact. Fixture filenames add ampersand/brackets. Two inspection stages keep the isolated app alive until external native evidence is reviewed. No Print pipeline change.

New /t:Rebuild with normal analyzers passed. New 0.2.2 native audit: 17 PASS. Single-output reveals Hasil Ω & [01].pdf; multi-output reveals Hasil Ω & [02].pdf among similar names. Native foreground Explorer screenshot highlight, details exact filename and 1 selected item corroborate each case. No Explorer file-list click/keyboard selection was sent. Initial occluded screenshot was discarded; only actual foreground Explorer observations accepted. Only privacy-cropped file-list/breadcrumb pixels and filtered accessibility lines are persisted. Crop records are explicit; no retouch/annotation or rendered app screenshot substitutes for Explorer.

Evidence: artifacts/result-actions-20261008/{explorer-proof.json,explorer-single.png,explorer-multiple-second.png,explorer-single-accessibility.json,explorer-multiple-second-accessibility.json}. Full-window temporary capture was used solely to crop out unrelated sidebar/account details; evidence includes only synthetic fixture. computer-use package import of sharp was unsupported; existing bundled Python/Pillow performed cropping, no dependency installed. Old computer-use blocker is resolved for this run.

Delivery/regression gates still in progress at this checkpoint; no installed activation or online publication.

## Final 0.2.2 delivery handoff — 8 October 2026

All result-action acceptance gates passed, including native Explorer exact selected-item proof for single and multi second output. One implementer, no subagents. Source/build/native UI/feed ownership RELEASED. No phase state change or Production claim.

Actual local package: 0.2.2 / Pengembangan, immutable build 20261008-084354-201e4764. Package: D:/AFT Ahmad Yani Semarang/Teknik/Dokumentasi Pertamina OTO/releases/windows-local/20261008-084354-201e4764. Manifest SHA256 DAAA6FCBF4829018FC5BF430323DF3504A9ADFA5C554D77EFEF36592E0C3C91E. App DLL SHA256 681159D4EAA17F3CF89C824D421942B12FF8142E004B7C0D93DBEB9AD613F550. All 392 payload lengths/hashes and 103 tested binaries verified. Previous immutable 0.2.1 and notes history retained; installed pointer unchanged.

Normal analyzer /t:Rebuild exit 0; eight final native commands exit 0, 1,206 assertions / 0 FAIL. Existing flow self-test also passed. Official eng/Build-LocalUpdate.ps1 ran with the evidence-local Verified-MSBuild.ps1 wrapper to reuse the exact frozen, analyzed, tested build; it checks source manifest, all 103 binary hashes and native/Explorer/visual evidence before copying into a clean delivery directory. No pipeline source or gate was changed/disabled. Official mandatory delivery gates and publication exit 0; delivery-verification.json rechecks every actual package payload and identity after publication.

Evidence root: artifacts/result-actions-20261008. analyzer-build.txt/result.json, native-commands.json, native-counts.json, standard-delivery-gates.txt, standard-native-counts.json, reuse-certified-build.json, verification-summary.json and delivery-verification.json retain commands/results. explorer-proof.json plus both cropped native screenshots and filtered accessibility observations prove exact selection. visual-review.json ties inspected About, notes and result layouts to the same DLL. print-source-preservation.json confirms Print pipeline files unchanged; scope-gate.json and source-frozen-hashes.json delimit this lane. Earlier failed audit/runtime evidence is retained and superseded, not represented as successful tests.

Changed scope: UnifiedResult.cs, new ResultFileReveal.cs/ResultActionsAudit.cs, ReadPreviewAudit.cs, Program.cs DEBUG hook, eng/ProductRelease.props, release-notes.json, this document and PRODUCT_VERSION.md. Existing unrelated dirty changes remain outside this lane. No dependency installation, private document upload, source mutation, association change or OS configuration change.

Limitations: Print was verified through the existing dialog with isolated empty printer list; no physical/network job or hardware UAT. No installed activation, online publication, network-disabled OS proof or Production approval. User can inspect/install the approved local package through Settings > Periksa update as a separate action. No remaining blocker for this bounded result-actions delivery.

Coordination limitation: automatic approval review rejected send_message_to_thread to Agent 0 parent because explicit human authorization to message that chat was not established in available evidence. No retry/workaround performed. Package and verification are complete; parent may read this saved handoff. Sending the report requires human authorization separately.

Coordination resolved 9 October 2026: user explicitly answered 'boleh' to the report-send permission question. Completion report successfully sent to Agent 0 parent chat 01a07c8d-3c57-7963-8500-daed7ddb07ee via send_message_to_thread. No implementation or package change.
