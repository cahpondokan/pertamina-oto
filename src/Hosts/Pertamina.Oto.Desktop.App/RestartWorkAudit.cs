#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private async Task AuditDocumentCloseAsync()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "document-close-audit"); Directory.CreateDirectory(directory);
        var report = new List<string>();
        void Check(bool ok, string label) { report.Add((ok ? "PASS " : "FAIL ") + label); if (!ok) throw new InvalidOperationException(label); }
        async Task Settle() { root.UpdateLayout(); await Task.Delay(250); root.UpdateLayout(); }
        async Task Capture(Microsoft.UI.Xaml.UIElement element, string name)
        {
            var bitmap = new Microsoft.UI.Xaml.Media.Imaging.RenderTargetBitmap(); await bitmap.RenderAsync(element);
            var pixels = await bitmap.GetPixelsAsync();
            var file = await (await Windows.Storage.StorageFolder.GetFolderFromPathAsync(directory)).CreateFileAsync(name + ".png", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(Windows.Graphics.Imaging.BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8, Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied,
                (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, pixels.ToArray()); await encoder.FlushAsync();
        }
        Microsoft.UI.Xaml.Controls.ContentDialog Popup() => Microsoft.UI.Xaml.Media.VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot)
            .SelectMany(p => Descendants(p.Child).Prepend(p.Child)).OfType<Microsoft.UI.Xaml.Controls.ContentDialog>().First();
        void Invoke(Microsoft.UI.Xaml.Controls.ContentDialog dialog, string name)
        {
            var button = Descendants(dialog).OfType<Microsoft.UI.Xaml.Controls.Button>().Single(b => b.Name == name);
            ((Microsoft.UI.Xaml.Automation.Provider.IInvokeProvider)new Microsoft.UI.Xaml.Automation.Peers.ButtonAutomationPeer(button)
                .GetPattern(Microsoft.UI.Xaml.Automation.Peers.PatternInterface.Invoke)).Invoke();
        }
        try
        {
            var source = Path.Combine(directory, "source.pdf"); WriteHistoryAuditFixture(source);
            var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(source)));
            folder = directory; openingEmptyInput = true;
            try { ShowTool(ToolCatalog.All.First(t => t.Id == "watermark")); } finally { openingEmptyInput = false; }
            files.Add(source); watermark.Text = "UNSAVED"; outputName.Text = "custom.pdf"; await Settle();
            Check(RestartHasSession, "real source/options guarded before close");
            var close = CloseDocumentsAsync(); await Settle(); var dialog = Popup();
            Check(dialog.DefaultButton == Microsoft.UI.Xaml.Controls.ContentDialogButton.Close, "close defaults to retain");
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter) presenter.Restore();
            foreach (var width in new[] { 1440, 640 })
            {
                AppWindow.Resize(new Windows.Graphics.SizeInt32(width, 760)); await Settle();
                report.Add("VIEW root DIP " + root.ActualWidth.ToString(System.Globalization.CultureInfo.InvariantCulture) + "x" + root.ActualHeight.ToString(System.Globalization.CultureInfo.InvariantCulture));
                var panel = Descendants(dialog).OfType<Microsoft.UI.Xaml.FrameworkElement>().First(e => e.Name == "BackgroundElement");
                Check(panel.ActualWidth <= root.ActualWidth && panel.ActualHeight <= root.ActualHeight, "native close dialog fits viewport " + width);
                Check(dialog.PrimaryButtonText == "Proses dan simpan" && dialog.SecondaryButtonText == "Tutup tanpa menyimpan", "visible explicit save/discard labels " + width);
                await Capture(panel, "close-" + width);
            }
            Invoke(dialog, "CloseButton"); await close;
            Check(files.SequenceEqual(new[] { source }) && watermark.Text == "UNSAVED" && RestartHasSession, "cancel preserves exact draft");
            close = CloseDocumentsAsync(); await Settle(); Invoke(Popup(), "SecondaryButton"); await close; await Settle();
            report.Add("STATE after-close " + string.Join(",", RestartPendingWork().Select(w => w.Name + ":" + w.Tool)));
            Check(!RestartHasSession, "explicit close plus discard removes pending work on empty screen");
            ShowSettings(); await Settle(); Check(!RestartHasSession, "closed work stays absent after settings navigation");
            Travel(-1); await Settle(); Check(!RestartHasSession && files.Count == 0, "history cannot resurrect closed work");
            async Task OpenTool(string id)
            {
                openingEmptyInput = true;
                try { ShowTool(ToolCatalog.All.First(t => t.Id == id)); } finally { openingEmptyInput = false; }
                await Settle();
            }
            async Task CloseTool(string action = "SecondaryButton")
            {
                var task = CloseDocumentsAsync(); await Settle();
                if (!task.IsCompleted) { Invoke(Popup(), action); }
                await task; await Settle();
            }
            await OpenTool("watermark"); files.Add(source); await Settle();
            Check(!RestartHasSession && watermark.Text.Length == 0, "fresh source after close has no inherited watermark/options");
            watermark.Text = "SAVE"; outputName.Text = "saved.pdf";
            await CloseTool("PrimaryButton");
            var savedFile = Directory.GetFiles(directory, "saved*.pdf").Single();
            Check(File.Exists(savedFile) && !RestartHasSession && files.Count == 0, "save from close dialog succeeds before release");
            var savedHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(savedFile)));
            await OpenTool("watermark"); files.Add(Path.Combine(directory, "missing.pdf")); watermark.Text = "FAILED"; password.Password = "synthetic-only-secret";
            await CloseTool("PrimaryButton");
            Check(files.Count == 1 && RestartHasSession && password.Password.Length > 0, "failed save from close dialog retains draft/secret and guard");
            await CloseTool();
            await OpenTool("watermark"); files.Add(source); watermark.Text = "CANCEL"; outputName.Text = "cancel.pdf"; password.Password = "synthetic-only-secret";
            var saving = ExecuteAsync(); active?.Cancel(); await saving;
            Check(RestartHasSession && !restartSavedTools.ContainsKey("watermark") && password.Password.Length > 0, "cancelled actual export retains secret and never establishes saved checkpoint");
            await CloseTool();
            await OpenTool("watermark"); files.Add(source); watermark.Text = "KEEP OTHER";
            ShowEmptyTool(ToolCatalog.All.First(t => t.Id == "compress")); ShowSettings();
            Check(RestartPendingWork().Count == 1 && !toolDrafts.ContainsKey("compress"), "empty tool cannot capture detached options from another tool");
            RestartPendingWork()[0].Review(); await Settle();
            using (var operation = new CancellationTokenSource())
            {
                active = operation; var refusal = CloseDocumentsAsync(); await Settle(); Invoke(Popup(), "CloseButton"); await refusal; active = null;
                Check(files.Count == 1 && watermark.Text == "KEEP OTHER" && RestartHasSession, "busy close refusal preserves active tool work");
            }
            await OpenTool("rotate"); files.Add(source); rotation.SelectedIndex = 1;
            Check(RestartPendingWork().Count == 2 && RestartPendingWork().All(w => w.Name == "source.pdf"), "two actual tool sessions identified independently by filename");
            var work = RestartPendingWork(); var warning = BuildRestartWorkDialog(work, _ => { }); var shown = warning.ShowAsync(); await Settle();
            Check(Descendants(warning).OfType<Microsoft.UI.Xaml.Controls.TextBlock>().Any(t => t.Text == work[0].Reason), "restart rows explain valid pending reason");
            await Capture(Descendants(warning).OfType<Microsoft.UI.Xaml.FrameworkElement>().First(e => e.Name == "BackgroundElement"), "pending-tools"); warning.Hide(); await shown;
            await CloseTool();
            Check(RestartPendingWork().Count == 1 && RestartPendingWork()[0].Tool == "Watermark", "closing one tool preserves other tool draft");
            ShowSettings(); RestartPendingWork()[0].Review(); await Settle();
            Check(watermark.Text == "KEEP OTHER", "pending dialog review restores other tool exactly");
            await CloseTool();
            await OpenTool("merge"); files.Add(source); var second = Path.Combine(directory, "second.pdf"); File.Copy(source, second); files.Add(second); await Settle();
            mergeAngles[source] = 90;
            RemoveToolInput(source); Check(RestartHasSession && files.Count == 1, "remove one input retains composition guard");
            RemoveToolInput(second); Check(RestartHasSession && files.Count == 0, "remove last input retains valid sourceless composition");
            ShowSettings(); Check(RestartHasSession, "sourceless composition remains guarded offscreen");
            RestartPendingWork()[0].Review(); await Settle(); Check(files.Count == 0 && toolWorkspace.IsLoaded, "sourceless pending review opens editable session");
            await CloseTool(); ShowSettings(); Travel(-1); await Settle(); Check(!RestartHasSession && mergeAngles.Count == 0, "closed empty composition clears angles and stays closed through history");
            await OpenOrganizerFilesAsync([source, second]); await Settle();
            ExcludeOrganizerSource(0); Check(RestartLayoutChanged() && layoutUndo.Count > 0, "organizer remove retains undo and pending composition");
            ExcludeOrganizerSource(1); Check(pageLayout.Count == 0 && RestartHasSession, "empty organizer still guarded until explicit close");
            RestoreLayout(false); Check(pageLayout.Count > 0, "organizer remove undo restores pages");
            await CloseTool(); ShowSettings(); Travel(-1); await Settle();
            Check(!RestartHasSession && layoutUndo.Count == 0 && organizerSources.Count == 0, "organizer explicit close clears only its provenance/undo/checkpoint");
            await OpenTool("watermark"); files.Add(second); watermark.Text = "READER DOES NOT DISCARD";
            ShowPdfReader(false); await pdfReader!.OpenAsync(source); await Settle();
            pdfReader.CloseReaderDocument();
            Check(pdfReader.ReaderSource is null && RestartPendingWork().Count == 1, "reader close preserves tool draft and releases only reader");
            ShowSettings(); ShowPdfReader(); await Settle();
            Check(pdfReader.ReaderSource is null, "reader history/auto-open cannot resurrect explicitly closed read session");
            var pendingOpen = pdfReader.OpenAsync(source); pdfReader.CloseReaderDocument(); await pendingOpen;
            Check(pdfReader.ReaderSource is null, "async reader open completed after close cannot reattach source");
            RestartPendingWork()[0].Review(); await Settle(); await CloseTool();
            await OpenTool("watermark"); files.Add(second); watermark.Text = "EDITOR OTHER TOOL";
            ShowLiveEditor(true); await liveEditor!.OpenAsync(source); await Settle();
            await liveEditor.AuditDocumentCloseEditorAsync(directory, Check, Settle, Popup, Invoke);
            Check(RestartPendingWork().Count == 1 && RestartPendingWork()[0].Tool == "Watermark", "closed editor preserves other document/tool draft and removes only editor work");
            RestartPendingWork()[0].Review(); await Settle(); await CloseTool();
            Check(!RestartHasSession, "closing remaining tool releases final pending work");
            await OpenTool("watermark"); files.Add(source); await Settle();
            var closeButton = Descendants(body).OfType<Microsoft.UI.Xaml.Controls.Button>().First(b => Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(b) == "Tutup dokumen");
            ((Microsoft.UI.Xaml.Automation.Provider.IInvokeProvider)new Microsoft.UI.Xaml.Automation.Peers.ButtonAutomationPeer(closeButton)
                .GetPattern(Microsoft.UI.Xaml.Automation.Peers.PatternInterface.Invoke)).Invoke(); await Settle();
            Check(files.Count == 0 && !RestartHasSession, "native toolbar close button follows session contract for pristine document");
            await OpenTool("watermark"); files.Add(source); await Settle();
            var removeButton = Descendants(body).OfType<Microsoft.UI.Xaml.Controls.Button>().First(b => Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(b) == "Keluarkan dari daftar");
            ((Microsoft.UI.Xaml.Automation.Provider.IInvokeProvider)new Microsoft.UI.Xaml.Automation.Peers.ButtonAutomationPeer(removeButton)
                .GetPattern(Microsoft.UI.Xaml.Automation.Peers.PatternInterface.Invoke)).Invoke(); await Settle();
            Check(files.Count == 0 && RestartHasSession, "native source-list remove action retains sourceless draft"); await CloseTool();
            await OpenTool("pdf-jpg"); files.Add(source); await Settle();
            removeButton = Descendants(body).OfType<Microsoft.UI.Xaml.Controls.Button>().First(b => Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(b) == "Keluarkan PDF dari daftar, bukan hapus file");
            ((Microsoft.UI.Xaml.Automation.Provider.IInvokeProvider)new Microsoft.UI.Xaml.Automation.Peers.ButtonAutomationPeer(removeButton)
                .GetPattern(Microsoft.UI.Xaml.Automation.Peers.PatternInterface.Invoke)).Invoke(); await Settle();
            Check(files.Count == 0 && RestartHasSession, "native JPG input remove action retains draft"); await CloseTool();
            await OpenTool("unlock"); files.Add(source); password.Password = "synthetic-only-secret";
            await OpenTool("rotate"); files.Add(second); rotation.SelectedIndex = 1; await CloseTool();
            Check(RestartPendingWork().Count == 1 && toolSessionSecrets.ContainsKey("unlock"), "password-only tool draft stays guarded across tools and other session close");
            RestartPendingWork()[0].Review(); await Settle();
            Check(password.Password.Length > 0 && !RestartToolState("unlock", CurrentToolDraft()).Contains(password.Password, StringComparison.Ordinal), "review retains session credential without serializing it");
            await CloseTool(); Check(!RestartHasSession && toolSessionSecrets.Count == 0, "discard releases only ended session credential");
            Check(savedHash == Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(savedFile))), "saved output preserved byte-for-byte");
            var afterHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(source)));
            Check(hash == afterHash, "original source hash unchanged");
        }
        catch (Exception error) { report.Add("ERROR " + error.GetType().Name + ": " + error.Message); Environment.ExitCode = 1; }
        File.WriteAllLines(Path.Combine(directory, "audit.txt"), report);
    }

    private sealed partial class LiveEditor
    {
        internal async Task AuditDocumentCloseEditorAsync(string directory, Action<bool, string> check, Func<Task> settle,
            Func<Microsoft.UI.Xaml.Controls.ContentDialog> popup, Action<Microsoft.UI.Xaml.Controls.ContentDialog, string> invoke)
        {
            Add(new(0, "text", 40, 120, 220, 32, "DRAFT", 14)); StartInlineText(chosen);
            inlineText!.Text = "INLINE WITHOUT ENTER"; await settle(); var exact = State; var undoCount = past.Count;
            var closing = CloseAsync(); await settle(); invoke(popup(), "CloseButton"); await closing;
            check(Dirty && State == exact && past.Count == undoCount, "editor cancelled close preserves inline text and undo | dirty=" + Dirty + " equal=" + (State == exact) + " undo=" + past.Count + "/" + undoCount + " working=" + Working);
            await SaveAsync(directory); check(!Dirty && lastResult is not null, "editor verified save establishes clean checkpoint");
            Restore(false); check(Dirty && RestartHasSession, "editor undo after save remains guarded");
            Restore(true); check(!Dirty && !RestartHasSession, "editor redo to saved state becomes clean");
            drawingShape = true;
            closing = CloseAsync(); await settle(); invoke(popup(), "CloseButton"); await closing;
            check(drawingShape && RestartHasSession, "cancelled close preserves shape gesture"); drawingShape = false;
            dragging = marks[0];
            closing = CloseAsync(); await settle(); invoke(popup(), "CloseButton"); await closing;
            check(dragging is not null && RestartHasSession, "cancelled close preserves drag gesture"); dragging = null;
            drawing = true; stroke.Add(new(10, 10));
            closing = CloseAsync(); await settle(); invoke(popup(), "CloseButton"); await closing;
            check(drawing && stroke.Count == 1 && RestartHasSession, "cancelled close protects in-progress drawing");
            closing = CloseAsync(); await settle(); invoke(popup(), "SecondaryButton"); await closing; await settle();
            check(pdf is null && sourceLock is null && !Dirty && !drawing && past.Count == 0 && lastResult is null,
                "editor discard releases lock, gesture, inline, undo and result association");
            owner.ShowLiveEditor(true);
            var opening = OpenAsync(Path.Combine(directory, "source.pdf"));
            check(Working, "actual editor open is observed in flight"); await CloseAsync(); await opening;
            check(path is not null && pdf is not null && sourceLock is not null, "close during async editor operation refuses release and retains loaded session");
            await CloseAsync(); check(path is null && !RestartHasSession, "pristine editor closes safely after async operation finishes");
        }
    }

    private async Task AuditRestartSavedToolsAsync(string source, string directory, Action<bool, string> check)
    {
        folder = directory; openingEmptyInput = true;
        try { ShowTool(ToolCatalog.All.First(t => t.Id == "watermark")); }
        finally { openingEmptyInput = false; }
        files.Add(source); watermark.Text = "TEST"; outputName.Text = "restart-watermark.pdf";
        check(RestartHasSession, "actual watermark tool starts protected");
        await ExecuteAsync();
        check(resultPath is not null && File.Exists(resultPath) && !RestartHasSession, "actual successful tool export clears warning with source still selected");
        watermark.Text = "SECOND"; check(RestartHasSession, "actual tool change after export is protected");
        ShowSettings();
        check(RestartHasSession, "actual tool change remains protected after navigation to settings");
        RestartPendingWork().Single().Review();
        check(watermark.Text == "SECOND" && files.SequenceEqual(new[] { source }), "review restores exact tool draft without processing it");
        var before = restartSavedTools["watermark"];
        files[0] = Path.Combine(directory, "nonexistent-input.pdf");
        await ExecuteAsync();
        check(RestartHasSession && restartSavedTools["watermark"] == before, "failed tool export never marks current work saved");
        toolDrafts.Clear(); restartSavedTools.Clear(); files.Clear(); ShowSettings(); toolDrafts.Clear();
        await OpenOrganizerFilesAsync([source]);
        check(!RestartLayoutChanged(), "actual pristine organizer import does not trigger warning");
        pageLayout[0] = pageLayout[0] with { Rotation = 90 };
        check(RestartLayoutChanged(), "actual organizer rotation triggers warning");
        outputName.Text = "restart-organizer.pdf"; await ExecuteOrganizerAsync();
        check(resultPath is not null && File.Exists(resultPath) && !RestartLayoutChanged(), "actual organizer save checkpoint remains clean on result screen");
        ShowSettings(); check(!RestartLayoutChanged(), "saved organizer checkpoint remains clean after navigation");
        pageLayout[0] = pageLayout[0] with { Rotation = 180 }; check(RestartLayoutChanged(), "later organizer edit remains guarded");
        pageLayout.Clear(); organizerSources.Clear(); restartSourcePageCounts.Clear(); restartSavedLayout = null;
        homeFiles.Clear(); chosenFiles.Clear(); files.Clear(); toolDrafts.Clear(); layoutUndo.Clear(); layoutRedo.Clear();
        selected = null; ShowSettings();
    }

    private void AuditRestartWorkStates(Action<bool, string> check)
    {
        foreach (var tool in ToolCatalog.All.Where(t => t.Available && t.Id is not ("edit" or "organize")))
        {
            var draft = new ToolDraft(["fixture.pdf"], "", "", "hasil-" + tool.Id + ".pdf", 0, 20);
            check(!RestartToolChanged(tool.Id, draft), tool.Id + " opening one document alone is not an edit");
            var changed = draft with { Output = "custom-output.pdf" };
            check(RestartToolChanged(tool.Id, changed), tool.Id + " custom output protected");
            restartSavedTools[tool.Id] = RestartToolState(tool.Id, changed);
            check(!RestartToolChanged(tool.Id, changed), tool.Id + " saved checkpoint unblocks restart");
            check(RestartToolChanged(tool.Id, changed with { Output = "another-output.pdf" }), tool.Id + " change after saved checkpoint protected");
            check(RestartToolChanged(tool.Id, draft), tool.Id + " returning to defaults after save still differs from saved output");
        }
        restartSavedTools.Clear();
        toolDrafts["watermark"] = new(["fixture.pdf"], "", "UNSAVED", "hasil-watermark.pdf", 0, 20);
        toolDrafts["rotate"] = new(["fixture.pdf"], "", "", "hasil-rotate.pdf", 1, 20);
        check(RestartPendingWork().Count == 2, "all changed tools reported across navigation");
        check(RestartPendingWork().All(w => w.Name == "fixture.pdf"), "dialog rows use filename only, not sensitive full paths");
        toolDrafts.Clear();
        organizerSources.Add("fixture.pdf"); restartSourcePageCounts["fixture.pdf"] = 2;
        pageLayout.Add(OrganizerPage.Create(0, 0)); pageLayout.Add(OrganizerPage.Create(0, 1));
        organizerOutput = "hasil-organize.pdf";
        check(!RestartLayoutChanged(), "unchanged organizer source is restart-safe");
        pageLayout.Reverse(); check(RestartLayoutChanged(), "reordered organizer protected");
        restartSavedLayout = RestartLayoutState(); check(!RestartLayoutChanged(), "saved organizer permits restart with source still open");
        pageLayout.Reverse(); check(RestartLayoutChanged(), "undo to original order after save is still a change"); pageLayout.Reverse();
        pageLayout[0] = pageLayout[0] with { Rotation = 90 }; check(RestartLayoutChanged(), "rotation after saved organizer protected");
        pageLayout[0] = pageLayout[0] with { Rotation = 0 }; check(!RestartLayoutChanged(), "return to saved organizer state permits restart");
        pageLayout.Clear(); organizerSources.Clear(); restartSourcePageCounts.Clear(); restartSavedLayout = null;
        homeFiles.Add("fixture.pdf"); chosenFiles.Add("fixture.pdf"); homeUndo.Push([]);
        check(!RestartHasSession, "home selection and history do not pretend to be changed document content");
        homeFiles.Clear(); chosenFiles.Clear(); homeUndo.Clear();
    }
}
#endif
