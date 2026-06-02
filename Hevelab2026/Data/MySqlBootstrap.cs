using Hevelab2026.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hevelab2026.Data;

/// <summary>
/// Verifica MySQL y crea usuario admin si no existe (solo Hostinger/desarrollo).
/// </summary>
public static class MySqlBootstrap
{
    public static async Task EnsureAdminAndConnectionAsync(
        ApplicationDbContext db,
        ILogger logger,
        CancellationToken ct = default)
    {
        if (!db.IsMySql)
            return;

        try
        {
            var canConnect = await db.Database.CanConnectAsync(ct);
            if (!canConnect)
            {
                logger.LogError("No se pudo conectar a la base de datos MySQL.");
                return;
            }

            logger.LogInformation("MySQL: conexión verificada correctamente.");

            if (await db.Usuarios.AnyAsync(u => u.NombreUsuario == "admin", ct))
            {
                logger.LogInformation("MySQL: usuario 'admin' ya existe.");
                return;
            }

            var empresa = await db.Empresas.FirstOrDefaultAsync(ct);
            if (empresa is null)
            {
                var moneda = await db.Monedas.FirstOrDefaultAsync(ct);
                if (moneda is null)
                {
                    moneda = new Moneda { Nombre = "Sol Peruano", Codigo = "PEN", Simbolo = "S/" };
                    db.Monedas.Add(moneda);
                    await db.SaveChangesAsync(ct);
                }

                empresa = new Empresa
                {
                    RazonSocial = "HeveLab S.A.C.",
                    NombreComercial = "HeveLab",
                    Ruc = "20601234567",
                    MonedaId = moneda.Id,
                    Activo = true
                };
                db.Empresas.Add(empresa);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("MySQL: empresa inicial creada.");
            }

            var rol = await db.Roles.FirstOrDefaultAsync(r => r.Nombre == "Administrador", ct);
            if (rol is null)
            {
                rol = new Rol { Nombre = "Administrador", Descripcion = "Acceso total" };
                db.Roles.Add(rol);
                await db.SaveChangesAsync(ct);
            }

            db.Usuarios.Add(new Usuario
            {
                EmpresaId = empresa.Id,
                RolId = rol.Id,
                Nombre = "Usuario",
                Apellido = "Administrador",
                NombreUsuario = "admin",
                Correo = "admin@hevelab.com",
                ContrasenaHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Activo = true
            });
            await db.SaveChangesAsync(ct);

            logger.LogWarning(
                "MySQL: se creó usuario admin / Admin123! (cámbielo en producción).");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al verificar o inicializar MySQL.");
        }
    }
}
