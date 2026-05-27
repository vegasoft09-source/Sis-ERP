using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Hevelab2026.Models;

namespace Hevelab2026.Controllers
{
    public class LibroMayorController : Controller
    {
        public IActionResult Index()
        {
            var viewModel = new LibroMayorViewModel
            {
                Cuentas = new List<CuentaMayor>
                {
                    new CuentaMayor { CodigoCuenta = "1011", NombreCuenta = "Caja General y Bancos", TipoCuenta = "Activo", SaldoInicial = 10000.00, DebeAnt = 8500.00, HaberAnt = 7200.00, SaldoAnt = 11300.00 },
                    new CuentaMayor { CodigoCuenta = "1211", NombreCuenta = "Cuentas por Cobrar Comerciales", TipoCuenta = "Activo", SaldoInicial = 5000.00, DebeAnt = 4200.00, HaberAnt = 4800.00, SaldoAnt = 4400.00 },
                    new CuentaMayor { CodigoCuenta = "4011", NombreCuenta = "IGV - Tributos por Pagar", TipoCuenta = "Pasivo", SaldoInicial = -1200.00, DebeAnt = 1000.00, HaberAnt = 2200.00, SaldoAnt = -2400.00 }
                },
                Movimientos = new List<MovimientoMayor>
                {
                    // Movimientos Caja y Bancos (1011) - Mayo
                    new MovimientoMayor { Cuenta = "1011", Fecha = "2024-05-03", Asiento = "A-002", Descripcion = "Cobro Factura F-2024-001", Debe = 1180.00, Haber = 0.00, DebeAnt = 980.00, HaberAnt = 0.00 },
                    new MovimientoMayor { Cuenta = "1011", Fecha = "2024-05-05", Asiento = "A-004", Descripcion = "Pago Proveedor Combustibles", Debe = 0.00, Haber = 250.00, DebeAnt = 0.00, HaberAnt = 300.00 },
                    new MovimientoMayor { Cuenta = "1011", Fecha = "2024-05-08", Asiento = "A-005", Descripcion = "Retiro efectivo para Caja Chica", Debe = 0.00, Haber = 500.00, DebeAnt = 0.00, HaberAnt = 400.00 },
                    new MovimientoMayor { Cuenta = "1011", Fecha = "2024-05-12", Asiento = "A-007", Descripcion = "Cobro Boleta B-2024-042", Debe = 177.00, Haber = 0.00, DebeAnt = 120.00, HaberAnt = 0.00 },
                    new MovimientoMayor { Cuenta = "1011", Fecha = "2024-05-15", Asiento = "A-010", Descripcion = "Pago de Planilla Quincenal", Debe = 0.00, Haber = 3500.00, DebeAnt = 0.00, HaberAnt = 3200.00 },
                    new MovimientoMayor { Cuenta = "1011", Fecha = "2024-05-20", Asiento = "A-012", Descripcion = "Compra de Útiles de Oficina", Debe = 0.00, Haber = 120.00, DebeAnt = 0.00, HaberAnt = 150.00 },
                    new MovimientoMayor { Cuenta = "1011", Fecha = "2024-05-25", Asiento = "A-015", Descripcion = "Depósito en Cuenta Corriente BCP", Debe = 0.00, Haber = 5000.00, DebeAnt = 0.00, HaberAnt = 4500.00 },
                    
                    // Movimientos Caja y Bancos (1011) - Junio
                    new MovimientoMayor { Cuenta = "1011", Fecha = "2024-06-03", Asiento = "A-018", Descripcion = "Cobro Factura F-2024-006", Debe = 1416.00, Haber = 0.00, DebeAnt = 1180.00, HaberAnt = 0.00 },
                    new MovimientoMayor { Cuenta = "1011", Fecha = "2024-06-08", Asiento = "A-020", Descripcion = "Pago del Alquiler de Oficinas", Debe = 0.00, Haber = 2200.00, DebeAnt = 0.00, HaberAnt = 2200.00 },

                    // Movimientos Cuentas por Cobrar (1211) - Mayo
                    new MovimientoMayor { Cuenta = "1211", Fecha = "2024-05-02", Asiento = "A-003", Descripcion = "Venta Factura F-2024-001 a Corp Logística", Debe = 1180.00, Haber = 0.00, DebeAnt = 1000.00, HaberAnt = 0.00 },
                    new MovimientoMayor { Cuenta = "1211", Fecha = "2024-05-03", Asiento = "A-002", Descripcion = "Cobro Factura F-2024-001 en Bancos", Debe = 0.00, Haber = 1180.00, DebeAnt = 0.00, HaberAnt = 1000.00 },
                    new MovimientoMayor { Cuenta = "1211", Fecha = "2024-05-10", Asiento = "A-006", Descripcion = "Venta Factura F-2024-005 a Inv Andinas", Debe = 4130.00, Haber = 0.00, DebeAnt = 3800.00, HaberAnt = 0.00 },
                    new MovimientoMayor { Cuenta = "1211", Fecha = "2024-05-12", Asiento = "A-007", Descripcion = "Cobro Boleta B-2024-042", Debe = 0.00, Haber = 177.00, DebeAnt = 0.00, HaberAnt = 120.00 },
                    new MovimientoMayor { Cuenta = "1211", Fecha = "2024-05-18", Asiento = "A-011", Descripcion = "Canje de Facturas por Letra Comercial", Debe = 0.00, Haber = 2000.00, DebeAnt = 0.00, HaberAnt = 1800.00 },
                    new MovimientoMayor { Cuenta = "1211", Fecha = "2024-05-22", Asiento = "A-014", Descripcion = "Venta Boleta B-2024-043 a María Ruiz", Debe = 94.40, Haber = 0.00, DebeAnt = 80.00, HaberAnt = 0.00 },
                    new MovimientoMayor { Cuenta = "1211", Fecha = "2024-05-28", Asiento = "A-016", Descripcion = "Cobro Letras Vencidas", Debe = 0.00, Haber = 1500.00, DebeAnt = 0.00, HaberAnt = 1200.00 },

                    // Movimientos Cuentas por Cobrar (1211) - Junio
                    new MovimientoMayor { Cuenta = "1211", Fecha = "2024-06-02", Asiento = "A-017", Descripcion = "Venta Boleta B-2024-044 a Elena Sánchez", Debe = 141.60, Haber = 0.00, DebeAnt = 94.40, HaberAnt = 0.00 },

                    // Movimientos IGV por Pagar (4011) - Mayo
                    new MovimientoMayor { Cuenta = "4011", Fecha = "2024-05-02", Asiento = "A-003", Descripcion = "IGV Ventas Factura F-2024-001", Debe = 0.00, Haber = 180.00, DebeAnt = 0.00, HaberAnt = 150.00 },
                    new MovimientoMayor { Cuenta = "4011", Fecha = "2024-05-04", Asiento = "A-004", Descripcion = "Crédito Fiscal IGV Compra Combustible", Debe = 38.14, Haber = 0.00, DebeAnt = 42.10, HaberAnt = 0.00 },
                    new MovimientoMayor { Cuenta = "4011", Fecha = "2024-05-10", Asiento = "A-006", Descripcion = "IGV Ventas Factura F-2024-005", Debe = 0.00, Haber = 630.00, DebeAnt = 0.00, HaberAnt = 580.00 },
                    new MovimientoMayor { Cuenta = "4011", Fecha = "2024-05-12", Asiento = "A-008", Descripcion = "IGV Ventas Boleta B-2024-042", Debe = 0.00, Haber = 27.00, DebeAnt = 0.00, HaberAnt = 18.00 },
                    new MovimientoMayor { Cuenta = "4011", Fecha = "2024-05-15", Asiento = "A-009", Descripcion = "Pago mensual IGV SUNAT", Debe = 1200.00, Haber = 0.00, DebeAnt = 1000.00, HaberAnt = 0.00 },
                    new MovimientoMayor { Cuenta = "4011", Fecha = "2024-05-20", Asiento = "A-013", Descripcion = "Crédito Fiscal IGV Compra Servicios", Debe = 18.30, Haber = 0.00, DebeAnt = 15.20, HaberAnt = 0.00 },
                    new MovimientoMayor { Cuenta = "4011", Fecha = "2024-05-22", Asiento = "A-014", Descripcion = "IGV Ventas Boleta B-2024-043", Debe = 0.00, Haber = 14.40, DebeAnt = 0.00, HaberAnt = 12.00 },

                    // Movimientos IGV por Pagar (4011) - Junio
                    new MovimientoMayor { Cuenta = "4011", Fecha = "2024-06-02", Asiento = "A-017", Descripcion = "IGV Ventas Boleta B-2024-044", Debe = 0.00, Haber = 21.60, DebeAnt = 0.00, HaberAnt = 14.40 }
                }
            };

            return View(viewModel);
        }
    }
}
