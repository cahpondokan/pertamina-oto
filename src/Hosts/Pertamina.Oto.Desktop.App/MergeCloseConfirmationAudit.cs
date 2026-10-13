#if DEBUG
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // Parent owns dispatch and evidence persistence. This helper never processes or changes a document.
    private async Task<IReadOnlyList<string>> AuditMergeCloseConfirmationAsync(Func<ContentDialog, string, Task>? capture = null)
    {
        var report = new List<string>();
        void Check(bool ok, string label)
        {
            report.Add((ok ? "PASS " : "FAIL ") + label);
            if (!ok) throw new InvalidOperationException(label);
        }
        foreach (var action in new[] { ContentDialogResult.None, ContentDialogResult.Primary, ContentDialogResult.Secondary })
        {
            var view = BuildMergeCloseConfirmation();
            var showing = view.Dialog.ShowAsync();
            try
            {
                await Task.Delay(250); view.Dialog.UpdateLayout();
                Check(view.Dialog.PrimaryButtonText.Length == 0 && view.Dialog.SecondaryButtonText.Length == 0 && view.Dialog.CloseButtonText.Length == 0,
                    "no duplicate footer actions");
                Check(Descendants(view.Dialog).OfType<Button>().Count(b => b.Name.StartsWith("MergeClose", StringComparison.Ordinal)) == 3, "exactly three Merge actions");
                Check(view.Return.FocusState != FocusState.Unfocused, "safe initial focus");
                Check(view.Result == ContentDialogResult.None, "dismiss defaults to retain");
                foreach (var button in new[] { view.Save, view.Discard, view.Return })
                {
                    Check(button.ActualWidth >= 44 && button.ActualHeight >= 44, button.Name + " hit target");
                    Check(AutomationProperties.GetName(button).Length > 0, button.Name + " accessible name");
                    Check(button.ActualWidth <= root.ActualWidth && button.ActualHeight <= root.ActualHeight, button.Name + " bounded");
                }
                var background = Descendants(view.Dialog).OfType<FrameworkElement>().FirstOrDefault(e => e.Name == "BackgroundElement");
                Check(background is not null && background.ActualWidth <= root.ActualWidth && background.ActualHeight <= root.ActualHeight, "modal fits current client");
                if (capture is not null) await capture(view.Dialog, "merge-close-" + action);
                var buttonToInvoke = action == ContentDialogResult.Primary ? view.Save : action == ContentDialogResult.Secondary ? view.Discard : view.Return;
                ((IInvokeProvider)new ButtonAutomationPeer(buttonToInvoke).GetPattern(PatternInterface.Invoke)).Invoke();
                await showing;
                Check(view.Result == action, "action mapping " + action);
            }
            finally { view.Dialog.Hide(); await showing; }
        }
        report.Add("LIMIT physical Enter/Escape, screen reader and live high-contrast switching require parent/native verification.");
        return report;
    }
}
#endif
