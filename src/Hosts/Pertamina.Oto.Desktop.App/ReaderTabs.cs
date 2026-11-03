using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;
using System.Runtime.InteropServices;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private static readonly List<MainWindow> readerWindows = [];
    private static MainWindow? lastReaderWindow;
    private readonly bool detachedReaderWindow;
    // Serialize deduplication and reservations across every reader window.
    private static readonly SemaphoreSlim readerOpenGate = new(1, 1);
    private static readonly LinkedList<PdfReaderView> warmReaders = new();
    private const long WarmReaderBudget = 64L * 1024 * 1024;
    private TabView? readerTabs;
    private readonly Grid readerSurface = new();
    private bool changingReaderTabs;
    private bool readerWindowClosed;
    private bool readerTransfer;
    private MainWindow? readerTearOutWindow;

    private IEnumerable<PdfReaderView> Readers => readerTabs?.TabItems.OfType<TabViewItem>()
        .Select(t => t.Tag).OfType<PdfReaderView>() ?? [];

    private void RegisterReaderWindow()
    {
        if (detachedReaderWindow) windowSizePolicy = new WindowSizePolicy(this, (FrameworkElement)Content);
        readerWindows.Add(this);
        Activated += (_, e) => { if (e.WindowActivationState != WindowActivationState.Deactivated) lastReaderWindow = this; };
    }

    private void DisposeReaderWindow()
    {
        readerWindowClosed = true;
        readerWindows.Remove(this);
        if (lastReaderWindow == this) lastReaderWindow = readerWindows.LastOrDefault();
        foreach (var reader in Readers.ToArray()) reader.Dispose();
        readerSurface.Children.Clear();
        readerTabs?.TabItems.Clear(); pdfReader = null;
    }

    private void EnsureReaderTabs(bool createInitialTab = true)
    {
        if (readerTabs is not null) { if (createInitialTab && readerTabs.TabItems.Count == 0) AddReaderTab(); return; }
        readerTabs = new TabView
        {
            TabWidthMode = TabViewWidthMode.SizeToContent,
            CanReorderTabs = true, CanTearOutTabs = true,
            HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top
        };
        AutomationProperties.SetName(readerTabs, "Tab dokumen PDF");
        readerTabs.AddTabButtonClick += async (_, _) => { if (!messageOpen && !NonReaderBusy && pdfReader?.busy != true && pdfReader?.openingReaderPath is null) { ShowPdfReader(false); if (pdfReader?.ReaderSource is not null) AddReaderTab(); await pdfReader!.PickAsync(); } };
        readerTabs.TabCloseRequested += (_, e) => { if (e.Tab.Tag is PdfReaderView reader) CloseReaderTab(reader); };
        readerTabs.SelectionChanged += (_, _) => SelectReaderTab();
        readerTabs.TabTearOutWindowRequested += (_, e) =>
        {
            TraceReaderTearOut("window-request", e.Tabs);
            if (e.Tabs.OfType<TabViewItem>().Any(t => t.Tag is not PdfReaderView r || !CanMoveReader(r))) return;
            var destination = readerTearOutWindow = new MainWindow(detachedReader: true);
            destination.ShowPdfReader(false);
            e.NewWindowId = destination.AppWindow.Id;
        };
        readerTabs.TabTearOutRequested += (_, e) =>
        {
            TraceReaderTearOut("transfer-request", e.Tabs);
            var destination = readerTearOutWindow ?? readerWindows.FirstOrDefault(w => w.AppWindow.Id == e.NewWindowId);
            if (destination is null) return;
            foreach (var tab in e.Tabs.OfType<TabViewItem>().ToArray())
                if (tab.Tag is PdfReaderView reader) reader.owner.MoveReaderTab(reader, destination);
            readerTearOutWindow = null;
            destination.Activate();
        };
        readerTabs.ExternalTornOutTabsDropping += (_, e) => e.AllowDrop = !messageOpen && !NonReaderBusy &&
            e.Tabs.OfType<TabViewItem>().All(t => t.Tag is PdfReaderView r && r.owner.CanMoveReader(r));
        readerTabs.ExternalTornOutTabsDropped += (_, e) =>
        {
            foreach (var tab in e.Tabs.OfType<TabViewItem>().ToArray())
                if (tab.Tag is PdfReaderView reader) reader.owner.MoveReaderTab(reader, this, e.DropIndex);
        };
        AddReaderShortcut(VirtualKey.W, VirtualKeyModifiers.Control, () => { if (pdfReader is not null) CloseReaderTab(pdfReader); });
        AddReaderShortcut(VirtualKey.Tab, VirtualKeyModifiers.Control, () => CycleReaderTab(1));
        AddReaderShortcut(VirtualKey.Tab, VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift, () => CycleReaderTab(-1));
        if (createInitialTab) AddReaderTab();
    }

    private static void TraceReaderTearOut(string stage, IEnumerable<object> tabs)
    {
#if DEBUG
        if (Program.ReaderInteractiveTest)
            File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "reader-native-audit-events.txt"), stage + ": " +
                string.Join(",", tabs.Select(t => t is TabViewItem { Tag: PdfReaderView r }
                    ? $"reader canMove={r.owner.CanMoveReader(r)} busy={r.busy} ownerClosed={r.owner.readerWindowClosed}"
                    : t.GetType().Name)) + Environment.NewLine);
