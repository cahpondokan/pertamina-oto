using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private bool restartRequested;
    internal static string RestartRunningVersion() => RunningBuildLabel();
    internal Action FreezeRestartStartup()
    {
        var controls = Descendants((DependencyObject)Content).OfType<Control>().Select(c => (Control: c, Enabled: c.IsEnabled)).ToArray();
        foreach (var entry in controls) entry.Control.IsEnabled = false;
        return () => { foreach (var entry in controls) entry.Control.IsEnabled = entry.Enabled; };
    }
    private ContentDialog? updateCompletion;
#if DEBUG
    private string? restartFixtureRoot;
#endif
    private string RestartInstallRoot
    {
        get
        {
#if DEBUG
            if (restartFixtureRoot is not null) return restartFixtureRoot;
#endif
            return LocalAppUpdate.InstallRoot;
        }
    }
    private bool RestartBusy => active is not null || importing || pastingDocument || extractLoading || removeLoading ||
        organizerHeld is not null || liveEditor?.Working == true || pdfReader?.RestartBusy == true;
    private bool RestartHasSession => RestartPendingWork().Count > 0;

    private sealed partial class PdfReaderView
    {
        internal bool RestartBusy => busy || searchCancellation is not null || !Rendering.IsCompleted || !searchNavigation.IsCompleted;
    }

    private ContentDialog NewUpdateDialog(string title, string icon, bool warning, string action, string dismiss, bool continuePrimary)
    {
        var dialog = new ContentDialog { XamlRoot = root.XamlRoot, PrimaryButtonText = action, CloseButtonText = dismiss,
            DefaultButton = ContentDialogButton.Close, RequestedTheme = ElementTheme.Light };
        dialog.Resources["ContentDialogMaxWidth"] = 680d;
        dialog.Resources["ContentDialogCornerRadius"] = new CornerRadius(12);
        dialog.Resources["ContentDialogBackground"] = Brush(255, 255, 255);
        var header = new Grid { Width = Math.Max(80, Math.Min(632, root.ActualWidth - 96)), Margin = new Thickness(0, 0, 0, 4) };
        header.Children.Add(new Border { Width = 64, Height = 64, CornerRadius = new CornerRadius(14),
            HorizontalAlignment = HorizontalAlignment.Left, Background = warning ? Brush(255, 244, 222) : Brush(234, 249, 234),
            Child = HistoryIcon(icon, 36) });
        var close = HistoryButton("x", "Tutup dialog update dan lanjut bekerja"); close.Width = 40; close.Height = 40;
        close.HorizontalAlignment = HorizontalAlignment.Right; close.VerticalAlignment = VerticalAlignment.Top;
        close.Click += (_, _) => dialog.Hide(); header.Children.Add(close); dialog.Title = header;
        AutomationProperties.SetName(dialog, title);
        void ResizeDialog(object sender, SizeChangedEventArgs args)
        {
            header.Width = Math.Max(80, Math.Min(632, root.ActualWidth - 96));
            if (dialog.Content is ScrollViewer scroll) scroll.MaxHeight = Math.Max(60, root.ActualHeight - 260);
        }
        dialog.Closed += (_, _) => root.SizeChanged -= ResizeDialog;
        dialog.Opened += async (_, _) =>
        {
            root.SizeChanged += ResizeDialog;
            await Task.Delay(30); dialog.UpdateLayout();
            var buttons = Descendants(dialog).OfType<Button>().ToArray();
            var primary = buttons.FirstOrDefault(b => b.Name == "PrimaryButton");
            var secondary = buttons.FirstOrDefault(b => b.Name == "CloseButton");
            if (primary is null || secondary is null) return;
            var blue = continuePrimary ? secondary : primary;
            var plain = continuePrimary ? primary : secondary;
            Grid.SetColumn(plain, 0); Grid.SetColumn(blue, 4);
            plain.TabIndex = 0; blue.TabIndex = 1;
            foreach (var button in new[] { plain, blue })
            {
                button.MinHeight = 44; button.CornerRadius = new CornerRadius(6);
                button.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold; button.FontSize = 14;
                button.Padding = new Thickness(12, 10, 12, 10);
            }
            plain.Background = Brush(255, 255, 255); plain.Foreground = Brush(23, 43, 70); plain.BorderBrush = Brush(170, 184, 203);
            blue.Background = Brush(0, 103, 217); blue.Foreground = Brush(255, 255, 255); blue.BorderBrush = Brush(0, 103, 217);
            blue.Resources["ButtonBackgroundPointerOver"] = Brush(0, 82, 180); blue.Resources["ButtonForegroundPointerOver"] = Brush(255, 255, 255);
            blue.Resources["ButtonBackgroundPressed"] = Brush(0, 65, 145); blue.Resources["ButtonForegroundPressed"] = Brush(255, 255, 255);
            if (continuePrimary)
            {
                var restart = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                restart.Children.Add(HistoryIcon("update-restart", 18)); restart.Children.Add(new TextBlock { Text = action });
                primary.Content = restart;
            }
            AutomationProperties.SetName(primary, action); AutomationProperties.SetName(secondary, dismiss);
            secondary.Focus(FocusState.Programmatic);
        };
        return dialog;
    }

    private StackPanel UpdateDialogBody(ContentDialog dialog, string title, string description)
    {
        var content = new StackPanel { Spacing = 18 };
        content.Children.Add(HistoryText(title, 26, true)); content.Children.Add(HistoryText(description, 16, muted: true));
        dialog.Content = new ScrollViewer { Content = content, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto, MaxHeight = Math.Max(60, root.ActualHeight - 260) };
        return content;
    }

    private ContentDialog BuildUpdateCompletion(string nextVersion)
    {
        LocalAppUpdate.ValidateVersion(nextVersion);
        var dialog = NewUpdateDialog("Versi baru siap digunakan", "update-check", false, "Mulai ulang sekarang", "Lanjut bekerja", true);
        var content = UpdateDialogBody(dialog, "Versi baru siap digunakan",
            "Anda bisa melanjutkan pekerjaan. Versi baru akan digunakan setelah aplikasi ditutup dan dibuka kembali.");
        var info = new Grid { ColumnSpacing = 12 };
        info.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); info.ColumnDefinitions.Add(new ColumnDefinition());
        info.Children.Add(HistoryIcon("update-info", 22));
        var text = HistoryText("Tidak perlu mulai ulang sekarang.", 14); text.Foreground = Brush(0, 86, 180);
        Grid.SetColumn(text, 1); info.Children.Add(text);
        content.Children.Add(new Border { Background = Brush(234, 244, 255), CornerRadius = new CornerRadius(8), Padding = new Thickness(16), Child = info });
        // Identity remains available, but no build-code wall competes with the approved main copy.
        var next = ApplicationVersion.ReadDirectory(LocalAppUpdate.SafePath(RestartInstallRoot, "versions/" + nextVersion), nextVersion);
        var details = new StackPanel { Spacing = 8 };
        details.Children.Add(HistoryText("Versi berjalan", 12, muted: true)); details.Children.Add(HistoryText(SafeRunningVersion(), 14));
        details.Children.Add(HistoryText("Versi berikutnya", 12, muted: true)); details.Children.Add(HistoryText(next.Display, 14));
        details.Children.Add(HistoryText(ApplicationVersion.UpdateDescription(ApplicationVersion.Running(), next), 13, muted: true));
        var versionDetails = new Expander { Header = "Detail versi", Content = details, HorizontalAlignment = HorizontalAlignment.Stretch };
        AutomationProperties.SetName(versionDetails, "Detail versi pembaruan"); content.Children.Add(versionDetails);
        return dialog;
    }

    private ContentDialog BuildRestartWorkDialog(IReadOnlyList<RestartWork> pending, Action<RestartWork> choose)
    {
        var dialog = NewUpdateDialog("Simpan perubahan dahulu", "update-document-alert", true, "Tinjau & simpan", "Nanti saja", false);
        var content = UpdateDialogBody(dialog, "Simpan perubahan dahulu",
            "Dokumen berikut memiliki perubahan yang belum disimpan. Tinjau dan simpan sebelum mulai ulang.");
        var documents = new StackPanel { Spacing = 8 };
        var rows = new List<Button>();
        foreach (var work in pending)
        {
            var row = new Grid { ColumnSpacing = 14, RowSpacing = 6, HorizontalAlignment = HorizontalAlignment.Stretch };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Children.Add(HistoryIcon("file-text", 30));
            var words = new StackPanel { Spacing = 3 };
            words.Children.Add(new TextBlock { Text = work.Name, FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = Brush(23, 43, 70), TextTrimming = TextTrimming.CharacterEllipsis });
            words.Children.Add(HistoryText(work.Tool, 12, muted: true)); Grid.SetColumn(words, 1); row.Children.Add(words);
            words.Children.Add(HistoryText(work.Reason, 12, muted: true));
            var badge = new Border { Background = Brush(255, 241, 213), CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10, 6, 10, 6), VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock { Text = "Belum disimpan", FontSize = 12, Foreground = Brush(133, 78, 0) } };
            Grid.SetColumn(badge, 2); row.Children.Add(badge);
            row.SizeChanged += (_, _) =>
            {
                var narrow = row.ActualWidth < 440;
                if (row.RowDefinitions.Count == 0) { row.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); row.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); }
                Grid.SetRow(badge, narrow ? 1 : 0); Grid.SetColumn(badge, narrow ? 1 : 2);
                badge.HorizontalAlignment = HorizontalAlignment.Left;
            };
            var button = new Button { Content = row, HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Stretch,
                MinHeight = 74, Padding = new Thickness(14), Background = Brush(255, 255, 255), BorderBrush = Brush(218, 227, 239),
                BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8) };
            AutomationProperties.SetName(button, work.Name + ", " + work.Tool + ", belum disimpan. Pilih untuk ditinjau.");
            ToolTipService.SetToolTip(button, work.Name);
            button.Click += (_, _) => { choose(work); foreach (var candidate in rows) candidate.BorderBrush = candidate == button ? Brush(0, 103, 217) : Brush(218, 227, 239); };
            rows.Add(button); documents.Children.Add(button);
        }
        if (pending.Count > 1) content.Children.Add(HistoryText("Pilih pekerjaan yang ingin ditinjau terlebih dahulu.", 13, muted: true));
        content.Children.Add(documents);
        content.Children.Add(HistoryText("Aplikasi tetap terbuka sampai Anda selesai.", 13, muted: true));
        return dialog;
    }

    private async Task ShowUpdateCompletionAsync(string nextVersion)
    {
        if (messageOpen || restartRequested) return;
        var focus = FocusManager.GetFocusedElement(root.XamlRoot) as Control; var resume = liveEditor?.PauseForReadPreview();
        messageOpen = true; ContentDialogResult answer;
        try { updateCompletion = BuildUpdateCompletion(nextVersion); answer = await updateCompletion.ShowAsync(); }
        finally { updateCompletion = null; messageOpen = false; if (resume is not null) await resume(); if (focus?.IsLoaded == true) focus.Focus(FocusState.Programmatic); }
        if (answer == ContentDialogResult.Primary) await RestartForUpdateAsync(nextVersion);
    }

    private async Task RestartForUpdateAsync(string nextVersion)
    {
        if (restartRequested || messageOpen) return;
        if (RestartBusy) { await MenuMessageAsync("Pekerjaan sedang berjalan", "Tunggu atau batalkan impor/proses dokumen sebelum memulai ulang. Aplikasi tetap terbuka."); return; }
        var pending = RestartPendingWork();
        if (pending.Count > 0)
        {
            var review = pending[0]; var focus = FocusManager.GetFocusedElement(root.XamlRoot) as Control;
            var resume = liveEditor?.PauseForReadPreview(); messageOpen = true; ContentDialogResult answer;
            try { answer = await BuildRestartWorkDialog(pending, work => review = work).ShowAsync(); }
            finally
            {
                messageOpen = false; if (resume is not null) await resume();
                if (focus?.IsLoaded == true) focus.Focus(FocusState.Programmatic);
            }
            if (answer == ContentDialogResult.Primary) review.Review();
            return;
        }
        restartRequested = true; messageOpen = true;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(25));
        var progress = new ContentDialog { XamlRoot = root.XamlRoot, Title = "Menyiapkan mulai ulang…", Content = "Memeriksa launcher dan menunggu konfirmasi aplikasi baru. Aplikasi ini tetap terbuka jika persiapan gagal.", CloseButtonText = "Batal", DefaultButton = ContentDialogButton.Close };
        progress.CloseButtonClick += (_, _) => timeout.Cancel();
        var shown = progress.ShowAsync();
        try
        {
            using var prepared = await RestartProtocol.PrepareAsync(RestartInstallRoot, nextVersion, timeout.Token);
            if (timeout.IsCancellationRequested || RestartBusy || RestartHasSession || !prepared.NextRunning) throw new IOException("Restart guard changed.");
            await prepared.CommitCloseAsync(); progress.Hide(); await shown; allowClose = true;
            try { Close(); } catch { allowClose = false; throw; }
        }
        catch (Exception error)
        {
            allowClose = false; progress.Hide(); await shown; messageOpen = false; LocalDiagnostics.Record("update-restart", error);
#if DEBUG
            if (Environment.GetEnvironmentVariable("OTO_RESTART_NATIVE_FIXTURE") == "1")
            { File.WriteAllText(Path.Combine(RestartInstallRoot, "native-parent-failed.txt"), error.GetType().Name + ": " + error.Message); Environment.ExitCode = 1; Application.Current.Exit(); return; }
#endif
            await MenuMessageAsync("Aplikasi tetap terbuka", "Mulai ulang belum dapat disiapkan. Launcher mungkin belum mendukung restart, tidak tersedia, atau versi baru belum mengonfirmasi startup. Pekerjaan tetap di sesi ini. Simpan dan tutup pekerjaan dahulu; Anda dapat menutup aplikasi normal dan membuka shortcut yang sama.");
        }
        finally { restartRequested = false; messageOpen = false; }
    }
}
