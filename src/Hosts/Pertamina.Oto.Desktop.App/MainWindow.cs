using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Pertamina.Oto.LocalPdf;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow : Window
{
    private readonly Grid root = new();
    private readonly StackPanel body = new() { Spacing = 20, Padding = new Thickness(32) };
    private readonly Grid historyPage = new() { Visibility = Visibility.Collapsed, Background = Brush(255, 255, 255) };
    private ScrollViewer? standardPageScroll;
    private Grid? standardPageHeader;
    private readonly Grid cards = new() { ColumnSpacing = 14, RowSpacing = 14 };
    private readonly ObservableCollection<string> files = [];
    private readonly ListBox fileList = new() { MaxHeight = 180, MinHeight = 80, SelectionMode = SelectionMode.Single };
    private readonly TextBox pages = new() { Header = "Halaman (kosong = semua)", PlaceholderText = "Contoh: 1,3-5,2" };
    private readonly TextBox watermark = new() { Header = "Teks watermark", MaxLength = 200, PlaceholderText = "DRAFT" };
    private readonly PasswordBox password = new() { Header = "Kata sandi pemilik PDF (jika diperlukan)", MaxLength = 127 };
    private readonly PasswordBox newPassword = new() { Header = "Kata sandi pembuka baru (minimal 8 karakter)", MaxLength = 127 };
    private readonly ComboBox rotation = new() { Header = "Rotasi searah jarum jam", ItemsSource = new[] { 90, 180, 270 }, SelectedIndex = 0 };
    private readonly NumberBox margin = new() { Header = "Margin potong setiap sisi (pt)", Value = 20, Minimum = 1, Maximum = 500 };
    private readonly TextBox outputName = new() { Header = "Nama file hasil", Text = "hasil.pdf", MaxLength = 120 };
    private readonly TextBlock location = new() { Text = "Folder hasil belum dipilih", TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock status = new() { TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true };
    private readonly ProgressBar progress = new() { IsIndeterminate = true, Visibility = Visibility.Collapsed };
    private readonly Button run = new() { Content = "Proses dan simpan hasil", Padding = new Thickness(24, 12, 24, 12) };
    private readonly Button cancel = new() { Content = "Batalkan", IsEnabled = false };
    private readonly Button openResult = new() { Content = "Buka hasil", Visibility = Visibility.Collapsed };
    private readonly List<string> history = [];
    private readonly List<Button> navigation = [];
    private readonly List<FrameworkElement> editable = [];
    private ToolCard? selected;
    private CancellationTokenSource? active;
    private string? folder;
    private string? resultPath;
    private int columns;
    private string? responsiveSignature;
    private UIElement[] responsiveChildren = [];
    private readonly Grid toolWorkspace = new() { ColumnSpacing = 20, RowSpacing = 20 };
    private readonly StackPanel toolOptions = new() { Spacing = 16 };
    private readonly TextBlock sidebarStatus = new() { Text = "LOCAL • OFFLINE\nVersi pengembangan", TextWrapping = TextWrapping.Wrap,
        Foreground = Brush(169, 202, 231), Margin = new Thickness(8, 28, 0, 0), FontSize = 12 };

    internal MainWindow()
    {
        Title = "PERTAMINA OTO — Desktop Offline · Development";
        try { AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "oto.ico")); }
        catch (Exception) { /* Missing icon must not prevent startup. */ }
        LoadFolderPreference();
        LoadLibrary();
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1480, 960));
        root.Background = Brush(246, 248, 251);
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(232) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var sidebar = new StackPanel { Spacing = 8, Padding = new Thickness(12), Background = Brush(7, 29, 61) };
        sidebar.Children.Add(BuildBrand());
        sidebar.Children.Add(Nav("\uE80F", "Beranda", ShowHome));
        sidebar.Children.Add(Nav("\uE8A5", "Pembaca PDF", ShowPdfReader));
        var tools = Nav("\uE80A", "Semua Alat", ShowCatalog);
        var recent = Nav("\uE81C", "Riwayat", ShowHistory);
        var help = Nav("\uE897", "Bantuan", ShowHelp);
        sidebar.Children.Add(tools);
        sidebar.Children.Add(Nav("\uE734", "Favorit", ShowFavorites));
        sidebar.Children.Add(recent);
        sidebar.Children.Add(Nav("\uE8B7", "File Diamankan", ShowSecured));
        sidebar.Children.Add(Nav("\uE99A", "Assistant", ShowAssistant));
        sidebar.Children.Add(Nav("\uE7EF", "Administrasi", ShowAdmin));
        sidebar.Children.Add(new Border { Height = 1, Background = Brush(53, 73, 100), Margin = new Thickness(12, 20, 12, 12) });
        sidebar.Children.Add(help);
        sidebar.Children.Add(Nav("\uE713", "Pengaturan", ShowSettings));
        sidebar.Children.Add(sidebarStatus);
        root.Children.Add(new ScrollViewer { Content = sidebar, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled });
        var scroll = new ScrollViewer { Content = body, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        standardPageScroll = scroll;
        var workspace = new Grid();
        workspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        workspace.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        standardPageHeader = BuildHeader();
        workspace.Children.Add(standardPageHeader); Grid.SetRow(scroll, 1); workspace.Children.Add(scroll);
        Grid.SetRow(historyPage, 1); workspace.Children.Add(historyPage);
        Grid.SetColumn(workspace, 1); root.Children.Add(workspace);
        var desktopShell = new Grid();
        desktopShell.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        desktopShell.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        desktopShell.Children.Add(BuildDesktopMenu());
        Grid.SetRow(root, 1); desktopShell.Children.Add(root);
        InitializeOnlineUpdates(desktopShell);
        Content = desktopShell;
        AddWorkspaceShortcuts();
        fileList.ItemsSource = files;
        files.CollectionChanged += (_, _) => UpdateAutomaticOutputName();
        files.CollectionChanged += (_, _) => { if (selected?.Id == "word-pdf") RefreshWordSource(); };
        files.CollectionChanged += (_, _) => { if (mergeTiles?.IsLoaded == true && selected?.Id == "merge") RefreshMergeTiles(); };
        files.CollectionChanged += (_, _) => { if (sourcePreviewHost?.IsLoaded == true) RefreshSourcePreview(); };
        run.Background = Brush(22, 93, 222); run.Foreground = Brush(255, 255, 255);
        run.Click += async (_, _) => await ExecuteAsync();
        cancel.Click += (_, _) => active?.Cancel();
        openResult.Click += async (_, _) =>
        {
            if (resultPath is null) return;
            try { await Launcher.LaunchFileAsync(await StorageFile.GetFileFromPathAsync(resultPath)); }
            catch (Exception) { status.Text = "Tidak dapat membuka viewer. File hasil tetap tersimpan di folder pilihan Anda."; }
        };
        Closed += (_, _) => { active?.Cancel(); liveEditor?.ReleaseFiles(); pdfReader?.Dispose(); };
        AppWindow.Closing += (_, e) =>
        {
            if (allowClose) return;
            e.Cancel = true;
            // Leave the native closing callback before opening a XAML dialog.
            if (closeCheckQueued) return;
            closeCheckQueued = true;
            if (!DispatcherQueue.TryEnqueue(async () =>
            {
                try { await ConfirmCloseAsync(); }
                catch (Exception error) { LocalDiagnostics.Record("close-window", error); }
                finally { closeCheckQueued = false; }
            })) closeCheckQueued = false;
        };
        root.SizeChanged += (_, _) => LayoutCards();
        ShowHome();
    }

    private Button Nav(string glyph, string label, Action click)
    {
        var button = new Button { Content = IconLabel(glyph, label), HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Background = Brush(7, 29, 61), Foreground = Brush(255, 255, 255), Padding = new Thickness(14), CornerRadius = new CornerRadius(10), BorderThickness = new Thickness(0), MinHeight = 48 };
        AutomationProperties.SetName(button, label);
        ToolTipService.SetToolTip(button, label);
        button.Click += async (_, _) => { if (active is not null || importing || messageOpen) return; try { click(); foreach (var item in navigation) item.Background = Brush(7, 29, 61); button.Background = Brush(23, 104, 212); } catch (Exception error) { LocalDiagnostics.Record("sidebar-navigation", error); await MenuMessageAsync("Halaman belum dapat dibuka", "Coba kembali. Dokumen di komputer tidak berubah."); } }; navigation.Add(button); return button;
    }

    private static StackPanel IconLabel(string glyph, string label)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        panel.Children.Add(new FontIcon { Glyph = glyph, FontFamily = new FontFamily("Segoe Fluent Icons"), FontSize = 20, Width = 24 });
        panel.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center });
        return panel;
    }

    private void Heading(string title, string description)
    {
        SetHistoryPageVisible(false);
        TrackScreen(title);
        breadcrumb.Text = "Ruang kerja  /  " + title;
        body.Children.Add(new TextBlock { Text = title, FontSize = 30, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
        body.Children.Add(new TextBlock { Text = description, FontSize = 14, Foreground = Brush(76, 91, 112), TextWrapping = TextWrapping.Wrap });
    }

    private void ShowCatalog()
    {
        if (active is not null || importing || messageOpen) return;
        CaptureWorkspaceContext(); SaveToolDraft();
        organizerVisible = false;
        body.Children.Clear(); selected = null;
        Heading("Semua alat", $"{ToolCatalog.All.Length} alat dalam {ToolCatalog.All.Select(t => t.Group).Distinct().Count()} kategori");
        body.Children.Add(Copy($"● {ToolCatalog.All.Count(t => t.Available)} aktif · fungsi dasar     ◷ {ToolCatalog.All.Count(t => !t.Available)} belum aktif", 13));
        body.Children.Add(Copy($"{homeFiles.Count(chosenFiles.Contains)} dokumen dipilih · Alat yang Anda pilih akan memakai dokumen ini.", 13));
        body.Children.Add(ActionButton("Lihat / pilih dokumen", ShowHome));
        var search = new TextBox { PlaceholderText = "Cari alat: gabung, kecilkan, putar…", MinHeight = 44, CornerRadius = new CornerRadius(10) };
        AutomationProperties.SetName(search, "Cari alat PDF");
        search.TextChanged += (_, _) => FillCards(search.Text);
        body.Children.Add(search); body.Children.Add(cards); FillCards("");
        body.Children.Add(Copy("Versi pengembangan · Gunakan salinan dokumen uji · File tetap di perangkat", 12));
    }

    private void FillCards(string search)
    {
        cards.Children.Clear();
        foreach (var group in FindTools(search).GroupBy(tool => tool.Group))
        {
            cards.Children.Add(BuildCategory(group.Key, group.ToArray()));
        }
        if (cards.Children.Count == 0) cards.Children.Add(Copy("Tidak ditemukan. Coba kata lain, misalnya gabung atau putar."));
        columns = 0; responsiveSignature = null; LayoutCards();
    }

    private void LayoutCards()
    {
        var compact = sidebarCollapsed || (readerFocus && pdfReader?.Visible == true) || root.ActualWidth < 900;
        var contentWidth = Math.Max(0, root.ActualWidth - (compact ? 104 : 296));
        // Keep the verified 0.1.0 desktop density. Only the smallest windows
        // need a one-column fallback that the old layout did not provide.
        var quickColumns = root.ActualWidth >= 1320 ? 6 : root.ActualWidth >= 760 ? 3 : contentWidth >= 330 ? 2 : 1;
        var entryColumns = root.ActualWidth >= 1120 ? Math.Max(1, Math.Min(3, homeEntries.Children.Count)) : 1;
        var count = root.ActualWidth >= 1700 ? 4 : root.ActualWidth >= 1280 ? 3 : root.ActualWidth >= 900 ? 2 : 1;
        var stackTools = contentWidth < 780;
        var signature = $"{quickColumns}:{entryColumns}:{count}:{stackTools}:{compact}:{quickTools.Children.Count}:{homeEntries.Children.Count}:{cards.Children.Count}:{toolWorkspace.Children.Count}";
        // Routes recreate panels while keeping the same child count. A width/count
        // cache alone leaves new panels at Grid row/column zero, hiding their peers.
        var children = quickTools.Children.Concat(homeEntries.Children).Concat(cards.Children).Concat(toolWorkspace.Children).ToArray();
        var childrenChanged = !responsiveChildren.SequenceEqual(children);
        if (responsiveSignature == signature && !childrenChanged) return;
        responsiveSignature = signature;
        responsiveChildren = children;
        sidebarStatus.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        quickTools.ColumnDefinitions.Clear(); quickTools.RowDefinitions.Clear();
        for (var i = 0; i < quickColumns; i++) quickTools.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        for (var i = 0; i < (6 + quickColumns - 1) / quickColumns; i++) quickTools.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var i = 0; i < quickTools.Children.Count; i++) { Grid.SetColumn((FrameworkElement)quickTools.Children[i], i % quickColumns); Grid.SetRow((FrameworkElement)quickTools.Children[i], i / quickColumns); }
        root.ColumnDefinitions[0].Width = new GridLength(compact ? 72 : 232);
        foreach (var item in navigation)
        {
            // The compact button is 48 DIP wide; 14 DIP padding clipped its 24 DIP icon.
            item.Padding = new Thickness(compact ? 8 : 14);
            item.HorizontalContentAlignment = compact ? HorizontalAlignment.Center : HorizontalAlignment.Left;
            if (item.Content is StackPanel p && p.Children.Count > 1) p.Children[1].Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        }
        body.Padding = new Thickness(compact ? 16 : 32);
        toolWorkspace.ColumnDefinitions.Clear(); toolWorkspace.RowDefinitions.Clear();
        toolWorkspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        toolWorkspace.ColumnDefinitions.Add(new ColumnDefinition { Width = stackTools ? new GridLength(0) : new GridLength(320) });
        toolWorkspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        toolWorkspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var i = 0; i < toolWorkspace.Children.Count; i++)
        {
            var child = (FrameworkElement)toolWorkspace.Children[i];
            Grid.SetColumn(child, stackTools ? 0 : i); Grid.SetRow(child, stackTools ? i : 0);
            Grid.SetColumnSpan(child, 1); Grid.SetRowSpan(child, 1);
        }
        homeEntries.ColumnDefinitions.Clear(); homeEntries.RowDefinitions.Clear();
        for (var i = 0; i < entryColumns; i++) homeEntries.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        for (var i = 0; i < (3 + entryColumns - 1) / entryColumns; i++) homeEntries.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var i = 0; i < homeEntries.Children.Count; i++) { Grid.SetColumn((FrameworkElement)homeEntries.Children[i], i % entryColumns); Grid.SetRow((FrameworkElement)homeEntries.Children[i], i / entryColumns); }
        if (count == columns && !childrenChanged) return;
        columns = count;
        cards.ColumnDefinitions.Clear(); cards.RowDefinitions.Clear();
        for (var i = 0; i < count; i++) cards.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        for (var i = 0; i < (cards.Children.Count + count - 1) / count; i++) cards.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var i = 0; i < cards.Children.Count; i++) { Grid.SetRow((FrameworkElement)cards.Children[i], i / count); Grid.SetColumn((FrameworkElement)cards.Children[i], i % count); }
    }

    private void ShowTool(ToolCard tool)
    {
        if (active is not null || importing) return;
        if (tool.Id == "edit") { ShowLiveEditor(); return; }
        SaveToolDraft();
        if (tool.Id == "organize") { ShowOrganizer(); return; }
        if (!openingEmptyInput && (!tool.Available || (!homeFiles.Any(chosenFiles.Contains) && (!toolDrafts.TryGetValue(tool.Id, out var draft) ||
            (draft.Files.Length == 0 && !RestartToolChanged(tool.Id, draft))))))
        { ShowEmptyTool(tool); return; }
        organizerVisible = false;
        closedToolSessions.Remove(tool.Id);
        activeToolControls = tool.Id;
        run.Content = "Proses dan simpan hasil";
        DetachToolControls();
        run.ClearValue(Control.FontSizeProperty); run.ClearValue(Control.BackgroundProperty); run.ClearValue(Control.ForegroundProperty);
        run.ClearValue(FrameworkElement.HorizontalAlignmentProperty);
        password.Visibility = Visibility.Visible;
        outputName.Visibility = Visibility.Visible; pages.Visibility = Visibility.Visible;
        body.Children.Clear(); editable.Clear(); selected = tool;
        if (run.Parent is Panel previousActions) previousActions.Children.Clear();
        Heading(tool.Id == "merge" ? "Gabung PDF" : tool.Name, tool.Id == "merge" ? "Tambahkan dokumen, atur urutan, lalu gabungkan." : tool.Description);
        if (tool.Id == "split" && body.Children[0] is TextBlock splitHeading) splitHeading.Text = "Pisahkan PDF";
        if (tool.Id == "extract" && body.Children[0] is TextBlock extractHeading) extractHeading.Text = "Ambil halaman PDF";
        if (tool.Id == "remove" && body.Children[0] is TextBlock removeHeading) removeHeading.Text = "Hapus halaman PDF";
        var favorite = new ToggleButton { Content = "★ Favorit", IsChecked = favorites.Contains(tool.Id) };
        favorite.Click += (_, _) => { if (favorite.IsChecked == true) favorites.Add(tool.Id); else favorites.Remove(tool.Id); SaveLibrary(); };
        if (tool.Id is not ("pdf-jpg" or "split" or "remove" or "extract")) body.Children.Add(favorite);
        if (!tool.Available)
        {
            body.Children.Add(new InfoBar { IsOpen = true, IsClosable = false, Severity = InfoBarSeverity.Warning,
                Title = "Belum dapat diproses", Message = tool.Description });
            ShowPendingWorkspace(tool);
            return;
        }
        if (tool.Id is "merge" or "pdf-jpg" or "remove" or "extract")
        {
            var limits = new Expander { Header = "Versi pengembangan · Gunakan salinan uji · Lihat batasan", HorizontalAlignment = HorizontalAlignment.Stretch,
                Content = Copy("Periksa hasil sebelum digunakan. Tata letak, form, bookmark, tautan, dan tanda tangan digital belum dijamin tetap sama. Maksimal 100 MB / 1000 halaman.", 13) };
            body.Children.Add(limits);
        }
        else if (tool.Id == "word-pdf") body.Children.Add(new InfoBar { IsOpen = true, IsClosable = false, Severity = InfoBarSeverity.Informational, Title = "DOCX lokal · A4 portrait", Message = "Periksa font dan tata letak hasil. Microsoft Word lokal berlisensi diperlukan; A3/Letter/landscape, DOC/DOCM dan konten aktif belum didukung." });
        else if (tool.Id != "split") body.Children.Add(new InfoBar { IsOpen = true, IsClosable = false, Severity = InfoBarSeverity.Warning,
            Title = "Salinan uji saja", Message = "Versi awal: tata letak, form, bookmark, tautan, dan tanda tangan digital belum dijamin tetap sama. Crop tidak menghapus konten tersembunyi. Maksimal 100 MB / 1000 halaman." });
        files.Clear(); password.Password = ""; newPassword.Password = ""; pages.Text = "";
        watermark.Text = ""; rotation.SelectedIndex = 0; margin.Value = 20;
        outputName.Text = "hasil-" + tool.Id + (tool.Id == "split" ? ".zip" : tool.Id == "pdf-jpg" ? ".jpg" : ".pdf");
        status.Text = "Pilih file untuk memulai."; resultPath = null; openResult.Visibility = Visibility.Collapsed;
        if (fileList.Parent is Panel previousFiles) previousFiles.Children.Remove(fileList);
        if (toolOptions.Parent is Panel previousOptions) previousOptions.Children.Remove(toolOptions);
        toolWorkspace.Children.Clear(); toolOptions.Children.Clear();
        var inputArea = new StackPanel { Spacing = 16 };
        var toolbar = new StackPanel { Spacing = 8 };
        toolbar.Children.Add(ActionButton("Tambah file", async () => await PickFilesAsync()));
        toolbar.Children.Add(ActionButton("Keluarkan dari daftar", () => { if (fileList.SelectedIndex >= 0) RemoveToolInput(files[fileList.SelectedIndex]); }));
        toolbar.Children.Add(ActionButton("↑", () => Move(-1), "Pindahkan file ke atas"));
        toolbar.Children.Add(ActionButton("↓", () => Move(1), "Pindahkan file ke bawah"));
        inputArea.Children.Add(toolbar); inputArea.Children.Add(fileList); editable.Add(fileList);
        inputArea.Children.Add(new TextBlock { Text = "Belum ada pratinjau halaman. Urutan daftar di atas digunakan untuk menyusun hasil.", TextWrapping = TextWrapping.Wrap, Foreground = Brush(100, 116, 139) });
        toolWorkspace.Children.Add(tool.Id == "split" ? SplitFrame(inputArea) : Section(tool.Id == "merge" ? "Dokumen" : "01  File sumber", "Pilih salinan uji. File asli tidak ditimpa.", inputArea));
        toolWorkspace.Children.Add(tool.Id == "split" ? SplitFrame(toolOptions) : Section(tool.Id == "merge" ? "Hasil gabungan" : "02  Pengaturan & hasil", "Tentukan opsi dan lokasi simpan.", toolOptions));
        body.Children.Add(toolWorkspace); LayoutCards();
        if (tool.Id is not ("merge" or "images" or "protect" or "unlock" or "compress" or "word-pdf")) AddEditable(pages);
        outputQuality = toolDrafts.TryGetValue(tool.Id, out var qualityDraft) ? qualityDraft.Quality : "medium";
        if (tool.Id == "images") toolOptions.Children.Add(BuildQualityOptions(true));
        if (tool.Id == "compress") toolOptions.Children.Add(Copy("Kualitas asli · Kompresi stream lossless. Ukuran bisa tetap atau bertambah.", 12));
        if (tool.Id == "watermark") AddEditable(watermark);
        if (tool.Id == "rotate") AddEditable(rotation);
        if (tool.Id == "crop") AddEditable(margin);
        if (tool.Id is not ("images" or "word-pdf")) AddEditable(password);
        if (tool.Id == "protect") AddEditable(newPassword);
        AddEditable(outputName);
        toolOptions.Children.Add(BuildOutputLocation());
        var actions = new StackPanel { Spacing = 12 };
        actions.Children.Add(run); actions.Children.Add(cancel); actions.Children.Add(openResult);
        toolOptions.Children.Add(actions); toolOptions.Children.Add(progress); toolOptions.Children.Add(status);
        run.IsEnabled = true;
        RestoreToolDraft(tool.Id);
        if (tool.Id is "pdf-jpg" or "split" or "remove" or "extract") actions.Children.Clear();
        if (tool.Id == "merge") BuildMergeWorkspace(inputArea);
        else BuildSourceWorkspace(inputArea);
        body.Children.Add(DocumentCloseButton());
    }

    private void AddEditable(Control control) { if (control.Parent is Panel old) old.Children.Remove(control); toolOptions.Children.Add(control); editable.Add(control); }

    private Button ActionButton(string label, Action action, string? accessibleLabel = null)
    {
        var button = new Button { Content = label };
        AutomationProperties.SetName(button, accessibleLabel ?? label);
        button.Click += (_, _) => action(); editable.Add(button); return button;
    }

    private void Move(int delta)
    {
        var index = fileList.SelectedIndex;
        if (index < 0 || index + delta < 0 || index + delta >= files.Count) return;
        files.Move(index, index + delta); fileList.SelectedIndex = index + delta;
    }

    private async Task PickFilesAsync()
    {
        try
        {
            var picker = new FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            foreach (var extension in selected?.Id == "images" ? new[] { ".jpg", ".jpeg", ".png" } : selected?.Id == "word-pdf" ? new[] { ".docx" } : new[] { ".pdf" })
                picker.FileTypeFilter.Add(extension);
            var picked = await picker.PickMultipleFilesAsync();
            if (picked.Count == 0) return;
            if (selected?.Id == "word-pdf") { await AcceptWordInputsAsync(picked); return; }
            if (selected?.Id == "merge") { await AcceptMergeFilesAsync(picked); return; }
            if (selected?.Id is not ("merge" or "images")) files.Clear();
            foreach (var file in picked)
            {
                if (files.Count >= 50) break;
                if (selected?.Id == "merge" && files.Contains(file.Path)) continue;
                files.Add(file.Path);
                if (selected?.Id is not ("merge" or "images")) break;
            }
            status.Text = string.Create(CultureInfo.InvariantCulture, $"{files.Count} file dipilih. Urutan daftar dipakai saat proses.");
        }
        catch (Exception) { status.Text = "Pemilih file tidak dapat dibuka. Coba kembali."; }
    }

    private async Task PickFolderAsync()
    {
        try
        {
            var picker = new FolderPicker(); picker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            var chosen = await picker.PickSingleFolderAsync();
            if (chosen is null) return;
            folder = chosen.Path; location.Text = "Folder hasil: " + folder;
            if (selected?.Id == "merge") UpdateMergeReady();
        }
        catch (Exception) { status.Text = "Pemilih folder tidak dapat dibuka. Coba kembali."; }
    }

    private async Task ExecuteAsync()
    {
        if (active is not null || selected is null) return;
        if (selected.Id == "organize") { await ExecuteOrganizerAsync(); return; }
        if (selected.Id == "organize")
        {
            if (importing || pageLayout.Count == 0) { status.Text = "Tambahkan minimal satu halaman terlebih dahulu."; return; }
            files.Clear(); foreach (var path in organizerSources) files.Add(path);
        }
        if (files.Count == 0) { status.Text = "Pilih file terlebih dahulu."; return; }
        if (folder is null) { status.Text = "Pilih folder hasil terlebih dahulu."; return; }
        UpdateAutomaticOutputName();
        var name = outputName.Text.Trim();
        var extension = selected.Id == "split" ? ".zip" : selected.Id == "pdf-jpg" ? ".jpg" : ".pdf";
        if (name.Length == 0 || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name != Path.GetFileName(name)
            || !name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
        { status.Text = "Masukkan nama file yang valid dengan akhiran " + extension; return; }
        var target = AvailableOutputPath(folder, name);
        var request = new PdfRequest(selected.Id, files.ToArray(), target, pages.Text, watermark.Text,
            selected.Id == "pdf-jpg" ? jpgAngle : (int)rotation.SelectedItem, margin.Value, password.Password, newPassword.Password,
            selected.Id == "organize" ? pageLayout.Select(p => new PagePlacement(p.Source, p.Page, p.Rotation)).ToArray() : null,
            selected.Id == "merge" ? files.Select(p => mergeAngles.GetValueOrDefault(p)).ToArray() : null,
            Quality: selected.Id is "images" or "pdf-jpg" ? outputQuality : "original");
        using var cancellation = new CancellationTokenSource(); active = cancellation;
        foreach (var control in editable.OfType<Control>()) control.IsEnabled = false;
        foreach (var item in navigation) item.IsEnabled = false;
        run.IsEnabled = false; cancel.IsEnabled = true; progress.Visibility = Visibility.Visible;
        globalSearch.IsEnabled = false;
        openResult.Visibility = Visibility.Collapsed; status.Text = "Memproses lokal… Anda dapat membatalkan.";
        PdfReply? jpgCompletion = null;
        PdfReply? mergeCompletion = null;
        if (selected.Id == "merge") SetMergeProcessing(true);
        if (selected.Id == "pdf-jpg") ShowJpgProgress();
        try
        {
            var reply = selected.Id == "pdf-jpg"
                ? await ExportJpgAsync(request, cancellation.Token)
                : selected.Id == "word-pdf" ? await WordPdfClient.RunAsync(request, cancellation.Token, new Progress<string>(label => status.Text = label))
                : selected.Id == "images" ? await ExportImagesWithQualityAsync(request, cancellation.Token)
                : await WorkerClient.RunAsync(request, cancellation.Token);
            status.Text = reply.Success
                ? string.Create(CultureInfo.InvariantCulture, $"{reply.Message}\n{reply.Pages} halaman • {reply.Bytes / 1024d:N1} KB\nTersimpan: {(selected.Id == "pdf-jpg" ? jpgResultFile : target)}")
                : reply.Message;
            if (reply.Success)
            {
                MarkRestartToolSaved(selected.Id);
                resultPath = selected.Id == "pdf-jpg" ? jpgResultFile : target; openResult.Visibility = Visibility.Visible;
                history.Insert(0, selected.Name + " — " + name + " — selesai lokal");
                completedActivities.Insert(0, (selected, name));
            }
            if (selected.Id == "pdf-jpg") jpgCompletion = reply;
            if (selected.Id != "pdf-jpg") mergeCompletion = reply;
        }
        finally
        {
            active = null;
            if (jpgCompletion?.Success == true || mergeCompletion?.Success == true) { password.Password = ""; newPassword.Password = ""; }
            foreach (var control in editable.OfType<Control>()) control.IsEnabled = true;
            foreach (var item in navigation) item.IsEnabled = true;
            run.IsEnabled = true; cancel.IsEnabled = false; progress.Visibility = Visibility.Collapsed;
            globalSearch.IsEnabled = true;
            if (selected?.Id == "merge")
            {
                SetMergeProcessing(false); UpdateMergeReady();
            }
            if (mergeCompletion is { Success: true })
            { if (selected?.Id == "word-pdf" && resultPath is not null) ShowCurrentResult([resultPath], $"{mergeCompletion.Pages} halaman · {mergeCompletion.Bytes / 1024d:N1} KB " + Environment.NewLine + mergeCompletion.Message); else ShowMergeResult(mergeCompletion); }
            if (selected?.Id == "pdf-jpg") FinishJpgScreen(jpgCompletion);
        }
    }

    private void ShowHistory()
    {
        if (active is not null) return;
        BuildPersistentHistory();
    }

    private void ShowHelp()
    {
        if (active is not null) return;
        body.Children.Clear(); Heading("Bantuan", "Pilih alat → tambah file → atur opsi → pilih folder dan nama baru → proses → buka hasil.");
        body.Children.Add(Section("Butuh bantuan?", "Susun laporan masalah dan panduan screenshot tanpa upload otomatis.", ActionButton("Buat laporan masalah", ShowIssueReport)));
        body.Children.Add(new TextBlock { Text = "Untuk melaporkan masalah: tekan Win + Shift + S, pilih area layar, lalu tempel tangkapan layar di percakapan proyek. Hindari menampilkan isi dokumen sensitif atau kata sandi.\n\nAplikasi tidak menimpa sumber atau hasil existing. Pisahkan PDF menyimpan PDF langsung, dalam folder baru bila hasilnya lebih dari satu. Organize menerima urutan seperti 3,1,1,2. Crop hanya mengubah area tampil, bukan menghapus data.\n\nIni build pengembangan, bukan rilis produksi. Operasi parser berjalan di proses terpisah dengan pembatalan dan timeout; pemisahan proses bukan sandbox keamanan.", TextWrapping = TextWrapping.Wrap });
    }

    private static SolidColorBrush Brush(byte red, byte green, byte blue) => new(ColorHelper.FromArgb(255, red, green, blue));
}
