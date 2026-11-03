#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed record OriginalCorpusCase(string Source, string Sha256, int[] Counts);
    internal async Task AuditOriginalCorpusAsync(string manifest)
    {
        ShowLiveEditor(true);
        await liveEditor!.AuditOriginalCorpusAsync(manifest);
    }

    private sealed partial class LiveEditor
    {
        internal async Task AuditOriginalCorpusAsync(string manifest)
        {
            var directory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(manifest)!, "native-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var report = new List<string>();
            void Check(bool ok, string label)
            {
                if (!ok) throw new InvalidOperationException(label);
                report.Add("PASS " + label);
            }
            async Task Capture(string name)
            {
                owner.root.UpdateLayout(); await Task.Delay(200);
                var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(owner.root);
                var pixels = await bitmap.GetPixelsAsync();
                var file = await (await StorageFolder.GetFolderFromPathAsync(directory)).CreateFileAsync(name + ".png", CreationCollisionOption.FailIfExists);
                using var output = await file.OpenAsync(FileAccessMode.ReadWrite);
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, output);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, pixels.ToArray());
                await encoder.FlushAsync();
            }
            try
            {
                var cases = JsonSerializer.Deserialize<OriginalCorpusCase[]>(await File.ReadAllTextAsync(manifest))!;
                Check(cases.Length == 2, "two explicitly authorized corpus documents");
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(1600, 1000));
                for (var d = 0; d < cases.Length; d++)
                {
                    var item = cases[d];
                    var hash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(item.Source)));
                    Check(hash.Equals(item.Sha256, StringComparison.OrdinalIgnoreCase), "exact source hash document " + (d + 1));
                    await OpenAsync(item.Source);
                    Check(HasDocument && pdf!.PageCount == item.Counts.Length, "native source opens document " + (d + 1));
                    for (var p = 0; p < item.Counts.Length; p++)
                    {
                        await RenderAsync(p); mode = "Edit teks asli";
                        await LoadOriginalRunsAsync(); BuildProperties(); Refresh();
                        Check(pageReady && originalRuns.Length == item.Counts[p], "all overlays on document " + (d + 1) + " page " + (p + 1));
                        if (p == 0) await Capture("document-" + (d + 1) + "-overlays");
                    }
                    await RenderAsync(0); mode = "Edit teks asli"; await LoadOriginalRunsAsync(); BuildProperties(); Refresh();
                    // A paragraph/table line near the end, formerly skipped after T*.
                    var run = originalRuns.Last(r => r.Text.Length > 20);
                    var value = run.Text[..^3];
                    var point = new Windows.Foundation.Point(run.X + 2, run.Y + 2);
                    lastTextTarget = null;
                    await OriginalTextPressedAsync(run, point, 20000);
                    await OriginalTextPressedAsync(run, point, 20001); await Task.Delay(100);
                    Check(inlineText is not null && ReferenceEquals(FocusManager.GetFocusedElement(View.XamlRoot), inlineText), "double-click callback focuses original text input");
                    inlineText!.Text = value; await Task.Delay(100);
                    Check(await CommitOriginalInlineAsync() && marks.Any(m => m.Text == value && m.LocalFont == ""), "native preview edits actual source text retaining font");
                    await RestoreOriginalHistoryAsync(false);
                    Check(marks.Count == 0, "undo restores original text");
                    await RestoreOriginalHistoryAsync(true);
                    Check(marks.Any(m => m.Text == value), "redo restores edited text");
                    await Capture("document-" + (d + 1) + "-edited");
                    StartOriginalInline(run); await Task.Delay(100); inlineText!.Text = new string('W', 200); await Task.Delay(100);
                    Check(!await CommitOriginalInlineAsync() && originalInlineDraft is not null, "overflow blocked with recoverable draft");
                    CancelOriginalInline();
                    await SaveAsync(directory, recordHistory: false);
                    Check(lastResult is not null && File.Exists(lastResult) && !Dirty, "Save As writes separate result");
                    var saved = lastResult!;
                    Check(!string.Equals(saved, item.Source, StringComparison.OrdinalIgnoreCase), "save cannot replace original");
                    await OpenAsync(saved); await RenderAsync(0); mode = "Edit teks asli"; await LoadOriginalRunsAsync(); BuildProperties(); Refresh();
                    Check(originalRuns.Any(r => r.Text == value && r.Font == run.Font && Math.Abs(r.X - run.X) < .002 && Math.Abs(r.Y - run.Y) < .002), "saved result reopens at original font and location");
                    Check(hash == Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(item.Source))), "original source remains byte-identical");
                    ReleaseFiles();
                }
            }
            catch (Exception error)
            {
                report.Add("FAIL " + error.GetType().Name + ": " + error.Message);
                Environment.ExitCode = 1;
            }
            finally
            {
                ReleaseFiles();
                await File.WriteAllLinesAsync(System.IO.Path.Combine(directory, "audit.txt"), report);
                await File.WriteAllTextAsync(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(manifest)!, "native-audit-path.txt"), directory);
            }
        }
    }
}
#endif
