#if DEBUG
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    internal async Task AuditStartupWindowAsync()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "startup-window-audit");
        Directory.CreateDirectory(directory);
        var report = new List<string>();
        void Check(bool ok, string name)
        { report.Add((ok ? "PASS " : "FAIL ") + name); if (!ok) throw new InvalidOperationException(name); }
        async Task Settle() { await Task.Delay(250); root.UpdateLayout(); }
        async Task Capture(string name)
        {
            var image = new RenderTargetBitmap(); await image.RenderAsync((UIElement)Content);
            var pixels = await image.GetPixelsAsync();
            var file = await (await StorageFolder.GetFolderFromPathAsync(directory)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                (uint)image.PixelWidth, (uint)image.PixelHeight, 96, 96, pixels.ToArray());
            await encoder.FlushAsync();
        }
        try
        {
            await Settle();
            Check(AppWindow.Presenter is OverlappedPresenter, "normal overlapped presenter, not fullscreen");
            var presenter = (OverlappedPresenter)AppWindow.Presenter;
            Check(presenter.State == OverlappedPresenterState.Maximized, "first activation is maximized");
            if (Environment.GetEnvironmentVariable("OTO_STARTUP_WINDOW_AUDIT") == "1")
                Check(pdfReader?.ReadingDocument is not null, "real PDF startup argument loaded before window audit");
            Check(presenter.HasTitleBar && presenter.HasBorder, "native caption and border retained");
            Check(presenter.IsMinimizable && presenter.IsMaximizable && presenter.IsResizable, "native window controls remain enabled");
            Check(AppWindow.IsShownInSwitchers && !presenter.IsAlwaysOnTop, "ordinary taskbar/switcher window, not always on top");
            await Capture("01-maximized-content");
            if (Environment.GetEnvironmentVariable("OTO_STARTUP_WINDOW_VISUAL") == "1")
                await Task.Delay(20000); // Optional isolated native-frame inspection only.
            presenter.Restore(); await Settle();
            var work = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest).WorkArea;
            Check(presenter.State == OverlappedPresenterState.Restored, "Restore works");
            Check(AppWindow.Size.Width <= work.Width && AppWindow.Size.Height <= work.Height
                && AppWindow.Position.X >= work.X && AppWindow.Position.Y >= work.Y
                && AppWindow.Position.X + AppWindow.Size.Width <= work.X + work.Width
                && AppWindow.Position.Y + AppWindow.Size.Height <= work.Y + work.Height, "restore rectangle fits current work area");
            var minimum = windowSizePolicy!.Minimum;
            var size = new SizeInt32(Math.Min(work.Width, Math.Max(minimum.Width, Math.Min(900, work.Width * 4 / 5))),
                Math.Min(work.Height, Math.Max(minimum.Height, Math.Min(650, work.Height * 4 / 5))));
            AppWindow.Resize(size); await Settle();
            Check(AppWindow.Size.Width == size.Width && AppWindow.Size.Height == size.Height, "user resize remains effective");
            PrepareStartupWindow(); Activate(); await Settle();
            Check(presenter.State == OverlappedPresenterState.Restored && AppWindow.Size.Width == size.Width, "repeat preparation and activation preserve user restore/resize");
            ShowSettings(); await Settle(); ShowHome(); await Settle();
            Check(presenter.State == OverlappedPresenterState.Restored, "navigation never remaximizes");
            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog { XamlRoot = root.XamlRoot, Title = "Audit sintetis", Content = "Jendela tetap dapat diubah ukurannya.", CloseButtonText = "Tutup" };
            var showing = dialog.ShowAsync(); await Settle(); dialog.Hide(); await showing;
            Check(presenter.State == OverlappedPresenterState.Restored, "dialog never remaximizes");
            var pdf = Path.Combine(directory, "synthetic-" + Guid.NewGuid().ToString("N") + ".pdf");
            WriteHistoryAuditFixture(pdf);
            Check(File.Exists(pdf), "synthetic PDF fixture exists");
            await OpenPdfReaderAsync(pdf); await Settle();
            Check(presenter.State == OverlappedPresenterState.Restored, "opening a document in the running instance never remaximizes");
            presenter.Minimize(); await Settle();
            Check(presenter.State == OverlappedPresenterState.Minimized, "Minimize works");
            presenter.Restore(); await Settle();
            Check(presenter.State == OverlappedPresenterState.Restored, "restore from minimize keeps user window state");
            await Capture("02-restored-content");
        }
        catch (Exception error) { report.Add("FAIL exception " + error.GetType().Name); Environment.ExitCode = 1; }
        File.WriteAllLines(Path.Combine(directory, "audit.txt"), report);
    }
}
#endif
