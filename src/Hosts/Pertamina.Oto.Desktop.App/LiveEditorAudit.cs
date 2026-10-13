#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pertamina.Oto.LocalPdf;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    internal async Task AuditLiveEditorAsync(string source)
    {
        ShowLiveEditor(true);
        await liveEditor!.AuditAsync(source);
    }

    private sealed partial class LiveEditor
    {
        internal async Task AuditAsync(string source)
        {
            var directory = System.IO.Path.Combine(AppContext.BaseDirectory, "live-editor-audit-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory); var report = new List<string>();
            void Check(bool ok, string name) { if (!ok) throw new InvalidOperationException(name + " | " + notice.Text); report.Add("PASS " + name); }
            async Task Capture(string name, UIElement? element = null)
            {
                owner.root.UpdateLayout(); await Task.Delay(200);
                var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(element ?? owner.root);
                var pixels = await bitmap.GetPixelsAsync();
                var file = await (await StorageFolder.GetFolderFromPathAsync(directory)).CreateFileAsync(name + ".png", CreationCollisionOption.FailIfExists);
                using var output = await file.OpenAsync(FileAccessMode.ReadWrite);
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, output);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, pixels.ToArray()); await encoder.FlushAsync();
            }
            static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
            try
            {
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(1600, 1000));
                owner.ShowLiveEditor(); await Capture("editor-standard-empty");
                Check(owner.emptyTool?.Id == "edit" && owner.emptyPanel?.IsLoaded == true && !Visible, "empty editor uses shared document intake without editing toolbar");
                owner.ShowLiveEditor(true);
                Check(IsTextPasteTarget(new TextBox()), "global paste preserves native text input target");
                Check(IsTextPasteTarget(new RichEditBox()) && IsTextPasteTarget(new PasswordBox()) && IsTextPasteTarget(new NumberBox()), "global paste recognizes rich text password and number input");
                Check(!IsTextPasteTarget(new Button()) && !IsTextPasteTarget(null), "global paste handles non-text and empty focus");
                await Capture("empty"); Check(!save.IsEnabled, "empty editor cannot save");
                var sourceHash = SHA256.HashData(await File.ReadAllBytesAsync(source));
                owner.ShowLiveEditor();
                await owner.AcceptEmptyInputsAsync([await StorageFile.GetFileFromPathAsync(System.IO.Path.GetFullPath(source))]);
                Check(HasDocument && owner.emptyTool is null && Visible, "shared intake opens editor only after accepting one PDF");
                Check(pdf is not null && pageReady && sheet.Width == 595 && sheet.Height == 842, "actual PDF loaded with authoritative point coordinates");
                Check(modes["Edit teks asli"].IsEnabled && modes["Gambar"].IsEnabled, "capability states accurate");
                if (Environment.GetCommandLineArgs().Contains("--editor-property-race-only"))
                {
                    Add(new(0, "text", 40, 140, 300, 35, "PROPERTY RACE", 16));
                    var inspectorGeneration = propertyGeneration;
                    await Capture("property-race-loaded");
                    Check(propertyGeneration == inspectorGeneration, "initial object selection retains inspector generation");
                    var input = properties.Children.OfType<TextBox>().Single(); input.Text = "PROPERTY UPDATED"; await Task.Delay(200);
                    Check(marks[0].Text == "PROPERTY UPDATED", "loaded inspector text updates selected object");
                    ReleaseFiles(); input.Text = "LATE EVENT AFTER RELEASE"; await Task.Delay(300);
                    Check(marks.Count == 0 && chosen == -1 && propertyGeneration > inspectorGeneration, "late inspector callback rejected after model release");
                    var sourceAfterRace = SHA256.HashData(await File.ReadAllBytesAsync(source));
                    Check(sourceHash.SequenceEqual(sourceAfterRace), "property race preserves source hash");
                    return;
                }
                if (pdf!.PageCount > 1)
                {
                    var zoomFrames = 0; var emptyZoomFrames = 0;
                    void ZoomFrame(object? sender, object e) { zoomFrames++; if (editorImages[0].Source is null) emptyZoomFrames++; }
                    CompositionTarget.Rendering += ZoomFrame;
                    try { viewport.ChangeView(0, 0, .8f, false); await Task.Delay(800); }
                    finally { CompositionTarget.Rendering -= ZoomFrame; }
                    await File.WriteAllTextAsync(System.IO.Path.Combine(directory, "editor-zoom-transition.json"), System.Text.Json.JsonSerializer.Serialize(new { zoomFrames, emptyZoomFrames }));
                    Check(zoomFrames > 0 && emptyZoomFrames == 0, "editor native zoom retains decoded active page throughout sampled transition");
                    var scrollZoom = viewport.ZoomFactor;
                    mode = "Tambah teks"; PlaceObjectAt(new Windows.Foundation.Point(40, 140)); EndInlineText();
                    var scrollDraft = State;
                    viewport.ChangeView(null, editorTops[1] * scrollZoom, null, true); await Task.Delay(1800);
                    Check(page == 1 && pageInput.Value == 2, "native continuous scroll selects next editor page and number");
                    Check(Math.Abs(viewport.ZoomFactor - scrollZoom) < .01, "continuous page change preserves zoom");
                    Check(State == scrollDraft && marks[0].Page == 0, "continuous scroll preserves draft on its original page");
                    mode = "Tambah teks"; PlaceObjectAt(new Windows.Foundation.Point(40, 140)); EndInlineText();
                    Check(marks[^1].Page == 1, "new object after scrolling belongs to active page");
                    Restore(false); Check(marks.Count == 1 && marks[0].Page == 0, "cross-page undo restores prior draft");
                    Restore(true); Check(marks.Count == 2 && marks[^1].Page == 1, "cross-page redo retains page ownership");
                    await RenderAsync(0); Check(page == 0 && Math.Abs(viewport.ZoomFactor - scrollZoom) < .01, "button navigation retains zoom and returns to page");
                    await Capture("continuous-scroll-draft");
                    marks.Clear(); past.Clear(); future.Clear(); chosen = -1; Refresh();
                }
                await AuditFreehandAsync(Check, directory, source, name => Capture(name));
                mode = "Bentuk";
                foreach (var kind in new[] { "rectangle", "ellipse", "line" })
                {
                    shape = kind; var baseline = marks.Count; var historyCount = past.Count;
                    PlaceObjectAt(new Windows.Foundation.Point(100, 180));
                    Check(marks.Count == baseline, "shape placement without drag creates nothing: " + kind);
                    BeginShape(new Windows.Foundation.Point(240, 320)); FinishShape();
                    Check(marks.Count == baseline && past.Count == historyCount, "click without movement creates no shape/history: " + kind);
                    BeginShape(new Windows.Foundation.Point(240, 320)); UpdateShape(new Windows.Foundation.Point(100, 220), false);
                    Check(marks.Count == baseline && shapePreview is { X: 100, Y: 220, Width: 140, Height: 100 }, "reverse drag preview follows pointer: " + kind);
                    if (kind == "ellipse") await Capture("shape-drag-preview");
                    FinishShape(); Check(marks.Count == baseline + 1 && past.Count == historyCount + 1, "release commits one shape and undo entry: " + kind);
                    Restore(false); Check(marks.Count == baseline, "one undo removes drawn shape: " + kind);
                    mode = "Bentuk";
                }
                shape = "line"; BeginShape(new Windows.Foundation.Point(80, 200)); UpdateShape(new Windows.Foundation.Point(240, 200), false);
                Check(shapePreview!.Points![0].Y == shapePreview.Points[1].Y, "horizontal drag stays horizontal"); CancelGesture();
                BeginShape(new Windows.Foundation.Point(80, 200)); UpdateShape(new Windows.Foundation.Point(80, 320), false);
                Check(shapePreview!.Points![0].X == shapePreview.Points[1].X, "vertical drag stays vertical"); CancelGesture();
                Check(marks.Count == 0 && shapePreview is null && !drawingShape, "cancel discards shape preview");
                shape = "ellipse"; BeginShape(new Windows.Foundation.Point(80, 200)); UpdateShape(new Windows.Foundation.Point(230, 280), true);
                Check(shapePreview!.Width == shapePreview.Height, "Shift constrains circle proportions"); CancelGesture();
                mode = "Tambah teks"; PlaceObjectAt(new Windows.Foundation.Point(40, 140));
                await Task.Delay(200);
                var directInput = inlineText!;
                Check(directInput is not null && (FocusManager.GetFocusedElement(View.XamlRoot) as TextBox) == directInput, "place text immediately focuses editable canvas input");
                directInput!.SelectAll(); directInput.SelectedText = "EDITOR NEW TEXT UPDATED";
                await Task.Delay(100);
                Check(marks[0].Text == "EDITOR NEW TEXT UPDATED", "native canvas textbox updates document model");
                Refresh();
                Check(inlineText == directInput && (FocusManager.GetFocusedElement(View.XamlRoot) as TextBox) == directInput, "canvas redraw preserves input identity and typing focus");
                Check(!HandleObjectKey(Windows.System.VirtualKey.Delete) && marks.Count == 1, "Delete during direct typing does not delete object");
                await AuditTextGesturesAsync(Check);
                directInput = inlineText!;
                await Capture("direct-typing");
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(1024, 800)); await Task.Delay(200);
                Check(inlineText == directInput && (FocusManager.GetFocusedElement(View.XamlRoot) as TextBox) == directInput, "direct typing stays available with collapsed inspector");
                await Capture("direct-typing-compact"); owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(1600, 1000));
                await SaveAsync(directory);
                Check(lastResult is not null && File.Exists(lastResult), "directly typed text saves to real PDF");
                var typedResult = await WorkerClient.RunAsync(new("edit-text-check", [lastResult!], System.IO.Path.Combine(directory, "verify-direct.pdf"), Rotation: 0), CancellationToken.None);
                Check(typedResult.TextRuns!.Any(r => r.Text == "EDITOR NEW TEXT UPDATED"), "saved PDF contains exact directly typed text");
                marks.Clear(); past.Clear(); future.Clear(); chosen = -1; savedState = State; BuildProperties(); Refresh();
                Add(new(0, "text", 40, 140, 300, 35, "EDITOR NEW TEXT", 16));
                Check(Dirty && save.IsEnabled, "add text creates saveable draft");
                var inspectorGenerationBeforeLayout = propertyGeneration;
                owner.root.UpdateLayout(); await Task.Delay(200);
                Check(propertyGeneration == inspectorGenerationBeforeLayout, "initial object selection does not recreate inspector");
                var text = properties.Children.OfType<TextBox>().Single(); text.Text = "EDITOR NEW TEXT UPDATED";
                await Task.Delay(200);
                Check(marks[0].Text == "EDITOR NEW TEXT UPDATED", "property textbox updates document model");
                Invoke(undo); await Task.Delay(200); Check(marks[0].Text == "EDITOR NEW TEXT", "undo property edit via UI automation");
                Invoke(redo); await Task.Delay(200); Check(marks[0].Text.EndsWith("UPDATED", StringComparison.Ordinal), "redo property edit via UI automation");
                chosen = 0; BuildProperties(); Refresh();
                Check(sheet.Children.OfType<HandleBorder>().Count() == 8, "eight visible object resize handles");
                properties.Children.OfType<TextBox>().Single().Focus(FocusState.Programmatic);
                Check(!HandleObjectKey(Windows.System.VirtualKey.Delete) && marks.Count == 1, "Delete while typing does not remove object");
                Check(viewport.Focus(FocusState.Programmatic) && !Typing(), "canvas accepts keyboard focus");
                Check(HandleObjectKey(Windows.System.VirtualKey.D, control: true) && marks.Count == 2, "Ctrl+D duplicates selected object");
                Check(HandleObjectKey(Windows.System.VirtualKey.Delete) && marks.Count == 1, "Delete removes selected object");
                Restore(false); Check(marks.Count == 2, "undo restores deleted object"); Restore(true); Check(marks.Count == 1, "redo removes restored object");
                chosen = 0; viewport.Focus(FocusState.Programmatic); var oldX = marks[0].X;
                Check(HandleObjectKey(Windows.System.VirtualKey.Right, shift: true) && marks[0].X == oldX + 10, "Shift+arrow moves object ten points"); Restore(false); chosen = 0;
                foreach (var handle in new[] { "nw", "n", "ne", "e", "se", "s", "sw", "w" })
                {
                    var resized = ResizeObject(marks[0], handle, 25, 15, false, sheet.Width, sheet.Height);
                    Check(resized.X >= 0 && resized.Y >= 0 && resized.X + resized.Width <= sheet.Width && resized.Y + resized.Height <= sheet.Height && resized != marks[0], "resize anchor " + handle);
                }
                var proportional = ResizeObject(marks[0], "se", 100, 40, true, sheet.Width, sheet.Height);
                Check(Math.Abs(proportional.Width / proportional.Height - marks[0].Width / marks[0].Height) < .001, "corner ratio constraint");
                Add(new(0, "rectangle", 40, 200, 180, 80, Size: 3, Color: "#1668D4"));
                Add(new(0, "ellipse", 250, 200, 120, 80, Size: 3));
                Add(new(0, "line", 40, 340, 200, 40, Size: 2));
                Add(new(0, "pen", 40, 430, 160, 40, Size: 3, Points: [new(0, 30), new(40, 0), new(80, 35), new(160, 10)]));
                var imagePath = System.IO.Path.Combine(directory, "synthetic-image.png");
                var imageFile = await (await StorageFolder.GetFolderFromPathAsync(directory)).CreateFileAsync("synthetic-image.png");
                using (var imageStream = await imageFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, imageStream);
                    var pixels = Enumerable.Range(0, 64 * 32).SelectMany(i => new byte[] { (byte)(i % 64 < 32 ? 212 : 80), 104, 23, 255 }).ToArray();
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, 64, 32, 96, 96, pixels); await encoder.FlushAsync();
                }
                await PickImageAsync(imagePath); Check(images.Count == 1 && marks[^1].Kind == "image", "real PNG decoded and placed");
                marks[^1] = marks[^1] with { X = 300, Y = 520, Width = 200, Height = 100 }; Refresh();
                await RenderAsync(1); Check(marks.Count == 6 && page == 1, "page navigation preserves draft");
                await RenderAsync(0); Check(page == 0 && marks.Count == 6, "return to edited page");
                var state = State; owner.ShowHome(); owner.ShowLiveEditor(); Check(State == state, "navigation away and return preserves draft");
                chosen = 0; BuildProperties(); Refresh(); await Capture("desktop");
                chosen = -1; Refresh(); viewport.ChangeView(0, 0, 1, true); await Capture("canvas-before", sheet);
                await SaveAsync(directory); Check(!Dirty && lastResult is not null && File.Exists(lastResult), "save publishes new PDF and clears dirty state");
                var firstOutput = lastResult!; await SaveAsync(directory); Check(lastResult != firstOutput && File.Exists(firstOutput), "repeat save preserves existing output with unique name");
                using (var cancelScreen = new CancellationTokenSource())
                {
                    ShowSaving(cancelScreen); await Capture("editor-saving");
                    Check(saveScreen is not null && workspace.Visibility == Visibility.Collapsed, "save progress replaces editor controls");
                    Invoke(((StackPanel)saveScreen!.Content).Children.OfType<Button>().Single());
                    Check(cancelScreen.IsCancellationRequested, "progress cancel requests worker cancellation");
                    HideSaveScreen(); Check(workspace.Visibility == Visibility.Visible, "cancelled progress restores editor");
                }
                var resultState = State; var savedHistoryCount = owner.savedJobs.Count;
                ShowSaved(firstOutput, "2 halaman · hasil audit"); await Capture("editor-saved");
                Check(saveScreen is not null && owner.savedJobs.Count == savedHistoryCount, "shared editor result avoids duplicate history entry");
                Check(!HandleObjectKey(Windows.System.VirtualKey.Delete) && State == resultState, "result screen blocks hidden draft keyboard edits");
                HideSaveScreen(); Check(State == resultState && workspace.Visibility == Visibility.Visible, "return from result retains editable draft");
                var priorFolder = owner.folder; var priorDefault = owner.defaultFolder;
                try
                {
                    var defaults = System.IO.Path.Combine(directory, "default-results"); Directory.CreateDirectory(defaults);
                    var session = System.IO.Path.Combine(directory, "session-results"); Directory.CreateDirectory(session);
                    owner.folder = null; owner.defaultFolder = defaults;
                    await SaveAsync(recordHistory: false);
                    Check(lastResult is not null && System.IO.Path.GetDirectoryName(lastResult) == defaults && File.Exists(lastResult), "editor saves to default folder without picker");
                    owner.folder = session;
                    await SaveAsync(recordHistory: false);
                    Check(lastResult is not null && System.IO.Path.GetDirectoryName(lastResult) == session && File.Exists(lastResult), "editor respects session folder override");
                    var previousResult = lastResult; await SaveAsync(recordHistory: false);
                    Check(lastResult != previousResult && File.Exists(previousResult), "default workflow preserves existing results with unique names");
                }
                finally { owner.folder = priorFolder; owner.defaultFolder = priorDefault; }
                var badTarget = System.IO.Path.Combine(directory, "cancelled.pdf"); using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
                var cancelled = await WorkerClient.RunAsync(new("edit", [source], badTarget, Marks: [marks[0]]), cancellation.Token);
                Check(!cancelled.Success && !File.Exists(badTarget), "cancel does not publish output");
                var overwrite = await WorkerClient.RunAsync(new("edit", [source], source, Marks: [marks[0]]), CancellationToken.None);
                Check(!overwrite.Success, "source overwrite refused by client");
                var afterHash = SHA256.HashData(await File.ReadAllBytesAsync(source));
                Check(sourceHash.SequenceEqual(afterHash), "source hash unchanged after export");
                await OpenAsync(firstOutput); Check(pageReady && marks.Count == 0, "saved result reopens internally");
                viewport.ChangeView(0, 0, 1, true); await Capture("canvas-after", sheet);
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(1024, 800)); await Capture("compact");
                Check(workspace.ColumnDefinitions[2].Width.Value == 0 && viewport.ActualWidth > 600, "compact layout preserves document viewport");
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(1600, 1000)); mode = "Edit teks asli"; await LoadOriginalRunsAsync(); BuildProperties(); Refresh();
                var original = originalRuns.First(r => r.Text == "PERTAMINA OTO - EDITOR TEST"); originalId = original.Id; BuildProperties(); Refresh();
                await Capture("original-before");
                await ApplyOriginalTextAsync(original, "PERTAMINA OTO");
                Check(marks.Any(m => m.Kind == "replace-text") && Dirty, "original replacement applied with real PDF preview");
                var workingPreview = originalPreviewFile!; Check(File.Exists(workingPreview), "original preview backed by actual worker output");
                var beforeInvalid = State; await ApplyOriginalTextAsync(original, "Ω");
                Check(State == beforeInvalid && notice.Text.Contains("Karakter", StringComparison.Ordinal), "missing glyph rejected without losing draft");
                Restore(false); for (var attempt = 0; attempt < 100 && Working; attempt++) await Task.Delay(100);
                Check(!Working && !marks.Any(m => m.Kind == "replace-text"), "undo original replacement restores preview");
                Restore(true); for (var attempt = 0; attempt < 100 && Working; attempt++) await Task.Delay(100);
                Check(!Working && marks.Any(m => m.Kind == "replace-text"), "redo original replacement restores preview");
                originalId = original.Id; BuildProperties(); Refresh(); await Capture("original-after");
                await SaveAsync(directory); Check(!Dirty && lastResult is not null && File.Exists(lastResult), "original replacement saved as new PDF");
                var originalResult = await WorkerClient.RunAsync(new("edit-text-check", [lastResult!], System.IO.Path.Combine(directory, "verify-text.pdf"), Rotation: 0), CancellationToken.None);
                Check(originalResult.TextRuns is { } verifiedRuns && verifiedRuns.Any(r => r.Text == "PERTAMINA OTO") && !verifiedRuns.Any(r => r.Text == original.Text), "reopened saved PDF contains replacement, not old text");
                originalId = original.Id; viewport.Focus(FocusState.Programmatic);
                Check(HandleObjectKey(Windows.System.VirtualKey.Delete), "Delete accepted for selected original text");
                for (var attempt = 0; attempt < 100 && Working; attempt++) await Task.Delay(100);
                Check(!Working && marks.Single(m => m.Kind == "replace-text").Text == "", "Delete removes original text in generated preview");
                Restore(false); for (var attempt = 0; attempt < 100 && Working; attempt++) await Task.Delay(100);
                Check(!Working && !Dirty, "undo original deletion restores saved state");
                originalId = original.Id; BuildProperties(); Refresh();
                var fontPicker = properties.Children.OfType<ComboBox>().Single(c => c.Header as string == "Font teks pengganti");
                Check(((IEnumerable<string>)fontPicker.ItemsSource).Contains("Times New Roman"), "installed local font appears in original editor picker");
                fontPicker.SelectedItem = "Times New Roman";
                Check(properties.Children.OfType<CheckBox>().Single().IsEnabled, "fit option enabled only after choosing a local font");
                properties.Children.OfType<TextBox>().Single().Text = "Zqx 789";
                Invoke(properties.Children.OfType<Button>().First());
                for (var attempt = 0; attempt < 100 && Working; attempt++) await Task.Delay(100);
                Check(!Working && marks.Any(m => m.Kind == "replace-text" && m.Text == "Zqx 789" && m.LocalFont == "Times New Roman"), "font picker apply changes actual original PDF preview");
                await Capture("local-font-preview");
                await SaveAsync(directory);
                var localResult = await WorkerClient.RunAsync(new("edit-text-check", [lastResult!], System.IO.Path.Combine(directory, "verify-local.pdf"), Rotation: 0), CancellationToken.None);
                Check(localResult.Success && localResult.TextRuns!.Any(r => r.Text == "Zqx 789" && r.MatchingLocalFont == "Times New Roman"), "local font result saves and reopens with font match");
                await AuditOriginalInlineAsync(Check, directory, name => Capture(name));
                mode = "Edit teks asli"; await LoadOriginalRunsAsync();
                var scrollOriginalRun = originalRuns.First(r => !string.IsNullOrWhiteSpace(r.Text));
                StartOriginalInline(scrollOriginalRun);
                originalInlineDraft = originalInlineDraft! with { Font = "Arial", Fit = true };
                inlineText!.Text = "Abc"; await Task.Delay(200);
                var originalScrollZoom = viewport.ZoomFactor;
                viewport.ChangeView(null, editorTops[1] * originalScrollZoom, null, true);
                for (var attempt = 0; attempt < 150 && (page != 1 || Working || scrollingEditor || editorScrollTimer.IsEnabled); attempt++) await Task.Delay(100);
                Check(page == 1 && originalInlineDraft is null && marks.Any(m => m.Kind == "replace-text" && m.Page == 0 && m.SourceId == scrollOriginalRun.Id && m.Text == "Abc"), "scroll commits original inline draft on source page without jumping back");
                Check(Math.Abs(viewport.ZoomFactor - originalScrollZoom) < .01, "original inline commit on scroll preserves zoom");
                await SaveAsync(directory);
                await CloseAsync(); Check(pdf is null && sourceLock is null && !Dirty, "close releases source lock and resets editor");
                await Task.Delay(100);
                Check(owner.emptyTool?.Id == "edit" && owner.emptyPanel?.IsLoaded == true && !Visible, "close returns to shared editor intake");
                Check(!File.Exists(workingPreview) && originalPreviewFile is null, "temporary original preview cleaned up");
                owner.ShowSettings(); await Capture("settings-local-update");
                Check(Descendants(owner.root).OfType<Button>().Any(b => b.Content as string == "Periksa update"), "settings exposes local update action");
                report.Add("NOT_RUN physical pinch/touch gestures, manual pointer drag, screen reader, network-denial environment. Original text limited to admitted WinAnsi/Identity-H TrueType Latin runs.");
            }
            catch (Exception error) { report.Add("FAIL " + error); Environment.ExitCode = 1; }
            finally { ReleaseFiles(); File.WriteAllLines(System.IO.Path.Combine(directory, "audit.txt"), report); }
        }
    }
}
#endif
