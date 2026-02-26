// ConsoleUI/InventoryMenu.cs
using InventoryManagementSystem.DataAccess;
using InventoryManagementSystem.Services;

namespace InventoryManagementSystem.ConsoleUI
{
    public class InventoryMenu
    {
        private readonly InventoryService _service = new();
        private readonly StockLevelRepository _stockRepo = new();
        private readonly StockTransactionRepository _txnRepo = new();
        private readonly ProductRepository _productRepo = new();
        private readonly WarehouseRepository _warehouseRepo = new();

        public void Show()
        {
            while (true)
            {
                ConsoleHelper.PrintHeader("Inventory Operations");
                Console.WriteLine("  1. View All Stock Levels");
                Console.WriteLine("  2. Stock In  (Receive goods)");
                Console.WriteLine("  3. Stock Out (Issue/Sell goods)");
                Console.WriteLine("  4. Stock Transfer (Between warehouses)");
                Console.WriteLine("  5. Stock Hold (Reserve for order/quotation)");
                Console.WriteLine("  6. Stock Adjustment (Correction/Damage)");
                Console.WriteLine("  7. View Transaction History");
                Console.WriteLine("  8. View Stock by Product");
                Console.WriteLine("  0. Back");

                string choice = ConsoleHelper.AskInput("Select");
                switch (choice)
                {
                    case "1": ViewAllStock(); break;
                    case "2": StockIn(); break;
                    case "3": StockOut(); break;
                    case "4": Transfer(); break;
                    case "5": Hold(); break;
                    case "6": Adjustment(); break;
                    case "7": ViewHistory(); break;
                    case "8": ViewByProduct(); break;
                    case "0": return;
                    default: ConsoleHelper.PrintError("Invalid option."); break;
                }
                ConsoleHelper.PressEnterToContinue();
            }
        }

        private void ViewAllStock()
        {
            ConsoleHelper.PrintHeader("Current Stock Levels");
            var levels = _stockRepo.GetAll();
            var rows = levels.Select(sl => new[]
            {
                sl.ProductName, sl.WarehouseName,
                sl.QuantityOnHand.ToString("N2"),
                sl.ReservedQuantity.ToString("N2"),
                sl.AvailableQuantity.ToString("N2"),
                sl.ReorderLevel.ToString("N2"),
                sl.IsLowStock ? "⚠ LOW" : "OK"
            }).ToList();

            ConsoleHelper.PrintTable(
                new[] { "Product", "Warehouse", "On Hand", "Reserved", "Available", "Reorder", "Status" },
                new[] { 22, 18, 10, 10, 10, 10, 8 },
                rows
            );
        }

        private void StockIn()
        {
            ConsoleHelper.PrintHeader("Stock In — Receive Goods");
            ShowProductsAndWarehouses();

            string pid = ConsoleHelper.AskRequired("Product ID");
            string wid = ConsoleHelper.AskRequired("Warehouse ID");
            decimal qty = ConsoleHelper.AskDecimal("Quantity");
            string ref_ = ConsoleHelper.AskInput("Reference (PO number, etc.)");

            var (ok, msg) = _service.StockIn(pid, wid, qty, ref_);
            if (ok) ConsoleHelper.PrintSuccess(msg);
            else    ConsoleHelper.PrintError(msg);
        }

        private void StockOut()
        {
            ConsoleHelper.PrintHeader("Stock Out — Issue / Sell Goods");
            ShowProductsAndWarehouses();

            string pid = ConsoleHelper.AskRequired("Product ID");
            string wid = ConsoleHelper.AskRequired("Warehouse ID");
            decimal qty = ConsoleHelper.AskDecimal("Quantity");
            string ref_ = ConsoleHelper.AskInput("Reference (Invoice number, etc.)");

            var (ok, msg) = _service.StockOut(pid, wid, qty, ref_);
            if (ok) ConsoleHelper.PrintSuccess(msg);
            else    ConsoleHelper.PrintError(msg);
        }

