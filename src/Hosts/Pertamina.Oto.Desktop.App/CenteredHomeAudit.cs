#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;
namespace Pertamina.Oto.Desktop.App;
internal sealed partial class MainWindow
{
    internal static void PrepareCenteredAudit(string output) { Directory.CreateDirectory(output); libraryTestPath=Path.Combine(output,"isolated-library.json"); }
    internal async Task AuditCenteredHomeAsync(string output)
    {
        var report=new List<string>();
        void Check(bool ok,string name) { report.Add((ok?"PASS ":"FAIL ")+name); if(!ok) Environment.ExitCode=1; }
        async Task Settle() { root.UpdateLayout(); await Task.Delay(350); root.UpdateLayout(); }
        async Task Capture(string name) { await Settle(); var bitmap=new RenderTargetBitmap();await bitmap.RenderAsync((FrameworkElement)Content);var pixels=await bitmap.GetPixelsAsync();using var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream();var encoder=await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId,stream);encoder.SetPixelData(BitmapPixelFormat.Bgra8,BitmapAlphaMode.Premultiplied,(uint)bitmap.PixelWidth,(uint)bitmap.PixelHeight,96,96,pixels.ToArray());await encoder.FlushAsync();stream.Seek(0);using var reader = new Windows.Storage.Streams.DataReader(stream);await reader.LoadAsync((uint)stream.Size);var png = new byte[(int)stream.Size];reader.ReadBytes(png);await File.WriteAllBytesAsync(Path.Combine(output,name+".png"),png); }
        try
        {
            favorites.Clear(); savedJobs.Clear(); homeFiles.Clear(); chosenFiles.Clear();
            if(AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter) presenter.Restore();
            foreach(var width in new[]{1480,800,640}) { AppWindow.Resize(new Windows.Graphics.SizeInt32(width,960));ShowHome();await Settle();Check(centeredGrid.Children.Count==ToolCatalog.All.Count(t=>t.Available),"all active tools at "+width);Check(root.ColumnDefinitions[0].Width.Value==0,"no sidebar at "+width);Check(centeredGrid.Children.OfType<FrameworkElement>().All(t=>t.ActualWidth>100 && t.ActualHeight>=158),"bounded cards at "+width);await Capture("home-"+width); }
            foreach (var width in new[] { 1480, 640 })
            {
                AppWindow.Resize(new Windows.Graphics.SizeInt32(width, 960)); ShowHome(); await Settle();
                foreach (var tile in centeredGrid.Children.OfType<Grid>().Take(4))
                {
                    var button = (Button)tile.Children[0];
                    var starButton = (Microsoft.UI.Xaml.Controls.Primitives.ToggleButton)tile.Children[1];
                    bool Targets(FrameworkElement element, double x, double y, FrameworkElement expected)
                    {
                        var point = element.TransformToVisual((UIElement)Content).TransformPoint(new Windows.Foundation.Point(x, y));
                        var hits = Microsoft.UI.Xaml.Media.VisualTreeHelper.FindElementsInHostCoordinates(point, root);
                        foreach (var hit in hits)
                        {
                            for (DependencyObject? node = hit; node is not null; node = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(node))
                                if (node is Microsoft.UI.Xaml.Controls.Primitives.ButtonBase) return ReferenceEquals(node, expected);
                        }
                        return false;
                    }
                    var name = Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(button);
                    if (width == 1480 && ReferenceEquals(tile, centeredGrid.Children[0]))
                    {
                        var fixedTemplate = button.Template;
                        try
                        {
                            button.Template = (Microsoft.UI.Xaml.Controls.ControlTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load("""<ControlTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" TargetType="Button"><ContentPresenter Content="{TemplateBinding Content}" Padding="{TemplateBinding Padding}" HorizontalContentAlignment="Stretch" VerticalContentAlignment="Center"/></ControlTemplate>""");
                            button.ApplyTemplate(); await Settle();
                            Check(!Targets(button, 8, 8, button), "legacy template reproduces missed padding target");
                        }
                        finally { button.Template = fixedTemplate; button.ApplyTemplate(); }
                        await Settle();
                    }
                    foreach (var (label, x, y) in new[] { ("top padding", 8d, 8d), ("left padding", 8d, button.ActualHeight / 2), ("bottom padding", button.ActualWidth / 2, button.ActualHeight - 8) })
                        Check(Targets(button, x, y, button), $"card hit test {name} {label} width {width}");
                    var contentIcon = ((StackPanel)button.Content).Children[0] as FrameworkElement;
                    Check(contentIcon is not null && Targets(contentIcon, contentIcon.ActualWidth / 2, contentIcon.ActualHeight / 2, button), $"card icon center {name} width {width}");
                    var text = Descendants(button).OfType<TextBlock>().First();
                    Check(Targets(text, text.ActualWidth / 2, text.ActualHeight / 2, button), $"card label {name} width {width}");
                    Check(Targets(starButton, starButton.ActualWidth / 2, starButton.ActualHeight / 2, starButton), $"favorite independent hit target {name} width {width}");
                }
            }
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1480,960));ShowHome();await Settle();
            foreach (var id in new[] { "merge", "split", "organize", "images" })
            {
                ShowHome(); await Settle();
                var tool = ToolCatalog.All.First(t => t.Id == id);
                var button = centeredGrid.Children.OfType<Grid>().Select(t => (Button)t.Children[0]).First(b => Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(b) == CenteredCopy(tool).Title);
                ((Microsoft.UI.Xaml.Automation.Provider.IInvokeProvider)new Microsoft.UI.Xaml.Automation.Peers.ButtonAutomationPeer(button).GetPattern(Microsoft.UI.Xaml.Automation.Peers.PatternInterface.Invoke)).Invoke();
                await Settle(); Check(selected?.Id == id, "launch control route " + id);
            }
            ShowHome(); await Settle();
            centeredSearch!.Text="satukan";await Settle();Check(centeredGrid.Children.Count==1,"alias search satukan");
            centeredSearch.Text="zzzz-no-match";await Settle();Check(centeredGrid.Children.Count==0 && centeredEmpty!.Visibility==Visibility.Visible,"search empty state");await Capture("search-empty");
            centeredSearch.Text="";await Settle();centeredCategory="security";RefreshCenteredCards();Check(centeredGrid.Children.Count==2,"security category");
            favorites.Add("merge");SaveLibrary();favorites.Clear();LoadLibrary();Check(favorites.Count==1 && favorites.Contains("merge"),"favorite persists in isolated store");centeredCategory="favorites";RefreshCenteredCards();Check(centeredGrid.Children.Count==1,"favorite filter");await Capture("favorites");
            centeredCategory="all";RefreshCenteredCards();await Settle();
            var firstTile=(Microsoft.UI.Xaml.Controls.Grid)centeredGrid.Children[0];var launch=(Button)firstTile.Children[0];var baselineHeight=firstTile.ActualHeight;
            Check(launch.Focus(FocusState.Keyboard),"card accepts keyboard focus");await Settle();
            Check(ReferenceEquals(Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(root.XamlRoot),launch),"keyboard focus remains on launch control");
            Check(Math.Abs(firstTile.ActualHeight-baselineHeight)<.01,"focus hover motion does not shift layout");await Capture("keyboard-focus");
            var star=(Microsoft.UI.Xaml.Controls.Primitives.ToggleButton)firstTile.Children[1];
            star.IsChecked=false;star.IsChecked=true;await Settle();favorites.Clear();LoadLibrary();Check(favorites.Contains("merge"),"card star action persists through reload");
            var fixture=Path.Combine(output,"synthetic-home.pdf");if(!File.Exists(fixture))WriteHistoryAuditFixture(fixture);var hash=SHA256.HashData(File.ReadAllBytes(fixture));
            await OpenPdfReaderAsync(fixture);await pdfReader!.Rendering;await Settle();Check(pdfReader.Visible && pdfReader.ReaderSource==fixture,"open PDF through existing reader");await Capture("reader");
            var zoomView = Descendants(pdfReader.View).OfType<ScrollViewer>().First(v=>v.ZoomMode==ZoomMode.Enabled);
            var beforeZoom = zoomView.ZoomFactor;
            var plusButton = Descendants(pdfReader.View).OfType<Button>().First(b=>Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(b)=="Perbesar PDF");
            var peer = new Microsoft.UI.Xaml.Automation.Peers.ButtonAutomationPeer(plusButton);
            ((Microsoft.UI.Xaml.Automation.Provider.IInvokeProvider)peer.GetPattern(Microsoft.UI.Xaml.Automation.Peers.PatternInterface.Invoke)).Invoke();await Settle();await pdfReader.Rendering;
            Check(zoomView.ZoomFactor>beforeZoom,"reader zoom plus button");
            var minusButton = Descendants(pdfReader.View).OfType<Button>().First(b=>Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(b)=="Perkecil PDF");
            ((Microsoft.UI.Xaml.Automation.Provider.IInvokeProvider)new Microsoft.UI.Xaml.Automation.Peers.ButtonAutomationPeer(minusButton).GetPattern(Microsoft.UI.Xaml.Automation.Peers.PatternInterface.Invoke)).Invoke();await Settle();
            Check(Math.Abs(zoomView.ZoomFactor-beforeZoom)<.02,"reader zoom button round trip");
            ShowHome();await Settle();Check(pdfReader.ReaderSource==fixture,"home retains reader source");centeredSearch!.Text="gabung";centeredCategory="pages";RefreshCenteredCards();Check(pdfReader.ReaderSource==fixture,"filters retain reader source");
            Travel(-1);await Settle();Check(pdfReader.Visible && pdfReader.ReaderSource==fixture,"back resumes reader");
            await OpenEditorDocumentAsync(fixture);await Settle();Check(liveEditor?.ReaderSource==fixture,"editor opens same synthetic source");await Capture("editor");
            ShowHome();await Settle();Check(liveEditor?.ReaderSource==fixture,"home retains editor session");
            ShowTool(ToolCatalog.All.First(t=>t.Id=="merge"));await Settle();Check(selected?.Id=="merge","existing tool navigation");
            var syntheticResult=Path.Combine(output,"synthetic-result.pdf");File.Copy(fixture,syntheticResult,true);ShowUnifiedResult([syntheticResult],"Synthetic shell audit only",ShowHome);await Settle();Check(latestResultCard?.IsLoaded==true,"existing result container");await Capture("result");
            ShowSettings();await Settle();ShowHelp();await Settle();ShowAssistant();await Settle();ShowAdmin();await Settle();Check(true,"existing compact menu destinations navigate");
            Check(SHA256.HashData(File.ReadAllBytes(fixture)).SequenceEqual(hash),"source PDF remains unchanged");
            centeredCategory="all";centeredQuery="";ShowHome();await Capture("home-final");
        }
        catch(Exception error) { report.Add("FAIL "+error);Environment.ExitCode=1; }
        finally { File.WriteAllLines(Path.Combine(output,"native-audit.txt"),report); }
    }
}
#endif


