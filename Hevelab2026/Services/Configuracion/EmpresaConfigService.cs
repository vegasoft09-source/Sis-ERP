using Dapper;
using Hevelab2026.Models.Configuracion;
using MySqlConnector;

namespace Hevelab2026.Services.Configuracion
{
    public class EmpresaConfigService : IEmpresaConfigService
    {
        private readonly string _connectionString;
        private readonly ILogger<EmpresaConfigService> _logger;

        public EmpresaConfigService(IConfiguration configuration, ILogger<EmpresaConfigService> logger)
        {
            _connectionString = configuration.GetConnectionString("SisErp")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SisErp'.");
            _logger = logger;
        }

        public async Task<EmpresaConfigForm?> ObtenerEmpresaAsync(int empresaId)
        {
            const string sql = """
                SELECT id, razon_social AS RazonSocial, nombre_comercial AS NombreComercial,
                       ruc AS Ruc, direccion AS Direccion, ciudad AS Ciudad,
                       telefono AS Telefono, correo AS Correo, sitio_web AS SitioWeb,
                       (logo IS NOT NULL AND LENGTH(logo) > 0) AS TieneLogo
                FROM empresa
                WHERE id = @Id AND activo = 1
                LIMIT 1;
                """;

            await using var con = new MySqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<EmpresaConfigForm>(sql, new { Id = empresaId });
        }

        public async Task<RegionalConfigForm?> ObtenerRegionalAsync(int empresaId)
        {
            const string sql = """
                SELECT id AS EmpresaId, pais_id AS PaisId, departamento_id AS DepartamentoId,
                       moneda_id AS MonedaId, codigo_postal AS CodigoPostal,
                       zona_horaria AS ZonaHoraria, idioma AS Idioma
                FROM empresa
                WHERE id = @Id AND activo = 1
                LIMIT 1;
                """;

            await using var con = new MySqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<RegionalConfigForm>(sql, new { Id = empresaId });
        }

        public async Task<bool> ActualizarEmpresaAsync(EmpresaConfigForm form)
        {
            const string sql = """
                UPDATE empresa SET
                    razon_social = @RazonSocial,
                    nombre_comercial = @NombreComercial,
                    ruc = @Ruc,
                    direccion = @Direccion,
                    ciudad = @Ciudad,
                    telefono = @Telefono,
                    correo = @Correo,
                    sitio_web = @SitioWeb,
                    actualizado_en = NOW()
                WHERE id = @Id AND activo = 1;
                """;

            await using var con = new MySqlConnection(_connectionString);
            var rows = await con.ExecuteAsync(sql, form);
            return rows > 0;
        }

        public async Task<bool> ActualizarRegionalAsync(RegionalConfigForm form)
        {
            const string sql = """
                UPDATE empresa SET
                    pais_id = @PaisId,
                    departamento_id = @DepartamentoId,
                    moneda_id = @MonedaId,
                    codigo_postal = @CodigoPostal,
                    zona_horaria = @ZonaHoraria,
                    idioma = @Idioma,
                    actualizado_en = NOW()
                WHERE id = @EmpresaId AND activo = 1;
                """;

            await using var con = new MySqlConnection(_connectionString);
            var rows = await con.ExecuteAsync(sql, form);
            return rows > 0;
        }

        public async Task<bool> GuardarLogoAsync(int empresaId, byte[] contenido)
        {
            const string sql = """
                UPDATE empresa SET logo = @Logo, actualizado_en = NOW()
                WHERE id = @Id AND activo = 1;
                """;

            await using var con = new MySqlConnection(_connectionString);
            var rows = await con.ExecuteAsync(sql, new { Id = empresaId, Logo = contenido });
            return rows > 0;
        }

        public async Task<byte[]?> ObtenerLogoAsync(int empresaId)
        {
            const string sql = "SELECT logo FROM empresa WHERE id = @Id AND activo = 1 LIMIT 1;";
            await using var con = new MySqlConnection(_connectionString);
            var logo = await con.QueryFirstOrDefaultAsync<byte[]?>(sql, new { Id = empresaId });
            return logo is { Length: > 0 } ? logo : null;
        }
    }
}
