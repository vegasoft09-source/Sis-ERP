using Hevelab2026.Models;
using Hevelab2026.Services.Productos;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers
{
    public class ProductoController : Controller
    {
        private readonly IProductoService _productoService;

        public ProductoController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        public async Task<IActionResult> Index(string? busqueda, CancellationToken ct)
        {
            var productos = await _productoService.GetAllAsync(busqueda, ct);
            return View(productos.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Buscar(string? search, CancellationToken ct)
        {
            var productos = await _productoService.GetAllAsync(search, ct);
            return Json(productos);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto, CancellationToken ct)
        {
            await _productoService.CreateAsync(producto, ct);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id, CancellationToken ct)
        {
            var producto = await _productoService.GetBySkuAsync(id, ct);
            if (producto == null) return NotFound();
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Producto producto, CancellationToken ct)
        {
            await _productoService.UpdateAsync(id, producto, ct);
            return RedirectToAction(nameof(Index));
        }
    }
}
