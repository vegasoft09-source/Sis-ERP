using Hevelab2026.Common.Exceptions;
using Hevelab2026.Data;
using Hevelab2026.Domain.Entities;
using Hevelab2026.Models;
using Hevelab2026.Repositories;
using Hevelab2026.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace Hevelab2026.Services.Productos;

public class ProductoService : IProductoService
{
    private readonly ApplicationDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ProductoService(ApplicationDbContext db, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _db = db;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<Producto>> GetAllAsync(string? search, CancellationToken ct = default)
    {
        var q = _db.Productos
            .Include(p => p.Stocks)
            .Include(p => p.Almacen)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            q = q.Where(p => p.Nombre.ToLower().Contains(s) || p.CodigoProducto.ToLower().Contains(s));
        }

        var list = await q.ToListAsync(ct);
        return list.Select(e => Map(e)).ToList();
    }

    public async Task<Producto?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        var e = await _db.Productos.Include(p => p.Stocks).FirstOrDefaultAsync(p => p.CodigoProducto == sku, ct);
        return e == null ? null : Map(e);
    }

    public async Task<Producto> CreateAsync(Producto model, CancellationToken ct = default)
    {
        if (await _db.Productos.AnyAsync(p => p.CodigoProducto == model.Sku, ct))
            throw new ConflictException($"Ya existe el producto {model.Sku}");

        var empresaId = _currentUser.EmpresaId;
        if (!await _db.Empresas.AnyAsync(e => e.Id == empresaId, ct))
            empresaId = await _db.Empresas.Select(e => e.Id).FirstAsync(ct);
        var almacenId = await _db.Almacenes.Where(a => a.EmpresaId == empresaId).Select(a => a.Id).FirstAsync(ct);

        var entity = new ProductoEntity
        {
            CodigoProducto = model.Sku,
            CodigoBarras = model.CodigoBarras,
            Nombre = model.Nombre,
            PrecioVentaBase = model.Precio,
            GrupoArticulos = model.Categoria,
            Fabricante = model.Marca,
            Descripcion = model.Descripcion,
            StockMinimo = model.StockMinimo,
            AlmacenId = almacenId,
            Activo = model.Disponible
        };
        await _uow.Productos.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        await _db.Stocks.AddAsync(new Stock
        {
            EmpresaId = empresaId,
            ProductoId = entity.Id,
            AlmacenId = almacenId,
            Cantidad = model.Stock
        }, ct);
        await _uow.SaveChangesAsync(ct);

        return Map(entity, model.Stock);
    }

    public async Task<Producto> UpdateAsync(string sku, Producto model, CancellationToken ct = default)
    {
        var entity = await _db.Productos.Include(p => p.Stocks)
            .FirstOrDefaultAsync(p => p.CodigoProducto == sku, ct)
            ?? throw new NotFoundException("Producto no encontrado");

        entity.Nombre = model.Nombre;
        entity.CodigoBarras = model.CodigoBarras;
        entity.PrecioVentaBase = model.Precio;
        entity.GrupoArticulos = model.Categoria;
        entity.Fabricante = model.Marca;
        entity.Descripcion = model.Descripcion;
        entity.StockMinimo = model.StockMinimo;
        entity.Activo = model.Disponible;

        var stock = entity.Stocks.FirstOrDefault();
        if (stock != null) stock.Cantidad = model.Stock;

        await _uow.SaveChangesAsync(ct);
        return Map(entity, model.Stock);
    }

    private static Producto Map(ProductoEntity e, decimal? stockOverride = null)
    {
        var stock = stockOverride ?? e.Stocks.Sum(s => s.Cantidad);
        return new Producto
        {
            Sku = e.CodigoProducto,
            CodigoBarras = e.CodigoBarras ?? "",
            Nombre = e.Nombre,
            Precio = e.PrecioVentaBase,
            Stock = (int)stock,
            Unidad = "UND",
            Categoria = e.GrupoArticulos ?? "",
            Marca = e.Fabricante ?? "",
            UbicacionAlmacen = e.Almacen?.Nombre ?? "",
            StockMinimo = e.StockMinimo,
            Disponible = e.Activo,
            Descripcion = e.Descripcion ?? ""
        };
    }
}
