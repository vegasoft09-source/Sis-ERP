using System.Linq.Expressions;
using Hevelab2026.Common;
using Hevelab2026.Data;
using Hevelab2026.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hevelab2026.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _db;
    protected readonly DbSet<T> _set;

    public Repository(ApplicationDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await _set.AsNoTracking().ToListAsync(ct);

    public virtual async Task<PagedResult<T>> GetPagedAsync(
        PagedQuery query,
        Expression<Func<T, bool>>? filter = null,
        CancellationToken ct = default)
    {
        var q = _set.AsNoTracking().AsQueryable();
        if (filter != null) q = q.Where(filter);

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<T>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct);
        return entity;
    }

    public virtual Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _set.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity == null) return;
        entity.Eliminado = true;
        entity.Activo = false;
        entity.ActualizadoEn = DateTime.UtcNow;
        await UpdateAsync(entity, ct);
    }

    public IQueryable<T> Query() => _set.AsQueryable();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;

    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
        Socios = new Repository<Socio>(db);
        Productos = new Repository<ProductoEntity>(db);
        PedidosVenta = new Repository<PedidoVenta>(db);
        PedidosCompra = new Repository<PedidoCompra>(db);
        SolicitudesCotizacion = new Repository<SolicitudCotizacion>(db);
        Transferencias = new Repository<Transferencia>(db);
        Usuarios = new Repository<Usuario>(db);
    }

    public IRepository<Socio> Socios { get; }
    public IRepository<ProductoEntity> Productos { get; }
    public IRepository<PedidoVenta> PedidosVenta { get; }
    public IRepository<PedidoCompra> PedidosCompra { get; }
    public IRepository<SolicitudCotizacion> SolicitudesCotizacion { get; }
    public IRepository<Transferencia> Transferencias { get; }
    public IRepository<Usuario> Usuarios { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
