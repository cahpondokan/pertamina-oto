using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private bool checkingUpdate;
    private static string RunningBuildLabel()
    {
        var info = Path.Combine(AppContext.BaseDirectory, "build-info.json");
        try { return File.Exists(info) ? LocalAppUpdate.Read<LocalAppUpdate.Release>(info).Version : "Build pengembangan"; }
        catch (Exception) { return "Build pengembangan"; }
    }
    private async Task CheckLocalUpdateAsync()
    {
        if (Environment.GetCommandLineArgs().Any(a => a.StartsWith("--audit", StringComparison.Ordinal))) return;
        await CheckManualOnlineUpdateAsync();
    }
    private async Task CheckPreparedLocalUpdateAsync()
    {
        if (PortableProfile.IsPortable) { await MenuMessageAsync("Update portable", "Ekstrak ZIP versi baru ke folder terpisah. Instalasi lokal tidak diubah."); return; }
        if (checkingUpdate || messageOpen) return;
        if (active is not null || importing || liveEditor?.Working == true) { await MenuMessageAsync("Pekerjaan sedang berjalan", "Selesaikan atau batalkan proses dokumen sebelum update."); return; }
        checkingUpdate = true;
        try
        {
            var rootPath = LocalAppUpdate.InstallRoot;
            if (!File.Exists(Path.Combine(rootPath, "update-source.json"))) { await MenuMessageAsync("Instalasi lokal belum tersedia", "Jalankan aplikasi dari shortcut PERTAMINA OTO di desktop setelah pemasangan lokal."); return; }
            var candidate = await Task.Run(() => LocalAppUpdate.Inspect(rootPath));
            var current = LocalAppUpdate.VersionAt(rootPath);
            if (candidate.Release.Version == current)
            {
                if (RunningBuildLabel() != current) await ShowUpdateCompletionAsync(current);
                else await MenuMessageAsync("Update aplikasi", "Versi terbaru yang disiapkan di folder rilis lokal sudah terpasang.");
                return;
            }
            var next = ApplicationVersion.Available(candidate.Manifest, candidate.Package);
            var notes = ReleaseNotes.Available(candidate.Manifest, candidate.Package, next);
            if (!await ConfirmNotesUpdateAsync("Update lokal tersedia", next, notes, "Pasang update",
                ApplicationVersion.UpdateDescription(ApplicationVersion.Running(), next) + "\nFile dokumen dan pengaturan tidak dihapus. Versi lama dipertahankan. Simpan draf sebelum membuka versi baru.")) return;
            messageOpen = true;
            var progress = new ContentDialog { XamlRoot = root.XamlRoot, Title = "Memasang update lokal…", Content = new ProgressRing { IsActive = true, Width = 48, Height = 48 } };
            var shown = progress.ShowAsync();
            string installed;
            try { installed = await Task.Run(() => LocalAppUpdate.InstallSelected(rootPath, null, candidate.Release.Version, candidate.Release.ManifestSha256)); }
            finally { progress.Hide(); await shown; messageOpen = false; }
            await ShowUpdateCompletionAsync(installed);
        }
        catch (Exception error) { LocalDiagnostics.Record("local-app-update", error); await MenuMessageAsync("Update belum berhasil", "Folder rilis mungkin tidak tersedia, paket tidak valid, atau sedang dipakai updater lain. Versi aktif tidak diganti sebelum pemeriksaan berhasil. Coba kembali setelah paket rilis siap."); }
        finally { checkingUpdate = false; }
    }
}
