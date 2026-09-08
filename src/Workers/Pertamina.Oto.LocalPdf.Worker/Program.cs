using System.Text.Json;
using Pertamina.Oto.LocalPdf;

// One process per operation. The desktop owns timeout, cancellation and publication.
if (args is ["--self-test"])
    return WorkerTests.Run();

try
{
    var line = await Console.In.ReadLineAsync();
    if (line is null || line.Length > 65536) throw new InvalidOperationException();
    var request = JsonSerializer.Deserialize<PdfRequest>(line)
        ?? throw new InvalidOperationException();
    var reply = PdfOperations.Execute(request);
    Console.WriteLine(JsonSerializer.Serialize(reply));
    return 0;
}
catch (InputException error)
{
    Console.WriteLine(JsonSerializer.Serialize(new PdfReply(false, error.Message)));
    return 2;
}
catch (Exception)
{
    // Engine exceptions may contain paths, content or passwords. Do not forward them.
    Console.WriteLine(JsonSerializer.Serialize(new PdfReply(false,
        "Tidak dapat memproses dokumen. Periksa format, kata sandi pemilik, dan kondisi file. Sumber tidak diubah.")));
    return 3;
}
