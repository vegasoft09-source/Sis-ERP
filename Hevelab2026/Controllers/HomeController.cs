using Hevelab2026.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Hevelab2026.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new DashboardViewModel
            {
                Metrics = new List<MetricCard>
                {
                    new MetricCard
                    {
                        Title = "Ventas Mensuales",
                        Value = "$24,850.00",
                        TrendText = "+12.4% vs mes anterior",
                        TrendType = "up",
                        ThemeColor = "primary",
                        IconSvg = @"<circle cx='9' cy='21' r='1'/><circle cx='20' cy='21' r='1'/><path d='M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6'/>"
                    },
                    new MetricCard
                    {
                        Title = "Nuevos Clientes",
                        Value = "184",
                        TrendText = "+8.2% vs mes anterior",
                        TrendType = "up",
                        ThemeColor = "success",
                        IconSvg = @"<path d='M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2'/><circle cx='9' cy='7' r='4'/><path d='M23 21v-2a4 4 0 0 0-3-3.87'/><path d='M16 3.13a4 4 0 0 1 0 7.75'/>"
                    },
                    new MetricCard
                    {
                        Title = "Stock Almacén",
                        Value = "1,420 u.",
                        TrendText = "12 productos stock crítico",
                        TrendType = "warning",
                        ThemeColor = "warning",
                        IconSvg = @"<path d='M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z'/><polyline points='7.5 4.21 12 6.81 16.5 4.21'/>"
                    },
                    new MetricCard
                    {
                        Title = "Facturas Cobrar",
                        Value = "$3,420.00",
                        TrendText = "8 facturas pendientes",
                        TrendType = "danger",
                        ThemeColor = "danger",
                        IconSvg = @"<line x1='18' y1='20' x2='18' y2='10'/><line x1='12' y1='20' x2='12' y2='4'/><line x1='6' y1='20' x2='6' y2='14'/>"
                    }
                },
                QuickAccessModules = new List<QuickAccessModule>
                {
                    new QuickAccessModule
                    {
                        Name = "Ventas e Ingresos",
                        Description = "Gestione cotizaciones, registre facturas electrónicas y visualice el historial de ventas.",
                        Controller = "Ventas",
                        BadgeText = "Módulo Listo",
                        BadgeType = "success",
                        IconSvg = @"<circle cx='9' cy='21' r='1'/><circle cx='20' cy='21' r='1'/><path d='M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6'/>"
                    },
                    new QuickAccessModule
                    {
                        Name = "Inventario y Stock",
                        Description = "Controle almacenes, actualice existencias, configure stock mínimo y gestione códigos.",
                        Controller = "Inventario",
                        BadgeText = "12 Críticos",
                        BadgeType = "warning",
                        IconSvg = @"<path d='M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z'/><polyline points='7.5 4.21 12 6.81 16.5 4.21'/>"
                    },
                    new QuickAccessModule
                    {
                        Name = "Clientes y CRM",
                        Description = "Directorio unificado de clientes, historial de transacciones y estados de cuenta rápidos.",
                        Controller = "Clientes",
                        BadgeText = "Nuevo",
                        BadgeType = "primary",
                        IconSvg = @"<path d='M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2'/><circle cx='9' cy='7' r='4'/><path d='M23 21v-2a4 4 0 0 0-3-3.87'/><path d='M16 3.13a4 4 0 0 1 0 7.75'/>"
                    },
                    new QuickAccessModule
                    {
                        Name = "Compras y Gastos",
                        Description = "Registre adquisiciones a proveedores, controle compras y programe cuentas por pagar.",
                        Controller = "Compras",
                        BadgeText = "Egresos",
                        BadgeType = "danger",
                        IconSvg = @"<path d='M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z'/><line x1='7' y1='7' x2='7.01' y2='7'/>"
                    },
                    new QuickAccessModule
                    {
                        Name = "Finanzas e Informes",
                        Description = "Visualice balances de caja, analice pérdidas y ganancias y exporte reportes ejecutivos.",
                        Controller = "Finanzas",
                        BadgeText = "Analítica",
                        BadgeType = "info",
                        IconSvg = @"<line x1='18' y1='20' x2='18' y2='10'/><line x1='12' y1='20' x2='12' y2='4'/><line x1='6' y1='20' x2='6' y2='14'/>"
                    },
                    new QuickAccessModule
                    {
                        Name = "Configuración",
                        Description = "Personalice los datos de su empresa, configure logotipos, monedas, usuarios y roles del sistema.",
                        Controller = "Settings",
                        BadgeText = "General",
                        BadgeType = "secondary",
                        IconSvg = @"<circle cx='12' cy='12' r='3'/><path d='M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 4.68 15a1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 9 4.68a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 19.4 9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z'/>"
                    }
                },
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
    }
}
