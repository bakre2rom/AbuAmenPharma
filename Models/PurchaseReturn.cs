using AbuAmenPharma.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class PurchaseReturn
{
    public long Id { get; set; }

    [Required]
    public DateTime ReturnDate { get; set; } = DateTime.Now;

    [Required]
    public long PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }

    [Required]
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [StringLength(250)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<PurchaseReturnLine> Lines { get; set; } = new List<PurchaseReturnLine>();
}

public class PurchaseReturnLine
{
    public long Id { get; set; }

    [Required]
    public long PurchaseReturnId { get; set; }
    public PurchaseReturn? PurchaseReturn { get; set; }

    [Required]
    public long PurchaseLineId { get; set; }
    public PurchaseLine? PurchaseLine { get; set; }

    [Required]
    public int ItemId { get; set; }
    public Item? Item { get; set; }

    [Required]
    public int BatchId { get; set; }
    public ItemBatch? Batch { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Qty { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }
}