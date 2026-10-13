using Pertamina.Oto.Downloads;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddSingleton<ReleaseCatalog>();
builder.Services.AddSingleton<BaselineCatalog>();
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
var app = builder.Build();
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; img-src 'self'; style-src 'self'; script-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    if (!app.Environment.IsDevelopment()) context.Response.Headers.StrictTransportSecurity = "max-age=31536000";
    await next(context);
});
app.UseStaticFiles();
app.MapRazorPages();
app.MapGet("/health", () => Results.NoContent());
app.MapGet("/api/releases/windows-x64/latest", (ReleaseCatalog catalog, BaselineCatalog baseline, HttpContext context) =>
{
    var release = catalog.Current();
    context.Response.Headers.CacheControl = "no-store";
    // Old desktop clients cannot consume launcher protocol 2. A transition release
    // remains downloadable on the website but must not enter their in-app updater.
    return baseline.Present || release is null || release.ManualInstallerOnly ? Results.NoContent() : Results.Json(release);
});
app.MapGet("/api/releases/windows-x64/installer", (ReleaseCatalog catalog, BaselineCatalog baseline, HttpContext context) =>
{
    var release = catalog.Current();
    context.Response.Headers.CacheControl = "no-store";
    if (baseline.Present)
        release = baseline.Current() is { } candidate ? candidate.ToReleaseInfo() with { ManualInstallerOnly = true } : null;
    return release is null ? Results.NoContent() : Results.Json(release);
});
app.MapGet("/api/releases/windows-x64/baseline-1/latest", (BaselineCatalog catalog, HttpContext context) =>
{
    context.Response.Headers.CacheControl = "no-store";
    var release = catalog.Current();
    return release is null ? Results.NoContent() : Results.Json(release);
});
app.MapMethods("/downloads/{fileName}", ["GET", "HEAD"], (string fileName, ReleaseCatalog catalog, BaselineCatalog baseline, HttpContext context) =>
{
    var download = baseline.Present ? baseline.Open(fileName) : catalog.Open(fileName);
    if (download is null) return Results.NotFound();
    context.Response.Headers.CacheControl = "public,max-age=3600";
    return Results.File(download, fileName.EndsWith(".exe", StringComparison.Ordinal) ? "application/octet-stream" : "application/zip", fileName, enableRangeProcessing: true);
});
app.Run();



