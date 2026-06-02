using Hevelab2026.Models.Config;

namespace Hevelab2026.Services.Config;

public interface IPerfilService
{
    Task<ConfiguracionPageViewModel> GetConfiguracionAsync(int userId, int empresaId, string tab, CancellationToken ct = default);
    Task<string?> GuardarPerfilAsync(int userId, PerfilViewModel model, IFormFile? foto, CancellationToken ct = default);
    Task GuardarEmpresaAsync(int empresaId, EmpresaConfigViewModel model, CancellationToken ct = default);
    Task RefrescarClaimsAsync(HttpContext httpContext, int userId, CancellationToken ct = default);
}
