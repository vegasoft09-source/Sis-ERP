using System.Security.Claims;
using Hevelab2026.Models.Configuracion;
using Hevelab2026.Services.Configuracion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hevelab2026.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class ConfiguracionController : Controller
    {
        private readonly IEmpresaConfigService _empresaService;
        private readonly IConfiguracionCatalogoService _catalogoService;
        private readonly ISistemaEstadoService _estadoService;
        private readonly ILogger<ConfiguracionController> _logger;

        private static readonly string[] LogoPermitidos = { ".png", ".jpg", ".jpeg", ".svg", ".webp" };
        private const int MaxLogoBytes = 2 * 1024 * 1024;

        public ConfiguracionController(
            IEmpresaConfigService empresaService,
            IConfiguracionCatalogoService catalogoService,
            ISistemaEstadoService estadoService,
            ILogger<ConfiguracionController> logger)
        {
            _empresaService = empresaService;
            _catalogoService = catalogoService;
            _estadoService = estadoService;
            _logger = logger;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? tab = ConfiguracionTabs.Empresa)
        {
            var empresaId = ObtenerEmpresaId();
            if (empresaId <= 0)
            {
                TempData["ConfigError"] = "No se pudo identificar la empresa de la sesión.";
                return RedirectToAction("Index", "Home");
            }

            if (!ConfiguracionTabs.EsValida(tab))
                tab = ConfiguracionTabs.Empresa;

            var vm = await ConstruirViewModelAsync(empresaId, tab);
            ViewData["Title"] = "Configuración del Sistema";
            ViewData["Breadcrumbs"] = new[] {
                ("Dashboard", "/"),
                ("Configuración", "/Configuracion")
            };
            return View(vm);
        }

        [HttpPost("Empresa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarEmpresa(EmpresaConfigForm empresa, IFormFile? logo)
        {
            empresa.Id = ObtenerEmpresaId();
            if (!ModelState.IsValid)
            {
                TempData["ConfigError"] = "Revise los campos de empresa.";
                return RedirectToAction(nameof(Index), new { tab = ConfiguracionTabs.Empresa });
            }

            var ok = await _empresaService.ActualizarEmpresaAsync(empresa);
            if (logo is { Length: > 0 })
            {
                var logoOk = await ProcesarLogoAsync(empresa.Id, logo);
                if (!logoOk)
                {
                    TempData["ConfigWarning"] = "Datos guardados, pero el logo no pudo procesarse.";
                    return RedirectToAction(nameof(Index), new { tab = ConfiguracionTabs.Empresa });
                }
            }

            TempData[ok ? "ConfigSuccess" : "ConfigError"] = ok
                ? "Información de empresa actualizada."
                : "No se pudo guardar la empresa.";
            return RedirectToAction(nameof(Index), new { tab = ConfiguracionTabs.Empresa });
        }

        [HttpPost("Regional")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarRegional(RegionalConfigForm regional)
        {
            regional.EmpresaId = ObtenerEmpresaId();
            var ok = await _empresaService.ActualizarRegionalAsync(regional);
            TempData[ok ? "ConfigSuccess" : "ConfigError"] = ok
                ? "Configuración regional actualizada."
                : "No se pudo guardar la configuración regional.";
            return RedirectToAction(nameof(Index), new { tab = ConfiguracionTabs.Regional });
        }

        [HttpGet("Logo")]
        public async Task<IActionResult> Logo()
        {
            var empresaId = ObtenerEmpresaId();
            var bytes = await _empresaService.ObtenerLogoAsync(empresaId);
            if (bytes is null)
                return NotFound();
            return File(bytes, "image/png");
        }

        /// <summary>Compatibilidad con enlaces /settings del layout.</summary>
        [HttpGet("/settings")]
        public IActionResult SettingsRedirect(string? tab) =>
            RedirectToAction(nameof(Index), new { tab });

        private async Task<ConfiguracionIndexViewModel> ConstruirViewModelAsync(int empresaId, string tab)
        {
            var empresa = await _empresaService.ObtenerEmpresaAsync(empresaId)
                ?? new EmpresaConfigForm { Id = empresaId };
            var regional = await _empresaService.ObtenerRegionalAsync(empresaId)
                ?? new RegionalConfigForm { EmpresaId = empresaId };

            return new ConfiguracionIndexViewModel
            {
                TabActiva = tab,
                Empresa = empresa,
                Regional = regional,
                Correos = new CorreoConfigForm(),
                Seguridad = new SeguridadConfigForm(),
                Estado = await _estadoService.ObtenerEstadoAsync(),
                Paises = await _catalogoService.ObtenerPaisesAsync(),
                Monedas = await _catalogoService.ObtenerMonedasAsync(),
                Departamentos = await _catalogoService.ObtenerDepartamentosAsync(regional.PaisId)
            };
        }

        private int ObtenerEmpresaId()
        {
            var claim = User.FindFirstValue("EmpresaId");
            return int.TryParse(claim, out var id) ? id : 1;
        }

        private async Task<bool> ProcesarLogoAsync(int empresaId, IFormFile logo)
        {
            var ext = Path.GetExtension(logo.FileName).ToLowerInvariant();
            if (!LogoPermitidos.Contains(ext))
            {
                ModelState.AddModelError(string.Empty, "Formato de logo no permitido.");
                return false;
            }
            if (logo.Length > MaxLogoBytes)
            {
                ModelState.AddModelError(string.Empty, "El logo no debe superar 2 MB.");
                return false;
            }

            await using var ms = new MemoryStream();
            await logo.CopyToAsync(ms);
            return await _empresaService.GuardarLogoAsync(empresaId, ms.ToArray());
        }
    }
}
