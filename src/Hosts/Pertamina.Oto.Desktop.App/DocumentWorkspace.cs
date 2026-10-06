using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Input;
using Pertamina.Oto.LocalPdf;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private readonly HashSet<string> chosenFiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stack<string[]> homeUndo = new();
    private readonly Stack<string[]> homeRedo = new();
    private readonly List<OrganizerPage> pageLayout = [];
    private readonly List<string> organizerSources = [];
    private readonly Stack<OrganizerPage[]> layoutUndo = new();
    private readonly Stack<OrganizerPage[]> layoutRedo = new();
    private readonly HashSet<int> chosenPages = [];
    private static readonly SemaphoreSlim RenderGate = new(1, 1);
    private static readonly Dictionary<string, (DateTimeOffset Modified, ulong Size, PdfDocument Document)> PreviewDocuments = new(StringComparer.OrdinalIgnoreCase);
    private readonly ScrollViewer pageGrid = new() { MaxHeight = 650, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    private readonly StackPanel sourceSummary = new() { Spacing = 10 };
    private readonly TextBlock layoutStatus = new() { TextWrapping = TextWrapping.Wrap };
    private bool organizerVisible;
    private bool importing;
    private string? focusedFile;

    private void AddWorkspaceShortcuts()
    {
        root.PreviewKeyDown += GlobalPasteKeyDown;
        foreach (var key in new[] { VirtualKey.Z, VirtualKey.Y })
        {
            var shortcut = new KeyboardAccelerator { Key = key, Modifiers = VirtualKeyModifiers.Control };
            shortcut.Invoked += async (_, e) =>
            {
                if (active is not null || importing || FocusManager.GetFocusedElement(root.XamlRoot) is TextBox or PasswordBox) return;
                if (key == VirtualKey.V)
                {
                    e.Handled = true; await PasteDocumentAnywhereAsync(); return;
                }
                if (!organizerVisible && homeEntries.Parent is null) return;
                e.Handled = true;
                if (key == VirtualKey.V) { if (organizerVisible) await PasteOrganizerAsync(); else await PasteHomeAsync(); }
                else if (organizerVisible) RestoreLayout(key == VirtualKey.Y);
                else RestoreHome(key == VirtualKey.Y);
            };
            root.KeyboardAcceleratorPlacementMode = KeyboardAcceleratorPlacementMode.Hidden;
            root.KeyboardAccelerators.Add(shortcut);
        }
    }

    private static Viewbox BuildBrand() => (Viewbox)Microsoft.UI.Xaml.Markup.XamlReader.Load("""
        <Viewbox xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" MaxWidth="192" Margin="4,12,4,24">
          <StackPanel Width="192" HorizontalAlignment="Center">
            <Canvas Width="112" Height="78" HorizontalAlignment="Center">
              <Path Data="M8,4 L48,13 L56,23 L64,13 L104,4 L94,25 L64,25 L56,32 L48,25 L18,25 Z" Fill="#ED2939"/>
              <Ellipse Canvas.Left="16" Canvas.Top="23" Width="32" Height="32" Stroke="White" StrokeThickness="8"/>
              <Ellipse Canvas.Left="64" Canvas.Top="23" Width="32" Height="32" Stroke="White" StrokeThickness="8"/>
              <Path Data="M48,26 L64,26 L64,34 L60,34 L60,57 L52,57 L52,34 L48,34 Z" Fill="White"/>
              <Path Data="M47,55 L56,49 L65,55 L56,68 Z" Fill="#00A6E8"/>
              <Path Data="M14,51 L48,66 L56,71 L64,66 L98,51 L87,67 L56,78 L25,67 Z" Fill="#64B32E"/>
            </Canvas>
            <TextBlock Text="PERTAMINA OTO" FontSize="21" FontWeight="Bold" Foreground="White" HorizontalAlignment="Center" Margin="0,10,0,2"/>
            <TextBlock Text="Office Tools Organizer" FontSize="11" Foreground="#BFCEE0" HorizontalAlignment="Center"/>
          </StackPanel>
        </Viewbox>
        """);

    private void RememberHome()
    {
        if (homeUndo.Count >= 30) homeUndo.Clear();
        homeUndo.Push(homeFiles.ToArray()); homeRedo.Clear();
    }

    private void RestoreHome(bool redo)
    {
        var from = redo ? homeRedo : homeUndo; var to = redo ? homeUndo : homeRedo;
        if (from.Count == 0) return;
        to.Push(homeFiles.ToArray()); homeFiles.Clear(); homeFiles.AddRange(from.Pop());
        chosenFiles.Clear(); chosenFiles.UnionWith(homeFiles); BuildDesignedHome();
    }

    private async Task ChooseHomeAsync()
    {
        if (active is not null || importing) return;
        try
        {
            var picker = new FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            foreach (var extension in HomeExtensions) picker.FileTypeFilter.Add(extension);
            await ReceiveHomeFilesAsync(await picker.PickMultipleFilesAsync());
        }
        catch (Exception) { HomeNotice("File tidak dapat ditambahkan."); }
    }

    private void BuildDocumentHome()
    {
        BeginScreen("Dokumen Anda siap", "Lihat preview, pilih dokumen, lalu tentukan pekerjaan. File asli tidak diubah.");
        var toolbar = new ResponsiveWrapPanel { Spacing = 8 };
        toolbar.Children.Add(HomeButton("Tambah file", "\uE710", async () => await ChooseHomeAsync(), true));
        toolbar.Children.Add(HomeButton("Tempel lagi", "\uE77F", async () => await PasteHomeAsync()));
        var undo = HomeButton("Undo", "\uE7A7", () => RestoreHome(false)); undo.IsEnabled = homeUndo.Count > 0;
        var redo = HomeButton("Redo", "\uE7A6", () => RestoreHome(true)); redo.IsEnabled = homeRedo.Count > 0;
        toolbar.Children.Add(undo); toolbar.Children.Add(redo); body.Children.Add(toolbar);
        homeFileStatus = Copy($"{homeFiles.Count} file · {chosenFiles.Count} dipilih · Preview lokal, tanpa upload");
        body.Children.Add(homeFileStatus);
        homeEntries.Children.Clear();
        var documentList = new StackPanel { Spacing = 12 };
        foreach (var path in homeFiles)
        {
            var row = new StackPanel { Spacing = 8 };
            var check = new CheckBox { Content = Path.GetFileName(path), IsChecked = chosenFiles.Contains(path) };
            check.Checked += (_, _) => { chosenFiles.Add(path); HomeNotice($"{chosenFiles.Count} file dipilih. Rekomendasi memakai file yang dicentang."); };
            check.Unchecked += (_, _) => { chosenFiles.Remove(path); HomeNotice($"{chosenFiles.Count} file dipilih. Rekomendasi memakai file yang dicentang."); };
            row.Children.Add(check);
            row.Children.Add(HomeButton("Lihat preview", "\uE890", () => { focusedFile = path; BuildDocumentHome(); }));
            var actions = new ResponsiveWrapPanel { Spacing = 6 };
            foreach (var delta in new[] { -1, 1 })
            {
                var move = new Button { Content = delta < 0 ? "↑" : "↓" };
                ToolTipService.SetToolTip(move, delta < 0 ? "Pindahkan file ke atas" : "Pindahkan file ke bawah");
                move.Click += (_, _) => { var index = homeFiles.IndexOf(path); var next = index + delta; if (next < 0 || next >= homeFiles.Count) return; RememberHome(); homeFiles.RemoveAt(index); homeFiles.Insert(next, path); BuildDocumentHome(); };
                actions.Children.Add(move);
            }
            var remove = new Button { Content = "Keluarkan" };
            remove.Click += (_, _) => { RememberHome(); homeFiles.Remove(path); chosenFiles.Remove(path); BuildDesignedHome(); };
            actions.Children.Add(remove); row.Children.Add(actions);
            documentList.Children.Add(Section("File " + (homeFiles.IndexOf(path) + 1), path == focusedFile ? "Sedang dipreview" : "File sumber", row));
        }
        if (homeFiles.Count > 1) homeEntries.Children.Add(new ScrollViewer { Content = documentList, MaxHeight = 640 });
        if (focusedFile is null || !homeFiles.Contains(focusedFile)) focusedFile = homeFiles[0];
        homeEntries.Children.Add(PreviewPanel(focusedFile));
        var recommendations = new StackPanel { Spacing = 12 };
        recommendations.Children.Add(Copy("Rekomendasi memakai file yang dicentang. Membuka preview tidak mengubah pilihan.", 12));
        foreach (var id in new[] { "organize", "merge", "compress", "split", "rotate", "images" })
        {
            var tool = ToolCatalog.All.First(t => t.Id == id);
            recommendations.Children.Add(HomeButton(DisplayTool(tool), tool.Glyph, () => OpenHomeTool(tool), id == "organize"));
        }
        var search = new TextBox { PlaceholderText = "Cari: satukan, kecilkan, putar…" };
        recommendations.Children.Add(search); var results = new StackPanel { Spacing = 6 }; recommendations.Children.Add(results);
        search.TextChanged += (_, _) => { results.Children.Clear(); foreach (var tool in FindTools(search.Text).Take(5)) results.Children.Add(HomeButton(DisplayTool(tool), tool.Glyph, () => OpenHomeTool(tool))); if (results.Children.Count == 0) results.Children.Add(Copy("Tidak ditemukan. Coba kata lain.")); };
        recommendations.Children.Add(Copy("Assistant · Model belum terhubung. Tidak ada dokumen dikirim ke AI.", 12));
        recommendations.Children.Add(HomeButton("Kosongkan daftar", "\uE711", () => { RememberHome(); homeFiles.Clear(); chosenFiles.Clear(); BuildDesignedHome(); }));
        homeEntries.Children.Add(Section("Mau diapakan?", "Cari alat tanpa memasukkan ulang dokumen", recommendations));
        body.Children.Add(homeEntries); LayoutCards();
    }

    private Border PreviewPanel(string path, uint firstPage = 0, int angle = 0, string previewPassword = "")
    {
        if (!Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase) || angle != 0)
            return SinglePagePreviewPanel(path, firstPage, angle, previewPassword);
        var reader = new PdfReaderView(this, previewOnly: true);
        reader.View.Height = 500;
        reader.View.Loaded += async (_, _) =>
        {
            await reader.OpenAsync(path, previewPassword);
            if (reader.Visible && firstPage > 0) await reader.NavigateAsync((int)firstPage);
        };
        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(reader.View);
        content.Children.Add(HomeButton("Perluas preview", "\uE740", async () => await ShowReadPreviewDialogAsync(path, reader.ReadingDocument, reader.ReadingPage)));
        return Section(Path.GetFileName(path), "Mode baca · Gulir antarhalaman · Cubit untuk zoom", content);
    }

    private static Border SinglePagePreviewPanel(string path, uint firstPage = 0, int angle = 0, string previewPassword = "")
    {
        var panel = new StackPanel { Spacing = 10 };
        var picture = new Image { Height = 400, Stretch = Stretch.Uniform, RenderTransformOrigin = new Windows.Foundation.Point(.5, .5), RenderTransform = new RotateTransform { Angle = angle } };
        var imageBounds = new Grid { Width = 400, Height = 400, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        picture.HorizontalAlignment = HorizontalAlignment.Center; picture.VerticalAlignment = VerticalAlignment.Center; imageBounds.Children.Add(picture);
        var viewport = new ScrollViewer { Content = imageBounds, Height = 450, ZoomMode = ZoomMode.Enabled, MinZoomFactor = .1f, MaxZoomFactor = 5,
            HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var info = Copy("Memuat preview lokal…", 12); panel.Children.Add(viewport); panel.Children.Add(info);
        uint index = firstPage; uint total = 1;
        var controls = new ResponsiveWrapPanel { Spacing = 8 };
        var previous = new Button { Content = "‹" }; var next = new Button { Content = "›" };
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(previous, "Halaman sebelumnya"); Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(next, "Halaman berikutnya");
        ZoomController? zoom = null;
        ToolTipService.SetToolTip(previous, "Halaman sebelumnya"); ToolTipService.SetToolTip(next, "Halaman berikutnya");
        async Task Load()
        {
            previous.IsEnabled = false; next.IsEnabled = false;
            total = await RenderPreviewAsync(path, index, picture, info, 850, previewPassword);
            if (picture.Source is BitmapImage bitmap && bitmap.PixelHeight > 0)
            {
                picture.Width = 400d * bitmap.PixelWidth / bitmap.PixelHeight;
                var radians = angle * Math.PI / 180;
                imageBounds.Width = Math.Abs(picture.Width * Math.Cos(radians)) + Math.Abs(picture.Height * Math.Sin(radians));
                imageBounds.Height = Math.Abs(picture.Width * Math.Sin(radians)) + Math.Abs(picture.Height * Math.Cos(radians));
            }
            zoom?.Sync();
            previous.IsEnabled = index > 0; next.IsEnabled = index + 1 < total;
        }
        previous.Click += async (_, _) => { if (index > 0) { index--; await Load(); } };
        next.Click += async (_, _) => { if (index + 1 < total) { index++; await Load(); } };
        controls.Children.Add(previous); controls.Children.Add(next);
        zoom = new ZoomController(viewport, "Zoom", .1, 5, () => viewport.ZoomFactor,
                scale => ApplyViewportZoom(viewport, scale),
            width => { imageBounds.UpdateLayout(); var factor = width ? viewport.ActualWidth / Math.Max(1, imageBounds.Width) : Math.Min(viewport.ActualWidth / Math.Max(1, imageBounds.Width), viewport.ActualHeight / Math.Max(1, imageBounds.Height)); viewport.ChangeView(0, 0, (float)Math.Clamp(factor, .1, 5), true); }, () => picture.Source is not null);
        controls.Children.Add(zoom.Toolbar); panel.Children.Add(controls);
        picture.Loaded += async (_, _) => await Load();
        return Section(Path.GetFileName(path), "Preview dokumen", panel);
    }

    private static async Task<uint> RenderPreviewAsync(string path, uint index, Image picture, TextBlock info, uint width, string previewPassword = "", Func<bool>? stillCurrent = null)
    {
        var raster = previewRasters.GetValue(picture, _ => new());
        raster.Path = path; raster.Page = index; raster.Password = previewPassword;
        var ticket = ++raster.Generation;
        await RenderGate.WaitAsync();
        try
        {
            if (!picture.IsLoaded || ticket != raster.Generation || !(stillCurrent?.Invoke() ?? true)) return 0;
            var file = await StorageFile.GetFileFromPathAsync(path);
            var properties = await file.GetBasicPropertiesAsync();
            if (properties.Size > 100 * 1024 * 1024) throw new InvalidOperationException();
            using var stream = new InMemoryRandomAccessStream();
            uint total;
            if (file.FileType.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                PdfDocument document;
                if (previewPassword.Length > 0)
                    document = await PdfDocument.LoadFromFileAsync(file, previewPassword);
                else
                {
                    if (!PreviewDocuments.TryGetValue(path, out var cached) || cached.Modified != properties.DateModified || cached.Size != properties.Size)
                    {
                        var loaded = await PdfDocument.LoadFromFileAsync(file);
                        if (PreviewDocuments.Count >= 2) PreviewDocuments.Clear();
                        cached = (properties.DateModified, properties.Size, loaded); PreviewDocuments[path] = cached;
                    }
                    document = cached.Document;
                }
                total = document.PageCount;
                if (total is 0 or > 1000 || index >= total) throw new InvalidOperationException();
                using var page = document.GetPage(index);
                await page.RenderToStreamAsync(stream, new PdfPageRenderOptions { DestinationWidth = width });
                stream.Seek(0);
                var bitmap = new BitmapImage(); await bitmap.SetSourceAsync(stream);
                if (ticket != raster.Generation || !picture.IsLoaded || !(stillCurrent?.Invoke() ?? true)) return 0;
                picture.Source = bitmap;
            }
            else
            {
                using var input = await file.OpenReadAsync();
                var bitmap = new BitmapImage { DecodePixelWidth = (int)width }; await bitmap.SetSourceAsync(input);
                if (ticket != raster.Generation || !picture.IsLoaded || !(stillCurrent?.Invoke() ?? true)) return 0;
                picture.Source = bitmap; total = 1;
            }
            raster.Width = width;
            info.Text = $"Halaman {index + 1} dari {total}"; return total;
        }
        catch (Exception) { info.Text = "Preview tidak tersedia: file terkunci, rusak, atau di luar batas. Sumber tidak diubah."; return 0; }
        finally { RenderGate.Release(); }
    }

    private void ShowOrganizer()
    {
        if (active is not null || importing) return;
        SaveToolDraft();
        if (!openingEmptyInput && pageLayout.Count == 0 && !homeFiles.Any(chosenFiles.Contains))
        { ShowEmptyTool(ToolCatalog.All.First(t => t.Id == "organize")); return; }
        DetachToolControls();
        BeginScreen("Atur halaman PDF", "Susun, putar, duplikat, atau keluarkan halaman. Simpan sebagai PDF baru; sumber tetap utuh.");
        organizerVisible = true; selected = ToolCatalog.All.First(t => t.Id == "organize");
        closedToolSessions.Remove("organize");
        activeToolControls = "organize";
        foreach (var control in new FrameworkElement[] { run, cancel, openResult, outputName, location, status, progress, password })
            if (control.Parent is Panel parent) parent.Children.Remove(control);
        var toolbar = new ResponsiveWrapPanel { Spacing = 8 };
        toolbar.Children.Add(HomeButton("＋ Tambah PDF", "\uE710", async () => await PickOrganizerAsync(), true));
        toolbar.Children.Add(HomeButton("Tempel PDF", "\uE77F", async () => await PasteOrganizerAsync()));
        toolbar.Children.Add(DocumentCloseButton());
        toolbar.Children.Add(HomeButton("Undo", "\uE7A7", () => RestoreLayout(false)));
        toolbar.Children.Add(HomeButton("Redo", "\uE7A6", () => RestoreLayout(true)));
        body.Children.Add(toolbar);
        body.Children.Add(layoutStatus);
        if (organizerZoomHost is not null) DetachOwned(toolWorkspace, organizerZoomHost);
        if (sourceSummary.Parent is Panel oldSummary) oldSummary.Children.Remove(sourceSummary);
        // New wrappers avoid sharing a XAML element between two parents.
        toolWorkspace.Children.Clear(); toolOptions.Children.Clear();
        ConfigureOrganizerDrop();
        toolWorkspace.Children.Add(OrganizerZoomHost());
        toolOptions.Children.Add(sourceSummary);
        outputName.Text = organizerOutput; toolOptions.Children.Add(outputName); toolOptions.Children.Add(password);
        toolOptions.Children.Add(HomeButton("PILIH FOLDER HASIL", "\uE8B7", async () => await PickFolderAsync()));
        toolOptions.Children.Add(BuildOutputLocation()); run.Content = "Simpan PDF baru";
        openResult.Visibility = Visibility.Collapsed; progress.Visibility = Visibility.Collapsed;
        toolOptions.Children.Add(run); toolOptions.Children.Add(status);
        if (toolOptions.Parent is Panel oldOptions) oldOptions.Children.Remove(toolOptions);
        toolWorkspace.Children.Add(toolOptions); body.Children.Add(toolWorkspace);
        editable.Add(outputName); editable.Add(password); editable.Add(pageGrid);
        foreach (var button in toolbar.Children.OfType<Button>()) editable.Add(button);
        RefreshPageGrid(); LayoutCards();
        _ = AddOrganizerSourcesAsync();
    }

    private async Task AddOrganizerSourcesAsync()
    {
        if (importing || active is not null) return;
        var incoming = new List<IStorageItem>();
        foreach (var path in homeFiles.Where(chosenFiles.Contains).ToArray())
        {
            if (organizerSources.Contains(path, StringComparer.OrdinalIgnoreCase)) continue;
            try { incoming.Add(await StorageFile.GetFileFromPathAsync(path)); }
            catch (Exception) { status.Text = "Sebagian sumber tidak tersedia."; }
        }
        if (incoming.Count > 0) await AcceptOrganizerFilesAsync(incoming);
    }
    private void SaveLayout() { if (layoutUndo.Count >= 30) layoutUndo.Clear(); layoutUndo.Push(pageLayout.ToArray()); layoutRedo.Clear(); }
    private void RestoreLayout(bool redo)
    {
        if (importing || active is not null) return;
        CancelOrganizerDrag();
        var from = redo ? layoutRedo : layoutUndo; var to = redo ? layoutUndo : layoutRedo;
        if (from.Count == 0) return;
        to.Push(pageLayout.ToArray()); pageLayout.Clear(); pageLayout.AddRange(from.Pop()); RefreshPageGrid();
    }

    private void ChangeLayout(string operation)
    {
        if (importing || active is not null || chosenPages.Count == 0) return;
        if (operation == "duplicate" && pageLayout.Count + chosenPages.Count > 1000) { status.Text = "Maksimal 1000 halaman hasil."; return; }
        SaveLayout();
        if (operation is "left" or "right")
        {
            if (chosenPages.Count != 1) { status.Text = "Pilih satu halaman untuk pindah dengan panah."; return; }
            var index = chosenPages.First(); var target = index + (operation == "left" ? -1 : 1);
            if (target >= 0 && target < pageLayout.Count) (pageLayout[index], pageLayout[target]) = (pageLayout[target], pageLayout[index]);
        }
        else foreach (var index in chosenPages.OrderDescending())
        {
            if (operation == "rotate") pageLayout[index] = pageLayout[index] with { Rotation = (pageLayout[index].Rotation + 90) % 360 };
            if (operation == "duplicate") pageLayout.Insert(index + 1, pageLayout[index].Duplicate());
            if (operation == "remove") pageLayout.RemoveAt(index);
        }
        if (operation == "rotate")
        {
            foreach (var index in chosenPages)
                if (organizerCards.TryGetValue(pageLayout[index].Id, out var card)) { card.Page = pageLayout[index]; card.Angle.Angle = card.Page.Rotation; }
        }
        else RefreshPageGrid();
    }

    private void RefreshPageGrid() => RenderOrganizerCards();
}
