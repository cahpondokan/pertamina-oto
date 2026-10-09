using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pertamina.Oto.LocalPdf;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private async Task ExecuteOrganizerAsync()
    {
        if (active is not null || selected?.Id != "organize" || importing) return;
        if (pageLayout.Count == 0 || organizerSources.Count == 0) { status.Text = "Tambahkan PDF dan minimal satu halaman hasil."; return; }
        if (folder is null) { status.Text = "Pilih folder hasil terlebih dahulu."; return; }
        UpdateAutomaticOutputName();
        var name = outputName.Text.Trim();
        if (name.Length == 0 || name != Path.GetFileName(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || !name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        { status.Text = "Masukkan nama file hasil dengan akhiran .pdf."; return; }
        var target = AvailableOutputPath(folder, name);
        // Snapshot the draft; WorkerClient protects sources and publishes only validated output.
        CancelOrganizerDrag();
        var usedSources = pageLayout.Where(p => p.Source >= 0).Select(p => p.Source).Distinct().ToArray();
        if (usedSources.Length == 0) usedSources = [0];
        var request = new PdfRequest("organize", usedSources.Select(i => organizerSources[i]).ToArray(), target, Password: password.Password,
            Layout: pageLayout.Select(p => new PagePlacement(p.Source < 0 ? p.Source : Array.IndexOf(usedSources, p.Source), p.Page, p.Rotation)).ToArray());
        var editor = body.Children.ToArray();
        void RestoreEditor() { organizerVisible = true; body.Children.Clear(); foreach (var item in editor) body.Children.Add(item); }
        using var cancellation = new CancellationTokenSource(); active = cancellation;
        foreach (var item in navigation) item.IsEnabled = false; globalSearch.IsEnabled = false;
        organizerVisible = false; body.Children.Clear();
        var processing = new StackPanel { Spacing = 20, Padding = new Thickness(24) };
        processing.Children.Add(Copy("Menyimpan susunan halaman…", 26));
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
            RestoreEditor(); RefreshPageGrid();
        }
        if (reply is not { Success: true }) { if (reply is not null) status.Text = reply.Message; return; }
        organizerOutput = outputName.Text; restartSavedLayout = RestartLayoutState();
        resultPath = target; status.Text = "PDF hasil tersimpan.";
        history.Insert(0, "Atur halaman — " + name + " — selesai lokal"); completedActivities.Insert(0, (selected, name));
        organizerVisible = false; body.Children.Clear();
        ShowUnifiedResult([target], $"{reply.Pages} halaman · {reply.Bytes / 1024d:N1} KB", () => { RestoreEditor(); RefreshPageGrid(); });
    }
}
