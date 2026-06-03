using Dapper;
using Hevelab2026.Models.Configuracion;
using MySqlConnector;

namespace Hevelab2026.Services.Configuracion
{
    public class ConfiguracionCatalogoService : IConfiguracionCatalogoService
    {
        private readonly string _connectionString;

        public ConfiguracionCatalogoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SisErp")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SisErp'.");
        }

        public async Task<IReadOnlyList<CatalogoItem>> ObtenerPaisesAsync()
        {
            const string sql = """
                SELECT id AS Id, nombre AS Nombre, codigo_iso AS Extra
                FROM pais WHERE activo = 1 ORDER BY nombre;
                """;
            await using var con = new MySqlConnection(_connectionString);
            var list = await con.QueryAsync<CatalogoItem>(sql);
            return list.ToList();
        }

        public async Task<IReadOnlyList<CatalogoItem>> ObtenerMonedasAsync()
        {
            const string sql = """
                SELECT id AS Id, nombre AS Nombre,
                       CONCAT(codigo, ' (', simbolo, ')') AS Extra
                FROM moneda WHERE activo = 1 ORDER BY nombre;
                """;
            await using var con = new MySqlConnection(_connectionString);
            var list = await con.QueryAsync<CatalogoItem>(sql);
            return list.ToList();
        }

        public async Task<IReadOnlyList<CatalogoItem>> ObtenerDepartamentosAsync(int? paisId)
        {
            const string sql = """
                SELECT id AS Id, nombre AS Nombre, NULL AS Extra
                FROM departamento
                WHERE activo = 1 AND (@PaisId IS NULL OR pais_id = @PaisId)
                ORDER BY nombre;
                """;
            await using var con = new MySqlConnection(_connectionString);
            var list = await con.QueryAsync<CatalogoItem>(sql, new { PaisId = paisId });
            return list.ToList();
        }
    }
}
