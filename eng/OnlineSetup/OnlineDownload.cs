using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Oto.OnlineSetup
{
    internal static class OnlineDownload
    {
        internal static readonly Uri Origin = new Uri("https://oto.ayani.id");
        internal static async Task FetchAsync(HttpClient client, Uri uri, string path, long size, string sha256, Action<long, long> progress, CancellationToken cancellation)
        {
            if (uri.Scheme != "https" || uri.Host != Origin.Host || uri.Port != 443 || uri.UserInfo.Length != 0 || uri.Query.Length != 0 || uri.Fragment.Length != 0
                || !uri.AbsolutePath.StartsWith("/downloads/PERTAMINA-OTO-Setup-x64-", StringComparison.Ordinal)
                || size <= 0 || size > 2L * 1024 * 1024 * 1024 || !System.Text.RegularExpressions.Regex.IsMatch(sha256 ?? "", "^[A-Fa-f0-9]{64}$"))
                throw new InvalidDataException("Alamat atau metadata unduhan tidak valid.");
            if (File.Exists(path)) throw new IOException("Lokasi unduhan sudah ada.");
            using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation))
            {
                deadline.CancelAfter(TimeSpan.FromMinutes(20));
                // At most two attempts, only transient HTTP connection failures; cancellation/integrity failures never retry.
                for (var attempt = 0; ; attempt++)
                {
                    try
                    {
                        using (var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, deadline.Token).ConfigureAwait(false))
                        {
                            if (response.RequestMessage != null && response.RequestMessage.RequestUri != uri) throw new InvalidDataException("Redirect tidak diizinkan.");
                            if ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400) throw new InvalidDataException("Redirect tidak diizinkan.");
                            response.EnsureSuccessStatusCode();
                            if (response.Content.Headers.ContentLength.HasValue && response.Content.Headers.ContentLength.Value != size) throw new InvalidDataException("Ukuran unduhan tidak cocok.");
                            using (var input = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                            using (var interrupt = deadline.Token.Register(() => input.Dispose()))
                            using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, true))
                            using (var hash = SHA256.Create())
                            {
                                var buffer = new byte[65536]; long total = 0;
                                while (true)
                                {
                                    var count = await input.ReadAsync(buffer, 0, buffer.Length, deadline.Token).ConfigureAwait(false);
                                    if (count == 0) break;
                                    total += count; if (total > size) throw new InvalidDataException("Unduhan melebihi batas.");
                                    hash.TransformBlock(buffer, 0, count, null, 0);
                                    await output.WriteAsync(buffer, 0, count, deadline.Token).ConfigureAwait(false);
                                    progress(total, size);
                                }
                                hash.TransformFinalBlock(new byte[0], 0, 0);
                                if (total != size || !BitConverter.ToString(hash.Hash).Replace("-", "").Equals(sha256, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Hash atau ukuran unduhan tidak cocok.");
                            }
                        }
                        cancellation.ThrowIfCancellationRequested(); return;
                    }
                    catch (Exception) when (deadline.IsCancellationRequested) { throw new OperationCanceledException(deadline.Token); }
                    catch (HttpRequestException) when (attempt == 0 && !deadline.IsCancellationRequested)
                    {
                        // path belongs exclusively to this attempt's fresh restricted staging directory.
                        if (File.Exists(path)) File.Delete(path);
                        await Task.Delay(750, deadline.Token).ConfigureAwait(false);
                    }
                }
            }
        }
        internal static void Verify(string path, long size, string expected)
        {
            using (var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var hash = SHA256.Create())
                if (input.Length != size || !BitConverter.ToString(hash.ComputeHash(input)).Replace("-", "").Equals(expected, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Paket berubah setelah unduhan.");
        }
    }
}
