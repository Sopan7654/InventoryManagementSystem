// Services/InventoryService.cs
using InventoryManagementSystem.DataAccess;
using InventoryManagementSystem.Models;
using MySql.Data.MySqlClient;

namespace InventoryManagementSystem.Services
{
    public class InventoryService
    {
        private readonly StockLevelRepository _stockRepo;
        private readonly StockTransactionRepository _txnRepo;

        public InventoryService()
        {
            _stockRepo = new StockLevelRepository();
            _txnRepo   = new StockTransactionRepository();
        }

        // Constructor for testing (dependency injection)
        public InventoryService(StockLevelRepository stockRepo, StockTransactionRepository txnRepo)
        {
            _stockRepo = stockRepo;
            _txnRepo   = txnRepo;
        }

        public (bool Success, string Message) StockIn(string productId, string warehouseId, decimal qty, string reference)
        {
            if (qty <= 0) return (false, "Quantity must be greater than zero.");

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var txn = conn.BeginTransaction();
            try
            {
                var existing = _stockRepo.GetByProductAndWarehouse(productId, warehouseId);
                if (existing != null)
                {
                    string upd = "UPDATE StockLevel SET QuantityOnHand=QuantityOnHand+@qty WHERE ProductId=@pid AND WarehouseId=@wid";
                    using var cmd = new MySqlCommand(upd, conn, txn);
                    cmd.Parameters.AddWithValue("@qty", qty);
                    cmd.Parameters.AddWithValue("@pid", productId);
                    cmd.Parameters.AddWithValue("@wid", warehouseId);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    string newId = "SL-" + DateTime.Now.Ticks;
                    string ins = @"INSERT INTO StockLevel(StockLevelId,ProductId,WarehouseId,QuantityOnHand,ReorderLevel,SafetyStock,ReservedQuantity)
                                   VALUES(@id,@pid,@wid,@qty,0,0,0)";
                    using var cmd = new MySqlCommand(ins, conn, txn);
                    cmd.Parameters.AddWithValue("@id",  newId);
                    cmd.Parameters.AddWithValue("@pid", productId);
                    cmd.Parameters.AddWithValue("@wid", warehouseId);
                    cmd.Parameters.AddWithValue("@qty", qty);
                    cmd.ExecuteNonQuery();
                }

                string txnId = "T-" + DateTime.Now.Ticks;
                string t = @"INSERT INTO StockTransaction(TransactionId,ProductId,WarehouseId,TransactionType,Quantity,TransactionDate,Reference)
                             VALUES(@id,@pid,@wid,'PURCHASE',@qty,NOW(),@ref)";
                using var tcmd = new MySqlCommand(t, conn, txn);
                tcmd.Parameters.AddWithValue("@id",  txnId);
                tcmd.Parameters.AddWithValue("@pid", productId);
                tcmd.Parameters.AddWithValue("@wid", warehouseId);
                tcmd.Parameters.AddWithValue("@qty", qty);
                tcmd.Parameters.AddWithValue("@ref", reference ?? (object)DBNull.Value);
                tcmd.ExecuteNonQuery();

                txn.Commit();
                return (true, $"Stock In successful. +{qty} units added.");
            }
            catch (Exception ex)
            {
                txn.Rollback();
                return (false, $"Error: {ex.Message}");
            }
        }

        public (bool Success, string Message) StockOut(string productId, string warehouseId, decimal qty, string reference)
        {
            if (qty <= 0) return (false, "Quantity must be greater than zero.");

            var stock = _stockRepo.GetByProductAndWarehouse(productId, warehouseId);
            if (stock == null) return (false, "No stock record found.");
            if (stock.AvailableQuantity < qty)
                return (false, $"Insufficient stock. Available: {stock.AvailableQuantity}, Requested: {qty}");

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var txn = conn.BeginTransaction();
            try
            {
                string upd = "UPDATE StockLevel SET QuantityOnHand=QuantityOnHand-@qty WHERE ProductId=@pid AND WarehouseId=@wid";
                using var cmd = new MySqlCommand(upd, conn, txn);
                cmd.Parameters.AddWithValue("@qty", qty);
                cmd.Parameters.AddWithValue("@pid", productId);
                cmd.Parameters.AddWithValue("@wid", warehouseId);
                cmd.ExecuteNonQuery();

                string t = @"INSERT INTO StockTransaction(TransactionId,ProductId,WarehouseId,TransactionType,Quantity,TransactionDate,Reference)
                             VALUES(@id,@pid,@wid,'SALE',@qty,NOW(),@ref)";
                using var tcmd = new MySqlCommand(t, conn, txn);
                tcmd.Parameters.AddWithValue("@id",  "T-" + DateTime.Now.Ticks);
                tcmd.Parameters.AddWithValue("@pid", productId);
                tcmd.Parameters.AddWithValue("@wid", warehouseId);
                tcmd.Parameters.AddWithValue("@qty", qty);
                tcmd.Parameters.AddWithValue("@ref", reference ?? (object)DBNull.Value);
                tcmd.ExecuteNonQuery();

                txn.Commit();
                return (true, $"Stock Out successful. -{qty} units removed.");
            }
            catch (Exception ex)
            {
                txn.Rollback();
                return (false, $"Error: {ex.Message}");
            }
        }

