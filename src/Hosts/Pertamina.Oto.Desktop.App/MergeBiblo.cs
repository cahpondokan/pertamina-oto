using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private static Image MergeBiblo(double size) => new()
    {
        Width = size, Height = size, Stretch = Stretch.Uniform,
        HorizontalAlignment = HorizontalAlignment.Center,
        Source = new BitmapImage(new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "Biblo", "028-gabungkan-pdf.png")))
    };
}
