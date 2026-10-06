using Microsoft.AspNetCore.Mvc.RazorPages;
using Pertamina.Oto.Distribution;

namespace Pertamina.Oto.Downloads.Pages;

public sealed class IndexModel : PageModel
{
    internal ReleaseInfo? Release { get; private set; }
    public void OnGet()
    {
        Release = HttpContext.RequestServices.GetRequiredService<ReleaseCatalog>().Current();
    }
}
