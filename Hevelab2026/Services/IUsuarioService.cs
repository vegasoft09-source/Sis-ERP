using Hevelab2026.Models.Auth;

namespace Hevelab2026.Services
{
    /// <summary>
    /// Contrato del servicio de autenticación de usuarios.
    /// Trabaja directamente con las tablas: usuario, empresa, rol, permiso, rol_permiso.
    /// </summary>
    public interface IUsuarioService
    {
        /// <summary>
        /// Autentica un usuario validando nombre_usuario + contraseña (BCrypt).
        /// Devuelve la sesión con permisos cargados, o null si falla.
        /// </summary>
        Task<UsuarioSesion?> AutenticarAsync(string nombreUsuario, string contrasena);

        /// <summary>
        /// Registra el último acceso del usuario en la tabla usuario.
        /// </summary>
        Task RegistrarUltimoAccesoAsync(int usuarioId);
    }
}
