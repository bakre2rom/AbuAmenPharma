using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbuAmenPharma.Models
{
    public class CustomerReceipt
    {
        public long Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnallocatedAmount { get; set; } = 0;

        [StringLength(250)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true; // تعطيل بدل حذف
    }
}
