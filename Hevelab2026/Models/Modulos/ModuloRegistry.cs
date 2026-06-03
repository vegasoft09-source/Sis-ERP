using Hevelab2026.Models;

namespace Hevelab2026.Models.Modulos
{
    /// <summary>
    /// Catálogo central de módulos del ERP. Para agregar uno nuevo, añade una entrada en <see cref="All"/>.
    /// </summary>
    public static class ModuloRegistry
    {
        private static readonly IReadOnlyList<ModuloDef> All = new List<ModuloDef>
        {
            // ── Gestión (maestros) ──
            new()
            {
                Id = "clientes", Name = "Clientes", Category = ModuloCategoria.Gestion, SortOrder = 10,
                Controller = "Clientes",
                Description = "Directorio de clientes, datos comerciales y segmentación.",
                BadgeText = "CRM", BadgeType = "primary",
                IconSvg = IconPaths.Users
            },
            new()
            {
                Id = "proveedores", Name = "Proveedores", Category = ModuloCategoria.Gestion, SortOrder = 20,
                Controller = "Proveedores",
                Description = "Catálogo de proveedores y condiciones de abastecimiento.",
                BadgeText = "Maestro", BadgeType = "secondary",
                IconSvg = IconPaths.Truck
            },

            // ── Comercial (ventas) ──
            new()
            {
                Id = "cotizaciones", Name = "Cotizaciones", Category = ModuloCategoria.Comercial, SortOrder = 30,
                Controller = "Cotizaciones",
                Description = "Elabore y gestione cotizaciones comerciales antes de la venta.",
                BadgeText = "Ventas", BadgeType = "success",
                IconSvg = IconPaths.FileText
            },
            new()
            {
                Id = "ordenes-venta", Name = "Órdenes de Venta", Category = ModuloCategoria.Comercial, SortOrder = 40,
                Controller = "Ordenes",
                Description = "Pedidos confirmados, seguimiento y conversión a factura.",
                BadgeText = "Pipeline", BadgeType = "success",
                IconSvg = IconPaths.ShoppingCart
            },
            new()
            {
                Id = "facturas", Name = "Facturación", Category = ModuloCategoria.Comercial, SortOrder = 50,
                Controller = "Facturas",
                Description = "Emisión de comprobantes, boletas y notas de crédito.",
                BadgeText = "SUNAT", BadgeType = "warning",
                IconSvg = IconPaths.Receipt
            },

            // ── Operaciones (compras + inventario) ──
            new()
            {
                Id = "solicitud-cotizacion", Name = "Solicitudes de Cotización", Category = ModuloCategoria.Operaciones, SortOrder = 60,
                Controller = "SolicitudDeCotizacion",
                Description = "Solicite precios a proveedores antes de comprar.",
                BadgeText = "Compras", BadgeType = "danger",
                IconSvg = IconPaths.Clipboard
            },
            new()
            {
                Id = "ordenes-compra", Name = "Órdenes de Compra", Category = ModuloCategoria.Operaciones, SortOrder = 70,
                Controller = "OrdenesDeCompra",
                Description = "Registre y apruebe órdenes de compra a proveedores.",
                BadgeText = "OC", BadgeType = "danger",
                IconSvg = IconPaths.Package
            },
            new()
            {
                Id = "productos", Name = "Productos", Category = ModuloCategoria.Operaciones, SortOrder = 80,
                Controller = "Producto",
                Description = "Catálogo de productos, SKU, precios y presentaciones.",
                BadgeText = "Catálogo", BadgeType = "warning",
                IconSvg = IconPaths.Box
            },
            new()
            {
                Id = "recepciones", Name = "Recepciones", Category = ModuloCategoria.Operaciones, SortOrder = 90,
                Controller = "RecepcionesView",
                Description = "Ingreso de mercadería al almacén desde compras o transferencias.",
                BadgeText = "Almacén", BadgeType = "warning",
                IconSvg = IconPaths.Inbox
            },

            // ── Finanzas ──
            new()
            {
                Id = "libro-mayor", Name = "Libro Mayor", Category = ModuloCategoria.Finanzas, SortOrder = 100,
                Controller = "LibroMayor",
                Description = "Consulta de cuentas, movimientos y saldos contables.",
                BadgeText = "Contable", BadgeType = "info",
                IconSvg = IconPaths.Book
            },
            new()
            {
                Id = "documentos", Name = "Documentos Contables", Category = ModuloCategoria.Finanzas, SortOrder = 110,
                Controller = "Documentos",
                Description = "Asientos, comprobantes y documentación financiera.",
                BadgeText = "Docs", BadgeType = "info",
                IconSvg = IconPaths.Folder
            },
            new()
            {
                Id = "reporte-contabilidad", Name = "Reporte Contabilidad", Category = ModuloCategoria.Finanzas, SortOrder = 120,
                Controller = "Reportes", Action = "Contabilidad",
                Description = "Ingresos, egresos y flujo contable consolidado.",
                BadgeText = "Reporte", BadgeType = "info",
                IconSvg = IconPaths.BarChart
            },
            new()
            {
                Id = "reporte-ventas", Name = "Reporte Ventas", Category = ModuloCategoria.Finanzas, SortOrder = 130,
                Controller = "Reportes", Action = "Ventas",
                Description = "Análisis de ventas por vendedor, cliente y periodo.",
                BadgeText = "Analítica", BadgeType = "info",
                IconSvg = IconPaths.TrendingUp
            },

            // ── RRHH ──
            new()
            {
                Id = "reporte-empleados", Name = "Reporte Empleados", Category = ModuloCategoria.Rrhh, SortOrder = 140,
                Controller = "Reportes", Action = "Empleados",
                Description = "Plantilla, desempeño y estado del personal.",
                BadgeText = "RRHH", BadgeType = "primary",
                IconSvg = IconPaths.UserCheck
            },

            // ── Sistema ──
            new()
            {
                Id = "sire", Name = "SIRE / SUNAT", Category = ModuloCategoria.Sistema, SortOrder = 150,
                Controller = "Sire",
                Description = "Integración y consultas tributarias electrónicas.",
                BadgeText = "Tributario", BadgeType = "warning",
                IconSvg = IconPaths.Shield
            },
            new()
            {
                Id = "reporte-inventario", Name = "Reporte Inventario", Category = ModuloCategoria.Sistema, SortOrder = 160,
                Controller = "Reportes", Action = "Inventario",
                Description = "Valorización de stock y rotación de productos.",
                BadgeText = "Stock", BadgeType = "secondary",
                IconSvg = IconPaths.Layers
            },
            new()
            {
                Id = "reporte-compras", Name = "Reporte Compras", Category = ModuloCategoria.Sistema, SortOrder = 170,
                Controller = "Reportes", Action = "Compras",
                Description = "Gasto por proveedor, categoría y periodo.",
                BadgeText = "Egresos", BadgeType = "secondary",
                IconSvg = IconPaths.PieChart
            },
            new()
            {
                Id = "configuracion", Name = "Configuración", Category = ModuloCategoria.Sistema, SortOrder = 180,
                Controller = "Home", Action = "Privacy",
                Description = "Ajustes de empresa, usuarios y preferencias del sistema.",
                BadgeText = "Ajustes", BadgeType = "secondary",
                IconSvg = IconPaths.Settings,
                Enabled = true
            }
        };

