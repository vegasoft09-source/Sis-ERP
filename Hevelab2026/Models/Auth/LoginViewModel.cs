using System.ComponentModel.DataAnnotations;

namespace Hevelab2026.Models.Auth
{
    /// <summary>
    /// ViewModel del formulario de inicio de sesión.
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [Display(Name = "Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }

        /// <summary>URL a la que redirigir después del login exitoso.</summary>
        public string? ReturnUrl { get; set; }
    }
}
