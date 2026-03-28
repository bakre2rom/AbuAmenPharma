namespace AbuAmenPharma.ViewModels
{
    public class SaleReturnCreateVM
    {
        public long SaleId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";

        public DateTime ReturnDate { get; set; } = DateTime.Now;
        public decimal Discount { get; set; } = 0;
        public string? Notes { get; set; }

        public List<SaleReturnLineVM> Lines { get; set; } = new();
    }

    public class SaleReturnLineVM
    {
        public long SaleLineId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = "";

        public decimal SoldQty { get; set; }
        public decimal AlreadyReturnedQty { get; set; }
        public decimal AvailableToReturn => SoldQty - AlreadyReturnedQty;

        public decimal UnitPrice { get; set; }
        public decimal ReturnQty { get; set; } // المستخدم
    }
}
