using Hevelab2026.Models.Configuracion;

namespace Hevelab2026.Services.Configuracion
{
    public interface IEmpresaConfigService
    {
        Task<EmpresaConfigForm?> ObtenerEmpresaAsync(int empresaId);
        Task<RegionalConfigForm?> ObtenerRegionalAsync(int empresaId);
        Task<bool> ActualizarEmpresaAsync(EmpresaConfigForm form);
        Task<bool> ActualizarRegionalAsync(RegionalConfigForm form);
        Task<bool> GuardarLogoAsync(int empresaId, byte[] contenido);
        Task<byte[]?> ObtenerLogoAsync(int empresaId);
    }
}
