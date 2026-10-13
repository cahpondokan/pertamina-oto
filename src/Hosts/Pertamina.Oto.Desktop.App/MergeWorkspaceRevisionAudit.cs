#if DEBUG
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private static readonly string[] MergeRevisionActionOrder = ["↻", "×", "⤢"];
    // Integrator calls on the synthetic loaded Merge fixture, before invoking destructive actions.
    internal async Task<string[]> AuditMergeWorkspaceRevisionAsync()
    {
        if (selected?.Id != "merge" || files.Count < 2 || active is not null || mergeTiles is null)
            throw new InvalidOperationException("Revision audit requires idle loaded Merge with two synthetic inputs.");
        root.UpdateLayout();
        var report = new List<string>();
        void Check(bool ok, string name) => report.Add((ok ? "PASS " : "FAIL ") + name);
        Check(body.Children[0] is Grid header && Descendants(header).OfType<ToggleButton>().Count() == 1,
            "compact title and favorite share first row");
        var limits = body.Children.OfType<Expander>().FirstOrDefault();
        Check(limits is { IsExpanded: false }, "development limitations default collapsed");
        Check(limits?.Header is Grid summary && Descendants(summary).OfType<TextBlock>().Any(t =>
            t.Text == "Versi pengembangan · Periksa hasil sebelum digunakan."), "exact development summary");
        Check(limits?.Content is StackPanel detail && detail.Children.OfType<TextBlock>().Any(t =>
            t.Text == "Batas pemrosesan: 100 MB · 1000 halaman"), "exact processing bounds");
        foreach (var tile in mergeCards.Values)
        {
            var buttons = Descendants(tile).OfType<Button>().ToArray();
            Check(buttons.Length == 3, "three actions per input card");
            Check(buttons.All(b => b.MinWidth >= 44 && b.MinHeight >= 44 && b.IsTabStop && b.UseSystemFocusVisuals),
                "native keyboard focus and 44 DIP targets");
            Check(buttons.Select(b => b.Content?.ToString()).SequenceEqual(MergeRevisionActionOrder), "rotate remove preview order");
            Check(buttons.All(b => !string.IsNullOrWhiteSpace(AutomationProperties.GetName(b))), "all actions accessible names");
            Check(!buttons.Any(b => b.Content?.ToString() == "Keluarkan dari daftar"), "no large below-card remove button");
            if (buttons.Length != 3) continue;
            var strip = (StackPanel)buttons[0].Parent;
            Check(buttons[0].Focus(FocusState.Keyboard), "rotate reachable by keyboard");
            await Task.Delay(30);
            Check(strip.Opacity == 1 && strip.IsHitTestVisible, "keyboard focus reveals actions");
            buttons[1].Focus(FocusState.Keyboard); await Task.Delay(30);
            Check(strip.Opacity == 1 && strip.IsHitTestVisible, "moving focus within strip retains visibility");
        }
        return report.ToArray();
    }
}
#endif
