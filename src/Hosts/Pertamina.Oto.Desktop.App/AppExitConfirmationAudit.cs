#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // Isolated native fixture only: never authorizes Close or processes a user document.
    internal async Task AuditAppExitConfirmationAsync(string output)
    {
        Directory.CreateDirectory(output);
        var report = new List<string>();
        void Check(bool ok, string label)
        {
            report.Add((ok ? "PASS " : "FAIL ") + label);
            File.WriteAllLines(Path.Combine(output, "audit.txt"), report);
            if (!ok) throw new InvalidOperationException(label);
        }
        var originalAllowClose = allowClose;
        var originalFiles = files.ToArray();
        var originalDrafts = toolDrafts.Count;
        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter normal) normal.Restore();
        foreach (var width in new[] { 1400, 660 })
        {
            AppWindow.Resize(new Windows.Graphics.SizeInt32(width, width == 660 ? 810 : 1000));
            await Task.Delay(200);
            foreach (var action in new[] { ContentDialogResult.None, ContentDialogResult.Primary })
            {
                var view = BuildAppExitConfirmation();
                var showing = view.Dialog.ShowAsync();
                try
                {
                    await Task.Delay(250); view.Dialog.UpdateLayout();
                    Check(view.Dialog.PrimaryButtonText.Length == 0 && view.Dialog.SecondaryButtonText.Length == 0 && view.Dialog.CloseButtonText.Length == 0,
                        width + ": no duplicate footer actions");
                    Check(Descendants(view.Dialog).OfType<Button>().Count(b => b.Name is "AppExitConfirm" or "AppExitReturn") == 2,
                        width + ": exactly two app exit actions");
                    Check(view.Return.FocusState != FocusState.Unfocused && view.Result == ContentDialogResult.None,
                        width + ": safe initial focus and default intent");
                    var copy = Descendants(view.Dialog).OfType<TextBlock>().Select(t => t.Text).ToArray();
                    foreach (var text in new[] { "Keluar dari OTO?", "Draf dan pilihan dokumen tidak disimpan otomatis.",
                        "Simpan hasil yang diperlukan sebelum keluar.", "File asli dan hasil tersimpan tetap aman." })
                        Check(copy.Contains(text), width + ": exact copy " + text);
                    foreach (var button in new[] { view.Exit, view.Return })
                    {
                        Check(button.ActualWidth >= 44 && button.ActualHeight >= 44 && button.ActualWidth <= root.ActualWidth,
                            width + ": " + button.Name + " bounded hit target");
                        Check(AutomationProperties.GetName(button).Length > 0 && button.UseSystemFocusVisuals && button.IsTabStop,
                            width + ": " + button.Name + " native accessibility");
                    }
                    Check(view.Exit.Resources.ThemeDictionaries.ContainsKey("HighContrast"), width + ": exit native high contrast fallback");
                    var biblo = Descendants(view.Dialog).OfType<Image>().Single(i => i.Name == "AppExitBiblo");
                    var bitmap = (BitmapImage)biblo.Source;
                    Check(bitmap.PixelWidth > 0 && biblo.ActualWidth > 0, width + ": exact asset decoded");
                    var background = Descendants(view.Dialog).OfType<FrameworkElement>().First(e => e.Name == "BackgroundElement");
                    Check(background.ActualWidth <= root.ActualWidth && background.ActualHeight <= root.ActualHeight,
                        width + ": modal fits client");
                    if (action == ContentDialogResult.None)
                    {
                        var rendered = new RenderTargetBitmap(); await rendered.RenderAsync(background);
                        var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync("app-exit-" + width + ".png", CreationCollisionOption.ReplaceExisting);
                        using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)rendered.PixelWidth,
                            (uint)rendered.PixelHeight, 96, 96, (await rendered.GetPixelsAsync()).ToArray());
                        await encoder.FlushAsync();
                    }
                    var invoke = action == ContentDialogResult.Primary ? view.Exit : view.Return;
                    ((IInvokeProvider)new ButtonAutomationPeer(invoke).GetPattern(PatternInterface.Invoke)).Invoke();
                    await showing;
                    Check(view.Result == action, width + ": exact result mapping " + action);
                    Check(allowClose == originalAllowClose && files.SequenceEqual(originalFiles) && toolDrafts.Count == originalDrafts,
                        width + ": dialog intent leaves close authority and session untouched");
                }
                finally { view.Dialog.Hide(); await showing; }
            }
        }
        ShowHome(); await Task.Delay(250);
        await AuditAppExitRetentionAsync(Check);
        report.Add("LIMIT physical Enter/Escape, pointer states, screen reader, DPI and live high contrast remain manual/native integration checks.");
        report.Add("LIMIT this fixture verifies dialog intent only; universal menu/window routing and busy/pending guards require integrator lifecycle verification.");
        File.WriteAllLines(Path.Combine(output, "audit.txt"), report);
    }
}
#endif
