using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Hevelab2026.Domain.Entities;
using Hevelab2026.DTOs.Auth;
using Microsoft.IdentityModel.Tokens;

namespace Hevelab2026.Services.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config) => _config = config;

    public AuthResponseDto GenerateTokens(Usuario usuario, string rolNombre)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var minutes = int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "60");
        var expires = DateTime.UtcNow.AddMinutes(minutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new(ClaimTypes.Email, usuario.Correo ?? string.Empty),
            new(ClaimTypes.Role, rolNombre),
            new("empresa_id", usuario.EmpresaId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshDays = int.Parse(_config["Jwt:RefreshTokenDays"] ?? "7");
        usuario.RefreshToken = refresh;
        usuario.RefreshTokenExpira = DateTime.UtcNow.AddDays(refreshDays);
        usuario.UltimoAcceso = DateTime.UtcNow;

        return new AuthResponseDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refresh,
            ExpiresAt = expires,
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre ?? string.Empty,
                Apellido = usuario.Apellido ?? string.Empty,
                NombreUsuario = usuario.NombreUsuario,
                Correo = usuario.Correo ?? string.Empty,
                Rol = rolNombre,
                EmpresaId = usuario.EmpresaId
            }
        };
    }

    public int? ValidateUserId(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            return null;
        var token = authorizationHeader["Bearer ".Length..];
        var handler = new JwtSecurityTokenHandler();
        try
        {
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!))
            }, out var validated);
            var jwt = (JwtSecurityToken)validated;
            var id = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(id, out var uid) ? uid : null;
        }
        catch { return null; }
    }
}
