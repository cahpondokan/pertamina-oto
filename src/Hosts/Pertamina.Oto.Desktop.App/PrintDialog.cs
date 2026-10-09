using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
        private bool printOpen;
#if DEBUG
    private Func<string[]>? auditPrintPrinters;
    private Func<string, string, Task>? auditPrintNotice;
    private Func<PrintWorker.Request, Action<int>, CancellationToken, Task<int>>? auditPrintWorker;
    private string? auditPrintStatus;
#endif
    private Task PrintNoticeAsync(string title, string text)
    {
#if DEBUG
        if (auditPrintNotice is not null) return auditPrintNotice(title, text);
#endif
        return MenuMessageAsync(title, text);
    }
    private sealed partial class PdfReaderView { internal bool CanPrint => document is not null && !busy; }
    private async Task PrintActiveAsync()
    {
        if (liveEditor?.Visible == true) { await liveEditor.PrintExportAsync(); return; }
        if (pdfReader?.Visible == true && pdfReader.ReadingDocument is not null && pdfReader.CanPrint && pdfReader.ReaderSource is { } path)
        { await ShowPrintAsync(path, readerResults.Contains(path) ? "Hasil proses tersimpan" : "Dokumen pembaca", pdfReader.ReadingPage); return; }
        await MenuMessageAsync("Cetak", "Buka PDF valid dan tunggu pemuatan selesai untuk mencetak.");
    }
    private sealed partial class LiveEditor
    {
        internal async Task PrintExportAsync()
        {
            if (Working) return;
            if (Dirty || lastResult is null)
            {
                await owner.PrintNoticeAsync("Simpan hasil edit dahulu", "Cetak memerlukan hasil PDF terbaru yang sudah diekspor. Gunakan Simpan PDF baru, lalu Cetak hasil. Draf dan undo tetap tersedia."); return;
            }
            await owner.ShowPrintAsync(lastResult, "Hasil edit tersimpan", page);
        }
    }
    private async Task ShowPrintAsync(string path, string source, int current = 0)
    {
        if (printOpen || messageOpen || active is not null || importing || liveEditor?.Working == true) return;
        printOpen = true; messageOpen = true;
        string? snapshot = null; PrintDevice? device = null;
        var dialog = new ContentDialog { XamlRoot = root.XamlRoot, Title = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Children = { new FontIcon { Glyph = "\uE749", FontSize = 28 }, HistoryText("Cetak dokumen", 22, true) } }, DefaultButton = ContentDialogButton.None, IsPrimaryButtonEnabled = false };
        dialog.Resources["ContentDialogMaxWidth"] = Math.Max(280, root.ActualWidth - 48);
        var submit = new Button { Content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { new FontIcon { Glyph = "\uE749", FontSize = 16 }, new TextBlock { Text = "Cetak" } } }, MinWidth = 104, MinHeight = 40, IsEnabled = false, Background = Brush(8, 102, 232), Foreground = Brush(255, 255, 255) };
        var cancelButton = new Button { Content = "Batal", MinWidth = 96, MinHeight = 40 };
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(submit, "Cetak dokumen");
        void SetSubmit(bool enabled) { dialog.IsPrimaryButtonEnabled = enabled; submit.IsEnabled = enabled; }
        var notice = HistoryText("Menyiapkan dokumen dan printer…", 13, muted: true);
        var jobNotice = HistoryText("", 13, true); jobNotice.Visibility = Visibility.Collapsed;
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(jobNotice, "Status pengiriman cetak");
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetLiveSetting(jobNotice, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
        var content = new Grid { RowSpacing = 12, Width = Math.Max(280, Math.Min(940, root.ActualWidth - 100)), Height = Math.Max(100, Math.Min(680, root.ActualHeight - 170)) };
        content.RowDefinitions.Add(new() { Height = GridLength.Auto }); content.RowDefinitions.Add(new()); content.RowDefinitions.Add(new() { Height = GridLength.Auto });
        var identity = new StackPanel { Spacing = 4 }; var documentName = HistoryText(System.IO.Path.GetFileName(path), 16, true); documentName.MaxLines = 2; documentName.TextTrimming = TextTrimming.CharacterEllipsis; identity.Children.Add(documentName); var documentDetails = HistoryText(source + " · File tetap di perangkat", 12, muted: true); documentDetails.MaxLines = 2; documentDetails.TextTrimming = TextTrimming.CharacterEllipsis; identity.Children.Add(documentDetails); content.Children.Add(identity);
        var columns = new Grid { ColumnSpacing = 20 }; columns.ColumnDefinitions.Add(new() { Width = new GridLength(300) }); columns.ColumnDefinitions.Add(new()); Grid.SetRow(columns, 1); content.Children.Add(columns);
        var settings = new StackPanel { Spacing = 12 }; columns.Children.Add(new ScrollViewer { Content = settings, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled });
        ComboBox Choice(string header, params string[] values) { var c = new ComboBox { Header = header, HorizontalAlignment = HorizontalAlignment.Stretch }; foreach (var v in values) c.Items.Add(v); if (values.Length > 0) c.SelectedIndex = 0; settings.Children.Add(c); return c; }
        var printers = Choice("Tujuan cetak"); var printerStatus = HistoryText("", 12, muted: true); settings.Children.Add(printerStatus);
        var preferences = new Button { Content = "Preferensi printer", HorizontalAlignment = HorizontalAlignment.Right }; settings.Children.Add(preferences);
        var selection = Choice("Halaman", "Semua halaman", "Halaman saat ini", "Rentang halaman");
        var custom = new TextBox { PlaceholderText = "Contoh: 1, 3–5", MaxLength = 2048, Visibility = Visibility.Collapsed }; settings.Children.Add(custom);
        var copies = new NumberBox { Header = "Salinan", Minimum = 1, Maximum = 99, Value = 1, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline }; settings.Children.Add(copies);
        var papers = Choice("Kertas"); var orientation = Choice("Orientasi", "Potret", "Lanskap"); var paperRow = new Grid { ColumnSpacing = 12 }; paperRow.ColumnDefinitions.Add(new()); paperRow.ColumnDefinitions.Add(new()); settings.Children.Remove(papers); settings.Children.Remove(orientation); paperRow.Children.Add(papers); Grid.SetColumn(orientation, 1); paperRow.Children.Add(orientation); settings.Children.Add(paperRow); var scaling = Choice("Skala cetak", "Sesuaikan area cetak", "Ukuran sebenarnya");
        settings.Children.Add(HistoryText("Skala cetak terpisah dari zoom pembaca. Ukuran sebenarnya dapat terpotong pada batas printer.", 12, muted: true));
        var color = new CheckBox { Content = "Warna", IsChecked = true }; var duplex = new ComboBox { Header = "Dua sisi", HorizontalAlignment = HorizontalAlignment.Stretch }; duplex.Items.Add("Satu sisi"); duplex.Items.Add("Balik sisi panjang"); duplex.Items.Add("Balik sisi pendek"); duplex.SelectedIndex = 0;
        var advanced = new StackPanel { Spacing = 8 }; advanced.Children.Add(color); advanced.Children.Add(duplex); settings.Children.Add(new Expander { Header = "Pengaturan lanjutan", Content = advanced, HorizontalAlignment = HorizontalAlignment.Stretch });
        var preview = new Grid { Background = Brush(237, 242, 248), Padding = new Thickness(14), RowSpacing = 8 }; Grid.SetColumn(preview, 1); columns.Children.Add(preview);
        preview.RowDefinitions.Add(new() { Height = GridLength.Auto }); preview.RowDefinitions.Add(new()); preview.RowDefinitions.Add(new() { Height = GridLength.Auto });
        preview.Children.Add(HistoryText("Pratinjau cetak", 15, true));
        var sheet = new Canvas { Background = Brush(255, 255, 255) }; var image = new Image { Stretch = Stretch.Fill }; var boundary = new Rectangle { Stroke = Brush(122, 150, 188), StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 4, 3 }, IsHitTestVisible = false }; sheet.Children.Add(image); sheet.Children.Add(boundary);
        var sheetHost = new Viewbox { Child = sheet, Stretch = Stretch.Uniform, Margin = new Thickness(12) }; Grid.SetRow(sheetHost, 1); preview.Children.Add(sheetHost);
        var nav = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, HorizontalAlignment = HorizontalAlignment.Center }; var prev = new Button { Content = "‹" }; var next = new Button { Content = "›" }; var counter = HistoryText("", 12); nav.Children.Add(prev); nav.Children.Add(counter); nav.Children.Add(next); Grid.SetRow(nav, 2); preview.Children.Add(nav);
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(prev, "Halaman pratinjau sebelumnya");
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(next, "Halaman pratinjau berikutnya");
        var footer = new StackPanel { Spacing = 4 }; var summary = HistoryText("", 13, true); summary.MaxLines = 2; summary.TextTrimming = TextTrimming.CharacterEllipsis; footer.Children.Add(summary); footer.Children.Add(notice); footer.Children.Add(jobNotice); var footerGrid = new Grid { ColumnSpacing = 16, Padding = new Thickness(0, 12, 0, 0), BorderBrush = Brush(220, 229, 241), BorderThickness = new Thickness(0, 1, 0, 0) }; footerGrid.ColumnDefinitions.Add(new()); footerGrid.ColumnDefinitions.Add(new() { Width = GridLength.Auto }); footerGrid.Children.Add(footer); var footerButtons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Bottom }; footerButtons.Children.Add(cancelButton); footerButtons.Children.Add(submit); Grid.SetColumn(footerButtons, 1); footerGrid.Children.Add(footerButtons); Grid.SetRow(footerGrid, 2); content.Children.Add(footerGrid); dialog.Content = content;
        var compactPreview = new Expander { Header = "Pratinjau cetak", HorizontalAlignment = HorizontalAlignment.Stretch };
        // Small windows retain settings and footer; preview remains available as a compact sheet.
        if (content.Width < 650) { columns.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star); columns.ColumnDefinitions[1].Width = new GridLength(content.Width < 450 ? 0 : 150); }
        // Reserve title/template space, then pin the footer outside the scrolling settings body.
        // Re-measure on root resize and text/layout changes rather than retaining the opening height.
        void AdaptPrintLayout()
        {
            var size = root.XamlRoot.Size;
            content.Width = Math.Max(260, Math.Min(940, size.Width - 100));
            var titleHeight = ((FrameworkElement)dialog.Title).ActualHeight;
            content.Height = Math.Max(100, Math.Min(680, size.Height - Math.Max(170, titleHeight + 120)));
            // In a short window the job result takes priority over the repeated settings summary.
            summary.Visibility = jobNotice.Visibility == Visibility.Visible && size.Height < 650 ? Visibility.Collapsed : Visibility.Visible;
            dialog.Resources["ContentDialogMaxHeight"] = Math.Max(200, size.Height - 32);
            dialog.Resources["ContentDialogMaxWidth"] = Math.Max(280, size.Width - 48);
            var compact = content.Width < 650;
            if (content.Width < 450 && columns.Children.Contains(preview))
            { columns.Children.Remove(preview); compactPreview.Content = preview; settings.Children.Add(compactPreview); preview.Height = 280; }
            else if (content.Width >= 450 && settings.Children.Contains(compactPreview))
            { settings.Children.Remove(compactPreview); compactPreview.Content = null; columns.Children.Add(preview); preview.Height = double.NaN; }
            columns.ColumnDefinitions[0].Width = compact ? new GridLength(1, GridUnitType.Star) : new GridLength(300);
            columns.ColumnDefinitions[1].Width = new GridLength(compact ? (content.Width < 450 ? 0 : 150) : 1, compact ? GridUnitType.Pixel : GridUnitType.Star);
            if (footerGrid.RowDefinitions.Count == 0) { footerGrid.RowDefinitions.Add(new() { Height = GridLength.Auto }); footerGrid.RowDefinitions.Add(new() { Height = GridLength.Auto }); }
            Grid.SetRow(footerButtons, compact ? 1 : 0); Grid.SetColumn(footerButtons, compact ? 0 : 1);
            Grid.SetColumnSpan(footerButtons, compact ? 2 : 1); Grid.SetColumnSpan(footer, compact ? 2 : 1);
            footerButtons.HorizontalAlignment = HorizontalAlignment.Right;
        }
        void OnPrintRootChanged(object? sender, XamlRootChangedEventArgs args) => AdaptPrintLayout();
        root.XamlRoot.Changed += OnPrintRootChanged;
        dialog.SizeChanged += (_, _) => AdaptPrintLayout();
        jobNotice.SizeChanged += (_, _) => AdaptPrintLayout();
        AdaptPrintLayout();
        PdfDocument? pdf = null; FileStream? sourceLock = null; int cursor = 0, version = 0, printerGeneration = 0; int[] pages = []; bool refreshing = false, sending = false; CancellationTokenSource? cancellation = null;
        async Task Refresh()
        {
            if (pdf is null || refreshing || sending) return;
            var ticket = ++version;
            SetSubmit(false);
            notice.Visibility = Visibility.Visible;
            try
            {
                pages = PrintPlan.Pages((string)selection.SelectedItem, custom.Text, (int)pdf.PageCount, current);
                if (device is null || papers.SelectedItem is not PrintDevice.Paper paper) { notice.Text = "Tidak ada printer yang tersedia. Periksa Windows atau sambungan printer."; return; }
                if (!double.IsFinite(copies.Value) || copies.Value != Math.Truncate(copies.Value) || copies.Value < 1 || copies.Value > 99 || copies.Value * pages.Length > 10000) throw new InvalidOperationException("Salinan harus 1–99; maksimal 10.000 lembar.");
                device.Configure(paper.Id, orientation.SelectedIndex == 1, color.IsChecked == true, (short)(duplex.SelectedIndex + 1));
                var b = device.Geometry(); cursor = Math.Clamp(cursor, 0, pages.Length - 1);
                using var page = pdf.GetPage((uint)pages[cursor]); var p = PrintPlan.Placement(page.Size.Width, page.Size.Height, b, scaling.SelectedIndex == 1);
                double X(double value) => value * 96 / b.DpiX; double Y(double value) => value * 96 / b.DpiY;
                sheet.Width = X(b.Width); sheet.Height = Y(b.Height); image.Clip = new RectangleGeometry { Rect = new Windows.Foundation.Rect(X(b.Left - p.X), Y(b.Top - p.Y), X(b.PrintableWidth), Y(b.PrintableHeight)) };
                image.Width = X(p.W); image.Height = Y(p.H); Canvas.SetLeft(image, X(p.X)); Canvas.SetTop(image, Y(p.Y));
                boundary.Width = X(b.PrintableWidth); boundary.Height = Y(b.PrintableHeight); boundary.StrokeThickness = Math.Max(1, sheet.Width / 400d); Canvas.SetLeft(boundary, X(b.Left)); Canvas.SetTop(boundary, Y(b.Top));
                var raster = PrintPlan.RasterSize(page.Size.Width, page.Size.Height, 800, 1600, 2_000_000);
                using var stream = new InMemoryRandomAccessStream(); await page.RenderToStreamAsync(stream, new PdfPageRenderOptions { DestinationWidth = raster.Width, DestinationHeight = raster.Height }); stream.Seek(0); var bitmap = new BitmapImage(); await bitmap.SetSourceAsync(stream);
                if (ticket != version) return; image.Source = bitmap;
                counter.Text = $"{cursor + 1} dari {pages.Length}"; prev.IsEnabled = cursor > 0; next.IsEnabled = cursor + 1 < pages.Length;
                printerStatus.Text = device.Status;
#if DEBUG
                if (auditPrintStatus is not null) printerStatus.Text = auditPrintStatus;
#endif
                summary.Text = $"{pages.Length} halaman · {copies.Value:0} salinan · {paper.Name} · {orientation.SelectedItem}";
                notice.Text = printerStatus.Text.Contains("Offline", StringComparison.Ordinal) || printerStatus.Text.Contains("perhatian", StringComparison.Ordinal)
                    ? "Windows melaporkan printer offline/perlu perhatian. Periksa sambungan/antrean; pilih ulang printer."
                    : "Printer terdaftar di Windows; keterjangkauan belum diverifikasi. Periksa sambungan dan antrean bila pengiriman gagal.";
                if (jobNotice.Visibility == Visibility.Visible && !printerStatus.Text.Contains("Offline", StringComparison.Ordinal) && !printerStatus.Text.Contains("perhatian", StringComparison.Ordinal)) notice.Visibility = Visibility.Collapsed;
                SetSubmit(!printerStatus.Text.Contains("Offline", StringComparison.Ordinal) && !printerStatus.Text.Contains("perhatian", StringComparison.Ordinal));
#if DEBUG
                dialog.Tag = device.Name;
#endif
            }
            catch (Exception error) { LocalDiagnostics.Record("print-preview", error); notice.Text = error is InvalidOperationException ? error.Message : "Pratinjau tidak tersedia. Periksa dokumen dan driver."; }
        }
        async Task PrinterChanged()
        {
            if (sending) return;
            var printerTicket = ++printerGeneration; version++;
            refreshing = true; SetSubmit(false); device?.Dispose(); device = null;
            try
            {
                if (printers.SelectedItem is not string name) return;
                var candidate = await Task.Run(() => new PrintDevice(name));
                if (printerTicket != printerGeneration) { candidate.Dispose(); return; }
                device = candidate; papers.Items.Clear(); foreach (var paper in device.Papers) papers.Items.Add(paper);
                papers.SelectedItem = device.Papers.FirstOrDefault(p => p.Id == device.PaperId) ?? device.Papers[0]; orientation.SelectedIndex = device.Orientation == 2 ? 1 : 0; color.IsChecked = device.ColorMode == 2; duplex.SelectedIndex = Math.Clamp(device.DuplexMode - 1, 0, 2);
                color.Visibility = device.Color ? Visibility.Visible : Visibility.Collapsed; duplex.Visibility = device.Duplex ? Visibility.Visible : Visibility.Collapsed; orientation.IsEnabled = device.Landscape;
            }
            catch (Exception error) { LocalDiagnostics.Record("print-printer", error); if (printerTicket == printerGeneration) notice.Text = "Printer atau driver tidak dapat diakses. Pilih printer lain."; }
            finally { if (printerTicket == printerGeneration) { refreshing = false; await Refresh(); } }
        }
        printers.SelectionChanged += async (_, _) => await PrinterChanged();
        foreach (var c in new[] { selection, papers, orientation, scaling, duplex }) c.SelectionChanged += async (_, _) => { custom.Visibility = selection.SelectedIndex == 2 ? Visibility.Visible : Visibility.Collapsed; cursor = 0; await Refresh(); };
        custom.TextChanged += async (_, _) => await Refresh(); copies.ValueChanged += async (_, _) => await Refresh(); color.Click += async (_, _) => await Refresh();
        prev.Click += async (_, _) => { cursor--; await Refresh(); }; next.Click += async (_, _) => { cursor++; await Refresh(); };
        preferences.Click += async (_, _) => { if (device is null || sending) return; try { device.Preferences(WinRT.Interop.WindowNative.GetWindowHandle(this)); refreshing = true; papers.SelectedItem = device.Papers.FirstOrDefault(p => p.Id == device.PaperId) ?? device.Papers[0]; orientation.SelectedIndex = device.Orientation == 2 ? 1 : 0; color.IsChecked = device.ColorMode == 2; duplex.SelectedIndex = Math.Clamp(device.DuplexMode - 1, 0, 2); } catch (Exception error) { LocalDiagnostics.Record("print-preferences", error); notice.Text = "Preferensi driver tidak dapat dibuka."; } finally { refreshing = false; } await Refresh(); };
        void CancelSubmission() { cancellation?.Cancel(); jobNotice.Text = "Membatalkan pengiriman… Tunggu hasil sebelum mencoba lagi."; }
        dialog.Closing += (_, args) => { if (sending) { args.Cancel = true; CancelSubmission(); } };
        cancelButton.Click += (_, _) => { if (sending) CancelSubmission(); else dialog.Hide(); };
        dialog.KeyDown += (_, args) => { if (args.Key == Windows.System.VirtualKey.Escape) { args.Handled = true; if (sending) CancelSubmission(); else dialog.Hide(); } };
        submit.Click += async (_, _) =>
        {
            if (sending || device is null || pdf is null) return;
            if (!Equals(printers.SelectedItem, device.Name)) { notice.Text = "Tunggu pemuatan printer yang dipilih."; return; }
            var controls = new Control[] { printers, preferences, selection, custom, copies, papers, orientation, scaling, color, duplex, prev, next };
            sending = true; foreach (var control in controls) control.IsEnabled = false; SetSubmit(false); cancellation = new();
            notice.Text = ""; notice.Visibility = Visibility.Collapsed; jobNotice.Visibility = Visibility.Visible; jobNotice.Text = "Mengirim… Menunggu tanggapan antrean Windows."; cancelButton.Content = "Batal"; AdaptPrintLayout();
            try
            {
                var selectedPages = pages.ToArray(); var selectedCopies = (int)copies.Value; var actualSize = scaling.SelectedIndex == 1;
                var request = new PrintWorker.Request(snapshot!, device.Name, device.ExportMode(), selectedPages, selectedCopies, actualSize, null);
                Func<PrintWorker.Request, Action<int>, CancellationToken, Task<int>> worker = PrintWorker.SubmitAsync;
#if DEBUG
                if (auditPrintWorker is not null) worker = auditPrintWorker;
#endif
                var attempt = cancellation;
                var job = await worker(request, n => DispatcherQueue.TryEnqueue(() => { if (sending && ReferenceEquals(cancellation, attempt) && !attempt.IsCancellationRequested) jobNotice.Text = $"Mengirim halaman {n}…"; }), cancellation.Token);
                jobNotice.Text = $"Diterima antrean Windows · pekerjaan {job}. Kertas tercetak belum terverifikasi. Periksa antrean sebelum Cetak lagi.";
                ((TextBlock)((StackPanel)submit.Content).Children[1]).Text = "Cetak lagi";
            }
            catch (OperationCanceledException) { jobNotice.Text = "Dibatalkan. Halaman yang sudah diterima mungkin tetap tercetak. Periksa antrean Windows sebelum Coba lagi."; ((TextBlock)((StackPanel)submit.Content).Children[1]).Text = "Coba lagi"; }
            catch (Exception error) { LocalDiagnostics.Record("print-submit", error); jobNotice.Text = "Gagal: " + PrintWorker.SafeFailureReason(error) + " Periksa sambungan dan antrean Windows sebelum Coba lagi."; ((TextBlock)((StackPanel)submit.Content).Children[1]).Text = "Coba lagi"; }
            finally { cancellation.Dispose(); cancellation = null; sending = false; foreach (var control in controls) control.IsEnabled = true; orientation.IsEnabled = device?.Landscape == true; prev.IsEnabled = cursor > 0; next.IsEnabled = cursor + 1 < pages.Length; cancelButton.Content = "Tutup"; await Refresh(); }
        };
        try
        {
            // Exclusive read lock through the entire dialog: missing/changed files cannot silently switch identity.
            sourceLock = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (sourceLock.Length == 0 || sourceLock.Length > 512L * 1024 * 1024) throw new InvalidOperationException("Batas cetak adalah PDF hingga 512 MB.");
            var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "PertaminaOTO-Print"); Directory.CreateDirectory(directory); snapshot = System.IO.Path.Combine(directory, Guid.NewGuid().ToString("N") + ".pdf");
            using (var target = new FileStream(snapshot, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true)) await sourceLock.CopyToAsync(target);
            if (await PrintSecurity.HasEncryptionAsync(snapshot)) throw new InvalidOperationException("PDF dilindungi. Gunakan hasil tanpa proteksi yang Anda berwenang cetak.");
            var snapshotPath = snapshot; pdf = await Task.Run(async () => await PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(snapshotPath)));
            if (pdf.IsPasswordProtected) throw new InvalidOperationException("PDF dilindungi. Cetak dokumen tanpa proteksi yang Anda berwenang gunakan.");
            ((TextBlock)identity.Children[1]).Text = $"{source} · {pdf.PageCount} halaman · {HistoryFileSize(sourceLock.Length)} · PDF diproses lokal; printer dapat memerlukan jaringan";
            if (pdf.PageCount == 0 || pdf.PageCount > 10000) throw new InvalidOperationException("Jumlah halaman tidak didukung.");
            foreach (var name in await Task.Run(() => {
#if DEBUG
                if (auditPrintPrinters is not null) return auditPrintPrinters();
#endif
                return PrintDevice.Names(); })) printers.Items.Add(name);
            if (printers.Items.Count > 0) printers.SelectedIndex = 0; else notice.Text = "Tidak ada printer tersedia di Windows.";
            #if DEBUG
            dialog.Opened += async (_, _) => { if (auditPrintDialog is not null) await auditPrintDialog(dialog); };
#endif
            await dialog.ShowAsync();
        }
        catch (Exception error) { LocalDiagnostics.Record("print-open", error); messageOpen = false; await PrintNoticeAsync("Cetak tidak tersedia", error is InvalidOperationException ? error.Message : "PDF tidak dapat dibaca atau printer Windows tidak tersedia. File asli tetap aman."); }
        finally { root.XamlRoot.Changed -= OnPrintRootChanged; version++; printerGeneration++; device?.Dispose(); sourceLock?.Dispose(); if (snapshot is not null) try { File.Delete(snapshot); } catch (Exception error) { LocalDiagnostics.Record("print-cleanup", error); } printOpen = false; messageOpen = false; }
    }
}
