namespace Hevelab2026.Services.Compras;

public class SolicitudCotizacionVm
{
    public int Id { get; set; }
    public string Referencia { get; set; } = "";
    public string Proveedor { get; set; } = "";
    public string Comprador { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaLimite { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "";
}

public class OrdenCompraVm
{
    public int Id { get; set; }
    public string NumeroDocumento { get; set; } = "";
    public string Proveedor { get; set; } = "";
    public DateTime FechaEmision { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "";
}

public class ProveedorVm
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string RazonSocial { get; set; } = "";
    public string Ruc { get; set; } = "";
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; }
}

public interface ICompraService
{
    Task<IReadOnlyList<SolicitudCotizacionVm>> GetSolicitudesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<OrdenCompraVm>> GetOrdenesCompraAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProveedorVm>> GetProveedoresAsync(string? search, CancellationToken ct = default);
    Task<int> CrearProveedorAsync(int empresaId, string razonSocial, string tipoDoc, string numeroDoc,
        string? telefono, string? email, string? direccion, CancellationToken ct = default);
    Task<int> CrearSolicitudAsync(int empresaId, int proveedorId, string? referencia, DateTime? fechaLimite,
        decimal total, string? observaciones, int? compradorId, CancellationToken ct = default);
}
