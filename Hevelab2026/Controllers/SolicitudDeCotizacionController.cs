using Hevelab2026.Models.Compras;
using Hevelab2026.Services.Compras;
using Hevelab2026.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hevelab2026.Controllers;

public class SolicitudDeCotizacionController : Controller
{
    private readonly ICompraService _compraService;
    private readonly ICurrentUserService _currentUser;

    public SolicitudDeCotizacionController(ICompraService compraService, ICurrentUserService currentUser)
    {
        _compraService = compraService;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewBag.Solicitudes = await _compraService.GetSolicitudesAsync(ct);
        ViewData["Title"] = "Solicitudes de cotización";
        ViewData["Breadcrumbs"] = new List<(string Label, string Url)>
        {
            ("Dashboard", "/"),
            ("Compras", "#"),
            ("Solicitudes", "")
        };
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Crear(CancellationToken ct)
    {
        await CargarProveedoresAsync(ct);
        return View(new SolicitudCompraFormModel { FechaLimite = DateTime.Today.AddDays(7) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(SolicitudCompraFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await CargarProveedoresAsync(ct);
            return View(model);
        }

        await _compraService.CrearSolicitudAsync(
            _currentUser.EmpresaId,
            model.ProveedorId,
            model.Referencia,
            model.FechaLimite,
            model.TotalEstimado,
            model.Observaciones,
            _currentUser.UserId,
            ct);

        TempData["Success"] = "Solicitud creada.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarProveedoresAsync(CancellationToken ct)
    {
        var proveedores = await _compraService.GetProveedoresAsync(null, ct);
        ViewBag.Proveedores = new SelectList(proveedores, "Id", "RazonSocial");
    }
}
