using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hevelab2026.Models;
using Dapper;
using MySqlConnector;

namespace Hevelab2026.Controllers
{
    public class SireController : Controller
    {
        private readonly string _connectionString;

        public SireController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SisErp")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión.");
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Breadcrumbs"] = new List<(string Label, string Url)>
            {
                ("Dashboard", "/"),
                ("Contabilidad", "#"),
                ("SIRE", "/Sire")
            };

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            // Query distinct periods
            const string sqlPeriodos = @"
                SELECT DISTINCT DATE_FORMAT(fecha_emision, '%Y-%m') AS Periodo 
                FROM account_move 
                WHERE tipo_movimiento IN ('out_invoice', 'out_refund', 'in_invoice')
                ORDER BY Periodo DESC;";

            var periodos = (await conn.QueryAsync<string>(sqlPeriodos))
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            if (!periodos.Any())
            {
                periodos.Add(DateTime.Now.ToString("yyyy-MM"));
                periodos.Add(DateTime.Now.AddMonths(-1).ToString("yyyy-MM"));
            }
            ViewBag.Periodos = periodos;

            // Query Sales for RV
            const string sqlVentas = @"
                SELECT 
                    DATE_FORMAT(m.fecha_emision, '%Y%m') AS Periodo,
                    CASE 
                        WHEN m.tipo_movimiento = 'out_refund' THEN '07'
                        WHEN m.numero_documento LIKE 'B%' THEN '03'
                        ELSE '01'
                    END AS TipoDoc,
                    m.numero_documento AS NumeroDoc,
                    DATE_FORMAT(m.fecha_emision, '%Y-%m-%d') AS Fecha,
                    s.numero_documento AS Ruc,
                    s.razon_social AS RazonSocial,
                    cur.codigo AS Moneda,
                    CASE WHEN m.total_impuestos > 0 THEN m.subtotal ELSE 0 END AS BaseGrav,
                    m.total_impuestos AS Igv,
                    CASE WHEN m.total_impuestos = 0 THEN m.subtotal ELSE 0 END AS BaseNoGrav,
                    m.total AS Total,
                    CASE WHEN m.total_impuestos > 0 THEN 'Gravada' ELSE 'Exonerada' END AS TipoOp
                FROM account_move m
                LEFT JOIN socio s ON s.id = m.socio_id
                LEFT JOIN moneda cur ON cur.id = m.moneda_id
                WHERE m.tipo_movimiento IN ('out_invoice', 'out_refund')
                ORDER BY m.fecha_emision DESC, m.id DESC;";

            var ventasRaw = await conn.QueryAsync(sqlVentas);
            var ventas = ventasRaw.Select(v => {
                var row = (IDictionary<string, object>)v;
                string numDoc = row.TryGetValue("NumeroDoc", out var ndVal) ? ndVal?.ToString() ?? "" : "";
                string serie = "";
                string correlativo = "";
                int hyphenIdx = numDoc.LastIndexOf('-');
                if (hyphenIdx != -1) {
                    serie = numDoc.Substring(0, hyphenIdx).Replace("-", "");
                    correlativo = numDoc.Substring(hyphenIdx + 1);
                } else if (numDoc.Length > 4) {
                    serie = numDoc.Substring(0, 4);
                    correlativo = numDoc.Substring(4);
                } else {
                    serie = numDoc;
                    correlativo = "";
                }
                return new VentaSire {
                    Periodo = row.TryGetValue("Periodo", out var pVal) ? pVal?.ToString() ?? "" : "",
                    TipoDoc = row.TryGetValue("TipoDoc", out var tdVal) ? tdVal?.ToString() ?? "01" : "01",
                    Serie = serie,
                    Correlativo = correlativo,
                    Fecha = row.TryGetValue("Fecha", out var fVal) ? fVal?.ToString() ?? "" : "",
                    Ruc = row.TryGetValue("Ruc", out var rVal) ? rVal?.ToString() ?? "" : "",
                    RazonSocial = row.TryGetValue("RazonSocial", out var rsVal) ? rsVal?.ToString() ?? "" : "",
                    Moneda = row.TryGetValue("Moneda", out var mVal) ? mVal?.ToString() ?? "PEN" : "PEN",
                    BaseGrav = row.TryGetValue("BaseGrav", out var bgVal) && bgVal != null ? Convert.ToDouble(bgVal) : 0.0,
                    Igv = row.TryGetValue("Igv", out var igvVal) && igvVal != null ? Convert.ToDouble(igvVal) : 0.0,
                    BaseNoGrav = row.TryGetValue("BaseNoGrav", out var bngVal) && bngVal != null ? Convert.ToDouble(bngVal) : 0.0,
                    Total = row.TryGetValue("Total", out var totVal) && totVal != null ? Convert.ToDouble(totVal) : 0.0,
                    TipoOp = row.TryGetValue("TipoOp", out var toVal) ? toVal?.ToString() ?? "Gravada" : "Gravada"
                };
            }).ToList();

