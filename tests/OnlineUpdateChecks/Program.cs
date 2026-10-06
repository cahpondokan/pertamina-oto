using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using Pertamina.Oto.Distribution;
using Pertamina.Oto.Desktop.App;
using System.Text.Json;
if (args.FirstOrDefault() is "--full-package" or "--live-transition" or "--native-live" or "--bootstrap-startup") return await FinalizationChecks.RunAsync(args);
var evidence = Path.GetFullPath(args[0]); Directory.CreateDirectory(evidence);
var results = new List<string>();
void Check(bool value, string label) { if (!value) throw new InvalidOperationException(label); results.Add("PASS " + label); }
async Task Reject(Func<Task> action, string label) { try { await action(); } catch (Exception e) when (e is IOException or InvalidDataException or OperationCanceledException or HttpRequestException) { results.Add("PASS " + label); return; } throw new InvalidOperationException(label); }
byte[] Archive(params string[] names) { using var memory = new MemoryStream(); using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, true)) foreach (var name in names) { using var writer = new StreamWriter(zip.CreateEntry(name).Open()); writer.Write("inert payload"); } return memory.ToArray(); }
try
{
 if(args.Contains("--real",StringComparer.Ordinal)) {
  using var client = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false, UseCookies = false });
  var online = await OnlineReleaseClient.FetchAsync(client,CancellationToken.None) ?? throw new InvalidDataException("No live release");
  var zipPath = Path.Combine(evidence,"real.zip");
  await OnlineUpdatePackage.DownloadAsync(client,online,zipPath,null,CancellationToken.None);
  var extracted = Path.Combine(evidence,"real-extracted"); OnlineUpdatePackage.Extract(zipPath,extracted,CancellationToken.None);
  var candidate = LocalAppUpdate.InspectFeed(Path.Combine(extracted,"feed"));
  Check(candidate.Release.Version == online.Version,"real HTTPS ZIP feed version matches");
  foreach(var entry in candidate.Manifest.Files) LocalAppUpdate.Verify(LocalAppUpdate.SafePath(candidate.Package,"app/"+entry.Path),entry);
  Check(true,$"real HTTPS ZIP {candidate.Manifest.Files.Length} payload hashes verified");
  File.WriteAllLines(Path.Combine(evidence,"real-results.txt"),results);
  Console.WriteLine($"Live build {online.Version}: {results.Count} PASS (no activation)"); return 0;
 }
 var data = Archive("feed/latest.json");
 var release = new ReleaseInfo(1,"20261002-180000-aaaaaaaa",DateTimeOffset.UtcNow,"development","PERTAMINA-OTO-Windows-x64-20261002-180000-aaaaaaaa.zip",data.Length,Convert.ToHexString(SHA256.HashData(data)),["Test"]);
 Check(release.IsNewerThan("20261001-180000-bbbbbbbb"),"newer offered");
 Check(!release.IsNewerThan(release.Version) && !release.IsNewerThan("20261003-180000-bbbbbbbb"),"same and older not offered");
 async Task Download(ReleaseInfo r, HttpStatusCode code = HttpStatusCode.OK, bool redirect = false, bool interrupted = false)
 {
  using var client = new HttpClient(new Handler(async (request, token) => {
   Check(request.RequestUri == new Uri(ReleaseInfo.Origin + "/downloads/" + release.FileName),"fixed HTTPS ZIP endpoint");
   if(interrupted) await Task.Delay(Timeout.Infinite,token);
   return new HttpResponseMessage(code) { Content = new ByteArrayContent(data), RequestMessage = redirect ? new HttpRequestMessage(HttpMethod.Get,"https://example.com/") : request };
  }));
  using var cancel = new CancellationTokenSource(interrupted ? 30 : 3000);
  await OnlineUpdatePackage.DownloadAsync(client,r,Path.Combine(evidence,Guid.NewGuid()+".zip"),null,cancel.Token);
 }
 await Download(release); Check(true,"verified streaming download");
 await Reject(()=>Download(release with { Sha256 = new string('0',64) }),"corrupt hash rejected");
 await Reject(()=>Download(release with { Size = data.Length+1 }),"length mismatch rejected");
 await Reject(()=>Download(release,HttpStatusCode.Redirect),"redirect rejected");
 await Reject(()=>Download(release,redirect:true),"foreign final URI rejected");
 await Reject(()=>Download(release,interrupted:true),"interrupted download cancelled");
 await Reject(()=>Download(release with { FileName = "../evil.zip" }),"invalid metadata rejected");
 foreach(var names in new[] { new[]{"../escape"},new[]{"C:/escape"},new[]{"a\\b"},new[]{"a","A"},new[]{"a","a/b"},new[]{"a./b"},new[]{"CON.txt"},new[]{"COM¹.txt"},new[]{"a:stream"} }) {
  var file = Path.Combine(evidence,Guid.NewGuid()+".zip"); File.WriteAllBytes(file,Archive(names));
  await Reject(()=>Task.Run(()=>OnlineUpdatePackage.Extract(file,Path.Combine(evidence,Guid.NewGuid().ToString()),CancellationToken.None)),"unsafe ZIP rejected: "+string.Join(',',names));
 }
 foreach(var kind in new[]{"symlink","bomb"}) {
  var file = Path.Combine(evidence,kind+".zip");
  using(var zip = ZipFile.Open(file,ZipArchiveMode.Create)) {
   var entry = zip.CreateEntry("payload",CompressionLevel.SmallestSize);
   if(kind == "symlink") entry.ExternalAttributes = unchecked((int)0xA0000000);
   using var stream = entry.Open(); stream.Write(kind == "bomb" ? new byte[4*1024*1024] : new byte[1]);
  }
  await Reject(()=>Task.Run(()=>OnlineUpdatePackage.Extract(file,Path.Combine(evidence,kind),CancellationToken.None)),kind+" rejected before extraction");
 }
 var good = Path.Combine(evidence,"good.zip"); File.WriteAllBytes(good,Archive("feed/latest.json","feed/build/app/file"));
 OnlineUpdatePackage.Extract(good,Path.Combine(evidence,"good"),CancellationToken.None); Check(File.Exists(Path.Combine(evidence,"good/feed/build/app/file")),"safe ZIP extracted");
 using var stopped = new CancellationTokenSource(); stopped.Cancel();
 await Reject(()=>Task.Run(()=>OnlineUpdatePackage.Extract(good,Path.Combine(evidence,"cancel"),stopped.Token)),"extraction cancellation");
 Check(UpdateTests.Run(Path.Combine(evidence,"legacy")) == 0,"13 legacy activation and corruption tests");
 var feed = Path.Combine(evidence,"feed"); var version = release.Version; var package = Path.Combine(feed,version);
 Directory.CreateDirectory(Path.Combine(package,"app/worker"));
 var entries = new List<LocalAppUpdate.Entry>();
 foreach(var name in new[]{"Pertamina.Oto.Desktop.App.exe","worker/Pertamina.Oto.LocalPdf.Worker.exe"}) {
  var path = Path.Combine(package,"app",name); File.WriteAllText(path,"inert");
  entries.Add(new(name,new FileInfo(path).Length,Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))));
 }
 var manifest = Path.Combine(package,"manifest.json"); File.WriteAllText(manifest,JsonSerializer.Serialize(new LocalAppUpdate.Manifest(version,entries.ToArray())));
 File.WriteAllText(Path.Combine(feed,"latest.json"),JsonSerializer.Serialize(new LocalAppUpdate.Release(version,Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(manifest))))));
 var installedRoot = Path.Combine(evidence,"installed"); Directory.CreateDirectory(installedRoot);
 const string first = "20261001-180000-bbbbbbbb";
 File.WriteAllText(Path.Combine(installedRoot,"current.json"),JsonSerializer.Serialize(new LocalAppUpdate.Release(first,new string('0',64))));
 await Reject(()=>Task.Run(()=>LocalAppUpdate.InstallFrom(installedRoot,feed,stopped.Token)),"cancel before activation rejected");
 Check(LocalAppUpdate.VersionAt(installedRoot) == first,"cancel preserves old active pointer");
 Check(LocalAppUpdate.InstallFrom(installedRoot,feed) == version,"verified inert feed activates B");
 Check(LocalAppUpdate.Read<LocalAppUpdate.Release>(Path.Combine(installedRoot,"previous.json")).Version == first,"previous pointer preserved");
 File.WriteAllLines(Path.Combine(evidence,"results.txt"),results); Console.WriteLine($"{results.Count} PASS, 0 FAIL"); return 0;
}
catch(Exception error) { results.Add("FAIL "+error); File.WriteAllLines(Path.Combine(evidence,"results.txt"),results); Console.Error.WriteLine(error); return 1; }
internal sealed class Handler(Func<HttpRequestMessage,CancellationToken,Task<HttpResponseMessage>> send) : HttpMessageHandler
{ protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)=>send(request,token); }
