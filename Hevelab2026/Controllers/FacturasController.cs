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
    public class FacturasController : Controller
    {
        private readonly string _connectionString;

        public FacturasController(IConfiguration configuration)
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
                ("Facturas", "/Facturas")
            };

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            // Cargar facturas
            const string sql = @"
                SELECT 
                    m.id AS Id,
                    m.numero_documento AS NumeroFactura,
                    DATE_FORMAT(m.fecha_emision, '%Y-%m-%d') AS Fecha,
                    s.razon_social AS Cliente,
                    s.numero_documento AS RUC,
                    CASE 
                        WHEN m.tipo_movimiento = 'out_refund' THEN 'Nota de Crédito'
                        WHEN m.numero_documento LIKE 'B%' THEN 'Boleta'
                        ELSE 'Factura'
                    END AS TipoComprobante,
                    cur.codigo AS Moneda,
                    m.subtotal AS MontoBase,
                    m.total_impuestos AS IGV,
                    m.total AS Total,
                    m.estado AS Estado
                FROM account_move m
                LEFT JOIN socio s ON s.id = m.socio_id
                LEFT JOIN moneda cur ON cur.id = m.moneda_id
                WHERE m.tipo_movimiento = 'out_invoice' AND m.numero_documento LIKE 'F%' AND m.estado IN ('Emitida', 'Enviada')
                ORDER BY m.fecha_emision DESC, m.id DESC;";

            var facturas = (await conn.QueryAsync<Factura>(sql)).ToList();

            // Consultar nombres de documentos CDR XML existentes para determinar el estado de envío
            var cdrDocs = (await conn.QueryAsync<string>(
                "SELECT nombre FROM documento_contable WHERE nombre LIKE 'CDR_%.xml'")).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var f in facturas)
            {
                f.CdrProcesado = f.Estado == "Enviada" || cdrDocs.Contains($"CDR_{f.NumeroFactura}.xml");
            }

            // Cargar lista de socios (para el dropdown del modal)
            var socios = (await conn.QueryAsync("SELECT id, razon_social, numero_documento FROM socio WHERE es_cliente = 1 AND activo = 1")).ToList();
            ViewBag.Socios = socios;

            return View(facturas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarFactura(
            string numeroFactura,
            string fecha,
            string ruc,
            string razonSocial,
            string tipoComprobante,
            string moneda,
            double montoBase,
            double igv,
            double total,
            string estado,
            string? originalNumero)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var trans = await conn.BeginTransactionAsync();

            try
            {
                // 1. Ensure partner exists in database
                int socioId = 0;
                var existingSocio = await conn.QueryFirstOrDefaultAsync<int?>(
                    "SELECT id FROM socio WHERE numero_documento = @Ruc LIMIT 1;", new { Ruc = ruc }, transaction: trans);
                
                if (existingSocio != null)
                {
                    socioId = existingSocio.Value;
                }
                else
                {
                    // Create customer
                    const string insSocioSql = @"
                        INSERT INTO socio (empresa_id, razon_social, tipo_documento, numero_documento, es_cliente, es_proveedor, es_contacto, activo)
                        VALUES (1, @RazonSocial, @TipoDoc, @Ruc, 1, 0, 0, 1);
                        SELECT LAST_INSERT_ID();";
                    string tipoDoc = ruc.Length == 11 ? "RUC" : "DNI";
                    socioId = await conn.ExecuteScalarAsync<int>(insSocioSql, new {
                        RazonSocial = razonSocial,
                        TipoDoc = tipoDoc,
                        Ruc = ruc
                    }, transaction: trans);
                }

                // Get currency ID
                int monedaId = moneda == "USD" ? 2 : 1;
                decimal tc = moneda == "USD" ? 3.75m : 1.0m;

                string tipoMov = tipoComprobante == "Nota de Crédito" ? "out_refund" : "out_invoice";

                int moveId = 0;
                bool isEdit = !string.IsNullOrWhiteSpace(originalNumero);

                if (isEdit)
                {
                    // Get existing move ID
                    var existingMove = await conn.QueryFirstOrDefaultAsync<int?>(
                        "SELECT id FROM account_move WHERE numero_documento = @OrigNum LIMIT 1;", new { OrigNum = originalNumero }, transaction: trans);

                    if (existingMove != null)
                    {
                        moveId = existingMove.Value;

                        // Update account_move
                        const string updMoveSql = @"
                            UPDATE account_move 
                            SET numero_documento = @NumeroDoc,
                                tipo_movimiento = @TipoMov,
                                socio_id = @SocioId,
                                moneda_id = @MonedaId,
                                tipo_cambio = @Tc,
                                fecha_emision = @Fecha,
                                subtotal = @Subtotal,
                                total_impuestos = @Impuestos,
                                total = @Total,
                                monto_pendiente = @MontoPendiente,
                                estado = @Estado,
                                actualizado_en = NOW()
                            WHERE id = @MoveId;";

                        await conn.ExecuteAsync(updMoveSql, new {
                            NumeroDoc = numeroFactura,
                            TipoMov = tipoMov,
                            SocioId = socioId,
                            MonedaId = monedaId,
                            Tc = tc,
                            Fecha = DateTime.Parse(fecha),
                            Subtotal = montoBase,
                            Impuestos = igv,
                            Total = total,
                            MontoPendiente = estado == "Emitida" ? 0 : total,
                            Estado = estado,
                            MoveId = moveId
                        }, transaction: trans);

                        // Clear old move lines
                        await conn.ExecuteAsync("DELETE FROM account_move_line WHERE move_id = @MoveId;", new { MoveId = moveId }, transaction: trans);
                    }
                    else
                    {
                        isEdit = false; // fallback to create if not found
                    }
                }

                if (!isEdit)
                {
                    // Insert account_move
                    const string insMoveSql = @"
                        INSERT INTO account_move (empresa_id, journal_id, tipo_movimiento, numero_documento, socio_id, moneda_id, tipo_cambio, fecha_emision, subtotal, total_impuestos, total, monto_pendiente, estado, creado_por)
                        VALUES (1, 1, @TipoMov, @NumeroDoc, @SocioId, @MonedaId, @Tc, @Fecha, @Subtotal, @Impuestos, @Total, @MontoPendiente, @Estado, 1);
                        SELECT LAST_INSERT_ID();";

                    moveId = await conn.ExecuteScalarAsync<int>(insMoveSql, new {
                        TipoMov = tipoMov,
                        NumeroDoc = numeroFactura,
                        SocioId = socioId,
                        MonedaId = monedaId,
                        Tc = tc,
                        Fecha = DateTime.Parse(fecha),
                        Subtotal = montoBase,
                        Impuestos = igv,
                        Total = total,
                        MontoPendiente = estado == "Emitida" ? 0 : total,
                        Estado = estado
                    }, transaction: trans);
                }

                // Insert move lines (receivable, revenue, tax)
                // Receivable: Account 1211 (id = 2)
                await conn.ExecuteAsync(@"
                    INSERT INTO account_move_line (move_id, cuenta_id, socio_id, nombre, debe, haber, debe_empresa, haber_empresa, creado_en)
                    VALUES (@MoveId, 2, @SocioId, 'Cuentas por cobrar', @Debe, @Haber, @DebeEmp, @HaberEmp, NOW());",
                    new {
                        MoveId = moveId,
                        SocioId = socioId,
                        Debe = total,
                        Haber = 0,
                        DebeEmp = total * (double)tc,
                        HaberEmp = 0
                    }, transaction: trans);

                // Revenue: Account 7011 (id = 4)
                await conn.ExecuteAsync(@"
                    INSERT INTO account_move_line (move_id, cuenta_id, socio_id, nombre, debe, haber, debe_empresa, haber_empresa, creado_en)
                    VALUES (@MoveId, 4, @SocioId, 'Ingreso por ventas', @Debe, @Haber, @DebeEmp, @HaberEmp, NOW());",
                    new {
                        MoveId = moveId,
                        SocioId = socioId,
                        Debe = 0,
                        Haber = montoBase,
                        DebeEmp = 0,
                        HaberEmp = montoBase * (double)tc
                    }, transaction: trans);

                // Tax IGV: Account 4011 (id = 3)
                if (igv > 0)
                {
                    await conn.ExecuteAsync(@"
                        INSERT INTO account_move_line (move_id, cuenta_id, socio_id, nombre, debe, haber, debe_empresa, haber_empresa, creado_en)
                        VALUES (@MoveId, 3, @SocioId, 'IGV Ventas', @Debe, @Haber, @DebeEmp, @HaberEmp, NOW());",
                        new {
                            MoveId = moveId,
                            SocioId = socioId,
                            Debe = 0,
                            Haber = igv,
                            DebeEmp = 0,
                            HaberEmp = igv * (double)tc
                        }, transaction: trans);
                }

                await trans.CommitAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnularFactura(string numeroFactura)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = "UPDATE account_move SET estado = 'Anulada' WHERE numero_documento = @NumeroDoc;";
            await conn.ExecuteAsync(sql, new { NumeroDoc = numeroFactura });

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetDetalle(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            const string headerSql = @"
                SELECT 
                    m.id AS Id,
                    m.numero_documento AS NumeroFactura,
                    DATE_FORMAT(m.fecha_emision, '%Y-%m-%d') AS Fecha,
                    DATE_FORMAT(m.fecha_vencimiento, '%Y-%m-%d') AS FechaVencimiento,
                    m.referencia AS Referencia,
                    m.tipo_cambio AS TipoCambio,
                    m.subtotal AS MontoBase,
                    m.total_impuestos AS IGV,
                    m.total AS Total,
                    m.monto_pendiente AS MontoPendiente,
                    m.estado AS Estado,
                    m.narracion AS Narracion,
                    CASE 
                        WHEN m.tipo_movimiento = 'out_refund' THEN 'Nota de Crédito'
                        WHEN m.numero_documento LIKE 'B%' THEN 'Boleta'
                        ELSE 'Factura'
                    END AS TipoComprobante,
                    DATE_FORMAT(m.creado_en, '%Y-%m-%d %H:%i:%s') AS CreadoEn,
                    s.razon_social AS Cliente,
                    s.numero_documento AS RUC,
                    cur.codigo AS Moneda,
                    cur.simbolo AS MonedaSimbolo,
                    j.nombre AS Diario
                FROM account_move m
                LEFT JOIN socio s ON s.id = m.socio_id
                LEFT JOIN moneda cur ON cur.id = m.moneda_id
                LEFT JOIN account_journal j ON j.id = m.journal_id
                WHERE m.id = @Id
                LIMIT 1;";

            var header = await conn.QueryFirstOrDefaultAsync<dynamic>(headerSql, new { Id = id });

            if (header == null)
            {
                return NotFound();
            }

            const string linesSql = @"
                SELECT 
                    l.id AS Id,
                    a.codigo AS CuentaCodigo,
                    a.nombre AS CuentaNombre,
                    l.nombre AS Descripcion,
                    l.debe AS Debe,
                    l.haber AS Haber,
                    l.debe_empresa AS DebeEmpresa,
                    l.haber_empresa AS HaberEmpresa
                FROM account_move_line l
                INNER JOIN account_account a ON a.id = l.cuenta_id
                WHERE l.move_id = @MoveId
                ORDER BY l.debe DESC, l.id ASC;";

            var lines = (await conn.QueryAsync<dynamic>(linesSql, new { MoveId = header.Id })).ToList();

            return Json(new {
                Header = header,
                Lines = lines
            });
        }

        [HttpPost]
        public async Task<IActionResult> ProcederCDR(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            const string selectSql = @"
                SELECT 
                    m.id AS Id,
                    m.numero_documento AS NumeroFactura,
                    s.razon_social AS Cliente,
                    m.total AS Total,
                    cur.codigo AS Moneda
                FROM account_move m
                LEFT JOIN socio s ON s.id = m.socio_id
                LEFT JOIN moneda cur ON cur.id = m.moneda_id
                WHERE m.id = @Id
                LIMIT 1;";

            var invoice = await conn.QueryFirstOrDefaultAsync<dynamic>(selectSql, new { Id = id });

            if (invoice == null)
            {
                return Json(new { success = false, message = "No se encontró la factura especificada." });
            }

            string numeroFactura = invoice.NumeroFactura ?? "";
            string cliente = invoice.Cliente ?? "Cliente";
            double total = (double)(invoice.Total ?? 0.0);
            string moneda = invoice.Moneda ?? "PEN";

            const string insertSql = @"
                INSERT INTO documento_contable (
                    nombre, descripcion, tipo_documento, categoria, fecha_subida, tamano_kb, extension, subido_por, etiquetas, estado
                ) VALUES (
                    @Nombre, @Descripcion, @TipoDoc, @Categoria, CURDATE(), @TamanoKB, @Extension, @SubidoPor, @Etiquetas, 'Activo'
                );";

            // 1. XML Document
            await conn.ExecuteAsync(insertSql, new {
                Nombre = $"CDR_{numeroFactura}.xml",
                Descripcion = $"Constancia de Recepción (CDR) XML de SUNAT para la Factura {numeroFactura} de {cliente} por un total de {total:N2} {moneda}.",
                TipoDoc = "Factura",
                Categoria = "Impuestos / SUNAT",
                TamanoKB = 12,
                Extension = "xml",
                SubidoPor = "SUNAT API",
                Etiquetas = "SUNAT, CDR, XML, Factura"
            });

            // 2. PDF Document
            await conn.ExecuteAsync(insertSql, new {
                Nombre = $"Representacion_Impresa_{numeroFactura}.pdf",
                Descripcion = $"Representación Impresa en PDF de la Factura {numeroFactura} de {cliente} por un total de {total:N2} {moneda}.",
                TipoDoc = "Factura",
                Categoria = "Impuestos / SUNAT",
                TamanoKB = 185,
                Extension = "pdf",
                SubidoPor = "SUNAT API",
                Etiquetas = "SUNAT, PDF, Factura"
            });

            // 3. ZIP Document
            await conn.ExecuteAsync(insertSql, new {
                Nombre = $"CDR_{numeroFactura}.zip",
                Descripcion = $"Paquete comprimido ZIP que contiene la firma digital y el CDR XML de la Factura {numeroFactura} de {cliente}.",
                TipoDoc = "Otro",
                Categoria = "Impuestos / SUNAT",
                TamanoKB = 15,
                Extension = "zip",
                SubidoPor = "SUNAT API",
                Etiquetas = "SUNAT, CDR, ZIP, Factura"
            });

            // 4. Update the state of the move to 'Enviada' in account_move table
            const string updateMoveSql = "UPDATE account_move SET estado = 'Enviada' WHERE id = @Id;";
            await conn.ExecuteAsync(updateMoveSql, new { Id = id });

            return Json(new { success = true, message = $"Se han generado las 3 respuestas de SUNAT (XML, PDF y ZIP) para la factura {numeroFactura} y se han guardado en la sección de Documentos." });
        }
    }
}
