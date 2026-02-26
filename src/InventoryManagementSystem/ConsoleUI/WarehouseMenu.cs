// ConsoleUI/WarehouseMenu.cs
using InventoryManagementSystem.DataAccess;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.ConsoleUI
{
    public class WarehouseMenu
    {
        private readonly WarehouseRepository _repo = new();
        private readonly BatchRepository _batchRepo = new();

        public void Show()
        {
            while (true)
            {
                ConsoleHelper.PrintHeader("Warehouse Management");
                Console.WriteLine("  1. View All Warehouses");
                Console.WriteLine("  2. Add New Warehouse");
                Console.WriteLine("  3. View Batches / Lots");
                Console.WriteLine("  4. Add New Batch");
                Console.WriteLine("  0. Back");

                string choice = ConsoleHelper.AskInput("Select");
                switch (choice)
                {
                    case "1": ViewAll(); break;
                    case "2": AddWarehouse(); break;
                    case "3": ViewBatches(); break;
                    case "4": AddBatch(); break;
                    case "0": return;
                    default: ConsoleHelper.PrintError("Invalid option."); break;
                }
                ConsoleHelper.PressEnterToContinue();
            }
        }

        private void ViewAll()
        {
            ConsoleHelper.PrintHeader("All Warehouses");
            var list = _repo.GetAll();
            var rows = list.Select(w => new[]
            {
                w.WarehouseId, w.WarehouseName,
                w.Location ?? "-",
                w.Capacity.HasValue ? w.Capacity.Value.ToString("N2") : "-"
            }).ToList();

            ConsoleHelper.PrintTable(
                new[] { "ID", "Warehouse Name", "Location", "Capacity" },
                new[] { 6, 22, 36, 12 },
                rows
            );
        }

        private void AddWarehouse()
        {
            ConsoleHelper.PrintHeader("Add New Warehouse");
            var w = new Warehouse
            {
                WarehouseId   = ConsoleHelper.AskRequired("Warehouse ID (e.g. W4)"),
                WarehouseName = ConsoleHelper.AskRequired("Warehouse Name"),
                Location      = ConsoleHelper.AskInput("Location"),
                Capacity      = null
            };
            string capStr = ConsoleHelper.AskInput("Capacity (optional)");
            if (decimal.TryParse(capStr, out decimal cap)) w.Capacity = cap;

            bool ok = _repo.Insert(w);
            if (ok) ConsoleHelper.PrintSuccess($"Warehouse '{w.WarehouseName}' added.");
            else    ConsoleHelper.PrintError("Failed to add warehouse.");
        }

        private void ViewBatches()
        {
            ConsoleHelper.PrintHeader("Batch / Lot Tracking");
            var batches = _batchRepo.GetAll();
            var rows = batches.Select(b => new[]
            {
                b.BatchId, b.BatchNumber, b.ProductName, b.WarehouseName,
                b.Quantity.ToString("N2"),
                b.ManufacturingDate?.ToString("dd-MM-yyyy") ?? "-",
                b.ExpiryDate?.ToString("dd-MM-yyyy") ?? "No Expiry",
                b.ExpiryStatus
            }).ToList();

            ConsoleHelper.PrintTable(
                new[] { "ID", "Batch #", "Product", "Warehouse", "Qty", "Mfg Date", "Expiry Date", "Status" },
                new[] { 6, 16, 20, 14, 8, 12, 12, 14 },
                rows
            );
        }

        private void AddBatch()
        {
            ConsoleHelper.PrintHeader("Add New Batch");
            var warehouses = _repo.GetAll();
            ConsoleHelper.PrintSectionTitle("Warehouses");
            foreach (var w in warehouses)
                Console.WriteLine($"  {w.WarehouseId,-6} {w.WarehouseName}");

            var b = new Batch
            {
                BatchId     = "B-" + DateTime.Now.Ticks,
                ProductId   = ConsoleHelper.AskRequired("Product ID"),
                WarehouseId = ConsoleHelper.AskRequired("Warehouse ID"),
                BatchNumber = ConsoleHelper.AskRequired("Batch Number"),
                Quantity    = ConsoleHelper.AskDecimal("Quantity")
            };

            string mfg = ConsoleHelper.AskInput("Manufacturing Date (dd-MM-yyyy, optional)");
            string exp = ConsoleHelper.AskInput("Expiry Date (dd-MM-yyyy, optional)");

            if (DateTime.TryParseExact(mfg, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var mfgDate))
                b.ManufacturingDate = mfgDate;
            if (DateTime.TryParseExact(exp, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var expDate))
                b.ExpiryDate = expDate;

            bool ok = _batchRepo.Insert(b);
            if (ok) ConsoleHelper.PrintSuccess($"Batch '{b.BatchNumber}' added.");
            else    ConsoleHelper.PrintError("Failed to add batch.");
        }
    }
}
