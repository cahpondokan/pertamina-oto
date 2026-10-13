using PdfSharp.Pdf;
using PdfSharp.Pdf.Content.Objects;

namespace Pertamina.Oto.LocalPdf;

// Read-only PDF text state. Unsupported metrics/transforms invalidate position, not text.
internal sealed class ReaderSearchGeometry(PdfPage page)
{
    private sealed record Clip(double Left, double Bottom, double Right, double Top);
    private sealed record State(double A = 1, double D = 1, double E = 0, double F = 0,
        string Font = "", double Size = 0, double Spacing = 0, double Word = 0,
        double Scale = 1, double Rise = 0, double Leading = 0, bool Safe = true, Clip? Bounds = null);
    private State state = new();
    private readonly Stack<State> stack = new();
    private double x, y, lineX, lineY, a = 1, d = 1;
    private bool positioned = true, matrixSafe = true;
    private Clip? rectangle;
    private bool pathPresent;
    private bool pendingClip;

    internal void Operator(COperator op)
    {
        var args = op.Operands;
        double N(int i) => i < args.Count ? args[i] switch { CInteger n => n.Value, CReal n => n.Value, _ => double.NaN } : double.NaN;
        void Line() { lineY -= d * state.Leading; x = lineX; y = lineY; positioned = matrixSafe; }
        switch (op.Name)
        {
            case "q": stack.Push(state); break;
            case "Q": state = stack.Count > 0 ? stack.Pop() : new(Safe: false); break;
            case "cm": state = state with { A = state.A * N(0), D = state.D * N(3), E = state.E + state.A * N(4), F = state.F + state.D * N(5), Safe = state.Safe && N(1) == 0 && N(2) == 0 }; break;
            case "BT": x = y = lineX = lineY = 0; a = d = 1; positioned = matrixSafe = true; break;
            case "Tf": state = state with { Font = args[0] is CName name ? name.Name : "", Size = N(1) }; break;
            case "Tc": state = state with { Spacing = N(0) }; break;
            case "Tw": state = state with { Word = N(0) }; break;
            case "Tz": state = state with { Scale = N(0) / 100 }; break;
            case "Ts": state = state with { Rise = N(0) }; break;
            case "TL": state = state with { Leading = N(0) }; break;
            case "Tm": a = N(0); d = N(3); x = lineX = N(4); y = lineY = N(5); positioned = matrixSafe = N(1) == 0 && N(2) == 0; break;
            case "Td": case "TD":
                if (op.Name == "TD") state = state with { Leading = -N(1) };
                lineX += a * N(0); lineY += d * N(1); x = lineX; y = lineY; positioned = matrixSafe; break;
            case "T*": case "'": Line(); break;
            case "\"": state = state with { Word = N(0), Spacing = N(1) }; Line(); break;
            case "gs": state = state with { Safe = state.Safe && GraphicsState(op).Safe }; break;
            case "re":
                if (!pathPresent && state.Safe && state.A > 0 && state.D > 0 && args.Count == 4 && new[] { N(0), N(1), N(2), N(3) }.All(double.IsFinite) && N(2) > 0 && N(3) > 0)
                    rectangle = new(state.E + state.A * N(0), state.F + state.D * N(1), state.E + state.A * (N(0) + N(2)), state.F + state.D * (N(1) + N(3)));
                else rectangle = null;
                pathPresent = true; break;
            case "m": case "l": case "c": case "v": case "y": case "h":
                rectangle = null; pathPresent = true; break;
            case "W": case "W*":
                pendingClip = true;
                break;
            case "n": case "S": case "s": case "f": case "F": case "f*": case "B": case "B*": case "b": case "b*":
                // The current path is not part of q/Q state; clipping commits at path end.
                if (pendingClip)
                {
                    if (rectangle is null) state = state with { Safe = false };
                    else state = state with { Bounds = state.Bounds is { } clip ? new(Math.Max(clip.Left, rectangle.Left), Math.Max(clip.Bottom, rectangle.Bottom), Math.Min(clip.Right, rectangle.Right), Math.Min(clip.Top, rectangle.Top)) : rectangle };
                }
                rectangle = null; pathPresent = pendingClip = false; break;
            case "Tr": if (N(0) != 0) state = state with { Safe = false }; break;
        }
    }

    // Deliberately narrow admission. A benign gs never repairs an inherited unsafe state.
    // Dictionary access resolves indirect resources through PDFsharp's resource collection.
    internal (bool Safe, bool FontUnknown) GraphicsState(COperator op)
    {
        try
        {
            if (op.Operands.Count != 1 || op.Operands[0] is not CName name ||
                page.Resources.Elements.GetDictionary("/ExtGState")?.Elements.GetDictionary(name.Name) is not { } dictionary)
                return (false, true);
            if (dictionary.Stream is not null) return (false, true);
            // Admission allows at most these four keys, so work per gs stays constant.
            if (dictionary.Elements.Count > 4) return (false, dictionary.Elements.ContainsKey("/Font"));
            var safe = true;
            foreach (var key in dictionary.Elements.Keys)
            {
                var item = dictionary.Elements[key];
                if (item is PdfSharp.Pdf.Advanced.PdfReference reference) item = reference.Value;
                safe &= key switch
                {
                    "/Type" => item is PdfName type && type.Value == "/ExtGState",
                    "/BM" => item is PdfName blend && blend.Value == "/Normal",
                    "/CA" or "/ca" => item is PdfInteger integer ? integer.Value == 1 : item is PdfReal real && real.Value == 1,
                    _ => false
                };
            }
            return (safe, dictionary.Elements.ContainsKey("/Font"));
        }
        catch (Exception) { return (false, true); }
    }

