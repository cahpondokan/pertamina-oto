using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using Pertamina.Oto.Desktop.App;

if (args.Length == 3 && args[0] == "--send")
{
    using var bridge = new ReaderActivation(args[1]);
    Environment.ExitCode = !bridge.IsPrimary && bridge.ForwardAsync(args[2]).GetAwaiter().GetResult() ? 0 : 1;
    return;
}
var output = Path.GetFullPath(args[0]); Directory.CreateDirectory(output);
var report = new List<string>();
void Check(bool ok, string name) { report.Add((ok ? "PASS " : "FAIL ") + name); if (!ok) throw new InvalidOperationException(name); }
try
{
    Check(ReaderActivation.IsNormalLaunch([]), "normal shortcut can register a receiver");
    Check(!ReaderActivation.IsNormalLaunch(["--audit-reader-ui"]) && !ReaderActivation.IsNormalLaunch(["--restart-ready", "token"]), "audit and restart launches isolated from production activation");
    var path = Path.Combine(output, "spasi ü & $(literal).pdf");
    Check(ReaderActivation.IsNormalLaunch(["--open-reader", path]) && ReaderActivation.IsNormalLaunch([path]), "Explorer and direct path supported without shell parsing");
    Check(!ReaderActivation.ValidPath("relative.pdf") && !ReaderActivation.ValidPath(@"\\server\document.pdf") && !ReaderActivation.ValidPath("https://server/document.pdf"), "relative remote and URL input refused");
    Check(!ReaderActivation.ValidPath(Path.Combine(output, "other.exe")) && !ReaderActivation.ValidPath(path + "\n"), "non-PDF and control characters refused");
    var identity = "synthetic-reader-test-" + Guid.NewGuid().ToString("N");
    using var primary = new ReaderActivation(identity);
    primary.Diagnostic = Console.WriteLine;
    var received = new ConcurrentQueue<string?>();
    primary.Start(p => { received.Enqueue(p); return Task.FromResult(true); });
    Check(primary.IsPrimary, "first instance owns activation receiver");
    using var secondary = new ReaderActivation(identity);
    secondary.Diagnostic = Console.WriteLine;
    Check(!secondary.IsPrimary, "second instance does not become another primary");
    Check(secondary.ForwardAsync(path).GetAwaiter().GetResult(), "second instance receives positive delivery acknowledgement");
    Check(received.TryDequeue(out var actual) && actual == path, "Unicode spaces and shell characters remain one literal path");
    Check(secondary.ForwardAsync(null).GetAwaiter().GetResult() && received.TryDequeue(out actual) && actual is null, "shortcut activation without PDF supported");
    Check(!secondary.ForwardAsync("relative.pdf").GetAwaiter().GetResult() && received.IsEmpty, "invalid input never reaches receiver");
    var start = new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = false, CreateNoWindow = true };
    start.ArgumentList.Add("--send"); start.ArgumentList.Add(identity); start.ArgumentList.Add(path);
    using (var process = Process.Start(start)!)
    {
        Check(process.WaitForExit(15000) && process.ExitCode == 0 && received.TryDequeue(out actual) && actual == path,
            "separate process forwards PDF and exits without a second receiver");
    }
    var parallel = Enumerable.Range(0, 8).Select(i => secondary.ForwardAsync(Path.Combine(output, $"{i}.pdf"))).ToArray();
    Check(Task.WhenAll(parallel).GetAwaiter().GetResult().All(x => x) && received.Count == 8, "concurrent PDF launches all reach one receiver");
    while (received.TryDequeue(out _)) { }
    var pipeName = "OTO.Reader." + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))[..32];
    foreach (var malformed in new[] { -1, 200000 })
    {
        using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.CurrentUserOnly);
        client.Connect(3000); client.Write(BitConverter.GetBytes(malformed));
    }
    Check(secondary.ForwardAsync(path).GetAwaiter().GetResult(), "oversized and negative request lengths cannot stop receiver");
    using var isolated = new ReaderActivation(identity + "-other-build");
    Check(isolated.IsPrimary, "different installation or build keeps independent activation identity");
}
catch (Exception error) { report.Add("FAIL " + error.GetType().Name + ": " + error.Message); Environment.ExitCode = 1; }
finally { File.WriteAllLines(Path.Combine(output, "checks.txt"), report); Console.WriteLine(string.Join(Environment.NewLine, report)); }
