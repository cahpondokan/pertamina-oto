using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private bool sidebarCollapsed;
    private bool messageOpen;

    private Grid BuildDesktopMenu()
    {
        var bar = new MenuBar { Background = Brush(255, 255, 255) };
        var file = new MenuBarItem { Title = "Berkas" };
        file.Items.Add(MenuCommand("Buka / tambah dokumen…", async () =>
        {
            if (liveEditor?.Visible == true) { await liveEditor.PickAsync(); return; }
            if (pdfReader?.Visible == true) { await pdfReader.PickAsync(); return; }
            if (emptyPanel?.IsLoaded == true) { await PickEmptyToolAsync(); return; }
            if (!organizerVisible) ShowHome();
            await ChooseHomeAsync();
        }, VirtualKey.O));
        file.Items.Add(MenuCommand("Proses dan simpan sebagai PDF baru…", async () =>
        {
            if (liveEditor?.Visible == true) { await liveEditor.SaveAsync(); return; }
            if (pdfReader?.Visible == true) { await pdfReader.SaveCopyAsync(); return; }
            if (selected?.Id == "split" && splitRun.IsLoaded) { await ExecuteSplitAsync(); return; }
            if (selected is not { Available: true } || !run.IsLoaded)
            { await MenuMessageAsync("Simpan hasil", "Pilih alat dan dokumen terlebih dahulu. Atur nama baru serta folder hasil di halaman alat."); return; }
            await ExecuteAsync();
        }, VirtualKey.S, true, true));
        file.Items.Add(MenuCommand("Cetak…", PrintActiveAsync, VirtualKey.P));
        file.Items.Add(MenuCommand("Buka hasil terakhir", async () =>
        {
            if (resultPath is null) { await MenuMessageAsync("Hasil", "Belum ada hasil pada sesi ini."); return; }
            try { await Launcher.LaunchFileAsync(await Windows.Storage.StorageFile.GetFileFromPathAsync(resultPath)); }
            catch (Exception) { await MenuMessageAsync("Hasil", "File hasil tidak dapat dibuka. Periksa folder penyimpanan Anda."); }
        }));
        file.Items.Add(new MenuFlyoutSeparator());
        file.Items.Add(MenuCommand("Keluar…", ConfirmCloseAsync));
        var edit = new MenuBarItem { Title = "Edit" };
        edit.Items.Add(MenuCommand("Urungkan perubahan dokumen", () => WorkspaceUndoAsync(false)));
        edit.Items.Add(MenuCommand("Ulangi perubahan dokumen", () => WorkspaceUndoAsync(true)));
        edit.Items.Add(MenuCommand("Tempel dokumen dari clipboard", PasteDocumentAnywhereAsync));
        edit.Items.Add(MenuCommand("Pilih semua dokumen / halaman", () => SelectWorkspaceAsync(true), VirtualKey.A));
        edit.Items.Add(MenuCommand("Batalkan pilihan", () => SelectWorkspaceAsync(false), VirtualKey.Escape, false));
        edit.Items.Add(new MenuFlyoutSeparator());
        edit.Items.Add(MenuCommand("Pengaturan", () => { ShowSettings(); return Task.CompletedTask; }));
        var view = new MenuBarItem { Title = "Tampilan" };

        view.Items.Add(MenuCommand("Beranda", () => { ShowHome(); return Task.CompletedTask; }));
        view.Items.Add(MenuCommand("Pembaca PDF", () => { ShowPdfReader(); return Task.CompletedTask; }));
        view.Items.Add(MenuCommand("Cari teks PDF / alat", () =>
        {
            if (pdfReader?.Visible == true) { pdfReader.FocusSearch(); return Task.CompletedTask; }
            ShowHome();
            centeredSearch?.Focus(FocusState.Programmatic);
            return Task.CompletedTask;
        }, VirtualKey.F));
        view.Items.Add(MenuCommand("Favorit", () => { ShowFavorites(); return Task.CompletedTask; }));
        view.Items.Add(MenuCommand("Riwayat sesi", () => { ShowHistory(); return Task.CompletedTask; }));
        view.Items.Add(new MenuFlyoutSeparator());
        view.Items.Add(MenuCommand("Perbesar preview", () => ZoomPreviewAsync(50), VirtualKey.Add));
        view.Items.Add(MenuCommand("Perkecil preview", () => ZoomPreviewAsync(-50), VirtualKey.Subtract));
        view.Items.Add(MenuCommand("Reset ukuran preview", () => ZoomPreviewAsync(0), VirtualKey.Number0));
        var help = new MenuBarItem { Title = "Bantuan" };
        help.Items.Add(MenuCommand("Panduan penggunaan", () => { ShowHelp(); return Task.CompletedTask; }, VirtualKey.F1, false));
        help.Items.Add(MenuCommand("Pintasan keyboard", () => MenuMessageAsync("Pintasan OTO",
            "Alt+← / Alt+→: kembali / maju\nCtrl+O: buka/tambah dokumen\nCtrl+Shift+S: proses dan simpan hasil baru\nCtrl+F: cari alat\nCtrl+V: tempel file di beranda/Organize\nCtrl+Z / Ctrl+Y: undo/redo dokumen\nCtrl+A / Esc: pilih semua / batalkan pilihan\nCtrl+Num + / Num −: zoom preview\nCtrl+0: reset preview\nCtrl+/: daftar pintasan\nF1: bantuan\n↑ / ↓ dan PgUp / PgDn: halaman PDF sebelumnya / berikutnya\nCtrl+Tab / Ctrl+Shift+Tab: pindah tab PDF\nCtrl+W: tutup tab PDF aktif\nAlt+F4: tutup jendela; pembaca tanpa draf langsung keluar\n\nSaat mengetik, pintasan teks mengikuti perilaku kotak teks Windows. File asli tidak dipotong/dihapus."), (VirtualKey)191));
        help.Items.Add(MenuCommand("Laporkan masalah", () => { ShowIssueReport(); return Task.CompletedTask; }));
        help.Items.Add(MenuCommand("Tentang OTO", () => { ShowSettings(); DispatcherQueue.TryEnqueue(() => ShowAboutDrawer(aboutSettingsButton)); return Task.CompletedTask; }));
        bar.Items.Add(file); bar.Items.Add(edit); bar.Items.Add(view); bar.Items.Add(help);
        var shell = new Grid { Background = Brush(255, 255, 255) };
        shell.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        shell.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var controls = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, Margin = new Thickness(8, 0, 8, 0) };

        backButton = NavigationButton("\uE72B", "Kembali · Alt+←", () => Travel(-1));
        forwardButton = NavigationButton("\uE72A", "Maju · Alt+→", () => Travel(1));
        controls.Children.Add(backButton); controls.Children.Add(forwardButton);
        AddNavigationShortcut(controls, VirtualKey.Left, () => Travel(-1));
        AddNavigationShortcut(controls, VirtualKey.Right, () => Travel(1));
        shell.Children.Add(controls); Grid.SetColumn(bar, 1); shell.Children.Add(bar); ResponsiveLayout.Header(shell, controls, bar, 500);
        return shell;
    }

    private MenuFlyoutItem MenuCommand(string title, Func<Task> action, VirtualKey key = VirtualKey.None, bool control = true, bool shift = false)
    {
        var item = new MenuFlyoutItem { Text = title };
        async Task Invoke()
        {
            if (active is not null || importing)
            { await MenuMessageAsync("Pekerjaan sedang berjalan", "Tunggu hingga selesai atau gunakan tombol Batalkan pada halaman alat."); return; }
            try { await action(); }
            catch (Exception) { await MenuMessageAsync("Perintah belum dapat dijalankan", "Coba kembali setelah menutup dialog lain. File asli tidak diubah oleh menu ini."); }
        }
        item.Click += async (_, _) => await Invoke();
        if (key != VirtualKey.None)
        {
            // WinUI fail-fasts when formatting OEM keys for menu labels or
            // automatic tooltips. Keep these global shortcuts on the hidden-
            // tooltip root, and leave their explicit labels on the menu items.
            var acceleratorOwner = key is VirtualKey.Add or VirtualKey.Subtract || (int)key == 191 ? (UIElement)root : item;
            if (acceleratorOwner == root)
            {
                root.KeyboardAcceleratorPlacementMode = KeyboardAcceleratorPlacementMode.Hidden;
                item.KeyboardAcceleratorTextOverride = key == VirtualKey.Add ? "Ctrl++" : key == VirtualKey.Subtract ? "Ctrl+-" : "Ctrl+/";
            }
            if (key is VirtualKey.Add or VirtualKey.Subtract)
            {
                var normal = new KeyboardAccelerator { Key = (VirtualKey)(key == VirtualKey.Add ? 187 : 189), Modifiers = VirtualKeyModifiers.Control };
                normal.Invoked += async (_, e) => { if (IsTextPasteTarget(FocusManager.GetFocusedElement(root.XamlRoot) as DependencyObject)) return; e.Handled = true; await Invoke(); };
                acceleratorOwner.KeyboardAccelerators.Add(normal);
                if (key == VirtualKey.Add)
                {
                    var shifted = new KeyboardAccelerator { Key = (VirtualKey)187, Modifiers = VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift };
                    shifted.Invoked += async (_, e) => { if (IsTextPasteTarget(FocusManager.GetFocusedElement(root.XamlRoot) as DependencyObject)) return; e.Handled = true; await Invoke(); };
                    acceleratorOwner.KeyboardAccelerators.Add(shifted);
                }
            }
            var shortcut = new KeyboardAccelerator { Key = key, Modifiers = (control ? VirtualKeyModifiers.Control : VirtualKeyModifiers.None) | (shift ? VirtualKeyModifiers.Shift : VirtualKeyModifiers.None) };
            shortcut.Invoked += async (_, e) =>
            {
                if (key is VirtualKey.Z or VirtualKey.Y && liveEditor?.Visible == true && liveEditor.Typing()) return;
                if (key is VirtualKey.Add or VirtualKey.Subtract or VirtualKey.Number0 && IsTextPasteTarget(FocusManager.GetFocusedElement(root.XamlRoot) as DependencyObject)) return;
                e.Handled = true; await Invoke();
            };
            acceleratorOwner.KeyboardAccelerators.Add(shortcut);
        }
        return item;
    }

    private Task WorkspaceUndoAsync(bool redo)
    {
        if (liveEditor?.Visible == true) { liveEditor.Restore(redo); return Task.CompletedTask; }
        if (organizerVisible) RestoreLayout(redo);
        else if (homeEntries.IsLoaded) RestoreHome(redo);
        else return MenuMessageAsync("Undo / redo dokumen", "Tersedia di Beranda dan Atur halaman PDF. Untuk teks, gunakan Ctrl+Z / Ctrl+Y saat kursor berada di kotak teks.");
        return Task.CompletedTask;
    }

    private async Task MenuMessageAsync(string title, string message)
    {
        if (messageOpen) return;
        messageOpen = true;
        try { await new ContentDialog { XamlRoot = root.XamlRoot, Title = title, Content = message, CloseButtonText = "Tutup" }.ShowAsync(); }
        catch (Exception error) { LocalDiagnostics.Record("message-dialog", error); }
        finally { messageOpen = false; }
    }
}
