namespace AbuAmenPharma.ViewModels
{
    public class BatchStockVM
    {
        public int BatchId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public string BatchNo { get; set; } = "";
        public DateOnly? ExpiryDate { get; set; }
        public decimal Balance { get; set; }
        public int? DaysToExpiry { get; set; } // null لو ExpiryDate=null
    }
}
