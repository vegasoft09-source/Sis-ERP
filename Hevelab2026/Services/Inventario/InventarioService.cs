using Hevelab2026.Common.Exceptions;
using Hevelab2026.Data;
using Hevelab2026.Domain.Entities;
using Hevelab2026.Models;
using Hevelab2026.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hevelab2026.Services.Inventario;

public class InventarioService : IInventarioService
{
    private readonly ApplicationDbContext _db;
    private readonly IUnitOfWork _uow;

    public InventarioService(ApplicationDbContext db, IUnitOfWork uow)
    {
        _db = db;
        _uow = uow;
    }

    public async Task<IReadOnlyList<RecepcionModel>> GetRecepcionesAsync(string? estado, CancellationToken ct = default)
    {
        var q = _db.Transferencias
            .Include(t => t.Estado)
            .Include(t => t.AlmacenOrigen)
            .Include(t => t.AlmacenDestino)
            .Include(t => t.Contacto)
            .Where(t => t.TipoOperacion == "recepcion")
            .AsNoTracking();

        if (!string.IsNullOrEmpty(estado))
            q = q.Where(t => t.Estado!.Codigo == estado);

        var list = await q.OrderByDescending(t => t.FechaProgramada).ToListAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task<RecepcionModel?> GetRecepcionAsync(string referencia, CancellationToken ct = default)
    {
        var t = await _db.Transferencias
            .Include(x => x.Estado)
            .Include(x => x.AlmacenOrigen)
            .Include(x => x.AlmacenDestino)
            .FirstOrDefaultAsync(x => x.NumeroReferencia == referencia, ct);
        return t == null ? null : Map(t);
    }

    public async Task<RecepcionModel> CrearRecepcionAsync(RecepcionModel model, CancellationToken ct = default)
    {
        var empresaId = await _db.Empresas.Select(e => e.Id).FirstAsync(ct);
        var estadoBorrador = await _db.EstadosTransferencia.FirstAsync(e => e.Codigo == "BORRADOR", ct);
        var count = await _db.Transferencias.CountAsync(ct);

        var entity = new Transferencia
        {
            EmpresaId = empresaId,
            NumeroReferencia = string.IsNullOrEmpty(model.Referencia) ? $"WH/IN/{(count + 1):D5}" : model.Referencia,
            TipoOperacion = "recepcion",
            DocumentoOrigen = model.DocumentoOrigen,
            ContactoNombre = model.Contacto,
            FechaProgramada = model.FechaProgramada,
            EstadoId = estadoBorrador.Id,
            Observaciones = model.Nota
        };

        var alm = await _db.Almacenes.Where(a => a.EmpresaId == empresaId).ToListAsync(ct);
        if (alm.Count >= 2)
        {
            entity.AlmacenOrigenId = alm[0].Id;
            entity.AlmacenDestinoId = alm[1].Id;
            model.Desde = alm[0].Nombre;
            model.A = alm[1].Nombre;
        }

        await _uow.Transferencias.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        model.Referencia = entity.NumeroReferencia;
        model.Estado = estadoBorrador.Codigo;
        return model;
    }

    private static RecepcionModel Map(Transferencia t) => new()
    {
        Referencia = t.NumeroReferencia,
        Desde = t.AlmacenOrigen?.Nombre ?? t.Contacto?.RazonSocial ?? "",
        A = t.AlmacenDestino?.Nombre ?? "",
        Contacto = t.ContactoNombre ?? t.Contacto?.RazonSocial ?? "",
        FechaProgramada = t.FechaProgramada ?? DateTime.UtcNow,
        DocumentoOrigen = t.DocumentoOrigen ?? "",
        Estado = t.Estado?.Codigo ?? "BORRADOR",
        Nota = t.Observaciones ?? ""
    };
}
