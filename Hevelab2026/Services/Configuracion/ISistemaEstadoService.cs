using Hevelab2026.Models.Configuracion;

namespace Hevelab2026.Services.Configuracion
{
    public interface ISistemaEstadoService
    {
        Task<SistemaEstadoViewModel> ObtenerEstadoAsync();
    }
}
