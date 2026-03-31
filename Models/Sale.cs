using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbuAmenPharma.Models
{
    public enum SalePaymentMode { Cash = 1, Credit = 2 }

    public class Sale
    {
        public long Id { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Required]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int? SalesmanId { get; set; }
        public Salesman? Salesman { get; set; }

        public SalePaymentMode PaymentMode { get; set; } = SalePaymentMode.Cash;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetTotal { get; set; }

        // نقدي/تحصيل داخل الفاتورة (ممكن 0 في الآجل)
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingAmount { get; set; } = 0;

        public bool IsPosted { get; set; } = false;

        [StringLength(250)]
        public string? Notes { get; set; }

        public ICollection<SaleLine> Lines { get; set; } = new List<SaleLine>();
    }

    public class SaleLine
    {
        public long Id { get; set; }

        [Required]
        public long SaleId { get; set; }
        public Sale? Sale { get; set; }

        [Required]
        public int ItemId { get; set; }
        public Item? Item { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Qty { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // من Item.DefaultSellPrice

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        public ICollection<SaleAllocation> Allocations { get; set; } = new List<SaleAllocation>();
    }

    public class SaleAllocation
    {
        public long Id { get; set; }

        [Required]
        public long SaleLineId { get; set; }
        public SaleLine? SaleLine { get; set; }

        [Required]
        public int BatchId { get; set; }
        public ItemBatch? Batch { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Qty { get; set; }
    }
}
