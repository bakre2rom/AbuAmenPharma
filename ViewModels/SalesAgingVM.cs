namespace AbuAmenPharma.ViewModels
{
    public class SalesAgingVM
    {
        public long SaleId { get; set; }
        public string CustomerName { get; set; } = "";
        public DateTime SaleDate { get; set; }
        public decimal NetTotal { get; set; }
        public decimal Paid { get; set; }
        public decimal Remaining { get; set; }
        public int Days { get; set; }

        public decimal Bucket_0_30 { get; set; }
        public decimal Bucket_31_60 { get; set; }
        public decimal Bucket_61_90 { get; set; }
        public decimal Bucket_90Plus { get; set; }
    }
}
