using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace Pertamina.Oto.LocalPdf;
internal static class WordTests
{
 internal static int Run()
 {
  var root=Path.Combine(Path.GetTempPath(),"oto-word-tests-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);var passed=0;var originalWord=Process.GetProcessesByName("WINWORD").Select(p=>p.Id).ToHashSet();
  using var log=new StreamWriter(Path.Combine(root,"checks.txt")){AutoFlush=true};
  void Check(bool value,string name){log.WriteLine((value?"PASS ":"FAIL ")+name);Console.WriteLine((value?"PASS ":"FAIL ")+name);if(!value)throw new InvalidOperationException();passed++;}
  bool Reject(string p){try{using var f=File.OpenRead(p);SafeDocx.Validate(f);return false;}catch(Exception e) when(e is WordInputException or InvalidDataException or System.Xml.XmlException){return true;}}
  try
  {
   var source=Path.Combine(root,"indonesia-a3.docx");Fixture(source,false);using(var f=File.OpenRead(source))SafeDocx.Validate(f);Check(true,"bounded DOCX admits synthetic Indonesia table image header footer PAGE A4");
   Check(WordProvider.Find() is not null,"exact admitted Word hash available");
   Check(!WordProvider.MatchesCandidate(source)&&!WordProvider.MatchesCandidate(Path.Combine(root,"missing.exe")),"missing/unadmitted provider candidates fail closed");
   var hash=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(source)));var output=Path.Combine(root,"indonesia-a3.pdf");
   var reply=OwnedWord.Export(new("word-pdf",[source],output));Check(reply.Success && reply.Pages==2 && reply.Bytes>1000,"real Word export A4 two pages");
   Check(hash==Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(source))),"source unchanged");
   using(var pdf=File.OpenRead(output)){var head=new byte[5];pdf.ReadExactly(head);Check(System.Text.Encoding.ASCII.GetString(head)=="%PDF-","PDF signature");}
   var a3=Path.Combine(root,"unsupported-a3.docx");Fixture(a3,true);Check(Reject(a3),"A3 rejected with explicit limitation before Word");
   var a4=Path.Combine(root,"indonesia-a4.docx");Fixture(a4,false);Check(OwnedWord.Export(new("word-pdf",[a4],Path.Combine(root,"indonesia-a4.pdf"))).Pages==2,"real Word export A4 two pages");
   foreach(var paper in new[]{("landscape",16838,11906),("letter",12240,15840)})
   {
    var input=Path.Combine(root,paper.Item1+".docx");Fixture(input,false);using(var z=ZipFile.Open(input,ZipArchiveMode.Update)){var e=z.GetEntry("word/document.xml")!;string xml;using(var r=new StreamReader(e.Open()))xml=r.ReadToEnd();e.Delete();using var w=new StreamWriter(z.CreateEntry("word/document.xml").Open());w.Write(xml.Replace("w:w=\"11906\" w:h=\"16838\"","w:w=\""+paper.Item2+"\" w:h=\""+paper.Item3+"\""+(paper.Item1=="landscape"?" w:orient=\"landscape\"":"")));}
    Check(Reject(input),paper.Item1+" rejected before Word because provider not verified");
   }
   var bad=Path.Combine(root,"broken.docx");File.WriteAllText(bad,"broken");Check(Reject(bad),"malformed ZIP rejected");
   foreach(var kind in new[]{"remote","macro","ole","field","dtd","traversal","duplicate","encrypted","altchunk","disguised-svg","active-mime","oversized-image"})
   {
    var path=Path.Combine(root,kind+".docx");SafeDocx.CreateSeed(path);
    using(var z=ZipFile.Open(path,ZipArchiveMode.Update))
    {
     void Add(string n,string text){using var w=new StreamWriter(z.CreateEntry(n).Open());w.Write(text);}
     if(kind=="remote")Add("word/_rels/document.xml.rels","<Relationships><Relationship Target=\"https://example.invalid/synthetic\" TargetMode=\"External\" Type=\"image\"/></Relationships>");
     if(kind=="macro")Add("word/vbaProject.bin","synthetic");
     if(kind=="disguised-svg")Add("word/media/hidden.png","<svg xmlns=\"http://www.w3.org/2000/svg\"><image href=\"https://example.invalid/synthetic\"/></svg>");
     if(kind=="active-mime"){z.GetEntry("[Content_Types].xml")!.Delete();Add("[Content_Types].xml","<Types><Default Extension=\"png\" ContentType=\"image/svg+xml\"/><Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/></Types>");}
     if(kind=="oversized-image"){var bytes=CreatePng();System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(16),100000);using var img=z.CreateEntry("word/media/bomb.png").Open();img.Write(bytes);}
     if(kind=="ole")Add("word/embeddings/object.bin","synthetic");
     if(kind=="traversal")Add("../escape.xml","<x/>");
     if(kind=="duplicate")Add("word/document.xml","<x/>");
     if(kind is "field" or "dtd" or "encrypted" or "altchunk")
     {z.GetEntry("word/document.xml")!.Delete();Add("word/document.xml",kind switch{"field"=>"<x><instrText>INCLUDETEXT synthetic</instrText></x>","dtd"=>"<!DOCTYPE x [<!ENTITY a 'synthetic'>]><x>&a;</x>","encrypted"=>"<x><docProtection/></x>",_=>"<x><altChunk/></x>"});}
    }
    Check(Reject(path),kind+" rejected before provider execution");
   }
   var seed=Path.Combine(root,"foreign-seed.docx");SafeDocx.CreateSeed(seed);var start=new ProcessStartInfo{FileName=WordProvider.Find()!,UseShellExecute=false,WindowStyle=ProcessWindowStyle.Hidden};start.ArgumentList.Add("/a");start.ArgumentList.Add(seed);
   using(var foreign=Process.Start(start)!)
   using(var foreignJob=new WordProcessJob(foreign))
   {
    Check(!originalWord.Contains(foreign.Id),"foreign sentinel belongs to test, not user");
    var native=OwnedWord.GetNativeWindow(foreign);dynamic window=native;dynamic app=window.Application;app.AutomationSecurity=3;app.Visible=false;dynamic document=window.Document;document.Content.Text="Synthetic unsaved sentinel";var count=(int)app.Documents.Count;var saved=(bool)document.Saved;
    Check(OwnedWord.Export(new("word-pdf",[a4],Path.Combine(root,"foreign-present.pdf"))).Success,"conversion while another Word instance open");
    foreign.Refresh();Check(!foreign.HasExited && (int)app.Documents.Count==count && (bool)document.Saved==saved && ((string)document.Content.Text).Trim()=="Synthetic unsaved sentinel","foreign Word document and unsaved state untouched");
    app.Quit(SaveChanges:0);foreign.WaitForExit(3000);Marshal.FinalReleaseComObject(document);Marshal.FinalReleaseComObject(app);Marshal.FinalReleaseComObject(native);
   }
   Thread.Sleep(300);Check(Process.GetProcessesByName("WINWORD").All(p=>originalWord.Contains(p.Id)),"no task Word process remains; original PIDs retained");
   log.WriteLine("TOTAL "+passed);Console.WriteLine("EVIDENCE "+root);return 0;
  }
  catch(Exception e){log.WriteLine("FAILED "+e.GetType().Name);Console.WriteLine("FAILED "+e.GetType().Name);Console.WriteLine("EVIDENCE "+root);return 1;}
 }
 private static byte[] CreatePng()
 {
  using var output=new MemoryStream();output.Write(new byte[]{137,80,78,71,13,10,26,10});
  void Chunk(string name,byte[] bytes){var length=new byte[4];System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(length,bytes.Length);output.Write(length);var type=System.Text.Encoding.ASCII.GetBytes(name);output.Write(type);output.Write(bytes);uint crc=0xffffffff;foreach(var b in type.Concat(bytes)){crc^=b;for(var i=0;i<8;i++)crc=(crc&1)!=0?0xedb88320^(crc>>1):crc>>1;}System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(length,crc^0xffffffff);output.Write(length);}
  var header=new byte[13];System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(header,32);System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4),32);header[8]=8;header[9]=2;Chunk("IHDR",header);
  using var compressed=new MemoryStream();using(var z=new ZLibStream(compressed,CompressionLevel.Optimal,true))for(var y=0;y<32;y++){z.WriteByte(0);for(var x=0;x<32;x++)z.Write(new byte[]{20,150,70});}Chunk("IDAT",compressed.ToArray());Chunk("IEND",[]);return output.ToArray();
 }
 private static bool ProcessExists()=>System.Diagnostics.Process.GetProcessesByName("WINWORD").Length!=0;
 internal static void Fixture(string path,bool a3)
 {
  SafeDocx.CreateSeed(path);using var z=ZipFile.Open(path,ZipArchiveMode.Update);z.GetEntry("word/document.xml")!.Delete();
  void Add(string n,string s){using var w=new StreamWriter(z.CreateEntry(n).Open());w.Write(s);}
  var ns="http://schemas.openxmlformats.org/wordprocessingml/2006/main";
  var image="<w:p><w:r><w:drawing><wp:inline xmlns:wp=\"http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing\"><wp:extent cx=\"914400\" cy=\"914400\"/><wp:docPr id=\"1\" name=\"Synthetic square\"/><a:graphic xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\"><a:graphicData uri=\"http://schemas.openxmlformats.org/drawingml/2006/picture\"><pic:pic xmlns:pic=\"http://schemas.openxmlformats.org/drawingml/2006/picture\"><pic:nvPicPr><pic:cNvPr id=\"0\" name=\"synthetic.png\"/><pic:cNvPicPr/></pic:nvPicPr><pic:blipFill><a:blip r:embed=\"image\"/><a:stretch><a:fillRect/></a:stretch></pic:blipFill><pic:spPr><a:xfrm><a:off x=\"0\" y=\"0\"/><a:ext cx=\"914400\" cy=\"914400\"/></a:xfrm><a:prstGeom prst=\"rect\"><a:avLst/></a:prstGeom></pic:spPr></pic:pic></a:graphicData></a:graphic></wp:inline></w:drawing></w:r></w:p>";
  var section="<w:sectPr><w:headerReference w:type=\"default\" r:id=\"header\"/><w:footerReference w:type=\"default\" r:id=\"footer\"/><w:pgSz w:w=\""+(a3?"23811":"11906")+"\" w:h=\"16838\""+(a3?" w:orient=\"landscape\"":"")+"/><w:pgMar w:top=\"1440\" w:right=\"1440\" w:bottom=\"1440\" w:left=\"1440\" w:header=\"720\" w:footer=\"720\"/></w:sectPr>";
  Add("word/document.xml","<w:document xmlns:w=\""+ns+"\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><w:body><w:p><w:r><w:rPr><w:rFonts w:ascii=\"Arial\" w:hAnsi=\"Arial\"/><w:sz w:val=\"32\"/></w:rPr><w:t>Uji Indonesia: laporan teknik dan keselamatan kerja.</w:t></w:r></w:p><w:tbl><w:tblPr><w:tblBorders><w:top w:val=\"single\"/><w:bottom w:val=\"single\"/><w:insideH w:val=\"single\"/><w:insideV w:val=\"single\"/></w:tblBorders></w:tblPr><w:tblGrid><w:gridCol w:w=\"4000\"/><w:gridCol w:w=\"4000\"/></w:tblGrid><w:tr><w:tc><w:p><w:r><w:t>Peralatan</w:t></w:r></w:p></w:tc><w:tc><w:p><w:r><w:t>Kondisi baik</w:t></w:r></w:p></w:tc></w:tr></w:tbl>"+image+"<w:p><w:r><w:br w:type=\"page\"/></w:r></w:p><w:p><w:r><w:t>Halaman kedua: pemeriksaan selesai.</w:t></w:r></w:p>"+section+"</w:body></w:document>");
  Add("word/settings.xml","<w:settings xmlns:w=\""+ns+"\"><w:evenAndOddHeaders w:val=\"0\"/><w:compat><w:compatSetting w:name=\"compatibilityMode\" w:uri=\"http://schemas.microsoft.com/office/word\" w:val=\"15\"/></w:compat></w:settings>");
  Add("word/header1.xml","<w:hdr xmlns:w=\""+ns+"\"><w:p><w:r><w:t>Header sintetis OTO</w:t></w:r></w:p></w:hdr>");
  Add("word/footer1.xml","<w:ftr xmlns:w=\""+ns+"\"><w:p><w:r><w:t xml:space=\"preserve\">Halaman </w:t></w:r><w:fldSimple w:instr=\"PAGE\"><w:r><w:t>1</w:t></w:r></w:fldSimple></w:p></w:ftr>");
  Add("word/_rels/document.xml.rels","<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"settings\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/settings\" Target=\"settings.xml\"/><Relationship Id=\"header\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/header\" Target=\"header1.xml\"/><Relationship Id=\"footer\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/footer\" Target=\"footer1.xml\"/><Relationship Id=\"image\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/image\" Target=\"media/synthetic.png\"/></Relationships>");
  z.GetEntry("[Content_Types].xml")!.Delete();Add("[Content_Types].xml","<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Default Extension=\"png\" ContentType=\"image/png\"/><Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/><Override PartName=\"/word/settings.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.settings+xml\"/><Override PartName=\"/word/header1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.header+xml\"/><Override PartName=\"/word/footer1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml\"/></Types>");
  using var png=z.CreateEntry("word/media/synthetic.png").Open();png.Write(CreatePng());
 }
}











