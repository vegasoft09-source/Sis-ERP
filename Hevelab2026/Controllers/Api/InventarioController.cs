using Hevelab2026.Models;
using Hevelab2026.Services.Inventario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers.Api;

[Authorize]
public class InventarioController : ApiControllerBase
{
    private readonly IInventarioService _service;

    public InventarioController(IInventarioService service) => _service = service;

    [HttpGet("recepciones")]
    public async Task<IActionResult> Recepciones([FromQuery] string? estado, CancellationToken ct) =>
        OkApi(await _service.GetRecepcionesAsync(estado, ct));

    [HttpGet("recepciones/{referencia}")]
    public async Task<IActionResult> Recepcion(string referencia, CancellationToken ct)
    {
        var item = await _service.GetRecepcionAsync(referencia, ct);
        return item == null ? NotFound() : OkApi(item);
    }

    [HttpPost("recepciones")]
    public async Task<IActionResult> CrearRecepcion([FromBody] RecepcionModel model, CancellationToken ct)
    {
        var created = await _service.CrearRecepcionAsync(model, ct);
        return OkApi(created);
    }
}
