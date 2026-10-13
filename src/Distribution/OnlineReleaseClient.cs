using System.Net;
using System.Text.Json;

namespace Pertamina.Oto.Distribution;

internal static class OnlineReleaseClient
{
    internal static async Task<DistributionRelease?> FetchBaselineAsync(HttpClient client, CancellationToken cancellationToken)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(10));
        using var response = await client.GetAsync(DistributionRelease.LatestUri, HttpCompletionOption.ResponseHeadersRead, deadline.Token).ConfigureAwait(false);
        if (response.RequestMessage?.RequestUri is { } final && final != DistributionRelease.LatestUri)
            throw new InvalidDataException("Redirect metadata tidak diizinkan.");
        if (response.StatusCode == HttpStatusCode.NoContent) return null;
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentType?.MediaType != "application/json" || response.Content.Headers.ContentLength > 98304)
            throw new InvalidDataException("Metadata baseline tidak valid.");
        await using var stream = await response.Content.ReadAsStreamAsync(deadline.Token).ConfigureAwait(false);
        using var buffer = new MemoryStream(); var chunk = new byte[4096];
        while (true)
        {
            var count = await stream.ReadAsync(chunk, deadline.Token).ConfigureAwait(false);
            if (count == 0) break;
            if (buffer.Length + count > 98304) throw new InvalidDataException("Metadata baseline terlalu besar.");
            buffer.Write(chunk, 0, count);
        }
        var release = JsonSerializer.Deserialize<DistributionRelease>(buffer.ToArray(), JsonSerializerOptions.Web)
            ?? throw new InvalidDataException("Metadata baseline kosong.");
        release.Validate(); return release;
    }
    internal static async Task<ReleaseInfo?> FetchAsync(HttpClient client, CancellationToken cancellationToken)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(10));
        using var response = await client.GetAsync(ReleaseInfo.LatestUri, HttpCompletionOption.ResponseHeadersRead, deadline.Token).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NoContent) return null;
        response.EnsureSuccessStatusCode();
        if (response.RequestMessage?.RequestUri is { } final && final != ReleaseInfo.LatestUri)
            throw new InvalidDataException("Redirect metadata tidak diizinkan.");
        if (response.Content.Headers.ContentType?.MediaType != "application/json" || response.Content.Headers.ContentLength > 16384)
            throw new InvalidDataException("Metadata rilis tidak valid.");
        await using var stream = await response.Content.ReadAsStreamAsync(deadline.Token).ConfigureAwait(false);
        using var buffer = new MemoryStream(); var chunk = new byte[2048];
        while (true)
        {
            var count = await stream.ReadAsync(chunk, deadline.Token).ConfigureAwait(false);
            if (count == 0) break;
            if (buffer.Length + count > 16384) throw new InvalidDataException("Metadata rilis terlalu besar.");
            buffer.Write(chunk, 0, count);
        }
        var release = JsonSerializer.Deserialize<ReleaseInfo>(buffer.ToArray(), JsonSerializerOptions.Web)
            ?? throw new InvalidDataException("Metadata rilis kosong.");
        release.Validate(); return release;
    }
}
