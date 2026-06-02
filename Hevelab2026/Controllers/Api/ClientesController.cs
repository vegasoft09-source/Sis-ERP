using Hevelab2026.Common;
using Hevelab2026.DTOs.Socios;
using Hevelab2026.Services.Socios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers.Api;

[Authorize]
public class ClientesController : ApiControllerBase
{
    private readonly ISocioService _service;

    public ClientesController(ISocioService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedQuery query, [FromQuery] string? estado, CancellationToken ct)
    {
        var result = await _service.GetClientesAsync(query, estado, ct);
        return OkApi(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await _service.GetByIdAsync(id, ct);
        return item == null ? NotFound(ApiResponse<object>.Fail("Cliente no encontrado")) : OkApi(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SocioCreateDto dto, CancellationToken ct)
    {
        var created = await _service.CreateClienteAsync(dto, ct: ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<SocioResponseDto>.Ok(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SocioUpdateDto dto, CancellationToken ct)
    {
        var updated = await _service.UpdateClienteAsync(id, dto, ct);
        return OkApi(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.ToggleActivoAsync(id, ct);
        return OkApi<object?>(null, "Estado actualizado");
    }
}
