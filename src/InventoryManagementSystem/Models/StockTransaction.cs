// Models/StockTransaction.cs
namespace InventoryManagementSystem.Models
{
    public class StockTransaction
    {
        public string TransactionId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty; // for display
        public string WarehouseId { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty; // for display
        public string TransactionType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Reference { get; set; }
    }

    public static class TransactionTypes
    {
        public const string Purchase     = "PURCHASE";
        public const string Sale         = "SALE";
        public const string Adjustment   = "ADJUSTMENT";
        public const string TransferIn   = "TRANSFER_IN";
        public const string TransferOut  = "TRANSFER_OUT";
        public const string Return       = "RETURN";
        public const string Hold         = "HOLD";
        public const string HoldRelease  = "HOLD_RELEASE";
        public const string WriteOff     = "WRITE_OFF";
    }
}
