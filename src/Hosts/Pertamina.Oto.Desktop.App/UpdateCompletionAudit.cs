#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private static string? restartAuditPreference;
    internal static void PrepareRestartAuditStorage()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "restart-audit"); Directory.CreateDirectory(directory);
        libraryTestPath = Path.Combine(directory, "library.json"); restartAuditPreference = Path.Combine(directory, "preferences.json");
        File.WriteAllText(restartAuditPreference, System.Text.Json.JsonSerializer.Serialize(new FolderPreference(directory)));
    }
    internal async Task AuditUpdateCompletionAsync()
    {
        if (Environment.GetEnvironmentVariable("OTO_DOCUMENT_CLOSE_AUDIT") == "1")
        { await AuditDocumentCloseAsync(); return; }
        var directory = Path.Combine(AppContext.BaseDirectory, "restart-audit"); Directory.CreateDirectory(directory);
        var report = new List<string>(); const string staged = "20261001-110100-bbbbbbbb";
        void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); report.Add("PASS " + label); }
        static IEnumerable<DependencyObject> Walk(DependencyObject node)
        { yield return node; for (var i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++) foreach (var child in Walk(VisualTreeHelper.GetChild(node, i))) yield return child; }
        static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
        ContentDialog Popup() => VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot).SelectMany(p => Walk(p.Child)).OfType<ContentDialog>().First();
        async Task Settle() { root.UpdateLayout(); await Task.Delay(250); root.UpdateLayout(); }
        async Task Capture(UIElement dialog, string name)
        {
            await Settle(); var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(dialog); var bytes = await bitmap.GetPixelsAsync();
            var file = await (await StorageFolder.GetFolderFromPathAsync(directory)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite); var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, bytes.ToArray()); await encoder.FlushAsync();
        }
        try
        {
            restartFixtureRoot = Path.Combine(directory, "missing-launcher-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(restartFixtureRoot);
            defaultFolder = null; folder = null; location.Text = "Folder hasil fixture"; ShowSettings(); await Settle();
            var opener = Descendants(body).OfType<Button>().First(b => AutomationProperties.GetName(b) == "Periksa update"); opener.Focus(FocusState.Programmatic);
            var beforeVersion = SafeRunningVersion();
            foreach (var width in new[] { 1440, 820, 620 })
            {
                AppWindow.Resize(new Windows.Graphics.SizeInt32(width, width == 1440 ? 1000 : 650)); await Settle();
                var show = ShowUpdateCompletionAsync(staged); await Settle(); var dialog = updateCompletion!;
                Check(dialog is not null && messageOpen && dialog.DefaultButton == ContentDialogButton.Close &&
                    (FocusManager.GetFocusedElement(root.XamlRoot) as Button)?.Name == "CloseButton", "native completion non-destructive initial focus and no destructive default " + width);
                var texts = Walk(dialog!).OfType<TextBlock>().Select(t => t.Text).ToArray();
                Check(texts.Contains("Versi baru siap digunakan") && texts.Contains("Tidak perlu mulai ulang sekarang."), "heading and truthful product/build activation explanation " + width);
                var details = Walk(dialog!).OfType<Expander>().Single(); details.IsExpanded = true; await Settle();
                var detailTexts = Walk(dialog!).OfType<TextBlock>().Select(t => t.Text).ToArray();
                Check(detailTexts.Contains(beforeVersion) && detailTexts.Contains("Versi produk belum tercatat · Build " + staged), "running and legacy staged identities retained under version details " + width);
                details.IsExpanded = false; await Settle();
                var restartButton = Walk(dialog!).OfType<Button>().Single(b => b.Name == "PrimaryButton");
                var continueButton = Walk(dialog!).OfType<Button>().Single(b => b.Name == "CloseButton");
                Check(Grid.GetColumn(restartButton) < Grid.GetColumn(continueButton) && continueButton.MinHeight >= 44 &&
                    ((SolidColorBrush)continueButton.Background).Color == Windows.UI.Color.FromArgb(255, 0, 103, 217),
                    "approved secondary restart left and blue continue right " + width);
                Check(Walk(dialog!).OfType<Image>().All(i => i.Source is SvgImageSource), "consistent local SVG icons " + width);
                Check(dialog!.PrimaryButtonText == "Mulai ulang sekarang" && dialog.CloseButtonText == "Lanjut bekerja", "native accessible footer actions " + width);
                var panel = Walk(dialog).OfType<FrameworkElement>().First(e => e.Name == "BackgroundElement");
                Check(panel.ActualWidth <= root.ActualWidth && panel.ActualHeight <= root.ActualHeight, "dialog panel fits desktop or compact viewport " + width);
                await Capture(panel, "completion-" + width);
                var close = Walk(dialog).OfType<Button>().Single(b => AutomationProperties.GetName(b) == "Tutup dialog update dan lanjut bekerja");
                if (width == 1440) Invoke(Walk(dialog).OfType<Button>().Single(b => b.Name == "CloseButton"));
                else if (width == 820) Invoke(close);
                else dialog.Hide(); // Same native dismissal path; physical Esc remains UAT.
                await show; await Settle();
                Check(!allowClose && !messageOpen && SafeRunningVersion() == beforeVersion, "continue/X/native dismissal keeps instance/version " + width);
                Check(ReferenceEquals(FocusManager.GetFocusedElement(root.XamlRoot), opener), "focus returns to opener " + width);
            }
            var stagedDirectory = Path.Combine(restartFixtureRoot, "versions", staged); Directory.CreateDirectory(stagedDirectory);
            File.WriteAllText(Path.Combine(stagedDirectory, "build-info.json"), System.Text.Json.JsonSerializer.Serialize(new LocalAppUpdate.Release(staged, "", ApplicationVersion.Running().ProductVersion, "Pengembangan")));
            var sameProductShow = ShowUpdateCompletionAsync(staged); await Settle();
            Walk(updateCompletion!).OfType<Expander>().Single().IsExpanded = true; await Settle();
            var sameProductTexts = Walk(updateCompletion!).OfType<TextBlock>().Select(t => t.Text).ToArray();
            Check(sameProductTexts.Count(t => t == ApplicationVersion.Running().Display) == 2, "running and staged same product identity independently read");
            Check(sameProductTexts.Any(t => t.StartsWith("Pembaruan build untuk versi produk yang sama", StringComparison.Ordinal)), "same product new build explained in native UI");
            Check(!sameProductTexts.Contains(staged), "known staged build ID absent from product rows");
            await Capture(Walk(updateCompletion!).OfType<FrameworkElement>().First(e => e.Name == "BackgroundElement"), "completion-same-product");
            updateCompletion!.Hide(); await sameProductShow; await Settle();
            Check(SafeRunningVersion() == beforeVersion, "staged identity never changes running product");
            Check(!RestartBusy && !RestartHasSession, "empty settings session eligible for preparation");
            using (var operation = new CancellationTokenSource())
            {
                active = operation; var attempt = RestartForUpdateAsync(staged); await Settle(); Check(Popup().Title as string == "Pekerjaan sedang berjalan", "worker blocks restart with explanation"); Popup().Hide(); await attempt; active = null;
                Check(!allowClose && !restartRequested, "busy refusal keeps old app");
            }
            importing = true; Check(RestartBusy, "import guarded"); importing = false;
            pastingDocument = true; Check(RestartBusy, "clipboard import guarded"); pastingDocument = false;
            extractLoading = true; Check(RestartBusy, "extract loading guarded"); extractLoading = false;
            toolDrafts["merge"] = new(["fixture.pdf"], "1-2", "draft", "draft.pdf", 0, 20);
            var savedDraft = toolDrafts["merge"];
            var unsaved = RestartForUpdateAsync(staged); await Settle(); var warning = Popup();
            Check(AutomationProperties.GetName(warning) == "Simpan perubahan dahulu" && warning.DefaultButton == ContentDialogButton.Close, "unsaved requests return/save with cancel default");
            Check(Walk(warning).OfType<TextBlock>().Any(t => t.Text == "fixture.pdf") &&
                warning.PrimaryButtonText == "Tinjau & simpan" && warning.CloseButtonText == "Nanti saja", "warning identifies actual work and distinct actions");
            foreach (var width in new[] { 1440, 820, 620 })
            {
                AppWindow.Resize(new Windows.Graphics.SizeInt32(width, width == 1440 ? 1000 : 700)); await Settle();
                var panel = Walk(warning).OfType<FrameworkElement>().First(e => e.Name == "BackgroundElement");
                Check(panel.ActualHeight <= root.ActualHeight && panel.ActualWidth <= root.ActualWidth, "warning fits viewport " + width);
                await Capture(panel, "unsaved-" + width);
            }
            Invoke(Walk(warning).OfType<Button>().Single(b => b.Name == "CloseButton")); await unsaved;
            Check(toolDrafts["merge"] == savedDraft && !allowClose, "cancel retains exact tool draft"); toolDrafts.Clear();
            toolDrafts["merge"] = new([], "", "", "pending-output-name.pdf", 1, 27);
            Check(RestartHasSession, "tool output/options snapshot guarded even without selected files"); toolDrafts.Clear();
            homeUndo.Push(["fixture.pdf"]); Check(!RestartHasSession, "home undo alone does not pretend to be unsaved document work"); homeUndo.Clear();
            homeRedo.Push(["fixture.pdf"]); Check(!RestartHasSession, "home redo alone is not a document change"); homeRedo.Clear();
            layoutUndo.Push([]); Check(!RestartHasSession, "empty organizer undo is not pending output"); layoutUndo.Clear();
            layoutRedo.Push([]); Check(!RestartHasSession, "empty organizer redo is not pending output"); layoutRedo.Clear();
            toolDrafts["split"] = new(["fixture.pdf"], "", "", "hasil-split.zip", 0, 20);
            splitSource = "fixture.pdf"; splitCount = 3; splitRanges.Clear(); splitRanges.Add(new(1, 3));
            Check(!RestartHasSession, "unchanged split source and default range are not edits");
            splitRanges.Add(new(2, 3)); Check(RestartHasSession, "custom split plan remains protected");
            splitSource = null; splitRanges.Clear(); splitCount = 0; toolDrafts.Clear();
            toolDrafts["extract"] = new(["fixture.pdf"], "1", "", "hasil-extract.pdf", 0, 20);
            Check(RestartHasSession, "explicit extract selection protected across navigation"); toolDrafts.Clear();
            var typing = new TextBox { Text = "pending unsaved typing" }; body.Children.Add(typing); await Settle(); Check(!RestartHasSession, "unrelated search textbox does not block restart");
            var continued = ShowUpdateCompletionAsync(staged); await Settle(); Invoke(Walk(updateCompletion!).OfType<Button>().Single(b => b.Name == "CloseButton")); await continued;
            Check(typing.Text == "pending unsaved typing", "continue preserves textbox exactly"); body.Children.Remove(typing);
            var source = Path.Combine(directory, "restart-editor-source.pdf"); WriteHistoryAuditFixture(source);
            var editable = Path.Combine(directory, "restart-editor-editable-" + Guid.NewGuid().ToString("N") + ".pdf");
            var reply = await WorkerClient.RunAsync(new("edit", [source], editable, Marks: [new(0, "text", 40, 120, 300, 35, "RESTART ORIGINAL", 16)]), CancellationToken.None);
            Check(reply.Success, "actual editable PDF fixture produced by admitted worker");
            ShowLiveEditor(true); await liveEditor!.OpenAsync(editable); await Settle();
            await liveEditor.AuditRestartInlineAsync(Check, async () =>
            { continued = ShowUpdateCompletionAsync(staged); await Settle(); updateCompletion!.Hide(); await continued; });
            liveEditor.ReleaseFiles(); liveEditor = null;
            ShowSettings(); await Settle(); toolDrafts.Clear(); // Explicit fixture cleanup only; production never clears drafts for restart.
            ShowPdfReader(); await pdfReader!.OpenAsync(source); await Settle();
            Check(!RestartHasSession, "actual reader with open PDF is not an unsaved edit");
            pdfReader.Dispose(); pdfReader = null; ShowSettings(); await Settle();
            await AuditRestartSavedToolsAsync(source, directory, Check);
            AuditRestartWorkStates(Check);
            var reviewed = ""; var longName = new string('L', 180) + ".pdf";
            var sampleWork = new RestartWork[] { new(longName, "Edit PDF", () => reviewed = "editor"), new("B.pdf", "Gabung PDF", () => reviewed = "merge") };
            RestartWork chosenWork = sampleWork[0];
            var multi = BuildRestartWorkDialog(sampleWork, work => chosenWork = work);
            var multiShown = multi.ShowAsync(); await Settle();
            Invoke(Walk(multi).OfType<Button>().Single(b => AutomationProperties.GetName(b).StartsWith("B.pdf,", StringComparison.Ordinal)));
            Check(chosenWork == sampleWork[1], "multiple documents can select exact review target");
            await Capture(Walk(multi).OfType<FrameworkElement>().First(e => e.Name == "BackgroundElement"), "unsaved-multiple-long-name");
            multi.Hide(); await multiShown; chosenWork.Review();
            Check(reviewed == "merge", "selected document review routes without saving or discarding");
            restartRequested = true; await RestartForUpdateAsync(staged); Check(!messageOpen && !allowClose, "repeat request gate prevents second launch"); restartRequested = false;
            var failed = RestartForUpdateAsync(staged); await Settle();
            for (var i = 0; i < 20 && Popup().Title as string != "Aplikasi tetap terbuka"; i++) await Settle();
            Check(Popup().Title as string == "Aplikasi tetap terbuka" && !allowClose, "missing launcher fails without closing old instance"); Popup().Hide(); await failed;
            Check(!restartRequested && !allowClose, "failed preparation can be retried safely");
            report.Add("LIMIT Native renders use synthetic staged version and isolated preference/library paths. Physical Esc, mouse/touchpad and screen-reader UAT remain manual.");
        }
        catch (Exception error) { report.Add("FAIL " + error.GetType().Name + ": " + error.Message); }
        File.WriteAllLines(Path.Combine(directory, "audit.txt"), report); Environment.ExitCode = report.Any(r => r.StartsWith("FAIL", StringComparison.Ordinal)) ? 1 : 0;
    }
    internal async Task AuditNativeRestartAsync(string fixtureRoot, string next)
    {
        restartFixtureRoot = Path.GetFullPath(fixtureRoot);
        var workspaceArtifacts = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, ".."));
        if (!restartFixtureRoot.StartsWith(workspaceArtifacts + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Fixture must remain under workspace artifacts.");
        Environment.SetEnvironmentVariable("OTO_RESTART_NATIVE_FIXTURE", "1");
        ShowSettings(); root.UpdateLayout(); await Task.Delay(250);
        if (RestartBusy || RestartHasSession) throw new InvalidOperationException("Native restart fixture must be empty.");
        File.WriteAllText(Path.Combine(restartFixtureRoot, "native-parent-started.txt"), "Isolated empty native parent; no user instance used.");
        await RestartForUpdateAsync(next);
    }
    private sealed partial class LiveEditor
    {
        internal async Task AuditRestartInlineAsync(Action<bool, string> check, Func<Task> dialog)
        {
            await LoadOriginalRunsAsync();
            var run = originalRuns.First(r => r.Text.Contains("RESTART ORIGINAL", StringComparison.Ordinal));
            StartOriginalInline(run); await Task.Delay(150); inlineText!.Text = "PENDING RESTART DRAFT"; await Task.Delay(150); inlineText.Select(3, 5);
            var before = originalInlineDraft; var undoCount = past.Count;
            check(owner.RestartHasSession && Dirty, "actual original inline pending typing blocks restart");
            await dialog();
            check(originalInlineDraft == before && inlineText?.Text == "PENDING RESTART DRAFT" && past.Count == undoCount, "native completion preserves actual inline draft and undo without committing");
            check(inlineText?.SelectionStart == 3 && inlineText.SelectionLength == 5, "native completion restores inline text selection");
            check(!owner.allowClose, "original inline session remains open");
            var closing = CloseAsync(); await Task.Delay(350);
            var closePopup = VisualTreeHelper.GetOpenPopupsForXamlRoot(owner.root.XamlRoot).SelectMany(p => new DependencyObject[] { p.Child }.Concat(Descendants(p.Child))).OfType<ContentDialog>().First();
            var retain = Descendants(closePopup).OfType<Button>().Single(b => b.Name == "CloseButton");
            ((IInvokeProvider)new ButtonAutomationPeer(retain).GetPattern(PatternInterface.Invoke)).Invoke(); await closing;
            check(originalInlineDraft == before && inlineText?.Text == "PENDING RESTART DRAFT" && past.Count == undoCount, "cancelled close preserves original inline draft without implicit commit");
            check(inlineText?.SelectionStart == 3 && inlineText.SelectionLength == 5, "cancelled close restores original inline selection");
            var unchangedDraft = originalInlineDraft; var unchangedPast = past.Count;
            var review = owner.RestartForUpdateAsync("20261001-110100-bbbbbbbb"); await Task.Delay(350);
            var popup = VisualTreeHelper.GetOpenPopupsForXamlRoot(owner.root.XamlRoot).SelectMany(p => new DependencyObject[] { p.Child }.Concat(Descendants(p.Child))).OfType<ContentDialog>().First();
            var reviewButton = Descendants(popup).OfType<Button>().Single(b => b.Name == "PrimaryButton");
            ((IInvokeProvider)new ButtonAutomationPeer(reviewButton).GetPattern(PatternInterface.Invoke)).Invoke();
            await review; await Task.Delay(150);
            check(Visible && originalInlineDraft == unchangedDraft && past.Count == unchangedPast, "actual review returns to editor without committing or discarding original draft");
            inlineText!.Text = new string('R', 200); await Task.Delay(100);
            await SaveAsync(Path.GetDirectoryName(path)!, recordHistory: false);
            check(Dirty && owner.RestartHasSession && originalInlineDraft?.Text == new string('R', 200), "failed overflow save retains exact draft and restart protection");
            inlineText!.Text = "ORIGINAL"; await Task.Delay(100);
            await SaveAsync(Path.GetDirectoryName(path)!, recordHistory: false);
            check(!Dirty, "actual editor save completes: " + notice.Text);
            check(past.Count > 0, "editor save retains undo history");
            check(!owner.RestartHasSession, "saved editor with retained undo does not block restart: " + string.Join(",", owner.RestartPendingWork().Select(w => w.Tool)));
            async Task SettleHistory()
            {
                for (var i = 0; i < 100 && Working; i++) await Task.Delay(50);
                check(!Working, "editor history transaction completed before next action");
            }
            Restore(false); await SettleHistory();
            check(Dirty && owner.RestartHasSession, "undo after save becomes a real unsaved change");
            Restore(true); await SettleHistory();
            check(!Dirty && !owner.RestartHasSession, "redo back to saved content is restart-safe without clearing history");
        }
    }
}
#endif
