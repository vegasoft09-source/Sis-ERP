using System.Security.Claims;
using Hevelab2026.DTOs.Auth;
using Hevelab2026.Models.Auth;
using Hevelab2026.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers;

/// <summary>Login web MVC (ruta /Auth/*). No confundir con Controllers.Api.AuthController.</summary>
[Route("Auth")]
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public class AuthController : Controller
{
    private readonly IWebAuthService _webAuth;
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IWebAuthService webAuth, IAuthService authService, ILogger<AuthController> logger)
    {
        _webAuth = webAuth;
        _authService = authService;
        _logger = logger;
    }

    [HttpGet("Login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocal(returnUrl);

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        var sesion = await _webAuth.ValidarCredencialesAsync(model.NombreUsuario, model.Contrasena, ct);
        if (sesion is null)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View(model);
        }

        AuthResponseDto? tokens = null;
        try
        {
            tokens = await _authService.LoginAsync(
                new LoginRequest(model.NombreUsuario, model.Contrasena), ct);
        }
        catch
        {
            // JWT opcional si refresh no está mapeado en MySQL
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, sesion.Id.ToString()),
            new(ClaimTypes.Name, sesion.NombreUsuario),
            new("NombreCompleto", sesion.NombreCompleto),
            new("EmpresaId", sesion.EmpresaId.ToString()),
            new("EmpresaNombre", sesion.EmpresaNombre),
            new(ClaimTypes.Role, sesion.RolNombre),
            new("RolId", sesion.RolId.ToString()),
            new(ClaimTypes.Email, sesion.Correo)
        };
        foreach (var permiso in sesion.Permisos)
            claims.Add(new Claim("Permiso", permiso));

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = model.Recordarme,
                ExpiresUtc = model.Recordarme
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(8)
            });

        if (tokens != null)
        {
            Response.Cookies.Append("hevelab_access_token", tokens.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = tokens.ExpiresAt
            });
        }

        _ = _webAuth.RegistrarUltimoAccesoAsync(sesion.Id, ct);

        _logger.LogInformation("Sesión iniciada: {Usuario} | Empresa: {Empresa}",
            sesion.NombreUsuario, sesion.EmpresaNombre);

        TempData["ShowWelcomeLoader"] = "true";
        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out var userId))
        {
            try { await _authService.LogoutAsync(userId); }
            catch { /* refresh token puede no existir en MySQL */ }
        }

        Response.Cookies.Delete("hevelab_access_token");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }
}
