namespace AbuAmenPharma.ViewModels
{
    public class InventoryStocktakeVM
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = "";

        public int BatchId { get; set; }
        public string BatchNo { get; set; } = "";

        public DateOnly ExpiryDate { get; set; }
        public decimal Balance { get; set; }

        public int DaysToExpiry { get; set; }
        public string Status { get; set; } = ""; // منتهي/قريب/سليم
    }
}
