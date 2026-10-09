#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;
internal sealed partial class MainWindow
{
    private Func<ContentDialog, Task>? auditPrintDialog;
    private sealed partial class PdfReaderView
    {
        internal static void WritePrintMixed(string path) => WriteZoomSurfaceFixture(path);
    }
    private async Task AuditEditorPrintRoutingAsync(string source, string directory, Action<bool, string> check)
    {
        ShowLiveEditor(true); await liveEditor!.AuditPrintRoutingAsync(source, directory, check);
    }
    private sealed partial class LiveEditor
    {
        internal async Task AuditPrintRoutingAsync(string source, string directory, Action<bool, string> check)
        {
            await OpenAsync(source); Add(new Pertamina.Oto.LocalPdf.EditorMark(0, "text", 40, 140, 200, 30, "Synthetic print edit")); var before = State; var undoCount = past.Count;
            owner.auditPrintNotice = (title, text) => { check(title == "Simpan hasil edit dahulu", "dirty editor explicitly requests export before print"); return Task.CompletedTask; };
            await PrintExportAsync(); check(Dirty && State == before && past.Count == undoCount, "print guard preserves editor draft and undo"); owner.auditPrintNotice = null;
            await SaveAsync(directory, false); check(lastResult is not null && !Dirty, "real edited output exported and validated");
            owner.auditPrintDialog = async dialog => { await Task.Delay(200); check(Descendants(dialog).OfType<TextBlock>().Any(t => t.Text == Path.GetFileName(lastResult)), "editor print targets latest exported result rather than source"); dialog.Hide(); };
            await PrintExportAsync(); owner.auditPrintDialog = null;
        }
    }
    internal async Task AuditPrintAsync()
    {
        var output = Path.Combine(AppContext.BaseDirectory, "print-audit"); Directory.CreateDirectory(output);
        var report = new List<string>();
        void Check(bool ok, string text) { report.Add((ok ? "PASS " : "FAIL ") + text); }
        static IEnumerable<DependencyObject> Children(DependencyObject node)
        { yield return node; for (var i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++) foreach (var c in Children(VisualTreeHelper.GetChild(node, i))) yield return c; }
        async Task Capture(UIElement target, string name)
        {
            var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(target);
            var bytes = (await bitmap.GetPixelsAsync()).ToArray();
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite); var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, bytes); await encoder.FlushAsync();
        }
        bool FullyVisible(FrameworkElement element)
        {
            if (element.Visibility != Visibility.Visible || element.ActualWidth <= 0 || element.ActualHeight <= 0) return false;
            var bounds = element.TransformToVisual(root).TransformBounds(new Windows.Foundation.Rect(0, 0, element.ActualWidth, element.ActualHeight));
            if (bounds.X < -1 || bounds.Y < -1 || bounds.Right > root.XamlRoot.Size.Width + 1 || bounds.Bottom > root.XamlRoot.Size.Height + 1) return false;
            for (DependencyObject? parent = VisualTreeHelper.GetParent(element); parent is not null; parent = VisualTreeHelper.GetParent(parent))
                if (parent is FrameworkElement frame && frame.ActualWidth > 0 && frame.ActualHeight > 0)
                {
                    var viewport = frame.TransformToVisual(root).TransformBounds(new Windows.Foundation.Rect(0, 0, frame.ActualWidth, frame.ActualHeight));
                    if (bounds.Y < viewport.Y - 1 || bounds.Bottom > viewport.Bottom + 1 || bounds.X < viewport.X - 1 || bounds.Right > viewport.Right + 1) return false;
                }
            return true;
        }
        static void Invoke(Button button) => ((Microsoft.UI.Xaml.Automation.Provider.IInvokeProvider)new Microsoft.UI.Xaml.Automation.Peers.ButtonAutomationPeer(button).GetPattern(Microsoft.UI.Xaml.Automation.Peers.PatternInterface.Invoke)).Invoke();
        static async Task<bool> Until(Func<bool> condition)
        { for (var i = 0; i < 100; i++) { if (condition()) return true; await Task.Delay(100); } return false; }
        try
        {
            foreach (var value in new[] { "A4", "Letter", "日本語 ukuran", "", new string('x', 64) })
            {
                var slot = new string('z', 64).ToCharArray(); value.CopyTo(0, slot, 0, value.Length); if (value.Length < 64) slot[value.Length] = '\0';
                Check(PrintDevice.ParsePaperName(slot) == value, "64-char paper slot stops at first null, synthetic case length " + value.Length);
            }
            foreach (var invalidLength in new[] { 0, 63, 65 })
            { var rejected = false; try { PrintDevice.ParsePaperName(new char[invalidLength]); } catch (ArgumentException) { rejected = true; } Check(rejected, "paper slot length rejected " + invalidLength); }
            foreach (var invalidCount in new[] { int.MinValue, -1, 0, 1001, int.MaxValue })
            { var rejected = false; try { PrintDevice.PaperBufferBytes(invalidCount); } catch (InvalidOperationException) { rejected = true; } Check(rejected, "paper count bounded " + invalidCount); }
            Check(PrintDevice.PaperBufferBytes(1) == 128 && PrintDevice.PaperBufferBytes(1000) == 128000, "paper allocation count boundaries");
            foreach (var state in new int?[] { null, 0, 0x80, 1 }) Check(PrintDevice.WindowsStatus(state).Contains("Keterjangkauan belum diverifikasi", StringComparison.Ordinal), "Windows status never implies verified reachability " + state);
            Check(PrintDevice.WindowsStatus(0).Contains("cache", StringComparison.Ordinal) && PrintDevice.WindowsStatus(0x80).Contains("Offline", StringComparison.Ordinal), "cached ready and offline distinguished");
            foreach (var error in new Exception[] { new IOException("SYNTHETIC_PRIVATE_VALUE"), new InvalidOperationException("SYNTHETIC_PRIVATE_VALUE"), new UnauthorizedAccessException("SYNTHETIC_PRIVATE_VALUE"), new NotSupportedException("SYNTHETIC_PRIVATE_VALUE") })
                Check(!PrintWorker.SafeFailureReason(error).Contains("SYNTHETIC_PRIVATE_VALUE", StringComparison.Ordinal), "safe reason excludes exception message " + error.GetType().Name);
            Check(PrintPlan.Pages("Semua halaman", "", 8, 3).SequenceEqual(Enumerable.Range(0, 8)), "all page mapping");
            foreach (var size in new[] { (1d, 100000d), (100000d, 1d), (100000d, 100000d) })
            {
                var r = PrintPlan.RasterSize(size.Item1, size.Item2, 800, 1600, 2_000_000);
                Check(r.Width <= 1600 && r.Height <= 1600 && (long)r.Width * r.Height <= 2_000_000, "extreme-aspect preview raster bounded " + size);
            }
            Check(PrintPlan.Pages("Halaman saat ini", "", 8, 3).SequenceEqual([3]), "current page zero-based mapping");
            Check(PrintPlan.Pages("Rentang halaman", "1, 3–5, 4", 8, 0).SequenceEqual([0, 2, 3, 4]), "custom ranges and duplicate removal");
            foreach (var invalid in new[] { "", "0", "9", "5-2", "1,,2", "a", "1-2-3", "-1", "2.5" })
            { var rejected = false; try { PrintPlan.Pages("Rentang halaman", invalid, 8, 0); } catch (InvalidOperationException) { rejected = true; } Check(rejected, "invalid range rejected: " + invalid); }
            var bounds = new PrintDevice.Bounds(2480, 3508, 50, 60, 2380, 3388, 300, 300);
            foreach (var size in new[] { (595d, 842d), (842d, 595d), (1123d, 1587d), (1587d, 1123d) })
            {
                var fit = PrintPlan.Placement(size.Item1, size.Item2, bounds, false); var actual = PrintPlan.Placement(size.Item1, size.Item2, bounds, true);
                Check(fit.X >= bounds.Left - 1 && fit.Y >= bounds.Top - 1 && fit.W <= bounds.PrintableWidth + 1 && fit.H <= bounds.PrintableHeight + 1, "fit within driver printable bounds " + size);
                Check(Math.Abs(actual.W - size.Item1 * 300 / 96) < .01, "actual size uses printer DPI " + size);
            }
            var names = PrintDevice.Names(); // Do not persist local printer names or environment details in evidence.
            var source = Path.Combine(output, "fixture-" + Guid.NewGuid().ToString("N") + ".pdf"); WriteHistoryAuditFixture(source); var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(source)));
                                    Check(!await PrintSecurity.HasEncryptionAsync(source), "ordinary PDF admitted by encryption guard");
            var protectedPdf = Path.Combine(output, "protected-" + Guid.NewGuid().ToString("N") + ".pdf");
            var protection = await WorkerClient.RunAsync(new("protect", [source], protectedPdf, NewPassword: "synthetic-print-test"), CancellationToken.None);
            Check(protection.Success && await PrintSecurity.HasEncryptionAsync(protectedPdf), "real encrypted PDF rejected before rendering/submission");
            var marker = Path.Combine(output, "boundary-marker.bin"); File.WriteAllBytes(marker, new byte[65533].Concat(System.Text.Encoding.ASCII.GetBytes("/Encrypt")).ToArray());
            Check(await PrintSecurity.HasEncryptionAsync(marker), "encryption token across scan buffer boundary rejected");
            var virtualPrinter = names.FirstOrDefault(n => n.Equals("Microsoft Print to PDF", StringComparison.OrdinalIgnoreCase));
            auditPrintPrinters = () => virtualPrinter is null ? [] : [virtualPrinter]; // All audit dialogs select only the virtual target.
            if (virtualPrinter is not null)
            {
                using var device = new PrintDevice(virtualPrinter);
                var paper = device.Papers.FirstOrDefault(p => p.Name.Contains("A4", StringComparison.OrdinalIgnoreCase)) ?? device.Papers[0];
                device.Configure(paper.Id, false, false, 1);
                var virtualOutput = Path.Combine(output, "virtual-" + Guid.NewGuid().ToString("N") + ".pdf");
                var request = new PrintWorker.Request(source, virtualPrinter, device.ExportMode(), [0], 2, false, virtualOutput);
                var job = await PrintWorker.SubmitAsync(request, _ => { }, CancellationToken.None);
                Check(job > 0, "actual isolated virtual print accepted by spooler");
                var virtualPdf = await Task.Run(async () => await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(virtualOutput)));
                Check(virtualPdf.PageCount == 2, "actual virtual PDF roundtrip with two copies");
                using var canceled = new CancellationTokenSource(); canceled.Cancel(); var rejected = false;
                try { await PrintWorker.SubmitAsync(request, _ => { }, canceled.Token); } catch (OperationCanceledException) { rejected = true; }
                Check(rejected, "pre-submit cancellation prevents another job");
                var mixed = Path.Combine(output, "mixed-" + Guid.NewGuid().ToString("N") + ".pdf"); PdfReaderView.WritePrintMixed(mixed);
                var rotated = Path.Combine(output, "mixed-rotated-" + Guid.NewGuid().ToString("N") + ".pdf");
                var rotationReply = await WorkerClient.RunAsync(new("rotate", [mixed], rotated, Pages: "3", Rotation: 90), CancellationToken.None);
                Check(rotationReply.Success, "mixed A4/A3/A2 and explicit page rotation fixture validated by admitted worker");
                foreach (var landscape in new[] { false, true }) foreach (var actual in new[] { false, true })
                {
                    device.Configure(paper.Id, landscape, false, 1);
                    var target = Path.Combine(output, $"virtual-{landscape}-{actual}-" + Guid.NewGuid().ToString("N") + ".pdf");
                    await PrintWorker.SubmitAsync(new(rotated, virtualPrinter, device.ExportMode(), [0, 2], 1, actual, target), _ => { }, CancellationToken.None);
                    var printed = await Task.Run(async () => await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(target)));
                    using var page = printed.GetPage(0);
                    Check(printed.PageCount == 2 && (page.Size.Width > page.Size.Height) == landscape, $"virtual custom-page/physical-orientation roundtrip landscape={landscape} actual={actual}");
                    using var rendered = new Windows.Storage.Streams.InMemoryRandomAccessStream(); await page.RenderToStreamAsync(rendered, new Windows.Data.Pdf.PdfPageRenderOptions { DestinationWidth = 800 }); rendered.Seek(0);
                    using var renderedStream = rendered.AsStreamForRead();
                    using (var png = new FileStream(target + ".png", FileMode.CreateNew, FileAccess.Write)) await renderedStream.CopyToAsync(png);
                    rendered.Seek(0); var decoder = await BitmapDecoder.CreateAsync(rendered); var pixels = (await decoder.GetPixelDataAsync()).DetachPixelData();
                    Check(pixels.Where((b, i) => i % 4 != 3 && b < 240).Take(101).Count() > 100, "virtual output has real rendered content " + landscape + "/" + actual);
                }
                var cancelTarget = Path.Combine(output, "virtual-cancel-" + Guid.NewGuid().ToString("N") + ".pdf");
                using var during = new CancellationTokenSource(); var aborted = false;
                try { await PrintWorker.SubmitAsync(new(source, virtualPrinter, device.ExportMode(), Enumerable.Repeat(0, 100).ToArray(), 1, false, cancelTarget), n => { if (n >= 1) during.Cancel(); }, during.Token); }
                catch (OperationCanceledException) { aborted = true; }
                Check(aborted && !Directory.EnumerateDirectories(output, "job-*").Any(), "in-flight virtual cancellation joins child and cleans request/progress state");
            }
            else report.Add("NOT_TESTED actual virtual-print roundtrip: Microsoft Print to PDF unavailable.");
            // Exercise the existing import path and inspect the real toolbar rather than changing its handler.
            homeFiles.Add(source); chosenFiles.Add(source); ShowOrganizer();
            Check(await Until(() => !importing && pageLayout.Count > 0), "synthetic organizer import completed through existing path"); root.UpdateLayout();
            var addPdf = Children(root).OfType<Button>().FirstOrDefault(b => Children(b).OfType<TextBlock>().Any(t => t.Text == "Tambah PDF"));
            Check(addPdf is not null && Children(addPdf).OfType<FontIcon>().Count(i => i.Glyph == "\uE710") == 1 && !Children(addPdf).OfType<TextBlock>().Any(t => t.Text.Contains('＋')), "organizer Add PDF uses single plus and unchanged native button");
            await Capture(root, "organizer-single-plus");
            if (virtualPrinter is not null)
            {
                foreach (var width in new[] { 1100, 620, 480 })
                {
                    AppWindow.Resize(new Windows.Graphics.SizeInt32(width, 600)); await Task.Delay(250);
                    auditPrintDialog = async dialog =>
                    {
                        await Task.Delay(1200); dialog.UpdateLayout();
                        if (width == 480) { foreach (var text in Children(dialog).OfType<TextBlock>()) text.FontSize *= 1.5; dialog.UpdateLayout(); await Task.Delay(200); }
                        var submit = Children(dialog).OfType<Button>().First(b => Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(b) == "Cetak dokumen");
                        var cancel = Children(dialog).OfType<Button>().First(b => Equals(b.Content, "Batal"));
                        var status = Children(dialog).OfType<TextBlock>().First(t => Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(t) == "Status pengiriman cetak");
                        void CheckFooter(string state) { dialog.UpdateLayout(); Check(FullyVisible(submit) && FullyVisible(cancel) && FullyVisible(status), $"footer buttons and full status inside actual root/scroll viewport {width} {state}"); }
                        Check(Children(dialog).OfType<TextBlock>().Any(t => t.Text.Contains("Keterjangkauan belum diverifikasi", StringComparison.Ordinal)), "visible unverified connection warning " + width);
                        var calls = 0; var completion = new TaskCompletionSource<int>();
                        auditPrintWorker = (_, progress, _) => { calls++; progress(1); return completion.Task; };
                        Check(await Until(() => submit.IsEnabled), "print preparation completed before simulated submit " + width);
                        Invoke(submit); Check(await Until(() => status.Text.StartsWith("Mengirim", StringComparison.Ordinal)), "sending appears immediately " + width); CheckFooter("sending"); await Capture(dialog, "sending-" + width);
                        Check(!submit.IsEnabled && cancel.IsEnabled && calls == 1, "sending prevents duplicate submission and allows cancel " + width);
                        AppWindow.Resize(new Windows.Graphics.SizeInt32(width == 1100 ? 760 : width, 580)); await Task.Delay(300); CheckFooter("resized sending"); await Capture(dialog, "resized-sending-" + width);
                        completion.SetResult(42); Check(await Until(() => status.Text.StartsWith("Diterima antrean Windows", StringComparison.Ordinal) && submit.IsEnabled), "simulated queue acceptance restores explicit print-again action " + width); CheckFooter("queued"); await Capture(dialog, "queued-" + width);
                        var terminal = status.Text; var copies = Children(dialog).OfType<NumberBox>().First(); copies.Value = 2; await Task.Delay(300);
                        Check(status.Text == terminal && calls == 1, "preview/settings preserve terminal result and do not resend " + width);
                        auditPrintWorker = (_, _, _) => { calls++; throw new InvalidOperationException("SYNTHETIC_PRIVATE_VALUE"); };
                        Invoke(submit); Check(await Until(() => status.Text.StartsWith("Gagal:", StringComparison.Ordinal) && submit.IsEnabled), "explicit retry simulated safe failure restores action " + width);
                        Check(!status.Text.Contains("SYNTHETIC_PRIVATE_VALUE", StringComparison.Ordinal) && calls == 2, "safe failure reason and exactly one explicit retry " + width); CheckFooter("error"); await Capture(dialog, "error-" + width);
                        auditPrintWorker = async (_, _, token) => { calls++; await Task.Delay(Timeout.Infinite, token); return 0; };
                        Invoke(submit); Check(await Until(() => !submit.IsEnabled && status.Text.StartsWith("Mengirim", StringComparison.Ordinal)), "explicit retry starts simulated cancellable sending " + width);
                        Invoke(cancel); Check(await Until(() => status.Text.StartsWith("Dibatalkan", StringComparison.Ordinal) && submit.IsEnabled), "simulated cancellation visible and retry restored " + width); CheckFooter("cancel"); await Capture(dialog, "cancel-" + width);
                        Check(calls == 3, "no automatic resend after cancel " + width);
                        auditPrintWorker = null;
                        auditPrintStatus = PrintDevice.WindowsStatus(0x80); copies.Value = 1; await Task.Delay(300);
                        Check(!submit.IsEnabled && Children(dialog).OfType<TextBlock>().Any(t => t.Text.StartsWith("Windows melaporkan printer offline", StringComparison.Ordinal) && FullyVisible(t)), "simulated Windows offline blocks submit with visible action " + width); CheckFooter("offline plus retained cancel"); await Capture(dialog, "offline-simulated-" + width);
                        auditPrintStatus = null; dialog.Hide();
                    };
                    await ShowPrintAsync(source, "SIMULASI status cetak · Tidak mengirim job");
                }
            }
            foreach (var width in new[] { 1400, 760, 620, 480 })
            {
                AppWindow.Resize(new Windows.Graphics.SizeInt32(width, 900)); await Task.Delay(250); root.UpdateLayout();
                auditPrintDialog = async dialog =>
                {
                    await Task.Delay(1200); dialog.UpdateLayout();
                    if (width == 480) { foreach (var text in Children(dialog).OfType<TextBlock>()) text.FontSize *= 1.5; dialog.UpdateLayout(); }
                    Check(((FrameworkElement)dialog.Content).ActualHeight <= dialog.XamlRoot.Size.Height - 80, "dialog bounded at width " + width);
                    var writeRejected = false; try { using var attemptedWrite = new FileStream(source, FileMode.Open, FileAccess.Write, FileShare.ReadWrite); } catch (IOException) { writeRejected = true; } Check(writeRejected, "source modification blocked while print identity locked " + width);
                    var controls = Children(dialog).OfType<ComboBox>().ToArray();
                    var range = controls.First(c => Equals(c.Header, "Halaman")); range.SelectedIndex = 2;
                    var custom = Children(dialog).OfType<TextBox>().First(c => c.PlaceholderText == "Contoh: 1, 3–5"); custom.Text = "0"; await Task.Delay(200); Check(!dialog.IsPrimaryButtonEnabled, "invalid range disables submit " + width);
                    custom.Text = "1"; await Task.Delay(300); await Capture(dialog, "dialog-" + width);
                    range.SelectedIndex = 1; await Task.Delay(300); Check(range.SelectedIndex == 1, "current selection available " + width);
                    var settingsScroll = Children(dialog).OfType<ScrollViewer>().First(v => v.Content is StackPanel);
                    settingsScroll.ChangeView(null, settingsScroll.ScrollableHeight, null, true); await Task.Delay(200);
                    Children(dialog).OfType<Expander>().First(e => Equals(e.Header, "Pengaturan lanjutan")).IsExpanded = true;
                    await Task.Delay(200); settingsScroll.ChangeView(null, settingsScroll.ScrollableHeight, null, true);
                    await Capture(dialog, "advanced-" + width);
                    dialog.Hide();
                };
                await ShowPrintAsync(source, "Dokumen uji", 0); Check(!printOpen && !messageOpen, "modal lifecycle released " + width);
            }
            auditPrintPrinters = () => [];
            auditPrintDialog = async dialog => { await Task.Delay(300); Check(!dialog.IsPrimaryButtonEnabled, "no-printer disables print with visible reason"); await Capture(dialog, "no-printer"); dialog.Hide(); };
            await ShowPrintAsync(source, "Dokumen uji"); auditPrintPrinters = null; auditPrintDialog = null;
            auditPrintNotice = (title, text) => { Check(title == "Cetak tidak tersedia", "missing/malformed/protected input reports failure"); return Task.CompletedTask; };
            await ShowPrintAsync(Path.Combine(output, "missing.pdf"), "Dokumen uji");
            await ShowPrintAsync(marker, "Dokumen uji");
            await ShowPrintAsync(protectedPdf, "Dokumen uji"); auditPrintNotice = null;
            await OpenPdfReaderAsync(source); await pdfReader!.NavigateAsync(1); var beforePage = pdfReader.ReadingPage; var beforeZoom = pdfReader.ReadingViewport.ZoomFactor;
            auditPrintDialog = async dialog => { await Task.Delay(200); Check(Children(dialog).OfType<TextBlock>().Any(t => t.Text == Path.GetFileName(source)), "menu/keyboard route targets active reader identity"); await Capture(dialog, "reader-active-print"); dialog.Hide(); };
            await PrintActiveAsync(); Check(pdfReader.ReadingPage == beforePage && Math.Abs(pdfReader.ReadingViewport.ZoomFactor - beforeZoom) < .001, "print preserves reader page and zoom"); auditPrintDialog = null;
            await AuditEditorPrintRoutingAsync(source, output, Check);
            Check(hash == Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(source))), "source bytes unchanged");
            Check(!Directory.EnumerateFiles(Path.Combine(Path.GetTempPath(), "PertaminaOTO-Print")).Any(), "print snapshots cleaned up");
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1400, 1000)); await Task.Delay(250); ShowUnifiedResult([source], "Hasil uji", ShowHome); root.UpdateLayout(); await Task.Delay(1500); await Capture(latestResultCard!, "result-single");
            Check(Children(root).OfType<Button>().Any(b => Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(b).Contains("Cetak hasil", StringComparison.Ordinal)), "single PDF result print action");
            ShowUnifiedResult([source, source], "Hasil uji", ShowHome); root.UpdateLayout();
            Check(Children(root).OfType<Button>().Count(b => Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(b) == "Cetak hasil ini") == 2, "multi-output explicit per-file print selection");
            ShowUnifiedResult([Path.Combine(output, "image.jpg")], "Hasil gambar", ShowHome); root.UpdateLayout();
            Check(!Children(root).OfType<Button>().Any(b => Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(b).Contains("Cetak hasil", StringComparison.Ordinal)), "non-PDF results do not show print");
            Check(names.Length == 0 || names.All(n => !string.IsNullOrWhiteSpace(n)), "Windows printer enumeration valid (" + names.Length + ")");
            report.Add("NOT_TESTED physical printers, monitor DPI, hardware keyboard/accessibility UAT. Only Microsoft Print to PDF used for spooler roundtrip; no physical jobs.");
        }
        catch (Exception error) { report.Add("FAIL " + error); }
        finally { auditPrintDialog = null; auditPrintNotice = null; auditPrintPrinters = null; auditPrintWorker = null; auditPrintStatus = null; File.WriteAllLines(Path.Combine(output, "audit.txt"), report); Environment.ExitCode = report.Any(s => s.StartsWith("FAIL ", StringComparison.Ordinal)) ? 1 : 0; }
    }
}
#endif
