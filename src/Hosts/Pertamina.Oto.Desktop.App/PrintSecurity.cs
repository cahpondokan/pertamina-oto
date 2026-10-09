namespace Pertamina.Oto.Desktop.App;
internal static class PrintSecurity
{
    // /Encrypt is required in the unencrypted PDF trailer/xref dictionary. Conservatively reject
    // any occurrence (including comments/content) rather than bypass an owner permission.
    internal static async Task<bool> HasEncryptionAsync(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, true);
        var buffer = new byte[65543]; var carry = 0;
        while (true)
        {
            var n = await stream.ReadAsync(buffer.AsMemory(carry, 65536)); if (n == 0) return false;
            var total = n + carry;
            if (buffer.AsSpan(0, total).IndexOf("/Encrypt"u8) >= 0) return true;
            carry = Math.Min(7, total); Array.Copy(buffer, total - carry, buffer, 0, carry);
        }
    }
}
