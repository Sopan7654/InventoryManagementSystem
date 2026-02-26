// DataAccess/PurchaseOrderRepository.cs
using MySql.Data.MySqlClient;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.DataAccess
{
    public class PurchaseOrderRepository
    {
        public List<PurchaseOrder> GetAll()
        {
            var list = new List<PurchaseOrder>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT po.PurchaseOrderId, po.SupplierId, s.SupplierName,
                                  po.OrderDate, po.Status
                           FROM PurchaseOrder po
                           JOIN Supplier s ON s.SupplierId = po.SupplierId
                           ORDER BY po.OrderDate DESC";
            using var cmd = new MySqlCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read()) list.Add(MapPO(rdr));
            return list;
        }

        public bool InsertPO(PurchaseOrder po)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = "INSERT INTO PurchaseOrder(PurchaseOrderId,SupplierId,OrderDate,Status) VALUES(@id,@sid,@date,@st)";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id",   po.PurchaseOrderId);
            cmd.Parameters.AddWithValue("@sid",  po.SupplierId);
            cmd.Parameters.AddWithValue("@date", po.OrderDate.Date);
            cmd.Parameters.AddWithValue("@st",   po.Status);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool InsertItem(PurchaseOrderItem item)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = "INSERT INTO PurchaseOrderItem(POItemId,PurchaseOrderId,ProductId,QuantityOrdered,UnitPrice) VALUES(@id,@poid,@pid,@qty,@price)";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id",    item.POItemId);
            cmd.Parameters.AddWithValue("@poid",  item.PurchaseOrderId);
            cmd.Parameters.AddWithValue("@pid",   item.ProductId);
            cmd.Parameters.AddWithValue("@qty",   item.QuantityOrdered);
            cmd.Parameters.AddWithValue("@price", item.UnitPrice);
            return cmd.ExecuteNonQuery() > 0;
        }

        public List<PurchaseOrderItem> GetItemsByPO(string poId)
        {
            var list = new List<PurchaseOrderItem>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT poi.POItemId, poi.PurchaseOrderId, poi.ProductId,
                                  p.ProductName, poi.QuantityOrdered, poi.UnitPrice
                           FROM PurchaseOrderItem poi
                           JOIN Product p ON p.ProductId = poi.ProductId
                           WHERE poi.PurchaseOrderId=@poid";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@poid", poId);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read()) list.Add(MapItem(rdr));
            return list;
        }

        public bool UpdateStatus(string poId, string status)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("UPDATE PurchaseOrder SET Status=@st WHERE PurchaseOrderId=@id", conn);
            cmd.Parameters.AddWithValue("@st", status);
            cmd.Parameters.AddWithValue("@id", poId);
            return cmd.ExecuteNonQuery() > 0;
        }

        private static PurchaseOrder MapPO(MySqlDataReader r) => new()
        {
            PurchaseOrderId = r["PurchaseOrderId"].ToString()!,
            SupplierId      = r["SupplierId"].ToString()!,
            SupplierName    = r["SupplierName"].ToString()!,
            OrderDate       = Convert.ToDateTime(r["OrderDate"]),
            Status          = r["Status"].ToString()!
        };

        private static PurchaseOrderItem MapItem(MySqlDataReader r) => new()
        {
            POItemId        = r["POItemId"].ToString()!,
            PurchaseOrderId = r["PurchaseOrderId"].ToString()!,
            ProductId       = r["ProductId"].ToString()!,
            ProductName     = r["ProductName"].ToString()!,
            QuantityOrdered = Convert.ToDecimal(r["QuantityOrdered"]),
            UnitPrice       = Convert.ToDecimal(r["UnitPrice"])
        };
    }
}
