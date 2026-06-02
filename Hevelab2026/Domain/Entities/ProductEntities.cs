namespace Hevelab2026.Domain.Entities;

public class Almacen : EmpresaEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
}

public class ProductoEntity : EmpresaEntity
{
    public string CodigoProducto { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public string? GrupoArticulos { get; set; }
    public string? Fabricante { get; set; }
    public string? Catalogo { get; set; }
    public string TipoProducto { get; set; } = "almacenable";
    public string SeguimientoInventario { get; set; } = "lote";
    public decimal PrecioVentaBase { get; set; }
    public decimal Costo { get; set; }
    public string Moneda { get; set; } = "PEN";
    public bool PermiteVentas { get; set; } = true;
    public bool PermiteComprar { get; set; } = true;
    public int? AlmacenId { get; set; }
    public Almacen? Almacen { get; set; }
    public string? Descripcion { get; set; }
    public int StockMinimo { get; set; }
    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    public ICollection<DetallePedidoVenta> DetallesVenta { get; set; } = new List<DetallePedidoVenta>();
    public ICollection<DetallePedidoCompra> DetallesCompra { get; set; } = new List<DetallePedidoCompra>();
}

public class Stock : EmpresaEntity
{
    public int ProductoId { get; set; }
    public ProductoEntity? Producto { get; set; }
    public int AlmacenId { get; set; }
    public Almacen? Almacen { get; set; }
    public decimal Cantidad { get; set; }
}
