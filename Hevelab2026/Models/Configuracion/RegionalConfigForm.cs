using System.ComponentModel.DataAnnotations;

namespace Hevelab2026.Models.Configuracion
{
    public class RegionalConfigForm
    {
        public int EmpresaId { get; set; }

        [Display(Name = "País")]
        public int? PaisId { get; set; }

        [Display(Name = "Departamento")]
        public int? DepartamentoId { get; set; }

        [Display(Name = "Moneda")]
        public int? MonedaId { get; set; }

        [StringLength(20)]
        [Display(Name = "Código postal")]
        public string? CodigoPostal { get; set; }

        [StringLength(100)]
        [Display(Name = "Zona horaria")]
        public string? ZonaHoraria { get; set; }

        [StringLength(20)]
        [Display(Name = "Idioma")]
        public string? Idioma { get; set; }
    }

    public class CatalogoItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Extra { get; set; }
    }
}
