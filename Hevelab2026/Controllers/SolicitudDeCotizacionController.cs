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

        var solicitudes = await _compraService.GetSolicitudesAsync(ct);
        int nextId = 1;
        if (solicitudes.Any())
        {
            var maxRef = solicitudes
                .Where(s => s.Referencia != null && s.Referencia.StartsWith("REQ-"))
                .Select(s =>
                {
                    if (int.TryParse(s.Referencia.Substring(4), out int val))
                        return val;
                    return 0;
                })
                .DefaultIfEmpty(0)
                .Max();
            nextId = maxRef + 1;
        }
        string nextRef = $"REQ-{nextId:D5}";

        return View(new SolicitudCompraFormModel { FechaLimite = DateTime.Today.AddDays(7), Referencia = nextRef });
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

        // Simulación de envío (Aquí iría la lógica de SmtpClient real)
        await Task.Delay(1500, ct);

        return Json(new { success = true, email = emailDestino, message = "Correo enviado exitosamente." });
    }
}
