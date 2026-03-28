using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbuAmenPharma.Models
{
    public class SaleReturn
    {
        public long Id { get; set; }

        public DateTime ReturnDate { get; set; } = DateTime.Now;

        [Required]
        public long SaleId { get; set; }
        public Sale Sale { get; set; } = null!;

        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetTotal { get; set; }

        [StringLength(250)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true; // تعطيل بدل حذف

        public ICollection<SaleReturnLine> Lines { get; set; } = new List<SaleReturnLine>();
    }

    public class SaleReturnLine
    {
        public long Id { get; set; }

        [Required]
        public long SaleReturnId { get; set; }
        public SaleReturn SaleReturn { get; set; } = null!;

        [Required]
        public long SaleLineId { get; set; }
        public SaleLine SaleLine { get; set; } = null!;

        [Required]
        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Qty { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        public ICollection<SaleReturnAllocation> Allocations { get; set; } = new List<SaleReturnAllocation>();
    }

    public class SaleReturnAllocation
    {
        public long Id { get; set; }

        [Required]
        public long SaleReturnLineId { get; set; }
        public SaleReturnLine SaleReturnLine { get; set; } = null!;

        [Required]
        public int BatchId { get; set; }
        public ItemBatch Batch { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Qty { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
