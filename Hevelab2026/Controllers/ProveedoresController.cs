using Hevelab2026.Services.Compras;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers
{
    public class ProveedoresController : Controller
    {
        private readonly ICompraService _compraService;

        public ProveedoresController(ICompraService compraService)
        {
            _compraService = compraService;
        }

        public async Task<IActionResult> Index(string? busqueda, CancellationToken ct)
        {
            ViewBag.Proveedores = await _compraService.GetProveedoresAsync(busqueda, ct);
            ViewData["Title"] = "Proveedores";
            return View();
        }
    }
}
