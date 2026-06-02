using Hevelab2026.Data;
using Microsoft.EntityFrameworkCore;

namespace Hevelab2026.Services.Compras;

public class CompraService : ICompraService
{
    private readonly ApplicationDbContext _db;

    public CompraService(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<SolicitudCotizacionVm>> GetSolicitudesAsync(CancellationToken ct = default)
    {
        if (_db.IsMySql)
            return Array.Empty<SolicitudCotizacionVm>();

        return await _db.SolicitudesCotizacion
            .Include(s => s.Proveedor)
            .AsNoTracking()
            .OrderByDescending(s => s.FechaCreacion)
            .Select(s => new SolicitudCotizacionVm
            {
                Id = s.Id,
                Referencia = s.NumeroReferencia,
                Proveedor = s.Proveedor != null ? s.Proveedor.RazonSocial : "",
                Comprador = s.Comprador ?? "",
                FechaCreacion = s.FechaCreacion,
                FechaLimite = s.FechaLimite,
                Total = s.TotalEstimado,
                Estado = s.Estado
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OrdenCompraVm>> GetOrdenesCompraAsync(CancellationToken ct = default) =>
        await _db.PedidosCompra
            .Include(p => p.Proveedor)
            .Include(p => p.EstadoPedido)
            .AsNoTracking()
            .OrderByDescending(p => p.FechaEmision)
            .Select(p => new OrdenCompraVm
            {
                Id = p.Id,
                NumeroDocumento = p.NumeroDocumento,
                Proveedor = p.Proveedor != null ? p.Proveedor.RazonSocial : "",
                FechaEmision = p.FechaEmision,
                Total = p.Total,
                Estado = p.EstadoPedido != null ? p.EstadoPedido.Nombre : ""
            })
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProveedorVm>> GetProveedoresAsync(string? search, CancellationToken ct = default)
    {
        var q = _db.Socios.AsNoTracking().Where(s => s.EsProveedor);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var t = search.ToLower();
            q = q.Where(s => s.RazonSocial.ToLower().Contains(t) || s.NumeroDocumento.Contains(t));
        }
        return await q.Select(s => new ProveedorVm
        {
            Id = s.Id,
            Codigo = s.Codigo,
            RazonSocial = s.RazonSocial,
            Ruc = s.NumeroDocumento,
            Telefono = s.Telefono,
            Email = s.Correo,
            Activo = s.Activo
        }).ToListAsync(ct);
    }
}
