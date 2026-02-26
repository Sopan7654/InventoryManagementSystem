// DataAccess/SupplierRepository.cs
using MySql.Data.MySqlClient;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.DataAccess
{
    public class SupplierRepository
    {
        public List<Supplier> GetAll()
        {
            var list = new List<Supplier>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM Supplier ORDER BY SupplierName", conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read()) list.Add(Map(rdr));
            return list;
        }

        public Supplier? GetById(string id)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM Supplier WHERE SupplierId=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var rdr = cmd.ExecuteReader();
            return rdr.Read() ? Map(rdr) : null;
        }

        public bool Insert(Supplier s)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"INSERT INTO Supplier (SupplierId,SupplierName,Email,Phone,Website)
                           VALUES (@id,@name,@email,@phone,@web)";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id",    s.SupplierId);
            cmd.Parameters.AddWithValue("@name",  s.SupplierName);
            cmd.Parameters.AddWithValue("@email", s.Email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@phone", s.Phone ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@web",   s.Website ?? (object)DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Update(Supplier s)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"UPDATE Supplier SET SupplierName=@name,Email=@email,Phone=@phone,Website=@web
                           WHERE SupplierId=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name",  s.SupplierName);
            cmd.Parameters.AddWithValue("@email", s.Email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@phone", s.Phone ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@web",   s.Website ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@id",    s.SupplierId);
            return cmd.ExecuteNonQuery() > 0;
        }

        private static Supplier Map(MySqlDataReader r) => new()
        {
            SupplierId   = r["SupplierId"].ToString()!,
            SupplierName = r["SupplierName"].ToString()!,
            Email        = r["Email"] == DBNull.Value ? null : r["Email"].ToString(),
            Phone        = r["Phone"] == DBNull.Value ? null : r["Phone"].ToString(),
            Website      = r["Website"] == DBNull.Value ? null : r["Website"].ToString()
        };
    }
}
