using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private StackPanel? sourcePreviewHost;
    private ComboBox? sourceChoice;
    private string? previewSource;
    private bool updatingSources;

    private void BuildSourceWorkspace(StackPanel inputArea)
    {
        if (selected?.Id == "word-pdf") { BuildWordWorkspace(inputArea); return; }
        if (selected?.Id == "extract") { BuildExtractWorkspace(inputArea); return; }
        if (selected?.Id == "remove") { BuildRemoveWorkspace(inputArea); return; }
        if (selected?.Id == "split") { BuildSplitWorkspace(inputArea); return; }
        if (selected?.Id == "pdf-jpg") { BuildSimpleJpg(inputArea); return; }
        inputArea.Children.Clear();
        var toolbar = new ResponsiveWrapPanel { Spacing = 8 };
        toolbar.Children.Add(ActionButton("＋ Tambah file", async () => await PickFilesAsync()));
        toolbar.Children.Add(ActionButton("Tempel file", async () => await PasteSourceAsync()));
        inputArea.Children.Add(toolbar);
        sourceChoice = new ComboBox { Header = "Dokumen sumber", HorizontalAlignment = HorizontalAlignment.Stretch };
        editable.Add(sourceChoice); inputArea.Children.Add(sourceChoice);
        sourceChoice.SelectionChanged += (_, _) =>
        {
            if (updatingSources || sourceChoice.SelectedIndex < 0 || sourceChoice.SelectedIndex >= files.Count) return;
            previewSource = files[sourceChoice.SelectedIndex]; RefreshSourcePreview();
        };
        var fileActions = new ResponsiveWrapPanel { Spacing = 8 };
        if (selected?.Id == "images")
        {
            foreach (var delta in new[] { -1, 1 })
                fileActions.Children.Add(ActionButton(delta < 0 ? "← Sebelumnya" : "Berikutnya →", () =>
                {
                    var index = sourceChoice.SelectedIndex; var next = index + delta;
                    if (index >= 0 && next >= 0 && next < files.Count) files.Move(index, next);
                }));
        }
        fileActions.Children.Add(ActionButton("Keluarkan dari daftar", () => { var i = sourceChoice.SelectedIndex; if (i >= 0 && i < files.Count) RemoveToolInput(files[i]); }));
        fileActions.Children.Add(ActionButton("Muat ulang preview", RefreshSourcePreview));
        inputArea.Children.Add(fileActions);
        inputArea.Children.Add(Copy("Pratinjau sumber — belum menerapkan pengaturan. Periksa Hasil tersimpan setelah proses.", 12));
        sourcePreviewHost = new StackPanel { Spacing = 10 };
        editable.Add(sourcePreviewHost); inputArea.Children.Add(sourcePreviewHost);
        sourcePreviewHost.Loaded += (_, _) => RefreshSourcePreview();
        RefreshSourcePreview();
    }

    private void RefreshSourcePreview()
    {
        if (selected?.Id == "word-pdf") { RefreshWordSource(); return; }
        if (selected?.Id == "extract") { _ = RefreshExtractSourceAsync(); return; }
        if (selected?.Id == "remove") { _ = RefreshRemoveSourceAsync(); return; }
        if (selected?.Id == "split") { _ = RefreshSplitSourceAsync(); return; }
        if (selected?.Id == "pdf-jpg") { RefreshJpgCard(); return; }
        if (sourcePreviewHost is null || sourceChoice is null || selected?.Id is "merge" or "organize" || updatingSources) return;
        updatingSources = true;
        try
        {
            sourcePreviewHost.Children.Clear(); sourceChoice.Items.Clear();
            foreach (var path in files) sourceChoice.Items.Add(Path.GetFileName(path));
            if (previewSource is null || !files.Contains(previewSource)) previewSource = files.FirstOrDefault();
            sourceChoice.SelectedIndex = previewSource is null ? -1 : files.IndexOf(previewSource);
            if (previewSource is null) { sourcePreviewHost.Children.Add(Copy("Tambahkan dokumen untuk melihat pratinjau.")); return; }
            sourcePreviewHost.Children.Add(PreviewPanel(previewSource, previewPassword: password.Password));
            if (selected?.Id == "images") sourcePreviewHost.Children.Add(Copy("Pratinjau gambar asli. Hasil PDF memakai A4, satu gambar per halaman.", 12));
            if (selected?.Id is "unlock" or "protect") sourcePreviewHost.Children.Add(Copy("Jika terkunci, isi kata sandi pemilik yang sah, lalu klik Muat ulang preview.", 12));
        }
        finally { updatingSources = false; }
    }

    private async Task PasteSourceAsync()
    {
        if (active is not null || selected is not { Available: true }) return;
        if (selected.Id == "word-pdf") { await PasteWordAsync(); return; }
        try
        {
            var data = Clipboard.GetContent();
            if (!data.Contains(StandardDataFormats.StorageItems)) { status.Text = "Salin file dari File Explorer, bukan teks atau screenshot."; return; }
            var images = selected.Id == "images";
            var incoming = (await data.GetStorageItemsAsync()).OfType<StorageFile>().Where(f =>
                !string.IsNullOrEmpty(f.Path) && !f.Path.StartsWith(@"\\", StringComparison.Ordinal) &&
                (images ? new[] { ".jpg", ".jpeg", ".png" }.Contains(f.FileType.ToLowerInvariant()) : f.FileType.Equals(".pdf", StringComparison.OrdinalIgnoreCase))).Take(images ? 50 : 1).ToArray();
            if (incoming.Length == 0) { status.Text = "Tidak ada file dengan format yang sesuai."; return; }
            if (!images) files.Clear();
            foreach (var file in incoming) { if (files.Count >= 50) break; if (!files.Contains(file.Path)) files.Add(file.Path); }
        }
        catch (Exception) { status.Text = "Clipboard tidak dapat dibaca."; }
    }

    private void AddSavedResultPreview(string path)
    {
        var actions = new StackPanel { Spacing = 8 };
        var openFolder = new Button { Content = "Buka folder hasil" };
        openFolder.Click += async (_, _) =>
        {
            try { await Windows.System.Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path)!)); }
            catch (Exception) { status.Text = "Folder tidak dapat dibuka. Gunakan lokasi simpan yang Anda pilih."; }
        };
        actions.Children.Add(openFolder);
        if (Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(path).Equals(".jpg", StringComparison.OrdinalIgnoreCase))
        {
            var view = new Button { Content = "Pratinjau hasil tersimpan" };
            view.Click += async (_, _) =>
            {
                if (messageOpen) return;
                messageOpen = true;
                try
                {
                    var panel = new StackPanel { Spacing = 8 };
                    var resultPassword = new PasswordBox { Header = "Kata sandi hasil (jika terenkripsi)", MaxLength = 127 };
                    var preview = new StackPanel();
                    preview.Children.Add(PreviewPanel(path));
                    var reload = new Button { Content = "Buka pratinjau dengan kata sandi" };
                    reload.Click += (_, _) => { preview.Children.Clear(); preview.Children.Add(PreviewPanel(path, previewPassword: resultPassword.Password)); resultPassword.Password = ""; };
                    panel.Children.Add(resultPassword); panel.Children.Add(reload); panel.Children.Add(preview);
                    var dialog = new ContentDialog { XamlRoot = root.XamlRoot, Title = "Hasil tersimpan", CloseButtonText = "Tutup",
                        Content = new ScrollViewer { Content = panel, MaxHeight = 600 } };
                    await dialog.ShowAsync();
                }
                catch (Exception) { status.Text = "Pratinjau hasil tidak dapat dibuka. File tetap tersimpan di folder hasil."; }
                finally { messageOpen = false; }
            };
            actions.Children.Add(view);
        }
        else actions.Children.Add(Copy("Hasil berupa ZIP. Ekstrak untuk memeriksa setiap file hasil.", 12));
        body.Children.Add(Section("Hasil tersimpan", Path.GetFileName(path), actions));
    }
}
