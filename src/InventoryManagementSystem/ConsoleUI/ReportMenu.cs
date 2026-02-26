// ConsoleUI/ReportMenu.cs
using InventoryManagementSystem.Services;

namespace InventoryManagementSystem.ConsoleUI
{
    public class ReportMenu
    {
        private readonly ReportService _service = new();

        public void Show()
        {
            while (true)
            {
                ConsoleHelper.PrintHeader("Reports & Alerts");
                Console.WriteLine("  1. Low Stock Alert");
                Console.WriteLine("  2. Expiring Batches (within 30 days)");
                Console.WriteLine("  3. Inventory Valuation Report");
                Console.WriteLine("  4. ABC Analysis");
                Console.WriteLine("  5. Transaction History (Last 20)");
                Console.WriteLine("  6. Warehouse Summary");
                Console.WriteLine("  0. Back");

                string choice = ConsoleHelper.AskInput("Select");
                switch (choice)
                {
                    case "1": LowStockAlert(); break;
                    case "2": ExpiringBatches(); break;
                    case "3": InventoryValuation(); break;
                    case "4": ABCAnalysis(); break;
                    case "5": TransactionHistory(); break;
                    case "6": WarehouseSummary(); break;
                    case "0": return;
                    default: ConsoleHelper.PrintError("Invalid option."); break;
                }
                ConsoleHelper.PressEnterToContinue();
            }
        }

        private void LowStockAlert()
        {
            ConsoleHelper.PrintHeader("Low Stock Alert");
            var items = _service.GetLowStockAlerts();
            if (items.Count == 0) { ConsoleHelper.PrintSuccess("All products are above reorder level!"); return; }

            ConsoleHelper.PrintWarning($"{items.Count} product(s) at or below reorder level:");
            var rows = items.Select(sl => new[]
            {
                sl.ProductName, sl.WarehouseName,
                sl.QuantityOnHand.ToString("N2"),
                sl.ReorderLevel.ToString("N2"),
                sl.SafetyStock.ToString("N2"),
                (sl.ReorderLevel - sl.QuantityOnHand).ToString("N2")
            }).ToList();

            ConsoleHelper.PrintTable(
                new[] { "Product", "Warehouse", "On Hand", "Reorder Lvl", "Safety Stk", "Shortfall" },
                new[] { 22, 18, 10, 12, 11, 10 },
                rows,
                ConsoleColor.Yellow
            );
        }

        private void ExpiringBatches()
        {
            ConsoleHelper.PrintHeader("Expiring Batches (Next 30 Days)");
            var batches = _service.GetExpiringBatches(30);
            if (batches.Count == 0) { ConsoleHelper.PrintSuccess("No batches expiring within 30 days."); return; }

            var rows = batches.Select(b => new[]
            {
                b.BatchNumber, b.ProductName, b.WarehouseName,
                b.Quantity.ToString("N2"),
                b.ExpiryDate?.ToString("dd-MM-yyyy") ?? "-",
                b.ExpiryStatus
            }).ToList();

            ConsoleHelper.PrintTable(
                new[] { "Batch #", "Product", "Warehouse", "Qty", "Expiry Date", "Status" },
                new[] { 16, 22, 16, 8, 13, 14 },
                rows,
                ConsoleColor.Red
            );
        }

        private void InventoryValuation()
        {
            ConsoleHelper.PrintHeader("Inventory Valuation Report");
            var items = _service.GetInventoryValue();
            decimal grandTotal = items.Sum(i => i.TotalValue);

            var rows = items.Select(i => new[]
            {
                i.SKU, i.Product, i.Warehouse,
                i.Qty.ToString("N2"),
                i.Cost.ToString("N2"),
                i.TotalValue.ToString("N2")
            }).ToList();

            ConsoleHelper.PrintTable(
                new[] { "SKU", "Product", "Warehouse", "Qty", "Cost", "Total Value" },
                new[] { 10, 22, 16, 10, 10, 14 },
                rows
            );

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n  Grand Total Stock Value: {grandTotal:N2}");
            Console.ResetColor();
        }

        private void ABCAnalysis()
        {
            ConsoleHelper.PrintHeader("ABC Analysis (Inventory Classification)");
            ConsoleHelper.PrintInfo("A = Top 70% value | B = Next 20% | C = Bottom 10%");
            var items = _service.GetABCAnalysis();

            var rows = items.Select(i => new[]
            {
                i.SKU, i.Product,
                i.TotalValue.ToString("N2"),
                i.ABCClass
            }).ToList();

            ConsoleHelper.PrintTable(
                new[] { "SKU", "Product Name", "Stock Value", "Class" },
                new[] { 10, 28, 14, 6 },
                rows
            );

            // Summary
            foreach (var cls in new[] { "A", "B", "C" })
            {
                var group = items.Where(i => i.ABCClass == cls).ToList();
                Console.ForegroundColor = cls == "A" ? ConsoleColor.Green : cls == "B" ? ConsoleColor.Yellow : ConsoleColor.Red;
                Console.WriteLine($"  Class {cls}: {group.Count} product(s) — Total Value: {group.Sum(g => g.TotalValue):N2}");
            }
            Console.ResetColor();
        }

        private void TransactionHistory()
        {
            ConsoleHelper.PrintHeader("Recent Transaction History");
            var txns = _service.GetTransactionHistory(20);

            var rows = txns.Select(t => new[]
            {
                t.TransactionDate.ToString("dd-MM-yy HH:mm"),
                t.TransactionType, t.ProductName,
                t.WarehouseName, t.Quantity.ToString("N2"),
                t.Reference ?? "-"
            }).ToList();

            ConsoleHelper.PrintTable(
                new[] { "Date", "Type", "Product", "Warehouse", "Qty", "Reference" },
                new[] { 15, 14, 22, 16, 10, 18 },
                rows
            );
        }

        private void WarehouseSummary()
        {
            ConsoleHelper.PrintHeader("Warehouse Stock Summary");
            var summary = _service.GetWarehouseSummary();

            var rows = summary.Select(s => new[]
            {
                s.Warehouse,
                s.Products.ToString(),
                s.TotalQty.ToString("N2"),
                s.TotalValue.ToString("N2")
            }).ToList();

            ConsoleHelper.PrintTable(
                new[] { "Warehouse", "Products", "Total Qty", "Total Value (Cost)" },
                new[] { 24, 10, 12, 20 },
                rows
            );
        }
    }
}
