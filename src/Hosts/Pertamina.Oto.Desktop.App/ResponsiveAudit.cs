#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;
internal sealed partial class MainWindow
{
    internal async Task AuditResponsiveAsync(bool interactive)
    {
        var output = Path.Combine(AppContext.BaseDirectory, "responsive-audit"); Directory.CreateDirectory(output);
        libraryTestPath = Path.Combine(output, "library.json"); savedJobs.Clear(); homeFiles.Clear(); chosenFiles.Clear();
        var report = new List<string>(); var failures = 0;
        void Check(bool pass, string label) { report.Add((pass ? "PASS " : "FAIL ") + label); if (!pass) failures++; File.WriteAllLines(Path.Combine(output, "audit-current.txt"), report); }
        async Task Settle() { root.UpdateLayout(); await Task.Delay(140); }
        async Task Capture(string label)
        {
            if (Environment.GetCommandLineArgs().Contains("--no-capture")) return;
            var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(root); var pixels = await bitmap.GetPixelsAsync();
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(label + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite); var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, pixels.ToArray()); await encoder.FlushAsync();
        }
        bool Visible(FrameworkElement node)
        {
            for (DependencyObject? item = node; item is not null && item != root; item = VisualTreeHelper.GetParent(item))
                if (item is UIElement { Visibility: Visibility.Collapsed }) return false;
            return node.ActualWidth > 0 && node.ActualHeight > 0;
        }
        void Bounds(string label)
        {
            var controls = Descendants(root).OfType<FrameworkElement>().Where(n => n is ButtonBase or TextBox or ComboBox or NumberBox or PasswordBox).Where(Visible).ToArray();
            var clipped = new List<string>();
            foreach (var control in controls)
            {
                var bounds = control.TransformToVisual(root).TransformBounds(new Rect(0, 0, control.ActualWidth, control.ActualHeight));
                if (bounds.Bottom <= 0 || bounds.Top >= root.ActualHeight) continue;
                if (bounds.Left < -1 || bounds.Right > root.ActualWidth + 1)
                    clipped.Add($"{control.GetType().Name}:{AutomationProperties.GetName(control)}:{bounds.Left:0}/{bounds.Right:0}");
            }
            Check(clipped.Count == 0, label + " horizontally bounded visible actions " + string.Join(";", clipped.Take(12)));
            foreach (var panel in Descendants(root).OfType<ResponsiveWrapPanel>().Where(Visible))
            {
                var children = panel.Children.OfType<FrameworkElement>().Where(Visible).ToArray();
                var rectangles = children.Select(c => c.TransformToVisual(panel).TransformBounds(new Rect(0, 0, c.ActualWidth, c.ActualHeight))).ToArray();
                var overlap = false;
                for (var i = 0; i < rectangles.Length; i++) for (var j = i + 1; j < rectangles.Length; j++)
                    if (Math.Min(rectangles[i].Right, rectangles[j].Right) - Math.Max(rectangles[i].Left, rectangles[j].Left) > 1
                        && Math.Min(rectangles[i].Bottom, rectangles[j].Bottom) - Math.Max(rectangles[i].Top, rectangles[j].Top) > 1) overlap = true;
                Check(!overlap, label + " action row has no overlap");
            }
            Check(standardPageScroll is null || standardPageScroll.HorizontalScrollBarVisibility == ScrollBarVisibility.Disabled, label + " page horizontal scroll disabled");
        }
        try
        {
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter p) p.Restore();
            var source = Path.Combine(output, "Dokumen sintetis nama panjang untuk uji layout OTO 2026.pdf"); WriteHistoryAuditFixture(source);
            var hash = SHA256.HashData(File.ReadAllBytes(source));
            for (var wait = 0; wait < 50 && root.XamlRoot is null; wait++) await Task.Delay(100); await Settle();
            report.Add($"NATIVE_SCALE {(root.XamlRoot ?? throw new InvalidOperationException("XAML root not loaded")).RasterizationScale}; physical DPI unchanged; root fixed DIP cases are SIMULATED_CONTENT not hardware-DPI proof");
            var focused = Environment.GetCommandLineArgs().Contains("--focused");
            var sizes = new[] { (1093d, 500d), (1600d, 820d), (1280d, 650d), (960d, 410d), (1536d, 880d), (1463d, 748d), (1024d, 565d), (1920d, 990d), (720d, 1180d), (3440d, 1360d), (480d, 460d), (640d, 340d) };
            var screens = new (string Name, Action Open)[] {
                ("home-empty", ShowHome), ("catalog", ShowCatalog), ("favorites", ShowFavorites), ("secured", ShowSecured),
                ("assistant", ShowAssistant), ("admin", ShowAdmin), ("help", ShowHelp), ("settings", ShowSettings), ("reader-landing", () => ShowPdfReader(false)) };
            foreach (var size in focused ? Array.Empty<(double, double)>() : sizes)
            {
                root.Width = size.Item1; root.Height = size.Item2; responsiveSignature = null;
                foreach (var screen in screens)
                {
                    screen.Open(); await Settle(); Bounds($"SIMULATED {size.Item1}x{size.Item2} {screen.Name}");
                    if (size.Item1 is 480 or 960 && screen.Name is "home-empty" or "settings" or "reader-landing" or "secured" or "assistant") await Capture($"simulated-{size.Item1}-{screen.Name}");
                }
            }
            root.Width = 640; root.Height = 560; homeFiles.Add(source); chosenFiles.Add(source); ShowHome(); await Settle(); Bounds("home populated long name"); await Capture("home-populated");
            foreach (var tool in focused ? Array.Empty<ToolCard>() : ToolCatalog.All)
            {
                File.WriteAllText(Path.Combine(output, "stage.txt"), "empty " + tool.Id); ShowEmptyTool(tool); await Settle(); Bounds("empty-entry " + tool.Id);
            }
            root.Width = root.Height = double.NaN; AppWindow.Resize(new Windows.Graphics.SizeInt32(720, 720)); await Settle();
            foreach (var tool in ToolCatalog.All.Where(t => t.Available && t.Id != "edit"))
            {
                File.WriteAllText(Path.Combine(output, "stage.txt"), "workspace " + tool.Id); openingEmptyInput = true; try { ShowTool(tool); } finally { openingEmptyInput = false; } File.WriteAllText(Path.Combine(output, "stage.txt"), "workspace-opened " + tool.Id);
                await Settle(); files.Clear(); if (tool.Id is not "word-pdf" and not "images") files.Add(source); await Task.Delay(800); await Settle(); Bounds("workspace " + tool.Id);
            }
            await OpenPdfReaderAsync(source); await Task.Delay(500); Check(pdfReader!.ReadingDocument is not null, "reader synthetic document loaded");
            foreach (var width in new[] { 480, 640, 960, 1280 })
            {
                root.Width = width; root.Height = 540; responsiveSignature = null; await Settle(); Bounds("reader loaded " + width);
                Check(pdfReader!.ReadingViewport.ActualHeight >= 40, "reader visible canvas " + width); await Capture("reader-" + width);
            }
            var readerZoom = pdfReader!.ReadingViewport.ZoomFactor; var readingPage = pdfReader.ReadingPage;
            root.Width = 1200; await Settle(); root.Width = 640; await Settle();
            root.Height = 340; await Settle(); Check(pdfReader.ReadingViewport.ActualHeight >= 30, "reader short viewport remains usable"); await Capture("reader-short"); root.Height = 540;
            Check(pdfReader.ReadingPage == readingPage, "reader resize retains page identity");
            Check(Descendants(root).OfType<Button>().Any(b => AutomationProperties.GetName(b) == "Halaman"), "reader narrow thumbnails toggle available");
            await OpenEditorDocumentAsync(source); await Task.Delay(500); Check(liveEditor!.HasDocument, "editor synthetic document loaded");
            liveEditor!.ResponsiveDraft();
            var editorState = liveEditor!.ResponsiveState; var chosen = liveEditor!.ResponsiveSelection;
            foreach (var width in new[] { 480, 640, 960, 1280 })
            {
                root.Width = width; root.Height = 600; await Settle(); Bounds("editor draft " + width);
                Check(liveEditor!.ResponsiveViewport.ActualHeight >= 40, "editor visible canvas " + width); await Capture("editor-" + width);
            }
            root.Width = 640; root.Height = 340; await Settle(); Bounds("editor short viewport"); Check(liveEditor!.ResponsiveViewport.ActualHeight >= 30, "editor short viewport remains usable"); await Capture("editor-short"); root.Height = 600; await Settle();
            Check(liveEditor!.ResponsiveState == editorState && liveEditor!.ResponsiveSelection == chosen && liveEditor!.Dirty, "editor resize preserves draft and selection");
            liveEditor!.Restore(false);
            SetHistoryPageVisible(false);
            ShowUnifiedResult([source], "Hasil sintetis dengan ringkasan panjang dan nama file panjang", ShowHome); root.Width = 480; await Settle(); Bounds("result long name"); await Capture("result-480");
            savedJobs.Clear(); for (var i = 0; i < 60; i++) savedJobs.Add(new(Guid.NewGuid(), DateTimeOffset.Now.AddMinutes(-i), "merge", [source], "Dokumen sintetis " + i));
            foreach (var width in new[] { 480, 640, 960 }) { root.Width = width; root.Height = 540; BuildHistoryGallery(); await Settle(); Bounds("dense history " + width); Check(historyViewport!.ScrollableHeight > 0, "dense history scroll reachable " + width); await OpenInternalPdfAsync(source); if (historyReaderRender is not null) await historyReaderRender; await Settle(); Bounds("history preview " + width); Check(Descendants(root).OfType<Button>().Any(b => AutomationProperties.GetName(b) == "Tutup preview"), "history preview back reachable " + width); CloseHistoryPreview(); }
            root.Width = root.Height = double.NaN; AppWindow.Resize(new Windows.Graphics.SizeInt32(800, 640)); ShowCatalog(); await Settle();
            report.Add($"NATIVE_WINDOW pixels={AppWindow.Size.Width}x{AppWindow.Size.Height}; rootDIP={root.ActualWidth:0.0}x{root.ActualHeight:0.0}; scale={(root.XamlRoot ?? throw new InvalidOperationException("XAML root not loaded")).RasterizationScale}");
            Bounds("native restored catalog"); await Capture("native-restored-catalog");
            await OpenPdfReaderAsync(source).WaitAsync(TimeSpan.FromSeconds(20)); await Settle();
            Check(pdfReader!.View.ActualHeight <= historyPage.ActualHeight + 1 && pdfReader.ReadingViewport.ActualHeight >= 30, "return from history keeps reader canvas inside host viewport");
            ShowLiveEditor(); await Settle();
            Check(liveEditor!.View.ActualHeight <= historyPage.ActualHeight + 1 && liveEditor.ResponsiveViewport.ActualHeight >= 30, "return from history keeps editor canvas inside host viewport");
            ShowCatalog(); await Settle();
            Check(SHA256.HashData(File.ReadAllBytes(source)).SequenceEqual(hash), "source PDF SHA256 unchanged");
            report.Add("NOT_TESTED physical DPI changes, monitor move, Windows text scale, physical touch and screen reader UAT");
            if (interactive) { await OpenPdfReaderAsync(source); Title = "PERTAMINA OTO — RESPONSIVE SYNTHETIC AUDIT"; }
        }
        catch (Exception error) { failures++; report.Add("FAIL EXCEPTION " + error); }
        finally { File.WriteAllLines(Path.Combine(output, "audit.txt"), report); Environment.ExitCode = failures > 0 ? 1 : 0; }
    }
    internal async Task AuditResponsivePreviewAsync()
    {
        var output=Path.Combine(AppContext.BaseDirectory,"responsive-preview"); Directory.CreateDirectory(output);
        libraryTestPath=Path.Combine(output,"library.json"); savedJobs.Clear(); homeFiles.Clear(); chosenFiles.Clear();
        var source=Path.Combine(output,"source.pdf"); WriteHistoryAuditFixture(source);
        for(var i=0;i<50 && root.XamlRoot is null;i++) await Task.Delay(100);
        if(AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter p) p.Restore();
        AppWindow.Resize(new Windows.Graphics.SizeInt32(800,640)); root.UpdateLayout(); await Task.Delay(300);
        await OpenPdfReaderAsync(source).WaitAsync(TimeSpan.FromSeconds(20));
        Title="PERTAMINA OTO — RESPONSIVE SYNTHETIC PREVIEW";
        var flags=Environment.GetCommandLineArgs();
        savedJobs.Clear(); for(var i=0;i<(flags.Contains("--preview-empty-history")?0:60);i++) savedJobs.Add(new(Guid.NewGuid(),DateTimeOffset.Now.AddMinutes(-i),"merge",[source],"Dokumen sintetis "+i));
        foreach(var width in new[]{480,640,960})
        {
            AppWindow.Resize(new Windows.Graphics.SizeInt32(width+16,619)); await Task.Delay(300); BuildHistoryGallery(); root.UpdateLayout(); await Task.Delay(300);
            if(!flags.Contains("--preview-history-only")) { await OpenInternalPdfAsync(source).WaitAsync(TimeSpan.FromSeconds(20)); if(historyReaderRender is not null) await historyReaderRender.WaitAsync(TimeSpan.FromSeconds(20)); }
            if(!flags.Contains("--preview-history-only")) CloseHistoryPreview(); await Task.Delay(300);
        }
        AppWindow.Resize(new Windows.Graphics.SizeInt32(800,640)); ShowCatalog(); root.UpdateLayout(); await Task.Delay(300);
        var report=new List<string>();
        for(var cycle=0;cycle<4;cycle++)
        {
            ShowCatalog(); await Task.Delay(300); await OpenPdfReaderAsync(source).WaitAsync(TimeSpan.FromSeconds(20));
            root.UpdateLayout(); await Task.Delay(800);
            var rendered=Descendants(pdfReader!.View).OfType<Image>().Any(i=>i.Source is not null);
            report.Add((rendered?"PASS ":"FAIL ")+"catalog to retained reader cycle "+cycle);
            var bounded=pdfReader.View.ActualHeight<=historyPage.ActualHeight+1 && pdfReader.ReadingViewport.ActualHeight>=30;
            report.Add((bounded?"PASS ":"FAIL ")+"reader canvas bounded after history cycle "+cycle);
            File.WriteAllLines(Path.Combine(output,"audit.txt"),report);
        }
        if(flags.Contains("--preview-stability-exit")) { await Task.Delay(60000); report.Add("PASS native idle survives 60 seconds after navigation"); File.WriteAllLines(Path.Combine(output,"audit.txt"),report); Microsoft.UI.Xaml.Application.Current.Exit(); }
    }
    internal async Task AuditResponsiveDialogsAsync()
    {
        var output = Path.Combine(AppContext.BaseDirectory, "responsive-dialogs"); Directory.CreateDirectory(output);
        libraryTestPath = Path.Combine(output, "library.json"); savedJobs.Clear(); homeFiles.Clear(); chosenFiles.Clear(); defaultFolder = "Folder sintetis lokal";
        var report = new List<string>();
        void Check(bool ok, string label) { report.Add((ok ? "PASS " : "FAIL ") + label); File.WriteAllLines(Path.Combine(output, "audit.txt"), report); }
        async Task Settle() { root.UpdateLayout(); await Task.Delay(300); }
        try
        {
            for(var i=0;i<50 && root.XamlRoot is null;i++) await Task.Delay(100);
            if(AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter p) p.Restore();
            foreach(var size in new[] {(480,340),(640,500),(960,650)})
            {
                root.Width = root.Height = double.NaN;
                var scale = root.XamlRoot!.RasterizationScale;
                AppWindow.Resize(new Windows.Graphics.SizeInt32((int)((size.Item1+16)*scale),(int)((size.Item2+79)*scale))); await Settle();
                ShowSettings(); await Settle(); aboutSettingsButton!.Focus(FocusState.Programmatic); ShowAboutDrawer(aboutSettingsButton); await Settle();
                Check(aboutPanel is not null && aboutPanel.ActualWidth <= root.ActualWidth+1 && aboutPanel.ActualHeight <= root.ActualHeight+1, $"about native bounds {size}");
                var expectedActions = new[] { "Tutup Tentang aplikasi", "Periksa update", "Riwayat versi", "Panduan penggunaan", "Laporkan masalah", "Lisensi komponen", "Detail teknis", "Salin info aplikasi" };
                var actualActions = aboutButtons.Select(AutomationProperties.GetName).ToArray();
                Check(actualActions.Length == expectedActions.Length && expectedActions.All(name => actualActions.Count(actual => actual == name) == 1), $"about named actions {size}");
                var footer = aboutButtons[^1]; var bounds=footer.TransformToVisual(root).TransformBounds(new Rect(0,0,footer.ActualWidth,footer.ActualHeight));
                Check(bounds.Top>=0 && bounds.Bottom<=root.ActualHeight+1 && bounds.Right<=root.ActualWidth+1, $"about copy footer reachable {size}");
                Check(Descendants(aboutPanel!).OfType<ScrollViewer>().First().ScrollableHeight>0 || size.Item2>=650, $"about content scrolls on short viewport {size}");
                CloseAboutDrawer(); await Settle(); Check(!messageOpen && ReferenceEquals(FocusManager.GetFocusedElement(root.XamlRoot),aboutSettingsButton), $"about closes and restores focus {size}");
                var pending=ShowAboutLicensesAsync(); await Settle();
                var dialog=VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot).SelectMany(p=>Descendants(p.Child).Prepend(p.Child)).OfType<ContentDialog>().First();
                var buttons=Descendants(dialog).OfType<Button>().Where(b=>b.Visibility==Visibility.Visible && b.ActualHeight>0).ToArray();
                Check(buttons.Length>0 && buttons.All(b=>{var r=b.TransformToVisual(root).TransformBounds(new Rect(0,0,b.ActualWidth,b.ActualHeight));return r.Left>=-1 && r.Right<=root.ActualWidth+1 && r.Top>=-1 && r.Bottom<=root.ActualHeight+1;}), $"license dialog close action bounded {size}");
                var content=(FrameworkElement)dialog.Content; Check(content.ActualWidth<=root.ActualWidth && content.ActualHeight<=root.ActualHeight, $"license scroll content bounded {size}");
                dialog.Hide(); await pending; await Settle(); Check(!messageOpen,$"license dismissal releases modal state {size}");
                var version=SafeRunningVersion(); await CheckLocalUpdateAsync(); Check(!messageOpen && !checkingUpdate && SafeRunningVersion()==version,$"audit guard prevents updater mutation {size}");
            }
            report.Add("NOT_TESTED physical DPI, screen reader and user updater installation");
        }
        catch(Exception error) {report.Add("FAIL "+error);}
        finally {File.WriteAllLines(Path.Combine(output,"audit.txt"),report); Environment.ExitCode=report.Any(l=>l.StartsWith("FAIL", StringComparison.Ordinal))?1:0;}
    }
    private sealed partial class LiveEditor
    {
        internal void ResponsiveDraft() => Add(new(0, "text", 40, 100, 280, 35, "DRAF SINTETIS", 16));
        internal string ResponsiveState => State;
        internal int ResponsiveSelection => chosen;
        internal ScrollViewer ResponsiveViewport => viewport;
    }
}
#endif
