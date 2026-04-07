namespace AbuAmenPharma.ViewModels
{
    public class PricingListVM
    {
        public string ItemName { get; set; } = "";
        public string ScientificName { get; set; } = "";
        public string BatchNo { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string Unit { get; set; } = "";
        public DateOnly ExpiryDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal Balance { get; set; }
    }
}
