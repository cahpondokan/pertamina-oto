using System.Net.Http;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pertamina.Oto.Distribution;
using Windows.Networking.Connectivity;

namespace Pertamina.Oto.Desktop.App;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001", Justification = "WinUI Window owns the lifetime; its Closed event cancels and disposes onlineUpdateLifetime.")]
internal sealed partial class MainWindow
{
    private readonly InfoBar onlineUpdateBanner = new() { IsOpen = false, IsClosable = true, Severity = InfoBarSeverity.Informational };
    private readonly DispatcherTimer onlineUpdateTimer = new() { Interval = TimeSpan.FromMinutes(30) };
    private readonly CancellationTokenSource onlineUpdateLifetime = new();
    private static readonly HttpClient OnlineHttp = new(new SocketsHttpHandler { AllowAutoRedirect = false, UseCookies = false, PooledConnectionLifetime = TimeSpan.FromMinutes(5) });
    private bool onlineUpdatesStarted;
    private bool checkingOnlineUpdate;
    private bool onlineUpdatesEnabled = true;
    private string? dismissedOnlineVersion;
    private string? offeredOnlineVersion;
    private DateTimeOffset lastOnlineAttempt;
    private TextBlock? onlineUpdateStatus;
    private static string OnlinePreferencePath => Environment.GetCommandLineArgs().Contains("--audit-online-update", StringComparer.Ordinal)
        ? Path.Combine(AppContext.BaseDirectory, "online-update-audit", "isolated-preference.json")
        : Path.Combine(PortableProfile.DataRoot, "online-updates.json");
    private sealed record OnlinePreference(bool Enabled, string? DismissedVersion);

    private void InitializeOnlineUpdates(Grid shell)
    {
        shell.RowDefinitions.Insert(1, new RowDefinition { Height = GridLength.Auto });
        Grid.SetRow(root, 2); Grid.SetRow(onlineUpdateBanner, 1); shell.Children.Add(onlineUpdateBanner);
        onlineUpdateBanner.ActionButton = ActionButton("Update", async () => await CheckManualOnlineUpdateAsync());
        onlineUpdateBanner.Closed += (_, e) =>
        {
            if (e.Reason != InfoBarCloseReason.CloseButton || offeredOnlineVersion is null) return;
            dismissedOnlineVersion = offeredOnlineVersion; SaveOnlinePreference();
        };
        try
        {
            if (File.Exists(OnlinePreferencePath) && new FileInfo(OnlinePreferencePath).Length <= 4096)
            {
                var saved = JsonSerializer.Deserialize<OnlinePreference>(File.ReadAllText(OnlinePreferencePath));
                if (saved is not null) { onlineUpdatesEnabled = saved.Enabled; dismissedOnlineVersion = saved.DismissedVersion; }
            }
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException) { LocalDiagnostics.Record("online-update-preference-read", error); }
        Activated += async (_, _) =>
        {
            if (onlineUpdatesStarted) return;
            onlineUpdatesStarted = true;
            // Native audit and development startup never contact the public endpoint.
            if (PortableProfile.IsPortable || !ReleaseInfo.ValidVersion(RunningBuildLabel()) || Environment.GetCommandLineArgs().Any(a => a.StartsWith("--audit", StringComparison.Ordinal) || a.StartsWith("--self-test", StringComparison.Ordinal))) return;
            NetworkInformation.NetworkStatusChanged += OnlineNetworkChanged;
            onlineUpdateTimer.Tick += async (_, _) => await CheckOnlineUpdateAsync(false);
            onlineUpdateTimer.Start();
            try { await Task.Delay(TimeSpan.FromSeconds(10), onlineUpdateLifetime.Token); await CheckOnlineUpdateAsync(false); }
            catch (OperationCanceledException) { }
        };
        Closed += (_, _) => { onlineUpdateTimer.Stop(); NetworkInformation.NetworkStatusChanged -= OnlineNetworkChanged; onlineUpdateLifetime.Cancel(); onlineUpdateLifetime.Dispose(); };
    }

    private void OnlineNetworkChanged(object sender) => DispatcherQueue.TryEnqueue(async () => await CheckOnlineUpdateAsync(false));

