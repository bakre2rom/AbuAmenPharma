namespace AbuAmenPharma.ViewModels
{
    public class CustomerBalanceVM
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance => Debit - Credit; // الرصيد الحالي
    }

    public class CustomerStatementVM
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public decimal OpeningBalance { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal ClosingBalance { get; set; }

        public List<CustomerStatementLineVM> Lines { get; set; } = new();
    }

    public class CustomerStatementLineVM
    {
        public DateTime Date { get; set; }
        public string TypeName { get; set; } = "";   // فاتورة بيع / سند قبض ...
        public long RefId { get; set; }
        public string? Notes { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal RunningBalance { get; set; }
    }
}
