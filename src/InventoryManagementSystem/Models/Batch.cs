// Models/Batch.cs
namespace InventoryManagementSystem.Models
{
    public class Batch
    {
        public string BatchId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty; // for display
        public string WarehouseId { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty; // for display
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime? ManufacturingDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }

        public string ExpiryStatus
        {
            get
            {
                if (ExpiryDate == null) return "No Expiry";
                var days = (ExpiryDate.Value - DateTime.Today).Days;
                if (days < 0) return "EXPIRED";
                if (days <= 30) return "EXPIRING SOON";
                return "OK";
            }
        }
    }
}
