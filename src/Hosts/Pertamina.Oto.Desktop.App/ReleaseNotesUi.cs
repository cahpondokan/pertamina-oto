using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private static string ReleaseViewedPath => Path.Combine(Path.GetDirectoryName(PreferenceFile)!, "release-notes-viewed.json");
    private Microsoft.UI.Dispatching.DispatcherQueueTimer? releaseNotesTimer;
    private ReleaseNotes.Catalog? startupNotes;
    private bool ReleaseNotesStartupSafe => root.IsLoaded && !messageOpen && !RestartBusy && RestartPendingWork().Count == 0
        && pdfReader?.Visible != true && liveEditor?.HasDocument != true
        && FocusManager.GetFocusedElement(root.XamlRoot) is not (TextBox or RichEditBox)
        && screenIndex >= 0 && screens[screenIndex].Title is "Mau mengerjakan apa hari ini?" or "Dokumen Anda siap";
    private static StackPanel NotesSummary(ReleaseNotes.Catalog? notes)
    {
        var content = new StackPanel { Spacing = 10 };
        foreach (var text in notes?.Releases[0].Summary ?? [ReleaseNotes.Missing]) content.Children.Add(Copy("• " + text, 14));
        return content;
    }
    private async Task ShowReleaseHistoryAsync(ReleaseNotes.Catalog? notes = null)
    {
        if (messageOpen) return;
        notes ??= ReleaseNotes.Read(AppContext.BaseDirectory, ApplicationVersion.Running().ProductVersion);
        var content = new StackPanel { Spacing = 16 };
        if (notes is null || notes.Releases.Length == 0) content.Children.Add(Copy(ReleaseNotes.Missing, 14));
        else foreach (var release in notes.Releases)
        {
            content.Children.Add(new TextBlock { Text = "OTO " + release.ProductVersion + " · " + DateOnly.ParseExact(release.Date, "yyyy-MM-dd").ToString("d MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("id-ID")), FontSize = 21, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
            void Category(string title, string[] items)
            {
                if (items.Length == 0) return;
                content.Children.Add(new TextBlock { Text = title, FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
                foreach (var item in items) content.Children.Add(Copy("• " + item, 14));
            }
            Category("Fitur baru", release.Features); Category("Peningkatan", release.Improvements);
            Category("Perbaikan", release.Fixes); Category("Batasan yang diketahui", release.Limitations);
        }
        await ShowNotesDialogAsync("Riwayat versi", content, "Tutup");
    }
    private async Task<ContentDialogResult> ShowNotesDialogAsync(string title, StackPanel content, string close,
        string? primary = null, Action? displayed = null)
    {
        var opener = FocusManager.GetFocusedElement(root.XamlRoot) as Control;
        var scroll = new ScrollViewer { Content = content, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        AutomationProperties.SetName(scroll, title);
        var dialog = new ContentDialog { XamlRoot = root.XamlRoot, Title = title, Content = scroll, CloseButtonText = close,
            PrimaryButtonText = primary ?? "", DefaultButton = ContentDialogButton.Close };
        ResponsiveLayout.BoundDialog(dialog, scroll, root);
        dialog.Opened += (_, _) => displayed?.Invoke();
        messageOpen = true;
        try { return await dialog.ShowAsync(); }
        finally { messageOpen = false; opener?.Focus(FocusState.Programmatic); }
    }
    private async Task<bool> ConfirmNotesUpdateAsync(string title, ApplicationVersion.Identity next, ReleaseNotes.Catalog? notes,
        string action, string explanation)
    {
        while (true)
        {
            var content = new StackPanel { Spacing = 16 };
            content.Children.Add(Copy("Versi saat ini: " + SafeRunningVersion(), 14));
            content.Children.Add(new TextBlock { Text = "Versi tujuan: " + next.Display, FontSize = 20, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
            content.Children.Add(NotesSummary(notes));
            var detail = ActionButton("Lihat detail perubahan", () => { });
            var requestDetail = false;
            ContentDialog? dialog = null;
            detail.Click += (_, _) => { requestDetail = true; dialog?.Hide(); };
            content.Children.Add(detail); content.Children.Add(Copy(explanation, 13));
            var scroll = new ScrollViewer { Content = content, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
            dialog = new ContentDialog { XamlRoot = root.XamlRoot, Title = title, Content = scroll, PrimaryButtonText = action, CloseButtonText = "Nanti", DefaultButton = ContentDialogButton.Close };
            ResponsiveLayout.BoundDialog(dialog, scroll, root);
            var opener = FocusManager.GetFocusedElement(root.XamlRoot) as Control;
            messageOpen = true;
            ContentDialogResult result;
            try { result = await dialog.ShowAsync(); }
            finally { messageOpen = false; opener?.Focus(FocusState.Programmatic); }
            if (!requestDetail) return result == ContentDialogResult.Primary;
            // Missing selected notes must never substitute the running package's history.
            await ShowReleaseHistoryAsync(notes ?? new ReleaseNotes.Catalog(1, next.ProductVersion ?? "", []));
        }
    }
    internal void ScheduleReleaseNotesStartup()
    {
        if (releaseNotesTimer is not null || Environment.GetCommandLineArgs().Any(a => a.StartsWith("--audit", StringComparison.Ordinal))
            || Environment.GetEnvironmentVariable("OTO_RESTART_NATIVE_FIXTURE") == "1" || Environment.GetEnvironmentVariable("OTO_STARTUP_WINDOW_AUDIT") == "1") return;
        try
        {
            var running = ApplicationVersion.Running();
            startupNotes = ReleaseNotes.Read(AppContext.BaseDirectory, running.ProductVersion);
            var current = LocalAppUpdate.VersionAt(LocalAppUpdate.InstallRoot);
            ApplicationVersion.Identity previous = new(null, null, null);
            try
            {
                var pointer = LocalAppUpdate.Read<LocalAppUpdate.Release>(Path.Combine(LocalAppUpdate.InstallRoot, "previous.json"));
                if (ApplicationVersion.ValidBuild(pointer.Version)) previous = ApplicationVersion.ReadDirectory(LocalAppUpdate.SafePath(LocalAppUpdate.InstallRoot, "versions/" + pointer.Version), pointer.Version);
            }
            catch (Exception) { /* First install or unavailable predecessor: no update claim. */ }
            var viewed = ReleaseNotes.ReadViewed(ReleaseViewedPath);
            if (!ReleaseNotes.ShouldShow(running, current, previous, viewed, startupNotes))
            {
                // Establish a first-install baseline, never acknowledge an upgrade whose notes failed.
                if (running.Known && running.BuildId == current && viewed.Versions.Length == 0
                    && (!previous.Known || ReleaseNotes.Compare(previous.ProductVersion!, running.ProductVersion!) >= 0))
                    ReleaseNotes.MarkViewed(ReleaseViewedPath, running.ProductVersion!);
                return;
            }
            releaseNotesTimer = DispatcherQueue.CreateTimer(); releaseNotesTimer.Interval = TimeSpan.FromSeconds(2);
            releaseNotesTimer.Tick += async (_, _) =>
            {
                // Wait without taking focus from Explorer documents, recovery/safety or typing.
                if (!ReleaseNotesStartupSafe) return;
                releaseNotesTimer.Stop();
                await ShowStartupNotesAsync(startupNotes!);
            };
            Closed += (_, _) => releaseNotesTimer?.Stop(); releaseNotesTimer.Start();
        }
        catch (Exception error) { LocalDiagnostics.Record("release-notes-startup", error); }
    }
    private async Task ShowStartupNotesAsync(ReleaseNotes.Catalog notes)
    {
        try
        {
            var result = await ShowNotesDialogAsync("Yang baru di OTO " + notes.ProductVersion, NotesSummary(notes), "Mulai menggunakan", "Lihat detail perubahan",
                () => { try { ReleaseNotes.MarkViewed(ReleaseViewedPath, notes.ProductVersion); } catch (Exception error) { LocalDiagnostics.Record("release-notes-viewed", error); } });
            if (result == ContentDialogResult.Primary) await ShowReleaseHistoryAsync(notes);
        }
        catch (Exception error) { LocalDiagnostics.Record("release-notes-panel", error); }
    }
}
