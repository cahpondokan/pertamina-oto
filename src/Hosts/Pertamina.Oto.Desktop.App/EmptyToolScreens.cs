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
        var shortcut = new KeyboardAccelerator { Key = VirtualKey.V, Modifiers = VirtualKeyModifiers.Control };
        shortcut.Invoked += async (_, e) => { if (emptyPanel?.IsLoaded != true || !tool.Available) return; e.Handled = true; await PasteEmptyToolAsync(); };
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
        try
        {
            var picker = new FileOpenPicker(); WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            if (emptyTool.Id == "images") { picker.FileTypeFilter.Add(".jpg"); picker.FileTypeFilter.Add(".jpeg"); picker.FileTypeFilter.Add(".png"); }
            else picker.FileTypeFilter.Add(emptyTool.Id == "word-pdf" ? ".docx" : ".pdf");
            await AcceptEmptyInputsAsync(await picker.PickMultipleFilesAsync());
        }
        catch (Exception) { if (emptyNotice is not null) emptyNotice.Text = "File tidak dapat dibuka. Coba kembali."; }
    }

    private async Task PasteEmptyToolAsync()
    {
        try
        {
            var data = Clipboard.GetContent();
            if (!data.Contains(StandardDataFormats.StorageItems)) { if (emptyNotice is not null) emptyNotice.Text = "Salin file dari File Explorer, bukan teks atau screenshot."; return; }
            await AcceptEmptyInputsAsync(await data.GetStorageItemsAsync());
        }
        catch (Exception) { if (emptyNotice is not null) emptyNotice.Text = "Clipboard tidak dapat dibaca."; }
    }

    private async Task AcceptEmptyInputsAsync(IEnumerable<IStorageItem> items)
    {
        var tool = emptyTool;
        if (tool is not { Available: true } || active is not null || importing) return;
        var accepted = new List<string>(); ulong total = 0;
        foreach (var item in items.Take(51))
        {
            if (item is not StorageFile file || string.IsNullOrEmpty(file.Path) || file.Path.StartsWith(@"\\", StringComparison.Ordinal)) continue;
            var extension = file.FileType.ToLowerInvariant();
            if (tool.Id == "images" ? extension is not (".jpg" or ".jpeg" or ".png") : extension != (tool.Id == "word-pdf" ? ".docx" : ".pdf")) continue;
            if (accepted.Contains(file.Path, StringComparer.OrdinalIgnoreCase)) continue;
            total += (await file.GetBasicPropertiesAsync()).Size;
            if (total > 100 * 1024 * 1024 || accepted.Count >= 50) { emptyNotice!.Text = "Maksimal 50 file, total 100 MB."; return; }
            accepted.Add(file.Path);
        }
        if (accepted.Count == 0) { emptyNotice!.Text = "Belum ada file dengan format yang sesuai."; return; }
        if (tool.Id is not ("merge" or "images" or "organize") && accepted.Count != 1) { emptyNotice!.Text = "Alat ini menerima satu dokumen. Pilih satu file saja."; return; }
        openingEmptyInput = true;
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
}
