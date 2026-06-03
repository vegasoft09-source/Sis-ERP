using System;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;

namespace test_db
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var connStr = "Server=localhost;Port=3306;Database=u340197236_SIS_ERP;User=root;Password=;CharSet=utf8mb4;";
            Console.WriteLine("Connecting to MySQL to clear mock data: " + connStr);
            
            try
            {
                using var conn = new MySqlConnection(connStr);
                await conn.OpenAsync();
                Console.WriteLine("Connection opened successfully!");

                // Clear documento_contable table
                await conn.ExecuteAsync("TRUNCATE TABLE documento_contable;");
                Console.WriteLine("TRUNCATE TABLE documento_contable completed successfully! All mock documents deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Global Exception: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
