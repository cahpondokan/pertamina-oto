#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // Isolated native regression: deliberately retain window size and child counts
    // while real routes replace controls. Never invalidate the production cache here.
    internal async Task AuditUiRegressionAsync(bool interactive)
    {
        var output = Path.Combine(AppContext.BaseDirectory, "ui-regression-audit"); Directory.CreateDirectory(output);
        var report = new List<string>(); var failures = 0;
        void Check(bool ok, string label)
        {
            report.Add((ok ? "PASS " : "FAIL ") + label); if (!ok) failures++;
            File.WriteAllLines(Path.Combine(output, "audit-current.txt"), report);
        }
        async Task Settle(int delay = 180) { root.UpdateLayout(); await Task.Delay(delay); root.UpdateLayout(); }
        bool Visible(FrameworkElement element)
        {
            for (DependencyObject? p = element; p is not null && p != root; p = VisualTreeHelper.GetParent(p))
                if (p is UIElement { Visibility: Visibility.Collapsed }) return false;
            return element.ActualWidth > 0 && element.ActualHeight > 0;
        }
        Rect Box(FrameworkElement element, UIElement relative) => element.TransformToVisual(relative).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
        void CatalogRows(string label)
        {
            var actual = centeredGrid.Children.OfType<FrameworkElement>().ToArray();
            Check(centeredGrid.IsLoaded && actual.Length == 16 && actual.All(Visible), label + " actual 16-tool catalog attached");
            Check(actual.All(c => c.ActualWidth > 0 && c.ActualWidth <= centeredGrid.ActualWidth + 1
                && Math.Abs(c.ActualHeight - 160) < 1), label + " content cards bounded with consistent height");
            foreach (var row in actual.GroupBy(c => Math.Round(Box(c, centeredGrid).Top, 1)))
            {
                var left = Box(row.First(), centeredGrid).Left;
                var right = centeredGrid.ActualWidth - Box(row.Last(), centeredGrid).Right;
                Check(left >= 0 && Math.Abs(left - right) <= 1 / root.XamlRoot.RasterizationScale + .01,
                    label + " actual row centered " + row.Key);
            }
        }
        void Separate(Grid grid, string label)
        {
            var nodes = grid.Children.OfType<FrameworkElement>().ToArray();
            Check(nodes.All(Visible), label + " all required panels have dimensions");
            var overlap = false;
            for (var i = 0; i < nodes.Length; i++) for (var j = i + 1; j < nodes.Length; j++)
            {
                var a = Box(nodes[i], grid); var b = Box(nodes[j], grid);
                if (Math.Min(a.Right, b.Right) - Math.Max(a.Left, b.Left) > 1 && Math.Min(a.Bottom, b.Bottom) - Math.Max(a.Top, b.Top) > 1) overlap = true;
            }
            Check(!overlap, label + " sibling panels do not overlap or occlude");
            Check(nodes.All(n => { var b = Box(n, grid); return b.Left >= -1 && b.Right <= grid.ActualWidth + 1; }), label + " panels fit container width");
        }
        void Bounds(string label)
        {
            var clipped = new List<string>();
            foreach (var c in Descendants(root).OfType<FrameworkElement>().Where(n => n is ButtonBase or TextBox or PasswordBox or ComboBox or NumberBox).Where(Visible))
            {
                var b = Box(c, root); if (b.Bottom <= 0 || b.Top >= root.ActualHeight) continue;
                if (b.Left < -1 || b.Right > root.ActualWidth + 1) clipped.Add(c.GetType().Name + ":" + AutomationProperties.GetName(c));
            }
            Check(clipped.Count == 0, label + " visible actions horizontally reachable " + string.Join(";", clipped.Take(6)));
        }
        var captured = new HashSet<string>();
        async Task Capture(string name)
        {
            if (Environment.GetCommandLineArgs().Contains("--stress-reader")) return;
            if (!captured.Add(name)) return;
            var bitmap = new RenderTargetBitmap(); await bitmap.RenderAsync(root); var pixels = await bitmap.GetPixelsAsync();
            var file = await (await StorageFolder.GetFolderFromPathAsync(output)).CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, pixels.ToArray()); await encoder.FlushAsync();
        }
        try
        {
            libraryTestPath = Path.Combine(output, "library.json"); savedJobs.Clear(); homeFiles.Clear(); chosenFiles.Clear();
            var source = Path.Combine(output, "Dokumen sintetis nama panjang untuk audit tampilan.pdf"); WriteHistoryAuditFixture(source);
            var second = Path.Combine(output, "Dokumen kedua untuk audit.pdf"); WriteHistoryAuditFixture(second);
            var image = Path.Combine(output, "Gambar sintetis.png"); File.Copy(Path.Combine(AppContext.BaseDirectory, "Assets/oto.png"), image, true);
            var word = Path.Combine(output, "Dokumen sintetis.docx");
            if (!File.Exists(word))
            using (var zip = System.IO.Compression.ZipFile.Open(word, System.IO.Compression.ZipArchiveMode.Create))
            {
                void Entry(string name, string value) { using var writer = new StreamWriter(zip.CreateEntry(name).Open()); writer.Write(value); }
                Entry("[Content_Types].xml", "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/></Types>");
                Entry("_rels/.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"word/document.xml\"/></Relationships>");
                Entry("word/document.xml", "<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body><w:p><w:r><w:t>DOKUMEN SINTETIS AUDIT UI</w:t></w:r></w:p><w:sectPr><w:pgSz w:w=\"11906\" w:h=\"16838\"/></w:sectPr></w:body></w:document>");
            }
            var hash = SHA256.HashData(File.ReadAllBytes(source));
            if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter) presenter.Restore();
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1920, 1040)); await Settle();
            // Exact desktop breakpoints from the PDB-verified 0.1.0 baseline,
            // including both sides of each transition and revisiting a width.
            foreach (var size in new[] { (900, 3, 1, 2), (1120, 3, 3, 2), (1280, 3, 3, 3), (1319, 3, 3, 3), (1320, 6, 3, 3), (1699, 6, 3, 3), (1700, 6, 3, 4), (1280, 3, 3, 3), (480, 2, 1, 1) })
            {
                root.Width = size.Item1; root.Height = 760; sidebarCollapsed = false;
                selected = null; files.Clear(); homeFiles.Clear(); chosenFiles.Clear();
                ShowHome(); await Settle();
                CatalogRows($"baseline home {size.Item1}"); Bounds($"baseline home {size.Item1}");
                ShowCatalog(); await Settle();
                CatalogRows($"baseline catalog {size.Item1}"); Bounds($"baseline catalog {size.Item1}");
                if (size.Item1 is 1280 or 1320) await Capture($"baseline-{size.Item1}-catalog");
                ShowHome(); await Settle(); if (size.Item1 is 1120 or 1320) await Capture($"baseline-{size.Item1}-home");
            }
            foreach (var width in new[] { 1920, 640, 480 })
            {
                root.Width = width; root.Height = 760; await Settle();
                foreach (var screen in new (string Name, Action Open)[] { ("activity", ShowActivity), ("secured", ShowSecured), ("assistant", ShowAssistant) })
                {
                    screen.Open(); await Settle();
                    var frame = body.Children.OfType<Grid>().Last();
                    var panel = (FrameworkElement)frame.Children[0];
                    Check(Math.Abs(Box(panel, body).Left - body.Padding.Left) < 1, $"baseline support left alignment {width} {screen.Name}");
                    Check(Math.Abs(panel.ActualWidth - Math.Min(850, frame.ActualWidth)) < 1, $"baseline support bounded width {width} {screen.Name}");
                    Bounds($"baseline support {width} {screen.Name}"); await Capture($"baseline-{width}-{screen.Name}");
                }
            }
            // Approved entry design replaces ONLY the former special merge/remove
            // empty states. Keep all unrelated baseline and loaded-workspace gates.
            var expectedEntries = new (string Id, string Title, string Picker, string Hint)[]
            {
                ("merge", "Gabungkan PDF", "Pilih PDF", "Minimal 2 PDF untuk digabungkan."),
                ("split", "Pisahkan PDF", "Pilih PDF", "Satu PDF untuk memulai."),
                ("remove", "Hapus halaman PDF", "Pilih PDF", "Sumber tetap utuh."),
                ("extract", "Ambil halaman PDF", "Pilih PDF", "Satu PDF untuk memulai."),
                ("organize", "Atur halaman PDF", "Pilih PDF", "Satu atau beberapa PDF."),
                ("compress", "Kompres PDF", "Pilih PDF", "Kompresi lossless."),
                ("images", "Gambar ke PDF", "Pilih gambar", "JPG atau PNG."),
                ("word-pdf", "Word ke PDF", "Pilih DOCX", "A4 portrait · Perlu Word lokal berlisensi."),
                ("pdf-jpg", "PDF ke JPG", "Pilih PDF", "Pilih halaman setelah dibuka."),
                ("rotate", "Putar halaman", "Pilih PDF", "90°, 180°, atau 270°."),
                ("numbers", "Nomor halaman", "Pilih PDF", "Nomor di bawah halaman."),
                ("watermark", "Watermark", "Pilih PDF", "Teks penanda dokumen."),
                ("crop", "Potong halaman", "Pilih PDF", "Bukan penghapusan data."),
                ("edit", "Edit isi PDF", "Pilih PDF", "Edit teks yang didukung."),
                ("unlock", "Buka kunci PDF", "Pilih PDF", "Perlu kata sandi pemilik."),
                ("protect", "Kunci PDF", "Pilih PDF", "Kata sandi pembuka baru.")
            };
            Check(expectedEntries.Select(e => e.Id).Order().SequenceEqual(ToolCatalog.All.Where(t => t.Available).Select(t => t.Id).Order()), "entry matrix covers exactly all 16 active tools");
            foreach (var width in new[] { 1920, 1366, 960, 640, 480 })
            {
                root.Width = width; root.Height = width >= 1366 ? 930 : 650; sidebarCollapsed = false;
                double? referenceHeight = null; double? referenceButtonWidth = null;
                foreach (var entry in expectedEntries)
                {
                    var tool = ToolCatalog.All.Single(t => t.Id == entry.Id);
                    ShowEmptyTool(tool); await Settle();
                    var label = $"uniform entry {width} {entry.Id}";
                    if (entry.Id == "merge")
                    {
                        // Approved Merge landing has its own Biblo layout and two shortcut actions.
                        Check(MergeEmptyActive && Descendants(emptyPanel!).OfType<TextBlock>().Any(t => t.Text == "Gabungkan PDF"), label + " approved Merge heading");
                        var mergeActions = Descendants(emptyPanel!).OfType<Button>().ToArray();
                        Check(mergeActions.Length == 2 && mergeActions.All(b => b.ActualHeight >= 44), label + " two bounded native actions");
                        Check(mergeActions.Any(b => AutomationProperties.GetAcceleratorKey(b) == "Ctrl+O")
                            && mergeActions.Any(b => AutomationProperties.GetAcceleratorKey(b) == "Ctrl+V"), label + " actual shortcut discovery");
                        Bounds(label); await Capture("entry-" + width + "-merge"); continue;
                    }
                    var panel = emptyPanel!; var content = (StackPanel)panel.Child;
                    var actions = (StackPanel)content.Children[3];
                    var pick = (Button)actions.Children[0]; var paste = (Button)actions.Children[1];
                    referenceHeight ??= panel.ActualHeight; referenceButtonWidth ??= pick.ActualWidth;
                    Check(((TextBlock)body.Children[0]).Text == entry.Title, label + " correct heading");
                    Check(content.Children.Count == 5 && content.Children[0] is Border { Child: Viewbox }, label + " same icon/title/hint/actions/status structure");
                    Check(((TextBlock)content.Children[1]).Text == "Mulai dengan dokumen Anda" && ((TextBlock)content.Children[2]).Text == "Seret file ke sini", label + " shared copy");
                    Check(AutomationProperties.GetName(pick) == entry.Picker && AutomationProperties.GetName(paste) == "Tempel file", label + " correct format and accessible names");
                    Check(AutomationProperties.GetAcceleratorKey(pick) == "Ctrl+O" && AutomationProperties.GetAcceleratorKey(paste) == "Ctrl+V", label + " shortcut discovery");
                    Check(emptyNotice!.Text == entry.Hint && emptyNotice.TextAlignment == TextAlignment.Center, label + " truthful centered capability hint");
                    Check(Math.Abs(pick.ActualWidth - paste.ActualWidth) < 1 && Math.Abs(pick.ActualWidth - referenceButtonWidth.Value) < 1, label + " consistent action width");
                    Check(Math.Abs(pick.ActualWidth - Math.Min(320, content.ActualWidth)) < 1, label + " bounded 320 DIP actions");
                    Check(Math.Abs(pick.ActualHeight - 46) < 1 && Math.Abs(paste.ActualHeight - 46) < 1, label + " equal 46 DIP action heights");
                    Check(width < 960 || Math.Abs(panel.ActualHeight - referenceHeight.Value) < 1, label + " desktop card height consistent");
                    Check(pick.Background is SolidColorBrush { Color: { R: 8, G: 105, B: 229 } } && paste.Background is SolidColorBrush { Color: { R: 255, G: 255, B: 255 } }, label + " blue select and white paste, no destructive select color");
                    var actionBox = Box(actions, panel);
                    Check(actionBox.Left >= panel.Padding.Left && actionBox.Right <= panel.ActualWidth - panel.Padding.Right + 1, label + " actions inside card");
                    Bounds(label);
                    if (width is 1920 or 640 or 480) await Capture($"entry-{width}-{entry.Id}");
                }
            }
            root.Width = 1920; root.Height = 930;
            foreach (var tool in ToolCatalog.All.Where(t => t.Available && t.Id != "edit"))
            {
                selected = null; activeToolControls = null; files.Clear(); homeFiles.Clear(); chosenFiles.Clear(); toolDrafts.Remove(tool.Id);
                ShowEmptyTool(tool); await Settle(); var entryPanel = emptyPanel;
                await AcceptEmptyInputsAsync(Array.Empty<IStorageItem>());
                Check(emptyPanel == entryPanel && emptyNotice!.Text == "Belum ada file dengan format yang sesuai.", "entry cancel/empty input retains " + tool.Id);
                await AcceptEmptyInputsAsync([await StorageFile.GetFileFromPathAsync(tool.Id == "images" ? source : image)]);
                Check(emptyPanel == entryPanel && emptyNotice!.Text == "Belum ada file dengan format yang sesuai.", "entry wrong format retained " + tool.Id);
                if (tool.Id is not ("merge" or "images" or "organize"))
                {
                    var valid = await StorageFile.GetFileFromPathAsync(tool.Id == "word-pdf" ? word : source);
                    // Distinct paths exercise the existing one-document guard.
                    var otherPath = tool.Id == "word-pdf" ? Path.Combine(output, "Second.docx") : second;
                    if (tool.Id == "word-pdf") File.Copy(word, otherPath, true);
                    await AcceptEmptyInputsAsync([valid, await StorageFile.GetFileFromPathAsync(otherPath)]);
                    Check(emptyPanel == entryPanel && emptyNotice!.Text.StartsWith("Alat ini menerima satu dokumen.", StringComparison.Ordinal), "entry rejects multiple documents " + tool.Id);
                }
                var inputs = new List<IStorageItem> { await StorageFile.GetFileFromPathAsync(tool.Id == "images" ? image : tool.Id == "word-pdf" ? word : source) };
                if (tool.Id is "merge" or "organize") inputs.Add(await StorageFile.GetFileFromPathAsync(second));
                await AcceptEmptyInputsAsync(inputs); await Settle(650);
                Check(!entryPanel!.IsLoaded && toolWorkspace.IsLoaded && toolWorkspace.Children.Count == 2, "entry to loaded workspace " + tool.Id);
                Separate(toolWorkspace, "entry loaded " + tool.Id);
            }
            root.Width = 1920; root.Height = 930; await Settle();
            if (failures > 0) throw new InvalidOperationException("Baseline recovery checks failed; stop before the full matrix.");
            foreach (var name in new[] { "arrow-right", "external-link" })
            {
                var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "History", name + ".svg");
                Check(File.Exists(iconPath), "reader icon packaged " + name);
                if (!File.Exists(iconPath)) continue;
                var iconSource = new SvgImageSource();
                var opened = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                iconSource.Opened += (_, _) => opened.TrySetResult(true);
                iconSource.OpenFailed += (_, _) => opened.TrySetResult(false);
                var icon = new Image { Source = iconSource, Width = 24, Height = 24 };
                body.Children.Add(icon); iconSource.UriSource = new Uri(iconPath);
                await Task.WhenAny(opened.Task, Task.Delay(3000));
                Check(opened.Task.IsCompletedSuccessfully && opened.Task.Result, "reader icon native decode " + name);
                body.Children.Remove(icon);
            }
            var smoke = Environment.GetCommandLineArgs().Contains("--repro-only");
            var screensOnly = Environment.GetCommandLineArgs().Contains("--screens-only");
            var stressReader = Environment.GetCommandLineArgs().Contains("--stress-reader");
            foreach (var width in smoke ? new[] { 1920 } : stressReader ? new[] { 1920, 640 } : new[] { 1920, 1366, 960, 640 })
            {
                root.Width = width; root.Height = width >= 1366 ? 930 : 650; sidebarCollapsed = false; await Settle();
                // Start each matrix entry with an actually empty home. The normal
                // navigation captures the previous tool inputs into the home list.
                selected = null; files.Clear(); homeFiles.Clear(); chosenFiles.Clear();
                for (var repeat = 0; repeat < 2; repeat++)
                {
                    ShowHome(); await Settle(); Check(homeFiles.Count == 0 && centeredGrid.IsLoaded, $"empty home fixture {width}/{repeat}");
                    CatalogRows($"home {width}/{repeat}"); Bounds($"home {width}/{repeat}");
                }
                await Capture($"{width}-home");
                homeFiles.Add(source); chosenFiles.Add(source);
                for (var repeat = 0; repeat < 2; repeat++) { ShowHome(); await Settle(350); CatalogRows($"home populated {width}/{repeat}"); Bounds($"home populated {width}/{repeat}"); }
                await Capture($"{width}-home-loaded");
                var tools = screensOnly || stressReader && width == 1920 ? Array.Empty<ToolCard>() : ToolCatalog.All.Where(t => t.Available && t.Id != "edit").ToArray();
                foreach (var tool in tools.Concat(tools.Reverse()))
                {
                    File.WriteAllText(Path.Combine(output, "stage.txt"), $"{width} {tool.Id}");
                    homeFiles.Clear(); chosenFiles.Clear();
                    openingEmptyInput = true; try { ShowTool(tool); } finally { openingEmptyInput = false; }
                    await Settle();
                    if (tool.Id == "organize")
                    {
                        if (pageLayout.Count == 0) await AcceptOrganizerFilesAsync(new[] { await StorageFile.GetFileFromPathAsync(source), await StorageFile.GetFileFromPathAsync(second) });
                    }
                    else
                    {
                        files.Clear(); files.Add(tool.Id == "images" ? image : tool.Id == "word-pdf" ? word : source);
                        if (tool.Id == "merge") files.Add(second);
                    }
                    await Settle(650);
                    if (standardPageScroll is not null) { standardPageScroll.ChangeView(null, 0, null, true); await Settle(); }
                    var label = $"workspace {width} {tool.Id}";
                    Check(toolWorkspace.Children.Count == 2, label + " source and settings attached"); Separate(toolWorkspace, label); Bounds(label);
                    Check(Descendants((FrameworkElement)toolWorkspace.Children[0]).OfType<Button>().Any(Visible), label + " source actions present");
                    Check(Descendants(root).OfType<Button>().Any(b => Visible(b) && AutomationProperties.GetName(b) == "Tutup dokumen"), label + " explicit close available");
                    foreach (var percent in Descendants(root).OfType<ComboBox>().Where(c => Visible(c) && AutomationProperties.GetName(c).EndsWith("dalam persen", StringComparison.Ordinal)))
                    {
                        var textPresenter = Descendants(percent).OfType<TextBox>().FirstOrDefault();
                        Check(!string.IsNullOrWhiteSpace(percent.Text) && textPresenter is not null && textPresenter.Text == percent.Text,
                            label + " native zoom text visible and matches value");
                    }
                    if (tool.Id is "compress" or "rotate" or "numbers" or "watermark" or "crop" or "unlock" or "protect")
                        Check(!Descendants(toolWorkspace).OfType<ComboBox>().Any(c => AutomationProperties.GetName(c) == "Dokumen sesi · pilih untuk membaca"), label + " embedded preview has no empty session selector");
                    if (tool.Id == "split")
                    {
                        var optionScroll = (ScrollViewer)toolOptions.Children[0];
                        Check(toolWorkspace.ActualWidth >= 780 || double.IsPositiveInfinity(optionScroll.MaxHeight), label + " stacked range options use outer page scrolling");
                        Check(splitTabs.SelectMany(t => ((StackPanel)t.Content).Children.OfType<TextBlock>()).Where(t => t.Text != "Belum tersedia").All(t => t.ReadLocalValue(TextBlock.ForegroundProperty) == DependencyProperty.UnsetValue), label + " selected tab label inherits contrasting foreground");
                    }
                    await Capture($"{width}-{tool.Id}");
                    if (standardPageScroll is not null && standardPageScroll.ScrollableHeight > 0)
                    {
                        standardPageScroll.ChangeView(null, standardPageScroll.ScrollableHeight, null, true); await Settle(); Bounds(label + " scrolled bottom");
                        // Merge close is intentionally in the document header; reach it at the top.
                        if (tool.Id == "merge") { standardPageScroll.ChangeView(null, 0, null, true); await Settle(); }
                        var close = Descendants(root).OfType<Button>().First(b => Visible(b) && AutomationProperties.GetName(b) == "Tutup dokumen");
                        var closeBox = Box(close, root);
                        Check(tool.Id == "organize" || closeBox.Bottom <= root.ActualHeight + 1 && closeBox.Top >= 0, label + " close reachable by scrolling");
                        await Capture($"{width}-{tool.Id}-bottom");
                    }
                }
                // Navigate away and back at exactly the same size, then compact sidebar.
                ShowCatalog(); await Settle(); ShowTool(ToolCatalog.All.First(t => t.Id == "merge")); await Settle(); Separate(toolWorkspace, $"return from catalog {width}");
                sidebarCollapsed = true; LayoutCards(); await Settle(); Separate(toolWorkspace, $"sidebar collapsed {width}");
                sidebarCollapsed = false; LayoutCards(); await Settle(); Separate(toolWorkspace, $"sidebar restored {width}");
                if (smoke) break;
                if (width is 1920 or 640)
                {
                    if (screensOnly && width == 640) { homeFiles.Clear(); homeFiles.Add(source); homeFiles.Add(second); chosenFiles.Clear(); chosenFiles.Add(source); chosenFiles.Add(second); }
                    foreach (var tool in ToolCatalog.All)
                    {
                        ShowEmptyTool(tool); await Settle(); Bounds($"empty {width} {tool.Id}"); await Capture($"{width}-empty-{tool.Id}");
                    }
                    var screens = new (string Name, Action Open)[] { ("catalog", ShowCatalog), ("favorites", ShowFavorites), ("secured", ShowSecured), ("assistant", ShowAssistant), ("admin", ShowAdmin), ("help", ShowHelp), ("settings", ShowSettings), ("reader-landing", () => ShowPdfReader(false)), ("history", ShowHistory) };
                    foreach (var screen in screens) { File.WriteAllText(Path.Combine(output, "stage.txt"), $"open {width} {screen.Name}"); screen.Open(); await Settle(); Bounds($"screen {width} {screen.Name}"); File.WriteAllText(Path.Combine(output, "stage.txt"), $"capture {width} {screen.Name}"); await Capture($"{width}-{screen.Name}"); }
                    for (var repeat = 0; repeat < (screensOnly ? 20 : 3); repeat++)
                    {
                        File.WriteAllText(Path.Combine(output, "stage.txt"), $"reader/history repeat {width}/{repeat}");
                        ShowPdfReader(false); await Settle(); Check(pdfReader!.View.IsLoaded && pdfReader.View.ActualWidth > 0, $"reader landing repeated navigation {width}/{repeat}");
                        Check(!Descendants(pdfReader.View).OfType<ComboBox>().Any(c => AutomationProperties.GetName(c) == "Dokumen sesi · pilih untuk membaca" && c.Visibility == Visibility.Visible), $"reader landing keeps document toolbar hidden {width}/{repeat}");
                        ShowHistory(); await Settle();
                    }
                    ShowUnifiedResult([source], "Hasil sintetis tersimpan", ShowHome); await Settle();
                    Check(historyPage.Visibility == Visibility.Collapsed && standardPageScroll?.Visibility == Visibility.Visible && latestResultCard is not null && Visible(latestResultCard), $"result {width} visible after history navigation");
                    Bounds($"result {width}"); await Capture($"{width}-result");
                }
            }
            Check(SHA256.HashData(File.ReadAllBytes(source)).SequenceEqual(hash), "synthetic source unchanged");
            report.Add("NOT_TESTED physical DPI/monitor/touch/screen-reader UAT. Fixed root dimensions are simulated DIP, not physical scale.");
            if (interactive)
            {
                root.Width = root.Height = double.NaN; AppWindow.Resize(new Windows.Graphics.SizeInt32(1480, 960));
                openingEmptyInput = true; try { ShowTool(ToolCatalog.All.First(t => t.Id == "merge")); } finally { openingEmptyInput = false; }
                files.Clear(); files.Add(source); files.Add(second); Title = "PERTAMINA OTO — UI REGRESSION SYNTHETIC AUDIT";
            }
        }
        catch (Exception error) { failures++; report.Add("FAIL EXCEPTION " + error); }
        finally { File.WriteAllLines(Path.Combine(output, "audit.txt"), report); Environment.ExitCode = failures == 0 ? 0 : 1; }
    }
}
#endif
