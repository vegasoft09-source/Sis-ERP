using Hevelab2026.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hevelab2026.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public bool IsMySql =>
        Database.ProviderName?.Contains("MySql", StringComparison.OrdinalIgnoreCase) == true;

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Moneda> Monedas => Set<Moneda>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Socio> Socios => Set<Socio>();
    public DbSet<Almacen> Almacenes => Set<Almacen>();
    public DbSet<ProductoEntity> Productos => Set<ProductoEntity>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<EstadoPedidoVenta> EstadosPedidoVenta => Set<EstadoPedidoVenta>();
    public DbSet<PedidoVenta> PedidosVenta => Set<PedidoVenta>();
    public DbSet<DetallePedidoVenta> DetallesPedidoVenta => Set<DetallePedidoVenta>();
    public DbSet<EstadoPedidoCompra> EstadosPedidoCompra => Set<EstadoPedidoCompra>();
    public DbSet<SolicitudCotizacion> SolicitudesCotizacion => Set<SolicitudCotizacion>();
    public DbSet<PedidoCompra> PedidosCompra => Set<PedidoCompra>();
    public DbSet<DetallePedidoCompra> DetallesPedidoCompra => Set<DetallePedidoCompra>();
    public DbSet<EstadoTransferencia> EstadosTransferencia => Set<EstadoTransferencia>();
    public DbSet<Transferencia> Transferencias => Set<Transferencia>();
    public DbSet<TransferenciaLinea> TransferenciasLinea => Set<TransferenciaLinea>();
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (Database.ProviderName?.Contains("MySql", StringComparison.OrdinalIgnoreCase) == true)
        {
            ErpMySqlModelConfigurator.Configure(modelBuilder);
            return;
        }

        ConfigureInMemoryDemo(modelBuilder);
    }

    private static void ConfigureInMemoryDemo(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entity.ClrType))
            {
                modelBuilder.Entity(entity.ClrType)
                    .Property(nameof(BaseEntity.Eliminado))
                    .HasDefaultValue(false);
            }
        }

        modelBuilder.Entity<Socio>(e =>
        {
            e.ToTable("socio");
            e.HasIndex(x => new { x.EmpresaId, x.NumeroDocumento });
            e.HasQueryFilter(x => !x.Eliminado);
        });

        modelBuilder.Entity<ProductoEntity>(e =>
        {
            e.ToTable("productos");
            e.HasIndex(x => new { x.EmpresaId, x.CodigoProducto }).IsUnique();
            e.HasQueryFilter(x => !x.Eliminado);
        });

        modelBuilder.Entity<PedidoVenta>(e =>
        {
            e.ToTable("pedido_venta");
            e.HasMany(x => x.Detalles).WithOne(x => x.PedidoVenta).HasForeignKey(x => x.PedidoVentaId);
            e.HasQueryFilter(x => !x.Eliminado);
        });

        modelBuilder.Entity<PedidoCompra>(e =>
        {
            e.ToTable("pedido_compra");
            e.HasQueryFilter(x => !x.Eliminado);
        });

        modelBuilder.Entity<Transferencia>(e =>
        {
            e.ToTable("transferencia");
            e.HasQueryFilter(x => !x.Eliminado);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuario");
            e.HasIndex(x => x.NombreUsuario).IsUnique();
            e.HasQueryFilter(x => !x.Eliminado);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreadoEn = now;
            if (entry.State == EntityState.Modified)
                entry.Entity.ActualizadoEn = now;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
