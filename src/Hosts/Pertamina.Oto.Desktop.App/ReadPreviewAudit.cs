#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    internal async Task AuditReadPreviewsAsync()
    {
        var output = Path.Combine(AppContext.BaseDirectory, "read-preview-audit"); Directory.CreateDirectory(output);
        libraryTestPath = Path.Combine(output, "isolated-library.json"); libraryUnreadable = false;
        favorites.Clear(); savedJobs.Clear();
        var report = new List<string>();
        void Check(bool pass, string label) { if (!pass) throw new InvalidOperationException(label); report.Add("PASS " + label); }
        async Task Capture(string name, UIElement? target = null)
        {
            var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(target ?? root); var bytes = await bitmap.GetPixelsAsync();
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite); var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, bytes.ToArray()); await encoder.FlushAsync();
        }
        static IEnumerable<DependencyObject> Descendants(DependencyObject item)
        {
            yield return item;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(item); i++)
                foreach (var child in Descendants(VisualTreeHelper.GetChild(item, i))) yield return child;
        }
        void Invoke(string name, DependencyObject parent)
        {
            var button = Descendants(parent).OfType<Button>().First(b => AutomationProperties.GetName(b) == name);
            ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
        }
        try
        {
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1586, 1000));
            var source = Path.Combine(output, "synthetic-source.pdf"); WriteHistoryAuditFixture(source);
            var originalHash = SHA256.HashData(File.ReadAllBytes(source));
            var editable = Path.Combine(output, "synthetic-editable-" + Guid.NewGuid().ToString("N") + ".pdf");
            var editableReply = await WorkerClient.RunAsync(new("edit", [source], editable,
                Marks: [new(0, "text", 40, 120, 300, 35, "EDITABLE ORIGINAL", 16)]), CancellationToken.None);
            Check(editableReply.Success, "editable original-text fixture produced by admitted worker");
            var editableHash = SHA256.HashData(File.ReadAllBytes(editable));
            var dialogChecked = false; var checkedDialogs = 0; string? dialogFailure = null;
            auditReadDialog = async reader =>
            {
                try { await reader.AuditEmbeddingAsync(Check); await Capture("expanded-read-preview", reader.View); dialogChecked = true; checkedDialogs++; }
                catch (Exception error) { dialogFailure = error.Message; }
            };
            homeFiles.Add(source); chosenFiles.Add(source); ShowHome(); root.UpdateLayout(); await Task.Delay(1500);
            Invoke("Perluas preview", root);
            for (var i = 0; i < 150 && !dialogChecked && dialogFailure is null; i++) await Task.Delay(100);
            Check(dialogChecked && dialogFailure is null, "home enlarged preview exercises shared scroll: " + dialogFailure);
            for (var i = 0; i < 30 && messageOpen; i++) await Task.Delay(100);
            Check(!messageOpen && chosenFiles.Contains(source), "home preview closes without changing document selection");
            ShowUnifiedResult([source], "Synthetic saved result", ShowHome); root.UpdateLayout(); await Task.Delay(300);
            Invoke("Buka hasil", root);
            for (var i = 0; i < 150 && pdfReader?.ReaderSource != Path.GetFullPath(source); i++) await Task.Delay(100);
            Check(pdfReader?.ReaderSource == Path.GetFullPath(source) && pdfReader.Visible && checkedDialogs == 1, "actual saved-result button opens exact result in full OTO reader");
            ShowUnifiedResult([source, editable], "Synthetic saved results", ShowHome); root.UpdateLayout(); await Task.Delay(300);
            Invoke("Preview hasil", root);
            for (var i = 0; i < 150 && checkedDialogs < 2 && dialogFailure is null; i++) await Task.Delay(100);
            Check(checkedDialogs == 2 && dialogFailure is null, $"explicit result preview exercises shared native preview (count={checkedDialogs}, failure={dialogFailure})");
            for (var i = 0; i < 30 && messageOpen; i++) await Task.Delay(100);
            // Inline source/result previews use the exact PreviewPanel factory used by active tools.
            BeginScreen("Preview audit", "Synthetic fixture"); SetHistoryPageVisible(false);
            var sourcePanel = PreviewPanel(source); body.Children.Add(sourcePanel); root.UpdateLayout(); await Task.Delay(1200);
            var scroll = Descendants(sourcePanel).OfType<ScrollViewer>().First(v => v.ZoomMode == ZoomMode.Enabled);
            Check(scroll.Content is Canvas && scroll.ScrollableHeight > 0, "tool source PreviewPanel uses continuous document surface");
            await Capture("tool-source-preview");
            body.Children.Clear(); await Task.Delay(200);
            Check(((Canvas)scroll.Content).Children.Count == 0, "removing source preview releases document frames");
            var resultPanel = PreviewPanel(source); body.Children.Add(resultPanel); root.UpdateLayout(); await Task.Delay(1200);
            Check(Descendants(resultPanel).OfType<ScrollViewer>().Any(v => v.ZoomMode == ZoomMode.Enabled && v.Content is Canvas), "saved result factory reuses shared reader");
            body.Children.Clear();
            await OpenEditorDocumentAsync(editable); await Task.Delay(300);
            await liveEditor!.AuditReadPreviewDraftAsync(Check);
            Check(checkedDialogs == 5 && dialogFailure is null, "editor source dialogs pass embedded scroll checks for object, inline and original-text drafts");
            Check(SHA256.HashData(File.ReadAllBytes(source)).SequenceEqual(originalHash), "preview entry points leave source SHA256 unchanged");
            Check(SHA256.HashData(File.ReadAllBytes(editable)).SequenceEqual(editableHash), "original-text draft preview leaves editable source SHA256 unchanged");
            report.Add("MANUAL_REQUIRED physical mouse/touchpad/pinch, screen reader UAT; editor source read excludes unsaved draft appearance");
        }
        catch (Exception error) { report.Add("FAIL " + error.Message); Environment.ExitCode = 1; }
        finally { auditReadDialog = null; libraryTestPath = null; await File.WriteAllLinesAsync(Path.Combine(output, "audit.txt"), report); }
    }

    private sealed partial class LiveEditor
    {
        internal async Task AuditReadPreviewDraftAsync(Action<bool, string> check)
        {
            Add(new(0, "text", 40, 140, 300, 35, "UNSAVED DRAFT", 16)); chosen = 0; mode = "Pilih"; Refresh();
            var state = State; var undoCount = past.Count; var redoCount = future.Count; var selectedObject = chosen;
            await owner.ShowReadPreviewDialogAsync(path!, pdf, page, "Source read; unsaved draft excluded");
            check(State == state && Dirty && past.Count == undoCount && future.Count == redoCount && chosen == selectedObject && mode == "Pilih", "editor source read preserves draft coordinates, selection and undo/redo stacks");
            Restore(false); check(marks.Count == 0, "editor undo still works after source read");
            Restore(true); check(State == state, "editor redo restores draft after source read");
            check(ReferenceEquals(viewport.Content, editorSurface) && editorSurface.Children.Contains(sheet) && (sheet.PointerCaptures is null || sheet.PointerCaptures.Count == 0), "editor continuous canvas and active-sheet gesture ownership retained");
            StartInlineText(0); await Task.Delay(150);
            var input = inlineText!; input.Text = "UNSAVED INLINE DRAFT"; await Task.Delay(150); input.Select(2, 6);
            var inlineState = State;
            await owner.ShowReadPreviewDialogAsync(path!, pdf, page);
            check(inlineText == input && State == inlineState && input.SelectionStart == 2 && input.SelectionLength == 6,
                $"read preview preserves active added-text input, draft and selection range (same={inlineText == input}, state={State == inlineState}, start={input.SelectionStart}, length={input.SelectionLength}, loaded={input.IsLoaded})");
            EndInlineText(); await LoadOriginalRunsAsync();
            check(originalRuns.Length > 0, "original-text draft test uses actual extracted PDF text");
            StartOriginalInline(originalRuns[0]); await Task.Delay(150);
            var originalInput = inlineText!; originalInput.Text += " X"; await Task.Delay(150); originalInput.Select(1, 4);
            var originalDraft = originalInlineDraft; var originalState = State; var originalUndo = past.Count;
            await owner.ShowReadPreviewDialogAsync(path!, pdf, page);
            check(originalInlineDraft == originalDraft && originalInlineDraft?.Changed == true && inlineText == originalInput
                && State == originalState && past.Count == originalUndo && originalInput.SelectionStart == 1 && originalInput.SelectionLength == 4,
                "read preview preserves uncommitted original-text draft without auto-apply or undo mutation");
        }
    }
}
#endif
