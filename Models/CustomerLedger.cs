using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbuAmenPharma.Models
{
    public enum CustomerLedgerType { Sale = 1, Receipt = 2, SaleReturn = 3 }

    public class CustomerLedger
    {
        public long Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public CustomerLedgerType Type { get; set; }
        public long RefId { get; set; } // رقم الفاتورة/السند

        [Column(TypeName = "decimal(18,2)")]
        public decimal Debit { get; set; } = 0;   // على العميل

        [Column(TypeName = "decimal(18,2)")]
        public decimal Credit { get; set; } = 0;  // للعميل

        [StringLength(250)]
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
