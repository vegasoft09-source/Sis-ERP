using Hevelab2026.Services.Compras;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers.Api;

[Authorize]
public class ComprasController : ApiControllerBase
{
    private readonly ICompraService _service;

    public ComprasController(ICompraService service) => _service = service;

    [HttpGet("solicitudes")]
    public async Task<IActionResult> Solicitudes(CancellationToken ct) =>
        OkApi(await _service.GetSolicitudesAsync(ct));

    [HttpGet("ordenes")]
    public async Task<IActionResult> Ordenes(CancellationToken ct) =>
        OkApi(await _service.GetOrdenesCompraAsync(ct));

    [HttpGet("proveedores")]
    public async Task<IActionResult> Proveedores([FromQuery] string? search, CancellationToken ct) =>
        OkApi(await _service.GetProveedoresAsync(search, ct));
}
