// Models/PurchaseOrder.cs
namespace InventoryManagementSystem.Models
{
    public class PurchaseOrder
    {
        public string PurchaseOrderId { get; set; } = string.Empty;
        public string SupplierId { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty; // for display
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = "PENDING";
    }

    public class PurchaseOrderItem
    {
        public string POItemId { get; set; } = string.Empty;
        public string PurchaseOrderId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty; // for display
        public decimal QuantityOrdered { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => QuantityOrdered * UnitPrice;
    }
}
