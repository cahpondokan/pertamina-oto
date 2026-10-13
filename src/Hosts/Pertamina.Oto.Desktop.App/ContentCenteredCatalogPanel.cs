using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Pertamina.Oto.Desktop.App;

// Catalog-only layout. Natural content widths survive wrapping; every row is centered.
internal sealed class ContentCenteredCatalogPanel : Panel
{
    public double Spacing { get; set; } = 12;

    private double Snap(double value)
    {
        var scale = XamlRoot?.RasterizationScale ?? 1;
        return Math.Round(value * scale, MidpointRounding.AwayFromZero) / scale;
    }

    private double CardWidth(UIElement child, double limit) => Math.Min(limit, Snap(child.DesiredSize.Width));

    protected override Size MeasureOverride(Size available)
    {
        var limit = double.IsFinite(available.Width) ? Math.Max(0, available.Width) : double.PositiveInfinity;
        foreach (var child in Children)
        {
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            if (child.Visibility != Visibility.Collapsed && child.DesiredSize.Width > limit)
                child.Measure(new Size(limit, double.PositiveInfinity));
        }
        var rows = Rows(limit);
        return new Size(rows.Count == 0 ? 0 : rows.Max(r => r.Width),
            rows.Sum(r => r.Height) + Math.Max(0, rows.Count - 1) * Spacing);
    }

    protected override Size ArrangeOverride(Size final)
    {
        foreach (var child in Children.Where(c => c.Visibility == Visibility.Collapsed))
            child.Arrange(new Rect(0, 0, 0, 0));
        double y = 0;
        foreach (var row in Rows(Math.Max(0, final.Width)))
        {
            var x = Snap(Math.Max(0, (final.Width - row.Width) / 2));
            foreach (var child in row.Items)
            {
                var width = CardWidth(child, Math.Max(0, final.Width));
                child.Arrange(new Rect(x, y, width, row.Height));
                x += width + Spacing;
            }
            y += row.Height + Spacing;
        }
        return final;
    }

    private List<CatalogRow> Rows(double limit)
    {
        var rows = new List<CatalogRow>();
        CatalogRow? row = null;
        foreach (var child in Children)
        {
            if (child.Visibility == Visibility.Collapsed) continue;
            var width = CardWidth(child, limit);
            if (row is null || row.Items.Count > 0 && row.Width + Spacing + width > limit)
            {
                row = new CatalogRow();
                rows.Add(row);
            }
            row.Width += (row.Items.Count > 0 ? Spacing : 0) + width;
            row.Height = Math.Max(row.Height, child.DesiredSize.Height);
            row.Items.Add(child);
        }
        return rows;
    }

    private sealed class CatalogRow
    {
        internal List<UIElement> Items { get; } = [];
        internal double Width { get; set; }
        internal double Height { get; set; }
    }
}
