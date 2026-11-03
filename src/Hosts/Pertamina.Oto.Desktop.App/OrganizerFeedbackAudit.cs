#if DEBUG
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // Real storage/DataPackage adapter and rendered WinUI controls; no physical Explorer-drag claim.
    private async Task AuditOrganizerFeedbackAsync(string output, string a, string b, Action<bool, string> check,
        Func<Task> settle, Func<string, UIElement?, Task> capture)
    {
        var invalid = Path.Combine(output, "invalid.txt");
        File.WriteAllText(invalid, "Synthetic non-PDF input; no user data.");
        var invalidHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(invalid)));
        await OpenOrganizerFilesAsync([a]); folder = output; await settle();
        var before = pageLayout.ToArray(); var undoCount = layoutUndo.Count; var redoCount = layoutRedo.Count;
        InfoBar Notice() => Descendants(organizerCanvas!).OfType<InfoBar>().Single(n => n.Name == "OrganizerInputNotice");
        async Task Import(params string[] paths)
        {
            var items = new List<IStorageItem>();
            foreach (var path in paths) items.Add(await StorageFile.GetFileFromPathAsync(path));
            var package = new DataPackage(); package.SetStorageItems(items);
            await AcceptOrganizerFilesAsync(await package.GetView().GetStorageItemsAsync()); await settle();
        }
        check(!Notice().IsOpen, "QA-02 successful import starts with notification hidden");
        await Import(invalid);
        check(pageLayout.SequenceEqual(before) && organizerSources.Count == 1 && layoutUndo.Count == undoCount && layoutRedo.Count == redoCount,
            "QA-02 invalid-only import retains exact draft sources and Undo/Redo");
        check(Notice() is { IsOpen: true, Visibility: Visibility.Visible, ActualHeight: > 0 }
            && Notice().Message.StartsWith("1 file ditolak", StringComparison.Ordinal), "QA-02 invalid-only rejection renders after hidden state");
        await Import(b, invalid);
        check(pageLayout.Count == 6 && organizerSources.Count == 2 && pageLayout.Take(before.Length).SequenceEqual(before),
            "QA-02 mixed import accepts valid PDF and excludes invalid item");
        RefreshPageGrid(); await settle();
        check(Notice().IsOpen && Notice().Message.StartsWith("1 file ditolak", StringComparison.Ordinal) && run.IsEnabled,
            "QA-02 mixed rejection survives grid refresh with saving enabled");
        await capture("feedback-mixed-1400", null);
        AppWindow.Resize(new Windows.Graphics.SizeInt32(660, 810)); await settle();
        standardPageScroll!.ChangeView(null, 0, null, true); await settle();
        var bounds = Notice().TransformToVisual(root).TransformBounds(new Windows.Foundation.Rect(0, 0, Notice().ActualWidth, Notice().ActualHeight));
        check(bounds.Top >= 0 && bounds.Bottom <= root.ActualHeight && bounds.Right <= root.ActualWidth,
            "QA-02 warning visible by drop area at minimum window width");
        await capture("feedback-mixed-660", null);
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1400, 1000)); await settle();
        await Import(a); check(!Notice().IsOpen, "QA-02 following valid import clears old rejection");
        await Import(invalid); check(Notice().IsOpen, "QA-02 repeated rejection reopens notification after collapse");
        check(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(invalid))) == invalidHash,
            "QA-02 rejected file bytes unchanged");
        check(run.Style == (Style)Application.Current.Resources["AccentButtonStyle"] && run.IsEnabled,
            "Organizer save uses enabled primary button style");
        foreach (var state in new[] { "Normal", "PointerOver", "Pressed" })
        {
            check(VisualStateManager.GoToState(run, state, false), "Organizer save native state " + state);
            await capture("save-button-" + state, run);
        }
        run.IsEnabled = false; await capture("save-button-disabled", run);
        RefreshPageGrid(); await settle();
        check(run.IsEnabled, "Organizer save readiness restores enabled state");
    }
}
#endif
