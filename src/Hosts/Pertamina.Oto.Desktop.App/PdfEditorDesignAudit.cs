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
    internal async Task AuditEditorDesignAsync()
    {
        var report = new List<string>();
        var output = Path.Combine(AppContext.BaseDirectory, "editor-design-audit"); Directory.CreateDirectory(output);

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
        try
        {
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1600, 1000));
            ShowPdfEditorDesignPreview(); await Settle();
            Check(editorSave is { IsEnabled: false }, "save disabled, no false functional editor");
            Check(editorDesign!.Children.OfType<InfoBar>().Any(bar => bar.IsOpen && bar.Title.Contains("Pratinjau", StringComparison.Ordinal)), "design-only screen remains explicitly labelled");
            Check(editorModeButtons.Count == 6, "six explicit tool modes");
            foreach (var mode in editorModeButtons.Keys)
            {
                var peer = new ButtonAutomationPeer(editorModeButtons[mode]);
                ((IInvokeProvider)peer.GetPattern(PatternInterface.Invoke)).Invoke(); await Settle();
                Check(editorProperties!.Children.OfType<TextBlock>().First().Text == mode, "context panel " + mode);
                Check(editorModeButtons[mode].ActualHeight >= 44, "action target " + mode);
                await Capture("mode-" + Array.IndexOf(editorModeButtons.Keys.ToArray(), mode));
            }
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1024, 800)); await Capture("compact");
            Check(editorDesign!.ActualWidth > 800, "compact sidebar leaves usable workspace");
            report.Add("NOT_RUN real PDF editing, original text, export, failure/cancel, engine security admission, full responsive drawer");
            report.Add("PARTIAL native design preview only; not a UI/UX functional pass");
        }
        catch (Exception ex) { report.Add("FAIL " + ex); Environment.ExitCode = 1; }
        finally { File.WriteAllLines(Path.Combine(output, "audit.txt"), report); }
    }
}
#endif
