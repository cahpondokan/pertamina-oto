using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Oto.OnlineSetup
{
    internal static class OnlineTests
    {
        internal static int VerifyLocalDownload(string source, string output)
        {
            Directory.CreateDirectory(output);
            try
            {
                long total = 0;
                var target = Path.Combine(output, "verified-offline-installer.exe");
                using (var client = new HttpClient(new Handler((request, token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request, Content = new StreamContent(File.OpenRead(source)) }))))
                    OnlineDownload.FetchAsync(client, new Uri(OnlineInfo.Url), target, OnlineInfo.Size, OnlineInfo.Sha256, (done, size) => total = done, CancellationToken.None).GetAwaiter().GetResult();
                OnlineDownload.Verify(target, OnlineInfo.Size, OnlineInfo.Sha256);
                if (total != OnlineInfo.Size) throw new Exception("full payload progress mismatch");
                File.WriteAllText(Path.Combine(output, "results.txt"), "PASS actual full offline installer streamed through online downloader: " + total + " bytes; exact pinned SHA256. Transport is a local HTTP handler; no public HTTPS, installer execution or installation.");
                return 0;
            }
            catch (Exception error) { File.WriteAllText(Path.Combine(output, "results.txt"), "FAIL " + error.GetType().Name); return 1; }
        }
        internal static int Run(string output)
        {
            Directory.CreateDirectory(output); var results = new List<string>();
            try
            {
                var bytes = Encoding.UTF8.GetBytes("Synthetic installer bytes; never executed.");
                if (!OnlinePlatform.Accept(9, new Version(10, 0, 19041))) throw new Exception("x64 platform"); results.Add("PASS native x64 Windows minimum accepted");
                foreach (var architecture in new ushort[] { 0, 12 }) { if (OnlinePlatform.Accept(architecture, new Version(10, 0, 22631))) throw new Exception("wrong architecture"); results.Add("PASS unsupported native architecture " + architecture); }
                if (OnlinePlatform.Accept(9, new Version(10, 0, 17763))) throw new Exception("minimum OS"); results.Add("PASS older Windows build rejected before download");
                string hash; using (var sha = SHA256.Create()) hash = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
                var uri = new Uri("https://oto.ayani.id/downloads/PERTAMINA-OTO-Setup-x64-20261011-104936-27c39aeb.exe");
                var counter = 0;
                Action<Func<Task>, string> reject = (action, label) => { try { action().GetAwaiter().GetResult(); } catch (Exception e) when (e is InvalidDataException || e is HttpRequestException || e is OperationCanceledException || e is IOException) { results.Add("PASS " + label); return; } throw new Exception(label); };
                Func<HttpResponseMessage, HttpClient> client = response => new HttpClient(new Handler((request, cancellation) => Task.FromResult(response)));
                Func<string> path = () => Path.Combine(output, "attempt-" + (++counter) + ".bin");
                using (var http = client(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) }))
                {
                    long done = 0; var destination = path();
                    OnlineDownload.FetchAsync(http, uri, destination, bytes.Length, hash, (d, t) => { done = d; if (t != bytes.Length) throw new Exception("total"); }, CancellationToken.None).GetAwaiter().GetResult();
                    OnlineDownload.Verify(destination, bytes.Length, hash);
                    if (done != bytes.Length) throw new Exception("progress"); results.Add("PASS exact bytes/hash/progress verified, inert bytes not executed");
                    File.WriteAllText(destination, "tamper"); reject(() => { OnlineDownload.Verify(destination, bytes.Length, hash); return Task.CompletedTask; }, "tamper before execute");
                }
                using (var http = client(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) })) reject(() => OnlineDownload.FetchAsync(http, uri, path(), bytes.Length, new string('a', 64), (_, __) => {}, CancellationToken.None), "bad hash");
                using (var http = client(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) })) reject(() => OnlineDownload.FetchAsync(http, uri, path(), bytes.Length + 1, hash, (_, __) => {}, CancellationToken.None), "wrong advertised length");
                using (var http = client(new HttpResponseMessage(HttpStatusCode.OK) { Content = new UnknownLengthContent(bytes) })) reject(() => OnlineDownload.FetchAsync(http, uri, path(), bytes.Length + 1, hash, (_, __) => {}, CancellationToken.None), "truncated chunked payload");
                using (var http = client(new HttpResponseMessage(HttpStatusCode.OK) { Content = new UnknownLengthContent(bytes) })) reject(() => OnlineDownload.FetchAsync(http, uri, path(), bytes.Length - 1, hash, (_, __) => {}, CancellationToken.None), "oversized chunked payload");
                var attempts = 0;
                using (var http = new HttpClient(new Handler((request, token) =>
                { if (++attempts == 1) throw new HttpRequestException("transient"); return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) }); })))
                {
                    OnlineDownload.FetchAsync(http, uri, path(), bytes.Length, hash, (_, __) => {}, CancellationToken.None).GetAwaiter().GetResult();
                    if (attempts != 2) throw new Exception("retry count"); results.Add("PASS transient failure succeeds on exactly one bounded retry");
                }
                using (var http = client(new HttpResponseMessage(HttpStatusCode.Redirect) { Content = new ByteArrayContent(bytes) })) reject(() => OnlineDownload.FetchAsync(http, uri, path(), bytes.Length, hash, (_, __) => {}, CancellationToken.None), "redirect");
                using (var http = client(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes), RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://evil.invalid/setup.exe") })) reject(() => OnlineDownload.FetchAsync(http, uri, path(), bytes.Length, hash, (_, __) => {}, CancellationToken.None), "cross-origin final URI");
                using (var http = new HttpClient(new Handler((request, cancellation) => { throw new HttpRequestException("offline"); }))) reject(() => OnlineDownload.FetchAsync(http, uri, path(), bytes.Length, hash, (_, __) => {}, CancellationToken.None), "offline bounded retry");
                using (var http = client(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) }))
                    foreach (var bad in new[] { "http://oto.ayani.id/downloads/PERTAMINA-OTO-Setup-x64-a.exe", "https://evil.invalid/downloads/PERTAMINA-OTO-Setup-x64-a.exe", uri + "?id=user", "https://oto.ayani.id:444/downloads/PERTAMINA-OTO-Setup-x64-a.exe" })
                        reject(() => OnlineDownload.FetchAsync(http, new Uri(bad), path(), bytes.Length, hash, (_, __) => {}, CancellationToken.None), "URL allowlist " + counter);
                using (var source = new CancellationTokenSource())
                using (var http = client(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) }))
                { source.Cancel(); reject(() => OnlineDownload.FetchAsync(http, uri, path(), bytes.Length, hash, (_, __) => {}, source.Token), "cancel before request"); }
                using (var source = new CancellationTokenSource())
                using (var http = client(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) }))
                    reject(() => OnlineDownload.FetchAsync(http, uri, path(), bytes.Length, hash, (_, __) => source.Cancel(), source.Token), "cancel after final byte before execute");
                if (Environment.Version.Major != 4) throw new Exception("Not Framework CLR4"); results.Add("PASS Framework CLR4 bootstrap; no .NET10 dependency");
                if (System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames().Length != 0) throw new Exception("Unexpected embedded payload");
                results.Add("PASS thin executable contains no embedded full application payload");
                File.WriteAllLines(Path.Combine(output, "results.txt"), results); return 0;
            }
            catch (Exception e) { results.Add("FAIL " + e); File.WriteAllLines(Path.Combine(output, "results.txt"), results); return 1; }
        }
        private sealed class Handler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> response;
            internal Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> response) { this.response = response; }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) { cancellationToken.ThrowIfCancellationRequested(); return response(request, cancellationToken); }
        }
        private sealed class UnknownLengthContent : HttpContent
        {
            private readonly byte[] bytes;
            internal UnknownLengthContent(byte[] bytes) { this.bytes = bytes; }
            protected override bool TryComputeLength(out long length) { length = 0; return false; }
            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context) { return stream.WriteAsync(bytes, 0, bytes.Length); }
            protected override Task<Stream> CreateContentReadStreamAsync() { return Task.FromResult<Stream>(new MemoryStream(bytes)); }
        }
    }
    internal static class OnlineUiAudit
    {
        internal static int Run(string output)
        {
            Directory.CreateDirectory(output); var results = new List<string>();
            foreach (var width in new[] { 680, 480, 360 })
            foreach (var height in new[] { 520, 360 })
            using (var window = new OnlineWindow(true))
            {
                window.Size = new Size(width, height); window.Show(); Application.DoEvents();
                foreach (var state in new[] { "Internet diperlukan untuk mengunduh installer lengkap.", "Mengunduh 1.234.567 / 299.000.000 byte", "Memverifikasi installer…", "Unduhan dibatalkan. Versi aktif tetap tersedia.", "Koneksi belum tersedia. Coba kembali saat internet siap." })
                {
                    window.State(state, 42); window.PerformLayout(); Application.DoEvents();
                    var file = Path.Combine(output, width + "x" + height + "-" + results.Count + ".png");
                    using (var bitmap = new Bitmap(window.Width, window.Height)) { window.DrawToBitmap(bitmap, new Rectangle(0, 0, window.Width, window.Height)); bitmap.Save(file); }
                    if (window.Controls.Count != 2 || window.AcceptButton == null || window.CancelButton == null) throw new Exception("Missing keyboard actions");
                    var primary = (Button)window.AcceptButton; var secondary = (Button)window.CancelButton;
                    foreach (var button in new[] { primary, secondary })
                    {
                        var bounds = window.RectangleToClient(button.RectangleToScreen(button.ClientRectangle));
                        if (!window.ClientRectangle.Contains(bounds) || button.AccessibleName.Length == 0) throw new Exception("Action outside viewport " + width + "x" + height);
                    }
                    window.Activate(); primary.Select(); Application.DoEvents(); primary.Focus();
                    if (!primary.Focused) throw new Exception("Primary action focus unavailable " + width + "x" + height);
                    results.Add("PASS native render " + width + " " + state);
                }
                window.Close();
            }
            File.WriteAllLines(Path.Combine(output, "results.txt"), results); return 0;
        }
    }
}
