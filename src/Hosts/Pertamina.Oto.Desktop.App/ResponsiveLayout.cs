using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Pertamina.Oto.Desktop.App;

// A bounded action row: child order and instances stay stable during reflow.
internal sealed class ResponsiveWrapPanel : Panel
{
    public double Spacing { get; set; } = 8;
    protected override Size MeasureOverride(Size available)
    {
        var width = double.IsFinite(available.Width) ? Math.Max(0, available.Width) : double.PositiveInfinity;
        double x = 0, y = 0, rowHeight = 0, used = 0;
        foreach (var child in Children)
        {
            child.Measure(new Size(width, double.PositiveInfinity));
            if (child.Visibility == Visibility.Collapsed) continue;
            var size = child.DesiredSize;
            if (x > 0 && x + size.Width > width) { used = Math.Max(used, x - Spacing); y += rowHeight + Spacing; x = rowHeight = 0; }
            x += size.Width + Spacing; rowHeight = Math.Max(rowHeight, size.Height);
        }
        return new Size(Math.Min(width, Math.Max(used, Math.Max(0, x - Spacing))), y + rowHeight);
    }
    protected override Size ArrangeOverride(Size final)
    {
        double x = 0, y = 0, rowHeight = 0;
        foreach (var child in Children)
        {
            if (child.Visibility == Visibility.Collapsed) { child.Arrange(new Rect(0, 0, 0, 0)); continue; }
            var size = child.DesiredSize; var width = Math.Min(final.Width, size.Width);
            if (x > 0 && x + width > final.Width) { y += rowHeight + Spacing; x = rowHeight = 0; }
            child.Arrange(new Rect(x, y, Math.Max(0, width), size.Height));
            x += width + Spacing; rowHeight = Math.Max(rowHeight, size.Height);
        }
        return final;
    }
}

internal static class ResponsiveLayout
{
    internal static Action Header(Grid grid, FrameworkElement title, FrameworkElement actions, double breakpoint = 720, Func<bool>? allowNarrow = null)
    {
        while (grid.RowDefinitions.Count < 2) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var originalColumn = Grid.GetColumn(title); var originalSpan = Grid.GetColumnSpan(title);
        bool? current = null;
        void Apply()
        {
            // A numeric breakpoint alone can leave the title just a few pixels
            // after an action becomes visible (reader at 800 DIP). Budget the
            // unwrapped action row as well; keep a useful title column.
            var actionWidth = actions is ResponsiveWrapPanel row
                ? row.Children.Where(c => c.Visibility == Visibility.Visible).Sum(c => c.DesiredSize.Width)
                    + Math.Max(0, row.Children.Count(c => c.Visibility == Visibility.Visible) - 1) * row.Spacing
                : 0;
            var titleRoom = grid.ActualWidth - grid.Padding.Left - grid.Padding.Right
                - Math.Max(0, grid.ColumnDefinitions.Count - 1) * grid.ColumnSpacing - actionWidth;
            var narrow = (grid.ActualWidth < breakpoint || actionWidth > 0 && titleRoom < 200)
                && (allowNarrow?.Invoke() ?? true);
            if (narrow == current) return; current = narrow;
            Grid.SetColumn(title, narrow ? 0 : originalColumn); Grid.SetColumnSpan(title, narrow ? grid.ColumnDefinitions.Count : originalSpan);
            Grid.SetColumn(actions, narrow ? 0 : grid.ColumnDefinitions.Count - 1);
            Grid.SetColumnSpan(actions, narrow ? grid.ColumnDefinitions.Count : 1);
            Grid.SetRow(actions, narrow ? 1 : 0);
            actions.Margin = new Thickness(0, narrow ? 8 : 0, 0, 0);
        }
        grid.SizeChanged += (_, _) => Apply(); actions.SizeChanged += (_, _) => Apply();
        grid.Loaded += (_, _) => Apply(); return Apply;
    }
    internal static void BoundDialog(ContentDialog dialog, FrameworkElement content, FrameworkElement viewport)
    {
        void Apply()
        {
            var width = Math.Max(80, viewport.ActualWidth - 72);
            content.MaxWidth = Math.Min(760, width);
            content.MaxHeight = Math.Max(60, viewport.ActualHeight - 180);
            dialog.Resources["ContentDialogMinWidth"] = Math.Min(280, width);
            dialog.Resources["ContentDialogMaxWidth"] = Math.Min(808, width + 48);
        }
        void Resize(object sender, SizeChangedEventArgs e) => Apply();
        dialog.Opened += (_, _) => { Apply(); viewport.SizeChanged += Resize; };
        dialog.Closed += (_, _) => viewport.SizeChanged -= Resize;
    }
}