        public static int Total => All.Count(m => m.Enabled);

        public static IReadOnlyList<ModuloDef> GetAll() =>
            All.Where(m => m.Enabled).OrderBy(m => m.SortOrder).ToList();

        public static IReadOnlyList<ModuloDef> GetByCategory(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId) || categoryId == ModuloCategoria.Todos)
                return GetAll();

            return GetAll().Where(m => m.Category == categoryId).ToList();
        }

        public static List<QuickAccessModule> ToQuickAccessModules() =>
            GetAll().Select(m => new QuickAccessModule
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Controller = m.Controller,
                Action = m.Action,
                Category = m.Category,
                IconSvg = m.IconSvg,
                BadgeText = m.BadgeText,
                BadgeType = m.BadgeType
            }).ToList();

        private static class IconPaths
        {
            public const string Users = @"<path d='M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2'/><circle cx='9' cy='7' r='4'/><path d='M23 21v-2a4 4 0 0 0-3-3.87'/><path d='M16 3.13a4 4 0 0 1 0 7.75'/>";
            public const string Truck = @"<rect x='1' y='3' width='15' height='13'/><polygon points='16 8 20 8 23 11 23 16 16 16 16 8'/><circle cx='5.5' cy='18.5' r='2.5'/><circle cx='18.5' cy='18.5' r='2.5'/>";
            public const string FileText = @"<path d='M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z'/><polyline points='14 2 14 8 20 8'/><line x1='16' y1='13' x2='8' y2='13'/><line x1='16' y1='17' x2='8' y2='17'/>";
            public const string ShoppingCart = @"<circle cx='9' cy='21' r='1'/><circle cx='20' cy='21' r='1'/><path d='M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6'/>";
            public const string Receipt = @"<path d='M4 2v20l2-1 2 1 2-1 2 1 2-1 2 1 2-1 2 1V2l-2 1-2-1-2 1-2-1-2 1-2-1-2 1Z'/><path d='M8 7h8'/><path d='M8 11h8'/>";
            public const string Clipboard = @"<path d='M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2'/><rect x='8' y='2' width='8' height='4' rx='1' ry='1'/>";
            public const string Package = @"<path d='M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z'/><polyline points='3.27 6.96 12 12.01 20.73 6.96'/><line x1='12' y1='22.08' x2='12' y2='12'/>";
            public const string Box = @"<path d='M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z'/><polyline points='7.5 4.21 12 6.81 16.5 4.21'/>";
            public const string Inbox = @"<polyline points='22 12 16 12 14 15 10 15 8 12 2 12'/><path d='M22 7l-7 7-4-4-7 7V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z'/>";
            public const string Book = @"<path d='M4 19.5A2.5 2.5 0 0 1 6.5 17H20'/><path d='M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z'/>";
            public const string Folder = @"<path d='M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z'/>";
            public const string BarChart = @"<line x1='18' y1='20' x2='18' y2='10'/><line x1='12' y1='20' x2='12' y2='4'/><line x1='6' y1='20' x2='6' y2='14'/>";
            public const string TrendingUp = @"<polyline points='23 6 13.5 15.5 8.5 10.5 1 18'/><polyline points='17 6 23 6 23 12'/>";
            public const string UserCheck = @"<path d='M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2'/><circle cx='8.5' cy='7' r='4'/><polyline points='17 11 19 13 23 9'/>";
            public const string Shield = @"<path d='M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z'/>";
            public const string Layers = @"<polygon points='12 2 2 7 12 12 22 7 12 2'/><polyline points='2 17 12 22 22 17'/><polyline points='2 12 12 17 22 12'/>";
            public const string PieChart = @"<path d='M21.21 15.89A10 10 0 1 1 8 2.83'/><path d='M22 12A10 10 0 0 0 12 2v10z'/>";
            public const string Settings = @"<circle cx='12' cy='12' r='3'/><path d='M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 4.68 15a1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 9 4.68a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 19.4 9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z'/>";
        }
    }
}
