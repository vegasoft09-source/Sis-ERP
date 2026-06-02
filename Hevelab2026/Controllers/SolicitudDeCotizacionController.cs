using Hevelab2026.Services.Compras;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers
{
    public class SolicitudDeCotizacionController : Controller
    {
        private readonly ICompraService _compraService;

        public SolicitudDeCotizacionController(ICompraService compraService)
        {
            _compraService = compraService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            ViewBag.Solicitudes = await _compraService.GetSolicitudesAsync(ct);
            ViewData["Title"] = "Solicitudes de cotización";
            return View();
        }
    }
}
