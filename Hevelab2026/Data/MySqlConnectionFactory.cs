using MySqlConnector;

namespace Hevelab2026.Data;

public static class MySqlConnectionFactory
{
    public static string BuildConnectionString(IConfiguration configuration)
    {
        var section = configuration.GetSection("MySql");
        if (section.Exists() && !string.IsNullOrWhiteSpace(section["Host"]))
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = section["Host"] ?? "localhost",
                Port = uint.TryParse(section["Port"], out var port) ? port : 3306,
                Database = section["Database"] ?? "",
                UserID = section["User"] ?? "",
                Password = section["Password"] ?? "",
                CharacterSet = "utf8mb4",
                AllowPublicKeyRetrieval = true
            };

            var ssl = section["SslMode"] ?? "Preferred";
            builder.SslMode = ssl.Equals("None", StringComparison.OrdinalIgnoreCase)
                ? MySqlSslMode.None
                : ssl.Equals("Required", StringComparison.OrdinalIgnoreCase)
                    ? MySqlSslMode.Required
                    : MySqlSslMode.Preferred;

            return builder.ConnectionString;
        }

        throw new InvalidOperationException("Configura la sección MySql (Host, Database, User, Password).");
    }

    public static async Task ValidateConnectionAsync(string connectionString, CancellationToken ct = default)
    {
        try
        {
            await using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync(ct);
        }
        catch (Exception ex)
        {
            var builder = new MySqlConnectionStringBuilder(connectionString);
            throw new InvalidOperationException(
                $"No se pudo conectar a MySQL en {builder.Server}:{builder.Port} " +
                $"(base: {builder.Database}, usuario: {builder.UserID}).\n\n" +
                "Causas frecuentes:\n" +
                "1) MySQL no está instalado o no está iniciado en tu PC (puerto 3306 cerrado).\n" +
                "2) Credenciales u340197236_* son de Hostinger: debes usar el HOST REMOTO del panel " +
                "(no 'localhost' desde tu computadora).\n" +
                "3) En Hostinger: hPanel → Bases de datos → MySQL → copia 'Hostname' (ej. srvXXX.hstgr.io).\n" +
                "4) Firewall o IP no autorizada en el hosting.\n\n" +
                $"Detalle técnico: {ex.Message}",
                ex);
        }
    }
}
