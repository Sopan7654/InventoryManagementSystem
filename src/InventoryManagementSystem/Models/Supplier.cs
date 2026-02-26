// Models/Supplier.cs
namespace InventoryManagementSystem.Models
{
    public class Supplier
    {
        public string SupplierId { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
    }
}
