// DataAccess/StockTransactionRepository.cs
using MySql.Data.MySqlClient;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.DataAccess
{
    public class StockTransactionRepository
    {
        public bool Insert(StockTransaction t)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"INSERT INTO StockTransaction
                            (TransactionId,ProductId,WarehouseId,TransactionType,Quantity,TransactionDate,Reference)
                           VALUES (@id,@pid,@wid,@type,@qty,@date,@ref)";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id",   t.TransactionId);
            cmd.Parameters.AddWithValue("@pid",  t.ProductId);
            cmd.Parameters.AddWithValue("@wid",  t.WarehouseId);
            cmd.Parameters.AddWithValue("@type", t.TransactionType);
            cmd.Parameters.AddWithValue("@qty",  t.Quantity);
            cmd.Parameters.AddWithValue("@date", t.TransactionDate);
            cmd.Parameters.AddWithValue("@ref",  t.Reference ?? (object)DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        public List<StockTransaction> GetAll(int limit = 50)
        {
            var list = new List<StockTransaction>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = $@"SELECT st.TransactionId, st.ProductId, p.ProductName,
                                   st.WarehouseId, w.WarehouseName,
                                   st.TransactionType, st.Quantity,
                                   st.TransactionDate, st.Reference
                            FROM StockTransaction st
                            JOIN Product   p ON p.ProductId   = st.ProductId
                            JOIN Warehouse w ON w.WarehouseId = st.WarehouseId
                            ORDER BY st.TransactionDate DESC
                            LIMIT {limit}";
            using var cmd = new MySqlCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read()) list.Add(Map(rdr));
            return list;
        }

        public List<StockTransaction> GetByProduct(string productId)
        {
            var list = new List<StockTransaction>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT st.TransactionId, st.ProductId, p.ProductName,
                                   st.WarehouseId, w.WarehouseName,
                                   st.TransactionType, st.Quantity,
                                   st.TransactionDate, st.Reference
                            FROM StockTransaction st
                            JOIN Product   p ON p.ProductId   = st.ProductId
                            JOIN Warehouse w ON w.WarehouseId = st.WarehouseId
                            WHERE st.ProductId=@pid
                            ORDER BY st.TransactionDate DESC";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pid", productId);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read()) list.Add(Map(rdr));
            return list;
        }

        private static StockTransaction Map(MySqlDataReader r) => new()
        {
            TransactionId   = r["TransactionId"].ToString()!,
            ProductId       = r["ProductId"].ToString()!,
            ProductName     = r["ProductName"].ToString()!,
            WarehouseId     = r["WarehouseId"].ToString()!,
            WarehouseName   = r["WarehouseName"].ToString()!,
            TransactionType = r["TransactionType"].ToString()!,
            Quantity        = Convert.ToDecimal(r["Quantity"]),
            TransactionDate = Convert.ToDateTime(r["TransactionDate"]),
            Reference       = r["Reference"] == DBNull.Value ? null : r["Reference"].ToString()
        };
    }
}
