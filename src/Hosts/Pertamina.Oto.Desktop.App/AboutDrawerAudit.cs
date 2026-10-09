#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    internal async Task AuditAboutDrawerAsync()
    {
        var output = Path.Combine(AppContext.BaseDirectory, "about-audit"); Directory.CreateDirectory(output);
        // Screenshot only synthetic settings; do not capture a loaded user folder behind the backdrop.
        defaultFolder = "Folder hasil lokal (audit sintetis)"; preferenceNotice = "";
        var report = new List<string>();
        void Check(bool ok, string label) { if (!ok) throw new InvalidOperationException(label); report.Add("PASS " + label); }
        static IEnumerable<DependencyObject> Walk(DependencyObject node)
        { yield return node; for (var i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++) foreach (var child in Walk(VisualTreeHelper.GetChild(node, i))) yield return child; }
        async Task Settle() { root.UpdateLayout(); await Task.Delay(300); root.UpdateLayout(); }
        void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
        async Task Capture(string name)
        {
            await Settle(); var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync((UIElement)Content); var bytes = await bitmap.GetPixelsAsync();
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite); var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, bytes.ToArray()); await encoder.FlushAsync();
        }
        async Task Open(int width, int height)
        {
            AppWindow.Resize(new Windows.Graphics.SizeInt32(width, height)); await Settle(); ShowSettings(); await Settle();
            var opener = Walk(root).OfType<Button>().Single(b => AutomationProperties.GetName(b) == "Tentang PERTAMINA OTO");
            opener.Focus(FocusState.Programmatic); Invoke(opener); await Settle();
            Check(aboutOverlay is not null && aboutPanel is not null, "settings opener displays drawer " + width);
            Check(aboutPanel!.ActualWidth <= root.ActualWidth && Math.Abs(aboutPanel.ActualWidth - Math.Min(550, root.ActualWidth)) < 2, "responsive width " + width);
            var position = aboutPanel.TransformToVisual(root).TransformPoint(new Windows.Foundation.Point());
            Check(Math.Abs(position.X + aboutPanel.ActualWidth - root.ActualWidth) < 2 && Math.Abs(aboutPanel.ActualHeight - root.ActualHeight) < 2, "right aligned full content height " + width);
        }
        try
        {
            await Open(1440, 1000);
            Check(aboutButtons.Count == 8, "all eight actions exist and have accessibility names");
            Check(aboutButtons.All(b => !string.IsNullOrWhiteSpace(AutomationProperties.GetName(b)) && b.IsEnabled), "actions enabled");
            Check(aboutDisabled.All(c => !c.Control.IsEnabled), "underlying controls and menu disabled");
            Check(ReferenceEquals(FocusManager.GetFocusedElement(root.XamlRoot), aboutButtons[0]), "initial focus close");
            MoveAboutFocus(true); Check(ReferenceEquals(FocusManager.GetFocusedElement(root.XamlRoot), aboutButtons[^1]), "reverse focus wraps inside drawer");
            MoveAboutFocus(false); Check(ReferenceEquals(FocusManager.GetFocusedElement(root.XamlRoot), aboutButtons[0]), "forward focus wraps inside drawer");
            foreach (var button in aboutButtons.ToArray()) { MoveAboutFocus(false); Check(aboutButtons.Contains((Button)FocusManager.GetFocusedElement(root.XamlRoot)), "focus remains within drawer"); }
            Check(Walk(aboutPanel!).OfType<TextBlock>().Any(t => t.Text == ApplicationVersion.Running().Display), "running embedded product/channel displayed");
            Check(!Walk(aboutPanel!).OfType<TextBlock>().Any(t => t.Text == RunningBuildLabel()), "build ID reserved for technical details");
            Check(Walk(aboutPanel!).OfType<Image>().Single().Source is BitmapImage, "existing official logo loaded");
            Check(File.Exists(Path.Combine(AppContext.BaseDirectory, "Assets", "oto.png")), "official logo shipped");
            var notices = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Assets", "About-component-notices.txt"));
            Check(notices.Contains("PDFsharp/6.2.4") && notices.Contains("Microsoft.WindowsAppSDK/1.7.250909003") && notices.Contains("ISC License"), "real resolved component inventory and notices shipped");
            Check(!SafeApplicationInfo().Contains(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), StringComparison.OrdinalIgnoreCase)
                && !SafeApplicationInfo().Contains(AppContext.BaseDirectory, StringComparison.OrdinalIgnoreCase)
                && !SafeApplicationInfo().Contains("MSIX identity"), "summary excludes user paths and fictitious MSIX identity");
            Check(SanitizeRunningVersion("C:\\private\\document.pdf") == "Build pengembangan"
                && SanitizeRunningVersion("secret\nvalue") == "Build pengembangan"
                && SanitizeRunningVersion("arbitrary-secret") == "Build pengembangan"
                && SanitizeRunningVersion("20261001-110000-abcdef12") == "20261001-110000-abcdef12"
                && SanitizeRunningVersion(new string('a', 81)) == "Build pengembangan", "invalid build metadata cannot leak paths/control text");
            // Preserve clipboard: only execute write/read when no existing user clipboard formats would be lost.
            var existingClipboard = Clipboard.GetContent();
            if (!existingClipboard.AvailableFormats.Any())
            {
                Invoke(aboutButtons[^1]); await Settle();
                Check(await Clipboard.GetContent().GetTextAsync() == SafeApplicationInfo(), "clipboard contains safe build summary exactly"); Clipboard.Clear();
            }
            else report.Add("LIMIT clipboard write skipped to preserve existing user data; exact summary audited");
            await Capture("01-about-desktop");
            var savedOpener = aboutOpener; var savedStates = aboutDisabled.ToArray();
            Invoke(aboutButtons[0]); await Settle();
            Check(aboutOverlay is null && aboutPanel is null, "close button removes drawer");
            Check(savedStates.All(s => s.Control.IsEnabled == s.Enabled), "enabled states restored exactly");
            Check(ReferenceEquals(FocusManager.GetFocusedElement(root.XamlRoot), savedOpener), "focus returns to settings opener");
            await Open(820, 650); await Capture("02-about-compact");
            var scroll = Walk(aboutPanel!).OfType<ScrollViewer>().First();
            Check(scroll.ScrollableHeight > 0, "small height scrolls content");
            var copy = aboutButtons[^1]; var p = copy.TransformToVisual(root).TransformPoint(new Windows.Foundation.Point());
            Check(p.Y >= 0 && p.Y + copy.ActualHeight <= root.ActualHeight, "copy footer visible at small height");
            Check(HandleAboutKey(Windows.System.VirtualKey.Escape, false), "Escape is handled by drawer key route"); await Settle();
            Check(aboutOverlay is null && ReferenceEquals(FocusManager.GetFocusedElement(root.XamlRoot), aboutSettingsButton), "Escape closes and restores opener focus");
            await Open(620, 600); await Capture("03-about-narrow");
            scroll = Walk(aboutPanel!).OfType<ScrollViewer>().First(); scroll.ChangeView(null, scroll.ScrollableHeight, null); await Capture("04-about-narrow-links");
            Check(scroll.VerticalOffset > 0 && aboutButtons.Single(b => AutomationProperties.GetName(b) == "Detail teknis").IsLoaded, "narrow drawer scroll reaches links");
            CloseAboutDrawer(); await Open(1440, 1000);
            Invoke(aboutButtons.Single(b => AutomationProperties.GetName(b) == "Panduan penggunaan")); await Settle();
            Check(aboutOverlay is null && Walk(root).OfType<TextBlock>().Any(t => t.Text.Contains("Bantuan", StringComparison.Ordinal)), "guide reuses existing help flow");
            await Open(1440, 1000); Invoke(aboutButtons.Single(b => AutomationProperties.GetName(b) == "Laporkan masalah")); await Settle();
            Check(aboutOverlay is null && Walk(root).OfType<TextBox>().Any(t => t.Header as string == "Judul masalah"), "report reuses local flow without external send");
            await Open(1440, 1000); Invoke(aboutButtons.Single(b => AutomationProperties.GetName(b) == "Detail teknis")); await Settle();
            Check(aboutOverlay is null && messageOpen, "technical dialog opens after drawer closes");
            var dialog = Walk((DependencyObject)Content).OfType<ContentDialog>().FirstOrDefault();
            // ContentDialogs live in a popup; find it through the XamlRoot popup tree.
            dialog ??= Microsoft.UI.Xaml.Media.VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot).SelectMany(popup => Walk(popup.Child)).OfType<ContentDialog>().FirstOrDefault();
            Check(dialog is not null, "technical dialog native popup found"); dialog!.Hide(); await Settle();
            await Open(1440, 1000); Invoke(aboutButtons.Single(b => AutomationProperties.GetName(b) == "Lisensi komponen")); await Settle();
            Check(aboutOverlay is null && messageOpen, "license dialog opens without stacked drawer");
            dialog = VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot).SelectMany(popup => Walk(popup.Child)).OfType<ContentDialog>().First(); dialog.Hide(); await Settle();
            await Open(1440, 1000); var beforeVersion = SafeRunningVersion(); Invoke(aboutButtons.Single(b => AutomationProperties.GetName(b) == "Periksa update"));
            for (var retry = 0; retry < 30 && !messageOpen; retry++) await Task.Delay(100);
            await Settle(); Check(aboutOverlay is null && !messageOpen, "audit updater guard preserves native session after drawer closes");
            Check(SafeRunningVersion() == beforeVersion, "checking update leaves running version unchanged");
            Check(!VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot).SelectMany(popup => Walk(popup.Child)).OfType<ContentDialog>().Any(), "audit updater does not open network dialog");
            report.Add("LIMIT Physical Esc/backdrop and screen-reader UAT remain manual; native handlers and focus cycle audited.");
        }
        catch (Exception error) { report.Add("FAIL " + error.GetType().Name + ": " + error.Message); }
        File.WriteAllLines(Path.Combine(output, "audit.txt"), report);
        Environment.ExitCode = report.Any(line => line.StartsWith("FAIL", StringComparison.Ordinal)) ? 1 : 0;
    }
}
#endif
