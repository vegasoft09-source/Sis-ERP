using Microsoft.AspNetCore.Mvc;
using Dapper;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hevelab2026.Controllers
{
    /// <summary>
    /// SOLO PARA DESARROLLO — genera hashes BCrypt.
    /// Accede a: /dev/hash?pwd=tuContraseña
    /// ELIMINAR o deshabilitar antes de pasar a producción.
    /// </summary>
    public class DevController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public DevController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("/dev/inspect-db")]
        public async Task<IActionResult> InspectDb()
        {
            var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var connStr = config.GetConnectionString("SisErp") ?? config.GetConnectionString("DefaultConnection");
            
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== DATABASE SCHEMA ===");
            sb.AppendLine($"Connection String: {connStr}");
            
            try
            {
                using var conn = new MySqlConnection(connStr);
                await conn.OpenAsync();
                
                var tables = (await conn.QueryAsync<string>("SHOW TABLES;")).ToList();
                sb.AppendLine($"Tables found: {string.Join(", ", tables)}");
                sb.AppendLine();
                
                foreach (var table in tables)
                {
                    sb.AppendLine($"Table: {table}");
                    sb.AppendLine("------------------------------------");
                    
                    var columns = await conn.QueryAsync("DESCRIBE `" + table + "`;");
                    foreach (var col in columns)
                    {
                        var dict = (IDictionary<string, object>)col;
                        var field = dict["Field"]?.ToString();
                        var type = dict["Type"]?.ToString();
                        var nullVal = dict["Null"]?.ToString();
                        var key = dict["Key"]?.ToString();
                        var defaultVal = dict["Default"]?.ToString() ?? "NULL";
                        var extra = dict["Extra"]?.ToString();
                        
                        sb.AppendLine($"  {field} - {type} - Null:{nullVal} - Key:{key} - Default:{defaultVal} - Extra:{extra}");
                    }
                    
                    var count = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM `{table}`;");
                    sb.AppendLine($"  Total Rows: {count}");
                    sb.AppendLine();
                }
                
                System.IO.File.WriteAllText("c:\\Users\\USER\\Desktop\\Sis-ERP-Sis-Erp2\\db_schema.txt", sb.ToString());

                // Also dump users to the console response for our convenience
                var users = (await conn.QueryAsync("SELECT id, nombre_usuario, nombre, apellido, correo, contrasena, rol_id FROM usuario")).ToList();
                var usersText = string.Join("\n", users.Select(u => $"ID: {u.id}, User: {u.nombre_usuario}, Name: {u.nombre} {u.apellido}, Email: {u.correo}, Rol: {u.rol_id}, PassHash: {u.contrasena}"));

                var socios = (await conn.QueryAsync("SELECT id, razon_social, nombres, apellidos, tipo_documento, numero_documento, es_cliente, es_proveedor FROM socio")).ToList();
                var sociosText = string.Join("\n", socios.Select(s => $"ID: {s.id}, RazonSocial: {s.razon_social}, Name: {s.nombres} {s.apellidos}, Doc: {s.tipo_documento} {s.numero_documento}, Cliente: {s.es_cliente}, Proveedor: {s.es_proveedor}"));

                return Content($"Success! Schema written to db_schema.txt. Tables: {string.Join(", ", tables)}\n\nUSERS:\n{usersText}\n\nSOCIOS:\n{sociosText}");
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        [HttpGet("/dev/hash")]
        public IActionResult Hash(string? pwd)
        {
            if (!_env.IsDevelopment())
                return NotFound();

            if (string.IsNullOrWhiteSpace(pwd))
            {
                var form = "<h2>Generador de Hash BCrypt</h2>" +
                           "<p>Uso: <code>/dev/hash?pwd=tuContraseña</code></p>" +
                           "<form>" +
                           "<input name='pwd' placeholder='Contraseña' style='padding:8px;font-size:16px;width:300px' />" +
                           "<button type='submit' style='padding:8px 16px;margin-left:8px'>Generar</button>" +
                           "</form>";
                return Content(form, "text/html");
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(pwd, workFactor: 11);

            var sql1 = $"UPDATE usuario SET contrasena = '{hash}' WHERE nombre_usuario = 'admin';";

            var html = "<!DOCTYPE html><html><head><title>Hash – Dev</title>" +
                       "<style>body{font-family:monospace;padding:2rem;background:#0d0f1a;color:#a5b4fc;}" +
                       ".box{background:#1e1b4b;padding:1.5rem;border-radius:8px;border:1px solid #4f46e5;}" +
                       ".hash{word-break:break-all;color:#34d399;font-size:1rem;margin:1rem 0;padding:1rem;" +
                       "background:#0f172a;border-radius:6px;}" +
                       ".sql{background:#0f172a;padding:1rem;border-radius:6px;color:#fbbf24;margin-top:1rem;}" +
                       "button{background:#4f46e5;color:white;border:none;padding:.5rem 1rem;" +
                       "border-radius:6px;cursor:pointer;margin-top:.5rem;}" +
                       "a{color:#818cf8;}</style></head><body>" +
                       "<div class='box'>" +
                       "<h2>🔐 Hash BCrypt generado</h2>" +
                       $"<p>Contraseña: <strong style='color:#f9a8d4'>'{pwd}'</strong></p>" +
                       $"<div class='hash' id='h'>{hash}</div>" +
                       "<button onclick=\"navigator.clipboard.writeText(document.getElementById('h').innerText)\">📋 Copiar hash</button>" +
                       "<div class='sql'><strong>SQL UPDATE listo:</strong><br/><br/>" +
                       $"<span id='sql'>{sql1}</span></div>" +
                       "<button onclick=\"navigator.clipboard.writeText(document.getElementById('sql').innerText)\" style='margin-top:.5rem'>📋 Copiar SQL</button>" +
                       "</div><br/><a href='/dev/hash'>← Generar otro</a>" +
                       "</body></html>";

            return Content(html, "text/html");
        }

        [HttpGet("/dev/seed-accounting")]
        public async Task<IActionResult> SeedAccounting()
        {
            var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var connStr = config.GetConnectionString("SisErp") ?? config.GetConnectionString("DefaultConnection");
            
            try
            {
                using var conn = new MySqlConnection(connStr);
                await conn.OpenAsync();
                using var trans = await conn.BeginTransactionAsync();
                
                // 1. Create table documento_contable if not exists
                var createDocTableSql = @"
                    CREATE TABLE IF NOT EXISTS documento_contable (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        nombre VARCHAR(255) NOT NULL,
                        descripcion TEXT,
                        tipo_documento VARCHAR(100) NOT NULL,
                        categoria VARCHAR(100) NOT NULL,
                        fecha_subida DATE NOT NULL,
                        tamano_kb INT NOT NULL,
                        extension VARCHAR(10) NOT NULL,
                        subido_por VARCHAR(150) NOT NULL,
                        etiquetas VARCHAR(255),
                        estado VARCHAR(20) NOT NULL DEFAULT 'Activo',
                        creado_en TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
                await conn.ExecuteAsync(createDocTableSql, transaction: trans);
                
                // 2. Seed account_account
                var accountCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM account_account;", transaction: trans);
                if (accountCount == 0)
                {
                    var insertAccountSql = @"
                        INSERT INTO account_account (id, empresa_id, codigo, nombre, tipo, naturaleza, permite_movimiento, requiere_socio, requiere_documento, activo)
                        VALUES 
                        (1, 1, '1011', 'Caja General y Bancos', 'Activo', 'DEUDORA', 1, 0, 0, 1),
                        (2, 1, '1211', 'Cuentas por Cobrar Comerciales', 'Activo', 'DEUDORA', 1, 1, 1, 1),
                        (3, 1, '4011', 'IGV - Tributos por Pagar', 'Pasivo', 'ACREEDORA', 1, 0, 0, 1),
                        (4, 1, '7011', 'Ventas de Mercaderías', 'Ingreso', 'ACREEDORA', 1, 1, 1, 1),
                        (5, 1, '6011', 'Compras de Mercaderías', 'Gasto', 'DEUDORA', 1, 1, 1, 1);";
                    await conn.ExecuteAsync(insertAccountSql, transaction: trans);
                }
                
                // 3. Seed account_journal
                var journalCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM account_journal;", transaction: trans);
                if (journalCount == 0)
                {
                    var insertJournalSql = @"
                        INSERT INTO account_journal (id, empresa_id, nombre, codigo, tipo, activo)
                        VALUES 
                        (1, 1, 'Registro de Ventas', 'VEN', 'Ventas', 1),
                        (2, 1, 'Registro de Compras', 'COM', 'Compras', 1),
                        (3, 1, 'Operaciones Diversas', 'MISC', 'Varios', 1);";
                    await conn.ExecuteAsync(insertJournalSql, transaction: trans);
                }
                
                // 4. Seed account_tax
                var taxCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM account_tax;", transaction: trans);
                if (taxCount == 0)
                {
                    var insertTaxSql = @"
                        INSERT INTO account_tax (id, empresa_id, nombre, codigo, tipo_impuesto, calculo, porcentaje, activo)
                        VALUES 
                        (1, 1, 'IGV 18%', 'IGV', 'IGV', 'PORCENTAJE', 18.0000, 1);";
                    await conn.ExecuteAsync(insertTaxSql, transaction: trans);
                }

                // Helper to ensure partner exists
                async Task<int> EnsureSocio(string ruc, string razonSocial, bool esCliente, bool esProveedor)
                {
                    var idVal = await conn.QueryFirstOrDefaultAsync<int?>(
                        "SELECT id FROM socio WHERE numero_documento = @Ruc LIMIT 1;", new { Ruc = ruc }, transaction: trans);
                    if (idVal != null) return idVal.Value;
                    
                    var sqlIns = @"
                        INSERT INTO socio (empresa_id, razon_social, tipo_documento, numero_documento, es_cliente, es_proveedor, es_contacto, activo)
                        VALUES (1, @RazonSocial, @TipoDoc, @Ruc, @EsCliente, @EsProveedor, 0, 1);
                        SELECT LAST_INSERT_ID();";
                    
                    string tipoDoc = ruc.Length == 11 ? "RUC" : "DNI";
                    return await conn.ExecuteScalarAsync<int>(sqlIns, new {
                        RazonSocial = razonSocial,
                        TipoDoc = tipoDoc,
                        Ruc = ruc,
                        EsCliente = esCliente ? 1 : 0,
                        EsProveedor = esProveedor ? 1 : 0
                    }, transaction: trans);
                }

                // 5. Seed account_move (Facturas)
                var moveCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM account_move;", transaction: trans);
                if (moveCount == 0)
                {
                    // Sales Mock Data
                    var sales = new[] {
                        new { num = "F-2024-001", date = "2024-05-10", name = "Corporación Logística S.A.C.", ruc = "20601234567", tipo = "Factura", mon = "PEN", baseVal = 1000.00, igv = 180.00, tot = 1180.00, est = "Emitida" },
                        new { num = "B-2024-042", date = "2024-05-12", name = "Juan Pérez Torres", ruc = "10408542109", tipo = "Boleta", mon = "PEN", baseVal = 150.00, igv = 27.00, tot = 177.00, est = "Emitida" },
                        new { num = "N-2024-003", date = "2024-05-15", name = "Minera Del Sur S.A.", ruc = "20109876543", tipo = "Nota de Crédito", mon = "USD", baseVal = 500.00, igv = 90.00, tot = 590.00, est = "Pendiente" },
                        new { num = "F-2024-004", date = "2024-05-18", name = "Servicios Tecnológicos Globales", ruc = "20556677889", tipo = "Factura", mon = "USD", baseVal = 2000.00, igv = 360.00, tot = 2360.00, est = "Anulada" },
                        new { num = "F-2024-005", date = "2024-05-20", name = "Inversiones Andinas S.R.L.", ruc = "20448833112", tipo = "Factura", mon = "PEN", baseVal = 3500.00, igv = 630.00, tot = 4130.00, est = "Pendiente" },
                        new { num = "B-2024-043", date = "2024-05-22", name = "María Ruiz Gomez", ruc = "10457812903", tipo = "Boleta", mon = "PEN", baseVal = 80.00, igv = 14.40, tot = 94.40, est = "Emitida" },
                        new { num = "F-2024-006", date = "2024-05-25", name = "Constructora e Inmobiliaria Patria", ruc = "20608541298", tipo = "Factura", mon = "PEN", baseVal = 1200.00, igv = 216.00, tot = 1416.00, est = "Emitida" },
                        new { num = "F-2024-007", date = "2024-05-28", name = "Alimentos y Bebidas del Perú", ruc = "20501248963", tipo = "Factura", mon = "USD", baseVal = 300.00, igv = 54.00, tot = 354.00, est = "Emitida" },
                        new { num = "N-2024-008", date = "2024-05-30", name = "Transportes Rápidos Lima", ruc = "20409852147", tipo = "Nota de Crédito", mon = "PEN", baseVal = 200.00, igv = 36.00, tot = 236.00, est = "Anulada" },
                        new { num = "B-2024-044", date = "2024-06-02", name = "Elena Sánchez Prado", ruc = "10254879632", tipo = "Boleta", mon = "PEN", baseVal = 120.00, igv = 21.60, tot = 141.60, est = "Pendiente" }
                    };

                    foreach (var s in sales)
                    {
                        int socioId = await EnsureSocio(s.ruc, s.name, esCliente: true, esProveedor: false);
                        int journalId = 1; // Sales
                        int monedaId = s.mon == "USD" ? 2 : 1;
                        decimal tc = s.mon == "USD" ? 3.75m : 1.0m;
                        
                        // Insert USD Currency if needed
                        if (monedaId == 2)
                        {
                            var hasUsd = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM moneda WHERE id = 2;", transaction: trans);
                            if (hasUsd == 0)
                            {
                                await conn.ExecuteAsync("INSERT INTO moneda (id, nombre, codigo, simbolo, activo) VALUES (2, 'Dólar Americano', 'USD', '$', 1);", transaction: trans);
                            }
                        }

                        var insMoveSql = @"
                            INSERT INTO account_move (empresa_id, journal_id, tipo_movimiento, numero_documento, socio_id, moneda_id, tipo_cambio, fecha_emision, subtotal, total_impuestos, total, monto_pendiente, estado, creado_por)
                            VALUES (1, @JournalId, @TipoMov, @NumeroDoc, @SocioId, @MonedaId, @Tc, @Fecha, @Subtotal, @Impuestos, @Total, @MontoPendiente, @Estado, 1);
                            SELECT LAST_INSERT_ID();";

                        string tipoMov = s.tipo == "Nota de Crédito" ? "out_refund" : "out_invoice";
                        
                        int moveId = await conn.ExecuteScalarAsync<int>(insMoveSql, new {
                            JournalId = journalId,
                            TipoMov = tipoMov,
                            NumeroDoc = s.num,
                            SocioId = socioId,
                            MonedaId = monedaId,
                            Tc = tc,
                            Fecha = DateTime.Parse(s.date),
                            Subtotal = s.baseVal,
                            Impuestos = s.igv,
                            Total = s.tot,
                            MontoPendiente = s.est == "Emitida" ? 0 : s.tot,
                            Estado = s.est
                        }, transaction: trans);

                        // Receivable: Account 2 (1211)
                        await conn.ExecuteAsync(@"
                            INSERT INTO account_move_line (move_id, cuenta_id, socio_id, nombre, debe, haber, debe_empresa, haber_empresa, creado_en)
                            VALUES (@MoveId, 2, @SocioId, 'Cuentas por cobrar', @Debe, @Haber, @DebeEmp, @HaberEmp, NOW());",
                            new {
                                MoveId = moveId,
                                SocioId = socioId,
                                Debe = s.tot,
                                Haber = 0,
                                DebeEmp = (decimal)s.tot * tc,
                                HaberEmp = 0
                            }, transaction: trans);

                        // Revenue: Account 4 (7011)
                        await conn.ExecuteAsync(@"
                            INSERT INTO account_move_line (move_id, cuenta_id, socio_id, nombre, debe, haber, debe_empresa, haber_empresa, creado_en)
                            VALUES (@MoveId, 4, @SocioId, 'Ingreso por ventas', @Debe, @Haber, @DebeEmp, @HaberEmp, NOW());",
                            new {
                                MoveId = moveId,
                                SocioId = socioId,
                                Debe = 0,
                                Haber = s.baseVal,
                                DebeEmp = 0,
                                HaberEmp = (decimal)s.baseVal * tc
                            }, transaction: trans);

                        // Tax IGV: Account 3 (4011)
                        if (s.igv > 0)
                        {
                            await conn.ExecuteAsync(@"
                                INSERT INTO account_move_line (move_id, cuenta_id, socio_id, nombre, debe, haber, debe_empresa, haber_empresa, creado_en)
                                VALUES (@MoveId, 3, @SocioId, 'IGV Ventas', @Debe, @Haber, @DebeEmp, @HaberEmp, NOW());",
                                new {
                                    MoveId = moveId,
                                    SocioId = socioId,
                                    Debe = 0,
                                    Haber = s.igv,
                                    DebeEmp = 0,
                                    HaberEmp = (decimal)s.igv * tc
                                }, transaction: trans);
                        }
                    }

                    // Purchases Mock Data
                    var purchases = new[] {
                        new { num = "F301-0451298", date = "2024-05-02", name = "Telefónica del Perú S.A.A.", ruc = "20100123456", mon = "PEN", tc = 1.00, baseVal = 450.00, igv = 81.00, tot = 531.00, anotado = "Sí" },
                        new { num = "F402-0984512", date = "2024-05-04", name = "Luz del Sur S.A.A.", ruc = "20200456123", mon = "PEN", tc = 1.00, baseVal = 820.00, igv = 147.60, tot = 967.60, anotado = "Sí" },
                        new { num = "F005-0004215", date = "2024-05-05", name = "Inversiones Grifo del Sur", ruc = "20300987654", mon = "PEN", tc = 1.00, baseVal = 250.00, igv = 45.00, tot = 295.00, anotado = "Sí" },
                        new { num = "F012-0012984", date = "2024-05-08", name = "Ferretería El Clavo S.A.C.", ruc = "20400512897", mon = "PEN", tc = 1.00, baseVal = 1500.00, igv = 270.00, tot = 1770.00, anotado = "Sí" },
                        new { num = "F002-0003254", date = "2024-05-15", name = "Distribuidora Útiles de Oficina", ruc = "30500147982", mon = "PEN", tc = 1.00, baseVal = 120.00, igv = 21.60, tot = 141.60, anotado = "Sí" },
                        new { num = "F020-0008541", date = "2024-05-20", name = "Importaciones Tecnológicas", ruc = "20600489512", mon = "USD", tc = 3.75, baseVal = 800.00, igv = 144.00, tot = 944.00, anotado = "Sí" },
                        new { num = "E001-0000421", date = "2024-05-25", name = "SUNAT - Tasas Especiales", ruc = "20110482159", mon = "PEN", tc = 1.00, baseVal = 0.00, igv = 0.00, tot = 50.00, anotado = "No" },
                        new { num = "F003-0001254", date = "2024-05-29", name = "Imprenta Rápida S.R.L.", ruc = "20509824631", mon = "PEN", tc = 1.00, baseVal = 300.00, igv = 40.00, tot = 340.00, anotado = "Sí" }
                    };

                    foreach (var p in purchases)
                    {
                        int socioId = await EnsureSocio(p.ruc, p.name, esCliente: false, esProveedor: true);
                        int journalId = 2; // Purchases
                        int monedaId = p.mon == "USD" ? 2 : 1;
                        decimal tc = (decimal)p.tc;

                        var insMoveSql = @"
                            INSERT INTO account_move (empresa_id, journal_id, tipo_movimiento, numero_documento, socio_id, moneda_id, tipo_cambio, fecha_emision, subtotal, total_impuestos, total, monto_pendiente, estado, creado_por)
                            VALUES (1, @JournalId, 'in_invoice', @NumeroDoc, @SocioId, @MonedaId, @Tc, @Fecha, @Subtotal, @Impuestos, @Total, 0, 'Emitida', 1);
                            SELECT LAST_INSERT_ID();";

                        int moveId = await conn.ExecuteScalarAsync<int>(insMoveSql, new {
                            JournalId = journalId,
                            NumeroDoc = p.num,
                            SocioId = socioId,
                            MonedaId = monedaId,
                            Tc = tc,
                            Fecha = DateTime.Parse(p.date),
                            Subtotal = p.baseVal,
                            Impuestos = p.igv,
                            Total = p.tot
                        }, transaction: trans);

                        var payAccId = await conn.ExecuteScalarAsync<int?>("SELECT id FROM account_account WHERE codigo = '4211';", transaction: trans);
                        if (payAccId == null)
                        {
                            payAccId = await conn.ExecuteScalarAsync<int>(@"
                                INSERT INTO account_account (empresa_id, codigo, nombre, tipo, naturaleza, permite_movimiento, requiere_socio, requiere_documento, activo)
                                VALUES (1, '4211', 'Cuentas por Pagar Comerciales', 'Pasivo', 'ACREEDORA', 1, 1, 1, 1);
                                SELECT LAST_INSERT_ID();", transaction: trans);
                        }

                        // Payable: Account 4211
                        await conn.ExecuteAsync(@"
                            INSERT INTO account_move_line (move_id, cuenta_id, socio_id, nombre, debe, haber, debe_empresa, haber_empresa, creado_en)
                            VALUES (@MoveId, @CuentaId, @SocioId, 'Cuentas por pagar', @Debe, @Haber, @DebeEmp, @HaberEmp, NOW());",
                            new {
                                MoveId = moveId,
                                CuentaId = payAccId.Value,
                                SocioId = socioId,
                                Debe = 0,
                                Haber = p.tot,
                                DebeEmp = 0,
                                HaberEmp = (decimal)p.tot * tc
                            }, transaction: trans);

                        // Expense: Account 5 (6011)
                        await conn.ExecuteAsync(@"
                            INSERT INTO account_move_line (move_id, cuenta_id, socio_id, nombre, debe, haber, debe_empresa, haber_empresa, creado_en)
                            VALUES (@MoveId, 5, @SocioId, 'Compra de mercadería', @Debe, @Haber, @DebeEmp, @HaberEmp, NOW());",
                            new {
                                MoveId = moveId,
                                SocioId = socioId,
                                Debe = p.baseVal,
                                Haber = 0,
                                DebeEmp = (decimal)p.baseVal * tc,
                                HaberEmp = 0
                            }, transaction: trans);

                        // Tax IGV: Account 3 (4011)
                        if (p.igv > 0)
                        {
                            await conn.ExecuteAsync(@"
                                INSERT INTO account_move_line (move_id, cuenta_id, socio_id, nombre, debe, haber, debe_empresa, haber_empresa, creado_en)
                                VALUES (@MoveId, 3, @SocioId, 'IGV Compras', @Debe, @Haber, @DebeEmp, @HaberEmp, NOW());",
                                new {
                                    MoveId = moveId,
                                    SocioId = socioId,
                                    Debe = p.igv,
                                    Haber = 0,
                                    DebeEmp = (decimal)p.igv * tc,
                                    HaberEmp = 0
                                }, transaction: trans);
                        }
                    }
                }

                // 6. Seed documento_contable
                var docCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM documento_contable;", transaction: trans);
                if (docCount == 0)
                {
                    var docs = new[] {
                        new { name = "Factura de Ventas F001-0008542.pdf", desc = "Factura emitida a Corporación Logística por servicios de consultoría.", tipo = "Factura", cat = "Ventas", date = "2026-05-10", kb = 245, ext = "pdf", user = "Admin. Contable", tags = "Ventas,Factura,Mayo", est = "Activo" },
                        new { name = "Conciliación Bancaria Mayo 2026.xlsx", desc = "Reporte de conciliación de la cuenta corriente principal del BCP.", tipo = "Estado de Cuenta", cat = "Cierre", date = "2026-05-24", kb = 1520, ext = "xlsx", user = "Luis Torres", tags = "BCP,Conciliación,Mayo", est = "Activo" },
                        new { name = "Comprobante de Compras F301-0451298.xml", desc = "XML SUNAT de factura de compras por adquisición de hardware de oficina.", tipo = "Factura", cat = "Compras", date = "2026-05-02", kb = 45, ext = "xml", user = "Sistema Contable", tags = "Compras,SUNAT,XML", est = "Activo" },
                        new { name = "Declaración PDT 621 Mayo 2026.pdf", desc = "Formulario virtual 621 presentado ante SUNAT correspondiente al periodo Mayo.", tipo = "Declaración", cat = "Impuestos", date = "2026-05-18", kb = 412, ext = "pdf", user = "Luis Torres", tags = "SUNAT,PDT621,Impuestos", est = "Activo" },
                        new { name = "Contrato de Alquiler Oficina Administrativa.pdf", desc = "Copia legalizada del contrato de arrendamiento de la oficina central.", tipo = "Contrato", cat = "Cierre", date = "2026-01-15", kb = 3450, ext = "pdf", user = "Legal Contable", tags = "Contrato,Oficinas,Legal", est = "Activo" },
                        new { name = "Planilla de Sueldos Mayo 2026.pdf", desc = "Detalle de remuneraciones y aportes de ley del periodo actual.", tipo = "Reporte", cat = "Cierre", date = "2026-05-25", kb = 890, ext = "pdf", user = "Recursos Humanos", tags = "Planilla,Sueldos,Cierre", est = "Activo" },
                        new { name = "Reporte Inventarios Fisicos Cierre.xlsx", desc = "Kardex valorizado conciliado tras inventario físico de almacenes.", tipo = "Reporte", cat = "Compras", date = "2026-05-20", kb = 2105, ext = "xlsx", user = "Supervisor Almacén", tags = "Inventario,Reporte,Kardex", est = "Activo" },
                        new { name = "Factura Proveedor Luz del Sur F402-0984512.pdf", desc = "Comprobante de servicios de energía eléctrica de la sede central.", tipo = "Factura", cat = "Compras", date = "2026-05-04", kb = 185, ext = "pdf", user = "Mesa de Partes", tags = "Servicios,Luz,Compras", est = "Activo" },
                        new { name = "Boleta de Ventas Electronica B001-0000421.xml", desc = "Comprobante XML SUNAT emitido por venta menor de mercaderías.", tipo = "Factura", cat = "Ventas", date = "2026-05-12", kb = 35, ext = "xml", user = "Caja Facturación", tags = "Ventas,Boleta,XML", est = "Activo" },
                        new { name = "Balance de Comprobación Mayo 2026.xlsx", desc = "Balance preliminar de saldos y cuentas de contabilidad analítica.", tipo = "Reporte", cat = "Cierre", date = "2026-05-26", kb = 920, ext = "xlsx", user = "Luis Torres", tags = "Balance,Mayo,Cierre", est = "Activo" },
                        new { name = "Declaracion Anual Impuesto Renta 2025.pdf", desc = "Formulario virtual presentativo anual para el ejercicio gravable anterior.", tipo = "Declaración", cat = "Impuestos", date = "2026-03-28", kb = 1880, ext = "pdf", user = "Gerente Contabilidad", tags = "Anual,Impuestos,SUNAT", est = "Activo" },
                        new { name = "Acta de Directorio Cierre de Ejercicio.pdf", desc = "Acta firmada aprobando estados financieros y distribución de dividendos.", tipo = "Contrato", cat = "Cierre", date = "2026-03-12", kb = 1205, ext = "pdf", user = "Gerente General", tags = "Acta,Directorio,Legal", est = "Activo" },
                        new { name = "Factura de Ventas F001-0008543.pdf", desc = "Factura duplicada o anulada correspondiente al mes de Abril.", tipo = "Factura", cat = "Ventas", date = "2026-04-15", kb = 230, ext = "pdf", user = "Luis Torres", tags = "Ventas,Archivado,Anulado", est = "Archivado" },
                        new { name = "Conciliación Bancaria Abril 2026.xlsx", desc = "Reporte final y aprobado de conciliaciones bancarias del mes de Abril.", tipo = "Estado de Cuenta", cat = "Cierre", date = "2026-04-30", kb = 1450, ext = "xlsx", user = "Gerente Contabilidad", tags = "BCP,Conciliación,Archivado", est = "Archivado" }
                    };

                    foreach (var d in docs)
                    {
                        var insDocSql = @"
                            INSERT INTO documento_contable (nombre, descripcion, tipo_documento, categoria, fecha_subida, tamano_kb, extension, subido_por, etiquetas, estado)
                            VALUES (@Nombre, @Desc, @TipoDoc, @Categoria, @Fecha, @Tamano, @Extension, @SubidoPor, @Etiquetas, @Estado);";
                        await conn.ExecuteAsync(insDocSql, new {
                            Nombre = d.name,
                            Desc = d.desc,
                            TipoDoc = d.tipo,
                            Categoria = d.cat,
                            Fecha = DateTime.Parse(d.date),
                            Tamano = d.kb,
                            Extension = d.ext,
                            SubidoPor = d.user,
                            Etiquetas = d.tags,
                            Estado = d.est
                        }, transaction: trans);
                    }
                }
                
                await trans.CommitAsync();
                return Content("Success! Accounting tables seeded successfully!");
            }
            catch (Exception ex)
            {
                return Content($"Error during seeding: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
