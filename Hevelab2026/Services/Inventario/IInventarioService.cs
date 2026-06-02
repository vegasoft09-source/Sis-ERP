using Hevelab2026.Models;

namespace Hevelab2026.Services.Inventario;

public interface IInventarioService
{
    Task<IReadOnlyList<RecepcionModel>> GetRecepcionesAsync(string? estado, CancellationToken ct = default);
    Task<RecepcionModel?> GetRecepcionAsync(string referencia, CancellationToken ct = default);
    Task<RecepcionModel> CrearRecepcionAsync(RecepcionModel model, CancellationToken ct = default);
}
