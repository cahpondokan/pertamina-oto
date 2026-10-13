# App exit / saved Merge close handoff — 13 October 2026

Worker: Penutupan — Hasil dan keluar utama dengan…; approved scoped UI revisions 2 and 3, not a phase execution. Shared checkout `worktrees/core-functional-first`, branch `task/core-functional-first`, HEAD `84cbb0f538020003bcfb6583e94c2fcf6524f87c`. Dirty source preserved; no subagents, dependencies, builds, running app, clipboard, user documents, installed pointer, feed or publication touched.

## Baseline and ownership

Before-source copies saved before apply_patch at `docs/development/app-exit-result-baseline-20261013/MergeCloseButton.cs.before` and `MergeResultPanel.cs.before`.

| Path under desktop app | Before SHA256 | After SHA256 |
|---|---|---|
| MergeCloseButton.cs | 29C98E0B1961C7D26FB32B63D5CB0BA750ADBE8FA24568529B99D1D3BE2EDB5D | 20C901707BF31A5B7A0ABCF9E5D4CC436D9AE5CDB9F94F032049CF870BA0DC4B |
| MergeResultPanel.cs | 9A7B73D7AAF65CB96C3550785802237410D35A4907D58D861BBB052EEDEFAB1F | unchanged |
| AppExitConfirmation.cs | absent | F8B683DADB65E188737115AF3ABEBFBEBA551FAAEEB3BF761DDC48368BF4B8B4 |
| AppExitConfirmationAudit.cs | absent | 9D91F4E4EC6A65636F67A6719046CF25DC1EB7C668EA21DDA7C463B111975F4E |
| Assets/BibloAppExit.png | absent | 53671F57B658D97EFF3D3BDEB6036EF636B04A1FBEB202173677106AC0F43B73 |

Asset byte-for-byte matches root `output/imagegen/biblo-app-exit-v1/biblo-keluar-oto.png`, preserving alpha; no image generation. Approved mockup viewed from `C:/Users/cahpo/.codex/generated_images/01a12df3-d5a1-7a90-aaed-790dece50553/exec-293f9f00-49da-4c57-a572-ac42df51a56e.png`.

## Implemented behavior / diff

MergeCloseButton.cs extracts the existing red native Button resource overrides into `StyleMergeDocumentCloseButton(Button)`. The loaded Merge header calls the helper; existing DocumentCloseButton handler, editable busy gate and location are retained. Normal fill/border/text remain #FFF1F2/#F5B6BD/#B42335, with existing hover/pressed/disabled resources and HighContrast fallback. MergeResultPanel.cs was inspected and remains unchanged because actual saved-result action is constructed in UnifiedResult.cs and decorated in SavedResultViewport.cs, both outside this worker's writer ownership.

AppExitConfirmation.cs adds `ShowAppExitConfirmationAsync()` returning Primary for exit / None for retain. It never sets allowClose, clears passwords/drafts, processes or writes a document. Exact approved title, two body sentences, reassurance, exactly two custom actions; safe initial focus, Enter and Escape return None; Space can explicitly invoke focused action. Exit blush resources preserve native states; safe action uses native AccentButtonStyle. Compact left Biblo, wrapped text, vertical scrolling and stacked actions below 480-DIP content width. Resize subscription removed on Closed; local finish guard rejects duplicate completion.

AppExitConfirmationAudit.cs adds DEBUG native fixture `AuditAppExitConfirmationAsync(string output)` for 1400/660 outer widths, both action mappings, exact copy, action count, safe focus, hit targets, asset decoding, modal bounds and unchanged close authority/session. Produces audit.txt and two PNGs; no actual Close call. It must run only under isolated audit storage.

## Exact integration hunks (integrator sole writer)

SavedResultViewport.cs, immediately after existing content replacement inside `foreach (var command in secondary.Children.OfType<Button>())`:

```diff
             if (label is "Tutup dokumen" or "Detail hasil") command.Content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = {
                 new FontIcon { Glyph = label == "Tutup dokumen" ? "\uE711" : "\uE946", FontSize = 16 }, new TextBlock { Text = label } } };
+            if (isMergeResult && label == "Tutup dokumen") StyleMergeDocumentCloseButton(command);
```

