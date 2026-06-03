using System.Security.Claims;
using Hevelab2026.Models.Auth;
using Hevelab2026.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers
{
    /// <summary>
    /// Controlador modular de autenticación.
    /// Rutas: /Auth/Login  |  /Auth/Logout
    /// </summary>
    public class AuthController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUsuarioService usuarioService, ILogger<AuthController> logger)
        {
            _usuarioService = usuarioService;
            _logger         = logger;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /Auth/Login
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Si ya está autenticado, redirigir al dashboard
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToLocal(returnUrl);

            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /Auth/Login
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Intentar autenticar contra la BD
            var sesion = await _usuarioService.AutenticarAsync(model.NombreUsuario, model.Contrasena);

            if (sesion is null)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return View(model);
            }

            // Construir los Claims para la cookie
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, sesion.Id.ToString()),
                new(ClaimTypes.Name,           sesion.NombreUsuario),
                new("NombreCompleto",          sesion.NombreCompleto),
                new("EmpresaId",               sesion.EmpresaId.ToString()),
                new("EmpresaNombre",           sesion.EmpresaNombre),
                new(ClaimTypes.Role,           sesion.RolNombre),
                new("RolId",                   sesion.RolId.ToString()),
                new("Correo",                  sesion.Correo),
            };

            // Agregar cada permiso como claim independiente
            foreach (var permiso in sesion.Permisos)
                claims.Add(new Claim("Permiso", permiso));

            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = model.Recordarme,
                ExpiresUtc   = model.Recordarme
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProps);

            // Registrar último acceso (sin bloquear respuesta)
            _ = _usuarioService.RegistrarUltimoAccesoAsync(sesion.Id);

            _logger.LogInformation("Sesión iniciada: {Usuario} | Empresa: {Empresa}",
                sesion.NombreUsuario, sesion.EmpresaNombre);

            // Trigger brief welcome loader naming the user
            TempData["ShowWelcomeLoader"] = "true";

            return RedirectToLocal(model.ReturnUrl);
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /Auth/Logout
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var usuario = User?.Identity?.Name ?? "Desconocido";
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("Sesión cerrada: {Usuario}", usuario);
            return RedirectToAction(nameof(Login));
        }

        // ─────────────────────────────────────────────────────────────────────
        // HELPER
        // ─────────────────────────────────────────────────────────────────────
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
