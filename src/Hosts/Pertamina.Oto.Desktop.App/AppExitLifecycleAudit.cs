#if DEBUG
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // Runs only inside isolated synthetic audit sessions; never confirms application exit.
    private async Task AuditAppExitRetentionAsync(Action<bool, string> check)
    {
        var before = CurrentToolDraft();
        var beforeSecret = CurrentSessionSecret(selected?.Id ?? "");
        var draftCount = toolDrafts.Count;
        var selectedId = selected?.Id;
        var result = resultPath;
        var pending = RestartPendingWork().Count;
        ContentDialog Popup() => VisualTreeHelper.GetOpenPopupsForXamlRoot(root.XamlRoot)
            .SelectMany(p => Descendants(p.Child).Prepend(p.Child)).OfType<ContentDialog>().Single();
        void Invoke(string name) => ((IInvokeProvider)new ButtonAutomationPeer(
            Descendants(Popup()).OfType<Button>().Single(b => b.Name == name)).GetPattern(PatternInterface.Invoke)).Invoke();
        var returning = ConfirmCloseAsync(); await Task.Delay(300);
        check(Descendants(Popup()).OfType<Button>().Any(b => b.Name == "AppExitReturn"), "main close uses universal exit dialog");
        var duplicate = ConfirmCloseAsync(); await duplicate;
        check(messageOpen && !allowClose, "reentrant close retains single dialog and close authority");
        Invoke("AppExitReturn"); await returning;
        var after = CurrentToolDraft();
        check(before.Files.SequenceEqual(after.Files) && (before with { Files = after.Files }) == after
            && beforeSecret == CurrentSessionSecret(selectedId ?? "") && draftCount == toolDrafts.Count
            && selected?.Id == selectedId && resultPath == result && pending == RestartPendingWork().Count,
            "main exit return preserves exact input/options/password/result/pending sessions");
        check(!allowClose && !messageOpen, "main exit return releases modal guard without authorizing exit");
        using var operation = new CancellationTokenSource();
        active = operation;
        try
        {
            var refusal = ConfirmCloseAsync(); await Task.Delay(300);
            check(Popup().Title as string == "Pekerjaan sedang berjalan", "busy main exit refuses universal confirmation");
            Invoke("CloseButton"); await refusal;
            check(!allowClose && !operation.IsCancellationRequested, "busy refusal neither cancels work nor authorizes exit");
        }
        finally { active = null; }
    }
}
#endif
