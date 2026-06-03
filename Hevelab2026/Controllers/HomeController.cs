using System.Security.Claims;
using Hevelab2026.Models;
using Hevelab2026.Models.Modulos;
using Hevelab2026.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Hevelab2026.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDashboardService _dashboardService;

        public HomeController(ILogger<HomeController> logger, IDashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var empresaId = ObtenerEmpresaId();
            var metrics = await _dashboardService.ObtenerMetricasAsync(empresaId);

            var model = new DashboardViewModel
            {
                Metrics = metrics,
                QuickAccessModules = ModuloRegistry.ToQuickAccessModules(),
                RecentSales = new List<RecentSale>
                {
                    new RecentSale { InvoiceNumber = "FAC-2026-004", Customer = "Inversiones Pizarro S.A.C.", Amount = "$1,250.00", Status = "Pagada", StatusType = "success", Date = "Hoy, 09:30 AM" },
                    new RecentSale { InvoiceNumber = "FAC-2026-003", Customer = "Distribuidora Alfa", Amount = "$840.00", Status = "Pendiente", StatusType = "warning", Date = "Ayer, 04:15 PM" },
                    new RecentSale { InvoiceNumber = "FAC-2026-002", Customer = "Corporación Gamma S.A.", Amount = "$2,100.00", Status = "Pagada", StatusType = "success", Date = "25 May 2026" },
                    new RecentSale { InvoiceNumber = "FAC-2026-001", Customer = "María Delgado Espinoza", Amount = "$350.00", Status = "Vencida", StatusType = "danger", Date = "24 May 2026" }
                },
                RecentActivities = new List<RecentActivity>
                {
                    new RecentActivity { Description = "Usuario Administrador modificó el logotipo del sistema", TimeAgo = "Hace 10 mins", User = "Admin", Type = "info" },
                    new RecentActivity { Description = "Se emitió la factura FAC-2026-004 para Inversiones Pizarro S.A.C.", TimeAgo = "Hace 2 horas", User = "Admin", Type = "success" },
                    new RecentActivity { Description = "Alerta de Almacén: Producto 'Plancha de Acero 1/2' llegó al stock mínimo", TimeAgo = "Hace 4 horas", User = "Sistema", Type = "warning" },
                    new RecentActivity { Description = "Se registró un nuevo cliente corporativo: Distribuidora Alfa", TimeAgo = "Ayer", User = "Admin", Type = "success" }
                }
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private int ObtenerEmpresaId()
        {
            var claim = User.FindFirstValue("EmpresaId");
            return int.TryParse(claim, out var id) && id > 0 ? id : 1;
        }
    }
}
