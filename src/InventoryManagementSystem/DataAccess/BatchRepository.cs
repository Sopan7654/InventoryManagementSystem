// DataAccess/BatchRepository.cs
using MySql.Data.MySqlClient;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.DataAccess
{
    public class BatchRepository
    {
        public List<Batch> GetAll()
        {
            var list = new List<Batch>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT b.BatchId, b.ProductId, p.ProductName,
                                  b.WarehouseId, w.WarehouseName,
                                  b.BatchNumber, b.ManufacturingDate, b.ExpiryDate, b.Quantity
                           FROM Batch b
                           JOIN Product   p ON p.ProductId   = b.ProductId
                           JOIN Warehouse w ON w.WarehouseId = b.WarehouseId
                           ORDER BY b.ExpiryDate, p.ProductName";
            using var cmd = new MySqlCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read()) list.Add(Map(rdr));
            return list;
        }

        public List<Batch> GetExpiringSoon(int days = 30)
        {
            var list = new List<Batch>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT b.BatchId, b.ProductId, p.ProductName,
                                  b.WarehouseId, w.WarehouseName,
                                  b.BatchNumber, b.ManufacturingDate, b.ExpiryDate, b.Quantity
                           FROM Batch b
                           JOIN Product   p ON p.ProductId   = b.ProductId
                           JOIN Warehouse w ON w.WarehouseId = b.WarehouseId
                           WHERE b.ExpiryDate IS NOT NULL
                             AND b.ExpiryDate >= CURDATE()
                             AND DATEDIFF(b.ExpiryDate,CURDATE()) <= @days
                           ORDER BY b.ExpiryDate";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@days", days);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read()) list.Add(Map(rdr));
            return list;
        }

        public bool Insert(Batch b)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"INSERT INTO Batch(BatchId,ProductId,WarehouseId,BatchNumber,ManufacturingDate,ExpiryDate,Quantity)
                           VALUES(@id,@pid,@wid,@num,@mfg,@exp,@qty)";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id",  b.BatchId);
            cmd.Parameters.AddWithValue("@pid", b.ProductId);
            cmd.Parameters.AddWithValue("@wid", b.WarehouseId);
            cmd.Parameters.AddWithValue("@num", b.BatchNumber);
            cmd.Parameters.AddWithValue("@mfg", b.ManufacturingDate.HasValue ? b.ManufacturingDate.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@exp", b.ExpiryDate.HasValue ? b.ExpiryDate.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@qty", b.Quantity);
            return cmd.ExecuteNonQuery() > 0;
        }

        private static Batch Map(MySqlDataReader r) => new()
        {
            BatchId           = r["BatchId"].ToString()!,
            ProductId         = r["ProductId"].ToString()!,
            ProductName       = r["ProductName"].ToString()!,
            WarehouseId       = r["WarehouseId"].ToString()!,
            WarehouseName     = r["WarehouseName"].ToString()!,
            BatchNumber       = r["BatchNumber"].ToString()!,
            ManufacturingDate = r["ManufacturingDate"] == DBNull.Value ? null : Convert.ToDateTime(r["ManufacturingDate"]),
            ExpiryDate        = r["ExpiryDate"] == DBNull.Value ? null : Convert.ToDateTime(r["ExpiryDate"]),
            Quantity          = Convert.ToDecimal(r["Quantity"])
        };
    }
}
