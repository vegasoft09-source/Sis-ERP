using Dapper;
using Hevelab2026.Models;
using MySqlConnector;

namespace Hevelab2026.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly string _connectionString;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(IConfiguration configuration, ILogger<DashboardService> logger)
        {
            _connectionString = configuration.GetConnectionString("SisErp")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SisErp'.");
            _logger = logger;
        }

        public async Task<List<MetricCard>> ObtenerMetricasAsync(int empresaId)
        {
            try
            {
                await using var con = new MySqlConnection(_connectionString);
                await con.OpenAsync();

                var simbolo = await ObtenerSimboloMonedaAsync(con, empresaId);

                return new List<MetricCard>
                {
                    await VentasMensualesAsync(con, empresaId, simbolo),
                    await NuevosClientesAsync(con, empresaId),
                    await StockAlmacenAsync(con, empresaId),
                    await FacturasPorCobrarAsync(con, empresaId, simbolo)
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron cargar métricas del dashboard");
                return MetricasVacias();
            }
        }

        private static List<MetricCard> MetricasVacias() =>
        [
            MetricaVacia("Ventas Mensuales", "$0.00", "primary",
                @"<line x1='12' y1='1' x2='12' y2='23'/><path d='M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6'/>"),
            MetricaVacia("Nuevos Clientes", "0", "success",
                @"<path d='M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2'/><circle cx='9' cy='7' r='4'/><path d='M23 21v-2a4 4 0 0 0-3-3.87'/><path d='M16 3.13a4 4 0 0 1 0 7.75'/>"),
            MetricaVacia("Stock Almacén", "0 u.", "warning",
                @"<path d='M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z'/><polyline points='7.5 4.21 12 6.81 16.5 4.21'/>"),
            MetricaVacia("Facturas Cobrar", "$0.00", "purple",
                @"<path d='M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z'/><polyline points='14 2 14 8 20 8'/><line x1='16' y1='13' x2='8' y2='13'/><line x1='16' y1='17' x2='8' y2='17'/>")
        ];

        private static MetricCard MetricaVacia(string title, string value, string theme, string icon) =>
            new()
            {
                Title = title,
                Value = value,
                TrendText = "Sin conexión a BD",
                TrendType = "warning",
                ThemeColor = theme,
                IconSvg = icon,
                Chart = new MetricChartData { Type = "line", Values = Array.Empty<double>() }
            };

        private async Task<MetricCard> VentasMensualesAsync(MySqlConnection con, int empresaId, string simbolo)
        {
            const string icon = @"<line x1='12' y1='1' x2='12' y2='23'/><path d='M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6'/>";

            if (await TablaExisteAsync(con, "venta"))
            {
                var mesActual = await con.ExecuteScalarAsync<decimal?>(@"
                    SELECT COALESCE(SUM(total), 0) FROM venta
                    WHERE empresa_id = @EmpresaId
                      AND MONTH(fecha) = MONTH(CURDATE()) AND YEAR(fecha) = YEAR(CURDATE());",
                    new { EmpresaId = empresaId });

                var mesAnterior = await con.ExecuteScalarAsync<decimal?>(@"
                    SELECT COALESCE(SUM(total), 0) FROM venta
                    WHERE empresa_id = @EmpresaId
                      AND MONTH(fecha) = MONTH(DATE_SUB(CURDATE(), INTERVAL 1 MONTH))
                      AND YEAR(fecha) = YEAR(DATE_SUB(CURDATE(), INTERVAL 1 MONTH));",
                    new { EmpresaId = empresaId });

                var serie = await SerieMensualAsync(con, "venta", "total", "fecha", empresaId, 6);

                return new MetricCard
                {
                    Title = "Ventas Mensuales",
                    Value = FormatoMoneda(mesActual ?? 0, simbolo),
                    TrendText = TextoVariacion(mesActual ?? 0, mesAnterior ?? 0),
                    TrendType = TipoVariacion(mesActual ?? 0, mesAnterior ?? 0),
                    ThemeColor = "primary",
                    IconSvg = icon,
                    Chart = new MetricChartData { Type = "line", Labels = serie.Labels, Values = serie.Values }
                };
            }

            if (await TablaExisteAsync(con, "factura_venta"))
            {
                var mesActual = await con.ExecuteScalarAsync<decimal?>(@"
                    SELECT COALESCE(SUM(total), 0) FROM factura_venta
                    WHERE empresa_id = @EmpresaId
                      AND MONTH(fecha_emision) = MONTH(CURDATE()) AND YEAR(fecha_emision) = YEAR(CURDATE());",
                    new { EmpresaId = empresaId });

                var mesAnterior = await con.ExecuteScalarAsync<decimal?>(@"
                    SELECT COALESCE(SUM(total), 0) FROM factura_venta
                    WHERE empresa_id = @EmpresaId
                      AND MONTH(fecha_emision) = MONTH(DATE_SUB(CURDATE(), INTERVAL 1 MONTH))
                      AND YEAR(fecha_emision) = YEAR(DATE_SUB(CURDATE(), INTERVAL 1 MONTH));",
                    new { EmpresaId = empresaId });

                var serie = await SerieMensualAsync(con, "factura_venta", "total", "fecha_emision", empresaId, 6);

                return new MetricCard
                {
                    Title = "Ventas Mensuales",
                    Value = FormatoMoneda(mesActual ?? 0, simbolo),
                    TrendText = TextoVariacion(mesActual ?? 0, mesAnterior ?? 0),
                    TrendType = TipoVariacion(mesActual ?? 0, mesAnterior ?? 0),
                    ThemeColor = "primary",
                    IconSvg = icon,
                    Chart = new MetricChartData { Type = "line", Labels = serie.Labels, Values = serie.Values }
                };
            }

            var vacio = RellenarMeses(6, 0);
            return new MetricCard
            {
                Title = "Ventas Mensuales",
                Value = FormatoMoneda(0, simbolo),
                TrendText = "Sin registros de ventas",
                TrendType = "warning",
                ThemeColor = "primary",
                IconSvg = icon,
                Chart = new MetricChartData { Type = "line", Labels = vacio.Labels, Values = vacio.Values }
            };
        }

        private async Task<MetricCard> NuevosClientesAsync(MySqlConnection con, int empresaId)
        {
            const string icon = @"<path d='M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2'/><circle cx='9' cy='7' r='4'/><path d='M23 21v-2a4 4 0 0 0-3-3.87'/><path d='M16 3.13a4 4 0 0 1 0 7.75'/>";

            if (await TablaExisteAsync(con, "cliente"))
            {
                var total = await con.ExecuteScalarAsync<int>(@"
                    SELECT COUNT(*) FROM cliente
                    WHERE empresa_id = @EmpresaId AND activo = 1;",
                    new { EmpresaId = empresaId });

                var mesActual = await con.ExecuteScalarAsync<int>(@"
                    SELECT COUNT(*) FROM cliente
                    WHERE empresa_id = @EmpresaId AND activo = 1
                      AND MONTH(creado_en) = MONTH(CURDATE()) AND YEAR(creado_en) = YEAR(CURDATE());",
                    new { EmpresaId = empresaId });

                var mesAnterior = await con.ExecuteScalarAsync<int>(@"
                    SELECT COUNT(*) FROM cliente
                    WHERE empresa_id = @EmpresaId AND activo = 1
                      AND MONTH(creado_en) = MONTH(DATE_SUB(CURDATE(), INTERVAL 1 MONTH))
                      AND YEAR(creado_en) = YEAR(DATE_SUB(CURDATE(), INTERVAL 1 MONTH));",
                    new { EmpresaId = empresaId });

                var serie = await SerieMensualAsync(con, "cliente", "1", "creado_en", empresaId, 6, agregado: "COUNT");

                return new MetricCard
                {
                    Title = "Nuevos Clientes",
                    Value = total.ToString("N0"),
                    TrendText = TextoVariacion(mesActual, mesAnterior),
                    TrendType = TipoVariacion(mesActual, mesAnterior),
                    ThemeColor = "success",
                    IconSvg = icon,
                    Chart = new MetricChartData { Type = "line", Labels = serie.Labels, Values = serie.Values }
                };
            }

            // Fallback: usuarios de la empresa registrados por mes (dato real en BD actual)
            var usuariosMes = await con.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM usuario
                WHERE empresa_id = @EmpresaId AND activo = 1
                  AND MONTH(creado_en) = MONTH(CURDATE()) AND YEAR(creado_en) = YEAR(CURDATE());",
                new { EmpresaId = empresaId });

            var usuariosMesAnt = await con.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM usuario
                WHERE empresa_id = @EmpresaId AND activo = 1
                  AND MONTH(creado_en) = MONTH(DATE_SUB(CURDATE(), INTERVAL 1 MONTH))
                  AND YEAR(creado_en) = YEAR(DATE_SUB(CURDATE(), INTERVAL 1 MONTH));",
                new { EmpresaId = empresaId });

            var totalActivos = await con.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM usuario WHERE empresa_id = @EmpresaId AND activo = 1;",
                new { EmpresaId = empresaId });

            var serieUsuarios = await SerieMensualAsync(con, "usuario", "1", "creado_en", empresaId, 6, agregado: "COUNT", filtroExtra: "AND activo = 1");

            return new MetricCard
            {
                Title = "Nuevos Clientes",
                Value = totalActivos.ToString("N0"),
                TrendText = usuariosMes > 0
                    ? $"{usuariosMes} alta(s) este mes"
                    : TextoVariacion(usuariosMes, usuariosMesAnt),
                TrendType = usuariosMes > 0 ? "up" : "warning",
                ThemeColor = "success",
                IconSvg = icon,
                Chart = new MetricChartData { Type = "line", Labels = serieUsuarios.Labels, Values = serieUsuarios.Values }
            };
        }

        private async Task<MetricCard> StockAlmacenAsync(MySqlConnection con, int empresaId)
        {
            const string icon = @"<path d='M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z'/><polyline points='7.5 4.21 12 6.81 16.5 4.21'/>";

            if (await TablaExisteAsync(con, "producto"))
            {
                var stock = await con.ExecuteScalarAsync<decimal?>(@"
                    SELECT COALESCE(SUM(stock), 0) FROM producto
                    WHERE empresa_id = @EmpresaId AND activo = 1;",
                    new { EmpresaId = empresaId });

                var criticos = await con.ExecuteScalarAsync<int>(@"
                    SELECT COUNT(*) FROM producto
                    WHERE empresa_id = @EmpresaId AND activo = 1
                      AND stock_minimo IS NOT NULL AND stock <= stock_minimo;",
                    new { EmpresaId = empresaId });

                var serie = await SerieMensualAsync(con, "producto", "stock", "actualizado_en", empresaId, 6, agregado: "SUM");

                return new MetricCard
                {
                    Title = "Stock Almacén",
                    Value = $"{(stock ?? 0):N0} u.",
                    TrendText = criticos > 0
                        ? $"{criticos} producto(s) stock crítico"
                        : "Stock dentro de rango",
                    TrendType = criticos > 0 ? "warning" : "up",
                    ThemeColor = "warning",
                    IconSvg = icon,
                    Chart = new MetricChartData { Type = "line", Labels = serie.Labels, Values = serie.Values }
                };
            }

            var vacio = RellenarMeses(6, 0);
            return new MetricCard
            {
                Title = "Stock Almacén",
                Value = "0 u.",
                TrendText = "Sin módulo de inventario",
                TrendType = "warning",
                ThemeColor = "warning",
                IconSvg = icon,
                Chart = new MetricChartData { Type = "line", Labels = vacio.Labels, Values = vacio.Values }
            };
        }

        private async Task<MetricCard> FacturasPorCobrarAsync(MySqlConnection con, int empresaId, string simbolo)
        {
            const string icon = @"<path d='M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z'/><polyline points='14 2 14 8 20 8'/><line x1='16' y1='13' x2='8' y2='13'/><line x1='16' y1='17' x2='8' y2='17'/>";

            foreach (var tabla in new[] { "factura_venta", "factura", "cuenta_por_cobrar" })
            {
                if (!await TablaExisteAsync(con, tabla)) continue;

                var (sqlTotal, sqlPendientes, colFecha, colMonto) = tabla switch
                {
                    "factura_venta" => (
                        @"SELECT COALESCE(SUM(saldo_pendiente), 0) FROM factura_venta
                          WHERE empresa_id = @EmpresaId AND saldo_pendiente > 0;",
                        @"SELECT COUNT(*) FROM factura_venta
                          WHERE empresa_id = @EmpresaId AND saldo_pendiente > 0;",
                        "fecha_emision", "saldo_pendiente"),
                    "cuenta_por_cobrar" => (
                        @"SELECT COALESCE(SUM(monto_pendiente), 0) FROM cuenta_por_cobrar
                          WHERE empresa_id = @EmpresaId AND monto_pendiente > 0;",
                        @"SELECT COUNT(*) FROM cuenta_por_cobrar
                          WHERE empresa_id = @EmpresaId AND monto_pendiente > 0;",
                        "fecha_vencimiento", "monto_pendiente"),
                    _ => (
                        @"SELECT COALESCE(SUM(total), 0) FROM factura
                          WHERE empresa_id = @EmpresaId AND estado IN ('pendiente','vencida','por cobrar');",
                        @"SELECT COUNT(*) FROM factura
                          WHERE empresa_id = @EmpresaId AND estado IN ('pendiente','vencida','por cobrar');",
                        "fecha_emision", "total")
                };

                var total = await con.ExecuteScalarAsync<decimal?>(sqlTotal, new { EmpresaId = empresaId });
                var pendientes = await con.ExecuteScalarAsync<int>(sqlPendientes, new { EmpresaId = empresaId });
                var serie = await SerieMensualAsync(con, tabla, colMonto, colFecha, empresaId, 6);

                return new MetricCard
                {
                    Title = "Facturas Cobrar",
                    Value = FormatoMoneda(total ?? 0, simbolo),
                    TrendText = pendientes > 0
                        ? $"{pendientes} factura(s) pendiente(s)"
                        : "Sin facturas pendientes",
                    TrendType = pendientes > 0 ? "down" : "up",
                    ThemeColor = "purple",
                    IconSvg = icon,
                    Chart = new MetricChartData { Type = "line", Labels = serie.Labels, Values = serie.Values }
                };
            }

            var vacio = RellenarMeses(6, 0);
            return new MetricCard
            {
                Title = "Facturas Cobrar",
                Value = FormatoMoneda(0, simbolo),
                TrendText = "Sin facturas pendientes",
                TrendType = "up",
                ThemeColor = "purple",
                IconSvg = icon,
                Chart = new MetricChartData { Type = "line", Labels = vacio.Labels, Values = vacio.Values }
            };
        }

        private static async Task<bool> TablaExisteAsync(MySqlConnection con, string tabla)
        {
            const string sql = """
                SELECT COUNT(*) FROM information_schema.tables
                WHERE table_schema = DATABASE() AND table_name = @tabla;
                """;
            return await con.ExecuteScalarAsync<int>(sql, new { tabla }) > 0;
        }

        private static async Task<string> ObtenerSimboloMonedaAsync(MySqlConnection con, int empresaId)
        {
            const string sql = """
                SELECT COALESCE(m.simbolo, 'S/.') FROM empresa e
                LEFT JOIN moneda m ON m.id = e.moneda_id
                WHERE e.id = @EmpresaId LIMIT 1;
                """;
            return await con.ExecuteScalarAsync<string>(sql, new { EmpresaId = empresaId }) ?? "S/.";
        }

        private static async Task<(string[] Labels, double[] Values)> SerieMensualAsync(
            MySqlConnection con, string tabla, string columnaValor, string columnaFecha,
            int empresaId, int meses, string agregado = "SUM", string? filtroExtra = null)
        {
            if (!await TablaExisteAsync(con, tabla))
                return RellenarMeses(meses, 0);

            var agg = agregado.Equals("COUNT", StringComparison.OrdinalIgnoreCase)
                ? $"COUNT({columnaValor})"
                : $"COALESCE(SUM({columnaValor}), 0)";

            var sql = $"""
                SELECT DATE_FORMAT({columnaFecha}, '%Y-%m') AS Mes, {agg} AS Total
                FROM {tabla}
                WHERE empresa_id = @EmpresaId
                  {filtroExtra ?? ""}
                  AND {columnaFecha} >= DATE_SUB(CURDATE(), INTERVAL @Meses MONTH)
                GROUP BY Mes ORDER BY Mes;
                """;

            var filas = (await con.QueryAsync<(string Mes, decimal Total)>(sql, new { EmpresaId = empresaId, Meses = meses - 1 })).ToList();
            var baseMeses = RellenarMeses(meses, 0);
            var mapa = filas.ToDictionary(f => f.Mes, f => (double)f.Total);

            for (var i = 0; i < baseMeses.Labels.Length; i++)
            {
                var key = MesClaveDesdeIndice(i, meses);
                if (mapa.TryGetValue(key, out var v))
                    baseMeses.Values[i] = v;
            }

            return baseMeses;
        }

        private static (string[] Labels, double[] Values) RellenarMeses(int cantidad, double valor)
        {
            var labels = new string[cantidad];
            var values = new double[cantidad];
            var hoy = DateTime.Today;
            for (var i = cantidad - 1; i >= 0; i--)
            {
                var d = hoy.AddMonths(-i);
                labels[cantidad - 1 - i] = d.ToString("MMM", System.Globalization.CultureInfo.GetCultureInfo("es-PE"));
                values[cantidad - 1 - i] = valor;
            }
            return (labels, values);
        }

        private static string MesClaveDesdeIndice(int indice, int totalMeses)
        {
            var d = DateTime.Today.AddMonths(-(totalMeses - 1 - indice));
            return d.ToString("yyyy-MM");
        }

        private static string FormatoMoneda(decimal monto, string simbolo) =>
            $"{simbolo} {monto:N2}";

        private static string TextoVariacion(decimal actual, decimal anterior)
        {
            if (anterior == 0)
                return actual > 0 ? "100% vs mes anterior" : "0% vs mes anterior";
            var pct = Math.Round((actual - anterior) / anterior * 100, 1);
            return $"{Math.Abs(pct):0.#}% vs mes anterior";
        }

        private static string TipoVariacion(decimal actual, decimal anterior)
        {
            if (actual > anterior) return "up";
            if (actual < anterior) return "down";
            return "warning";
        }
    }
}
