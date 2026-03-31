using System.ComponentModel.DataAnnotations;

namespace AbuAmenPharma.ViewModels
{
    public class InventoryStockVM
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int BatchId { get; set; }
        public string? BatchNo { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal RemainingQty { get; set; }
        public decimal TotalValue => RemainingQty * PurchasePrice;
        public bool IsValuationReport { get; set; }
    }
}
