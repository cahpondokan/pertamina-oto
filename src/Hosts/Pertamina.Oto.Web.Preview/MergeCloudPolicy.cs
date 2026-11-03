// Public client identifiers only. No OAuth client secrets or server document flow.
internal static class MergeCloudPolicy
{
    internal static string WithDiagnostics(string policy) => policy.Contains("connect-src 'none'", StringComparison.Ordinal)
        ? policy.Replace("connect-src 'none'", "connect-src 'self'", StringComparison.Ordinal)
        : policy.Replace("connect-src ", "connect-src 'self' ", StringComparison.Ordinal);

    internal static bool Enabled(IConfiguration config, string provider) =>
        config[$"MergeCloud:{provider}:Admitted"] == "true" &&
        !string.IsNullOrWhiteSpace(config[$"MergeCloud:{provider}:Key"]) &&
        (provider != "Google" || (!string.IsNullOrWhiteSpace(config["MergeCloud:Google:ClientId"]) && !string.IsNullOrWhiteSpace(config["MergeCloud:Google:ProjectNumber"])));

    internal static string ContentSecurityPolicy(IConfiguration config)
    {
        var google = Enabled(config, "Google"); var dropbox = Enabled(config, "Dropbox");
        var scripts = "'self'" + (google ? " https://apis.google.com https://accounts.google.com" : "") + (dropbox ? " https://www.dropbox.com" : "");
        var connections = (google ? "https://www.googleapis.com https://accounts.google.com" : "") + (dropbox ? " https://dl.dropboxusercontent.com https://*.dl.dropboxusercontent.com" : "");
        if (string.IsNullOrWhiteSpace(connections)) connections = "'none'";
        return $"default-src 'self'; script-src {scripts}; worker-src 'self'; style-src 'self'; img-src 'self' blob:; font-src 'self'; connect-src {connections}; object-src 'none'; frame-src {(google ? "https://docs.google.com https://accounts.google.com" : "'none'")}; base-uri 'self'; frame-ancestors 'none'; form-action 'none'";
    }
}
