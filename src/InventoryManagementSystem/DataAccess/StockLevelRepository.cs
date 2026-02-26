// DataAccess/StockLevelRepository.cs
using MySql.Data.MySqlClient;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.DataAccess
{
    public class StockLevelRepository
    {
        public List<StockLevel> GetAll()
        {
            var list = new List<StockLevel>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT sl.StockLevelId, sl.ProductId, p.ProductName,
                                  sl.WarehouseId, w.WarehouseName,
                                  sl.QuantityOnHand, sl.ReorderLevel,
                                  sl.SafetyStock, sl.ReservedQuantity
                           FROM StockLevel sl
                           JOIN Product p   ON p.ProductId   = sl.ProductId
                           JOIN Warehouse w ON w.WarehouseId = sl.WarehouseId
                           ORDER BY p.ProductName, w.WarehouseName";
            using var cmd = new MySqlCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read()) list.Add(Map(rdr));
            return list;
        }

        public StockLevel? GetByProductAndWarehouse(string productId, string warehouseId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT sl.StockLevelId, sl.ProductId, p.ProductName,
                                  sl.WarehouseId, w.WarehouseName,
                                  sl.QuantityOnHand, sl.ReorderLevel,
                                  sl.SafetyStock, sl.ReservedQuantity
                           FROM StockLevel sl
                           JOIN Product p   ON p.ProductId   = sl.ProductId
                           JOIN Warehouse w ON w.WarehouseId = sl.WarehouseId
                           WHERE sl.ProductId=@pid AND sl.WarehouseId=@wid";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pid", productId);
            cmd.Parameters.AddWithValue("@wid", warehouseId);
            using var rdr = cmd.ExecuteReader();
            return rdr.Read() ? Map(rdr) : null;
        }

        public List<StockLevel> GetLowStock()
        {
            var list = new List<StockLevel>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT sl.StockLevelId, sl.ProductId, p.ProductName,
                                  sl.WarehouseId, w.WarehouseName,
                                  sl.QuantityOnHand, sl.ReorderLevel,
                                  sl.SafetyStock, sl.ReservedQuantity
                           FROM StockLevel sl
                           JOIN Product p   ON p.ProductId   = sl.ProductId
                           JOIN Warehouse w ON w.WarehouseId = sl.WarehouseId
                           WHERE sl.QuantityOnHand <= sl.ReorderLevel AND p.IsActive = 1
                           ORDER BY (sl.ReorderLevel - sl.QuantityOnHand) DESC";
            using var cmd = new MySqlCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read()) list.Add(Map(rdr));
            return list;
        }

        public void Upsert(string productId, string warehouseId, decimal qty, decimal reorder, decimal safety)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            // Simplified approach: check & update or insert
            var existing = GetByProductAndWarehouse(productId, warehouseId);
            if (existing != null)
            {
                string upd = "UPDATE StockLevel SET QuantityOnHand=QuantityOnHand+@qty WHERE ProductId=@pid AND WarehouseId=@wid";
                using var cmd2 = new MySqlCommand(upd, conn);
                cmd2.Parameters.AddWithValue("@qty", qty);
                cmd2.Parameters.AddWithValue("@pid", productId);
                cmd2.Parameters.AddWithValue("@wid", warehouseId);
                cmd2.ExecuteNonQuery();
            }
            else
            {
                string newId = "SL-" + DateTime.Now.Ticks;
                string ins = @"INSERT INTO StockLevel(StockLevelId,ProductId,WarehouseId,QuantityOnHand,ReorderLevel,SafetyStock,ReservedQuantity)
                               VALUES(@id,@pid,@wid,@qty,@ro,@ss,0)";
                using var cmd3 = new MySqlCommand(ins, conn);
                cmd3.Parameters.AddWithValue("@id",  newId);
                cmd3.Parameters.AddWithValue("@pid", productId);
                cmd3.Parameters.AddWithValue("@wid", warehouseId);
                cmd3.Parameters.AddWithValue("@qty", qty);
                cmd3.Parameters.AddWithValue("@ro",  reorder);
                cmd3.Parameters.AddWithValue("@ss",  safety);
                cmd3.ExecuteNonQuery();
            }
        }

        public bool UpdateQuantity(string productId, string warehouseId, decimal delta)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = "UPDATE StockLevel SET QuantityOnHand=QuantityOnHand+@delta WHERE ProductId=@pid AND WarehouseId=@wid";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@delta", delta);
            cmd.Parameters.AddWithValue("@pid",   productId);
            cmd.Parameters.AddWithValue("@wid",   warehouseId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateReserved(string productId, string warehouseId, decimal delta)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = "UPDATE StockLevel SET ReservedQuantity=ReservedQuantity+@delta WHERE ProductId=@pid AND WarehouseId=@wid";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@delta", delta);
            cmd.Parameters.AddWithValue("@pid",   productId);
            cmd.Parameters.AddWithValue("@wid",   warehouseId);
            return cmd.ExecuteNonQuery() > 0;
        }

        private static StockLevel Map(MySqlDataReader r) => new()
        {
            StockLevelId     = r["StockLevelId"].ToString()!,
            ProductId        = r["ProductId"].ToString()!,
            ProductName      = r["ProductName"].ToString()!,
            WarehouseId      = r["WarehouseId"].ToString()!,
            WarehouseName    = r["WarehouseName"].ToString()!,
            QuantityOnHand   = Convert.ToDecimal(r["QuantityOnHand"]),
            ReorderLevel     = Convert.ToDecimal(r["ReorderLevel"]),
            SafetyStock      = Convert.ToDecimal(r["SafetyStock"]),
            ReservedQuantity = Convert.ToDecimal(r["ReservedQuantity"])
        };
    }
}
