using Hevelab2026.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hevelab2026.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (db.IsMySql)
            return;

        if (await db.Empresas.AnyAsync())
            return;

        var moneda = new Moneda { Nombre = "Sol Peruano", Codigo = "PEN", Simbolo = "S/" };
        db.Monedas.Add(moneda);
        await db.SaveChangesAsync();

        var empresa = new Empresa
        {
            RazonSocial = "HeveLab S.A.C.",
            NombreComercial = "HeveLab",
            Ruc = "20601234567",
            Correo = "admin@hevelab.com",
            MonedaId = moneda.Id,
            Activo = true
        };
        db.Empresas.Add(empresa);
        await db.SaveChangesAsync();

        var rolAdmin = new Rol { Nombre = "Administrador", Descripcion = "Acceso total" };
        var rolVentas = new Rol { Nombre = "Ventas", Descripcion = "Módulo ventas" };
        db.Roles.AddRange(rolAdmin, rolVentas);
        await db.SaveChangesAsync();

        var permisos = new[]
        {
            new Permiso { Codigo = "clientes.read", Nombre = "Ver clientes", Modulo = "ventas" },
            new Permiso { Codigo = "clientes.write", Nombre = "Editar clientes", Modulo = "ventas" },
            new Permiso { Codigo = "productos.read", Nombre = "Ver productos", Modulo = "inventario" },
            new Permiso { Codigo = "compras.read", Nombre = "Ver compras", Modulo = "compras" },
        };
        db.Permisos.AddRange(permisos);
        await db.SaveChangesAsync();

        foreach (var p in permisos)
            db.RolPermisos.Add(new RolPermiso { RolId = rolAdmin.Id, PermisoId = p.Id });

        var admin = new Usuario
        {
            EmpresaId = empresa.Id,
            RolId = rolAdmin.Id,
            Nombre = "Usuario",
            Apellido = "Administrador",
            NombreUsuario = "admin",
            Correo = "admin@hevelab.com",
            ContrasenaHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Activo = true
        };
        db.Usuarios.Add(admin);
        await db.SaveChangesAsync();

        var almacen = new Almacen { EmpresaId = empresa.Id, Codigo = "WH-01", Nombre = "Almacén Central" };
        var almacenNorte = new Almacen { EmpresaId = empresa.Id, Codigo = "WH-02", Nombre = "Sede Norte" };
        db.Almacenes.AddRange(almacen, almacenNorte);
        await db.SaveChangesAsync();

        var estadosVenta = new[]
        {
            new EstadoPedidoVenta { Nombre = "Pendiente", Codigo = "PENDIENTE", Secuencia = 1 },
            new EstadoPedidoVenta { Nombre = "Aprobado", Codigo = "APROBADO", Secuencia = 2 },
            new EstadoPedidoVenta { Nombre = "Rechazado", Codigo = "RECHAZADO", Secuencia = 3 },
            new EstadoPedidoVenta { Nombre = "Convertido", Codigo = "CONVERTIDO", Secuencia = 4 },
        };
        db.EstadosPedidoVenta.AddRange(estadosVenta);
        await db.SaveChangesAsync();

        var estadosCompra = new[]
        {
            new EstadoPedidoCompra { Nombre = "Borrador", Codigo = "BORRADOR", Secuencia = 1 },
            new EstadoPedidoCompra { Nombre = "Confirmado", Codigo = "CONFIRMADO", Secuencia = 2 },
            new EstadoPedidoCompra { Nombre = "Recibido", Codigo = "RECIBIDO", Secuencia = 3 },
        };
        db.EstadosPedidoCompra.AddRange(estadosCompra);

        var estadosTransf = new[]
        {
            new EstadoTransferencia { Nombre = "Borrador", Codigo = "BORRADOR" },
            new EstadoTransferencia { Nombre = "Enviado", Codigo = "ENVIADO" },
            new EstadoTransferencia { Nombre = "Aceptado", Codigo = "ACEPTADO" },
            new EstadoTransferencia { Nombre = "Rechazado", Codigo = "RECHAZADO" },
        };
        db.EstadosTransferencia.AddRange(estadosTransf);
        await db.SaveChangesAsync();

        SeedSocios(db, empresa.Id);
        SeedProductos(db, empresa.Id, almacen.Id);
        SeedCotizaciones(db, empresa.Id, estadosVenta);
        SeedCompras(db, empresa.Id, estadosCompra);
        SeedRecepciones(db, empresa.Id, almacen.Id, almacenNorte.Id, estadosTransf);
        await db.SaveChangesAsync();
    }

    private static void SeedSocios(ApplicationDbContext db, int empresaId)
    {
        var clientes = new[]
        {
            ("CLI-001", "Corporación Aceros SAC", "RUC", "20512345678", 45000m),
            ("CLI-002", "Juan Pérez Soluciones", "DNI", "45678912", 15000m),
            ("CLI-003", "Constructora del Sur E.I.R.L.", "RUC", "20601234567", 50000m),
        };
        foreach (var (cod, rs, td, nd, lim) in clientes)
        {
            db.Socios.Add(new Socio
            {
                EmpresaId = empresaId,
                Codigo = cod,
                RazonSocial = rs,
                TipoDocumento = td,
                NumeroDocumento = nd,
                EsCliente = true,
                LimiteCredito = lim,
                Correo = $"contacto@{cod.ToLower().Replace("-", "")}.com",
                Ciudad = "Lima",
                Activo = true
            });
        }

        db.Socios.Add(new Socio
        {
            EmpresaId = empresaId,
            Codigo = "PROV-001",
            RazonSocial = "Tech Solutions Inc.",
            TipoDocumento = "RUC",
            NumeroDocumento = "20123456789",
            EsProveedor = true,
            Activo = true
        });
    }

    private static void SeedProductos(ApplicationDbContext db, int empresaId, int almacenId)
    {
        var items = new[]
        {
            ("PR-001", "Alcohol en Gel 1L 70%", 15m, 250, 20),
            ("PR-002", "Mascarillas KN95 Pack x 20", 45m, 1000, 100),
            ("PR-003", "Termómetro Digital Infrarrojo", 125m, 15, 5),
        };
        foreach (var (sku, nombre, precio, stock, min) in items)
        {
            var p = new ProductoEntity
            {
                EmpresaId = empresaId,
                CodigoProducto = sku,
                Nombre = nombre,
                PrecioVentaBase = precio,
                Costo = precio * 0.7m,
                StockMinimo = min,
                AlmacenId = almacenId,
                GrupoArticulos = "Salud",
                Activo = true
            };
            db.Productos.Add(p);
            db.SaveChanges();
            db.Stocks.Add(new Stock { EmpresaId = empresaId, ProductoId = p.Id, AlmacenId = almacenId, Cantidad = stock });
        }
    }

    private static void SeedCotizaciones(ApplicationDbContext db, int empresaId, EstadoPedidoVenta[] estados)
    {
        var clientes = db.Socios.Where(s => s.EsCliente).Take(3).ToList();
        var pendiente = estados.First(e => e.Codigo == "PENDIENTE");
        var aprobado = estados.First(e => e.Codigo == "APROBADO");
        int n = 1;
        foreach (var c in clientes)
        {
            db.PedidosVenta.Add(new PedidoVenta
            {
                EmpresaId = empresaId,
                NumeroDocumento = $"COT-{n:D3}",
                TipoDocumento = "cotizacion",
                ClienteId = c.Id,
                EstadoPedidoVentaId = n % 2 == 0 ? aprobado.Id : pendiente.Id,
                Subtotal = 1000 * n,
                TotalImpuestos = 180m * n,
                Total = 1180m * n,
                CondicionPago = "Contado",
                Observaciones = "Cotización demo"
            });
            n++;
        }
    }

    private static void SeedCompras(ApplicationDbContext db, int empresaId, EstadoPedidoCompra[] estados)
    {
        var prov = db.Socios.First(s => s.EsProveedor);
        db.SolicitudesCotizacion.Add(new SolicitudCotizacion
        {
            EmpresaId = empresaId,
            NumeroReferencia = "RFQ-2024-001",
            ProveedorId = prov.Id,
            Estado = "NUEVO",
            Comprador = "Juan Delgado",
            TotalEstimado = 12450m
        });

        var borrador = estados.First(e => e.Codigo == "BORRADOR");
        db.PedidosCompra.Add(new PedidoCompra
        {
            EmpresaId = empresaId,
            NumeroDocumento = "OC-2023-089",
            ProveedorId = prov.Id,
            EstadoPedidoCompraId = borrador.Id,
            Subtotal = 5000,
            Total = 5900
        });
    }

    private static void SeedRecepciones(ApplicationDbContext db, int empresaId, int almOrigen, int almDest, EstadoTransferencia[] estados)
    {
        var borrador = estados.First(e => e.Codigo == "BORRADOR");
        var enviado = estados.First(e => e.Codigo == "ENVIADO");
        var prov = db.Socios.First(s => s.EsProveedor);

        db.Transferencias.AddRange(
            new Transferencia
            {
                EmpresaId = empresaId,
                NumeroReferencia = "WH/IN/00124",
                TipoOperacion = "recepcion",
                AlmacenOrigenId = almOrigen,
                AlmacenDestinoId = almDest,
                ContactoId = prov.Id,
                ContactoNombre = "Juan Pérez",
                DocumentoOrigen = "OC-2023-089",
                FechaProgramada = new DateTime(2023, 10, 24, 10, 30, 0),
                EstadoId = borrador.Id
            },
            new Transferencia
            {
                EmpresaId = empresaId,
                NumeroReferencia = "WH/IN/00125",
                TipoOperacion = "recepcion",
                ContactoNombre = "María García",
                DocumentoOrigen = "OC-2023-092",
                FechaProgramada = new DateTime(2023, 10, 23, 15, 45, 0),
                EstadoId = enviado.Id
            });
    }
}
