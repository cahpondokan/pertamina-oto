#if DEBUG
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pertamina.Oto.LocalPdf;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
namespace Pertamina.Oto.Desktop.App;
internal sealed partial class MainWindow
{
 internal static void PrepareWordAuditStorage(){libraryTestPath=Path.Combine(AppContext.BaseDirectory,"word-audit-library.json");}
 internal async Task AuditWordAsync(string fixtures)
 {
  var directory=Path.Combine(AppContext.BaseDirectory,"word-audit-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"word-audit-path.txt"),directory);var report=new List<string>();
  void Check(bool pass,string name){report.Add((pass?"PASS ":"FAIL ")+name);if(!pass)throw new InvalidOperationException(name);}
  async Task Capture(string name,UIElement? element=null){root.UpdateLayout();await Task.Delay(250);var bitmap=new RenderTargetBitmap();await bitmap.RenderAsync(element??root);var pixels=await bitmap.GetPixelsAsync();var file=await(await StorageFolder.GetFolderFromPathAsync(directory)).CreateFileAsync(name+".png",CreationCollisionOption.FailIfExists);using var stream=await file.OpenAsync(FileAccessMode.ReadWrite);var encoder=await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId,stream);encoder.SetPixelData(BitmapPixelFormat.Bgra8,BitmapAlphaMode.Premultiplied,(uint)bitmap.PixelWidth,(uint)bitmap.PixelHeight,96,96,pixels.ToArray());await encoder.FlushAsync();}
  static IEnumerable<DependencyObject> Walk(DependencyObject parent){yield return parent;for(var i=0;i<VisualTreeHelper.GetChildrenCount(parent);i++)foreach(var child in Walk(VisualTreeHelper.GetChild(parent,i)))yield return child;}
  var source=Path.GetFullPath(Path.Combine(fixtures,"indonesia-a4.docx"));var sourceHash=SHA256.HashData(File.ReadAllBytes(source));var initialWord=System.Diagnostics.Process.GetProcessesByName("WINWORD").Select(p=>p.Id).ToHashSet();
  try
  {
   AppWindow.Resize(new Windows.Graphics.SizeInt32(1450,1100));homeFiles.Clear();chosenFiles.Clear();toolDrafts.Clear();
   var tool=ToolCatalog.All.Single(t=>t.Id=="word-pdf");Check(tool.Available,"Word catalog has implemented capability");
   ShowTool(tool);await Task.Delay(500);Check(InputFormat(tool).Contains("DOCX",StringComparison.Ordinal),"empty intake names supported DOCX");await Capture("empty-intake");
   await AcceptEmptyInputsAsync([await StorageFile.GetFileFromPathAsync(source)]);await Task.Delay(500);Check(selected?.Id=="word-pdf"&&files.Count==1,"empty intake accepts one DOCX into workspace");Check(!toolOptions.Children.Contains(pages)&&!toolOptions.Children.Contains(password),"no PDF range/password controls on DOCX");
   Check(run.IsEnabled&&run.Content?.ToString()=="Konversi ke PDF","provider ready enables real conversion action");
   Check(Walk(root).OfType<Button>().Any(b=>AutomationProperties.GetName(b)=="Pilih DOCX"),"picker accessible name");Check(Walk(root).OfType<Button>().Any(b=>AutomationProperties.GetName(b)=="Tempel file"),"paste accessible name");Check(Walk(root).OfType<Border>().Any(b=>b.AllowDrop&&AutomationProperties.GetName(b).Contains("DOCX",StringComparison.Ordinal)),"DOCX drop target accessible");
   Check(!toolOptions.Children.OfType<StackPanel>().Any(p=>p.Children.OfType<Microsoft.UI.Xaml.Controls.Primitives.ToggleButton>().Any()),"no false raster quality settings");
   await Capture("docx-workspace");
   var old=files[0];await AcceptWordInputsAsync([await StorageFile.GetFileFromPathAsync(source),await StorageFile.GetFileFromPathAsync(source)]);Check(files.Count==1&&files[0]==old,"multi-input rejection retains source");
   var originalFolder=folder;folder=directory;outputName.Text="hasil-word-pdf.pdf";UpdateAutomaticOutputName();Check(outputName.Text.StartsWith("indonesia-a4_",StringComparison.Ordinal),"source-derived automatic output name");
   var final=Path.Combine(directory,outputName.Text);await ExecuteAsync();await Task.Delay(350);Check(File.Exists(final)&&latestResultCard?.IsLoaded==true,"real UI conversion publishes validated PDF and result panel");Check(savedJobs.Any(j=>j.Paths.Contains(final)),"result added to isolated history metadata");await Capture("result-panel");
   var search=await WorkerClient.RunAsync(new("reader-search",[final],Path.Combine(directory,"search-unused.pdf"),Text:"Indonesia"),CancellationToken.None);Check(search.Success&&search.Search?.Hits.Length>0,"Word export retains searchable vector text");
   var headerSearch=await WorkerClient.RunAsync(new("reader-search",[final],Path.Combine(directory,"header-unused.pdf"),Text:"Header sintetis"),CancellationToken.None);
   var footerSearch=await WorkerClient.RunAsync(new("reader-search",[final],Path.Combine(directory,"footer-unused.pdf"),Text:"Halaman 2"),CancellationToken.None);
   var pdf=await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(final));Check(pdf.PageCount==2,"independent renderer reopens two pages");
   for(uint i=0;i<pdf.PageCount;i++){using var page=pdf.GetPage(i);Check(Math.Abs(page.Size.Width-595.3*4/3)<2&&Math.Abs(page.Size.Height-841.9*4/3)<2,"A4 page geometry "+(i+1));var file=await(await StorageFolder.GetFolderFromPathAsync(directory)).CreateFileAsync("page-"+(i+1)+".png",CreationCollisionOption.FailIfExists);using var stream=await file.OpenAsync(FileAccessMode.ReadWrite);await page.RenderToStreamAsync(stream,new Windows.Data.Pdf.PdfPageRenderOptions{DestinationWidth=1200});Check(stream.Size>1000,"independent full-page render "+(i+1));}
      async Task<byte[]> Crop(string path,bool header)
   {
    using var image=await(await StorageFile.GetFileFromPathAsync(path)).OpenReadAsync();var decoder=await BitmapDecoder.CreateAsync(image);var bounds=new BitmapBounds{X=0,Y=header?0:decoder.PixelHeight-150,Width=decoder.PixelWidth,Height=header?120u:150u};var pixels=await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8,BitmapAlphaMode.Straight,new BitmapTransform{Bounds=bounds},ExifOrientationMode.IgnoreExifOrientation,ColorManagementMode.DoNotColorManage);return pixels.DetachPixelData();
   }
   var firstHeader=await Crop(Path.Combine(directory,"page-1.png"),true);var secondHeader=await Crop(Path.Combine(directory,"page-2.png"),true);
   var ink1=firstHeader.Where((b,i)=>i%4!=3&&b<180).Count();var ink2=secondHeader.Where((b,i)=>i%4!=3&&b<180).Count();var difference=firstHeader.Zip(secondHeader,(a,b)=>Math.Abs(a-b)).Sum(v=>(long)v);Check(ink1>100&&ink2>100&&difference<(long)firstHeader.Length*3,"independent matching nonempty header raster on both pages");
   var firstFooter=await Crop(Path.Combine(directory,"page-1.png"),false);var secondFooter=await Crop(Path.Combine(directory,"page-2.png"),false);Check(!firstFooter.SequenceEqual(secondFooter),"page-number footer changes between independently rendered pages");
   report.Add("INFO Header/footer extraction can be partial in existing search engine; visual page images are the fidelity oracle.");
   var resultHash=SHA256.HashData(File.ReadAllBytes(final));Check(!(await WordPdfClient.RunAsync(new("word-pdf",[source],final),CancellationToken.None)).Success,"existing output rejected");Check(resultHash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(final))),"existing output unchanged");
   Check(AvailableOutputPath(directory,Path.GetFileName(final))!=final,"conflicting name chooses unique sibling");
   Check(!(await WordPdfClient.RunAsync(new("word-pdf",[source],source),CancellationToken.None)).Success,"source alias output rejected");
   using(var locked=new FileStream(source,FileMode.Open,FileAccess.Read,FileShare.None))Check(!(await WordPdfClient.RunAsync(new("word-pdf",[source],Path.Combine(directory,"locked.pdf")),CancellationToken.None)).Success,"locked source fails safely");
   var bad=Path.Combine(directory,"malformed.docx");await File.WriteAllTextAsync(bad,"broken synthetic");Check(!(await WordPdfClient.RunAsync(new("word-pdf",[bad],Path.Combine(directory,"bad.pdf")),CancellationToken.None)).Success&&!File.Exists(Path.Combine(directory,"bad.pdf")),"malformed input no published output");
   using(var cancelBefore=new CancellationTokenSource()){cancelBefore.Cancel();Check(!(await WordPdfClient.RunAsync(new("word-pdf",[source],Path.Combine(directory,"pre-cancel.pdf")),cancelBefore.Token)).Success&&!File.Exists(Path.Combine(directory,"pre-cancel.pdf")),"pre-start cancel no result");}
   using(var cancelDuring=new CancellationTokenSource()){var observed=false;var reply=await WordPdfClient.RunAsync(new("word-pdf",[source],Path.Combine(directory,"during-cancel.pdf")),cancelDuring.Token,new Progress<string>(s=>{if(s.Contains("terbuka",StringComparison.Ordinal)){observed=true;cancelDuring.Cancel();}}));Check(observed&&!reply.Success&&!File.Exists(Path.Combine(directory,"during-cancel.pdf")),"in-flight cancellation after Word opened no result");}
   Check(!(await WordPdfClient.RunAsync(new("word-pdf",[source],Path.Combine(directory,"timeout.pdf")),CancellationToken.None,timeoutOverride:TimeSpan.FromMilliseconds(200))).Success&&!File.Exists(Path.Combine(directory,"timeout.pdf")),"bounded timeout kills owned helper/process no result");
   var workerExe=Path.Combine(AppContext.BaseDirectory,"word-worker","Pertamina.Oto.WordPdf.Worker.exe");var parked=workerExe+".test-parked";
   File.Move(workerExe,parked,false);try{Check(!(await WordPdfClient.RunAsync(new("word-pdf",[source],Path.Combine(directory,"missing-worker.pdf")),CancellationToken.None)).Success,"missing worker safe failure");}finally{File.Move(parked,workerExe,false);}
   Check(sourceHash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(source))),"all paths preserve source hash");
   await Task.Delay(500);Check(System.Diagnostics.Process.GetProcessesByName("WINWORD").All(p=>initialWord.Contains(p.Id)),"no leaked task Word PID after cancel/timeout");
   Check(!Directory.EnumerateFiles(directory,".oto-word-*").Any()&&!Directory.EnumerateDirectories(directory,".oto-word-*").Any(),"no staging source or partial PDF left in output folder");
   // Exercise shared internal reader using the result, without external viewer.
   var previewLoaded=false;auditReadDialog=async reader=>{Check(reader.Visible&&reader.ReadingDocument?.PageCount==2,"shared preview loaded actual two-page Word PDF");await reader.NavigateAsync(1);Check(reader.ReadingPage==1,"result preview navigates to second page");await Capture("internal-result-preview",reader.View);await reader.NavigateAsync(0);Check(reader.ReadingPage==0,"result preview returns to first page");Check(reader.ReadingViewport.ZoomMode==ZoomMode.Enabled,"result preview retains native zoom");previewLoaded=true;};try{await ShowReadPreviewDialogAsync(final);Check(previewLoaded,"internal result read-preview renders actual saved PDF");}finally{auditReadDialog=null;}
   folder=originalFolder;Environment.ExitCode=0;
  }
  catch(Exception e){report.Add("ERROR "+e.GetType().Name+" "+e.HResult);report.AddRange(new System.Diagnostics.StackTrace(e).GetFrames().Select(f=>f.GetMethod()?.DeclaringType?.Name+"."+f.GetMethod()?.Name));Environment.ExitCode=1;}
  finally{File.WriteAllLines(Path.Combine(directory,"audit.txt"),report);}
 }
}
#endif











