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
    private readonly HashSet<int> removeSelection = [];
    private readonly List<(ToggleButton Button, Image Image, TextBlock Mark, Border Tint)> removeTiles = [];
    private TextBox removePages = new();
    private TextBlock removeSummary = new();
    private Button removeRun = new();
    private string? removeSource;
    private int removeCount;
    private int removeGeneration;
    private bool removeUpdating;
    private bool removeLoading;

    private void BuildRemoveWorkspace(StackPanel inputArea)
    {
        inputArea.Children.Clear();
        if (run.Parent is Panel actions) actions.Children.Clear();
        toolOptions.Children.Clear();
        removeSource = files.FirstOrDefault(); removeCount = 0; removeSelection.Clear(); removeTiles.Clear();
        var toolbar = new ResponsiveWrapPanel { Spacing = 8 };
        var pick = new Button { Content = "Ganti PDF" };
        pick.Click += async (_, _) => await PickRemoveFileAsync(); toolbar.Children.Add(pick);
        var paste = new Button { Content = "Tempel file" };
        paste.Click += async (_, _) => await PasteRemoveFileAsync(); toolbar.Children.Add(paste);
        inputArea.Children.Add(toolbar);
        inputArea.Children.Add(Copy("Klik halaman yang akan dihapus. Klik lagi untuk membatalkan pilihan. Anda juga dapat menjatuhkan satu PDF di sini.", 14));
        sourcePreviewHost = new StackPanel { Spacing = 10 };
        inputArea.Children.Add(sourcePreviewHost);
        sourcePreviewHost.Loaded += async (_, _) =>
        {
            if (removeCount > 0 && string.Equals(removeSource, files.FirstOrDefault(), StringComparison.OrdinalIgnoreCase)) return;
            await RefreshRemoveSourceAsync();
        };
        inputArea.AllowDrop = true;
        inputArea.DragOver += (_, e) => e.AcceptedOperation = active is null && e.DataView.Contains(StandardDataFormats.StorageItems) ? DataPackageOperation.Copy : DataPackageOperation.None;
        inputArea.Drop += async (_, e) =>
        {
            e.Handled = true; var deferral = e.GetDeferral();
            try { if (e.DataView.Contains(StandardDataFormats.StorageItems)) await AcceptRemoveFileAsync(await e.DataView.GetStorageItemsAsync()); }
            catch (Exception) { status.Text = "File tidak dapat dibaca. Pilih satu PDF lokal."; }
            finally { deferral.Complete(); }
        };
        var shortcut = new KeyboardAccelerator { Key = VirtualKey.V, Modifiers = VirtualKeyModifiers.Control };
        shortcut.Invoked += async (_, e) =>
        {
            if (selected?.Id != "remove" || !inputArea.IsLoaded || active is not null || FocusManager.GetFocusedElement(root.XamlRoot) is TextBox or PasswordBox) return;
            e.Handled = true; await PasteRemoveFileAsync();
        };
        // Ctrl+V is handled by the window root.
        removeSummary = Copy("Memuat halaman…", 14);
        toolOptions.Children.Add(new Border { Child = removeSummary, Padding = new Thickness(12), Background = Brush(225, 241, 255), CornerRadius = new CornerRadius(8) });
        removePages = new TextBox { Header = "Halaman yang akan dihapus", PlaceholderText = "Contoh: 2,4-6", Text = pages.Text, MaxLength = 5000 };
        removePages.TextChanged += (_, _) => { if (!removeUpdating) ApplyRemoveText(); };
        toolOptions.Children.Add(new Expander { Header = "Pilih dengan nomor halaman", Content = removePages, HorizontalAlignment = HorizontalAlignment.Stretch });
        var clear = new Button { Content = "Batalkan semua pilihan" };
        clear.Click += (_, _) => { removeSelection.Clear(); SyncRemoveSelection(); }; toolOptions.Children.Add(clear);
        var locked = new StackPanel { Spacing = 8 }; locked.Children.Add(password);
        var reload = new Button { Content = "Muat ulang preview" };
        reload.Click += async (_, _) => await RefreshRemoveSourceAsync(); locked.Children.Add(reload);
        toolOptions.Children.Add(new Expander { Header = "PDF dengan kata sandi", Content = locked, HorizontalAlignment = HorizontalAlignment.Stretch });
        toolOptions.Children.Add(outputName);
        toolOptions.Children.Add(BuildOutputLocation());
        removeRun = new Button { Content = "Pilih halaman untuk dihapus", FontSize = 16, MinHeight = 48,
            Background = Brush(230, 44, 48), Foreground = Brush(255, 255, 255), CornerRadius = new CornerRadius(10), HorizontalAlignment = HorizontalAlignment.Stretch, IsEnabled = false };
        removeRun.Click += async (_, _) => await ExecuteRemoveAsync();
        toolOptions.Children.Add(removeRun); toolOptions.Children.Add(status);
        toolOptions.Children.Add(Copy("Disimpan sebagai PDF baru. File asli tetap utuh.", 12));
    }

    private async Task PickRemoveFileAsync()
    {
        if (active is not null) return;
        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker(); picker.FileTypeFilter.Add(".pdf");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            var file = await picker.PickSingleFileAsync();
            if (file is not null) await AcceptRemoveFileAsync([file]);
        }
        catch (Exception) { status.Text = "Pemilih file tidak dapat dibuka."; }
    }

    private async Task PasteRemoveFileAsync()
    {
        if (active is not null) return;
        try
        {
            var data = Clipboard.GetContent();
            if (!data.Contains(StandardDataFormats.StorageItems)) { status.Text = "Salin file PDF dari File Explorer, bukan teks atau screenshot."; return; }
            await AcceptRemoveFileAsync(await data.GetStorageItemsAsync());
        }
        catch (Exception) { status.Text = "Clipboard tidak dapat dibaca."; }
    }

    private async Task AcceptRemoveFileAsync(IEnumerable<IStorageItem> items)
    {
        if (active is not null || selected?.Id != "remove") return;
        var incoming = items.Take(2).ToArray();
        if (incoming.Length != 1 || incoming[0] is not StorageFile file || !file.FileType.Equals(".pdf", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(file.Path) || file.Path.StartsWith(@"\\", StringComparison.Ordinal))
        { status.Text = "Pilih satu file PDF lokal."; return; }
        if ((await file.GetBasicPropertiesAsync()).Size > 100 * 1024 * 1024) { status.Text = "PDF dibatasi 100 MB."; return; }
        if (active is not null || selected?.Id != "remove") return;
        password.Password = "";
        files.Clear(); files.Add(file.Path);
    }

    private async Task RefreshRemoveSourceAsync()
    {
        if (selected?.Id != "remove" || sourcePreviewHost?.IsLoaded != true || active is not null) return;
        var generation = ++removeGeneration; var host = sourcePreviewHost;
        var path = files.FirstOrDefault();
        var changed = !string.Equals(path, removeSource, StringComparison.OrdinalIgnoreCase);
        removeSource = path; removeCount = 0; removeLoading = true; removeTiles.Clear(); host.Children.Clear();
        if (changed) { removeSelection.Clear(); removePages.Text = ""; pages.Text = ""; }
        removeRun.IsEnabled = false;
        removeSummary.Text = "Memuat halaman…";
        try
        {
            if (path is null) { removeSummary.Text = "Pilih satu PDF untuk memulai."; return; }
            var (file, properties) = await OpenPreviewStorageAsync(path);
            if (properties.Size > 100 * 1024 * 1024) throw new InvalidOperationException();
            var previewPassword = password.Password;
            var pdf = await PdfDocument.LoadFromFileAsync(file, previewPassword);
            if (generation != removeGeneration || selected?.Id != "remove" || !host.IsLoaded) return;
            if (pdf.PageCount is 0 or > 1000) throw new InvalidOperationException();
            removeCount = (int)pdf.PageCount;
            host.Children.Add(Copy(Path.GetFileName(path), 14));
            var grid = new GridView { SelectionMode = ListViewSelectionMode.None, IsItemClickEnabled = false, MaxHeight = 650 };
            host.Children.Add(ZoomablePreview(grid));
            for (var i = 1; i <= removeCount; i++)
            {
                var number = i;
                var picture = new Image { Height = 205, Stretch = Stretch.Uniform };
                var info = Copy("Memuat preview…", 11);
                var rendered = false;
                picture.Loaded += async (_, _) => { if (rendered) return; rendered = true; await RenderPreviewAsync(path, (uint)(number - 1), picture, info, 400, previewPassword); };
                var mark = new TextBlock { Text = "\uE74D", FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 20, Margin = new Thickness(6), Foreground = Brush(210, 35, 42), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top, Visibility = Visibility.Collapsed, IsHitTestVisible = false };
                var tint = new Border { Background = Brush(230, 44, 48), Opacity = .16, Visibility = Visibility.Collapsed, IsHitTestVisible = false };
                var preview = new Grid(); preview.Children.Add(picture); preview.Children.Add(tint); preview.Children.Add(mark);
                var content = new StackPanel { Spacing = 8, Width = 170 }; content.Children.Add(preview);
                var numberLabel = Copy(number.ToString(System.Globalization.CultureInfo.InvariantCulture), 13); numberLabel.HorizontalAlignment = HorizontalAlignment.Center; content.Children.Add(numberLabel);
                var tile = new ToggleButton { Content = content, Padding = new Thickness(10), Margin = new Thickness(4), BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(8), Background = Brush(255, 255, 255) };
                tile.Click += (_, _) => { if (active is not null || removeLoading) return; if (!removeSelection.Add(number)) removeSelection.Remove(number); SyncRemoveSelection(); };
                ToolTipService.SetToolTip(tile, $"Halaman {number} · Klik untuk tandai / batalkan hapus");
                void Reveal(bool show) => mark.Visibility = show || removeSelection.Contains(number) ? Visibility.Visible : Visibility.Collapsed;
                tile.PointerEntered += (_, _) => Reveal(true);
                tile.PointerExited += (_, _) => Reveal(tile.FocusState != FocusState.Unfocused);
                tile.GotFocus += (_, _) => Reveal(true);
                tile.LostFocus += (_, _) => Reveal(false);
                removeTiles.Add((tile, picture, mark, tint)); grid.Items.Add(tile);
            }
            removeLoading = false; ApplyRemoveText();
        }
        catch (Exception)
        {
            if (generation != removeGeneration || selected?.Id != "remove") return;
            removeCount = 0; removeSummary.Text = "Preview tidak tersedia. PDF mungkin terkunci, rusak, atau melewati batas 100 MB / 1000 halaman. Isi kata sandi bila diperlukan lalu muat ulang.";
        }
        finally { if (generation == removeGeneration) removeLoading = false; }
    }

    private void ApplyRemoveText()
    {
        if (removeLoading) return;
        removeSelection.Clear();
        pages.Text = removePages.Text;
        if (!RemovePageSelection.TryParse(removePages.Text, removeCount, out var selection))
        {
            PaintRemoveSelection(); removeRun.IsEnabled = false;
            removeSummary.Text = "Nomor halaman tidak valid. Gunakan nomor/rentang dalam dokumen, misalnya 2,4-6."; return;
        }
        removeSelection.UnionWith(selection); PaintRemoveSelection(); UpdateRemoveSummary();
    }

    private void SyncRemoveSelection()
    {
        removeUpdating = true;
        try { removePages.Text = string.Join(",", removeSelection.Order()); pages.Text = removePages.Text; }
        finally { removeUpdating = false; }
        PaintRemoveSelection(); UpdateRemoveSummary();
    }

    private void PaintRemoveSelection()
    {
        for (var i = 0; i < removeTiles.Count; i++)
        {
            var chosen = removeSelection.Contains(i + 1); var tile = removeTiles[i];
            tile.Button.IsChecked = chosen; tile.Image.Opacity = chosen ? .75 : 1;
            tile.Tint.Visibility = chosen ? Visibility.Visible : Visibility.Collapsed;
            tile.Mark.Visibility = chosen ? Visibility.Visible : Visibility.Collapsed;
            tile.Button.BorderBrush = chosen ? Brush(230, 44, 48) : Brush(223, 229, 238);
            AutomationProperties.SetName(tile.Button, $"Halaman {i + 1}, " + (chosen ? "akan dihapus; klik untuk batal" : "dipertahankan; klik untuk hapus"));
        }
    }

    private void UpdateRemoveSummary()
    {
        removeSummary.Text = $"{removeCount} halaman · {removeSelection.Count} akan dihapus · {removeCount - removeSelection.Count} tersisa";
        if (removeSelection.Count == 0) removeSummary.Text += "\nPilih halaman yang akan dihapus.";
        else if (removeSelection.Count == removeCount) removeSummary.Text += "\nSisakan minimal satu halaman.";
        removeRun.Content = removeSelection.Count == 0 ? "Pilih halaman untuk dihapus" : $"Hapus {removeSelection.Count} halaman & simpan";
        removeRun.IsEnabled = active is null && !removeLoading && removeSelection.Count > 0 && removeSelection.Count < removeCount;
    }

    private async Task ExecuteRemoveAsync()
    {
        if (active is not null || selected?.Id != "remove" || removeLoading) return;
        ApplyRemoveText(); if (!removeRun.IsEnabled || files.Count != 1) return;
        if (folder is null) { status.Text = "Pilih folder hasil terlebih dahulu."; return; }
        UpdateAutomaticOutputName();
        var name = outputName.Text.Trim();
        if (name.Length == 0 || name != Path.GetFileName(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || !name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        { status.Text = "Masukkan nama file hasil dengan akhiran .pdf."; return; }
        var target = AvailableOutputPath(folder, name);
        // Submit normalized, unique page numbers: the worker intentionally rejects duplicates.
        var request = new PdfRequest("remove", files.ToArray(), target, string.Join(",", removeSelection.Order()), Password: password.Password);
        var editor = body.Children.ToArray();
        void RestoreEditor() { body.Children.Clear(); foreach (var item in editor) body.Children.Add(item); }
        using var cancellation = new CancellationTokenSource(); active = cancellation;
        foreach (var item in navigation) item.IsEnabled = false; globalSearch.IsEnabled = false;
        body.Children.Clear();
        var processing = new StackPanel { Spacing = 20, Padding = new Thickness(24) };
        processing.Children.Add(Copy("Menghapus halaman…", 26));
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
            RestoreEditor(); UpdateRemoveSummary();
        }
        if (reply is not { Success: true }) { if (reply is not null) status.Text = reply.Message; return; }
        MarkRestartToolSaved("remove");
        resultPath = target; status.Text = "PDF hasil tersimpan.";
        history.Insert(0, "Hapus halaman — " + name + " — selesai lokal"); completedActivities.Insert(0, (selected, name));
        body.Children.Clear();
        ShowUnifiedResult([target], $"{reply.Pages} halaman · {reply.Bytes / 1024d:N1} KB", () => { RestoreEditor(); UpdateRemoveSummary(); });
    }
}

internal static class RemovePageSelection
{
    internal static bool TryParse(string text, int total, out int[] pages)
    {
        pages = []; var selected = new SortedSet<int>();
        if (string.IsNullOrWhiteSpace(text)) return true;
        foreach (var part in text.Split(','))
        {
            var bounds = part.Trim().Split('-');
            if (bounds.Length is < 1 or > 2 || !int.TryParse(bounds[0], out var first)) return false;
            var last = first;
            if (bounds.Length == 2 && !int.TryParse(bounds[1], out last)) return false;
            if (first < 1 || last < first || last > total) return false;
            for (var page = first; page <= last; page++) selected.Add(page);
        }
        pages = selected.ToArray(); return true;
    }
}
