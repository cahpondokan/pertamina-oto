using System.Text.Json;
using Pertamina.Oto.LocalPdf;
internal static class Program
{
 [STAThread] private static int Main(string[] args)
 {
  var deadline=new Thread(()=>{Thread.Sleep(120000);Environment.Exit(124);}){IsBackground=true};deadline.Start();
  try
  {
   if(args is ["--self-test"])return WordTests.Run();
   var line=Console.ReadLine();if(line is null||line.Length>65536)throw new WordInputException("Permintaan tidak valid.");
   var request=JsonSerializer.Deserialize<PdfRequest>(line)??throw new WordInputException("Permintaan tidak valid.");
   Console.WriteLine(JsonSerializer.Serialize(OwnedWord.Export(request)));return 0;
  }
  catch(WordInputException error){Console.WriteLine(JsonSerializer.Serialize(new PdfReply(false,error.Message)));return 2;}
  catch(Exception){Console.WriteLine(JsonSerializer.Serialize(new PdfReply(false,"Konversi Word gagal. Periksa DOCX, aktivasi Word, dan ruang disk. Sumber tidak diubah.")));return 3;}
 }
}

