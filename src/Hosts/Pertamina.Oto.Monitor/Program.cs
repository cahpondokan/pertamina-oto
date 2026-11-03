using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Oto.Monitor;

var builder = WebApplication.CreateBuilder(args);
// No request bodies, passwords, exception text, raw URLs or document data in logs.
builder.Logging.ClearProviders();
var development = builder.Environment.IsDevelopment();
var origin = builder.Configuration["Monitor:Origin"] ?? "https://monitor.ayani.id";
if (!Uri.TryCreate(origin, UriKind.Absolute, out var originUri) || originUri.AbsolutePath != "/" || originUri.Query.Length != 0 || originUri.Fragment.Length != 0 || originUri.UserInfo.Length != 0 ||
    (!development && (originUri.Scheme != "https" || originUri.Host != "monitor.ayani.id"))) throw new InvalidOperationException("Invalid private origin.");
origin = origin.TrimEnd('/');
var credentials = builder.Configuration["Monitor:CredentialFile"] ?? throw new InvalidOperationException("Admin credential file required.");
var dataDirectory = builder.Configuration["Monitor:DataDirectory"] ?? throw new InvalidOperationException("Private data directory required.");
var keyDirectory = builder.Configuration["Monitor:KeyDirectory"] ?? throw new InvalidOperationException("Private keys directory required.");
foreach (var path in new[] { credentials, dataDirectory, keyDirectory })
{
    PrivateFiles.Check(path);
    if ((Path.GetFullPath(path) + Path.DirectorySeparatorChar).StartsWith(Path.GetFullPath(builder.Environment.ContentRootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Private data must be outside the application.");
}
var port = builder.Configuration.GetValue("Monitor:Port", 5091);
if (port is < 1024 or > 65535) throw new InvalidOperationException("Invalid private port.");
if (OperatingSystem.IsWindows()) Directory.CreateDirectory(keyDirectory);
else Directory.CreateDirectory(keyDirectory, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
builder.WebHost.ConfigureKestrel(options => { options.Listen(IPAddress.Loopback, port); options.AddServerHeader = false; options.Limits.MaxRequestBodySize = 4096; options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(10); });
builder.Services.Configure<FormOptions>(options => { options.ValueCountLimit = 4; options.ValueLengthLimit = 1024; options.KeyLengthLimit = 64; options.MultipartBodyLengthLimit = 4096; });
builder.Services.AddSingleton(new MonitorSecurity(credentials));
builder.Services.AddSingleton(new MonitorData(dataDirectory));
builder.Services.AddDataProtection().SetApplicationName("OTO.PrivateMonitor.v1").PersistKeysToFileSystem(new DirectoryInfo(keyDirectory));
builder.Services.AddAntiforgery(options => {
    options.Cookie.Name = development ? "oto-monitor-csrf-dev" : "__Host-oto-monitor-csrf";
    options.Cookie.SecurePolicy = development ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true; options.Cookie.SameSite = SameSiteMode.Strict;
    options.FormFieldName = "csrf";
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => {
    options.Cookie.Name = development ? "oto-monitor-dev" : "__Host-oto-monitor";
    options.Cookie.Path = "/"; options.Cookie.HttpOnly = true; options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = development ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.LoginPath = "/login"; options.ExpireTimeSpan = TimeSpan.FromHours(2); options.SlidingExpiration = false;
    options.Events.OnRedirectToLogin = context => { if (context.Request.Path.StartsWithSegments("/api")) context.Response.StatusCode = 401; else context.Response.Redirect("/login"); return Task.CompletedTask; };
    options.Events.OnValidatePrincipal = context => {
        if (!context.HttpContext.RequestServices.GetRequiredService<MonitorSecurity>().ValidSession(context.Principal?.FindFirstValue("sid"))) context.RejectPrincipal();
        return Task.CompletedTask;
    };
});
builder.Services.AddAuthorization();
builder.Services.Configure<ForwardedHeadersOptions>(options => {
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
});
var app = builder.Build();
app.UseForwardedHeaders();
app.Use(async (context, next) => {
    context.Response.Headers.CacheControl = "no-store";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; style-src 'self'; script-src 'self'; connect-src 'self'; img-src 'self'; base-uri 'none'; frame-ancestors 'none'; form-action 'self'";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "same-origin";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow, noarchive";
    if (context.Connection.RemoteIpAddress is not { } address || !IPAddress.IsLoopback(address) ||
        !string.Equals(context.Request.Host.Value, originUri.Authority, StringComparison.OrdinalIgnoreCase) ||
        (!development && !context.Request.IsHttps)) { context.Response.StatusCode = 404; return; }
    // Reject cross-site and sibling-subdomain requests, including top-level navigations.
    var site = context.Request.Headers["Sec-Fetch-Site"].ToString();
    if (site.Length > 0 && site is not "same-origin" and not "none") { context.Response.StatusCode = 403; return; }
    if (HttpMethods.IsPost(context.Request.Method) && context.Request.Headers.Origin != origin) { context.Response.StatusCode = 403; return; }
    try { await next(context); }
    catch (Exception ex) when (ex is not OutOfMemoryException) {
        if (!context.Response.HasStarted) { context.Response.StatusCode = 503; await context.Response.WriteAsync("Layanan sementara tidak tersedia."); }
    }
});
app.UseAuthentication();
app.UseAuthorization();
string Page(HttpContext context, IAntiforgery antiforgery, string file, string message = "") => File.ReadAllText(Path.Combine(app.Environment.ContentRootPath, "Pages", file))
    .Replace("{{csrf}}", HtmlEncoder.Default.Encode(antiforgery.GetAndStoreTokens(context).RequestToken!), StringComparison.Ordinal)
    .Replace("{{message}}", HtmlEncoder.Default.Encode(message), StringComparison.Ordinal);
app.MapGet("/login", (HttpContext context, IAntiforgery antiforgery) => context.User.Identity?.IsAuthenticated == true
    ? Results.Redirect("/") : Results.Content(Page(context, antiforgery, "login.html"), "text/html; charset=utf-8"));
app.MapPost("/login", async (HttpContext context, IAntiforgery antiforgery, MonitorSecurity security) => {
    if (security.Locked) { context.Response.Headers.RetryAfter = "300"; return Results.Content(Page(context, antiforgery, "login.html", "Terlalu banyak percobaan. Coba lagi dalam 5 menit."), "text/html; charset=utf-8", statusCode: 429); }
    try { await antiforgery.ValidateRequestAsync(context); } catch (AntiforgeryValidationException) { return Results.StatusCode(400); }
    var form = await context.Request.ReadFormAsync();
    if (!security.Verify(form["username"].ToString(), form["password"].ToString())) return Results.Content(Page(context, antiforgery, "login.html", "Nama pengelola atau kata sandi tidak sesuai."), "text/html; charset=utf-8", statusCode: 401);
    var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "Pengelola OTO"), new Claim("sid", security.CreateSession())], CookieAuthenticationDefaults.AuthenticationScheme));
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = false, AllowRefresh = false });
    return Results.Redirect("/");
});
app.MapPost("/logout", async (HttpContext context, IAntiforgery antiforgery, MonitorSecurity security) => {
    try { await antiforgery.ValidateRequestAsync(context); } catch (AntiforgeryValidationException) { return Results.StatusCode(400); }
    security.Revoke(context.User.FindFirstValue("sid")); await context.SignOutAsync(); return Results.Redirect("/login");
}).RequireAuthorization();
app.MapGet("/", (HttpContext context, IAntiforgery antiforgery) => Results.Content(Page(context, antiforgery, "dashboard.html"), "text/html; charset=utf-8")).RequireAuthorization();
app.MapGet("/api/overview", (HttpContext context, MonitorData data) => {
    var query = context.Request.Query;
    if (query.Keys.Any(key => key is not "days" and not "feature" and not "outcome" and not "process" and not "page")) return Results.BadRequest();
    if (!int.TryParse(query["days"].FirstOrDefault() ?? "1", out var days) || days is not 1 and not 7 and not 30 and not 0 ||
        !int.TryParse(query["page"].FirstOrDefault() ?? "1", out var page) || page is < 1 or > 656) return Results.BadRequest();
    return data.Overview(days, query["feature"].ToString(), query["outcome"].ToString(), query["process"].ToString(), page);
}).RequireAuthorization();
foreach (var asset in new[] { "monitor.css", "monitor.js" })
{
    var name = asset;
    app.MapGet("/assets/" + name, () => Results.File(Path.Combine(app.Environment.ContentRootPath, "wwwroot", name), name.EndsWith(".css", StringComparison.Ordinal) ? "text/css" : "text/javascript"));
}
app.MapGet("/health", () => Results.Ok(new { status = "up" })).RequireAuthorization();
app.Run();
