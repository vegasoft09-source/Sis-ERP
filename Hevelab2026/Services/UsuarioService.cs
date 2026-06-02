using Dapper;
using Hevelab2026.Models.Auth;
using MySqlConnector;

namespace Hevelab2026.Services
{
    /// <summary>
    /// Implementación del servicio de autenticación.
    /// Consulta MySQL directamente con Dapper, usando las tablas del Sis-ERP.
    /// </summary>
    public class UsuarioService : IUsuarioService
    {
        private readonly string _connectionString;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(IConfiguration configuration, ILogger<UsuarioService> logger)
        {
            _connectionString = configuration.GetConnectionString("SisErp")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SisErp'.");
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────
        // AUTENTICAR
        // ─────────────────────────────────────────────────────────────────────
        public async Task<UsuarioSesion?> AutenticarAsync(string nombreUsuario, string contrasena)
        {
            try
            {
                await using var con = new MySqlConnection(_connectionString);

                // Consulta principal: usuario + empresa + rol
                const string sqlUsuario = """
                    SELECT
                        u.id,
                        u.empresa_id,
                        e.nombre_comercial  AS empresa_nombre,
                        u.rol_id,
                        r.nombre            AS rol_nombre,
                        u.nombre,
                        u.apellido,
                        u.nombre_usuario,
                        u.correo,
                        u.contrasena
                    FROM usuario u
                    INNER JOIN empresa e ON e.id = u.empresa_id AND e.activo = 1
                    INNER JOIN rol     r ON r.id = u.rol_id     AND r.activo = 1
                    WHERE u.nombre_usuario = @NombreUsuario
                      AND u.activo = 1
                    LIMIT 1;
                    """;

                var row = await con.QueryFirstOrDefaultAsync(sqlUsuario, new { NombreUsuario = nombreUsuario });

                if (row is null)
                {
                    _logger.LogWarning("Login fallido: usuario '{Usuario}' no encontrado o inactivo.", nombreUsuario);
                    return null;
                }

                // Verificar contraseña con BCrypt
                string hashGuardado = (string)row.contrasena;
                bool passwordOk = BCrypt.Net.BCrypt.Verify(contrasena, hashGuardado);

                if (!passwordOk)
                {
                    _logger.LogWarning("Login fallido: contraseña incorrecta para '{Usuario}'.", nombreUsuario);
                    return null;
                }

                // Cargar permisos del rol (tabla rol_permiso JOIN permiso)
                var permisos = await ObtenerPermisosPorRolAsync(con, (int)row.rol_id);

                var sesion = new UsuarioSesion
                {
                    Id            = (int)row.id,
                    EmpresaId     = (int)row.empresa_id,
                    EmpresaNombre = (string)row.empresa_nombre,
                    RolId         = (int)row.rol_id,
                    RolNombre     = (string)row.rol_nombre,
                    Nombre        = (string)(row.nombre ?? string.Empty),
                    Apellido      = row.apellido is DBNull || row.apellido is null ? string.Empty : (string)row.apellido,
                    NombreUsuario = (string)row.nombre_usuario,
                    Correo        = (string)(row.correo ?? string.Empty),
                    Permisos      = permisos
                };

                _logger.LogInformation("Login exitoso para '{Usuario}' (Rol: {Rol}, Empresa: {Empresa}).",
                    nombreUsuario, sesion.RolNombre, sesion.EmpresaNombre);

                return sesion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al autenticar el usuario '{Usuario}'.", nombreUsuario);
                return null;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // REGISTRAR ÚLTIMO ACCESO
        // ─────────────────────────────────────────────────────────────────────
        public async Task RegistrarUltimoAccesoAsync(int usuarioId)
        {
            try
            {
                await using var con = new MySqlConnection(_connectionString);
                const string sql = """
                    UPDATE usuario
                       SET ultimo_acceso   = NOW(),
                           actualizado_en  = NOW()
                     WHERE id = @Id;
                    """;
                await con.ExecuteAsync(sql, new { Id = usuarioId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar último acceso para usuarioId={Id}.", usuarioId);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // HELPER PRIVADO
        // ─────────────────────────────────────────────────────────────────────
        private static async Task<List<string>> ObtenerPermisosPorRolAsync(MySqlConnection con, int rolId)
        {
            const string sql = """
                SELECT p.codigo
                FROM rol_permiso rp
                INNER JOIN permiso p ON p.id = rp.permiso_id AND p.activo = 1
                WHERE rp.rol_id = @RolId;
                """;

            var codigos = await con.QueryAsync<string>(sql, new { RolId = rolId });
            return codigos.ToList();
        }
    }
}
