using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbuAmenPharma.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string NameAr { get; set; } = string.Empty;
        public string? GenericName { get; set; }

        [StringLength(50)]
        public string? BarCode { get; set; }

        [Required]
        public int ManufacturerId { get; set; }
        public Manufacturer? Manufacturer { get; set; }

        [Required]
        public int UnitId { get; set; }
        public Unit? Unit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DefaultPurchasePrice { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DefaultSellPrice { get; set; } = 0;

        public int ReorderLevel { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public ICollection<ItemBatch> Batches { get; set; } = new List<ItemBatch>();
    }

    public class ItemBatch
    {
        public int Id { get; set; }

        [Required]
        public int ItemId { get; set; }
        public Item? Item { get; set; }

        [Required, StringLength(50)]
        public string BatchNo { get; set; } = string.Empty;

        [Required]
        public DateOnly ExpiryDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellPrice { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    }

    public enum StockRefType
    {
        Purchase = 1,
        PurchaseReturn = 2,
        Sale = 3,
        SaleReturn = 4,
        Adjust = 5,
        SaleReturnDisable = 6,
    }

    public class StockMovement
    {
        public long Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public int ItemId { get; set; }
        public Item? Item { get; set; }

        [Required]
        public int BatchId { get; set; }
        public ItemBatch? Batch { get; set; }

        public decimal QtyIn { get; set; } = 0;
        public decimal QtyOut { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; } = 0;

        public StockRefType RefType { get; set; }
        public long RefId { get; set; } // رقم الفاتورة/السند
        [StringLength(250)]
        public string? Notes { get; set; }
    }
}
