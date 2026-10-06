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
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    internal async Task AuditReleaseNotesAsync()
    {
        var output = Path.Combine(AppContext.BaseDirectory, "release-notes-audit"); Directory.CreateDirectory(output);
        var report = new List<string>();
        void Check(bool value, string label) { if (!value) throw new InvalidOperationException(label); report.Add("PASS " + label); }
        static IEnumerable<DependencyObject> Walk(DependencyObject node)
        { yield return node; for (var i=0;i<VisualTreeHelper.GetChildrenCount(node);i++) foreach(var child in Walk(VisualTreeHelper.GetChild(node,i))) yield return child; }
        static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
        async Task<ContentDialog> PopupAsync()
        {
            for (var attempt = 0; attempt < 30; attempt++)
            {
                root.UpdateLayout();
                var popup = VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot).SelectMany(p => Walk(p.Child)).OfType<ContentDialog>().FirstOrDefault();
                if (popup is not null) return popup;
                await Task.Delay(100);
            }
            throw new InvalidOperationException("Native notes dialog did not open within three seconds.");
        }
        async Task Settle() { await Task.Delay(350); root.UpdateLayout(); }
        async Task Capture(ContentDialog dialog, string name)
        {
            await Settle(); var panel = Walk(dialog).OfType<FrameworkElement>().First(e=>e.Name=="BackgroundElement");
            Check(panel.ActualWidth <= root.ActualWidth && panel.ActualHeight <= root.ActualHeight, "panel within viewport " + name);
            var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(panel); var bytes = await bitmap.GetPixelsAsync();
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name+".png",CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite); var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId,stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8,BitmapAlphaMode.Premultiplied,(uint)bitmap.PixelWidth,(uint)bitmap.PixelHeight,96,96,bytes.ToArray()); await encoder.FlushAsync();
        }
        try
        {
            var notes = ReleaseNotes.Read(AppContext.BaseDirectory, ApplicationVersion.Running().ProductVersion)!;
            Check(notes is not null, "offline bundled notes read without network");
            ShowHome(); await Settle(); run.Focus(FocusState.Programmatic);
            // Safety guard is the same predicate used by the startup timer.
            messageOpen = true; Check(!ReleaseNotesStartupSafe, "safety or recovery modal defers startup"); messageOpen = false;
            importing = true; Check(!ReleaseNotesStartupSafe, "processing defers startup"); importing = false;
            ShowSettings(); await Settle();
            Check(!ReleaseNotesStartupSafe, "active settings interaction defers startup");
            var opener = Walk(root).OfType<Button>().Single(b=>AutomationProperties.GetName(b)=="Tentang PERTAMINA OTO");
            var next = ApplicationVersion.Running() with { BuildId="20261004-140000-bbbbbbbb" };
            foreach(var width in new[]{1440,820,620})
            {
                AppWindow.Resize(new Windows.Graphics.SizeInt32(width,width==1440?1000:650)); await Settle(); opener.Focus(FocusState.Programmatic);
                var confirmation = ConfirmNotesUpdateAsync("Update lokal tersedia",next,notes,"Pasang update","Dokumen tetap lokal. Versi lama dipertahankan.");
                await Settle(); var dialog = await PopupAsync(); var texts=Walk(dialog).OfType<TextBlock>().Select(t=>t.Text).ToArray();
                Check(texts.Any(t=>t=="Versi saat ini: "+SafeRunningVersion()) && texts.Any(t=>t=="Versi tujuan: "+next.Display),"running and chosen identity "+width);
                Check(notes!.Releases[0].Summary.All(t=>texts.Contains("• "+t)),"exact summary "+width);
                Check(!texts.Any(t=>t.Contains(next.BuildId!,StringComparison.Ordinal)),"technical build hidden "+width);
                Check(dialog.DefaultButton==ContentDialogButton.Close,"safe cancel default "+width);
                var details=Walk(dialog).OfType<Button>().Single(b=>AutomationProperties.GetName(b)=="Lihat detail perubahan");
                Check(details.IsEnabled && details.MinHeight>=0,"accessible detail action "+width);
                await Capture(dialog,"before-update-"+width);
                Invoke(details); await Settle(); dialog=await PopupAsync(); texts=Walk(dialog).OfType<TextBlock>().Select(t=>t.Text).ToArray();
                Check(dialog.Title as string == "Riwayat versi" && texts.Contains("Perbaikan") && texts.Contains("Batasan yang diketahui") && !texts.Contains("Fitur baru"),"details hides empty categories "+width);
                var scroll=(ScrollViewer)dialog.Content; Check(scroll.HorizontalScrollBarVisibility==ScrollBarVisibility.Disabled,"wrapped history "+width);
                await Capture(dialog,"history-top-"+width); scroll.ChangeView(null,scroll.ScrollableHeight,null); await Capture(dialog,"history-bottom-"+width);
                dialog.Hide(); await Settle(); dialog=await PopupAsync(); dialog.Hide();
                Check(!await confirmation,"detail returns to exact confirmation then cancel "+width);
                Check(!messageOpen && ReferenceEquals(FocusManager.GetFocusedElement(root.XamlRoot),opener),"focus restored "+width);
            }
            // Only Opened of a successful native panel acknowledges; no staging/download path writes this file.
            var viewedBefore = ReleaseNotes.ReadViewed(ReleaseViewedPath);
            Check(viewedBefore.Versions.Length==0,"confirmation/history do not acknowledge startup panel");
            opener.Focus(FocusState.Programmatic); var startup=ShowStartupNotesAsync(notes!); await Settle(); var panelDialog=await PopupAsync();
            Check(panelDialog.Title as string=="Yang baru di OTO "+notes!.ProductVersion,"actual running product startup heading");
            Check(panelDialog.CloseButtonText=="Mulai menggunakan" && panelDialog.PrimaryButtonText=="Lihat detail perubahan","startup actions");
            Check(ReleaseNotes.ReadViewed(ReleaseViewedPath).Versions.Contains(notes.ProductVersion),"successful display persists seen version");
            await Capture(panelDialog,"after-startup-620"); panelDialog.Hide(); await startup;
            Check(!messageOpen && ReferenceEquals(FocusManager.GetFocusedElement(root.XamlRoot),opener),"startup dismiss restores focus");
            ShowAboutDrawer(opener); await Settle();
            Check(aboutButtons.Any(b=>AutomationProperties.GetName(b)=="Riwayat versi"),"About history available after acknowledgment");
            Invoke(aboutButtons.Single(b=>AutomationProperties.GetName(b)=="Riwayat versi")); await Settle(); var history=await PopupAsync();
            Check(aboutOverlay is null && history.Title as string=="Riwayat versi","About closes before history"); history.Hide(); await Settle();
            var fallback = ConfirmNotesUpdateAsync("Update lokal tersedia",new(null,null,"20261004-150000-cccccccc"),null,"Pasang update","Paket lama.");
            await Settle(); var missing=await PopupAsync(); Check(Walk(missing).OfType<TextBlock>().Any(t=>t.Text.Contains(ReleaseNotes.Missing,StringComparison.Ordinal)),"honest unavailable selected notes");
            Invoke(Walk(missing).OfType<Button>().Single(b=>AutomationProperties.GetName(b)=="Lihat detail perubahan")); await Settle(); missing=await PopupAsync();
            Check(Walk(missing).OfType<TextBlock>().Any(t=>t.Text==ReleaseNotes.Missing) && !Walk(missing).OfType<TextBlock>().Any(t=>t.Text=="Perbaikan"),"legacy selected details never uses current notes");
            missing.Hide(); await Settle(); (await PopupAsync()).Hide(); await fallback;
            report.Add("LIMIT Physical keyboard/pointer/DPI/multimonitor/screen-reader UAT not performed; native automation and real rendered controls only.");
        }
        catch(Exception error) { report.Add("FAIL "+error.GetType().Name+": "+error.Message); }
        File.WriteAllLines(Path.Combine(output,"audit.txt"),report); Environment.ExitCode=report.Any(l=>l.StartsWith("FAIL",StringComparison.Ordinal))?1:0;
    }
}
#endif
