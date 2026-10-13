using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private bool openingEmptyInput;
    private ToolCard? emptyTool;
    private TextBlock? emptyNotice;
    private Border? emptyPanel;
    private bool acceptingEmptyInput;
    private bool pickingMergeEmptyInput;
    private ScrollViewer? mergeEmptyDropSurface;
    private bool mergeEmptyPreviousAllowDrop;
    private Microsoft.UI.Xaml.Media.Brush? mergeEmptyPreviousBackground;
    private TextBlock? mergeEmptyDropHint;

    private static string InputFormat(ToolCard tool) => tool.Id switch
    {
        "images" => "JPG / PNG", "word-pdf" => "Word DOCX (A4 portrait)", "ppt-pdf" => "PowerPoint",
        "excel-pdf" => "Excel", "html-pdf" => "HTML", "scan" => "scanner", _ => "PDF"
    };

    private void ShowEmptyTool(ToolCard tool)
    {
        if (!tool.Available) { ShowUnavailableEmptyTool(tool); return; }
        BeginScreen(tool.Name, tool.Description);
        activeToolControls = null;
        selected = tool; emptyTool = tool;
        if (tool.Id == "edit" && liveEditor?.HasDocument == true) { ShowLiveEditor(); return; }
        if (tool.Id == "merge") { ShowMergeEmptyCard(); return; }

        var (title, pickerLabel, hint) = EmptyToolLabels(tool.Id);
        if (body.Children[0] is TextBlock heading) heading.Text = title;
        if (body.Children[1] is TextBlock description)
            description.Text = "Mulai dari dokumen Anda. Pengaturan muncul setelah file dipilih.";

        // One entry component for all active tools. Only the icon, format and
        // factual capability hint vary; loaded workspaces keep their own layouts.
        var content = new StackPanel { Spacing = 12, MaxWidth = 520, HorizontalAlignment = HorizontalAlignment.Stretch };
        content.Children.Add(EmptyToolIcon(tool));
        content.Children.Add(new TextBlock { Text = "Mulai dengan dokumen Anda", FontSize = 24,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center });
        var dropHint = Copy("Seret file ke sini", 14); dropHint.TextAlignment = TextAlignment.Center;
        content.Children.Add(dropHint);

        var actions = new StackPanel { Spacing = 12, MaxWidth = 320, HorizontalAlignment = HorizontalAlignment.Stretch };
        var pick = HomeButton(pickerLabel, "\uE710", async () => await PickEmptyToolAsync(), true);
        var paste = HomeButton("Tempel file", "\uE77F", async () => await PasteEmptyToolAsync());
        ToolTipService.SetToolTip(pick, pickerLabel + " · Ctrl+O");
        ToolTipService.SetToolTip(paste, "Salin file dari File Explorer, lalu tempel · Ctrl+V");
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetAcceleratorKey(pick, "Ctrl+O");
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetAcceleratorKey(paste, "Ctrl+V");
        actions.Children.Add(pick); actions.Children.Add(paste); content.Children.Add(actions);
        emptyNotice = Copy(hint, 13); emptyNotice.TextAlignment = TextAlignment.Center; emptyNotice.MinHeight = 20;
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetLiveSetting(emptyNotice, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
        content.Children.Add(emptyNotice);
        emptyPanel = new Border { Child = content, MaxWidth = 850, HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(32), Margin = new Thickness(0, 28, 0, 12), CornerRadius = new CornerRadius(20),
            Background = Brush(240, 247, 255), BorderBrush = Brush(153, 190, 240), BorderThickness = new Thickness(1), AllowDrop = true };
        emptyPanel.DragOver += (_, e) => e.AcceptedOperation = e.DataView.Contains(StandardDataFormats.StorageItems) ? DataPackageOperation.Copy : DataPackageOperation.None;
        emptyPanel.Drop += async (_, e) => { var deferral = e.GetDeferral(); try { await AcceptEmptyInputsAsync(await e.DataView.GetStorageItemsAsync()); } catch (Exception) { if (emptyNotice is not null) emptyNotice.Text = "File tidak dapat dibaca."; } finally { deferral.Complete(); } };
        body.Children.Add(emptyPanel);
        body.Children.Add(new TextBlock { Text = "File asli tidak diubah · Versi pengembangan · Gunakan salinan dokumen uji", TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, FontSize = 12 });
    }

    private bool MergeEmptyActive => emptyTool?.Id == "merge" && emptyPanel?.IsLoaded == true && body.Children.Contains(emptyPanel);

    private void ShowMergeEmptyCard()
    {
        body.Children.Clear();
        var content = new StackPanel { Spacing = 16, MaxWidth = 480, HorizontalAlignment = HorizontalAlignment.Stretch };
        var mascot = MergeBiblo(120); content.Children.Add(mascot);
        var mergeTitle = new TextBlock { Text = "Gabungkan PDF", FontSize = 32, Foreground = Brush(9, 35, 87),
            FontWeight = Microsoft.UI.Text.FontWeights.Bold, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center };
        content.Children.Add(mergeTitle);
        var subtitle = Copy("Pilih minimal 2 PDF untuk digabungkan.", 18); subtitle.TextAlignment = TextAlignment.Center;
        subtitle.Foreground = Brush(69, 89, 130); content.Children.Add(subtitle);
        var actions = new StackPanel { Spacing = 12, Margin = new Thickness(0, 8, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch };
        var pick = HomeButton("Pilih PDF", "\uE8B7", async () => await PickEmptyToolAsync(), true);
        pick.HorizontalAlignment = HorizontalAlignment.Stretch; pick.MinHeight = 60; pick.FontSize = 22;
        pick.Background = Brush(0, 103, 255); pick.Foreground = Brush(255, 255, 255); pick.CornerRadius = new CornerRadius(14);
        pick.Resources["ButtonBackgroundPointerOver"] = Brush(0, 85, 210);
        pick.Resources["ButtonForegroundPointerOver"] = Brush(255, 255, 255);
        pick.Resources["ButtonBackgroundPressed"] = Brush(9, 65, 150);
        pick.Resources["ButtonForegroundPressed"] = Brush(255, 255, 255);
        pick.Content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16, Children = {
            new FontIcon { Glyph = "\uE8B7", FontSize = 28 }, new TextBlock { Text = "Pilih PDF", FontSize = 22,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center } } };
        var paste = HomeButton("Tempel file", "\uE77F", async () => await PasteEmptyToolAsync());
        paste.HorizontalAlignment = HorizontalAlignment.Stretch; paste.MinHeight = 52; paste.FontSize = 17;
        paste.Background = Brush(255, 255, 255); paste.Foreground = Brush(0, 103, 255);
        paste.BorderBrush = Brush(0, 103, 255); paste.BorderThickness = new Thickness(1); paste.CornerRadius = new CornerRadius(14);
        paste.Resources["ButtonBackgroundPointerOver"] = Brush(233, 242, 255);
        paste.Resources["ButtonForegroundPointerOver"] = Brush(0, 85, 210);
        paste.Content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 14, Children = {
            new FontIcon { Glyph = "\uE77F", FontSize = 23 }, new TextBlock { Text = "Tempel file", FontSize = 17,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center } } };
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(paste, "Tempel file");
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetAcceleratorKey(paste, "Ctrl+V");
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetAcceleratorKey(pick, "Ctrl+O");
        ToolTipService.SetToolTip(pick, new ToolTip { Content = "Ctrl+O", PlacementTarget = pick });
        ToolTipService.SetToolTip(paste, new ToolTip { Content = "Ctrl+V", PlacementTarget = paste });
        actions.Children.Add(pick); actions.Children.Add(paste); content.Children.Add(actions);
        var dropInstruction = MergeEmptyInstruction("\uE8A5", "atau seret PDF ke area ini");
        dropInstruction.HorizontalAlignment = HorizontalAlignment.Center;
        mergeEmptyDropHint = (TextBlock)dropInstruction.Children[1];
        content.Children.Add(dropInstruction);
        emptyNotice = Copy("", 14); emptyNotice.TextAlignment = TextAlignment.Center;
        emptyNotice.Foreground = Brush(69, 89, 130); emptyNotice.MinHeight = 0;
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetLiveSetting(emptyNotice, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
        // Status remains live but contributes no idle whitespace or duplicate instruction.
        var notice = emptyNotice; var dropLabel = mergeEmptyDropHint;
        dropInstruction.Children.Add(notice); notice.Visibility = Visibility.Collapsed; Grid.SetColumn(notice, 1);
        var noticeCallback = notice.RegisterPropertyChangedCallback(TextBlock.TextProperty, (_, _) => {
            notice.Visibility = string.IsNullOrEmpty(notice.Text) ? Visibility.Collapsed : Visibility.Visible;
            dropLabel.Visibility = string.IsNullOrEmpty(notice.Text) ? Visibility.Visible : Visibility.Collapsed;
        });
        emptyPanel = new Border { Child = content, MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(24), Margin = new Thickness(0, 4, 0, 0), CornerRadius = new CornerRadius(22),
            Background = Brush(255, 255, 255), BorderBrush = Brush(201, 218, 255), BorderThickness = new Thickness(1) };
        emptyPanel.Loaded += MergeEmptyLoaded;
        emptyPanel.Loaded += (_, _) => { if (MergeEmptyActive) { standardPageScroll?.ChangeView(null, 0, null, true); pick.Focus(FocusState.Programmatic); } };
        emptyPanel.Unloaded += MergeEmptyUnloaded;
        emptyPanel.Unloaded += (_, _) => notice.UnregisterPropertyChangedCallback(TextBlock.TextProperty, noticeCallback);
        var resultPanel = emptyPanel;
        var viewport = standardPageScroll;
        void FitMergeEmpty()
        {
            var compact = viewport is not null && viewport.ActualHeight < 650;
            mascot.Width = mascot.Height = compact ? 96 : 120; content.Spacing = compact ? 12 : 16;
            mergeTitle.FontSize = 32; subtitle.FontSize = 18;
            resultPanel.Padding = compact ? new Thickness(24, 16, 24, 16) : new Thickness(24);
            pick.MinHeight = compact ? 52 : 60; paste.MinHeight = compact ? 44 : 52;
        }
        void ResizeMergeEmpty(object sender, SizeChangedEventArgs e) => FitMergeEmpty();
        emptyPanel.Loaded += (_, _) => FitMergeEmpty();
        if (viewport is not null) viewport.SizeChanged += ResizeMergeEmpty;
        emptyPanel.Unloaded += (_, _) => { if (viewport is not null) viewport.SizeChanged -= ResizeMergeEmpty; };
        body.Children.Add(emptyPanel);
        var safety = Copy("\u2713  File asli tetap aman · Diproses di perangkat ini", 14);
        safety.TextAlignment = TextAlignment.Center; safety.Foreground = Brush(0, 110, 82); body.Children.Add(safety);
    }

    private static Grid MergeEmptyInstruction(string glyph, string text)
    {
        var row = new Grid { ColumnSpacing = 14, HorizontalAlignment = HorizontalAlignment.Stretch };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.Children.Add(new FontIcon { Glyph = glyph, FontSize = 22, Foreground = Brush(0, 103, 255), VerticalAlignment = VerticalAlignment.Center });
        var label = Copy(text, 14); label.Foreground = Brush(9, 35, 87); label.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(label, 1); row.Children.Add(label); return row;
    }

    private void MergeEmptyLoaded(object sender, RoutedEventArgs e)
    {
        DetachMergeEmptyDrop();
        if (!MergeEmptyActive || standardPageScroll is null) return;
        mergeEmptyDropSurface = standardPageScroll;
        mergeEmptyPreviousAllowDrop = standardPageScroll.AllowDrop;
        mergeEmptyPreviousBackground = standardPageScroll.Background;
        standardPageScroll.AllowDrop = true; standardPageScroll.Background = Brush(243, 248, 255);
        standardPageScroll.DragOver += MergeEmptyDragOver;
        standardPageScroll.DragLeave += MergeEmptyDragLeave;
        standardPageScroll.Drop += MergeEmptyDrop;
        Closed += MergeEmptyWindowClosed;
    }

    private void MergeEmptyUnloaded(object sender, RoutedEventArgs e)
    {
        DetachMergeEmptyDrop();
        if (sender is Border panel) { panel.Loaded -= MergeEmptyLoaded; panel.Unloaded -= MergeEmptyUnloaded; }
    }

    private void MergeEmptyWindowClosed(object sender, WindowEventArgs args) => DetachMergeEmptyDrop();

    private void DetachMergeEmptyDrop()
    {
        SetMergeEmptyDragHighlight(false);
        Closed -= MergeEmptyWindowClosed;
        if (mergeEmptyDropSurface is not { } surface) return;
        surface.DragOver -= MergeEmptyDragOver; surface.DragLeave -= MergeEmptyDragLeave; surface.Drop -= MergeEmptyDrop;
        surface.AllowDrop = mergeEmptyPreviousAllowDrop; surface.Background = mergeEmptyPreviousBackground;
        mergeEmptyDropSurface = null;
    }

    private void MergeEmptyDragOver(object sender, DragEventArgs e)
    {
        if (!MergeEmptyActive) return;
        e.AcceptedOperation = e.DataView.Contains(StandardDataFormats.StorageItems) ? DataPackageOperation.Copy : DataPackageOperation.None;
        SetMergeEmptyDragHighlight(e.AcceptedOperation == DataPackageOperation.Copy);
        e.Handled = true;
    }

    private void MergeEmptyDragLeave(object sender, DragEventArgs e) => SetMergeEmptyDragHighlight(false);

    private void SetMergeEmptyDragHighlight(bool highlighted)
    {
        if (emptyTool?.Id != "merge" || emptyPanel is null || mergeEmptyDropHint is null) return;
        emptyPanel.Background = highlighted ? Brush(233, 242, 255) : Brush(255, 255, 255);
        emptyPanel.BorderBrush = highlighted ? Brush(0, 103, 255) : Brush(201, 218, 255);
        mergeEmptyDropHint.Text = highlighted ? "Lepaskan untuk menambahkan PDF" : "atau seret PDF ke area ini";
    }

    private async void MergeEmptyDrop(object sender, DragEventArgs e)
    {
        if (!MergeEmptyActive || !e.DataView.Contains(StandardDataFormats.StorageItems)) return;
        e.Handled = true;
        SetMergeEmptyDragHighlight(false);
        var panel = emptyPanel; var deferral = e.GetDeferral();
        try { var items = await e.DataView.GetStorageItemsAsync(); if (MergeEmptyActive && ReferenceEquals(panel, emptyPanel)) await AcceptEmptyInputsAsync(items); }
        catch (Exception) { if (MergeEmptyActive && ReferenceEquals(panel, emptyPanel) && emptyNotice is not null) emptyNotice.Text = "File tidak dapat dibaca."; }
        finally { deferral.Complete(); }
    }

    private static (string Title, string Picker, string Hint) EmptyToolLabels(string id) => id switch
    {
        "merge" => ("Gabungkan PDF", "Pilih PDF", "Minimal 2 PDF untuk digabungkan."),
        "split" => ("Pisahkan PDF", "Pilih PDF", "Satu PDF untuk memulai."),
        "remove" => ("Hapus halaman PDF", "Pilih PDF", "Sumber tetap utuh."),
        "extract" => ("Ambil halaman PDF", "Pilih PDF", "Satu PDF untuk memulai."),
        "organize" => ("Atur halaman PDF", "Pilih PDF", "Satu atau beberapa PDF."),
        "compress" => ("Kompres PDF", "Pilih PDF", "Kompresi lossless."),
        "images" => ("Gambar ke PDF", "Pilih gambar", "JPG atau PNG."),
        "word-pdf" => ("Word ke PDF", "Pilih DOCX", "A4 portrait · Perlu Word lokal berlisensi."),
        "pdf-jpg" => ("PDF ke JPG", "Pilih PDF", "Pilih halaman setelah dibuka."),
        "rotate" => ("Putar halaman", "Pilih PDF", "90°, 180°, atau 270°."),
        "numbers" => ("Nomor halaman", "Pilih PDF", "Nomor di bawah halaman."),
        "watermark" => ("Watermark", "Pilih PDF", "Teks penanda dokumen."),
        "crop" => ("Potong halaman", "Pilih PDF", "Bukan penghapusan data."),
        "edit" => ("Edit isi PDF", "Pilih PDF", "Edit teks yang didukung."),
        "unlock" => ("Buka kunci PDF", "Pilih PDF", "Perlu kata sandi pemilik."),
        "protect" => ("Kunci PDF", "Pilih PDF", "Kata sandi pembuka baru."),
        _ => throw new ArgumentOutOfRangeException(nameof(id), "Active tools require reviewed entry copy.")
    };

    private static Border EmptyToolIcon(ToolCard tool)
    {
        // Local vector geometry: no external assets, fonts or network dependency.
        const string file = "M6,2 L14,2 L20,8 L20,22 L6,22 Z M14,2 L14,8 L20,8 ";
        var path = tool.Id switch
        {
            "merge" => "M8,2 L15,2 L20,7 L20,19 L8,19 Z M15,2 L15,7 L20,7 M5,5 L3,5 L3,22 L15,22",
            "split" => "M8,7 A3,3 0 1 1 2,7 A3,3 0 1 1 8,7 M8,18 A3,3 0 1 1 2,18 A3,3 0 1 1 8,18 M8,8 L21,21 M8,17 L21,2",
            "remove" => file + "M10,15 L16,15",
            "extract" => file + "M10,15 L16,15 M13,12 L13,18",
            "organize" => "M3,4 L4,4 M8,4 L21,4 M3,9 L4,9 M8,9 L21,9 M3,14 L4,14 M8,14 L21,14 M3,19 L4,19 M8,19 L21,19",
            "compress" => "M3,3 L9,9 M3,9 L9,9 L9,3 M21,21 L15,15 M15,21 L15,15 L21,15",
            "images" or "pdf-jpg" => "M3,3 L21,3 L21,21 L3,21 Z M3,17 L9,11 L13,15 L16,12 L21,17 M17,7 A1,1 0 1 1 15,7 A1,1 0 1 1 17,7",
            "word-pdf" => file + "M8,12 L10,18 L12,14 L14,18 L16,12",
            "rotate" => "M4,9 A9,9 0 1 1 3,15 M4,3 L4,9 L10,9",
            "numbers" => "M9,2 L6,22 M18,2 L15,22 M3,8 L22,8 M2,16 L21,16",
            "watermark" => "M12,2 C9,7 4,11 4,16 A8,7 0 0 0 20,16 C20,11 15,7 12,2 Z M8,15 C7,18 10,20 12,19",
            "crop" => "M6,2 L6,18 L22,18 M2,6 L18,6 L18,22",
            "edit" => "M3,17 L16,4 L21,9 L8,22 L2,23 Z M14,6 L19,11 M3,17 L8,22",
            "unlock" => "M5,11 L19,11 L19,22 L5,22 Z M8,11 L8,6 A5,5 0 0 1 18,6 M12,15 L12,18",
            "protect" => "M5,11 L19,11 L19,22 L5,22 Z M7,11 L7,6 A5,5 0 0 1 17,6 L17,11 M12,15 L12,18",
            _ => throw new ArgumentOutOfRangeException(nameof(tool))
        };
        return new Border { Width = 60, Height = 60, CornerRadius = new CornerRadius(16), HorizontalAlignment = HorizontalAlignment.Center,
            Background = Brush(222, 237, 255), Child = (UIElement)Microsoft.UI.Xaml.Markup.XamlReader.Load(
                $"<Viewbox xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Width='32' Height='32'><Canvas Width='24' Height='24'><Path Data='{path}' Stroke='#0F69E8' StrokeThickness='1.7' StrokeLineJoin='Round' StrokeStartLineCap='Round' StrokeEndLineCap='Round'/></Canvas></Viewbox>") };
    }

    // Unavailable tools retain their existing explanation and disabled controls.
    private void ShowUnavailableEmptyTool(ToolCard tool)
    {
        BeginScreen(tool.Name, tool.Available ? tool.Description : "Fitur ini belum aktif.");
        activeToolControls = null;
        // Keep route identity canonical while displaying Indonesian titles.
        if (body.Children[0] is TextBlock heading) heading.Text = CatalogName(tool);
        if (body.Children[1] is TextBlock description) description.Text = tool.Available
            ? tool.Id == "merge" ? "Satukan beberapa PDF menjadi satu dokumen." : "Mulai dari dokumen Anda. Pengaturan muncul setelah file dipilih."
            : "Fitur ini belum aktif. Belum ada dokumen yang akan diproses.";
        selected = tool; emptyTool = tool;
        if (tool.Id == "edit" && liveEditor?.HasDocument == true) { ShowLiveEditor(); return; }
        var content = new StackPanel { Spacing = 16, HorizontalAlignment = HorizontalAlignment.Center, MaxWidth = 520 };
        var icon = LargeIcon(tool.Id == "merge" ? "\uE8C8" : tool.Glyph); icon.HorizontalAlignment = HorizontalAlignment.Center; content.Children.Add(icon);
        content.Children.Add(new TextBlock { Text = tool.Available ? "Mulai dengan dokumen Anda" : "Fitur sedang disiapkan", FontSize = 24,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center });
        content.Children.Add(new TextBlock { Text = tool.Id == "scan" ? "Koneksi scanner belum tersedia" : "Tarik file " + InputFormat(tool) + " ke sini", TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap });
        var pick = HomeButton("Pilih " + InputFormat(tool) + "   Ctrl+O", "\uE710", async () => await PickEmptyToolAsync(), true);
        pick.IsEnabled = tool.Available; content.Children.Add(pick);
        var paste = HomeButton("Tempel file   Ctrl+V", "\uE77F", async () => await PasteEmptyToolAsync());
        paste.IsEnabled = tool.Available; content.Children.Add(paste);
        // Ctrl+V is registered once on the window root.
        content.Children.Add(Copy("Salin file dari File Explorer untuk menempel", 12));
        emptyNotice = Copy(tool.Available ? tool.Id switch
        {
            "merge" => "Minimal 2 PDF untuk digabungkan.", "images" => "Satu atau beberapa JPG/PNG menjadi PDF.",
            "word-pdf" => "Pilih satu DOCX A4 portrait. Microsoft Word lokal berlisensi diperlukan.",
            "organize" => "Pilih satu atau beberapa PDF untuk menyusun halaman.", _ => "Pilih satu PDF untuk memulai."
        } : tool.Description, 13);
        content.Children.Add(emptyNotice);
        emptyPanel = new Border { Child = content, MaxWidth = 850, HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(32), Margin = new Thickness(0, 28, 0, 12), CornerRadius = new CornerRadius(20),
            Background = Brush(240, 247, 255), BorderBrush = Brush(153, 190, 240), BorderThickness = new Thickness(1), AllowDrop = tool.Available };
        emptyPanel.DragOver += (_, e) => e.AcceptedOperation = e.DataView.Contains(StandardDataFormats.StorageItems) ? DataPackageOperation.Copy : DataPackageOperation.None;
        emptyPanel.Drop += async (_, e) => { var deferral = e.GetDeferral(); try { await AcceptEmptyInputsAsync(await e.DataView.GetStorageItemsAsync()); } catch (Exception) { if (emptyNotice is not null) emptyNotice.Text = "File tidak dapat dibaca."; } finally { deferral.Complete(); } };
        body.Children.Add(emptyPanel);
        body.Children.Add(new TextBlock { Text = "File asli tidak diubah · Versi pengembangan · Gunakan salinan dokumen uji", TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, FontSize = 12 });
    }

    private async Task PickEmptyToolAsync()
    {
        if (emptyTool is not { Available: true } || active is not null || importing) return;
        var mergePicker = MergeEmptyActive;
        if (mergePicker && (pickingMergeEmptyInput || messageOpen)) return;
        if (mergePicker) pickingMergeEmptyInput = true;
        var panel = emptyPanel;
        try
        {
            var picker = new FileOpenPicker(); WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            if (emptyTool.Id == "images") { picker.FileTypeFilter.Add(".jpg"); picker.FileTypeFilter.Add(".jpeg"); picker.FileTypeFilter.Add(".png"); }
            else picker.FileTypeFilter.Add(emptyTool.Id == "word-pdf" ? ".docx" : ".pdf");
            var items = await picker.PickMultipleFilesAsync();
            if (mergePicker && items.Count == 0) return; // Cancel keeps the idle Merge instruction.
            if (ReferenceEquals(panel, emptyPanel) && panel?.IsLoaded == true && body.Children.Contains(panel)) await AcceptEmptyInputsAsync(items);
        }
        catch (Exception) { if (emptyNotice is not null) emptyNotice.Text = "File tidak dapat dibuka. Coba kembali."; }
        finally { if (mergePicker) pickingMergeEmptyInput = false; }
    }

    private async Task PasteEmptyToolAsync()
    {
        var panel = emptyPanel;
        try
        {
            var data = Clipboard.GetContent();
            if (!data.Contains(StandardDataFormats.StorageItems)) { if (emptyNotice is not null) emptyNotice.Text = "Salin file dari File Explorer, bukan teks atau screenshot."; return; }
            var items = await data.GetStorageItemsAsync();
            if (ReferenceEquals(panel, emptyPanel) && panel?.IsLoaded == true && body.Children.Contains(panel)) await AcceptEmptyInputsAsync(items);
        }
        catch (Exception) { if (emptyNotice is not null) emptyNotice.Text = "Clipboard tidak dapat dibaca."; }
    }

    private async Task AcceptEmptyInputsAsync(IEnumerable<IStorageItem> items)
    {
        var tool = emptyTool;
        var panel = emptyPanel;
        if (tool is not { Available: true } || active is not null || importing || acceptingEmptyInput || panel?.IsLoaded != true || !body.Children.Contains(panel)) return;
        acceptingEmptyInput = true;
        try
        {
            var accepted = new List<string>(); ulong total = 0;
            foreach (var item in items.Take(51))
            {
                if (item is not StorageFile file || string.IsNullOrEmpty(file.Path) || file.Path.StartsWith(@"\\", StringComparison.Ordinal)) continue;
                var extension = file.FileType.ToLowerInvariant();
                if (tool.Id == "images" ? extension is not (".jpg" or ".jpeg" or ".png") : extension != (tool.Id == "word-pdf" ? ".docx" : ".pdf")) continue;
                if (accepted.Contains(file.Path, StringComparer.OrdinalIgnoreCase)) continue;
                total += (await file.GetBasicPropertiesAsync()).Size;
                if (!ReferenceEquals(panel, emptyPanel) || !body.Children.Contains(panel)) return;
                if (total > 100 * 1024 * 1024 || accepted.Count >= 50) { emptyNotice!.Text = "Maksimal 50 file, total 100 MB."; return; }
                accepted.Add(file.Path);
            }
            if (accepted.Count == 0) { emptyNotice!.Text = "Belum ada file dengan format yang sesuai."; return; }
            if (tool.Id is not ("merge" or "images" or "organize") && accepted.Count != 1) { emptyNotice!.Text = "Alat ini menerima satu dokumen. Pilih satu file saja."; return; }
            openingEmptyInput = true;
            if (tool.Id == "merge") DetachMergeEmptyDrop();
            try
            {
                if (tool.Id == "edit") { await OpenEditorDocumentAsync(accepted[0]); return; }
                if (tool.Id == "organize")
                {
                    foreach (var path in accepted) { if (!homeFiles.Contains(path)) homeFiles.Add(path); chosenFiles.Add(path); }
                    ShowOrganizer();
                }
                else { ShowTool(tool); files.Clear(); foreach (var path in accepted) files.Add(path); status.Text = $"{files.Count} file siap. Periksa opsi dan pilih folder hasil."; }
            }
            finally { openingEmptyInput = false; }
        }
        finally { acceptingEmptyInput = false; }
    }
}