        private void Transfer()
        {
            ConsoleHelper.PrintHeader("Stock Transfer");
            ShowProductsAndWarehouses();

            string pid = ConsoleHelper.AskRequired("Product ID");
            string from = ConsoleHelper.AskRequired("From Warehouse ID");
            string to   = ConsoleHelper.AskRequired("To Warehouse ID");
            decimal qty = ConsoleHelper.AskDecimal("Quantity");

            var (ok, msg) = _service.Transfer(pid, from, to, qty);
            if (ok) ConsoleHelper.PrintSuccess(msg);
            else    ConsoleHelper.PrintError(msg);
        }

        private void Hold()
        {
            ConsoleHelper.PrintHeader("Stock Hold — Reserve Inventory");
            ShowProductsAndWarehouses();

            string pid = ConsoleHelper.AskRequired("Product ID");
            string wid = ConsoleHelper.AskRequired("Warehouse ID");
            decimal qty = ConsoleHelper.AskDecimal("Quantity to Hold");
            string ref_ = ConsoleHelper.AskInput("Reference (Quotation number, etc.)");

            var (ok, msg) = _service.HoldStock(pid, wid, qty, ref_);
            if (ok) ConsoleHelper.PrintSuccess(msg);
            else    ConsoleHelper.PrintError(msg);
        }

        private void Adjustment()
        {
            ConsoleHelper.PrintHeader("Stock Adjustment");
            ConsoleHelper.PrintInfo("Use positive qty to ADD stock, negative qty to REMOVE stock.");
            ShowProductsAndWarehouses();

            string pid = ConsoleHelper.AskRequired("Product ID");
            string wid = ConsoleHelper.AskRequired("Warehouse ID");
            decimal qty = ConsoleHelper.AskDecimal("Adjustment Quantity (e.g. -10 or +5)");
            string reason = ConsoleHelper.AskInput("Reason");

            var (ok, msg) = _service.Adjustment(pid, wid, qty, reason);
            if (ok) ConsoleHelper.PrintSuccess(msg);
            else    ConsoleHelper.PrintError(msg);
        }

        private void ViewHistory()
        {
            ConsoleHelper.PrintHeader("Stock Transaction History");
            int limit = ConsoleHelper.AskInt("How many records to show? [20]", 20);
            var txns = _txnRepo.GetAll(limit);
            var rows = txns.Select(t => new[]
            {
                t.TransactionDate.ToString("dd-MM-yy HH:mm"),
                t.TransactionType,
                t.ProductName,
                t.WarehouseName,
                t.Quantity.ToString("N2"),
                t.Reference ?? "-"
            }).ToList();

            ConsoleHelper.PrintTable(
                new[] { "Date", "Type", "Product", "Warehouse", "Qty", "Reference" },
                new[] { 15, 14, 22, 16, 10, 18 },
                rows
            );
        }

        private void ViewByProduct()
        {
            ConsoleHelper.PrintHeader("Stock History by Product");
            string pid = ConsoleHelper.AskRequired("Enter Product ID");
            var txns = _txnRepo.GetByProduct(pid);

            if (txns.Count == 0) { ConsoleHelper.PrintWarning("No transactions found."); return; }

            ConsoleHelper.PrintInfo($"Product: {txns[0].ProductName}");
            var rows = txns.Select(t => new[]
            {
                t.TransactionDate.ToString("dd-MM-yy HH:mm"),
                t.TransactionType, t.WarehouseName,
                t.Quantity.ToString("N2"), t.Reference ?? "-"
            }).ToList();

            ConsoleHelper.PrintTable(
                new[] { "Date", "Type", "Warehouse", "Qty", "Reference" },
                new[] { 15, 15, 16, 10, 20 },
                rows
            );
        }

        private void ShowProductsAndWarehouses()
        {
            var products   = _productRepo.GetAll();
            var warehouses = _warehouseRepo.GetAll();

            ConsoleHelper.PrintSectionTitle("Products");
            foreach (var p in products.Where(p => p.IsActive))
                Console.WriteLine($"  {p.ProductId,-6} {p.SKU,-12} {p.ProductName}");

            ConsoleHelper.PrintSectionTitle("Warehouses");
            foreach (var w in warehouses)
                Console.WriteLine($"  {w.WarehouseId,-6} {w.WarehouseName}");
        }
    }
}
