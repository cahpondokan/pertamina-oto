using System.Net;
using Oto.WebDiagnostics;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Pertamina.Oto.Web.Preview;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, EnvironmentName = "Preview" });
// Local preview does not require Windows Event Log access or document/server storage.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
// No authentication/session cookies and no persisted keys.
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.Configure<KeyManagementOptions>(options => options.XmlRepository = new PreviewMemoryKeyRepository());
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<DiagnosticsStore>();
var app = builder.Build();
var diagnostics = app.Services.GetRequiredService<DiagnosticsStore>();
app.Use(async (context, next) =>
{
    if (context.Connection.RemoteIpAddress is { } address && !IPAddress.IsLoopback(address))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }
    context.Response.Headers.ContentSecurityPolicy = MergeCloudPolicy.Enabled(builder.Configuration, "Google") || MergeCloudPolicy.Enabled(builder.Configuration, "Dropbox")
        ? MergeCloudPolicy.ContentSecurityPolicy(builder.Configuration)
        : "default-src 'self'; script-src 'self'; worker-src 'self'; style-src 'self'; img-src 'self' blob:; font-src 'self'; connect-src 'none'; object-src 'none'; frame-src 'none'; base-uri 'self'; frame-ancestors 'none'; form-action 'none'";
    if (diagnostics.Enabled) context.Response.Headers.ContentSecurityPolicy = MergeCloudPolicy.WithDiagnostics(context.Response.Headers.ContentSecurityPolicy.ToString());
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Cache-Control"] = "no-store";
    if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method) && !DiagnosticsHosting.IsIngestion(context.Request))
    {
        context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
        return;
    }
    try { await next(context); }
    catch (Exception) { await DiagnosticsHosting.SafeServerError(context, diagnostics); }
});
app.UseStaticFiles();
app.MapControllers();
app.MapWebDiagnostics();
app.Run();
