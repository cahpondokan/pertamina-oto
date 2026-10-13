using System.Runtime.InteropServices;

namespace Pertamina.Oto.Desktop.App;

internal static class ResultFileReveal
{
    // Called on the WinUI STA thread, whose COM apartment is already initialized.
    internal static void Select(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("Saved result is missing.");
        var parsed = SHParseDisplayName(fullPath, IntPtr.Zero, out var item, 0, out _);
        try
        {
            Marshal.ThrowExceptionForHR(parsed);
            if (item == IntPtr.Zero) throw new InvalidOperationException("Windows could not identify the saved result.");
            // Zero child count selects this exact absolute item in its parent folder.
            Marshal.ThrowExceptionForHR(SHOpenFolderAndSelectItems(item, 0, IntPtr.Zero, 0));
        }
        finally { if (item != IntPtr.Zero) Marshal.FreeCoTaskMem(item); }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int SHParseDisplayName(string name, IntPtr context, out IntPtr item, uint attributes, out uint foundAttributes);

    [DllImport("shell32.dll", ExactSpelling = true)]
    private static extern int SHOpenFolderAndSelectItems(IntPtr item, uint count, IntPtr children, uint flags);
}
