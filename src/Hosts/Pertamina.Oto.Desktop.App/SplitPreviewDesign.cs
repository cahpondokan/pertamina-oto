using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private string splitPreviewKey = "";
    private string splitPreviewSecret = "";
    private readonly Dictionary<int, ImageSource> splitPreviewBitmaps = [];

    private void RefreshSplitDesign()
    {
        if (sourcePreviewHost is null || splitSource is null || splitCount == 0 || selected?.Id != "split") return;
        var ranges = splitMode is SplitMode.Custom or SplitMode.Fixed;
        var key = splitSource + ":" + (ranges ? string.Join(";", splitGroups.Select(g => string.Join(",", g))) : "pages");
        if (splitPreviewKey == key) return;
        splitPreviewKey = key;
        sourcePreviewHost.Children.Clear(); splitTiles.Clear();
        var grid = new GridView { SelectionMode = ListViewSelectionMode.None, IsItemClickEnabled = false, MaxHeight = 600 };
        sourcePreviewHost.Children.Add(ZoomablePreview(grid));
        if (ranges)
        {
            for (var i = 0; i < splitGroups.Length; i++)
            {
                var group = splitGroups[i];
                var stack = new StackPanel { Spacing = 10 };
                stack.Children.Add(Copy($"Bagian {i + 1} · {group.Length} halaman", 13));
                stack.Children.Add(Copy("Halaman " + SplitPlan.Describe(group), 12));
                var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
                row.Children.Add(SplitPageCard(group[0]));
                if (group.Length > 1)
                {
                    row.Children.Add(new TextBlock { Text = "…", VerticalAlignment = VerticalAlignment.Center, FontSize = 22 });
                    row.Children.Add(SplitPageCard(group[^1]));
                }
                stack.Children.Add(row);
                var frame = new Border { Child = stack, Padding = new Thickness(10), BorderBrush = Brush(202, 213, 231), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10) };
                var rangeIndex = i;
                frame.PointerEntered += (_, _) => HighlightSplitRange(rangeIndex, true);
                frame.PointerExited += (_, _) => HighlightSplitRange(rangeIndex, false);
                grid.Items.Add(frame);
            }
        }
        else
        {
            for (var page = 1; page <= splitCount; page++)
            {
                var content = SplitPageContent(page);
                var check = new CheckBox { Content = content, Padding = new Thickness(4) };
                var border = SplitHoverFrame(check);
                var groupLabel = Copy("", 11);
                check.Click += (_, _) =>
                {
                    if (splitUpdating || active is not null || splitMode != SplitMode.SelectedPages) return;
                    splitSelection = string.Join(",", splitTiles.Select((tile, index) => (tile, index)).Where(t => t.tile.Check.IsChecked == true).Select(t => t.index + 1));
                    if (splitSelectionBox is not null) splitSelectionBox.Text = splitSelection;
                    UpdateSplitPlan();
                };
                splitTiles.Add((check, border, groupLabel)); grid.Items.Add(border);
            }
        }
    }

    private void HighlightSplitRange(int index, bool highlighted)
    {
        if (splitMode != SplitMode.Custom || splitCombine) return;
        var rows = splitFields.Children.OfType<Border>().ToArray();
        if (index >= rows.Length) return;
        rows[index].Background = highlighted ? Brush(224, 237, 255) : Brush(242, 245, 250);
    }

    private Border SplitPageCard(int page) => SplitHoverFrame(SplitPageContent(page));

    private static Border SplitHoverFrame(UIElement content)
    {
        var border = new Border { Child = content, Padding = new Thickness(10), Background = Brush(255, 255, 255),
            BorderBrush = Brush(226, 231, 240), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8) };
        border.PointerEntered += (_, _) => border.BorderBrush = Brush(22, 93, 222);
        border.PointerExited += (_, _) => border.BorderBrush = Brush(226, 231, 240);
        border.GotFocus += (_, _) => border.BorderBrush = Brush(22, 93, 222);
        border.LostFocus += (_, _) => border.BorderBrush = Brush(226, 231, 240);
        return border;
    }

    private StackPanel SplitPageContent(int page)
    {
        var content = new StackPanel { Width = 145, Spacing = 8 };
        var image = new Image { Height = 190, Stretch = Stretch.Uniform };
        var generation = splitGeneration; var source = splitSource!; var secret = splitPreviewSecret;
        var loaded = false;
        if (splitPreviewBitmaps.TryGetValue(page, out var bitmap)) { image.Source = bitmap; loaded = true; }
        image.Loaded += async (_, _) =>
        {
            if (loaded || generation != splitGeneration) return;
            loaded = true;
            var info = Copy("", 12);
            await RenderPreviewAsync(source, (uint)(page - 1), image, info, 400, secret);
            if (generation == splitGeneration && image.Source is not null) splitPreviewBitmaps[page] = image.Source;
            if (image.Source is null) ToolTipService.SetToolTip(content, info.Text);
        };
        content.Children.Add(image);
        content.Children.Add(new TextBlock { Text = page.ToString(System.Globalization.CultureInfo.InvariantCulture), FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center });
        ToolTipService.SetToolTip(content, $"Halaman {page}");
        return content;
    }
}
