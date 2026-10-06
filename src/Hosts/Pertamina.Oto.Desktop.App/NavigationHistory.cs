using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed record Screen(string Title, Action Open, string? Tool);
    private sealed record ToolDraft(string[] Files, string Pages, string Text, string Output, int Rotation, double Margin, string Quality = "medium");
    private readonly List<Screen> screens = [];
    private readonly Dictionary<string, ToolDraft> toolDrafts = [];
    private int screenIndex = -1;
    private bool travelling;
    private bool allowClose;
    private bool closeCheckQueued;
    private Button? backButton;
    private Button? forwardButton;
    private string organizerOutput = "hasil-organize.pdf";

    private void SaveToolDraft()
    {
        if (screenIndex < 0 || travelling) return;
        if (screens[screenIndex].Title == "Atur halaman PDF") { if (activeToolControls == "organize" && !closedToolSessions.Contains("organize")) organizerOutput = outputName.Text; return; }
        if (screens[screenIndex].Tool is not { } id) return;
        if (closedToolSessions.Contains(id) || selected?.Id != id || activeToolControls != id) return;
        toolDrafts[id] = CurrentToolDraft();
        var secret = CurrentSessionSecret(id);
        if (secret.Password.Length > 0 || secret.NewPassword.Length > 0) toolSessionSecrets[id] = secret;
        else toolSessionSecrets.Remove(id);
        // Credentials remain separate session memory; never serialize them with navigation/history.
    }

    private void RestoreToolDraft(string id)
    {
        if (closedToolSessions.Contains(id) || !toolDrafts.TryGetValue(id, out var draft)) return;
        files.Clear(); foreach (var path in draft.Files) files.Add(path);
        pages.Text = draft.Pages; watermark.Text = draft.Text; outputName.Text = draft.Output;
        rotation.SelectedIndex = draft.Rotation; margin.Value = draft.Margin;
        outputQuality = draft.Quality;
        if (toolSessionSecrets.TryGetValue(id, out var secret))
        { if (id == "split") splitPassword.Password = secret.Password; else { password.Password = secret.Password; newPassword.Password = secret.NewPassword; } }
    }

    private void TrackScreen(string title)
    {
        if (travelling) return;
        if (screenIndex >= 0 && screens[screenIndex].Title == title) return;
        var tool = ToolCatalog.All.FirstOrDefault(t => t.Name == title || (title == "Gabung PDF" && t.Id == "merge"));
        Action? open = title switch
        {
            "Mau mengerjakan apa hari ini?" or "Dokumen Anda siap" => ShowHome,
            "Semua alat" or "Semua Alat" => ShowCatalog,
            "Pembaca PDF" => ShowPdfReader,
            "Atur halaman PDF" => ShowOrganizer,
            "Favorit" => ShowFavorites, "Riwayat sesi" or "Riwayat" => ShowHistory,
            "Aktivitas" => ShowActivity, "Bantuan" => ShowHelp,
            "Pengaturan dasar" => ShowSettings, "File Diamankan" => ShowSecured,
            "Assistant" => ShowAssistant, "Administrasi" => ShowAdmin, "Laporkan masalah" => ShowIssueReport,
            _ => tool is null ? null : () => ShowTool(tool)
        };
        if (open is null) open = ShowHome;
        if (screenIndex + 1 < screens.Count) screens.RemoveRange(screenIndex + 1, screens.Count - screenIndex - 1);
        screens.Add(new(title, open, tool?.Id));
        if (screens.Count > 50) screens.RemoveAt(0);
        screenIndex = screens.Count - 1; RefreshNavigation();
    }

    private void Travel(int direction)
    {
        if (active is not null || importing || messageOpen) return;
        var target = screenIndex + direction;
        if (target < 0 || target >= screens.Count) return;
        SaveToolDraft(); travelling = true;
        try { screenIndex = target; screens[target].Open(); }
        finally { travelling = false; RefreshNavigation(); }
    }

    private void RefreshNavigation()
    {
        if (backButton is not null) backButton.IsEnabled = screenIndex > 0;
        if (forwardButton is not null) forwardButton.IsEnabled = screenIndex >= 0 && screenIndex < screens.Count - 1;
    }

    private static Button NavigationButton(string glyph, string name, Action action)
    {
        var button = new Button { Content = new FontIcon { Glyph = glyph, FontSize = 16 },
            Width = 36, Height = 34, Padding = new Thickness(6), Background = Brush(255, 255, 255), BorderThickness = new Thickness(0) };
        ToolTipService.SetToolTip(button, name); AutomationProperties.SetName(button, name);
        button.Click += (_, _) => action(); return button;
    }

    private static void AddNavigationShortcut(UIElement owner, VirtualKey key, Action action)
    {
        var shortcut = new KeyboardAccelerator { Key = key, Modifiers = VirtualKeyModifiers.Menu };
        shortcut.Invoked += (_, e) => { e.Handled = true; action(); }; owner.KeyboardAccelerators.Add(shortcut);
    }

    private async Task ConfirmCloseAsync()
    {
        if (messageOpen) return;
        if (active is not null || importing || liveEditor?.Working == true)
        { await MenuMessageAsync("Pekerjaan sedang berjalan", "Batalkan atau tunggu pekerjaan selesai sebelum keluar."); return; }
        if (homeFiles.Count == 0 && files.Count == 0 && pageLayout.Count == 0 && toolDrafts.Count == 0 && liveEditor?.Dirty != true)
        { allowClose = true; Close(); return; }
        messageOpen = true;
        try
        {
            var dialog = new ContentDialog { XamlRoot = root.XamlRoot, Title = "Tutup sesi OTO?",
                Content = "Pilihan dokumen dan draf sesi tidak disimpan otomatis. Pastikan hasil yang diperlukan sudah disimpan. File asli tidak dihapus.",
                PrimaryButtonText = "Keluar", CloseButtonText = "Kembali", DefaultButton = ContentDialogButton.Close };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary) { allowClose = true; Close(); }
        }
        finally { messageOpen = false; }
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject parent)
    {
        var count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }

    private Task SelectWorkspaceAsync(bool select)
    {
        if (FocusManager.GetFocusedElement(root.XamlRoot) is TextBox text)
        { if (select) text.SelectAll(); else text.Select(text.SelectionStart, 0); return Task.CompletedTask; }
        if (!organizerVisible && !homeEntries.IsLoaded) return Task.CompletedTask;
        foreach (var checkbox in Descendants(body).OfType<CheckBox>()) checkbox.IsChecked = select;
        if (!organizerVisible)
        {
            chosenFiles.Clear(); if (select) chosenFiles.UnionWith(homeFiles);
            HomeNotice($"{chosenFiles.Count} file dipilih.");
        }
        return Task.CompletedTask;
    }

    private Task ZoomPreviewAsync(int delta)
    {
        if (!ZoomController.Route(FocusManager.GetFocusedElement(root.XamlRoot) as DependencyObject, Math.Sign(delta)))
            return MenuMessageAsync("Zoom preview", "Buka dan pilih preview dokumen terlebih dahulu. Zoom hanya mengubah tampilan.");
        return Task.CompletedTask;
    }
}
