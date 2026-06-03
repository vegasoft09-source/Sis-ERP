using System.ComponentModel.DataAnnotations;

namespace Hevelab2026.Models.Configuracion
{
    public class EmpresaConfigForm
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La razón social es obligatoria.")]
        [StringLength(255)]
        [Display(Name = "Razón social")]
        public string RazonSocial { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Nombre comercial")]
        public string? NombreComercial { get; set; }

        [StringLength(20)]
        [Display(Name = "RUC")]
        public string? Ruc { get; set; }

        [StringLength(255)]
        [Display(Name = "Dirección fiscal")]
        public string? Direccion { get; set; }

        [StringLength(100)]
        public string? Ciudad { get; set; }

        [StringLength(30)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [EmailAddress]
        [StringLength(150)]
        [Display(Name = "Correo corporativo")]
        public string? Correo { get; set; }

        [StringLength(255)]
        [Display(Name = "Sitio web")]
        public string? SitioWeb { get; set; }

        public bool TieneLogo { get; set; }
    }
}