    private void SaveOnlinePreference()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(OnlinePreferencePath)!);
            var pending = OnlinePreferencePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(pending, JsonSerializer.Serialize(new OnlinePreference(onlineUpdatesEnabled, dismissedOnlineVersion)));
            File.Move(pending, OnlinePreferencePath, true);
        }
        catch (Exception error) { LocalDiagnostics.Record("online-update-preference-write", error); if (onlineUpdateStatus is not null) onlineUpdateStatus.Text = "Pilihan berlaku untuk sesi ini; belum dapat disimpan."; }
    }

    private async Task CheckOnlineUpdateAsync(bool manual)
    {
        if ((!manual && PortableProfile.IsPortable) || checkingOnlineUpdate || onlineUpdateLifetime.IsCancellationRequested || (!manual && (!onlineUpdatesEnabled || DateTimeOffset.UtcNow - lastOnlineAttempt < TimeSpan.FromSeconds(30)))) return;
        if (!manual && NetworkInformation.GetInternetConnectionProfile()?.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.InternetAccess) return;
        checkingOnlineUpdate = true; lastOnlineAttempt = DateTimeOffset.UtcNow;
        if (manual && onlineUpdateStatus is not null) onlineUpdateStatus.Text = "Memeriksa informasi versi di oto.ayani.id…";
        try
        {
            var release = await OnlineReleaseClient.FetchAsync(OnlineHttp, onlineUpdateLifetime.Token);
            if (onlineUpdateLifetime.IsCancellationRequested || (!manual && !onlineUpdatesEnabled)) return;
            var running = RunningBuildLabel();
            if (release is not null && release.IsNewerThan(running))
            {
                if (manual || dismissedOnlineVersion != release.Version) ShowOnlineRelease(release);
                if (onlineUpdateStatus is not null) onlineUpdateStatus.Text = OnlineProductDescription(release) + " Pilih Update untuk mengunduh dan memasang atas pilihan Anda.";
            }
            else if (onlineUpdateStatus is not null) onlineUpdateStatus.Text = release is null
                ? "Belum ada rilis yang diterbitkan di portal."
                : ReleaseInfo.ValidVersion(running) ? "Tidak ada versi online yang lebih baru dari aplikasi ini." : "Portal dapat dijangkau. Perbandingan otomatis aktif pada build yang sudah dipasang.";
        }
        catch (Exception error) when (error is HttpRequestException or IOException or InvalidDataException or JsonException or OperationCanceledException)
        {
            if (manual && !onlineUpdateLifetime.IsCancellationRequested && onlineUpdateStatus is not null)
                onlineUpdateStatus.Text = "Portal update belum dapat dijangkau atau belum siap. Anda tetap dapat menggunakan fitur PDF lokal.";
        }
        finally { checkingOnlineUpdate = false; }
    }

    private void ShowOnlineRelease(ReleaseInfo release)
    {
        offeredOnlineVersion = release.Version;
        onlineUpdateBanner.Title = "Pembaruan OTO tersedia";
        onlineUpdateBanner.Message = OnlineProductDescription(release) + " Simpan pekerjaan saat Anda siap memasangnya.";
        onlineUpdateBanner.IsOpen = true;
    }

    private static string OnlineProductDescription(ReleaseInfo release) => ApplicationVersion.UpdateDescription(ApplicationVersion.Running(),
        new(release.ProductVersion, release.Channel == "development" ? "Pengembangan" : null, release.Version));

    private async Task OpenOnlineDownloadsAsync()
    {
        try
        {
            if (!await Windows.System.Launcher.LaunchUriAsync(ReleaseInfo.DownloadPage))
                await MenuMessageAsync("Buka halaman unduhan", "Buka https://oto.ayani.id melalui browser Anda.");
        }
        catch (Exception error) { LocalDiagnostics.Record("online-download-page", error); await MenuMessageAsync("Halaman belum terbuka", "Buka https://oto.ayani.id melalui browser Anda."); }
    }

    private void AddOnlineUpdateSetting(Action<string, string, string, FrameworkElement> row, StackPanel content)
    {
        if (PortableProfile.IsPortable)
        {
            row("\uE774", "Unduhan portable", "Tidak ada pemeriksaan online otomatis. Buka website hanya atas pilihan Anda.", ActionButton("Buka unduhan", async () => await OpenOnlineDownloadsAsync()));
            return;
        }
        var enabled = new ToggleSwitch { IsOn = onlineUpdatesEnabled, OnContent = "Aktif", OffContent = "Nonaktif" };
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(enabled, "Notifikasi update saat OTO berjalan");
        enabled.Toggled += (_, _) => { onlineUpdatesEnabled = enabled.IsOn; if (!enabled.IsOn) onlineUpdateBanner.IsOpen = false; SaveOnlinePreference(); };
        row("\uE774", "Notifikasi update online", "Saat OTO terbuka, periksa versi di oto.ayani.id setiap 30 menit dan ketika koneksi tersedia. Dokumen tidak dikirim.", enabled);
        var actions = new ResponsiveWrapPanel { Spacing = 8 };
        actions.Children.Add(ActionButton("Update", async () => await CheckManualOnlineUpdateAsync()));
        actions.Children.Add(ActionButton("Update lokal", async () => await CheckPreparedLocalUpdateAsync()));
        actions.Children.Add(ActionButton("Buka unduhan", async () => await OpenOnlineDownloadsAsync()));
        content.Children.Add(actions);
        onlineUpdateStatus = Copy("Pemeriksaan online tidak menghalangi pemrosesan PDF saat offline. Update tidak dipasang otomatis.", 12);
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetLiveSetting(onlineUpdateStatus, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
        content.Children.Add(onlineUpdateStatus);
    }
}
