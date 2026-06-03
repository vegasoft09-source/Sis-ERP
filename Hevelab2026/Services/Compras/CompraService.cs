using Hevelab2026.Data;
using Hevelab2026.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hevelab2026.Services.Compras;

public class CompraService : ICompraService
{
    private readonly ApplicationDbContext _db;

    public CompraService(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<SolicitudCotizacionVm>> GetSolicitudesAsync(CancellationToken ct = default)
    {
        if (_db.IsMySql)
        {
            var borradorIds = await _db.EstadosPedidoCompra
                .Where(e => e.Codigo == "BORRADOR" || e.Nombre.Contains("Borrador") || e.Nombre.Contains("Nuevo"))
                .Select(e => e.Id)
                .ToListAsync(ct);

            var q = _db.PedidosCompra
                .Include(p => p.Proveedor)
                .Include(p => p.EstadoPedido)
                .AsNoTracking();

            if (borradorIds.Count > 0)
                q = q.Where(p => borradorIds.Contains(p.EstadoPedidoCompraId)
                    || p.NumeroDocumento.StartsWith("SOL")
                    || p.NumeroDocumento.StartsWith("RFQ"));
            else
                q = q.Where(p => p.NumeroDocumento.StartsWith("SOL") || p.NumeroDocumento.StartsWith("RFQ"));

            return await q.OrderByDescending(p => p.FechaEmision)
                .Select(p => new SolicitudCotizacionVm
                {
                    Id = p.Id,
                    Referencia = p.NumeroDocumento,
                    Proveedor = p.Proveedor != null ? p.Proveedor.RazonSocial : "",
                    Comprador = "",
                    FechaCreacion = p.FechaEmision,
                    FechaLimite = null,
                    Total = p.Total,
                    Estado = p.EstadoPedido != null ? p.EstadoPedido.Nombre : "Borrador"
                })
                .ToListAsync(ct);
        }

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

    public async Task<IReadOnlyList<OrdenCompraVm>> GetOrdenesCompraAsync(CancellationToken ct = default)
    {
        var borradorIds = await _db.EstadosPedidoCompra
            .Where(e => e.Codigo == "BORRADOR" || e.Nombre.Contains("Borrador"))
            .Select(e => e.Id)
            .ToListAsync(ct);

        var q = _db.PedidosCompra
            .Include(p => p.Proveedor)
            .Include(p => p.EstadoPedido)
            .AsNoTracking();

        if (borradorIds.Count > 0)
            q = q.Where(p => !borradorIds.Contains(p.EstadoPedidoCompraId)
                && !p.NumeroDocumento.StartsWith("SOL")
                && !p.NumeroDocumento.StartsWith("RFQ"));
        else
            q = q.Where(p => !p.NumeroDocumento.StartsWith("SOL") && !p.NumeroDocumento.StartsWith("RFQ"));

        return await q.OrderByDescending(p => p.FechaEmision)
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
    }

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

    public async Task<int> CrearProveedorAsync(int empresaId, string razonSocial, string tipoDoc, string numeroDoc,
        string? telefono, string? email, string? direccion, CancellationToken ct = default)
    {
        var count = await _db.Socios.CountAsync(s => s.EmpresaId == empresaId && s.EsProveedor, ct);
        var entity = new Socio
        {
            EmpresaId = empresaId,
            Codigo = $"PRO-{(count + 1):D3}",
            RazonSocial = razonSocial.Trim(),
            TipoDocumento = tipoDoc,
            NumeroDocumento = numeroDoc.Trim(),
            Telefono = telefono,
            Correo = email,
            Direccion = direccion,
            EsProveedor = true,
            EsCliente = false,
            Activo = true
        };
        _db.Socios.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task<int> CrearSolicitudAsync(int empresaId, int proveedorId, string? referencia, DateTime? fechaLimite,
        decimal total, string? observaciones, int? compradorId, CancellationToken ct = default)
    {
        if (!_db.IsMySql)
        {
            var sol = new SolicitudCotizacion
            {
                EmpresaId = empresaId,
                ProveedorId = proveedorId,
                NumeroReferencia = referencia ?? $"SOL-{DateTime.UtcNow:yyyyMMddHHmm}",
                FechaLimite = fechaLimite,
                TotalEstimado = total,
                Observaciones = observaciones,
                Comprador = compradorId?.ToString(),
                Estado = "NUEVO"
            };
            _db.SolicitudesCotizacion.Add(sol);
            await _db.SaveChangesAsync(ct);
            return sol.Id;
        }

        var estadoId = await _db.EstadosPedidoCompra
            .Where(e => e.Codigo == "BORRADOR" || e.Nombre.Contains("Borrador"))
            .Select(e => e.Id)
            .FirstOrDefaultAsync(ct);

        if (estadoId == 0)
        {
            var est = new EstadoPedidoCompra { Nombre = "Borrador", Codigo = "BORRADOR", Secuencia = 1 };
            _db.EstadosPedidoCompra.Add(est);
            await _db.SaveChangesAsync(ct);
            estadoId = est.Id;
        }

        var comprador = compradorId ?? await _db.Usuarios
            .Where(u => u.EmpresaId == empresaId && u.Activo)
            .Select(u => (int?)u.Id)
            .FirstOrDefaultAsync(ct)
            ?? await _db.Usuarios.Select(u => (int?)u.Id).FirstOrDefaultAsync(ct);

        if (comprador is null or 0)
            throw new InvalidOperationException("No hay usuarios en la base de datos para asignar como comprador.");

        var count = await _db.PedidosCompra.CountAsync(ct);
        var pedido = new PedidoCompra
        {
            EmpresaId = empresaId,
            ProveedorId = proveedorId,
            CompradorId = comprador,
            EstadoPedidoCompraId = estadoId,
            NumeroDocumento = referencia ?? $"SOL-{(count + 1):D4}",
            FechaEmision = DateTime.UtcNow,
            Total = total,
            Subtotal = total,
            Observaciones = observaciones
        };
        _db.PedidosCompra.Add(pedido);
        await _db.SaveChangesAsync(ct);
        return pedido.Id;
    }
}
