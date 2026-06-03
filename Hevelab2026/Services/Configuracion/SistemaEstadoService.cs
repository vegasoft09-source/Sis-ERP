using System.Reflection;
using Dapper;
using Hevelab2026.Models.Configuracion;
using MySqlConnector;

namespace Hevelab2026.Services.Configuracion
{
    public class SistemaEstadoService : ISistemaEstadoService
    {
        private readonly string _connectionString;

        public SistemaEstadoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SisErp")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SisErp'.");
        }

        public async Task<SistemaEstadoViewModel> ObtenerEstadoAsync()
        {
            var vm = new SistemaEstadoViewModel
            {
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0",
                UltimoRespaldo = "Pendiente de módulo backup",
                EspacioUsado = "—"
            };

            try
            {
                await using var con = new MySqlConnection(_connectionString);
                await con.OpenAsync();
                vm.BaseDatosConectada = true;
                vm.BaseDatosDetalle = "Conectada";

                const string sqlUsuarios = "SELECT COUNT(*) FROM usuario WHERE activo = 1;";
                vm.UsuariosActivos = await con.ExecuteScalarAsync<int>(sqlUsuarios);
            }
            catch (Exception ex)
            {
                vm.BaseDatosConectada = false;
                vm.BaseDatosDetalle = ex.Message;
            }

            return vm;
        }
    }
}
