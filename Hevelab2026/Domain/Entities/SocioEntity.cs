namespace Hevelab2026.Domain.Entities;

/// <summary>Tabla socio — clientes, proveedores y contactos (documentacion.html mod-socio).</summary>
public class Socio : EmpresaEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string TipoPersona { get; set; } = "juridica";
    public string? Nombres { get; set; }
    public string? Apellidos { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string TipoDocumento { get; set; } = "RUC";
    public string NumeroDocumento { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? PuestoTrabajo { get; set; }
    public string? GrupoClientes { get; set; }
    public string? GrupoProveedor { get; set; }
    public decimal LimiteCredito { get; set; }
    public bool EsCliente { get; set; }
    public bool EsProveedor { get; set; }
    public bool EsContacto { get; set; }
    public int? VendedorId { get; set; }
    public int? CompradorId { get; set; }
}
