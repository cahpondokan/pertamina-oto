#if DEBUG
using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;
internal sealed partial class MainWindow
{
    internal async Task AuditCrashStabilityAsync(string output)
    {
        Directory.CreateDirectory(output);
        var report = new List<string>();
        void Step(string name) { report.Add("STEP " + name); File.WriteAllLines(Path.Combine(output,"progress.txt"),report); }
        void Check(bool ok, string name) { report.Add((ok ? "PASS " : "FAIL ") + name); if(!ok) Environment.ExitCode=1; File.WriteAllLines(Path.Combine(output,"progress.txt"),report); }
        async Task Settle(int milliseconds=50) { root.UpdateLayout(); await Task.Delay(milliseconds); root.UpdateLayout(); }
        try
        {
            Title = "OTO synthetic crash audit 01a128b8";
            if(Environment.GetCommandLineArgs().Contains("--menu-only"))
            {
                await Settle(300);
                var button=Descendants((UIElement)Content).OfType<Button>().Single(b=>b.Flyout is MenuFlyout f && f.Items.OfType<MenuFlyoutSubItem>().Any(s=>s.Text=="Tampilan"));
                var menu=(MenuFlyout)button.Flyout;
                for(var repeat=0;repeat<3;repeat++) foreach(var sub in menu.Items.OfType<MenuFlyoutSubItem>())
                {
                    Step($"submenu {repeat} {sub.Text}");menu.ShowAt(button);await Settle(200);
                    var peer=Microsoft.UI.Xaml.Automation.Peers.FrameworkElementAutomationPeer.CreatePeerForElement(sub);
                    var provider=(Microsoft.UI.Xaml.Automation.Provider.IExpandCollapseProvider)peer.GetPattern(Microsoft.UI.Xaml.Automation.Peers.PatternInterface.ExpandCollapse);
                    provider.Expand();await Settle(400);Check(true,$"submenu expanded {repeat} {sub.Text}");provider.Collapse();menu.Hide();await Settle(100);
                }
                Check(root.KeyboardAcceleratorPlacementMode==Microsoft.UI.Xaml.Input.KeyboardAcceleratorPlacementMode.Hidden,"root does not auto-format shortcut tooltips");
                foreach(var key in new[]{107,109,187,189,191}) Check(root.KeyboardAccelerators.Any(k=>(int)k.Key==key),"symbol shortcut still registered "+key);
                return;
            }
            var fixture=Path.Combine(output,"source.pdf"); WriteHistoryAuditFixture(fixture);
            var second=Path.Combine(output,"second.pdf"); File.Copy(fixture,second,true);
            var hash=SHA256.HashData(File.ReadAllBytes(fixture));
            homeFiles.Clear(); chosenFiles.Clear(); files.Clear(); toolDrafts.Clear(); savedJobs.Clear(); favorites.Clear();
            if(AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter) presenter.Restore();
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1480,960));
            foreach(var cycle in Enumerable.Range(0,4))
            {
                foreach(var tool in ToolCatalog.All)
                {
                    Step($"empty {cycle} {tool.Id}");
                    if(centeredGrid.IsLoaded && centeredGrid.Children.FirstOrDefault() is Grid tile && tile.Children.FirstOrDefault() is Button launch) launch.Focus(FocusState.Keyboard);
                    ShowTool(tool); await Settle();
                    ShowHome(); await Settle(); Travel(-1); await Settle(); Travel(1); await Settle();
                    for(var wait=0;wait<10 && (selected is not null || !centeredGrid.IsLoaded);wait++)await Settle();
                    Check(selected is null && centeredGrid.IsLoaded,$"empty navigation {cycle} {tool.Id} (selected={selected?.Id ?? "none"}, homeLoaded={centeredGrid.IsLoaded})");
                }
            }
            foreach(var cycle in Enumerable.Range(0,4))
            {
                foreach(var tool in ToolCatalog.All.Where(t=>t.Available && t.Id is not ("edit" or "images" or "word-pdf")))
                {
                    Step($"loaded {cycle} {tool.Id}");
                    homeFiles.Clear(); homeFiles.Add(fixture); chosenFiles.Clear(); chosenFiles.Add(fixture);
                    ShowTool(tool); await Settle(20);
                    if(tool.Id=="merge")
                    {
                        var extra=await Task.Run(async()=>await StorageFile.GetFileFromPathAsync(second));
                        await AcceptMergeFilesAsync([extra]);
                        Check(files.Count==files.Distinct(StringComparer.OrdinalIgnoreCase).Count(),"merge input admission deduplicates retained draft");
                    }
                    ShowHome(); await Settle(20); ShowHistory(); await Settle(20);
                    ShowTool(tool); await Settle(40); Travel(-1); await Settle(20); Travel(1); await Settle(20);
                    Check(selected?.Id==tool.Id,$"loaded quick navigation {cycle} {tool.Id}");
                }
                ShowSettings();await Settle();ShowHelp();await Settle();ShowAssistant();await Settle();ShowAdmin();await Settle();
                Check(true,$"support routes {cycle}");
            }
            Step("native operations");
            var outputs=Path.Combine(output,"outputs");Directory.CreateDirectory(outputs);folder=outputs;
            var imageInput=Path.Combine(output,"image.png");File.Copy(Path.Combine(AppContext.BaseDirectory,"Assets/oto.png"),imageInput,true);
            var encrypted=Path.Combine(output,"encrypted.pdf");
            var protection=await WorkerClient.RunAsync(new("protect",[fixture],encrypted,NewPassword:"synthetic-stability"),CancellationToken.None);
            Check(protection.Success,"encrypted synthetic fixture validated");
            foreach(var tool in ToolCatalog.All.Where(t=>t.Available && t.Id is not ("edit" or "word-pdf" or "split" or "organize")))
            {
                Step("operation "+tool.Id);homeFiles.Clear();chosenFiles.Clear();toolDrafts.Remove(tool.Id);
                openingEmptyInput=true;try{ShowTool(tool);}finally{openingEmptyInput=false;}
                files.Clear();files.Add(tool.Id=="images"?imageInput:tool.Id=="unlock"?encrypted:fixture);
                if(tool.Id=="merge")files.Add(second);
                await Settle(300);
                pages.Text="1";watermark.Text="SYNTHETIC STABILITY";outputName.Text="native-"+tool.Id+(tool.Id=="pdf-jpg"?".jpg":".pdf");
                if(tool.Id=="remove")removePages.Text="1";
                if(tool.Id=="extract")extractPages.Text="1";
                password.Password=tool.Id=="unlock"?"synthetic-stability":"";newPassword.Password="synthetic-stability";
                resultPath=null;await ExecuteAsync();await Settle(100);
                var target=resultPath;
                Check(target is not null && File.Exists(target),"native validated operation output "+tool.Id);
                if(target is not null && File.Exists(target))
                {
                    var readable=target;
                    if(tool.Id=="protect")
                    {
                        readable=Path.Combine(output,"verified-unlocked.pdf");
                        var verified=await WorkerClient.RunAsync(new("unlock",[target],readable,Password:"synthetic-stability"),CancellationToken.None);
                        Check(verified.Success,"protected output unlocks with authorized password");
                    }
                    var storage=await Task.Run(async()=>await StorageFile.GetFileFromPathAsync(readable));
                    if(tool.Id=="pdf-jpg")
                    {using var stream=await storage.OpenReadAsync();var decoder=await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);Check(decoder.PixelWidth>0 && decoder.PixelHeight>0,"JPG output decodes");}
                    else
                    {var pdf=await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(storage);Check(pdf.PageCount>0,"output PDF reopens "+tool.Id);}
                    ShowHistory();await Settle();ShowCurrentResult([target],"Synthetic operation validation");await Settle();ShowHome();await Settle();
                    Check(centeredGrid.IsLoaded,"result/history return "+tool.Id);
                }
            }
            Step("split native operation");await OpenSplitFileAsync(fixture);await Settle(300);await RefreshSplitSourceAsync();
            for(var wait=0;wait<50 && splitCount!=3;wait++)await Settle(100);
            Check(splitCount==3,"split source ready with three pages");
            folder=outputs;splitMode=SplitMode.AllPages;UpdateSplitPlan();resultPath=null;await ExecuteSplitAsync();await Settle();
            Check(resultPath is not null && File.Exists(resultPath),"native split output");
            if(resultPath is not null){var saved=await Task.Run(async()=>await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(resultPath)));Check(saved.PageCount==1,"split output reopens with one page");}
            Step("organize native operation");await OpenOrganizerFilesAsync([fixture]);folder=outputs;outputName.Text="native-organize.pdf";resultPath=null;await ExecuteOrganizerAsync();await Settle();
            Check(resultPath is not null && File.Exists(resultPath),"native organize output");
            if(resultPath is not null){var saved=await Task.Run(async()=>await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(resultPath)));Check(saved.PageCount==3,"organize output reopens with three pages");}
            Step("reader"); await OpenPdfReaderAsync(fixture); await pdfReader!.Rendering;
            for(var n=0;n<30;n++) {ShowHome();await Settle(20);ShowPdfReader();await Settle(20);ShowHistory();await Settle(20);ShowPdfReader();await Settle(20);}
            Check(pdfReader.ReaderSource==fixture,"reader retained after 120 transitions");
            Step("editor"); await OpenEditorDocumentAsync(fixture); await Settle(300);
            for(var n=0;n<20;n++){ShowHome();await Settle(20);ShowLiveEditor();await Settle(20);}
            Check(liveEditor?.ReaderSource==fixture,"editor retained after 40 transitions");
            await Settle(2000);
            Check(SHA256.HashData(File.ReadAllBytes(fixture)).SequenceEqual(hash),"source hash unchanged");
            ShowHome();await Settle();
        }
        catch(Exception e) { report.Add("FAIL "+e.GetType().Name+" HResult "+e.HResult.ToString("X8",System.Globalization.CultureInfo.InvariantCulture)+" "+e.StackTrace);Environment.ExitCode=1; }
        finally {File.WriteAllLines(Path.Combine(output,"audit.txt"),report);}
    }
}
#endif
