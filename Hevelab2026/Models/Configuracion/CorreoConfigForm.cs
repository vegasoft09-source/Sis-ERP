using System.ComponentModel.DataAnnotations;

namespace Hevelab2026.Models.Configuracion
{
    /// <summary>SMTP / notificaciones — preparado para tabla futura.</summary>
    public class CorreoConfigForm
    {
        [Display(Name = "Servidor SMTP")]
        public string? Servidor { get; set; }

        [Display(Name = "Puerto")]
        public int Puerto { get; set; } = 587;

        [Display(Name = "Usuario")]
        public string? Usuario { get; set; }

        [Display(Name = "Correo remitente")]
        [EmailAddress]
        public string? Remitente { get; set; }

        public bool UsarSsl { get; set; } = true;
        public bool ModuloEnDesarrollo { get; set; } = true;
    }
}
