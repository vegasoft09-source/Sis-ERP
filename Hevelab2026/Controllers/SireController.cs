using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Hevelab2026.Models;

namespace Hevelab2026.Controllers
{
    public class SireController : Controller
    {
        public IActionResult Index()
        {
            var viewModel = new SireViewModel
            {
                VentasRV = new List<VentaSire>
                {
                    new VentaSire { Periodo = "202405", TipoDoc = "01", Serie = "F001", Correlativo = "0008542", Fecha = "2024-05-10", Ruc = "20601234567", RazonSocial = "Corporación Logística S.A.C.", Moneda = "PEN", BaseGrav = 1000.00, Igv = 180.00, BaseNoGrav = 0.00, Total = 1180.00, TipoOp = "Gravada" },
                    new VentaSire { Periodo = "202405", TipoDoc = "03", Serie = "B001", Correlativo = "0000421", Fecha = "2024-05-12", Ruc = "10408542109", RazonSocial = "Juan Pérez Torres", Moneda = "PEN", BaseGrav = 150.00, Igv = 27.00, BaseNoGrav = 0.00, Total = 177.00, TipoOp = "Gravada" },
                    new VentaSire { Periodo = "202405", TipoDoc = "01", Serie = "F001", Correlativo = "0008543", Fecha = "2024-05-15", Ruc = "2010987654", RazonSocial = "Minera Del Sur S.A.", Moneda = "USD", BaseGrav = 500.00, Igv = 90.00, BaseNoGrav = 0.00, Total = 590.00, TipoOp = "Gravada" },
                    new VentaSire { Periodo = "202405", TipoDoc = "01", Serie = "F001", Correlativo = "0008544", Fecha = "2024-05-18", Ruc = "20556677889", RazonSocial = "Servicios Tecnológicos Globales", Moneda = "USD", BaseGrav = 0.00, Igv = 0.00, BaseNoGrav = 2000.00, Total = 2000.00, TipoOp = "Exonerada" },
                    new VentaSire { Periodo = "202405", TipoDoc = "01", Serie = "F001", Correlativo = "0008545", Fecha = "2024-05-20", Ruc = "20448833112", RazonSocial = "Inversiones Andinas S.R.L.", Moneda = "PEN", BaseGrav = 3500.00, Igv = 580.00, BaseNoGrav = 0.00, Total = 4080.00, TipoOp = "Gravada" },
                    new VentaSire { Periodo = "202405", TipoDoc = "03", Serie = "B001", Correlativo = "0000422", Fecha = "2024-05-22", Ruc = "10457812903", RazonSocial = "María Ruiz Gomez", Moneda = "PEN", BaseGrav = 80.00, Igv = 14.40, BaseNoGrav = 0.00, Total = 94.40, TipoOp = "Gravada" },
                    new VentaSire { Periodo = "202405", TipoDoc = "01", Serie = "F001", Correlativo = "0008546", Fecha = "2024-05-25", Ruc = "20608541298", RazonSocial = "Constructora e Inmobiliaria Patria", Moneda = "PEN", BaseGrav = 1200.00, Igv = 216.00, BaseNoGrav = 0.00, Total = 1416.00, TipoOp = "Gravada" },
                    new VentaSire { Periodo = "202405", TipoDoc = "01", Serie = "F001", Correlativo = "0008547", Fecha = "2024-05-28", Ruc = "20501248963", RazonSocial = "Alimentos y Bebidas del Perú", Moneda = "USD", BaseGrav = 300.00, Igv = 54.00, BaseNoGrav = 0.00, Total = 354.00, TipoOp = "Gravada" }
                },
                ComprasRC = new List<CompraSire>
                {
                    new CompraSire { Periodo = "202405", TipoDoc = "01", Serie = "F301", Correlativo = "0451298", Fecha = "2024-05-02", FechaVenc = "2024-06-02", Ruc = "20100123456", RazonSocial = "Telefónica del Perú S.A.A.", Moneda = "PEN", TC = 1.00, BaseGrav = 450.00, Igv = 81.00, NoGravado = 0.00, Total = 531.00, Anotado = "Sí" },
                    new CompraSire { Periodo = "202405", TipoDoc = "01", Serie = "F402", Correlativo = "0984512", Fecha = "2024-05-04", FechaVenc = "2024-06-04", Ruc = "20200456123", RazonSocial = "Luz del Sur S.A.A.", Moneda = "PEN", TC = 1.00, BaseGrav = 820.00, Igv = 147.60, NoGravado = 0.00, Total = 967.60, Anotado = "Sí" },
                    new CompraSire { Periodo = "202405", TipoDoc = "01", Serie = "F005", Correlativo = "0004215", Fecha = "2024-05-05", FechaVenc = "2024-06-05", Ruc = "20300987654", RazonSocial = "Inversiones Grifo del Sur", Moneda = "PEN", TC = 1.00, BaseGrav = 250.00, Igv = 45.00, NoGravado = 0.00, Total = 295.00, Anotado = "Sí" },
                    new CompraSire { Periodo = "202405", TipoDoc = "01", Serie = "F012", Correlativo = "0012984", Fecha = "2024-05-08", FechaVenc = "2024-06-08", Ruc = "20400512897", RazonSocial = "Ferretería El Clavo S.A.C.", Moneda = "PEN", TC = 1.00, BaseGrav = 1500.00, Igv = 270.00, NoGravado = 0.00, Total = 1770.00, Anotado = "Sí" },
                    new CompraSire { Periodo = "202405", TipoDoc = "01", Serie = "F002", Correlativo = "0003254", Fecha = "2024-05-15", FechaVenc = "2024-06-15", Ruc = "30500147982", RazonSocial = "Distribuidora Útiles de Oficina", Moneda = "PEN", TC = 1.00, BaseGrav = 120.00, Igv = 21.60, NoGravado = 0.00, Total = 141.60, Anotado = "Sí" },
                    new CompraSire { Periodo = "202405", TipoDoc = "01", Serie = "F020", Correlativo = "0008541", Fecha = "2024-05-20", FechaVenc = "2024-06-20", Ruc = "20600489512", RazonSocial = "Importaciones Tecnológicas", Moneda = "USD", TC = 3.75, BaseGrav = 800.00, Igv = 144.00, NoGravado = 0.00, Total = 944.00, Anotado = "Sí" },
                    new CompraSire { Periodo = "202405", TipoDoc = "14", Serie = "E001", Correlativo = "0000421", Fecha = "2024-05-25", FechaVenc = "2024-05-25", Ruc = "20110482159", RazonSocial = "SUNAT - Tasas Especiales", Moneda = "PEN", TC = 1.00, BaseGrav = 0.00, Igv = 0.00, NoGravado = 50.00, Total = 50.00, Anotado = "No" },
                    new CompraSire { Periodo = "202405", TipoDoc = "01", Serie = "F003", Correlativo = "0001254", Fecha = "2024-05-29", FechaVenc = "2024-06-29", Ruc = "20509824631", RazonSocial = "Imprenta Rápida S.R.L.", Moneda = "PEN", TC = 1.00, BaseGrav = 300.00, Igv = 40.00, NoGravado = 0.00, Total = 340.00, Anotado = "Sí" }
                }
            };

            return View(viewModel);
        }
    }
}
