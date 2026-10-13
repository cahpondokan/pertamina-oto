using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
namespace Pertamina.Oto.LocalPdf;
internal sealed class WordProcessJob : IDisposable
{
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern nint CreateJobObject(nint attributes,string? name);
 [DllImport("kernel32.dll",SetLastError=true)] static extern bool SetInformationJobObject(nint job,int type,ref Limits info,uint length);
 [DllImport("kernel32.dll",SetLastError=true)] static extern bool AssignProcessToJobObject(nint job,nint process);
 [DllImport("kernel32.dll")] static extern bool CloseHandle(nint handle);
 [StructLayout(LayoutKind.Sequential)] struct Basic { public long ProcessTime,JobTime; public uint Flags;public nuint MinWorking,MaxWorking;public uint ActiveProcessLimit;public nuint Affinity;public uint Priority,Scheduling; }
 [StructLayout(LayoutKind.Sequential)] struct Io {public ulong ReadOps,WriteOps,OtherOps,ReadBytes,WriteBytes,OtherBytes;}
 [StructLayout(LayoutKind.Sequential)] struct Limits {public Basic Basic;public Io Io;public nuint ProcessMemory,JobMemory,PeakProcessMemory,PeakJobMemory;}
 private nint handle;
 internal WordProcessJob(Process process)
 {
  handle=CreateJobObject(0,null);
  var info=new Limits{Basic=new Basic{Flags=0x2000|0x200|0x8,ActiveProcessLimit=4},JobMemory=(nuint)(1024L*1024*1024)};
  if(handle==0 || !SetInformationJobObject(handle,9,ref info,(uint)Marshal.SizeOf<Limits>()) || !AssignProcessToJobObject(handle,process.Handle))
  {Dispose();throw new WordInputException("Isolasi proses Word tidak tersedia. Tidak ada hasil yang diterbitkan.");}
 }
 public void Dispose(){if(handle!=0){CloseHandle(handle);handle=0;}}
}
internal static class OwnedWord
{
 [DllImport("user32.dll")] static extern bool EnumWindows(Callback cb,nint arg);
 [DllImport("user32.dll")] static extern bool EnumChildWindows(nint p,Callback cb,nint arg);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetClassName(nint w,[Out] char[] n,int cap);
 [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(nint w,out uint pid);
 [DllImport("oleacc.dll")] static extern int AccessibleObjectFromWindow(nint w,uint id,ref Guid iid,[MarshalAs(UnmanagedType.Interface)] out object result);
 delegate bool Callback(nint w,nint arg);
 internal static PdfReply Export(PdfRequest request)
 {
  if(request.Tool!="word-pdf"||request.Inputs.Length!=1||request.Quality!="original")throw new WordInputException("Word ke PDF menerima satu DOCX dengan kualitas asli.");
  var path=request.Inputs[0];
  if(!Path.GetExtension(path).Equals(".docx",StringComparison.OrdinalIgnoreCase)||Path.GetFullPath(path).StartsWith(@"\\",StringComparison.Ordinal)|| (File.GetAttributes(path)&FileAttributes.ReparsePoint)!=0)throw new WordInputException("Pilih satu DOCX lokal. DOC, DOCM, tautan, dan lokasi jaringan belum didukung.");
  if(File.Exists(request.Output)||Directory.Exists(request.Output)||string.Equals(Path.GetFullPath(path),Path.GetFullPath(request.Output),StringComparison.OrdinalIgnoreCase))throw new WordInputException("Hasil harus memakai nama baru dan tidak menimpa sumber.");
  var provider=WordProvider.Find()??throw new WordInputException("Word lokal yang disetujui tidak ditemukan atau versinya berubah. Microsoft Word terpasang dan berlisensi diperlukan; aplikasi tidak menginstalnya.");
  using var source=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);SafeDocx.Validate(source);
  var temp=request.Output+".word";if(Directory.Exists(temp))throw new WordInputException("Staging sudah ada. Coba kembali dengan nama hasil baru.");Directory.CreateDirectory(temp);
  var copy=Path.Combine(temp,"input.docx");var seed=Path.Combine(temp,"seed.docx");
  Process? owned=null;WordProcessJob? job=null;object? native=null;object? application=null;object? document=null;
  try
  {
   using(var target=new FileStream(copy,FileMode.CreateNew,FileAccess.Write)){source.Position=0;source.CopyTo(target);}
   SafeDocx.CreateSeed(seed);
   var prior=Process.GetProcessesByName("WINWORD").Select(p=>p.Id).ToHashSet();
   var start=new ProcessStartInfo{FileName=provider,UseShellExecute=false,WindowStyle=ProcessWindowStyle.Hidden,WorkingDirectory=temp};
   start.ArgumentList.Add("/a");start.ArgumentList.Add(seed); // one supported switch: disables global templates/add-ins and locks setting files.
   owned=Process.Start(start)??throw new WordInputException("Word tidak dapat dimulai.");
   if(prior.Contains(owned.Id))throw new WordInputException("Kepemilikan proses Word tidak dapat dibuktikan.");
   job=new WordProcessJob(owned);
   // Termination of this helper closes the private job handle, including a stuck COM call.
   Console.Error.WriteLine("WORD_STAGE_OWNED");native=GetNativeWindow(owned);Console.Error.WriteLine("WORD_STAGE_READY");
   dynamic window=native;var windowThread=GetWindowThreadProcessId((nint)(int)window.Hwnd,out var pid);
   if(windowThread==0||pid!=owned.Id)throw new WordInputException("Instance Word tidak terisolasi. Proses dibatalkan.");
   dynamic app=window.Application;application=app;
   app.AutomationSecurity=3;app.DisplayAlerts=0;app.Visible=false;
   // /a locks setting files; only this process's options are changed.
   app.Options.UpdateLinksAtOpen=false;app.Options.UpdateFieldsAtPrint=false;app.Options.UpdateLinksAtPrint=false;
   app.Options.CheckSpellingAsYouType=false;app.Options.CheckGrammarAsYouType=false;app.Options.CheckGrammarWithSpelling=false;
   app.Options.SaveInterval=0;app.Options.BackgroundSave=false;
   app.Options.SaveNormalPrompt=false;Console.Error.WriteLine("WORD_STAGE_SETTINGS");
   dynamic doc=app.Documents.Open(FileName:copy,ConfirmConversions:false,ReadOnly:true,AddToRecentFiles:false,PasswordDocument:"",PasswordTemplate:"",Revert:false,WritePasswordDocument:"",WritePasswordTemplate:"",Visible:true,OpenAndRepair:false,NoEncodingDialog:true);
   document=doc;Console.Error.WriteLine("WORD_STAGE_OPEN");
   if(doc.HasVBProject || doc.InlineShapes.Count>1000)throw new WordInputException("Konten dokumen belum didukung.");
   
   doc.Activate();doc.Windows[1].View.Type=3;app.Visible=false;doc.Repaginate();
   var count=(int)doc.ComputeStatistics(2,false);
   if(count is <1 or >1000)throw new WordInputException("Hasil dibatasi 1–1000 halaman.");
   Console.Error.WriteLine("WORD_STAGE_EXPORT");doc.ExportAsFixedFormat(OutputFileName:request.Output,ExportFormat:17,OpenAfterExport:false,OptimizeFor:0,Range:0,Item:0,IncludeDocProps:false,KeepIRM:false,CreateBookmarks:1,DocStructureTags:true,BitmapMissingFonts:false,UseISO19005_1:false);
   Console.Error.WriteLine("WORD_STAGE_EXPORTED");doc.Close(SaveChanges:0);Marshal.FinalReleaseComObject(document);document=null;
   // Only the application obtained from the verified owned Window, never GetActiveObject.
   app.Quit(SaveChanges:0);
   var bytes=new FileInfo(request.Output).Length;
   if(bytes<=0||bytes>250L*1024*1024)throw new WordInputException("Ukuran hasil melampaui batas.");
   return new(true,"Ekspor Word selesai. Periksa font dan tata letak hasil.",count,bytes);
  }
  finally
  {
   // Closing job guarantees cleanup if COM Quit failed. Do not enumerate/kill unrelated Office processes.
   job?.Dispose();
   if(owned is not null){try{if(!owned.HasExited){owned.Kill(false);owned.WaitForExit(3000);}}catch(InvalidOperationException){}owned.Dispose();}
   foreach(var obj in new[]{document,application,native})if(obj is not null&&Marshal.IsComObject(obj))try{Marshal.FinalReleaseComObject(obj);}catch(InvalidComObjectException){}
   // Fixed owned paths only, no recursive deletes or computed user-directory cleanup.
   foreach(var p in new[]{copy,seed,Path.Combine(temp,"~$input.docx"),Path.Combine(temp,"~$seed.docx")})try{if(File.Exists(p))File.Delete(p);}catch(IOException){}catch(UnauthorizedAccessException){}
   try{Directory.Delete(temp,false);}catch(IOException){}catch(UnauthorizedAccessException){}
  }
 }
 internal static object GetNativeWindow(Process owned)
 {
  for(var i=0;i<150;i++)
  {
   owned.Refresh();if(owned.HasExited)break;nint hwnd=0;
   EnumWindows((w,_)=>{if(GetWindowThreadProcessId(w,out var pid)==0)return true;var name=new char[100];var length=GetClassName(w,name,100);if(pid==owned.Id&&length>0&&new string(name,0,length)=="OpusApp"){hwnd=w;return false;}return true;},0);
   object? native=null;
   if(hwnd!=0)EnumChildWindows(hwnd,(child,_)=>{if(GetWindowThreadProcessId(child,out var pid)==0||pid!=owned.Id)return true;var name=new char[100];var length=GetClassName(child,name,100);if(length<=0||new string(name,0,length)!="_WwG")return true;var iid=new Guid("00020400-0000-0000-C000-000000000046");if(AccessibleObjectFromWindow(child,0xfffffff0,ref iid,out var found)==0)native=found;return native is null;},0);
   if(native is not null)return native;Thread.Sleep(100);
  }
  throw new WordInputException("Word belum siap atau memerlukan aktivasi/dialog. Buka Word sendiri untuk memeriksa lisensi, lalu coba kembali.");
 }
}















