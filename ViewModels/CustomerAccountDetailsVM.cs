using AbuAmenPharma.Models;

namespace AbuAmenPharma.ViewModels
{
    public class CustomerAccountDetailsVM
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public string? Phone { get; set; }
        public string? SalesmanName { get; set; }

        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal CurrentBalance => TotalDebit - TotalCredit;

        public List<CustomerLedgerLineVM> Lines { get; set; } = new();
    }

    public class CustomerLedgerLineVM
    {
        public long LedgerId { get; set; }
        public DateTime Date { get; set; }
        public CustomerLedgerType Type { get; set; }
        public string TypeName { get; set; } = "";

        public long RefId { get; set; }
        public string ReferenceLabel { get; set; } = "";
        public string ReferenceController { get; set; } = "";
        public string ReferenceAction { get; set; } = "Details";

        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal RunningBalance { get; set; }
        public string? Notes { get; set; }
    }
}
