using Hevelab2026.Models;

namespace Hevelab2026.Services.Ventas;

public interface IPedidoVentaService
{
    Task<IReadOnlyList<Cotizacion>> GetCotizacionesAsync(string? fecha, string? cliente, string? estado, CancellationToken ct = default);
    Task<Cotizacion?> GetCotizacionAsync(int id, CancellationToken ct = default);
    Task<Cotizacion> SaveCotizacionAsync(Cotizacion model, CancellationToken ct = default);
    Task<int> ConvertirAOrdenAsync(int cotizacionId, CancellationToken ct = default);
}
