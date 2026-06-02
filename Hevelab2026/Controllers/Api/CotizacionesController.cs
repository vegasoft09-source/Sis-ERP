using Hevelab2026.Models;
using Hevelab2026.Services.Ventas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers.Api;

[Authorize]
public class CotizacionesController : ApiControllerBase
{
    private readonly IPedidoVentaService _service;

    public CotizacionesController(IPedidoVentaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? fecha, [FromQuery] string? cliente, [FromQuery] string? estado, CancellationToken ct)
    {
        var items = await _service.GetCotizacionesAsync(fecha, cliente, estado, ct);
        return OkApi(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await _service.GetCotizacionAsync(id, ct);
        return item == null ? NotFound() : OkApi(item);
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] Cotizacion model, CancellationToken ct)
    {
        var saved = await _service.SaveCotizacionAsync(model, ct);
        return OkApi(saved);
    }

    [HttpPost("{id:int}/convertir-orden")]
    public async Task<IActionResult> Convertir(int id, CancellationToken ct)
    {
        var ordenId = await _service.ConvertirAOrdenAsync(id, ct);
        return OkApi(new { ordenId });
    }
}
