using AbuAmenPharma.Models;

namespace AbuAmenPharma.ViewModels
{
    public class SaleCreateVM
    {
        public DateTime SaleDate { get; set; } = DateTime.Now;

        public int CustomerId { get; set; }


        public SalePaymentMode PaymentMode { get; set; } = SalePaymentMode.Cash;
        public decimal Discount { get; set; } = 0;
        public decimal PaidAmount { get; set; } = 0;
        public bool UseCustomerCredit { get; set; } = false;
        public decimal CustomerCreditUsed { get; set; } = 0;

        public string? Notes { get; set; }
        public List<SaleLineVM> Lines { get; set; } = new();
    }

    public class SaleLineVM
    {
        public int ItemId { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
