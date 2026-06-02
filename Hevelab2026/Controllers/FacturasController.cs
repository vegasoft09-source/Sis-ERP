using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Hevelab2026.Models;

namespace Hevelab2026.Controllers
{
    public class FacturasController : Controller
    {
        public IActionResult Index()
        {
            var facturas = new List<Factura>
            {
                new Factura { NumeroFactura = "F-2024-001", Fecha = "2024-05-10", Cliente = "Corporación Logística S.A.C.", RUC = "20601234567", TipoComprobante = "Factura", Moneda = "PEN", MontoBase = 1000.00, IGV = 180.00, Total = 1180.00, Estado = "Emitida" },
                new Factura { NumeroFactura = "B-2024-042", Fecha = "2024-05-12", Cliente = "Juan Pérez Torres", RUC = "10408542109", TipoComprobante = "Boleta", Moneda = "PEN", MontoBase = 150.00, IGV = 27.00, Total = 177.00, Estado = "Emitida" },
                new Factura { NumeroFactura = "N-2024-003", Fecha = "2024-05-15", Cliente = "Minera Del Sur S.A.", RUC = "20109876543", TipoComprobante = "Nota de Crédito", Moneda = "USD", MontoBase = 500.00, IGV = 90.00, Total = 590.00, Estado = "Pendiente" },
                new Factura { NumeroFactura = "F-2024-004", Fecha = "2024-05-18", Cliente = "Servicios Tecnológicos Globales", RUC = "20556677889", TipoComprobante = "Factura", Moneda = "USD", MontoBase = 2000.00, IGV = 360.00, Total = 2360.00, Estado = "Anulada" },
                new Factura { NumeroFactura = "F-2024-005", Fecha = "2024-05-20", Cliente = "Inversiones Andinas S.R.L.", RUC = "20448833112", TipoComprobante = "Factura", Moneda = "PEN", MontoBase = 3500.00, IGV = 630.00, Total = 4130.00, Estado = "Pendiente" },
                new Factura { NumeroFactura = "B-2024-043", Fecha = "2024-05-22", Cliente = "María Ruiz Gomez", RUC = "10457812903", TipoComprobante = "Boleta", Moneda = "PEN", MontoBase = 80.00, IGV = 14.40, Total = 94.40, Estado = "Emitida" },
                new Factura { NumeroFactura = "F-2024-006", Fecha = "2024-05-25", Cliente = "Constructora e Inmobiliaria Patria", RUC = "20608541298", TipoComprobante = "Factura", Moneda = "PEN", MontoBase = 1200.00, IGV = 216.00, Total = 1416.00, Estado = "Emitida" },
                new Factura { NumeroFactura = "F-2024-007", Fecha = "2024-05-28", Cliente = "Alimentos y Bebidas del Perú", RUC = "20501248963", TipoComprobante = "Factura", Moneda = "USD", MontoBase = 300.00, IGV = 54.00, Total = 354.00, Estado = "Emitida" },
                new Factura { NumeroFactura = "N-2024-008", Fecha = "2024-05-30", Cliente = "Transportes Rápidos Lima", RUC = "20409852147", TipoComprobante = "Nota de Crédito", Moneda = "PEN", MontoBase = 200.00, IGV = 36.00, Total = 236.00, Estado = "Anulada" },
                new Factura { NumeroFactura = "B-2024-044", Fecha = "2024-06-02", Cliente = "Elena Sánchez Prado", RUC = "10254879632", TipoComprobante = "Boleta", Moneda = "PEN", MontoBase = 120.00, IGV = 21.60, Total = 141.60, Estado = "Pendiente" }
            };

            return View(facturas);
        }
    }
}
