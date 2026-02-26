// Models/Warehouse.cs
namespace InventoryManagementSystem.Models
{
    public class Warehouse
    {
        public string WarehouseId { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public decimal? Capacity { get; set; }
    }
}