    internal void Adjust(double value) => x -= a * value / 1000 * state.Size * state.Scale;
    internal void UnknownAdvance() => positioned = false;

    internal PdfSearchBox? Glyph(string raw)
    {
        var font = page.Resources.Elements.GetDictionary("/Font")?.Elements.GetDictionary(state.Font);
        var width = font is null ? null : Width(font, raw);
        if (width is null) { positioned = false; return null; }
        var advance = width.Value / 1000 * state.Size;
        var descriptor = font!.Elements.GetDictionary("/FontDescriptor");
        var name = font.Elements.GetName("/BaseFont");
        var ascent = descriptor?.Elements.ContainsKey("/Ascent") == true ? descriptor.Elements.GetReal("/Ascent") / 1000
            : name.StartsWith("/Helvetica", StringComparison.Ordinal) ? .718 : name.StartsWith("/Times", StringComparison.Ordinal) ? .683 : .85;
        var descent = descriptor?.Elements.ContainsKey("/Descent") == true ? descriptor.Elements.GetReal("/Descent") / 1000
            : name.StartsWith("/Helvetica", StringComparison.Ordinal) ? -.207 : name.StartsWith("/Times", StringComparison.Ordinal) ? -.217 : -.22;
        var left = state.E + state.A * x;
        var bottom = state.F + state.D * (y + d * (state.Rise + state.Size * descent));
        var right = left + state.A * a * advance * state.Scale;
        var top = state.F + state.D * (y + d * (state.Rise + state.Size * ascent));
        x += a * (advance + state.Spacing + (raw == " " ? state.Word : 0)) * state.Scale;
        if (!positioned || !state.Safe || pendingClip || state.Size <= 0 || state.Scale <= 0 || a <= 0 || d <= 0 || state.A <= 0 || state.D <= 0) return null;
        // Only wholly contained glyphs are proven visible. Never invent a clipped text box.
        if (state.Bounds is { } clip && (left < clip.Left || right > clip.Right || bottom < clip.Bottom || top > clip.Top)) return null;
        var crop = page.CropBox;
        if (crop.Width <= 0 || crop.Height <= 0) crop = page.MediaBox;
        var w = crop.Width; var h = crop.Height;
        if (w <= 0 || h <= 0) return null;
        left -= crop.X1; right -= crop.X1; bottom -= crop.Y1; top -= crop.Y1;
        // A rotated displayed page uses the same clockwise transform as Windows.Data.Pdf.
        var rotation = ((page.Rotate % 360) + 360) % 360;
        var points = new[] { (left, bottom), (left, top), (right, bottom), (right, top) }.Select(p => rotation switch
        {
            0 => (p.Item1 / w, 1 - p.Item2 / h),
            90 => (p.Item2 / h, p.Item1 / w),
            180 => (1 - p.Item1 / w, p.Item2 / h),
            270 => (1 - p.Item2 / h, 1 - p.Item1 / w),
            _ => (double.NaN, double.NaN)
        }).ToArray();
        var minX = points.Min(p => p.Item1); var maxX = points.Max(p => p.Item1);
        var minY = points.Min(p => p.Item2); var maxY = points.Max(p => p.Item2);
        if (!new[] { minX, maxX, minY, maxY }.All(double.IsFinite) || minX < 0 || minY < 0 || maxX > 1 || maxY > 1 || maxX <= minX || maxY <= minY) return null;
        return new(minX, minY, maxX - minX, maxY - minY);
    }

    private static double? Width(PdfDictionary font, string raw)
    {
        var code = raw.Aggregate(0, (value, c) => value * 256 + c);
        if (font.Elements.GetName("/Subtype") == "/Type0")
        {
            // ToUnicode is not a CID mapping. Only Identity-H proves code == CID.
            if (font.Elements.GetName("/Encoding") != "/Identity-H") return null;
            var descendant = font.Elements.GetArray("/DescendantFonts")?.Elements.GetDictionary(0);
            if (descendant is null) return null;
            var widths = descendant.Elements.GetArray("/W");
            if (widths is not null)
                for (var i = 0; i < widths.Elements.Count;)
                {
                    var first = widths.Elements.GetInteger(i++);
                    if (i >= widths.Elements.Count) return null;
                    if (widths.Elements[i] is PdfArray values)
                    { i++; if (code >= first && code - first < values.Elements.Count) return Valid(values.Elements.GetReal(code - first)); }
                    else
                    {
                        var last = widths.Elements.GetInteger(i++);
                        if (i >= widths.Elements.Count) return null;
                        var value = widths.Elements.GetReal(i++); if (code >= first && code <= last) return Valid(value);
                    }
                }
            return Valid(descendant.Elements.ContainsKey("/DW") ? descendant.Elements.GetReal("/DW") : 1000);
        }
        var simple = font.Elements.GetArray("/Widths"); var index = code - font.Elements.GetInteger("/FirstChar");
        if (simple is not null) return index >= 0 && index < simple.Elements.Count ? Valid(simple.Elements.GetReal(index)) : null;
        if (font.Elements.ContainsKey("/FontDescriptor")) return null;
        return PdfOperations.SearchStandardWidth(font.Elements.GetName("/BaseFont").TrimStart('/'), code);
    }
    private static double? Valid(double value) => double.IsFinite(value) && value is > 0 and <= 4000 ? value : null;
}
