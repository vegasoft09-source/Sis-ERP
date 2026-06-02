using Hevelab2026.Services.Compras;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers
{
    public class OrdenesDeCompraController : Controller
    {
        private readonly ICompraService _compraService;

        public OrdenesDeCompraController(ICompraService compraService)
        {
            _compraService = compraService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            ViewBag.Ordenes = await _compraService.GetOrdenesCompraAsync(ct);
            ViewData["Title"] = "Órdenes de compra";
            return View();
        }
    }
}
