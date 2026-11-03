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
    private string? onlineUpdateStatusText;
#if DEBUG
    private HttpClient? notificationFixtureClient;
    private string? notificationFixtureBuild;
#endif
    private static bool IsOnlineAudit => Environment.GetCommandLineArgs().Any(a => a.StartsWith("--audit", StringComparison.Ordinal) || a.StartsWith("--self-test", StringComparison.Ordinal));
    private string NotificationBuild =>
#if DEBUG
        notificationFixtureBuild ??
#endif
        RunningBuildLabel();
    private bool CanCheckOnlineNotifications => !PortableProfile.IsPortable && ReleaseInfo.ValidVersion(NotificationBuild) &&
#if DEBUG
        (notificationFixtureClient is not null || !IsOnlineAudit);
#else
        !IsOnlineAudit;
#endif
    private void SetOnlineUpdateStatus(string text)
    {
        onlineUpdateStatusText = text;
        if (onlineUpdateStatus is not null) onlineUpdateStatus.Text = text;
    }
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
            if (!CanCheckOnlineNotifications) return;
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
        if (!CanCheckOnlineNotifications || checkingUpdate || checkingOnlineUpdate || onlineUpdateLifetime.IsCancellationRequested || (!manual && (!onlineUpdatesEnabled || DateTimeOffset.UtcNow - lastOnlineAttempt < TimeSpan.FromSeconds(30)))) return;
        // Reachability is determined by the bounded HTTPS request. Windows' connectivity
        // classification may be stale or disabled even while this endpoint is reachable.
        checkingOnlineUpdate = true; lastOnlineAttempt = DateTimeOffset.UtcNow;
        SetOnlineUpdateStatus("Memeriksa informasi versi di oto.ayani.id…");
        try
        {
            var client = OnlineHttp;
#if DEBUG
            client = notificationFixtureClient ?? client;
#endif
            var release = (await OnlineReleaseClient.FetchBaselineAsync(client, onlineUpdateLifetime.Token))?.ToReleaseInfo();
            if (onlineUpdateLifetime.IsCancellationRequested || !onlineUpdatesEnabled) return;
            if (checkingUpdate) return; // The explicit check now owns visible update feedback.
            var running = NotificationBuild;
            if (release is not null && release.IsNewerThan(running))
            {
                if (manual || dismissedOnlineVersion != release.Version) ShowOnlineRelease(release);
                SetOnlineUpdateStatus(OnlineProductDescription(release) + " Pilih Update untuk mengunduh dan memasang atas pilihan Anda.");
            }
            else SetOnlineUpdateStatus(release is null
                ? "Belum ada rilis yang diterbitkan di portal."
                : "Tidak ada versi online yang lebih baru dari aplikasi ini.");
        }
        catch (Exception error) when (error is HttpRequestException or IOException or InvalidDataException or JsonException or OperationCanceledException)
        {
            if (!onlineUpdateLifetime.IsCancellationRequested && onlineUpdatesEnabled && !checkingUpdate)
                SetOnlineUpdateStatus("Portal update belum dapat dijangkau atau belum siap. Anda tetap dapat menggunakan fitur PDF lokal.");
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
        var supported = CanCheckOnlineNotifications;
        var enabled = new ToggleSwitch { IsOn = onlineUpdatesEnabled, IsEnabled = supported,
            OnContent = supported ? "Aktif" : "Belum tersedia", OffContent = "Nonaktif" };
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(enabled, "Notifikasi update saat OTO berjalan");
        enabled.Toggled += async (_, _) =>
        {
            onlineUpdatesEnabled = enabled.IsOn;
            if (!enabled.IsOn) { onlineUpdateBanner.IsOpen = false; SetOnlineUpdateStatus("Notifikasi otomatis nonaktif. Periksa update tetap dapat digunakan."); }
            SaveOnlinePreference();
            if (enabled.IsOn && supported) await CheckOnlineUpdateAsync(true);
        };
        row("\uE774", "Notifikasi update online", supported
            ? "Periksa saat diaktifkan, saat OTO terbuka, setiap 30 menit dan ketika koneksi tersedia. Dokumen tidak dikirim."
            : "Pemeriksaan otomatis tidak berjalan pada salinan pengujian ini. Gunakan aplikasi terpasang dari shortcut PERTAMINA OTO.", enabled);
        var actions = new ResponsiveWrapPanel { Spacing = 8 };
        actions.Children.Add(ActionButton("Update", async () => await CheckManualOnlineUpdateAsync()));
        actions.Children.Add(ActionButton("Update lokal", async () => await CheckPreparedLocalUpdateAsync()));
        actions.Children.Add(ActionButton("Buka unduhan", async () => await OpenOnlineDownloadsAsync()));
        content.Children.Add(actions);
        onlineUpdateStatus = Copy(onlineUpdateStatusText ?? (supported
            ? "Pemeriksaan online tidak menghalangi pemrosesan PDF saat offline. Update tidak dipasang otomatis."
            : "Update online memerlukan aplikasi terpasang. Jalankan shortcut PERTAMINA OTO."), 12);
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetLiveSetting(onlineUpdateStatus, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
        content.Children.Add(onlineUpdateStatus);
    }
}
