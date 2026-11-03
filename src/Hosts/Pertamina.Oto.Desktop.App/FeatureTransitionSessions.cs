using Microsoft.UI.Xaml;
using Pertamina.Oto.LocalPdf;
using Windows.Data.Pdf;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // All checkpoints below are session memory, including credentials. Never persist/log them.
    private FeatureTransitionOrigin? retainedTransitionOrigin;
    private string? transitionOrganizerResult;
    private readonly Dictionary<string, string> transitionSavedResults = [];
    private TransitionDestinationApproval? transitionDestinationApproval;
    private sealed record TransitionSessionStamp(string State, SessionSecret? Secret, LiveEditor? Editor);
    private sealed record TransitionDestinationApproval(string Tool, string[] Inputs, TransitionSessionStamp Stamp);

    private bool FeatureTransitionBusy => RestartBusy || restartRequested || acceptingEmptyInput ||
        pickingMergeEmptyInput || liveEditor?.TransitionGesture == true || pdfReader is { Visible: true, TransitionBusy: true };

    private bool TransitionOwnsControls(string id) => selected?.Id == id && activeToolControls == id &&
        screenIndex >= 0 && screenIndex < screens.Count && (screens[screenIndex].Tool == id ||
            id == "organize" && screens[screenIndex].Title == "Atur halaman PDF") &&
        !closedToolSessions.Contains(id);

    private ToolDraft? TransitionDraft(string id) => TransitionOwnsControls(id) ? CurrentToolDraft() :
        closedToolSessions.Contains(id) ? null : toolDrafts.GetValueOrDefault(id);

    private SessionSecret? TransitionSecret(string id) => TransitionOwnsControls(id)
        ? CurrentSessionSecret(id) : toolSessionSecrets.GetValueOrDefault(id);

    private static bool TransitionHasSecret(SessionSecret? value) => value is not null &&
        (value.Password.Length > 0 || value.NewPassword.Length > 0);

    private FeatureTransitionOrigin TransitionOriginFor(string id)
    {
        var tool = ToolCatalog.All.First(t => t.Id == id);
        if (id == "edit") return new(id, CatalogName(tool), liveEditor?.TransitionSource is { } path ? [path] : [],
            liveEditor?.RestartHasSession == true, liveEditor?.TransitionSavedResult);
        if (id == "organize")
        {
            var originals = pageLayout.Where(p => p.Source >= 0 && p.Source < organizerSources.Count)
                .Select(p => organizerSources[p.Source]).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            return new(id, CatalogName(tool), originals, RestartLayoutChanged() || TransitionHasSecret(TransitionSecret(id)),
                transitionOrganizerResult);
        }
        var draft = TransitionDraft(id);
        if (draft is null) return new(id, CatalogName(tool), [], false, null);
        if (TransitionOwnsControls(id) && resultPath is not null && restartSavedTools.ContainsKey(id))
            transitionSavedResults[id] = resultPath;
        return new(id, CatalogName(tool), draft.Files.ToArray(), RestartToolChanged(id, draft) ||
            TransitionHasSecret(TransitionSecret(id)), transitionSavedResults.GetValueOrDefault(id));
    }

    private FeatureTransitionOrigin CaptureFeatureTransitionOrigin()
    {
        var home = homeFiles.Where(chosenFiles.Contains).ToArray();
        if (pdfReader is { Visible: true, TransitionPath: { } readerPath })
            return retainedTransitionOrigin = new(null, readerResults.Contains(readerPath)
                ? "Pembaca PDF · Hasil tersimpan" : "Pembaca PDF", [readerPath], false, readerResults.Contains(readerPath) ? readerPath : null);
        string? id = liveEditor?.Visible == true ? "edit" : organizerVisible ? "organize" :
            selected is { } current && TransitionOwnsControls(current.Id) ? current.Id : null;
        if (id is null)
        {
            if (retainedTransitionOrigin is { ToolId: null } retainedReader &&
                retainedReader.Label.StartsWith("Pembaca PDF", StringComparison.Ordinal) &&
                home.ToHashSet(StringComparer.OrdinalIgnoreCase).SetEquals(retainedReader.Files)) return retainedReader;
            // Catalog/home clear selected. History locates the retained session; a changed home
            // selection takes precedence, so detached controls cannot overwrite fresh choices.
            id = retainedTransitionOrigin?.ToolId;
            if (id is null)
            {
                var previous = screens.Take(Math.Clamp(screenIndex + 1, 0, screens.Count))
                    .LastOrDefault(s => s.Tool is not null || s.Title == "Atur halaman PDF");
                id = previous?.Title == "Atur halaman PDF" ? "organize" : previous?.Tool;
            }
            if (id is not null && ToolCatalog.All.Any(t => t.Id == id && t.Available) && !closedToolSessions.Contains(id))
            {
                var retained = TransitionOriginFor(id);
                if (home.Length > 0 && !home.ToHashSet(StringComparer.OrdinalIgnoreCase).SetEquals(retained.Files))
                    return new(null, "Dokumen dipilih di Beranda", home, false, null);
                // An explicit empty selection on the document list must remain empty.
                if (home.Length == 0 && homeEntries.IsLoaded) return new(null, "Dokumen dipilih di Beranda", [], false, null);
                if (retained.Files.Length > 0 || retained.HasPendingChanges)
                    return retainedTransitionOrigin = retained;
            }
            return new(null, "Dokumen dipilih di Beranda", home, false, null);
        }
        if (!ToolCatalog.All.Any(t => t.Id == id && t.Available))
            return new(null, "Dokumen dipilih di Beranda", home, false, null);
        var origin = TransitionOriginFor(id);
        // Organizer's legacy SaveToolDraft stores its output name but not its password.
        if (id == "organize" && TransitionOwnsControls(id))
        {
            var secret = CurrentSessionSecret(id);
            if (TransitionHasSecret(secret)) toolSessionSecrets[id] = secret;
            else toolSessionSecrets.Remove(id);
        }
        return retainedTransitionOrigin = origin;
    }

    private async Task<string?> ChooseSessionTransitionActionAsync(ToolCard target, string title,
        string description, IReadOnlyList<TransitionAction> actions, string? notice = null)
    {
        // Retain the exact inline control and uncommitted original-text draft while focus is modal.
        var resume = liveEditor?.HoldFeatureTransitionInput();
        try
        {
#if DEBUG
            if (featureTransitionActionAudit is not null) return await featureTransitionActionAudit(actions);
#endif
            return await ChooseTransitionActionAsync(target, title, description, actions, notice);
        }
        finally { if (resume is not null) await resume(); }
    }

    private async Task<string[]?> ResolveFeatureTransitionOriginAsync(ToolCard target, FeatureTransitionOrigin origin)
    {
        transitionDestinationApproval = null;
        if (!target.Available || FeatureTransitionBusy) return null;
        if (origin.ToolId == target.Id) return origin.Files.ToArray();
        if (origin.HasPendingChanges)
        {
            var answer = await ChooseSessionTransitionActionAsync(target, "Pilih versi dokumen",
                origin.Label + " memiliki pekerjaan yang belum disimpan.",
                [new("original", "Pakai dokumen asli", "Buka sumber asli; urutan, putaran, dan edit draf tidak ikut. Draf tetap tersedia."),
                 new("review", "Tinjau dan simpan dahulu", "Kembali ke fitur sumber. Simpan dengan tombol yang tersedia, lalu pilih fitur tujuan lagi.")],
                "Perpindahan tidak menyimpan atau memproses draf secara otomatis.");
            if (FeatureTransitionBusy) return null;
            if (answer == "original") return origin.Files.ToArray();
            if (answer == "review" && origin.ToolId is { } id)
            {
                await ResumeFeatureTransitionSessionAsync(ToolCatalog.All.First(t => t.Id == id));
                TransitionReviewNotice("Tinjau dan simpan hasil terlebih dahulu, lalu pilih " + CatalogName(target) + " lagi.");
            }
            return null;
        }
        if (origin.SavedResult is { } saved && !origin.Files.Contains(saved, StringComparer.OrdinalIgnoreCase))
        {
            var answer = await ChooseSessionTransitionActionAsync(target, "Pilih versi dokumen",
                "Tentukan dokumen dari " + origin.Label + " yang ingin dibuka.",
                [new("result", "Pakai hasil tersimpan", "Gunakan file hasil proses terakhir: " + Path.GetFileName(saved)),
                 new("original", "Pakai dokumen asli", "Gunakan sumber asli sebelum pengaturan, susunan, atau edit diterapkan.")],
                "Hasil dan sumber asli adalah file yang berbeda. Ketersediaan file akan diperiksa sebelum dibuka.");
            if (FeatureTransitionBusy) return null;
            return answer == "result" ? [saved] : answer == "original" ? origin.Files.ToArray() : null;
        }
        return origin.Files.ToArray();
    }

    private bool HasFeatureTransitionSession(string id) => !closedToolSessions.Contains(id) && (id switch
    {
        "edit" => liveEditor?.HasDocument == true,
        "organize" => organizerSources.Count > 0 || pageLayout.Count > 0 || layoutUndo.Count > 0 || layoutRedo.Count > 0,
        _ => TransitionDraft(id) is { } draft && (draft.Files.Length > 0 || RestartToolChanged(id, draft) || TransitionHasSecret(TransitionSecret(id)))
    });

    private TransitionSessionStamp FeatureTransitionStamp(string id)
    {
        var draft = TransitionDraft(id);
        var state = id switch
        {
            "edit" => liveEditor?.TransitionState ?? "",
            "organize" => System.Text.Json.JsonSerializer.Serialize(new
            { Sources = organizerSources.ToArray(), Pages = pageLayout.ToArray(), Undo = layoutUndo.ToArray(), Redo = layoutRedo.ToArray(),
              Selection = chosenPages.Order().ToArray(), Output = organizerVisible ? outputName.Text : organizerOutput, Saved = restartSavedLayout }),
            _ => draft is null ? "" : RestartToolState(id, draft)
        };
        return new(state, TransitionSecret(id), id == "edit" ? liveEditor : null);
    }

    private async Task<bool> PrepareFeatureTransitionDestinationAsync(ToolCard target, string[] incoming)
    {
        transitionDestinationApproval = null;
        if (!target.Available || FeatureTransitionBusy) return false;
        var stamp = FeatureTransitionStamp(target.Id);
        if (HasFeatureTransitionSession(target.Id))
        {
            if (incoming.Length == 0)
            {
                await ResumeFeatureTransitionSessionAsync(target);
                return false;
            }
            var dirty = TransitionOriginFor(target.Id).HasPendingChanges;
            var actions = new List<TransitionAction>
            {
                new("resume", "Lanjutkan draf yang ada", "Buka kembali pekerjaan di " + CatalogName(target) + ". Dokumen pilihan baru tidak ditambahkan.")
            };
            if (dirty) actions.Add(new("review", "Tinjau dan simpan draf dahulu", "Kembali ke draf tujuan untuk menyimpan. Setelah selesai, pilih dokumen dan fitur lagi."));
            actions.Add(new("replace", dirty ? "Buang draf dan ganti dokumen" : "Ganti dokumen",
                dirty ? "Perubahan, susunan, dan riwayat Urungkan pada draf tujuan akan dibuang setelah dokumen baru berhasil dibuka. File asli dan hasil tersimpan tetap ada."
                      : "Mulai sesi baru dengan dokumen pilihan. File asli dan hasil tersimpan tetap ada.", true));
            var answer = await ChooseSessionTransitionActionAsync(target, "Draf tujuan sudah tersedia",
                CatalogName(target) + (dirty ? " memiliki pekerjaan yang belum disimpan." : " sudah memiliki dokumen terbuka."), actions,
                "Dokumen baru tidak akan mengganti draf tanpa pilihan Anda.");
            if (FeatureTransitionBusy || FeatureTransitionStamp(target.Id) != stamp) return false;
            if (answer is "resume" or "review")
            {
                await ResumeFeatureTransitionSessionAsync(target);
                if (answer == "review") TransitionReviewNotice("Simpan draf dengan tombol yang tersedia. Setelah selesai, pilih dokumen dan fitur tujuan lagi.");
                return false;
            }
            if (answer != "replace") return false;
        }
        transitionDestinationApproval = new(target.Id, incoming.ToArray(), stamp);
        return true;
    }

    private void TransitionReviewNotice(string text)
    {
        status.Text = text;
        if (liveEditor?.Visible == true) liveEditor.TransitionReview(text);
    }

    private async Task ResumeFeatureTransitionSessionAsync(ToolCard tool)
    {
        if (FeatureTransitionBusy || !tool.Available) return;
        if (tool.Id == "edit" && liveEditor?.Visible == true) { liveEditor.ReviewRestartWork(); return; }
        if (tool.Id == "organize" && organizerVisible) return;
        if (tool.Id is not "organize" && TransitionOwnsControls(tool.Id) && toolWorkspace.IsLoaded) return;
        var restore = SnapshotFeatureTransitionMemory(); var restorePresentation = SnapshotFeatureTransitionPresentation();
        var resumeEditor = liveEditor?.HoldFeatureTransitionInput();
        var chosen = chosenFiles.ToArray();
        var organizerSelection = chosenPages.ToArray();
        var preservedOutput = organizerOutput;
        var preservedAutomaticName = automaticOutputNames.GetValueOrDefault(tool.Id);
        var wasOpening = openingEmptyInput;
        var secret = TransitionSecret(tool.Id);
        try
        {
            chosenFiles.Clear(); // ShowOrganizer otherwise appends home selection without asking.
            openingEmptyInput = true;
            if (tool.Id == "edit") { ShowLiveEditor(); liveEditor?.ReviewRestartWork(); return; }
            if (tool.Id == "organize")
            {
                ShowOrganizer();
                outputName.Text = preservedOutput;
                if (preservedAutomaticName is null) automaticOutputNames.Remove(tool.Id);
                else automaticOutputNames[tool.Id] = preservedAutomaticName;
                chosenPages.Clear(); chosenPages.UnionWith(organizerSelection.Where(i => i >= 0 && i < pageLayout.Count));
            }
            else ShowTool(tool);
            if (secret is not null)
            {
                if (tool.Id == "split") splitPassword.Password = secret.Password;
                else { password.Password = secret.Password; newPassword.Password = secret.NewPassword; }
            }
            if (tool.Id == "organize") resultPath = transitionOrganizerResult;
            else resultPath = transitionSavedResults.GetValueOrDefault(tool.Id);
        }
        catch { try { restorePresentation(); } finally { restore(); } throw; }
        finally
        {
            chosenFiles.Clear(); chosenFiles.UnionWith(chosen); openingEmptyInput = wasOpening;
            if (resumeEditor is not null) await resumeEditor();
        }
    }

    private async Task OpenFeatureTransitionDestinationAsync(ToolCard target, string[] incoming)
    {
        var approval = transitionDestinationApproval;
        transitionDestinationApproval = null; // A replacement decision is single-use and input-specific.
        if (!target.Available) return;
        if (FeatureTransitionBusy || approval is null || approval.Tool != target.Id ||
            !approval.Inputs.SequenceEqual(incoming, StringComparer.OrdinalIgnoreCase) || approval.Stamp != FeatureTransitionStamp(target.Id))
            throw new InvalidOperationException("Transition decision expired; no draft changed.");

        // Snapshot before any session mutation; input handles hold the validated sources stable.
        var restore = SnapshotFeatureTransitionMemory();
        var restorePresentation = SnapshotFeatureTransitionPresentation();
        var locks = new List<FileStream>();
        LiveEditor? candidate = null;
        var previousEditor = liveEditor;
        var resumeEditor = previousEditor?.HoldFeatureTransitionInput();
        var committed = false;
        var sessionMutated = false;
        var presentationMutated = false;
        try
        {
            if (incoming.Length > 50 || incoming.Length > 1 && target.Id is not ("merge" or "images" or "organize"))
                throw new IOException("Unsupported transition input count.");
            long total = 0;
            foreach (var path in incoming)
            {
                if (!Path.IsPathFullyQualified(path) || path.StartsWith(@"\\", StringComparison.Ordinal)) throw new IOException("Local file required.");
                var extension = Path.GetExtension(path).ToLowerInvariant();
                if (!(target.Id == "images" ? extension is ".jpg" or ".jpeg" or ".png" : extension == (target.Id == "word-pdf" ? ".docx" : ".pdf")))
                    throw new IOException("Incompatible transition input.");
                var handle = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                locks.Add(handle); total += handle.Length;
                if (handle.Length == 0 || total > 100L * 1024 * 1024) throw new IOException("Transition input limit.");
            }
            var layout = new List<OrganizerPage>();
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (target.Id == "organize")
            {
                for (var source = 0; source < incoming.Length; source++)
                {
                    if (counts.ContainsKey(incoming[source])) throw new IOException("Duplicate organizer source.");
                    var document = await PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(incoming[source]));
                    if (document.IsPasswordProtected || document.PageCount == 0 || layout.Count + document.PageCount > 1000)
                        throw new IOException("Organizer input unavailable.");
                    counts[incoming[source]] = (int)document.PageCount;
                    for (var page = 0; page < document.PageCount; page++) layout.Add(OrganizerPage.Create(source, page));
                }
            }
            if (target.Id == "edit" && incoming.Length == 1)
            {
                // A separate editor owns the candidate. The old object (including inline input,
                // undo, original text, image handles and source lock) is untouched on failure.
                candidate = new LiveEditor(this);
                await candidate.OpenAsync(incoming[0]);
                if (!candidate.TransitionReady) throw new IOException("Editor candidate did not open.");
            }
            if (FeatureTransitionBusy || approval.Stamp != FeatureTransitionStamp(target.Id))
                throw new InvalidOperationException("Transition changed while opening.");
#if DEBUG
            featureTransitionBeforeCommitAudit?.Invoke();
#endif
            sessionMutated = true;
            SaveToolDraft();
            activeToolControls = null; // Prevent legacy ShowTool recapturing the old target over the new draft.
            resultPath = null;
            closedToolSessions.Remove(target.Id); removedToolInputs.Remove(target.Id);
            toolSessionSecrets.Remove(target.Id); restartSavedTools.Remove(target.Id); transitionSavedResults.Remove(target.Id);
#if DEBUG
            featureTransitionAfterMutationAudit?.Invoke();
#endif
            if (target.Id == "edit")
            {
                if (candidate is not null) liveEditor = candidate;
                presentationMutated = true;
                if (incoming.Length == 0) ShowEmptyTool(target); else ShowLiveEditor(true);
            }
            else if (target.Id == "organize")
            {
                organizerSources.Clear(); organizerSources.AddRange(incoming);
                pageLayout.Clear(); pageLayout.AddRange(layout); layoutUndo.Clear(); layoutRedo.Clear(); chosenPages.Clear();
                restartSourcePageCounts.Clear(); foreach (var item in counts) restartSourcePageCounts[item.Key] = item.Value;
                restartSavedLayout = null; transitionOrganizerResult = null; organizerOutput = "hasil-organize.pdf";
                organizerThumbnails.Clear(); password.Password = ""; newPassword.Password = "";
                presentationMutated = true;
                ShowTransitionToolWithoutHomeImport(target, incoming.Length > 0);
            }
            else
            {
                toolDrafts[target.Id] = new(incoming.ToArray(), "", "", "hasil-" + target.Id +
                    (target.Id == "split" ? ".zip" : target.Id == "pdf-jpg" ? ".jpg" : ".pdf"), 0, 20);
                if (target.Id == "merge") mergeAngles.Clear();
                if (target.Id == "pdf-jpg") { jpgAngle = 0; jpgSource = null; }
                if (target.Id == "split") { splitSource = null; splitRanges.Clear(); splitSelection = ""; splitCombine = false; splitPreviewSecret = ""; }
                presentationMutated = true;
                ShowTransitionToolWithoutHomeImport(target, incoming.Length > 0);
            }
            retainedTransitionOrigin = TransitionOriginFor(target.Id);
            committed = true;
        }
        catch
        {
            if (sessionMutated)
            {
                // Restore the old logical tree as well as its model. Rebuilding the old page
                // through ShowTool would itself clear selections/start new preview work.
                try { if (presentationMutated) restorePresentation(); }
                finally { restore(); }
            }
            throw; // Routing owns the user-facing file failure notice and selection rollback.
        }
        finally
        {
            foreach (var handle in locks) handle.Dispose();
            if (!committed) candidate?.ReleaseFiles();
            if (resumeEditor is not null) await resumeEditor();
            if (committed && candidate is not null && previousEditor is not null) previousEditor.ReleaseFiles();
        }
    }

    private void ShowTransitionToolWithoutHomeImport(ToolCard tool, bool populated)
    {
        var chosen = chosenFiles.ToArray(); var wasOpening = openingEmptyInput;
        try
        {
            chosenFiles.Clear(); openingEmptyInput = populated;
            if (populated) { if (tool.Id == "organize") ShowOrganizer(); else ShowTool(tool); }
            else ShowEmptyTool(tool);
        }
        finally { chosenFiles.Clear(); chosenFiles.UnionWith(chosen); openingEmptyInput = wasOpening; }
    }

    private Action SnapshotFeatureTransitionPresentation()
    {
        // Traverse owned logical children, never WinUI template/virtualization internals.
        var seen = new HashSet<UIElement>();
        var clear = new List<Action>(); var restore = new List<Action>();
        void Capture(UIElement element)
        {
            if (!seen.Add(element)) return;
            if (element is Microsoft.UI.Xaml.Controls.Panel panel)
            {
                var children = panel.Children.ToArray();
                clear.Add(() => panel.Children.Clear());
                restore.Add(() => { foreach (var child in children) panel.Children.Add(child); });
                if (panel is Microsoft.UI.Xaml.Controls.Grid grid)
                {
                    var rows = grid.RowDefinitions.ToArray(); var columns = grid.ColumnDefinitions.ToArray();
                    restore.Add(() =>
                    {
                        grid.RowDefinitions.Clear(); foreach (var row in rows) grid.RowDefinitions.Add(row);
                        grid.ColumnDefinitions.Clear(); foreach (var column in columns) grid.ColumnDefinitions.Add(column);
                    });
                }
                foreach (var child in children) Capture(child);
            }
            else if (element is Microsoft.UI.Xaml.Controls.Border border)
            {
                var child = border.Child; clear.Add(() => border.Child = null); restore.Add(() => border.Child = child);
                if (child is not null) Capture(child);
            }
            else if (element is Microsoft.UI.Xaml.Controls.ContentControl content)
            {
                var value = content.Content; clear.Add(() => content.Content = null); restore.Add(() => content.Content = value);
                if (value is UIElement child) Capture(child);
            }
        }
        Capture(body); Capture(toolWorkspace); Capture(toolOptions); Capture(historyPage);
        var historyVisibility = historyPage.Visibility;
        var scrollVisibility = standardPageScroll?.Visibility;
        var heading = breadcrumb.Text; var oldEmpty = emptyTool;
        return () =>
        {
            // Release any new owner of a shared control before reconstructing prior ownership.
            foreach (var element in seen.Where(e => e != body && e != historyPage))
            {
                DetachOwned(body, element); DetachOwned(historyPage, element);
                DetachOwned(toolWorkspace, element); DetachOwned(toolOptions, element);
            }
            foreach (var action in clear) action();
            foreach (var action in restore) action();
            historyPage.Visibility = historyVisibility;
            if (standardPageScroll is not null && scrollVisibility is { } visibility) standardPageScroll.Visibility = visibility;
            breadcrumb.Text = heading; emptyTool = oldEmpty;
        };
    }

    private Action SnapshotFeatureTransitionMemory()
    {
        var draft = CurrentToolDraft(); var oldSelected = selected; var oldActive = activeToolControls;
        var oldResult = resultPath; var oldVisible = organizerVisible; var oldEditor = liveEditor; var oldOrigin = retainedTransitionOrigin;
        var oldSecret = new SessionSecret(password.Password, newPassword.Password); var oldSplitPassword = splitPassword.Password;
        var drafts = toolDrafts.ToArray(); var secrets = toolSessionSecrets.ToArray(); var saved = restartSavedTools.ToArray();
        var outputs = transitionSavedResults.ToArray(); var closed = closedToolSessions.ToArray(); var removed = removedToolInputs.ToArray();
        var sources = organizerSources.ToArray(); var layout = pageLayout.ToArray(); var undo = layoutUndo.ToArray(); var redo = layoutRedo.ToArray();
        var selection = chosenPages.ToArray(); var counts = restartSourcePageCounts.ToArray(); var thumbnails = organizerThumbnails.ToArray();
        var savedLayout = restartSavedLayout; var output = organizerOutput; var organizerResult = transitionOrganizerResult;
        var angles = mergeAngles.ToArray(); var oldJpgAngle = jpgAngle; var oldJpgSource = jpgSource;
        var oldSplitSource = splitSource; var ranges = splitRanges.ToArray(); var oldSplitSelection = splitSelection;
        var oldSplitMode = splitMode; var oldSplitStep = splitStep; var oldSplitCombine = splitCombine; var oldSplitCount = splitCount;
        var oldSplitKey = splitPreviewKey; var oldSplitSecret = splitPreviewSecret; var splitBitmaps = splitPreviewBitmaps.ToArray();
        var oldRemoveSource = removeSource; var oldRemoveCount = removeCount; var remove = removeSelection.ToArray();
        var oldExtractSource = extractSource; var oldExtractCount = extractCount; var extract = extractSelection.ToArray();
        var home = homeFiles.ToArray(); var chosen = chosenFiles.ToArray();
        var names = automaticOutputNames.ToArray(); var oldUpdating = updatingSources;
        var navigation = screens.ToArray(); var navigationIndex = screenIndex;
        static void Map<TKey, TValue>(Dictionary<TKey, TValue> map, KeyValuePair<TKey, TValue>[] values) where TKey : notnull
        { map.Clear(); foreach (var pair in values) map.Add(pair.Key, pair.Value); }
        static void Stack<T>(Stack<T> stack, T[] values) { stack.Clear(); foreach (var value in values.Reverse()) stack.Push(value); }
        return () =>
        {
            selected = null; updatingSources = true;
            activeToolControls = oldActive; resultPath = oldResult; organizerVisible = oldVisible;
            liveEditor = oldEditor; retainedTransitionOrigin = oldOrigin;
            Map(toolDrafts, drafts); Map(toolSessionSecrets, secrets); Map(restartSavedTools, saved); Map(transitionSavedResults, outputs);
            closedToolSessions.Clear(); closedToolSessions.UnionWith(closed); removedToolInputs.Clear(); removedToolInputs.UnionWith(removed);
            organizerSources.Clear(); organizerSources.AddRange(sources); pageLayout.Clear(); pageLayout.AddRange(layout);
            Stack(layoutUndo, undo); Stack(layoutRedo, redo); chosenPages.Clear(); chosenPages.UnionWith(selection);
            Map(restartSourcePageCounts, counts); Map(organizerThumbnails, thumbnails);
            restartSavedLayout = savedLayout; organizerOutput = output; transitionOrganizerResult = organizerResult;
            Map(mergeAngles, angles); jpgAngle = oldJpgAngle; jpgSource = oldJpgSource;
            splitSource = oldSplitSource; splitRanges.Clear(); splitRanges.AddRange(ranges); splitSelection = oldSplitSelection;
            splitMode = oldSplitMode; splitStep = oldSplitStep; splitCombine = oldSplitCombine; splitCount = oldSplitCount;
            splitPreviewKey = oldSplitKey; splitPreviewSecret = oldSplitSecret; Map(splitPreviewBitmaps, splitBitmaps);
            removeSource = oldRemoveSource; removeCount = oldRemoveCount; removeSelection.Clear(); removeSelection.UnionWith(remove);
            extractSource = oldExtractSource; extractCount = oldExtractCount; extractSelection.Clear(); extractSelection.AddRange(extract);
            files.Clear(); foreach (var file in draft.Files) files.Add(file);
            pages.Text = draft.Pages; watermark.Text = draft.Text; outputName.Text = draft.Output;
            rotation.SelectedIndex = draft.Rotation; margin.Value = draft.Margin; outputQuality = draft.Quality;
            password.Password = oldSecret.Password; newPassword.Password = oldSecret.NewPassword; splitPassword.Password = oldSplitPassword;
            homeFiles.Clear(); homeFiles.AddRange(home); chosenFiles.Clear(); chosenFiles.UnionWith(chosen);
            Map(automaticOutputNames, names); screens.Clear(); screens.AddRange(navigation); screenIndex = navigationIndex;
            selected = oldSelected; updatingSources = oldUpdating;
        };
    }

    private sealed partial class PdfReaderView
    {
        internal string? TransitionPath => path;
        internal bool TransitionBusy => busy;
    }

    private sealed partial class LiveEditor
    {
        private bool transitionRetainInput;
        internal string? TransitionSource => path;
        internal string? TransitionSavedResult => lastResult;
        internal bool TransitionReady => HasDocument && pageReady && !Working;
        internal bool TransitionGesture => drawing || drawingShape || dragging is not null || originalInlineCommit is { IsCompleted: false };
        internal string TransitionState => System.Text.Json.JsonSerializer.Serialize(new
        { path, State, savedState, lastResult, originalInlineDraft, Inline = inlineText?.Text, Undo = past.ToArray(), Redo = future.ToArray() });
        internal void TransitionReview(string message) { ReviewRestartWork(); notice.Text = message; }
        internal Func<Task> HoldFeatureTransitionInput()
        {
            var previous = transitionRetainInput; var previousReading = readingPreview;
            transitionRetainInput = true;
            var resume = PauseForReadPreview();
            return async () =>
            {
                try { await resume(); }
                finally { transitionRetainInput = previous; readingPreview = previousReading; }
            };
        }
    }
}
