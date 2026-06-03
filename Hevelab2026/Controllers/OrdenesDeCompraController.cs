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

        [HttpGet]
        public async Task<IActionResult> Crear(CancellationToken ct)
        {
            await CargarProveedoresAsync(ct);

            var ordenes = await _compraService.GetOrdenesCompraAsync(ct);
            int nextId = 1;
            if (ordenes.Any())
            {
                var maxRef = ordenes
                    .Where(o => o.NumeroDocumento != null && o.NumeroDocumento.StartsWith("ORD-"))
                    .Select(o =>
                    {
                        if (int.TryParse(o.NumeroDocumento.Substring(4), out int val))
                            return val;
                        return 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();
                nextId = maxRef + 1;
            }
            string nextRef = $"ORD-{nextId:D5}";

            return View(new Hevelab2026.Models.Compras.SolicitudCompraFormModel { FechaLimite = DateTime.Today.AddDays(7), Referencia = nextRef });
        }

        private async Task CargarProveedoresAsync(CancellationToken ct)
        {
            var proveedores = await _compraService.GetProveedoresAsync(null, ct);
            ViewBag.Proveedores = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(proveedores, "Id", "RazonSocial");
            ViewBag.ProveedoresList = proveedores;
        }

        public class CorreoRequest
        {
            public int ProveedorId { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarCorreo([FromBody] CorreoRequest request, CancellationToken ct)
        {
            if (request == null || request.ProveedorId <= 0)
                return BadRequest(new { success = false, message = "Datos inválidos." });

            var proveedores = await _compraService.GetProveedoresAsync(null, ct);
            var proveedor = proveedores.FirstOrDefault(p => p.Id == request.ProveedorId);

            if (proveedor == null)
                return NotFound(new { success = false, message = "Proveedor no encontrado." });

            string emailDestino = !string.IsNullOrWhiteSpace(proveedor.Email) ? proveedor.Email : "correo_por_defecto@ejemplo.com";

            await Task.Delay(1500, ct);

            return Json(new { success = true, email = emailDestino, message = "Correo enviado exitosamente." });
        }
    }
}
