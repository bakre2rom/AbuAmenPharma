using System.ComponentModel.DataAnnotations.Schema;

namespace AbuAmenPharma.Models
{
    public class CustomerReceiptAllocation
    {
        public long Id { get; set; }

        public long ReceiptId { get; set; }
        public CustomerReceipt Receipt { get; set; } = null!;

        public long SaleId { get; set; }
        public Sale Sale { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
