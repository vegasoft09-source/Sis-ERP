using Hevelab2026.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Hevelab2026.Data;

/// <summary>
/// Mapeo EF al esquema MySQL de Hostinger (tablas snake_case en documentacion.html).
/// </summary>
public static class ErpMySqlModelConfigurator
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        IgnoreSoftDelete(modelBuilder);

        modelBuilder.Entity<Moneda>(e =>
        {
            e.ToTable("moneda");
            e.Ignore(x => x.Eliminado);
            e.Ignore(x => x.CreadoEn);
            e.Ignore(x => x.ActualizadoEn);
        });

        modelBuilder.Entity<Empresa>(e =>
        {
            e.ToTable("empresa");
            e.Ignore(x => x.Eliminado);
            e.Property(x => x.CreadoEn).HasColumnName("creado_en");
            e.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en");
        });

        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("rol");
            e.Ignore(x => x.Eliminado);
            e.Ignore(x => x.CreadoEn);
            e.Ignore(x => x.ActualizadoEn);
        });

        modelBuilder.Entity<Permiso>(e =>
        {
            e.ToTable("permiso");
            e.Ignore(x => x.Modulo);
            e.Ignore(x => x.Eliminado);
            e.Ignore(x => x.CreadoEn);
            e.Ignore(x => x.ActualizadoEn);
        });

        modelBuilder.Entity<RolPermiso>(e =>
        {
            e.ToTable("rol_permiso");
            e.Ignore(x => x.Activo);
            e.Ignore(x => x.Eliminado);
            e.Ignore(x => x.CreadoEn);
            e.Ignore(x => x.ActualizadoEn);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuario");
            e.Ignore(x => x.Eliminado);
            e.Ignore(x => x.RefreshToken);
            e.Ignore(x => x.RefreshTokenExpira);
            e.Property(x => x.ContrasenaHash).HasColumnName("contrasena");
            e.Property(x => x.NombreUsuario).HasColumnName("nombre_usuario");
            e.Property(x => x.UltimoAcceso).HasColumnName("ultimo_acceso");
            e.Property(x => x.CreadoEn).HasColumnName("creado_en");
            e.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en");
            e.HasIndex(x => x.NombreUsuario).IsUnique();
        });

        modelBuilder.Entity<Socio>(e =>
        {
            e.ToTable("socio");
            e.Ignore(x => x.Eliminado);
            e.Ignore(x => x.LimiteCredito);
            e.Ignore(x => x.GrupoClientes);
            e.Ignore(x => x.GrupoProveedor);
            e.Property(x => x.CreadoEn).HasColumnName("creado_en");
            e.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en");
            e.HasIndex(x => new { x.EmpresaId, x.NumeroDocumento });
        });

        modelBuilder.Entity<Almacen>(e =>
        {
            e.ToTable("almacen");
            e.Ignore(x => x.Eliminado);
            e.Property(x => x.CreadoEn).HasColumnName("creado_en");
            e.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en");
        });

        modelBuilder.Entity<ProductoEntity>(e =>
        {
            e.ToTable("productos");
            e.Ignore(x => x.EmpresaId);
            e.Ignore(x => x.Empresa);
            e.Ignore(x => x.Activo);
            e.Ignore(x => x.Eliminado);
            e.Ignore(x => x.CreadoEn);
            e.Ignore(x => x.ActualizadoEn);
            e.Ignore(x => x.Descripcion);
            e.Ignore(x => x.StockMinimo);
            e.HasIndex(x => x.CodigoProducto).IsUnique();
        });

        modelBuilder.Entity<Stock>(e =>
        {
            e.ToTable("stock");
            e.Ignore(x => x.Activo);
            e.Ignore(x => x.Eliminado);
            e.Ignore(x => x.CreadoEn);
            e.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en");
        });

        modelBuilder.Entity<EstadoPedidoVenta>(e =>
        {
            e.ToTable("estado_pedido_venta");
            e.Ignore(x => x.Eliminado);
            e.Ignore(x => x.Secuencia);
            e.Property(x => x.Codigo).HasColumnName("descripcion");
            e.Property(x => x.CreadoEn).HasColumnName("creado_en");
            e.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en");
        });

        modelBuilder.Entity<EstadoPedidoCompra>(e =>
        {
            e.ToTable("estado_pedido_compra");
            e.Ignore(x => x.Eliminado);
            e.Ignore(x => x.Secuencia);
            e.Property(x => x.Codigo).HasColumnName("descripcion");
            e.Property(x => x.CreadoEn).HasColumnName("creado_en");
            e.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en");
        });

        modelBuilder.Entity<EstadoTransferencia>(e =>
        {
            e.ToTable("estado_transferencia");
            e.Ignore(x => x.Eliminado);
            e.Property(x => x.Codigo).HasColumnName("descripcion");
            e.Property(x => x.CreadoEn).HasColumnName("creado_en");
            e.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en");
        });

        modelBuilder.Entity<PedidoVenta>(e =>
        {
            e.ToTable("pedido_venta");
            e.Ignore(x => x.EmpresaId);
            e.Ignore(x => x.Empresa);
            e.Ignore(x => x.TipoDocumento);
            e.Ignore(x => x.CondicionPago);
            e.Ignore(x => x.MetodoPago);
            e.Ignore(x => x.TotalImpuestos);
            e.Ignore(x => x.Activo);
            e.Ignore(x => x.Eliminado);
            e.Property(x => x.CreadoPorId).HasColumnName("creado_por");
            e.Property(x => x.CreadoEn).HasColumnName("creado_en");
            e.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en");
            e.HasMany(x => x.Detalles).WithOne(x => x.PedidoVenta).HasForeignKey(x => x.PedidoVentaId);
        });

        modelBuilder.Entity<DetallePedidoVenta>(e =>
        {
            e.ToTable("detalle_pedido_venta");
            e.Ignore(x => x.Activo);
            e.Ignore(x => x.Eliminado);
            e.Ignore(x => x.CreadoEn);
            e.Ignore(x => x.ActualizadoEn);
        });

        modelBuilder.Entity<PedidoCompra>(e =>
        {
            e.ToTable("pedido_compra");
            e.Ignore(x => x.EmpresaId);
            e.Ignore(x => x.Empresa);
            e.Ignore(x => x.TerminosPago);
            e.Ignore(x => x.Activo);
            e.Ignore(x => x.Eliminado);
            e.Property(x => x.CreadoEn).HasColumnName("creado_en");
            e.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en");
        });

        modelBuilder.Entity<DetallePedidoCompra>(e =>
        {
            e.ToTable("detalle_pedido_compra");
            e.Ignore(x => x.Activo);
            e.Ignore(x => x.Eliminado);
            e.Ignore(x => x.CreadoEn);
            e.Ignore(x => x.ActualizadoEn);
        });

        modelBuilder.Entity<Transferencia>(e =>
        {
            e.ToTable("transferencia");
            e.Ignore(x => x.ContactoNombre);
            e.Ignore(x => x.Activo);
            e.Ignore(x => x.Eliminado);
            e.Property(x => x.CreadoEn).HasColumnName("creado_en");
            e.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en");
        });

        modelBuilder.Entity<TransferenciaLinea>(e =>
        {
            e.ToTable("transferencia_linea");
            e.Ignore(x => x.Activo);
            e.Ignore(x => x.Eliminado);
            e.Ignore(x => x.CreadoEn);
            e.Ignore(x => x.ActualizadoEn);
        });

        modelBuilder.Entity<MovimientoStock>(e =>
        {
            e.ToTable("movimiento_stock");
            e.Ignore(x => x.Activo);
            e.Ignore(x => x.Eliminado);
            e.Ignore(x => x.ActualizadoEn);
            e.Property(x => x.CreadoEn).HasColumnName("creado_en");
        });

        modelBuilder.Ignore<SolicitudCotizacion>();

        ApplySnakeCaseColumns(modelBuilder);
    }

    private static void IgnoreSoftDelete(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entity.ClrType))
                modelBuilder.Entity(entity.ClrType).Ignore(nameof(BaseEntity.Eliminado));
        }
    }

    private static void ApplySnakeCaseColumns(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                if (property.IsPrimaryKey()) continue;
                var name = property.GetColumnName();
                if (name != null && !name.Contains('_'))
                    property.SetColumnName(ToSnakeCase(name));
            }
        }
    }

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var sb = new System.Text.StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0) sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
                sb.Append(c);
        }
        return sb.ToString();
    }
}
