namespace Hevelab2026.Domain.Entities;

public class Moneda : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = "PEN";
    public string Simbolo { get; set; } = "S/";
}

public class Empresa : BaseEntity
{
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string Ruc { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public int? MonedaId { get; set; }
    public Moneda? Moneda { get; set; }
    public string ZonaHoraria { get; set; } = "America/Lima";
    public string Idioma { get; set; } = "es-PE";
}

public class Rol : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}

public class Permiso : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public class RolPermiso : BaseEntity
{
    public int RolId { get; set; }
    public Rol? Rol { get; set; }
    public int PermisoId { get; set; }
    public Permiso? Permiso { get; set; }
}

public class Usuario : EmpresaEntity
{
    public int RolId { get; set; }
    public Rol? Rol { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string ContrasenaHash { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpira { get; set; }
}
