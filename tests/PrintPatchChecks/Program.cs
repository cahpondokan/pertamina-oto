using System.Runtime.InteropServices;
using Pertamina.Oto.Desktop.App;

var output = Path.GetFullPath(args[0]); Directory.CreateDirectory(output);
var report = new List<string>();
void Check(bool value, string description) { if (!value) throw new InvalidOperationException(description); report.Add("PASS " + description); }
foreach (var value in new[] { "A4", "Letter", "日本語 ukuran", "", new string('x', 64), "A4\0SYNTHETIC_PADDING" })
{
    var slot = Enumerable.Repeat('z', 64).ToArray(); value.CopyTo(0, slot, 0, value.Length);
    if (value.Length < 64) slot[value.Length] = '\0';
    var expected = value.Split('\0')[0];
    Check(PrintDevice.ParsePaperName(slot) == expected, "first null or exact slot boundary: case length " + value.Length);
}
var buffer = Marshal.AllocHGlobal(256);
try
{
    var slots = Enumerable.Repeat('z', 128).ToArray(); slots[0] = 'A'; slots[1] = '4'; slots[2] = '\0';
    Marshal.Copy(slots, 0, buffer, slots.Length);
    Check(PrintDevice.ParsePaperName(Marshal.PtrToStringUni(buffer, 64)!.AsSpan()) == "A4", "real unmanaged 128-byte slot excludes nonzero padding and following slot");
    Marshal.Copy(new string('n', 64).ToCharArray(), 0, buffer, 64);
    Check(PrintDevice.ParsePaperName(Marshal.PtrToStringUni(buffer, 64)!.AsSpan()) == new string('n', 64), "unmanaged exact64 without null stops before following slot");
}
finally { Marshal.FreeHGlobal(buffer); }
foreach (var size in new[] { 0, 63, 65 })
{ var rejected = false; try { PrintDevice.ParsePaperName(new char[size]); } catch (ArgumentException) { rejected = true; } Check(rejected, "invalid slot size " + size); }
foreach (var count in new[] { int.MinValue, -1, 0, 1001, int.MaxValue })
{ var rejected = false; try { PrintDevice.PaperBufferBytes(count); } catch (InvalidOperationException) { rejected = true; } Check(rejected, "invalid allocation count " + count); }
Check(PrintDevice.PaperBufferBytes(1) == 128 && PrintDevice.PaperBufferBytes(1000) == 128000, "valid allocation extremes");
foreach (var state in new int?[] { null, 0, 0x80, 1 }) Check(PrintDevice.WindowsStatus(state).Contains("Keterjangkauan belum diverifikasi", StringComparison.Ordinal), "Windows state never claims verified reachability " + state);
Check(PrintDevice.WindowsStatus(0).Contains("cache", StringComparison.Ordinal), "ready status explicitly may be cached");
Check(PrintDevice.WindowsStatus(0x80).Contains("Offline menurut Windows", StringComparison.Ordinal), "Windows offline distinguished");
File.WriteAllLines(Path.Combine(output, "results.txt"), report);
Console.WriteLine(report.Count + " PASS; no printer enumeration, network calls or print jobs.");
