using System.Text.Json;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed record RestartWork(string Name, string Tool, Action Review, string Reason = "Pengaturan atau susunan hasil belum disimpan");
    // Memory-only output checkpoints. Never persist document contents or passwords here.
    private readonly Dictionary<string, string> restartSavedTools = [];
    private readonly Dictionary<string, int> restartSourcePageCounts = new(StringComparer.OrdinalIgnoreCase);
    private string? restartSavedLayout;

    private ToolDraft CurrentToolDraft() => new(files.ToArray(), pages.Text, watermark.Text, outputName.Text,
        rotation.SelectedIndex, margin.Value, outputQuality);

    private string RestartToolState(string id, ToolDraft draft) => JsonSerializer.Serialize(new
    {
        draft.Files, draft.Output,
        Pages = id is "remove" or "extract" or "rotate" or "watermark" or "numbers" or "crop" or "pdf-jpg" ? draft.Pages : "",
        Text = id == "watermark" ? draft.Text : "",
        Rotation = id == "rotate" ? draft.Rotation : 0,
        Margin = id == "crop" ? draft.Margin : 20,
        Quality = id is "images" or "pdf-jpg" ? draft.Quality : "original",
        Angles = id == "merge" ? draft.Files.Select(p => mergeAngles.GetValueOrDefault(p)).ToArray() : [],
        Jpg = id == "pdf-jpg" ? jpgAngle : 0,
        Split = id == "split" ? JsonSerializer.Serialize(new { splitSource, splitMode, splitStep, splitCombine, splitSelection, splitRanges }) : ""
    });

    private bool CustomRestartOutput(string id, string output) => output.Length > 0 &&
        output != "hasil-" + id + ".pdf" && output != "hasil-" + id + ".zip" && output != "hasil-" + id + ".jpg" &&
        output != automaticOutputNames.GetValueOrDefault(id);

    private bool RestartToolChanged(string id, ToolDraft draft)
    {
        if (removedToolInputs.Contains(id)) return true;
        if (restartSavedTools.TryGetValue(id, out var saved)) return saved != RestartToolState(id, draft);
        // Choosing/reading one source is not an edit. Explicit output options and multi-source
        // compositions are work to review; search boxes and navigation history are not.
        return CustomRestartOutput(id, draft.Output) ||
            (id is "merge" or "images" && draft.Files.Length > 1) ||
            (id == "merge" && draft.Files.Any(p => mergeAngles.GetValueOrDefault(p) != 0)) ||
            (id is "remove" or "extract" or "rotate" or "watermark" or "numbers" or "crop" or "pdf-jpg" && draft.Pages.Length > 0) ||
            (id == "watermark" && draft.Text.Length > 0) || (id == "rotate" && draft.Rotation != 0) ||
            (id == "crop" && draft.Margin != 20) || (id is "images" or "pdf-jpg" && draft.Quality != "medium") ||
            (id == "pdf-jpg" && jpgAngle != 0) ||
            (id == "split" && splitSource is not null && (splitMode != SplitMode.Custom || splitCombine || splitSelection.Length > 0 ||
                splitRanges.Count != 1 || splitRanges[0] != new SplitRange(1, splitCount)));
    }

    private string RestartLayoutState() => JsonSerializer.Serialize(new
    {
        Sources = organizerSources.ToArray(), Layout = pageLayout.Select(p => new { p.Source, p.Page, p.Rotation }),
        Output = organizerVisible ? outputName.Text : organizerOutput
    });

    private bool RestartLayoutChanged()
    {
        if (restartSavedLayout is not null) return restartSavedLayout != RestartLayoutState();
        if (organizerSources.Count == 0 && pageLayout.Count == 0) return false;
        if (organizerSources.Count != 1 || !restartSourcePageCounts.TryGetValue(organizerSources[0], out var count)) return true;
        return pageLayout.Count != count || pageLayout.Where((p, i) => p.Source != 0 || p.Page != i || p.Rotation != 0).Any() ||
            CustomRestartOutput("organize", organizerVisible ? outputName.Text : organizerOutput);
    }

    private void MarkRestartToolSaved(string id)
    { removedToolInputs.Remove(id); toolSessionSecrets.Remove(id); restartSavedTools[id] = RestartToolState(id, CurrentToolDraft()); }

    private List<RestartWork> RestartPendingWork()
    {
        var pending = new List<RestartWork>();
        if (liveEditor?.RestartHasSession == true)
            pending.Add(new(liveEditor.RestartDocumentName, "Edit PDF", () => { ShowLiveEditor(true); liveEditor.ReviewRestartWork(); }, "Edit teks/objek atau gerakan editor belum disimpan"));
        if (RestartLayoutChanged())
            pending.Add(new(organizerSources.Count > 0 ? string.Join(", ", organizerSources.Take(3).Select(Path.GetFileName)) : "Susunan halaman PDF",
                "Atur halaman PDF", () => { openingEmptyInput = true; try { ShowOrganizer(); } finally { openingEmptyInput = false; } }));
        var drafts = new Dictionary<string, ToolDraft>(toolDrafts);
        if (selected is { Id: not ("edit" or "organize") } current &&
            activeToolControls == current.Id && !closedToolSessions.Contains(current.Id) && screenIndex >= 0 && screens[screenIndex].Tool == current.Id) drafts[current.Id] = CurrentToolDraft();
        foreach (var (id, draft) in drafts)
        {
            if (id is "edit" or "organize" || closedToolSessions.Contains(id)) continue;
            var tool = ToolCatalog.All.FirstOrDefault(t => t.Id == id);
            if (tool is null || !tool.Available) continue;
            var secret = activeToolControls == id && selected?.Id == id ? CurrentSessionSecret(id) : toolSessionSecrets.GetValueOrDefault(id);
            var pendingPassword = secret is not null && (secret.Password.Length > 0 || secret.NewPassword.Length > 0);
            if (!pendingPassword && !RestartToolChanged(id, draft)) continue;
            var name = draft.Files.Length > 0 ? string.Join(", ", draft.Files.Take(3).Select(Path.GetFileName)) :
                removedToolInputs.Contains(id) ? "Susunan input · semua file dikeluarkan" : "Pengaturan hasil";
            var reason = pendingPassword ? "Kata sandi pemrosesan diisi; hasil belum disimpan" : removedToolInputs.Contains(id)
                ? "Input dikeluarkan; susunan draf belum disimpan" : restartSavedTools.ContainsKey(id)
                ? "Perubahan sejak hasil terakhir disimpan" : "Pengaturan atau susunan hasil belum disimpan";
            pending.Add(new(name, CatalogName(tool), () =>
            {
                // Already-visible work must not be rebuilt (pending password/input and selection).
                if (activeToolControls == id && selected?.Id == id && toolWorkspace.IsLoaded) { run.Focus(Microsoft.UI.Xaml.FocusState.Programmatic); return; }
                var preserved = activeToolControls == id && selected?.Id == id ? CurrentSessionSecret(id) : toolSessionSecrets.GetValueOrDefault(id);
                openingEmptyInput = true;
                try { ShowTool(tool); if (preserved is not null) { if (id == "split") splitPassword.Password = preserved.Password; else { password.Password = preserved.Password; newPassword.Password = preserved.NewPassword; } } }
                finally { openingEmptyInput = false; }
            }, reason));
        }
        return pending;
    }

    private sealed partial class LiveEditor
    {
        internal bool RestartHasSession => Dirty || drawing || drawingShape || dragging is not null;
        internal string RestartDocumentName => path is null ? "Dokumen editor" : Path.GetFileName(path);
        internal void ReviewRestartWork()
        {
            HideSaveScreen(); Refresh();
            if (inlineText?.IsLoaded == true) inlineText.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
            else save.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
        }
    }
}
