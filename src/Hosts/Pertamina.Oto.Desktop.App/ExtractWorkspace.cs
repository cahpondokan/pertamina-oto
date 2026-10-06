using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pertamina.Oto.LocalPdf;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private readonly List<int> extractSelection = [];
    private readonly List<(ToggleButton Button, Image Image, TextBlock Mark)> extractTiles = [];
    private TextBox extractPages = new();
    private TextBlock extractSummary = new();
    private Button extractRun = new();
    private string? extractSource;
    private int extractCount;
    private int extractGeneration;
    private bool extractUpdating;
    private bool extractLoading;

    private void BuildExtractWorkspace(StackPanel inputArea)
    {
        inputArea.Children.Clear();
        if (run.Parent is Panel actions) actions.Children.Clear();
        toolOptions.Children.Clear();
        extractSource = files.FirstOrDefault(); extractCount = 0; extractSelection.Clear(); extractTiles.Clear();
        var toolbar = new ResponsiveWrapPanel { Spacing = 8 };
        var pick = new Button { Content = "PILIH FILE" };
        pick.Click += async (_, _) => await PickExtractFileAsync(); toolbar.Children.Add(pick);
        var paste = new Button { Content = "Tempel file" };
        paste.Click += async (_, _) => await PasteExtractFileAsync(); toolbar.Children.Add(paste);
        inputArea.Children.Add(toolbar);
        inputArea.Children.Add(Copy("Klik halaman sesuai urutan hasil yang diinginkan. Klik lagi untuk membatalkan pilihan. Atau tulis urutan/rentang di pengaturan. Jatuhkan satu PDF di sini untuk mengganti sumber.", 14));
        sourcePreviewHost = new StackPanel { Spacing = 10 };
        inputArea.Children.Add(sourcePreviewHost);
        sourcePreviewHost.Loaded += async (_, _) =>
        {
            if (extractCount > 0 && string.Equals(extractSource, files.FirstOrDefault(), StringComparison.OrdinalIgnoreCase)) return;
            await RefreshExtractSourceAsync();
        };
        inputArea.AllowDrop = true;
        inputArea.DragOver += (_, e) => e.AcceptedOperation = active is null && e.DataView.Contains(StandardDataFormats.StorageItems) ? DataPackageOperation.Copy : DataPackageOperation.None;
        inputArea.Drop += async (_, e) =>
        {
            e.Handled = true; var deferral = e.GetDeferral();
            try { if (e.DataView.Contains(StandardDataFormats.StorageItems)) await AcceptExtractFileAsync(await e.DataView.GetStorageItemsAsync()); }
            catch (Exception) { status.Text = "File tidak dapat dibaca. Pilih satu PDF lokal."; }
            finally { deferral.Complete(); }
        };
        var shortcut = new KeyboardAccelerator { Key = VirtualKey.V, Modifiers = VirtualKeyModifiers.Control };
        shortcut.Invoked += async (_, e) =>
        {
            if (selected?.Id != "extract" || !inputArea.IsLoaded || active is not null || FocusManager.GetFocusedElement(root.XamlRoot) is TextBox or PasswordBox) return;
            e.Handled = true; await PasteExtractFileAsync();
        };
        // Ctrl+V is handled by the window root.
        extractSummary = Copy("Memuat halaman…", 14);
        toolOptions.Children.Add(new Border { Child = extractSummary, Padding = new Thickness(12), Background = Brush(225, 241, 255), CornerRadius = new CornerRadius(8) });
        extractPages = new TextBox { Header = "Halaman yang akan diambil", PlaceholderText = "Contoh: 3,1,4-6", Text = pages.Text, MaxLength = 5000 };
        extractPages.TextChanged += (_, _) => { if (!extractUpdating) ApplyExtractText(); };
        toolOptions.Children.Add(extractPages);
        var clear = new Button { Content = "Batalkan semua pilihan" };
        clear.Click += (_, _) => { extractSelection.Clear(); SyncExtractSelection(); }; toolOptions.Children.Add(clear);
        var locked = new StackPanel { Spacing = 8 }; locked.Children.Add(password);
        var reload = new Button { Content = "Muat ulang preview" };
        reload.Click += async (_, _) => await RefreshExtractSourceAsync(); locked.Children.Add(reload);
        toolOptions.Children.Add(new Expander { Header = "PDF dengan kata sandi", Content = locked, HorizontalAlignment = HorizontalAlignment.Stretch });
        toolOptions.Children.Add(outputName);
        var chooseFolder = new Button { Content = "PILIH FOLDER HASIL", HorizontalAlignment = HorizontalAlignment.Stretch };
        chooseFolder.Click += async (_, _) => await PickFolderAsync(); toolOptions.Children.Add(chooseFolder);
        toolOptions.Children.Add(BuildOutputLocation());
        extractRun = new Button { Content = "Ambil halaman", FontSize = 20, MinHeight = 60,
            Background = Brush(23, 104, 212), Foreground = Brush(255, 255, 255), CornerRadius = new CornerRadius(10), HorizontalAlignment = HorizontalAlignment.Stretch, IsEnabled = false };
        extractRun.Click += async (_, _) => await ExecuteExtractAsync();
        toolOptions.Children.Add(extractRun); toolOptions.Children.Add(status);
    }

    private async Task PickExtractFileAsync()
    {
        if (active is not null) return;
        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker(); picker.FileTypeFilter.Add(".pdf");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            var file = await picker.PickSingleFileAsync();
            if (file is not null) await AcceptExtractFileAsync([file]);
        }
        catch (Exception) { status.Text = "Pemilih file tidak dapat dibuka."; }
    }

    private async Task PasteExtractFileAsync()
    {
        if (active is not null) return;
        try
        {
            var data = Clipboard.GetContent();
            if (!data.Contains(StandardDataFormats.StorageItems)) { status.Text = "Salin file PDF dari File Explorer, bukan teks atau screenshot."; return; }
            await AcceptExtractFileAsync(await data.GetStorageItemsAsync());
        }
        catch (Exception) { status.Text = "Clipboard tidak dapat dibaca."; }
    }

    private async Task AcceptExtractFileAsync(IEnumerable<IStorageItem> items)
    {
        if (active is not null || selected?.Id != "extract") return;
        var incoming = items.Take(2).ToArray();
        if (incoming.Length != 1 || incoming[0] is not StorageFile file || !file.FileType.Equals(".pdf", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(file.Path) || file.Path.StartsWith(@"\\", StringComparison.Ordinal))
        { status.Text = "Pilih satu file PDF lokal."; return; }
        if ((await file.GetBasicPropertiesAsync()).Size > 100 * 1024 * 1024) { status.Text = "PDF dibatasi 100 MB."; return; }
        if (active is not null || selected?.Id != "extract") return;
        password.Password = "";
        files.Clear(); files.Add(file.Path);
    }

    private async Task RefreshExtractSourceAsync()
    {
        if (selected?.Id != "extract" || sourcePreviewHost?.IsLoaded != true || active is not null) return;
        var generation = ++extractGeneration; var host = sourcePreviewHost;
        var path = files.FirstOrDefault();
        var changed = !string.Equals(path, extractSource, StringComparison.OrdinalIgnoreCase);
        extractSource = path; extractCount = 0; extractLoading = true; extractTiles.Clear(); host.Children.Clear();
        if (changed) { extractSelection.Clear(); extractPages.Text = ""; pages.Text = ""; }
        extractRun.IsEnabled = false;
        extractSummary.Text = "Memuat halaman…";
        try
        {
            if (path is null) { extractSummary.Text = "Pilih satu PDF untuk memulai."; return; }
            var file = await StorageFile.GetFileFromPathAsync(path);
            if ((await file.GetBasicPropertiesAsync()).Size > 100 * 1024 * 1024) throw new InvalidOperationException();
            var previewPassword = password.Password;
            var pdf = await PdfDocument.LoadFromFileAsync(file, previewPassword);
            if (generation != extractGeneration || selected?.Id != "extract" || !host.IsLoaded) return;
            if (pdf.PageCount is 0 or > 1000) throw new InvalidOperationException();
            extractCount = (int)pdf.PageCount;
            host.Children.Add(Copy(Path.GetFileName(path), 14));
            var grid = new GridView { SelectionMode = ListViewSelectionMode.None, IsItemClickEnabled = false, MaxHeight = 650 };
            host.Children.Add(ZoomablePreview(grid));
            for (var i = 1; i <= extractCount; i++)
            {
                var number = i;
                var picture = new Image { Height = 205, Stretch = Stretch.Uniform };
                var info = Copy("Memuat preview…", 11);
                picture.Loaded += async (_, _) => await RenderPreviewAsync(path, (uint)(number - 1), picture, info, 240, previewPassword);
                var mark = new TextBlock { Text = "✓", FontSize = 76, Foreground = Brush(23, 104, 212), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Visibility = Visibility.Collapsed, IsHitTestVisible = false };
                var preview = new Grid(); preview.Children.Add(picture); preview.Children.Add(mark);
                var content = new StackPanel { Spacing = 6, Width = 170 }; content.Children.Add(preview); content.Children.Add(Copy("Halaman " + number, 13)); content.Children.Add(info);
                var tile = new ToggleButton { Content = content, Padding = new Thickness(10), Margin = new Thickness(4), BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(8), Background = Brush(255, 255, 255) };
                tile.Click += (_, _) => { if (active is not null || extractLoading) return; if (!extractSelection.Remove(number)) extractSelection.Add(number); SyncExtractSelection(); };
                extractTiles.Add((tile, picture, mark)); grid.Items.Add(tile);
            }
            extractLoading = false; ApplyExtractText();
        }
        catch (Exception)
        {
            if (generation != extractGeneration || selected?.Id != "extract") return;
            extractCount = 0; extractSummary.Text = "Preview tidak tersedia. PDF mungkin terkunci, rusak, atau melewati batas 100 MB / 1000 halaman. Isi kata sandi bila diperlukan lalu muat ulang.";
        }
        finally { if (generation == extractGeneration) extractLoading = false; }
    }

    private void ApplyExtractText()
    {
        if (extractLoading) return;
        extractSelection.Clear();
        pages.Text = extractPages.Text;
        if (!ExtractPageSelection.TryParse(extractPages.Text, extractCount, out var selection))
        {
            PaintExtractSelection(); extractRun.IsEnabled = false;
            extractSummary.Text = "Nomor halaman tidak valid. Gunakan nomor/rentang dalam dokumen, misalnya 3,1,4-6. Nomor tidak boleh berulang."; return;
        }
        extractSelection.AddRange(selection); PaintExtractSelection(); UpdateExtractSummary();
    }

    private void SyncExtractSelection()
    {
        extractUpdating = true;
        try { extractPages.Text = string.Join(",", extractSelection); pages.Text = extractPages.Text; }
        finally { extractUpdating = false; }
        PaintExtractSelection(); UpdateExtractSummary();
    }

    private void PaintExtractSelection()
    {
        for (var i = 0; i < extractTiles.Count; i++)
        {
            var chosen = extractSelection.Contains(i + 1); var tile = extractTiles[i];
            tile.Button.IsChecked = chosen; tile.Image.Opacity = chosen ? 1 : .65;
            tile.Mark.Visibility = chosen ? Visibility.Visible : Visibility.Collapsed;
            tile.Button.BorderBrush = chosen ? Brush(23, 104, 212) : Brush(223, 229, 238);
            AutomationProperties.SetName(tile.Button, $"Halaman {i + 1}, " + (chosen ? "akan diambil; klik untuk batal" : "tidak dipilih; klik untuk ambil"));
        }
    }

    private void UpdateExtractSummary()
    {
        extractSummary.Text = $"Sumber: {extractCount} halaman\nHasil: {extractSelection.Count} halaman\nUrutan: {string.Join(", ", extractSelection.Take(20))}{(extractSelection.Count > 20 ? "…" : "")}";
        if (extractSelection.Count == 0) extractSummary.Text += "\nPilih halaman yang akan diambil.";
        
        extractRun.IsEnabled = active is null && !extractLoading && extractSelection.Count > 0 && extractCount > 0;
    }

    private async Task ExecuteExtractAsync()
    {
        if (active is not null || selected?.Id != "extract" || extractLoading) return;
        ApplyExtractText(); if (!extractRun.IsEnabled || files.Count != 1) return;
        if (folder is null) { status.Text = "Pilih folder hasil terlebih dahulu."; return; }
        UpdateAutomaticOutputName();
        var name = outputName.Text.Trim();
        if (name.Length == 0 || name != Path.GetFileName(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || !name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        { status.Text = "Masukkan nama file hasil dengan akhiran .pdf."; return; }
        var target = AvailableOutputPath(folder, name);
        // Preserve the explicit output order; duplicates are rejected before execution.
        var request = new PdfRequest("extract", files.ToArray(), target, string.Join(",", extractSelection), Password: password.Password);
        var editor = body.Children.ToArray();
        void RestoreEditor() { body.Children.Clear(); foreach (var item in editor) body.Children.Add(item); }
        using var cancellation = new CancellationTokenSource(); active = cancellation;
        foreach (var item in navigation) item.IsEnabled = false; globalSearch.IsEnabled = false;
        body.Children.Clear();
        var processing = new StackPanel { Spacing = 20, Padding = new Thickness(24) };
        processing.Children.Add(Copy("Mengambil halaman…", 26));
        processing.Children.Add(Copy("Memproses dan memvalidasi PDF secara lokal."));
        processing.Children.Add(new ProgressBar { IsIndeterminate = true });
        var stop = new Button { Content = "BATALKAN" }; stop.Click += (_, _) => cancellation.Cancel(); processing.Children.Add(stop); body.Children.Add(processing);
        PdfReply? reply = null;
        try { reply = await WorkerClient.RunAsync(request, cancellation.Token); }
        catch (Exception) { status.Text = "Proses gagal. File asli tidak diubah."; }
        finally
        {
            active = null; password.Password = "";
            foreach (var item in navigation) item.IsEnabled = true; globalSearch.IsEnabled = true;
            RestoreEditor(); UpdateExtractSummary();
        }
        if (reply is not { Success: true }) { if (reply is not null) status.Text = reply.Message; return; }
        MarkRestartToolSaved("extract");
        resultPath = target; status.Text = "PDF hasil tersimpan.";
        history.Insert(0, "Ambil halaman — " + name + " — selesai lokal"); completedActivities.Insert(0, (selected, name));
        body.Children.Clear();
        ShowUnifiedResult([target], $"{reply.Pages} halaman · {reply.Bytes / 1024d:N1} KB", () => { RestoreEditor(); UpdateExtractSummary(); });
    }
}

internal static class ExtractPageSelection
{
    internal static bool TryParse(string text, int total, out int[] pages)
    {
        pages = []; var selected = new List<int>();
        if (string.IsNullOrWhiteSpace(text)) return true;
        foreach (var part in text.Split(','))
        {
            var bounds = part.Trim().Split('-');
            if (bounds.Length is < 1 or > 2 || !int.TryParse(bounds[0].Trim(), System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var first)) return false;
            var last = first;
            if (bounds.Length == 2 && !int.TryParse(bounds[1].Trim(), System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out last)) return false;
            if (first < 1 || last < first || last > total) return false;
            for (var page = first; page <= last; page++) { if (selected.Contains(page)) return false; selected.Add(page); }
        }
        pages = selected.ToArray(); return true;
    }
}