        public (bool Success, string Message) Transfer(string productId, string fromWarehouseId, string toWarehouseId, decimal qty)
        {
            if (qty <= 0) return (false, "Quantity must be greater than zero.");
            if (fromWarehouseId == toWarehouseId) return (false, "Source and destination warehouses must be different.");

            var stock = _stockRepo.GetByProductAndWarehouse(productId, fromWarehouseId);
            if (stock == null) return (false, "No stock in source warehouse.");
            if (stock.AvailableQuantity < qty)
                return (false, $"Insufficient stock. Available: {stock.AvailableQuantity}");

            string ref_ = "TRF-" + DateTime.Now.ToString("yyyyMMddHHmmss");

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var txn = conn.BeginTransaction();
            try
            {
                // Deduct from source
                string deduct = "UPDATE StockLevel SET QuantityOnHand=QuantityOnHand-@qty WHERE ProductId=@pid AND WarehouseId=@wid";
                using var cmd1 = new MySqlCommand(deduct, conn, txn);
                cmd1.Parameters.AddWithValue("@qty", qty);
                cmd1.Parameters.AddWithValue("@pid", productId);
                cmd1.Parameters.AddWithValue("@wid", fromWarehouseId);
                cmd1.ExecuteNonQuery();

                // Add to destination (insert if not exists)
                var dest = _stockRepo.GetByProductAndWarehouse(productId, toWarehouseId);
                if (dest != null)
                {
                    string add = "UPDATE StockLevel SET QuantityOnHand=QuantityOnHand+@qty WHERE ProductId=@pid AND WarehouseId=@wid";
                    using var cmd2 = new MySqlCommand(add, conn, txn);
                    cmd2.Parameters.AddWithValue("@qty", qty);
                    cmd2.Parameters.AddWithValue("@pid", productId);
                    cmd2.Parameters.AddWithValue("@wid", toWarehouseId);
                    cmd2.ExecuteNonQuery();
                }
                else
                {
                    string ins = @"INSERT INTO StockLevel(StockLevelId,ProductId,WarehouseId,QuantityOnHand,ReorderLevel,SafetyStock,ReservedQuantity)
                                   VALUES(@id,@pid,@wid,@qty,0,0,0)";
                    using var cmd2 = new MySqlCommand(ins, conn, txn);
                    cmd2.Parameters.AddWithValue("@id",  "SL-" + DateTime.Now.Ticks);
                    cmd2.Parameters.AddWithValue("@pid", productId);
                    cmd2.Parameters.AddWithValue("@wid", toWarehouseId);
                    cmd2.Parameters.AddWithValue("@qty", qty);
                    cmd2.ExecuteNonQuery();
                }

                // Log TRANSFER_OUT
                string t1 = @"INSERT INTO StockTransaction(TransactionId,ProductId,WarehouseId,TransactionType,Quantity,TransactionDate,Reference)
                              VALUES(@id,@pid,@wid,'TRANSFER_OUT',@qty,NOW(),@ref)";
                using var tc1 = new MySqlCommand(t1, conn, txn);
                tc1.Parameters.AddWithValue("@id",  "T-" + DateTime.Now.Ticks);
                tc1.Parameters.AddWithValue("@pid", productId);
                tc1.Parameters.AddWithValue("@wid", fromWarehouseId);
                tc1.Parameters.AddWithValue("@qty", qty);
                tc1.Parameters.AddWithValue("@ref", ref_);
                tc1.ExecuteNonQuery();

                // Log TRANSFER_IN
                string t2 = @"INSERT INTO StockTransaction(TransactionId,ProductId,WarehouseId,TransactionType,Quantity,TransactionDate,Reference)
                              VALUES(@id,@pid,@wid,'TRANSFER_IN',@qty,NOW(),@ref)";
                using var tc2 = new MySqlCommand(t2, conn, txn);
                tc2.Parameters.AddWithValue("@id",  "T-" + (DateTime.Now.Ticks + 1));
                tc2.Parameters.AddWithValue("@pid", productId);
                tc2.Parameters.AddWithValue("@wid", toWarehouseId);
                tc2.Parameters.AddWithValue("@qty", qty);
                tc2.Parameters.AddWithValue("@ref", ref_);
                tc2.ExecuteNonQuery();

                txn.Commit();
                return (true, $"Transfer complete. {qty} units moved. Ref: {ref_}");
            }
            catch (Exception ex)
            {
                txn.Rollback();
                return (false, $"Error: {ex.Message}");
            }
        }

