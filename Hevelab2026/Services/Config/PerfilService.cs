using System.Security.Claims;
using Hevelab2026.Data;
using Hevelab2026.Models.Config;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace Hevelab2026.Services.Config;

public class PerfilService : IPerfilService
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public PerfilService(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<ConfiguracionPageViewModel> GetConfiguracionAsync(
        int userId, int empresaId, string tab, CancellationToken ct = default)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Empresa)
            .AsNoTracking()
            .FirstAsync(u => u.Id == userId, ct);

        var empresa = await _db.Empresas.AsNoTracking()
            .FirstAsync(e => e.Id == empresaId, ct);

        var usuarios = await _db.Usuarios
            .Include(u => u.Rol)
            .AsNoTracking()
            .Where(u => u.EmpresaId == empresaId)
            .OrderBy(u => u.Nombre)
            .Select(u => new UsuarioListaVm
            {
                Id = u.Id,
                NombreCompleto = (u.Nombre + " " + u.Apellido).Trim(),
                NombreUsuario = u.NombreUsuario,
                Correo = u.Correo,
                Rol = u.Rol != null ? u.Rol.Nombre : "",
                Activo = u.Activo,
                Foto = u.Foto
            })
            .ToListAsync(ct);

        return new ConfiguracionPageViewModel
        {
            Tab = tab,
            Perfil = new PerfilViewModel
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                NombreUsuario = usuario.NombreUsuario,
                Correo = usuario.Correo,
                Telefono = usuario.Telefono,
                Idioma = usuario.Idioma,
                ZonaHoraria = usuario.ZonaHoraria,
                FotoActual = usuario.Foto,
                RolNombre = usuario.Rol?.Nombre ?? "",
                EmpresaNombre = empresa.NombreComercial ?? empresa.RazonSocial
            },
            Empresa = new EmpresaConfigViewModel
            {
                Id = empresa.Id,
                RazonSocial = empresa.RazonSocial,
                NombreComercial = empresa.NombreComercial,
                Ruc = empresa.Ruc,
                Telefono = empresa.Telefono,
                Correo = empresa.Correo,
                Direccion = empresa.Direccion
            },
            Usuarios = usuarios
        };
    }

    public async Task<string?> GuardarPerfilAsync(
        int userId, PerfilViewModel model, IFormFile? foto, CancellationToken ct = default)
    {
        var usuario = await _db.Usuarios.FirstAsync(u => u.Id == userId, ct);

        if (await _db.Usuarios.AnyAsync(u =>
                u.Id != userId && u.NombreUsuario == model.NombreUsuario, ct))
            throw new InvalidOperationException("El nombre de usuario ya está en uso.");

        if (await _db.Usuarios.AnyAsync(u =>
                u.Id != userId && u.Correo == model.Correo, ct))
            throw new InvalidOperationException("El correo ya está registrado.");

        usuario.Nombre = model.Nombre.Trim();
        usuario.Apellido = model.Apellido.Trim();
        usuario.NombreUsuario = model.NombreUsuario.Trim();
        usuario.Correo = model.Correo.Trim();
        usuario.Telefono = model.Telefono?.Trim();
        usuario.Idioma = model.Idioma;
        usuario.ZonaHoraria = model.ZonaHoraria;
        usuario.ActualizadoEn = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(model.NuevaContrasena))
            usuario.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(model.NuevaContrasena);

        if (foto is { Length: > 0 })
        {
            var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();
            if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp" or ".gif"))
                throw new InvalidOperationException("Formato de imagen no permitido.");

            var dir = Path.Combine(_env.WebRootPath, "uploads", "perfiles");
            Directory.CreateDirectory(dir);
            var fileName = $"user_{userId}{ext}";
            var path = Path.Combine(dir, fileName);
            await using (var stream = File.Create(path))
                await foto.CopyToAsync(stream, ct);
            usuario.Foto = $"/uploads/perfiles/{fileName}";
        }

        await _db.SaveChangesAsync(ct);
        return usuario.Foto;
    }

    public async Task GuardarEmpresaAsync(int empresaId, EmpresaConfigViewModel model, CancellationToken ct = default)
    {
        var empresa = await _db.Empresas.FirstAsync(e => e.Id == empresaId, ct);
        empresa.RazonSocial = model.RazonSocial.Trim();
        empresa.NombreComercial = model.NombreComercial?.Trim();
        empresa.Ruc = model.Ruc.Trim();
        empresa.Telefono = model.Telefono?.Trim();
        empresa.Correo = model.Correo?.Trim();
        empresa.Direccion = model.Direccion?.Trim();
        empresa.ActualizadoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RefrescarClaimsAsync(HttpContext httpContext, int userId, CancellationToken ct = default)
    {
        var u = await _db.Usuarios.Include(x => x.Rol).Include(x => x.Empresa)
            .AsNoTracking().FirstAsync(x => x.Id == userId, ct);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new(ClaimTypes.Name, u.NombreUsuario),
            new("NombreCompleto", $"{u.Nombre} {u.Apellido}".Trim()),
            new("EmpresaId", u.EmpresaId.ToString()),
            new("EmpresaNombre", u.Empresa?.NombreComercial ?? u.Empresa?.RazonSocial ?? ""),
            new(ClaimTypes.Role, u.Rol?.Nombre ?? "Usuario"),
            new("RolId", u.RolId.ToString()),
            new(ClaimTypes.Email, u.Correo),
            new("Telefono", u.Telefono ?? ""),
            new("Foto", u.Foto ?? ""),
            new("Idioma", u.Idioma),
            new("ZonaHoraria", u.ZonaHoraria)
        };

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
    }
}
