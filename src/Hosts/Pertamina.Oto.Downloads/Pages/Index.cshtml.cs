using Microsoft.AspNetCore.Mvc.RazorPages;
using Pertamina.Oto.Distribution;

namespace Pertamina.Oto.Downloads.Pages;

public sealed class IndexModel : PageModel
{
    internal ReleaseInfo? Release { get; private set; }
    internal DistributionRelease? Distribution { get; private set; }
    internal bool FixturePreview { get; private set; }
    internal static string FormatSize(long bytes) => bytes < 1024 ? $"{bytes} byte" : bytes < 1048576
        ? $"{(bytes / 1024d).ToString("0.0", System.Globalization.CultureInfo.GetCultureInfo("id-ID"))} KB"
        : $"{(bytes / 1048576d).ToString("0.0", System.Globalization.CultureInfo.GetCultureInfo("id-ID"))} MB";
    public void OnGet()
    {
        var baseline = HttpContext.RequestServices.GetRequiredService<BaselineCatalog>();
        Distribution = baseline.Current();
        Release = baseline.Present ? Distribution?.ToReleaseInfo() : HttpContext.RequestServices.GetRequiredService<ReleaseCatalog>().Current();
        FixturePreview = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetValue<bool>("Distribution:FixturePreview");
    }
}

