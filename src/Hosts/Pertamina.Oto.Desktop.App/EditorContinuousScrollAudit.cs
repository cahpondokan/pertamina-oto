#if DEBUG
using Microsoft.UI.Xaml;
using Pertamina.Oto.LocalPdf;

namespace Pertamina.Oto.Desktop.App;
internal sealed partial class MainWindow
{
    internal async Task AuditEditorScrollAsync(string source)
    {
        ShowLiveEditor(true); await liveEditor!.AuditContinuousScrollAsync(source);
    }
    private sealed partial class LiveEditor
    {
        internal async Task AuditContinuousScrollAsync(string source)
        {
            var directory = System.IO.Path.Combine(AppContext.BaseDirectory, "editor-scroll-audit-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory); var report = new List<string>();
            void Check(bool ok, string message) { if (!ok) throw new InvalidOperationException(message + " | " + notice.Text); report.Add("PASS " + message); }
            async Task Settle(int expected)
            {
                for (var i = 0; i < 150; i++) { await Task.Delay(100); if (page == expected && !Working && !scrollingEditor && !editorScrollTimer.IsEnabled) return; }
                Check(false, "scroll settled on page " + expected);
            }
            try
            {
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(1600, 1000));
                var hash = System.Security.Cryptography.SHA256.HashData(await File.ReadAllBytesAsync(source));
                await OpenAsync(source); owner.root.UpdateLayout(); await Task.Delay(500);
                Check(pdf is not null && pageReady && editorSheets.Length == pdf.PageCount, "document slots installed with authoritative page dimensions");
                viewport.ChangeView(null, null, .8f, true); await Settle(0); var zoom = viewport.ZoomFactor;
                mode = "Tambah teks"; PlaceObjectAt(new Windows.Foundation.Point(40, 140));
                inlineText!.Text = "SCROLL PAGE ONE"; await Task.Delay(200); EndInlineText(); var first = marks[0];
                viewport.ChangeView(null, editorTops[1] * zoom, null, true); await Settle(1);
                Check(pageInput.Value == 2 && thumbnails.Children.Count > 1, "native ScrollViewer offset updates active page, number and thumbnail list");
                Check(Math.Abs(viewport.ZoomFactor - zoom) < .01 && marks[0] == first, "scroll preserves zoom and inline added text draft");
                Check(sheet == editorSheets[1] && Math.Abs(sheet.Width - pageSizes[1].Width) < .01 && Math.Abs(sheet.Height - pageSizes[1].Height) < .01, $"active hit-test canvas uses selected page dimensions ({sheet.Width} x {sheet.Height}; expected {pageSizes[1].Width} x {pageSizes[1].Height}; page {page})");
                mode = "Tambah teks"; PlaceObjectAt(new Windows.Foundation.Point(50, 150)); inlineText!.Text = "SCROLL PAGE TWO"; await Task.Delay(200); EndInlineText();
                Check(marks[^1].Page == 1 && marks[0].Page == 0, "cross-page edits retain page ownership");
                var draft = State; Restore(false); Check(State != draft && marks[0] == first, "undo across pages"); Restore(true); Check(State == draft, "redo across pages");
                await RenderAsync(0); await Settle(0); Check(Math.Abs(viewport.ZoomFactor - zoom) < .01, "explicit page navigation preserves zoom");
                pageInput.Value = 2; await Settle(1); Check(page == 1 && Math.Abs(viewport.ZoomFactor - zoom) < .01, "page number navigation preserves zoom");
                var last = editorTops.Length - 1;
                viewport.ChangeView(null, editorTops[last] * zoom, null, true);
                viewport.ChangeView(null, editorTops[Math.Min(5, last)] * zoom, null, true);
                viewport.ChangeView(null, editorTops[last] * zoom, null, true); await Settle(last);
                Check(editorImages.Count(i => i.Source is not null) <= 12 && editorImages.Count(i => i.Source is not null) < editorImages.Length, "rapid navigation retains bounded page bitmaps");
                Check(editorImages.Where(i => i.Source is not null).All(i => i.Source is Microsoft.UI.Xaml.Media.Imaging.BitmapImage b && b.PixelWidth <= 1801 && b.PixelHeight <= 1801), "bitmap dimensions bounded for tall or wide pages");
                viewport.ChangeView(null, 0, null, true); await Settle(0); Check(pageInput.Value == 1, "native scrolling returns to preceding pages");
                await SaveAsync(directory, false); Check(lastResult is not null, "cross-page draft saves to separate PDF");
                var reply = await WorkerClient.RunAsync(new("edit-text-check", [lastResult!], System.IO.Path.Combine(directory, "check.pdf"), Rotation: 1), CancellationToken.None);
                Check(reply.Success && reply.TextRuns!.Any(r => r.Text == "SCROLL PAGE TWO"), "saved second-page text validated by worker");
                Check(System.Security.Cryptography.SHA256.HashData(await File.ReadAllBytesAsync(source)).SequenceEqual(hash), "source PDF unchanged");
                report.Add("NOT_RUN physical wheel/touchpad/touch input; native ChangeView exercises the same ViewChanged pipeline. Original inline text coverage remains in --audit-live-editor.");
            }
            catch (Exception error) { report.Add("FAIL " + error); Environment.ExitCode = 1; }
            finally { ReleaseFiles(); File.WriteAllLines(System.IO.Path.Combine(directory, "audit.txt"), report); File.WriteAllText(System.IO.Path.Combine(AppContext.BaseDirectory, "editor-scroll-audit-path.txt"), directory); }
        }
    }
}
#endif
