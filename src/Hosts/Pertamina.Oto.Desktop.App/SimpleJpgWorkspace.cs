using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private int jpgAngle;
    private string? jpgSource;
    private void BuildSimpleJpg(StackPanel inputArea)
    {
        inputArea.Children.Clear();
        var replace = ActionButton("Ganti PDF", async () => await PickFilesAsync());
        replace.HorizontalAlignment = HorizontalAlignment.Right; inputArea.Children.Add(replace);
        sourcePreviewHost = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, Spacing = 8 };
        inputArea.Children.Add(sourcePreviewHost);
        sourcePreviewHost.Loaded += (_, _) => RefreshJpgCard();
        // Detach shared controls from the old action panel before reusing them.
        if (run.Parent is Panel old) old.Children.Clear();
        toolOptions.Children.Clear();
        var all = new RadioButton { Content = "Semua halaman", IsChecked = string.IsNullOrWhiteSpace(pages.Text), GroupName = "jpg-pages" };
        var some = new RadioButton { Content = "Halaman tertentu", IsChecked = !string.IsNullOrWhiteSpace(pages.Text), GroupName = "jpg-pages" };
        pages.Visibility = all.IsChecked == true ? Visibility.Collapsed : Visibility.Visible;
        all.Checked += (_, _) => { pages.Text = ""; pages.Visibility = Visibility.Collapsed; };
        some.Checked += (_, _) => pages.Visibility = Visibility.Visible;
        toolOptions.Children.Add(BuildQualityOptions(false));
        toolOptions.Children.Add(all); toolOptions.Children.Add(some); toolOptions.Children.Add(pages);
        editable.Add(all); editable.Add(some);
        toolOptions.Children.Add(Copy("JPG langsung · Beberapa halaman disimpan dalam satu folder", 12));
        outputName.Visibility = Visibility.Collapsed; password.Visibility = Visibility.Collapsed;
        toolOptions.Children.Add(ActionButton("Opsi tambahan", () => { outputName.Visibility = Visibility.Visible; password.Visibility = Visibility.Visible; }));
        toolOptions.Children.Add(outputName); toolOptions.Children.Add(password);
        toolOptions.Children.Add(BuildOutputLocation());
        run.Content = "Ubah ke JPG"; run.HorizontalAlignment = HorizontalAlignment.Stretch;
        toolOptions.Children.Add(run); toolOptions.Children.Add(cancel); toolOptions.Children.Add(openResult);
        toolOptions.Children.Add(progress); toolOptions.Children.Add(status);
        RefreshJpgCard();
    }

    private void RefreshJpgCard()
    {
        if (sourcePreviewHost is null) return;
        sourcePreviewHost.Children.Clear();
        var path = files.FirstOrDefault();
        if (path is null) { jpgSource = null; jpgAngle = 0; sourcePreviewHost.Children.Add(Copy("Pilih PDF untuk memulai.")); return; }
        if (jpgSource != path) { jpgSource = path; jpgAngle = 0; outputName.Text = Path.GetFileNameWithoutExtension(path) + ".jpg"; }
        var tile = new StackPanel { Width = 210, Padding = new Thickness(12), Spacing = 10 };
        var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 6, Opacity = 0 };
        var rotate = new Button { Content = "↻" }; var remove = new Button { Content = "Keluarkan dari daftar" };
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(rotate, "Putar semua halaman 90 derajat");
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(remove, "Keluarkan PDF dari daftar, bukan hapus file");
        ToolTipService.SetToolTip(rotate, "Putar semua halaman 90°"); ToolTipService.SetToolTip(remove, "Keluarkan dari daftar");
        rotate.Click += (_, _) => { if (active is null) { jpgAngle = (jpgAngle + 90) % 360; RefreshJpgCard(); } };
        remove.Click += (_, _) => RemoveToolInput(path);
        actions.Children.Add(rotate); tile.Children.Add(actions); tile.Children.Add(remove);
        var picture = new Image { Height = 195, Stretch = Stretch.Uniform, RenderTransformOrigin = new Windows.Foundation.Point(.5,.5), RenderTransform = new RotateTransform { Angle = jpgAngle } };
        var view = new Button { Content = picture, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brush(255,255,255), BorderThickness = new Thickness(0) };
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(view, "Perbesar pratinjau " + Path.GetFileName(path));
        view.Click += async (_, _) =>
        {
            if (messageOpen || active is not null) return;
            messageOpen = true;
            try { await new ContentDialog { XamlRoot = root.XamlRoot, Title = "Pratinjau sumber", Content = PreviewPanel(path, angle: jpgAngle, previewPassword: password.Password), CloseButtonText = "Tutup" }.ShowAsync(); }
            catch (Exception) { status.Text = "Pratinjau tidak dapat dibuka."; }
            finally { messageOpen = false; }
        };
        tile.Children.Add(view);
        var name = new TextBlock { Text = Path.GetFileName(path), TextTrimming = TextTrimming.CharacterEllipsis, FontSize = 12 };
        ToolTipService.SetToolTip(name, Path.GetFileName(path)); tile.Children.Add(name);
        var info = Copy("Memuat…", 12); tile.Children.Add(info);
        var border = new Border { Child = tile, Background = Brush(255,255,255), BorderBrush = Brush(221,229,241), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10) };
        border.PointerEntered += (_, _) => { actions.Opacity = 1; border.BorderBrush = Brush(23,104,212); };
        border.PointerExited += (_, _) => { if (rotate.FocusState == FocusState.Unfocused && remove.FocusState == FocusState.Unfocused && view.FocusState == FocusState.Unfocused) actions.Opacity = 0; border.BorderBrush = Brush(221,229,241); };
        border.GotFocus += (_, _) => actions.Opacity = 1;
        border.Tapped += (_, _) => actions.Opacity = 1;
        picture.Loaded += async (_, _) =>
        {
            var count = await RenderPreviewAsync(path, 0, picture, info, 420, password.Password);
            try { var size = (await (await StorageFile.GetFileFromPathAsync(path)).GetBasicPropertiesAsync()).Size; ToolTipService.SetToolTip(border, $"{size / 1024d:N1} KB · {count} halaman"); }
            catch (Exception) { /* Rendering already reports unreadable sources. */ }
        };
        sourcePreviewHost.Children.Add(border);
    }
}
