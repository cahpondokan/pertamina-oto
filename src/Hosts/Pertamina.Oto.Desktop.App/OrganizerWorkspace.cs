using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
#if DEBUG
    internal async Task OpenOrganizerFilesAsync(IEnumerable<string> paths)
    {
        openingEmptyInput = true;
        try { ShowOrganizer(); }
        finally { openingEmptyInput = false; }
        var inputs = new List<IStorageItem>();
        foreach (var path in paths.Take(50))
        {
            try { inputs.Add(await StorageFile.GetFileFromPathAsync(Path.GetFullPath(path))); }
            catch (Exception) { status.Text = "File contoh tidak tersedia."; }
        }
        await AcceptOrganizerFilesAsync(inputs);
    }
#endif

    private static SolidColorBrush OrganizerColor(int source)
    {
        if (source < 0) return Brush(100, 116, 139);
        if (source == 0) return Brush(204, 78, 92);
        if (source == 1) return Brush(0, 137, 157);
        // Golden-angle hue spacing, plus explicit source labels: color is never the only cue.
        var hue = source * 137.508 % 360 / 60;
        var x = .62 * (1 - Math.Abs(hue % 2 - 1));
        var (r, g, b) = hue switch
        {
            < 1 => (.62, x, 0d), < 2 => (x, .62, 0d), < 3 => (0d, .62, x),
            < 4 => (0d, x, .62), < 5 => (x, 0d, .62), _ => (.62, 0d, x)
        };
        return Brush((byte)((r + .18) * 255), (byte)((g + .18) * 255), (byte)((b + .18) * 255));
    }

    private async Task PickOrganizerAsync()
    {
        if (active is not null || importing) return;
        try
        {
            var picker = new FileOpenPicker(); picker.FileTypeFilter.Add(".pdf");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            await AcceptOrganizerFilesAsync(await picker.PickMultipleFilesAsync());
        }
        catch (Exception) { status.Text = "Pemilih file tidak dapat dibuka."; }
    }

    private async Task PasteOrganizerAsync()
    {
        if (active is not null || importing) return;
        try
        {
            var data = Clipboard.GetContent();
            if (!data.Contains(StandardDataFormats.StorageItems)) { status.Text = "Salin PDF dari File Explorer, lalu tekan Ctrl+V."; return; }
            await AcceptOrganizerFilesAsync(await data.GetStorageItemsAsync());
        }
        catch (Exception) { status.Text = "Clipboard tidak dapat dibaca."; }
    }

    private async Task AcceptOrganizerFilesAsync(IEnumerable<IStorageItem> items)
    {
        if (active is not null || importing || !organizerVisible) return;
        CancelOrganizerDrag();
        importing = true; run.IsEnabled = false;
        var rejected = 0;
        try
        {
            long bytes = organizerSources.Sum(p => new FileInfo(p).Length);
            foreach (var item in items.Take(51))
            {
                if (!organizerVisible) return;
                if (item is not StorageFile file || string.IsNullOrEmpty(file.Path) || file.Path.StartsWith(@"\\", StringComparison.Ordinal)
                    || !file.FileType.Equals(".pdf", StringComparison.OrdinalIgnoreCase)) { rejected++; continue; }
                if (organizerSources.Contains(file.Path, StringComparer.OrdinalIgnoreCase)) continue;
                try
                {
                    var size = (long)(await file.GetBasicPropertiesAsync()).Size;
                    if (organizerSources.Count >= 50 || bytes + size > 100L * 1024 * 1024) { rejected++; continue; }
                    var pdf = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(file);
                    if (!organizerVisible) return;
                    if (pdf.PageCount == 0 || pageLayout.Count + pdf.PageCount > 1000) { rejected++; continue; }
                    SaveLayout(); var source = organizerSources.Count; organizerSources.Add(file.Path); bytes += size;
                    restartSourcePageCounts[file.Path] = (int)pdf.PageCount;
                    for (var i = 0; i < pdf.PageCount; i++) pageLayout.Add(OrganizerPage.Create(source, i));
                }
                catch (Exception) { rejected++; }
            }
            status.Text = rejected == 0 ? "PDF ditambahkan. Seret halaman untuk mengatur urutan." : $"{rejected} file ditolak: format, lokasi, terkunci/rusak, atau batas 50 file / 100 MB / 1000 halaman.";
        }
        catch (Exception) { status.Text = "Sumber tidak dapat dibaca. Periksa apakah file masih tersedia."; }
        finally { importing = false; if (organizerVisible) RefreshPageGrid(); }
    }


    private void InsertOrganizerBlank(int position)
    {
        if (active is not null || importing) return;
        CancelOrganizerDrag();
        if (pageLayout.Count >= 1000) { status.Text = "Maksimal 1000 halaman hasil."; return; }
        SaveLayout(); pageLayout.Insert(Math.Clamp(position, 0, pageLayout.Count), OrganizerPage.Create(-1, -1)); RefreshPageGrid();
        status.Text = "Halaman kosong A4 ditambahkan. Undo untuk membatalkan.";
    }
}
