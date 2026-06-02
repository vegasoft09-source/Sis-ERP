using Hevelab2026.Models;

namespace Hevelab2026.Services.Productos;

public interface IProductoService
{
    Task<IReadOnlyList<Producto>> GetAllAsync(string? search, CancellationToken ct = default);
    Task<Producto?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<Producto> CreateAsync(Producto model, CancellationToken ct = default);
    Task<Producto> UpdateAsync(string sku, Producto model, CancellationToken ct = default);
}
