using Hevelab2026.Common.Exceptions;
using Hevelab2026.Data;
using Hevelab2026.Domain.Entities;
using Hevelab2026.Models;
using Hevelab2026.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hevelab2026.Services.Ventas;

public class PedidoVentaService : IPedidoVentaService
{
    private readonly ApplicationDbContext _db;
    private readonly IUnitOfWork _uow;

    public PedidoVentaService(ApplicationDbContext db, IUnitOfWork uow)
    {
        _db = db;
        _uow = uow;
    }

    public async Task<IReadOnlyList<Cotizacion>> GetCotizacionesAsync(string? fecha, string? cliente, string? estado, CancellationToken ct = default)
    {
        var q = _db.PedidosVenta
            .Include(p => p.Cliente)
            .Include(p => p.EstadoPedido)
            .Include(p => p.Vendedor)
            .AsNoTracking();

        if (!_db.IsMySql)
            q = q.Where(p => p.NumeroDocumento.StartsWith("COT") || p.NumeroDocumento.StartsWith("cot"));

        if (!string.IsNullOrWhiteSpace(cliente))
        {
            var c = cliente.ToLower();
            q = q.Where(x => x.Cliente!.RazonSocial.ToLower().Contains(c));
        }

        if (!string.IsNullOrEmpty(estado) && estado != "Todos los estados")
            q = q.Where(x => x.EstadoPedido!.Nombre == estado);

        var list = await q.OrderByDescending(x => x.FechaEmision).ToListAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task<Cotizacion?> GetCotizacionAsync(int id, CancellationToken ct = default)
    {
        var p = await _db.PedidosVenta
            .Include(x => x.Cliente)
            .Include(x => x.EstadoPedido)
            .Include(x => x.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return p == null ? null : Map(p);
    }

    public async Task<Cotizacion> SaveCotizacionAsync(Cotizacion model, CancellationToken ct = default)
    {
        PedidoVenta entity;
        if (model.Id > 0)
        {
            entity = await _db.PedidosVenta
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == model.Id, ct)
                ?? throw new NotFoundException("Cotización no encontrada");
        }
        else
        {
            var estadoId = await _db.EstadosPedidoVenta
                .Where(e => e.Codigo == "PENDIENTE" || e.Nombre.Contains("Pendiente"))
                .Select(e => e.Id).FirstOrDefaultAsync(ct);
            if (estadoId == 0)
            {
                var est = new EstadoPedidoVenta { Nombre = "Pendiente", Codigo = "PENDIENTE", Secuencia = 1 };
                _db.EstadosPedidoVenta.Add(est);
                await _db.SaveChangesAsync(ct);
                estadoId = est.Id;
            }
            var count = await _db.PedidosVenta.CountAsync(ct);
            entity = new PedidoVenta
            {
                NumeroDocumento = $"COT-{(count + 1):D3}",
                EstadoPedidoVentaId = estadoId,
                FechaEmision = DateTime.UtcNow
            };
            await _uow.PedidosVenta.AddAsync(entity, ct);
        }

        var cliente = await _db.Socios.FirstOrDefaultAsync(s =>
            s.RazonSocial == model.RazonSocial || s.RazonSocial == model.Cliente, ct);
        if (cliente != null) entity.ClienteId = cliente.Id;

        entity.Subtotal = model.Subtotal;
        entity.DescuentoTotal = model.Descuento;
        entity.TotalImpuestos = model.Impuestos;
        entity.Total = model.Total;
        entity.CondicionPago = model.CondicionPago;
        entity.Observaciones = model.Notas;
        entity.DireccionCliente = model.Direccion;
        entity.MetodoPago = model.MetodoPago;

        if (!string.IsNullOrEmpty(model.Estado))
        {
            var est = await _db.EstadosPedidoVenta.FirstOrDefaultAsync(e => e.Nombre == model.Estado, ct);
            if (est != null) entity.EstadoPedidoVentaId = est.Id;
        }

        await _uow.SaveChangesAsync(ct);
        model.Id = entity.Id;
        model.NumeroPedido = entity.NumeroDocumento;
        return model;
    }

    public async Task<int> ConvertirAOrdenAsync(int cotizacionId, CancellationToken ct = default)
    {
        var cot = await _db.PedidosVenta.FirstOrDefaultAsync(p => p.Id == cotizacionId, ct)
            ?? throw new NotFoundException("Cotización no encontrada");

        var convertido = await _db.EstadosPedidoVenta
            .FirstOrDefaultAsync(e => e.Codigo == "CONVERTIDO" || e.Nombre.Contains("Convertido"), ct);
        if (convertido != null)
            cot.EstadoPedidoVentaId = convertido.Id;
        cot.TipoDocumento = "orden";
        await _uow.SaveChangesAsync(ct);
        return cot.Id;
    }

    private static Cotizacion Map(PedidoVenta p) => new()
    {
        Id = p.Id,
        NumeroPedido = p.NumeroDocumento,
        FechaCreacion = p.FechaEmision,
        Cliente = p.Cliente?.RazonSocial ?? "",
        RazonSocial = p.Cliente?.RazonSocial ?? "",
        RUC = p.Cliente?.NumeroDocumento ?? "",
        Subtotal = p.Subtotal,
        Total = p.Total,
        Estado = p.EstadoPedido?.Nombre ?? "Pendiente",
        CondicionPago = p.CondicionPago ?? "",
        Vendedor = p.Vendedor != null ? $"{p.Vendedor.Nombre} {p.Vendedor.Apellido}" : "",
        Descuento = p.DescuentoTotal,
        Impuestos = p.TotalImpuestos,
        Notas = p.Observaciones ?? "",
        Direccion = p.DireccionCliente ?? "",
        MetodoPago = p.MetodoPago ?? ""
    };
}
