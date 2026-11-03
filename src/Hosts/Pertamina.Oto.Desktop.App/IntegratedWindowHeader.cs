using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Windowing;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private Grid? integratedWindowHeader;
    private Grid BuildIntegratedWindowHeader()
    {
        var shell = integratedWindowHeader = new Grid { Height = 48, Background = Brush(29,33,40), RequestedTheme = ElementTheme.Dark };
        foreach (var width in new[] { GridLength.Auto, GridLength.Auto, new GridLength(1, GridUnitType.Star), GridLength.Auto, GridLength.Auto, GridLength.Auto })
            shell.ColumnDefinitions.Add(new() { Width = width });
        var brand = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(12,0,14,0), VerticalAlignment = VerticalAlignment.Center };
        brand.Children.Add(new Image { Source = new BitmapImage(new Uri(Path.Combine(AppContext.BaseDirectory,"Assets","oto.png"))), Width = 24, Height = 24 });
        var brandName = new TextBlock { Text = "PERTAMINA OTO", FontSize = 12, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Brush(239,243,249), VerticalAlignment = VerticalAlignment.Center };
        brand.Children.Add(brandName); shell.Children.Add(brand);
        var home = centeredHomeNavigation = new Button { Content = IconLabel("\uE80F", "Beranda"), Height = 36, Padding = new Thickness(10,0,10,0), Margin = new Thickness(0,0,8,0), BorderThickness = new Thickness(0), Background = Brush(29,33,40), Foreground = Brush(239,243,249), CornerRadius = new CornerRadius(6) };
        AutomationProperties.SetName(home, "Beranda");
        home.Click += (_,_) => { if (!messageOpen && !NonReaderBusy && pdfReader?.busy != true) ShowHome(); };
        Grid.SetColumn(home,1); shell.Children.Add(home);
        EnsureReaderTabs(createInitialTab: false);
        readerTabs!.Height = 48; readerTabs.Padding = new Thickness(0,8,0,0); readerTabs.RequestedTheme = ElementTheme.Dark;
        readerTabs.Resources["TabViewBackground"] = Brush(29,33,40);
        readerTabs.Resources["TabViewItemHeaderBackgroundSelected"] = Brush(43,49,59);
        readerTabs.Resources["TabViewItemHeaderBackground"] = Brush(29,33,40);
        readerTabs.Resources["TabViewItemHeaderMinWidth"] = 100d;
        readerTabs.Resources["TabViewItemHeaderMaxWidth"] = 250d;
        Grid.SetColumn(readerTabs,2); shell.Children.Add(readerTabs);
        var menu = new Button { Content = new FontIcon { Glyph = "\uE712", FontSize = 16 }, Width = 36, Height = 36, Padding = new Thickness(0), BorderThickness = new Thickness(0), Background = Brush(29,33,40), Foreground = Brush(239,243,249), Flyout = BuildIntegratedAppMenu() };
        AutomationProperties.SetName(menu,"Menu aplikasi"); ToolTipService.SetToolTip(menu,"Menu aplikasi");
        Grid.SetColumn(menu,3); shell.Children.Add(menu);
        // Only this non-interactive area is the native drag target. Tabs/buttons are
        // outside it, so pointer input and native caption controls retain ownership.
        var drag = new Grid { MinWidth = 48, Background = Brush(29,33,40) };
        var stage = new TextBlock { Text = "Pengembangan", FontSize = 10, Foreground = Brush(168,178,193), Margin = new Thickness(12,0,12,0), VerticalAlignment = VerticalAlignment.Center };
        drag.Children.Add(stage); Grid.SetColumn(drag,4); shell.Children.Add(drag);
        var caption = new Grid(); Grid.SetColumn(caption,5); shell.Children.Add(caption);
        AddNavigationShortcut(shell, Windows.System.VirtualKey.Left, () => Travel(-1));
        AddNavigationShortcut(shell, Windows.System.VirtualKey.Right, () => Travel(1));
        shell.SizeChanged += (_,_) =>
        {
            brandName.Visibility = shell.ActualWidth < 1050 ? Visibility.Collapsed : Visibility.Visible;
            stage.Visibility = shell.ActualWidth < 1200 ? Visibility.Collapsed : Visibility.Visible;
            if (home.Content is StackPanel labels)
                foreach (var label in labels.Children.OfType<TextBlock>()) label.Visibility = shell.ActualWidth < 600 ? Visibility.Collapsed : Visibility.Visible;
            caption.Width = ExtendsContentIntoTitleBar ? AppWindow.TitleBar.RightInset / (shell.XamlRoot?.RasterizationScale ?? 1) : 0;
        };
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            ExtendsContentIntoTitleBar = true;
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            SetTitleBar(drag);
            AppWindow.TitleBar.ButtonBackgroundColor = Windows.UI.Color.FromArgb(255,29,33,40);
            AppWindow.TitleBar.ButtonInactiveBackgroundColor = Windows.UI.Color.FromArgb(255,29,33,40);
            AppWindow.TitleBar.ButtonForegroundColor = Windows.UI.Color.FromArgb(255,239,243,249);
            AppWindow.TitleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(255,156,166,180);
        }
        return shell;
    }

    private bool ReaderRouteVisible => historyPage.Visibility == Visibility.Visible && historyPage.Children.Contains(readerSurface);
    private void ActivateReaderTabFromHeader(PdfReaderView reader)
    {
        if (messageOpen || NonReaderBusy || pdfReader?.busy == true || pdfReader?.openingReaderPath is not null) return;
        ShowPdfReader(false);
        readerTabs!.SelectedItem = FindReaderTab(reader);
        reader.Resume(); reader.FocusReading();
    }
}