            // Query Purchases for RC
            const string sqlCompras = @"
                SELECT 
                    DATE_FORMAT(m.fecha_emision, '%Y%m') AS Periodo,
                    CASE 
                        WHEN m.numero_documento LIKE 'E%' THEN '14'
                        ELSE '01'
                    END AS TipoDoc,
                    m.numero_documento AS NumeroDoc,
                    DATE_FORMAT(m.fecha_emision, '%Y-%m-%d') AS Fecha,
                    DATE_FORMAT(COALESCE(m.fecha_vencimiento, m.fecha_emision), '%Y-%m-%d') AS FechaVenc,
                    s.numero_documento AS Ruc,
                    s.razon_social AS RazonSocial,
                    cur.codigo AS Moneda,
                    m.tipo_cambio AS TC,
                    CASE WHEN m.total_impuestos > 0 THEN m.subtotal ELSE 0 END AS BaseGrav,
                    m.total_impuestos AS Igv,
                    CASE WHEN m.total_impuestos = 0 THEN m.subtotal ELSE 0 END AS NoGravado,
                    m.total AS Total,
                    'Sí' AS Anotado
                FROM account_move m
                LEFT JOIN socio s ON s.id = m.socio_id
                LEFT JOIN moneda cur ON cur.id = m.moneda_id
                WHERE m.tipo_movimiento = 'in_invoice'
                ORDER BY m.fecha_emision DESC, m.id DESC;";

            var comprasRaw = await conn.QueryAsync(sqlCompras);
            var compras = comprasRaw.Select(c => {
                var row = (IDictionary<string, object>)c;
                string numDoc = row.TryGetValue("NumeroDoc", out var ndVal) ? ndVal?.ToString() ?? "" : "";
                string serie = "";
                string correlativo = "";
                int hyphenIdx = numDoc.LastIndexOf('-');
                if (hyphenIdx != -1) {
                    serie = numDoc.Substring(0, hyphenIdx).Replace("-", "");
                    correlativo = numDoc.Substring(hyphenIdx + 1);
                } else if (numDoc.Length > 4) {
                    serie = numDoc.Substring(0, 4);
                    correlativo = numDoc.Substring(4);
                } else {
                    serie = numDoc;
                    correlativo = "";
                }
                return new CompraSire {
                    Periodo = row.TryGetValue("Periodo", out var pVal) ? pVal?.ToString() ?? "" : "",
                    TipoDoc = row.TryGetValue("TipoDoc", out var tdVal) ? tdVal?.ToString() ?? "01" : "01",
                    Serie = serie,
                    Correlativo = correlativo,
                    Fecha = row.TryGetValue("Fecha", out var fVal) ? fVal?.ToString() ?? "" : "",
                    FechaVenc = row.TryGetValue("FechaVenc", out var fvVal) ? fvVal?.ToString() ?? "" : "",
                    Ruc = row.TryGetValue("Ruc", out var rVal) ? rVal?.ToString() ?? "" : "",
                    RazonSocial = row.TryGetValue("RazonSocial", out var rsVal) ? rsVal?.ToString() ?? "" : "",
                    Moneda = row.TryGetValue("Moneda", out var mVal) ? mVal?.ToString() ?? "PEN" : "PEN",
                    TC = row.TryGetValue("TC", out var tcVal) && tcVal != null ? Convert.ToDouble(tcVal) : 1.0,
                    BaseGrav = row.TryGetValue("BaseGrav", out var bgVal) && bgVal != null ? Convert.ToDouble(bgVal) : 0.0,
                    Igv = row.TryGetValue("Igv", out var igvVal) && igvVal != null ? Convert.ToDouble(igvVal) : 0.0,
                    NoGravado = row.TryGetValue("NoGravado", out var ngVal) && ngVal != null ? Convert.ToDouble(ngVal) : 0.0,
                    Total = row.TryGetValue("Total", out var totVal) && totVal != null ? Convert.ToDouble(totVal) : 0.0,
                    Anotado = row.TryGetValue("Anotado", out var aVal) ? aVal?.ToString() ?? "Sí" : "Sí"
                };
            }).ToList();

            var viewModel = new SireViewModel
            {
                VentasRV = ventas,
                ComprasRC = compras
            };

            return View(viewModel);
        }
    }
}
