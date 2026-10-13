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
    internal async Task AuditFavoritesUiAsync()
    {
        var report = new List<string>();
        var output = Path.Combine(AppContext.BaseDirectory, "favorites-ui-audit"); Directory.CreateDirectory(output);
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
            var button = Descendants(body).OfType<Button>().First(b => AutomationProperties.GetName(b) == name);
            var peer = new ButtonAutomationPeer(button); ((IInvokeProvider)peer.GetPattern(PatternInterface.Invoke)).Invoke(); await Settle();
        }
        try
        {
            favorites.Clear(); favorites.UnionWith(["organize", "images", "pdf-jpg"]);
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1480, 960));
            ShowFavorites(); await Settle();
            Check(Descendants(body).OfType<Button>().All(b => !string.IsNullOrWhiteSpace(AutomationProperties.GetName(b))), "accessible names for all buttons");
            Check(Descendants(body).OfType<Button>().Where(b => b.Content is StackPanel).All(b => b.ActualHeight >= 44), "all application action targets >=44 DIP");
            await Capture("01-desktop");
            var search = Descendants(body).OfType<TextBox>().First();
            Check(search.Focus(FocusState.Keyboard), "keyboard focus on search");
            search.Text = "Gambar"; await Settle();
            Check(Descendants(body).OfType<TextBlock>().Any(t => t.Text == "1 dari 3 alat"), "search filters cards");
            search.Text = "xyz-nomatch"; await Capture("02-no-results");
            Check(Descendants(body).OfType<TextBlock>().Any(t => t.Text.StartsWith("Tidak ada alat", StringComparison.Ordinal) && t.Visibility == Visibility.Visible), "no results explanation");
            search.Text = "";
            AppWindow.Resize(new Windows.Graphics.SizeInt32(640, 800)); await Capture("03-narrow");
            var cards = Descendants(body).OfType<Border>().Where(b => b.Tag is string).ToArray();
            Check(cards.Length == 3 && cards.All(c => c.ActualWidth > 240 && c.ActualHeight >= 252), "narrow cards not clipped");
            var bounds = cards.Select(c => c.TransformToVisual(body).TransformBounds(new Windows.Foundation.Rect(0, 0, c.ActualWidth, c.ActualHeight))).ToArray();
            Check(bounds.All(r => r.X >= 0 && r.Right <= body.ActualWidth + 1) &&
                !bounds.Where((r, i) => bounds.Skip(i + 1).Any(other => r.X < other.Right && r.Right > other.X && r.Y < other.Bottom && r.Bottom > other.Y)).Any(),
                "adaptive narrow favorites stay within viewport without overlap");
            await Click("Hapus Gambar ke PDF dari favorit");
            Check(!favorites.Contains("images"), "remove favorite");
            await Click("Urungkan penghapusan Gambar ke PDF");
            Check(favorites.Contains("images"), "undo removal");
            Check(File.Exists(libraryTestPath), "favorites persisted to isolated storage");
            await Click("Buka Atur halaman PDF");
            Check(selected?.Id == "organize", "open favorite tool (upload state without documents)");
            ShowFavorites(); await Settle();
            await Click("Tambah favorit");
            Check(centeredGrid.IsLoaded && centeredGrid.Children.Count == ToolCatalog.All.Count(t => t.Available), "add favorite opens current centered tool catalog");
            favorites.Clear(); ShowFavorites(); await Capture("04-empty");
            Check(Descendants(body).OfType<TextBlock>().Any(t => t.Text.StartsWith("Belum ada favorit", StringComparison.Ordinal)), "empty state");
            report.Add("PASS audit complete");
        }
        catch (Exception ex) { report.Add("FAIL " + ex); Environment.ExitCode = 1; }
        finally { File.WriteAllLines(Path.Combine(output, "audit.txt"), report); }
    }
}
#endif
