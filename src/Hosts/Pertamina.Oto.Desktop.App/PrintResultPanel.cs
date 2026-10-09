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
    private static void ArrangePrintResult(StackPanel panel, UIElement list, UIElement actions, string path, string detail)
    {
        var index = panel.Children.IndexOf(list); panel.Children.Remove(list); panel.Children.Remove(actions);
        var layout = new Grid { ColumnSpacing = 24, RowSpacing = 16 }; layout.ColumnDefinitions.Add(new() { Width = new GridLength(2, GridUnitType.Star) }); layout.ColumnDefinitions.Add(new() { Width = new GridLength(3, GridUnitType.Star) });
        layout.RowDefinitions.Add(new() { Height = GridLength.Auto }); layout.RowDefinitions.Add(new() { Height = GridLength.Auto });
        var bitmap = new Image { Stretch = Stretch.Uniform, MaxHeight = 420, Margin = new Thickness(12), HorizontalAlignment = HorizontalAlignment.Center };
        var previewText = HistoryText("Halaman 1 · Memuat pratinjau…", 12, muted: true); previewText.HorizontalAlignment = HorizontalAlignment.Center;
        var preview = new StackPanel { Spacing = 10, Padding = new Thickness(16) }; preview.Children.Add(HistoryText("Pratinjau hasil", 14, true)); preview.Children.Add(bitmap); preview.Children.Add(previewText);
        var well = new Border { Child = preview, Background = Brush(237, 243, 249), CornerRadius = new CornerRadius(8) }; layout.Children.Add(well);
        var info = new StackPanel { Spacing = 16 }; Grid.SetColumn(info, 1); layout.Children.Add(info);
        info.Children.Add(HistoryText("FILE HASIL", 12, muted: true)); var name = HistoryText(Path.GetFileName(path), 20, true); name.TextWrapping = TextWrapping.Wrap; name.IsTextSelectionEnabled = true; info.Children.Add(name);
        var metadata = HistoryText(detail, 14, muted: true); info.Children.Add(metadata); info.Children.Add(new Border { Height = 1, Background = Brush(220, 229, 241) });
        info.Children.Add(HistoryText("Lokasi penyimpanan", 13, true)); var folder = Path.GetDirectoryName(path)!; var folderName = HistoryText("▱  " + Path.GetFileName(folder), 16); folderName.TextWrapping = TextWrapping.Wrap; ToolTipService.SetToolTip(folderName, folder); info.Children.Add(folderName);
        info.Children.Add(HistoryText(detail, 13, muted: true)); info.Children.Add(actions);
        info.Children.Add(new Border { Background = Brush(240, 246, 253), Padding = new Thickness(12), CornerRadius = new CornerRadius(8), Child = HistoryText("Cetak hasil membuka pengaturan printer.\nDokumen yang dicetak adalah file hasil di atas.", 13, muted: true) });
        panel.Children.Insert(index, layout);
        layout.SizeChanged += (_, _) => { var compact = layout.ActualWidth < 650; Grid.SetColumn(info, compact ? 0 : 1); Grid.SetRow(info, compact ? 1 : 0); Grid.SetColumnSpan(well, compact ? 2 : 1); Grid.SetColumnSpan(info, compact ? 2 : 1); bitmap.MaxHeight = compact ? 240 : 420; };
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
    }
}
