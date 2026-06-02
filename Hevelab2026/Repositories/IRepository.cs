using System.Linq.Expressions;
using Hevelab2026.Common;
using Hevelab2026.Domain.Entities;

namespace Hevelab2026.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<PagedResult<T>> GetPagedAsync(
        PagedQuery query,
        Expression<Func<T, bool>>? filter = null,
        CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task SoftDeleteAsync(int id, CancellationToken ct = default);
    IQueryable<T> Query();
}

public interface IUnitOfWork
{
    IRepository<Socio> Socios { get; }
    IRepository<ProductoEntity> Productos { get; }
    IRepository<PedidoVenta> PedidosVenta { get; }
    IRepository<PedidoCompra> PedidosCompra { get; }
    IRepository<SolicitudCotizacion> SolicitudesCotizacion { get; }
    IRepository<Transferencia> Transferencias { get; }
    IRepository<Usuario> Usuarios { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
