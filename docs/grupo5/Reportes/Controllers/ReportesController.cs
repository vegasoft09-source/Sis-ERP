using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hevelab2026.Controllers
{
    public class ReportesController : Controller
    {
        // Modelos locales para Reportes
        public class RepVenta { public string Fecha { get; set; } = ""; public string Vendedor { get; set; } = ""; public string Cliente { get; set; } = ""; public decimal Monto { get; set; } }
        public class RepCompra { public string Fecha { get; set; } = ""; public string Proveedor { get; set; } = ""; public string Categoria { get; set; } = ""; public decimal Total { get; set; } }
        public class RepInventario { public string Sku { get; set; } = ""; public string Producto { get; set; } = ""; public int Stock { get; set; } public decimal ValorUnitario { get; set; } }
        public class RepContabilidad { public string Fecha { get; set; } = ""; public string Concepto { get; set; } = ""; public string Tipo { get; set; } = ""; public decimal Monto { get; set; } }
        public class RepEmpleado { public string Codigo { get; set; } = ""; public string Nombre { get; set; } = ""; public string Departamento { get; set; } = ""; public string Estado { get; set; } = ""; public decimal Desempeno { get; set; } }

        // Mock Data
        private static List<RepVenta> ventas = new List<RepVenta>
        {
            new RepVenta { Fecha = "2024-05-01", Vendedor = "Carlos Ruiz", Cliente = "Construcciones S.A.C.", Monto = 12000.50m },
            new RepVenta { Fecha = "2024-05-02", Vendedor = "Ana Gomez", Cliente = "Logística Global", Monto = 8500.00m },
            new RepVenta { Fecha = "2024-05-05", Vendedor = "Luis Torres", Cliente = "Inversiones Sur", Monto = 23400.20m },
            new RepVenta { Fecha = "2024-05-10", Vendedor = "Carlos Ruiz", Cliente = "Tech Avanzada", Monto = 4500.00m },
            new RepVenta { Fecha = "2024-05-12", Vendedor = "Luis Torres", Cliente = "Agroexportadora", Monto = 31200.00m }
        };

        private static List<RepCompra> compras = new List<RepCompra>
        {
            new RepCompra { Fecha = "2024-05-03", Proveedor = "Aceros Arequipa", Categoria = "Materiales", Total = 15000.00m },
            new RepCompra { Fecha = "2024-05-08", Proveedor = "Ferretería Industrial", Categoria = "Herramientas", Total = 3200.50m },
            new RepCompra { Fecha = "2024-05-11", Proveedor = "Sistemas IT SAC", Categoria = "Equipos", Total = 8900.00m },
            new RepCompra { Fecha = "2024-05-15", Proveedor = "Aceros Arequipa", Categoria = "Materiales", Total = 22000.00m }
        };

        private static List<RepInventario> inventario = new List<RepInventario>
        {
            new RepInventario { Sku = "MAT-0092", Producto = "Vigueta de Acero Estructural", Stock = 150, ValorUnitario = 1240.00m },
            new RepInventario { Sku = "CON-0155", Producto = "Concreto Premezclado f'c=250", Stock = 500, ValorUnitario = 2150.00m },
            new RepInventario { Sku = "HER-0021", Producto = "Taladro Percutor Industrial", Stock = 12, ValorUnitario = 850.00m },
            new RepInventario { Sku = "MAT-0010", Producto = "Cemento Portland Tipo I", Stock = 1200, ValorUnitario = 28.50m },
            new RepInventario { Sku = "PNT-0050", Producto = "Pintura Epóxica 1 Galón", Stock = 45, ValorUnitario = 120.00m }
        };

        private static List<RepContabilidad> contabilidad = new List<RepContabilidad>
        {
            new RepContabilidad { Fecha = "2024-05-01", Concepto = "Pago Factura Venta S001", Tipo = "Ingreso", Monto = 12000.50m },
            new RepContabilidad { Fecha = "2024-05-02", Concepto = "Pago Planilla Abril", Tipo = "Egreso", Monto = 18500.00m },
            new RepContabilidad { Fecha = "2024-05-03", Concepto = "Compra Aceros Arequipa", Tipo = "Egreso", Monto = 15000.00m },
            new RepContabilidad { Fecha = "2024-05-05", Concepto = "Cobro Factura Venta S003", Tipo = "Ingreso", Monto = 23400.20m },
            new RepContabilidad { Fecha = "2024-05-10", Concepto = "Pago Servicios (Luz/Agua)", Tipo = "Egreso", Monto = 2100.00m }
        };

        private static List<RepEmpleado> empleados = new List<RepEmpleado>
        {
            new RepEmpleado { Codigo = "EMP-001", Nombre = "Juan Pérez", Departamento = "Ventas", Estado = "Activo", Desempeno = 85.5m },
            new RepEmpleado { Codigo = "EMP-002", Nombre = "María García", Departamento = "Administración", Estado = "Activo", Desempeno = 92.0m },
            new RepEmpleado { Codigo = "EMP-003", Nombre = "Carlos Ruiz", Departamento = "Ventas", Estado = "Inactivo", Desempeno = 78.0m },
            new RepEmpleado { Codigo = "EMP-004", Nombre = "Ana Gomez", Departamento = "Logística", Estado = "Activo", Desempeno = 88.5m },
            new RepEmpleado { Codigo = "EMP-005", Nombre = "Roberto Sánchez", Departamento = "TI", Estado = "Activo", Desempeno = 95.0m }
        };

        public IActionResult Index()
        {
            return RedirectToAction("Ventas");
        }

        public IActionResult Ventas()
        {
            ViewBag.ActiveMenu = "Ventas";
            return View(ventas);
        }

        public IActionResult Compras()
        {
            ViewBag.ActiveMenu = "Compras";
            return View(compras);
        }

        public IActionResult Inventario()
        {
            ViewBag.ActiveMenu = "Inventario";
            return View(inventario);
        }

        public IActionResult Contabilidad()
        {
            ViewBag.ActiveMenu = "Contabilidad";
            return View(contabilidad);
        }

        public IActionResult Empleados()
        {
            ViewBag.ActiveMenu = "Empleados";
            return View(empleados);
        }
    }
}
