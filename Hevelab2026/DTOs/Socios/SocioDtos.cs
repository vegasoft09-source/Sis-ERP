namespace Hevelab2026.DTOs.Socios;

public class SocioCreateDto
{
    public string RazonSocial { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = "RUC";
    public string NumeroDocumento { get; set; } = string.Empty;
    public string? TipoCliente { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public decimal LimiteCredito { get; set; }
    public string? PuestoTrabajo { get; set; }
    public string? GrupoClientes { get; set; }
    public bool EsCliente { get; set; } = true;
    public bool EsProveedor { get; set; }
}

public class SocioUpdateDto : SocioCreateDto
{
    public bool Activo { get; set; } = true;
}

public class SocioResponseDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string TipoCliente { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public decimal LimiteCredito { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string? PuestoTrabajo { get; set; }
    public string? GrupoClientes { get; set; }
}
