using Hevelab2026.Domain.Entities;
using Hevelab2026.DTOs.Auth;

namespace Hevelab2026.Services.Auth;

public interface IJwtTokenService
{
    AuthResponseDto GenerateTokens(Usuario usuario, string rolNombre);
    int? ValidateUserId(string? authorizationHeader);
}
