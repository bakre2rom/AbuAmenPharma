namespace AbuAmenPharma.ViewModels
{
    public class ItemBatchListVM
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ItemNameAr { get; set; } = "";
        public string BatchNo { get; set; } = "";
        public DateOnly ExpiryDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellPrice { get; set; }
        public decimal Balance { get; set; } // QtyIn - QtyOut
    }
}
