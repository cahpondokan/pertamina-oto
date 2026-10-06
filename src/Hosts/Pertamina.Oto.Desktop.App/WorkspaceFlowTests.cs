namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    internal void TestWorkspaceFlow()
    {
        var report = new List<string>();
        try
        {
            homeFiles.Clear(); chosenFiles.Clear(); files.Clear(); toolDrafts.Clear();
            foreach (var id in new[] { "pdf-jpg", "merge", "split", "remove", "extract", "rotate", "watermark", "protect", "merge" })
            {
                openingEmptyInput = true;
                try { ShowTool(ToolCatalog.All.First(t => t.Id == id)); }
                finally { openingEmptyInput = false; }
                outputName.Text = "check-" + id + ".pdf";
                ShowCatalog();
                if (!toolDrafts.TryGetValue(id, out var draft) || draft.Output != "check-" + id + ".pdf")
                    throw new InvalidOperationException("draft " + id);
                report.Add("PASS editor/catalog transition and draft " + id);
            }
            ShowHome(); ShowFavorites(); ShowHistory(); ShowCatalog();
            report.Add("PASS home/favorites/history/catalog transitions");
            for (var i = 0; i < 3; i++)
            {
                openingEmptyInput = true;
                try { ShowOrganizer(); ShowCatalog(); ShowTool(ToolCatalog.All.First(t => t.Id == "merge")); }
                finally { openingEmptyInput = false; }
            }
            report.Add("PASS repeated organizer/catalog/merge transitions");
            var originalOrder = pageLayout.Select(p => p.Id).ToArray();
            var originalPitch = OrganizerPitchX;
            previewScale = 1.8;
            ArrangeOrganizerCards(pageLayout, false);
            if (OrganizerPitchX <= originalPitch || !originalOrder.SequenceEqual(pageLayout.Select(p => p.Id)))
                throw new InvalidOperationException("zoom layout");
            previewScale = 1;
            ArrangeOrganizerCards(pageLayout, false);
            report.Add("PASS zoom geometry and order preservation (empty layout)");
            files.Clear(); files.Add("test-a.pdf"); files.Add("test-b.pdf"); files.Add("test-c.pdf");
            RefreshMergeTiles();
            var firstCard = mergeCards["test-a.pdf"];
            var untouchedCard = mergeCards["test-c.pdf"];
            files.Move(0, 1); RefreshMergeTiles();
            if (!ReferenceEquals(firstCard, mergeCards["test-a.pdf"]) || !ReferenceEquals(untouchedCard, mergeTiles!.Items[2]))
                throw new InvalidOperationException("merge card identity");
            files.Remove("test-b.pdf"); RefreshMergeTiles();
            if (mergeTiles!.Items.Count != 2 || mergeCards.ContainsKey("test-b.pdf")) throw new InvalidOperationException("merge card removal");
            report.Add("PASS merge reorder preserves cards and removal updates only selected document");
            files.Clear(); RefreshMergeTiles();
            openingEmptyInput = true;
            try { ShowTool(ToolCatalog.All.First(t => t.Id == "remove")); }
            finally { openingEmptyInput = false; }
            removeCount = 3; removeLoading = false; removeSelection.Clear(); UpdateRemoveSummary();
            if (removeRun.IsEnabled) throw new InvalidOperationException("remove empty selection");
            removeSelection.Add(2); SyncRemoveSelection();
            if (!removeRun.IsEnabled || !Equals(removeRun.Content, "Hapus 1 halaman & simpan") || pages.Text != "2") throw new InvalidOperationException("remove selected count");
            removeSelection.UnionWith([1, 3]); SyncRemoveSelection();
            if (removeRun.IsEnabled || !removeSummary.Text.Contains("Sisakan minimal satu halaman", StringComparison.Ordinal)) throw new InvalidOperationException("remove all guard");
            removeSelection.Clear(); SyncRemoveSelection();
            if (removeRun.IsEnabled || pages.Text.Length != 0) throw new InvalidOperationException("remove undo selection");
            report.Add("PASS remove selection, count label, deselection and keep-one-page guard");
            openingEmptyInput = true;
            try { ShowTool(ToolCatalog.All.First(t => t.Id == "split")); }
            finally { openingEmptyInput = false; }
            splitSource = "preview-test.pdf"; splitCount = 8;
            splitRanges.Clear(); splitRanges.Add(new(1, 3)); splitRanges.Add(new(5, 8));
            splitMode = SplitMode.Custom; splitCombine = false; BuildSplitFields();
            if (splitGroups.Length != 2 || splitTabs.Count != 3 || splitTabs[2].IsEnabled) throw new InvalidOperationException("split visual modes");
            splitMode = SplitMode.AllPages; BuildSplitFields();
            if (splitTiles.Count != 8 || splitTiles.Any(t => t.Check.IsChecked != true)) throw new InvalidOperationException("split all pages preview");
            var stableTile = splitTiles[0].Border;
            splitMode = SplitMode.SelectedPages; splitSelection = "1,3,5"; BuildSplitFields();
            if (splitGroups.Length != 3 || !ReferenceEquals(stableTile, splitTiles[0].Border) || splitTiles[1].Check.IsChecked == true) throw new InvalidOperationException("split selection preview");
            splitCombine = true; UpdateSplitPlan();
            if (splitGroups.Length != 1 || splitGroups[0].Length != 3) throw new InvalidOperationException("split combined selection");
            report.Add("PASS split mode cards, range groups, stable page selection and combined output plan");
        }
        catch (Exception error) { report.Add("FAIL " + error.GetType().Name); LocalDiagnostics.Record("flow-selftest", error); Environment.ExitCode = 1; }
        finally { File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "flow-selftest.txt"), report); }
    }
}
