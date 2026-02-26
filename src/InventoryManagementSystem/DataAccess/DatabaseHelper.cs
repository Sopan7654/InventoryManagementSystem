// DataAccess/DatabaseHelper.cs
using MySql.Data.MySqlClient;
using System.Text.Json;

namespace InventoryManagementSystem.DataAccess
{
    public class DatabaseHelper
    {
        private static string _connectionString = string.Empty;

        public static void Initialize()
        {
            try
            {
                string configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (!File.Exists(configPath))
                    throw new FileNotFoundException("appsettings.json not found.");

                string json = File.ReadAllText(configPath);
                using var doc = JsonDocument.Parse(json);
                _connectionString = doc.RootElement
                    .GetProperty("ConnectionStrings")
                    .GetProperty("InventoryDB")
                    .GetString() ?? string.Empty;

                if (string.IsNullOrEmpty(_connectionString))
                    throw new InvalidOperationException("Connection string is empty.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Config Error] {ex.Message}");
                Console.ResetColor();
                throw;
            }
        }

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public static bool TestConnection()
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
