using Hevelab2026.Data;
using Hevelab2026.Models;
using Microsoft.EntityFrameworkCore;

namespace Hevelab2026.Services.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _db;

    public DashboardService(ApplicationDbContext db) => _db = db;

    public async Task<DashboardViewModel> GetDashboardAsync(CancellationToken ct = default)
    {
        var clientesCount = await _db.Socios.CountAsync(s => s.EsCliente && s.Activo, ct);
        var proveedoresCount = await _db.Socios.CountAsync(s => s.EsProveedor && s.Activo, ct);
        var productosCount = await _db.Productos.CountAsync(ct);
        var cotizacionesCount = await _db.PedidosVenta.CountAsync(
            p => p.NumeroDocumento.StartsWith("COT") || p.NumeroDocumento.StartsWith("cot"), ct);
        var ordenesCompraCount = await _db.PedidosCompra.CountAsync(ct);
        var stockTotal = await _db.Stocks.SumAsync(s => (decimal?)s.Cantidad, ct) ?? 0m;

        var ventasMes = await _db.PedidosVenta
            .Where(p => p.FechaEmision.Month == DateTime.UtcNow.Month && p.FechaEmision.Year == DateTime.UtcNow.Year)
            .SumAsync(p => (decimal?)p.Total, ct) ?? 0m;

        var recentSales = await _db.PedidosVenta
            .Include(p => p.Cliente)
            .Include(p => p.EstadoPedido)
            .OrderByDescending(p => p.FechaEmision)
            .Take(5)
            .AsNoTracking()
            .ToListAsync(ct);

        return new DashboardViewModel
        {
            Metrics =
            [
                new MetricCard
                {
                    Title = "Clientes activos",
                    Value = clientesCount.ToString("N0"),
                    TrendText = $"{proveedoresCount} proveedores",
                    TrendType = "up",
                    ThemeColor = "primary",
                    IconSvg = @"<path d='M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2'/><circle cx='9' cy='7' r='4'/>"
                },
                new MetricCard
                {
                    Title = "Productos",
                    Value = productosCount.ToString("N0"),
                    TrendText = $"Stock total: {stockTotal:N0} u.",
                    TrendType = "up",
                    ThemeColor = "success",
                    IconSvg = @"<path d='M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z'/>"
                },
                new MetricCard
                {
                    Title = "Cotizaciones",
                    Value = cotizacionesCount.ToString("N0"),
                    TrendText = $"{ordenesCompraCount} órdenes de compra",
                    TrendType = "warning",
                    ThemeColor = "warning",
                    IconSvg = @"<circle cx='9' cy='21' r='1'/><circle cx='20' cy='21' r='1'/><path d='M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6'/>"
                },
                new MetricCard
                {
                    Title = "Ventas del mes",
                    Value = $"S/ {ventasMes:N2}",
                    TrendText = "Desde pedidos de venta",
                    TrendType = "up",
                    ThemeColor = "danger",
                    IconSvg = @"<line x1='18' y1='20' x2='18' y2='10'/><line x1='12' y1='20' x2='12' y2='4'/><line x1='6' y1='20' x2='6' y2='14'/>"
                }
            ],
            QuickAccessModules =
            [
                new QuickAccessModule { Name = "Clientes", Description = "Directorio de clientes desde la base de datos.", Controller = "Clientes", IconSvg = @"<path d='M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2'/><circle cx='9' cy='7' r='4'/>", BadgeText = clientesCount.ToString(), BadgeType = "primary" },
                new QuickAccessModule { Name = "Productos", Description = "Catálogo e inventario de productos.", Controller = "Producto", IconSvg = @"<path d='M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z'/>", BadgeText = productosCount.ToString(), BadgeType = "success" },
                new QuickAccessModule { Name = "Cotizaciones", Description = "Pedidos de venta / cotizaciones.", Controller = "Cotizaciones", IconSvg = @"<circle cx='9' cy='21' r='1'/><circle cx='20' cy='21' r='1'/><path d='M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6'/>", BadgeText = cotizacionesCount.ToString(), BadgeType = "warning" },
                new QuickAccessModule { Name = "Compras", Description = "Proveedores y órdenes de compra.", Controller = "OrdenesDeCompra", IconSvg = @"<path d='M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z'/>", BadgeText = ordenesCompraCount.ToString(), BadgeType = "danger" },
                new QuickAccessModule { Name = "Recepciones", Description = "Transferencias y recepciones de almacén.", Controller = "RecepcionesView", IconSvg = @"<polyline points='22 12 18 12 15 21 9 3 6 12 2 12'/>", BadgeText = "Inventario", BadgeType = "info" }
            ],
            RecentSales = recentSales.Select(p => new RecentSale
            {
                InvoiceNumber = p.NumeroDocumento,
                Customer = p.Cliente?.RazonSocial ?? "—",
                Amount = $"S/ {p.Total:N2}",
                Status = p.EstadoPedido?.Nombre ?? "—",
                StatusType = MapEstado(p.EstadoPedido?.Nombre),
                Date = p.FechaEmision.ToLocalTime().ToString("dd MMM yyyy HH:mm")
            }).ToList(),
            RecentActivities = recentSales.Select(p => new RecentActivity
            {
                Description = $"Pedido {p.NumeroDocumento} — {p.Cliente?.RazonSocial ?? "Cliente"}",
                TimeAgo = p.FechaEmision.ToLocalTime().ToString("dd/MM/yyyy"),
                User = "Sistema",
                Type = "info"
            }).ToList()
        };
    }

    private static string MapEstado(string? nombre) =>
        nombre?.ToLowerInvariant() switch
        {
            var n when n.Contains("aprob") || n.Contains("pagad") || n.Contains("confirm") => "success",
            var n when n.Contains("pend") || n.Contains("borrador") => "warning",
            var n when n.Contains("rechaz") || n.Contains("venc") || n.Contains("cancel") => "danger",
            _ => "info"
        };
}
