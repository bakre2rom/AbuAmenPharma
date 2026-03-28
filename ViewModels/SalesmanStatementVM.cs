namespace AbuAmenPharma.ViewModels
{
    public class SalesmanStatementVM
    {
        public int SalesmanId { get; set; }
        public string SalesmanName { get; set; } = "";

        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public decimal TotalNetSales { get; set; }     // إجمالي صافي المبيعات
        public decimal TotalCash { get; set; }         // إجمالي نقدي
        public decimal TotalCredit { get; set; }       // إجمالي آجل
        public decimal TotalPaid { get; set; }       // إجمالي آجل
        public decimal TotalRemaining { get; set; }       // إجمالي آجل
        public int SalesCount { get; set; }

        public List<SalesmanStatementLineVM> Lines { get; set; } = new();
    }

    public class SalesmanStatementLineVM
    {
        public DateTime Date { get; set; }
        public int SaleId { get; set; }
        public string CustomerName { get; set; } = "";
        public string PaymentMode { get; set; } = "";   // Cash/Credit
        public decimal NetTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string? Notes { get; set; }
    }
}
