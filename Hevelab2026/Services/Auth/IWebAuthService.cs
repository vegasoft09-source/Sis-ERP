using Hevelab2026.Models.Auth;

namespace Hevelab2026.Services.Auth;

public interface IWebAuthService
{
    Task<UsuarioSesion?> ValidarCredencialesAsync(string nombreUsuario, string contrasena, CancellationToken ct = default);
    Task RegistrarUltimoAccesoAsync(int usuarioId, CancellationToken ct = default);
}
