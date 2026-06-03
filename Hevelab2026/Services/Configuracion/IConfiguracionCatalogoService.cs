using Hevelab2026.Models.Configuracion;

namespace Hevelab2026.Services.Configuracion
{
    public interface IConfiguracionCatalogoService
    {
        Task<IReadOnlyList<CatalogoItem>> ObtenerPaisesAsync();
        Task<IReadOnlyList<CatalogoItem>> ObtenerMonedasAsync();
        Task<IReadOnlyList<CatalogoItem>> ObtenerDepartamentosAsync(int? paisId);
    }
}
