#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private static readonly JsonSerializerOptions WindowAuditJson = new() { WriteIndented = true, IncludeFields = true };
    internal async Task AuditWindowSizeAsync()
    {
        var output = Path.Combine(AppContext.BaseDirectory, "window-size-audit");
        Directory.CreateDirectory(output);
        libraryTestPath = Path.Combine(output, "library.json");
        defaultFolder = "Folder audit sintetis";
        Title = "PERTAMINA OTO — TASK04 SYNTHETIC WINDOW AUDIT";
        var shell = (FrameworkElement)Content;
        var observations = new List<object>();
        var checks = new List<string>();
        void Check(bool ok, string label) => checks.Add((ok ? "PASS " : "FAIL ") + label);
        void Save()
        {
            File.WriteAllText(Path.Combine(output, "observations.json"), JsonSerializer.Serialize(observations, WindowAuditJson));
            File.WriteAllLines(Path.Combine(output, "audit.txt"), checks);
        }
        async Task Settle() { root.UpdateLayout(); await Task.Delay(220); }
        async Task Size(int width, int height)
        {
            shell.Width = shell.Height = root.Width = root.Height = double.NaN;
            var scale = (root.XamlRoot ?? throw new InvalidOperationException("Root not loaded")).RasterizationScale;
            AppWindow.ResizeClient(new SizeInt32((int)Math.Ceiling(width * scale), (int)Math.Ceiling(height * scale)));
            await Settle();
        }
        bool Present(FrameworkElement node)
        {
            for (DependencyObject? p = node; p is not null; p = VisualTreeHelper.GetParent(p))
                if (p is UIElement { Visibility: Visibility.Collapsed }) return false;
            return node.IsLoaded && node.ActualWidth > 0 && node.ActualHeight > 0;
        }
        Rect RectOf(FrameworkElement node) => node.TransformToVisual(shell).TransformBounds(new Rect(0, 0, node.ActualWidth, node.ActualHeight));
        async Task Capture(string name)
        {
            var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync((UIElement)Content);
            var pixels = (await bitmap.GetPixelsAsync()).ToArray();
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, pixels);
            await encoder.FlushAsync();
        }
        async Task Observe(string route, FrameworkElement? scope = null, FrameworkElement? document = null)
        {
            scope ??= shell;
            var issues = new List<string>(); var count = 0; var scrollCount = 0;
            foreach (var control in Descendants(scope).OfType<FrameworkElement>().Where(n => n is ButtonBase or TextBox or ComboBox or NumberBox or PasswordBox).Where(Present))
            {
                count++;
                var rect = RectOf(control);
                var name = AutomationProperties.GetName(control);
                if (name.Length == 0 && control is Button b) name = b.Content as string ?? "button";
                var scrolls = new List<ScrollViewer>();
                for (DependencyObject? p = VisualTreeHelper.GetParent(control); p is not null && p != shell; p = VisualTreeHelper.GetParent(p))
                {
                    if (p is ScrollViewer v) scrolls.Add(v);
                    if (p is FrameworkElement frame && Present(frame))
                    {
                        var bounds = RectOf(frame);
                        if (rect.Left < bounds.Left - 2 || rect.Right > bounds.Right + 2)
                            issues.Add("horizontal ancestor clipping " + control.GetType().Name + ":" + name);
                    }
                }
                if (rect.Left < -2 || rect.Right > shell.ActualWidth + 2) issues.Add("horizontal shell clipping " + name);
                // Scrollbar template repeat buttons are not the application's primary targets.
                if (rect.Height < 24 && control is not RepeatButton) issues.Add("target height below 24 " + name);
                if (rect.Top < -2 || rect.Bottom > shell.ActualHeight + 2)
                {
                    if (scrolls.Count == 0) issues.Add("vertical fixed control clipped " + name);
                    else
                    {
                        // Inspect controls outside the initially visible area, then restore scroll.
                        var offsets = scrolls.Select(v => v.VerticalOffset).ToArray();
                        // BringIntoView alone may be deferred by a scroller during navigation.
                        // Exercise each actual scroll ancestor explicitly, then verify the result.
                        foreach (var scroll in scrolls)
                        {
                            var target = RectOf(control); var view = RectOf(scroll);
                            var delta = target.Bottom > view.Bottom ? target.Bottom - view.Bottom : target.Top < view.Top ? target.Top - view.Top : 0;
                            scroll.ChangeView(null, Math.Clamp(scroll.VerticalOffset + delta, 0, scroll.ScrollableHeight), null, true);
                            await Task.Delay(60); root.UpdateLayout();
                        }
                        var reached = RectOf(control);
                        if (reached.Top < -2 || reached.Bottom > shell.ActualHeight + 2)
                            issues.Add("vertical scroll reach failed " + name);
                        for (var i = 0; i < scrolls.Count; i++) scrolls[i].ChangeView(null, offsets[i], null, true);
                        scrollCount++;
                    }
                }
            }
            var work = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest).WorkArea;
            observations.Add(new { Route = route, Mode = "ACTUAL_NATIVE_RESIZE_CURRENT_MONITOR", OuterPx = AppWindow.Size, ClientPx = AppWindow.ClientSize,
                ClientDip = new { Width = shell.ActualWidth, Height = shell.ActualHeight }, RootDip = new { Width = root.ActualWidth, Height = root.ActualHeight },
                Scale = (root.XamlRoot ?? throw new InvalidOperationException("Root not loaded")).RasterizationScale, WorkAreaPx = work, Controls = count, OffscreenScrollControls = scrollCount,
                DocumentDip = document is null ? null : new { Width = document.ActualWidth, Height = document.ActualHeight }, Issues = issues.Distinct().ToArray() });
            if (Environment.GetCommandLineArgs().Contains("--window-final"))
                Check(issues.Count == 0, "controls reachable and unclipped " + route);
            Save();
        }
        async Task Pause(string stage)
        {
            await Capture(stage);
            File.WriteAllText(Path.Combine(output, "stage.txt"), stage);
            if (!Environment.GetCommandLineArgs().Contains("--window-visual")) return;
            var next = Path.Combine(output, stage + ".continue");
            var lastSize = AppWindow.Size; var lastPosition = AppWindow.Position;
            for (var i = 0; i < 600 && !File.Exists(next); i++)
            {
                await Task.Delay(500);
                if (!lastSize.Equals(AppWindow.Size) || !lastPosition.Equals(AppWindow.Position))
                {
                    lastSize = AppWindow.Size; lastPosition = AppWindow.Position;
                    observations.Add(new { Route = "native-interaction-" + stage, OuterPx = lastSize, PositionPx = lastPosition,
                        ClientPx = AppWindow.ClientSize, ClientDip = new { Width = shell.ActualWidth, Height = shell.ActualHeight },
                        Scale = root.XamlRoot!.RasterizationScale, Presenter = ((OverlappedPresenter)AppWindow.Presenter).State.ToString(),
                        DraftDirty = liveEditor?.Dirty, DraftState = liveEditor?.ResponsiveState, Selection = liveEditor?.ResponsiveSelection });
                    Save();
                }
            }
            Check(File.Exists(next), "native visual checkpoint released " + stage);
        }
        try
        {
            for (var i = 0; i < 50 && root.XamlRoot is null; i++) await Task.Delay(100);
            var presenter = (OverlappedPresenter)AppWindow.Presenter;
            await Settle(); await Observe("startup-maximized"); await Pause("01-maximized");
            presenter.Restore(); await Settle();
            var source = Path.Combine(output, "Synthetic window audit.pdf"); WriteHistoryAuditFixture(source);
            var hash = SHA256.HashData(File.ReadAllBytes(source));
            var flags = Environment.GetCommandLineArgs();
            var widths = new[] { 360, 400, 440, 480, 520, 560, 600, 640, 720, 800 };
            var heights = new[] { 280, 320, 360, 400, 440, 480, 520, 560, 600, 640 };
            var probes = widths.Select(w => (w, 800)).Concat(heights.Select(h => (800, h)))
                .Concat(new[] { (480, 480), (520, 480), (560, 480), (600, 480), (640, 480), (560, 520), (600, 520), (640, 520), (640, 560), (720, 560) }).ToArray();
            if (flags.Contains("--window-inventory")) probes = [(480, 640), (500, 620), (520, 620), (520, 640), (560, 640)];
            if (flags.Contains("--window-search")) probes = [(480, 640), (520, 640), (560, 640), (600, 640), (640, 640), (640, 680), (640, 720), (640, 760), (640, 800), (720, 720), (800, 680), (800, 720), (800, 760), (960, 640), (960, 680), (960, 720)];
            if (flags.Contains("--window-candidate")) probes = [(600, 760), (640, 740), (640, 760), (640, 800), (800, 760), (960, 640)];
            if (flags.Contains("--window-final")) probes = [(640, 760)];
            foreach (var (width, height) in probes)
            {
                await Size(width, height);
                if (flags.Contains("--window-inventory") || flags.Contains("--window-final"))
                {
                    foreach (var screen in new (string Name, Action Open)[] { ("home", ShowHome), ("catalog", ShowCatalog), ("favorites", ShowFavorites), ("storage", ShowSecured), ("assistant", ShowAssistant), ("admin", ShowAdmin), ("help", ShowHelp), ("settings", ShowSettings), ("reader-landing", () => ShowPdfReader(false)) })
                    { screen.Open(); await Settle(); await Observe(screen.Name); }
                    foreach (var tool in ToolCatalog.All) { ShowEmptyTool(tool); await Settle(); await Observe("intake-" + tool.Id); }
                    foreach (var tool in ToolCatalog.All.Where(t => t.Available && t.Id != "edit"))
                    {
                        openingEmptyInput = true; try { ShowTool(tool); } finally { openingEmptyInput = false; }
                        files.Clear(); if (tool.Id is not "word-pdf" and not "images") files.Add(source);
                        await Task.Delay(650); await Settle(); await Observe("workspace-" + tool.Id);
                    }
                    savedJobs.Clear(); for (var i = 0; i < 40; i++) savedJobs.Add(new(Guid.NewGuid(), DateTimeOffset.Now.AddMinutes(-i), "merge", [source], "Synthetic " + i));
                    BuildHistoryGallery(); await Settle(); await Observe("history-populated");
                    await OpenInternalPdfAsync(source); if (historyReaderRender is not null) await historyReaderRender; await Observe("history-preview"); CloseHistoryPreview();
                    ShowUnifiedResult([source], "Hasil sintetis · File tetap di perangkat", ShowHome); await Settle(); await Observe("result-single");
                    ShowUnifiedResult([source, source], "Dua hasil sintetis", ShowHome); await Settle(); await Observe("result-multiple");
                    homeFiles.Clear(); chosenFiles.Clear(); homeFiles.Add(source); chosenFiles.Add(source); ShowOrganizer();
                    for (var i = 0; i < 100 && importing; i++) await Task.Delay(100);
                    await Settle(); await Observe("organizer-loaded");
                }
                await OpenPdfReaderAsync(source); await Settle(); await Observe("reader-loaded", document: pdfReader!.ReadingViewport);
                if (flags.Contains("--window-search") || flags.Contains("--window-candidate") || flags.Contains("--window-final"))
                {
                    var hits = await pdfReader!.WindowAuditSearchAsync(); await Settle();
                    Check(hits >= 6, "real search populated result list " + width + "x" + height);
                    await Observe("reader-search-expanded", document: pdfReader.ReadingViewport);
                    if (flags.Contains("--window-candidate")) await Pause("candidate-search-" + width + "x" + height);
                    if (flags.Contains("--window-final")) await Pause("final-search-minimum");
                    if ((width, height) is (520, 640) or (640, 800) or (800, 760)) await Pause("search-" + width + "x" + height);
                    pdfReader.WindowAuditClearSearch();
                }
                await OpenEditorDocumentAsync(source); await Settle(); liveEditor!.ResponsiveDraft(); await Observe("editor-draft", document: liveEditor.ResponsiveViewport);
                var state = liveEditor.ResponsiveState; var selection = liveEditor.ResponsiveSelection;
                await Size(width + 30, height + 30); await Size(width, height);
                Check(liveEditor.ResponsiveState == state && liveEditor.ResponsiveSelection == selection && liveEditor.Dirty, "resize preserves draft " + width + "x" + height);
                liveEditor.Restore(false); ShowHome();
                auditPrintPrinters = () => [];
                auditPrintDialog = async dialog =>
                {
                    await Task.Delay(600); dialog.UpdateLayout();
                    await Observe("print-no-printer", dialog);
                    foreach (var button in Descendants(dialog).OfType<Button>().Where(b => AutomationProperties.GetName(b) == "Cetak dokumen" || Equals(b.Content, "Batal")))
                    {
                        var r = RectOf(button);
                        observations.Add(new { Route = "print-footer", ClientDip = new { Width = shell.ActualWidth, Height = shell.ActualHeight }, Name = AutomationProperties.GetName(button), Bounds = r, FullyInClient = r.Left >= 0 && r.Top >= 0 && r.Right <= shell.ActualWidth && r.Bottom <= shell.ActualHeight });
                    }
                    if ((width, height) is (640, 560) or (480, 480) or (520, 640)) await Capture("print-" + width + "x" + height);
                    dialog.Hide(); Save();
                };
                await ShowPrintAsync(source, "Synthetic audit · No jobs"); auditPrintPrinters = null; auditPrintDialog = null;
                if (flags.Contains("--window-final")) await AuditWindowPrintStatesAsync(source, Check, async (label, dialog) =>
                {
                    await Observe(label, dialog);
                    await Capture(label);
                    if (label == "print-simulated-ready") await Pause("final-print-ready");
                });
                if ((width, height) is (640, 560) or (480, 480) or (520, 640)) { await OpenEditorDocumentAsync(source); await Pause("editor-" + width + "x" + height); liveEditor!.Restore(false); }
            }
            Check(hash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(source))), "source unchanged");
            if (flags.Contains("--window-final"))
            {
                var minimum = windowSizePolicy!.Minimum;
                foreach (var delta in new[] { -1, 0, 1 })
                {
                    AppWindow.Resize(new SizeInt32(minimum.Width + delta, minimum.Height + delta)); await Settle();
                    Check(AppWindow.Size.Width == minimum.Width + Math.Max(0, delta) && AppWindow.Size.Height == minimum.Height + Math.Max(0, delta), "native boundary min " + delta);
                    await Observe("native-boundary-" + delta);
                }
                await OpenEditorDocumentAsync(source); liveEditor!.ResponsiveDraft();
                var draft = liveEditor.ResponsiveState; var selection = liveEditor.ResponsiveSelection;
                await Pause("final-native-editor");
                Check(liveEditor.ResponsiveState == draft && liveEditor.ResponsiveSelection == selection && liveEditor.Dirty, "native interaction preserves draft and selection");
                await Observe("native-after-interaction", document: liveEditor.ResponsiveViewport);
                liveEditor.Restore(false);
            }
            await Size(640, 560); ShowCatalog(); await Pause("02-native-restored");
            Check(presenter.HasTitleBar && presenter.HasBorder && presenter.IsResizable && presenter.IsMinimizable && presenter.IsMaximizable, "caption and controls retained");
        }
        catch (Exception error) { checks.Add("FAIL exception " + error); Environment.ExitCode = 1; }
        finally { auditPrintPrinters = null; auditPrintDialog = null; Save();
            if (checks.Any(c => c.StartsWith("FAIL ", StringComparison.Ordinal))) Environment.ExitCode = 1; }
    }
    private sealed partial class PdfReaderView
    {
        internal async Task<int> WindowAuditSearchAsync()
        {
            searchInput.Text = "Tim";
            await SearchAsync(); await searchNavigation;
            return searchHits.Length;
        }
        internal void WindowAuditClearSearch() => ClearSearch();
    }
}
#endif
