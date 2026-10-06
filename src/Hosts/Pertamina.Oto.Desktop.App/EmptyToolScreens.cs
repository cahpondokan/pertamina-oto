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
        if (tool.Id == "merge")
        {
            content.Children.Clear();
            content.Children.Add(new TextBlock { Text = "Gabungkan file PDF", FontSize = 30,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap });
            var hint = Copy("Pilih PDF, atur urutannya, lalu simpan sebagai satu dokumen.", 17);
            hint.TextAlignment = TextAlignment.Center; content.Children.Add(hint);
            pick.Content = "Pilih file PDF"; pick.MinHeight = 68; pick.FontSize = 23;
            pick.HorizontalAlignment = HorizontalAlignment.Center; pick.MinWidth = 0;
            // Restore the 0.1.0 button's desktop width without making it
            // overflow the content on a narrow window.
            void SizeMergePicker() => pick.Width = Math.Min(280, Math.Max(0, content.ActualWidth));
            content.SizeChanged += (_, _) => SizeMergePicker();
            content.Loaded += (_, _) => SizeMergePicker();
            content.Children.Add(pick);
            var dropHint = Copy("atau jatuhkan PDF di sini", 14); dropHint.TextAlignment = TextAlignment.Center;
            content.Children.Add(dropHint); content.Children.Add(paste); content.Children.Add(emptyNotice);
            emptyPanel.Padding = new Thickness(32, 48, 32, 48);
        }
        if (tool.Id == "remove")
        {
            content.Children.Clear();
            content.Children.Add(new TextBlock { Text = "Hapus halaman PDF", FontSize = 30, TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap });
            var hint = Copy("Pilih halaman yang tidak diperlukan, lalu simpan PDF baru tanpa halaman tersebut.", 17);
            hint.TextAlignment = TextAlignment.Center; content.Children.Add(hint);
            pick.Content = "PILIH FILE PDF"; pick.MinHeight = 68; pick.FontSize = 23; pick.MinWidth = 0;
            pick.Background = Brush(230, 44, 48); pick.Foreground = Brush(255, 255, 255);
            content.Children.Add(pick); content.Children.Add(Copy("atau jatuhkan satu PDF di sini", 14)); content.Children.Add(paste); content.Children.Add(emptyNotice);
            emptyPanel.Padding = new Thickness(32, 48, 32, 48);
        }
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
