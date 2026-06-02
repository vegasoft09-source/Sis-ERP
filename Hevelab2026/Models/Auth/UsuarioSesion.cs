namespace Hevelab2026.Models.Auth
{
    /// <summary>
    /// Datos del usuario autenticado almacenados en los Claims de la cookie de sesión.
    /// </summary>
    public class UsuarioSesion
    {
        public int    Id                    { get; set; }
        public int    EmpresaId             { get; set; }
        public string EmpresaNombre         { get; set; } = string.Empty;
        public int    RolId                 { get; set; }
        public string RolNombre             { get; set; } = string.Empty;
        public string Nombre                { get; set; } = string.Empty;
        public string Apellido              { get; set; } = string.Empty;
        public string NombreCompleto        => $"{Nombre} {Apellido}".Trim();
        public string NombreUsuario         { get; set; } = string.Empty;
        public string Correo                { get; set; } = string.Empty;

        /// <summary>Permisos cargados desde rol_permiso JOIN permiso (campo 'codigo').</summary>
        public List<string> Permisos        { get; set; } = new();
    }
}
