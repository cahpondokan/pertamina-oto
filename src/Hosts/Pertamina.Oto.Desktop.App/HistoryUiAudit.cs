#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
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
    internal async Task AuditHistoryUiAsync(string fixture)
    {
        var report = new List<string>();
        var output = Path.Combine(AppContext.BaseDirectory, "history-ui-audit"); Directory.CreateDirectory(output);
        libraryTestPath = Path.Combine(output, "isolated-library.json"); libraryUnreadable = false;
        void Check(bool pass, string label) { if (!pass) throw new InvalidOperationException(label); report.Add("PASS " + label); }
        async Task Settle() { root.UpdateLayout(); await Task.Delay(350); root.UpdateLayout(); }
        async Task Capture(string name)
        {
            await Settle();
            var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(root);
            var pixels = await bitmap.GetPixelsAsync();
            var directory = await StorageFolder.GetFolderFromPathAsync(output);
            var file = await directory.CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, pixels.ToArray());
            await encoder.FlushAsync();
            report.Add("CAPTURE " + name + " " + bitmap.PixelWidth + "x" + bitmap.PixelHeight);
        }
        static IEnumerable<DependencyObject> Descendants(DependencyObject item)
        {
            yield return item;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(item); i++)
                foreach (var child in Descendants(VisualTreeHelper.GetChild(item, i))) yield return child;
        }
        async Task Click(string name)
        {
            var button = Descendants(historyPage).OfType<Button>().First(b => AutomationProperties.GetName(b) == name);
            var peer = new ButtonAutomationPeer(button); ((IInvokeProvider)peer.GetPattern(PatternInterface.Invoke)).Invoke(); await Settle();
        }
        try
        {
            savedJobs.Clear(); historySearch = ""; historyFilter = 0; historySort = 0; historyScale = .6;
            // Dedicated A4 content makes visual fidelity measurable, unlike blank fixtures.
            fixture = Path.Combine(output, "audit-report.pdf"); WriteHistoryAuditFixture(fixture);
            var names = new[] { "Laporan inspeksi.pdf", "Dokumentasi pompa.pdf", "Bagian 01.pdf", "Laporan pemeliharaan.pdf", "Pemeriksaan filter.pdf", "Dokumentasi penggantian elemen filter.pdf" };
            var sources = new List<string>();
            for (var i = 0; i < names.Length; i++)
            {
                var path = Path.Combine(output, names[i]); File.Copy(fixture, path, true); sources.Add(path);
                savedJobs.Add(new(Guid.NewGuid(), DateTimeOffset.Now.AddDays(i < 3 ? 0 : -1).AddMinutes(-i * 20), i % 2 == 0 ? "merge" : "split", [path], "Dokumen sintetis untuk audit UI"));
            }
            var hashes = sources.Select(p => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(p)))).ToArray();
            BuildHistoryGallery(); await Settle();
            for (var i = 0; i < 30 && historyRows.Any(r => !r.Attempted); i++) await Task.Delay(100);
            Check(historyPage.Visibility == Visibility.Visible && standardPageScroll!.Visibility == Visibility.Collapsed && standardPageHeader!.Visibility == Visibility.Collapsed, "dedicated viewport, no outer scroll or duplicate breadcrumb");
            Check(historyRows.Count == 6 && historyRows.Take(3).All(r => r.Picture.Source is not null), "real PDF first-page thumbnails loaded");
            Check(historyRows.All(r => r.Frame.ActualHeight >= 88), "detail row height >= 88 DIP");
            Check(historyShell!.ActualHeight > 400 && historyViewport!.ActualHeight < historyShell.ActualHeight, "header and footer fixed, content scroll constrained");
            Check(Descendants(historyPage).OfType<Button>().All(b => !string.IsNullOrWhiteSpace(AutomationProperties.GetName(b))), "all history buttons have accessible names");
            Check(historySearchBox!.Focus(FocusState.Keyboard), "search supports keyboard focus");
            var filterControl = Descendants(historyPage).OfType<ComboBox>().First(b => AutomationProperties.GetName(b) == "Filter alat");
            Check(filterControl.Focus(FocusState.Keyboard), "tool filter supports keyboard focus");
            filterControl.SelectedIndex = 1; await Settle(); Check(historyRows.Count == 3, "tool filter updates result set"); filterControl.SelectedIndex = 0; await Settle();
            var sortControl = Descendants(historyPage).OfType<ComboBox>().First(b => AutomationProperties.GetName(b) == "Urutan dokumen");
            sortControl.SelectedIndex = 2; await Settle(); Check(historyRows[0].Name.Text == "Bagian 01.pdf", "sort by name changes visible order"); sortControl.SelectedIndex = 0; await Settle();
            await Capture("01-initial-details");
            var originalRows = historyRows.Select(r => r.Frame).ToArray();
            await OpenInternalPdfAsync(sources[0]); if (historyReaderRender is not null) await historyReaderRender; await Settle();
            Check(historyPreviewFrame!.Visibility == Visibility.Visible && historyLibrary!.Visibility == Visibility.Visible, "select document opens split preview");
            Check(historyRows.Select(r => r.Frame).SequenceEqual(originalRows), "opening preview retains list identity and thumbnails");
            Check(historyReaderImage!.Source is not null, "reader renders genuine PDF locally");
            Check(historyReaderImage.ActualHeight * historyReaderViewport!.ZoomFactor <= historyReaderViewport.ActualHeight + 2, "initial PDF fits vertically");
            Check(historyReaderImage.ActualWidth * historyReaderViewport.ZoomFactor <= historyReaderViewport.ActualWidth + 2, "initial PDF fits horizontally");
            Check(historyRows[0].Frame.BorderThickness.Left == 3, "selected row has blue left indicator");
            await historyReadingView!.AuditEmbeddingAsync(Check, () => Capture("08-continuous-page-boundary"));
            await historyGoToPage!(0); await Settle();
            Check(historyRows.Select(r => r.Frame).SequenceEqual(originalRows), "continuous scroll retains history rows and selected document");
            Check(Math.Abs(historyCanvas!.TransformToVisual(historyViewport!).TransformPoint(new Windows.Foundation.Point(0, 0)).Y) < 2, "document list stays top aligned when fewer rows fit");
            await Capture("02-split-preview");
            var zoom = historyReaderViewport.ZoomFactor;
            await Click("Perbesar PDF"); Check(historyReaderViewport.ZoomFactor > zoom, "reader zoom button changes only PDF zoom");
            Check(historyScale == .6, "PDF zoom does not change library density");
            var fitting = Descendants(historyPage).OfType<Button>().First(b => AutomationProperties.GetName(b) == "Pilihan sesuaikan halaman atau lebar");
            Check(fitting.Flyout is MenuFlyout { Items.Count: 2 } fitMenu && ((MenuFlyoutItem)fitMenu.Items[0]).Text == "Sesuaikan halaman" && ((MenuFlyoutItem)fitMenu.Items[1]).Text == "Sesuaikan lebar", "fit dropdown contains explicit page and width modes");
            historyReadingView!.FitPreview(false); await historyReadingView.Rendering; await Settle();
            await historyGoToPage!(1); await Settle(); Check(historyReaderImage.Source is not null, "page navigation renders next page");
            await Click("Perluas preview"); Check(historyLibrary!.Visibility == Visibility.Collapsed, "expand hides list without destroying it");
            await Capture("03-expanded-reader");
            await Click("Tampilkan daftar dokumen");
            await Click("Tutup preview"); Check(historyPreviewFrame.Visibility == Visibility.Collapsed && historyRows.Select(r => r.Frame).SequenceEqual(originalRows), "close returns to unchanged document list");
            historySearchBox!.Text = "pompa"; await Settle(); Check(historyRows.Count == 1, "search matches filename");
            await OpenInternalPdfAsync(sources[1]); if (historyReaderRender is not null) await historyReaderRender;
            CloseHistoryPreview(); Check(historySearchBox.Text == "pompa" && historyRows.Count == 1, "close preview preserves search");
            var rapidFirst = OpenInternalPdfAsync(sources[0]); var rapidSecond = OpenInternalPdfAsync(sources[1]);
            await Task.WhenAll(rapidFirst, rapidSecond); if (historyReaderRender is not null) await historyReaderRender;
            Check(historySelectedPath == sources[1], "rapid document selection keeps newest open request");
            CloseHistoryPreview();
            historySearchBox.Text = ""; await Settle();
            await Click("Tampilan kartu"); Check(!historyDetails && historyRows[0].Picture.ActualHeight > 100, "explicit card mode changes real thumbnail layout without changing scale");
            await Capture("04-card-view");
            await Click("Tampilan detail");
            var beforeScale = historyRows.Select(r => r.Frame).ToArray();
            historyViewport!.ChangeView(0, 0, 1.5f, true); await Settle();
            Check(historyDetails && historyScale == .6 && historyRows.Select(r => r.Frame).SequenceEqual(beforeScale), "native detail pinch retains explicit mode and item identities");
            await Click("Tampilan kartu");
            var cardSizes = new List<double>(); var emptyCardFrames = 0; var cardImage = historyRows[0].Picture;
            void CardFrame(object? sender, object e)
            {
                cardSizes.Add(cardImage.ActualWidth * historyViewport.ZoomFactor);
                if (historyRows.All(r => r.Picture.Source is null)) emptyCardFrames++;
            }
            CompositionTarget.Rendering += CardFrame;
            try
            {
                historyViewport.ChangeView(0, 0, 1.5f, false); await Settle();
                for (var attempt = 0; attempt < 20 && (Math.Abs(historyScale - .9) > .01 || Math.Abs(historyViewport.ZoomFactor - 1) > .001); attempt++)
                { await Task.Delay(100); root.UpdateLayout(); }
            }
            finally { CompositionTarget.Rendering -= CardFrame; }
            await File.WriteAllTextAsync(Path.Combine(output, "card-zoom-transition.json"), System.Text.Json.JsonSerializer.Serialize(new { cardSizes, emptyCardFrames }));
            Check(cardSizes.Count > 0 && emptyCardFrames == 0, "history native card transition retains thumbnails");
            Check(cardSizes.Zip(cardSizes.Skip(1), (a, b) => b >= a - 2).All(x => x), "history native commit has no backward size flash");
            Check(!historyDetails && Math.Abs(historyScale - .9) < .01 && Math.Abs(historyViewport.ZoomFactor - 1) < .001 && historyRows.Select(r => r.Frame).SequenceEqual(beforeScale), "native card zoom reflows once without changing explicit mode or replacing cards");
            await Click("Tampilan detail");
            await OpenInternalPdfAsync(sources[0]); if (historyReaderRender is not null) await historyReaderRender;
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1024, 768)); await Settle(); await Capture("05-compact-split");
            AppWindow.Resize(new Windows.Graphics.SizeInt32(640, 720)); await Settle();
            Check(historyLibrary!.Visibility == Visibility.Collapsed && historyPreviewFrame!.ActualWidth <= historyPage.ActualWidth, "narrow window keeps reader usable with close action");
            await Capture("06-narrow-reader");
            CloseHistoryPreview(); await Settle(); await Capture("07-narrow-list");
            ShowHome(); await Settle(); Check(historyPage.Visibility == Visibility.Collapsed && standardPageScroll!.Visibility == Visibility.Visible, "navigation restores normal application shell");
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1480, 960));
            BuildHistoryGallery(); await Settle();
            await OpenInternalPdfAsync(sources[0]); if (historyReaderRender is not null) await historyReaderRender;
            var selectedJob = savedJobs.First(j => j.Paths[0] == sources[0]); RemoveHistoryJob(selectedJob); await Settle();
            Check(historyPreviewFrame!.Visibility == Visibility.Collapsed && File.Exists(sources[0]) && savedJobs.Count == 5, "remove selected history record closes preview but retains PDF");
            var missing = Path.Combine(output, "tidak-ada.pdf");
            savedJobs.Add(new(Guid.NewGuid(), DateTimeOffset.Now, "merge", [missing], "Missing fixture")); drawHistory?.Invoke(); await Settle();
            Check(historyRows.First(r => r.Path == missing).Inline.Text == "File tidak ditemukan", "missing files have explicit status");
            await OpenInternalPdfAsync(missing); Check(historyNotice!.Text.Contains("tidak dapat dibuka", StringComparison.Ordinal), "missing file open reports error without closing application");
            await OpenInternalPdfAsync(Path.ChangeExtension(sources[0], ".jpg")); Check(historyNotice.Text.Contains("format", StringComparison.Ordinal), "non-PDF result directs user to folder");
            savedJobs.Clear(); drawHistory?.Invoke(); await Settle();
            Check(historyRows.Count == 0 && historyCanvas!.Children.OfType<TextBlock>().Any(t => t.Text.Contains("Belum ada riwayat", StringComparison.Ordinal)), "empty history offers clear next action");
            var cancelled = OpenInternalPdfAsync(sources[0]); CloseHistoryPreview(); await cancelled;
            Check(historyPreviewFrame!.Visibility == Visibility.Collapsed, "closing preview invalidates an in-flight document open");
            var pending = OpenInternalPdfAsync(sources[0]); ShowCatalog(); await pending;
            Check(historyPage.Visibility == Visibility.Collapsed, "late PDF load cannot reopen history after navigation");
            Check(sources.Select(p => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(p)))).SequenceEqual(hashes), "all source PDFs unchanged");
            report.Add("MANUAL_REQUIRED physical touchpad gesture and keyboard traversal; no production/UAT approval");
        }
        catch (Exception error) { report.Add("FAIL " + error.GetType().Name + ": " + error.Message); Environment.ExitCode = 1; }
        finally { libraryTestPath = null; await File.WriteAllLinesAsync(Path.Combine(output, "audit.txt"), report); }
    }

    private static void WriteHistoryAuditFixture(string path, int width = 595, int height = 842, bool editableText = false)
    {
        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>", "<< /Type /Pages /Kids [4 0 R 6 0 R 8 0 R] /Count 3 >>",
            editableText ? "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"
                : "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };
        for (var page = 1; page <= 3; page++)
        {
            objects.Add(FormattableString.Invariant($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {width} {height}] /Resources << /Font << /F1 3 0 R >> >> /Contents {objects.Count + 2} 0 R >>"));
            var content = new System.Text.StringBuilder();
            content.AppendLine("0.09 0.17 0.27 rg BT /F1 13 Tf 48 786 Td (PERTAMINA OTO) Tj ET");
            content.AppendLine("0.08 0.39 0.80 rg 48 765 499 3 re f");
            content.AppendLine("0.09 0.17 0.27 rg BT /F1 19 Tf 181 716 Td (LAPORAN INSPEKSI) Tj ET");
            content.AppendLine("BT /F1 11 Tf 193 695 Td (Peralatan Operasional) Tj ET");
            var sections = new[] { "INFORMASI UMUM", "HASIL PEMERIKSAAN", "TINDAK LANJUT" };
            var fields = new[] { "Lokasi", "Tanggal inspeksi", "Peralatan", "Tim inspeksi", "Kondisi", "Kinerja", "Kebocoran", "Rekomendasi", "Pekerjaan", "Penanggung jawab", "Status", "Catatan" };
            var values = new[] { "Terminal BBM - Area operasional", "17 September 2026", "Pompa transfer BBM", "Tim Teknik Operasional", "Baik", "Sesuai spesifikasi", "Tidak ditemukan", "Pemeliharaan berkala", "Pemantauan rutin", "Tim pemeliharaan", "Terjadwal", "Dokumen sintetis - bukan data operasional" };
            for (var section = 0; section < 3; section++)
            {
                var y = 643 - section * 163;
                content.AppendLine(FormattableString.Invariant($"0.86 0.9 0.94 rg 48 {y} 499 24 re f 0.09 0.17 0.27 rg BT /F1 11 Tf 56 {y + 8} Td ({sections[section]}) Tj ET"));
                for (var line = 0; line < 4; line++)
                {
                    var rowY = y - 27 * (line + 1); var i = section * 4 + line;
                    content.AppendLine(FormattableString.Invariant($"0.84 0.88 0.92 RG 0.5 w 48 {rowY} 499 27 re S 225 {rowY} m 225 {rowY + 27} l S 0.12 0.18 0.25 rg BT /F1 10 Tf 56 {rowY + 10} Td ({fields[i]}) Tj ET BT /F1 10 Tf 232 {rowY + 10} Td ({values[i]}) Tj ET"));
                }
            }
            content.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"0.42 0.47 0.55 rg BT /F1 9 Tf 48 54 Td (CONTOH UNTUK AUDIT UI - Halaman {page} dari 3) Tj ET");
            var stream = content.ToString(); objects.Add($"<< /Length {System.Text.Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}endstream");
        }
        var pdf = new System.Text.StringBuilder("%PDF-1.4\n"); var offsets = new List<int> { 0 };
        for (var i = 0; i < objects.Count; i++) { offsets.Add(System.Text.Encoding.ASCII.GetByteCount(pdf.ToString())); pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"{i + 1} 0 obj\n{objects[i]}\nendobj\n"); }
        var xref = System.Text.Encoding.ASCII.GetByteCount(pdf.ToString()); pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1)) pdf.Append(offset.ToString("D10", System.Globalization.CultureInfo.InvariantCulture) + " 00000 n \n");
        pdf.Append(System.Globalization.CultureInfo.InvariantCulture, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        File.WriteAllBytes(path, System.Text.Encoding.ASCII.GetBytes(pdf.ToString()));
    }
}
#endif
