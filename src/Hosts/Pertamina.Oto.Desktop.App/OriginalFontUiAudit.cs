#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Pertamina.Oto.LocalPdf;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    internal static void PrepareOriginalFontAuditStorage() => libraryTestPath = Path.Combine(AppContext.BaseDirectory, "font-audit-library.json");
    internal async Task AuditOriginalFontCompatibilityAsync(string source)
    {
        ShowLiveEditor(true);
        await liveEditor!.AuditOriginalFontCompatibilityAsync(source);
    }

    private sealed partial class LiveEditor
    {
        internal async Task AuditOriginalFontCompatibilityAsync(string source)
        {
            var directory = System.IO.Path.Combine(AppContext.BaseDirectory, "font-ui-audit-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var report = new List<string>();
            void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label + " | " + notice.Text); report.Add("PASS " + label); }
            async Task Capture(string name)
            {
                owner.root.UpdateLayout(); await Task.Delay(200);
                var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(owner.root);
                var pixels = await bitmap.GetPixelsAsync();
                var file = await (await StorageFolder.GetFolderFromPathAsync(directory)).CreateFileAsync(name + ".png");
                using var output = await file.OpenAsync(FileAccessMode.ReadWrite);
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, output);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, pixels.ToArray());
                await encoder.FlushAsync();
            }
            try
            {
                var hash = SHA256.HashData(await File.ReadAllBytesAsync(source));
                Check(Convert.ToHexString(hash) == "A4E475D5B5CD1601766B0702C73523E1C3A377139CED424A364E295A44201C3B", "exact synthetic fixture");
                owner.AppWindow.Resize(new Windows.Graphics.SizeInt32(1600, 1000));
                await OpenAsync(source);
                for (var p = 0; p < 4; p++)
                {
                    await RenderAsync(p); mode = "Edit teks asli"; await LoadOriginalRunsAsync(); BuildProperties(); Refresh();
                    Check(pageReady && originalRuns.Length >= 20, "native page " + (p + 1) + " exposes actual text overlays");
                    if (p == 3) Check(!originalRuns.Any(r => r.Text == "KODE UJI 123") && notice.Text.Contains("Dilewati", StringComparison.Ordinal), "unsupported rotation disclosed without disabling whole page");
                    await Capture("page-" + (p + 1));
                }
                await RenderAsync(1); mode = "Edit teks asli"; await LoadOriginalRunsAsync(); BuildProperties(); Refresh();
                var run = originalRuns.First(r => r.Text == "Nama: Budi Santoso | Nomor: 12345");
                var point = new Windows.Foundation.Point(run.X + 3, run.Y + 7);
                lastTextTarget = null;
                await OriginalTextPressedAsync(run, point, 10000);
                Check(inlineText is null && originalId == run.Id, "first click selects original mapped font");
                await OriginalTextPressedAsync(run, point, 10001); await Task.Delay(150);
                Check(inlineText is not null && ReferenceEquals(FocusManager.GetFocusedElement(View.XamlRoot), inlineText), "double click opens focused native text input");
                Check(originalInlineDraft?.Font == "", "preserve-PDF-font selection does not silently choose local font");
                Check(InlineTextPressed(inlineText!, point, 10002), "triple click routed");
                await Task.Delay(100);
                Check(inlineText!.SelectedText == run.Text, "triple click selects whole source text");
                inlineText.SelectedText = "Nama: Budi";
                await Task.Delay(100);
                Check(await CommitOriginalInlineAsync() && marks.Any(m => m.Text == "Nama: Budi" && m.LocalFont == ""), "inline edit creates actual PDF preview preserving source font");
                await Capture("inline-edited");
                StartOriginalInline(run); await Task.Delay(100); inlineText!.Text = new string('W', 200); await Task.Delay(100);
                Check(!await CommitOriginalInlineAsync() && originalInlineDraft is not null, "rejected oversized edit retains correctable draft");
                CancelOriginalInline();
                await SaveAsync(directory, recordHistory: false);
                Check(lastResult is not null && File.Exists(lastResult) && !Dirty, "native Save As writes new PDF");
                var saved = lastResult!;
                await OpenAsync(saved); await RenderAsync(1); mode = "Edit teks asli"; await LoadOriginalRunsAsync(); BuildProperties(); Refresh();
                Check(originalRuns.Any(r => r.Text == "Nama: Budi" && r.Font == run.Font), "saved PDF opens internally with new text and same source font");
                await Capture("saved-reopened");
                var afterHash = SHA256.HashData(await File.ReadAllBytesAsync(source));
                Check(hash.SequenceEqual(afterHash), "original fixture hash preserved");
                report.Add("NOT_RUN physical mouse/keyboard/touch, screen reader, clean-PC, network-denial environment.");
            }
            catch (Exception error) { report.Add("FAIL " + error); Environment.ExitCode = 1; }
            finally
            {
                ReleaseFiles();
                File.WriteAllLines(System.IO.Path.Combine(directory, "audit.txt"), report);
                File.WriteAllText(System.IO.Path.Combine(AppContext.BaseDirectory, "font-audit-path.txt"), directory);
            }
        }
    }
}
#endif
