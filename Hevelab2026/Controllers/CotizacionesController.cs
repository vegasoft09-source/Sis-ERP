using Microsoft.AspNetCore.Mvc;
using Hevelab2026.Models;
using Hevelab2026.Services.Ventas;

namespace Hevelab2026.Controllers
{
    public class CotizacionesController : Controller
    {
        private readonly IPedidoVentaService _pedidoVentaService;

        public CotizacionesController(IPedidoVentaService pedidoVentaService)
        {
            _pedidoVentaService = pedidoVentaService;
        }

        public async Task<IActionResult> Index(string fecha, string cliente, string estado, CancellationToken ct)
        {
            var datos = await _pedidoVentaService.GetCotizacionesAsync(fecha, cliente, estado, ct);
            ViewBag.Fecha = fecha;
            ViewBag.Cliente = cliente;
            ViewBag.Estado = estado;
            return View(datos.ToList());
        }

        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var cotizacion = await _pedidoVentaService.GetCotizacionAsync(id, ct);
            if (cotizacion == null) return NotFound();
            return View(cotizacion);
        }

        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var cotizacion = await _pedidoVentaService.GetCotizacionAsync(id, ct);
            if (cotizacion == null) return NotFound();
            return View(cotizacion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Cotizacion model, CancellationToken ct)
        {
            await _pedidoVentaService.SaveCotizacionAsync(model, ct);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cotizacion model, CancellationToken ct)
        {
            await _pedidoVentaService.SaveCotizacionAsync(model, ct);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ConvertirAOrden(int id, CancellationToken ct)
        {
            await _pedidoVentaService.ConvertirAOrdenAsync(id, ct);
            return RedirectToAction("Create", "Ordenes");
        }
    }
}
