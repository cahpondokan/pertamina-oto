using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
namespace Pertamina.Oto.LocalPdf;
internal sealed class WordInputException(string message) : Exception(message);
internal static class SafeDocx
{
 internal const long MaxBytes = 100L * 1024 * 1024;
 private static void Reject() => throw new WordInputException("DOCX ini memiliki struktur atau konten yang belum didukung secara aman. Gunakan DOCX tanpa macro, link eksternal, objek tertanam, atau field aktif.");
 internal static void Validate(Stream stream)
 {
  if (!stream.CanSeek || stream.Length is <=0 or >MaxBytes) Reject();
  stream.Position=0;
  using var zip=new ZipArchive(stream,ZipArchiveMode.Read,true);
  if(zip.Entries.Count is <3 or >2048) Reject();
  long expanded=0; var names=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  foreach(var e in zip.Entries)
  {
   if(!names.Add(e.FullName) || e.FullName.Contains('\\') || e.FullName.Contains(':') || e.FullName.Split('/').Any(s=>s is "" or "." or "..") || e.Length>20*1024*1024 || e.CompressedLength==0 && e.Length>0) Reject();
   expanded+=e.Length; if(expanded>MaxBytes || e.Length>Math.Max(1024*1024,e.CompressedLength*200)) Reject();
   var n=e.FullName;
   if(!Regex.IsMatch(n,@"^(\[Content_Types\]\.xml|_rels/\.rels|docProps/(core|app)\.xml|word/(document|styles|settings|fontTable|numbering|webSettings|header\d+|footer\d+|footnotes|endnotes)\.xml|word/theme/theme\d+\.xml|word/_rels/[A-Za-z0-9]+\.xml\.rels|word/media/[A-Za-z0-9_.-]+\.(png|jpe?g))$",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant)) Reject();
   if(n.StartsWith("word/media/",StringComparison.Ordinal)){using var media=e.Open();var header=new byte[24];media.ReadExactly(header);if(n.EndsWith(".png",StringComparison.OrdinalIgnoreCase)){if(!header.AsSpan(0,8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10})||System.Text.Encoding.ASCII.GetString(header,12,4)!="IHDR")Reject();var width=System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(16,4));var height=System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(20,4));if(width==0||height==0||width>20000||height>20000||(ulong)width*height>25000000)Reject();}else if(header[0]!=255||header[1]!=216||header[2]!=255)Reject();}
   if(n.EndsWith(".xml",StringComparison.OrdinalIgnoreCase)||n.EndsWith(".rels",StringComparison.OrdinalIgnoreCase))
   {
    using var entry=e.Open();using var reader=XmlReader.Create(entry,new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=20*1024*1024});
    var xml=XDocument.Load(reader);
    if(n.EndsWith(".rels",StringComparison.OrdinalIgnoreCase))
    {
     foreach(var rel in xml.Descendants())
     {
      if(rel.Name.LocalName!="Relationship")continue;
      var target=(string?)rel.Attribute("Target")??""; var mode=(string?)rel.Attribute("TargetMode")??"";
      if(mode.Length!=0 && mode!="Internal" || target.Length==0 || target.Contains(':') || target.Contains('\\') || target.Contains('%') || target.Contains('?') || target.Contains('#') || target.Split('/').Any(s=>s is "." or "..") || target.StartsWith('/'))Reject();
      var basePart=n=="_rels/.rels"?"":n[..n.IndexOf("/_rels/",StringComparison.Ordinal)]+"/";if(zip.GetEntry(basePart+target) is null)Reject();
      var type=(string?)rel.Attribute("Type")??"";
      if(!Regex.IsMatch(type,@"^http://schemas\.(openxmlformats\.org/officeDocument/2006|openxmlformats\.org/package/2006|microsoft\.com/office/2006)/relationships/(officeDocument|metadata/core-properties|extended-properties|styles|settings|webSettings|fontTable|theme|numbering|header|footer|footnotes|endnotes|image)$",RegexOptions.CultureInvariant))Reject();
     }
    }
    if(n=="word/document.xml" && !xml.Descendants().Any(el=>el.Name.LocalName=="pgSz"))throw new WordInputException("DOCX harus menetapkan ukuran kertas A4 portrait secara eksplisit.");
    foreach(var size in xml.Descendants().Where(el=>el.Name.LocalName=="pgSz"))
    {
     var w=size.Attributes().FirstOrDefault(a=>a.Name.LocalName=="w")?.Value;
     var h=size.Attributes().FirstOrDefault(a=>a.Name.LocalName=="h")?.Value;
     if(!int.TryParse(w,out var width)||!int.TryParse(h,out var height)||
        !(Math.Abs(width-11906)<=2&&Math.Abs(height-16838)<=2))
      throw new WordInputException("Versi awal menerima A4 portrait. A3, landscape dan Letter belum lolos uji ekspor provider lokal dan tidak dikonversi.");
    }
    foreach(var el in xml.Descendants())
    {
     if(el.Name.LocalName is "altChunk" or "object" or "OLEObject" or "control" or "subDoc" or "attachedTemplate" or "docProtection" or "embeddedFont" or "movie" or "audio" or "video" or "dataBinding")Reject();
     if(el.Name.LocalName is "instrText" or "fldSimple")
     {
      var instruction=el.Name.LocalName=="instrText"?el.Value:el.Attributes().FirstOrDefault(a=>a.Name.LocalName=="instr")?.Value??"";
      // Reject split/unknown instructions instead of materializing or updating them.
      if(!Regex.IsMatch(instruction,@"^\s*(PAGE|NUMPAGES|SECTION|SECTIONPAGES)(\s+\\\*\s+(MERGEFORMAT|Arabic))?\s*$",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant))Reject();
     }
     foreach(var a in el.Attributes())
      if(a.Name.LocalName is "href" or "src" or "base" || a.Value.StartsWith("file:",StringComparison.OrdinalIgnoreCase) || a.Value.StartsWith(@"\\",StringComparison.Ordinal))Reject();
    }
   }
  }
  if(!names.Contains("word/document.xml") || !names.Contains("[Content_Types].xml") || !names.Contains("_rels/.rels"))Reject();
  var content=zip.GetEntry("[Content_Types].xml")!;using var contentStream=content.Open();using var cr=XmlReader.Create(contentStream,new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=1024*1024});
  var types=XDocument.Load(cr);
  var safeTypes=new HashSet<string>(StringComparer.Ordinal){"application/xml","application/vnd.openxmlformats-package.relationships+xml","image/png","image/jpeg","application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml","application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml","application/vnd.openxmlformats-officedocument.wordprocessingml.settings+xml","application/vnd.openxmlformats-officedocument.wordprocessingml.webSettings+xml","application/vnd.openxmlformats-officedocument.wordprocessingml.fontTable+xml","application/vnd.openxmlformats-officedocument.wordprocessingml.numbering+xml","application/vnd.openxmlformats-officedocument.wordprocessingml.header+xml","application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml","application/vnd.openxmlformats-officedocument.wordprocessingml.footnotes+xml","application/vnd.openxmlformats-officedocument.wordprocessingml.endnotes+xml","application/vnd.openxmlformats-officedocument.theme+xml","application/vnd.openxmlformats-package.core-properties+xml","application/vnd.openxmlformats-officedocument.extended-properties+xml"};
  foreach(var type in types.Descendants().Where(e=>e.Name.LocalName is "Default" or "Override"))if(!safeTypes.Contains((string?)type.Attribute("ContentType")??""))Reject();var overrides=types.Descendants().Where(e=>e.Name.LocalName=="Override" && (string?)e.Attribute("PartName")=="/word/document.xml").ToArray();
  if(overrides.Length!=1 || (string?)overrides[0].Attribute("ContentType")!="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml" || types.Descendants().Attributes().Any(a=>a.Value.Contains("macro",StringComparison.OrdinalIgnoreCase)||a.Value.Contains("vba",StringComparison.OrdinalIgnoreCase)||a.Value.Contains("oleObject",StringComparison.OrdinalIgnoreCase)))Reject();
  stream.Position=0;
 }
 internal static void CreateSeed(string path)
 {
  using var file=new FileStream(path,FileMode.CreateNew,FileAccess.Write);using var zip=new ZipArchive(file,ZipArchiveMode.Create);
  void Add(string name,string value){using var writer=new StreamWriter(zip.CreateEntry(name).Open());writer.Write(value);}
  Add("[Content_Types].xml","<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/></Types>");
  Add("_rels/.rels","<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"word/document.xml\"/></Relationships>");
  Add("word/document.xml","<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body><w:p/><w:sectPr><w:pgSz w:w=\"11906\" w:h=\"16838\"/></w:sectPr></w:body></w:document>");
 }
}





