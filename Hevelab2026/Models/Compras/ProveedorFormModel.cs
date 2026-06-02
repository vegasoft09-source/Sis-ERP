using System.ComponentModel.DataAnnotations;

namespace Hevelab2026.Models.Compras;

public class ProveedorFormModel
{
    [Required]
    [Display(Name = "Razón social")]
    public string RazonSocial { get; set; } = "";

    [Required]
    [Display(Name = "Tipo documento")]
    public string TipoDocumento { get; set; } = "RUC";

    [Required]
    [Display(Name = "Número documento")]
    public string NumeroDocumento { get; set; } = "";

    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [EmailAddress]
    [Display(Name = "Correo")]
    public string? Email { get; set; }

    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }
}

public class SolicitudCompraFormModel
{
    [Required]
    [Display(Name = "Proveedor")]
    public int ProveedorId { get; set; }

    [Display(Name = "Referencia")]
    public string? Referencia { get; set; }

    [Display(Name = "Fecha límite")]
    [DataType(DataType.Date)]
    public DateTime? FechaLimite { get; set; }

    [Display(Name = "Total estimado")]
    public decimal TotalEstimado { get; set; }

    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }
}
