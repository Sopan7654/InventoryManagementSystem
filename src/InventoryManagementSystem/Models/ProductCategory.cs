// Models/ProductCategory.cs
namespace InventoryManagementSystem.Models
{
    public class ProductCategory
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; } // for display
    }
}