UnifiedResult.cs line91 already puts this action immediately after Kembali edit. Do not change handlers; per-document approved MergeCloseConfirmation remains unchanged.

Project content:

```diff
     <Content Include="Assets/Biblo/*" CopyToOutputDirectory="PreserveNewest" />
+    <Content Include="Assets/BibloAppExit.png" CopyToOutputDirectory="PreserveNewest" />
```

NavigationHistory.cs actual `ConfirmCloseAsync`, not DocumentLifecycle.cs. Remove its empty-session allowClose bypass to make idle empty/home exit universal. Replace current active/importing/editor-working check with RestartBusy (includes pending paste, reader/search/rendering, extraction/removal and organizer hold). Keep messageOpen and AppWindow.Closing closeCheckQueued guards. Replace dialog construction with:

```csharp
if (await ShowAppExitConfirmationAsync() == ContentDialogResult.Primary && !RestartBusy)
{ allowClose = true; Close(); }
```

Before showing modal, after messageOpen=true, capture `var focused = Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(root.XamlRoot) as Control;` and `var resumeEditing = liveEditor?.PauseForReadPreview();`. In finally, while messageOpen is still true, if !allowClose invoke resumeEditing when nonnull, otherwise restore loaded focused Control using FocusState.Programmatic. Use nested finally to ensure messageOpen=false even if restoration throws. PauseForReadPreview sets readingPreview before LostFocus can commit pending original text and restores inline input/selection; existing patterns in ReadPreviewDialog.cs15 and UpdateCompletion.cs175. Route DesktopMenu Keluar through ConfirmCloseAsync so no duplicate dialog/busy bypass. These are integrator-owned hunks and have NOT been applied by this worker.

Program.cs DEBUG audit registration next to existing mergeCloseAuditIndex:

```csharp
var appExitAuditIndex = Array.IndexOf(homeAuditArgs, "--audit-app-exit");
if (appExitAuditIndex >= 0 && appExitAuditIndex + 1 < homeAuditArgs.Length)
    MainWindow.PrepareCenteredAudit(homeAuditArgs[appExitAuditIndex + 1]);
```

In the existing post-activation DEBUG audit dispatch before normal startup:

```csharp
if (appExitAuditIndex >= 0 && appExitAuditIndex + 1 < homeAuditArgs.Length)
{ await ((MainWindow)window).AuditAppExitConfirmationAsync(homeAuditArgs[appExitAuditIndex + 1]); Exit(); return; }
```

## Verification and limitations

PowerShell source assertions: 14 PASS — exact title/body/instruction/reassurance, exactly two Action constructors, safe keys/focus, image hash equality, shared Merge style, unchanged MergeResultPanel hash, no close-authority mutation, no document IO, native state keys, native HighContrast fallback. All assertions completed; combined command exit1 is expected from subsequent `git diff --no-index` showing the owned change, not assertion failure. `git diff --check` exit0 on existing tracked workspace diff (CRLF warnings only); owned/new untracked files require direct inspection as Git omits them. Before-copy versus MergeCloseButton diff confirms only helper extraction, no handler mutation. The first read-only rg call used unsupported PowerShell brace expansion (parser exit1); corrected explicit file arguments succeeded.

Build/native/runtime tests intentionally pending integrator's single combined build. Native fixture itself is uncompiled/unexecuted, so no runtime PASS claim. Physical keyboard/pointer states, screen reader, DPI and live HighContrast switching remain unverified. Audit integrated universal home/window/menu routing, busy refusal, return preserving synthetic non-Merge CurrentToolDraft/password/session values and pending inline editor/selection before publication. Do not log those values. Guard lifetime during focus restoration deserves explicit native verification.

No external network, dependency, security boundary or destructive action introduced. Resource cost: one compact image and dialog created per invocation, event unsubscription on dismissal; no numerical memory benchmark. No blocker within helper scope; integration/runtime verification is the remaining dependency. Recommended order: content registration + lifecycle/menu and saved-result callsites, audit dispatch, single build, native audits, then authorized delivery gates.
