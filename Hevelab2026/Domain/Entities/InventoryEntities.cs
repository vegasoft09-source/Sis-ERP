namespace Hevelab2026.Domain.Entities;

public class EstadoTransferencia : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
}

public class Transferencia : EmpresaEntity
{
    public string NumeroReferencia { get; set; } = string.Empty;
    public string TipoOperacion { get; set; } = "recepcion";
    public string? DocumentoOrigen { get; set; }
    public int? AlmacenOrigenId { get; set; }
    public Almacen? AlmacenOrigen { get; set; }
    public int? AlmacenDestinoId { get; set; }
    public Almacen? AlmacenDestino { get; set; }
    public int? ContactoId { get; set; }
    public Socio? Contacto { get; set; }
    public string? ContactoNombre { get; set; }
    public int EstadoId { get; set; }
    public EstadoTransferencia? Estado { get; set; }
    public DateTime? FechaProgramada { get; set; }
    public DateTime? FechaRealizada { get; set; }
    public int? PedidoCompraId { get; set; }
    public string? Observaciones { get; set; }
    public ICollection<TransferenciaLinea> Lineas { get; set; } = new List<TransferenciaLinea>();
}

public class TransferenciaLinea : BaseEntity
{
    public int TransferenciaId { get; set; }
    public Transferencia? Transferencia { get; set; }
    public int ProductoId { get; set; }
    public ProductoEntity? Producto { get; set; }
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = "UND";
    public string? Descripcion { get; set; }
}

public class MovimientoStock : EmpresaEntity
{
    public int ProductoId { get; set; }
    public ProductoEntity? Producto { get; set; }
    public int AlmacenId { get; set; }
    public Almacen? Almacen { get; set; }
    public int? TransferenciaId { get; set; }
    public string TipoMovimiento { get; set; } = "entrada";
    public string Origen { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal Saldo { get; set; }
}
