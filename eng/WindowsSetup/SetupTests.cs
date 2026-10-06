using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oto.Setup
{
    internal static class SetupTests
    {
        internal static async Task<int> RunAsync(string output)
        {
            Directory.CreateDirectory(output); var results = new List<string>();
            Action<bool, string> check = (condition, label) => { if (!condition) throw new InvalidOperationException(label); results.Add("PASS " + label); };
            Action<Action, string> reject = (action, label) => { try { action(); } catch (Exception e) when (e is IOException || e is InvalidDataException || e is OperationCanceledException) { results.Add("PASS " + label); return; } throw new InvalidOperationException(label); };
            var quiet = new Progress<SetupProgress>();
            try
            {
                check(RuntimeRequirement.AcceptsVersion("10.0.0") && RuntimeRequirement.AcceptsVersion("10.0.12"), "stable .NET 10 runtime accepted");
                foreach (var value in new[] { "8.0.30", "11.0.0", "10.1.0", "10.0", "10.0.1-preview.1", "not-a-version" })
                    check(!RuntimeRequirement.AcceptsVersion(value), "incompatible version rejected: " + value);
                var runtime = Path.Combine(output, "runtime"); var shared = Path.Combine(runtime, "shared", "Microsoft.NETCore.App", "10.0.11"); var host = Path.Combine(runtime, "host", "fxr", "10.0.12");
                Directory.CreateDirectory(shared); Directory.CreateDirectory(host);
                Pe(Path.Combine(runtime, "dotnet.exe"), 0x8664); Pe(Path.Combine(shared, "coreclr.dll"), 0x8664); Pe(Path.Combine(host, "hostfxr.dll"), 0x8664); File.WriteAllText(Path.Combine(shared, "System.Private.CoreLib.dll"), "inert");
                check(RuntimeRequirement.FindInRoot(runtime) == "10.0.11", "x64 runtime found with a newer compatible hostfxr");
                Pe(Path.Combine(runtime, "dotnet.exe"), 0x14c); check(RuntimeRequirement.FindInRoot(runtime) == null, "x86 dotnet is not accepted as x64 prerequisite");
                Pe(Path.Combine(runtime, "dotnet.exe"), 0x8664); Pe(Path.Combine(shared, "coreclr.dll"), 0x14c); check(RuntimeRequirement.FindInRoot(runtime) == null, "x86 coreclr is not accepted");
                check(RuntimeRequirement.FindInRoot(Path.Combine(output, "missing")) == null, "missing runtime root detected");
                reject(() => SetupOperations.SafeEntry(output, "../outside.txt"), "archive traversal rejected");
                reject(() => SetupOperations.SafeEntry(output, "C:/outside.txt"), "absolute archive path rejected");
                reject(() => SetupOperations.SafeEntry(output, "test.exe:stream"), "alternate stream rejected");
                reject(() => SetupOperations.SafeEntry(output, "folder./bad.txt"), "trailing-dot path rejected");
                reject(() => SetupOperations.CleanPrivateFolder(output), "cleanup cannot target arbitrary workspace folder");
                var privateFolder = SetupOperations.CreatePrivateFolder(); File.WriteAllText(Path.Combine(privateFolder, "fixture.txt"), "test"); SetupOperations.CleanPrivateFolder(privateFolder);
                check(!Directory.Exists(privateFolder), "only dedicated private staging folder is cleaned");
                using (var zip = Zip("../escape.txt", "x")) reject(() => SetupOperations.Extract(zip, Path.Combine(output, "extract-bad"), 1, quiet, CancellationToken.None), "zip-slip rejected before output write");
                check(!File.Exists(Path.Combine(output, "escape.txt")), "zip-slip does not create outside file");
                using (var zip = Zip("safe.txt", "hello")) reject(() => SetupOperations.Extract(zip, Path.Combine(output, "extract-size"), 1, quiet, CancellationToken.None), "expanded size mismatch rejected");
                using (var zip = Zip("safe.txt", "hello")) SetupOperations.Extract(zip, Path.Combine(output, "extract-ok"), 5, quiet, CancellationToken.None);
                check(File.ReadAllText(Path.Combine(output, "extract-ok", "safe.txt")) == "hello", "valid bounded archive extracted");
                var longName = new string('a', 90) + "/" + new string('b', 90) + "/test.txt";
                using (var zip = Zip(longName, "long")) SetupOperations.Extract(zip, Path.Combine(output, "extract-long"), 4, quiet, CancellationToken.None);
                check(File.ReadAllText(@"\\?\" + Path.GetFullPath(Path.Combine(output, "extract-long", longName))) == "long", "payload extracts under long Windows user/workspace paths");
                using (var duplicate = new MemoryStream())
                {
                    using (var zip = new ZipArchive(duplicate, ZipArchiveMode.Create, true)) { zip.CreateEntry("A.txt"); zip.CreateEntry("a.txt"); }
                    duplicate.Position = 0;
                    reject(() => SetupOperations.Extract(duplicate, Path.Combine(output, "extract-duplicate"), 0, quiet, CancellationToken.None), "case-insensitive duplicate ZIP names rejected");
                }
                using (var zip = Zip("safe.txt", "hello")) reject(() => SetupOperations.Extract(zip, Path.Combine(output, "extract-cancel"), 5, quiet, new CancellationToken(true)), "extraction cancellation stops before file write");
                var bytes = Encoding.UTF8.GetBytes("pinned fixture"); string hash;
                using (var sha = SHA512.Create()) hash = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
                using (var input = new MemoryStream(bytes)) TrustedRuntime.VerifyHash(input, hash);
                results.Add("PASS matching SHA512 accepted");
                using (var input = new MemoryStream(bytes)) reject(() => TrustedRuntime.VerifyHash(input, new string('0', 128)), "bad SHA512 rejects downloaded executable");
                reject(() => TrustedRuntime.VerifyMicrosoftSignature(Assembly.GetExecutingAssembly().Location), "unsigned bootstrapper cannot masquerade as Microsoft runtime");

                using (var client = new HttpClient(new FakeHandler((request, token) =>
                {
                    check(request.RequestUri.AbsoluteUri == RuntimeRequirement.DownloadUrl && request.Method == HttpMethod.Get && request.RequestUri.Query.Length == 0, "download only requests pinned HTTPS Microsoft URL");
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) });
                })))
                    await TrustedRuntime.DownloadAsync(client, Path.Combine(output, "download.bin"), quiet, CancellationToken.None);
                check(File.ReadAllBytes(Path.Combine(output, "download.bin")).SequenceEqual(bytes), "download streamed completely");
                await RejectAsync(async () =>
                {
                    using (var client = new HttpClient(new FakeHandler((_, __) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Redirect)))))
                        await TrustedRuntime.DownloadAsync(client, Path.Combine(output, "redirect.bin"), quiet, CancellationToken.None);
                }, results, "redirect response does not become executable");
                await RejectAsync(async () =>
                {
                    using (var client = new HttpClient(new FakeHandler((_, __) =>
                    { var content = new ByteArrayContent(bytes); content.Headers.ContentLength = RuntimeRequirement.MaximumDownload + 1; return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content }); })))
                        await TrustedRuntime.DownloadAsync(client, Path.Combine(output, "oversized.bin"), quiet, CancellationToken.None);
                }, results, "oversized download rejected before write");
                await RejectAsync(async () =>
                {
                    using (var client = new HttpClient(new FakeHandler((_, __) =>
                    { var content = new ByteArrayContent(bytes); content.Headers.ContentLength = bytes.Length + 1; return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content }); })))
                        await TrustedRuntime.DownloadAsync(client, Path.Combine(output, "truncated.bin"), quiet, CancellationToken.None);
                }, results, "truncated runtime download rejected");
                await RejectAsync(async () =>
                {
                    using (var client = new HttpClient(new FakeHandler((_, __) => throw new HttpRequestException("offline"))))
                        await TrustedRuntime.DownloadAsync(client, Path.Combine(output, "offline.bin"), quiet, CancellationToken.None);
                }, results, "offline download failure is recoverable");

                var installed = new FakeOperations { Present = true };
                await SetupFlow.RunAsync(installed, quiet, CancellationToken.None);
                check(installed.Downloads == 0 && installed.RuntimeInstalls == 0 && installed.AppInstalls == 1, "existing runtime skips all network and runtime installation");
                var missing = new FakeOperations(); await SetupFlow.RunAsync(missing, quiet, CancellationToken.None);
                check(missing.Downloads == 1 && missing.RuntimeInstalls == 1 && missing.Detections == 2 && missing.AppInstalls == 1, "missing runtime installed and redetected before app installation");
                foreach (var code in new[] { 1223, 1602, 1603, 1641 })
                {
                    var failure = new FakeOperations { RuntimeExitCode = code };
                    await RejectAsync(() => SetupFlow.RunAsync(failure, quiet, CancellationToken.None), results, "runtime exit " + code + " stops app installation");
                    check(failure.AppInstalls == 0, "no app activation after prerequisite failure " + code);
                }
                var reboot = new FakeOperations { RuntimeExitCode = 3010 };
                check((await SetupFlow.RunAsync(reboot, quiet, CancellationToken.None)).RestartRequired && reboot.AppInstalls == 1, "3010 requires user restart and does not request automatic reboot");
                var absentAfter = new FakeOperations { AppearsAfterInstall = false };
                await RejectAsync(() => SetupFlow.RunAsync(absentAfter, quiet, CancellationToken.None), results, "success exit without runtime detection cannot activate OTO");
                check(absentAfter.AppInstalls == 0, "missing post-install prerequisite preserves app state");
                var corrupt = new FakeOperations { BadDownload = true };
                await RejectAsync(() => SetupFlow.RunAsync(corrupt, quiet, CancellationToken.None), results, "corrupt prerequisite download stops coordinator");
                check(corrupt.RuntimeInstalls == 0 && corrupt.AppInstalls == 0, "invalid prerequisite never executed");
                var shortcut = new FakeOperations { Present = true, AppExitCode = 2 };
                check((await SetupFlow.RunAsync(shortcut, quiet, CancellationToken.None)).ShortcutWarning, "shortcut warning distinguished from failed install");
                var appFailure = new FakeOperations { Present = true, AppExitCode = 1 };
                await RejectAsync(() => SetupFlow.RunAsync(appFailure, quiet, CancellationToken.None), results, "app installation failure not reported as success");
                var cancelled = new FakeOperations(); await RejectAsync(() => SetupFlow.RunAsync(cancelled, quiet, new CancellationToken(true)), results, "cancellation prevents prerequisite and app execution");
                check(cancelled.RuntimeInstalls == 0 && cancelled.AppInstalls == 0, "cancelled flow has no installer execution");
                SetupOperations.VerifyPayload(); results.Add("PASS actual embedded OTO payload hash verified");
                results.Add("INFO locally detected runtime: " + (RuntimeRequirement.Detect() ?? "missing"));
                check(Environment.Version.Major == 4, "bootstrapper runs on built-in .NET Framework CLR without loading modern .NET 10");
                File.WriteAllLines(Path.Combine(output, "test-results.txt"), results); return 0;
            }
            catch (Exception e) { results.Add("FAIL " + e); File.WriteAllLines(Path.Combine(output, "test-results.txt"), results); return 1; }
        }
        private static async Task RejectAsync(Func<Task> action, List<string> results, string label)
        { try { await action(); } catch (Exception e) when (e is IOException || e is InvalidDataException || e is HttpRequestException || e is OperationCanceledException || e is InvalidOperationException) { results.Add("PASS " + label); return; } throw new Exception(label); }
        private static MemoryStream Zip(string name, string value)
        {
            var stream = new MemoryStream();
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true)) using (var writer = new StreamWriter(zip.CreateEntry(name).Open(), new UTF8Encoding(false))) writer.Write(value);
            stream.Position = 0; return stream;
        }
        private static void Pe(string path, ushort architecture)
        {
            using (var file = new FileStream(path, FileMode.Create)) using (var writer = new BinaryWriter(file))
            { writer.Write((ushort)0x5A4D); file.Position = 60; writer.Write(128); file.Position = 128; writer.Write(0x4550); writer.Write(architecture); }
        }
        private sealed class FakeHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send;
            internal FakeHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> action) { send = action; }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) { return send(request, token); }
        }
        private sealed class FakeOperations : ISetupOperations
        {
            internal bool Present, AppearsAfterInstall = true, BadDownload;
            internal int Downloads, RuntimeInstalls, AppInstalls, Detections, RuntimeExitCode, AppExitCode;
            public string DetectRuntime() { Detections++; return Present ? "10.0.12" : null; }
            public Task PrepareAsync(IProgress<SetupProgress> progress, CancellationToken token) { token.ThrowIfCancellationRequested(); return Task.CompletedTask; }
            public Task DownloadRuntimeAsync(IProgress<SetupProgress> progress, CancellationToken token) { Downloads++; if (BadDownload) throw new InvalidDataException("corrupt fixture"); return Task.CompletedTask; }
            public Task<int> InstallRuntimeAsync() { RuntimeInstalls++; Present = AppearsAfterInstall; return Task.FromResult(RuntimeExitCode); }
            public Task<int> InstallAppAsync() { AppInstalls++; return Task.FromResult(AppExitCode); }
        }
    }
}
