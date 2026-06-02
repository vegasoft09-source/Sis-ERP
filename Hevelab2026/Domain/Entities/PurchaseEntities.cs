namespace Hevelab2026.Domain.Entities;

public class EstadoPedidoCompra : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public int Secuencia { get; set; }
}

/// <summary>Solicitud de cotización / RFQ (compras).</summary>
public class SolicitudCotizacion : EmpresaEntity
{
    public string NumeroReferencia { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaLimite { get; set; }
    public int? ProveedorId { get; set; }
    public Socio? Proveedor { get; set; }
    public string Estado { get; set; } = "NUEVO";
    public string? Comprador { get; set; }
    public decimal TotalEstimado { get; set; }
    public string? Observaciones { get; set; }
}

public class PedidoCompra : EmpresaEntity
{
    public string NumeroDocumento { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    public int ProveedorId { get; set; }
    public Socio? Proveedor { get; set; }
    public int? CompradorId { get; set; }
    public int EstadoPedidoCompraId { get; set; }
    public EstadoPedidoCompra? EstadoPedido { get; set; }
    public string? TerminosPago { get; set; }
    public string? DireccionProveedor { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DescuentoTotal { get; set; }
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public ICollection<DetallePedidoCompra> Detalles { get; set; } = new List<DetallePedidoCompra>();
}

public class DetallePedidoCompra : BaseEntity
{
    public int PedidoCompraId { get; set; }
    public PedidoCompra? PedidoCompra { get; set; }
    public int ProductoId { get; set; }
    public ProductoEntity? Producto { get; set; }
    public string? Descripcion { get; set; }
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = "UND";
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
}
