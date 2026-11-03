#if DEBUG
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Pertamina.Oto.Distribution;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private async Task AuditOnlineFeedbackAsync(Action<bool, string> check)
    {
        const string newer = "20261002-180000-bbbbbbbb";
        var hash = new string('a', 64);
        var baseline = new DistributionRelease(2, "PERTAMINA OTO", "0.3.0", newer, DateTimeOffset.UtcNow,
            "development", "x64", true, 2, 1, hash, 1000, ["Synthetic notification"],
            new($"PERTAMINA-OTO-OnlineSetup-x64-{newer}.exe", 1, hash),
            new($"PERTAMINA-OTO-Setup-x64-{newer}.exe", 100, hash),
            new($"PERTAMINA-OTO-Windows-x64-{newer}.zip", 100, hash));
        static HttpResponseMessage Reply(object value) => new(HttpStatusCode.OK)
        { Content = new StringContent(JsonSerializer.Serialize(value, JsonSerializerOptions.Web), System.Text.Encoding.UTF8, "application/json") };
        async Task Until(Func<bool> condition)
        {
            for (var i = 0; i < 200; i++) { root.UpdateLayout(); if (condition()) return; await Task.Delay(25); }
            throw new TimeoutException("Online feedback callback did not complete");
        }
        static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
        ContentDialog? Dialog(string title) => VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot)
            .SelectMany(p => Descendants(p.Child).Prepend(p.Child)).OfType<ContentDialog>().FirstOrDefault(d => d.Title as string == title);
        async Task Close(string title)
        {
            await Until(() => Dialog(title) is { IsLoaded: true });
            Invoke(Descendants(Dialog(title)!).OfType<Button>().Single(b => b.Name == "CloseButton"));
            await Until(() => !checkingUpdate && !messageOpen);
        }
        var currentPath = Path.Combine(manualUpdateFixtureRoot!, "current.json");
        var originalPointer = File.ReadAllBytes(currentPath);
        var priorEnabled = onlineUpdatesEnabled; var priorDismissed = dismissedOnlineVersion;
        var originalBuild = manualUpdateFixtureBuild;
        var notificationRequests = 0;
        try
        {
            using var notifications = new HttpClient(new OnlineAuditHandler((request, _) =>
            {
                if (request.RequestUri != DistributionRelease.LatestUri) throw new InvalidDataException("Wrong notification endpoint");
                notificationRequests++; return Task.FromResult(Reply(baseline));
            }));
            notificationFixtureBuild = originalBuild; notificationFixtureClient = notifications;
            onlineUpdatesEnabled = false; onlineUpdateBanner.IsOpen = false; dismissedOnlineVersion = newer;
            ShowSettings(); await Task.Delay(300); root.UpdateLayout();
            var toggle = Descendants(root).OfType<ToggleSwitch>().Single(t => AutomationProperties.GetName(t) == "Notifikasi update saat OTO berjalan");
            check(toggle.IsEnabled, "online notification switch available for admitted installed-build fixture");
            toggle.IsOn = true;
            await Until(() => !checkingOnlineUpdate && notificationRequests == 1 && onlineUpdateBanner.IsOpen);
            check(offeredOnlineVersion == newer, "enabling notifications immediately fetches schema2 and displays newer build, including dismissed build");
            toggle.IsOn = false; check(!onlineUpdateBanner.IsOpen, "notification off hides offer without installing");
            toggle.IsOn = true; await Until(() => !checkingOnlineUpdate && notificationRequests == 2);
            check(onlineUpdateBanner.IsOpen, "re-enabling does not wait for thirty-minute timer or thirty-second throttle");

            using var failing = new HttpClient(new OnlineAuditHandler((_, _) => throw new HttpRequestException("synthetic offline")));
            notificationFixtureClient = failing; await CheckOnlineUpdateAsync(true);
            check(onlineUpdateStatus!.Text.Contains("belum dapat dijangkau", StringComparison.Ordinal), "notification failure is visible in settings while local work remains available");
            var failureStatus = onlineUpdateStatus.Text; ShowSettings(); await Task.Delay(250);
            check(onlineUpdateStatus!.Text == failureStatus, "last online result survives settings rebuild");

            var releasePoll = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var delayed = new HttpClient(new OnlineAuditHandler(async (_, token) => { await releasePoll.Task.WaitAsync(token); return Reply(baseline); }));
            var manualRequests = 0;
            using var manual = new HttpClient(new OnlineAuditHandler((_, _) =>
            { manualRequests++; return Task.FromResult(Reply(new ReleaseInfo(1, originalBuild!, DateTimeOffset.UtcNow, "development", $"PERTAMINA-OTO-Windows-x64-{originalBuild}.zip", 100, hash, ["Same installed fixture"]))); }));
            notificationFixtureClient = delayed; manualUpdateFixtureClient = manual;
            var pendingPoll = CheckOnlineUpdateAsync(true);
            try
            {
                check(checkingOnlineUpdate, "background metadata request is actually pending before primary click");
                Invoke(Descendants(root).OfType<Button>().Single(b => AutomationProperties.GetName(b) == "Periksa update"));
                await Until(() => Dialog("Sudah terbaru") is not null);
                check(manualRequests == 1 && checkingOnlineUpdate, "actual Periksa update button responds while background metadata is pending");
                await Close("Sudah terbaru");
            }
            finally
            {
                var currentToggle = Descendants(root).OfType<ToggleSwitch>().Single(t => AutomationProperties.GetName(t) == "Notifikasi update saat OTO berjalan");
                currentToggle.IsOn = false;
                releasePoll.TrySetResult(true); await pendingPoll;
            }
            check(!onlineUpdateBanner.IsOpen && onlineUpdateStatus!.Text.Contains("nonaktif", StringComparison.Ordinal),
                "disabling notifications during a pending request suppresses its late banner and status");
            manualUpdateFixtureBuild = "Build pengembangan";
            Invoke(Descendants(root).OfType<Button>().Single(b => AutomationProperties.GetName(b) == "Periksa update"));
            await Until(() => Dialog("Instalasi diperlukan") is not null);
            check(manualRequests == 1, "uninstalled build gets immediate explanation without unnecessary metadata request");
            await Close("Instalasi diperlukan");
            check(File.ReadAllBytes(currentPath).SequenceEqual(originalPointer), "all notification and check actions retain active installation pointer");
        }
        finally
        {
            manualUpdateFixtureClient = null; manualUpdateFixtureBuild = originalBuild;
            notificationFixtureClient = null; notificationFixtureBuild = null;
            onlineUpdatesEnabled = priorEnabled; dismissedOnlineVersion = priorDismissed; SaveOnlinePreference();
            onlineUpdateBanner.IsOpen = false; lastOnlineAttempt = default;
        }
    }
}
#endif
