// DataAccess/WarehouseRepository.cs
using MySql.Data.MySqlClient;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.DataAccess
{
    public class WarehouseRepository
    {
        public List<Warehouse> GetAll()
        {
            var list = new List<Warehouse>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM Warehouse ORDER BY WarehouseName", conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read()) list.Add(Map(rdr));
            return list;
        }

        public Warehouse? GetById(string id)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM Warehouse WHERE WarehouseId=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var rdr = cmd.ExecuteReader();
            return rdr.Read() ? Map(rdr) : null;
        }

        public bool Insert(Warehouse w)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"INSERT INTO Warehouse (WarehouseId,WarehouseName,Location,Capacity)
                           VALUES (@id,@name,@loc,@cap)";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id",   w.WarehouseId);
            cmd.Parameters.AddWithValue("@name", w.WarehouseName);
            cmd.Parameters.AddWithValue("@loc",  w.Location ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@cap",  w.Capacity.HasValue ? w.Capacity.Value : (object)DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        private static Warehouse Map(MySqlDataReader r) => new()
        {
            WarehouseId   = r["WarehouseId"].ToString()!,
            WarehouseName = r["WarehouseName"].ToString()!,
            Location      = r["Location"] == DBNull.Value ? null : r["Location"].ToString(),
            Capacity      = r["Capacity"] == DBNull.Value ? null : Convert.ToDecimal(r["Capacity"])
        };
    }
}
