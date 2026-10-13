using System.Diagnostics;
using System.Globalization;

namespace Pertamina.Oto.Desktop.App;

internal static class LocalDiagnostics
{
    private static readonly object Gate = new();
    internal static void Record(string operation, Exception? error)
    {
        try
        {
            lock (Gate)
            {
                var directory = Path.Combine(PortableProfile.DataRoot, "Logs");
                Directory.CreateDirectory(directory);
                var path = Path.Combine(directory, "errors.log");
                if (File.Exists(path) && new FileInfo(path).Length > 512 * 1024)
                    File.Move(path, Path.Combine(directory, "errors.previous.log"), true);
                // Never write exception messages, document paths, arguments or passwords.
                var frames = error is null ? "" : string.Join(" > ", new StackTrace(error, false).GetFrames().Take(16)
                    .Select(f => f.GetMethod()).Where(m => m is not null).Select(m => m!.DeclaringType?.FullName + "." + m.Name));
                var line = string.Join(" | ", DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture), operation,
                    typeof(LocalDiagnostics).Assembly.ManifestModule.ModuleVersionId.ToString(),
                    error?.GetType().FullName ?? "unknown", error?.HResult.ToString("X8", CultureInfo.InvariantCulture) ?? "", frames);
                File.AppendAllText(path, line + Environment.NewLine);
            }
        }
        catch (Exception) { /* Diagnostics must never cause another crash. */ }
    }
}
