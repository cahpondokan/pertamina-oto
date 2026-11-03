using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private sealed partial class PdfReaderView
    {
        private Grid? integratedToolbar;
        private Action? layoutIntegratedToolbar;
        private static void ReaderToolbarButton(Button button, string glyph, string? label = null, bool primary = false)
        {
            var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 7, VerticalAlignment = VerticalAlignment.Center };
            content.Children.Add(new FontIcon { Glyph = glyph, FontSize = 15 });
            if (label is not null) content.Children.Add(new TextBlock { Text = label, FontSize = 12, VerticalAlignment = VerticalAlignment.Center });
            button.Content = content; button.MinHeight = 34; button.MinWidth = 34;
            button.Padding = new Thickness(label is null ? 8 : 10,4,label is null ? 8 : 10,4);
            button.Background = primary ? Brush(8,102,232) : Brush(43,49,59);
            button.Foreground = Brush(239,243,249); button.BorderThickness = new Thickness(0);
            button.CornerRadius = new CornerRadius(5);
        }
        private void BuildIntegratedReaderToolbar(Grid legacyHeader, Grid legacyToolbar, Grid footer,
            ResponsiveWrapPanel actions, ResponsiveWrapPanel tools, Grid search, ScrollViewer controlsScroll,
            ResponsiveWrapPanel controls, Button focus, Button thumbnailsButton)
        {
            legacyHeader.Visibility = legacyToolbar.Visibility = Visibility.Collapsed;
            actions.Children.Remove(print); actions.Children.Remove(edit); actions.Children.Remove(more); actions.Children.Remove(focus);
            tools.Children.Remove(organize); tools.Children.Remove(thumbnailsButton);
            legacyToolbar.Children.Remove(search); legacyToolbar.Children.Remove(bookmark);
            footer.Children.Remove(controlsScroll); controlsScroll.Content = null;
            var bar = integratedToolbar = new Grid { Padding = new Thickness(12,8,12,8), ColumnSpacing = 12, RowSpacing = 6,
                RequestedTheme = ElementTheme.Dark, Background = Brush(43,49,59) };
            bar.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
            bar.ColumnDefinitions.Add(new());
            bar.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
            bar.RowDefinitions.Add(new() { Height = GridLength.Auto }); bar.RowDefinitions.Add(new() { Height = GridLength.Auto });
            var left = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
            ReaderToolbarButton(thumbnailsButton,"\uE8A5"); ToolTipService.SetToolTip(thumbnailsButton,"Halaman");
            var read = new Border { Background = Brush(60,77,99), CornerRadius = new CornerRadius(5), Padding = new Thickness(12,6,12,6), VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock { Text = "Baca", FontSize = 12, Foreground = Brush(218,233,255), FontWeight = Microsoft.UI.Text.FontWeights.SemiBold } };
            ReaderToolbarButton(organize,"\uE80A","Atur halaman");
            left.Children.Add(thumbnailsButton); left.Children.Add(read); left.Children.Add(organize); bar.Children.Add(left);
            searchInput.MinWidth = 80; searchInput.PlaceholderText = "Cari isi PDF";
            ToolTipService.SetToolTip(searchInput,"Ctrl+F");
            foreach (var b in search.Children.OfType<Button>()) { ReaderToolbarButton(b,"\uE721"); ToolTipService.SetToolTip(b,"Ctrl+F"); }
            search.VerticalAlignment = VerticalAlignment.Center; search.MaxWidth = 360; search.HorizontalAlignment = HorizontalAlignment.Stretch;
            Grid.SetColumn(search,1); Grid.SetRow(search,0); Grid.SetColumnSpan(search,1); bar.Children.Add(search);
            ReaderToolbarButton(previous,"\uE76B"); ReaderToolbarButton(next,"\uE76C");
            count.Foreground = Brush(219,226,235); count.FontSize = 12; count.VerticalAlignment = VerticalAlignment.Center;
            pageInput.Width = 46; pageInput.MinHeight = 34;
            foreach (var label in zoomControls.Toolbar.Children.OfType<TextBlock>()) label.Visibility = Visibility.Collapsed;
            var zoomButtons = zoomControls.Toolbar.Children.OfType<Button>().ToArray();
            for (var i = 0; i < zoomButtons.Length; i++) ReaderToolbarButton(zoomButtons[i], i == 0 ? "\uE738" : i == 1 ? "\uE710" : "\uE9A6");
            zoomControls.Percent.Width = 106; zoomControls.Percent.MinHeight = 34;
            controls.Spacing = 3; controls.VerticalAlignment = VerticalAlignment.Center;
            var right = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
            ReaderToolbarButton(print,"\uE749","Cetak"); ToolTipService.SetToolTip(print,"Ctrl+P");
            ReaderToolbarButton(edit,"\uE70F","Edit PDF",true);
            ReaderToolbarButton(focus,"\uE740"); ToolTipService.SetToolTip(focus,"Mode fokus");
            ReaderToolbarButton(bookmark,"\uE734"); ReaderToolbarButton(more,"\uE712");
            right.Children.Add(controls); right.Children.Add(print); right.Children.Add(edit); right.Children.Add(focus); right.Children.Add(bookmark); right.Children.Add(more);
            Grid.SetColumn(right,2); bar.Children.Add(right);
            Grid.SetRow(bar,1); View.Children.Add(bar);
            // Keep the original document metadata available without a repeated title.
            footer.Children.Clear(); footer.ColumnDefinitions.Clear(); footer.Padding = new Thickness(16,5,16,5);
            footer.Children.Add(new TextBlock { Text = "Offline · File asli tetap utuh", FontSize = 11, Foreground = Brush(88,106,128) });
            layoutIntegratedToolbar = () =>
            {
                var narrow = View.ActualWidth < 1250;
                Grid.SetRow(right,narrow ? 1 : 0); Grid.SetColumn(right,narrow ? 0 : 2); Grid.SetColumnSpan(right,narrow ? 3 : 1);
                right.HorizontalAlignment = narrow ? HorizontalAlignment.Center : HorizontalAlignment.Right;
                // At compact widths all commands remain available; labels give way
                // to named icon controls, while page/zoom controls keep their sizes.
                foreach (var b in new[] { print,edit,organize })
                    if (b.Content is StackPanel group) foreach (var label in group.Children.OfType<TextBlock>()) label.Visibility = View.ActualWidth < 850 ? Visibility.Collapsed : Visibility.Visible;
                search.MaxWidth = narrow ? double.PositiveInfinity : 360;
                Grid.SetColumnSpan(search,narrow ? 2 : 1);
            };
            bar.SizeChanged += (_,_) => layoutIntegratedToolbar();
            readerChrome = [bar, footer, workspace];
        }
    }
}
