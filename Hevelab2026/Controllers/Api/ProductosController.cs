using Hevelab2026.Models;
using Hevelab2026.Services.Productos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers.Api;

[Authorize]
public class ProductosController : ApiControllerBase
{
    private readonly IProductoService _service;

    public ProductosController(IProductoService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, CancellationToken ct)
    {
        var items = await _service.GetAllAsync(search, ct);
        return OkApi(items);
    }

    [HttpGet("{sku}")]
    public async Task<IActionResult> GetBySku(string sku, CancellationToken ct)
    {
        var item = await _service.GetBySkuAsync(sku, ct);
        return item == null ? NotFound() : OkApi(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Producto model, CancellationToken ct)
    {
        var created = await _service.CreateAsync(model, ct);
        return OkApi(created);
    }

    [HttpPut("{sku}")]
    public async Task<IActionResult> Update(string sku, [FromBody] Producto model, CancellationToken ct)
    {
        var updated = await _service.UpdateAsync(sku, model, ct);
        return OkApi(updated);
    }
}
