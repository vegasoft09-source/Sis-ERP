using System.ComponentModel.DataAnnotations;

namespace Hevelab2026.Models.Config;

public class PerfilViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [Display(Name = "Nombres")]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [Display(Name = "Apellidos")]
    public string Apellido { get; set; } = "";

    [Required]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = "";

    [Required]
    [EmailAddress]
    [Display(Name = "Correo")]
    public string Correo { get; set; } = "";

    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [Display(Name = "Idioma")]
    public string Idioma { get; set; } = "es-PE";

    [Display(Name = "Zona horaria")]
    public string ZonaHoraria { get; set; } = "America/Lima";

    public string? FotoActual { get; set; }

    [Display(Name = "Nueva contraseña")]
    [DataType(DataType.Password)]
    public string? NuevaContrasena { get; set; }

    [Display(Name = "Confirmar contraseña")]
    [DataType(DataType.Password)]
    [Compare(nameof(NuevaContrasena), ErrorMessage = "Las contraseñas no coinciden")]
    public string? ConfirmarContrasena { get; set; }

    public string RolNombre { get; set; } = "";
    public string EmpresaNombre { get; set; } = "";
}

public class EmpresaConfigViewModel
{
    public int Id { get; set; }
    public string RazonSocial { get; set; } = "";
    public string? NombreComercial { get; set; }
    public string Ruc { get; set; } = "";
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
}

public class ConfiguracionPageViewModel
{
    public string Tab { get; set; } = "profile";
    public PerfilViewModel Perfil { get; set; } = new();
    public EmpresaConfigViewModel Empresa { get; set; } = new();
    public IReadOnlyList<UsuarioListaVm> Usuarios { get; set; } = Array.Empty<UsuarioListaVm>();
}

public class UsuarioListaVm
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = "";
    public string NombreUsuario { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Rol { get; set; } = "";
    public bool Activo { get; set; }
    public string? Foto { get; set; }
}
