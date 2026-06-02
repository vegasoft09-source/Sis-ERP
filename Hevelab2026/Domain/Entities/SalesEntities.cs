namespace Hevelab2026.Domain.Entities;

public class EstadoPedidoVenta : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public int Secuencia { get; set; }
}

public class PedidoVenta : EmpresaEntity
{
    public string NumeroDocumento { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = "cotizacion";
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    public int ClienteId { get; set; }
    public Socio? Cliente { get; set; }
    public int? VendedorId { get; set; }
    public Usuario? Vendedor { get; set; }
    public int? MonedaId { get; set; }
    public Moneda? Moneda { get; set; }
    public int EstadoPedidoVentaId { get; set; }
    public EstadoPedidoVenta? EstadoPedido { get; set; }
    public string? CondicionPago { get; set; }
    public string? MetodoPago { get; set; }
    public string? DireccionCliente { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DescuentoTotal { get; set; }
    public decimal TotalImpuestos { get; set; }
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public int? CreadoPorId { get; set; }
    public ICollection<DetallePedidoVenta> Detalles { get; set; } = new List<DetallePedidoVenta>();
}

public class DetallePedidoVenta : BaseEntity
{
    public int PedidoVentaId { get; set; }
    public PedidoVenta? PedidoVenta { get; set; }
    public int ProductoId { get; set; }
    public ProductoEntity? Producto { get; set; }
    public string? Descripcion { get; set; }
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = "UND";
    public decimal PrecioUnitario { get; set; }
    public decimal PorcentajeDescuento { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
}
