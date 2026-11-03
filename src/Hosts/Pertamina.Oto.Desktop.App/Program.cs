using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Pertamina.Oto.Desktop.App;

internal static class Program
{
    internal static ReaderActivation? ReaderBridge { get; private set; }
    internal static bool ReaderInteractiveTest =>
#if DEBUG
        Environment.GetEnvironmentVariable("OTO_READER_TEST_MODE") == "1";
#else
        false;
#endif
    [STAThread]
    private static void Main()
    {
        var printArgs = Environment.GetCommandLineArgs();
        if (printArgs.Length == 3 && printArgs[1] == "--print-worker") { WinRT.ComWrappersSupport.InitializeComWrappers(); Environment.ExitCode = Task.Run(() => PrintWorker.RunAsync(printArgs[2])).GetAwaiter().GetResult(); return; }
        try { PortableProfile.ValidateStartup(); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidOperationException or KeyNotFoundException)
        {
            Environment.ExitCode = 1;
            _ = PortableStartupMessage(IntPtr.Zero, "Paket portable tidak dapat dimulai. Ekstrak seluruh ZIP ke folder lokal yang dapat ditulis, tanpa link atau junction. Folder Data dan penanda paket harus valid. Instalasi lama tidak digunakan sebagai pengganti.", "PERTAMINA OTO · Uji pengembangan", 0x10);
            return;
        }
        AppDomain.CurrentDomain.UnhandledException += (_, e) => LocalDiagnostics.Record("runtime-unhandled", e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (_, e) => LocalDiagnostics.Record("task-unobserved", e.Exception);
        var launchArgs = printArgs.Skip(1).ToArray();
        var normalLaunch = ReaderActivation.IsNormalLaunch(launchArgs);
        var lifetimeAudit = false;
#if DEBUG
        lifetimeAudit = launchArgs.Length == 3 && launchArgs[0] == "--audit-reader-lifetime";
#endif
        // A restarted version still owns normal PDF activations after its handshake.
        // It must always complete that handshake instead of forwarding the restart command.
        using var readerActivation = normalLaunch || launchArgs.FirstOrDefault() == "--restart-ready" || lifetimeAudit
            ? new ReaderActivation(Environment.UserName + "|" + Path.GetFullPath(AppContext.BaseDirectory).ToUpperInvariant() + (lifetimeAudit ? "|lifetime-audit" : "")) : null;
        if (normalLaunch && readerActivation is { IsPrimary: false })
        {
            if (!readerActivation.ForwardAsync(ReaderActivation.LaunchPath(launchArgs)).GetAwaiter().GetResult())
            {
                Environment.ExitCode = 1;
                _ = PortableStartupMessage(IntPtr.Zero, "Jendela OTO belum dapat menerima dokumen. Tunggu sebentar lalu buka PDF kembali. Dokumen yang sudah terbuka tetap aman.", "PERTAMINA OTO", 0x10);
            }
            return;
        }
        ReaderBridge = readerActivation is { IsPrimary: true } ? readerActivation : null;
        WinRT.ComWrappersSupport.InitializeComWrappers();
        Application.Start(callback =>
        {
            SynchronizationContext.SetSynchronizationContext(
                new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread()));
            _ = new DesktopApplication();
        });
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "MessageBoxW", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern int PortableStartupMessage(IntPtr window, string text, string caption, uint type);
}

public sealed partial class DesktopApplication : Application
{
    private Window? window;

