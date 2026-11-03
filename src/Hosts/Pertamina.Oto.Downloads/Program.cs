using Pertamina.Oto.Downloads;
using Oto.WebDiagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ReleaseCatalog>();
builder.Services.AddSingleton<BaselineCatalog>();
builder.Services.AddSingleton<DiagnosticsStore>();
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
var app = builder.Build();
var diagnostics = app.Services.GetRequiredService<DiagnosticsStore>();
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Content-Security-Policy"] = MergeCloudPolicy.Enabled(builder.Configuration, "Google") || MergeCloudPolicy.Enabled(builder.Configuration, "Dropbox")
        ? MergeCloudPolicy.ContentSecurityPolicy(builder.Configuration)
        : "default-src 'self'; script-src 'self'; worker-src 'self'; style-src 'self'; img-src 'self' blob:; font-src 'self'; connect-src 'none'; object-src 'none'; frame-src 'none'; base-uri 'self'; frame-ancestors 'none'; form-action 'none'";
    if (diagnostics.Enabled) context.Response.Headers.ContentSecurityPolicy = MergeCloudPolicy.WithDiagnostics(context.Response.Headers.ContentSecurityPolicy.ToString());
    context.Response.Headers.CacheControl = "no-store";
    if (!app.Environment.IsDevelopment()) context.Response.Headers.StrictTransportSecurity = "max-age=31536000";
    if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method) && !DiagnosticsHosting.IsIngestion(context.Request))
    {
        context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
        return;
    }
    try { await next(context); }
    catch (Exception) { await DiagnosticsHosting.SafeServerError(context, diagnostics); }
});
app.UseStaticFiles();
app.MapRazorPages();
app.MapControllers();
app.MapWebDiagnostics();
app.MapGet("/vendor/pdfjs-6.4.299/LICENSE", (IWebHostEnvironment environment) =>
    Results.File(Path.Combine(environment.WebRootPath, "vendor", "pdfjs-6.4.299", "LICENSE"), "text/plain"));
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



