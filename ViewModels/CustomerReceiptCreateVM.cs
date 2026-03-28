namespace AbuAmenPharma.ViewModels
{
    public class CustomerReceiptCreateVM
    {
        public DateTime Date { get; set; } = DateTime.Now;
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }

        public bool AutoAllocate { get; set; } = true; // توزيع تلقائي
        public long? SaleId { get; set; } // لو المستخدم اختار فاتورة
    }
}
