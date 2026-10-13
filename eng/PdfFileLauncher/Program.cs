using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Pertamina.Oto.Desktop.App;
using Pertamina.Oto.PdfOpener;

[assembly: SupportedOSPlatform("windows")]

// Test invocation errors must never masquerade as a user's PDF failure.
if (args.Length > 0 && args[0] == "--self-test")
{
    if (args.Length != 2 || string.IsNullOrWhiteSpace(args[1])) return 2;
    try { return PdfAssociationTests.Run(Path.GetFullPath(args[1])); }
    catch (Exception) { return 2; }
}
var stage = "input";
try
{
    var command = PdfOpenCommand.Create(LocalAppUpdate.InstallRoot, args, value => stage = value);
    stage = "launch";
    using var child = Process.Start(command);
    if (child is null) throw new InvalidOperationException();
    return 0;
}
catch (Exception error)
{
    PdfOpenFailure.Record(stage, error);
    _ = NativeMessage.Show(IntPtr.Zero, PdfOpenFailure.Message(stage, error), "PERTAMINA OTO", 0x10);
    return 1;
}

internal static class NativeMessage
{
    [DllImport("user32.dll", EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode, ExactSpelling = true)]
    internal static extern int Show(IntPtr parent, string message, string title, uint type);
}
