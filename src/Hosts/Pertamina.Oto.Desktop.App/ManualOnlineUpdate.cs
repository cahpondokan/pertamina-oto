using Microsoft.UI.Xaml.Controls;
using Pertamina.Oto.Distribution;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
#if DEBUG
    private string? manualUpdateFixtureRoot;
    private string? manualUpdateFixtureBuild;
    private System.Net.Http.HttpClient? manualUpdateFixtureClient;
    private string? manualUpdateFixtureDataRoot;
    private Action<string>? manualUpdateFixtureStatus;
#endif
    private string ManualUpdateRoot =>
#if DEBUG
        manualUpdateFixtureRoot ??
#endif
        LocalAppUpdate.InstallRoot;
    private string ManualUpdateBuild =>
#if DEBUG
        manualUpdateFixtureBuild ??
#endif
        RunningBuildLabel();
    private System.Net.Http.HttpClient ManualUpdateHttp =>
#if DEBUG
        manualUpdateFixtureClient ??
#endif
        OnlineHttp;
    private async Task CheckManualOnlineUpdateAsync()
    {
        if (checkingUpdate || checkingOnlineUpdate || messageOpen) return;
        if (PortableProfile.IsPortable) { await OpenOnlineDownloadsAsync(); return; }
        if (RestartBusy) { await MenuMessageAsync("Pekerjaan sedang berjalan", "Selesaikan proses dokumen sebelum update."); return; }
        checkingUpdate = true;
        using var cancel = CancellationTokenSource.CreateLinkedTokenSource(onlineUpdateLifetime.Token);
        try
        {
            var status = Copy("Memeriksa versi di oto.ayani.id…", 14);
            var progress = new ContentDialog { XamlRoot = root.XamlRoot, Title = "Update aplikasi", Content = status, CloseButtonText = "Batalkan" };
            progress.CloseButtonClick += (_, _) => cancel.Cancel();
            messageOpen = true; var shown = progress.ShowAsync();
            ReleaseInfo? release;
            DistributionRelease? baseline = null;
            try
            {
#if DEBUG
                if (manualUpdateFixtureClient is not null) release = await OnlineReleaseClient.FetchAsync(ManualUpdateHttp, cancel.Token);
                else
#endif
                { baseline = await OnlineReleaseClient.FetchBaselineAsync(ManualUpdateHttp, cancel.Token); release = baseline?.ToReleaseInfo(); }
            }
            finally { progress.Hide(); await shown; messageOpen = false; }
            if (cancel.IsCancellationRequested) return;
            if (!ReleaseInfo.ValidVersion(ManualUpdateBuild)) { await MenuMessageAsync("Instalasi diperlukan", "Jalankan versi terpasang dari shortcut PERTAMINA OTO untuk menggunakan update online."); return; }
            if (release is null || !release.IsNewerThan(LocalAppUpdate.VersionAt(ManualUpdateRoot)))
            {
                var current = LocalAppUpdate.VersionAt(ManualUpdateRoot);
                if (current != ManualUpdateBuild) await ShowUpdateCompletionAsync(current);
                else await MenuMessageAsync(release is null ? "Rilis belum tersedia" : "Sudah terbaru", release is null ? "Website belum menyediakan metadata rilis. Status versi terbaru belum dapat dipastikan." : "Versi terbaru yang tersedia sudah terpasang.");
                return;
            }
            var notes = ReleaseNotes.Online(release);
            var next = new ApplicationVersion.Identity(release.ProductVersion, "Pengembangan", release.Version);
            if (!await ConfirmNotesUpdateAsync("Update tersedia", next, notes, "Unduh dan pasang",
                ApplicationVersion.UpdateDescription(ApplicationVersion.Running(), next) + "\nUnduhan: " + (release.Size / 1048576d).ToString("N1", System.Globalization.CultureInfo.CurrentCulture) + " MB\nUnduh dan pasang dari oto.ayani.id? Dokumen tidak dikirim. Hash memeriksa integritas; paket pengembangan belum ditandatangani. Versi lama dipertahankan. Simpan draf sebelum membuka versi baru.")) return;
            var dataRoot = PortableProfile.DataRoot;
#if DEBUG
            dataRoot = manualUpdateFixtureDataRoot ?? dataRoot;
#endif
            var staging = Path.Combine(dataRoot, "update-downloads", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(staging);
            status.Text = "Mengunduh update…";
            progress.Title = "Mengunduh dan memasang update";
            messageOpen = true; shown = progress.ShowAsync();
            string installed;
            try
            {
                var zip = Path.Combine(staging, "package.zip");
                void Report(string text)
                {
                    status.Text = text;
#if DEBUG
                    manualUpdateFixtureStatus?.Invoke(text);
#endif
                }
                IProgress<string> reports = new Progress<string>(Report);
                await OnlineUpdatePackage.DownloadAsync(ManualUpdateHttp, release, zip, reports, cancel.Token);
                Report("Memverifikasi isi paket…");
                installed = await Task.Run(() =>
                {
                    var extracted = Path.Combine(staging, "extracted");
                    OnlineUpdatePackage.Extract(zip, extracted, cancel.Token);
                    var feed = Path.Combine(extracted, "feed");
                    var candidate = LocalAppUpdate.InspectFeed(feed);
                    if (candidate.Release.Version != release.Version) throw new InvalidDataException("Versi paket tidak cocok.");
                    if (baseline is not null && (!candidate.Release.ManifestSha256.Equals(baseline.ManifestSha256, StringComparison.OrdinalIgnoreCase)
                        || ApplicationVersion.Available(candidate.Manifest, candidate.Package).ProductVersion != baseline.ProductVersion
                        || !candidate.Manifest.Files.Any(e => e.Path == "restart-launcher/restart-capability.json")))
                        throw new InvalidDataException("Manifest atau baseline paket tidak cocok.");
                    if (notes is not null)
                    {
                        var selected = candidate.Manifest.Files.SingleOrDefault(e => e.Path == "Assets/release-notes.json");
                        if (selected is null || !selected.Sha256.Equals(release.ReleaseNotesSha256, StringComparison.OrdinalIgnoreCase)
                            || ApplicationVersion.Available(candidate.Manifest, candidate.Package).ProductVersion != release.ProductVersion)
                            throw new InvalidDataException("Catatan dan identitas paket tidak cocok.");
                    }
                    foreach (var entry in candidate.Manifest.Files) { cancel.Token.ThrowIfCancellationRequested(); LocalAppUpdate.Verify(LocalAppUpdate.SafePath(candidate.Package, "app/" + entry.Path), entry, cancel.Token); }
                    if (baseline is not null) BaselineUpdateAdmission.Validate(Path.Combine(candidate.Package, "app"), candidate.Manifest);
                    cancel.Token.ThrowIfCancellationRequested();
                    reports.Report("Memasang update…");
                    return LocalAppUpdate.InstallSelected(ManualUpdateRoot, feed, release.Version, candidate.Release.ManifestSha256, cancel.Token);
                }, cancel.Token);
            }
            finally { progress.Hide(); await shown; messageOpen = false; }
            await ShowUpdateCompletionAsync(installed);
        }
        catch (OperationCanceledException) { if (!onlineUpdateLifetime.IsCancellationRequested) await MenuMessageAsync("Update dibatalkan", "Versi aktif tetap dapat dijalankan. Periksa update kembali saat siap."); }
        catch (Exception error) { LocalDiagnostics.Record("manual-online-update", error); await MenuMessageAsync("Update belum berhasil", "Koneksi, paket, ruang penyimpanan, atau instalasi belum dapat diverifikasi. Versi aktif tidak diganti sebelum pemeriksaan berhasil. Fitur PDF lokal tetap tersedia."); }
        finally { checkingUpdate = false; }
    }
}
