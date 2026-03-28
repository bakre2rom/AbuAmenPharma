using System.ComponentModel.DataAnnotations;

namespace AbuAmenPharma.ViewModels
{
    public class PurchaseReturnCreateVM
    {
        [Required]
        public long PurchaseId { get; set; }

        public DateTime ReturnDate { get; set; } = DateTime.Now;
        public string? Notes { get; set; }

        public string SupplierName { get; set; } = "";
        public int SupplierId { get; set; }

        public List<PurchaseReturnLineVM> Lines { get; set; } = new();
    }

    public class PurchaseReturnLineVM
    {
        public long PurchaseLineId { get; set; }
        public int ItemId { get; set; }
        public int BatchId { get; set; }

        public string ItemName { get; set; } = "";
        public string BatchNo { get; set; } = "";
        public DateOnly? ExpiryDate { get; set; }

        public decimal PurchasedQty { get; set; }
        public decimal AlreadyReturnedQty { get; set; }  // للتأكد
        public decimal AvailableToReturn => PurchasedQty - AlreadyReturnedQty;

        public decimal UnitCost { get; set; }
        public decimal ReturnQty { get; set; } // يدخلها المستخدم
    }
}
