#if DEBUG
using System.Security.Cryptography;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    // The supervising process must verify actual exit. Do not call Application.Exit
    // on success here: doing so would hide a broken last-window shutdown path.
    internal async Task AuditReaderLifetimeAsync(string output, string scenario)
    {
        Directory.CreateDirectory(output);
        var report = new List<string>();
        void Log(string line) { report.Add(line); File.WriteAllLines(Path.Combine(output, "audit.txt"), report); }
        void Check(bool ok, string name) { Log((ok ? "PASS " : "FAIL ") + name); if (!ok) throw new InvalidOperationException(name); }
        var source = Path.Combine(output, "Lifetime synthetic.pdf");
        PdfReaderView.WriteLifetimeFixture(source);
        var before = SHA256.HashData(await File.ReadAllBytesAsync(source));
        await OpenPdfReaderAsync(source);
        await Task.Delay(350);
        Check(pdfReader?.ReaderSource == source && Readers.Count() == 1, "reader is loaded before native close");
        var last = this;
        if (scenario == "primary-first")
        {
            DetachReaderTab(pdfReader!);
            last = readerWindows.Single(w => w != this);
            await Task.Delay(250);
            // Empty primary shell needs its normal confirmation; use a fresh
            // reader there so the real reader-only close guard is exercised.
            var second = Path.Combine(output, "Other synthetic.pdf");
            PdfReaderView.WriteLifetimeFixture(second);
            await OpenPdfReaderAsync(second);
            await ConfirmCloseAsync();
            await Task.Delay(250);
            Check(readerWindowClosed && !last.readerWindowClosed && readerWindows.Count == 1,
                "closing primary preserves detached reader and process");
        }
        else if (scenario != "single") throw new ArgumentException("Unknown lifetime scenario.");
        var after = SHA256.HashData(await File.ReadAllBytesAsync(source));
        Check(before.SequenceEqual(after), "reading did not change synthetic source");
        Log("CLOSE_REQUEST last window; supervisor must verify process exit");
        await last.ConfirmCloseAsync();
        // Reaching this point alone is NOT a passed process-lifetime assertion.
    }

    private sealed partial class PdfReaderView
    {
        internal static void WriteLifetimeFixture(string path) => WriteContinuousScrollFixture(path, 3);
    }
}
#endif
