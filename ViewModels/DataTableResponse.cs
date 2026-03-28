namespace AbuAmenPharma.ViewModels
{
    public class DataTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<SaleRow>? data { get; set; }
    }

    public class SaleRow
    {
        public int id { get; set; }
        public string saleDate { get; set; }
        public string customerName { get; set; }
        public string salesmanName { get; set; }
        public string paymentMode { get; set; }
        public decimal netTotal { get; set; }
        public decimal paidAmount { get; set; }
        public decimal remainingAmount { get; set; }
    }

}
