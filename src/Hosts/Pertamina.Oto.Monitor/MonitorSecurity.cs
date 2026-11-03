using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;

namespace Oto.Monitor;

internal sealed record AdminCredential(string Username, string Salt, string Hash, int Iterations);

internal sealed class MonitorSecurity
{
    private readonly AdminCredential credential;
    private readonly byte[] salt;
    private readonly byte[] expected;
    private readonly ConcurrentDictionary<string, DateTimeOffset> sessions = new(StringComparer.Ordinal);
    private readonly object gate = new();
    private DateTimeOffset lockedUntil;
    private int failedAttempts;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    internal MonitorSecurity(string path)
    {
        PrivateFiles.Check(path);
        if (new FileInfo(path).Length > 4096) throw new InvalidOperationException("Admin configuration invalid.");
        credential = JsonSerializer.Deserialize<AdminCredential>(File.ReadAllText(path), Json)
            ?? throw new InvalidOperationException("Admin configuration unavailable.");
        if (credential.Username.Length is < 3 or > 40 || credential.Username.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_') || credential.Iterations != 600000)
            throw new InvalidOperationException("Admin configuration invalid.");
        salt = Convert.FromBase64String(credential.Salt); expected = Convert.FromBase64String(credential.Hash);
        if (salt.Length != 32 || expected.Length != 32) throw new InvalidOperationException("Admin configuration invalid.");
        if (!OperatingSystem.IsWindows() && (File.GetUnixFileMode(path) & (UnixFileMode.GroupWrite | UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute)) != 0)
            throw new InvalidOperationException("Admin configuration permissions invalid.");
    }

    internal bool Locked { get { lock (gate) return DateTimeOffset.UtcNow < lockedUntil; } }
    internal bool Verify(string username, string password)
    {
        lock (gate)
        {
            if (DateTimeOffset.UtcNow < lockedUntil || password.Length > 128 || username.Length > 40) return false;
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, credential.Iterations, HashAlgorithmName.SHA256, 32);
            var verified = CryptographicOperations.FixedTimeEquals(actual, expected) & string.Equals(username, credential.Username, StringComparison.Ordinal);
            CryptographicOperations.ZeroMemory(actual);
            if (verified) { failedAttempts = 0; return true; }
            if (++failedAttempts >= 5) { lockedUntil = DateTimeOffset.UtcNow.AddMinutes(5); failedAttempts = 0; }
            return false;
        }
    }

    internal string CreateSession()
    {
        foreach (var entry in sessions) if (entry.Value <= DateTimeOffset.UtcNow) sessions.TryRemove(entry.Key, out _);
        // A bounded number of concurrent operator sessions. Oldest is revoked.
        if (sessions.Count >= 5) sessions.TryRemove(sessions.MinBy(entry => entry.Value).Key, out _);
        var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        sessions[id] = DateTimeOffset.UtcNow.AddHours(2);
        return id;
    }
    internal bool ValidSession(string? id) => id is not null && sessions.TryGetValue(id, out var expires) && expires > DateTimeOffset.UtcNow;
    internal void Revoke(string? id) { if (id is not null) sessions.TryRemove(id, out _); }
}

internal static class PrivateFiles
{
    internal static void Check(string path)
    {
        if (!Path.IsPathFullyQualified(path)) throw new IOException("Absolute private path required.");
        for (var current = path; !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
            if ((File.Exists(current) || Directory.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("Private path link rejected.");
    }
}
