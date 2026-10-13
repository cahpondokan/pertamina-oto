using Microsoft.UI.Xaml.Controls;
using Windows.Data.Pdf;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private async Task OpenMergeOrganizerAsync()
    {
        if (active is not null || importing || messageOpen || files.Count == 0) return;
        if (pageLayout.Count > 0)
        {
            messageOpen = true;
            try
            {
                if (await ShowOrganizerTransferConfirmationAsync() != ContentDialogResult.Primary) return;
            }
            finally { messageOpen = false; }
        }
        var inputs = files.ToArray();
        var prepared = new List<OrganizerPage>();
        importing = true;
        try
        {
            long bytes = 0;
            for (var i = 0; i < inputs.Length; i++)
            {
                bytes += new FileInfo(inputs[i]).Length;
                if (bytes > 100 * 1024 * 1024) throw new InvalidOperationException();
                var file = await StorageFile.GetFileFromPathAsync(inputs[i]);
                var pdf = password.Password.Length == 0 ? await PdfDocument.LoadFromFileAsync(file) : await PdfDocument.LoadFromFileAsync(file, password.Password);
                if (prepared.Count + pdf.PageCount > 1000) throw new InvalidOperationException();
                for (var page = 0; page < pdf.PageCount; page++) prepared.Add(OrganizerPage.Create(i, page, mergeAngles.GetValueOrDefault(inputs[i])));
            }
            SaveToolDraft();
            pageLayout.Clear(); pageLayout.AddRange(prepared);
            organizerSources.Clear(); organizerSources.AddRange(inputs);
            organizerThumbnails.Clear(); layoutUndo.Clear(); layoutRedo.Clear();
            chosenFiles.Clear();
            foreach (var path in inputs) { if (!homeFiles.Contains(path)) homeFiles.Add(path); chosenFiles.Add(path); }
            importing = false;
            ShowOrganizer();
        }
        catch (Exception error)
        {
            LocalDiagnostics.Record("merge-organizer-transfer", error);
            status.Text = "Dokumen belum dapat dibuka di Atur halaman. Periksa kata sandi dan batas 100 MB / 1000 halaman. Daftar Gabung PDF tetap tersedia.";
        }
        finally { importing = false; }
    }
}
