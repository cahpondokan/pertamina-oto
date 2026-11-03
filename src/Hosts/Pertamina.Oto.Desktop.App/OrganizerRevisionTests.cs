#if DEBUG
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // Runs on the real WinUI thread with synthetic PDFs, without system UI automation.
    internal async Task TestOrganizerRevisionAsync(string fixture, string? evidence = null)
    {
        var log = new List<string>();
        void Check(bool condition, string name) { if (!condition) throw new InvalidOperationException(name); log.Add("PASS " + name); }
        try
        {
            await OpenOrganizerFilesAsync([fixture]);
            Check(pageLayout.Count >= 3, "fixture loaded");
            var original = pageLayout.ToArray();
            while (pageLayout.Count < 15) pageLayout.Add(original[pageLayout.Count % original.Length].Duplicate());
            RefreshPageGrid(); root.UpdateLayout();
            await Task.WhenAll(organizerThumbnails.Values); await Task.Delay(150);
            root.UpdateLayout(); ArrangeOrganizerCards(pageLayout, false);
            var identities = organizerCards.ToDictionary(p => p.Key, p => (p.Value.Root, p.Value.Picture, p.Value.Picture.Source));
            var cached = organizerThumbnails.Count;
            Check(identities.Values.All(p => p.Source is not null), "real thumbnail bitmaps loaded");
            void Stable(string name)
            {
                Check(identities.Where(p => organizerCards.ContainsKey(p.Key)).All(p => ReferenceEquals(p.Value.Root, organizerCards[p.Key].Root)
                    && ReferenceEquals(p.Value.Picture, organizerCards[p.Key].Picture) && ReferenceEquals(p.Value.Source, organizerCards[p.Key].Picture.Source)), name);
                Check(organizerThumbnails.Count == cached, name + " no new thumbnail request");
            }
            pageGrid.ChangeView(null, 100, null, true); await Task.Delay(100); var scroll = pageGrid.VerticalOffset;
            Check(scroll > 0, "scroll fixture has nonzero offset");
            void PreviewMove(int from, int to, bool commit)
            {
                var before = pageLayout.ToArray(); var card = organizerCards[before[from].Id]; var undo = layoutUndo.Count;
                organizerHeld = card; BeginOrganizerDrag(card);
                var preview = organizerDrag!.Preview(to); ArrangeOrganizerCards(preview, true);
                Check(pageLayout.SequenceEqual(before), "preview leaves committed order unchanged");
                Check(organizerGhost is not null && Math.Abs(organizerGhost.Opacity - .65) < .01 && organizerPlaceholder.Visibility == Visibility.Visible, "transparent ghost and placeholder active");
                Check(preview.Where(p => p.Id != card.Page.Id).Select((p) => organizerCards[p.Id]).All(c => c.X == OrganizerSlot(Array.FindIndex(preview, p => p.Id == c.Page.Id)).X), "affected cards receive preview positions");
                Check(organizerCards.Values.Any(c => c.Motion is not null), "position animation scheduled");
                Stable("preview retains visual identity");
                if (commit) FinishOrganizerDrag(true); else CancelOrganizerDrag();
                Check(pageLayout.SequenceEqual(commit ? preview : before), commit ? "drop commits preview order" : "cancel restores original order");
                Check(pageLayout.Select((p, i) => Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(organizerCards[p.Id].Root).StartsWith($"Halaman {i + 1},", StringComparison.Ordinal)).All(ok => ok), "accessible page numbers follow order");
                Check(pageLayout.Select((p, i) => organizerCards[p.Id].Root.TabIndex == i * 5 + 1).All(ok => ok), "keyboard tab sequence follows visual order");
                Check(organizerGhost is null && organizerPlaceholder.Visibility == Visibility.Collapsed && card.Root.Opacity == 1, "drag visuals cleaned up");
                Check(layoutUndo.Count == undo + (commit && from != to ? 1 : 0), "cancel does not create undo entry");
                Stable("drop/cancel retains visual identity");
                Check(Math.Abs(pageGrid.VerticalOffset - scroll) < 1, "drag retains scroll");
            }
            PreviewMove(0, 9, true); PreviewMove(9, 0, true); PreviewMove(1, organizerColumns + 1, true); PreviewMove(6, 2, false);
            var rotateId = pageLayout[3].Id; var angle = pageLayout[3].Rotation;
            ActOnOrganizerCard(rotateId, "rotate");
            Check(organizerCards[rotateId].Angle.Angle == (angle + 90) % 360, "rotation updates target transform"); Stable("rotation retains unrelated thumbnails");
            foreach (var location in new[] { 0, 1, 2 })
            {
                var gap = location == 0 ? 0 : location == 1 ? 4 : pageLayout.Count;
                var count = pageLayout.Count; InsertOrganizerBlank(gap);
                Check(pageLayout.Count == count + 1 && pageLayout[gap].Source == -1, "blank inserted at requested gap"); Stable("blank insertion retains unrelated cards");
            }
            Check(organizerGaps.Count == pageLayout.Count + 1, "start/end and inter-page gaps available");
            Check(organizerGaps.Select((g, i) => ((Button)g.Children[0]).TabIndex == i * 5).All(ok => ok), "keyboard reaches each gap before its card");
            Check(organizerGaps.All(g => ((Button)g.Children[0]).Opacity == 0), "gap controls hidden without hover/focus");
            var removeId = pageLayout[5].Id; ActOnOrganizerCard(removeId, "remove"); identities.Remove(removeId);
            Check(!organizerCards.ContainsKey(removeId), "only removed card detached"); Stable("delete retains unrelated cards");
            RestoreLayout(false); Check(pageLayout.Any(p => p.Id == removeId), "undo restores deleted page identity"); Stable("undo retains remaining cards");
            var duplicateId = pageLayout[2].Id; var countBefore = pageLayout.Count; ActOnOrganizerCard(duplicateId, "duplicate");
            Check(pageLayout.Count == countBefore + 1 && pageLayout.Select(p => p.Id).Distinct().Count() == pageLayout.Count, "duplicate gets a unique identity");
            Stable("duplicate shares cached bitmap without replacing existing visuals");
            Check(Math.Abs(pageGrid.VerticalOffset - scroll) < 1, "edit operations retain scroll");
            log.Add($"{log.Count} checks PASS. GUI pointer timing/animation smoothness still needs visual acceptance.");
        }
        catch (Exception error) { log.Add("FAIL " + error.GetType().Name + ": " + error.Message); Environment.ExitCode = 1; }
        await File.WriteAllLinesAsync(Path.Combine(evidence ?? AppContext.BaseDirectory, "organizer-revision-tests.txt"), log);
    }
}
#endif
