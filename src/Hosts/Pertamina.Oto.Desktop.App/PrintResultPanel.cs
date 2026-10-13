using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Pertamina.Oto.Desktop.App;
internal sealed partial class MainWindow
{
    private static Grid ArrangePrintResult(StackPanel panel, UIElement list, UIElement actions, string path, string detail, bool fitViewport = false)
    {
        var index = panel.Children.IndexOf(list); panel.Children.Remove(list); panel.Children.Remove(actions);
        var layout = new Grid { ColumnSpacing = 24, RowSpacing = 16 }; layout.ColumnDefinitions.Add(new() { Width = new GridLength(2, GridUnitType.Star) }); layout.ColumnDefinitions.Add(new() { Width = new GridLength(3, GridUnitType.Star) });
        layout.RowDefinitions.Add(new() { Height = GridLength.Auto }); layout.RowDefinitions.Add(new() { Height = GridLength.Auto });
        var bitmap = new Image { Stretch = Stretch.Uniform, MaxHeight = 420, Margin = new Thickness(12), HorizontalAlignment = HorizontalAlignment.Center };
        var previewText = HistoryText("Halaman 1 · Memuat pratinjau…", 12, muted: true); previewText.HorizontalAlignment = HorizontalAlignment.Center;
        var preview = new StackPanel { Spacing = 10, Padding = new Thickness(16) }; preview.Children.Add(HistoryText("Pratinjau hasil", 14, true)); preview.Children.Add(bitmap); preview.Children.Add(previewText);
        UIElement previewContent = preview;
        if (fitViewport)
        {
            preview.Children.Clear();
            var fittedPreview = new Grid { Padding = new Thickness(12), RowSpacing = 8 };
            fittedPreview.RowDefinitions.Add(new() { Height = GridLength.Auto });
            fittedPreview.RowDefinitions.Add(new() { Height = new GridLength(1, GridUnitType.Star) });
            fittedPreview.RowDefinitions.Add(new() { Height = GridLength.Auto });
            fittedPreview.Children.Add(HistoryText("Pratinjau hasil", 13, true));
            bitmap.MaxHeight = double.PositiveInfinity; bitmap.Margin = new Thickness(4);
            Grid.SetRow(bitmap, 1); fittedPreview.Children.Add(bitmap);
            Grid.SetRow(previewText, 2); fittedPreview.Children.Add(previewText);
            previewContent = fittedPreview;
            layout.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
        }
        var well = new Border { Child = previewContent, Background = Brush(237, 243, 249), CornerRadius = new CornerRadius(8) }; layout.Children.Add(well);
        var info = new StackPanel { Spacing = 16 }; Grid.SetColumn(info, 1); layout.Children.Add(info);
        info.Children.Add(HistoryText("FILE HASIL", 12, muted: true)); var name = HistoryText(Path.GetFileName(path), 20, true); name.TextWrapping = TextWrapping.Wrap; name.IsTextSelectionEnabled = true; info.Children.Add(name);
        var metadata = HistoryText(detail, 14, muted: true); info.Children.Add(metadata); info.Children.Add(new Border { Height = 1, Background = Brush(220, 229, 241) });
        info.Children.Add(HistoryText("Lokasi penyimpanan", 13, true)); var folder = Path.GetDirectoryName(path)!; var folderName = HistoryText("▱  " + Path.GetFileName(folder), 16); folderName.TextWrapping = TextWrapping.Wrap; ToolTipService.SetToolTip(folderName, folder); info.Children.Add(folderName);
        if (!fitViewport) info.Children.Add(HistoryText(detail, 13, muted: true)); info.Children.Add(actions);
        if (!fitViewport) info.Children.Add(new Border { Background = Brush(240, 246, 253), Padding = new Thickness(12), CornerRadius = new CornerRadius(8), Child = HistoryText("Cetak hasil membuka pengaturan printer.\nDokumen yang dicetak adalah file hasil di atas.", 13, muted: true) });
        if (fitViewport)
        {
            info.Spacing = 20; info.VerticalAlignment = VerticalAlignment.Top;
            info.Children.Clear();
            var fileHeader = new Grid { ColumnSpacing = 14 };
            fileHeader.ColumnDefinitions.Add(new() { Width = GridLength.Auto }); fileHeader.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
            var pdfIcon = new Border { Background = Brush(207, 44, 54), CornerRadius = new CornerRadius(5), Padding = new Thickness(7, 10, 7, 10), VerticalAlignment = VerticalAlignment.Top, Child = new TextBlock { Text = "PDF", FontSize = 13, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(255, 255, 255) } };
            fileHeader.Children.Add(pdfIcon);
            var fileDescription = new StackPanel { Spacing = 12 }; fileDescription.Children.Add(name);
            var chips = new ResponsiveWrapPanel { Spacing = 8 };
            void UpdateChips()
            {
                chips.Children.Clear();
                foreach (var value in metadata.Text.Split('·', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Where(v => v != "PDF"))
                {
                    // Keep result actions reachable while asynchronous preview metadata is pending.
                    var label = HistoryText(value, 13, muted: true);
                    label.MaxLines = 2; label.TextTrimming = TextTrimming.CharacterEllipsis;
                    ToolTipService.SetToolTip(label, value);
                    chips.Children.Add(new Border { Background = Brush(237, 243, 249), CornerRadius = new CornerRadius(18), Padding = new Thickness(12, 6, 12, 6), Child = label });
                }
            }
            UpdateChips(); metadata.RegisterPropertyChangedCallback(TextBlock.TextProperty, (_, _) => UpdateChips());
            fileDescription.Children.Add(chips); Grid.SetColumn(fileDescription, 1); fileHeader.Children.Add(fileDescription); info.Children.Add(fileHeader);
            var location = new StackPanel { Spacing = 6 }; location.Children.Add(HistoryText("Lokasi penyimpanan", 13, muted: true)); location.Children.Add(folderName);
            info.Children.Add(new Border { Child = location, Background = Brush(247, 250, 253), BorderBrush = Brush(220, 229, 241), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10), Padding = new Thickness(16) });
            info.Children.Add(actions);
            var safety = HistoryText("✓  Tersimpan aman di perangkat ini", 13, muted: true); info.Children.Add(safety);
            well.CornerRadius = new CornerRadius(12);
            name.MaxLines = 2; name.TextTrimming = TextTrimming.CharacterEllipsis;
            ToolTipService.SetToolTip(name, Path.GetFileName(path));
            metadata.MaxLines = 2; metadata.TextTrimming = TextTrimming.CharacterEllipsis;
            folderName.MaxLines = 1; folderName.TextTrimming = TextTrimming.CharacterEllipsis;
            foreach (var command in ((Panel)actions).Children.OfType<Button>())
            {
                var label = Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(command);
                if (label.StartsWith("Cetak", StringComparison.Ordinal))
                    ToolTipService.SetToolTip(command, "Membuka pengaturan printer untuk file hasil ini.");
                else command.Content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = {
                    new FontIcon { Glyph = label == "Buka hasil" ? "\uE8A7" : "\uE8B7", FontSize = 18 }, new TextBlock { Text = label } } };
            }
        }
        panel.Children.Insert(index, layout);
        layout.SizeChanged += (_, _) =>
        {
            if (fitViewport)
            {
                var compactFit = layout.ActualWidth < 650;
                layout.ColumnSpacing = compactFit ? 12 : 28; info.Spacing = compactFit ? 8 : 20;
                name.FontSize = compactFit ? 16 : 20;
                // The full path in inline details replaces the folder-only tile in tight windows.
                info.Children.OfType<Border>().First().Visibility = compactFit ? Visibility.Collapsed : Visibility.Visible;
                layout.ColumnDefinitions[0].Width = new GridLength(compactFit ? 2 : 46, GridUnitType.Star);
                layout.ColumnDefinitions[1].Width = new GridLength(compactFit ? 3 : 54, GridUnitType.Star);
                return;
            }
            var compact = layout.ActualWidth < 650; Grid.SetColumn(info, compact ? 0 : 1); Grid.SetRow(info, compact ? 1 : 0); Grid.SetColumnSpan(well, compact ? 2 : 1); Grid.SetColumnSpan(info, compact ? 2 : 1); bitmap.MaxHeight = compact ? 240 : 420;
        };
        bitmap.Loaded += async (_, _) =>
        {
            try
            {
                if (await PrintSecurity.HasEncryptionAsync(path)) { if (actions is Panel commands) foreach (var command in commands.Children.OfType<Button>().Where(b => Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(b).StartsWith("Cetak", StringComparison.Ordinal)).ToArray()) commands.Children.Remove(command); previewText.Text = "PDF dilindungi · cetak tidak tersedia"; return; }
                var pdf = await Task.Run(async () => await PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(path)));
                if (!bitmap.IsLoaded || pdf.PageCount == 0) return;
                using var page = pdf.GetPage(0); var raster = PrintPlan.RasterSize(page.Size.Width, page.Size.Height, 500, 1600, 2_000_000);
                using var stream = new InMemoryRandomAccessStream(); await page.RenderToStreamAsync(stream, new PdfPageRenderOptions { DestinationWidth = raster.Width, DestinationHeight = raster.Height }); stream.Seek(0);
                var image = new BitmapImage(); await image.SetSourceAsync(stream); if (!bitmap.IsLoaded) return; bitmap.Source = image;
                previewText.Text = $"Halaman 1 dari {pdf.PageCount}"; metadata.Text = $"PDF · {pdf.PageCount} halaman · {HistoryFileSize(new FileInfo(path).Length)}";
            }
            catch (Exception error) { LocalDiagnostics.Record("print-result-preview", error); previewText.Text = "Pratinjau tidak tersedia. Periksa file hasil."; }
        };
        return layout;
    }
}
