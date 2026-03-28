using AbuAmenPharma.Models;
using System.ComponentModel.DataAnnotations;

namespace AbuAmenPharma.ViewModels
{
    public class PurchaseCreateVM
    {
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Required]
        public int SupplierId { get; set; }

        public string? InvoiceNo { get; set; }
        public decimal Discount { get; set; } = 0;
        public string? Notes { get; set; }

        public List<PurchaseLineVM> Lines { get; set; } = new();
    }

    public class PurchaseLineVM
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; } // للعرض فقط

        public DateOnly ExpiryDate { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitCost { get; set; }
    }
}
