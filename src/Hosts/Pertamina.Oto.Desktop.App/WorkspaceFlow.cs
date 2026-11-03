using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private static void DetachOwned(UIElement tree, UIElement target)
    {
        if (tree is Panel panel)
        {
            if (panel.Children.Remove(target)) return;
            foreach (var child in panel.Children.ToArray()) DetachOwned(child, target);
        }
        else if (tree is Border border && border.Child is { } child)
        {
            if (child == target) border.Child = null; else DetachOwned(child, target);
        }
        else if (tree is ContentControl content && content.Content is UIElement element)
        {
            if (element == target) content.Content = null; else DetachOwned(element, target);
        }
    }

    private void DetachToolControls()
    {
        foreach (var control in new UIElement[] { pages, password, newPassword, outputName, watermark, margin, rotation, fileList, run, cancel, openResult, progress, status, location })
        {
            DetachOwned(toolWorkspace, control); DetachOwned(toolOptions, control);
        }
        DetachOwned(toolWorkspace, toolOptions);
        restoreOrganizerPresentation?.Invoke(); restoreOrganizerPresentation = null;
    }
    private void CaptureWorkspaceContext()
    {
        // The session lane owns source identity, including the live editor and retained
        // context after leaving a tool. Detached controls/selected are not an origin.
        var origin = CaptureFeatureTransitionOrigin();
        if (!toolWorkspace.IsLoaded && liveEditor?.Visible != true && pdfReader?.Visible != true) return;
        foreach (var path in origin.Files)
            if (!homeFiles.Contains(path, StringComparer.OrdinalIgnoreCase)) homeFiles.Add(path);
        chosenFiles.Clear(); chosenFiles.UnionWith(origin.Files);
    }

    private async Task OpenWorkspaceToolAsync(ToolCard tool)
    {
        if (featureTransitionRoutingActive)
        {
            // The owner presents one refusal after its current modal releases. Opening
            // a second ContentDialog while the first one is active is not supported.
            featureTransitionOverlapNotice = true;
            return;
        }
        featureTransitionRoutingActive = true;
        var originalHome = homeFiles.ToArray();
        var originalChosen = chosenFiles.ToArray();
        var opened = false;
        var restoreSelections = false;
        var leases = new List<FileStream>();
        Func<Task>? resumeEditorInput = null;
        try
        {
            if (FeatureTransitionBusy || messageOpen)
            {
                await RefuseFeatureTransitionRoutingAsync(tool);
                return;
            }
            resumeEditorInput = liveEditor?.HoldFeatureTransitionInput();
            var origin = CaptureFeatureTransitionOrigin();
            SaveToolDraft();
            if (!tool.Available)
            {
                ShowEmptyTool(tool); opened = true;
                return;
            }
            var inputs = await ResolveFeatureTransitionOriginAsync(tool, origin);
            if (!await FeatureTransitionRoutingMayContinueAsync(tool) || inputs is null) return;
            // Resolution may deliberately return to the source for save/review. It
            // returns null in that case; a non-null list is the user's transfer intent.
            var routeScreen = screenIndex;
            if (inputs.Length > FeatureTransitionMaximumFiles)
            {
                await ChooseTransitionActionAsync(tool, "Terlalu banyak dokumen",
                    "Pilih maksimal 50 dokumen pada sumber, lalu buka fitur kembali. Tidak ada dokumen yang dipilih atau dibuang otomatis.",
                    [new("return", "Kembali ke sumber", "Perbaiki pilihan dokumen sebelum melanjutkan.")]);
                return;
            }
            if (inputs.Length > 0)
            {
                var inspections = await InspectFeatureTransitionInputsAsync(tool, inputs);
                if (!await FeatureTransitionRoutingMayContinueAsync(tool) || !FeatureTransitionRoutingContextCurrent(routeScreen, originalHome, originalChosen)) return;
                var multiple = FeatureTransitionAllowsMultiple(tool);
                if (FeatureTransitionNeedsSelection(inspections))
                {
                    var documents = inspections.Select((item, index) =>
                    {
                        var sourceIndex = origin.ToolId == "organize" ? organizerSources.FindIndex(p => string.Equals(p, item.Path, StringComparison.OrdinalIgnoreCase)) : -1;
                        if (sourceIndex < 0) sourceIndex = index;
                        return new TransitionDocument(item.Path, FeatureTransitionFileName(item.Path),
                            FeatureTransitionDocumentIdentity(origin, item.Path) + " · " + item.Detail,
                            OrganizerSourceCode(sourceIndex), sourceIndex, item.Problem);
                    }).ToArray();
                    var chosen = await ChooseTransitionDocumentsAsync(tool, documents,
                        FeatureTransitionSourceContext(origin, inputs), FeatureTransitionSelectionNotice(tool, inspections, origin), multiple);
                    if (!await FeatureTransitionRoutingMayContinueAsync(tool) || chosen is null || !FeatureTransitionRoutingContextCurrent(routeScreen, originalHome, originalChosen)) return;
                    if (!TryResolveFeatureTransitionSelection(documents, chosen, multiple, out inputs))
                    {
                        await ChooseTransitionActionAsync(tool, "Pilihan dokumen belum valid",
                            "Pilih dokumen yang tersedia sesuai kapasitas fitur. Pilihan sumber tetap tersimpan.",
                            [new("return", "Kembali", "Tinjau pilihan dokumen lalu coba lagi.")]);
                        return;
                    }
                }
                // Hold read-only leases through destination decisions/open. A file
                // cannot be replaced between readiness validation and draft reset.
                var verified = await InspectFeatureTransitionInputsAsync(tool, inputs, leases);
                if (!await FeatureTransitionRoutingMayContinueAsync(tool) || !FeatureTransitionRoutingContextCurrent(routeScreen, originalHome, originalChosen)) return;
                var problem = FeatureTransitionSelectionProblem(verified);
                if (problem is not null)
                {
                    await ShowFeatureTransitionRoutingFailureAsync(tool, verified, problem);
                    return;
                }
            }
            if (!FeatureTransitionRoutingContextCurrent(routeScreen, originalHome, originalChosen)) return;
            restoreSelections = true;
            var prepared = await PrepareFeatureTransitionDestinationAsync(tool, inputs);
            if (!await FeatureTransitionRoutingMayContinueAsync(tool) || !prepared) return;
            await OpenFeatureTransitionDestinationAsync(tool, inputs);
            opened = true;
        }
        catch (Exception error)
        {
            LocalDiagnostics.Record("feature-transition-routing", error);
            try
            {
                await ChooseTransitionActionAsync(tool, "Fitur belum dapat dibuka",
                    "Perpindahan belum selesai. Periksa dokumen dan coba lagi. File sumber tidak diubah.",
                    [new("return", "Kembali", "Kembali ke dokumen dan draf yang tersimpan dalam sesi.")]);
            }
            catch (Exception noticeError)
            {
                LocalDiagnostics.Record("feature-transition-failure-dialog", noticeError);
                status.Text = "Perpindahan fitur belum selesai. Dokumen sumber tidak diubah; coba kembali.";
            }
        }
        finally
        {
            foreach (var lease in leases) lease.Dispose();
            // Metadata and choice collection never modify these lists. Roll back only
            // the session commit boundary; don't erase a new selection/paste made
            // while an earlier metadata read was pending or a busy process completed.
            if (!opened && restoreSelections)
            {
                homeFiles.Clear(); homeFiles.AddRange(originalHome);
                chosenFiles.Clear(); chosenFiles.UnionWith(originalChosen);
            }
            try
            {
                if (featureTransitionOverlapNotice)
                {
                    featureTransitionOverlapNotice = false;
                    await RefuseFeatureTransitionRoutingAsync(tool);
                }
            }
            finally
            {
                try { if (resumeEditorInput is not null) await resumeEditorInput(); }
                finally { featureTransitionOverlapNotice = false; featureTransitionRoutingActive = false; }
            }
        }
    }
}

