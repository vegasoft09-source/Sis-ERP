using Hevelab2026.DTOs.Auth;
using Hevelab2026.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers.Api;

/// <summary>API JWT — rutas bajo /api/v1/Auth. Login web: /Auth/Login</summary>
[Route("api/v1/Auth")]
[ApiExplorerSettings(GroupName = "api")]
public class AuthApiController : ApiControllerBase
{
    private readonly IAuthService _auth;

    public AuthApiController(IAuthService auth) => _auth = auth;

    /// <summary>El login web es MVC en /Auth/Login. La API solo acepta POST JSON.</summary>
    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult LoginGet() => Redirect("/Auth/Login");

    [HttpPost("login")]
    [AllowAnonymous]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> Login([FromBody] LoginRequest? request, CancellationToken ct)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.NombreUsuario))
            return FailApi("Envíe JSON: { \"nombreUsuario\": \"...\", \"contrasena\": \"...\" }", StatusCodes.Status400BadRequest);

        var result = await _auth.LoginAsync(request, ct);
        Response.Cookies.Append("hevelab_access_token", result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = result.ExpiresAt
        });
        return OkApi(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _auth.RefreshAsync(request, ct);
        return OkApi(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var id = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await _auth.LogoutAsync(id, ct);
        return OkApi<object?>(null, "Sesión cerrada");
    }
}
