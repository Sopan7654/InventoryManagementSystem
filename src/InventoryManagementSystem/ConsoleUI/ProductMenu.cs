// ConsoleUI/ProductMenu.cs
using InventoryManagementSystem.DataAccess;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.ConsoleUI
{
    public class ProductMenu
    {
        private readonly ProductRepository _repo = new();
        private readonly CategoryRepository _catRepo = new();

        public void Show()
        {
            while (true)
            {
                ConsoleHelper.PrintHeader("Product Management");
                Console.WriteLine("  1. View All Products");
                Console.WriteLine("  2. Search by SKU");
                Console.WriteLine("  3. Add New Product");
                Console.WriteLine("  4. Edit Product");
                Console.WriteLine("  5. Toggle Active/Inactive");
                Console.WriteLine("  0. Back");

                string choice = ConsoleHelper.AskInput("Select");
                switch (choice)
                {
                    case "1": ViewAll(); break;
                    case "2": SearchBySKU(); break;
                    case "3": AddProduct(); break;
                    case "4": EditProduct(); break;
                    case "5": ToggleStatus(); break;
                    case "0": return;
                    default: ConsoleHelper.PrintError("Invalid option."); break;
                }
                ConsoleHelper.PressEnterToContinue();
            }
        }

        private void ViewAll()
        {
            ConsoleHelper.PrintHeader("All Products");
            var products = _repo.GetAll();
            var rows = products.Select(p => new[]
            {
                p.ProductId, p.SKU, p.ProductName, p.CategoryName ?? "-",
                p.UnitOfMeasure, p.Cost.ToString("N2"), p.ListPrice.ToString("N2"),
                p.IsActive ? "Active" : "Inactive"
            }).ToList();

            ConsoleHelper.PrintTable(
                new[] { "ID", "SKU", "Product Name", "Category", "UOM", "Cost", "Price", "Status" },
                new[] { 6, 10, 22, 16, 6, 10, 10, 9 },
                rows
            );
        }

        private void SearchBySKU()
        {
            string sku = ConsoleHelper.AskRequired("Enter SKU");
            var p = _repo.GetBySKU(sku);
            if (p == null) { ConsoleHelper.PrintError("Product not found."); return; }

            ConsoleHelper.PrintSectionTitle("Product Details");
            Console.WriteLine($"  ID          : {p.ProductId}");
            Console.WriteLine($"  SKU         : {p.SKU}");
            Console.WriteLine($"  Name        : {p.ProductName}");
            Console.WriteLine($"  Description : {p.Description ?? "-"}");
            Console.WriteLine($"  Category    : {p.CategoryName ?? "-"}");
            Console.WriteLine($"  UOM         : {p.UnitOfMeasure}");
            Console.WriteLine($"  Cost        : {p.Cost:N2}");
            Console.WriteLine($"  List Price  : {p.ListPrice:N2}");
            Console.WriteLine($"  Status      : {(p.IsActive ? "Active" : "Inactive")}");
        }

        private void AddProduct()
        {
            ConsoleHelper.PrintHeader("Add New Product");
            ShowCategories();

            string id   = ConsoleHelper.AskRequired("Product ID (e.g. P13)");
            string sku  = ConsoleHelper.AskRequired("SKU");
            if (_repo.SKUExists(sku)) { ConsoleHelper.PrintError("SKU already exists."); return; }

            var p = new Product
            {
                ProductId     = id,
                SKU           = sku,
                ProductName   = ConsoleHelper.AskRequired("Product Name"),
                Description   = ConsoleHelper.AskInput("Description (optional)"),
                CategoryId    = ConsoleHelper.AskInput("Category ID (optional)"),
                UnitOfMeasure = ConsoleHelper.AskInput("Unit of Measure [PCS]").DefaultIfEmpty("PCS"),
                Cost          = ConsoleHelper.AskDecimal("Cost Price"),
                ListPrice     = ConsoleHelper.AskDecimal("List/Selling Price"),
                IsActive      = true
            };

            bool ok = _repo.Insert(p);
            if (ok) ConsoleHelper.PrintSuccess($"Product '{p.ProductName}' added successfully!");
            else    ConsoleHelper.PrintError("Failed to add product.");
        }

        private void EditProduct()
        {
            ConsoleHelper.PrintHeader("Edit Product");
            string id = ConsoleHelper.AskRequired("Enter Product ID to edit");
            var p = _repo.GetById(id);
            if (p == null) { ConsoleHelper.PrintError("Product not found."); return; }

            Console.WriteLine($"\n  Editing: {p.ProductName} (press Enter to keep current value)\n");

            string newName = ConsoleHelper.AskInput($"Name [{p.ProductName}]");
            string newDesc = ConsoleHelper.AskInput($"Description [{p.Description ?? "none"}]");
            string newUom  = ConsoleHelper.AskInput($"UOM [{p.UnitOfMeasure}]");
            string newCost = ConsoleHelper.AskInput($"Cost [{p.Cost:N2}]");
            string newPrice= ConsoleHelper.AskInput($"List Price [{p.ListPrice:N2}]");

            if (!string.IsNullOrWhiteSpace(newName))  p.ProductName   = newName;
            if (!string.IsNullOrWhiteSpace(newDesc))  p.Description   = newDesc;
            if (!string.IsNullOrWhiteSpace(newUom))   p.UnitOfMeasure = newUom;
            if (decimal.TryParse(newCost,  out decimal c)) p.Cost      = c;
            if (decimal.TryParse(newPrice, out decimal pr)) p.ListPrice = pr;

            bool ok = _repo.Update(p);
            if (ok) ConsoleHelper.PrintSuccess("Product updated.");
            else    ConsoleHelper.PrintError("Update failed.");
        }

        private void ToggleStatus()
        {
            string id = ConsoleHelper.AskRequired("Enter Product ID");
            var p = _repo.GetById(id);
            if (p == null) { ConsoleHelper.PrintError("Not found."); return; }
            p.IsActive = !p.IsActive;
            _repo.Update(p);
            ConsoleHelper.PrintSuccess($"Product is now: {(p.IsActive ? "Active" : "Inactive")}");
        }

        private void ShowCategories()
        {
            var cats = _catRepo.GetAll();
            ConsoleHelper.PrintSectionTitle("Available Categories");
            foreach (var c in cats)
                Console.WriteLine($"  {c.CategoryId,-6} {c.CategoryName} {(c.ParentCategoryId != null ? $"[under {c.ParentCategoryName}]" : "")}");
        }
    }

    public static class StringExtensions
    {
        public static string DefaultIfEmpty(this string s, string def) => string.IsNullOrWhiteSpace(s) ? def : s;
    }
}
