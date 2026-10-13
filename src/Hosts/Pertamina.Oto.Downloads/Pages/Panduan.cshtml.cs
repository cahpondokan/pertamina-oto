using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pertamina.Oto.Downloads.Pages;

public sealed class PanduanModel : PageModel
{
    internal Pertamina.Oto.Distribution.ReleaseInfo? Release { get; private set; }
    public void OnGet()
    {
        var baseline = HttpContext.RequestServices.GetRequiredService<BaselineCatalog>();
        Release = baseline.Present ? baseline.Current()?.ToReleaseInfo() : HttpContext.RequestServices.GetRequiredService<ReleaseCatalog>().Current();
    }
}
