using System.Globalization;
using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Pertamina.Oto.Desktop.App;
using Pertamina.Oto.LocalPdf;

var root = Path.Combine(Path.GetTempPath(), "oto-client-test-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
var source = Path.Combine(root, "synthetic.pdf");
// Minimal fixture with explicit xref offsets; contains only synthetic test content.
var pdf = new StringBuilder("%PDF-1.4\n");
var offsets = new List<int> { 0 };
string[] objects =
[
    "<< /Type /Catalog /Pages 2 0 R >>",
    "<< /Type /Pages /Kids [3 0 R 4 0 R 5 0 R] /Count 3 >>",
    "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 400 600] /Resources <<>> >>",
    "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 410 600] /Resources <<>> >>",
    "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 420 600] /Resources <<>> >>"
];
for (var i = 0; i < objects.Length; i++)
{
    offsets.Add(pdf.Length);
    pdf.Append(CultureInfo.InvariantCulture, $"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
}
var xref = pdf.Length;
pdf.Append(CultureInfo.InvariantCulture, $"xref\n0 {offsets.Count}\n0000000000 65535 f \n");
foreach (var offset in offsets.Skip(1)) pdf.Append(CultureInfo.InvariantCulture, $"{offset:D10} 00000 n \n");
pdf.Append(CultureInfo.InvariantCulture, $"trailer\n<< /Size {offsets.Count} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
await File.WriteAllTextAsync(source, pdf.ToString(), Encoding.ASCII);
var sourceHash = SHA256.HashData(await File.ReadAllBytesAsync(source));
var assertions = new List<string>();
void Check(bool condition, string label)
{
    if (!condition) throw new InvalidOperationException("FAIL: " + label);
    assertions.Add(label); Console.WriteLine("PASS: " + label);
}
string Output(string tool) => Path.Combine(root, tool + ".pdf");
async Task<PdfReply> Run(PdfRequest request) => await WorkerClient.RunAsync(request, CancellationToken.None);

foreach (var tool in new[] { "merge", "extract", "remove", "organize", "rotate", "crop", "numbers", "watermark", "compress", "protect" })
{
    var inputs = tool == "merge" ? new[] { source, source } : new[] { source };
    var reply = await Run(new(tool, inputs, Output(tool), tool == "remove" ? "2" : "", "SYNTHETIC", NewPassword: "synthetic-password"));
    Check(reply.Success && File.Exists(Output(tool)), "desktop client → worker → atomic save: " + tool + "; " + reply.Message);
}
var unlocked = await Run(new("unlock", [Output("protect")], Output("unlock"), Password: "synthetic-password"));
Check(unlocked.Success, "unlock through process boundary");
var denied = await Run(new("unlock", [Output("protect")], Output("wrong-password"), Password: "wrong"));
Check(!denied.Success && !File.Exists(Output("wrong-password")), "wrong password produces no published result");
var split = await Run(new("split", [source], Path.Combine(root, "split.zip")));
Check(split.Success && split.Pages == 3, "split publishes ZIP");
var png = Path.Combine(root, "pixel.png");
await File.WriteAllBytesAsync(png, MakePng());
var image = await Run(new("images", [png], Output("image")));
Check(image.Success && image.Pages == 1, "PNG image → PDF through process boundary; " + image.Message);
var conflictHash = SHA256.HashData(await File.ReadAllBytesAsync(Output("merge")));
var conflict = await Run(new("merge", [source, source], Output("merge")));
var conflictAfter = SHA256.HashData(await File.ReadAllBytesAsync(Output("merge")));
Check(!conflict.Success && conflictHash.SequenceEqual(conflictAfter), "existing target unchanged");
var alias = await Run(new("merge", [source, source], source));
Check(!alias.Success, "source target rejected");
var removeAll = await Run(new("remove", [source], Output("remove-all"), "1-3"));
Check(!removeAll.Success && !File.Exists(Output("remove-all")), "empty result rejected");
using (var cancellation = new CancellationTokenSource())
{
    cancellation.Cancel();
    var cancelled = await WorkerClient.RunAsync(new("merge", [source, source], Output("cancelled")), cancellation.Token);
    Check(!cancelled.Success && !File.Exists(Output("cancelled")), "pre-start cancellation leaves no output");
}
var invalid = Path.Combine(root, "invalid.pdf");
await File.WriteAllTextAsync(invalid, "This is not a PDF. Synthetic test only.");
var malformed = await Run(new("compress", [invalid], Output("malformed")));
Check(!malformed.Success && !File.Exists(Output("malformed")), "malformed input rejected without publishing");
Check(!Directory.EnumerateFiles(root, ".oto-*.tmp").Any(), "no staging files remain");
var sourceAfter = SHA256.HashData(await File.ReadAllBytesAsync(source));
Check(sourceHash.SequenceEqual(sourceAfter), "original source SHA256 unchanged");
await File.WriteAllTextAsync(Path.Combine(root, "results.json"), JsonSerializer.Serialize(new { Checks = assertions, SourceUnchanged = true }, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{assertions.Count} integration checks passed. Evidence: {root}"));

static byte[] MakePng()
{
    using var png = new MemoryStream();
    png.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
    void Chunk(string name, byte[] data)
    {
        var length = new byte[4]; BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        png.Write(length);
        var type = Encoding.ASCII.GetBytes(name); png.Write(type); png.Write(data);
        uint crc = uint.MaxValue;
        foreach (var value in type.Concat(data))
        {
            crc ^= value;
            for (var i = 0; i < 8; i++) crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xEDB88320u : 0);
        }
        BinaryPrimitives.WriteUInt32BigEndian(length, ~crc); png.Write(length);
    }
    var header = new byte[13];
    BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(0, 4), 32);
    BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4, 4), 32);
    header[8] = 8; header[9] = 2;
    Chunk("IHDR", header);
    using var compressed = new MemoryStream();
    using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, true))
    {
        for (var y = 0; y < 32; y++)
        {
            zlib.WriteByte(0);
            for (var x = 0; x < 32; x++) zlib.Write(new byte[] { 220, 30, 30 });
        }
    }
    Chunk("IDAT", compressed.ToArray()); Chunk("IEND", []);
    return png.ToArray();
}