        public (bool Success, string Message) HoldStock(string productId, string warehouseId, decimal qty, string reference)
        {
            var stock = _stockRepo.GetByProductAndWarehouse(productId, warehouseId);
            if (stock == null) return (false, "No stock record found.");
            if (stock.AvailableQuantity < qty)
                return (false, $"Cannot hold. Available: {stock.AvailableQuantity}");

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var txn = conn.BeginTransaction();
            try
            {
                string upd = "UPDATE StockLevel SET ReservedQuantity=ReservedQuantity+@qty WHERE ProductId=@pid AND WarehouseId=@wid";
                using var cmd = new MySqlCommand(upd, conn, txn);
                cmd.Parameters.AddWithValue("@qty", qty); cmd.Parameters.AddWithValue("@pid", productId); cmd.Parameters.AddWithValue("@wid", warehouseId);
                cmd.ExecuteNonQuery();

                string t = @"INSERT INTO StockTransaction(TransactionId,ProductId,WarehouseId,TransactionType,Quantity,TransactionDate,Reference)
                             VALUES(@id,@pid,@wid,'HOLD',@qty,NOW(),@ref)";
                using var tc = new MySqlCommand(t, conn, txn);
                tc.Parameters.AddWithValue("@id",  "T-" + DateTime.Now.Ticks);
                tc.Parameters.AddWithValue("@pid", productId); tc.Parameters.AddWithValue("@wid", warehouseId);
                tc.Parameters.AddWithValue("@qty", qty); tc.Parameters.AddWithValue("@ref", reference ?? (object)DBNull.Value);
                tc.ExecuteNonQuery();

                txn.Commit();
                return (true, $"{qty} units held. Reference: {reference}");
            }
            catch (Exception ex) { txn.Rollback(); return (false, ex.Message); }
        }

        public (bool Success, string Message) Adjustment(string productId, string warehouseId, decimal qty, string reason)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var txn = conn.BeginTransaction();
            try
            {
                string upd = "UPDATE StockLevel SET QuantityOnHand=QuantityOnHand+@qty WHERE ProductId=@pid AND WarehouseId=@wid";
                using var cmd = new MySqlCommand(upd, conn, txn);
                cmd.Parameters.AddWithValue("@qty", qty); cmd.Parameters.AddWithValue("@pid", productId); cmd.Parameters.AddWithValue("@wid", warehouseId);
                cmd.ExecuteNonQuery();

                string t = @"INSERT INTO StockTransaction(TransactionId,ProductId,WarehouseId,TransactionType,Quantity,TransactionDate,Reference)
                             VALUES(@id,@pid,@wid,'ADJUSTMENT',@qty,NOW(),@ref)";
                using var tc = new MySqlCommand(t, conn, txn);
                tc.Parameters.AddWithValue("@id",  "T-" + DateTime.Now.Ticks);
                tc.Parameters.AddWithValue("@pid", productId); tc.Parameters.AddWithValue("@wid", warehouseId);
                tc.Parameters.AddWithValue("@qty", qty); tc.Parameters.AddWithValue("@ref", reason ?? (object)DBNull.Value);
                tc.ExecuteNonQuery();

                txn.Commit();
                return (true, $"Adjustment applied: {(qty >= 0 ? "+" : "")}{qty} units.");
            }
            catch (Exception ex) { txn.Rollback(); return (false, ex.Message); }
        }
    }
}
