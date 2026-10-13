#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private static readonly string[] CatalogRevisionExpectedOrder = ["merge", "split", "organize", "compress", "remove", "extract", "images", "word-pdf", "pdf-jpg", "rotate", "numbers", "watermark", "crop", "edit", "unlock", "protect"];
    internal async Task AuditCatalogContentWidthAsync(string output)
    {
        Directory.CreateDirectory(output);
        var report = new List<string>();
        void Check(bool ok, string name)
        {
            report.Add((ok ? "PASS " : "FAIL ") + name);
            if (!ok) Environment.ExitCode = 1;
        }
        async Task Settle()
        {
            root.UpdateLayout(); await Task.Delay(350); root.UpdateLayout();
        }
        async Task Capture(string name)
        {
            await Settle();
            var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(root);
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth,
                (uint)bitmap.PixelHeight, 96, 96, (await bitmap.GetPixelsAsync()).ToArray()); await encoder.FlushAsync();
        }
        var previousCategory = centeredCategory;
        var previousQuery = centeredQuery;
        try
        {
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter) presenter.Restore();
            centeredCategory = "all"; centeredQuery = "";
            var expected = CatalogRevisionExpectedOrder
                .Select(id => CenteredCopy(ToolCatalog.All.First(t => t.Id == id)).Title).ToArray();
            foreach (var width in new[] { 1920, 1480, 1024, 800, 660 })
            {
                AppWindow.Resize(new Windows.Graphics.SizeInt32(width, 810));
                ShowHome(); await Settle();
                var cards = centeredGrid.Children.OfType<Grid>().ToArray();
                // Odd physical-pixel slack necessarily leaves one pixel on one side.
                var centerTolerance = 1 / root.XamlRoot.RasterizationScale + .01;
                Check(cards.Length == 16, $"16 cards at {width}");
                Check(cards.Select(c => AutomationProperties.GetName((Button)c.Children[0])).SequenceEqual(expected), $"catalog order at {width}");
                var rows = cards.GroupBy(c => Math.Round(c.TransformToVisual(centeredGrid).TransformPoint(new Point()).Y, 1));
                foreach (var row in rows)
                {
                    var first = row.First(); var last = row.Last();
                    var left = first.TransformToVisual(centeredGrid).TransformPoint(new Point()).X;
                    var right = centeredGrid.ActualWidth - last.TransformToVisual(centeredGrid).TransformPoint(new Point()).X - last.ActualWidth;
                    Check(Math.Abs(left - right) <= centerTolerance && left >= 0, $"symmetric row {row.Key} at {width}: left={left:F3}, right={right:F3}, scale={root.XamlRoot.RasterizationScale:F2}");
                    var items = row.ToArray();
                    for (var i = 1; i < items.Length; i++)
                    {
                        var gap = items[i].TransformToVisual(centeredGrid).TransformPoint(new Point()).X
                            - items[i - 1].TransformToVisual(centeredGrid).TransformPoint(new Point()).X - items[i - 1].ActualWidth;
                        Check(Math.Abs(gap - 12) < 1, $"consistent gap {row.Key}/{i} at {width}");
                    }
                }
                Check(cards.Select(c => Math.Round(c.ActualWidth)).Distinct().Count() > 1, $"variable widths at {width}");
                foreach (var card in cards)
                {
                    var launch = (Button)card.Children[0];
                    var name = AutomationProperties.GetName(launch);
                    Check(Math.Abs(card.ActualHeight - 160) < 1, $"fixed height {name} at {width}");
                    Check(card.ActualWidth <= centeredGrid.ActualWidth + 1, $"viewport cap {name} at {width}");
                    var texts = Descendants((StackPanel)launch.Content).OfType<TextBlock>().ToArray();
                    foreach (var text in texts)
                    {
                        var x = text.TransformToVisual(card).TransformPoint(new Point()).X;
                        Check(x >= 34 && x + text.ActualWidth <= card.ActualWidth - 34 + 1, $"text padding {name} at {width}");
                        Check(text.DesiredSize.Height <= text.ActualHeight + 1, $"text readable height {name} at {width}");
                    }
                    Check(card.Children[1] is Microsoft.UI.Xaml.Controls.Primitives.ToggleButton star
                        && star.IsChecked == favorites.Contains(ToolCatalog.All.First(t => CenteredCopy(t).Title == name).Id), $"favorite state {name} at {width}");
                }
                await Capture("catalog-" + width);
            }
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter maximized) maximized.Maximize();
            ShowHome(); await Capture("catalog-maximized");
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter restored) restored.Restore();
            await Settle();
            centeredSearch!.Text = "satukan"; await Settle();
            Check(centeredGrid.Children.Count == 1, "alias search retains one centered card");
            var single = (FrameworkElement)centeredGrid.Children[0];
            var singleLeft = single.TransformToVisual(centeredGrid).TransformPoint(new Point()).X;
            Check(Math.Abs(singleLeft * 2 + single.ActualWidth - centeredGrid.ActualWidth) <= 1 / root.XamlRoot.RasterizationScale + .01,
                "single search row centered within one unavoidable physical pixel");
            centeredSearch.Text = ""; centeredCategory = "security"; RefreshCenteredCards(); await Settle();
            Check(centeredGrid.Children.Count == 2, "security filter retained");
        }
        catch (Exception ex) { Check(false, "audit exception " + ex.GetType().Name); }
        finally
        {
            centeredCategory = previousCategory; centeredQuery = previousQuery;
            await File.WriteAllLinesAsync(Path.Combine(output, "catalog-content-width-audit.txt"), report);
        }
    }
}
#endif
