using Microsoft.AspNetCore.Mvc;
using Pertamina.Oto.Web.Preview.Models;

namespace Pertamina.Oto.Downloads;

public sealed class BrowserController : Controller
{
    [HttpGet("/")]
    [HttpHead("/")]
    public IActionResult Index() => Catalog(false);

    [HttpGet("/favorit")]
    [HttpHead("/favorit")]
    public IActionResult Favorites() => Catalog(true);

    [HttpGet("/alat/{id}")]
    [HttpHead("/alat/{id}")]
    public IActionResult Workspace(string id)
    {
        var tool = ToolCatalog.All.FirstOrDefault(item => item.Id == id && ToolCatalog.BrowserEnabled(id));
        if (tool is null) return NotFound();
        ViewData["CatalogOnly"] = true;
        return View("~/Views/Home/BrowserWorkspace.cshtml", new WorkspaceViewModel(tool.Id, tool.Title, tool.Description, tool.Icon));
    }

    private ViewResult Catalog(bool favorites)
    {
        ViewData["CatalogOnly"] = true;
        ViewData["Favorites"] = favorites;
        return View("~/Views/Home/Website.cshtml", ToolCatalog.All);
    }
}
