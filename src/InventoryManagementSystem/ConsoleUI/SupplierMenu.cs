// ConsoleUI/SupplierMenu.cs
using InventoryManagementSystem.DataAccess;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.ConsoleUI
{
    public class SupplierMenu
    {
        private readonly SupplierRepository _repo = new();

        public void Show()
        {
            while (true)
            {
                ConsoleHelper.PrintHeader("Supplier Management");
                Console.WriteLine("  1. View All Suppliers");
                Console.WriteLine("  2. Add New Supplier");
                Console.WriteLine("  3. Edit Supplier");
                Console.WriteLine("  0. Back");

                string choice = ConsoleHelper.AskInput("Select");
                switch (choice)
                {
                    case "1": ViewAll(); break;
                    case "2": AddSupplier(); break;
                    case "3": EditSupplier(); break;
                    case "0": return;
                    default: ConsoleHelper.PrintError("Invalid option."); break;
                }
                ConsoleHelper.PressEnterToContinue();
            }
        }

        private void ViewAll()
        {
            ConsoleHelper.PrintHeader("All Suppliers");
            var list = _repo.GetAll();
            var rows = list.Select(s => new[]
            {
                s.SupplierId, s.SupplierName,
                s.Email ?? "-", s.Phone ?? "-", s.Website ?? "-"
            }).ToList();

            ConsoleHelper.PrintTable(
                new[] { "ID", "Supplier Name", "Email", "Phone", "Website" },
                new[] { 6, 22, 24, 14, 22 },
                rows
            );
        }

        private void AddSupplier()
        {
            ConsoleHelper.PrintHeader("Add New Supplier");
            var s = new Supplier
            {
                SupplierId   = ConsoleHelper.AskRequired("Supplier ID (e.g. S6)"),
                SupplierName = ConsoleHelper.AskRequired("Supplier Name"),
                Email        = ConsoleHelper.AskInput("Email"),
                Phone        = ConsoleHelper.AskInput("Phone"),
                Website      = ConsoleHelper.AskInput("Website")
            };

            bool ok = _repo.Insert(s);
            if (ok) ConsoleHelper.PrintSuccess($"Supplier '{s.SupplierName}' added.");
            else    ConsoleHelper.PrintError("Failed to add supplier.");
        }

        private void EditSupplier()
        {
            string id = ConsoleHelper.AskRequired("Enter Supplier ID to edit");
            var s = _repo.GetById(id);
            if (s == null) { ConsoleHelper.PrintError("Supplier not found."); return; }

            Console.WriteLine($"\n  Editing: {s.SupplierName} (press Enter to keep current)\n");
            string n = ConsoleHelper.AskInput($"Name [{s.SupplierName}]");
            string e = ConsoleHelper.AskInput($"Email [{s.Email ?? "none"}]");
            string p = ConsoleHelper.AskInput($"Phone [{s.Phone ?? "none"}]");
            string w = ConsoleHelper.AskInput($"Website [{s.Website ?? "none"}]");

            if (!string.IsNullOrWhiteSpace(n)) s.SupplierName = n;
            if (!string.IsNullOrWhiteSpace(e)) s.Email = e;
            if (!string.IsNullOrWhiteSpace(p)) s.Phone = p;
            if (!string.IsNullOrWhiteSpace(w)) s.Website = w;

            bool ok = _repo.Update(s);
            if (ok) ConsoleHelper.PrintSuccess("Supplier updated.");
            else    ConsoleHelper.PrintError("Update failed.");
        }
    }
}
