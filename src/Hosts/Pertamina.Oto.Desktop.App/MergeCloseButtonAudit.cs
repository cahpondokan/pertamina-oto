using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // Parent calls on a loaded, idle Merge screen after layout; does not close any document.
    internal string[] AuditMergeCloseButton()
    {
        if (selected?.Id != "merge" || resultPath is not null || active is not null)
            throw new InvalidOperationException("Close-button audit requires loaded idle Merge before result.");
        root.UpdateLayout();
        var checks = new List<string>();
        void Check(bool ok, string label) => checks.Add((ok ? "PASS " : "FAIL ") + label);
        var buttons = Descendants(body).OfType<Button>().Where(b => AutomationProperties.GetName(b) == "Tutup dokumen").ToArray();
        Check(buttons.Length == 1, "exactly one loaded Merge close action; no footer duplicate");
        var button = buttons.SingleOrDefault();
        if (button is null) return checks.ToArray();
        var card = toolWorkspace.Children.OfType<Border>().FirstOrDefault();
        Check(card is not null && Descendants(card).Contains(button), "close action belongs to Dokumen card");
        Check(button.Parent is Grid heading && Grid.GetColumn(button) == 1 &&
            heading.Children.OfType<TextBlock>().Any(t => t.Text == "Dokumen"), "close aligned right of document title");
        Check(button.Content is TextBlock { Text: "×  Tutup dokumen", TextWrapping: TextWrapping.Wrap }, "icon plus wrapped close label");
        Check(button.MinHeight >= 44 && button.ActualHeight >= 44 && button.MinWidth >= 44, "native minimum hit target 44 DIP");
        Check(button.CornerRadius.TopLeft == 8 && button.UseSystemFocusVisuals && button.IsTabStop, "rounded style and native keyboard focus visuals");
        Check(button.Resources.ThemeDictionaries.ContainsKey("HighContrast"), "high contrast native theme fallback available");
        Check(editable.Contains(button) && button.IsEnabled, "close uses existing editable busy gate");
        Check(button.Focus(FocusState.Keyboard), "close keyboard focus reachable");
        return checks.ToArray();
    }
}
