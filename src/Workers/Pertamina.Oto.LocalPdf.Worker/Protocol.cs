namespace Pertamina.Oto.LocalPdf;

// Local stdin/stdout protocol only. Never persist requests: they can contain passwords.
internal sealed record PdfRequest(string Tool, string[] Inputs, string Output,
    string Pages = "", string Text = "", int Rotation = 90, double Margin = 20,
    string Password = "", string NewPassword = "", PagePlacement[]? Layout = null, int[]? FileRotations = null, EditorMark[]? Marks = null, string Quality = "original");
internal sealed record EditorPoint(double X, double Y);
internal sealed record EditorMark(int Page, string Kind, double X, double Y, double Width, double Height,
    string Text = "", double Size = 14, string Color = "#111827", EditorPoint[]? Points = null, int ImageIndex = 0, int SourceId = -1, string OriginalText = "", string LocalFont = "", bool FitText = false);
internal sealed record EditorTextRun(int Id, string Text, string Font, double X, double Y, double Width, double Height, double Size, string Characters, string MatchingLocalFont = "");
internal sealed record EditorPageSize(double Width, double Height);
internal sealed record PagePlacement(int Source, int Page, int Rotation = 0);
// Rectangles are normalized to the displayed crop/rotation, independent of zoom.
internal sealed record PdfSearchBox(double X, double Y, double Width, double Height);
internal sealed record PdfSearchHit(int Page, string Snippet, int Id = 0, int SnippetMatch = 0, int MatchLength = 0, PdfSearchBox[]? Boxes = null);
internal sealed record PdfSearchResult(PdfSearchHit[] Hits, int TextPages, int PartialPages, bool Truncated);
internal sealed record PdfReply(bool Success, string Message, int Pages = 0, long Bytes = 0, EditorPageSize[]? PageSizes = null, EditorTextRun[]? TextRuns = null, string[]? LocalFonts = null, PdfSearchResult? Search = null);
