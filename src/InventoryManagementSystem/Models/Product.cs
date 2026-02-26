// Models/Product.cs
namespace InventoryManagementSystem.Models
{
    public class Product
    {
        public string ProductId { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CategoryId { get; set; }
        public string? CategoryName { get; set; } // for display
        public string UnitOfMeasure { get; set; } = "PCS";
        public decimal Cost { get; set; }
        public decimal ListPrice { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
