using Hevelab2026.Data;
using Hevelab2026.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace Hevelab2026.Services.Auth;

public class WebAuthService : IWebAuthService
{
    private readonly ApplicationDbContext _db;

    public WebAuthService(ApplicationDbContext db) => _db = db;

    public async Task<UsuarioSesion?> ValidarCredencialesAsync(string nombreUsuario, string contrasena, CancellationToken ct = default)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Empresa)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Activo, ct);

        if (usuario == null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(contrasena, usuario.ContrasenaHash))
            return null;

        var permisos = await _db.RolPermisos
            .Where(rp => rp.RolId == usuario.RolId)
            .Include(rp => rp.Permiso)
            .Select(rp => rp.Permiso!.Codigo)
            .ToListAsync(ct);

        return new UsuarioSesion
        {
            Id = usuario.Id,
            EmpresaId = usuario.EmpresaId,
            EmpresaNombre = usuario.Empresa?.NombreComercial ?? usuario.Empresa?.RazonSocial ?? "Empresa",
            RolId = usuario.RolId,
            RolNombre = usuario.Rol?.Nombre ?? "Usuario",
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            NombreUsuario = usuario.NombreUsuario,
            Correo = usuario.Correo,
            Telefono = usuario.Telefono,
            Foto = usuario.Foto,
            Idioma = usuario.Idioma,
            ZonaHoraria = usuario.ZonaHoraria,
            Permisos = permisos
        };
    }

    public async Task RegistrarUltimoAccesoAsync(int usuarioId, CancellationToken ct = default)
    {
        var usuario = await _db.Usuarios.FindAsync([usuarioId], ct);
        if (usuario == null) return;
        usuario.UltimoAcceso = DateTime.UtcNow;
        usuario.ActualizadoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}
