using Microsoft.AspNetCore.Mvc;
using Pertamina.Oto.Web.Preview.Models;

namespace Pertamina.Oto.Web.Preview.Controllers;

public sealed class HomeController(IConfiguration configuration) : Controller
{
    private bool CatalogOnly => configuration.GetValue<bool>("Preview:CatalogOnly");

    [HttpGet("/")]
    public IActionResult Index()
    {
        ViewData["CatalogOnly"] = CatalogOnly;
        return View(CatalogOnly ? "Website" : "Index", ToolCatalog.All);
    }

    [HttpGet("/favorit")]
    public IActionResult Favorites()
    {
        ViewData["Favorites"] = true;
        ViewData["CatalogOnly"] = CatalogOnly;
        return View(CatalogOnly ? "Website" : "Index", ToolCatalog.All);
    }

    [HttpGet("/alat/{id}")]
    public IActionResult Workspace(string id)
    {
        if (CatalogOnly)
        {
            var active = ToolCatalog.All.FirstOrDefault(item => item.Id == id && ToolCatalog.BrowserEnabled(id));
            if (active is null) return NotFound();
            ViewData["CatalogOnly"] = true;
            return View("BrowserWorkspace", new WorkspaceViewModel(active.Id, active.Title, active.Description, active.Icon));
        }
        var tool = ToolCatalog.All.FirstOrDefault(item => item.Id == id && item.Preview);
        if (tool is null) return NotFound();
        return View(new WorkspaceViewModel(tool.Id, tool.Title, tool.Description, tool.Icon));
    }

    [HttpGet("/panduan")]
    public IActionResult Guide() => CatalogOnly ? Redirect("https://oto.ayani.id/panduan") : View();
}
