using Microsoft.AspNetCore.Mvc;
using Hevelab2026.Models;
using Hevelab2026.Services.Inventario;

namespace Hevelab2026.Controllers
{
    public class RecepcionesViewController : Controller
    {
        private readonly IInventarioService _inventarioService;

        public RecepcionesViewController(IInventarioService inventarioService)
        {
            _inventarioService = inventarioService;
        }

        public async Task<IActionResult> Index(string? estado, CancellationToken ct)
        {
            var lista = await _inventarioService.GetRecepcionesAsync(estado, ct);
            return View(lista.ToList());
        }

        [HttpGet]
        public IActionResult Crear()
        {
            var nuevoModelo = new RecepcionModel
            {
                FechaProgramada = DateTime.Now,
                FechaLimite = DateTime.Now.AddDays(2)
            };
            return View(nuevoModelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(RecepcionModel modelo, CancellationToken ct)
        {
            if (ModelState.IsValid)
            {
                await _inventarioService.CrearRecepcionAsync(modelo, ct);
                return RedirectToAction(nameof(Index));
            }
            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(string id, CancellationToken ct)
        {
            var detalle = await _inventarioService.GetRecepcionAsync(id, ct);
            if (detalle == null) return NotFound();
            return View(detalle);
        }
    }
}
