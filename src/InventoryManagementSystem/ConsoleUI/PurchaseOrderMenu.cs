// ConsoleUI/PurchaseOrderMenu.cs
using InventoryManagementSystem.DataAccess;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services;

namespace InventoryManagementSystem.ConsoleUI
{
    public class PurchaseOrderMenu
    {
        private readonly PurchaseOrderRepository _repo = new();
        private readonly SupplierRepository _supplierRepo = new();
        private readonly ProductRepository _productRepo = new();
        private readonly InventoryService _inventoryService = new();

        public void Show()
        {
            while (true)
            {
                ConsoleHelper.PrintHeader("Purchase Orders");
                Console.WriteLine("  1. View All Purchase Orders");
                Console.WriteLine("  2. Create New Purchase Order");
                Console.WriteLine("  3. Receive Purchase Order (Update Stock)");
                Console.WriteLine("  0. Back");

                string choice = ConsoleHelper.AskInput("Select");
                switch (choice)
                {
                    case "1": ViewAll(); break;
                    case "2": CreatePO(); break;
                    case "3": ReceivePO(); break;
                    case "0": return;
                    default:  ConsoleHelper.PrintError("Invalid option."); break;
                }
                ConsoleHelper.PressEnterToContinue();
            }
        }

        private void ViewAll()
        {
            ConsoleHelper.PrintHeader("All Purchase Orders");
            var pos = _repo.GetAll();
            var rows = pos.Select(po => new[]
            {
                po.PurchaseOrderId, po.SupplierName,
                po.OrderDate.ToString("dd-MM-yyyy"), po.Status
            }).ToList();

            ConsoleHelper.PrintTable(
                new[] { "PO ID", "Supplier", "Order Date", "Status" },
                new[] { 12, 26, 14, 12 },
                rows
            );
        }

        private void CreatePO()
        {
            ConsoleHelper.PrintHeader("Create Purchase Order");

            var suppliers = _supplierRepo.GetAll();
            ConsoleHelper.PrintSectionTitle("Suppliers");
            foreach (var s in suppliers)
                Console.WriteLine($"  {s.SupplierId,-6} {s.SupplierName}");

            string poId = "PO-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string sid  = ConsoleHelper.AskRequired("Supplier ID");

            var po = new PurchaseOrder
            {
                PurchaseOrderId = poId,
                SupplierId      = sid,
                OrderDate       = DateTime.Today,
                Status          = "PENDING"
            };
            _repo.InsertPO(po);

            var products = _productRepo.GetAll();
            ConsoleHelper.PrintSectionTitle("Products");
            foreach (var p in products.Where(p => p.IsActive))
                Console.WriteLine($"  {p.ProductId,-6} {p.SKU,-12} {p.ProductName}");

            ConsoleHelper.PrintInfo("\nAdd items to this PO (enter blank Product ID to finish):");
            int i = 1;
            while (true)
            {
                string pid = ConsoleHelper.AskInput($"Item {i} - Product ID (blank to finish)");
                if (string.IsNullOrWhiteSpace(pid)) break;

                var item = new PurchaseOrderItem
                {
                    POItemId        = $"{poId}-I{i}",
                    PurchaseOrderId = poId,
                    ProductId       = pid,
                    QuantityOrdered = ConsoleHelper.AskDecimal("  Quantity"),
                    UnitPrice       = ConsoleHelper.AskDecimal("  Unit Price")
                };
                _repo.InsertItem(item);
                i++;
            }

            ConsoleHelper.PrintSuccess($"Purchase Order {poId} created with {i-1} item(s).");
        }

        private void ReceivePO()
        {
            ConsoleHelper.PrintHeader("Receive Purchase Order");
            string poId = ConsoleHelper.AskRequired("Enter Purchase Order ID");

            var items = _repo.GetItemsByPO(poId);
            if (items.Count == 0) { ConsoleHelper.PrintError("PO not found or has no items."); return; }

            string wid = ConsoleHelper.AskRequired("Receiving Warehouse ID");

            foreach (var item in items)
            {
                var (ok, msg) = _inventoryService.StockIn(item.ProductId, wid, item.QuantityOrdered, poId);
                Console.WriteLine($"  {item.ProductName}: {(ok ? "" : "FAILED - ")}{msg}");
            }

            _repo.UpdateStatus(poId, "RECEIVED");
            ConsoleHelper.PrintSuccess($"PO {poId} marked as RECEIVED and stock updated.");
        }
    }
}
