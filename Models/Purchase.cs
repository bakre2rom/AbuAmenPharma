using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbuAmenPharma.Models
{
    public class Purchase
    {
        public long Id { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Required]
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        [StringLength(50)]
        public string InvoiceNo { get; set; } = "";

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetTotal { get; set; }

        public bool IsPosted { get; set; } = false; // بعد الترحيل للمخزون

        [StringLength(250)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<PurchaseLine> Lines { get; set; } = new List<PurchaseLine>();
    }

    public class PurchaseLine
    {
        public long Id { get; set; }

        [Required]
        public long PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }

        [Required]
        public int ItemId { get; set; }
        public Item? Item { get; set; }

        public int BatchId { get; set; }
        public ItemBatch Batch { get; set; } = null!;

        [Required]
        public DateOnly ExpiryDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Qty { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }
    }

    public class Supplier
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Phone { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
