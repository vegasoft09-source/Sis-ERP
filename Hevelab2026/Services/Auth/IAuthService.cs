using Hevelab2026.DTOs.Auth;

namespace Hevelab2026.Services.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task LogoutAsync(int userId, CancellationToken ct = default);
}