    internal DesktopApplication()
    {
        UnhandledException += (_, e) => LocalDiagnostics.Record("xaml-unhandled", e.Exception);
        // Do not mark unknown errors handled: application state may be unsafe.
        RequestedTheme = ApplicationTheme.Light;
        InitializeComponent();
#if DEBUG
        if (Environment.GetCommandLineArgs().Contains("--trace-layout"))
            DebugSettings.LayoutCycleTracingLevel = LayoutCycleTracingLevel.High;
#endif
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
#if DEBUG
        if (Environment.GetEnvironmentVariable("OTO_READER_TEST_MODE") == "1") MainWindow.PrepareOriginalFontAuditStorage();
        if (Environment.GetCommandLineArgs().Contains("--audit-reader-lifetime", StringComparer.Ordinal)) MainWindow.PrepareOriginalFontAuditStorage();
#endif
        if (Environment.GetCommandLineArgs().Contains("--self-test-flow"))
        {
            var testWindow = new MainWindow(); testWindow.TestWorkspaceFlow(); Exit(); return;
        }
        if (Environment.GetCommandLineArgs().Contains("--self-test-library"))
        {
            var testWindow = new MainWindow(); testWindow.TestLibraryStorage(); Exit(); return;
        }
        if (Environment.GetCommandLineArgs().Contains("--self-test-split"))
        {
            await SplitSelfTest.RunAsync();
            Exit(); return;
        }
        if (Environment.GetCommandLineArgs().Contains("--self-test-jpg"))
        {
            var testWindow = new MainWindow();
            await testWindow.TestJpgExportAsync();
            Exit(); return;
        }
#if DEBUG
        if (Environment.GetCommandLineArgs().Contains("--audit-release-notes", StringComparer.Ordinal) || Environment.GetCommandLineArgs().Contains("--audit-about", StringComparer.Ordinal) || Environment.GetCommandLineArgs().Contains("--audit-update-completion", StringComparer.Ordinal) || Environment.GetCommandLineArgs().Contains("--audit-restart-parent", StringComparer.Ordinal)
            || Environment.GetCommandLineArgs().Contains("--audit-startup-window", StringComparer.Ordinal)
            || Environment.GetCommandLineArgs().Contains("--audit-online-update", StringComparer.Ordinal)
            || Environment.GetEnvironmentVariable("OTO_STARTUP_WINDOW_AUDIT") == "1"
            || Environment.GetEnvironmentVariable("OTO_RESTART_NATIVE_FIXTURE") == "1") MainWindow.PrepareRestartAuditStorage();
#endif
#if DEBUG
        if (Environment.GetCommandLineArgs().Contains("--audit-word", StringComparer.Ordinal)) MainWindow.PrepareWordAuditStorage();
        if (Environment.GetCommandLineArgs().Contains("--audit-window-size", StringComparer.Ordinal)
            || Environment.GetCommandLineArgs().Contains("--audit-print", StringComparer.Ordinal)
            || Environment.GetCommandLineArgs().Contains("--audit-result-actions", StringComparer.Ordinal)
            || Environment.GetCommandLineArgs().Contains("--audit-ui-regression", StringComparer.Ordinal)
            || Environment.GetCommandLineArgs().Contains("--audit-responsive-preview", StringComparer.Ordinal)
            || Environment.GetCommandLineArgs().Contains("--audit-responsive", StringComparer.Ordinal)
            || Environment.GetCommandLineArgs().Contains("--audit-zoom-smoothness", StringComparer.Ordinal)
            || Environment.GetCommandLineArgs().Contains("--audit-zoom", StringComparer.Ordinal)
            || Environment.GetCommandLineArgs().Contains("--audit-reader-ui", StringComparer.Ordinal)
            || Environment.GetCommandLineArgs().Contains("--audit-reader-tabs", StringComparer.Ordinal)
            || Environment.GetCommandLineArgs().Contains("--audit-font-compatibility", StringComparer.Ordinal)
            || Environment.GetCommandLineArgs().Contains("--audit-live-editor", StringComparer.Ordinal)) MainWindow.PrepareOriginalFontAuditStorage();
#endif
#if DEBUG
        var homeAuditArgs = Environment.GetCommandLineArgs();
        var transitionAuditIndex = Array.FindIndex(homeAuditArgs, a => a is "--audit-feature-transition-dialog" or "--audit-feature-transition-routing" or "--audit-feature-transition-session");
        if (transitionAuditIndex >= 0 && transitionAuditIndex + 1 < homeAuditArgs.Length)
            MainWindow.PrepareMergePanelAudit(homeAuditArgs[transitionAuditIndex + 1]);
        var homeAuditIndex = Array.IndexOf(homeAuditArgs, "--audit-centered-home");
        var catalogWidthAuditIndex = Array.IndexOf(homeAuditArgs, "--audit-catalog-content-width");
        var appExitAuditIndex = Array.IndexOf(homeAuditArgs, "--audit-app-exit");
        var organizerTransferAuditIndex = Array.IndexOf(homeAuditArgs, "--audit-organizer-transfer");
        var organizerDesignAuditIndex = Array.IndexOf(homeAuditArgs, "--audit-organizer-design");
        if (organizerDesignAuditIndex >= 0 && organizerDesignAuditIndex + 1 < homeAuditArgs.Length) MainWindow.PrepareMergePanelAudit(homeAuditArgs[organizerDesignAuditIndex + 1]);
        if (organizerTransferAuditIndex >= 0 && organizerTransferAuditIndex + 1 < homeAuditArgs.Length) MainWindow.PrepareMergePanelAudit(homeAuditArgs[organizerTransferAuditIndex + 1]);
        if (catalogWidthAuditIndex >= 0 && catalogWidthAuditIndex + 1 < homeAuditArgs.Length) MainWindow.PrepareCenteredAudit(homeAuditArgs[catalogWidthAuditIndex + 1]);
        if (appExitAuditIndex >= 0 && appExitAuditIndex + 1 < homeAuditArgs.Length) MainWindow.PrepareMergePanelAudit(homeAuditArgs[appExitAuditIndex + 1]);
        var mergePanelAuditIndex = Array.IndexOf(homeAuditArgs, "--audit-merge-panel");
        var savedResultAuditIndex = Array.IndexOf(homeAuditArgs, "--audit-saved-result-fit");
        var mergeEmptyAuditIndex = Array.IndexOf(homeAuditArgs, "--audit-merge-empty");
        var mergeCloseAuditIndex = Array.IndexOf(homeAuditArgs, "--audit-merge-close-flow");
        if (mergeCloseAuditIndex >= 0 && mergeCloseAuditIndex + 1 < homeAuditArgs.Length) MainWindow.PrepareMergePanelAudit(homeAuditArgs[mergeCloseAuditIndex + 1]);
        if (mergeEmptyAuditIndex >= 0 && mergeEmptyAuditIndex + 1 < homeAuditArgs.Length) MainWindow.PrepareMergeEmptyAudit(homeAuditArgs[mergeEmptyAuditIndex + 1]);
        if (savedResultAuditIndex >= 0 && savedResultAuditIndex + 1 < homeAuditArgs.Length) MainWindow.PrepareSavedResultAudit(homeAuditArgs[savedResultAuditIndex + 1]);
        if (mergePanelAuditIndex >= 0 && mergePanelAuditIndex + 1 < homeAuditArgs.Length) MainWindow.PrepareMergePanelAudit(homeAuditArgs[mergePanelAuditIndex + 1]);
        if (homeAuditIndex >= 0 && homeAuditIndex + 1 < homeAuditArgs.Length) MainWindow.PrepareCenteredAudit(homeAuditArgs[homeAuditIndex + 1]);
        var crashAuditIndex = Array.IndexOf(homeAuditArgs, "--audit-crash-stability");
        if (crashAuditIndex >= 0 && crashAuditIndex + 1 < homeAuditArgs.Length) MainWindow.PrepareCenteredAudit(homeAuditArgs[crashAuditIndex + 1]);
#endif
        window = new MainWindow();
        ((MainWindow)window).PrepareStartupWindow();
        window.Activate();
        if (Program.ReaderBridge is { } activation)
        {
            var queue = window.DispatcherQueue;
            activation.Start(path =>
            {
                // Acknowledge only after the UI queue accepts the request; opening may show a password dialog.
                var accepted = queue.TryEnqueue(async () =>
                {
                    try { await MainWindow.ReceiveReaderActivationAsync(path); }
                    catch (Exception error) { LocalDiagnostics.Record("reader-activation", error); }
                });
                return Task.FromResult(accepted);
            });
        }
        var arguments = Environment.GetCommandLineArgs();
#if DEBUG
        var lifetimeIndex = Array.IndexOf(arguments, "--audit-reader-lifetime");
        if (lifetimeIndex >= 0 && lifetimeIndex + 2 < arguments.Length)
        {
            try { await ((MainWindow)window).AuditReaderLifetimeAsync(arguments[lifetimeIndex + 1], arguments[lifetimeIndex + 2]); }
            catch (Exception error)
            {
                LocalDiagnostics.Record("reader-lifetime-audit", error);
                Environment.ExitCode = 1; Exit();
            }
            return;
        }
#endif
        var restartIndex = Array.IndexOf(arguments, "--restart-ready");
        if (restartIndex >= 0)
        {
            var restore = ((MainWindow)window).FreezeRestartStartup();
            ((UIElement)window.Content).IsHitTestVisible = false;
            var accepted = restartIndex + 2 < arguments.Length && await RestartProtocol.AcceptAsync(arguments[restartIndex + 1], arguments[restartIndex + 2], MainWindow.RestartRunningVersion());
            ((UIElement)window.Content).IsHitTestVisible = true;
            restore();
            if (!accepted) Exit();
            else ((MainWindow)window).ScheduleReleaseNotesStartup();
#if DEBUG
            if (accepted && Environment.GetEnvironmentVariable("OTO_RESTART_NATIVE_FIXTURE") == "1")
            {
                if (window.AppWindow.Presenter is not Microsoft.UI.Windowing.OverlappedPresenter { State: Microsoft.UI.Windowing.OverlappedPresenterState.Maximized, HasTitleBar: true, IsResizable: true })
                { Environment.ExitCode = 1; Exit(); return; }
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "native-restart-committed.txt"), "Startup acknowledged, exact old process exited, new native instance committed; maximized with native title bar and resizing enabled."); Exit();
            }
#endif
            return;
        }
#if DEBUG
        if (homeAuditIndex >= 0 && homeAuditIndex + 1 < homeAuditArgs.Length) { await ((MainWindow)window).AuditCenteredHomeAsync(homeAuditArgs[homeAuditIndex + 1]); Exit(); return; }
        if (transitionAuditIndex >= 0 && transitionAuditIndex + 1 < homeAuditArgs.Length)
        {
            var evidence = homeAuditArgs[transitionAuditIndex + 1];
            try
            {
                for (var attempt = 0; attempt < 100 && ((UIElement)window.Content).XamlRoot is null; attempt++) await Task.Delay(50);
                if (((UIElement)window.Content).XamlRoot is null) throw new InvalidOperationException("Native audit window did not load.");
                switch (homeAuditArgs[transitionAuditIndex])
                {
                    case "--audit-feature-transition-dialog": await ((MainWindow)window).AuditFeatureTransitionDialogAsync(evidence); break;
                    case "--audit-feature-transition-routing": await ((MainWindow)window).AuditFeatureTransitionRoutingAsync(evidence); break;
                    default: await ((MainWindow)window).AuditFeatureTransitionSessionAsync(evidence); break;
                }
            }
            catch (Exception error)
            {
                LocalDiagnostics.Record("feature-transition-audit", error); Environment.ExitCode = 1;
                Directory.CreateDirectory(evidence);
                File.WriteAllText(Path.Combine(evidence, "failure.txt"), error.ToString());
            }
            finally { Exit(); }
            return;
        }
        if (catalogWidthAuditIndex >= 0 && catalogWidthAuditIndex + 1 < homeAuditArgs.Length) { await ((MainWindow)window).AuditCatalogContentWidthAsync(homeAuditArgs[catalogWidthAuditIndex + 1]); Exit(); return; }
        if (appExitAuditIndex >= 0 && appExitAuditIndex + 1 < homeAuditArgs.Length) { await ((MainWindow)window).AuditAppExitConfirmationAsync(homeAuditArgs[appExitAuditIndex + 1]); Exit(); return; }
        if (organizerDesignAuditIndex >= 0 && organizerDesignAuditIndex + 1 < homeAuditArgs.Length)
        {
            var interactive = homeAuditArgs.Contains("--interactive-organizer", StringComparer.Ordinal);
            try { await ((MainWindow)window).AuditOrganizerDesignAsync(homeAuditArgs[organizerDesignAuditIndex + 1], interactive); }
            catch (Exception error) { LocalDiagnostics.Record("organizer-design-audit", error); Environment.ExitCode = 1; }
            finally { if (!interactive || Environment.ExitCode != 0) Exit(); }
            return;
        }
        if (organizerTransferAuditIndex >= 0 && organizerTransferAuditIndex + 1 < homeAuditArgs.Length)
        {
            try { await ((MainWindow)window).AuditOrganizerTransferAsync(homeAuditArgs[organizerTransferAuditIndex + 1]); }
            catch (Exception error) { LocalDiagnostics.Record("organizer-transfer-audit", error); Environment.ExitCode = 1; }
            finally { Exit(); }
            return;
        }
        if (mergeCloseAuditIndex >= 0 && mergeCloseAuditIndex + 1 < homeAuditArgs.Length) { await ((MainWindow)window).AuditMergeCloseFlowAsync(homeAuditArgs[mergeCloseAuditIndex + 1]); Exit(); return; }
        if (savedResultAuditIndex >= 0 && savedResultAuditIndex + 1 < homeAuditArgs.Length) { await ((MainWindow)window).AuditSavedResultViewportAsync(homeAuditArgs[savedResultAuditIndex + 1]); Exit(); return; }
        if (mergeEmptyAuditIndex >= 0 && mergeEmptyAuditIndex + 1 < homeAuditArgs.Length) { await ((MainWindow)window).AuditMergeEmptyAsync(homeAuditArgs[mergeEmptyAuditIndex + 1]); Exit(); return; }
        if (mergePanelAuditIndex >= 0 && mergePanelAuditIndex + 1 < homeAuditArgs.Length) { await ((MainWindow)window).AuditMergeResultPanelAsync(homeAuditArgs[mergePanelAuditIndex + 1]); if (!homeAuditArgs.Contains("--interactive-merge", StringComparer.Ordinal)) Exit(); return; }
        if (crashAuditIndex >= 0 && crashAuditIndex + 1 < homeAuditArgs.Length) { await ((MainWindow)window).AuditCrashStabilityAsync(homeAuditArgs[crashAuditIndex + 1]); Exit(); return; }
        if (arguments.Contains("--audit-window-size", StringComparer.Ordinal)) { await ((MainWindow)window).AuditWindowSizeAsync(); Exit(); return; }
        if (arguments.Contains("--audit-print", StringComparer.Ordinal)) { await ((MainWindow)window).AuditPrintAsync(); Exit(); return; }
        if (arguments.Contains("--audit-result-actions", StringComparer.Ordinal)) { await ((MainWindow)window).AuditResultActionsAsync(); Exit(); return; }
        if (arguments.Contains("--audit-startup-window", StringComparer.Ordinal))
        { await ((MainWindow)window).AuditStartupWindowAsync(); Exit(); return; }
        if (arguments.Contains("--audit-update-completion", StringComparer.Ordinal))
        { await ((MainWindow)window).AuditUpdateCompletionAsync(); Exit(); return; }
        var restartAuditIndex = Array.IndexOf(arguments, "--audit-restart-parent");
        if (restartAuditIndex >= 0 && restartAuditIndex + 2 < arguments.Length)
        { await ((MainWindow)window).AuditNativeRestartAsync(arguments[restartAuditIndex + 1], arguments[restartAuditIndex + 2]); return; }
        var wordAuditIndex = Array.IndexOf(arguments, "--audit-word");
        if (wordAuditIndex >= 0 && wordAuditIndex + 1 < arguments.Length) { await ((MainWindow)window).AuditWordAsync(arguments[wordAuditIndex + 1]); Exit(); return; }
        if (arguments.Contains("--audit-release-notes", StringComparer.Ordinal)) { await ((MainWindow)window).AuditReleaseNotesAsync(); Exit(); return; }
        if (arguments.Contains("--audit-about", StringComparer.Ordinal))
        {
            await ((MainWindow)window).AuditAboutDrawerAsync(); Exit(); return;
        }
        if (arguments.Contains("--audit-read-previews", StringComparer.Ordinal))
        {
            await ((MainWindow)window).AuditReadPreviewsAsync(); Exit(); return;
        }
        if (arguments.Contains("--audit-zoom-smoothness", StringComparer.Ordinal))
        { await ((MainWindow)window).AuditZoomSmoothnessAsync(); Exit(); return; }
        if (arguments.Contains("--audit-quality", StringComparer.Ordinal))
        {
            await ((MainWindow)window).AuditQualityAsync(); Exit(); return;
        }
        if (arguments.Contains("--audit-online-update", StringComparer.Ordinal))
        {
            await ((MainWindow)window).AuditOnlineUpdateAsync(); Exit(); return;
        }
        if (arguments.Contains("--audit-reader-ui", StringComparer.Ordinal))
        {
            await ((MainWindow)window).AuditReaderUiAsync(); Exit(); return;
        }
        var readerTabsAuditIndex = Array.IndexOf(arguments, "--audit-reader-tabs");
        if (readerTabsAuditIndex >= 0 && readerTabsAuditIndex + 1 < arguments.Length)
        {
            await ((MainWindow)window).AuditReaderTabsAsync(arguments[readerTabsAuditIndex + 1]); Exit(); return;
        }
        if (arguments.Contains("--audit-ui-regression", StringComparer.Ordinal))
        { var interactive = arguments.Contains("--interactive", StringComparer.Ordinal); await ((MainWindow)window).AuditUiRegressionAsync(interactive); if (!interactive) Exit(); return; }
        if (arguments.Contains("--audit-responsive-preview", StringComparer.Ordinal))
        { await ((MainWindow)window).AuditResponsivePreviewAsync(); return; }
        if (arguments.Contains("--audit-responsive-dialogs", StringComparer.Ordinal))
        { await ((MainWindow)window).AuditResponsiveDialogsAsync(); Exit(); return; }
        if (arguments.Contains("--audit-responsive", StringComparer.Ordinal))
        { var interactive = arguments.Contains("--interactive", StringComparer.Ordinal); await ((MainWindow)window).AuditResponsiveAsync(interactive); if (!interactive) Exit(); return; }
        if (arguments.Contains("--audit-zoom", StringComparer.Ordinal))
        { var interactive = arguments.Contains("--interactive", StringComparer.Ordinal); await ((MainWindow)window).AuditZoomAsync(interactive); if (!interactive) Exit(); return; }
        var liveAuditIndex = Array.IndexOf(arguments, "--audit-live-editor");
        var scrollAuditIndex = Array.IndexOf(arguments, "--audit-editor-scroll");
        if (scrollAuditIndex >= 0 && scrollAuditIndex + 1 < arguments.Length)
        {
            await ((MainWindow)window).AuditEditorScrollAsync(arguments[scrollAuditIndex + 1]); Exit(); return;
        }
        var fontAuditIndex = Array.IndexOf(arguments, "--audit-font-compatibility");
        var corpusAuditIndex = Array.IndexOf(arguments, "--audit-original-corpus");
        if (corpusAuditIndex >= 0 && corpusAuditIndex + 1 < arguments.Length && Environment.GetEnvironmentVariable("OTO_READER_TEST_MODE") == "1")
        {
            await ((MainWindow)window).AuditOriginalCorpusAsync(arguments[corpusAuditIndex + 1]); Exit(); return;
        }
        if (fontAuditIndex >= 0 && fontAuditIndex + 1 < arguments.Length)
        {
            await ((MainWindow)window).AuditOriginalFontCompatibilityAsync(arguments[fontAuditIndex + 1]); Exit(); return;
        }
        if (liveAuditIndex >= 0 && liveAuditIndex + 1 < arguments.Length)
        {
            await ((MainWindow)window).AuditLiveEditorAsync(arguments[liveAuditIndex + 1]); Exit(); return;
        }
        if (arguments.Contains("--open-editor", StringComparer.Ordinal))
        {
            ((MainWindow)window).OpenEditorScreen(); return;
        }
        if (arguments.Contains("--audit-editor-ui", StringComparer.Ordinal))
        {
            await ((MainWindow)window).AuditEditorDesignAsync(); Exit(); return;
        }
        if (arguments.Contains("--preview-editor-ui", StringComparer.Ordinal))
        {
            ((MainWindow)window).ShowPdfEditorDesignPreview(); return;
        }
        if (arguments.Contains("--audit-favorites-ui", StringComparer.Ordinal))
        {
            await ((MainWindow)window).AuditFavoritesUiAsync();
            Exit(); return;
        }
        var historyAuditIndex = Array.IndexOf(arguments, "--audit-history-ui");
        if (historyAuditIndex >= 0 && historyAuditIndex + 1 < arguments.Length)
        {
            await ((MainWindow)window).AuditHistoryUiAsync(arguments[historyAuditIndex + 1]);
            Exit(); return;
        }
        var organizerTestIndex = Array.IndexOf(arguments, "--self-test-organizer");
        if (organizerTestIndex >= 0 && organizerTestIndex + 1 < arguments.Length)
        {
            await ((MainWindow)window).TestOrganizerRevisionAsync(arguments[organizerTestIndex + 1]);
            Exit(); return;
        }
#endif
        if (!Program.ReaderInteractiveTest) ((MainWindow)window).ScheduleReleaseNotesStartup();
        var readerIndex = Array.IndexOf(arguments, "--open-reader");
        if (readerIndex >= 0 && readerIndex + 1 < arguments.Length)
        {
            await ((MainWindow)window).OpenPdfReaderAsync(arguments[readerIndex + 1]);
#if DEBUG
            if (Environment.GetEnvironmentVariable("OTO_STARTUP_WINDOW_AUDIT") == "1") { await ((MainWindow)window).AuditStartupWindowAsync(); Exit(); }
#endif
            return;
        }
        if (arguments.Length == 2 && Path.IsPathFullyQualified(arguments[1]) && Path.GetExtension(arguments[1]).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            await ((MainWindow)window).OpenPdfReaderAsync(arguments[1]);
#if DEBUG
            if (Environment.GetEnvironmentVariable("OTO_STARTUP_WINDOW_AUDIT") == "1") { await ((MainWindow)window).AuditStartupWindowAsync(); Exit(); }
#endif
            return;
        }
        var splitIndex = Array.IndexOf(arguments, "--open-split");
        if (splitIndex >= 0 && splitIndex + 1 < arguments.Length && File.Exists(arguments[splitIndex + 1]))
            await ((MainWindow)window).OpenSplitFileAsync(arguments[splitIndex + 1]);
#if DEBUG
        var organizerIndex = Array.IndexOf(arguments, "--open-organizer");
        if (organizerIndex >= 0)
            await ((MainWindow)window).OpenOrganizerFilesAsync(arguments.Skip(organizerIndex + 1));
#endif
    }
}
