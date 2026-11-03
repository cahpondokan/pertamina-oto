using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private Border? latestResultCard;
    private void ShowUnifiedResult(string[] paths, string detail, Action back, StackPanel? host = null, Action? restart = null, Action? preview = null)
    {
        if (paths.Length == 0) return;
        foreach (var result in paths.Where(p => Path.GetExtension(p).Equals(".pdf", StringComparison.OrdinalIgnoreCase))) readerResults.Add(Path.GetFullPath(result));
        if (host is null)
        {
            RememberResult(paths, detail);
            // Results use the standard workspace, never the history overlay.
            // A completion arriving after navigation must remain visible.
            SetHistoryPageVisible(false);
            breadcrumb.Text = "Ruang kerja  /  Hasil tersimpan";
        }
        var destination = host ?? body;
        destination.Children.Clear(); if (host is null) organizerVisible = false;
        var panel = new StackPanel { Spacing = 20, Padding = new Thickness(28) };
        var title = paths.Length > 1 ? $"{paths.Length} file berhasil disimpan" : Path.GetExtension(paths[0]).Equals(".jpg", StringComparison.OrdinalIgnoreCase) ? "Gambar berhasil disimpan" : "PDF berhasil disimpan";
        panel.Children.Add(new TextBlock { Text = "✓  " + title, FontSize = 26, Foreground = Brush(17, 115, 82), TextWrapping = TextWrapping.Wrap });
        panel.Children.Add(Copy("Hasil sudah siap digunakan.", 14));
        var notice = Copy("", 13);
        AutomationProperties.SetLiveSetting(notice, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
        async Task Open(string path)
        {
            try
            {
                if (!Available(path)) return;
                var ok = await Launcher.LaunchFileAsync(await StorageFile.GetFileFromPathAsync(path));
                if (!ok) notice.Text = "Windows tidak dapat membuka hasil. Periksa lokasi file.";
            }
            catch (Exception error) { LocalDiagnostics.Record("open-result", error); notice.Text = "Hasil tidak dapat dibuka. File mungkin telah dipindahkan."; }
        }
        bool Available(string path)
        {
            if (File.Exists(path)) { notice.Text = ""; return true; }
            notice.Text = "File hasil tidak ditemukan. File mungkin telah dipindahkan atau dihapus."; return false;
        }
        async Task ReadResult(string path) { if (Available(path)) await OpenPdfReaderAsync(path, processed: true); }
        async Task PrintResult(string path) { if (Available(path)) await ShowPrintAsync(path, "Hasil proses tersimpan"); }
        void Reveal(string path)
        {
            if (!Available(path)) return;
            try { ResultFileReveal.Select(path); }
            catch (Exception error) { LocalDiagnostics.Record("reveal-result", error); notice.Text = "File hasil tidak dapat ditampilkan dengan seleksi di Explorer. Periksa apakah file telah dipindahkan atau dihapus."; }
        }
        Button Command(string label, Action action, bool primary = false)
        {
            var button = new Button { Content = label, MinHeight = 44, Padding = new Thickness(18, 10, 18, 10), CornerRadius = new CornerRadius(7) };
            AutomationProperties.SetName(button, label);
            if (label.StartsWith("Cetak", StringComparison.Ordinal)) button.Content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { new FontIcon { Glyph = "\uE749", FontSize = 18 }, new TextBlock { Text = label } } };
            if (primary) { button.Background = Brush(23, 104, 212); button.Foreground = Brush(255, 255, 255); }
            button.Click += (_, _) => action(); return button;
        }
        var list = new StackPanel { Spacing = 8 };
        foreach (var path in paths)
        {
            var row = new StackPanel { Spacing = 4, Padding = new Thickness(12) };
            if (paths.Length > 1 && Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                { row.Children.Add(Command("Buka hasil", async () => await ReadResult(path))); row.Children.Add(Command("Cetak hasil ini", async () => await PrintResult(path))); }
            var name = Copy(Path.GetFileName(path), 15); name.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold; name.MaxLines = 2; name.IsTextSelectionEnabled = true;
            ToolTipService.SetToolTip(name, Path.GetFileName(path)); row.Children.Add(name);
            if (paths.Length == 1) row.Children.Add(Copy(detail, 13));
            else row.Children.Add(Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase)
                ? Command("Preview hasil", async () => await ShowReadPreviewDialogAsync(path))
                : Command("Buka file ↗", async () => await Open(path)));
            if (paths.Length > 1) row.Children.Add(Command("Tampilkan di folder", () => Reveal(path)));
            list.Children.Add(new Border { Child = row, Background = Brush(244, 247, 251), CornerRadius = new CornerRadius(8) });
        }
        var resultList = new ScrollViewer { Content = list, MaxHeight = 260, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled }; panel.Children.Add(resultList);
        var directory = Path.GetDirectoryName(paths[0])!;
        var actions = new ResponsiveWrapPanel { Spacing = 12 };
        var pdfResult = paths.Length == 1 && Path.GetExtension(paths[0]).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        if (paths.Length == 1) actions.Children.Add(Command(pdfResult || preview is not null ? "Buka hasil" : "↗  Buka file",
            pdfResult ? async () => await ReadResult(paths[0]) : preview ?? (async () => await Open(paths[0])), true));
        if (pdfResult) actions.Children.Add(Command("Cetak hasil", async () => await PrintResult(paths[0])));
        if (paths.Length == 1) actions.Children.Add(Command("Tampilkan di folder", () => Reveal(paths[0]))); panel.Children.Add(actions);
        var fitPdfResult = pdfResult && host is null;
        var pdfLayout = pdfResult ? ArrangePrintResult(panel, resultList, actions, paths[0], detail, fitPdfResult) : null;
        panel.Children.Add(new Border { Height = 1, Background = Brush(221, 229, 241) });
        var secondary = new ResponsiveWrapPanel { Spacing = 16 };
        secondary.Children.Add(Command("←  Kembali edit", back)); secondary.Children.Add(restart is null ? DocumentCloseButton() : Command("Tutup dokumen", restart)); panel.Children.Add(secondary);
        if (!pdfResult) { var location = Copy("Lokasi penyimpanan: " + directory, 13); location.IsTextSelectionEnabled = true; panel.Children.Add(location); }
        var summary = detail + Environment.NewLine + string.Join(Environment.NewLine, paths);
        var details = new StackPanel { Spacing = 10 }; var text = Copy(summary, 12); text.IsTextSelectionEnabled = true; details.Children.Add(text);
        details.Children.Add(Command("Salin ringkasan", () => { try { var data = new DataPackage(); data.SetText(summary); Clipboard.SetContent(data); notice.Text = "Ringkasan disalin."; } catch (Exception) { notice.Text = "Clipboard tidak tersedia."; } }));
        panel.Children.Add(new Expander { Header = "Detail hasil", Content = details, HorizontalAlignment = HorizontalAlignment.Stretch });
        panel.Children.Add(notice);
        latestResultCard = new Border { Child = panel, MaxWidth = pdfResult ? 1060 : 700, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brush(255, 255, 255), BorderBrush = Brush(221, 229, 241), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(14), Margin = new Thickness(0, 24, 0, 0) };
        destination.Children.Add(latestResultCard);
        var footer = Copy("File asli tetap aman · Disimpan di perangkat ini", 12); footer.HorizontalAlignment = HorizontalAlignment.Center; destination.Children.Add(footer);
        if (fitPdfResult) FitSavedResultViewport(latestResultCard, panel, pdfLayout!, secondary, details, notice, footer, selected?.Id == "merge", selected?.Id == "organize");
    }

    private void ShowCurrentResult(string[] paths, string detail)
    {
        var editor = body.Children.ToArray(); var controls = editable.ToArray();
        ShowUnifiedResult(paths, detail, () => { body.Children.Clear(); editable.Clear(); foreach (var item in editor) body.Children.Add(item); editable.AddRange(controls); });
    }
}
