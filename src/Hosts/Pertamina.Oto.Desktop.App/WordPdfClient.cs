using System.Diagnostics;
using System.Text.Json;
using Pertamina.Oto.LocalPdf;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
namespace Pertamina.Oto.Desktop.App;
internal static class WordPdfClient
{
 internal static async Task<PdfReply> RunAsync(PdfRequest request,CancellationToken cancellation,IProgress<string>? progress=null,TimeSpan? timeoutOverride=null)
 {
  var final=Path.GetFullPath(request.Output);var stage=Path.Combine(Path.GetDirectoryName(final)!,".oto-word-"+Guid.NewGuid().ToString("N")+".pdf");
  var reply=await RunCoreAsync(request,final,stage,progress,timeoutOverride,cancellation);
  var cleaned=await CleanupAsync(stage);
  return cleaned?reply:reply with{Message=reply.Message+" Pembersihan salinan sementara belum selesai; periksa folder hasil sebelum menutup aplikasi."};
 }
 private static async Task<PdfReply> RunCoreAsync(PdfRequest request,string final,string stage,IProgress<string>? progress,TimeSpan? timeoutOverride,CancellationToken cancellation)
 {
  using var timeout=CancellationTokenSource.CreateLinkedTokenSource(cancellation);timeout.CancelAfter(timeoutOverride??TimeSpan.FromMinutes(2));var token=timeout.Token;
  using var process=new Process{StartInfo=new ProcessStartInfo{FileName=Path.Combine(AppContext.BaseDirectory,"word-worker","Pertamina.Oto.WordPdf.Worker.exe"),UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardOutput=true,RedirectStandardError=true,WorkingDirectory=AppContext.BaseDirectory}};
  FileStream? source=null;
  try
  {
   token.ThrowIfCancellationRequested();
   if(request.Inputs.Length!=1||!Path.GetExtension(request.Inputs[0]).Equals(".docx",StringComparison.OrdinalIgnoreCase))return new(false,"Pilih satu DOCX. DOC/DOCM belum didukung.");
   if(File.Exists(final)||Directory.Exists(final)||string.Equals(Path.GetFullPath(request.Inputs[0]),final,StringComparison.OrdinalIgnoreCase))return new(false,"Hasil tidak boleh menimpa file sumber atau file existing.");
   if(!IsLocalPlainPath(request.Inputs[0])||!IsLocalPlainPath(Path.GetDirectoryName(final)!))return new(false,"Pilih sumber dan folder hasil lokal tanpa tautan/reparse atau lokasi jaringan.");
   source=new FileStream(request.Inputs[0],FileMode.Open,FileAccess.Read,FileShare.Read);
   if(source.Length is <=0 or >100L*1024*1024)return new(false,"DOCX dibatasi 100 MB.");
   progress?.Report("Memeriksa DOCX dan provider Word lokal…");
   if(!process.Start())return new(false,"Worker Word tidak dapat dimulai.");
   using var kill=token.Register(()=>Stop(process));
   await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(request with{Output=stage}).AsMemory(),token);process.StandardInput.Close();
   var response=ReadBounded(process.StandardOutput,token);var stages=ReadStages(process.StandardError,token,progress);
   while(!process.HasExited){if(process.WorkingSet64>256L*1024*1024){Stop(process);return new(false,"Worker Word melewati batas memori; hasil tidak diterbitkan.");}await Task.Delay(100,token);process.Refresh();}
   await stages;var reply=JsonSerializer.Deserialize<PdfReply>(await response);
   if(process.ExitCode!=0||reply is not{Success:true})return reply is{Success:false}?reply:new(false,"Word berhenti atau melewati batas waktu. Sumber tidak diubah.");
   var info=new FileInfo(stage);if(!info.Exists||info.Length<=0||info.Length!=reply.Bytes||info.Length>250L*1024*1024)return new(false,"Ukuran PDF hasil tidak valid; tidak diterbitkan.");
   progress?.Report("Memvalidasi dan membuka ulang PDF hasil…");
   var pdf=await PdfDocument.LoadFromFileAsync(await StorageFile.GetFileFromPathAsync(stage)).AsTask(token);
   if(pdf.PageCount is <1 or >1000 || pdf.PageCount!=reply.Pages)return new(false,"Jumlah halaman hasil tidak valid.");
   for(uint index=0;index<pdf.PageCount;index++)
   {
    token.ThrowIfCancellationRequested();using var page=pdf.GetPage(index);using var rendered=new InMemoryRandomAccessStream();
    await page.RenderToStreamAsync(rendered,new PdfPageRenderOptions{DestinationWidth=32}).AsTask(token);
    if(rendered.Size==0)return new(false,"Halaman hasil tidak dapat dirender.");
   }
   token.ThrowIfCancellationRequested();File.Move(stage,final,false);
   return reply with{Message="PDF selesai dan berhasil dibuka ulang. Periksa font dan tata letak sebelum digunakan."};
  }
  catch(OperationCanceledException){Stop(process);return new(false,cancellation.IsCancellationRequested?"Konversi dibatalkan. Sumber tidak diubah.":"Word melewati batas 2 menit; proses milik task dihentikan. Sumber tidak diubah.");}
  catch(Exception){Stop(process);return new(false,"Konversi Word gagal. Periksa DOCX, Word terpasang/berlisensi, folder, dan ruang disk. Sumber tidak diubah.");}
  finally
  {
   Stop(process);source?.Dispose();
  }
 }
 private static async Task<bool> CleanupAsync(string stage)
 {
  var temp=stage+".word";
  // Owned Word termination is asynchronous after the helper/job handle exits. Retry only this unique job's files.
  for(var attempt=0;attempt<30;attempt++)
  {
   try
   {
    if(File.Exists(stage))File.Delete(stage);
    if(Directory.Exists(temp))
    {
     if((File.GetAttributes(temp)&System.IO.FileAttributes.ReparsePoint)!=0)return false;
     foreach(var file in Directory.EnumerateFiles(temp))
     {if((File.GetAttributes(file)&System.IO.FileAttributes.ReparsePoint)!=0)return false;File.Delete(file);}
     Directory.Delete(temp,false);
    }
    return true;
   }
   catch(IOException){}catch(UnauthorizedAccessException){}
   await Task.Delay(100);
  }
  return false;
 }
 internal static bool IsLocalPlainPath(string path)
 {
  var full=Path.GetFullPath(path);if(full.StartsWith(@"\\",StringComparison.Ordinal)||new DriveInfo(Path.GetPathRoot(full)!).DriveType==DriveType.Network)return false;
  for(var current=full;!string.IsNullOrEmpty(current);current=Path.GetDirectoryName(current))if((File.GetAttributes(current)&System.IO.FileAttributes.ReparsePoint)!=0)return false;
  return true;
 }
 private static void Stop(Process p){try{if(!p.HasExited){p.Kill(true);p.WaitForExit(3000);}}catch(InvalidOperationException){}catch(System.ComponentModel.Win32Exception){}}
 private static async Task<string> ReadBounded(StreamReader reader,CancellationToken token){var buffer=new char[65536];var length=0;while(length<buffer.Length){var n=await reader.ReadAsync(buffer.AsMemory(length),token);if(n==0)return new string(buffer,0,length);length+=n;}throw new InvalidDataException();}
 private static async Task ReadStages(StreamReader reader,CancellationToken token,IProgress<string>? progress)
 {
  var buffer=new char[1024];var pending="";
  while(true){var n=await reader.ReadAsync(buffer,token);if(n==0)break;pending+=new string(buffer,0,n);if(pending.Length>4096)throw new InvalidDataException();int end;while((end=pending.IndexOf('\n'))>=0){var stage=pending[..end].Trim();pending=pending[(end+1)..];var label=stage switch{"WORD_STAGE_OWNED"=>"Menyiapkan Word terisolasi…","WORD_STAGE_OPEN"=>"Dokumen terbuka secara read-only…","WORD_STAGE_EXPORT"=>"Mengekspor PDF…","WORD_STAGE_EXPORTED"=>"Memeriksa hasil…",_=>null};if(label is not null)progress?.Report(label);}}
 }
}



