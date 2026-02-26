// Models/StockLevel.cs
namespace InventoryManagementSystem.Models
{
    public class StockLevel
    {
        public string StockLevelId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty; // for display
        public string WarehouseId { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty; // for display
        public decimal QuantityOnHand { get; set; }
        public decimal ReorderLevel { get; set; }
        public decimal SafetyStock { get; set; }
        public decimal ReservedQuantity { get; set; }
        public decimal AvailableQuantity => QuantityOnHand - ReservedQuantity;
        public bool IsLowStock => QuantityOnHand <= ReorderLevel;
    }
}
