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
    public class LibroMayorController : Controller
    {
        private readonly string _connectionString;

        public LibroMayorController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SisErp")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión.");
        }

        public async Task<IActionResult> Index(string? mes, string? anio)
        {
            ViewData["Breadcrumbs"] = new List<(string Label, string Url)>
            {
                ("Dashboard", "/"),
                ("Contabilidad", "#"),
                ("Libro Mayor", "/LibroMayor")
            };

            string mesVal = string.IsNullOrEmpty(mes) ? DateTime.Now.ToString("MM") : mes;
            string anioVal = string.IsNullOrEmpty(anio) ? DateTime.Now.Year.ToString() : anio;

            int mesInt = int.Parse(mesVal);
            int anioInt = int.Parse(anioVal);

            var firstDayOfSelectedMonth = new DateTime(anioInt, mesInt, 1);
            var lastDayOfSelectedMonth = firstDayOfSelectedMonth.AddMonths(1).AddDays(-1);

            var prevMonthDate = firstDayOfSelectedMonth.AddMonths(-1);
            var firstDayOfPrevMonth = new DateTime(prevMonthDate.Year, prevMonthDate.Month, 1);
            var lastDayOfPrevMonth = firstDayOfPrevMonth.AddMonths(1).AddDays(-1);

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            // Get available months from actual data for period navigation
            var availableMonths = (await conn.QueryAsync(
                @"SELECT DISTINCT MONTH(fecha_emision) AS Mes, YEAR(fecha_emision) AS Anio 
                  FROM account_move 
                  WHERE estado != 'Anulada' 
                  ORDER BY Anio DESC, Mes DESC")).ToList();

            ViewBag.AvailableMonths = availableMonths.Select(m => new { Mes = (int)m.Mes, Anio = (int)m.Anio }).ToList();

            var accountsDb = (await conn.QueryAsync("SELECT id, codigo, nombre, tipo FROM account_account WHERE activo = 1")).ToList();

            var accountsList = new List<CuentaMayor>();
            foreach (var a in accountsDb)
            {
                int accountId = a.id;
                string codigo = a.codigo;
                string nombre = a.nombre;
                string tipo = a.tipo;

                var initialBalanceDb = await conn.ExecuteScalarAsync<double?>(
                    @"SELECT SUM(debe_empresa - haber_empresa) 
                      FROM account_move_line aml
                      INNER JOIN account_move m ON aml.move_id = m.id
                      WHERE aml.cuenta_id = @AccountId AND m.fecha_emision < @StartDate AND m.estado != 'Anulada';",
                    new { AccountId = accountId, StartDate = firstDayOfSelectedMonth });

                var debeAntDb = await conn.ExecuteScalarAsync<double?>(
                    @"SELECT SUM(debe_empresa) 
                      FROM account_move_line aml
                      INNER JOIN account_move m ON aml.move_id = m.id
                      WHERE aml.cuenta_id = @AccountId AND m.fecha_emision >= @PrevStart AND m.fecha_emision <= @PrevEnd AND m.estado != 'Anulada';",
                    new { AccountId = accountId, PrevStart = firstDayOfPrevMonth, PrevEnd = lastDayOfPrevMonth });

                var haberAntDb = await conn.ExecuteScalarAsync<double?>(
                    @"SELECT SUM(haber_empresa) 
                      FROM account_move_line aml
                      INNER JOIN account_move m ON aml.move_id = m.id
                      WHERE aml.cuenta_id = @AccountId AND m.fecha_emision >= @PrevStart AND m.fecha_emision <= @PrevEnd AND m.estado != 'Anulada';",
                    new { AccountId = accountId, PrevStart = firstDayOfPrevMonth, PrevEnd = lastDayOfPrevMonth });

                double saldoInicial = initialBalanceDb ?? 0;
                double debeAnt = debeAntDb ?? 0;
                double haberAnt = haberAntDb ?? 0;
                double saldoAnt = saldoInicial;

                accountsList.Add(new CuentaMayor
                {
                    CodigoCuenta = codigo,
                    NombreCuenta = nombre,
                    TipoCuenta = tipo,
                    SaldoInicial = saldoInicial,
                    DebeAnt = debeAnt,
                    HaberAnt = haberAnt,
                    SaldoAnt = saldoAnt
                });
            }

            const string sqlMovements = @"
                SELECT 
                    a.codigo AS Cuenta,
                    DATE_FORMAT(m.fecha_emision, '%Y-%m-%d') AS Fecha,
                    m.numero_documento AS Asiento,
                    aml.nombre AS Descripcion,
                    aml.debe_empresa AS Debe,
                    aml.haber_empresa AS Haber,
                    COALESCE(aml.debe_empresa * 0.9, 0) AS DebeAnt,
                    COALESCE(aml.haber_empresa * 0.9, 0) AS HaberAnt
                FROM account_move_line aml
                INNER JOIN account_move m ON aml.move_id = m.id
                INNER JOIN account_account a ON aml.cuenta_id = a.id
                WHERE m.estado != 'Anulada'
                  AND m.fecha_emision >= @StartDate
                  AND m.fecha_emision <= @EndDate
                ORDER BY m.fecha_emision ASC, aml.id ASC;";

            var movements = (await conn.QueryAsync<MovimientoMayor>(sqlMovements, new { StartDate = firstDayOfSelectedMonth, EndDate = lastDayOfSelectedMonth })).ToList();

            var viewModel = new LibroMayorViewModel
            {
                Cuentas = accountsList,
                Movimientos = movements
            };

            ViewBag.SelectedMes = mesVal;
            ViewBag.SelectedAnio = anioVal;

            return View(viewModel);
        }
    }
}
