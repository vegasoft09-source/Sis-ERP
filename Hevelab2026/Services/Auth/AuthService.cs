using Hevelab2026.Common.Exceptions;
using Hevelab2026.Data;
using Hevelab2026.DTOs.Auth;
using Hevelab2026.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hevelab2026.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtTokenService _jwt;
    private readonly ApplicationDbContext _db;

    public AuthService(IUnitOfWork uow, IJwtTokenService jwt, ApplicationDbContext db)
    {
        _uow = uow;
        _jwt = jwt;
        _db = db;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario == request.NombreUsuario && u.Activo, ct);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Contrasena, usuario.ContrasenaHash))
            throw new UnauthorizedException("Credenciales inválidas");

        var response = _jwt.GenerateTokens(usuario, usuario.Rol?.Nombre ?? "Usuario");
        await _uow.SaveChangesAsync(ct);
        return response;
    }

    public async Task<AuthResponseDto> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u =>
                u.RefreshToken == request.RefreshToken &&
                u.RefreshTokenExpira > DateTime.UtcNow &&
                u.Activo, ct);

        if (usuario == null)
            throw new UnauthorizedException("Refresh token inválido o expirado");

        var response = _jwt.GenerateTokens(usuario, usuario.Rol?.Nombre ?? "Usuario");
        await _uow.SaveChangesAsync(ct);
        return response;
    }

    public async Task LogoutAsync(int userId, CancellationToken ct = default)
    {
        var usuario = await _uow.Usuarios.GetByIdAsync(userId, ct);
        if (usuario == null) return;
        usuario.RefreshToken = null;
        usuario.RefreshTokenExpira = null;
        await _uow.SaveChangesAsync(ct);
    }
}
