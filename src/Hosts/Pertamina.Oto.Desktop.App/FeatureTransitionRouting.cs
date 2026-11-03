using System.Globalization;
using System.IO.Compression;
using System.Xml;
using Windows.Data.Pdf;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Pertamina.Oto.Desktop.App;

internal sealed partial class MainWindow
{
    private const int FeatureTransitionMaximumFiles = 50;
    private const long FeatureTransitionMaximumBytes = 100L * 1024 * 1024;
    private bool featureTransitionRoutingActive;
    private bool featureTransitionOverlapNotice;

    // Metadata only. Never store passwords, document contents, or parser exceptions
    // in these objects; the UI renders Problem as a per-document rejection reason.
    private sealed record FeatureTransitionInspection(string Path, string Detail,
        long Bytes = 0, int? Pages = null, string? Problem = null, string? Warning = null);

    private static bool FeatureTransitionAllowsMultiple(ToolCard tool) => tool.Id is "merge" or "organize" or "images";

    private static bool FeatureTransitionSupportsPassword(ToolCard tool) => tool.Id is
        "merge" or "split" or "remove" or "extract" or "compress" or "pdf-jpg" or "rotate"
        or "numbers" or "watermark" or "crop" or "unlock" or "protect";

    private static bool FeatureTransitionCompatible(ToolCard tool, string path)
    {
        if (!tool.Available) return false;
        var extension = Path.GetExtension(path);
        return tool.Id switch
        {
            "images" => extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".png", StringComparison.OrdinalIgnoreCase),
            "word-pdf" => extension.Equals(".docx", StringComparison.OrdinalIgnoreCase),
            _ => extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)
        };
    }

    private static string FeatureTransitionExpectedFormat(ToolCard tool) => tool.Id switch
    {
        "images" => "Fitur ini menerima JPG atau PNG.",
        "word-pdf" => "Fitur ini menerima satu DOCX. DOC dan DOCM belum didukung.",
        _ => "Fitur ini menerima PDF."
    };

    private static string FeatureTransitionFileName(string path)
    {
        try { return string.IsNullOrWhiteSpace(path) ? "Dokumen tanpa lokasi" : Path.GetFileName(path); }
        catch (ArgumentException) { return "Lokasi dokumen tidak valid"; }
    }

    private static bool FeatureTransitionNeedsSelection(IReadOnlyList<FeatureTransitionInspection> inputs) =>
        inputs.Count != 1 || inputs[0].Problem is not null || inputs[0].Warning is not null;

    private static string FeatureTransitionDocumentIdentity(FeatureTransitionOrigin origin, string path) =>
        string.Equals(origin.SavedResult, path, StringComparison.OrdinalIgnoreCase)
            ? "Hasil tersimpan" : "Sumber asli · " + origin.Label;

    private static string FeatureTransitionSourceContext(FeatureTransitionOrigin origin, string[] inputs)
    {
        var saved = inputs.Length == 1 && string.Equals(inputs[0], origin.SavedResult, StringComparison.OrdinalIgnoreCase);
        return saved ? "Hasil tersimpan dari " + origin.Label : "Dokumen sumber dari " + origin.Label;
    }

    private static string FeatureTransitionSelectionNotice(ToolCard tool, IReadOnlyList<FeatureTransitionInspection> inputs, FeatureTransitionOrigin origin)
    {
        var cardinality = FeatureTransitionAllowsMultiple(tool)
            ? "Centang dokumen yang akan dibuka. Urutan mengikuti daftar sumber."
            : "Pilih satu dokumen yang akan dibuka.";
        var rejected = inputs.Count(item => item.Problem is not null);
        var identity = inputs.Count == 1 && string.Equals(inputs[0].Path, origin.SavedResult, StringComparison.OrdinalIgnoreCase)
            ? "Yang dibuka adalah file hasil yang sudah tersimpan. "
            : origin.ToolId == "organize" ? "Yang dibuka adalah file sumber, bukan hasil susunan halaman. "
            : "Yang dibuka adalah file sumber; perubahan draf tidak ikut. ";
        return identity + cardinality + " Dokumen yang tidak dipilih tetap tersedia. "
            + (rejected > 0 ? $"{rejected} dokumen belum dapat dipakai; alasan tercantum pada masing-masing dokumen. " : "")
            + (tool.Id == "word-pdf" ? "Pemeriksaan ini tidak menjalankan Word. Provider dan struktur lengkap diperiksa saat konversi. " : "")
            + "File asli tidak diubah.";
    }

    private static bool TryResolveFeatureTransitionSelection(IReadOnlyList<TransitionDocument> offered,
        string[] chosen, bool multiple, out string[] paths)
    {
        paths = [];
        if (chosen.Length == 0 || chosen.Length > FeatureTransitionMaximumFiles || !multiple && chosen.Length != 1) return false;
        var selectedPaths = new HashSet<string>(chosen, StringComparer.OrdinalIgnoreCase);
        if (selectedPaths.Count != chosen.Length) return false;
        // Return the offered originals, in source order, never UI labels or a newly
        // supplied path. Reject an ambiguous duplicate instead of choosing one.
        foreach (var path in selectedPaths)
            if (offered.Count(item => item.Problem is null && string.Equals(item.Path, path, StringComparison.OrdinalIgnoreCase)) != 1)
                return false;
        paths = offered.Where(item => item.Problem is null && selectedPaths.Contains(item.Path)).Select(item => item.Path).ToArray();
        return paths.Length == chosen.Length;
    }

    private async Task<bool> FeatureTransitionRoutingMayContinueAsync(ToolCard tool)
    {
        if (!FeatureTransitionBusy && !messageOpen) return true;
        await RefuseFeatureTransitionRoutingAsync(tool);
        return false;
    }

    private bool FeatureTransitionRoutingContextCurrent(int screen, IReadOnlyList<string> home, IReadOnlyList<string> chosen) =>
        screenIndex == screen && homeFiles.SequenceEqual(home, StringComparer.OrdinalIgnoreCase)
        && chosenFiles.SetEquals(chosen);

    private async Task RefuseFeatureTransitionRoutingAsync(ToolCard tool)
    {
        // Do not compete with the active modal; keep at most one queued refusal.
        while (messageOpen && root.IsLoaded) await Task.Delay(50);
        if (!root.IsLoaded) return;
        try
        {
            await ChooseTransitionActionAsync(tool, "Perpindahan fitur belum tersedia",
                "Tunggu dialog atau pekerjaan yang sedang berjalan selesai, atau batalkan pekerjaan tersebut sebelum membuka fitur lagi.",
                [new("return", "Kembali", "Lanjutkan pekerjaan yang sedang terbuka.")]);
        }
        catch (Exception error)
        {
            LocalDiagnostics.Record("feature-transition-busy-dialog", error);
            status.Text = "Perpindahan fitur ditunda. Tunggu pekerjaan atau dialog yang sedang terbuka selesai.";
        }
    }

    private static string? FeatureTransitionSelectionProblem(IReadOnlyList<FeatureTransitionInspection> inputs)
    {
        if (inputs.Any(item => item.Problem is not null)) return "Dokumen yang dipilih berubah atau belum dapat dibaca. Periksa alasan pada daftar berikut.";
        if (inputs.Count > FeatureTransitionMaximumFiles) return "Pilih maksimal 50 file. Pilihan tidak dikurangi otomatis.";
        if (inputs.Sum(item => item.Bytes) > FeatureTransitionMaximumBytes) return "Total pilihan melebihi 100 MB. Kembali ke sumber dan kurangi pilihan.";
        if (inputs.Sum(item => item.Pages ?? 0) > 1000) return "Total pilihan melebihi 1000 halaman. Kembali ke sumber dan kurangi pilihan.";
        return null;
    }

    private async Task ShowFeatureTransitionRoutingFailureAsync(ToolCard tool,
        IReadOnlyList<FeatureTransitionInspection> inputs, string reason)
    {
        // This dialog is informational: all rows are disabled because the transfer
        // must restart and revalidate before any destination decision is made.
        var documents = inputs.Select((item, index) => new TransitionDocument(item.Path,
            FeatureTransitionFileName(item.Path), item.Detail, OrganizerSourceCode(index), index,
            item.Problem ?? reason)).ToArray();
        await ChooseTransitionDocumentsAsync(tool, documents, "Dokumen belum dapat dipindahkan", reason, FeatureTransitionAllowsMultiple(tool));
    }

    private static async Task<FeatureTransitionInspection[]> InspectFeatureTransitionInputsAsync(
        ToolCard tool, string[] inputs, List<FileStream>? retainedLeases = null)
    {
        if (inputs.Length > FeatureTransitionMaximumFiles) throw new ArgumentOutOfRangeException(nameof(inputs));
        using var budget = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var result = new List<FeatureTransitionInspection>(inputs.Length);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in inputs)
        {
            if (!seen.Add(path))
            {
                result.Add(new(path, "Dokumen duplikat", Problem: "Lokasi ini muncul lebih dari sekali. Perbaiki daftar sumber sebelum melanjutkan."));
                continue;
            }
            using var perFile = CancellationTokenSource.CreateLinkedTokenSource(budget.Token);
            perFile.CancelAfter(TimeSpan.FromSeconds(4));
            // Storage/PDF metadata stays off the UI STA. Work is serial and bounded
            // by 50 files / 100 MB each, with no render, worker, upload, or write.
            var inspected = await Task.Run(() => InspectFeatureTransitionInputAsync(tool, path, retainedLeases is not null, perFile.Token));
            result.Add(inspected.Item);
            if (inspected.Lease is not null) retainedLeases!.Add(inspected.Lease);
        }
        return result.ToArray();
    }

    private static async Task<(FeatureTransitionInspection Item, FileStream? Lease)> InspectFeatureTransitionInputAsync(
        ToolCard tool, string path, bool retainLease, CancellationToken token)
    {
        FileStream? lease = null;
        var detail = "Belum dapat diperiksa";
        long bytes = 0;
        FeatureTransitionInspection Reject(string reason) => new(path, detail, bytes, Problem: reason);
        try
        {
            token.ThrowIfCancellationRequested();
            if (!FeatureTransitionCompatible(tool, path)) return (Reject(FeatureTransitionExpectedFormat(tool)), null);
            if (!Path.IsPathFullyQualified(path) || path.StartsWith(@"\\", StringComparison.Ordinal)
                || path.Length < 3 || path[1] != ':' || path.IndexOf(':', 2) >= 0)
                return (Reject("Gunakan file pada drive lokal, bukan lokasi jaringan, perangkat khusus, atau tautan."), null);
            if (!WordPdfClient.IsLocalPlainPath(path))
                return (Reject("Lokasi jaringan atau tautan file/folder tidak didukung. Pilih salinan lokal."), null);
            token.ThrowIfCancellationRequested();
            // No write/delete sharing: selected source identities stay stable until
            // the destination has either committed or declined the transition.
            lease = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            bytes = lease.Length;
            detail = (bytes / (1024d * 1024)).ToString("0.##", CultureInfo.CurrentCulture) + " MB";
            if (bytes == 0) return (Reject("File kosong tidak dapat dibuka."), null);
            if (bytes > FeatureTransitionMaximumBytes) return (Reject("Ukuran file melebihi batas 100 MB."), null);
            FeatureTransitionInspection item;
            if (tool.Id == "word-pdf")
            {
                InspectFeatureTransitionDocx(lease, token);
                item = new(path, "DOCX · " + detail + " · Struktur awal terbaca; validasi lengkap saat konversi", bytes);
            }
            else
            {
                var storage = await StorageFile.GetFileFromPathAsync(path).AsTask(token);
                if (tool.Id == "images")
                {
                    using var stream = await storage.OpenReadAsync().AsTask(token);
                    var image = await BitmapDecoder.CreateAsync(stream).AsTask(token);
                    var extension = Path.GetExtension(path);
                    var expected = extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ? BitmapDecoder.PngDecoderId : BitmapDecoder.JpegDecoderId;
                    if (image.DecoderInformation.CodecId != expected) return (Reject("Isi gambar tidak sesuai dengan format JPG/PNG pada nama file."), null);
                    if (image.PixelWidth == 0 || image.PixelHeight == 0 || (long)image.PixelWidth * image.PixelHeight > 40_000_000)
                        return (Reject("Gambar kosong atau melebihi batas 40 megapiksel."), null);
                    item = new(path, $"{image.PixelWidth} × {image.PixelHeight} piksel · {detail}", bytes, 1);
                }
                else
                {
                    try
                    {
                        using var input = await storage.OpenReadAsync().AsTask(token);
                        var pdf = await PdfDocument.LoadFromStreamAsync(input).AsTask(token);
                        if (pdf.PageCount is 0 or > 1000) return (Reject("PDF kosong atau melebihi batas 1000 halaman."), null);
                        if (pdf.IsPasswordProtected && !FeatureTransitionSupportsPassword(tool))
                            return (Reject("PDF dilindungi kata sandi. Buka melalui Unlock PDF dengan kata sandi yang sah terlebih dahulu."), null);
                        var warning = pdf.IsPasswordProtected ? "PDF dilindungi. Kata sandi yang sah mungkin diperlukan di fitur tujuan sebelum memproses." : null;
                        item = new(path, $"{pdf.PageCount} halaman · {detail}" + (warning is null ? "" : " · " + warning), bytes, (int)pdf.PageCount, Warning: warning);
                    }
                    catch (Exception error) when (error.HResult is unchecked((int)0x80048040) or unchecked((int)0x8007052B))
                    {
                        if (!FeatureTransitionSupportsPassword(tool)) return (Reject("PDF meminta kata sandi. Gunakan Unlock PDF dengan kata sandi yang sah sebelum berpindah ke fitur ini."), null);
                        const string warning = "PDF meminta kata sandi; jumlah halaman belum terverifikasi. Masukkan kata sandi yang sah di fitur tujuan. Batas total 1000 halaman diperiksa sebelum hasil disimpan.";
                        item = new(path, detail + " · " + warning, bytes, Warning: warning);
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            var retained = retainLease ? lease : null;
            if (retainLease) lease = null;
            return (item, retained);
        }
        catch (FileNotFoundException) { return (Reject("File tidak ditemukan. File mungkin dipindahkan atau dihapus."), null); }
        catch (DirectoryNotFoundException) { return (Reject("Folder sumber tidak ditemukan atau drive sudah dilepas."), null); }
        catch (UnauthorizedAccessException) { return (Reject("Akses baca ditolak. Periksa izin file atau pilih salinan yang dapat dibaca."), null); }
        catch (OperationCanceledException) { return (Reject("Pemeriksaan belum selesai dalam batas waktu. Coba kembali setelah drive siap."), null); }
        catch (InvalidDataException) { return (Reject("Struktur file rusak atau belum didukung. Pilih dokumen lain."), null); }
        catch (XmlException) { return (Reject("Struktur DOCX rusak atau tidak dapat dibaca dengan aman."), null); }
        catch (IOException error) when ((error.HResult & 0xffff) is 32 or 33)
        { return (Reject("File sedang dikunci aplikasi lain. Tutup akses yang mengunci lalu coba kembali."), null); }
        catch (Exception error)
        {
            LocalDiagnostics.Record("feature-transition-preflight", error);
            return (Reject("File tidak dapat dibaca. File mungkin rusak, akses dicabut, atau formatnya tidak didukung."), null);
        }
        finally { lease?.Dispose(); }
    }

    private static void InspectFeatureTransitionDocx(Stream source, CancellationToken token)
    {
        // Lightweight package/readability preflight only. SafeDocx and the existing
        // isolated Word worker remain the authority for conversion admission.
        using var zip = new ZipArchive(source, ZipArchiveMode.Read, leaveOpen: true);
        if (zip.Entries.Count is < 3 or > 2048) throw new InvalidDataException();
        long expanded = 0;
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in zip.Entries)
        {
            token.ThrowIfCancellationRequested();
            expanded += entry.Length;
            if (!names.Add(entry.FullName) || entry.Length > 20L * 1024 * 1024 || expanded > FeatureTransitionMaximumBytes
                || entry.Length > Math.Max(1024L * 1024, entry.CompressedLength * 200)) throw new InvalidDataException();
        }
        foreach (var name in new[] { "[Content_Types].xml", "_rels/.rels", "word/document.xml" })
        {
            var entry = zip.GetEntry(name) ?? throw new InvalidDataException();
            using var input = entry.Open();
            using var reader = XmlReader.Create(input, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 20L * 1024 * 1024
            });
            var expected = name == "word/document.xml" ? "document" : name == "_rels/.rels" ? "Relationships" : "Types";
            var expectedNamespace = name == "word/document.xml" ? "http://schemas.openxmlformats.org/wordprocessingml/2006/main"
                : name == "_rels/.rels" ? "http://schemas.openxmlformats.org/package/2006/relationships"
                : "http://schemas.openxmlformats.org/package/2006/content-types";
            if (reader.MoveToContent() != XmlNodeType.Element || reader.LocalName != expected || reader.NamespaceURI != expectedNamespace)
                throw new InvalidDataException();
            while (reader.Read()) token.ThrowIfCancellationRequested();
        }
    }
}
