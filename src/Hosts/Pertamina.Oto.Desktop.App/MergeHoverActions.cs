using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private StackPanel CreateMergeHoverActions(StackPanel tile, Func<bool> canAct,
        Action rotate, Action remove, Func<Task> preview)
    {
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top,
            Opacity = 0, IsHitTestVisible = false };
        Button Small(string glyph, string name)
        {
            var button = new Button { Content = glyph, Width = 44, Height = 44,
                MinWidth = 44, MinHeight = 44, Padding = new Thickness(4) };
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(button, name);
            ToolTipService.SetToolTip(button, name);
            actions.Children.Add(button);
            return button;
        }
        Small("↻", "Putar seluruh halaman dokumen 90°").Click += (_, _) => { if (canAct()) rotate(); };
        Small("×", "Keluarkan dari daftar; file asli tidak dihapus").Click += (_, _) => { if (canAct()) remove(); };
        Small("⤢", "Perbesar dan baca pratinjau PDF").Click += async (_, _) => { if (canAct()) await preview(); };
        bool pointerInside = false;
        bool HasFocus()
        {
            if (tile.XamlRoot is null) return false;
            var current = FocusManager.GetFocusedElement(tile.XamlRoot) as DependencyObject;
            while (current is not null)
            {
                if (ReferenceEquals(current, tile)) return true;
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }
        void Refresh()
        {
            var visible = pointerInside || HasFocus();
            actions.Opacity = visible ? 1 : 0;
            actions.IsHitTestVisible = visible;
        }
        tile.PointerEntered += (_, _) => { pointerInside = true; Refresh(); };
        tile.PointerExited += (_, _) => { pointerInside = false; Refresh(); };
        tile.GotFocus += (_, _) => Refresh();
        // Defer until focus has moved; moving between the three buttons keeps the strip visible.
        tile.LostFocus += (_, _) => DispatcherQueue.TryEnqueue(Refresh);
        return actions;
    }
}
