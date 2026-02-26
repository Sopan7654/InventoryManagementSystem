// Services/ReportService.cs
using InventoryManagementSystem.DataAccess;
using InventoryManagementSystem.Models;
using MySql.Data.MySqlClient;

namespace InventoryManagementSystem.Services
{
    public class ReportService
    {
        private readonly StockLevelRepository _stockRepo;
        private readonly StockTransactionRepository _txnRepo;
        private readonly BatchRepository _batchRepo;
        private readonly ProductRepository _productRepo;

        public ReportService()
        {
            _stockRepo   = new StockLevelRepository();
            _txnRepo     = new StockTransactionRepository();
            _batchRepo   = new BatchRepository();
            _productRepo = new ProductRepository();
        }

        public List<StockLevel> GetLowStockAlerts() => _stockRepo.GetLowStock();

        public List<Batch> GetExpiringBatches(int days = 30) => _batchRepo.GetExpiringSoon(days);

        // Inventory value (cost x quantity)
        public List<(string SKU, string Product, string Warehouse, decimal Qty, decimal Cost, decimal TotalValue)>
            GetInventoryValue()
        {
            var result = new List<(string, string, string, decimal, decimal, decimal)>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT p.SKU, p.ProductName, w.WarehouseName,
                                  sl.QuantityOnHand, p.Cost,
                                  ROUND(sl.QuantityOnHand * p.Cost, 2) AS TotalValue
                           FROM StockLevel sl
                           JOIN Product p ON p.ProductId = sl.ProductId
                           JOIN Warehouse w ON w.WarehouseId = sl.WarehouseId
                           WHERE p.IsActive = 1
                           ORDER BY TotalValue DESC";
            using var cmd = new MySqlCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                result.Add((
                    rdr["SKU"].ToString()!,
                    rdr["ProductName"].ToString()!,
                    rdr["WarehouseName"].ToString()!,
                    Convert.ToDecimal(rdr["QuantityOnHand"]),
                    Convert.ToDecimal(rdr["Cost"]),
                    Convert.ToDecimal(rdr["TotalValue"])
                ));
            return result;
        }

        // ABC Analysis: A = top 70% value, B = next 20%, C = remaining 10%
        public List<(string SKU, string Product, decimal TotalValue, string ABCClass)> GetABCAnalysis()
        {
            var valuation = GetInventoryValue();
            decimal grandTotal = valuation.Sum(v => v.TotalValue);

            var result = new List<(string, string, decimal, string)>();
            decimal running = 0;

            foreach (var item in valuation.OrderByDescending(v => v.TotalValue))
            {
                running += item.TotalValue;
                double pct = grandTotal > 0 ? (double)(running / grandTotal) * 100 : 100;
                string cls = pct <= 70 ? "A" : pct <= 90 ? "B" : "C";
                result.Add((item.SKU, item.Product, item.TotalValue, cls));
            }
            return result;
        }

        // Stock transaction history
        public List<StockTransaction> GetTransactionHistory(int limit = 20) => _txnRepo.GetAll(limit);

        // Stock summary per warehouse
        public List<(string Warehouse, int Products, decimal TotalQty, decimal TotalValue)> GetWarehouseSummary()
        {
            var result = new List<(string, int, decimal, decimal)>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT w.WarehouseName,
                                  COUNT(DISTINCT sl.ProductId) AS Products,
                                  SUM(sl.QuantityOnHand) AS TotalQty,
                                  ROUND(SUM(sl.QuantityOnHand * p.Cost), 2) AS TotalValue
                           FROM StockLevel sl
                           JOIN Product p ON p.ProductId = sl.ProductId
                           JOIN Warehouse w ON w.WarehouseId = sl.WarehouseId
                           GROUP BY w.WarehouseId, w.WarehouseName
                           ORDER BY TotalValue DESC";
            using var cmd = new MySqlCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                result.Add((
                    rdr["WarehouseName"].ToString()!,
                    Convert.ToInt32(rdr["Products"]),
                    Convert.ToDecimal(rdr["TotalQty"]),
                    Convert.ToDecimal(rdr["TotalValue"])
                ));
            return result;
        }
    }
}
