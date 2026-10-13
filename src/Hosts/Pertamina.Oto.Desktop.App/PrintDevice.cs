using System.Runtime.InteropServices;

namespace Pertamina.Oto.Desktop.App;

// All driver settings are private DEVMODE buffers. Never SetPrinter/default-printer mutation.
internal sealed class PrintDevice : IDisposable
{
    internal sealed record Paper(short Id, string Name) { public override string ToString() => Name; }
    internal sealed record Bounds(int Width, int Height, int Left, int Top, int PrintableWidth, int PrintableHeight, int DpiX, int DpiY);
    internal string Name { get; }
    private IntPtr printer, mode; private int modeSize; private bool colorSelectable;
    internal List<Paper> Papers { get; } = [];
    internal bool Duplex { get; }
    internal bool Color => colorSelectable;
    internal bool Landscape { get; }
    internal short PaperId => Marshal.ReadInt16(mode, 78);
    internal short ColorMode => Marshal.ReadInt16(mode, 92);
    internal short DuplexMode => Marshal.ReadInt16(mode, 94);
    internal short Orientation => Marshal.ReadInt16(mode, 76);
    internal string Status { get; } = "Status Windows tidak tersedia · Keterjangkauan belum diverifikasi.";
    internal static string WindowsStatus(int? state) => (state is null ? "Status Windows tidak tersedia" : state == 0 ? "Siap menurut Windows (dapat berasal dari cache)" : (state & 0x80) != 0 ? "Offline menurut Windows" : "Printer membutuhkan perhatian menurut Windows") + " · Keterjangkauan belum diverifikasi.";
    private string ReadWindowsStatus()
    {
            GetPrinter(printer, 6, IntPtr.Zero, 0, out var size);
            if (size < 4 || size > 65536) return WindowsStatus(null);
            var buffer = Marshal.AllocHGlobal((int)size);
            try { if (!GetPrinter(printer, 6, buffer, size, out _)) return WindowsStatus(null);
                return WindowsStatus(Marshal.ReadInt32(buffer)); }
            finally { Marshal.FreeHGlobal(buffer); }
    }
    // DC_PAPERNAMES slots contain at most 64 UTF-16 characters; padding is unspecified.
    internal static string ParsePaperName(ReadOnlySpan<char> slot)
    {
        if (slot.Length != 64) throw new ArgumentException("Slot nama kertas harus 64 karakter.", nameof(slot));
        var terminator = slot.IndexOf('\0');
        return new string(terminator < 0 ? slot : slot[..terminator]);
    }
    internal static int PaperBufferBytes(int count)
    {
        if (count is < 1 or > 1000) throw new InvalidOperationException("Ukuran kertas driver tidak tersedia.");
        return checked(count * 128);
    }
    internal static string[] Names()
    {
        EnumPrinters(6, null, 4, IntPtr.Zero, 0, out var bytes, out _);
        if (bytes == 0) return [];
        if (bytes > 4 * 1024 * 1024) throw new InvalidOperationException("Daftar printer terlalu besar.");
        var buffer = Marshal.AllocHGlobal((int)bytes);
        try
        {
            if (!EnumPrinters(6, null, 4, buffer, bytes, out _, out var count)) throw new InvalidOperationException("Printer Windows tidak tersedia.");
            var stride = IntPtr.Size == 8 ? 24 : 12;
            return Enumerable.Range(0, checked((int)count)).Select(i => Marshal.PtrToStringUni(Marshal.ReadIntPtr(buffer, i * stride))!).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToArray();
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }
    internal PrintDevice(string name)
    {
        Name = name;
        try
        {
            if (!OpenPrinter(name, out printer, IntPtr.Zero)) throw new InvalidOperationException("Printer tidak dapat diakses.");
            var size = DocumentProperties(IntPtr.Zero, printer, name, IntPtr.Zero, IntPtr.Zero, 0);
            if (size < 220 || size > 1024 * 1024) throw new InvalidOperationException("Driver printer tidak didukung.");
            modeSize = size; mode = Marshal.AllocHGlobal(size);
            if (DocumentProperties(IntPtr.Zero, printer, name, mode, IntPtr.Zero, 2) != 1) throw new InvalidOperationException("Pengaturan printer tidak tersedia.");
            var count = Capability(2); var nameBytes = PaperBufferBytes(count);
            if (Capability(16) != count) throw new InvalidOperationException("Daftar kertas berubah.");
            var ids = Marshal.AllocHGlobal(count * 2); var names = Marshal.AllocHGlobal(nameBytes);
            try
            {
                Marshal.Copy(new byte[nameBytes], 0, names, nameBytes);
                if (DeviceCapabilities(name, null, 2, ids, mode) != count || DeviceCapabilities(name, null, 16, names, mode) != count) throw new InvalidOperationException("Daftar kertas berubah.");
                for (var i = 0; i < count; i++) Papers.Add(new(Marshal.ReadInt16(ids, i * 2), ParsePaperName(Marshal.PtrToStringUni(names + i * 128, 64)!.AsSpan())));
            }
            finally { Marshal.FreeHGlobal(ids); Marshal.FreeHGlobal(names); }
            colorSelectable = Capability(32) == 1 && SupportsColorMode(1) && SupportsColorMode(2);
            Duplex = Capability(7) == 1; Landscape = Capability(17) > 0;
            Status = ReadWindowsStatus(); // Retrieved with construction on the dialog's background task; no live reachability claim.
        }
        catch { Dispose(); throw; }
    }
    private int Capability(short kind) => DeviceCapabilities(Name, null, kind, IntPtr.Zero, mode);
    private bool SupportsColorMode(short value)
    {
        var buffer = Marshal.AllocHGlobal(modeSize);
        try
        {
            Marshal.Copy(ExportMode(), 0, buffer, modeSize);
            Marshal.WriteInt32(buffer, 72, Marshal.ReadInt32(buffer, 72) | 0x800); Marshal.WriteInt16(buffer, 92, value);
            return DocumentProperties(IntPtr.Zero, printer, Name, buffer, buffer, 10) == 1
                && (Marshal.ReadInt32(buffer, 72) & 0x800) != 0 && Marshal.ReadInt16(buffer, 92) == value;
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }
    internal void Preferences(IntPtr window)
    {
        if (DocumentProperties(window, printer, Name, mode, mode, 14) < 0) throw new InvalidOperationException("Preferensi driver gagal.");
    }
    internal void Configure(short paper, bool landscape, bool color, short duplex)
    {
        var fields = (Marshal.ReadInt32(mode, 72) & ~12) | 1 | 2 | 0x10 | 0x100; Marshal.WriteInt16(mode, 84, 100);
        Marshal.WriteInt16(mode, 76, (short)(landscape ? 2 : 1)); Marshal.WriteInt16(mode, 78, paper);
        Marshal.WriteInt16(mode, 86, 1); // software copies, no driver multiplication
        if (Color) { fields |= 0x800; Marshal.WriteInt16(mode, 92, (short)(color ? 2 : 1)); }
        if (Duplex) { fields |= 0x1000; Marshal.WriteInt16(mode, 94, duplex); }
        Marshal.WriteInt32(mode, 72, fields);
        if (DocumentProperties(IntPtr.Zero, printer, Name, mode, mode, 10) != 1) throw new InvalidOperationException("Driver menolak pengaturan.");
        if (PaperId != paper || Orientation != (landscape ? 2 : 1)) throw new InvalidOperationException("Driver tidak mendukung tata letak yang dipilih.");
        if (Color && ColorMode != (color ? 2 : 1) || Duplex && DuplexMode != duplex || Marshal.ReadInt16(mode, 86) > 1)
            throw new InvalidOperationException("Driver tidak mendukung pilihan warna, dua sisi atau salinan ini.");
    }
        internal byte[] ExportMode() { var bytes = new byte[modeSize]; Marshal.Copy(mode, bytes, 0, bytes.Length); return bytes; }
    internal void ImportMode(byte[] bytes)
    {
        if (bytes.Length != modeSize || BitConverter.ToUInt16(bytes, 68) < 220 || BitConverter.ToUInt16(bytes, 68) + BitConverter.ToUInt16(bytes, 70) > bytes.Length) throw new InvalidOperationException("Pengaturan driver berubah.");
        Marshal.Copy(bytes, 0, mode, bytes.Length);
        if (DocumentProperties(IntPtr.Zero, printer, Name, mode, mode, 10) != 1) throw new InvalidOperationException("Pengaturan driver ditolak.");
    }
    internal static void CancelJob(string name, int job)
    { if (OpenPrinter(name, out var handle, IntPtr.Zero)) try { _ = SetJob(handle, (uint)job, 0, IntPtr.Zero, 3); } finally { ClosePrinter(handle); } }
    [DllImport("winspool.drv", CharSet = CharSet.Unicode)] private static extern bool SetJob(IntPtr handle, uint job, uint level, IntPtr info, uint command);
    internal IntPtr CreateContext()
    {
        var dc = CreateDC("WINSPOOL", Name, null, mode);
        if (dc == IntPtr.Zero) throw new InvalidOperationException("Driver tidak menyediakan konteks cetak.");
        return dc;
    }
    internal Bounds Geometry()
    {
        var dc = CreateContext(); try { return Geometry(dc); } finally { DeleteDC(dc); }
    }
    internal static Bounds Geometry(IntPtr dc)
    {
        var b = new Bounds(GetDeviceCaps(dc, 110), GetDeviceCaps(dc, 111), GetDeviceCaps(dc, 112), GetDeviceCaps(dc, 113), GetDeviceCaps(dc, 8), GetDeviceCaps(dc, 10), GetDeviceCaps(dc, 88), GetDeviceCaps(dc, 90));
        if (b.Width <= 0 || b.Height <= 0 || b.PrintableWidth <= 0 || b.PrintableHeight <= 0 || b.DpiX <= 0 || b.DpiY <= 0) throw new InvalidOperationException("Area cetak driver tidak valid.");
        return b;
    }
    public void Dispose() { if (mode != IntPtr.Zero) { Marshal.FreeHGlobal(mode); mode = IntPtr.Zero; } if (printer != IntPtr.Zero) { ClosePrinter(printer); printer = IntPtr.Zero; } }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] internal struct DocInfo { internal int Size; internal string Name; internal string? Output; internal string? Type; internal int Flags; }
    [StructLayout(LayoutKind.Sequential)] internal struct BitmapInfo { internal uint Size; internal int Width, Height; internal ushort Planes, Bits; internal uint Compression, Bytes; internal int X, Y; internal uint Used, Important; }
    [DllImport("winspool.drv", CharSet = CharSet.Unicode)] private static extern bool EnumPrinters(uint flags, string? name, uint level, IntPtr data, uint size, out uint needed, out uint returned);
    [DllImport("winspool.drv", CharSet = CharSet.Unicode)] private static extern bool OpenPrinter(string name, out IntPtr handle, IntPtr defaults);
    [DllImport("winspool.drv")] private static extern bool ClosePrinter(IntPtr handle);
    [DllImport("winspool.drv", CharSet = CharSet.Unicode)] private static extern int DocumentProperties(IntPtr window, IntPtr printer, string name, IntPtr output, IntPtr input, uint flags);
    [DllImport("winspool.drv", CharSet = CharSet.Unicode)] private static extern int DeviceCapabilities(string name, string? port, short capability, IntPtr output, IntPtr mode);
    [DllImport("winspool.drv", CharSet = CharSet.Unicode)] private static extern bool GetPrinter(IntPtr printer, uint level, IntPtr output, uint size, out uint needed);
    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr CreateDC(string driver, string device, string? output, IntPtr mode);
    [DllImport("gdi32.dll")] internal static extern bool DeleteDC(IntPtr dc);
    [DllImport("gdi32.dll")] private static extern int GetDeviceCaps(IntPtr dc, int index);
    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)] internal static extern int StartDoc(IntPtr dc, ref DocInfo info);
    [DllImport("gdi32.dll")] internal static extern int StartPage(IntPtr dc);
    [DllImport("gdi32.dll")] internal static extern int EndPage(IntPtr dc);
    [DllImport("gdi32.dll")] internal static extern int EndDoc(IntPtr dc);
    [DllImport("gdi32.dll")] internal static extern int AbortDoc(IntPtr dc);
    [DllImport("gdi32.dll")] internal static extern int StretchDIBits(IntPtr dc, int x, int y, int width, int height, int sx, int sy, int sw, int sh, byte[] pixels, ref BitmapInfo info, uint usage, uint rop);
}
