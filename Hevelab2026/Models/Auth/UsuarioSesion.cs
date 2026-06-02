namespace Hevelab2026.Models.Auth;

public class UsuarioSesion
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string EmpresaNombre { get; set; } = string.Empty;
    public int RolId { get; set; }
    public string RolNombre { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
    public string NombreUsuario { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Foto { get; set; }
    public string Idioma { get; set; } = "es-PE";
    public string ZonaHoraria { get; set; } = "America/Lima";
    public List<string> Permisos { get; set; } = new();
}
