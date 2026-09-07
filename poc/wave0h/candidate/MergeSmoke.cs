// Unbuilt smoke harness. Only reviewed generator output in a private guest.
// External process/guest supervision is mandatory; fatal stack overflow is not caught.
using System;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

internal static class MergeSmoke
{
    public static int Main(string[] args)
    {
        if (args.Length < 3 || args.Length > 4) return 2;
        string output = Path.GetFullPath(args[0]);
        if (File.Exists(output)) return 3;
        for (int i = 1; i < args.Length; i++)
        {
            string input = Path.GetFullPath(args[i]);
            if (string.Equals(input, output, StringComparison.OrdinalIgnoreCase)) return 3;
            if (!File.Exists(input) || new FileInfo(input).Length > 65536) return 4;
        }
        using var merged = new PdfDocument();
        for (int i = 1; i < args.Length; i++)
        {
            using var source = PdfReader.Open(args[i], PdfDocumentOpenMode.Import);
            if (source.PageCount < 1 || source.PageCount > 3) return 5;
            for (int page = 0; page < source.PageCount; page++)
                merged.AddPage(source.Pages[page]);
        }
        using var target = new FileStream(output, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
        merged.Save(target, false);
        // Post-write check only; external disk/resource limits are still necessary.
        return target.Length <= 262144 ? 0 : 6;
    }
}
