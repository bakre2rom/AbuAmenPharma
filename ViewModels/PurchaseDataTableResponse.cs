namespace AbuAmenPharma.ViewModels
{
    public class PurchaseDataTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<PurchaseRow>? data { get; set; }
    }

    public class PurchaseRow
    {
        public int id { get; set; }
        public string purchaseDate { get; set; }
        public string? supplierName { get; set; }
        public decimal netTotal { get; set; }
        public bool isPosted { get; set; }
    }
}
