using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // An ended session must not be recaptured from detached controls or history routes.
    private readonly HashSet<string> closedToolSessions = [];
    private readonly HashSet<string> removedToolInputs = [];
    private string? activeToolControls;
    private bool mergeCloseSaveInProgress;
    private sealed record SessionSecret(string Password, string NewPassword);
    // Session memory only; deliberately never part of serialized drafts/history/evidence.
    private readonly Dictionary<string, SessionSecret> toolSessionSecrets = [];
    private SessionSecret CurrentSessionSecret(string id) => id == "split" ? new(splitPassword.Password, "") : new(password.Password, newPassword.Password);

    private static void ConfigureDocumentCloseDialog(ContentDialog dialog)
    {
        dialog.Opened += (_, _) =>
        {
            foreach (var button in Descendants(dialog).OfType<Button>().Where(b => b.Name is "PrimaryButton" or "SecondaryButton" or "CloseButton"))
            {
                var text = button.Name == "PrimaryButton" ? dialog.PrimaryButtonText : button.Name == "SecondaryButton" ? dialog.SecondaryButtonText : dialog.CloseButtonText;
                button.Content = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center };
                button.MinHeight = 44; AutomationProperties.SetName(button, text);
            }
        };
    }

    private Button DocumentCloseButton(string text = "Tutup dokumen")
    {
        var button = new Button { Content = text };
        ToolTipService.SetToolTip(button, "Akhiri sesi alat ini. File di komputer tidak dihapus.");
        AutomationProperties.SetName(button, "Tutup dokumen");
        button.Click += async (_, _) => await CloseDocumentsAsync();
        return button;
    }

    private async Task CloseDocumentsAsync()
    {
        if (messageOpen || selected is null) return;
        if (RestartBusy) { await MenuMessageAsync("Pekerjaan sedang berjalan", "Tunggu atau batalkan proses sebelum menutup dokumen."); return; }
        messageOpen = true;
        var tool = selected;
        string? closedSavedMergePath = null;
        try
        {
            var pending = tool.Id == "organize" ? RestartLayoutChanged() :
                !closedToolSessions.Contains(tool.Id) && (RestartToolChanged(tool.Id, CurrentToolDraft()) ||
                (tool.Id == "split" ? splitPassword.Password.Length > 0 : password.Password.Length > 0 || newPassword.Password.Length > 0));
            if (pending)
            {
                ContentDialogResult answer;
                if (tool.Id == "merge") answer = await ShowMergeCloseConfirmationAsync();
                else
                {
                var dialog = new ContentDialog { XamlRoot = root.XamlRoot, Title = "Tutup dokumen?",
                    Content = CatalogName(tool) + ": perubahan belum disimpan. Simpan hasil atau tutup tanpa menyimpan. File asli dan hasil yang sudah disimpan tetap tersedia.",
                    PrimaryButtonText = "Proses dan simpan", SecondaryButtonText = "Tutup tanpa menyimpan",
                    CloseButtonText = "Kembali mengedit", DefaultButton = ContentDialogButton.Close };
                ConfigureDocumentCloseDialog(dialog); answer = await dialog.ShowAsync();
                }
                if (answer == ContentDialogResult.None) return;
                if (answer == ContentDialogResult.Primary)
                {
                    var previousResult = resultPath;
                    mergeCloseSaveInProgress = tool.Id == "merge";
                    switch (tool.Id)
                    {
                        case "organize": await ExecuteOrganizerAsync(); break;
                        case "split": await ExecuteSplitAsync(); break;
                        case "remove": await ExecuteRemoveAsync(); break;
                        case "extract": await ExecuteExtractAsync(); break;
                        default: await ExecuteAsync(); break;
                    }
                    // Failed/cancelled/unsupported exports never authorize close.
                    var saved = tool.Id == "organize" ? restartSavedLayout is not null && !RestartLayoutChanged() :
                        restartSavedTools.ContainsKey(tool.Id) && !RestartToolChanged(tool.Id, CurrentToolDraft());
                    if (RestartBusy || !saved || resultPath is null || resultPath == previousResult || !File.Exists(resultPath))
                    { status.Text = "Dokumen tetap terbuka. Hasil belum berhasil disimpan."; return; }
                    if (tool.Id == "merge") closedSavedMergePath = resultPath;
                }
            }
            if (RestartBusy || selected?.Id != tool.Id) return;
            var closingInputs = tool.Id == "organize" ? organizerSources.ToArray() : files.ToArray();
            foreach (var input in closingInputs) chosenFiles.Remove(input);
            closedToolSessions.Add(tool.Id); removedToolInputs.Remove(tool.Id);
            toolSessionSecrets.Remove(tool.Id);
            activeToolControls = null;
            organizerVisible = false; CancelOrganizerDrag();
            files.Clear(); toolDrafts.Remove(tool.Id); resultPath = null; toolOptions.Visibility = Visibility.Visible;
            restartSavedTools.Remove(tool.Id);
            password.Password = ""; newPassword.Password = "";
            pages.Text = ""; watermark.Text = ""; outputName.Text = ""; rotation.SelectedIndex = 0; margin.Value = 20; outputQuality = "medium";
            if (tool.Id == "split") { splitPassword.Password = ""; splitGeneration++; splitSource = null; splitCount = 0; splitRanges.Clear(); splitSelection = ""; splitCombine = false; splitPreviewSecret = ""; splitPreviewKey = ""; splitPreviewBitmaps.Clear(); splitTiles.Clear(); }
            if (tool.Id == "merge") mergeAngles.Clear();
            if (tool.Id == "pdf-jpg") { jpgAngle = 0; jpgSource = null; }
            if (tool.Id == "remove") { removeGeneration++; removeSource = null; removeSelection.Clear(); removeTiles.Clear(); }
            if (tool.Id == "extract") { extractGeneration++; extractSource = null; extractSelection.Clear(); extractTiles.Clear(); }
            if (tool.Id == "organize")
            {
                pageLayout.Clear(); organizerSources.Clear(); layoutUndo.Clear(); layoutRedo.Clear(); chosenPages.Clear();
                restartSavedLayout = null; restartSourcePageCounts.Clear();
                sourceSummary.Children.Clear(); organizerSourceLabels.Clear();
                organizerOutput = "hasil-organize.pdf";
            }
            ShowEmptyTool(tool);
            if (closedSavedMergePath is not null) ShowMergeClosedSavedNotice(closedSavedMergePath);
        }
        catch (Exception error)
        {
            LocalDiagnostics.Record("close-document", error);
            status.Text = "Dokumen belum dapat ditutup. Coba kembali. File di komputer tidak dihapus.";
        }
        finally { mergeCloseSaveInProgress = false; messageOpen = false; }
    }

    private void RemoveToolInput(string input)
    {
        if (active is not null || importing || selected is null || !files.Contains(input)) return;
        removedToolInputs.Add(selected.Id); files.Remove(input);
        status.Text = files.Count == 0
            ? "Input terakhir dikeluarkan. Draf tetap dalam sesi; gunakan Tutup dokumen untuk mengakhirinya. File di komputer tidak dihapus."
            : "Input dikeluarkan dari daftar. Susunan belum disimpan; file di komputer tidak dihapus.";
    }

    private void ExcludeOrganizerSource(int source)
    {
        if (active is not null || importing) return;
        CancelOrganizerDrag(); SaveLayout();
        pageLayout.RemoveAll(p => p.Source == source);
        RefreshPageGrid();
        var undo = new Button { Content = "Dokumen dikeluarkan — Urungkan" };
        undo.Click += (_, _) => { RestoreLayout(false); sourceSummary.Children.Remove(undo); ShowOrganizer(); };
        if (pageLayout.Count == 0)
        {
            organizerVisible = false;
            ShowEmptyTool(ToolCatalog.All.First(t => t.Id == "organize"));
            body.Children.Add(undo);
            body.Children.Add(DocumentCloseButton());
        }
        else sourceSummary.Children.Add(undo);
        status.Text = "Dokumen dikeluarkan dari daftar. Susunan dan Urungkan tetap dalam sesi; gunakan Tutup dokumen untuk mengakhirinya. File asli tetap aman.";
    }
}
