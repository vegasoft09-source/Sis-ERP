using Hevelab2026.Models;
using Hevelab2026.Services.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Hevelab2026.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IDashboardService _dashboard;

    public HomeController(ILogger<HomeController> logger, IDashboardService dashboard)
    {
        _logger = logger;
        _dashboard = dashboard;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var model = await _dashboard.GetDashboardAsync(ct);
        return View(model);
    }

    public IActionResult Privacy() => View();

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
