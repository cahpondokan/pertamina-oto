namespace Pertamina.Oto.LocalPdf;

// Local stdin/stdout protocol only. Never persist requests: they can contain passwords.
internal sealed record PdfRequest(string Tool, string[] Inputs, string Output,
    string Pages = "", string Text = "", int Rotation = 90, double Margin = 20,
    string Password = "", string NewPassword = "");
internal sealed record PdfReply(bool Success, string Message, int Pages = 0, long Bytes = 0);
