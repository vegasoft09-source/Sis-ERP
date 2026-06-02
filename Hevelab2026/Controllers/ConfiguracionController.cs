using Hevelab2026.Models.Config;
using Hevelab2026.Services.Config;
using Hevelab2026.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers;

[Authorize]
[Route("settings")]
[Route("Configuracion")]
public class ConfiguracionController : Controller
{
    private readonly IPerfilService _perfil;
    private readonly ICurrentUserService _currentUser;

    public ConfiguracionController(IPerfilService perfil, ICurrentUserService currentUser)
    {
        _perfil = perfil;
        _currentUser = currentUser;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(string? tab, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return RedirectToAction("Login", "Auth");

        var activeTab = string.IsNullOrWhiteSpace(tab) ? "profile" : tab.ToLowerInvariant();
        var vm = await _perfil.GetConfiguracionAsync(userId.Value, _currentUser.EmpresaId, activeTab, ct);

        ViewData["Breadcrumbs"] = new List<(string Label, string Url)>
        {
            ("Dashboard", "/"),
            ("Configuración", "")
        };

        return View(vm);
    }

    [HttpPost("perfil")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarPerfil(PerfilViewModel model, IFormFile? foto, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
        {
            var page = await _perfil.GetConfiguracionAsync(userId.Value, _currentUser.EmpresaId, "profile", ct);
            page.Perfil = model;
            page.Tab = "profile";
            return View("Index", page);
        }

        try
        {
            await _perfil.GuardarPerfilAsync(userId.Value, model, foto, ct);
            await _perfil.RefrescarClaimsAsync(HttpContext, userId.Value, ct);
            TempData["Success"] = "Perfil actualizado correctamente.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            var page = await _perfil.GetConfiguracionAsync(userId.Value, _currentUser.EmpresaId, "profile", ct);
            page.Perfil = model;
            page.Tab = "profile";
            return View("Index", page);
        }

        return RedirectToAction(nameof(Index), new { tab = "profile" });
    }

    [HttpPost("empresa")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarEmpresa(EmpresaConfigViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Index), new { tab = "empresa" });

        await _perfil.GuardarEmpresaAsync(_currentUser.EmpresaId, model, ct);
        TempData["Success"] = "Datos de empresa guardados.";
        return RedirectToAction(nameof(Index), new { tab = "empresa" });
    }
}
