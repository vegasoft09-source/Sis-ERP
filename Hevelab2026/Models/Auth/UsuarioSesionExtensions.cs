using System.Security.Claims;

namespace Hevelab2026.Models.Auth;

public static class UsuarioSesionExtensions
{
    /// <summary>Reconstruye UsuarioSesion desde el ClaimsPrincipal en memoria (sin BD).</summary>
    public static UsuarioSesion ToUsuarioSesion(this ClaimsPrincipal user)
    {
        static string? Get(ClaimsPrincipal u, string type) =>
            u.FindFirstValue(type);

        static int GetInt(ClaimsPrincipal u, string type) =>
            int.TryParse(u.FindFirstValue(type), out var v) ? v : 0;

        static int? GetIntNullable(ClaimsPrincipal u, string type) =>
            int.TryParse(u.FindFirstValue(type), out var v) ? v : null;

        return new UsuarioSesion
        {
            Id            = GetInt(user, ClaimTypes.NameIdentifier),
            NombreUsuario = Get(user, ClaimTypes.Name)  ?? "",
            Correo        = Get(user, ClaimTypes.Email) ?? "",
            RolNombre     = Get(user, ClaimTypes.Role)  ?? "",
            Nombre        = Get(user, "Nombre")         ?? "",
            Apellido      = Get(user, "Apellido")       ?? "",
            EmpresaId     = GetInt(user, "EmpresaId"),
            EmpresaNombre = Get(user, "EmpresaNombre")  ?? "",
            RolId         = GetInt(user, "RolId"),
            Telefono      = Get(user, "Telefono"),
            Foto          = Get(user, "Foto"),
            Idioma        = Get(user, "Idioma")         ?? "es-PE",
            ZonaHoraria   = Get(user, "ZonaHoraria")    ?? "America/Lima",
            Permisos      = user.FindAll("Permiso").Select(c => c.Value).ToList()
        };
    }

    /// <summary>Verifica si el usuario tiene un permiso específico.</summary>
    public static bool TienePermiso(this ClaimsPrincipal user, string permiso) =>
        user.HasClaim("Permiso", permiso);
}