#endif
    }

    private void AddReaderShortcut(VirtualKey key, VirtualKeyModifiers modifiers, Action action)
    {
        var shortcut = new KeyboardAccelerator { Key = key, Modifiers = modifiers };
        shortcut.Invoked += (_, e) =>
        {
            if (messageOpen || readerTabs?.IsLoaded != true || historyPage.Visibility != Visibility.Visible) return;
            e.Handled = true; action();
        };
        // The persistent reading surface is a sibling of the tab strip.
        historyPage.KeyboardAccelerators.Add(shortcut);
    }

    private PdfReaderView AddReaderTab()
    {
        var reader = new PdfReaderView(this);
        reader.View.Visibility = Visibility.Collapsed;
        readerSurface.Children.Add(reader.View);
        var tab = new TabViewItem { Header = "Buka PDF", Tag = reader,
            IconSource = new SymbolIconSource { Symbol = Symbol.Document } };
        var menu = new MenuFlyout();
        var detach = new MenuFlyoutItem { Text = "Pindahkan ke jendela baru" };
        detach.Click += (_, _) => reader.owner.DetachReaderTab(reader);
        menu.Items.Add(detach);
        var join = new MenuFlyoutSubItem { Text = "Pindahkan ke jendela OTO" }; menu.Items.Add(join);
        var close = new MenuFlyoutItem { Text = "Tutup tab", KeyboardAcceleratorTextOverride = "Ctrl+W" };
        close.Click += (_, _) => reader.owner.CloseReaderTab(reader); menu.Items.Add(close);
        menu.Opening += (_, _) =>
        {
            detach.IsEnabled = reader.ReaderSource is not null && reader.owner.CanMoveReader(reader);
            join.Items.Clear();
            foreach (var destination in readerWindows.Where(w => w != reader.owner && !w.readerWindowClosed).ToArray())
            {
                var item = new MenuFlyoutItem { Text = destination.pdfReader?.ReaderSource is { } file ? Path.GetFileName(file) : "OTO · Pembaca PDF",
                    IsEnabled = reader.owner.CanMoveReader(reader) && !destination.messageOpen && !destination.NonReaderBusy };
                item.Click += (_, _) => { reader.owner.MoveReaderTab(reader, destination); destination.Activate(); };
                join.Items.Add(item);
            }
            join.IsEnabled = join.Items.Count > 0;
        };
        tab.ContextFlyout = menu;
        tab.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler((_, e) =>
        {
            if (!e.GetCurrentPoint(tab).Properties.IsLeftButtonPressed) return;
            // TabView's native tear-out path can consume Tapped, including for an
            // already selected tab. Observe pointer input without marking it handled.
            // The close button still owns its own action.
            for (var element = e.OriginalSource as DependencyObject; element is not null && element != tab; element = VisualTreeHelper.GetParent(element))
                if (element is Button) return;
            if (!reader.owner.ReaderRouteVisible) reader.owner.ActivateReaderTabFromHeader(reader);
        }), true);
        tab.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler((_,e) =>
        {
            if (e.Key is VirtualKey.Enter or VirtualKey.Space && !reader.owner.ReaderRouteVisible)
                reader.owner.ActivateReaderTabFromHeader(reader);
        }), true);
        readerTabs!.TabItems.Add(tab); readerTabs.SelectedItem = tab;
        SelectReaderTab();
        return reader;
    }

    private void SelectReaderTab()
    {
        if (changingReaderTabs || readerWindowClosed) return;
        var nextReader = (readerTabs?.SelectedItem as TabViewItem)?.Tag as PdfReaderView;
        if (nextReader == pdfReader)
        {
            if (nextReader is not null && !ReaderRouteVisible && !messageOpen && !NonReaderBusy && !nextReader.busy && nextReader.openingReaderPath is null)
            { ShowPdfReader(false); nextReader.Resume(); nextReader.FocusReading(); }
            return;
        }
        if (messageOpen || NonReaderBusy || pdfReader?.busy == true || pdfReader?.openingReaderPath is not null)
        {
            changingReaderTabs = true;
            try { readerTabs!.SelectedItem = FindReaderTab(pdfReader); }
            finally { changingReaderTabs = false; }
            return;
        }
        // The destination is about to become active, so do not evict its warm
        // image when admitting the tab we are leaving to the inactive cache.
        if (nextReader is not null) warmReaders.Remove(nextReader);
        if (pdfReader is { } previousReader)
        {
            previousReader.Deactivate();
            previousReader.View.Visibility = Visibility.Collapsed;
        }
        pdfReader = nextReader;
        if (nextReader is null) return;
        if (!ReaderRouteVisible && nextReader.ReaderSource is not null) ShowPdfReader(false);
        nextReader.View.Visibility = Visibility.Visible;
        nextReader.Resume();
        DispatcherQueue.TryEnqueue(() => { if (pdfReader == nextReader && !readerWindowClosed) { nextReader.Resume(); nextReader.FocusReading(); } });
    }

    private TabViewItem? FindReaderTab(PdfReaderView? reader) => readerTabs?.TabItems.OfType<TabViewItem>().FirstOrDefault(t => ReferenceEquals(t.Tag, reader));

    private void UpdateReaderTab(PdfReaderView reader)
    {
        if (FindReaderTab(reader) is not { } tab) return;
        tab.Header = reader.ReaderSource is { } path ? Path.GetFileName(path) : "Buka PDF";
        ToolTipService.SetToolTip(tab, reader.ReaderSource ?? "Buka dokumen PDF");
        AutomationProperties.SetName(tab, tab.Header.ToString()!);
    }

    private void CycleReaderTab(int direction)
    {
        if (readerTabs is null || readerTabs.TabItems.Count < 2 || messageOpen) return;
        readerTabs.SelectedIndex = (readerTabs.SelectedIndex + direction + readerTabs.TabItems.Count) % readerTabs.TabItems.Count;
    }

    private void CloseReaderTab(PdfReaderView reader)
    {
        if (messageOpen || reader.savingCopy || FindReaderTab(reader) is not { } tab) return;
        if (readerTabs!.TabItems.Count == 1 && !detachedReaderWindow)
        {
            reader.ResetReaderDocument(); UpdateReaderTab(reader); return;
        }
        reader.Deactivate();
        changingReaderTabs = true;
        try { readerTabs.TabItems.Remove(tab); readerSurface.Children.Remove(reader.View); reader.Dispose(); if (pdfReader == reader) pdfReader = null; }
        finally { changingReaderTabs = false; }
        if (readerTabs.TabItems.Count == 0)
        {
            if (detachedReaderWindow && !NonReaderBusy && RestartPendingWork().Count == 0)
            { allowClose = true; Close(); }
            else AddReaderTab();
        }
        else SelectReaderTab();
    }

    private bool CanMoveReader(PdfReaderView reader) => !messageOpen && !NonReaderBusy && !reader.busy && !readerTransfer &&
        !readerWindowClosed && reader.ReaderSource is not null && FindReaderTab(reader) is not null;

    private void DetachReaderTab(PdfReaderView reader)
    {
        if (!CanMoveReader(reader)) return;
        var destination = new MainWindow(detachedReader: true);
        destination.AppWindow.Move(new Windows.Graphics.PointInt32(AppWindow.Position.X + 40, AppWindow.Position.Y + 40));
        MoveReaderTab(reader, destination); destination.Activate();
    }

    private void MoveReaderTab(PdfReaderView reader, MainWindow destination, int index = -1)
    {
        if (destination == this || !CanMoveReader(reader) || destination.messageOpen || destination.NonReaderBusy || destination.readerWindowClosed) return;
        var tab = FindReaderTab(reader)!;
        destination.ShowPdfReader(false);
        readerTransfer = destination.readerTransfer = true;
        changingReaderTabs = destination.changingReaderTabs = true;
        try
        {
            reader.Deactivate(); reader.View.Visibility = Visibility.Collapsed;
            warmReaders.Remove(reader);
            readerSurface.Children.Remove(reader.View);
            readerTabs!.TabItems.Remove(tab);
            foreach (var blank in destination.Readers.Where(r => r.ReaderSource is null && !r.busy).ToArray())
            { destination.readerTabs!.TabItems.Remove(destination.FindReaderTab(blank)); destination.readerSurface.Children.Remove(blank.View); blank.Dispose(); }
            if (destination.pdfReader is { } previousDestination)
            { previousDestination.Deactivate(); previousDestination.View.Visibility = Visibility.Collapsed; }
            if (reader.ReaderSource is { } path && readerResults.Contains(path)) destination.readerResults.Add(path);
            reader.RebindOwner(destination);
            destination.readerSurface.Children.Add(reader.View);
            destination.readerTabs!.TabItems.Insert(index < 0 ? destination.readerTabs.TabItems.Count : Math.Clamp(index, 0, destination.readerTabs.TabItems.Count), tab);
            destination.readerTabs.SelectedItem = tab;
            destination.pdfReader = reader;
            warmReaders.Remove(reader); reader.View.Visibility = Visibility.Visible;
            if (pdfReader == reader) pdfReader = null;
        }
        finally
        { changingReaderTabs = destination.changingReaderTabs = false; readerTransfer = destination.readerTransfer = false; }
        if (readerTabs!.TabItems.Count == 0)
        {
            if (detachedReaderWindow && RestartPendingWork().Count == 0) { allowClose = true; Close(); }
            else AddReaderTab();
        }
        else SelectReaderTab();
        destination.DispatcherQueue.TryEnqueue(() => { reader.Resume(); reader.FocusReading(); });
    }

    private static (MainWindow Window, PdfReaderView Reader)? FindOpenReader(string path)
    {
        foreach (var window in readerWindows)
            foreach (var reader in window.Readers)
                if (string.Equals(reader.ReaderSource ?? reader.openingReaderPath, path, StringComparison.OrdinalIgnoreCase)) return (window, reader);
        return null;
    }

    internal static async Task<bool> ReceiveReaderActivationAsync(string? path)
    {
        var full = path is null ? null : Path.GetFullPath(path);
        var existing = full is null ? null : FindOpenReader(full);
        var destination = existing?.Window ?? lastReaderWindow ?? readerWindows.LastOrDefault();
        if (destination is null || destination.readerWindowClosed) return false;
        destination.BringReaderWindowToFront();
        if (full is null) return true;
        if (existing is not null)
        {
            // A document already open (or opening) belongs to this window even
            // when a dialog is temporarily blocking it. Never make a duplicate.
            await destination.OpenPdfReaderAsync(full);
            return true;
        }
        if (destination.messageOpen || destination.NonReaderBusy)
        { destination = new MainWindow(detachedReader: true); destination.BringReaderWindowToFront(); }
        await destination.OpenPdfReaderAsync(full);
        return true;
    }

    private void BringReaderWindowToFront()
    {
        if (readerWindowClosed) return;
        var handle = WinRT.Interop.WindowNative.GetWindowHandle(this);
        if (ReaderIsIconic(handle)) _ = ReaderShowWindow(handle, 9); // SW_RESTORE, only if minimized.
        Activate();
        if (ReaderSetForegroundWindow(handle)) return;
        // Respect Windows foreground policy. No topmost/focus-stealing loop.
        var flash = new ReaderFlashInfo { Size = (uint)Marshal.SizeOf<ReaderFlashInfo>(), Window = handle, Flags = 2, Count = 3 };
        _ = ReaderFlashWindowEx(ref flash);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ReaderFlashInfo { internal uint Size; internal IntPtr Window; internal uint Flags; internal uint Count; internal uint Timeout; }
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReaderSetForegroundWindow(IntPtr window);
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("user32.dll", EntryPoint = "IsIconic", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReaderIsIconic(IntPtr window);
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("user32.dll", EntryPoint = "ShowWindow", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReaderShowWindow(IntPtr window, int command);
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("user32.dll", EntryPoint = "FlashWindowEx", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReaderFlashWindowEx(ref ReaderFlashInfo info);

    private sealed partial class PdfReaderView
    {
        private static void RetainInactiveReader(PdfReaderView reader)
        {
            warmReaders.Remove(reader);
            reader.TrimInactivePageBitmaps();
            if (!reader.disposed && reader.document is not null) warmReaders.AddLast(reader);
            while (warmReaders.Count > 2 || warmReaders.Sum(r => r.RetainedRasterBytes) > WarmReaderBudget)
            {
                var oldest = warmReaders.First!.Value; warmReaders.RemoveFirst();
                oldest.ClearPageBitmaps(); oldest.thumbnails.Children.Clear();
            }
        }

        internal string? openingReaderPath;
        internal bool savingCopy;
        private bool disposed;
        internal void RebindOwner(MainWindow destination)
        {
            owner.Activated -= LandingOwnerActivated;
            owner = destination;
            if (!previewOnly) owner.Activated += LandingOwnerActivated;
        }
        internal void FocusReading() { if (Visible && document is not null) viewport.Focus(FocusState.Programmatic); }

        private async Task NavigateReaderKeyAsync(int direction) => await GoAsync(pageIndex + direction);

        private bool ReaderKeyAllowed(DependencyObject? focused)
        {
            if (!Visible || document is null || busy || owner.messageOpen) return false;
            if (VisualTreeHelper.GetOpenPopupsForXamlRoot(View.XamlRoot)
                .Any(p => p.Child is not null && Descendants(p.Child).Prepend(p.Child)
                    .Any(c => c is MenuFlyoutPresenter or ContentDialog or ComboBoxItem))) return false;
            for (var current = focused; current is not null && current != View; current = VisualTreeHelper.GetParent(current))
                if (current is TextBox or PasswordBox or ComboBox or ListViewBase or SelectorItem or NumberBox or Slider or MenuFlyoutPresenter) return false;
            return true;
        }
    }
}
