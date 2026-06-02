using Hevelab2026.Models.Compras;
using Hevelab2026.Services.Compras;
using Hevelab2026.Services.Security;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers;

public class ProveedoresController : Controller
{
    private readonly ICompraService _compraService;
    private readonly ICurrentUserService _currentUser;

    public ProveedoresController(ICompraService compraService, ICurrentUserService currentUser)
    {
        _compraService = compraService;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(string? busqueda, CancellationToken ct)
    {
        ViewBag.Proveedores = await _compraService.GetProveedoresAsync(busqueda, ct);
        ViewBag.Busqueda = busqueda;
        ViewData["Title"] = "Proveedores";
        ViewData["Breadcrumbs"] = new List<(string Label, string Url)>
        {
            ("Dashboard", "/"),
            ("Compras", "#"),
            ("Proveedores", "")
        };
        return View();
    }

    [HttpGet]
    public IActionResult Formulario() => View(new ProveedorFormModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Formulario(ProveedorFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        await _compraService.CrearProveedorAsync(
            _currentUser.EmpresaId,
            model.RazonSocial,
            model.TipoDocumento,
            model.NumeroDocumento,
            model.Telefono,
            model.Email,
            model.Direccion,
            ct);

        TempData["Success"] = "Proveedor registrado.";
        return RedirectToAction(nameof(Index));
    }
}
