#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
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
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    internal async Task AuditOrganizerDesignAsync(string output, bool interactive)
    {
        Directory.CreateDirectory(output);
        var report = new List<string>();
        void Check(bool ok, string label)
        {
            report.Add((ok ? "PASS " : "FAIL ") + label); File.WriteAllLines(Path.Combine(output, "audit.txt"), report);
            if (!ok) throw new InvalidOperationException(label);
        }
        async Task Settle() { root.UpdateLayout(); await Task.Delay(250); root.UpdateLayout(); }
        async Task Capture(string name, UIElement? element = null)
        {
            await Settle(); var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(element ?? root);
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, (await bitmap.GetPixelsAsync()).ToArray());
            await encoder.FlushAsync();
        }
        static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
        async Task<ContentDialog> Popup()
        {
            for (var i = 0; i < 100; i++)
            {
                var dialog = VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot).SelectMany(p => Descendants(p.Child).Prepend(p.Child)).OfType<ContentDialog>().FirstOrDefault();
                if (dialog is not null) { await Settle(); return dialog; }
                await Task.Delay(30);
            }
            throw new InvalidOperationException("Organizer close modal missing");
        }
        async Task Choose(string name) => Invoke(Descendants(await Popup()).OfType<Button>().Single(b => b.Name == name));
        string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        var a = Path.Combine(output, "Dokumen_A_" + new string('x', 80) + ".pdf"); var b = Path.Combine(output, "Dokumen_B.pdf");
        async Task Open()
        {
            await OpenOrganizerFilesAsync([a, b]); folder = output; outputName.Text = "Susunan.pdf";
            await Settle(); await Task.WhenAll(organizerThumbnails.Values); await Settle();
        }
        try
        {
            WriteHistoryAuditFixture(a, 595, 842); WriteHistoryAuditFixture(b, 420, 595);
            var hashes = new[] { Hash(a), Hash(b) };
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter normal) normal.Restore();
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1400, 1000));
            await AuditOrganizerFeedbackAsync(output, a, b, Check, Settle, Capture);
            await Open();
            Check(organizerSourceLabels[0].TextTrimming == TextTrimming.CharacterEllipsis && ToolTipService.GetToolTip(organizerSourceLabels[0]) is string tip && tip.Contains(Path.GetFileName(a), StringComparison.Ordinal), "long source filename retains full tooltip");
            Check(pageLayout.Count == 6 && organizerSources.Count == 2, "two local synthetic PDFs imported through actual adapter");
            Check(!Descendants(body).OfType<Button>().Any(x => AutomationProperties.GetName(x) == "Tempel PDF"), "loaded toolbar has no paste button");
            Check(CenteredIcon("organize") is Image image && image.Source is BitmapImage bitmap && bitmap.UriSource.LocalPath.EndsWith("031-atur-ulang-halaman-pdf.png", StringComparison.Ordinal), "home icon uses exact organizer031 asset");
            Check(IsTextPasteTarget(outputName) && IsTextPasteTarget(password), "native text inputs recognized as paste owners; no clipboard mutation");
            var original = pageLayout.ToArray(); var moved = original[4];
            organizerHeld = organizerCards[moved.Id]; BeginOrganizerDrag(organizerHeld); organizerDrag!.Preview(1); FinishOrganizerDrag(true);
            Check(pageLayout[1].Id == moved.Id && organizerCards[moved.Id].Origin.Text == "B · Hal. 2" && organizerCards[moved.Id].Number.Text == "Urutan 2", "drag commit changes result order while B2 origin and instance stay stable");
            ActOnOrganizerCard(moved.Id, "duplicate");
            Check(pageLayout[2].Id != moved.Id && pageLayout[2].Source == moved.Source && pageLayout[2].Page == moved.Page, "duplicate gets separate identity and same provenance");
            InsertOrganizerBlank(3);
            Check(pageLayout[3].Source == -1 && organizerCards[pageLayout[3].Id].Origin.Text == "Kosong · A4", "blank explicitly has no source origin");
            var singleBlankLayout = pageLayout.ToArray();
            InsertOrganizerBlank(4);
            var independentBlanks = pageLayout.Where(p => p.Source < 0).ToArray();
            Check(independentBlanks.Length == 2 && independentBlanks.Select(p => p.Id).Distinct().Count() == 2,
                "QA-01 independent blank inserts retain separate instance IDs");
            Check(independentBlanks.All(p => AutomationProperties.GetName(organizerCards[p.Id].Root).Contains("Halaman kosong A4", StringComparison.Ordinal)
                && !AutomationProperties.GetName(organizerCards[p.Id].Root).Contains("Salinan halaman", StringComparison.Ordinal)),
                "QA-01 independent blanks never infer duplicate provenance from sentinel values");
            var twoBlankLayout = pageLayout.ToArray();
            foreach (var (page, index) in pageLayout.Select((page, index) => (page, index))) UpdateOrganizerNumber(organizerCards[page.Id], index);
            Check(pageLayout.SequenceEqual(twoBlankLayout), "QA-01 label refresh preserves IDs source page order and rotation");
            ActOnOrganizerCard(independentBlanks[0].Id, "duplicate");
            var blankCopies = pageLayout.Where(p => p.Source < 0).ToArray();
            Check(blankCopies.Length == 3 && blankCopies.Select(p => p.Id).Distinct().Count() == 3
                && blankCopies.All(p => !AutomationProperties.GetName(organizerCards[p.Id].Root).Contains("Salinan halaman", StringComparison.Ordinal)),
                "QA-01 blank duplication keeps unique identity without claiming source-page provenance");
            var sourceCopies = pageLayout.Where(p => p.Source == moved.Source && p.Page == moved.Page).ToArray();
            Check(sourceCopies.Length == 2 && sourceCopies.All(p => AutomationProperties.GetName(organizerCards[p.Id].Root).Contains("Salinan halaman; instance terpisah.", StringComparison.Ordinal)),
                "QA-01 actual PDF source duplicates retain accessible duplicate description");
            RestoreLayout(false); Check(pageLayout.SequenceEqual(twoBlankLayout), "QA-01 undo blank duplicate restores exact prior layout");
            RestoreLayout(false); Check(pageLayout.SequenceEqual(singleBlankLayout), "QA-01 undo second blank preserves original save fixture");
            ActOnOrganizerCard(pageLayout[0].Id, "rotate");
            Check(pageLayout[0].Rotation == 90 && organizerUndoButton!.IsEnabled, "rotation updates undo readiness");
            var order = pageLayout.ToArray(); RestoreLayout(false); RestoreLayout(true);
            Check(pageLayout.SequenceEqual(order), "undo redo restores exact unique instances and angles");
            var sample = organizerCards[moved.Id]; sample.Root.Focus(FocusState.Keyboard); await Settle();
            Check(sample.Actions.Opacity == 1 && sample.Actions.IsHitTestVisible, "keyboard focus reveals preview rotate remove controls");
            Check(organizerGaps.Where(g => ((Button)g.Children[0]).Opacity == 1).Count() >= 2, "adjacent insertion controls visible for focused page");
            Check(sample.Actions.Children.OfType<Button>().All(x => ToolTipService.GetToolTip(x) is not null && AutomationProperties.GetName(x).Length > 0), "all card actions have tooltip and accessible name");
            Check(sample.Root.ContextFlyout is MenuFlyout menu && menu.Items.Count == 3, "duplicate and move context commands retained");
            await Capture("loaded-focus-1400");
            foreach (var size in new[] { (660, 810), (1000, 850), (1400, 1000) })
            {
                AppWindow.Resize(new Windows.Graphics.SizeInt32(size.Item1, size.Item2)); await Settle();
                Check(pageGrid.ActualWidth > 180 && pageGrid.ActualHeight >= 250, size.Item1 + ": useful independently scrolling grid");
                Check(toolWorkspace.ActualWidth <= root.ActualWidth && toolOptions.ActualWidth >= 250, size.Item1 + ": panels within supported width");
                standardPageScroll!.ChangeView(null, standardPageScroll.ScrollableHeight, null, true); await Settle();
                var rect = run.TransformToVisual(root).TransformBounds(new Windows.Foundation.Rect(0, 0, run.ActualWidth, run.ActualHeight));
                Check(rect.Left >= 0 && rect.Right <= root.ActualWidth + 1 && rect.Bottom <= root.ActualHeight + 1, size.Item1 + ": save action reachable at page bottom");
                await Capture("loaded-bottom-" + size.Item1);
                standardPageScroll.ChangeView(null, 0, null, true);
                var closing = CloseDocumentsAsync(); var dialog = await Popup();
                Check(Descendants(dialog).OfType<TextBlock>().Any(x => x.Text == "Susunan halaman belum disimpan."), size.Item1 + ": organizer copy in reused close dialog");
                Check(Descendants(dialog).OfType<Image>().Any(x => x.Source is BitmapImage icon && icon.UriSource.LocalPath.EndsWith("031-atur-ulang-halaman-pdf.png", StringComparison.Ordinal)), size.Item1 + ": organizer031 in reused modal");
                Check(FocusManager.GetFocusedElement(root.XamlRoot) is Button { Name: "MergeCloseReturn" }, size.Item1 + ": safe default focus");
                await Capture("close-" + size.Item1, Descendants(dialog).OfType<FrameworkElement>().First(x => x.Name == "BackgroundElement"));
                await Choose("MergeCloseReturn"); await closing;
                Check(pageLayout.SequenceEqual(order), size.Item1 + ": cancelled close retains exact arrangement");
            }
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter max) max.Maximize(); await Settle(); await Capture("loaded-maximized");
            var many = pageLayout.ToArray();
            while (pageLayout.Count < 100) pageLayout.Add(original[pageLayout.Count % original.Length].Duplicate());
            RefreshPageGrid(); await Settle(); pageGrid.ChangeView(null, pageGrid.ScrollableHeight, null, true); await Settle();
            Check(pageGrid.ScrollableHeight > 100 && pageLayout.Select(p => p.Id).Distinct().Count() == 100, "100 page fixture scrolls with unique instances");
            await Capture("many-pages"); pageLayout.Clear(); pageLayout.AddRange(many); RefreshPageGrid();
            foreach (var scale in new[] { .5, 1d, 2d }) { previewScale = scale; ArrangeOrganizerCards(pageLayout, false); await Settle(); Check(organizerCards.Values.All(c => c.Picture.ActualWidth <= c.Root.ActualWidth), "thumbnail fit at scale " + scale); }
            previewScale = 1; ArrangeOrganizerCards(pageLayout, false); pageGrid.ChangeView(null, 0, null, true);
            await ExecuteOrganizerAsync(); await Settle();
            var saved = resultPath!;
            Check(File.Exists(saved) && !RestartLayoutChanged(), "actual worker validates new saved result and checkpoint");
            var pdf = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(saved));
            Check(pdf.PageCount == many.Length, "saved result reopens with exact composed page count");
            var sourceA = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(a));
            var sourceB = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(b));
            for (var i = 0; i < many.Length; i++)
            {
                using var actual = pdf.GetPage((uint)i);
                if (many[i].Source < 0) { Check(actual.Size.Height > actual.Size.Width, i + ": blank output portrait A4"); continue; }
                using var source = (many[i].Source == 0 ? sourceA : sourceB).GetPage((uint)many[i].Page);
                var rotated = many[i].Rotation % 180 != 0;
                Check(Math.Abs(actual.Size.Width - (rotated ? source.Size.Height : source.Size.Width)) < 1 && Math.Abs(actual.Size.Height - (rotated ? source.Size.Width : source.Size.Height)) < 1, i + ": output origin order and rotation dimensions");
            }
            var savedHash = Hash(saved);
            Check(Descendants(latestResultCard!).OfType<Image>().Any(x => x.Name == "SavedOrganizerBiblo"), "existing fitted saved-result uses organizer031");
            Button ResultButton(string label) => Descendants(latestResultCard!).OfType<Button>().Single(x => AutomationProperties.GetName(x) == label);
            Check(ResultButton("Tampilkan di folder").IsEnabled, "exact-file reveal uses existing shared result action");
            await Capture("saved-result");
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter resultNormal) resultNormal.Restore();
            foreach (var width in new[] { 660, 1400 })
            {
                AppWindow.Resize(new Windows.Graphics.SizeInt32(width, width == 660 ? 810 : 1000)); await Settle();
                Check(standardPageScroll!.ScrollableHeight <= 1, width + ": organizer saved result fits without page scroll");
                foreach (var button in Descendants(latestResultCard!).OfType<Button>())
                {
                    var bounds = button.TransformToVisual(root).TransformBounds(new Windows.Foundation.Rect(0, 0, button.ActualWidth, button.ActualHeight));
                    Check(bounds.Left >= 0 && bounds.Right <= root.ActualWidth + 1 && bounds.Bottom <= root.ActualHeight + 1 && button.Focus(FocusState.Keyboard), width + ": result action visible and keyboard reachable " + AutomationProperties.GetName(button));
                }
                await Capture("saved-result-" + width);
            }
            var printOpened = false;
            auditPrintPrinters = () => [];
            auditPrintDialog = async dialog => { await Task.Delay(200); printOpened = Descendants(dialog).OfType<TextBlock>().Any(x => x.Text == Path.GetFileName(saved)); dialog.Hide(); };
            Invoke(ResultButton("Cetak hasil"));
            for (var i = 0; i < 80 && (!printOpened || messageOpen); i++) await Task.Delay(100);
            Check(printOpened, "existing print preview receives exact result; no printers or submission"); auditPrintPrinters = null; auditPrintDialog = null;
            Invoke(ResultButton("Buka hasil"));
            // The reader shell becomes visible before its asynchronous file load finishes.
            for (var i = 0; i < 80 && (pdfReader?.Visible != true || pdfReader.ReaderSource != saved || pdfReader.TransitionBusy); i++) await Task.Delay(100);
            Check(pdfReader?.Visible == true && pdfReader.ReaderSource == saved && !pdfReader.TransitionBusy,
                "existing internal reader opens exact organizer output");
            ShowOrganizer(); await Settle();
            // Exercise the actual result Back callback, which restores existing editor controls.
            await ExecuteOrganizerAsync(); await Settle(); Invoke(ResultButton("←  Kembali edit")); await Settle();
            Check(organizerVisible && breadcrumb.Text == "Ruang kerja  /  Atur halaman PDF", "result Back restores organizer breadcrumb and editor");
            await AcceptOrganizerFilesAsync([await StorageFile.GetFileFromPathAsync(Path.Combine(output, "invalid.txt"))]); await Settle();
            Check(Descendants(organizerCanvas!).OfType<InfoBar>().Single(n => n.Name == "OrganizerInputNotice") is { IsOpen: true, Visibility: Visibility.Visible, ActualHeight: > 0 } restoredNotice
                && restoredNotice.Message.StartsWith("1 file ditolak", StringComparison.Ordinal), "QA-02 rejection remains visible after result Back reattaches editor");
            await Capture("rejection-after-result-back");
            var previousGridHeight = pageGrid.MaxHeight;
            AppWindow.Resize(new Windows.Graphics.SizeInt32(660, 810)); await Settle();
            Check(pageGrid.MaxHeight < previousGridHeight, "restored editor reattaches responsive resize handler");
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1400, 1000)); await Settle();
            ActOnOrganizerCard(pageLayout[0].Id, "rotate");
            await AuditOrganizerReturnKeysAsync(output, Check);
            password.Password = "synthetic"; var beforeFailure = pageLayout.ToArray(); var originalFolder = folder; folder = Path.Combine(output, "missing-folder");
            var failed = CloseDocumentsAsync(); await Choose("MergeCloseSave"); await failed;
            Check(pageLayout.SequenceEqual(beforeFailure) && password.Password == "synthetic" && organizerVisible && !organizerCloseSaveInProgress, "failed close-save retains arrangement password and guards");
            folder = originalFolder; outputName.Text = "Cancelled.pdf";
            var cancelled = CloseDocumentsAsync(); await Choose("MergeCloseSave");
            for (var i = 0; i < 100 && active is null && !cancelled.IsCompleted; i++) await Task.Delay(1);
            Check(active is not null, "actual close-save exposes cancellation"); active!.Cancel(); await cancelled;
            Check(pageLayout.SequenceEqual(beforeFailure) && password.Password == "synthetic" && !File.Exists(Path.Combine(output, "Cancelled.pdf")), "cancelled save retains draft and does not publish result");
            password.Password = ""; outputName.Text = "Closed-saved.pdf";
            var closeSave = CloseDocumentsAsync(); await Choose("MergeCloseSave"); await closeSave; await Settle();
            Check(pageLayout.Count == 0 && organizerSources.Count == 0 && emptyTool?.Id == "organize" && PdfEmptyActive && !RestartLayoutChanged(), "successful close-save clears only organizer session and opens reused landing");
            var notice = (MergeClosedNoticeState)body.Children.OfType<Border>().Single(x => x.Tag is MergeClosedNoticeState).Tag;
            Check(File.Exists(notice.SavedPath) && Path.GetFileName(notice.SavedPath) == "Closed-saved.pdf", "saved landing notice captures exact new output");
            var opened = false; auditReadDialog = p => { opened = p.ReaderSource == notice.SavedPath; return Task.CompletedTask; };
            await notice.OpenAsync(); auditReadDialog = null; Check(opened, "landing Buka hasil targets immutable saved path");
            AppWindow.Resize(new Windows.Graphics.SizeInt32(660, 810)); await Settle(); await Capture("closed-saved-landing-minimum");
            Check(PdfEmptyActive && Descendants(emptyPanel!).OfType<Button>().Count() == 2, "minimum organizer landing retains both native Merge input actions");
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1400, 1000)); await Settle(); await Capture("closed-saved-landing");
            await Open(); var discard = CloseDocumentsAsync(); await Choose("MergeCloseDiscard"); await discard; await Settle();
            Check(PdfEmptyActive && pageLayout.Count == 0 && !body.Children.OfType<Border>().Any(x => x.Tag is MergeClosedNoticeState), "discard returns to landing without success claim");
            await Open(); ExcludeOrganizerSource(0); await Settle();
            Check(pageLayout.All(p => p.Source == 1) && organizerCards.Values.All(c => c.Origin.Text.StartsWith("B ·", StringComparison.Ordinal)), "excluding A never renumbers B");
            RestoreLayout(false); Check(pageLayout.Any(p => p.Source == 0), "Undo restores excluded source with stable code");
            Check(new[] { Hash(a), Hash(b) }.SequenceEqual(hashes) && Hash(saved) == savedHash, "all original and saved source hashes unchanged");
            openingEmptyInput = true;
            try { ShowTool(ToolCatalog.All.First(t => t.Id == "merge")); }
            finally { openingEmptyInput = false; }
            await Settle();
            Check(toolOptions.Spacing == 16 && outputName.Header is string header && header == "Nama file hasil", "organizer compact presentation does not leak into Merge");
            Check(!run.Resources.ThemeDictionaries.ContainsKey("Default")
                || !((ResourceDictionary)run.Resources.ThemeDictionaries["Default"]).ContainsKey("AccentButtonBackground"), "Organizer blue button resources do not leak into Merge");
            ShowOrganizer(); await Settle();
            await TestOrganizerRevisionAsync(a, output);
            Check(Environment.ExitCode == 0, "existing organizer identity drag scroll regression passes");
            if (interactive)
            {
                AppWindow.Title = "OTO · Organizer isolated developer audit";
                pageLayout.Clear(); pageLayout.AddRange(many); RefreshPageGrid();
                AppWindow.Resize(new Windows.Graphics.SizeInt32(1400, 1000));
                report.Add("INTERACTIVE synthetic isolated candidate retained for native UI inspection.");
            }
            report.Add("LIMIT current monitor only. No physical clipboard mutation, print jobs, live high contrast/DPI/multimonitor or network-disabled human UAT. Reveal handler reviewed; Explorer selection not asserted here.");
        }
        catch (Exception error) { report.Add("FAIL " + error); Environment.ExitCode = 1; try { await Capture("failure"); } catch (Exception) { } }
        finally { auditPrintPrinters = null; auditPrintDialog = null; auditReadDialog = null; File.WriteAllLines(Path.Combine(output, "audit.txt"), report); }
    }

    // Integration evidence: real ShowAsync, dispatcher, buttons and production callbacks.
    // This does not synthesize OS keyboard routing or claim physical held-key coverage.
    private async Task AuditOrganizerReturnKeysAsync(string output, Action<bool, string> check)
    {
        var trace = new List<string> { "CLASS native programmatic event-sequence integration; not physical hold/OS routing" };
        var tracePath = Path.Combine(output, "organizer-return-keys.txt");
        void Trace(string value) { trace.Add(value); File.WriteAllLines(tracePath, trace); }
        void Check(bool ok, string label) { Trace((ok ? "PASS " : "FAIL ") + label); check(ok, label); }
        async Task Barrier()
        {
            var ready = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!DispatcherQueue.TryEnqueue(() => ready.TrySetResult(true))) throw new InvalidOperationException("OUI07 dispatcher unavailable");
            await ready.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
        static bool SameStack(Stack<OrganizerPage[]> stack, OrganizerPage[][] snapshot) =>
            stack.Count == snapshot.Length && stack.Zip(snapshot).All(pair => pair.First.SequenceEqual(pair.Second));
        static void RestoreStack(Stack<OrganizerPage[]> stack, OrganizerPage[][] snapshot)
        {
            stack.Clear(); foreach (var item in snapshot.Reverse()) stack.Push(item);
        }
        Dictionary<string, string> PdfHashes() => Directory.EnumerateFiles(output, "*.pdf", SearchOption.AllDirectories)
            .ToDictionary(path => path, path => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), StringComparer.OrdinalIgnoreCase);
        var originalSecret = password.Password;
        var originalUndo = layoutUndo.Select(entry => entry.ToArray()).ToArray();
        var originalRedo = layoutRedo.Select(entry => entry.ToArray()).ToArray();
        MergeCloseDialogView? liveView = null;
        Task? liveClose = null;
        try
        {
            // Keep a meaningful redo history without changing the caller's arrangement.
            ActOnOrganizerCard(pageLayout[0].Id, "rotate"); RestoreLayout(false);
            password.Password = "oui07-synthetic-memory-only";
            Check(layoutUndo.Count > 0 && layoutRedo.Count > 0 && pageLayout.Any(p => p.Rotation != 0)
                && resultPath is not null && File.Exists(resultPath) && restartSavedLayout is not null,
                "OUI07 fixture has undo redo rotation saved PDF and checkpoint");
            var cases = new[] { "K01", "K02", "K03", "K04", "K05-one", "K05-two", "K06-one", "K06-two", "K07-other", "K07-same", "K08", "K09", "K10" };
            foreach (var id in cases)
            {
                var prefix = "OUI07-" + id;
                var layout = pageLayout.ToArray(); var sources = organizerSources.ToArray();
                var undo = layoutUndo.Select(entry => entry.ToArray()).ToArray();
                var redo = layoutRedo.Select(entry => entry.ToArray()).ToArray();
                var secret = password.Password; var tool = selected; var name = outputName.Text;
                var destination = folder; var result = resultPath; var checkpoint = restartSavedLayout;
                var pdfs = PdfHashes(); var closedCount = 0; var openedCount = 0;
                var downCalls = 0; var upCalls = 0;
                var opened = new TaskCompletionSource<MergeCloseDialogView>(TaskCreationOptions.RunContinuationsAsynchronously);
                auditOrganizerCloseOpened = view =>
                {
                    openedCount++; liveView = view;
                    view.Dialog.Closed += (_, _) => closedCount++;
                    opened.TrySetResult(view);
                };
                liveClose = CloseDocumentsAsync();
                var current = await opened.Task.WaitAsync(TimeSpan.FromSeconds(5));
                await Barrier();
                bool PopupOpen() => VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot)
                    .Any(p => Descendants(p.Child).Prepend(p.Child).Any(child => ReferenceEquals(child, current.Dialog)));
                void Retained(string stage)
                {
                    Check(pageLayout.SequenceEqual(layout) && organizerSources.SequenceEqual(sources), prefix + " " + stage + " layout IDs sources pages order rotation retained");
                    Check(SameStack(layoutUndo, undo) && SameStack(layoutRedo, redo), prefix + " " + stage + " both complete history stacks retained");
                    Check(password.Password == secret && ReferenceEquals(selected, tool) && organizerVisible
                        && outputName.Text == name && folder == destination && resultPath == result && restartSavedLayout == checkpoint,
                        prefix + " " + stage + " secret tool output and saved checkpoint retained");
                    var now = PdfHashes();
                    Check(now.Count == pdfs.Count && pdfs.All(pair => now.TryGetValue(pair.Key, out var value) && value == pair.Value)
                        && active is null && !organizerCloseSaveInProgress && !mergeCloseSaveInProgress,
                        prefix + " " + stage + " PDF file set hashes retained and no save worker");
                }
                async Task Held(string stage)
                {
                    await Barrier(); await Task.Delay(250); await Barrier();
                    Check(PopupOpen() && !liveClose.IsCompleted && closedCount == 0 && messageOpen,
                        prefix + " " + stage + " dialog remains open with lifecycle guard");
                    Retained(stage);
                }
                void Key(bool down, VirtualKey key, bool expected = true)
                {
                    var callback = down ? current.OrganizerKeyDown : current.OrganizerKeyUp;
                    if (callback is null) throw new InvalidOperationException(prefix + " callback unavailable before event");
                    var handled = callback(key);
                    if (down) downCalls++; else upCalls++;
                    Trace(prefix + " " + (down ? "down " : "up ") + key + " handled=" + handled);
                    Check(handled == expected, prefix + " callback consumption " + (down ? "down " : "up ") + key);
                }
                void OpeningTrace(string stage) => Trace(prefix + " " + stage + " opened=" + openedCount
                    + " popup=" + PopupOpen() + " completed=" + liveClose.IsCompleted + " guard=" + messageOpen
                    + " default=" + current.Dialog.DefaultButton + " returnFocus=" + ReferenceEquals(FocusManager.GetFocusedElement(root.XamlRoot), current.Return)
                    + " focusName=" + (FocusManager.GetFocusedElement(root.XamlRoot) as FrameworkElement)?.Name);
                OpeningTrace("opened barrier");
                // Match the existing native Popup helper's settle before checking focus.
                root.UpdateLayout(); await Task.Delay(250); root.UpdateLayout(); await Barrier();
                OpeningTrace("settled");
                Check(openedCount == 1 && PopupOpen() && !liveClose.IsCompleted && messageOpen
                    && current.Dialog.DefaultButton == ContentDialogButton.None
                    && ReferenceEquals(FocusManager.GetFocusedElement(root.XamlRoot), current.Return),
                    prefix + " actual lifecycle ShowAsync opened once with safe focus");
                switch (id)
                {
                    case "K01":
                    case "K02":
                        var key = id == "K01" ? VirtualKey.Enter : VirtualKey.Escape;
                        Key(true, key); Key(true, key); Key(true, key); Key(true, key);
                        await Held("repeated down"); Key(false, key); break;
                    case "K03":
                    case "K04":
                        Key(true, VirtualKey.Enter); Key(true, VirtualKey.Escape);
                        var first = id == "K03" ? VirtualKey.Enter : VirtualKey.Escape;
                        var last = id == "K03" ? VirtualKey.Escape : VirtualKey.Enter;
                        Key(false, first); await Held("first release"); Key(false, last); break;
                    case "K05-one":
                    case "K05-two":
                    case "K06-one":
                    case "K06-two":
                        Key(true, VirtualKey.Enter);
                        if (id.EndsWith("two", StringComparison.Ordinal)) Key(true, VirtualKey.Escape);
                        Invoke(id.StartsWith("K05", StringComparison.Ordinal) ? current.Save : current.Discard);
                        Trace(prefix + " invoke actual " + (id.StartsWith("K05", StringComparison.Ordinal) ? "Save" : "Discard"));
                        await Held("button while held"); Key(false, VirtualKey.Enter);
                        if (id.EndsWith("two", StringComparison.Ordinal))
                        { await Held("button first release"); Key(false, VirtualKey.Escape); }
                        break;
                    case "K07-other":
                    case "K07-same":
                        Key(true, VirtualKey.Enter); Key(false, VirtualKey.Enter);
                        // No await between release and re-press: close is still queued.
                        var again = id == "K07-other" ? VirtualKey.Escape : VirtualKey.Enter;
                        Key(true, again); await Held("queued close suppressed by new down"); Key(false, again); break;
                    case "K08":
                        Key(true, VirtualKey.A, false); Key(false, VirtualKey.A, false);
                        Key(false, VirtualKey.Enter); Key(false, VirtualKey.Escape);
                        await Held("unknown and fresh up without down"); Invoke(current.Return); break;
                    case "K09":
                        Key(true, VirtualKey.Enter); await Held("before duplicate release");
                        Key(false, VirtualKey.Enter); Key(false, VirtualKey.Enter); Key(false, VirtualKey.Escape); break;
                    case "K10":
                        Key(false, VirtualKey.Enter); Key(false, VirtualKey.Escape);
                        await Held("fresh dialog has no previous return request"); Invoke(current.Return); break;
                }
                await Barrier(); await liveClose.WaitAsync(TimeSpan.FromSeconds(5)); await Barrier();
                Check(current.Result == ContentDialogResult.None && closedCount == 1 && !PopupOpen()
                    && !messageOpen && current.OrganizerKeyDown is null && current.OrganizerKeyUp is null,
                    prefix + " exactly one None close completes lifecycle and clears callback references");
                Retained("after close");
                Trace(prefix + " complete downCalls=" + downCalls + " upCalls=" + upCalls + " result=" + current.Result);
                auditOrganizerCloseOpened = null; liveView = null; liveClose = null;
            }
        }
        finally
        {
            auditOrganizerCloseOpened = null;
            if (liveView is not null && liveClose is { IsCompleted: false })
            {
                liveView.Dialog.Hide();
                await liveClose.WaitAsync(TimeSpan.FromSeconds(5));
            }
            password.Password = originalSecret;
            RestoreStack(layoutUndo, originalUndo); RestoreStack(layoutRedo, originalRedo);
            RefreshPageGrid();
            File.WriteAllLines(tracePath, trace);
        }
    }
}
#endif

